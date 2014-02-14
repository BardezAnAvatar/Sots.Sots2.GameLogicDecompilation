using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class WeaponAssignment
	{
		public LogicalBank Bank;
		public LogicalWeapon Weapon;
		public string ModuleNode;
		public int DesignID;
		public int? InitialTargetFilter;
		public int? InitialFireMode;
	}
}
