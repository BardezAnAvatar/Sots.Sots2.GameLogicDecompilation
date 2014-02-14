using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class GrandMenaceDestroyedCondition : TriggerCondition
	{
		internal const string XmlGrandMenaceDestroyedConditionName = "GrandMenaceDestroyed";
		private const string XmlMenaceIdName = "MenaceId";
		public int MenaceId;
		public override string XmlName
		{
			get
			{
				return "GrandMenaceDestroyed";
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
