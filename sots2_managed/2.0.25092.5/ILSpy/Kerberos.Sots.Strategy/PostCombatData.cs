using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal class PostCombatData
	{
		public int? SystemId;
		public List<string> AdditionalInfo = new List<string>();
		public List<FleetInfo> FleetsInCombat = new List<FleetInfo>();
		public Dictionary<ShipInfo, int> FleetDamageTable = new Dictionary<ShipInfo, int>();
		public Dictionary<int, Dictionary<int, float>> WeaponDamageTable = new Dictionary<int, Dictionary<int, float>>();
		public List<DestroyedShip> DestroyedShips = new List<DestroyedShip>();
		public List<int> PlayersInCombat = new List<int>();
	}
}
