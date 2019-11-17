using System;

namespace API.Attributes
{
	/// <summary>
	/// Contains flags specifying which types of requests will be affected.
	/// </summary>
	[Flags]
	public enum ServerAttributeTargets
	{
		/// <summary>
		/// Doesn't apply to any request.
		/// </summary>
		None = 0,
		/// <summary>
		/// Applies to HTML page requests.
		/// </summary>
		HTML = 1 << 0,
		/// <summary>
		/// Applies to background resource requests.
		/// </summary>
		Resource = 1 << 1,
		/// <summary>
		/// Applies to API/JSON requests.
		/// </summary>
		JSON = 1 << 2,
		/// <summary>
		/// Applies to all requests.
		/// </summary>
		All = ~None
	}
}
