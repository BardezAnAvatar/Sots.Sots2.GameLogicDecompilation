using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class BiosphereAmountCondition : TriggerCondition
	{
		internal const string XmlBiosphereAmountConditionName = "BiosphereAmount";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitNameName = "OrbitName";
		private const string XmlBiosphereAmountName = "BiosphereAmount";
		public int SystemId;
		public string OrbitName;
		public float BiosphereAmount;
		public override string XmlName
		{
			get
			{
				return "BiosphereAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.OrbitName, "OrbitName", ref node);
			XmlHelper.AddNode(this.BiosphereAmount, "BiosphereAmount", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitName = XmlHelper.GetData<string>(node, "OrbitName");
			this.BiosphereAmount = XmlHelper.GetData<float>(node, "BiosphereAmount");
		}
	}
}
