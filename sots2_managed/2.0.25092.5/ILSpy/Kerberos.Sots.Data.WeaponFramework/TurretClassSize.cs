using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.WeaponFramework
{
	public class TurretClassSize : IXmlLoadSave
	{
		internal const string XmlTurretClassSizeName = "TurretClassSize";
		private const string XmlTurretSizeName = "TurretSize";
		private const string XmlTurretName = "Turret";
		private const string XmlBarrelName = "Barrel";
		private const string XmlBaseName = "Base";
		public string TurretSize = "";
		public string Turret = "";
		public string Barrel = "";
		public string Base = "";
		public string XmlName
		{
			get
			{
				return "TurretClassSize";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.TurretSize, "TurretSize", ref node);
			XmlHelper.AddNode(this.Turret, "Turret", ref node);
			XmlHelper.AddNode(this.Barrel, "Barrel", ref node);
			XmlHelper.AddNode(this.Base, "Base", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.TurretSize = XmlHelper.GetData<string>(node, "TurretSize");
			this.Turret = XmlHelper.GetData<string>(node, "Turret");
			this.Barrel = XmlHelper.GetData<string>(node, "Barrel");
			this.Base = XmlHelper.GetData<string>(node, "Base");
		}
	}
}
