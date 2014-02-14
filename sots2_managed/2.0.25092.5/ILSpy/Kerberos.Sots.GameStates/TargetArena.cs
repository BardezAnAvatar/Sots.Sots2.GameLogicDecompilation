using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class TargetArena : IDisposable
	{
		private App _game;
		private GameObjectSet _objects;
		private GameObjectSet _targetObjects;
		private bool _activated;
		private bool _ready;
		private bool _targetsLoaded;
		private RealShipClasses _currentShipClass;
		private Player _targetPlayer;
		private ShipSectionAsset _section;
		private WeaponTestWeaponLauncher _launcher;
		private string _faction;
		private string _modelFile;
		private Ship[] _targets;
		private float _goalDist;
		private readonly Vector3[] TargetDirs;
		public int WeaponLauncherID
		{
			get
			{
				if (this._launcher == null)
				{
					return 0;
				}
				return this._launcher.ObjectID;
			}
		}
		private Matrix[] GetTargetTransforms(float goalDist)
		{
			return this.TargetDirs.Select(delegate(Vector3 x)
			{
				Vector3 vector = -x;
				float num = Vector3.Dot(vector, Vector3.UnitY);
				Vector3 up = (num > 0.99f) ? (-Vector3.UnitZ) : ((num < -0.99f) ? Vector3.UnitZ : Vector3.UnitY);
				return Matrix.CreateWorld(x * goalDist, vector, up);
			}).ToArray<Matrix>();
		}
		private Ship AddTarget(Vector3 pos, Vector3 rot)
		{
			CreateShipParams createShipParams = new CreateShipParams();
			createShipParams.player = this._targetPlayer;
			createShipParams.sections = new ShipSectionAsset[]
			{
				this._section
			};
			createShipParams.turretHousings = this._game.AssetDatabase.TurretHousings;
			createShipParams.weapons = this._game.AssetDatabase.Weapons;
			createShipParams.psionics = this._game.AssetDatabase.Psionics;
			createShipParams.faction = this._game.AssetDatabase.Factions.First((Faction x) => this._faction == x.Name);
			createShipParams.isKillable = false;
			createShipParams.enableAI = false;
			Ship ship = Ship.CreateShip(this._game, createShipParams);
			ship.Position = pos;
			ship.Rotation = rot;
			this._targetObjects.Add(ship);
			return ship;
		}
        public TargetArena(App game, string faction)
        {
            Func<Faction, bool> predicate = null;
            this._currentShipClass = RealShipClasses.NumShipClasses;
            this._modelFile = @"props\models\Target_Engine.scene";
            this._targets = new Ship[6];
            this._goalDist = 400f;
            this.TargetDirs = new Vector3[] { new Vector3(0f, 1f, 0f), new Vector3(0f, -1f, 0f), new Vector3(1f, 0f, 0f), new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, -1f) };
            this._game = game;
            this._objects = new GameObjectSet(game);
            this._targetObjects = new GameObjectSet(game);
            this._faction = faction;
            PlayerInfo pi = new PlayerInfo
            {
                AvatarAssetPath = string.Empty,
                BadgeAssetPath = string.Empty,
                PrimaryColor = Vector3.One,
                SecondaryColor = Vector3.One
            };
            this._targetPlayer = new Player(game, null, pi, Player.ClientTypes.AI);
            this._objects.Add(this._targetPlayer);
            if (predicate == null)
            {
                predicate = x => x.Name == faction;
            }
            Faction faction2 = game.AssetDatabase.Factions.FirstOrDefault<Faction>(predicate);
            if (faction2 == null)
            {
                faction2 = game.AssetDatabase.Factions.First<Faction>();
            }
            LogicalWeapon weapon = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == "Mis_Missile");
            LogicalWeapon weapon2 = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == weapon.SubWeapon);
            WeaponModelPaths weaponModelPaths = LogicalWeapon.GetWeaponModelPaths(weapon, faction2);
            WeaponModelPaths weaponModelPaths2 = LogicalWeapon.GetWeaponModelPaths(weapon2, faction2);
            weapon.AddGameObjectReference();
            this._launcher = game.AddObject<WeaponTestWeaponLauncher>(new List<object> { this._targetPlayer.ObjectID, (weapon.GameObject != null) ? weapon.GameObject.ObjectID : 0, weaponModelPaths.ModelPath, weaponModelPaths2.ModelPath }.ToArray());
            this._objects.Add(this._launcher);
            this.ResetTargets();
        }
        public void ResetTargets()
		{
			this._targetObjects.Clear(true);
			this._targetsLoaded = false;
			this._section = new ShipSectionAsset
			{
				Banks = new LogicalBank[0],
				Modules = new LogicalModuleMount[0],
				ExcludeSections = new string[0],
				Class = ShipClass.BattleRider,
				Faction = this._faction,
				FileName = string.Empty,
				Structure = 100,
				Maneuvering = new ShipManeuveringInfo
				{
					LinearAccel = 200f,
					LinearSpeed = 400f,
					RotationSpeed = 300f,
					RotAccel = new Vector3(100f, 100f, 100f)
				},
				ManeuveringType = "Fast",
				Mass = 15000f,
				ModelName = this._modelFile,
				DestroyedModelName = this._modelFile,
				DamageEffect = new LogicalEffect(),
				DeathEffect = new LogicalEffect(),
				ReactorFailureDeathEffect = new LogicalEffect(),
				ReactorCriticalDeathEffect = new LogicalEffect(),
				AbsorbedDeathEffect = new LogicalEffect(),
				Mounts = new LogicalMount[0],
				PsionicAbilities = new SectionEnumerations.PsionicAbility[0],
				Type = ShipSectionType.Mission
			};
			Matrix[] targetTransforms = this.GetTargetTransforms(this._goalDist);
			for (int i = 0; i < targetTransforms.Length; i++)
			{
				this._targets[i] = this.AddTarget(targetTransforms[i].Position, targetTransforms[i].EulerAngles);
			}
		}
		public void PositionsChanged()
		{
			Matrix[] targetTransforms = this.GetTargetTransforms(this._goalDist);
			for (int i = 0; i < targetTransforms.Length; i++)
			{
				this._targets[i].Maneuvering.PostAddGoal(targetTransforms[i].Position, targetTransforms[i].Forward);
			}
		}
		public void SetShipClass(RealShipClasses value)
		{
			if (this._currentShipClass == value)
			{
				return;
			}
			switch (value)
			{
			case RealShipClasses.Cruiser:
			case RealShipClasses.BattleCruiser:
				this._goalDist = 750f;
				this._modelFile = "props\\models\\CR_Target.scene";
				break;
			case RealShipClasses.Dreadnought:
			case RealShipClasses.BattleShip:
				this._goalDist = 1500f;
				this._modelFile = "props\\models\\DN_Target.scene";
				break;
			case RealShipClasses.Leviathan:
				this._goalDist = 2000f;
				this._modelFile = "props\\models\\LV_Target.scene";
				break;
			case RealShipClasses.BattleRider:
			case RealShipClasses.Drone:
			case RealShipClasses.BoardingPod:
			case RealShipClasses.EscapePod:
			case RealShipClasses.AssaultShuttle:
			case RealShipClasses.Biomissile:
				this._goalDist = 300f;
				this._modelFile = "props\\models\\BR_Target.scene";
				break;
			case RealShipClasses.Station:
				this._goalDist = 2500f;
				this._modelFile = "props\\models\\LV_Target.scene";
				break;
			}
			this._currentShipClass = value;
			this.ResetTargets();
			this.PositionsChanged();
		}
		public void Activate()
		{
			this._activated = true;
		}
		public void Update()
		{
			if (this._activated && !this._ready && this._objects.IsReady())
			{
				this._ready = true;
				this._objects.Activate();
			}
			if (this._activated && !this._targetsLoaded && this._targetObjects.IsReady())
			{
				this.PositionsChanged();
				this._targetObjects.Activate();
				Ship[] targets = this._targets;
				for (int i = 0; i < targets.Length; i++)
				{
					Ship ship = targets[i];
					ship.Maneuvering.PostSetProp("CanAvoid", false);
				}
				this._targetsLoaded = true;
			}
		}
		public void ResetTargetPositions()
		{
			Ship[] targets = this._targets;
			for (int i = 0; i < targets.Length; i++)
			{
				Ship ship = targets[i];
				ship.Maneuvering.PostSetProp("ResetPosition", new object[0]);
				ship.PostSetProp("ClearDamageVisuals", new object[0]);
			}
		}
		public void LaunchWeapon(IGameObject target, int numLaunches)
		{
			if (this._launcher != null)
			{
				int num = (target != null) ? target.ObjectID : 0;
				this._launcher.PostSetProp("Fire", new object[]
				{
					numLaunches,
					num
				});
			}
		}
		public void Dispose()
		{
			this._objects.Dispose();
			this._targetObjects.Dispose();
		}
	}
}
