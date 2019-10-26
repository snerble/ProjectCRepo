using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MySQL.Modeling
{
	/// <summary>
	/// Package-only class containing various utilities used in the Modeling namespace
	/// </summary>
	static class Utils
	{
		/// <summary>
		/// Simple dictionary that maps a C# type to a MySqlDbType. Used for automatic typing for queries.
		/// </summary>
		public static Dictionary<Type, MySqlDbType> TypeMap { get; } = new Dictionary<Type, MySqlDbType>
		{
			{typeof(bool), MySqlDbType.Bit },
			{typeof(sbyte), MySqlDbType.Byte },
			{typeof(short), MySqlDbType.Int16 },
			{typeof(int), MySqlDbType.Int32 },
			{typeof(long), MySqlDbType.Int64 },
			{typeof(byte), MySqlDbType.UByte },
			{typeof(ushort), MySqlDbType.UInt16 },
			{typeof(uint), MySqlDbType.UInt32 },
			{typeof(ulong), MySqlDbType.UInt64 },
			{typeof(string), MySqlDbType.Text },
			{typeof(byte[]), MySqlDbType.Blob }
		};

		/// <summary>
		/// Returns the <see cref="ColumnAttribute"/> of the specified property, or generates a new
		/// <see cref="ColumnAttribute"/> based on the property's name and type.
		/// </summary>
		public static ColumnAttribute GetColumnData(PropertyInfo property)
		{
			var attribute = property.GetCustomAttribute<ColumnAttribute>() ?? new ColumnAttribute(property.Name);
			if (attribute.Type == null)
			{
				if (!TypeMap.TryGetValue(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType, out MySqlDbType type))
				{
					if (property.PropertyType.IsEnum) attribute.Type = MySqlDbType.Enum;
					else throw new OperationCanceledException($"Could not parse type '{property.PropertyType.Name}'.");
				}
				else attribute.Type = type;
			}
			return attribute;
		}

		/// <summary>
		/// Returns the table name of the specified data model, or the data model type name if no name is specified.
		/// </summary>
		public static string GetName<T>() where T : ItemAdapter
			=> typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;

		/// <summary>
		/// Returns an <see cref="IEnumerable{T}"/> of <see cref="PropertyInfo"/>s of the specified data model.
		/// </summary>
		public static IEnumerable<PropertyInfo> GetColumns<T>() where T : ItemAdapter
		{
			// yield all properties
			foreach (var property in typeof(T).GetProperties())
				yield return property;
		}
		/// <summary>
		/// Returns an <see cref="IEnumerable{T}"/> containing <see cref="PropertyInfo"/> instances
		/// of the specified type extending <see cref="ItemAdapter"/>.
		/// </summary>
		/// <param name="t">The type of a class extending <see cref="ItemAdapter"/>.</param>
		public static IEnumerable<PropertyInfo> GetColumns(Type t)
		{
			// throw exception if the type is not a subclass of item adapter
			if (!typeof(ItemAdapter).IsAssignableFrom(t))
				throw new ArgumentException($"The specified type is not assignable from '{typeof(ItemAdapter).Name}'.");
			// yield all properties
			foreach (var property in t.GetProperties())
				yield return property;
		}
	}
}
