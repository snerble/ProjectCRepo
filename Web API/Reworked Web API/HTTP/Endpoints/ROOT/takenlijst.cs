using System.Collections.Generic;
using System.Net;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/takenlijst")]
    public sealed class HTMLtakenlijst : HTMLEndpoint
    {
        public HTMLtakenlijst(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

        public override void GET(Dictionary<string, string> parameters)
            //Templates worden naar client gestuurd, url wordt van endpoint gehaald
            => Server.SendText(Response, Templates.RunTemplate(GetUrl<HTMLtakenlijst>() + ".cshtml", Request, parameters));

        public override void POST(Dictionary<string, string> parameters)
        {
           
            Program.Log.Debug("Received post request.");

            
        }


    }


}