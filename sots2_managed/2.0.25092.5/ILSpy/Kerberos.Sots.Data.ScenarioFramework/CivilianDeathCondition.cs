using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class CivilianDeathCondition : TriggerCondition
	{
		internal const string XmlCivilianDeathConditionName = "CivilianDeath";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitNameName = "OrbitName";
		private const string XmlCivilianAmountName = "CivilianAmount";
		public int SystemId;
		public string OrbitName = "";
		public int CivilianAmount;
		public override string XmlName
		{
			get
			{
				return "CivilianDeath";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.OrbitName, "OrbitName", ref node);
			XmlHelper.AddNode(this.CivilianAmount, "CivilianAmount", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitName = XmlHelper.GetData<string>(node, "OrbitName");
			this.CivilianAmount = XmlHelper.GetData<int>(node, "CivilianAmount");
		}
	}
}
