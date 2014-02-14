using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ModuleFramework
{
	public class ShipModuleXmlUtility
	{
		private const string XmlShipModuleName = "ShipModule";
		public static void LoadShipModuleFromXml(string filename, ref ShipModule sm)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			sm.LoadFromXmlNode(xmlDocument["ShipModule"]);
			sm.SavePath = filename;
		}
		public static void LoadShipModuleFromXmlForTools(string filename, ref ShipModule sm)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			sm.LoadFromXmlNode(xmlDocument["ShipModule"]);
			sm.SavePath = filename;
		}
		public static void SaveShipModuleToXmlForTools(string filename, ShipModule ss)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement newChild = xmlDocument.CreateElement("ShipModule");
			ss.AttachToXmlNode(ref newChild);
			xmlDocument.AppendChild(newChild);
			xmlDocument.Save(filename);
		}
	}
}
