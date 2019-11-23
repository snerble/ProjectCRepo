using API.Config;
using API.Database;
using API.HTTP;
using Config.Exceptions;
using Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

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
		public static AppDatabase Database;

		private static readonly List<Server> Servers = new List<Server>();
		private static Listener listener;

		private static BlockingCollection<HttpListenerContext> JSONQueue;
		private static BlockingCollection<HttpListenerContext> ResourceQueue;

		static void Main()
		{
			// Set window title
			var assembly = Assembly.GetExecutingAssembly().GetName();
			Console.Title = $"{assembly.Name} v{assembly.Version}";

			Log.Info(DEBUG ? "Starting server in DEBUG mode" : "Starting server");
			Log.Info("Loading configurations");
			try
			{
				Config = new AppConfig("config.json");
			}
			catch (ConfigException e)
			{
				Log.Fatal($"{e.GetType().Name}: {e.Message}", e, false);
				Terminate(14001);
			}
			catch (Exception e)
			{
				Log.Fatal($"Unexpected error: {e.GetType().Name}: " + e.Message, e, true);
				Terminate(1);
			}
			// Set logging level
			Log.Config($"Setting log level to '{Config["appSettings"]["logLevel"]}'");
			Log.LogLevel = Level.GetLevel(Config["appSettings"]["logLevel"].Value<string>());

			// Assign the reload event to OnConfigReload
			Config.Reload += OnConfigReload;

			// Create the HTTPListener
			Log.Config("Creating listener...");
			string[] addresses = Config["serverSettings"]["serverAddresses"].ToObject<string[]>();
			addresses = addresses.Length == 0 ? new string[] { GetLocalIP(), "localhost" } : addresses;
			listener = new Listener(addresses);
			Log.Info("Listening on: " + string.Join(", ", addresses));

			// Get custom queues
			JSONQueue = listener.GetCustomQueue(x =>
				x.Request.ContentType == "application/json"
				|| (x.Request.ContentType == "application/octet-stream"
					&& x.Request.Cookies["session"] != null)
			);
			ResourceQueue = listener.GetCustomQueue(x =>
				x.Request.AcceptTypes != null
				&& !x.Request.AcceptTypes.Contains("text/html")
			);

			// Call the rest of the setup
			Setup();

			// Start listening
			Log.Config("Starting listener...");
			listener.Start();

			// Create exiter thread that releases the exit mutex when enter is pressed
			new Thread(() =>
			{
				Console.ReadLine();
				ExitLock.Release();
			})
			{ Name = "Program Exiter" }.Start();

			// Wait until the exit mutex is released, then terminate
			ExitLock.Wait();
			Terminate(ExitCode);
		}

		/// <summary>
		/// Repeatable part of the program setup. Note that the threads and database must first be disposed or null.
		/// </summary>
		static void Setup()
		{
			#region Apply AppSettings
			dynamic appSettings = Config["appSettings"];

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
			Database = new AppDatabase();
			Log.Info($"Opened connection to '{Database.Connection.DataSource}'.");

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

			#region Setup Threads
			dynamic performance = Config["performance"];

			for (int i = 0; i < (int)performance.apiThreads; i++)
			{
				var server = new JsonServer(JSONQueue);
				Servers.Add(server);
				server.Start();
			}
			for (int i = 0; i < (int)performance.htmlThreads; i++)
			{
				var server = new HTMLServer(listener.Queue);
				Servers.Add(server);
				server.Start();
			}
			for (int i = 0; i < (int)performance.resourceThreads; i++)
			{
				var server = new ResourceServer(ResourceQueue);
				Servers.Add(server);
				server.Start();
			}
			#endregion
		}

		/// <summary>
		/// Starts a new thread that disposes of all other threads used by the program.
		/// </summary>
		private static void ClearThreads()
		{
			foreach (var server in Servers)
			{
				server.Interrupt();
				server.Join();
			}
			Servers.Clear();
		}

		/// <summary>
		/// Event that handles new config settings.
		/// </summary>
		private static void OnConfigReload(object sender, ReloadEventArgs e)
		{
			var changed = e.Diff.Changed;
			if (changed?["appSettings"]?["logLevel"] != null) // loglevel changed
			{
				Log.Config($"Setting log level to '{Config["appSettings"]["logLevel"]}'");
				Log.LogLevel = Level.GetLevel(Config["appSettings"]["logLevel"].Value<string>());
			}

			// Things that require a soft restart
			if (changed?["dbSettings"] != null || changed?["performance"] != null)
			{
				static void restarter()
				{
					Log.Info("RESTARTING SERVER");
					Log.Fine("Some values have been changed that require a soft restart.");
					ClearThreads();
					Database.Dispose();
					Setup();
					Log.Info("RESTART SUCCESSFUL");
				}
				new Thread(restarter) { Name = "Restarter" }.Start();
			}
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

		static int exitcode = 0;
		static readonly SemaphoreSlim ExitLock = new SemaphoreSlim(0, 1);
		/// <summary>
		/// Specifies the program exit code and terminates the program when set.
		/// </summary>
		public static int ExitCode
		{
			private get { return exitcode; }
			set
			{
				exitcode = value;
				ExitLock.Release();
			}
		}

		/// <summary>
		/// Ends the program with the specified exit code.
		/// </summary>
		static void Terminate(int exitCode = 0)
		{
			Log.Info("Terminating...");
			ClearThreads();
			Log.Dispose();
			Environment.Exit(exitCode);
		}
	}
}
