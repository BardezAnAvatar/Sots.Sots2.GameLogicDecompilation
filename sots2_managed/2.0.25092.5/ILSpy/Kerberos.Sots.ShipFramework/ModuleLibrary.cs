using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.ShipFramework
{
	internal static class ModuleLibrary
	{
		private static IEnumerable<string> ExtractModuleFiles(string filename)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			XmlElement xmlElement = xmlDocument["ModuleList"];
			XmlElement source = xmlElement["Modules"];
			foreach (XmlElement current in 
				from x in source.OfType<XmlElement>()
				where x.Name == "Module"
				select x)
			{
				yield return PathHelpers.FixSeparators(current.GetAttribute("File"));
			}
			yield break;
		}
		public static LogicalModule CreateLogicalModuleFromFile(string moduleFile, string faction)
		{
			ShipModule shipModule = new ShipModule();
			ShipModuleXmlUtility.LoadShipModuleFromXml(moduleFile, ref shipModule);
			LogicalModule logicalModule = new LogicalModule();
			logicalModule.ModuleTitle = shipModule.ModuleTitle;
			logicalModule.Description = shipModule.Description;
			logicalModule.ModuleType = (shipModule.ModuleType ?? string.Empty);
			logicalModule.ModulePath = PathHelpers.Combine(new string[]
			{
				"factions",
				faction,
				"modules",
				Path.GetFileName(moduleFile)
			});
			logicalModule.ModuleName = Path.GetFileNameWithoutExtension(logicalModule.ModulePath);
			logicalModule.ModelPath = shipModule.ModelPath;
			logicalModule.LowStructModelPath = (shipModule.DamagedModelPath ?? string.Empty);
			logicalModule.DeadModelPath = (shipModule.DestroyedModelPath ?? string.Empty);
			logicalModule.LowStruct = (float)shipModule.StructDamageAmount;
			logicalModule.AmbientSound = shipModule.AmbientSound;
			if (!string.IsNullOrEmpty(shipModule.AbilityType))
			{
				logicalModule.AbilityType = (ModuleEnums.ModuleAbilities)Enum.Parse(typeof(ModuleEnums.ModuleAbilities), shipModule.AbilityType);
			}
			logicalModule.AbilitySupply = shipModule.AbilitySupply;
			if (logicalModule.AbilitySupply == 0)
			{
				if (logicalModule.AbilityType == ModuleEnums.ModuleAbilities.GoopArmorRepair)
				{
					logicalModule.AbilitySupply = 3;
				}
				else
				{
					if (logicalModule.AbilityType == ModuleEnums.ModuleAbilities.JokerECM)
					{
						logicalModule.AbilitySupply = 5;
					}
				}
			}
			logicalModule.Crew = shipModule.Crew;
			logicalModule.CrewRequired = shipModule.CrewRequired;
			logicalModule.Supply = shipModule.Supply;
			logicalModule.Structure = (float)shipModule.Structure;
			logicalModule.StructureBonus = (float)shipModule.StructureBonus;
			logicalModule.ArmorBonus = shipModule.ArmorBonus;
			logicalModule.ECCM = shipModule.ECCM;
			logicalModule.ECM = shipModule.ECM;
			logicalModule.RepairPointsBonus = shipModule.RepairPointsBonus;
			logicalModule.AccelBonus = shipModule.AccelerationBonus / 100f;
			logicalModule.CriticalHitBonus = shipModule.CriticalHitBonus;
			logicalModule.SensorBonus = shipModule.SensorBonus;
			logicalModule.AdmiralSurvivalBonus = shipModule.AdmiralSurvivalBonus;
			logicalModule.PsionicPowerBonus = shipModule.PsionicPowerBonus;
			logicalModule.PsionicStaminaBonus = shipModule.PsionicStaminaBonus;
			logicalModule.PowerBonus = shipModule.Power;
			logicalModule.DamageEffect = new LogicalEffect
			{
				Name = shipModule.DamagedEffectPath ?? string.Empty
			};
			logicalModule.DeathEffect = new LogicalEffect
			{
				Name = shipModule.DestroyedEffectPath ?? string.Empty
			};
			logicalModule.AssignByDefault = shipModule.AssignByDefault;
			logicalModule.Icon = shipModule.IconSprite;
			logicalModule.ROFBonus = shipModule.ROFBonus;
			logicalModule.CrewEfficiencyBonus = shipModule.CrewEfficiencyBonus;
			if (logicalModule.AbilityType == ModuleEnums.ModuleAbilities.KarnakTargeting)
			{
				logicalModule.AccuracyBonus = 10f;
				logicalModule.DamageBonus = 0.15f;
			}
			if (logicalModule.ModulePath.Contains("cr_"))
			{
				logicalModule.Class = ShipClass.Cruiser;
			}
			else
			{
				if (logicalModule.ModulePath.Contains("dn_"))
				{
					logicalModule.Class = ShipClass.Dreadnought;
				}
				else
				{
					if (logicalModule.ModulePath.Contains("lv_"))
					{
						logicalModule.Class = ShipClass.Leviathan;
					}
					else
					{
						if (logicalModule.ModulePath.Contains("sn_"))
						{
							logicalModule.Class = ShipClass.Station;
						}
					}
				}
			}
			List<LogicalBank> list = new List<LogicalBank>();
			List<LogicalMount> list2 = new List<LogicalMount>();
			List<LogicalPsionic> list3 = new List<LogicalPsionic>();
			foreach (Bank current in shipModule.Banks)
			{
				LogicalBank logicalBank = new LogicalBank
				{
					TurretSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), current.Size),
					Section = null,
					Module = logicalModule,
					GUID = Guid.Parse(current.Id),
					DefaultWeaponName = current.DefaultWeapon
				};
				logicalBank.TurretClass = (WeaponEnums.TurretClasses)Enum.Parse(typeof(WeaponEnums.TurretClasses), current.Class);
				list.Add(logicalBank);
				foreach (Mount current2 in current.Mounts)
				{
					LogicalMount logicalMount = new LogicalMount();
					logicalMount.Bank = logicalBank;
					logicalMount.NodeName = current2.NodeName;
					logicalMount.FireAnimName = ((current2.SectionFireAnimation != null) ? current2.SectionFireAnimation : "");
					logicalMount.ReloadAnimName = ((current2.SectionReloadAnimation != null) ? current2.SectionReloadAnimation : "");
					logicalMount.Yaw.Min = current2.YawMin;
					logicalMount.Yaw.Max = current2.YawMax;
					logicalMount.Pitch.Min = current2.PitchMin;
					logicalMount.Pitch.Max = current2.PitchMax;
					logicalMount.Pitch.Min = Math.Max(-90f, logicalMount.Pitch.Min);
					logicalMount.Pitch.Max = Math.Min(90f, logicalMount.Pitch.Max);
					list2.Add(logicalMount);
				}
			}
			List<string> list4 = new List<string>();
			List<ShipSectionType> list5 = new List<ShipSectionType>();
			foreach (ExcludedSection current3 in shipModule.ExcludedSections)
			{
				list4.Add(current3.Name);
			}
			foreach (ExcludedType current4 in shipModule.ExcludedTypes)
			{
				ShipSectionType item = ShipSectionType.Command;
				if (current4.Name == "Engine")
				{
					item = ShipSectionType.Engine;
				}
				else
				{
					if (current4.Name == "Mission")
					{
						item = ShipSectionType.Mission;
					}
				}
				list5.Add(item);
			}
			List<string> list6 = new List<string>();
			foreach (IncludedSection current5 in shipModule.IncludedSections)
			{
				list6.Add(current5.Name);
			}
			logicalModule.NumPsionicSlots = 0;
			if (logicalModule.ModuleTitle.Contains("PROFESSORX") || logicalModule.ModuleTitle.Contains("PSIWAR"))
			{
				if (logicalModule.Class == ShipClass.Cruiser)
				{
					logicalModule.NumPsionicSlots = 1;
				}
				else
				{
					if (logicalModule.Class == ShipClass.Dreadnought)
					{
						logicalModule.NumPsionicSlots = 3;
					}
				}
				for (int i = 0; i < logicalModule.NumPsionicSlots; i++)
				{
					LogicalPsionic item2 = new LogicalPsionic();
					list3.Add(item2);
				}
			}
			logicalModule.Banks = list.ToArray();
			logicalModule.Mounts = list2.ToArray();
			logicalModule.Psionics = list3.ToArray();
			logicalModule.Techs = shipModule.Techs;
			logicalModule.SavingsCost = shipModule.SavingsCost;
			logicalModule.UpkeepCost = shipModule.UpkeepCost;
			logicalModule.ProductionCost = shipModule.ProductionCost;
			logicalModule.ExcludeSections = list4.ToArray();
			logicalModule.IncludeSections = list6.ToArray();
			logicalModule.ExcludeSectionTypes = list5.ToArray();
			logicalModule.Faction = faction;
			return logicalModule;
		}
		public static IEnumerable<LogicalModule> Enumerate()
		{
			try
			{
				string[] array = ScriptHost.FileSystem.FindDirectories("factions\\*");
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					string text2 = PathHelpers.Combine(new string[]
					{
						text,
						"modules\\_modules.xml"
					});
					if (ScriptHost.FileSystem.FileExists(text2))
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
						foreach (string current in ModuleLibrary.ExtractModuleFiles(text2))
						{
							yield return ModuleLibrary.CreateLogicalModuleFromFile(current, fileNameWithoutExtension);
						}
					}
				}
			}
			finally
			{
			}
			yield break;
		}
	}
}
