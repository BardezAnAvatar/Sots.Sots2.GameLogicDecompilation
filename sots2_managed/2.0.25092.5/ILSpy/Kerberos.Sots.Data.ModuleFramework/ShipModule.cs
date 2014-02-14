using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ModuleFramework
{
	public class ShipModule : IXmlLoadSave
	{
		internal const string XmlModuleName = "Module";
		private const string XmlDescriptionName = "Description";
		private const string XmlModelPathName = "ModelFile";
		private const string XmlDamagedModelPathName = "DamagedModelFile";
		private const string XmlDamagedEffectPathName = "DamagedEffectFile";
		private const string XmlDestroyedModelPathName = "DestroyedModelFile";
		private const string XmlDestroyedEffectPathName = "DestroyedEffectFile";
		private const string XmlIconSpriteName = "IconSprite";
		private const string XmlAmbiendSoundName = "AmbientSound";
		private const string XmlAssignByDefaultName = "AssignByDefault";
		private const string XmlModuleSizeName = "ModuleSize";
		private const string XmlModuleTypeName = "ModuleType";
		private const string XmlAbilityTypeName = "AbilityType";
		private const string XmlAbilitySupplyName = "AbilitySupply";
		private const string XmlSavingsCostName = "SavingsCost";
		private const string XmlUpkeepCostName = "UpkeepCost";
		private const string XmlProductionCostName = "ProductionCost";
		private const string XmlCrewName = "Crew";
		private const string XmlCrewRequiredName = "CrewRequired";
		private const string XmlSupplyName = "Supply";
		private const string XmlPowerName = "Power";
		private const string XmlStructName = "Struct";
		private const string XmlStructDamagedAmountName = "StructDamagedAmount";
		private const string XmlStructBonusName = "StructBonus";
		private const string XmlArmorBonusName = "ArmorBonus";
		private const string XmlPsionicPowerBonusName = "PsionicPowerBonus";
		private const string XmlPsionicStaminaBonusName = "PsionicStaminaBonus";
		private const string XmlECCMName = "ECCM";
		private const string XmlECMName = "ECM";
		private const string XmlSensorBonusName = "SensorBonus";
		private const string XmlRepairPointsBonusName = "RepairPointsBonus";
		private const string XmlMassBonusName = "MassBonus";
		private const string XmlCriticalHitBonusName = "CriticalHitBonus";
		private const string XmlAdmiralSurvivalBonusName = "AdmiralSurvivalBonus";
		private const string XmlRateOfFireBonusName = "RateOfFireBonus";
		private const string XmlCrewEfficiencyBonusName = "CrewEfficiencyBonus";
		private const string XmlDamageName = "Damage";
		private const string XmlBanksName = "Banks";
		private const string XmlBankName = "Bank";
		private const string XmlPsionicsName = "Psionics";
		private const string XmlPsionicName = "Psionic";
		private const string XmlTechsName = "RequiredTechs";
		private const string XmlTechName = "Tech";
		private const string XmlExcludedSectionsName = "ExcludedSections";
		private const string XmlExcludedSectionName = "ExcludedSection";
		private const string XmlIncludedSectionsName = "IncludedSections";
		private const string XmlIncludedSectionName = "IncludedSection";
		private const string XmlExcludedTypesName = "ExcludedTypes";
		private const string XmlExcludedTypeName = "ExcludedType";
		private const string XmlPsionicAbilitiesName = "PsionicAbilities";
		public string SavePath;
		public string ModuleTitle = "";
		public string Description = "";
		public string ModelPath = "";
		public string DamagedModelPath = "";
		public string DamagedEffectPath = "";
		public string DestroyedModelPath = "";
		public string DestroyedEffectPath = "";
		public string IconSprite = "";
		public string AmbientSound = "";
		public bool AssignByDefault;
		public string ModuleSize = "";
		public string ModuleType = "";
		public string AbilityType = "";
		public int AbilitySupply;
		public int SavingsCost;
		public int UpkeepCost;
		public int ProductionCost;
		public int Crew;
		public int CrewRequired;
		public int Supply;
		public int Power;
		public int Structure;
		public int StructDamageAmount;
		public int StructureBonus;
		public int ArmorBonus;
		public float PsionicPowerBonus;
		public float PsionicStaminaBonus;
		public float ECCM;
		public float ECM;
		public float SensorBonus;
		public int RepairPointsBonus;
		public float AccelerationBonus;
		public float CriticalHitBonus;
		public float AdmiralSurvivalBonus;
		public float Damage;
		public float ROFBonus;
		public float CrewEfficiencyBonus;
		public List<Bank> Banks = new List<Bank>();
		public List<Tech> Techs = new List<Tech>();
		public List<ExcludedSection> ExcludedSections = new List<ExcludedSection>();
		public List<IncludedSection> IncludedSections = new List<IncludedSection>();
		public List<ExcludedType> ExcludedTypes = new List<ExcludedType>();
		public List<AvailablePsionicAbility> PsionicAbilities = new List<AvailablePsionicAbility>();
		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.ModuleTitle, "Module", ref node);
			XmlHelper.AddNode(this.Description, "Description", ref node);
			XmlHelper.AddNode(this.ModelPath, "ModelFile", ref node);
			XmlHelper.AddNode(this.DamagedModelPath, "DamagedModelFile", ref node);
			XmlHelper.AddNode(this.DamagedEffectPath, "DamagedEffectFile", ref node);
			XmlHelper.AddNode(this.DestroyedModelPath, "DestroyedModelFile", ref node);
			XmlHelper.AddNode(this.DestroyedEffectPath, "DestroyedEffectFile", ref node);
			XmlHelper.AddNode(this.IconSprite, "IconSprite", ref node);
			XmlHelper.AddNode(this.AmbientSound, "AmbientSound", ref node);
			XmlHelper.AddNode(this.AssignByDefault, "AssignByDefault", ref node);
			XmlHelper.AddNode(this.ModuleSize, "ModuleSize", ref node);
			XmlHelper.AddNode(this.ModuleType, "ModuleType", ref node);
			XmlHelper.AddNode(this.AbilityType, "AbilityType", ref node);
			XmlHelper.AddNode(this.AbilitySupply, "AbilitySupply", ref node);
			XmlHelper.AddNode(this.SavingsCost, "SavingsCost", ref node);
			XmlHelper.AddNode(this.UpkeepCost, "UpkeepCost", ref node);
			XmlHelper.AddNode(this.ProductionCost, "ProductionCost", ref node);
			XmlHelper.AddNode(this.Crew, "Crew", ref node);
			XmlHelper.AddNode(this.CrewRequired, "CrewRequired", ref node);
			XmlHelper.AddNode(this.Supply, "Supply", ref node);
			XmlHelper.AddNode(this.Power, "Power", ref node);
			XmlHelper.AddNode(this.Structure, "Struct", ref node);
			XmlHelper.AddNode(this.StructDamageAmount, "StructDamagedAmount", ref node);
			XmlHelper.AddNode(this.StructureBonus, "StructBonus", ref node);
			XmlHelper.AddNode(this.ArmorBonus, "ArmorBonus", ref node);
			XmlHelper.AddNode(this.PsionicPowerBonus, "PsionicPowerBonus", ref node);
			XmlHelper.AddNode(this.PsionicStaminaBonus, "PsionicStaminaBonus", ref node);
			XmlHelper.AddNode(this.ECCM, "ECCM", ref node);
			XmlHelper.AddNode(this.ECM, "ECM", ref node);
			XmlHelper.AddNode(this.SensorBonus, "SensorBonus", ref node);
			XmlHelper.AddNode(this.RepairPointsBonus, "RepairPointsBonus", ref node);
			XmlHelper.AddNode(this.AccelerationBonus, "MassBonus", ref node);
			XmlHelper.AddNode(this.CriticalHitBonus, "CriticalHitBonus", ref node);
			XmlHelper.AddNode(this.AdmiralSurvivalBonus, "AdmiralSurvivalBonus", ref node);
			XmlHelper.AddNode(this.Damage, "Damage", ref node);
			XmlHelper.AddNode(this.ROFBonus, "RateOfFireBonus", ref node);
			XmlHelper.AddNode(this.CrewEfficiencyBonus, "CrewEfficiencyBonus", ref node);
			XmlHelper.AddObjectCollectionNode(this.Banks, "Banks", "Bank", ref node);
			XmlHelper.AddObjectCollectionNode(this.Techs, "RequiredTechs", "Tech", ref node);
			XmlHelper.AddObjectCollectionNode(this.ExcludedSections, "ExcludedSections", "ExcludedSection", ref node);
			XmlHelper.AddObjectCollectionNode(this.IncludedSections, "IncludedSections", "IncludedSection", ref node);
			XmlHelper.AddObjectCollectionNode(this.ExcludedTypes, "ExcludedTypes", "ExcludedType", ref node);
			XmlHelper.AddObjectCollectionNode(this.PsionicAbilities, "PsionicAbilities", "PsionicAbility", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.ModuleTitle = XmlHelper.GetData<string>(node, "Module");
			this.Description = XmlHelper.GetData<string>(node, "Description");
			this.ModelPath = XmlHelper.GetData<string>(node, "ModelFile");
			this.DamagedModelPath = XmlHelper.GetData<string>(node, "DamagedModelFile");
			this.DamagedEffectPath = XmlHelper.GetData<string>(node, "DamagedEffectFile");
			this.DestroyedModelPath = XmlHelper.GetData<string>(node, "DestroyedModelFile");
			this.DestroyedEffectPath = XmlHelper.GetData<string>(node, "DestroyedEffectFile");
			this.IconSprite = XmlHelper.GetDataOrDefault<string>(node["IconSprite"], string.Empty);
			this.AmbientSound = XmlHelper.GetData<string>(node, "AmbientSound");
			this.AssignByDefault = XmlHelper.GetDataOrDefault<bool>(node["AssignByDefault"], false);
			this.ModuleSize = XmlHelper.GetData<string>(node, "ModuleSize");
			this.ModuleType = XmlHelper.GetData<string>(node, "ModuleType");
			this.AbilityType = XmlHelper.GetData<string>(node, "AbilityType");
			this.AbilitySupply = XmlHelper.GetData<int>(node, "AbilitySupply");
			this.SavingsCost = XmlHelper.GetData<int>(node, "SavingsCost");
			this.UpkeepCost = XmlHelper.GetData<int>(node, "UpkeepCost");
			this.ProductionCost = XmlHelper.GetData<int>(node, "ProductionCost");
			this.Crew = XmlHelper.GetData<int>(node, "Crew");
			this.CrewRequired = XmlHelper.GetData<int>(node, "CrewRequired");
			this.Supply = XmlHelper.GetData<int>(node, "Supply");
			this.Power = XmlHelper.GetData<int>(node, "Power");
			this.Structure = XmlHelper.GetData<int>(node, "Struct");
			this.StructDamageAmount = XmlHelper.GetData<int>(node, "StructDamagedAmount");
			this.StructureBonus = XmlHelper.GetData<int>(node, "StructBonus");
			this.ArmorBonus = XmlHelper.GetData<int>(node, "ArmorBonus");
			this.PsionicPowerBonus = XmlHelper.GetData<float>(node, "PsionicPowerBonus");
			this.PsionicStaminaBonus = XmlHelper.GetData<float>(node, "PsionicStaminaBonus");
			this.ECCM = XmlHelper.GetData<float>(node, "ECCM");
			this.ECM = XmlHelper.GetData<float>(node, "ECM");
			this.SensorBonus = XmlHelper.GetData<float>(node, "SensorBonus");
			this.RepairPointsBonus = XmlHelper.GetData<int>(node, "RepairPointsBonus");
			this.AccelerationBonus = XmlHelper.GetData<float>(node, "MassBonus");
			this.CriticalHitBonus = XmlHelper.GetData<float>(node, "CriticalHitBonus");
			this.AdmiralSurvivalBonus = XmlHelper.GetData<float>(node, "AdmiralSurvivalBonus");
			this.Damage = XmlHelper.GetData<float>(node, "Damage");
			this.ROFBonus = XmlHelper.GetData<float>(node, "RateOfFireBonus");
			this.CrewEfficiencyBonus = XmlHelper.GetData<float>(node, "CrewEfficiencyBonus");
			this.Banks = XmlHelper.GetDataObjectCollection<Bank>(node, "Banks", "Bank");
			this.Techs = XmlHelper.GetDataObjectCollection<Tech>(node, "RequiredTechs", "Tech");
			this.ExcludedSections = XmlHelper.GetDataObjectCollection<ExcludedSection>(node, "ExcludedSections", "ExcludedSection");
			this.IncludedSections = XmlHelper.GetDataObjectCollection<IncludedSection>(node, "IncludedSections", "IncludedSection");
			this.ExcludedTypes = XmlHelper.GetDataObjectCollection<ExcludedType>(node, "ExcludedTypes", "ExcludedType");
			this.PsionicAbilities = XmlHelper.GetDataObjectCollection<AvailablePsionicAbility>(node, "PsionicAbilities", "PsionicAbility");
		}
	}
}