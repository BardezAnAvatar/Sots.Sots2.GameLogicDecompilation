using System;
namespace Kerberos.Sots.Framework
{
	internal struct GrabBagItem<T>
	{
		public bool IsTaken;
		public T Value;
	}
}
