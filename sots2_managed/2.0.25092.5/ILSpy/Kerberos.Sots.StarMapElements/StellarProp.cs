using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.StarMapElements
{
	internal sealed class StellarProp : ILegacyStarMapObject
	{
		Feature ILegacyStarMapObject.Params
		{
			get
			{
				return this.Params;
			}
		}
		public StellarBody Params
		{
			get;
			set;
		}
		public Matrix Transform
		{
			get;
			set;
		}
	}
}
