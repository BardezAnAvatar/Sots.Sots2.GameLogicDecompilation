using System;
namespace Kerberos.Sots.Data
{
	internal class RetrofitOrderInfo : IIDProvider
	{
		public int SystemID;
		public int DesignID;
		public int ShipID;
		public int? InvoiceID;
		public int ID
		{
			get;
			set;
		}
	}
}
