using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class SurrenderEmpireAction : TriggerAction
	{
		internal const string XmlSurrenderEmpireActionName = "SurrenderEmpire";
		private const string XmlSurrenderingPlayerName = "SurrenderingPlayer";
		private const string XmlCapturingPlayerName = "CapturingPlayer";
		public int SurrenderingPlayer;
		public int CapturingPlayer;
		public override string XmlName
		{
			get
			{
				return "SurrenderEmpire";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SurrenderingPlayer, "SurrenderingPlayer", ref node);
			XmlHelper.AddNode(this.CapturingPlayer, "CapturingPlayer", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SurrenderingPlayer = XmlHelper.GetData<int>(node, "SurrenderingPlayer");
			this.CapturingPlayer = XmlHelper.GetData<int>(node, "CapturingPlayer");
		}
	}
}
