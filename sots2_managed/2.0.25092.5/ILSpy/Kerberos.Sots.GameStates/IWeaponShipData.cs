using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal interface IWeaponShipData
	{
		List<LogicalWeapon> Weapons
		{
			get;
		}
		List<int> Designs
		{
			get;
		}
		LogicalBank Bank
		{
			get;
		}
		LogicalWeapon SelectedWeapon
		{
			get;
			set;
		}
		int SelectedDesign
		{
			get;
			set;
		}
		bool RequiresDesign
		{
			get;
			set;
		}
		bool DesignIsSelectable
		{
			get;
			set;
		}
		int? FiringMode
		{
			get;
			set;
		}
		int? FilterMode
		{
			get;
			set;
		}
	}
}
