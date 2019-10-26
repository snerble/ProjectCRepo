using MySQL.Modeling;

namespace API.Database
{
	[Table("tasks")]
	public sealed class Task : ItemAdapter
	{
		public int? Id { get; set; }
		public int Group { get; set; }
		public int Creator { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public byte Priority { get; set; }
	}
}
