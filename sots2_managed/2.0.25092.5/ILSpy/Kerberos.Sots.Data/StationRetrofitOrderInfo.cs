using System;
namespace Kerberos.Sots.Data
{
	internal class StationRetrofitOrderInfo : IIDProvider
	{
		public int DesignID;
		public int ShipID;
		public int ID
		{
			get;
			set;
		}
	}
}
