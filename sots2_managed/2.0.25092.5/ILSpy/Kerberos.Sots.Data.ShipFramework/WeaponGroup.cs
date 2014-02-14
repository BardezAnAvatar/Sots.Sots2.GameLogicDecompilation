using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class WeaponGroup : IXmlLoadSave
	{
		internal const string XmlWeaponGroupName = "WeaponGroup";
		private const string XmlNameName = "Name";
		public string Name = "";
		public string XmlName
		{
			get
			{
				return "WeaponGroup";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
		}
	}
}
