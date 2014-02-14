using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PlanetAmountCondition : TriggerCondition
	{
		internal const string XmlPlanetAmountConditionName = "PlanetAmount";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlNumberOfPlanetsName = "NumberOfPlanets";
		public int PlayerSlot;
		public int NumberOfPlanets;
		public override string XmlName
		{
			get
			{
				return "PlanetAmount";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.NumberOfPlanets, "NumberOfPlanets", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.NumberOfPlanets = XmlHelper.GetData<int>(node, "NumberOfPlanets");
		}
	}
}
