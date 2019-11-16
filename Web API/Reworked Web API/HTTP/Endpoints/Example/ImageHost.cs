using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Subclass of <see cref="HTMLEndpoint"/> that displays all image and video resources on the server. Created to illustrate the capability of these endpoints.
	/// </summary>
	[EndpointUrl("/u/image")]
	public sealed class ImageHost : HTMLEndpoint
	{
		private static readonly string[] VideoExtensions = new string[] { ".webm", ".ogg", ".mp4" };
		private static readonly string[] ImageExtensions = new string[] { ".apng", ".bmp", ".gif", ".ico", ".cur", ".jpeg", ".jpg", ".jfif", ".pjpeg", ".pjp", ".png", ".svg", ".tif", ".tiff", ".webp" };

		/// <summary>
		/// Returns files in the specified directory.
		/// </summary>
		/// <param name="path">The path in which to search.</param>
		/// <param name="recurse">Whether to search for files recursively or not.</param>
		/// <param name="predicate">A predicate to test every file for a condition.</param>
		private static IEnumerable<string> FileSearch(string path, bool recurse = false, Func<string, bool> predicate = null)
		{
			// Throw exception if the path doesn't exist
			if (!Directory.Exists(path)) throw new ArgumentException("The specified path is invalid.");
			predicate ??= x => true; // Default predicate always returns true

			// Yield all files in 'path' that satisfy the predicate
			foreach (var file in Directory.EnumerateFiles(path).Where(predicate))
				yield return file;

			// If 'recurse' is true, yield all underlying files in 'path' that satisfy the predicate
			if (recurse)
				foreach (var dir in Directory.EnumerateDirectories(path))
					foreach (var file in FileSearch(dir, true, predicate))
						yield return file;
		}

		public override void GET(Dictionary<string, string> parameters)
		{
			string src = Program.Config["serverSettings"]["resourceDir"].ToObject<string>();

			bool recurse = parameters.ContainsKey("recurse");
			bool all = parameters.ContainsKey("all");
			string[] searchParams = new string[0];
			int limit = 0;
			int page = 0;

			if (parameters.ContainsKey("image")) searchParams = parameters["image"].ToLower().Split(',');
			if (parameters.ContainsKey("limit")) int.TryParse(parameters["limit"], out limit);
			if (parameters.ContainsKey("page")) int.TryParse(parameters["page"], out page);

			bool predicate(string file)
			{
				var ext = Path.GetExtension(file).ToLower();
				if (!ImageExtensions.Contains(ext) && !VideoExtensions.Contains(ext)) return false; // Must be image or video
				if (searchParams.Length > 0 && !searchParams.Any(x => file.ToLower().Contains(x))) return false; // Must contain one search param
				return true;
			}

			// Get files
			var images = new List<string>();
			if (all) images.AddRange(FileSearch(src, recurse, predicate));
			else images.Add(FileSearch(src, recurse, predicate).FirstOrDefault());

			// If no images are found, send "no images found"
			if (images.FirstOrDefault() == null)
			{
				Server.SendText("No images found");
				return;
			}

			// If images is less than 1, replace it with images.Count
			if (limit <= 0) limit = images.Count;
			// Convert all image paths to relative paths
			images = images.Select(x => Path.GetRelativePath(src, x)).ToList();

			// If the 'all' parameter is not specified, send only one image
			if (!parameters.ContainsKey("all"))
			{
				// Send the image at the specified index
				Server.Send(File.ReadAllBytes(src + '\\' + images[page]));
				return;
			}

			// Generate page links for use in the template
			var pages = new string[images.Count / limit];
			for (int i = 0; i < pages.Length; i++)
			{
				parameters["page"] = i.ToString();
				pages[i] = Request.Url.Scheme + "://"
					+ Request.Url.Host
					+ Request.Url.AbsolutePath + '?'
					+ string.Join("&", parameters.Select(x => x.Key + (x.Value.Length == 0 ? "" : "=") + x.Value));
			}

			// Run template with dynamic model and send the result
			Server.SendText(Templates.RunTemplate(GetUrl<ImageHost>() + ".cshtml", Request, parameters, new
			{
				images,
				limit,
				page,
				VideoExtensions,
				pages
			}));
		}
	}
}
