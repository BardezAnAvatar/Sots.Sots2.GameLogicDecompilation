using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ProvinceChangedAction : TriggerAction
	{
		internal const string XmlProvinceChangedActionName = "ProvinceChanged";
		private const string XmlProvinceIdName = "ProvinceId";
		private const string XmlNewPlayerName = "NewPlayer";
		public int ProvinceId;
		public int NewPlayer;
		public override string XmlName
		{
			get
			{
				return "ProvinceChanged";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.ProvinceId, "ProvinceId", ref node);
			XmlHelper.AddNode(this.NewPlayer, "NewPlayer", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.ProvinceId = XmlHelper.GetData<int>(node, "ProvinceId");
			this.NewPlayer = XmlHelper.GetData<int>(node, "NewPlayer");
		}
	}
}
