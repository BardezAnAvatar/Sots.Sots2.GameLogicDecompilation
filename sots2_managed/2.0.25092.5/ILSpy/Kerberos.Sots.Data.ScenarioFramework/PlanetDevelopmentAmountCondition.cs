using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlanetDevelopmentAmountCondition : TriggerCondition
	{
		internal const string XmlPlanetDevelopmentAmountConditionName = "PlanetDevelopmentAmount";
		public override string XmlName
		{
			get
			{
				return "PlanetDevelopmentAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
		}
	}
}
