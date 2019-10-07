using Config;
using Newtonsoft.Json.Linq;

namespace API
{
	class Config : ConfigBase
	{
		public Config(string file) : base(file) { }

		protected override void Setup()
		{
			// Build database settings
			TryAddItem(Content, "dbSettings", new JObject());
			TryAddItem((JObject)Content["dbSettings"], "dbHostAddress", (string)null);
			TryAddItem((JObject)Content["dbSettings"], "database", (string)null);
			TryAddItem((JObject)Content["dbSettings"], "username", (string)null);
			TryAddItem((JObject)Content["dbSettings"], "password", (string)null);
			TryAddItem((JObject)Content["dbSettings"], "timeout",	-1);
			TryAddItem((JObject)Content["dbSettings"], "persistLogin", true);
			TryAddItem((JObject)Content["dbSettings"], "caching", true);

			// Build performance settings
			TryAddItem(Content, "performance", new JObject());
			TryAddItem((JObject)Content["performance"], "apiThreads", 5);
			TryAddItem((JObject)Content["performance"], "htmlThreads", 5);
			TryAddItem((JObject)Content["performance"], "resourceThreads", 5);

			Save();
			Program.Log.Info(Content.ToString());
		}
	}
}
