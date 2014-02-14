using Kerberos.Sots.Data.StarMapFramework;
using System;
namespace Kerberos.Sots
{
	internal interface IStellarEntity
	{
		Kerberos.Sots.Data.StarMapFramework.Orbit Params
		{
			get;
		}
		int ID
		{
			get;
			set;
		}
		Orbit Orbit
		{
			get;
			set;
		}
	}
}
