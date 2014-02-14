using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class CloakedShipDetection
	{
		private static int kCloakTimeoutFrames = 600;
		private Dictionary<Ship, int> m_EnemyShips;
		public CloakedShipDetection()
		{
			this.m_EnemyShips = new Dictionary<Ship, int>();
		}
		public void AddShip(Ship ship)
		{
			if (!this.m_EnemyShips.ContainsKey(ship))
			{
				this.m_EnemyShips.Add(ship, CloakedShipDetection.kCloakTimeoutFrames);
			}
		}
		public void RemoveShip(Ship ship)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
			{
				this.m_EnemyShips.Remove(ship);
			}
		}
		public void ShipSpotted(Ship ship)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
			{
				this.m_EnemyShips[ship] = CloakedShipDetection.kCloakTimeoutFrames;
			}
		}
		public float GetVisibilityPercent(Ship ship)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
			{
				return (float)this.m_EnemyShips[ship] / (float)CloakedShipDetection.kCloakTimeoutFrames;
			}
			return 1f;
		}
		public void UpdateCloakedDetection(int elapsedFrames)
		{
			if (this.m_EnemyShips.Count == 0)
			{
				return;
			}
			List<Ship> list = this.m_EnemyShips.Keys.ToList<Ship>();
			foreach (Ship current in list)
			{
				if (current.CloakedState == CloakedState.Cloaking)
				{
					this.m_EnemyShips[current] = Math.Max(this.m_EnemyShips[current] - elapsedFrames, 0);
				}
				else
				{
					this.m_EnemyShips[current] = CloakedShipDetection.kCloakTimeoutFrames;
				}
			}
		}
	}
}
