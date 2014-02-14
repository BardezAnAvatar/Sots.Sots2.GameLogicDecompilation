using System;
namespace Kerberos.Sots.Framework
{
	public struct Weighted<T>
	{
		public T Value;
		public int Weight;
		public Weighted(T value, int weight)
		{
			this.Value = value;
			this.Weight = weight;
		}
	}
}
