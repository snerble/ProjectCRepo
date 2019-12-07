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
            //var user = Database.Select<User>().LastOrDefault();
            //var task = Database.Select<Task>().ToArray();

            //Templates worden naar client gestuurd, url wordt van endpoint gehaald
            Server.SendText(
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
                var users = Database.Select<User>().ToList();
            foreach (var user in users)
                Program.Log.Info(user);

            var tasks = Database.Select<Task>().ToList();
            foreach (var task in tasks)
                Program.Log.Info(task);

            /*Database.Insert(new User()
            {
                Id = 2,
                Username = "Cindy",
                Password = "1",
                Token = 0,
            });

            /*Database.Insert(new Group()
            {
                Id = 1,
                Creator = 2,
                Name = "Groep 1"
            });*/

            Database.Insert(new Task()
            {
                Creator = 2,
                Title = parameters["title"],
                Description = parameters["description"],
                Group = 1
            });



            //Program.Log.Debug("Received post request.");

            //Response.Redirect("");
            //Server.SendError(Response, HttpStatusCode.Redirect);

            Server.SendError(System.Net.HttpStatusCode.Created);

            
        }
    }
}
