using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class RemoveWeaponAction : TriggerAction
	{
		internal const string XmlRemoveWeaponActionName = "RemoveWeapon";
		private const string XmlPlayerName = "Player";
		private const string XmlWeaponFileName = "WeaponFile";
		public int Player;
		public string WeaponFile = "";
		public override string XmlName
		{
			get
			{
				return "RemoveWeapon";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Player, "Player", ref node);
			XmlHelper.AddNode(this.WeaponFile, "WeaponFile", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.WeaponFile = XmlHelper.GetData<string>(node, "WeaponFile");
		}
	}
}
