using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Data
{
	internal class AIFleetIntel
	{
		public int PlayerID;
		public int IntelOnPlayerID;
		public int LastSeen;
		public int LastSeenSystem;
		public Vector3 LastSeenCoords;
		public int NumDestroyers;
		public int NumCruisers;
		public int NumDreadnoughts;
		public int NumLeviathans;
	}
}
