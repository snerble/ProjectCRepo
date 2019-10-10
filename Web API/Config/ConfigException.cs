using System;

namespace Config.Exceptions
{
	/// <summary>
	/// The exception that is thrown when a value in the config failed verification.
	/// </summary>
	public class ConfigException : Exception
	{
		/// <summary>
		/// Creates a new instance of <see cref="ConfigException"/>.
		/// </summary>
		public ConfigException() : base() { }
		/// <summary>
		/// Creates a new instance of <see cref="ConfigException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ConfigException(string message) : base(message) { }
		/// <summary>
		/// Creates a new instance of <see cref="ConfigException"/> with a specified error
		/// message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public ConfigException(string message, Exception innerException) : base(message, innerException) { }
	}
}
