using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal abstract class IntelMissionDesc
	{
		public IntelMission ID
		{
			get;
			protected set;
		}
		public string Name
		{
			get;
			protected set;
		}
		public IEnumerable<TurnEventType> TurnEventTypes
		{
			get;
			protected set;
		}
		public abstract void OnCommit(GameSession game, int playerId, int targetPlayerId, int? missionid = null);
		public virtual void OnProcessTurnEvent(GameSession game, TurnEvent e)
		{
		}
	}
}
