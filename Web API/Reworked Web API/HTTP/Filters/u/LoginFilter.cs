using API.HTTP.Endpoints;
using System;
using System.Linq;
using System.Net;

namespace API.HTTP.Filters
{
	/// <summary>
	/// Custom <see cref="Filter"/> class that prevents unauthorized access to urls underneath the /u url.
	/// <para/>
	/// If a request was made without the correct login info, a 404 is sent instead.
	/// </summary>
	[FilterUrl("/u/")]
	public sealed class LoginFilter : Filter
	{
		protected override void Main()
		{
			// Get login data
			string username = Request.Cookies.FirstOrDefault(x => x.Name == "username")?.Value;
			string token = Request.Cookies.FirstOrDefault(x => x.Name == "token")?.Value;

			// Check if the values are not null
			if (username == null || token == null)
			{
				Program.Log.Fine($"Denied unauthorized request to \"{Request.Url.LocalPath}\"");
				Response.Redirect(Endpoint.GetUrl<Login>() + "?redirect=" + Uri.EscapeDataString(Request.RawUrl));
				Server.SendError(Response, HttpStatusCode.Found);
				Interrupt();
			}
		}
	}
}
