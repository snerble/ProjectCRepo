using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// API endpoint that sets login cookies when you give the right login information.
	/// </summary>
	[EndpointUrl("/login.json")]
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
	}
}
