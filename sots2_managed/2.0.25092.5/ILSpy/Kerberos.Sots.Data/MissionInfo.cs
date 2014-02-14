using Kerberos.Sots.StarFleet;
using System;
namespace Kerberos.Sots.Data
{
	internal class MissionInfo : IIDProvider
	{
		public int FleetID;
		public MissionType Type;
		public int TargetSystemID;
		public int TargetOrbitalObjectID;
		public int TargetFleetID;
		public int Duration;
		public bool UseDirectRoute;
		public int TurnStarted;
		public int StartingSystem;
		public int? StationType;
		public int ID
		{
			get;
			set;
		}
	}
}
