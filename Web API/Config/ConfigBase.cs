﻿using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Config
{
	public abstract class ConfigBase
	{
		/// <summary>
		/// Gets or sets a section from the config with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns>A JObject with a key equal to <paramref name="key"/>.</returns>
		public JToken this[string key]
		{
			get { return Content[key]; }
			set { Content[key] = value; }
		}

		/// <summary>
		/// The path of the file this <see cref="ConfigBase"/> uses.
		/// </summary>
		public string ConfigFile { get; }
		/// <summary>
		/// The raw JSON data of this <see cref="ConfigBase"/>.
		/// </summary>
		private JObject Content { get; }

		/// <summary>
		/// Creates a new instance of <see cref="ConfigBase"/>.
		/// </summary>
		public ConfigBase(string file)
		{
			if (Path.GetExtension(file).ToLower() != ".json")
				throw new ArgumentException($"Invalid file extension. Expected .json, not {Path.GetExtension(file)}.");
			ConfigFile = file;
			if (File.Exists(file))
				Content = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(file));
			Setup();
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
			using JsonTextWriter writer = new JsonTextWriter(File.CreateText(ConfigFile));
		}

		/// <summary>
		/// Tries to add an item to the <see cref="JObject"/>. If it already exists, a typecheck will be done to see if it matches type T.
		/// If it does not match type T, then it will be replaced.
		/// </summary>
		/// <typeparam name="T">Generic type extending object.</typeparam>
		/// <param name="json">The <see cref="JObject"/> to alter.</param>
		/// <param name="key">The key of the value to add.</param>
		/// <param name="value">The value to add to the JObject.</param>
		protected static void TryAddItem<T>(JObject json, string key, T value)
		{
			if (json.ContainsKey(key)) // Start typechecking
			{
				try
				{
					// Get and cast the value that is already in the JObject
					JToken _value = json.GetValue(key);
					return;
				} catch (Exception) // If it couldn't cast the value to T, remove it and add it again (below).
				{
					json.Remove(key);
				}
			}
			// Add value to JObject
			if (typeof(T).IsSubclassOf(typeof(JToken))) // If value is already a JToken instance, add it directly
			{
				json.Add(key, (JToken)Convert.ChangeType(value, typeof(JToken)));
			}
			else json.Add(key, new JValue(value)); // Add a generic JValue to the json dict
		}
	}
}