using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Kerberos.Sots.UI.Dialogs
{
	internal class IntelCriticalSuccessDialog : Dialog
	{
		private readonly int _targetPlayer;
		private readonly GameSession _game;
		private Button _okButton;
		private EspionagePlayerHeader _playerHeader;
		private Label _descLabel;
		private DropDownList[] _intelDdls;
		private DropDownList _blameDdl;
		public IntelCriticalSuccessDialog(GameSession game, int targetPlayer) : base(game.App, "dialogIntelMajorSuccess")
		{
			this._targetPlayer = targetPlayer;
			this._game = game;
		}
		private void RepopulateIntelDDLs(DropDownList ignore)
		{
			Dictionary<DropDownList, IntelMissionDesc> dictionary = new Dictionary<DropDownList, IntelMissionDesc>();
			IEnumerable<DropDownList> enumerable = 
				from x in this._intelDdls
				where x != ignore
				select x;
			foreach (DropDownList current in enumerable)
			{
				if (current.SelectedItem != null)
				{
					dictionary[current] = (IntelMissionDesc)current.SelectedItem;
				}
			}
			foreach (DropDownList current2 in enumerable)
			{
				HashSet<IntelMissionDesc> hashSet = new HashSet<IntelMissionDesc>(this._game.AssetDatabase.IntelMissions);
				foreach (KeyValuePair<DropDownList, IntelMissionDesc> current3 in dictionary)
				{
					if (current3.Key != current2)
					{
						hashSet.Remove(current3.Value);
					}
				}
				current2.Clear();
				foreach (IntelMissionDesc current4 in hashSet)
				{
					current2.AddItem(current4, current4.Name);
				}
				IntelMissionDesc intelMissionDesc = null;
				if (!dictionary.TryGetValue(current2, out intelMissionDesc))
				{
					intelMissionDesc = hashSet.FirstOrDefault<IntelMissionDesc>();
					if (intelMissionDesc != null)
					{
						dictionary[current2] = intelMissionDesc;
					}
				}
				current2.SetSelection(intelMissionDesc);
			}
		}
		private void RepopulateBlameDDL()
		{
			List<PlayerInfo> list = (
				from x in this._game.GameDatabase.GetPlayerInfos()
				where x.isStandardPlayer && x.ID != this._game.LocalPlayer.ID
				select x).ToList<PlayerInfo>();
			this._blameDdl.Clear();
			this._blameDdl.AddItem(string.Empty, App.Localize("@UI_DIPLOMACY_INTEL_CRITICAL_SUCCESS_NOBLAME"));
			foreach (PlayerInfo current in list)
			{
				this._blameDdl.AddItem(current, current.Name);
				this._blameDdl.GetLastItemID();
			}
			if (list.Count > 0)
			{
				this._blameDdl.SetSelection(list[0]);
			}
			else
			{
				this._blameDdl.SetSelection(string.Empty);
			}
			this.BlameDDLSelectionChanged(null, null);
		}
		private IEnumerable<IntelMissionDesc> GetSelectedMissions()
		{
			List<IntelMissionDesc> list = new List<IntelMissionDesc>();
			DropDownList[] intelDdls = this._intelDdls;
			DropDownList[] array = intelDdls;
			for (int i = 0; i < array.Length; i++)
			{
				DropDownList dropDownList = array[i];
				if (dropDownList.SelectedItem != null)
				{
					list.Add((IntelMissionDesc)dropDownList.SelectedItem);
				}
			}
			return list;
		}
		private void IntelDDLSelectionChanged(object sender, EventArgs e)
		{
			this.RepopulateIntelDDLs(sender as DropDownList);
		}
		private void BlameDDLSelectionChanged(object sender, EventArgs e)
		{
			PlayerInfo playerInfo = this._blameDdl.SelectedItem as PlayerInfo;
			if (playerInfo == null)
			{
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"imgBlameAvatar"
				}), false);
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"imgBlameBadge"
				}), false);
				return;
			}
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"imgBlameAvatar"
			}), true);
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"imgBlameBadge"
			}), true);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"imgBlameAvatar"
			}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"imgBlameBadge"
			}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
		}
		private void _okButton_Clicked(object sender, EventArgs e)
		{
			PlayerInfo blamePlayer = this._blameDdl.SelectedItem as PlayerInfo;
			this._app.Game.DoIntelMissionCriticalSuccess(this._targetPlayer, this.GetSelectedMissions(), blamePlayer);
			this._app.UI.CloseDialog(this, true);
		}
		public override void Initialize()
		{
			this._okButton = new Button(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"btnOk"
			}), null);
			this._okButton.Clicked += new EventHandler(this._okButton_Clicked);
			this._playerHeader = new EspionagePlayerHeader(this._game, base.ID);
			this._descLabel = new Label(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"lblIntelDesc"
			}));
			this._intelDdls = new DropDownList[]
			{
				new DropDownList(base.UI, base.UI.Path(new string[]
				{
					base.ID,
					"ddlIntel1"
				})),
				new DropDownList(base.UI, base.UI.Path(new string[]
				{
					base.ID,
					"ddlIntel2"
				})),
				new DropDownList(base.UI, base.UI.Path(new string[]
				{
					base.ID,
					"ddlIntel3"
				}))
			};
			DropDownList[] intelDdls = this._intelDdls;
			for (int i = 0; i < intelDdls.Length; i++)
			{
				DropDownList dropDownList = intelDdls[i];
				dropDownList.SelectionChanged += new EventHandler(this.IntelDDLSelectionChanged);
			}
			this._blameDdl = new DropDownList(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"ddlBlame"
			}));
			this._blameDdl.SelectionChanged += new EventHandler(this.BlameDDLSelectionChanged);
			base.AddPanels(new PanelBinding[]
			{
				this._okButton,
				this._playerHeader,
				this._descLabel,
				this._blameDdl
			});
			base.AddPanels(this._intelDdls);
			PlayerInfo playerInfo = this._game.GameDatabase.GetPlayerInfo(this._targetPlayer);
			this._playerHeader.UpdateFromPlayerInfo(this._game.LocalPlayer.ID, playerInfo);
			DiplomacyUI.SyncPanelColor(this._app, base.ID, playerInfo.PrimaryColor);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(App.Localize("@UI_DIPLOMACY_INTEL_CRITICAL_SUCCESS_TITLE") + "\n");
			stringBuilder.Append(string.Format(App.Localize("@UI_DIPLOMACY_INTEL_INFO_DESC_TARGET") + "\n", playerInfo.Name));
			this._descLabel.SetText(stringBuilder.ToString());
			this.RepopulateIntelDDLs(null);
			this.RepopulateBlameDDL();
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			base.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Recursive);
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
