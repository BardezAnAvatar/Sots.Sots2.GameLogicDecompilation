using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalModule
	{
		public string Faction = "";
		public string ModuleName = "";
		public string ModuleTitle = "";
		public string Description = "";
		public string ModuleType = "";
		public string ModulePath = "";
		public string ModelPath = "";
		public string LowStructModelPath = "";
		public string DeadModelPath = "";
		public string AmbientSound = "";
		public ModuleEnums.ModuleAbilities AbilityType;
		public string Icon = "";
		public float LowStruct;
		public int AbilitySupply;
		public int Crew;
		public int CrewRequired;
		public int Supply;
		public float Structure;
		public float StructureBonus;
		public int ArmorBonus;
		public float ECCM;
		public float ECM;
		public int RepairPointsBonus;
		public float AccelBonus;
		public float CriticalHitBonus;
		public float AccuracyBonus;
		public float ROFBonus;
		public float CrewEfficiencyBonus;
		public float DamageBonus;
		public float SensorBonus;
		public float AdmiralSurvivalBonus;
		public float PsionicPowerBonus;
		public float PsionicStaminaBonus;
		public int PowerBonus;
		public ShipClass Class = ShipClass.BattleRider;
		public LogicalEffect DamageEffect;
		public LogicalEffect DeathEffect;
		public bool AssignByDefault;
		public int SavingsCost;
		public int UpkeepCost;
		public int ProductionCost;
		public int NumPsionicSlots;
		public LogicalBank[] Banks;
		public LogicalMount[] Mounts;
		public LogicalPsionic[] Psionics;
		public List<Tech> Techs = new List<Tech>();
		public ShipSectionType[] ExcludeSectionTypes;
		public string[] ExcludeSections;
		public string[] IncludeSections;
		public bool SectionIsExcluded(ShipSectionAsset section)
		{
			string sectionName = Path.GetFileNameWithoutExtension(section.FileName);
			if (this.ExcludeSectionTypes.Any((ShipSectionType x) => x == section.Type))
			{
				return true;
			}
			if (this.IncludeSections.Length > 0)
			{
				return !this.IncludeSections.Any((string x) => x == sectionName);
			}
			return this.ExcludeSections.Any((string x) => x == sectionName);
		}
		public static IEnumerable<LogicalModule> EnumerateModuleFits(IEnumerable<LogicalModule> modules, ShipSectionAsset section, int sectionModuleMountIndex, bool debugStations = false)
		{
			LogicalModuleMount logicalModuleMount = section.Modules[sectionModuleMountIndex];
			if (section.Class == ShipClass.Station && !debugStations && !ModuleShipData.DebugAutoAssignModules)
			{
				if (!string.IsNullOrEmpty(logicalModuleMount.AssignedModuleName))
				{
					foreach (LogicalModule current in modules)
					{
						if (current.AssignByDefault && current.Faction == section.Faction && current.ModuleName == logicalModuleMount.AssignedModuleName && !current.SectionIsExcluded(section))
						{
							yield return current;
						}
					}
				}
				else
				{
					foreach (LogicalModule current2 in modules)
					{
						if (current2.AssignByDefault && current2.ModuleType == logicalModuleMount.ModuleType && !current2.SectionIsExcluded(section) && current2.Faction == section.Faction)
						{
							yield return current2;
						}
					}
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(logicalModuleMount.AssignedModuleName))
				{
					foreach (LogicalModule current3 in modules)
					{
						if (current3.Faction == section.Faction && current3.ModuleName == logicalModuleMount.AssignedModuleName && !current3.SectionIsExcluded(section))
						{
							yield return current3;
						}
					}
				}
				else
				{
					foreach (LogicalModule current4 in modules)
					{
						if (current4.ModuleType == logicalModuleMount.ModuleType && !current4.SectionIsExcluded(section) && current4.Faction == section.Faction)
						{
							yield return current4;
						}
					}
				}
			}
			yield break;
		}
	}
}
