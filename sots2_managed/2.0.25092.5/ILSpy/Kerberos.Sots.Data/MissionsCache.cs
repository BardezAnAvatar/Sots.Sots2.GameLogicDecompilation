using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal sealed class MissionsCache : RowCache<int, MissionInfo>
	{
		public MissionsCache(SQLiteConnection db, AssetDatabase assets) : base(db, assets)
		{
		}
		private static MissionInfo GetMissionInfoFromRow(Row row)
		{
			return new MissionInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				FleetID = row[1].SQLiteValueToInteger(),
				Type = (MissionType)row[2].SQLiteValueToInteger(),
				TargetSystemID = row[3].SQLiteValueToOneBasedIndex(),
				TargetOrbitalObjectID = row[4].SQLiteValueToOneBasedIndex(),
				TargetFleetID = row[5].SQLiteValueToOneBasedIndex(),
				Duration = row[6].SQLiteValueToInteger(),
				UseDirectRoute = row[7].SQLiteValueToBoolean(),
				TurnStarted = row[8].SQLiteValueToInteger(),
				StartingSystem = row[9].SQLiteValueToOneBasedIndex(),
				StationType = row[10].SQLiteValueToNullableInteger()
			};
		}
		protected override int OnInsert(SQLiteConnection db, int? key, MissionInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Mission insertion does not permit explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertMission, new object[]
			{
				value.FleetID.ToSQLiteValue(),
				((int)value.Type).ToSQLiteValue(),
				value.TargetSystemID.ToOneBasedSQLiteValue(),
				value.TargetOrbitalObjectID.ToOneBasedSQLiteValue(),
				value.TargetFleetID.ToOneBasedSQLiteValue(),
				value.Duration.ToSQLiteValue(),
				value.UseDirectRoute.ToSQLiteValue(),
				value.TurnStarted.ToSQLiteValue(),
				value.StartingSystem.ToOneBasedSQLiteValue(),
				value.StationType.ToNullableSQLiteValue()
			}));
			value.ID = num;
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteTableQuery(string.Format(Queries.RemoveMission, key.ToSQLiteValue()), true);
		}
		protected override IEnumerable<KeyValuePair<int, MissionInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetMissionInfos, true))
				{
					MissionInfo missionInfoFromRow = MissionsCache.GetMissionInfoFromRow(current);
					yield return new KeyValuePair<int, MissionInfo>(missionInfoFromRow.ID, missionInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetMissionInfo, current2.ToSQLiteValue()), true))
					{
						MissionInfo missionInfoFromRow2 = MissionsCache.GetMissionInfoFromRow(current3);
						yield return new KeyValuePair<int, MissionInfo>(missionInfoFromRow2.ID, missionInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, MissionInfo value)
		{
			db.ExecuteTableQuery(string.Format(Queries.UpdateMission, new object[]
			{
				value.ID.ToSQLiteValue(),
				((int)value.Type).ToSQLiteValue(),
				value.TargetSystemID.ToOneBasedSQLiteValue(),
				value.TargetOrbitalObjectID.ToOneBasedSQLiteValue(),
				value.TargetFleetID.ToOneBasedSQLiteValue(),
				value.Duration.ToSQLiteValue(),
				value.UseDirectRoute.ToSQLiteValue()
			}), true);
		}
	}
}
