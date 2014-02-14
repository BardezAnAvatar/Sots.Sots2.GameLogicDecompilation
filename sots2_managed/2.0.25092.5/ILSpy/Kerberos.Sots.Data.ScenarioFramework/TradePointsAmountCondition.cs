using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TradePointsAmountCondition : TriggerCondition
	{
		internal const string XmlTradePointsAmountConditionName = "TradePointsAmount";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlTradePointAmountName = "TradePointAmount";
		public int PlayerSlot;
		public float TradePointAmount;
		public override string XmlName
		{
			get
			{
				return "TradePointsAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.TradePointAmount, "TradePointAmount", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.TradePointAmount = XmlHelper.GetData<float>(node, "TradePointAmount");
		}
	}
}
