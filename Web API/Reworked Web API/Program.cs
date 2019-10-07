using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logging;

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
		public static Config Settings;

		static void Main(string[] args)
		{
			Log.Info("Starting server");

			Log.Info("Loading configurations");
			Settings = new Config("config.json");

			Log.Info("Terminating...");
		}
	}
}
