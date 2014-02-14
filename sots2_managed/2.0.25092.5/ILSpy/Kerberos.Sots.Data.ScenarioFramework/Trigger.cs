using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class Trigger : IXmlLoadSave
	{
		internal const string XmlTriggerName = "Trigger";
		private const string XmlNameName = "Name";
		private const string XmlIsRecurring = "IsRecurring";
		private const string XmlContextName = "Context";
		private const string XmlConditionsName = "Conditions";
		private const string XmlActionsName = "Actions";
		public string Name;
		public bool IsRecurring;
		public TriggerContext Context = new AlwaysContext();
		public List<TriggerCondition> Conditions = new List<TriggerCondition>();
		public List<TriggerAction> Actions = new List<TriggerAction>();
		internal List<FleetInfo> RangeTriggeredFleets = new List<FleetInfo>();
		public string XmlName
		{
			get
			{
				return "Trigger";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.IsRecurring, "IsRecurring", ref node);
			XmlHelper.AddObjectNode(this.Context, "Context", ref node);
			XmlHelper.AddObjectCollectionNode(this.Conditions, "Conditions", ref node);
			XmlHelper.AddObjectCollectionNode(this.Actions, "Actions", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.IsRecurring = XmlHelper.GetData<bool>(node, "IsRecurring");
			this.Context = XmlHelper.GetDataObject<TriggerContext>(node, "Context", ScenarioEnumerations.ContextTypeMap);
			this.Conditions = XmlHelper.GetDataObjectCollection<TriggerCondition>(node, "Conditions", ScenarioEnumerations.ConditionTypeMap);
			this.Actions = XmlHelper.GetDataObjectCollection<TriggerAction>(node, "Actions", ScenarioEnumerations.ActionTypeMap);
		}
	}
}
