using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class SwarmerSpawnerControl : CombatAIController
	{
		protected App m_Game;
		protected Ship m_SwarmerSpawner;
		protected List<Ship> m_LoadingSwarmers;
		protected List<Ship> m_LoadingGuardians;
		protected List<Ship> m_LaunchingShips;
		protected List<int> m_AttackingPlanetSwarmers;
		protected List<int> m_AttackingPlanetGardians;
		protected List<SwarmerAttackerControl> m_SpawnedSwarmers;
		protected List<SwarmerAttackerControl> m_SpawnedGuardians;
		protected List<SwarmerTarget> m_TargetList;
		protected SwarmerSpawnerStates m_State;
		protected int m_MaxSwarmers;
		protected int m_MaxGuardians;
		protected int m_UpdateListDelay;
		protected int m_MaxToCreate;
		protected int m_NumSwarmersToAttackPlanet;
		protected int m_NumGardiansToAttackPlanet;
		protected bool m_UpdateTargetList;
		private bool m_IsDeepSpace;
		public SwarmerSpawnerStates State
		{
			get
			{
				return this.m_State;
			}
			set
			{
				this.m_State = value;
			}
		}
		public override Ship GetShip()
		{
			return this.m_SwarmerSpawner;
		}
		public override void SetTarget(IGameObject target)
		{
		}
		public override IGameObject GetTarget()
		{
			return null;
		}
		public SwarmerSpawnerControl(App game, Ship ship, int systemId)
		{
			this.m_Game = game;
			this.m_SwarmerSpawner = ship;
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			this.m_IsDeepSpace = (starSystemInfo != null && starSystemInfo.IsDeepSpace);
		}
		public override void Initialize()
		{
			this.m_LoadingSwarmers = new List<Ship>();
			this.m_LoadingGuardians = new List<Ship>();
			this.m_LaunchingShips = new List<Ship>();
			this.m_AttackingPlanetSwarmers = new List<int>();
			this.m_AttackingPlanetGardians = new List<int>();
			this.m_SpawnedSwarmers = new List<SwarmerAttackerControl>();
			this.m_SpawnedGuardians = new List<SwarmerAttackerControl>();
			this.m_TargetList = new List<SwarmerTarget>();
			this.m_State = SwarmerSpawnerStates.IDLE;
			this.m_UpdateTargetList = false;
			this.m_MaxSwarmers = 30;
			this.m_MaxGuardians = 5;
			this.m_NumSwarmersToAttackPlanet = 0;
			this.m_NumGardiansToAttackPlanet = 0;
			if (this.m_Game.Game.ScriptModules != null && this.m_Game.Game.ScriptModules.Swarmers != null)
			{
				if (this.m_SwarmerSpawner.DesignID == this.m_Game.Game.ScriptModules.Swarmers.SwarmQueenDesignID)
				{
					this.m_MaxSwarmers = this.m_Game.AssetDatabase.GlobalSwarmerData.NumQueenSwarmers;
					this.m_MaxGuardians = this.m_Game.AssetDatabase.GlobalSwarmerData.NumQueenGuardians;
					this.m_NumSwarmersToAttackPlanet = this.m_MaxSwarmers / 2;
					this.m_NumGardiansToAttackPlanet = this.m_MaxGuardians / 2;
				}
				else
				{
					this.m_MaxSwarmers = this.m_Game.AssetDatabase.GlobalSwarmerData.NumHiveSwarmers;
					this.m_MaxGuardians = this.m_Game.AssetDatabase.GlobalSwarmerData.NumHiveGuardians;
				}
			}
			this.m_MaxToCreate = this.m_MaxSwarmers + this.m_MaxGuardians;
			this.m_UpdateListDelay = 0;
			this.m_SwarmerSpawner.PostSetProp("SetAsOnlyLaunchCarrier", true);
		}
		private void SpawnMaxSwarmerAttackers(Swarmers swarmers)
		{
			if (this.m_MaxToCreate <= 0)
			{
				return;
			}
			Matrix worldMat = Matrix.CreateRotationYPR(this.m_SwarmerSpawner.Maneuvering.Rotation);
			worldMat.Position = this.m_SwarmerSpawner.Maneuvering.Position;
			int num = Math.Max(this.m_SwarmerSpawner.BattleRiderMounts.Count<BattleRiderMount>() - (this.m_LoadingSwarmers.Count + this.m_LoadingGuardians.Count + this.m_LaunchingShips.Count), 0);
			if (num == 0)
			{
				return;
			}
			int num2 = Math.Max(this.m_MaxSwarmers - (this.m_LoadingSwarmers.Count + this.m_SpawnedSwarmers.Count), 0);
			int num3 = Math.Max(this.m_MaxGuardians - (this.m_LoadingGuardians.Count + this.m_SpawnedGuardians.Count), 0);
			if (num2 + num3 == 0)
			{
				return;
			}
			int num4 = Math.Min(num / 2, num2);
			int num5 = Math.Min(num - num4, num3);
			num4 += Math.Max(num - (num4 + num5), 0);
			num4 = Math.Min(num4, num2);
			int num6 = num4 + num5;
			for (int i = 0; i < num6; i++)
			{
				int num7;
				if (num4 > 0 && (i % 2 == 0 || num5 == 0))
				{
					num7 = swarmers.SwarmerDesignID;
					num4--;
				}
				else
				{
					if (num5 <= 0)
					{
						break;
					}
					num7 = swarmers.GuardianDesignID;
					num5--;
				}
				Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, num7, this.m_SwarmerSpawner.ObjectID, this.m_SwarmerSpawner.InputID, this.m_SwarmerSpawner.Player.ObjectID);
				if (ship != null)
				{
					if (num7 == swarmers.GuardianDesignID)
					{
						this.m_LoadingGuardians.Add(ship);
					}
					else
					{
						this.m_LoadingSwarmers.Add(ship);
					}
					this.m_MaxToCreate--;
				}
				if (this.m_MaxToCreate <= 0)
				{
					break;
				}
			}
			if (this.m_LoadingSwarmers.Count > 0 || this.m_LoadingGuardians.Count > 0)
			{
				this.m_State = SwarmerSpawnerStates.INTEGRATESWARMER;
			}
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (SwarmerAttackerControl current in this.m_SpawnedSwarmers)
			{
				if (current.GetShip() == obj)
				{
					this.m_SpawnedSwarmers.Remove(current);
					break;
				}
			}
			foreach (SwarmerAttackerControl current2 in this.m_SpawnedGuardians)
			{
				if (current2.GetShip() == obj)
				{
					this.m_SpawnedGuardians.Remove(current2);
					break;
				}
			}
			foreach (Ship current3 in this.m_LoadingSwarmers)
			{
				if (current3 == obj)
				{
					this.m_LoadingSwarmers.Remove(current3);
					break;
				}
			}
			foreach (Ship current4 in this.m_LoadingGuardians)
			{
				if (current4 == obj)
				{
					this.m_LoadingGuardians.Remove(current4);
					break;
				}
			}
			foreach (SwarmerTarget current5 in this.m_TargetList)
			{
				if (current5.Target == obj)
				{
					this.m_TargetList.Remove(current5);
					break;
				}
			}
			foreach (Ship current6 in this.m_LaunchingShips)
			{
				if (current6 == obj)
				{
					this.m_LaunchingShips.Remove(current6);
					break;
				}
			}
		}
		public void AddChild(CombatAIController child)
		{
			if (child is SwarmerAttackerControl)
			{
				foreach (Ship current in this.m_LoadingSwarmers)
				{
					if (current == child.GetShip())
					{
						this.m_LoadingSwarmers.Remove(current);
						this.m_LaunchingShips.Add(current);
						this.m_SpawnedSwarmers.Add(child as SwarmerAttackerControl);
						break;
					}
				}
				foreach (Ship current2 in this.m_LoadingGuardians)
				{
					if (current2 == child.GetShip())
					{
						this.m_LoadingGuardians.Remove(current2);
						this.m_LaunchingShips.Add(current2);
						this.m_SpawnedGuardians.Add(child as SwarmerAttackerControl);
						break;
					}
				}
			}
		}
		public bool IsThisMyParent(Ship ship)
		{
			return this.m_LoadingSwarmers.Any((Ship x) => x == ship) || this.m_LoadingGuardians.Any((Ship x) => x == ship);
		}
		public override void OnThink()
		{
			if (this.m_SwarmerSpawner == null)
			{
				return;
			}
			this.UpdateTargetList();
			if (this.m_TargetList.Count > 0)
			{
				this.MaintainMaxSwarmers();
			}
			switch (this.m_State)
			{
			case SwarmerSpawnerStates.IDLE:
				this.ThinkIdle();
				return;
			case SwarmerSpawnerStates.EMITSWARMER:
				this.ThinkEmitAttackSwarmer();
				return;
			case SwarmerSpawnerStates.INTEGRATESWARMER:
				this.ThinkIntegrateAttackSwarmer();
				return;
			case SwarmerSpawnerStates.ADDINGSWARMERS:
				this.ThinkAddingSwarmers();
				return;
			case SwarmerSpawnerStates.LAUNCHSWARMER:
				this.ThinkLaunch();
				return;
			case SwarmerSpawnerStates.WAITFORLAUNCH:
				this.ThinkWaitForLaunch();
				return;
			default:
				return;
			}
		}
		public override void ForceFlee()
		{
		}
		public override bool VictoryConditionIsMet()
		{
			return false;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_UpdateTargetList;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			List<SwarmerTarget> list = new List<SwarmerTarget>();
			List<SwarmerTarget> list2 = new List<SwarmerTarget>();
			foreach (SwarmerTarget current in this.m_TargetList)
			{
				current.ClearNumTargets();
			}
			list.AddRange(this.m_TargetList);
			foreach (IGameObject current2 in objs)
			{
				if (current2 is Ship)
				{
					Ship ship = current2 as Ship;
					if (Ship.IsActiveShip(ship) && (this.m_IsDeepSpace || ship.IsDetected(this.m_SwarmerSpawner.Player)) && ship.Player != this.m_SwarmerSpawner.Player)
					{
						list2.Add(new SwarmerTarget
						{
							Target = ship
						});
						SwarmerTarget swarmerTarget = list.FirstOrDefault((SwarmerTarget x) => x.Target == ship);
						if (swarmerTarget != null)
						{
							list.Remove(swarmerTarget);
						}
					}
				}
				else
				{
					if (current2 is StellarBody)
					{
						StellarBody planet = current2 as StellarBody;
						if (planet.Population > 0.0)
						{
							list2.Add(new SwarmerTarget
							{
								Target = planet
							});
							SwarmerTarget swarmerTarget2 = list.FirstOrDefault((SwarmerTarget x) => x.Target == planet);
							if (swarmerTarget2 != null)
							{
								list.Remove(swarmerTarget2);
							}
						}
					}
				}
			}
			if (list.Count > 0)
			{
				foreach (SwarmerAttackerControl swarmer in this.m_SpawnedSwarmers)
				{
					if (list.Any((SwarmerTarget x) => x.Target == swarmer.GetTarget()))
					{
						swarmer.SetTarget(null);
					}
				}
				foreach (SwarmerAttackerControl guardian in this.m_SpawnedGuardians)
				{
					if (list.Any((SwarmerTarget x) => x.Target == guardian.GetTarget()))
					{
						guardian.SetTarget(null);
					}
				}
			}
			if (list2.Count > this.m_TargetList.Count)
			{
				using (List<SwarmerTarget>.Enumerator enumerator5 = this.m_TargetList.GetEnumerator())
				{
					while (enumerator5.MoveNext())
					{
						SwarmerTarget current3 = enumerator5.Current;
						current3.ClearNumTargets();
						int num = 0;
						int num2 = 0;
						if (current3.Target is Ship)
						{
							ShipClass shipClass = (current3.Target as Ship).ShipClass;
							num = SwarmerAttackerControl.NumSwarmersPerShip(shipClass);
							num2 = SwarmerAttackerControl.NumGuardiansPerShip(shipClass);
						}
						else
						{
							if (current3.Target is StellarBody)
							{
								num = this.m_NumSwarmersToAttackPlanet;
								num2 = this.m_NumGardiansToAttackPlanet;
							}
						}
						foreach (SwarmerAttackerControl current4 in this.m_SpawnedSwarmers)
						{
							if (current4.GetTarget() == current3.Target)
							{
								if (num > 0)
								{
									num--;
									current3.IncSwarmersOnTarget();
								}
								else
								{
									current4.SetTarget(null);
								}
							}
						}
						foreach (SwarmerAttackerControl current5 in this.m_SpawnedGuardians)
						{
							if (current5.GetTarget() == current3.Target)
							{
								if (num2 > 0)
								{
									num2--;
									current3.IncGuardiansOnTarget();
								}
								else
								{
									current5.SetTarget(null);
								}
							}
						}
					}
					goto IL_4F9;
				}
			}
			foreach (SwarmerAttackerControl swarmer in this.m_SpawnedSwarmers)
			{
				if (swarmer.GetTarget() != null)
				{
					SwarmerTarget swarmerTarget3 = this.m_TargetList.FirstOrDefault((SwarmerTarget x) => x.Target == swarmer.GetTarget());
					if (swarmerTarget3 != null)
					{
						swarmerTarget3.IncSwarmersOnTarget();
					}
				}
			}
			foreach (SwarmerAttackerControl guardian in this.m_SpawnedGuardians)
			{
				if (guardian.GetTarget() != null)
				{
					SwarmerTarget swarmerTarget4 = this.m_TargetList.FirstOrDefault((SwarmerTarget x) => x.Target == guardian.GetTarget());
					if (swarmerTarget4 != null)
					{
						swarmerTarget4.IncGuardiansOnTarget();
					}
				}
			}
			IL_4F9:
			foreach (SwarmerTarget target in list2)
			{
				if (!this.m_TargetList.Any((SwarmerTarget x) => x.Target == target.Target))
				{
					target.ClearNumTargets();
					this.m_TargetList.Add(target);
				}
			}
			this.m_UpdateTargetList = false;
		}
		public void RequestTargetFromParent(SwarmerAttackerControl sac)
		{
			if (this.m_TargetList.Count == 0)
			{
				return;
			}
			SwarmerTarget swarmerTarget = null;
			SwarmerTarget swarmerTarget2 = null;
			SwarmerTarget swarmerTarget3 = null;
			float num = 3.40282347E+38f;
			float num2 = 3.40282347E+38f;
			float num3 = 3.40282347E+38f;
			foreach (SwarmerTarget current in this.m_TargetList)
			{
				int num4 = 0;
				int num5 = 0;
				float num6 = 0f;
				if (current.Target is Ship)
				{
					Ship ship = current.Target as Ship;
					ShipClass shipClass = ship.ShipClass;
					num4 = SwarmerAttackerControl.NumSwarmersPerShip(shipClass) - current.SwarmersOnTarget;
					num5 = SwarmerAttackerControl.NumGuardiansPerShip(shipClass) - current.GuardiansOnTarget;
					num6 = (ship.Position - sac.GetShip().Position).LengthSquared;
					if (num6 < num3)
					{
						num3 = num6;
						swarmerTarget3 = current;
					}
				}
				else
				{
					if (current.Target is StellarBody)
					{
						StellarBody stellarBody = current.Target as StellarBody;
						num4 = this.m_NumSwarmersToAttackPlanet - this.m_AttackingPlanetSwarmers.Count;
						num5 = this.m_NumGardiansToAttackPlanet - this.m_AttackingPlanetGardians.Count;
						num6 = (stellarBody.Parameters.Position - sac.GetShip().Position).LengthSquared;
						if (num6 < num2)
						{
							num2 = num6;
							swarmerTarget2 = current;
							continue;
						}
						continue;
					}
				}
				if (sac.Type == SwarmerAttackerType.GAURDIAN)
				{
					if (num5 <= 0)
					{
						continue;
					}
				}
				else
				{
					if (num4 <= 0)
					{
						continue;
					}
				}
				if (num6 < num)
				{
					num = num6;
					swarmerTarget = current;
				}
			}
			if (swarmerTarget2 != null)
			{
				if (this.m_AttackingPlanetSwarmers.Contains(sac.GetShip().ObjectID) || this.m_AttackingPlanetGardians.Contains(sac.GetShip().ObjectID))
				{
					swarmerTarget = swarmerTarget2;
				}
				else
				{
					int num7 = (sac.Type == SwarmerAttackerType.GAURDIAN) ? (this.m_NumGardiansToAttackPlanet - this.m_AttackingPlanetGardians.Count) : (this.m_NumSwarmersToAttackPlanet - this.m_AttackingPlanetSwarmers.Count);
					if (num7 > 0)
					{
						swarmerTarget = swarmerTarget2;
					}
				}
			}
			if (swarmerTarget != null || swarmerTarget3 != null)
			{
				if (sac.Type == SwarmerAttackerType.GAURDIAN)
				{
					if (swarmerTarget != null)
					{
						swarmerTarget.IncGuardiansOnTarget();
					}
					else
					{
						swarmerTarget3.IncGuardiansOnTarget();
					}
				}
				else
				{
					if (swarmerTarget != null)
					{
						swarmerTarget.IncSwarmersOnTarget();
					}
					else
					{
						swarmerTarget3.IncSwarmersOnTarget();
					}
				}
				if (swarmerTarget == null)
				{
					sac.SetTarget(swarmerTarget3.Target);
					return;
				}
				sac.SetTarget(swarmerTarget.Target);
				if (swarmerTarget.Target is StellarBody && !this.m_AttackingPlanetSwarmers.Contains(sac.GetShip().ObjectID) && !this.m_AttackingPlanetGardians.Contains(sac.GetShip().ObjectID))
				{
					if (sac.Type == SwarmerAttackerType.GAURDIAN)
					{
						this.m_AttackingPlanetGardians.Add(sac.GetShip().ObjectID);
						return;
					}
					this.m_AttackingPlanetSwarmers.Add(sac.GetShip().ObjectID);
					return;
				}
			}
			else
			{
				sac.SetTarget(null);
			}
		}
		protected void MaintainMaxSwarmers()
		{
			if (this.ReadyToEmitFighters() && this.m_MaxToCreate > 0)
			{
				int num = this.m_LoadingSwarmers.Count + this.m_LoadingGuardians.Count;
				int num2 = this.m_SpawnedSwarmers.Count + this.m_SpawnedGuardians.Count;
				if (num + num2 < this.m_MaxSwarmers + this.m_MaxGuardians)
				{
					this.m_State = SwarmerSpawnerStates.EMITSWARMER;
				}
			}
		}
		protected void UpdateTargetList()
		{
			if (!this.m_UpdateTargetList)
			{
				this.m_UpdateListDelay--;
				if (this.m_UpdateListDelay <= 0)
				{
					this.m_UpdateListDelay = 30;
					this.m_UpdateTargetList = true;
				}
			}
		}
		public override bool NeedsAParent()
		{
			return false;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		protected virtual void ThinkIdle()
		{
		}
		protected virtual void ThinkEmitAttackSwarmer()
		{
			if (this.m_MaxToCreate > 0 && this.m_Game.Game.ScriptModules.Swarmers != null)
			{
				this.SpawnMaxSwarmerAttackers(this.m_Game.Game.ScriptModules.Swarmers);
				return;
			}
			this.m_State = SwarmerSpawnerStates.IDLE;
		}
		protected virtual void ThinkIntegrateAttackSwarmer()
		{
			if (this.m_LoadingSwarmers.Count + this.m_LoadingGuardians.Count == 0)
			{
				this.m_State = SwarmerSpawnerStates.IDLE;
				return;
			}
			bool flag = true;
			foreach (Ship current in this.m_LoadingSwarmers)
			{
				if (current.ObjectStatus != GameObjectStatus.Ready)
				{
					flag = false;
				}
			}
			foreach (Ship current2 in this.m_LoadingGuardians)
			{
				if (current2.ObjectStatus != GameObjectStatus.Ready)
				{
					flag = false;
				}
			}
			if (flag)
			{
				foreach (Ship current3 in this.m_LoadingSwarmers)
				{
					current3.Player = this.m_SwarmerSpawner.Player;
					current3.Active = true;
					this.m_Game.CurrentState.AddGameObject(current3, false);
				}
				foreach (Ship current4 in this.m_LoadingGuardians)
				{
					current4.Player = this.m_SwarmerSpawner.Player;
					current4.Active = true;
					this.m_Game.CurrentState.AddGameObject(current4, false);
				}
				this.m_State = SwarmerSpawnerStates.ADDINGSWARMERS;
			}
		}
		protected virtual void ThinkAddingSwarmers()
		{
			if (this.m_LoadingSwarmers.Count + this.m_LoadingGuardians.Count == 0)
			{
				if (this.m_TargetList.Count == 0)
				{
					this.m_State = SwarmerSpawnerStates.IDLE;
					return;
				}
				this.m_State = SwarmerSpawnerStates.LAUNCHSWARMER;
			}
		}
		protected virtual void ThinkLaunch()
		{
			if (this.m_LaunchingShips.Count == 0)
			{
				this.m_State = SwarmerSpawnerStates.IDLE;
				return;
			}
			bool flag = true;
			foreach (Ship current in this.m_LaunchingShips)
			{
				if (!current.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				this.m_SwarmerSpawner.PostSetProp("LaunchBattleriders", new object[0]);
				this.m_State = SwarmerSpawnerStates.WAITFORLAUNCH;
			}
		}
		protected virtual void ThinkWaitForLaunch()
		{
			List<Ship> list = new List<Ship>();
			foreach (Ship current in this.m_LaunchingShips)
			{
				if (!current.DockedWithParent)
				{
					list.Add(current);
				}
			}
			foreach (Ship current2 in list)
			{
				this.m_LaunchingShips.Remove(current2);
			}
			if (this.m_LaunchingShips.Count == 0)
			{
				this.m_State = SwarmerSpawnerStates.IDLE;
			}
		}
		public bool ReadyToEmitFighters()
		{
			switch (this.m_State)
			{
			case SwarmerSpawnerStates.EMITSWARMER:
			case SwarmerSpawnerStates.INTEGRATESWARMER:
			case SwarmerSpawnerStates.ADDINGSWARMERS:
			case SwarmerSpawnerStates.LAUNCHSWARMER:
			case SwarmerSpawnerStates.WAITFORLAUNCH:
				return false;
			default:
				return true;
			}
		}
	}
}
