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

		protected override void Main(HttpListenerRequest request, HttpListenerResponse response)
		{
			response.ContentType = "text/html";
			string url = request.Url.AbsolutePath;

			// Find all url filters
			foreach (var filterType in Filter.GetFilters(url))
			{
				var filter = Activator.CreateInstance(filterType) as Filter;
				// If invoke returned false, then further url parsing should be interrupted.
				if (!filter.Invoke(request, response)) return;
			}

			// Find an endpoint
			var endpoint = Endpoint.GetEndpoint<HTMLEndpoint>(url);
			if (endpoint != null)
			{
				// Create an instance of the endpoint
				(Activator.CreateInstance(endpoint) as Endpoint).Invoke(request, response);
				return;
			}

			// Replace blank url with index.html
			if (url == "/") url = "/index.html";

			// Try to find a file endpoint
			string file = Uri.UnescapeDataString(url);
			if (File.Exists(Program.Config.HTMLSourceDir + file))
			{
				SendHTML(response, file);
				return;
			}

			// Try to serve a custom page if an image was requested
			file = Program.Config.ResourceDir + Uri.UnescapeDataString(url);
			if (File.Exists(file) && request.AcceptTypes.Any(x => x.Contains("image/")))
			{
				ServeImage(response, url);
				return;
			}

			// Send 404 if no endpoint is found
			SendError(response, HttpStatusCode.NotFound);
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
				SendText(response,
					"<html style=\"text-align: center\">" +
					"<body style=\"background-color: black; margin: 0; padding: 0;\">" +
					"<video controls style=\"width: 100%; max-height: 100vh;\">" +
					$"<source src=\"{file}\" type=\"video/{Path.GetExtension(file)[1..]}\">" +
					"</video>" +
					"</body>" +
					"</html>");
				return;
			}
			Send(response, File.ReadAllBytes(Program.Config.ResourceDir + Uri.UnescapeDataString(file)));
		}
	}
}
