using API.Config;
using Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using MySQL.Modeling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace API.Database.Tests
{
	/// <summary>
	/// A class containing unit tests for all <see cref="ItemAdapter"/> subclasses used by the application.
	/// <para>
	/// The unit tests focus mainly on the integrity of the models; Every model must be able to be selected, inserted,
	/// deleted and updated from the database. This ensures that no model can inherently cause unexpected exceptions.
	/// </para>
	/// <para>
	/// Additionally, there are unit tests for custom methods and constraints for a select few models.
	/// </para>
	/// </summary>
	[TestClass()]
	public class ModelTests
	{
		/// <summary>
		/// Gets the <see cref="Logger"/> used to write detailed messages.
		/// <para>Equivalent to calling <see cref="Program.Log"/>.</para>
		/// </summary>
		public static Logger Log => Program.Log;

		/// <summary>
		/// Gets a <see cref="AppDatabase"/> instance for use in the unit tests.
		/// <para>Equivalent to calling <see cref="Utils.GetDatabase"/>.</para>
		/// </summary>
		public static AppDatabase Database => Utils.GetDatabase();
		/// <summary>
		/// Gets the <see cref="MySqlTransaction"/> instance that is used for all unit tests.
		/// </summary>
		public MySqlTransaction Transaction { get; private set; }

		/// <summary>
		/// Gets a small text sample containing unicode characters for use in unit tests.
		/// </summary>
		public static string UnicodeSample { get; } = "გთხოვთ ახლავე გაიაროთ";
		/// <summary>
		/// Gets a small text sample containing Emojis for use in unit tests.
		/// </summary>
		public static string EmojiSample { get; } = "🤔🤔🤔🤔🤔🤔🤔🤔🤔🤔";


		/// <summary>
		/// Initializes resources used across all unit tests.
		/// </summary>
		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			// Modify the logger
			Log.OutputStreams.Clear();
			Log.Format = "{asctime:HH:mm:ss} {className}.{funcName,-10} {levelname,6}: {message}";
			// Load the config
			Program.Config = new AppConfig("config.json");
		}
		/// <summary>
		/// Disposes of all resources used across all unit tests.
		/// </summary>
		[ClassCleanup]
		public static void ClassCleanup()
		{
			// Dispose the database through the Utils class
			Utils.DisposeDatabases();
			// Dispose the logger
			Log.Dispose();
		}

		/// <summary>
		/// Initializes all resources used by the unit tests.
		/// </summary>
		[TestInitialize()]
		public void Initialize()
		{
			// Create a transaction to prevent the changes from being applied
			Transaction = Database.Connection.BeginTransaction();
		}
		/// <summary>
		/// Disposes of all resources used by the unit tests.
		/// </summary>
		[TestCleanup()]
		public void Cleanup()
		{
			// Revert changes
			Transaction?.Rollback();
			// Dispose the transaction
			Transaction?.Dispose();
		}

		[TestMethod]
		[Priority(int.MaxValue)]
		[Description("Pings the database to measure and log the latency.")]
		public void Ping()
		{
			var timer = new Stopwatch();
			timer.Start();
			Database.Connection.Ping();
			Log.Info("Ping: " + Utils.FormatTimer(timer));
		}

		#region Sample getters
		/// <summary>
		/// Returns an array of <see cref="User"/> instances with varying data.
		/// </summary>
		private User[] GetSampleUsers()
		{
			// Create an array of users
			var users = new User[]
			{
				new User() { Username = "UnitTest_User", Password = "UnitTest_SamplePassword" },
				new User() { Username = "UnitTest_User_Admin", Password = "UnitTest_SamplePassword", AccessLevel = AccessLevel.Admin },
				new User() { Username = "UnitTest_User_Unicode_" + UnicodeSample, Password = UnicodeSample },
				new User() { Username = "UnitTest_User_Emoji_" + EmojiSample, Password = EmojiSample },
			};
			// Hash all passwords
			foreach (var user in users)
				user.Password = user.GetPasswordHash();
			return users;
		}
		/// <summary>
		/// Returns an array of <see cref="Group"/> instances with varying data.
		/// </summary>
		private Group[] GetSampleGroups()
		{
			// Insert a user to satisfy the Creator foreign key and get it's new id, if it isn't already set
			if (ForeignUserId == -1)
				ForeignUserId = (int)Database.Insert(new User() { Username = "UnitTest_GroupCreator" });

			// Return a new array of groups
			return new Group[]
			{
				new Group() { Creator = ForeignUserId, Name = "UnitTest_Group", Description = "UnitTest_SampleDescription" },
				new Group() { Creator = ForeignUserId, Name = "UnitTest_Group_NullDescription", Description = null },
				new Group() { Creator = ForeignUserId, Name = "UnitTest_Group_Unicode_" + UnicodeSample, Description = UnicodeSample },
				new Group() { Creator = ForeignUserId, Name = "UnitTest_Group_Emoji_" + EmojiSample, Description = EmojiSample },
			};
		}
		#endregion

		/// <summary>
		/// Gets or sets the id of the <see cref="User"/> that is used to satisfy all foreign keys
		/// to the users table.
		/// </summary>
		private int ForeignUserId { get; set; } = -1;
		/// <summary>
		/// Gets or sets the id of the <see cref="Group"/> that is used to satisfy all foreign keys
		/// to the groups table.
		/// </summary>
		private int ForeignGroupId { get; set; } = -1;
		/// <summary>
		/// Gets or sets the id of the <see cref="Task"/> that is used to satisfy all foreign keys
		/// to the tasks table.
		/// </summary>
		private int ForeignTaskId { get; set; } = -1;

		/// <summary>
		/// Returns an array of <see cref="ItemAdapter"/> instances of the specified type, whose values
		/// mustn't cause issues for it's unit tests.
		/// </summary>
		/// <param name="type">A <see cref="Type"/> object extending <see cref="ItemAdapter"/>.</param>
		private ItemAdapter[] GetSample(Type type)
		{
			// Assert that the type is an ItemAdapter
			Assert.IsTrue(type.IsSubclassOf(typeof(ItemAdapter)), $"The type '{type.FullName}' does not extend '{typeof(ItemAdapter).FullName}'.");

			// Return a sample based on the type name
			switch (type.Name)
			{
				case nameof(User):
					return new[] {
						new User() { Username = "UnitTest_User", Password = "UnitTest_SamplePassword" },
						new User() { Username = "UnitTest_User_Admin", Password = "UnitTest_SamplePassword", AccessLevel = AccessLevel.Admin },
						new User() { Username = "UnitTest_User_Unicode_" + UnicodeSample, Password = UnicodeSample },
						new User() { Username = "UnitTest_User_Emoji_" + EmojiSample, Password = EmojiSample },
					}.Select(x =>
					{ // Hash all passwords
						x.Password = x.GetPasswordHash();
						return x;
					}).ToArray();
				case nameof(Group):
					// Create a new user to satisfy the foreign key constraint
					if (ForeignUserId == -1) ForeignUserId = (int)Database.Insert(new User() { Username = "UnitTest_GroupCreator" });
					return new[] {
						new Group() { Name = "UnitTest_Group", Description = "UnitTest_SampleDescription" },
						new Group() { Name = "UnitTest_Group_NullDescription", Description = null },
						new Group() { Name = "UnitTest_Group_Unicode_" + UnicodeSample, Description = UnicodeSample },
						new Group() { Name = "UnitTest_Group_Emoji_" + EmojiSample, Description = EmojiSample },
					}.Select(x =>
					{ // Set foreign keys
						x.Creator = ForeignUserId;
						return x;
					}).ToArray();
				case nameof(Task):
					// Create a new user and group to satisfy foreign key contraints
					if (ForeignUserId == -1) ForeignUserId = (int)Database.Insert(new User() { Username = "UnitTest_TaskCreator" });
					if (ForeignGroupId == -1) ForeignGroupId = (int)Database.Insert(new Group() { Creator = ForeignUserId, Name = "UnitTest_TaskGroup" });
					return new[] {
						new Task() { Title = "UnitTest_Group", Description = "UnitTest_SampleDescription", Priority = 1 },
						new Task() { Title = "UnitTest_Group_DifferentPriority", Description = "UnitTest_SampleDescription", Priority = 2 },
						new Task() { Title = "UnitTest_Group_Unicode_" + UnicodeSample, Description = UnicodeSample, Priority = 1 },
						new Task() { Title = "UnitTest_Group_Emoji_" + EmojiSample, Description = EmojiSample, Priority = 1 },
					}.Select(x =>
					{ // Set foreign keys
						x.Group = ForeignGroupId;
						x.Creator = ForeignUserId;
						return x;
					}).ToArray();
				case nameof(Comment):
					// Create a new user, group and task to satisfy foreign key constraints
					if (ForeignUserId == -1) ForeignUserId = (int)Database.Insert(new User() { Username = "UnitTest_CommentCreator" });
					if (ForeignGroupId == -1) ForeignGroupId = (int)Database.Insert(new Group() { Creator = ForeignUserId, Name = "UnitTest_TaskCommentGroup" });
					if (ForeignTaskId == -1) ForeignTaskId = (int)Database.Insert(new Task() { Creator = ForeignUserId, Group = ForeignGroupId, Title = "UnitTest_CommentTask" });
					return new[] {
						new Comment() { Message = "UnitTest_SampleMessage" },
						new Comment() { Message = "UnitTest_SampleMessage_Edited", Edited = DateTimeOffset.Now.ToUnixTimeSeconds() + 3600 },
						new Comment() { Message = "UnitTest_SampleMessage_Unicode_" + UnicodeSample },
						new Comment() { Message = "UnitTest_SampleMessage_Emoji_" + EmojiSample },
					}.Select(x =>
					{ // Set foreign keys
						x.Creator = ForeignUserId;
						x.Task = ForeignTaskId;
						return x;
					}).ToArray();
				case nameof(Session):
					// Create a new user to satisfy the foreign key constraint
					if (ForeignUserId == -1) ForeignUserId = (int)Database.Insert(new User() { Username = "UnitTest_SessionOwner" });
					return new[] {
						new Session() { Id = "57eb511e691fe342993c516c06003491", Key = new byte[0] },
						new Session() { Id = "ea862ffbd5dde044b6a881df01655787", Key = Convert.FromBase64String("hZdo/NO7gZqgzP4kqf2nKFTXD9kONiEwxpdmpkRNPT8=") },
						new Session() { Id = "6bb339793e1abd469dc99e8e9efd995a", User = ForeignUserId, Key = new byte[0] },
						new Session() { Id = "2fa99e90849c7246a6eb1d2387296a5e", User = ForeignUserId, Key = Convert.FromBase64String("F9fm2sY5+R0/ZXdUhHbKmGPPdAOMhprA9RLzifMJQsA=") },
					}.Select((x, i) =>
					{ // Set the expiration based on the index (each item expires 1 hour after the prior)
						x.Expires = DateTimeOffset.Now.ToUnixTimeSeconds() + 3600 * (i + 1);
						return x;
					}).ToArray();
				case nameof(Resource):
					return new[] {
					new Resource() { Filename = "UnitTest_Resource", Data = new byte[0], Hash = "d41d8cd98f00b204e9800998ecf8427e" },
					new Resource() { Filename = "UnitTest_Resource_Unicode_" + UnicodeSample, Data = new byte[0], Hash = "d41d8cd98f00b204e9800998ecf8427e" },
					new Resource() { Filename = "UnitTest_Resource_Emoji_" + EmojiSample, Data = new byte[0], Hash = "d41d8cd98f00b204e9800998ecf8427e" },
				};
				default:
					Assert.Fail($"The sample for type '{type.FullName}' is not implemented.");
					return null;
			};
		}

		[TestMethod]
		[Description("Tests if various ItemAdapter objects can successfully be updated.")]
		[DataRow(typeof(User), DisplayName = nameof(User))]
		[DataRow(typeof(Group), DisplayName = nameof(Group))]
		[DataRow(typeof(Task), DisplayName = nameof(Task))]
		[DataRow(typeof(Comment), DisplayName = nameof(Comment))]
		[DataRow(typeof(Session), DisplayName = nameof(Session))]
		[DataRow(typeof(Resource), DisplayName = nameof(Resource))]
		public void Generic_Update(Type type)
		{
			// Assert that the type is an ItemAdapter
			Assert.IsTrue(type.IsSubclassOf(typeof(ItemAdapter)), $"The type '{type.FullName}' does not extend '{typeof(ItemAdapter).FullName}'.");
			
			// Get the Action for swapping data from one item to another
			Action<dynamic, dynamic> updateAction = type.Name switch
			{
				nameof(User) => (x, y) => {
					x.Username = y.Username;
					x.Password = y.Password;
					x.Created = y.Created;
					x.AccessLevel = y.AccessLevel;
				},
				nameof(Group) => (x, y) =>{
					x.Name = y.Name;
					x.Created = y.Created;
					x.Description = y.Description;
				},
				nameof(Task) => (x, y) => {
					x.Title = y.Title;
					x.Description = y.Description;
					x.Created = y.Created;
					x.Priority = y.Priority;
				},
				nameof(Comment) => (x, y) => {
					x.Message = y.Message;
					x.Created = y.Created;
					x.Edited = y.Edited;
				},
				nameof(Session) => (x, y) => {
					x.User = y.User;
				},
				nameof(Resource) => (x, y) => {
					x.Filename = y.Filename;
				},
				_ => null
			};

			Func<User, bool, long> func = Database.Insert<User>;

			// Assert that the action was set
			Assert.IsNotNull(updateAction, $"The test for type '{type.FullName}' is not implemented.");

			// Get and insert the sample data for the specified type
			var sample = GetSample(type);
			InvokeGenericMethod<long>((Func<ICollection<ItemAdapter>, bool, long>)Database.Insert, type, sample, false);
			// Copy the sample so we can safely update the values later
			var sample_copy = sample.Select(x => Convert.ChangeType(x.Clone(), type) as ItemAdapter).ToArray();

			long changed = 0; // Counter for keeping track of how many items actually changed
			for (int i = 0; i < sample.Length; i++)
			{
				// Skip items that are equal to one another
				if (sample[i] == sample_copy[^(i + 1)]) continue;
				changed++;
				// Update the sample using the action
				updateAction.Invoke(sample[i], sample_copy[^(i + 1)]);
			}

			// Run the query and get the amount of affected rows
			long updated = InvokeGenericMethod<long>((Func<ICollection<ItemAdapter>, long>)Database.Update, type, new[] { sample });

			Assert.AreEqual(changed, updated, "The query did not update the expected amount of rows.");
		}

		[TestMethod]
		[DataRow(typeof(User), DisplayName = nameof(User))]
		[DataRow(typeof(Group), DisplayName = nameof(Group))]
		[DataRow(typeof(Task), DisplayName = nameof(Task))]
		[DataRow(typeof(Comment), DisplayName = nameof(Comment))]
		[DataRow(typeof(Session), DisplayName = nameof(Session))]
		[DataRow(typeof(Resource), DisplayName = nameof(Resource))]
		public void Generic_Insert(Type type)
		{
			// Get a sample and check if it can be inserted without throwing MySqlException
			var sample = GetSample(type);
			try
			{
				InvokeGenericMethod<long>((Func<ICollection<ItemAdapter>, bool, long>)Database.Insert, type, sample, false);
			}
			catch (MySqlException)
			{
				Assert.Fail("The insert query failed to upload the sample.");
			}
		}

		#region User Model Tests
		[TestMethod]
		[Priority(0)]
		[TestCategory("User Table")]
		[Description("Tests if a set of sample users can be uploaded into the database.")]
		public void User_Insert()
		{
			// Create sample users
			var user_noId = new User() { Username = "UnitTest_User_NoId" }; // Auto increment id replacement test user
			var user_presetId = new User() { Id = 1, Username = "UnitTest_User_PresetId" }; // Auto increment predefined id test user
			var user_hashTest = new User() { Username = "UnitTest_User_HashTest", Password = "HashTest" }; // Password hashing test user
			user_hashTest.Password = user_hashTest.GetPasswordHash();

			// Check if the hashing method produces the expected result
			Assert.AreEqual(user_hashTest.Password,
				   "cd7af020b76daf1f42f3cf9d0046293dcb17da9ee95b111bb0550e9906f2e4d5b820f18b74fdb19673585a22e17752459718d91dc87ae21a63d370bfa7ab3d9b"
			);

			Database.Insert(GetSampleUsers()); // Test the sample can be inserted
			Database.Insert(user_presetId); // Insert to keep the predefined primary key value
			Database.Insert(user_noId); // Insert to replace the primary key with a new value

			// Id may no longer be null after inserting
			Assert.IsNotNull(user_noId.Id);
			// Check if the user with the preset id was found in the database
			Assert.AreEqual(Database.Select<User>("`id` = 1").FirstOrDefault(), user_presetId);
		}

		[TestMethod]
		[Priority(0)]
		[TestCategory("User Table")]
		[Description("Tests if a set of sample users can successfully be selected from the database.")]
		public void User_Select()
		{
			// Get sample users
			var users = GetSampleUsers();
			// Insert the sample
			Database.Insert(users);
			// Select the new users from the database
			var selectedUsers = Database.Select<User>(string.Join(" OR ", users.Select(x => $"`id` = {x.Id}"))).ToArray();

			// Assert that the selected users match the sample array
			Assert.IsTrue(selectedUsers.Length == users.Length);
			foreach (var (selectedUser, user) in selectedUsers.Zip(users))
				Assert.AreEqual(selectedUser, user);
		}

		[TestMethod]
		[Priority(0)]
		[TestCategory("User Table")]
		[Description("Tests if a set of sample users can be deleted from the database.")]
		public void User_Delete()
		{
			// Get sample users
			var users = GetSampleUsers();
			// Insert the sample
			Database.Insert(users);

			// Delete the sample and get the amount of deleted users
			long deleted = Database.Delete(users);

			// Check if the amount of deleted users is equal to the sample size
			Assert.AreEqual(users.Length, deleted);
		}

		[TestMethod]
		[Priority(0)]
		[TestCategory("User Table")]
		[Description("Tests if set of sample users' data can be swapped and updated.")]
		public void User_Update()
		{
			// Get sample users
			var users = GetSampleUsers();
			var users_copy = users.Select(x => x.Clone() as User).ToArray();
			// Insert the sample
			Database.Insert(users);

			// Counter for how many users were changed
			long changed = 0;
			// Swap the usernames and passwords
			for (int i = 0; i < users.Length; i++)
			{
				// Skip users that are equal to one another
				if (users[i] == users[^(i + 1)]) continue;
				changed++;
				users[i].Username = users_copy[^(i + 1)].Username;
				users[i].Password = users_copy[^(i + 1)].Password;
				users[i].AccessLevel = users_copy[^(i + 1)].AccessLevel;
			}
			// Update all users
			long updated = Database.Update(users);

			// Assert that the amount of updated rows is equal to the amount of changed sample users
			Assert.AreEqual(changed, updated);
		}
		#endregion

		#region Group Model Tests
		[TestMethod]
		[Priority(1)]
		[TestCategory("Group Table")]
		[Description("Tests if a set of sample groups can be uploaded into the database.")]
		public void Group_Insert()
		{
			// Get the standard sample
			var sample = GetSampleGroups();

			// Create sample to test the auto increment column
			var group_noId = new Group() { Creator = ForeignUserId, Name = "UnitTest_Group_NoId" }; // Auto increment id replacement test
			var group_presetId = new Group() { Creator = ForeignUserId, Id = 1, Name = "UnitTest_Group_PresetId" }; // Auto increment predefined id test

			Database.Insert(sample); // Test the sample can be inserted
			Database.Insert(group_presetId); // Insert to keep the predefined primary key value
			Database.Insert(group_noId); // Insert to replace the primary key with a new value

			Assert.IsNotNull(group_noId.Id, "The id was not replaced during after query.");
			Assert.AreEqual(Database.Select<Group>("`id` = 1").FirstOrDefault(), group_presetId,
				"The item with the preset id could not be found in the database.");
		}

		[TestMethod]
		[Priority(1)]
		[TestCategory("Group Table")]
		[Description("Tests if a set of sample groups can successfully be selected from the database.")]
		public void Group_Select()
		{
			// Get sample groups
			var groups = GetSampleGroups();
			// Insert the sample
			Database.Insert(groups);
			// Select the new users from the database
			var selectedGroups = Database.Select<Group>(string.Join(" OR ", groups.Select(x => $"`id` = {x.Id}"))).ToArray();

			Assert.IsTrue(selectedGroups.SequenceEqual(groups), "The selected data does not match the sample.");
		}

		[TestMethod]
		[Priority(1)]
		[TestCategory("Group Table")]
		[Description("Tests if a set of sample groups can be deleted from the database.")]
		public void Group_Delete()
		{
			// Insert the sample
			var groups = GetSampleGroups();
			Database.Insert(groups);

			// Delete the sample and get the amount of deleted users
			long deleted = Database.Delete(groups);

			Assert.AreEqual(groups.Length, deleted, "The query did not delete the expected amount of rows.");
		}
		#endregion

		/// <summary>
		/// Invokes a generic function with the specified <see cref="Type"/> and returns the result.
		/// </summary>
		/// <param name="func">The generic function to invoke.</param>
		/// <param name="type">The type to use as generic type parameter.</param>
		private static T InvokeGenericMethod<T>(Func<T> func, Type type)
		{
			// Cast the generic type to a specific type
			var concreteMethod = func.Method.GetGenericMethodDefinition().MakeGenericMethod(new[] { type });
			// Invoke and return the new concretely typed method
			return (T)concreteMethod.Invoke(func.Target, null);
		}
		/// <summary>
		/// Invokes a generic function with the specified <see cref="Type"/> and parameter and
		/// returns the result.
		/// </summary>
		/// <param name="func">The generic function to invoke.</param>
		/// <param name="type">The type to use as generic type parameter.</param>
		/// <param name="args">An array of arguments to pass to the function.</param>
		private static T InvokeGenericMethod<T>(dynamic func, Type type, params object[] args)
		{
			// Cast the generic type to a specific type
			var concreteMethod = func.Method.GetGenericMethodDefinition().MakeGenericMethod(new[] { type });
			// Invoke and return the new concretely typed method
			return (T)concreteMethod.Invoke(func.Target, args);
		}
	}
}