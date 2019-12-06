using API.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/Login1")]
    public class Login1_deprecated : HTMLEndpoint
    {
        public override void GET(Dictionary<string, string> parameters)
        {
                Server.SendFile(Program.Config.HTMLSourceDir + "/login.html");
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
                Server.SendError(HttpStatusCode.NoContent);
            }//else if(username or password incorrect)
            //{        Program.Log.Info("Wrong info");
            //         Response.Redirect("login_wrong.html");
            //         Server.SendError(Response, HttpStatusCode.Redirect);
            //}
            else
            {
                Program.Log.Info("Correct info");
                Response.Redirect("home_vp.html");
                Server.SendError(HttpStatusCode.Redirect);
            }
        }
    }
}
