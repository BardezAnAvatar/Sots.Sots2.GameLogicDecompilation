using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AddScalarToScalarAction : TriggerAction
	{
		internal const string XmlAddScalarToScalarActionName = "AddScalarToScalarAction";
		private const string XmlScalarToAddName = "ScalarToAdd";
		private const string XmlScalarAddedToName = "ScalarAddedTo";
		public string ScalarToAdd = "";
		public string ScalarAddedTo = "";
		public override string XmlName
		{
			get
			{
				return "AddScalarToScalarAction";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.ScalarToAdd, "ScalarToAdd", ref node);
			XmlHelper.AddNode(this.ScalarAddedTo, "ScalarAddedTo", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ScalarToAdd = XmlHelper.GetData<string>(node, "ScalarToAdd");
			this.ScalarAddedTo = XmlHelper.GetData<string>(node, "ScalarAddedTo");
		}
	}
}
