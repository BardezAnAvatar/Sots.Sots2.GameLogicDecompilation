using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.Strategy
{
	internal struct StimulusSpending
	{
		public double RequestedTotal;
		public double RequestedMining;
		public double RequestedColonization;
		public double RequestedTrade;
		public double ProjectedMining;
		public double ProjectedColonization;
		public double ProjectedTrade;
		public double ProjectedTotal
		{
			get
			{
				return this.ProjectedMining + this.ProjectedColonization + this.ProjectedTrade;
			}
		}
		public StimulusSpending(PlayerInfo playerInfo, double total, SpendingPool pool, SpendingCaps caps)
		{
			this.RequestedTotal = total;
			this.RequestedMining = total * (double)playerInfo.RateStimulusMining;
			this.RequestedColonization = total * (double)playerInfo.RateStimulusColonization;
			this.RequestedTrade = total - this.RequestedMining - this.RequestedColonization;
			this.ProjectedMining = pool.Distribute(this.RequestedMining, caps.StimulusMining);
			this.ProjectedColonization = pool.Distribute(this.RequestedColonization, caps.StimulusColonization);
			this.ProjectedTrade = pool.Distribute(this.RequestedTrade, caps.StimulusTrade);
		}
	}
}
