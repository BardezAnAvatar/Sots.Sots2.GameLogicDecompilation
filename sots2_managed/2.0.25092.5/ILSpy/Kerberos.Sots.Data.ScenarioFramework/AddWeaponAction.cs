using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AddWeaponAction : TriggerAction
	{
		internal const string XmlAddWeaponActionName = "AddWeapon";
		private const string XmlPlayerName = "Player";
		private const string XmlWeaponFileName = "WeaponFile";
		public int Player;
		public string WeaponFile = "";
		public override string XmlName
		{
			get
			{
				return "AddWeapon";
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
