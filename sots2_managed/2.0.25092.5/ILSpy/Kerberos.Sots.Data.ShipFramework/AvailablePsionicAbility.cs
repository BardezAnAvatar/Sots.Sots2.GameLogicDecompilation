using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class AvailablePsionicAbility : IXmlLoadSave
	{
		internal const string XmlAvailablePsionicAbilityName = "PsionicAbility";
		private const string XmlNameName = "Name";
		private const string XmlModifierName = "Modifier";
		public string Name = "";
		public float Modifier;
		public string XmlName
		{
			get
			{
				return "PsionicAbility";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Modifier, "Modifier", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Modifier = XmlHelper.GetDataOrDefault<float>(node["Modifier"], 1f);
		}
	}
}
