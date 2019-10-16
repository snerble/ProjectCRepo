using Config.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Config
{
	/// <summary>
	/// Abstract class for a flexible JSON configuration file.
	/// </summary>
	/// <remarks>
	/// If the target config file is edited, the data of the <see cref="ConfigBase"/> object is updated.
	/// Any changes made in the object before saving are discarded.
	/// </remarks>
	public abstract class ConfigBase
	{
		/// <summary>
		/// Gets or sets a section from the config with the given key.
		/// </summary>
		/// <param name="key">The key of the value to get or set.</param>
		public JObject this[string key]
		{
			get { return Content[key] as JObject; }
			set { Content[key] = value; }
		}

		/// <summary>
		/// The path of the file this <see cref="ConfigBase"/> uses.
		/// </summary>
		public string ConfigFile { get; }
		/// <summary>
		/// Gets or sets the <see cref="FileInfo"/> object associated with the <see cref="ConfigFile"/>.
		/// </summary>
		private FileSystemWatcher ConfigWatcher { get; set; } = null;
		/// <summary>
		/// The raw JSON data of this <see cref="ConfigBase"/>.
		/// </summary>
		protected JObject Content { get; private set; }

		/// <summary>
		/// Gets whether this <see cref="ConfigBase"/> instance will reload it's contents when it's file is updated.
		/// </summary>
		protected abstract bool AutoReload { get; }
		private void OnChanged(object source, FileSystemEventArgs e)
		{
			ConfigWatcher.EnableRaisingEvents = false;
			if (e.FullPath != Path.GetFullPath(ConfigFile)) return;

			// Try to read the file because it is often still in use.
			for (int i = 0; i < 10; i++)
			{
				// TODO see if Stream.Lock helps
				try { Content = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(ConfigFile)); }
				catch (IOException) { Thread.Sleep(50); }
			}

			OnReload(source, e);
			ConfigWatcher.EnableRaisingEvents = true;
		}
		protected virtual void OnReload(object source, FileSystemEventArgs e) { }

		/// <summary>
		/// Creates a new instance of <see cref="ConfigBase"/>.
		/// </summary>
		public ConfigBase(string file)
		{
			// Parse file argument
			if (Path.GetExtension(file).ToLower() != ".json")
				throw new ArgumentException($"Invalid file extension. Expected .json, not {Path.GetExtension(file)}.");
			ConfigFile = file;

			// Set JObject content
			if (File.Exists(file))
			{
				Content = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(file));
				ConfigWatcher = new FileSystemWatcher(Path.GetDirectoryName(Path.GetFullPath(file)));
				ConfigWatcher.Changed += OnChanged;
			}
			if (Content == null) Content = new JObject();
			
			// Run content setup
			Setup();
			ConfigWatcher.EnableRaisingEvents = true;
		}

		/// <summary>
		/// Fills and typechecks the config immediately after loading.
		/// </summary>
		protected abstract void Setup();

		/// <summary>
		/// Writes this config's data to <see cref="ConfigFile"/>.
		/// </summary>
		public void Save()
		{
			ConfigWatcher.EnableRaisingEvents = false;
			var outJson = new JObject(Content.Properties().OrderBy(x => x.Name));
			StreamWriter writer = File.CreateText(ConfigFile);
			writer.Write(JsonConvert.SerializeObject(outJson, Formatting.Indented));
			writer.Dispose();
			ConfigWatcher.EnableRaisingEvents = true;
		}

		/// <summary>
		/// Convenience method that automatically converts <paramref name="json"/> to a <see cref="JObject"/>.
		/// Equivalent to calling <see cref="TryAddItem{T}(JObject, string, T)"/> like so:
		/// <code>TryAddItem((<see cref="JObject"/>)<paramref name="json"/>, <paramref name="key"/>, <paramref name="value"/>);</code>
		/// </summary>
		protected static void TryAddItem<T>(JToken json, string key, T value) => TryAddItem(json as JObject, key, value);
		/// <summary>
		/// Tries to add a key and value to a <see cref="JObject"/> if the value doesn't already exist.
		/// </summary>
		/// <typeparam name="T">Generic type. Instances of <see cref="JToken"/> are added as-is.</typeparam>
		/// <param name="json">The <see cref="JObject"/> to alter.</param>
		/// <param name="key">The key of the value to add.</param>
		/// <param name="value">The value to add to the JObject.</param>
		/// <exception cref="TypeMismatchException">Thrown when the existing value's type is not equal to <typeparamref name="T"/>.</exception>
		protected static void TryAddItem<T>(JObject json, string key, T value)
		{
			if (json.ContainsKey(key)) // Start typechecking
			{
				try
				{
					// Try to cast the value that is already in the JObject
					json.GetValue(key).Value<T>();
					return;
				} catch (Exception)
				{
					// Throw exception if the value could not be cast to T
					string path = json.Path + (json.Path.Length == 0 ? key : $"['{key}']");
					throw new TypeMismatchException($"Incorrect type for value '{path}'. Expected {typeof(T).Name} instead of {json.GetValue(key).Type}.");
				}
			}
			// Add value to JObject
			if (typeof(T).IsSubclassOf(typeof(JToken))) // If value is already a JToken instance, add it directly
				json.Add(key, value as JToken);
			else json.Add(key, new JValue(value)); // Add a generic JValue to the json dict
		}

		public override string ToString() => Content.ToString();

		public static explicit operator JObject(ConfigBase config) => new JObject(config.Content);
	}
}
