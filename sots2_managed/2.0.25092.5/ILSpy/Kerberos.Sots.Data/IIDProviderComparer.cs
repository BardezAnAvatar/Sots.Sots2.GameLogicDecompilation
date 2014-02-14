using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class IIDProviderComparer : Comparer<IIDProvider>, IEqualityComparer<IIDProvider>
	{
		public override int Compare(IIDProvider x, IIDProvider y)
		{
			if (x.ID < y.ID)
			{
				return -1;
			}
			if (x.ID > y.ID)
			{
				return 1;
			}
			return 0;
		}
		public bool Equals(IIDProvider x, IIDProvider y)
		{
			return x.ID == y.ID;
		}
		public int GetHashCode(IIDProvider obj)
		{
			return obj.ID;
		}
	}
}
