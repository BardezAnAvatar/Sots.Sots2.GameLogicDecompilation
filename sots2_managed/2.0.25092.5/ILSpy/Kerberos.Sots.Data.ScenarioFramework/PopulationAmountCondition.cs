using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PopulationAmountCondition : TriggerCondition
	{
		internal const string XmlPopulationAmountConditionName = "PopulationAmount";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitNameName = "OrbitName";
		private const string XmlPopulationNumberName = "PopulationNumber";
		public int SystemId;
		public string OrbitName;
		public float PopulationNumber;
		public override string XmlName
		{
			get
			{
				return "PopulationAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.OrbitName, "OrbitName", ref node);
			XmlHelper.AddNode(this.PopulationNumber, "PopulationNumber", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitName = XmlHelper.GetData<string>(node, "OrbitName");
			this.PopulationNumber = XmlHelper.GetData<float>(node, "PopulationNumber");
		}
	}
}
