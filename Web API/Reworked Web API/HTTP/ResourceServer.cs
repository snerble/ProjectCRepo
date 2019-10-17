using API.HTTP.Filters;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;

namespace API.HTTP
{
	/// <summary>
	/// A custom <see cref="Server"/> class that only handles requests for resources such as images, stylesheets and scripts.
	/// </summary>
	public sealed class ResourceServer : Server
	{
		private long PartialDataLimit => Program.Config["serverSettings"]["partialDataLimit"].ToObject<long>();
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
		/// Creates a new instance of <see cref="ResourceServer"/>.
		/// </summary>
		/// <param name="queue">The source of requests for this <see cref="ResourceServer"/>.</param>
		public ResourceServer(BlockingCollection<HttpListenerContext> queue) : base(queue) { }

		protected override void Main(HttpListenerRequest request, HttpListenerResponse response)
		{
			string url = request.Url.AbsolutePath;

			// Find all url filters
			foreach (var filterType in Filter.GetFilters(url))
			{
				var filter = Activator.CreateInstance(filterType, request, response) as Filter;
				// If invoke returned false, then further url parsing should be interrupted.
				if (!filter.Invoke()) return;
			}

			// Add bytes accept range header to advertise partial request support.
			response.AddHeader("Accept-Ranges", $"bytes");

			// Try to find the resource and send it
			string file = ResourceDir + Uri.UnescapeDataString(url);
			if (File.Exists(file))
			{
				// If a range was specified, create and send a partial response
				if (request.Headers.Get("Range") != null)
				{
					string rangeStr = request.Headers.Get("Range");
					string[] range = rangeStr.Replace("bytes=", "").Split('-');

					var fs = File.OpenRead(file);
					int filesize = (int)fs.Length;

					// Prepare range values
					int start = int.Parse(range[0]);
					int end = -1;
					if (range[1].Trim().Length > 0) int.TryParse(range[1], out end);
					if (end == -1) end = filesize-1;
					end += 1; // the end byte value is inclusive, so increment by one

					// Limit buffer size to data limit
					byte[] buffer = new byte[end - start > PartialDataLimit ? PartialDataLimit : end - start];
					fs.Position = start;
					int read = fs.Read(buffer, 0, buffer.Length);
					fs.Dispose();

					response.AddHeader("Content-Range", $"bytes {start}-{start+read-1}/{filesize}");
					Send(response, buffer, HttpStatusCode.PartialContent);
					return;
				}

				Send(response, File.ReadAllBytes(file));
				return;
			}

			// Send 404 if no endpoint is found
			SendError(response, HttpStatusCode.NotFound);
		}
	}
}
