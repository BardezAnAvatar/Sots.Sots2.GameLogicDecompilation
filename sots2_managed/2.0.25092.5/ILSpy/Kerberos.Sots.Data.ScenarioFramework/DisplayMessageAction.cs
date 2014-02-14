using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class DisplayMessageAction : TriggerAction
	{
		internal const string XmlDisplayMessageActionName = "DisplayMessage";
		private const string XmlMessageName = "Message";
		public string Message = "";
		public override string XmlName
		{
			get
			{
				return "DisplayMessage";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Message, "Message", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Message = XmlHelper.GetData<string>(node, "Message");
		}
	}
}
