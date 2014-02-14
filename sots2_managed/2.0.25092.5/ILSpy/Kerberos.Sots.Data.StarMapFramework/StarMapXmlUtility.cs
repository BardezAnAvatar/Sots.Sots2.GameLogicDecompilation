using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.IO;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class StarMapXmlUtility
	{
		public static void SaveStarmapToXmlForTools(string filename, Starmap s)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement newChild = xmlDocument.CreateElement("Starmap");
			s.AttachToXmlNode(ref newChild);
			xmlDocument.AppendChild(newChild);
			xmlDocument.Save(filename);
		}
		public static void LoadStarmapFromXmlForTools(string filename, ref Starmap s)
		{
			if (File.Exists(filename))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(filename);
				s.LoadFromXmlNode(xmlDocument["Starmap"]);
			}
		}
		public static void LoadStarmapFromXml(string filename, ref Starmap s)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			s.LoadFromXmlNode(xmlDocument["Starmap"]);
		}
	}
}
