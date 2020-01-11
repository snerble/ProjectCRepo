using MySql.Data.MySqlClient;
using MySQL.Modeling;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

		[AutoIncrement]
		public int? Id { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public long Created { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
		public AccessLevel AccessLevel { get; set; } = AccessLevel.User;

		/// <summary>
		/// Returns a salted <see cref="SHA512"/> password hash using this <see cref="User"/>'s username and password.
		/// </summary>
		public string GetPasswordHash()
		{
			// Create a sha instance
			using var sha = SHA512.Create();
			// Compute the hash
			var passwordHash = sha.ComputeHash(Encoding.UTF8.GetBytes(Username + "#:#" + Password));
			// Convert the hash bytes to a hex string and return it
			return string.Concat(passwordHash.Select(x => x.ToString("x2")));
		}
	}
}
