using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class TerrainLibrary
	{
		internal const string XmlTerrainLibraryName = "TerrainLibrary";
		internal const string XmlFeaturesName = "Features";
		public List<Feature> Features = new List<Feature>();
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddObjectCollectionNode(this.Features, "Features", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				this.Features = XmlHelper.GetDataObjectCollection<Feature>(node, "Features", Feature.TypeMap);
			}
		}
	}
}
