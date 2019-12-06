using API.Database;
using Newtonsoft.Json.Linq;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
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
			// Get required parameters
			string username = parameters?["username"];
			string password = parameters?["password"];

			if (username == null || password == null)
			{
				// Send bad request status code if the required parameters are missing
				Server.SendError(HttpStatusCode.BadRequest);
				return;
			}

			// Get user from database
			var user = Program.Database.Select<User>($"`username` = '{username}' AND `password` = '{password}'").FirstOrDefault();

			// Login is successfull if the query matched something in the database
			if (user != null)
			{
				// Begin a transaction so that we won't upload the session if SendError threw an exception.
				var transaction = Program.Database.Connection.BeginTransaction();

				// Create a new session
				var session = Utils.CreateSession(user);
				// Set a new session cookie
				Utils.AddCookie(Response, "session", session.Id);

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
