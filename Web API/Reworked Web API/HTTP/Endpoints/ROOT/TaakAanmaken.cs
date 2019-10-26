using System.Collections.Generic;
using System.Net;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/taakaanmaken")]
    public sealed class HTMLTaakAanmaken : HTMLEndpoint
    {
        public HTMLTaakAanmaken(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

        public override void GET(Dictionary<string, string> parameters)
            //Templates worden naar client gestuurd, url wordt van endpoint gehaald
            => Server.SendText(Response, Templates.RunTemplate(GetUrl<HTMLTaakAanmaken>() + ".cshtml", Request, parameters));

        public override void POST(Dictionary<string, string> parameters)
        {
            Program.Log.Debug("Received post request.");

           
            

        }
    }

   
}
