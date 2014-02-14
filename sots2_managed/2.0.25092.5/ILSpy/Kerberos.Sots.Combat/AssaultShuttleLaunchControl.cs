using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.Combat
{
	internal class AssaultShuttleLaunchControl : BattleRiderLaunchControl
	{
		public bool m_AutoDisabled = true;
		public AssaultShuttleLaunchControl(App game, CombatAI commanderAI, Ship ship) : base(game, commanderAI, ship, WeaponEnums.TurretClasses.AssaultShuttle)
		{
			this.m_CurrMaxLaunchDelay = BattleRiderLaunchControl.kMinRiderHoldDuration;
			this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
			this.m_AutoDisabled = true;
			if (this.m_Ship != null)
			{
				this.m_Ship.PostSetProp("SetDisableAutoLaunching", true);
			}
		}
		public override void UpdateBattleRiderWeaponControl(int framesElapsed)
		{
			if (this.m_Ship == null)
			{
				return;
			}
			StellarBody stellarBody = (this.m_Ship.Target is StellarBody) ? (this.m_Ship.Target as StellarBody) : base.PlanetTargetInRange();
			if (stellarBody == null)
			{
				if (!base.CarrierCanLaunch())
				{
					if (!this.m_AutoDisabled)
					{
						this.m_Ship.PostSetProp("SetDisableAutoLaunching", true);
						this.m_AutoDisabled = true;
					}
					base.RecoverRiders();
					return;
				}
			}
			else
			{
				if (base.CarrierCanLaunch() && AssaultShuttleLaunchControl.TargetInRange(stellarBody, this.m_Ship.Position, this.m_MinAttackDist))
				{
					if (this.m_AutoDisabled)
					{
						this.m_Ship.PostSetProp("SetDisableAutoLaunching", false);
						this.m_AutoDisabled = false;
					}
					this.m_LaunchDelay -= framesElapsed;
					if (this.m_LaunchDelay <= 0)
					{
						base.LaunchRiders(stellarBody);
						return;
					}
				}
				else
				{
					this.m_LaunchDelay = this.m_CurrMaxLaunchDelay;
				}
			}
		}
		private static bool TargetInRange(StellarBody body, Vector3 pos, float range)
		{
			if (body == null)
			{
				return false;
			}
			float num = body.Parameters.Radius + 750f + range;
			return (body.Parameters.Position - pos).LengthSquared < num * num;
		}
	}
}
