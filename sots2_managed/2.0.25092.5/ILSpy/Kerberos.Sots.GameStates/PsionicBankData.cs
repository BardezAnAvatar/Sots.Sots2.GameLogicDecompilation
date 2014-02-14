using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class PsionicBankData : IPsionicShipData
	{
		private readonly List<LogicalPsionic> _psionics = new List<LogicalPsionic>();
		private readonly List<int> _designs = new List<int>();
		public SectionShipData Section;
		public int BankIndex;
		private LogicalPsionic _selectedPsionic;
		private int _selectedDesign;
		public LogicalBank Bank
		{
			get;
			set;
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
		public List<LogicalPsionic> Psionics
		{
			get
			{
				return this._psionics;
			}
		}
		public List<int> Designs
		{
			get
			{
				return this._designs;
			}
		}
		public LogicalPsionic SelectedPsionic
		{
			get
			{
				LogicalPsionic arg_26_0;
				if ((arg_26_0 = this._selectedPsionic) == null)
				{
					if (this.Psionics.Count <= 0)
					{
						return null;
					}
					arg_26_0 = this.Psionics[0];
				}
				return arg_26_0;
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
