using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.Combat
{
	internal class LargeRiderLaunchControl : BattleRiderLaunchControl
	{
		public LargeRiderLaunchControl(App game, CombatAI commanderAI, Ship ship, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponType)
		{
			this.m_LaunchDelay = BattleRiderLaunchControl.kMinRiderHoldDuration;
			this.m_MinAttackDist = game.AssetDatabase.DefaultTacSensorRange;
		}
		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if (this.m_Ship.Target != null && this.m_Ship.Target is Ship && base.CarrierCanLaunch() && (this.m_Ship.Maneuvering.Position - (this.m_Ship.Target as Ship).Maneuvering.Position).LengthSquared < this.m_MinAttackDist * this.m_MinAttackDist)
			{
				this.m_LaunchDelay -= framesElapsed;
				if (this.m_LaunchDelay <= 0)
				{
					base.LaunchRiders(this.m_Ship.Target);
				}
			}
		}
	}
}
