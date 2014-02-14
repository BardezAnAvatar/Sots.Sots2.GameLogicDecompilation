using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
namespace Kerberos.Sots.Data.GenericFramework
{
	public static class DataHelpers
	{
		public static IEnumerable<string> EnumerateFactionFileNamesForTools(string gameRoot, string factionName)
		{
			foreach (string current in Directory.EnumerateDirectories(Path.Combine(gameRoot, "factions")))
			{
				if (factionName == null || Path.GetFileName(current) == factionName)
				{
					string text = Path.Combine(current, "faction.xml");
					if (File.Exists(text))
					{
						yield return text;
					}
				}
			}
			yield break;
		}
		public static IEnumerable<string> EnumerateRaceFileNames()
		{
			try
			{
				string[] array = ScriptHost.FileSystem.FindDirectories("races\\*");
				for (int i = 0; i < array.Length; i++)
				{
					string path = array[i];
					string text = Path.Combine(path, "race.xml");
					if (ScriptHost.FileSystem.FileExists(text))
					{
						yield return text;
					}
				}
			}
			finally
			{
			}
			yield break;
		}
	}
}
