using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.SQLite;

using Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Additions;
using Bardez.Projects.SwordOfTheStars.SotS2.Utility;

using Original = Kerberos.Sots.Data;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data
{
    /// <summary>Contains performance enhancements to Kerberos.Sots.Data.ModuleInstancesCache</summary>
	internal class ModuleInstancesCache
    {
        #region Fields
        /// <summary>Original Kerberos instance to extend</summary>
        protected Original.ModuleInstancesCache BaseInstance;
        #endregion


        #region Kerberos exposed members
        #region Fields
        protected SQLiteConnection db
        {
            get { return ReflectionHelper.PrivateField<Original.RowCache<Int32, ModuleInstanceInfo>, SQLiteConnection>(this.BaseInstance, "db"); }
        }

        protected Dictionary<Int32, ModuleInstanceInfo> items
        {
            get { return ReflectionHelper.PrivateField<Original.RowCache<Int32, ModuleInstanceInfo>, Dictionary<Int32, ModuleInstanceInfo>>(this.BaseInstance, "items"); }
        }
        #endregion


        #region Methods
        protected void SynchronizeWithDatabase()
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.ModuleInstancesCache>("SynchronizeWithDatabase");
            mi.Invoke(this.BaseInstance, null);
        }

        protected int OnInsert(SQLiteConnection db, int? key, ModuleInstanceInfo value)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.ModuleInstancesCache>("OnInsert");
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
        public ModuleInstancesCache(Original.ModuleInstancesCache cache)
        {
            this.BaseInstance = cache;
        }
        #endregion


        #region Performance Enhancements
        /// <summary>Performance enhancement to bulk insert ModuleInstanceInfo</summary>
        /// <param name="modulesToInsert">Collection of ModuleInstanceInfo to insert</param>
        public void BulkInsert(IList<ModuleInstanceInfo> modulesToInsert)
        {
            this.SynchronizeWithDatabase();

            List<Int32> keysToSync = new List<Int32>();

            foreach (ModuleInstanceInfo section in modulesToInsert)
            {
                Int32 moduleId = this.OnInsert(this.db, null, section);
                this.items[moduleId] = section;
                keysToSync.Add(moduleId);
                section.ID = moduleId;   //populate the key generated
            }
            
            this.SyncRange(keysToSync);

            //I would expect there to be a final SynchronizeWithDatabase call, but there isn't.
        }
        #endregion
    }
}