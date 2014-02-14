using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.Combat
{
	internal class BioMissileLaunchControl : BattleRiderLaunchControl
	{
		public BioMissileLaunchControl(App game, CombatAI commanderAI, Ship ship) : base(game, commanderAI, ship, WeaponEnums.TurretClasses.Biomissile)
		{
		}
		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if (this.m_Ship.Target != null && this.m_Ship.Target is StellarBody && base.CarrierCanLaunch() && (this.m_Ship.Maneuvering.Position - (this.m_Ship.Target as StellarBody).Parameters.Position).LengthSquared < this.m_MinAttackDist * this.m_MinAttackDist)
			{
				base.LaunchRiders(this.m_Ship.Target);
			}
		}
	}
}
