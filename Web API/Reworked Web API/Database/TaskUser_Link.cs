using MySQL.Modeling;
using System;

namespace API.Database
{
	[Table("tasks_users")]
	public sealed class TaskUser_Link : ItemAdapter
	{
		public int Task { get; set; }
		public int User { get; set; }
		public long Start { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
		public long? End { get; set; }
	}
}
