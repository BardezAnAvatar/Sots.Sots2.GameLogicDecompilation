using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class Locust
	{
		private const string FactionName = "locusts";
		private const string PlayerName = "Locust Swarm";
		private const string PlayerAvatar = "\\base\\factions\\locusts\\avatars\\Locusts_Avatar.tga";
		private const string FleetName = "Locust Swarm";
		private const string ScoutFleetName = "Locust Swarm Scout";
		private const string WorldShipDesignFile = "lv_locust_worldship.section";
		private const string HeraldMoonDesignFile = "dn_locust_heraldmoon.section";
		private const string NeedleShipDesignFile = "locust_needleship.section";
		private int PlayerId = -1;
		private int _worldShipDesignId;
		private int _heraldMoonDesignId;
		private int _needleShipDesignId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int WorldShipDesignId
		{
			get
			{
				return this._worldShipDesignId;
			}
		}
		public int HeraldMoonDesignId
		{
			get
			{
				return this._heraldMoonDesignId;
			}
		}
		public int NeedleShipDesignId
		{
			get
			{
				return this._needleShipDesignId;
			}
		}
		public static Locust InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Locust locust = new Locust();
			locust.PlayerId = gamedb.InsertPlayer("Locust Swarm", "locusts", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\locusts\\avatars\\Locusts_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(locust.PlayerId, "World Ship", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "locusts", "lv_locust_worldship.section")
			});
			DesignInfo design2 = new DesignInfo(locust.PlayerId, "Herald Moon", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "locusts", "dn_locust_heraldmoon.section")
			});
			DesignInfo design3 = new DesignInfo(locust.PlayerId, "Needle Ship", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "locusts", "locust_needleship.section")
			});
			locust._worldShipDesignId = gamedb.InsertDesignByDesignInfo(design);
			locust._heraldMoonDesignId = gamedb.InsertDesignByDesignInfo(design2);
			locust._needleShipDesignId = gamedb.InsertDesignByDesignInfo(design3);
			return locust;
		}
		public static Locust ResumeEncounter(GameDatabase gamedb)
		{
			Locust locust = new Locust();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Locust Swarm"));
			locust.PlayerId = playerInfo.ID;
			List<DesignInfo> source2 = gamedb.GetDesignInfosForPlayer(locust.PlayerId).ToList<DesignInfo>();
			locust._worldShipDesignId = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("lv_locust_worldship.section")).ID;
			locust._heraldMoonDesignId = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("dn_locust_heraldmoon.section")).ID;
			locust._needleShipDesignId = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("locust_needleship.section")).ID;
			return locust;
		}
		public void ExecuteInstance(GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			int fleetId = gamedb.InsertFleet(this.PlayerId, 0, systemid, systemid, "Locust Swarm", FleetType.FL_NORMAL);
			gamedb.InsertLocustSwarmInfo(new LocustSwarmInfo
			{
				FleetId = new int?(fleetId),
				NumDrones = assetdb.GlobalLocustData.MaxDrones
			});
			gamedb.InsertShip(fleetId, this._worldShipDesignId, null, (ShipParams)0, null, 0);
			if (gamedb.HasEndOfFleshExpansion())
			{
				int id = gamedb.GetLocustSwarmInfos().First((LocustSwarmInfo x) => x.FleetId == fleetId).Id;
				for (int i = 0; i < assetdb.GlobalLocustData.InitialLocustScouts; i++)
				{
					gamedb.InsertLocustSwarmScoutInfo(new LocustSwarmScoutInfo
					{
						LocustInfoId = id,
						NumDrones = assetdb.GlobalLocustData.MaxDrones,
						TargetSystemId = systemid,
						ShipId = gamedb.InsertShip(fleetId, this._heraldMoonDesignId, null, (ShipParams)0, null, 0)
					});
				}
			}
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			int systemid;
			if (!targetSystem.HasValue)
			{
				List<KeyValuePair<StarSystemInfo, Vector3>> list = EncounterTools.GetOutlyingStars(gamedb).ToList<KeyValuePair<StarSystemInfo, Vector3>>();
				List<KeyValuePair<StarSystemInfo, Vector3>> range = list.GetRange(0, (int)Math.Ceiling((double)((float)list.Count / 3f)));
				if (range.Count == 0)
				{
					return;
				}
				systemid = safeRandom.Choose(range).Key.ID;
			}
			else
			{
				systemid = targetSystem.Value;
			}
			gamedb.InsertIncomingGM(systemid, EasterEgg.GM_LOCUST_SWARM, gamedb.GetTurnCount() + 1);
			List<PlayerInfo> list2 = gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list2)
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, current.ID) > 0)
				{
					gamedb.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_LOCUST,
						EventMessage = TurnEventMessage.EM_INCOMING_LOCUST,
						PlayerID = current.ID,
						TurnNumber = gamedb.GetTurnCount()
					});
				}
			}
		}
		public void UpdateTurn(GameSession game, int id)
		{
			LocustSwarmInfo locustSwarmInfo = game.GameDatabase.GetLocustSwarmInfo(id);
			if (locustSwarmInfo == null || !locustSwarmInfo.FleetId.HasValue)
			{
				game.GameDatabase.RemoveEncounter(id);
				return;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(locustSwarmInfo.FleetId.Value);
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
			if (starSystemInfo != null && !starSystemInfo.IsDeepSpace)
			{
				locustSwarmInfo = this.SpendResources(game, locustSwarmInfo, fleetInfo);
			}
			game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
			if (game.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID) != null || game.isHostilesAtSystem(this.PlayerId, fleetInfo.SystemID))
			{
				game.GameDatabase.UpdateLocustSwarmInfo(locustSwarmInfo);
				return;
			}
			if (locustSwarmInfo.Resources >= game.AssetDatabase.GlobalLocustData.MinResourceSpawnAmount)
			{
				this.AddInstance(game.GameDatabase, game.AssetDatabase, new int?(starSystemInfo.ID));
				locustSwarmInfo.Resources -= game.AssetDatabase.GlobalLocustData.MinResourceSpawnAmount;
			}
			int num = 0;
			List<PlanetInfo> list = game.GameDatabase.GetStarSystemPlanetInfos(starSystemInfo.ID).ToList<PlanetInfo>();
			foreach (PlanetInfo current in list)
			{
				int num2 = Math.Min(current.Resources, game.AssetDatabase.GlobalLocustData.MaxSalvageRate - num);
				current.Resources -= num2;
				num += num2;
				game.GameDatabase.UpdatePlanet(current);
				if (num == game.AssetDatabase.GlobalLocustData.MaxSalvageRate)
				{
					break;
				}
			}
			locustSwarmInfo.Resources += num;
			int num3 = list.Sum((PlanetInfo x) => x.Resources);
			bool flag = false;
			List<LocustSwarmInfo> list2 = game.GameDatabase.GetLocustSwarmInfos().ToList<LocustSwarmInfo>();
			foreach (LocustSwarmInfo current2 in list2)
			{
				if (current2.Id != id && id >= current2.Id && current2.FleetId.HasValue)
				{
					FleetInfo fleetInfo2 = game.GameDatabase.GetFleetInfo(current2.FleetId.Value);
					if (fleetInfo2 != null && fleetInfo2.SystemID == fleetInfo.SystemID)
					{
						flag = true;
						break;
					}
				}
			}
			if (game.GameDatabase.HasEndOfFleshExpansion())
			{
				bool nestWaitingToGroupUp = (num3 == 0 && game.GameDatabase.GetLocustSwarmScoutTargetInfos().Count<LocustSwarmScoutTargetInfo>() > 0) || flag;
				this.UpdateScoutMission(game, locustSwarmInfo, fleetInfo, starSystemInfo, nestWaitingToGroupUp);
			}
			if ((num3 == 0 || flag) && !this.SetNextWorldTarget(game, locustSwarmInfo, fleetInfo, starSystemInfo))
			{
				game.GameDatabase.RemoveFleet(fleetInfo.ID);
				game.GameDatabase.RemoveEncounter(locustSwarmInfo.Id);
				return;
			}
			game.GameDatabase.UpdateLocustSwarmInfo(locustSwarmInfo);
		}
		private LocustSwarmInfo SpendResources(GameSession game, LocustSwarmInfo info, FleetInfo fi)
		{
			List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(fi.ID, true).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
                int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(game, current.DesignInfo, current.ID);
				int val = healthAndHealthMax[1] - healthAndHealthMax[0];
				int num = Math.Min(val, info.Resources);
				if (num > 0)
				{
					info.Resources -= num;
                    Kerberos.Sots.StarFleet.StarFleet.RepairShip(game.App, current, num);
				}
			}
			if (info.NumDrones < game.AssetDatabase.GlobalLocustData.MaxDrones)
			{
				int num2 = Math.Min(game.AssetDatabase.GlobalLocustData.MaxDrones - info.NumDrones, info.Resources / game.AssetDatabase.GlobalLocustData.DroneCost);
				info.NumDrones += num2;
				info.Resources -= num2 * game.AssetDatabase.GlobalLocustData.DroneCost;
			}
			if (game.GameDatabase.HasEndOfFleshExpansion() && info.Resources > 0)
			{
				List<LocustSwarmScoutInfo> list2 = game.GameDatabase.GetLocustSwarmScoutsForLocustNest(info.Id).ToList<LocustSwarmScoutInfo>();
				int num3 = Math.Max(game.AssetDatabase.GlobalLocustData.MinLocustScouts - list2.Count, 0);
				info.Resources = this.RepairScouts(game, list2, info.Resources);
				info.Resources = this.RepairScoutDrones(game, list2, info.Resources);
				int num4 = 0;
				while (num4 < num3 && info.Resources >= game.AssetDatabase.GlobalLocustData.LocustScoutCost)
				{
					LocustSwarmScoutInfo locustSwarmScoutInfo = new LocustSwarmScoutInfo();
					locustSwarmScoutInfo.LocustInfoId = info.Id;
					locustSwarmScoutInfo.NumDrones = game.AssetDatabase.GlobalLocustData.MaxDrones;
					locustSwarmScoutInfo.TargetSystemId = fi.SystemID;
					locustSwarmScoutInfo.ShipId = game.GameDatabase.InsertShip(info.FleetId.Value, this._heraldMoonDesignId, null, (ShipParams)0, null, 0);
					game.GameDatabase.InsertLocustSwarmScoutInfo(locustSwarmScoutInfo);
					info.Resources -= game.AssetDatabase.GlobalLocustData.LocustScoutCost;
					num4++;
				}
			}
			return info;
		}
		private int RepairScouts(GameSession game, List<LocustSwarmScoutInfo> scouts, int resources)
		{
			if (resources > 0 && scouts.Count > 0)
			{
				foreach (LocustSwarmScoutInfo current in scouts)
				{
					ShipInfo shipInfo = game.GameDatabase.GetShipInfo(current.ShipId, true);
                    int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(game, shipInfo.DesignInfo, shipInfo.ID);
					int val = healthAndHealthMax[1] - healthAndHealthMax[0];
					int num = Math.Min(val, resources);
					if (num > 0)
					{
						resources -= num;
                        Kerberos.Sots.StarFleet.StarFleet.RepairShip(game.App, shipInfo, num);
					}
					if (resources == 0)
					{
						break;
					}
				}
			}
			return resources;
		}
		private int RepairScoutDrones(GameSession game, List<LocustSwarmScoutInfo> scouts, int resources)
		{
			if (resources > 0 && scouts.Count > 0)
			{
				scouts.Sort((LocustSwarmScoutInfo x, LocustSwarmScoutInfo y) => x.NumDrones.CompareTo(y.NumDrones));
				List<LocustSwarmScoutInfo> list = new List<LocustSwarmScoutInfo>();
				int num = 10;
				int num2 = game.AssetDatabase.GlobalLocustData.MaxDrones / num + 1;
				bool flag = false;
				while (!flag && num2 > 0)
				{
					list.Clear();
					foreach (LocustSwarmScoutInfo current in scouts)
					{
						if (resources > 0)
						{
							int val = Math.Min(game.AssetDatabase.GlobalLocustData.MaxDrones - current.NumDrones, num);
							if (current.NumDrones < game.AssetDatabase.GlobalLocustData.MaxDrones)
							{
								int num3 = Math.Min(val, resources / game.AssetDatabase.GlobalLocustData.DroneCost);
								current.NumDrones += num3;
								resources -= num3 * game.AssetDatabase.GlobalLocustData.DroneCost;
							}
						}
						if (current.NumDrones == game.AssetDatabase.GlobalLocustData.MaxDrones || resources == 0)
						{
							list.Add(current);
						}
					}
					foreach (LocustSwarmScoutInfo current2 in list)
					{
						scouts.Remove(current2);
						game.GameDatabase.UpdateLocustSwarmScoutInfo(current2);
					}
					flag = (scouts.Count == 0 || resources == 0);
					num2--;
				}
			}
			return resources;
		}
		private void UpdateScoutMission(GameSession game, LocustSwarmInfo info, FleetInfo fleet, StarSystemInfo currentSystem, bool nestWaitingToGroupUp)
		{
			List<LocustSwarmScoutInfo> list = game.GameDatabase.GetLocustSwarmScoutsForLocustNest(info.Id).ToList<LocustSwarmScoutInfo>();
			if (list.Count == 0)
			{
				return;
			}
			List<int> list2 = new List<int>();
			List<LocustSwarmScoutTargetInfo> source = game.GameDatabase.GetLocustSwarmScoutTargetInfos().ToList<LocustSwarmScoutTargetInfo>();
			List<LocustSwarmScoutInfo> list3 = game.GameDatabase.GetLocustSwarmScoutInfos().ToList<LocustSwarmScoutInfo>();
			list2.AddRange((
				from x in source
				select x.SystemId).ToList<int>());
			foreach (LocustSwarmScoutInfo current in list3)
			{
				if (current.TargetSystemId != 0 && !list2.Contains(current.TargetSystemId))
				{
					list2.Add(current.TargetSystemId);
				}
			}
			int num = (
				from x in list
				where x.TargetSystemId == currentSystem.ID
				select x).Count<LocustSwarmScoutInfo>();
			foreach (LocustSwarmScoutInfo current2 in list)
			{
				ShipInfo shipInfo = game.GameDatabase.GetShipInfo(current2.ShipId, false);
				if (nestWaitingToGroupUp)
				{
					if (shipInfo.FleetID != fleet.ID && game.GameDatabase.GetMissionByFleetID(shipInfo.FleetID) == null)
					{
						game.GameDatabase.TransferShip(current2.ShipId, fleet.ID);
						Vector3 value = default(Vector3);
						value.Y = 0f;
						float num2 = (float)(((num + 1) % 5 + 1) / 2);
						float num3 = ((num + 1) % 2 == 0) ? 1f : -1f;
						value.Z = -300f * num2;
						value.X = num3 * 500f * num2;
						game.GameDatabase.UpdateShipFleetPosition(current2.ShipId, new Vector3?(value));
						current2.TargetSystemId = fleet.SystemID;
						game.GameDatabase.UpdateLocustSwarmScoutInfo(current2);
						game.GameDatabase.RemoveFleet(shipInfo.FleetID);
						num++;
					}
				}
				else
				{
					if (shipInfo.FleetID == fleet.ID || game.GameDatabase.GetMissionByFleetID(shipInfo.FleetID) == null)
					{
						int item = this.SetNextScoutTarget(game, info, currentSystem, current2, list2);
						if (!list2.Contains(item))
						{
							list2.Add(item);
						}
					}
				}
			}
		}
		private int SetNextScoutTarget(GameSession game, LocustSwarmInfo info, StarSystemInfo currentSystem, LocustSwarmScoutInfo scout, List<int> scoutedSystems)
		{
			List<int> previousTargets = game.GameDatabase.GetLocustSwarmTargets().ToList<int>();
			List<StarSystemInfo> closestStars = EncounterTools.GetClosestStars(game.GameDatabase, currentSystem);
			closestStars.RemoveAll((StarSystemInfo x) => previousTargets.Contains(x.ID));
			closestStars.RemoveAll((StarSystemInfo x) => scoutedSystems.Contains(x.ID));
			if (closestStars.Count > 0)
			{
				scout.TargetSystemId = closestStars.First<StarSystemInfo>().ID;
				int num = game.GameDatabase.InsertFleet(this.PlayerId, 0, currentSystem.ID, currentSystem.ID, "Locust Swarm Scout", FleetType.FL_NORMAL);
				game.GameDatabase.TransferShip(scout.ShipId, num);
				int missionID = game.GameDatabase.InsertMission(num, MissionType.SURVEY, scout.TargetSystemId, 0, 0, 0, false, null);
                Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.SURVEY, missionID, num, scout.TargetSystemId, 0, new int?(currentSystem.ID));
				game.GameDatabase.UpdateLocustSwarmScoutInfo(scout);
			}
			return scout.TargetSystemId;
		}
        private bool SetNextWorldTarget(GameSession game, LocustSwarmInfo info, FleetInfo fleet, StarSystemInfo currentSystem)
        {
            Func<StarSystemInfo, float> keySelector = null;
            List<int> previousTargets = game.GameDatabase.GetLocustSwarmTargets().ToList<int>();
            List<StarSystemInfo> source = new List<StarSystemInfo>();
            if (!game.GameDatabase.HasEndOfFleshExpansion())
            {
                source = EncounterTools.GetClosestStars(game.GameDatabase, currentSystem);
            }
            else
            {
                List<LocustSwarmScoutInfo> list2 = game.GameDatabase.GetLocustSwarmScoutsForLocustNest(info.Id).ToList<LocustSwarmScoutInfo>();
                if (list2.Count <= 0)
                {
                    source = EncounterTools.GetClosestStars(game.GameDatabase, currentSystem);
                }
                else
                {
                    bool flag = true;
                    foreach (LocustSwarmScoutInfo info2 in list2)
                    {
                        ShipInfo shipInfo = game.GameDatabase.GetShipInfo(info2.ShipId, false);
                        if (shipInfo != null)
                        {
                            if (shipInfo.FleetID != info.FleetId)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        return true;
                    }
                    bool flag2 = false;
                    List<StarSystemInfo> collection = new List<StarSystemInfo>();
                    foreach (LocustSwarmScoutTargetInfo info4 in game.GameDatabase.GetLocustSwarmScoutTargetInfos().ToList<LocustSwarmScoutTargetInfo>())
                    {
                        if (info4.SystemId != currentSystem.ID)
                        {
                            flag2 = true;
                            int num = 0;
                            foreach (PlanetInfo info5 in game.GameDatabase.GetStarSystemPlanetInfos(info4.SystemId).ToList<PlanetInfo>())
                            {
                                num += info5.Resources;
                            }
                            if (num > 0)
                            {
                                StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(info4.SystemId);
                                if (info4.IsHostile)
                                {
                                    collection.Add(starSystemInfo);
                                }
                                else
                                {
                                    source.Add(starSystemInfo);
                                }
                            }
                        }
                    }
                    if (!flag2)
                    {
                        source = EncounterTools.GetClosestStars(game.GameDatabase, currentSystem);
                    }
                    if (source.Count == 0)
                    {
                        source.AddRange(collection);
                    }
                    if (keySelector == null)
                    {
                        keySelector = x => (x.Origin - currentSystem.Origin).LengthSquared;
                    }
                    source.OrderBy<StarSystemInfo, float>(keySelector);
                }
            }
            source.RemoveAll(x => previousTargets.Contains(x.ID));
            if (source.Count > 0)
            {
                game.GameDatabase.InsertLocustSwarmTarget(info.Id, fleet.SystemID);
                int iD = source.First<StarSystemInfo>().ID;
                int missionID = game.GameDatabase.InsertMission(fleet.ID, MissionType.STRIKE, source.First<StarSystemInfo>().ID, 0, 0, 0, false, null);
                game.GameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(iD));
                this.UpdateScoutedSystems(game, iD);
                return true;
            }
            return false;
        }
        public void UpdateScoutedSystems(GameSession game, int systemID)
		{
			if (game.GameDatabase.GetLocustSwarmScoutTargetInfos().Any((LocustSwarmScoutTargetInfo x) => x.SystemId == systemID))
			{
				return;
			}
			LocustSwarmScoutTargetInfo locustSwarmScoutTargetInfo = new LocustSwarmScoutTargetInfo();
			locustSwarmScoutTargetInfo.SystemId = systemID;
			locustSwarmScoutTargetInfo.IsHostile = game.isHostilesAtSystem(this.PlayerId, systemID);
			game.GameDatabase.InsertLocustSwarmScoutTargetInfo(locustSwarmScoutTargetInfo);
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, int systemID)
		{
			return Locust.GetSpawnTransform(app, systemID);
		}
		public static Matrix GetSpawnTransform(App app, int systemId)
		{
			bool flag = false;
			float num = 0f;
			float s = 0f;
			OrbitalObjectInfo orbitalObjectInfo = null;
			Vector3 v = Vector3.Zero;
			Vector3? vector = null;
			foreach (OrbitalObjectInfo current in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
			{
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(current.ID);
				if (!flag || colonyInfoForPlanet != null)
				{
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(current.ID);
					float num2 = 1000f;
					if (planetInfo != null)
					{
						num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					}
					Vector3 position = app.GameDatabase.GetOrbitalTransform(current.ID).Position;
					float num3 = position.Length + num2;
					if (num3 > num || (!flag && colonyInfoForPlanet != null))
					{
						orbitalObjectInfo = current;
						num = num3;
						flag = (colonyInfoForPlanet != null);
						v = position;
						s = num2 + 10000f;
						if (current.ParentID.HasValue && current.ParentID.Value != 0)
						{
							vector = new Vector3?(app.GameDatabase.GetOrbitalTransform(current.ID).Position);
						}
						else
						{
							vector = null;
						}
						if (flag)
						{
							break;
						}
					}
				}
			}
			if (orbitalObjectInfo == null)
			{
				return Matrix.Identity;
			}
			Vector3 vector2 = Vector3.Zero;
			if (vector.HasValue)
			{
				Matrix matrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Normalize(vector.Value), Vector3.UnitY);
				Vector3 v2 = Vector3.Normalize(v - vector.Value);
				vector2 = matrix.Right * s;
				if (Vector3.Dot(matrix.Right, v2) < 0f)
				{
					vector2 *= -1f;
				}
			}
			Vector3 v3 = -v;
			v3.Normalize();
			Vector3 vector3 = v - v3 * s + vector2;
			return Matrix.CreateWorld(vector3, Vector3.Normalize(v - vector3), Vector3.UnitY);
		}
	}
}
