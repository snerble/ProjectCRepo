using API.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using API.Database;
using System.Linq;

namespace API.HTTP.Endpoints.ROOT
{
	[RequiresLogin]
	[EndpointUrl("/taskenroll")]
	public sealed class JTaskEnroll : JsonEndpoint
	{
		/// <summary>
		/// Returns all users who are enrolled in the specified task and their enrollment period.
		/// 
		/// Required JSON params:
		///		- task [int] : The id of the task whose enrolled users to return. May not be less than 0.
		///	
		/// Responds with:
		///		- 200 "OK"					 : Sent along with a JSON containing an array of the names of the users who are enrolled.
		///		- 204 "No Content"			 : Sent if no users are enrolled.
		///		- 422 "Unprocessable Entity" : Sent when the arguments failed validation. A JSON with extra info is also sent.
		/// </summary>
		public override void GET(JObject json, Dictionary<string, string> parameters)
		{
			// Validate required parameters
			if (!ValidateParams(json,
				("task", x => x.Value<int>() >= 0))) // Must be int and not less than 0
				return;

			// Get params
			var task = json["task"].Value<int>();

			// Get all task-user links to the specified task
			var taskUser_links = Database.Select<TaskUser_Link>($"`task` = {task}").ToList();

			// Send 204 "No Content" if no users are enrolled
			if (!taskUser_links.Any())
			{
				Server.SendError(HttpStatusCode.NoContent);
				return;
			}

			// Get all enrolled users
			var users = Database.Select<User>(string.Join(" OR ", taskUser_links.Select(x => $"`id` = {x.User}")));

			var outJson = new JObject();
			var results = new JArray();
			outJson.Add("results", results);

			// Fill the results array with data
			foreach (var user in users)
			{
				// Get the corresponding task-user link for 'user'
				var taskUser_link = taskUser_links.First(x => x.User == user.Id.Value);

				var entry = new JObject() {
					{"Username", user.Username },
					{"Start", taskUser_link.Start }
				};
				// Add end value if it isn't null
				if (taskUser_link.End.HasValue)
					entry.Add("End", taskUser_link.End.Value);

				results.Add(entry);
			}

			// Send the outJson with all the results
			Server.SendJSON(outJson);
		}

		/// <summary>
		/// Enrolls the current user to the specified task.
		/// 
		/// Required JSON parameters:
		///		- task [int] : The id of the task to enroll in. Must be valid.
		///	
		/// Responds with:
		///		- 204 "No Content"			 : Sent to indicate success.
		///		- 409 "Conflict"			 : Sent when the user has already enrolled for the task.
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
				("task", x => Database.Select<Task>($"`id` = {x.Value<int>()}").Any()))) // Must be int and a valid task id
				return;

			// Get parameters
			var task = json["task"].Value<int>();

			// Check if the user isn't already enrolled
			if (Database.Select<TaskUser_Link>($"`task` = {task} AND `user` = {CurrentUser.Id.Value}").Any())
			{
				// Send 409 "Conflict" to indicate duplicate enrollment
				Server.SendError(HttpStatusCode.Conflict);
				return;
			}

			// Create task-user link object
			var taskUser_link = new TaskUser_Link()
			{
				User = CurrentUser.Id.Value,
				Task = task
			};

			// Insert the task-user link
			Database.Insert(taskUser_link);

			// Send 204 "No Content" to indicate success without response body
			Server.SendError(HttpStatusCode.NoContent);
		}

		/// <summary>
		/// Deletes the enrollment of the current user from the specified task.
		/// 
		/// Required JSON parameters:
		///		- task [int] : The id of the task to delete the enrollment from. May not be less than 0.
		///	
		/// Responds with:
		///		- 204 "No Content"			 : Sent to indicate successfull deletion.
		///		- 304 "Not Modified"		 : Sent when the user hasn't enrolled for the task and thus nothing was deleted.
		///		- 422 "Unprocessable Entity" : Sent when the arguments failed validation. A JSON with extra info is also sent.
		/// </summary>
		public override void DELETE(JObject json, Dictionary<string, string> parameters)
		{
			// Validate required parameters
			if (!ValidateParams(json,
				("task", x => x.Value<int>() >= 0))) // Must be int and not less than 0
				return;

			// Get parameters
			var task = json["task"].Value<int>();

			// Delete the task-user link for this user and the task
			var deletedRows = Database.Delete<TaskUser_Link>($"`task` = {task} AND `user` = {CurrentUser.Id.Value}");

			// If nothing was deleted, send 304 "Not Modified"
			if (deletedRows == 0)
			{
				Server.SendError(HttpStatusCode.NotModified);
				return;
			}

			// Send 204 "No Content" to indicate success without response body
			Server.SendError(HttpStatusCode.NoContent);
		}

		/// <summary>
		/// Finishes the enrollment of the current user from the specified task.
		/// 
		/// Required JSON parameters:
		///		- task [int] : The id of the task whose enrollment to must be finished. May not be less than 0.
		///	
		/// Responds with:
		///		- 204 "No Content"			 : Sent to indicate success.
		///		- 400 "Bad Request"			 : Sent when the user hasn't enrolled for the task or if the enrollment was already finished.
		///		- 422 "Unprocessable Entity" : Sent when the arguments failed validation. A JSON with extra info is also sent.
		/// </summary>
		public override void PATCH(JObject json, Dictionary<string, string> parameters)
		{
			// Validate required parameters
			if (!ValidateParams(json,
				("task", x => x.Value<int>() >= 0))) // Must be int and not less than 0
				return;

			// Get parameters
			var task = json["task"].Value<int>();

			// Get the task-user link for the current user
			var enrollment = Database.Select<TaskUser_Link>($"`task` = {task} AND `user` = {CurrentUser.Id.Value}").FirstOrDefault();

			// If the enrollment does not exist or has already ended, send a 400 "Bad Request".
			if (enrollment == null || enrollment.End.HasValue)
			{
				Server.SendError(HttpStatusCode.BadRequest);
				return;
			}

			// Update the End value to the current time
			enrollment.End = DateTimeOffset.Now.ToUnixTimeSeconds();

			// Update the task-user link object in the database
			Database.Update(enrollment);

			// Send 204 "No Content" to indicate success without response body
			Server.SendError(HttpStatusCode.NoContent);
		}
	}
}
