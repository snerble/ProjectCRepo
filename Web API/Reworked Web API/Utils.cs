using API.Attributes;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace API
{
	/// <summary>
	/// A static class containing various static utilities for this project.
	/// </summary>
	static class Utils
	{
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
		/// Determines a text file's encoding by analyzing its byte order mark (BOM).
		/// Defaults to ASCII when detection of the text file's endianness fails.
		/// </summary>
		/// <param name="filename">The text file to analyze.</param>
		/// <returns>The detected encoding.</returns>
		public static Encoding GetEncoding(string filename)
		{
			// Read the BOM
			var bom = new byte[4];
			using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				file.Read(bom, 0, 4);
			}

			// Analyze the BOM
			if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
			if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
			if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
			if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
			if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
			return Encoding.ASCII;
		}

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
		/// Adds a cookie to the response object.
		/// </summary>
		/// <param name="response">The response object to add a cookie to.</param>
		/// <param name="name">The name of the cookie.</param>
		/// <param name="value">The value of the cookie.</param>
		public static void AddCookie(HttpListenerResponse response, string name, object value)
			=> response.Headers.Add("Set-Cookie", $"{name}={value}");
	}
}
