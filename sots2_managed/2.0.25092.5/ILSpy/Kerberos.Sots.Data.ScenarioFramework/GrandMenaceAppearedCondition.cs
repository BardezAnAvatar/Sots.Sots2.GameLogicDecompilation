using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class GrandMenaceAppearedCondition : TriggerCondition
	{
		internal const string XmlGrandMenaceAppearedConditionName = "GrandMenaceAppeared";
		private const string XmlMenaceIdName = "MenaceId";
		public int MenaceId;
		public override string XmlName
		{
			get
			{
				return "GrandMenaceAppeared";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.MenaceId, "MenaceId", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.MenaceId = XmlHelper.GetData<int>(node, "MenaceId");
		}
	}
}
