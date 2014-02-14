using System;
namespace Kerberos.Sots.StarFleet
{
	internal class MissionEstimate
	{
		public int TurnsToTarget;
		public int TurnsToReturn;
		public int TurnsAtTarget;
		public int TurnsForConstruction;
		public float ConstructionCost;
		public int TripsTillSelfSufficeincy;
		public int TurnsTillPhase1Completion;
		public int TotalTurns
		{
			get
			{
				int num = 0;
				num += this.TurnsForConstruction;
				num += this.TurnsToTarget;
				num += this.TurnsAtTarget;
				num += this.TurnsToReturn;
				return num + this.TurnsColonySupport;
			}
		}
		public int TurnsColonySupport
		{
			get
			{
				if (this.TripsTillSelfSufficeincy > 0)
				{
					return (this.TurnsToTarget + this.TurnsToReturn) * this.TripsTillSelfSufficeincy;
				}
				return 0;
			}
		}
		public int TotalTravelTurns
		{
			get
			{
				int num = 0;
				num += this.TurnsToTarget;
				return num + this.TurnsToReturn;
			}
		}
	}
}
