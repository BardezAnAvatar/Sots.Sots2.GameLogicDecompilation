using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class AttackRiderLaunchControl : BattleRiderLaunchControl
	{
		public AttackRiderLaunchControl(App game, CombatAI commanderAI, Ship ship, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponType)
		{
			this.m_CurrMaxLaunchDelay = BattleRiderLaunchControl.kMinRiderHoldDuration * 5;
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
		}
		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			bool flag = (this.m_Ship.Target != null && this.m_Ship.Target is Ship) || this.m_CommanderAI.GetEnemyGroups().Count<EnemyGroup>() > 0;
			if (flag && base.CarrierCanLaunch())
			{
				IGameObject gameObject = (this.m_Ship.Target != null) ? this.m_Ship.Target : base.ShipTargetInRange();
				if (gameObject != null)
				{
					this.m_LaunchDelay -= framesElapsed;
					if (this.m_LaunchDelay <= 0 || !this.m_HasLaunchedBefore)
					{
						base.LaunchRiders(gameObject);
						return;
					}
				}
			}
			else
			{
				if (!base.AllRidersAreLaunched())
				{
					this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
				}
			}
		}
	}
}
