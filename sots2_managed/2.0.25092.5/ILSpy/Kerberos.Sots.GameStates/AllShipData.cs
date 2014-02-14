using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class AllShipData
	{
		public readonly Ring<FactionShipData> Factions = new Ring<FactionShipData>();
		public FactionShipData GetCurrentFactionShipData()
		{
			return this.Factions.Current;
		}
		public ClassShipData GetCurrentClassShipData()
		{
			FactionShipData current = this.Factions.Current;
			if (current != null)
			{
				return current.SelectedClass;
			}
			return null;
		}
		public SectionTypeShipData GetCurrentSectionTypeShipData(ShipSectionType sectionType)
		{
			return this.Factions.Current.SelectedClass.SectionTypes.FirstOrDefault((SectionTypeShipData x) => x.SectionType == sectionType);
		}
		public IEnumerable<SectionShipData> GetCurrentSections()
		{
			SectionShipData currentSectionData = this.GetCurrentSectionData(ShipSectionType.Mission);
			if (currentSectionData != null)
			{
				yield return currentSectionData;
			}
			SectionShipData currentSectionData2 = this.GetCurrentSectionData(ShipSectionType.Command);
			if (currentSectionData2 != null)
			{
				yield return currentSectionData2;
			}
			SectionShipData currentSectionData3 = this.GetCurrentSectionData(ShipSectionType.Engine);
			if (currentSectionData3 != null)
			{
				yield return currentSectionData3;
			}
			yield break;
		}
		public SectionShipData GetCurrentSectionData(ShipSectionType sectionType)
		{
			SectionTypeShipData currentSectionTypeShipData = this.GetCurrentSectionTypeShipData(sectionType);
			if (currentSectionTypeShipData == null)
			{
				return null;
			}
			return currentSectionTypeShipData.SelectedSection;
		}
		public ShipSectionAsset GetCurrentSection(ShipSectionType sectionType)
		{
			SectionShipData currentSectionData = this.GetCurrentSectionData(sectionType);
			if (currentSectionData == null)
			{
				return null;
			}
			return currentSectionData.Section;
		}
		public string GetCurrentSectionAssetName(ShipSectionType sectionType)
		{
			ShipSectionAsset currentSection = this.GetCurrentSection(sectionType);
			if (currentSection != null)
			{
				return currentSection.FileName;
			}
			return string.Empty;
		}
		public string GetCurrentSectionName(ShipSectionType sectionType)
		{
			ShipSectionAsset currentSection = this.GetCurrentSection(sectionType);
			if (currentSection != null)
			{
				return App.Localize(currentSection.Title);
			}
			return "unknown";
		}
		public string GetCurrentFactionName()
		{
			if (this.Factions.Current != null)
			{
				return this.Factions.Current.Faction.Name;
			}
			return "unknown";
		}
		public Faction GetCurrentFaction()
		{
			if (this.Factions.Current != null)
			{
				return this.Factions.Current.Faction;
			}
			return null;
		}
		public ModuleShipData GetCurrentModuleMount(ShipSectionAsset section, string mountNodeName)
		{
			SectionShipData sectionShipData = this.GetCurrentSections().FirstOrDefault((SectionShipData x) => x.Section == section);
			if (sectionShipData == null)
			{
				return null;
			}
			return sectionShipData.Modules.FirstOrDefault((ModuleShipData x) => x.ModuleMount.NodeName == mountNodeName);
		}
		public WeaponBankShipData GetCurrentWeaponBank(GameDatabase db, WeaponBankInfo bankInfo)
		{
			foreach (SectionShipData current in this.GetCurrentSections())
			{
				foreach (WeaponBankShipData current2 in current.WeaponBanks)
				{
					if (bankInfo.BankGUID == current2.Bank.GUID)
					{
						return current2;
					}
				}
			}
			return null;
		}
		public IWeaponShipData GetCurrentWeaponBank(WeaponBank bank)
		{
			foreach (SectionShipData current in this.GetCurrentSections())
			{
				foreach (WeaponBankShipData current2 in current.WeaponBanks)
				{
					if (current2.Bank == bank.LogicalBank)
					{
						IWeaponShipData result = current2;
						return result;
					}
				}
				if (bank.Module != null)
				{
					foreach (ModuleShipData current3 in current.Modules)
					{
						if (current3.SelectedModule != null && bank.Module.Attachment == current3.ModuleMount)
						{
							LogicalBank[] banks = current3.SelectedModule.Module.Banks;
							for (int i = 0; i < banks.Length; i++)
							{
								LogicalBank logicalBank = banks[i];
								if (logicalBank == bank.LogicalBank)
								{
									IWeaponShipData result = current3.SelectedModule;
									return result;
								}
							}
						}
					}
				}
			}
			return null;
		}
		public IEnumerable<WeaponBankShipData> GetCurrentWeaponBanks(ShipSectionType sectionType)
		{
			SectionShipData currentSectionData = this.GetCurrentSectionData(sectionType);
			if (currentSectionData != null)
			{
				foreach (WeaponBankShipData current in currentSectionData.WeaponBanks)
				{
					yield return current;
				}
			}
			yield break;
		}
		public IEnumerable<WeaponBankShipData> GetCurrentWeaponBanks()
		{
			foreach (WeaponBankShipData current in this.GetCurrentWeaponBanks(ShipSectionType.Command))
			{
				yield return current;
			}
			foreach (WeaponBankShipData current2 in this.GetCurrentWeaponBanks(ShipSectionType.Mission))
			{
				yield return current2;
			}
			foreach (WeaponBankShipData current3 in this.GetCurrentWeaponBanks(ShipSectionType.Engine))
			{
				yield return current3;
			}
			yield break;
		}
		private IEnumerable<WeaponAssignment> GetWeaponAssignments(ShipSectionType sectionType)
		{
			foreach (WeaponBankShipData current in this.GetCurrentWeaponBanks(sectionType))
			{
				yield return new WeaponAssignment
				{
					ModuleNode = "",
					Bank = current.Bank,
					Weapon = current.SelectedWeapon,
					DesignID = current.SelectedDesign,
					InitialFireMode = current.FiringMode,
					InitialTargetFilter = current.FilterMode
				};
			}
			foreach (ModuleShipData current2 in this.GetCurrentSectionModules())
			{
				if (current2.SelectedModule != null && current2.SelectedModule.SelectedWeapon != null)
				{
					yield return new WeaponAssignment
					{
						ModuleNode = current2.ModuleMount.NodeName,
						Bank = current2.SelectedModule.Module.Banks[0],
						Weapon = current2.SelectedModule.SelectedWeapon,
						DesignID = current2.SelectedModule.SelectedDesign
					};
				}
			}
			yield break;
		}
		public IEnumerable<WeaponAssignment> GetWeaponAssignments()
		{
			foreach (WeaponAssignment current in this.GetWeaponAssignments(ShipSectionType.Command))
			{
				yield return current;
			}
			foreach (WeaponAssignment current2 in this.GetWeaponAssignments(ShipSectionType.Mission))
			{
				yield return current2;
			}
			foreach (WeaponAssignment current3 in this.GetWeaponAssignments(ShipSectionType.Engine))
			{
				yield return current3;
			}
			yield break;
		}
		public RealShipClasses? GetCurrentClass()
		{
			FactionShipData current = this.Factions.Current;
			if (current == null)
			{
				return null;
			}
			ClassShipData selectedClass = current.SelectedClass;
			if (selectedClass == null)
			{
				return null;
			}
			return new RealShipClasses?(selectedClass.Class);
		}
		private IEnumerable<ModuleAssignment> GetModuleAssignments(ShipSectionType sectionType)
		{
			IEnumerable<ModuleShipData> currentSectionModules = this.GetCurrentSectionModules(sectionType);
			foreach (ModuleShipData current in currentSectionModules)
			{
				if (current.SelectedModule != null)
				{
					yield return new ModuleAssignment
					{
						ModuleMount = current.ModuleMount,
						Module = current.SelectedModule.Module,
						PsionicAbilities = current.SelectedModule.SelectedPsionic.ToArray()
					};
				}
			}
			yield break;
		}
		public IEnumerable<ModuleAssignment> GetModuleAssignments()
		{
			foreach (ModuleAssignment current in this.GetModuleAssignments(ShipSectionType.Command))
			{
				yield return current;
			}
			foreach (ModuleAssignment current2 in this.GetModuleAssignments(ShipSectionType.Mission))
			{
				yield return current2;
			}
			foreach (ModuleAssignment current3 in this.GetModuleAssignments(ShipSectionType.Engine))
			{
				yield return current3;
			}
			yield break;
		}
		public IEnumerable<ModuleShipData> GetCurrentSectionModules(ShipSectionType sectionType)
		{
			SectionShipData currentSectionData = this.GetCurrentSectionData(sectionType);
			if (currentSectionData != null)
			{
				foreach (ModuleShipData current in currentSectionData.Modules)
				{
					yield return current;
				}
			}
			yield break;
		}
		public IEnumerable<ModuleShipData> GetCurrentSectionModules()
		{
			foreach (ModuleShipData current in this.GetCurrentSectionModules(ShipSectionType.Command))
			{
				yield return current;
			}
			foreach (ModuleShipData current2 in this.GetCurrentSectionModules(ShipSectionType.Mission))
			{
				yield return current2;
			}
			foreach (ModuleShipData current3 in this.GetCurrentSectionModules(ShipSectionType.Engine))
			{
				yield return current3;
			}
			yield break;
		}
		public bool IsCurrentShipDataValid()
		{
			return this.GetCurrentSection(ShipSectionType.Mission) != null;
		}
	}
}
