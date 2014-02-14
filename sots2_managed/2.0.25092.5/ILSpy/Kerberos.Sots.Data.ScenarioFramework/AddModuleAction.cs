using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class AddModuleAction : TriggerAction
	{
		internal const string XmlAddModuleActionName = "AddModule";
		private const string XmlPlayerName = "Player";
		private const string XmlModuleFileName = "ModuleFile";
		public int Player;
		public string ModuleFile = "";
		public override string XmlName
		{
			get
			{
				return "AddModule";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Player, "Player", ref node);
			XmlHelper.AddNode(this.ModuleFile, "ModuleFile", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Player = XmlHelper.GetData<int>(node, "Player");
			this.ModuleFile = XmlHelper.GetData<string>(node, "ModuleFile");
		}
	}
}
