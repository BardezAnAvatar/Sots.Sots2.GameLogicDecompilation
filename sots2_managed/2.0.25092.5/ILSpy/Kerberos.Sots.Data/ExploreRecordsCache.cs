using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal sealed class ExploreRecordsCache : RowCache<int, ExploreRecordInfo>
	{
		public ExploreRecordsCache(SQLiteConnection db, AssetDatabase assets) : base(db, assets)
		{
		}
		private static ExploreRecordInfo GetExploreRecordInfoFromRow(Row row)
		{
			return new ExploreRecordInfo
			{
				SystemId = int.Parse(row[0]),
				PlayerId = int.Parse(row[1]),
				LastTurnExplored = int.Parse(row[2]),
				Visible = bool.Parse(row[3]),
				Explored = bool.Parse(row[4])
			};
		}
		public static int GetRecordKey(ExploreRecordInfo value)
		{
			return value.SystemId << 8 | value.PlayerId;
		}
		public static int GetSystemFromKey(int key)
		{
			return key >> 8;
		}
		public static int GetPlayerFromKey(int key)
		{
			return key & 255;
		}
		protected override int OnInsert(SQLiteConnection db, int? key, ExploreRecordInfo value)
		{
			db.ExecuteIntegerQuery(string.Format(Queries.UpdateExploreRecord, new object[]
			{
				value.SystemId.ToSQLiteValue(),
				value.PlayerId.ToSQLiteValue(),
				value.LastTurnExplored.ToSQLiteValue(),
				value.Visible.ToString(),
				value.Explored.ToString()
			}));
			return ExploreRecordsCache.GetRecordKey(value);
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			throw new NotImplementedException("All updating is handled via finer-grained external calls and reliance on Sync().");
		}
		protected override IEnumerable<KeyValuePair<int, ExploreRecordInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetExploreRecordInfos, true))
				{
					ExploreRecordInfo exploreRecordInfoFromRow = ExploreRecordsCache.GetExploreRecordInfoFromRow(current);
					yield return new KeyValuePair<int, ExploreRecordInfo>(ExploreRecordsCache.GetRecordKey(exploreRecordInfoFromRow), exploreRecordInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					int systemFromKey = ExploreRecordsCache.GetSystemFromKey(current2);
					int playerFromKey = ExploreRecordsCache.GetPlayerFromKey(current2);
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetExploreRecordInfo, systemFromKey.ToSQLiteValue(), playerFromKey.ToSQLiteValue()), true))
					{
						ExploreRecordInfo exploreRecordInfoFromRow2 = ExploreRecordsCache.GetExploreRecordInfoFromRow(current3);
						yield return new KeyValuePair<int, ExploreRecordInfo>(current2, exploreRecordInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, ExploreRecordInfo value)
		{
			db.ExecuteTableQuery(string.Format(Queries.UpdateExploreRecord, new object[]
			{
				value.SystemId,
				value.PlayerId,
				value.LastTurnExplored,
				value.Visible,
				value.Explored
			}), true);
			base.Sync(ExploreRecordsCache.GetRecordKey(value));
		}
	}
}
