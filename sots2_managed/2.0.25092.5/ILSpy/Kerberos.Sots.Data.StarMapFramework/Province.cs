using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Province : IXmlLoadSave
	{
		internal const string XmlProvinceName = "Province";
		internal const string XmlProvinceIdName = "Id";
		internal const string XmlNameName = "Name";
		internal const string XmlCapitalIdName = "CapitalId";
		internal const string XmlPlayerName = "Player";
		public int? Id;
		public string Name = "";
		public int CapitalID;
		public int Player;
		public virtual string XmlName
		{
			get
			{
				return "Province";
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Id, "Id", ref node);
			XmlHelper.AddNode(this.Name, "Name", ref node);
			XmlHelper.AddNode(this.CapitalID, "CapitalId", ref node);
			XmlHelper.AddNode(this.Player, "Player", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				this.Id = XmlHelper.GetData<int?>(node, "Id");
				this.Name = XmlHelper.GetData<string>(node, "Name");
				this.CapitalID = XmlHelper.GetData<int>(node, "CapitalId");
				this.Player = XmlHelper.GetData<int>(node, "Player");
			}
		}
	}
}
