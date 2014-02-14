using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal sealed class IntelMissionDesc_RandomSystem : IntelMissionDesc_XxxSystem
	{
		public IntelMissionDesc_RandomSystem()
		{
			base.ID = IntelMission.RandomSystem;
			base.Name = App.Localize("@INTEL_NAME_RANDOM_SYSTEM_INFO");
			base.TurnEventTypes = new TurnEventType[]
			{
				TurnEventType.EV_INTEL_MISSION_RANDOM_SYSTEM
			};
		}
		public override void OnCommit(GameSession game, int playerId, int targetPlayerId, int? missionid = null)
		{
			List<int> list = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayerId).ToList<int>();
			if (missionid.HasValue)
			{
				List<CounterIntelResponse> source = game.GameDatabase.GetCounterIntelResponses(missionid.Value).ToList<CounterIntelResponse>();
				if (source.Any((CounterIntelResponse x) => !x.auto))
				{
					list.Clear();
					list.Add(int.Parse(source.First<CounterIntelResponse>().value));
				}
			}
			if (list.Count == 0)
			{
				game.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_NO_RANDOM_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_NO_RANDOM_SYSTEM,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					TurnNumber = game.GameDatabase.GetTurnCount()
				});
				return;
			}
			int systemID = game.Random.Choose(list);
			game.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_INTEL_MISSION_RANDOM_SYSTEM,
				EventMessage = TurnEventMessage.EM_INTEL_MISSION_RANDOM_SYSTEM,
				PlayerID = playerId,
				TargetPlayerID = targetPlayerId,
				SystemID = systemID,
				TurnNumber = game.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}
	}
}
