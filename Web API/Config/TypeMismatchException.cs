using System;

namespace Config.Exceptions
{
	/// <summary>
	/// The exception that is thrown when a value in an existing config does not match the type of the default value.
	/// </summary>
	public class TypeMismatchException : ConfigException
	{
		/// <summary>
		/// Creates a new instance of <see cref="TypeMismatchException"/>.
		/// </summary>
		public TypeMismatchException() : base() { }
		/// <summary>
		/// Creates a new instance of <see cref="TypeMismatchException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public TypeMismatchException(string message) : base(message) { }
		/// <summary>
		/// Creates a new instance of <see cref="TypeMismatchException"/> with a specified error
		/// message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public TypeMismatchException(string message, Exception innerException) : base(message, innerException) { }
	}
}
