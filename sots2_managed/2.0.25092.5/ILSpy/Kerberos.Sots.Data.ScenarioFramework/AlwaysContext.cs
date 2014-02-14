using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AlwaysContext : TriggerContext
	{
		internal const string XmlAlwaysContextName = "AlwaysContext";
		public override string XmlName
		{
			get
			{
				return "AlwaysContext";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
		}
	}
}
