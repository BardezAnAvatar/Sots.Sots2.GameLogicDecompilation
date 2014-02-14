using System;
namespace Kerberos.Sots.Data
{
	internal class ColonyTrapInfo : IIDProvider
	{
		public int SystemID;
		public int PlanetID;
		public int FleetID;
		public int ID
		{
			get;
			set;
		}
	}
}
