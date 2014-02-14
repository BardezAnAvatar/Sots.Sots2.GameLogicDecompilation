using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal abstract class RowCache<TRowKey, TRowObject> : IEnumerable<KeyValuePair<TRowKey, TRowObject>>, IEnumerable where TRowKey : struct where TRowObject : new()
	{
		private readonly Dictionary<TRowKey, TRowObject> items = new Dictionary<TRowKey, TRowObject>();
		private readonly HashSet<TRowKey> staleItems = new HashSet<TRowKey>();
		private readonly SQLiteConnection db;
		private bool syncfromdb = true;
		private bool syncall = true;
		public event RowObjectDirtiedEventHandler<TRowKey> RowObjectDirtied;
		public event RowObjectDirtiedEventHandler<TRowKey> RowObjectRemoving;
		protected AssetDatabase Assets
		{
			get;
			private set;
		}
		public TRowObject this[TRowKey key]
		{
			get
			{
				this.SynchronizeWithDatabase();
				return this.items[key];
			}
		}
		public IEnumerable<TRowKey> Keys
		{
			get
			{
				this.SynchronizeWithDatabase();
				return this.items.Keys;
			}
		}
		public IEnumerable<TRowObject> Values
		{
			get
			{
				this.SynchronizeWithDatabase();
				return this.items.Values;
			}
		}
		protected static void Trace(string message)
		{
			App.Log.Trace(message, "data", LogLevel.Verbose);
		}
		protected static void Warn(string message)
		{
			App.Log.Warn(message, "data");
		}
		protected void SynchronizeWithDatabase()
		{
			if (this.syncfromdb)
			{
				if (this.syncall)
				{
					if (ScriptHost.AllowConsole)
					{
						RowCache<TRowKey, TRowObject>.Trace(string.Format("{0}: Synchronizing all objects", base.GetType().Name));
					}
					using (IEnumerator<KeyValuePair<TRowKey, TRowObject>> enumerator = this.OnSynchronizeWithDatabase(this.db, null).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<TRowKey, TRowObject> current = enumerator.Current;
							this.items.Add(current.Key, current.Value);
						}
						goto IL_149;
					}
				}
				List<TRowKey> list = null;
				if (ScriptHost.AllowConsole)
				{
					list = new List<TRowKey>();
				}
				foreach (KeyValuePair<TRowKey, TRowObject> current2 in this.OnSynchronizeWithDatabase(this.db, this.staleItems))
				{
					this.items[current2.Key] = current2.Value;
					if (list != null)
					{
						list.Add(current2.Key);
					}
				}
				if (ScriptHost.AllowConsole)
				{
					foreach (TRowKey current3 in list)
					{
						RowCache<TRowKey, TRowObject>.Trace(string.Format("{0}: Synchronized object for key {1}", base.GetType().Name, current3));
					}
				}
				IL_149:
				this.staleItems.Clear();
				this.syncall = false;
				this.syncfromdb = false;
			}
		}
		public RowCache(SQLiteConnection db, AssetDatabase assets)
		{
			this.db = db;
			this.Assets = assets;
		}
		public bool ContainsKey(TRowKey key)
		{
			this.SynchronizeWithDatabase();
			return this.items.ContainsKey(key);
		}
		public IEnumerator<KeyValuePair<TRowKey, TRowObject>> GetEnumerator()
		{
			this.SynchronizeWithDatabase();
			return this.items.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			this.SynchronizeWithDatabase();
			return ((IEnumerable)this.items).GetEnumerator();
		}
		protected virtual void OnCleared()
		{
		}
		public void Clear()
		{
			this.items.Clear();
			this.syncfromdb = true;
			this.syncall = true;
			this.OnCleared();
		}
		public TRowKey Insert(TRowKey? key, TRowObject value)
		{
			this.SynchronizeWithDatabase();
			TRowKey tRowKey = this.OnInsert(this.db, key, value);
			this.items[tRowKey] = value;
			this.Sync(tRowKey);
			return tRowKey;
		}
		private void InvokeRowObjectDirtied(TRowKey key)
		{
			if (this.RowObjectDirtied != null)
			{
				this.RowObjectDirtied(this, key);
			}
		}
		public void Update(TRowKey key, TRowObject value)
		{
			this.SynchronizeWithDatabase();
			if (!this.items.ContainsKey(key))
			{
				throw new ArgumentOutOfRangeException("key", "Cannot update. No such row key exists: " + key.ToString());
			}
			this.OnUpdate(this.db, key, value);
			this.Sync(key);
		}
		private void InvokeRowObjectRemoving(TRowKey key)
		{
			if (this.RowObjectRemoving != null)
			{
				this.RowObjectRemoving(this, key);
			}
		}
		public void Remove(TRowKey key)
		{
			this.SynchronizeWithDatabase();
			if (this.items.ContainsKey(key))
			{
				this.OnRemove(this.db, key);
				this.InvokeRowObjectRemoving(key);
				this.items.Remove(key);
			}
		}
		public void Sync(TRowKey key)
		{
			this.staleItems.Add(key);
			this.syncfromdb = true;
			this.InvokeRowObjectDirtied(key);
		}
		public void SyncRange(IEnumerable<TRowKey> keys)
		{
			foreach (TRowKey current in keys)
			{
				this.Sync(current);
			}
		}
		protected abstract IEnumerable<KeyValuePair<TRowKey, TRowObject>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<TRowKey> range);
		protected abstract TRowKey OnInsert(SQLiteConnection db, TRowKey? key, TRowObject value);
		protected abstract void OnUpdate(SQLiteConnection db, TRowKey key, TRowObject value);
		protected abstract void OnRemove(SQLiteConnection db, TRowKey key);
	}
}
