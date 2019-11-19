﻿using API.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/Login1")]
    public class Login1 : HTMLEndpoint
    {
        public override void GET(Dictionary<string, string> parameters)
        {
                Server.SendHTML(Response, "/login.html");
        }

        public override void POST(Dictionary<string, string> parameters)
        {
            string username = parameters["username"];
            string password = parameters["password"];

            var user = Program.Database.Select<User>($"username = '{username}' AND password = '{password}'").FirstOrDefault();

            Program.Log.Info(user);



            if (user == null)
            {
                Program.Log.Info("Invalid login info");
                Server.SendError(Response, HttpStatusCode.NoContent);
            }//else if(username or password incorrect)
            //{        Program.Log.Info("Wrong info");
            //         Response.Redirect("pagina met rood randje om wat fout is/ text met dat de gegevens niet kloppen");
            //         Server.SendError(Response, HttpStatusCode.Redirect);
            //}
            else
            {
                Program.Log.Info("Correct info");
                Response.Redirect("home_vp.html");
                Server.SendError(Response, HttpStatusCode.Redirect);
            }
        }
    }
}