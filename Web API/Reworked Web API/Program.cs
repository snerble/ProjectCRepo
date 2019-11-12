using API.Config;
using API.Database;
using API.HTTP;
using Config.Exceptions;
using Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

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
		private static Listener listener;
		private static AppDatabase database;

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

			Log.Config("Creating database connection...");
			database = new AppDatabase();
			Log.Info($"Opened connection to '{database.Connection.DataSource}'.");

			while (true)
			{
				var users = database.Select<User>("accessLevel = 1 and LOWER(username) LIKE '%test%' LIMIT 100").ToList();
				if (users.Count == 0) break;
				database.Delete(users);
			}

			Terminate();

			try
			{
				// Compile razor .cshtml templates
				Templates.CompileAll();
			}
			catch (Exception e)
			{
				// Print any error and terminate
				Log.Fatal("Template compilation failed:");
				Log.Fatal($"{e.GetType().Name}: {e.Message}", e);
				Terminate(1);
			}

			#region Setup Server
			dynamic serverSettings = Config["serverSettings"];
			dynamic performance = Config["performance"];

			Log.Config("Creating listener...");
			string[] addresses = serverSettings.serverAddresses.ToObject<string[]>();
			addresses = addresses.Length == 0 ? new string[] { GetLocalIP(), "localhost" } : addresses;
			listener = new Listener(addresses);
			Log.Info("Listening on: " + string.Join(", ", addresses));

			// Get custom queues
			var JSONQueue = listener.GetCustomQueue(x => x.Request.ContentType == "application/json");
			var HTMLQueue = listener.GetCustomQueue(x => x.Request.AcceptTypes != null && x.Request.AcceptTypes.Contains("text/html"));

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

			Log.Config("Starting listener...");
			listener.Start();
			#endregion

			Console.ReadLine();
			Terminate(0);
		}

		/// <summary>
		/// Returns the IP address of this device.
		/// </summary>
		static string GetLocalIP()
		{
			// Open a socket and connect to google's DNS
			using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
			socket.Connect("8.8.8.8", 65530);
			// Get the local endpoint and return it's address
			IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
			return endPoint.Address.ToString();
		}

		/// <summary>
		/// Ends the program with the specified exit code.
		/// </summary>
		static void Terminate(int exitCode = 0)
		{
			Log.Info("Terminating...");
			listener?.Stop();
			foreach (var server in Servers)
			{
				server.Interrupt();
				server.Join();
			}
			Log.Dispose();
			Environment.Exit(exitCode);
		}
	}
}
