using API.Attributes;
using API.HTTP.Endpoints;
using API.HTTP.Filters;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

namespace API.HTTP
{
	/// <summary>
	/// A custom <see cref="Server"/> instance that only accepts JSON type requests.
	/// </summary>
	public sealed class JsonServer : Server
	{
		/// <summary>
		/// Creates a new instance of <see cref="JsonServer"/>.
		/// </summary>
		/// <param name="queue">The source of requests for this <see cref="JsonServer"/>.</param>
		public JsonServer(BlockingCollection<HttpListenerContext> queue) : base(queue) { }

		protected override void Main()
		{
			Response.ContentType = "application/json";
			string url = Request.Url.AbsolutePath.ToLower();

			// Apply redirects
			var redirect = Program.Redirects.FirstOrDefault(x => (x.ValidOn & ServerAttributeTargets.JSON) != 0 && x.Target == url);
			if (redirect != null)
			{
				// Send a 301 Permanent Redirect
				Response.Redirect(redirect.Redirect);
				SendError(HttpStatusCode.PermanentRedirect);
				return;
			}

			// Apply aliases
			var alias = Program.Aliases.FirstOrDefault(x => (x.ValidOn & ServerAttributeTargets.JSON) != 0 && (x.Target == url || x.Alias == url));
			if (alias != null)
			{
				if (alias.HideTarget && url == alias.Target)
				{
					// Send 404 Not Found if the target was requested but should be hidden
					SendError(HttpStatusCode.NotFound);
					return;
				}
				// Replace the requested url with the actual target url
				url = alias.Target;
			}

			// Find all url filters
			foreach (var filterType in Filter.GetFilters(url))
			{
				var filter = Activator.CreateInstance(filterType) as Filter;
				// If invoke returned false, then further url parsing should be interrupted.
				if (!filter.Invoke(Request, Response, this)) return;
			}

			// Find an endpoint
			var endpoint = Endpoint.GetEndpoint<JsonEndpoint>(url);
			if (endpoint != null)
			{
				// Create an instance of the endpoint
				(Activator.CreateInstance(endpoint) as Endpoint).Invoke(Request, Response, this);
				// Close the response if the endpoint didn't close it
				try { Response.Close(); }
				catch (ObjectDisposedException) { }
				return;
			}

			// Send 404 if no endpoint is found
			SendError(HttpStatusCode.NotFound);
		}
	}
}
