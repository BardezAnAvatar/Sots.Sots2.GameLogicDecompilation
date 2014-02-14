using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlayerDeathCondition : EventTriggerCondition
	{
		internal const string XmlPlayerDeathConditionName = "PlayerDeath";
		private const string XmlPlayerSlotName = "PlayerSlot";
		public int PlayerSlot;
		public override string XmlName
		{
			get
			{
				return "PlayerDeath";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
		}
	}
}
