using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class ShipOptionGroup : IXmlLoadSave
	{
		public List<ShipOption> ShipOptions = new List<ShipOption>();
		internal static readonly string XmlNameShipOptionGroup = "ShipOptionGroup";
		private static readonly string XmlNameShipOptions = "ShipOptions";
		private static readonly string XmlNameShipOption = "ShipOption";
		public string XmlName
		{
			get
			{
				return ShipOptionGroup.XmlNameShipOptionGroup;
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddObjectCollectionNode(this.ShipOptions, ShipOptionGroup.XmlNameShipOptions, ShipOptionGroup.XmlNameShipOption, ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.ShipOptions = XmlHelper.GetDataObjectCollection<ShipOption>(node, ShipOptionGroup.XmlNameShipOptions, ShipOptionGroup.XmlNameShipOption);
		}
	}
}
