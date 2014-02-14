using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	public class MCFleetInfo
	{
		public int FleetID;
		public int RetreatFleetID;
		public List<MCShipInfo> Ships = new List<MCShipInfo>();
	}
}
