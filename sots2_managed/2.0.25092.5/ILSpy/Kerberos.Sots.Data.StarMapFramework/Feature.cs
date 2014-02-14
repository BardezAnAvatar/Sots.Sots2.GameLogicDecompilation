using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Feature : IXmlLoadSave
	{
		private const string XmlNameName = "Name";
		private const string XmlLocalSpaceName = "LocalSpace";
		private const string XmlInheritObject = "Inherit";
		private const string XmlIsVisible = "IsVisible";
		private const char XmlMatrixSeperator = ',';
		public string Name;
		public Matrix LocalSpace = Matrix.Identity;
		public bool isVisible = true;
		public string InheritObject;
		public static Dictionary<string, Type> TypeMap = new Dictionary<string, Type>
		{

			{
				"Terrain",
				typeof(Terrain)
			},

			{
				"StellarBody",
				typeof(StellarBody)
			},

			{
				"System",
				typeof(StarSystem)
			}
		};
		public virtual string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}
		public virtual void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.LocalSpace.ToString(','), "LocalSpace", ref node);
			XmlHelper.AddNode(this.InheritObject, "Inherit", ref node);
			XmlHelper.AddNode(this.isVisible, "IsVisible", ref node);
		}
		public virtual void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				this.Name = XmlHelper.GetData<string>(node, "Name");
				this.LocalSpace = Matrix.Parse(XmlHelper.GetData<string>(node, "LocalSpace"), new char[]
				{
					','
				});
				this.InheritObject = XmlHelper.GetData<string>(node, "Inherit");
				this.isVisible = XmlHelper.GetDataOrDefault<bool>(node["IsVisible"], true);
			}
		}
	}
}
