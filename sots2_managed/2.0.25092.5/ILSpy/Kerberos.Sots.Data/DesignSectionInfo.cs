using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal class DesignSectionInfo : IIDProvider
	{
		public string FilePath;
		public ShipSectionAsset ShipSectionAsset;
		public List<WeaponBankInfo> WeaponBanks;
		public List<DesignModuleInfo> Modules;
		public List<int> Techs;
		public DesignInfo DesignInfo
		{
			get;
			set;
		}
		public int ID
		{
			get;
			set;
		}
		public int GetMinStructure(GameDatabase db, AssetDatabase ab)
		{
			int num = 0;
			if (this.Modules != null)
			{
				foreach (DesignModuleInfo current in this.Modules)
				{
					string mPath = db.GetModuleAsset(current.ModuleID);
					LogicalModule logicalModule = ab.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == mPath);
					if (logicalModule != null)
					{
						num += (int)logicalModule.StructureBonus;
					}
				}
				num = -num;
			}
			return num;
		}
	}
}
