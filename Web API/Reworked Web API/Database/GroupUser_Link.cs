using MySQL.Modeling;

namespace API.Database
{
	public enum Rank
	{
		User = 1,
		Moderator = 2,
		Admin = 3
	}

	[Table("groups_users")]
	public sealed class GroupUser_Link : ItemAdapter
	{
		public int Group { get; set; }
		public int User { get; set; }
		public Rank Rank { get; set; } = Rank.User;
	}
}
