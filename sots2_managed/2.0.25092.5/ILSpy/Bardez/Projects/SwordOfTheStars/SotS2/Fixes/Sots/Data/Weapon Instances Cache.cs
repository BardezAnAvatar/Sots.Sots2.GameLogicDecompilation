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
    /// <summary>Contains performance enhancements to Kerberos.Sots.Data.WeaponInstancesCache</summary>
	internal class WeaponInstancesCache
    {
        #region Fields
        /// <summary>Original Kerberos instance to extend</summary>
        protected Original.WeaponInstancesCache BaseInstance;
        #endregion


        #region Kerberos exposed members
        #region Fields
        protected SQLiteConnection db
        {
            get { return ReflectionHelper.PrivateField<Original.RowCache<Int32, WeaponInstanceInfo>, SQLiteConnection>(this.BaseInstance, "db"); }
        }

        protected Dictionary<Int32, WeaponInstanceInfo> items
        {
            get { return ReflectionHelper.PrivateField<Original.RowCache<Int32, WeaponInstanceInfo>, Dictionary<Int32, WeaponInstanceInfo>>(this.BaseInstance, "items"); }
        }
        #endregion


        #region Methods
        protected void SynchronizeWithDatabase()
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.WeaponInstancesCache>("SynchronizeWithDatabase");
            mi.Invoke(this.BaseInstance, null);
        }

        protected int OnInsert(SQLiteConnection db, int? key, WeaponInstanceInfo value)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.WeaponInstancesCache>("OnInsert");
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
        public WeaponInstancesCache(Original.WeaponInstancesCache cache)
        {
            this.BaseInstance = cache;
        }
        #endregion


        #region Performance Enhancements
        /// <summary>Performance enhancement to bulk insert WeaponInstanceInfo</summary>
        /// <param name="weaponsToInsert">Collection of WeaponInstanceInfo to insert</param>
        public void BulkInsert(IList<WeaponInstanceInfo> weaponsToInsert)
        {
            this.SynchronizeWithDatabase();

            List<Int32> keysToSync = new List<Int32>();

            foreach (WeaponInstanceInfo weapon in weaponsToInsert)
            {
                Int32 weaponId = this.OnInsert(this.db, null, weapon);
                this.items[weaponId] = weapon;
                keysToSync.Add(weaponId);
                weapon.ID = weaponId;   //populate the key generated
            }
            
            this.SyncRange(keysToSync);

            //I would expect there to be a final SynchronizeWithDatabase call, but there isn't.
        }
        #endregion
    }
}