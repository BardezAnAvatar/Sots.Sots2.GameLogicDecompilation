using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AdmiralChangedAction : TriggerAction
	{
		internal const string XmlAdmiralChangedActionName = "AdmiralChanged";
		private const string XmlOldAdmiralName = "OldAdmiral";
		private const string XmlNewAdmiralName = "NewAdmiral";
		private const string XmlNewPlayerName = "NewPlayer";
		public Admiral OldAdmiral = new Admiral();
		public Admiral NewAdmiral = new Admiral();
		public int NewPlayer;
		public override string XmlName
		{
			get
			{
				return "AdmiralChanged";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.OldAdmiral, "OldAdmiral", ref node);
			XmlHelper.AddNode(this.NewAdmiral, "NewAdmiral", ref node);
			XmlHelper.AddNode(this.NewPlayer, "NewPlayer", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.OldAdmiral.LoadFromXmlNode(node["OldAdmiral"]);
			this.NewAdmiral.LoadFromXmlNode(node["NewAdmiral"]);
			this.NewPlayer = XmlHelper.GetData<int>(node, "NewPlayer");
		}
	}
}
