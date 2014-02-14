using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Ships;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIP)]
	internal class Ship : CompoundGameObject, IDisposable, IPosition, IOrientatable, IActive
	{
		public class DetectionState
		{
			public int playerID;
			public bool spotted;
			public bool scanned;
			public DetectionState(int player_id)
			{
				this.playerID = player_id;
				this.spotted = false;
				this.scanned = false;
			}
		}
		private readonly List<IGameObject> _objects = new List<IGameObject>();
		private Faction _faction;
		private int _currentTurretIndex;
		private int _currentModuleIndex;
		private bool _active;
		private bool _checkStatusBootstrapped;
		private bool _isDeployed;
		private bool _isDisposed;
		private bool _isUnderAttack;
		private float _accuracyModifier;
		private float _pdAccuracyModifier;
		private int _parentID;
		private int _parentDatabaseID;
		private SuulkaType _suulkaType;
		private bool _isSystemDefenceBoat;
		private bool _defenseBoatActive;
		private bool _isPolice;
		private bool _isPolicePatrolling;
		private bool _isQShip;
		private bool _isTrapDrone;
		private bool _isAcceleratorHoop;
		private bool _isListener;
		private bool _isLoa;
		private bool _isGardener;
		private Sphere _boundingSphere;
		private List<SpecWeaponControl> _weaponControls;
		private Turret.FiringEnum _turretFiring;
		private Player _player;
		private bool _visible;
		private CombatStance _stance;
		private int _currentPsiPower;
		private int _maxPsiPower;
		private float _sensorRange;
		private float _bonusSpottedRange;
		private CloakedState _cloakedState;
		private int _databaseID;
		private int _designID;
		private int _reserveSize;
		private int _riderIndex;
		private ShipClass _shipClass;
		private RealShipClasses _realShipClass;
		private ShipRole _shipRole;
		private SectionEnumerations.CombatAiType _combatAI;
		private ShipFleetAbilityType _abilityType;
		private BattleRiderTypes _battleRiderType;
		private float _signature;
		private List<Ship.DetectionState> _detectionStates;
		private int _inputId;
		private IGameObject _target;
		private bool _blindFireActive;
		private TaskGroup _taskGroup;
		private bool _isPlayerControlled;
		private WeaponRole _wpnRole;
		private string _priorityWeapon;
		private bool _bIsDriveless;
		private bool _bIsDestroyed;
		private bool _bIsNeutronStar;
		private bool _bHasRetreated;
		private bool _bHitByNodeCannon;
		private bool _instantlyKilled;
		private bool _bCanAcceptMoveOrders;
		private bool _bDockedWithParent;
		private bool _bCarrierCanLaunch;
		private bool _bIsPlanetAssaultShip;
		private bool _bAssaultingPlanet;
		private bool _bCanAvoid;
		private bool _bAuthoritive = true;
		public bool IsUnderAttack
		{
			get
			{
				return this._isUnderAttack;
			}
		}
		public float AccuracyModifier
		{
			get
			{
				return this._accuracyModifier;
			}
		}
		public float PDAccuracyModifier
		{
			get
			{
				return this._pdAccuracyModifier;
			}
		}
		public int ParentID
		{
			get
			{
				return this._parentID;
			}
			set
			{
				this._parentID = value;
			}
		}
		public int ParentDatabaseID
		{
			get
			{
				return this._parentDatabaseID;
			}
			set
			{
				this._parentDatabaseID = value;
			}
		}
		public bool IsSuulka
		{
			get
			{
				return this._suulkaType != SuulkaType.None;
			}
		}
		public bool IsSystemDefenceBoat
		{
			get
			{
				return this._isSystemDefenceBoat;
			}
		}
		public bool DefenseBoatActive
		{
			get
			{
				return this._defenseBoatActive;
			}
			set
			{
				if (!this._isSystemDefenceBoat || this._defenseBoatActive || !value)
				{
					return;
				}
				this.PostSetProp("ActivateDefenseBoat", new object[0]);
				this._defenseBoatActive = value;
			}
		}
		public bool IsPolice
		{
			get
			{
				return this._isPolice;
			}
		}
		public bool IsPolicePatrolling
		{
			get
			{
				return this._isPolicePatrolling;
			}
		}
		public bool IsQShip
		{
			get
			{
				return this._isQShip;
			}
		}
		public bool IsTrapDrone
		{
			get
			{
				return this._isTrapDrone;
			}
		}
		public bool IsAcceleratorHoop
		{
			get
			{
				return this._isAcceleratorHoop;
			}
		}
		public bool IsListener
		{
			get
			{
				return this._isListener;
			}
		}
		public bool IsNPCFreighter
		{
			get
			{
				return this._shipRole == ShipRole.FREIGHTER && !this._isQShip;
			}
		}
		public bool IsLoa
		{
			get
			{
				return this._isLoa;
			}
		}
		public bool IsGardener
		{
			get
			{
				return this._isGardener;
			}
		}
		public Sphere ShipSphere
		{
			get
			{
				return this._boundingSphere;
			}
		}
		public List<SpecWeaponControl> WeaponControls
		{
			get
			{
				return this._weaponControls;
			}
		}
		public bool WeaponControlsIsInitilized
		{
			get
			{
				return this._weaponControls != null;
			}
		}
		public bool IsCarrier
		{
			get
			{
				return this.BattleRiderSquads.Count<BattleRiderSquad>() > 0;
			}
		}
		public bool IsBattleRider
		{
			get
			{
				return this is BattleRiderShip;
			}
		}
		public bool IsWraithAbductor
		{
			get
			{
				return this is WraithAbductorShip;
			}
		}
		public Faction Faction
		{
			get
			{
				return this._faction;
			}
		}
		public ShipManeuvering Maneuvering
		{
			get
			{
				return this._objects.OfType<ShipManeuvering>().First<ShipManeuvering>();
			}
		}
		public IEnumerable<WeaponBank> WeaponBanks
		{
			get
			{
				return this._objects.OfType<WeaponBank>();
			}
		}
		public RigidBody RigidBody
		{
			get
			{
				return this._objects.OfType<RigidBody>().First<RigidBody>();
			}
		}
		public Shield Shield
		{
			get
			{
				return this._objects.OfType<Shield>().FirstOrDefault<Shield>();
			}
		}
		public CompoundCollisionShape CompoundCollisionShape
		{
			get
			{
				return this._objects.OfType<CompoundCollisionShape>().First<CompoundCollisionShape>();
			}
		}
		public IEnumerable<GenericCollisionShape> TurretShapes
		{
			get
			{
				return this._objects.OfType<GenericCollisionShape>();
			}
		}
		public IEnumerable<BattleRiderSquad> BattleRiderSquads
		{
			get
			{
				return this._objects.OfType<BattleRiderSquad>();
			}
		}
		public IEnumerable<BattleRiderMount> BattleRiderMounts
		{
			get
			{
				return this._objects.OfType<BattleRiderMount>();
			}
		}
		public IEnumerable<MountObject> MountObjects
		{
			get
			{
				return this._objects.OfType<MountObject>();
			}
		}
		public IEnumerable<CollisionShape> CollisionShapes
		{
			get
			{
				return this._objects.OfType<CollisionShape>();
			}
		}
		public IEnumerable<CollisionShape> SectionCollisionShapes
		{
			get
			{
				return 
					from x in this._objects.OfType<CollisionShape>()
					where x.GetTag() is ShipSectionAsset
					select x;
			}
		}
		public IEnumerable<CollisionShape> ModuleCollisionShapes
		{
			get
			{
				return 
					from x in this._objects.OfType<CollisionShape>()
					where x.GetTag() is Module
					select x;
			}
		}
		public IEnumerable<Section> Sections
		{
			get
			{
				return 
					from x in this._objects.OfType<Section>()
					where x.GetTag() is ShipSectionAsset
					select x;
			}
		}
		public IEnumerable<Turret> Turrets
		{
			get
			{
				return this._objects.OfType<Turret>();
			}
		}
		public IEnumerable<TurretBase> TurretBases
		{
			get
			{
				return this._objects.OfType<TurretBase>();
			}
		}
		public IEnumerable<Module> Modules
		{
			get
			{
				return this._objects.OfType<Module>();
			}
		}
		public Section MissionSection
		{
			get
			{
				return this.Sections.First((Section x) => x.GetTag<ShipSectionAsset>().Type == ShipSectionType.Mission);
			}
		}
		public IEnumerable<Psionic> Psionics
		{
			get
			{
				return this._objects.OfType<Psionic>();
			}
		}
		public IEnumerable<AttachableEffectObject> AttachableEffects
		{
			get
			{
				return this._objects.OfType<AttachableEffectObject>();
			}
		}
		public Vector3 Position
		{
			get
			{
				return this.Maneuvering.Position;
			}
			set
			{
				this.RigidBody.PostSetPosition(value);
				this.Maneuvering.Position = value;
			}
		}
		public Vector3 Rotation
		{
			get
			{
				return this.Maneuvering.Rotation;
			}
			set
			{
				this.RigidBody.PostSetRotation(value);
				this.Maneuvering.Rotation = value;
			}
		}
		public Vector3 Velocity
		{
			get
			{
				return this.Maneuvering.Velocity;
			}
		}
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
				{
					return;
				}
				this._active = value;
				this.PostSetActive(this._active);
			}
		}
		public bool Deployed
		{
			get
			{
				return this._isDeployed;
			}
			set
			{
				if (value == this._isDeployed)
				{
					return;
				}
				this._isDeployed = value;
				this.PostSetProp("deployed", true);
				this.DisableManeuvering(true);
			}
		}
		public Turret.FiringEnum ListenTurretFiring
		{
			get
			{
				return this._turretFiring;
			}
		}
		public Player Player
		{
			get
			{
				return this._player;
			}
			set
			{
				this.PostSetPlayer(value.ObjectID);
				this._player = value;
			}
		}
		public bool Visible
		{
			get
			{
				return this._visible;
			}
			set
			{
				this.PostSetProp("SetVisible", value);
				this._visible = value;
			}
		}
		public CombatStance CombatStance
		{
			get
			{
				return this._stance;
			}
		}
		public int CurrentPsiPower
		{
			get
			{
				return this._currentPsiPower;
			}
		}
		public int MaxPsiPower
		{
			get
			{
				return this._maxPsiPower;
			}
		}
		public float SensorRange
		{
			get
			{
				return this._sensorRange;
			}
		}
		public float BonusSpottedRange
		{
			get
			{
				return this._bonusSpottedRange;
			}
		}
		public CloakedState CloakedState
		{
			get
			{
				return this._cloakedState;
			}
		}
		public int DatabaseID
		{
			get
			{
				return this._databaseID;
			}
		}
		public int DesignID
		{
			get
			{
				return this._designID;
			}
		}
		public int ReserveSize
		{
			get
			{
				return this._reserveSize;
			}
		}
		public int RiderIndex
		{
			get
			{
				return this._riderIndex;
			}
		}
		public ShipClass ShipClass
		{
			get
			{
				return this._shipClass;
			}
		}
		public RealShipClasses RealShipClass
		{
			get
			{
				return this._realShipClass;
			}
		}
		public ShipRole ShipRole
		{
			get
			{
				return this._shipRole;
			}
		}
		public SectionEnumerations.CombatAiType CombatAI
		{
			get
			{
				return this._combatAI;
			}
		}
		public ShipFleetAbilityType AbilityType
		{
			get
			{
				return this._abilityType;
			}
		}
		public BattleRiderTypes BattleRiderType
		{
			get
			{
				return this._battleRiderType;
			}
		}
		public float Signature
		{
			get
			{
				return this._signature;
			}
			set
			{
				this._signature = value;
			}
		}
		public int InputID
		{
			get
			{
				return this._inputId;
			}
		}
		public IGameObject Target
		{
			get
			{
				return this._target;
			}
			set
			{
				this._target = value;
			}
		}
		public bool BlindFireActive
		{
			get
			{
				return this._blindFireActive;
			}
			set
			{
				this._blindFireActive = value;
			}
		}
		public TaskGroup TaskGroup
		{
			get
			{
				return this._taskGroup;
			}
			set
			{
				this._taskGroup = value;
			}
		}
		public bool IsPlayerControlled
		{
			get
			{
				return this._isPlayerControlled;
			}
			set
			{
				if (this._isPlayerControlled != value && value && this._taskGroup != null)
				{
					this._taskGroup.RemoveShip(this);
					this._taskGroup = null;
				}
				this._isPlayerControlled = value;
			}
		}
		public WeaponRole WeaponRole
		{
			get
			{
				return this._wpnRole;
			}
		}
		public string PriorityWeaponName
		{
			get
			{
				return this._priorityWeapon;
			}
		}
		public bool IsDriveless
		{
			get
			{
				return this._bIsDriveless;
			}
			set
			{
				this._bIsDriveless = value;
				if (value && this.TaskGroup != null)
				{
					this.TaskGroup.RemoveShip(this);
				}
			}
		}
		public bool IsDestroyed
		{
			get
			{
				return this._bIsDestroyed;
			}
		}
		public bool IsNeutronStar
		{
			get
			{
				return this._bIsNeutronStar;
			}
		}
		public bool HasRetreated
		{
			get
			{
				return this._bHasRetreated;
			}
		}
		public bool HitByNodeCannon
		{
			get
			{
				return this._bHitByNodeCannon;
			}
		}
		public bool InstantlyKilled
		{
			get
			{
				return this._instantlyKilled;
			}
		}
		public bool CanAcceptMoveOrders
		{
			get
			{
				return this._bCanAcceptMoveOrders;
			}
		}
		public bool DockedWithParent
		{
			get
			{
				return this._bDockedWithParent;
			}
		}
		public bool CarrierCanLaunch
		{
			get
			{
				return this._bCarrierCanLaunch;
			}
		}
		public bool IsPlanetAssaultShip
		{
			get
			{
				return this._bIsPlanetAssaultShip;
			}
		}
		public bool AssaultingPlanet
		{
			get
			{
				return this._bAssaultingPlanet;
			}
		}
		public bool CanAvoid
		{
			get
			{
				return this._bCanAvoid;
			}
			set
			{
				if (value != this._bCanAvoid)
				{
					this.Maneuvering.PostSetProp("CanAvoid", value);
				}
				this._bCanAvoid = value;
			}
		}
		public bool Authoritive
		{
			get
			{
				return this._bAuthoritive;
			}
			set
			{
				if (value != this._bAuthoritive)
				{
					foreach (Turret current in this.Turrets)
					{
						current.PostSetProp("IsAutoritive", value);
					}
				}
				this._bAuthoritive = value;
			}
		}
		public void InitializeWeaponControls()
		{
			this._weaponControls = new List<SpecWeaponControl>();
		}
		protected override GameObjectStatus OnCheckStatus()
		{
			GameObjectStatus gameObjectStatus = base.OnCheckStatus();
			if (gameObjectStatus != GameObjectStatus.Ready)
			{
				return gameObjectStatus;
			}
			if (this._objects.Any((IGameObject x) => x.ObjectStatus == GameObjectStatus.Pending))
			{
				return GameObjectStatus.Pending;
			}
			if (!this._checkStatusBootstrapped)
			{
				this._checkStatusBootstrapped = true;
				Section missionSection = this.MissionSection;
				CompoundCollisionShape compoundCollisionShape = this.CompoundCollisionShape;
				foreach (Section current in this.Sections)
				{
					if (current != missionSection)
					{
						current.PostSetParent(missionSection, current.GetTag<ShipSectionAsset>().Type.ToString(), current.GetTag<ShipSectionAsset>().Type.ToString());
					}
				}
				foreach (CollisionShape sectionShape in this.SectionCollisionShapes)
				{
					Section section = this.Sections.First((Section x) => x.GetTag<ShipSectionAsset>() == sectionShape.GetTag<ShipSectionAsset>());
					sectionShape.PostSetAggregate(section);
					foreach (CollisionShape current2 in 
						from x in this.CollisionShapes
						where x.GetTag<CollisionShape>() == sectionShape
						select x)
					{
						current2.PostSetAggregate(section);
					}
					if (section == missionSection)
					{
						sectionShape.PostAttach(compoundCollisionShape);
					}
					else
					{
						sectionShape.PostAttach(section, compoundCollisionShape, section, section.GetTag<ShipSectionAsset>().Type.ToString(), missionSection, section.GetTag<ShipSectionAsset>().Type.ToString());
					}
				}
				foreach (GenericCollisionShape current3 in this.TurretShapes)
				{
					Turret tag = current3.GetTag<Turret>();
					LogicalMount mount = tag.GetTag<LogicalMount>();
					IGameObject gameObject = null;
					if (mount.Bank.Section != null)
					{
						gameObject = this.Sections.First((Section x) => x.GetTag<ShipSectionAsset>() == mount.Bank.Section);
					}
					if (mount.Bank.Module != null)
					{
						gameObject = this.Modules.First((Module x) => x.GetTag<LogicalModule>() == mount.Bank.Module);
					}
					if (gameObject != null)
					{
						current3.PostSetAggregate(tag);
						current3.PostAttach(tag, compoundCollisionShape, tag, "", gameObject, mount.NodeName);
					}
				}
				foreach (CollisionShape moduleShape in this.ModuleCollisionShapes)
				{
					Module module = moduleShape.GetTag<Module>();
					Section socket = this.Sections.First((Section x) => x.GetTag<ShipSectionAsset>() == module.Attachment.Section);
					moduleShape.PostSetAggregate(module);
					foreach (CollisionShape current4 in 
						from x in this.CollisionShapes
						where x.GetTag<CollisionShape>() == moduleShape
						select x)
					{
						current4.PostSetAggregate(module);
					}
					moduleShape.PostAttach(module, compoundCollisionShape, module, "", socket, module.Attachment.NodeName);
				}
				foreach (MountObject mo in this.MountObjects)
				{
					IGameObject parent = this._objects.First((IGameObject x) => x.ObjectID == mo.ParentID);
					mo.PostSetParent(parent, mo.NodeName);
				}
				foreach (TurretBase current5 in this.TurretBases)
				{
					Turret tag2 = current5.GetTag<Turret>();
					LogicalMount mount = tag2.GetTag<LogicalMount>();
					IGameObject gameObject2 = null;
					if (mount.Bank.Section != null)
					{
						gameObject2 = this.Sections.First((Section x) => x.GetTag<ShipSectionAsset>() == mount.Bank.Section);
					}
					if (mount.Bank.Module != null)
					{
						gameObject2 = this.Modules.First((Module x) => x.GetTag<LogicalModule>() == mount.Bank.Module);
					}
					if (gameObject2 != null)
					{
						current5.PostSetParent(gameObject2, mount.NodeName);
					}
				}
				foreach (Module module in this.Modules)
				{
					Section parent2 = this.Sections.First((Section x) => x.GetTag<ShipSectionAsset>() == module.Attachment.Section);
					module.PostSetParent(parent2, module.Attachment.NodeName);
				}
				compoundCollisionShape.PostAttach(this.RigidBody);
				missionSection.PostSetParent(this.RigidBody);
				this.Maneuvering.PostAttach(this.RigidBody);
				IGameObject[] first;
				if (this.Shield != null)
				{
					first = new IGameObject[]
					{
						this.RigidBody,
						this.Maneuvering,
						this.CompoundCollisionShape,
						this.Shield
					};
				}
				else
				{
					first = new IGameObject[]
					{
						this.RigidBody,
						this.Maneuvering,
						this.CompoundCollisionShape
					};
				}
				this.PostObjectAddObjects(first.Concat(this.Sections).Concat(this.Modules).Concat(this.WeaponBanks).Concat(this.MountObjects).Concat(this.TurretBases).Concat(this.BattleRiderSquads).Concat(this.Psionics).Concat(this.AttachableEffects).ToArray<IGameObject>());
				this.RigidBody.PostSetAggregate(this);
				compoundCollisionShape.PostSetProp("SetPairedObject", new object[]
				{
					compoundCollisionShape.ObjectID,
					base.ObjectID
				});
				this._bIsPlanetAssaultShip = this.WeaponBanks.Any((WeaponBank x) => WeaponEnums.IsPlanetAssaultWeapon(x.TurretClass));
			}
			return GameObjectStatus.Ready;
		}
		public static bool IsShipClassBigger(ShipClass sc1, ShipClass sc2, bool sameSizeIsTrue = false)
		{
			if (sc1 == sc2)
			{
				return sameSizeIsTrue;
			}
			bool result = false;
			switch (sc2)
			{
			case ShipClass.Cruiser:
				result = (sc1 != ShipClass.BattleRider);
				break;
			case ShipClass.Dreadnought:
				result = (sc1 == ShipClass.Leviathan);
				break;
			case ShipClass.BattleRider:
				result = true;
				break;
			}
			return result;
		}
		public static bool IsStationSize(RealShipClasses rsc)
		{
			switch (rsc)
			{
			case RealShipClasses.Station:
			case RealShipClasses.Platform:
				return true;
			default:
				return false;
			}
		}
		public static bool IsBattleRiderSize(RealShipClasses rsc)
		{
			switch (rsc)
			{
			case RealShipClasses.BattleRider:
			case RealShipClasses.Drone:
			case RealShipClasses.BoardingPod:
			case RealShipClasses.EscapePod:
			case RealShipClasses.AssaultShuttle:
			case RealShipClasses.Biomissile:
				return true;
			}
			return false;
		}
		public static Ship CreateShip(GameSession game, Matrix world, ShipInfo shipInfo, int parentId, int inputId, int playerId = 0, bool isInDeepSpace = false, IEnumerable<Player> playersInCombat = null)
		{
			DesignInfo designInfo = shipInfo.DesignInfo;
			if (designInfo == null)
			{
				designInfo = game.GameDatabase.GetDesignInfo(shipInfo.DesignID);
			}
			Player player;
			if (playerId != 0)
			{
				player = (game.App.GetGameObject(playerId) as Player);
			}
			else
			{
				player = game.GetPlayerObject(designInfo.PlayerID);
			}
			return Ship.CreateShip(game.App, world, designInfo, shipInfo.ShipName, shipInfo.SerialNumber, parentId, inputId, player, shipInfo.ID, shipInfo.RiderIndex, shipInfo.ParentID, true, false, isInDeepSpace, playersInCombat);
		}
        public static Ship CreateShip(App game, Matrix world, DesignInfo design, string shipName, int serialNumber, int parentId, int inputId, Player player, int shipInfoID = 0, int riderindex = -1, int parentDBID = -1, bool autoAddDrawable = true, bool defenseBoatActive = false, bool isInDeepSpace = false, IEnumerable<Player> playersInCombat = null)
        {
            IEnumerable<string> enumerable = from x in design.DesignSections select x.FilePath;
            IEnumerable<string> weapons = from x in game.AssetDatabase.Weapons select x.Name;
            IEnumerable<string> modules = from x in game.AssetDatabase.Modules select x.ModuleName;
            List<WeaponAssignment> list = new List<WeaponAssignment>();
            List<SectionInstanceInfo> list2 = new List<SectionInstanceInfo>();
            List<ModuleAssignment> list3 = new List<ModuleAssignment>();
            if (shipInfoID != 0)
            {
                list2 = game.GameDatabase.GetShipSectionInstances(shipInfoID).ToList<SectionInstanceInfo>();
            }
            AssignedSectionTechs[] techsArray = new AssignedSectionTechs[3];
            for (int i = 0; i < techsArray.Length; i++)
            {
                techsArray[i] = new AssignedSectionTechs();
            }
            Func<ShipSectionAsset, bool> predicate = null;
            foreach (DesignSectionInfo sectionInfo in design.DesignSections)
            {
                if (predicate == null)
                {
                    predicate = x => x.FileName == sectionInfo.FilePath;
                }
                ShipSectionAsset asset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>(predicate);
                Func<WeaponBankInfo, bool> func = null;
                Func<LogicalWeapon, bool> func2 = null;
                foreach (LogicalBank bank in asset.Banks)
                {
                    if (func == null)
                    {
                        func = x => x.BankGUID == bank.GUID;
                    }
                    WeaponBankInfo info = sectionInfo.WeaponBanks.FirstOrDefault<WeaponBankInfo>(func);
                    bool flag = false;
                    if ((info != null) && info.WeaponID.HasValue)
                    {
                        string weaponName = Path.GetFileNameWithoutExtension(game.GameDatabase.GetWeaponAsset(info.WeaponID.Value));
                        WeaponAssignment assignment2 = new WeaponAssignment
                        {
                            ModuleNode = "",
                            Bank = bank,
                            Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>(weapon => string.Equals(weapon.WeaponName, weaponName, StringComparison.InvariantCultureIgnoreCase)),
                            DesignID = ((info != null) && info.DesignID.HasValue) ? info.DesignID.Value : 0
                        };
                        int? filterMode = info.FilterMode;
                        assignment2.InitialTargetFilter = new int?(filterMode.HasValue ? filterMode.GetValueOrDefault() : 0);
                        int? firingMode = info.FiringMode;
                        assignment2.InitialFireMode = new int?(firingMode.HasValue ? firingMode.GetValueOrDefault() : 0);
                        WeaponAssignment item = assignment2;
                        list.Add(item);
                        flag = true;
                    }
                    if (!flag && !string.IsNullOrEmpty(bank.DefaultWeaponName))
                    {
                        WeaponAssignment assignment4 = new WeaponAssignment
                        {
                            ModuleNode = "",
                            Bank = bank
                        };
                        if (func2 == null)
                        {
                            func2 = weapon => string.Equals(weapon.WeaponName, bank.DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase);
                        }
                        assignment4.Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>(func2);
                        assignment4.DesignID = ((info != null) && info.DesignID.HasValue) ? info.DesignID.Value : 0;
                        WeaponAssignment assignment3 = assignment4;
                        list.Add(assignment3);
                        flag = true;
                    }
                }
                Func<DesignModuleInfo, bool> func3 = null;
                foreach (LogicalModuleMount sectionModule in asset.Modules)
                {
                    string path;
                    if (func3 == null)
                    {
                        func3 = x => x.MountNodeName == sectionModule.NodeName;
                    }
                    DesignModuleInfo info2 = sectionInfo.Modules.FirstOrDefault<DesignModuleInfo>(func3);
                    if (info2 != null)
                    {
                        path = game.GameDatabase.GetModuleAsset(info2.ModuleID);
                        LogicalModule module = game.AssetDatabase.Modules.FirstOrDefault<LogicalModule>(x => x.ModulePath == path);
                        ModuleAssignment assignment7 = new ModuleAssignment
                        {
                            ModuleMount = sectionModule,
                            Module = module,
                            PsionicAbilities = (info2.PsionicAbilities != null) ? (from x in info2.PsionicAbilities select x.Ability).ToArray<SectionEnumerations.PsionicAbility>() : new SectionEnumerations.PsionicAbility[0]
                        };
                        list3.Add(assignment7);
                        if (info2.WeaponID.HasValue)
                        {
                            string weaponPath = game.GameDatabase.GetWeaponAsset(info2.WeaponID.Value);
                            WeaponAssignment assignment5 = new WeaponAssignment
                            {
                                ModuleNode = info2.MountNodeName,
                                Bank = module.Banks[0],
                                Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>(x => x.FileName == weaponPath),
                                DesignID = 0
                            };
                            list.Add(assignment5);
                        }
                    }
                }
                foreach (int num2 in sectionInfo.Techs)
                {
                    techsArray[(int)asset.Type].Techs.Add(num2);
                }
            }
            ShipInfo shipInfo = game.GameDatabase.GetShipInfo(shipInfoID, false);
            IEnumerable<ShipSectionAsset> vSections = from x in enumerable select game.AssetDatabase.ShipSections.First<ShipSectionAsset>(y => y.FileName == x);
            CreateShipParams createShipParams = new CreateShipParams
            {
                AutoAddDrawable = autoAddDrawable,
                player = player
            };
            if (playersInCombat != null)
            {
                createShipParams.playersInCombat = playersInCombat.ToList<Player>();
            }
            createShipParams.sections = vSections;
            createShipParams.sectionInstances = list2;
            createShipParams.turretHousings = game.AssetDatabase.TurretHousings;
            createShipParams.weapons = game.AssetDatabase.Weapons;
            if (design.StationType != StationType.INVALID_TYPE)
            {
                createShipParams.preferredWeapons = game.AssetDatabase.Weapons.Where<LogicalWeapon>(delegate(LogicalWeapon x)
                {
                    if (!weapons.Contains<string>(x.Name))
                    {
                        return false;
                    }
                    if (x.Range <= 1500f)
                    {
                        return x.DefaultWeaponSize == WeaponEnums.WeaponSizes.VeryLight;
                    }
                    return true;
                });
            }
            else
            {
                createShipParams.preferredWeapons = from x in game.AssetDatabase.Weapons
                                                    where weapons.Contains<string>(x.Name)
                                                    select x;
            }
            createShipParams.assignedWeapons = list;
            createShipParams.modules = game.AssetDatabase.Modules;
            createShipParams.preferredModules = from x in game.AssetDatabase.Modules
                                                where modules.Contains<string>(x.ModuleName)
                                                select x;
            createShipParams.assignedModules = list3;
            createShipParams.psionics = game.AssetDatabase.Psionics;
            createShipParams.assignedTechs = techsArray;
            createShipParams.faction = game.AssetDatabase.Factions.First<Faction>(x => vSections.First<ShipSectionAsset>().Faction == x.Name);
            createShipParams.shipName = shipName;
            createShipParams.shipDesignName = (design != null) ? design.Name : "";
            createShipParams.serialNumber = serialNumber;
            createShipParams.parentID = parentId;
            createShipParams.inputID = inputId;
            createShipParams.role = design.Role;
            createShipParams.wpnRole = design.WeaponRole;
            createShipParams.databaseId = shipInfoID;
            createShipParams.designId = design.ID;
            createShipParams.isKillable = true;
            createShipParams.enableAI = true;
            createShipParams.isInDeepSpace = isInDeepSpace;
            createShipParams.riderindex = riderindex;
            createShipParams.parentDBID = parentDBID;
            createShipParams.curPsiPower = (shipInfo != null) ? shipInfo.PsionicPower : 0;
            createShipParams.spawnMatrix = new Matrix?(world);
            createShipParams.defenceBoatIsActive = defenseBoatActive;
            createShipParams.priorityWeapon = design.PriorityWeaponName;
            createShipParams.obtainedSlaves = (shipInfo != null) ? shipInfo.SlavesObtained : 0.0;
            return CreateShip(game, createShipParams);
        }
        public static Ship CreateShip(App game, CreateShipParams createShipParams)
		{
			ShipSectionAsset shipSectionAsset = createShipParams.sections.First((ShipSectionAsset section) => section.Type == ShipSectionType.Mission);
			if (shipSectionAsset.IsWraithAbductor)
			{
				return new WraithAbductorShip(game, createShipParams);
			}
			if (shipSectionAsset.IsBattleRider)
			{
				return new BattleRiderShip(game, createShipParams);
			}
			return new Ship(game, createShipParams);
		}
		public static WeaponModelPaths GetWeaponModelPathsWithFixAssetNameForDLC(LogicalWeapon weapon, Faction faction, string preferredMount)
		{
			WeaponModelPaths weaponModelPaths = LogicalWeapon.GetWeaponModelPaths(weapon, faction);
			weaponModelPaths.ModelPath = Ship.FixAssetNameForDLC(weaponModelPaths.ModelPath, preferredMount);
			weaponModelPaths.DefaultModelPath = Ship.FixAssetNameForDLC(weaponModelPaths.ModelPath, preferredMount);
			return weaponModelPaths;
		}
		public static string GetPreferredMount(App game, Player player, Faction faction, List<ShipSectionAsset> sections)
		{
			Subfaction subfaction = faction.Subfactions[Math.Min(player.SubfactionIndex, faction.Subfactions.Length - 1)];
			string text = string.Empty;
			if (game.LocalPlayer != player || !subfaction.DlcID.HasValue || game.Steam.HasDLC((int)subfaction.DlcID.Value))
			{
				text = subfaction.MountName;
			}
			else
			{
				text = faction.Subfactions[0].MountName;
			}
			foreach (ShipSectionAsset current in sections)
			{
				string str = string.Format("\\{0}\\{1}", text, current.ModelName);
				if (!ScriptHost.FileSystem.FileExists(str + "~"))
				{
					text = string.Empty;
					break;
				}
			}
			return text;
		}
		public static string FixAssetNameForDLC(string unbasedPath, string preferredMount)
		{
			if (string.IsNullOrEmpty(unbasedPath))
			{
				return unbasedPath;
			}
			string text = string.Format("\\{0}\\{1}", preferredMount, unbasedPath);
			if (ScriptHost.FileSystem.FileExists(text + "~"))
			{
				return text;
			}
			text = string.Format("\\eof\\{0}", unbasedPath);
			if (ScriptHost.FileSystem.FileExists(text + "~"))
			{
				return text;
			}
			return string.Format("\\base\\{0}", unbasedPath);
		}
		private void Prepare(App game, CreateShipParams createShipParams)
		{
			string preferredMount = Ship.GetPreferredMount(game, createShipParams.player, createShipParams.faction, createShipParams.sections.ToList<ShipSectionAsset>());
			game.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._taskGroup = null;
			this._bIsDestroyed = false;
			this._bHasRetreated = false;
			this._bHitByNodeCannon = false;
			this._bIsDriveless = true;
			this._bCanAcceptMoveOrders = true;
			this._checkStatusBootstrapped = false;
			this._visible = true;
			this._bDockedWithParent = false;
			this._bCarrierCanLaunch = true;
			this._bIsPlanetAssaultShip = false;
			this._bAssaultingPlanet = false;
			this._bAuthoritive = true;
			this._isUnderAttack = false;
			this._instantlyKilled = false;
			this._isDeployed = false;
			this._priorityWeapon = createShipParams.priorityWeapon;
			this._sensorRange = 0f;
			this._faction = createShipParams.faction;
			this._databaseID = createShipParams.databaseId;
			this._player = createShipParams.player;
			this._bIsNeutronStar = (game.Game != null && game.Game.ScriptModules.NeutronStar != null && game.Game.ScriptModules.NeutronStar.PlayerID == this._player.ID);
			this._isGardener = (game.Game != null && game.Game.ScriptModules.Gardeners != null && game.Game.ScriptModules.Gardeners.GardenerDesignId == createShipParams.designId);
			this._wpnRole = createShipParams.wpnRole;
			this._inputId = createShipParams.inputID;
			this._boundingSphere = new Sphere(createShipParams.player.ObjectID, Vector3.Zero, 10f);
			this._reserveSize = (
				from x in createShipParams.sections
				select x.ReserveSize).Sum();
			this._riderIndex = createShipParams.riderindex;
			this._parentID = createShipParams.parentID;
			this._parentDatabaseID = createShipParams.parentDBID;
			this._turretFiring = Turret.FiringEnum.NotFiring;
			this._blindFireActive = false;
			this._isLoa = (this._faction.Name == "loa");
			this._accuracyModifier = 0f;
			if (this._isLoa)
			{
				this._accuracyModifier = 0.25f;
			}
			if (createShipParams.sections.Any((ShipSectionAsset x) => x.IsFireControl))
			{
				this._accuracyModifier += 0.25f;
			}
			else
			{
				if (createShipParams.sections.Any((ShipSectionAsset x) => x.IsAIControl))
				{
					this._accuracyModifier += 0.5f;
				}
			}
			this._accuracyModifier = Math.Max(1f - this._accuracyModifier, 0f);
			this._currentTurretIndex = 0;
			this._currentTurretIndex = 0;
			this._shipClass = createShipParams.sections.First<ShipSectionAsset>().Class;
			this._realShipClass = createShipParams.sections.First<ShipSectionAsset>().RealClass;
			this._shipRole = createShipParams.role;
			this._isSystemDefenceBoat = false;
			this._defenseBoatActive = false;
			this._isPolice = false;
			this._suulkaType = SuulkaType.None;
			this._isQShip = false;
			this._isAcceleratorHoop = false;
			this._isListener = false;
			this._combatAI = SectionEnumerations.CombatAiType.Normal;
			this._abilityType = ShipFleetAbilityType.None;
			this._battleRiderType = BattleRiderTypes.Unspecified;
			string text = "Medium";
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			foreach (ShipSectionAsset current in createShipParams.sections)
			{
				if (!flag && current.ManeuveringType != "")
				{
					text = current.ManeuveringType;
					flag = true;
				}
				if (current.Type == ShipSectionType.Engine || this._shipClass == ShipClass.BattleRider || this._shipClass == ShipClass.Leviathan)
				{
					this._bIsDriveless = false;
				}
				if (current.Type == ShipSectionType.Mission)
				{
					this._combatAI = current.CombatAIType;
					this._battleRiderType = current.BattleRiderType;
					this._abilityType = current.ShipFleetAbilityType;
					this._suulkaType = current.SuulkaType;
					num = current.StationLevel;
				}
				if (this._shipRole == ShipRole.FREIGHTER && current.FileName.Contains("_qship"))
				{
					this._isQShip = true;
				}
				flag2 = (flag2 || current.IsGravBoat);
				this._isAcceleratorHoop = (this._isAcceleratorHoop || current.IsAccelerator);
				this._isListener = (this._isListener || current.IsListener);
				this._isPolice = (this._isPolice || current.isPolice);
				this._isSystemDefenceBoat = (this._isSystemDefenceBoat || current.RealClass == RealShipClasses.SystemDefenseBoat);
			}
			this._isTrapDrone = (this._combatAI == SectionEnumerations.CombatAiType.TrapDrone);
			this._isPolicePatrolling = this._isPolice;
			if (createShipParams.sections.Count<ShipSectionAsset>() == 1)
			{
				this._bIsDriveless = false;
			}
			this._bCanAcceptMoveOrders = (this._shipClass != ShipClass.Station && (this._shipRole != ShipRole.FREIGHTER || this._isQShip) && !this._isTrapDrone && !this._isAcceleratorHoop && !this._isGardener);
			if (this._bCanAcceptMoveOrders && this._shipClass == ShipClass.BattleRider)
			{
				this._bCanAcceptMoveOrders = (createShipParams.parentID == 0);
			}
			if (!flag)
			{
				switch (this._shipClass)
				{
				case ShipClass.Cruiser:
					text = "Medium";
					break;
				case ShipClass.Dreadnought:
				case ShipClass.Leviathan:
				case ShipClass.Station:
					text = "Slow";
					break;
				case ShipClass.BattleRider:
					text = "Fast";
					break;
				}
			}
			this._bonusSpottedRange = 0f;
			if (num > 0)
			{
				if (num == 5)
				{
					this._bonusSpottedRange = -1f;
				}
				else
				{
					this._bonusSpottedRange += (float)(num - 1) * game.AssetDatabase.GlobalSpotterRangeData.StationLVLOffset;
				}
			}
			if (this._combatAI == SectionEnumerations.CombatAiType.Meteor)
			{
				this._bonusSpottedRange = -1f;
			}
			if (this._bonusSpottedRange == 0f)
			{
				this._bonusSpottedRange = game.AssetDatabase.GlobalSpotterRangeData.SpotterValues[(int)GlobalSpotterRangeData.GetTypeFromShipClass(this._shipClass)];
			}
			List<SectionInstanceInfo> list = createShipParams.sectionInstances.ToList<SectionInstanceInfo>();
			List<PlayerTechInfo> list2 = (this._player != null && game.GameDatabase != null && this._player.IsStandardPlayer) ? game.GameDatabase.GetPlayerTechInfos(this._player.ID).ToList<PlayerTechInfo>() : new List<PlayerTechInfo>();
			List<SectionEnumerations.DesignAttribute> attributes = new List<SectionEnumerations.DesignAttribute>();
			if (createShipParams.designId != 0)
			{
				attributes = game.GameDatabase.GetDesignAttributesForDesign(createShipParams.designId).ToList<SectionEnumerations.DesignAttribute>();
			}
			this._designID = createShipParams.designId;
			DesignInfo designInfo = null;
			ShipInfo shipInfo = null;
			FleetInfo fleetInfo = null;
			List<AdmiralInfo.TraitType> list3 = new List<AdmiralInfo.TraitType>();
			if (game.GameDatabase != null)
			{
				designInfo = game.GameDatabase.GetDesignInfo(this._designID);
				shipInfo = game.GameDatabase.GetShipInfo(createShipParams.databaseId, true);
				if (shipInfo != null)
				{
					fleetInfo = game.GameDatabase.GetFleetInfo(shipInfo.FleetID);
					if (fleetInfo != null)
					{
						list3 = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
					}
				}
			}
			createShipParams.faction.AddFactionReference(game);
			if (shipInfo != null && fleetInfo != null && designInfo != null)
			{
				this._maxPsiPower = ShipInfo.GetMaxPsionicPower(game, designInfo, game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>());
			}
			else
			{
				this._maxPsiPower = (int)(createShipParams.faction.PsionicPowerPerCrew * (float)(
					from x in createShipParams.sections
					select x.Crew).Sum());
			}
			if (this.IsSuulka)
			{
				this._maxPsiPower = (int)(
					from x in createShipParams.sections
					select x.PsionicPowerLevel).Sum();
				this._maxPsiPower = ((this._maxPsiPower > 0) ? this._maxPsiPower : 1000);
			}
			bool flag3 = list.Count == 0;
			this._currentPsiPower = Math.Min(createShipParams.curPsiPower, this._maxPsiPower);
			if (flag3)
			{
				this._currentPsiPower = this._maxPsiPower;
			}
			this._pdAccuracyModifier = Math.Max(1f - Player.GetPDAccuracyBonus(game.AssetDatabase, list2), 0f);
			int num2 = 0;
			List<string> allShipTechsIds = new List<string>();
			AssignedSectionTechs[] assignedTechs = createShipParams.assignedTechs;
			for (int i = 0; i < assignedTechs.Length; i++)
			{
				AssignedSectionTechs assignedSectionTechs = assignedTechs[i];
				if (assignedSectionTechs != null)
				{
					foreach (int current2 in assignedSectionTechs.Techs)
					{
						string techFileID = game.GameDatabase.GetTechFileID(current2);
						allShipTechsIds.Add(techFileID);
						if (techFileID == "IND_Stealth_Armor")
						{
							num2++;
						}
					}
				}
			}
			bool flag4 = createShipParams.isKillable && !this._bIsNeutronStar;
			bool hasStealthTech = num2 == createShipParams.sections.Count<ShipSectionAsset>();
			List<object> list4 = new List<object>();
			list4.Add(0);
			list4.Add((this._player != null) ? this._player.ObjectID : 0);
			list4.Add(createShipParams.inputID);
			list4.Add(createShipParams.AutoAddDrawable);
			list4.Add((!string.IsNullOrEmpty(createShipParams.shipName)) ? createShipParams.shipName.ToUpper() : "");
			list4.Add(createShipParams.serialNumber.ToString());
			list4.Add(createShipParams.shipDesignName);
			list4.Add(this._designID);
			list4.Add(this._databaseID);
			list4.Add((int)this._shipClass);
			list4.Add((int)this._shipRole);
			list4.Add((this._faction.FactionObj != null) ? this._faction.FactionObj.ObjectID : 0);
			list4.Add(this._suulkaType);
			list4.Add(this._isLoa);
			list4.Add(this._isAcceleratorHoop);
			list4.Add(this._isQShip);
			list4.Add(this._realShipClass == RealShipClasses.Platform);
			list4.Add(this._bIsNeutronStar);
			list4.Add(this._isGardener);
			list4.Add(flag2);
			list4.Add(this._isPolice);
			list4.Add(flag4);
			list4.Add(this._player.PlayerInfo.AutoUseGoopModules);
			list4.Add(this._player.PlayerInfo.AutoUseJokerModules);
			list4.Add(this._player.PlayerInfo.AutoAoe);
			list4.Add(createShipParams.sections.FirstOrDefault<ShipSectionAsset>().StationLevel);
			list4.Add(createShipParams.enableAI);
			list4.Add(this.CombatAI);
			list4.Add(game.AssetDatabase.ShipEMPEffect.Name);
			list4.Add(this._bonusSpottedRange);
			list4.Add(this._reserveSize);
			list4.Add((
				from x in createShipParams.sections
				select x.SlaveCapacity).Sum());
			list4.Add(createShipParams.obtainedSlaves);
			list4.Add(this._maxPsiPower);
			list4.Add(this._currentPsiPower);
			list4.Add((
				from x in createShipParams.sections
				select x.ShipExplosiveDamage).Sum());
			list4.Add((
				from x in createShipParams.sections
				select x.ShipExplosiveRange).Max());
			list4.Add(this.GetBaseSignature(game, createShipParams.sections.ToList<ShipSectionAsset>(), hasStealthTech));
			list4.Add(this.CalcShipCritModifier(attributes, 1f));
			list4.Add(this.CalcRepairCritModifier(attributes, list3, 1f));
			list4.Add(this.CalcCrewDeathFromStructureModifier(attributes, 0));
			list4.Add(this.CalcCrewDeathFromBoardingModifier(attributes, 0));
			list4.Add(Ship.GetBioMissileBonusModifier(game, this, createShipParams.sections.First((ShipSectionAsset x) => x.Type == ShipSectionType.Mission)));
			list4.Add(Ship.GetElectricEffectModifier(game.AssetDatabase, allShipTechsIds));
			list4.Add(Ship.HasAbsorberTech(createShipParams.sections.ToList<ShipSectionAsset>(), allShipTechsIds));
			list4.Add(Ship.GetPsiResistanceFromTech(game.AssetDatabase, allShipTechsIds));
			List<object> arg_DCF_0 = list4;
			bool arg_DCA_0;
			if (Player.HasNodeDriveTech(list2))
			{
				arg_DCA_0 = createShipParams.sections.Any((ShipSectionAsset x) => x.NodeSpeed > 0f);
			}
			else
			{
				arg_DCA_0 = false;
			}
			arg_DCF_0.Add(arg_DCA_0);
			float num3 = 0f;
			if (this._shipClass == ShipClass.Cruiser || this._shipClass == ShipClass.Dreadnought)
			{
				num3 = Player.GetSubversionRange(game.AssetDatabase, list2, this._isLoa);
			}
			list4.Add(num3);
			if (num3 > 0f)
			{
				if (this._isLoa)
				{
					list4.Add(game.AssetDatabase.GetTechBonus<float>("PSI_Subversion", "missilechanceL"));
					list4.Add(game.AssetDatabase.GetTechBonus<float>("PSI_Subversion", "dronechanceL"));
				}
				else
				{
					list4.Add(game.AssetDatabase.GetTechBonus<float>("PSI_Subversion", "missilechanceN"));
					list4.Add(game.AssetDatabase.GetTechBonus<float>("PSI_Subversion", "dronechanceN"));
				}
			}
			list4.Add(Player.HasWarpPulseTech(list2) && this._shipClass != ShipClass.BattleRider);
			list4.Add(this.GetPlanetDamageBonusFromAdmiralTraits(fleetInfo, list3));
			list4.Add(this.GetBaseROFBonusFromAdmiralTraits(game.GameDatabase, fleetInfo, list3, createShipParams.playersInCombat));
			list4.Add(this.GetInStandOffROFBonusFromAdmiralTraits(fleetInfo, list3));
			list4.Add(this._isSystemDefenceBoat);
			if (this._isSystemDefenceBoat)
			{
				this._defenseBoatActive = createShipParams.defenceBoatIsActive;
				list4.Add(createShipParams.defenceBoatIsActive);
				float num4 = 0f;
				Vector3 vector = Vector3.Zero;
				OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(createShipParams.defenceBoatOrbitalID);
				if (orbitalObjectInfo != null)
				{
					vector = game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position;
					PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
					if (planetInfo != null)
					{
						num4 = game.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(base.App.Game, planetInfo.ID).Radius + 375f;
					}
				}
				else
				{
					if (createShipParams.spawnMatrix.HasValue)
					{
						vector = createShipParams.spawnMatrix.Value.Position;
					}
				}
				list4.Add(num4);
				list4.Add(vector);
			}
			if (this.IsWraithAbductor || this.IsBattleRider)
			{
				ShipSectionAsset shipSectionAsset = createShipParams.sections.FirstOrDefault((ShipSectionAsset x) => x.Type == ShipSectionType.Mission);
				float num5 = (
					from x in createShipParams.sections
					select x.MissionTime).Sum();
				PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfos(this._player.ID).FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "NRG_SWS_Systems");
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				{
					num5 += num5 * game.AssetDatabase.GetTechBonus<float>(playerTechInfo.TechFileID, "ridertimebonus");
				}
				list4.Add((num5 > 0f) ? num5 : 30f);
				if (this.IsBattleRider)
				{
					Ship gameObject = game.GetGameObject<Ship>(createShipParams.parentID);
					BattleRiderSquad battleRiderSquad = (gameObject != null) ? gameObject.AssignRiderToSquad(this as BattleRiderShip, this._riderIndex) : null;
					list4.Add(this._riderIndex);
					list4.Add((battleRiderSquad != null) ? battleRiderSquad.ObjectID : 0);
					list4.Add((int)((shipSectionAsset != null) ? shipSectionAsset.BattleRiderType : BattleRiderTypes.Unspecified));
					list4.Add((shipSectionAsset.BattleRiderType == BattleRiderTypes.boardingpod) ? game.GameDatabase.GetStratModifier<float>(StratModifiers.BoardingPartyModifier, this._player.ID) : 1f);
				}
			}
			game.AddExistingObject(this, list4.ToArray());
			string text2 = this.Faction.Name;
			if (text2 == "liir_zuul")
			{
				if (this._realShipClass == RealShipClasses.BattleCruiser || this._realShipClass == RealShipClasses.BattleShip || this._realShipClass == RealShipClasses.SystemDefenseBoat)
				{
					text2 = "zuul";
				}
				else
				{
					text2 = "liir";
				}
			}
			ShipSpeedModifiers shipSpeedModifiers = Player.GetShipSpeedModifiers(game.AssetDatabase, this._player, this._realShipClass, list2, createShipParams.isInDeepSpace);
			float num6 = (
				from x in createShipParams.sections
				select x.Maneuvering.Deacceleration).Sum();
			float num7 = Math.Max(this.CalcTopSpeed(attributes, (
				from x in createShipParams.sections
				select x.Maneuvering.LinearSpeed).Sum()), 0f);
			App arg_1505_0 = base.App;
			object[] array = new object[17];
			array[0] = base.ObjectID;
			array[1] = Math.Max(this.CalcAccel(attributes, (
				from x in createShipParams.sections
				select x.Maneuvering.LinearAccel).Sum()), 0f) * shipSpeedModifiers.LinearAccelModifier;
			array[2] = Math.Max(this.CalcTurnThrust(attributes, (
				from x in createShipParams.sections
				select x.Maneuvering.RotAccel.X).Sum()), 0f) * shipSpeedModifiers.RotAccelModifier;
			array[3] = Math.Max(this.CalcTurnThrust(attributes, (
				from x in createShipParams.sections
				select x.Maneuvering.RotAccel.Y).Sum()), 0f) * shipSpeedModifiers.RotAccelModifier;
			array[4] = Math.Max(this.CalcTurnThrust(attributes, (
				from x in createShipParams.sections
				select x.Maneuvering.RotAccel.Z).Sum()), 0f) * shipSpeedModifiers.RotAccelModifier;
			array[5] = num7 * shipSpeedModifiers.SpeedModifier;
			array[6] = Math.Max(this.CalcTurnSpeed(attributes, (
				from x in createShipParams.sections
				select x.Maneuvering.RotationSpeed).Sum()), 0f) * shipSpeedModifiers.RotSpeedModifier;
			array[7] = ((num6 >= 1f) ? num6 : 2f);
			array[8] = 0f;
			array[9] = 0f;
			array[10] = 0f;
			array[11] = 0f;
			array[12] = this.GetInPursueSpeedBonusFromAdmiralTraits(fleetInfo, list3);
			array[13] = this.CanAcceptMoveOrders;
			array[14] = text;
			array[15] = text2;
			array[16] = createShipParams.inputID;
			ShipManeuvering shipManeuvering = arg_1505_0.AddObject<ShipManeuvering>(array);
			shipManeuvering.MaxShipSpeed = num7;
			this.AddObject(shipManeuvering);
			CompoundCollisionShape value = base.App.AddObject<CompoundCollisionShape>(new object[0]);
			this.AddObject(value);
			LogicalEffect turretEffect = new LogicalEffect
			{
				Name = "effects\\Weapons\\NuclearMissile_Impact.effect"
			};
			float kineticDampeningValue = Player.GetKineticDampeningValue(game.AssetDatabase, list2);
			bool flag5 = (
				from x in createShipParams.sections
				select x.StrategicSensorRange).Sum() == 0f;
			bool flag6 = (
				from x in createShipParams.sections
				select x.TacticalSensorRange).Sum() == 0f;
			int num8 = 0;
			foreach (ShipSectionAsset section in createShipParams.sections)
			{
				List<string> list5 = new List<string>();
				if (game.GameDatabase != null && createShipParams.assignedTechs[(int)section.Type] != null)
				{
					foreach (int current3 in createShipParams.assignedTechs[(int)section.Type].Techs)
					{
						list5.Add(game.GameDatabase.GetTechFileID(current3));
					}
				}
				int armorBonusFromTech = Ship.GetArmorBonusFromTech(game.AssetDatabase, list5);
				int num9 = Ship.GetPermArmorBonusFromTech(game.AssetDatabase, list5);
				if (game.GameDatabase != null)
				{
					num9 += ((this._realShipClass != RealShipClasses.AssaultShuttle && this._realShipClass != RealShipClasses.Biomissile && this._realShipClass != RealShipClasses.BoardingPod && this._realShipClass != RealShipClasses.Drone && this._realShipClass != RealShipClasses.EscapePod && this._realShipClass != RealShipClasses.BattleCruiser && this._realShipClass != RealShipClasses.BattleRider && this._realShipClass != RealShipClasses.BattleShip) ? game.GameDatabase.GetStratModifier<int>(StratModifiers.PhaseDislocationARBonus, createShipParams.player.ID) : 0);
				}
				CollisionShape collisionShape = base.App.AddObject<CollisionShape>(new object[]
				{
					PathHelpers.Combine(new string[]
					{
						Path.GetDirectoryName(section.ModelName),
						Path.GetFileNameWithoutExtension(section.ModelName) + "_convex.obj"
					})
				});
				collisionShape.SetTag(section);
				this.AddObject(collisionShape);
				CollisionShape collisionShape2 = string.IsNullOrEmpty(section.DamagedModelName) ? null : base.App.AddObject<CollisionShape>(new object[]
				{
					PathHelpers.Combine(new string[]
					{
						Path.GetDirectoryName(section.DamagedModelName),
						Path.GetFileNameWithoutExtension(section.DamagedModelName) + "_convex.obj"
					})
				});
				if (collisionShape2 != null)
				{
					collisionShape2.SetTag(collisionShape);
					this.AddObject(collisionShape2);
				}
				string text3 = (!string.IsNullOrEmpty(section.DestroyedModelName)) ? PathHelpers.Combine(new string[]
				{
					Path.GetDirectoryName(section.DestroyedModelName),
					Path.GetFileNameWithoutExtension(section.DestroyedModelName) + "_convex.obj"
				}) : string.Empty;
				CollisionShape collisionShape3 = base.App.AddObject<CollisionShape>(new object[]
				{
					text3,
					section.Type.ToString(),
					section.Type.ToString()
				});
				collisionShape3.SetTag(collisionShape);
				this.AddObject(collisionShape3);
				float num10 = this.CalcScannerRange(attributes, section.TacticalSensorRange);
				float num11 = section.StrategicSensorRange;
				if (section.Type == ShipSectionType.Mission)
				{
					if (flag6)
					{
						num10 = ((this._shipClass == ShipClass.BattleRider) ? game.AssetDatabase.DefaultBRTacSensorRange : game.AssetDatabase.DefaultTacSensorRange);
					}
					if (flag5)
					{
						num11 = ((this._shipClass == ShipClass.BattleRider) ? 0f : game.AssetDatabase.DefaultStratSensorRange);
					}
				}
				this._sensorRange = Math.Max(num10, this._sensorRange);
				int supplyWithTech = Ship.GetSupplyWithTech(game.AssetDatabase, list5, section.Supply);
				int powerWithTech = Ship.GetPowerWithTech(game.AssetDatabase, list5, list2, section.Power);
				int structureWithTech = Ship.GetStructureWithTech(game.AssetDatabase, list5, section.Structure);
				SectionInstanceInfo sectionInstanceInfo = null;
				if (designInfo != null)
				{
					DesignSectionInfo dsi = designInfo.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ShipSectionAsset == section);
					if (dsi != null)
					{
						sectionInstanceInfo = ((list.Count > 0) ? list.FirstOrDefault((SectionInstanceInfo x) => x.SectionID == dsi.ID) : null);
					}
				}
				List<object> list6 = new List<object>();
				list6.Add(Ship.FixAssetNameForDLC(section.ModelName, preferredMount));
				list6.Add(collisionShape.ObjectID);
				list6.Add(section.DamagedModelName ?? string.Empty);
				list6.Add((collisionShape2 != null) ? collisionShape2.ObjectID : 0);
				list6.Add(section.DestroyedModelName);
				list6.Add(collisionShape3.ObjectID);
				LogicalShipSpark[] array2 = base.App.AssetDatabase.ShipSparks.ToArray<LogicalShipSpark>();
				int num12 = array2.Length;
				list6.Add(num12);
				for (int j = 0; j < num12; j++)
				{
					list6.Add(array2[j].Type);
					list6.Add(array2[j].SparkEffect.Name);
				}
				list6.Add(section.Type.ToString());
				list6.Add(section.Type.ToString());
				list6.Add(section.SectionName ?? string.Empty);
				list6.Add(section.Type);
				list6.Add(section.Mass);
				list6.Add(section.AmbientSound);
				list6.Add(section.EngineSound);
				list6.Add(section.UnderAttackSound);
				list6.Add(section.DestroyedSound);
				list6.Add(structureWithTech);
				list6.Add(section.LowStruct);
				list6.Add(section.Crew);
				list6.Add(section.CrewRequired);
				list6.Add(powerWithTech);
				list6.Add(supplyWithTech);
				list6.Add(section.ProductionCost);
				list6.Add(section.ECM);
				list6.Add(section.ECCM);
				list6.Add(this.CalcSignature(attributes, section.Signature));
				list6.Add(num10);
				list6.Add(num11);
				list6.Add(section.isDeepScan);
				list6.Add(section.hasJammer);
				list6.Add(this.GetCloakingType(list5, section.cloakingType));
				list6.Add((sectionInstanceInfo != null) ? Math.Min(sectionInstanceInfo.Structure, structureWithTech) : structureWithTech);
				list6.Add((sectionInstanceInfo != null) ? sectionInstanceInfo.Crew : section.Crew);
				list6.Add((sectionInstanceInfo != null) ? sectionInstanceInfo.Supply : supplyWithTech);
				list6.Add(Ship.GetRichocetModifier(game.AssetDatabase, list5));
				list6.Add(Ship.GetBeamReflectModifier(game.AssetDatabase, list5));
				list6.Add(Ship.GetLaserReflectModifier(game.AssetDatabase, list5));
				list6.Add(kineticDampeningValue);
				list6.Add(base.ObjectID);
				list6.Add(section.DeathEffect.Name ?? string.Empty);
				list6.Add(section.ReactorFailureDeathEffect.Name ?? string.Empty);
				list6.Add(section.ReactorCriticalDeathEffect.Name ?? string.Empty);
				list6.Add(section.AbsorbedDeathEffect.Name ?? string.Empty);
				list6.Add(section.GetExtraArmorLayers() + num9);
				for (int k = 0; k < 4; k++)
				{
					int num13 = Ship.CalcArmorWidthModifier(attributes, 0) + armorBonusFromTech;
					int num14 = section.Armor[k].Y + num13;
					if (sectionInstanceInfo != null && sectionInstanceInfo.Armor.ContainsKey((ArmorSide)k) && sectionInstanceInfo.Armor[(ArmorSide)k].Height == num14)
					{
						list6.Add(sectionInstanceInfo.Armor[(ArmorSide)k]);
					}
					else
					{
						list6.Add(section.CreateFreshArmor((ArmorSide)k, num13));
					}
				}
				Section section3 = base.App.AddObject<Section>(list6.ToArray());
				section3.SetTag(section);
				section3.ShipSectionAsset = section;
				this.AddObject(section3);
				List<WeaponInstanceInfo> source = (sectionInstanceInfo != null) ? game.GameDatabase.GetWeaponInstances(sectionInstanceInfo.ID).ToList<WeaponInstanceInfo>() : new List<WeaponInstanceInfo>();
				List<WeaponInstanceInfo> weaponIns = (
					from x in source
					where !x.ModuleInstanceID.HasValue || x.ModuleInstanceID.Value == 0
					select x).ToList<WeaponInstanceInfo>();
				for (int l = 0; l < section.Banks.Length; l++)
				{
					LogicalBank bank = section.Banks[l];
					this.CreateBankDetails(createShipParams.turretHousings, createShipParams.weapons, createShipParams.preferredWeapons, createShipParams.assignedWeapons, weaponIns, list2, createShipParams.faction, shipInfo, fleetInfo, preferredMount, turretEffect, section, section3, null, bank, flag3);
				}
				this.CreateBattleRiderSquads(section3, null);
				List<ModuleInstanceInfo> list7 = (sectionInstanceInfo != null) ? game.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>() : new List<ModuleInstanceInfo>();
				for (int m = 0; m < section.Modules.Length; m++)
				{
					LogicalModuleMount moduleMount = section.Modules[m];
					if (createShipParams.assignedModules != null)
					{
						LogicalModule logicalModule = null;
						ModuleAssignment moduleAssignment = createShipParams.assignedModules.FirstOrDefault((ModuleAssignment x) => x.ModuleMount == moduleMount);
						if (moduleAssignment != null)
						{
							logicalModule = moduleAssignment.Module;
						}
						if (logicalModule == null && (!string.IsNullOrEmpty(moduleMount.AssignedModuleName) || section.Class == ShipClass.Station))
						{
							logicalModule = LogicalModule.EnumerateModuleFits(createShipParams.preferredModules, section, m, false).FirstOrDefault<LogicalModule>();
						}
						if (logicalModule != null)
						{
							CollisionShape collisionShape4 = base.App.AddObject<CollisionShape>(new object[]
							{
								PathHelpers.Combine(new string[]
								{
									Path.GetDirectoryName(logicalModule.ModelPath),
									Path.GetFileNameWithoutExtension(logicalModule.ModelPath) + "_convex.obj"
								}),
								"",
								moduleMount.NodeName
							});
							this.AddObject(collisionShape4);
							string text4 = (!string.IsNullOrEmpty(logicalModule.LowStructModelPath)) ? PathHelpers.Combine(new string[]
							{
								Path.GetDirectoryName(logicalModule.LowStructModelPath),
								Path.GetFileNameWithoutExtension(logicalModule.LowStructModelPath) + "_convex.obj"
							}) : string.Empty;
							CollisionShape collisionShape5 = null;
							if (!string.IsNullOrEmpty(text4))
							{
								collisionShape5 = base.App.AddObject<CollisionShape>(new object[]
								{
									text4
								});
								this.AddObject(collisionShape5);
							}
							string text5 = (!string.IsNullOrEmpty(logicalModule.DeadModelPath)) ? PathHelpers.Combine(new string[]
							{
								Path.GetDirectoryName(logicalModule.DeadModelPath),
								Path.GetFileNameWithoutExtension(logicalModule.DeadModelPath) + "_convex.obj"
							}) : string.Empty;
							CollisionShape collisionShape6 = null;
							if (!string.IsNullOrEmpty(text5))
							{
								collisionShape6 = base.App.AddObject<CollisionShape>(new object[]
								{
									text5
								});
								this.AddObject(collisionShape6);
							}
							Module module = this.CreateModule(moduleMount, logicalModule, list7, preferredMount, this, section3, collisionShape4, collisionShape5, collisionShape6, flag4, this._combatAI == SectionEnumerations.CombatAiType.Comet);
							module.Attachment = moduleMount;
							module.LogicalModule = logicalModule;
							module.AttachedSection = section3;
							module.SetTag(logicalModule);
							collisionShape4.SetTag(module);
							if (collisionShape5 != null)
							{
								collisionShape5.SetTag(collisionShape4);
							}
							if (collisionShape6 != null)
							{
								collisionShape6.SetTag(collisionShape4);
							}
							this.AddObject(module);
							int modInstId = 0;
							ModuleInstanceInfo moduleInstanceInfo = list7.FirstOrDefault((ModuleInstanceInfo x) => x.ModuleNodeID == moduleMount.NodeName);
							if (moduleInstanceInfo != null)
							{
								modInstId = moduleInstanceInfo.ID;
							}
							List<WeaponInstanceInfo> weaponIns2 = (
								from x in source
								where x.ModuleInstanceID == modInstId
								select x).ToList<WeaponInstanceInfo>();
							for (int n = 0; n < logicalModule.Banks.Length; n++)
							{
								LogicalBank bank2 = logicalModule.Banks[n];
								this.CreateBankDetails(createShipParams.turretHousings, createShipParams.weapons, createShipParams.preferredWeapons, createShipParams.assignedWeapons, weaponIns2, list2, createShipParams.faction, shipInfo, fleetInfo, preferredMount, turretEffect, section, section3, module, bank2, flag3);
							}
							this.CreateBattleRiderSquads(section3, module);
						}
					}
				}
				num8++;
			}
			List<SectionEnumerations.PsionicAbility> list8 = new List<SectionEnumerations.PsionicAbility>();
			foreach (SectionEnumerations.PsionicAbility[] current4 in 
				from x in createShipParams.sections
				select x.PsionicAbilities)
			{
				SectionEnumerations.PsionicAbility[] array3 = current4;
				SectionEnumerations.PsionicAbility psionic;
				for (int i = 0; i < array3.Length; i++)
				{
					psionic = array3[i];
					if (!list8.Any((SectionEnumerations.PsionicAbility x) => x == psionic))
					{
						list8.Add(psionic);
					}
				}
			}
			if (createShipParams.assignedModules != null)
			{
				foreach (ModuleAssignment current5 in createShipParams.assignedModules)
				{
					if (current5.PsionicAbilities != null)
					{
						SectionEnumerations.PsionicAbility[] array3 = current5.PsionicAbilities;
						SectionEnumerations.PsionicAbility psionic;
						for (int i = 0; i < array3.Length; i++)
						{
							psionic = array3[i];
							if (!list8.Any((SectionEnumerations.PsionicAbility x) => x == psionic))
							{
								list8.Add(psionic);
							}
						}
					}
				}
			}
			if (this.Modules != null)
			{
				if (this.Modules.Any((Module x) => x.LogicalModule.AbilityType == ModuleEnums.ModuleAbilities.AbaddonLaser))
				{
					list8.Add(SectionEnumerations.PsionicAbility.AbaddonLaser);
				}
			}
			if (createShipParams.addPsionics)
			{
				this.CreatePsionics(createShipParams.psionics, this, list8);
			}
			Section section2 = null;
			LogicalShield logicalShield = null;
			if (this._combatAI == SectionEnumerations.CombatAiType.VonNeumannDisc)
			{
				logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => x.TechID == "SLD_Shield_Mk._IV");
				section2 = this.MissionSection;
			}
			else
			{
				logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => allShipTechsIds.Contains(x.TechID));
				if (logicalShield == null)
				{
					foreach (ShipSectionAsset ssa in createShipParams.sections)
					{
						logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => ssa.ShipOptions.Any((string[] y) => y.Contains(x.TechID)));
						if (logicalShield != null)
						{
							break;
						}
					}
				}
			}
			if (logicalShield == null)
			{
				section2 = this.Sections.FirstOrDefault((Section x) => x.ShipSectionAsset.Type == ShipSectionType.Command);
				if (section2 != null)
				{
					if (section2.ShipSectionAsset.Title.Contains("DISRUPTOR"))
					{
						logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => x.TechID == "SLD_Disruptor_Shields");
					}
					else
					{
						if (section2.ShipSectionAsset.Title.Contains("DEFLECTOR"))
						{
							logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => x.TechID == "SLD_Deflector_Shields");
						}
					}
				}
			}
			if (logicalShield != null)
			{
				if (section2 == null)
				{
					foreach (Section current6 in this.Sections)
					{
						foreach (string[] current7 in current6.ShipSectionAsset.ShipOptions)
						{
							if (current7.Contains(logicalShield.TechID))
							{
								section2 = current6;
								break;
							}
						}
						if (section2 != null)
						{
							break;
						}
					}
				}
				if (section2 != null)
				{
					Shield value2 = new Shield(game, this, logicalShield, section2, list2, true);
					this.AddObject(value2);
				}
			}
			App arg_2889_0 = base.App;
			array = new object[2];
			array[0] = ((!flag4) ? 1000f : 1f) * (
				from x in createShipParams.sections
				select x.Mass).Sum();
			array[1] = createShipParams.isKillable;
			RigidBody value3 = arg_2889_0.AddObject<RigidBody>(array);
			this.AddObject(value3);
			if (createShipParams.spawnMatrix.HasValue)
			{
				this.Position = createShipParams.spawnMatrix.Value.Position;
				this.Rotation = createShipParams.spawnMatrix.Value.EulerAngles;
				this.Maneuvering.Destination = this.Position;
			}
			if (!string.IsNullOrEmpty(this._priorityWeapon))
			{
				WeaponBank weaponBank = this.WeaponBanks.FirstOrDefault((WeaponBank x) => x.Weapon.WeaponName == this._priorityWeapon);
				if (weaponBank != null)
				{
					this.PostSetProp("SetPriorityWeapon", weaponBank.Weapon.GameObject.ObjectID);
				}
			}
			if (this.IsWraithAbductor)
			{
				WeaponBank weaponBank2 = this.WeaponBanks.FirstOrDefault((WeaponBank x) => x.TurretClass == WeaponEnums.TurretClasses.AssaultShuttle);
				if (weaponBank2 != null && weaponBank2.Weapon != null)
				{
					this.PostSetProp("SetShipWeapon", weaponBank2.Weapon.GameObject.ObjectID);
				}
			}
			else
			{
				if (this._combatAI == SectionEnumerations.CombatAiType.LocustFighter)
				{
					LogicalWeapon logicalWeapon = base.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == "AssaultLocustFighter");
					if (logicalWeapon != null)
					{
						logicalWeapon.AddGameObjectReference();
						this.PostSetProp("SetShipWeapon", logicalWeapon.GameObject.ObjectID);
					}
				}
			}
			if (this._combatAI == SectionEnumerations.CombatAiType.VonNeumannDisc)
			{
				VonNeumannDiscTypes vonNeumannDiscTypes = VonNeumannDiscControl.DiscTypeFromMissionSection(this.MissionSection);
				if (vonNeumannDiscTypes == VonNeumannDiscTypes.EMPULSER)
				{
					List<object> list9 = new List<object>();
					list9.Add(base.ObjectID);
					list9.Add("effects\\Weapons\\EMP_Wave.effect");
					list9.Add(750f);
					list9.Add(true);
					EMPulsar value4 = base.App.AddObject<EMPulsar>(list9.ToArray());
					this.AddObject(value4);
					return;
				}
				if (vonNeumannDiscTypes == VonNeumannDiscTypes.SCREAMER)
				{
					List<object> list10 = new List<object>();
					list10.Add(base.ObjectID);
					list10.Add("");
					list10.Add(750f);
					WildWeasel value5 = base.App.AddObject<WildWeasel>(list10.ToArray());
					this.AddObject(value5);
				}
			}
		}
		private void CreateBankDetails(IEnumerable<LogicalTurretHousing> turretHousings, IEnumerable<LogicalWeapon> weapons, IEnumerable<LogicalWeapon> preferredWeapons, IEnumerable<WeaponAssignment> assignedWeapons, IEnumerable<WeaponInstanceInfo> weaponIns, List<PlayerTechInfo> playerTechs, Faction faction, ShipInfo ship, FleetInfo fleet, string preferredMount, LogicalEffect turretEffect, ShipSectionAsset section, Section sectionObj, Module module, LogicalBank bank, bool isTestMode)
		{
			bool flag = WeaponEnums.IsBattleRider(bank.TurretClass);
			if (flag)
			{
				if (module == null)
				{
					int arg_38_0 = base.ObjectID;
				}
				else
				{
					int arg_42_0 = module.ObjectID;
				}
				IEnumerable<LogicalMount> arg_90_0;
				if (module == null)
				{
					arg_90_0 = 
						from x in section.Mounts
						where x.Bank == bank
						select x;
				}
				else
				{
					arg_90_0 = 
						from x in module.LogicalModule.Mounts
						where x.Bank == bank
						select x;
				}
				IEnumerable<LogicalMount> enumerable = arg_90_0;
				int num = 0;
				int designID = 0;
				int num2 = 0;
				int num3 = 0;
				string moduleNodeName = (module != null) ? module.Attachment.NodeName : "";
				WeaponBank weaponBank = null;
				LogicalWeapon logicalWeapon = Ship.SelectWeapon(section, bank, assignedWeapons, preferredWeapons, weapons, moduleNodeName, out designID, out num2, out num3);
				bool flag2 = WeaponEnums.IsWeaponBattleRider(bank.TurretClass);
				if (logicalWeapon != null)
				{
					logicalWeapon.AddGameObjectReference();
					num = logicalWeapon.GameObject.ObjectID;
					if (flag2)
					{
						weaponBank = new WeaponBank(base.App, this, bank, module, logicalWeapon, Player.GetWeaponLevelFromTechs(logicalWeapon, playerTechs.ToList<PlayerTechInfo>()), designID, 0, 0, bank.TurretSize, bank.TurretClass);
						weaponBank.AddExistingObject(base.App);
						this.AddObject(weaponBank);
					}
				}
				MountObject.WeaponModels weaponModels = new MountObject.WeaponModels();
				weaponModels.FillOutModelFilesWithWeapon(logicalWeapon, faction, preferredMount, weapons);
				using (IEnumerator<LogicalMount> enumerator = enumerable.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						LogicalMount current = enumerator.Current;
						BattleRiderMount battleRiderMount = base.App.AddObject<BattleRiderMount>(new object[]
						{
							num,
							base.ObjectID,
							(sectionObj != null) ? sectionObj.ObjectID : 0,
							(module != null) ? module.ObjectID : 0,
							(weaponBank != null) ? weaponBank.ObjectID : 0,
							weaponModels.WeaponModelPath.ModelPath,
							weaponModels.WeaponModelPath.DefaultModelPath,
							weaponModels.SubWeaponModelPath.ModelPath,
							weaponModels.SubWeaponModelPath.DefaultModelPath,
							weaponModels.SecondaryWeaponModelPath.ModelPath,
							weaponModels.SecondaryWeaponModelPath.DefaultModelPath,
							weaponModels.SecondarySubWeaponModelPath.ModelPath,
							weaponModels.SecondarySubWeaponModelPath.DefaultModelPath,
							current.NodeName,
							bank.TurretClass
						});
						if (flag2)
						{
							battleRiderMount.IsWeapon = true;
							battleRiderMount.DesignID = designID;
						}
						this.AddObject(battleRiderMount);
						battleRiderMount.ParentID = ((module != null) ? module.ObjectID : sectionObj.ObjectID);
						battleRiderMount.SquadIndex = this.BattleRiderMounts.Count<BattleRiderMount>() - 1;
						battleRiderMount.NodeName = current.NodeName;
						battleRiderMount.WeaponBank = bank;
						if (weaponBank != null)
						{
							battleRiderMount.BankIcon = weaponBank.Weapon.IconSpriteName;
						}
						else
						{
							battleRiderMount.BankIcon = "";
						}
						battleRiderMount.AssignedSection = sectionObj;
						battleRiderMount.AssignedModule = module;
					}
					return;
				}
			}
			if (this.Faction.Name == "slavers")
			{
				Faction faction2 = base.App.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.Name == "zuul");
				this.CreateTurretsForBank(preferredMount, faction2, ship, fleet, section, sectionObj, assignedWeapons, preferredWeapons, weapons, turretHousings, weaponIns, playerTechs, turretEffect, module, bank, isTestMode);
				return;
			}
			this.CreateTurretsForBank(preferredMount, faction, ship, fleet, section, sectionObj, assignedWeapons, preferredWeapons, weapons, turretHousings, weaponIns, playerTechs, turretEffect, module, bank, isTestMode);
		}
		private void CreateBattleRiderSquads(Section section, Module module)
		{
			int parentID = (module != null) ? module.ObjectID : section.ObjectID;
			List<BattleRiderMount> list = (
				from x in this.BattleRiderMounts
				where x.ParentID == parentID
				select x).ToList<BattleRiderMount>();
			if (list.Count == 0)
			{
				return;
			}
			list.Sort((BattleRiderMount x, BattleRiderMount y) => x.SquadIndex.CompareTo(y.SquadIndex));
			List<object> list2 = new List<object>();
			list2.Add(base.ObjectID);
			list2.Add(section.ObjectID);
			list2.Add((module != null) ? module.ObjectID : 0);
			if (module != null)
			{
				list2.Add(0f);
				list2.Add(0f);
			}
			else
			{
				list2.Add(section.ShipSectionAsset.LaunchDelay);
				list2.Add(section.ShipSectionAsset.DockingDelay);
			}
			list2.Add(3f);
			list2.Add(this._realShipClass == RealShipClasses.Platform);
			list2.Add(list.First<BattleRiderMount>().BankIcon);
			int num = this.BattleRiderSquads.Count<BattleRiderSquad>();
			if (module != null || this.OnlyOneSquad())
			{
				list2.Add(num);
				list2.Add(list.Count);
				foreach (BattleRiderMount current in list)
				{
					list2.Add(current.ObjectID);
				}
				BattleRiderSquad battleRiderSquad = base.App.AddObject<BattleRiderSquad>(list2.ToArray());
				this.AddObject(battleRiderSquad);
				battleRiderSquad.ParentID = parentID;
				battleRiderSquad.AttachedSection = section;
				battleRiderSquad.AttachedModule = module;
				battleRiderSquad.NumRiders = list.Count;
				battleRiderSquad.Mounts.AddRange(list);
				return;
			}
			List<LogicalBank> list3 = new List<LogicalBank>();
			foreach (BattleRiderMount current2 in list)
			{
				if (!list3.Contains(current2.WeaponBank))
				{
					list3.Add(current2.WeaponBank);
				}
			}
			foreach (LogicalBank bank in list3)
			{
				List<BattleRiderMount> list4 = (
					from x in list
					where x.WeaponBank == bank
					select x).ToList<BattleRiderMount>();
				int numRidersPerSquad = BattleRiderSquad.GetNumRidersPerSquad(bank.TurretClass, this._shipClass, list4.Count);
				List<BattleRiderMount> list5 = new List<BattleRiderMount>();
				foreach (BattleRiderMount current3 in list4)
				{
					list5.Add(current3);
					if (list5.Count >= numRidersPerSquad)
					{
						List<object> list6 = new List<object>();
						list6.AddRange(list2);
						list6.Add(num);
						list6.Add(numRidersPerSquad);
						foreach (BattleRiderMount current4 in list5)
						{
							list6.Add(current4.ObjectID);
						}
						BattleRiderSquad battleRiderSquad2 = base.App.AddObject<BattleRiderSquad>(list6.ToArray());
						this.AddObject(battleRiderSquad2);
						battleRiderSquad2.ParentID = parentID;
						battleRiderSquad2.AttachedSection = section;
						battleRiderSquad2.AttachedModule = module;
						battleRiderSquad2.NumRiders = numRidersPerSquad;
						battleRiderSquad2.Mounts.AddRange(list5);
						num++;
						list5.Clear();
					}
				}
			}
		}
		public void ExternallyAssignShieldToShip(App game, LogicalShield logShield)
		{
			if (this.Shield != null)
			{
				return;
			}
			List<PlayerTechInfo> playerTechs = (this._player != null && game.GameDatabase != null) ? game.GameDatabase.GetPlayerTechInfos(this._player.ID).ToList<PlayerTechInfo>() : new List<PlayerTechInfo>();
			Shield value = new Shield(base.App, this, logShield, this.MissionSection, playerTechs, false);
			this.AddObject(value);
			IGameObject[] objects = new IGameObject[]
			{
				this.Shield
			};
			this.PostObjectAddObjects(objects);
		}
		protected Ship(App game, CreateShipParams createShipParams)
		{
			this.Prepare(game, createShipParams);
		}
		private void AddObject(IGameObject value)
		{
			this._objects.Add(value);
		}
		private void RemoveObject(IGameObject value)
		{
			this._objects.Remove(value);
		}
		private void RemoveObjects(List<IGameObject> objs)
		{
			foreach (IGameObject current in objs)
			{
				base.App.CurrentState.RemoveGameObject(current);
				this.RemoveObject(current);
			}
		}
		public static float GetTurretHealth(WeaponEnums.WeaponSizes turretSize)
		{
			switch (turretSize)
			{
			case WeaponEnums.WeaponSizes.VeryLight:
				return 10f;
			case WeaponEnums.WeaponSizes.Light:
				return 20f;
			case WeaponEnums.WeaponSizes.Medium:
				return 30f;
			case WeaponEnums.WeaponSizes.Heavy:
				return 40f;
			case WeaponEnums.WeaponSizes.VeryHeavy:
				return 60f;
			case WeaponEnums.WeaponSizes.SuperHeavy:
				return 100f;
			default:
				return 10f;
			}
		}
		private static float GetTurretShapeRadius(WeaponEnums.WeaponSizes turretSize)
		{
			switch (turretSize)
			{
			case WeaponEnums.WeaponSizes.VeryLight:
				return 1f;
			case WeaponEnums.WeaponSizes.Light:
				return 2.5f;
			case WeaponEnums.WeaponSizes.Medium:
				return 5f;
			case WeaponEnums.WeaponSizes.Heavy:
				return 10f;
			case WeaponEnums.WeaponSizes.VeryHeavy:
				return 20f;
			case WeaponEnums.WeaponSizes.SuperHeavy:
				return 30f;
			default:
				return 1f;
			}
		}
		private static bool GetTurretNeedsCollision(WeaponEnums.TurretClasses turretClass, string turretModelName)
		{
			if (turretModelName.ToLower() == "turret_dummy")
			{
				return false;
			}
			switch (turretClass)
			{
			case WeaponEnums.TurretClasses.Standard:
			case WeaponEnums.TurretClasses.Missile:
				return true;
			default:
				return false;
			}
		}
		public static LogicalWeapon SelectWeapon(ShipSectionAsset section, LogicalBank bank, IEnumerable<WeaponAssignment> assignedWeapons, IEnumerable<LogicalWeapon> preferredWeapons, IEnumerable<LogicalWeapon> weapons, string moduleNodeName, out int designID, out int targetFilter, out int fireMode)
		{
			LogicalWeapon logicalWeapon = null;
			designID = 0;
			targetFilter = 0;
			fireMode = 0;
			if (assignedWeapons != null)
			{
				WeaponAssignment weaponAssignment = assignedWeapons.FirstOrDefault((WeaponAssignment x) => x.Bank == bank && (x.ModuleNode == null || x.ModuleNode == moduleNodeName));
				if (weaponAssignment != null)
				{
					logicalWeapon = weaponAssignment.Weapon;
					designID = weaponAssignment.DesignID;
					targetFilter = (weaponAssignment.InitialTargetFilter ?? 0);
					fireMode = (weaponAssignment.InitialFireMode ?? 0);
				}
			}
			if (logicalWeapon == null && !string.IsNullOrEmpty(bank.DefaultWeaponName))
			{
				logicalWeapon = weapons.FirstOrDefault((LogicalWeapon x) => string.Equals(x.WeaponName, bank.DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase));
			}
			if (logicalWeapon == null && preferredWeapons != null)
			{
				logicalWeapon = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, preferredWeapons, bank.TurretSize, bank.TurretClass).FirstOrDefault<LogicalWeapon>();
			}
			if (logicalWeapon == null)
			{
				logicalWeapon = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, weapons, bank.TurretSize, bank.TurretClass).FirstOrDefault<LogicalWeapon>();
			}
			if (logicalWeapon == null)
			{
				logicalWeapon = weapons.First((LogicalWeapon x) => x.PayloadType != WeaponEnums.PayloadTypes.BattleRider);
				App.Log.Warn(string.Format("No weapon found to match {0}, {1}! on [{2}] Picking an inappropriate default to keep the game from crashing.", bank.TurretSize, bank.TurretClass, section.FileName), "design");
			}
			logicalWeapon.AddGameObjectReference();
			return logicalWeapon;
		}
		private float CalcTurnSpeed(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Fast_In_The_Curves))
			{
				value *= 1.1f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Nimble_Lil_Minx))
			{
				value *= 1.1f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Bit_Of_A_Hog))
			{
				value *= 0.9f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Muscle_Machine))
			{
				value *= 0.8f;
			}
			return value;
		}
		private float CalcTurnThrust(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Nimble_Lil_Minx))
			{
				value *= 1.2f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Bit_Of_A_Hog))
			{
				value *= 0.9f;
			}
			return value;
		}
		private float CalcTopSpeed(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Muscle_Machine))
			{
				value *= 1.1f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ghost_Of_The_Hood))
			{
				value *= 1.1f;
			}
			return value;
		}
		private float CalcAccel(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Muscle_Machine))
			{
				value *= 1.2f;
			}
			return value;
		}
		private float CalcRateOfFire(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Aces_And_Eights))
			{
				value *= 0.85f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ol_Yellow_Streak))
			{
				value *= 1.1f;
			}
			return value;
		}
		private float CalcSignature(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Aces_And_Eights))
			{
				value *= 1.2f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ol_Yellow_Streak))
			{
				value *= 0.8f;
			}
			return value;
		}
		private float CalcScannerRange(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Louis_And_Clark))
			{
				value *= 1.1f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Four_Eyes))
			{
				value *= 0.9f;
			}
			return value;
		}
		private float CalcBallisticWeaponRange(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Sniper))
			{
				value *= 1.25f;
			}
			return value;
		}
		private float GetBaseSignature(App game, List<ShipSectionAsset> sections, bool hasStealthTech)
		{
			if (sections == null || sections.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			if (sections.Any((ShipSectionAsset x) => x.isDeepScan))
			{
				num += 30f;
			}
			if (hasStealthTech)
			{
				num -= game.AssetDatabase.TacStealthArmorBonus;
			}
			switch (this.ShipClass)
			{
			case ShipClass.Dreadnought:
				num += 50f;
				break;
			case ShipClass.Leviathan:
			case ShipClass.Station:
				num += 100f;
				break;
			case ShipClass.BattleRider:
				if (this.RealShipClass == RealShipClasses.Drone)
				{
					num -= 50f;
				}
				else
				{
					num -= 25f;
				}
				break;
			}
			return num;
		}
		private float CalcShipCritModifier(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Hard_Luck_Ship))
			{
				value *= 2f;
			}
			return value;
		}
		private float CalcRepairCritModifier(List<SectionEnumerations.DesignAttribute> attributes, List<AdmiralInfo.TraitType> admiralTraits, float value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			float num = 1f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Spirit_Of_The_Yorktown))
			{
				num += 0.5f;
			}
			if (admiralTraits.Contains(AdmiralInfo.TraitType.Elite))
			{
				num += 1f;
			}
			return value * num;
		}
		private int CalcCrewDeathFromStructureModifier(List<SectionEnumerations.DesignAttribute> attributes, int value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Death_Trap))
			{
				value++;
			}
			return value;
		}
		private int CalcCrewDeathFromBoardingModifier(List<SectionEnumerations.DesignAttribute> attributes, int value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Death_Trap))
			{
				value++;
			}
			return value;
		}
		public static int CalcArmorWidthModifier(List<SectionEnumerations.DesignAttribute> attributes, int value)
		{
			if (attributes.Count == 0)
			{
				return value;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ol_Ironsides))
			{
				value += 2;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ghost_Of_The_Hood))
			{
				value -= 2;
			}
			return value;
		}
		public static bool HasAbsorberTech(List<ShipSectionAsset> sections, List<string> techs)
		{
			return sections.Any((ShipSectionAsset x) => x.IsAbsorberSection) || techs.Contains("NRG_Energy_Absorbers");
		}
		public static int GetPsiResistanceFromTech(AssetDatabase ab, List<string> techs)
		{
			if (techs.Contains("CYB_PsiShield"))
			{
				return ab.GetTechBonus<int>("CYB_PsiShield", "psipower");
			}
			return 0;
		}
		public static int GetArmorBonusFromTech(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
			{
				return 0;
			}
			if (techs.Contains("IND_Polysteel"))
			{
				return ab.GetTechBonus<int>("IND_Polysteel", "armorlayers");
			}
			if (techs.Contains("IND_MagnoCeramic_Latices"))
			{
				return ab.GetTechBonus<int>("IND_MagnoCeramic_Latices", "armorlayers");
			}
			if (techs.Contains("IND_Quark_Resonators"))
			{
				return ab.GetTechBonus<int>("IND_Quark_Resonators", "armorlayers");
			}
			if (techs.Contains("IND_Adamantine_Alloys"))
			{
				return ab.GetTechBonus<int>("IND_Adamantine_Alloys", "armorlayers");
			}
			return 0;
		}
		public static int GetPermArmorBonusFromTech(AssetDatabase ab, List<string> techs)
		{
			if (techs.Contains("IND_Adamantine_Alloys"))
			{
				return ab.GetTechBonus<int>("IND_Adamantine_Alloys", "permarmorlayers");
			}
			return 0;
		}
		public static float GetRichocetModifier(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
			{
				return 0f;
			}
			if (techs.Contains("IND_Polysteel"))
			{
				return ab.GetTechBonus<float>("IND_Polysteel", "ricochet");
			}
			if (techs.Contains("IND_MagnoCeramic_Latices"))
			{
				return ab.GetTechBonus<float>("IND_MagnoCeramic_Latices", "ricochet");
			}
			if (techs.Contains("IND_Quark_Resonators"))
			{
				return ab.GetTechBonus<float>("IND_Quark_Resonators", "ricochet");
			}
			if (techs.Contains("IND_Adamantine_Alloys"))
			{
				return ab.GetTechBonus<float>("IND_Adamantine_Alloys", "ricochet");
			}
			return 0f;
		}
		public static float GetBeamReflectModifier(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
			{
				return 0f;
			}
			if (techs.Contains("IND_Reflective"))
			{
				return ab.GetTechBonus<float>("IND_Reflective", "beamdamage");
			}
			if (techs.Contains("IND_Improved_Reflective"))
			{
				return ab.GetTechBonus<float>("IND_Improved_Reflective", "beamdamage");
			}
			return 0f;
		}
		public static float GetLaserReflectModifier(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
			{
				return 0f;
			}
			if (techs.Contains("IND_Reflective"))
			{
				return ab.GetTechBonus<float>("IND_Reflective", "laserricochet");
			}
			if (techs.Contains("IND_Improved_Reflective"))
			{
				return ab.GetTechBonus<float>("IND_Improved_Reflective", "laserricochet");
			}
			return 0f;
		}
		public static float GetElectricEffectModifier(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
			{
				return 1f;
			}
			float num = 1f;
			if (techs.Contains("IND_Electronic_Hardening"))
			{
				num += ab.GetTechBonus<float>("IND_Electronic_Hardening", "damage");
			}
			return num;
		}
		public static int GetPowerWithTech(AssetDatabase ab, List<string> techs, List<PlayerTechInfo> playerTechs, int currentPower)
		{
			if (techs.Count == 0)
			{
				return currentPower;
			}
			float num = 1f;
			if (techs.Contains("NRG_Plasma_Induction"))
			{
				num += ab.GetTechBonus<float>("NRG_Plasma_Induction", "power");
			}
			if (techs.Contains("NRG_Wave_Amplification"))
			{
				num += ab.GetTechBonus<float>("NRG_Wave_Amplification", "power");
			}
			num += Player.GetPowerBonus(ab, playerTechs);
			return (int)((float)currentPower * num);
		}
		public static int GetSupplyWithTech(AssetDatabase ab, List<string> techs, int currentSupply)
		{
			if (techs.Count == 0)
			{
				return currentSupply;
			}
			float num = 1f;
			if (techs.Contains("NRG_Plasma_Focusing"))
			{
				num += ab.GetTechBonus<float>("NRG_Plasma_Focusing", "supply");
			}
			return (int)((float)currentSupply * num);
		}
		public static int GetStructureWithTech(AssetDatabase ab, List<string> techs, int currentStruct)
		{
			if (techs.Count == 0)
			{
				return currentStruct;
			}
			float num = 1f;
			if (techs.Contains("SLD_Structural_Fields"))
			{
				num += ab.GetTechBonus<float>("SLD_Structural_Fields", "structure");
			}
			return (int)((float)currentStruct * num);
		}
		private CloakingType GetCloakingType(List<string> techs, CloakingType sectionCloakType)
		{
			if (sectionCloakType == CloakingType.None)
			{
				return CloakingType.None;
			}
			if (sectionCloakType == CloakingType.Cloaking)
			{
				if (techs.Contains("SLD_Improved_Cloaking"))
				{
					return CloakingType.ImprovedCloaking;
				}
				if (techs.Contains("SLD_Intangibility"))
				{
					return CloakingType.Intangible;
				}
			}
			return sectionCloakType;
		}
		private float GetPlanetDamageBonusFromAdmiralTraits(FleetInfo fleet, List<AdmiralInfo.TraitType> traits)
		{
			float num = 0f;
			if (fleet != null && traits.Contains(AdmiralInfo.TraitType.Sherman))
			{
				num += 2f;
			}
			return num;
		}
		private float GetBaseROFBonusFromAdmiralTraits(GameDatabase db, FleetInfo fleet, List<AdmiralInfo.TraitType> traits, List<Player> playersInCombat)
		{
			float num = 0f;
			if (fleet != null)
			{
				if (traits.Any((AdmiralInfo.TraitType x) => x == AdmiralInfo.TraitType.Defender || x == AdmiralInfo.TraitType.Attacker || x == AdmiralInfo.TraitType.GloryHound || x == AdmiralInfo.TraitType.Technophobe))
				{
					int? systemOwningPlayer = db.GetSystemOwningPlayer(fleet.SystemID);
					DiplomacyState diplomacyState = systemOwningPlayer.HasValue ? db.GetDiplomacyStateBetweenPlayers(systemOwningPlayer.Value, fleet.PlayerID) : DiplomacyState.UNKNOWN;
					if (traits.Contains(AdmiralInfo.TraitType.Attacker))
					{
						num += ((diplomacyState == DiplomacyState.WAR || !systemOwningPlayer.HasValue) ? 0.1f : -0.1f);
					}
					if (traits.Contains(AdmiralInfo.TraitType.Defender))
					{
						num += ((systemOwningPlayer.HasValue && (systemOwningPlayer.Value == fleet.PlayerID || diplomacyState == DiplomacyState.ALLIED)) ? 0.1f : -0.1f);
					}
					if (traits.Contains(AdmiralInfo.TraitType.GloryHound))
					{
						num += ((systemOwningPlayer.HasValue && (systemOwningPlayer.Value == fleet.PlayerID || diplomacyState == DiplomacyState.ALLIED)) ? 0.2f : 0f);
					}
					if (traits.Contains(AdmiralInfo.TraitType.Technophobe))
					{
						if (playersInCombat.Any((Player x) => x.ID != fleet.ID && x.Faction.Name == "loa" && db.GetDiplomacyStateBetweenPlayers(x.ID, fleet.PlayerID) == DiplomacyState.WAR))
						{
							num += 0.15f;
						}
					}
				}
			}
			return num;
		}
		private float GetInStandOffROFBonusFromAdmiralTraits(FleetInfo fleet, List<AdmiralInfo.TraitType> traits)
		{
			float num = 0f;
			if (fleet != null && traits.Contains(AdmiralInfo.TraitType.ArtilleryExpert))
			{
				num += 0.1f;
			}
			return num;
		}
		private float GetInPursueSpeedBonusFromAdmiralTraits(FleetInfo fleet, List<AdmiralInfo.TraitType> traits)
		{
			float num = 0f;
			if (fleet != null && traits.Contains(AdmiralInfo.TraitType.Hunter))
			{
				num += 0.15f;
			}
			return num;
		}
		private static float GetBioMissileBonusModifier(App game, Ship ship, ShipSectionAsset missionSection)
		{
			float result = 1f;
			if (ship.IsSuulka)
			{
				SuulkaPsiBonus suulkaPsiBonus = game.AssetDatabase.SuulkaPsiBonuses.First((SuulkaPsiBonus x) => x.Name == missionSection.SectionName);
				result = suulkaPsiBonus.BioMissileMultiplyer;
			}
			return result;
		}
		public static int GetNumShipsThatShouldTarget(Ship ship)
		{
			switch (ship.ShipClass)
			{
			case ShipClass.Cruiser:
				return 2;
			case ShipClass.Dreadnought:
				return 3;
			case ShipClass.Leviathan:
				return 5;
			case ShipClass.BattleRider:
				return 1;
			case ShipClass.Station:
				return 6;
			default:
				return 2;
			}
		}
		private IList<Turret> CreateTurretsForBank(string preferredMount, Faction faction, ShipInfo ship, FleetInfo fleet, ShipSectionAsset section, Section sectionObj, IEnumerable<WeaponAssignment> assignedWeapons, IEnumerable<LogicalWeapon> preferredWeapons, IEnumerable<LogicalWeapon> weapons, IEnumerable<LogicalTurretHousing> turretHousings, IEnumerable<WeaponInstanceInfo> weaponIns, List<PlayerTechInfo> playerTechs, LogicalEffect turretEffect, Module module, LogicalBank bank, bool isTestMode)
		{
			List<Turret> list = new List<Turret>();
			try
			{
				int designID = 0;
				int targetFilter = 0;
				int fireMode = 0;
				string moduleNodeName = (module != null) ? module.Attachment.NodeName : "";
				LogicalWeapon weapon = Ship.SelectWeapon(section, bank, assignedWeapons, preferredWeapons, weapons, moduleNodeName, out designID, out targetFilter, out fireMode);
				LogicalTurretClass weaponTurretClass = weapon.GetLogicalTurretClassForMount(bank.TurretSize, bank.TurretClass);
				LogicalTurretHousing housing = turretHousings.First((LogicalTurretHousing housingCandidate) => weaponTurretClass.TurretClass == housingCandidate.Class && weapon.DefaultWeaponSize == housingCandidate.WeaponSize && bank.TurretSize == housingCandidate.MountSize);
				float turretHealth = Ship.GetTurretHealth(bank.TurretSize);
				float turretShapeRadius = Ship.GetTurretShapeRadius(bank.TurretSize);
				bool flag = faction.Name == "swarm" && this.RealShipClass == RealShipClasses.Dreadnought;
				if (!isTestMode && weapon != null && weapon.PayloadType == WeaponEnums.PayloadTypes.MegaBeam)
				{
					PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "DRV_Neutrino_Blast_Wave");
					if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
					{
						return list;
					}
				}
				MountObject.WeaponModels weaponModels = new MountObject.WeaponModels();
				weaponModels.FillOutModelFilesWithWeapon(weapon, faction, weapons);
				int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(weapon, playerTechs.ToList<PlayerTechInfo>());
				if (this._player.IsAI() && (bank.TurretSize == WeaponEnums.WeaponSizes.Light || bank.TurretSize == WeaponEnums.WeaponSizes.VeryLight))
				{
					if (ship != null && ship.DesignInfo != null)
					{
						if (ship.DesignInfo.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.Banks.Count((LogicalBank y) => y.TurretSize != WeaponEnums.WeaponSizes.Light && y.TurretSize != WeaponEnums.WeaponSizes.VeryLight)) <= 0)
						{
							goto IL_1DE;
						}
					}
					targetFilter = 4;
				}
				IL_1DE:
				WeaponBank weaponBank = new WeaponBank(base.App, this, bank, module, weapon, weaponLevelFromTechs, designID, targetFilter, fireMode, bank.TurretSize, bank.TurretClass);
				weaponBank.AddExistingObject(base.App);
				this.AddObject(weaponBank);
				LogicalBank localBank = bank;
				foreach (LogicalMount mount in 
					from x in section.Mounts
					where x.Bank == localBank
					select x)
				{
					string text = Ship.FixAssetNameForDLC(weaponTurretClass.GetBaseModel(faction, mount, housing), preferredMount);
					string baseDamageModel = weaponTurretClass.GetBaseDamageModel(faction, mount, housing);
					string turretModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetTurretModelName(faction, mount, housing), preferredMount);
					string barrelModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetBarrelModelName(faction, mount), preferredMount);
					GenericCollisionShape genericCollisionShape = null;
					if (flag || Ship.GetTurretNeedsCollision(bank.TurretClass, turretModelName))
					{
						genericCollisionShape = base.App.AddObject<GenericCollisionShape>(new object[]
						{
							GenericCollisionShape.CollisionShapeType.SPHERE,
							turretShapeRadius
						});
						this.AddObject(genericCollisionShape);
					}
					TurretBase turretBase = null;
					if (!string.IsNullOrEmpty(text))
					{
						turretBase = new TurretBase(base.App, text, baseDamageModel, sectionObj, module);
						this.AddObject(turretBase);
					}
					float turretHealth2 = turretHealth;
					if (weaponIns != null)
					{
						WeaponInstanceInfo weaponInstanceInfo = weaponIns.FirstOrDefault((WeaponInstanceInfo x) => x.NodeName == mount.NodeName);
						if (weaponInstanceInfo != null)
						{
							turretHealth2 = weaponInstanceInfo.Structure;
						}
					}
					Turret.TurretDescription description = new Turret.TurretDescription
					{
						BarrelModelName = barrelModelName,
						DestroyedTurretEffect = turretEffect,
						Housing = housing,
						LogicalBank = bank,
						Mount = mount,
						ParentObject = (IGameObject)module ?? sectionObj,
						Section = sectionObj,
						Module = module,
						Ship = this,
						SInfo = ship,
						Fleet = fleet,
						TurretBase = turretBase,
						CollisionShapeRadius = turretShapeRadius,
						TurretCollisionShape = genericCollisionShape,
						TurretHealth = turretHealth2,
						MaxTurretHealth = turretHealth,
						TurretModelName = turretModelName,
						WeaponBank = weaponBank,
						Weapon = weapon,
						TechModifiers = Player.ObtainWeaponTechModifiers(base.App.AssetDatabase, localBank.TurretClass, weapon, playerTechs),
						WeaponModels = weaponModels,
						TurretIndex = this._currentTurretIndex
					};
					Turret turret = new Turret(base.App, description);
					this._currentTurretIndex++;
					if (turretBase != null)
					{
						turretBase.SetTag(turret);
					}
					this.AddObject(turret);
					if (genericCollisionShape != null)
					{
						genericCollisionShape.SetTag(turret);
					}
					list.Add(turret);
				}
				if (module != null && module.LogicalModule != null)
				{
					foreach (LogicalMount mount in 
						from x in module.LogicalModule.Mounts
						where x.Bank == localBank
						select x)
					{
						string text2 = Ship.FixAssetNameForDLC(weaponTurretClass.GetBaseModel(faction, mount, housing), preferredMount);
						string baseDamageModel2 = weaponTurretClass.GetBaseDamageModel(faction, mount, housing);
						string turretModelName2 = Ship.FixAssetNameForDLC(weaponTurretClass.GetTurretModelName(faction, mount, housing), preferredMount);
						string barrelModelName2 = Ship.FixAssetNameForDLC(weaponTurretClass.GetBarrelModelName(faction, mount), preferredMount);
						GenericCollisionShape genericCollisionShape2 = null;
						if (flag || Ship.GetTurretNeedsCollision(bank.TurretClass, turretModelName2))
						{
							genericCollisionShape2 = base.App.AddObject<GenericCollisionShape>(new object[]
							{
								GenericCollisionShape.CollisionShapeType.SPHERE,
								turretShapeRadius
							});
							this.AddObject(genericCollisionShape2);
						}
						TurretBase turretBase2 = null;
						if (!string.IsNullOrEmpty(text2))
						{
							turretBase2 = new TurretBase(base.App, text2, baseDamageModel2, sectionObj, module);
							this.AddObject(turretBase2);
						}
						float turretHealth3 = turretHealth;
						if (weaponIns != null)
						{
							WeaponInstanceInfo weaponInstanceInfo2 = weaponIns.FirstOrDefault((WeaponInstanceInfo x) => x.NodeName == mount.NodeName);
							if (weaponInstanceInfo2 != null)
							{
								turretHealth3 = weaponInstanceInfo2.Structure;
							}
						}
						Turret.TurretDescription description2 = new Turret.TurretDescription
						{
							BarrelModelName = barrelModelName2,
							DestroyedTurretEffect = turretEffect,
							Housing = housing,
							LogicalBank = bank,
							Mount = mount,
                            ParentObject = (IGameObject)module ?? sectionObj,
							Section = sectionObj,
							Module = module,
							Ship = this,
							SInfo = ship,
							Fleet = fleet,
							TurretBase = turretBase2,
							CollisionShapeRadius = turretShapeRadius,
							TurretCollisionShape = genericCollisionShape2,
							TurretHealth = turretHealth3,
							MaxTurretHealth = turretHealth,
							TurretModelName = turretModelName2,
							WeaponBank = weaponBank,
							Weapon = weapon,
							WeaponModels = weaponModels,
							TechModifiers = Player.ObtainWeaponTechModifiers(base.App.AssetDatabase, localBank.TurretClass, weapon, playerTechs)
						};
						Turret turret2 = new Turret(base.App, description2);
						if (turretBase2 != null)
						{
							turretBase2.SetTag(turret2);
						}
						this.AddObject(turret2);
						if (genericCollisionShape2 != null)
						{
							genericCollisionShape2.SetTag(turret2);
						}
						list.Add(turret2);
					}
					foreach (Turret current in list)
					{
						if (module.LogicalModule.AbilityType == ModuleEnums.ModuleAbilities.Tendril)
						{
							module.PostSetProp("SetTendrilTurret", current.ObjectID);
						}
					}
				}
			}
			catch (Exception ex)
			{
				App.Log.Warn(ex.ToString(), "design");
			}
			return list;
		}
		public float GetTacSensorBonus(LogicalModule logModule, StationType stationType, int level)
		{
			if (stationType == StationType.INVALID_TYPE)
			{
				return logModule.SensorBonus;
			}
			if (logModule.ModuleType != ModuleEnums.ModuleSlotTypes.Sensor.ToString())
			{
				return logModule.SensorBonus;
			}
			switch (stationType)
			{
			case StationType.NAVAL:
				return 500f;
			case StationType.SCIENCE:
				return 250f;
			case StationType.CIVILIAN:
				return 500f;
			case StationType.DIPLOMATIC:
				return 200f;
			case StationType.GATE:
				return 500f;
			default:
				return 0f;
			}
		}
		public bool OnlyOneSquad()
		{
			SectionEnumerations.CombatAiType combatAI = this._combatAI;
			switch (combatAI)
			{
			case SectionEnumerations.CombatAiType.SwarmerHive:
			case SectionEnumerations.CombatAiType.SwarmerQueen:
				break;
			case SectionEnumerations.CombatAiType.SwarmerQueenLarva:
				return false;
			default:
				switch (combatAI)
				{
				case SectionEnumerations.CombatAiType.VonNeumannBerserkerMotherShip:
				case SectionEnumerations.CombatAiType.VonNeumannDisc:
				case SectionEnumerations.CombatAiType.LocustMoon:
				case SectionEnumerations.CombatAiType.LocustWorld:
					break;
				case SectionEnumerations.CombatAiType.VonNeumannNeoBerserker:
				case SectionEnumerations.CombatAiType.VonNeumannPyramid:
				case SectionEnumerations.CombatAiType.VonNeumannPlanetKiller:
					return false;
				default:
					if (combatAI != SectionEnumerations.CombatAiType.MorrigiRelic)
					{
						return false;
					}
					break;
				}
				break;
			}
			return true;
		}
		public Module CreateModule(LogicalModuleMount modMount, LogicalModule logModule, List<ModuleInstanceInfo> modInstances, string preferredMount, Ship ship, Section section, CollisionShape shape, CollisionShape lowStructShape, CollisionShape deadShape, bool isKillable, bool removeWhenKilled)
		{
			float num = logModule.Structure;
			ModuleInstanceInfo moduleInstanceInfo = modInstances.FirstOrDefault((ModuleInstanceInfo x) => x.ModuleNodeID == modMount.NodeName);
			if (moduleInstanceInfo != null)
			{
				num = (float)moduleInstanceInfo.Structure;
			}
			List<object> list = new List<object>();
			list.Add(ScriptHost.AllowConsole);
			list.Add(ship.ObjectID);
			list.Add(section.ObjectID);
			list.Add(Ship.FixAssetNameForDLC(logModule.ModelPath, preferredMount));
			list.Add(shape.ObjectID);
			list.Add(Ship.FixAssetNameForDLC(logModule.LowStructModelPath, preferredMount));
			list.Add((lowStructShape != null) ? lowStructShape.ObjectID : 0);
			list.Add(Ship.FixAssetNameForDLC(logModule.DeadModelPath, preferredMount));
			list.Add((deadShape != null) ? deadShape.ObjectID : 0);
			list.Add(isKillable);
			list.Add(removeWhenKilled);
			list.Add(this._currentModuleIndex);
			list.Add("");
			list.Add(modMount.NodeName);
			list.Add(logModule.AmbientSound);
			list.Add(logModule.AbilityType.ToString());
			list.Add(logModule.LowStruct);
			list.Add(logModule.Structure);
			list.Add(logModule.StructureBonus);
			list.Add(logModule.AbilitySupply);
			list.Add(logModule.Crew);
			list.Add(logModule.CrewRequired);
			list.Add(logModule.Supply);
			list.Add(logModule.PowerBonus);
			list.Add(logModule.RepairPointsBonus);
			list.Add(logModule.ECCM);
			list.Add(logModule.ECM);
			list.Add(logModule.AccelBonus);
			list.Add(logModule.CriticalHitBonus);
			list.Add(logModule.AccuracyBonus);
			list.Add(logModule.ROFBonus);
			list.Add(logModule.CrewEfficiencyBonus);
			list.Add(logModule.DamageBonus);
			list.Add(this.GetTacSensorBonus(logModule, section.ShipSectionAsset.StationType, section.ShipSectionAsset.StationLevel));
			list.Add(logModule.PsionicPowerBonus);
			list.Add(logModule.PsionicStaminaBonus);
			list.Add(logModule.AdmiralSurvivalBonus);
			list.Add(num);
			list.Add(logModule.DamageEffect.Name);
			list.Add(logModule.DeathEffect.Name);
			list.Add(500f);
			Module module;
			switch (logModule.AbilityType)
			{
			case ModuleEnums.ModuleAbilities.Tentacle:
				module = base.App.AddObject<SuulkaTentacleModule>(list.ToArray());
				goto IL_406;
			case ModuleEnums.ModuleAbilities.Tendril:
				module = base.App.AddObject<SuulkaTendrilModule>(list.ToArray());
				goto IL_406;
			case ModuleEnums.ModuleAbilities.GoopArmorRepair:
				list.Add(logModule.AbilitySupply);
				list.Add(3f);
				module = base.App.AddObject<GoopModule>(list.ToArray());
				goto IL_406;
			case ModuleEnums.ModuleAbilities.JokerECM:
				list.Add(logModule.AbilitySupply);
				list.Add(3f);
				module = base.App.AddObject<JokerECMModule>(list.ToArray());
				goto IL_406;
			case ModuleEnums.ModuleAbilities.AbaddonLaser:
				module = base.App.AddObject<AbaddonLaserModule>(list.ToArray());
				goto IL_406;
			}
			module = base.App.AddObject<Module>(list.ToArray());
			IL_406:
			this._currentModuleIndex++;
			if (module != null)
			{
				module.IsAlive = (num > 0f);
			}
			return module;
		}
		private void CreatePsionics(IEnumerable<LogicalPsionic> psionics, Ship owner, List<SectionEnumerations.PsionicAbility> psionicList)
		{
			if (psionics == null)
			{
				return;
			}
			foreach (SectionEnumerations.PsionicAbility psionicType in psionicList)
			{
				LogicalPsionic logicalPsionic = psionics.FirstOrDefault((LogicalPsionic x) => x.Name == psionicType.ToString());
				if (logicalPsionic != null)
				{
					Psionic psionic = null;
					List<object> list = new List<object>();
					list.Add(owner.ObjectID);
					list.Add(logicalPsionic.MinPower);
					list.Add(logicalPsionic.MaxPower);
					list.Add(logicalPsionic.BaseCost);
					list.Add(logicalPsionic.Range);
					list.Add(logicalPsionic.BaseDamage);
					list.Add(logicalPsionic.CastorEffect.Name);
					list.Add(logicalPsionic.CastEffect.Name);
					list.Add(logicalPsionic.ApplyEffect.Name);
					list.Add(logicalPsionic.Model);
					list.Add(this.IsSuulka);
					if (this.IsSuulka)
					{
						string suulkaName = this.MissionSection.ShipSectionAsset.SectionName;
						SuulkaPsiBonus suulkaPsiBonus = base.App.AssetDatabase.SuulkaPsiBonuses.First((SuulkaPsiBonus x) => x.Name == suulkaName);
						list.Add(suulkaPsiBonus.Rate[(int)psionicType]);
						list.Add(suulkaPsiBonus.PsiEfficiency[(int)psionicType]);
						list.Add(suulkaPsiBonus.PsiDrainMultiplyer);
						list.Add(suulkaPsiBonus.LifeDrainMultiplyer);
						list.Add(suulkaPsiBonus.TKFistMultiplyer);
						list.Add(suulkaPsiBonus.CrushMultiplyer);
						list.Add(suulkaPsiBonus.FearMultiplyer);
						list.Add(suulkaPsiBonus.ControlDuration);
						list.Add(suulkaPsiBonus.MovementMultiplyer);
					}
					switch (psionicType)
					{
					case SectionEnumerations.PsionicAbility.TKFist:
						psionic = base.App.AddObject<TKFist>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Hold:
						psionic = base.App.AddObject<Hold>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Crush:
						psionic = base.App.AddObject<Crush>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Reflector:
						psionic = base.App.AddObject<Reflector>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Repair:
						psionic = base.App.AddObject<Repair>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.AbaddonLaser:
						psionic = base.App.AddObject<AbaddonLaser>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Fear:
						psionic = base.App.AddObject<Fear>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Inspiration:
						psionic = base.App.AddObject<Inspiration>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Reveal:
						psionic = base.App.AddObject<Reveal>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Posses:
						psionic = base.App.AddObject<Posses>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Listen:
						psionic = base.App.AddObject<Listen>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Block:
						psionic = base.App.AddObject<Block>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.PsiDrain:
						psionic = base.App.AddObject<PsiDrain>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.WildFire:
						psionic = base.App.AddObject<WildFire>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Control:
						psionic = base.App.AddObject<Control>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.LifeDrain:
						psionic = base.App.AddObject<LifeDrain>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Mirage:
						psionic = base.App.AddObject<Mirage>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.FalseFriend:
						psionic = base.App.AddObject<FalseFriend>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Invisibility:
						psionic = base.App.AddObject<Invisibility>(list.ToArray());
						break;
					case SectionEnumerations.PsionicAbility.Movement:
						psionic = base.App.AddObject<Movement>(list.ToArray());
						break;
					}
					if (psionic != null)
					{
						psionic.Type = psionicType;
						psionic.SetTag(owner);
						this.AddObject(psionic);
					}
				}
			}
		}
		public void Dispose()
		{
			if (!this._isDisposed)
			{
				base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
				if (this._faction != null)
				{
					this._faction.ReleaseFactionReference(base.App);
				}
				this._faction = null;
				this._target = null;
				this._taskGroup = null;
				foreach (IGameObject current in this._objects)
				{
					current.ClearTag();
					if (current is IDisposable)
					{
						(current as IDisposable).Dispose();
					}
				}
				this.ClearTag();
				base.App.ReleaseObjects(this._objects.Concat(new Ship[]
				{
					this
				}));
			}
			this._isDisposed = true;
		}
		public BattleRiderSquad AssignRiderToSquad(BattleRiderShip brs, int riderIndex)
		{
			if (riderIndex <= -1)
			{
				return this.BattleRiderSquads.FirstOrDefault<BattleRiderSquad>();
			}
			foreach (BattleRiderSquad current in this.BattleRiderSquads)
			{
				BattleRiderMount battleRiderMount = current.Mounts.FirstOrDefault((BattleRiderMount x) => x.SquadIndex == riderIndex);
				if (battleRiderMount != null && BattleRiderMount.CanBattleRiderConnect(battleRiderMount.WeaponBank.TurretClass, brs.BattleRiderType, brs.ShipClass))
				{
					return current;
				}
			}
			return null;
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName != null)
			{
				if (eventName == "FireOnObject")
				{
					this.ProcessGameEvent_FireOnObject(eventParams);
					return;
				}
				if (eventName == "ClearTargets")
				{
					this.ProcessGameEvent_ClearTargets(eventParams);
					return;
				}
				if (eventName == "HoldFire")
				{
					this.ProcessGameEvent_HoldFire(eventParams);
					return;
				}
				if (eventName == "FireOnObjectWithSpecWeapon")
				{
					this.ProcessGameEvent_FireOnObjectWithSpecWeapon(eventParams);
					return;
				}
				if (eventName == "RemoveShipContainingID")
				{
					this.ProcessGameEvent_RemoveShipContainingID(eventParams);
					return;
				}
				if (!(eventName == "RemoveObjectsInCompound"))
				{
					return;
				}
				this.ProcessGameEvent_RemoveObjectsInCompound(eventParams);
			}
		}
		private void ProcessGameEvent_RemoveObjectsInCompound(string[] eventParams)
		{
			char c = '_';
			string[] array = eventParams[0].Split(new char[]
			{
				c
			});
			int objID = int.Parse(array[0]);
			if (!this._objects.Any((IGameObject x) => x.ObjectID == objID) || array.Count<string>() < 2)
			{
				return;
			}
			for (int i = 1; i < array.Count<string>(); i++)
			{
				int id = int.Parse(array[i]);
				IGameObject gameObject = this._objects.FirstOrDefault((IGameObject x) => x.ObjectID == id);
				if (gameObject != null)
				{
					this.RemoveObject(gameObject);
				}
			}
		}
		private void ProcessGameEvent_RemoveShipContainingID(string[] eventParams)
		{
			char c = '_';
			string[] array = eventParams[0].Split(new char[]
			{
				c
			});
			int num = int.Parse(array[0]);
			bool flag = false;
			foreach (IGameObject current in this._objects)
			{
				if (num == current.ObjectID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			this.Dispose();
		}
		private void ProcessGameEvent_ClearTargets(string[] eventParams)
		{
			char c = '_';
			string[] array = eventParams[0].Split(new char[]
			{
				c
			});
			int shipId = int.Parse(array[0]);
			if (!this._objects.Any((IGameObject x) => x.ObjectID == shipId))
			{
				return;
			}
			if (this.IsCarrier || this._shipClass == ShipClass.BattleRider)
			{
				this.PostSetProp("ClearTarget", new object[0]);
			}
			foreach (WeaponBank current in this.WeaponBanks)
			{
				current.PostSetProp("ClearTarget", new object[0]);
			}
			foreach (Module current2 in 
				from x in this.Modules
				where x is SuulkaTendrilModule || x is SuulkaTentacleModule
				select x)
			{
				current2.PostSetProp("ClearTarget", new object[0]);
			}
		}
		private void ProcessGameEvent_HoldFire(string[] eventParams)
		{
			char c = '_';
			string[] array = eventParams[0].Split(new char[]
			{
				c
			});
			int num = int.Parse(array[0]);
			if (num != base.ObjectID)
			{
				return;
			}
			bool value = int.Parse(array[1]) == 1;
			this.PostSetProp("HoldFire", value);
		}
		private void ProcessGameEvent_FireOnObject(string[] eventParams)
		{
			char c = '_';
			string[] array = eventParams[0].Split(new char[]
			{
				c
			});
			int num = int.Parse(array[0]);
			if (num != base.ObjectID)
			{
				return;
			}
			bool setAsRiderTarget = int.Parse(array[6]) == 1 || base.App.CurrentState is DesignScreenState || base.App.CurrentState is ComparativeAnalysysState;
			this.SetShipTarget(int.Parse(array[1]), new Vector3(float.Parse(array[3]), float.Parse(array[4]), float.Parse(array[5])), setAsRiderTarget, int.Parse(array[2]));
		}
		private void ProcessGameEvent_FireOnObjectWithSpecWeapon(string[] eventParams)
		{
			char c = '^';
			string[] msg = eventParams[0].Split(new char[]
			{
				c
			});
			if (msg.Length < 6)
			{
				return;
			}
			int num = int.Parse(msg[0]);
			if (num != base.ObjectID)
			{
				return;
			}
			foreach (Turret current in 
				from x in this.Turrets
				where x.Weapon.WeaponName == msg[1]
				select x)
			{
				current.PostSetProp("SetTarget", new object[]
				{
					int.Parse(msg[2]),
					new Vector3(float.Parse(msg[3]), float.Parse(msg[4]), float.Parse(msg[5])),
					false
				});
			}
		}
		public void SyncAltitude()
		{
			if (this._shipClass == ShipClass.BattleRider || this._shipClass == ShipClass.Station || this._combatAI != SectionEnumerations.CombatAiType.Normal)
			{
				return;
			}
			this.PostSetProp("SyncAltitude", this.Position.Y);
		}
		public void SetShipTarget(int targetId, Vector3 localPosition, bool setAsRiderTarget = true, int subTargetId = 0)
		{
			this.PostSetProp("SetMainShipTarget", new object[]
			{
				targetId,
				subTargetId,
				localPosition,
				setAsRiderTarget
			});
			this._target = base.App.GetGameObject(targetId);
		}
		public void SetBlindFireTarget(Vector3 center, Vector3 localPosition, float radius, float fireDuration)
		{
			this.PostSetProp("SetBlindFireTarget", new object[]
			{
				center,
				localPosition,
				radius,
				fireDuration
			});
			this._blindFireActive = true;
			this._target = null;
		}
		public void SetShipSpecWeaponTarget(int weaponID, int targetId, Vector3 localPosition)
		{
			foreach (WeaponBank current in 
				from x in this.WeaponBanks
				where x.Weapon.UniqueWeaponID == weaponID
				select x)
			{
				current.PostSetProp("SetTarget", new object[]
				{
					targetId,
					localPosition
				});
			}
		}
		public void SetShipWeaponToggleOn(int weaponID, bool on)
		{
			foreach (WeaponBank current in 
				from x in this.WeaponBanks
				where x.Weapon.UniqueWeaponID == weaponID
				select x)
			{
				current.ToggleState = on;
			}
		}
		public void SetShipPositionalTarget(int weaponID, Vector3 targetPos, bool clearCurrentTargets)
		{
			WeaponBank weaponBank = this.WeaponBanks.FirstOrDefault((WeaponBank x) => x.Weapon.UniqueWeaponID == weaponID);
			if (weaponBank != null)
			{
				this.PostSetProp("SetPositionTarget", new object[]
				{
					weaponBank.Weapon.GameObject.ObjectID,
					targetPos,
					clearCurrentTargets
				});
			}
		}
		public void InitialSetPos(Vector3 position, Vector3 rotation)
		{
			this.Position = position;
			this.Rotation = rotation;
			this.Maneuvering.Destination = position;
		}
		public void KillShip(bool instantRemove = false)
		{
			if (!this._bIsDestroyed)
			{
				this.PostSetProp("KillShip", instantRemove);
			}
			this._bIsDestroyed = true;
			this.IsDriveless = true;
		}
		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (base.OnEngineMessage(messageId, message))
			{
				return true;
			}
			if (messageId != InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE)
			{
				if (messageId != InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
				{
					if (messageId == InteropMessageID.IMID_SCRIPT_OBJECT_SETPLAYER)
					{
						int id = message.ReadInteger();
						bool flag = message.ReadBool();
						Player gameObject = base.App.GetGameObject<Player>(id);
						this.Player = gameObject;
						if (flag)
						{
							this.DisableManeuvering(true);
						}
						return true;
					}
					App.Log.Warn("Unhandled message (id=" + messageId + ").", "design");
				}
				else
				{
					string a = message.ReadString();
					if (a == "Position")
					{
						Vector3 position = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
						this.Position = position;
						return true;
					}
					if (a == "Stance")
					{
						this._stance = (CombatStance)message.ReadInteger();
						if (this._stance == CombatStance.RETREAT && !this.Maneuvering.RetreatData.SetDestination)
						{
							this.Maneuvering.RetreatDestination = Vector3.Normalize(this.Maneuvering.Position) * this.Maneuvering.RetreatData.SystemRadius;
						}
						return true;
					}
					if (a == "SetAcceptMoveOrders")
					{
						int num = message.ReadInteger();
						this._bCanAcceptMoveOrders = (num == 1);
						return true;
					}
					if (a == "UnderAttack")
					{
						this._isUnderAttack = message.ReadBool();
						return true;
					}
					if (a == "DockedWithParent")
					{
						this._bDockedWithParent = message.ReadBool();
						return true;
					}
					if (a == "CarrierReadyToLaunch")
					{
						this._bCarrierCanLaunch = message.ReadBool();
						return true;
					}
					if (a == "AssaultingPlanet")
					{
						this._bAssaultingPlanet = message.ReadBool();
						return true;
					}
					if (a == "Cloaked")
					{
						this._cloakedState = (CloakedState)message.ReadInteger();
						return true;
					}
					if (a == "SetShipSphere")
					{
						this._boundingSphere.center.X = message.ReadSingle();
						this._boundingSphere.center.Y = message.ReadSingle();
						this._boundingSphere.center.Z = message.ReadSingle();
						this._boundingSphere.radius = message.ReadSingle();
						return true;
					}
					if (a == "WeaponFiringStateChanged")
					{
						this._turretFiring = (Turret.FiringEnum)message.ReadInteger();
						return true;
					}
					if (a == "SetPriorityWeapon")
					{
						this._priorityWeapon = message.ReadString();
						return true;
					}
					if (a == "ActivatedDeadObjects")
					{
						List<int> list = new List<int>();
						int num2 = message.ReadInteger();
						for (int i = 0; i < num2; i++)
						{
							list.Add(message.ReadInteger());
						}
						this.HandleActivatedDeadObjects(list);
						return true;
					}
					if (a == "ShipKilled")
					{
						this._instantlyKilled = message.ReadBool();
						this._bIsDestroyed = true;
						this.IsDriveless = true;
						return true;
					}
					if (a == "BlindFireActive")
					{
						this._blindFireActive = message.ReadBool();
						return true;
					}
					if (a == "Retreated")
					{
						if (this._shipRole != ShipRole.FREIGHTER)
						{
							this._bHasRetreated = true;
						}
						this.Active = false;
						return true;
					}
					if (a == "HitByNodeCannon")
					{
						this._bHitByNodeCannon = true;
						this.Active = false;
						return true;
					}
					if (a == "DisablePolicePatrol")
					{
						this._isPolicePatrolling = false;
						if (this._taskGroup != null)
						{
							this._taskGroup.RemoveShip(this);
							this._taskGroup = null;
						}
						return true;
					}
					if (a == "ActivateDefenseBoat")
					{
						this._defenseBoatActive = true;
						return true;
					}
					if (a == "SectionKilled")
					{
						int num3 = message.ReadInteger();
						foreach (Section current in this.Sections)
						{
							if (num3 == current.ObjectID)
							{
								current.IsAlive = false;
								this.HandleSectionsKilled();
								break;
							}
						}
						return true;
					}
					if (a == "ModuleKilled")
					{
						int num4 = message.ReadInteger();
						int destroyedByPlayer = message.ReadInteger();
						foreach (Module current2 in this.Modules)
						{
							if (num4 == current2.ObjectID)
							{
								current2.IsAlive = false;
								current2.DestroyedByPlayer = destroyedByPlayer;
								this.HandleModuleKilled();
								break;
							}
						}
						return true;
					}
					if (a == "DisableTurrets")
					{
						int sectID = message.ReadInteger();
						bool value = message.ReadBool();
						foreach (Turret current3 in 
							from x in this.Turrets
							where x.ParentID == sectID
							select x)
						{
							current3.PostSetProp("Disable", value);
						}
						return true;
					}
					if (a == "KillShipNoExplode")
					{
						this.DisableManeuvering(true);
						foreach (Turret current4 in this.Turrets)
						{
							current4.PostSetProp("Disable", true);
						}
						this._bIsDestroyed = true;
						return true;
					}
					if (a == "UpdatePsiPower")
					{
						this._currentPsiPower = message.ReadInteger();
						return true;
					}
				}
				return false;
			}
			int num5 = message.ReadInteger();
			if (num5 == base.ObjectID)
			{
				this.Dispose();
			}
			return true;
		}
		private void HandleActivatedDeadObjects(List<int> ids)
		{
			List<IGameObject> list = new List<IGameObject>();
			foreach (int id in ids)
			{
				IGameObject gameObject = this._objects.FirstOrDefault((IGameObject x) => x.ObjectID == id);
				if (gameObject != null)
				{
					if (gameObject is Section)
					{
						(gameObject as Section).IsAlive = false;
					}
					else
					{
						if (gameObject is Module)
						{
							(gameObject as Module).IsAlive = false;
							Section attachedSection = (gameObject as Module).AttachedSection;
							if (attachedSection == null || !attachedSection.IsAlive)
							{
								list.Add(gameObject);
							}
						}
						else
						{
							if (gameObject is MountObject)
							{
								list.Add(gameObject);
							}
							else
							{
								if (gameObject is Shield)
								{
									list.Add(gameObject);
								}
							}
						}
					}
				}
			}
			this.HandleSectionsKilled();
			this.HandleModuleKilled();
			this.RemoveObjects(list);
		}
		private void HandleSectionsKilled()
		{
			float num = 0f;
			Vector3 zero = Vector3.Zero;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			int num5 = 0;
			int num6 = 0;
			foreach (Section current in this.Sections)
			{
				ShipSectionAsset tag = current.GetTag<ShipSectionAsset>();
				if (current.IsAlive)
				{
					num5++;
					num += tag.Maneuvering.LinearAccel;
					zero.X += tag.Maneuvering.RotAccel.X;
					zero.Y += tag.Maneuvering.RotAccel.Y;
					zero.Z += tag.Maneuvering.RotAccel.Z;
					num2 += tag.Maneuvering.LinearSpeed;
					num3 += tag.Maneuvering.RotationSpeed;
				}
				else
				{
					num6++;
					num += ((tag.Maneuvering.LinearAccel < 0f) ? 1.1f : 0.1f) * tag.Maneuvering.LinearAccel;
					zero.X += ((tag.Maneuvering.RotAccel.X < 0f) ? 1.1f : 0.1f) * tag.Maneuvering.RotAccel.X;
					zero.Y += ((tag.Maneuvering.RotAccel.Y < 0f) ? 1.1f : 0.1f) * tag.Maneuvering.RotAccel.Y;
					zero.Z += ((tag.Maneuvering.RotAccel.Z < 0f) ? 1.1f : 0.1f) * tag.Maneuvering.RotAccel.Z;
					num2 += ((tag.Maneuvering.LinearSpeed < 0f) ? 1.1f : 0.1f) * tag.Maneuvering.LinearSpeed;
					num3 += ((tag.Maneuvering.RotationSpeed < 0f) ? 1.1f : 0.1f) * tag.Maneuvering.RotationSpeed;
				}
				num4 += tag.Maneuvering.Deacceleration;
				if (tag.Type == ShipSectionType.Engine && !current.IsAlive)
				{
					this.IsDriveless = true;
				}
			}
			num4 = ((num4 >= 1f) ? num4 : 2f);
			zero.X = Math.Max(zero.X, 5f);
			zero.Y = Math.Max(zero.Y, 5f);
			zero.Z = Math.Max(zero.Z, 5f);
			this.Maneuvering.PostSetProp("SetManeuveringInfo", new object[]
			{
				Math.Max(num, 10f),
				zero,
				Math.Max(num2, 10f),
				Math.Max(num3, 5f),
				num4,
				this._bIsDriveless
			});
			List<IGameObject> list = new List<IGameObject>();
			foreach (TurretBase current2 in this.TurretBases)
			{
				if (current2.AttachedSection != null && !current2.AttachedSection.IsAlive)
				{
					list.Add(current2);
				}
			}
			this.RemoveObjects(list);
		}
		private void HandleModuleKilled()
		{
			List<IGameObject> list = new List<IGameObject>();
			foreach (TurretBase current in this.TurretBases)
			{
				if ((current.AttachedSection != null && !current.AttachedSection.IsAlive) || (current.AttachedModule != null && !current.AttachedModule.IsAlive))
				{
					list.Add(current);
				}
			}
			this.RemoveObjects(list);
		}
		private void DisableManeuvering(bool disable)
		{
			this.Maneuvering.PostSetProp("SetDriveless", disable);
			this.IsDriveless = disable;
		}
		public bool IsDetected(Player player)
		{
			if (this._detectionStates == null || this._detectionStates.Count == 0)
			{
				return true;
			}
			foreach (Ship.DetectionState current in this._detectionStates)
			{
				if (current.playerID == player.ID)
				{
					return current.scanned;
				}
			}
			return false;
		}
		public bool IsVisible(Player player)
		{
			foreach (Ship.DetectionState current in this._detectionStates)
			{
				if (current.playerID == player.ID)
				{
					return current.spotted;
				}
			}
			return false;
		}
		public void SetCombatStance(CombatStance stance)
		{
			this.PostSetProp("SetCombatStance", (int)stance);
			this._stance = stance;
		}
		public void SetCloaked(bool cloaked)
		{
			this.PostSetProp("SetCloaked", cloaked);
		}
		public int GetAddedResourcesAsTaskObjective()
		{
			SectionEnumerations.CombatAiType combatAI = this._combatAI;
			switch (combatAI)
			{
			case SectionEnumerations.CombatAiType.SwarmerHive:
				return 50;
			case SectionEnumerations.CombatAiType.SwarmerQueenLarva:
				return 50;
			case SectionEnumerations.CombatAiType.SwarmerQueen:
				return 100;
			default:
				if (combatAI == SectionEnumerations.CombatAiType.VonNeumannPlanetKiller)
				{
					return 300;
				}
				if (combatAI != SectionEnumerations.CombatAiType.CommandMonitor)
				{
					return 0;
				}
				return 50;
			}
		}
		public int GetCruiserEquivalent()
		{
			switch (this._shipClass)
			{
			case ShipClass.Cruiser:
				return 1;
			case ShipClass.Dreadnought:
				return 3;
			case ShipClass.Leviathan:
				return 9;
			case ShipClass.BattleRider:
				return 0;
			case ShipClass.Station:
				return 27;
			default:
				return 0;
			}
		}
		public Ship.DetectionState GetDetectionStateForPlayer(int playerID)
		{
			if (this._detectionStates == null)
			{
				this._detectionStates = new List<Ship.DetectionState>();
			}
			foreach (Ship.DetectionState current in this._detectionStates)
			{
				if (current.playerID == playerID)
				{
					return current;
				}
			}
			Ship.DetectionState detectionState = new Ship.DetectionState(playerID);
			this._detectionStates.Add(detectionState);
			return detectionState;
		}
		public Turret GetTurretWithWeaponTrait(WeaponEnums.WeaponTraits trait)
		{
			foreach (Turret current in this.Turrets)
			{
				if (current.Weapon.Traits.Any((WeaponEnums.WeaponTraits x) => x == trait))
				{
					return current;
				}
			}
			return null;
		}
		public WeaponBank GetWeaponBankWithWeaponTrait(WeaponEnums.WeaponTraits trait)
		{
			foreach (WeaponBank current in this.WeaponBanks)
			{
				if (current.Weapon.Traits.Any((WeaponEnums.WeaponTraits x) => x == trait))
				{
					return current;
				}
			}
			return null;
		}
		public static bool IsActiveShip(Ship ship)
		{
			return ship != null && !ship.IsDestroyed && !ship.DockedWithParent && !ship.HasRetreated && !ship.HitByNodeCannon && !ship.IsNeutronStar;
		}
	}
}
