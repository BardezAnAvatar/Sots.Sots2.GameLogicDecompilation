using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class TestNetworkState : GameState
	{
		private readonly List<string> _networkLog = new List<string>();
		public TestNetworkState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			base.App.UI.LoadScreen("TestNetwork");
		}
		protected override void OnEnter()
		{
			base.App.GameSetup.IsMultiplayer = true;
			base.App.UI.SetScreen("TestNetwork");
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
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
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "host_test_button")
			{
				base.App.SwitchGameStateViaLoadingScreen(null, null, base.App.GetGameState<CombatState>(), new object[0]);
			}
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "netlog")
			{
				this.ProcessGameEvent_NetworkLog(eventParams);
			}
		}
		private void ProcessGameEvent_NetworkLog(string[] eventParams)
		{
			for (int i = 0; i < eventParams.Length; i++)
			{
				string text = eventParams[i];
				if (!string.IsNullOrWhiteSpace(text))
				{
					this._networkLog.Add(text);
					base.App.UI.AddItem("network_log", "", this._networkLog.Count, text);
				}
			}
		}
	}
}
