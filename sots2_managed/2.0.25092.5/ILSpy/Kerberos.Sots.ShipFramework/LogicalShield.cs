using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalShield
	{
		public enum ShieldType
		{
			DEFLECTOR_SHIELD,
			DISRUPTOR_SHIELD,
			SHIELD_MK_I,
			SHIELD_MK_II,
			SHIELD_MK_III,
			SHIELD_MK_IV,
			STRUCTURAL_FIELDS,
			RECHARGERS,
			MESON_SHIELD,
			GRAV_SHIELD,
			SHIELD_PROJECTORS,
			FOCUSED_SHIELDING,
			PSI_SHIELD
		}
		public string Name = "";
		public string TechID = "";
		public LogicalShield.ShieldType Type;
		public ShieldData CRShieldData = new ShieldData();
		public ShieldData DNShieldData = new ShieldData();
	}
}
