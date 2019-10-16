using API.HTTP.Endpoints;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace API.HTTP
{
	/// <summary>
	/// A custom <see cref="Server"/> class that exclusively serves pages (most commonly html pages) to clients like browsers.
	/// </summary>
	public sealed class HTMLServer : Server
	{
		private string HTMLFileDir
		{
			get
			{
				return Path.GetFullPath(Path.Combine(
					Directory.GetCurrentDirectory(),
					Program.Config["serverSettings"]["htmlSourceDir"].ToObject<string>()
				));
			}
		}
		private string ResourceDir
		{
			get
			{
				return Path.GetFullPath(Path.Combine(
					Directory.GetCurrentDirectory(),
					Program.Config["serverSettings"]["resourceDir"].ToObject<string>()
				));
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="HTMLServer"/>.
		/// </summary>
		/// <param name="queue">The source of requests for this <see cref="HTMLServer"/>.</param>
		public HTMLServer(BlockingCollection<HttpListenerContext> queue) : base(queue) { }

		protected override void Main(HttpListenerRequest request, HttpListenerResponse response)
		{
			string url = request.Url.AbsolutePath;

			// Get current assembly and loop through all of it's types/classes
			Assembly asm = Assembly.GetExecutingAssembly();
			foreach (Type type in asm.GetTypes())
			{
				// Find all subclasses of HTMLEndpoint
				if (type.IsSubclassOf(typeof(HTMLEndpoint)))
				{
					// Check if any of it's EndpointUrl attributes match the requested url
					var attributes = type.GetCustomAttributes(typeof(EndpointUrl)).Select(x => x as EndpointUrl);
					if (attributes.Any(x => x.Url == url))
					{
						// Create an instance of the endpoint that was found
						Activator.CreateInstance(type, request, response);
						// Close the stream if it wasn't closed by the endpoint
						return;
					}
				}
			}

			// Replace blank url with index.html
			if (url == "/") url = "/index.html";
			// Try to find a file endpoint
			// Using simple string concatination for a rather effective path injection blocker
			// TODO implement a system that checks if the resource is actually pointing to something inside the designated folders.
			string file = HTMLFileDir + Uri.UnescapeDataString(url);
			if (File.Exists(file))
			{
				Send(response, File.ReadAllBytes(file));
				return;
			}

			// Try to serve a custom page if an image was requested
			file = ResourceDir + Uri.UnescapeDataString(url);
			if (File.Exists(file) && request.AcceptTypes.Any(x => x.Contains("image/")) && ServeImage(request, response, url))
				return;

			// Send 404 if no endpoint is found
			SendError(response, HttpStatusCode.NotFound);
		}

		/// <summary>
		/// Custom function that sends a custom page for image requests.
		/// </summary>
		/// <param name="response"></param>
		private bool ServeImage(HttpListenerRequest request, HttpListenerResponse response, string file)
		{
			if (new string[] { ".webm", ".mp4", ".ogg" }.Contains(Path.GetExtension(file)))
			{
				response.AddHeader("Accept-Ranges", "bytes");
				SendText(response, "<html style=\"text-align: center\">" +
					"<body style=\"background-color: black; margin: 0; padding: 0;\">" +
					"<video controls style=\"width: 100%; max-height: 100vh;\">" +
					$"<source src=\"{file}\" type=\"video/{Path.GetExtension(file)[1..]}\">" +
					"</video>" +
					"</body>" +
					"</html>");
				return true;
			}
			return false;
		}
	}
}
