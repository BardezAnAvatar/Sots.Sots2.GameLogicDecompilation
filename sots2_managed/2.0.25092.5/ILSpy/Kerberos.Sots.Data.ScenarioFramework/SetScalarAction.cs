using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class SetScalarAction : TriggerAction
	{
		internal const string XmlSetScalarAction = "SetScalar";
		private const string XmlScalarName = "Scalar";
		private const string XmlValueName = "Value";
		public string Scalar = "";
		public float Value;
		public override string XmlName
		{
			get
			{
				return "SetScalar";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Scalar, "Scalar", ref node);
			XmlHelper.AddNode(this.Value, "Value", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Scalar = XmlHelper.GetData<string>(node, "Scalar");
			this.Value = XmlHelper.GetData<float>(node, "Value");
		}
	}
}
