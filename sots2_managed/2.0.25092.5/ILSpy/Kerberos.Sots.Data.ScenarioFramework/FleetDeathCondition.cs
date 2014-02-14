using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class FleetDeathCondition : EventTriggerCondition
	{
		internal const string XmlFleetDeathConditionName = "FleetDeath";
		private const string XmlFleetNameName = "Name";
		public string FleetName = "";
		public override string XmlName
		{
			get
			{
				return "FleetDeath";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.FleetName, "Name", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			XmlHelper.GetData<string>(node, "Name");
		}
	}
}
