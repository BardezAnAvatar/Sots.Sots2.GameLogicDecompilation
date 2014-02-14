using System;
using System.Collections;
namespace Meebey.SmartIrc4net
{
	public class NonRfcChannel : Channel
	{
		private Hashtable _Halfops = Hashtable.Synchronized(new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer()));
		public Hashtable Halfops
		{
			get
			{
				return (Hashtable)this._Halfops.Clone();
			}
		}
		internal Hashtable UnsafeHalfops
		{
			get
			{
				return this._Halfops;
			}
		}
		internal NonRfcChannel(string name) : base(name)
		{
		}
	}
}
