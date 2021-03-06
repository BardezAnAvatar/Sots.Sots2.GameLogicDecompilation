using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class TestAnimationState : GameState
	{
		private GameObjectSet _crits;
		private CombatInput _combatInput;
		private CombatGrid _combatGrid;
		private OrbitCameraController _camera;
		private StaticModel _bigbad;
		private StaticModel _litmus1;
		private StaticModel _litmus2;
		private StaticModel _litmus3;
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._crits = new GameObjectSet(base.App);
			this._crits.Add(new Sky(base.App, SkyUsage.InSystem, 0));
			this._camera = this._crits.Add<OrbitCameraController>(new object[0]);
			this._camera.DesiredDistance = 30f;
			this._combatGrid = this._crits.Add<CombatGrid>(new object[0]);
			this._combatGrid.GridSize = 5000f;
			this._combatGrid.CellSize = 500f;
			this._combatInput = this._crits.Add<CombatInput>(new object[0]);
			this._combatInput.CameraID = this._camera.ObjectID;
			this._combatInput.CombatGridID = this._combatGrid.ObjectID;
			this._bigbad = this._crits.Add<StaticModel>(new object[]
			{
				"\\base\\factions\\zuul\\models\\Suul'ka\\suulka_test.scene"
			});
			this._litmus1 = this._crits.Add<StaticModel>(new object[]
			{
				"\\base\\props\\litmus_01.scene"
			});
			this._litmus2 = this._crits.Add<StaticModel>(new object[]
			{
				"\\base\\props\\litmus_02.scene"
			});
			this._litmus3 = this._crits.Add<StaticModel>(new object[]
			{
				"\\base\\props\\litmus_03.scene"
			});
			this._bigbad.Position = new Vector3(0f, -800f, 0f);
			this._litmus1.Position = new Vector3(-300f, 0f, 0f);
			this._litmus2.Position = new Vector3(0f, 0f, 0f);
			this._litmus3.Position = new Vector3(300f, 0f, 0f);
		}
		protected override void OnEnter()
		{
			this._crits.Activate();
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			if (this._crits != null)
			{
				this._crits.Dispose();
			}
		}
		protected override void OnUpdate()
		{
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		public TestAnimationState(App game) : base(game)
		{
		}
	}
}
