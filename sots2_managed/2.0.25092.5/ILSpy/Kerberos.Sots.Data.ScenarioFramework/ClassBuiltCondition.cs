using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ClassBuiltCondition : TriggerCondition
	{
		internal const string XmlClassBuiltConditionName = "ClassBuilt";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlClassName = "Class";
		public int PlayerSlot;
		public string Class = "";
		public override string XmlName
		{
			get
			{
				return "ClassBuilt";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.Class, "Class", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.Class = XmlHelper.GetData<string>(node, "Class");
		}
	}
}
