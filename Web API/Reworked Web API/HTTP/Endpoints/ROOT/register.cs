using API.Database;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace API.HTTP.Endpoints
{
	[EndpointUrl("/register")]
    public class Register : HTMLEndpoint
    {
        public override void GET(Dictionary<string, string> parameters)
            => Server.SendFile(Program.Config.HTMLSourceDir + "/register.html");

        public override void POST(Dictionary<string, string> parameters)
        {
			// Validate the presence of all required parameters
			if (!ValidateParams(parameters,
					("username", x => x.Any()), // Username may not be empty
					("password", x => x.Length == 128))) // Password must be 128 characters long (thus must be hashed with sha512)
			{
				// Send a 400 Bad Request if any required parameters are missing
				Server.SendError(HttpStatusCode.BadRequest);
				return;
			}

			// Get all required parameters
			var username = parameters["username"];
			var password = parameters["password"];

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

			// Redirect the client to the login
			Response.Redirect(GetUrl<Login>());
			Server.SendError(HttpStatusCode.Redirect);
        }
    }
}
