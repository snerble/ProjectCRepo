using API.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace API.HTTP.Endpoints
{
	[EndpointUrl("/resource")]
	public sealed class JResource : JsonEndpoint
	{
		public override void GET(JObject json, Dictionary<string, string> parameters)
		{
			// Validate all option parameters
			if (!ValidateParams(json, ValidationMode.Options,
					("Id", x => x.Type == JTokenType.Integer),
					("Hash", x => x.Type == JTokenType.String && x.Value<string>().Length == 33)))
				return;

			// Get and cast all values (nullable because options and not all parameters may be present)
			var id = json?["Id"]?.Value<long>();
			var hash = json?["Hash"]?.Value<string>();

			// Create list of conditions (concatinated with " AND " later)
			var conditionList = new List<string>();
			if (id.HasValue) conditionList.Add($"`id` = {id}");
			if (hash != null) conditionList.Add($"`hash` = '{hash}'");

			// Run query with condition
			var results = Program.Database.Select<Resource>(string.Join(" AND ", conditionList));

			// If none were found, send a 204
			if (!results.Any())
			{
				Server.SendError(HttpStatusCode.NoContent);
				return;
			}

			// Return al results
			Server.SendJSON(new JObject()
			{
				{"results", new JArray(results.Select(x => { x.Data = null; return (JObject)x; })) }
			});
		}

		public override void POST(JObject json, Dictionary<string, string> parameters)
		{
			// Get the method parameter if specified and rerout to GET because GET can't actually have a request body
			if (parameters.ContainsKey("method") && parameters["method"].ToString().ToUpper() == "GET")
			{
				GET(json, parameters);
				return;
			}

			// Validate all required parameters
			if (!ValidateParams(json,
					("Filename", x => x.Type == JTokenType.String && x.ToString().Length < 256),
					("Data", x => x.Type == JTokenType.String)))
				return;

			// Try to convert the data parameter, or send an error message to the client.
			byte[] data;
			try
			{
				data = Convert.FromBase64String(json["Data"].ToString());
			}
			catch (FormatException)
			{
				Server.SendJSON(new JObject() { { "invalid", new JArray() { "Data" } } }, HttpStatusCode.UnprocessableEntity);
				return;
			}

			// Get parameters
			var filename = json["Filename"].Value<string>();

			// Calculate and create hex md5 hash
			using var md5 = MD5.Create();
			var hash = string.Concat(md5.ComputeHash(data).Select(x => x.ToString("x2")));

			// Look for other resources with the same hash and name
			foreach (var otherResource in Program.Database.Select<Resource>($"`filename` = '{filename}' AND `hash` = '{hash}'"))
			{
				// If another resource has exactly the same data, send 208 already reported with the conflicting Id
				if (otherResource.Data.SequenceEqual(data))
				{
					Server.SendJSON(new JObject()
					{
						{"Id", otherResource.Id }
					}, HttpStatusCode.AlreadyReported);
					return;
				}
			}

			// Start transaction and only upload when the client received the response with the new Id
			var transaction = Program.Database.Connection.BeginTransaction();
			// Create and upload the new resource
			var newId = Program.Database.Insert(new Resource() { Filename = filename, Data = data, Hash = hash });
			// Send the id back to the client
			Server.SendJSON(new JObject()
			{
				{"Id", newId }
			}, HttpStatusCode.Created);
			// Commit transaction changes
			transaction.Commit();
		}

		public override void DELETE(JObject json, Dictionary<string, string> parameters)
		{
			// TODO create HasAccess(User) function so not anyone can do this
			// Validate all required parameters
			if (!ValidateParams(json, ("Id", x => x.Type == JTokenType.Integer)))
				return;

			// Get all parameters
			var id = json["Id"].Value<long>();

			// Delete the resource from the database
			var affectedRows = Program.Database.Delete<Resource>("`id` = " + id);

			// Send OK if something was changed, or 208 if the thing was already gone/not present
			if (affectedRows == 0) Server.SendError(HttpStatusCode.AlreadyReported);
			else Server.SendError(HttpStatusCode.NoContent);
		}
	}
}
