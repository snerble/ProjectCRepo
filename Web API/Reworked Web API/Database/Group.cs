using MySQL.Modeling;

namespace API.Database
{
	[Table("groups")]
	public sealed class Group : ItemAdapter
	{
		[AutoIncrement]
		public int? Id { get; set; }
		public int Creator { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
