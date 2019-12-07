using API.Database;
using System.Collections.Generic;
using System.Net;

namespace API.HTTP.Endpoints.ROOT
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
					("username", null),
					("password", null)))
			{
				// Send a 400 Bad Request if any required parameters are missing
				Server.SendError(HttpStatusCode.BadRequest);
				return;
			}

			// Get all required parameters
			var username = parameters["username"];
			var password = parameters["password"];

			// Create the new user object
			var user = new User() { Username = username, Password = password };
			// Salt and hash it's password
			user.Password = user.GetPasswordHash();

			// Upload the new user to the database
			Program.Database.Insert(user);

			// Redirect the client to the login
			Response.Redirect(GetUrl<Login>());
			Server.SendError(HttpStatusCode.Redirect);
        }
    }
}
