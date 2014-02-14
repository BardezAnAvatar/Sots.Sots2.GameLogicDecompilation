using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.Combat
{
	internal class BoardingPodLaunchControl : BattleRiderLaunchControl
	{
		public BoardingPodLaunchControl(App game, CombatAI commanderAI, Ship ship) : base(game, commanderAI, ship, WeaponEnums.TurretClasses.BoardingPod)
		{
			this.m_CurrMaxLaunchDelay = commanderAI.AIRandom.NextInclusive(BattleRiderLaunchControl.kMinRiderHoldDuration, BattleRiderLaunchControl.kMinRiderHoldDuration * 3);
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}
		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if (base.CarrierCanLaunch())
			{
				if (this.m_Ship.Target != null && this.m_Ship.Target is Ship && (this.m_Ship.Maneuvering.Position - (this.m_Ship.Target as Ship).Maneuvering.Position).LengthSquared < this.m_MinAttackDist * this.m_MinAttackDist)
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
