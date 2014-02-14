using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AIStrategyChangedAction : TriggerAction
	{
		internal const string XmlAIStrategyChangedActionName = "AIStrategyChanged";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlNewStrategyName = "NewStrategy";
		public int PlayerSlot;
		public string NewStrategy = "";
		public override string XmlName
		{
			get
			{
				return "AIStrategyChanged";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.NewStrategy, "NewStrategy", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.NewStrategy = XmlHelper.GetData<string>(node, "NewStrategy");
		}
	}
}
