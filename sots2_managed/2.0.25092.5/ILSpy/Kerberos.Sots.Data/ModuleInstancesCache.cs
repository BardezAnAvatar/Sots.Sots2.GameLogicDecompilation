using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal sealed class ModuleInstancesCache : RowCache<int, ModuleInstanceInfo>
	{
		private readonly SectionInstancesCache section_instances;
		private readonly Dictionary<int, List<int>> sectionInstanceIdToModuleInstanceIds = new Dictionary<int, List<int>>();
		private bool requires_sync = true;
		private void ModuleInstanceAdded(int sectionInstanceID, int moduleInstanceID)
		{
			List<int> list;
			if (!this.sectionInstanceIdToModuleInstanceIds.TryGetValue(sectionInstanceID, out list))
			{
				list = new List<int>();
				this.sectionInstanceIdToModuleInstanceIds.Add(sectionInstanceID, list);
			}
			if (!list.Contains(moduleInstanceID))
			{
				list.Add(moduleInstanceID);
			}
		}
		private void ModuleInstanceRemoved(int sectionInstanceID, int moduleInstanceID)
		{
			List<int> list;
			if (this.sectionInstanceIdToModuleInstanceIds.TryGetValue(sectionInstanceID, out list))
			{
				list.Remove(moduleInstanceID);
				if (list.Count == 0)
				{
					this.sectionInstanceIdToModuleInstanceIds.Remove(sectionInstanceID);
				}
			}
		}
		public ModuleInstancesCache(SQLiteConnection db, AssetDatabase assets, SectionInstancesCache section_instances) : base(db, assets)
		{
			this.section_instances = section_instances;
			section_instances.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.section_instances_RowObjectRemoving);
		}
		protected override void OnCleared()
		{
			base.OnCleared();
			this.sectionInstanceIdToModuleInstanceIds.Clear();
		}
		private void section_instances_RowObjectRemoving(object sender, int key)
		{
			List<int> list;
			if (this.sectionInstanceIdToModuleInstanceIds.TryGetValue(key, out list))
			{
				foreach (int current in list)
				{
					base.Remove(current);
				}
			}
		}
		public IEnumerable<ModuleInstanceInfo> EnumerateBySectionInstanceID(int value)
		{
			if (this.requires_sync)
			{
				base.SynchronizeWithDatabase();
			}
			List<int> source;
			if (this.sectionInstanceIdToModuleInstanceIds.TryGetValue(value, out source))
			{
				return 
					from x in source
					select base[x];
			}
			return EmptyEnumerable<ModuleInstanceInfo>.Default;
		}
		private ModuleInstanceInfo GetModuleInstanceInfoFromRow(Row row)
		{
			return new ModuleInstanceInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				SectionInstanceID = row[1].SQLiteValueToInteger(),
				ModuleNodeID = row[2].SQLiteValueToString(),
				Structure = row[3].SQLiteValueToInteger(),
				RepairPoints = row[4].SQLiteValueToInteger()
			};
		}
		protected override IEnumerable<KeyValuePair<int, ModuleInstanceInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetAllModuleInstances, true))
				{
					ModuleInstanceInfo moduleInstanceInfoFromRow = this.GetModuleInstanceInfoFromRow(current);
					this.ModuleInstanceAdded(moduleInstanceInfoFromRow.SectionInstanceID, moduleInstanceInfoFromRow.ID);
					yield return new KeyValuePair<int, ModuleInstanceInfo>(moduleInstanceInfoFromRow.ID, moduleInstanceInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetModuleInstance, current2.ToSQLiteValue()), true))
					{
						ModuleInstanceInfo moduleInstanceInfoFromRow2 = this.GetModuleInstanceInfoFromRow(current3);
						this.ModuleInstanceAdded(moduleInstanceInfoFromRow2.SectionInstanceID, moduleInstanceInfoFromRow2.ID);
						yield return new KeyValuePair<int, ModuleInstanceInfo>(moduleInstanceInfoFromRow2.ID, moduleInstanceInfoFromRow2);
					}
				}
			}
			this.requires_sync = false;
			yield break;
		}
		protected override int OnInsert(SQLiteConnection db, int? key, ModuleInstanceInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "ModuleInstanceInfo insertion does not allow explicit specification of an ID.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertModuleInstance, new object[]
			{
				value.SectionInstanceID.ToSQLiteValue(),
				value.ModuleNodeID.ToSQLiteValue(),
				value.Structure.ToSQLiteValue(),
				value.RepairPoints.ToSQLiteValue()
			}));
			this.ModuleInstanceAdded(value.SectionInstanceID, num);
			return num;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, ModuleInstanceInfo value)
		{
			ModuleInstanceInfo moduleInstanceInfo = base[key];
			db.ExecuteNonQuery(string.Format(Queries.UpdateModuleInstance, new object[]
			{
				key.ToSQLiteValue(),
				value.SectionInstanceID.ToSQLiteValue(),
				value.ModuleNodeID.ToSQLiteValue(),
				value.Structure.ToSQLiteValue(),
				value.RepairPoints.ToSQLiteValue()
			}), false, true);
			if (moduleInstanceInfo.SectionInstanceID != value.SectionInstanceID)
			{
				this.ModuleInstanceRemoved(moduleInstanceInfo.SectionInstanceID, key);
				this.ModuleInstanceAdded(value.SectionInstanceID, key);
			}
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
		}
	}
}
