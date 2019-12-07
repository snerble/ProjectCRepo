using MySql.Data.MySqlClient;
using MySQL.Modeling;
using System;
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
		/// Returns a salted <see cref="SHA256"/> password hash using this <see cref="User"/>'s username and password.
		/// </summary>
		/// <returns></returns>
		public string GetSHA256PasswordHash()
		{
			// Create a sha instance
			using var sha256 = SHA256.Create();
			// Compute the hash
			var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(Username + "#:#" + Password));
			// Convert the hash bytes back to a string and return it
			return Encoding.UTF8.GetString(passwordHash);
		}
	}
}
