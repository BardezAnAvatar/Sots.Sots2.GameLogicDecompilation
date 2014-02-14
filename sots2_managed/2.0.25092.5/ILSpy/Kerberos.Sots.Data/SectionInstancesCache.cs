using Kerberos.Sots.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal sealed class SectionInstancesCache : RowCache<int, SectionInstanceInfo>
	{
		private readonly DesignsCache designs;
		private readonly ShipsCache ships;
		private readonly StationsCache stations;
		private ArmorInstancesCache armor_instances;
		private WeaponInstancesCache weapon_instances;
		private ModuleInstancesCache module_instances;
		public SectionInstancesCache(SQLiteConnection db, AssetDatabase assets, DesignsCache designs, ShipsCache ships, StationsCache stations) : base(db, assets)
		{
			this.designs = designs;
			this.ships = ships;
			this.stations = stations;
			designs.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.designs_RowObjectRemoving);
			ships.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.ships_RowObjectRemoving);
			stations.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.stations_RowObjectRemoving);
		}
		internal void PostInit(ArmorInstancesCache armor_instances, WeaponInstancesCache weapon_instances, ModuleInstancesCache module_instances)
		{
			this.armor_instances = armor_instances;
			this.weapon_instances = weapon_instances;
			this.module_instances = module_instances;
			weapon_instances.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.weapon_instances_RowObjectDirtied);
			module_instances.RowObjectDirtied += new RowObjectDirtiedEventHandler<int>(this.module_instances_RowObjectDirtied);
		}
		private void module_instances_RowObjectDirtied(object sender, int key)
		{
			base.SyncRange(
				from x in base.Values
				where this.module_instances[key].SectionInstanceID == x.ID
				select x into y
				select y.ID);
		}
		private void weapon_instances_RowObjectDirtied(object sender, int key)
		{
			base.SyncRange(
				from x in base.Values
				where this.weapon_instances[key].SectionInstanceID == x.ID
				select x into y
				select y.ID);
		}
		private void stations_RowObjectRemoving(object sender, int key)
		{
			foreach (SectionInstanceInfo current in (
				from x in base.Values
				where x.StationID == key
				select x).ToList<SectionInstanceInfo>())
			{
				base.Remove(current.ID);
			}
		}
		private void ships_RowObjectRemoving(object sender, int key)
		{
			foreach (SectionInstanceInfo current in (
				from x in base.Values
				where x.ShipID == key
				select x).ToList<SectionInstanceInfo>())
			{
				base.Remove(current.ID);
			}
		}
		private void designs_RowObjectRemoving(object sender, int key)
		{
			foreach (SectionInstanceInfo current in (
				from x in base.Values
				where this.designs[key].DesignSections.Any((DesignSectionInfo y) => x.SectionID == y.ID)
				select x).ToList<SectionInstanceInfo>())
			{
				base.Remove(current.ID);
			}
		}
		private SectionInstanceInfo GetSectionInstanceInfoFromRow(Row row)
		{
			int num = row[0].SQLiteValueToInteger();
			return new SectionInstanceInfo
			{
				ID = num,
				SectionID = row[1].SQLiteValueToInteger(),
				ShipID = row[2].SQLiteValueToNullableInteger(),
				StationID = row[3].SQLiteValueToNullableInteger(),
				Structure = row[4].SQLiteValueToInteger(),
				Supply = row[5].SQLiteValueToInteger(),
				Crew = row[6].SQLiteValueToInteger(),
				Signature = row[7].SQLiteValueToSingle(),
				RepairPoints = row[8].SQLiteValueToInteger(),
				Armor = this.armor_instances.ContainsKey(num) ? this.armor_instances[num] : ArmorInstancesCache.EmptyArmorInstances,
				WeaponInstances = this.weapon_instances.EnumerateBySectionInstanceID(num).ToList<WeaponInstanceInfo>(),
				ModuleInstances = this.module_instances.EnumerateBySectionInstanceID(num).ToList<ModuleInstanceInfo>()
			};
		}
		protected override IEnumerable<KeyValuePair<int, SectionInstanceInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetAllShipSectionInstances, true))
				{
					SectionInstanceInfo sectionInstanceInfoFromRow = this.GetSectionInstanceInfoFromRow(current);
					yield return new KeyValuePair<int, SectionInstanceInfo>(sectionInstanceInfoFromRow.ID, sectionInstanceInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetShipSectionInstance, current2.ToSQLiteValue()), true))
					{
						SectionInstanceInfo sectionInstanceInfoFromRow2 = this.GetSectionInstanceInfoFromRow(current3);
						yield return new KeyValuePair<int, SectionInstanceInfo>(sectionInstanceInfoFromRow2.ID, sectionInstanceInfoFromRow2);
					}
				}
			}
			yield break;
		}
		protected override int OnInsert(SQLiteConnection db, int? key, SectionInstanceInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "SectionInstanceInfo insertion does not allow explicit specification of an ID.");
			}
			return db.ExecuteIntegerQuery(string.Format(Queries.InsertSectionInstance, new object[]
			{
				value.SectionID.ToSQLiteValue(),
				value.ShipID.ToNullableSQLiteValue(),
				value.StationID.ToNullableSQLiteValue(),
				value.Structure.ToSQLiteValue(),
				value.Supply.ToSQLiteValue(),
				value.Crew.ToSQLiteValue(),
				value.Signature.ToSQLiteValue(),
				value.RepairPoints.ToSQLiteValue(),
				string.Empty
			}));
		}
		protected override void OnUpdate(SQLiteConnection db, int key, SectionInstanceInfo value)
		{
			if (!value.ShipID.HasValue)
			{
				throw new ArgumentException("Exactly one of section.ShipID or section.StationID must have a value.");
			}
			db.ExecuteNonQuery(string.Format(Queries.UpdateSectionInstance, new object[]
			{
				key.ToSQLiteValue(),
				value.SectionID.ToSQLiteValue(),
				value.ShipID.ToNullableSQLiteValue(),
				value.StationID.ToNullableSQLiteValue(),
				value.Structure.ToSQLiteValue(),
				value.Supply.ToSQLiteValue(),
				value.Crew.ToSQLiteValue(),
				value.Signature.ToSQLiteValue(),
				value.RepairPoints.ToSQLiteValue()
			}), false, true);
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteNonQuery(string.Format(Queries.RemoveSectionInstance, key.ToSQLiteValue()), false, true);
		}
	}
}
