using API.Database;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace API.HTTP.Endpoints
{

    [EndpointUrl("/taakaanmaken")]
    public sealed class HTMLTaakAanmaken : HTMLEndpoint
    {
        public override void GET(Dictionary<string, string> parameters)
        {
            var user = Program.Database.Select<User>().LastOrDefault();
            var task = Program.Database.Select<Task>().LastOrDefault();

            //Templates worden naar client gestuurd, url wordt van endpoint gehaald
            Server.SendText(Response,
                Templates.RunTemplate(
                    GetUrl<HTMLTaakAanmaken>() + ".cshtml",
                    Request,
                    parameters,
                    new
                    {
                        User = user,
                        Task = task
                    }
                )
            );
        }

        public override void POST(Dictionary<string, string> parameters)
        {

            var users = Program.Database.Select<User>().ToList();
            foreach (var user in users)
                Program.Log.Info(user);

            var tasks = Program.Database.Select<Task>().ToList();
            foreach (var task in tasks)
                Program.Log.Info(task);

            Program.Database.Insert(new User()
            {
                Username = "heheh",
                Password = "1"
            });

            Program.Database.Insert(new Task() 
            { 
                Title = "pls"
            });

            //Program.Log.Debug("Received post request.");

            //Response.Redirect("");
            //Server.SendError(Response, HttpStatusCode.Redirect);


        }
    }

   
}
