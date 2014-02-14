using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal class AITechStyles
	{
		public IEnumerable<AITechStyleInfo> TechStyleInfos
		{
			get;
			private set;
		}
		public IEnumerable<Tech> TechUnion
		{
			get;
			private set;
		}
		public AITechStyles(AssetDatabase assetdb, IEnumerable<AITechStyleInfo> styles)
		{
			this.TechStyleInfos = styles;
			List<Tech> list = new List<Tech>();
			foreach (Tech current in assetdb.MasterTechTree.Technologies)
			{
				foreach (AITechStyleInfo current2 in styles)
				{
					if (assetdb.MasterTechTree.GetTechFamilyEnum(current) == current2.TechFamily)
					{
						list.Add(current);
						break;
					}
				}
			}
			this.TechUnion = list;
		}
	}
}
