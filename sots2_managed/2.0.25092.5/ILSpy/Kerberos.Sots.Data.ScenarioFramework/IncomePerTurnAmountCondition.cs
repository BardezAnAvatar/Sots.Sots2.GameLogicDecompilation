using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class IncomePerTurnAmountCondition : TriggerCondition
	{
		internal const string XmlIncomePerTurnConditionName = "IncomePerTurn";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlIncomeAmountName = "IncomeAmount";
		public int PlayerSlot;
		public float IncomeAmount;
		public override string XmlName
		{
			get
			{
				return "IncomePerTurn";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.IncomeAmount, "IncomeAmount", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.IncomeAmount = XmlHelper.GetData<float>(node, "IncomeAmount");
		}
	}
}
