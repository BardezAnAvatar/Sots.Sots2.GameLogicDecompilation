using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class TaskGroup
	{
		private const float MIN_FOR_ALT_ATTACK_MODES = 5f;
		private const float THREAT_RANGE = 6000f;
		private const int AVE_ATTACK_CHANGE_RATE = 4800;
		private const int AVE_ATTACK_CHANGE_RATE_DEV = 1200;
		public static int ABS_MIN_AGGRESSORS_PER_GROUP = 3;
		public static int DESIRED_MIN_AGGRESSORS_PER_GROUP = 5;
		public static int NUM_SHIPS_PER_SCOUT = 5;
		public static float ATTACK_GROUP_RANGE = 3000f;
		private App m_Game;
		private CombatAI m_CommanderAI;
		private List<Ship> m_Ships;
		private List<TaskGroupShipControl> m_ShipControls;
		private int[] m_NumShipTypes;
		private int m_CurrentChangeAttackDelay;
		private int m_ChangeAttackTime;
		private bool m_RequestRefreshShipControls;
		private float m_GroupSpeed;
		private TacticalObjective m_Objective;
		public TaskGroupOrders m_Orders;
		public bool m_bIsPlayerOrder;
		private EnemyGroup m_EnemyGroupInContact;
		private bool m_bIsInContactWithEnemy;
		private ObjectiveType m_RequestedObjectiveType;
		private TaskGroupType m_Type;
		public PatrolType m_PatrolType;
		public List<Ship> m_Targets;
		public EnemyGroup m_TargetGroup;
		public TaskGroup m_TargetTaskGroup;
		public CombatAI Commander
		{
			get
			{
				return this.m_CommanderAI;
			}
		}
		public float GroupSpeed
		{
			get
			{
				return this.m_GroupSpeed;
			}
		}
		public TacticalObjective Objective
		{
			get
			{
				return this.m_Objective;
			}
			set
			{
				if (value != this.m_Objective)
				{
					if (this.m_Objective != null)
					{
						this.m_Objective.RemoveTaskGroup(this);
					}
					this.m_Objective = value;
					if (this.m_Objective != null)
					{
						this.m_Objective.AssignResources(this);
					}
					this.m_RequestedObjectiveType = ObjectiveType.NO_OBJECTIVE;
					this.m_RequestRefreshShipControls = true;
				}
			}
		}
		public EnemyGroup EnemyGroupInContact
		{
			get
			{
				return this.m_EnemyGroupInContact;
			}
		}
		public bool IsInContactWithEnemy
		{
			get
			{
				return this.m_bIsInContactWithEnemy;
			}
		}
		public TaskGroupType Type
		{
			get
			{
				return this.m_Type;
			}
			set
			{
				this.m_Type = value;
			}
		}
		public void ClearEnemyGroupInContact()
		{
			this.m_EnemyGroupInContact = null;
			this.m_bIsInContactWithEnemy = false;
		}
		public TaskGroup(App game, CombatAI commandAI)
		{
			this.m_Game = game;
			this.m_CommanderAI = commandAI;
			this.m_Objective = null;
			this.m_Orders = TaskGroupOrders.None;
			this.m_Type = TaskGroupType.Aggressive;
			this.m_bIsPlayerOrder = false;
			this.m_bIsInContactWithEnemy = false;
			this.m_EnemyGroupInContact = null;
			this.m_Ships = new List<Ship>();
			this.m_Targets = new List<Ship>();
			this.m_ShipControls = new List<TaskGroupShipControl>();
			this.m_TargetGroup = null;
			this.m_TargetTaskGroup = null;
			this.m_PatrolType = PatrolType.Circular;
			this.m_RequestedObjectiveType = ObjectiveType.NO_OBJECTIVE;
			this.m_NumShipTypes = new int[14];
			for (int i = 0; i < 14; i++)
			{
				this.m_NumShipTypes[i] = 0;
			}
			this.m_GroupSpeed = 0f;
			this.m_RequestRefreshShipControls = false;
			this.m_ChangeAttackTime = 4800;
			this.m_CurrentChangeAttackDelay = this.m_ChangeAttackTime;
		}
		public void ShutDown()
		{
			if (this.m_Objective != null)
			{
				this.m_Objective.RemoveTaskGroup(this);
			}
			foreach (TaskGroupShipControl current in this.m_ShipControls)
			{
				current.ShutDown();
			}
		}
		public void AddShips(List<Ship> ships)
		{
			foreach (Ship current in ships)
			{
				this.AddShip(current);
			}
		}
		public ObjectiveType GetRequestedObjectiveType()
		{
			return this.m_RequestedObjectiveType;
		}
		public Vector3 GetBaseGroupPosition()
		{
			Vector3 vector = Vector3.Zero;
			if (this.m_Ships.Count == 0)
			{
				return vector;
			}
			if (this.m_ShipControls.Count == 0)
			{
				foreach (Ship current in this.m_Ships)
				{
					vector += current.Maneuvering.Position;
				}
				return vector / (float)this.m_Ships.Count;
			}
			foreach (TaskGroupShipControl current2 in this.m_ShipControls)
			{
				vector += current2.GetCurrentPosition();
			}
			return vector / (float)this.m_ShipControls.Count;
		}
		public static TaskGroupType GetTaskTypeFromShip(Ship ship)
		{
			if (ship.IsSuulka)
			{
				return TaskGroupType.Aggressive;
			}
			if (!ship.IsCarrier && ship.WeaponBanks.ToList<WeaponBank>().Count == 0)
			{
				return TaskGroupType.UnArmed;
			}
			if (ship.IsPlanetAssaultShip)
			{
				return TaskGroupType.PlanetAssault;
			}
			switch (ship.ShipRole)
			{
			case ShipRole.COMMAND:
				if (!Ship.IsShipClassBigger(ship.ShipClass, ShipClass.Dreadnought, true))
				{
					return TaskGroupType.Civilian;
				}
				return TaskGroupType.Aggressive;
			case ShipRole.COLONIZER:
			case ShipRole.CONSTRUCTOR:
			case ShipRole.SUPPLY:
			case ShipRole.GATE:
				return TaskGroupType.Civilian;
			case ShipRole.SCOUT:
			case ShipRole.BORE:
				return TaskGroupType.Passive;
			case ShipRole.FREIGHTER:
				if (!ship.IsNPCFreighter)
				{
					return TaskGroupType.Passive;
				}
				return TaskGroupType.Freighter;
			}
			return TaskGroupType.Aggressive;
		}
		public void UpdateTaskGroupType()
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			foreach (Ship current in this.m_Ships)
			{
				if (current.IsPolice)
				{
					num++;
				}
				else
				{
					switch (TaskGroup.GetTaskTypeFromShip(current))
					{
					case TaskGroupType.Passive:
						num4++;
						continue;
					case TaskGroupType.Civilian:
						num5++;
						continue;
					case TaskGroupType.Freighter:
						num2++;
						continue;
					case TaskGroupType.UnArmed:
						num6++;
						continue;
					case TaskGroupType.PlanetAssault:
						num7++;
						continue;
					}
					num3++;
				}
			}
			if (num > 0)
			{
				this.m_Type = TaskGroupType.Police;
			}
			else
			{
				if (num2 > 0)
				{
					this.m_Type = TaskGroupType.Freighter;
				}
				else
				{
					if (num6 > 0)
					{
						this.m_Type = TaskGroupType.UnArmed;
					}
					else
					{
						if (this.m_CommanderAI.IsEncounterCombat)
						{
							this.m_Type = TaskGroupType.Aggressive;
						}
						else
						{
							if (num7 > 0)
							{
								this.m_Type = TaskGroupType.PlanetAssault;
							}
							else
							{
								if (this.m_Ships.Count == 2 && num3 != this.m_Ships.Count && num5 != this.m_Ships.Count && num4 != this.m_Ships.Count)
								{
									this.m_Type = ((num5 > 0) ? TaskGroupType.Passive : TaskGroupType.Aggressive);
								}
								else
								{
									if (num5 > num4 && num5 > num3)
									{
										this.m_Type = ((num3 > num4) ? TaskGroupType.Passive : TaskGroupType.Civilian);
									}
									else
									{
										if (num3 > num4 && num3 > num5)
										{
											this.m_Type = ((num5 > num4 && num5 > num3 / 5) ? TaskGroupType.Passive : TaskGroupType.Aggressive);
										}
										else
										{
											this.m_Type = TaskGroupType.Passive;
										}
									}
								}
							}
						}
					}
				}
			}
			if (this.m_Type == TaskGroupType.Passive && (this.m_CommanderAI.GetAIType() == OverallAIType.PIRATE || this.m_CommanderAI.GetAIType() == OverallAIType.SLAVER))
			{
				this.m_Type = TaskGroupType.Aggressive;
			}
			if (this.m_Type == TaskGroupType.Civilian || this.m_Type == TaskGroupType.Freighter || this.m_Type == TaskGroupType.Police)
			{
				this.m_PatrolType = PatrolType.Orbit;
				return;
			}
			this.m_PatrolType = PatrolType.Circular;
		}
		public void UpdateGroupSpeed()
		{
			this.m_GroupSpeed = 0f;
			if (this.m_Ships.Count > 0)
			{
				this.m_GroupSpeed = this.m_Ships.Average((Ship x) => x.Maneuvering.MaxShipSpeed);
			}
		}
		public static bool IsValidTaskGroupShip(Ship ship)
		{
			return Ship.IsActiveShip(ship) && !ship.IsDriveless && !Ship.IsStationSize(ship.RealShipClass) && !Ship.IsBattleRiderSize(ship.RealShipClass) && !ship.IsAcceleratorHoop && ship.CombatStance != CombatStance.RETREAT && !ship.AssaultingPlanet && (!ship.IsSystemDefenceBoat || ship.DefenseBoatActive);
		}
		public bool IsDesiredGroupTargetTaken(TaskGroupShipControl group, Ship desiredTarget)
		{
			int num = CombatAI.AssessGroupStrength(group.GetShips());
			foreach (TaskGroupShipControl current in this.m_ShipControls)
			{
				if (current != group && current.GroupPriorityTarget == desiredTarget)
				{
					if (CombatAI.AssessGroupStrength(current.GetShips()) >= num)
					{
						return true;
					}
					break;
				}
			}
			return false;
		}
		public void AddShip(Ship ship)
		{
			if (ship.TaskGroup != null)
			{
				ship.TaskGroup.RemoveShip(ship);
			}
			this.m_Ships.Add(ship);
			ship.TaskGroup = this;
			this.m_NumShipTypes[(int)ship.RealShipClass]++;
			this.UpdateTaskGroupType();
			this.UpdateGroupSpeed();
			if (ship.IsSuulka)
			{
				ship.Maneuvering.TargetFacingAngle = TargetFacingAngle.BroadSide;
			}
			this.m_RequestRefreshShipControls = true;
		}
		public void RemoveShip(Ship ship)
		{
			if (!this.m_Ships.Contains(ship))
			{
				return;
			}
			ship.TaskGroup = null;
			if (this.m_Ships.Contains(ship))
			{
				this.m_NumShipTypes[(int)ship.RealShipClass]--;
				this.m_Ships.Remove(ship);
			}
			foreach (TaskGroupShipControl current in this.m_ShipControls)
			{
				current.RemoveShip(ship);
			}
			this.UpdateTaskGroupType();
			this.UpdateGroupSpeed();
			this.m_RequestRefreshShipControls = true;
		}
		public void ObjectRemoved(Ship ship)
		{
			this.m_Targets.Remove(ship);
			this.RemoveShip(ship);
			foreach (TaskGroupShipControl current in this.m_ShipControls)
			{
				if (current.GroupPriorityTarget == ship)
				{
					current.ClearPriorityTarget();
				}
			}
		}
		public bool ContainsShip(Ship ship)
		{
			return this.m_Ships.Contains(ship);
		}
		public int GetShipCount()
		{
			return this.m_Ships.Count;
		}
		public List<Ship> GetShips()
		{
			return this.m_Ships;
		}
		public void NotifyAllRidersDeployed(Ship carrier)
		{
		}
		public void NotifyAllRidersDocked(Ship carrier)
		{
			if (this.m_CommanderAI.GetAIType() == OverallAIType.SLAVER)
			{
				if (carrier.WeaponBanks.Any((WeaponBank x) => x.TurretClass == WeaponEnums.TurretClasses.AssaultShuttle))
				{
					carrier.SetCombatStance(CombatStance.RETREAT);
					List<AssaultShuttleLaunchControl> list = carrier.WeaponControls.OfType<AssaultShuttleLaunchControl>().ToList<AssaultShuttleLaunchControl>();
					foreach (AssaultShuttleLaunchControl current in list)
					{
						current.DisableWeaponFire = true;
					}
					carrier.TaskGroup = null;
				}
				if (!this.m_Ships.Any((Ship x) => x.WeaponBanks.Any((WeaponBank y) => y.TurretClass == WeaponEnums.TurretClasses.AssaultShuttle)))
				{
					this.m_Objective = null;
					this.m_RequestedObjectiveType = ObjectiveType.RETREAT;
				}
			}
		}
		public void Update(int framesElapsed)
		{
			if (this.m_Objective == null)
			{
				return;
			}
			if (!this.m_RequestRefreshShipControls && this.TaskGroupCanMerge())
			{
				Vector3 currentPosition = this.m_ShipControls[0].GetCurrentPosition();
				foreach (TaskGroupShipControl current in this.m_ShipControls)
				{
					if (current != this.m_ShipControls[0] && current.CanMerge && (currentPosition - current.GetCurrentPosition()).LengthSquared < TaskGroup.ATTACK_GROUP_RANGE * TaskGroup.ATTACK_GROUP_RANGE)
					{
						this.m_RequestRefreshShipControls = true;
						break;
					}
				}
			}
			if (this.m_Objective is AttackGroupObjective && (float)this.m_Ships.Count > 5f && this.m_ShipControls.Count > 0 && this.m_Objective.m_TargetEnemyGroup != null)
			{
				Vector3 currentPosition2 = this.m_ShipControls[0].GetCurrentPosition();
				Vector3 lastKnownHeading = this.m_Objective.m_TargetEnemyGroup.m_LastKnownHeading;
				Ship closestShip = this.m_Objective.m_TargetEnemyGroup.GetClosestShip(currentPosition2, TaskGroup.ATTACK_GROUP_RANGE);
				if (closestShip != null && lastKnownHeading.LengthSquared < 100f)
				{
					this.m_CurrentChangeAttackDelay -= framesElapsed;
				}
				else
				{
					this.m_CurrentChangeAttackDelay = this.m_ChangeAttackTime;
				}
				if ((float)this.m_CurrentChangeAttackDelay <= 0f)
				{
					this.m_RequestRefreshShipControls = true;
					this.m_ChangeAttackTime = this.m_CommanderAI.AIRandom.NextInclusive(3600, 6000);
					this.m_CurrentChangeAttackDelay = this.m_ChangeAttackTime;
				}
			}
			if (this.m_RequestRefreshShipControls)
			{
				this.RefreshShipControls();
			}
			if (this.m_ShipControls.Count == 0)
			{
				this.AssignShipsToObjective();
			}
			List<TaskGroupShipControl> list = new List<TaskGroupShipControl>();
			foreach (TaskGroupShipControl current2 in this.m_ShipControls)
			{
				if (current2.GetShipCount() > 0)
				{
					current2.Update(framesElapsed);
				}
				else
				{
					list.Add(current2);
				}
			}
			foreach (TaskGroupShipControl current3 in list)
			{
				current3.ShutDown();
				this.m_ShipControls.Remove(current3);
			}
			if (this.m_Type == TaskGroupType.BoardingGroup)
			{
				if (!this.m_Ships.Any((Ship x) => x.WeaponControls.Any((SpecWeaponControl y) => y is BoardingPodLaunchControl)))
				{
					this.Objective = null;
					this.UpdateTaskGroupType();
				}
			}
			if ((this.m_Type == TaskGroupType.Civilian || this.m_Type == TaskGroupType.UnArmed) && this.m_Objective is EvadeEnemyObjective)
			{
				int num;
				if (this.m_Type == TaskGroupType.Civilian)
				{
					num = (
						from x in this.m_CommanderAI.GetTaskGroups()
						where x.Type == TaskGroupType.Aggressive || x.Type == TaskGroupType.Passive
						select x).Count<TaskGroup>();
				}
				else
				{
					num = (
						from x in this.m_CommanderAI.GetTaskGroups()
						where x.Type == TaskGroupType.Aggressive || x.Type == TaskGroupType.Passive || x.Type == TaskGroupType.Civilian
						select x).Count<TaskGroup>();
				}
				if (num == 0)
				{
					this.Objective = null;
				}
			}
		}
		private bool TaskGroupCanMerge()
		{
			return this.m_ShipControls.Count > 1 && this.m_Type != TaskGroupType.Freighter && (this.m_Objective is AttackGroupObjective || this.m_Objective is AttackPlanetObjective);
		}
		private void RefreshShipControls()
		{
			foreach (TaskGroupShipControl current in this.m_ShipControls)
			{
				current.ShutDown();
			}
			this.m_ShipControls.Clear();
			this.m_RequestRefreshShipControls = false;
		}
		private void AssignShipsToObjective()
		{
			if (this.m_Ships.Count == 0)
			{
				return;
			}
			if (this.m_Objective is ScoutObjective)
			{
				Ship ship = CombatAI.FindBestScout(this.m_Ships);
				if (ship == null)
				{
					this.m_RequestedObjectiveType = ObjectiveType.PATROL;
					return;
				}
				ScoutShipControl scoutShipControl = new ScoutShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, ship.SensorRange * 0.75f);
				scoutShipControl.AddShip(ship, false);
				if (this.m_Type == TaskGroupType.Civilian)
				{
					foreach (Ship current in this.m_Ships)
					{
						if (current != ship)
						{
							scoutShipControl.AddShip(current, false);
						}
					}
					this.m_ShipControls.Add(scoutShipControl);
					return;
				}
				ScoutTrailShipControl scoutTrailShipControl = new ScoutTrailShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, scoutShipControl);
				foreach (Ship current2 in this.m_Ships)
				{
					if (current2 != ship)
					{
						scoutTrailShipControl.AddShip(current2, false);
					}
				}
				this.m_ShipControls.Add(scoutShipControl);
				this.m_ShipControls.Add(scoutTrailShipControl);
				return;
			}
			else
			{
				if (this.m_Objective is PatrolObjective)
				{
					PatrolShipControl patrolShipControl = null;
					if (this.m_Type == TaskGroupType.Police)
					{
						patrolShipControl = new PatrolShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, this.m_PatrolType, Vector3.UnitZ, this.m_Game.AssetDatabase.PolicePatrolRadius, this.m_CommanderAI.AIRandom.CoinToss(0.5));
					}
					else
					{
						Vector3 dir = -this.m_Objective.GetObjectiveLocation();
						if (dir.LengthSquared > 0f)
						{
							dir.Normalize();
						}
						else
						{
							dir = -Vector3.UnitZ;
						}
						bool clockwise = this.m_Type != TaskGroupType.Freighter && this.m_CommanderAI.AIRandom.CoinToss(0.5);
						patrolShipControl = new PatrolShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, this.m_PatrolType, dir, 10000f, clockwise);
					}
					foreach (Ship current3 in this.m_Ships)
					{
						patrolShipControl.AddShip(current3, false);
					}
					this.m_ShipControls.Add(patrolShipControl);
					return;
				}
				if (this.m_Objective is AttackGroupObjective)
				{
					this.AssignShipsToAttackObjective();
					return;
				}
				if (this.m_Objective is AttackPlanetObjective)
				{
					AttackPlanetShipControl attackPlanetShipControl = new AttackPlanetShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI);
					foreach (Ship current4 in this.m_Ships)
					{
						attackPlanetShipControl.AddShip(current4, false);
					}
					this.m_ShipControls.Add(attackPlanetShipControl);
					return;
				}
				if (this.m_Objective is DefendPlanetObjective)
				{
					this.AssignShipsToDefendObjective();
					return;
				}
				if (this.m_Objective is EvadeEnemyObjective)
				{
					EvadeEnemyObjective evadeEnemyObjective = this.m_Objective as EvadeEnemyObjective;
					if (evadeEnemyObjective.EvadePatrolObjective != null)
					{
						Vector3 safePatrolDirection = evadeEnemyObjective.GetSafePatrolDirection(this.GetBaseGroupPosition());
						PatrolShipControl patrolShipControl2 = new PatrolShipControl(this.m_Game, evadeEnemyObjective.EvadePatrolObjective, this.m_CommanderAI, PatrolType.Circular, safePatrolDirection, 10000f, this.m_CommanderAI.AIRandom.CoinToss(0.5));
						foreach (Ship current5 in this.m_Ships)
						{
							patrolShipControl2.AddShip(current5, false);
						}
						this.m_ShipControls.Add(patrolShipControl2);
						return;
					}
					this.m_RequestedObjectiveType = ObjectiveType.PATROL;
					return;
				}
				else
				{
					if (this.m_Objective is RetreatObjective)
					{
						RetreatShipControl retreatShipControl = new RetreatShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, this.m_Type != TaskGroupType.Freighter);
						foreach (Ship current6 in this.m_Ships)
						{
							retreatShipControl.AddShip(current6, false);
						}
						this.m_ShipControls.Add(retreatShipControl);
						return;
					}
					if (this.m_Objective is BoardTargetObjective)
					{
						float num = 3.40282347E+38f;
						foreach (Ship current7 in this.m_Ships)
						{
							num = Math.Min(num, CombatAI.GetAveEffectiveWeaponRange(current7, false));
						}
						num *= 0.75f;
						PursueShipControl pursueShipControl = new PursueShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, num);
						foreach (Ship current8 in this.m_Ships)
						{
							pursueShipControl.AddShip(current8, false);
						}
						this.m_ShipControls.Add(pursueShipControl);
						return;
					}
					if (this.m_Objective is FollowTaskGroupObjective)
					{
						SupportGroupShipControl supportGroupShipControl = new SupportGroupShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, null);
						foreach (Ship current9 in this.m_Ships)
						{
							supportGroupShipControl.AddShip(current9, false);
						}
						this.m_ShipControls.Add(supportGroupShipControl);
					}
					return;
				}
			}
		}
		private void AssignShipsToDefendObjective()
		{
			DefendPlanetObjective defendPlanetObjective = this.m_Objective as DefendPlanetObjective;
			if (defendPlanetObjective.DefendPatrolObjective != null)
			{
				int num = 0;
				using (List<Ship>.Enumerator enumerator = this.m_Ships.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Ship current = enumerator.Current;
						if (num < this.m_ShipControls.Count)
						{
							this.m_ShipControls[num].AddShip(current, false);
						}
						else
						{
							Vector3 patrolDirection = defendPlanetObjective.GetPatrolDirection(num);
							PatrolShipControl patrolShipControl = new PatrolShipControl(this.m_Game, defendPlanetObjective.DefendPatrolObjective, this.m_CommanderAI, this.m_PatrolType, patrolDirection, 10000f, this.m_CommanderAI.AIRandom.CoinToss(0.5));
							patrolShipControl.AddShip(current, false);
							this.m_ShipControls.Add(patrolShipControl);
						}
						num = (num + 1) % 6;
					}
					return;
				}
			}
			this.m_RequestedObjectiveType = ObjectiveType.PATROL;
		}
		private void AssignShipsToAttackObjective()
		{
			if (this.m_Objective != null && this.m_Objective.m_TargetEnemyGroup == null)
			{
				this.m_RequestedObjectiveType = ObjectiveType.PATROL;
				return;
			}
			if (this.m_Type == TaskGroupType.Police || this.m_CommanderAI.GetAIType() == OverallAIType.PIRATE)
			{
				this.m_ShipControls.Add(this.CreatePursueAttackGroup(this.m_Ships));
				return;
			}
			Vector3 objectiveLocation = this.m_Objective.GetObjectiveLocation();
			float groupRadius = this.m_Objective.m_TargetEnemyGroup.GetGroupRadius();
			List<Ship> list = new List<Ship>();
			List<Ship> list2 = new List<Ship>();
			int[] array = new int[14];
			foreach (Ship current in this.m_Ships)
			{
				float num = groupRadius + TaskGroup.ATTACK_GROUP_RANGE + current.SensorRange;
				if ((objectiveLocation - current.Maneuvering.Position).LengthSquared < num * num)
				{
					list.Add(current);
					array[(int)current.RealShipClass]++;
				}
				else
				{
					list2.Add(current);
				}
			}
			int num2 = CombatAI.AssessGroupStrength(this.m_Objective.m_TargetEnemyGroup.m_Ships);
			int num3 = CombatAI.AssessGroupStrength(list);
			if (num3 > num2 && this.m_Type != TaskGroupType.Civilian)
			{
				List<Ship> list3 = (
					from x in list
					where x.RealShipClass == RealShipClasses.Leviathan || x.RealShipClass == RealShipClasses.Dreadnought
					select x).ToList<Ship>();
				foreach (Ship current2 in list3)
				{
					list.Remove(current2);
				}
				if (list3.Count > 0)
				{
					this.m_ShipControls.Add(this.CreateStandOffAttackGroup(list3));
				}
				if (list.Count > 0)
				{
					float num4 = 5f;
					float num5 = Math.Min(Math.Max((float)list.Count / 5f - 1f, 0f), 1f) * 5f;
					float num6 = 0f;
					float maxValue = num4 + num5 + num6;
					float num7 = this.m_CommanderAI.AIRandom.NextInclusive(0f, maxValue);
					if (num7 <= num4)
					{
						List<Ship> list4 = new List<Ship>();
						int num8 = Math.Max(list.Count - 5, 0);
						if (num8 > 0 && this.m_Type != TaskGroupType.Civilian)
						{
							int num9 = 1;
							while (num9 > 0 && num8 > 0)
							{
								num9 = this.m_CommanderAI.AIRandom.NextInclusive(0, num8);
								for (int i = 0; i < num9; i++)
								{
									list4.Add(list[i]);
								}
								if (list4.Count == 0)
								{
									break;
								}
								foreach (Ship current3 in list4)
								{
									list.Remove(current3);
								}
								this.m_ShipControls.Add(this.CreateStandOffAttackGroup(list4));
								num8 = Math.Max(list.Count - 5, 0);
							}
						}
						this.m_ShipControls.Add(this.CreateFlyByAttackGroup(list));
					}
					else
					{
						if (list.Count > 0)
						{
							bool flag = false;
							int num10 = Math.Min(list.Count, 6);
							int val = (int)Math.Ceiling((double)list.Count / (double)num10);
							float num11 = MathHelper.DegreesToRadians(360f / (float)num10);
							float num12 = -(num11 * 0.5f);
							Vector3 baseGroupPosition = this.GetBaseGroupPosition();
							Vector3 forward = objectiveLocation - baseGroupPosition;
							forward.Y = 0f;
							forward.Normalize();
							Matrix rhs = Matrix.CreateWorld(baseGroupPosition, forward, Vector3.UnitY);
							for (int j = 0; j < num10; j++)
							{
								float num13 = (j % 2 == 0) ? -1f : 1f;
								float num14 = (float)Math.Floor((double)(j % num10 + 1) * 0.5);
								Matrix lhs = Matrix.CreateRotationYPR(num13 * num11 * num14 + num12, 0f, 0f);
								lhs *= rhs;
								List<Ship> list5 = new List<Ship>();
								int num15 = (list.Count <= num10 - j) ? 1 : Math.Min(val, list.Count - 1);
								for (int k = 0; k < num15; k++)
								{
									list5.Add(list[k]);
								}
								foreach (Ship current4 in list5)
								{
									list.Remove(current4);
								}
								if (flag)
								{
									this.m_ShipControls.Add(this.CreateFlankAttackGroups(list5, lhs.Forward));
								}
								else
								{
									this.m_ShipControls.Add(this.CreateSurroundAttackGroups(list5, lhs.Forward));
								}
								if (list.Count == 0)
								{
									break;
								}
							}
						}
					}
				}
			}
			else
			{
				if (num3 * 2 < num2)
				{
					this.m_ShipControls.Add(this.CreateStandOffAttackGroup(list));
				}
				else
				{
					this.m_ShipControls.Add(this.CreatePursueAttackGroup(list));
				}
			}
			TaskGroupShipControl taskGroupShipControl = this.m_ShipControls[0];
			foreach (Ship current5 in list2)
			{
				bool flag2 = false;
				foreach (TaskGroupShipControl current6 in this.m_ShipControls)
				{
					if (current6 != taskGroupShipControl)
					{
						foreach (Ship current7 in current6.GetShips())
						{
							if ((current7.Maneuvering.Position - current5.Maneuvering.Position).LengthSquared < TaskGroup.ATTACK_GROUP_RANGE * TaskGroup.ATTACK_GROUP_RANGE)
							{
								current6.AddShip(current5, false);
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							break;
						}
					}
				}
				if (!flag2)
				{
					this.m_ShipControls.Add(this.CreateSupportGroup(new List<Ship>
					{
						current5
					}, this.m_ShipControls[0]));
				}
			}
		}
		private TaskGroupShipControl CreateStandOffAttackGroup(List<Ship> ships)
		{
			float num = 0f;
			float num2 = 3.40282347E+38f;
			foreach (Ship current in ships)
			{
				num += CombatAI.GetMinEffectiveWeaponRange(current, false);
				num2 = Math.Min(num2, current.SensorRange);
			}
			num /= (float)ships.Count;
			num2 *= 0.75f;
			num *= 0.75f;
			float num3 = num + 500f;
			if (num3 > num2)
			{
				num3 = num2;
				num = 0.75f * num3;
			}
			else
			{
				if (num >= num3)
				{
					num = num3 * 0.75f;
				}
			}
			StandOffShipControl standOffShipControl = new StandOffShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, num, num3);
			foreach (Ship current2 in ships)
			{
				standOffShipControl.AddShip(current2, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(current2) == TaskGroupType.Civilian);
			}
			return standOffShipControl;
		}
		private TaskGroupShipControl CreatePursueAttackGroup(List<Ship> ships)
		{
			float num = 3.40282347E+38f;
			float num2 = 3.40282347E+38f;
			foreach (Ship current in ships)
			{
				num = Math.Min(num, CombatAI.GetAveEffectiveWeaponRange(current, false));
				num2 = Math.Min(num2, current.SensorRange);
			}
			num = Math.Min(num * 0.75f, num2 * 0.75f);
			PursueShipControl pursueShipControl = new PursueShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, num);
			foreach (Ship current2 in ships)
			{
				pursueShipControl.AddShip(current2, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(current2) == TaskGroupType.Civilian);
			}
			return pursueShipControl;
		}
		private TaskGroupShipControl CreateSupportGroup(List<Ship> ships, TaskGroupShipControl supportGroup)
		{
			SupportGroupShipControl supportGroupShipControl = new SupportGroupShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, supportGroup);
			foreach (Ship current in ships)
			{
				supportGroupShipControl.AddShip(current, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(current) == TaskGroupType.Civilian);
			}
			return supportGroupShipControl;
		}
		private TaskGroupShipControl CreateFlyByAttackGroup(List<Ship> ships)
		{
			float num = 0f;
			foreach (Ship current in ships)
			{
				num += CombatAI.GetMinEffectiveWeaponRange(current, false);
			}
			num /= (float)ships.Count;
			num *= 0.75f;
			FlyByShipControl flyByShipControl = new FlyByShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, Math.Max(num, 1000f));
			foreach (Ship current2 in ships)
			{
				flyByShipControl.AddShip(current2, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(current2) == TaskGroupType.Civilian);
			}
			return flyByShipControl;
		}
		private TaskGroupShipControl CreateSurroundAttackGroups(List<Ship> ships, Vector3 attackVec)
		{
			float num = 0f;
			foreach (Ship current in ships)
			{
				num += CombatAI.GetMinEffectiveWeaponRange(current, false);
			}
			num /= (float)ships.Count;
			num *= 0.75f;
			float desiredDist = num + 500f;
			SurroundShipControl surroundShipControl = new SurroundShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, attackVec, num, desiredDist);
			foreach (Ship current2 in ships)
			{
				surroundShipControl.AddShip(current2, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(current2) == TaskGroupType.Civilian);
			}
			return surroundShipControl;
		}
		private TaskGroupShipControl CreateFlankAttackGroups(List<Ship> ships, Vector3 attackVec)
		{
			float num = 0f;
			foreach (Ship current in ships)
			{
				num += CombatAI.GetMinEffectiveWeaponRange(current, false);
			}
			num /= (float)ships.Count;
			num *= 0.75f;
			float desiredDist = num + 500f;
			FlankShipControl flankShipControl = new FlankShipControl(this.m_Game, this.m_Objective, this.m_CommanderAI, attackVec, num, desiredDist);
			foreach (Ship current2 in ships)
			{
				flankShipControl.AddShip(current2, (this.m_Type == TaskGroupType.Aggressive || this.m_Type == TaskGroupType.Passive) && TaskGroup.GetTaskTypeFromShip(current2) == TaskGroupType.Civilian);
			}
			return flankShipControl;
		}
		public bool UpdateEnemyContact()
		{
			this.m_bIsInContactWithEnemy = false;
			if (this.m_Objective is DefendPlanetObjective)
			{
				this.m_EnemyGroupInContact = (this.m_Objective as DefendPlanetObjective).GetClosestThreat();
				this.m_bIsInContactWithEnemy = (this.m_EnemyGroupInContact != null);
			}
			if (!this.m_bIsInContactWithEnemy)
			{
				foreach (EnemyGroup current in this.m_CommanderAI.GetEnemyGroups())
				{
					foreach (Ship current2 in current.m_Ships)
					{
						foreach (TaskGroupShipControl current3 in this.m_ShipControls)
						{
							float lengthSquared = (current2.Maneuvering.Position - current3.GetCurrentPosition()).LengthSquared;
							if (lengthSquared < current3.SensorRange * current3.SensorRange && (this.m_EnemyGroupInContact == null || this.m_EnemyGroupInContact == current || current.IsHigherPriorityThan(this.m_EnemyGroupInContact, this.m_CommanderAI, false)))
							{
								if (current3 is ScoutShipControl)
								{
									(current3 as ScoutShipControl).NotifyEnemyGroupDetected(current);
									if (this.m_ShipControls.Any((TaskGroupShipControl x) => x is ScoutTrailShipControl))
									{
										continue;
									}
								}
								this.m_bIsInContactWithEnemy = true;
								this.m_EnemyGroupInContact = current;
							}
						}
					}
				}
			}
			if (this.m_bIsInContactWithEnemy && !(this.m_Objective is RetreatObjective) && !(this.m_Objective is BoardTargetObjective) && this.m_Type != TaskGroupType.Freighter && this.m_Type != TaskGroupType.BoardingGroup)
			{
				if (this.m_Objective is AttackGroupObjective || this.m_Objective is AttackPlanetObjective)
				{
					float arg_235_0;
					if (this.m_EnemyGroupInContact.m_Ships.Count <= 0)
					{
						arg_235_0 = 0f;
					}
					else
					{
						arg_235_0 = this.m_EnemyGroupInContact.m_Ships.Average((Ship x) => x.Maneuvering.MaxShipSpeed);
					}
					float num = arg_235_0;
					if ((!(this.m_Objective is AttackPlanetObjective) || (this.m_Type != TaskGroupType.PlanetAssault && num <= this.m_GroupSpeed + 5f)) && (this.m_Objective.m_TargetEnemyGroup == null || (this.m_Objective.m_TargetEnemyGroup != this.m_EnemyGroupInContact && !this.m_Objective.m_TargetEnemyGroup.IsFreighterEnemyGroup())) && (this.m_Objective.m_TargetEnemyGroup == null || this.m_EnemyGroupInContact.IsHigherPriorityThan(this.m_Objective.m_TargetEnemyGroup, this.m_CommanderAI, false)))
					{
						this.Objective = null;
					}
				}
				else
				{
					if (this.m_Objective is EvadeEnemyObjective && this.m_CommanderAI.GetTaskGroups().Count<TaskGroup>() > 1)
					{
						EvadeEnemyObjective evadeEnemyObjective = this.m_Objective as EvadeEnemyObjective;
						Vector3 safePatrolDirection = evadeEnemyObjective.GetSafePatrolDirection(this.GetBaseGroupPosition());
						using (List<TaskGroupShipControl>.Enumerator enumerator4 = this.m_ShipControls.GetEnumerator())
						{
							while (enumerator4.MoveNext())
							{
								TaskGroupShipControl current4 = enumerator4.Current;
								if (current4 is PatrolShipControl)
								{
									PatrolShipControl patrolShipControl = current4 as PatrolShipControl;
									if (Vector3.Dot(safePatrolDirection, patrolShipControl.PreviousDir) < 0.8f)
									{
										patrolShipControl.ResetPatrolWaypoints(PatrolType.Circular, safePatrolDirection, 10000f);
									}
								}
							}
							goto IL_386;
						}
					}
					this.m_RequestedObjectiveType = ObjectiveType.ATTACK_TARGET;
				}
			}
			IL_386:
			return this.m_bIsInContactWithEnemy;
		}
	}
}
