using Config;
using Config.Exceptions;
using Logging;
using System.IO;
using Newtonsoft.Json.Linq;
using System;

namespace API.Config
{
	/// <summary>
	/// Custom class implementing <see cref="ConfigBase"/>.
	/// Provides the configuration features nescessary for the Web API project.
	/// </summary>
	sealed class AppConfig : ConfigBase
	{
		/// <summary>
		/// Gets whether this <see cref="AppConfig"/> instance reloads it's content when it's file is changed.
		/// </summary>
		protected override bool AutoReload { get; } = true;

		public string HTMLSourceDir
		{
			get
			{
				return Path.GetFullPath(Path.Combine(
					Directory.GetCurrentDirectory(),
					Program.Config["serverSettings"]["htmlSourceDir"].ToObject<string>()
				));
			}
		}
		public string ResourceDir
		{
			get
			{
				return Path.GetFullPath(Path.Combine(
					Directory.GetCurrentDirectory(),
					Program.Config["serverSettings"]["resourceDir"].ToObject<string>()
				));
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="AppConfig"/>.
		/// </summary>
		/// <param name="file">The path to the config JSON file. The file extension must be '.json'.</param>
		public AppConfig(string file) : base(file) { }

		/// <summary>
		/// Sets up the JSON file associated with this <see cref="AppConfig"/>.
		/// </summary>
		/// <remarks>
		/// To validate values (e.g. check if an int is within a certain range), please put this
		/// code in <see cref="Verify"/>.
		/// </remarks>
		protected override void Setup()
		{
			// Build database settings
			TryAddItem(Content, "dbSettings", new JObject());
			TryAddItem(Content["dbSettings"], "serverAddress", (string)null);
			TryAddItem(Content["dbSettings"], "database", (string)null);
			TryAddItem(Content["dbSettings"], "username", (string)null);
			TryAddItem(Content["dbSettings"], "password", (string)null);
			TryAddItem(Content["dbSettings"], "timeout", -1);
			TryAddItem(Content["dbSettings"], "persistLogin", true);
			TryAddItem(Content["dbSettings"], "caching", true);

			// Build performance settings
			TryAddItem(Content, "performance", new JObject());
			TryAddItem(Content["performance"], "apiThreads", 5);
			TryAddItem(Content["performance"], "htmlThreads", 5);
			TryAddItem(Content["performance"], "resourceThreads", 5);

			// Build application settings
			TryAddItem(Content, "appSettings", new JObject());
			TryAddItem(Content["appSettings"], "logLevel", Program.DEBUG ? Level.ALL.Name : Level.INFO.Name);
			TryAddItem(Content["appSettings"], "logDir", "Logs");

			// Build server settings
			TryAddItem(Content, "serverSettings", new JObject());
			TryAddItem(Content["serverSettings"], "partialDataLimit", 1024UL * 1024UL);
			TryAddItem(Content["serverSettings"], "htmlSourceDir", "../../../../Web API/HTML");
			TryAddItem(Content["serverSettings"], "resourceDir", "../../../../Web API/HTML");
			TryAddItem(Content["serverSettings"], "serverAddresses", new JArray() { "localhost" });

			Save();
			Verify();
		}

		/// <summary>
		/// This function contains the code that validates all arguments. Please don't put validation code in <see cref="Setup"/>.
		/// </summary>
		private void Verify()
		{
			// Verify database settings
			if (Content["dbSettings"]["serverAddress"].Value<string>() == null) throw new ConfigException("Value 'dbSettings['serverAddress']' may not be null.");
			if (Content["dbSettings"]["database"].Value<string>() == null) throw new ConfigException("Value 'dbSettings['database']' may not be null.");
			if (Content["dbSettings"]["username"].Value<string>() == null) throw new ConfigException("Value 'dbSettings['username']' may not be null.");
			if (Content["dbSettings"]["password"].Value<string>() == null) throw new ConfigException("Value 'dbSettings['password']' may not be null.");

			// Verify performance settings
			if (Content["performance"]["apiThreads"].Value<int>() < 1) throw new ConfigException("Value 'performance['apiThreads']' may not be less than 1.");
			if (Content["performance"]["htmlThreads"].Value<int>() < 1) throw new ConfigException("Value 'performance['htmlThreads']' may not be less than 1.");
			if (Content["performance"]["resourceThreads"].Value<int>() < 1) throw new ConfigException("Value 'performance['resourceThreads']' may not be less than 1.");
			
			// Verify application settings
			if (Level.GetLevel(Content["appSettings"]["logLevel"].Value<string>()) == null)
				throw new ConfigException("Invalid log level specified at 'appSettings['logLevel']'.");

			// Verify server settings
			if (!Directory.Exists(Content["serverSettings"]["htmlSourceDir"].Value<string>()))
				throw new ConfigException("No such directory specified at 'serverSettings['htmlSourceDir']'.");
			if (!Directory.Exists(Content["serverSettings"]["resourceDir"].Value<string>()))
				throw new ConfigException("No such directory specified at 'serverSettings['resourceDir']'.");
		}

		/// <summary>
		/// The function that is called when the config file has been edited by another process.
		/// </summary>
		/// <param name="newContent">The content of the new config.</param>
		/// <remarks>
		/// The new content is raw and may not satisfy the requirements of <see cref="Setup"/>.
		/// </remarks>
		protected override void OnReload(JObject newContent)
		{
			JObject oldContent = Content;
			try
			{
				Content = newContent;
				Setup();
			}
			catch (Exception e)
			{
				Program.Log.Error($"Reload failed: {e.Message}", e, false);
				Program.Log.Error($"Restoring previous config...");
				Content = oldContent;
				Save();
				return;
			}
			Program.Log.Info("Reloaded config.");
		}

		/// <summary>
		/// Updates the current config JObject with the specified JObject.
		/// </summary>
		/// <param name="newContent">A new <see cref="JObject"/> to use for this config.</param>
		/// <exception cref="ConfigException">Thrown when <paramref name="newContent"/> fails the config validation.</exception>
		public void Update(JObject newContent)
		{
			JObject oldContent = Content;
			try
			{
				Content = newContent;
				Setup();
				Save();
			}
			catch (Exception e)
			{
				Program.Log.Error($"Update failed: {e.Message}", e, false);
				Program.Log.Error($"Restoring previous config...");
				Content = oldContent;
				return;
			}
			Program.Log.Info("Updated config.");
		}

		// Example of a property that refers directly to a config setting. The setter is optional.
		/*public string ServerAddress
		{
			get { return Program.Config["dbSettings"]["serverAddress"].Value<string>(); }
			set { Program.Config["dbSettings"]["serverAddress"] = value; Program.Config.Save(); }
		}*/
	}
}
