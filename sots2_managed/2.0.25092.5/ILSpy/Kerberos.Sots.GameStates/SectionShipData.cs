using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class SectionShipData
	{
		public ShipSectionAsset Section;
		public readonly List<WeaponBankShipData> WeaponBanks = new List<WeaponBankShipData>();
		public readonly List<ModuleShipData> Modules = new List<ModuleShipData>();
	}
}
