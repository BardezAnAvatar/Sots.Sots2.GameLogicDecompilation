using Kerberos.Sots.Data.WeaponFramework;
using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalBank
	{
		public ShipSectionAsset Section;
		public LogicalModule Module;
		public WeaponEnums.WeaponSizes TurretSize;
		public WeaponEnums.TurretClasses TurretClass;
		public int FrameX;
		public int FrameY;
		public Guid GUID;
		public string DefaultWeaponName;
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				this.GUID,
				",",
				this.TurretClass,
				",",
				this.TurretSize
			});
		}
	}
}
