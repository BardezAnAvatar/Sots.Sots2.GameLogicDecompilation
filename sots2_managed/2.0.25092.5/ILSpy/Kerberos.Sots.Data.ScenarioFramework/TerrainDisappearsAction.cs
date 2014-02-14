using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TerrainDisappearsAction : TriggerAction
	{
		internal const string XmlTerrainDisappearsActionName = "TerrainDisappears";
		private const string XmlTerrainNameName = "TerrainName";
		public string TerrainName = "";
		public override string XmlName
		{
			get
			{
				return "TerrainDisappears";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.TerrainName, "TerrainName", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.TerrainName = XmlHelper.GetData<string>(node, "TerrainName");
		}
	}
}
