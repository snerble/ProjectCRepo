using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// An abstract class representing a webserver endpoint that can be invoked.
	/// </summary>
	public abstract class Endpoint
	{
		/// <summary>
		/// Gets the request object passed to this <see cref="Endpoint"/> instance.
		/// </summary>
		protected HttpListenerRequest Request { get; }
		/// <summary>
		/// Gets the response object passed to this <see cref="Endpoint"/> instance.
		/// </summary>
		protected HttpListenerResponse Response { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="Endpoint"/> and immediately calls the right http method function.
		/// </summary>
		/// <param name="request">The <see cref="HttpListenerRequest"/> object to pass to this endpoint.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> object to pass to this endpoint.</param>
		public Endpoint(HttpListenerRequest request, HttpListenerResponse response)
		{
			Request = request;
			Response = response;
		}

		/// <summary>
		/// Returns the type of an <see cref="Endpoint"/> subclass whose <see cref="EndpointUrl"/> attribute
		/// matches the specified <paramref name="url"/>, or null if none were found.
		/// </summary>
		/// <typeparam name="T">The type that the returned endpoint must extend.</typeparam>
		/// <param name="url">The url of the endpoint.</param>
		/// <param name="asRegex">If true, interprets <paramref name="url"/> as a regular expression.</param>
		/// <exception cref="ArgumentNullException"/>
		public static Type GetEndpoint<T>(string url, bool asRegex = false) where T : Endpoint
		{
			// Regex url cannot be null
			if (asRegex && url == null) throw new ArgumentNullException("url", "Url used as RegEx cannot be null");
			// Get the assembly of type T and iterate through it's types
			Assembly asm = typeof(T).Assembly;
			foreach (var type in asm.GetTypes())
			{
				// Every non abstract class is tested
				if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)))
				{
					// Get all EndpointUrl attributes and see if they match the given url
					var attributes = type.GetCustomAttributes(typeof(EndpointUrl)).Cast<EndpointUrl>();
					if (!asRegex && attributes.Any(x => x.Url == url)) return type;
					else if (attributes.Any(x => Regex.IsMatch(x.Url, url))) return type;
				}
			}
			return null;
		}

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
