using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class PlanetOrbit : Orbit
	{
		public const string XmlPlanetOrbitName = "Planet";
		public const string XmlSizeName = "Size";
		public const string XmlSuitabilityName = "Suitability";
		public const string XmlResourcesName = "Resources";
		public const string XmlPlanetTypeName = "PlanetType";
		public const string XmlInhabitedByPlayerName = "InhabitedByPlayer";
		public const string XmlCapitalOrbitName = "CapitalOrbit";
		public const string XmlBiosphereName = "Biosphere";
		public const string XmlMaterialName = "MaterialName";
		private int? _size;
		public float? Suitability;
		public int? Resources;
		public string PlanetType = "";
		public int? InhabitedByPlayer;
		public bool CapitalOrbit;
		public int? Biosphere;
		public string MaterialName = "";
		public int? Size
		{
			get
			{
				return this._size;
			}
			set
			{
				if (value.HasValue && value.Value > 0)
				{
					this._size = value;
					return;
				}
				this._size = null;
			}
		}
		public override string XmlName
		{
			get
			{
				return "Planet";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode(this.Size, "Size", ref node);
			XmlHelper.AddNode(this.Suitability, "Suitability", ref node);
			XmlHelper.AddNode(this.Resources, "Resources", ref node);
			XmlHelper.AddNode(this.PlanetType, "PlanetType", ref node);
			XmlHelper.AddNode(this.InhabitedByPlayer, "InhabitedByPlayer", ref node);
			XmlHelper.AddNode(this.CapitalOrbit, "CapitalOrbit", ref node);
			XmlHelper.AddNode(this.Biosphere, "Biosphere", ref node);
			XmlHelper.AddNode(this.MaterialName, "MaterialName", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				base.LoadFromXmlNode(node);
				this.Size = XmlHelper.GetData<int?>(node, "Size");
				int? data = XmlHelper.GetData<int?>(node, "Suitability");
				this.Suitability = (data.HasValue ? new float?((float)data.GetValueOrDefault()) : null);
				this.Resources = XmlHelper.GetData<int?>(node, "Resources");
				this.PlanetType = XmlHelper.GetData<string>(node, "PlanetType");
				this.InhabitedByPlayer = XmlHelper.GetData<int?>(node, "InhabitedByPlayer");
				this.CapitalOrbit = XmlHelper.GetData<bool>(node, "CapitalOrbit");
				this.Biosphere = XmlHelper.GetData<int?>(node, "Biosphere");
				this.MaterialName = XmlHelper.GetData<string>(node, "MaterialName");
			}
		}
	}
}
