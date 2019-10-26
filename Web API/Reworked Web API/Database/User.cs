using MySQL.Modeling;

namespace API.Database
{
	/// <summary>
	/// Contains the access levels that can be assigned to a <see cref="Database.User"/> instance.
	/// </summary>
	public enum AccessLevel
	{
		User,
		Admin
	}

	/// <summary>
	/// Represents the 'users' table in the database.
	/// </summary>
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
		public long Token { get; set; }
		public AccessLevel AccessLevel { get; set; }
	}

	[Table("groups")]
	public sealed class Group : ItemAdapter
	{
		public int? Id { get; set; }
		public int Creator { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
