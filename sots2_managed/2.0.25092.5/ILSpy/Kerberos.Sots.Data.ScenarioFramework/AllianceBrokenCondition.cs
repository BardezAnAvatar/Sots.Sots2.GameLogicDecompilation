using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AllianceBrokenCondition : TriggerCondition
	{
		internal const string XmlAllianceBrokenConditionName = "AllianceBroken";
		private const string XmlPlayerSlot1Name = "PlayerSlot1";
		private const string XmlPlayerSlot2Name = "PlayerSlot2";
		public int PlayerSlot1;
		public int PlayerSlot2;
		public override string XmlName
		{
			get
			{
				return "AllianceBroken";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot1, "PlayerSlot1", ref node);
			XmlHelper.AddNode(this.PlayerSlot2, "PlayerSlot2", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot1 = XmlHelper.GetData<int>(node, "PlayerSlot1");
			this.PlayerSlot2 = XmlHelper.GetData<int>(node, "PlayerSlot2");
		}
	}
}
