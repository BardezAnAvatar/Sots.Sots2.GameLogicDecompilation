using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Xml;
namespace Kerberos.Sots.GameStates
{
	internal class CologneShipsState : CommonCombatState
	{
		private XmlDocument _config;
		public CologneShipsState(App game) : base(game)
		{
		}
		protected override void OnCombatEnding()
		{
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			this._config = new XmlDocument();
			this._config.Load(ScriptHost.FileSystem, "data\\CologneShipsConfig.xml");
			Vector3 zero = Vector3.Zero;
			int num = 0;
			base.OnPrepare(prev, new object[]
			{
				this._config,
				zero,
				num
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
