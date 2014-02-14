using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Framework
{
	internal class ReadOnlyDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> _dict;
		public IEnumerable<TKey> Keys
		{
			get
			{
				return this._dict.Keys;
			}
		}
		public IEnumerable<TValue> Values
		{
			get
			{
				return this._dict.Values;
			}
		}
		public TValue this[TKey key]
		{
			get
			{
				return this._dict[key];
			}
		}
		public bool ContainsKey(TKey key)
		{
			return this._dict.ContainsKey(key);
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
			return this._dict.TryGetValue(key, out value);
		}
		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
		{
			this._dict = dictionary;
		}
	}
}
