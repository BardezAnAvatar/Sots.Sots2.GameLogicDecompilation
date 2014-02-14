using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class WraithAbductorAssaultControl : SpecWeaponControl
	{
		private StellarBody m_TargetPlanet;
		private int m_LaunchDelay;
		private int m_CurrMaxLaunchDelay;
		private float m_MinAttackDist;
		private bool m_Assaulting;
		public WraithAbductorAssaultControl(App game, CombatAI commanderAI, Ship ship) : base(game, commanderAI, ship, WeaponEnums.TurretClasses.AssaultShuttle)
		{
			this.m_Ship = ship;
			this.m_CurrMaxLaunchDelay = 300;
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
			this.m_MinAttackDist = ship.MissionSection.ShipSectionAsset.MissionTime * ship.Maneuvering.MaxShipSpeed * 0.5f;
			this.m_Assaulting = false;
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj == this.m_TargetPlanet)
			{
				this.m_TargetPlanet = null;
			}
			base.ObjectRemoved(obj);
		}
		public override bool RemoveWeaponControl()
		{
			return this.m_Ship == null;
		}
		public override void Update(int framesElapsed)
		{
			StellarBody closestEnemyPlanet = this.m_CommanderAI.GetClosestEnemyPlanet(this.m_Ship.Maneuvering.Position, this.m_MinAttackDist);
			if (closestEnemyPlanet != null)
			{
				if (!this.m_Ship.AssaultingPlanet)
				{
					this.m_LaunchDelay -= framesElapsed;
					if (this.m_LaunchDelay <= 0)
					{
						this.m_Ship.PostSetProp("LaunchBattleriders", new object[0]);
					}
				}
				else
				{
					this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
				}
			}
			else
			{
				this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
				if (this.m_Ship.AssaultingPlanet)
				{
					this.m_Ship.PostSetProp("RecoverBattleriders", new object[0]);
				}
			}
			if (this.m_CommanderAI.GetAIType() == OverallAIType.SLAVER && this.m_Assaulting != this.m_Ship.AssaultingPlanet && this.m_Assaulting)
			{
				this.m_Ship.SetCombatStance(CombatStance.RETREAT);
				this.m_Ship.TaskGroup = null;
			}
			this.m_Assaulting = this.m_Ship.AssaultingPlanet;
		}
		public bool CanAssaultPlanet()
		{
			return this.m_Ship.Maneuvering.SpeedState != ShipSpeedState.Overthrust && this.m_Ship.CombatStance != CombatStance.RETREAT && this.m_Ship.Target != null;
		}
	}
}
