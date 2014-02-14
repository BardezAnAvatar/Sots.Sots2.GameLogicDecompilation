using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal sealed class ShipsCache : RowCache<int, ShipInfo>
	{
		private readonly DesignsCache designs;
		private readonly Dictionary<int, int> designIDToShipID;
		public ShipsCache(SQLiteConnection db, AssetDatabase assets, DesignsCache designs) : base(db, assets)
		{
			this.designIDToShipID = new Dictionary<int, int>();
			this.designs = designs;
			designs.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.designs_RowObjectDirtied);
		}
		protected override void OnCleared()
		{
			base.OnCleared();
			this.designIDToShipID.Clear();
		}
		private void designs_RowObjectDirtied(object sender, int key)
		{
			if (sender == this.designs && this.designIDToShipID.ContainsKey(key))
			{
				base.Sync(this.designIDToShipID[key]);
			}
		}
		private ShipInfo GetShipInfoFromRow(Row row)
		{
			int num = row[2].SQLiteValueToInteger();
			return new ShipInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				FleetID = row[1].SQLiteValueToOneBasedIndex(),
				DesignID = num,
				ParentID = row[3].SQLiteValueToInteger(),
				ShipName = row[4].SQLiteValueToString(),
				SerialNumber = row[5].SQLiteValueToInteger(),
				ShipFleetPosition = row[6].SQLiteValueToNullableVector3(),
				ShipSystemPosition = row[7].SQLiteValueToNullableMatrix(),
				Params = (ShipParams)row[8].SQLiteValueToInteger(),
				RiderIndex = row[9].SQLiteValueToInteger(),
				PsionicPower = row[10].SQLiteValueToInteger(),
				AIFleetID = row[11].SQLiteValueToNullableInteger(),
				ComissionDate = row[12].SQLiteValueToInteger(),
				SlavesObtained = row[13].SQLiteValueToDouble(),
				LoaCubes = row[14].SQLiteValueToInteger(),
				DesignInfo = this.designs[num]
			};
		}
		protected override int OnInsert(SQLiteConnection db, int? key, ShipInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Mission insertion does not permit explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertShip, new object[]
			{
				value.FleetID.ToOneBasedSQLiteValue(),
				value.DesignID.ToSQLiteValue(),
				value.ParentID.ToSQLiteValue(),
				value.ShipName.ToSQLiteValue(),
				value.SerialNumber.ToSQLiteValue(),
				((int)value.Params).ToSQLiteValue(),
				value.RiderIndex.ToSQLiteValue(),
				value.PsionicPower.ToSQLiteValue(),
				value.AIFleetID.ToNullableSQLiteValue(),
				value.ComissionDate.ToSQLiteValue(),
				value.LoaCubes.ToSQLiteValue()
			}));
			value.ID = num;
			this.designIDToShipID[value.DesignID] = value.ID;
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			this.designIDToShipID.Remove(base[key].DesignID);
			db.ExecuteNonQuery(string.Format(Queries.RemoveShip, key.ToSQLiteValue()), false, true);
		}
		protected override IEnumerable<KeyValuePair<int, ShipInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetShipInfos, true))
				{
					ShipInfo shipInfoFromRow = this.GetShipInfoFromRow(current);
					this.designIDToShipID[shipInfoFromRow.DesignID] = shipInfoFromRow.ID;
					yield return new KeyValuePair<int, ShipInfo>(shipInfoFromRow.ID, shipInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetShipInfo, current2.ToSQLiteValue()), true))
					{
						ShipInfo shipInfoFromRow2 = this.GetShipInfoFromRow(current3);
						this.designIDToShipID[shipInfoFromRow2.DesignID] = shipInfoFromRow2.ID;
						yield return new KeyValuePair<int, ShipInfo>(shipInfoFromRow2.ID, shipInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, ShipInfo value)
		{
			throw new NotImplementedException("All updating is handled via finer-grained external calls and reliance on Sync().");
		}
	}
}
