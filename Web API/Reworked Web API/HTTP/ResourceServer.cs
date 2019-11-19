using API.Attributes;
using API.HTTP.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;

namespace API.HTTP
{
	/// <summary>
	/// A custom <see cref="Server"/> class that only handles requests for resources such as images, stylesheets and scripts.
	/// </summary>
	public sealed class ResourceServer : Server
	{
		private long PartialDataLimit => Program.Config["serverSettings"]["partialDataLimit"].Value<long>();

		/// <summary>
		/// Creates a new instance of <see cref="ResourceServer"/>.
		/// </summary>
		/// <param name="queue">The source of requests for this <see cref="ResourceServer"/>.</param>
		public ResourceServer(BlockingCollection<HttpListenerContext> queue) : base(queue) { }

		protected override void Main()
		{
			string url = Request.Url.AbsolutePath.ToLower();

			// Apply redirects
			var redirect = Utils.Redirects.FirstOrDefault(x => (x.ValidOn & ServerAttributeTargets.Resource) != 0 && x.Target == url);
			if (redirect != null)
			{
				// Send a 301 Permanent Redirect
				Response.Redirect(redirect.Redirect);
				SendError(HttpStatusCode.PermanentRedirect);
				return;
			}

			// Apply aliases
			var alias = Utils.Aliases.FirstOrDefault(x => (x.ValidOn & ServerAttributeTargets.Resource) != 0 && (x.Target == url || x.Alias == url));
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

			// Find all url filters
			foreach (var filterType in Filter.GetFilters(url))
			{
				var filter = Activator.CreateInstance(filterType) as Filter;
				// If invoke returned false, then further url parsing should be interrupted.
				if (!filter.Invoke(Request, Response, this)) return;
			}

			// Add bytes accept range header to advertise partial request support.
			Response.AddHeader("Accept-Ranges", $"bytes");

			// Try to find the resource and send it
			string file = Program.Config.ResourceDir + Uri.UnescapeDataString(url);
			if (File.Exists(file))
			{
				Response.AddHeader("Date", Utils.FormatTimeStamp(File.GetLastWriteTimeUtc(file)));
				// If a range was specified, create and send a partial response
				if (Request.Headers.Get("Range") != null)
				{
					string rangeStr = Request.Headers.Get("Range");
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

					Response.AddHeader("Content-Range", $"bytes {start}-{start+read-1}/{filesize}");
					Send(buffer, HttpStatusCode.PartialContent);
					return;
				}

				SendFile(file);
				return;
			}

			// Send 404 if no endpoint is found
			SendError(HttpStatusCode.NotFound);
		}
	}
}
