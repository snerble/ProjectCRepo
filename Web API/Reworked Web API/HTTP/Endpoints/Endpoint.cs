using System;
using System.Collections.Generic;
using System.Net;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// An abstract class representing a webserver endpoint that can be invoked.
	/// </summary>
	public abstract class Endpoint
	{
		/// <summary>
		/// The request object passed to this <see cref="Endpoint"/> instance.
		/// </summary>
		protected HttpListenerRequest Request { get; }
		/// <summary>
		/// The response object passed to this <see cref="Endpoint"/> instance.
		/// </summary>
		protected HttpListenerResponse Response { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="Endpoint"/>.
		/// </summary>
		/// <param name="request">The <see cref="HttpListenerRequest"/> object to pass to this endpoint.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> object to pass to this endpoint.</param>
		public Endpoint(HttpListenerRequest request, HttpListenerResponse response)
		{
			Request = request;
			Response = response;
		}

		/// <summary>
		/// Invokes this <see cref="Endpoint"/>.
		/// </summary>
		public abstract void Invoke();

		/// <summary>
		/// Splits a url query into a dictionary.
		/// </summary>
		protected static Dictionary<string, string> SplitQuery(string query)
		{
			var queryParts = query.Split('?');
			// Return empty dict if there is no query string
			if (queryParts.Length == 1) return new Dictionary<string, string>();

			// Split the query string into key-value pairs
			var items = Uri.UnescapeDataString(queryParts[1]).Split('&');
			var outDict = new Dictionary<string, string>();
			foreach (var item in items)
			{
				// Try to parse every key-value pair
				var keyValuePair = item.Split('=', 2);
				outDict[keyValuePair[0]] = string.Join("", keyValuePair[1..]);
			}
			return outDict;
		}
	}
}
