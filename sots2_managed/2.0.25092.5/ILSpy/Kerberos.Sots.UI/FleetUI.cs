using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class FleetUI
	{
		public const string UIFleetItemDetails = "partFleetDetails";
		public const string UIFleetItemEscortShips = "EscortShipsList";
		public const string UIFleetItemEssentialShips = "EssentialShipsList";
		public const string UIPlanetsTab = "planetsTab";
		public const string UIFleetsTab = "fleetsTab";
		public const string UIFleetList = "partSystemFleets";
		public const string UIPlanetList = "partSystemPlanets";
		public const string UIFleetSelectedControl = "gameFleetList";
		public static bool ShowFleetListDefault;
		public static int StarItemID
		{
			get
			{
				return -1;
			}
		}
		public static void SyncSelectablePlanetListControl(GameSession game, string planetListId, string subsection, string name, Vector4 color, bool setSelColor)
		{
			string globalID = game.UI.GetGlobalID(planetListId + "." + subsection);
			game.UI.SetPropertyString(globalID + ".listitem_name", "text", name);
			if (setSelColor)
			{
				Vector4 value = new Vector4(255f, 255f, 255f, 100f);
				if (color.W > 0f)
				{
					value = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
				}
				game.UI.SetPropertyColor(string.Concat(new string[]
				{
					globalID,
					".LC;",
					globalID,
					".RC;",
					globalID,
					".BG"
				}), "color", value);
			}
			game.UI.SetPropertyColor(string.Concat(new string[]
			{
				globalID,
				".colony_insert.LC;",
				globalID,
				".colony_insert.RC;",
				globalID,
				".colony_insert.BG"
			}), "color", color);
		}
		public static void SyncPlanetItemControl(GameSession game, string panelName, OrbitalObjectInfo orbital)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbital.ID);
			if (planetInfo == null)
			{
				return;
			}
			game.GameDatabase.GetMoons(orbital.ID);
			string propertyValue = App.Localize("@UI_PLANET_TYPE_" + planetInfo.Type.ToUpperInvariant());
			Vector4 color = new Vector4(0f, 0f, 0f, 0f);
			AIColonyIntel colonyIntelForPlanet = game.GameDatabase.GetColonyIntelForPlanet(game.LocalPlayer.ID, planetInfo.ID);
			if (colonyIntelForPlanet != null)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colonyIntelForPlanet.OwningPlayerID);
				color.X = playerInfo.PrimaryColor.X * 255f;
				color.Y = playerInfo.PrimaryColor.Y * 255f;
				color.Z = playerInfo.PrimaryColor.Z * 255f;
				color.W = 255f;
			}
			game.UI.AddItem(panelName, string.Empty, orbital.ID, string.Empty);
			string itemGlobalID = game.UI.GetItemGlobalID(panelName, string.Empty, orbital.ID, string.Empty);
			string panelId = game.UI.Path(new string[]
			{
				itemGlobalID,
				"expand_button"
			});
			game.UI.SetVisible(panelId, false);
			bool flag = game.GameDatabase.CanColonizePlanet(game.LocalPlayer.ID, planetInfo.ID, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)) || (colonyIntelForPlanet != null && colonyIntelForPlanet.PlayerID == game.LocalPlayer.ID);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				itemGlobalID,
				"header_idle.idle.h_good"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				itemGlobalID,
				"header_idle.idle.h_bad"
			}), !flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				itemGlobalID,
				"header_idle.mouse_over.h_good"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				itemGlobalID,
				"header_idle.mouse_over.h_bad"
			}), !flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				itemGlobalID,
				"header_sel.idle.h_good"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				itemGlobalID,
				"header_sel.idle.h_bad"
			}), !flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				itemGlobalID,
				"header_sel.mouse_over.h_good"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				itemGlobalID,
				"header_sel.mouse_over.h_bad"
			}), !flag);
			FleetUI.SyncSelectablePlanetListControl(game, itemGlobalID, "header_idle.idle", orbital.Name, color, false);
			FleetUI.SyncSelectablePlanetListControl(game, itemGlobalID, "header_idle.mouse_over", orbital.Name, color, false);
			FleetUI.SyncSelectablePlanetListControl(game, itemGlobalID, "header_sel.idle", orbital.Name, color, true);
			FleetUI.SyncSelectablePlanetListControl(game, itemGlobalID, "header_sel.mouse_over", orbital.Name, color, true);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				itemGlobalID,
				"planetItemPlanetType"
			}), "text", propertyValue);
			game.UI.AutoSize(game.UI.Path(new string[]
			{
				itemGlobalID,
				"expanded"
			}));
		}
		public static void SyncExistingPlanet(GameSession game, string panelName, OrbitalObjectInfo orbital)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbital.ID);
			if (planetInfo == null)
			{
				return;
			}
			game.GameDatabase.GetMoons(orbital.ID);
			string propertyValue = App.Localize("@UI_PLANET_TYPE_" + planetInfo.Type.ToUpperInvariant());
			Vector4 color = new Vector4(0f, 0f, 0f, 0f);
			AIColonyIntel colonyIntelForPlanet = game.GameDatabase.GetColonyIntelForPlanet(game.LocalPlayer.ID, planetInfo.ID);
			if (colonyIntelForPlanet != null)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colonyIntelForPlanet.PlayerID);
				color.X = playerInfo.PrimaryColor.X * 255f;
				color.Y = playerInfo.PrimaryColor.Y * 255f;
				color.Z = playerInfo.PrimaryColor.Z * 255f;
				color.W = 255f;
			}
			bool flag = game.GameDatabase.CanColonizePlanet(game.LocalPlayer.ID, planetInfo.ID, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)) || (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == game.LocalPlayer.ID);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"header_idle.idle.h_good"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"header_idle.idle.h_bad"
			}), !flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"header_idle.mouse_over.h_good"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"header_idle.mouse_over.h_bad"
			}), !flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"header_sel.idle.h_good"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"header_sel.idle.h_bad"
			}), !flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"header_sel.mouse_over.h_good"
			}), flag);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				"header_sel.mouse_over.h_bad"
			}), !flag);
			FleetUI.SyncSelectablePlanetListControl(game, panelName, "header_idle.idle", orbital.Name, color, false);
			FleetUI.SyncSelectablePlanetListControl(game, panelName, "header_idle.mouse_over", orbital.Name, color, false);
			FleetUI.SyncSelectablePlanetListControl(game, panelName, "header_sel.idle", orbital.Name, color, true);
			FleetUI.SyncSelectablePlanetListControl(game, panelName, "header_sel.mouse_over", orbital.Name, color, true);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"planetItemPlanetType"
			}), "text", propertyValue);
			game.UI.AutoSize(game.UI.Path(new string[]
			{
				panelName,
				"expanded"
			}));
		}
		public static void SyncPlanetListControl(GameSession game, string panelName, IEnumerable<int> orbitalObjects)
		{
			game.UI.ClearItems(panelName);
			foreach (int current in orbitalObjects)
			{
				OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(current);
				FleetUI.SyncPlanetItemControl(game, panelName, orbitalObjectInfo);
			}
			game.UI.AutoSize(panelName);
		}
		public static void SyncPlanetListControl(GameSession game, string panelName, int systemId, IEnumerable<int> orbitalObjects)
		{
			bool flag = game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemId);
			game.UI.ClearItems(panelName);
			List<OrbitalObjectInfo> list = game.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId).ToList<OrbitalObjectInfo>();
			Dictionary<int, float> DistanceMap = new Dictionary<int, float>();
			foreach (OrbitalObjectInfo current in list)
			{
				DistanceMap.Add(current.ID, current.OrbitalPath.Scale.Y);
			}
			list.Sort(delegate(OrbitalObjectInfo x, OrbitalObjectInfo y)
			{
				float num = x.ParentID.HasValue ? DistanceMap[x.ParentID.Value] : x.OrbitalPath.Scale.Y;
				float value = y.ParentID.HasValue ? DistanceMap[y.ParentID.Value] : y.OrbitalPath.Scale.Y;
				int num2 = num.CompareTo(value);
				if (num2 == 0)
				{
					if (x.ParentID.HasValue && y.ParentID.HasValue)
					{
						return x.OrbitalPath.Scale.Y.CompareTo(y.OrbitalPath.Scale.Y);
					}
					if (x.ParentID.HasValue)
					{
						return 1;
					}
					if (y.ParentID.HasValue)
					{
						return -1;
					}
				}
				return num2;
			});
			if (orbitalObjects.Contains(FleetUI.StarItemID))
			{
				Vector4 color = new Vector4(0f, 0f, 0f, 0f);
				StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
				game.UI.AddItem(panelName, string.Empty, FleetUI.StarItemID, string.Empty);
				string itemGlobalID = game.UI.GetItemGlobalID(panelName, string.Empty, FleetUI.StarItemID, string.Empty);
				FleetUI.SyncSelectablePlanetListControl(game, itemGlobalID, "header_idle.idle", starSystemInfo.Name, color, false);
				FleetUI.SyncSelectablePlanetListControl(game, itemGlobalID, "header_idle.mouse_over", starSystemInfo.Name, color, false);
				FleetUI.SyncSelectablePlanetListControl(game, itemGlobalID, "header_sel.idle", starSystemInfo.Name, color, true);
				FleetUI.SyncSelectablePlanetListControl(game, itemGlobalID, "header_sel.mouse_over", starSystemInfo.Name, color, true);
				string panelId = game.UI.Path(new string[]
				{
					panelName,
					itemGlobalID,
					"expand_button"
				});
				game.UI.SetVisible(panelId, false);
			}
			if (flag)
			{
				foreach (OrbitalObjectInfo current2 in list)
				{
					if (current2.ID != FleetUI.StarItemID && orbitalObjects.Contains(current2.ID))
					{
						FleetUI.SyncPlanetItemControl(game, panelName, current2);
					}
				}
			}
			game.UI.AutoSize(panelName);
		}
		private static string GetShipClassBadge(ShipClass shipClass, string className)
		{
			string result = "CR";
			switch (shipClass)
			{
			case ShipClass.Cruiser:
				if (className == "Command")
				{
					result = "CR_CMD";
				}
				break;
			case ShipClass.Dreadnought:
				if (className == "Command")
				{
					result = "DN_CMD";
				}
				else
				{
					result = "DN";
				}
				break;
			case ShipClass.Leviathan:
				result = "LV_CMD";
				break;
			}
			return result;
		}
		public static void SyncShipsCount(App game, string panelName, FleetInfo fleet, List<ShipItem> listships, bool essentials)
		{
		}
		public static void SyncFleetAndPlanetListWidget(GameSession game, string panelName, int systemId, bool show = true)
		{
			if (systemId == 0)
			{
				game.UI.SetVisible(panelName, false);
				return;
			}
			bool flag = game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemId);
			List<FleetInfo> list = game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
			List<StationInfo> list2 = game.GameDatabase.GetStationForSystemAndPlayer(systemId, game.LocalPlayer.ID).ToList<StationInfo>();
			bool flag2 = true;
			if (!game.SystemHasPlayerColony(systemId, game.LocalPlayer.ID) && !StarMap.IsInRange(game.GameDatabase, game.LocalPlayer.ID, game.GameDatabase.GetStarSystemInfo(systemId), null))
			{
				flag2 = false;
			}
			List<int> list3 = game.GameDatabase.GetStarSystemPlanets(systemId).ToList<int>();
			if ((list.Count == 0 && list2.Count == 0 && !flag) || (!flag2 && !flag))
			{
				game.UI.SetVisible(panelName, false);
				return;
			}
			bool flag3 = FleetUI.ShowFleetListDefault;
			bool flag4 = ((list.Count<FleetInfo>() != 0 && flag2) || list2.Count != 0) && show;
			bool flag5 = list3.Count<int>() != 0 && flag && show;
			if (flag3 && !flag4)
			{
				flag3 = false;
			}
			else
			{
				if (!flag3 && !flag5)
				{
					flag3 = true;
				}
			}
			game.UI.SetEnabled("planetsTab", flag5);
			game.UI.SetVisible("partSystemPlanets", flag5 && !flag3);
			game.UI.SetChecked("planetsTab", flag5 && !flag3);
			game.UI.SetEnabled("fleetsTab", flag4);
			game.UI.SetChecked("fleetsTab", flag4 && flag3);
			game.UI.SetVisible("partSystemFleets", flag4 && flag3);
			game.UI.SetVisible(panelName, show);
			FleetUI.SyncPlanetListControl(game, "partSystemPlanets", systemId, list3);
		}
		public static void SyncFleetAndPlanetListWidget(GameSession game, string panelName, int systemId, IEnumerable<FleetInfo> fleets, IEnumerable<int> orbitalObjectIds, bool show = true)
		{
			if (systemId == 0 || (fleets.Count<FleetInfo>() == 0 && orbitalObjectIds.Count<int>() == 0))
			{
				game.UI.SetVisible(panelName, false);
				return;
			}
			if (fleets.Count<FleetInfo>() == 0)
			{
				game.UI.SetVisible("fleetsTab", false);
			}
			else
			{
				game.UI.SetVisible("fleetsTab", show);
			}
			if (orbitalObjectIds.Count<int>() == 0)
			{
				game.UI.SetVisible("planetsTab", false);
				game.UI.SetPropertyInt("fleetsTab", "left", 30);
			}
			else
			{
				game.UI.SetVisible("planetsTab", show);
				game.UI.SetPropertyInt("fleetsTab", "left", 115);
			}
			game.UI.AutoSize("fleetsTab");
			game.UI.SetVisible(panelName, show);
			FleetUI.SyncPlanetListControl(game, "partSystemPlanets", systemId, orbitalObjectIds);
		}
		public static bool HandleFleetAndPlanetWidgetInput(App game, string panelName, string buttonPressed)
		{
			if (buttonPressed == "planetsTab")
			{
				FleetUI.ShowFleetListDefault = false;
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"partSystemFleets"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"partSystemPlanets"
				}), true);
				game.UI.SetChecked(game.UI.Path(new string[]
				{
					panelName,
					"planetsTab"
				}), true);
				game.UI.SetChecked(game.UI.Path(new string[]
				{
					panelName,
					"fleetsTab"
				}), false);
				return true;
			}
			if (buttonPressed == "fleetsTab")
			{
				FleetUI.ShowFleetListDefault = true;
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"partSystemFleets"
				}), true);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"partSystemPlanets"
				}), false);
				game.UI.SetChecked(game.UI.Path(new string[]
				{
					panelName,
					"planetsTab"
				}), false);
				game.UI.SetChecked(game.UI.Path(new string[]
				{
					panelName,
					"fleetsTab"
				}), true);
				return true;
			}
			return false;
		}
	}
}
