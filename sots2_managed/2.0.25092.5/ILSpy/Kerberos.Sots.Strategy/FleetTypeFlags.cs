using System;
namespace Kerberos.Sots.Strategy
{
	public enum FleetTypeFlags
	{
		UNKNOWN,
		COMBAT,
		PLANETATTACK,
		PATROL = 4,
		DEFEND = 8,
		COLONIZE = 16,
		CONSTRUCTION = 32,
		SURVEY = 64,
		GATE = 128,
		NPG = 256,
		ANY = 511
	}
}
