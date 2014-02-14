using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal sealed class WeaponInstancesCache : RowCache<int, WeaponInstanceInfo>
	{
		private readonly SectionInstancesCache section_instances;
		private readonly Dictionary<int, List<int>> sectionInstanceIdToWeaponInstanceIds = new Dictionary<int, List<int>>();
		private bool requires_sync = true;
		private void WeaponInstanceAdded(int sectionInstanceID, int weaponInstanceID)
		{
			List<int> list;
			if (!this.sectionInstanceIdToWeaponInstanceIds.TryGetValue(sectionInstanceID, out list))
			{
				list = new List<int>();
				this.sectionInstanceIdToWeaponInstanceIds.Add(sectionInstanceID, list);
			}
			if (!list.Contains(weaponInstanceID))
			{
				list.Add(weaponInstanceID);
			}
		}
		private void WeaponInstanceRemoved(int sectionInstanceID, int weaponInstanceID)
		{
			List<int> list;
			if (this.sectionInstanceIdToWeaponInstanceIds.TryGetValue(sectionInstanceID, out list))
			{
				list.Remove(weaponInstanceID);
				if (list.Count == 0)
				{
					this.sectionInstanceIdToWeaponInstanceIds.Remove(sectionInstanceID);
				}
			}
		}
		public WeaponInstancesCache(SQLiteConnection db, AssetDatabase assets, SectionInstancesCache section_instances) : base(db, assets)
		{
			this.section_instances = section_instances;
			section_instances.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.section_instances_RowObjectRemoving);
		}
		protected override void OnCleared()
		{
			base.OnCleared();
			this.sectionInstanceIdToWeaponInstanceIds.Clear();
		}
		private void section_instances_RowObjectRemoving(object sender, int key)
		{
			List<int> list;
			if (this.sectionInstanceIdToWeaponInstanceIds.TryGetValue(key, out list))
			{
				foreach (int current in list)
				{
					base.Remove(current);
				}
			}
		}
		public IEnumerable<WeaponInstanceInfo> EnumerateBySectionInstanceID(int value)
		{
			if (this.requires_sync)
			{
				base.SynchronizeWithDatabase();
			}
			List<int> source;
			if (this.sectionInstanceIdToWeaponInstanceIds.TryGetValue(value, out source))
			{
				return 
					from x in source
					select base[x];
			}
			return EmptyEnumerable<WeaponInstanceInfo>.Default;
		}
		private WeaponInstanceInfo GetWeaponInstanceInfoFromRow(Row row)
		{
			return new WeaponInstanceInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				SectionInstanceID = row[1].SQLiteValueToInteger(),
				WeaponID = row[2].SQLiteValueToInteger(),
				Structure = row[3].SQLiteValueToSingle(),
				MaxStructure = row[4].SQLiteValueToSingle(),
				NodeName = row[5].SQLiteValueToString(),
				ModuleInstanceID = row[6].SQLiteValueToNullableInteger()
			};
		}
		protected override IEnumerable<KeyValuePair<int, WeaponInstanceInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetAllWeaponInstances, true))
				{
					WeaponInstanceInfo weaponInstanceInfoFromRow = this.GetWeaponInstanceInfoFromRow(current);
					this.WeaponInstanceAdded(weaponInstanceInfoFromRow.SectionInstanceID, weaponInstanceInfoFromRow.ID);
					yield return new KeyValuePair<int, WeaponInstanceInfo>(weaponInstanceInfoFromRow.ID, weaponInstanceInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetWeaponInstance, current2.ToSQLiteValue()), true))
					{
						WeaponInstanceInfo weaponInstanceInfoFromRow2 = this.GetWeaponInstanceInfoFromRow(current3);
						this.WeaponInstanceAdded(weaponInstanceInfoFromRow2.SectionInstanceID, weaponInstanceInfoFromRow2.ID);
						yield return new KeyValuePair<int, WeaponInstanceInfo>(weaponInstanceInfoFromRow2.ID, weaponInstanceInfoFromRow2);
					}
				}
			}
			this.requires_sync = false;
			yield break;
		}
		protected override int OnInsert(SQLiteConnection db, int? key, WeaponInstanceInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "WeaponInstanceInfo insertion does not allow explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertWeaponInstance, new object[]
			{
				value.SectionInstanceID.ToSQLiteValue(),
				value.WeaponID.ToSQLiteValue(),
				value.Structure.ToSQLiteValue(),
				value.MaxStructure.ToSQLiteValue(),
				value.NodeName.ToSQLiteValue(),
				value.ModuleInstanceID.ToNullableSQLiteValue()
			}));
			this.WeaponInstanceAdded(value.SectionInstanceID, num);
			return num;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, WeaponInstanceInfo value)
		{
			WeaponInstanceInfo weaponInstanceInfo = base[key];
			db.ExecuteNonQuery(string.Format(Queries.UpdateWeaponInstance, key.ToSQLiteValue(), value.Structure.ToSQLiteValue()), false, true);
			if (weaponInstanceInfo.SectionInstanceID != value.SectionInstanceID)
			{
				this.WeaponInstanceRemoved(weaponInstanceInfo.SectionInstanceID, key);
				this.WeaponInstanceAdded(value.SectionInstanceID, key);
			}
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
		}
	}
}
