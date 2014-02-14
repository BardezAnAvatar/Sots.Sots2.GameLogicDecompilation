using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TriggerAction : IXmlLoadSave
	{
		public virtual string XmlName
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		public virtual void AttachToXmlNode(ref XmlElement node)
		{
		}
		public virtual void LoadFromXmlNode(XmlElement node)
		{
		}
	}
}
