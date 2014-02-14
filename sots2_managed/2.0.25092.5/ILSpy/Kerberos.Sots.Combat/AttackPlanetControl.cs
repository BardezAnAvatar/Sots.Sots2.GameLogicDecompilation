using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class AttackPlanetControl : SpecWeaponControl
	{
		private int m_WeaponID;
		private StellarBody m_TargetPlanet;
		public AttackPlanetControl(App game, CombatAI commanderAI, Ship ship, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponType)
		{
			this.m_TargetPlanet = null;
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault((WeaponBank x) => x.TurretClass == weaponType);
			if (weaponBank != null)
			{
				this.m_WeaponID = weaponBank.Weapon.UniqueWeaponID;
			}
		}
		public override void Update(int framesElapsed)
		{
			if (this.m_Ship.Target != null && this.m_Ship.Target is StellarBody && !this.m_DisableWeaponFire)
			{
				if (this.m_TargetPlanet != this.m_Ship.Target)
				{
					this.m_TargetPlanet = (this.m_Ship.Target as StellarBody);
					this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, true);
					this.m_Ship.SetShipSpecWeaponTarget(this.m_WeaponID, this.m_TargetPlanet.ObjectID, Vector3.Zero);
					return;
				}
			}
			else
			{
				this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, false);
				this.m_TargetPlanet = null;
			}
		}
	}
}
