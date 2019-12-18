using MySQL.Modeling;

namespace API.Database
{
	[Table("tasks_resources")]
	public sealed class TaskResource_Link : ItemAdapter
	{
		public int Task { get; set; }
		public int? Resource { get; set; }
	}
}
