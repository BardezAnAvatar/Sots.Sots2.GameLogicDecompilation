using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class ScenarioEnumerations
	{
		public static string FleetsInRangeVariableName = "$FleetsInRange$";
		public static string[] FleetConditionShipTypes = new string[]
		{
			"Cruiser",
			"Dreadnaught",
			"Leviathan",
			"Colony",
			"Supply"
		};
		public static Dictionary<string, GovernmentInfo.GovernmentType> GovernmentTypes = new Dictionary<string, GovernmentInfo.GovernmentType>
		{

			{
				"Centrism",
				GovernmentInfo.GovernmentType.Centrism
			},

			{
				"Communalism",
				GovernmentInfo.GovernmentType.Communalism
			},

			{
				"Junta",
				GovernmentInfo.GovernmentType.Junta
			},

			{
				"Plutocracy",
				GovernmentInfo.GovernmentType.Plutocracy
			},

			{
				"Socialism",
				GovernmentInfo.GovernmentType.Socialism
			},

			{
				"Mercantilism",
				GovernmentInfo.GovernmentType.Mercantilism
			},

			{
				"Cooperativism",
				GovernmentInfo.GovernmentType.Cooperativism
			},

			{
				"Anarchism",
				GovernmentInfo.GovernmentType.Anarchism
			},

			{
				"Liberationism",
				GovernmentInfo.GovernmentType.Liberationism
			}
		};
		public static Dictionary<string, Type> ContextTypeMap = new Dictionary<string, Type>
		{

			{
				"AlwaysContext",
				typeof(AlwaysContext)
			},

			{
				"StartContext",
				typeof(StartContext)
			},

			{
				"EndContext",
				typeof(EndContext)
			},

			{
				"RangeContext",
				typeof(RangeContext)
			}
		};
		public static Dictionary<string, Type> ConditionTypeMap = new Dictionary<string, Type>
		{

			{
				"GameOver",
				typeof(GameOverCondition)
			},

			{
				"ScalarAmount",
				typeof(ScalarAmountCondition)
			},

			{
				"TriggerTriggered",
				typeof(TriggerTriggeredCondition)
			},

			{
				"SystemRange",
				typeof(SystemRangeCondition)
			},

			{
				"FleetRange",
				typeof(FleetRangeCondition)
			},

			{
				"ProvinceRange",
				typeof(ProvinceRangeCondition)
			},

			{
				"ColonyDeath",
				typeof(ColonyDeathCondition)
			},

			{
				"PlanetDeath",
				typeof(PlanetDeathCondition)
			},

			{
				"FleetDeath",
				typeof(FleetDeathCondition)
			},

			{
				"ShipDeath",
				typeof(ShipDeathCondition)
			},

			{
				"AdmiralDeath",
				typeof(AdmiralDeathCondition)
			},

			{
				"PlayerDeath",
				typeof(PlayerDeathCondition)
			},

			{
				"AllianceFormed",
				typeof(AllianceFormedCondition)
			},

			{
				"AllianceBroken",
				typeof(AllianceBrokenCondition)
			},

			{
				"GrandMenaceAppeared",
				typeof(GrandMenaceAppearedCondition)
			},

			{
				"GrandMenaceDestroyed",
				typeof(GrandMenaceDestroyedCondition)
			},

			{
				"ResourceAmount",
				typeof(ResourceAmountCondition)
			},

			{
				"PlanetAmount",
				typeof(PlanetAmountCondition)
			},

			{
				"BiosphereAmount",
				typeof(BiosphereAmountCondition)
			},

			{
				"AllianceAmount",
				typeof(AllianceAmountCondition)
			},

			{
				"PopulationAmount",
				typeof(PopulationAmountCondition)
			},

			{
				"FleetAmount",
				typeof(FleetAmountCondition)
			},

			{
				"CommandPointAmount",
				typeof(CommandPointAmountCondition)
			},

			{
				"TerrainRange",
				typeof(TerrainRangeCondition)
			},

			{
				"TerrainColonized",
				typeof(TerrainColonizedCondition)
			},

			{
				"PlanetColonized",
				typeof(PlanetColonizedCondition)
			},

			{
				"TreatyBroken",
				typeof(TreatyBrokenCondition)
			},

			{
				"CivilianDeath",
				typeof(CivilianDeathCondition)
			},

			{
				"MoralAmount",
				typeof(MoralAmountCondition)
			},

			{
				"GovernmentType",
				typeof(GovernmentTypeCondition)
			},

			{
				"RevelationBegins",
				typeof(RevelationBeginsCondition)
			},

			{
				"ResearchObtained",
				typeof(ResearchObtainedCondition)
			},

			{
				"ClassBuilt",
				typeof(ClassBuiltCondition)
			},

			{
				"TradePointsAmount",
				typeof(TradePointsAmountCondition)
			},

			{
				"IncomePerTurn",
				typeof(IncomePerTurnAmountCondition)
			},

			{
				"FactionEncountered",
				typeof(FactionEncounteredCondition)
			},

			{
				"WorldType",
				typeof(WorldTypeCondition)
			},

			{
				"PlanetDevelopmentAmount",
				typeof(PlanetDevelopmentAmountCondition)
			}
		};
		public static Dictionary<string, Type> ActionTypeMap = new Dictionary<string, Type>
		{

			{
				"GameOverAction",
				typeof(GameOverAction)
			},

			{
				"PointPerPlanetDeathAction",
				typeof(PointPerPlanetDeathAction)
			},

			{
				"PointPerColonyDeathAction",
				typeof(PointPerColonyDeathAction)
			},

			{
				"PointPerShipTypeAction",
				typeof(PointPerShipTypeAction)
			},

			{
				"AddScalarToScalarAction",
				typeof(AddScalarToScalarAction)
			},

			{
				"SetScalar",
				typeof(SetScalarAction)
			},

			{
				"ChangeScalarAction",
				typeof(ChangeScalarAction)
			},

			{
				"SpawnUnit",
				typeof(SpawnUnitAction)
			},

			{
				"DiplomacyChanged",
				typeof(DiplomacyChangedAction)
			},

			{
				"ColonyChanged",
				typeof(ColonyChangedAction)
			},

			{
				"AIStrategyChanged",
				typeof(AIStrategyChangedAction)
			},

			{
				"ResearchAwarded",
				typeof(ResearchAwardedAction)
			},

			{
				"AdmiralChanged",
				typeof(AdmiralChangedAction)
			},

			{
				"RebellionOccurs",
				typeof(RebellionOccursAction)
			},

			{
				"RebellionEnds",
				typeof(RebellionEndsAction)
			},

			{
				"PlanetDestroyed",
				typeof(PlanetDestroyedAction)
			},

			{
				"PlanetAdded",
				typeof(PlanetAddedAction)
			},

			{
				"TerrainAppears",
				typeof(TerrainAppearsAction)
			},

			{
				"TerrainDisappears",
				typeof(TerrainDisappearsAction)
			},

			{
				"ChangedTreasury",
				typeof(ChangeTreasuryAction)
			},

			{
				"ChangeResources",
				typeof(ChangeResourcesAction)
			},

			{
				"RemoveFleetAction",
				typeof(RemoveFleetAction)
			},

			{
				"AddWeapon",
				typeof(AddWeaponAction)
			},

			{
				"RemoveWeapon",
				typeof(RemoveWeaponAction)
			},

			{
				"AddSection",
				typeof(AddSectionAction)
			},

			{
				"RemoveSection",
				typeof(RemoveSectionAction)
			},

			{
				"AddModule",
				typeof(AddModuleAction)
			},

			{
				"RemoveModule",
				typeof(RemoveModuleAction)
			},

			{
				"ProvinceChanged",
				typeof(ProvinceChangedAction)
			},

			{
				"SurrenderSystem",
				typeof(SurrenderSystemAction)
			},

			{
				"SurrenderEmpire",
				typeof(SurrenderEmpireAction)
			},

			{
				"StratModifierChanged",
				typeof(StratModifierChangedAction)
			},

			{
				"DisplayMessage",
				typeof(DisplayMessageAction)
			},

			{
				"MoveFleet",
				typeof(MoveFleetAction)
			}
		};
		public static Dictionary<string, StationType> StationTypes = new Dictionary<string, StationType>
		{

			{
				"Civilian",
				StationType.CIVILIAN
			},

			{
				"Diplomatic",
				StationType.DIPLOMATIC
			},

			{
				"Naval",
				StationType.NAVAL
			},

			{
				"Science",
				StationType.SCIENCE
			},

			{
				"Gate",
				StationType.GATE
			}
		};
		public static string[] PlayerRelations = new string[]
		{
			"Aggressive"
		};
		public static string[] DiplomacyRules = new string[]
		{
			"Alliance",
			"Non-Aggression Pact",
			"Cease-Fire Agreement",
			"War"
		};
		public static string[] Factions = new string[]
		{
			"",
			"hiver",
			"human",
			"liir_zuul",
			"morrigi",
			"tarkas",
			"zuul"
		};
		public static string[] Races = new string[]
		{
			"hiver",
			"hordezuul",
			"human",
			"liir",
			"morrigi",
			"presterzuul",
			"tarka"
		};
		public static int[] StationStages = new int[]
		{
			1,
			2,
			3,
			4,
			5
		};
		public static string[] AdmiralGenders = new string[]
		{
			"",
			"Male",
			"Female",
			"Other"
		};
		public static string[] AIDifficulty = new string[]
		{
			"",
			"Easy",
			"Normal",
			"Difficult"
		};
		public static List<string> GetRacesForFaction(string faction)
		{
			List<string> list = new List<string>();
			switch (faction)
			{
			case "hiver":
				list.Add("hiver");
				break;
			case "human":
				list.Add("human");
				break;
			case "liir_zuul":
				list.Add("liir");
				list.Add("presterzuul");
				break;
			case "morrigi":
				list.Add("morrigi");
				list.Add("human");
				list.Add("tarka");
				list.Add("hiver");
				list.Add("presterzuul");
				list.Add("liir");
				break;
			case "tarkas":
				list.Add("tarka");
				break;
			case "zuul":
				list.Add("hordezuul");
				break;
			case "loa":
				list.Add("loa");
				break;
			}
			return list;
		}
		public static List<string> GetFactionsForRace(string race)
		{
			List<string> list = new List<string>();
			switch (race)
			{
			case "hiver":
				list.Add("hiver");
				list.Add("morrigi");
				break;
			case "human":
				list.Add("human");
				list.Add("morrigi");
				break;
			case "hordezuul":
				list.Add("zuul");
				break;
			case "liir":
				list.Add("liir_zuul");
				list.Add("morrigi");
				break;
			case "morrigi":
				list.Add("morrigi");
				break;
			case "presterzuul":
				list.Add("liir_zuul");
				list.Add("morrigi");
				break;
			case "tarka":
				list.Add("tarkas");
				list.Add("morrigi");
				break;
			}
			return list;
		}
	}
}
