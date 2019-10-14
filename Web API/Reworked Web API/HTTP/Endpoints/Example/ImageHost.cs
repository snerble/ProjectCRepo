using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// Subclass of <see cref="HTMLEndpoint"/> that displays all image and video resources on the server. Created to illustrate the capability of these endpoints.
	/// </summary>
	[EndpointUrl("/image")]
	public sealed class ImageHost : HTMLEndpoint
	{
		public ImageHost(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

		/// <summary>
		/// Simple recursive function that returns an array of all files under a certain path.
		/// </summary>
		/// <param name="path">The path to search recursively.</param>
		private string[] Recurse(string path)
		{
			List<string> outList = Directory.EnumerateFiles(path).ToList();
			foreach (var dir in Directory.EnumerateDirectories(path))
				outList.AddRange(Recurse(dir));
			return outList.ToArray();
		}

		public override void GET(Dictionary<string, string> parameters)
		{
			string src = Program.Config["serverSettings"]["resourceDir"].ToObject<string>();
			string[] images;
			// Get files
			if (parameters.ContainsKey("recurse")) images = Recurse(src); // recursively if specified
			else images = Directory.EnumerateFiles(src).ToArray(); // otherwise just list all of them

			// Get all files that contain the strings in the image parameter. delimited with ,
			if (parameters.ContainsKey("image"))
				images = images.Where(x => parameters["image"].ToLower().Split(',').Any(y => x.Contains(y))).ToArray();

			// filter out all non-image or video files
			string[] imageAndVideoExtensions = new string[] { ".webm", ".ogg", ".mp4", ".apng", ".bmp", ".gif", ".ico", ".cur", ".jpeg", ".jpg", ".jfif", ".pjpeg", ".pjp", ".png", ".svg", ".tif", ".tiff", ".webp" };
			images = images.Where(x => imageAndVideoExtensions.Contains(Path.GetExtension(x).ToLower())).ToArray();

			// If no images are found, send "no images found"
			if (images.Length == 0)
			{
				Server.SendText(Response, "No images found");
				return;
			}

			// If the 'all' parameter is not specified, send only one image
			if (!parameters.ContainsKey("all"))
			{
				// Get the index value
				int index = 0;
				if (parameters.ContainsKey("index")) int.TryParse(parameters["index"], out index);

				// If the index is out of the available range, return error message
				if (index >= images.Length || index < 0)
				{
					Server.SendText(Response, $"Index out of range. (0-{images.Length - 1})");
					return;
				}

				// Send the image at the specified index
				Server.Send(Response, File.ReadAllBytes(images[index]));
				return;
			}

			// Prepare generated html if the 'all' parameter was specified
			string outtext = "<html style=\"text-align: center; font-family: Calibri; font-size: 13pt;\">";

			// Get image grid width value
			int width = 5;
			if (parameters.ContainsKey("w")) int.TryParse(parameters["w"], out width);
			width = width > images.Length ? images.Length : width;

			// Get limit and page value
			int limit = 0;
			if (parameters.ContainsKey("limit")) int.TryParse(parameters["limit"], out limit);
			int page = 0;
			if (parameters.ContainsKey("page")) int.TryParse(parameters["page"], out page);
			int imageCount = images.Length;
			if (limit > 0)
			{
				if (page < 0 || page > (images.Length / limit))
				{
					Server.SendText(Response, $"page value out of range. (0-{images.Length / limit})");
					return;
				}
				page = page > (images.Length / limit) ? (images.Length / limit) : page;

				int offset = limit * page;
				int end = offset + limit > images.Length ? images.Length : offset + limit;
				images = images[offset..end];
			}

			// Append page links if a limit is used
			if (limit > 0)
			{
				outtext += "<div style=\"position: sticky; text-align: center; top: 5px; margin: 1% auto 1% auto; z-index: 100; " +
					"width: fit-content; background-color: white; border: 2px solid #d4d4d4; border-radius: 5px; padding: 3px\">";
				for (int i = 0; i <= (imageCount / limit); i++)
				{
					if (i == page)
					{
						outtext += $"<b style=\"margin: 5px\">{i}</b>";
						continue;
					}
					string pageUrl = Request.Url.Scheme + "://" + Request.Url.Host + Request.Url.AbsolutePath + '?';
					parameters["page"] = i.ToString();
					pageUrl += string.Join('&', parameters.Select(item => item.Key + (item.Value.Length == 0 ? "" : "=" + item.Value)));
					outtext += $"<a style=\"display: inline-block; margin: 5px\" href=\"{pageUrl}\">{i}</a>";
				}
				outtext += "</div>";
			}
			outtext += @$"<style>
a {{
	text-decoration: none;
}}
a:hover {{
	text-decoration: underline;
}}
.item {{
	position: relative;
	display: inline-block;
	margin: 2px;
	vertical-align: top;
}}
.item img, .item video {{
	height: auto;
	flex: 1;
	width: auto;
	height: auto;
	max-width: 100%;
	max-height: 25vh;
}}
img, video {{
	border-radius: 4px;
}}
.desc {{
	margin: 0 auto 0 auto;
	padding: 5px 5px 8px 5px;
	position: absolute;
	width: -webkit-fill-available;
	bottom: 0;
	left: 0;
	word-break: break-word;
	overflow-y: hidden;
	overflow-x: hidden;
	max-height: 95%;
	opacity: 0;
	-webkit-transition: opacity 0.25s;
	font-weight: bold;
	color: white;
	background: #000000B2;
	border-radius: 0 0 4px 4px;
}}
.item:hover .desc {{
	opacity: 1;
}}
</style>";
			// Generate image label for each image found
			foreach (var _image in images)
			{
				var image = Path.GetRelativePath(Program.Config["serverSettings"]["resourceDir"].ToObject<string>(), _image);

				outtext += "<div class=\"item\">";
				// If extension is a video, create a video label (doesn't seem to work on mobile though)
				if (new string[] { ".webm", ".mp4", ".ogg" }.Contains(Path.GetExtension(image).ToLower()))
				{
					outtext +=
						$"<video title=\"{Path.GetFileName(image)}\" controls>" +
							$"<source src=\"{Uri.EscapeDataString(image)}\" type=\"video/{Path.GetExtension(image).ToLower()[1..]}\">" +
						$"</video>";
				} // Otherwise just create an image label with a description div
				else
				{
					outtext += $"<image src=\"{Uri.EscapeDataString(image)}\"/>";
					outtext += $"<div class=\"desc\">{Path.GetFileName(image)}</div>";
				}
				outtext += "</div>";
			}

			// Append the closing html label and send the text
			Server.SendText(Response, outtext + "</html>");
		}
	}
}
