using Kerberos.Sots.Data;
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
	internal class LocustNestControl : CombatAIController
	{
		private App m_Game;
		protected Ship m_LocustNest;
		private List<Ship> m_LoadingFighters;
		private List<Ship> m_LaunchingFighters;
		protected List<StellarBody> m_Planets;
		private List<LocustFighterControl> m_SpawnedFighters;
		protected List<LocustTarget> m_TargetList;
		private IGameObject m_Target;
		private LocustNestStates m_State;
		private int m_FleetID;
		private int m_LocustSwarmID;
		private int m_MaxNumFighters;
		private int m_MaxNumFightersTotal;
		private int m_NumDestroyedDrones;
		private int m_NumLocustsReachedPlanet;
		protected int m_UpdateListDelay;
		protected bool m_UpdateTargetList;
		public LocustNestStates State
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
			return this.m_LocustNest;
		}
		public override void SetTarget(IGameObject target)
		{
			this.m_Target = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public LocustNestControl(App game, Ship ship, int fleetId)
		{
			this.m_Game = game;
			this.m_LocustNest = ship;
			this.m_TargetList = new List<LocustTarget>();
			this.m_FleetID = fleetId;
		}
		public override void Initialize()
		{
			this.m_LoadingFighters = new List<Ship>();
			this.m_LaunchingFighters = new List<Ship>();
			this.m_SpawnedFighters = new List<LocustFighterControl>();
			this.m_Planets = new List<StellarBody>();
			this.m_State = LocustNestStates.SEEK;
			this.m_Target = null;
			this.m_MaxNumFighters = ((this is LocustMoonControl) ? this.m_Game.AssetDatabase.GlobalLocustData.MaxMoonCombatDrones : this.m_Game.AssetDatabase.GlobalLocustData.MaxCombatDrones);
			this.m_NumDestroyedDrones = 0;
			this.m_UpdateListDelay = 0;
			this.m_UpdateTargetList = false;
			this.m_LocustNest.PostSetProp("SetAsOnlyLaunchCarrier", true);
			this.m_NumLocustsReachedPlanet = this.m_Game.AssetDatabase.GlobalLocustData.NumToLand;
			if (this is LocustMoonControl)
			{
				this.m_LocustNest.Maneuvering.TargetFacingAngle = TargetFacingAngle.BroadSide;
			}
			List<LocustSwarmInfo> source = this.m_Game.GameDatabase.GetLocustSwarmInfos().ToList<LocustSwarmInfo>();
			LocustSwarmInfo locustSwarmInfo = source.FirstOrDefault((LocustSwarmInfo x) => x.FleetId == this.m_FleetID);
			this.m_LocustSwarmID = 0;
			this.m_MaxNumFightersTotal = this.m_Game.AssetDatabase.GlobalLocustData.MaxDrones;
			if (locustSwarmInfo != null)
			{
				this.m_LocustSwarmID = locustSwarmInfo.Id;
				this.m_MaxNumFightersTotal = locustSwarmInfo.NumDrones;
			}
			this.SpawnMaxFighters();
		}
		public override void Terminate()
		{
			LocustSwarmInfo locustSwarmInfo = this.m_Game.GameDatabase.GetLocustSwarmInfo(this.m_LocustSwarmID);
			if (locustSwarmInfo != null)
			{
				locustSwarmInfo.NumDrones = Math.Max(locustSwarmInfo.NumDrones - this.m_NumDestroyedDrones, 0);
				this.m_Game.GameDatabase.UpdateLocustSwarmInfo(locustSwarmInfo);
			}
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (LocustFighterControl current in this.m_SpawnedFighters)
			{
				if (current.GetShip() == obj)
				{
					if (current.GetShip().IsDestroyed)
					{
						this.m_NumDestroyedDrones++;
					}
					this.m_SpawnedFighters.Remove(current);
					break;
				}
			}
			foreach (Ship current2 in this.m_LoadingFighters)
			{
				if (current2 == obj)
				{
					if (current2.IsDestroyed)
					{
						this.m_NumDestroyedDrones++;
					}
					this.m_LoadingFighters.Remove(current2);
					break;
				}
			}
			foreach (Ship current3 in this.m_LaunchingFighters)
			{
				if (current3 == obj)
				{
					if (current3.IsDestroyed)
					{
						this.m_NumDestroyedDrones++;
					}
					this.m_LaunchingFighters.Remove(current3);
					break;
				}
			}
			foreach (LocustTarget current4 in this.m_TargetList)
			{
				if (current4.Target == obj)
				{
					this.m_TargetList.Remove(current4);
					break;
				}
			}
			if (this.m_Target == obj)
			{
				this.m_Target = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_LocustNest == null)
			{
				return;
			}
			this.UpdateTargetList();
			switch (this.m_State)
			{
			case LocustNestStates.SEEK:
				this.ThinkSeek();
				return;
			case LocustNestStates.TRACK:
				this.ThinkTrack();
				return;
			case LocustNestStates.EMITFIGHTER:
				this.ThinkEmitFighter();
				return;
			case LocustNestStates.INTEGRATEFIGHTER:
				this.ThinkIntegrateFighter();
				return;
			case LocustNestStates.WAITFORDOCKED:
				this.ThinkWaitForDocked();
				return;
			case LocustNestStates.LAUNCHFIGHTER:
				this.ThinkLaunch();
				return;
			case LocustNestStates.WAITFORLAUNCH:
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
			this.m_Planets.Clear();
			List<LocustTarget> list = new List<LocustTarget>();
			List<LocustTarget> list2 = new List<LocustTarget>();
			foreach (LocustTarget current in this.m_TargetList)
			{
				current.ClearNumTargets();
			}
			list2.AddRange(this.m_TargetList);
			foreach (IGameObject current2 in objs)
			{
				if (current2 is Ship)
				{
					Ship ship = current2 as Ship;
					if (Ship.IsActiveShip(ship) && ship.Player != this.m_LocustNest.Player)
					{
						bool flag = ship.IsDetected(this.m_LocustNest.Player);
						foreach (LocustFighterControl current3 in this.m_SpawnedFighters)
						{
							if (ship == current3.GetTarget())
							{
								if (!flag)
								{
									current3.SetTarget(null);
									break;
								}
								break;
							}
						}
						if (flag)
						{
							list.Add(new LocustTarget
							{
								Target = ship
							});
							LocustTarget locustTarget = list2.FirstOrDefault((LocustTarget x) => x.Target == ship);
							if (locustTarget != null)
							{
								list2.Remove(locustTarget);
							}
						}
					}
				}
				if (current2 is StellarBody)
				{
					StellarBody stellarBody = current2 as StellarBody;
					ColonyInfo colonyInfoForPlanet = this.m_Game.GameDatabase.GetColonyInfoForPlanet(stellarBody.Parameters.OrbitalID);
					if (colonyInfoForPlanet != null)
					{
						this.m_Planets.Add(stellarBody);
					}
				}
			}
			if (list2.Count > 0)
			{
				foreach (LocustFighterControl fighter in this.m_SpawnedFighters)
				{
					if (list2.Any((LocustTarget x) => x.Target == fighter.GetTarget()))
					{
						fighter.SetTarget(null);
					}
				}
			}
			if (list.Count > this.m_TargetList.Count)
			{
				using (List<LocustTarget>.Enumerator enumerator5 = this.m_TargetList.GetEnumerator())
				{
					while (enumerator5.MoveNext())
					{
						LocustTarget current4 = enumerator5.Current;
						ShipClass shipClass = current4.Target.ShipClass;
						current4.ClearNumTargets();
						int num = LocustFighterControl.NumFightersPerShip(shipClass);
						foreach (LocustFighterControl current5 in this.m_SpawnedFighters)
						{
							if (current5.GetTarget() == current4.Target)
							{
								if (num > 0)
								{
									num--;
									current4.IncFightersOnTarget();
								}
								else
								{
									current5.SetTarget(null);
								}
							}
						}
					}
					goto IL_382;
				}
			}
			foreach (LocustFighterControl fighter in this.m_SpawnedFighters)
			{
				if (fighter.GetTarget() != null)
				{
					LocustTarget locustTarget2 = this.m_TargetList.FirstOrDefault((LocustTarget x) => x.Target == fighter.GetTarget());
					if (locustTarget2 != null)
					{
						locustTarget2.IncFightersOnTarget();
					}
				}
			}
			IL_382:
			foreach (LocustTarget target in list)
			{
				if (!this.m_TargetList.Any((LocustTarget x) => x.Target == target.Target))
				{
					target.ClearNumTargets();
					this.m_TargetList.Add(target);
				}
			}
			this.m_UpdateTargetList = false;
		}
		public override bool NeedsAParent()
		{
			return false;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		public bool IsThisMyNest(Ship ship)
		{
			return this.m_LoadingFighters.Any((Ship x) => x == ship);
		}
		protected virtual void PickTarget()
		{
			IGameObject target = null;
			float num = 3.40282347E+38f;
			foreach (StellarBody current in this.m_Planets)
			{
				float lengthSquared = (current.Parameters.Position - this.m_LocustNest.Maneuvering.Position).LengthSquared;
				if (lengthSquared < num)
				{
					target = current;
					num = lengthSquared;
				}
			}
			this.SetTarget(target);
		}
		public void AddFighter(CombatAIController fighter)
		{
			if (fighter is LocustFighterControl)
			{
				foreach (Ship current in this.m_LoadingFighters)
				{
					if (current == fighter.GetShip())
					{
						this.m_LoadingFighters.Remove(current);
						this.m_LaunchingFighters.Add(current);
						break;
					}
				}
				this.m_SpawnedFighters.Add(fighter as LocustFighterControl);
			}
			if (this.m_LoadingFighters.Count == 0)
			{
				this.m_State = LocustNestStates.WAITFORDOCKED;
			}
		}
		private void ThinkSeek()
		{
			int num = this.m_LoadingFighters.Count + this.m_SpawnedFighters.Count;
			if (num < this.m_MaxNumFighters)
			{
				this.m_State = LocustNestStates.EMITFIGHTER;
				return;
			}
			if (this.m_Target != null)
			{
				this.m_State = LocustNestStates.TRACK;
				return;
			}
			this.PickTarget();
		}
		private void ThinkTrack()
		{
			int num = this.m_LoadingFighters.Count + this.m_SpawnedFighters.Count;
			if (num < this.m_MaxNumFighters)
			{
				this.m_State = LocustNestStates.EMITFIGHTER;
				return;
			}
			if (this.m_Target == null)
			{
				this.m_State = LocustNestStates.SEEK;
				return;
			}
			Vector3 vector = Vector3.Zero;
			if (this.m_Target is StellarBody)
			{
				vector = (this.m_Target as StellarBody).Parameters.Position;
			}
			else
			{
				if (this.m_Target is Ship)
				{
					vector = (this.m_Target as Ship).Position;
				}
			}
			Vector3 look = vector - this.m_LocustNest.Maneuvering.Position;
			look.Y = 0f;
			look.Normalize();
			this.m_LocustNest.Maneuvering.PostAddGoal(vector, look);
			if (this.m_LocustNest.Target != this.m_Target)
			{
				this.m_LocustNest.SetShipTarget(this.m_Target.ObjectID, Vector3.Zero, true, 0);
			}
		}
		private void ThinkEmitFighter()
		{
			this.SpawnMaxFighters();
		}
		private void ThinkIntegrateFighter()
		{
			if (this.m_LoadingFighters.Count == 0)
			{
				this.m_State = LocustNestStates.SEEK;
				return;
			}
			bool flag = true;
			foreach (Ship current in this.m_LoadingFighters)
			{
				if (current.ObjectStatus != GameObjectStatus.Ready)
				{
					flag = false;
				}
			}
			if (flag)
			{
				foreach (Ship current2 in this.m_LoadingFighters)
				{
					current2.Player = this.m_LocustNest.Player;
					current2.Active = true;
					this.m_Game.CurrentState.AddGameObject(current2, false);
				}
				this.m_State = LocustNestStates.SEEK;
			}
		}
		private void ThinkWaitForDocked()
		{
			bool flag = true;
			foreach (Ship current in this.m_LaunchingFighters)
			{
				if (!current.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				this.m_State = LocustNestStates.LAUNCHFIGHTER;
			}
		}
		private void ThinkLaunch()
		{
			if (this.m_LaunchingFighters.Count == 0)
			{
				this.m_State = LocustNestStates.SEEK;
				return;
			}
			this.m_LocustNest.PostSetProp("LaunchBattleriders", new object[0]);
			this.m_State = LocustNestStates.WAITFORLAUNCH;
		}
		private void ThinkWaitForLaunch()
		{
			List<Ship> list = new List<Ship>();
			foreach (Ship current in this.m_LaunchingFighters)
			{
				if (!current.DockedWithParent)
				{
					list.Add(current);
				}
			}
			foreach (Ship current2 in list)
			{
				this.m_LaunchingFighters.Remove(current2);
			}
			if (this.m_LaunchingFighters.Count == 0)
			{
				int num = this.m_LoadingFighters.Count + this.m_SpawnedFighters.Count;
				if (num < this.m_MaxNumFighters)
				{
					this.m_State = LocustNestStates.EMITFIGHTER;
					return;
				}
				this.m_State = LocustNestStates.SEEK;
			}
		}
		public void RequestTargetFromParent(LocustFighterControl lfc)
		{
			if (this.m_TargetList.Count == 0 && !(this.m_Target is StellarBody))
			{
				lfc.SetTarget(this.m_LocustNest);
				return;
			}
			LocustTarget locustTarget = null;
			float num = 3.40282347E+38f;
			foreach (LocustTarget current in this.m_TargetList)
			{
				ShipClass shipClass = current.Target.ShipClass;
				int num2 = LocustFighterControl.NumFightersPerShip(shipClass);
				if (current.FightersOnTarget < num2)
				{
					float lengthSquared = (current.Target.Position - lfc.GetShip().Position).LengthSquared;
					if (lengthSquared < num)
					{
						num = lengthSquared;
						locustTarget = current;
					}
				}
			}
			if (locustTarget != null)
			{
				locustTarget.IncFightersOnTarget();
				lfc.SetTarget(locustTarget.Target);
				return;
			}
			if (this.m_NumLocustsReachedPlanet > 0 && this.m_Target is StellarBody)
			{
				lfc.SetTarget(this.m_Target);
				return;
			}
			lfc.SetTarget(this.m_LocustNest);
		}
		public void NotifyFighterHasLanded()
		{
			this.m_NumLocustsReachedPlanet--;
			if (this.m_NumLocustsReachedPlanet <= 0)
			{
				foreach (LocustFighterControl current in this.m_SpawnedFighters)
				{
					current.ClearPlanetTarget();
				}
			}
		}
		private void SpawnMaxFighters()
		{
			if (this.m_Game.Game.ScriptModules.Locust == null || this.m_MaxNumFightersTotal <= 0)
			{
				return;
			}
			int num = this.m_LoadingFighters.Count + this.m_SpawnedFighters.Count;
			int val = Math.Max(this.m_LocustNest.BattleRiderMounts.Count<BattleRiderMount>() - this.m_LoadingFighters.Count, 0);
			int val2 = Math.Max(Math.Min(this.m_MaxNumFighters, this.m_MaxNumFightersTotal) - num, 0);
			int num2 = Math.Min(val2, val);
			Matrix worldMat = Matrix.CreateRotationYPR(this.m_LocustNest.Maneuvering.Rotation);
			worldMat.Position = this.m_LocustNest.Maneuvering.Position;
			int needleShipDesignId = this.m_Game.Game.ScriptModules.Locust.NeedleShipDesignId;
			for (int i = 0; i < num2; i++)
			{
				Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, needleShipDesignId, this.m_LocustNest.ObjectID, this.m_LocustNest.InputID, this.m_LocustNest.Player.ObjectID);
				if (ship != null)
				{
					this.m_LoadingFighters.Add(ship);
					this.m_MaxNumFightersTotal--;
				}
			}
			if (this.m_LoadingFighters.Count > 0)
			{
				this.m_State = LocustNestStates.INTEGRATEFIGHTER;
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
		private bool FighterRequestingTarget()
		{
			bool result = false;
			foreach (LocustFighterControl current in this.m_SpawnedFighters)
			{
				if (current.RequestingNewTarget())
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
