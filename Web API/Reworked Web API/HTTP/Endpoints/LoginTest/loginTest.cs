using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace API.HTTP.Endpoints.LoginTest
{
    [EndpointUrl("/logintest")]
    public sealed class loginTest : JsonEndpoint
    {
        public override void GET(JObject json, Dictionary<string, string> parameters)
        {
            Server.SendJSON(new JObject
            {
                {"data", "Thees ees test, ees very gut!" }
            });
        }
    }
}
