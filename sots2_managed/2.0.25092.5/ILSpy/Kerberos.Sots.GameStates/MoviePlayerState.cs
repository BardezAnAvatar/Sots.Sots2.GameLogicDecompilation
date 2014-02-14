using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class MoviePlayerState : GameState
	{
		private string _movie;
		private Action _action;
		private bool _actionPerformed;
		private bool _preparingState;
		private GameState _nextState;
		private object[] _nextStateParams;
		private bool _isMultiplayer;
		private bool _movieDone;
		private bool _switchStates;
		public bool SwitchStates
		{
			get
			{
				return this._switchStates;
			}
			set
			{
				this._switchStates = value;
			}
		}
		public override bool IsTransitionState
		{
			get
			{
				return true;
			}
		}
		private bool IsNextStateReady()
		{
			return this._nextState != null && this._nextState.IsReady();
		}
		public MoviePlayerState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (parms.Length > 0)
			{
				this._movie = (string)parms[0];
				this._action = (Action)parms[1];
				this._nextState = (GameState)parms[2];
				this._isMultiplayer = (bool)parms[3];
				this._nextStateParams = (object[])parms[4];
			}
			else
			{
				this._movie = "movies\\Paradox_Interactive_ID_uncompressed.bik";
				this._action = null;
				this._nextState = base.App.GetGameState<MainMenuState>();
				this._isMultiplayer = false;
			}
			base.App.UI.LoadScreen("MoviePlayer");
		}
		protected override void OnEnter()
		{
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			base.App.UI.SetScreen("MoviePlayer");
			base.App.UI.SetPropertyString("movie", "movie", this._movie);
		}
		protected override void OnExit(GameState next, ExitReason reason)
		{
			base.App.UI.SetPropertyBool("movie", "movie_done", true);
			this._movie = null;
			this._action = null;
			this._actionPerformed = false;
			this._preparingState = false;
			this._nextState = null;
			this._nextStateParams = null;
			base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
		}
		protected override void OnUpdate()
		{
			if (!this._movieDone)
			{
				return;
			}
			if (!this._actionPerformed)
			{
				this._actionPerformed = true;
				if (this._action != null)
				{
					this._action();
				}
			}
			if (this._actionPerformed && !this._preparingState)
			{
				this._preparingState = true;
				base.App.PrepareGameState(this._nextState, this._nextStateParams);
			}
			if (this._nextState.IsReady())
			{
				base.App.SwitchToPreparedGameState();
			}
			if (this._isMultiplayer)
			{
				if (this._nextState.IsReady())
				{
					base.App.Network.Ready();
				}
				if (this._switchStates && this._nextState.IsReady())
				{
					base.App.SwitchToPreparedGameState();
				}
			}
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "movie_done")
			{
				this._movieDone = true;
			}
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				base.App.UI.SetPropertyBool("movie", "movie_done", true);
			}
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
