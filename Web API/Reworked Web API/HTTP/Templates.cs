using API.Config;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace API.HTTP
{
	/// <summary>
	/// A static class that manages the Razor .cshtml templates of this project.
	/// </summary>
	public static class Templates
	{
		private static AppConfig Config => Program.Config;
		private static Logging.Logger Log => Program.Log;

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

		/// <summary>
		/// Compiles all Razor source files located in HTML source directory.
		/// </summary>
		public static void CompileAll()
		{
			// Get all .cshtml files recursively
			var templates = FileSearch(Config.HTMLSourceDir, true, x => Path.GetExtension(x).ToLower() == ".cshtml");
			if (!templates.Any()) return;

			Log.Config($"Compiling Razor templates…");
			var timer = new Stopwatch(); // Timer for diagnostics
			foreach (var file in templates)
			{
				timer.Restart();
				var name = '/' + Path.GetRelativePath(Config.HTMLSourceDir, file).Replace('\\', '/');
				// Skip already compiled templates
				if (Engine.Razor.IsTemplateCached(name, null)) continue;
				Engine.Razor.Compile(File.ReadAllText(file), name);
				Log.Config($"Compiled '{name}' in {timer.ElapsedMilliseconds} ms.");
			}
		}

		/// <summary>
		/// Runs a compiled Razor CSHTML template and returns it's result.
		/// </summary>
		/// <param name="file">The Razor template file to run.</param>
		/// <param name="request">A request object to add to the ViewBag.</param>
		/// <param name="parameters">A dictionary of url parameters to add to the ViewBag.</param>
		/// <param name="model">An object to pass to the template as @Model.</param>
		public static string RunTemplate(string file,
			HttpListenerRequest request,
			Dictionary<string, string> parameters,
			object model = null)
		{
			if (!Engine.Razor.IsTemplateCached(file, null)) throw new ArgumentException($"File '{file}' is not cached or does not exist.");
			var viewBag = new DynamicViewBag(new Dictionary<string, object>
			{
				{"request", request },
				{"parameters", parameters }
			});
			
			return Engine.Razor.Run(file, null, model, viewBag);
		}
	}
}
