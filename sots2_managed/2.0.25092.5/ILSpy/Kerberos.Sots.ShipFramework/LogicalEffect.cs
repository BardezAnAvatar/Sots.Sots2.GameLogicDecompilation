using System;
using System.Xml;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalEffect
	{
		public string Name;
		public static LogicalEffect ParseXml(XmlNode node)
		{
			XmlElement xmlElement = (XmlElement)node;
			return new LogicalEffect
			{
				Name = xmlElement.GetAttribute("Name")
			};
		}
	}
}
