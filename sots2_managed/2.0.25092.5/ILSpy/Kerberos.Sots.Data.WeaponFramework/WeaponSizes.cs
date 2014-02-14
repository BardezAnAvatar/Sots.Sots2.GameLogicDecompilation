using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data.WeaponFramework
{
	public static class WeaponSizes
	{
		private static readonly Dictionary<WeaponEnums.WeaponSizes, WeaponEnums.WeaponSizes[]> AllowedMountWeaponSizeMap;
		static WeaponSizes()
		{
			WeaponSizes.AllowedMountWeaponSizeMap = new Dictionary<WeaponEnums.WeaponSizes, WeaponEnums.WeaponSizes[]>();
			Dictionary<WeaponEnums.WeaponSizes, WeaponEnums.WeaponSizes[]> arg_18_0 = WeaponSizes.AllowedMountWeaponSizeMap;
			WeaponEnums.WeaponSizes arg_18_1 = WeaponEnums.WeaponSizes.VeryLight;
			WeaponEnums.WeaponSizes[] value = new WeaponEnums.WeaponSizes[1];
			arg_18_0[arg_18_1] = value;
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.Light] = new WeaponEnums.WeaponSizes[]
			{
				WeaponEnums.WeaponSizes.VeryLight,
				WeaponEnums.WeaponSizes.Light
			};
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.Medium] = new WeaponEnums.WeaponSizes[]
			{
				WeaponEnums.WeaponSizes.Light,
				WeaponEnums.WeaponSizes.Medium
			};
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.Heavy] = new WeaponEnums.WeaponSizes[]
			{
				WeaponEnums.WeaponSizes.Light,
				WeaponEnums.WeaponSizes.Medium,
				WeaponEnums.WeaponSizes.Heavy
			};
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.VeryHeavy] = new WeaponEnums.WeaponSizes[]
			{
				WeaponEnums.WeaponSizes.Medium,
				WeaponEnums.WeaponSizes.Heavy,
				WeaponEnums.WeaponSizes.VeryHeavy
			};
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.SuperHeavy] = new WeaponEnums.WeaponSizes[]
			{
				WeaponEnums.WeaponSizes.Heavy,
				WeaponEnums.WeaponSizes.VeryHeavy,
				WeaponEnums.WeaponSizes.SuperHeavy
			};
		}
		public static int GuessNumBarrels(WeaponEnums.WeaponSizes weaponSize, WeaponEnums.WeaponSizes mountSize)
		{
			return Math.Max(1, mountSize - weaponSize + 1);
		}
		public static bool WeaponSizeFitsMount(WeaponEnums.WeaponSizes weaponSize, WeaponEnums.WeaponSizes mountSize)
		{
			return WeaponSizes.AllowedMountWeaponSizeMap[mountSize].Contains(weaponSize);
		}
	}
}
