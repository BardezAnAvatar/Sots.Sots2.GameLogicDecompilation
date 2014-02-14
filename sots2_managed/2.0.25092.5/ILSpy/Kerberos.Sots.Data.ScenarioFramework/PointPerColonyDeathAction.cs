using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class PointPerColonyDeathAction : TriggerAction
	{
		internal const string XmlPointPerColonyDeathActionName = "PointPerColonyDeathAction";
		private const string XmlScalarNameName = "ScalarName";
		private const string XmlAmountPerColonyName = "AmountPerColony";
		public string ScalarName = "";
		public float AmountPerColony;
		public override string XmlName
		{
			get
			{
				return "PointPerColonyDeathAction";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.ScalarName, "ScalarName", ref node);
			XmlHelper.AddNode(this.AmountPerColony, "AmountPerColony", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ScalarName = XmlHelper.GetData<string>(node, "ScalarName");
			this.AmountPerColony = XmlHelper.GetData<float>(node, "AmountPerColony");
		}
	}
}
