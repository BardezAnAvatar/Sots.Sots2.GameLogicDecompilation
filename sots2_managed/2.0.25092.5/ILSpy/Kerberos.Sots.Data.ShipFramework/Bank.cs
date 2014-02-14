using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class Bank : IXmlLoadSave
	{
		public string Id = string.Empty;
		public string Size = WeaponEnums.WeaponSizes.Light.ToString();
		public string Class;
		public string WeaponGroup = "";
		public string DefaultWeapon = "";
		public int FrameX;
		public int FrameY;
		public List<Mount> Mounts = new List<Mount>();
		internal static readonly string XmlNameBank = "Bank";
		private static readonly string XmlNameId = "Id";
		private static readonly string XmlNameTurretSize = "TurretSize";
		private static readonly string XmlNameTurretClass = "TurretClass";
		private static readonly string XmlNameWeaponGroup = "WeaponGroup";
		private static readonly string XmlNameDefaultWeapon = "DefaultWeapon";
		private static readonly string XmlNameMounts = "Mounts";
		private static readonly string XmlNameFrameX = "FrameX";
		private static readonly string XmlNameFrameY = "FrameY";
		public string XmlName
		{
			get
			{
				return Bank.XmlNameBank;
			}
		}
		public Bank()
		{
			if (ToolEnvironment.IsRunningInTool)
			{
				this.Id = Guid.NewGuid().ToString();
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Id, Bank.XmlNameId, ref node);
			XmlHelper.AddNode(this.Size, Bank.XmlNameTurretSize, ref node);
			XmlHelper.AddNode(this.Class, Bank.XmlNameTurretClass, ref node);
			XmlHelper.AddNode(this.WeaponGroup, Bank.XmlNameWeaponGroup, ref node);
			XmlHelper.AddNode(this.DefaultWeapon, Bank.XmlNameDefaultWeapon, ref node);
			XmlHelper.AddNode(this.FrameX, Bank.XmlNameFrameX, ref node);
			XmlHelper.AddNode(this.FrameY, Bank.XmlNameFrameY, ref node);
			XmlHelper.AddObjectCollectionNode(this.Mounts, Bank.XmlNameMounts, Mount.XmlNameMount, ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Id = XmlHelper.GetData<string>(node, Bank.XmlNameId);
			this.Size = XmlHelper.GetData<string>(node, Bank.XmlNameTurretSize);
			this.Class = XmlHelper.GetData<string>(node, Bank.XmlNameTurretClass);
			this.WeaponGroup = XmlHelper.GetData<string>(node, Bank.XmlNameWeaponGroup);
			this.DefaultWeapon = XmlHelper.GetData<string>(node, Bank.XmlNameDefaultWeapon);
			this.FrameX = XmlHelper.GetData<int>(node, Bank.XmlNameFrameX);
			this.FrameY = XmlHelper.GetData<int>(node, Bank.XmlNameFrameY);
			this.Mounts = XmlHelper.GetDataObjectCollection<Mount>(node, Bank.XmlNameMounts, Mount.XmlNameMount);
			if (ToolEnvironment.IsRunningInTool && string.IsNullOrEmpty(this.Id))
			{
				this.Id = Guid.NewGuid().ToString();
			}
		}
	}
}
