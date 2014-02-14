using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ChangeTreasuryAction : TriggerAction
	{
		internal const string XmlChangeTreasuryActionName = "ChangedTreasury";
		private const string XmlPlayerName = "Player";
		private const string XmlAmountToAddName = "AmountToAdd";
		public int Player;
		public float AmountToAdd;
		public override string XmlName
		{
			get
			{
				return "ChangedTreasury";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Player, "Player", ref node);
			XmlHelper.AddNode(this.AmountToAdd, "AmountToAdd", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.AmountToAdd = XmlHelper.GetData<float>(node, "AmountToAdd");
		}
	}
}
