using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class SystemRangeCondition : TriggerCondition
	{
		internal const string XmlSystemRangeConditionName = "SystemRange";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlPlanetName = "SystemId";
		private const string XmlDistanceName = "Distance";
		public int PlayerSlot;
		public int SystemId;
		public float Distance;
		public override string XmlName
		{
			get
			{
				return "SystemRange";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.Distance, "Distance", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.Distance = XmlHelper.GetData<float>(node, "Distance");
		}
	}
}
