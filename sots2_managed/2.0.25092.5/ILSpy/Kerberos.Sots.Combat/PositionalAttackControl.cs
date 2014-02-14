using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class PositionalAttackControl : SpecWeaponControl
	{
		private static int kMaxHoldShipDelay = 720;
		private float m_WeaponSpeed;
		private int m_WeaponID;
		private int m_HoldShipDelay;
		private bool m_TargetSet;
		private bool m_DetonatingWeapon;
		private Vector3 m_TargetPosition;
		public int WeaponID
		{
			get
			{
				return this.m_WeaponID;
			}
		}
		public PositionalAttackControl(App game, CombatAI commanderAI, Ship ship, int weaponID, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponType)
		{
			this.m_DetonatingWeapon = false;
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault((WeaponBank x) => x.Weapon.UniqueWeaponID == weaponID);
			if (weaponBank != null)
			{
				this.m_WeaponSpeed = weaponBank.Weapon.Speed;
				this.m_WeaponID = weaponBank.Weapon.UniqueWeaponID;
				this.m_DetonatingWeapon = weaponBank.Weapon.Traits.Contains(WeaponEnums.WeaponTraits.Detonating);
			}
			this.m_TargetSet = false;
			this.m_TargetPosition = Vector3.Zero;
			this.m_HoldShipDelay = 0;
		}
		public override void Update(int framesElapsed)
		{
			if (this.m_DisableWeaponFire)
			{
				return;
			}
			this.m_HoldShipDelay -= framesElapsed;
			this.m_RequestHoldShip = (this.m_HoldShipDelay > 0);
			if (this.m_Ship.TaskGroup == null || !(this.m_Ship.TaskGroup.Objective is AttackGroupObjective) || this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup == null)
			{
				return;
			}
			if (!this.IsReadyToFire())
			{
				this.m_TargetSet = false;
				this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, false);
				this.m_HoldShipDelay = PositionalAttackControl.kMaxHoldShipDelay;
				return;
			}
			Vector3 desiredTargetPosition = this.GetDesiredTargetPosition(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup);
			if (!this.m_TargetSet || (this.m_TargetPosition - desiredTargetPosition).LengthSquared > 250000f)
			{
				this.m_TargetPosition = desiredTargetPosition;
				this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, true);
				this.m_Ship.SetShipPositionalTarget(this.m_WeaponID, desiredTargetPosition, true);
				this.m_TargetSet = true;
				this.m_HoldShipDelay = PositionalAttackControl.kMaxHoldShipDelay;
			}
		}
		protected virtual bool IsReadyToFire()
		{
			return this.m_DetonatingWeapon || this.m_Ship.Maneuvering.Velocity.LengthSquared < 100f;
		}
		protected virtual Vector3 GetDesiredTargetPosition(EnemyGroup enemy)
		{
			Vector3 vector = enemy.m_LastKnownPosition;
			if (this.m_WeaponSpeed > 0f)
			{
				float length = (this.m_Ship.Maneuvering.Position - vector).Length;
				float s = length / this.m_WeaponSpeed;
				vector += enemy.m_LastKnownHeading * s;
				if ((enemy.m_LastKnownDestination - enemy.m_LastKnownPosition).LengthSquared < (enemy.m_LastKnownPosition - vector).LengthSquared)
				{
					vector = enemy.m_LastKnownDestination;
				}
			}
			return vector;
		}
	}
}
