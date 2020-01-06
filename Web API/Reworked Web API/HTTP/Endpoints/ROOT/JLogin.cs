using API.Database;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace API.HTTP.Endpoints
{
	[EndpointUrl("/jlogin")]
    public sealed class JLogin : JsonEndpoint
    {
        public override void GET(JObject json, Dictionary<string, string> parameters)
            => Server.SendError(HttpStatusCode.NotImplemented);

        public override void POST(JObject json, Dictionary<string, string> parameters)
        {
            if (!json.TryGetValue("username", out JToken usernameToken))
            {
                Server.SendError(HttpStatusCode.BadRequest);
                return;
            }
            if (!json.TryGetValue("password", out JToken passwordToken))
            {
                Server.SendError(HttpStatusCode.BadRequest);
                return;
            }

            string username = usernameToken.Value<string>();
            string password = passwordToken.Value<string>();

            var user = Program.Database.Select<User>($"username = '{username}' AND password = '{password}'").FirstOrDefault();
            
            if (user != null)
            {
                // If the request already had a session, update the userId
                if (CurrentSession != null && CurrentSession.User == null)
                {
                    CurrentSession.User = user.Id;
                    Database.Update(CurrentSession);
                }
                else
                {
                    // Create a new session
                    CurrentSession = Utils.CreateSession(user);
                    // Set a new session cookie
                    Utils.AddCookie(Response, "session", CurrentSession.Id);
                }

                // user exist. valid login
                Server.SendJSON(new JObject
                {
                    {"sessionId", CurrentSession.Id },
                    {"accesslevel", (int)user.AccessLevel}
                });
            }
            else
            {
                // invalid login
                Server.SendError(HttpStatusCode.Unauthorized);
            }
        }
    }
}
