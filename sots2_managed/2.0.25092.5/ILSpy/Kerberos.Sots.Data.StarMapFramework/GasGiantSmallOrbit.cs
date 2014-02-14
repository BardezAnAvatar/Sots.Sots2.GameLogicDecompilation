using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class GasGiantSmallOrbit : Orbit
	{
		public const string XmlGasGiantSmallName = "GasGiantSmall";
		public const string XmlSizeName = "Size";
		public const string XmlMaterialName = "MaterialName";
		private float? _size;
		public string MaterialName = "";
		public float? Size
		{
			get
			{
				return this._size;
			}
			set
			{
				if (value.HasValue && value.Value > 0f)
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
				return "GasGiantSmall";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode(this.Size, "Size", ref node);
			XmlHelper.AddNode(this.MaterialName, "MaterialName", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				base.LoadFromXmlNode(node);
				this.Size = XmlHelper.GetData<float?>(node, "Size");
				this.MaterialName = XmlHelper.GetData<string>(node, "MaterialName");
			}
		}
	}
}
