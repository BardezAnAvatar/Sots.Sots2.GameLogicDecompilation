using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ModuleMount : IXmlLoadSave
	{
		internal const string XmlModuleName = "Module";
		private const string XmlNameName = "Name";
		private const string XmlNodeName = "Node";
		public string NodeName;
		public string ModuleName;
		public string XmlName
		{
			get
			{
				return "Module";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.NodeName, "Name", ref node);
			XmlHelper.AddNode(this.ModuleName, "Node", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.NodeName = XmlHelper.GetData<string>(node, "Name");
			this.ModuleName = XmlHelper.GetData<string>(node, "Node");
		}
	}
}
