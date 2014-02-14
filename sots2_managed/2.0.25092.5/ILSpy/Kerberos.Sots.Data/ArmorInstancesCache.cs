using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal sealed class ArmorInstancesCache : RowCache<int, Dictionary<ArmorSide, DamagePattern>>
	{
		private readonly SectionInstancesCache section_instances;
		public static readonly Dictionary<ArmorSide, DamagePattern> EmptyArmorInstances;
		static ArmorInstancesCache()
		{
			ArmorInstancesCache.EmptyArmorInstances = new Dictionary<ArmorSide, DamagePattern>();
			for (int i = 0; i < 4; i++)
			{
				ArmorInstancesCache.EmptyArmorInstances.Add((ArmorSide)i, new DamagePattern(0, 0));
			}
		}
		public ArmorInstancesCache(SQLiteConnection db, AssetDatabase assets, SectionInstancesCache section_instances) : base(db, assets)
		{
			this.section_instances = section_instances;
			section_instances.RowObjectRemoving += new RowObjectDirtiedEventHandler<int>(this.section_instances_RowObjectRemoving);
		}
		private void section_instances_RowObjectRemoving(object sender, int key)
		{
			base.Remove(key);
		}
		private Dictionary<ArmorSide, DamagePattern> GetArmorInstancesFromRows(IEnumerable<Row> rows)
		{
			Dictionary<ArmorSide, DamagePattern> dictionary = new Dictionary<ArmorSide, DamagePattern>();
			foreach (Row current in rows)
			{
				dictionary.Add((ArmorSide)current[2].SQLiteValueToInteger(), DamagePattern.FromDatabaseString(current[3].SQLiteValueToString()));
			}
			for (int i = 0; i < 4; i++)
			{
				if (!dictionary.ContainsKey((ArmorSide)i))
				{
					dictionary.Add((ArmorSide)i, new DamagePattern(0, 0));
				}
			}
			return dictionary;
		}
		private IEnumerable<KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>> GetAllArmorInstancesFromRows(IEnumerable<Row> rows)
		{
			int sectionInstanceIdIndex = 1;
			return 
				from row in rows
				group row by row[sectionInstanceIdIndex].SQLiteValueToInteger() into x
				select new KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>(x.Key, this.GetArmorInstancesFromRows(x));
		}
		protected override IEnumerable<KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				Table rows = db.ExecuteTableQuery(Queries.GetAllArmorInstances, true);
				IEnumerable<KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>> allArmorInstancesFromRows = this.GetAllArmorInstancesFromRows(rows);
				foreach (KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>> current in allArmorInstancesFromRows)
				{
					yield return current;
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					Table rows2 = db.ExecuteTableQuery(string.Format(Queries.GetArmorInstances, current2.ToSQLiteValue()), true);
					Dictionary<ArmorSide, DamagePattern> armorInstancesFromRows = this.GetArmorInstancesFromRows(rows2);
					yield return new KeyValuePair<int, Dictionary<ArmorSide, DamagePattern>>(current2, armorInstancesFromRows);
				}
			}
			yield break;
		}
		protected override int OnInsert(SQLiteConnection db, int? key, Dictionary<ArmorSide, DamagePattern> value)
		{
			if (!key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Armor instance insertion requires explicit specification of an existing section instance ID.");
			}
			foreach (KeyValuePair<ArmorSide, DamagePattern> current in value)
			{
				db.ExecuteNonQuery(string.Format(Queries.InsertArmorInstance, key.Value, ((int)current.Key).ToSQLiteValue(), current.Value.ToDatabaseString()), false, true);
			}
			return key.Value;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, Dictionary<ArmorSide, DamagePattern> value)
		{
			foreach (KeyValuePair<ArmorSide, DamagePattern> current in value)
			{
				db.ExecuteNonQuery(string.Format(Queries.UpdateArmorInstance, key, ((int)current.Key).ToSQLiteValue(), current.Value.ToDatabaseString()), false, true);
			}
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
		}
	}
}
