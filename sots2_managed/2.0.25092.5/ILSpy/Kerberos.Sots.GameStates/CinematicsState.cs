using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class CinematicsState : GameState
	{
		public CinematicsState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			base.App.UI.LoadScreen("Cinematics");
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("Cinematics");
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "gameBackButton")
			{
				base.App.SwitchGameState("MainMenuState");
			}
		}
		protected override void OnUpdate()
		{
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
