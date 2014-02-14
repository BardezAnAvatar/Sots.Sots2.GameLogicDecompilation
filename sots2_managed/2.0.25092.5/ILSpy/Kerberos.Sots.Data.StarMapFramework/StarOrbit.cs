using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class StarOrbit : Orbit
	{
		public const string XmlStarOrbitName = "Star";
		public string StellarClass
		{
			get;
			set;
		}
		public override string XmlName
		{
			get
			{
				return "Star";
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
