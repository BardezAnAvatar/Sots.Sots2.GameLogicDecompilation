using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class FactionEncounteredCondition : TriggerCondition
	{
		internal const string XmlFactionEncounteredConditionName = "FactionEncountered";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlFactionName = "Faction";
		public int PlayerSlot;
		public string Faction = "";
		public override string XmlName
		{
			get
			{
				return "FactionEncountered";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.Faction, "Faction", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
		}
	}
}
