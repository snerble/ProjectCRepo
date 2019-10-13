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
		private string HTMLFileDir => Program.Config["serverSettings"]["htmlSourceDir"].ToObject<string>();

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
						if (response.OutputStream.CanWrite)
							response.OutputStream.Close();
						return;
					}
				}
			}

			// Replace blank url with index.html
			if (url == "/") url = "/index.html";
			// Try to find a file endpoint
			var file = HTMLFileDir + url;
			if (File.Exists(file))
			{
				Send(response, File.ReadAllBytes(file));
				return;
			}

			// Send 404 if no endpoint is found
			SendError(response, HttpStatusCode.NotFound);
		}
	}
}
