using System;
using System.Collections.Generic;
namespace Kerberos.Sots.ShipFramework
{
	internal class WeaponRangeTable
	{
		public WeaponRangeTableItem PointBlank;
		public WeaponRangeTableItem Effective;
		public WeaponRangeTableItem Maximum;
		public float PlanetRange;
		public WeaponRangeTableItem this[WeaponRanges range]
		{
			get
			{
				switch (range)
				{
				case WeaponRanges.PointBlank:
					return this.PointBlank;
				case WeaponRanges.Effective:
					return this.Effective;
				case WeaponRanges.Maximum:
					return this.Maximum;
				default:
					throw new ArgumentOutOfRangeException("range");
				}
			}
		}
		public IEnumerable<object> EnumerateScriptMessageParams()
		{
			yield return this.PointBlank.Range;
			yield return this.PointBlank.Deviation;
			yield return this.PointBlank.Damage;
			yield return this.Effective.Range;
			yield return this.Effective.Deviation;
			yield return this.Effective.Damage;
			yield return this.Maximum.Range;
			yield return this.Maximum.Deviation;
			yield return this.Maximum.Damage;
			yield return this.PlanetRange;
			yield break;
		}
	}
}
