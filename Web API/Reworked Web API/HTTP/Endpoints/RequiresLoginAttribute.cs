using System;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Indicates that an <see cref="Endpoint"/> requires valid login info in order to respond.
	/// If login info is missing, a 401 Unauthorized status code is sent instead.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RequiresLoginAttribute : Attribute { }
}
