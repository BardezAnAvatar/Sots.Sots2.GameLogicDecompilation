using System;
using System.Xml;
namespace Kerberos.Sots.Data.Xml
{
	public interface IXmlLoadSave
	{
		string XmlName
		{
			get;
		}
		void LoadFromXmlNode(XmlElement node);
		void AttachToXmlNode(ref XmlElement node);
	}
}
