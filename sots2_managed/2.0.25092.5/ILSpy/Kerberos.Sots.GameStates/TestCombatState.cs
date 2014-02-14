using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class TestCombatState : GameState
	{
		public TestCombatState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			base.App.UI.LoadScreen("TestCombat");
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("TestCombat");
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
		}
		protected override void OnExit(GameState next, ExitReason reason)
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
