using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using API.Database;

namespace API.HTTP.Endpoints
{
	[EndpointUrl("/register")]
	public sealed class JRegister : JsonEndpoint
	{
		public override void GET(JObject json, Dictionary<string, string> parameters)
			=> Server.SendError(HttpStatusCode.NotImplemented);

		/// <summary>
		/// Required json arguments for this method are:
		///		- username [string] : May not be an empty string and may not be longer than 100 chars (limit is specified in the database)
		///		- password [string] : Must be 128 chars long and must be hashed with SHA512. (See the API.Database.User.GetPasswordHash() for how this must be done)
		///		
		/// Responds with:
		///		- 201 "Created"				 : Sent when the user was successfully created.
		///		- 409 "Conflict"			 : Sent when the username is already taken.
		///		- 422 "Unprocessable Entity" : Sent when the arguments failed validation. A JSON with extra info is also sent.
		/// </summary>
		public override void POST(JObject json, Dictionary<string, string> parameters)
		{
			// Validate the presence of all required parameters
			if (!ValidateParams(json,
					("username", x => x.Value<string>().Any()), // Username may not be empty
					("password", x => x.Value<string>().Length == 128))) // Password must be 128 characters long (thus must be hashed with sha512)
			{
				// If validate returned false, a response was already sent
				return;
			}

			// Get and cast all required parameters
			var username = json["username"].Value<string>();
			var password = json["password"].Value<string>();

			// Check if the user does not already exist
			if (Database.Select<User>($"`username` = '{username}'").Any())
			{
				// Send 409 Conflict status code to indicate a duplicate
				Server.SendError(HttpStatusCode.Conflict);
				return;
			}

			// Create the new user object
			var user = new User() { Username = username, Password = password };
			// Salt and hash it's password
			user.Password = user.GetPasswordHash();

			// Upload the new user to the database
			Database.Insert(user);

			// Send 201 Created to indicate success
			Server.SendError(HttpStatusCode.Created);
		}
	}
}
