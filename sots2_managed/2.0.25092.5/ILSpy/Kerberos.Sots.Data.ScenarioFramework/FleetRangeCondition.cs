using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class FleetRangeCondition : TriggerCondition
	{
		internal const string XmlFleetRangeConditionName = "FleetRange";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlFleetNameName = "FleetName";
		private const string XmlDistanceName = "Distance";
		public int PlayerSlot;
		public string FleetName = "";
		public float Distance;
		public override string XmlName
		{
			get
			{
				return "FleetRange";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.FleetName, "FleetName", ref node);
			XmlHelper.AddNode(this.Distance, "Distance", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.FleetName = XmlHelper.GetData<string>(node, "FleetName");
			this.Distance = (float)XmlHelper.GetData<int>(node, "Distance");
		}
	}
}
