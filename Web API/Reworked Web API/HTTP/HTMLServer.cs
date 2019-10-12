using System.Collections.Concurrent;
using System.Net;
using System.IO;

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
		public HTMLServer(BlockingCollection<HttpListenerContext> queue) : base(queue)
		{
			Program.Log.Config($"Created server {Name}");
		}

		protected override void Main(HttpListenerRequest request, HttpListenerResponse response)
		{
			// Seperate the raw url into url and tokens
			string[] _ = request.RawUrl.Split('?');
			string url = _[0];
			string[] tokens;
			if (_.Length >= 2) tokens = _[1].Split('&');
			// emtpy urls default to index.html
			if (url == "/") url = "/index.html";

			// Deny any request trying to reach an endpoint outside of the HTML source folder
			if (url.Contains("..")) SendError(response, HttpStatusCode.Forbidden);

			if (false)
			{
				// TODO implement code endpoints
			}

			// Try to find a file endpoint
			var file = HTMLFileDir + url;
			if (File.Exists(file))
			{
				Send(response, File.ReadAllBytes(file));
				return;
			}

			// If no file endpoint was found, send 404
			SendError(response, HttpStatusCode.NotFound);
		}
	}
}
