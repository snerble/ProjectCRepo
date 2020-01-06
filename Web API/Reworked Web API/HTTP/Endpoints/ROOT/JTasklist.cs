using System.Linq;
using System.Collections.Generic;
using System.Text;
using API.Database;
using Newtonsoft.Json.Linq;

namespace API.HTTP.Endpoints.ROOT
{
    [EndpointUrl("/tasklist")]
    public class JTasklist : JsonEndpoint
    {
        public override void GET(JObject json, Dictionary<string, string> parameters)
        {
            Program.Log.Info(CurrentUser);
            var results = Program.Database.Select<Group>("creator = " + CurrentUser.Id);

            Server.SendJSON(new JObject() {
                {"results", new JArray(results.Select(x => (JObject)x)) }
            });
        }
    }
}
