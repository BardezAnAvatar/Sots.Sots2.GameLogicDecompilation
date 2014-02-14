using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal sealed class IntelMissionDesc_HighestTradeSystem : IntelMissionDesc_XxxSystem
	{
		public IntelMissionDesc_HighestTradeSystem()
		{
			base.ID = IntelMission.HighestTradeSystem;
			base.Name = App.Localize("@INTEL_NAME_HIGHEST_TRADE_SYSTEM_INFO");
			base.TurnEventTypes = new TurnEventType[]
			{
				TurnEventType.EV_INTEL_MISSION_HIGHEST_TRADE_SYSTEM
			};
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
					EventType = TurnEventType.EV_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					SystemID = int.Parse(source.First<CounterIntelResponse>().value),
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				return;
			}
			TradeResultsTable tradeResultsTable = game.GameDatabase.GetTradeResultsTable();
			List<KeyValuePair<int, TradeNode>> list = (
				from x in tradeResultsTable.TradeNodes
				where game.GameDatabase.GetSystemOwningPlayer(x.Key) == targetPlayerId
				select x).ToList<KeyValuePair<int, TradeNode>>();
			if (list.Count == 0)
			{
				game.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_NO_HIGHEST_TRADE_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_NO_HIGHEST_TRADE_SYSTEM,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					TurnNumber = game.GameDatabase.GetTurnCount()
				});
				return;
			}
			int systemID = (
				from x in list
				orderby x.Value.GetTotalImportsAndExports() descending
				select x).First<KeyValuePair<int, TradeNode>>().Key;
			if (source.Any((CounterIntelResponse x) => x.auto))
			{
				List<int> choices = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayerId).ToList<int>();
				systemID = game.Random.Choose(choices);
			}
			game.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
				EventMessage = TurnEventMessage.EM_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
				PlayerID = playerId,
				TargetPlayerID = targetPlayerId,
				SystemID = systemID,
				TurnNumber = game.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}
	}
}
