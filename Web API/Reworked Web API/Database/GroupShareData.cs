using MySQL.Modeling;
using System;

namespace API.Database
{
	[Table("groups_sharing")]
	public sealed class GroupShareData : ItemAdapter
	{
		public int Group { get; set; }
		public string Code { get; set; }
		public long Created { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
		public int Expiration { get; set; } = 7 * 24 * 60 * 60;
	}
}
