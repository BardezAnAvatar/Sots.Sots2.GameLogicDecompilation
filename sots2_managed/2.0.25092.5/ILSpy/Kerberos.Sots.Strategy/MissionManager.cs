using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class MissionManager
	{
		private readonly StrategicAI ai;
		private readonly List<MissionManagerTargetInfo> targets = new List<MissionManagerTargetInfo>();
		public IEnumerable<MissionManagerTargetInfo> Targets
		{
			get
			{
				return this.targets;
			}
		}
		public MissionManager(StrategicAI ai)
		{
			this.ai = ai;
		}
		private bool IsInvadeEffective(int orbitalObjectId)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.ai.Game.GameDatabase.GetOrbitalObjectInfo(orbitalObjectId);
			List<CombatData> list = this.ai.Game.CombatData.GetCombatsForPlayer(this.ai.Game.GameDatabase, this.ai.Player.ID, orbitalObjectInfo.StarSystemID, 10).ToList<CombatData>();
			foreach (CombatData current in list)
			{
				PlayerCombatData player = current.GetPlayer(this.ai.Player.ID);
				if (player.FleetCount >= 3)
				{
					return false;
				}
			}
			return true;
		}
		internal void Update()
		{
			GameDatabase gameDatabase = this.ai.Game.GameDatabase;
			Player player = this.ai.Player;
			int turn = gameDatabase.GetTurnCount();
			this.targets.RemoveAll((MissionManagerTargetInfo x) => turn > x.ArrivalTurn || !this.ai.IsValidInvasionTarget(x.OrbitalObjectID) || !this.IsInvadeEffective(x.OrbitalObjectID));
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			foreach (int current in gameDatabase.GetPlayerIDs())
			{
				if (gameDatabase.GetDiplomacyStateBetweenPlayers(player.ID, current) == DiplomacyState.WAR)
				{
					foreach (AIFleetInfo current2 in 
						from x in gameDatabase.GetAIFleetInfos(player.ID)
						where x.FleetID.HasValue
						select x)
					{
						if ((current2.FleetType & 1536) != 0)
						{
							foreach (TargetOrbitalObjectScore current3 in this.ai.GetTargetsForInvasion(current2.FleetID.Value, current))
							{
								if (dictionary.ContainsKey(current3.OrbitalObjectID))
								{
									Dictionary<int, float> dictionary2;
									int orbitalObjectID;
									(dictionary2 = dictionary)[orbitalObjectID = current3.OrbitalObjectID] = dictionary2[orbitalObjectID] + current3.Score;
								}
								else
								{
									dictionary[current3.OrbitalObjectID] = current3.Score;
								}
							}
						}
					}
				}
			}
			foreach (MissionManagerTargetInfo current4 in this.targets)
			{
				dictionary.Remove(current4.OrbitalObjectID);
			}
			List<MissionManagerTargetInfo> list = (
				from x in dictionary
				select new MissionManagerTargetInfo
				{
					OrbitalObjectID = x.Key,
					Score = x.Value
				} into y
				orderby y.Score descending
				select y).ToList<MissionManagerTargetInfo>();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ArrivalTurn = turn + 3 + 2 * i;
			}
			this.targets.AddRange(list);
		}
	}
}
