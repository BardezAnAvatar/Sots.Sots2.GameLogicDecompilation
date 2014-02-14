using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal class PendingCombat
	{
		public int ConflictID;
		public int SystemID;
		public List<int> FleetIDs;
		public CombatType Type;
		public PostCombatData CombatResults;
		public List<int> PlayersInCombat;
		public Dictionary<int, ResolutionType> CombatResolutionSelections = new Dictionary<int, ResolutionType>();
		public Dictionary<int, AutoResolveStance> CombatStanceSelections = new Dictionary<int, AutoResolveStance>();
		public Dictionary<int, int> SelectedPlayerFleets = new Dictionary<int, int>();
		public List<int> NPCPlayersInCombat;
		public int CardID;
		public bool complete;
		public PendingCombat()
		{
			this.FleetIDs = new List<int>();
			this.NPCPlayersInCombat = new List<int>();
		}
	}
}
