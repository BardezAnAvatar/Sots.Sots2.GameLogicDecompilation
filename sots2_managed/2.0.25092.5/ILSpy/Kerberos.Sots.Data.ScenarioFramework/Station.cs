using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Station : IXmlLoadSave
	{
		internal const string XmlStationName = "Station";
		private const string XmlNameName = "Name";
		private const string XmlTypeName = "Type";
		private const string XmlStageName = "Stage";
		private const string XmlLocationName = "Location";
		private const string XmlOrbitName = "Orbit";
		private const string XmlModulesName = "Modules";
		public string Name;
		public string Type;
		public int Stage;
		public int Location;
		public int Orbit;
		public List<ModuleMount> Modules = new List<ModuleMount>();
		public string XmlName
		{
			get
			{
				return "Station";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Type, "Type", ref node);
			XmlHelper.AddNode(this.Stage, "Stage", ref node);
			XmlHelper.AddNode(this.Location, "Location", ref node);
			XmlHelper.AddNode(this.Orbit, "Orbit", ref node);
			XmlHelper.AddObjectCollectionNode(this.Modules, "Modules", "Module", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Type = XmlHelper.GetData<string>(node, "Type");
			this.Stage = XmlHelper.GetData<int>(node, "Stage");
			this.Location = XmlHelper.GetData<int>(node, "Location");
			this.Orbit = XmlHelper.GetData<int>(node, "Orbit");
			this.Modules = XmlHelper.GetDataObjectCollection<ModuleMount>(node, "Modules", "Module");
		}
	}
}
