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
		public static Config Settings = new Config()

		public static Logger Log = new Logger(Level.ALL, Console.Out);

		static void Main(string[] args)
		{
			Log.Info("Starting server");



			Log.Info("Terminating...");
		}
	}
}
