using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class ShipSection : IXmlLoadSave
	{
		internal const string XmlShipSectionName = "ShipSection";
		private const string XmlNameAiType = "AiType";
		private const string XmlNameRealClass = "RealClass";
		private const string XmlNameTitle = "Title";
		private const string XmlNameDescription = "Description";
		private const string XmlNameModelPath = "ModelFile";
		private const string XmlNameDamagedModelPath = "DamagedModelFile";
		private const string XmlNameDamagedEffectPath = "DamagedEffectFile";
		private const string XmlNameDestroyedModelPath = "DestroyedModelFile";
		private const string XmlNameDestroyedEffectPath = "DestroyedEffectFile";
		private const string XmlNameAmbientSound = "AmbientSound";
		private const string XmlNameEngineSound = "EngineSound";
		private const string XmlNameCameraDistanceFactor = "CameraDistanceFactor";
		private const string XmlNameStruct = "Struct";
		private const string XmlNameStructDamageAmount = "StructDamageAmount";
		private const string XmlNameDeathDamage = "DeathDamage";
		private const string XmlNameExplosionRadius = "ExplosionRadius";
		private const string XmlNameMass = "Mass";
		private const string XmlNameTop = "Top";
		private const string XmlNameBottom = "Bottom";
		private const string XmlNameSide = "Side";
		private const string XmlNameCrew = "Crew";
		private const string XmlNameCrewRequired = "CrewRequired";
		private const string XmlNamePower = "Power";
		private const string XmlNameSupply = "Supply";
		private const string XmlNameECM = "ECM";
		private const string XmlNameECCM = "ECCM";
		private const string XmlNameColonizerSpace = "ColonizerSpace";
		private const string XmlNameSavingsCost = "SavingsCost";
		private const string XmlNameProductionCost = "ProductionCost";
		private const string XmlNameFtlSpeed = "FtlSpeed";
		private const string XmlNameNodeSpeed = "NodeSpeed";
		private const string XmlNameMissionTime = "MissionTime";
		private const string XmlNameCommandPoints = "CommandPoints";
		private const string XmlNameRepairPoints = "RepairPoints";
		private const string XmlNameTerraformingPoints = "TerraformingPoints";
		private const string XmlNameFreighterSpace = "FreighterSpace";
		private const string XmlNameSignature = "Signature";
		private const string XmlNameTacticalSensorRange = "TacticalSensorRange";
		private const string XmlNameStrategicSensorRange = "StrategicSensorRange";
		private const string XmlNameLaunchDelay = "LaunchDelay";
		private const string XmlNameDockingDelay = "DockingDelay";
		private const string XmlNameBattleRiderReserveSize = "BattleRiderReserveSize";
		private const string XmlNameStationType = "StationType";
		private const string XmlNameStationLevel = "StationLevel";
		private const string XmlNameIsConstructor = "IsConstructor";
		private const string XmlNameIsFreighter = "IsFreighter";
		private const string XmlNameConstructionPoints = "ConstructionPoints";
		private const string XmlNameIsPolice = "IsPolice";
		private const string XmlPsionicPowerLevel = "PsionicPowerLevel";
		private const string XmlNameAcceleration = "Acceleration";
		private const string XmlNameRotationalAccelerationYaw = "RotationalAccelerationYaw";
		private const string XmlNameRotationalAccelerationPitch = "RotationalAccelerationPitch";
		private const string XmlNameRotationalAccelerationRoll = "RotationalAccelerationRoll";
		private const string XmlNameDecceleration = "Decceleration";
		private const string XmlNameLinearSpeed = "LinearSpeed";
		private const string XmlNameRotationSpeed = "RotationSpeed";
		private const string XmlNameBanks = "Banks";
		private const string XmlNameBank = "Bank";
		private const string XmlNameTechs = "RequiredTechs";
		private const string XmlNameTech = "Tech";
		private const string XmlNameModules = "Modules";
		private const string XmlNameModule = "Module";
		private const string XmlNameExcludedSections = "ExcludedSections";
		private const string XmlNameExcludedSection = "ExcludedSection";
		private const string XmlNameExcludedTypes = "ExcludedTypes";
		private const string XmlNameExcludedType = "ExcludedType";
		private const string XmlNamePsionicAbilities = "PsionicAbilities";
		private const string XmlNameWeaponGroups = "WeaponGroups";
		private const string XmlNameShipOptionGroups = "ShipOptionGroups";
		private const string XmlSlaveCapacity = "SlaveCapacity";
		private const string XmlNameFleetSpeedModifier = "FleetSpeedModifier";
		private const string XmlPreviewImage = "PreviewImage";
		public string SavePath;
		public ShipSectionType ShipType;
		public ShipClass ShipClass;
		public RealShipClasses? RealShipClass;
		public string Title = "";
		public string Description = "";
		public string CombatAiType = "";
		public string ModelPath = "";
		public string DamageModelPath = "";
		public string DamageEffectPath = "";
		public string DestroyedModelPath = "";
		public string DestroyedEffectPath = "";
		public string AmbientSound = "";
		public string EngineSound = "";
		public string UnderAttackSound = "";
		public string DestroyedSound = "";
		public float CameraDistanceFactor;
		public int Struct;
		public int StructDamageAmount;
		public float DeathDamage;
		public float ExplosionRadius;
		public int Mass;
		public Size TopArmor = new Size
		{
			X = 10,
			Y = 10
		};
		public Size BottomArmor = new Size
		{
			X = 10,
			Y = 10
		};
		public Size SideArmor = new Size
		{
			X = 10,
			Y = 10
		};
		public int Crew;
		public int CrewRequired;
		public int Power;
		public int Supply;
		public float ECM;
		public float ECCM;
		public int ColonizerSpace;
		public int SavingsCost;
		public int ProductionCost;
		public float FtlSpeed;
		public float NodeSpeed;
		public float MissionTime;
		public int CommandPoints;
		public int RepairPoints;
		public int TerraformingPoints;
		public int FreighterSpace;
		public float Signature;
		public float TacticalSensorRange;
		public float StrategicSensorRange;
		public float LaunchDelay;
		public float DockingDelay;
		public int BattleRiderReserveSize;
		public string StationType = "";
		public int StationLevel;
		public bool isConstructor;
		public bool isFreighter;
		public int ConstructionPoints;
		public bool isPolice;
		public float PsionicPowerLevel;
		public int SlaveCapacity;
		public float FleetSpeedModifier;
		public string PreviewImage;
		public float Acceleration;
		public float RotationalAccelerationYaw;
		public float RotationalAccelerationPitch;
		public float RotationalAccelerationRoll;
		public float Decceleration;
		public float LinearSpeed;
		public float RotationSpeed;
		public List<Bank> Banks = new List<Bank>();
		public List<ModuleMount> Modules = new List<ModuleMount>();
		public List<BattleRiderMount> BattleRiderMounts = new List<BattleRiderMount>();
		public List<Tech> Techs = new List<Tech>();
		public List<ExcludedSection> ExcludedSections = new List<ExcludedSection>();
		public List<ExcludedType> ExcludedTypes = new List<ExcludedType>();
		public List<AvailablePsionicAbility> PsionicAbilities = new List<AvailablePsionicAbility>();
		public List<WeaponGroup> WeaponGroups = new List<WeaponGroup>();
		public List<ShipOptionGroup> ShipOptionGroups = new List<ShipOptionGroup>();
		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.RealShipClass, "RealClass", ref node);
			XmlHelper.AddNode(this.Title, "Title", ref node);
			XmlHelper.AddNode(this.Description, "Description", ref node);
			XmlHelper.AddNode(this.CombatAiType, "AiType", ref node);
			XmlHelper.AddNode(this.ModelPath, "ModelFile", ref node);
			XmlHelper.AddNode(this.DamageModelPath, "DamagedModelFile", ref node);
			XmlHelper.AddNode(this.DamageEffectPath, "DamagedEffectFile", ref node);
			XmlHelper.AddNode(this.DestroyedModelPath, "DestroyedModelFile", ref node);
			XmlHelper.AddNode(this.DestroyedEffectPath, "DestroyedEffectFile", ref node);
			XmlHelper.AddNode(this.AmbientSound, "AmbientSound", ref node);
			XmlHelper.AddNode(this.EngineSound, "EngineSound", ref node);
			XmlHelper.AddNode(this.CameraDistanceFactor, "CameraDistanceFactor", ref node);
			XmlHelper.AddNode(this.Struct, "Struct", ref node);
			XmlHelper.AddNode(this.StructDamageAmount, "StructDamageAmount", ref node);
			XmlHelper.AddNode(this.DeathDamage, "DeathDamage", ref node);
			XmlHelper.AddNode(this.ExplosionRadius, "ExplosionRadius", ref node);
			XmlHelper.AddNode(this.Mass, "Mass", ref node);
			XmlHelper.AddNode(this.TopArmor, "Top", ref node);
			XmlHelper.AddNode(this.BottomArmor, "Bottom", ref node);
			XmlHelper.AddNode(this.SideArmor, "Side", ref node);
			XmlHelper.AddNode(this.Crew, "Crew", ref node);
			XmlHelper.AddNode(this.CrewRequired, "CrewRequired", ref node);
			XmlHelper.AddNode(this.Power, "Power", ref node);
			XmlHelper.AddNode(this.Supply, "Supply", ref node);
			XmlHelper.AddNode(this.ECM, "ECM", ref node);
			XmlHelper.AddNode(this.ECCM, "ECCM", ref node);
			XmlHelper.AddNode(this.ColonizerSpace, "ColonizerSpace", ref node);
			XmlHelper.AddNode(this.SavingsCost, "SavingsCost", ref node);
			XmlHelper.AddNode(this.ProductionCost, "ProductionCost", ref node);
			XmlHelper.AddNode(this.FtlSpeed, "FtlSpeed", ref node);
			XmlHelper.AddNode(this.NodeSpeed, "NodeSpeed", ref node);
			XmlHelper.AddNode(this.MissionTime, "MissionTime", ref node);
			XmlHelper.AddNode(this.CommandPoints, "CommandPoints", ref node);
			XmlHelper.AddNode(this.RepairPoints, "RepairPoints", ref node);
			XmlHelper.AddNode(this.TerraformingPoints, "TerraformingPoints", ref node);
			XmlHelper.AddNode(this.FreighterSpace, "FreighterSpace", ref node);
			XmlHelper.AddNode(this.Signature, "Signature", ref node);
			XmlHelper.AddNode(this.TacticalSensorRange, "TacticalSensorRange", ref node);
			XmlHelper.AddNode(this.StrategicSensorRange, "StrategicSensorRange", ref node);
			XmlHelper.AddNode(this.LaunchDelay, "LaunchDelay", ref node);
			XmlHelper.AddNode(this.DockingDelay, "DockingDelay", ref node);
			XmlHelper.AddNode(this.BattleRiderReserveSize, "BattleRiderReserveSize", ref node);
			XmlHelper.AddNode(this.StationType, "StationType", ref node);
			XmlHelper.AddNode(this.StationLevel, "StationLevel", ref node);
			XmlHelper.AddNode(this.isConstructor, "IsConstructor", ref node);
			XmlHelper.AddNode(this.isFreighter, "IsFreighter", ref node);
			XmlHelper.AddNode(this.ConstructionPoints, "ConstructionPoints", ref node);
			XmlHelper.AddNode(this.isPolice, "IsPolice", ref node);
			XmlHelper.AddNode(this.PsionicPowerLevel, "PsionicPowerLevel", ref node);
			XmlHelper.AddNode(this.SlaveCapacity, "SlaveCapacity", ref node);
			XmlHelper.AddNode(this.FleetSpeedModifier, "FleetSpeedModifier", ref node);
			XmlHelper.AddNode(this.PreviewImage, "PreviewImage", ref node);
			XmlHelper.AddNode(this.Acceleration, "Acceleration", ref node);
			XmlHelper.AddNode(this.RotationalAccelerationYaw, "RotationalAccelerationYaw", ref node);
			XmlHelper.AddNode(this.RotationalAccelerationPitch, "RotationalAccelerationPitch", ref node);
			XmlHelper.AddNode(this.RotationalAccelerationRoll, "RotationalAccelerationRoll", ref node);
			XmlHelper.AddNode(this.Decceleration, "Decceleration", ref node);
			XmlHelper.AddNode(this.LinearSpeed, "LinearSpeed", ref node);
			XmlHelper.AddNode(this.RotationSpeed, "RotationSpeed", ref node);
			XmlHelper.AddObjectCollectionNode(this.Banks, "Banks", ref node);
			XmlHelper.AddObjectCollectionNode(this.Techs, "RequiredTechs", "Tech", ref node);
			XmlHelper.AddObjectCollectionNode(this.Modules, "Modules", "Module", ref node);
			XmlHelper.AddObjectCollectionNode(this.ExcludedSections, "ExcludedSections", "ExcludedSection", ref node);
			XmlHelper.AddObjectCollectionNode(this.ExcludedTypes, "ExcludedTypes", "ExcludedType", ref node);
			XmlHelper.AddObjectCollectionNode(this.PsionicAbilities, "PsionicAbilities", "PsionicAbility", ref node);
			XmlHelper.AddObjectCollectionNode(this.WeaponGroups, "WeaponGroups", "WeaponGroup", ref node);
			XmlHelper.AddObjectCollectionNode(this.ShipOptionGroups, "ShipOptionGroups", ShipOptionGroup.XmlNameShipOptionGroup, ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			string data = XmlHelper.GetData<string>(node, "RealClass");
			if (!string.IsNullOrEmpty(data))
			{
				this.RealShipClass = new RealShipClasses?((RealShipClasses)Enum.Parse(typeof(RealShipClasses), data));
			}
			else
			{
				this.RealShipClass = null;
			}
			this.Title = XmlHelper.GetData<string>(node, "Title");
			this.Description = XmlHelper.GetData<string>(node, "Description");
			this.CombatAiType = XmlHelper.GetData<string>(node, "AiType");
			this.ModelPath = XmlHelper.GetData<string>(node, "ModelFile");
			this.DamageModelPath = XmlHelper.GetData<string>(node, "DamagedModelFile");
			this.DamageEffectPath = XmlHelper.GetData<string>(node, "DamagedEffectFile");
			this.DestroyedModelPath = XmlHelper.GetData<string>(node, "DestroyedModelFile");
			this.DestroyedEffectPath = XmlHelper.GetData<string>(node, "DestroyedEffectFile");
			this.AmbientSound = XmlHelper.GetData<string>(node, "AmbientSound");
			this.EngineSound = XmlHelper.GetDataOrDefault<string>(node["EngineSound"], "");
			this.CameraDistanceFactor = XmlHelper.GetDataOrDefault<float>(node["CameraDistanceFactor"], 1f);
			this.Struct = XmlHelper.GetData<int>(node, "Struct");
			this.StructDamageAmount = XmlHelper.GetData<int>(node, "StructDamageAmount");
			this.DeathDamage = XmlHelper.GetData<float>(node, "DeathDamage");
			this.ExplosionRadius = XmlHelper.GetData<float>(node, "ExplosionRadius");
			this.Mass = XmlHelper.GetData<int>(node, "Mass");
			this.TopArmor = XmlHelper.GetData<string>(node, "Top");
			this.BottomArmor = XmlHelper.GetData<string>(node, "Bottom");
			this.SideArmor = XmlHelper.GetData<string>(node, "Side");
			this.Crew = XmlHelper.GetData<int>(node, "Crew");
			this.CrewRequired = XmlHelper.GetData<int>(node, "CrewRequired");
			this.Power = XmlHelper.GetData<int>(node, "Power");
			this.Supply = XmlHelper.GetData<int>(node, "Supply");
			this.ECM = XmlHelper.GetData<float>(node, "ECM");
			this.ECCM = XmlHelper.GetData<float>(node, "ECCM");
			this.ColonizerSpace = XmlHelper.GetData<int>(node, "ColonizerSpace");
			this.SavingsCost = XmlHelper.GetData<int>(node, "SavingsCost");
			this.ProductionCost = XmlHelper.GetData<int>(node, "ProductionCost");
			this.FtlSpeed = XmlHelper.GetData<float>(node, "FtlSpeed");
			this.NodeSpeed = XmlHelper.GetData<float>(node, "NodeSpeed");
			this.MissionTime = XmlHelper.GetData<float>(node, "MissionTime");
			this.CommandPoints = XmlHelper.GetData<int>(node, "CommandPoints");
			this.RepairPoints = XmlHelper.GetData<int>(node, "RepairPoints");
			this.TerraformingPoints = XmlHelper.GetData<int>(node, "TerraformingPoints");
			this.FreighterSpace = XmlHelper.GetData<int>(node, "FreighterSpace");
			this.Signature = XmlHelper.GetData<float>(node, "Signature");
			this.TacticalSensorRange = XmlHelper.GetData<float>(node, "TacticalSensorRange");
			this.StrategicSensorRange = XmlHelper.GetData<float>(node, "StrategicSensorRange");
			this.LaunchDelay = XmlHelper.GetData<float>(node, "LaunchDelay");
			this.DockingDelay = XmlHelper.GetData<float>(node, "DockingDelay");
			this.BattleRiderReserveSize = XmlHelper.GetData<int>(node, "BattleRiderReserveSize");
			this.StationType = XmlHelper.GetData<string>(node, "StationType");
			this.StationLevel = XmlHelper.GetData<int>(node, "StationLevel");
			this.isConstructor = XmlHelper.GetData<bool>(node, "IsConstructor");
			this.isFreighter = XmlHelper.GetData<bool>(node, "IsFreighter");
			this.ConstructionPoints = XmlHelper.GetData<int>(node, "ConstructionPoints");
			this.isPolice = XmlHelper.GetData<bool>(node, "IsPolice");
			this.PsionicPowerLevel = XmlHelper.GetData<float>(node, "PsionicPowerLevel");
			this.SlaveCapacity = XmlHelper.GetData<int>(node, "SlaveCapacity");
			this.FleetSpeedModifier = XmlHelper.GetDataOrDefault<float>(node["FleetSpeedModifier"], 1f);
			this.PreviewImage = XmlHelper.GetData<string>(node, "PreviewImage");
			this.Acceleration = XmlHelper.GetData<float>(node, "Acceleration");
			this.RotationalAccelerationYaw = XmlHelper.GetData<float>(node, "RotationalAccelerationYaw");
			this.RotationalAccelerationPitch = XmlHelper.GetData<float>(node, "RotationalAccelerationPitch");
			this.RotationalAccelerationRoll = XmlHelper.GetData<float>(node, "RotationalAccelerationRoll");
			this.Decceleration = XmlHelper.GetData<float>(node, "Decceleration");
			this.LinearSpeed = XmlHelper.GetData<float>(node, "LinearSpeed");
			this.RotationSpeed = XmlHelper.GetData<float>(node, "RotationSpeed");
			this.Banks = XmlHelper.GetDataObjectCollection<Bank>(node, "Banks", "Bank");
			this.Techs = XmlHelper.GetDataObjectCollection<Tech>(node, "RequiredTechs", "Tech");
			this.Modules = XmlHelper.GetDataObjectCollection<ModuleMount>(node, "Modules", "Module");
			this.ExcludedSections = XmlHelper.GetDataObjectCollection<ExcludedSection>(node, "ExcludedSections", "ExcludedSection");
			this.ExcludedTypes = XmlHelper.GetDataObjectCollection<ExcludedType>(node, "ExcludedTypes", "ExcludedType");
			this.PsionicAbilities = XmlHelper.GetDataObjectCollection<AvailablePsionicAbility>(node, "PsionicAbilities", "PsionicAbility");
			this.WeaponGroups = XmlHelper.GetDataObjectCollection<WeaponGroup>(node, "WeaponGroups", "WeaponGroup");
			this.ShipOptionGroups = XmlHelper.GetDataObjectCollection<ShipOptionGroup>(node, "ShipOptionGroups", ShipOptionGroup.XmlNameShipOptionGroup);
		}
	}
}
