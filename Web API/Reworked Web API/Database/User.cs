using MySQL.Modeling;
using System;

namespace API.Database
{
	/// <summary>
	/// Contains the access levels that can be assigned to a <see cref="Database.User"/> instance.
	/// </summary>
	public enum AccessLevel
	{
		User = 1,
		Admin = 2
	}

	[Table("users")]
	public sealed class User : ItemAdapter
	{
		//[AutoIncrement][Column("id")]
		//public int Id { get; set; }

		//[Fulltext][Column("username", Length = 100)]
		//public string Username { get; set; }

		//[Column("password", Length = 128)]
		//public string Password { get; set; }

		//[Column("token")]
		//public int Token { get; set; }

		//[Column("accessLevel")]
		//public AccessLevel AccessLevel { get; set; }

		public int? Id { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public long Token { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
		public AccessLevel AccessLevel { get; set; } = AccessLevel.User;
	}
}
