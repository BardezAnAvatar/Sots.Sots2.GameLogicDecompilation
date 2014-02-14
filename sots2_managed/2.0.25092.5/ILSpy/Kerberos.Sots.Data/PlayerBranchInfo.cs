using System;
namespace Kerberos.Sots.Data
{
	internal class PlayerBranchInfo
	{
		public struct PrimaryKey
		{
			public int FromTechID;
			public int ToTechID;
			public int PlayerID;
			public override string ToString()
			{
				return string.Format("{0}: {1}->{2}", this.PlayerID, this.FromTechID, this.ToTechID);
			}
		}
		public int PlayerID;
		public int FromTechID;
		public int ToTechID;
		public int ResearchCost;
		public float Feasibility;
		public PlayerBranchInfo.PrimaryKey ID
		{
			get
			{
				return new PlayerBranchInfo.PrimaryKey
				{
					FromTechID = this.FromTechID,
					ToTechID = this.ToTechID,
					PlayerID = this.PlayerID
				};
			}
		}
		public override string ToString()
		{
			return string.Format("${3} %{4}", this.ResearchCost, this.Feasibility);
		}
	}
}
