using Newtonsoft.Json.Linq;
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
	public sealed class HTMLLogin : HTMLEndpoint
	{
		private readonly string Page = @"<!DOCTYPE HTML>
<style>
body {
	text-align: center;
	font-family: Verdana, sans-serif;
}
form input {
	border-radius: 5pt;
	padding: 5pt;
	margin: 10pt;
}
</style>
<html>
	<body>
		<form method='post'>
			<input type='text' name='username' placeholder='Username'><br>
			<input type='password' name='password' placeholder='Password'><br>
			<input type='submit' value='Log In'>
			<input hidden name='redirect' value='@()'>
		</form>
	</body>
</html>
";

		public HTMLLogin(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

		public override void GET(Dictionary<string, string> parameters)
		{
			Server.SendText(Response, Page.Replace("#REDIRECT#", parameters.ContainsKey("redirect") ? parameters["redirect"] : ""));
		}

		public override void POST(Dictionary<string, string> parameters)
		{
			Program.Log.Debug("Received post request.");

			// Try to get the values or send an error
			if (!parameters.TryGetValue("username", out string username)) { Server.SendError(Response, HttpStatusCode.BadRequest); return; }
			if (!parameters.TryGetValue("password", out string password)) { Server.SendError(Response, HttpStatusCode.BadRequest); return; }

			// Send 401 unauthorized if the login info is incorrect
			if (!Login.VerifyLogin(username, password)) { Server.SendError(Response, HttpStatusCode.Unauthorized); return; }

			Server.AddCookie(Response, "username", username);
			Server.AddCookie(Response, "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			// Imagine a convenient method that gets the permissions from the database
			string permissions;
			if (username == "ts") permissions = "Moderator";
			else if (username == "mg") permissions = "Admin";
			else permissions = "User";
			Server.AddCookie(Response, "permission", permissions);

			// Try to get redirect url, or use default homepage
			parameters.TryGetValue("redirect", out string redirectUrl);
			if (redirectUrl == null || redirectUrl.Length == 0)
				redirectUrl = GetUrl<ImageHost>() + "?all&recurse&image=wallpaper&limit=25";
			
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
		public Login(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

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
				Server.AddCookie(Response, "token", "placeholder"); // TODO remove placeholder
				Server.AddCookie(Response, "permission", "User");
				Server.SendError(Response, HttpStatusCode.NoContent);
				return;
			}
			if (username == "ts" && password == "ts")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", "placeholder"); // TODO remove placeholder
				Server.AddCookie(Response, "permission", "Moderator");
				Server.SendError(Response, HttpStatusCode.NoContent);
				return;
			}
			if (username == "mg" && password == "mg")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", "placeholder"); // TODO remove placeholder
				Server.AddCookie(Response, "permission", "Admin");
				Server.SendError(Response, HttpStatusCode.NoContent);
				return;
			}
			Server.SendError(Response, HttpStatusCode.Unauthorized);
		}

		public static bool VerifyLogin(string username, string password)
		{
			if (username == "vp" && password == "vp") return true;
			if (username == "ts" && password == "ts") return true;
			if (username == "mg" && password == "mg") return true;
			return false;
		}
	}

	[EndpointUrl("/logout")]
	public sealed class HTMLLogout : HTMLEndpoint
	{
		public HTMLLogout(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

		public override void GET(Dictionary<string, string> parameters)
		{
			new Logout(Request, Response);
		}
	}

	[EndpointUrl("/logout")]
	public sealed class Logout : JsonEndpoint
	{
		public Logout(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

		public override void GET(JObject json, Dictionary<string, string> parameters)
		{
			Server.AddCookie(Response, "username", "deleted; expires=Thu, 01 Jan 1970 00:00:00 GMT");
			Server.AddCookie(Response, "token", "deleted; expires=Thu, 01 Jan 1970 00:00:00 GMT");
			Server.AddCookie(Response, "permission", "deleted; expires=Thu, 01 Jan 1970 00:00:00 GMT");
			Response.Redirect("/");
			Server.SendError(Response, HttpStatusCode.Redirect);
		}
	}
}
