using System;
namespace Kerberos.Sots.GameObjects
{
	internal enum NPlayerStatus
	{
		PS_LOBBY,
		PS_READY,
		PS_DOWNLOADING,
		PS_TURN,
		PS_WAIT,
		PS_REACTION,
		PS_PRECOMBAT,
		PS_COMBAT_WAIT,
		PS_COMBAT,
		PS_POSTCOMBAT,
		PS_DEFEATED
	}
}
