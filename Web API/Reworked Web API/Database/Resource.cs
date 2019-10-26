using MySQL.Modeling;

namespace API.Database
{
	[Table("resources")]
	public sealed class Resource : ItemAdapter
	{
		public int? Id { get; set; }
		public string Filename { get; set; }
		public byte[] Data { get; set; }
		public string Hash { get; set; }
	}
}
