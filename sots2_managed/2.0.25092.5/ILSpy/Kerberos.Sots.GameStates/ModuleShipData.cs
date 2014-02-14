using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class ModuleShipData
	{
		public SectionShipData Section;
		public int ModuleIndex;
		public LogicalModuleMount ModuleMount;
		public readonly List<ModuleData> Modules = new List<ModuleData>();
		public static bool DebugAutoAssignModules;
		private ModuleData _selected;
		public ModuleData SelectedModule
		{
			get
			{
				if (ModuleShipData.DebugAutoAssignModules && this._selected == null)
				{
					return this.Modules.FirstOrDefault<ModuleData>();
				}
				return this._selected;
			}
			set
			{
				this._selected = value;
			}
		}
	}
}
