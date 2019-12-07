using API.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Sends a page containing a login form
	/// </summary>
	[EndpointUrl("/login")]
	public sealed class Login : HTMLEndpoint
	{
		public override void GET(Dictionary<string, string> parameters)
			=> Server.SendFile(Program.Config.HTMLSourceDir + "/login.html");

		public override void POST(Dictionary<string, string> parameters)
		{
			// Validate parameters (this one only checks if username and password are specified)
			if (!ValidateParams(parameters,
					("username", null),
					("password", null)))
			{
				// Send bad request status code if the required parameters are missing
				Server.SendError(HttpStatusCode.BadRequest);
				return;
			}

			// Get required parameters
			string username = parameters["username"];
			string password = parameters["password"];

			// Create a new user object (so we can use the hashing method from the user class)
			var mockUser = new User() { Username = username, Password = password };
			// Hash it's password
			mockUser.Password = mockUser.GetPasswordHash();

			// Try to get the user from database
			var user = Program.Database.Select<User>($"`username` = '{mockUser.Username}' AND `password` = '{mockUser.Password}'").FirstOrDefault();

			// Login is successfull if the query matched something in the database
			if (user != null)
			{
				// Begin a transaction so that we won't upload the session if SendError threw an exception.
				var transaction = Program.Database.Connection.BeginTransaction();

				// If the request already had a session, update the userId
				if (CurrentSession != null)
				{
					CurrentSession.User = user.Id;
					Program.Database.Update(CurrentSession);
				}
				else
				{
					// Create a new session
					var session = Utils.CreateSession(user);
					// Set a new session cookie
					Utils.AddCookie(Response, "session", session.Id);
				}

				// Get and unescape the redirect url from the parameters (if present)
				var redirect = Request.QueryString.AllKeys.Contains("redir") ? Request.QueryString["redir"] : null;

				// Redirect to the url specified in the parameters, or the home page
				Response.Redirect(redirect ?? "/home_vp.html");
				Server.SendError(HttpStatusCode.Redirect);

				// Apply changes
				transaction.Commit();
			}
			else
			{
				// Redirect to the incorrect login html page if the login data was invalid
				Response.Redirect("login_wrong.html");
				Server.SendError(HttpStatusCode.Redirect);
			}
		}
	}
}
