using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class SectionEnumerations
	{
		public enum CombatAiType
		{
			Normal,
			Drone,
			AssaultShuttle,
			NodeFighter,
			SuicideDrone,
			DrainerDrone,
			EMPDrone,
			EWDrone,
			TrapDrone,
			Swarmer,
			SwarmerGuardian,
			SwarmerHive,
			SwarmerQueenLarva,
			SwarmerQueen,
			VonNeumannCollectorMotherShip,
			VonNeumannCollectorProbe,
			VonNeumannSeekerMotherShip,
			VonNeumannSeekerProbe,
			VonNeumannBerserkerMotherShip,
			VonNeumannNeoBerserker,
			VonNeumannDisc,
			VonNeumannPyramid,
			VonNeumannPlanetKiller,
			LocustMoon,
			LocustWorld,
			LocustFighter,
			SystemKiller,
			MorrigiRelic,
			MorrigiCrow,
			Meteor,
			Comet,
			Specter,
			Gardener,
			Protean,
			CommandMonitor,
			NormalMonitor,
			GhostShip
		}
		public enum DesignAttribute
		{
			Fast_In_The_Curves,
			Nimble_Lil_Minx,
			Bit_Of_A_Hog,
			Muscle_Machine,
			Hard_Luck_Ship,
			Aces_And_Eights,
			Ol_Ironsides,
			Ghost_Of_The_Hood,
			Ol_Yellow_Streak,
			Louis_And_Clark,
			Four_Eyes,
			Spirit_Of_The_Yorktown,
			Sniper,
			Dead_Eye,
			Death_Trap
		}
		public enum BattleRiderClass
		{
			Drone,
			BoardingPod,
			EscapePod,
			AssaultShuttle,
			Patrol,
			Scout,
			Spinal,
			Escort,
			Intercepter,
			BioMissile,
			Torpedo
		}
		public enum PsionicAbility
		{
			TKFist,
			Hold,
			Crush,
			Reflector,
			Repair,
			AbaddonLaser,
			Fear,
			Inspiration,
			Reveal,
			Posses,
			Listen,
			Block,
			PsiDrain,
			WildFire,
			Control,
			LifeDrain,
			Mirage,
			FalseFriend,
			Invisibility,
			Movement,
			None = -1
		}
		public static List<SectionEnumerations.DesignAttribute> GoodDesignAttributes = new List<SectionEnumerations.DesignAttribute>
		{
			SectionEnumerations.DesignAttribute.Fast_In_The_Curves,
			SectionEnumerations.DesignAttribute.Nimble_Lil_Minx,
			SectionEnumerations.DesignAttribute.Muscle_Machine,
			SectionEnumerations.DesignAttribute.Aces_And_Eights,
			SectionEnumerations.DesignAttribute.Ol_Ironsides,
			SectionEnumerations.DesignAttribute.Louis_And_Clark,
			SectionEnumerations.DesignAttribute.Spirit_Of_The_Yorktown,
			SectionEnumerations.DesignAttribute.Sniper,
			SectionEnumerations.DesignAttribute.Dead_Eye
		};
		public static List<SectionEnumerations.DesignAttribute> BadDesignAttributes = new List<SectionEnumerations.DesignAttribute>
		{
			SectionEnumerations.DesignAttribute.Bit_Of_A_Hog,
			SectionEnumerations.DesignAttribute.Muscle_Machine,
			SectionEnumerations.DesignAttribute.Hard_Luck_Ship,
			SectionEnumerations.DesignAttribute.Aces_And_Eights,
			SectionEnumerations.DesignAttribute.Ghost_Of_The_Hood,
			SectionEnumerations.DesignAttribute.Ol_Yellow_Streak,
			SectionEnumerations.DesignAttribute.Four_Eyes,
			SectionEnumerations.DesignAttribute.Death_Trap
		};
		public static readonly string[] TurretSizes = WeaponEnums._weaponSizes;
		public static Dictionary<string, StationType> StationTypesWithInvalid = new Dictionary<string, StationType>
		{

			{
				"",
				StationType.INVALID_TYPE
			},

			{
				"Naval",
				StationType.NAVAL
			},

			{
				"Civilian",
				StationType.CIVILIAN
			},

			{
				"Science",
				StationType.SCIENCE
			},

			{
				"Diplomatic",
				StationType.DIPLOMATIC
			},

			{
				"Gate",
				StationType.GATE
			},

			{
				"Mining",
				StationType.MINING
			}
		};
		public static string[] ExcludedSectionTypes = new string[]
		{
			ShipSectionType.Command.ToString(),
			ShipSectionType.Mission.ToString(),
			ShipSectionType.Engine.ToString()
		};
		public static Dictionary<string, string[]> StationStageMap = new Dictionary<string, string[]>
		{

			{
				"Civilian",
				new string[]
				{
					"way_station",
					"trading_post",
					"merchanter_station",
					"nexus",
					"star_city"
				}
			},

			{
				"Diplomatic",
				new string[]
				{
					"customs_station",
					"consulate",
					"embassy",
					"council_station",
					"star_chamber"
				}
			},

			{
				"Naval",
				new string[]
				{
					"outpost",
					"fwd_base",
					"naval_base",
					"star_base",
					"sector_base"
				}
			},

			{
				"Science",
				new string[]
				{
					"field_station",
					"star_lab",
					"research_base",
					"polytechnic_institute",
					"science_center"
				}
			},

			{
				"Gate",
				new string[]
				{
					""
				}
			}
		};
	}
}
