using System;
namespace Kerberos.Sots.Strategy
{
	internal class SpendingPool
	{
		public double Excess
		{
			get;
			private set;
		}
		public double Distribute(double amount, double cap)
		{
			if (amount > cap)
			{
				this.Excess += amount - cap;
				amount = cap;
			}
			return amount;
		}
	}
}
