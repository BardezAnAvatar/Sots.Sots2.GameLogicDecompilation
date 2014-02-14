using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class SelectProfileDialog : Dialog
	{
		public const string CreateButton = "createButton";
		public const string DeleteButton = "deleteButton";
		public const string OKButton = "okButton";
		private string _enterProfileNameDialog;
		private string _confirmDeleteDialog;
		private int _selection;
		public SelectProfileDialog(App game) : base(game, "dialogSelectProfile")
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelName, msgType, msgParams);
			if (msgType == "button_clicked")
			{
				if (panelName == "createButton")
				{
					this._enterProfileNameDialog = this._app.UI.CreateDialog(new GenericTextEntryDialog(this._app, App.Localize("@PROFILE_DIALOG"), App.Localize("@PROFILE_CREATE_DIALOG"), App.Localize("@GENERAL_DEFACTO"), 16, 2, true, EditBoxFilterMode.ProfileName), null);
					return;
				}
				if (panelName == "deleteButton")
				{
					List<Profile> availableProfiles = Profile.GetAvailableProfiles();
					Profile profile = availableProfiles[this._selection];
					this._confirmDeleteDialog = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@DELETE_HEADER"), string.Format(App.Localize("@DELETE_DIALOG"), profile.ProfileName), "dialogGenericQuestion"), null);
					return;
				}
				if (panelName == "okButton")
				{
					List<Profile> availableProfiles2 = Profile.GetAvailableProfiles();
					this._app.UserProfile = availableProfiles2[this._selection];
					this._app.GameSettings.LastProfile = availableProfiles2[this._selection].ProfileName;
					this._app.GameSettings.Save();
					if (!HotKeyManager.GetAvailableProfiles().Contains(this._app.UserProfile.ProfileName))
					{
						this._app.HotKeyManager.CreateProfile(this._app.UserProfile.ProfileName);
					}
					this._app.HotKeyManager.LoadProfile(this._app.UserProfile.ProfileName, false);
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				if (msgType == "dialog_closed")
				{
					if (panelName == this._enterProfileNameDialog)
					{
						if (bool.Parse(msgParams[0]))
						{
							List<Profile> availableProfiles3 = Profile.GetAvailableProfiles();
							foreach (Profile current in availableProfiles3)
							{
								if (current.ProfileName == msgParams[1])
								{
									this._enterProfileNameDialog = this._app.UI.CreateDialog(new GenericTextEntryDialog(this._app, App.Localize("@PROFILE_DIALOG"), App.Localize("@PROFILE_CREATE_DIALOG"), App.Localize("@GENERAL_DEFACTO"), 16, 2, false, EditBoxFilterMode.ProfileName), null);
									this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@ALREADY_EXISTS"), App.Localize("@ALREADY_EXISTS_TEXT"), "dialogGenericMessage"), null);
									return;
								}
							}
							Profile profile2 = new Profile();
							profile2.CreateProfile(msgParams[1]);
							this.RefreshProfileList();
							return;
						}
					}
					else
					{
						if (panelName == this._confirmDeleteDialog && bool.Parse(msgParams[0]))
						{
							List<Profile> availableProfiles4 = Profile.GetAvailableProfiles();
							Profile profile3 = availableProfiles4[this._selection];
							profile3.DeleteProfile();
							if (availableProfiles4.Count<Profile>() == 1)
							{
								this._enterProfileNameDialog = this._app.UI.CreateDialog(new GenericTextEntryDialog(this._app, App.Localize("@PROFILE_DIALOG"), App.Localize("@PROFILE_CREATE_DIALOG"), App.Localize("@GENERAL_DEFACTO"), 16, 2, false, EditBoxFilterMode.ProfileName), null);
							}
							this.RefreshProfileList();
							return;
						}
					}
				}
				else
				{
					if (msgType == "list_sel_changed")
					{
						this._selection = int.Parse(msgParams[0]);
					}
				}
			}
		}
		private void RefreshProfileList()
		{
			List<Profile> availableProfiles = Profile.GetAvailableProfiles();
			int num = 0;
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				base.ID,
				"profileList"
			}));
			foreach (Profile current in availableProfiles)
			{
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					base.ID,
					"profileList"
				}), "", num, current.ProfileName);
				num++;
			}
			this._app.UI.SetSelection(this._app.UI.Path(new string[]
			{
				base.ID,
				"profileList"
			}), 0);
		}
		public override void Initialize()
		{
			List<Profile> availableProfiles = Profile.GetAvailableProfiles();
			if (availableProfiles.Count<Profile>() == 0)
			{
				this._enterProfileNameDialog = this._app.UI.CreateDialog(new GenericTextEntryDialog(this._app, App.Localize("@PROFILE_DIALOG"), App.Localize("@PROFILE_CREATE_DIALOG"), App.Localize("@GENERAL_DEFACTO"), 16, 2, false, EditBoxFilterMode.ProfileName), null);
			}
			else
			{
				this.RefreshProfileList();
			}
			base.Initialize();
		}
		public override string[] CloseDialog()
		{
			return new string[0];
		}
	}
}
