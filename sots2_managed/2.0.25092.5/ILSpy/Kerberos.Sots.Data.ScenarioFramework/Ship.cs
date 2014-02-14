using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Ship : IXmlLoadSave
	{
		internal const string XmlShipName = "Ship";
		private const string XmlNameName = "Name";
		private const string XmlClassName = "Class";
		private const string XmlFactionFame = "Faction";
		private const string XmlAvailableToPlayerName = "AvailableToPlayer";
		private const string XmlSectionsName = "Sections";
		public string Name = "";
		public string Class = "";
		public string Faction = "";
		public bool AvailableToPlayer;
		public List<Section> Sections = new List<Section>();
		public string XmlName
		{
			get
			{
				return "Ship";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Class, "Class", ref node);
			XmlHelper.AddNode(this.Faction, "Faction", ref node);
			XmlHelper.AddNode(this.AvailableToPlayer, "AvailableToPlayer", ref node);
			XmlHelper.AddObjectCollectionNode(this.Sections, "Sections", "Section", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Class = XmlHelper.GetData<string>(node, "Class");
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
			this.AvailableToPlayer = XmlHelper.GetData<bool>(node, "AvailableToPlayer");
			this.Sections = XmlHelper.GetDataObjectCollection<Section>(node, "Sections", "Section");
		}
	}
}
