using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Data
{
	internal class FleetLocation
	{
		public int FleetID;
		public int SystemID;
		public Vector3 Coords;
		public Vector3? Direction;
		public Vector3? FromSystemCoords;
		public Vector3? ToSystemCoords;
	}
}
