using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class LoadingScreenState : GameState
	{
		private float _minTime;
		private string _text;
		private string _image;
		private Action _action;
		private LoadingFinishedDelegate _loadingFinishedDelegate;
		private bool _actionPerformed;
		private bool _preparingState;
		private GameState _nextState;
		private GameState _prevState;
		private object[] _nextStateParams;
		private DateTime _startTime;
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
		public GameState PreviousState
		{
			get
			{
				return this._prevState;
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
		public LoadingScreenState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._minTime = (float)parms[0];
			this._text = (string)parms[1];
			this._image = (string)parms[2];
			this._action = (Action)parms[3];
			this._loadingFinishedDelegate = (LoadingFinishedDelegate)parms[4];
			this._nextState = (GameState)parms[5];
			this._nextStateParams = (object[])parms[6];
			this._actionPerformed = false;
			this._prevState = prev;
			base.App.UI.LoadScreen("LoadingScreen");
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("LoadingScreen");
			this._startTime = DateTime.Now;
			base.App.UI.SetText("SplashText", this._text);
			base.App.UI.SetPropertyString("SplashImage", "texture", this._image);
			base.App.UI.Update();
		}
		protected override void OnExit(GameState next, ExitReason reason)
		{
			base.App.UI.DeleteScreen("LoadingScreen");
			this._minTime = 0f;
			this._text = null;
			this._image = null;
			this._action = null;
			this._actionPerformed = false;
			this._preparingState = false;
			this._nextState = null;
			this._nextStateParams = null;
		}
		protected override void OnUpdate()
		{
			TimeSpan t = DateTime.Now - this._startTime;
			if (this._loadingFinishedDelegate != null && !this._loadingFinishedDelegate())
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
			if (t >= TimeSpan.FromSeconds((double)this._minTime + 2.0) && this._nextState.IsReady())
			{
				base.App.SwitchToPreparedGameState();
			}
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
