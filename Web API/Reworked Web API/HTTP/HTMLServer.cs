using System.Collections.Concurrent;
using System.IO;
using System.Net;

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
			// Seperate the raw url into url and tokens
			string[] urlParts = request.RawUrl.Split('?');
			string url = urlParts[0];
			string[] tokens;
			if (urlParts.Length >= 2) tokens = urlParts[1].Split('&');
			// emtpy urls default to index.html
			if (url == "/") url = "/index.html";

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
