using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAP)]
	internal class StarMap : StarMapBase
	{
		private class PlayerSystemPair
		{
			public int PlayerID;
			public int SystemID;
		}
		public static bool AlwaysInRange;
		private readonly GameSession _sim;
		private readonly GameDatabase _db;
		private StarMapViewFilter _viewFilter;
		public StarMapViewFilter ViewFilter
		{
			get
			{
				return this._viewFilter;
			}
			set
			{
				this._viewFilter = value;
				this.PostSetProp("ViewFilter", new object[]
				{
					value
				});
			}
		}
		public StarMap(App game, GameSession sim, Sky sky) : base(game, sky)
		{
			this._sim = sim;
			this._db = sim.GameDatabase;
		}
		protected override void GetAdditionalParams(List<object> parms)
		{
		}
		protected override void UpdateProvince(StarMapProvince o, ProvinceInfo oi, StarMapBase.SyncContext context)
		{
			ProvinceInfo provinceInfo = this._db.GetProvinceInfo(oi.ID);
			oi.Name = provinceInfo.Name;
			oi.PlayerID = provinceInfo.PlayerID;
			oi.CapitalSystemID = provinceInfo.CapitalSystemID;
			o.SetPosition(this._db.GetStarSystemOrigin(oi.CapitalSystemID));
			if (this._db.IsStarSystemVisibleToPlayer(this._sim.LocalPlayer.ID, oi.CapitalSystemID))
			{
				o.SetCapital(this.Systems.Reverse[oi.CapitalSystemID]);
			}
			o.SetPlayer(this._sim.GetPlayerObject(oi.PlayerID));
		}
		protected override StarMapProvince CreateProvince(GameObjectSet gos, ProvinceInfo oi, StarMapBase.SyncContext context)
		{
			StarMapProvince starMapProvince = new StarMapProvince(base.App);
			starMapProvince.SetLabel(oi.Name);
			gos.Add(starMapProvince);
			return starMapProvince;
		}
		protected override StarMapFilter CreateFilter(GameObjectSet gos, StarMapViewFilter type, StarMapBase.SyncContext context)
		{
			StarMapFilter starMapFilter = new StarMapFilter(base.App);
			starMapFilter.SetFilterType(type);
			gos.Add(starMapFilter);
			return starMapFilter;
		}
		protected override StarMapNodeLine CreateNodeLine(GameObjectSet gos, NodeLineInfo oi, StarMapBase.SyncContext context)
		{
			StarMapNodeLine starMapNodeLine = new StarMapNodeLine(base.App, this._db.GetStarSystemOrigin(oi.System1ID), this._db.GetStarSystemOrigin(oi.System2ID));
			gos.Add(starMapNodeLine);
			return starMapNodeLine;
		}
		protected override StarMapProp CreateProp(GameObjectSet gos, StellarPropInfo oi, StarMapBase.SyncContext context)
		{
			StarMapProp starMapProp = new StarMapProp(base.App, oi.AssetPath, oi.Transform.Position, oi.Transform.EulerAngles, 1f);
			gos.Add(starMapProp);
			return starMapProp;
		}
		protected override StarMapTerrain CreateTerrain(GameObjectSet gos, TerrainInfo oi, StarMapBase.SyncContext context)
		{
			StarMapTerrain starMapTerrain = new StarMapTerrain(base.App, oi.Origin, oi.Name);
			gos.Add(starMapTerrain);
			return starMapTerrain;
		}
		public void UpdateSystemTrade(int systemID)
		{
			KeyValuePair<StarMapSystem, int>? keyValuePair = new KeyValuePair<StarMapSystem, int>?(this.Systems.Forward.FirstOrDefault((KeyValuePair<StarMapSystem, int> x) => x.Value == systemID));
			if (keyValuePair.HasValue)
			{
				keyValuePair.Value.Key.SetProductionValues(this._sim.GetExportCapacity(keyValuePair.Value.Value), this._sim.GetMaxExportCapacity(keyValuePair.Value.Value));
			}
		}
		protected override void UpdateSystem(StarMapSystem o, StarSystemInfo systemInfo, StarMapBase.SyncContext context)
		{
			int? starSystemProvinceID = this._db.GetStarSystemProvinceID(systemInfo.ID);
			o.SetProvince(starSystemProvinceID.HasValue ? this.Provinces.Reverse[starSystemProvinceID.Value] : null);
			o.SetPosition(systemInfo.Origin);
			o.SetTerrain(systemInfo.TerrainID.HasValue ? this.Terrain.Reverse[systemInfo.TerrainID.Value] : null);
			IEnumerable<int> starSystemOrbitalObjectIDs = this._db.GetStarSystemOrbitalObjectIDs(systemInfo.ID);
			List<int> list = new List<int>();
			bool flag = false;
			List<StationInfo> list2 = (
				from x in this._db.GetStationForSystem(systemInfo.ID)
				where x.DesignInfo.StationType == StationType.NAVAL
				select x).ToList<StationInfo>();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			bool flag2 = false;
			foreach (int current in starSystemOrbitalObjectIDs)
			{
				AIColonyIntel ci = this._db.GetColonyIntelForPlanet(this._sim.LocalPlayer.ID, current);
				if (ci != null)
				{
					if (!dictionary.ContainsKey(ci.OwningPlayerID))
					{
						dictionary.Add(ci.OwningPlayerID, 0);
					}
					Dictionary<int, int> dictionary2;
					int owningPlayerID;
					(dictionary2 = dictionary)[owningPlayerID = ci.OwningPlayerID] = dictionary2[owningPlayerID] + 1;
					if (ci.OwningPlayerID == this._sim.LocalPlayer.ID)
					{
						flag = true;
					}
					list.Add(ci.OwningPlayerID);
					List<TreatyInfo> source = this._sim.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>();
					source = (
						from x in source
						where x.Type == TreatyType.Trade
						select x).ToList<TreatyInfo>();
					if (flag || source.Any((TreatyInfo x) => (x.InitiatingPlayerId == this._sim.LocalPlayer.ID && x.ReceivingPlayerId == ci.OwningPlayerID) || (x.ReceivingPlayerId == this._sim.LocalPlayer.ID && x.InitiatingPlayerId == ci.OwningPlayerID)))
					{
						flag2 = true;
					}
				}
			}
			if (dictionary.Count == 0)
			{
				foreach (StationInfo current2 in list2)
				{
					if (current2.PlayerID == this._sim.LocalPlayer.ID)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				float supportRange = GameSession.GetSupportRange(this._db.AssetDatabase, this._db, this._sim.LocalPlayer.ID);
				o.SetSupportRange(supportRange);
			}
			int? systemOwningPlayer = this._db.GetSystemOwningPlayer(systemInfo.ID);
			if (systemOwningPlayer.HasValue && this._sim.GameDatabase.IsStarSystemVisibleToPlayer(this._sim.LocalPlayer.ID, systemInfo.ID) && StarMap.IsInRange(this._sim.GameDatabase, this._sim.LocalPlayer.ID, systemInfo.ID))
			{
				o.SetPlayerBadge(Path.GetFileNameWithoutExtension(this._db.GetPlayerInfo(systemOwningPlayer.Value).BadgeAssetPath));
				o.SetOwningPlayer(this._sim.GetPlayerObject(systemOwningPlayer.Value));
			}
			else
			{
				o.SetPlayerBadge("");
				o.SetOwningPlayer(null);
			}
			list.Sort();
			o.SetPlayers((
				from playerId in list
				select this._sim.GetPlayerObject(playerId)).ToArray<Player>());
			List<PlayerInfo> playerInfos = context.PlayerInfos;
			List<Player> list3 = new List<Player>();
			List<Player> list4 = new List<Player>();
			foreach (PlayerInfo current3 in playerInfos)
			{
				if (this._db.SystemHasGate(systemInfo.ID, current3.ID) && (current3.ID == this._sim.LocalPlayer.ID || (this._db.IsSurveyed(this._sim.LocalPlayer.ID, systemInfo.ID) && StarMap.IsInRange(this._db, current3.ID, systemInfo.ID))))
				{
					list3.Add(this._sim.GetPlayerObject(current3.ID));
				}
				if (this._db.SystemHasAccelerator(systemInfo.ID, current3.ID) && (current3.ID == this._sim.LocalPlayer.ID || (this._db.IsSurveyed(this._sim.LocalPlayer.ID, systemInfo.ID) && StarMap.IsInRange(this._db, current3.ID, systemInfo.ID))))
				{
					list4.Add(this._sim.GetPlayerObject(current3.ID));
				}
				IEnumerable<StationInfo> stationForSystemAndPlayer = this._db.GetStationForSystemAndPlayer(systemInfo.ID, current3.ID);
				if (current3.ID == this._sim.LocalPlayer.ID)
				{
					o.SetHasNavalStation(stationForSystemAndPlayer.Any((StationInfo x) => x.PlayerID == this._sim.LocalPlayer.ID && x.DesignInfo.StationLevel > 0 && x.DesignInfo.StationType == StationType.NAVAL));
					o.SetHasScienceStation(stationForSystemAndPlayer.Any((StationInfo x) => x.PlayerID == this._sim.LocalPlayer.ID && x.DesignInfo.StationLevel > 0 && x.DesignInfo.StationType == StationType.SCIENCE));
					o.SetHasTradeStation(stationForSystemAndPlayer.Any((StationInfo x) => x.PlayerID == this._sim.LocalPlayer.ID && x.DesignInfo.StationLevel > 0 && x.DesignInfo.StationType == StationType.CIVILIAN));
					o.SetHasDiploStation(stationForSystemAndPlayer.Any((StationInfo x) => x.PlayerID == this._sim.LocalPlayer.ID && x.DesignInfo.StationLevel > 0 && x.DesignInfo.StationType == StationType.DIPLOMATIC));
				}
				o.SetStationCapacity(this._db.GetNumberMaxStationsSupportedBySystem(this._sim, systemInfo.ID, this._sim.LocalPlayer.ID));
				if (current3.ID == this._sim.LocalPlayer.ID && systemOwningPlayer.HasValue)
				{
					int systemSupportedCruiserEquivalent = this._db.GetSystemSupportedCruiserEquivalent(this._sim, systemInfo.ID, current3.ID);
					int remainingSupportPoints = this._db.GetRemainingSupportPoints(this._sim, systemInfo.ID, current3.ID);
					if (systemOwningPlayer.Value == this._sim.LocalPlayer.ID)
					{
						o.SetNavalCapacity(systemSupportedCruiserEquivalent);
						o.SetNavalUsage(systemSupportedCruiserEquivalent - remainingSupportPoints);
					}
					else
					{
						o.SetNavalCapacity(0);
						o.SetNavalUsage(0);
					}
				}
			}
			o.SetColonyTrapped((
				from x in this._sim.GameDatabase.GetColonyTrapInfosAtSystem(systemInfo.ID)
				where this._sim.GameDatabase.GetFleetInfo(x.FleetID) != null && this._sim.GameDatabase.GetFleetInfo(x.FleetID).PlayerID == this._sim.LocalPlayer.ID
				select x).Any<ColonyTrapInfo>());
			o.SetPlayersWithGates(list3.ToArray());
			o.SetPlayersWithAccelerators(list4.ToArray());
			o.SetSensorRange(this._sim.GameDatabase.GetSystemStratSensorRange(systemInfo.ID, this._sim.LocalPlayer.ID));
			TradeResultsTable tradeResultsTable = this._sim.GameDatabase.GetTradeResultsTable();
			TradeResultsTable lastTradeResultsHistoryTable = this._sim.GameDatabase.GetLastTradeResultsHistoryTable();
			if (flag2 && tradeResultsTable.TradeNodes.ContainsKey(systemInfo.ID))
			{
				if (lastTradeResultsHistoryTable.TradeNodes.ContainsKey(systemInfo.ID))
				{
					o.SetTradeValues(this._sim, tradeResultsTable.TradeNodes[systemInfo.ID], lastTradeResultsHistoryTable.TradeNodes[systemInfo.ID], systemInfo.ID);
				}
				else
				{
					o.SetTradeValues(this._sim, tradeResultsTable.TradeNodes[systemInfo.ID], new TradeNode(), systemInfo.ID);
				}
			}
			else
			{
				o.SetTradeValues(this._sim, new TradeNode(), new TradeNode(), systemInfo.ID);
			}
			int lastTurnExploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._sim.LocalPlayer.ID, systemInfo.ID);
			int turnCount = this._db.GetTurnCount();
			o.SetIsSurveyed(lastTurnExploredByPlayer != 0);
			this.SetSystemHasBeenRecentlySurveyed(this.Systems.Reverse[systemInfo.ID], lastTurnExploredByPlayer != 0 && turnCount - lastTurnExploredByPlayer <= 5 && !flag);
			this.SetSystemHasRecentCombat(this.Systems.Reverse[systemInfo.ID], this._sim.CombatData.GetFirstCombatInSystem(this._sim.GameDatabase, systemInfo.ID, this._sim.GameDatabase.GetTurnCount() - 1) != null);
			this.SetSystemIsMissionTarget(this.Systems.Reverse[systemInfo.ID], this._db.GetPlayerMissionInfosAtSystem(this._sim.LocalPlayer.ID, systemInfo.ID).Any<MissionInfo>(), this._db.GetPlayerInfo(this._sim.LocalPlayer.ID).PrimaryColor);
			bool value = SuperNova.IsPlayerSystemsInSuperNovaEffectRanges(this._db, this._sim.LocalPlayer.ID, systemInfo.ID) || NeutronStar.IsPlayerSystemsInNeutronStarEffectRanges(this._sim, this._sim.LocalPlayer.ID, systemInfo.ID);
			this.SetSystemRequriesSuperNovaWarning(this.Systems.Reverse[systemInfo.ID], value);
			this.UpdateSystemTrade(systemInfo.ID);
			o.SetHasLoaGate(this._db.GetFleetInfoBySystemID(systemInfo.ID, FleetType.FL_ACCELERATOR).Any<FleetInfo>());
		}
		public void ClearSystemEffects()
		{
			List<StarSystemInfo> list = base.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			foreach (StarSystemInfo current in list)
			{
				if (this.Systems.Reverse.Keys.Contains(current.ID))
				{
					this.SetSystemIsMissionTarget(this.Systems.Reverse[current.ID], false, this._db.GetPlayerInfo(this._sim.LocalPlayer.ID).PrimaryColor);
					this.SetSystemHasRecentCombat(this.Systems.Reverse[current.ID], false);
					this.SetSystemHasBeenRecentlySurveyed(this.Systems.Reverse[current.ID], false);
					this.SetSystemRequriesSuperNovaWarning(this.Systems.Reverse[current.ID], false);
				}
			}
		}
		public void SetMissionEffectTarget(StarSystemInfo sys, bool value)
		{
			if (!this.Systems.Reverse.Keys.Contains(sys.ID))
			{
				return;
			}
			this.SetSystemIsMissionTarget(this.Systems.Reverse[sys.ID], value, this._db.GetPlayerInfo(this._sim.LocalPlayer.ID).PrimaryColor);
		}
		protected override StarMapSystem CreateSystem(GameObjectSet gos, StarSystemInfo oi, StarMapBase.SyncContext context)
		{
			StellarClass stellarClass = StellarClass.Parse(oi.StellarClass);
			StarDisplayParams displayParams = StarHelper.GetDisplayParams(stellarClass);
			StarMapSystem starMapSystem = new StarMapSystem(base.App, displayParams.AssetPath, oi.Origin, StarHelper.CalcRadius(stellarClass.Size) / StarSystemVars.Instance.StarRadiusIa, oi.Name);
			starMapSystem.SetSensorRange(this._sim.GameDatabase.GetSystemStratSensorRange(oi.ID, this._sim.LocalPlayer.ID));
			gos.Add(starMapSystem);
			return starMapSystem;
		}
		protected override void UpdateFleet(StarMapFleet o, FleetInfo oi, StarMapBase.SyncContext context)
		{
			FleetLocation fleetLocation = this._db.GetFleetLocation(oi.ID, true);
			o.SetSystemID(fleetLocation.SystemID);
			o.SetPosition(fleetLocation.Coords);
			o.FleetID = oi.ID;
			if (fleetLocation.Direction.HasValue)
			{
				o.SetDirection(fleetLocation.Direction.Value);
			}
			MoveOrderInfo moveOrderInfoByFleetID = base.App.GameDatabase.GetMoveOrderInfoByFleetID(oi.ID);
			if (moveOrderInfoByFleetID != null && moveOrderInfoByFleetID.Progress <= 0f)
			{
				StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fleetLocation.SystemID);
				if (starSystemInfo != null)
				{
					o.SetPosition(starSystemInfo.Origin);
				}
			}
			o.SetIsInTransit(moveOrderInfoByFleetID != null && (moveOrderInfoByFleetID.Progress > 0f || (oi.Type == FleetType.FL_ACCELERATOR && (oi.SystemID == 0 || this._sim.GameDatabase.GetStarSystemInfo(oi.SystemID).IsDeepSpace))));
			o.SetPlayer(this._sim.GetPlayerObject(oi.PlayerID));
			if (oi.PlayerID == this._sim.LocalPlayer.ID)
			{
				o.SetSensorRange(GameSession.GetFleetSensorRange(base.App.AssetDatabase, this._db, oi.ID));
			}
			if (o.ObjectStatus == GameObjectStatus.Ready)
			{
				o.PostSetActive(true);
			}
			o.SetIsLoaGate(oi.Type == FleetType.FL_ACCELERATOR);
		}
		protected override StarMapFleet CreateFleet(GameObjectSet gos, FleetInfo oi, StarMapBase.SyncContext context)
		{
			int playerFactionID = this._db.GetPlayerFactionID(oi.PlayerID);
			string factionName = this._db.GetFactionName(playerFactionID);
			AssetDatabase.MiniMapData miniShipDirectory = this._db.AssetDatabase.GetMiniShipDirectory(this._sim.App, factionName, oi.Type, this._db.GetShipInfoByFleetID(oi.ID, false).ToList<ShipInfo>());
			StarMapFleet starMapFleet = new StarMapFleet(base.App, miniShipDirectory.ID);
			if (oi.PlayerID == this._sim.LocalPlayer.ID)
			{
				starMapFleet.SetSensorRange(GameSession.GetFleetSensorRange(base.App.AssetDatabase, this._db, oi.ID));
			}
			this.UpdateFleet(starMapFleet, oi, context);
			gos.Add(starMapFleet);
			return starMapFleet;
		}
		protected override void OnInitialize(GameObjectSet gos, params object[] parms)
		{
			if (this._sim.IsMultiplayer)
			{
				this.PostObjectAddObjects(new IGameObject[]
				{
					new StarMapServerName(base.App, new Vector3(0f, 0f, 0f), base.App.Network.GameName)
				});
			}
			StarMapBase.SyncContext context = new StarMapBase.SyncContext(this._db);
			this.CreateFilter(gos, StarMapViewFilter.VF_SUPPORT_RANGE, context);
			this.CreateFilter(gos, StarMapViewFilter.VF_SENSOR_RANGE, context);
			this.CreateFilter(gos, StarMapViewFilter.VF_TRADE, context);
			this.Props.Sync(gos, this._db.GetStellarProps(), context, false);
			this.Sync(gos);
		}
		public void Sync(GameObjectSet gos)
		{
			StarMapBase.SyncContext context = new StarMapBase.SyncContext(this._db);
			List<StarSystemInfo> list = this._db.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<StarSystemInfo> list2 = (
				from x in list
				where !x.IsVisible
				select x).ToList<StarSystemInfo>();
			foreach (StarSystemInfo current in list2)
			{
				if (!StarMap.IsInRange(this._db, this._sim.LocalPlayer.ID, current.ID))
				{
					list.Remove(current);
				}
				else
				{
					current.IsVisible = true;
					this._db.UpdateStarSystemVisible(current.ID, true);
				}
			}
			IEnumerable<StarMapTerrain> source = this.Terrain.Sync(gos, this._db.GetTerrainInfos(), context, false);
			IEnumerable<StarMapProvince> source2 = this.Provinces.Sync(gos, this._db.GetProvinceInfos(), context, false);
			IEnumerable<StarMapSystem> source3 = this.Systems.Sync(gos, list, context, false);
			List<FleetInfo> list3 = (
				from x in this._db.GetFleetInfos(FleetType.FL_NORMAL | FleetType.FL_CARAVAN | FleetType.FL_ACCELERATOR)
				where !x.IsReserveFleet && this._sim.GetPlayerObject(x.PlayerID) != null
				select x).ToList<FleetInfo>();
			int swarmerPlayer = (this._sim.ScriptModules != null && this._sim.ScriptModules.Swarmers != null) ? this._sim.ScriptModules.Swarmers.PlayerID : 0;
			if (swarmerPlayer != 0)
			{
				List<FleetInfo> list4 = (
					from x in list3
					where x.PlayerID == swarmerPlayer && x.Name.Contains("Swarm")
					select x).ToList<FleetInfo>();
				foreach (FleetInfo current2 in list4)
				{
					list3.Remove(current2);
				}
			}
			List<FleetInfo> list5 = (
				from x in list3
				where this._db.GetMissionByFleetID(x.ID) != null && this._db.GetMissionByFleetID(x.ID).Type == MissionType.PIRACY
				select x).ToList<FleetInfo>();
			foreach (FleetInfo current3 in list5)
			{
				if (!this._db.PirateFleetVisibleToPlayer(current3.ID, this._sim.LocalPlayer.ID))
				{
					list3.Remove(current3);
				}
			}
			IEnumerable<StarMapFleet> source4 = this.Fleets.Sync(gos, list3, context, true);
			this.PostObjectAddObjects(source2.ToArray<StarMapProvince>());
			this.PostObjectAddObjects(source3.ToArray<StarMapSystem>());
			this.PostObjectAddObjects(source4.ToArray<StarMapFleet>());
			if (this._sim.LocalPlayer.Faction.Name == "human")
			{
				IEnumerable<StarMapNodeLine> source5 = this.NodeLines.Sync(gos, (
					from x in this._db.GetExploredNodeLines(this._sim.LocalPlayer.ID)
					where x.IsPermenant
					select x).ToList<NodeLineInfo>(), context, false);
				this.PostObjectAddObjects(source5.ToArray<StarMapNodeLine>());
			}
			this.PostObjectAddObjects(source.ToArray<StarMapTerrain>());
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			List<StarMap.PlayerSystemPair> list6 = new List<StarMap.PlayerSystemPair>();
			foreach (StarMapFleet current4 in this.Fleets.Forward.Keys)
			{
				this._sim.GameDatabase.IsStealthFleet(current4.FleetID);
				if (current4.InTransit)
				{
					current4.SetVisible(StarMap.IsInRange(base.App.Game.GameDatabase, this._sim.LocalPlayer.ID, current4.Position, 1f, null));
				}
				else
				{
					dictionary[current4.SystemID] = 0;
					bool flag = false;
					foreach (StarMap.PlayerSystemPair current5 in list6)
					{
						if (current5.PlayerID == current4.PlayerID && current5.SystemID == current4.SystemID)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						current4.SetVisible(false);
					}
					else
					{
						current4.SetVisible(StarMap.IsInRange(base.App.Game.GameDatabase, this._sim.LocalPlayer.ID, current4.Position, 1f, null));
						list6.Add(new StarMap.PlayerSystemPair
						{
							PlayerID = current4.PlayerID,
							SystemID = current4.SystemID
						});
					}
				}
			}
			foreach (StarMapFleet current6 in this.Fleets.Forward.Keys)
			{
				if (!current6.InTransit && current6.IsVisible)
				{
					current6.SetSystemFleetIndex(dictionary[current6.SystemID]);
					Dictionary<int, int> dictionary2;
					int systemID;
					(dictionary2 = dictionary)[systemID = current6.SystemID] = dictionary2[systemID] + 1;
				}
			}
			foreach (StarMapFleet current7 in this.Fleets.Forward.Keys)
			{
				if (!current7.InTransit && current7.IsVisible)
				{
					current7.SetSystemFleetCount(dictionary[current7.SystemID]);
				}
			}
			IEnumerable<HomeworldInfo> homeworlds = this._db.GetHomeworlds();
			foreach (HomeworldInfo hw in homeworlds)
			{
				ColonyInfo hwci = this._db.GetColonyInfo(hw.ColonyID);
				if (hwci != null && list.Any((StarSystemInfo x) => x.ID == hw.SystemID && hw.PlayerID == hwci.PlayerID))
				{
					this.PostSetProp("Homeworld", new object[]
					{
						this._sim.GetPlayerObject(hw.PlayerID).ObjectID,
						this.Systems.Reverse[hw.SystemID].ObjectID
					});
				}
			}
			foreach (StarSystemInfo current8 in list)
			{
				this.PostSetProp("ProvinceCapitalEffect", new object[]
				{
					false,
					this.Systems.Reverse[current8.ID].ObjectID
				});
			}
			List<ProvinceInfo> list7 = this._db.GetProvinceInfos().ToList<ProvinceInfo>();
			foreach (ProvinceInfo p in list7)
			{
				if (list.Any((StarSystemInfo x) => x.ID == p.CapitalSystemID))
				{
					this.PostSetProp("ProvinceCapitalEffect", new object[]
					{
						true,
						this.Systems.Reverse[p.CapitalSystemID].ObjectID
					});
				}
			}
			this.PostSetProp("RegenerateTerrain", new object[0]);
			this.PostSetProp("RegenerateBorders", new object[0]);
			this.PostSetProp("RegenerateFilters", new object[0]);
		}
		public void Select(IGameObject o)
		{
			this.PostSetProp("Selected", o.GetObjectID());
		}
		private void SetSystemIsMissionTarget(StarMapSystem system, bool value, Vector3 Color)
		{
			this.PostSetProp("MissionTarget", new object[]
			{
				system.ObjectID,
				value,
				Color.X,
				Color.Y,
				Color.Z
			});
		}
		private void SetSystemHasBeenRecentlySurveyed(StarMapSystem system, bool value)
		{
			this.PostSetProp("RecentSurvey", new object[]
			{
				system.ObjectID,
				value
			});
		}
		private void SetSystemHasRecentCombat(StarMapSystem system, bool value)
		{
			this.PostSetProp("RecentCombatEffect", new object[]
			{
				system.ObjectID,
				value
			});
		}
		private void SetSystemRequriesSuperNovaWarning(StarMapSystem system, bool value)
		{
			this.PostSetProp("SuperNovaWarningEffect", new object[]
			{
				system.ObjectID,
				value
			});
		}
		public static bool IsInRange(GameDatabase db, int playerid, int systemId)
		{
			return StarMap.IsInRange(db, playerid, db.GetStarSystemOrigin(systemId), 1f, null);
		}
		public static bool IsInRange(GameDatabase db, int playerid, StarSystemInfo ssi, Dictionary<int, List<ShipInfo>> cachedFleetShips = null)
		{
			return StarMap.IsInRange(db, playerid, ssi.Origin, 1f, cachedFleetShips);
		}
		public static bool IsInRange(GameDatabase db, int playerid, Vector3 loc, float rangeMultiplier = 1f, Dictionary<int, List<ShipInfo>> cachedFleetShips = null)
		{
			if (StarMap.AlwaysInRange)
			{
				return true;
			}
			List<int> list = (
				from x in db.GetPlayerInfos()
				where x.ID == playerid || db.GetDiplomacyStateBetweenPlayers(x.ID, playerid) == DiplomacyState.ALLIED
				select x.ID).ToList<int>();
			List<int> list2 = new List<int>();
			foreach (int current in list)
			{
				list2.AddRange(db.GetPlayerColonySystemIDs(current));
			}
			foreach (int current2 in list2)
			{
				StarSystemInfo starSystemInfo = db.GetStarSystemInfo(current2);
				if (!(starSystemInfo == null))
				{
					float systemStratSensorRange = db.GetSystemStratSensorRange(current2, playerid);
					float length = (starSystemInfo.Origin - loc).Length;
					if (length <= systemStratSensorRange * rangeMultiplier)
					{
						bool result = true;
						return result;
					}
				}
			}
			List<FleetInfo> list3 = new List<FleetInfo>();
			foreach (int current3 in list)
			{
				list3.AddRange(db.GetFleetInfosByPlayerID(current3, FleetType.FL_NORMAL | FleetType.FL_DEFENSE | FleetType.FL_GATE | FleetType.FL_CARAVAN | FleetType.FL_ACCELERATOR));
			}
			float num = db.AssetDatabase.DefaultStratSensorRange;
			foreach (FleetInfo current4 in list3)
			{
				List<ShipInfo> cachedShips = null;
				if (cachedFleetShips != null && !cachedFleetShips.TryGetValue(current4.ID, out cachedShips))
				{
					cachedShips = new List<ShipInfo>();
				}
				num = GameSession.GetFleetSensorRange(db.AssetDatabase, db, current4, cachedShips);
				if (num == 0f && db.GetShipsByFleetID(current4.ID).Any<int>())
				{
					num = db.AssetDatabase.DefaultStratSensorRange;
				}
				FleetLocation fleetLocation = db.GetFleetLocation(current4.ID, false);
				Vector3 coords = fleetLocation.Coords;
				float length2 = (coords - loc).Length;
				if (length2 <= num * rangeMultiplier)
				{
					bool result = true;
					return result;
				}
			}
			return false;
		}
		public static bool IsInRange(GameDatabase db, int playerid, Vector3 loc, Dictionary<FleetInfo, List<ShipInfo>> fleetShips, List<StarSystemInfo> colonySystems)
		{
			if (StarMap.AlwaysInRange)
			{
				return true;
			}
			foreach (StarSystemInfo current in colonySystems)
			{
				float systemStratSensorRange = db.GetSystemStratSensorRange(current.ID, playerid);
				float length = (current.Origin - loc).Length;
				if (length <= systemStratSensorRange)
				{
					bool result = true;
					return result;
				}
			}
			float num = db.AssetDatabase.DefaultStratSensorRange;
			foreach (KeyValuePair<FleetInfo, List<ShipInfo>> current2 in fleetShips)
			{
				num = GameSession.GetFleetSensorRange(db.AssetDatabase, db, current2.Key, current2.Value);
				if (num == 0f && current2.Value.Count > 0)
				{
					num = db.AssetDatabase.DefaultStratSensorRange;
				}
				FleetLocation fleetLocation = db.GetFleetLocation(current2.Key.ID, false);
				Vector3 coords = fleetLocation.Coords;
				float length2 = (coords - loc).Length;
				if (length2 <= num)
				{
					bool result = true;
					return result;
				}
			}
			return false;
		}
	}
}
