using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class WorldTypeCondition : TriggerCondition
	{
		internal const string XmlWorldTypeConditionName = "WorldType";
		public override string XmlName
		{
			get
			{
				return "WorldType";
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
