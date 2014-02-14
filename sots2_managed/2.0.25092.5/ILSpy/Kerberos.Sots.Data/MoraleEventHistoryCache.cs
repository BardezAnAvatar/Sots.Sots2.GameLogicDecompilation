using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal sealed class MoraleEventHistoryCache : RowCache<int, MoraleEventHistory>
	{
		public MoraleEventHistoryCache(SQLiteConnection db, AssetDatabase assets) : base(db, assets)
		{
		}
		private static MoraleEventHistory ParseMoraleEventHistory(Row row)
		{
			return new MoraleEventHistory
			{
				id = row[0].SQLiteValueToInteger(),
				turn = row[1].SQLiteValueToInteger(),
				moraleEvent = (MoralEvent)row[2].SQLiteValueToInteger(),
				playerId = row[3].SQLiteValueToInteger(),
				value = row[4].SQLiteValueToInteger(),
				colonyId = row[5].SQLiteValueToNullableInteger(),
				systemId = row[6].SQLiteValueToNullableInteger(),
				provinceId = row[7].SQLiteValueToNullableInteger()
			};
		}
		protected override int OnInsert(SQLiteConnection db, int? key, MoraleEventHistory value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "MoraleEventHistory insertion does not permit explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertMoraleHistoryEvent, new object[]
			{
				value.turn.ToSQLiteValue(),
				((int)value.moraleEvent).ToSQLiteValue(),
				value.playerId.ToSQLiteValue(),
				value.value.ToSQLiteValue(),
				value.colonyId.ToNullableSQLiteValue(),
				value.systemId.ToNullableSQLiteValue(),
				value.provinceId.ToNullableSQLiteValue()
			}));
			value.id = num;
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteTableQuery(string.Format(Queries.RemoveMoraleHistoryEvent, key.ToSQLiteValue()), true);
		}
		protected override IEnumerable<KeyValuePair<int, MoraleEventHistory>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetMoraleHistoryEvents, true))
				{
					MoraleEventHistory moraleEventHistory = MoraleEventHistoryCache.ParseMoraleEventHistory(current);
					yield return new KeyValuePair<int, MoraleEventHistory>(moraleEventHistory.id, moraleEventHistory);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetMoraleHistoryEvent, current2.ToSQLiteValue()), true))
					{
						MoraleEventHistory moraleEventHistory2 = MoraleEventHistoryCache.ParseMoraleEventHistory(current3);
						yield return new KeyValuePair<int, MoraleEventHistory>(moraleEventHistory2.id, moraleEventHistory2);
					}
				}
			}
			yield break;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, MoraleEventHistory value)
		{
		}
	}
}
