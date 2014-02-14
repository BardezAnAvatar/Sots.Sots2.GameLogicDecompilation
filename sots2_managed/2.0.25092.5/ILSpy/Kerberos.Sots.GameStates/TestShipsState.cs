using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Xml;
namespace Kerberos.Sots.GameStates
{
	internal class TestShipsState : CommonCombatState
	{
		private XmlDocument _config;
		public TestShipsState(App game) : base(game)
		{
		}
		protected override void OnCombatEnding()
		{
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			this._config = new XmlDocument();
			this._config.Load(ScriptHost.FileSystem, "data\\TestShipsConfig.xml");
			int num = 0;
			base.OnPrepare(prev, new object[]
			{
				num,
				this._config,
				true
			});
		}
		protected override void OnEnter()
		{
			base.OnEnter();
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.OnExit(prev, reason);
			this._config = null;
		}
		protected override void SyncPlayerList()
		{
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
