using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal sealed class IntelMissionDesc_NewestColonySystem : IntelMissionDesc_XxxSystem
	{
		public IntelMissionDesc_NewestColonySystem()
		{
			base.ID = IntelMission.NewestColonySystem;
			base.Name = App.Localize("@INTEL_NAME_NEWEST_COLONY_SYSTEM_INFO");
			base.TurnEventTypes = new TurnEventType[]
			{
				TurnEventType.EV_INTEL_MISSION_NEWEST_COLONY_SYSTEM
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
					EventType = TurnEventType.EV_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					SystemID = int.Parse(source.First<CounterIntelResponse>().value),
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				return;
			}
			List<ColonyInfo> list = game.GameDatabase.GetPlayerColoniesByPlayerId(targetPlayerId).ToList<ColonyInfo>();
			if (list.Count == 0)
			{
				game.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_NO_NEWEST_COLONY_SYSTEM,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_NO_NEWEST_COLONY_SYSTEM,
					PlayerID = playerId,
					TargetPlayerID = targetPlayerId,
					TurnNumber = game.GameDatabase.GetTurnCount()
				});
				return;
			}
			ColonyInfo colonyInfo = (
				from x in list
				orderby x.TurnEstablished descending
				select x).First<ColonyInfo>();
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
			int systemID = orbitalObjectInfo.StarSystemID;
			if (source.Any((CounterIntelResponse x) => x.auto))
			{
				List<int> choices = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayerId).ToList<int>();
				systemID = game.Random.Choose(choices);
			}
			game.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
				EventMessage = TurnEventMessage.EM_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
				PlayerID = playerId,
				TargetPlayerID = targetPlayerId,
				SystemID = systemID,
				ColonyID = colonyInfo.ID,
				TurnNumber = game.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}
	}
}
