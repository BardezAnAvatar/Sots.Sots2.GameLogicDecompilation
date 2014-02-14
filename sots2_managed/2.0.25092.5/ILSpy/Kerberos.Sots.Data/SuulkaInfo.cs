using System;
namespace Kerberos.Sots.Data
{
	internal class SuulkaInfo : IIDProvider
	{
		public int? PlayerID;
		public int ShipID;
		public int AdmiralID;
		public int? StationID;
		public int ArrivalTurns;
		public int ID
		{
			get;
			set;
		}
	}
}
