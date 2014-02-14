using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class TreatyInfo
	{
		public int InitiatingPlayerId;
		public int ReceivingPlayerId;
		public TreatyType Type;
		public int Duration;
		public int StartingTurn;
		public bool Active;
		public bool Removed;
		public List<TreatyConsequenceInfo> Consequences = new List<TreatyConsequenceInfo>();
		public List<TreatyIncentiveInfo> Incentives = new List<TreatyIncentiveInfo>();
		public int ID
		{
			get;
			set;
		}
	}
}
