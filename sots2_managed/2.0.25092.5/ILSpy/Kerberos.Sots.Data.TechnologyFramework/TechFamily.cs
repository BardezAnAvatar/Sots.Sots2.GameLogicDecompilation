using Kerberos.Sots.Data.Xml;
using System;
using System.IO;
using System.Xml;
namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class TechFamily : IXmlLoadSave
	{
		internal const string XmlFamilyName = "Family";
		private const string XmlNameName = "Name";
		private const string XmlIdName = "Id";
		private const string XmlIconName = "Icon";
		private const string XmlFactionDefinedName = "FactionDefined";
		public string Name = "";
		public string Id = "";
		public string Icon = "";
		public bool FactionDefined;
		public string XmlName
		{
			get
			{
				return "Family";
			}
		}
		public string GetProperIconPath()
		{
			return Path.Combine("Tech", this.Icon.Replace(".\\", string.Empty));
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.Id, "Id", ref node);
			XmlHelper.AddNode(this.Icon, "Icon", ref node);
			XmlHelper.AddNode(this.FactionDefined, "FactionDefined", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Name = XmlHelper.GetData<string>(node, "Name");
			this.Id = XmlHelper.GetData<string>(node, "Id");
			this.Icon = XmlHelper.GetData<string>(node, "Icon");
			this.FactionDefined = XmlHelper.GetData<bool>(node, "FactionDefined");
		}
	}
}
