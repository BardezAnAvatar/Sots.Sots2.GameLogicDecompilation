using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class GardenerInfo
	{
		public int Id;
		public int SystemId;
		public int FleetId;
		public int GardenerFleetId;
		public int TurnsToWait;
		public bool IsGardener;
		public int? DeepSpaceSystemId;
		public int? DeepSpaceOrbitalId;
		public Dictionary<int, int> ShipOrbitMap = new Dictionary<int, int>();
	}
}
