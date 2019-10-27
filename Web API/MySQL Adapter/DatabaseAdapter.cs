using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using MySQL.Modeling;

namespace MySQL
{
	/// <summary>
	/// Abstract class that must be implemented to set up a MySQL database.
	/// </summary>
	public abstract class DatabaseAdapter
	{
		/// <summary>
		/// The <see cref="MySqlConnection"/> instance used by this adapter.
		/// </summary>
		public abstract MySqlConnection Connection { get; }

		/// <summary>
		/// Returns all entries of the given data model in an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">A subclass of <see cref="ItemAdapter"/>.</typeparam>
		public IEnumerable<T> Select<T>() where T : ItemAdapter, new()
		{
			using var command = new MySqlCommand($"SELECT * FROM `{Utils.GetName<T>()}`", Connection);
			using var reader = command.ExecuteReader();

			// Get the properties of the model and sort them based on name
			var properties = Utils.GetColumns<T>().ToDictionary(x => Utils.GetColumnData(x).Name.ToLower());
			do
			{
				// Collect field names in a list
				var fields = new List<string>();
				for (int i = 0; i < reader.FieldCount; i++)
					fields.Add(reader.GetName(i).ToLower());
				// Loop through items
				while (reader.Read())
				{
					T outObj = new T();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						var property = properties[fields[i]];
						var type = property.PropertyType;
						var value = reader.GetValue(i);

						// Convert enum since database always returns string
						if (type.IsEnum) value = Enum.Parse(type, value.ToString());

						property.SetValue(outObj, value);
					}
					yield return outObj;
				}
			}
			while (reader.NextResult());
		}

		public long Insert<T>(T item) where T : ItemAdapter
		{
			var properties = Utils.GetColumns<T>().ToDictionary(x => Utils.GetColumnData(x));

			string sql = $"INSERT INTO `{Utils.GetName<T>()}` ({string.Join(", ", properties.Keys.Select(x => $"`{x.Name}`"))})" +
				$"VALUES ({string.Join(", ", properties.Keys.Select(x => $"@{x.Name}"))})";
			using var command = new MySqlCommand(sql, Connection);

			foreach (var keyValue in properties)
			{
				var parameter = new MySqlParameter(keyValue.Key.Name, keyValue.Value.GetValue(item) ?? DBNull.Value);
				command.Parameters.Add(parameter);
			}

			command.ExecuteNonQuery();
			return command.LastInsertedId;
		}
	}
}
