using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.WeaponFramework
{
	public class MuzzleDescriptor : IXmlLoadSave
	{
		protected const string XmlMuzzleDescriptorName = "MuzzleDescriptor";
		private const string XmlMuzzleTypeName = "MuzzleType";
		private const string XmlWidthName = "Width";
		private const string XmlHeightName = "Height";
		public string MuzzleType = "";
		public float Width;
		public float Height;
		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.MuzzleType, "MuzzleType", ref node);
			XmlHelper.AddNode(this.Width, "Width", ref node);
			XmlHelper.AddNode(this.Height, "Height", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.MuzzleType = XmlHelper.GetData<string>(node, "MuzzleType");
			this.Width = (float)XmlHelper.GetData<int>(node, "Width");
			this.Height = (float)XmlHelper.GetData<int>(node, "Height");
		}
	}
}
