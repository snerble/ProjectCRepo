using Newtonsoft.Json.Linq;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/taakaanmaken")]
    public sealed class HTMLTaakAanmaken : HTMLEndpoint
    {
        //Absolute pad naar html folder, HTMLServer.cs voor meer info
        public string HtmlDir => Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            Program.Config["serverSettings"]["htmlSourceDir"].ToObject<string>()
        ));
        
        public HTMLTaakAanmaken(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

        public override void GET(Dictionary<string, string> parameters)
            => Server.SendText(Response, Templates.RunTemplate(GetUrl<HTMLTaakAanmaken>() + ".cshtml", Request, parameters));

        public override void POST(Dictionary<string, string> parameters)
        {
            Program.Log.Debug("Received post request.");

            // voeg cookie toe
            // cookie is false
            // if button is clicked
            // cookie is true
            // laat nieuwe page zien
            

        }
    }

   
}
