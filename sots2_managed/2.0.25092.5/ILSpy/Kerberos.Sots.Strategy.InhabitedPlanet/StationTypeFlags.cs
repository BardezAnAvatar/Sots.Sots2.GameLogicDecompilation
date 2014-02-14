using System;
namespace Kerberos.Sots.Strategy.InhabitedPlanet
{
	[Flags]
	public enum StationTypeFlags
	{
		NAVAL = 2,
		SCIENCE = 4,
		CIVILIAN = 8,
		DIPLOMATIC = 16,
		GATE = 32,
		MINING = 64,
		DEFENCE = 128
	}
}
