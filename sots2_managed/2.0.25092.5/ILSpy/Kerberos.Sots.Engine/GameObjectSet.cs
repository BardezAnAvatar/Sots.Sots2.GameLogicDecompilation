using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Engine
{
	internal sealed class GameObjectSet : IDisposable, IEnumerable<IGameObject>, IEnumerable
	{
		private List<IGameObject> _pending = new List<IGameObject>();
		private bool _disposed;
		public App App
		{
			get;
			set;
		}
		public IEnumerable<IGameObject> Objects
		{
			get
			{
				return this._pending;
			}
		}
		public T Add<T>(params object[] initParams) where T : IGameObject
		{
			T t = this.App.AddObject<T>(initParams);
			this._pending.Add(t);
			return t;
		}
		public void Add(IGameObject value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!this._pending.Contains(value))
			{
				this._pending.Add(value);
			}
		}
		public void Add(IEnumerable<IGameObject> range)
		{
			foreach (IGameObject current in range)
			{
				this.Add(current);
			}
		}
		public void Remove(IGameObject value)
		{
			this._pending.Remove(value);
		}
		public bool IsReady()
		{
			return this._pending.All((IGameObject x) => x.ObjectStatus != GameObjectStatus.Pending);
		}
		public bool AnyFailed()
		{
			return this._pending.Any((IGameObject x) => x.ObjectStatus == GameObjectStatus.Failed);
		}
		public GameObjectStatus CheckStatus()
		{
			foreach (IGameObject current in this._pending)
			{
				GameObjectStatus objectStatus = current.ObjectStatus;
				if (objectStatus != GameObjectStatus.Ready)
				{
					return objectStatus;
				}
			}
			return GameObjectStatus.Ready;
		}
		public void Clear(bool releaseObjects)
		{
			if (releaseObjects)
			{
				foreach (IGameObject current in this._pending)
				{
					if (current is IDisposable)
					{
						(current as IDisposable).Dispose();
					}
					else
					{
						this.App.ReleaseObject(current);
					}
				}
			}
			this._pending.Clear();
		}
		public void Dispose()
		{
			if (!this._disposed)
			{
				this.Clear(true);
				this._disposed = true;
			}
		}
		public GameObjectSet(App game)
		{
			this.App = game;
		}
		public void Activate()
		{
			foreach (IGameObject current in this._pending)
			{
				if (current is IActive)
				{
					(current as IActive).Active = true;
				}
			}
		}
		public void Deactivate()
		{
			foreach (IGameObject current in this._pending)
			{
				if (current is IActive)
				{
					(current as IActive).Active = false;
				}
			}
		}
		public IGameObject GetObjectRef(int objectID)
		{
			foreach (IGameObject current in this._pending)
			{
				if (objectID == current.ObjectID)
				{
					return current;
				}
			}
			return null;
		}
		public IEnumerator<IGameObject> GetEnumerator()
		{
			return this._pending.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._pending.GetEnumerator();
		}
	}
}
