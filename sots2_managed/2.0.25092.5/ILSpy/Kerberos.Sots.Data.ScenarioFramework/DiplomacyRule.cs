using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class DiplomacyRule : IXmlLoadSave
	{
		internal const string XmlDiplomacyRuleName = "DiplomacyRule";
		private const string XmlPlayer1Name = "Player1";
		private const string XmlPlayer2Name = "Player2";
		private const string XmlRuleName = "Rule";
		private const string XmlLockedName = "Locked";
		public int Player1;
		public int Player2;
		public string Rule;
		public bool Locked;
		public string XmlName
		{
			get
			{
				return "DiplomacyRule";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Player1, "Player1", ref node);
			XmlHelper.AddNode(this.Player2, "Player2", ref node);
			XmlHelper.AddNode(this.Rule, "Rule", ref node);
			XmlHelper.AddNode(this.Locked, "Locked", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Player1 = XmlHelper.GetData<int>(node, "Player1");
			this.Player2 = XmlHelper.GetData<int>(node, "Player2");
			this.Rule = XmlHelper.GetData<string>(node, "Rule");
			this.Locked = XmlHelper.GetData<bool>(node, "Locked");
		}
	}
}
