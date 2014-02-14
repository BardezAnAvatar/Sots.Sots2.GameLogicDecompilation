using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class TableCache<TRowKey, TRowObject> : Dictionary<TRowKey, TRowObject> where TRowObject : new()
	{
		public bool IsDirty
		{
			get;
			set;
		}
		public TableCache()
		{
			this.IsDirty = true;
		}
		public new void Clear()
		{
			if (ScriptHost.AllowConsole && !this.IsDirty)
			{
				App.Log.Trace(string.Format("{0}<{1},{2}> cleared.", base.GetType().Name, typeof(TRowKey).Name, typeof(TRowObject).Name), "data", LogLevel.Verbose);
			}
			base.Clear();
			this.IsDirty = true;
		}
		public TRowObject Find(TRowKey primaryKey)
		{
			TRowObject result;
			base.TryGetValue(primaryKey, out result);
			return result;
		}
		public void Cache(TRowKey primaryKey, TRowObject rowObject)
		{
			if (rowObject == null)
			{
				throw new ArgumentNullException("rowObject");
			}
			base[primaryKey] = rowObject;
		}
	}
}
