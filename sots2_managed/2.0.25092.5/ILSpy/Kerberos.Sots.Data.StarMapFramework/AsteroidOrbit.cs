using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class AsteroidOrbit : Orbit
	{
		public const string XmlAsteroidOrbitName = "Asteroid";
		public const string XmlDensityName = "Density";
		public const string XmlWidthName = "Width";
		public int? Density;
		public int? Width;
		public override string XmlName
		{
			get
			{
				return "Asteroid";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode(this.Density, "Density", ref node);
			XmlHelper.AddNode(this.Width, "Width", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				base.LoadFromXmlNode(node);
				this.Density = XmlHelper.GetData<int?>(node, "Density");
				this.Width = XmlHelper.GetData<int?>(node, "Width");
			}
		}
	}
}
