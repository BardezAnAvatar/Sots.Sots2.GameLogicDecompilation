using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class CombatData
	{
		private int _combatID;
		private int _systemID;
		private int _turn;
		private List<PlayerCombatData> _data;
		public int CombatID
		{
			get
			{
				return this._combatID;
			}
		}
		public int SystemID
		{
			get
			{
				return this._systemID;
			}
		}
		public int Turn
		{
			get
			{
				return this._turn;
			}
		}
		private void Construct()
		{
			this._data = new List<PlayerCombatData>();
		}
		public CombatData(int combatID, int systemID, int turn)
		{
			this.Construct();
			this._systemID = systemID;
			this._turn = turn;
			this._combatID = combatID;
		}
		public CombatData(ScriptMessageReader mr, int version)
		{
			this.Construct();
			this._combatID = mr.ReadInteger();
			this._systemID = mr.ReadInteger();
			this._turn = mr.ReadInteger();
			int num = mr.ReadInteger();
			for (int i = 0; i < num; i++)
			{
				this._data.Add(new PlayerCombatData(mr, version));
			}
		}
		public PlayerCombatData GetOrAddPlayer(int playerID)
		{
			foreach (PlayerCombatData current in this._data)
			{
				if (current.PlayerID == playerID)
				{
					return current;
				}
			}
			this._data.Add(new PlayerCombatData(playerID));
			return this._data.Last<PlayerCombatData>();
		}
		public PlayerCombatData GetPlayer(int playerID)
		{
			foreach (PlayerCombatData current in this._data)
			{
				if (current.PlayerID == playerID)
				{
					return current;
				}
			}
			return null;
		}
		public List<PlayerCombatData> GetPlayers()
		{
			return this._data;
		}
		public List<object> ToList()
		{
			List<object> list = new List<object>();
			list.Add(this._combatID);
			list.Add(this._systemID);
			list.Add(this._turn);
			list.Add(this._data.Count<PlayerCombatData>());
			foreach (PlayerCombatData current in this._data)
			{
				list.AddRange(current.ToList());
			}
			return list;
		}
		public byte[] ToByteArray()
		{
			ScriptMessageWriter scriptMessageWriter = new ScriptMessageWriter();
			scriptMessageWriter.Write(this.ToList().ToArray());
			return scriptMessageWriter.GetBuffer();
		}
	}
}
