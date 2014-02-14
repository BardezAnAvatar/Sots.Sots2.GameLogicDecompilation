using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Framework
{
	public static class WeightedChoices
	{
		public static T Choose<T>(Random random, IEnumerable<Weighted<T>> weights)
		{
			if (!weights.Any<Weighted<T>>())
			{
				throw new ArgumentException("Nothing to choose.");
			}
			return WeightedChoices.Choose<T>(random.NextDouble(), weights);
		}
		public static T Choose<T>(double normalizedRoll, IEnumerable<Weighted<T>> weights)
		{
			if (!weights.Any<Weighted<T>>())
			{
				throw new ArgumentException("Nothing to choose.");
			}
			long num = 0L;
			foreach (Weighted<T> current in weights)
			{
				num += (long)current.Weight;
			}
			double num2 = Math.Max(0.0, Math.Min(normalizedRoll, 1.0));
			long num3 = (long)Math.Ceiling(num2 * (double)num);
			long num4 = 0L;
			foreach (Weighted<T> current2 in weights)
			{
				num4 += (long)current2.Weight;
				if (num3 <= num4)
				{
					return current2.Value;
				}
			}
			return weights.Last<Weighted<T>>().Value;
		}
	}
}
