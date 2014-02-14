using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.Data
{
	internal class DiplomacyActionHistoryEntryInfo
	{
		public int PlayerId;
		public int TowardsPlayerId;
		public int? TurnCount;
		public DiplomacyAction Action;
		public int? ActionSubType;
		public float? ActionData;
		public int? Duration;
		public int? ConsequenceType;
		public float? ConsequenceData;
		public int ID
		{
			get;
			set;
		}
	}
}
