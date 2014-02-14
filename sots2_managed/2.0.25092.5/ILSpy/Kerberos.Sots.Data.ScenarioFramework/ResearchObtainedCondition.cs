using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ResearchObtainedCondition : TriggerCondition
	{
		internal const string XmlResearchObtainedConditionName = "ResearchObtained";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlTechIdName = "TechId";
		public int PlayerSlot;
		public string TechId = "";
		public override string XmlName
		{
			get
			{
				return "ResearchObtained";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.TechId, "TechId", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.TechId = XmlHelper.GetData<string>(node, "TechId");
		}
	}
}
