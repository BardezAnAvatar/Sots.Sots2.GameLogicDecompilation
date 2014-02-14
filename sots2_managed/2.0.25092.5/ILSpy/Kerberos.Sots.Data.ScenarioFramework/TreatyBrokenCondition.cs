using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TreatyBrokenCondition : TriggerCondition
	{
		internal const string XmlTreatyBrokenConditionName = "TreatyBroken";
		private const string XmlTreatyIdName = "TreatyId";
		public int TreatyId;
		public override string XmlName
		{
			get
			{
				return "TreatyBroken";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.TreatyId, "TreatyId", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.TreatyId = XmlHelper.GetData<int>(node, "TreatyId");
		}
	}
}
