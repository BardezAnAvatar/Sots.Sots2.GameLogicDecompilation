using Kerberos.Sots.StarFleet;
using System;
namespace Kerberos.Sots.Data
{
	internal class WaypointInfo : IIDProvider
	{
		public int MissionID;
		public WaypointType Type;
		public int? SystemID;
		public int ID
		{
			get;
			set;
		}
	}
}
