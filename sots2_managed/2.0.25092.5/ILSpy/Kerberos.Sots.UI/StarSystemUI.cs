using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class StarSystemUI
	{
		public const string UIBorderTitle = "title";
		public const string UISystemViewButton = "gameSystemButton";
		public const string UIStellarClassText = "partStellarClass";
		public const string UIMiniMapPanel = "partMiniSystem";
		public const string UIPlanetListControl = "gamePlanetList";
		public const string UIColonyDetails = "colonyControl";
		public const string UIPlanetDetails = "gamePlanetDetails";
		public const string UIMoonDetails = "gameMoonDetails";
		public const string UIGasGiantDetails = "gameGasGiantDetails";
		public const string UIStarDetails = "gameStarDetails";
		private static string UIMoraleEventTooltip;
		internal static string ShowMoraleEventToolTip(GameSession game, int colonyid, int x, int y)
		{
			if (StarSystemUI.UIMoraleEventTooltip == null)
			{
				StarSystemUI.UIMoraleEventTooltip = game.UI.CreatePanelFromTemplate("moraleeventspopup", null);
			}
			List<MoraleEventHistory> list = game.GameDatabase.GetMoraleHistoryEventsForColony(colonyid).ToList<MoraleEventHistory>();
			for (int i = 0; i < 5; i++)
			{
				string panelId = game.UI.Path(new string[]
				{
					StarSystemUI.UIMoraleEventTooltip,
					"event" + i.ToString()
				});
				if (i < list.Count || i == 0)
				{
					game.UI.SetVisible(panelId, true);
					if (list.Count != 0)
					{
						game.UI.SetPropertyString(panelId, "label", App.Localize("@UI_" + list[i].moraleEvent.ToString()));
						game.UI.SetPropertyString(panelId, "value", list[i].value.ToString());
					}
					else
					{
						game.UI.SetPropertyString(panelId, "label", App.Localize("@UI_NO_EVENTS"));
						game.UI.SetPropertyString(panelId, "value", "");
					}
				}
				else
				{
					game.UI.SetVisible(panelId, false);
				}
			}
			game.UI.AutoSize(StarSystemUI.UIMoraleEventTooltip);
			game.UI.ForceLayout(StarSystemUI.UIMoraleEventTooltip);
			game.UI.ShowTooltip(StarSystemUI.UIMoraleEventTooltip, (float)x, (float)y);
			return StarSystemUI.UIMoraleEventTooltip;
		}
		internal static void SyncStarDetailsControl(GameSession game, string panelId, int systemId)
		{
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			int num = game.GameDatabase.GetPlanetInfosOrbitingStar(starSystemInfo.ID).Count<PlanetInfo>();
			string propertyValue = starSystemInfo.Name.ToUpperInvariant();
			string stellarClass = starSystemInfo.StellarClass;
			string stellarActivity = new StellarClass(starSystemInfo.StellarClass).GetStellarActivity();
			string propertyValue2 = num.ToString();
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"title"
			}), "text", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partStellarClass"
			}), "value", stellarClass);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partSolarActivity"
			}), "value", stellarActivity);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partPlanetCount"
			}), "value", propertyValue2);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partStellarClass"
			}), "text", stellarClass);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partSolarActivity"
			}), "text", stellarActivity);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partPlanetCount"
			}), "text", propertyValue2);
			game.UI.AutoSize(panelId);
		}
		internal static void SyncStarDetailsStations(GameSession game, string panelId, int systemId, int playerId)
		{
			game.GameDatabase.GetStarSystemInfo(systemId);
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(playerId).FactionID);
			if (faction.CanUseGate())
			{
				StationInfo hiverGateForSystem = game.GameDatabase.GetHiverGateForSystem(systemId, playerId);
				if (hiverGateForSystem != null)
				{
					game.UI.AddItem(game.UI.Path(new string[]
					{
						panelId,
						"stations"
					}), "", 10, "");
					string itemGlobalID = game.UI.GetItemGlobalID(game.UI.Path(new string[]
					{
						panelId,
						"stations"
					}), "", 10, "");
					game.UI.SetPropertyString(game.UI.Path(new string[]
					{
						itemGlobalID,
						"icon"
					}), "sprite", "systemTagGateStation");
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						itemGlobalID,
						"ring"
					}), false);
				}
				else
				{
					if (game.GameDatabase.SystemHasGate(systemId, playerId))
					{
						game.UI.AddItem(game.UI.Path(new string[]
						{
							panelId,
							"stations"
						}), "", 11, "");
						string itemGlobalID2 = game.UI.GetItemGlobalID(game.UI.Path(new string[]
						{
							panelId,
							"stations"
						}), "", 11, "");
						game.UI.SetPropertyString(game.UI.Path(new string[]
						{
							itemGlobalID2,
							"icon"
						}), "sprite", "systemTagGate");
						game.UI.SetVisible(game.UI.Path(new string[]
						{
							itemGlobalID2,
							"ring"
						}), false);
					}
				}
			}
			List<StationInfo> source = game.GameDatabase.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>();
			bool flag = source.Any((StationInfo x) => x.DesignInfo.StationType == StationType.NAVAL);
			bool flag2 = source.Any((StationInfo x) => x.DesignInfo.StationType == StationType.SCIENCE);
			bool flag3 = source.Any((StationInfo x) => x.DesignInfo.StationType == StationType.DIPLOMATIC);
			bool flag4 = source.Any((StationInfo x) => x.DesignInfo.StationType == StationType.CIVILIAN);
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			int numberMaxStationsSupportedBySystem = game.GameDatabase.GetNumberMaxStationsSupportedBySystem(game, systemId, playerId);
			if (numberMaxStationsSupportedBySystem > 0)
			{
				for (int i = 0; i < numberMaxStationsSupportedBySystem; i++)
				{
					if (flag && !flag5)
					{
						flag5 = true;
					}
					else
					{
						if (flag2 && !flag6)
						{
							flag6 = true;
						}
						else
						{
							if (flag3 && !flag7)
							{
								flag7 = true;
							}
							else
							{
								if (flag4 && !flag8)
								{
									flag8 = true;
								}
								else
								{
									game.UI.AddItem(game.UI.Path(new string[]
									{
										panelId,
										"stations"
									}), "", 100 + i, "");
									string itemGlobalID3 = game.UI.GetItemGlobalID(game.UI.Path(new string[]
									{
										panelId,
										"stations"
									}), "", 100 + i, "");
									game.UI.SetVisible(game.UI.Path(new string[]
									{
										itemGlobalID3,
										"icon"
									}), false);
									game.UI.SetVisible(game.UI.Path(new string[]
									{
										itemGlobalID3,
										"ring"
									}), true);
								}
							}
						}
					}
				}
			}
			if (flag4)
			{
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelId,
					"stations"
				}), "", 4, "");
				string itemGlobalID4 = game.UI.GetItemGlobalID(game.UI.Path(new string[]
				{
					panelId,
					"stations"
				}), "", 4, "");
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID4,
					"icon"
				}), "sprite", "systemTagCivilian");
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					itemGlobalID4,
					"ring"
				}), flag8);
			}
			if (flag3)
			{
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelId,
					"stations"
				}), "", 3, "");
				string itemGlobalID5 = game.UI.GetItemGlobalID(game.UI.Path(new string[]
				{
					panelId,
					"stations"
				}), "", 3, "");
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID5,
					"icon"
				}), "sprite", "systemTagDiplomatic");
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					itemGlobalID5,
					"ring"
				}), flag7);
			}
			if (flag2)
			{
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelId,
					"stations"
				}), "", 2, "");
				string itemGlobalID6 = game.UI.GetItemGlobalID(game.UI.Path(new string[]
				{
					panelId,
					"stations"
				}), "", 2, "");
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID6,
					"icon"
				}), "sprite", "systemTagScience");
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					itemGlobalID6,
					"ring"
				}), flag6);
			}
			if (flag)
			{
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelId,
					"stations"
				}), "", 1, "");
				string itemGlobalID7 = game.UI.GetItemGlobalID(game.UI.Path(new string[]
				{
					panelId,
					"stations"
				}), "", 1, "");
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID7,
					"icon"
				}), "sprite", "AnchorTag");
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					itemGlobalID7,
					"ring"
				}), flag5);
			}
		}
		internal static void SyncGasGiantDetailsControl(GameSession game, string panelId, int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			int num = game.GameDatabase.CountMoons(orbitalId);
			string propertyValue = orbitalObjectInfo.Name.ToUpperInvariant();
			string sizeString = StarSystemDetailsUI.GetSizeString(planetInfo.Size);
			string propertyValue2 = App.Localize("@UI_GAS_GIANT_TYPE");
			string propertyValue3 = num.ToString();
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"title"
			}), "text", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partSize"
			}), "value", sizeString);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partType"
			}), "value", propertyValue2);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partMoonCount"
			}), "value", propertyValue3);
			game.UI.AutoSize(panelId);
		}
		internal static void SyncMoonDetailsControl(GameSession game, string panelId, int orbitalId)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			int resources = planetInfo.Resources;
			string propertyValue = orbitalObjectInfo.Name.ToUpperInvariant();
			string sizeString = StarSystemDetailsUI.GetSizeString(planetInfo.Size);
			string propertyValue2 = resources.ToString("N0");
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"title"
			}), "text", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partSize"
			}), "value", sizeString);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partResources"
			}), "value", propertyValue2);
		}
		internal static void SyncAsteroidDetailsControl(GameSession game, string panelId, int orbitalId)
		{
			LargeAsteroidInfo largeAsteroidInfo = game.GameDatabase.GetLargeAsteroidInfo(orbitalId);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			int resources = largeAsteroidInfo.Resources;
			string propertyValue = orbitalObjectInfo.Name.ToUpperInvariant();
			string propertyValue2 = resources.ToString("N0");
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"title"
			}), "text", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partSize"
			}), "value", "");
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partResources"
			}), "value", propertyValue2);
		}
		internal static void SyncPlanetDetailsControl(GameSession game, string panelId, int orbitalId)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			float planetHazardRating = game.GameDatabase.GetPlanetHazardRating(game.LocalPlayer.ID, orbitalId, false);
			float nonAbsolutePlanetHazardRating = game.GameDatabase.GetNonAbsolutePlanetHazardRating(game.LocalPlayer.ID, orbitalId, false);
			int resources = planetInfo.Resources;
			int biosphere = planetInfo.Biosphere;
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
			double civilianPopulation = game.GameDatabase.GetCivilianPopulation(orbitalId, (colonyInfoForPlanet != null) ? game.GameDatabase.GetPlayerInfo(colonyInfoForPlanet.PlayerID).FactionID : 0, colonyInfoForPlanet != null && game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(colonyInfoForPlanet.PlayerID)).HasSlaves());
			float infrastructure = planetInfo.Infrastructure;
			int num = game.GameDatabase.CountMoons(orbitalId);
			int num2 = (int)Colony.EstimateColonyDevelopmentCost(game, orbitalId, game.LocalPlayer.ID);
			string propertyValue = orbitalObjectInfo.Name.ToUpperInvariant();
			string propertyValue2 = num.ToString();
			string propertyValue3 = biosphere.ToString("N0");
			string propertyValue4 = resources.ToString("N0");
			string propertyValue5 = civilianPopulation.ToString("N0");
			string propertyValue6 = App.Localize("@UI_PLANET_TYPE_" + planetInfo.Type.ToUpperInvariant());
			string infraString = StarSystemDetailsUI.GetInfraString(infrastructure);
			string hazardString = StarSystemDetailsUI.GetHazardString(planetHazardRating);
			string propertyValue7 = num2.ToString("N0");
			string sizeString = StarSystemDetailsUI.GetSizeString(planetInfo.Size);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"title"
			}), "text", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partSize"
			}), "value", sizeString);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partResources"
			}), "value", propertyValue4);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partBiosphere"
			}), "value", propertyValue3);
			if (num2 == 0 || !game.GameDatabase.CanColonizePlanet(game.LocalPlayer.ID, orbitalId, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)))
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partDevCost"
				}), false);
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partDevCost"
				}), true);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelId,
					"partDevCost"
				}), "value", propertyValue7);
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partType"
			}), "value", propertyValue6);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partCivPop"
			}), "value", propertyValue5);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partInfra"
			}), "value", infraString);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partMoonCount"
			}), "value", propertyValue2);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partHazard"
			}), "value", hazardString);
			game.UI.SetSliderValue(game.UI.Path(new string[]
			{
				panelId,
				"partClimateSlider"
			}), (int)nonAbsolutePlanetHazardRating);
			if (game.LocalPlayer.Faction.Name == "loa")
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partClimateSlider"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"loaSuitability"
				}), true);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partHazard"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"parthazardLevel"
				}), false);
				game.UI.SetText(game.UI.Path(new string[]
				{
					panelId,
					"loaSuitability"
				}), "Growth Potential: " + Colony.GetLoaGrowthPotential(game, planetInfo.ID, game.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID, game.LocalPlayer.ID).ToString("0.0%"));
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"loaSuitability"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partClimateSlider"
				}), true);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partHazard"
				}), true);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"parthazardLevel"
				}), true);
			}
			Vector3 value = new Vector3(255f, 0f, 0f);
			Vector3 value2 = new Vector3(0f, 255f, 0f);
			Vector3 value3 = new Vector3(255f, 255f, 255f);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelId,
				"partResources"
			}), "color", value3);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelId,
				"partBiosphere"
			}), "color", value3);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelId,
				"partCivPop"
			}), "color", value3);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelId,
				"partInfra"
			}), "color", value3);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelId,
				"partHazard"
			}), "color", value3);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelId,
				"rebellionActive"
			}), colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == game.LocalPlayer.ID && colonyInfoForPlanet.RebellionType != RebellionType.None);
			if (colonyInfoForPlanet != null)
			{
				ColonyHistoryData lastColonyHistoryForColony = game.GameDatabase.GetLastColonyHistoryForColony(colonyInfoForPlanet.ID);
				if (lastColonyHistoryForColony != null)
				{
					if (lastColonyHistoryForColony.resources < resources)
					{
						game.UI.SetPropertyColor(game.UI.Path(new string[]
						{
							panelId,
							"partResources"
						}), "color", value2);
					}
					else
					{
						if (lastColonyHistoryForColony.resources > resources)
						{
							game.UI.SetPropertyColor(game.UI.Path(new string[]
							{
								panelId,
								"partResources"
							}), "color", value);
						}
					}
					if (lastColonyHistoryForColony.biosphere < biosphere)
					{
						game.UI.SetPropertyColor(game.UI.Path(new string[]
						{
							panelId,
							"partBiosphere"
						}), "color", value2);
					}
					else
					{
						if (lastColonyHistoryForColony.biosphere > biosphere)
						{
							game.UI.SetPropertyColor(game.UI.Path(new string[]
							{
								panelId,
								"partBiosphere"
							}), "color", value);
						}
					}
					if (lastColonyHistoryForColony.civ_pop < civilianPopulation)
					{
						game.UI.SetPropertyColor(game.UI.Path(new string[]
						{
							panelId,
							"partCivPop"
						}), "color", value2);
					}
					else
					{
						if (lastColonyHistoryForColony.civ_pop > civilianPopulation)
						{
							game.UI.SetPropertyColor(game.UI.Path(new string[]
							{
								panelId,
								"partCivPop"
							}), "color", value);
						}
					}
					if (lastColonyHistoryForColony.infrastructure < infrastructure)
					{
						game.UI.SetPropertyColor(game.UI.Path(new string[]
						{
							panelId,
							"partInfra"
						}), "color", value2);
					}
					else
					{
						if (lastColonyHistoryForColony.infrastructure < infrastructure)
						{
							game.UI.SetPropertyColor(game.UI.Path(new string[]
							{
								panelId,
								"partInfra"
							}), "color", value);
						}
					}
					if (lastColonyHistoryForColony.hazard > planetHazardRating)
					{
						game.UI.SetPropertyColor(game.UI.Path(new string[]
						{
							panelId,
							"partHazard"
						}), "color", value2);
						return;
					}
					if (lastColonyHistoryForColony.hazard < planetHazardRating)
					{
						game.UI.SetPropertyColor(game.UI.Path(new string[]
						{
							panelId,
							"partHazard"
						}), "color", value);
					}
				}
			}
		}
		public static void SyncPlanetCardPanelColor(App game, string panelName, Vector3 color)
		{
			List<string> list = new List<string>
			{
				"BOL1",
				"BOL2",
				"BOL3",
				"BOL4",
				"BOL5",
				"BOL6",
				"BOL7",
				"BOL8",
				"L_Cap",
				"R_Cap",
				"PC_OWNER_S"
			};
			foreach (string current in list)
			{
				game.UI.SetPropertyColorNormalized(game.UI.Path(new string[]
				{
					panelName,
					current
				}), "color", color);
			}
		}
		internal static void SyncPlanetDetailsControlNew(GameSession game, string panelId, int orbitalId)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			float planetHazardRating = game.GameDatabase.GetPlanetHazardRating(game.LocalPlayer.ID, orbitalId, false);
			float nonAbsolutePlanetHazardRating = game.GameDatabase.GetNonAbsolutePlanetHazardRating(game.LocalPlayer.ID, orbitalId, false);
			int resources = planetInfo.Resources;
			int biosphere = planetInfo.Biosphere;
			double civilianPopulation = game.GameDatabase.GetCivilianPopulation(orbitalId, 0, false);
			float infrastructure = planetInfo.Infrastructure;
			int num = game.GameDatabase.CountMoons(orbitalId);
			int num2 = (int)Colony.EstimateColonyDevelopmentCost(game, orbitalId, game.LocalPlayer.ID);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalId);
			string propertyValue = orbitalObjectInfo.Name.ToUpperInvariant();
			string propertyValue2 = num.ToString();
			string propertyValue3 = biosphere.ToString("N0");
			string propertyValue4 = resources.ToString("N0");
			string propertyValue5 = civilianPopulation.ToString("N0");
			string propertyValue6 = App.Localize("@UI_PLANET_TYPE_" + planetInfo.Type.ToUpperInvariant());
			string infraString = StarSystemDetailsUI.GetInfraString(infrastructure);
			string hazardString = StarSystemDetailsUI.GetHazardString(planetHazardRating);
			string propertyValue7 = num2.ToString("N0");
			string sizeString = StarSystemDetailsUI.GetSizeString(planetInfo.Size);
			string propertyValue8 = "0";
			string propertyValue9 = "0";
			string propertyValue10 = "0";
			string propertyValue11 = "0";
			if (colonyInfoForPlanet != null)
			{
				propertyValue8 = colonyInfoForPlanet.ImperialPop.ToString("N0");
				propertyValue10 = colonyInfoForPlanet.EconomyRating.ToString("N0");
				propertyValue9 = Colony.GetIndustrialOutput(game, colonyInfoForPlanet, planetInfo).ToString("N0");
				propertyValue11 = Colony.GetColonySupportCost(game.AssetDatabase, game.GameDatabase, colonyInfoForPlanet).ToString("N0");
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"title"
			}), "text", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partSize"
			}), "text", sizeString);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partResources"
			}), "text", propertyValue4);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partBiosphere"
			}), "text", propertyValue3);
			if (game.GameDatabase.IsHazardousPlanet(game.LocalPlayer.ID, orbitalId, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)))
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"Unsuitable"
				}), true);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"Suitable"
				}), false);
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"Unsuitable"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"Suitable"
				}), true);
			}
			if (!game.GameDatabase.CanColonizePlanet(game.LocalPlayer.ID, orbitalId, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)))
			{
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelId,
					"partDevCost"
				}), "text", "Prohibitive");
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partDevCost"
				}), true);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelId,
					"partDevCost"
				}), "text", propertyValue7);
			}
			if (colonyInfoForPlanet != null)
			{
				if (colonyInfoForPlanet.PlayerID == game.LocalPlayer.ID)
				{
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						panelId,
						"devText"
					}), false);
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						panelId,
						"incomeText"
					}), true);
					game.UI.SetPropertyString(game.UI.Path(new string[]
					{
						panelId,
						"partDevCost"
					}), "text", (Colony.GetTaxRevenue(game.App, colonyInfoForPlanet) - Colony.GetColonySupportCost(game.AssetDatabase, game.GameDatabase, colonyInfoForPlanet)).ToString("N0"));
				}
				else
				{
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						panelId,
						"devText"
					}), true);
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						panelId,
						"incomeText"
					}), false);
					game.UI.SetPropertyString(game.UI.Path(new string[]
					{
						panelId,
						"partDevCost"
					}), "text", "Inhabited");
				}
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colonyInfoForPlanet.PlayerID);
				StarSystemUI.SyncPlanetCardPanelColor(game.App, panelId + ".itemDetails.ownerPC", playerInfo.PrimaryColor);
				StarSystemUI.SyncPlanetCardPanelColor(game.App, panelId + ".expanded.ownerPC", playerInfo.PrimaryColor);
			}
			else
			{
				game.UI.SetVisible(panelId + ".itemDetails.ownerPC", false);
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partType"
			}), "text", propertyValue6);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partCivPop"
			}), "text", propertyValue5);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partImpPop"
			}), "text", propertyValue8);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partInfra"
			}), "text", infraString);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partMoonCount"
			}), "text", propertyValue2);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partHazard"
			}), "text", hazardString);
			game.UI.SetSliderValue(game.UI.Path(new string[]
			{
				panelId,
				"partClimateSlider"
			}), (int)nonAbsolutePlanetHazardRating);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partEconRating"
			}), "text", propertyValue10);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partLifeSupCost"
			}), "text", propertyValue11);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelId,
				"partIndustrial"
			}), "text", propertyValue9);
			if (game.LocalPlayer.Faction.Name == "loa")
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partClimateSlider"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"loaSuitability"
				}), true);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"partHazard"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelId,
					"parthazardLevel"
				}), false);
				game.UI.SetText(game.UI.Path(new string[]
				{
					panelId,
					"loaSuitability"
				}), "Growth Potential: " + Colony.GetLoaGrowthPotential(game, planetInfo.ID, game.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID, game.LocalPlayer.ID).ToString("0.0%"));
				return;
			}
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelId,
				"loaSuitability"
			}), false);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelId,
				"partClimateSlider"
			}), true);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelId,
				"partHazard"
			}), true);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelId,
				"parthazardLevel"
			}), true);
		}
		private static string FormatName(string name, int colonyID, int widgetID)
		{
			return string.Concat(new string[]
			{
				"__",
				name,
				"|",
				widgetID.ToString(),
				"|",
				colonyID.ToString()
			});
		}
		internal static void ClearColonyDetailsControl(GameSession sim, string panelId)
		{
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partCivPop"
			}), "label", "");
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"incomeText"
			}), false);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"devText"
			}), true);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partStage"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partOverharvestSlider",
				"right_label"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"civEqualibText"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partCivPop"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partImpPop"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partDevCost"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partEconRating"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partLifeSupCost"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partIndustrial"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partTerraSlider",
				"right_label"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partInfraSlider",
				"right_label"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partShipConSlider",
				"right_label"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partOverDevelopment",
				"right_label"
			}), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partWorkRate",
				"right_label"
			}), "text", "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"gameMorale_human"
			}), "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"gameMorale_zuul"
			}), "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"gameMorale_liir_zuul"
			}), "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"gameMorale_tarkas"
			}), "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"gameMorale_hiver"
			}), "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"gameMorale_morrigi"
			}), "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"gameMorale_loa"
			}), "");
		}
		internal static void SyncColonyDetailsControlNew(GameSession sim, string panelId, int colonyId, int widgetId, string sliderName)
		{
			ColonyInfo colonyInfo = sim.GameDatabase.GetColonyInfo(colonyId);
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
			double civilianPopulation = sim.GameDatabase.GetCivilianPopulation(colonyInfo.OrbitalObjectID, 0, false);
			float economyRating = colonyInfo.EconomyRating;
			int num = (int)Colony.GetColonySupportCost(sim.AssetDatabase, sim.GameDatabase, colonyInfo);
			int num2 = (int)Colony.GetTaxRevenue(sim.App, colonyInfo) - num;
			int num3 = (int)Colony.GetIndustrialOutput(sim, colonyInfo, planetInfo);
			float num4 = Colony.GetBiosphereDelta(sim, colonyInfo, planetInfo, 0.0);
			float num5 = (float)Colony.GetInfrastructureDelta(sim, colonyInfo, planetInfo) * 100f;
			float terraformingDelta = Colony.GetTerraformingDelta(sim, colonyInfo, planetInfo, 0.0);
			float shipConstResources = Colony.GetShipConstResources(sim, colonyInfo, planetInfo);
			float num6 = colonyInfo.OverdevelopProgress / Colony.GetOverdevelopmentTarget(sim, planetInfo) * 100f;
			int num7 = (colonyInfo.OverdevelopRate == 0f) ? -1 : ((int)Math.Ceiling((double)((Colony.GetOverdevelopmentTarget(sim, planetInfo) - colonyInfo.OverdevelopProgress) / ((float)num3 * colonyInfo.OverdevelopRate))));
			bool flag = sim.GameDatabase.GetPlanetHazardRating(sim.LocalPlayer.ID, colonyInfo.OrbitalObjectID, false) != 0f;
			bool flag2 = planetInfo.Infrastructure < 1f;
			bool flag3 = true;
			bool value = Colony.CanBeOverdeveloped(sim.AssetDatabase, sim.GameDatabase, colonyInfo, planetInfo);
			int num8 = (int)Colony.CalcOverharvestRate(sim.AssetDatabase, sim.GameDatabase, colonyInfo, planetInfo);
			bool flag4 = sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID)).HasSlaves();
			string propertyValue = num8.ToString("N0");
			string propertyValue2 = civilianPopulation.ToString("N0");
			string propertyValue3 = colonyInfo.ImperialPop.ToString("N0");
			string propertyValue4 = num2.ToString("N0");
			string propertyValue5 = ((int)(economyRating * 100f)).ToString("N0");
			string propertyValue6 = num.ToString("N0");
			string propertyValue7 = num3.ToString("N0");
			string text = terraformingDelta.ToString("F2");
			int value2 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.TerraRate);
			string text2 = num5.ToString("F2");
			int value3 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.InfraRate);
			string text3 = shipConstResources.ToString("N0");
			int value4 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.ShipConRate);
			string propertyValue8 = string.Format(App.Localize("@UI_GAMECOLONYDETAILS_WORKRATECHANGE"), Colony.GetSlaveIndustrialOutput(sim, colonyInfo).ToString("N0"), Colony.GetKilledSlavePopulation(sim, colonyInfo, sim.GameDatabase.GetSlavePopulation(planetInfo.ID, sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID))).ToString("N0"));
			int value5 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.SlaveWorkRate);
			int value6 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.OverharvestRate);
			string propertyValue9 = string.Format("{0:0.00}% ({1} {2})", num6, (num7 == -1) ? "∞" : num7.ToString(), (num7 == 1) ? App.Localize("@UI_GENERAL_TURN") : App.Localize("@UI_GENERAL_TURNS"));
			int value7 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.OverdevelopRate);
			int value8 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.TradeRate);
			int num9 = (int)(colonyInfo.CivilianWeight * 100f);
			string propertyValue10 = string.Format("{0:0}% ({1} {2})", num9, (num4 > 0f) ? ("+" + num4.ToString()) : num4.ToString(), App.Localize("@UI_GAMECOLONYDETAILS_BIOSPHERE"));
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"pnlSlaves"
			}), flag4);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"pnlMorale"
			}), !flag4);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partCivSlider"
			}), !flag4);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partHardenedStructure"
			}), sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowHardenedStructures, colonyInfo.PlayerID));
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"lblHardenedStructure"
			}), sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowHardenedStructures, colonyInfo.PlayerID));
			sim.UI.SetChecked(sim.UI.Path(new string[]
			{
				panelId,
				"partHardenedStructure"
			}), colonyInfo.isHardenedStructures);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				StarSystemUI.FormatName("partTerraSlider", colonyId, widgetId)
			}), flag);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				StarSystemUI.FormatName("partInfraSlider", colonyId, widgetId)
			}), flag2);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				StarSystemUI.FormatName("partShipConSlider", colonyId, widgetId)
			}), flag3);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				StarSystemUI.FormatName("partOverDevelopment", colonyId, widgetId)
			}), value);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partStage"
			}), "text", App.Localize("@UI_COLONYSTAGE_" + colonyInfo.CurrentStage.ToString().ToUpper()));
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partOverharvestSlider",
				"right_label"
			}), "text", propertyValue);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"civEqualibText"
			}), "text", propertyValue10);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partCivPop"
			}), "text", propertyValue2);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partImpPop"
			}), "text", propertyValue3);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partDevCost"
			}), "text", propertyValue4);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partEconRating"
			}), "text", propertyValue5);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partLifeSupCost"
			}), "text", propertyValue6);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partIndustrial"
			}), "text", propertyValue7);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partTerraSlider",
				"right_label"
			}), "text", text);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partInfraSlider",
				"right_label"
			}), "text", text2);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partShipConSlider",
				"right_label"
			}), "text", text3);
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"shipconstructionValue"
			}), flag3 ? text3 : "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"infrastructureValue"
			}), flag2 ? text2 : "");
			sim.UI.SetText(sim.UI.Path(new string[]
			{
				panelId,
				"terraformingValue"
			}), flag ? text : "");
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partOverDevelopment",
				"right_label"
			}), "text", propertyValue9);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partWorkRate",
				"right_label"
			}), "text", propertyValue8);
			if (sliderName != StarSystemUI.FormatName("partTradeSlider", colonyId, widgetId))
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partTradeSlider", colonyId, widgetId)
				}), value8);
				sim.UI.ClearSliderNotches(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partTradeSlider", colonyId, widgetId)
				}));
				List<double> tradeRatesForWholeExportsForColony = sim.GetTradeRatesForWholeExportsForColony(colonyInfo.ID);
				foreach (double num10 in tradeRatesForWholeExportsForColony)
				{
					sim.UI.AddSliderNotch(sim.UI.Path(new string[]
					{
						panelId,
						StarSystemUI.FormatName("partTradeSlider", colonyId, widgetId)
					}), (int)Math.Ceiling(num10 * 100.0));
				}
			}
			if (sliderName != StarSystemUI.FormatName("partCivSlider", colonyId, widgetId))
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partCivSlider", colonyId, widgetId)
				}), num9);
			}
			if (sliderName != StarSystemUI.FormatName("partTerraSlider", colonyId, widgetId))
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partTerraSlider", colonyId, widgetId)
				}), value2);
			}
			if (sliderName != StarSystemUI.FormatName("partInfraSlider", colonyId, widgetId))
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partInfraSlider", colonyId, widgetId)
				}), value3);
			}
			if (sliderName != StarSystemUI.FormatName("partShipConSlider", colonyId, widgetId))
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partShipConSlider", colonyId, widgetId)
				}), value4);
			}
			if (sliderName != StarSystemUI.FormatName("partOverDevelopment", colonyId, widgetId))
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partOverDevelopment", colonyId, widgetId)
				}), value7);
			}
			if (sliderName != "partOverharvestSlider")
			{
				sim.UI.SetSliderRange(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partOverharvestSlider", colonyId, widgetId)
				}), (int)(sim.GameDatabase.GetStratModifier<float>(StratModifiers.MinOverharvestRate, colonyInfo.PlayerID) * 100f), 100);
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					StarSystemUI.FormatName("partOverharvestSlider", colonyId, widgetId)
				}), value6);
			}
			if (sliderName != "partWorkRate")
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partWorkRate"
				}), value5);
			}
			foreach (Faction current in sim.AssetDatabase.Factions)
			{
				if (current.IsPlayable)
				{
					sim.UI.SetText(sim.UI.Path(new string[]
					{
						panelId,
						"gameMorale_" + current.Name
					}), "--");
				}
			}
			foreach (ColonyFactionInfo current2 in sim.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID))
			{
				string factionName = sim.GameDatabase.GetFactionName(current2.FactionID);
				sim.UI.SetText(sim.UI.Path(new string[]
				{
					panelId,
					"gameMorale_" + factionName
				}), string.Format("{0:0}", current2.Morale));
			}
			sim.UI.AutoSize(sim.UI.Path(new string[]
			{
				panelId,
				"partConstructionSliders",
				"ColonyModifiers"
			}));
			sim.UI.AutoSize(sim.UI.Path(new string[]
			{
				panelId,
				"partConstructionSliders"
			}));
			sim.UI.AutoSize(sim.UI.Path(new string[]
			{
				panelId,
				"pnlMoraleStats"
			}));
			sim.UI.SetPostMouseOverEvents("moraleeventtooltipover", true);
			sim.UI.SetPostMouseOverEvents(sim.UI.Path(new string[]
			{
				panelId,
				"MoraleRow"
			}), true);
			ColonyFactionInfo[] factions = colonyInfo.Factions;
			for (int i = 0; i < factions.Length; i++)
			{
				ColonyFactionInfo colonyFactionInfo = factions[i];
				string itemGlobalID = sim.UI.GetItemGlobalID(sim.UI.Path(new string[]
				{
					panelId,
					"MoraleRow"
				}), "", colonyFactionInfo.FactionID, "");
				Faction faction = sim.AssetDatabase.GetFaction(colonyFactionInfo.FactionID);
				sim.UI.SetPropertyString(sim.UI.Path(new string[]
				{
					itemGlobalID,
					"factionicon"
				}), "sprite", "logo_" + faction.Name.ToLower());
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					itemGlobalID,
					string.Concat(new object[]
					{
						"__partPopSlider|",
						widgetId.ToString(),
						"|",
						colonyInfo.ID.ToString(),
						"|",
						colonyFactionInfo.FactionID
					})
				}), (int)(colonyFactionInfo.CivPopWeight * 100f));
				sim.UI.SetText(sim.UI.Path(new string[]
				{
					itemGlobalID,
					"gameMorale_human"
				}), colonyFactionInfo.Morale.ToString());
				double num11 = (colonyInfo.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld) ? (Colony.GetMaxCivilianPop(sim.GameDatabase, planetInfo) * (double)sim.AssetDatabase.GemWorldCivMaxBonus) : Colony.GetMaxCivilianPop(sim.GameDatabase, planetInfo);
				num11 *= (double)colonyInfo.CivilianWeight;
				num11 *= (double)colonyFactionInfo.CivPopWeight;
				num11 *= (double)sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID)).GetImmigrationPopBonusValueForFaction(sim.AssetDatabase.GetFaction(colonyFactionInfo.FactionID));
				sim.UI.SetText(sim.UI.Path(new string[]
				{
					itemGlobalID,
					"popRatio"
				}), (colonyFactionInfo.CivilianPop / 1000000.0).ToString("0.0") + "M / " + (num11 / 1000000.0).ToString("0.0") + "M");
			}
		}
		public static void SyncPanelColor(App game, string panelName, Vector3 color)
		{
			List<string> list = new List<string>
			{
				"TLC",
				"TRC",
				"BLC",
				"BRC",
				"TC",
				"BC",
				"FILL"
			};
			foreach (string current in list)
			{
				game.UI.SetPropertyColorNormalized(game.UI.Path(new string[]
				{
					panelName,
					current
				}), "color", color);
			}
		}
		internal static void SyncColonyDetailsControl(GameSession sim, string panelId, int colonyId, string sliderName = "")
		{
			ColonyInfo colonyInfo = sim.GameDatabase.GetColonyInfo(colonyId);
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID);
			StarSystemUI.SyncPanelColor(sim.App, sim.UI.Path(new string[]
			{
				panelId,
				"pnlColonyStats"
			}), playerInfo.PrimaryColor);
			StarSystemUI.SyncPanelColor(sim.App, sim.UI.Path(new string[]
			{
				panelId,
				"pnlMoraleStats"
			}), playerInfo.PrimaryColor);
			StarSystemUI.SyncPanelColor(sim.App, sim.UI.Path(new string[]
			{
				panelId,
				"partConstructionSliders"
			}), playerInfo.PrimaryColor);
			Faction faction = sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID));
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
			double civilianPopulation = sim.GameDatabase.GetCivilianPopulation(colonyInfo.OrbitalObjectID, faction.ID, faction.HasSlaves());
			double slavePopulation = sim.GameDatabase.GetSlavePopulation(colonyInfo.OrbitalObjectID, faction.ID);
			float economyRating = colonyInfo.EconomyRating;
			int num = (int)Colony.GetColonySupportCost(sim.AssetDatabase, sim.GameDatabase, colonyInfo);
			int num2 = (int)Colony.GetTaxRevenue(sim.App, colonyInfo) - num;
			int num3 = (int)Colony.GetIndustrialOutput(sim, colonyInfo, planetInfo);
			float num4 = Colony.GetBiosphereDelta(sim, colonyInfo, planetInfo, 0.0);
			float num5 = (float)Colony.GetInfrastructureDelta(sim, colonyInfo, planetInfo) * 100f;
			float terraformingDelta = Colony.GetTerraformingDelta(sim, colonyInfo, planetInfo, 0.0);
			float shipConstResources = Colony.GetShipConstResources(sim, colonyInfo, planetInfo);
			float num6 = colonyInfo.OverdevelopProgress / Colony.GetOverdevelopmentTarget(sim, planetInfo) * 100f;
			int num7 = (colonyInfo.OverdevelopRate == 0f) ? -1 : ((int)Math.Ceiling((double)((Colony.GetOverdevelopmentTarget(sim, planetInfo) - colonyInfo.OverdevelopProgress) / ((float)num3 * colonyInfo.OverdevelopRate))));
			bool value = sim.GameDatabase.GetPlanetHazardRating(sim.LocalPlayer.ID, colonyInfo.OrbitalObjectID, false) != 0f;
			bool value2 = planetInfo.Infrastructure < 1f;
			bool value3 = true;
			bool value4 = Colony.CanBeOverdeveloped(sim.AssetDatabase, sim.GameDatabase, colonyInfo, planetInfo);
			int num8 = (int)Colony.CalcOverharvestRate(sim.AssetDatabase, sim.GameDatabase, colonyInfo, planetInfo);
			bool flag = sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID)).HasSlaves();
			bool value5 = colonyInfo.PlayerID == sim.LocalPlayer.ID;
			string propertyValue = num8.ToString("N0");
			string propertyValue2 = civilianPopulation.ToString("N0");
			string propertyValue3 = slavePopulation.ToString("N0");
			string propertyValue4 = colonyInfo.ImperialPop.ToString("N0");
			string propertyValue5 = num2.ToString("N0");
			string propertyValue6 = ((int)(economyRating * 100f)).ToString("N0");
			string propertyValue7 = num.ToString("N0");
			string propertyValue8 = num3.ToString("N0");
			string propertyValue9 = terraformingDelta.ToString("F2");
			int value6 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.TerraRate);
			string propertyValue10 = num5.ToString("F2");
			int value7 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.InfraRate);
			string propertyValue11 = shipConstResources.ToString("N0");
			int value8 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.ShipConRate);
			string propertyValue12 = string.Format(App.Localize("@UI_GAMECOLONYDETAILS_WORKRATECHANGE"), Colony.GetSlaveIndustrialOutput(sim, colonyInfo).ToString("N0"), Colony.GetKilledSlavePopulation(sim, colonyInfo, sim.GameDatabase.GetSlavePopulation(planetInfo.ID, sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID))).ToString("N0"));
			int value9 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.SlaveWorkRate);
			int value10 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.OverharvestRate);
			string propertyValue13 = string.Format("{0:0.00}% ({1} {2})", num6, (num7 == -1) ? "∞" : num7.ToString(), (num7 == 1) ? App.Localize("@UI_GENERAL_TURN") : App.Localize("@UI_GENERAL_TURNS"));
			int value11 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.OverdevelopRate);
			float num9 = (float)StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.TradeRate);
			int num10 = (int)(colonyInfo.CivilianWeight * 100f);
			string propertyValue14 = string.Format("{0:0}% ({1} {2})", num10, (num4 > 0f) ? ("+" + num4.ToString()) : num4.ToString(), App.Localize("@UI_GAMECOLONYDETAILS_BIOSPHERE"));
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"pnlSlaves"
			}), flag);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partSlavePop"
			}), flag);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"pnlMorale"
			}), !flag);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partCivSlider"
			}), !flag);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partHardenedStructure"
			}), sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowHardenedStructures, colonyInfo.PlayerID));
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"lblHardenedStructure"
			}), sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowHardenedStructures, colonyInfo.PlayerID));
			sim.UI.SetChecked(sim.UI.Path(new string[]
			{
				panelId,
				"partHardenedStructure"
			}), colonyInfo.isHardenedStructures);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partHardenedStructure"
			}), value5);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partTerraSlider"
			}), value);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partTerraSlider"
			}), value5);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partInfraSlider"
			}), value2);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partInfraSlider"
			}), value5);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partShipConSlider"
			}), value3);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partShipConSlider"
			}), value5);
			sim.UI.SetVisible(sim.UI.Path(new string[]
			{
				panelId,
				"partOverDevelopment"
			}), value4);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partOverDevelopment"
			}), value5);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partTradeSlider"
			}), value5);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partCivSlider"
			}), value5);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partOverharvestSlider"
			}), value5);
			sim.UI.SetEnabled(sim.UI.Path(new string[]
			{
				panelId,
				"partWorkRate"
			}), value5);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partStage"
			}), "text", App.Localize("@UI_COLONYSTAGE_" + colonyInfo.CurrentStage.ToString().ToUpper()));
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partOverharvestSlider",
				"right_label"
			}), "text", propertyValue);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partCivSlider",
				"right_label"
			}), "text", propertyValue14);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partCivPop"
			}), "value", propertyValue2);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partSlavePop"
			}), "value", propertyValue3);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partImpPop"
			}), "value", propertyValue4);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partIncome"
			}), "value", propertyValue5);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partEconRating"
			}), "value", propertyValue6);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partLifeSupCost"
			}), "value", propertyValue7);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partIndustrial"
			}), "value", propertyValue8);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partTerraSlider",
				"right_label"
			}), "text", propertyValue9);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partInfraSlider",
				"right_label"
			}), "text", propertyValue10);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partShipConSlider",
				"right_label"
			}), "text", propertyValue11);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partOverDevelopment",
				"right_label"
			}), "text", propertyValue13);
			sim.UI.SetPropertyString(sim.UI.Path(new string[]
			{
				panelId,
				"partWorkRate",
				"right_label"
			}), "text", propertyValue12);
			if (sliderName != "partTradeSlider")
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partTradeSlider"
				}), (int)num9);
				sim.UI.ClearSliderNotches(sim.UI.Path(new string[]
				{
					panelId,
					"partTradeSlider"
				}));
				foreach (double num11 in sim.GetTradeRatesForWholeExportsForColony(colonyInfo.ID))
				{
					sim.UI.AddSliderNotch(sim.UI.Path(new string[]
					{
						panelId,
						"partTradeSlider"
					}), (int)Math.Ceiling(num11 * 100.0));
				}
			}
			if (sliderName != "partCivSlider")
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partCivSlider"
				}), num10);
			}
			if (sliderName != "partTerraSlider")
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partTerraSlider"
				}), value6);
			}
			if (sliderName != "partInfraSlider")
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partInfraSlider"
				}), value7);
			}
			if (sliderName != "partShipConSlider")
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partShipConSlider"
				}), value8);
			}
			if (sliderName != "partOverDevelopment")
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partOverDevelopment"
				}), value11);
			}
			if (sliderName != "partOverharvestSlider")
			{
				sim.UI.SetSliderRange(sim.UI.Path(new string[]
				{
					panelId,
					"partOverharvestSlider"
				}), (int)(sim.GameDatabase.GetStratModifier<float>(StratModifiers.MinOverharvestRate, colonyInfo.PlayerID) * 100f), 100);
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partOverharvestSlider"
				}), value10);
			}
			if (sliderName != "partWorkRate")
			{
				sim.UI.SetSliderValue(sim.UI.Path(new string[]
				{
					panelId,
					"partWorkRate"
				}), value9);
			}
			foreach (Faction current in sim.AssetDatabase.Factions)
			{
				if (current.IsPlayable)
				{
					sim.UI.SetText(sim.UI.Path(new string[]
					{
						panelId,
						"MoraleBar.MoraleRow.gameMorale_" + current.Name + ".value"
					}), "--");
					sim.UI.SetPropertyColor(sim.UI.Path(new string[]
					{
						panelId,
						"MoraleBar.MoraleRow.gameMorale_" + current.Name + ".value"
					}), "color", 255f, 255f, 255f);
					sim.UI.SetVisible(sim.UI.Path(new string[]
					{
						panelId,
						"MoraleBar.MoraleRow.gameMorale_" + current.Name
					}), false);
					sim.UI.SetTooltip(sim.UI.Path(new string[]
					{
						panelId,
						"MoraleBar.MoraleRow.gameMorale_" + current.Name
					}), current.Name);
				}
			}
			foreach (ColonyFactionInfo current2 in sim.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID))
			{
				string factionName = sim.GameDatabase.GetFactionName(current2.FactionID);
				sim.UI.SetText(sim.UI.Path(new string[]
				{
					panelId,
					"MoraleBar.MoraleRow.gameMorale_" + factionName + ".value"
				}), string.Format("{0:0}", current2.Morale));
				sim.UI.SetVisible(sim.UI.Path(new string[]
				{
					panelId,
					"MoraleBar.MoraleRow.gameMorale_" + factionName
				}), true);
			}
			sim.UI.AutoSize(sim.UI.Path(new string[]
			{
				panelId,
				"partConstructionSliders",
				"ColonyModifiers"
			}));
			sim.UI.AutoSize(sim.UI.Path(new string[]
			{
				panelId,
				"partConstructionSliders"
			}));
			sim.UI.AutoSize(sim.UI.Path(new string[]
			{
				panelId,
				"pnlMoraleStats"
			}));
			ColonyHistoryData lastColonyHistoryForColony = sim.GameDatabase.GetLastColonyHistoryForColony(colonyId);
			Vector3 value12 = new Vector3(255f, 0f, 0f);
			Vector3 value13 = new Vector3(0f, 255f, 0f);
			Vector3 value14 = new Vector3(255f, 255f, 255f);
			sim.UI.SetPropertyColor(sim.UI.Path(new string[]
			{
				panelId,
				"partCivPop"
			}), "color", value14);
			sim.UI.SetPropertyColor(sim.UI.Path(new string[]
			{
				panelId,
				"partSlavePop"
			}), "color", value14);
			sim.UI.SetPropertyColor(sim.UI.Path(new string[]
			{
				panelId,
				"partImpPop"
			}), "color", value14);
			sim.UI.SetPropertyColor(sim.UI.Path(new string[]
			{
				panelId,
				"partIncome"
			}), "color", value14);
			sim.UI.SetPropertyColor(sim.UI.Path(new string[]
			{
				panelId,
				"partEconRating"
			}), "color", value14);
			sim.UI.SetPropertyColor(sim.UI.Path(new string[]
			{
				panelId,
				"partLifeSupCost"
			}), "color", value14);
			sim.UI.SetPropertyColor(sim.UI.Path(new string[]
			{
				panelId,
				"partIndustrial"
			}), "color", value14);
			if (lastColonyHistoryForColony != null)
			{
				if (lastColonyHistoryForColony.civ_pop < civilianPopulation)
				{
					sim.UI.SetPropertyColor(sim.UI.Path(new string[]
					{
						panelId,
						"partCivPop"
					}), "color", value13);
				}
				else
				{
					if (lastColonyHistoryForColony.civ_pop > civilianPopulation)
					{
						sim.UI.SetPropertyColor(sim.UI.Path(new string[]
						{
							panelId,
							"partCivPop"
						}), "color", value12);
					}
				}
				if (lastColonyHistoryForColony.slave_pop < slavePopulation)
				{
					sim.UI.SetPropertyColor(sim.UI.Path(new string[]
					{
						panelId,
						"partSlavePop"
					}), "color", value13);
				}
				else
				{
					if (lastColonyHistoryForColony.slave_pop > slavePopulation)
					{
						sim.UI.SetPropertyColor(sim.UI.Path(new string[]
						{
							panelId,
							"partSlavePop"
						}), "color", value12);
					}
				}
				if (lastColonyHistoryForColony.imp_pop < colonyInfo.ImperialPop)
				{
					sim.UI.SetPropertyColor(sim.UI.Path(new string[]
					{
						panelId,
						"partImpPop"
					}), "color", value13);
				}
				else
				{
					if (lastColonyHistoryForColony.imp_pop > colonyInfo.ImperialPop)
					{
						sim.UI.SetPropertyColor(sim.UI.Path(new string[]
						{
							panelId,
							"partImpPop"
						}), "color", value12);
					}
				}
				if (0 < num2)
				{
					sim.UI.SetPropertyColor(sim.UI.Path(new string[]
					{
						panelId,
						"partIncome"
					}), "color", value13);
				}
				else
				{
					if (0 > num2)
					{
						sim.UI.SetPropertyColor(sim.UI.Path(new string[]
						{
							panelId,
							"partIncome"
						}), "color", value12);
					}
				}
				if (lastColonyHistoryForColony.econ_rating < economyRating)
				{
					sim.UI.SetPropertyColor(sim.UI.Path(new string[]
					{
						panelId,
						"partEconRating"
					}), "color", value13);
				}
				else
				{
					if (lastColonyHistoryForColony.econ_rating > economyRating)
					{
						sim.UI.SetPropertyColor(sim.UI.Path(new string[]
						{
							panelId,
							"partEconRating"
						}), "color", value12);
					}
				}
				if (0 > num)
				{
					sim.UI.SetPropertyColor(sim.UI.Path(new string[]
					{
						panelId,
						"partLifeSupCost"
					}), "color", value13);
				}
				else
				{
					if (0 < num)
					{
						sim.UI.SetPropertyColor(sim.UI.Path(new string[]
						{
							panelId,
							"partLifeSupCost"
						}), "color", value12);
					}
				}
				if (lastColonyHistoryForColony.industrial_output < num3)
				{
					sim.UI.SetPropertyColor(sim.UI.Path(new string[]
					{
						panelId,
						"partIndustrial"
					}), "color", value13);
				}
				else
				{
					if (lastColonyHistoryForColony.industrial_output > num3)
					{
						sim.UI.SetPropertyColor(sim.UI.Path(new string[]
						{
							panelId,
							"partIndustrial"
						}), "color", value12);
					}
				}
			}
			List<ColonyFactionMoraleHistory> source = sim.GameDatabase.GetLastColonyMoraleHistoryForColony(colonyId).ToList<ColonyFactionMoraleHistory>();
			foreach (ColonyFactionInfo civFaction in sim.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID))
			{
				ColonyFactionMoraleHistory colonyFactionMoraleHistory = source.FirstOrDefault((ColonyFactionMoraleHistory x) => x.factionid == civFaction.FactionID);
				if (colonyFactionMoraleHistory != null)
				{
					string factionName2 = sim.GameDatabase.GetFactionName(civFaction.FactionID);
					if (colonyFactionMoraleHistory.morale < civFaction.Morale)
					{
						sim.UI.SetPropertyColor(sim.UI.Path(new string[]
						{
							panelId,
							"MoraleBar.MoraleRow.gameMorale_" + factionName2 + ".value"
						}), "color", value13);
					}
					else
					{
						if (colonyFactionMoraleHistory.morale > civFaction.Morale)
						{
							sim.UI.SetPropertyColor(sim.UI.Path(new string[]
							{
								panelId,
								"MoraleBar.MoraleRow.gameMorale_" + factionName2 + ".value"
							}), "color", value12);
						}
					}
				}
			}
		}
		internal static void SyncSystemDetailsWidget(App game, string panelName, int systemId, bool showScreenNavButtons, bool show = true)
		{
			if (systemId == 0)
			{
				game.UI.SetVisible(panelName, false);
				return;
			}
			game.UI.SetVisible(panelName, show);
			string panelId = game.UI.Path(new string[]
			{
				panelName,
				"title"
			});
			string panelId2 = game.UI.Path(new string[]
			{
				panelName,
				"partStellarClass"
			});
			string mapPanelId = game.UI.Path(new string[]
			{
				panelName,
				"partMiniSystem"
			});
			string text = game.UI.Path(new string[]
			{
				panelName,
				"ScreenNavButtons"
			});
			game.UI.SetVisible(text, showScreenNavButtons);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				text,
				"gameBuildButton.active"
			}), false);
			game.UI.SetText(game.UI.Path(new string[]
			{
				text,
				"numTurns"
			}), "");
			if (systemId == 0)
			{
				game.UI.SetText(panelId2, string.Empty);
			}
			else
			{
				StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
				game.UI.SetText(panelId, string.Format("{0} SYSTEM", starSystemInfo.Name.ToUpperInvariant()));
				game.UI.SetText(panelId2, starSystemInfo.StellarClass);
				int? systemOwningPlayer = game.GameDatabase.GetSystemOwningPlayer(systemId);
				if (systemOwningPlayer.HasValue && systemOwningPlayer.Value == game.LocalPlayer.ID)
				{
					IEnumerable<BuildOrderInfo> buildOrdersForSystem = game.GameDatabase.GetBuildOrdersForSystem(systemId);
					bool value = false;
					foreach (BuildOrderInfo current in buildOrdersForSystem)
					{
						if (game.GameDatabase.GetDesignInfo(current.DesignID).PlayerID == game.LocalPlayer.ID)
						{
							value = true;
							break;
						}
					}
					IEnumerable<RetrofitOrderInfo> retrofitOrdersForSystem = game.GameDatabase.GetRetrofitOrdersForSystem(systemId);
					foreach (RetrofitOrderInfo current2 in retrofitOrdersForSystem)
					{
						if (game.GameDatabase.GetDesignInfo(current2.DesignID).PlayerID == game.LocalPlayer.ID)
						{
							value = true;
							break;
						}
					}
					double num = 0.0;
					double num2 = 0.0;
					float num3 = 0f;
					List<ColonyInfo> list = new List<ColonyInfo>();
					List<int> list2 = game.GameDatabase.GetStarSystemPlanets(systemId).ToList<int>();
					foreach (int current3 in list2)
					{
						ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(current3);
						if (colonyInfoForPlanet != null)
						{
							num += Colony.GetTaxRevenue(game, colonyInfoForPlanet);
							num2 += (double)colonyInfoForPlanet.ShipConRate;
							num3 += Colony.GetConstructionPoints(game.Game, colonyInfoForPlanet);
							list.Add(colonyInfoForPlanet);
						}
					}
					num3 *= game.Game.GetStationBuildModifierForSystem(systemId, game.LocalPlayer.ID);
					float num4 = num3;
					int num5 = 0;
					int num6 = 0;
					foreach (BuildOrderInfo current4 in buildOrdersForSystem)
					{
						num6 += current4.Progress;
						num5 += current4.ProductionTarget;
					}
					int num7 = (int)Math.Ceiling((double)((float)(num5 - num6) / num4));
					int num8 = 0;
					if (retrofitOrdersForSystem.Count<RetrofitOrderInfo>() > 0)
					{
						ShipInfo shipInfo = game.GameDatabase.GetShipInfo(retrofitOrdersForSystem.First<RetrofitOrderInfo>().ShipID, true);
                        num8 = (int)Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(game, shipInfo, retrofitOrdersForSystem.Count<RetrofitOrderInfo>());
					}
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						text,
						"gameBuildButton.active"
					}), value);
					if (num7 > 0 || num8 > 0)
					{
						game.UI.SetText(game.UI.Path(new string[]
						{
							text,
							"numTurns"
						}), (num8 <= num7) ? num7.ToString() : num8.ToString());
					}
					else
					{
						game.UI.SetText(game.UI.Path(new string[]
						{
							text,
							"numTurns"
						}), "");
					}
				}
			}
			bool flag = game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemId);
			game.UI.SetEnabled("gameSystemButton", flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"SysBorder_Unsurveyed"
			}), !flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"SysBorder_Surveyed"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"UnSurveyed"
			}), !flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"Surveyed"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"btnSystemOpen"
			}), game.GameDatabase.GetPlayerColonySystemIDs(game.LocalPlayer.ID).Contains(systemId));
			game.UI.SetChecked(game.UI.Path(new string[]
			{
				panelName,
				"btnSystemOpen"
			}), game.GameDatabase.GetStarSystemInfo(systemId).IsOpen);
			StarSystemMapUI.Sync(game, systemId, mapPanelId, true);
			game.UI.AutoSize(panelName);
		}
		internal static void SyncPlanetDetailsWidget(GameSession game, string panelName, int systemId, int orbitId, IGameObject planetViewObject, PlanetView planetView)
		{
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"gamePlanetDetails"
			}), false);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"gameMoonDetails"
			}), false);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"gameGasGiantDetails"
			}), false);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"gameStarDetails"
			}), false);
			if (planetView != null)
			{
				planetView.PostSetProp("Planet", (planetViewObject != null) ? planetViewObject.ObjectID : 0);
			}
			if (systemId == 0 || orbitId == -2)
			{
				return;
			}
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitId);
			if (planetInfo != null)
			{
				if (StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
				{
					game.UI.SetVisible("gamePlanetDetails", true);
					StarSystemUI.SyncPlanetDetailsControl(game, "gamePlanetDetails", orbitId);
					if (planetView != null)
					{
						game.UI.Send(new object[]
						{
							"SetGameObject",
							game.UI.Path(new string[]
							{
								"gamePlanetDetails",
								"Planet_panel"
							}),
							planetView.ObjectID
						});
						return;
					}
				}
				else
				{
					if (planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Gaseous)
					{
						game.UI.SetVisible("gameGasGiantDetails", true);
						StarSystemUI.SyncGasGiantDetailsControl(game, "gameGasGiantDetails", orbitId);
						if (planetView != null)
						{
							game.UI.Send(new object[]
							{
								"SetGameObject",
								game.UI.Path(new string[]
								{
									"gameGasGiantDetails",
									"Planet_panel"
								}),
								planetView.ObjectID
							});
							return;
						}
					}
					else
					{
						if (planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Barren)
						{
							game.UI.SetVisible("gameMoonDetails", true);
							StarSystemUI.SyncMoonDetailsControl(game, "gameMoonDetails", orbitId);
							if (planetView != null)
							{
								game.UI.Send(new object[]
								{
									"SetGameObject",
									game.UI.Path(new string[]
									{
										"gameMoonDetails",
										"Planet_panel"
									}),
									planetView.ObjectID
								});
							}
						}
					}
				}
				return;
			}
			if (game.GameDatabase.GetLargeAsteroidInfo(orbitId) == null)
			{
				game.UI.SetVisible("gameStarDetails", true);
				StarSystemUI.SyncStarDetailsControl(game, "gameStarDetails", systemId);
				if (planetView != null)
				{
					game.UI.Send(new object[]
					{
						"SetGameObject",
						game.UI.Path(new string[]
						{
							"gameStarDetails",
							"Planet_panel"
						}),
						planetView.ObjectID
					});
				}
				bool flag = game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemId);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"gameStarDetails.StarBorder_UnSurveyed"
				}), !flag);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"gameStarDetails.StarBorder_Surveyed"
				}), flag);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"gameStarDetails.Star_Unsurveyed"
				}), !flag);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"gameStarDetails.Star_Surveyed"
				}), flag);
				return;
			}
			game.UI.SetVisible("gameMoonDetails", true);
			StarSystemUI.SyncAsteroidDetailsControl(game, "gameMoonDetails", orbitId);
		}
		internal static void SyncColonyDetailsWidget(GameSession sim, string panelName, int planetId, string sliderName = "")
		{
			AIColonyIntel colonyIntelForPlanet = sim.GameDatabase.GetColonyIntelForPlanet(sim.LocalPlayer.ID, planetId);
			if (colonyIntelForPlanet == null || colonyIntelForPlanet.OwningPlayerID == 0 || !colonyIntelForPlanet.ColonyID.HasValue)
			{
				sim.UI.SetVisible(panelName, false);
				return;
			}
			bool flag = false;
			if (colonyIntelForPlanet != null)
			{
				PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colonyIntelForPlanet.OwningPlayerID);
				if (!playerInfo.isStandardPlayer && playerInfo.includeInDiplomacy)
				{
					flag = true;
				}
			}
			if ((colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == sim.LocalPlayer.ID) || flag)
			{
				sim.UI.SetVisible(panelName, true);
				StarSystemUI.SyncColonyDetailsControl(sim, "colonyControl", colonyIntelForPlanet.ColonyID.Value, sliderName);
				sim.UI.SetEnabled(sim.UI.Path(new string[]
				{
					sliderName,
					"btnAbandon"
				}), !flag);
				sim.UI.SetVisible(sim.UI.Path(new string[]
				{
					sliderName,
					"btnAbandon"
				}), !flag);
				return;
			}
			sim.UI.SetVisible(panelName, false);
		}
		internal static void SyncPlanetListWidget(GameSession sim, string panelName, IEnumerable<int> orbitalIds)
		{
			if (orbitalIds.Count<int>() == 0)
			{
				sim.UI.SetVisible(panelName, false);
				return;
			}
			sim.UI.SetVisible(panelName, true);
			FleetUI.SyncPlanetListControl(sim, "gamePlanetList", orbitalIds);
		}
	}
}
