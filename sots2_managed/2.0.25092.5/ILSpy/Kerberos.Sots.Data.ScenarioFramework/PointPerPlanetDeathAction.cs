using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PointPerPlanetDeathAction : TriggerAction
	{
		internal const string XmlPointPerPlanetDeathActionName = "PointPerPlanetDeathAction";
		private const string XmlScalarNameName = "Name";
		private const string XmlAmountPerPlanetName = "AmountPerPlanet";
		public string ScalarName = "";
		public float AmountPerPlanet;
		public override string XmlName
		{
			get
			{
				return "PointPerPlanetDeathAction";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.ScalarName, "Name", ref node);
			XmlHelper.AddNode(this.AmountPerPlanet, "AmountPerPlanet", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ScalarName = XmlHelper.GetData<string>(node, "Name");
			this.AmountPerPlanet = XmlHelper.GetData<float>(node, "AmountPerPlanet");
		}
	}
}
