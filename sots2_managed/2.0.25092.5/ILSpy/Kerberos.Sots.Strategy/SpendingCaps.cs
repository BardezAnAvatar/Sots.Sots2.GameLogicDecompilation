using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.Strategy
{
	internal class SpendingCaps
	{
		public readonly double StimulusMining;
		public readonly double StimulusColonization;
		public readonly double StimulusTrade;
		public readonly double SecurityOperations;
		public readonly double SecurityIntelligence;
		public readonly double SecurityCounterIntelligence;
		public readonly double ResearchCurrentProject;
		public readonly double ResearchSpecialProject;
		public readonly double ResearchSalvageResearch;
		public SpendingCaps(GameDatabase db, PlayerInfo playerInfo, BudgetProjection projection)
		{
			int playerResearchingTechID = db.GetPlayerResearchingTechID(playerInfo.ID);
			int playerFeasibilityStudyTechId = db.GetPlayerFeasibilityStudyTechId(playerInfo.ID);
			bool flag = playerResearchingTechID != 0 || playerFeasibilityStudyTechId != 0;
			if (projection == BudgetProjection.Actual)
			{
				this.StimulusMining = 0.0;
				this.StimulusColonization = 0.0;
				this.StimulusTrade = 1.7976931348623157E+308;
				this.SecurityOperations = 1.7976931348623157E+308;
				this.SecurityIntelligence = 1.7976931348623157E+308;
				this.SecurityCounterIntelligence = 1.7976931348623157E+308;
				this.ResearchCurrentProject = (flag ? 1.7976931348623157E+308 : 0.0);
				this.ResearchSpecialProject = 0.0;
				this.ResearchSalvageResearch = 0.0;
				return;
			}
			this.StimulusMining = 1.7976931348623157E+308;
			this.StimulusColonization = 1.7976931348623157E+308;
			this.StimulusTrade = 1.7976931348623157E+308;
			this.SecurityOperations = 1.7976931348623157E+308;
			this.SecurityIntelligence = 1.7976931348623157E+308;
			this.SecurityCounterIntelligence = 1.7976931348623157E+308;
			this.ResearchCurrentProject = 1.7976931348623157E+308;
			this.ResearchSpecialProject = 1.7976931348623157E+308;
			this.ResearchSalvageResearch = 1.7976931348623157E+308;
		}
	}
}
