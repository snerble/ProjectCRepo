using System;
using System.Net;

namespace API.Attributes
{
#nullable enable
	/// <summary>
	/// Specifies which url should be used when a specific errorcode is sent to the client.
	/// </summary>
	/// <remarks>
	/// This attribute only applies to responses with Content-Type 'text/html' with no response body.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class ErrorPageAttribute : Attribute
	{
		/// <summary>
		/// Gets the <see cref="HttpStatusCode"/> to which this <see cref="ErrorPageAttribute"/> applies.
		/// </summary>
		public HttpStatusCode StatusCode { get; }
		/// <summary>
		/// Gets the url of the errorpage.
		/// </summary>
		public string Url { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="ErrorPageAttribute"/>.
		/// </summary>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to which this <see cref="ErrorPageAttribute"/> applies.</param>
		/// <param name="url">The url of the errorpage.</param>
		public ErrorPageAttribute(HttpStatusCode statusCode, string url)
		{
			StatusCode = statusCode;
			Url = url?.ToLower() ?? throw new ArgumentNullException(nameof(url));
		}
	}
#nullable disable
}
