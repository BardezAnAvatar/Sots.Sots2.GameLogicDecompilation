using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
namespace Kerberos.Sots.Data
{
	public class CommonStrings : Dictionary<string, string>
	{
		private string _gameRoot;
		private string _forcedLanguage;
		public string TwoLetterISOLanguageName
		{
			get;
			private set;
		}
		public string Directory
		{
			get;
			private set;
		}
		public string UnrootedDirectory
		{
			get;
			private set;
		}
		public void Reload()
		{
			base.Clear();
			string text = this._forcedLanguage ?? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
			if (App.Log != null)
			{
				App.Log.Trace("Reloading common strings for locale: " + text, "data");
			}
			string localeDirectory = CommonStrings.GetLocaleDirectory(text);
			string text2 = Path.Combine(localeDirectory, "strings.csv");
			string text3 = (this._gameRoot != null) ? Path.Combine(this._gameRoot, text2) : text2;
			if ((this._gameRoot != null) ? (!File.Exists(text3)) : (!ScriptHost.FileSystem.FileExists(text2)))
			{
				if (App.Log != null)
				{
					App.Log.Warn("No locale strings.csv found for language '" + text + "'. Defaulting to 'en'.", "data");
				}
				text = "en";
				localeDirectory = CommonStrings.GetLocaleDirectory(text);
				text2 = Path.Combine(localeDirectory, "strings.csv");
				text3 = ((this._gameRoot != null) ? Path.Combine(this._gameRoot, text2) : text2);
			}
			this.TwoLetterISOLanguageName = text;
			this.UnrootedDirectory = localeDirectory;
			this.Directory = ((this._gameRoot != null) ? Path.Combine(this._gameRoot, localeDirectory) : localeDirectory);
			this.MergeCsv(text3);
			this.MergeCsv(Path.Combine(this.Directory, "speech.csv"));
		}
		private void Construct(string twoLetterISOLanguageName)
		{
			this._forcedLanguage = twoLetterISOLanguageName;
			this.Reload();
		}
		public CommonStrings(string twoLetterISOLanguageName, string gameRoot)
		{
			this._gameRoot = gameRoot;
			this.Construct(twoLetterISOLanguageName);
		}
		public CommonStrings(string twoLetterISOLanguageName)
		{
			this.Construct(twoLetterISOLanguageName);
		}
		public bool Localize(string id, out string localized)
		{
			localized = string.Empty;
			if (string.IsNullOrEmpty(id))
			{
				return false;
			}
			bool flag = false;
			bool result;
			try
			{
				Monitor.Enter(this, ref flag);
				if (id[0] == '@')
				{
					string key = id.Substring(1);
					string text;
					if (base.TryGetValue(key, out text))
					{
						localized = text;
						result = true;
						return result;
					}
				}
				localized = "*" + id;
				result = false;
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
			return result;
		}
		public string Localize(string id)
		{
			string result;
			this.Localize(id, out result);
			return result;
		}
		private void MergeCsv(string filename)
		{
			IEnumerable<string[]> enumerable;
			if (this._gameRoot != null)
			{
				enumerable = CsvOperations.Read(filename, '"', ',', 0, 2);
			}
			else
			{
				enumerable = CsvOperations.Read(ScriptHost.FileSystem, filename, '"', ',', 0, 2);
			}
			foreach (string[] current in enumerable)
			{
				if (current.Length != 0)
				{
					string text = current[0].Trim();
					if (text.Length != 0)
					{
						if (base.ContainsKey(text))
						{
							throw new InvalidDataException(string.Format("Duplicate ID '{0}' found in '{1}'.", text, filename));
						}
						string value;
						if (current.Length > 1)
						{
							value = current[1].Trim();
						}
						else
						{
							value = string.Empty;
						}
						base[text] = value;
					}
				}
			}
		}
		private static string GetLocaleDirectory(string twoLetterISOLanguageName)
		{
			return Path.Combine(new string[]
			{
				"locale\\" + twoLetterISOLanguageName
			});
		}
		public static string GetRootLocaleDirectory(string gameRoot)
		{
			return Path.Combine(gameRoot, "locale");
		}
	}
}
