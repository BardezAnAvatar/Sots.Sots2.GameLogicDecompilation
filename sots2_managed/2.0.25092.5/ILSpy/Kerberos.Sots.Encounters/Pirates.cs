using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class Pirates
	{
		private const string FactionName = "slavers";
		private const string PlayerName = "Pirate";
		private const string PlayerAvatar = "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga";
		private const string FleetName = "Pirate Raiders";
		private const string droneSection = "br_drone.section";
		private const string boardingPodSection = "br_boardingpod.section";
		private const string PirateBaseShipName = "Pirate Base";
		private const string PirateBaseSectionName = "sn_piratebase.section";
		private int _pirateBaseDesignId;
		private static readonly string[] availableCommandSections = new string[]
		{
			"cr_cmd.section",
			"cr_cmd_cloaking.section",
			"cr_cmd_hammerhead.section",
			"cr_cmd_strafe.section",
			"cr_cmd_deepscan.section"
		};
		private static readonly string[] availableMissionSections = new string[]
		{
			"cr_mis_armor.section",
			"cr_mis_boarding.section",
			"cr_mis_dronecarrier.section",
			"cr_mis_supply.section"
		};
		private static readonly string[] availableEngineSections = new string[]
		{
			"cr_eng_fusion.section",
			"cr_eng_antimatter.section"
		};
		private int PlayerId = -1;
		public static bool ForceEncounter = false;
		public int PirateBaseDesignId
		{
			get
			{
				return this._pirateBaseDesignId;
			}
		}
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public static Pirates InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Pirates pirates = new Pirates();
			pirates.PlayerId = gamedb.InsertPlayer("Pirate", "slavers", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			foreach (LogicalWeapon current in assetdb.Weapons)
			{
				gamedb.InsertWeapon(current, pirates.PlayerId);
			}
			if (gamedb.HasEndOfFleshExpansion())
			{
				pirates._pirateBaseDesignId = gamedb.InsertDesignByDesignInfo(new DesignInfo(pirates.PlayerId, "Pirate Base", new string[]
				{
					string.Format("factions\\{0}\\sections\\{1}", "slavers", "sn_piratebase.section")
				})
				{
					StationType = StationType.NAVAL
				});
			}
			return pirates;
		}
		public static Pirates ResumeEncounter(GameDatabase gamedb)
		{
			Pirates pirates = new Pirates();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Pirate"));
			if (playerInfo != null)
			{
				pirates.PlayerId = playerInfo.ID;
			}
			else
			{
				pirates.PlayerId = gamedb.InsertPlayer("Pirate", "slavers", null, new Vector3(0f), new Vector3(0f), "", "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			DesignInfo designInfo = gamedb.GetDesignInfosForPlayer(pirates.PlayerId).FirstOrDefault((DesignInfo x) => x.Name == "Pirate Base");
			pirates._pirateBaseDesignId = ((designInfo != null) ? designInfo.ID : -1);
			return pirates;
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, GameSession game, int SystemId, int OrbitId)
		{
			Random random = new Random();
			PirateBaseInfo pirateBaseInfo = new PirateBaseInfo();
			pirateBaseInfo.SystemId = SystemId;
			pirateBaseInfo.NumShips = assetdb.GlobalPiracyData.PiracyMinBaseShips;
			pirateBaseInfo.TurnsUntilAddShip = assetdb.GlobalPiracyData.PiracyBaseTurnsPerUpdate;
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(OrbitId);
			float num = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + (float)StarSystemVars.Instance.StationOrbitDistance;
			OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(OrbitId);
			OrbitalPath path = default(OrbitalPath);
			path.Scale = new Vector2(num, num);
			path.Rotation = new Vector3(0f, 0f, 0f);
			path.DeltaAngle = random.NextInclusive(0f, 360f);
			path.InitialAngle = 0f;
			DesignInfo designInfo = gamedb.GetDesignInfo(this.PirateBaseDesignId);
			pirateBaseInfo.BaseStationId = gamedb.InsertStation(OrbitId, orbitalObjectInfo.StarSystemID, path, designInfo.Name, this.PlayerId, designInfo);
			gamedb.InsertPirateBaseInfo(pirateBaseInfo);
		}
		public void UpdateTurn(GameSession game)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				game.GameDatabase.RemoveFleet(current.ID);
			}
			List<DesignInfo> list2 = game.GameDatabase.GetDesignInfosForPlayer(this.PlayerId).ToList<DesignInfo>();
			foreach (DesignInfo current2 in list2)
			{
				if (current2.ID != this.PirateBaseDesignId)
				{
					game.GameDatabase.RemovePlayerDesign(current2.ID);
				}
			}
			List<PirateBaseInfo> pirateBases = game.GameDatabase.GetPirateBaseInfos().ToList<PirateBaseInfo>();
			foreach (PirateBaseInfo current3 in pirateBases)
			{
				if (game.GameDatabase.GetStationInfo(current3.BaseStationId) == null)
				{
					game.GameDatabase.RemoveEncounter(current3.Id);
				}
				else
				{
					current3.TurnsUntilAddShip--;
					if (current3.TurnsUntilAddShip <= 0)
					{
						current3.TurnsUntilAddShip = game.AssetDatabase.GlobalPiracyData.PiracyBaseTurnsPerUpdate;
						current3.NumShips = Math.Min(current3.NumShips + 1, game.AssetDatabase.GlobalPiracyData.PiracyTotalMaxShips);
					}
					game.GameDatabase.UpdatePirateBaseInfo(current3);
				}
			}
			Random safeRandom = App.GetSafeRandom();
			float num = game.AssetDatabase.GlobalPiracyData.PiracyBaseOdds;
			TradeResultsTable tradeResultsTable = game.GameDatabase.GetTradeResultsTable();
			List<PlayerInfo> list3 = game.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			List<int> list4 = new List<int>();
			foreach (PlayerInfo current4 in list3)
			{
				if (game.AssetDatabase.GetFaction(current4.FactionID).HasSlaves())
				{
					IEnumerable<int> playerColonySystemIDs = game.GameDatabase.GetPlayerColonySystemIDs(current4.ID);
					list4.AddRange(playerColonySystemIDs);
				}
			}
			foreach (KeyValuePair<int, TradeNode> current5 in tradeResultsTable.TradeNodes)
			{
				num = game.AssetDatabase.GlobalPiracyData.PiracyBaseOdds;
				int? p = game.GameDatabase.GetSystemOwningPlayer(current5.Key);
				if (p.HasValue && list3.Any((PlayerInfo x) => x.ID == p.Value) && current5.Value.Freighters != 0 && !list4.Contains(current5.Key))
				{
					Player playerObject = game.GetPlayerObject(p.Value);
					if (playerObject == null || !playerObject.IsAI())
					{
						List<FleetInfo> list5 = game.GameDatabase.GetFleetInfoBySystemID(current5.Key, FleetType.FL_DEFENSE).ToList<FleetInfo>();
						foreach (FleetInfo current6 in list5)
						{
							List<ShipInfo> list6 = game.GameDatabase.GetShipInfoByFleetID(current6.ID, false).ToList<ShipInfo>();
							foreach (ShipInfo current7 in list6)
							{
								if (current7.IsPoliceShip() && game.GameDatabase.GetShipSystemPosition(current7.ID).HasValue)
								{
									num += game.AssetDatabase.GlobalPiracyData.PiracyModPolice;
								}
							}
						}
						float num2 = game.AssetDatabase.GlobalPiracyData.PiracyModNoNavalBase;
						List<StationInfo> list7 = game.GameDatabase.GetStationForSystem(current5.Key).ToList<StationInfo>();
						foreach (StationInfo current8 in list7)
						{
							if (current8.DesignInfo.StationType == StationType.NAVAL)
							{
								num2 = Math.Min(num2, (float)current8.DesignInfo.StationLevel * game.AssetDatabase.GlobalPiracyData.PiracyModNavalBase);
							}
						}
						num += ((num2 < 0f) ? num2 : game.AssetDatabase.GlobalPiracyData.PiracyModNoNavalBase);
						Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(current5.Key);
						foreach (int current9 in list4)
						{
							if ((game.GameDatabase.GetStarSystemOrigin(current9) - starSystemOrigin).Length < game.AssetDatabase.GlobalPiracyData.PiracyMinZuulProximity)
							{
								num += game.AssetDatabase.GlobalPiracyData.PiracyModZuulProximity;
							}
						}
						if (game.GameDatabase.GetStarSystemInfo(current5.Key).IsOpen)
						{
							num += 0.02f;
						}
						int num3 = 0;
						if (pirateBases.Count > 0)
						{
							List<StarSystemInfo> source = game.GameDatabase.GetSystemsInRange(starSystemOrigin, (float)game.AssetDatabase.GlobalPiracyData.PiracyBaseRange).ToList<StarSystemInfo>();
							num3 = (
								from x in source
								where pirateBases.Any((PirateBaseInfo y) => y.SystemId == x.ID)
								select x).Count<StarSystemInfo>();
						}
						if (num3 > 0)
						{
							num += game.AssetDatabase.GlobalPiracyData.PiracyBaseMod;
						}
						num *= game.GameDatabase.GetStratModifier<float>(StratModifiers.ChanceOfPirates, p.Value);
						if (safeRandom.CoinToss((double)num) || Pirates.ForceEncounter)
						{
							int num4 = game.GameDatabase.GetStarSystemInfo(current5.Key).ProvinceID.HasValue ? 0 : 1;
							num4 += game.AssetDatabase.GlobalPiracyData.PiracyBaseShipBonus * num3;
							int numShips = safeRandom.Next(game.AssetDatabase.GlobalPiracyData.PiracyMinShips + num4, game.AssetDatabase.GlobalPiracyData.PiracyMaxShips + num4 + 1);
							this.SpawnPirateFleet(game, current5.Key, numShips);
						}
					}
				}
			}
		}
		public int SpawnPirateFleet(GameSession game, int targetSystem, int numShips)
		{
			Random random = new Random();
			int num = game.GameDatabase.InsertFleet(this.PlayerID, 0, targetSystem, 0, "Pirate Raiders", FleetType.FL_NORMAL);
			Dictionary<LogicalWeapon, int> dictionary = new Dictionary<LogicalWeapon, int>();
			List<PlayerInfo> list = game.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			List<PlayerInfo> list2 = new List<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				Faction faction = game.AssetDatabase.GetFaction(current.FactionID);
				if (faction.Name != "liir_zuul" && faction.Name != "hiver" && faction.Name != "loa")
				{
					list2.Add(current);
				}
				List<LogicalWeapon> list3 = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, current.ID).ToList<LogicalWeapon>();
				foreach (LogicalWeapon current2 in list3)
				{
					if (!dictionary.ContainsKey(current2))
					{
						dictionary.Add(current2, 1);
					}
					else
					{
						Dictionary<LogicalWeapon, int> dictionary2;
						LogicalWeapon key;
						(dictionary2 = dictionary)[key = current2] = dictionary2[key] + 1;
					}
				}
			}
			if (list2.Count > 0)
			{
				List<LogicalWeapon> list4 = new List<LogicalWeapon>();
				foreach (LogicalWeapon current3 in game.AssetDatabase.Weapons.ToList<LogicalWeapon>())
				{
					if (current3.TurretClasses.Count<LogicalTurretClass>() > 0 && WeaponEnums.IsBattleRider(current3.DefaultWeaponClass))
					{
						list4.Add(current3);
					}
				}
				foreach (KeyValuePair<LogicalWeapon, int> current4 in dictionary)
				{
					if (current4.Value > 1 && !list4.Contains(current4.Key))
					{
						list4.Add(current4.Key);
					}
				}
				for (int i = 0; i < numShips + 1; i++)
				{
					PlayerInfo playerInfo = random.Choose(list2);
					Faction faction2 = game.AssetDatabase.GetFaction(playerInfo.FactionID);
					DesignInfo designInfo = DesignLab.DesignShip(game, ShipClass.BattleRider, ShipRole.BOARDINGPOD, WeaponRole.PLANET_ATTACK, playerInfo.ID);
					DesignInfo designInfo2 = DesignLab.DesignShip(game, ShipClass.BattleRider, ShipRole.DRONE, WeaponRole.UNDEFINED, playerInfo.ID);
					designInfo.PlayerID = this.PlayerID;
					designInfo2.PlayerID = this.PlayerID;
					game.GameDatabase.InsertDesignByDesignInfo(designInfo);
					game.GameDatabase.InsertDesignByDesignInfo(designInfo2);
					DesignInfo designInfo4;
					if (i == 0)
					{
						DesignInfo designInfo3 = DesignLab.DesignShip(game, ShipClass.Cruiser, ShipRole.COMMAND, WeaponRole.UNDEFINED, playerInfo.ID);
						int arg_354_0 = playerInfo.ID;
						string arg_354_1 = "";
						string[] array = new string[3];
						array[0] = designInfo3.DesignSections.First((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Command).FilePath;
						array[1] = designInfo3.DesignSections.First((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission).FilePath;
						array[2] = designInfo3.DesignSections.First((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Engine).FilePath;
						designInfo4 = new DesignInfo(arg_354_0, arg_354_1, array);
					}
					else
					{
						List<ShipSectionAsset> availableSections = game.GetAvailableShipSections(playerInfo.ID).ToList<ShipSectionAsset>();
						List<string> list5 = Pirates.availableCommandSections.ToList<string>();
						List<string> list6 = Pirates.availableMissionSections.ToList<string>();
						List<string> list7 = Pirates.availableEngineSections.ToList<string>();
						for (int j = 0; j < list5.Count<string>(); j++)
						{
							list5[j] = string.Format("factions\\{0}\\sections\\{1}", faction2.Name, list5[j]);
						}
						for (int k = 0; k < list6.Count<string>(); k++)
						{
							list6[k] = string.Format("factions\\{0}\\sections\\{1}", faction2.Name, list6[k]);
						}
						for (int l = 0; l < list7.Count<string>(); l++)
						{
							list7[l] = string.Format("factions\\{0}\\sections\\{1}", faction2.Name, list7[l]);
						}
						list5.RemoveAll((string x) => !availableSections.Any((ShipSectionAsset y) => y.FileName == x));
						list6.RemoveAll((string x) => !availableSections.Any((ShipSectionAsset y) => y.FileName == x));
						list7.RemoveAll((string x) => !availableSections.Any((ShipSectionAsset y) => y.FileName == x));
						designInfo4 = new DesignInfo(playerInfo.ID, "", new string[]
						{
							random.Choose(list5),
							random.Choose(list6),
							random.Choose(list7)
						});
					}
					designInfo4.Name = DesignLab.GenerateDesignName(game.AssetDatabase, game.GameDatabase, null, designInfo4, DesignLab.NameGenerators.FactionRandom);
					DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, designInfo4);
					designInfo4 = DesignLab.AssignWeaponsToDesign(game, designInfo4, list4, this.PlayerId, WeaponRole.BRAWLER, null);
					designInfo4.PlayerID = this.PlayerID;
					int designID = game.GameDatabase.InsertDesignByDesignInfo(designInfo4);
					int carrierID = game.GameDatabase.InsertShip(num, designID, null, (ShipParams)0, null, 0);
					game.AddDefaultStartingRiders(num, designID, carrierID);
				}
			}
			return num;
		}
		public void AddEncounter(GameSession game, PlayerInfo targetPlayer)
		{
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, PirateBaseInfo pbi)
		{
			if (pbi == null)
			{
				return Matrix.Identity;
			}
			StationInfo stationInfo = app.GameDatabase.GetStationInfo(pbi.BaseStationId);
			OrbitalObjectInfo orbitalObjectInfo = app.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
			PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ParentID.Value);
			float num = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
			Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value);
			Vector3 vector = Vector3.Normalize(orbitalTransform.Position);
			return Matrix.CreateWorld(orbitalTransform.Position + vector * (num + 750f + 1000f), -vector, Vector3.UnitY);
		}
		public static Matrix GetSpawnTransform(App app, PirateBaseInfo pbi)
		{
			if (pbi == null)
			{
				return Matrix.Identity;
			}
			StationInfo stationInfo = app.GameDatabase.GetStationInfo(pbi.BaseStationId);
			OrbitalObjectInfo orbitalObjectInfo = app.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
			Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
			Matrix orbitalTransform2 = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value);
			Vector3 vector = Vector3.Normalize(orbitalTransform.Position - orbitalTransform2.Position);
			return Matrix.CreateWorld(orbitalTransform.Position + vector * 2000f, -vector, Vector3.UnitY);
		}
	}
}
