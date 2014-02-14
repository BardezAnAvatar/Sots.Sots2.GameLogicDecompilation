using Kerberos.Sots.Engine;
using Kerberos.Sots.UI;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class LoadGameState : GameState
	{
		public LoadGameState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			base.App.UI.CreateDialog(new LoadGameDialog(base.App, "fun"), null);
		}
		protected override void OnEnter()
		{
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
		}
		protected override void OnUpdate()
		{
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
