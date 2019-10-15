using API.HTTP.Endpoints;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Reflection;

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

		protected override void Main(HttpListenerRequest request, HttpListenerResponse response)
		{
			string url = request.Url.AbsolutePath;

			// Get current assembly and loop through all of it's types/classes
			Assembly asm = Assembly.GetExecutingAssembly();
			foreach (Type type in asm.GetTypes())
			{
				// Find all subclasses of HTMLEndpoint
				if (type.IsSubclassOf(typeof(JsonEndpoint)))
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

			// Send 404 if no endpoint is found
			SendError(response, HttpStatusCode.NotFound);
		}
	}
}
