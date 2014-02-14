using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.ShipFramework
{
	internal class ShipSectionAsset
	{
		public string Title;
		public string Description;
		public string FileName;
		public string SectionName;
		public string Faction;
		public ShipSectionType Type;
		public ShipClass Class;
		public RealShipClasses RealClass;
		public string AmbientSound = "";
		public string EngineSound = "";
		public string UnderAttackSound = "";
		public string DestroyedSound = "";
		public BattleRiderTypes BattleRiderType;
		public string ModelName;
		public string DestroyedModelName;
		public string DamagedModelName;
		public string[] RequiredTechs;
		public List<string[]> ShipOptions = new List<string[]>();
		public LogicalBank[] Banks;
		public LogicalMount[] Mounts;
		public LogicalModuleMount[] Modules;
		public ShipSectionType[] ExcludeSectionTypes;
		public SectionEnumerations.PsionicAbility[] PsionicAbilities;
		public LogicalEffect DamageEffect;
		public LogicalEffect DeathEffect;
		public LogicalEffect ReactorFailureDeathEffect;
		public LogicalEffect ReactorCriticalDeathEffect;
		public LogicalEffect AbsorbedDeathEffect;
		public string[] ExcludeSections;
		public CarrierType CarrierType;
		public bool IsCarrier;
		public bool IsBattleRider;
		public bool isPolice;
		public bool isPropaganda;
		public bool IsSuperTransport;
		public bool IsBoreShip;
		public bool IsSupplyShip;
		public bool IsGateShip;
		public bool IsFreighter;
		public bool IsScavenger;
		public bool IsWraithAbductor;
		public bool IsAccelerator;
		public bool IsLoaCube;
		public bool IsTrapShip;
		public bool IsGravBoat;
		public bool IsAbsorberSection;
		public bool IsFireControl;
		public bool IsAIControl;
		public bool IsListener;
		public int Structure = 1000;
		public int LowStruct;
		public SuulkaType SuulkaType;
		public float Mass = 10000f;
		public int SavingsCost = 30000;
		public int ProductionCost = 2000;
		public int ColonizationSpace;
		public int TerraformingSpace;
		public int ConstructionPoints;
		public int FreighterSpace;
		public int ReserveSize;
		public int RepairPoints;
		public float FtlSpeed;
		public float NodeSpeed;
		public float MissionTime;
		public float LaunchDelay;
		public float DockingDelay;
		public int Crew;
		public int CrewRequired;
		public int Power;
		public int Supply;
		public int SlaveCapacity;
		public float ShipExplosiveDamage;
		public float ShipExplosiveRange;
		public float PsionicPowerLevel;
		public float ECCM;
		public float ECM;
		public int CommandPoints;
		public float Signature;
		public float TacticalSensorRange;
		public float StrategicSensorRange;
		public float FleetSpeedModifier = 1f;
		public StationType StationType;
		public SectionEnumerations.CombatAiType CombatAIType;
		public ShipFleetAbilityType ShipFleetAbilityType;
		public int StationLevel;
		public bool isConstructor;
		public bool isDeepScan;
		public bool isMineLayer;
		public CloakingType cloakingType;
		public bool hasJammer;
		public string ManeuveringType = "";
		public ShipManeuveringInfo Maneuvering = new ShipManeuveringInfo();
		public readonly Kerberos.Sots.Framework.Size[] Armor = new Kerberos.Sots.Framework.Size[]
		{
			new Kerberos.Sots.Framework.Size
			{
				X = 10,
				Y = 10
			},
			new Kerberos.Sots.Framework.Size
			{
				X = 10,
				Y = 10
			},
			new Kerberos.Sots.Framework.Size
			{
				X = 10,
				Y = 10
			},
			new Kerberos.Sots.Framework.Size
			{
				X = 10,
				Y = 10
			}
		};
		public bool IsSuulka
		{
			get
			{
				return this.SuulkaType != SuulkaType.None;
			}
		}
		private static PlatformTypes? GetPlatformType(string sectionname)
		{
			if (sectionname == "sn_dronesat" || sectionname == "sn_drone" || sectionname == "sn_drone_satellite")
			{
				return new PlatformTypes?(PlatformTypes.dronesat);
			}
			if (sectionname == "sn_brsat" || sectionname == "sn_br_satellite")
			{
				return new PlatformTypes?(PlatformTypes.brsat);
			}
			if (sectionname == "sn_torpsat" || sectionname == "sn_torpedo_satellite" || sectionname == "sn_torp_satellite")
			{
				return new PlatformTypes?(PlatformTypes.torpsat);
			}
			if (sectionname == "sn_scansat" || sectionname == "sn_scan_satellite")
			{
				return new PlatformTypes?(PlatformTypes.scansat);
			}
			if (sectionname == "sn_asteroid_monitor")
			{
				return new PlatformTypes?(PlatformTypes.monitorsat);
			}
			if (sectionname == "sn_missile_satellite")
			{
				return new PlatformTypes?(PlatformTypes.missilesat);
			}
			return null;
		}
		public PlatformTypes? GetPlatformType()
		{
			return ShipSectionAsset.GetPlatformType(this.SectionName);
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.Faction,
				" ",
				this.Class.ToString(),
				" ",
				this.ModelName,
				" (",
				this.Type.ToString(),
				")"
			});
		}
		private static string GetShipDefaultDeathEffect(ShipClass sc, BattleRiderTypes brt = BattleRiderTypes.Unspecified)
		{
			switch (sc)
			{
			case ShipClass.Cruiser:
				return "effects\\ShipDeath\\cr_death.effect";
			case ShipClass.Dreadnought:
			case ShipClass.Leviathan:
				return "effects\\ShipDeath\\dn_death.effect";
			case ShipClass.BattleRider:
				if (brt == BattleRiderTypes.biomissile)
				{
					return "effects\\Weapons\\biomissile_impact.effect";
				}
				break;
			}
			return "effects\\ShipDeath\\placeholder.effect";
		}
		private static string GetReactorShieldFailureDeathEffect(ShipClass sc, ShipSectionType sst)
		{
			if (sst == ShipSectionType.Engine)
			{
				switch (sc)
				{
				case ShipClass.Cruiser:
					return "effects\\ShipDeath\\Cruiser_SRF.effect";
				case ShipClass.Dreadnought:
					return "effects\\ShipDeath\\Dreadnought_SRF.effect";
				case ShipClass.Leviathan:
					return "effects\\ShipDeath\\Levi_SRF.effect";
				}
			}
			return ShipSectionAsset.GetShipDefaultDeathEffect(sc, BattleRiderTypes.Unspecified);
		}
		private static string GetReactorCriticalDeathEffect(ShipClass sc, ShipSectionType sst, string fileName)
		{
			if (sst == ShipSectionType.Engine || sc == ShipClass.Leviathan)
			{
				switch (sc)
				{
				case ShipClass.Cruiser:
					if (fileName.Contains("Antimatter") || fileName.Contains("antimatter"))
					{
						return "effects\\ShipDeath\\Cruiser_CRF_Antimatter.effect";
					}
					if (fileName.Contains("Fusion") || fileName.Contains("fusion"))
					{
						return "effects\\ShipDeath\\Cruiser_CRF_Fusion.effect";
					}
					break;
				case ShipClass.Dreadnought:
					if (fileName.Contains("Antimatter") || fileName.Contains("antimatter"))
					{
						return "effects\\ShipDeath\\Dreadnought_CRF_Antimatter.effect";
					}
					if (fileName.Contains("Fusion") || fileName.Contains("fusion"))
					{
						return "effects\\ShipDeath\\Dreadnought_CRF_Fusion.effect";
					}
					break;
				case ShipClass.Leviathan:
					return "effects\\ShipDeath\\Levi_CRF_Antimatter.effect";
				}
			}
			return ShipSectionAsset.GetShipDefaultDeathEffect(sc, BattleRiderTypes.Unspecified);
		}
		private static string GetAbsorbedDeathEffect(ShipClass sc, ShipSectionType sst)
		{
			if (sst == ShipSectionType.Mission)
			{
				switch (sc)
				{
				case ShipClass.Cruiser:
					return "effects\\ShipDeath\\Cruiser_SRF.effect";
				case ShipClass.Dreadnought:
					return "effects\\ShipDeath\\Dreadnought_SRF.effect";
				case ShipClass.Leviathan:
					return "effects\\ShipDeath\\Levi_SRF.effect";
				}
			}
			return "";
		}
		public int GetExtraArmorLayers()
		{
			switch (this.Class)
			{
			case ShipClass.Cruiser:
			case ShipClass.Station:
				return 1;
			case ShipClass.Dreadnought:
				return 2;
			case ShipClass.Leviathan:
				return 3;
			}
			return 0;
		}
		public DamagePattern CreateFreshArmor(ArmorSide side, int ArmorWidthModifier)
		{
			return new DamagePattern(this.Armor[(int)side].X, Math.Max(0, this.Armor[(int)side].Y + ArmorWidthModifier));
		}
		public bool SectionIsExcluded(ShipSectionAsset section)
		{
			string sectionName = Path.GetFileNameWithoutExtension(section.FileName);
			return this.ExcludeSectionTypes.Any((ShipSectionType x) => x == section.Type) || this.ExcludeSections.Any((string x) => x == sectionName);
		}
		public bool CarrierTypeMatchesRole(ShipRole role)
		{
			if (role == ShipRole.CARRIER)
			{
				return this.CarrierType == CarrierType.Destroyer || this.CarrierType == CarrierType.BattleCruiser || this.CarrierType == CarrierType.BattleShip;
			}
			switch (role)
			{
			case ShipRole.CARRIER_ASSAULT:
				return this.CarrierType == CarrierType.AssaultShuttle;
			case ShipRole.CARRIER_DRONE:
				return this.CarrierType == CarrierType.Drone;
			case ShipRole.CARRIER_BIO:
				return this.CarrierType == CarrierType.BioMissile;
			case ShipRole.CARRIER_BOARDING:
				return this.CarrierType == CarrierType.BoardingPod;
			default:
				return false;
			}
		}
		public static bool IsBattleRiderClass(RealShipClasses shipClass)
		{
			switch (shipClass)
			{
			case RealShipClasses.BattleRider:
			case RealShipClasses.BattleCruiser:
			case RealShipClasses.BattleShip:
			case RealShipClasses.Drone:
			case RealShipClasses.BoardingPod:
			case RealShipClasses.EscapePod:
			case RealShipClasses.AssaultShuttle:
			case RealShipClasses.Biomissile:
				return true;
			default:
				return false;
			}
		}
		public static bool IsWeaponBattleRiderClass(RealShipClasses shipClass)
		{
			switch (shipClass)
			{
			case RealShipClasses.Drone:
			case RealShipClasses.BoardingPod:
			case RealShipClasses.EscapePod:
			case RealShipClasses.AssaultShuttle:
			case RealShipClasses.Biomissile:
				return true;
			default:
				return false;
			}
		}
		public static CarrierType GetCarrierType(List<LogicalBank> banks)
		{
			int[] array = new int[8];
			for (int i = 0; i < 8; i++)
			{
				array[i] = 0;
			}
			foreach (LogicalBank current in banks)
			{
				switch (current.TurretClass)
				{
				case WeaponEnums.TurretClasses.Biomissile:
					array[3]++;
					break;
				case WeaponEnums.TurretClasses.Drone:
					array[1]++;
					break;
				case WeaponEnums.TurretClasses.AssaultShuttle:
					array[2]++;
					break;
				case WeaponEnums.TurretClasses.DestroyerRider:
					array[5]++;
					break;
				case WeaponEnums.TurretClasses.CruiserRider:
					array[6]++;
					break;
				case WeaponEnums.TurretClasses.DreadnoughtRider:
					array[7]++;
					break;
				case WeaponEnums.TurretClasses.BoardingPod:
					array[4]++;
					break;
				}
			}
			CarrierType result = CarrierType.None;
			int num = 0;
			for (int j = 0; j < 8; j++)
			{
				if (array[j] > num)
				{
					num = array[j];
					result = (CarrierType)j;
				}
			}
			return result;
		}
		public void LoadFromXml(AssetDatabase assetdb, string filename, string faction, ShipSectionType sectionType, ShipClass sectionClass)
		{
			ShipSection shipSection = new ShipSection();
			ShipXmlUtility.LoadShipSectionFromXml(filename, ref shipSection);
			this.Type = sectionType;
			this.Class = sectionClass;
			this.Faction = faction;
			this.SectionName = Path.GetFileNameWithoutExtension(filename);
			string text = "";
			switch (this.Class)
			{
			case ShipClass.Cruiser:
				text = "CR";
				break;
			case ShipClass.Dreadnought:
				text = "DN";
				break;
			case ShipClass.Leviathan:
				text = "LV";
				break;
			case ShipClass.BattleRider:
				text = "BR";
				break;
			case ShipClass.Station:
				text = "SN";
				break;
			}
			this.ModelName = FileSystemHelpers.StripMountName(shipSection.ModelPath);
			string text2 = Path.Combine(Path.GetDirectoryName(this.ModelName), string.Concat(new string[]
			{
				"Damage_",
				text,
				"_",
				this.Type.ToString(),
				"_Default.scene"
			}));
			this.DestroyedModelName = (string.IsNullOrEmpty(shipSection.DestroyedModelPath) ? text2 : shipSection.DestroyedModelPath);
			this.DamagedModelName = shipSection.DamageModelPath;
			this.AmbientSound = shipSection.AmbientSound;
			this.EngineSound = shipSection.EngineSound;
			string text3 = string.Format("COMBAT_023-01_{0}_GeneralShipsBeingAttacked", faction);
			string text4 = "";
			switch (this.Class)
			{
			case ShipClass.Cruiser:
				text4 = string.Format("COMBAT_029-01_{0}_CruiserDestroyed", faction);
				break;
			case ShipClass.Dreadnought:
				text4 = string.Format("COMBAT_030-01_{0}_DreadnoughtDestroyed", faction);
				break;
			case ShipClass.Leviathan:
				text4 = string.Format("COMBAT_031-01_{0}_LeviathanDestroyed", faction);
				break;
			case ShipClass.BattleRider:
				text4 = string.Format("COMBAT_020-01_{0}_BattleRiderDestroyed", faction);
				break;
			case ShipClass.Station:
				switch (this.StationType)
				{
				case StationType.NAVAL:
					text3 = string.Format("COMBAT_067-01_{0}_NavalStationUnderAttack", faction);
					text4 = string.Format("COMBAT_066-01_{0}_NavalStationDestroyed", faction);
					break;
				case StationType.SCIENCE:
					text3 = string.Format("COMBAT_069-01_{0}_ScienceStationUnderAttack", faction);
					text4 = string.Format("COMBAT_068-01_{0}_ScienceStationDestroyed", faction);
					break;
				case StationType.CIVILIAN:
					text3 = string.Format("COMBAT_071-01_{0}_CivilianStationUnderAttack", faction);
					text4 = string.Format("COMBAT_072-01_{0}_CivilianStationDestroyed", faction);
					break;
				case StationType.DIPLOMATIC:
					text3 = string.Format("COMBAT_070a-01_{0}_DiplomaticStationUnderAttack", faction);
					text4 = string.Format("COMBAT_070-01_{0}_DiplomaticStationDestroyed", faction);
					break;
				}
				break;
			}
			this.UnderAttackSound = (string.IsNullOrEmpty(shipSection.UnderAttackSound) ? text3 : shipSection.UnderAttackSound);
			this.DestroyedSound = (string.IsNullOrEmpty(shipSection.DestroyedSound) ? text4 : shipSection.DestroyedSound);
			this.Title = shipSection.Title;
			this.Description = shipSection.Description;
			if (string.IsNullOrWhiteSpace(this.Title))
			{
				this.Title = Path.GetFileNameWithoutExtension(filename);
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
			this.CombatAIType = ((!string.IsNullOrEmpty(shipSection.CombatAiType)) ? ((SectionEnumerations.CombatAiType)Enum.Parse(typeof(SectionEnumerations.CombatAiType), shipSection.CombatAiType)) : SectionEnumerations.CombatAiType.Normal);
			SectionEnumerations.CombatAiType combatAIType = this.CombatAIType;
			if (combatAIType <= SectionEnumerations.CombatAiType.VonNeumannSeekerProbe)
			{
				switch (combatAIType)
				{
				case SectionEnumerations.CombatAiType.Drone:
					this.SetBattleRiderType(BattleRiderTypes.drone);
					goto IL_36B;
				case SectionEnumerations.CombatAiType.AssaultShuttle:
					this.SetBattleRiderType(BattleRiderTypes.assaultshuttle);
					goto IL_36B;
				case SectionEnumerations.CombatAiType.NodeFighter:
					this.SetBattleRiderType(BattleRiderTypes.nodefighter);
					goto IL_36B;
				default:
					switch (combatAIType)
					{
					case SectionEnumerations.CombatAiType.Swarmer:
					case SectionEnumerations.CombatAiType.SwarmerGuardian:
						break;
					default:
						if (combatAIType != SectionEnumerations.CombatAiType.VonNeumannSeekerProbe)
						{
							goto IL_358;
						}
						this.SetBattleRiderType(BattleRiderTypes.assaultshuttle);
						goto IL_36B;
					}
					break;
				}
			}
			else
			{
				if (combatAIType != SectionEnumerations.CombatAiType.VonNeumannPyramid && combatAIType != SectionEnumerations.CombatAiType.LocustFighter && combatAIType != SectionEnumerations.CombatAiType.MorrigiCrow)
				{
					goto IL_358;
				}
			}
			this.SetBattleRiderType(BattleRiderTypes.battlerider);
			goto IL_36B;
			IL_358:
			this.SetBattleRiderType(ObtainShipClassTypes.GetBattleRiderTypeByName(this.Class, fileNameWithoutExtension));
			IL_36B:
			if (fileNameWithoutExtension.Contains("protectorate"))
			{
				this.ShipFleetAbilityType = ShipFleetAbilityType.Protectorate;
			}
			else
			{
				if (fileNameWithoutExtension.Contains("suulka_the_hidden"))
				{
					this.ShipFleetAbilityType = ShipFleetAbilityType.Hidden;
				}
				else
				{
					if (fileNameWithoutExtension.Contains("suulka_the_deaf"))
					{
						this.ShipFleetAbilityType = ShipFleetAbilityType.Deaf;
					}
				}
			}
			this.IsSuperTransport = this.SectionName.StartsWith("lv_supertransport", StringComparison.InvariantCultureIgnoreCase);
			this.IsBoreShip = this.SectionName.EndsWith("bore", StringComparison.InvariantCultureIgnoreCase);
			this.IsSupplyShip = this.SectionName.Contains("_supply");
			this.IsGateShip = this.SectionName.StartsWith("cr_mis_gate", StringComparison.InvariantCultureIgnoreCase);
			this.IsTrapShip = this.SectionName.StartsWith("cr_mis_colonytrickster", StringComparison.InvariantCultureIgnoreCase);
			this.IsGravBoat = this.SectionName.StartsWith("cr_mis_gravboat", StringComparison.InvariantCultureIgnoreCase);
			this.IsAbsorberSection = (this.SectionName.Contains("_absorber") || this.SectionName.Contains("_absorbtion"));
			this.IsListener = this.SectionName.Contains("_listener");
			this.IsFireControl = (this.Title.Contains("CR_CMD_FIRECONTROL") || this.Title.Contains("CR_CMD_FIRE_CONTROL"));
			this.IsAIControl = this.Title.Contains("CR_CMD_AI");
			this.SuulkaType = this.GetSuulkaType(this.SectionName);
			this.IsFreighter = shipSection.isFreighter;
			this.FreighterSpace = shipSection.FreighterSpace;
			this.isPolice = shipSection.isPolice;
			this.SlaveCapacity = shipSection.SlaveCapacity;
			this.isPropaganda = this.SectionName.StartsWith("cr_mis_propaganda", StringComparison.InvariantCultureIgnoreCase);
			this.IsAccelerator = this.SectionName.StartsWith("cr_mis_ngp", StringComparison.InvariantCultureIgnoreCase);
			this.IsLoaCube = this.SectionName.StartsWith("cr_mis_cube", StringComparison.InvariantCultureIgnoreCase);
			this.IsScavenger = (this.FileName.Contains("mis_scavenger") || this.FileName.Contains("dn_mis_subjugator"));
			this.IsWraithAbductor = this.FileName.Contains("wraith_abductor");
			Kerberos.Sots.Framework.Size size = default(Kerberos.Sots.Framework.Size);
			size.X = shipSection.TopArmor.X;
			size.Y = shipSection.TopArmor.Y;
			this.Armor[1] = size;
			Kerberos.Sots.Framework.Size size2 = default(Kerberos.Sots.Framework.Size);
			size2.X = shipSection.BottomArmor.X;
			size2.Y = shipSection.BottomArmor.Y;
			this.Armor[3] = size2;
			Kerberos.Sots.Framework.Size size3 = default(Kerberos.Sots.Framework.Size);
			size3.X = shipSection.SideArmor.X;
			size3.Y = shipSection.SideArmor.Y;
			this.Armor[2] = (this.Armor[0] = size3);
			this.Structure = shipSection.Struct;
			this.LowStruct = shipSection.StructDamageAmount;
			this.Mass = (float)shipSection.Mass;
			this.SavingsCost = shipSection.SavingsCost;
			this.ProductionCost = shipSection.ProductionCost;
			this.ColonizationSpace = shipSection.ColonizerSpace;
			this.TerraformingSpace = shipSection.TerraformingPoints;
			this.ConstructionPoints = shipSection.ConstructionPoints;
			this.ReserveSize = shipSection.BattleRiderReserveSize;
			this.RepairPoints = shipSection.RepairPoints;
			this.FtlSpeed = shipSection.FtlSpeed;
			this.NodeSpeed = shipSection.NodeSpeed;
			this.MissionTime = shipSection.MissionTime;
			this.LaunchDelay = shipSection.LaunchDelay;
			this.DockingDelay = shipSection.DockingDelay;
			this.Crew = shipSection.Crew;
			this.CrewRequired = shipSection.CrewRequired;
			this.Power = shipSection.Power;
			this.Supply = shipSection.Supply;
			this.ECM = shipSection.ECM;
			this.ECCM = shipSection.ECCM;
			this.CommandPoints = shipSection.CommandPoints;
			this.Signature = shipSection.Signature;
			this.TacticalSensorRange = shipSection.TacticalSensorRange;
			this.ShipExplosiveDamage = shipSection.DeathDamage;
			this.ShipExplosiveRange = shipSection.ExplosionRadius;
			this.PsionicPowerLevel = shipSection.PsionicPowerLevel;
			if (this.TacticalSensorRange <= 0f)
			{
				this.TacticalSensorRange = 20000f;
			}
			this.DamageEffect = new LogicalEffect
			{
				Name = (!string.IsNullOrEmpty(shipSection.DamageEffectPath)) ? shipSection.DamageEffectPath : ShipSectionAsset.GetShipDefaultDeathEffect(this.Class, this.BattleRiderType)
			};
			this.DeathEffect = new LogicalEffect
			{
				Name = (!string.IsNullOrEmpty(shipSection.DestroyedEffectPath)) ? shipSection.DestroyedEffectPath : ShipSectionAsset.GetShipDefaultDeathEffect(this.Class, this.BattleRiderType)
			};
			this.ReactorFailureDeathEffect = new LogicalEffect
			{
				Name = ShipSectionAsset.GetReactorShieldFailureDeathEffect(this.Class, this.Type)
			};
			this.ReactorCriticalDeathEffect = new LogicalEffect
			{
				Name = ShipSectionAsset.GetReactorCriticalDeathEffect(this.Class, this.Type, fileNameWithoutExtension)
			};
			this.AbsorbedDeathEffect = new LogicalEffect
			{
				Name = ShipSectionAsset.GetAbsorbedDeathEffect(this.Class, this.Type)
			};
			this.StrategicSensorRange = shipSection.StrategicSensorRange;
			this.FleetSpeedModifier = shipSection.FleetSpeedModifier;
			if (this.StrategicSensorRange <= 0f)
			{
				this.StrategicSensorRange = assetdb.DefaultStratSensorRange;
			}
			this.StationType = ((shipSection.StationType != null) ? SectionEnumerations.StationTypesWithInvalid[shipSection.StationType] : StationType.INVALID_TYPE);
			this.StationLevel = shipSection.StationLevel;
			this.isConstructor = shipSection.isConstructor;
			this.Maneuvering.LinearAccel = shipSection.Acceleration;
			this.Maneuvering.RotAccel.X = shipSection.RotationalAccelerationYaw;
			this.Maneuvering.RotAccel.Y = shipSection.RotationalAccelerationPitch;
			this.Maneuvering.RotAccel.Z = shipSection.RotationalAccelerationRoll;
			this.Maneuvering.Deacceleration = shipSection.Decceleration;
			this.Maneuvering.LinearSpeed = shipSection.LinearSpeed;
			this.Maneuvering.RotationSpeed = shipSection.RotationSpeed;
			HashSet<string> hashSet = new HashSet<string>();
			foreach (Kerberos.Sots.Data.ShipFramework.Tech current in shipSection.Techs)
			{
				hashSet.Add(current.Name);
				this.isDeepScan = (this.isDeepScan || current.Name == "CCC_Advanced_Sensors");
				this.hasJammer = (this.hasJammer || current.Name == "CCC_Sensor_Jammer");
				string name;
				if (this.cloakingType == CloakingType.None && (name = current.Name) != null)
				{
					if (!(name == "SLD_Cloaking"))
					{
						if (name == "SLD_Improved_Cloaking")
						{
							this.cloakingType = CloakingType.ImprovedCloaking;
						}
					}
					else
					{
						this.cloakingType = CloakingType.Cloaking;
					}
				}
			}
			List<HashSet<string>> list = new List<HashSet<string>>();
			foreach (ShipOptionGroup current2 in shipSection.ShipOptionGroups)
			{
				HashSet<string> hashSet2 = new HashSet<string>();
				foreach (ShipOption current3 in current2.ShipOptions)
				{
					hashSet2.Add(current3.Name);
				}
				list.Add(hashSet2);
			}
			switch (sectionClass)
			{
			case ShipClass.Cruiser:
				hashSet.Add("ENG_Cruiser_Construction");
				break;
			case ShipClass.Dreadnought:
				hashSet.Add("ENG_Dreadnought_Construction");
				break;
			case ShipClass.Leviathan:
				hashSet.Add("ENG_Leviathian_Construction");
				break;
			}
			List<LogicalModuleMount> list2 = new List<LogicalModuleMount>();
			foreach (ModuleMount current4 in shipSection.Modules)
			{
				LogicalModuleMount logicalModuleMount = new LogicalModuleMount
				{
					Section = this
				};
				logicalModuleMount.AssignedModuleName = current4.AssignedModuleName;
				logicalModuleMount.ModuleType = current4.Type;
				logicalModuleMount.NodeName = current4.NodeName;
				logicalModuleMount.FrameX = current4.FrameX;
				logicalModuleMount.FrameY = current4.FrameY;
				list2.Add(logicalModuleMount);
			}
			List<LogicalBank> list3 = new List<LogicalBank>();
			List<LogicalMount> list4 = new List<LogicalMount>();
			foreach (Bank current5 in shipSection.Banks)
			{
				LogicalBank logicalBank = new LogicalBank
				{
					TurretSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), current5.Size),
					Section = this,
					Module = null,
					GUID = Guid.Parse(current5.Id),
					DefaultWeaponName = current5.DefaultWeapon
				};
				logicalBank.TurretClass = (WeaponEnums.TurretClasses)Enum.Parse(typeof(WeaponEnums.TurretClasses), current5.Class);
				logicalBank.FrameX = current5.FrameX;
				logicalBank.FrameY = current5.FrameY;
				this.IsCarrier = (this.IsCarrier || WeaponEnums.IsBattleRider(logicalBank.TurretClass));
				this.isMineLayer = (this.isMineLayer || logicalBank.TurretClass == WeaponEnums.TurretClasses.Minelayer);
				list3.Add(logicalBank);
				foreach (Mount current6 in current5.Mounts)
				{
					LogicalMount logicalMount = new LogicalMount();
					logicalMount.Bank = logicalBank;
					logicalMount.NodeName = current6.NodeName;
					logicalMount.TurretOverload = current6.TurretOverload;
					logicalMount.BarrelOverload = current6.BarrelOverload;
					logicalMount.BaseOverload = current6.BaseOverload;
					logicalMount.FireAnimName = ((current6.SectionFireAnimation != null) ? current6.SectionFireAnimation : "");
					logicalMount.ReloadAnimName = ((current6.SectionReloadAnimation != null) ? current6.SectionReloadAnimation : "");
					logicalMount.Yaw.Min = current6.YawMin;
					logicalMount.Yaw.Max = current6.YawMax;
					logicalMount.Pitch.Min = current6.PitchMin;
					logicalMount.Pitch.Max = current6.PitchMax;
					logicalMount.Pitch.Min = Math.Max(-90f, logicalMount.Pitch.Min);
					logicalMount.Pitch.Max = Math.Min(90f, logicalMount.Pitch.Max);
					list4.Add(logicalMount);
				}
			}
			if (this.IsCarrier)
			{
				this.CarrierType = ShipSectionAsset.GetCarrierType(list3);
			}
			List<string> list5 = new List<string>();
			List<ShipSectionType> list6 = new List<ShipSectionType>();
			foreach (ExcludedSection current7 in shipSection.ExcludedSections)
			{
				list5.Add(current7.Name);
			}
			foreach (ExcludedType current8 in shipSection.ExcludedTypes)
			{
				ShipSectionType item = ShipSectionType.Command;
				if (current8.Name == "Engine")
				{
					item = ShipSectionType.Engine;
				}
				else
				{
					if (current8.Name == "Mission")
					{
						item = ShipSectionType.Mission;
					}
				}
				list6.Add(item);
			}
			List<SectionEnumerations.PsionicAbility> list7 = new List<SectionEnumerations.PsionicAbility>();
			foreach (AvailablePsionicAbility current9 in shipSection.PsionicAbilities)
			{
				list7.Add((SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), current9.Name));
			}
			foreach (HashSet<string> current10 in list)
			{
				this.ShipOptions.Add(current10.ToArray<string>());
			}
			this.RequiredTechs = hashSet.ToArray<string>();
			this.Banks = list3.ToArray();
			this.Mounts = list4.ToArray();
			this.Modules = list2.ToArray();
			this.ExcludeSections = list5.ToArray();
			this.ExcludeSectionTypes = list6.ToArray();
			this.PsionicAbilities = list7.ToArray();
			if (!shipSection.RealShipClass.HasValue)
			{
				if (this.RealClass != RealShipClasses.BattleCruiser && this.RealClass != RealShipClasses.BattleShip)
				{
					this.RealClass = ObtainShipClassTypes.GetRealShipClass(this.Class, this.BattleRiderType, filename);
				}
			}
			else
			{
				this.RealClass = shipSection.RealShipClass.Value;
			}
			if (this.CombatAIType == SectionEnumerations.CombatAiType.VonNeumannDisc)
			{
				this.cloakingType = CloakingType.ImprovedCloaking;
			}
		}
		private void SetBattleRiderType(BattleRiderTypes brt)
		{
			this.BattleRiderType = brt;
			this.IsBattleRider = (brt != BattleRiderTypes.Unspecified);
		}
		private SuulkaType GetSuulkaType(string sectionName)
		{
			if (sectionName == "lv_suulka_the_cannibal")
			{
				return SuulkaType.TheCannibal;
			}
			if (sectionName == "lv_suulka_the_deaf")
			{
				return SuulkaType.TheDeaf;
			}
			if (sectionName == "lv_suulka_the_hidden")
			{
				return SuulkaType.TheHidden;
			}
			if (sectionName == "lv_suulka_the_immortal")
			{
				return SuulkaType.TheImmortal;
			}
			if (sectionName == "lv_suulka_the_kraken")
			{
				return SuulkaType.TheKraken;
			}
			if (sectionName == "lv_suulka_the_shaper")
			{
				return SuulkaType.TheBloodweaver;
			}
			if (sectionName == "lv_suulka_the_siren")
			{
				return SuulkaType.TheSiren;
			}
			if (sectionName == "lv_suulka_usurper")
			{
				return SuulkaType.TheUsurper;
			}
			if (sectionName == "lv_blackelder")
			{
				return SuulkaType.TheBlack;
			}
			return SuulkaType.None;
		}
	}
}
