using MySQL.Modeling;

namespace API.Database
{
	public enum Rank
	{
		User,
		Moderator,
		Admin
	}

	[Table("groups_users")]
	public sealed class GroupUser_Link : ItemAdapter
	{
		public int Group { get; set; }
		public int User { get; set; }
		public Rank Rank { get; set; }
	}
}
