using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class StartContext : TriggerContext
	{
		internal const string XmlStartContextName = "StartContext";
		private const string XmlStartTurnName = "StartTurn";
		public int StartTurn;
		public override string XmlName
		{
			get
			{
				return "StartContext";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.StartTurn, "StartTurn", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.StartTurn = XmlHelper.GetData<int>(node, "StartTurn");
		}
	}
}
