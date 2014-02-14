using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class FleetAmountCondition : TriggerCondition
	{
		internal const string XmlFleetAmountConditionName = "FleetAmount";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlNumFleetsName = "NumFleets";
		public int PlayerSlot;
		public int NumberOfFleets;
		public override string XmlName
		{
			get
			{
				return "FleetAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.NumberOfFleets, "NumFleets", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.NumberOfFleets = XmlHelper.GetData<int>(node, "NumFleets");
		}
	}
}
