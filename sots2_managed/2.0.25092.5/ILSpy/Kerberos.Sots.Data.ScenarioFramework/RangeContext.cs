using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class RangeContext : TriggerContext
	{
		internal const string XmlRangeContextName = "RangeContext";
		private const string XmlStartTurnName = "StartTurn";
		private const string XmlEndTurnName = "EndTurn";
		public int StartTurn;
		public int EndTurn;
		public override string XmlName
		{
			get
			{
				return "RangeContext";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.StartTurn, "StartTurn", ref node);
			XmlHelper.AddNode(this.EndTurn, "EndTurn", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.StartTurn = XmlHelper.GetData<int>(node, "StartTurn");
			this.EndTurn = XmlHelper.GetData<int>(node, "EndTurn");
		}
	}
}
