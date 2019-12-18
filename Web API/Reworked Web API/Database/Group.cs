using MySQL.Modeling;
using System;

namespace API.Database
{
	[Table("groups")]
	public sealed class Group : ItemAdapter
	{
		[AutoIncrement]
		public int? Id { get; set; }
		public int Creator { get; set; }
		public string Name { get; set; }
		public long Created { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
		public string Description { get; set; }
	}
}
