using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Strategy;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlayerRelation : IXmlLoadSave
	{
		internal const string XmlPlayerRelationName = "PlayerRelation";
		private const string XmlPlayerName = "Player";
		private const string XmlRelationsName = "Relations";
		private const string XmlDiplomacyStateName = "DiplomacyState";
		public int Player;
		public int Relations;
		public DiplomacyState DiplomacyState = DiplomacyState.UNKNOWN;
		public string XmlName
		{
			get
			{
				return "PlayerRelation";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Player, "Player", ref node);
			XmlHelper.AddNode(this.Relations, "Relations", ref node);
			XmlHelper.AddNode(this.DiplomacyState, "DiplomacyState", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.Relations = XmlHelper.GetData<int>(node, "Relations");
			string data = XmlHelper.GetData<string>(node, "DiplomacyState");
			Enum.TryParse<DiplomacyState>(data, out this.DiplomacyState);
		}
	}
}
