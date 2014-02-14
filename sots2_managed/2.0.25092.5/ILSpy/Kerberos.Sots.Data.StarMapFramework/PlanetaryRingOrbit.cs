using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class PlanetaryRingOrbit : Orbit
	{
		public const string XmlPlanetaryRingOrbitName = "PlanetaryRing";
		public override string XmlName
		{
			get
			{
				return "PlanetaryRing";
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
