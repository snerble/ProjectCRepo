using Config;
using System;
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

		protected override void Setup()
		{
			// Build/check database settings
			TryAddItem(Content, "dbSettings", new JObject());
			TryAddItem((JObject)Content["dbSettings"], "serverAddress", (string)null);
			TryAddItem((JObject)Content["dbSettings"], "database", (string)null);
			TryAddItem((JObject)Content["dbSettings"], "username", (string)null);
			TryAddItem((JObject)Content["dbSettings"], "password", (string)null);
			TryAddItem((JObject)Content["dbSettings"], "timeout",	-1);
			TryAddItem((JObject)Content["dbSettings"], "persistLogin", true);
			TryAddItem((JObject)Content["dbSettings"], "caching", true);

			// Build/check performance settings
			TryAddItem(Content, "performance", new JObject());
			TryAddItem((JObject)Content["performance"], "apiThreads", 5);
			TryAddItem((JObject)Content["performance"], "htmlThreads", 5);
			TryAddItem((JObject)Content["performance"], "resourceThreads", 5);

			Save();
		}

		// Example of a property that refers directly to a config setting. The setter is optional.
		/*public string ServerAddress
		{
			get { return Program.Config["dbSettings"]["serverAddress"].Value<string>(); }
			set { Program.Config["dbSettings"]["serverAddress"] = value; Program.Config.Save(); }
		}*/
	}
}
