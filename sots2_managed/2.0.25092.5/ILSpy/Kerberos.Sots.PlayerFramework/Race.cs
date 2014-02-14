using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.IO;
using System.Xml;
namespace Kerberos.Sots.PlayerFramework
{
	internal class Race
	{
		public string Name;
		public string RaceFileName;
		public string Directory;
		public string AssetPath;
		private XmlElement Variables;
		public string GetVariable(string name)
		{
			if (this.Variables == null)
			{
				return null;
			}
			XmlElement xmlElement = this.Variables[name];
			if (xmlElement == null)
			{
				return null;
			}
			return xmlElement.InnerText.ToString();
		}
		public void LoadXml(string filename, string name)
		{
			this.RaceFileName = filename;
			this.Directory = Path.GetDirectoryName(filename);
			this.Name = name;
			this.AssetPath = Path.Combine("races", this.Name);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			this.Variables = xmlDocument["Race"]["Variables"];
		}
	}
}
