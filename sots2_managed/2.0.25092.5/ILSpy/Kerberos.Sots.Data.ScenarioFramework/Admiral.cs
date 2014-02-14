using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Admiral : IXmlLoadSave
	{
		internal const string XmlAdmiralName = "Admiral";
		private const string XmlNameName = "Name";
		private const string XmlPortraitName = "Portrait";
		private const string XmlAgeName = "Age";
		private const string XmlFactionName = "Faction";
		private const string XmlGenderName = "Gender";
		private const string XmlHomePlanetName = "HomePlanet";
		private const string XmlReactionRatingName = "ReactionRating";
		private const string XmlEvasionRatingName = "EvasionRating";
		private const string XmlSpecialCharacteristicsName = "SpecialCharacteristics";
		private const string XmlSpecialCharacteristicName = "Characteristic";
		public string Name = "";
		public string Portrait = "";
		public int Age;
		public string Faction = "";
		public string Gender = "";
		public int HomePlanet;
		public float ReactionRating;
		public float EvasionRating;
		public List<SpecialCharacteristic> SpecialCharacteristics = new List<SpecialCharacteristic>();
		public string XmlName
		{
			get
			{
				return "Admiral";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Portrait, "Portrait", ref node);
			XmlHelper.AddNode(this.Age, "Age", ref node);
			XmlHelper.AddNode(this.Faction, "Faction", ref node);
			XmlHelper.AddNode(this.Gender, "Gender", ref node);
			XmlHelper.AddNode(this.HomePlanet, "HomePlanet", ref node);
			XmlHelper.AddNode(this.ReactionRating, "ReactionRating", ref node);
			XmlHelper.AddNode(this.EvasionRating, "EvasionRating", ref node);
			XmlHelper.AddCollectionNode<SpecialCharacteristic>(this.SpecialCharacteristics, "SpecialCharacteristics", "Characteristic", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Portrait = XmlHelper.GetData<string>(node, "Portrait");
			this.Age = XmlHelper.GetData<int>(node, "Age");
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
			this.Gender = XmlHelper.GetData<string>(node, "Gender");
			this.HomePlanet = XmlHelper.GetData<int>(node, "HomePlanet");
			this.ReactionRating = XmlHelper.GetData<float>(node, "ReactionRating");
			this.EvasionRating = XmlHelper.GetData<float>(node, "EvasionRating");
			this.SpecialCharacteristics = XmlHelper.GetDataCollection<SpecialCharacteristic>(node, "SpecialCharacteristics", "Characteristic");
		}
	}
}
