using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class GameOverCondition : EventTriggerCondition
	{
		internal const string XmlGameOverConditionName = "GameOver";
		private const string XmlReasonName = "Reason";
		public string Reason = "";
		public override string XmlName
		{
			get
			{
				return "GameOver";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Reason, "Reason", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Reason = XmlHelper.GetData<string>(node, "Reason");
		}
	}
}
