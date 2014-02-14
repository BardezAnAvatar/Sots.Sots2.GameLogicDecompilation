using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class ApproachingFleet
	{
		public string Name = "";
		public int FleetID;
		public int PlayerID;
		public Vector3 DirFromSystem = default(Vector3);
		public Vector3 Location = default(Vector3);
	}
}
