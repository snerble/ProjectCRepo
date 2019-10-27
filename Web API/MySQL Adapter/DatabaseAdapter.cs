using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
			var properties = Utils.GetAllColumns<T>().ToDictionary(x => Utils.GetColumnData(x).Name.ToLower());
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

		/// <summary>
		/// Inserts an object into this database.
		/// </summary>
		/// <typeparam name="T">A type extending <see cref="ItemAdapter"/>.</typeparam>
		/// <param name="item">The item to insert into the database.</param>
		/// <param name="discardIndex">If true, will skip getting and assigning the last insert id to the item.</param>
		/// <returns>If the table has an auto increment column, the id of the inserted item. Otherwise -1.</returns>
		public long Insert<T>(T item, bool discardIndex = false) where T : ItemAdapter
			=> Insert(new T[] { item }, discardIndex);
		/// <summary>
		/// Inserts the collection of objects into this database.
		/// </summary>
		/// <typeparam name="T">A type extending <see cref="ItemAdapter"/>.</typeparam>
		/// <param name="items">The objects to insert into the database.</param>
		/// <param name="discardIndex">If true, will skip getting and assigning the last insert id to the items.</param>
		/// <returns>If the table has an auto increment column, the id of the inserted item. Otherwise -1.</returns>
		public long Insert<T>(ICollection<T> items, bool discardIndex = false) where T : ItemAdapter
		{
			// Get all columns/properties of the model
			var columns = Utils.GetAllColumns<T>().ToDictionary(x => Utils.GetColumnData(x));

			// Func that creates a parameter string for one item
			string getValueString(T item) => '(' + string.Join(", ", columns.Keys.Select(x => $"@{x.Name}_{item.GetHashCode()}")) + ')';

			// Generate query for itemadapter and create a set of parameters unique to every item using their hashcode
			string sql = $"INSERT INTO `{Utils.GetName<T>()}` ({string.Join(", ", columns.Keys.Select(x => $"`{x.Name}`"))})" +
						"VALUES" + string.Join(",", items.Select(x => getValueString(x))) + ';';

			// Create command
			using var command = new MySqlCommand(sql, Connection);
			// Create parameter objects for every value and item
			foreach (var item in items)
				foreach (var keyValue in columns)
					command.Parameters.Add(new MySqlParameter(keyValue.Key.Name + '_' + item.GetHashCode(), keyValue.Value.GetValue(item) ?? DBNull.Value));

			// Run command and get the scalar
			command.ExecuteNonQuery();
			if (discardIndex) return -1;
			var scalar = command.LastInsertedId;
			if (scalar == -1) return -1; // skip other things if it is -1

			// Get the auto increment property/column
			var autoIncrement = Utils.GetColumns<AutoIncrementAttribute>(columns.Values).FirstOrDefault();
			if (autoIncrement != null)
			{
				var autoIncType = Nullable.GetUnderlyingType(autoIncrement.PropertyType) ?? autoIncrement.PropertyType;
				long i = 0;
				// Assign the insert index to every item (if their auto increment value is null)
				foreach (var item in items)
				{
					// Set the autoIncrement column to the existing value, or scalar + i if it is null
					object oldValue = autoIncrement.GetValue(item);
					if (oldValue != null) // skip if the auto increment was already assigned
					{
						var oldValueLong = Convert.ToInt64(oldValue);
						if (oldValueLong > scalar + i)
							i = scalar - oldValueLong;
						continue;
					}
					// Convert the scalar to the type of the auto increment property and set it
					autoIncrement.SetValue(item, Convert.ChangeType(scalar + i, autoIncType));
					i++;
				}
			}
			return scalar;
		}
	}
}
