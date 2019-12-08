using API.Config;
using API.Database;
using Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using MySQL.Modeling;
using System;
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
		/// Equivalent to calling <see cref="Program.Log"/>.
		/// </summary>
		public static Logger Log => Program.Log;

		/// <summary>
		/// Gets the <see cref="AppDatabase"/> instance used by the unit tests.
		/// </summary>
		public static AppDatabase Database { get; private set; }

		/// <summary>
		/// Gets the <see cref="MySqlTransaction"/> instance that is used for all unit tests.
		/// </summary>
		public MySqlTransaction Transaction { get; private set; }

		/// <summary>
		/// Initializes the resources for the unit tests.
		/// </summary>
		[TestInitialize()]
		public void Initialize()
		{
			// Modify the logger
			Log.OutputStreams.Clear();
			Log.Format = "{asctime:HH:mm:ss} {className}.{funcName,-10} {levelname,6}: {message}";

			// Load the config (if it doesn't already exist)
			Program.Config ??= new AppConfig("config.json");
			// Create a new database connection if it doesn't already exist
			Database ??= Utils.GetDatabase();
			// Create a transaction to prevent the changes from being applied (if it doesn't already exist)
			Transaction = Database.Connection.BeginTransaction();
		}
		/// <summary>
		/// Disposes of all resources used by the unit tests.
		/// </summary>
		[TestCleanup()]
		public void Cleanup()
		{
			// Revert changes
			//Transaction?.Rollback();
			// Dispose the transaction
			Transaction?.Dispose();
		}

		/// <summary>
		/// Pings the database. Purely for diagnostics.
		/// </summary>
		[TestMethod]
		public void Ping()
		{
			var timer = new Stopwatch();
			timer.Start();
			Database.Connection.Ping();
			Log.Info("Ping: " + Utils.FormatTimer(timer));
		}

		#region User Tests
		/// <summary>
		/// Tests if a <see cref="User"/> item can be uploaded into the database.
		/// </summary>
		[TestMethod]
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

			Database.Insert(GetUserSample()); // Test the sample can be inserted
			Database.Insert(user_presetId); // Insert to keep the predefined primary key value
			Database.Insert(user_noId); // Insert to replace the primary key with a new value

			// Id may no longer be null after inserting
			Assert.IsNotNull(user_noId.Id);
			// Check if the user with the preset id was found in the database
			Assert.AreEqual(Database.Select<User>("`id` = 1").FirstOrDefault(), user_presetId);
		}
		/// <summary>
		/// Tests if <see cref="User"/> instances can successfully be selected from the database.
		/// </summary>
		[TestMethod]
		public void User_Select()
		{
			// Get sample users
			var users = GetUserSample();
			// Insert the sample
			Database.Insert(users);
			// Select the new users from the database
			var selectedUsers = Database.Select<User>(string.Join(" OR ", users.Select(x => $"`id` = {x.Id}"))).ToArray();

			// Assert that the selected users match the sample array
			Assert.IsTrue(selectedUsers.Length == users.Length);
			foreach (var (selectedUser, user) in selectedUsers.Zip(users))
				Assert.AreEqual(selectedUser, user);
		}
		/// <summary>
		/// Tests if a set of sample <see cref="User"/>s can be deleted from the database.
		/// </summary>
		[TestMethod]
		public void User_Delete()
		{
			// Get sample users
			var users = GetUserSample();
			// Insert the sample
			Database.Insert(users);

			// Delete the sample and get the amount of deleted users
			long deleted = Database.Delete(users);

			// Check if the amount of deleted users is equal to the sample size
			Assert.AreEqual(users.Length, deleted);
		}
		/// <summary>
		/// Tests if a <see cref="User"/> sample's username and password can be swapped and updated.
		/// </summary>
		[TestMethod]
		public void User_Update()
		{
			// Get sample users
			var users = GetUserSample();
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
			}
			// Update all users
			long updated = Database.Update(users);

			// Assert that the amount of updated rows is equal to the amount of changed sample users
			Assert.AreEqual(changed, updated);
		}

		/// <summary>
		/// Returns an array of <see cref="User"/> instances with varying data.
		/// </summary>
		private User[] GetUserSample()
		{
			// Create an array of users
			var users = new User[] {
				new User() { Username = "UnitTest_User", Password = "UnitTest_SamplePassword" },
				new User() { Username = "UnitTest_User_Unicode_გთხოვთ ახლავე გაიაროთ", Password = "გთხოვთ ახლავე გაიაროთ" },
				new User() { Username = "UnitTest_User_Emoji_🤔🤔🤔🤔🤔🤔🤔🤔🤔🤔", Password = "🤔🤔🤔🤔🤔🤔🤔🤔🤔🤔" },
			};
			// Hash all passwords
			foreach (var user in users)
				user.Password = user.GetPasswordHash();
			return users;
		}
		#endregion
	}
}