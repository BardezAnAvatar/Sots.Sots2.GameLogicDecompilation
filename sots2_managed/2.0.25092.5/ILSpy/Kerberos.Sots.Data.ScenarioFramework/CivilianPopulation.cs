using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class CivilianPopulation : IXmlLoadSave
	{
		internal const string XmlCivilianPopulationName = "CivilianPopulation";
		private const string XmlFactionName = "Faction";
		private const string XmlPopulationName = "Population";
		public string Faction = "";
		public double Population;
		public string XmlName
		{
			get
			{
				return "CivilianPopulation";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Faction, "Faction", ref node);
			XmlHelper.AddNode(this.Population, "Population", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Faction = XmlHelper.GetData<string>(node, "Faction");
			this.Population = XmlHelper.GetData<double>(node, "Population");
		}
	}
}
