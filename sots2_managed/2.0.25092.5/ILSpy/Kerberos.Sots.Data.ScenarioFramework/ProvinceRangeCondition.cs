using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ProvinceRangeCondition : TriggerCondition
	{
		internal const string XmlProvinceRangeConditionName = "ProvinceRange";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlProvinceIdName = "ProvinceId";
		private const string XmlDistanceName = "Name";
		public int PlayerSlot;
		public int ProvinceId;
		public float Distance;
		public override string XmlName
		{
			get
			{
				return "ProvinceRange";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.ProvinceId, "ProvinceId", ref node);
			XmlHelper.AddNode(this.Distance, "Name", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.PlayerSlot = XmlHelper.GetData<int>(node, "PlayerSlot");
			this.ProvinceId = XmlHelper.GetData<int>(node, "ProvinceId");
			this.Distance = XmlHelper.GetData<float>(node, "Name");
		}
	}
}
