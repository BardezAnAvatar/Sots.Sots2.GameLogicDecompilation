using System;
using System.Collections;
using System.Collections.Generic;
namespace Kerberos.Sots.Framework
{
	internal class EmptyEnumerable<T> : IEnumerable<T>, IEnumerable
	{
		private class EmptyEnumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			public static readonly EmptyEnumerable<T>.EmptyEnumerator Default = new EmptyEnumerable<T>.EmptyEnumerator();
			public T Current
			{
				get
				{
					throw new InvalidOperationException("There is never any Current value for an EmptyEnumerator.");
				}
			}
			object IEnumerator.Current
			{
				get
				{
					throw new InvalidOperationException("There is never any Current value for an EmptyEnumerator.");
				}
			}
			public void Dispose()
			{
			}
			public bool MoveNext()
			{
				return false;
			}
			public void Reset()
			{
			}
		}
		public static readonly EmptyEnumerable<T> Default = new EmptyEnumerable<T>();
		public IEnumerator<T> GetEnumerator()
		{
			return EmptyEnumerable<T>.EmptyEnumerator.Default;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return EmptyEnumerable<T>.EmptyEnumerator.Default;
		}
	}
}
