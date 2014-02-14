using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class MoralAmountCondition : TriggerCondition
	{
		internal const string XmlMoralAmountConditionName = "MoralAmount";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitNameName = "OrbitName";
		private const string XmlFactionName = "Faction";
		private const string XmlMoralAmountName = "MoralAmount";
		public int SystemId;
		public string OrbitName = "";
		public string Faction = "";
		public float MoralAmount;
		public override string XmlName
		{
			get
			{
				return "MoralAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.OrbitName, "OrbitName", ref node);
			XmlHelper.AddNode(this.Faction, "Faction", ref node);
			XmlHelper.AddNode(this.MoralAmount, "MoralAmount", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitName = XmlHelper.GetData<string>(node, "OrbitName");
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
			this.MoralAmount = XmlHelper.GetData<float>(node, "MoralAmount");
		}
	}
}
