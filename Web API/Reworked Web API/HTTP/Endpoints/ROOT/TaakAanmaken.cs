using API.Database;
using System.Collections.Generic;
using System.Linq;

namespace API.HTTP.Endpoints
{

    [EndpointUrl("/taakaanmaken")]
    public sealed class HTMLTaakAanmaken : HTMLEndpoint
    {
        public override void GET(Dictionary<string, string> parameters)
        {
            //var user = Program.Database.Select<User>().LastOrDefault();
            //var task = Program.Database.Select<Task>().ToArray();

            //Templates worden naar client gestuurd, url wordt van endpoint gehaald
            Server.SendText(Response,
                Templates.RunTemplate(
                    GetUrl<HTMLTaakAanmaken>() + ".cshtml",
                    Request,
                    parameters
                    //new
                    //{
                    //    User = user,
                    //    Task = task
                    //}
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

            /*Program.Database.Insert(new User()
            {
                Id = 2,
                Username = "Cindy",
                Password = "1",
                Token = 0,
            });

            /*Program.Database.Insert(new Group()
            {
                Id = 1,
                Creator = 2,
                Name = "Groep 1"
            });*/

            /*Program.Database.Insert(new Task()
            {
                Creator = 2,
                Title = parameters["title"],
                Description = parameters["description"],
                Group = 1
            });*/

            //Program.Log.Debug("Received post request.");

            //Response.Redirect("");
            //Server.SendError(Response, HttpStatusCode.Redirect);

        }
    }

   
}
