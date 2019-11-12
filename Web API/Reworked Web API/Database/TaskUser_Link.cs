using MySQL.Modeling;

namespace API.Database
{
	[Table("tasks_users")]
	public sealed class TaskUser_Link : ItemAdapter
	{
		public int Task { get; set; }
		public int User { get; set; }
		public long Start { get; set; }
		public long? End { get; set; }
	}
}
