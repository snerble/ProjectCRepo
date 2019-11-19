using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace API.HTTP.Endpoints.ROOT
{
    [EndpointUrl("/profile")]
    public sealed class Profile : HTMLEndpoint
    {
        public override void GET(Dictionary<string, string> parameters)
        {
            Response.Headers.Add("Content-Type", "text/html");
            Server.SendFile(Program.Config.HTMLSourceDir + "/profile.html");
            //Server.SendText(Response, "hello");
        }
    }
}
