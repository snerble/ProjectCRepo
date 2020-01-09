using API.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using API.Database;

namespace API.HTTP.Endpoints.ROOT
{
	[RequiresLogin]
	[EndpointUrl("/groupsharing")]
	public sealed class JGroupSharing : JsonEndpoint
	{
		/// <summary>
		/// Creates and returns a share code for the specified group.
		/// 
		/// Required JSON parameters:
		///		- group [int] : The id of the group whose share code to get. May not be less than 0.
		/// 
		/// Responds with:
		///		- 200 "OK"					 : Sent along with a JSON containing an existing share code.
		///		- 201 "Created"				 : Sent along with a JSON containing the new share code.
		///		- 400 "Bad Request"			 : Sent when the group id is invalid.
		///		- 403 "Forbidden"			 : Sent when the current user lacks the privileges to create a share link.
		///		- 422 "Unprocessable Entity" : Sent when the arguments failed validation. A JSON with extra info is also sent.
		/// </summary>
		public override void GET(JObject json, Dictionary<string, string> parameters)
		{
			// Validate required params
			if (!ValidateParams(json,
				("group", x => x.Value<int>() >= 0))) // Must be int and not less than 0
				return;

			// Get the params
			var group_id = json["group"].Value<int>();

			// Get the specified group
			var group = Database.Select<Group>($"`id` = {group_id}").FirstOrDefault();

			// If the group does not exist, send a 400 "Bad Request"
			if (group == null)
			{
				Server.SendError(HttpStatusCode.BadRequest);
				return;
			}

			// If the creator is not the current user
			if (group.Creator != CurrentUser.Id.Value)
			{
				// Check if the current user has admin privileges
				var groupUser_link = Database.Select<GroupUser_Link>($"`group` = {group_id} AND `user` = {CurrentUser.Id.Value}").FirstOrDefault();

				// If the user has not joined the group or is not admin, send a 403 "Forbidden"
				if (groupUser_link == null || groupUser_link.Rank != Rank.Admin)
				{
					Server.SendError(HttpStatusCode.Forbidden);
					return;
				}
			}

			// Try to get an existing sharecode. If it exists, send a 200 "OK" with the sharecode.
			var existingShareCode = Database.Select<GroupShareData>($"`group` = {group_id}").FirstOrDefault();
			if (existingShareCode != null)
			{
				// If the sharecode has not expired, send that code
				if (existingShareCode.Created + existingShareCode.Expiration >= DateTimeOffset.Now.ToUnixTimeSeconds())
				{
					Server.SendJSON(new JObject() {
						{"code", existingShareCode.Code }
					}, HttpStatusCode.OK);
					return;
				}
				// Otherwise, delete the existing code and continue
				else
				{
					Database.Delete(existingShareCode);
					Program.Log.Fine($"Deleted expired sharecode '{existingShareCode.Code}'");
				}
			}

			// Create the sharecode by taking the first 8 chars from a GUID to base64
			var sharecode = new GroupShareData()
			{
				Group = group_id,
				Code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..8],
			};

			// Upload the sharecode
			Database.Insert(sharecode);

			// Send a 201 "Created" with the new sharecode
			Server.SendJSON(new JObject() {
				{"code", sharecode.Code }
			}, HttpStatusCode.Created);
			return;
		}

		/// <summary>
		/// Adds the current user to the group associated with the specified sharecode.
		/// 
		/// Required JSON arguments are:
		///		- code [string] : A valid sharecode. Must be 8 characters long.
		///		
		/// Responds with:
		///		- 204 "No Content"			 : Sent when the current user successfully joined the group.
		///		- 400 "Bad Request"			 : Sent when the sharecode is invalid.
		///		- 409 "Conflict"			 : Sent when the current user is already the group creator or has already joined the group.
		///		- 410 "Gone"				 : Sent when the sharecode that was specified has since expired.
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

			// Validate required params
			if (!ValidateParams(json,
				("code", x => x.Value<string>().Length == 8))) // Must be string and 8 chars long
				return;

			// Get params
			var sharecode = json["code"].Value<string>();

			// Get the sharecode
			var groupShareData = Database.Select<GroupShareData>($"`code` = '{sharecode}'").FirstOrDefault();

			// If the sharedata is null, send 400 "Bad Request"
			if (groupShareData == null)
			{
				Server.SendError(HttpStatusCode.BadRequest);
				return;
			}
			// If the sharedata has expired, delete it and send a 410 "Gone"
			else if (groupShareData.Created + groupShareData.Expiration < DateTimeOffset.Now.ToUnixTimeSeconds())
			{
				Database.Delete(groupShareData);
				Program.Log.Fine($"Deleted expired sharecode '{groupShareData.Code}'");
				Server.SendError(HttpStatusCode.Gone);
				return;
			}

			// Get the group associated with the sharecode
			var group = Database.Select<Group>($"`id` = {groupShareData.Group}").First();
			// If the current user is already the group creator, send a 409 "Conflict"
			if (group.Creator == CurrentUser.Id.Value)
			{
				Server.SendError(HttpStatusCode.Conflict);
				return;
			}
			
			// If the user has already joined the group, send a 409 "Conflict"
			var groupUser_link = Database.Select<GroupUser_Link>($"`group` = {groupShareData.Group} AND `user` = {CurrentUser.Id.Value}").FirstOrDefault();
			if (groupUser_link != null)
			{
				Server.SendError(HttpStatusCode.Conflict);
				return;
			}

			// Create the group-user link for the user and the group
			var new_groupUser_link = new GroupUser_Link()
			{
				Group = groupShareData.Group,
				User = CurrentUser.Id.Value,
			};

			// Upload the group-user link
			Database.Insert(new_groupUser_link);

			// Send 204 "No Content" to indicate success with no response body.
			Server.SendError(HttpStatusCode.NoContent);
		}
	}
}
