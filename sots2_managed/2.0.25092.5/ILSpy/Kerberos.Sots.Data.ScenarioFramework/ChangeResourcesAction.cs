using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ChangeResourcesAction : TriggerAction
	{
		internal const string XmlChangeResourcesActionName = "ChangeResources";
		private const string XmlSystemIdName = "SystemId";
		private const string XmlOrbitNameName = "OrbitName";
		private const string XmlAmountToAddName = "AmountToAdd";
		public int SystemId;
		public string OrbitName = "";
		public float AmountToAdd;
		public override string XmlName
		{
			get
			{
				return "ChangeResources";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.SystemId, "SystemId", ref node);
			XmlHelper.AddNode(this.OrbitName, "OrbitName", ref node);
			XmlHelper.AddNode(this.AmountToAdd, "AmountToAdd", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.SystemId = XmlHelper.GetData<int>(node, "SystemId");
			this.OrbitName = XmlHelper.GetData<string>(node, "OrbitName");
			this.AmountToAdd = XmlHelper.GetData<float>(node, "AmountToAdd");
		}
	}
}
