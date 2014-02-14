using System;
namespace Kerberos.Sots.Data
{
	internal class BuildSiteInfo : IIDProvider
	{
		public int StationID;
		public int PlanetID;
		public int ShipID;
		public int Resources;
		public int ID
		{
			get;
			set;
		}
		public int OrbitalID
		{
			get
			{
				if (this.PlanetID != 0)
				{
					return this.PlanetID;
				}
				return this.StationID;
			}
		}
	}
}
