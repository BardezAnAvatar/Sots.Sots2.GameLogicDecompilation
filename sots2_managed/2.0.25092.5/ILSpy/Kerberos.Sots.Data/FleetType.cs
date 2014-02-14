using System;
namespace Kerberos.Sots.Data
{
	[Flags]
	public enum FleetType
	{
		FL_NORMAL = 1,
		FL_RESERVE = 2,
		FL_DEFENSE = 4,
		FL_GATE = 8,
		FL_LIMBOFLEET = 16,
		FL_STATION = 32,
		FL_CARAVAN = 64,
		FL_TRAP = 128,
		FL_ACCELERATOR = 256,
		FL_ALL = 335,
		FL_ALL_COMBAT = 268
	}
}
