using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class BattleRiderMount : IXmlLoadSave
	{
		public string NodeName = "";
		public List<BattleRiderType> AllowableTypes = new List<BattleRiderType>();
		internal static readonly string XmlNameBattleRiderMount = "BattleRiderMount";
		private static readonly string XmlNameNodeName = "NodeName";
		private static readonly string XmlNameAllowableTypes = "AllowableTypes";
		private static readonly string XmlNameBattleRiderType = "BattleRiderType";
		public string XmlName
		{
			get
			{
				return BattleRiderMount.XmlNameBattleRiderMount;
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.NodeName, BattleRiderMount.XmlNameNodeName, ref node);
			XmlHelper.AddObjectCollectionNode(this.AllowableTypes, BattleRiderMount.XmlNameAllowableTypes, BattleRiderMount.XmlNameBattleRiderType, ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.NodeName = XmlHelper.GetData<string>(node, BattleRiderMount.XmlNameNodeName);
			this.AllowableTypes = XmlHelper.GetDataObjectCollection<BattleRiderType>(node, BattleRiderMount.XmlNameAllowableTypes, BattleRiderMount.XmlNameBattleRiderType);
		}
	}
}
