using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AdmiralDeathCondition : EventTriggerCondition
	{
		internal const string XmlAdmiralDeathConditionName = "AdmiralDeath";
		private const string XmlAdmiralNameName = "AdmiralName";
		public string AdmiralName = "";
		public override string XmlName
		{
			get
			{
				return "AdmiralDeath";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.AdmiralName, "AdmiralName", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.AdmiralName = XmlHelper.GetData<string>(node, "AdmiralName");
		}
	}
}
