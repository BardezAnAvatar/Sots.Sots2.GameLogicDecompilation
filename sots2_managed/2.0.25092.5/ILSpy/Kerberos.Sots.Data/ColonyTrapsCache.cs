using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal sealed class ColonyTrapsCache : RowCache<int, ColonyTrapInfo>
	{
		public ColonyTrapsCache(SQLiteConnection db, AssetDatabase assets) : base(db, assets)
		{
		}
		private static ColonyTrapInfo GetColonyTrapInfoFromRow(Row row)
		{
			return new ColonyTrapInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				SystemID = row[1].SQLiteValueToInteger(),
				PlanetID = row[2].SQLiteValueToInteger(),
				FleetID = row[3].SQLiteValueToInteger()
			};
		}
		protected override int OnInsert(SQLiteConnection db, int? key, ColonyTrapInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Colony trap insertion does not permit explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertColonyTrap, value.SystemID.ToSQLiteValue(), value.PlanetID.ToSQLiteValue(), value.FleetID.ToSQLiteValue()));
			value.ID = num;
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteTableQuery(string.Format(Queries.RemoveColonyTrapInfo, key.ToSQLiteValue()), true);
		}
		protected override IEnumerable<KeyValuePair<int, ColonyTrapInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetColonyTrapInfos, true))
				{
					ColonyTrapInfo colonyTrapInfoFromRow = ColonyTrapsCache.GetColonyTrapInfoFromRow(current);
					yield return new KeyValuePair<int, ColonyTrapInfo>(colonyTrapInfoFromRow.ID, colonyTrapInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetColonyTrapInfo, current2.ToSQLiteValue()), true))
					{
						ColonyTrapInfo colonyTrapInfoFromRow2 = ColonyTrapsCache.GetColonyTrapInfoFromRow(current3);
						yield return new KeyValuePair<int, ColonyTrapInfo>(colonyTrapInfoFromRow2.ID, colonyTrapInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, ColonyTrapInfo value)
		{
			throw new NotImplementedException("All updating is handled via finer-grained external calls and reliance on Sync().");
		}
	}
}
