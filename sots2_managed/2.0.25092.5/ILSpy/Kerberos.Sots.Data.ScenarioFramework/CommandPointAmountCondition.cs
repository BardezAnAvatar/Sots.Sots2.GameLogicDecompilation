using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class CommandPointAmountCondition : TriggerCondition
	{
		internal const string XmlCommandPointAmountConditionName = "CommandPointAmount";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlCommandPointAmountName = "CommandPointAmount";
		public int PlayerSlot;
		public int CommandPoints;
		public override string XmlName
		{
			get
			{
				return "CommandPointAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.CommandPoints, "CommandPointAmount", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.CommandPoints = XmlHelper.GetData<int>(node, "CommandPointAmount");
		}
	}
}
