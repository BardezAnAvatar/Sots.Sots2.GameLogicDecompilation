using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TerrainRangeCondition : TriggerCondition
	{
		internal const string XmlTerrainRangeConditionName = "TerrainRange";
		private const string XmlTerrainNameName = "TerrainName";
		private const string XmlDistanceName = "Distance";
		public string TerrainName = "";
		public float Distance;
		public override string XmlName
		{
			get
			{
				return "TerrainRange";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.TerrainName, "TerrainName", ref node);
			XmlHelper.AddNode(this.Distance, "Distance", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.TerrainName = XmlHelper.GetData<string>(node, "TerrainName");
			this.Distance = XmlHelper.GetData<float>(node, "Distance");
		}
	}
}
