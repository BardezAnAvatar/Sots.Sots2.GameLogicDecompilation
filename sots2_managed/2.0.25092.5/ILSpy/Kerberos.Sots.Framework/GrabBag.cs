using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Framework
{
	internal class GrabBag<T> : IEnumerable<GrabBagItem<T>>, IEnumerable
	{
		private readonly Random _rand;
		private readonly List<GrabBagItem<T>> _items;
		public GrabBag(Random random, IEnumerable<T> items)
		{
			if (random == null)
			{
				throw new ArgumentNullException("random");
			}
			this._rand = random;
			this._items = new List<GrabBagItem<T>>(
				from x in items
				select new GrabBagItem<T>
				{
					IsTaken = false,
					Value = x
				});
		}
		public void Reset()
		{
			for (int i = 0; i < this._items.Count; i++)
			{
				GrabBagItem<T> value = this._items[i];
				value.IsTaken = false;
				this._items[i] = value;
			}
		}
		public bool Replace(T item)
		{
			int num = this._items.FindIndex((GrabBagItem<T> x) => EqualityComparer<T>.Default.Equals(x.Value, item));
			if (num == -1)
			{
				return false;
			}
			if (!this._items[num].IsTaken)
			{
				return false;
			}
			GrabBagItem<T> value = this._items[num];
			value.IsTaken = false;
			this._items[num] = value;
			return true;
		}
		public bool IsTaken(T item)
		{
			int num = this._items.FindIndex((GrabBagItem<T> x) => EqualityComparer<T>.Default.Equals(x.Value, item));
			return num != -1 && this._items[num].IsTaken;
		}
		public bool Take(T item)
		{
			int num = this._items.FindIndex((GrabBagItem<T> x) => EqualityComparer<T>.Default.Equals(x.Value, item));
			if (num == -1)
			{
				return false;
			}
			if (this._items[num].IsTaken)
			{
				return false;
			}
			GrabBagItem<T> value = this._items[num];
			value.IsTaken = true;
			this._items[num] = value;
			return true;
		}
		public T TakeRandom()
		{
			if (this._items.Count == 0)
			{
				throw new InvalidOperationException("No items to take.");
			}
			int num = this._rand.Next(this._items.Count);
			for (int i = 0; i < this._items.Count; i++)
			{
				int index = (i + num) % this._items.Count;
				if (!this._items[index].IsTaken)
				{
					GrabBagItem<T> value = this._items[index];
					value.IsTaken = true;
					this._items[index] = value;
					return this._items[index].Value;
				}
			}
			throw new InvalidOperationException("All items are taken.");
		}
		public IEnumerator<GrabBagItem<T>> GetEnumerator()
		{
			return this._items.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._items.GetEnumerator();
		}
		public IEnumerable<T> GetAvailableItems()
		{
			return 
				from x in this._items
				where !x.IsTaken
				select x into y
				select y.Value;
		}
		public IEnumerable<T> GetTakenItems()
		{
			return 
				from x in this._items
				where x.IsTaken
				select x into y
				select y.Value;
		}
	}
}
