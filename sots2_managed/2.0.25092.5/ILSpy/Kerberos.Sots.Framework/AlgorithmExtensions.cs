using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Framework
{
	internal static class AlgorithmExtensions
	{
		public static void DistributePercentages<E>(ref Dictionary<E, float> ratios, E lockedVar, float newValue)
		{
			float num = 1f;
			bool flag = false;
			if (ratios.ContainsKey(lockedVar))
			{
				if (ratios.Count == 1)
				{
					ratios[lockedVar] = 1f;
					return;
				}
				num = 1f - ratios[lockedVar];
				ratios.Remove(lockedVar);
				flag = true;
			}
			Dictionary<E, float> dictionary = new Dictionary<E, float>(ratios);
			foreach (E current in dictionary.Keys)
			{
				ratios[current] = ((num == 0f) ? 0f : (ratios[current] / num));
			}
			num = 1f - newValue;
			float num2 = num * (1f - ratios.Sum((KeyValuePair<E, float> x) => x.Value));
			foreach (E current2 in dictionary.Keys)
			{
				Dictionary<E, float> dictionary2;
				E key;
				(dictionary2 = ratios)[key = current2] = dictionary2[key] * num;
				Dictionary<E, float> dictionary3;
				E key2;
				(dictionary3 = ratios)[key2 = current2] = dictionary3[key2] + num2 / (float)ratios.Keys.Count;
			}
			if (flag)
			{
				ratios.Add(lockedVar, newValue);
			}
		}
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
		{
			T[] array = source.ToArray<T>();
			for (int i = array.Length - 1; i > 0; i--)
			{
				int num = rng.Next(i + 1);
				T t = array[i];
				array[i] = array[num];
				array[num] = t;
			}
			try
			{
				T[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					T t2 = array2[j];
					yield return t2;
				}
			}
			finally
			{
			}
			yield break;
		}
		public static void Shuffle<T>(this IList<T> list)
		{
			Random random = new Random();
			int i = list.Count;
			while (i > 1)
			{
				i--;
				int index = random.Next(i + 1);
				T value = list[index];
				list[index] = list[i];
				list[i] = value;
			}
		}
		public static bool ExistsFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T target)
		{
			foreach (T current in source)
			{
				if (predicate(current))
				{
					target = current;
					return true;
				}
			}
			target = default(T);
			return false;
		}
		public static IEnumerable<T> IntersectSet<T>(this IEnumerable<T> first, ISet<T> second)
		{
			foreach (T current in first)
			{
				if (second.Contains(current))
				{
					yield return current;
				}
			}
			yield break;
		}
		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
		{
			if (val.CompareTo(min) < 0)
			{
				return min;
			}
			if (val.CompareTo(max) > 0)
			{
				return max;
			}
			return val;
		}
		public static float Normalize(this float val, float min, float max)
		{
			if (val > max)
			{
				return 1f;
			}
			if (val < min)
			{
				return 0f;
			}
			float num = max - min;
			return (val - min) / num;
		}
	}
}
