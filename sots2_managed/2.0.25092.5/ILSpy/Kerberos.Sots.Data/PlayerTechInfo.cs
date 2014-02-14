using System;
namespace Kerberos.Sots.Data
{
	internal class PlayerTechInfo
	{
		public struct PrimaryKey
		{
			public int PlayerID;
			public int TechID;
		}
		public int PlayerID;
		public int TechID;
		public string TechFileID;
		public TechStates State;
		public int Progress;
		public int ResearchCost;
		public float Feasibility;
		public float PlayerFeasibility;
		public int? TurnResearched;
		public PlayerTechInfo.PrimaryKey ID
		{
			get
			{
				return new PlayerTechInfo.PrimaryKey
				{
					PlayerID = this.PlayerID,
					TechID = this.TechID
				};
			}
		}
		public override string ToString()
		{
			return this.TechFileID ?? string.Empty;
		}
	}
}
