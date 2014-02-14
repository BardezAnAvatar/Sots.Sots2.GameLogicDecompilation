using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.ShipFramework
{
	internal static class SectionLibrary
	{
		private static IEnumerable<ShipSectionAsset> LoadXml(AssetDatabase assetdb, string filename, string faction)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			XmlElement xmlElement = xmlDocument["SectionList"];
			XmlElement source = xmlElement["Sections"];
			foreach (XmlElement current in 
				from x in source.OfType<XmlElement>()
				where x.Name == "Section"
				select x)
			{
				string text = PathHelpers.FixSeparators(current.GetAttribute("File"));
				ShipSectionAsset shipSectionAsset = new ShipSectionAsset
				{
					FileName = text
				};
				shipSectionAsset.LoadFromXml(assetdb, text, faction, (ShipSectionType)Enum.Parse(typeof(ShipSectionType), current.GetAttribute("Type")), (ShipClass)Enum.Parse(typeof(ShipClass), current.GetAttribute("Class")));
				yield return shipSectionAsset;
			}
			yield break;
		}
		public static IEnumerable<ShipSectionAsset> Enumerate(AssetDatabase assetdb)
		{
			try
			{
				string[] array = ScriptHost.FileSystem.FindDirectories("factions\\*");
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					string text2 = PathHelpers.Combine(new string[]
					{
						text,
						"sections\\_sections.xml"
					});
					if (ScriptHost.FileSystem.FileExists(text2))
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
						foreach (ShipSectionAsset current in SectionLibrary.LoadXml(assetdb, text2, fileNameWithoutExtension))
						{
							yield return current;
						}
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
