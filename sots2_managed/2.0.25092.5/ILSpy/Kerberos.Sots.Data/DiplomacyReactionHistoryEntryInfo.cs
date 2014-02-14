using System;
namespace Kerberos.Sots.Data
{
	internal class DiplomacyReactionHistoryEntryInfo
	{
		public int PlayerId;
		public int TowardsPlayerId;
		public int? TurnCount;
		public int Difference;
		public StratModifiers Reaction;
		public int ID
		{
			get;
			set;
		}
	}
}
