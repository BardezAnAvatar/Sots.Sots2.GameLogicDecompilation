using System;
using System.Collections;
using System.Collections.Generic;
namespace Kerberos.Sots.Data.SQLite
{
	internal class Table : IEnumerable<Row>, IEnumerable
	{
		public Row[] Rows;
		public Row this[int index]
		{
			get
			{
				return this.Rows[index];
			}
			set
			{
				this.Rows[index] = value;
			}
		}
		public IEnumerator<Row> GetEnumerator()
		{
			return ((IEnumerable<Row>)this.Rows).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Rows.GetEnumerator();
		}
	}
}
