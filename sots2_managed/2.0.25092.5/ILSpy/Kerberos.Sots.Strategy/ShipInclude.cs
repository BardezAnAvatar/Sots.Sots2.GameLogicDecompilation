using System;
namespace Kerberos.Sots.Strategy
{
	internal class ShipInclude
	{
		public ShipInclusionType InclusionType
		{
			get;
			set;
		}
		public int Amount
		{
			get;
			set;
		}
		public ShipRole ShipRole
		{
			get;
			set;
		}
		public WeaponRole? WeaponRole
		{
			get;
			set;
		}
		public string Faction
		{
			get;
			set;
		}
		public string FactionExclusion
		{
			get;
			set;
		}
	}
}
