using System;

namespace API.Attributes
{
#nullable enable
	/// <summary>
	/// Specifies an alias for a specific url.
	/// </summary>
	/// <remarks>
	/// This attribute is handled after <see cref="RedirectAttribute"/>.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class AliasAttribute : Attribute
	{
		/// <summary>
		/// Gets the target url to which this <see cref="AliasAttribute"/> applies.
		/// </summary>
		public string Target { get; }
		/// <summary>
		/// Gets the url alias.
		/// </summary>
		public string? Alias { get; }

		/// <summary>
		/// Gets or sets what requests are affected by this <see cref="AliasAttribute"/> using
		/// <see cref="ServerAttributeTargets"/> flags. <see cref="ServerAttributeTargets.All"/> by default.
		/// </summary>
		public ServerAttributeTargets ValidOn { get; set; }
		/// <summary>
		/// Gets or sets what requests are not affected by this <see cref="AliasAttribute"/> using
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
		/// Gets or sets whether the target url should be inaccessible. True by default
		/// </summary>
		public bool HideTarget { get; set; } = true;

		/// <summary>
		/// Initializes a new instance of <see cref="AliasAttribute"/>.
		/// </summary>
		/// <param name="target">The target url to which this <see cref="AliasAttribute"/> applies.</param>
		/// <param name="alias">The alias to use.</param>
		/// <remarks>
		/// If <see cref="HideTarget"/> is true and <paramref name="alias"/> is null, this attribute will completely hide
		/// <paramref name="target"/> with no means to access it.
		/// </remarks>
		public AliasAttribute(string target, string? alias)
		{
			Target = target?.ToLower() ?? throw new ArgumentNullException(nameof(target));
			Alias = alias?.ToLower();
		}
	}
#nullable disable
}
