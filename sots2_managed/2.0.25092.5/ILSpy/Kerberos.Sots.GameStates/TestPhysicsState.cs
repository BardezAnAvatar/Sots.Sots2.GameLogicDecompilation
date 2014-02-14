using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class TestPhysicsState : GameState
	{
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private CombatInput _input;
		private Sky _sky;
		private CombatGrid _grid;
		private Random _rnd;
		private bool hack1;
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._rnd = new Random();
			this._crits = new GameObjectSet(base.App);
			this._sky = new Sky(base.App, SkyUsage.InSystem, 0);
			this._crits.Add(this._sky);
			this._camera = this._crits.Add<OrbitCameraController>(new object[0]);
			this._input = this._crits.Add<CombatInput>(new object[0]);
			this._grid = this._crits.Add<CombatGrid>(new object[0]);
		}
		protected override void OnEnter()
		{
			this._sky.Active = true;
			this._grid.GridSize = 1000f;
			this._grid.CellSize = 50f;
			this._grid.Active = true;
			this._input.Active = true;
			this._input.CameraID = this._camera.ObjectID;
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
			bool arg_06_0 = this.hack1;
			this.hack1 = !this.hack1;
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		public TestPhysicsState(App game) : base(game)
		{
		}
	}
}
