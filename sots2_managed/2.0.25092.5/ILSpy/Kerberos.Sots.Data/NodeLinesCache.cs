using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal sealed class NodeLinesCache : RowCache<int, NodeLineInfo>
	{
		public NodeLinesCache(SQLiteConnection db, AssetDatabase assets) : base(db, assets)
		{
		}
		private static NodeLineInfo GetNodeLineInfoFromRow(Row row)
		{
			return new NodeLineInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				System1ID = row[1].SQLiteValueToInteger(),
				System2ID = row[2].SQLiteValueToInteger(),
				Health = row[3].SQLiteValueToInteger()
			};
		}
		protected override int OnInsert(SQLiteConnection db, int? key, NodeLineInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Node line insertion does not permit explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertNodeLine, value.System1ID.ToSQLiteValue(), value.System2ID.ToSQLiteValue(), value.Health.ToSQLiteValue()));
			value.ID = num;
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteNonQuery(string.Format(Queries.RemoveNodeLine, key.ToSQLiteValue()), false, true);
		}
		protected override IEnumerable<KeyValuePair<int, NodeLineInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetNodeLines, true))
				{
					NodeLineInfo nodeLineInfoFromRow = NodeLinesCache.GetNodeLineInfoFromRow(current);
					nodeLineInfoFromRow.IsLoaLine = db.ExecuteTableQuery(string.Format("SELECT * FROM loa_line_records WHERE node_line_id = {0}", nodeLineInfoFromRow.ID.ToSQLiteValue()), true).Any<Row>();
					yield return new KeyValuePair<int, NodeLineInfo>(nodeLineInfoFromRow.ID, nodeLineInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetNodeLine, current2.ToSQLiteValue()), true))
					{
						NodeLineInfo nodeLineInfoFromRow2 = NodeLinesCache.GetNodeLineInfoFromRow(current3);
						nodeLineInfoFromRow2.IsLoaLine = db.ExecuteTableQuery(string.Format("SELECT * FROM loa_line_records WHERE node_line_id = {0}", nodeLineInfoFromRow2.ID.ToSQLiteValue()), true).Any<Row>();
						yield return new KeyValuePair<int, NodeLineInfo>(nodeLineInfoFromRow2.ID, nodeLineInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, NodeLineInfo value)
		{
			db.ExecuteNonQuery(string.Format(Queries.UpdateNodeLineHealth, value.ID.ToSQLiteValue(), value.Health.ToSQLiteValue()), false, true);
			base.Sync(key);
		}
	}
}
