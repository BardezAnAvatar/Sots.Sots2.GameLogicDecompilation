using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal sealed class DiplomacyStatesCache : RowCache<int, DiplomacyInfo>
	{
		private PlayersCache players;
		private readonly BidirMap<KeyValuePair<int, int>, int> byPlayer = new BidirMap<KeyValuePair<int, int>, int>();
		private void TryRemoveByPlayer(int playerId, int towardsPlayerId)
		{
			KeyValuePair<int, int> keyValuePair = new KeyValuePair<int, int>(playerId, towardsPlayerId);
			if (this.byPlayer.Forward.ContainsKey(keyValuePair))
			{
				this.byPlayer.Remove(keyValuePair, this.byPlayer.Forward[keyValuePair]);
			}
		}
		private void TryInsertByPlayer(int playerId, int towardsPlayerId, int diplomacyInfoId)
		{
			this.byPlayer.Insert(new KeyValuePair<int, int>(playerId, towardsPlayerId), diplomacyInfoId);
		}
		private int InsertDiplomaticStateOneWay(int playerID, int towardPlayerID, DiplomacyState type, int relations, bool isEncountered)
		{
			return base.Insert(null, new DiplomacyInfo
			{
				PlayerID = playerID,
				TowardsPlayerID = towardPlayerID,
				State = type,
				Relations = relations,
				isEncountered = isEncountered
			});
		}
		public int InsertDiplomaticState(int playerID, int towardPlayerID, DiplomacyState type, int relations, bool isEncountered, bool reciprocal)
		{
			if (reciprocal)
			{
				this.InsertDiplomaticStateOneWay(towardPlayerID, playerID, type, relations, isEncountered);
			}
			return this.InsertDiplomaticStateOneWay(playerID, towardPlayerID, type, relations, isEncountered);
		}
		public DiplomacyInfo GetDiplomacyInfoByPlayer(int playerId, int towardsPlayerId)
		{
			base.SynchronizeWithDatabase();
			int key;
			if (this.byPlayer.Forward.TryGetValue(new KeyValuePair<int, int>(playerId, towardsPlayerId), out key))
			{
				return base[key];
			}
			return null;
		}
		private static DiplomacyInfo GetDiplomacyInfoFromRow(Row r)
		{
			return new DiplomacyInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerID = r[1].SQLiteValueToInteger(),
				TowardsPlayerID = r[2].SQLiteValueToInteger(),
				State = (DiplomacyState)r[3].SQLiteValueToInteger(),
				Relations = r[4].SQLiteValueToInteger(),
				isEncountered = r[5].SQLiteValueToBoolean()
			};
		}
		public DiplomacyStatesCache(SQLiteConnection db, AssetDatabase assets) : base(db, assets)
		{
			base.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.DiplomacyStatesCache_RowObjectDirtied);
		}
		private void DiplomacyStatesCache_RowObjectDirtied(object sender, int key)
		{
			if (this.byPlayer.Reverse.ContainsKey(key))
			{
				this.byPlayer.Remove(this.byPlayer.Reverse[key], key);
			}
		}
		public void PostInit(PlayersCache players)
		{
			this.players = players;
		}
		protected override void OnCleared()
		{
			base.OnCleared();
			this.byPlayer.Clear();
		}
		protected override IEnumerable<KeyValuePair<int, DiplomacyInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetDiplomacyInfos, true))
				{
					DiplomacyInfo diplomacyInfoFromRow = DiplomacyStatesCache.GetDiplomacyInfoFromRow(current);
					this.TryInsertByPlayer(diplomacyInfoFromRow.PlayerID, diplomacyInfoFromRow.TowardsPlayerID, diplomacyInfoFromRow.ID);
					yield return new KeyValuePair<int, DiplomacyInfo>(diplomacyInfoFromRow.ID, diplomacyInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetDiplomacyInfo, current2.ToSQLiteValue()), true))
					{
						DiplomacyInfo diplomacyInfoFromRow2 = DiplomacyStatesCache.GetDiplomacyInfoFromRow(current3);
						this.TryInsertByPlayer(diplomacyInfoFromRow2.PlayerID, diplomacyInfoFromRow2.TowardsPlayerID, diplomacyInfoFromRow2.ID);
						yield return new KeyValuePair<int, DiplomacyInfo>(diplomacyInfoFromRow2.ID, diplomacyInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override int OnInsert(SQLiteConnection db, int? key, DiplomacyInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "DiplomacyInfo insertion does not permit explicit specification of an ID.");
			}
			this.players.GetDefaultDiplomacyReactionValue(value.PlayerID, value.TowardsPlayerID);
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertDiplomacyInfo, new object[]
			{
				value.PlayerID.ToSQLiteValue(),
				value.TowardsPlayerID.ToSQLiteValue(),
				((int)value.State).ToSQLiteValue(),
				value.Relations,
				false.ToSQLiteValue()
			}));
			this.TryInsertByPlayer(value.PlayerID, value.TowardsPlayerID, num);
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			throw new NotImplementedException("DiplomacyInfo rows are never removed.");
		}
		protected override void OnUpdate(SQLiteConnection db, int key, DiplomacyInfo value)
		{
			this.TryRemoveByPlayer(value.PlayerID, value.TowardsPlayerID);
			db.ExecuteNonQuery(string.Format(Queries.UpdateDiplomacyInfo, new object[]
			{
				value.PlayerID.ToSQLiteValue(),
				value.TowardsPlayerID.ToSQLiteValue(),
				((int)value.State).ToSQLiteValue(),
				value.Relations.ToSQLiteValue(),
				value.isEncountered.ToSQLiteValue()
			}), false, true);
			this.TryInsertByPlayer(value.PlayerID, value.TowardsPlayerID, key);
		}
	}
}
