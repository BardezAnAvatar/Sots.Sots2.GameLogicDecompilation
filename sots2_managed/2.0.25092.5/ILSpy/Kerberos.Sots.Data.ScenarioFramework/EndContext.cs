using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class EndContext : TriggerContext
	{
		internal const string XmlEndContextName = "EndContext";
		private const string XmlEndTurnName = "EndTurn";
		public int EndTurn;
		public override string XmlName
		{
			get
			{
				return "EndContext";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.EndTurn, "EndTurn", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.EndTurn = XmlHelper.GetData<int>(node, "EndTurn");
		}
	}
}
