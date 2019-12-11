using System;
using API.HTTP.Endpoints;

namespace API.Attributes
{
	/// <summary>
	/// Indicates that an <see cref="Endpoint"/> requires valid login info in order to respond.
	/// If login info is missing, a 401 Unauthorized status code is sent instead.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class RequiresLoginAttribute : Attribute
	{
		/// <summary>
		/// Gets the local url to which this <see cref="RequiresLoginAttribute"/> applies.
		/// </summary>
		public string Target { get; }

		/// <summary>
		/// Gets or sets what requests are affected by this <see cref="RequiresLoginAttribute"/> using
		/// <see cref="ServerAttributeTargets"/> flags. <see cref="ServerAttributeTargets.All"/> by default.
		/// </summary>
		public ServerAttributeTargets ValidOn { get; set; } = ServerAttributeTargets.All;
		/// <summary>
		/// Gets or sets what requests are not affected by this <see cref="RequiresLoginAttribute"/> using
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
		/// Initializes a new instance of <see cref="RequiresLoginAttribute"/>.
		/// <para>This constructor is only relevant when applied to an <see cref="Endpoint"/>.</para>
		/// </summary>
		public RequiresLoginAttribute() { }
		/// <summary>
		/// Initializes a new instance of <see cref="RequiresLoginAttribute"/> with the specified target url.
		/// <para>This constructor is only relevant when applied to the assembly.</para>
		/// </summary>
		/// <param name="targetUrl">The url of the resource that requires the client to be logged in.</param>
		public RequiresLoginAttribute(string targetUrl)
		{
			Target = targetUrl;
		}
	}
}
