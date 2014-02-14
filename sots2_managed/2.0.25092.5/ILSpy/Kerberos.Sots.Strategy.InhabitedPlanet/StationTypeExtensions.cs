using System;
namespace Kerberos.Sots.Strategy.InhabitedPlanet
{
	public static class StationTypeExtensions
	{
		public static string ToDisplayText(this StationType stationType, string faction)
		{
			switch (stationType)
			{
			case StationType.NAVAL:
				return "Naval";
			case StationType.SCIENCE:
				return "Science";
			case StationType.CIVILIAN:
				if (!(faction == "zuul"))
				{
					return "Civilian";
				}
				return "Slave";
			case StationType.DIPLOMATIC:
				if (!(faction == "zuul"))
				{
					return "Diplomatic";
				}
				return "Tribute";
			case StationType.GATE:
				return "Gate";
			case StationType.MINING:
				return "Mining";
			case StationType.DEFENCE:
				return "Defence";
			default:
				return "Unknown";
			}
		}
	}
}
