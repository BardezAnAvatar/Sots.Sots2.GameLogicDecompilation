using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class NodeCannonAttackControl : PositionalAttackControl
	{
		private float m_EffectRange;
		private float m_MaxRange;
		public NodeCannonAttackControl(App game, CombatAI commanderAI, Ship ship, int weaponID, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponID, weaponType)
		{
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault((WeaponBank x) => x.Weapon.UniqueWeaponID == weaponID);
			if (weaponBank != null)
			{
				this.m_EffectRange = weaponBank.Weapon.GravityAffectRange;
				this.m_MaxRange = weaponBank.Weapon.Range;
			}
		}
		protected override bool IsReadyToFire()
		{
			if (this.m_Ship.TaskGroup == null || this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup == null || this.m_Ship.Maneuvering.Velocity.LengthSquared > 100f)
			{
				return false;
			}
			Vector3 desiredTargetPosition = this.GetDesiredTargetPosition(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup);
			if ((this.m_Ship.Position - desiredTargetPosition).LengthSquared > this.m_MaxRange * this.m_MaxRange)
			{
				return false;
			}
			bool flag = false;
			foreach (Ship current in this.m_CommanderAI.GetFriendlyShips())
			{
				float num = this.m_EffectRange + current.ShipSphere.radius + 200f;
				if (Ship.IsActiveShip(current) && (current.Position - desiredTargetPosition).LengthSquared < num * num)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return false;
			}
			if (this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships.Count > 3)
			{
				return true;
			}
			int num2 = 0;
			foreach (Ship current2 in this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships)
			{
				if ((current2.Position - desiredTargetPosition).LengthSquared < this.m_EffectRange * this.m_EffectRange)
				{
					if (Ship.IsShipClassBigger(current2.ShipClass, ShipClass.Cruiser, false))
					{
						num2 += 3;
					}
					else
					{
						num2++;
					}
				}
			}
			return num2 > 3;
		}
		protected override Vector3 GetDesiredTargetPosition(EnemyGroup enemy)
		{
			Vector3 vector = enemy.m_LastKnownPosition;
			if ((enemy.m_LastKnownDestination - enemy.m_LastKnownPosition).LengthSquared < (enemy.m_LastKnownPosition - vector).LengthSquared)
			{
				vector = enemy.m_LastKnownDestination;
			}
			Vector3 v = this.m_Ship.Maneuvering.Position - vector;
			float num = v.Normalize();
			float num2 = this.m_EffectRange + this.m_Ship.ShipSphere.radius + 500f;
			if (num < num2)
			{
				vector = this.m_Ship.Maneuvering.Position + v * num2;
			}
			return vector;
		}
	}
}
