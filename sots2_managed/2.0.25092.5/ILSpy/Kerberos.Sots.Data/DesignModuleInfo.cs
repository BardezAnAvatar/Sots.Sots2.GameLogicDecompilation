using Kerberos.Sots.Data.ModuleFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class DesignModuleInfo : IIDProvider
	{
		public string MountNodeName;
		public int ModuleID;
		public int? WeaponID;
		public int? DesignID;
		public ModuleEnums.StationModuleType? StationModuleType;
		public List<ModulePsionicInfo> PsionicAbilities = new List<ModulePsionicInfo>();
		public DesignSectionInfo DesignSectionInfo
		{
			get;
			internal set;
		}
		public int ID
		{
			get;
			set;
		}
		public override string ToString()
		{
			string arg_45_0;
			if ((arg_45_0 = this.ID.ToString() + "," + this.MountNodeName) == null)
			{
				arg_45_0 = ((string.Empty + "," + this.StationModuleType) ?? string.Empty);
			}
			return arg_45_0;
		}
	}
}
