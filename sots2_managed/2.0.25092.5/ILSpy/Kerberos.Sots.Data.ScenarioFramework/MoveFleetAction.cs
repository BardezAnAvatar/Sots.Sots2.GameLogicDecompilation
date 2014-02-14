using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class MoveFleetAction : TriggerAction
	{
		internal const string XmlMoveFleetActionName = "MoveFleet";
		private const string XmlFleetNameName = "Name";
		private const string XmlDestinationIdName = "DestinationId";
		public string FleetName = "";
		public int DestinationId;
		public override string XmlName
		{
			get
			{
				return "MoveFleet";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.FleetName, "Name", ref node);
			XmlHelper.AddNode(this.DestinationId, "DestinationId", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.FleetName = XmlHelper.GetData<string>(node, "Name");
			this.DestinationId = XmlHelper.GetData<int>(node, "DestinationId");
		}
	}
}
