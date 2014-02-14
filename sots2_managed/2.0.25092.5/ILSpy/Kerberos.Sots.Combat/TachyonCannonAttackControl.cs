using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class TachyonCannonAttackControl : PositionalAttackControl
	{
		private int m_EnemyGroupStrngth = 27;
		private float m_MaxRange;
		public TachyonCannonAttackControl(App game, CombatAI commanderAI, Ship ship, int weaponID, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponID, weaponType)
		{
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault((WeaponBank x) => x.Weapon.UniqueWeaponID == weaponID);
			if (weaponBank != null)
			{
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
			float num = this.m_MaxRange * 0.75f;
			Vector3 v = desiredTargetPosition - this.m_Ship.Position;
			v.Y = 0f;
			float num2 = v.Normalize();
			if (num2 > num)
			{
				return false;
			}
			bool flag = false;
			foreach (Ship current in this.m_CommanderAI.GetFriendlyShips())
			{
				if (Ship.IsActiveShip(current))
				{
					Vector3 v2 = current.Position - this.m_Ship.Position;
					v2.Y = 0f;
					float num3 = v2.Normalize();
					if (num3 <= this.m_MaxRange && Vector3.Dot(v2, v) > 0.9f)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				return false;
			}
			if (CombatAI.AssessGroupStrength(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships) < this.m_EnemyGroupStrngth)
			{
				return true;
			}
			List<Ship> list = new List<Ship>();
			foreach (Ship current2 in this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships)
			{
				Vector3 v3 = current2.Position - this.m_Ship.Position;
				v3.Y = 0f;
				float num4 = v3.Normalize();
				if (num4 <= this.m_MaxRange && Vector3.Dot(v3, v) > 0.9f)
				{
					list.Add(current2);
				}
			}
			return CombatAI.AssessGroupStrength(this.m_Ship.TaskGroup.Objective.m_TargetEnemyGroup.m_Ships) >= this.m_EnemyGroupStrngth;
		}
		protected override Vector3 GetDesiredTargetPosition(EnemyGroup enemy)
		{
			Vector3 v = enemy.m_LastKnownPosition;
			if ((enemy.m_LastKnownDestination - enemy.m_LastKnownPosition).LengthSquared < (enemy.m_LastKnownPosition - v).LengthSquared)
			{
				v = enemy.m_LastKnownDestination;
			}
			return this.m_Ship.Maneuvering.Position + Vector3.Normalize(this.m_Ship.Maneuvering.Position - v) * this.m_MaxRange;
		}
	}
}
