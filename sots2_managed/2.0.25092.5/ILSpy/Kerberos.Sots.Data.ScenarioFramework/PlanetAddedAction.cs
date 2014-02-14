using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlanetAddedAction : TriggerAction
	{
		internal const string XmlPlanetAddedActionName = "PlanetAdded";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlNewPlanetName = "NewPlanet";
		public int SystemId;
		public PlanetOrbit NewPlanet;
		public override string XmlName
		{
			get
			{
				return "PlanetAdded";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.NewPlanet, "NewPlanet", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.NewPlanet.LoadFromXmlNode(node["NewPlanet"]);
		}
	}
}
