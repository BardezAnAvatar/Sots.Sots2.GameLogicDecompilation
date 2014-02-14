using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Bank : IXmlLoadSave
	{
		internal const string XmlBankName = "Bank";
		private const string XmlGUIDName = "GUID";
		private const string XmlNameName = "Name";
		private const string XmlMountSizeName = "MountSize";
		private const string XmlMountClassName = "MountClass";
		private const string XmlWeaponName = "Weapon";
		public string GUID;
		public string Name;
		public string MountSize;
		public string MountClass;
		public string Weapon;
		public string XmlName
		{
			get
			{
				return "Bank";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.GUID, "GUID", ref node);
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.MountSize, "MountSize", ref node);
			XmlHelper.AddNode(this.MountClass, "MountClass", ref node);
			XmlHelper.AddNode(this.Weapon, "Weapon", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.GUID = XmlHelper.GetData<string>(node, "GUID");
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.MountSize = XmlHelper.GetData<string>(node, "MountSize");
			this.MountClass = XmlHelper.GetData<string>(node, "MountClass");
			this.Weapon = XmlHelper.GetData<string>(node, "Weapon");
		}
	}
}
