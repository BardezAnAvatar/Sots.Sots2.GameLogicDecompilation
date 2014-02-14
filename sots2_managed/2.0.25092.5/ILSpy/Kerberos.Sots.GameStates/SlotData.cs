using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
namespace Kerberos.Sots.GameStates
{
	public class SlotData
	{
		public int OccupantID;
		public int Parent;
		public int ParentDBID;
		public StationTypeFlags SupportedTypes;
		public float Rotation;
		public Vector3 Position = Vector3.Zero;
	}
}
