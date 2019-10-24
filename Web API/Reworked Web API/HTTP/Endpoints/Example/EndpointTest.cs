using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Test subclass of <see cref="HTMLEndpoint"/>. Will be removed later.
	/// </summary>
	[EndpointUrl("/")]
	public sealed class EndpointTest : HTMLEndpoint
	{
		public EndpointTest(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

		public override void GET(Dictionary<string, string> parameters)
		{
			if (parameters.Count == 0)
			{
				Server.SendError(Response, HttpStatusCode.OK);
				return;
			}
			string outtext = "";
			int max = parameters.Max(x => x.Key.Length);
			foreach (var item in parameters)
				outtext += $"{item.Key}:{new string(' ', max - item.Key.Length)} {item.Value}\n";
			Server.SendText(Response, outtext);
		}
	}
}
