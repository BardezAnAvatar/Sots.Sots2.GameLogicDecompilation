using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Terrain : Feature
	{
		internal const string XmlTerrainName = "Terrain";
		private const string XmlFeaturesName = "Features";
		private const string XmlNodeLinesName = "NodeLines";
		private const string XmlProvincesName = "Provinces";
		public List<Feature> Features = new List<Feature>();
		public List<NodeLine> NodeLines = new List<NodeLine>();
		public List<Province> Provinces = new List<Province>();
		public override string XmlName
		{
			get
			{
				return "Terrain";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddObjectCollectionNode(this.Features, "Features", ref node);
			XmlHelper.AddObjectCollectionNode(this.NodeLines, "NodeLines", "NodeLine", ref node);
			XmlHelper.AddObjectCollectionNode(this.Provinces, "Provinces", "Province", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				base.LoadFromXmlNode(node);
				this.Features = XmlHelper.GetDataObjectCollection<Feature>(node, "Features", Feature.TypeMap);
				this.NodeLines = XmlHelper.GetDataObjectCollection<NodeLine>(node, "NodeLines", "NodeLine");
				this.Provinces = XmlHelper.GetDataObjectCollection<Province>(node, "Provinces", "Province");
			}
		}
	}
}
