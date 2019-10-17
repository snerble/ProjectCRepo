using System;

namespace API.HTTP.Filters
{
	/// <summary>
	/// Attribute class that specifies the target url for a <see cref="Filter"/> class.
	/// This class cannot be inherited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class FilterUrl : Attribute
	{
		/// <summary>
		/// Gets the target url of this attribute.
		/// </summary>
		public string Url { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="FilterUrl"/>.
		/// </summary>
		/// <param name="url">The target url of the <see cref="Filter"/>.</param>
		public FilterUrl(string url)
		{
			if (!url.EndsWith("/")) throw new ArgumentException("Argument 'url' must end with '/' to indicate a full url path.");
			Url = url?.ToLower() ?? throw new ArgumentNullException("url");
		}
	}
}
