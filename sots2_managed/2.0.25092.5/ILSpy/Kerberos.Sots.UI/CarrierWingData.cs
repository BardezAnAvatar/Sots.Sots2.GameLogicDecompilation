using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class CarrierWingData
	{
		public List<int> SlotIndexes = new List<int>();
		public WeaponEnums.TurretClasses Class;
		public BattleRiderTypes DefaultType;
	}
}
