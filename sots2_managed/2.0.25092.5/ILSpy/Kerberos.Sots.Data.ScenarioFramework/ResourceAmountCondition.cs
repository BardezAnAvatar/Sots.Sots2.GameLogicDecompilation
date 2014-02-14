using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ResourceAmountCondition : TriggerCondition
	{
		internal const string XmlResourceAmountConditionName = "ResourceAmount";
		private const string XmlResourceAmountName = "ResourceAmount";
		public int ResourceAmount;
		public override string XmlName
		{
			get
			{
				return "ResourceAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.ResourceAmount, "ResourceAmount", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ResourceAmount = XmlHelper.GetData<int>(node, "ResourceAmount");
		}
	}
}
