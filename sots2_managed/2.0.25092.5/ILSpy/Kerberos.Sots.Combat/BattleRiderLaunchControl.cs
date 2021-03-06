using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class BattleRiderLaunchControl : SpecWeaponControl
	{
		protected static int kMinRiderHoldDuration = 60;
		protected List<Ship> m_Riders;
		protected List<Ship> m_LaunchingRiders;
		protected List<Ship> m_LaunchedRiders;
		protected int m_LaunchDelay;
		protected int m_CurrMaxLaunchDelay;
		protected float m_MinAttackDist;
		protected bool m_HasLaunchedBefore;
		private int m_RidersAdded;
		private int m_RidersRemoved;
		private int m_RemoveDelay;
		public BattleRiderLaunchControl(App game, CombatAI commanderAI, Ship ship, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponType)
		{
			this.m_Riders = new List<Ship>();
			this.m_LaunchingRiders = new List<Ship>();
			this.m_LaunchedRiders = new List<Ship>();
			this.m_LaunchDelay = 0;
			this.m_CurrMaxLaunchDelay = 0;
			this.m_MinAttackDist = 0f;
			this.m_HasLaunchedBefore = false;
			this.m_RidersAdded = 0;
			this.m_RidersRemoved = 0;
			this.m_RemoveDelay = 120;
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj is Ship)
			{
				this.RemoveRider(obj as Ship);
			}
			base.ObjectRemoved(obj);
		}
		public override bool RemoveWeaponControl()
		{
			return (this.m_RidersRemoved > 0 && this.m_RidersRemoved == this.m_RidersAdded) || (this.m_RemoveDelay <= 0 && this.m_RidersAdded == 0) || base.RemoveWeaponControl();
		}
		public override void Update(int framesElapsed)
		{
			this.UpdateRiderLists();
			this.UpdateBattleRiderWeaponControl(framesElapsed);
			if (this.m_RidersAdded == 0)
			{
				this.m_RemoveDelay -= framesElapsed;
			}
		}
		public virtual void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
		}
		public void SetMinAttackRange(float range)
		{
			this.m_MinAttackDist = range;
		}
		public float GetMinAttackRange()
		{
			return this.m_MinAttackDist;
		}
		public void LaunchRiders(IGameObject target)
		{
			int targetId = (target != null) ? target.ObjectID : 0;
			foreach (Ship current in this.m_Riders)
			{
				current.SetShipTarget(targetId, Vector3.Zero, true, 0);
				if (!this.m_LaunchedRiders.Contains(current) && !this.m_LaunchingRiders.Contains(current))
				{
					this.m_LaunchingRiders.Add(current);
				}
			}
			this.m_Ship.PostSetProp("LaunchBattleriders", new object[0]);
			this.m_HasLaunchedBefore = true;
		}
		public void RecoverRiders()
		{
			this.m_Ship.PostSetProp("RecoverBattleriders", new object[0]);
			this.m_LaunchingRiders.Clear();
		}
		public void UpdateRiderLists()
		{
			List<Ship> list = new List<Ship>();
			foreach (Ship current in this.m_LaunchedRiders)
			{
				if (current.DockedWithParent)
				{
					list.Add(current);
				}
			}
			bool flag = this.m_LaunchedRiders.Count > 0;
			foreach (Ship current2 in list)
			{
				this.m_LaunchedRiders.Remove(current2);
			}
			if (flag && this.m_LaunchedRiders.Count == 0 && this.m_Ship.TaskGroup != null)
			{
				this.m_Ship.TaskGroup.NotifyAllRidersDocked(this.m_Ship);
			}
			list.Clear();
			foreach (Ship current3 in this.m_LaunchingRiders)
			{
				if (!current3.DockedWithParent)
				{
					list.Add(current3);
				}
			}
			bool flag2 = this.m_LaunchingRiders.Count > 0;
			foreach (Ship current4 in list)
			{
				this.m_LaunchingRiders.Remove(current4);
				if (!this.m_LaunchedRiders.Contains(current4))
				{
					this.m_LaunchedRiders.Add(current4);
				}
			}
			if (flag2 && this.m_LaunchingRiders.Count == 0 && this.m_Ship.TaskGroup != null)
			{
				this.m_Ship.TaskGroup.NotifyAllRidersDeployed(this.m_Ship);
			}
		}
		public void AddRiders(List<Ship> riders)
		{
			foreach (Ship current in riders)
			{
				this.AddRider(current);
			}
		}
		public void AddRider(Ship rider)
		{
			if (!this.m_Riders.Contains(rider))
			{
				this.m_Riders.Add(rider);
			}
			if (rider.Deployed && !this.m_LaunchedRiders.Contains(rider))
			{
				this.m_LaunchedRiders.Add(rider);
			}
			if (this.m_MinAttackDist <= 0f)
			{
				this.m_MinAttackDist = rider.MissionSection.ShipSectionAsset.MissionTime * 0.5f * rider.Maneuvering.MaxShipSpeed;
			}
			this.m_RidersAdded++;
		}
		public void RemoveRider(Ship rider)
		{
			this.m_Riders.Remove(rider);
			this.m_LaunchedRiders.Remove(rider);
			this.m_LaunchingRiders.Remove(rider);
			this.m_RidersRemoved++;
		}
		public bool CarrierCanLaunch()
		{
			return this.m_Riders.Count > 0 && this.m_Ship != null && this.m_Ship.CarrierCanLaunch && !this.m_DisableWeaponFire && this.m_Ship.Maneuvering.SpeedState != ShipSpeedState.Overthrust && this.m_Ship.CombatStance != CombatStance.RETREAT && this.m_Ship.CloakedState == CloakedState.None && this.m_LaunchingRiders.Count == 0 && this.m_LaunchedRiders.Count == 0;
		}
		public bool AllRidersAreLaunched()
		{
			return this.m_LaunchedRiders.Count == this.m_Riders.Count;
		}
		public Ship ShipTargetInRange()
		{
			float num = 3.40282347E+38f;
			Ship result = null;
			foreach (EnemyGroup current in this.m_CommanderAI.GetEnemyGroups())
			{
				Ship closestShip = current.GetClosestShip(this.m_Ship.Maneuvering.Position, this.m_MinAttackDist);
				if (closestShip != null)
				{
					float lengthSquared = (closestShip.Maneuvering.Position - this.m_Ship.Maneuvering.Position).LengthSquared;
					if (lengthSquared < num)
					{
						num = lengthSquared;
						result = closestShip;
					}
				}
			}
			return result;
		}
		public StellarBody PlanetTargetInRange()
		{
			return this.m_CommanderAI.GetClosestEnemyPlanet(this.m_Ship.Maneuvering.Position, this.m_MinAttackDist);
		}
	}
}
