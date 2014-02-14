using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ScenarioXmlUtility
	{
		private const string XmlScenarioName = "Scenario";
		public static void LoadScenarioFromXml(string filename, ref Scenario s)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			s.LoadFromXmlNode(xmlDocument["Scenario"]);
		}
		public static void LoadScenarioFromXmlForTools(string filename, ref Scenario s)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			s.LoadFromXmlNode(xmlDocument["Scenario"]);
		}
		public static void SaveScenarioToXmlForTools(string filename, Scenario s)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement newChild = xmlDocument.CreateElement("Scenario");
			s.AttachToXmlNode(ref newChild);
			xmlDocument.AppendChild(newChild);
			xmlDocument.Save(filename);
		}
	}
}
