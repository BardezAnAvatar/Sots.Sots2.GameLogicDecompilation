using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class StellarBody : Feature
	{
		internal const string XmlStellarBodyName = "StellarBody";
		private const string XmlModelName = "Model";
		public string Model;
		public override string XmlName
		{
			get
			{
				return "StellarBody";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode(this.Model, "Model", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				base.LoadFromXmlNode(node);
				this.Model = XmlHelper.GetData<string>(node, "Model");
			}
		}
	}
}
