using System;
namespace Kerberos.Sots.Strategy.InhabitedPlanet
{
	internal static class StationTypeHelper
	{
		public static StationTypeFlags ToFlags(this StationType value)
		{
			return (StationTypeFlags)(1 << (int)value);
		}
		public static StationType ToType(this StationTypeFlags value)
		{
			for (int i = 0; i < 8; i++)
			{
				StationType stationType = (StationType)i;
				StationTypeFlags stationTypeFlags = stationType.ToFlags();
				if ((stationTypeFlags & value) == stationTypeFlags)
				{
					return stationType;
				}
			}
			return StationType.INVALID_TYPE;
		}
	}
}
