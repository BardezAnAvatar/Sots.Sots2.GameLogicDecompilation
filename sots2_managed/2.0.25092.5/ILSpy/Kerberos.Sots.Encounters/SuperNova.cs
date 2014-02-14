using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class SuperNova
	{
		public static SuperNova InitializeEncounter()
		{
			return new SuperNova();
		}
		public static SuperNova ResumeEncounter()
		{
			return new SuperNova();
		}
		public void UpdateTurn(GameSession game, int id)
		{
			SuperNovaInfo superNovaInfo = game.GameDatabase.GetSuperNovaInfo(id);
			if (superNovaInfo == null)
			{
				game.GameDatabase.RemoveEncounter(id);
				return;
			}
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(superNovaInfo.SystemId);
			Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(superNovaInfo.SystemId);
			List<StarSystemInfo> list = game.GameDatabase.GetSystemsInRange(starSystemOrigin, game.AssetDatabase.GlobalSuperNovaData.BlastRadius).ToList<StarSystemInfo>();
			list.Insert(0, starSystemInfo);
			float systemInRangeMinHazard = game.AssetDatabase.GlobalSuperNovaData.SystemInRangeMinHazard;
			float systemInRangeMaxHazard = game.AssetDatabase.GlobalSuperNovaData.SystemInRangeMaxHazard;
			int systemInRangeBioReduction = game.AssetDatabase.GlobalSuperNovaData.SystemInRangeBioReduction;
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (StarSystemInfo current in list)
			{
				List<ColonyInfo> list2 = game.GameDatabase.GetColonyInfosForSystem(current.ID).ToList<ColonyInfo>();
				foreach (ColonyInfo current2 in list2)
				{
					if (!dictionary.ContainsKey(current2.PlayerID))
					{
						dictionary.Add(current2.PlayerID, 0);
					}
					Dictionary<int, int> dictionary2;
					int playerID;
					(dictionary2 = dictionary)[playerID = current2.PlayerID] = dictionary2[playerID] + 1;
				}
			}
			List<int> list3 = new List<int>();
			foreach (StarSystemInfo current3 in list)
			{
				List<ColonyInfo> list4 = game.GameDatabase.GetColonyInfosForSystem(current3.ID).ToList<ColonyInfo>();
				if (superNovaInfo.TurnsRemaining > 0)
				{
					using (List<ColonyInfo>.Enumerator enumerator4 = list4.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							ColonyInfo current4 = enumerator4.Current;
							if (!list3.Contains(current4.PlayerID))
							{
								game.GameDatabase.InsertTurnEvent(new TurnEvent
								{
									EventType = TurnEventType.EV_SUPER_NOVA_TURN,
									EventMessage = TurnEventMessage.EM_SUPER_NOVA_TURN,
									PlayerID = current4.PlayerID,
									SystemID = superNovaInfo.SystemId,
									ArrivalTurns = superNovaInfo.TurnsRemaining,
									TurnNumber = game.GameDatabase.GetTurnCount(),
									Param1 = starSystemInfo.Name,
									NumShips = dictionary[current4.PlayerID],
									ShowsDialog = true
								});
								list3.Add(current4.PlayerID);
								Player playerObject = game.GetPlayerObject(current4.PlayerID);
								if (playerObject != null && playerObject.IsStandardPlayer)
								{
									game.InsertNewRadiationShieldResearchProject(current4.PlayerID);
								}
							}
						}
						continue;
					}
				}
				foreach (ColonyInfo current5 in list4)
				{
					if (!list3.Contains(current5.PlayerID))
					{
						game.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_SUPER_NOVA_DESTROYED_SYSTEM,
							EventMessage = TurnEventMessage.EM_SUPER_NOVA_DESTROYED_SYSTEM,
							PlayerID = current5.PlayerID,
							SystemID = superNovaInfo.SystemId,
							TurnNumber = game.GameDatabase.GetTurnCount(),
							Param1 = starSystemInfo.Name,
							NumShips = dictionary[current5.PlayerID],
							ShowsDialog = true
						});
						list3.Add(current5.PlayerID);
					}
				}
				if (current3.ID != superNovaInfo.SystemId)
				{
					float length = (starSystemOrigin - game.GameDatabase.GetStarSystemOrigin(current3.ID)).Length;
					List<int> list5 = new List<int>();
					foreach (ColonyInfo current6 in list4)
					{
						if (!list5.Contains(current6.PlayerID))
						{
							PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(current6.PlayerID);
							if (playerInfo.isStandardPlayer)
							{
								GameSession.ApplyMoralEvent(game.App, MoralEvent.ME_SUPER_NOVA_RADIATION, current6.PlayerID, null, null, new int?(current3.ID));
							}
							list5.Add(current6.PlayerID);
						}
						if (!game.GameDatabase.GetHasPlayerStudiedSpecialProject(current6.PlayerID, SpecialProjectType.RadiationShielding))
						{
							ColonyFactionInfo[] factions = current6.Factions;
							for (int i = 0; i < factions.Length; i++)
							{
								ColonyFactionInfo colonyFactionInfo = factions[i];
								game.GameDatabase.RemoveCivilianPopulation(colonyFactionInfo.OrbitalObjectID, colonyFactionInfo.FactionID);
							}
							game.GameDatabase.RemoveColonyOnPlanet(current6.OrbitalObjectID);
							PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(current6.OrbitalObjectID);
							planetInfo.Biosphere = systemInRangeBioReduction;
							planetInfo.Suitability += (float)(App.GetSafeRandom().CoinToss(50) ? -1 : 1) * ((systemInRangeMaxHazard - systemInRangeMinHazard) * Math.Min(Math.Max(1f - length / game.AssetDatabase.GlobalSuperNovaData.BlastRadius, 1f), 0f) + systemInRangeMinHazard);
							game.GameDatabase.UpdatePlanet(planetInfo);
						}
					}
				}
			}
			if (superNovaInfo.TurnsRemaining > 0)
			{
				superNovaInfo.TurnsRemaining--;
				game.GameDatabase.UpdateSuperNovaInfo(superNovaInfo);
				return;
			}
			game.GameDatabase.RemoveEncounter(id);
			game.GameDatabase.DestroyStarSystem(game, superNovaInfo.SystemId);
			if (game.App.CurrentState is StarMapState)
			{
				((StarMapState)game.App.CurrentState).ClearSelectedObject();
				((StarMapState)game.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
			}
		}
		public void ExecuteInstance(GameSession game, GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			gamedb.InsertSuperNovaInfo(new SuperNovaInfo
			{
				SystemId = systemid,
				TurnsRemaining = App.GetSafeRandom().NextInclusive(assetdb.GlobalSuperNovaData.MinExplodeTurns, assetdb.GlobalSuperNovaData.MaxExplodeTurns)
			});
			List<int> list = game.GameDatabase.GetStandardPlayerIDs().ToList<int>();
			List<int> list2 = new List<int>();
			Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(systemid);
			List<StarSystemInfo> list3 = game.GameDatabase.GetSystemsInRange(starSystemOrigin, game.AssetDatabase.GlobalSuperNovaData.BlastRadius).ToList<StarSystemInfo>();
			foreach (StarSystemInfo current in list3)
			{
				if (current.ID != systemid)
				{
					List<ColonyInfo> list4 = game.GameDatabase.GetColonyInfosForSystem(current.ID).ToList<ColonyInfo>();
					foreach (ColonyInfo current2 in list4)
					{
						if (!list2.Contains(current2.PlayerID) && list.Contains(current2.PlayerID))
						{
							game.InsertNewRadiationShieldResearchProject(current2.PlayerID);
							list2.Add(current2.PlayerID);
						}
					}
				}
			}
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int targetSystem)
		{
			gamedb.InsertIncomingGM(targetSystem, EasterEgg.GM_SUPER_NOVA, gamedb.GetTurnCount() + 1);
			List<PlayerInfo> list = gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, current.ID) > 0)
				{
					gamedb.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_SUPER_NOVA,
						EventMessage = TurnEventMessage.EM_INCOMING_SUPER_NOVA,
						PlayerID = current.ID,
						SystemID = targetSystem,
						TurnNumber = gamedb.GetTurnCount()
					});
				}
			}
		}
		public static void AddSuperNovas(GameSession game, GameDatabase gamedb, AssetDatabase assetdb)
		{
			if (!gamedb.HasEndOfFleshExpansion() || game.ScriptModules.SuperNova == null || gamedb.GetTurnCount() < assetdb.GlobalSuperNovaData.MinTurns)
			{
				return;
			}
			string nameValue = game.GameDatabase.GetNameValue("GMCount");
			if (string.IsNullOrEmpty(nameValue))
			{
				game.GameDatabase.InsertNameValuePair("GMCount", "0");
				nameValue = game.GameDatabase.GetNameValue("GMCount");
			}
			int nameValue2 = game.GameDatabase.GetNameValue<int>("GSGrandMenaceCount");
			int num = int.Parse(nameValue);
			if (num >= nameValue2)
			{
				return;
			}
			Random safeRandom = App.GetSafeRandom();
			if (!safeRandom.CoinToss(assetdb.GlobalSuperNovaData.Chance))
			{
				return;
			}
			List<StarSystemInfo> list = gamedb.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<SuperNovaInfo> source = gamedb.GetSuperNovaInfos().ToList<SuperNovaInfo>();
			List<int> list2 = new List<int>();
			foreach (StarSystemInfo ssi in list)
			{
				StellarClass stellarClass = new StellarClass(ssi.StellarClass);
				if (stellarClass.Type == StellarType.O || stellarClass.Type == StellarType.B)
				{
					if (!source.Any((SuperNovaInfo x) => x.SystemId == ssi.ID))
					{
						list2.Add(ssi.ID);
					}
				}
			}
			if (list2.Count > 0)
			{
				game.ScriptModules.SuperNova.AddInstance(gamedb, assetdb, safeRandom.Choose(list2));
				GameDatabase arg_191_0 = game.GameDatabase;
				string arg_191_1 = "GMCount";
				int num2;
				num = (num2 = num + 1);
				arg_191_0.UpdateNameValuePair(arg_191_1, num2.ToString());
			}
		}
		public static bool IsPlayerSystemsInSuperNovaEffectRanges(GameDatabase gamedb, int playerId, int systemId)
		{
			StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(systemId);
			if (starSystemInfo == null)
			{
				return false;
			}
			List<SuperNovaInfo> list = gamedb.GetSuperNovaInfos().ToList<SuperNovaInfo>();
			if (list.Count == 0)
			{
				return false;
			}
			bool flag = false;
			List<ColonyInfo> list2 = gamedb.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list2)
			{
				if (current.PlayerID == playerId)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			float num = gamedb.AssetDatabase.GlobalSuperNovaData.BlastRadius * gamedb.AssetDatabase.GlobalSuperNovaData.BlastRadius;
			foreach (SuperNovaInfo current2 in list)
			{
				Vector3 starSystemOrigin = gamedb.GetStarSystemOrigin(current2.SystemId);
				if ((starSystemOrigin - starSystemInfo.Origin).LengthSquared <= num)
				{
					return true;
				}
			}
			return false;
		}
	}
}
