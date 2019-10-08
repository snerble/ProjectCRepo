using System;
using Logging;
using API.Config;

namespace API
{
	class Program
	{
		/// <summary>
		/// True when the program is running in debug mode. False otherwise.
		/// </summary>
		public static bool DEBUG { get; } =
#if DEBUG
			true
#else
			false
#endif
			;

		public static Logger Log = new Logger(Level.ALL, Console.Out);
		public static AppConfig Config = new AppConfig("config.json");

		static void Main(string[] args)
		{
			Log.Info("Starting server");

			Log.Info("Terminating...");
		}
	}
}
