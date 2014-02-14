using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class CombatDataHelper
	{
		private List<CombatData> _combatData;
		public CombatDataHelper()
		{
			this._combatData = new List<CombatData>();
		}
		public CombatData AddCombat(int conflictID, int systemID, int turn)
		{
			this._combatData.Add(new CombatData(conflictID, systemID, turn));
			return this._combatData.Last<CombatData>();
		}
		public void AddCombat(ScriptMessageReader mr, int version)
		{
			this._combatData.Add(new CombatData(mr, version));
		}
		public CombatData GetFirstCombatInSystem(GameDatabase db, int systemID, int turn)
		{
			foreach (CombatData current in this._combatData)
			{
				if (current.SystemID == systemID && current.Turn == turn)
				{
					CombatData result = current;
					return result;
				}
			}
			int version = 0;
			ScriptMessageReader combatData = db.GetCombatData(systemID, turn, out version);
			if (combatData != null)
			{
				this.AddCombat(combatData, version);
				return this.GetLastCombat();
			}
			return null;
		}
		public CombatData GetCombat(GameDatabase db, int combatID, int systemID, int turn)
		{
			foreach (CombatData current in this._combatData)
			{
				if (current.CombatID == combatID && current.SystemID == systemID && current.Turn == turn)
				{
					CombatData result = current;
					return result;
				}
			}
			int version = 0;
			ScriptMessageReader combatData = db.GetCombatData(systemID, combatID, turn, out version);
			if (combatData != null)
			{
				this.AddCombat(combatData, version);
				return this.GetLastCombat();
			}
			return null;
		}
		public IEnumerable<CombatData> GetCombatsForPlayer(GameDatabase db, int playerID, int turnCount)
		{
			foreach (CombatData current in this._combatData)
			{
				if (current.GetPlayers().Any((PlayerCombatData x) => x.PlayerID == playerID))
				{
					if (turnCount != 0)
					{
						int turnCount2 = db.GetTurnCount();
						if (turnCount2 - current.Turn > turnCount)
						{
							continue;
						}
					}
					yield return current;
				}
			}
			yield break;
		}
		public IEnumerable<CombatData> GetCombatsForPlayer(GameDatabase db, int playerID, int systemId, int turnCount)
		{
			return 
				from x in this.GetCombatsForPlayer(db, playerID, turnCount)
				where x.SystemID == systemId
				select x;
		}
		public CombatData GetLastCombat()
		{
			if (this._combatData.Count > 0)
			{
				return this._combatData.Last<CombatData>();
			}
			return null;
		}
		public IEnumerable<CombatData> GetCombats(int turn)
		{
			foreach (CombatData current in this._combatData)
			{
				if (current.Turn == turn)
				{
					yield return current;
				}
			}
			yield break;
		}
		public void ClearCombatData()
		{
			this._combatData.Clear();
		}
	}
}
