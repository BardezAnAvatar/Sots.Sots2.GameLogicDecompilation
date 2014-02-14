using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class EmptyOrbit : Orbit
	{
		public const string XmlEmptyOrbitName = "Empty";
		public override string XmlName
		{
			get
			{
				return "Empty";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				base.LoadFromXmlNode(node);
			}
		}
	}
}
