using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class NetLoginDialog : Dialog
	{
		public const string CreateButton = "createButton";
		public const string DeleteButton = "deleteButton";
		public const string NewUserButton = "buttonNewUser";
		public const string LANTab = "buttonLAN";
		public const string InternetTab = "buttonInternet";
		public const string OKButton = "buttonLogin";
		public const string CancelButton = "buttonCancel";
		public const string UserBox = "login_username";
		public const string PassBox = "login_password";
		private string _enteredUser;
		private string _enteredPass;
		private bool _lanmode;
		private bool _cancelled;
		private NetProcessNetworkDialog _processDialog;
		private bool _success;
		public NetLoginDialog(App game) : base(game, "dialogNetLogin")
		{
			this._enteredUser = "";
			this._enteredPass = "";
			this._app.UI.Send(new object[]
			{
				"SetFilterMode",
				"login_username",
				EditBoxFilterMode.GameSpyNick.ToString()
			});
		}
		public override void HandleScriptMessage(ScriptMessageReader mr)
		{
			switch (mr.ReadInteger())
			{
			case 0:
				this._success = mr.ReadBool();
				this.ShowButton(true);
				break;
			case 1:
			case 6:
			case 7:
			case 8:
				break;
			case 2:
				this.AddDialogString(App.Localize("@GAMESPY_CONNECTING"));
				return;
			case 3:
				this.AddDialogString(App.Localize("@GAMESPY_CONNECT_FAILED"));
				return;
			case 4:
				this.AddDialogString(App.Localize("@GAMESPY_INVALID_PASS"));
				return;
			case 5:
				this.AddDialogString(App.Localize("@INVALID_USERNAME_TEXT"));
				return;
			case 9:
				this.AddDialogString(App.Localize("@GAMESPY_CONNECT_CHAT_FAILED"));
				return;
			case 10:
				this.AddDialogString(App.Localize("@GAMESPY_CONNECTED"));
				return;
			default:
				return;
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_opened")
			{
				if (panelName == base.ID)
				{
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						base.ID,
						"lblUser"
					}), "text", App.Localize("@UI_GAMELOBBY_USERNAME_COLON"));
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						base.ID,
						"lblPass"
					}), "text", App.Localize("@UI_GAMELOBBY_PASSWORD_COLON"));
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == "buttonLAN")
					{
						this.SetLANMode(true);
						return;
					}
					if (panelName == "buttonInternet")
					{
						this.SetLANMode(false);
						return;
					}
					if (panelName == "buttonNewUser")
					{
						this._app.UI.CreateDialog(new NetCreateUserDialog(this._app), null);
						return;
					}
					if (panelName == "buttonCancel")
					{
						this._cancelled = true;
						this._app.UI.CloseDialog(this, true);
						return;
					}
					if (panelName == "buttonLogin")
					{
						if (this._lanmode)
						{
							if (string.IsNullOrEmpty(this._enteredUser) || this._enteredUser.Contains(" "))
							{
								this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@INVALID_USER"), App.Localize("@INVALID_USERNAME_TEXT"), "dialogGenericMessage"), null);
								return;
							}
							this._app.Network.Login(this._enteredUser);
							this._app.Network.IsOffline = true;
							this._app.UI.CloseDialog(this, true);
							return;
						}
						else
						{
							if (string.IsNullOrEmpty(this._enteredUser) || this._enteredUser.Contains(" "))
							{
								this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@INVALID_USER"), App.Localize("@INVALID_USERNAME_TEXT"), "dialogGenericMessage"), null);
								return;
							}
							if (!this._app.Network.IsLoggedIn)
							{
								this._app.Network.Login(this._enteredUser);
								this._app.Network.IsOffline = false;
								return;
							}
							this._app.Network.IsOffline = false;
							this._app.UI.CloseDialog(this, true);
							return;
						}
					}
				}
				else
				{
					if (msgType == "text_changed")
					{
						if (panelName == "login_username")
						{
							this._enteredUser = msgParams[0];
							return;
						}
						if (panelName == "login_password")
						{
							this._enteredPass = msgParams[0];
							return;
						}
					}
					else
					{
						if (msgType == "dialog_closed" && this._processDialog != null && panelName == this._processDialog.ID)
						{
							this._processDialog = null;
							if (this._success)
							{
								this._app.Network.IsLoggedIn = true;
								this._app.UI.CloseDialog(this, true);
							}
						}
					}
				}
			}
		}
		public override void Initialize()
		{
			base.Initialize();
			this.SetLANMode(true);
		}
		public void SetResult(bool success)
		{
			this._success = success;
		}
		public void AddDialogString(string val)
		{
			if (this._processDialog == null)
			{
				this._processDialog = new NetProcessNetworkDialog(this._app);
				this._app.UI.CreateDialog(this._processDialog, null);
			}
			this._processDialog.AddDialogString(val);
		}
		public void ShowButton(bool val)
		{
			if (this._processDialog != null)
			{
				this._processDialog.ShowButton(val);
			}
		}
		private void SetLANMode(bool val)
		{
			this._lanmode = val;
			if (val)
			{
				this._app.UI.SetEnabled(this._app.UI.Path(new string[]
				{
					base.ID,
					"login_username"
				}), false);
				this._app.UI.SetEnabled(this._app.UI.Path(new string[]
				{
					base.ID,
					"login_password"
				}), false);
				this._app.UI.SetEnabled(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblUser"
				}), false);
				this._app.UI.SetEnabled(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblPass"
				}), false);
				this._app.UI.SetEnabled(this._app.UI.Path(new string[]
				{
					base.ID,
					"buttonNewUser"
				}), false);
				this._app.UI.SetChecked(this._app.UI.Path(new string[]
				{
					base.ID,
					"buttonLAN"
				}), true);
				this._app.UI.SetChecked(this._app.UI.Path(new string[]
				{
					base.ID,
					"buttonInternet"
				}), false);
				this._enteredUser = this._app.UserProfile.ProfileName;
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					base.ID,
					"login_username"
				}), "text", this._app.UserProfile.ProfileName);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					base.ID,
					"login_password"
				}), "text", "");
				return;
			}
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"login_username"
			}), true);
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"login_password"
			}), true);
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"lblUser"
			}), true);
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"lblPass"
			}), true);
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"buttonNewUser"
			}), true);
			this._app.UI.SetChecked(this._app.UI.Path(new string[]
			{
				base.ID,
				"buttonLAN"
			}), false);
			this._app.UI.SetChecked(this._app.UI.Path(new string[]
			{
				base.ID,
				"buttonInternet"
			}), true);
			this._enteredUser = (this._app.UserProfile.Username ?? "");
			this._enteredPass = (this._app.UserProfile.Password ?? "");
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"login_username"
			}), "text", this._enteredUser);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"login_password"
			}), "text", this._enteredPass);
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"login_username"
			}), !this._app.Network.IsLoggedIn);
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"login_password"
			}), !this._app.Network.IsLoggedIn);
		}
		public override string[] CloseDialog()
		{
			return new string[]
			{
				this._cancelled.ToString()
			};
		}
	}
}
