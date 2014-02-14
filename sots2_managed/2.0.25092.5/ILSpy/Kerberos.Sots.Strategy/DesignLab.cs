using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Ships;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Kerberos.Sots.Strategy
{
	internal class DesignLab
	{
		private enum DefenseStrat
		{
			Energy,
			Ballistics,
			HeavyBeam,
			Point
		}
		public class ModuleSlotInfo
		{
			public LogicalModuleMount mountInfo;
			public DesignModuleInfo currentModule;
		}
		private struct WeaponScore
		{
			public LogicalWeapon Weapon;
			public float Score;
		}
		internal enum NameGenerators
		{
			FactionRandom,
			MissionSectionDerived
		}
		private static ShipPreference[] _preferences;
		private static readonly ShipClass[] _designShipClassFallback;
		private static readonly Dictionary<ShipRole, WeaponRole> DefaultShipWeaponRoles;
		public static Rectangle DEFAULT_CRUISER_SIZE;
		public static Rectangle DEFAULT_DREADNAUGHT_SIZE;
		public static Rectangle DEFAULT_LEVIATHAN_SIZE;
		private static void Warn(string message)
		{
			App.Log.Warn(message, "design");
		}
		private static void Trace(string message)
		{
			App.Log.Trace(message, "design");
		}
		private static void TraceVerbose(string message)
		{
			App.Log.Trace(message, "design", LogLevel.Verbose);
		}
		public static void Init(GameSession game)
		{
			DesignLab.TraceVerbose("  First-time call: loading section preferences...");
			DesignLab._preferences = DesignLab.LoadSectionPreferences(game);
		}
		private static void InitDefaultShipWeaponRoles()
		{
			ShipRole[] array = (ShipRole[])Enum.GetValues(typeof(ShipRole));
			for (int i = 0; i < array.Length; i++)
			{
				ShipRole key = array[i];
				switch (key)
				{
				case ShipRole.UNDEFINED:
					break;
				case ShipRole.COMBAT:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.BRAWLER;
					break;
				case ShipRole.CARRIER:
				case ShipRole.CARRIER_ASSAULT:
				case ShipRole.CARRIER_DRONE:
				case ShipRole.CARRIER_BIO:
				case ShipRole.CARRIER_BOARDING:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.COMMAND:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.COLONIZER:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.CONSTRUCTOR:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.SCOUT:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.SUPPLY:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.E_WARFARE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.DISABLING;
					break;
				case ShipRole.GATE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.BORE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.FREIGHTER:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.SCAVENGER:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.DRONE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.ASSAULTSHUTTLE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.SLAVEDISK:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.BOARDINGPOD:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.BIOMISSILE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.TRAPDRONE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.POLICE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.DISABLING;
					break;
				case ShipRole.PLATFORM:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.BRAWLER;
					break;
				case ShipRole.BR_PATROL:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.BR_SCOUT:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.BR_SPINAL:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.BRAWLER;
					break;
				case ShipRole.BR_ESCORT:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.POINT_DEFENSE;
					break;
				case ShipRole.BR_INTERCEPTOR:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.BRAWLER;
					break;
				case ShipRole.BR_TORPEDO:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.BATTLECRUISER:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.BRAWLER;
					break;
				case ShipRole.BATTLESHIP:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.BRAWLER;
					break;
				case ShipRole.ACCELERATOR_GATE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				case ShipRole.LOA_CUBE:
					DesignLab.DefaultShipWeaponRoles[key] = WeaponRole.STAND_OFF;
					break;
				default:
					throw new ArgumentOutOfRangeException("role");
				}
			}
		}
		static DesignLab()
		{
			DesignLab._designShipClassFallback = new ShipClass[]
			{
				ShipClass.Leviathan,
				ShipClass.Dreadnought,
				ShipClass.Cruiser,
				ShipClass.BattleRider
			};
			DesignLab.DefaultShipWeaponRoles = new Dictionary<ShipRole, WeaponRole>();
			DesignLab.DEFAULT_CRUISER_SIZE = new Rectangle(0f, 0f, 9f, 9f);
			DesignLab.DEFAULT_DREADNAUGHT_SIZE = new Rectangle(0f, 0f, 27f, 12f);
			DesignLab.DEFAULT_LEVIATHAN_SIZE = new Rectangle(0f, 0f, 81f, 27f);
			DesignLab.InitDefaultShipWeaponRoles();
		}
		public static DesignInfo SetDefaultDesign(GameSession game, ShipRole role, WeaponRole? weaponRole, int playerID, string optionalName, bool? startsPrototyped, AITechStyles optionalTechStyles, AIStance? optionalStance)
		{
			List<DesignInfo> list = game.GameDatabase.GetVisibleDesignInfosForPlayerAndRole(playerID, role, weaponRole).ToList<DesignInfo>();
			list.RemoveAll((DesignInfo x) => x.IsSuulka());
			if (!list.Any<DesignInfo>())
			{
				DesignLab.TraceVerbose(string.Format("  Creating default design {0},{1} for player {2}...", role, weaponRole.HasValue ? weaponRole.Value.ToString() : "(unspecified)", playerID));
				ShipClass[] designShipClassFallback = DesignLab._designShipClassFallback;
				for (int i = 0; i < designShipClassFallback.Length; i++)
				{
					ShipClass shipClass = designShipClassFallback[i];
					WeaponRole wpnRole;
					if (weaponRole.HasValue)
					{
						wpnRole = weaponRole.Value;
					}
					else
					{
						if (optionalStance.HasValue)
						{
							wpnRole = DesignLab.SuggestWeaponRoleForNewDesign(optionalStance.Value, role, shipClass);
						}
						else
						{
							if (DesignLab.DefaultShipWeaponRoles.ContainsKey(role))
							{
								wpnRole = DesignLab.DefaultShipWeaponRoles[role];
							}
							else
							{
								wpnRole = WeaponRole.STAND_OFF;
							}
						}
					}
					DesignInfo designInfo = DesignLab.DesignShip(game, shipClass, role, wpnRole, playerID);
					if (designInfo != null)
					{
						if (startsPrototyped.HasValue)
						{
							designInfo.isPrototyped = startsPrototyped.Value;
						}
						else
						{
							designInfo.isPrototyped = (shipClass == ShipClass.BattleRider || shipClass == ShipClass.Station);
						}
						designInfo.Name = optionalName;
						int designID = game.GameDatabase.InsertDesignByDesignInfo(designInfo);
						return game.GameDatabase.GetDesignInfo(designID);
					}
				}
			}
			return (
				from x in list
				orderby x.DesignDate descending
				select x).FirstOrDefault<DesignInfo>();
		}
		public static DesignInfo DesignShip(GameSession game, ShipClass shipClass, ShipRole role, WeaponRole wpnRole, int playerID)
		{
			return DesignLab.DesignShip(game, shipClass, role, wpnRole, playerID, null);
		}
		private static IEnumerable<int> SelectShipOptions(GameDatabase db, ShipSectionAsset section, int playerId, ShipRole role, WeaponRole wpnRole)
		{
			foreach (string[] current in section.ShipOptions)
			{
				for (int i = current.Length - 1; i >= 0; i--)
				{
					string text = current[i];
					if (role == ShipRole.SCOUT || !(text == "IND_Stealth_Armor"))
					{
						int techID = db.GetTechID(text);
						PlayerTechInfo playerTechInfo = db.GetPlayerTechInfo(playerId, techID);
						if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
						{
							yield return techID;
							break;
						}
					}
				}
			}
			yield break;
		}
		public static DesignInfo CreateInitialShipDesign(GameSession game, string name, IEnumerable<ShipSectionAsset> sections, int playerID, AITechStyles optionalAITechStyles)
		{
			DesignInfo designInfo = DesignLab.DesignShipCore(game, sections, null, null, null, playerID, optionalAITechStyles);
			designInfo.Name = name;
			designInfo.isPrototyped = true;
			return designInfo;
		}
		public static DesignInfo DesignShip(GameSession game, ShipClass shipClass, ShipRole role, WeaponRole wpnRole, int playerID, AITechStyles optionalAITechStyles)
		{
			return DesignLab.DesignShipCore(game, new ShipSectionAsset[0], new ShipClass?(shipClass), new ShipRole?(role), new WeaponRole?(wpnRole), playerID, optionalAITechStyles);
		}
		private static DesignInfo DesignShipCore(GameSession game, IEnumerable<ShipSectionAsset> explicitSections, ShipClass? nshipClass, ShipRole? nrole, WeaponRole? nwpnRole, int playerID, AITechStyles optionalAITechStyles)
		{
			DesignLab.TraceVerbose(string.Format("Creating a new design for player {0} to satisfy {1}, {2}, {3}...", new object[]
			{
				playerID,
				nshipClass.HasValue ? nshipClass.Value.ToString() : "(null)",
				nrole.HasValue ? nrole.Value.ToString() : "(null)",
				nwpnRole.HasValue ? nwpnRole.Value.ToString() : "(null)"
			}));
			if (DesignLab._preferences == null)
			{
				DesignLab.Init(game);
			}
			List<ShipSectionAsset> list = new List<ShipSectionAsset>();
			ShipSectionAsset shipSectionAsset = explicitSections.FirstOrDefault((ShipSectionAsset x) => x.Type == ShipSectionType.Mission);
			ShipClass shipClass;
			ShipRole shipRole;
			WeaponRole weaponRole;
			if (shipSectionAsset == null)
			{
				if (!nshipClass.HasValue || !nrole.HasValue || !nwpnRole.HasValue)
				{
					throw new ArgumentException("If there is no explicit mission section then nshipClass, nrole and nwpnRole must all have values.");
				}
				shipClass = nshipClass.Value;
				shipRole = nrole.Value;
				weaponRole = nwpnRole.Value;
				shipSectionAsset = DesignLab.ChooseMissionSection(game, shipClass, shipRole, weaponRole, playerID);
			}
			else
			{
				if (nshipClass.HasValue || nrole.HasValue || nwpnRole.HasValue)
				{
					throw new ArgumentException("If there is an explicit mission section then none of nshipClass, nrole nor nwpnRole can have values.");
				}
				shipClass = shipSectionAsset.Class;
				shipRole = DesignLab.GetRole(shipSectionAsset);
				if (DesignLab.DefaultShipWeaponRoles.ContainsKey(shipRole))
				{
					weaponRole = DesignLab.DefaultShipWeaponRoles[shipRole];
				}
				else
				{
					weaponRole = WeaponRole.STAND_OFF;
				}
			}
			if (shipSectionAsset == null)
			{
				DesignLab.TraceVerbose("  Failed: No mission section available. It is possible that the player has not met prerequisites yet.");
				return null;
			}
			DesignLab.TraceVerbose(string.Format("  Mission: {0}", shipSectionAsset.FileName));
			list.Add(shipSectionAsset);
			ShipSectionAsset shipSectionAsset2 = null;
			if (!shipSectionAsset.ExcludeSectionTypes.Contains(ShipSectionType.Engine))
			{
				shipSectionAsset2 = explicitSections.FirstOrDefault((ShipSectionAsset x) => x.Type == ShipSectionType.Engine);
				if (shipSectionAsset2 == null)
				{
					shipSectionAsset2 = DesignLab.ChooseDriveSection(game, shipClass, playerID, list);
				}
				if (shipSectionAsset2 != null)
				{
					list.Add(shipSectionAsset2);
					DesignLab.TraceVerbose(string.Format("  Engine: {0}", shipSectionAsset2.FileName));
				}
				else
				{
					DesignLab.TraceVerbose("  Engine: n/a");
				}
			}
			else
			{
				DesignLab.TraceVerbose("  Engine: n/a");
			}
			if (!shipSectionAsset.ExcludeSectionTypes.Contains(ShipSectionType.Command) && shipSectionAsset2 != null && !shipSectionAsset2.ExcludeSectionTypes.Contains(ShipSectionType.Command))
			{
				ShipSectionAsset shipSectionAsset3 = explicitSections.FirstOrDefault((ShipSectionAsset x) => x.Type == ShipSectionType.Command);
				if (shipSectionAsset3 == null)
				{
					shipSectionAsset3 = DesignLab.ChooseCommandSection(game, shipClass, shipSectionAsset.RealClass, shipRole, weaponRole, playerID, list);
				}
				if (shipSectionAsset3 != null)
				{
					list.Add(shipSectionAsset3);
					DesignLab.TraceVerbose(string.Format("  Command: {0}", shipSectionAsset3.FileName));
				}
				else
				{
					DesignLab.TraceVerbose("  Command: n/a");
				}
			}
			else
			{
				DesignLab.TraceVerbose("  Command: n/a");
			}
			DesignInfo designInfo = new DesignInfo();
			List<DesignSectionInfo> list2 = new List<DesignSectionInfo>();
			List<LogicalWeapon> availableWeapons = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, playerID).ToList<LogicalWeapon>();
			List<LogicalPsionic> remainingPsionics = (
				from x in game.AssetDatabase.Psionics
				where x.IsAvailable(game.GameDatabase, playerID, false)
				select x).ToList<LogicalPsionic>();
			foreach (ShipSectionAsset current in list)
			{
				DesignLab.TraceVerbose(string.Format("  Designing details for {0}...", current.FileName));
				DesignSectionInfo designSectionInfo = new DesignSectionInfo
				{
					DesignInfo = designInfo
				};
				designSectionInfo.FilePath = current.FileName;
				designSectionInfo.ShipSectionAsset = current;
				designSectionInfo.Modules = DesignLab.ChooseModules(game, availableWeapons, shipClass, shipRole, weaponRole, current, playerID, optionalAITechStyles, remainingPsionics);
				designSectionInfo.WeaponBanks = DesignLab.ChooseWeapons(game, availableWeapons, shipRole, weaponRole, current, playerID, optionalAITechStyles);
				designSectionInfo.Techs = new List<int>();
				designSectionInfo.Techs.AddRange(DesignLab.SelectShipOptions(game.GameDatabase, current, playerID, shipRole, weaponRole));
				list2.Add(designSectionInfo);
			}
			designInfo.Name = null;
			designInfo.PlayerID = playerID;
			designInfo.DesignSections = list2.ToArray();
			designInfo.Role = shipRole;
			designInfo.WeaponRole = weaponRole;
			if (designInfo.Role == ShipRole.ASSAULTSHUTTLE || designInfo.Role == ShipRole.DRONE)
			{
				designInfo.isPrototyped = true;
			}
			else
			{
				DesignLab.TraceVerbose("  Requires prototyping.");
			}
			DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, designInfo);
			return designInfo;
		}
		public static DesignInfo CreateCounterDesign(GameSession game, ShipClass shipClass, int playerId, StrategicAI.DesignConfigurationInfo enemyInfo)
		{
			Dictionary<DesignLab.DefenseStrat, float> dictionary = new Dictionary<DesignLab.DefenseStrat, float>();
			Dictionary<WeaponRole, float> dictionary2 = new Dictionary<WeaponRole, float>();
			Dictionary<ShipSectionType, ShipSectionAsset> shipSections = new Dictionary<ShipSectionType, ShipSectionAsset>();
			Random random = new Random();
			dictionary[DesignLab.DefenseStrat.Energy] = enemyInfo.EnergyWeapons;
			dictionary[DesignLab.DefenseStrat.Ballistics] = enemyInfo.BallisticsWeapons;
			dictionary[DesignLab.DefenseStrat.HeavyBeam] = enemyInfo.HeavyBeamWeapons;
			dictionary[DesignLab.DefenseStrat.Point] = enemyInfo.MissileWeapons;
			dictionary2[WeaponRole.ENERGY] = enemyInfo.EnergyDefense;
			dictionary2[WeaponRole.BALLISTICS] = enemyInfo.BallisticsDefense;
			dictionary2[WeaponRole.STAND_OFF] = enemyInfo.PointDefense + enemyInfo.BallisticsDefense;
			IEnumerable<Weighted<DesignLab.DefenseStrat>> weights = 
				from x in dictionary
				select new Weighted<DesignLab.DefenseStrat>(x.Key, (int)x.Value);
			DesignLab.DefenseStrat defenseStrat = WeightedChoices.Choose<DesignLab.DefenseStrat>(random, weights);
			IEnumerable<ShipSectionAsset> availableShipSections = game.GetAvailableShipSections(playerId, ShipSectionType.Command, shipClass);
			game.GetAvailableShipSections(playerId, ShipSectionType.Mission, shipClass);
			if (!shipSections.ContainsKey(ShipSectionType.Mission) || shipSections[ShipSectionType.Mission] == null)
			{
				ShipRole role = (shipClass == ShipClass.Leviathan) ? ShipRole.COMMAND : ShipRole.COMBAT;
				shipSections[ShipSectionType.Mission] = DesignLab.ChooseMissionSection(game, shipClass, role, WeaponRole.BRAWLER, playerId);
			}
			switch (defenseStrat)
			{
			case DesignLab.DefenseStrat.Energy:
				shipSections[ShipSectionType.Command] = availableShipSections.FirstOrDefault(delegate(ShipSectionAsset x)
				{
					if (DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], x))
					{
						return x.ShipOptions.Any((string[] y) => y.Contains("SLD_Meson_Shields"));
					}
					return false;
				});
				if (shipSections[ShipSectionType.Command] == null)
				{
					shipSections[ShipSectionType.Command] = availableShipSections.FirstOrDefault((ShipSectionAsset x) => DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], x) && x.RequiredTechs.Contains("SLD_Disruptor_Shields"));
					if (shipSections[ShipSectionType.Command] == null)
					{
						shipSections[ShipSectionType.Command] = availableShipSections.FirstOrDefault((ShipSectionAsset x) => DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], x) && x.RequiredTechs.Contains("NRG_Energy_Absorbers"));
						if (shipSections[ShipSectionType.Command] != null)
						{
						}
					}
				}
				break;
			case DesignLab.DefenseStrat.Ballistics:
				shipSections[ShipSectionType.Command] = availableShipSections.FirstOrDefault((ShipSectionAsset x) => DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], x) && x.RequiredTechs.Contains("SLD_Deflector_Shields"));
				if (shipSections[ShipSectionType.Command] != null)
				{
				}
				break;
			case DesignLab.DefenseStrat.HeavyBeam:
			{
				Dictionary<WeaponRole, float> dictionary3;
				(dictionary3 = dictionary2)[WeaponRole.BALLISTICS] = dictionary3[WeaponRole.BALLISTICS] + 50f;
				break;
			}
			}
			if (!shipSections.ContainsKey(ShipSectionType.Command) || shipSections[ShipSectionType.Command] == null)
			{
				shipSections[ShipSectionType.Command] = DesignLab.ChooseCommandSection(game, shipClass, shipSections[ShipSectionType.Mission].RealClass, ShipRole.COMBAT, WeaponRole.BRAWLER, playerId, null);
			}
			WeaponRole weaponRole = WeaponRole.BRAWLER;
			if (defenseStrat == DesignLab.DefenseStrat.Point && random.CoinToss(0.75))
			{
				weaponRole = WeaponRole.POINT_DEFENSE;
			}
			else
			{
				float num = dictionary2.Max((KeyValuePair<WeaponRole, float> x) => x.Value) * 1.05f;
				if (num != 0f)
				{
					Dictionary<WeaponRole, float> dictionary4 = new Dictionary<WeaponRole, float>(dictionary2);
					foreach (KeyValuePair<WeaponRole, float> current in dictionary4)
					{
						dictionary2[current.Key] = (1f - current.Value / num) * 100f;
					}
				}
				IEnumerable<Weighted<WeaponRole>> weights2 = 
					from x in dictionary2
					select new Weighted<WeaponRole>(x.Key, (int)x.Value);
				weaponRole = WeightedChoices.Choose<WeaponRole>(random, weights2);
			}
			DesignInfo designInfo = new DesignInfo();
			List<DesignSectionInfo> list = new List<DesignSectionInfo>();
			List<LogicalWeapon> availableWeapons = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, playerId).ToList<LogicalWeapon>();
			(
				from x in game.AssetDatabase.Psionics
				where x.IsAvailable(game.GameDatabase, playerId, false)
				select x).ToList<LogicalPsionic>();
			if (shipSections[ShipSectionType.Mission] != null && shipSections[ShipSectionType.Command] != null && !DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], shipSections[ShipSectionType.Command]))
			{
				shipSections[ShipSectionType.Command] = null;
			}
			shipSections[ShipSectionType.Engine] = DesignLab.ChooseDriveSection(game, shipClass, playerId, (
				from x in shipSections.Values
				where x != null && x.Type == ShipSectionType.Mission
				select x).ToList<ShipSectionAsset>());
			foreach (KeyValuePair<ShipSectionType, ShipSectionAsset> current2 in shipSections)
			{
				if (current2.Value != null)
				{
					ShipSectionAsset value = current2.Value;
					DesignSectionInfo designSectionInfo = new DesignSectionInfo
					{
						DesignInfo = designInfo
					};
					designSectionInfo.FilePath = value.FileName;
					designSectionInfo.ShipSectionAsset = value;
					designSectionInfo.Modules = DesignLab.ChooseModules(game, availableWeapons, shipClass, ShipRole.COMBAT, weaponRole, value, playerId, null, null);
					designSectionInfo.WeaponBanks = DesignLab.ChooseWeapons(game, availableWeapons, ShipRole.COMBAT, weaponRole, value, playerId, null);
					designSectionInfo.Techs = new List<int>();
					designSectionInfo.Techs.AddRange(DesignLab.SelectShipOptions(game.GameDatabase, value, playerId, ShipRole.COMBAT, weaponRole));
					if (current2.Key == ShipSectionType.Command && defenseStrat == DesignLab.DefenseStrat.Energy)
					{
						if (value.ShipOptions.Any((string[] y) => y.Contains("SLD_Meson_Shields")))
						{
							int techID = game.GameDatabase.GetTechID("SLD_Meson_Shields");
							PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfo(playerId, techID);
							if (!designSectionInfo.Techs.Contains(techID) && playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
							{
								designSectionInfo.Techs.Add(techID);
							}
						}
					}
					list.Add(designSectionInfo);
				}
			}
			designInfo.Name = null;
			designInfo.PlayerID = playerId;
			designInfo.DesignSections = list.ToArray();
			designInfo.Role = ShipRole.COMBAT;
			designInfo.WeaponRole = weaponRole;
			DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, designInfo);
			return designInfo;
		}
		public static ShipSectionAsset ChooseDriveSection(GameSession game, ShipClass shipClass, int playerID, List<ShipSectionAsset> sections)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(playerID);
			string factionName = game.GameDatabase.GetFactionName(playerInfo.FactionID);
			ShipSectionAsset shipSectionAsset = null;
			float num = 0f;
			IEnumerable<ShipSectionAsset> availableShipSections = game.GetAvailableShipSections(playerID, ShipSectionType.Engine, shipClass);
			foreach (ShipSectionAsset current in availableShipSections)
			{
				bool flag = true;
				if (sections != null)
				{
					foreach (ShipSectionAsset current2 in sections)
					{
						if (!DesignLab.AreSectionsCompatible(current, current2))
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					float num2 = current.FtlSpeed;
					if (factionName == "human" || factionName == "zuul")
					{
						num2 = current.NodeSpeed;
					}
					if (shipSectionAsset == null || num2 > num)
					{
						shipSectionAsset = current;
						num = num2;
					}
				}
			}
			return shipSectionAsset;
		}
		public static ShipRole GetRole(ShipSectionAsset missionsection)
		{
			if (missionsection.StationType != StationType.INVALID_TYPE)
			{
				return ShipRole.UNDEFINED;
			}
			if (missionsection.CommandPoints > 0)
			{
				return ShipRole.COMMAND;
			}
			if (missionsection.ColonizationSpace > 0)
			{
				return ShipRole.COLONIZER;
			}
			if (missionsection.CombatAIType == SectionEnumerations.CombatAiType.TrapDrone)
			{
				return ShipRole.TRAPDRONE;
			}
			if (missionsection.isConstructor)
			{
				return ShipRole.CONSTRUCTOR;
			}
			if (missionsection.IsSupplyShip)
			{
				return ShipRole.SUPPLY;
			}
			if (missionsection.IsBoreShip)
			{
				return ShipRole.BORE;
			}
			if (missionsection.IsGateShip)
			{
				return ShipRole.GATE;
			}
			if (missionsection.IsAccelerator)
			{
				return ShipRole.ACCELERATOR_GATE;
			}
			if (missionsection.IsLoaCube)
			{
				return ShipRole.LOA_CUBE;
			}
			if (missionsection.RealClass == RealShipClasses.Platform)
			{
				return ShipRole.PLATFORM;
			}
			if (missionsection.IsScavenger)
			{
				return ShipRole.SCAVENGER;
			}
			if (missionsection.isPolice)
			{
				return ShipRole.POLICE;
			}
			if (missionsection.IsFreighter)
			{
				return ShipRole.FREIGHTER;
			}
			if (missionsection.isDeepScan)
			{
				return ShipRole.SCOUT;
			}
			if (missionsection.RealClass == RealShipClasses.Drone)
			{
				return ShipRole.DRONE;
			}
			if (missionsection.RealClass == RealShipClasses.AssaultShuttle && missionsection.SlaveCapacity > 0)
			{
				return ShipRole.SLAVEDISK;
			}
			if (missionsection.RealClass == RealShipClasses.AssaultShuttle)
			{
				return ShipRole.ASSAULTSHUTTLE;
			}
			if (missionsection.RealClass == RealShipClasses.BoardingPod)
			{
				return ShipRole.BOARDINGPOD;
			}
			if (missionsection.RealClass == RealShipClasses.Biomissile)
			{
				return ShipRole.BIOMISSILE;
			}
			if (missionsection.RealClass == RealShipClasses.BattleRider)
			{
				switch (missionsection.BattleRiderType)
				{
				case BattleRiderTypes.patrol:
					return ShipRole.BR_PATROL;
				case BattleRiderTypes.scout:
					return ShipRole.BR_SCOUT;
				case BattleRiderTypes.spinal:
					return ShipRole.BR_SPINAL;
				case BattleRiderTypes.escort:
					return ShipRole.BR_ESCORT;
				case BattleRiderTypes.interceptor:
					return ShipRole.BR_INTERCEPTOR;
				case BattleRiderTypes.torpedo:
					return ShipRole.BR_TORPEDO;
				}
				return ShipRole.COMBAT;
			}
			if (missionsection.RealClass == RealShipClasses.BattleCruiser)
			{
				return ShipRole.BATTLECRUISER;
			}
			if (missionsection.RealClass == RealShipClasses.BattleShip)
			{
				return ShipRole.BATTLESHIP;
			}
			if (!missionsection.IsCarrier)
			{
				return ShipRole.COMBAT;
			}
			switch (missionsection.CarrierType)
			{
			case CarrierType.Drone:
				return ShipRole.CARRIER_DRONE;
			case CarrierType.AssaultShuttle:
				return ShipRole.CARRIER_ASSAULT;
			case CarrierType.BioMissile:
				return ShipRole.CARRIER_BIO;
			case CarrierType.BoardingPod:
				return ShipRole.CARRIER_BOARDING;
			default:
				return ShipRole.CARRIER;
			}
		}
		private static ShipSectionAsset ChooseMissionSection(GameSession sim, ShipClass shipClass, ShipRole role, WeaponRole wpnRole, int playerID)
		{
			ShipSectionAsset shipSectionAsset = null;
			RealShipClasses realClass = DesignLab.GetRealShipClassFromShipClassAndRole(shipClass, role);
			switch (role)
			{
			case ShipRole.COMBAT:
				shipSectionAsset = DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Mission, wpnRole, playerID, new ShipRole?(role));
				return shipSectionAsset;
			case ShipRole.CARRIER:
			case ShipRole.CARRIER_ASSAULT:
			case ShipRole.CARRIER_DRONE:
			case ShipRole.CARRIER_BIO:
			case ShipRole.CARRIER_BOARDING:
			{
				ShipClass shipClass2 = ShipClass.BattleRider;
				int num = 0;
				using (IEnumerator<ShipSectionAsset> enumerator = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == realClass
					select x).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ShipSectionAsset current = enumerator.Current;
						if (DesignLab.GetRole(current) == role && current.CarrierTypeMatchesRole(role) && !current.IsSuulka)
						{
							int num2 = current.Banks.Count((LogicalBank x) => WeaponEnums.IsBattleRider(x.TurretClass));
							if ((current.Class == shipClass2 && num2 > num) || Ship.IsShipClassBigger(current.Class, shipClass2, false))
							{
								shipClass2 = current.Class;
								num = num2;
								shipSectionAsset = current;
							}
						}
					}
					return shipSectionAsset;
				}
				break;
			}
			case ShipRole.COMMAND:
				break;
			case ShipRole.COLONIZER:
				goto IL_252;
			case ShipRole.CONSTRUCTOR:
				goto IL_2D5;
			case ShipRole.SCOUT:
				goto IL_34A;
			case ShipRole.SUPPLY:
			{
				int num3 = 0;
				using (IEnumerator<ShipSectionAsset> enumerator2 = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == realClass
					select x).GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ShipSectionAsset current2 = enumerator2.Current;
						if (DesignLab.GetRole(current2) == role && current2.Supply > num3 && !current2.IsSuulka)
						{
							shipSectionAsset = current2;
							num3 = current2.Supply;
						}
					}
					return shipSectionAsset;
				}
				goto IL_478;
			}
			case ShipRole.E_WARFARE:
				goto IL_478;
			case ShipRole.GATE:
				shipSectionAsset = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault((ShipSectionAsset x) => x.IsGateShip && !x.IsSuulka);
				return shipSectionAsset;
			case ShipRole.BORE:
				shipSectionAsset = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault((ShipSectionAsset x) => x.IsBoreShip && !x.IsSuulka);
				return shipSectionAsset;
			case ShipRole.FREIGHTER:
			{
				List<ShipSectionAsset> list = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.IsFreighter && !x.IsSuulka
					select x).ToList<ShipSectionAsset>();
				if (list.Count != 0)
				{
					shipSectionAsset = (
						from x in list
						orderby x.FreighterSpace
						select x).First<ShipSectionAsset>();
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.SCAVENGER:
			{
				List<ShipSectionAsset> list2 = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.IsScavenger && !x.IsSuulka
					select x).ToList<ShipSectionAsset>();
				if (list2.Count != 0)
				{
					shipSectionAsset = list2.First<ShipSectionAsset>();
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.DRONE:
			{
				List<ShipSectionAsset> list3 = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == RealShipClasses.Drone && !x.IsSuulka
					select x).ToList<ShipSectionAsset>();
				if (list3.Count != 0)
				{
					shipSectionAsset = (
						from x in list3
						orderby x.ExcludeSectionTypes.Length descending
						select x).First<ShipSectionAsset>();
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.ASSAULTSHUTTLE:
			{
				List<ShipSectionAsset> list4 = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == RealShipClasses.AssaultShuttle && !x.IsSuulka
					select x).ToList<ShipSectionAsset>();
				if (list4.Count != 0)
				{
					shipSectionAsset = (
						from x in list4
						orderby x.ExcludeSectionTypes.Length descending
						select x).First<ShipSectionAsset>();
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.SLAVEDISK:
			{
				List<ShipSectionAsset> list5 = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == RealShipClasses.AssaultShuttle && x.SlaveCapacity > 0 && !x.IsSuulka
					select x).ToList<ShipSectionAsset>();
				if (list5.Count != 0)
				{
					shipSectionAsset = list5.First<ShipSectionAsset>();
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.BOARDINGPOD:
			{
				List<ShipSectionAsset> list6 = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == RealShipClasses.BoardingPod && !x.IsSuulka
					select x).ToList<ShipSectionAsset>();
				if (list6.Count != 0)
				{
					shipSectionAsset = (
						from x in list6
						orderby x.ExcludeSectionTypes.Length descending
						select x).First<ShipSectionAsset>();
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.BIOMISSILE:
			{
				List<ShipSectionAsset> list7 = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == RealShipClasses.Biomissile && !x.IsSuulka
					select x).ToList<ShipSectionAsset>();
				if (list7.Count != 0)
				{
					shipSectionAsset = (
						from x in list7
						orderby x.ExcludeSectionTypes.Length descending
						select x).First<ShipSectionAsset>();
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.TRAPDRONE:
			{
				List<ShipSectionAsset> list8 = (
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.CombatAIType == SectionEnumerations.CombatAiType.TrapDrone && !x.IsSuulka
					select x).ToList<ShipSectionAsset>();
				if (list8.Count != 0)
				{
					shipSectionAsset = (
						from x in list8
						orderby x.ExcludeSectionTypes.Length descending
						select x).First<ShipSectionAsset>();
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.POLICE:
			{
				IEnumerable<ShipSectionAsset> source = 
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.isPolice && !x.IsSuulka
					select x;
				shipSectionAsset = source.FirstOrDefault<ShipSectionAsset>();
				return shipSectionAsset;
			}
			case ShipRole.PLATFORM:
			{
				IEnumerable<ShipSectionAsset> enumerable = 
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == RealShipClasses.Platform && !x.IsSuulka
					select x;
				if (enumerable.Any<ShipSectionAsset>())
				{
					shipSectionAsset = sim.Random.Choose(enumerable);
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.BR_PATROL:
			case ShipRole.BR_SCOUT:
			case ShipRole.BR_SPINAL:
			case ShipRole.BR_ESCORT:
			case ShipRole.BR_INTERCEPTOR:
			case ShipRole.BR_TORPEDO:
			{
				IEnumerable<ShipSectionAsset> enumerable2 = 
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.RealClass == realClass && DesignLab.GetRole(x) == role && !x.IsSuulka
					select x;
				if (enumerable2.Any<ShipSectionAsset>())
				{
					shipSectionAsset = sim.Random.Choose(enumerable2);
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.BATTLECRUISER:
			{
				if (shipClass != ShipClass.Cruiser)
				{
					return shipSectionAsset;
				}
				IEnumerable<ShipSectionAsset> enumerable3 = 
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.BattleRiderType == BattleRiderTypes.battlerider && !x.IsSuulka
					select x;
				if (enumerable3.Any<ShipSectionAsset>())
				{
					shipSectionAsset = sim.Random.Choose(enumerable3);
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.BATTLESHIP:
			{
				if (shipClass != ShipClass.Dreadnought)
				{
					return shipSectionAsset;
				}
				IEnumerable<ShipSectionAsset> enumerable4 = 
					from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
					where x.BattleRiderType == BattleRiderTypes.battlerider && !x.IsSuulka
					select x;
				if (enumerable4.Any<ShipSectionAsset>())
				{
					shipSectionAsset = sim.Random.Choose(enumerable4);
					return shipSectionAsset;
				}
				return shipSectionAsset;
			}
			case ShipRole.ACCELERATOR_GATE:
				shipSectionAsset = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault((ShipSectionAsset x) => x.IsAccelerator);
				return shipSectionAsset;
			case ShipRole.LOA_CUBE:
				shipSectionAsset = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault((ShipSectionAsset x) => x.IsLoaCube);
				return shipSectionAsset;
			default:
				throw new ArgumentOutOfRangeException("role");
			}
			int num4 = 0;
			using (IEnumerator<ShipSectionAsset> enumerator3 = (
				from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
				where x.RealClass == realClass
				select x).GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					ShipSectionAsset current3 = enumerator3.Current;
					if (DesignLab.GetRole(current3) == role && current3.CommandPoints > num4 && !current3.IsSuulka)
					{
						shipSectionAsset = current3;
						num4 = current3.CommandPoints;
					}
				}
				return shipSectionAsset;
			}
			IL_252:
			int num5 = 0;
			using (IEnumerator<ShipSectionAsset> enumerator4 = (
				from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
				where x.RealClass == realClass
				select x).GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					ShipSectionAsset current4 = enumerator4.Current;
					if (DesignLab.GetRole(current4) == role && current4.ColonizationSpace > num5 && !current4.IsSuulka)
					{
						shipSectionAsset = current4;
						num5 = current4.ColonizationSpace;
					}
				}
				return shipSectionAsset;
			}
			IL_2D5:
			using (IEnumerator<ShipSectionAsset> enumerator5 = (
				from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
				where x.RealClass == realClass
				select x).GetEnumerator())
			{
				while (enumerator5.MoveNext())
				{
					ShipSectionAsset current5 = enumerator5.Current;
					if (DesignLab.GetRole(current5) == role && current5.isConstructor && !current5.IsSuulka)
					{
						shipSectionAsset = current5;
					}
				}
				return shipSectionAsset;
			}
			IL_34A:
			int num6 = 0;
			foreach (ShipSectionAsset current6 in 
				from x in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass)
				where x.RealClass == realClass
				select x)
			{
				if (DesignLab.GetRole(current6) == role && current6.TacticalSensorRange > (float)num6 && !current6.IsSuulka)
				{
					shipSectionAsset = current6;
					num6 = current6.Supply;
				}
			}
			if (shipSectionAsset == null)
			{
				shipSectionAsset = DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Mission, wpnRole, playerID, new ShipRole?(role));
				return shipSectionAsset;
			}
			return shipSectionAsset;
			IL_478:
			shipSectionAsset = DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Mission, wpnRole, playerID, new ShipRole?(role));
			return shipSectionAsset;
		}
        private static ShipSectionAsset ChooseSectionForCombat(GameSession sim, ShipClass shipClass, RealShipClasses realClass, ShipSectionType sectionType, WeaponRole wpnRole, int playerID, ShipRole? role = new ShipRole?())
        {
            List<ShipSectionAsset> list = (from x in sim.GetAvailableShipSections(playerID, sectionType, shipClass)
                                           where !x.IsSuulka && (x.RealClass == realClass)
                                           select x).ToList<ShipSectionAsset>();
            if ((sectionType == ShipSectionType.Mission) && role.HasValue)
            {
                list.RemoveAll(x => GetRole(x) != role.Value);
            }
            if (list.Count == 0)
            {
                return null;
            }
            List<ShipPreference> list2 = new List<ShipPreference>();
            int factionID = sim.GameDatabase.GetPlayerInfo(playerID).FactionID;
            float num2 = 0f;
            Func<ShipSectionAsset, bool> predicate = null;
            foreach (ShipPreference shipPref in _preferences)
            {
                if ((shipPref.factionID == factionID) && (shipPref.preferenceWeight > 0f))
                {
                    if (predicate == null)
                    {
                        predicate = x => x.FileName.ToLower() == shipPref.sectionName.ToLower();
                    }
                    ShipSectionAsset item = sim.AssetDatabase.ShipSections.First<ShipSectionAsset>(predicate);
                    if (((item != null) && (item.Type == sectionType)) && list.Contains(item))
                    {
                        list2.Add(shipPref);
                        num2 += shipPref.preferenceWeight;
                    }
                }
            }
            if (list2.Count == 0)
            {
                return list[App.GetSafeRandom().Next(list.Count)];
            }
            float num3 = ((float)App.GetSafeRandom().NextDouble()) * num2;
            using (List<ShipPreference>.Enumerator enumerator = list2.GetEnumerator())
            {
                Func<ShipSectionAsset, bool> func2 = null;
                ShipPreference shipPref;
                while (enumerator.MoveNext())
                {
                    shipPref = enumerator.Current;
                    if (num3 > shipPref.preferenceWeight)
                    {
                        num3 -= shipPref.preferenceWeight;
                    }
                    else
                    {
                        if (func2 == null)
                        {
                            func2 = x => x.FileName == shipPref.sectionName;
                        }
                        return sim.AssetDatabase.ShipSections.First<ShipSectionAsset>(func2);
                    }
                }
            }
            throw new NullReferenceException("The AI couldn't decided on a ship to use on it's new design. This is probably because there were no available sections for the specified type and class of ship... ");
        }
        private static ShipSectionAsset ChooseCommandSection(GameSession sim, ShipClass shipClass, RealShipClasses realClass, ShipRole role, WeaponRole wpnRole, int playerID, List<ShipSectionAsset> sections)
		{
			ShipSectionAsset shipSectionAsset = null;
			switch (role)
			{
			case ShipRole.COMBAT:
				break;
			case ShipRole.CARRIER:
			case ShipRole.COMMAND:
			case ShipRole.COLONIZER:
			case ShipRole.CONSTRUCTOR:
			case ShipRole.SUPPLY:
				goto IL_B2;
			case ShipRole.SCOUT:
			{
				int num = 0;
				using (IEnumerator<ShipSectionAsset> enumerator = sim.GetAvailableShipSections(playerID, ShipSectionType.Command, shipClass).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ShipSectionAsset current = enumerator.Current;
						if (current.isDeepScan && current.TacticalSensorRange > (float)num && !current.IsSuulka)
						{
							shipSectionAsset = current;
							num = current.Supply;
						}
					}
					goto IL_B2;
				}
				break;
			}
			default:
				switch (role)
				{
				case ShipRole.CARRIER_ASSAULT:
				case ShipRole.CARRIER_DRONE:
				case ShipRole.CARRIER_BIO:
				case ShipRole.CARRIER_BOARDING:
					goto IL_B2;
				default:
					goto IL_B2;
				}
				break;
			}
			shipSectionAsset = DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Command, wpnRole, playerID, null);
			IL_B2:
			if (shipSectionAsset == null)
			{
				shipSectionAsset = DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Command, wpnRole, playerID, null);
			}
			return shipSectionAsset;
		}
		private static List<DesignModuleInfo> ChooseModules(GameSession game, IList<LogicalWeapon> availableWeapons, ShipClass shipClass, ShipRole role, WeaponRole wpnRole, ShipSectionAsset sectionAsset, int playerID, AITechStyles optionalAITechStyles, List<LogicalPsionic> remainingPsionics)
		{
			List<DesignModuleInfo> list = new List<DesignModuleInfo>();
			if (sectionAsset.Modules.Length == 0)
			{
				DesignLab.TraceVerbose(string.Format("No modules required for {0}.", sectionAsset.FileName));
			}
			else
			{
				DesignLab.TraceVerbose(string.Format("Choosing modules to fit {0} ({1}, {2})...\n  Slots to fill: {3}", new object[]
				{
					sectionAsset.FileName,
					role,
					wpnRole,
					sectionAsset.Modules.Length
				}));
				List<LogicalModule> availableModulesForSection = DesignLab.GetAvailableModulesForSection(game, sectionAsset, playerID);
				if (availableModulesForSection.Count == 0)
				{
					DesignLab.TraceVerbose("  No modules available.");
				}
				else
				{
					DesignLab.TraceVerbose(string.Format("  Modules available: {0}", availableModulesForSection.Count));
					string name = game.GameDatabase.GetPlayerFaction(playerID).Name;
					for (int i = 0; i < sectionAsset.Modules.Length; i++)
					{
						LogicalModuleMount logicalModuleMount = sectionAsset.Modules[i];
						List<LogicalModule> list2 = (
							from x in LogicalModule.EnumerateModuleFits(availableModulesForSection, sectionAsset, i, false)
							where x.NumPsionicSlots <= 0 || remainingPsionics.Count > 0
							select x).ToList<LogicalModule>();
						if (list2.Count > 0)
						{
							int index = App.GetSafeRandom().Next(list2.Count);
							LogicalModule logicalModule = list2[index];
							DesignLab.TraceVerbose(string.Format("    {0}...", logicalModule.ModuleName));
							DesignModuleInfo designModuleInfo = new DesignModuleInfo();
							designModuleInfo.ModuleID = game.GameDatabase.GetModuleID(logicalModule.ModulePath, playerID);
							designModuleInfo.MountNodeName = logicalModuleMount.NodeName;
							LogicalBank logicalBank = logicalModule.Banks.FirstOrDefault<LogicalBank>();
							if (logicalBank != null)
							{
								WeaponBankInfo weaponBankInfo = DesignLab.AssignBestWeaponToBank(game, logicalBank, availableWeapons, role, wpnRole, sectionAsset, logicalBank.TurretClass, logicalBank.TurretSize, playerID, optionalAITechStyles, name);
								designModuleInfo.WeaponID = weaponBankInfo.WeaponID;
								if (WeaponEnums.IsWeaponBattleRider(logicalBank.TurretClass))
								{
									designModuleInfo.DesignID = DesignLab.ChooseBattleRider(game, DesignLab.GetWeaponRiderShipRole(logicalBank.TurretClass, sectionAsset.IsScavenger), wpnRole, playerID);
								}
							}
							if (logicalModule.NumPsionicSlots > 0)
							{
								int num = logicalModule.NumPsionicSlots;
								while (remainingPsionics.Count > 0 && num > 0)
								{
									int index2 = game.Random.Next(remainingPsionics.Count);
									designModuleInfo.PsionicAbilities.Add(new ModulePsionicInfo
									{
										Ability = remainingPsionics[index2].Ability
									});
									remainingPsionics.RemoveAt(index2);
									num--;
								}
							}
							list.Add(designModuleInfo);
						}
					}
				}
			}
			return list;
		}
		private static List<LogicalModule> GetAvailableModulesForSection(GameSession game, ShipSectionAsset sectionAsset, int playerID)
		{
			List<LogicalModule> list = new List<LogicalModule>();
			foreach (LogicalModule current in game.AssetDatabase.Modules)
			{
				if (current.Faction == sectionAsset.Faction && current.Class == sectionAsset.Class)
				{
					bool flag = true;
					foreach (Kerberos.Sots.Data.ShipFramework.Tech current2 in current.Techs)
					{
						if (!game.GameDatabase.PlayerHasTech(playerID, current2.Name))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list.Add(current);
					}
				}
			}
			return list;
		}
		public static List<DesignLab.ModuleSlotInfo> GetModuleSlotInfo(App game, StationInfo station, int playerID)
		{
			List<DesignLab.ModuleSlotInfo> list = new List<DesignLab.ModuleSlotInfo>();
			(
				from ds in station.DesignInfo.DesignSections
				select game.AssetDatabase.ShipSections.First((ShipSectionAsset sa) => sa.FileName == ds.FilePath)).ToList<ShipSectionAsset>();
			DesignSectionInfo designSection = station.DesignInfo.DesignSections[0];
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == designSection.FilePath);
			LogicalModuleMount[] modules = shipSectionAsset.Modules;
			for (int i = 0; i < modules.Length; i++)
			{
				LogicalModuleMount logicalModuleMount = modules[i];
				DesignLab.ModuleSlotInfo moduleSlotInfo = new DesignLab.ModuleSlotInfo();
				moduleSlotInfo.mountInfo = logicalModuleMount;
				foreach (DesignModuleInfo current in designSection.Modules)
				{
					if (current.MountNodeName == logicalModuleMount.NodeName)
					{
						moduleSlotInfo.currentModule = current;
						break;
					}
				}
				list.Add(moduleSlotInfo);
			}
			return list;
		}
		public static List<LogicalModule> GetAvailableModulesForSlot(App game, StationInfo station, int playerID, int slot)
		{
			List<LogicalModule> list = new List<LogicalModule>();
			(
				from ds in station.DesignInfo.DesignSections
				select game.AssetDatabase.ShipSections.First((ShipSectionAsset sa) => sa.FileName == ds.FilePath)).ToList<ShipSectionAsset>();
			DesignSectionInfo designSection = station.DesignInfo.DesignSections[0];
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == designSection.FilePath);
			LogicalModuleMount logicalModuleMount = shipSectionAsset.Modules[slot];
			foreach (LogicalModule current in game.AssetDatabase.Modules)
			{
				if (current.Faction == shipSectionAsset.Faction && current.Class == shipSectionAsset.Class && current.ModuleType == logicalModuleMount.ModuleType)
				{
					bool flag = true;
					foreach (Kerberos.Sots.Data.ShipFramework.Tech current2 in current.Techs)
					{
						if (!game.GameDatabase.PlayerHasTech(playerID, current2.Name))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list.Add(current);
					}
				}
			}
			return list;
		}
		public static void RemoveModuleFromSlot(App game, StationInfo station, int playerID, int slot)
		{
			(
				from ds in station.DesignInfo.DesignSections
				select game.AssetDatabase.ShipSections.First((ShipSectionAsset sa) => sa.FileName == ds.FilePath)).ToList<ShipSectionAsset>();
			DesignSectionInfo designSection = station.DesignInfo.DesignSections[0];
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == designSection.FilePath);
			LogicalModuleMount logicalModuleMount = shipSectionAsset.Modules[slot];
			foreach (DesignModuleInfo current in designSection.Modules)
			{
				if (current.MountNodeName == logicalModuleMount.NodeName)
				{
					game.GameDatabase.RemoveDesignModule(current);
					designSection.Modules.Remove(current);
					break;
				}
			}
		}
		public static DesignInfo AssignWeaponsToDesign(GameSession game, DesignInfo di, List<LogicalWeapon> availableWeapons, int playerID, WeaponRole wpRole, AITechStyles optionalAITechStyles)
		{
			DesignSectionInfo[] designSections = di.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				designSectionInfo.WeaponBanks = DesignLab.ChooseWeapons(game, availableWeapons, di.Role, wpRole, designSectionInfo.ShipSectionAsset, playerID, optionalAITechStyles);
			}
			return di;
		}
		public static List<WeaponBankInfo> ChooseWeapons(GameSession game, IList<LogicalWeapon> availableWeapons, ShipRole role, WeaponRole wpnRole, ShipSectionAsset sectionAsset, int playerID, AITechStyles optionalAITechStyles)
		{
			DesignLab.TraceVerbose(string.Format("Choosing weapons to fit {0} ({1}, {2})...", sectionAsset.FileName, role, wpnRole));
			List<WeaponBankInfo> list = new List<WeaponBankInfo>();
			string name = game.GameDatabase.GetPlayerFaction(playerID).Name;
			LogicalBank[] banks = sectionAsset.Banks;
			for (int i = 0; i < banks.Length; i++)
			{
				LogicalBank logicalBank = banks[i];
				if (WeaponEnums.IsWeaponBattleRider(logicalBank.TurretClass))
				{
					DesignLab.TraceVerbose("Bank is " + logicalBank.TurretClass + " (for a battle rider)...");
					list.Add(new WeaponBankInfo
					{
						BankGUID = logicalBank.GUID,
						WeaponID = null,
						DesignID = DesignLab.ChooseBattleRider(game, DesignLab.GetWeaponRiderShipRole(logicalBank.TurretClass, sectionAsset.IsScavenger), wpnRole, playerID)
					});
				}
				else
				{
					WeaponBankInfo item = DesignLab.AssignBestWeaponToBank(game, logicalBank, availableWeapons, role, wpnRole, sectionAsset, logicalBank.TurretClass, logicalBank.TurretSize, playerID, optionalAITechStyles, name);
					list.Add(item);
				}
			}
			return list;
		}
		public static ShipRole GetWeaponRiderShipRole(WeaponEnums.TurretClasses turretClass, bool isScavenger)
		{
			ShipRole result = ShipRole.DRONE;
			switch (turretClass)
			{
			case WeaponEnums.TurretClasses.Biomissile:
				result = ShipRole.BIOMISSILE;
				break;
			case WeaponEnums.TurretClasses.Drone:
				result = ShipRole.DRONE;
				break;
			case WeaponEnums.TurretClasses.AssaultShuttle:
				if (isScavenger)
				{
					result = ShipRole.SLAVEDISK;
				}
				else
				{
					result = ShipRole.ASSAULTSHUTTLE;
				}
				break;
			default:
				if (turretClass == WeaponEnums.TurretClasses.BoardingPod)
				{
					result = ShipRole.BOARDINGPOD;
				}
				break;
			}
			return result;
		}
		public static int? ChooseBattleRider(GameSession game, ShipRole role, WeaponRole wpnRole, int playerID)
		{
			int? result = null;
			int num = 0;
			List<DesignInfo> list = (
				from x in game.GameDatabase.GetDesignInfosForPlayer(playerID)
				where x.Class == ShipClass.BattleRider
				select x).ToList<DesignInfo>();
			if (list.Count == 0)
			{
				DesignInfo designInfo = DesignLab.DesignShip(game, ShipClass.BattleRider, role, wpnRole, playerID);
				if (designInfo != null && designInfo != null)
				{
					designInfo.ID = game.GameDatabase.InsertDesignByDesignInfo(designInfo);
					DesignLab.Trace(string.Concat(new object[]
					{
						"Player ",
						playerID,
						" designed a new ",
						designInfo.Role,
						" ",
						designInfo.Class,
						"."
					}));
				}
				list.Add(designInfo);
			}
			foreach (DesignInfo current in list)
			{
				if (current.Role == role && (!result.HasValue || current.DesignDate > num))
				{
					result = new int?(current.ID);
					num = current.DesignDate;
				}
			}
			return result;
		}
		private static WeaponBankInfo AssignBestWeaponToBank(GameSession game, LogicalBank bank, IList<LogicalWeapon> availableWeapons, ShipRole role, WeaponRole wpnRole, ShipSectionAsset section, WeaponEnums.TurretClasses turretClass, WeaponEnums.WeaponSizes turretSize, int playerID, AITechStyles optionalAITechStyles, string playerFaction)
		{
			List<LogicalWeapon> list = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, availableWeapons, turretSize, turretClass).ToList<LogicalWeapon>();
			if (App.Log.Level >= LogLevel.Verbose)
			{
				DesignLab.TraceVerbose(string.Format("Assigning best weapon fitting {0}, size={1}, turret class={2}:", section.Faction, turretSize, turretClass));
				if (list.Count == 0)
				{
					DesignLab.TraceVerbose("  None available");
				}
				else
				{
					DesignLab.TraceVerbose(string.Format("  Scoring for {0}:", wpnRole));
					foreach (LogicalWeapon current in list)
					{
						DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", current.ToString(), DesignLab.PickWeaponScore(game.AssetDatabase, current, wpnRole, optionalAITechStyles, turretClass, playerFaction)));
					}
				}
			}
			WeaponBankInfo weaponBankInfo = new WeaponBankInfo();
			weaponBankInfo.BankGUID = bank.GUID;
			if (list.Count > 0)
			{
				weaponBankInfo.WeaponID = DesignLab.PickBestWeaponForRole(game, list, playerID, (bank.TurretSize == WeaponEnums.WeaponSizes.Light) ? WeaponRole.POINT_DEFENSE : wpnRole, optionalAITechStyles, turretClass, playerFaction);
				if (App.Log.Level >= LogLevel.Verbose)
				{
					if (!weaponBankInfo.WeaponID.HasValue)
					{
						DesignLab.TraceVerbose("Selected weapon: none");
					}
					else
					{
						string weaponAsset = game.GameDatabase.GetWeaponAsset(weaponBankInfo.WeaponID.Value);
						if (weaponAsset != null)
						{
							DesignLab.TraceVerbose("Selected weapon: " + weaponAsset);
						}
						else
						{
							DesignLab.TraceVerbose(string.Format("Selected weapon: (not in db: {0})", weaponBankInfo.WeaponID.Value));
						}
					}
				}
			}
			return weaponBankInfo;
		}
		private static float PickTechStylesWeaponMultiplier(LogicalWeapon wpn, AITechStyles optionalTechStyles)
		{
			if (optionalTechStyles == null)
			{
				return 1f;
			}
			Kerberos.Sots.Data.WeaponFramework.Tech[] requiredTechs = wpn.RequiredTechs;
			Kerberos.Sots.Data.WeaponFramework.Tech required;
			for (int i = 0; i < requiredTechs.Length; i++)
			{
				required = requiredTechs[i];
				if (optionalTechStyles.TechUnion.Any((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == required.Name))
				{
					return 2f;
				}
			}
			return 1f;
		}
		private static float PickFactionWeaponModifier(LogicalWeapon wpn, WeaponEnums.TurretClasses weaponType, string faction)
		{
			switch (faction)
			{
			case "hiver":
				if (wpn.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic) || wpn.PayloadType == WeaponEnums.PayloadTypes.Missile)
				{
					return 2f;
				}
				break;
			case "human":
				if (!wpn.Traits.Any((WeaponEnums.WeaponTraits x) => x == WeaponEnums.WeaponTraits.Ballistic || x == WeaponEnums.WeaponTraits.Energy) && (weaponType != WeaponEnums.TurretClasses.Torpedo || wpn.Traits.Contains(WeaponEnums.WeaponTraits.Tracking)))
				{
					return 0.5f;
				}
				break;
			case "liir_zuul":
				if (weaponType == WeaponEnums.TurretClasses.Torpedo || wpn.Traits.Contains(WeaponEnums.WeaponTraits.Energy))
				{
					return 2f;
				}
				if (wpn.PayloadType == WeaponEnums.PayloadTypes.Missile)
				{
					return 0.5f;
				}
				break;
			case "morrigi":
				if (wpn.Traits.Contains(WeaponEnums.WeaponTraits.Energy) || wpn.PayloadType == WeaponEnums.PayloadTypes.BattleRider)
				{
					return 2f;
				}
				break;
			case "tarkas":
				if (wpn.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic))
				{
					return 2f;
				}
				if (wpn.Traits.Contains(WeaponEnums.WeaponTraits.Energy))
				{
					return 0.5f;
				}
				break;
			case "zuul":
				if (wpn.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic) || wpn.PayloadType == WeaponEnums.PayloadTypes.Missile)
				{
					return 3f;
				}
				break;
			case "loa":
			{
				float num2 = 0f;
				if (wpn.Traits.Contains(WeaponEnums.WeaponTraits.Brawler))
				{
					num2 += 1f;
				}
				if (wpn.Traits.Any((WeaponEnums.WeaponTraits x) => x == WeaponEnums.WeaponTraits.Energy || x == WeaponEnums.WeaponTraits.Draining) || wpn.PayloadType == WeaponEnums.PayloadTypes.Torpedo)
				{
					num2 += 2f;
				}
				if (num2 > 0f)
				{
					return num2 + 1f;
				}
				break;
			}
			}
			return 1f;
		}
		private static float PickHighDamageWeaponScore(AssetDatabase assetdb, LogicalWeapon wpn)
		{
			float num = 0f;
			if (wpn.IsPDWeapon())
			{
				return num;
			}
			int num2 = 1;
			LogicalWeapon logicalWeapon = wpn;
			if (!string.IsNullOrEmpty(wpn.SubWeapon) && wpn.NumSubWeapons > 0)
			{
				LogicalWeapon logicalWeapon2 = assetdb.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == wpn.SubWeapon);
				if (logicalWeapon2 != null)
				{
					logicalWeapon = logicalWeapon2;
					num2 = wpn.NumSubWeapons;
				}
			}
			if (logicalWeapon.PayloadType == WeaponEnums.PayloadTypes.DOTCloud)
			{
				float num3 = 1f + logicalWeapon.RangeTable.Maximum.Range / 500f;
				num = logicalWeapon.DOTDamage * (logicalWeapon.TimeToLive / 0.5f) / wpn.RechargeTime * num3;
			}
			else
			{
				if (logicalWeapon.PayloadType == WeaponEnums.PayloadTypes.Emitter)
				{
					num = logicalWeapon.RangeTable.Effective.Damage * logicalWeapon.Duration / logicalWeapon.BeamDamagePeriod / logicalWeapon.RechargeTime;
				}
				else
				{
					if (logicalWeapon.PayloadType == WeaponEnums.PayloadTypes.Beam)
					{
						num = logicalWeapon.RangeTable.Effective.Damage * logicalWeapon.Duration / logicalWeapon.BeamDamagePeriod / logicalWeapon.RechargeTime / (logicalWeapon.RangeTable.Effective.Deviation + 1f);
					}
					else
					{
						float num4 = 1f - 1f * Math.Abs(logicalWeapon.VolleyDeviation).Normalize(0f, 10f);
						float num5 = 1f + (float)logicalWeapon.ArmorPiercingLevel / 5f;
						num = logicalWeapon.RangeTable.Effective.Damage * (float)logicalWeapon.NumVolleys / logicalWeapon.RechargeTime / (logicalWeapon.RangeTable.Effective.Deviation + 1f) * num4 * num5;
					}
				}
			}
			return num * (float)num2;
		}
		private static float PickPointDefenseWeaponScore(LogicalWeapon wpn)
		{
			return 100f / wpn.RechargeTime + wpn.RangeTable.Effective.Damage / 1024f;
		}
		private static float PickPlanetAttackWeaponScore(LogicalWeapon wpn)
		{
			return wpn.PopDamage;
		}
		private static float PickDisablingWeaponScore(AssetDatabase assetdb, LogicalWeapon wpn)
		{
			float num = DesignLab.PickHighDamageWeaponScore(assetdb, wpn);
			bool flag = wpn.Traits.Any((WeaponEnums.WeaponTraits x) => x == WeaponEnums.WeaponTraits.Disabling);
			return num * (float)(flag ? 1000 : 1);
		}
		private static float PickLongRangeWeaponScore(LogicalWeapon wpn)
		{
			return wpn.RangeTable.Effective.Range + wpn.RangeTable.Effective.Damage * 5f;
		}
		private static float PickWeaponScore(AssetDatabase assetdb, LogicalWeapon weapon, WeaponRole role, AITechStyles optionalTechStyles, WeaponEnums.TurretClasses weaponType, string playerFaction)
		{
			float num;
			switch (role)
			{
			case WeaponRole.STAND_OFF:
				num = DesignLab.PickLongRangeWeaponScore(weapon);
				break;
			case WeaponRole.BRAWLER:
				num = DesignLab.PickHighDamageWeaponScore(assetdb, weapon);
				break;
			case WeaponRole.POINT_DEFENSE:
				num = DesignLab.PickPointDefenseWeaponScore(weapon);
				break;
			case WeaponRole.PLANET_ATTACK:
				num = DesignLab.PickPlanetAttackWeaponScore(weapon);
				break;
			case WeaponRole.DISABLING:
				num = DesignLab.PickDisablingWeaponScore(assetdb, weapon);
				break;
			case WeaponRole.ENERGY:
				num = DesignLab.PickHighDamageWeaponScore(assetdb, weapon);
				if (weapon.PayloadType == WeaponEnums.PayloadTypes.Bolt && weapon.Traits.Contains(WeaponEnums.WeaponTraits.Energy))
				{
					num *= 100f;
				}
				break;
			case WeaponRole.BALLISTICS:
				num = DesignLab.PickHighDamageWeaponScore(assetdb, weapon);
				if (weapon.PayloadType == WeaponEnums.PayloadTypes.Bolt && weapon.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic))
				{
					num *= 100f;
				}
				break;
			default:
				num = DesignLab.PickLongRangeWeaponScore(weapon);
				break;
			}
			num *= DesignLab.PickTechStylesWeaponMultiplier(weapon, optionalTechStyles);
			return num * DesignLab.PickFactionWeaponModifier(weapon, weaponType, playerFaction);
		}
		public static int? PickBestWeaponForRole(GameSession game, IList<LogicalWeapon> weapons, int PlayerId, WeaponRole role, AITechStyles optionalAITechStyles, WeaponEnums.TurretClasses bankType, string playerFaction)
		{
			DesignLab.WeaponScore weaponScore = (
				from x in weapons
				select new DesignLab.WeaponScore
				{
					Weapon = x,
					Score = DesignLab.PickWeaponScore(game.AssetDatabase, x, role, optionalAITechStyles, bankType, playerFaction)
				} into y
				orderby y.Score descending
				select y).First<DesignLab.WeaponScore>();
			return game.GameDatabase.GetWeaponID(weaponScore.Weapon.FileName, PlayerId);
		}
		private static ShipPreference[] LoadSectionPreferences(GameSession game)
		{
			List<ShipPreference> list = new List<ShipPreference>();
			foreach (string[] current in CsvOperations.Read(ScriptHost.FileSystem, "factions\\section_preferences.csv", '"', ',', 0, 3))
			{
				ShipPreference item;
				item.factionID = game.GameDatabase.GetFactionIdFromName(current[0]);
				item.sectionName = current[1];
				item.preferenceWeight = float.Parse(current[2]);
				list.Add(item);
			}
			return list.ToArray();
		}
		private static bool AreSectionsCompatible(ShipSectionAsset section1, ShipSectionAsset section2)
		{
			return !section1.ExcludeSectionTypes.Contains(section2.Type) && !section2.ExcludeSectionTypes.Contains(section1.Type) && !section1.SectionIsExcluded(section2) && !section2.SectionIsExcluded(section1);
		}
		private static bool ShipClassWantsAdditionalSections(ShipClass value, ShipSectionType sectionType)
		{
			if (sectionType == ShipSectionType.Mission)
			{
				return true;
			}
			switch (value)
			{
			case ShipClass.Leviathan:
			case ShipClass.Station:
				return false;
			}
			return true;
		}
		public static void SummarizeDesign(AssetDatabase assetDatabase, GameDatabase gameDatabase, DesignInfo design)
		{
			DesignSectionInfo[] designSections = design.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				designSectionInfo.DesignInfo = design;
				designSectionInfo.ShipSectionAsset = assetDatabase.GetShipSectionAsset(designSectionInfo.FilePath);
			}
			IEnumerable<ShipSectionAsset> source = 
				from y in design.DesignSections
				select y.ShipSectionAsset into x
				where x.Type == ShipSectionType.Mission
				select x;
			if (source.Count<ShipSectionAsset>() != 1)
			{
				throw new InvalidOperationException("Ship design requires exactly one mission section.");
			}
			ShipSectionAsset shipSectionAsset = source.First<ShipSectionAsset>();
			bool flag = DesignLab.ShipClassWantsAdditionalSections(shipSectionAsset.Class, ShipSectionType.Command) && !shipSectionAsset.ExcludeSectionTypes.Contains(ShipSectionType.Command);
			IEnumerable<ShipSectionAsset> source2 = 
				from y in design.DesignSections
				select y.ShipSectionAsset into x
				where x.Type == ShipSectionType.Command
				select x;
			if (flag)
			{
				if (source2.Count<ShipSectionAsset>() != 1)
				{
					throw new InvalidOperationException("Ship design requires exactly one command section.");
				}
			}
			else
			{
				if (source2.Any<ShipSectionAsset>())
				{
					throw new InvalidOperationException("Ship design cannot have a command section.");
				}
			}
			bool flag2 = DesignLab.ShipClassWantsAdditionalSections(shipSectionAsset.Class, ShipSectionType.Engine) && !shipSectionAsset.ExcludeSectionTypes.Contains(ShipSectionType.Engine);
			IEnumerable<ShipSectionAsset> source3 = 
				from y in design.DesignSections
				select y.ShipSectionAsset into x
				where x.Type == ShipSectionType.Engine
				select x;
			if (flag2)
			{
				if (source3.Count<ShipSectionAsset>() != 1)
				{
					throw new InvalidOperationException("Ship design requires exactly one engine section.");
				}
			}
			else
			{
				if (source3.Any<ShipSectionAsset>())
				{
					throw new InvalidOperationException("Ship design cannot have an engine section.");
				}
			}
			design.SavingsCost = 0;
			design.StratSensorRange = 0f;
			design.ProductionCost = 0;
			design.Armour = design.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.Armor.Sum((Kerberos.Sots.Framework.Size y) => y.X * y.Y));
			design.Structure = (float)design.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.Structure);
			design.NumTurrets = design.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.Mounts.Length);
			design.Mass = design.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.Mass);
			design.Acceleration = design.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.Maneuvering.LinearAccel);
			design.TopSpeed = design.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.Maneuvering.LinearSpeed);
			design.Class = shipSectionAsset.Class;
			design.DesignDate = gameDatabase.GetTurnCount();
			if (design.DesignSections[0].ShipSectionAsset.StationType == StationType.INVALID_TYPE && design.Class != ShipClass.Station && design.StationType == StationType.INVALID_TYPE && design.Role == ShipRole.UNDEFINED)
			{
				design.Role = DesignLab.GetRole(shipSectionAsset);
			}
			design.CrewAvailable = 0;
			design.PowerAvailable = 0;
			design.SupplyAvailable = 0;
			design.CrewRequired = 0;
			design.PowerRequired = 0;
			design.SupplyRequired = 0;
			string name = gameDatabase.AssetDatabase.GetFaction(gameDatabase.GetPlayerFactionID(design.PlayerID)).Name;
			List<PlayerTechInfo> playerTechs = (
				from x in gameDatabase.GetPlayerTechInfos(design.PlayerID)
				where x.State == TechStates.Researched
				select x).ToList<PlayerTechInfo>();
			DesignSectionInfo[] designSections2 = design.DesignSections;
			for (int j = 0; j < designSections2.Length; j++)
			{
				DesignSectionInfo designSectionInfo2 = designSections2[j];
				List<string> list = new List<string>();
				if (designSectionInfo2.Techs != null)
				{
					foreach (int current in designSectionInfo2.Techs)
					{
						list.Add(gameDatabase.GetTechFileID(current));
					}
				}
				ShipSectionAsset sectionAsset = designSectionInfo2.ShipSectionAsset;
				double num = (double)sectionAsset.SavingsCost;
				design.CrewAvailable += sectionAsset.Crew;
				design.PowerAvailable += Ship.GetPowerWithTech(assetDatabase, list, playerTechs, sectionAsset.Power);
				design.SupplyAvailable += Ship.GetSupplyWithTech(assetDatabase, list, sectionAsset.Supply);
				int num2 = sectionAsset.CrewRequired;
				double num3 = 0.0;
				if (designSectionInfo2.Techs != null)
				{
					foreach (int current2 in designSectionInfo2.Techs)
					{
						string techIdentifier = gameDatabase.GetTechFileID(current2);
						Kerberos.Sots.Data.TechnologyFramework.Tech tech = assetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techIdentifier);
						num3 += Math.Max((double)tech.CostMultiplier - 1.0, 0.0);
					}
				}
				design.ProductionCost += sectionAsset.ProductionCost;
				if (name == "loa")
				{
					design.ProductionCost += (int)((double)sectionAsset.ProductionCost * (num3 * (double)gameDatabase.AssetDatabase.LoaTechModMod));
				}
				num += num * num3;
				float num4 = 1f;
				if (designSectionInfo2.Modules != null)
				{
					foreach (DesignModuleInfo current3 in designSectionInfo2.Modules)
					{
						string modulePath = gameDatabase.GetModuleAsset(current3.ModuleID);
						LogicalModule module = assetDatabase.Modules.First((LogicalModule x) => x.ModulePath == modulePath);
						if (module.CrewEfficiencyBonus > 0f)
						{
							num4 *= module.CrewEfficiencyBonus;
						}
						design.CrewAvailable += module.Crew;
						design.PowerAvailable += module.PowerBonus;
						design.SupplyAvailable += module.Supply;
						num2 += module.CrewRequired;
						num += (double)module.SavingsCost;
						design.ProductionCost += module.ProductionCost;
						int? weaponID = current3.WeaponID;
						if (weaponID.HasValue)
						{
							string weaponPath = gameDatabase.GetWeaponAsset(weaponID.Value);
							LogicalWeapon logicalWeapon = assetDatabase.Weapons.First((LogicalWeapon x) => x.FileName == weaponPath);
							int num5 = module.Mounts.Count((LogicalMount x) => x.Bank == module.Banks[0]);
							num2 += (logicalWeapon.isCrewPerBank ? logicalWeapon.Crew : (logicalWeapon.Crew * num5));
							design.PowerRequired += (logicalWeapon.isPowerPerBank ? logicalWeapon.Power : (logicalWeapon.Power * num5));
							design.SupplyRequired += (logicalWeapon.isSupplyPerBank ? logicalWeapon.Supply : (logicalWeapon.Supply * num5));
						}
						if (design.StationType != StationType.INVALID_TYPE)
						{
							if (design.StationType == StationType.DIPLOMATIC && design.StationLevel > 0)
							{
								if (current3.StationModuleType == ModuleEnums.StationModuleType.Sensor)
								{
									design.StratSensorRange += 0.2f;
									design.TacSensorRange += 200f;
								}
							}
							else
							{
								if (design.StationType == StationType.NAVAL && design.StationLevel > 0)
								{
									if (current3.StationModuleType == ModuleEnums.StationModuleType.Sensor)
									{
										design.StratSensorRange += 0.5f;
										design.TacSensorRange += 500f;
									}
								}
								else
								{
									if (design.StationType == StationType.SCIENCE && design.StationLevel > 0)
									{
										if (current3.StationModuleType == ModuleEnums.StationModuleType.Sensor)
										{
											design.StratSensorRange += 0.25f;
											design.TacSensorRange += 250f;
										}
									}
									else
									{
										if (design.StationType == StationType.CIVILIAN && design.StationLevel > 0)
										{
											if (current3.StationModuleType == ModuleEnums.StationModuleType.Sensor)
											{
												design.StratSensorRange += 0.25f;
												design.TacSensorRange += 500f;
											}
										}
										else
										{
											if (design.StationType == StationType.GATE && design.StationLevel > 0)
											{
												ModuleEnums.StationModuleType? stationModuleType = current3.StationModuleType;
												ModuleEnums.StationModuleType valueOrDefault = stationModuleType.GetValueOrDefault();
												if (stationModuleType.HasValue)
												{
													if (valueOrDefault != ModuleEnums.StationModuleType.Sensor)
													{
														if (valueOrDefault == ModuleEnums.StationModuleType.Bastion)
														{
															design.Structure += design.Structure * 0.1f;
														}
													}
													else
													{
														design.StratSensorRange += 0.5f;
														design.TacSensorRange += 500f;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				if (designSectionInfo2.WeaponBanks != null)
				{
					List<WeaponBankInfo> list2 = designSectionInfo2.WeaponBanks.ToList<WeaponBankInfo>();
					int iBank;
					for (iBank = 0; iBank < sectionAsset.Banks.Length; iBank++)
					{
						if (iBank < list2.Count)
						{
							int? weaponID2 = list2[iBank].WeaponID;
							if (weaponID2.HasValue && weaponID2.Value != 0)
							{
								string weaponPath = gameDatabase.GetWeaponAsset(list2[iBank].WeaponID.Value);
								LogicalWeapon logicalWeapon2 = assetDatabase.Weapons.First((LogicalWeapon x) => x.FileName == weaponPath);
								int num6 = sectionAsset.Mounts.Count((LogicalMount x) => x.Bank == sectionAsset.Banks[iBank]);
								int num7 = WeaponSizes.GuessNumBarrels(logicalWeapon2.DefaultWeaponSize, sectionAsset.Banks[iBank].TurretSize);
								num2 += (logicalWeapon2.isCrewPerBank ? logicalWeapon2.Crew : (logicalWeapon2.Crew * num6 * num7));
								design.PowerRequired += (logicalWeapon2.isPowerPerBank ? logicalWeapon2.Power : (logicalWeapon2.Power * num6 * num7));
								design.SupplyRequired += (logicalWeapon2.isSupplyPerBank ? logicalWeapon2.Supply : (logicalWeapon2.Supply * num6 * num7));
								num += (double)(logicalWeapon2.Cost * num7 * num6);
								if (gameDatabase.AssetDatabase.GetFaction(gameDatabase.GetPlayerFactionID(design.PlayerID)).Name == "loa")
								{
									design.ProductionCost += (int)((float)(logicalWeapon2.Cost * num7 * num6) * 0.1f);
								}
								if (list2[iBank].DesignID.HasValue)
								{
									DesignInfo designInfo = gameDatabase.GetDesignInfo(list2[iBank].DesignID.Value);
									if (designInfo != null)
									{
										num += (double)(designInfo.SavingsCost * num6);
									}
								}
							}
						}
					}
				}
				design.CrewRequired += (int)Math.Round((double)((float)num2 * num4));
				design.SavingsCost += (int)num;
			}
			design.SupplyRequired += (int)Math.Ceiling((double)((float)design.CrewAvailable / 2f));
			if (string.IsNullOrEmpty(design.Name))
			{
				DesignLab.NameGenerators nameGenerator;
				switch (design.Role)
				{
				case ShipRole.DRONE:
				case ShipRole.ASSAULTSHUTTLE:
				case ShipRole.BOARDINGPOD:
				case ShipRole.BIOMISSILE:
				case ShipRole.TRAPDRONE:
					nameGenerator = DesignLab.NameGenerators.MissionSectionDerived;
					goto IL_CFF;
				}
				nameGenerator = DesignLab.NameGenerators.FactionRandom;
				IL_CFF:
				design.Name = DesignLab.GenerateDesignName(assetDatabase, gameDatabase, null, design, nameGenerator);
			}
			if (shipSectionAsset.IsGateShip)
			{
				design.Role = ShipRole.GATE;
				return;
			}
			if (shipSectionAsset.IsBoreShip)
			{
				design.Role = ShipRole.BORE;
				return;
			}
			if (shipSectionAsset.IsFreighter)
			{
				design.Role = ShipRole.FREIGHTER;
				return;
			}
			if (shipSectionAsset.isConstructor)
			{
				design.Role = ShipRole.CONSTRUCTOR;
				return;
			}
			if (shipSectionAsset.CombatAIType == SectionEnumerations.CombatAiType.TrapDrone)
			{
				design.Role = ShipRole.TRAPDRONE;
				return;
			}
			if (shipSectionAsset.RealClass == RealShipClasses.AssaultShuttle)
			{
				design.Role = ((shipSectionAsset.SlaveCapacity > 0) ? ShipRole.SLAVEDISK : ShipRole.ASSAULTSHUTTLE);
				return;
			}
			if (shipSectionAsset.RealClass == RealShipClasses.BoardingPod)
			{
				design.Role = ShipRole.BOARDINGPOD;
				return;
			}
			if (shipSectionAsset.RealClass == RealShipClasses.Drone)
			{
				design.Role = ShipRole.DRONE;
				return;
			}
			if (shipSectionAsset.RealClass == RealShipClasses.Biomissile)
			{
				design.Role = ShipRole.BIOMISSILE;
				return;
			}
			if (shipSectionAsset.IsScavenger)
			{
				design.Role = ShipRole.SCAVENGER;
				return;
			}
			if (shipSectionAsset.BattleRiderType == BattleRiderTypes.escort)
			{
				design.Role = ShipRole.BR_ESCORT;
				return;
			}
			if (shipSectionAsset.BattleRiderType == BattleRiderTypes.interceptor)
			{
				design.Role = ShipRole.BR_INTERCEPTOR;
				return;
			}
			if (shipSectionAsset.BattleRiderType == BattleRiderTypes.patrol)
			{
				design.Role = ShipRole.BR_PATROL;
				return;
			}
			if (shipSectionAsset.BattleRiderType == BattleRiderTypes.scout)
			{
				design.Role = ShipRole.BR_SCOUT;
				return;
			}
			if (shipSectionAsset.BattleRiderType == BattleRiderTypes.spinal)
			{
				design.Role = ShipRole.BR_SPINAL;
				return;
			}
			if (shipSectionAsset.BattleRiderType == BattleRiderTypes.torpedo)
			{
				design.Role = ShipRole.BR_TORPEDO;
				return;
			}
			if (shipSectionAsset.RealClass == RealShipClasses.BattleCruiser)
			{
				design.Role = ShipRole.BATTLECRUISER;
				return;
			}
			if (shipSectionAsset.RealClass == RealShipClasses.BattleShip)
			{
				design.Role = ShipRole.BATTLESHIP;
			}
		}
		internal static string GenerateDesignName(AssetDatabase assetdb, GameDatabase db, DesignInfo predecessor, DesignInfo newDesign, DesignLab.NameGenerators nameGenerator)
		{
			if (predecessor != null)
			{
				return predecessor.Name;
			}
			PlayerInfo playerInfo = db.GetPlayerInfo(newDesign.PlayerID);
			if (nameGenerator == DesignLab.NameGenerators.FactionRandom)
			{
				FactionInfo factionInfo = db.GetFactionInfo(playerInfo.FactionID);
				Faction faction = assetdb.GetFaction(factionInfo.Name);
				return AssetDatabase.CommonStrings.Localize(faction.DesignNames.GetNextStringID());
			}
			return App.Localize((
				from x in newDesign.DesignSections
				select assetdb.GetShipSectionAsset(x.FilePath)).First((ShipSectionAsset y) => y.Type == ShipSectionType.Mission).Title);
		}
		public static DesignInfo GetStationDesignInfo(AssetDatabase assetdb, GameDatabase gamedb, int playerId, StationType type, int level)
		{
			string arg = "";
			if (level == 0)
			{
				arg = "sn_underconstruction.section";
			}
			else
			{
				switch (type)
				{
				case StationType.NAVAL:
					switch (level)
					{
					case 1:
						arg = "sn_naval_outpost.section";
						break;
					case 2:
						arg = "sn_naval_forward_base.section";
						break;
					case 3:
						arg = "sn_naval_naval_base.section";
						break;
					case 4:
						arg = "sn_naval_star_base.section";
						break;
					case 5:
						arg = "sn_naval_sector_base.section";
						break;
					}
					break;
				case StationType.SCIENCE:
					switch (level)
					{
					case 1:
						arg = "sn_science_field_station.section";
						break;
					case 2:
						arg = "sn_science_star_lab.section";
						break;
					case 3:
						arg = "sn_science_research_base.section";
						break;
					case 4:
						arg = "sn_science_polytechnic_institute.section";
						break;
					case 5:
						arg = "sn_science_science_center.section";
						break;
					}
					break;
				case StationType.CIVILIAN:
					switch (level)
					{
					case 1:
						arg = "sn_civilian_way_station.section";
						break;
					case 2:
						arg = "sn_civilian_trading_post.section";
						break;
					case 3:
						arg = "sn_civilian_merchanter_station.section";
						break;
					case 4:
						arg = "sn_civilian_nexus.section";
						break;
					case 5:
						arg = "sn_civilian_star_city.section";
						break;
					}
					break;
				case StationType.DIPLOMATIC:
					switch (level)
					{
					case 1:
						arg = "sn_diplomatic_customs_station.section";
						break;
					case 2:
						arg = "sn_diplomatic_consulate.section";
						break;
					case 3:
						arg = "sn_diplomatic_embassy.section";
						break;
					case 4:
						arg = "sn_diplomatic_council_station.section";
						break;
					case 5:
						arg = "sn_diplomatic_star_chamber.section";
						break;
					}
					break;
				case StationType.GATE:
					switch (level)
					{
					case 1:
						arg = "sn_gate_gateway.section";
						break;
					case 2:
						arg = "sn_gate_caster.section";
						break;
					case 3:
						arg = "sn_gate_far_caster.section";
						break;
					case 4:
						arg = "sn_gate_lens.section";
						break;
					case 5:
						arg = "sn_gate_mirror_of_creation.section";
						break;
					}
					break;
				case StationType.MINING:
					arg = "sn_mining_station.section";
					break;
				case StationType.DEFENCE:
					arg = CommonSectionNames.cr_mis_sdb + ".section";
					break;
				}
			}
			string name = gamedb.GetFactionInfo(gamedb.GetPlayerFactionID(playerId)).Name;
			string fullPath = string.Format("factions\\{0}\\sections\\{1}", name, arg);
			ShipSectionAsset shipSectionAsset = null;
			if (shipSectionAsset == null)
			{
				shipSectionAsset = assetdb.ShipSections.First((ShipSectionAsset x) => x.FileName == fullPath);
			}
			DesignInfo designInfo = new DesignInfo(playerId, App.Localize(shipSectionAsset.Title), new string[]
			{
				fullPath
			});
			designInfo.StationType = type;
			designInfo.StationLevel = level;
			DesignLab.SummarizeDesign(assetdb, gamedb, designInfo);
			return designInfo;
		}
		public static List<WeaponRole> GetWeaponRolesForNewDesign(AIStance stance, ShipRole role, ShipClass shipClass)
		{
			float[] array = new float[8];
			switch (stance)
			{
			case AIStance.EXPANDING:
				array[2] = 1f;
				array[1] = 2f;
				break;
			case AIStance.ARMING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1f;
				array[4] = 0.8f;
				array[5] = 0.5f;
				break;
			case AIStance.HUNKERING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1f;
				array[4] = 0.8f;
				array[5] = 0.5f;
				break;
			case AIStance.CONQUERING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1f;
				array[4] = 1.5f;
				array[5] = 0.5f;
				break;
			case AIStance.DESTROYING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1f;
				array[4] = 1.5f;
				array[5] = 0.5f;
				break;
			case AIStance.DEFENDING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1.5f;
				array[4] = 0.8f;
				array[5] = 1f;
				break;
			}
			switch (role)
			{
			case ShipRole.COMBAT:
			case ShipRole.POLICE:
			case ShipRole.PLATFORM:
			case ShipRole.BR_PATROL:
			case ShipRole.BR_SPINAL:
			case ShipRole.BR_INTERCEPTOR:
			case ShipRole.BR_TORPEDO:
			case ShipRole.BATTLECRUISER:
			case ShipRole.BATTLESHIP:
				array[2] *= 1.5f;
				break;
			case ShipRole.CARRIER:
			case ShipRole.CARRIER_ASSAULT:
			case ShipRole.CARRIER_DRONE:
			case ShipRole.CARRIER_BIO:
			case ShipRole.CARRIER_BOARDING:
				array[2] *= 0f;
				array[4] *= 1.5f;
				array[3] *= 2f;
				break;
			case ShipRole.COMMAND:
				array[2] *= 0f;
				array[4] *= 0f;
				array[5] *= 2f;
				break;
			case ShipRole.COLONIZER:
			case ShipRole.BR_SCOUT:
				array[2] *= 0f;
				array[3] *= 1.5f;
				array[4] *= 2f;
				break;
			case ShipRole.CONSTRUCTOR:
				array[2] *= 0f;
				array[4] *= 0f;
				array[3] *= 2f;
				break;
			case ShipRole.SCOUT:
			case ShipRole.BR_ESCORT:
				array[4] *= 0f;
				array[2] *= 0.5f;
				array[1] *= 1.5f;
				array[5] *= 3f;
				break;
			case ShipRole.SUPPLY:
				array[2] *= 0f;
				array[4] *= 0f;
				array[3] *= 2f;
				break;
			}
			List<WeaponRole> list = new List<WeaponRole>();
			for (int i = 0; i < 8; i++)
			{
				if (array[i] > 0f)
				{
					list.Add((WeaponRole)i);
				}
			}
			if (list.Count == 0)
			{
				list.Add(WeaponRole.STAND_OFF);
			}
			return list;
		}
		public static WeaponRole SuggestWeaponRoleForNewDesign(AIStance stance, ShipRole role, ShipClass shipClass)
		{
			float[] array = new float[8];
			switch (stance)
			{
			case AIStance.EXPANDING:
				array[2] = 1f;
				array[1] = 2f;
				break;
			case AIStance.ARMING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1f;
				array[4] = 0.8f;
				array[5] = 0.5f;
				break;
			case AIStance.HUNKERING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1f;
				array[4] = 0.8f;
				array[5] = 0.5f;
				break;
			case AIStance.CONQUERING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1f;
				array[4] = 1.5f;
				array[5] = 0.5f;
				break;
			case AIStance.DESTROYING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1f;
				array[4] = 1.5f;
				array[5] = 0.5f;
				break;
			case AIStance.DEFENDING:
				array[2] = 1f;
				array[1] = 1.5f;
				array[3] = 1.5f;
				array[4] = 0.8f;
				array[5] = 1f;
				break;
			}
			switch (role)
			{
			case ShipRole.COMBAT:
			case ShipRole.POLICE:
			case ShipRole.PLATFORM:
			case ShipRole.BR_PATROL:
			case ShipRole.BR_SPINAL:
			case ShipRole.BR_INTERCEPTOR:
			case ShipRole.BR_TORPEDO:
			case ShipRole.BATTLECRUISER:
			case ShipRole.BATTLESHIP:
				array[2] *= 1.5f;
				break;
			case ShipRole.CARRIER:
			case ShipRole.CARRIER_ASSAULT:
			case ShipRole.CARRIER_DRONE:
			case ShipRole.CARRIER_BIO:
			case ShipRole.CARRIER_BOARDING:
				array[2] *= 0f;
				array[4] *= 1.5f;
				array[3] *= 2f;
				break;
			case ShipRole.COMMAND:
				array[2] *= 0f;
				array[4] *= 0f;
				array[5] *= 2f;
				break;
			case ShipRole.COLONIZER:
			case ShipRole.BR_SCOUT:
				array[2] *= 0f;
				array[3] *= 1.5f;
				array[4] *= 2f;
				break;
			case ShipRole.CONSTRUCTOR:
				array[2] *= 0f;
				array[4] *= 0f;
				array[3] *= 2f;
				break;
			case ShipRole.SCOUT:
			case ShipRole.BR_ESCORT:
				array[4] *= 0f;
				array[2] *= 0.5f;
				array[1] *= 1.5f;
				array[5] *= 3f;
				break;
			case ShipRole.SUPPLY:
				array[2] *= 0f;
				array[4] *= 0f;
				array[3] *= 2f;
				break;
			}
			float num = 0f;
			float[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				float num2 = array2[i];
				num += num2;
			}
			float num3 = (float)App.GetSafeRandom().NextDouble() * num;
			for (int j = 1; j < array.Count<float>(); j++)
			{
				if (num3 < array[j])
				{
					return (WeaponRole)j;
				}
				num3 -= array[j];
			}
			return WeaponRole.STAND_OFF;
		}
		public static DesignInfo GetDesignByRole(GameSession game, Player player, AITechStyles techStyles, AIStance? stance, ShipRole shipRole, WeaponRole? weaponRole = null)
		{
			return DesignLab.SetDefaultDesign(game, shipRole, weaponRole, player.ID, null, null, techStyles, stance);
		}
		public static DesignInfo GetBestDesignByRole(GameSession game, Player player, AIStance? stance, ShipRole desiredRole, List<ShipRole> desiredRoles, WeaponRole? weaponRole = null)
		{
			List<DesignInfo> currentDesignsByRole = DesignLab.GetCurrentDesignsByRole(game, player, stance, desiredRoles, weaponRole);
			int num = 0;
			DesignInfo result = null;
			foreach (DesignInfo current in currentDesignsByRole)
			{
				int num2 = DesignLab.GetDesignScore(current);
				if (current.Role == desiredRole)
				{
					if (weaponRole.HasValue && current.WeaponRole == weaponRole)
					{
						num2 *= 20;
					}
					else
					{
						num2 *= 10;
					}
				}
				if (num2 > num)
				{
					num = num2;
					result = current;
				}
			}
			return result;
		}
		private static int GetDesignScore(DesignInfo design)
		{
			if (design.isPrototyped)
			{
				switch (design.Class)
				{
				case ShipClass.Cruiser:
					return 2 * design.DesignDate;
				case ShipClass.Dreadnought:
					return 3 * design.DesignDate;
				case ShipClass.Leviathan:
					return 4 * design.DesignDate;
				case ShipClass.BattleRider:
					return design.DesignDate;
				}
			}
			return 0;
		}
		public static List<DesignInfo> GetCurrentDesignsByRole(GameSession game, Player player, AIStance? stance, List<ShipRole> desiredRoles, WeaponRole? weaponRole = null)
		{
			List<DesignInfo> source = game.GameDatabase.GetDesignInfosForPlayer(player.ID).ToList<DesignInfo>();
			List<DesignInfo> list = new List<DesignInfo>();
			ShipClass[] designShipClassFallback = DesignLab._designShipClassFallback;
			ShipClass shipClass;
			for (int i = 0; i < designShipClassFallback.Length; i++)
			{
				shipClass = designShipClassFallback[i];
				if (shipClass != ShipClass.Station)
				{
					List<WeaponRole> selectedWeaponRoles = new List<WeaponRole>();
					if (weaponRole.HasValue)
					{
						selectedWeaponRoles.Add(weaponRole.Value);
					}
					else
					{
						if (stance.HasValue)
						{
							using (List<ShipRole>.Enumerator enumerator = desiredRoles.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									ShipRole current = enumerator.Current;
									List<WeaponRole> weaponRolesForNewDesign = DesignLab.GetWeaponRolesForNewDesign(stance.Value, current, shipClass);
									foreach (WeaponRole current2 in weaponRolesForNewDesign)
									{
										if (!selectedWeaponRoles.Contains(current2))
										{
											selectedWeaponRoles.Add(current2);
										}
									}
								}
								goto IL_1D0;
							}
						}
						if (desiredRoles.Any((ShipRole x) => DesignLab.DefaultShipWeaponRoles.ContainsKey(x)))
						{
							using (List<ShipRole>.Enumerator enumerator3 = desiredRoles.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									ShipRole current3 = enumerator3.Current;
									if (DesignLab.DefaultShipWeaponRoles.ContainsKey(current3))
									{
										selectedWeaponRoles.Add(DesignLab.DefaultShipWeaponRoles[current3]);
									}
								}
								goto IL_1D0;
							}
						}
						selectedWeaponRoles.Add(WeaponRole.STAND_OFF);
					}
					IL_1D0:
					List<DesignInfo> list2 = (
						from x in source
						where x.Class == shipClass && desiredRoles.Contains(x.Role) && selectedWeaponRoles.Contains(x.WeaponRole)
						select x).ToList<DesignInfo>();
					foreach (DesignInfo current4 in list2)
					{
						if (!list.Contains(current4))
						{
							list.Add(current4);
						}
					}
				}
			}
			return list;
		}
		public static ShipClass SuggestShipClassForNewDesign(GameDatabase db, Player player, ShipRole role)
		{
			ShipClass shipClass = ShipClass.Cruiser;
			if (db.PlayerHasLeviathans(player.ID))
			{
				shipClass = ShipClass.Leviathan;
			}
			else
			{
				if (db.PlayerHasDreadnoughts(player.ID))
				{
					shipClass = ShipClass.Dreadnought;
				}
			}
			switch (role)
			{
			case ShipRole.COMBAT:
				if (shipClass == ShipClass.Leviathan && App.GetSafeRandom().Next(10) < 3)
				{
					return ShipClass.Leviathan;
				}
				if ((shipClass == ShipClass.Leviathan || shipClass == ShipClass.Dreadnought) && App.GetSafeRandom().Next(10) < 4)
				{
					return ShipClass.Dreadnought;
				}
				return ShipClass.Cruiser;
			case ShipRole.CARRIER:
			case ShipRole.CARRIER_ASSAULT:
			case ShipRole.CARRIER_DRONE:
			case ShipRole.CARRIER_BIO:
			case ShipRole.CARRIER_BOARDING:
				return shipClass;
			case ShipRole.COMMAND:
				if (shipClass == ShipClass.Leviathan)
				{
					return ShipClass.Dreadnought;
				}
				return shipClass;
			case ShipRole.COLONIZER:
			case ShipRole.POLICE:
				return ShipClass.Cruiser;
			case ShipRole.CONSTRUCTOR:
				return ShipClass.Cruiser;
			case ShipRole.SCOUT:
				return ShipClass.Cruiser;
			case ShipRole.SUPPLY:
				if (shipClass == ShipClass.Leviathan)
				{
					return ShipClass.Dreadnought;
				}
				return shipClass;
			case ShipRole.DRONE:
			case ShipRole.ASSAULTSHUTTLE:
			case ShipRole.SLAVEDISK:
			case ShipRole.BR_PATROL:
			case ShipRole.BR_SCOUT:
			case ShipRole.BR_SPINAL:
			case ShipRole.BR_ESCORT:
			case ShipRole.BR_INTERCEPTOR:
			case ShipRole.BR_TORPEDO:
				return ShipClass.BattleRider;
			case ShipRole.PLATFORM:
				return ShipClass.Station;
			case ShipRole.BATTLECRUISER:
				return ShipClass.Cruiser;
			case ShipRole.BATTLESHIP:
				return ShipClass.Dreadnought;
			}
			return ShipClass.Cruiser;
		}
		public static RealShipClasses GetRealShipClassFromShipClassAndRole(ShipClass desiredClass, ShipRole role)
		{
			switch (role)
			{
			case ShipRole.PLATFORM:
				return RealShipClasses.Platform;
			case ShipRole.BR_PATROL:
			case ShipRole.BR_SCOUT:
			case ShipRole.BR_SPINAL:
			case ShipRole.BR_ESCORT:
			case ShipRole.BR_INTERCEPTOR:
			case ShipRole.BR_TORPEDO:
				return RealShipClasses.BattleRider;
			case ShipRole.BATTLECRUISER:
				return RealShipClasses.BattleCruiser;
			case ShipRole.BATTLESHIP:
				return RealShipClasses.BattleShip;
			default:
				switch (desiredClass)
				{
				case ShipClass.Cruiser:
					return RealShipClasses.Cruiser;
				case ShipClass.Dreadnought:
					return RealShipClasses.Dreadnought;
				case ShipClass.Leviathan:
					return RealShipClasses.Leviathan;
				default:
					return RealShipClasses.Cruiser;
				}
				break;
			}
		}
		public static string DeduceFleetTemplate(GameDatabase db, GameSession game, int FleetID)
		{
			FleetInfo fleetInfo = db.GetFleetInfo(FleetID);
            if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(db, fleetInfo) || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game, fleetInfo))
			{
				return "DEFAULT_COMBAT";
			}
			IEnumerable<ShipInfo> shipInfoByFleetID = db.GetShipInfoByFleetID(FleetID, true);
			string text = null;
			float num = -1f;
			List<FleetTemplate> list = new List<FleetTemplate>();
			foreach (FleetTemplate current in db.AssetDatabase.FleetTemplates)
			{
                if (current.MissionTypes.Any((MissionType x) => x == MissionType.COLONIZATION || x == MissionType.SUPPORT) && Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(game, FleetID) > 0.0)
				{
					list.Add(current);
				}
                if (current.MissionTypes.Any((MissionType x) => x == MissionType.CONSTRUCT_STN || x == MissionType.UPGRADE_STN) && Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(game, FleetID) > 0f)
				{
					list.Add(current);
				}
				if (current.MissionTypes.Any((MissionType x) => x == MissionType.GATE))
				{
					if (db.GetShipInfoByFleetID(FleetID, true).Any((ShipInfo x) => x.DesignInfo.Role == ShipRole.GATE))
					{
						list.Add(current);
					}
				}
			}
			if (list.Count == 1)
			{
				text = list.First<FleetTemplate>().Name;
			}
			if (text == null)
			{
				if (list.Count == 0)
				{
					list = db.AssetDatabase.FleetTemplates;
				}
				foreach (FleetTemplate current2 in list)
				{
					float num2 = 0f;
					float num3 = 0f;
					foreach (ShipInfo ship in shipInfoByFleetID)
					{
						if (current2.ShipIncludes.FirstOrDefault((ShipInclude x) => x.ShipRole == ship.DesignInfo.Role && x.WeaponRole == ship.DesignInfo.WeaponRole) != null)
						{
							num3 += 1f;
						}
						else
						{
							if (current2.ShipIncludes.FirstOrDefault((ShipInclude x) => x.ShipRole == ship.DesignInfo.Role) != null)
							{
								num2 += 1f;
							}
						}
					}
					num3 += num2 / 2f;
					if (num3 > num)
					{
						num = num3;
						text = current2.Name;
					}
				}
			}
			return text;
		}
		public static int GetTemplateFillAmount(GameDatabase db, FleetTemplate template, DesignInfo commandDesign, DesignInfo fillDesign)
		{
			int num = db.GetDesignCommandPointQuota(db.AssetDatabase, commandDesign.ID);
			int num2 = 0;
			int num3 = 0;
			foreach (ShipInclude current in template.ShipIncludes)
			{
				if (current.InclusionType == ShipInclusionType.REQUIRED)
				{
					num2 += fillDesign.CommandPointCost * current.Amount;
				}
				else
				{
					num3++;
				}
			}
			num -= num2;
			if (fillDesign.CommandPointCost == 0)
			{
				DesignLab.Warn("GetTemplateFillAmount: fillDesign.CommandPointCost is zero");
				return 0;
			}
			if (num3 == 0)
			{
				DesignLab.Warn("GetTemplateFillAmount: fillAmount is zero");
				return 0;
			}
			return num / fillDesign.CommandPointCost / num3;
		}
		public static List<int> GetMissionRequiredDesigns(GameSession game, MissionType missionType, int player)
		{
			List<int> list = new List<int>();
			IEnumerable<DesignInfo> designInfosForPlayer = game.GameDatabase.GetDesignInfosForPlayer(player);
			DesignInfo designInfo = null;
			switch (missionType)
			{
			case MissionType.COLONIZATION:
			case MissionType.SUPPORT:
				foreach (DesignInfo current in designInfosForPlayer)
				{
                    if (Kerberos.Sots.StarFleet.StarFleet.CanDesignColonize(game, current.ID))
					{
						if (designInfo == null)
						{
							designInfo = current;
						}
						else
						{
							if (designInfo.DesignDate < current.DesignDate)
							{
								designInfo = current;
							}
						}
					}
				}
				list.Add(designInfo.ID);
				break;
			case MissionType.CONSTRUCT_STN:
				foreach (DesignInfo current2 in designInfosForPlayer)
				{
                    if (Kerberos.Sots.StarFleet.StarFleet.CanDesignConstruct(game, current2.ID))
					{
						if (designInfo == null)
						{
							designInfo = current2;
						}
						else
						{
							if (designInfo.DesignDate < current2.DesignDate)
							{
								designInfo = current2;
							}
						}
					}
				}
				list.Add(designInfo.ID);
				break;
			}
			return list;
		}
		public static DesignInfo CreateStationDesignInfo(AssetDatabase assetdb, GameDatabase gamedb, int playerId, StationType type, int level, bool insertDesign)
		{
			DesignInfo designInfo = DesignLab.GetStationDesignInfo(assetdb, gamedb, playerId, type, level);
			if (insertDesign)
			{
				designInfo.ID = gamedb.InsertDesignByDesignInfo(designInfo);
				designInfo = gamedb.GetDesignInfo(designInfo.ID);
			}
			else
			{
				DesignLab.SummarizeDesign(assetdb, gamedb, designInfo);
			}
			return designInfo;
		}
		public static Rectangle GetShipSize(DesignInfo des)
		{
			if (des.Role == ShipRole.CONSTRUCTOR)
			{
				return DesignLab.DEFAULT_DREADNAUGHT_SIZE;
			}
			switch (des.Class)
			{
			case ShipClass.Cruiser:
				return DesignLab.DEFAULT_CRUISER_SIZE;
			case ShipClass.Dreadnought:
				return DesignLab.DEFAULT_DREADNAUGHT_SIZE;
			case ShipClass.Leviathan:
				return DesignLab.DEFAULT_LEVIATHAN_SIZE;
			default:
				return DesignLab.DEFAULT_CRUISER_SIZE;
			}
		}
		private static void PrintDesignTable(StringBuilder result, string title, IList<DesignInfo> designs)
		{
			result.AppendLine();
			result.AppendLine(string.Format("{0} ({1}):", title, designs.Count));
			result.AppendLine();
			result.AppendLine("       ID | Name                           | Class       | DesignDate | isPrototyped | Role            | WeaponRole      | NumBuilt ");
			result.AppendLine("----------+--------------------------------+-------------+------------+--------------+-----------------+-----------------+----------");
			foreach (DesignInfo current in designs)
			{
				result.AppendLine(string.Format(" {0,8} | {1,-30} | {2,-11} | {3,10} | {4,-12} | {5,-15} | {6,-15} | {7,8} ", new object[]
				{
					current.ID,
					current.Name,
					current.Class,
					current.DesignDate,
					current.isPrototyped,
					current.Role,
					current.WeaponRole,
					current.NumBuilt
				}));
			}
		}
		public static void PrintPlayerDesignSummary(StringBuilder result, App app, int playerid, bool includeStationDesigns = false)
		{
			List<DesignInfo> source = (
				from x in app.GameDatabase.GetDesignInfosForPlayer(playerid)
				where includeStationDesigns || x.Class != ShipClass.Station
				select x).ToList<DesignInfo>();
			List<DesignInfo> activeDesigns = (
				from x in app.GameDatabase.GetVisibleDesignInfosForPlayer(playerid)
				where includeStationDesigns || x.Class != ShipClass.Station
				select x).ToList<DesignInfo>();
			List<DesignInfo> designs = (
				from x in activeDesigns
				where StrategicAI.IsDesignObsolete(app.Game, playerid, x.ID)
				select x).ToList<DesignInfo>();
			List<DesignInfo> designs2 = (
				from x in source
				where !activeDesigns.Any((DesignInfo y) => y.ID == x.ID)
				select x).ToList<DesignInfo>();
			result.Append("Design reports for player " + playerid);
			if (!includeStationDesigns)
			{
				result.Append(", excluding station designs");
			}
			result.AppendLine(":");
			DesignLab.PrintDesignTable(result, "Active designs", activeDesigns);
			DesignLab.PrintDesignTable(result, "Obsolete designs (AI)", designs);
			DesignLab.PrintDesignTable(result, "Retired designs", designs2);
		}
	}
}
