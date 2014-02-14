using System;
using System.IO;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class TerrainLibXmlUtility
	{
		public static void SaveTerrainLibToXml(string filename, TerrainLibrary tl)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement newChild = xmlDocument.CreateElement("TerrainLibrary");
			tl.AttachToXmlNode(ref newChild);
			xmlDocument.AppendChild(newChild);
			xmlDocument.Save(filename);
		}
		public static void LoadTerrainLibFromXml(string filename, ref TerrainLibrary tl)
		{
			if (File.Exists(filename))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(filename);
				tl.LoadFromXmlNode(xmlDocument["TerrainLibrary"]);
			}
		}
	}
}
