using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.IO;
namespace Kerberos.Sots.ShipFramework
{
	public class LogicalTurretClass
	{
		public WeaponEnums.TurretClasses TurretClass;
		public WeaponEnums.WeaponSizes TurretSize;
		public string BarrelModelName;
		public string TurretModelName;
		public string BaseModelName;
		public static LogicalTurretClass GetLogicalTurretClassForMount(IEnumerable<LogicalTurretClass> turretClasses, string defaultWeaponSize, string defaultWeaponClass, string mountSize, string mountClass)
		{
			WeaponEnums.WeaponSizes defaultWeaponSize2;
			WeaponEnums.TurretClasses defaultWeaponClass2;
			WeaponEnums.WeaponSizes mountSize2;
			WeaponEnums.TurretClasses mountClass2;
			if (!Enum.TryParse<WeaponEnums.WeaponSizes>(defaultWeaponSize, out defaultWeaponSize2) || !Enum.TryParse<WeaponEnums.TurretClasses>(defaultWeaponClass, out defaultWeaponClass2) || !Enum.TryParse<WeaponEnums.WeaponSizes>(mountSize, out mountSize2) || !Enum.TryParse<WeaponEnums.TurretClasses>(mountClass, out mountClass2))
			{
				return null;
			}
			return LogicalTurretClass.GetLogicalTurretClassForMount(turretClasses, defaultWeaponSize2, defaultWeaponClass2, mountSize2, mountClass2);
		}
		public static LogicalTurretClass GetLogicalTurretClassForMount(IEnumerable<LogicalTurretClass> turretClasses, WeaponEnums.WeaponSizes defaultWeaponSize, WeaponEnums.TurretClasses defaultWeaponClass, WeaponEnums.WeaponSizes mountSize, WeaponEnums.TurretClasses mountClass)
		{
			foreach (LogicalTurretClass current in turretClasses)
			{
				if (current.TurretClass == mountClass && current.TurretSize == mountSize)
				{
					return current;
				}
			}
			return null;
		}
		internal string GetBaseModel(Faction faction, LogicalMount mount, LogicalTurretHousing housing)
		{
			if (!string.IsNullOrEmpty(mount.BaseOverload))
			{
				return faction.GetWeaponModelPath(mount.BaseOverload);
			}
			if (!string.IsNullOrEmpty(this.BaseModelName))
			{
				return faction.GetWeaponModelPath(this.BaseModelName);
			}
			if (!string.IsNullOrEmpty(housing.BaseModelName))
			{
				return faction.GetWeaponModelPath(housing.BaseModelName);
			}
			return string.Empty;
		}
		internal string GetBaseDamageModel(Faction faction, LogicalMount mount, LogicalTurretHousing housing)
		{
			string baseModel = this.GetBaseModel(faction, mount, housing);
			if (!string.IsNullOrEmpty(baseModel))
			{
				return Path.Combine(Path.GetDirectoryName(baseModel), Path.GetFileNameWithoutExtension(baseModel) + "_Damaged" + Path.GetExtension(baseModel));
			}
			return string.Empty;
		}
		internal string GetTurretModelName(Faction faction, LogicalMount mount, LogicalTurretHousing housing)
		{
			if (!string.IsNullOrEmpty(mount.TurretOverload))
			{
				return faction.GetWeaponModelPath(mount.TurretOverload);
			}
			if (!string.IsNullOrEmpty(this.TurretModelName))
			{
				return faction.GetWeaponModelPath(this.TurretModelName);
			}
			if (!string.IsNullOrEmpty(housing.ModelName))
			{
				return faction.GetWeaponModelPath(housing.ModelName);
			}
			return faction.GetWeaponModelPath("Turret_Dummy.scene");
		}
		internal string GetBarrelModelName(Faction faction, LogicalMount mount)
		{
			if (!string.IsNullOrEmpty(mount.BarrelOverload))
			{
				return faction.GetWeaponModelPath(mount.BarrelOverload);
			}
			if (!string.IsNullOrEmpty(this.BarrelModelName))
			{
				return faction.GetWeaponModelPath(this.BarrelModelName);
			}
			return faction.GetWeaponModelPath("Turret_Dummy.scene");
		}
	}
}
