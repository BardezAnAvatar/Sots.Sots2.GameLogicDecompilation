using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class GovernmentTypeCondition : TriggerCondition
	{
		internal const string XmlGovernmentTypeConditionName = "GovernmentType";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlGovernmentTypeName = "GovernmentType";
		public int PlayerSlot;
		public string GovernmentType = "";
		public override string XmlName
		{
			get
			{
				return "GovernmentType";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.GovernmentType, "GovernmentType", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.GovernmentType = XmlHelper.GetData<string>(node, "GovernmentType");
		}
	}
}
