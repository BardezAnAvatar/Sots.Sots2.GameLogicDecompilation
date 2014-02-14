using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PointPerShipTypeAction : TriggerAction
	{
		internal const string XmlPointPerShipTypeActionName = "PointPerShipTypeAction";
		private const string XmlFleetName = "Fleet";
		private const string XmlShipTypeName = "ShipType";
		private const string XmlAmountPerShipName = "AmountPerShip";
		private const string XmlScalarNameName = "ScalarName";
		public string Fleet = "";
		public string ShipType = "";
		public float AmountPerShip;
		public string ScalarName = "";
		public override string XmlName
		{
			get
			{
				return "PointPerShipTypeAction";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Fleet, "Fleet", ref node);
			XmlHelper.AddNode(this.ShipType, "ShipType", ref node);
			XmlHelper.AddNode(this.AmountPerShip, "AmountPerShip", ref node);
			XmlHelper.AddNode(this.ScalarName, "ScalarName", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Fleet = XmlHelper.GetData<string>(node, "Fleet");
			this.ShipType = XmlHelper.GetData<string>(node, "ShipType");
			this.AmountPerShip = XmlHelper.GetData<float>(node, "AmountPerShip");
			this.ScalarName = XmlHelper.GetData<string>(node, "ScalarName");
		}
	}
}
