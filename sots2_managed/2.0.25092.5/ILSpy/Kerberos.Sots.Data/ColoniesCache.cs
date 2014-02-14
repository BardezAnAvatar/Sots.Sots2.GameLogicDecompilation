using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal sealed class ColoniesCache : RowCache<int, ColonyInfo>
	{
		public ColoniesCache(SQLiteConnection db, AssetDatabase assets) : base(db, assets)
		{
		}
		private static ColonyFactionInfo GetColonyFactionInfoFromRow(Row row, int orbitalObjectID)
		{
			return new ColonyFactionInfo
			{
				OrbitalObjectID = orbitalObjectID,
				FactionID = row[0].SQLiteValueToInteger(),
				CivilianPop = row[1].SQLiteValueToDouble(),
				Morale = row[2].SQLiteValueToInteger(),
				CivPopWeight = row[3].SQLiteValueToSingle(),
				TurnEstablished = row[4].SQLiteValueToInteger(),
				LastMorale = row[5].SQLiteValueToInteger()
			};
		}
		public static IEnumerable<ColonyFactionInfo> GetColonyFactionInfosFromOrbitalObjectID(SQLiteConnection db, int orbitalObjectID)
		{
			foreach (Row current in db.ExecuteTableQuery(string.Format(Queries.GetCivilianPopulationsForColony, orbitalObjectID), true))
			{
				yield return ColoniesCache.GetColonyFactionInfoFromRow(current, orbitalObjectID);
			}
			yield break;
		}
		private static ColonyInfo GetColonyInfoFromRow(SQLiteConnection db, Row row)
		{
			int num = row[1].SQLiteValueToInteger();
			float num2 = row[6].SQLiteValueToSingle();
			float num3 = row[7].SQLiteValueToSingle();
			float num4 = row[8].SQLiteValueToSingle();
			float num5 = row[13].SQLiteValueToSingle();
			int cachedStarSystemID = db.ExecuteIntegerQuery(string.Format(Queries.GetOrbitalObjectStarSystemID, num.ToSQLiteValue()));
			return new ColonyInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				OrbitalObjectID = num,
				PlayerID = row[2].SQLiteValueToInteger(),
				ImperialPop = row[3].SQLiteValueToDouble(),
				CivilianWeight = row[4].SQLiteValueToSingle(),
				TurnEstablished = row[5].SQLiteValueToInteger(),
				TerraRate = num2,
				InfraRate = num3,
				ShipConRate = num4,
				TradeRate = (1f - (num2 + num3 + num4 + num5)).Saturate(),
				OverharvestRate = row[9].SQLiteValueToSingle(),
				EconomyRating = row[10].SQLiteValueToSingle(),
				CurrentStage = (ColonyStage)row[11].SQLiteValueToInteger(),
				OverdevelopProgress = row[12].SQLiteValueToSingle(),
				OverdevelopRate = num5,
				PopulationBiosphereRate = row[14].SQLiteValueToSingle(),
				isHardenedStructures = row[15].SQLiteValueToBoolean(),
				RebellionType = (RebellionType)row[16].SQLiteValueToInteger(),
				RebellionTurns = row[17].SQLiteValueToInteger(),
				TurnsOverharvested = row[18].SQLiteValueToInteger(),
				RepairPoints = row[19].SQLiteValueToInteger(),
				SlaveWorkRate = row[20].SQLiteValueToSingle(),
				DamagedLastTurn = row[21].SQLiteValueToBoolean(),
				RepairPointsMax = row[22].SQLiteValueToInteger(),
				OwningColony = row[23].SQLiteValueToBoolean(),
				ReplicantsOn = row[24].SQLiteValueToBoolean(),
				Factions = ColoniesCache.GetColonyFactionInfosFromOrbitalObjectID(db, num).ToArray<ColonyFactionInfo>(),
				CachedStarSystemID = cachedStarSystemID
			};
		}
		protected override int OnInsert(SQLiteConnection db, int? key, ColonyInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Colony insertion does not permit explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertColony, new object[]
			{
				value.OrbitalObjectID.ToSQLiteValue(),
				value.PlayerID.ToSQLiteValue(),
				value.ImperialPop.ToSQLiteValue(),
				value.CivilianWeight.ToSQLiteValue(),
				value.TurnEstablished.ToSQLiteValue()
			}));
			value.CachedStarSystemID = db.ExecuteIntegerQuery(string.Format(Queries.GetOrbitalObjectStarSystemID, value.OrbitalObjectID.ToSQLiteValue()));
			value.ID = num;
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteNonQuery(string.Format(Queries.RemoveColony, key), false, true);
		}
		protected override IEnumerable<KeyValuePair<int, ColonyInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetColonyInfos, true))
				{
					ColonyInfo colonyInfoFromRow = ColoniesCache.GetColonyInfoFromRow(db, current);
					yield return new KeyValuePair<int, ColonyInfo>(colonyInfoFromRow.ID, colonyInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetColonyInfo, current2.ToSQLiteValue()), true))
					{
						ColonyInfo colonyInfoFromRow2 = ColoniesCache.GetColonyInfoFromRow(db, current3);
						yield return new KeyValuePair<int, ColonyInfo>(colonyInfoFromRow2.ID, colonyInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, ColonyInfo value)
		{
			db.ExecuteNonQuery(string.Format(Queries.UpdateColony, new object[]
			{
				key,
				value.PlayerID,
				value.ImperialPop,
				value.CivilianWeight,
				value.TerraRate,
				value.InfraRate,
				value.ShipConRate,
				value.OverharvestRate,
				value.EconomyRating,
				((int)value.CurrentStage).ToSQLiteValue(),
				value.OverdevelopProgress.ToSQLiteValue(),
				value.OverdevelopRate.ToSQLiteValue(),
				value.PopulationBiosphereRate.ToSQLiteValue(),
				value.isHardenedStructures.ToSQLiteValue(),
				((int)value.RebellionType).ToSQLiteValue(),
				value.RebellionTurns.ToSQLiteValue(),
				value.TurnsOverharvested.ToSQLiteValue(),
				value.RepairPoints.ToSQLiteValue(),
				value.SlaveWorkRate.ToSQLiteValue(),
				value.DamagedLastTurn.ToSQLiteValue(),
				value.RepairPointsMax.ToSQLiteValue(),
				value.OwningColony.ToSQLiteValue(),
				value.ReplicantsOn.ToSQLiteValue()
			}), false, true);
		}
	}
}
