using API.Database;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace API.HTTP.Endpoints
{
    [EndpointUrl("/takenlijst")]
    public sealed class HTMLtakenlijst : HTMLEndpoint
    {
        public override void GET(Dictionary<string, string> parameters)
        {
            var user = Database.Select<User>().LastOrDefault();
            var tasks = Database.Select<Task>().ToList();

            //Templates worden naar client gestuurd, url wordt van endpoint gehaald
            Server.SendText(
                Templates.RunTemplate(
                    GetUrl<HTMLtakenlijst>() + ".cshtml",
                    Request,
                    parameters,
                    new
                    {
                        User = user,
                        Tasks = tasks
                    }
                )
            );
        }
        public override void POST(Dictionary<string, string> parameters)
        {
            Program.Log.Debug("Received post request.");
        }

        public override void DELETE(Dictionary<string, string> parameters)
        {
            Program.Log.Info("Received DELETE request");

            //Database.Delete<Task>($"parameters["task"] = { Title }");
        }
    }


}