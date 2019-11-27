using System;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Indicates that the <see cref="JsonEndpoint"/> requires encrypted data.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class RequiresEncryptionAttribute : Attribute { }
}
