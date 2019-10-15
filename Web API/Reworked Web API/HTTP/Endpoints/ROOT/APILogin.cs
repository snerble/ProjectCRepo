using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Workaround for testing. Allows a browser to access the api function without supplying a JSON payload.
	/// </summary>
	[EndpointUrl("/login.json")]
	public sealed class HTMLLogin : HTMLEndpoint
	{
		public HTMLLogin(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

		public override void GET(Dictionary<string, string> parameters)
		{
			new APILogin(Request, Response);
		}
	}

	/// <summary>
	/// API endpoint that sets login cookies when you give the right login information.
	/// </summary>
	[EndpointUrl("/login.json")]
	public sealed class APILogin : JsonEndpoint
	{
		public APILogin(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

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
			}
			else if (username == "ts" && password == "ts")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", "placeholder"); // TODO remove placeholder
				Server.AddCookie(Response, "permission", "Moderator");
				Server.SendError(Response, HttpStatusCode.NoContent);
			}
			else if (username == "mg" && password == "mg")
			{
				Server.AddCookie(Response, "username", username);
				Server.AddCookie(Response, "token", "placeholder"); // TODO remove placeholder
				Server.AddCookie(Response, "permission", "Admin");
				Server.SendError(Response, HttpStatusCode.NoContent);
			}
		}
	}
}
