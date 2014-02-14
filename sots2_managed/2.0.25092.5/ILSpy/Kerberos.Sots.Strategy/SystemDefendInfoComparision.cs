using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal class SystemDefendInfoComparision : IComparer<SystemDefendInfo>
	{
		public int Compare(SystemDefendInfo a, SystemDefendInfo b)
		{
			if (a == b)
			{
				return 0;
			}
			if (a.IsHomeWorld && !b.IsHomeWorld)
			{
				return -1;
			}
			if (b.IsHomeWorld && !a.IsHomeWorld)
			{
				return 1;
			}
			if (a.IsCapital && !b.IsCapital)
			{
				return -1;
			}
			if (b.IsCapital && !a.IsCapital)
			{
				return 1;
			}
			if (a.ProductionRate > b.ProductionRate)
			{
				return -1;
			}
			if (a.ProductionRate < b.ProductionRate)
			{
				return 1;
			}
			if (a.NumColonies > b.NumColonies)
			{
				return -1;
			}
			if (a.NumColonies < b.NumColonies)
			{
				return 1;
			}
			return 0;
		}
	}
}
