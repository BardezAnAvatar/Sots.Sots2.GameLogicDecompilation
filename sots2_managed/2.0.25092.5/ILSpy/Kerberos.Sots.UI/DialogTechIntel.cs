using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.GameStates;
using System;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DialogTechIntel : Dialog
	{
		public const string OKButton = "okButton";
		public const string EncyclopediaInfoButton = "navTechInfo";
		private int _techID;
		private PlayerInfo _targetPlayer;
		private ResearchInfoPanel _researchInfo;
		public DialogTechIntel(App game, int techid, PlayerInfo targetPlayer) : base(game, "dialogResearchIntel")
		{
			this._techID = techid;
			this._targetPlayer = targetPlayer;
		}
		public override void Initialize()
		{
			this._app.UI.SetVisible("navTechInfo", false);
			string pti = this._app.GameDatabase.GetTechFileID(this._techID);
			this._app.AssetDatabase.MasterTechTree.Technologies.First((Tech x) => x.Id == pti);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"subheader"
			}), "Stole intel from " + this._targetPlayer.Name);
			this._researchInfo = new ResearchInfoPanel(this._app.UI, this._app.UI.Path(new string[]
			{
				base.ID,
				"research_details"
			}));
			this._researchInfo.SetTech(this._app, this._techID);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"playerAvatar"
			}), "sprite", Path.GetFileNameWithoutExtension(this._targetPlayer.AvatarAssetPath));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"playerBadge"
			}), "sprite", Path.GetFileNameWithoutExtension(this._targetPlayer.BadgeAssetPath));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "navTechInfo")
				{
					string techFileID = this._app.GameDatabase.GetTechFileID(this._techID);
					if (techFileID != null)
					{
						SotspediaState.NavigateToLink(this._app, "#" + techFileID);
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
