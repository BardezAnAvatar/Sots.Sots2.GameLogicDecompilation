using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class ArtifactOrbit : Orbit
	{
		public const string XmlArtifactOrbitName = "Artifact";
		public override string XmlName
		{
			get
			{
				return "Artifact";
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
