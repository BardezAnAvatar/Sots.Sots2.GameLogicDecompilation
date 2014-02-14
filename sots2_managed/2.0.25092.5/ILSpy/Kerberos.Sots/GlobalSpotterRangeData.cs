using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots
{
	public class GlobalSpotterRangeData
	{
		public enum SpotterValueTypes
		{
			BattleRider,
			Cruiser,
			Dreadnought,
			Leviathan,
			Station,
			NumTypes
		}
		public float[] SpotterValues = new float[5];
		public float StationLVLOffset;
		public static GlobalSpotterRangeData.SpotterValueTypes GetTypeFromShipClass(ShipClass sc)
		{
			switch (sc)
			{
			case ShipClass.Cruiser:
				return GlobalSpotterRangeData.SpotterValueTypes.Cruiser;
			case ShipClass.Dreadnought:
				return GlobalSpotterRangeData.SpotterValueTypes.Dreadnought;
			case ShipClass.Leviathan:
				return GlobalSpotterRangeData.SpotterValueTypes.Leviathan;
			case ShipClass.BattleRider:
				return GlobalSpotterRangeData.SpotterValueTypes.BattleRider;
			case ShipClass.Station:
				return GlobalSpotterRangeData.SpotterValueTypes.Station;
			default:
				return GlobalSpotterRangeData.SpotterValueTypes.Cruiser;
			}
		}
	}
}
