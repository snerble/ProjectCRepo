using Newtonsoft.Json.Linq;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Sends a page containing a login form
	/// </summary>
	[EndpointUrl("/login")]
	public sealed class HTMLLogin : HTMLEndpoint
	{
		public override void GET(Dictionary<string, string> parameters)
			=> Server.SendText(Response, Templates.RunTemplate(GetUrl<HTMLLogin>() + ".cshtml", Request, parameters));

		public override void POST(Dictionary<string, string> parameters)
		{
			Program.Log.Debug("Received post request.");

			// Try to get the values or send an error
			if (!parameters.TryGetValue("username", out string username)) { Server.SendError(Response, HttpStatusCode.BadRequest); return; }
			if (!parameters.TryGetValue("password", out string password)) { Server.SendError(Response, HttpStatusCode.BadRequest); return; }

			// Send 401 unauthorized if the login info is incorrect
			if (username == "vp" && password == "vp")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				Server.AddCookie(Response, "permission", "User");
			}
			else if (username == "ts" && password == "ts")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				Server.AddCookie(Response, "permission", "Moderator");
			}
			else if (username == "mg" && password == "mg")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				Server.AddCookie(Response, "permission", "Admin");
			}
			else Server.SendError(Response, HttpStatusCode.Unauthorized);

			// Try to get redirect url, or use default homepage
			parameters.TryGetValue("redirect", out string redirectUrl);
			if (redirectUrl == null || redirectUrl.Length == 0) redirectUrl = GetUrl<ImageHost>();
			else redirectUrl = Uri.UnescapeDataString(redirectUrl);
			
			Response.Redirect(redirectUrl);
			Server.SendError(Response, HttpStatusCode.Redirect);
		}
	}

	/// <summary>
	/// API endpoint that sets login cookies when you give the right login information.
	/// </summary>
	[EndpointUrl("/login")]
	public sealed class Login : JsonEndpoint
	{
		public override void GET(JObject json, Dictionary<string, string> parameters)
		{
			// Temp endpoint piping
			if (parameters.ContainsKey("username")) json["username"] = parameters["username"];
			if (parameters.ContainsKey("password")) json["password"] = parameters["password"];

			// Try to get the values, or send an error
			if (!json.TryGetValue("username", out JToken usernameToken)) { Server.SendError(Response, HttpStatusCode.BadRequest); return; }
			if (!json.TryGetValue("password", out JToken passwordToken)) { Server.SendError(Response, HttpStatusCode.BadRequest); return; }

			// Convert JTokens
			string username = usernameToken.Value<string>();
			string password = passwordToken.Value<string>();

			// Check if username and password are correct
			// TODO Use database for this
			if (username == "vp" && password == "vp")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				Server.AddCookie(Response, "permission", "User");
				Server.SendError(Response, HttpStatusCode.NoContent);
				return;
			}
			if (username == "ts" && password == "ts")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				Server.AddCookie(Response, "permission", "Moderator");
				Server.SendError(Response, HttpStatusCode.NoContent);
				return;
			}
			if (username == "mg" && password == "mg")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				Server.AddCookie(Response, "permission", "Admin");
				Server.SendError(Response, HttpStatusCode.NoContent);
				return;
			}
			Server.SendError(Response, HttpStatusCode.Unauthorized);
		}
	}

	/// <summary>
	/// Delegate HTML endpoint for the <see cref="Login"/> endpoint.
	/// </summary>
	[EndpointUrl("/logout")]
	public sealed class HTMLLogout : HTMLEndpoint
	{
		public override void GET(Dictionary<string, string> parameters)
		{
			new Logout().Invoke(Request, Response);
		}
	}

	/// <summary>
	/// API endpoint that removes the login cookies.
	/// </summary>
	[EndpointUrl("/logout")]
	public sealed class Logout : JsonEndpoint
	{
		public static string Expiration = DateTimeOffset.FromUnixTimeSeconds(0).ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'");

		public override void GET(JObject json, Dictionary<string, string> parameters)
		{
			Server.AddCookie(Response, "username", "deleted; expires=" + Expiration);
			Server.AddCookie(Response, "token", "deleted; expires=" + Expiration);
			Server.AddCookie(Response, "permission", "deleted; expires=" + Expiration);
			Response.Redirect("/");
			Server.SendError(Response, HttpStatusCode.Redirect);
		}
	}
}
