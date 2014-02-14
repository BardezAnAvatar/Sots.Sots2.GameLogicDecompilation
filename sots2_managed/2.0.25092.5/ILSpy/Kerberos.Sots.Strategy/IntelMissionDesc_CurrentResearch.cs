using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal sealed class IntelMissionDesc_CurrentResearch : IntelMissionDesc
	{
		public IntelMissionDesc_CurrentResearch()
		{
			base.ID = IntelMission.CurrentResearch;
			base.Name = App.Localize("@INTEL_NAME_CURRENT_RESEARCH");
			base.TurnEventTypes = new TurnEventType[0];
		}
		public override void OnCommit(GameSession game, int playerId, int targetPlayerId, int? missionid = null)
		{
			List<CounterIntelResponse> source = new List<CounterIntelResponse>();
			if (missionid.HasValue)
			{
				source = game.GameDatabase.GetCounterIntelResponses(missionid.Value).ToList<CounterIntelResponse>();
			}
			if (source.Any((CounterIntelResponse x) => !x.auto))
			{
				game.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_CURRENT_TECH,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_CURRENT_TECH,
					TechID = game.GameDatabase.GetTechID(source.First<CounterIntelResponse>().value),
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				return;
			}
			int num = game.GameDatabase.GetPlayerResearchingTechID(targetPlayerId);
			if (source.Any((CounterIntelResponse x) => x.auto))
			{
				List<PlayerTechInfo> list = game.GameDatabase.GetPlayerTechInfos(targetPlayerId).ToList<PlayerTechInfo>();
				num = list.ToArray()[game.Random.Next(0, list.Count)].TechID;
			}
			if (num != 0)
			{
				game.GameDatabase.GetPlayerTechInfo(targetPlayerId, num);
				game.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_CURRENT_TECH,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_CURRENT_TECH,
					TechID = num,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				return;
			}
			List<PlayerTechInfo> list2 = (
				from x in game.GameDatabase.GetPlayerTechInfos(targetPlayerId)
				where x.State == TechStates.Researched && x.TurnResearched > 1
				select x).ToList<PlayerTechInfo>();
			if (list2.Count == 0)
			{
				game.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_NO_COMPLETE_TECHS,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_NO_COMPLETE_TECHS,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					TurnNumber = game.GameDatabase.GetTurnCount()
				});
				return;
			}
			PlayerTechInfo playerTechInfo = (
				from x in list2
				orderby x.TurnResearched descending
				select x).First<PlayerTechInfo>();
			game.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_INTEL_MISSION_RECENT_TECH,
				EventMessage = TurnEventMessage.EM_INTEL_MISSION_RECENT_TECH,
				TechID = playerTechInfo.TechID,
				PlayerID = playerId,
				TargetPlayerID = targetPlayerId,
				TurnNumber = game.GameDatabase.GetTurnCount()
			});
		}
	}
}
