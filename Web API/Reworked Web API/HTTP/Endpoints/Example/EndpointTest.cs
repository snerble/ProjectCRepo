using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Test subclass of <see cref="JsonEndpoint"/>. Will be removed later.
	/// </summary>
	[EndpointUrl("/test.json")]
	public sealed class EndpointTest : JsonEndpoint
	{
		public EndpointTest(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

		protected override void GET(JObject json, Dictionary<string, string> parameters)
		{
			if (parameters.Count == 0)
			{
				Server.SendError(Response, HttpStatusCode.OK);
				return;
			}
			string outtext = "<html>";
			int max = parameters.Max(x => x.Key.Length);
			foreach (var item in parameters)
			{
				outtext += $"{item.Key}:{new string(' ', max - item.Key.Length)}   {item.Value}<br>";
			}
			Server.SendText(Response, outtext + "</html>");
		}
	}
}
