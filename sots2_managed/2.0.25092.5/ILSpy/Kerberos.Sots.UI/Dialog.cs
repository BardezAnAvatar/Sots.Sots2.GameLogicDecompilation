using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal abstract class Dialog : PanelBinding, IDisposable
	{
		protected App _app;
		public string Template
		{
			get;
			private set;
		}
		public Dialog(App game, string template) : base(game.UI, Guid.NewGuid().ToString())
		{
			this.Template = template;
			this._app = game;
			this._app.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			this._app.UI.UpdateEvent += new UIEventUpdate(this.UICommChannel_Update);
		}
		public abstract string[] CloseDialog();
		public virtual void Initialize()
		{
		}
		private void UICommChannel_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._app.UI.GetTopDialog() == this)
			{
				this.OnPanelMessage(panelName, msgType, msgParams);
			}
		}
		protected virtual void OnUpdate()
		{
		}
		private void UICommChannel_Update()
		{
			this.OnUpdate();
		}
		public virtual void Dispose()
		{
			this._app.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			this._app.UI.UpdateEvent -= new UIEventUpdate(this.UICommChannel_Update);
		}
		public virtual void HandleScriptMessage(ScriptMessageReader mr)
		{
		}
	}
}
