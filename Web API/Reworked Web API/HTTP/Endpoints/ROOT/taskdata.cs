using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using API.Database;
using Newtonsoft.Json.Linq;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/taskdata")]
    public sealed class taskdata : JsonEndpoint
    {
        public override void GET(JObject json, Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }

        public override void POST(JObject json, Dictionary<string, string> parameters)
        {
            var tasks = Program.Database.Select<Task>();
            var taskJson = new JObject();

            foreach (Task task in tasks)
            {
                taskJson.Add("results", new JArray(tasks.ToArray()));
            }

            Server.SendJSON(taskJson);
        }
    }
}
