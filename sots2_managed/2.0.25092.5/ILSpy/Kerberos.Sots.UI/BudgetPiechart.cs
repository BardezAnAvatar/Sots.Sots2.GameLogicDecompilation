using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.UI
{
	internal class BudgetPiechart : Piechart
	{
		private readonly AssetDatabase _assetdb;
		public void SetSlices(Budget budget)
		{
			double num = budget.TotalRevenue + budget.NetSavingsLoss - budget.PendingStationsModulesCost - budget.PendingBuildShipsCost - budget.PendingBuildStationsCost;
			num = Math.Max(num, 0.0);
			if (num == 0.0)
			{
				base.SetSlices(new PiechartSlice[]
				{
					new PiechartSlice(this._assetdb.PieChartColourSavings, 1.0)
				});
				return;
			}
			PiechartSlice[] slices = new PiechartSlice[]
			{
				new PiechartSlice(this._assetdb.PieChartColourShipMaintenance, budget.UpkeepExpenses / num),
				new PiechartSlice(this._assetdb.PieChartColourPlanetaryDevelopment, budget.ColonySupportExpenses / num),
				new PiechartSlice(this._assetdb.PieChartColourDebtInterest, budget.DebtInterest / num),
				new PiechartSlice(this._assetdb.PieChartColourResearch, budget.ResearchSpending.ProjectedTotal / num),
				new PiechartSlice(this._assetdb.PieChartColourSecurity, budget.SecuritySpending.ProjectedTotal / num),
				new PiechartSlice(this._assetdb.PieChartColourStimulus, budget.StimulusSpending.ProjectedTotal / num),
				new PiechartSlice(this._assetdb.PieChartColourSavings, budget.NetSavingsIncome / num),
				new PiechartSlice(this._assetdb.PieChartColourCorruption, budget.CorruptionExpenses / num)
			};
			base.SetSlices(slices);
		}
		public BudgetPiechart(UICommChannel ui, string id, AssetDatabase assetdb) : base(ui, id)
		{
			this._assetdb = assetdb;
		}
	}
}
