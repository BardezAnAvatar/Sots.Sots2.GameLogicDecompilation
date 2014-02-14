using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Colony : IXmlLoadSave
	{
		internal const string XmlColonyName = "Colony";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitIdName = "OrbitId";
		private const string XmlImperialPopulationName = "ImperialPopulation";
		private const string XmlCivilianPopulationsName = "CivilianPopulations";
		private const string XmlInfrastructureName = "Infrastructure";
		private const string XmlIsIdealColonyName = "IsIdealColony";
		public int SystemId;
		public int OrbitId;
		public double ImperialPopulation;
		public double Infrastructure;
		public bool IsIdealColony;
		public List<CivilianPopulation> CivilianPopulations = new List<CivilianPopulation>();
		public string XmlName
		{
			get
			{
				return "Colony";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.OrbitId, "OrbitId", ref node);
			XmlHelper.AddNode(this.ImperialPopulation, "ImperialPopulation", ref node);
			XmlHelper.AddNode(this.Infrastructure, "Infrastructure", ref node);
			XmlHelper.AddNode(this.IsIdealColony, "IsIdealColony", ref node);
			XmlHelper.AddObjectCollectionNode(this.CivilianPopulations, "CivilianPopulations", "CivilianPopulation", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitId = XmlHelper.GetData<int>(node, "OrbitId");
			this.ImperialPopulation = XmlHelper.GetData<double>(node, "ImperialPopulation");
			this.Infrastructure = XmlHelper.GetData<double>(node, "Infrastructure");
			this.IsIdealColony = XmlHelper.GetData<bool>(node, "IsIdealColony");
			this.CivilianPopulations = XmlHelper.GetDataObjectCollection<CivilianPopulation>(node, "CivilianPopulations", "CivilianPopulation");
		}
	}
}
