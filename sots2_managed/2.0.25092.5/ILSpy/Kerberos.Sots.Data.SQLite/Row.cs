using System;
using System.Collections;
using System.Collections.Generic;
namespace Kerberos.Sots.Data.SQLite
{
	internal class Row : IEnumerable<string>, IEnumerable
	{
		public string[] Values;
		public string this[int index]
		{
			get
			{
				return this.Values[index];
			}
			set
			{
				this.Values[index] = value;
			}
		}
		public IEnumerator<string> GetEnumerator()
		{
			return ((IEnumerable<string>)this.Values).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Values.GetEnumerator();
		}
	}
}
