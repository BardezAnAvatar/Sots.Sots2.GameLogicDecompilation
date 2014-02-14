using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TriggerTriggeredCondition : TriggerCondition
	{
		internal const string XmlTriggerTriggeredConditionName = "TriggerTriggered";
		private const string XmlTriggerNameName = "TriggerName";
		public string TriggerName;
		public override string XmlName
		{
			get
			{
				return "TriggerTriggered";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.TriggerName, "TriggerName", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.TriggerName = XmlHelper.GetData<string>(node, "TriggerName");
		}
	}
}
