using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class Mount : IXmlLoadSave
	{
		public string NodeName = "";
		public string TurretOverload = "";
		public string BarrelOverload = "";
		public string BaseOverload = "";
		public float YawMin;
		public float YawMax;
		public float PitchMin;
		public float PitchMax;
		public string SectionFireAnimation = "";
		public string SectionReloadAnimation = "";
		internal static readonly string XmlNameMount = "Mount";
		private static readonly string XmlNameNodeName = "NodeName";
		private static readonly string XmlNameTurretOverloadName = "TurretOverload";
		private static readonly string XmlNameBarrelOverloadName = "BarrelOverload";
		private static readonly string XmlNameBaseOverloadName = "BaseOverload";
		private static readonly string XmlNameYawMin = "YawMin";
		private static readonly string XmlNameYawMax = "YawMax";
		private static readonly string XmlNamePitchMin = "PitchMin";
		private static readonly string XmlNamePitchMax = "PitchMax";
		private static readonly string XmlNameSectionFireAnimation = "SectionFireAnimation";
		private static readonly string XmlNameSectionReloadAnimation = "SectionReloadAnimation";
		public string XmlName
		{
			get
			{
				return Mount.XmlNameMount;
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.NodeName, Mount.XmlNameNodeName, ref node);
			XmlHelper.AddNode(this.TurretOverload, Mount.XmlNameTurretOverloadName, ref node);
			XmlHelper.AddNode(this.BarrelOverload, Mount.XmlNameBarrelOverloadName, ref node);
			XmlHelper.AddNode(this.BaseOverload, Mount.XmlNameBaseOverloadName, ref node);
			XmlHelper.AddNode(this.YawMin, Mount.XmlNameYawMin, ref node);
			XmlHelper.AddNode(this.YawMax, Mount.XmlNameYawMax, ref node);
			XmlHelper.AddNode(this.PitchMin, Mount.XmlNamePitchMin, ref node);
			XmlHelper.AddNode(this.PitchMax, Mount.XmlNamePitchMax, ref node);
			XmlHelper.AddNode(this.SectionFireAnimation, Mount.XmlNameSectionFireAnimation, ref node);
			XmlHelper.AddNode(this.SectionReloadAnimation, Mount.XmlNameSectionReloadAnimation, ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.NodeName = XmlHelper.GetData<string>(node, Mount.XmlNameNodeName);
			this.TurretOverload = XmlHelper.GetData<string>(node, Mount.XmlNameTurretOverloadName);
			this.BarrelOverload = XmlHelper.GetData<string>(node, Mount.XmlNameBarrelOverloadName);
			this.BaseOverload = XmlHelper.GetData<string>(node, Mount.XmlNameBaseOverloadName);
			this.YawMin = XmlHelper.GetData<float>(node, Mount.XmlNameYawMin);
			this.YawMax = XmlHelper.GetData<float>(node, Mount.XmlNameYawMax);
			this.PitchMin = XmlHelper.GetData<float>(node, Mount.XmlNamePitchMin);
			this.PitchMax = XmlHelper.GetData<float>(node, Mount.XmlNamePitchMax);
			this.SectionFireAnimation = XmlHelper.GetData<string>(node, Mount.XmlNameSectionFireAnimation);
			this.SectionReloadAnimation = XmlHelper.GetData<string>(node, Mount.XmlNameSectionReloadAnimation);
		}
	}
}
