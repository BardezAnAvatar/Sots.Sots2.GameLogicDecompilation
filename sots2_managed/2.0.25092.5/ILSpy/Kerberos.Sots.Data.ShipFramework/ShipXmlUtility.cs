using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class ShipXmlUtility
	{
		public static void LoadShipSectionFromXml(string filename, ref ShipSection ss)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			ss.LoadFromXmlNode(xmlDocument["ShipSection"]);
		}
		public static void LoadShipSectionFromXmlForTools(string filename, ref ShipSection ss)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			ss.LoadFromXmlNode(xmlDocument["ShipSection"]);
		}
		public static void SaveShipSectionToXmlForTools(string filename, ShipSection ss)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement newChild = xmlDocument.CreateElement("ShipSection");
			ss.AttachToXmlNode(ref newChild);
			xmlDocument.AppendChild(newChild);
			xmlDocument.Save(filename);
		}
	}
}
