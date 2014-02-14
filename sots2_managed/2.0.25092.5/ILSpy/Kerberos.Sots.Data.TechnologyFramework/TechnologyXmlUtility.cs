using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class TechnologyXmlUtility
	{
		public static void LoadTechTreeFromXml(string filename, ref TechTree tt)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			tt.LoadFromXmlNode(xmlDocument["TechTree"]);
		}
		public static void LoadTechTreeFromXmlForTools(string filename, ref TechTree tt)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			tt.LoadFromXmlNode(xmlDocument["TechTree"]);
		}
		public static void SaveTechTreeToXmlForTools(string filename, TechTree tt)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement newChild = xmlDocument.CreateElement("TechTree");
			tt.AttachToXmlNode(ref newChild);
			xmlDocument.AppendChild(newChild);
			xmlDocument.Save(filename);
		}
	}
}
