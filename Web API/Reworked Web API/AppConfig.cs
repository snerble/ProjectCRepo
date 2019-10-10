using Config;
using Config.Exceptions;
using Logging;
using Newtonsoft.Json.Linq;

namespace API.Config
{
	/// <summary>
	/// Custom class implementing <see cref="ConfigBase"/>.
	/// Provides the configuration features nescessary for the Web API project.
	/// </summary>
	sealed class AppConfig : ConfigBase
	{
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
			// TODO find a way to collect all exceptions and put them in one big exception so all problems can be displayed at once.
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
			TryAddItem(Content["appSettings"], "logLevel", Program.DEBUG ? Level.DEBUG.Name : Level.INFO.Name);
			TryAddItem(Content["appSettings"], "logDir", "Logs");

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
		}

		// Example of a property that refers directly to a config setting. The setter is optional.
		/*public string ServerAddress
		{
			get { return Program.Config["dbSettings"]["serverAddress"].Value<string>(); }
			set { Program.Config["dbSettings"]["serverAddress"] = value; Program.Config.Save(); }
		}*/
	}
}
