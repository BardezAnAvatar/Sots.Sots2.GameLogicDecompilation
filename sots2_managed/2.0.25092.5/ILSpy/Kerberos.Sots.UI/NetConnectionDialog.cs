using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.UI
{
	internal class NetConnectionDialog : Dialog
	{
		public const string cancelButton = "dialogButtonCancel";
		public const string okButton = "dialogButtonOk";
		private string _title;
		private string _text;
		private bool _success;
		public NetConnectionDialog(App app, string title = "Connecting", string text = "", string template = "dialogNetConnection") : base(app, template)
		{
			this._title = title;
			this._text = text;
		}
		public override void Initialize()
		{
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"title"
			}), "text", this._title);
			this.RefreshText();
		}
		public override void HandleScriptMessage(ScriptMessageReader mr)
		{
			Network.DialogAction dialogAction = (Network.DialogAction)mr.ReadInteger();
			Network.DialogAction dialogAction2 = dialogAction;
			switch (dialogAction2)
			{
			case Network.DialogAction.DA_FINALIZE:
				this._success = mr.ReadBool();
				this._app.UI.CloseDialog(this, true);
				return;
			case Network.DialogAction.DA_RAW_STRING:
				this.AddString(mr.ReadString());
				return;
			default:
				switch (dialogAction2)
				{
				case Network.DialogAction.DA_CONNECT_CONNECTING:
					this.AddString("Connecting to Host.");
					return;
				case Network.DialogAction.DA_CONNECT_FAILED:
					this.AddString("Failed to connect to Host.");
					return;
				case Network.DialogAction.DA_CONNECT_SUCCESS:
					this.AddString("Connection to Host succeeded.");
					return;
				case Network.DialogAction.DA_CONNECT_TIMED_OUT:
					this.AddString("Connection to Host timed out.");
					return;
				case Network.DialogAction.DA_CONNECT_INVALID_PASSWORD:
					this.AddString("Invalid password.");
					return;
				case Network.DialogAction.DA_CONNECT_NAT_FAILURE:
					this.AddString("NAT Negotiation failed.");
					return;
				default:
					return;
				}
				break;
			}
		}
		public void AddString(string text)
		{
			this._text = this._text + "\n" + text;
			this.RefreshText();
		}
		public void ClearText()
		{
			this._text = string.Empty;
			this.RefreshText();
		}
		public void RefreshText()
		{
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"textbox"
			}), "text", this._text);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && (panelName == "dialogButtonOk" || panelName == "dialogButtonCancel"))
			{
				this._app.UI.CloseDialog(this, true);
			}
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
