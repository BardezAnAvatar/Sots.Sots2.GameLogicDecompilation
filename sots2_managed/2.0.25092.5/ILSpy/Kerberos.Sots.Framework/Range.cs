using System;
namespace Kerberos.Sots.Framework
{
	public struct Range<T> where T : IComparable<T>
	{
		public T Min
		{
			get;
			private set;
		}
		public T Max
		{
			get;
			private set;
		}
		public Range(T min, T max)
		{
			this = default(Range<T>);
			if (min.CompareTo(max) <= 0)
			{
				this.Min = min;
				this.Max = max;
				return;
			}
			this.Min = max;
			this.Max = min;
		}
	}
}
