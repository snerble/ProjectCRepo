using System;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// A custom attribute class meant to decorate <see cref="Endpoint"/> subclasses and specify their target url.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class EndpointUrl : Attribute
	{
		/// <summary>
		/// The target url of an endpoint.
		/// </summary>
		public string Url { get; }

		/// <summary>
		/// Specifies the target url of an endpoint.
		/// </summary>
		/// <param name="url">The target url.</param>
		public EndpointUrl(string url)
		{
			Url = url?.ToLower() ?? throw new ArgumentNullException("url");
		}
	}
}
