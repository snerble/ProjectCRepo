using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

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
		public JsonServer(BlockingCollection<HttpListenerContext> queue) : base(queue)
		{
			Program.Log.Config($"Created server {Name}");
		}

		protected override void Main(HttpListenerRequest request, HttpListenerResponse response)
		{
			SendJSON(response, new JObject() { { "yeet", "lol" } });
		}
	}
}
