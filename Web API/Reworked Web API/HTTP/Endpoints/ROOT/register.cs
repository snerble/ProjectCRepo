﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using API.Database;
using Newtonsoft.Json.Linq;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/register")]
    public sealed class register : JsonEndpoint
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

            if(user == null)
            {
                var newUser = new User();
                newUser.Username = username;
                newUser.Password = password;
                Program.Database.Insert<User>(newUser);
            }
        }
    }
}
