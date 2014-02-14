using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class NodeLine : IXmlLoadSave
	{
		internal const string XmlNodeLineName = "NodeLine";
		internal const string XmlSystemAName = "SystemA";
		internal const string XmlSystemBName = "SystemB";
		internal const string XmlIsPermanent = "isPermanent";
		public int SystemA;
		public int SystemB;
		public bool isPermanent = true;
		public virtual string XmlName
		{
			get
			{
				return "NodeLine";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemA, "SystemA", ref node);
			XmlHelper.AddNode(this.SystemB, "SystemB", ref node);
			XmlHelper.AddNode(this.isPermanent, "isPermanent", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				this.SystemA = XmlHelper.GetData<int>(node, "SystemA");
				this.SystemB = XmlHelper.GetData<int>(node, "SystemB");
				this.isPermanent = XmlHelper.GetData<bool>(node, "isPermanent");
			}
		}
	}
}
