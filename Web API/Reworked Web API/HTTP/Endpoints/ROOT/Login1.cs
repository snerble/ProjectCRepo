using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/Login1")]
    public class Login1 : HTMLEndpoint
    {
        public Login1(HttpListenerRequest request, HttpListenerResponse response) : base(request, response)
        {
        }

        public override void GET(Dictionary<string, string> parameters)
        {
            Server.SendHTML(Response, "/login.html");
        }

        public override void POST(Dictionary<string, string> parameters)
        {
            Program.Log.Info(string.Join(", ", parameters.Values));
            Response.Redirect("home_vp.html");
            Server.SendError(Response, HttpStatusCode.Redirect);
        }
    }
}
