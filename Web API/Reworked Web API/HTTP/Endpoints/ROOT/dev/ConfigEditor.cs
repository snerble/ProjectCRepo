using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace API.HTTP.Endpoints
{
	[EndpointUrl("/dev/config")]
	public sealed class ConfigEditor : HTMLEndpoint
	{
		public override void GET(Dictionary<string, string> parameters)
		{
			Server.SendText(
				Response,
				Templates.RunTemplate(
					GetUrl<ConfigEditor>() + ".cshtml",
					Request,
					parameters,
					new
					{
						Title = "Server Configuration",
						Data = Program.Config.ToString(),
						Language = "javascript"
					}
				)
			);
		}
	}

	[EndpointUrl("/dev/config")]
	public sealed class ConfigUploader : JsonEndpoint
	{
		public override void GET(JObject json, Dictionary<string, string> parameters)
			=> Server.SendError(Response, HttpStatusCode.NotImplemented);

		public void PUT(JObject json, Dictionary<string, string> parameters)
		{
			lock (Program.Config)
			{
				try
				{
					Program.Config.Update(json);
					Server.SendError(Response, HttpStatusCode.Created);
				}
				catch (Exception)
				{
					Server.SendError(Response, HttpStatusCode.PreconditionFailed);
				}
			}
		}
	}
}
