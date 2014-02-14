using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Ships;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.GameStates
{
	internal abstract class CommonCombatState : GameState
	{
		private enum CombatSubState
		{
			Running,
			Ending,
			Ended
		}
		public class PiracyFreighterInfo
		{
			public int FreighterID;
			public int ShipID;
		}
		public class PirateEncounterData
		{
			public bool IsPirateEncounter;
			public PirateBaseInfo PirateBase;
			public List<int> PiratePlayerIDs;
			public List<Ship> PoliceShipsInSystem;
			public Dictionary<int, List<Ship>> PirateShipsInSystem;
			public Dictionary<int, int> PlayerBounties;
			public Dictionary<int, List<CommonCombatState.PiracyFreighterInfo>> PlayerFreightersInSystem;
			public Dictionary<int, int> DestroyedFreighters;
			public PirateEncounterData()
			{
				this.IsPirateEncounter = false;
				this.PirateBase = null;
				this.PiratePlayerIDs = new List<int>();
				this.PoliceShipsInSystem = new List<Ship>();
				this.PirateShipsInSystem = new Dictionary<int, List<Ship>>();
				this.PlayerBounties = new Dictionary<int, int>();
				this.PlayerFreightersInSystem = new Dictionary<int, List<CommonCombatState.PiracyFreighterInfo>>();
				this.DestroyedFreighters = new Dictionary<int, int>();
			}
			public void Clear()
			{
				this.IsPirateEncounter = false;
				this.PirateBase = null;
				this.PiratePlayerIDs.Clear();
				this.PoliceShipsInSystem.Clear();
				this.PirateShipsInSystem.Clear();
				this.PlayerBounties.Clear();
				this.PlayerFreightersInSystem.Clear();
				this.DestroyedFreighters.Clear();
			}
		}
		public class ColonyTrapCombatData
		{
			public bool IsTrapCombat;
			public List<int> TrapPlayers;
			public Dictionary<int, int> ColonyFleetToTrap;
			public Dictionary<int, int> TrapToPlanet;
			public List<FleetInfo> TrapFleets;
			public List<FleetInfo> ColonyTrappedFleets;
			public ColonyTrapCombatData()
			{
				this.IsTrapCombat = false;
				this.ColonyFleetToTrap = new Dictionary<int, int>();
				this.TrapToPlanet = new Dictionary<int, int>();
				this.TrapPlayers = new List<int>();
				this.TrapFleets = new List<FleetInfo>();
				this.ColonyTrappedFleets = new List<FleetInfo>();
			}
			public void Clear()
			{
				this.IsTrapCombat = false;
				this.ColonyFleetToTrap.Clear();
				this.TrapToPlanet.Clear();
				this.TrapPlayers.Clear();
				this.TrapFleets.Clear();
				this.ColonyTrappedFleets.Clear();
			}
		}
		public const string ScratchFile = "data/scratch_combat.xml";
		private const int DEFAULT_COMMAND_POINTS = 18;
		protected GameObjectSet _crits;
		public List<IGameObject> _postLoadedObjects;
		private Sky _sky;
		private OrbitCameraController _camera;
		private Combat _combat;
		private CombatInput _input;
		private CombatGrid _grid;
		private CombatSensor _sensor;
		public StarSystem _starSystemObjects;
		protected int _systemId;
		protected int _combatId;
		private int _detectionUpdateRate;
		private bool _testingState;
		private bool _authority;
		private bool _ignoreEncounterSpawnPos;
		private CommonCombatState.PirateEncounterData _pirateEncounterData;
		private CommonCombatState.ColonyTrapCombatData _trapCombatData;
		private List<SpawnProfile> _spawnPositions;
		private List<EntrySpawnLocation> _entryLocations;
		private List<Ship> _hitByNodeCannon;
		private List<FleetInfo> _mediaHeroFleets;
		private Random _random;
		private NeutralCombatStanceInfo _neutralCombatStanceInfo;
		private BidirMap<int, Ship> _ships;
		public List<CombatAI> AI_Commanders;
		public List<PointOfInterest> _pointsOfInterest;
		private CommonCombatState.CombatSubState _subState;
		private bool _combatEndingStatsGathered;
		private bool _combatEndDelayComplete;
		private bool _isPaused;
		private bool _engCombatActive;
		private PendingCombat _lastPendingCombat;
		protected List<Player> _playersInCombat;
		private List<Player> _ignoreCombatZonePlayers;
		private Dictionary<int, Dictionary<int, DiplomacyState>> _initialDiploStates;
		protected List<Player> _playersWithAssets;
		public Dictionary<int, Dictionary<int, bool>> _combatStanceMap = new Dictionary<int, Dictionary<int, bool>>();
		private Dictionary<int, int> _fleetsPerPlayer;
		private CombatData _combatData;
		protected Vector3? _simSpawnPosition = null;
		private bool _sim;
		private Vector3 _origin = Vector3.Zero;
		private XmlDocument _combatConfig;
		private Dictionary<IGameObject, XmlElement> _gameObjectConfigs;
		public static bool RetainCombatConfig;
		private List<DetectionSpheres> m_DetectionSpheres;
		private List<DetectionSpheres> m_SlewPlanetDetectionSpheres;
		private bool m_SlewMode;
		public IEnumerable<IGameObject> Objects
		{
			get
			{
				return this._crits.Objects;
			}
		}
		protected bool SimMode
		{
			get
			{
				return this._sim;
			}
			set
			{
				this._sim = value;
				foreach (CombatAI current in this.AI_Commanders)
				{
					current.SimMode = this._sim;
				}
			}
		}
		public Vector3 Origin
		{
			get
			{
				return this._origin;
			}
		}
		public CombatInput Input
		{
			get
			{
				return this._input;
			}
		}
		public Combat Combat
		{
			get
			{
				return this._combat;
			}
		}
		public List<Player> PlayersInCombat
		{
			get
			{
				return this._playersInCombat;
			}
		}
		public int GetCombatID()
		{
			return this._combatId;
		}
		public void SaveCombatConfig(string filename)
		{
			if (!CommonCombatState.RetainCombatConfig)
			{
				throw new InvalidOperationException("There is no retained combat configuration to recover evidence from. (Did you forget to tell the game to retain combat configuration evidence before entering combat?)");
			}
			foreach (IGameObject current in this.Objects)
			{
				if (this._gameObjectConfigs.ContainsKey(current))
				{
					Ship ship = current as Ship;
					if (ship != null && ship.Active)
					{
						Vector3 position = ship.Maneuvering.Position;
						Vector3 rotation = ship.Maneuvering.Rotation;
						Vector3 rotation2 = Vector3.RadiansToDegrees(Matrix.CreateWorld(Vector3.Zero, rotation, Vector3.UnitY).EulerAngles);
						CombatConfig.ChangeXmlElementPositionAndRotation(this._gameObjectConfigs[current], position, rotation2);
					}
				}
			}
			this._combatConfig.Save(filename);
		}
		public CommonCombatState(App game) : base(game)
		{
			this.AI_Commanders = new List<CombatAI>();
			this._postLoadedObjects = new List<IGameObject>();
			this.m_DetectionSpheres = new List<DetectionSpheres>();
			this.m_SlewPlanetDetectionSpheres = new List<DetectionSpheres>();
			this._spawnPositions = new List<SpawnProfile>();
			this._pointsOfInterest = new List<PointOfInterest>();
			this._hitByNodeCannon = new List<Ship>();
			this._mediaHeroFleets = new List<FleetInfo>();
			this._isPaused = false;
			this._engCombatActive = false;
			this._pirateEncounterData = new CommonCombatState.PirateEncounterData();
			this._trapCombatData = new CommonCombatState.ColonyTrapCombatData();
			this.m_SlewMode = false;
			this._playersInCombat = new List<Player>();
			this._ignoreCombatZonePlayers = new List<Player>();
			this._initialDiploStates = null;
			this._playersWithAssets = new List<Player>();
			this._neutralCombatStanceInfo = default(NeutralCombatStanceInfo);
			this._detectionUpdateRate = 0;
			this._random = new Random();
		}
		public static Vector3 ApplyOriginShift(Vector3 origin, Vector3 position)
		{
			return position - origin;
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			this._simSpawnPosition = null;
			this._ships = new BidirMap<int, Ship>();
			base.App.ObjectReleased += new Action<IGameObject>(this.Game_ObjectReleased);
			this._pirateEncounterData.Clear();
			this._trapCombatData.Clear();
			this._detectionUpdateRate = 0;
			if (stateParams == null || stateParams.Length == 0)
			{
				this.SimMode = false;
				this._authority = true;
				if (base.App.GameDatabase == null)
				{
					base.App.NewGame();
				}
				object[] array = new object[2];
				object[] arg_D1_0 = array;
				int arg_D1_1 = 0;
				PendingCombat pendingCombat = new PendingCombat();
				pendingCombat.SystemID = base.App.GameDatabase.GetHomeworlds().First((HomeworldInfo x) => x.PlayerID == base.App.LocalPlayer.ID).SystemID;
				arg_D1_0[arg_D1_1] = pendingCombat;
				stateParams = array;
			}
			this._spawnPositions.Clear();
			this._lastPendingCombat = ((stateParams[0] is PendingCombat) ? ((PendingCombat)stateParams[0]) : new PendingCombat());
			this._systemId = this._lastPendingCombat.SystemID;
			this._combatId = this._lastPendingCombat.ConflictID;
			this._trapCombatData.IsTrapCombat = (this._lastPendingCombat.Type == CombatType.CT_Colony_Trap);
			this._pirateEncounterData.IsPirateEncounter = (this._lastPendingCombat.Type == CombatType.CT_Piracy);
			this._pirateEncounterData.PirateBase = base.App.GameDatabase.GetPirateBaseInfos().FirstOrDefault((PirateBaseInfo x) => x.SystemId == this._systemId);
			XmlDocument xmlDocument = (XmlDocument)stateParams[1];
			if (stateParams.Length >= 4)
			{
				this._testingState = (bool)stateParams[2];
				this._authority = (bool)stateParams[3];
			}
			else
			{
				this._testingState = true;
				this._authority = true;
			}
			int shipOrbitParentId = this.SelectOriginOrbital(this._systemId);
			this._origin = Vector3.Zero;
			float radius = 1E+09f;
			this._crits = new GameObjectSet(base.App);
			this._sensor = this._crits.Add<CombatSensor>(new object[0]);
			this._input = this._crits.Add<CombatInput>(new object[0]);
			this._starSystemObjects = new StarSystem(base.App, 1f, this._systemId, Vector3.Zero, true, this._sensor, true, this._input.ObjectID, false, true);
			this._starSystemObjects.PostSetProp("InputEnabled", false);
			this._crits.Add(this._starSystemObjects);
			foreach (Ship current in this._starSystemObjects.Crits.OfType<Ship>())
			{
				this._crits.Add(current);
			}
			this._sky = new Sky(base.App, SkyUsage.InSystem, this._systemId);
			this._crits.Add(this._sky);
			this._camera = this._crits.Add<OrbitCameraController>(new object[0]);
			this._grid = this._crits.Add<CombatGrid>(new object[0]);
			this._fleetsPerPlayer = new Dictionary<int, int>();
			foreach (int current2 in this._lastPendingCombat.FleetIDs)
			{
				FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(current2);
				if (fleetInfo != null)
				{
					if (this._fleetsPerPlayer.ContainsKey(fleetInfo.PlayerID))
					{
						Dictionary<int, int> fleetsPerPlayer;
						int playerID;
						(fleetsPerPlayer = this._fleetsPerPlayer)[playerID = fleetInfo.PlayerID] = fleetsPerPlayer[playerID] + 1;
					}
					else
					{
						this._fleetsPerPlayer[fleetInfo.PlayerID] = 1;
					}
				}
			}
			Dictionary<IGameObject, XmlElement> dictionary = new Dictionary<IGameObject, XmlElement>();
			if (xmlDocument != null)
			{
				dictionary = CombatConfig.CreateGameObjects(base.App, this.Origin, xmlDocument, this._input.ObjectID);
			}
			if (this._lastPendingCombat.PlayersInCombat != null && this._lastPendingCombat.PlayersInCombat.Count > 0)
			{
				this._playersInCombat = new List<Player>();
				using (List<int>.Enumerator enumerator2 = this._lastPendingCombat.PlayersInCombat.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						int current3 = enumerator2.Current;
						this._playersInCombat.Add(base.App.Game.GetPlayerObject(current3));
					}
					goto IL_4F4;
				}
			}
			this._playersInCombat = GameSession.GetPlayersWithCombatAssets(base.App, this._systemId).ToList<Player>();
			base.App.GameDatabase.GetFreighterInfosForSystem(this._systemId);
			foreach (IGameObject current4 in dictionary.Keys)
			{
				if (current4 is Ship)
				{
					Player player = (current4 as Ship).Player;
					if (!this._playersInCombat.Contains(player))
					{
						this._playersInCombat.Add(player);
					}
				}
			}
			IL_4F4:
			foreach (int npcId in this._lastPendingCombat.NPCPlayersInCombat)
			{
				if (!this._playersInCombat.Any((Player x) => x.ID == npcId))
				{
					this._playersInCombat.Add(base.App.Game.GetPlayerObject(npcId));
				}
			}
			List<FleetInfo> list = new List<FleetInfo>();
			if (this._pirateEncounterData.IsPirateEncounter)
			{
				List<FleetInfo> list2 = (
					from x in base.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_NORMAL)
                    where !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, x)
					select x).ToList<FleetInfo>();
				foreach (FleetInfo current5 in list2)
				{
					MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(current5.ID);
					if (missionByFleetID != null && missionByFleetID.Type == MissionType.PIRACY)
					{
						list.Add(current5);
						Player playerObject = base.App.Game.GetPlayerObject(current5.PlayerID);
						if (!this._playersInCombat.Contains(playerObject))
						{
							this._playersInCombat.Add(playerObject);
						}
					}
				}
			}
			foreach (Player current6 in this._playersInCombat)
			{
				this._playersWithAssets.Add(current6);
			}
			Dictionary<int, List<FleetInfo>> dictionary2 = new Dictionary<int, List<FleetInfo>>();
			if (this._lastPendingCombat != null)
			{
				foreach (FleetInfo current7 in list)
				{
					if (!dictionary2.ContainsKey(current7.PlayerID))
					{
						dictionary2[current7.PlayerID] = new List<FleetInfo>(new FleetInfo[]
						{
							current7
						});
						this._pirateEncounterData.PiratePlayerIDs.Add(current7.PlayerID);
					}
				}
				foreach (KeyValuePair<int, int> selectedPlayerFleet in this._lastPendingCombat.SelectedPlayerFleets)
				{
					if (this._pirateEncounterData.IsPirateEncounter)
					{
						if (list.Any(delegate(FleetInfo x)
						{
							int arg_14_0 = x.PlayerID;
							KeyValuePair<int, int> selectedPlayerFleet2 = selectedPlayerFleet;
							return arg_14_0 == selectedPlayerFleet2.Key;
						}))
						{
							continue;
						}
					}
					GameDatabase arg_79D_0 = base.App.GameDatabase;
					KeyValuePair<int, int> selectedPlayerFleet3 = selectedPlayerFleet;
					FleetInfo fleetInfo2 = arg_79D_0.GetFleetInfo(selectedPlayerFleet3.Value);
					if (fleetInfo2 == null)
					{
						GameDatabase arg_7CA_0 = base.App.GameDatabase;
						selectedPlayerFleet3 = selectedPlayerFleet;
						List<FleetInfo> list3 = arg_7CA_0.GetFleetsByPlayerAndSystem(selectedPlayerFleet3.Key, this._systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
						if (list3 != null && list3.Count > 0)
						{
							fleetInfo2 = list3.First<FleetInfo>();
						}
					}
					if (this._trapCombatData.IsTrapCombat)
					{
						PlayerInfo pi = (fleetInfo2 != null) ? base.App.GameDatabase.GetPlayerInfo(fleetInfo2.PlayerID) : null;
						Faction faction = (pi != null) ? base.App.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.ID == pi.FactionID) : null;
						if (faction == null || faction.Name == "morrigi")
						{
							fleetInfo2 = null;
						}
					}
					Dictionary<int, List<FleetInfo>> arg_8A0_0 = dictionary2;
					selectedPlayerFleet3 = selectedPlayerFleet;
					arg_8A0_0[selectedPlayerFleet3.Key] = new List<FleetInfo>(new FleetInfo[]
					{
						fleetInfo2
					});
				}
			}
			if (this._systemId != 0)
			{
				List<FleetInfo> list4 = base.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
				list4.AddRange(base.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_ALL_COMBAT).ToList<FleetInfo>());
				list4.AddRange((
					from x in base.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_NORMAL)
                    where Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, x)
					select x).ToList<FleetInfo>());
				foreach (FleetInfo current8 in list4)
				{
					if (current8 != null)
					{
						if (!dictionary2.ContainsKey(current8.PlayerID))
						{
							dictionary2[current8.PlayerID] = new List<FleetInfo>(new FleetInfo[]
							{
								current8
							});
						}
						else
						{
                            if (!dictionary2[current8.PlayerID].Contains(current8) && ((FleetType.FL_ALL_COMBAT & current8.Type) != (FleetType)0 || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, current8)))
							{
								dictionary2[current8.PlayerID].Add(current8);
							}
						}
					}
				}
				VonNeumann vonNeumann = base.App.Game.ScriptModules.VonNeumann;
				if (vonNeumann != null && vonNeumann.HomeWorldSystemID == this._systemId)
				{
					foreach (FleetInfo current9 in list4)
					{
						if (current9 != null && current9.PlayerID == vonNeumann.PlayerID && vonNeumann.CanSpawnFleetAtHomeWorld(current9))
						{
							if (!dictionary2.ContainsKey(current9.PlayerID))
							{
								dictionary2[current9.PlayerID] = new List<FleetInfo>(new FleetInfo[]
								{
									current9
								});
							}
							else
							{
								if (!dictionary2[current9.PlayerID].Contains(current9))
								{
									dictionary2[current9.PlayerID].Add(current9);
								}
							}
						}
					}
				}
			}
			if (this._trapCombatData.IsTrapCombat)
			{
				List<FleetInfo> list5 = new List<FleetInfo>();
				foreach (KeyValuePair<int, List<FleetInfo>> current10 in dictionary2)
				{
					list5.AddRange(current10.Value);
				}
				foreach (FleetInfo current11 in list5)
				{
					if (current11 != null && current11.IsNormalFleet)
					{
						PlayerInfo pi = base.App.GameDatabase.GetPlayerInfo(current11.PlayerID);
						if (pi != null && pi.isStandardPlayer)
						{
							Faction faction2 = base.App.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.ID == pi.FactionID);
							if (faction2 != null && !(faction2.Name == "morrigi"))
							{
								MissionInfo missionByFleetID2 = base.App.GameDatabase.GetMissionByFleetID(current11.ID);
								if (missionByFleetID2 != null && missionByFleetID2.Type == MissionType.COLONIZATION)
								{
									ColonyTrapInfo colonyTrapInfoByPlanetID = base.App.GameDatabase.GetColonyTrapInfoByPlanetID(missionByFleetID2.TargetOrbitalObjectID);
									if (colonyTrapInfoByPlanetID != null)
									{
										FleetInfo fleetInfo3 = base.App.GameDatabase.GetFleetInfo(colonyTrapInfoByPlanetID.FleetID);
										if (fleetInfo3 != null && base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo3.PlayerID, current11.PlayerID) != DiplomacyState.ALLIED)
										{
											if (!dictionary2.ContainsKey(fleetInfo3.PlayerID))
											{
												dictionary2[fleetInfo3.PlayerID] = new List<FleetInfo>(new FleetInfo[]
												{
													fleetInfo3
												});
											}
											else
											{
												if (!dictionary2[fleetInfo3.PlayerID].Contains(fleetInfo3))
												{
													dictionary2[fleetInfo3.PlayerID].Add(fleetInfo3);
												}
											}
											if (!this._trapCombatData.TrapFleets.Contains(fleetInfo3))
											{
												this._trapCombatData.TrapFleets.Add(fleetInfo3);
												this._trapCombatData.TrapToPlanet.Add(fleetInfo3.ID, colonyTrapInfoByPlanetID.PlanetID);
												if (!this._trapCombatData.TrapPlayers.Contains(fleetInfo3.PlayerID))
												{
													this._trapCombatData.TrapPlayers.Add(fleetInfo3.PlayerID);
												}
											}
											if (!this._trapCombatData.ColonyTrappedFleets.Contains(current11))
											{
												this._trapCombatData.ColonyTrappedFleets.Add(current11);
												this._trapCombatData.ColonyFleetToTrap.Add(current11.ID, fleetInfo3.ID);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			int encounterIDAtSystem = base.App.GameDatabase.GetEncounterIDAtSystem(EasterEgg.EE_ASTEROID_MONITOR, this._systemId);
			List<MorrigiRelicInfo> source = base.App.GameDatabase.GetMorrigiRelicInfos().ToList<MorrigiRelicInfo>();
			AsteroidMonitorInfo asteroidMonitorInfo = base.App.GameDatabase.GetAsteroidMonitorInfo(encounterIDAtSystem);
			MorrigiRelicInfo mri = source.FirstOrDefault((MorrigiRelicInfo x) => x.SystemId == this._systemId);
			this.RebuildInitialDiploStates(asteroidMonitorInfo, mri);
			Dictionary<Player, Dictionary<Player, PlayerCombatDiplomacy>> dictionary3 = new Dictionary<Player, Dictionary<Player, PlayerCombatDiplomacy>>();
			foreach (Player current12 in this._playersInCombat)
			{
				dictionary3.Add(current12, new Dictionary<Player, PlayerCombatDiplomacy>());
				foreach (Player current13 in this._playersInCombat)
				{
					if (current13 != current12)
					{
						if (!this.EncounterIsNeutral(current12.ID, current13.ID, asteroidMonitorInfo, mri))
						{
							DiplomacyState diplomacyState = this.GetDiplomacyState(current12.ID, current13.ID);
							if (diplomacyState == DiplomacyState.WAR)
							{
								dictionary3[current12].Add(current13, PlayerCombatDiplomacy.War);
							}
							else
							{
								if (diplomacyState == DiplomacyState.ALLIED)
								{
									dictionary3[current12].Add(current13, PlayerCombatDiplomacy.Allied);
								}
								else
								{
									dictionary3[current12].Add(current13, PlayerCombatDiplomacy.Neutral);
								}
							}
						}
						else
						{
							dictionary3[current12].Add(current13, PlayerCombatDiplomacy.Neutral);
						}
					}
				}
			}
			this.IdentifyEntryPointLocations(dictionary2);
			if (base.App.Game.ScriptModules.Pirates != null && !this._pirateEncounterData.IsPirateEncounter)
			{
				this._pirateEncounterData.IsPirateEncounter = dictionary2.Keys.Any((int x) => x == base.App.Game.ScriptModules.Pirates.PlayerID);
				if (this._pirateEncounterData.IsPirateEncounter)
				{
					this._pirateEncounterData.PiratePlayerIDs.Add(base.App.Game.ScriptModules.Pirates.PlayerID);
				}
			}
			this._ignoreEncounterSpawnPos = false;
			foreach (Player current14 in (
				from x in this._playersInCombat
				where x.IsStandardPlayer
				select x).ToList<Player>())
			{
				foreach (Player current15 in (
					from x in this._playersInCombat
					where x.IsStandardPlayer
					select x).ToList<Player>())
				{
					if (current14 != current15)
					{
						DiplomacyState diplomacyState2 = this.GetDiplomacyState(current14.ID, current15.ID);
						if (diplomacyState2 == DiplomacyState.WAR || diplomacyState2 != DiplomacyState.ALLIED)
						{
							this._ignoreEncounterSpawnPos = true;
							break;
						}
					}
				}
			}
			OrbitalObjectInfo[] objects = base.App.GameDatabase.GetStarSystemOrbitalObjectInfos(this._systemId).ToArray<OrbitalObjectInfo>();
			int[] first = this.CreateShips(this._crits, this._systemId, shipOrbitParentId, dictionary2, objects).ToArray<int>();
			if (this._pirateEncounterData.IsPirateEncounter)
			{
				first.Concat(this.SpawnPiracyEncounterShips(this._crits, objects).ToArray<int>());
			}
			if (CommonCombatState.RetainCombatConfig)
			{
				this._combatConfig = xmlDocument;
			}
			if (xmlDocument != null)
			{
				if (CommonCombatState.RetainCombatConfig)
				{
					this._gameObjectConfigs = dictionary;
				}
				this._crits.Add(dictionary.Keys);
			}
			List<BattleRiderSquad> list6 = new List<BattleRiderSquad>();
			List<Ship> list7 = new List<Ship>();
			List<IGameObject> list8 = new List<IGameObject>();
			Dictionary<int, List<Ship>> dictionary4 = new Dictionary<int, List<Ship>>();
			foreach (IGameObject current16 in 
				from x in this.Objects
				where x is Ship
				select x)
			{
				Ship ship = current16 as Ship;
				if (ship != null)
				{
					if (dictionary4.ContainsKey(ship.Player.ID))
					{
						dictionary4[ship.Player.ID].Add(ship);
					}
					else
					{
						dictionary4.Add(ship.Player.ID, new List<Ship>
						{
							ship
						});
					}
					if (ship.IsCarrier)
					{
						foreach (BattleRiderSquad current17 in ship.BattleRiderSquads)
						{
							list6.Add(current17);
						}
						list7.Add(ship);
					}
					if (ship.IsBattleRider)
					{
						list8.Add(current16);
					}
				}
			}
			foreach (KeyValuePair<int, List<Ship>> current18 in dictionary4)
			{
				Dictionary<ShipFleetAbilityType, List<Ship>> dictionary5 = new Dictionary<ShipFleetAbilityType, List<Ship>>();
				foreach (Ship current19 in current18.Value)
				{
					if (current19.AbilityType != ShipFleetAbilityType.None)
					{
						if (dictionary5.ContainsKey(current19.AbilityType))
						{
							dictionary5[current19.AbilityType].Add(current19);
						}
						else
						{
							dictionary5.Add(current19.AbilityType, new List<Ship>
							{
								current19
							});
						}
					}
				}
				foreach (KeyValuePair<ShipFleetAbilityType, List<Ship>> current20 in dictionary5)
				{
					this.CreateShipAbility(current20.Key, current20.Value, current18.Value);
				}
			}
			int num = 0;
			int num2 = 0;
			using (List<IGameObject>.Enumerator enumerator14 = list8.GetEnumerator())
			{
				Ship battleRider;
				while (enumerator14.MoveNext())
				{
					battleRider = (Ship)enumerator14.Current;
					if (battleRider.ParentDatabaseID != 0)
					{
						Ship ship2 = list7.FirstOrDefault((Ship x) => x.DatabaseID == battleRider.ParentDatabaseID);
						if (ship2 != null)
						{
							BattleRiderSquad battleRiderSquad = (ship2 != null) ? ship2.AssignRiderToSquad(battleRider as BattleRiderShip, battleRider.RiderIndex) : null;
							if (battleRiderSquad != null)
							{
								battleRider.ParentID = ship2.ObjectID;
								battleRider.PostSetBattleRiderParent(battleRiderSquad.ObjectID);
							}
						}
					}
					else
					{
						int num3 = 0;
						foreach (BattleRiderSquad current21 in list6)
						{
							if (num3 == num2 && num < current21.NumRiders)
							{
								battleRider.ParentID = current21.ParentID;
								battleRider.PostSetBattleRiderParent(current21.ObjectID);
								num++;
								if (num >= current21.NumRiders)
								{
									num = 0;
									num2++;
									break;
								}
								break;
							}
							else
							{
								num3++;
							}
						}
					}
				}
			}
			foreach (IGameObject current22 in this.Objects)
			{
				if (current22 is Ship)
				{
					Ship ship3 = current22 as Ship;
					this.AddAItoCombat(ship3);
				}
			}
			float num4 = base.App.GameSetup.CombatTurnLength;
			if (this._pirateEncounterData.IsPirateEncounter && this._pirateEncounterData.PirateBase == null)
			{
				num4 = 2.5f;
			}
			else
			{
				if (base.App.Game != null && base.App.Game.ScriptModules != null && !this._playersInCombat.Any((Player x) => x.ID == base.App.LocalPlayer.ID))
				{
					int num5 = (
						from x in this._playersInCombat
						where x.IsStandardPlayer && x.ID != base.App.LocalPlayer.ID
						select x).Count<Player>();
					int num6 = (
						from x in this._playersInCombat
						where base.App.Game.ScriptModules.IsEncounterPlayer(x.ID)
						select x).Count<Player>();
					if (num5 == 1 && num6 > 0)
					{
						num4 = Math.Min(num4, 5f);
					}
				}
			}
			this._mediaHeroFleets.Clear();
			foreach (KeyValuePair<int, List<FleetInfo>> current23 in dictionary2)
			{
				foreach (FleetInfo fleet in current23.Value)
				{
					if (fleet != null)
					{
						List<AdmiralInfo.TraitType> list9 = base.App.GameDatabase.GetAdmiralTraits(fleet.AdmiralID).ToList<AdmiralInfo.TraitType>();
						if (list9.Contains(AdmiralInfo.TraitType.GloryHound))
						{
							foreach (StellarBody current24 in 
								from x in this._starSystemObjects.GetPlanetsInSystem()
								where x.Parameters.ColonyPlayerID == fleet.PlayerID
								select x)
							{
								current24.PostSetProp("SetPlanetDamageModifier", 1.5f);
							}
						}
						if (list9.Contains(AdmiralInfo.TraitType.MediaHero))
						{
							this._mediaHeroFleets.Add(fleet);
						}
					}
				}
			}
			this._combat = Combat.Create(base.App, this._camera, this._input, this._sensor, this._starSystemObjects, this._grid, this._origin, radius, (int)(num4 * 60000f), this._playersInCombat.ToArray(), dictionary3, this.SimMode);
			this._combat.PostObjectAddObjects(this._crits.ToArray<IGameObject>());
			this._combat.PostSetProp("SetSlewModeMultipliers", new object[]
			{
				base.App.AssetDatabase.SlewModeMultiplier,
				base.App.AssetDatabase.SlewModeDecelMultiplier
			});
			this._crits.Add(this._combat);
			this.PopulateStarSystemAcceleratorHoops();
			this.PopulateCombatAsteroidBeltJammers();
			this.InitializePlayerCombatZonesToIgnore();
			if (this._sim)
			{
				List<object> list10 = new List<object>();
				list10.Add(true);
				list10.Add(false);
				foreach (StellarBody current25 in this._starSystemObjects.Crits.OfType<StellarBody>().ToList<StellarBody>())
				{
					current25.PostSetProp("SetSensorMode", list10.ToArray());
				}
			}
		}
		private void PopulateCombatAsteroidBeltJammers()
		{
			if (this._starSystemObjects == null || this._starSystemObjects.AsteroidBelts.Count == 0)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(this._starSystemObjects.AsteroidBelts.Count);
			foreach (AsteroidBelt current in this._starSystemObjects.AsteroidBelts)
			{
				list.Add(current.ObjectID);
			}
			this._combat.PostSetProp("PopulateAsteroidBeltJammers", list.ToArray());
		}
		private void InitializePlayerCombatZonesToIgnore()
		{
			if (this._starSystemObjects == null)
			{
				return;
			}
			foreach (Player current in this._playersInCombat)
			{
				if (!current.IsStandardPlayer || (this._pirateEncounterData.IsPirateEncounter && this._pirateEncounterData.PiratePlayerIDs.Contains(current.ID)))
				{
					this._ignoreCombatZonePlayers.Add(current);
				}
			}
			if (this._ignoreCombatZonePlayers.Count > 0)
			{
				List<object> list = new List<object>();
				list.Add(this._ignoreCombatZonePlayers.Count);
				foreach (Player current2 in this._ignoreCombatZonePlayers)
				{
					list.Add(current2.ObjectID);
				}
				this._starSystemObjects.PostSetProp("SetPlayerZoneColorsToIgnore", list.ToArray());
			}
		}
		private void PopulateStarSystemAcceleratorHoops()
		{
			if (this._starSystemObjects == null)
			{
				return;
			}
			List<Ship> list = this._crits.OfType<Ship>().Where(delegate(Ship x)
			{
				if (x.IsAcceleratorHoop && x.WeaponBanks != null)
				{
					return x.WeaponBanks.Any((WeaponBank y) => y.Weapon.PayloadType == WeaponEnums.PayloadTypes.MegaBeam);
				}
				return false;
			}).ToList<Ship>();
			if (list.Count > 0)
			{
				List<object> list2 = new List<object>();
				list2.Add(list.Count);
				foreach (Ship current in list)
				{
					list2.Add(current.ObjectID);
					list2.Add((
						from x in current.WeaponBanks
						select x.Weapon).First((LogicalWeapon x) => x.PayloadType == WeaponEnums.PayloadTypes.MegaBeam).UniqueWeaponID);
					list2.Add(current.Position);
				}
				this._starSystemObjects.PostSetProp("SetAcceleratorHoopLocationsCombat", list2.ToArray());
			}
		}
		private void Game_ObjectReleased(IGameObject obj)
		{
			Ship ship = obj as Ship;
			if (ship != null && this._ships.Reverse.ContainsKey(ship))
			{
				this._ships.Remove(this._ships.Reverse[ship], ship);
			}
		}
		protected abstract void OnCombatEnding();
		protected abstract void SyncPlayerList();
		public Ship GetShipCompoundByObjectID(int objectID)
		{
			return (Ship)base.App.GetGameObject(objectID);
		}
		private void IdentifyEntryPointLocations(IDictionary<int, List<FleetInfo>> selectedPlayerFleets)
		{
			this._entryLocations = new List<EntrySpawnLocation>();
			foreach (List<FleetInfo> current in selectedPlayerFleets.Values)
			{
				foreach (FleetInfo fleet in current)
				{
					if (fleet != null && fleet.Type != FleetType.FL_DEFENSE && fleet.Type == FleetType.FL_NORMAL && !(fleet.PreviousSystemID == this._systemId) && fleet.PreviousSystemID.HasValue)
					{
						CombatZonePositionInfo enteryZoneForOuterSystem = this._starSystemObjects.GetEnteryZoneForOuterSystem(fleet.PreviousSystemID.Value);
						if (enteryZoneForOuterSystem != null)
						{
							Player player = base.App.GetPlayer(fleet.PlayerID);
							Faction faction = player.Faction;
							if (!this._entryLocations.Any((EntrySpawnLocation x) => x.PreviousSystemID == fleet.PreviousSystemID.Value && x.FactionID == faction.ID))
							{
								Vector3 starSystemOrigin = base.App.GameDatabase.GetStarSystemOrigin(this._systemId);
								Vector3 starSystemOrigin2 = base.App.GameDatabase.GetStarSystemOrigin(fleet.PreviousSystemID.Value);
								Vector3 value = starSystemOrigin2 - starSystemOrigin;
								value.Y = 0f;
								EntrySpawnLocation entrySpawnLocation = new EntrySpawnLocation();
								entrySpawnLocation.FactionID = faction.ID;
								entrySpawnLocation.PreviousSystemID = fleet.PreviousSystemID.Value;
								entrySpawnLocation.Position = Vector3.Normalize(value) * enteryZoneForOuterSystem.RadiusLower;
								this._entryLocations.Add(entrySpawnLocation);
							}
						}
					}
				}
			}
		}
		private IEnumerable<int> CreateShips(GameObjectSet crits, int systemId, int shipOrbitParentId, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, OrbitalObjectInfo[] objects)
		{
			if (systemId != 0 || shipOrbitParentId != 0)
			{
				List<CombatEasterEggData> combatEasterEggData = this.GetCombatEasterEggData(objects, selectedPlayerFleets);
				List<CombatRandomData> randData = new List<CombatRandomData>();
				if (combatEasterEggData.Count == 0)
				{
					randData = this.GetCombatRandomData(objects, selectedPlayerFleets);
				}
				this.InitializeNeutralCombatStanceInfo(selectedPlayerFleets);
				foreach (List<FleetInfo> current in selectedPlayerFleets.Values)
				{
					int num = 0;
					foreach (FleetInfo current2 in current)
					{
						if (current2 != null && current2.Type != FleetType.FL_RESERVE && (base.App.Game.ScriptModules.VonNeumann == null || base.App.Game.ScriptModules.VonNeumann.PlayerID != current2.PlayerID || base.App.Game.ScriptModules.VonNeumann.HomeWorldSystemID != this._systemId || base.App.Game.ScriptModules.VonNeumann.CanSpawnFleetAtHomeWorld(current2)))
						{
							FleetType type = current2.Type;
							if (type != FleetType.FL_DEFENSE)
							{
								if (type != FleetType.FL_GATE)
								{
									if (type != FleetType.FL_ACCELERATOR)
									{
										foreach (int current3 in this.SpawnFleet(crits, current2, combatEasterEggData, randData, selectedPlayerFleets, objects, num))
										{
											yield return current3;
										}
									}
									else
									{
										foreach (int current4 in this.SpawnAcceleratorFleet(crits, current2))
										{
											yield return current4;
										}
									}
								}
								else
								{
									foreach (int current5 in this.SpawnGateFleet(crits, current2))
									{
										yield return current5;
									}
								}
							}
							else
							{
								foreach (int current6 in this.SpawnDefenseFleet(crits, current2))
								{
									yield return current6;
								}
							}
							num++;
						}
					}
				}
				this.CreateAllPointsOfInterest(crits, objects, combatEasterEggData);
			}
			yield break;
		}
		private int GetNumFreighters(int max)
		{
			int i;
			for (i = 1; i < max; i++)
			{
				if (i == 1)
				{
					if (!this._random.CoinToss(50))
					{
						break;
					}
				}
				else
				{
					if (i == 2)
					{
						if (!this._random.CoinToss(20))
						{
							break;
						}
					}
					else
					{
						if (!this._random.CoinToss(10))
						{
							break;
						}
					}
				}
			}
			return i;
		}
		private IEnumerable<int> SpawnPiracyEncounterShips(GameObjectSet crits, OrbitalObjectInfo[] objects)
		{
			List<StellarBody> list = this._starSystemObjects.Crits.Objects.OfType<StellarBody>().ToList<StellarBody>();
			List<StarModel> list2 = this._starSystemObjects.Crits.Objects.OfType<StarModel>().ToList<StarModel>();
			List<StellarBody> list3 = new List<StellarBody>();
			float num = 0f;
			foreach (StellarBody current in list)
			{
				if (current.Population > 0.0)
				{
					list3.Add(current);
				}
				Vector3 position = current.Parameters.Position;
				float lengthSquared = position.LengthSquared;
				if (lengthSquared > num)
				{
					num = lengthSquared;
				}
			}
			if (this._starSystemObjects.CombatZones.Count > 0)
			{
				CombatZonePositionInfo combatZonePositionInfo = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
				num = (combatZonePositionInfo.RadiusLower + combatZonePositionInfo.RadiusUpper) * 0.5f;
			}
			else
			{
				if (num > 0f)
				{
					num = (float)Math.Sqrt((double)num);
				}
				else
				{
					if (list2.Count > 0)
					{
						num = list2.First<StarModel>().Radius + 7500f + 10000f;
					}
					else
					{
						num = 50000f;
					}
				}
			}
			Dictionary<int, List<PlayerTechInfo>> dictionary = new Dictionary<int, List<PlayerTechInfo>>();
			Dictionary<int, List<FreighterInfo>> dictionary2 = new Dictionary<int, List<FreighterInfo>>();
			Dictionary<int, StationInfo> dictionary3 = new Dictionary<int, StationInfo>();
			Dictionary<int, int> dictionary4 = new Dictionary<int, int>();
			TradeResultsTable tradeResultsTable = base.App.GameDatabase.GetTradeResultsTable();
			List<FreighterInfo> source = base.App.GameDatabase.GetFreighterInfosForSystem(this._systemId).ToList<FreighterInfo>();
			foreach (Player p in this._playersInCombat)
			{
				dictionary.Add(p.ID, (
					from x in base.App.GameDatabase.GetPlayerTechInfos(p.ID)
					where x.State == TechStates.Researched
					select x).ToList<PlayerTechInfo>());
				dictionary2.Add(p.ID, (
					from x in source
					where x.PlayerId == p.ID
					select x).ToList<FreighterInfo>());
				dictionary3.Add(p.ID, base.App.GameDatabase.GetCivilianStationForSystemAndPlayer(this._systemId, p.ID));
				int num2 = 0;
				if (tradeResultsTable != null && tradeResultsTable.TradeNodes.ContainsKey(this._systemId))
				{
					num2 += tradeResultsTable.TradeNodes[this._systemId].ImportInt;
					num2 += tradeResultsTable.TradeNodes[this._systemId].ImportProv;
					num2 += tradeResultsTable.TradeNodes[this._systemId].ImportLoc;
				}
				int numFreighters = this.GetNumFreighters(Math.Max((dictionary2[p.ID].Count + num2) * 60 / 100, 1));
				dictionary4.Add(p.ID, numFreighters);
			}
			List<Ship> list4 = new List<Ship>();
			List<Ship> list5 = new List<Ship>();
			foreach (Player current2 in this._playersInCombat)
			{
				if (dictionary2[current2.ID].Count != 0)
				{
					Vector3 vector = Vector3.Zero;
					float num3 = 0f;
					if (dictionary3[current2.ID] != null)
					{
						Vector3 position2 = base.App.GameDatabase.GetOrbitalTransform(dictionary3[current2.ID].OrbitalObjectID).Position;
						float num4 = 3.40282347E+38f;
						for (int i = 0; i < objects.Length; i++)
						{
							OrbitalObjectInfo orbitalObjectInfo = objects[i];
							if (orbitalObjectInfo.ID != dictionary3[current2.ID].OrbitalObjectID)
							{
								float lengthSquared2 = (base.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position - position2).LengthSquared;
								if (lengthSquared2 < num4)
								{
									num4 = lengthSquared2;
									num3 = lengthSquared2;
									vector = position2;
								}
							}
						}
						num3 = (float)Math.Sqrt((double)num3) + 500f;
					}
					else
					{
						double num5 = 0.0;
						for (int i = 0; i < objects.Length; i++)
						{
							OrbitalObjectInfo orbitalObjectInfo2 = objects[i];
							PlanetInfo planetInfo = base.App.GameDatabase.GetPlanetInfo(orbitalObjectInfo2.ID);
							if (planetInfo != null)
							{
								ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo2.ID);
								if (colonyInfoForPlanet != null)
								{
									double totalPopulation = base.App.GameDatabase.GetTotalPopulation(colonyInfoForPlanet);
									if (totalPopulation > num5)
									{
										num5 = totalPopulation;
										vector = base.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo2.ID).Position;
										num3 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 750f;
									}
								}
							}
						}
					}
					bool flag = dictionary[current2.ID].Any((PlayerTechInfo x) => x.TechFileID == "CCC_Convoy_Systems");
					int num6 = flag ? 2 : 4;
					num6 = Math.Min(num6, this._starSystemObjects.NeighboringSystems.Count);
					List<NeighboringSystemInfo> list6 = new List<NeighboringSystemInfo>();
					for (int j = 0; j < num6; j++)
					{
						NeighboringSystemInfo neighboringSystemInfo = null;
						float num7 = 3.40282347E+38f;
						foreach (NeighboringSystemInfo current3 in this._starSystemObjects.NeighboringSystems)
						{
							if (!list6.Contains(current3))
							{
								float lengthSquared3 = (current3.BaseOffsetLocation - vector).LengthSquared;
								if (lengthSquared3 < num7)
								{
									neighboringSystemInfo = current3;
									num7 = lengthSquared3;
								}
							}
						}
						if (neighboringSystemInfo != null)
						{
							list6.Add(neighboringSystemInfo);
						}
					}
					int num8 = dictionary4[current2.ID];
					foreach (FreighterInfo current4 in dictionary2[current2.ID])
					{
						ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(current4.ShipId, false);
						if (shipInfo != null)
						{
							Vector3 validFreighterSpawnPosition = this.GetValidFreighterSpawnPosition(list3, list, list2, list6, vector, num3, num, flag);
							Vector3 forward = Vector3.Normalize(-validFreighterSpawnPosition);
							Matrix world = Matrix.CreateWorld(validFreighterSpawnPosition, forward, Vector3.UnitY);
							Ship ship = Ship.CreateShip(base.App.Game, world, shipInfo, 0, this._input.ObjectID, current2.ObjectID, this._starSystemObjects.IsDeepSpace, null);
							NeighboringSystemInfo neighboringSystemInfo2 = null;
							if (list6.Count > 0)
							{
								float num9 = 3.40282347E+38f;
								foreach (NeighboringSystemInfo current5 in list6)
								{
									float lengthSquared4 = (validFreighterSpawnPosition - current5.BaseOffsetLocation).LengthSquared;
									if (lengthSquared4 < num9)
									{
										num9 = lengthSquared4;
										neighboringSystemInfo2 = current5;
									}
								}
							}
							ship.Maneuvering.RetreatData = this.GetRetreatData(current4.PlayerId, (neighboringSystemInfo2 != null) ? neighboringSystemInfo2.SystemID : 0, validFreighterSpawnPosition);
							crits.Add(ship);
							yield return ship.ObjectID;
							if (this._pirateEncounterData.PlayerFreightersInSystem.ContainsKey(current2.ID))
							{
								this._pirateEncounterData.PlayerFreightersInSystem[current2.ID].Add(new CommonCombatState.PiracyFreighterInfo
								{
									ShipID = shipInfo.ID,
									FreighterID = current4.ID
								});
							}
							else
							{
								this._pirateEncounterData.PlayerFreightersInSystem.Add(current2.ID, new List<CommonCombatState.PiracyFreighterInfo>
								{
									new CommonCombatState.PiracyFreighterInfo
									{
										ShipID = shipInfo.ID,
										FreighterID = current4.ID
									}
								});
							}
							if (ship.IsQShip)
							{
								list5.Add(ship);
							}
							else
							{
								list4.Add(ship);
							}
							num8--;
							if (num8 <= 0)
							{
								break;
							}
						}
					}
				}
			}
			if (list4.Count<Ship>() != 0 || list5.Count<Ship>() != 0)
			{
				if (list5.Count > 0 && list4.Count == 0)
				{
					list4.AddRange(list5);
					list5.Clear();
				}
				int num10 = 0;
				if (list4.Count > 0)
				{
					foreach (Ship current6 in this._pirateEncounterData.PoliceShipsInSystem)
					{
						Vector3 position3 = list4[num10].Position;
						float num11 = (float)this._random.NextInclusive(0.0, 6.2831853071795862);
						Vector3 vector2 = new Vector3
						{
							X = (float)Math.Sin((double)num11) * 1000f,
							Z = (float)(-(float)Math.Cos((double)num11)) * 1000f,
							Y = 0f
						} + position3;
						Matrix matrix = Matrix.CreateWorld(vector2, Vector3.Normalize(position3 - vector2), Vector3.UnitY);
						current6.InitialSetPos(matrix.Position, matrix.EulerAngles);
						num10 = (num10 + 1) % list4.Count;
					}
				}
				Dictionary<Ship, List<Ship>> dictionary5 = new Dictionary<Ship, List<Ship>>();
				Dictionary<Ship, List<Ship>> dictionary6 = new Dictionary<Ship, List<Ship>>();
				foreach (Ship current7 in list4)
				{
					if (!dictionary5.ContainsKey(current7))
					{
						bool flag2 = false;
						foreach (KeyValuePair<Ship, List<Ship>> current8 in dictionary5)
						{
							if (current8.Value.Contains(current7))
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							dictionary5.Add(current7, new List<Ship>());
							foreach (Ship current9 in list4)
							{
								if (current9 != current7 && (current9.Position - current7.Position).LengthSquared < 9000000f)
								{
									dictionary5[current7].Add(current9);
								}
							}
						}
					}
				}
				int freighterPlayer = 0;
				int num12 = -2147483648;
				Vector3 vector3 = default(Vector3);
				Vector3? vector4 = null;
				foreach (KeyValuePair<Ship, List<Ship>> current10 in dictionary5)
				{
					int num13 = 1 + current10.Value.Count;
					Vector3 vector5 = current10.Key.Position;
					foreach (Ship current11 in current10.Value)
					{
						vector5 += current11.Position;
					}
					vector5 /= (float)num13;
					dictionary6.Add(current10.Key, new List<Ship>());
					int num14 = 0;
					foreach (Ship current12 in this._pirateEncounterData.PoliceShipsInSystem)
					{
						if ((vector5 - current12.Position).LengthSquared < 9000000f)
						{
							dictionary6[current10.Key].Add(current12);
							num14++;
						}
					}
					int num15 = num13 - num14;
					if (num15 > num12)
					{
						num12 = num15;
						vector3 = vector5;
						freighterPlayer = current10.Key.Player.ID;
						vector4 = new Vector3?(current10.Key.Maneuvering.RetreatDestination);
					}
				}
				Vector3 retreatDestination = vector4.HasValue ? vector4.Value : (Vector3.Normalize(vector3) * num);
				foreach (Ship current13 in list5)
				{
					current13.InitialSetPos(this.GetValidNearFreighterSpawnPosition(list, list2, vector3, 750f, 2000f, num), Vector3.Zero);
					current13.Maneuvering.RetreatDestination = retreatDestination;
				}
				Vector3 v = default(Vector3);
				foreach (SpawnProfile current14 in this._spawnPositions)
				{
					if (this._pirateEncounterData.PiratePlayerIDs.Contains(current14._playerID))
					{
						v = current14._spawnPosition;
						current14._spawnPosition = this.GetValidNearFreighterSpawnPosition(list, list2, vector3, 5000f, 5000f, num);
						current14._spawnFacing = Vector3.Normalize(vector3 - current14._spawnPosition);
						current14._startingPosition = current14._spawnPosition;
						int systemID = 0;
						FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(current14._fleetID);
						if (fleetInfo != null && fleetInfo.PreviousSystemID.HasValue)
						{
							systemID = fleetInfo.PreviousSystemID.Value;
						}
						RetreatData retreatData = this.GetRetreatData(current14._playerID, systemID, current14._spawnPosition);
						current14._retreatPosition = retreatData.DefaultDestination;
						foreach (KeyValuePair<int, List<Ship>> current15 in this._pirateEncounterData.PirateShipsInSystem)
						{
							if (current15.Key == current14._playerID)
							{
								foreach (Ship current16 in current15.Value)
								{
									Matrix matrix2 = Matrix.CreateWorld(current16.Position - v + current14._spawnPosition, current14._spawnFacing, Vector3.UnitY);
									current16.InitialSetPos(matrix2.Position, matrix2.EulerAngles);
									current16.Maneuvering.RetreatData = retreatData;
								}
							}
						}
					}
				}
				SpawnProfile spawnProfile = this._spawnPositions.FirstOrDefault((SpawnProfile x) => x._playerID == freighterPlayer);
				if (spawnProfile != null)
				{
					FleetInfo fleetInfo2 = base.App.GameDatabase.GetFleetInfo(spawnProfile._fleetID);
					if (fleetInfo2 != null)
					{
						AdmiralInfo admiralInfo = base.App.GameDatabase.GetAdmiralInfo(fleetInfo2.AdmiralID);
						List<AdmiralInfo.TraitType> list7 = base.App.GameDatabase.GetAdmiralTraits(fleetInfo2.AdmiralID).ToList<AdmiralInfo.TraitType>();
						if (admiralInfo != null && this._random.CoinToss((list7.Contains(AdmiralInfo.TraitType.Vigilant) ? 2 : 1) * admiralInfo.ReactionBonus))
						{
							spawnProfile._startingPosition = this.GetValidNearFreighterSpawnPosition(list, list2, vector3, 3000f, 3000f, num);
							spawnProfile._spawnPosition = spawnProfile._startingPosition;
							spawnProfile._spawnFacing = Vector3.Normalize(vector3 - spawnProfile._startingPosition);
							RetreatData retreatData2 = this.GetRetreatData(fleetInfo2.PlayerID, fleetInfo2.PreviousSystemID.HasValue ? fleetInfo2.PreviousSystemID.Value : 0, spawnProfile._startingPosition);
							spawnProfile._retreatPosition = retreatData2.DefaultDestination;
							Matrix mat = Matrix.CreateWorld(spawnProfile._spawnPosition, spawnProfile._spawnFacing, Vector3.UnitY);
							int num16 = 0;
							Vector3 vector6 = spawnProfile._startingPosition;
							Vector3 v2 = new Vector3(spawnProfile._spawnFacing.Z, 0f, spawnProfile._spawnFacing.X * -1f) * 600f;
							foreach (int current17 in spawnProfile._activeShips)
							{
								if (this._ships.Forward.ContainsKey(current17))
								{
									Ship ship2 = this._ships.Forward[current17];
									Vector3? shipFleetPosition = base.App.GameDatabase.GetShipFleetPosition(ship2.DatabaseID);
									if (!ShipSectionAsset.IsBattleRiderClass(ship2.RealShipClass) || ship2.RiderIndex < 0)
									{
										if (shipFleetPosition.HasValue)
										{
											vector6 = Vector3.Transform(shipFleetPosition.Value, mat);
										}
										else
										{
											if (num16 > 0)
											{
												vector6 += v2;
											}
										}
										Matrix matrix3 = Matrix.CreateWorld(vector6, spawnProfile._spawnFacing, Vector3.UnitY);
										ship2.InitialSetPos(matrix3.Position, matrix3.EulerAngles);
									}
								}
								num16++;
							}
						}
					}
				}
			}
			yield break;
		}
		private Vector3 GetValidFreighterSpawnPosition(List<StellarBody> populatedPlanets, List<StellarBody> planets, List<StarModel> stars, List<NeighboringSystemInfo> closestSystems, Vector3 baseCenter, float baseOffset, float furthestOffset, bool hasConvoySystems)
		{
			float num = hasConvoySystems ? 20000f : 15000f;
			Vector3 vector = Vector3.Zero;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				if (closestSystems.Count > 0)
				{
					int index = this._random.NextInclusive(0, closestSystems.Count - 1);
					NeighboringSystemInfo neighboringSystemInfo = closestSystems[index];
					Vector3 v = neighboringSystemInfo.BaseOffsetLocation - baseCenter;
					float num2 = v.Normalize();
					float num3;
					if (hasConvoySystems)
					{
						num3 = this._random.NextInclusive(baseOffset, Math.Min(num, num2));
					}
					else
					{
						num3 = this._random.NextInclusive(num, num2);
					}
					vector = baseCenter + v * num3;
				}
				else
				{
					if (hasConvoySystems)
					{
						float num4 = (float)this._random.NextInclusive(0.0, 6.2831853071795862);
						float num3 = this._random.NextInclusive(0f, num);
						int index2 = this._random.NextInclusive(0, populatedPlanets.Count - 1);
						vector.X = (float)Math.Sin((double)num4) * num3;
						vector.Z = (float)(-(float)Math.Cos((double)num4)) * num3;
						vector.Y = 0f;
						vector += populatedPlanets[index2].Parameters.Position;
					}
					else
					{
						float num4 = (float)this._random.NextInclusive(0.0, 6.2831853071795862);
						float num3 = this._random.NextInclusive(0f, furthestOffset);
						vector.X = (float)Math.Sin((double)num4) * num3;
						vector.Z = (float)(-(float)Math.Cos((double)num4)) * num3;
						vector.Y = 0f;
					}
				}
				if (vector.LengthSquared > furthestOffset * furthestOffset)
				{
					flag = false;
				}
				if (flag)
				{
					foreach (StarModel current in stars)
					{
						float num5 = current.Radius + 7500f;
						if ((vector - current.Position).LengthSquared < num5 * num5)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					foreach (StellarBody current2 in planets)
					{
						float num6 = current2.Parameters.Radius + 750f;
						if (current2.Population > 0.0 && !hasConvoySystems)
						{
							num6 += num;
						}
						if ((vector - current2.Parameters.Position).LengthSquared < num6 * num6)
						{
							flag = false;
							break;
						}
					}
				}
			}
			return vector;
		}
		private Vector3 GetValidNearFreighterSpawnPosition(List<StellarBody> planets, List<StarModel> stars, Vector3 pirateAttackCenter, float minOffset, float maxOffset, float systemBoundary)
		{
			Vector3 vector = Vector3.Zero;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				float num = (float)this._random.NextInclusive(0.0, 6.2831853071795862);
				float num2 = this._random.NextInclusive(minOffset, maxOffset);
				vector.X = (float)Math.Sin((double)num) * num2;
				vector.Z = (float)(-(float)Math.Cos((double)num)) * num2;
				vector.Y = 0f;
				vector += pirateAttackCenter;
				if (vector.LengthSquared > systemBoundary * systemBoundary)
				{
					flag = false;
				}
				if (flag)
				{
					foreach (StarModel current in stars)
					{
						float num3 = current.Radius + 7500f;
						if ((vector - current.Position).LengthSquared < num3 * num3)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					foreach (StellarBody current2 in planets)
					{
						float num4 = current2.Parameters.Radius + 750f;
						if ((vector - current2.Parameters.Position).LengthSquared < num4 * num4)
						{
							flag = false;
							break;
						}
					}
				}
			}
			return vector;
		}
		private IEnumerable<int> SpawnFleet(GameObjectSet crits, FleetInfo fleet, List<CombatEasterEggData> eeData, List<CombatRandomData> randData, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, OrbitalObjectInfo[] objects, int spawnedFleetCount)
		{
			List<Ship> list = new List<Ship>();
			List<FleetFormationCreationData> list2 = new List<FleetFormationCreationData>();
			bool stratModifier = base.App.GetStratModifier<bool>(StratModifiers.AllowPoliceInCombat, fleet.PlayerID);
			SpawnProfile spawnProfileForFleet = this.GetSpawnProfileForFleet(fleet.ID, selectedPlayerFleets, eeData, randData, ref spawnedFleetCount);
			Matrix matrix = Matrix.CreateWorld(spawnProfileForFleet._startingPosition, spawnProfileForFleet._spawnFacing, Vector3.UnitY);
			RetreatData retreatData = this.GetRetreatData(fleet.PlayerID, fleet.PreviousSystemID.HasValue ? fleet.PreviousSystemID.Value : 0, spawnProfileForFleet._startingPosition);
			spawnProfileForFleet._retreatPosition = retreatData.DefaultDestination;
			Vector3 vector = spawnProfileForFleet._startingPosition;
			Vector3 v = new Vector3(spawnProfileForFleet._spawnFacing.Z, 0f, spawnProfileForFleet._spawnFacing.X * -1f) * 600f;
			FleetFormationCreationData item = default(FleetFormationCreationData);
			item.FormationData = new List<FormationCreationData>();
			new List<ShipInfo>();
			List<ShipInfo> list3 = base.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			foreach (ShipInfo current in list3.ToList<ShipInfo>())
			{
				foreach (ShipInfo rider in base.App.GameDatabase.GetBattleRidersByParentID(current.ID))
				{
					if (!list3.Any((ShipInfo x) => x.ID == rider.ID))
					{
						list3.Add(rider);
					}
				}
			}
			Player player = base.App.GetPlayer(fleet.PlayerID);
			int playerId = (player != null) ? player.ObjectID : 0;
			bool flag = false;
			int num = 0;
			bool flag2 = false;
			foreach (ShipInfo current2 in list3)
			{
				bool flag3 = false;
				IEnumerable<string> designSectionNames = base.App.GameDatabase.GetDesignSectionNames(current2.DesignID);
				RealShipClasses shipClass = RealShipClasses.Cruiser;
				SectionEnumerations.CombatAiType combatAiType = SectionEnumerations.CombatAiType.Normal;
				foreach (string s in designSectionNames)
				{
					ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == s);
					if (shipSectionAsset != null)
					{
						if (shipSectionAsset.isPolice)
						{
							flag3 = true;
						}
						if (shipSectionAsset.Type == ShipSectionType.Mission)
						{
							combatAiType = shipSectionAsset.CombatAIType;
							ShipClass arg_3B8_0 = shipSectionAsset.Class;
							shipClass = shipSectionAsset.RealClass;
							bool arg_3CB_0 = shipSectionAsset.isMineLayer;
						}
					}
				}
				if ((!ShipSectionAsset.IsBattleRiderClass(shipClass) || current2.RiderIndex >= 0 || fleet.IsTrapFleet) && (fleet.Type == FleetType.FL_DEFENSE || (!current2.DesignInfo.IsSDB() && !current2.DesignInfo.IsPlatform())) && !current2.DesignInfo.IsLoaCube())
				{
					Matrix matrix2 = Matrix.Identity;
					Vector3? shipFleetPosition = base.App.GameDatabase.GetShipFleetPosition(current2.ID);
					flag2 = (flag2 || shipFleetPosition.HasValue);
					if (!dictionary.ContainsKey(current2.ID))
					{
						dictionary.Add(current2.ID, false);
					}
					switch (combatAiType)
					{
					case SectionEnumerations.CombatAiType.SystemKiller:
					{
						Matrix? previousShipTransform = base.App.Game.MCCarryOverData.GetPreviousShipTransform(this._systemId, fleet.ID, current2.ID);
						if (previousShipTransform.HasValue)
						{
							matrix2 = previousShipTransform.Value;
						}
						else
						{
							matrix2 = SystemKiller.GetSpawnTransform(base.App, fleet.ID, this._starSystemObjects, objects);
						}
						break;
					}
					case SectionEnumerations.CombatAiType.MorrigiRelic:
					{
						Matrix? shipSystemPosition = base.App.GameDatabase.GetShipSystemPosition(current2.ID);
						if (!shipSystemPosition.HasValue)
						{
							matrix2 = MorrigiRelic.GetSpawnTransform(base.App, this._random, num, objects);
							base.App.GameDatabase.UpdateShipSystemPosition(current2.ID, new Matrix?(matrix2));
						}
						else
						{
							matrix2 = shipSystemPosition.Value;
						}
						break;
					}
					case SectionEnumerations.CombatAiType.MorrigiCrow:
					case SectionEnumerations.CombatAiType.Specter:
						goto IL_7C4;
					case SectionEnumerations.CombatAiType.Meteor:
					{
						Matrix? shipSystemPosition2 = base.App.GameDatabase.GetShipSystemPosition(current2.ID);
						if (!shipSystemPosition2.HasValue)
						{
							matrix2 = MeteorShower.GetSpawnTransform(base.App, this._systemId, 0);
						}
						else
						{
							matrix2 = shipSystemPosition2.Value;
						}
						break;
					}
					case SectionEnumerations.CombatAiType.Comet:
						matrix2 = Comet.GetSpawnTransform(base.App, this._starSystemObjects, objects);
						break;
					case SectionEnumerations.CombatAiType.Gardener:
					case SectionEnumerations.CombatAiType.Protean:
						matrix2 = Gardeners.GetSpawnTransform(base.App, current2.ID, fleet.ID, num, this._systemId, objects);
						break;
					case SectionEnumerations.CombatAiType.CommandMonitor:
					case SectionEnumerations.CombatAiType.NormalMonitor:
					{
						Matrix? shipSystemPosition3 = base.App.GameDatabase.GetShipSystemPosition(current2.ID);
						if (!shipSystemPosition3.HasValue)
						{
							matrix2 = AsteroidMonitor.GetSpawnTransform(base.App, current2.DesignID, num, list3.Count, this._systemId);
							base.App.GameDatabase.UpdateShipSystemPosition(current2.ID, new Matrix?(matrix2));
						}
						else
						{
							matrix2 = shipSystemPosition3.Value;
						}
						break;
					}
					default:
						goto IL_7C4;
					}
					IL_983:
					Ship ship = Ship.CreateShip(base.App.Game, matrix2, current2, 0, this._input.ObjectID, playerId, this._starSystemObjects.IsDeepSpace, this._playersInCombat);
					ship.ParentDatabaseID = current2.ParentID;
					ship.Maneuvering.RetreatData = retreatData;
					crits.Add(ship);
					if (this._pirateEncounterData.IsPirateEncounter)
					{
						if (flag3)
						{
							this._pirateEncounterData.PoliceShipsInSystem.Add(ship);
						}
						if (fleet.Type == FleetType.FL_NORMAL && this._pirateEncounterData.PiratePlayerIDs.Contains(fleet.PlayerID))
						{
							if (this._pirateEncounterData.PirateShipsInSystem.ContainsKey(fleet.PlayerID))
							{
								this._pirateEncounterData.PirateShipsInSystem[fleet.PlayerID].Add(ship);
							}
							else
							{
								this._pirateEncounterData.PirateShipsInSystem.Add(fleet.PlayerID, new List<Ship>
								{
									ship
								});
							}
							if (fleet.PlayerID != base.App.Game.ScriptModules.Pirates.PlayerID)
							{
								Player playerObject = base.App.Game.GetPlayerObject(base.App.Game.ScriptModules.Pirates.PlayerID);
								if (playerObject != null)
								{
									ship.PostSetProp("SetPiracyPlayer", playerObject.ObjectID);
								}
							}
						}
					}
					yield return ship.ObjectID;
					if (!flag3 || stratModifier)
					{
						this._ships.Insert(current2.ID, ship);
					}
					if (ship.Player == base.App.LocalPlayer)
					{
						if (shipFleetPosition.HasValue)
						{
							item.FormationData.Add(new FormationCreationData
							{
								ShipID = ship.ObjectID,
								DesignID = current2.DesignID,
								ShipRole = ship.ShipRole,
								ShipClass = ship.ShipClass,
								FormationPosition = shipFleetPosition.Value
							});
							flag = true;
						}
						if (ship.ShipClass != ShipClass.BattleRider && ship.ShipClass != ShipClass.Station)
						{
							list.Add(ship);
						}
					}
					num++;
					continue;
					IL_7C4:
					if (!ShipSectionAsset.IsBattleRiderClass(shipClass) || current2.RiderIndex < 0)
					{
						if (shipFleetPosition.HasValue)
						{
							vector = Vector3.Transform(shipFleetPosition.Value, matrix);
						}
						else
						{
							if (num > 0)
							{
								vector += v;
							}
						}
					}
					vector = CommonCombatState.ApplyOriginShift(this.Origin, vector);
					Matrix matrix3 = Matrix.CreateWorld(vector, spawnProfileForFleet._spawnFacing, Vector3.UnitY);
					matrix2 = matrix3;
					Matrix? matrix4 = base.App.GameDatabase.GetShipSystemPosition(current2.ID);
					if (matrix4.HasValue && !this._trapCombatData.ColonyTrappedFleets.Contains(fleet))
					{
						matrix2 = matrix4.Value;
					}
					dictionary[current2.ID] = (dictionary[current2.ID] || matrix4.HasValue);
					matrix4 = base.App.Game.MCCarryOverData.GetPreviousShipTransform(this._systemId, fleet.ID, current2.ID);
					if (matrix4.HasValue)
					{
						matrix2 = matrix4.Value;
					}
					dictionary[current2.ID] = (dictionary[current2.ID] || matrix4.HasValue);
					goto IL_983;
				}
			}
			if (!flag2 && player.Faction.Name == "loa")
			{
				List<FormationPatternData> list4 = FormationPatternCreator.CreateCubeFormationPattern(list, matrix);
				foreach (FormationPatternData current3 in list4)
				{
					if (current3.Ship != null)
					{
						item.FormationData.Add(new FormationCreationData
						{
							ShipID = current3.Ship.ObjectID,
							DesignID = current3.Ship.DesignID,
							ShipRole = current3.Ship.ShipRole,
							ShipClass = current3.Ship.ShipClass,
							FormationPosition = current3.Position
						});
						if (!dictionary[current3.Ship.DatabaseID])
						{
							Vector3 position = Vector3.Transform(current3.Position, matrix);
							Matrix matrix5 = Matrix.CreateWorld(position, spawnProfileForFleet._spawnFacing, Vector3.UnitY);
							current3.Ship.InitialSetPos(matrix5.Position, matrix5.EulerAngles);
						}
					}
				}
				if (player == base.App.LocalPlayer)
				{
					flag = true;
				}
			}
			if (flag)
			{
				list2.Add(item);
			}
			if (flag)
			{
				this.SetSelectionToGroup(list);
				this.PostNewPlayerFormation(list2);
			}
			yield break;
		}
		protected static void Trace(string message)
		{
			App.Log.Trace(message, "combat");
		}
		protected static void Warn(string message)
		{
			App.Log.Warn(message, "combat");
		}
		private IEnumerable<int> SpawnDefenseFleet(GameObjectSet crits, FleetInfo fleet)
		{
			if (fleet != null && fleet.Type == FleetType.FL_DEFENSE)
			{
				bool stratModifier = base.App.GetStratModifier<bool>(StratModifiers.AllowPoliceInCombat, fleet.PlayerID);
				List<ShipInfo> list = base.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo current in list.ToList<ShipInfo>())
				{
					foreach (ShipInfo rider in base.App.GameDatabase.GetBattleRidersByParentID(current.ID))
					{
						if (!list.Any((ShipInfo x) => x.ID == rider.ID))
						{
							list.Add(rider);
						}
					}
				}
				List<object> list2 = new List<object>();
				Player player = base.App.GetPlayer(fleet.PlayerID);
				int playerId = (player != null) ? player.ObjectID : 0;
				foreach (ShipInfo current2 in list)
				{
					Matrix? matrix = (current2.RiderIndex >= 0) ? base.App.GameDatabase.GetShipSystemPosition(current2.ParentID) : base.App.GameDatabase.GetShipSystemPosition(current2.ID);
					if (!matrix.HasValue)
					{
						CommonCombatState.Warn(string.Concat(new string[]
						{
							"Ship [",
							current2.ShipName,
							"] is in a [",
							fleet.Type.ToString(),
							"] fleet failed to create valid transform, therefore it will not be spawned"
						}));
					}
					else
					{
						bool flag = false;
						bool flag2 = false;
						IEnumerable<string> designSectionNames = base.App.GameDatabase.GetDesignSectionNames(current2.DesignID);
						foreach (string s in designSectionNames)
						{
							ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == s);
							if (shipSectionAsset != null)
							{
								if (shipSectionAsset.isPolice)
								{
									flag = true;
								}
								if (shipSectionAsset.Type == ShipSectionType.Mission)
								{
									SectionEnumerations.CombatAiType arg_32F_0 = shipSectionAsset.CombatAIType;
									ShipClass arg_336_0 = shipSectionAsset.Class;
									RealShipClasses arg_33D_0 = shipSectionAsset.RealClass;
									flag2 = shipSectionAsset.isMineLayer;
								}
							}
						}
						Matrix value = matrix.Value;
						if (fleet.Type == FleetType.FL_DEFENSE && flag2)
						{
							DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(current2.DesignID);
							if (designInfo != null)
							{
								DesignSectionInfo mission = designInfo.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission);
								if (mission != null)
								{
									ShipSectionAsset shipSectionAsset2 = base.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == mission.FilePath);
									try
									{
										LogicalBank[] banks = shipSectionAsset2.Banks;
										int i = 0;
										LogicalBank lb;
										while (i < banks.Length)
										{
											lb = banks[i];
											if (lb.TurretClass == WeaponEnums.TurretClasses.Minelayer)
											{
												WeaponBankInfo weaponBankInfo = mission.WeaponBanks.FirstOrDefault((WeaponBankInfo x) => x.BankGUID == lb.GUID);
												if (weaponBankInfo != null && weaponBankInfo.WeaponID.HasValue)
												{
													string weaponName = Path.GetFileNameWithoutExtension(base.App.GameDatabase.GetWeaponAsset(weaponBankInfo.WeaponID.Value));
													LogicalWeapon weapon = base.App.AssetDatabase.Weapons.First((LogicalWeapon w) => string.Equals(w.WeaponName, weaponName, StringComparison.InvariantCultureIgnoreCase));
													LogicalWeapon logicalWeapon = (!string.IsNullOrEmpty(weapon.SubWeapon)) ? base.App.AssetDatabase.Weapons.First((LogicalWeapon w) => string.Equals(w.WeaponName, weapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase)) : null;
													weapon.AddGameObjectReference();
													if (logicalWeapon != null)
													{
														logicalWeapon.AddGameObjectReference();
													}
													Faction faction = base.App.AssetDatabase.Factions.First((Faction x) => mission.ShipSectionAsset.Faction == x.Name);
													Player playerObject = base.App.Game.GetPlayerObject(designInfo.PlayerID);
													Subfaction subfaction = null;
													string preferredMount = "";
													if (playerObject != null)
													{
														subfaction = faction.Subfactions[Math.Min(playerObject.SubfactionIndex, faction.Subfactions.Length - 1)];
														if (base.App.LocalPlayer != playerObject || !subfaction.DlcID.HasValue || base.App.Steam.HasDLC((int)subfaction.DlcID.Value))
														{
															preferredMount = subfaction.MountName;
														}
														else
														{
															preferredMount = faction.Subfactions[0].MountName;
														}
													}
													MountObject.WeaponModels weaponModels = new MountObject.WeaponModels();
													weaponModels.FillOutModelFilesWithWeapon(weapon, faction, preferredMount, base.App.AssetDatabase.Weapons);
													List<object> list3 = new List<object>();
													list3.Add(current2.ID);
													list3.Add(value.Position);
													list3.Add(Vector3.Normalize(value.Forward));
													list3.Add(base.App.AssetDatabase.MineFieldParams.Width);
													list3.Add(base.App.AssetDatabase.MineFieldParams.Length);
													list3.Add(base.App.AssetDatabase.MineFieldParams.Height);
													list3.Add(base.App.AssetDatabase.MineFieldParams.SpacingOffset);
													list3.Add(weapon.GameObject.ObjectID);
													list3.Add((logicalWeapon != null) ? logicalWeapon.GameObject.ObjectID : 0);
													list3.Add((player != null) ? player.ObjectID : 0);
													list3.Add(weaponModels.WeaponModelPath.ModelPath);
													list3.Add(weaponModels.SubWeaponModelPath.ModelPath);
													IGameObject gameObject = base.App.AddObject(InteropGameObjectType.IGOT_MINEFIELD, list3.ToArray());
													crits.Add(gameObject);
													yield return gameObject.ObjectID;
													break;
												}
												break;
											}
											else
											{
												i++;
											}
										}
									}
									finally
									{
									}
								}
							}
						}
						else
						{
							Ship ship = Ship.CreateShip(base.App.Game, value, current2, 0, this._input.ObjectID, playerId, this._starSystemObjects.IsDeepSpace, this._playersInCombat);
							ship.ParentDatabaseID = current2.ParentID;
							CombatZonePositionInfo combatZonePositionInfo = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
							ship.Maneuvering.RetreatDestination = Vector3.Normalize(value.Position) * combatZonePositionInfo.RadiusUpper;
							crits.Add(ship);
							if (this._pirateEncounterData.IsPirateEncounter && flag)
							{
								this._pirateEncounterData.PoliceShipsInSystem.Add(ship);
							}
							SDBInfo sDBInfoFromShip = base.App.GameDatabase.GetSDBInfoFromShip(current2.ID);
							if (sDBInfoFromShip != null)
							{
								list2.Add(ship.ObjectID);
								list2.Add(sDBInfoFromShip.OrbitalId);
							}
							yield return ship.ObjectID;
							if (!flag || stratModifier)
							{
								this._ships.Insert(current2.ID, ship);
							}
						}
					}
				}
				list2.Insert(0, list2.Count / 2);
				if (player == base.App.LocalPlayer)
				{
					this._starSystemObjects.PostSetProp("SetSDBSlotValuesCombat", list2.ToArray());
				}
			}
			yield break;
		}
		private IEnumerable<int> SpawnAcceleratorFleet(GameObjectSet crits, FleetInfo fleet)
		{
			if (fleet != null && fleet.Type == FleetType.FL_ACCELERATOR)
			{
				Player player = base.App.GetPlayer(fleet.PlayerID);
				int playerId = (player != null) ? player.ObjectID : 0;
				List<ShipInfo> list = base.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo current in list)
				{
					Matrix world = Matrix.Identity;
					Matrix? matrix = base.App.GameDatabase.GetShipSystemPosition(current.ID);
					if (!matrix.HasValue)
					{
						StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(this._systemId);
						if (starSystemInfo != null && !starSystemInfo.IsDeepSpace)
						{
							matrix = GameSession.GetValidGateShipTransform(base.App.Game, this._systemId, fleet.ID);
							if (matrix.HasValue)
							{
								base.App.GameDatabase.UpdateShipSystemPosition(current.ID, new Matrix?(matrix.Value));
							}
							CommonCombatState.Warn(string.Concat(new string[]
							{
								"Loa Gate [",
								current.ShipName,
								"] is in a [",
								fleet.Type.ToString(),
								"] fleet and doesnt not have a valid transform, attempt to create new transform"
							}));
						}
					}
					if (matrix.HasValue)
					{
						world = matrix.Value;
					}
					Ship ship = Ship.CreateShip(base.App.Game, world, current, 0, this._input.ObjectID, playerId, this._starSystemObjects.IsDeepSpace, this._playersInCombat);
					ship.ParentDatabaseID = current.ParentID;
					crits.Add(ship);
					yield return ship.ObjectID;
					this._ships.Insert(current.ID, ship);
				}
			}
			yield break;
		}
		private IEnumerable<int> SpawnGateFleet(GameObjectSet crits, FleetInfo fleet)
		{
			if (fleet != null && fleet.Type == FleetType.FL_GATE)
			{
				Player player = base.App.GetPlayer(fleet.PlayerID);
				int playerId = (player != null) ? player.ObjectID : 0;
				List<ShipInfo> list = base.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo current in list)
				{
					Matrix? matrix = base.App.GameDatabase.GetShipSystemPosition(current.ID);
					if (!matrix.HasValue && (fleet.Type == FleetType.FL_DEFENSE || fleet.Type == FleetType.FL_GATE))
					{
						if (fleet.Type == FleetType.FL_GATE)
						{
							matrix = GameSession.GetValidGateShipTransform(base.App.Game, this._systemId, fleet.ID);
							if (matrix.HasValue)
							{
								base.App.GameDatabase.UpdateShipSystemPosition(current.ID, new Matrix?(matrix.Value));
							}
							CommonCombatState.Warn(string.Concat(new string[]
							{
								"Ship [",
								current.ShipName,
								"] is in a [",
								fleet.Type.ToString(),
								"] fleet and doesnt not have a valid transform, attempt to create new transform"
							}));
						}
						if (!matrix.HasValue)
						{
							CommonCombatState.Warn(string.Concat(new string[]
							{
								"Ship [",
								current.ShipName,
								"] is in a [",
								fleet.Type.ToString(),
								"] fleet failed to create valid transform, therefore it will not be spawned"
							}));
							continue;
						}
					}
					Matrix value = matrix.Value;
					Ship ship = Ship.CreateShip(base.App.Game, value, current, 0, this._input.ObjectID, playerId, this._starSystemObjects.IsDeepSpace, this._playersInCombat);
					ship.ParentDatabaseID = current.ParentID;
					crits.Add(ship);
					ship.Deployed = ((current.Params & ShipParams.HS_GATE_DEPLOYED) != (ShipParams)0);
					yield return ship.ObjectID;
					this._ships.Insert(current.ID, ship);
				}
			}
			yield break;
		}
		private RetreatData GetRetreatData(int playerID, int systemID, Vector3 spawnPos)
		{
			RetreatData retreatData = new RetreatData();
			retreatData.DefaultDestination = spawnPos;
			retreatData.SystemRadius = this._starSystemObjects.GetSystemRadius();
			if (this._starSystemObjects.CombatZones.Count > 0)
			{
				retreatData.SystemRadius = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>().RadiusUpper;
			}
			PlayerInfo pi = base.App.GameDatabase.GetPlayerInfo(playerID);
			Faction faction = null;
			if (pi != null)
			{
				faction = base.App.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.ID == pi.FactionID);
			}
			bool flag = false;
			if (faction != null)
			{
				if (faction.CanUseNodeLine(null))
				{
					NodeLineInfo nodeLineBetweenSystems = base.App.GameDatabase.GetNodeLineBetweenSystems(playerID, this._systemId, systemID, faction.CanUseNodeLine(new bool?(true)), false);
					if (nodeLineBetweenSystems != null)
					{
						Vector3 starSystemOrigin = base.App.GameDatabase.GetStarSystemOrigin(this._systemId);
						Vector3 starSystemOrigin2 = base.App.GameDatabase.GetStarSystemOrigin(systemID);
						Vector3 value = starSystemOrigin2 - starSystemOrigin;
						value.Y = 0f;
						if (this._starSystemObjects.CombatZones.Count > 0)
						{
							CombatZonePositionInfo combatZonePositionInfo = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
							retreatData.DefaultDestination = Vector3.Normalize(value) * (combatZonePositionInfo.RadiusLower + faction.EntryPointOffset);
						}
						else
						{
							retreatData.DefaultDestination = Vector3.Normalize(value) * (StarSystem.CombatZoneMapRadii.ElementAt(StarSystem.CombatZoneMapRadii.Count<float>() - 1) * 5700f + faction.EntryPointOffset);
						}
						flag = true;
					}
					if (faction.CanUseNodeLine(new bool?(true)))
					{
						retreatData.DefaultDestination = this._starSystemObjects.GetClosestPermanentNodeToPosition(base.App, spawnPos);
						flag = true;
					}
				}
				else
				{
					if (faction.CanUseGate() || faction.CanUseAccelerators())
					{
						Matrix? gateSpawnTransform = this.GetGateSpawnTransform(playerID);
						if (gateSpawnTransform.HasValue)
						{
							retreatData.DefaultDestination = gateSpawnTransform.Value.Position;
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				if (base.App.Game.ScriptModules.GhostShip != null && base.App.Game.ScriptModules.GhostShip.PlayerID == playerID)
				{
					retreatData.DefaultDestination = this._starSystemObjects.GetClosestPermanentNodeToPosition(base.App, spawnPos);
					flag = true;
				}
				else
				{
					if (this._starSystemObjects.CombatZones.Count > 0)
					{
						CombatZonePositionInfo combatZonePositionInfo2 = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
						retreatData.DefaultDestination = Vector3.Normalize(spawnPos) * combatZonePositionInfo2.RadiusUpper;
					}
					else
					{
						retreatData.DefaultDestination = Vector3.Normalize(spawnPos) * 125000f;
					}
				}
			}
			retreatData.SetDestination = flag;
			return retreatData;
		}
		private void AddRidersToShip(GameObjectSet crits, Ship ship)
		{
			if (ship == null)
			{
				return;
			}
			foreach (Kerberos.Sots.GameObjects.BattleRiderMount current in ship.BattleRiderMounts)
			{
				DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(current.DesignID);
				if (designInfo != null)
				{
					ShipInfo shipInfo = new ShipInfo
					{
						DesignID = current.DesignID,
						DesignInfo = designInfo,
						FleetID = 0,
						ParentID = ship.DatabaseID,
						SerialNumber = 0,
						ShipName = string.Empty
					};
					Ship ship2 = Ship.CreateShip(base.App.Game, Matrix.Identity, shipInfo, 0, this._input.ObjectID, ship.Player.ObjectID, this._starSystemObjects.IsDeepSpace, this._playersInCombat);
					ship2.ParentDatabaseID = ship.DatabaseID;
					crits.Add(ship2);
				}
			}
		}
		private void CreateAllPointsOfInterest(GameObjectSet crits, OrbitalObjectInfo[] oribitalObjects, List<CombatEasterEggData> easterEgg)
		{
			StationInfo stationInfo = (this._pirateEncounterData.PirateBase != null) ? base.App.GameDatabase.GetStationInfo(this._pirateEncounterData.PirateBase.BaseStationId) : null;
			List<IGameObject> list = crits.Objects.ToList<IGameObject>();
			foreach (IGameObject current in list)
			{
				if (current is Ship)
				{
					Ship ship = current as Ship;
					SectionEnumerations.CombatAiType combatAI = ship.CombatAI;
					switch (combatAI)
					{
					case SectionEnumerations.CombatAiType.SwarmerHive:
					case SectionEnumerations.CombatAiType.SwarmerQueen:
					case SectionEnumerations.CombatAiType.VonNeumannCollectorMotherShip:
					case SectionEnumerations.CombatAiType.VonNeumannSeekerMotherShip:
					case SectionEnumerations.CombatAiType.VonNeumannBerserkerMotherShip:
					case SectionEnumerations.CombatAiType.VonNeumannNeoBerserker:
					case SectionEnumerations.CombatAiType.VonNeumannPlanetKiller:
					case SectionEnumerations.CombatAiType.LocustMoon:
					case SectionEnumerations.CombatAiType.LocustWorld:
					case SectionEnumerations.CombatAiType.SystemKiller:
						this.AddPointOfInterest(ship.ObjectID, ship.Maneuvering.Position, false);
						break;
					case SectionEnumerations.CombatAiType.SwarmerQueenLarva:
					case SectionEnumerations.CombatAiType.VonNeumannCollectorProbe:
					case SectionEnumerations.CombatAiType.VonNeumannSeekerProbe:
					case SectionEnumerations.CombatAiType.VonNeumannDisc:
					case SectionEnumerations.CombatAiType.VonNeumannPyramid:
					case SectionEnumerations.CombatAiType.LocustFighter:
						break;
					default:
						if (combatAI == SectionEnumerations.CombatAiType.CommandMonitor)
						{
							FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(base.App.GameDatabase.GetShipInfo(ship.DatabaseID, false).FleetID);
							if (fleetInfo != null && fleetInfo.PlayerID == base.App.Game.ScriptModules.AsteroidMonitor.PlayerID)
							{
								this.AddPointOfInterest(ship.ObjectID, ship.Maneuvering.Position, false);
							}
						}
						break;
					}
					if (this._pirateEncounterData.IsPirateEncounter && stationInfo != null && ship.DatabaseID == stationInfo.ShipID)
					{
						this.AddPointOfInterest(ship.ObjectID, ship.Maneuvering.Position, false);
					}
				}
			}
			if (easterEgg.Count > 0)
			{
				foreach (CombatEasterEggData current2 in easterEgg)
				{
					if (current2.Type == EasterEgg.EE_GARDENERS)
					{
						List<PlanetInfo> gardenerPlanetsFromList = Gardeners.GetGardenerPlanetsFromList(base.App, this._systemId);
						foreach (PlanetInfo current3 in gardenerPlanetsFromList)
						{
							this.AddPointOfInterest(0, base.App.GameDatabase.GetOrbitalTransform(current3.ID).Position, false);
						}
					}
				}
			}
		}
		private void AddPointOfInterest(int targetID, Vector3 startPosition, bool hasBeenSeen)
		{
			PointOfInterest pointOfInterest = this._crits.Add<PointOfInterest>(new object[]
			{
				"effects\\ui\\Point_of_Interest.effect",
				targetID
			});
			pointOfInterest.TargetID = targetID;
			pointOfInterest.HasBeenSeen = hasBeenSeen;
			pointOfInterest.Position = startPosition;
			this._postLoadedObjects.Add(pointOfInterest);
		}
		private List<CombatEasterEggData> GetCombatEasterEggData(OrbitalObjectInfo[] oribitalObjects, IDictionary<int, List<FleetInfo>> selectedPlayerFleets)
		{
			List<CombatEasterEggData> list = new List<CombatEasterEggData>();
			foreach (List<FleetInfo> current in selectedPlayerFleets.Values)
			{
				foreach (FleetInfo current2 in current)
				{
					if (current2 != null)
					{
						if (base.App.Game.ScriptModules.AsteroidMonitor != null && base.App.Game.ScriptModules.AsteroidMonitor.PlayerID == current2.PlayerID)
						{
							list.Add(new CombatEasterEggData
							{
								Type = EasterEgg.EE_ASTEROID_MONITOR,
								FleetID = current2.ID
							});
						}
						else
						{
							if (base.App.Game.ScriptModules.MorrigiRelic != null && base.App.Game.ScriptModules.MorrigiRelic.PlayerID == current2.PlayerID)
							{
								list.Add(new CombatEasterEggData
								{
									Type = EasterEgg.EE_MORRIGI_RELIC,
									FleetID = current2.ID
								});
							}
							else
							{
                                if (!Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, current2) && base.App.Game.ScriptModules.Gardeners != null && base.App.Game.ScriptModules.Gardeners.PlayerID == current2.PlayerID)
								{
									list.Add(new CombatEasterEggData
									{
										Type = EasterEgg.EE_GARDENERS,
										FleetID = current2.ID
									});
								}
								else
								{
									if (base.App.Game.ScriptModules.Swarmers != null && base.App.Game.ScriptModules.Swarmers.PlayerID == current2.PlayerID)
									{
										list.Add(new CombatEasterEggData
										{
											Type = EasterEgg.EE_SWARM,
											FleetID = current2.ID
										});
									}
									else
									{
										if (base.App.Game.ScriptModules.VonNeumann != null && base.App.Game.ScriptModules.VonNeumann.PlayerID == current2.PlayerID)
										{
											list.Add(new CombatEasterEggData
											{
												Type = EasterEgg.EE_VON_NEUMANN,
												FleetID = current2.ID
											});
										}
										else
										{
                                            if (Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, current2))
											{
												list.Add(new CombatEasterEggData
												{
													Type = EasterEgg.GM_GARDENER,
													FleetID = current2.ID
												});
											}
											else
											{
												if (base.App.Game.ScriptModules.Locust != null && base.App.Game.ScriptModules.Locust.PlayerID == current2.PlayerID)
												{
													list.Add(new CombatEasterEggData
													{
														Type = EasterEgg.GM_LOCUST_SWARM,
														FleetID = current2.ID
													});
												}
												else
												{
													if (base.App.Game.ScriptModules.Comet != null && base.App.Game.ScriptModules.Comet.PlayerID == current2.PlayerID)
													{
														list.Add(new CombatEasterEggData
														{
															Type = EasterEgg.GM_COMET,
															FleetID = current2.ID
														});
													}
													else
													{
														if (base.App.Game.ScriptModules.SystemKiller != null && base.App.Game.ScriptModules.SystemKiller.PlayerID == current2.PlayerID)
														{
															list.Add(new CombatEasterEggData
															{
																Type = EasterEgg.GM_SYSTEM_KILLER,
																FleetID = current2.ID
															});
														}
														else
														{
															if (base.App.Game.ScriptModules.Pirates != null && base.App.Game.ScriptModules.Pirates.PlayerID == current2.PlayerID && this._pirateEncounterData.PirateBase != null)
															{
																list.Add(new CombatEasterEggData
																{
																	Type = EasterEgg.EE_PIRATE_BASE,
																	FleetID = current2.ID
																});
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			foreach (CombatEasterEggData current3 in list)
			{
				switch (current3.Type)
				{
				case EasterEgg.EE_SWARM:
					current3.BaseFleetSpawnMatrix = Swarmers.GetBaseEnemyFleetTrans(base.App, this._systemId);
					break;
				case EasterEgg.EE_ASTEROID_MONITOR:
					current3.BaseFleetSpawnMatrix = AsteroidMonitor.GetBaseEnemyFleetTrans(base.App, base.App.GameDatabase.GetShipInfoByFleetID(current3.FleetID, false).ToList<ShipInfo>(), this._systemId);
					break;
				case EasterEgg.EE_PIRATE_BASE:
					current3.BaseFleetSpawnMatrix = Pirates.GetBaseEnemyFleetTrans(base.App, this._pirateEncounterData.PirateBase);
					break;
				case EasterEgg.EE_VON_NEUMANN:
					current3.BaseFleetSpawnMatrix = VonNeumann.GetBaseEnemyFleetTrans(base.App, this._systemId);
					break;
				case EasterEgg.EE_GARDENERS:
				case EasterEgg.GM_GARDENER:
					current3.BaseFleetSpawnMatrix = Gardeners.GetBaseEnemyFleetTrans(base.App, this._systemId, oribitalObjects);
					break;
				case EasterEgg.EE_MORRIGI_RELIC:
					current3.BaseFleetSpawnMatrix = MorrigiRelic.GetBaseEnemyFleetTrans(base.App, (
						from x in base.App.GameDatabase.GetShipInfoByFleetID(current3.FleetID, false)
						where x.ParentID == 0
						select x).ToList<ShipInfo>(), oribitalObjects);
					break;
				case EasterEgg.GM_SYSTEM_KILLER:
					current3.BaseFleetSpawnMatrix = SystemKiller.GetBaseEnemyFleetTrans(base.App, current3.FleetID, this._starSystemObjects, oribitalObjects);
					break;
				case EasterEgg.GM_LOCUST_SWARM:
					current3.BaseFleetSpawnMatrix = Locust.GetBaseEnemyFleetTrans(base.App, this._systemId);
					break;
				case EasterEgg.GM_COMET:
					current3.BaseFleetSpawnMatrix = Comet.GetBaseEnemyFleetTrans(base.App, this._starSystemObjects, oribitalObjects);
					break;
				}
			}
			return list;
		}
		private List<CombatRandomData> GetCombatRandomData(OrbitalObjectInfo[] oribitalObjects, IDictionary<int, List<FleetInfo>> selectedPlayerFleets)
		{
			List<CombatRandomData> list = new List<CombatRandomData>();
			foreach (List<FleetInfo> current in selectedPlayerFleets.Values)
			{
				foreach (FleetInfo current2 in current)
				{
					if (current2 != null)
					{
						if (base.App.Game.ScriptModules.MeteorShower != null && base.App.Game.ScriptModules.MeteorShower.PlayerID == current2.PlayerID)
						{
							list.Add(new CombatRandomData
							{
								Type = RandomEncounter.RE_ASTEROID_SHOWER,
								FleetID = current2.ID
							});
						}
						else
						{
							if (base.App.Game.ScriptModules.Spectre != null && base.App.Game.ScriptModules.Spectre.PlayerID == current2.PlayerID)
							{
								list.Add(new CombatRandomData
								{
									Type = RandomEncounter.RE_SPECTORS,
									FleetID = current2.ID
								});
							}
							else
							{
								if (base.App.Game.ScriptModules.GhostShip != null && base.App.Game.ScriptModules.GhostShip.PlayerID == current2.PlayerID)
								{
									list.Add(new CombatRandomData
									{
										Type = RandomEncounter.RE_GHOST_SHIP,
										FleetID = current2.ID
									});
								}
								else
								{
									if (base.App.Game.ScriptModules.Slaver != null && base.App.Game.ScriptModules.Slaver.PlayerID == current2.PlayerID)
									{
										list.Add(new CombatRandomData
										{
											Type = RandomEncounter.RE_SLAVERS,
											FleetID = current2.ID
										});
									}
								}
							}
						}
					}
				}
			}
			foreach (CombatRandomData current3 in list)
			{
				switch (current3.Type)
				{
				case RandomEncounter.RE_ASTEROID_SHOWER:
					current3.BaseFleetSpawnMatrix = MeteorShower.GetBaseEnemyFleetTrans(base.App, this._systemId);
					continue;
				case RandomEncounter.RE_SPECTORS:
					current3.BaseFleetSpawnMatrix = Spectre.GetBaseEnemyFleetTrans(base.App, this._systemId);
					continue;
				case RandomEncounter.RE_SLAVERS:
					current3.BaseFleetSpawnMatrix = Slaver.GetBaseEnemyFleetTrans(base.App, this._starSystemObjects);
					continue;
				case RandomEncounter.RE_GHOST_SHIP:
					current3.BaseFleetSpawnMatrix = GhostShip.GetBaseEnemyFleetTrans(base.App, this._starSystemObjects);
					continue;
				}
				current3.BaseFleetSpawnMatrix = Matrix.Identity;
			}
			return list;
		}
		private CombatEasterEggData GetMostThreateningEasterEgg(List<CombatEasterEggData> ceed)
		{
			if (ceed.Count == 0)
			{
				return null;
			}
			CombatEasterEggData combatEasterEggData = ceed.FirstOrDefault((CombatEasterEggData x) => x.Type == EasterEgg.GM_SYSTEM_KILLER);
			if (combatEasterEggData != null)
			{
				return combatEasterEggData;
			}
			combatEasterEggData = ceed.FirstOrDefault((CombatEasterEggData x) => x.Type == EasterEgg.GM_LOCUST_SWARM);
			if (combatEasterEggData != null)
			{
				return combatEasterEggData;
			}
			combatEasterEggData = ceed.FirstOrDefault((CombatEasterEggData x) => x.Type == EasterEgg.GM_COMET);
			if (combatEasterEggData != null)
			{
				return combatEasterEggData;
			}
			combatEasterEggData = ceed.FirstOrDefault((CombatEasterEggData x) => x.Type == EasterEgg.GM_GARDENER);
			if (combatEasterEggData != null)
			{
				return combatEasterEggData;
			}
			combatEasterEggData = ceed.FirstOrDefault((CombatEasterEggData x) => x.Type == EasterEgg.EE_VON_NEUMANN);
			if (combatEasterEggData != null)
			{
				return combatEasterEggData;
			}
			combatEasterEggData = ceed.FirstOrDefault((CombatEasterEggData x) => x.Type == EasterEgg.EE_SWARM);
			if (combatEasterEggData != null)
			{
				return combatEasterEggData;
			}
			combatEasterEggData = ceed.FirstOrDefault((CombatEasterEggData x) => x.Type == EasterEgg.EE_GARDENERS);
			if (combatEasterEggData != null)
			{
				return combatEasterEggData;
			}
			return ceed.First<CombatEasterEggData>();
		}
		private CombatRandomData GetMostThreateningRandom(List<CombatRandomData> crd)
		{
			if (crd.Count == 0)
			{
				return null;
			}
			CombatRandomData combatRandomData = crd.FirstOrDefault((CombatRandomData x) => x.Type == RandomEncounter.RE_GHOST_SHIP);
			if (combatRandomData != null)
			{
				return combatRandomData;
			}
			combatRandomData = crd.FirstOrDefault((CombatRandomData x) => x.Type == RandomEncounter.RE_SPECTORS);
			if (combatRandomData != null)
			{
				return combatRandomData;
			}
			return crd.First<CombatRandomData>();
		}
		private Vector3 GetBaseFleetSpawnDirectionEasterEgg(CombatEasterEggData ceed)
		{
			if (ceed == null || ceed.BaseFleetSpawnMatrix.Position.LengthSquared < 1.401298E-45f)
			{
				return Vector3.UnitX;
			}
			switch (ceed.Type)
			{
			case EasterEgg.EE_VON_NEUMANN:
			case EasterEgg.GM_LOCUST_SWARM:
				return Vector3.Cross(ceed.BaseFleetSpawnMatrix.Forward, Vector3.UnitY);
			case EasterEgg.GM_SYSTEM_KILLER:
			case EasterEgg.GM_COMET:
			case EasterEgg.GM_GARDENER:
				return ceed.BaseFleetSpawnMatrix.Forward;
			}
			return Vector3.Normalize(ceed.BaseFleetSpawnMatrix.Position);
		}
		private Vector3 GetBaseFleetSpawnDirectionFromRandom(CombatRandomData crd)
		{
			if (crd == null || crd.BaseFleetSpawnMatrix.Position.LengthSquared < 1.401298E-45f)
			{
				return Vector3.UnitX;
			}
			switch (crd.Type)
			{
			case RandomEncounter.RE_SPECTORS:
			case RandomEncounter.RE_PIRATES:
			case RandomEncounter.RE_SLAVERS:
			case RandomEncounter.RE_REFUGEES:
			case RandomEncounter.RE_GHOST_SHIP:
				return crd.BaseFleetSpawnMatrix.Forward;
			case RandomEncounter.RE_FLYING_DUTCHMAN:
				return Vector3.Cross(crd.BaseFleetSpawnMatrix.Forward, Vector3.UnitY);
			}
			return -crd.BaseFleetSpawnMatrix.Forward;
		}
		private void SetSelectionToGroup(List<Ship> ships)
		{
			List<object> list = new List<object>();
			list.Add(1);
			list.Add(ships.Count);
			foreach (Ship current in ships)
			{
				list.Add(current.ObjectID);
			}
			this._input.PostSetProp("SetSelectionToGroup", list.ToArray());
		}
		private void CreateShipAbility(ShipFleetAbilityType type, List<Ship> owners, List<Ship> affected)
		{
			if (owners.Count == 0)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(owners.Count);
			foreach (Ship current in owners)
			{
				list.Add(current.ObjectID);
				list.Add(current.MissionSection.ObjectID);
			}
			list.Add(affected.Count);
			foreach (Ship current2 in affected)
			{
				list.Add(current2.ObjectID);
			}
			ShipFleetAbility shipFleetAbility = null;
			switch (type)
			{
			case ShipFleetAbilityType.None:
				goto IL_1B5;
			case ShipFleetAbilityType.Protectorate:
			{
				shipFleetAbility = base.App.AddObject<ProtectorateAbility>(list.ToArray());
				LogicalShield logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => x.TechID == "SLD_Shield_Mk._II");
				if (logicalShield == null)
				{
					goto IL_1B5;
				}
				using (List<Ship>.Enumerator enumerator3 = affected.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Ship current3 = enumerator3.Current;
						if (current3.Shield == null && !ShipSectionAsset.IsWeaponBattleRiderClass(current3.RealShipClass) && Ship.IsShipClassBigger(ShipClass.Cruiser, current3.ShipClass, true))
						{
							current3.ExternallyAssignShieldToShip(base.App, logicalShield);
						}
					}
					goto IL_1B5;
				}
				break;
			}
			case ShipFleetAbilityType.Hidden:
				break;
			case ShipFleetAbilityType.Deaf:
				shipFleetAbility = base.App.AddObject<TheDeafAbility>(list.ToArray());
				goto IL_1B5;
			default:
				goto IL_1B5;
			}
			shipFleetAbility = base.App.AddObject<TheHiddenAbility>(list.ToArray());
			IL_1B5:
			if (shipFleetAbility != null)
			{
				this._crits.Add(shipFleetAbility);
			}
		}
		private void PostNewPlayerFormation(List<FleetFormationCreationData> ffcd)
		{
			List<object> list = new List<object>();
			list.Add(InteropMessageID.IMID_ENGINE_ADD_FORMATION_PATTERN);
			list.Add(ffcd.Count);
			foreach (FleetFormationCreationData current in ffcd)
			{
				list.Add(current.FormationData.Count);
				foreach (FormationCreationData current2 in current.FormationData)
				{
					list.Add(current2.ShipID);
					list.Add(current2.DesignID);
					list.Add((int)current2.ShipRole);
					list.Add((int)current2.ShipClass);
					list.Add(current2.FormationPosition);
				}
			}
			base.App.PostEngineMessage(list);
			if (ffcd.Count > 0 && this._input != null)
			{
				this._input.PostSetProp("SyncFormationGroups", new object[0]);
			}
		}
		private SpawnProfile GetSpawnProfileForFleet(int fleetID, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, List<CombatEasterEggData> eeData, List<CombatRandomData> randData, ref int spawnedFleetCount)
		{
			foreach (SpawnProfile current in this._spawnPositions)
			{
				if (current._fleetID == fleetID)
				{
					SpawnProfile result = current;
					return result;
				}
			}
			SpawnProfile spawnProfile = new SpawnProfile();
			spawnProfile._fleetID = fleetID;
			spawnProfile._spawnFacing = Vector3.Zero;
			int num = 0;
			FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(fleetID);
			spawnProfile._playerID = fleetInfo.PlayerID;
			foreach (ShipInfo current2 in base.App.GameDatabase.GetShipInfoByFleetID(fleetID, false))
			{
				spawnProfile._reserveShips.Add(current2.ID);
				int shipCommandPointQuota = base.App.GameDatabase.GetShipCommandPointQuota(current2.ID);
				if (shipCommandPointQuota > num)
				{
					num = shipCommandPointQuota;
					spawnProfile._activeCommandShipID = current2.ID;
				}
			}
			if (spawnProfile._activeCommandShipID > 0)
			{
				spawnProfile._reserveShips.Remove(spawnProfile._activeCommandShipID);
				spawnProfile._activeShips.Add(spawnProfile._activeCommandShipID);
				num -= base.App.GameDatabase.GetCommandPointCost(base.App.GameDatabase.GetShipInfo(spawnProfile._activeCommandShipID, false).DesignID);
			}
			else
			{
				num = 18;
			}
			while (num > 0 && spawnProfile._reserveShips.Count<int>() > 0)
			{
				int num2 = 0;
				int num3 = 0;
				foreach (ShipInfo current3 in base.App.GameDatabase.GetShipInfoByFleetID(fleetID, false))
				{
					if (current3.ID != spawnProfile._activeCommandShipID && spawnProfile._reserveShips.Contains(current3.ID))
					{
						int commandPointCost = base.App.GameDatabase.GetCommandPointCost(current3.DesignID);
						if (commandPointCost <= num && commandPointCost > num3)
						{
							num3 = commandPointCost;
							num2 = current3.ID;
						}
					}
				}
				if (num2 == 0)
				{
					break;
				}
				spawnProfile._reserveShips.Remove(num2);
				spawnProfile._activeShips.Add(num2);
				num -= num3;
				foreach (ShipInfo current4 in base.App.GameDatabase.GetBattleRidersByParentID(num2))
				{
					spawnProfile._reserveShips.Remove(current4.ID);
					if (current4.RiderIndex >= 0)
					{
						spawnProfile._activeShips.Add(current4.ID);
					}
				}
			}
			if (base.App.Game.ScriptModules.VonNeumann != null && base.App.Game.ScriptModules.VonNeumann.PlayerID == fleetInfo.PlayerID && base.App.Game.ScriptModules.VonNeumann.HomeWorldSystemID == this._systemId)
			{
				Matrix? vNFleetSpawnMatrixAtHomeWorld = base.App.Game.ScriptModules.VonNeumann.GetVNFleetSpawnMatrixAtHomeWorld(base.App.GameDatabase, fleetInfo, spawnedFleetCount);
				if (vNFleetSpawnMatrixAtHomeWorld.HasValue)
				{
					spawnProfile._spawnFacing = vNFleetSpawnMatrixAtHomeWorld.Value.Forward;
					spawnProfile._startingPosition = vNFleetSpawnMatrixAtHomeWorld.Value.Position;
					spawnProfile._spawnPosition = spawnProfile._startingPosition;
				}
				return spawnProfile;
			}
			if (this._trapCombatData.IsTrapCombat)
			{
				return this.GetTrapSpawnLocation(fleetInfo, spawnProfile, ref spawnedFleetCount);
			}
			if (this._pirateEncounterData.PirateBase != null && base.App.Game.ScriptModules.Pirates != null && base.App.Game.ScriptModules.Pirates.PlayerID == fleetInfo.PlayerID)
			{
				StationInfo stationInfo = base.App.Game.GameDatabase.GetStationInfo(this._pirateEncounterData.PirateBase.BaseStationId);
				Matrix orbitalTransform = base.App.Game.GameDatabase.GetOrbitalTransform(stationInfo.OrbitalObjectID);
				Vector3 vector = orbitalTransform.Position;
				OrbitalObjectInfo orbitalObjectInfo = base.App.Game.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
				if (orbitalObjectInfo.ParentID.HasValue)
				{
					Matrix orbitalTransform2 = base.App.Game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value);
					vector = orbitalTransform.Position - orbitalTransform2.Position;
				}
				vector.Normalize();
				spawnProfile._spawnFacing = -vector;
				spawnProfile._startingPosition = orbitalTransform.Position + vector * 2500f;
				spawnProfile._spawnPosition = spawnProfile._startingPosition;
				return spawnProfile;
			}
			SpawnProfile spawnProfile2 = this.GetSpawnInforAtHomeColony(fleetInfo, spawnProfile, ref spawnedFleetCount);
			if (spawnProfile2 != null)
			{
				this._spawnPositions.Add(spawnProfile2);
				spawnedFleetCount++;
				return spawnProfile2;
			}
			spawnProfile2 = this.GetSpawnInforGate(fleetInfo, spawnProfile, selectedPlayerFleets, eeData, randData, ref spawnedFleetCount);
			if (spawnProfile2 != null)
			{
				bool flag = false;
				foreach (KeyValuePair<int, List<FleetInfo>> current5 in selectedPlayerFleets)
				{
					if (this.GetDiplomacyState(current5.Key, fleetInfo.PlayerID) == DiplomacyState.WAR)
					{
						if ((
							from x in current5.Value
							where x != null
							select x).Count<FleetInfo>() > 0)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					SpawnProfile spawnInfoFromCombatZone = this.GetSpawnInfoFromCombatZone(fleetInfo, spawnProfile, selectedPlayerFleets, ref spawnedFleetCount);
					if (spawnInfoFromCombatZone != null)
					{
						spawnProfile2 = spawnInfoFromCombatZone;
					}
				}
				this._spawnPositions.Add(spawnProfile);
				spawnedFleetCount++;
				return spawnProfile2;
			}
			spawnProfile2 = this.GetSpawnInforNotHomeColony(fleetInfo, spawnProfile, selectedPlayerFleets, eeData, randData, ref spawnedFleetCount);
			if (spawnProfile2 != null)
			{
				this._spawnPositions.Add(spawnProfile);
				spawnedFleetCount++;
				return spawnProfile2;
			}
			spawnProfile2 = this.GetDefaultSpawnInfo(fleetInfo, spawnProfile, selectedPlayerFleets, ref spawnedFleetCount);
			if (spawnProfile2 != null)
			{
				this._spawnPositions.Add(spawnProfile);
				spawnedFleetCount++;
				return spawnProfile2;
			}
			spawnedFleetCount++;
			return spawnProfile;
		}
		private SpawnProfile GetTrapSpawnLocation(FleetInfo fleet, SpawnProfile spawnPos, ref int spawnedFleetCount)
		{
			List<StellarBody> planetsInSystem = this._starSystemObjects.GetPlanetsInSystem();
			if (this._trapCombatData.TrapFleets.Contains(fleet))
			{
				int planetID = this._trapCombatData.TrapToPlanet[fleet.ID];
				StellarBody stellarBody = planetsInSystem.FirstOrDefault((StellarBody x) => x.PlanetInfo.ID == planetID);
				if (stellarBody != null)
				{
					spawnPos._startingPosition = stellarBody.Parameters.Position;
					spawnPos._spawnFacing = Vector3.Normalize(stellarBody.Parameters.Position);
				}
				spawnPos._spawnPosition = spawnPos._startingPosition;
				return spawnPos;
			}
			if (this._trapCombatData.ColonyTrappedFleets.Contains(fleet))
			{
				int planetID = this._trapCombatData.TrapToPlanet[this._trapCombatData.ColonyFleetToTrap[fleet.ID]];
				StellarBody stellarBody2 = planetsInSystem.FirstOrDefault((StellarBody x) => x.PlanetInfo.ID == planetID);
				if (stellarBody2 != null)
				{
					Vector3 zero = Vector3.Zero;
					zero.X = (this._random.CoinToss(0.5) ? -1f : 1f) * this._random.NextInclusive(1E-05f, 1f);
					zero.Z = (this._random.CoinToss(0.5) ? -1f : 1f) * this._random.NextInclusive(1E-05f, 1f);
					zero.Normalize();
					spawnPos._startingPosition = stellarBody2.Parameters.Position + zero * (stellarBody2.Parameters.Radius + 750f + 2000f);
					spawnPos._spawnFacing = Vector3.Normalize(stellarBody2.Parameters.Position - spawnPos._startingPosition);
				}
			}
			else
			{
				FleetInfo fleetInfo = this._random.Choose(this._trapCombatData.TrapFleets);
				int planetID = this._trapCombatData.TrapToPlanet[fleetInfo.ID];
				StellarBody stellarBody3 = planetsInSystem.FirstOrDefault((StellarBody x) => x.PlanetInfo.ID == planetID);
				if (stellarBody3 != null)
				{
					Vector3 zero2 = Vector3.Zero;
					zero2.X = (this._random.CoinToss(0.5) ? -1f : 1f) * this._random.NextInclusive(1E-05f, 1f);
					zero2.Z = (this._random.CoinToss(0.5) ? -1f : 1f) * this._random.NextInclusive(1E-05f, 1f);
					zero2.Normalize();
					spawnPos._startingPosition = stellarBody3.Parameters.Position + zero2 * (stellarBody3.Parameters.Radius + 750f + 5000f);
					spawnPos._spawnFacing = Vector3.Normalize(stellarBody3.Parameters.Position - spawnPos._startingPosition);
				}
			}
			spawnPos._spawnPosition = spawnPos._startingPosition;
			return spawnPos;
		}
		private SpawnProfile GetSpawnInforAtHomeColony(FleetInfo fleet, SpawnProfile spawnPos, ref int spawnedFleetCount)
		{
			if (this._systemId != 0)
			{
				bool flag = false;
				PlanetInfo[] starSystemPlanetInfos = base.App.GameDatabase.GetStarSystemPlanetInfos(this._systemId);
				for (int i = 0; i < starSystemPlanetInfos.Length; i++)
				{
					PlanetInfo planetInfo = starSystemPlanetInfos[i];
					ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
					if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == fleet.PlayerID)
					{
						Vector3 position = base.App.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position;
						float num = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
						spawnPos._startingPosition = position;
						Vector3 v = Vector3.Normalize(spawnPos._startingPosition) * (num + 5000f + 1000f * spawnedFleetCount);
						spawnPos._startingPosition += v;
						spawnPos._spawnFacing = Vector3.Normalize(spawnPos._startingPosition);
						List<Ship> stationsAroundPlanet = this._starSystemObjects.GetStationsAroundPlanet(planetInfo.ID);
						if (stationsAroundPlanet.Count > 0)
						{
							foreach (Ship current in stationsAroundPlanet)
							{
								StationInfo stationInfo = this._starSystemObjects.GetStationInfo(current);
								if (stationInfo != null && stationInfo.DesignInfo.StationType == StationType.NAVAL && stationInfo.DesignInfo.StationLevel > 0)
								{
									Vector3 v2 = current.Position - position;
									v2.Normalize();
									spawnPos._startingPosition = current.Position + v2 * 3500f;
									v = Vector3.Normalize(spawnPos._startingPosition) * (1000f * spawnedFleetCount);
									spawnPos._startingPosition += v;
								}
							}
						}
						spawnPos._spawnPosition = spawnPos._startingPosition;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Player playerObject = base.App.Game.GetPlayerObject(fleet.PlayerID);
					if (playerObject == null || !playerObject.IsStandardPlayer || !playerObject.IsAI())
					{
						return spawnPos;
					}
					Vector3? vector = null;
					float num2 = 3.40282347E+38f;
					List<FleetInfo> list = base.App.GameDatabase.GetFleetInfoBySystemID(this._systemId, FleetType.FL_ALL_COMBAT).ToList<FleetInfo>();
					foreach (FleetInfo current2 in list)
					{
						DiplomacyState diplomacyState = this.GetDiplomacyState(fleet.PlayerID, current2.PlayerID);
						if (diplomacyState == DiplomacyState.WAR)
						{
							if (current2.Type == FleetType.FL_NORMAL)
							{
								vector = null;
								break;
							}
							if (current2.Type == FleetType.FL_GATE || current2.Type == FleetType.FL_ACCELERATOR)
							{
								List<ShipInfo> list2 = base.App.GameDatabase.GetShipInfoByFleetID(current2.ID, false).ToList<ShipInfo>();
								if (list2.Count != 0)
								{
									Vector3 vector2 = Vector3.Zero;
									int num3 = 0;
									foreach (ShipInfo current3 in list2)
									{
										Matrix? shipSystemPosition = base.App.GameDatabase.GetShipSystemPosition(current3.ID);
										if (shipSystemPosition.HasValue)
										{
											num3++;
											vector2 += shipSystemPosition.Value.Position;
										}
									}
									if (num3 != 0)
									{
										vector2 /= (float)num3;
										CombatZonePositionInfo closestZoneToPosition = this._starSystemObjects.GetClosestZoneToPosition(base.App, fleet.PlayerID, vector2);
										if (closestZoneToPosition != null)
										{
											float lengthSquared = (closestZoneToPosition.Center - spawnPos._spawnPosition).LengthSquared;
											if (lengthSquared < num2)
											{
												vector = new Vector3?(closestZoneToPosition.Center);
												num2 = lengthSquared;
											}
										}
									}
								}
							}
						}
					}
					if (vector.HasValue)
					{
						spawnPos._spawnFacing = Vector3.Normalize(vector.Value);
						spawnPos._startingPosition = vector.Value - spawnPos._spawnFacing * 5000f;
						spawnPos._spawnPosition = spawnPos._startingPosition;
					}
					return spawnPos;
				}
			}
			return null;
		}
		private Matrix? GetGateSpawnTransform(int playerID)
		{
			int? systemOwningPlayer = base.App.GameDatabase.GetSystemOwningPlayer(this._systemId);
			if (systemOwningPlayer.HasValue && systemOwningPlayer.Value == playerID)
			{
				return null;
			}
			List<FleetInfo> list = base.App.GameDatabase.GetFleetsByPlayerAndSystem(playerID, this._systemId, FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
			if (list.Count == 0)
			{
				return null;
			}
			List<ShipInfo> list2 = base.App.GameDatabase.GetShipInfoByFleetID(list.First<FleetInfo>().ID, false).ToList<ShipInfo>();
			if (list2.Count == 0)
			{
				return null;
			}
			return base.App.GameDatabase.GetShipSystemPosition(list2.First<ShipInfo>().ID);
		}
		private SpawnProfile GetSpawnInforGate(FleetInfo fleet, SpawnProfile spawnPos, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, List<CombatEasterEggData> eeData, List<CombatRandomData> randData, ref int spawnedFleetCount)
		{
			if (eeData.Count > 0 || randData.Count > 0 || fleet.IsGateFleet)
			{
				return null;
			}
			Matrix? gateSpawnTransform = this.GetGateSpawnTransform(fleet.PlayerID);
			if (!gateSpawnTransform.HasValue)
			{
				return null;
			}
			spawnPos._spawnFacing = gateSpawnTransform.Value.Forward;
			spawnPos._startingPosition = gateSpawnTransform.Value.Position + gateSpawnTransform.Value.Forward * 1500f;
			spawnPos._spawnPosition = spawnPos._startingPosition;
			return spawnPos;
		}
		private SpawnProfile GetSpawnInforNotHomeColony(FleetInfo fleet, SpawnProfile spawnPos, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, List<CombatEasterEggData> eeData, List<CombatRandomData> randData, ref int spawnedFleetCount)
		{
			CombatEasterEggData combatEasterEggData = eeData.FirstOrDefault((CombatEasterEggData x) => x.FleetID == fleet.ID);
			CombatRandomData combatRandomData = randData.FirstOrDefault((CombatRandomData x) => x.FleetID == fleet.ID);
			bool flag = eeData.Count > 0 || randData.Count > 0;
			if (this._systemId != 0)
			{
				OrbitalObjectInfo orbitalObjectInfo = null;
				float num = 0f;
				float val = 0f;
				int num2 = 0;
				if (combatEasterEggData != null)
				{
					num2 = base.App.GameDatabase.GetEncounterOrbitalId(combatEasterEggData.Type, this._systemId);
				}
				if (num2 != 0)
				{
					base.App.GameDatabase.GetEncounterPlayerId(base.App.Game, this._systemId);
					orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(num2);
				}
				if (combatEasterEggData != null || combatRandomData != null)
				{
					Matrix matrix = Matrix.Identity;
					if (orbitalObjectInfo != null)
					{
						matrix = base.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
					}
					if (combatEasterEggData != null)
					{
						switch (combatEasterEggData.Type)
						{
						case EasterEgg.EE_SWARM:
							matrix = Swarmers.GetSpawnTransform(base.App, this._systemId);
							break;
						case EasterEgg.EE_PIRATE_BASE:
							matrix = Pirates.GetSpawnTransform(base.App, this._pirateEncounterData.PirateBase);
							break;
						case EasterEgg.EE_VON_NEUMANN:
							matrix = VonNeumann.GetSpawnTransform(base.App, this._systemId);
							break;
						case EasterEgg.GM_LOCUST_SWARM:
							matrix = Locust.GetSpawnTransform(base.App, this._systemId);
							break;
						}
					}
					else
					{
						if (combatRandomData != null)
						{
							RandomEncounter type = combatRandomData.Type;
							if (type != RandomEncounter.RE_SPECTORS)
							{
								switch (type)
								{
								case RandomEncounter.RE_SLAVERS:
									matrix = Slaver.GetSpawnTransform(base.App, this._starSystemObjects);
									break;
								case RandomEncounter.RE_GHOST_SHIP:
									matrix = GhostShip.GetSpawnTransform(base.App, this._starSystemObjects);
									break;
								}
							}
							else
							{
								matrix = Spectre.GetSpawnTransform(base.App, this._systemId);
							}
						}
					}
					spawnPos._spawnFacing = matrix.Forward;
					spawnPos._spawnPosition = matrix.Position;
					spawnPos._startingPosition = matrix.Position;
					return spawnPos;
				}
				bool flag2 = false;
				bool flag3 = false;
				if (orbitalObjectInfo == null)
				{
					float num3 = 0f;
					foreach (OrbitalObjectInfo current in base.App.GameDatabase.GetStarSystemOrbitalObjectInfos(this._systemId))
					{
						PlanetInfo planetInfo = base.App.GameDatabase.GetPlanetInfo(current.ID);
						new List<Ship>();
						ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(current.ID);
						if (colonyInfoForPlanet != null)
						{
							DiplomacyState diplomacyState = this.GetDiplomacyState(colonyInfoForPlanet.PlayerID, fleet.PlayerID);
							flag2 = (flag2 || diplomacyState == DiplomacyState.WAR);
							flag3 = (flag3 || diplomacyState == DiplomacyState.ALLIED);
							if (diplomacyState == DiplomacyState.WAR)
							{
								float lengthSquared = base.App.GameDatabase.GetOrbitalTransform(current.ID).Position.LengthSquared;
								if (lengthSquared > num3)
								{
									num3 = lengthSquared;
								}
							}
						}
						float length = base.App.GameDatabase.GetOrbitalTransform(current.ID).Position.Length;
						if (!current.ParentID.HasValue)
						{
							val = Math.Max(val, length);
						}
						if (planetInfo != null)
						{
							float num4 = length + StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
							if (num4 > num)
							{
								orbitalObjectInfo = current;
								num = num4;
							}
						}
					}
				}
				if (!flag || this._ignoreEncounterSpawnPos)
				{
					SpawnProfile spawnProfile = this.GetSpawnInfoFromCombatZone(fleet, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
					if (spawnProfile != null)
					{
						return spawnProfile;
					}
					if (flag3)
					{
						spawnProfile = this.GetSpawnInfoForFriendlySystem(fleet, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
						if (spawnProfile != null)
						{
							return spawnProfile;
						}
					}
					if (!flag2)
					{
						int? systemOwningPlayer = base.App.GameDatabase.GetSystemOwningPlayer(this._systemId);
						if (!systemOwningPlayer.HasValue || this.GetDiplomacyState(systemOwningPlayer.Value, fleet.PlayerID) != DiplomacyState.WAR)
						{
							spawnProfile = this.GetSpawnInfoForDefaultControlZone(fleet, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
							if (spawnProfile != null)
							{
								return spawnProfile;
							}
							spawnProfile = this.GetSpawnInfoAtNeutralSystem(fleet, spawnPos, selectedPlayerFleets, ref spawnedFleetCount);
							if (spawnProfile != null)
							{
								return spawnProfile;
							}
						}
					}
				}
				if (orbitalObjectInfo != null || flag)
				{
					int factionID = base.App.GameDatabase.GetPlayerInfo(fleet.PlayerID).FactionID;
					Faction faction = base.App.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.ID == factionID);
					if (faction == null)
					{
						return null;
					}
					Vector3 vector = default(Vector3);
					List<ColonyInfo> list = base.App.GameDatabase.GetColonyInfosForSystem(this._systemId).ToList<ColonyInfo>();
					if (list.Count == 0)
					{
						vector = Vector3.UnitX;
					}
					else
					{
						float num5 = 0f;
						foreach (ColonyInfo current2 in list)
						{
							if (current2.PlayerID != fleet.PlayerID)
							{
								vector += base.App.GameDatabase.GetOrbitalTransform(current2.OrbitalObjectID).Position;
								num5 += 1f;
							}
						}
						vector /= num5;
					}
					float num6 = 0f;
					if (orbitalObjectInfo != null)
					{
						PlanetInfo planetInfo2 = base.App.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
						if (planetInfo2 != null)
						{
							num6 = StarSystemVars.Instance.SizeToRadius(planetInfo2.Size);
						}
						else
						{
							StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(this._systemId);
							num6 = StarHelper.CalcRadius(StellarClass.Parse(starSystemInfo.StellarClass).Size) * 3f;
						}
					}
					Vector3 vector2 = Vector3.UnitX;
					Vector3 v;
					if (flag && !this._ignoreEncounterSpawnPos)
					{
						float num7 = 0f;
						if (fleet.AdmiralID != 0)
						{
							AdmiralInfo admiralInfo = base.App.GameDatabase.GetAdmiralInfo(fleet.AdmiralID);
							if (admiralInfo != null)
							{
								num7 = Math.Min((float)admiralInfo.ReactionBonus * base.App.GetStratModifier<float>(StratModifiers.AdmiralReactionModifier, admiralInfo.PlayerID) / 100f, 1f);
							}
						}
						float minEncounterStartPos = base.App.AssetDatabase.MinEncounterStartPos;
						float num8 = Math.Max(base.App.AssetDatabase.MaxEncounterStartPos, minEncounterStartPos);
						float num9 = (num8 - minEncounterStartPos) * num7;
						if (eeData.Count > 0)
						{
							CombatEasterEggData mostThreateningEasterEgg = this.GetMostThreateningEasterEgg(eeData);
							spawnPos._startingPosition = mostThreateningEasterEgg.BaseFleetSpawnMatrix.Position;
							vector2 = this.GetBaseFleetSpawnDirectionEasterEgg(mostThreateningEasterEgg);
						}
						else
						{
							if (randData.Count > 0)
							{
								CombatRandomData mostThreateningRandom = this.GetMostThreateningRandom(randData);
								spawnPos._startingPosition = mostThreateningRandom.BaseFleetSpawnMatrix.Position;
								vector2 = this.GetBaseFleetSpawnDirectionFromRandom(mostThreateningRandom);
							}
							else
							{
								if (orbitalObjectInfo != null)
								{
									spawnPos._startingPosition = base.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position;
									if ((double)spawnPos._startingPosition.LengthSquared < 0.0001)
									{
										vector2 = Vector3.UnitX;
									}
									else
									{
										vector2 = Vector3.Normalize(spawnPos._startingPosition);
									}
								}
							}
						}
						v = vector2 * (minEncounterStartPos + num9 + 5000f);
					}
					else
					{
						if (!flag || this._ignoreEncounterSpawnPos)
						{
							SpawnProfile spawnProfileForEnterSystem = this.GetSpawnProfileForEnterSystem(fleet, faction, spawnPos);
							if (spawnProfileForEnterSystem != null)
							{
								return spawnProfileForEnterSystem;
							}
						}
						float num10 = 0f;
						if (faction.Name == "liir_zuul")
						{
							num10 = 6000f;
						}
						else
						{
							if (faction.Name == "morrigi")
							{
								num10 = 5500f;
							}
							else
							{
								if (faction.Name == "hiver")
								{
									num10 = 7000f;
								}
								else
								{
									if (faction.Name == "tarkas")
									{
										num10 = 4000f;
									}
									else
									{
										if (faction.Name == "human" || faction.Name == "zuul")
										{
											num10 = 5000f;
										}
										else
										{
											if (faction.Name == "loa")
											{
												num10 = 4000f;
											}
										}
									}
								}
							}
						}
						Vector3 v2 = vector * -1f;
						v2.Normalize();
						if (!(faction.Name == "zuul") && !(faction.Name == "human"))
						{
							spawnPos._startingPosition = v2 * num;
						}
						else
						{
							if (orbitalObjectInfo != null)
							{
								spawnPos._startingPosition = base.App.GameDatabase.GetOrbitalTransform(orbitalObjectInfo).Position;
							}
						}
						if (faction.Name == "zuul" && base.App.GameDatabase.PlayerHasTech(fleet.PlayerID, "DRV_Star_Tear"))
						{
							spawnPos._startingPosition = v2 * 40000f;
						}
						v = vector2 * (num10 + num6);
					}
					Vector3 v3 = Vector3.Cross(vector2, new Vector3(0f, 1f, 0f));
					v3.Normalize();
					spawnPos._startingPosition += v + v3 * spawnedFleetCount * 10000f;
					spawnPos._spawnPosition = spawnPos._startingPosition;
					spawnPos._spawnFacing = vector2 * -1f;
					return spawnPos;
				}
			}
			return null;
		}
		private SpawnProfile GetDefaultSpawnInfo(FleetInfo fleet, SpawnProfile spawnPos, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, ref int spawnedFleetCount)
		{
			float num = this._starSystemObjects.GetStarRadius();
			if (base.App.GameDatabase.GetNeutronStarInfos().Any((NeutronStarInfo x) => x.DeepSpaceSystemId.HasValue && x.DeepSpaceSystemId.Value == this._systemId) || base.App.GameDatabase.GetGardenerInfos().Any((GardenerInfo x) => x.DeepSpaceSystemId.HasValue && x.DeepSpaceSystemId.Value == this._systemId))
			{
				num = 5000f;
			}
			float yawRadians = MathHelper.DegreesToRadians(360f / (float)selectedPlayerFleets.Count) * (float)this._spawnPositions.Count;
			Matrix matrix = Matrix.CreateRotationYPR(yawRadians, 0f, 0f);
			spawnPos._startingPosition += matrix.Forward * (num + 15000f + 5000f * spawnedFleetCount);
			spawnPos._spawnPosition = spawnPos._startingPosition;
			spawnPos._spawnFacing = Vector3.Normalize(spawnPos._startingPosition) * -1f;
			return spawnPos;
		}
		private void InitializeNeutralCombatStanceInfo(IDictionary<int, List<FleetInfo>> selectedPlayerFleets)
		{
			this._neutralCombatStanceInfo.InitData();
			FleetInfo fleetInfo = null;
			FleetInfo fleetInfo2 = null;
			foreach (List<FleetInfo> current in selectedPlayerFleets.Values)
			{
				fleetInfo = current.FirstOrDefault((FleetInfo x) => x != null && x.AdmiralID != 0);
				fleetInfo2 = null;
				if (fleetInfo != null && fleetInfo.IsNormalFleet)
				{
					foreach (List<FleetInfo> current2 in selectedPlayerFleets.Values)
					{
						foreach (FleetInfo current3 in current2)
						{
							if (current3 != null && current3.AdmiralID != 0 && current3.Type == FleetType.FL_NORMAL)
							{
								if (fleetInfo.PlayerID == current3.PlayerID)
								{
									break;
								}
								if (base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo.PlayerID, current3.PlayerID) == DiplomacyState.WAR)
								{
									fleetInfo2 = current3;
									break;
								}
							}
						}
					}
					if (fleetInfo != null && fleetInfo2 != null)
					{
						break;
					}
				}
			}
			if (fleetInfo == null || fleetInfo2 == null)
			{
				this._neutralCombatStanceInfo.Stance = NeutralCombatStance.Invalid;
				return;
			}
			AdmiralInfo admiralInfo = base.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			AdmiralInfo admiralInfo2 = base.App.GameDatabase.GetAdmiralInfo(fleetInfo2.AdmiralID);
			int num = admiralInfo.ReactionBonus + this._random.NextInclusive(1, 30);
			int num2 = admiralInfo2.ReactionBonus + this._random.NextInclusive(1, 30);
			int num3 = Math.Abs(num - num2);
			FleetInfo fleetInfo3 = fleetInfo;
			FleetInfo fleetInfo4 = fleetInfo2;
			Vector3 v = Vector3.UnitX;
			float s = 5000f;
			if (num3 > 20)
			{
				this._neutralCombatStanceInfo.Stance = NeutralCombatStance.Chasing;
				fleetInfo3 = ((num > num2) ? fleetInfo2 : fleetInfo);
				fleetInfo4 = ((num > num2) ? fleetInfo : fleetInfo2);
			}
			else
			{
				if (num3 >= 10)
				{
					this._neutralCombatStanceInfo.Stance = NeutralCombatStance.Facing;
				}
				else
				{
					this._neutralCombatStanceInfo.Stance = NeutralCombatStance.Side;
					v = -Vector3.UnitZ;
				}
			}
			this._neutralCombatStanceInfo.FleetAPos.FleetID = fleetInfo3.ID;
			this._neutralCombatStanceInfo.FleetBPos.FleetID = fleetInfo4.ID;
			switch (this._neutralCombatStanceInfo.Stance)
			{
			case NeutralCombatStance.Facing:
				this._neutralCombatStanceInfo.FleetAPos.Position = new Vector3(0f, 0f, -3000f);
				this._neutralCombatStanceInfo.FleetAPos.Facing = Vector3.UnitZ;
				this._neutralCombatStanceInfo.FleetBPos.Position = new Vector3(0f, 0f, 3000f);
				this._neutralCombatStanceInfo.FleetBPos.Facing = -Vector3.UnitZ;
				break;
			case NeutralCombatStance.Chasing:
				this._neutralCombatStanceInfo.FleetAPos.Position = new Vector3(0f, 0f, -2000f);
				this._neutralCombatStanceInfo.FleetAPos.Facing = -Vector3.UnitZ;
				this._neutralCombatStanceInfo.FleetBPos.Position = new Vector3(0f, 0f, 2000f);
				this._neutralCombatStanceInfo.FleetBPos.Facing = -Vector3.UnitZ;
				break;
			case NeutralCombatStance.Side:
				this._neutralCombatStanceInfo.FleetAPos.Position = new Vector3(0f, 0f, -2500f);
				this._neutralCombatStanceInfo.FleetAPos.Facing = -Vector3.UnitZ;
				this._neutralCombatStanceInfo.FleetBPos.Position = new Vector3(0f, 0f, 2500f);
				this._neutralCombatStanceInfo.FleetBPos.Facing = -Vector3.UnitZ;
				break;
			}
			foreach (List<FleetInfo> current4 in selectedPlayerFleets.Values)
			{
				foreach (FleetInfo current5 in current4)
				{
					if (current5 != null && current5.IsNormalFleet)
					{
						if (current5.PlayerID == fleetInfo3.PlayerID || current5.PlayerID == fleetInfo4.PlayerID)
						{
							break;
						}
						DiplomacyState diplomacyStateBetweenPlayers = base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo3.PlayerID, current5.PlayerID);
						DiplomacyState diplomacyStateBetweenPlayers2 = base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo4.PlayerID, current5.PlayerID);
						if (diplomacyStateBetweenPlayers != DiplomacyState.ALLIED && diplomacyStateBetweenPlayers2 != DiplomacyState.ALLIED)
						{
							break;
						}
						if (diplomacyStateBetweenPlayers == DiplomacyState.ALLIED)
						{
							NeutralCombatStanceSpawnPosition neutralCombatStanceSpawnPosition = new NeutralCombatStanceSpawnPosition();
							neutralCombatStanceSpawnPosition.FleetID = current5.ID;
							neutralCombatStanceSpawnPosition.Position = this._neutralCombatStanceInfo.FleetAPos.Position + v * s * (float)(this._neutralCombatStanceInfo.FleetAAllies.Count + 1);
							neutralCombatStanceSpawnPosition.Facing = this._neutralCombatStanceInfo.FleetAPos.Facing;
							this._neutralCombatStanceInfo.FleetAAllies.Add(neutralCombatStanceSpawnPosition);
						}
						else
						{
							if (diplomacyStateBetweenPlayers2 == DiplomacyState.ALLIED)
							{
								NeutralCombatStanceSpawnPosition neutralCombatStanceSpawnPosition2 = new NeutralCombatStanceSpawnPosition();
								neutralCombatStanceSpawnPosition2.FleetID = current5.ID;
								neutralCombatStanceSpawnPosition2.Position = this._neutralCombatStanceInfo.FleetBPos.Position + v * s * (float)(this._neutralCombatStanceInfo.FleetBAllies.Count + 1);
								neutralCombatStanceSpawnPosition2.Facing = this._neutralCombatStanceInfo.FleetBPos.Facing;
								this._neutralCombatStanceInfo.FleetBAllies.Add(neutralCombatStanceSpawnPosition2);
							}
						}
					}
				}
			}
			float num4 = 2000f;
			Vector3 vector = Vector3.Zero;
			int num5 = 2 + this._neutralCombatStanceInfo.FleetAAllies.Count + this._neutralCombatStanceInfo.FleetBAllies.Count;
			foreach (NeutralCombatStanceSpawnPosition current6 in this._neutralCombatStanceInfo.FleetAAllies)
			{
				vector += current6.Position;
			}
			foreach (NeutralCombatStanceSpawnPosition current7 in this._neutralCombatStanceInfo.FleetBAllies)
			{
				vector += current7.Position;
			}
			vector /= (float)num5;
			float num6 = (this._neutralCombatStanceInfo.FleetAPos.Position - vector).LengthSquared;
			num6 = Math.Max((this._neutralCombatStanceInfo.FleetBPos.Position - vector).LengthSquared, num6);
			foreach (NeutralCombatStanceSpawnPosition current8 in this._neutralCombatStanceInfo.FleetAAllies)
			{
				num6 = Math.Max((current8.Position - vector).LengthSquared, num6);
			}
			foreach (NeutralCombatStanceSpawnPosition current9 in this._neutralCombatStanceInfo.FleetBAllies)
			{
				num6 = Math.Max((current9.Position - vector).LengthSquared, num6);
			}
			num6 = (float)Math.Sqrt((double)num6);
			num4 += num6;
			List<StellarBody> planetsInSystem = this._starSystemObjects.GetPlanetsInSystem();
			float starRadius = this._starSystemObjects.GetStarRadius();
			float num7 = 0f;
			if (this._starSystemObjects.CombatZones.Count > 0)
			{
				CombatZonePositionInfo combatZonePositionInfo = this._starSystemObjects.CombatZones.Last<CombatZonePositionInfo>();
				num7 = Math.Max(combatZonePositionInfo.RadiusUpper - num4, 10000f);
			}
			else
			{
				num7 = Math.Max(125000f - num4, 10000f);
			}
			if (starRadius > 0f)
			{
				num7 = Math.Max(num4 + 7500f + starRadius, num7);
			}
			Vector3 safeRandSpawnPos = Vector3.Zero;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				safeRandSpawnPos.X = this._random.NextInclusive(-num7, num7);
				safeRandSpawnPos.Z = this._random.NextInclusive(-num7, num7);
				if (starRadius > 0f)
				{
					float num8 = starRadius + num4 + 7500f;
					float lengthSquared = safeRandSpawnPos.LengthSquared;
					if (lengthSquared < num8 * num8)
					{
						flag = false;
					}
				}
				if (flag)
				{
					foreach (StellarBody current10 in planetsInSystem)
					{
						float num9 = current10.Parameters.Radius + num4 + 750f;
						float lengthSquared2 = (current10.Parameters.Position - safeRandSpawnPos).LengthSquared;
						if (lengthSquared2 < num9 * num9)
						{
							flag = false;
							break;
						}
						if (!flag)
						{
							break;
						}
					}
				}
			}
			safeRandSpawnPos -= vector;
			this._neutralCombatStanceInfo.FleetAPos.Position += safeRandSpawnPos;
			this._neutralCombatStanceInfo.FleetAAllies.ForEach(delegate(NeutralCombatStanceSpawnPosition x)
			{
				x.Position += safeRandSpawnPos;
			});
			this._neutralCombatStanceInfo.FleetBPos.Position += safeRandSpawnPos;
			this._neutralCombatStanceInfo.FleetBAllies.ForEach(delegate(NeutralCombatStanceSpawnPosition x)
			{
				x.Position += safeRandSpawnPos;
			});
		}
		private SpawnProfile GetSpawnInfoAtNeutralSystem(FleetInfo fleet, SpawnProfile spawnPos, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, ref int spawnedFleetCount)
		{
			if (this._neutralCombatStanceInfo.Stance == NeutralCombatStance.Invalid || this._neutralCombatStanceInfo.Stance == NeutralCombatStance.None)
			{
				return null;
			}
			if (this._neutralCombatStanceInfo.FleetAPos.FleetID == fleet.ID)
			{
				spawnPos._spawnPosition = this._neutralCombatStanceInfo.FleetAPos.Position;
				spawnPos._spawnFacing = this._neutralCombatStanceInfo.FleetAPos.Facing;
				spawnPos._startingPosition = spawnPos._spawnPosition;
				return spawnPos;
			}
			if (this._neutralCombatStanceInfo.FleetBPos.FleetID == fleet.ID)
			{
				spawnPos._spawnPosition = this._neutralCombatStanceInfo.FleetBPos.Position;
				spawnPos._spawnFacing = this._neutralCombatStanceInfo.FleetBPos.Facing;
				spawnPos._startingPosition = spawnPos._spawnPosition;
				return spawnPos;
			}
			NeutralCombatStanceSpawnPosition neutralCombatStanceSpawnPosition = this._neutralCombatStanceInfo.FleetAAllies.FirstOrDefault((NeutralCombatStanceSpawnPosition x) => x.FleetID == fleet.ID);
			if (neutralCombatStanceSpawnPosition == null)
			{
				neutralCombatStanceSpawnPosition = this._neutralCombatStanceInfo.FleetBAllies.FirstOrDefault((NeutralCombatStanceSpawnPosition x) => x.FleetID == fleet.ID);
			}
			if (neutralCombatStanceSpawnPosition != null)
			{
				spawnPos._spawnPosition = neutralCombatStanceSpawnPosition.Position;
				spawnPos._spawnFacing = neutralCombatStanceSpawnPosition.Facing;
				spawnPos._startingPosition = spawnPos._spawnPosition;
				return spawnPos;
			}
			return null;
		}
		private SpawnProfile GetSpawnInfoFromCombatZone(FleetInfo fleet, SpawnProfile spawnPos, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, ref int spawnedFleetCount)
		{
			StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(this._systemId);
			if (starSystemInfo.ControlZones != null)
			{
				float num = 3.40282347E+38f;
				CombatZonePositionInfo czpi = null;
				Vector3 bestPlanetPos = Vector3.Zero;
				List<StarModel> list = new List<StarModel>();
				List<StellarBody> list2 = new List<StellarBody>();
				foreach (IGameObject current in (
					from x in this._starSystemObjects.Crits.Objects
					where x is StellarBody || x is StarModel
					select x).ToList<IGameObject>())
				{
					if (current is StarModel)
					{
						list.Add(current as StarModel);
					}
					else
					{
						if (current is StellarBody)
						{
							StellarBody stellarBody = current as StellarBody;
							if (stellarBody != null)
							{
								list2.Add(stellarBody);
							}
							ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(stellarBody.Parameters.OrbitalID);
							if (stellarBody != null && colonyInfoForPlanet != null && this.GetDiplomacyState(fleet.PlayerID, colonyInfoForPlanet.PlayerID) == DiplomacyState.WAR)
							{
								CombatZonePositionInfo closestZoneToPosition = this._starSystemObjects.GetClosestZoneToPosition(base.App, fleet.PlayerID, stellarBody.Parameters.Position);
								if (closestZoneToPosition == null)
								{
									break;
								}
								float lengthSquared = (closestZoneToPosition.Center - stellarBody.Parameters.Position).LengthSquared;
								if (lengthSquared < num)
								{
									num = lengthSquared;
									czpi = closestZoneToPosition;
									bestPlanetPos = stellarBody.Parameters.Position;
								}
							}
						}
					}
				}
				return this.GetSpawnProfileFromCombatZone(spawnPos, czpi, bestPlanetPos, list2, list);
			}
			return null;
		}
		private SpawnProfile GetSpawnInfoForDefaultControlZone(FleetInfo fleet, SpawnProfile spawnPos, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, ref int spawnedFleetCount)
		{
			List<CombatZonePositionInfo> list = (
				from x in this._starSystemObjects.CombatZones
				where x.Player == fleet.PlayerID
				select x).ToList<CombatZonePositionInfo>();
			if (list.Count > 0)
			{
				list.Sort((CombatZonePositionInfo x, CombatZonePositionInfo y) => x.Center.LengthSquared.CompareTo(y.Center.LengthSquared));
				float num = -1f;
				int num2 = 0;
				foreach (CombatZonePositionInfo current in list)
				{
					if (num < 0f)
					{
						num = current.Center.LengthSquared;
					}
					else
					{
						if (current.Center.LengthSquared > num)
						{
							break;
						}
						num2++;
					}
				}
				if (num > 0f)
				{
					List<StarModel> list2 = new List<StarModel>();
					List<StellarBody> list3 = new List<StellarBody>();
					foreach (IGameObject current2 in (
						from x in this._starSystemObjects.Crits.Objects
						where x is StellarBody || x is StarModel
						select x).ToList<IGameObject>())
					{
						if (current2 is StarModel)
						{
							list2.Add(current2 as StarModel);
						}
						else
						{
							if (current2 is StellarBody)
							{
								list3.Add(current2 as StellarBody);
							}
						}
					}
					int index = this._random.NextInclusive(0, num2);
					return this.GetSpawnProfileFromCombatZone(spawnPos, list[index], Vector3.Zero, list3, list2);
				}
			}
			return null;
		}
		private SpawnProfile GetSpawnInfoForFriendlySystem(FleetInfo fleet, SpawnProfile spawnPos, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, ref int spawnedFleetCount)
		{
			if (this._systemId != 0)
			{
				PlanetInfo[] starSystemPlanetInfos = base.App.GameDatabase.GetStarSystemPlanetInfos(this._systemId);
				for (int i = 0; i < starSystemPlanetInfos.Length; i++)
				{
					PlanetInfo planetInfo = starSystemPlanetInfos[i];
					ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
					if (colonyInfoForPlanet != null && base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleet.PlayerID, colonyInfoForPlanet.PlayerID) == DiplomacyState.ALLIED)
					{
						Vector3 position = base.App.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position;
						float num = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
						spawnPos._startingPosition = position;
						Vector3 v = Vector3.Normalize(spawnPos._startingPosition) * (num + 7000f + 1000f * spawnedFleetCount);
						spawnPos._startingPosition += v;
						spawnPos._spawnFacing = Vector3.Normalize(spawnPos._startingPosition);
						List<Ship> stationsAroundPlanet = this._starSystemObjects.GetStationsAroundPlanet(planetInfo.ID);
						if (stationsAroundPlanet.Count > 0)
						{
							foreach (Ship current in stationsAroundPlanet)
							{
								StationInfo stationInfo = this._starSystemObjects.GetStationInfo(current);
								if (stationInfo != null && stationInfo.DesignInfo.StationType == StationType.NAVAL && stationInfo.DesignInfo.StationLevel > 0)
								{
									Vector3 v2 = current.Position - position;
									v2.Normalize();
									spawnPos._startingPosition = current.Position + v2 * 3500f;
									v = Vector3.Normalize(spawnPos._startingPosition) * (2000f + 1000f * spawnedFleetCount);
									spawnPos._startingPosition += v;
								}
							}
						}
						spawnPos._spawnPosition = spawnPos._startingPosition;
						return spawnPos;
					}
				}
			}
			return null;
		}
		private SpawnProfile GetSpawnProfileFromCombatZone(SpawnProfile spawnPos, CombatZonePositionInfo czpi, Vector3 bestPlanetPos, List<StellarBody> planets, List<StarModel> stars)
		{
			if (czpi != null)
			{
				float num = MathHelper.DegreesToRadians(5f);
				Vector3 vector = czpi.Center;
				bool flag = true;
				int num2 = 30;
				while (!flag && num2 > 0)
				{
					flag = true;
					float yawRadians = this._random.NextInclusive(czpi.AngleLeft + num, czpi.AngleRight - num);
					float s = this._random.NextInclusive(czpi.RadiusLower + 2000f, czpi.RadiusUpper - 2000f);
					vector = Matrix.CreateRotationYPR(yawRadians, 0f, 0f).Forward * s;
					foreach (StarModel current in stars)
					{
						float num3 = current.Radius + 2000f + 7500f;
						float lengthSquared = (current.Position - vector).LengthSquared;
						if (lengthSquared < num3 * num3)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						foreach (StellarBody current2 in planets)
						{
							float num4 = current2.Parameters.Radius + 2000f + 750f;
							float lengthSquared2 = (current2.Parameters.Position - vector).LengthSquared;
							if (lengthSquared2 < num4 * num4)
							{
								flag = false;
								break;
							}
						}
					}
					num2--;
				}
				if (flag)
				{
					spawnPos._spawnPosition = vector;
					spawnPos._spawnFacing = bestPlanetPos - vector;
					spawnPos._spawnFacing.Normalize();
					spawnPos._startingPosition = vector;
					return spawnPos;
				}
			}
			return null;
		}
		private SpawnProfile GetSpawnProfileForEnterSystem(FleetInfo fleet, Faction faction, SpawnProfile spawnPos)
		{
			if (!fleet.PreviousSystemID.HasValue || fleet.PreviousSystemID == this._systemId)
			{
				return null;
			}
			EntrySpawnLocation entrySpawnLocation = this._entryLocations.FirstOrDefault((EntrySpawnLocation x) => x.PreviousSystemID == fleet.PreviousSystemID.Value && x.FactionID == faction.ID);
			if (entrySpawnLocation == null)
			{
				return null;
			}
			spawnPos._spawnFacing = -Vector3.Normalize(entrySpawnLocation.Position);
			if (faction.Name == "zuul" && base.App.GameDatabase.PlayerHasTech(fleet.PlayerID, "DRV_Star_Tear"))
			{
				spawnPos._startingPosition = -(spawnPos._spawnFacing * (this._starSystemObjects.GetStarRadius() + faction.StarTearTechEnteryPointOffset));
			}
			else
			{
				spawnPos._spawnPosition = entrySpawnLocation.Position - spawnPos._spawnFacing * (faction.EntryPointOffset + 1500f);
			}
			return this.GetSafeEntryPosition(spawnPos);
		}
		private SpawnProfile GetSafeEntryPosition(SpawnProfile spawnPos)
		{
			bool flag = true;
			foreach (SpawnProfile current in this._spawnPositions)
			{
				if (current.SpawnProfileOverlaps(spawnPos))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				flag = true;
				spawnPos._spawnPosition.Y = spawnPos._spawnPosition.Y - 400f;
				foreach (SpawnProfile current2 in this._spawnPositions)
				{
					if (current2.SpawnProfileOverlaps(spawnPos))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				flag = true;
				spawnPos._spawnPosition.Y = spawnPos._spawnPosition.Y + 800f;
				foreach (SpawnProfile current3 in this._spawnPositions)
				{
					if (current3.SpawnProfileOverlaps(spawnPos))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				spawnPos._spawnPosition.Y = spawnPos._spawnPosition.Y - 400f;
				while (!flag)
				{
					flag = true;
					spawnPos._spawnPosition.X = spawnPos._spawnPosition.X + 3000f;
					foreach (SpawnProfile current4 in this._spawnPositions)
					{
						if (current4.SpawnProfileOverlaps(spawnPos))
						{
							flag = false;
							break;
						}
					}
				}
			}
			spawnPos._startingPosition = spawnPos._spawnPosition;
			return spawnPos;
		}
		private SpawnProfile GetSpawnProfileForInitialAttack(FleetInfo fleet, SpawnProfile spawnPos, PlanetInfo furthestTarget, float furthestOrbitDist, float edgeDistance, IDictionary<int, List<FleetInfo>> selectedPlayerFleets, ref int spawnedFleetCount)
		{
			if (furthestTarget == null)
			{
				return null;
			}
			Matrix orbitalTransform = base.App.GameDatabase.GetOrbitalTransform(furthestTarget.ID);
			int num = this._starSystemObjects.GetCombatZoneRingAtRange(furthestOrbitDist) + 1;
			int combatZoneInRing = this._starSystemObjects.GetCombatZoneInRing(num, orbitalTransform.Position);
			int num2 = Math.Min(Math.Max(this._playersInCombat.Count / 2, 1), 3);
			int num3 = combatZoneInRing - num2;
			int num4 = num2 * 2 + 1;
			List<int> list = new List<int>();
			for (int i = 0; i < num4; i++)
			{
				int num5 = num3 + i;
				if (num5 < 0)
				{
					num5 = StarSystem.CombatZoneMapAngleDivs[num] + num5;
				}
				list.Add(num5);
			}
			foreach (SpawnProfile current in this._spawnPositions)
			{
				CombatZonePositionInfo closestZoneToPosition = this._starSystemObjects.GetClosestZoneToPosition(base.App, 0, current._spawnPosition);
				if (closestZoneToPosition != null && closestZoneToPosition.RingIndex == num)
				{
					foreach (int current2 in list)
					{
						if (current2 == closestZoneToPosition.ZoneIndex)
						{
							list.Remove(current2);
							break;
						}
					}
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			int index = this._random.NextInclusive(0, list.Count - 1);
			CombatZonePositionInfo combatZonePositionInfo = this._starSystemObjects.GetCombatZonePositionInfo(num, list[index]);
			if (combatZonePositionInfo == null)
			{
				return null;
			}
			spawnPos._spawnPosition = Vector3.Normalize(combatZonePositionInfo.Center) * (combatZonePositionInfo.RadiusLower + edgeDistance + spawnedFleetCount * 1000f);
			spawnPos._spawnFacing = Vector3.Normalize(orbitalTransform.Position - spawnPos._spawnPosition);
			spawnPos._startingPosition = spawnPos._spawnPosition;
			return spawnPos;
		}
		private int SelectOriginOrbital(int systemId)
		{
			if (systemId == 0)
			{
				return 0;
			}
			PlanetInfo[] starSystemPlanetInfos = base.App.GameDatabase.GetStarSystemPlanetInfos(systemId);
			PlanetInfo planetInfo = starSystemPlanetInfos.FirstOrDefault((PlanetInfo x) => base.App.GameDatabase.GetColonyInfoForPlanet(x.ID) != null);
			if (planetInfo != null)
			{
				return planetInfo.ID;
			}
			PlanetInfo planetInfo2 = starSystemPlanetInfos.FirstOrDefault((PlanetInfo x) => StellarBodyTypes.IsTerrestrial(x.Type.ToLowerInvariant()));
			if (planetInfo2 != null)
			{
				return planetInfo2.ID;
			}
			PlanetInfo planetInfo3 = starSystemPlanetInfos.FirstOrDefault((PlanetInfo x) => x.Type.ToLowerInvariant() == StellarBodyTypes.Gaseous);
			if (planetInfo3 != null)
			{
				return planetInfo3.ID;
			}
			return 0;
		}
		protected override void OnEnter()
		{
			this._subState = CommonCombatState.CombatSubState.Running;
			this._combatEndingStatsGathered = false;
			this._combatEndDelayComplete = false;
			this._camera.MinDistance = 6f;
			this._camera.DesiredDistance = 1000f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-215f);
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-10f);
			this._camera.MaxDistance = 2000000f;
			this._grid.GridSize = 5000f;
			this._grid.CellSize = 500f;
			int num = 0;
			List<Player> list = new List<Player>();
			foreach (CombatAI current in this.AI_Commanders)
			{
				if (current.m_Player.IsStandardPlayer && current.m_bIsHumanPlayerControlled)
				{
					num++;
					list.Add(current.m_Player);
				}
			}
			List<StellarBody> list2 = this._starSystemObjects.Crits.Objects.OfType<StellarBody>().ToList<StellarBody>();
			foreach (StellarBody p in list2)
			{
				if (!list.Any((Player x) => x.ID == p.Parameters.ColonyPlayerID))
				{
					Player player = base.App.GetPlayer(p.Parameters.ColonyPlayerID);
					if (this.isHumanControlledAI(player))
					{
						num++;
						list.Add(player);
					}
				}
			}
			this._input.PlayerId = base.App.LocalPlayer.ObjectID;
			this._input.CameraID = this._camera.ObjectID;
			this._input.CombatGridID = this._grid.ObjectID;
			this._input.CombatSensorID = this._sensor.ObjectID;
			this._input.CombatID = this._combat.ObjectID;
			this._input.EnableTimeScale = (num == 1);
			foreach (IActive current2 in this._crits.OfType<IActive>())
			{
				current2.Active = true;
			}
			foreach (SpawnProfile current3 in this._spawnPositions)
			{
				foreach (int current4 in current3._activeShips)
				{
					if (this._ships.Forward.ContainsKey(current4))
					{
						this._ships.Forward[current4].Active = true;
					}
				}
			}
			IEnumerable<IGameObject> enumerable = 
				from x in this._crits.Objects
				where x is Ship
				select x;
			GameObject gameObject = null;
			using (IEnumerator<IGameObject> enumerator6 = enumerable.GetEnumerator())
			{
				while (enumerator6.MoveNext())
				{
					Ship ship = (Ship)enumerator6.Current;
					if (ship.Player.ID == base.App.LocalPlayer.ID && (gameObject == null || ((gameObject as Ship).ShipClass == ShipClass.Station && ship.ShipClass != ShipClass.Station)))
					{
						gameObject = ship;
					}
					ship.SyncAltitude();
				}
			}
			if (gameObject == null)
			{
				IEnumerable<IGameObject> enumerable2 = 
					from x in this._starSystemObjects.Crits.Objects
					where x is StellarBody
					select x;
				foreach (IGameObject current5 in enumerable2)
				{
					StellarBody stellarBody = current5 as StellarBody;
					if (stellarBody != null && stellarBody.Parameters.ColonyPlayerID == base.App.LocalPlayer.ID)
					{
						gameObject = stellarBody;
						break;
					}
				}
			}
			if (gameObject != null)
			{
				this._camera.TargetID = gameObject.ObjectID;
			}
			string text = string.Format("Combat_{0}", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)));
			text += (this.isSmallScaleCombat() ? "_low" : "");
			base.App.PostPlayMusic(text);
			this.m_SlewMode = false;
			this._combat.PostSetProp("SetSlewMode", this.m_SlewMode);
			bool flag = false;
			foreach (Player current6 in this._playersInCombat)
			{
				foreach (Player current7 in this._playersInCombat)
				{
					if (this.GetDiplomacyState(current6.ID, current7.ID) == DiplomacyState.WAR)
					{
						flag = true;
					}
				}
			}
			this._combatStanceMap.Clear();
			foreach (Player current8 in this._playersInCombat)
			{
				this._combatStanceMap.Add(current8.ID, new Dictionary<int, bool>());
				foreach (Player current9 in this._playersInCombat)
				{
					if (current8 != current9)
					{
						DiplomacyState diplomacyState = this.GetDiplomacyState(current8.ID, current9.ID);
						this._combatStanceMap[current8.ID].Add(current9.ID, diplomacyState != DiplomacyState.WAR && (flag || diplomacyState != DiplomacyState.NEUTRAL));
					}
				}
			}
			if (!this.SimMode)
			{
				this._camera.DesiredDistance = 250f;
				Vector3 vector = Vector3.DegreesToRadians(new Vector3(0f, -30f, 0f));
				Vector3 vector2 = Vector3.DegreesToRadians(new Vector3(0f, -30f, 0f));
				this._camera.PostSetProp("SinglePassAttractMode", new object[]
				{
					true,
					this._camera.TargetID,
					5,
					vector,
					vector2,
					15f,
					15f
				});
				this._input.PostSetProp("HookHotKeys", base.App.HotKeyManager.GetObjectID());
			}
			this._combat.PostSetProp("CombatStart", this.SimMode || !base.App.GameSetup.IsMultiplayer);
			if (this._lastPendingCombat.SelectedPlayerFleets.ContainsKey(base.App.Game.ScriptModules.VonNeumann.PlayerID))
			{
				List<ShipInfo> source = base.App.GameDatabase.GetShipInfoByFleetID(this._lastPendingCombat.SelectedPlayerFleets[base.App.Game.ScriptModules.VonNeumann.PlayerID], true).ToList<ShipInfo>();
				if (source.Any((ShipInfo x) => x.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId))
				{
					using (List<int>.Enumerator enumerator12 = this._lastPendingCombat.PlayersInCombat.GetEnumerator())
					{
						while (enumerator12.MoveNext())
						{
							int current10 = enumerator12.Current;
							base.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_VN_COLLECTOR_ATTACK,
								EventMessage = TurnEventMessage.EM_VN_COLLECTOR_ATTACK,
								PlayerID = current10,
								SystemID = this._lastPendingCombat.SystemID,
								TurnNumber = base.App.GameDatabase.GetTurnCount()
							});
						}
						goto IL_A9E;
					}
				}
				if (source.Any((ShipInfo x) => x.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.SeekerMothership].DesignId))
				{
					using (List<int>.Enumerator enumerator13 = this._lastPendingCombat.PlayersInCombat.GetEnumerator())
					{
						while (enumerator13.MoveNext())
						{
							int current11 = enumerator13.Current;
							base.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_VN_SEEKER_ATTACK,
								EventMessage = TurnEventMessage.EM_VN_SEEKER_ATTACK,
								PlayerID = current11,
								SystemID = this._lastPendingCombat.SystemID,
								TurnNumber = base.App.GameDatabase.GetTurnCount()
							});
						}
						goto IL_A9E;
					}
				}
				if (source.Any((ShipInfo x) => x.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.BerserkerMothership].DesignId))
				{
					using (List<int>.Enumerator enumerator14 = this._lastPendingCombat.PlayersInCombat.GetEnumerator())
					{
						while (enumerator14.MoveNext())
						{
							int current12 = enumerator14.Current;
							base.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_VN_BERSERKER_ATTACK,
								EventMessage = TurnEventMessage.EM_VN_BERSERKER_ATTACK,
								PlayerID = current12,
								SystemID = this._lastPendingCombat.SystemID,
								TurnNumber = base.App.GameDatabase.GetTurnCount()
							});
						}
						goto IL_A9E;
					}
				}
				if (source.Any((ShipInfo x) => x.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.PlanetKiller].DesignId))
				{
					foreach (int current13 in this._lastPendingCombat.PlayersInCombat)
					{
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_VN_SYS_KILLER_ATTACK,
							EventMessage = TurnEventMessage.EM_VN_SYS_KILLER_ATTACK,
							PlayerID = current13,
							SystemID = this._lastPendingCombat.SystemID,
							TurnNumber = base.App.GameDatabase.GetTurnCount()
						});
					}
				}
			}
			IL_A9E:
			if (!this._pirateEncounterData.IsPirateEncounter && base.App.Game != null && base.App.Game.ScriptModules != null && !this._playersInCombat.Any((Player x) => base.App.Game.ScriptModules.IsEncounterPlayer(x.ID)))
			{
				Player player2 = this._playersInCombat.FirstOrDefault((Player x) => x == base.App.Game.LocalPlayer);
				if (player2 != null)
				{
					List<Ship> list3 = (
						from x in enumerable.OfType<Ship>()
						where x.IsSuulka
						select x).ToList<Ship>();
					foreach (Ship current14 in list3)
					{
						if (current14.Player != player2 && this.GetDiplomacyState(player2.ID, current14.Player.ID) == DiplomacyState.WAR)
						{
							base.App.PostRequestSpeech(string.Format("COMBAT_126-01_{0}_EnterBattleWithSuulka", player2.Faction.Name), 50, 20, 0f);
							break;
						}
					}
				}
			}
		}
		protected bool isSmallScaleCombat()
		{
			IEnumerable<IGameObject> source = 
				from x in this._crits.Objects
				where x is Ship
				select x;
			return source.Count<IGameObject>() < base.App.AssetDatabase.LargeCombatThreshold;
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this._starSystemObjects = null;
			this.m_SlewMode = false;
			if (!this._sim)
			{
				this._input.PostSetProp("UnHookHotKeys", base.App.HotKeyManager.GetObjectID());
			}
			if (this._crits != null)
			{
				Player[] array = GameSession.GetPlayersWithCombatAssets(base.App, this._systemId).ToArray<Player>();
				foreach (IGameObject current in this._crits)
				{
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (ship.IsDestroyed)
						{
							Player[] array2 = array;
							for (int i = 0; i < array2.Length; i++)
							{
								Player player = array2[i];
								if (base.App.GameDatabase.GetPlayerInfo(player.ID).isStandardPlayer && player.ID != ship.Player.ID)
								{
									int iD = player.ID;
									if (base.App.GameDatabase.GetPlayerInfo(ship.Player.ID).isStandardPlayer)
									{
										base.App.GameDatabase.ApplyDiplomacyReaction(ship.Player.ID, iD, StratModifiers.DiplomacyReactionKillShips, ship.GetCruiserEquivalent());
									}
									foreach (int current2 in base.App.GameDatabase.GetStandardPlayerIDs())
									{
										if (current2 != ship.Player.ID && current2 != player.ID && base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(current2, ship.Player.ID) == DiplomacyState.WAR)
										{
											base.App.GameDatabase.ApplyDiplomacyReaction(current2, iD, StratModifiers.DiplomacyReactionKillEnemy, ship.GetCruiserEquivalent());
										}
									}
								}
							}
							if (ship.Faction.Name == "zuul" && ship.ShipClass == ShipClass.Leviathan)
							{
								foreach (int current3 in base.App.GameDatabase.GetPlayerIDs())
								{
									if (current3 != ship.Player.ID)
									{
										Player[] array3 = array;
										for (int j = 0; j < array3.Length; j++)
										{
											Player player2 = array3[j];
											if (current3 != player2.ID)
											{
												base.App.GameDatabase.ApplyDiplomacyReaction(current3, player2.ID, StratModifiers.DiplomacyReactionKillSuulka, 1);
											}
										}
									}
								}
							}
						}
					}
				}
				this._crits.Dispose();
				this._crits = null;
			}
			base.App.ObjectReleased -= new Action<IGameObject>(this.Game_ObjectReleased);
			this._ships = null;
			foreach (CombatAI current4 in this.AI_Commanders)
			{
				current4.Shutdown();
			}
			this.AI_Commanders.Clear();
			this._pirateEncounterData.Clear();
			this._trapCombatData.Clear();
			this._spawnPositions.Clear();
			this._hitByNodeCannon.Clear();
			this._mediaHeroFleets.Clear();
			this._pointsOfInterest.Clear();
			this._playersInCombat.Clear();
			this._ignoreCombatZonePlayers.Clear();
			this._playersWithAssets.Clear();
			this._combatStanceMap.Clear();
			if (this._entryLocations != null)
			{
				this._entryLocations.Clear();
			}
			if (this._fleetsPerPlayer != null)
			{
				this._fleetsPerPlayer.Clear();
			}
			if (this._ships != null)
			{
				this._ships.Clear();
			}
			if (this._initialDiploStates != null)
			{
				this._initialDiploStates.Clear();
			}
			this._combatConfig = null;
			this._gameObjectConfigs = null;
		}
		public bool EndCombat()
		{
			if (this._subState == CommonCombatState.CombatSubState.Ending || this._subState == CommonCombatState.CombatSubState.Ended)
			{
				return false;
			}
			List<SwarmerInfo> list = base.App.GameDatabase.GetSwarmerInfos().ToList<SwarmerInfo>();
			foreach (SwarmerInfo current in list)
			{
				if (current.QueenFleetId.HasValue)
				{
					FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(current.QueenFleetId.Value);
					if (fleetInfo != null && fleetInfo.SystemID == this._systemId)
					{
						Swarmers.ClearTransform(base.App.GameDatabase, current);
					}
				}
			}
			this._subState = CommonCombatState.CombatSubState.Ending;
			base.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_COMBAT_ENDED
			});
			this.OnCombatEnding();
			return true;
		}
		private void UpdateRunning()
		{
			List<IGameObject> list = new List<IGameObject>();
			foreach (IGameObject current in this._postLoadedObjects)
			{
				if (current.ObjectStatus == GameObjectStatus.Ready && !this._isPaused)
				{
					if (current is IActive)
					{
						(current as IActive).Active = true;
					}
					this._crits.Add(current);
					list.Add(current);
					if (current is PointOfInterest)
					{
						this._pointsOfInterest.Add(current as PointOfInterest);
					}
				}
			}
			if (list.Count > 0)
			{
				this._combat.PostObjectAddObjects(list.ToArray());
			}
			foreach (IGameObject current2 in list)
			{
				this._postLoadedObjects.Remove(current2);
			}
			if (!this._isPaused && this._engCombatActive)
			{
				List<IGameObject> combatGameObjects = CombatAI.GetCombatGameObjects(this.Objects);
				this._detectionUpdateRate--;
				if (this._detectionUpdateRate <= 0)
				{
					this.DetectionUpdate(combatGameObjects);
					this._detectionUpdateRate = 3;
				}
				if (!this._testingState && this.CheckVictory() && this._authority)
				{
					this.EndCombat();
				}
				foreach (CombatAI current3 in this.AI_Commanders)
				{
					current3.Update(combatGameObjects);
				}
			}
		}
		protected virtual GameState GetExitState()
		{
			return base.App.PreviousState;
		}
		private bool UpdateEnding()
		{
			if (this._combatEndingStatsGathered && this._combatEndDelayComplete)
			{
				GameState exitState = this.GetExitState();
				GameState value;
				if (exitState != this)
				{
					value = (exitState ?? base.App.GetGameState<MainMenuState>());
				}
				else
				{
					value = base.App.GetGameState<StarMapState>();
				}
				base.App.SwitchGameState(value, new object[0]);
				return true;
			}
			return false;
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_START_SENDINGDATA(ScriptMessageReader mr)
		{
			this._combatData = base.App.Game.CombatData.AddCombat(this._combatId, this._systemId, base.App.GameDatabase.GetTurnCount());
			this._combatEndingStatsGathered = false;
			this._lastPendingCombat.CombatResults = new PostCombatData();
			foreach (Player current in GameSession.GetPlayersWithCombatAssets(base.App, this._lastPendingCombat.SystemID))
			{
				this._lastPendingCombat.CombatResults.PlayersInCombat.Add(current.ID);
			}
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_SHIP(ScriptMessageReader mr)
		{
			int id = mr.ReadInteger();
			ShipClass shipClass = (ShipClass)mr.ReadInteger();
			Vector3 position = default(Vector3);
			Vector3 forward = default(Vector3);
			position.X = mr.ReadSingle();
			position.Y = mr.ReadSingle();
			position.Z = mr.ReadSingle();
			forward.X = mr.ReadSingle();
			forward.Y = mr.ReadSingle();
			forward.Z = mr.ReadSingle();
			int num = mr.ReadInteger();
			float num2 = mr.ReadSingle();
			float num3 = 0f;
			int power = mr.ReadInteger();
			double num4 = mr.ReadDouble();
			Ship gameObject = base.App.GetGameObject<Ship>(id);
			ShipInfo si = base.App.GameDatabase.GetShipInfo(gameObject.DatabaseID, true);
			List<SectionInstanceInfo> source = new List<SectionInstanceInfo>();
			if (si != null)
			{
				if (shipClass != ShipClass.Station && shipClass != ShipClass.BattleRider)
				{
					FleetInfo fi = base.App.GameDatabase.GetFleetInfo(si.FleetID);
					if (fi != null && !this._lastPendingCombat.CombatResults.FleetsInCombat.Any((FleetInfo x) => x.ID == fi.ID))
					{
						this._lastPendingCombat.CombatResults.FleetsInCombat.Add(fi);
					}
				}
				CommonCombatState.Trace(string.Format("- Ship Combat Data Id: {0} Class: {1} kill Count: {2}, damageApplied: {3} psionicPowerRemaining:{4} -", new object[]
				{
					gameObject.DatabaseID,
					shipClass,
					num,
					num2,
					num3
				}));
				if (num4 > 0.0)
				{
					base.App.GameDatabase.UpdateShipObtainedSlaves(si.ID, num4);
				}
				if (!si.DesignInfo.isAttributesDiscovered)
				{
					si.DesignInfo.isAttributesDiscovered = true;
					List<SectionEnumerations.DesignAttribute> list = base.App.GameDatabase.GetDesignAttributesForDesign(si.DesignID).ToList<SectionEnumerations.DesignAttribute>();
					foreach (SectionEnumerations.DesignAttribute current in list)
					{
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_ATTRIBUTES_DISCOVERED,
							EventMessage = TurnEventMessage.EM_ATTRIBUTES_DISCOVERED,
							PlayerID = si.DesignInfo.PlayerID,
							DesignID = si.DesignID,
							DesignAttribute = current,
							TurnNumber = base.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					base.App.GameDatabase.UpdateDesignAttributeDiscovered(si.DesignInfo.ID, true);
				}
				source = base.App.GameDatabase.GetShipSectionInstances(si.ID).ToList<SectionInstanceInfo>();
				base.App.GameDatabase.UpdateShipPsionicPower(si.ID, power);
				if (gameObject.HasRetreated)
				{
					FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(si.FleetID);
					PlayerInfo playerInfo = (fleetInfo != null) ? base.App.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID) : null;
					if (playerInfo != null && playerInfo.isStandardPlayer && !fleetInfo.IsDefenseFleet && !fleetInfo.IsGateFleet)
					{
						List<int> list2 = base.App.GameDatabase.GetStandardPlayerIDs().ToList<int>();
						if (list2.Contains(fleetInfo.PlayerID))
						{
							int num5 = base.App.Game.MCCarryOverData.GetRetreatFleetID(fleetInfo.SystemID, fleetInfo.ID);
							if (num5 == 0 || num5 != fleetInfo.ID)
							{
								if (num5 == 0)
								{
									num5 = base.App.GameDatabase.InsertFleet(fleetInfo.PlayerID, 0, fleetInfo.SystemID, fleetInfo.SupportingSystemID, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL);
									int missionID = base.App.GameDatabase.InsertMission(num5, MissionType.RETREAT, 0, 0, 0, 0, false, null);
									base.App.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, null);
									base.App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, null);
									base.App.Game.MCCarryOverData.SetRetreatFleetID(fleetInfo.SystemID, fleetInfo.ID, num5);
								}
								List<ShipInfo> list3 = base.App.GameDatabase.GetShipInfoByFleetID(si.FleetID, false).ToList<ShipInfo>();
								list3.RemoveAll((ShipInfo x) => x.ID == si.ID);
								int shipsCommandPointQuota = base.App.GameDatabase.GetShipsCommandPointQuota(list3);
								int shipsCommandPointCost = base.App.GameDatabase.GetShipsCommandPointCost(list3);
								if (shipsCommandPointQuota < shipsCommandPointCost)
								{
									if (num5 != 0)
									{
										List<int> list4 = base.App.GameDatabase.GetShipsByFleetID(num5).ToList<int>();
										foreach (int current2 in list4)
										{
											base.App.GameDatabase.TransferShip(current2, fleetInfo.ID);
										}
										MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(num5);
										if (missionByFleetID != null)
										{
											base.App.GameDatabase.RemoveMission(missionByFleetID.ID);
										}
										base.App.GameDatabase.RemoveFleet(num5);
										base.App.Game.MCCarryOverData.SetRetreatFleetID(fleetInfo.SystemID, fleetInfo.ID, fleetInfo.ID);
									}
									MissionInfo missionByFleetID2 = base.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
									if (missionByFleetID2 != null)
									{
										base.App.GameDatabase.RemoveMission(missionByFleetID2.ID);
									}
									int missionID2 = base.App.GameDatabase.InsertMission(fleetInfo.ID, MissionType.RETREAT, 0, 0, 0, 0, false, null);
									base.App.GameDatabase.InsertWaypoint(missionID2, WaypointType.ReturnHome, null);
									this.CheckToReturnControlZonesToOwner(si.DesignInfo.PlayerID);
								}
								else
								{
									base.App.GameDatabase.TransferShip(si.ID, num5);
								}
							}
						}
					}
				}
				else
				{
					if (gameObject.HitByNodeCannon)
					{
						this._hitByNodeCannon.Add(gameObject);
					}
				}
			}
			int num6 = mr.ReadInteger();
			for (int i = 0; i < num6; i++)
			{
				int num7 = mr.ReadInteger();
				SectionInstanceInfo sectionInstance = null;
				if (si != null)
				{
					foreach (Kerberos.Sots.GameObjects.Section s in gameObject.Sections)
					{
						if (s.ObjectID == num7)
						{
							DesignSectionInfo dsi = si.DesignInfo.DesignSections.First((DesignSectionInfo x) => x.ShipSectionAsset.Type == s.ShipSectionAsset.Type);
							sectionInstance = source.FirstOrDefault((SectionInstanceInfo x) => x.SectionID == dsi.ID);
							break;
						}
					}
				}
				float num8 = mr.ReadSingle();
				num3 += num8;
				float num9 = mr.ReadSingle();
				int crew = mr.ReadInteger();
				int supply = mr.ReadInteger();
				int num10 = mr.ReadInteger();
				for (int j = 0; j < num10; j++)
				{
					bool flag = mr.ReadInteger() == 1;
					if (flag)
					{
						DamagePattern value = DamagePattern.Read(mr);
						if (si != null)
						{
							sectionInstance.Armor[(ArmorSide)j] = value;
						}
					}
				}
				List<WeaponInstanceInfo> list5 = new List<WeaponInstanceInfo>();
				List<ModuleInstanceInfo> list6 = new List<ModuleInstanceInfo>();
				if (si != null)
				{
					if (!ShipSectionAsset.IsWeaponBattleRiderClass(gameObject.RealShipClass))
					{
						DesignSectionInfo designSectionInfo = si.DesignInfo.DesignSections.First((DesignSectionInfo x) => x.ID == sectionInstance.SectionID);
						int minStructure = designSectionInfo.GetMinStructure(base.App.GameDatabase, base.App.AssetDatabase);
						sectionInstance.Structure = Math.Max((int)Math.Ceiling((double)num9), minStructure);
						sectionInstance.Crew = crew;
						sectionInstance.Supply = supply;
					}
					base.App.GameDatabase.UpdateArmorInstances(sectionInstance.ID, sectionInstance.Armor);
					base.App.GameDatabase.UpdateSectionInstance(sectionInstance);
					list6 = base.App.GameDatabase.GetModuleInstances(sectionInstance.ID).ToList<ModuleInstanceInfo>();
					list5 = base.App.GameDatabase.GetWeaponInstances(sectionInstance.ID).ToList<WeaponInstanceInfo>();
				}
				int num11 = mr.ReadInteger();
				List<int> list7 = (
					from x in list6
					select x.ID).ToList<int>();
				for (int k = 0; k < num11; k++)
				{
					int id2 = mr.ReadInteger();
					float num12 = mr.ReadSingle();
					if (list6.Count != 0)
					{
						Module m = base.App.GetGameObject<Module>(id2);
						if (m != null)
						{
							ModuleInstanceInfo moduleInstanceInfo = list6.FirstOrDefault((ModuleInstanceInfo x) => x.ModuleNodeID == m.Attachment.NodeName);
							if (moduleInstanceInfo != null)
							{
								moduleInstanceInfo.Structure = Math.Max((int)Math.Ceiling((double)num12), 0);
								base.App.GameDatabase.UpdateModuleInstance(moduleInstanceInfo);
								list7.Remove(moduleInstanceInfo.ID);
							}
						}
					}
				}
				foreach (int deadModID in list7)
				{
					ModuleInstanceInfo moduleInstanceInfo2 = list6.FirstOrDefault((ModuleInstanceInfo x) => x.ID == deadModID);
					if (moduleInstanceInfo2 != null)
					{
						moduleInstanceInfo2.Structure = 0;
						base.App.GameDatabase.UpdateModuleInstance(moduleInstanceInfo2);
					}
				}
				int num13 = mr.ReadInteger();
				list7 = (
					from x in list5
					select x.ID).ToList<int>();
				for (int l = 0; l < num13; l++)
				{
					int num14 = mr.ReadInteger();
					for (int n = 0; n < num14; n++)
					{
						int id3 = mr.ReadInteger();
						float num15 = mr.ReadSingle();
						if (list5.Count != 0)
						{
							Turret t = base.App.GetGameObject<Turret>(id3);
							if (t != null)
							{
								WeaponInstanceInfo weaponInstanceInfo = list5.FirstOrDefault((WeaponInstanceInfo x) => x.NodeName == t.NodeName);
								if (weaponInstanceInfo != null)
								{
									weaponInstanceInfo.Structure = Math.Max((float)Math.Ceiling((double)num15), 0f);
									base.App.GameDatabase.UpdateWeaponInstance(weaponInstanceInfo);
									list7.Remove(weaponInstanceInfo.ID);
								}
							}
						}
					}
				}
				foreach (int deadWepID in list7)
				{
					WeaponInstanceInfo weaponInstanceInfo2 = list5.FirstOrDefault((WeaponInstanceInfo x) => x.ID == deadWepID);
					if (weaponInstanceInfo2 != null)
					{
						weaponInstanceInfo2.Structure = 0f;
						base.App.GameDatabase.UpdateWeaponInstance(weaponInstanceInfo2);
					}
				}
			}
			if (si != null && gameObject != null)
			{
				this._combatData.GetOrAddPlayer(gameObject.Player.ID).AddShipData(si.DesignID, num2, num3, num, false);
				base.App.Game.MCCarryOverData.AddCarryOverInfo(this._systemId, si.FleetID, si.ID, Matrix.CreateWorld(position, forward, Vector3.UnitY));
			}
			CommonCombatState.Trace(string.Format("- TotalDamage:{0} -", num3));
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_PLANET(ScriptMessageReader mr)
		{
			List<int> AlliedPlayers = (
				from x in this._lastPendingCombat.CombatResults.PlayersInCombat
				where x == base.App.Game.LocalPlayer.ID || base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(base.App.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED
				select x).ToList<int>();
			List<int> list = (
				from x in this._lastPendingCombat.CombatResults.PlayersInCombat
				where !AlliedPlayers.Contains(x)
				select x).ToList<int>();
			int num = mr.ReadInteger();
			mr.ReadInteger();
			bool flag = mr.ReadBool();
			CommonCombatState.Trace("Getting InteropMessageID.IMID_SCRIPT_COMBAT_DATA_PLANET for planet object ID: " + num);
			int num2 = 0;
			OrbitalObjectInfo orbitalObjectInfo = null;
			PlanetInfo planetInfo = null;
			ColonyInfo colonyInfo = null;
			StarSystemInfo system = null;
			if (this._starSystemObjects.PlanetMap.Forward.ContainsKey(base.App.GetGameObject(num)))
			{
				num2 = this._starSystemObjects.PlanetMap.Forward[base.App.GetGameObject(num)];
				orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(num2);
				colonyInfo = base.App.GameDatabase.GetColonyInfoForPlanet(num2);
				planetInfo = base.App.GameDatabase.GetPlanetInfo(num2);
				system = ((planetInfo == null) ? null : base.App.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID));
			}
			float num3 = mr.ReadSingle();
			if (num3 != 0f && orbitalObjectInfo != null)
			{
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_PLANET_DAMAGE"), num3, orbitalObjectInfo.Name));
			}
			int num4 = mr.ReadInteger();
			for (int m = 0; m < num4; m++)
			{
				int playerGameObjectID = mr.ReadInteger();
				Player player = this._playersInCombat.FirstOrDefault((Player x) => x.ObjectID == playerGameObjectID);
				int num5 = mr.ReadInteger();
				for (int j = 0; j < num5; j++)
				{
					WeaponEnums.PlagueType plagueType = (WeaponEnums.PlagueType)mr.ReadInteger();
					double num6 = mr.ReadDouble();
					if (plagueType != WeaponEnums.PlagueType.ZUUL)
					{
						if (orbitalObjectInfo != null)
						{
							string item = string.Format(App.Localize("@UI_POST_COMBAT_STAT_PLAGUED"), orbitalObjectInfo.Name, App.Localize("@UI_PLAGUE_" + plagueType.ToString().ToUpper()));
							this._lastPendingCombat.CombatResults.AdditionalInfo.Add(item);
						}
						if (colonyInfo != null)
						{
							GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_PLAGUE_OUTBREAK, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, null);
							PlagueInfo pi = new PlagueInfo
							{
								PlagueType = plagueType,
								ColonyId = colonyInfo.ID,
								LaunchingPlayer = player.ID,
								InfectedPopulationCivilian = Math.Floor(num6 * 0.75),
								InfectedPopulationImperial = Math.Floor(num6 * 0.25),
								InfectionRate = base.App.AssetDatabase.GetPlagueInfectionRate(plagueType)
							};
							base.App.GameDatabase.InsertPlagueInfo(pi);
							base.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_PLAGUE_STARTED,
								EventMessage = TurnEventMessage.EM_PLAGUE_STARTED,
								PlagueType = plagueType,
								ColonyID = colonyInfo.ID,
								PlayerID = colonyInfo.PlayerID,
								TurnNumber = base.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
					}
				}
			}
			double num7 = Math.Floor(mr.ReadDouble());
			double num8 = Math.Floor(mr.ReadDouble());
			double num9 = 0.0;
			int num10 = mr.ReadInteger();
			List<PopulationData> list2 = new List<PopulationData>();
			if (num10 > 0)
			{
				for (int k = 0; k < num10; k++)
				{
					string civilianFactionType = mr.ReadString();
					double num11 = Math.Floor(mr.ReadDouble());
					double num12 = Math.Floor(mr.ReadDouble());
					num9 += num12;
					if (num12 != 0.0 && orbitalObjectInfo != null)
					{
						this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_CIVILIANS_DEAD"), civilianFactionType, num12, orbitalObjectInfo.Name));
						if (colonyInfo != null)
						{
							ColonyFactionInfo cfi = colonyInfo.Factions.First((ColonyFactionInfo x) => this.App.GameDatabase.GetFactionName(x.FactionID) == civilianFactionType);
							cfi.CivilianPop = num11;
							if (cfi.CivilianPop <= 0.0)
							{
								base.App.GameDatabase.RemoveCivilianPopulation(colonyInfo.OrbitalObjectID, cfi.FactionID);
								List<ColonyFactionInfo> list3 = colonyInfo.Factions.ToList<ColonyFactionInfo>();
								list3.RemoveAll((ColonyFactionInfo x) => x.FactionID == cfi.FactionID);
								colonyInfo.Factions = list3.ToArray();
							}
							else
							{
								base.App.GameDatabase.UpdateCivilianPopulation(cfi);
							}
						}
					}
					else
					{
						if (num11 <= 0.0 && colonyInfo != null)
						{
							ColonyFactionInfo cfi = colonyInfo.Factions.FirstOrDefault((ColonyFactionInfo x) => this.App.GameDatabase.GetFactionName(x.FactionID) == civilianFactionType);
							if (cfi != null)
							{
								base.App.GameDatabase.RemoveCivilianPopulation(colonyInfo.OrbitalObjectID, cfi.FactionID);
								List<ColonyFactionInfo> list4 = colonyInfo.Factions.ToList<ColonyFactionInfo>();
								list4.RemoveAll((ColonyFactionInfo x) => x.FactionID == cfi.FactionID);
								colonyInfo.Factions = list4.ToArray();
							}
						}
					}
					PopulationData item2;
					item2.faction = civilianFactionType;
					item2.damage = num12;
					list2.Add(item2);
				}
			}
			int num13 = (int)Math.Floor(num9 / 200000000.0);
			for (int l = 0; l < num13; l++)
			{
				if (colonyInfo != null)
				{
					GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_200MILLION_CIV_DEATHS, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, null);
				}
			}
			if (num8 != 0.0 && orbitalObjectInfo != null)
			{
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_IMPERIALS_DEAD"), num8, orbitalObjectInfo.Name));
			}
			float num14 = mr.ReadSingle();
			float num15 = mr.ReadSingle();
			if (num15 != 0f && orbitalObjectInfo != null)
			{
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_INFRASTRUCTURE_DESTROYED"), num15 * 100f, orbitalObjectInfo.Name));
			}
			float num16 = mr.ReadSingle();
			float num17 = mr.ReadSingle();
			if (num17 != 0f && orbitalObjectInfo != null)
			{
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_SUITABILITY_CHANGE"), num17, orbitalObjectInfo.Name));
			}
			if (planetInfo != null && colonyInfo != null)
			{
				colonyInfo.ModifyEconomyRating(base.App.GameDatabase, ColonyInfo.EconomicChangeReason.CombatInfrastructureLoss2Points, (int)Math.Floor((double)(num15 / 0.02f)));
				base.App.GameDatabase.UpdateColony(colonyInfo);
				planetInfo.Infrastructure = num14;
				planetInfo.Suitability = num16;
				planetInfo.Biosphere = (int)Math.Max((float)planetInfo.Biosphere - Math.Abs(num17) * 10f, 0f);
				base.App.GameDatabase.UpdatePlanet(planetInfo);
				List<FleetInfo> source = base.App.GameDatabase.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				if (source.Any((FleetInfo x) => x.PlayerID == base.App.Game.ScriptModules.MeteorShower.PlayerID))
				{
					NPCFactionCombatAI nPCFactionCombatAI = this.AI_Commanders.FirstOrDefault((CombatAI x) => x.m_Player.ID == base.App.Game.ScriptModules.MeteorShower.PlayerID) as NPCFactionCombatAI;
					if (nPCFactionCombatAI != null && nPCFactionCombatAI.PlanetsAttackedByNPC.Contains(num))
					{
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_ASTEROID_STORM,
							EventMessage = TurnEventMessage.EM_ASTEROID_STORM,
							PlayerID = colonyInfo.PlayerID,
							ColonyID = colonyInfo.ID,
							SystemID = system.ID,
							CivilianPop = (float)(num9 + num8),
							Infrastructure = num15 * 100f,
							TurnNumber = base.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
				else
				{
					if (source.Any((FleetInfo x) => x.PlayerID == base.App.Game.ScriptModules.Slaver.PlayerID))
					{
						NPCFactionCombatAI nPCFactionCombatAI2 = this.AI_Commanders.FirstOrDefault((CombatAI x) => x.m_Player.ID == base.App.Game.ScriptModules.Slaver.PlayerID) as NPCFactionCombatAI;
						if (nPCFactionCombatAI2 != null && nPCFactionCombatAI2.PlanetsAttackedByNPC.Contains(num))
						{
							base.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_SLAVER_ATTACK,
								EventMessage = TurnEventMessage.EM_SLAVER_ATTACK,
								PlayerID = colonyInfo.PlayerID,
								ColonyID = colonyInfo.ID,
								OrbitalID = colonyInfo.OrbitalObjectID,
								SystemID = system.ID,
								CivilianPop = (float)num9,
								ImperialPop = (float)num8,
								TurnNumber = base.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
					}
				}
			}
			if (colonyInfo != null && system != null && orbitalObjectInfo != null)
			{
				if (num7 <= 0.0)
				{
					List<PlayerInfo> standardPlayers = base.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
					bool flag2 = colonyInfo.IsIndependentColony(base.App);
					if (flag2)
					{
						List<FleetInfo> list5 = base.App.GameDatabase.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
						foreach (FleetInfo current in list5)
						{
							MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(current.ID);
							if (missionByFleetID != null && missionByFleetID.Type == MissionType.INVASION && missionByFleetID.TargetOrbitalObjectID == orbitalObjectInfo.ID)
							{
								base.App.GameDatabase.InsertGovernmentAction(current.PlayerID, App.Localize("@GA_INDEPENDANTCONQUERED"), "IndependantConquered", 0, 0);
								foreach (PlayerInfo current2 in standardPlayers)
								{
									if (base.App.GameDatabase.GetDiplomacyInfo(current.PlayerID, current2.ID).isEncountered)
									{
										base.App.GameDatabase.ApplyDiplomacyReaction(current.PlayerID, current2.ID, StratModifiers.DiplomacyReactionInvadeIndependentWorld, 1);
									}
								}
							}
						}
					}
					base.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_COLONY_DESTROYED,
						EventMessage = TurnEventMessage.EM_COLONY_DESTROYED,
						PlayerID = colonyInfo.PlayerID,
						ColonyID = colonyInfo.ID,
						SystemID = system.ID,
						TurnNumber = base.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					StationTypeFlags stationTypeFlags = StationTypeFlags.CIVILIAN | StationTypeFlags.DIPLOMATIC | StationTypeFlags.DEFENCE;
					List<StationInfo> list6 = base.App.GameDatabase.GetStationForSystemAndPlayer(this._systemId, colonyInfo.PlayerID).ToList<StationInfo>();
					foreach (StationInfo current3 in list6)
					{
						if (current3.OrbitalObjectInfo.ParentID == orbitalObjectInfo.ID && (1 << (int)current3.DesignInfo.StationType & (int)stationTypeFlags) != 0)
						{
							base.App.GameDatabase.DestroyStation(base.App.Game, current3.ID, 0);
						}
					}
					base.App.GameDatabase.RemoveColonyOnPlanet(num2);
					bool flag3 = base.App.GameDatabase.GetColonyInfosForSystem(system.ID).Count<ColonyInfo>() == 0;
					this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_COLONY_DESTROYED"), orbitalObjectInfo.Name));
					if (list.Any((int x) => standardPlayers.Any((PlayerInfo y) => y.ID == x)))
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_WORLD_ENEMY, colonyInfo.PlayerID, null, system.ProvinceID, null);
					}
					foreach (int i in list)
					{
						base.App.GameDatabase.ApplyDiplomacyReaction(i, colonyInfo.PlayerID, StratModifiers.DiplomacyReactionKillColony, 1);
						int factionId = base.App.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID);
						List<PlayerInfo> list7 = (
							from x in standardPlayers
							where x.FactionID == factionId && x.ID != i
							select x).ToList<PlayerInfo>();
						foreach (PlayerInfo current4 in list7)
						{
							if (base.App.GameDatabase.GetDiplomacyInfo(i, current4.ID).isEncountered)
							{
								base.App.GameDatabase.ApplyDiplomacyReaction(i, current4.ID, StratModifiers.DiplomacyReactionKillRaceWorld, 1);
							}
						}
					}
					if (colonyInfo.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld)
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_GEM, colonyInfo.PlayerID, null, system.ProvinceID, null);
					}
					else
					{
						if (colonyInfo.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld)
						{
							GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_FORGE, colonyInfo.PlayerID, null, system.ProvinceID, null);
						}
					}
					if (!flag3 || flag2)
					{
						goto IL_125C;
					}
					foreach (int current5 in list)
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_SYSTEM_CONQUERED, current5, null, null, null);
					}
					if (system.ProvinceID.HasValue && system.ID == base.App.GameDatabase.GetProvinceCapitalSystemID(system.ProvinceID.Value))
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_PROVINCE_CAPITAL, colonyInfo.PlayerID, null, null, null);
					}
					List<StarSystemInfo> source2 = base.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
					if (system.ProvinceID.HasValue)
					{
						if (!source2.Any((StarSystemInfo x) => x.ProvinceID == system.ProvinceID))
						{
							GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_PROVINCE, colonyInfo.PlayerID, null, null, null);
							foreach (int current6 in list)
							{
								GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_PROVINCE_CAPTURED, current6, null, null, null);
							}
						}
					}
					if (base.App.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID).Homeworld == colonyInfo.OrbitalObjectID)
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_EMPIRE_CAPITAL, colonyInfo.PlayerID, null, null, null);
					}
					if (base.App.GameDatabase.GetPlayerColoniesByPlayerId(colonyInfo.PlayerID).Count<ColonyInfo>() != 0)
					{
						goto IL_125C;
					}
					using (List<int>.Enumerator enumerator4 = list.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							int current7 = enumerator4.Current;
							GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_EMPIRE_DESTROYED, current7, null, null, null);
						}
						goto IL_125C;
					}
				}
				colonyInfo.DamagedLastTurn = (colonyInfo.ImperialPop != num7);
				colonyInfo.ImperialPop = num7;
				base.App.GameDatabase.UpdateColony(colonyInfo);
			}
			IL_125C:
			if (colonyInfo != null && planetInfo != null)
			{
				this._combatData.GetOrAddPlayer(colonyInfo.PlayerID).AddPlanetData(orbitalObjectInfo.ID, num17, num15, num8, list2);
			}
			if (flag)
			{
				base.App.GameDatabase.DestroyOrbitalObject(base.App.Game, num2);
				this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_ORBITAL_DESTROYED"), orbitalObjectInfo.Name));
			}
			CommonCombatState.Trace(string.Format("Planet Data - Imperial Pop: {0} Diff: {1} \n Infrastructure: {2} diff:{3} \n Suitability:{4} diff:{5} \n total Damage: {6}", new object[]
			{
				num7,
				num8,
				num14,
				num15,
				num16,
				num17,
				num3
			}));
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_STAR(ScriptMessageReader mr)
		{
			mr.ReadInteger();
			mr.ReadBool();
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DESTROYED_SHIPS(ScriptMessageReader mr)
		{
			int num = mr.ReadInteger();
			int num2 = num;
			List<int> list = new List<int>();
			for (int i = 0; i < num; i++)
			{
				int num3 = mr.ReadInteger();
				int num4 = mr.ReadInteger();
				string text = mr.ReadString();
				float num5 = mr.ReadSingle();
				float num6 = mr.ReadSingle();
				int kills = mr.ReadInteger();
				int num7 = mr.ReadInteger();
				DestroyedShip item = new DestroyedShip
				{
					DatabaseId = num3,
					Name = text,
					DamageReceived = num5,
					DamageApplied = num6
				};
				this._lastPendingCombat.CombatResults.DestroyedShips.Add(item);
				CommonCombatState.Trace(string.Format("- Ship Destroyed Message Id: {0} name: {1} damageRecieved: {2}, damageApplied: {3} -", new object[]
				{
					num3,
					text,
					num5,
					num6
				}));
				List<int> AlliedPlayers = (
					from x in this._lastPendingCombat.CombatResults.PlayersInCombat
					where x == base.App.Game.LocalPlayer.ID || base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(base.App.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED
					select x).ToList<int>();
				List<int> list2 = (
					from x in this._lastPendingCombat.CombatResults.PlayersInCombat
					where !AlliedPlayers.Contains(x)
					select x).ToList<int>();
				DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(num4);
				if (designInfo != null)
				{
					if (designInfo.Class == ShipClass.Station)
					{
						bool flag = false;
						List<StationInfo> list3 = base.App.GameDatabase.GetStationForSystemAndPlayer(this._systemId, designInfo.PlayerID).ToList<StationInfo>();
						foreach (StationInfo current in list3)
						{
							if (current.DesignInfo.ID == num4)
							{
								this.ApplyRewardsForShipDeath(base.App.GameDatabase.GetShipInfo(current.ShipID, false), designInfo.PlayerID, num7, null);
								if (base.App.Game.ScriptModules.Pirates == null || base.App.Game.ScriptModules.Pirates.PlayerID != current.PlayerID)
								{
									SuulkaInfo suulkaByStationID = base.App.GameDatabase.GetSuulkaByStationID(current.ID);
									if (suulkaByStationID != null)
									{
										List<int> list4 = (
											from x in this._playersInCombat
											where x.IsStandardPlayer
											select x.ID).ToList<int>();
										foreach (int current2 in list4)
										{
											base.App.GameDatabase.InsertTurnEvent(new TurnEvent
											{
												EventType = TurnEventType.EV_SUULKA_LEAVES,
												EventMessage = TurnEventMessage.EM_SUULKA_LEAVES,
												PlayerID = current2,
												ShipID = suulkaByStationID.ShipID,
												SystemID = this._systemId,
												Param1 = suulkaByStationID.ID.ToString(),
												TurnNumber = base.App.GameDatabase.GetTurnCount(),
												ShowsDialog = false
											});
										}
									}
									base.App.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_STATION_DESTROYED,
										EventMessage = TurnEventMessage.EM_STATION_DESTROYED,
										PlayerID = designInfo.PlayerID,
										OrbitalID = current.OrbitalObjectID,
										SystemID = base.App.GameDatabase.GetOrbitalObjectInfo(current.OrbitalObjectID).StarSystemID,
										TurnNumber = base.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
									GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_STATION, designInfo.PlayerID, null, null, new int?(this._lastPendingCombat.SystemID));
									if (designInfo.StationLevel == 5)
									{
										GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_LVL5_STATION, designInfo.PlayerID, null, null, new int?(this._lastPendingCombat.SystemID));
									}
								}
								flag = true;
								base.App.GameDatabase.DestroyStation(base.App.Game, current.OrbitalObjectID, 0);
								break;
							}
						}
						if (flag)
						{
							goto IL_15DB;
						}
					}
					DesignSectionInfo designSectionInfo = designInfo.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission);
					if (designSectionInfo == null || ShipSectionAsset.IsWeaponBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass))
					{
						num2--;
						this._lastPendingCombat.CombatResults.DestroyedShips.Remove(item);
					}
					ShipInfo si = base.App.GameDatabase.GetShipInfo(num3, true);
					if (si != null)
					{
						if (si.DesignInfo.IsSuperTransport())
						{
							List<ShipInfo> list5 = base.App.GameDatabase.GetShipInfoByFleetID(si.FleetID, true).ToList<ShipInfo>();
							foreach (ShipInfo current3 in list5)
							{
								if (current3.DesignInfo.IsSDB() || current3.DesignInfo.IsPlatform())
								{
									base.App.GameDatabase.RemoveShip(current3.ID);
								}
							}
						}
						if (base.App.AssetDatabase.GetFaction(base.App.GameDatabase.GetPlayerInfo(designInfo.PlayerID).FactionID).Name == "loa" && !list.Contains(si.FleetID))
						{
							list.Add(si.FleetID);
						}
						if (this._pirateEncounterData.IsPirateEncounter && designInfo.Role == ShipRole.FREIGHTER && this._pirateEncounterData.PlayerFreightersInSystem.ContainsKey(designInfo.PlayerID))
						{
							CommonCombatState.PiracyFreighterInfo piracyFreighterInfo = this._pirateEncounterData.PlayerFreightersInSystem[designInfo.PlayerID].First((CommonCombatState.PiracyFreighterInfo x) => x.ShipID == si.ID);
							base.App.GameDatabase.RemoveFreighterInfo(piracyFreighterInfo.FreighterID);
							if (this._pirateEncounterData.DestroyedFreighters.ContainsKey(designInfo.PlayerID))
							{
								Dictionary<int, int> destroyedFreighters;
								int playerID;
								(destroyedFreighters = this._pirateEncounterData.DestroyedFreighters)[playerID = designInfo.PlayerID] = destroyedFreighters[playerID] + 1;
							}
							else
							{
								this._pirateEncounterData.DestroyedFreighters.Add(designInfo.PlayerID, 1);
							}
							List<ColonyInfo> list6 = base.App.GameDatabase.GetColonyInfosForSystem(this._systemId).ToList<ColonyInfo>();
							foreach (ColonyInfo current4 in list6)
							{
								current4.ModifyEconomyRating(base.App.GameDatabase, ColonyInfo.EconomicChangeReason.FreighterKilled, 1);
								base.App.GameDatabase.UpdateColony(current4);
							}
						}
						si.DesignInfo = designInfo;
						FleetInfo fi = base.App.GameDatabase.GetFleetInfo(si.FleetID);
						if (fi == null)
						{
							CommonCombatState.Warn("Attemping to access a fleet, for a destroyed ship, which has already been removed.");
						}
						else
						{
							this.ApplyRewardsForShipDeath(si, fi.PlayerID, num7, fi);
							Player playerByObjectID = base.App.GetPlayerByObjectID(num7);
							int playerID2 = (playerByObjectID != null) ? playerByObjectID.ID : 0;
							this._combatData.GetOrAddPlayer(fi.PlayerID).AddShipData(si.DesignID, num6, num5, kills, true);
							if (fi.PlayerID == base.App.Game.LocalPlayer.ID && si.DesignInfo.Class == ShipClass.Leviathan)
							{
								base.App.SteamHelper.DoAchievement(AchievementType.SOTS2_INCONVIENIENCE);
							}
							if (base.App.Game.ScriptModules.AsteroidMonitor != null && si.DesignID == base.App.Game.ScriptModules.AsteroidMonitor.MonitorCommandDesignId)
							{
								foreach (int current5 in base.App.GameDatabase.GetPlayerIDs())
								{
									if (current5 != fi.ID)
									{
										base.App.GameDatabase.ChangeDiplomacyState(fi.PlayerID, current5, DiplomacyState.NEUTRAL);
									}
								}
							}
							if (si.DesignInfo.IsSuulka())
							{
								TurnEvent turnEvent = base.App.GameDatabase.GetTurnEventsByTurnNumber(base.App.GameDatabase.GetTurnCount(), fi.PlayerID).FirstOrDefault((TurnEvent x) => x.ShipID == si.ID);
								if (turnEvent != null)
								{
									base.App.GameDatabase.RemoveTurnEvent(turnEvent.ID);
								}
								base.App.GameDatabase.InsertTurnEvent(new TurnEvent
								{
									EventType = TurnEventType.EV_SUULKA_DIES,
									EventMessage = TurnEventMessage.EM_SUULKA_DIES,
									PlayerID = playerID2,
									SystemID = fi.SystemID,
									ShipID = si.ID,
									DesignID = si.DesignID,
									TurnNumber = base.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
								SuulkaInfo suulkaByShipID = base.App.GameDatabase.GetSuulkaByShipID(si.ID);
								if (suulkaByShipID != null)
								{
									base.App.GameDatabase.RemoveSuulka(suulkaByShipID.ID);
								}
							}
							GameTrigger.PushEvent(EventType.EVNT_SHIPDIED, si.DesignInfo.Class, base.App.Game);
							base.App.GameDatabase.RemoveShip(num3);
							bool flag2 = base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(si.DesignInfo.PlayerID)) == "loa";
							bool flag3 = false;
							if (si.DesignInfo.Class == ShipClass.Leviathan)
							{
								GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_FLAGSHIP, si.DesignInfo.PlayerID, null, null, new int?(this._lastPendingCombat.SystemID));
								flag3 = !flag2;
								if (AlliedPlayers.Contains(si.DesignInfo.PlayerID))
								{
									foreach (int current6 in list2)
									{
										GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_LEVIATHAN_DESTROYED, current6, null, null, null);
									}
									GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_LEVIATHAN, si.DesignInfo.PlayerID, null, null, new int?(this._lastPendingCombat.SystemID));
								}
								else
								{
									foreach (int current7 in AlliedPlayers)
									{
										GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_LEVIATHAN_DESTROYED, current7, null, null, null);
									}
									GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_LEVIATHAN, si.DesignInfo.PlayerID, null, null, new int?(this._lastPendingCombat.SystemID));
								}
							}
							else
							{
								if (si.DesignInfo.DesignSections.Any((DesignSectionInfo x) => x.FilePath.Contains("cnc")) && !flag2)
								{
									if (si.DesignInfo.Class == ShipClass.Dreadnought)
									{
										GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_LOSE_FLAGSHIP, si.DesignInfo.PlayerID, null, null, new int?(this._lastPendingCombat.SystemID));
									}
									flag3 = true;
								}
							}
							List<int> list7 = base.App.GameDatabase.GetShipsByFleetID(si.FleetID).ToList<int>();
							if (list7.Count == 0)
							{
								if (fi.AdmiralID != 0 && playerByObjectID != null && playerByObjectID.IsStandardPlayer)
								{
									this.CheckAdmiralCaptured(fi, playerByObjectID.ID);
								}
								if (fi.PlayerID == base.App.Game.ScriptModules.Gardeners.PlayerID)
								{
									base.App.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_PROTEANS_REMOVED,
										EventMessage = TurnEventMessage.EM_PROTEANS_REMOVED,
										PlayerID = playerID2,
										SystemID = fi.SystemID,
										TurnNumber = base.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								}
								else
								{
									if (fi.PlayerID == base.App.Game.ScriptModules.Swarmers.PlayerID)
									{
										if (si.DesignID == base.App.Game.ScriptModules.Swarmers.SwarmQueenDesignID)
										{
											base.App.GameDatabase.InsertTurnEvent(new TurnEvent
											{
												EventType = TurnEventType.EV_SWARM_QUEEN_DESTROYED,
												EventMessage = TurnEventMessage.EM_SWARM_QUEEN_DESTROYED,
												PlayerID = playerID2,
												SystemID = fi.SystemID,
												TurnNumber = base.App.GameDatabase.GetTurnCount(),
												ShowsDialog = false
											});
										}
										else
										{
											base.App.GameDatabase.InsertTurnEvent(new TurnEvent
											{
												EventType = TurnEventType.EV_SWARM_DESTROYED,
												EventMessage = TurnEventMessage.EM_SWARM_DESTROYED,
												PlayerID = playerID2,
												SystemID = fi.SystemID,
												TurnNumber = base.App.GameDatabase.GetTurnCount(),
												ShowsDialog = false
											});
										}
									}
									else
									{
										if (fi.PlayerID == base.App.Game.ScriptModules.SystemKiller.PlayerID)
										{
											FleetLocation fleetLocation = base.App.GameDatabase.GetFleetLocation(fi.ID, false);
											if (fleetLocation == null)
											{
												goto IL_12A0;
											}
											using (IEnumerator<int> enumerator5 = base.App.GameDatabase.GetStandardPlayerIDs().GetEnumerator())
											{
												while (enumerator5.MoveNext())
												{
													int current8 = enumerator5.Current;
													if (StarMap.IsInRange(base.App.GameDatabase, current8, fleetLocation.Coords, 1f, null))
													{
														base.App.GameDatabase.InsertTurnEvent(new TurnEvent
														{
															EventType = TurnEventType.EV_SYS_KILLER_DESTROYED,
															EventMessage = TurnEventMessage.EM_SYS_KILLER_DESTROYED,
															PlayerID = current8,
															TurnNumber = base.App.GameDatabase.GetTurnCount()
														});
													}
												}
												goto IL_12A0;
											}
										}
										if (fi.PlayerID == base.App.Game.ScriptModules.MorrigiRelic.PlayerID)
										{
											base.App.GameDatabase.InsertTurnEvent(new TurnEvent
											{
												EventType = TurnEventType.EV_TOMB_DESTROYED,
												EventMessage = TurnEventMessage.EM_TOMB_DESTROYED,
												PlayerID = playerID2,
												SystemID = fi.SystemID,
												TurnNumber = base.App.GameDatabase.GetTurnCount(),
												ShowsDialog = false
											});
										}
										else
										{
											if (fi.PlayerID == base.App.Game.ScriptModules.VonNeumann.PlayerID && base.App.Game.ScriptModules.VonNeumann.IsHomeWorldFleet(fi))
											{
												base.App.Game.ScriptModules.VonNeumann.HandleHomeSystemDefeated(base.App, fi, (
													from x in this._playersInCombat
													where x.IsStandardPlayer && x.ID != fi.PlayerID
													select x.ID).ToList<int>());
												base.App.GameDatabase.InsertTurnEvent(new TurnEvent
												{
													EventType = TurnEventType.EV_FLEET_DESTROYED,
													EventMessage = TurnEventMessage.EM_FLEET_DESTROYED,
													PlayerID = playerID2,
													SystemID = fi.SystemID,
													FleetID = fi.ID,
													TurnNumber = base.App.GameDatabase.GetTurnCount(),
													ShowsDialog = false
												});
											}
											else
											{
												base.App.GameDatabase.InsertTurnEvent(new TurnEvent
												{
													EventType = TurnEventType.EV_FLEET_DESTROYED,
													EventMessage = TurnEventMessage.EM_FLEET_DESTROYED,
													PlayerID = playerID2,
													SystemID = fi.SystemID,
													FleetID = fi.ID,
													TurnNumber = base.App.GameDatabase.GetTurnCount(),
													ShowsDialog = false
												});
											}
										}
									}
								}
								IL_12A0:
								GameTrigger.PushEvent(EventType.EVNT_FLEETDIED, fi.Name, base.App.Game);
								base.App.GameDatabase.RemoveFleet(si.FleetID);
								this.CheckToReturnControlZonesToOwner(si.DesignInfo.PlayerID);
								if (AlliedPlayers.Contains(si.DesignInfo.PlayerID))
								{
									using (List<int>.Enumerator enumerator2 = list2.GetEnumerator())
									{
										while (enumerator2.MoveNext())
										{
											int current9 = enumerator2.Current;
											GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_FLEET_DESTROYED, current9, null, null, null);
										}
										goto IL_15DB;
									}
								}
								using (List<int>.Enumerator enumerator2 = AlliedPlayers.GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										int current10 = enumerator2.Current;
										GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ENEMY_FLEET_DESTROYED, current10, null, null, null);
									}
									goto IL_15DB;
								}
							}
							if (flag3)
							{
								PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(fi.PlayerID);
								if (playerInfo != null && playerInfo.isStandardPlayer)
								{
									if (list7.Max((int x) => base.App.GameDatabase.GetShipCommandPointQuota(x)) == 0)
									{
										if (fi.AdmiralID != 0)
										{
											this.CheckAdmiralSurvival(fi, si);
										}
										int num8 = base.App.Game.MCCarryOverData.GetRetreatFleetID(fi.SystemID, fi.ID);
										if (num8 != fi.ID)
										{
											if (num8 == 0)
											{
												num8 = base.App.GameDatabase.InsertFleet(fi.PlayerID, 0, fi.SystemID, fi.SupportingSystemID, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL);
												int missionID = base.App.GameDatabase.InsertMission(num8, MissionType.RETREAT, 0, 0, 0, 0, false, null);
												base.App.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, null);
												base.App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, null);
												base.App.Game.MCCarryOverData.SetRetreatFleetID(fi.SystemID, fi.ID, num8);
											}
											foreach (int current11 in list7)
											{
												base.App.GameDatabase.TransferShip(current11, num8);
											}
											base.App.GameDatabase.RemoveFleet(fi.ID);
										}
										this.CheckToReturnControlZonesToOwner(si.DesignInfo.PlayerID);
									}
								}
							}
						}
					}
				}
				IL_15DB:;
			}
			this._lastPendingCombat.CombatResults.AdditionalInfo.Add(string.Format(App.Localize("@UI_POST_COMBAT_STAT_DESTROYED_SHIPS"), num2));
			foreach (int current12 in list)
			{
				FleetInfo fi = base.App.GameDatabase.GetFleetInfo(current12);
				if (fi != null)
				{
					MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(fi.ID);
                    int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(base.App.Game, current12);
					if (fi.FleetConfigID.HasValue)
					{
						LoaFleetComposition loaFleetComposition = base.App.GameDatabase.GetLoaFleetCompositions().FirstOrDefault((LoaFleetComposition x) => x.ID == fi.FleetConfigID.Value);
						if (loaFleetComposition != null)
						{
                            List<DesignInfo> source = Kerberos.Sots.StarFleet.StarFleet.GetDesignBuildOrderForComposition(base.App.Game, fi.ID, loaFleetComposition, (missionByFleetID != null) ? missionByFleetID.Type : MissionType.NO_MISSION).ToList<DesignInfo>();
							if (!source.Any<DesignInfo>())
							{
								base.App.GameDatabase.RemoveFleet(fi.ID);
							}
							else
							{
								DesignInfo designInfo2 = source.First<DesignInfo>();
								if (designInfo2.GetCommandPoints() == 0 || designInfo2.ProductionCost > fleetLoaCubeValue)
								{
									base.App.GameDatabase.RemoveFleet(fi.ID);
								}
							}
						}
					}
				}
			}
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_CAPTURED_SHIPS(ScriptMessageReader mr)
		{
			int num = mr.ReadInteger();
			for (int i = 0; i < num; i++)
			{
				int id = mr.ReadInteger();
				mr.ReadInteger();
				int objectId = mr.ReadInteger();
				int objectId2 = mr.ReadInteger();
				bool flag = mr.ReadBool();
				Ship gameObject = base.App.GetGameObject<Ship>(id);
				base.App.GetPlayerByObjectID(objectId);
				Player playerByObjectID = base.App.GetPlayerByObjectID(objectId2);
				if (this._pirateEncounterData.IsPirateEncounter && gameObject != null && playerByObjectID != null && this._pirateEncounterData.PiratePlayerIDs.Contains(playerByObjectID.ID))
				{
					int num2 = 0;
					if (gameObject.ShipRole == ShipRole.FREIGHTER)
					{
						num2 = base.App.AssetDatabase.GlobalPiracyData.Bounties[3];
					}
					if (this._pirateEncounterData.PlayerBounties.ContainsKey(playerByObjectID.ID))
					{
						Dictionary<int, int> playerBounties;
						int iD;
						(playerBounties = this._pirateEncounterData.PlayerBounties)[iD = playerByObjectID.ID] = playerBounties[iD] + num2;
					}
					else
					{
						this._pirateEncounterData.PlayerBounties.Add(playerByObjectID.ID, num2);
					}
				}
			}
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS(ScriptMessageReader mr)
		{
			int num = mr.ReadInteger();
			for (int i = 0; i < num; i++)
			{
				int id = mr.ReadInteger();
				int num2 = mr.ReadInteger();
				Player gameObject = base.App.GetGameObject<Player>(id);
				CommonCombatState.Trace(string.Format("- Weapon Damage Stats for Player {0} -", gameObject.ID));
				this._lastPendingCombat.CombatResults.WeaponDamageTable.Add(gameObject.ID, new Dictionary<int, float>());
				for (int j = 0; j < num2; j++)
				{
					int weaponID = mr.ReadInteger();
					float num3 = mr.ReadSingle();
					LogicalWeapon logicalWeapon = base.App.AssetDatabase.Weapons.First((LogicalWeapon x) => x.GameObject != null && x.GameObject.ObjectID == weaponID);
					this._combatData.GetOrAddPlayer(gameObject.ID).AddWeaponData(logicalWeapon.UniqueWeaponID, num3);
					this._lastPendingCombat.CombatResults.WeaponDamageTable[gameObject.ID].Add(logicalWeapon.UniqueWeaponID, num3);
					CommonCombatState.Trace(string.Format("   Weapon: {0} Damage: {1}", logicalWeapon.UniqueWeaponID, num3));
				}
			}
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_ZONE_STATES(ScriptMessageReader mr)
		{
			int num = mr.ReadInteger();
			List<int> list = new List<int>();
			for (int i = 0; i < num; i++)
			{
				int objectId = mr.ReadInteger();
				Player playerByObjectID = base.App.GetPlayerByObjectID(objectId);
				if (playerByObjectID != null)
				{
					list.Add(playerByObjectID.ID);
				}
				else
				{
					list.Add(0);
				}
			}
			base.App.GameDatabase.UpdateSystemCombatZones(this._systemId, list);
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_ZONE_OWNER_CHANGED(ScriptMessageReader mr)
		{
			int ring = mr.ReadInteger();
			int zone = mr.ReadInteger();
			Player gameObject = base.App.GetGameObject<Player>(mr.ReadInteger());
			if (this._starSystemObjects != null)
			{
				CombatZonePositionInfo zone2 = this._starSystemObjects.ChangeCombatZoneOwner(ring, zone, gameObject);
				foreach (CombatAI current in this.AI_Commanders)
				{
					current.NotifyCombatZoneChanged(zone2);
				}
			}
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_PLAYER_DIPLO_CHANGED(ScriptMessageReader mr)
		{
			int objectId = mr.ReadInteger();
			int objectId2 = mr.ReadInteger();
			int num = mr.ReadInteger();
			if (this._initialDiploStates != null)
			{
				PlayerCombatDiplomacy playerCombatDiplomacy = (PlayerCombatDiplomacy)num;
				Player playerByObjectID = base.App.GetPlayerByObjectID(objectId);
				Player playerByObjectID2 = base.App.GetPlayerByObjectID(objectId2);
				if (playerByObjectID != null && playerByObjectID2 != null && playerCombatDiplomacy == PlayerCombatDiplomacy.War)
				{
					this.SetDiplomacyState(playerByObjectID.ID, playerByObjectID2.ID, DiplomacyState.WAR);
					this.SyncPlayerList();
				}
			}
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_ENDED(ScriptMessageReader mr)
		{
			mr.ReadInteger();
			this.EndCombat();
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_SET_PAUSE_STATE(ScriptMessageReader mr)
		{
			this._isPaused = (mr.ReadInteger() == 1);
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_SET_COMBAT_ACTIVE(ScriptMessageReader mr)
		{
			this._engCombatActive = mr.ReadBool();
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_END_SENDINGDATA(ScriptMessageReader mr)
		{
			if (this._authority)
			{
				if (this._lastPendingCombat.CombatResults == null)
				{
					this._lastPendingCombat.CombatResults = new PostCombatData();
				}
				if ((base.App.Network.IsHosting || !base.App.GameSetup.IsMultiplayer) && this._combatData.SystemID != 0)
				{
					base.App.GameDatabase.InsertCombatData(this._combatData.SystemID, this._combatData.CombatID, this._combatData.Turn, this._combatData.ToByteArray());
				}
				this.ProcessShipsHitByNodeCannon();
				foreach (Player player in this._playersInCombat)
				{
					PlayerCombatData pcd = this._combatData.GetPlayer(player.ID);
					if (pcd != null)
					{
						pcd.VictoryStatus = base.App.Game.GetPlayerVictoryStatus(player.ID, this._systemId);
						pcd.FleetCount = (this._fleetsPerPlayer.ContainsKey(player.ID) ? this._fleetsPerPlayer[player.ID] : 0);
						if (pcd.VictoryStatus == GameSession.VictoryStatus.Win && pcd.FleetCount > 0)
						{
							if (this._mediaHeroFleets.Any((FleetInfo x) => x.PlayerID == player.ID))
							{
								GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ADMIRAL_MEDIA_HERO_WIN, player.ID, null, null, null);
							}
						}
						if (pcd.PlayerID == base.App.Game.ScriptModules.Spectre.PlayerID)
						{
							foreach (Player current in this._playersInCombat)
							{
								Player player2 = base.App.GetPlayer(current.ID);
								if (player2.IsStandardPlayer && player2.ID != pcd.PlayerID)
								{
									base.App.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_SPECTRE_ATTACK,
										EventMessage = TurnEventMessage.EM_SPECTRE_ATTACK,
										PlayerID = player2.ID,
										SystemID = this._combatData.SystemID,
										TurnNumber = base.App.GameDatabase.GetTurnCount()
									});
								}
							}
						}
						if (pcd.PlayerID == base.App.Game.ScriptModules.MeteorShower.PlayerID)
						{
							CombatAI combatAI = this.AI_Commanders.FirstOrDefault((CombatAI x) => x.m_Player.ID == pcd.PlayerID);
							List<int> list = new List<int>();
							foreach (ColonyInfo current2 in base.App.GameDatabase.GetColonyInfosForSystem(this._systemId))
							{
								if (!list.Contains(current2.PlayerID))
								{
									list.Add(current2.PlayerID);
								}
							}
							foreach (int current3 in list)
							{
								GameSession.ApplyMoralEvent(base.App, ((NPCFactionCombatAI)combatAI).NumPlanetStruckAsteroids, MoralEvent.ME_ASTEROID_STRIKE, current3, null, null, new int?(this._systemId));
							}
						}
					}
				}
				foreach (NPCFactionCombatAI current4 in this.AI_Commanders.OfType<NPCFactionCombatAI>())
				{
					current4.HandlePostCombat(this._playersInCombat, (
						from x in this._spawnPositions
						select x._fleetID).ToList<int>(), this._systemId);
				}
				this.UpdateDiploStatesInScript();
				if (this._pirateEncounterData.IsPirateEncounter)
				{
					foreach (KeyValuePair<int, List<CommonCombatState.PiracyFreighterInfo>> current5 in this._pirateEncounterData.PlayerFreightersInSystem)
					{
						int numShips = 0;
						if (!this._pirateEncounterData.DestroyedFreighters.TryGetValue(current5.Key, out numShips))
						{
							numShips = 0;
						}
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_PIRATE_RAID,
							EventMessage = TurnEventMessage.EM_PIRATE_RAID,
							PlayerID = current5.Key,
							NumShips = numShips,
							TurnNumber = base.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					foreach (KeyValuePair<int, int> current6 in this._pirateEncounterData.PlayerBounties)
					{
						PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(current6.Key);
						if (playerInfo != null && (base.App.Game.ScriptModules.Pirates == null || base.App.Game.ScriptModules.Pirates.PlayerID != playerInfo.ID))
						{
							base.App.GameDatabase.UpdatePlayerSavings(current6.Key, playerInfo.Savings + (double)current6.Value);
						}
					}
				}
				foreach (Player current7 in this._playersInCombat)
				{
					if (this._ignoreCombatZonePlayers.Contains(current7))
					{
						StarSystem.RemoveSystemPlayerColor(base.App.GameDatabase, this._systemId, current7.ID);
					}
				}
				StarSystem.RestoreNeutralSystemColor(base.App, this._systemId, true);
				if (base.App.GameSetup.IsMultiplayer)
				{
					base.App.Network.SendCarryOverData(base.App.Game.MCCarryOverData.GetCarryOverDataList(this._systemId));
					base.App.Network.SendCombatData(this._combatData);
				}
			}
			this._combatData = null;
			this._combatEndingStatsGathered = true;
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_END_DELAYCOMPLETE(ScriptMessageReader mr)
		{
			this._combatEndDelayComplete = true;
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECT_ADD(ScriptMessageReader mr)
		{
			this.AddObject(mr);
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECTS_ADD(ScriptMessageReader mr)
		{
			this.AddObjects(mr);
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECT_RELEASE(ScriptMessageReader mr)
		{
			this.RemoveObject(mr);
		}
		private void OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECTS_RELEASE(ScriptMessageReader mr)
		{
			this.RemoveObjects(mr);
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
			case InteropMessageID.IMID_SCRIPT_SET_PAUSE_STATE:
				this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_SET_PAUSE_STATE(mr);
				return;
			case InteropMessageID.IMID_SCRIPT_SET_COMBAT_ACTIVE:
				this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_SET_COMBAT_ACTIVE(mr);
				return;
			case InteropMessageID.IMID_ENGINE_SET_AUTHORITIVE_STATE:
			case InteropMessageID.IMID_ENGINE_OBJECT_ADD:
			case InteropMessageID.IMID_ENGINE_OBJECT_ADDED:
			case InteropMessageID.IMID_ENGINE_OBJECTS_ADDED:
			case InteropMessageID.IMID_SCRIPT_OBJECT_STATUS:
				break;
			case InteropMessageID.IMID_SCRIPT_PLAYER_DIPLO_CHANGED:
				this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_PLAYER_DIPLO_CHANGED(mr);
				return;
			case InteropMessageID.IMID_SCRIPT_ZONE_OWNER_CHANGED:
				this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_ZONE_OWNER_CHANGED(mr);
				return;
			case InteropMessageID.IMID_SCRIPT_OBJECT_ADD:
				this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECT_ADD(mr);
				return;
			case InteropMessageID.IMID_SCRIPT_OBJECTS_ADD:
				this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECTS_ADD(mr);
				return;
			case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
				this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECT_RELEASE(mr);
				return;
			case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
				this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_OBJECTS_RELEASE(mr);
				return;
			default:
				switch (messageID)
				{
				case InteropMessageID.IMID_SCRIPT_COMBAT_ENDED:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_ENDED(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_START_SENDINGDATA:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_START_SENDINGDATA(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_SHIP:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_SHIP(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_PLANET:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_PLANET(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_STAR:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DATA_STAR(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_COMBAT_DESTROYED_SHIPS:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_DESTROYED_SHIPS(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_COMBAT_CAPTURED_SHIPS:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_CAPTURED_SHIPS(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_COMBAT_ZONE_STATES:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_COMBAT_ZONE_STATES(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_END_SENDINGDATA:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_END_SENDINGDATA(mr);
					return;
				case InteropMessageID.IMID_SCRIPT_END_DELAYCOMPLETE:
					this.OnEngineMessage_InteropMessageID_IMID_SCRIPT_END_DELAYCOMPLETE(mr);
					return;
				}
				break;
			}
			CommonCombatState.Warn("Unhandled message (id=" + messageID + ").");
		}
		protected void CheckAdmiralSurvival(FleetInfo fi, ShipInfo si)
		{
			float num = 0.35f;
			AdmiralInfo admiralInfo = base.App.GameDatabase.GetAdmiralInfo(fi.AdmiralID);
			if (admiralInfo != null)
			{
				num = (float)admiralInfo.EvasionBonus;
			}
			List<AdmiralInfo.TraitType> list = base.App.GameDatabase.GetAdmiralTraits(fi.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list.Contains(AdmiralInfo.TraitType.Slippery))
			{
				num += 0.2f;
			}
			if (admiralInfo.Engram)
			{
				num += 0.1f;
			}
			string factionName = base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(fi.PlayerID));
			List<LogicalModule> list2 = (
				from x in base.App.AssetDatabase.Modules
				where x.Faction == factionName && x.ModulePath.Contains("hannibal")
				select x).ToList<LogicalModule>();
			bool flag = false;
			int moduleId = 0;
			foreach (LogicalModule current in list2)
			{
				moduleId = base.App.GameDatabase.GetModuleID(current.ModulePath, fi.PlayerID);
				bool arg_132_0;
				if (flag)
				{
					arg_132_0 = si.DesignInfo.DesignSections.Any((DesignSectionInfo x) => x.Modules.Any((DesignModuleInfo y) => y.ModuleID == moduleId));
				}
				else
				{
					arg_132_0 = false;
				}
				flag = arg_132_0;
			}
			if (flag)
			{
				num += 0.25f;
			}
			if (!this._random.CoinToss((double)num))
			{
				if (admiralInfo.HomeworldID.HasValue)
				{
					ColonyInfo colonyInfo = base.App.GameDatabase.GetColonyInfo(admiralInfo.HomeworldID.Value);
					if (colonyInfo != null && colonyInfo.PlayerID == admiralInfo.PlayerID)
					{
						GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ADMIRAL_KILLED, fi.PlayerID, null, null, new int?(colonyInfo.CachedStarSystemID));
					}
				}
				if (list.Contains(AdmiralInfo.TraitType.MediaHero))
				{
					GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_ADMIRAL_MEDIA_HERO_KILLED, fi.PlayerID, null, null, null);
				}
				base.App.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_ADMIRAL_DEAD,
					EventMessage = TurnEventMessage.EM_ADMIRAL_DEAD,
					PlayerID = fi.PlayerID,
					AdmiralID = fi.AdmiralID,
					TurnNumber = base.App.GameDatabase.GetTurnCount()
				});
				base.App.GameDatabase.RemoveAdmiral(fi.AdmiralID);
			}
		}
		protected void CheckAdmiralCaptured(FleetInfo fi, int destroyingPlayer)
		{
			float num = 0.75f;
			AdmiralInfo admiralInfo = base.App.GameDatabase.GetAdmiralInfo(fi.AdmiralID);
			if (admiralInfo != null)
			{
				num -= (float)admiralInfo.EvasionBonus;
			}
			List<AdmiralInfo.TraitType> list = base.App.GameDatabase.GetAdmiralTraits(fi.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list.Contains(AdmiralInfo.TraitType.Slippery))
			{
				num -= 0.2f;
			}
			if (this._random.CoinToss((double)num))
			{
				List<FleetInfo> list2 = base.App.GameDatabase.GetFleetsByPlayerAndSystem(destroyingPlayer, fi.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				foreach (FleetInfo current in list2)
				{
					List<AdmiralInfo.TraitType> list3 = base.App.GameDatabase.GetAdmiralTraits(current.AdmiralID).ToList<AdmiralInfo.TraitType>();
					if (list3.Contains(AdmiralInfo.TraitType.Inquisitor) && this._random.CoinToss(0.25))
					{
						foreach (AdmiralInfo.TraitType tt in list)
						{
							if (!list3.Contains(tt))
							{
								if (!list3.Any((AdmiralInfo.TraitType x) => AdmiralInfo.AreTraitsMutuallyExclusive(x, tt)))
								{
									base.App.GameDatabase.AddAdmiralTrait(current.AdmiralID, tt, 1);
								}
							}
						}
					}
					if (list3.Contains(AdmiralInfo.TraitType.Evangelist) && this._random.CoinToss(0.25))
					{
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_ADMIRAL_INTEL_LEAK,
							EventMessage = TurnEventMessage.EM_ADMIRAL_INTEL_LEAK_TAKE,
							PlayerID = current.PlayerID,
							AdmiralID = current.AdmiralID,
							TargetPlayerID = destroyingPlayer,
							TurnNumber = base.App.GameDatabase.GetTurnCount()
						});
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_ADMIRAL_INTEL_LEAK,
							EventMessage = TurnEventMessage.EM_ADMIRAL_INTEL_LEAK_GIVE,
							PlayerID = fi.PlayerID,
							AdmiralID = fi.AdmiralID,
							TargetPlayerID = destroyingPlayer,
							TurnNumber = base.App.GameDatabase.GetTurnCount()
						});
						base.App.AssetDatabase.IntelMissions.Choose(this._random).OnCommit(base.App.Game, destroyingPlayer, fi.PlayerID, null);
					}
					if (list3.Contains(AdmiralInfo.TraitType.HeadHunter) && list.Contains(AdmiralInfo.TraitType.Conscript) && this._random.CoinToss(0.5))
					{
						AdmiralInfo admiralInfo2 = base.App.GameDatabase.GetAdmiralInfo(fi.AdmiralID);
						admiralInfo2.PlayerID = current.PlayerID;
						base.App.GameDatabase.UpdateAdmiralInfo(admiralInfo2);
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_ADMIRAL_DEFECTS,
							EventMessage = TurnEventMessage.EM_ADMIRAL_DEFECTS,
							PlayerID = fi.PlayerID,
							AdmiralID = fi.AdmiralID,
							TargetPlayerID = destroyingPlayer,
							TurnNumber = base.App.GameDatabase.GetTurnCount()
						});
						return;
					}
					base.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_ADMIRAL_CAPTURED,
						EventMessage = TurnEventMessage.EM_ADMIRAL_CAPTURED,
						PlayerID = fi.PlayerID,
						AdmiralID = fi.AdmiralID,
						TargetPlayerID = destroyingPlayer,
						TurnNumber = base.App.GameDatabase.GetTurnCount()
					});
				}
				base.App.GameDatabase.RemoveAdmiral(fi.AdmiralID);
				return;
			}
			base.App.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_ADMIRAL_ESCAPES,
				EventMessage = TurnEventMessage.EM_ADMIRAL_ESCAPES,
				PlayerID = fi.PlayerID,
				AdmiralID = fi.AdmiralID,
				TargetPlayerID = destroyingPlayer,
				TurnNumber = base.App.GameDatabase.GetTurnCount()
			});
		}
		protected override void OnUpdate()
		{
			CommonCombatState.CombatSubState subState = this._subState;
			if (subState == CommonCombatState.CombatSubState.Ending)
			{
				if (this.UpdateEnding())
				{
					this._subState = CommonCombatState.CombatSubState.Ended;
					return;
				}
			}
			else
			{
				this.UpdateRunning();
			}
		}
		public override bool IsReady()
		{
			bool result = true;
			if (this._crits != null && !this._crits.IsReady())
			{
				result = false;
			}
			return result;
		}
		public override void AddGameObject(IGameObject gameObject, bool autoSetActive = false)
		{
			if (gameObject == null)
			{
				return;
			}
			if (autoSetActive)
			{
				this._postLoadedObjects.Add(gameObject);
				return;
			}
			this._combat.PostObjectAddObjects(new IGameObject[]
			{
				gameObject
			});
			this._crits.Add(gameObject);
		}
		public override void RemoveGameObject(IGameObject gameObject)
		{
			if (gameObject == null)
			{
				return;
			}
			IGameObject gameObject2 = this._crits.Objects.FirstOrDefault((IGameObject x) => x.ObjectID == gameObject.ObjectID && x is IDisposable);
			if (gameObject2 != null)
			{
				if (this._starSystemObjects != null && this._starSystemObjects.Crits.Objects.Contains(gameObject2))
				{
					this._starSystemObjects.Crits.Remove(gameObject2);
				}
				(gameObject2 as IDisposable).Dispose();
			}
			else
			{
				if (this._starSystemObjects != null)
				{
					gameObject2 = this._starSystemObjects.Crits.Objects.FirstOrDefault((IGameObject x) => x.ObjectID == gameObject.ObjectID && x is IDisposable);
					if (gameObject2 != null)
					{
						(gameObject2 as IDisposable).Dispose();
						this._starSystemObjects.Crits.Remove(gameObject);
					}
				}
				if (gameObject2 == null)
				{
					IGameObject gameObject3 = base.App.GetGameObject(gameObject.ObjectID);
					if (gameObject3 != null)
					{
						base.App.ReleaseObject(gameObject3);
					}
				}
			}
			this._crits.Remove(gameObject);
			foreach (CombatAI current in this.AI_Commanders)
			{
				current.ObjectRemoved(gameObject);
			}
			this._postLoadedObjects.Remove(gameObject);
		}
		private bool isHumanControlledAI(Player player)
		{
			if (player == null || !player.IsStandardPlayer)
			{
				return false;
			}
			bool result = false;
			if (!this.SimMode && !player.IsAI())
			{
				PendingCombat currentCombat = base.App.Game.GetCurrentCombat();
				if (currentCombat != null)
				{
					if (currentCombat.CombatStanceSelections.ContainsKey(player.ID))
					{
						ResolutionType resolutionType = currentCombat.CombatResolutionSelections[player.ID];
						if (resolutionType == ResolutionType.FIGHT || resolutionType == ResolutionType.FIGHT_ON_FIGHT)
						{
							CommonCombatState.Trace("Player " + player.ID + " chose not to simulate.");
							result = true;
						}
					}
				}
				else
				{
					result = !player.IsAI();
				}
			}
			if (!this.SimMode && player == base.App.Game.LocalPlayer)
			{
				CommonCombatState.Trace("Local player, not sim, setting AI to false.");
				result = true;
			}
			if (!this._authority)
			{
				result = true;
			}
			return result;
		}
		public void AddAItoCombat(Ship ship)
		{
			if (this.AI_Commanders.Count<CombatAI>() > 0)
			{
				foreach (CombatAI current in this.AI_Commanders)
				{
					if (current.m_Player == ship.Player)
					{
						return;
					}
				}
			}
			Dictionary<int, DiplomacyState> diploStates = null;
			if (this._initialDiploStates == null || !this._initialDiploStates.TryGetValue(ship.Player.ID, out diploStates))
			{
				diploStates = null;
			}
			int fleetID = 0;
			SpawnProfile spawnProfile = this._spawnPositions.FirstOrDefault((SpawnProfile x) => x._playerID == ship.Player.ID);
			if (spawnProfile != null)
			{
				fleetID = spawnProfile._fleetID;
			}
			FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(fleetID);
			CombatAI combatAI;
			if (((this._testingState || !ship.Player.IsStandardPlayer) && ship.Faction.UsesNPCCombatAI) || (fleetInfo != null && fleetInfo.IsTrapFleet) || ship.CombatAI == SectionEnumerations.CombatAiType.TrapDrone)
			{
				combatAI = new NPCFactionCombatAI(base.App, ship.Player, false, this._systemId, this._starSystemObjects, diploStates);
			}
			else
			{
				bool flag = this.isHumanControlledAI(ship.Player);
				CommonCombatState.Trace("Player controlled: " + flag);
				if (this._pirateEncounterData.IsPirateEncounter && this._pirateEncounterData.PiratePlayerIDs.Contains(ship.Player.ID))
				{
					combatAI = new PirateCombatAI(base.App, ship.Player, flag, this._starSystemObjects, diploStates);
				}
				else
				{
					if (base.App.Game.ScriptModules.Slaver != null && base.App.Game.ScriptModules.Slaver.PlayerID == ship.Player.ID)
					{
						combatAI = new SlaverCombatAI(base.App, ship.Player, flag, this._starSystemObjects, diploStates);
					}
					else
					{
						combatAI = new CombatAI(base.App, ship.Player, flag, this._starSystemObjects, diploStates, false);
					}
				}
			}
			combatAI.SimMode = this.SimMode;
			combatAI.m_FleetID = fleetID;
			combatAI.InTestMode = this._testingState;
			ShipInfo si = base.App.GameDatabase.GetShipInfo(ship.DatabaseID, false);
			if (si != null)
			{
				combatAI.m_SpawnProfile = this._spawnPositions.FirstOrDefault((SpawnProfile x) => x._fleetID == si.FleetID);
			}
			this.AI_Commanders.Add(combatAI);
		}
		public CombatAI GetCommanderForPlayerID(int playerID)
		{
			foreach (CombatAI current in this.AI_Commanders)
			{
				if (current.m_Player.ID == playerID)
				{
					return current;
				}
			}
			return null;
		}
		protected void AddObject(ScriptMessageReader data)
		{
			InteropGameObjectType interopGameObjectType = (InteropGameObjectType)data.ReadInteger();
			IGameObject gameObject = null;
			IGameObject gameObject2 = null;
			InteropGameObjectType interopGameObjectType2 = interopGameObjectType;
			if (interopGameObjectType2 == InteropGameObjectType.IGOT_SHIP)
			{
				int id = data.ReadInteger();
				gameObject2 = base.App.GetGameObject(id);
				Vector3 position = default(Vector3);
				Vector3 forward = default(Vector3);
				position.X = data.ReadSingle();
				position.Y = data.ReadSingle();
				position.Z = data.ReadSingle();
				forward.X = data.ReadSingle();
				forward.Y = data.ReadSingle();
				forward.Z = data.ReadSingle();
				Matrix world = Matrix.CreateWorld(position, forward, Vector3.UnitY);
				int num = data.ReadInteger();
				int parentId = data.ReadInteger();
				int playerId = data.ReadInteger();
				bool value = data.ReadBool();
				if (num != 0)
				{
					DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(num);
					ShipInfo shipInfo = new ShipInfo
					{
						DesignID = 0,
						DesignInfo = designInfo,
						FleetID = 0,
						ParentID = 0,
						SerialNumber = 0,
						ShipName = string.Empty
					};
					gameObject = Ship.CreateShip(base.App.Game, world, shipInfo, parentId, this._input.ObjectID, playerId, this._starSystemObjects.IsDeepSpace, this._playersInCombat);
					gameObject.PostSetProp("IsMirage", value);
				}
			}
			if (gameObject != null)
			{
				this._postLoadedObjects.Add(gameObject);
			}
			if (gameObject2 != null)
			{
				gameObject2.PostNotifyObjectHasBeenAdded((gameObject != null) ? gameObject.ObjectID : 0);
			}
		}
		protected void AddObjects(ScriptMessageReader data)
		{
			InteropGameObjectType interopGameObjectType = (InteropGameObjectType)data.ReadInteger();
			List<IGameObject> list = new List<IGameObject>();
			IGameObject gameObject = null;
			InteropGameObjectType interopGameObjectType2 = interopGameObjectType;
			if (interopGameObjectType2 == InteropGameObjectType.IGOT_SHIP)
			{
				int id = data.ReadInteger();
				gameObject = base.App.GetGameObject(id);
				int num = data.ReadInteger();
				int designID = data.ReadInteger();
				int parentId = data.ReadInteger();
				int playerId = data.ReadInteger();
				DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(designID);
				ShipInfo shipInfo = new ShipInfo
				{
					DesignID = 0,
					DesignInfo = designInfo,
					FleetID = 0,
					ParentID = 0,
					SerialNumber = 0,
					ShipName = string.Empty
				};
				for (int i = 0; i < num; i++)
				{
					list.Add(Ship.CreateShip(base.App.Game, Matrix.Identity, shipInfo, parentId, this._input.ObjectID, playerId, this._starSystemObjects.IsDeepSpace, this._playersInCombat));
				}
			}
			if (list.Count > 0)
			{
				this._postLoadedObjects.AddRange(list.ToArray());
			}
			if (gameObject != null)
			{
				gameObject.PostNotifyObjectsHaveBeenAdded((
					from x in list
					select x.ObjectID).ToArray<int>());
			}
		}
		protected void RemoveObject(ScriptMessageReader data)
		{
			int id = data.ReadInteger();
			this.RemoveGameObject(base.App.GetGameObject(id));
		}
		protected void RemoveObjects(ScriptMessageReader data)
		{
			for (int id = data.ReadInteger(); id != 0; id = data.ReadInteger())
			{
				this.RemoveGameObject(base.App.GetGameObject(id));
			}
		}
		private bool CheckVictory()
		{
			foreach (CombatAI current in this.AI_Commanders)
			{
				if (current.VictoryConditionsAreMet())
				{
					bool result = true;
					return result;
				}
			}
			List<Player> playersWithAssets = this._playersWithAssets;
			if (base.App.Game.ScriptModules.NeutronStar != null)
			{
				playersWithAssets.RemoveAll(x => x.ID == base.App.Game.ScriptModules.NeutronStar.PlayerID);
			}
			foreach (int x in this._combatStanceMap.Keys.ToList<int>())
			{
				if (playersWithAssets.Any(p => p.ID == x))
				{
					foreach (int y in this._combatStanceMap[x].Keys.ToList<int>())
					{
						if (playersWithAssets.Any((Player p) => p.ID == y) && !this._combatStanceMap[x][y])
						{
							bool result = false;
							return result;
						}
					}
				}
			}
			return true;
		}
		private bool EncounterIsNeutral(int playerAID, int playerBID, AsteroidMonitorInfo ami, MorrigiRelicInfo mri)
		{
			if (ami == null && mri == null)
			{
				return false;
			}
			if (ami != null && base.App.Game.ScriptModules.AsteroidMonitor != null)
			{
				if (base.App.Game.ScriptModules.AsteroidMonitor.PlayerID == playerAID)
				{
					return !ami.IsAggressive;
				}
				if (base.App.Game.ScriptModules.AsteroidMonitor.PlayerID == playerBID)
				{
					return !ami.IsAggressive;
				}
			}
			if (mri != null && base.App.Game.ScriptModules.MorrigiRelic != null)
			{
				if (base.App.Game.ScriptModules.MorrigiRelic.PlayerID == playerAID)
				{
					return !mri.IsAggressive;
				}
				if (base.App.Game.ScriptModules.MorrigiRelic.PlayerID == playerBID)
				{
					return !mri.IsAggressive;
				}
			}
			return false;
		}
		private void RebuildInitialDiploStates(AsteroidMonitorInfo ami, MorrigiRelicInfo mri)
		{
			this._initialDiploStates = new Dictionary<int, Dictionary<int, DiplomacyState>>();
			foreach (Player player1 in this._playersInCombat)
			{
				Dictionary<int, DiplomacyState> dictionary;
				if (!this._initialDiploStates.TryGetValue(player1.ID, out dictionary))
				{
					dictionary = new Dictionary<int, DiplomacyState>();
					this._initialDiploStates[player1.ID] = dictionary;
				}
				foreach (Player player2 in this._playersInCombat)
				{
					if (this._testingState)
					{
						dictionary[player2.ID] = ((player1.ID == player2.ID) ? DiplomacyState.NEUTRAL : DiplomacyState.WAR);
					}
					else
					{
						if (this._trapCombatData.IsTrapCombat)
						{
							if (this._trapCombatData.TrapPlayers.Any((int x) => x == player1.ID || x == player2.ID))
							{
								dictionary[player2.ID] = DiplomacyState.WAR;
								continue;
							}
						}
						if (this._pirateEncounterData.IsPirateEncounter)
						{
							if (this._pirateEncounterData.PiratePlayerIDs.Any((int x) => x == player1.ID || x == player2.ID))
							{
								dictionary[player2.ID] = DiplomacyState.WAR;
								continue;
							}
						}
						if (!this.EncounterIsNeutral(player1.ID, player2.ID, ami, mri))
						{
							dictionary[player2.ID] = base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(player1.ID, player2.ID);
						}
						else
						{
							dictionary[player2.ID] = DiplomacyState.NEUTRAL;
						}
					}
				}
			}
		}
		private void UpdateDiploStatesInScript()
		{
			if (this._initialDiploStates == null || this._trapCombatData.IsTrapCombat || this._pirateEncounterData.IsPirateEncounter)
			{
				return;
			}
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, Dictionary<int, DiplomacyState>> current in this._initialDiploStates)
			{
				PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(current.Key);
				if (playerInfo != null && playerInfo.isStandardPlayer)
				{
					foreach (KeyValuePair<int, DiplomacyState> current2 in current.Value)
					{
						if (!list.Contains(current2.Key))
						{
							playerInfo = base.App.GameDatabase.GetPlayerInfo(current2.Key);
							if (playerInfo != null && playerInfo.isStandardPlayer)
							{
								DiplomacyInfo diplomacyInfo = base.App.GameDatabase.GetDiplomacyInfo(current.Key, current2.Key);
								if (diplomacyInfo != null)
								{
									base.App.Game.GameDatabase.UpdateDiplomacyState(current.Key, current2.Key, current2.Value, diplomacyInfo.Relations, true);
								}
							}
						}
					}
					list.Add(current.Key);
				}
			}
		}
		public DiplomacyState GetDiplomacyState(int playerA, int playerB)
		{
			DiplomacyState result = DiplomacyState.NEUTRAL;
			if (this._initialDiploStates != null)
			{
				Dictionary<int, DiplomacyState> dictionary;
				if (!this._initialDiploStates.TryGetValue(playerA, out dictionary))
				{
					result = DiplomacyState.NEUTRAL;
				}
				else
				{
					if (!dictionary.TryGetValue(playerB, out result))
					{
						result = DiplomacyState.NEUTRAL;
					}
				}
			}
			return result;
		}
		public void SetDiplomacyState(int playerA, int playerB, DiplomacyState state)
		{
			if (this._initialDiploStates != null)
			{
				if (this._initialDiploStates.ContainsKey(playerA) && this._initialDiploStates[playerA].ContainsKey(playerB))
				{
					this._initialDiploStates[playerA][playerB] = state;
				}
				if (this._initialDiploStates.ContainsKey(playerB) && this._initialDiploStates[playerB].ContainsKey(playerA))
				{
					this._initialDiploStates[playerB][playerA] = state;
				}
			}
		}
		private void DetectionUpdate(List<IGameObject> objects)
		{
			List<int> list = new List<int>();
			List<Ship> list2 = new List<Ship>();
			List<Ship> list3 = new List<Ship>();
			List<StellarBody> list4 = new List<StellarBody>();
			this.m_DetectionSpheres.Clear();
			this.m_SlewPlanetDetectionSpheres.Clear();
			this._playersWithAssets.Clear();
			float val = CombatAI.GetMaxWeaponRangeFromShips(objects.OfType<Ship>().ToList<Ship>()) + 2000f;
			foreach (IGameObject current in objects)
			{
				if (current is Ship)
				{
					Ship ship = current as Ship;
					if (!Ship.IsActiveShip(ship))
					{
						if (ship.DockedWithParent)
						{
							list3.Add(ship);
						}
					}
					else
					{
						DetectionSpheres detectionSpheres = new DetectionSpheres(ship.Player.ID, ship.Maneuvering.Position);
						detectionSpheres.minRadius = ship.ShipSphere.radius + 2000f;
						detectionSpheres.sensorRange = ship.SensorRange;
						detectionSpheres.slewModeRange = base.App.AssetDatabase.SlewModeExitRange;
						detectionSpheres.ignoreNeutralPlanets = (ship.ShipClass == ShipClass.Station);
						DetectionSpheres detectionSpheres2 = new DetectionSpheres(ship.Player.ID, ship.Maneuvering.Position);
						detectionSpheres2.slewModeRange = Math.Max(detectionSpheres.slewModeRange, CombatAI.GetMaxWeaponRange(ship, false));
						detectionSpheres2.ignoreNeutralPlanets = (ship.ShipClass == ShipClass.Station);
						if (!this.m_SlewMode)
						{
							detectionSpheres.slewModeRange = Math.Max(detectionSpheres.slewModeRange + base.App.AssetDatabase.SlewModeEnterOffset, base.App.AssetDatabase.SlewModeExitRange + 2000f);
							detectionSpheres2.slewModeRange += base.App.AssetDatabase.SlewModeEnterOffset;
						}
						if (ship.IsUnderAttack)
						{
							detectionSpheres.sensorRange = Math.Max(detectionSpheres.sensorRange, val);
						}
						this.m_DetectionSpheres.Add(detectionSpheres);
						this.m_SlewPlanetDetectionSpheres.Add(detectionSpheres2);
						float length = ship.Maneuvering.Velocity.Length;
						ship.Signature = Math.Min(ship.Signature - 0.05f, length);
						if (!list.Contains(ship.Player.ID))
						{
							list.Add(ship.Player.ID);
						}
						if (!this._playersWithAssets.Contains(ship.Player))
						{
							if (this._playersInCombat.Any((Player x) => x == ship.Player))
							{
								this._playersWithAssets.Add(ship.Player);
							}
						}
						list2.Add(current as Ship);
					}
				}
				else
				{
					if (current is StellarBody)
					{
						StellarBody stellarBody = current as StellarBody;
						if (stellarBody.Parameters.ColonyPlayerID != 0 && stellarBody.Population > 0.0)
						{
							if (!list.Contains(stellarBody.Parameters.ColonyPlayerID))
							{
								list.Add(stellarBody.Parameters.ColonyPlayerID);
							}
							Player p = base.App.Game.GetPlayerObject(stellarBody.Parameters.ColonyPlayerID);
							if (p != null && !this._playersWithAssets.Contains(p) && this._playersInCombat.Any((Player x) => x == p))
							{
								this._playersWithAssets.Add(p);
							}
							DetectionSpheres detectionSpheres3 = new DetectionSpheres(stellarBody.Parameters.ColonyPlayerID, stellarBody.Parameters.Position);
							detectionSpheres3.minRadius = stellarBody.Parameters.Radius + Math.Min(base.App.AssetDatabase.DefaultPlanetSensorRange * 0.25f, 2000f);
							detectionSpheres3.sensorRange = stellarBody.Parameters.Radius + base.App.AssetDatabase.DefaultPlanetSensorRange;
							detectionSpheres3.slewModeRange = detectionSpheres3.sensorRange;
							detectionSpheres3.isPlanet = true;
							detectionSpheres3.ignoreNeutralPlanets = true;
							this.m_DetectionSpheres.Add(detectionSpheres3);
							list4.Add(current as StellarBody);
						}
					}
				}
			}
			bool flag = list.Count > 1;
			foreach (Ship current2 in list2)
			{
				foreach (int current3 in list)
				{
					if (current3 != current2.Player.ID)
					{
						Ship.DetectionState detectionStateForPlayer = current2.GetDetectionStateForPlayer(current3);
						bool spotted = detectionStateForPlayer.spotted;
						detectionStateForPlayer.scanned = false;
						float num = current2.ShipSphere.radius + current2.BonusSpottedRange;
						detectionStateForPlayer.spotted = (current2.BonusSpottedRange < 0f);
						foreach (DetectionSpheres current4 in this.m_DetectionSpheres)
						{
							if (current4.playerID == current3)
							{
								float lengthSquared = (current4.center - current2.Maneuvering.Position).LengthSquared;
								if (!detectionStateForPlayer.spotted)
								{
									float num2 = current4.minRadius + num;
									if (lengthSquared <= num2 * num2)
									{
										detectionStateForPlayer.spotted = true;
									}
								}
								if (!detectionStateForPlayer.scanned)
								{
									float num3 = current4.sensorRange + current2.ShipSphere.radius;
									if (num3 * num3 > lengthSquared)
									{
										detectionStateForPlayer.scanned = true;
									}
								}
								if (flag)
								{
									DiplomacyState diplomacyState = this.GetDiplomacyState(current2.Player.ID, current3);
									if (diplomacyState == DiplomacyState.WAR || (diplomacyState != DiplomacyState.ALLIED && (!current4.isPlanet || current2.ShipClass != ShipClass.Station)))
									{
										float num4 = current4.slewModeRange + current2.ShipSphere.radius;
										if (num4 * num4 > lengthSquared)
										{
											flag = false;
										}
									}
								}
								if (detectionStateForPlayer.spotted && detectionStateForPlayer.scanned && !flag)
								{
									break;
								}
							}
						}
						if (current3 == base.App.Game.LocalPlayer.ID && (detectionStateForPlayer.spotted != spotted || current2.Visible != detectionStateForPlayer.spotted))
						{
							current2.Visible = detectionStateForPlayer.spotted;
						}
					}
					else
					{
						if (current3 == base.App.Game.LocalPlayer.ID && !current2.Visible)
						{
							current2.Visible = true;
							Ship.DetectionState detectionStateForPlayer2 = current2.GetDetectionStateForPlayer(current3);
							detectionStateForPlayer2.spotted = true;
							detectionStateForPlayer2.scanned = true;
						}
					}
				}
			}
			foreach (Ship rider in list3)
			{
				Ship ship2 = list2.FirstOrDefault((Ship x) => x.DatabaseID == rider.ParentDatabaseID);
				if (ship2 != null)
				{
					foreach (int current5 in list)
					{
						Ship.DetectionState detectionStateForPlayer3 = rider.GetDetectionStateForPlayer(current5);
						if (current5 == base.App.Game.LocalPlayer.ID)
						{
							detectionStateForPlayer3.spotted = true;
							detectionStateForPlayer3.scanned = true;
						}
						else
						{
							detectionStateForPlayer3.spotted = false;
							detectionStateForPlayer3.scanned = false;
						}
					}
					if (rider.Visible != ship2.Visible)
					{
						rider.Visible = ship2.Visible;
					}
				}
			}
			if (flag)
			{
				foreach (StellarBody current6 in list4)
				{
					foreach (int current7 in list)
					{
						if (current6.Parameters.ColonyPlayerID != 0 && current7 != current6.Parameters.ColonyPlayerID)
						{
							foreach (DetectionSpheres current8 in this.m_SlewPlanetDetectionSpheres)
							{
								if (current8.playerID == current7)
								{
									DiplomacyState diplomacyState2 = this.GetDiplomacyState(current6.Parameters.ColonyPlayerID, current7);
									if (diplomacyState2 == DiplomacyState.WAR || (diplomacyState2 != DiplomacyState.ALLIED && !current8.ignoreNeutralPlanets))
									{
										Vector3 vector = current8.center - current6.Parameters.Position;
										float num5 = current8.slewModeRange + current6.Parameters.Radius;
										if (num5 * num5 > vector.LengthSquared)
										{
											flag = false;
											break;
										}
									}
								}
							}
						}
						if (!flag)
						{
							break;
						}
					}
					if (!flag)
					{
						break;
					}
				}
			}
			if (this._pointsOfInterest.Count > 0)
			{
				foreach (PointOfInterest poi in this._pointsOfInterest)
				{
					if (!poi.HasBeenSeen)
					{
						if (poi.TargetID != 0)
						{
							IGameObject gameObject = this._crits.Objects.FirstOrDefault((IGameObject x) => x.ObjectID == poi.TargetID);
							if (gameObject != null && gameObject is Ship)
							{
								Ship.DetectionState detectionStateForPlayer4 = (gameObject as Ship).GetDetectionStateForPlayer(base.App.LocalPlayer.ID);
								poi.HasBeenSeen = detectionStateForPlayer4.scanned;
								continue;
							}
						}
						foreach (DetectionSpheres current9 in this.m_DetectionSpheres)
						{
							if (current9.playerID == base.App.LocalPlayer.ID)
							{
								Vector3 vector2 = current9.center - poi.Position;
								float num6 = current9.minRadius + base.App.AssetDatabase.GlobalSpotterRangeData.SpotterValues[1] + 2000f;
								if (num6 * num6 > vector2.LengthSquared)
								{
									poi.HasBeenSeen = true;
									break;
								}
							}
						}
					}
				}
			}
			if (this.m_SlewMode != flag)
			{
				this.m_SlewMode = flag;
				this._combat.PostSetProp("SetSlewMode", this.m_SlewMode);
			}
		}
		private void CheckToReturnControlZonesToOwner(int playerID)
		{
			List<FleetInfo> list = base.App.GameDatabase.GetFleetsByPlayerAndSystem(playerID, this._systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			int num = 0;
			foreach (FleetInfo current in list)
			{
				MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(current.ID);
				if (missionByFleetID == null || (missionByFleetID.Type != MissionType.RETREAT && missionByFleetID.Type != MissionType.RETURN))
				{
					num++;
				}
			}
			if (num == 0)
			{
				StarSystem.RemoveSystemPlayerColor(base.App.GameDatabase, this._systemId, playerID);
			}
		}
		private void ApplyRewardsForShipDeath(ShipInfo si, int playerID, int killedByPlayerGOID, FleetInfo fi = null)
		{
			if (this._pirateEncounterData.IsPirateEncounter && this._pirateEncounterData.PiratePlayerIDs.Contains(playerID))
			{
				Player playerByObjectID = base.App.GetPlayerByObjectID(killedByPlayerGOID);
				if (playerByObjectID != null)
				{
					PiracyGlobalData.PiracyBountyType piracyBountyType = (base.App.Game.ScriptModules.Pirates.PirateBaseDesignId == si.DesignID) ? PiracyGlobalData.PiracyBountyType.PirateBaseDestroyed : PiracyGlobalData.PiracyBountyType.PirateShipDestroyed;
					int num = base.App.AssetDatabase.GlobalPiracyData.Bounties[(int)piracyBountyType];
					if (this._pirateEncounterData.PlayerBounties.ContainsKey(playerByObjectID.ID))
					{
						Dictionary<int, int> playerBounties;
						int iD;
						(playerBounties = this._pirateEncounterData.PlayerBounties)[iD = playerByObjectID.ID] = playerBounties[iD] + num;
					}
					else
					{
						this._pirateEncounterData.PlayerBounties.Add(playerByObjectID.ID, num);
					}
					if (piracyBountyType == PiracyGlobalData.PiracyBountyType.PirateBaseDestroyed)
					{
						List<int> list = base.App.GameDatabase.GetStandardPlayerIDs().ToList<int>();
						foreach (int current in list)
						{
							if (current != playerByObjectID.ID)
							{
								string factionName = base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(current));
								int reactionAmount = 0;
								base.App.AssetDatabase.GlobalPiracyData.ReactionBonuses.TryGetValue(factionName, out reactionAmount);
								base.App.GameDatabase.ApplyDiplomacyReaction(playerByObjectID.ID, current, reactionAmount, null, 1);
							}
						}
						base.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_PIRATE_BASE_DESTROYED,
							EventMessage = TurnEventMessage.EM_PIRATE_BASE_DESTROYED,
							PlayerID = playerByObjectID.ID,
							SystemID = this._systemId,
							TurnNumber = base.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						return;
					}
					if (playerID == base.App.Game.ScriptModules.Pirates.PlayerID)
					{
						PirateBaseInfo pirateBaseInfo = base.App.GameDatabase.GetPirateBaseInfos().FirstOrDefault((PirateBaseInfo x) => x.SystemId == this._systemId);
						if (pirateBaseInfo != null)
						{
							pirateBaseInfo.NumShips = Math.Max(pirateBaseInfo.NumShips - 1, 0);
							base.App.GameDatabase.UpdatePirateBaseInfo(pirateBaseInfo);
							return;
						}
					}
				}
			}
			else
			{
				if (this._pirateEncounterData.IsPirateEncounter && si.DesignInfo.Role == ShipRole.FREIGHTER)
				{
					Player playerByObjectID2 = base.App.GetPlayerByObjectID(killedByPlayerGOID);
					if (playerByObjectID2 != null)
					{
						int num2 = base.App.AssetDatabase.GlobalPiracyData.Bounties[2];
						if (this._pirateEncounterData.PlayerBounties.ContainsKey(playerByObjectID2.ID))
						{
							Dictionary<int, int> playerBounties2;
							int iD2;
							(playerBounties2 = this._pirateEncounterData.PlayerBounties)[iD2 = playerByObjectID2.ID] = playerBounties2[iD2] + num2;
							return;
						}
						this._pirateEncounterData.PlayerBounties.Add(playerByObjectID2.ID, num2);
						return;
					}
				}
				else
				{
					if (base.App.Game.ScriptModules.GhostShip != null && base.App.Game.ScriptModules.GhostShip.PlayerID == playerID)
					{
						List<PlayerInfo> list2 = base.App.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>();
						foreach (PlayerInfo current2 in list2)
						{
							if (current2.isStandardPlayer && current2.ID != playerID)
							{
								DiplomacyInfo diplomacyInfo = base.App.GameDatabase.GetDiplomacyInfo(playerID, current2.ID);
								if (diplomacyInfo != null)
								{
									base.App.Game.GameDatabase.UpdateDiplomacyState(playerID, current2.ID, diplomacyInfo.State, diplomacyInfo.Relations + 100, true);
								}
							}
						}
						Player playerByObjectID3 = base.App.GetPlayerByObjectID(killedByPlayerGOID);
						if (playerByObjectID3 != null)
						{
							float stratModifier = base.App.GameDatabase.GetStratModifier<float>(StratModifiers.LeviathanResearchModifier, playerByObjectID3.ID);
							base.App.GameDatabase.SetStratModifier(StratModifiers.LeviathanResearchModifier, playerByObjectID3.ID, stratModifier + 0.2f);
							GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_GHOSTSHIP_KILLED, playerByObjectID3.ID, null, null, null);
							return;
						}
					}
					else
					{
						if (base.App.Game.ScriptModules.VonNeumann != null && base.App.Game.ScriptModules.VonNeumann.PlayerID == playerID)
						{
							if (si.DesignID == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId)
							{
								Player playerByObjectID4 = base.App.GetPlayerByObjectID(killedByPlayerGOID);
								if (playerByObjectID4 != null && playerID != playerByObjectID4.ID)
								{
									PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(playerByObjectID4.ID);
									double availableRevenue = base.App.Game.CalculateNetRevenue(playerInfo);
									int num3 = (int)GameSession.SplitResearchRevenue(playerInfo, availableRevenue);
									int num4 = base.App.Game.ConvertToResearchPoints(playerInfo.ID, (double)num3);
									base.App.GameDatabase.UpdatePlayerAdditionalResearchPoints(playerByObjectID4.ID, playerInfo.AdditionalResearchPoints + (int)((float)num4 * 0.05f));
									return;
								}
							}
						}
						else
						{
							if (base.App.Game.ScriptModules.Locust != null && base.App.Game.ScriptModules.Locust.PlayerID == playerID)
							{
								if (si.DesignID == base.App.Game.ScriptModules.Locust.HeraldMoonDesignId || si.DesignID == base.App.Game.ScriptModules.Locust.WorldShipDesignId)
								{
									Player playerByObjectID5 = base.App.GetPlayerByObjectID(killedByPlayerGOID);
									if (playerByObjectID5 != null)
									{
										base.App.GameDatabase.InsertTurnEvent(new TurnEvent
										{
											EventType = TurnEventType.EV_LOCUST_SHIP_DESTROYED,
											EventMessage = TurnEventMessage.EM_LOCUST_SHIP_DESTROYED,
											PlayerID = playerByObjectID5.ID,
											SystemID = this._systemId,
											TurnNumber = base.App.GameDatabase.GetTurnCount(),
											ShowsDialog = false
										});
										bool flag = false;
										List<LocustSwarmInfo> list3 = base.App.GameDatabase.GetLocustSwarmInfos().ToList<LocustSwarmInfo>();
										foreach (LocustSwarmInfo current3 in list3)
										{
											if (current3.FleetId.HasValue)
											{
												List<ShipInfo> source = base.App.GameDatabase.GetShipInfoByFleetID(current3.FleetId.Value, true).ToList<ShipInfo>();
												flag = source.Any((ShipInfo x) => (x.DesignID == this.App.Game.ScriptModules.Locust.HeraldMoonDesignId || x.DesignID == this.App.Game.ScriptModules.Locust.WorldShipDesignId) && x.ID != si.ID);
												if (flag)
												{
													break;
												}
											}
										}
										if (!flag)
										{
											base.App.GameDatabase.InsertTurnEvent(new TurnEvent
											{
												EventType = TurnEventType.EV_LOCUST_INFESTATION_DEFEATED,
												EventMessage = TurnEventMessage.EM_LOCUST_INFESTATION_DEFEATED,
												PlayerID = playerByObjectID5.ID,
												TurnNumber = base.App.GameDatabase.GetTurnCount(),
												ShowsDialog = false
											});
											return;
										}
									}
								}
							}
							else
							{
								if (si.DesignInfo.StationType == StationType.GATE || (fi != null && fi.IsGateFleet))
								{
									base.App.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_GATE_DESTROYED,
										EventMessage = TurnEventMessage.EM_GATE_DESTROYED,
										PlayerID = si.DesignInfo.PlayerID,
										SystemID = this._systemId,
										TurnNumber = base.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
									return;
								}
								if (base.App.Game.ScriptModules.Swarmers != null && base.App.Game.ScriptModules.Swarmers.PlayerID == playerID)
								{
									double num5 = 0.0;
									if (si.DesignID == base.App.Game.ScriptModules.Swarmers.HiveDesignID)
									{
										num5 = 50000.0;
									}
									else
									{
										if (si.DesignID == base.App.Game.ScriptModules.Swarmers.SwarmQueenDesignID)
										{
											num5 = 40000.0;
										}
									}
									if (num5 > 0.0)
									{
										Player playerByObjectID6 = base.App.GetPlayerByObjectID(killedByPlayerGOID);
										if (playerByObjectID6 != null && playerByObjectID6.ID != playerID)
										{
											PlayerInfo playerInfo2 = base.App.GameDatabase.GetPlayerInfo(playerID);
											if (playerInfo2 != null && playerInfo2.isStandardPlayer)
											{
												base.App.GameDatabase.UpdatePlayerSavings(playerInfo2.ID, playerInfo2.Savings + num5);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		private void ProcessShipsHitByNodeCannon()
		{
			if (this._systemId == 0 || this._hitByNodeCannon.Count == 0)
			{
				return;
			}
			Vector3 systemOrigin = base.App.GameDatabase.GetStarSystemOrigin(this._systemId);
			List<StarSystemInfo> list = base.App.GameDatabase.GetSystemsInRange(systemOrigin, 10f).ToList<StarSystemInfo>();
			foreach (StarSystemInfo current in list)
			{
				if (current.ID == this._systemId)
				{
					list.Remove(current);
					break;
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			list.Sort(delegate(StarSystemInfo a, StarSystemInfo b)
			{
				float lengthSquared = (a.Origin - systemOrigin).LengthSquared;
				float lengthSquared2 = (b.Origin - systemOrigin).LengthSquared;
				return lengthSquared.CompareTo(lengthSquared2);
			});
			int index = this._random.NextInclusive(0, Math.Max(Math.Min(list.Count - 1, 3), 0));
			StarSystemInfo starSystemInfo = list[index];
			Dictionary<int, List<Ship>> dictionary = new Dictionary<int, List<Ship>>();
			foreach (Ship current2 in this._hitByNodeCannon)
			{
				if (dictionary.ContainsKey(current2.Player.ID))
				{
					dictionary[current2.Player.ID].Add(current2);
				}
				else
				{
					dictionary.Add(current2.Player.ID, new List<Ship>
					{
						current2
					});
				}
			}
			foreach (KeyValuePair<int, List<Ship>> fleets in dictionary)
			{
				KeyValuePair<int, List<Ship>> fleets5 = fleets;
				if (fleets5.Value.Count != 0)
				{
					FleetInfo fleetInfo = this._lastPendingCombat.CombatResults.FleetsInCombat.FirstOrDefault(delegate(FleetInfo x)
					{
						int arg_14_0 = x.PlayerID;
						KeyValuePair<int, List<Ship>> fleets4 = fleets;
						return arg_14_0 == fleets4.Key;
					});
					GameDatabase arg_222_0 = base.App.GameDatabase;
					KeyValuePair<int, List<Ship>> fleets2 = fleets;
					int num = arg_222_0.InsertFleet(fleets2.Key, 0, starSystemInfo.ID, (fleetInfo != null) ? fleetInfo.SupportingSystemID : 0, App.Localize("@FLEET_NODE_CANNONED_FLEET"), FleetType.FL_NORMAL);
					base.App.GameDatabase.UpdateFleetLocation(num, starSystemInfo.ID, null);
					int missionID = base.App.GameDatabase.InsertMission(num, MissionType.RETREAT, 0, 0, 0, 0, false, null);
					base.App.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, null);
					base.App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, null);
					KeyValuePair<int, List<Ship>> fleets3 = fleets;
					foreach (Ship current3 in fleets3.Value)
					{
						if (current3 != null)
						{
							ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(current3.DatabaseID, false);
							if (shipInfo != null)
							{
								FleetInfo fleetInfo2 = base.App.GameDatabase.GetFleetInfo(shipInfo.FleetID);
								base.App.GameDatabase.TransferShip(current3.DatabaseID, num);
								if (fleetInfo2 != null && fleetInfo2.ID != num && base.App.GameDatabase.GetShipsByFleetID(fleetInfo2.ID).Count<int>() == 0)
								{
									base.App.GameDatabase.RemoveFleet(fleetInfo2.ID);
								}
								base.App.GameDatabase.InsertTurnEvent(new TurnEvent
								{
									EventType = TurnEventType.EV_SHIPS_SCATTERED_NODE_CANNON,
									EventMessage = TurnEventMessage.EM_SHIPS_SCATTERED_NODE_CANNON,
									PlayerID = current3.Player.ID,
									ShipID = current3.DatabaseID,
									TurnNumber = base.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
							}
						}
					}
				}
			}
			foreach (FleetInfo current4 in this._lastPendingCombat.CombatResults.FleetsInCombat)
			{
				PlayerInfo playerInfo = (current4 != null) ? base.App.GameDatabase.GetPlayerInfo(current4.PlayerID) : null;
				if (playerInfo != null && playerInfo.isStandardPlayer && !current4.IsDefenseFleet && !current4.IsGateFleet)
				{
					int retreatFleetID = base.App.Game.MCCarryOverData.GetRetreatFleetID(current4.SystemID, current4.ID);
					if (retreatFleetID == 0 || retreatFleetID != current4.ID)
					{
						List<ShipInfo> ships = base.App.GameDatabase.GetShipInfoByFleetID(current4.ID, false).ToList<ShipInfo>();
						int shipsCommandPointQuota = base.App.GameDatabase.GetShipsCommandPointQuota(ships);
						int shipsCommandPointCost = base.App.GameDatabase.GetShipsCommandPointCost(ships);
						if (shipsCommandPointQuota < shipsCommandPointCost)
						{
							if (retreatFleetID != 0)
							{
								List<int> list2 = base.App.GameDatabase.GetShipsByFleetID(retreatFleetID).ToList<int>();
								foreach (int current5 in list2)
								{
									base.App.GameDatabase.TransferShip(current5, current4.ID);
								}
								MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(retreatFleetID);
								if (missionByFleetID != null)
								{
									base.App.GameDatabase.RemoveMission(missionByFleetID.ID);
								}
								base.App.GameDatabase.RemoveFleet(retreatFleetID);
								base.App.Game.MCCarryOverData.SetRetreatFleetID(current4.SystemID, current4.ID, current4.ID);
							}
							MissionInfo missionByFleetID2 = base.App.GameDatabase.GetMissionByFleetID(current4.ID);
							if (missionByFleetID2 != null)
							{
								base.App.GameDatabase.RemoveMission(missionByFleetID2.ID);
							}
							int missionID2 = base.App.GameDatabase.InsertMission(current4.ID, MissionType.RETREAT, 0, 0, 0, 0, false, null);
							base.App.GameDatabase.InsertWaypoint(missionID2, WaypointType.ReturnHome, null);
							this.CheckToReturnControlZonesToOwner(playerInfo.ID);
						}
					}
				}
			}
		}
	}
}
