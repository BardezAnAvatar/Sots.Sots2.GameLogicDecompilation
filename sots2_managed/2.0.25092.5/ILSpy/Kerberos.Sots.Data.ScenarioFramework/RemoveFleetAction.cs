using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class RemoveFleetAction : TriggerAction
	{
		internal const string XmlRemoveFleetActionName = "RemoveFleetAction";
		private const string XmlFleetNameName = "FleetName";
		public string FleetName = "";
		public override string XmlName
		{
			get
			{
				return "RemoveFleetAction";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.FleetName, "FleetName", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.FleetName = XmlHelper.GetData<string>(node, "FleetName");
		}
	}
}
