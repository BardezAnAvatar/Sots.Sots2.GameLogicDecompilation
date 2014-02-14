using Kerberos.Sots.Data.GenericFramework;
using System;
using System.Collections.Generic;
using System.IO;
namespace Kerberos.Sots.PlayerFramework
{
	internal static class RaceLibrary
	{
		public static IEnumerable<Race> Enumerate()
		{
			foreach (string current in DataHelpers.EnumerateRaceFileNames())
			{
				Race race = new Race();
				race.LoadXml(current, Path.GetFileNameWithoutExtension(Path.GetDirectoryName(current)));
				yield return race;
			}
			yield break;
		}
	}
}
