using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Player : IXmlLoadSave
	{
		internal const string XmlPlayerName = "Player";
		private const string XmlNameName = "Name";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlFactionName = "Faction";
		private const string XmlResourcesName = "Resources";
		private const string XmlTreasuryName = "Treasury";
		private const string XmlIsAIName = "IsAI";
		private const string XmlIsAIRebellionName = "IsAIRebellion";
		private const string XmlAIDifficultyName = "AIDifficulty";
		private const string XmlAvatarName = "Avatar";
		private const string XmlBadgeName = "Badge";
		private const string XmlEmpireColorName = "EmpireColor";
		private const string XmlShipColorName = "ShipColor";
		private const string XmlFleetsName = "Fleets";
		private const string XmlShipDesignsName = "ShipDesigns";
		private const string XmlStationsName = "Stations";
		private const string XmlAdmiralsName = "Admirals";
		private const string XmlStartingTechsName = "StartingTechs";
		private const string XmlAvailableTechsName = "AvailableTechs";
		private const string XmlTechName = "Tech";
		private const string XmlRelationsName = "Relations";
		private const string XmlColoniesName = "Colonies";
		public string Name = "";
		public int PlayerSlot;
		public string Faction = "";
		public float Treasury;
		public bool isAI;
		public bool isAIRebellion;
		public string AIDifficulty = "";
		public string Avatar = "";
		public string Badge = "";
		public Vector3 EmpireColor;
		public Vector3 ShipColor;
		public List<Fleet> Fleets = new List<Fleet>();
		public List<Ship> ShipDesigns = new List<Ship>();
		public List<Station> Stations = new List<Station>();
		public List<Admiral> Admirals = new List<Admiral>();
		public List<Tech> StartingTechs = new List<Tech>();
		public List<Tech> AvailableTechs = new List<Tech>();
		public List<PlayerRelation> Relations = new List<PlayerRelation>();
		public List<Colony> Colonies = new List<Colony>();
		public string XmlName
		{
			get
			{
				return "Player";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.Faction, "Faction", ref node);
			XmlHelper.AddNode(this.Treasury, "Treasury", ref node);
			XmlHelper.AddNode(this.isAI, "IsAI", ref node);
			XmlHelper.AddNode(this.isAIRebellion, "IsAIRebellion", ref node);
			XmlHelper.AddNode(this.AIDifficulty, "AIDifficulty", ref node);
			XmlHelper.AddNode(this.Avatar, "Avatar", ref node);
			XmlHelper.AddNode(this.Badge, "Badge", ref node);
			XmlHelper.AddNode(this.EmpireColor.ToString(), "EmpireColor", ref node);
			XmlHelper.AddNode(this.ShipColor.ToString(), "ShipColor", ref node);
			XmlHelper.AddObjectCollectionNode(this.Fleets, "Fleets", ref node);
			XmlHelper.AddObjectCollectionNode(this.ShipDesigns, "ShipDesigns", ref node);
			XmlHelper.AddObjectCollectionNode(this.Stations, "Stations", ref node);
			XmlHelper.AddObjectCollectionNode(this.Admirals, "Admirals", ref node);
			XmlHelper.AddObjectCollectionNode(this.StartingTechs, "StartingTechs", "Tech", ref node);
			XmlHelper.AddObjectCollectionNode(this.AvailableTechs, "AvailableTechs", "Tech", ref node);
			XmlHelper.AddObjectCollectionNode(this.Relations, "Relations", ref node);
			XmlHelper.AddObjectCollectionNode(this.Colonies, "Colonies", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
			this.Treasury = XmlHelper.GetData<float>(node, "Treasury");
			this.isAI = XmlHelper.GetData<bool>(node, "IsAI");
			this.isAIRebellion = XmlHelper.GetData<bool>(node, "IsAIRebellion");
			this.AIDifficulty = XmlHelper.GetData<string>(node, "AIDifficulty");
			this.Avatar = XmlHelper.GetData<string>(node, "Avatar");
			this.Badge = XmlHelper.GetData<string>(node, "Badge");
			this.EmpireColor = Vector3.Parse(XmlHelper.GetData<string>(node, "EmpireColor"));
			this.ShipColor = Vector3.Parse(XmlHelper.GetData<string>(node, "ShipColor"));
			this.Fleets = XmlHelper.GetDataObjectCollection<Fleet>(node, "Fleets", "Fleet");
			this.ShipDesigns = XmlHelper.GetDataObjectCollection<Ship>(node, "ShipDesigns", "Ship");
			this.Stations = XmlHelper.GetDataObjectCollection<Station>(node, "Stations", "Station");
			this.Admirals = XmlHelper.GetDataObjectCollection<Admiral>(node, "Admirals", "Admiral");
			this.StartingTechs = XmlHelper.GetDataObjectCollection<Tech>(node, "StartingTechs", "Tech");
			this.AvailableTechs = XmlHelper.GetDataObjectCollection<Tech>(node, "AvailableTechs", "Tech");
			this.Relations = XmlHelper.GetDataObjectCollection<PlayerRelation>(node, "Relations", "PlayerRelation");
			this.Colonies = XmlHelper.GetDataObjectCollection<Colony>(node, "Colonies", "Colony");
		}
	}
}
