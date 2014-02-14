using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal class StrategicTaskComparision : IComparer<StrategicTask>
	{
		public int Compare(StrategicTask alpha, StrategicTask beta)
		{
			if (alpha == beta)
			{
				return 0;
			}
			if (alpha.Score == beta.Score)
			{
				if (alpha.Mission < beta.Mission)
				{
					return -1;
				}
				if (alpha.Mission > beta.Mission)
				{
					return 1;
				}
				if (alpha.SubScore > beta.SubScore)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				if (alpha.Score > beta.Score)
				{
					return -1;
				}
				if (alpha.Score < beta.Score)
				{
					return 1;
				}
				return 0;
			}
		}
	}
}
