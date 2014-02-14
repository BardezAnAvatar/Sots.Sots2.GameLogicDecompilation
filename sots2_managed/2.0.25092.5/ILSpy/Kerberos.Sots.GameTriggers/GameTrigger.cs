using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameTriggers
{
	internal class GameTrigger
	{
		internal delegate bool EvaluateContextDelegate(TriggerContext tc, GameSession g, Trigger t);
		internal delegate bool EvaluateConditionDelegate(TriggerCondition tc, GameSession g, Trigger t);
		internal delegate bool EvaluateActionDelegate(TriggerAction ta, GameSession g, Trigger t);
		internal static Dictionary<Type, GameTrigger.EvaluateContextDelegate> ContextFunctionMap = new Dictionary<Type, GameTrigger.EvaluateContextDelegate>
		{

			{
				typeof(AlwaysContext),
				new GameTrigger.EvaluateContextDelegate(GameTrigger.EvaluateAlwaysContext)
			},

			{
				typeof(StartContext),
				new GameTrigger.EvaluateContextDelegate(GameTrigger.EvaluateStartContext)
			},

			{
				typeof(EndContext),
				new GameTrigger.EvaluateContextDelegate(GameTrigger.EvaluateEndContext)
			},

			{
				typeof(RangeContext),
				new GameTrigger.EvaluateContextDelegate(GameTrigger.EvaluateRangeContext)
			}
		};
		internal static Dictionary<Type, GameTrigger.EvaluateConditionDelegate> ConditionFunctionMap = new Dictionary<Type, GameTrigger.EvaluateConditionDelegate>
		{

			{
				typeof(GameOverCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateGameOverCondition)
			},

			{
				typeof(ScalarAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateScalarAmountCondition)
			},

			{
				typeof(TriggerTriggeredCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTriggerTriggeredCondition)
			},

			{
				typeof(SystemRangeCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateSystemRangeCondition)
			},

			{
				typeof(FleetRangeCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateFleetRangeCondition)
			},

			{
				typeof(ProvinceRangeCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateProvinceRangeCondition)
			},

			{
				typeof(ColonyDeathCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateColonyDeathCondition)
			},

			{
				typeof(PlanetDeathCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlanetDeathCondition)
			},

			{
				typeof(FleetDeathCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateFleetDeathCondition)
			},

			{
				typeof(ShipDeathCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateShipDeathCondition)
			},

			{
				typeof(AdmiralDeathCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateAdmiralDeathCondition)
			},

			{
				typeof(PlayerDeathCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlayerDeathCondition)
			},

			{
				typeof(AllianceFormedCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateAllianceFormedCondition)
			},

			{
				typeof(AllianceBrokenCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateAllianceBrokenCondition)
			},

			{
				typeof(GrandMenaceAppearedCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateGrandMenaceAppearedCondition)
			},

			{
				typeof(GrandMenaceDestroyedCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateGrandMenaceDestroyedCondition)
			},

			{
				typeof(ResourceAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateResourceAmountCondition)
			},

			{
				typeof(PlanetAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlanetAmountCondition)
			},

			{
				typeof(BiosphereAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateBiosphereAmountCondition)
			},

			{
				typeof(AllianceAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateAllianceAmountCondition)
			},

			{
				typeof(PopulationAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePopulationAmountCondition)
			},

			{
				typeof(FleetAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateFleetAmountCondition)
			},

			{
				typeof(CommandPointAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateCommandPointAmountCondition)
			},

			{
				typeof(TerrainRangeCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTerrainRangeCondition)
			},

			{
				typeof(TerrainColonizedCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTerrainColonizedCondition)
			},

			{
				typeof(PlanetColonizedCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlanetColonizedCondition)
			},

			{
				typeof(TreatyBrokenCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTreatyBrokenCondition)
			},

			{
				typeof(CivilianDeathCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateCivilianDeathCondition)
			},

			{
				typeof(MoralAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateMoralAmountCondition)
			},

			{
				typeof(GovernmentTypeCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateGovernmentTypeCondition)
			},

			{
				typeof(RevelationBeginsCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateRevelationBeginsCondition)
			},

			{
				typeof(ResearchObtainedCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateResearchObtainedCondition)
			},

			{
				typeof(ClassBuiltCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateClassBuiltCondition)
			},

			{
				typeof(TradePointsAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateTradePointsAmountCondition)
			},

			{
				typeof(IncomePerTurnAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateIncomePerTurnAmountCondition)
			},

			{
				typeof(FactionEncounteredCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateFactionEncounteredCondition)
			},

			{
				typeof(WorldTypeCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluateWorldTypeCondition)
			},

			{
				typeof(PlanetDevelopmentAmountCondition),
				new GameTrigger.EvaluateConditionDelegate(GameTrigger.EvaluatePlanetDevelopmentAmountCondition)
			}
		};
		internal static Dictionary<Type, GameTrigger.EvaluateActionDelegate> ActionFunctionMap = new Dictionary<Type, GameTrigger.EvaluateActionDelegate>
		{

			{
				typeof(GameOverAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateGameOverAction)
			},

			{
				typeof(PointPerPlanetDeathAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePointPerPlanetDeathAction)
			},

			{
				typeof(PointPerColonyDeathAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePointPerColonyDeathAction)
			},

			{
				typeof(PointPerShipTypeAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePointPerShipTypeAction)
			},

			{
				typeof(AddScalarToScalarAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAddScalarToScalarAction)
			},

			{
				typeof(SetScalarAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateSetScalarAction)
			},

			{
				typeof(ChangeScalarAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateChangeScalarAction)
			},

			{
				typeof(SpawnUnitAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateSpawnUnitAction)
			},

			{
				typeof(DiplomacyChangedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateDiplomacyChangedAction)
			},

			{
				typeof(ColonyChangedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateColonyChangedAction)
			},

			{
				typeof(AIStrategyChangedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAIStrategyChangedAction)
			},

			{
				typeof(ResearchAwardedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateResearchAwardedAction)
			},

			{
				typeof(AdmiralChangedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAdmiralChangedAction)
			},

			{
				typeof(RebellionOccursAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRebellionOccursAction)
			},

			{
				typeof(RebellionEndsAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRebellionEndsAction)
			},

			{
				typeof(PlanetDestroyedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePlanetDestroyedAction)
			},

			{
				typeof(PlanetAddedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluatePlanetAddedAction)
			},

			{
				typeof(TerrainAppearsAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateTerrainAppearsAction)
			},

			{
				typeof(TerrainDisappearsAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateTerrainDisappearsAction)
			},

			{
				typeof(ChangeTreasuryAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateChangeTreasuryAction)
			},

			{
				typeof(ChangeResourcesAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateChangeResourcesAction)
			},

			{
				typeof(RemoveFleetAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRemoveFleetAction)
			},

			{
				typeof(AddWeaponAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAddWeaponAction)
			},

			{
				typeof(RemoveWeaponAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRemoveWeaponAction)
			},

			{
				typeof(AddSectionAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAddSectionAction)
			},

			{
				typeof(RemoveSectionAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRemoveSectionAction)
			},

			{
				typeof(AddModuleAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateAddModuleAction)
			},

			{
				typeof(RemoveModuleAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateRemoveModuleAction)
			},

			{
				typeof(ProvinceChangedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateProvinceChangedAction)
			},

			{
				typeof(SurrenderSystemAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateSurrenderSystemAction)
			},

			{
				typeof(SurrenderEmpireAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateSurrenderEmpireAction)
			},

			{
				typeof(StratModifierChangedAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateStratModifierChangedAction)
			},

			{
				typeof(DisplayMessageAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateDisplayMessageAction)
			},

			{
				typeof(MoveFleetAction),
				new GameTrigger.EvaluateActionDelegate(GameTrigger.EvaluateMoveFleetAction)
			}
		};
		internal static int GetTriggerTriggeredDepth(List<Trigger> triggerPool, Trigger t)
		{
			if (GameTrigger.isTriggerTriggeredCondition(t))
			{
				int num = 0;
				using (List<TriggerCondition>.Enumerator enumerator = t.Conditions.GetEnumerator())
				{
					TriggerTriggeredCondition ttc;
					while (enumerator.MoveNext())
					{
						ttc = (TriggerTriggeredCondition)enumerator.Current;
						num = Math.Max(num, GameTrigger.GetTriggerTriggeredDepth(triggerPool, triggerPool.First((Trigger x) => x.Name == ttc.TriggerName)) + 1);
					}
				}
				return num;
			}
			return 1;
		}
		internal static bool isTriggerTriggeredCondition(Trigger t)
		{
			return t.Conditions.OfType<TriggerTriggeredCondition>().Count<TriggerTriggeredCondition>() > 0;
		}
		internal static void PushEvent(EventType et, object data, GameSession sim)
		{
			sim.TriggerEvents.Add(new EventStub
			{
				EventType = et,
				Data = data
			});
		}
		internal static void ClearEvents(GameSession sim)
		{
			sim.TriggerEvents.Clear();
		}
		internal static bool Evaluate(TriggerContext tc, GameSession g, Trigger t)
		{
			return GameTrigger.ContextFunctionMap.ContainsKey(tc.GetType()) && GameTrigger.ContextFunctionMap[tc.GetType()](tc, g, t);
		}
		internal static bool EvaluateAlwaysContext(TriggerContext tc, GameSession g, Trigger t)
		{
			return true;
		}
		internal static bool EvaluateStartContext(TriggerContext tc, GameSession g, Trigger t)
		{
			StartContext startContext = tc as StartContext;
			return g.GameDatabase.GetTurnCount() >= startContext.StartTurn;
		}
		internal static bool EvaluateEndContext(TriggerContext tc, GameSession g, Trigger t)
		{
			EndContext endContext = tc as EndContext;
			return g.GameDatabase.GetTurnCount() <= endContext.EndTurn;
		}
		internal static bool EvaluateRangeContext(TriggerContext tc, GameSession g, Trigger t)
		{
			RangeContext rangeContext = tc as RangeContext;
			int turnCount = g.GameDatabase.GetTurnCount();
			return turnCount >= rangeContext.StartTurn && turnCount <= rangeContext.EndTurn;
		}
		internal static bool Evaluate(TriggerCondition tc, GameSession g, Trigger t)
		{
			return GameTrigger.ConditionFunctionMap.ContainsKey(tc.GetType()) && GameTrigger.ConditionFunctionMap[tc.GetType()](tc, g, t);
		}
		internal static bool EvaluateGameOverCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			GameOverCondition goc = tc as GameOverCondition;
			EventStub eventStub = sim.TriggerEvents.FirstOrDefault((EventStub x) => x.EventType == EventType.EVNT_GAMEOVER && (string.IsNullOrEmpty(goc.Reason) || (string)x.Data == goc.Reason));
			if (eventStub != null)
			{
				sim.TriggerEvents.Remove(eventStub);
				return true;
			}
			return false;
		}
		internal static bool EvaluateScalarAmountCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			ScalarAmountCondition scalarAmountCondition = tc as ScalarAmountCondition;
			return sim.TriggerScalars.ContainsKey(scalarAmountCondition.Scalar) && sim.TriggerScalars[scalarAmountCondition.Scalar] > scalarAmountCondition.Value;
		}
		internal static bool EvaluateTriggerTriggeredCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			EventStub eventStub = sim.TriggerEvents.FirstOrDefault((EventStub x) => x.EventType == EventType.EVNT_TRIGGERTRIGGERED && (string)x.Data == (tc as TriggerTriggeredCondition).TriggerName);
			if (eventStub != null)
			{
				sim.TriggerEvents.Remove(eventStub);
				return true;
			}
			return false;
		}
		internal static bool EvaluateSystemRangeCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			SystemRangeCondition systemRangeCondition = tc as SystemRangeCondition;
			IEnumerable<FleetInfo> fleetInfosByPlayerID = sim.GameDatabase.GetFleetInfosByPlayerID(systemRangeCondition.PlayerSlot, FleetType.FL_NORMAL);
			StarSystemInfo starSystemInfo = sim.GameDatabase.GetStarSystemInfo(systemRangeCondition.SystemId);
			bool result = false;
			foreach (FleetInfo current in fleetInfosByPlayerID)
			{
				if ((sim.GameDatabase.GetFleetLocation(current.ID, false).Coords - starSystemInfo.Origin).Length <= systemRangeCondition.Distance)
				{
					t.RangeTriggeredFleets.Add(current);
					result = true;
				}
			}
			return result;
		}
		internal static bool EvaluateFleetRangeCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			bool result = false;
			FleetRangeCondition fleetRangeCondition = tc as FleetRangeCondition;
			IEnumerable<FleetInfo> fleetInfosByPlayerID = sim.GameDatabase.GetFleetInfosByPlayerID(fleetRangeCondition.PlayerSlot, FleetType.FL_NORMAL);
			FleetInfo fleetInfoByFleetName = sim.GameDatabase.GetFleetInfoByFleetName(fleetRangeCondition.FleetName, FleetType.FL_NORMAL);
			foreach (FleetInfo current in fleetInfosByPlayerID)
			{
				if ((sim.GameDatabase.GetFleetLocation(current.ID, false).Coords - sim.GameDatabase.GetFleetLocation(fleetInfoByFleetName.ID, false).Coords).Length <= fleetRangeCondition.Distance)
				{
					t.RangeTriggeredFleets.Add(current);
					result = true;
				}
			}
			return result;
		}
		internal static bool EvaluateProvinceRangeCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			bool result = false;
			ProvinceRangeCondition prc = tc as ProvinceRangeCondition;
			IEnumerable<FleetInfo> enumerable = new List<FleetInfo>(sim.GameDatabase.GetFleetInfosByPlayerID(prc.PlayerSlot, FleetType.FL_NORMAL));
			IEnumerable<StarSystemInfo> enumerable2 = new List<StarSystemInfo>(
				from x in sim.GameDatabase.GetStarSystemInfos()
				where x.ProvinceID == prc.ProvinceId
				select x);
			foreach (StarSystemInfo current in enumerable2)
			{
				foreach (FleetInfo current2 in enumerable)
				{
					if ((sim.GameDatabase.GetFleetLocation(current2.ID, false).Coords - current.Origin).Length <= prc.Distance)
					{
						t.RangeTriggeredFleets.Add(current2);
						result = true;
					}
				}
			}
			return result;
		}
		internal static bool EvaluateTerrainRangeCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateColonyDeathCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			ColonyDeathCondition cdc = tc as ColonyDeathCondition;
			int orbitId = sim.GameDatabase.GetStarSystemOrbitalObjectInfos(cdc.SystemId).First((OrbitalObjectInfo x) => x.Name == cdc.OrbitName).ID;
			EventStub eventStub = sim.TriggerEvents.FirstOrDefault((EventStub x) => x.EventType == EventType.EVNT_COLONYDIED && (int)x.Data == orbitId);
			if (eventStub != null)
			{
				sim.TriggerEvents.Remove(eventStub);
				return true;
			}
			return false;
		}
		internal static bool EvaluatePlanetDeathCondition(TriggerCondition tc, GameSession sim, Trigger t)
		{
			PlanetDeathCondition pdc = tc as PlanetDeathCondition;
			int orbitId = sim.GameDatabase.GetStarSystemOrbitalObjectInfos(pdc.SystemId).First((OrbitalObjectInfo x) => x.Name == pdc.OrbitName).ID;
			EventStub eventStub = sim.TriggerEvents.FirstOrDefault((EventStub x) => x.EventType == EventType.EVNT_PLANETDIED && (int)x.Data == orbitId);
			if (eventStub != null)
			{
				sim.TriggerEvents.Remove(eventStub);
				return true;
			}
			return false;
		}
		internal static bool EvaluateFleetDeathCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			FleetDeathCondition fdc = tc as FleetDeathCondition;
			EventStub eventStub = g.TriggerEvents.FirstOrDefault((EventStub x) => x.EventType == EventType.EVNT_FLEETDIED && (string)x.Data == fdc.FleetName);
			if (eventStub != null)
			{
				g.TriggerEvents.Remove(eventStub);
				return true;
			}
			return false;
		}
		internal static bool EvaluateShipDeathCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			ShipDeathCondition fdc = tc as ShipDeathCondition;
			EventStub eventStub = g.TriggerEvents.FirstOrDefault((EventStub x) => x.EventType == EventType.EVNT_SHIPDIED && (ShipClass)x.Data == fdc.ShipClass);
			if (eventStub != null)
			{
				g.TriggerEvents.Remove(eventStub);
				return true;
			}
			return false;
		}
		internal static bool EvaluateAdmiralDeathCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			AdmiralDeathCondition adc = tc as AdmiralDeathCondition;
			EventStub eventStub = g.TriggerEvents.FirstOrDefault((EventStub x) => x.EventType == EventType.EVNT_ADMIRALDIED && (string)x.Data == adc.AdmiralName);
			if (eventStub != null)
			{
				g.TriggerEvents.Remove(eventStub);
				return true;
			}
			return false;
		}
		internal static bool EvaluatePlayerDeathCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			PlayerDeathCondition pdc = tc as PlayerDeathCondition;
			EventStub eventStub = g.TriggerEvents.FirstOrDefault((EventStub x) => x.EventType == EventType.EVNT_PLAYERDIED && (int)x.Data == pdc.PlayerSlot);
			if (eventStub != null)
			{
				g.TriggerEvents.Remove(eventStub);
				return true;
			}
			return false;
		}
		internal static bool EvaluateAllianceFormedCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateAllianceBrokenCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateGrandMenaceAppearedCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateGrandMenaceDestroyedCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateResourceAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluatePlanetAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			PlanetAmountCondition pac = tc as PlanetAmountCondition;
			return (
				from x in g.GameDatabase.GetColonyInfos()
				where x.PlayerID == pac.PlayerSlot
				select x).Count<ColonyInfo>() > pac.NumberOfPlanets;
		}
		internal static bool EvaluateBiosphereAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			BiosphereAmountCondition bac = tc as BiosphereAmountCondition;
			OrbitalObjectInfo ooi = g.GameDatabase.GetStarSystemOrbitalObjectInfos(bac.SystemId).First((OrbitalObjectInfo x) => x.Name == bac.OrbitName);
			PlanetInfo planetInfo = g.GameDatabase.GetStarSystemPlanetInfos(bac.SystemId).First((PlanetInfo x) => x.ID == ooi.ID);
			return (float)planetInfo.Biosphere >= bac.BiosphereAmount;
		}
		internal static bool EvaluateAllianceAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluatePopulationAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			PopulationAmountCondition pac = tc as PopulationAmountCondition;
			OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(pac.SystemId).First((OrbitalObjectInfo x) => x.Name == pac.OrbitName);
			double num = g.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID).ImperialPop;
			foreach (ColonyFactionInfo current in g.GameDatabase.GetCivilianPopulations(orbitalObjectInfo.ID))
			{
				num += current.CivilianPop;
			}
			return num >= (double)pac.PopulationNumber;
		}
		internal static bool EvaluateFleetAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			FleetAmountCondition fleetAmountCondition = tc as FleetAmountCondition;
			return g.GameDatabase.GetFleetInfosByPlayerID(fleetAmountCondition.PlayerSlot, FleetType.FL_NORMAL).Count<FleetInfo>() >= fleetAmountCondition.NumberOfFleets;
		}
		internal static bool EvaluateCommandPointAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			int num = 0;
			CommandPointAmountCondition commandPointAmountCondition = tc as CommandPointAmountCondition;
			IEnumerable<FleetInfo> fleetInfosByPlayerID = g.GameDatabase.GetFleetInfosByPlayerID(commandPointAmountCondition.PlayerSlot, FleetType.FL_NORMAL);
			foreach (FleetInfo current in fleetInfosByPlayerID)
			{
				foreach (int current2 in g.GameDatabase.GetShipsByFleetID(current.ID))
				{
					num += g.GameDatabase.GetCommandPointCost(g.GameDatabase.GetShipInfo(current2, false).DesignID);
				}
			}
			return num >= commandPointAmountCondition.CommandPoints;
		}
		internal static bool EvaluateTerrainColonizedCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluatePlanetColonizedCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			PlanetColonizedCondition pcc = tc as PlanetColonizedCondition;
			OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(pcc.SystemId).First((OrbitalObjectInfo x) => x.Name == pcc.OrbitName);
			ColonyInfo colonyInfoForPlanet = g.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID);
			if (colonyInfoForPlanet != null)
			{
				IEnumerable<MissionInfo> missionsByPlanetDest = g.GameDatabase.GetMissionsByPlanetDest(orbitalObjectInfo.ID);
				foreach (MissionInfo current in missionsByPlanetDest)
				{
					if (current.Type == MissionType.COLONIZATION)
					{
						FleetInfo fleetInfo = g.GameDatabase.GetFleetInfo(current.FleetID);
						if (fleetInfo.PlayerID != colonyInfoForPlanet.PlayerID)
						{
							g.GameDatabase.ApplyDiplomacyReaction(fleetInfo.PlayerID, colonyInfoForPlanet.PlayerID, StratModifiers.DiplomacyReactionColonizeClaimedWorld, 1);
						}
					}
				}
				return true;
			}
			return false;
		}
		internal static bool EvaluateTreatyBrokenCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateCivilianDeathCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateMoralAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			MoralAmountCondition mac = tc as MoralAmountCondition;
			int iD = g.GameDatabase.GetStarSystemOrbitalObjectInfos(mac.SystemId).First((OrbitalObjectInfo x) => x.Name == mac.OrbitName).ID;
			int fid = g.GameDatabase.GetFactionIdFromName(mac.Faction);
			return (float)g.GameDatabase.GetCivilianPopulations(iD).First((ColonyFactionInfo x) => x.FactionID == fid).Morale <= mac.MoralAmount;
		}
		internal static bool EvaluateGovernmentTypeCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			GovernmentTypeCondition governmentTypeCondition = tc as GovernmentTypeCondition;
			return g.GameDatabase.GetGovernmentInfo(governmentTypeCondition.PlayerSlot).CurrentType == ScenarioEnumerations.GovernmentTypes[governmentTypeCondition.GovernmentType];
		}
		internal static bool EvaluateRevelationBeginsCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateResearchObtainedCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			ResearchObtainedCondition roc = tc as ResearchObtainedCondition;
			return g.GameDatabase.GetPlayerTechInfos(roc.PlayerSlot).Any((PlayerTechInfo x) => x.TechFileID == roc.TechId && x.State == TechStates.Researched);
		}
		internal static bool EvaluateClassBuiltCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			ClassBuiltCondition classBuiltCondition = tc as ClassBuiltCondition;
			IEnumerable<FleetInfo> fleetInfosByPlayerID = g.GameDatabase.GetFleetInfosByPlayerID(classBuiltCondition.PlayerSlot, FleetType.FL_NORMAL);
			foreach (FleetInfo current in fleetInfosByPlayerID)
			{
				foreach (ShipInfo current2 in g.GameDatabase.GetShipInfoByFleetID(current.ID, false))
				{
					if (current2.DesignInfo.Class.ToString() == classBuiltCondition.Class)
					{
						return true;
					}
				}
			}
			return false;
		}
		internal static bool EvaluateTradePointsAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateIncomePerTurnAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateFactionEncounteredCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateWorldTypeCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluatePlanetDevelopmentAmountCondition(TriggerCondition tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool Evaluate(TriggerAction ta, GameSession g, Trigger t)
		{
			return GameTrigger.ActionFunctionMap.ContainsKey(ta.GetType()) && GameTrigger.ActionFunctionMap[ta.GetType()](ta, g, t);
		}
		internal static bool EvaluateGameOverAction(TriggerAction tc, GameSession g, Trigger t)
		{
			GameOverAction gameOverAction = tc as GameOverAction;
			GameTrigger.PushEvent(EventType.EVNT_GAMEOVER, gameOverAction.Reason, g);
			return true;
		}
		internal static bool EvaluatePointPerPlanetDeathAction(TriggerAction tc, GameSession g, Trigger t)
		{
			PointPerPlanetDeathAction pointPerPlanetDeathAction = tc as PointPerPlanetDeathAction;
			float num = (float)(
				from x in g.TriggerEvents
				where x.EventType == EventType.EVNT_PLANETDIED
				select x).Count<EventStub>() * pointPerPlanetDeathAction.AmountPerPlanet;
			if (!g.TriggerScalars.ContainsKey(pointPerPlanetDeathAction.ScalarName))
			{
				g.TriggerScalars.Add(pointPerPlanetDeathAction.ScalarName, 0f);
			}
			Dictionary<string, float> triggerScalars;
			string scalarName;
			(triggerScalars = g.TriggerScalars)[scalarName = pointPerPlanetDeathAction.ScalarName] = triggerScalars[scalarName] + num;
			return true;
		}
		internal static bool EvaluatePointPerColonyDeathAction(TriggerAction tc, GameSession g, Trigger t)
		{
			PointPerColonyDeathAction pointPerColonyDeathAction = tc as PointPerColonyDeathAction;
			float num = (float)(
				from x in g.TriggerEvents
				where x.EventType == EventType.EVNT_COLONYDIED
				select x).Count<EventStub>() * pointPerColonyDeathAction.AmountPerColony;
			if (!g.TriggerScalars.ContainsKey(pointPerColonyDeathAction.ScalarName))
			{
				g.TriggerScalars.Add(pointPerColonyDeathAction.ScalarName, 0f);
			}
			Dictionary<string, float> triggerScalars;
			string scalarName;
			(triggerScalars = g.TriggerScalars)[scalarName = pointPerColonyDeathAction.ScalarName] = triggerScalars[scalarName] + num;
			return true;
		}
		internal static bool EvaluatePointPerShipTypeAction(TriggerAction tc, GameSession g, Trigger t)
		{
			PointPerShipTypeAction ppsta = tc as PointPerShipTypeAction;
			List<FleetInfo> list = new List<FleetInfo>();
			float num = 0f;
			if (ppsta.Fleet == ScenarioEnumerations.FleetsInRangeVariableName)
			{
				list.AddRange(t.RangeTriggeredFleets);
			}
			else
			{
				list.AddRange(
					from x in g.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL)
					where x.Name == ppsta.Fleet
					select x);
			}
			foreach (FleetInfo current in list)
			{
				List<ShipInfo> list2 = new List<ShipInfo>(g.GameDatabase.GetShipInfoByFleetID(current.ID, false));
				foreach (ShipInfo current2 in list2)
				{
					string shipType;
					if ((shipType = ppsta.ShipType) != null)
					{
						if (!(shipType == "Cruiser"))
						{
							if (!(shipType == "Dreadnaught"))
							{
								if (!(shipType == "Leviathan"))
								{
									if (!(shipType == "Colony"))
									{
										if (shipType == "Supply")
										{
											if (g.GameDatabase.GetDesignInfo(current2.DesignID).Role == ShipRole.SUPPLY)
											{
												num += ppsta.AmountPerShip;
											}
										}
									}
									else
									{
										if (g.GameDatabase.GetDesignInfo(current2.DesignID).Role == ShipRole.COLONIZER)
										{
											num += ppsta.AmountPerShip;
										}
									}
								}
								else
								{
									if (g.GameDatabase.GetDesignInfo(current2.DesignID).Class == ShipClass.Leviathan)
									{
										num += ppsta.AmountPerShip;
									}
								}
							}
							else
							{
								if (g.GameDatabase.GetDesignInfo(current2.DesignID).Class == ShipClass.Dreadnought)
								{
									num += ppsta.AmountPerShip;
								}
							}
						}
						else
						{
							if (g.GameDatabase.GetDesignInfo(current2.DesignID).Class == ShipClass.Cruiser)
							{
								num += ppsta.AmountPerShip;
							}
						}
					}
				}
			}
			if (!g.TriggerScalars.ContainsKey(ppsta.ScalarName))
			{
				g.TriggerScalars.Add(ppsta.ScalarName, num);
			}
			else
			{
				Dictionary<string, float> triggerScalars;
				string scalarName;
				(triggerScalars = g.TriggerScalars)[scalarName = ppsta.ScalarName] = triggerScalars[scalarName] + num;
			}
			return true;
		}
		internal static bool EvaluateAddScalarToScalarAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AddScalarToScalarAction addScalarToScalarAction = tc as AddScalarToScalarAction;
			if (!g.TriggerScalars.ContainsKey(addScalarToScalarAction.ScalarAddedTo))
			{
				g.TriggerScalars.Add(addScalarToScalarAction.ScalarAddedTo, 0f);
			}
			if (g.TriggerScalars.ContainsKey(addScalarToScalarAction.ScalarToAdd))
			{
				Dictionary<string, float> triggerScalars;
				string scalarAddedTo;
				(triggerScalars = g.TriggerScalars)[scalarAddedTo = addScalarToScalarAction.ScalarAddedTo] = triggerScalars[scalarAddedTo] + g.TriggerScalars[addScalarToScalarAction.ScalarToAdd];
			}
			return true;
		}
		internal static bool EvaluateSetScalarAction(TriggerAction tc, GameSession g, Trigger t)
		{
			SetScalarAction setScalarAction = tc as SetScalarAction;
			if (!g.TriggerScalars.ContainsKey(setScalarAction.Scalar))
			{
				g.TriggerScalars.Add(setScalarAction.Scalar, 0f);
			}
			g.TriggerScalars[setScalarAction.Scalar] = setScalarAction.Value;
			return true;
		}
		internal static bool EvaluateChangeScalarAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ChangeScalarAction changeScalarAction = tc as ChangeScalarAction;
			if (!g.TriggerScalars.ContainsKey(changeScalarAction.Scalar))
			{
				g.TriggerScalars.Add(changeScalarAction.Scalar, 0f);
			}
			Dictionary<string, float> triggerScalars;
			string scalar;
			(triggerScalars = g.TriggerScalars)[scalar = changeScalarAction.Scalar] = triggerScalars[scalar] + changeScalarAction.Value;
			return true;
		}
		internal static bool EvaluateDiplomacyChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateColonyChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ColonyChangedAction cca = tc as ColonyChangedAction;
			OrbitalObjectInfo ooi = g.GameDatabase.GetStarSystemOrbitalObjectInfos(cca.SystemId).First((OrbitalObjectInfo x) => x.Name == cca.OrbitName);
			ColonyInfo colonyInfo = g.GameDatabase.GetColonyInfos().First((ColonyInfo x) => x.OrbitalObjectID == ooi.ID);
			colonyInfo.PlayerID = cca.NewPlayer;
			g.GameDatabase.UpdateColony(colonyInfo);
			return true;
		}
		internal static bool EvaluateAIStrategyChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateResearchAwardedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ResearchAwardedAction researchAwardedAction = tc as ResearchAwardedAction;
			g.GameDatabase.UpdatePlayerTechState(researchAwardedAction.PlayerSlot, g.GameDatabase.GetTechID(researchAwardedAction.TechId), TechStates.Researched);
			return true;
		}
		internal static bool EvaluateAdmiralChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AdmiralChangedAction aca = tc as AdmiralChangedAction;
			AdmiralInfo admiralInfo = g.GameDatabase.GetAdmiralInfos().First((AdmiralInfo x) => x.Name == aca.OldAdmiral.Name);
			admiralInfo.Age = (float)aca.NewAdmiral.Age;
			admiralInfo.EvasionBonus = (int)aca.NewAdmiral.EvasionRating;
			admiralInfo.Gender = aca.NewAdmiral.Gender;
			admiralInfo.HomeworldID = new int?(aca.NewAdmiral.HomePlanet);
			admiralInfo.Name = aca.NewAdmiral.Name;
			admiralInfo.PlayerID = aca.NewPlayer;
			admiralInfo.Race = aca.NewAdmiral.Faction;
			admiralInfo.ReactionBonus = (int)aca.NewAdmiral.ReactionRating;
			g.GameDatabase.UpdateAdmiralInfo(admiralInfo);
			return true;
		}
		internal static bool EvaluateRebellionOccursAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateRebellionEndsAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluatePlanetDestroyedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			PlanetDestroyedAction pda = tc as PlanetDestroyedAction;
			OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(pda.SystemId).First((OrbitalObjectInfo x) => x.Name == pda.OrbitName);
			g.GameDatabase.RemoveOrbitalObject(orbitalObjectInfo.ID);
			return true;
		}
		internal static bool EvaluatePlanetAddedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			Random random = new Random();
			PlanetAddedAction paa = tc as PlanetAddedAction;
			OrbitalObjectInfo orbitalObjectInfo = null;
			if (!string.IsNullOrEmpty(paa.NewPlanet.Parent))
			{
				orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(paa.SystemId).First((OrbitalObjectInfo x) => x.Name == paa.NewPlanet.Parent);
			}
			float orbitStep = StarSystemVars.Instance.StarOrbitStep;
			if (orbitalObjectInfo != null)
			{
				if (g.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID) != null)
				{
					orbitStep = StarSystemVars.Instance.PlanetOrbitStep;
				}
				else
				{
					orbitStep = StarSystemVars.Instance.GasGiantOrbitStep;
				}
			}
			float parentRadius = StarSystemVars.Instance.StarRadius(StellarClass.Parse(g.GameDatabase.GetStarSystemInfo(paa.SystemId).StellarClass).Size);
			if (orbitalObjectInfo != null)
			{
				parentRadius = StarSystemVars.Instance.SizeToRadius(g.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID).Size);
			}
			float eccentricity = paa.NewPlanet.Eccentricity.HasValue ? paa.NewPlanet.Eccentricity.Value : random.NextNormal(StarSystemVars.Instance.OrbitEccentricityRange);
			if (!paa.NewPlanet.Inclination.HasValue)
			{
				random.NextNormal(StarSystemVars.Instance.OrbitInclinationRange);
			}
			else
			{
				float arg_173_0 = paa.NewPlanet.Inclination.Value;
			}
			float num = Orbit.CalcOrbitRadius(parentRadius, orbitStep, paa.NewPlanet.OrbitNumber);
			float x2 = Ellipse.CalcSemiMinorAxis(num, eccentricity);
			g.GameDatabase.InsertPlanet(new int?(orbitalObjectInfo.ID), paa.SystemId, new OrbitalPath
			{
				Scale = new Vector2(x2, num),
				InitialAngle = random.NextSingle() % 6.28318548f
			}, paa.NewPlanet.Name, paa.NewPlanet.PlanetType, null, paa.NewPlanet.Suitability.Value, paa.NewPlanet.Biosphere.Value, paa.NewPlanet.Resources.Value, (float)paa.NewPlanet.Size.Value);
			return true;
		}
		internal static bool EvaluateTerrainAppearsAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateTerrainDisappearsAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateChangeTreasuryAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ChangeTreasuryAction changeTreasuryAction = tc as ChangeTreasuryAction;
			g.GameDatabase.UpdatePlayerSavings(changeTreasuryAction.Player, g.GameDatabase.GetPlayerInfo(changeTreasuryAction.Player).Savings + (double)changeTreasuryAction.AmountToAdd);
			return true;
		}
		internal static bool EvaluateChangeResourcesAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ChangeResourcesAction cra = tc as ChangeResourcesAction;
			OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetStarSystemOrbitalObjectInfos(cra.SystemId).First((OrbitalObjectInfo x) => x.Name == cra.OrbitName);
			PlanetInfo planetInfo = g.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
			planetInfo.Resources += (int)cra.AmountToAdd;
			g.GameDatabase.UpdatePlanet(planetInfo);
			return true;
		}
		internal static bool EvaluateSpawnUnitAction(TriggerAction tc, GameSession g, Trigger t)
		{
			SpawnUnitAction spawnUnitAction = tc as SpawnUnitAction;
			DesignInfo designInfo = new DesignInfo();
			designInfo.PlayerID = spawnUnitAction.PlayerSlot;
			designInfo.Name = spawnUnitAction.ShipToAdd.Name;
			List<DesignSectionInfo> list = new List<DesignSectionInfo>();
			foreach (Section current in spawnUnitAction.ShipToAdd.Sections)
			{
				DesignSectionInfo designSectionInfo = new DesignSectionInfo
				{
					DesignInfo = designInfo
				};
				designSectionInfo.FilePath = string.Format("factions\\{0}\\sections\\{1}", spawnUnitAction.ShipToAdd.Faction, current.SectionFile);
				List<WeaponBankInfo> list2 = new List<WeaponBankInfo>();
				foreach (Bank current2 in current.Banks)
				{
					list2.Add(new WeaponBankInfo
					{
						WeaponID = g.GameDatabase.GetWeaponID(current2.Weapon, spawnUnitAction.PlayerSlot),
						BankGUID = Guid.Parse(current2.GUID)
					});
				}
				designSectionInfo.WeaponBanks = list2;
				list.Add(designSectionInfo);
			}
			designInfo.DesignSections = list.ToArray();
			g.GameDatabase.InsertDesignByDesignInfo(designInfo);
			return false;
		}
		internal static bool EvaluateRemoveFleetAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateAddWeaponAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AddWeaponAction addWeaponAction = tc as AddWeaponAction;
			LogicalWeapon weapon = WeaponLibrary.CreateLogicalWeaponFromFile(g.App, addWeaponAction.WeaponFile, -1);
			g.GameDatabase.InsertWeapon(weapon, addWeaponAction.Player);
			return true;
		}
		internal static bool EvaluateRemoveWeaponAction(TriggerAction tc, GameSession g, Trigger t)
		{
			RemoveWeaponAction removeWeaponAction = tc as RemoveWeaponAction;
			g.GameDatabase.RemoveWeapon(g.GameDatabase.GetWeaponID(removeWeaponAction.WeaponFile, removeWeaponAction.Player));
			return true;
		}
		internal static bool EvaluateAddSectionAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AddSectionAction addSectionAction = tc as AddSectionAction;
			g.GameDatabase.InsertSectionAsset(addSectionAction.SectionFile, addSectionAction.Player);
			return true;
		}
		internal static bool EvaluateRemoveSectionAction(TriggerAction tc, GameSession g, Trigger t)
		{
			RemoveSectionAction removeSectionAction = tc as RemoveSectionAction;
			g.GameDatabase.RemoveSection(g.GameDatabase.GetSectionAssetID(removeSectionAction.SectionFile, removeSectionAction.Player));
			return true;
		}
		internal static bool EvaluateAddModuleAction(TriggerAction tc, GameSession g, Trigger t)
		{
			AddModuleAction addModuleAction = tc as AddModuleAction;
			string factionName = g.GameDatabase.GetFactionName(g.GameDatabase.GetPlayerInfo(addModuleAction.Player).FactionID);
			g.GameDatabase.InsertModule(ModuleLibrary.CreateLogicalModuleFromFile(addModuleAction.ModuleFile, factionName), addModuleAction.Player);
			return true;
		}
		internal static bool EvaluateRemoveModuleAction(TriggerAction tc, GameSession g, Trigger t)
		{
			RemoveModuleAction removeModuleAction = tc as RemoveModuleAction;
			g.GameDatabase.RemoveModule(g.GameDatabase.GetModuleID(removeModuleAction.ModuleFile, removeModuleAction.Player));
			return true;
		}
		internal static bool EvaluateProvinceChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			ProvinceChangedAction provinceChangedAction = tc as ProvinceChangedAction;
			ProvinceInfo pi = g.GameDatabase.GetProvinceInfo(provinceChangedAction.ProvinceId);
			int playerID = pi.PlayerID;
			pi.PlayerID = provinceChangedAction.NewPlayer;
			IEnumerable<StarSystemInfo> enumerable = 
				from x in g.GameDatabase.GetStarSystemInfos()
				where x.ProvinceID == pi.ID
				select x;
			foreach (StarSystemInfo current in enumerable)
			{
				PlanetInfo[] starSystemPlanetInfos = g.GameDatabase.GetStarSystemPlanetInfos(current.ID);
				for (int i = 0; i < starSystemPlanetInfos.Length; i++)
				{
					PlanetInfo planetInfo = starSystemPlanetInfos[i];
					ColonyInfo colonyInfoForPlanet = g.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
					if (colonyInfoForPlanet.PlayerID == playerID)
					{
						colonyInfoForPlanet.PlayerID = provinceChangedAction.NewPlayer;
						g.GameDatabase.UpdateColony(colonyInfoForPlanet);
					}
				}
			}
			g.GameDatabase.UpdateProvinceInfo(pi);
			return false;
		}
		internal static bool EvaluateSurrenderSystemAction(TriggerAction tc, GameSession g, Trigger t)
		{
			SurrenderSystemAction surrenderSystemAction = tc as SurrenderSystemAction;
			IEnumerable<PlanetInfo> starSystemPlanetInfos = g.GameDatabase.GetStarSystemPlanetInfos(surrenderSystemAction.SystemId);
			List<int> list = new List<int>();
			foreach (PlanetInfo current in starSystemPlanetInfos)
			{
				ColonyInfo colonyInfoForPlanet = g.GameDatabase.GetColonyInfoForPlanet(current.ID);
				if (colonyInfoForPlanet != null)
				{
					if (colonyInfoForPlanet.PlayerID != surrenderSystemAction.NewPlayer && !list.Contains(colonyInfoForPlanet.PlayerID))
					{
						list.Add(colonyInfoForPlanet.PlayerID);
					}
					colonyInfoForPlanet.PlayerID = surrenderSystemAction.NewPlayer;
					g.GameDatabase.UpdateColony(colonyInfoForPlanet);
				}
			}
			foreach (int current2 in list)
			{
				Kerberos.Sots.GameStates.StarSystem.RemoveSystemPlayerColor(g.GameDatabase, surrenderSystemAction.SystemId, current2);
			}
			return true;
		}
		internal static bool EvaluateSurrenderEmpireAction(TriggerAction tc, GameSession g, Trigger t)
		{
			SurrenderEmpireAction sea = tc as SurrenderEmpireAction;
			List<ColonyInfo> list = (
				from x in g.GameDatabase.GetColonyInfos()
				where x.PlayerID == sea.SurrenderingPlayer
				select x).ToList<ColonyInfo>();
			List<int> list2 = new List<int>();
			foreach (ColonyInfo current in list)
			{
				OrbitalObjectInfo orbitalObjectInfo = g.GameDatabase.GetOrbitalObjectInfo(current.OrbitalObjectID);
				if (orbitalObjectInfo != null && !list2.Contains(orbitalObjectInfo.StarSystemID))
				{
					list2.Add(orbitalObjectInfo.StarSystemID);
				}
				current.PlayerID = sea.CapturingPlayer;
				g.GameDatabase.UpdateColony(current);
			}
			foreach (int current2 in list2)
			{
				Kerberos.Sots.GameStates.StarSystem.RemoveSystemPlayerColor(g.GameDatabase, current2, sea.SurrenderingPlayer);
			}
			return false;
		}
		internal static bool EvaluateStratModifierChangedAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
		internal static bool EvaluateDisplayMessageAction(TriggerAction tc, GameSession g, Trigger t)
		{
			DisplayMessageAction displayMessageAction = tc as DisplayMessageAction;
			string text = "";
			string[] array = displayMessageAction.Message.Split(new char[]
			{
				'$'
			});
			for (int i = 0; i < array.Length; i++)
			{
				if (i % 2 == 0)
				{
					text += array[i];
				}
				else
				{
					string[] array2;
					if (array[i].Contains('|'))
					{
						array2 = array[i].Split(new char[]
						{
							'|'
						});
					}
					else
					{
						array2 = new string[]
						{
							"{0}",
							array[i]
						};
					}
					if (g.TriggerScalars.ContainsKey(array[i]))
					{
						text += string.Format(array2[0], g.TriggerScalars[array2[1]]);
					}
				}
			}
			g.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_SCRIPT_MESSAGE,
				EventDesc = text,
				TurnNumber = g.GameDatabase.GetTurnCount()
			});
			return false;
		}
		internal static bool EvaluateMoveFleetAction(TriggerAction tc, GameSession g, Trigger t)
		{
			return false;
		}
	}
}
