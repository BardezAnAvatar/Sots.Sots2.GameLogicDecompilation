using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.Strategy
{
	internal struct ResearchSpending
	{
		public double RequestedTotal;
		public double RequestedCurrentProject;
		public double RequestedSpecialProject;
		public double RequestedSalvageResearch;
		public double ProjectedCurrentProject;
		public double ProjectedSpecialProject;
		public double ProjectedSalvageResearch;
		public double ProjectedTotal
		{
			get
			{
				return this.ProjectedCurrentProject + this.ProjectedSpecialProject + this.ProjectedSalvageResearch;
			}
		}
		public ResearchSpending(PlayerInfo playerInfo, double total, SpendingPool pool, SpendingCaps caps)
		{
			this.RequestedTotal = total;
			this.RequestedCurrentProject = total * (double)playerInfo.RateResearchCurrentProject;
			this.RequestedSpecialProject = total * (double)playerInfo.RateResearchSpecialProject;
			this.RequestedSalvageResearch = total - this.RequestedCurrentProject - this.RequestedSpecialProject;
			this.ProjectedCurrentProject = pool.Distribute(this.RequestedCurrentProject, caps.ResearchCurrentProject);
			this.ProjectedSpecialProject = pool.Distribute(this.RequestedSpecialProject, caps.ResearchSpecialProject);
			this.ProjectedSalvageResearch = pool.Distribute(this.RequestedSalvageResearch, caps.ResearchSalvageResearch);
		}
	}
}
