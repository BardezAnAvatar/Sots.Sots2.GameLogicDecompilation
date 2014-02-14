using System;
namespace Kerberos.Sots.ShipFramework
{
	internal static class BattleRiderTypesExtensions
	{
		public static bool IsBattleRiderType(this BattleRiderTypes type)
		{
			switch (type)
			{
			case BattleRiderTypes.nodefighter:
			case BattleRiderTypes.patrol:
			case BattleRiderTypes.scout:
			case BattleRiderTypes.spinal:
			case BattleRiderTypes.escort:
			case BattleRiderTypes.interceptor:
			case BattleRiderTypes.torpedo:
			case BattleRiderTypes.battlerider:
				return true;
			}
			return false;
		}
		public static bool IsControllableBattleRider(this BattleRiderTypes type)
		{
			return type == BattleRiderTypes.Unspecified || type == BattleRiderTypes.battlerider;
		}
	}
}
