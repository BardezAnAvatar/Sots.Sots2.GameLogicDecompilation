using System;
namespace Kerberos.Sots.Combat
{
	internal enum SwarmerSpawnerStates
	{
		IDLE,
		EMITSWARMER,
		INTEGRATESWARMER,
		ADDINGSWARMERS,
		LAUNCHSWARMER,
		WAITFORLAUNCH,
		SEEK,
		TRACK
	}
}
