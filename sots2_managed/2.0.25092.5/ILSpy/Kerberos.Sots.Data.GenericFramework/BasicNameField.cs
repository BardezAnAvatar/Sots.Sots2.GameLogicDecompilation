using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.GenericFramework
{
	public class BasicNameField : IXmlLoadSave
	{
		private const string XmlNameName = "Name";
		public string Name = "";
		public string XmlName
		{
			get
			{
				return "Name";
			}
		}
		public override string ToString()
		{
			return this.Name ?? string.Empty;
		}
		public virtual void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
		}
		public virtual void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				this.Name = XmlHelper.GetData<string>(node, "Name");
			}
		}
	}
}
