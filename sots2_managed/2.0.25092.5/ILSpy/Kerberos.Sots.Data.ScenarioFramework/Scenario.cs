using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Scenario : IXmlLoadSave
	{
		public class ScenarioInfo
		{
			public string FileName;
			public string Title;
			public Starmap.StarmapInfo StarmapInfo;
			public string GetFallbackTitle()
			{
				if (!string.IsNullOrEmpty(this.Title))
				{
					return this.Title;
				}
				return Path.GetFileNameWithoutExtension(this.FileName);
			}
			public ScenarioInfo(string filename)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(ScriptHost.FileSystem, filename);
				XmlElement xmlElement = xmlDocument["Scenario"];
				this.FileName = filename;
				this.Title = xmlElement["Name"].ExtractStringOrDefault(string.Empty);
				this.StarmapInfo = new Starmap.StarmapInfo(xmlElement["Starmap"].ExtractStringOrDefault(string.Empty));
			}
		}
		internal const string XmlScenarioName = "Scenario";
		private const string XmlNameName = "Name";
		private const string XmlStarmapName = "Starmap";
		private const string XmlPlayerStartConditionsName = "PlayerStartConditions";
		private const string XmlDiplomacyRulesName = "DiplomacyRules";
		private const string XmlTriggersName = "Triggers";
		private const string XmlEconomicEfficiencyName = "EconomicEfficiency";
		private const string XmlResearchEfficiencyName = "ResearchEfficiency";
		public string Name = "";
		public string Starmap = "";
		public int EconomicEfficiency;
		public int ResearchEfficiency;
		public List<Player> PlayerStartConditions = new List<Player>();
		public List<DiplomacyRule> DiplomacyRules = new List<DiplomacyRule>();
		public List<Trigger> Triggers = new List<Trigger>();
		public string XmlName
		{
			get
			{
				return "Scenario";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Starmap, "Starmap", ref node);
			XmlHelper.AddNode(this.EconomicEfficiency, "EconomicEfficiency", ref node);
			XmlHelper.AddNode(this.ResearchEfficiency, "ResearchEfficiency", ref node);
			XmlHelper.AddObjectCollectionNode(this.PlayerStartConditions, "PlayerStartConditions", ref node);
			XmlHelper.AddObjectCollectionNode(this.DiplomacyRules, "DiplomacyRules", ref node);
			XmlHelper.AddObjectCollectionNode(this.Triggers, "Triggers", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Starmap = XmlHelper.GetData<string>(node, "Starmap");
			this.EconomicEfficiency = XmlHelper.GetDataOrDefault<int>(node["EconomicEfficiency"], 100);
			this.ResearchEfficiency = XmlHelper.GetDataOrDefault<int>(node["ResearchEfficiency"], 100);
			this.PlayerStartConditions = XmlHelper.GetDataObjectCollection<Player>(node, "PlayerStartConditions", "Player");
			this.DiplomacyRules = XmlHelper.GetDataObjectCollection<DiplomacyRule>(node, "DiplomacyRules", "DiplomacyRule");
			this.Triggers = XmlHelper.GetDataObjectCollection<Trigger>(node, "Triggers", "Trigger");
		}
	}
}
