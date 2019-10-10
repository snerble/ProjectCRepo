using System;
using Logging;
using API.Config;
using Config.Exceptions;

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
		public static AppConfig Config;

		static void Main()
		{
			Log.Info("Starting server");

			Log.Info("Loading configurations");
			try { Config = new AppConfig("config.json"); }
			catch(ConfigException e)
			{
				Log.Fatal($"{e.GetType().Name}: {e.Message}", e, false);
				Terminate(14001);
			}
			catch(Exception e)
			{
				Log.Fatal($"Unexpected error: {e.GetType().Name}: " + e.Message, e, true);
				Terminate(1);
			}

			Terminate(0);
		}

		static void Terminate(int exitCode = -1)
		{
			Log.Info("Terminating...");
			Environment.Exit(exitCode);
		}
	}
}
