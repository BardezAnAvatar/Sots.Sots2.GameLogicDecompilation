using System;
using System.Collections;
using System.Collections.Generic;
namespace Kerberos.Sots.Framework
{
	internal class Ring<T> : IEnumerable<T>, IEnumerable where T : class
	{
		private IList<T> _data;
		private int _current;
		public int Count
		{
			get
			{
				return this._data.Count;
			}
		}
		public T this[int i]
		{
			get
			{
				return this._data[i];
			}
		}
		public T Current
		{
			get
			{
				return this.GetCurrent();
			}
			set
			{
				this.SetCurrent(value);
			}
		}
		public Ring()
		{
			this._data = new List<T>();
			this._current = 0;
		}
		public void Add(T t)
		{
			this._data.Add(t);
		}
		public T GetCurrent()
		{
			T result = default(T);
			if (this._data != null && this._current >= 0 && this._current < this._data.Count)
			{
				result = this._data[this._current];
			}
			return result;
		}
		public void SetCurrent(T t)
		{
			int num = this.IndexOf(t);
			if (num < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			this._current = num;
		}
		public int IndexOf(T t)
		{
			if (this._data != null)
			{
				for (int i = 0; i < this._data.Count; i++)
				{
					if (this._data[i] == t)
					{
						return i;
					}
				}
			}
			return -1;
		}
		public T Next()
		{
			T result = default(T);
			if (this._data != null)
			{
				this._current++;
				if (this._current < 0 || this._current >= this._data.Count)
				{
					this._current = 0;
				}
				result = this.GetCurrent();
			}
			return result;
		}
		public T Prev()
		{
			T result = default(T);
			if (this._data != null)
			{
				this._current--;
				if (this._current < 0 || this._current >= this._data.Count)
				{
					this._current = this._data.Count - 1;
				}
				result = this.GetCurrent();
			}
			return result;
		}
		public IEnumerator<T> GetEnumerator()
		{
			return this._data.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._data.GetEnumerator();
		}
	}
}
