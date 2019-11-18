using MySQL.Modeling;
using System;

namespace API.Database
{
	[Table("tasks")]
	public sealed class Task : ItemAdapter
	{
		[AutoIncrement]
		public int? Id { get; set; }
		public int Group { get; set; }
		public int Creator { get; set; }
		public long Creation { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
		public string Title { get; set; }
		public string Description { get; set; }
		public sbyte Priority { get; set; }
	}
}
