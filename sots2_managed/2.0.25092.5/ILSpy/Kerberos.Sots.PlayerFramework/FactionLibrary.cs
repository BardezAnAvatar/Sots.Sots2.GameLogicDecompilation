using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.PlayerFramework
{
	internal static class FactionLibrary
	{
		public static IEnumerable<Faction> Enumerate()
		{
			HashSet<string> hashSet = new HashSet<string>(
				from x in ScriptHost.FileSystem.FindDirectories("factions\\*")
				select FileSystemHelpers.StripMountName(x));
			foreach (string current in hashSet)
			{
				string text = Path.Combine(current, "faction.xml");
				if (ScriptHost.FileSystem.FileExists(text))
				{
					Faction faction = Faction.LoadXml(text);
					yield return faction;
				}
			}
			yield break;
		}
	}
}
