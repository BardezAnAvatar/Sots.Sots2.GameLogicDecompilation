using System;
namespace Kerberos.Sots.Data
{
	internal class InvoiceInstanceInfo : IIDProvider
	{
		public int PlayerID;
		public int SystemID;
		public string Name;
		public int ID
		{
			get;
			set;
		}
	}
}
