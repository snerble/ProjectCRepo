using System;

namespace API.Attributes
{
#nullable enable
	/// <summary>
	/// Specifies a url that will always redirect to another url.
	/// </summary>
	/// <remarks>
	/// This attribute is handled before <see cref="AliasAttribute"/>.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class RedirectAttribute : Attribute
	{
		/// <summary>
		/// Gets the local url to which this <see cref="RedirectAttribute"/> applies.
		/// </summary>
		public string Target { get; }
		/// <summary>
		/// Gets the local url to which the client must be redirected to.
		/// </summary>
		public string Redirect { get; }

		/// <summary>
		/// Gets or sets what requests are affected by this <see cref="RedirectAttribute"/> using
		/// <see cref="ServerAttributeTargets"/> flags. <see cref="ServerAttributeTargets.All"/> by default.
		/// </summary>
		public ServerAttributeTargets ValidOn { get; set; } = ServerAttributeTargets.All;
		/// <summary>
		/// Gets or sets what requests are not affected by this <see cref="RedirectAttribute"/> using
		/// <see cref="ServerAttributeTargets"/> flags. <see cref="ServerAttributeTargets.None"/> by default.
		/// </summary>
		/// <remarks>
		/// Identical to assigning a value to <see cref="ValidOn"/> using bitwise NOT.
		/// </remarks>
		public ServerAttributeTargets InvalidOn
		{
			get { return ~ValidOn; }
			set { ValidOn = ~value; }
		}

		/// <summary>
		/// Initializes a new instance of <see cref="RedirectAttribute"/>.
		/// </summary>
		/// <param name="targetUrl">The target url to redirect.</param>
		/// <param name="redirectUrl">The url that the client will be redirected to.</param>
		public RedirectAttribute(string targetUrl, string redirectUrl)
		{
			Target = targetUrl?.ToLower() ?? throw new ArgumentNullException(nameof(targetUrl));
			Redirect = redirectUrl?.ToLower() ?? throw new ArgumentNullException(nameof(redirectUrl));
		}
	}
#nullable disable
}
