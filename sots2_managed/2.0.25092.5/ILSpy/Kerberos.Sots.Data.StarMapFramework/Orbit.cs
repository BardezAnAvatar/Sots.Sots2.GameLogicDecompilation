using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Orbit : IXmlLoadSave
	{
		public const string XmlOrbitName = "Orbit";
		public const string XmlNameName = "Name";
		public const string XmlTypeName = "Type";
		public const string XmlParentName = "Parent";
		public const string XmlOrbitNumberName = "OrbitNumber";
		public const string XmlEccentricityName = "Eccentricity";
		public const string XmlInclinationName = "Inclination";
		public string Name = "";
		public string Type = "";
		public string Parent = "";
		public float? Eccentricity;
		public float? Inclination;
		public int OrbitNumber;
		public static Dictionary<string, Type> TypeMap = new Dictionary<string, Type>
		{

			{
				"Empty",
				typeof(EmptyOrbit)
			},

			{
				"Star",
				typeof(StarOrbit)
			},

			{
				"Artifact",
				typeof(ArtifactOrbit)
			},

			{
				"GasGiantSmall",
				typeof(GasGiantSmallOrbit)
			},

			{
				"GasGiantLarge",
				typeof(GasGiantLargeOrbit)
			},

			{
				"Moon",
				typeof(MoonOrbit)
			},

			{
				"PlanetaryRing",
				typeof(PlanetaryRingOrbit)
			},

			{
				"Planet",
				typeof(PlanetOrbit)
			},

			{
				"Asteroid",
				typeof(AsteroidOrbit)
			}
		};
		public virtual string XmlName
		{
			get
			{
				return "Orbit";
			}
		}
		public virtual void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Type, "Type", ref node);
			XmlHelper.AddNode(this.Parent, "Parent", ref node);
			XmlHelper.AddNode(this.Eccentricity, "Eccentricity", ref node);
			XmlHelper.AddNode(this.Inclination, "Inclination", ref node);
			XmlHelper.AddNode(this.OrbitNumber, "OrbitNumber", ref node);
		}
		public virtual void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				this.Name = XmlHelper.GetData<string>(node, "Name");
				this.Type = XmlHelper.GetData<string>(node, "Type");
				this.Parent = XmlHelper.GetData<string>(node, "Parent");
				this.Eccentricity = XmlHelper.GetData<float?>(node, "Eccentricity");
				this.Inclination = XmlHelper.GetData<float?>(node, "Inclination");
				this.OrbitNumber = XmlHelper.GetData<int>(node, "OrbitNumber");
			}
		}
	}
}
