using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class TestAssetsState : GameState
	{
		private const float DistFromCenter = 0f;
		private GameObjectSet _crits;
		private CombatInput _combatInput;
		private CombatGrid _combatGrid;
		private Sky _sky;
		private OrbitCameraController _camera;
		private Random _rand = new Random();
		private Ship _ship;
		private Ship _ship2;
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			base.App.NewGame();
			this._crits = new GameObjectSet(base.App);
			this._sky = new Sky(base.App, SkyUsage.InSystem, 0);
			this._camera = this._crits.Add<OrbitCameraController>(new object[0]);
			this._camera.TargetPosition = new Vector3(0f, 0f, 0f);
			this._combatInput = this._crits.Add<CombatInput>(new object[0]);
			this._combatGrid = this._crits.Add<CombatGrid>(new object[0]);
			CreateShipParams createShipParams = new CreateShipParams();
			createShipParams.player = base.App.LocalPlayer;
			createShipParams.sections = 
				from x in base.App.AssetDatabase.ShipSections
				where x.Faction == "morrigi" && x.Class == ShipClass.Cruiser && (x.FileName.Contains("cr_cmd.section") || x.FileName.Contains("cr_eng_fusion.section") || x.FileName.Contains("cr_mis_armor.section"))
				select x;
			createShipParams.turretHousings = base.App.AssetDatabase.TurretHousings;
			createShipParams.weapons = base.App.AssetDatabase.Weapons;
			createShipParams.preferredWeapons = base.App.AssetDatabase.Weapons;
			createShipParams.modules = base.App.AssetDatabase.Modules;
			createShipParams.preferredModules = base.App.AssetDatabase.Modules;
			createShipParams.psionics = base.App.AssetDatabase.Psionics;
			createShipParams.faction = base.App.AssetDatabase.GetFaction("morrigi");
			createShipParams.shipName = "BOOGER";
			createShipParams.inputID = this._combatInput.ObjectID;
			this._ship = Ship.CreateShip(base.App, createShipParams);
			this._crits.Add(this._ship);
			this._ship2 = Ship.CreateShip(base.App, createShipParams);
			this._ship2.Position = new Vector3(1000f, 0f, 0f);
			this._crits.Add(this._ship2);
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("Combat");
			this._camera.DesiredDistance = 30f;
			this._combatGrid.GridSize = 5000f;
			this._combatGrid.CellSize = 500f;
			this._combatInput.CameraID = this._camera.ObjectID;
			this._combatInput.CombatGridID = this._combatGrid.ObjectID;
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
		public TestAssetsState(App game) : base(game)
		{
		}
	}
}
