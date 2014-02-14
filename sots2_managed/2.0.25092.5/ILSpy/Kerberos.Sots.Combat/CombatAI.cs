using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class CombatAI
	{
		public const float kEnemyGroupThreshold = 1.25f;
		public const float kEnemyGroupDistance = 2000f;
		public const float kPlanetPatrolOffset = 1500f;
		public const float kStarPatrolOffset = 2500f;
		public const float kPlanetOffset = 750f;
		public const float kStarOffset = 7500f;
		public const float kBufferDist = 500f;
		public const int kTaskGroupUpdateRate = 5;
		public const int kTaskGroupUpdateRateSimMode = 2;
		protected App m_Game;
		private OverallAIType m_AIType;
		private bool m_UseCTAMainMenu;
		protected int m_UpdateCounter;
		protected int m_NodeMawUpdateRate;
		protected int m_FramesElapsed;
		public int m_FleetID;
		public Player m_Player;
		public bool m_bIsHumanPlayerControlled;
		public bool m_bHasListener;
		protected List<Ship> m_Friendly;
		protected List<Ship> m_Enemy;
		protected List<StellarBody> m_Planets;
		protected List<StarModel> m_Stars;
		protected List<TaskGroup> m_TaskGroups;
		protected List<EnemyGroup> m_EnemyGroups;
		protected List<SpecWeaponControl> m_ShipWeaponControls;
		protected List<ShipPsionicControl> m_ShipPsionicControls;
		protected List<NodeMawInfo> m_NodeMaws;
		private List<int> m_EncounterPlayerIDs;
		private Dictionary<int, DiplomacyState> _currentDiploStates;
		protected int m_EGUpdatePhase;
		protected List<TacticalObjective> m_Objectives;
		protected CombatZoneEnemySpotted m_SpottedEnemies;
		protected CloakedShipDetection m_CloakedEnemyDetection;
		protected bool m_bEnemyShipsInSystem;
		public bool m_bPlanetsInitialized;
		public bool m_bObjectivesInitialized;
		private bool m_OwnsSystem;
		public SpawnProfile m_SpawnProfile;
		private Random m_Random;
		private int m_SystemID;
		private bool m_IsEncounterCombat;
		private float m_SystemRadius;
		private float m_MinSystemRadius;
		private bool _simMode;
		private bool _inTestMode;
		public bool OwnsSystem
		{
			get
			{
				return this.m_OwnsSystem;
			}
		}
		public Random AIRandom
		{
			get
			{
				return this.m_Random;
			}
		}
		public bool IsEncounterCombat
		{
			get
			{
				return this.m_IsEncounterCombat;
			}
		}
		public float SystemRadius
		{
			get
			{
				return this.m_SystemRadius;
			}
		}
		public float MinSystemRadius
		{
			get
			{
				return this.m_MinSystemRadius;
			}
		}
		public bool EnemiesPresentInSystem
		{
			get
			{
				return this.m_bEnemyShipsInSystem;
			}
		}
		public List<StellarBody> PlanetsInSystem
		{
			get
			{
				return this.m_Planets;
			}
		}
		public List<StarModel> StarsInSystem
		{
			get
			{
				return this.m_Stars;
			}
		}
		public bool SimMode
		{
			get
			{
				return this._simMode;
			}
			set
			{
				this._simMode = value;
			}
		}
		public bool InTestMode
		{
			get
			{
				return this._inTestMode;
			}
			set
			{
				this._inTestMode = value;
			}
		}
		public void SetAIType(OverallAIType type)
		{
			this.m_AIType = type;
		}
		public OverallAIType GetAIType()
		{
			return this.m_AIType;
		}
		public CombatAI(App game, Player player, bool playerControlled, Kerberos.Sots.GameStates.StarSystem starSystem, Dictionary<int, DiplomacyState> diploStates, bool useCTAMainMenu = false)
		{
			this.m_Game = game;
			this.m_Player = player;
			this.m_bIsHumanPlayerControlled = playerControlled;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_EnemyGroups = new List<EnemyGroup>();
			this.m_ShipWeaponControls = new List<SpecWeaponControl>();
			this.m_ShipPsionicControls = new List<ShipPsionicControl>();
			this.m_Friendly = new List<Ship>();
			this.m_Enemy = new List<Ship>();
			this.m_Planets = new List<StellarBody>();
			this.m_Stars = new List<StarModel>();
			this.m_EGUpdatePhase = 0;
			this.m_Objectives = new List<TacticalObjective>();
			this.m_bPlanetsInitialized = false;
			this.m_bObjectivesInitialized = false;
			this.m_bEnemyShipsInSystem = false;
			this.m_bHasListener = false;
			this.m_IsEncounterCombat = false;
			this.m_UpdateCounter = 0;
			this.m_NodeMawUpdateRate = 0;
			this.m_FramesElapsed = 0;
			this.m_FleetID = 0;
			this.m_AIType = OverallAIType.AGGRESSIVE;
			this.m_Random = new Random();
			this.m_SpottedEnemies = new CombatZoneEnemySpotted(starSystem);
			this.m_CloakedEnemyDetection = new CloakedShipDetection();
			this._currentDiploStates = diploStates;
			this.m_SpawnProfile = null;
			this.m_OwnsSystem = starSystem.GetPlanetsInSystem().Any((StellarBody x) => x.Parameters.ColonyPlayerID == player.ID);
			this.m_UseCTAMainMenu = useCTAMainMenu;
			this.m_SystemID = starSystem.SystemID;
			this.m_EncounterPlayerIDs = CombatAI.GetAllEncounterPlayerIDs(game);
			this.InitializeNodeMaws(game, starSystem);
			this.m_SystemRadius = starSystem.GetSystemRadius();
			this.m_MinSystemRadius = starSystem.GetBaseOffset() * 5700f;
		}
		public virtual void Shutdown()
		{
			foreach (TaskGroup current in this.m_TaskGroups)
			{
				current.ShutDown();
			}
			foreach (SpecWeaponControl current2 in this.m_ShipWeaponControls)
			{
				current2.Shutdown();
			}
			foreach (ShipPsionicControl current3 in this.m_ShipPsionicControls)
			{
				current3.Shutdown();
			}
			foreach (TacticalObjective current4 in this.m_Objectives)
			{
				current4.Shutdown();
			}
			this.m_Friendly.Clear();
			this.m_Enemy.Clear();
			this.m_TaskGroups.Clear();
			this.m_EnemyGroups.Clear();
			this.m_ShipWeaponControls.Clear();
			this.m_ShipPsionicControls.Clear();
			this.m_Planets.Clear();
			this.m_Stars.Clear();
			this.m_Objectives.Clear();
			this.m_Game = null;
			this.m_Player = null;
			this.m_SpottedEnemies = null;
			this.m_CloakedEnemyDetection = null;
		}
		public virtual void ObjectRemoved(IGameObject obj)
		{
			if (obj is Ship)
			{
				Ship ship = obj as Ship;
				this.m_Friendly.Remove(ship);
				this.m_Enemy.Remove(ship);
				foreach (TaskGroup current in this.m_TaskGroups)
				{
					current.ObjectRemoved(ship);
				}
				foreach (EnemyGroup current2 in this.m_EnemyGroups)
				{
					current2.m_Ships.Remove(ship);
				}
				List<TacticalObjective> list = new List<TacticalObjective>();
				foreach (TacticalObjective current3 in this.m_Objectives)
				{
					if (current3.m_PoliceOwner == obj)
					{
						current3.m_PoliceOwner = null;
						list.Add(current3);
					}
				}
				foreach (TacticalObjective current4 in list)
				{
					this.m_Objectives.Remove(current4);
				}
				List<SpecWeaponControl> list2 = new List<SpecWeaponControl>();
				foreach (SpecWeaponControl current5 in this.m_ShipWeaponControls)
				{
					if (current5.ControlledShip == ship)
					{
						list2.Add(current5);
					}
					else
					{
						current5.ObjectRemoved(obj);
					}
				}
				foreach (SpecWeaponControl current6 in list2)
				{
					current6.Shutdown();
					this.m_ShipWeaponControls.Remove(current6);
				}
				List<ShipPsionicControl> list3 = new List<ShipPsionicControl>();
				foreach (ShipPsionicControl current7 in this.m_ShipPsionicControls)
				{
					if (current7.ControlledShip == ship)
					{
						list3.Add(current7);
					}
				}
				foreach (ShipPsionicControl current8 in list3)
				{
					current8.Shutdown();
					this.m_ShipPsionicControls.Remove(current8);
				}
			}
			if (obj is StellarBody)
			{
				this.m_Planets.Remove(obj as StellarBody);
			}
			if (obj is StarModel)
			{
				this.m_Stars.Remove(obj as StarModel);
			}
		}
		private static List<int> GetAllEncounterPlayerIDs(App game)
		{
			List<int> list = new List<int>();
			if (game.Game.ScriptModules.AsteroidMonitor != null)
			{
				list.Add(game.Game.ScriptModules.AsteroidMonitor.PlayerID);
			}
			if (game.Game.ScriptModules.MorrigiRelic != null)
			{
				list.Add(game.Game.ScriptModules.MorrigiRelic.PlayerID);
			}
			if (game.Game.ScriptModules.Gardeners != null)
			{
				list.Add(game.Game.ScriptModules.Gardeners.PlayerID);
			}
			if (game.Game.ScriptModules.Swarmers != null)
			{
				list.Add(game.Game.ScriptModules.Swarmers.PlayerID);
			}
			if (game.Game.ScriptModules.VonNeumann != null)
			{
				list.Add(game.Game.ScriptModules.VonNeumann.PlayerID);
			}
			if (game.Game.ScriptModules.Locust != null)
			{
				list.Add(game.Game.ScriptModules.Locust.PlayerID);
			}
			if (game.Game.ScriptModules.Comet != null)
			{
				list.Add(game.Game.ScriptModules.Comet.PlayerID);
			}
			if (game.Game.ScriptModules.SystemKiller != null)
			{
				list.Add(game.Game.ScriptModules.SystemKiller.PlayerID);
			}
			if (game.Game.ScriptModules.MeteorShower != null)
			{
				list.Add(game.Game.ScriptModules.MeteorShower.PlayerID);
			}
			if (game.Game.ScriptModules.Spectre != null)
			{
				list.Add(game.Game.ScriptModules.Spectre.PlayerID);
			}
			if (game.Game.ScriptModules.GhostShip != null)
			{
				list.Add(game.Game.ScriptModules.GhostShip.PlayerID);
			}
			return list;
		}
		public bool IsEncounterPlayer(int playerId)
		{
			return this.m_EncounterPlayerIDs.Contains(playerId);
		}
		public float GetCloakedDetectionPercent(Ship ship)
		{
			return this.m_CloakedEnemyDetection.GetVisibilityPercent(ship);
		}
		public bool IsShipDetected(Ship ship)
		{
			return ship != null && this.GetCloakedDetectionPercent(ship) > 0f && (ship.IsDetected(this.m_Player) || this.m_SpottedEnemies.IsShipSpotted(ship));
		}
		public bool ShipCanChangeTarget(Ship ship)
		{
			if (ship == null)
			{
				return false;
			}
			ShipPsionicControl shipPsionicControl = this.m_ShipPsionicControls.FirstOrDefault((ShipPsionicControl x) => x.ControlledShip == ship);
			return shipPsionicControl == null || shipPsionicControl.CanChangeTarget();
		}
		public virtual bool VictoryConditionsAreMet()
		{
			return false;
		}
		public virtual void Update(List<IGameObject> objs)
		{
			this.PurgeDestroyed();
			if (!App.m_bAI_Enabled)
			{
				return;
			}
			List<TaskGroup> list = new List<TaskGroup>();
			foreach (TaskGroup current in this.m_TaskGroups)
			{
				if (current.GetShipCount() == 0)
				{
					list.Add(current);
				}
			}
			foreach (TaskGroup current2 in list)
			{
				foreach (TacticalObjective current3 in this.m_Objectives)
				{
					if (current3.m_TargetTaskGroup == current2)
					{
						current3.m_TargetTaskGroup = null;
					}
				}
				this.m_TaskGroups.Remove(current2);
				current2.ShutDown();
			}
			this.m_FramesElapsed += (this._simMode ? 20 : 1);
			this.m_UpdateCounter--;
			if (this.m_UpdateCounter > 0)
			{
				return;
			}
			this.m_UpdateCounter = (this._simMode ? 2 : 5);
			List<Ship> list2 = new List<Ship>();
			List<Ship> list3 = new List<Ship>();
			List<StellarBody> list4 = new List<StellarBody>();
			List<StarModel> list5 = new List<StarModel>();
			this.m_bEnemyShipsInSystem = false;
			bool flag = false;
			foreach (IGameObject current4 in objs)
			{
				if (current4 is Ship)
				{
					Ship ship = current4 as Ship;
					if (!ship.IsDestroyed && !ship.HasRetreated && !ship.HitByNodeCannon)
					{
						if (ship.Player == this.m_Player)
						{
							flag = (flag || ship.IsListener);
							if (this.m_bIsHumanPlayerControlled && !ship.IsNPCFreighter && (!ship.IsPolice || !ship.IsPolicePatrolling))
							{
								continue;
							}
							list2.Add(ship);
							if (TaskGroup.IsValidTaskGroupShip(ship) && this.m_TaskGroups.Count > 0 && ship.TaskGroup == null)
							{
								this.AddToBestTaskGroup(ship);
							}
						}
						else
						{
							if (this.GetDiplomacyState(ship.Player.ID) == DiplomacyState.WAR || (this._inTestMode && this.m_Player != ship.Player))
							{
								bool flag2 = ship.ShipClass == ShipClass.Station || ship.IsNPCFreighter || ship.Deployed || ship.IsAcceleratorHoop || ship.IsDriveless || this.IsEncounterPlayer(ship.Player.ID) || this.m_bHasListener;
								this.m_CloakedEnemyDetection.AddShip(ship);
								if (this.m_CloakedEnemyDetection.GetVisibilityPercent(ship) > 0f)
								{
									this.m_SpottedEnemies.AddShip(ship, flag2);
								}
								else
								{
									this.m_SpottedEnemies.RemoveShip(ship);
								}
								if (this.IsShipDetected(ship) || flag2)
								{
									list3.Add(ship);
									this.m_SpottedEnemies.SetEnemySpotted(ship);
								}
								this.m_bEnemyShipsInSystem = true;
								this.m_IsEncounterCombat = (this.m_IsEncounterCombat || this.IsEncounterPlayer(ship.Player.ID));
							}
						}
						if (this.m_UseCTAMainMenu && TaskGroup.IsValidTaskGroupShip(ship) && ship.CombatStance != CombatStance.RETREAT && ship.CombatStance != CombatStance.CLOSE_TO_ATTACK)
						{
							ship.SetCombatStance(CombatStance.CLOSE_TO_ATTACK);
						}
					}
				}
				if (!this.m_bPlanetsInitialized)
				{
					if (current4 is StellarBody)
					{
						StellarBody item = current4 as StellarBody;
						list4.Add(item);
					}
					if (current4 is StarModel)
					{
						StarModel item2 = current4 as StarModel;
						list5.Add(item2);
					}
				}
			}
			this.m_Friendly = list2;
			this.m_Enemy = list3;
			if (!this.m_bPlanetsInitialized)
			{
				this.m_Planets = list4;
				this.m_Stars = list5;
				this.m_bPlanetsInitialized = true;
			}
			if (this.m_Friendly.Count > 0 || flag || this.m_bHasListener)
			{
				this.m_CloakedEnemyDetection.UpdateCloakedDetection(this.m_FramesElapsed);
				if (this.m_EnemyGroups.Count<EnemyGroup>() < 1)
				{
					this.IdentifyEnemyGroups(this.m_Enemy);
				}
				this.UpdateEnemyGroups();
				if ((flag || this.m_bHasListener) && this.m_Player == this.m_Game.LocalPlayer)
				{
					this.SyncListenerTargets();
				}
				this.m_bHasListener = flag;
			}
			if (this.m_Friendly.Count == 0)
			{
				this.m_FramesElapsed = 0;
				return;
			}
			foreach (Ship current5 in this.m_Friendly)
			{
				this.TryAddSpecialShipControl(current5);
			}
			if (this.m_TaskGroups.Count<TaskGroup>() < 1)
			{
				this.SetInitialTaskGroups(this.m_Friendly);
			}
			this.UpdateObjectives();
			this.UpdateMergeTaskGroups();
			foreach (TaskGroup current6 in this.m_TaskGroups)
			{
				this.UpdateTaskGroup(current6);
			}
			this.UpdateSpecialWeaponUpdates(this.m_FramesElapsed);
			this.UpdateShipPsionics(this.m_FramesElapsed);
			this.UpdateNodeMaws();
			this.m_FramesElapsed = 0;
		}
		private void SyncListenerTargets()
		{
			List<object> list = new List<object>();
			int count = this.m_EnemyGroups.Count;
			list.Add(InteropMessageID.IMID_ENGINE_COMBAT_SYNC_LISTENERS);
			if (this.m_bHasListener)
			{
				list.Add(count);
				using (List<EnemyGroup>.Enumerator enumerator = this.m_EnemyGroups.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						EnemyGroup current = enumerator.Current;
						int count2 = current.m_Ships.Count;
						list.Add(count2);
						foreach (Ship current2 in current.m_Ships)
						{
							list.Add(current2.ObjectID);
						}
					}
					goto IL_CD;
				}
			}
			list.Add(0);
			IL_CD:
			this.m_Game.PostEngineMessage(list.ToArray());
		}
		public void UpdateDiploStates(int playerID, DiplomacyState diploState)
		{
			if (this._currentDiploStates == null)
			{
				return;
			}
			if (this._currentDiploStates.ContainsKey(playerID))
			{
				this._currentDiploStates[playerID] = diploState;
			}
		}
		protected virtual void PurgeDestroyed()
		{
			if (this.m_Enemy == null || this.m_Friendly == null)
			{
				return;
			}
			List<Ship> list = new List<Ship>();
			foreach (Ship current in this.m_Enemy)
			{
				if (!Ship.IsActiveShip(current) || current.Player == this.m_Player)
				{
					list.Add(current);
				}
			}
			foreach (Ship current2 in list)
			{
				this.m_Enemy.Remove(current2);
				this.m_SpottedEnemies.RemoveShip(current2);
			}
			list.Clear();
			foreach (Ship current3 in this.m_Friendly)
			{
				if (!Ship.IsActiveShip(current3) || current3.Player != this.m_Player)
				{
					list.Add(current3);
				}
			}
			foreach (Ship current4 in list)
			{
				if (current4.TaskGroup != null)
				{
					current4.TaskGroup.RemoveShip(current4);
				}
				this.m_Friendly.Remove(current4);
			}
			foreach (EnemyGroup current5 in this.m_EnemyGroups)
			{
				list.Clear();
				foreach (Ship current6 in current5.m_Ships)
				{
					if (current6.Player == this.m_Player || !this.IsShipDetected(current6))
					{
						list.Add(current6);
					}
				}
				foreach (Ship current7 in list)
				{
					current5.m_Ships.Remove(current7);
				}
				current5.PurgeDestroyed();
			}
		}
		protected void UpdateSpecialWeaponUpdates(int elapsedFrames)
		{
			List<SpecWeaponControl> list = new List<SpecWeaponControl>();
			foreach (SpecWeaponControl current in this.m_ShipWeaponControls)
			{
				if (!current.RemoveWeaponControl())
				{
					current.Update(elapsedFrames);
				}
				else
				{
					list.Add(current);
				}
			}
			foreach (SpecWeaponControl current2 in list)
			{
				current2.Shutdown();
				this.m_ShipWeaponControls.Remove(current2);
			}
		}
		protected void UpdateShipPsionics(int elapsedFrames)
		{
			List<ShipPsionicControl> list = new List<ShipPsionicControl>();
			foreach (ShipPsionicControl current in this.m_ShipPsionicControls)
			{
				if (current.ControlledShip != null && current.ControlledShip.CurrentPsiPower > 0)
				{
					current.Update(elapsedFrames);
				}
				else
				{
					list.Add(current);
				}
			}
			foreach (ShipPsionicControl current2 in list)
			{
				current2.Shutdown();
				this.m_ShipPsionicControls.Remove(current2);
			}
		}
		protected virtual void UpdateObjectives()
		{
			if (!this.m_bObjectivesInitialized)
			{
				this.InitializeObjectives();
			}
			List<TacticalObjective> list = new List<TacticalObjective>();
			foreach (TacticalObjective current in this.m_Objectives)
			{
				if (current.IsComplete())
				{
					list.Add(current);
				}
				else
				{
					current.Update();
					if (current.m_RequestTaskGroup)
					{
						List<TaskGroup> list2 = (
							from x in this.m_TaskGroups
							where !(x.Objective is RetreatObjective)
							select x).ToList<TaskGroup>();
						EnemyGroup enemyGroup = current.m_TargetEnemyGroup;
						if (enemyGroup == null && current is DefendPlanetObjective)
						{
							enemyGroup = (current as DefendPlanetObjective).GetClosestThreat();
						}
						int num = current.ResourceNeeds();
						while (list2.Count > 0 && current.m_CurrentResources < num)
						{
							TaskGroup taskGroup = null;
							float num2 = 3.40282347E+38f;
							foreach (TaskGroup current2 in list2)
							{
								if (!(current2.Objective is EvadeEnemyObjective) && current2.Type != TaskGroupType.Freighter && current2.Type != TaskGroupType.UnArmed)
								{
									if (current2.Objective is AttackGroupObjective && current2.Objective.m_TargetEnemyGroup != null)
									{
										CombatAI.AssessGroupStrength(current2.Objective.m_TargetEnemyGroup.m_Ships);
										if (current2.Objective.m_TargetEnemyGroup.IsHigherPriorityThan(enemyGroup, this, true))
										{
											continue;
										}
									}
									float lengthSquared = (current2.GetBaseGroupPosition() - current.GetObjectiveLocation()).LengthSquared;
									if (lengthSquared < num2)
									{
										num2 = lengthSquared;
										taskGroup = current2;
									}
								}
							}
							if (taskGroup == null)
							{
								taskGroup = list2.FirstOrDefault((TaskGroup x) => x.Type != TaskGroupType.Freighter && x.Type != TaskGroupType.UnArmed);
								break;
							}
							taskGroup.Objective = current;
							list2.Remove(taskGroup);
						}
					}
				}
			}
			foreach (TacticalObjective current3 in list)
			{
				foreach (TaskGroup current4 in this.m_TaskGroups)
				{
					if (current4.Objective == current3)
					{
						current4.Objective = null;
					}
				}
				this.m_Objectives.Remove(current3);
			}
		}
		private void UpdateMergeTaskGroups()
		{
			if (this.m_TaskGroups.Count < 2)
			{
				return;
			}
			bool flag = false;
			while (!flag)
			{
				flag = true;
				List<Ship> list = new List<Ship>();
				foreach (TaskGroup current in this.m_TaskGroups)
				{
					if (current.GetShipCount() != 0 && (current.Objective is AttackGroupObjective || current.Objective is AttackPlanetObjective) && current.Type != TaskGroupType.Police)
					{
						Vector3 baseGroupPosition = current.GetBaseGroupPosition();
						foreach (TaskGroup current2 in this.m_TaskGroups)
						{
							if (current2 != current && current2.Objective == current.Objective && current2.GetShipCount() != 0 && (current.Objective is AttackGroupObjective || current.Objective is AttackPlanetObjective) && current2.Type != TaskGroupType.Police && (current.Type == current2.Type || (current.Type != TaskGroupType.PlanetAssault && current2.Type != TaskGroupType.PlanetAssault)) && (baseGroupPosition - current2.GetBaseGroupPosition()).LengthSquared < 9000000f)
							{
								list.AddRange(current2.GetShips());
								flag = false;
							}
						}
						if (!flag)
						{
							current.AddShips(list);
							break;
						}
					}
				}
			}
		}
		private void AddToBestTaskGroup(Ship ship)
		{
			if (this.m_TaskGroups.Count < 1)
			{
				return;
			}
			if (ship.TaskGroup != null || !TaskGroup.IsValidTaskGroupShip(ship))
			{
				return;
			}
			TaskGroup taskGroup = null;
			TaskGroupType taskGroupType = TaskGroup.GetTaskTypeFromShip(ship);
			if (this.m_IsEncounterCombat && (taskGroupType == TaskGroupType.Passive || taskGroupType == TaskGroupType.Civilian || taskGroupType == TaskGroupType.PlanetAssault))
			{
				taskGroupType = TaskGroupType.Aggressive;
			}
			float num = 3.40282347E+38f;
			foreach (TaskGroup current in this.m_TaskGroups)
			{
				if (current.Type != TaskGroupType.Police)
				{
					if (taskGroupType == TaskGroupType.Civilian)
					{
						if (current.Type == TaskGroupType.Civilian)
						{
							taskGroup = current;
							break;
						}
					}
					else
					{
						if (taskGroupType == TaskGroupType.UnArmed)
						{
							if (current.Type == TaskGroupType.UnArmed)
							{
								taskGroup = current;
								break;
							}
						}
						else
						{
							if (taskGroupType == TaskGroupType.PlanetAssault)
							{
								if (current.Type == TaskGroupType.PlanetAssault)
								{
									taskGroup = current;
									break;
								}
							}
							else
							{
								float lengthSquared = (current.GetBaseGroupPosition() - ship.Position).LengthSquared;
								if (lengthSquared < num && lengthSquared < TaskGroup.ATTACK_GROUP_RANGE * TaskGroup.ATTACK_GROUP_RANGE)
								{
									num = lengthSquared;
									taskGroup = current;
								}
							}
						}
					}
				}
			}
			if (taskGroup != null)
			{
				taskGroup.AddShip(ship);
				return;
			}
			TaskGroup taskGroup2 = new TaskGroup(this.m_Game, this);
			taskGroup2.AddShips(new List<Ship>
			{
				ship
			});
			taskGroup2.UpdateTaskGroupType();
			this.m_TaskGroups.Add(taskGroup2);
		}
		private void UpdateTaskGroup(TaskGroup group)
		{
			group.UpdateEnemyContact();
			this.UpdateTaskGroupObjective(group);
			group.Update(this.m_FramesElapsed);
		}
		private void UpdateTaskGroupObjective(TaskGroup group)
		{
			if (group.GetShipCount() == 0)
			{
				return;
			}
			ObjectiveType objectiveType = group.GetRequestedObjectiveType();
			OverallAIType aIType = this.m_AIType;
			if (aIType == OverallAIType.SLAVER && (group.Objective == null || (!(group.Objective is RetreatObjective) && !(group.Objective is AttackPlanetObjective))))
			{
				objectiveType = ObjectiveType.ATTACK_TARGET;
			}
			if (group.Objective == null || (objectiveType != ObjectiveType.NO_OBJECTIVE && objectiveType != group.Objective.m_ObjectiveType))
			{
				TacticalObjective tacticalObjective = null;
				if (group.Objective != null)
				{
					switch (objectiveType)
					{
					case ObjectiveType.PATROL:
						tacticalObjective = this.GetPatrolObjective(group);
						break;
					case ObjectiveType.SCOUT:
						tacticalObjective = this.GetScoutObjective(group);
						break;
					case ObjectiveType.DEFEND_TARGET:
						tacticalObjective = this.GetDefendObjective(group);
						break;
					case ObjectiveType.ATTACK_TARGET:
						if (this.m_AIType == OverallAIType.PIRATE)
						{
							tacticalObjective = this.GetBoardTargetObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetAttackObjective(group);
						}
						break;
					case ObjectiveType.EVADE_ENEMY:
						tacticalObjective = this.GetEvadeObjective(group);
						break;
					case ObjectiveType.RETREAT:
						tacticalObjective = this.GetRetreatObjective(group);
						break;
					}
				}
				if (tacticalObjective == null && this.m_AIType == OverallAIType.PIRATE)
				{
					if (group.Type == TaskGroupType.BoardingGroup)
					{
						tacticalObjective = this.GetBoardTargetObjective(group);
						if (tacticalObjective != null)
						{
							bool flag = false;
							foreach (TacticalObjective current in 
								from x in this.m_Objectives
								where x is FollowTaskGroupObjective
								select x)
							{
								if (current.m_TargetTaskGroup == group)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								FollowTaskGroupObjective item = new FollowTaskGroupObjective(group);
								this.m_Objectives.Add(item);
							}
						}
						else
						{
							tacticalObjective = this.GetAttackFreighterObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetAttackObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetDefendObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetRetreatObjective(group);
						}
					}
					else
					{
						if (group.Type == TaskGroupType.FollowGroup)
						{
							tacticalObjective = this.GetFollowBoardingGroupObjective(group);
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetAttackObjective(group);
							}
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetAttackFreighterObjective(group);
							}
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetDefendObjective(group);
							}
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetRetreatObjective(group);
							}
						}
					}
				}
				if (tacticalObjective == null)
				{
					switch (group.Type)
					{
					case TaskGroupType.Aggressive:
					case TaskGroupType.Passive:
						if (this.m_AIType == OverallAIType.PIRATE)
						{
							tacticalObjective = this.GetAttackFreighterObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetAttackObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetScoutObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetDefendObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetPatrolObjective(group);
						}
						break;
					case TaskGroupType.Civilian:
						if ((
							from x in this.m_TaskGroups
							where x.Type == TaskGroupType.Aggressive || x.Type == TaskGroupType.Passive
							select x).Count<TaskGroup>() == 0)
						{
							tacticalObjective = this.GetAttackObjective(group);
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetScoutObjective(group);
							}
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetDefendObjective(group);
							}
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetPatrolObjective(group);
							}
						}
						else
						{
							tacticalObjective = this.GetEvadeObjective(group);
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetDefendObjective(group);
							}
							if (tacticalObjective == null)
							{
								tacticalObjective = this.GetPatrolObjective(group);
							}
						}
						break;
					case TaskGroupType.Police:
						tacticalObjective = this.GetAttackObjective(group);
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetPatrolObjective(group);
						}
						break;
					case TaskGroupType.Freighter:
					{
						TacticalObjective patrolObjective = this.GetPatrolObjective(group);
						TacticalObjective retreatObjective = this.GetRetreatObjective(group);
						Vector3 baseGroupPosition = group.GetBaseGroupPosition();
						if (patrolObjective == null || (retreatObjective.GetObjectiveLocation() - baseGroupPosition).LengthSquared < (patrolObjective.GetObjectiveLocation() - baseGroupPosition).LengthSquared)
						{
							tacticalObjective = retreatObjective;
						}
						else
						{
							tacticalObjective = patrolObjective;
						}
						break;
					}
					case TaskGroupType.UnArmed:
						if ((
							from x in this.m_TaskGroups
							where x.Type == TaskGroupType.Aggressive || x.Type == TaskGroupType.Passive || x.Type == TaskGroupType.Civilian
							select x).Count<TaskGroup>() == 0)
						{
							tacticalObjective = this.GetRetreatObjective(group);
						}
						else
						{
							tacticalObjective = this.GetEvadeObjective(group);
						}
						break;
					case TaskGroupType.PlanetAssault:
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetAttackObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetDefendObjective(group);
						}
						if (tacticalObjective == null)
						{
							tacticalObjective = this.GetPatrolObjective(group);
						}
						break;
					}
				}
				group.Objective = tacticalObjective;
			}
		}
		protected TacticalObjective GetPatrolObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			int num = 20;
			float num2 = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is PatrolObjective
				select x)
			{
				if (tg.Type == TaskGroupType.Police)
				{
					foreach (Ship current2 in tg.GetShips())
					{
						if (current.m_PoliceOwner == current2)
						{
							tacticalObjective = current;
							break;
						}
					}
					if (tacticalObjective != null)
					{
						break;
					}
				}
				else
				{
					if (tg.Type == TaskGroupType.Freighter)
					{
						if (current.m_Planet != null)
						{
							float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
							if (lengthSquared < num2)
							{
								tacticalObjective = current;
								num2 = lengthSquared;
							}
						}
					}
					else
					{
						float lengthSquared2 = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
						if (current.m_TaskGroups.Count < num || lengthSquared2 < num2)
						{
							tacticalObjective = current;
							num2 = lengthSquared2;
							num = current.m_TaskGroups.Count;
						}
					}
				}
			}
			return tacticalObjective;
		}
		protected TacticalObjective GetScoutObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			int num = 20;
			float num2 = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is ScoutObjective
				select x)
			{
				float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
				if (current.m_TaskGroups.Count < num || lengthSquared < num2)
				{
					result = current;
					num2 = lengthSquared;
					num = current.m_TaskGroups.Count;
				}
			}
			return result;
		}
		protected TacticalObjective GetAttackObjective(TaskGroup tg)
		{
			TacticalObjective tacticalObjective;
			if (tg.Type == TaskGroupType.PlanetAssault)
			{
				tacticalObjective = this.GetAttackPlanetObjective(tg);
				if (tacticalObjective == null)
				{
					tacticalObjective = this.GetAttackEnemyGroupObjective(tg);
				}
			}
			else
			{
				TacticalObjective attackEnemyGroupObjective = this.GetAttackEnemyGroupObjective(tg);
				TacticalObjective attackPlanetObjective = this.GetAttackPlanetObjective(tg);
				float num = 0f;
				if (attackEnemyGroupObjective == null || this.m_AIType == OverallAIType.SLAVER || num > tg.GroupSpeed + 5f)
				{
					tacticalObjective = attackPlanetObjective;
				}
				else
				{
					tacticalObjective = attackEnemyGroupObjective;
				}
			}
			return tacticalObjective;
		}
		protected TacticalObjective GetAttackEnemyGroupObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			if (tg.IsInContactWithEnemy)
			{
				using (IEnumerator<TacticalObjective> enumerator = (
					from x in this.m_Objectives
					where x is AttackGroupObjective
					select x).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						TacticalObjective current = enumerator.Current;
						if (current.m_TargetEnemyGroup == tg.EnemyGroupInContact)
						{
							result = current;
							break;
						}
					}
					return result;
				}
			}
			if (this.m_IsEncounterCombat)
			{
				float num = 3.40282347E+38f;
				Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
				List<TacticalObjective> list = (
					from x in this.m_Objectives
					where x is AttackGroupObjective && x.m_TargetEnemyGroup != null && x.m_TargetEnemyGroup.IsEncounterEnemyGroup(this)
					select x).ToList<TacticalObjective>();
				foreach (TacticalObjective current2 in list)
				{
					float lengthSquared = (baseGroupPosition - current2.GetObjectiveLocation()).LengthSquared;
					if (lengthSquared < num)
					{
						result = current2;
						num = lengthSquared;
					}
				}
			}
			return result;
		}
		protected TacticalObjective GetAttackPlanetObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is AttackPlanetObjective
				select x)
			{
				float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
				if (lengthSquared < num)
				{
					result = current;
					num = lengthSquared;
				}
			}
			return result;
		}
		protected TacticalObjective GetAttackFreighterObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is AttackGroupObjective
				select x)
			{
				if (current.m_TargetEnemyGroup != null && current.m_TargetEnemyGroup.IsFreighterEnemyGroup())
				{
					float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
					if (lengthSquared < num)
					{
						result = current;
						num = lengthSquared;
					}
				}
			}
			return result;
		}
		protected TacticalObjective GetBoardTargetObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is BoardTargetObjective
				select x)
			{
				if (current.m_TargetEnemyGroup != null && current.m_TargetEnemyGroup.IsFreighterEnemyGroup())
				{
					float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
					if (lengthSquared < num)
					{
						result = current;
						num = lengthSquared;
					}
				}
			}
			return result;
		}
		protected TacticalObjective GetFollowBoardingGroupObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is FollowTaskGroupObjective
				select x)
			{
				float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
				if (lengthSquared < num)
				{
					result = current;
					num = lengthSquared;
				}
			}
			return result;
		}
		protected TacticalObjective GetDefendObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is DefendPlanetObjective
				select x)
			{
				float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
				if (lengthSquared < num)
				{
					result = current;
					num = lengthSquared;
				}
			}
			return result;
		}
		protected TacticalObjective GetEvadeObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is EvadeEnemyObjective
				select x)
			{
				if (!(current as EvadeEnemyObjective).IsUnsafe)
				{
					float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
					if (lengthSquared < num)
					{
						result = current;
						num = lengthSquared;
					}
				}
			}
			return result;
		}
		protected TacticalObjective GetRetreatObjective(TaskGroup tg)
		{
			TacticalObjective result = null;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			float num = 3.40282347E+38f;
			foreach (TacticalObjective current in 
				from x in this.m_Objectives
				where x is RetreatObjective
				select x)
			{
				float lengthSquared = (baseGroupPosition - current.GetObjectiveLocation()).LengthSquared;
				if (lengthSquared < num)
				{
					result = current;
					num = lengthSquared;
				}
			}
			return result;
		}
		public static int AssessGroupStrength(List<Ship> ships)
		{
			int num = 0;
			foreach (Ship current in ships)
			{
				num += CombatAI.GetShipStrength(current);
			}
			return num;
		}
		public int GetTargetShipScore(Ship ship)
		{
			int num = CombatAI.GetShipStrength(ship) * 3;
			int targetShipBonusScore = this.GetTargetShipBonusScore(ship);
			return num + targetShipBonusScore;
		}
		public Ship GetBestShipTarget(Vector3 pos, List<Ship> ships)
		{
			if (ships.Count == 0)
			{
				return null;
			}
			ShipTargetComparision comparer = new ShipTargetComparision(this, pos);
			ships.Sort(comparer);
			return ships.First<Ship>();
		}
		public static int GetShipStrength(Ship ship)
		{
			int num = 0;
			if (!TaskGroup.IsValidTaskGroupShip(ship))
			{
				return num;
			}
			return num + CombatAI.GetShipStrength(ship.ShipClass);
		}
		public static int GetShipStrength(ShipClass shipClass)
		{
			int num = 0;
			switch (shipClass)
			{
			case ShipClass.Cruiser:
				num += 3;
				break;
			case ShipClass.Dreadnought:
				num += 9;
				break;
			case ShipClass.Leviathan:
				num += 27;
				break;
			default:
				num++;
				break;
			}
			return num;
		}
		private int GetTargetShipBonusScore(Ship ship)
		{
			int num = 0;
			if (!TaskGroup.IsValidTaskGroupShip(ship))
			{
				return num;
			}
			TaskGroupType taskTypeFromShip = TaskGroup.GetTaskTypeFromShip(ship);
			if (this.m_AIType == OverallAIType.PIRATE)
			{
				if (taskTypeFromShip == TaskGroupType.Freighter)
				{
					return 50;
				}
				if (taskTypeFromShip == TaskGroupType.Police)
				{
					return 25;
				}
			}
			if (ship.ShipRole == ShipRole.COMMAND)
			{
				num += 3;
			}
			if (ship.IsCarrier)
			{
				num += 3;
			}
			if (this.m_OwnsSystem && taskTypeFromShip == TaskGroupType.PlanetAssault)
			{
				num += 6;
			}
			return num;
		}
		private void SetInitialTaskGroups(List<Ship> ships)
		{
			List<Ship> list = new List<Ship>();
			foreach (Ship current in ships)
			{
				if (current.TaskGroup == null && TaskGroup.IsValidTaskGroupShip(current))
				{
					list.Add(current);
				}
			}
			switch (this.m_AIType)
			{
			case OverallAIType.SLAVER:
				this.CreateSlaverTaskGroup(list);
				return;
			case OverallAIType.PIRATE:
				this.CreatePirateTaskGroup(list);
				return;
			default:
				this.CreateNormalTaskGroups(list);
				return;
			}
		}
		private void CreateNormalTaskGroups(List<Ship> ships)
		{
			if (ships.Count == 0)
			{
				return;
			}
			ShipInfo shipInfo = this.m_Game.GameDatabase.GetShipInfo(ships.First<Ship>().DatabaseID, false);
			int fleetID = (shipInfo != null) ? shipInfo.FleetID : 0;
			List<Ship> list = new List<Ship>();
			List<Ship> list2 = new List<Ship>();
			List<Ship> list3 = new List<Ship>();
			List<Ship> list4 = new List<Ship>();
			List<Ship> list5 = new List<Ship>();
			List<Ship> list6 = new List<Ship>();
			List<Ship> list7 = new List<Ship>();
			foreach (Ship current in ships)
			{
				if (current.IsPolice)
				{
					list5.Add(current);
				}
				else
				{
					switch (TaskGroup.GetTaskTypeFromShip(current))
					{
					case TaskGroupType.Passive:
						list2.Add(current);
						continue;
					case TaskGroupType.Civilian:
						list3.Add(current);
						continue;
					case TaskGroupType.Freighter:
						list6.Add(current);
						continue;
					case TaskGroupType.UnArmed:
						list4.Add(current);
						continue;
					case TaskGroupType.PlanetAssault:
						list7.Add(current);
						continue;
					}
					list.Add(current);
				}
			}
			MissionInfo missionByFleetID = this.m_Game.GameDatabase.GetMissionByFleetID(fleetID);
			if (this.m_IsEncounterCombat)
			{
				this.AssignShipsToTaskGroupsForEncounterAttack(list, list2, list3, list7);
			}
			else
			{
				if ((missionByFleetID != null && (missionByFleetID.Type == MissionType.INVASION || missionByFleetID.Type == MissionType.STRIKE)) || this.m_Planets.Count + this.m_Stars.Count == 0)
				{
					this.AssignShipsToTaskGroupsForAttack(list, list2, list3);
				}
				else
				{
					bool flag = false;
					bool flag2 = false;
					foreach (ColonyInfo current2 in this.m_Game.GameDatabase.GetColonyInfosForSystem(0))
					{
						if (current2.PlayerID == this.m_Player.ID)
						{
							flag = true;
						}
						else
						{
							if (this.GetDiplomacyState(current2.PlayerID) == DiplomacyState.WAR)
							{
								flag2 = true;
							}
						}
					}
					if (flag && flag2)
					{
						this.AssignShipsToTaskGroupsForMultiMission(list, list2, list3);
					}
					else
					{
						if (flag)
						{
							this.AssignShipsToTaskGroupsForDefending(list, list2, list3);
						}
						else
						{
							this.AssignShipsToTaskGroupsForAttack(list, list2, list3);
						}
					}
				}
			}
			if (!this.m_IsEncounterCombat)
			{
				this.CreatePlanetAssaultTaskGroup(list7);
			}
			this.CreateUnarmedTaskGroup(list4);
			this.CreateFreighterTaskGroups(list6);
			this.AssignShipsToPoliceTaskGroups(list5);
		}
		private void AddNearbyShipsToTaskGroup(TaskGroup group, Ship ship, List<Ship> ungroupedList)
		{
			if (group.Type == TaskGroupType.Police)
			{
				return;
			}
			bool flag = true;
			while (flag)
			{
				flag = false;
				foreach (Ship current in ungroupedList)
				{
					float length = (ship.Maneuvering.Position - current.Maneuvering.Position).Length;
					if (length < 2000f)
					{
						group.AddShip(current);
						ungroupedList.Remove(current);
						flag = true;
						break;
					}
				}
			}
		}
		private void AssignShipsToTaskGroupsForEncounterAttack(List<Ship> aggressive, List<Ship> passive, List<Ship> civilian, List<Ship> planetAssault)
		{
			if (aggressive.Count + passive.Count + civilian.Count > 0)
			{
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShips(aggressive);
				taskGroup.AddShips(passive);
				taskGroup.AddShips(civilian);
				taskGroup.AddShips(planetAssault);
				taskGroup.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup);
			}
		}
		private void AssignShipsToTaskGroupsForAttack(List<Ship> aggressive, List<Ship> passive, List<Ship> civilian)
		{
			bool flag = false;
			List<Ship> list = new List<Ship>();
			list.AddRange(aggressive);
			if (list.Count < TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
			{
				list.AddRange(passive);
				flag = true;
			}
			if (list.Count >= TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
			{
				int count = list.Count;
				int num = count / TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + 1;
				if (count % TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP < TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
				{
					num--;
				}
				int num2 = count / num;
				for (int i = 0; i < num; i++)
				{
					Ship ship = list[0];
					List<Ship> list2 = new List<Ship>();
					list2.Add(ship);
					aggressive.Remove(ship);
					for (int j = 0; j < num2; j++)
					{
						float num3 = 3.40282347E+38f;
						Ship ship2 = null;
						foreach (Ship current in list)
						{
							float lengthSquared = (ship.Position - current.Position).LengthSquared;
							if (lengthSquared < num3)
							{
								ship2 = current;
								num3 = lengthSquared;
							}
						}
						if (ship2 == null)
						{
							break;
						}
						list2.Add(ship2);
						list.Remove(ship2);
					}
					TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
					taskGroup.AddShips(list2);
					this.m_TaskGroups.Add(taskGroup);
				}
			}
			if (this.m_TaskGroups.Count == 0)
			{
				this.m_TaskGroups.Add(new TaskGroup(this.m_Game, this));
			}
			while (list.Count > 0)
			{
				this.m_TaskGroups[this.m_TaskGroups.Count - 1].AddShip(list[0]);
				list.RemoveAt(0);
			}
			int num4 = 0;
			int count2 = this.m_TaskGroups.Count;
			List<Ship> list3 = new List<Ship>();
			if (!flag)
			{
				list3.AddRange(passive);
			}
			foreach (Ship current2 in list3)
			{
				this.m_TaskGroups[num4].AddShip(current2);
				num4 = (num4 + 1) % count2;
			}
			if (civilian.Count > 0)
			{
				if (aggressive.Count + passive.Count > 0)
				{
					TaskGroup taskGroup2 = new TaskGroup(this.m_Game, this);
					taskGroup2.AddShips(civilian);
					this.m_TaskGroups.Add(taskGroup2);
				}
				else
				{
					this.m_TaskGroups[0].AddShips(civilian);
				}
			}
			foreach (TaskGroup current3 in this.m_TaskGroups)
			{
				current3.UpdateTaskGroupType();
				if (current3.Type == TaskGroupType.Aggressive)
				{
					current3.m_Orders = TaskGroupOrders.Scout;
				}
				else
				{
					current3.m_Orders = TaskGroupOrders.Patrol;
				}
			}
		}
		private void AssignShipsToTaskGroupsForDefending(List<Ship> aggressive, List<Ship> passive, List<Ship> civilian)
		{
			List<Ship> list = new List<Ship>();
			list.AddRange(aggressive);
			int num = list.Count / TaskGroup.NUM_SHIPS_PER_SCOUT;
			if (num < 1)
			{
				list.AddRange(passive);
			}
			num = list.Count / TaskGroup.NUM_SHIPS_PER_SCOUT;
			if (num < 1)
			{
				if (list.Count > 0)
				{
					TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
					taskGroup.AddShips(list);
					this.m_TaskGroups.Add(taskGroup);
					taskGroup = new TaskGroup(this.m_Game, this);
					taskGroup.AddShips(civilian);
					this.m_TaskGroups.Add(taskGroup);
				}
				else
				{
					if (civilian.Count > 0)
					{
						TaskGroup taskGroup2 = new TaskGroup(this.m_Game, this);
						taskGroup2.AddShips(civilian);
						this.m_TaskGroups.Add(taskGroup2);
					}
				}
			}
			else
			{
				for (int i = 0; i < num; i++)
				{
					Ship ship = CombatAI.FindBestScout(list);
					if (ship == null)
					{
						break;
					}
					TaskGroup taskGroup3 = new TaskGroup(this.m_Game, this);
					taskGroup3.AddShip(ship);
					taskGroup3.UpdateTaskGroupType();
					taskGroup3.m_Orders = TaskGroupOrders.Scout;
					this.m_TaskGroups.Add(taskGroup3);
					list.Remove(ship);
				}
				if (list.Count >= TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
				{
					int count = list.Count;
					int num2 = count / TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP + 1;
					if (count % TaskGroup.DESIRED_MIN_AGGRESSORS_PER_GROUP < TaskGroup.ABS_MIN_AGGRESSORS_PER_GROUP)
					{
						num2--;
					}
					int num3 = count / num2;
					for (int j = 0; j < num2; j++)
					{
						Ship ship2 = list[0];
						List<Ship> list2 = new List<Ship>();
						list2.Add(ship2);
						aggressive.Remove(ship2);
						for (int k = 0; k < num3; k++)
						{
							float num4 = 3.40282347E+38f;
							Ship ship3 = null;
							foreach (Ship current in list)
							{
								float lengthSquared = (ship2.Position - current.Position).LengthSquared;
								if (lengthSquared < num4)
								{
									ship3 = current;
									num4 = lengthSquared;
								}
							}
							if (ship3 == null)
							{
								break;
							}
							list2.Add(ship3);
							list.Remove(ship3);
						}
						TaskGroup taskGroup4 = new TaskGroup(this.m_Game, this);
						taskGroup4.AddShips(list2);
						this.m_TaskGroups.Add(taskGroup4);
					}
					while (list.Count > 0)
					{
						this.m_TaskGroups[this.m_TaskGroups.Count - 1].AddShip(list[0]);
						list.RemoveAt(0);
					}
					if (civilian.Count > 0)
					{
						TaskGroup taskGroup5 = new TaskGroup(this.m_Game, this);
						taskGroup5.AddShips(civilian);
						this.m_TaskGroups.Add(taskGroup5);
					}
				}
			}
			foreach (TaskGroup current2 in this.m_TaskGroups)
			{
				current2.UpdateTaskGroupType();
			}
		}
		public static Ship FindBestScout(List<Ship> ships)
		{
			Ship ship = ships.FirstOrDefault((Ship x) => x.ShipRole == ShipRole.SCOUT && !Ship.IsBattleRiderSize(x.RealShipClass));
			if (ship == null)
			{
				List<Ship> list = (
					from x in ships
					where x.ShipRole == ShipRole.COMMAND
					select x).ToList<Ship>();
				float num = 0f;
				foreach (Ship current in ships)
				{
					if (!list.Contains(current) && !current.IsDriveless && current.Maneuvering.MaxShipSpeed > num)
					{
						ship = current;
					}
				}
				if (ship == null)
				{
					foreach (Ship current2 in list)
					{
						if (!current2.IsDriveless && current2.Maneuvering.MaxShipSpeed > num)
						{
							ship = current2;
						}
					}
				}
			}
			if (ship == null)
			{
				ship = ships.FirstOrDefault<Ship>();
			}
			return ship;
		}
		private void AssignShipsToTaskGroupsForMultiMission(List<Ship> aggressive, List<Ship> passive, List<Ship> civilian)
		{
		}
		private void AssignShipsToPoliceTaskGroups(List<Ship> police)
		{
			foreach (Ship current in police)
			{
				bool flag = false;
				foreach (TacticalObjective current2 in 
					from x in this.m_Objectives
					where x.m_PoliceOwner != null
					select x)
				{
					if (current2.m_PoliceOwner == current)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					PatrolObjective item = new PatrolObjective(current, current.Position, this.m_Game.AssetDatabase.PolicePatrolRadius, this.m_Game.AssetDatabase.PolicePatrolRadius);
					this.m_Objectives.Add(item);
				}
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShip(current);
				taskGroup.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup);
			}
		}
		private void CreateSlaverTaskGroup(List<Ship> ships)
		{
			if (ships.Count > 0)
			{
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShips(ships);
				taskGroup.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup);
			}
		}
		private void CreatePirateTaskGroup(List<Ship> ships)
		{
			if (ships.Count > 0)
			{
				bool flag = false;
				if (!this.m_Game.GameDatabase.GetPirateBaseInfos().Any((PirateBaseInfo x) => x.SystemId == this.m_SystemID))
				{
					foreach (Ship current in ships)
					{
						if (current.WeaponBanks.Any((WeaponBank x) => x.TurretClass == WeaponEnums.TurretClasses.BoardingPod))
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
					taskGroup.AddShips(ships);
					taskGroup.Type = TaskGroupType.BoardingGroup;
					this.m_TaskGroups.Add(taskGroup);
					return;
				}
				TaskGroup taskGroup2 = new TaskGroup(this.m_Game, this);
				taskGroup2.AddShips(ships);
				taskGroup2.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup2);
			}
		}
		private void CreateUnarmedTaskGroup(List<Ship> ships)
		{
			if (ships.Count > 0)
			{
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShips(ships);
				taskGroup.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup);
			}
		}
		private void CreatePlanetAssaultTaskGroup(List<Ship> ships)
		{
			if (ships.Count > 0)
			{
				TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
				taskGroup.AddShips(ships);
				taskGroup.UpdateTaskGroupType();
				this.m_TaskGroups.Add(taskGroup);
			}
		}
		private void CreateFreighterTaskGroups(List<Ship> freighters)
		{
			if (freighters.Count > 0)
			{
				foreach (Ship current in freighters)
				{
					TaskGroup taskGroup = new TaskGroup(this.m_Game, this);
					taskGroup.AddShip(current);
					taskGroup.UpdateTaskGroupType();
					this.m_TaskGroups.Add(taskGroup);
				}
			}
		}
		private void InitializeObjectives()
		{
			this.m_bObjectivesInitialized = true;
			this.CreateEnemyObjectives();
			this.CreatePatrolObjectives();
			this.CreateScoutObjectives();
			this.CreatePlanetObjectives();
			this.CreateEvadeObjectives();
			this.CreateRetreatObjectives();
		}
		private void CreateEnemyObjectives()
		{
			foreach (EnemyGroup current in this.m_EnemyGroups)
			{
				if (current.IsFreighterEnemyGroup())
				{
					TacticalObjective item = new BoardTargetObjective(current);
					this.m_Objectives.Add(item);
				}
				TacticalObjective item2 = new AttackGroupObjective(current);
				this.m_Objectives.Add(item2);
			}
		}
		private void CreatePatrolObjectives()
		{
			if (this.m_Planets.Count == 0 && this.m_Stars.Count == 0)
			{
				Vector3 dest = Vector3.Zero;
				if (!this.m_Game.GameDatabase.GetNeutronStarInfos().Any((NeutronStarInfo x) => x.DeepSpaceSystemId.HasValue && x.DeepSpaceSystemId.Value == this.m_SystemID))
				{
					if (!this.m_Game.GameDatabase.GetGardenerInfos().Any((GardenerInfo x) => x.DeepSpaceSystemId.HasValue && x.DeepSpaceSystemId.Value == this.m_SystemID))
					{
						goto IL_11F;
					}
				}
				Vector3 v = default(Vector3);
				v.X = (this.m_Random.CoinToss(0.5) ? -1f : 1f) * this.m_Random.NextInclusive(1E-05f, 1f);
				v.Z = (this.m_Random.CoinToss(0.5) ? -1f : 1f) * this.m_Random.NextInclusive(1E-05f, 1f);
				v.Normalize();
				dest = v * 20000f;
				IL_11F:
				TacticalObjective item = new PatrolObjective(dest, 0f, 50000f);
				this.m_Objectives.Add(item);
				return;
			}
			foreach (StellarBody current in this.m_Planets)
			{
				ColonyInfo colonyInfoForPlanet = this.m_Game.GameDatabase.GetColonyInfoForPlanet(current.Parameters.OrbitalID);
				if (colonyInfoForPlanet == null || (colonyInfoForPlanet.PlayerID != this.m_Player.ID && this.GetDiplomacyState(colonyInfoForPlanet.PlayerID) != DiplomacyState.WAR))
				{
					float num = this.ObtainMinPatrolDistFromTarget(current);
					if (num > 0f)
					{
						TacticalObjective tacticalObjective = new PatrolObjective(current.Parameters.Position, num, this.ObtainMaxPatrolDistFromTarget(current, num));
						tacticalObjective.m_Planet = current;
						this.m_Objectives.Add(tacticalObjective);
					}
				}
			}
			foreach (StarModel current2 in this.m_Stars)
			{
				float num2 = this.ObtainMinPatrolDistFromTarget(current2);
				if (num2 > 0f)
				{
					TacticalObjective item2 = new PatrolObjective(current2.Position, num2, this.ObtainMaxPatrolDistFromTarget(current2, num2));
					this.m_Objectives.Add(item2);
				}
			}
		}
		private float ObtainMinPatrolDistFromTarget(IGameObject target)
		{
			float num = 1500f;
			Vector3 v = Vector3.Zero;
			if (target is StellarBody)
			{
				StellarBody stellarBody = target as StellarBody;
				v = stellarBody.Parameters.Position;
				num = stellarBody.Parameters.Radius + 750f + 1500f;
			}
			else
			{
				if (!(target is StarModel))
				{
					return num;
				}
				StarModel starModel = target as StarModel;
				v = starModel.Position;
				num = starModel.Radius + 7500f + 2500f;
			}
			foreach (StellarBody current in this.m_Planets)
			{
				if (current != target)
				{
					float num2 = num + current.Parameters.Radius + 1500f;
					float lengthSquared = (current.Parameters.Position - v).LengthSquared;
					if (lengthSquared < num2 * num2)
					{
						float val = (float)Math.Sqrt((double)lengthSquared) + 1500f + current.Parameters.Radius;
						num = Math.Max(num, val);
					}
				}
			}
			foreach (StarModel current2 in this.m_Stars)
			{
				if (current2 != target)
				{
					float num3 = num + 2500f;
					float lengthSquared2 = (current2.Position - v).LengthSquared;
					if (lengthSquared2 < num3 * num3)
					{
						return 0f;
					}
				}
			}
			return num;
		}
		private float ObtainMaxPatrolDistFromTarget(IGameObject target, float minPatrolDist)
		{
			float result = minPatrolDist + 10000f;
			Vector3 vector = Vector3.Zero;
			if (target is StellarBody)
			{
				StellarBody stellarBody = target as StellarBody;
				vector = stellarBody.Parameters.Position;
			}
			else
			{
				if (!(target is StarModel))
				{
					return result;
				}
				StarModel starModel = target as StarModel;
				vector = starModel.Position;
			}
			bool flag = false;
			float num = 3.40282347E+38f;
			float s = 0f;
			Vector3 vector2 = Vector3.Zero;
			foreach (StellarBody current in this.m_Planets)
			{
				if (current != target)
				{
					float num2 = minPatrolDist + current.Parameters.Radius + 1500f;
					float lengthSquared = (current.Parameters.Position - vector).LengthSquared;
					if (lengthSquared >= num2 * num2 && lengthSquared < num)
					{
						num = lengthSquared;
						vector2 = current.Parameters.Position;
						s = current.Parameters.Radius + 1500f;
						flag = true;
					}
				}
			}
			foreach (StarModel current2 in this.m_Stars)
			{
				if (current2 != target)
				{
					float num3 = minPatrolDist + 2500f;
					float lengthSquared2 = (current2.Position - vector).LengthSquared;
					if (lengthSquared2 >= num3 * num3 && lengthSquared2 < num)
					{
						num = lengthSquared2;
						vector2 = current2.Position;
						s = 2500f;
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return 1000000f;
			}
			Vector3 v = vector - vector2;
			v.Normalize();
			Vector3 v2 = vector2 + v * s;
			return (vector - v2).Length;
		}
		private void CreateScoutObjectives()
		{
			foreach (StellarBody current in this.m_Planets)
			{
				ColonyInfo colonyInfoForPlanet = this.m_Game.GameDatabase.GetColonyInfoForPlanet(current.Parameters.OrbitalID);
				if (colonyInfoForPlanet != null && this.GetDiplomacyState(colonyInfoForPlanet.PlayerID) == DiplomacyState.WAR)
				{
					TacticalObjective item = new ScoutObjective(current, this.m_Player);
					this.m_Objectives.Add(item);
				}
			}
			foreach (EnemyGroup current2 in this.m_EnemyGroups)
			{
				bool flag = false;
				using (IEnumerator<TacticalObjective> enumerator3 = (
					from x in this.m_Objectives
					where x is ScoutObjective
					select x).GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						ScoutObjective scoutObjective = (ScoutObjective)enumerator3.Current;
						if ((scoutObjective.m_Destination - current2.m_LastKnownPosition).LengthSquared < 2.5E+09f)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					TacticalObjective item2 = new ScoutObjective(current2, this.m_Player);
					this.m_Objectives.Add(item2);
				}
			}
		}
		private void CreatePlanetObjectives()
		{
			if (this.m_Game == null || this.m_Game.GameDatabase == null)
			{
				return;
			}
			GameState currentState = this.m_Game.CurrentState;
			if (currentState == null || !(currentState is CommonCombatState))
			{
				return;
			}
			List<int> list = new List<int>();
			foreach (Ship current in this.m_Friendly)
			{
				ShipInfo shipInfo = this.m_Game.GameDatabase.GetShipInfo(current.DatabaseID, false);
				if (shipInfo != null && !list.Contains(shipInfo.FleetID))
				{
					list.Add(shipInfo.FleetID);
				}
			}
			PirateBaseInfo pirateBaseInfo = this.m_Game.GameDatabase.GetPirateBaseInfos().FirstOrDefault((PirateBaseInfo x) => x.SystemId == this.m_SystemID);
			bool flag = this.m_AIType == OverallAIType.PIRATE && pirateBaseInfo != null;
			OrbitalObjectInfo orbitalObjectInfo = null;
			if (flag)
			{
				StationInfo stationInfo = this.m_Game.GameDatabase.GetStationInfo(pirateBaseInfo.BaseStationId);
				if (stationInfo != null)
				{
					orbitalObjectInfo = this.m_Game.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
				}
			}
			foreach (StellarBody planet in this.m_Planets)
			{
				if (flag && orbitalObjectInfo != null && orbitalObjectInfo.ParentID == planet.Parameters.OrbitalID)
				{
					float num = this.ObtainMinPatrolDistFromTarget(planet);
					PatrolObjective patrolObjective = null;
					if (num > 0f)
					{
						patrolObjective = new PatrolObjective(planet.Parameters.Position, num, this.ObtainMaxPatrolDistFromTarget(planet, num));
						patrolObjective.m_Planet = planet;
						this.m_Objectives.Add(patrolObjective);
					}
					TacticalObjective item = new DefendPlanetObjective(planet, this, patrolObjective);
					this.m_Objectives.Add(item);
				}
				else
				{
					ColonyInfo colonyInfoForPlanet = this.m_Game.GameDatabase.GetColonyInfoForPlanet(planet.Parameters.OrbitalID);
					if (colonyInfoForPlanet != null)
					{
						if (colonyInfoForPlanet.PlayerID == this.m_Player.ID)
						{
							float num2 = this.ObtainMinPatrolDistFromTarget(planet);
							PatrolObjective patrolObjective2 = null;
							if (num2 > 0f)
							{
								patrolObjective2 = new PatrolObjective(planet.Parameters.Position, num2, this.ObtainMaxPatrolDistFromTarget(planet, num2));
								patrolObjective2.m_Planet = planet;
								this.m_Objectives.Add(patrolObjective2);
							}
							TacticalObjective item2 = new DefendPlanetObjective(planet, this, patrolObjective2);
							this.m_Objectives.Add(item2);
						}
						else
						{
							if (this.GetDiplomacyState(colonyInfoForPlanet.PlayerID) == DiplomacyState.WAR)
							{
								TacticalObjective item3 = new AttackPlanetObjective(planet);
								this.m_Objectives.Add(item3);
							}
							else
							{
								foreach (int current2 in list)
								{
									MissionInfo missionByFleetID = this.m_Game.GameDatabase.GetMissionByFleetID(current2);
									if (missionByFleetID != null && (missionByFleetID.Type == MissionType.INVASION || missionByFleetID.Type == MissionType.STRIKE) && missionByFleetID.TargetOrbitalObjectID == planet.Parameters.OrbitalID)
									{
										if (!this.m_Objectives.Any((TacticalObjective x) => x.m_ObjectiveType == ObjectiveType.ATTACK_TARGET && x.m_Planet == planet))
										{
											TacticalObjective item4 = new AttackPlanetObjective(planet);
											this.m_Objectives.Add(item4);
											break;
										}
										break;
									}
								}
							}
						}
					}
				}
			}
		}
		private void CreateEvadeObjectives()
		{
			List<TacticalObjective> list = (
				from x in this.m_Objectives
				where x is PatrolObjective || x is DefendPlanetObjective
				select x).ToList<TacticalObjective>();
			foreach (TacticalObjective current in list)
			{
				if (current is PatrolObjective)
				{
					float sensorRange = this.m_Game.AssetDatabase.DefaultPlanetSensorRange * 0.5f;
					if (current.m_Planet != null)
					{
						sensorRange = current.m_Planet.Parameters.Radius + 750f + 1500f;
					}
					EvadeEnemyObjective item = new EvadeEnemyObjective(this, current as PatrolObjective, sensorRange);
					this.m_Objectives.Add(item);
				}
				if (current is DefendPlanetObjective && (current as DefendPlanetObjective).DefendPatrolObjective != null)
				{
					EvadeEnemyObjective item2 = new EvadeEnemyObjective(this, (current as DefendPlanetObjective).DefendPatrolObjective, current.m_Planet.Parameters.Radius + 750f + 1500f);
					this.m_Objectives.Add(item2);
				}
			}
		}
		private void CreateRetreatObjectives()
		{
			if (this.m_SpottedEnemies != null)
			{
				List<Vector3> allEntryPoints = this.m_SpottedEnemies.GetAllEntryPoints();
				foreach (Vector3 current in allEntryPoints)
				{
					RetreatObjective item = new RetreatObjective(current);
					this.m_Objectives.Add(item);
				}
			}
			if (!this.m_Objectives.Any((TacticalObjective x) => x is RetreatObjective))
			{
				if (this.m_SpawnProfile != null)
				{
					RetreatObjective item2 = new RetreatObjective(this.m_SpawnProfile._retreatPosition);
					this.m_Objectives.Add(item2);
					return;
				}
				RetreatObjective item3 = new RetreatObjective(Vector3.UnitZ * 100000f);
				this.m_Objectives.Add(item3);
			}
		}
		private void IdentifyEnemyGroups(List<Ship> enemyShips)
		{
			this.m_EnemyGroups.Clear();
			List<Ship> list = new List<Ship>();
			List<Ship> list2 = new List<Ship>();
			using (List<Ship>.Enumerator enumerator = enemyShips.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Ship current = enumerator.Current;
					if (current.ShipClass != ShipClass.BattleRider)
					{
						if (current.IsNPCFreighter)
						{
							list2.Add(current);
						}
						else
						{
							list.Add(current);
						}
					}
				}
				goto IL_1B0;
			}
			IL_66:
			EnemyGroup enemyGroup = new EnemyGroup();
			if (enemyGroup.m_Ships.Count<Ship>() < 1)
			{
				Ship item = list[0];
				enemyGroup.m_Ships.Add(item);
				list.Remove(item);
			}
			if (!enemyGroup.IsFreighterEnemyGroup())
			{
				if (list.Count<Ship>() > 0)
				{
					bool flag = true;
					while (flag)
					{
						flag = false;
						foreach (Ship current2 in enemyGroup.m_Ships)
						{
							foreach (Ship current3 in list)
							{
								if (!current3.IsNPCFreighter)
								{
									float length = (current2.Maneuvering.Position - current3.Maneuvering.Position).Length;
									if (length < 2000f)
									{
										enemyGroup.m_Ships.Add(current3);
										list.Remove(current3);
										flag = true;
										break;
									}
								}
							}
							if (flag)
							{
								break;
							}
						}
					}
				}
				if (enemyGroup.m_Ships.Count<Ship>() > 0)
				{
					enemyGroup.m_LastKnownPosition = CombatAI.FindCentreOfMass(enemyGroup.m_Ships);
					enemyGroup.m_LastKnownDestination = enemyGroup.m_LastKnownPosition;
				}
				this.AddEnemyGroup(enemyGroup);
			}
			IL_1B0:
			if (list.Count<Ship>() <= 0)
			{
				return;
			}
			goto IL_66;
		}
		private void AddEnemyGroup(EnemyGroup eg)
		{
			this.m_EnemyGroups.Add(eg);
		}
		private void UpdateEnemyGroups()
		{
			List<EnemyGroup> list = new List<EnemyGroup>();
			foreach (EnemyGroup current in this.m_EnemyGroups)
			{
				if (current.m_Ships.Count<Ship>() < 1)
				{
					list.Add(current);
				}
			}
			foreach (EnemyGroup current2 in list)
			{
				foreach (TacticalObjective current3 in this.m_Objectives)
				{
					if (current3.m_TargetEnemyGroup == current2)
					{
						current3.m_TargetEnemyGroup = null;
					}
				}
				foreach (TaskGroup current4 in this.m_TaskGroups)
				{
					if (current4.EnemyGroupInContact == current2)
					{
						current4.ClearEnemyGroupInContact();
					}
				}
				this.m_EnemyGroups.Remove(current2);
			}
			if (this.m_EGUpdatePhase == 0)
			{
				List<Ship> list2 = new List<Ship>();
				List<Ship> list3 = new List<Ship>();
				foreach (Ship current5 in this.m_Enemy)
				{
					if (this.IsShipDetected(current5) && current5.ShipClass != ShipClass.BattleRider)
					{
						if (current5.IsNPCFreighter)
						{
							list3.Add(current5);
						}
						else
						{
							list2.Add(current5);
						}
					}
				}
				foreach (EnemyGroup current6 in this.m_EnemyGroups)
				{
					if (!current6.IsFreighterEnemyGroup())
					{
						foreach (Ship current7 in current6.m_Ships)
						{
							if (list2.Contains(current7))
							{
								list2.Remove(current7);
							}
						}
					}
				}
				foreach (Ship current8 in list2)
				{
					bool flag = false;
					foreach (EnemyGroup current9 in this.m_EnemyGroups)
					{
						flag = this.TryAddShipToEnemyGroup(current8, current9, 2000f);
					}
					if (!flag)
					{
						EnemyGroup enemyGroup = new EnemyGroup();
						enemyGroup.m_Ships.Add(current8);
						if (enemyGroup.m_Ships.Count<Ship>() > 0)
						{
							enemyGroup.m_LastKnownPosition = CombatAI.FindCentreOfMass(enemyGroup.m_Ships);
							enemyGroup.m_LastKnownDestination = enemyGroup.m_LastKnownPosition;
						}
						this.AddEnemyGroup(enemyGroup);
					}
				}
				foreach (Ship current10 in list3)
				{
					bool flag2 = false;
					foreach (EnemyGroup current11 in this.m_EnemyGroups)
					{
						if (current11.m_Ships.Contains(current10))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						EnemyGroup enemyGroup2 = new EnemyGroup();
						enemyGroup2.m_Ships.Add(current10);
						if (enemyGroup2.m_Ships.Count<Ship>() > 0)
						{
							enemyGroup2.m_LastKnownPosition = CombatAI.FindCentreOfMass(enemyGroup2.m_Ships);
							enemyGroup2.m_LastKnownDestination = enemyGroup2.m_LastKnownPosition;
						}
						this.AddEnemyGroup(enemyGroup2);
					}
				}
				using (List<EnemyGroup>.Enumerator enumerator12 = this.m_EnemyGroups.GetEnumerator())
				{
					while (enumerator12.MoveNext())
					{
						EnemyGroup current12 = enumerator12.Current;
						bool flag3 = false;
						foreach (TacticalObjective current13 in this.m_Objectives)
						{
							if (current13.m_TargetEnemyGroup == current12)
							{
								flag3 = true;
								break;
							}
						}
						if (!flag3)
						{
							if (current12.IsFreighterEnemyGroup())
							{
								TacticalObjective item = new BoardTargetObjective(current12);
								this.m_Objectives.Add(item);
							}
							TacticalObjective item2 = new AttackGroupObjective(current12);
							this.m_Objectives.Add(item2);
						}
					}
					goto IL_9A2;
				}
			}
			if (this.m_EGUpdatePhase == 1)
			{
				for (int i = 0; i < this.m_EnemyGroups.Count<EnemyGroup>() - 1; i++)
				{
					bool flag4 = false;
					EnemyGroup enemyGroup3 = this.m_EnemyGroups[i];
					for (int j = i + 1; j < this.m_EnemyGroups.Count<EnemyGroup>(); j++)
					{
						EnemyGroup enemyGroup4 = this.m_EnemyGroups[j];
						List<Ship> list4 = new List<Ship>();
						foreach (Ship current14 in enemyGroup4.m_Ships)
						{
							if (this.TryAddShipToEnemyGroup(current14, enemyGroup3, 2000f))
							{
								list4.Add(current14);
								flag4 = true;
							}
						}
						if (flag4)
						{
							Vector3 averageVelocity = enemyGroup3.GetAverageVelocity(this);
							Vector3 averageVelocity2 = enemyGroup4.GetAverageVelocity(this);
							if (averageVelocity.LengthSquared > 0f && averageVelocity2.Length > 0f)
							{
								averageVelocity.Normalize();
								averageVelocity2.Normalize();
								if (Vector3.Dot(averageVelocity, averageVelocity2) < 0.7f)
								{
									flag4 = false;
								}
							}
						}
						foreach (Ship current15 in list4)
						{
							enemyGroup4.m_Ships.Remove(current15);
						}
						if (flag4)
						{
							list4.Clear();
							using (List<Ship>.Enumerator enumerator5 = enemyGroup4.m_Ships.GetEnumerator())
							{
								if (enumerator5.MoveNext())
								{
									Ship current16 = enumerator5.Current;
									list4.Add(current16);
									enemyGroup3.m_Ships.Add(current16);
								}
							}
							foreach (Ship current17 in list4)
							{
								enemyGroup4.m_Ships.Remove(current17);
							}
							if (enemyGroup4.m_Ships.Count<Ship>() < 1)
							{
								this.m_EnemyGroups.Remove(enemyGroup4);
							}
						}
					}
					if (flag4)
					{
						break;
					}
				}
			}
			else
			{
				if (this.m_EGUpdatePhase == 2)
				{
					using (List<EnemyGroup>.Enumerator enumerator = this.m_EnemyGroups.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							EnemyGroup current18 = enumerator.Current;
							EnemyGroup enemyGroup5 = new EnemyGroup();
							bool flag5 = false;
							for (int k = 0; k < current18.m_Ships.Count<Ship>(); k++)
							{
								Ship ship = current18.m_Ships[k];
								if (enemyGroup5.m_Ships.Count<Ship>() < 1)
								{
									enemyGroup5.m_Ships.Add(ship);
								}
								else
								{
									if (!enemyGroup5.m_Ships.Contains(ship) && this.TryAddShipToEnemyGroup(ship, enemyGroup5, 2500f))
									{
										k = 0;
									}
								}
							}
							if (enemyGroup5.m_Ships.Count<Ship>() > 0 && enemyGroup5.m_Ships.Count<Ship>() < current18.m_Ships.Count<Ship>())
							{
								foreach (Ship current19 in enemyGroup5.m_Ships)
								{
									current18.m_Ships.Remove(current19);
								}
								this.AddEnemyGroup(enemyGroup5);
								flag5 = true;
							}
							if (flag5)
							{
								break;
							}
						}
						goto IL_9A2;
					}
				}
				if (this.m_EGUpdatePhase == 3)
				{
					for (int l = 0; l < this.m_EnemyGroups.Count<EnemyGroup>(); l++)
					{
						Vector3 vector = Vector3.Zero;
						Vector3 vector2 = Vector3.Zero;
						int num = 0;
						foreach (Ship current20 in this.m_EnemyGroups[l].m_Ships)
						{
							if (this._inTestMode || this.IsShipDetected(current20))
							{
								vector += current20.Maneuvering.Position;
								vector2 += current20.Maneuvering.Destination;
								num++;
							}
						}
						if (num < 1)
						{
							this.m_EnemyGroups.Remove(this.m_EnemyGroups[l]);
						}
						else
						{
							if (num > 0)
							{
								this.m_EnemyGroups[l].m_LastKnownPosition = vector / (float)num;
								this.m_EnemyGroups[l].m_LastKnownPosition.Y = 0f;
								this.m_EnemyGroups[l].m_LastKnownDestination = vector2 / (float)num;
								this.m_EnemyGroups[l].m_LastKnownDestination.Y = 0f;
								this.m_EnemyGroups[l].GetAverageVelocity(this);
							}
							else
							{
								if (this.m_EnemyGroups[l].m_Ships.Count > 0)
								{
									this.m_EnemyGroups[l].m_LastKnownPosition = CombatAI.FindCentreOfMass(this.m_EnemyGroups[l].m_Ships);
									this.m_EnemyGroups[l].m_LastKnownPosition.Y = 0f;
									this.m_EnemyGroups[l].m_LastKnownDestination = this.m_EnemyGroups[l].m_LastKnownPosition;
								}
							}
						}
					}
				}
				else
				{
					int arg_9A1_0 = this.m_EGUpdatePhase;
				}
			}
			IL_9A2:
			this.m_EGUpdatePhase = (this.m_EGUpdatePhase + 1) % 5;
		}
		private void TryAddSpecialShipControl(Ship ship)
		{
			if (ship.WeaponControlsIsInitilized)
			{
				return;
			}
			ship.InitializeWeaponControls();
			if (ship.IsSystemDefenceBoat && !ship.DefenseBoatActive)
			{
				SDBInfo sdbInfo = this.m_Game.GameDatabase.GetSDBInfoFromShip(ship.DatabaseID);
				StellarBody planet = null;
				if (sdbInfo != null)
				{
					planet = this.m_Planets.FirstOrDefault((StellarBody x) => x.Parameters.OrbitalID == sdbInfo.OrbitalId);
				}
				SystemDefenseBoatControl item = new SystemDefenseBoatControl(this.m_Game, this, ship, planet);
				this.m_ShipWeaponControls.Add(item);
			}
			if (ship.IsWraithAbductor)
			{
				WraithAbductorAssaultControl item2 = new WraithAbductorAssaultControl(this.m_Game, this, ship);
				this.m_ShipWeaponControls.Add(item2);
			}
			else
			{
				if (ship.IsCarrier)
				{
					if (ship.BattleRiderMounts.Any((BattleRiderMount x) => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.Biomissile))
					{
						BioMissileLaunchControl bioMissileLaunchControl = new BioMissileLaunchControl(this.m_Game, this, ship);
						bioMissileLaunchControl.AddRiders((
							from x in this.m_Friendly
							where x.IsBattleRider && x.RealShipClass == RealShipClasses.Biomissile && (x.ParentDatabaseID == ship.DatabaseID || x.ParentID == ship.ObjectID)
							select x).ToList<Ship>());
						this.m_ShipWeaponControls.Add(bioMissileLaunchControl);
					}
					if (ship.BattleRiderMounts.Any((BattleRiderMount x) => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.Drone))
					{
						DroneLaunchControl droneLaunchControl = new DroneLaunchControl(this.m_Game, this, ship);
						droneLaunchControl.AddRiders((
							from x in this.m_Friendly
							where x.IsBattleRider && x.RealShipClass == RealShipClasses.Drone && (x.ParentDatabaseID == ship.DatabaseID || x.ParentID == ship.ObjectID)
							select x).ToList<Ship>());
						this.m_ShipWeaponControls.Add(droneLaunchControl);
					}
					if (ship.BattleRiderMounts.Any((BattleRiderMount x) => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.AssaultShuttle))
					{
						AssaultShuttleLaunchControl assaultShuttleLaunchControl = new AssaultShuttleLaunchControl(this.m_Game, this, ship);
						assaultShuttleLaunchControl.AddRiders((
							from x in this.m_Friendly
							where x.IsBattleRider && x.RealShipClass == RealShipClasses.AssaultShuttle && (x.ParentDatabaseID == ship.DatabaseID || x.ParentID == ship.ObjectID)
							select x).ToList<Ship>());
						this.m_ShipWeaponControls.Add(assaultShuttleLaunchControl);
					}
					if (ship.BattleRiderMounts.Any((BattleRiderMount x) => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.BoardingPod))
					{
						BoardingPodLaunchControl boardingPodLaunchControl = new BoardingPodLaunchControl(this.m_Game, this, ship);
						boardingPodLaunchControl.AddRiders((
							from x in this.m_Friendly
							where x.IsBattleRider && x.RealShipClass == RealShipClasses.BoardingPod && (x.ParentDatabaseID == ship.DatabaseID || x.ParentID == ship.ObjectID)
							select x).ToList<Ship>());
						this.m_ShipWeaponControls.Add(boardingPodLaunchControl);
					}
					if (ship.BattleRiderMounts.Any((BattleRiderMount x) => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.DestroyerRider))
					{
						AttackRiderLaunchControl attackRiderLaunchControl = new AttackRiderLaunchControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.DestroyerRider);
						attackRiderLaunchControl.AddRiders((
							from x in this.m_Friendly
							where x.IsBattleRider && x.RealShipClass == RealShipClasses.BattleRider && (x.ParentDatabaseID == ship.DatabaseID || x.ParentID == ship.ObjectID)
							select x).ToList<Ship>());
						this.m_ShipWeaponControls.Add(attackRiderLaunchControl);
					}
					if (ship.BattleRiderMounts.Any((BattleRiderMount x) => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.CruiserRider))
					{
						LargeRiderLaunchControl largeRiderLaunchControl = new LargeRiderLaunchControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.CruiserRider);
						largeRiderLaunchControl.AddRiders((
							from x in this.m_Friendly
							where x.IsBattleRider && x.ShipClass == ShipClass.Cruiser && (x.ParentDatabaseID == ship.DatabaseID || x.ParentID == ship.ObjectID)
							select x).ToList<Ship>());
						this.m_ShipWeaponControls.Add(largeRiderLaunchControl);
					}
					if (ship.BattleRiderMounts.Any((BattleRiderMount x) => x.WeaponBank.TurretClass == WeaponEnums.TurretClasses.DreadnoughtRider))
					{
						LargeRiderLaunchControl largeRiderLaunchControl2 = new LargeRiderLaunchControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.DreadnoughtRider);
						largeRiderLaunchControl2.AddRiders((
							from x in this.m_Friendly
							where x.IsBattleRider && x.ShipClass == ShipClass.Dreadnought && (x.ParentDatabaseID == ship.DatabaseID || x.ParentID == ship.ObjectID)
							select x).ToList<Ship>());
						this.m_ShipWeaponControls.Add(largeRiderLaunchControl2);
					}
				}
			}
			if (ship.WeaponBanks.Any((WeaponBank x) => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.COL))
			{
				WeaponBank weaponBank = ship.WeaponBanks.First((WeaponBank x) => x.TurretClass == WeaponEnums.TurretClasses.COL);
				PositionalAttackControl item3 = new PositionalAttackControl(this.m_Game, this, ship, weaponBank.Weapon.UniqueWeaponID, WeaponEnums.TurretClasses.COL);
				this.m_ShipWeaponControls.Add(item3);
			}
			if (ship.IsAcceleratorHoop)
			{
				if (ship.WeaponBanks.Any((WeaponBank x) => x.Weapon != null && x.Weapon.PayloadType == WeaponEnums.PayloadTypes.MegaBeam))
				{
					WeaponBank weaponBank2 = ship.WeaponBanks.First((WeaponBank x) => x.Weapon.PayloadType == WeaponEnums.PayloadTypes.MegaBeam);
					TachyonCannonAttackControl item4 = new TachyonCannonAttackControl(this.m_Game, this, ship, weaponBank2.Weapon.UniqueWeaponID, WeaponEnums.TurretClasses.FreeBeam);
					this.m_ShipWeaponControls.Add(item4);
				}
			}
			foreach (WeaponBank wb in ship.WeaponBanks)
			{
				if (wb.Weapon.Traits.Contains(WeaponEnums.WeaponTraits.Detonating))
				{
					if (!this.m_ShipWeaponControls.OfType<PositionalAttackControl>().Any((PositionalAttackControl x) => x.ControlledShip == ship && x.WeaponID == wb.Weapon.UniqueWeaponID))
					{
						PositionalAttackControl item5 = new PositionalAttackControl(this.m_Game, this, ship, wb.Weapon.UniqueWeaponID, wb.TurretClass);
						this.m_ShipWeaponControls.Add(item5);
					}
				}
			}
			if (ship.WeaponBanks.Any((WeaponBank x) => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.NodeCannon))
			{
				WeaponBank weaponBank3 = ship.WeaponBanks.First((WeaponBank x) => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.NodeCannon);
				NodeCannonAttackControl item6 = new NodeCannonAttackControl(this.m_Game, this, ship, weaponBank3.Weapon.UniqueWeaponID, WeaponEnums.TurretClasses.NodeCannon);
				this.m_ShipWeaponControls.Add(item6);
			}
			if (ship.WeaponBanks.Any((WeaponBank x) => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.Minelayer))
			{
				MineLayerControl item7 = new MineLayerControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.Minelayer);
				this.m_ShipWeaponControls.Add(item7);
			}
			if (ship.WeaponBanks.Any((WeaponBank x) => x.LogicalBank.TurretClass == WeaponEnums.TurretClasses.Siege))
			{
				AttackPlanetControl item8 = new AttackPlanetControl(this.m_Game, this, ship, WeaponEnums.TurretClasses.Siege);
				this.m_ShipWeaponControls.Add(item8);
			}
			if (ship.CurrentPsiPower > 0 && ship.Psionics.Count<Psionic>() > 0)
			{
				ShipPsionicControl item9 = new ShipPsionicControl(this.m_Game, this, ship);
				this.m_ShipPsionicControls.Add(item9);
			}
		}
		private bool TryAddShipToEnemyGroup(Ship ship, EnemyGroup eGroup, float range = 2000f)
		{
			if (eGroup.IsFreighterEnemyGroup())
			{
				return false;
			}
			foreach (Ship current in eGroup.m_Ships)
			{
				float lengthSquared = (ship.Maneuvering.Position - current.Maneuvering.Position).LengthSquared;
				if (lengthSquared < range * range)
				{
					eGroup.m_Ships.Add(ship);
					return true;
				}
			}
			return false;
		}
		private Ship GetShipByID(int id, bool friendly)
		{
			if (friendly)
			{
				using (List<Ship>.Enumerator enumerator = this.m_Friendly.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Ship current = enumerator.Current;
						if (current.ObjectID == id)
						{
							Ship result = current;
							return result;
						}
					}
					goto IL_7C;
				}
			}
			foreach (Ship current2 in this.m_Friendly)
			{
				if (current2.ObjectID == id)
				{
					Ship result = current2;
					return result;
				}
			}
			IL_7C:
			return null;
		}
		public static float GetMaxWeaponRangeFromShips(List<Ship> ships)
		{
			float num = 0f;
			foreach (Ship current in ships)
			{
				List<WeaponBank> list = current.WeaponBanks.ToList<WeaponBank>();
				if (list.Count > 0)
				{
					num = Math.Max(num, list.Max((WeaponBank x) => x.Weapon.Range));
				}
			}
			return num;
		}
		public static float GetMinWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0f;
			bool flag = false;
			foreach (WeaponBank current in ship.WeaponBanks)
			{
				if (usePD || !current.Weapon.IsPDWeapon())
				{
					float range = current.Weapon.Range;
					if (range > 0f)
					{
						if (!flag)
						{
							num = range;
							flag = true;
						}
						else
						{
							if (range < num)
							{
								num = range;
							}
						}
					}
				}
			}
			if (flag)
			{
				return num;
			}
			return 500f;
		}
		public static float GetMinEffectiveWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0f;
			bool flag = false;
			foreach (WeaponBank current in ship.WeaponBanks)
			{
				if (usePD || !current.Weapon.IsPDWeapon())
				{
					float range = current.Weapon.RangeTable.Effective.Range;
					if (range > 0f)
					{
						if (!flag)
						{
							num = range;
							flag = true;
						}
						else
						{
							if (range < num)
							{
								num = range;
							}
						}
					}
				}
			}
			if (flag)
			{
				return num;
			}
			return 500f;
		}
		public static float GetMaxEffectiveWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0f;
			foreach (WeaponBank current in ship.WeaponBanks)
			{
				if (usePD || !current.Weapon.IsPDWeapon())
				{
					num = Math.Max(current.Weapon.RangeTable.Effective.Range, num);
				}
			}
			if (num > 0f)
			{
				return num;
			}
			return 500f;
		}
		public static float GetMinPointBlankWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0f;
			bool flag = false;
			foreach (WeaponBank current in ship.WeaponBanks)
			{
				if (usePD || !current.Weapon.IsPDWeapon())
				{
					float range = current.Weapon.RangeTable.PointBlank.Range;
					if (range > 0f)
					{
						if (!flag)
						{
							num = range;
							flag = true;
						}
						else
						{
							if (range < num)
							{
								num = range;
							}
						}
					}
				}
			}
			if (flag)
			{
				return num;
			}
			return 500f;
		}
		public static float GetAveEffectiveWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0f;
			int num2 = 0;
			foreach (WeaponBank current in ship.WeaponBanks)
			{
				if (usePD || !current.Weapon.IsPDWeapon())
				{
					float range = current.Weapon.RangeTable.Effective.Range;
					if (range > 0f)
					{
						num += range;
						num2++;
					}
				}
			}
			if (num2 > 0)
			{
				return num / (float)num2;
			}
			return 500f;
		}
		public static float GetMaxWeaponRange(Ship ship, bool usePD = false)
		{
			float num = 0f;
			bool flag = false;
			foreach (WeaponBank current in ship.WeaponBanks)
			{
				if (usePD || !current.Weapon.IsPDWeapon())
				{
					float range = current.Weapon.RangeTable.Maximum.Range;
					if (range > 0f)
					{
						if (!flag)
						{
							num = range;
							flag = true;
						}
						else
						{
							if (range > num)
							{
								num = range;
							}
						}
					}
				}
			}
			if (flag)
			{
				return num;
			}
			return 500f;
		}
		public static bool IsTargetInRange(Ship ship, IGameObject target)
		{
			if (target == null)
			{
				return false;
			}
			float num = 0f;
			if (target is Ship)
			{
				Ship ship2 = target as Ship;
				num = (ship.Maneuvering.Position - ship2.Maneuvering.Position).Length + ship.ShipSphere.radius;
			}
			else
			{
				if (target is StellarBody)
				{
					StellarBody stellarBody = target as StellarBody;
					num = (ship.Maneuvering.Position - stellarBody.Parameters.Position).Length - stellarBody.Parameters.Radius;
				}
			}
			float num2 = CombatAI.GetMaxWeaponRange(ship, false);
			num2 *= 1.3f;
			return num < num2;
		}
		public static Vector3 FindCentreOfMass(List<Ship> ships)
		{
			Vector3 result = new Vector3(0f, 0f, 0f);
			float num = 0f;
			float num2 = 0f;
			int num3 = 0;
			foreach (Ship current in ships)
			{
				num += current.Maneuvering.Position.X;
				num2 += current.Maneuvering.Position.Z;
				num3++;
			}
			if (num3 < 1)
			{
				return result;
			}
			result.X = num / (float)num3;
			result.Z = num2 / (float)num3;
			return result;
		}
		private TaskGroup GetTaskGroupForShipList(List<int> shipIDs)
		{
			List<TaskGroup> list = new List<TaskGroup>();
			List<int> list2 = new List<int>();
			foreach (int current in shipIDs)
			{
				Ship shipByID = this.GetShipByID(current, true);
				if (shipByID != null && shipByID.TaskGroup != null && !list.Contains(shipByID.TaskGroup))
				{
					list.Add(shipByID.TaskGroup);
					list2.Add(0);
				}
			}
			if (list.Count<TaskGroup>() == 1)
			{
				return list[0];
			}
			if (list.Count<TaskGroup>() > 1)
			{
				int num = 0;
				int index = -1;
				for (int i = 0; i < list.Count<TaskGroup>(); i++)
				{
					foreach (int current2 in shipIDs)
					{
						Ship shipByID2 = this.GetShipByID(current2, true);
						if (shipByID2 != null && shipByID2.TaskGroup == list[i])
						{
							List<int> list3;
							int index2;
							(list3 = list2)[index2 = i] = list3[index2] + 1;
							if (list2[i] > num)
							{
								num = list2[i];
								index = i;
							}
						}
					}
				}
				return list[index];
			}
			return new TaskGroup(this.m_Game, this);
		}
		public void AddTaskGroup(TaskGroup nuGroup)
		{
			if (!this.m_TaskGroups.Contains(nuGroup))
			{
				this.m_TaskGroups.Add(nuGroup);
			}
		}
		public bool IsFriendOrAlly(int playerID)
		{
			if (this._currentDiploStates == null)
			{
				return playerID == this.m_Player.ID;
			}
			if (playerID == this.m_Player.ID)
			{
				return true;
			}
			DiplomacyState diplomacyState = this.GetDiplomacyState(playerID);
			return diplomacyState == DiplomacyState.PEACE || diplomacyState == DiplomacyState.ALLIED;
		}
		public void SetDiplomacyState(int playerID, DiplomacyState state)
		{
			if (this._currentDiploStates == null)
			{
				return;
			}
			if (this._currentDiploStates.ContainsKey(playerID))
			{
				this._currentDiploStates[playerID] = state;
			}
		}
		public virtual DiplomacyState GetDiplomacyState(int playerID)
		{
			if (this._currentDiploStates != null)
			{
				DiplomacyState result;
				if (!this._currentDiploStates.TryGetValue(playerID, out result))
				{
					result = DiplomacyState.NEUTRAL;
				}
				return result;
			}
			if (playerID != this.m_Player.ID)
			{
				return DiplomacyState.WAR;
			}
			return DiplomacyState.PEACE;
		}
		public StellarBody GetPlanetContainingPosition(Vector3 position)
		{
			StellarBody result = null;
			foreach (StellarBody current in this.m_Planets)
			{
				float lengthSquared = (current.Parameters.Position - position).LengthSquared;
				float num = current.Parameters.Radius + 750f + 500f;
				if (lengthSquared < num * num)
				{
					result = current;
					break;
				}
			}
			return result;
		}
		public StellarBody GetClosestEnemyPlanet(Vector3 position, float range)
		{
			StellarBody result = null;
			float num = 3.40282347E+38f;
			foreach (StellarBody current in this.m_Planets)
			{
				if (current.Population != 0.0 && this.GetDiplomacyState(current.Parameters.ColonyPlayerID) == DiplomacyState.WAR)
				{
					float lengthSquared = (current.Parameters.Position - position).LengthSquared;
					float num2 = current.Parameters.Radius + 750f + 500f + range;
					if (lengthSquared < num && lengthSquared < num2 * num2)
					{
						num = lengthSquared;
						result = current;
					}
				}
			}
			return result;
		}
		public Vector3 GetSafeDestination(Vector3 currentPos, Vector3 dest)
		{
			Vector3 vector = dest;
			bool flag = false;
			foreach (StellarBody current in this.m_Planets)
			{
				float lengthSquared = (current.Parameters.Position - vector).LengthSquared;
				float num = current.Parameters.Radius + 750f + 500f;
				if (lengthSquared < num * num)
				{
					Vector3 v = dest - current.Parameters.Position;
					v.Y = 0f;
					if (v.LengthSquared > 0f)
					{
						v.Normalize();
					}
					else
					{
						v = currentPos - dest;
						v.Y = 0f;
						v.Normalize();
					}
					vector = current.Parameters.Position + v * num;
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				foreach (StarModel current2 in this.m_Stars)
				{
					float lengthSquared2 = (current2.Position - vector).LengthSquared;
					float num2 = current2.Radius + 7500f + 500f;
					if (lengthSquared2 < num2 * num2)
					{
						Vector3 v2 = dest - current2.Position;
						v2.Y = 0f;
						v2.Normalize();
						vector = current2.Position + v2 * num2;
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
			}
			return vector;
		}
		public Vector3 PickNewDest(Vector3 desiredDest, Vector3 from, float radius, float minRightAngle, float minLeftAngle)
		{
			Vector3 result = Vector3.Zero;
			Vector3 vector = from - desiredDest;
			vector.Y = 0f;
			vector.Normalize();
			if (minRightAngle < 0.01f && minLeftAngle < 0.01f)
			{
				result = desiredDest + vector * radius;
			}
			else
			{
				if (Math.Abs(minRightAngle) < Math.Abs(minLeftAngle))
				{
					Vector3 v = vector;
					v.X = (float)Math.Cos((double)minRightAngle) * vector.X - (float)Math.Sin((double)minRightAngle) * vector.Z;
					v.Z = (float)Math.Sin((double)minRightAngle) * vector.X + (float)Math.Cos((double)minRightAngle) * vector.Z;
					result = desiredDest + v * radius;
				}
				else
				{
					Vector3 v2 = vector;
					v2.X = (float)Math.Cos((double)minLeftAngle) * vector.X - (float)Math.Sin((double)minLeftAngle) * vector.Z;
					v2.Z = (float)Math.Sin((double)minLeftAngle) * vector.X + (float)Math.Cos((double)minLeftAngle) * vector.Z;
					result = desiredDest + v2 * radius;
				}
			}
			return result;
		}
		private void FaceTarget(Ship ship, Ship target)
		{
			Vector3 look = target.Maneuvering.Position - ship.Maneuvering.Position;
			look.Y = 0f;
			look.Normalize();
			ship.Maneuvering.PostAddGoal(ship.Maneuvering.Position, look);
		}
		private void TurnBroadsideToTarget(Ship ship, Ship target)
		{
			Matrix matrix = Matrix.CreateRotationYPR(ship.Maneuvering.Rotation);
			Vector3 vector = target.Maneuvering.Position - ship.Maneuvering.Position;
			vector.Y = 0f;
			vector.Normalize();
			Vector3 vector2 = Vector3.Cross(vector, Vector3.UnitY);
			if (Vector3.Dot(vector2, matrix.Forward) < 0f)
			{
				vector = Vector3.Cross(Vector3.UnitY, vector);
			}
			else
			{
				vector = vector2;
			}
			ship.Maneuvering.PostAddGoal(ship.Maneuvering.Position, vector);
		}
		public List<Ship> GetFriendlyShips()
		{
			return this.m_Friendly;
		}
		public IEnumerable<EnemyGroup> GetEnemyGroups()
		{
			return this.m_EnemyGroups;
		}
		public IEnumerable<TaskGroup> GetTaskGroups()
		{
			return this.m_TaskGroups;
		}
		public void FlagAttackingShip(Ship attackingShip)
		{
			this.m_SpottedEnemies.AddShip(attackingShip, true);
			this.m_SpottedEnemies.SetEnemySpotted(attackingShip);
		}
		public void NotifyCombatZoneChanged(CombatZonePositionInfo zone)
		{
			if (zone == null || zone.Player == 0 || this.GetDiplomacyState(zone.Player) != DiplomacyState.WAR)
			{
				return;
			}
			this.m_SpottedEnemies.UpdateSpottedShipsInZone(zone);
			List<Ship> detectedShips = this.m_SpottedEnemies.GetDetectedShips();
			bool flag = false;
			foreach (Ship current in detectedShips)
			{
				if (!this.m_Enemy.Contains(current))
				{
					flag = true;
					this.m_Enemy.Add(current);
				}
				this.m_CloakedEnemyDetection.ShipSpotted(current);
			}
			if (flag)
			{
				if (this.m_EnemyGroups.Count == 0)
				{
					this.IdentifyEnemyGroups(this.m_Enemy);
				}
				else
				{
					this.m_EGUpdatePhase = 0;
					this.UpdateEnemyGroups();
				}
			}
			List<EnemyGroup> list = new List<EnemyGroup>();
			foreach (EnemyGroup current2 in this.m_EnemyGroups)
			{
				foreach (Ship current3 in detectedShips)
				{
					if (!current3.IsDetected(this.m_Player) && current2.m_Ships.Contains(current3))
					{
						list.Add(current2);
						break;
					}
				}
			}
			List<ScoutObjective> list2 = this.m_Objectives.OfType<ScoutObjective>().ToList<ScoutObjective>();
			foreach (ScoutObjective current4 in list2)
			{
				if (current4.m_TargetEnemyGroup != null && list.Contains(current4.m_TargetEnemyGroup))
				{
					list.Remove(current4.m_TargetEnemyGroup);
				}
			}
			foreach (EnemyGroup current5 in list)
			{
				ScoutObjective item = new ScoutObjective(current5, this.m_Player);
				this.m_Objectives.Add(item);
			}
			if (list.Count > 0)
			{
				TaskGroup taskGroup = null;
				if (this.m_TaskGroups.Count == 1)
				{
					this.m_TaskGroups.ElementAt(0).Objective = null;
				}
				if (taskGroup == null)
				{
					foreach (TaskGroup current6 in this.m_TaskGroups)
					{
						if (current6.Type != TaskGroupType.Civilian && (current6.Objective is PatrolObjective || current6.Objective is DefendPlanetObjective))
						{
							taskGroup = current6;
							break;
						}
					}
				}
				if (taskGroup != null)
				{
					taskGroup.Objective = null;
				}
			}
		}
		private void InitializeNodeMaws(App game, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			this.m_NodeMaws = new List<NodeMawInfo>();
			if (starSystem == null)
			{
				return;
			}
			List<Vector3> nodeMawLocationsForPlayer = starSystem.GetNodeMawLocationsForPlayer(game, this.m_Player.ID);
			if (nodeMawLocationsForPlayer.Count == 0)
			{
				return;
			}
			LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == "NodeMaw");
			if (logicalWeapon == null)
			{
				return;
			}
			foreach (Vector3 current in nodeMawLocationsForPlayer)
			{
				logicalWeapon.AddGameObjectReference();
				this.m_NodeMaws.Add(new NodeMawInfo
				{
					Weapon = logicalWeapon,
					Range = logicalWeapon.RangeTable.Maximum.Range,
					Pos = current
				});
			}
			this.m_NodeMawUpdateRate = 120;
		}
		private void UpdateNodeMaws()
		{
			if (this.m_NodeMaws.Count == 0)
			{
				return;
			}
			this.m_NodeMawUpdateRate -= this.m_FramesElapsed;
			if (this.m_NodeMawUpdateRate > 0)
			{
				return;
			}
			this.m_NodeMawUpdateRate = 90;
			foreach (NodeMawInfo current in this.m_NodeMaws)
			{
				bool flag = false;
				List<Ship> list = new List<Ship>();
				List<Ship> list2 = new List<Ship>();
				foreach (Ship current2 in this.m_Friendly)
				{
					if (Ship.IsActiveShip(current2) && (current2.Position - current.Pos).LengthSquared < current.Range * current.Range)
					{
						list2.Add(current2);
						if (current2.IsSuulka || current2.ShipRole == ShipRole.COMMAND)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					foreach (Ship current3 in this.m_Enemy)
					{
						if (Ship.IsActiveShip(current3) && (current3.Position - current.Pos).LengthSquared < current.Range * current.Range)
						{
							list.Add(current3);
						}
					}
					if (list.Count > 3 && list2.Count < 2)
					{
						this.m_Player.PostSetProp("SpawnNodeMaw", new object[]
						{
							current.Weapon.GameObject.ObjectID,
							current.Pos.X,
							current.Pos.Y,
							current.Pos.Z,
							current.Rot.X,
							current.Rot.Y,
							current.Rot.Z,
							current.Rot.W
						});
						current.Weapon.ReleaseGameObjectReference();
						this.m_NodeMaws.Remove(current);
						break;
					}
				}
			}
		}
		public static bool IsCombatGameObject(IGameObject obj)
		{
			return obj is Ship || obj is StellarBody || obj is StarModel;
		}
		public static List<IGameObject> GetCombatGameObjects(IEnumerable<IGameObject> objects)
		{
			List<IGameObject> objs = (
				from x in objects
				where CombatAI.IsCombatGameObject(x)
				select x).ToList<IGameObject>();
			List<IGameObject> list = new List<IGameObject>();
			list.AddRange(objs);
			IGameObject gameObject = objects.FirstOrDefault((IGameObject x) => x is Kerberos.Sots.GameStates.StarSystem);
			if (gameObject != null && gameObject is Kerberos.Sots.GameStates.StarSystem)
			{
				list.AddRange((
					from x in (gameObject as Kerberos.Sots.GameStates.StarSystem).Crits.Objects
					where !objs.Contains(x)
					select x).ToList<IGameObject>());
			}
			return list;
		}
	}
}
