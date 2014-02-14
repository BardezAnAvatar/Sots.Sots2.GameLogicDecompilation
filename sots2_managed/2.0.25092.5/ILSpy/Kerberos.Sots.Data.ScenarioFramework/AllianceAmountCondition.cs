using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AllianceAmountCondition : TriggerCondition
	{
		internal const string XmlAllianceAmountConditionName = "AllianceAmount";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlNumAlliancesName = "NumAlliances";
		public int PlayerSlot;
		public int NumberOfAlliances;
		public override string XmlName
		{
			get
			{
				return "AllianceAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.NumberOfAlliances, "NumAlliances", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.NumberOfAlliances = XmlHelper.GetData<int>(node, "NumAlliances");
		}
	}
}
