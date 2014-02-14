using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class CombatZoneEnemySpotted
	{
		private Dictionary<Ship, bool> m_EnemyShips;
		private Kerberos.Sots.GameStates.StarSystem m_StarSystem;
		public CombatZoneEnemySpotted(Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			this.m_EnemyShips = new Dictionary<Ship, bool>();
			this.m_StarSystem = starSystem;
		}
		public void AddShip(Ship ship, bool seenByDefault = false)
		{
			if (!this.m_EnemyShips.ContainsKey(ship))
			{
				this.m_EnemyShips.Add(ship, seenByDefault);
			}
		}
		public void RemoveShip(Ship ship)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
			{
				this.m_EnemyShips.Remove(ship);
			}
		}
		public void SetEnemySpotted(Ship ship)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
			{
				this.m_EnemyShips[ship] = true;
			}
		}
		public bool IsShipSpotted(Ship ship)
		{
			return this.m_EnemyShips.ContainsKey(ship) && this.m_EnemyShips[ship];
		}
		public void UpdateSpottedShipsInZone(CombatZonePositionInfo cz)
		{
			if (this.m_StarSystem == null || cz == null || cz.Player == 0)
			{
				return;
			}
			int combatZoneIndex = Kerberos.Sots.GameStates.StarSystem.GetCombatZoneIndex(cz.RingIndex, cz.ZoneIndex);
			List<Ship> list = new List<Ship>();
			foreach (KeyValuePair<Ship, bool> current in this.m_EnemyShips)
			{
				if (current.Key.Player != null && current.Key.Player.ID == cz.Player && !current.Value && combatZoneIndex == this.m_StarSystem.GetCombatZoneIndexAtPosition(current.Key.Maneuvering.Position))
				{
					list.Add(current.Key);
				}
			}
			foreach (Ship current2 in list)
			{
				this.m_EnemyShips[current2] = true;
			}
		}
		public List<Ship> GetDetectedShips()
		{
			List<Ship> list = new List<Ship>();
			foreach (KeyValuePair<Ship, bool> current in this.m_EnemyShips)
			{
				if (current.Value)
				{
					list.Add(current.Key);
				}
			}
			return list;
		}
		public List<Vector3> GetAllEntryPoints()
		{
			List<Vector3> list = new List<Vector3>();
			if (this.m_StarSystem != null)
			{
				CombatZonePositionInfo combatZonePositionInfo = this.m_StarSystem.GetCombatZonePositionInfo(this.m_StarSystem.GetFurthestRing() - 1, 0);
				float s = (combatZonePositionInfo != null) ? combatZonePositionInfo.RadiusUpper : ((this.m_StarSystem.GetBaseOffset() + Kerberos.Sots.GameStates.StarSystem.CombatZoneMapRadii.Last<float>()) * 5700f);
				foreach (NeighboringSystemInfo current in this.m_StarSystem.NeighboringSystems)
				{
					Vector3 pos = current.DirFromSystem * s;
					if (!list.Any((Vector3 x) => (x - pos).LengthSquared < 10000f))
					{
						list.Add(pos);
					}
				}
			}
			return list;
		}
	}
}
