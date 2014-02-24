﻿using System;
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
    /// <summary>Contains performance enhancements to Kerberos.Sots.Data.SectionInstancesCache</summary>
	internal class SectionInstancesCache
    {
        #region Fields
        /// <summary>Original Kerberos instance to extend</summary>
        protected Original.SectionInstancesCache BaseInstance;
        #endregion


        #region Kerberos exposed members
        #region Fields
        protected SQLiteConnection db
        {
            get { return ReflectionHelper.PrivateField<Original.RowCache<Int32, SectionInstanceInfo>, SQLiteConnection>(this.BaseInstance, "db"); }
        }

        protected Dictionary<Int32, SectionInstanceInfo> items
        {
            get { return ReflectionHelper.PrivateField<Original.RowCache<Int32, SectionInstanceInfo>, Dictionary<Int32, SectionInstanceInfo>>(this.BaseInstance, "items"); }
        }
        #endregion


        #region Methods
        protected void SynchronizeWithDatabase()
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.SectionInstancesCache>("SynchronizeWithDatabase");
            mi.Invoke(this.BaseInstance, null);
        }

        protected int OnInsert(SQLiteConnection db, int? key, SectionInstanceInfo value)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.SectionInstancesCache>("OnInsert");
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
        public SectionInstancesCache(Original.SectionInstancesCache cache)
        {
            this.BaseInstance = cache;
        }
        #endregion


        #region Performance Enhancements
        /// <summary>Performance enhancement to bulk insert ShipInfos</summary>
        /// <param name="sectionsToInsert">Collection of SectionInstanceInfo to insert</param>
        public void BulkInsert(IList<SectionInstanceInfo> sectionsToInsert)
        {
            this.SynchronizeWithDatabase();

            List<Int32> keysToSync = new List<Int32>();

            foreach (SectionInstanceInfo section in sectionsToInsert)
            {
                Int32 sectionId = this.OnInsert(this.db, null, section);
                this.items[sectionId] = section;
                keysToSync.Add(sectionId);
                section.ID = sectionId;   //populate the key generated
            }
            
            this.SyncRange(keysToSync);

            //I would expect there to be a final SynchronizeWithDatabase call, but there isn't.
        }
        #endregion
    }
}