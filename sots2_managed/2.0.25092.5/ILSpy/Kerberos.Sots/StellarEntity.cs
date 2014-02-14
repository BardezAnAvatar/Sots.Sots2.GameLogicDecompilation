using Kerberos.Sots.Data.StarMapFramework;
using System;
namespace Kerberos.Sots
{
	internal class StellarEntity : IStellarEntity
	{
		public Kerberos.Sots.Data.StarMapFramework.Orbit Params
		{
			get;
			set;
		}
		public int ID
		{
			get;
			set;
		}
		public Orbit Orbit
		{
			get;
			set;
		}
	}
}
