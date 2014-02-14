using System;
namespace Kerberos.Sots.Data
{
	internal class TreatyIncentiveInfo
	{
		public int TreatyId;
		public IncentiveType Type = IncentiveType.Savings;
		public float IncentiveValue;
		public int ID
		{
			get;
			set;
		}
	}
}
