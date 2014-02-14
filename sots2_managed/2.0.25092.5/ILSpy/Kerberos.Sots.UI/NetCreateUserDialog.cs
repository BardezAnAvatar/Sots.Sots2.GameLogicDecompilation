using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class NetCreateUserDialog : Dialog
	{
		public const string OKButton = "okButton";
		public const string CancelButton = "cancelButton";
		public const string FieldEmail = "new_email";
		public const string FieldUsername = "new_username";
		public const string FieldPassword = "new_password";
		public const string FieldConfirmPassword = "new_confirm";
		private string _enteredEmail = "";
		private string _enteredUsername = "";
		private string _enteredPassword = "";
		private string _enteredConfirmPassword = "";
		private bool _success;
		private NetProcessNetworkDialog _processDialog;
		private string _eulaConfirmDialogId;
		public NetCreateUserDialog(App game) : base(game, "dialogNetCreateUser")
		{
			this._app.UI.Send(new object[]
			{
				"SetFilterMode",
				"new_username",
				EditBoxFilterMode.GameSpyNick.ToString()
			});
		}
		public override void HandleScriptMessage(ScriptMessageReader mr)
		{
			Network.DialogAction dialogAction = (Network.DialogAction)mr.ReadInteger();
			Network.DialogAction dialogAction2 = dialogAction;
			if (dialogAction2 != Network.DialogAction.DA_FINALIZE)
			{
				switch (dialogAction2)
				{
				case Network.DialogAction.DA_NEWUSER_CREATING:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_CREATE_NEW_USER"));
					return;
				case Network.DialogAction.DA_NEWUSER_PASSWORD_MISMATCH:
				case Network.DialogAction.DA_NEWUSER_OFFLINE:
					break;
				case Network.DialogAction.DA_NEWUSER_INVALID_USERNAME:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_INVALID_USERNAME"));
					return;
				case Network.DialogAction.DA_NEWUSER_NICK_IN_USE:
					this.AddDialogString("Nickname in use.");
					return;
				case Network.DialogAction.DA_NEWUSER_INVALID_PASSWORD:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_INVALID_PASSWORD"));
					return;
				case Network.DialogAction.DA_NEWUSER_FAILED:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_USER_CREATION_FAILED"));
					return;
				case Network.DialogAction.DA_NEWUSER_SUCCESS:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_NEW_USER_CREATED"));
					return;
				default:
					return;
				}
			}
			else
			{
				this._success = mr.ReadBool();
				this.ShowButton(true);
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_opened" && panelName == base.ID)
			{
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblEmail"
				}), "text", App.Localize("@UI_GAMELOBBY_EMAIL_COLON"));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblUser"
				}), "text", App.Localize("@UI_GAMELOBBY_USERNAME_COLON_B"));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblPass"
				}), "text", App.Localize("@UI_GAMELOBBY_PASSWORD_COLON_B"));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblConfirm"
				}), "text", App.Localize("@UI_GAMELOBBY_CONFIRM_PASSWORD"));
			}
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._eulaConfirmDialogId = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@GAMESPY_AGREEMENT"), App.Localize("@GAMESPY_AGREEMENT_DESC"), "dialogEULAConfirm"), null);
					this._app.UI.SetVisible(this._eulaConfirmDialogId, false);
					this._app.UI.SetVisible(this._eulaConfirmDialogId, true);
				}
				else
				{
					if (panelName == "cancelButton")
					{
						this._app.UI.CloseDialog(this, true);
					}
				}
			}
			if (msgType == "edit_confirmed")
			{
				if (panelName == "new_email")
				{
					this._app.UI.Send(new object[]
					{
						"FocusKeyboard",
						this._app.UI.Path(new string[]
						{
							base.ID,
							"new_username"
						})
					});
					return;
				}
				if (panelName == "new_username")
				{
					this._app.UI.Send(new object[]
					{
						"FocusKeyboard",
						this._app.UI.Path(new string[]
						{
							base.ID,
							"new_password"
						})
					});
					return;
				}
				if (panelName == "new_password")
				{
					this._app.UI.Send(new object[]
					{
						"FocusKeyboard",
						this._app.UI.Path(new string[]
						{
							base.ID,
							"new_confirm"
						})
					});
					return;
				}
				if (panelName == "new_confirm")
				{
					this.Confirm();
					return;
				}
			}
			else
			{
				if (msgType == "text_changed")
				{
					if (panelName == "new_email")
					{
						this._enteredEmail = msgParams[0];
						return;
					}
					if (panelName == "new_username")
					{
						this._enteredUsername = msgParams[0];
						return;
					}
					if (panelName == "new_password")
					{
						this._enteredPassword = msgParams[0];
						return;
					}
					if (panelName == "new_confirm")
					{
						this._enteredConfirmPassword = msgParams[0];
						return;
					}
				}
				else
				{
					if (msgType == "dialog_closed")
					{
						if (this._processDialog != null && panelName == this._processDialog.ID)
						{
							this._processDialog = null;
							if (this._success)
							{
								this._app.UI.CloseDialog(this, true);
							}
						}
						if (panelName == this._eulaConfirmDialogId && msgParams.Count<string>() > 0 && msgParams[0] == "True")
						{
							this.Confirm();
						}
					}
				}
			}
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
		private void Confirm()
		{
			if (this._enteredPassword != this._enteredConfirmPassword)
			{
				this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@PASSWORDS_DO_NOT_MATCH"), App.Localize("@PASSWORDS_DO_NOT_MATCH_TEXT"), "dialogGenericMessage"), null);
				return;
			}
			this._app.Network.NewUser(base.ID, this._enteredEmail, this._enteredUsername, this._enteredPassword);
		}
		public override void Initialize()
		{
			base.Initialize();
		}
		public override string[] CloseDialog()
		{
			return new string[]
			{
				this._success.ToString()
			};
		}
	}
}
