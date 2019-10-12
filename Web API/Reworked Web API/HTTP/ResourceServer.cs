using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;

namespace API.HTTP
{
	/// <summary>
	/// A custom <see cref="Server"/> class that only handles requests for resources such as images, stylesheets and scripts.
	/// </summary>
	public sealed class ResourceServer : Server
	{
		private string ResourceDir => Program.Config["serverSettings"]["resourceDir"].ToObject<string>();

		/// <summary>
		/// Creates a new instance of <see cref="ResourceServer"/>.
		/// </summary>
		/// <param name="queue">The source of requests for this <see cref="ResourceServer"/>.</param>
		public ResourceServer(BlockingCollection<HttpListenerContext> queue) : base(queue)
		{
			Program.Log.Config($"Created server {Name}");
		}

		protected override void Main(HttpListenerRequest request, HttpListenerResponse response)
		{
			string url = request.RawUrl.Split('?')[0];

			// Try to find the resource and send it
			string file = ResourceDir + url;
			if (File.Exists(file))
			{
				Send(response, File.ReadAllBytes(file));
				return;
			}

			// Send 404 if nothing is found
			SendError(response, HttpStatusCode.NotFound);
		}
	}
}
