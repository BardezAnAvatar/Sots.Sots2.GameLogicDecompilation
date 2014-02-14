using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class ModuleData : IWeaponShipData
	{
		private readonly List<LogicalWeapon> _weapons = new List<LogicalWeapon>();
		private readonly List<int> _designs = new List<int>();
		public LogicalModule Module;
		private LogicalWeapon _selectedWeapon;
		private List<SectionEnumerations.PsionicAbility> _selectedPsionic;
		private int _selectedDesign;
		public LogicalBank Bank
		{
			get
			{
				return null;
			}
		}
		public bool RequiresDesign
		{
			get;
			set;
		}
		public bool DesignIsSelectable
		{
			get;
			set;
		}
		public List<LogicalWeapon> Weapons
		{
			get
			{
				return this._weapons;
			}
		}
		public List<int> Designs
		{
			get
			{
				return this._designs;
			}
		}
		public LogicalWeapon SelectedWeapon
		{
			get
			{
				LogicalWeapon arg_26_0;
				if ((arg_26_0 = this._selectedWeapon) == null)
				{
					if (this.Weapons.Count <= 0)
					{
						return null;
					}
					arg_26_0 = this.Weapons[0];
				}
				return arg_26_0;
			}
			set
			{
				this._selectedWeapon = value;
			}
		}
		public List<SectionEnumerations.PsionicAbility> SelectedPsionic
		{
			get
			{
				return this._selectedPsionic;
			}
			set
			{
				this._selectedPsionic = value;
			}
		}
		public int SelectedDesign
		{
			get
			{
				if (this._selectedDesign != 0 || this.Designs.Count <= 0)
				{
					return this._selectedDesign;
				}
				return this.Designs[0];
			}
			set
			{
				this._selectedDesign = value;
			}
		}
		public int? FiringMode
		{
			get;
			set;
		}
		public int? FilterMode
		{
			get;
			set;
		}
	}
}
