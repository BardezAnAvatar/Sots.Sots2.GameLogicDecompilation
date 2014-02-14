using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Data
{
	internal class StellarInfo : IIDProvider
	{
		public Vector3 Origin;
		public int ID
		{
			get;
			set;
		}
	}
}
