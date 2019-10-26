using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MySQL.Modeling
{
	/// <summary>
	/// Abstract class for modeling a MySQL table item into an object.
	/// </summary>
	/// <remarks>
	/// Subclasses of <see cref="ItemAdapter"/> should specify a name with the <see cref="TableAttribute"/> attribute.
	/// Otherwise, the class name will be used.
	/// </remarks>
	public abstract class ItemAdapter
	{
		/// <summary>
		/// Returns a string representation of this <see cref="ItemAdapter"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return GetType().Name + "(" + string.Join(", ", Utils.GetColumns(GetType()).Select(x => $"{Utils.GetColumnData(x).Name}: {x.GetValue(this) ?? "NULL"}")) + ")";
		}
	}
}
