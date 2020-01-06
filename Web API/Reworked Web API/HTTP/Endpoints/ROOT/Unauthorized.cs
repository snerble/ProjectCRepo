using System;
using System.Collections.Generic;
using System.Net;

namespace API.HTTP.Endpoints.ROOT
{
	[EndpointUrl("/unauthorized")]
	public sealed class Unauthorized : HTMLEndpoint
	{
		public override void GET(Dictionary<string, string> parameters)
		{
			// Build the new url (if a query string was already present, append with '&', otherwise append with '?')
			var newUrl = GetUrl<Login>() + '?' + "redir=" + Uri.EscapeDataString(Request.RawUrl);

			// Redirect the client to the login page
			Response.Redirect(newUrl);
			Server.SendError(HttpStatusCode.Redirect);
		}
	}
}
