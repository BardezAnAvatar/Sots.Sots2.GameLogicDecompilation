using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AddSectionAction : TriggerAction
	{
		internal const string XmlAddSectionActionName = "AddSection";
		private const string XmlPlayerName = "Player";
		private const string XmlSectionFileName = "SectionFile";
		public int Player;
		public string SectionFile = "";
		public override string XmlName
		{
			get
			{
				return "AddSection";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Player, "Player", ref node);
			XmlHelper.AddNode(this.SectionFile, "SectionFile", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.SectionFile = XmlHelper.GetData<string>(node, "SectionFile");
		}
	}
}
