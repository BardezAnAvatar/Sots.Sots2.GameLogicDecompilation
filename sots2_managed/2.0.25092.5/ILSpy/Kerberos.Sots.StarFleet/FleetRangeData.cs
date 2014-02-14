using System;
namespace Kerberos.Sots.StarFleet
{
	public class FleetRangeData
	{
		public float FleetRange;
		public float? FleetTravelSpeed;
		public float? FleetNodeTravelSpeed;
		public FleetRangeData()
		{
			this.FleetRange = 0f;
			this.FleetTravelSpeed = null;
			this.FleetNodeTravelSpeed = null;
		}
	}
}
