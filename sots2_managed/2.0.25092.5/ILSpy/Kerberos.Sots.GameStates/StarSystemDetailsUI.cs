using Kerberos.Sots.Data;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal static class StarSystemDetailsUI
	{
		private enum FleetDetail
		{
			Admiral,
			SpeedRange,
			ShipCounts,
			Destination,
			Location
		}
		public const string UITradeSlider = "partTradeSlider";
		public const string UITerraSlider = "partTerraSlider";
		public const string UIInfraSlider = "partInfraSlider";
		public const string UIOverDevSlider = "partOverDevelopment";
		public const string UIShipConSlider = "partShipConSlider";
		public const string UIOverharvestSlider = "partOverharvestSlider";
		public const string UICivPopulationSlider = "partCivSlider";
		public const string UISlaveWorkSlider = "partWorkRate";
		public static int StarItemID
		{
			get
			{
				return -1;
			}
		}
		public static string GetInfraString(float infra)
		{
			return ((int)(infra * 100f)).ToString();
		}
		public static string GetHazardString(float hazard)
		{
			return ((int)Math.Abs(hazard)).ToString();
		}
		public static string GetSizeString(float size)
		{
			if (size >= 1f)
			{
				return string.Format("{0}", (int)size);
			}
			return string.Format("{0:N}", size);
		}
		public static IEnumerable<int> CollectPlanetListItemsForSupportMission(App game, int systemId)
		{
			IEnumerable<int> starSystemPlanets = game.GameDatabase.GetStarSystemPlanets(systemId);
			foreach (int current in starSystemPlanets)
			{
				if (game.GameDatabase.CanSupportPlanet(game.LocalPlayer.ID, current))
				{
					yield return current;
				}
			}
			yield break;
		}
		public static IEnumerable<int> CollectPlanetListItemsForInvasionMission(App game, int systemId)
		{
			IEnumerable<int> starSystemPlanets = game.GameDatabase.GetStarSystemPlanets(systemId);
			foreach (int current in starSystemPlanets)
			{
				if (game.GameDatabase.CanInvadePlanet(game.LocalPlayer.ID, current))
				{
					yield return current;
				}
			}
			yield break;
		}
		public static IEnumerable<int> CollectPlanetListItemsForColonizeMission(App game, int systemId, int playerId)
		{
			IEnumerable<int> starSystemPlanets = game.GameDatabase.GetStarSystemPlanets(systemId);
			foreach (int current in starSystemPlanets)
			{
				if (game.GameDatabase.CanColonizePlanet(playerId, current, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerId)))
				{
					IEnumerable<MissionInfo> missionsByPlanetDest = game.GameDatabase.GetMissionsByPlanetDest(current);
					bool flag = true;
					foreach (MissionInfo current2 in missionsByPlanetDest)
					{
						if (current2.Type == MissionType.COLONIZATION)
						{
							flag = false;
							break;
						}
					}
					if (game.GameDatabase.GetPlanetHazardRating(playerId, current, false) > (float)game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerId))
					{
						flag = false;
					}
					if (flag || game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(playerId).FactionID).Name == "loa")
					{
						yield return current;
					}
				}
			}
			yield break;
		}
		public static IEnumerable<int> CollectPlanetListItemsForEvacuateMission(App game, int systemId, int playerId)
		{
			List<int> list = game.GameDatabase.GetStarSystemPlanets(systemId).ToList<int>();
			foreach (int current in list)
			{
				ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(current);
				if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == playerId && game.GameDatabase.GetCivilianPopulation(current, 0, false) > 0.0)
				{
					yield return current;
				}
			}
			yield break;
		}
		public static IEnumerable<int> CollectPlanetListItemsForConstructionMission(App game, int systemId)
		{
			IEnumerable<PlanetInfo> planetInfosOrbitingStar = game.GameDatabase.GetPlanetInfosOrbitingStar(systemId);
			foreach (PlanetInfo current in planetInfosOrbitingStar)
			{
				yield return current.ID;
			}
			yield return StarSystemDetailsUI.StarItemID;
			yield break;
		}
		public static bool IsOutputRateSlider(string panelName)
		{
			return panelName == "partTradeSlider" || panelName == "partTerraSlider" || panelName == "partInfraSlider" || panelName == "partOverDevelopment" || panelName == "partShipConSlider";
		}
		public static int OutputRateToSliderValue(float value)
		{
			return (int)(value * 100f);
		}
		public static float SliderValueToOutputRate(int value)
		{
			return (float)value / 100f;
		}
		public static void SetOutputRate(App game, int orbitalId, string panelName, string valueStr)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalId);
			Colony.OutputRate rate = Colony.OutputRate.Trade;
			if (panelName == "partInfraSlider")
			{
				rate = Colony.OutputRate.Infra;
			}
			else
			{
				if (panelName == "partTerraSlider")
				{
					rate = Colony.OutputRate.Terra;
				}
				else
				{
					if (panelName == "partShipConSlider")
					{
						rate = Colony.OutputRate.ShipCon;
					}
					else
					{
						if (panelName == "partOverDevelopment")
						{
							rate = Colony.OutputRate.OverDev;
						}
					}
				}
			}
			float value = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(valueStr));
			Colony.SetOutputRate(game.GameDatabase, game.AssetDatabase, ref colonyInfoForPlanet, planetInfo, rate, value);
			game.GameDatabase.UpdateColony(colonyInfoForPlanet);
		}
		public static void SetOutputRateNew(App game, int orbitalId, string panelName, string valueStr)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalId);
			Colony.OutputRate rate = Colony.OutputRate.Trade;
			if (panelName.Contains("partInfraSlider"))
			{
				rate = Colony.OutputRate.Infra;
			}
			else
			{
				if (panelName.Contains("partTerraSlider"))
				{
					rate = Colony.OutputRate.Terra;
				}
				else
				{
					if (panelName.Contains("partShipConSlider"))
					{
						rate = Colony.OutputRate.ShipCon;
					}
					else
					{
						if (panelName.Contains("partOverDevelopment"))
						{
							rate = Colony.OutputRate.OverDev;
						}
					}
				}
			}
			float value = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(valueStr));
			Colony.SetOutputRate(game.GameDatabase, game.AssetDatabase, ref colonyInfoForPlanet, planetInfo, rate, value);
			game.GameDatabase.UpdateColony(colonyInfoForPlanet);
		}
	}
}
