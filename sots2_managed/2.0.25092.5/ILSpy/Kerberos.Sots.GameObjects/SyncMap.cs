using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameObjects
{
	internal class SyncMap<TObject, TInfo, TContext> : BidirMap<TObject, int> where TObject : IGameObject where TInfo : IIDProvider where TContext : class
	{
		private readonly Func<GameObjectSet, TInfo, TContext, TObject> _create;
		private readonly Action<TObject, TInfo, TContext> _update;
		public SyncMap(Func<GameObjectSet, TInfo, TContext, TObject> create, Action<TObject, TInfo, TContext> update)
		{
			this._create = create;
			this._update = update;
		}
		public IEnumerable<TObject> Sync(GameObjectSet gos, IEnumerable<TInfo> all, TContext context, bool updateOnCreate = false)
		{
			List<TObject> list = new List<TObject>();
			HashSet<int> hashSet = new HashSet<int>(this.Forward.Values);
			foreach (TInfo current in all)
			{
				hashSet.Remove(current.ID);
				TObject tObject;
				if (this.Reverse.TryGetValue(current.ID, out tObject))
				{
					if (this._update != null)
					{
						this._update(tObject, current, context);
					}
				}
				else
				{
					tObject = this._create(gos, current, context);
					base.Insert(tObject, current.ID);
					list.Add(tObject);
					if (updateOnCreate && this._update != null)
					{
						this._update(tObject, current, context);
					}
				}
			}
			foreach (int current2 in hashSet)
			{
				TObject tObject2 = this.Reverse[current2];
				gos.Remove(tObject2);
				gos.App.ReleaseObject(tObject2);
				base.Remove(tObject2, current2);
			}
			return list;
		}
	}
}
