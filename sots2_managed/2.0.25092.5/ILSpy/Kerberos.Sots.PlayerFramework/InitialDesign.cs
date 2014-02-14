using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.PlayerFramework
{
	internal class InitialDesign
	{
		public string WeaponBiasTechFamilyID;
		public string Name;
		public string[] Sections;
		public IEnumerable<ShipSectionAsset> EnumerateShipSectionAssets(AssetDatabase assetdb, Faction faction)
		{
			try
			{
				string[] sections = this.Sections;
				string name;
				for (int i = 0; i < sections.Length; i++)
				{
					name = sections[i];
					yield return assetdb.ShipSections.First((ShipSectionAsset x) => x.Faction == faction.Name && x.SectionName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
				}
			}
			finally
			{
			}
			yield break;
		}
	}
}
