using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class DiplomacyChangedAction : TriggerAction
	{
		internal const string XmlDiplomacyChangedActionName = "DiplomacyChanged";
		private const string XmlOldDiplomacyRuleName = "OldDiplomacyRule";
		private const string XmlNewDiplomacyRuleName = "NewDiplomacyRule";
		public DiplomacyRule OldDiplomacyRule = new DiplomacyRule();
		public DiplomacyRule NewDiplomacyRule = new DiplomacyRule();
		public override string XmlName
		{
			get
			{
				return "DiplomacyChanged";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.OldDiplomacyRule, "OldDiplomacyRule", ref node);
			XmlHelper.AddNode(this.NewDiplomacyRule, "NewDiplomacyRule", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.OldDiplomacyRule.LoadFromXmlNode(node["OldDiplomacyRule"]);
			this.NewDiplomacyRule.LoadFromXmlNode(node["NewDiplomacyRule"]);
		}
	}
}
