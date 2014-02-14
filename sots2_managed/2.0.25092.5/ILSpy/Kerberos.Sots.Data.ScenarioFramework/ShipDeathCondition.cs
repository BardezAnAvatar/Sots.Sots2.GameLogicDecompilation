using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.ShipFramework;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ShipDeathCondition : EventTriggerCondition
	{
		internal const string XmlShipDeathConditionName = "ShipDeath";
		private const string XmlShipClassName = "Name";
		public ShipClass ShipClass;
		public override string XmlName
		{
			get
			{
				return "ShipDeath";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.ShipClass, "Name", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			string data = XmlHelper.GetData<string>(node, "Name");
			Enum.TryParse<ShipClass>(data, out this.ShipClass);
		}
	}
}
