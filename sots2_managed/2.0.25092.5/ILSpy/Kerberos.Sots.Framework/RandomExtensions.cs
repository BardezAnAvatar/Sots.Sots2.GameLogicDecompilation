using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Framework
{
	public static class RandomExtensions
	{
		public static float NextSingle(this Random random)
		{
			return (float)random.NextDouble();
		}
		public static int NextInclusive(this Random random, Range<int> range)
		{
			return random.NextInclusive(range.Min, range.Max);
		}
		public static float NextInclusive(this Random random, Range<float> range)
		{
			return random.NextInclusive(range.Min, range.Max);
		}
		public static double NextInclusive(this Random random, Range<double> range)
		{
			return random.NextInclusive(range.Min, range.Max);
		}
		public static int NextInclusive(this Random random, int minValue, int maxValue)
		{
			return random.Next(minValue, maxValue + 1);
		}
		public static double NextInclusive(this Random random, double minValue, double maxValue)
		{
			return ScalarExtensions.Lerp(minValue, maxValue, random.NextDouble());
		}
		public static float NextInclusive(this Random random, float minValue, float maxValue)
		{
			return ScalarExtensions.Lerp(minValue, maxValue, (float)random.NextDouble());
		}
		public static T Choose<T>(this Random random, IList<T> choices)
		{
			if (choices.Count == 0)
			{
				throw new InvalidOperationException("Cannot choose item from an empty list.");
			}
			int index = random.Next(choices.Count);
			return choices[index];
		}
		public static T Choose<T>(this Random random, IEnumerable<T> choices)
		{
			if (!choices.Any<T>())
			{
				throw new InvalidOperationException("Cannot choose item from an empty enumeration.");
			}
			int index = random.Next(choices.Count<T>());
			return choices.ElementAt(index);
		}
		public static bool CoinToss(this Random random, double odds)
		{
			if (odds <= 0.0)
			{
				return false;
			}
			if (odds >= 1.0)
			{
				return true;
			}
			double num = random.NextDouble();
			return num <= odds;
		}
		public static bool CoinToss(this Random random, int odds)
		{
			if (odds <= 0)
			{
				return false;
			}
			if (odds >= 100)
			{
				return true;
			}
			int num = random.NextInclusive(0, 100);
			return num <= odds;
		}
		public static double NextNormal(this Random random)
		{
			double num;
			double num3;
			do
			{
				num = 2.0 * random.NextDouble() - 1.0;
				double num2 = 2.0 * random.NextDouble() - 1.0;
				num3 = num * num + num2 * num2;
			}
			while (num3 >= 1.0);
			num3 = Math.Sqrt(-2.0 * Math.Log(num3) / num3);
			return num * num3;
		}
		public static double NextNormal(this Random random, double min, double max)
		{
			double mean = (min + max) / 2.0;
			return random.NextNormal(min, max, mean);
		}
		public static double NextNormal(this Random random, double min, double max, double mean)
		{
			int num = 3;
			double num2 = (max - mean) / (double)num;
			double num3;
			do
			{
				num3 = num2 * random.NextNormal() + mean;
			}
			while (num3 < min || num3 > max);
			return num3;
		}
		public static double NextNormal(this Random random, Range<double> range)
		{
			return random.NextNormal(range.Min, range.Max);
		}
		public static float NextNormal(this Random random, Range<float> range)
		{
			return (float)random.NextNormal((double)range.Min, (double)range.Max);
		}
		public static Vector3 PointInSphere(this Random random, float radius)
		{
			Vector3 result;
			do
			{
				float x = random.NextInclusive(-radius, radius);
				float y = random.NextInclusive(-radius, radius);
				float z = random.NextInclusive(-radius, radius);
				result = new Vector3(x, y, z);
			}
			while (result.Length > radius);
			return result;
		}
	}
}
