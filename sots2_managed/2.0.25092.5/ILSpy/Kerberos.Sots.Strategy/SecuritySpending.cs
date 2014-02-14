using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.Strategy
{
	internal struct SecuritySpending
	{
		public double RequestedTotal;
		public double RequestedOperations;
		public double RequestedIntelligence;
		public double RequestedCounterIntelligence;
		public double ProjectedOperations;
		public double ProjectedIntelligence;
		public double ProjectedCounterIntelligence;
		public double ProjectedTotal
		{
			get
			{
				return this.ProjectedOperations + this.ProjectedIntelligence + this.ProjectedCounterIntelligence;
			}
		}
		public SecuritySpending(PlayerInfo playerInfo, double total, SpendingPool pool, SpendingCaps caps)
		{
			this.RequestedTotal = total;
			this.RequestedOperations = total * (double)playerInfo.RateSecurityOperations;
			this.RequestedIntelligence = total * (double)playerInfo.RateSecurityIntelligence;
			this.RequestedCounterIntelligence = total - this.RequestedOperations - this.RequestedIntelligence;
			this.ProjectedOperations = pool.Distribute(this.RequestedOperations, caps.SecurityOperations);
			this.ProjectedIntelligence = pool.Distribute(this.RequestedIntelligence, caps.SecurityIntelligence);
			this.ProjectedCounterIntelligence = pool.Distribute(this.RequestedCounterIntelligence, caps.SecurityCounterIntelligence);
		}
	}
}
