using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlanetColonizedCondition : TriggerCondition
	{
		internal const string XmlPlanetColonizedConditionName = "PlanetColonized";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitNameName = "OrbitName";
		public int SystemId;
		public string OrbitName = "";
		public override string XmlName
		{
			get
			{
				return "PlanetColonized";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.OrbitName, "OrbitName", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitName = XmlHelper.GetData<string>(node, "OrbitName");
		}
	}
}
