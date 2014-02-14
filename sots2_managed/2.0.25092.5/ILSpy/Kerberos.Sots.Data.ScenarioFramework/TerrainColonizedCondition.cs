using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TerrainColonizedCondition : TriggerCondition
	{
		internal const string XmlTerrainColonizedConditionName = "TerrainColonized";
		private const string XmlTerrainNameName = "TerrainName";
		private const string XmlColonizedPercentageName = "ColonizedPercentage";
		public string TerrainName = "";
		public float ColonizedPercentage;
		public override string XmlName
		{
			get
			{
				return "TerrainColonized";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.TerrainName, "TerrainName", ref node);
			XmlHelper.AddNode(this.ColonizedPercentage, "ColonizedPercentage", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.TerrainName = XmlHelper.GetData<string>(node, "TerrainName");
			this.ColonizedPercentage = XmlHelper.GetData<float>(node, "ColonizedPercentage");
		}
	}
}
