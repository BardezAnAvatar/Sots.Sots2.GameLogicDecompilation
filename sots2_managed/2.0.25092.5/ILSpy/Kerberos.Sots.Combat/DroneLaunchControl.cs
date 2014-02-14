using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class DroneLaunchControl : BattleRiderLaunchControl
	{
		public DroneLaunchControl(App game, CombatAI commanderAI, Ship ship) : base(game, commanderAI, ship, WeaponEnums.TurretClasses.Drone)
		{
			this.m_CurrMaxLaunchDelay = commanderAI.AIRandom.NextInclusive(BattleRiderLaunchControl.kMinRiderHoldDuration, BattleRiderLaunchControl.kMinRiderHoldDuration * 3);
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}
		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if ((this.m_Ship.Target == null || !(this.m_Ship.Target is Ship)) && this.m_CommanderAI.GetEnemyGroups().Count<EnemyGroup>() <= 0)
			{
				if (!base.CarrierCanLaunch())
				{
					base.RecoverRiders();
					return;
				}
			}
			else
			{
				if (base.CarrierCanLaunch())
				{
					IGameObject gameObject = (this.m_Ship.Target != null) ? this.m_Ship.Target : base.ShipTargetInRange();
					if (gameObject != null)
					{
						this.m_LaunchDelay -= framesElapsed;
						if (this.m_LaunchDelay <= 0)
						{
							base.LaunchRiders(gameObject);
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
}
