using Kerberos.Sots.Data.GenericFramework;
using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class Tech : IXmlLoadSave, IComparable
	{
		private enum TechThreatLevel
		{
			TECHTHREAT_LOW,
			TECHTHREAT_MEDIUM,
			TECHTHREAT_HIGH,
			TECHTHREAT_NUMLEVELS
		}
		public struct TechThreatDamage
		{
			public bool m_bRequiresSystem;
			public int m_ProgressMin;
			public int m_ProgressMax;
			public float m_PopulationMin;
			public float m_PopulationMax;
			public float m_InfraMin;
			public float m_InfraMax;
		}
		internal const string XmlTechName = "Tech";
		private const string XmlIdName = "Id";
		private const string XmlNameName = "Name";
		private const string XmlFamilyName = "Family";
		private const string XmlRequirementsName = "Requires";
		private const string XmlRequiredTechName = "RequiredTech";
		private const string XmlAllowablesName = "Allowables";
		private const string XmlIconName = "Icon";
		private const string XmlGroupName = "Group";
		private const string XmlValueName = "Value";
		private const string XmlCostMultiplierName = "CostMultiplier";
		private const string XmlDangerLevelName = "DangerLevel";
		private const string XmlAllowAIRebellionName = "AllowAIRebellion";
		private const string XmlAIResearchModesName = "AIResearchModes";
		private const string XmlAIResearchModeName = "AIResearchMode";
		public string Name = "";
		public string Id = "";
		public string Family = "";
		public List<BasicNameField> Requires = new List<BasicNameField>();
		public List<Allowable> Allows = new List<Allowable>();
		public string Icon = "";
		public string Group = "";
		public float Value;
		public float CostMultiplier;
		public int DangerLevel = 1;
		public bool AllowAIRebellion;
		public AICostFactors AICostFactors = AICostFactors.Default;
		public List<BasicNameField> AIResearchModes = new List<BasicNameField>();
		private static Tech.TechThreatDamage[] _techthreatdamages = new Tech.TechThreatDamage[]
		{
			new Tech.TechThreatDamage
			{
				m_bRequiresSystem = false,
				m_ProgressMin = 50,
				m_ProgressMax = 100,
				m_PopulationMin = 0f,
				m_PopulationMax = 0f,
				m_InfraMin = 0f,
				m_InfraMax = 0f
			},
			new Tech.TechThreatDamage
			{
				m_bRequiresSystem = true,
				m_ProgressMin = 100,
				m_ProgressMax = 100,
				m_PopulationMin = 0.01f,
				m_PopulationMax = 0.05f,
				m_InfraMin = 0.01f,
				m_InfraMax = 0.05f
			},
			new Tech.TechThreatDamage
			{
				m_bRequiresSystem = true,
				m_ProgressMin = 100,
				m_ProgressMax = 100,
				m_PopulationMin = 0.05f,
				m_PopulationMax = 0.2f,
				m_InfraMin = 0.05f,
				m_InfraMax = 0.25f
			}
		};
		public IEnumerable<AIResearchModes> AIResearchModeEnums
		{
			get
			{
				return 
					from x in this.AIResearchModes
					select (AIResearchModes)Enum.Parse(typeof(AIResearchModes), x.Name);
			}
		}
		public string XmlName
		{
			get
			{
				return "Tech";
			}
		}
		public string GetProperIconPath()
		{
			return Path.Combine("Tech", this.Icon.Replace(".\\", string.Empty));
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Id, "Id", ref node);
			XmlHelper.AddNode(this.Family, "Family", ref node);
			XmlHelper.AddObjectCollectionNode(this.Requires, "Requires", "RequiredTech", ref node);
			XmlHelper.AddObjectCollectionNode(this.Allows, "Allowables", "AllowedTech", ref node);
			XmlHelper.AddNode(this.Icon, "Icon", ref node);
			XmlHelper.AddNode(this.Group, "Group", ref node);
			XmlHelper.AddNode(this.Value, "Value", ref node);
			XmlHelper.AddNode(this.CostMultiplier, "CostMultiplier", ref node);
			XmlHelper.AddNode(this.DangerLevel, "DangerLevel", ref node);
			XmlHelper.AddNode(this.AllowAIRebellion, "AllowAIRebellion", ref node);
			this.AICostFactors.AttachToXmlNode(ref node);
			XmlHelper.AddObjectCollectionNode(this.AIResearchModes, "AIResearchModes", "AIResearchMode", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Id = XmlHelper.GetData<string>(node, "Id");
			this.Family = XmlHelper.GetData<string>(node, "Family");
			this.Requires = XmlHelper.GetDataObjectCollection<BasicNameField>(node, "Requires", "RequiredTech");
			this.Allows = XmlHelper.GetDataObjectCollection<Allowable>(node, "Allowables", "AllowedTech");
			this.Icon = XmlHelper.GetData<string>(node, "Icon");
			this.Group = XmlHelper.GetData<string>(node, "Group");
			this.Value = XmlHelper.GetData<float>(node, "Value");
			this.CostMultiplier = XmlHelper.GetDataOrDefault<float>(node["CostMultiplier"], 1f);
			this.DangerLevel = XmlHelper.GetDataOrDefault<int>(node["DangerLevel"], 1);
			this.AllowAIRebellion = XmlHelper.GetDataOrDefault<bool>(node["AllowAIRebellion"], false);
			this.AICostFactors.LoadFromXmlNode(node);
			this.AIResearchModes = XmlHelper.GetDataObjectCollection<BasicNameField>(node, "AIResearchModes", "AIResearchMode");
		}
		public override string ToString()
		{
			return this.Id;
		}
		public int CompareTo(object obj)
		{
			if (obj is Tech)
			{
				return this.Id.CompareTo(((Tech)obj).Id);
			}
			throw new ArgumentException();
		}
		public static Tech.TechThreatDamage GetTechThreatDamage(int threat)
		{
			return Tech._techthreatdamages[threat];
		}
	}
}
