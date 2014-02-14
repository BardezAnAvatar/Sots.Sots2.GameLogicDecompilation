using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TerrainAppearsAction : TriggerAction
	{
		internal const string XmlTerrainAppearsActionName = "TerrainAppears";
		private const string XmlTerrainNameName = "Name";
		public string TerrainName = "";
		public override string XmlName
		{
			get
			{
				return "TerrainAppears";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.TerrainName, "Name", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.TerrainName = XmlHelper.GetData<string>(node, "Name");
		}
	}
}
