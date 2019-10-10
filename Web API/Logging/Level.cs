using System;
using System.Collections.Generic;

namespace Logging
{
	/// <summary>
	/// A class for creating logging levels.
	/// <para>This contains several default logging levels.</para>
	/// </summary>
	/// <seealso cref="DefaultLevels"/>
	public class Level
	{
		/// <summary>
		/// A collection of custom named <see cref="Level"/> instances.
		/// </summary>
		public static readonly Dictionary<string, Level> CustomLevels = new Dictionary<string, Level>();
		/// <summary>
		/// A collection of the default logging levels.
		/// </summary>
		public static readonly Dictionary<string, Level> DefaultLevels = new Dictionary<string, Level>();

		#region Default Levels
		/// <summary>
		/// Disables all log messages.
		/// </summary>
		public static readonly Level OFF = new Level(int.MinValue, "OFF", true);
		/// <summary>
		/// Mostly used for debugging code. 
		/// </summary>
		public static readonly Level DEBUG = new Level(-1000, "DEBUG", true);
		/// <summary>
		/// Used for errors after which the program cannot continue running.
		/// </summary>
		public static readonly Level FATAL = new Level(-500, "FATAL", true);
		/// <summary>
		/// Used for errors that nonetheless do not prevent the program from continuing.
		/// </summary>
		public static readonly Level ERROR = new Level(-250, "ERROR", true);
		/// <summary>
		/// Used to warn for things that may be out of the ordinary, but are otherwise not a problem.
		/// </summary>
		public static readonly Level WARN = new Level(-100, "WARN", true);
		/// <summary>
		/// Used for general program information/feedback.
		/// </summary>
		public static readonly Level INFO = new Level(0, "INFO", true);
		/// <summary>
		/// Used for logs that document program configuration events.
		/// </summary>
		public static readonly Level CONFIG = new Level(250, "CONFIG", true);
		/// <summary>
		/// Used for relatively fine logging. Not as fine as TRACE.
		/// </summary>
		public static readonly Level FINE = new Level(500, "FINE", true);
		/// <summary>
		/// Used for very fine information. E.G object construction, function calls, etc.
		/// </summary>
		public static readonly Level TRACE = new Level(1000, "TRACE", true);
		/// <summary>
		/// Enables all log messages.
		/// </summary>
		public static readonly Level ALL = new Level(int.MaxValue, "ALL", true);
		#endregion

		public string Name { get; }
		public int Value { get; }

		private Level(int value, string name, bool isDefault)
		{
			Value = value;
			Name = name;
			if (isDefault) DefaultLevels[name] = this;
		}

		/// <summary>
		/// Creates a new instance of <see cref="Level"/>.
		/// </summary>
		/// <param name="value">The logging level value. This must be a unique value.</param>
		/// <param name="name">The name of the logging level. This is case sensitive and must be unique.</param>
		/// <returns>The newly created <see cref="Level"/> instance.</returns>
		/// <exception cref="ArgumentException">When <paramref name="name"/> or <paramref name="value"/> are not unique.</exception>
		public Level(int value, string name) : this(value, name, false)
		{
			CustomLevels[name] = this;
		}

		/// <summary>
		/// Returns a string representing this object.
		/// </summary>
		public override string ToString() => Name;

		/// <summary>
		/// Returns a <see cref="Level"/> object with the same name.
		/// </summary>
		public static Level GetLevel(string name)
		{
			if (DefaultLevels.ContainsKey(name)) return DefaultLevels[name];
			if (CustomLevels.ContainsKey(name)) return CustomLevels[name];
			return null;
		}
	}
}