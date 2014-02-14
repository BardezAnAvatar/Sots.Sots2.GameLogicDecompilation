using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Fleet : IXmlLoadSave
	{
		internal const string XmlFleetName = "Fleet";
		private const string XmlNameName = "Name";
		private const string XmlAdmiralName = "Admiral";
		private const string XmlSupportingStationName = "SupportingStation";
		private const string XmlLocationName = "Location";
		private const string XmlShipsName = "Ships";
		public string Name;
		public string Admiral;
		public string SupportingStation;
		public int Location;
		public List<Ship> Ships = new List<Ship>();
		public string XmlName
		{
			get
			{
				return "Fleet";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Admiral, "Admiral", ref node);
			XmlHelper.AddNode(this.SupportingStation, "SupportingStation", ref node);
			XmlHelper.AddNode(this.Location, "Location", ref node);
			XmlHelper.AddObjectCollectionNode(this.Ships, "Ships", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Admiral = XmlHelper.GetData<string>(node, "Admiral");
			this.SupportingStation = XmlHelper.GetData<string>(node, "SupportingStation");
			this.Location = XmlHelper.GetData<int>(node, "Location");
			this.Ships = XmlHelper.GetDataObjectCollection<Ship>(node, "Ships", "Ship");
		}
	}
}
