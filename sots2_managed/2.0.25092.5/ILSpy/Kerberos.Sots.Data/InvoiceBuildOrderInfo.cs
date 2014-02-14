using System;
namespace Kerberos.Sots.Data
{
	internal class InvoiceBuildOrderInfo : IIDProvider
	{
		public int DesignID;
		public string ShipName;
		public int InvoiceID;
		public int LoaCubes;
		public int ID
		{
			get;
			set;
		}
	}
}
