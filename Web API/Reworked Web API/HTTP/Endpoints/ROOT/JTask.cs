using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using API.Database;
using System.Net;
using System.Linq;
using API.Attributes;

namespace API.HTTP.Endpoints
{
	[RequiresLogin]
	[EndpointUrl("/task")]
	public sealed class JTask : JsonEndpoint
	{
		/// <summary>
		/// Sends a list of <see cref="Task"/>s based on the specified group.
		/// 
		/// Note: This method will be invoked through the POST method using a url parameter.
		///		The mobile app automatically does this.
		/// 
		/// Required JSON arguments are:
		///		- group [int] : The id of the group whose tasks we want to get. May not be less than 0.
		///	
		/// Optional JSON arguments are:
		///		- offset [int] : The amount of tasks to skip before returning the tasklist. May not be less than 0.
		///		- limit [int]  : The maximum amount of tasks to return. May not be less than 1.
		///		
		/// Responds with:
		///		- 200 "OK"					 : Sent along with a JSON containing an array of tasks.
		///		- 409 "Conflict"			 : Sent when the username is already taken.
		///		- 422 "Unprocessable Entity" : Sent when the arguments failed validation. A JSON with extra info is also sent.
		/// </summary>
		public override void GET(JObject json, Dictionary<string, string> parameters)
		{
			// Validate required parameters
			if (!ValidateParams(json,
				("group", x => x.Value<int>() >= 0))) // Must be int and not less than 0
				return;

			// Validate optional parameters
			if (!ValidateParams(json, ValidationMode.Optional,
				("offset", x => x.Value<int>() >= 0), // Must be int and not less than 0
				("limit", x => x.Value<int>() >= 1))) // Must be int and not less than 1
				return;

			// Get parameters
			var group_id = json["group"].Value<int>();
			var offset = json["offset"].Value<int?>();
			var limit = json["limit"].Value<int?>();

			// Get tasks belonging to the specified group with a certain limit
			var results = Database.Select<Task>($"`group` = {group_id} LIMIT {offset ?? 0},{limit ?? long.MaxValue}");

			// Send 200 "OK" with the results
			Server.SendJSON(new JObject() {
				{"results", new JArray(results.Select(x => (JObject)x)) }
			});
		}

		/// <summary>
		/// Creates a new task with the specified data with the current user as the creator.
		/// 
		/// Required JSON arguments are:
		///		- group [int]    : The id of the group whose tasks we want to get. May not be less than 0 and must be valid.
		///		- title [string] : The title of the task. May not be empty.
		///	
		/// Optional JSON arguments are:
		///		- description [string] : The description of the task.
		///		- priority [sbyte]     : The priority of the task. 0 <= x <= 3 must be true. 0 by default.
		///		
		/// Responds with:
		///		- 201 "Created"				 : Sent along with a JSON containing the new task's id.
		///		- 422 "Unprocessable Entity" : Sent when the arguments failed validation. A JSON with extra info is also sent.
		/// </summary>
		public override void POST(JObject json, Dictionary<string, string> parameters)
		{
			// Get the method parameter if specified and rerout to GET because GET can't actually have a request body
			if (parameters.ContainsKey("method") && parameters["method"].ToString().ToUpper() == "GET")
			{
				GET(json, parameters);
				return;
			}

			// Validate required parameters
			if (!ValidateParams(json,
				("group", x => x.Value<int>() >= 0 && Database.Select<Group>("`id` = " + x.Value<int>()).Any()), // Must be int, not less than 0 and a valid group id
				("title", x => x.Value<string>().Any()))) // Must be string and not empty
				return;

			// Validate optional parameters
			if (!ValidateParams(json, ValidationMode.Optional,
				("description", x => x.Value<string>() != null), // Must be string
				("priority", x => x.Value<sbyte>() >= 0 && x.Value<sbyte>() <= 3))) // Must be sbyte and 0 <= x <= 3
				return;

			// Get parameters
			var group = json["group"].Value<int>();
			var title = json["title"].Value<string>();
			var description = json["description"].Value<string>();
			var priority = json.ContainsKey("priority") ? json["priority"].Value<sbyte>() : (sbyte)0;

			// Create the task
			var task = new Task()
			{
				Group = group,
				Creator = CurrentUser.Id.Value,
				Title = title,
				Description = description,
				Priority = priority
			};

			// Insert the task (and update it's id)
			Database.Insert(task);

			// Send the new id with a 201 "Created"
			Server.SendJSON(new JObject() {
				{"id", task.Id.Value }
			}, HttpStatusCode.Created);
		}
	}
}
