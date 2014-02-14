using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class RevelationBeginsCondition : TriggerCondition
	{
		internal const string XmlRevelationBeginsConditionName = "RevelationBegins";
		public override string XmlName
		{
			get
			{
				return "RevelationBegins";
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
