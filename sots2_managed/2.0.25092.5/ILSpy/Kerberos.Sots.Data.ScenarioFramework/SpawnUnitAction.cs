using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class SpawnUnitAction : TriggerAction
	{
		internal const string XmlSpawnUnitActionName = "SpawnUnit";
		private const string XmlFleetNameName = "FleetName";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlShipToAddName = "ShipToAdd";
		public int PlayerSlot;
		public string FleetName = "";
		public int SystemId;
		public Ship ShipToAdd = new Ship();
		public override string XmlName
		{
			get
			{
				return "SpawnUnit";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.FleetName, "FleetName", ref node);
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.ShipToAdd, "ShipToAdd", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.FleetName = XmlHelper.GetData<string>(node, "FleetName");
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.ShipToAdd.LoadFromXmlNode(node["ShipToAdd"]);
		}
	}
}
