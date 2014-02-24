using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.ShipFramework;

using Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Additions;
using Bardez.Projects.SwordOfTheStars.SotS2.Utility;

using Original = Kerberos.Sots.Data;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data
{
    /// <summary>Contains performance enhancements to Kerberos.Sots.Data.SectionInstancesCache</summary>
	internal class ArmorInstancesCache
    {
        #region Fields
        /// <summary>Original Kerberos instance to extend</summary>
        protected Original.ArmorInstancesCache BaseInstance;
        #endregion


        #region Kerberos exposed members
        #region Fields
        protected SQLiteConnection db
        {
            get { return ReflectionHelper.PrivateField<Original.RowCache<Int32, Dictionary<ArmorSide, DamagePattern>>, SQLiteConnection>(this.BaseInstance, "db"); }
        }

        protected Dictionary<Int32, Dictionary<ArmorSide, DamagePattern>> items
        {
            get { return ReflectionHelper.PrivateField<Original.RowCache<Int32, Dictionary<ArmorSide, DamagePattern>>, Dictionary<Int32, Dictionary<ArmorSide, DamagePattern>>>(this.BaseInstance, "items"); }
        }
        #endregion


        #region Methods
        protected void SynchronizeWithDatabase()
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.ArmorInstancesCache>("SynchronizeWithDatabase");
            mi.Invoke(this.BaseInstance, null);
        }

        protected int OnInsert(SQLiteConnection db, int? key, Dictionary<ArmorSide, DamagePattern> value)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.ArmorInstancesCache>("OnInsert");
            Object result = mi.Invoke(this.BaseInstance, new Object[] { db, key, value });
            return (Int32)result;
        }

        public void SyncRange(IEnumerable<Int32> keys)
        {
            this.BaseInstance.SyncRange(keys);
        }
        #endregion
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="cache">Original Kerberos instance to extend</param>
        public ArmorInstancesCache(Original.ArmorInstancesCache cache)
        {
            this.BaseInstance = cache;
        }
        #endregion


        #region Performance Enhancements
        /// <summary>Performance enhancement to bulk insert ShipInfos</summary>
        /// <param name="armorToInsert">Collection of ArmorInstanceInsertion parameters to insert and their return values</param>
        public void BulkInsert(IList<ArmorInstanceInsertion> armorToInsert)
        {
            this.SynchronizeWithDatabase();

            List<Int32> keysToSync = new List<Int32>();

            foreach (ArmorInstanceInsertion armor in armorToInsert)
            {
                Int32 sectionId = armor.ShipSection.ID;
                this.OnInsert(this.db, sectionId, armor.Armor);
                this.items[sectionId] = armor.Armor;
                keysToSync.Add(sectionId);
            }
            
            this.SyncRange(keysToSync);

            //I would expect there to be a final SynchronizeWithDatabase call, but there isn't.
        }
        #endregion
    }
}