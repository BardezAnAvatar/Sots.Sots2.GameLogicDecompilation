using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.Combat
{
	internal class SystemDefenseBoatControl : SpecWeaponControl
	{
		private Vector3 m_Pos;
		private float m_MinRange;
		public SystemDefenseBoatControl(App game, CombatAI commanderAI, Ship ship, StellarBody planet) : base(game, commanderAI, ship, WeaponEnums.TurretClasses.Standard)
		{
			this.m_MinRange = 10000f;
			if (planet != null)
			{
				this.m_MinRange += planet.Parameters.Radius;
				this.m_Pos = planet.Parameters.Position;
				return;
			}
			this.m_Pos = ship.Position;
		}
		public override bool RemoveWeaponControl()
		{
			return this.m_Ship == null || this.m_Ship.DefenseBoatActive;
		}
		public override void Update(int framesElapsed)
		{
			bool flag = false;
			foreach (EnemyGroup current in this.m_CommanderAI.GetEnemyGroups())
			{
				Ship closestShip = current.GetClosestShip(this.m_Pos, this.m_MinRange);
				if (closestShip != null)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				this.m_Ship.DefenseBoatActive = true;
			}
		}
	}
}
