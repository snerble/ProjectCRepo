using API.Attributes;
using API.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace API
{
	/// <summary>
	/// A static class containing various static utilities for this project.
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// A dictionary containing instances of <see cref="AppDatabase"/> mapped to a <see cref="Thread"/> ID.
		/// </summary>
		private static Dictionary<int, AppDatabase> Databases { get; } = new Dictionary<int, AppDatabase>();

		/// <summary>
		/// Gets an array of all <see cref="RedirectAttribute"/>s listed in the server properties.
		/// </summary>
		public static ReadOnlyCollection<RedirectAttribute> Redirects { get; } = Array.AsReadOnly(
				Assembly.GetExecutingAssembly().GetCustomAttributes<RedirectAttribute>() as RedirectAttribute[]
			);
		/// <summary>
		/// Gets an array of all <see cref="AliasAttribute"/>s listed in the server properties.
		/// </summary>
		public static ReadOnlyCollection<AliasAttribute> Aliases { get; } = Array.AsReadOnly(
				Assembly.GetExecutingAssembly().GetCustomAttributes<AliasAttribute>() as AliasAttribute[]
			);
		/// <summary>
		/// Gets an array of all <see cref="ErrorPageAttribute"/>s listed in the server properties.
		/// </summary>
		public static ReadOnlyCollection<ErrorPageAttribute> ErrorPages { get; } = Array.AsReadOnly(
				Assembly.GetExecutingAssembly().GetCustomAttributes<ErrorPageAttribute>() as ErrorPageAttribute[]
			);

		/// <summary>
		/// Cache of active sessions handled by the server to save on database queries.
		/// </summary>
		public static List<Session> Sessions { get; } = new List<Session>();
		/// <summary>
		/// A timestamp in unix time marking when the next call to <see cref="GetSession(string)"/>
		/// should perform a cleanup query.
		/// </summary>
		private static DateTime SessionCleanup = DateTime.UnixEpoch;

		/// <summary>
		/// Returns an <see cref="AppDatabase"/> instance unique to the current calling thread.
		/// <para>If no <see cref="AppDatabase"/> exist yet for the current thread, a new instance will be created.</para>
		/// </summary>
		/// <seealso cref="DisposeDatabases"/>
		public static AppDatabase GetDatabase()
		{
			// Lock the databases property to prevent conflicts from the DisposeDatabases method
			lock (Databases)
			{
				// Get the id of the thread that called this method
				var threadId = Thread.CurrentThread.ManagedThreadId;

				// Create a new AppDatabase if it doesn't exist yet
				if (!Databases.ContainsKey(threadId))
				{
					Databases.Add(threadId, new AppDatabase());
					Program.Log.Fine($"Opened connection to '{Databases[threadId].Connection.DataSource}'.");
				}

				// Return an AppDatabase from the cache
				return Databases[threadId];
			}
		}
		/// <summary>
		/// Disposes all cached <see cref="AppDatabase"/>s and clears the cache.
		/// </summary>
		public static void DisposeDatabases()
		{
			// Lock the databases property to prevent conflicts from the GetDatabase method
			lock (Databases)
			{
				// Close all connections
				foreach (var database in Databases.Values)
				{
					Program.Log.Info($"Closing connection to '{database.Connection.DataSource}'...");
					lock (database) // Lock each database to ensure a Server instance can complete its task before it is disposed.
						database.Dispose();
				}
				// Clear all elements from the cache
				Databases.Clear();
			}

		}

#nullable enable
		/// <summary>
		/// Gets a <see cref="Session"/> from the cache or the database.
		/// </summary>
		/// <param name="sessionId">The id of the session to get.</param>
		public static Session? GetSession(string sessionId)
		{
			if (sessionId is null) throw new ArgumentNullException(nameof(sessionId));

			// Run cleanup query if the cleanup timestamp has been passed
			if (SessionCleanup < DateTime.UtcNow)
			{
				Program.Log.Fine("Running session cleanup...");
				Program.Log.Fine("Deleted " +
					GetDatabase().Delete<Session>("`expires` < UNIX_TIMESTAMP()") +
					" expired sessions."
				);
				// Set the cleanup timestamp 2 hours into the future
				SessionCleanup = DateTime.UtcNow.AddHours(2);
			}

			// Get session from cache or database
			var session = Sessions.FirstOrDefault(x => x.Id == sessionId);
			if (session == null)
			{
				session = GetDatabase().Select<Session>($"`id` = '{sessionId}'").FirstOrDefault();
				if (session == null || session.User == null) return session;
				Sessions.Add(session);
			}
			// Remove session if expired
			if (DateTimeOffset.FromUnixTimeSeconds(session.Expires) <= DateTime.UtcNow)
			{
				GetDatabase().Delete(session);
				Sessions.Remove(session);
				return null;
			}
			return session;
		}
		/// <summary>
		/// Returns and uploads a new <see cref="Session"/> with an optional attached <see cref="User"/> object.\
		/// </summary>
		/// <param name="userId">The user to attach to the new <see cref="Session"/>.</param>
		/// <remarks>
		/// It is recommended that this method is called during a database transaction, since the session
		/// should not be uploaded if it is lost.
		/// </remarks>
		public static Session CreateSession(int userId)
		{
			// Get the user
			var user = GetDatabase().Select<User>($"`id` = {userId}").FirstOrDefault();

			// Throw exception if the user does not exist
			if (user == null) throw new ArgumentException("No such user with id " + userId);

			// Invoke overloaded method
			return CreateSession(user);
		}
		/// <summary>
		/// Returns and uploads a new <see cref="Session"/> with an optional attached <see cref="User"/> object.
		/// <para>If <paramref name="user"/> is null, the new <see cref="Session"/> will not be cached.</para>
		/// </summary>
		/// <param name="user">The user to attach to the new <see cref="Session"/>.</param>
		/// <remarks>
		/// It is recommended that this method is called during a database transaction, since the session
		/// should not be uploaded if it is lost.
		/// </remarks>
		public static Session CreateSession(User? user = null)
		{
			// Create the session object and give it a new AES key and GUID
			using var aes = Aes.Create();
			var session = new Session()
			{
				Id = string.Concat(Guid.NewGuid().ToByteArray().Select(x => x.ToString("x2"))),
				User = user?.Id,
				Key = aes.Key
			};

			// Upload the new session
			GetDatabase().Insert(session);

			// Return the new session
			return session;
		}
		
		/// <summary>
		/// Returns whether or not a request is encrypted by looking at it's headers.
		/// </summary>
		/// <param name="request">The request to check for encryption.</param>
		public static bool IsRequestEncrypted(HttpListenerRequest request)
		{
			if (request is null) throw new ArgumentNullException(nameof(request));
			return request.ContentType == "application/octet-stream"
				&& request.Cookies["session"] != null
				&& request.Headers.AllKeys.Contains("Content-IV");
		}

		/// <summary>
		/// Uses the <see cref="Aes"/> cipher and decrypts an encoded message from a session
		/// using the specified initialization vector.
		/// </summary>
		/// <param name="sessionId">The id of a <see cref="Session"/>.</param>
		/// <param name="encoded">The message to decode.</param>
		/// <param name="iv">The initalization vector of the encoded message.</param>
		/// <returns>The decoded message.</returns>
		public static byte[] AESDecrypt(string sessionId, byte[] encoded, byte[] iv)
			=> AESDecrypt(GetSession(sessionId) ?? throw new ArgumentException("The specified session Id is invalid."), encoded, iv);
		/// <summary>
		/// Uses the <see cref="Aes"/> cipher and decrypts an encoded message from a session
		/// using the specified initialization vector.
		/// </summary>
		/// <param name="session">A <see cref="Session"/> instance containing a defined key.</param>
		/// <param name="encoded">The data to decode.</param>
		/// <param name="iv">The initalization vector of the encoded data.</param>
		/// <returns>The decoded data.</returns>
		public static byte[] AESDecrypt(Session session, byte[] encoded, byte[] iv)
		{
			// Null checks
			if (session is null) throw new ArgumentNullException(nameof(session));
			if (encoded is null) throw new ArgumentNullException(nameof(encoded));
			if (iv is null) throw new ArgumentNullException(nameof(iv));

			// Create new aes and set key and iv
			using var aes = Aes.Create();
			aes.Padding = PaddingMode.Zeros;
			aes.Key = session.Key;
			aes.IV = iv;

			// Decode data
			using var mem = new MemoryStream(); // Create memory stream for the decryptor to write the decoded data to
			using var cs = new CryptoStream(mem, aes.CreateDecryptor(), CryptoStreamMode.Write); // Create crypto stream which decodes the data
			cs.Write(encoded); // Decode 'encoded' by writing it through 'cs' and onto 'mem'.
			cs.FlushFinalBlock(); // Flush the last buffered block and update 'mem'. (basically append remaining bytes)

			return mem.ToArray();
		}

		/// <summary>
		/// Uses the <see cref="Aes"/> cipher and encrypts the data using the specified key.
		/// </summary>
		/// <param name="sessionId">The id of the session whose key to use for encryption.</param>
		/// <param name="data">The data to encode.</param>
		/// <param name="iv">A byte array of exactly 32 bytes to which the initialization vector will be copied.</param>
		/// <returns>The encoded data.</returns>
		public static byte[] AESEncrypt(string sessionId, byte[] data, out byte[] iv)
			=> AESEncrypt(GetSession(sessionId) ?? throw new ArgumentException("The specified session Id is invalid."), data, out iv);
		/// <summary>
		/// Uses the <see cref="Aes"/> cipher and encrypts the data using the specified key.
		/// </summary>
		/// <param name="session">The session whose key to use for encoding.</param>
		/// <param name="data">The data to encode.</param>
		/// <param name="iv">A byte array of exactly 32 bytes to which the initialization vector will be copied.</param>
		/// <returns>The encoded data.</returns>
		public static byte[] AESEncrypt(Session session, byte[] data, out byte[] iv)
		{
			// Null checks
			if (session is null) throw new ArgumentNullException(nameof(session));
			if (data is null) throw new ArgumentNullException(nameof(data));

			// Create new aes and set the key
			using var aes = Aes.Create();
			aes.Padding = PaddingMode.Zeros;
			aes.Key = session.Key;
			iv = aes.IV;

			// Encode data
			using var mem = new MemoryStream(); // Memory stream for writing a variable byte array
			using var cs = new CryptoStream(mem, aes.CreateEncryptor(), CryptoStreamMode.Write); // Crypto stream for writing encoded data to 'mem'
			cs.Write(data); // Write the data which will be encrypted and written to 'mem'
			cs.FlushFinalBlock(); // Flush remaining bytes (if any)

			return mem.ToArray();
		}
#nullable disable

		/// <summary>
		/// Formats a <see cref="DateTime"/> object into a string suitable for HTTP headers.
		/// </summary>
		/// <param name="timeStamp">The <see cref="DateTime"/> to format.</param>
		/// <returns>The formatted time string.</returns>
		public static string FormatTimeStamp(DateTime timeStamp)
			=> timeStamp.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'");
		/// <summary>
		/// Formats a <see cref="DateTimeOffset"/> object into a string suitable for HTTP headers.
		/// </summary>
		/// <param name="timeStamp">The <see cref="DateTimeOffset"/> to format.</param>
		/// <returns>The formatted time string.</returns>
		public static string FormatTimeStamp(DateTimeOffset timeStamp)
			=> timeStamp.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'");

		/// <summary>
		/// Returns a formatted string depicting the elapsed time in either nanoseconds, microseconds or milliseconds
		/// depending on the magnitude of elapsed time.
		/// </summary>
		/// <param name="timer">The <see cref="Stopwatch"/> instance whose elapsed time to format.</param>
		public static string FormatTimer(Stopwatch timer)
		{
			timer.Stop();
			try
			{
				if (timer.ElapsedTicks < 10) return timer.ElapsedTicks * 100 + " ns";
				if (timer.ElapsedTicks < 10000) return timer.ElapsedTicks / 10 + " µs";
				return timer.ElapsedMilliseconds + " ms";
			}
			finally
			{
				timer.Start();
			}
		}
		/// <summary>
		/// Returns a formatted string depicting an amount of data in multiples of the byte unit.
		/// </summary>
		/// <param name="length"></param>
		/// <param name="decimals">The amount of fractional digits to include in the output</param>
		/// <param name="asBinaryUnit">If true, formats the size as a binary unit rather than as an SI unit.</param>
		public static string FormatDataLength(long length, int decimals = 2, bool asBinaryUnit = true)
		{
			var magnitude = 0;
			for (; magnitude <= 8; magnitude++)
				if ((asBinaryUnit ? Math.Pow(1000, magnitude + 1) : Math.Pow(2, 10 * (magnitude + 1))) >= length)
					break;
			var unit = (magnitude) switch
			{
				0 => "bytes",
				1 => asBinaryUnit ? "KiB" : "kB",
				2 => asBinaryUnit ? "MiB" : "MB",
				3 => asBinaryUnit ? "GiB" : "GB",
				4 => asBinaryUnit ? "TiB" : "TB",
				5 => asBinaryUnit ? "PiB" : "PB",
				6 => asBinaryUnit ? "EiB" : "EB",
				7 => asBinaryUnit ? "ZiB" : "ZB",
				_ => asBinaryUnit ? "YiB" : "YB"
			};
			return $"{Math.Round(length / (asBinaryUnit ? Math.Pow(2, 10 * magnitude) : Math.Pow(1000, magnitude)), decimals)} {unit}";
		}

		/// <summary>
		/// Adds a cookie to the response object.
		/// </summary>
		/// <param name="response">The response object to add a cookie to.</param>
		/// <param name="name">The name of the cookie.</param>
		/// <param name="value">The value of the cookie.</param>
		public static void AddCookie(HttpListenerResponse response, string name, object value)
			=> response.Headers.Add("Set-Cookie", $"{name}={value}");
	}
}
