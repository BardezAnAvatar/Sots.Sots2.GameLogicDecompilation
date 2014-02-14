using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class SurrenderSystemAction : TriggerAction
	{
		internal const string XmlSurrenderSystemActionName = "SurrenderSystem";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlNewPlayerName = "NewPlayer";
		public int SystemId;
		public int NewPlayer;
		public override string XmlName
		{
			get
			{
				return "SurrenderSystem";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.NewPlayer, "NewPlayer", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.NewPlayer = XmlHelper.GetData<int>(node, "NewPlayer");
		}
	}
}
