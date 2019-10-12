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
			string url = request.Url.AbsolutePath;
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

			// Send 404 if no endpoint is found
			SendError(response, HttpStatusCode.NotFound);
		}
	}
}
