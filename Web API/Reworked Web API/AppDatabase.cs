using MySql.Data.MySqlClient;
using MySQL;
using Newtonsoft.Json.Linq;
using System;

namespace API.Database
{
	/// <summary>
	/// A <see cref="DatabaseAdapter"/> subclass representing this project's database. This class cannot be inherited.
	/// </summary>
	public sealed class AppDatabase : DatabaseAdapter
	{
		/// <summary>
		/// Gets a <see cref="MySqlConnection"/> instance made from the data in <see cref="Program.Config"/>.
		/// </summary>
		public override MySqlConnection Connection { get; } = GetConnection();

		/// <summary>
		/// Initializes a new instance of <see cref="AppDatabase"/>.
		/// </summary>
		public AppDatabase() : base()
		{
			Connection.Open();
		}

		/// <summary>
		/// Returns a <see cref="MySqlConnection"/> created from the data in <see cref="Program.Config"/>.
		/// </summary>
		private static MySqlConnection GetConnection()
		{
			// Get the db settings from the config.
			var DBSettings = Program.Config["dbSettings"];

			// Convert serverAddress to 
			var address = new Uri(DBSettings["serverAddress"].Value<string>());
			var builder = new MySqlConnectionStringBuilder
			{
				AllowBatch = true,
				TreatTinyAsBoolean = true,
				// Set config settings
				Server = address.Host,
				Port = address.Port == -1 ? 3306u : (uint)address.Port,
				Database = DBSettings["database"].Value<string>(),
				UserID = DBSettings["username"].Value<string>(),
				Password = DBSettings["password"].Value<string>(),
				ConnectionTimeout = DBSettings["timeout"].Value<uint>(),
				PersistSecurityInfo = DBSettings["persistLogin"].Value<bool>(),
				TableCaching = DBSettings["caching"].Value<bool>()
			};

			return new MySqlConnection(builder.GetConnectionString(true));
		}
	}
}
