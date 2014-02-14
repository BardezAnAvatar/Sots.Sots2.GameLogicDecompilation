using Kerberos.Sots.Data.GenericFramework;
using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class TechTree : IXmlLoadSave
	{
		internal const string XmlTechTreeName = "TechTree";
		private const string XmlTechnologiesName = "Technologies";
		private const string XmlTechGroupsName = "TechGroups";
		private const string XmlTechFamiliesName = "TechFamilies";
		private const string XmlGroupName = "Group";
		public List<Tech> Technologies = new List<Tech>();
		public List<BasicNameField> TechGroups = new List<BasicNameField>();
		public List<TechFamily> TechFamilies = new List<TechFamily>();
		public Tech this[string techId]
		{
			get
			{
				return this.Technologies.First((Tech x) => techId == x.Id);
			}
		}
		public string XmlName
		{
			get
			{
				return "TechTree";
			}
		}
		public TechFamilies GetTechFamilyEnumFromName(string techFamilyName)
		{
			TechFamily techFamily = this.TechFamilies.First((TechFamily x) => x.Id == techFamilyName);
			return (TechFamilies)Enum.Parse(typeof(TechFamilies), techFamily.Name);
		}
		public TechFamilies GetTechFamilyEnum(Tech tech)
		{
			return this.GetTechFamilyEnumFromName(tech.Family);
		}
		private bool IsRoot(Tech value)
		{
			return !this.Technologies.Any((Tech x) => x.Allows.Any((Allowable y) => y.Id == value.Id));
		}
		public Tech[] GetRoots()
		{
			return (
				from x in this.Technologies
				where this.IsRoot(x)
				select x).ToArray<Tech>();
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddObjectCollectionNode(this.Technologies, "Technologies", ref node);
			XmlHelper.AddObjectCollectionNode(this.TechGroups, "TechGroups", "Group", ref node);
			XmlHelper.AddObjectCollectionNode(this.TechFamilies, "TechFamilies", "Family", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Technologies = XmlHelper.GetDataObjectCollection<Tech>(node, "Technologies", "Tech");
			this.TechGroups = XmlHelper.GetDataObjectCollection<BasicNameField>(node, "TechGroups", "Group");
			this.TechFamilies = XmlHelper.GetDataObjectCollection<TechFamily>(node, "TechFamilies", "Family");
		}
	}
}
