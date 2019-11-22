using MySQL.Modeling;
using System;

namespace API.Database
{
	[Table("sessions")]
	public sealed class Session : ItemAdapter
	{
		/// <summary>
		/// Unique 128-bits GUID for the session code.
		/// </summary>
		[Primary]
		public string Id { get; set; }
		/// <summary>
		/// 256-bits <see cref="System.Security.Cryptography.Aes"/> key.
		/// </summary>
		public byte[] Key { get; set; }
		/// <summary>
		/// Unix timestamp for when a session expires, represented in total seconds.
		/// </summary>
		public long Expires { get; set; } = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
	}
}
