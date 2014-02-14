using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.WeaponFramework
{
	public class WeaponXmlUtility
	{
		private const string XmlWeaponName = "Weapon";
		public static void LoadWeaponFromXml(string filename, ref Weapon w)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			w.LoadFromXmlNode(xmlDocument["Weapon"]);
		}
		public static void LoadWeaponFromXmlForTools(string filename, ref Weapon w)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			w.LoadFromXmlNode(xmlDocument["Weapon"]);
		}
		public static void SaveWeaponToXmlForTools(string filename, Weapon w)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement newChild = xmlDocument.CreateElement("Weapon");
			w.AttachToXmlNode(ref newChild);
			xmlDocument.AppendChild(newChild);
			xmlDocument.Save(filename);
		}
	}
}
