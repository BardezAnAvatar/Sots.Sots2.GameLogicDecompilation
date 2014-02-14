using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal sealed class StationsCache : RowCache<int, StationInfo>
	{
		private readonly DesignsCache designs;
		private readonly Dictionary<int, int> designIDToStationID;
		public StationsCache(SQLiteConnection db, AssetDatabase assets, DesignsCache designs) : base(db, assets)
		{
			this.designIDToStationID = new Dictionary<int, int>();
			this.designs = designs;
			designs.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.designs_RowObjectDirtied);
		}
		protected override void OnCleared()
		{
			base.OnCleared();
			this.designIDToStationID.Clear();
		}
		private void designs_RowObjectDirtied(object sender, int key)
		{
			if (sender == this.designs && this.designIDToStationID.ContainsKey(key))
			{
				base.Sync(this.designIDToStationID[key]);
			}
		}
		public static StationInfo GetStationInfoFromRow(SQLiteConnection db, DesignsCache designs, Row row)
		{
			return new StationInfo
			{
				OrbitalObjectInfo = GameDatabase.GetOrbitalObjectInfo(db, row[0].SQLiteValueToInteger()),
				PlayerID = row[1].SQLiteValueToInteger(),
				DesignInfo = designs[row[2].SQLiteValueToInteger()],
				WarehousedGoods = row[3].SQLiteValueToInteger(),
				ShipID = row[4].SQLiteValueToOneBasedIndex()
			};
		}
		protected override int OnInsert(SQLiteConnection db, int? key, StationInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Station insertion does not permit explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertStation, new object[]
			{
				value.OrbitalObjectInfo.ParentID.ToNullableSQLiteValue(),
				value.OrbitalObjectInfo.StarSystemID.ToSQLiteValue(),
				value.OrbitalObjectInfo.OrbitalPath.ToString().ToSQLiteValue(),
				value.OrbitalObjectInfo.Name.ToNullableSQLiteValue(),
				value.PlayerID.ToSQLiteValue(),
				value.DesignInfo.ID.ToSQLiteValue(),
				value.ShipID.ToSQLiteValue()
			}));
			value.OrbitalObjectInfo.ID = num;
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteNonQuery(string.Format(Queries.RemoveStation, key.ToSQLiteValue()), false, true);
		}
		protected override IEnumerable<KeyValuePair<int, StationInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetStationInfos, true))
				{
					StationInfo stationInfoFromRow = StationsCache.GetStationInfoFromRow(db, this.designs, current);
					this.designIDToStationID[stationInfoFromRow.DesignInfo.ID] = stationInfoFromRow.ID;
					yield return new KeyValuePair<int, StationInfo>(stationInfoFromRow.ID, stationInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetStationInfo, current2.ToSQLiteValue()), true))
					{
						StationInfo stationInfoFromRow2 = StationsCache.GetStationInfoFromRow(db, this.designs, current3);
						this.designIDToStationID[stationInfoFromRow2.DesignInfo.ID] = stationInfoFromRow2.ID;
						yield return new KeyValuePair<int, StationInfo>(stationInfoFromRow2.ID, stationInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, StationInfo value)
		{
			db.ExecuteNonQuery(string.Format(Queries.UpdateStation, new object[]
			{
				value.OrbitalObjectInfo.ID.ToSQLiteValue(),
				value.PlayerID.ToSQLiteValue(),
				value.DesignInfo.ID.ToSQLiteValue(),
				value.ShipID.ToSQLiteValue()
			}), false, true);
			value.OrbitalObjectInfo = GameDatabase.GetOrbitalObjectInfo(db, value.OrbitalObjectInfo.ID);
			base.Sync(key);
		}
	}
}
