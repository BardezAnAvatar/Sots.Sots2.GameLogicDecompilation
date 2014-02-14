using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class DefenseManagerState : BasicStarSystemState, IKeyBindListener
	{
		private const string UIMissionAdmiralName = "gameAdmiralName";
		private const string UIMissionAdmiralFleet = "gameAdmiralFleet";
		private const string UIMissionAdmiralSkills = "gameAdmiralSkills";
		private const string UIMissionAdmiralAvatar = "gameAdmiralAvatar";
		protected const int UIItemMissionTotalTime = 0;
		protected const int UIItemMissionTravelTime = 1;
		protected const int UIItemMissionTime = 2;
		protected const int UIItemMissionBuildTime = 3;
		protected const int UIItemMissionCostSeparator = 4;
		protected const int UIItemMissionCost = 5;
		protected const int UIItemMissionSupportTime = 6;
		private int _targetSystemID;
		private int _selectedPlanetID;
		private GameObjectSet _dmcrits;
		private DefenseManager _manager;
		private List<IGameObject> _pendingObjects;
		private bool _finishing;
		private FleetWidget _fleetWidget;
		private DefenseWidget _defenseWidget;
		private CombatInput _input;
		private static readonly string UIExitButton = "gameExitButton";
		public DefenseManagerState(App game) : base(game)
		{
		}
		protected override void OnBack()
		{
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame();
				this._targetSystemID = base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID).SystemID;
				DesignInfo designInfo = new DesignInfo();
				designInfo.PlayerID = base.App.LocalPlayer.ID;
				designInfo.DesignSections = new DesignSectionInfo[1];
				designInfo.DesignSections[0] = new DesignSectionInfo
				{
					DesignInfo = designInfo
				};
				designInfo.DesignSections[0].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\sn_drone_satellite.section";
				int designID = base.App.GameDatabase.InsertDesignByDesignInfo(designInfo);
				int iD = base.App.GameDatabase.InsertOrGetDefenseFleetInfo(this._targetSystemID, base.App.LocalPlayer.ID).ID;
				for (int i = 0; i < 5; i++)
				{
					base.App.GameDatabase.InsertShip(iD, designID, null, (ShipParams)0, null, 0);
				}
			}
			if (stateParams.Count<object>() > 0)
			{
				this._targetSystemID = (int)stateParams[0];
			}
			else
			{
				this._targetSystemID = base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID).SystemID;
			}
			PlanetInfo planetInfo = base.App.GameDatabase.GetPlanetInfosOrbitingStar(this._targetSystemID).FirstOrDefault<PlanetInfo>();
			if (planetInfo != null)
			{
				this._selectedPlanetID = planetInfo.ID;
			}
			else
			{
				this._selectedPlanetID = 0;
			}
			base.OnPrepare(prev, new object[]
			{
				this._targetSystemID,
				this._selectedPlanetID
			});
			this._pendingObjects = new List<IGameObject>();
			this._manager = new DefenseManager(base.App);
			this._dmcrits = new GameObjectSet(base.App);
			base.App.UI.LoadScreen("DefenseManager");
			this._fleetWidget = new FleetWidget(base.App, "DefenseManager.gameFleetList");
			bool flag = true;
			if (stateParams.Count<object>() >= 2)
			{
				flag = (bool)stateParams[1];
			}
			this._fleetWidget.EnableRightClick = flag;
			this._fleetWidget.SetEnabled(flag);
			this._defenseWidget = new DefenseWidget(base.App, "defenseItemTray");
			FleetWidget expr_270 = this._fleetWidget;
			expr_270.OnFleetsModified = (FleetWidget.FleetsModifiedDelegate)Delegate.Combine(expr_270.OnFleetsModified, new FleetWidget.FleetsModifiedDelegate(this.FleetsModified));
		}
		private void FleetsModified(App app, int[] modifiedFleetIds)
		{
			this._defenseWidget.SetSyncedFleet(base.App.GameDatabase.InsertOrGetDefenseFleetInfo(this._targetSystemID, base.App.LocalPlayer.ID).ID);
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			base.App.UI.SetScreen("DefenseManager");
			this._finishing = false;
			this._dmcrits.Activate();
			base.Camera.DesiredDistance = 150000f;
			base.Camera.DesiredPitch = MathHelper.DegreesToRadians(-45f);
			base.Camera.MaxDistance = 500000f;
			this._manager.PostSetProp("SetStarSystem", this._starsystem);
			this._manager.PostSetProp("SyncFleetList", base.App.UI.Path(new string[]
			{
				"fleetDetailsWidget",
				"gameFleetList"
			}));
			this._manager.PostSetProp("LocalPlayerObjectID", base.App.LocalPlayer.ObjectID);
			this._manager.PostSetProp("SetFleetWidget", this._fleetWidget);
			this._manager.PostSetProp("SetDefenseWidget", this._defenseWidget);
			this._manager.Active = true;
			float width = base.App.AssetDatabase.MineFieldParams.Width;
			float length = base.App.AssetDatabase.MineFieldParams.Length;
			double num = Math.Sqrt((double)(length * length + width * width)) + 500.0;
			double num2 = (double)base.App.AssetDatabase.PolicePatrolRadius;
			this._manager.PostSetProp("SetMinefieldSize", new object[]
			{
				num,
				num2
			});
			this.SyncFleetShipModels();
			this._starsystem.PostSetProp("AutoDrawEnabled", false);
			this._starsystem.PostSetProp("ZoneMapEnabled", true);
			this._starsystem.PostSetProp("ZoneFocusEnabled", true);
			this._input = new CombatInput();
			this._dmcrits.Add(this._input);
			this._fleetWidget.PreferredSelectMode = true;
			List<FleetInfo> list = base.App.GameDatabase.GetFleetsByPlayerAndSystem(base.App.LocalPlayer.ID, this._targetSystemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_DEFENSE).ToList<FleetInfo>();
            list.RemoveAll((FleetInfo x) => Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, x));
			this._fleetWidget.SetSyncedFleets(list);
			if (this._targetSystemID != 0)
			{
				this._fleetWidget.ListStations = true;
				this._fleetWidget.SetSyncedStations(base.App.GameDatabase.GetStationForSystemAndPlayer(this._targetSystemID, base.App.LocalPlayer.ID).ToList<StationInfo>());
			}
			this._defenseWidget.SetSyncedFleet(base.App.GameDatabase.InsertOrGetDefenseFleetInfo(this._targetSystemID, base.App.LocalPlayer.ID).ID);
			this.SyncPlanetTypeInfo();
			this.SyncSDBSlots();
			base.App.HotKeyManager.AddListener(this);
		}
		private void SyncPlanetTypeInfo()
		{
			IEnumerable<PlanetInfo> starSystemPlanetInfos = base.App.GameDatabase.GetStarSystemPlanetInfos(this._targetSystemID);
			foreach (PlanetInfo current in starSystemPlanetInfos)
			{
				this._manager.PostSetProp("DefenseSyncPlanetInfo", new object[]
				{
					current.ID,
					current.Type
				});
			}
		}
		public void SyncSDBSlots()
		{
			FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(this._defenseWidget.GetSynchedFleet());
			if (fleetInfo != null)
			{
				IEnumerable<ShipInfo> shipInfoByFleetID = base.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true);
				foreach (ShipInfo current in shipInfoByFleetID)
				{
					bool flag = false;
					SDBInfo sDBInfo = null;
					DesignSectionInfo[] designSections = current.DesignInfo.DesignSections;
					for (int i = 0; i < designSections.Length; i++)
					{
						DesignSectionInfo designSectionInfo = designSections[i];
						if (designSectionInfo.FilePath.ToLower().Contains("_sdb"))
						{
							flag = true;
							sDBInfo = base.App.GameDatabase.GetSDBInfoFromShip(current.ID);
							break;
						}
					}
					if (flag && sDBInfo != null)
					{
						this._manager.PostSetProp("SyncSDBSlots", new object[]
						{
							sDBInfo.ShipId,
							sDBInfo.OrbitalId
						});
					}
				}
			}
		}
		private void SyncFleetShipModels()
		{
			IEnumerable<FleetInfo> fleetsByPlayerAndSystem = base.App.GameDatabase.GetFleetsByPlayerAndSystem(base.App.LocalPlayer.ID, this._targetSystemID, FleetType.FL_ALL);
			foreach (FleetInfo current in fleetsByPlayerAndSystem)
			{
				if (current.PlayerID == base.App.LocalPlayer.ID)
				{
					IEnumerable<ShipInfo> enumerable = 
						from x in base.App.GameDatabase.GetShipInfoByFleetID(current.ID, true)
						where !x.DesignInfo.DesignSections.Any((DesignSectionInfo y) => ShipSectionAsset.IsBattleRiderClass(y.ShipSectionAsset.RealClass))
						select x;
					this._manager.PostSetProp("AddFleet", new object[]
					{
						current.ID,
						enumerable.Count<ShipInfo>()
					});
					foreach (ShipInfo current2 in enumerable)
					{
						List<object> list = new List<object>();
						list.Add(0);
						ShipDummy shipDummy = new ShipDummy(base.App, CreateShipDummyParams.ObtainShipDummyParams(base.App, current2));
						base.App.AddExistingObject(shipDummy, list.ToArray());
						this._manager.PostObjectAddObjects(new IGameObject[]
						{
							shipDummy
						});
						this._pendingObjects.Add(shipDummy);
						Vector3? shipFleetPosition = base.App.GameDatabase.GetShipFleetPosition(current2.ID);
						Matrix? shipSystemPosition = base.App.GameDatabase.GetShipSystemPosition(current2.ID);
						shipDummy.PostSetProp("SetShipID", current2.ID);
						int commandPointCost = base.App.GameDatabase.GetCommandPointCost(current2.DesignID);
						shipDummy.PostSetProp("SetShipCommandCost", commandPointCost);
						shipDummy.PostSetProp("SetFleetID", current.ID);
						shipDummy.PostSetProp("SetShipName", current2.ShipName);
						if (shipFleetPosition.HasValue)
						{
							shipDummy.PostSetProp("SetFleetPosition", new object[]
							{
								shipFleetPosition.Value.X,
								shipFleetPosition.Value.Y,
								shipFleetPosition.Value.Z
							});
						}
						if (shipSystemPosition.HasValue)
						{
							shipDummy.PostSetProp("SetSystemTransform", shipSystemPosition.Value);
						}
					}
				}
			}
		}
		protected override void OnUpdate()
		{
			List<IGameObject> list = new List<IGameObject>();
			foreach (IGameObject current in this._pendingObjects)
			{
				if (current.ObjectStatus == GameObjectStatus.Ready)
				{
					if (current is IActive)
					{
						(current as IActive).Active = true;
					}
					this._dmcrits.Add(current);
					list.Add(current);
				}
			}
			foreach (IGameObject current2 in list)
			{
				this._pendingObjects.Remove(current2);
			}
			if (this._fleetWidget != null && this._fleetWidget.DefenseFleetUpdated && this._defenseWidget != null)
			{
				this._fleetWidget.DefenseFleetUpdated = false;
				this._defenseWidget.SetSyncedFleet(base.App.GameDatabase.InsertOrGetDefenseFleetInfo(this._targetSystemID, base.App.LocalPlayer.ID).ID);
			}
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			this._pendingObjects.Clear();
			this._fleetWidget.Dispose();
			this._defenseWidget.Dispose();
			this._dmcrits.Dispose();
			this._dmcrits = null;
			this._manager.Dispose();
			base.OnExit(prev, reason);
		}
		public override bool IsReady()
		{
			return this._dmcrits.IsReady() && base.IsReady();
		}
		protected override void OnUIGameEvent(string eventName, string[] eventParams)
		{
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
			case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSE_POSITIONS:
			{
				int num = mr.ReadInteger();
				for (int i = 0; i < num; i++)
				{
					mr.ReadInteger();
					int num2 = mr.ReadInteger();
					for (int j = 0; j < num2; j++)
					{
						bool flag = mr.ReadBool();
						if (flag)
						{
							bool flag2 = mr.ReadBool();
							int shipID = mr.ReadInteger();
							Matrix? position;
							if (flag2)
							{
								position = new Matrix?(new Matrix(mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle()));
							}
							else
							{
								position = null;
							}
							base.App.GameDatabase.UpdateShipSystemPosition(shipID, position);
						}
					}
				}
				if (this._finishing)
				{
					base.App.SwitchGameState<StarMapState>(new object[0]);
				}
				break;
			}
			case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSEBOAT_DATA:
			{
				int shipID2 = mr.ReadInteger();
				int num3 = mr.ReadInteger();
				base.App.GameDatabase.RemoveSDBByShipID(shipID2);
				if (num3 != 0)
				{
					base.App.GameDatabase.InsertSDB(num3, shipID2);
				}
				break;
			}
			}
			base.OnEngineMessage(messageID, mr);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "gameFleetList" && !string.IsNullOrEmpty(msgParams[0]))
				{
					int.Parse(msgParams[0]);
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == DefenseManagerState.UIExitButton)
					{
						this._manager.PostSetProp("SyncSystemPositions", new object[0]);
						this._finishing = true;
						return;
					}
				}
				else
				{
					if (msgType == "DragAndDropEvent")
					{
						bool flag = false;
						flag = !flag;
					}
				}
			}
		}
		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(base.Name))
			{
				switch (action)
				{
				case HotKeyManager.HotKeyActions.State_Starmap:
					base.App.UI.LockUI();
					base.App.SwitchGameState<StarMapState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_BuildScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<BuildScreenState>(new object[]
					{
						this._targetSystemID
					});
					return true;
				case HotKeyManager.HotKeyActions.State_DesignScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<DesignScreenState>(new object[]
					{
						false,
						base.Name
					});
					return true;
				case HotKeyManager.HotKeyActions.State_ResearchScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<ResearchScreenState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_SotspediaScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<SotspediaState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<DiplomacyScreenState>(new object[0]);
					return true;
				}
			}
			return false;
		}
	}
}
