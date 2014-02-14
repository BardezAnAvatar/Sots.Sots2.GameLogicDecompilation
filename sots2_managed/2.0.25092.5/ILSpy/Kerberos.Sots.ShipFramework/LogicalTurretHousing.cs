using Kerberos.Sots.Data.WeaponFramework;
using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalTurretHousing
	{
		public WeaponEnums.WeaponSizes MountSize;
		public WeaponEnums.WeaponSizes WeaponSize;
		public WeaponEnums.TurretClasses Class;
		public float TrackSpeed;
		public string ModelName;
		public string BaseModelName;
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				this.Class,
				",",
				this.MountSize,
				",",
				this.WeaponSize
			});
		}
	}
}
