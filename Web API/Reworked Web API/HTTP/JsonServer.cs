using API.HTTP.Endpoints;
using API.HTTP.Filters;
using System;
using System.Collections.Concurrent;
using System.Net;

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

			// Find all url filters
			foreach (var filterType in Filter.GetFilters(url))
			{
				var filter = Activator.CreateInstance(filterType, request, response) as Filter;
				// If invoke returned false, then further url parsing should be interrupted.
				if (!filter.Invoke()) return;
			}

			// Find an endpoint
			var endpoint = Endpoint.GetEndpoint<JsonEndpoint>(url);
			if (endpoint != null)
			{
				// Create an instance of the endpoint
				Activator.CreateInstance(endpoint, request, response);
				// Close the response if the endpoint didn't close it
				try { response.Close(); }
				catch (ObjectDisposedException) { }
				return;
			}

			// Send 404 if no endpoint is found
			SendError(response, HttpStatusCode.NotFound);
		}
	}
}
