using System.Linq;
using System.Collections.Generic;
using System.Text;
using API.Database;
using Newtonsoft.Json.Linq;
using API.Attributes;
using System.Net;

namespace API.HTTP.Endpoints.ROOT
{
	[RequiresLogin]
    [EndpointUrl("/group")]
    public sealed class JGroup : JsonEndpoint
    {
		/// <summary>
		/// Returns a list of groups that the current user either created or has joined.
		///	
		/// Responds with:
		///		- 200 "OK" : Sent along with a JSON containing an array of groups.
		/// </summary>
		public override void GET(JObject json, Dictionary<string, string> parameters)
        {
			var groupUser_links = Database.Select<GroupUser_Link>($"`user` = {CurrentUser.Id.Value}");

			// Get a condition string for all joined groups
			var condition = "";
			if (groupUser_links.Any())
				condition = " OR " + string.Join(" OR ", groupUser_links.Select(x => $"`id` = {x.Group}"));

			// Get all groups the current user created or has joined
            var results = Program.Database.Select<Group>("`creator` = " + CurrentUser.Id + condition);

			// Send json containing the results
            Server.SendJSON(new JObject() {
                {"results", new JArray(results.Select(x => (JObject)x)) }
            });
        }

		/// <summary>
		/// Creates a new group with the specified data with the current user as the creator.
		/// 
		/// Required JSON arguments are:
		///		- name [string] : The name of the group. May not be empty.
		///	
		/// Optional JSON arguments are:
		///		- description [string] : The description of the group.
		///		
		/// Responds with:
		///		- 201 "Created"				 : Sent along with a JSON containing the new group's id.
		///		- 422 "Unprocessable Entity" : Sent when the arguments failed validation. A JSON with extra info is also sent.
		/// </summary>
		public override void POST(JObject json, Dictionary<string, string> parameters)
		{
			// Validate required parameters
			if (!ValidateParams(json,
				("name", x => x.Value<string>().Any()))) // Must be string and not empty
				return;

			// Validate optional parameters
			if (!ValidateParams(json, ValidationMode.Optional,
				("description", x => x.Value<string>() != null))) // Must be string
				return;

			// Get parameters
			var name = json["name"].Value<string>();
			var description = json?["description"].Value<string>();

			// Create the group
			var group = new Group()
			{
				Creator = CurrentUser.Id.Value,
				Name = name,
				Description = description
			};

			// Insert the group (and update it's id)
			Database.Insert(group);

			// Send the new id with a 201 "Created"
			Server.SendJSON(new JObject() {
				{"id", group.Id.Value }
			}, HttpStatusCode.Created);
		}
	}
}
