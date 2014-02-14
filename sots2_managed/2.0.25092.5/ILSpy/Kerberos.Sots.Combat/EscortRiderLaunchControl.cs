using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.Combat
{
	internal class EscortRiderLaunchControl : BattleRiderLaunchControl
	{
		public EscortRiderLaunchControl(App game, CombatAI commanderAI, Ship ship, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponType)
		{
			this.m_CurrMaxLaunchDelay = BattleRiderLaunchControl.kMinRiderHoldDuration * 5;
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}
		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if (this.m_Ship.Target != null && this.m_Ship.Target is Ship && base.CarrierCanLaunch())
			{
				if ((this.m_Ship.Maneuvering.Position - (this.m_Ship.Target as Ship).Maneuvering.Position).LengthSquared < this.m_MinAttackDist * this.m_MinAttackDist)
				{
					this.m_LaunchDelay -= framesElapsed;
					if (this.m_LaunchDelay <= 0)
					{
						base.LaunchRiders(this.m_Ship.Target);
						return;
					}
				}
			}
			else
			{
				this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
			}
		}
	}
}
