using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class SplashState : GameState
	{
		private GameState _initialState;
		public SplashState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._initialState = (GameState)parms[0];
			base.App.UI.LoadScreen("Splash");
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("Splash");
			base.App.UI.Update();
			if (!base.App.IsInitialized())
			{
				base.App.Initialize();
			}
			base.App.PostRequestGuiSound("universal_kerbgrowl");
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this._initialState = null;
			base.App.UI.DeleteScreen("Splash");
		}
		protected override void OnUpdate()
		{
			if (base.App.IsInitialized())
			{
				base.App.SwitchGameState(this._initialState, new object[0]);
			}
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
