using API.Attributes;
using API.HTTP.Endpoints;
using API.HTTP.Filters;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;

namespace API.HTTP
{
	/// <summary>
	/// A custom <see cref="Server"/> class that exclusively serves pages (most commonly html pages) to clients like browsers.
	/// </summary>
	public sealed class HTMLServer : Server
	{
		/// <summary>
		/// Creates a new instance of <see cref="HTMLServer"/>.
		/// </summary>
		/// <param name="queue">The source of requests for this <see cref="HTMLServer"/>.</param>
		public HTMLServer(BlockingCollection<HttpListenerContext> queue) : base(queue) { }

		protected override void Main()
		{
			

			var url = Request.Url.AbsolutePath.ToLower();
			
			// Apply redirects
			var redirect = Utils.Redirects.FirstOrDefault(x => (x.ValidOn & ServerAttributeTargets.HTML) != 0 && x.Target == url);
			if (redirect != null)
			{
				// Send a 308 Permanent Redirect
				Response.Redirect(redirect.Redirect);
				SendError(HttpStatusCode.PermanentRedirect);
				return;
			}

			// Apply aliases
			var alias = Utils.Aliases.FirstOrDefault(x => (x.ValidOn & ServerAttributeTargets.HTML) != 0 && (x.Target == url || x.Alias == url));
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
			Main(url);
		}
		/// <summary>
		/// Seconday main function used request reinterpretation.
		/// </summary>
		/// <param name="url">The url to parse.</param>
		/// <remarks>
		/// This function was nescessary to implement the ErrorPageAttribute in the most abstracted way possible.
		/// </remarks>
		private void Main(string url)
		{
			Response.ContentType = "text/html";

			// Find and invoke all url filters
			foreach (var filterType in Filter.GetFilters(url))
			{
				var filter = Activator.CreateInstance(filterType) as Filter;
				// If invoke returned false, then further url parsing should be interrupted.
				if (!filter.Invoke(Request, Response, this)) return;
			}

			// Find an endpoint
			var endpoint = Endpoint.GetEndpoint<HTMLEndpoint>(url);
			if (endpoint != null)
			{
				// Create an instance of the endpoint
				(Activator.CreateInstance(endpoint) as Endpoint).Invoke(Request, Response, this);
				return;
			}

			// Replace blank url with index.html
			if (url == "/") url = "/index.html";

			// Try to find a file endpoint
			string file = Program.Config.HTMLSourceDir + Uri.UnescapeDataString(url);
			if (File.Exists(file))
			{
				Response.AddHeader("Date", Utils.FormatTimeStamp(File.GetLastWriteTimeUtc(file)));
				SendFile(file);
				return;
			}

			// Try to serve a custom page if an image was requested
			file = Program.Config.ResourceDir + Uri.UnescapeDataString(url);
			if (File.Exists(file) && Request.AcceptTypes.Any(x => x.Contains("image/")))
			{
				Response.AddHeader("Date", Utils.FormatTimeStamp(File.GetLastWriteTimeUtc(file)));
				ServeImage(Response, url);
				return;
			}

			// Send 404 if no endpoint is found
			SendError(HttpStatusCode.NotFound);
		}

		/// <summary>
		/// Custom function that sends a custom page for image requests.
		/// </summary>
		/// <param name="response"></param>
		private void ServeImage(HttpListenerResponse response, string file)
		{
			if (new string[] { ".webm", ".mp4", ".ogg" }.Contains(Path.GetExtension(file)))
			{
				response.AddHeader("Accept-Ranges", "bytes");
				// TODO implement these shenanigans in a neat (hopefully .cshtml) template
				SendText("<html style=\"text-align: center\">" +
					"<body style=\"background-color: black; margin: 0; padding: 0;\">" +
					"<video controls style=\"width: 100%; max-height: 100vh;\">" +
					$"<source src=\"{file}\" type=\"video/{Path.GetExtension(file)[1..]}\">" +
					"</video>" +
					"</body>" +
					"</html>");
				return;
			}
			Send(File.ReadAllBytes(Program.Config.ResourceDir + Uri.UnescapeDataString(file)));
		}

		private HttpStatusCode PreviousCode;
		public override void SendError(HttpStatusCode statusCode)
		{
			// Try to find an errorpage attribute
			var errorPage = Utils.ErrorPages.FirstOrDefault(x => x.StatusCode == statusCode);
			if (errorPage != null)
			{
				// Infinite loop detection
				if (errorPage.Url == Request.Url.AbsoluteUri.ToLower() || statusCode == PreviousCode)
				{
					Program.Log.Error($"Caught infinite loop for error page '{errorPage.Url}' for status code {(int)statusCode}.");
					Program.Log.Error("Please check Properties.cs"); // TODO replace with proper support message once this program gets published.
					Program.ExitCode = 1;
					return;
				}
				// Store current statuscode to detect infinite loop
				PreviousCode = statusCode;
				// Parse the error page url instead and send that endpoint
				Main(errorPage.Url);
				// Unset the statuscode cache
				PreviousCode = 0;
				return;
			}
			base.SendError(statusCode);
		}
	}
}
