using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Section : IXmlLoadSave
	{
		internal const string XmlSectionName = "Section";
		private const string XmlSectionFileName = "SectionFile";
		private const string XmlBanksName = "Banks";
		private const string XmlModulesName = "Modules";
		public string SectionFile = "";
		public List<Bank> Banks = new List<Bank>();
		public List<ModuleMount> Modules = new List<ModuleMount>();
		public string XmlName
		{
			get
			{
				return "Section";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SectionFile, "SectionFile", ref node);
			XmlHelper.AddObjectCollectionNode(this.Banks, "Banks", ref node);
			XmlHelper.AddObjectCollectionNode(this.Modules, "Modules", "Module", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.SectionFile = XmlHelper.GetData<string>(node, "SectionFile");
			this.Banks = XmlHelper.GetDataObjectCollection<Bank>(node, "Banks", "Bank");
			this.Modules = XmlHelper.GetDataObjectCollection<ModuleMount>(node, "Modules", "Module");
		}
	}
}
