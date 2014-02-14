using Kerberos.Sots.Data.GenericFramework;
using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class ShipOption : BasicNameField, IXmlLoadSave
	{
		public bool AvailableByDefault;
		private static readonly string XmlNameAvailableByDefault = "AvailableByDefault";
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.AvailableByDefault, ShipOption.XmlNameAvailableByDefault, ref node);
			base.AttachToXmlNode(ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.AvailableByDefault = XmlHelper.GetData<bool>(node, ShipOption.XmlNameAvailableByDefault);
			base.LoadFromXmlNode(node);
		}
	}
}
