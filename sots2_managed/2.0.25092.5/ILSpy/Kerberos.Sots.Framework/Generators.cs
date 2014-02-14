using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Framework
{
	internal static class Generators
	{
		public static IEnumerable<int> Sequence(int first, int count, int step)
		{
			int num = first;
			for (int i = 0; i < count; i++)
			{
				yield return num;
				num += step;
			}
			yield break;
		}
	}
}
