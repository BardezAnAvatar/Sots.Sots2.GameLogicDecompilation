using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class StratModifierChangedAction : TriggerAction
	{
		internal const string XmlStratModifierChangedActionName = "StratModifierChanged";
		private const string XmlStratModifierName = "StratModifier";
		private const string XmlAmountChangedName = "AmountChanged";
		public string StratModifier = "";
		public float AmountChanged;
		public override string XmlName
		{
			get
			{
				return "StratModifierChanged";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.StratModifier, "StratModifier", ref node);
			XmlHelper.AddNode(this.AmountChanged, "AmountChanged", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.StratModifier = XmlHelper.GetData<string>(node, "StratModifier");
			this.AmountChanged = XmlHelper.GetData<float>(node, "AmountChanged");
		}
	}
}
