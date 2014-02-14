using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	public class MCSystemInfo
	{
		public int SystemID;
		public List<int> ControlZones = new List<int>();
		public List<MCFleetInfo> Fleets = new List<MCFleetInfo>();
	}
}
