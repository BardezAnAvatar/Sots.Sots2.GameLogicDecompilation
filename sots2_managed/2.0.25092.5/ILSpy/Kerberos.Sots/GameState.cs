using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots
{
	internal abstract class GameState
	{
		private bool _guiLoaded;
		private bool _entered;
		protected App App
		{
			get;
			private set;
		}
		public string Name
		{
			get
			{
				return base.GetType().Name;
			}
		}
		public virtual bool IsTransitionState
		{
			get
			{
				return false;
			}
		}
		public virtual bool IsScreenState
		{
			get
			{
				return true;
			}
		}
		private static void Trace(string message)
		{
			App.Log.Trace(message, "state");
		}
		private static void Warn(string message)
		{
			App.Log.Warn(message, "state");
		}
		protected abstract void OnPrepare(GameState prev, object[] parms);
		protected abstract void OnEnter();
		protected abstract void OnExit(GameState next, ExitReason reason);
		protected abstract void OnUpdate();
		public virtual void AddGameObject(IGameObject gameObject, bool autoSetActive = false)
		{
		}
		public virtual void RemoveGameObject(IGameObject gameObject)
		{
		}
		public abstract void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr);
		protected virtual void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
		}
		internal void Prepare(GameState prev, object[] parms)
		{
			ScriptHost.Engine.RenderingEnabled = false;
			try
			{
				GameState.Trace(string.Format("Preparing {0} for transition from {1}.", this.Name, (prev != null) ? prev.Name : "nothing"));
				this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_PanelMessage);
				this.OnPrepare(prev, parms);
			}
			finally
			{
				ScriptHost.Engine.RenderingEnabled = true;
			}
		}
		internal void Enter()
		{
			ScriptHost.Engine.RenderingEnabled = false;
			try
			{
				GameState.Trace(string.Format("Entering {0}.", this.Name));
				this.App.UI.UnlockUI();
				if (this.App.GameSettings.AudioEnabled)
				{
					this.App.PostEnableAllSounds();
				}
				this.OnEnter();
				this.App.HotKeyManager.SyncKeyProfile(this.Name);
				this.App.HotKeyManager.SetEnabled(true);
				this.App.PostEngineMessage(new object[]
				{
					InteropMessageID.IMID_ENGINE_EXITSTATE
				});
				this._entered = true;
				if (this.App.Game != null)
				{
					this.App.Game.ShowCombatDialog(true, this);
				}
			}
			finally
			{
				ScriptHost.Engine.RenderingEnabled = true;
			}
		}
		internal void Exit(GameState next, ExitReason reason)
		{
			GameState.Trace(string.Format("Exiting {0}.", this.Name));
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			if (this.App.Game != null)
			{
				this.App.Game.ShowCombatDialog(false, null);
			}
			this.OnExit(next, reason);
			this._entered = false;
		}
		internal void Update()
		{
			this.OnUpdate();
		}
		protected GameState(App game)
		{
			this.App = game;
		}
		public override string ToString()
		{
			return this.Name;
		}
		private void UICommChannel_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "screen_loaded")
			{
				this._guiLoaded = true;
				return;
			}
			if (!this.IsScreenState || (this._guiLoaded && this._entered))
			{
				this.UICommChannel_OnPanelMessage(panelName, msgType, msgParams);
			}
		}
		public virtual bool IsReady()
		{
			return !this.IsScreenState || this._guiLoaded;
		}
	}
}
