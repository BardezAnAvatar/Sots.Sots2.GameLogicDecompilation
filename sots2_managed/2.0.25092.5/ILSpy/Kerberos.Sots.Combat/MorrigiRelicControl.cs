using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class MorrigiRelicControl : CombatAIController
	{
		public class TargetingData
		{
			public Ship Target;
			public int NumTargeting;
		}
		private App m_Game;
		private Ship m_MorrigiRelic;
		private List<ShipInfo> m_AvailableCrows;
		private List<Ship> m_LoadingCrows;
		private List<Ship> m_LaunchingCrows;
		private List<MorrigiCrowControl> m_SpawnedCrows;
		private IGameObject m_Target;
		private MorrigiRelicStates m_State;
		private int m_FleetId;
		private bool m_IsAggressive;
		public MorrigiRelicStates State
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
		public static MorrigiRelicGlobalData.RelicType GetMorrigiRelicTypeFromName(string sectionName)
		{
			MorrigiRelicGlobalData.RelicType result = MorrigiRelicGlobalData.RelicType.Pristine1;
			int num = 0;
			if (sectionName.Contains("levelone"))
			{
				num = 1;
			}
			if (sectionName.Contains("leveltwo"))
			{
				num = 2;
			}
			if (sectionName.Contains("levelthree"))
			{
				num = 3;
			}
			if (sectionName.Contains("levelfour"))
			{
				num = 4;
			}
			if (sectionName.Contains("levelfive"))
			{
				num = 5;
			}
			if (sectionName.Contains("pristine"))
			{
				switch (num)
				{
				case 1:
					result = MorrigiRelicGlobalData.RelicType.Pristine1;
					break;
				case 2:
					result = MorrigiRelicGlobalData.RelicType.Pristine2;
					break;
				case 3:
					result = MorrigiRelicGlobalData.RelicType.Pristine3;
					break;
				case 4:
					result = MorrigiRelicGlobalData.RelicType.Pristine4;
					break;
				case 5:
					result = MorrigiRelicGlobalData.RelicType.Pristine5;
					break;
				}
			}
			else
			{
				if (sectionName.Contains("stealth"))
				{
					switch (num)
					{
					case 1:
						result = MorrigiRelicGlobalData.RelicType.Stealth1;
						break;
					case 2:
						result = MorrigiRelicGlobalData.RelicType.Stealth2;
						break;
					case 3:
						result = MorrigiRelicGlobalData.RelicType.Stealth3;
						break;
					case 4:
						result = MorrigiRelicGlobalData.RelicType.Stealth4;
						break;
					case 5:
						result = MorrigiRelicGlobalData.RelicType.Stealth5;
						break;
					}
				}
			}
			return result;
		}
		public override Ship GetShip()
		{
			return this.m_MorrigiRelic;
		}
		public override void SetTarget(IGameObject target)
		{
			this.m_Target = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public MorrigiRelicControl(App game, Ship ship, int fleetId)
		{
			this.m_Game = game;
			this.m_MorrigiRelic = ship;
			this.m_FleetId = fleetId;
		}
		public override void Initialize()
		{
			this.m_LoadingCrows = new List<Ship>();
			this.m_LaunchingCrows = new List<Ship>();
			this.m_SpawnedCrows = new List<MorrigiCrowControl>();
			this.m_State = MorrigiRelicStates.IDLE;
			this.m_Target = null;
			this.m_AvailableCrows = this.m_Game.GameDatabase.GetBattleRidersByParentID(this.m_MorrigiRelic.DatabaseID).ToList<ShipInfo>();
			this.m_MorrigiRelic.PostSetProp("SetAsOnlyLaunchCarrier", true);
			this.m_IsAggressive = false;
			List<MorrigiRelicInfo> source = this.m_Game.GameDatabase.GetMorrigiRelicInfos().ToList<MorrigiRelicInfo>();
			MorrigiRelicInfo morrigiRelicInfo = source.FirstOrDefault((MorrigiRelicInfo x) => x.FleetId == this.m_FleetId);
			if (morrigiRelicInfo != null)
			{
				this.m_IsAggressive = morrigiRelicInfo.IsAggressive;
			}
			if (this.m_IsAggressive)
			{
				this.SpawnMaxCrows();
			}
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (MorrigiCrowControl current in this.m_SpawnedCrows)
			{
				if (current.GetShip() == obj)
				{
					this.m_SpawnedCrows.Remove(current);
					break;
				}
			}
			foreach (Ship current2 in this.m_LoadingCrows)
			{
				if (current2 == obj)
				{
					this.m_LoadingCrows.Remove(current2);
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
			if (this.m_MorrigiRelic == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case MorrigiRelicStates.IDLE:
				this.ThinkIdle();
				return;
			case MorrigiRelicStates.EMITCROW:
				this.ThinkEmitCrow();
				return;
			case MorrigiRelicStates.INTEGRATECROW:
				this.ThinkIntegrateCrow();
				return;
			case MorrigiRelicStates.WAITFORDOCKED:
				this.ThinkWaitForDocked();
				return;
			case MorrigiRelicStates.LAUNCHCROW:
				this.ThinkLaunch();
				return;
			case MorrigiRelicStates.WAITFORLAUNCH:
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
			return this.m_IsAggressive && this.m_AvailableCrows.Count == 0 && this.m_LaunchingCrows.Count == 0 && this.m_LoadingCrows.Count == 0;
		}
		public override bool RequestingNewTarget()
		{
			bool result = false;
			foreach (MorrigiCrowControl current in this.m_SpawnedCrows)
			{
				if (current.RequestingNewTarget())
				{
					result = true;
					break;
				}
			}
			return result;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			List<MorrigiRelicControl.TargetingData> list = new List<MorrigiRelicControl.TargetingData>();
			foreach (IGameObject current in objs)
			{
				if (current is Ship)
				{
					Ship ship = current as Ship;
					if (ship.Active && Ship.IsActiveShip(ship) && ship.Player != this.m_MorrigiRelic.Player)
					{
						bool flag = ship.IsDetected(this.m_MorrigiRelic.Player);
						int num = 0;
						foreach (MorrigiCrowControl current2 in this.m_SpawnedCrows)
						{
							if (ship == current2.GetTarget())
							{
								if (!flag)
								{
									current2.SetTarget(null);
								}
								else
								{
									num++;
								}
							}
						}
						if (flag)
						{
							list.Add(new MorrigiRelicControl.TargetingData
							{
								Target = ship,
								NumTargeting = num
							});
						}
					}
				}
			}
			foreach (MorrigiCrowControl current3 in this.m_SpawnedCrows)
			{
				if (current3.RequestingNewTarget())
				{
					Vector3 position = current3.GetShip().Position;
					float num2 = 3.40282347E+38f;
					int num3 = -1;
					Ship closestTarg = null;
					foreach (MorrigiRelicControl.TargetingData current4 in list)
					{
						float lengthSquared = (current4.Target.Position - position).LengthSquared;
						if (num3 < 0 || current4.NumTargeting < num3 || (num3 == current4.NumTargeting && lengthSquared < num2))
						{
							closestTarg = current4.Target;
							num2 = lengthSquared;
							num3 = current4.NumTargeting;
						}
					}
					if (closestTarg != null)
					{
						current3.SetTarget(closestTarg);
						MorrigiRelicControl.TargetingData targetingData = list.FirstOrDefault((MorrigiRelicControl.TargetingData x) => x.Target == closestTarg);
						if (targetingData != null)
						{
							targetingData.NumTargeting++;
						}
					}
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
		public bool IsThisMyRelic(Ship ship)
		{
			return this.m_LoadingCrows.Any((Ship x) => x == ship);
		}
		public void AddCrow(CombatAIController crow)
		{
			if (crow is MorrigiCrowControl)
			{
				foreach (Ship current in this.m_LoadingCrows)
				{
					if (current == crow.GetShip())
					{
						this.m_LoadingCrows.Remove(current);
						this.m_LaunchingCrows.Add(current);
						break;
					}
				}
				this.m_SpawnedCrows.Add(crow as MorrigiCrowControl);
			}
		}
		private void ThinkIdle()
		{
			if (this.m_LoadingCrows.Count + this.m_SpawnedCrows.Count < this.m_AvailableCrows.Count)
			{
				this.m_State = MorrigiRelicStates.EMITCROW;
			}
		}
		private void ThinkEmitCrow()
		{
			this.SpawnMaxCrows();
		}
		private void ThinkIntegrateCrow()
		{
			if (this.m_LoadingCrows.Count > 0)
			{
				bool flag = true;
				foreach (Ship current in this.m_LoadingCrows)
				{
					if (current.ObjectStatus != GameObjectStatus.Ready)
					{
						flag = false;
					}
				}
				if (flag)
				{
					foreach (Ship current2 in this.m_LoadingCrows)
					{
						current2.Player = this.m_MorrigiRelic.Player;
						current2.Active = true;
						this.m_Game.CurrentState.AddGameObject(current2, false);
					}
					this.m_State = MorrigiRelicStates.WAITFORDOCKED;
				}
			}
		}
		private void ThinkWaitForDocked()
		{
			if (this.m_LoadingCrows.Count > 0)
			{
				return;
			}
			bool flag = true;
			foreach (Ship current in this.m_LaunchingCrows)
			{
				if (!current.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				this.m_State = MorrigiRelicStates.LAUNCHCROW;
			}
		}
		private void ThinkLaunch()
		{
			if (this.m_LaunchingCrows.Count == 0)
			{
				this.m_State = MorrigiRelicStates.IDLE;
				return;
			}
			this.m_MorrigiRelic.PostSetProp("LaunchBattleriders", new object[0]);
			this.m_State = MorrigiRelicStates.WAITFORLAUNCH;
		}
		private void ThinkWaitForLaunch()
		{
			List<Ship> list = new List<Ship>();
			foreach (Ship current in this.m_LaunchingCrows)
			{
				if (!current.DockedWithParent)
				{
					list.Add(current);
				}
			}
			foreach (Ship current2 in list)
			{
				this.m_LaunchingCrows.Remove(current2);
			}
			if (this.m_LaunchingCrows.Count == 0)
			{
				int num = this.m_LoadingCrows.Count + this.m_SpawnedCrows.Count;
				if (num < this.m_AvailableCrows.Count)
				{
					this.m_State = MorrigiRelicStates.EMITCROW;
					return;
				}
				this.m_State = MorrigiRelicStates.IDLE;
			}
		}
		private void SpawnMaxCrows()
		{
			int num = this.m_LoadingCrows.Count + this.m_SpawnedCrows.Count;
			int val = Math.Max(this.m_MorrigiRelic.BattleRiderMounts.Count<BattleRiderMount>() - this.m_LoadingCrows.Count, 0);
			int val2 = Math.Max(this.m_AvailableCrows.Count - num, 0);
			int num2 = Math.Min(val2, val);
			List<ShipInfo> list = new List<ShipInfo>();
			Matrix world = Matrix.CreateRotationYPR(this.m_MorrigiRelic.Maneuvering.Rotation);
			world.Position = this.m_MorrigiRelic.Maneuvering.Position;
			for (int i = 0; i < num2; i++)
			{
				this.m_AvailableCrows[i].RiderIndex = 0;
				Ship ship = Ship.CreateShip(this.m_Game.Game, world, this.m_AvailableCrows[i], this.m_MorrigiRelic.ObjectID, this.m_MorrigiRelic.InputID, this.m_MorrigiRelic.Player.ObjectID, false, null);
				if (ship != null)
				{
					ship.ParentDatabaseID = this.m_AvailableCrows[i].ParentID;
					ship.Maneuvering.RetreatDestination = this.m_MorrigiRelic.Maneuvering.RetreatDestination;
					this.m_LoadingCrows.Add(ship);
				}
				list.Add(this.m_AvailableCrows[i]);
			}
			foreach (ShipInfo current in list)
			{
				this.m_AvailableCrows.Remove(current);
			}
			if (this.m_LoadingCrows.Count > 0)
			{
				this.m_State = MorrigiRelicStates.INTEGRATECROW;
			}
		}
	}
}
