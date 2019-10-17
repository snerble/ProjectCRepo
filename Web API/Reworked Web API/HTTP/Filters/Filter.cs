using API.HTTP.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace API.HTTP.Filters
{
	/// <summary>
	/// An abstract class representing a function that is invoked before an <see cref="Endpoint"/> is reached.
	/// </summary>
	/// <remarks>
	/// To implement a filter, you must extend <see cref="Filter"/> and give the subclass the <see cref="FilterUrl"/>
	/// attribute to specify a target url.
	/// </remarks>
	public abstract class Filter
	{
		/// <summary>
		/// Gets the request object passed to this <see cref="Filter"/> instance.
		/// </summary>
		protected HttpListenerRequest Request { get; }
		/// <summary>
		/// Gets the response object passed to this <see cref="Filter"/> instance.
		/// </summary>
		protected HttpListenerResponse Response { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="Filter"/>.
		/// </summary>
		/// <param name="request">The <see cref="HttpListenerRequest"/> object to pass to this endpoint.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> object to pass to this endpoint.</param>
		public Filter(HttpListenerRequest request, HttpListenerResponse response)
		{
			Request = request;
			Response = response;
		}

		/// <summary>
		/// Performs the primary function of this <see cref="Filter"/>.
		/// </summary>
		/// <returns>False if this <see cref="Filter"/> requests to interrupt the parsing of the URL. Otherwise true.</returns>
		public bool Invoke()
		{
			try { Main(); }
			catch (OperationCanceledException) { return false; }
			return true;
		}

		/// <summary>
		/// Abstract method that represents the primary function of this <see cref="Filter"/>.
		/// </summary>
		protected abstract void Main();

		/// <summary>
		/// Interrupts the parsing of the requested URL by throwing an <see cref="OperationCanceledException"/>.
		/// </summary>
		/// <remarks>
		/// Simply a convenience method. Do not put this in a try-catch that catches <see cref="OperationCanceledException"/>.
		/// </remarks>
		protected static void Interrupt() => throw new OperationCanceledException();

		/// <summary>
		/// Returns all types of <see cref="Filter"/> subclasses whose <see cref="FilterUrl"/> attribute
		/// matches the specified <paramref name="url"/>.
		/// </summary>
		/// <param name="url">The url of the filter.</param>
		/// <param name="asRegex">If true, interprets <paramref name="url"/> as a regular expression.</param>
		/// <exception cref="ArgumentNullException"/>
		public static IEnumerable<Type> GetFilters(string url, bool asRegex = false) => GetFilters<Filter>(url, asRegex);
		/// <summary>
		/// Returns all types of <see cref="Filter"/> subclasses whose <see cref="FilterUrl"/> attribute
		/// matches the specified <paramref name="url"/>.
		/// </summary>
		/// <typeparam name="T">The type that the returned filter must extend.</typeparam>
		/// <param name="url">The url of the filter.</param>
		/// <param name="asRegex">If true, interprets <paramref name="url"/> as a regular expression.</param>
		/// <exception cref="ArgumentNullException"/>
		public static IEnumerable<Type> GetFilters<T>(string url, bool asRegex = false) where T : Filter
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
					// Get all FilterUrl attributes and see if they match the given url
					var attributes = type.GetCustomAttributes(typeof(FilterUrl)).Cast<FilterUrl>();
					if (!asRegex && attributes.Any(x => url.ToLower().StartsWith(x.Url))) yield return type;
					else if (attributes.Any(x => Regex.IsMatch(x.Url, url))) yield return type;
				}
			}
		}
	}
}
