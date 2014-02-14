using System;
namespace Kerberos.Sots.Data
{
	internal class InvoiceInfo : IIDProvider
	{
		public int PlayerID;
		public string Name;
		public bool isFavorite;
		public int ID
		{
			get;
			set;
		}
	}
}
