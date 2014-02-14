using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Framework
{
	internal class BidirMap<TForwardKey, TReverseKey>
	{
		public readonly Dictionary<TForwardKey, TReverseKey> Forward = new Dictionary<TForwardKey, TReverseKey>();
		public readonly Dictionary<TReverseKey, TForwardKey> Reverse = new Dictionary<TReverseKey, TForwardKey>();
		public void Insert(TForwardKey f, TReverseKey r)
		{
			this.Forward[f] = r;
			this.Reverse[r] = f;
		}
		public void Remove(TForwardKey f, TReverseKey r)
		{
			this.Forward.Remove(f);
			this.Reverse.Remove(r);
		}
		public void Clear()
		{
			this.Forward.Clear();
			this.Reverse.Clear();
		}
	}
}
