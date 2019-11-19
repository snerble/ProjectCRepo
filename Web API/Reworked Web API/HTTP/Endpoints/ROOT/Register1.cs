using API.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace API.HTTP.Endpoints.ROOT
{
    [EndpointUrl("/Register1")]

    // public zetten zodat die overal gebruikt/ gezien kan worden. aangeven dat het een htmlendpoint is
    //voor de webbrowser pagina's.
    public class Register1 : HTMLEndpoint
    {
        // laat dit nou de pagina zien? HTTP GET is used to read/retrieve
        // a representation of a rescoure. only used to read data and not change it
        // wat houd  HttpListenerResponse in?
        public override void GET(Dictionary<string, string> parameters)
        {
            Server.SendFile(Program.Config.HTMLSourceDir + "/register.html");
        }

        // POST is for creating new resources,
        public override void POST(Dictionary<string, string> parameters)
        {
            // de onderdelen die dus moeten worden opgeslagen in de database
            string name = parameters["name"];
            string username = parameters["username"];
            string email = parameters["email"];
            string password = parameters["password"];
            // ik heb een confirm password maar dat is alleen een check dus hoort hier niet bij

            // wat is het verschil tussen GETinsert<> en Insert<> // dit is vast niet goed
            Program.Database.Insert(new User() { Username = username, Password = password });

           // Program.Log.Info(newUser);// zo krijg je de info te zien in zwarte beeld/ command line??
            
            //als dus alles is ingevuld en gelukt is met database dan moet die door
            Response.Redirect("home_vp.html");
            Server.SendError(System.Net.HttpStatusCode.Redirect);

            // moet ik een error implementeren? inprincipe conditions in js

            // uiteindelijk dus ook cookies?
        }
    }
}
