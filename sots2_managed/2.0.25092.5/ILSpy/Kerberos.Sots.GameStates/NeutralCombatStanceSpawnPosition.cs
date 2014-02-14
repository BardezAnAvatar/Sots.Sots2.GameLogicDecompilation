using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class NeutralCombatStanceSpawnPosition
	{
		public int FleetID;
		public Vector3 Position;
		public Vector3 Facing;
		public void SetInfo(int fleetID, Vector3 pos, Vector3 facing)
		{
			this.FleetID = fleetID;
			this.Position = pos;
			this.Facing = facing;
		}
	}
}
