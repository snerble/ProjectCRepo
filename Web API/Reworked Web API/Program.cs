using API.Config;
using API.HTTP;
using Config.Exceptions;
using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		private static readonly List<Server> Servers = new List<Server>();

		static void Main()
		{
			Log.Info(DEBUG ? "Starting server in DEBUG mode" : "Starting server");
			Log.Info("Loading configurations");
			try
			{
				Config = new AppConfig("config.json");
			}
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

			#region Apply AppSettings
			dynamic appSettings = Config["appSettings"];

			Log.Config($"Setting log level to '{appSettings.logLevel}'");
			Log.LogLevel = Level.GetLevel((string)appSettings.logLevel);

			// Create log files in release mode only
			if (!DEBUG)
			{
				Log.Config("Creating log file");
				string log = appSettings.logDir;
				// Create the directory if it doesn't exist
				try
				{
					if (!Directory.Exists(log)) Directory.CreateDirectory(log);
				}
				catch (Exception e)
				{
					Log.Fatal($"{e.GetType().Name}: {e.Message}", e, false);
					Terminate(14001);
				}
				log += "/latest.log";
				// Delete the file if it already exists
				if (File.Exists(log)) File.Delete(log);
				Log.OutputStreams.Add(File.CreateText(log));
			}
			#endregion

			#region Setup Server
			dynamic serverSettings = Config["serverSettings"];
			dynamic performance = Config["performance"];

			Log.Config("Creating listener...");
			var listener = new Listener(serverSettings.serverAddresses.ToObject<string[]>());

			// Get custom queues
			var JSONQueue = listener.GetCustomQueue(x => x.Request.ContentType == "application/json");
			var HTMLQueue = listener.GetCustomQueue(x => x.Request.AcceptTypes.Contains("text/html"));

			for (int i = 0; i < (int)performance.apiThreads; i++)
			{
				var server = new JsonServer(JSONQueue);
				Servers.Add(server);
				server.Start();
			}
			for (int i = 0; i < (int)performance.htmlThreads; i++)
			{
				var server = new HTMLServer(HTMLQueue);
				Servers.Add(server);
				server.Start();
			}
			for (int i = 0; i < (int)performance.resourceThreads; i++)
			{
				var server = new ResourceServer(listener.Queue);
				Servers.Add(server);
				server.Start();
			}

			listener.Start();
			#endregion

			Console.ReadKey();
			Terminate(0);
		}

		static void Terminate(int exitCode = -1)
		{
			foreach (var server in Servers)
				server.Interrupt();
			foreach (var server in Servers)
				server.Join();
			Log.Info("Terminating...");
			Log.Dispose();
			Environment.Exit(exitCode);
		}
	}
}
