using System;
namespace Kerberos.Sots.Data
{
	internal class WeaponBankInfo : IIDProvider
	{
		public int? WeaponID;
		public int? DesignID;
		public int? FiringMode;
		public int? FilterMode;
		public Guid BankGUID;
		public int ID
		{
			get;
			set;
		}
	}
}
