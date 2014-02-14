using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class RiderManagerState : GameState, IKeyBindListener
	{
		private RiderManager _manager;
		private int _targetSystemID;
		private List<ShipDummy> _shipDummies;
		private List<IGameObject> _pendingObjects;
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private FleetWidget _fleetWidget;
		private static readonly string UIExitButton = "gameCancelMissionButton";
		public RiderManagerState(App game) : base(game)
		{
		}
		public static bool CanOpen(GameSession sim, int systemID)
		{
			return sim != null && systemID > 0 && sim.GameDatabase.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_DEFENSE).Any((FleetInfo x) => x.PlayerID == sim.LocalPlayer.ID);
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame();
				if (stateParams.Count<object>() > 0)
				{
					this._targetSystemID = (int)stateParams[0];
				}
				else
				{
					this._targetSystemID = base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID).SystemID;
				}
				DesignInfo designInfo = new DesignInfo();
				designInfo.PlayerID = base.App.LocalPlayer.ID;
				designInfo.DesignSections = new DesignSectionInfo[2];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\br_eng_fusion.section";
				designInfo.DesignSections[1] = new DesignSectionInfo();
				designInfo.DesignSections[1].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\br_msn_spinal.section";
				int designID = base.App.GameDatabase.InsertDesignByDesignInfo(designInfo);
				int? reserveFleetID = base.App.GameDatabase.GetReserveFleetID(base.App.LocalPlayer.ID, this._targetSystemID);
				for (int i = 0; i < 5; i++)
				{
					base.App.GameDatabase.InsertShip(reserveFleetID.Value, designID, null, (ShipParams)0, null, 0);
				}
				designInfo = new DesignInfo();
				designInfo.PlayerID = base.App.LocalPlayer.ID;
				designInfo.DesignSections = new DesignSectionInfo[2];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\br_eng_fusion.section";
				designInfo.DesignSections[1] = new DesignSectionInfo();
				designInfo.DesignSections[1].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\br_msn_scout.section";
				designID = base.App.GameDatabase.InsertDesignByDesignInfo(designInfo);
				for (int j = 0; j < 5; j++)
				{
					base.App.GameDatabase.InsertShip(reserveFleetID.Value, designID, null, (ShipParams)0, null, 0);
				}
				designInfo = new DesignInfo();
				designInfo.PlayerID = base.App.LocalPlayer.ID;
				designInfo.Name = "My Fun Design Has A Long Ass Name";
				designInfo.Role = ShipRole.CARRIER;
				designInfo.DesignSections = new DesignSectionInfo[3];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\cr_eng_fusion.section";
				designInfo.DesignSections[1] = new DesignSectionInfo();
				designInfo.DesignSections[1].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\cr_mis_brcarrier.section";
				designInfo.DesignSections[2] = new DesignSectionInfo();
				designInfo.DesignSections[2].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\cr_cmd_assault.section";
				designID = base.App.GameDatabase.InsertDesignByDesignInfo(designInfo);
				Random random = new Random();
				for (int k = 0; k < 5; k++)
				{
					int shipID = base.App.GameDatabase.InsertShip(reserveFleetID.Value, designID, null, (ShipParams)0, null, 0);
					IEnumerable<SectionInstanceInfo> shipSectionInstances = base.App.GameDatabase.GetShipSectionInstances(shipID);
					foreach (SectionInstanceInfo current in shipSectionInstances)
					{
						current.Structure = (int)((float)current.Structure * random.NextSingle());
						base.App.GameDatabase.UpdateSectionInstance(current);
					}
				}
				designInfo = new DesignInfo();
				designInfo.PlayerID = base.App.LocalPlayer.ID;
				designInfo.Name = "Repair Dem";
				designInfo.Role = ShipRole.CARRIER;
				designInfo.DesignSections = new DesignSectionInfo[3];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\cr_eng_fusion.section";
				designInfo.DesignSections[1] = new DesignSectionInfo();
				designInfo.DesignSections[1].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\cr_mis_repair.section";
				designInfo.DesignSections[2] = new DesignSectionInfo();
				designInfo.DesignSections[2].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\cr_cmd_assault.section";
				designID = base.App.GameDatabase.InsertDesignByDesignInfo(designInfo);
				for (int l = 0; l < 3; l++)
				{
					base.App.GameDatabase.InsertShip(reserveFleetID.Value, designID, null, (ShipParams)0, null, 0);
				}
				new DesignInfo();
				designInfo.PlayerID = base.App.LocalPlayer.ID;
				designInfo.Name = "Little MEEP";
				designInfo.DesignSections = new DesignSectionInfo[2];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\br_eng_fusion.section";
				designInfo.DesignSections[1] = new DesignSectionInfo();
				designInfo.DesignSections[1].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\br_msn_spinal.section";
				base.App.GameDatabase.InsertDesignByDesignInfo(designInfo);
				designInfo = new DesignInfo();
				designInfo.PlayerID = base.App.LocalPlayer.ID;
				designInfo.Name = "My Fun Leviathan";
				designInfo.Role = ShipRole.CARRIER;
				designInfo.Class = ShipClass.Leviathan;
				designInfo.DesignSections = new DesignSectionInfo[1];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + base.App.LocalPlayer.Faction.Name + "\\sections\\lv_carrier.section";
				designID = base.App.GameDatabase.InsertDesignByDesignInfo(designInfo);
				for (int m = 0; m < 5; m++)
				{
					base.App.GameDatabase.InsertShip(reserveFleetID.Value, designID, null, (ShipParams)0, null, 0);
				}
			}
			else
			{
				if (stateParams.Count<object>() > 0)
				{
					this._targetSystemID = (int)stateParams[0];
				}
				else
				{
					this._targetSystemID = base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID).SystemID;
				}
			}
			base.App.UI.LoadScreen("RiderManager");
			this._pendingObjects = new List<IGameObject>();
			this._shipDummies = new List<ShipDummy>();
			this._crits = new GameObjectSet(base.App);
			this._fleetWidget = new FleetWidget(base.App, "RiderManager.gameFleetWidget");
			this._fleetWidget.SeparateDefenseFleet = false;
			this._fleetWidget.ShipSelectionEnabled = true;
			base.App.UI.ClearItems("RiderManager.riderList");
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("RiderManager");
			this._manager = new RiderManager(base.App, "RiderManager");
			this._camera = new OrbitCameraController(base.App);
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(0f);
			this._camera.TargetPosition = new Vector3(0f, 0f, 0f);
			this._camera.MaxDistance = 6000f;
			this._camera.DesiredDistance = this._camera.MaxDistance;
			this._manager.PostSetProp("CameraController", this._camera);
			this._manager.PostSetActive(true);
			IEnumerable<FleetInfo> fleetsByPlayerAndSystem = base.App.GameDatabase.GetFleetsByPlayerAndSystem(base.App.LocalPlayer.ID, this._targetSystemID, FleetType.FL_ALL);
			this._manager.SetSyncedFleets(fleetsByPlayerAndSystem.ToList<FleetInfo>());
			this.SyncFleetShipModels();
			this._fleetWidget.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.FleetWidgetShipFilter);
			this._fleetWidget.ShowEmptyFleets = false;
			this._fleetWidget.SetSyncedFleets(fleetsByPlayerAndSystem.ToList<FleetInfo>());
			this._manager.PostSetProp("SetFleetWidget", this._fleetWidget);
			base.App.HotKeyManager.AddListener(this);
		}
		public FleetWidget.FilterShips FleetWidgetShipFilter(ShipInfo ship, DesignInfo design)
		{
			int num = 0;
			DesignSectionInfo[] designSections = design.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.GetShipSectionAsset(designSectionInfo.FilePath);
				if (shipSectionAsset.BattleRiderType != BattleRiderTypes.Unspecified)
				{
					BattleRiderTypes arg_3A_0 = shipSectionAsset.BattleRiderType;
				}
				num += RiderManager.GetNumRiderSlots(base.App, designSectionInfo);
			}
			int num2 = 0;
			DesignSectionInfo[] designSections2 = design.DesignSections;
			for (int j = 0; j < designSections2.Length; j++)
			{
				DesignSectionInfo designSectionInfo2 = designSections2[j];
				num2 += base.App.AssetDatabase.GetShipSectionAsset(designSectionInfo2.FilePath).ReserveSize;
			}
			if (num2 > 0 || num > 0)
			{
				return FleetWidget.FilterShips.Enable;
			}
			if (design.Class == ShipClass.BattleRider)
			{
				return FleetWidget.FilterShips.Ignore;
			}
			return FleetWidget.FilterShips.Ignore;
		}
		private void SyncFleetShipModels()
		{
			List<int> syncedShips = this._manager.GetSyncedShips();
			foreach (int current in syncedShips)
			{
				ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(current, true);
				List<object> list = new List<object>();
				list.Add(0);
				ShipDummy shipDummy = new ShipDummy(base.App, CreateShipDummyParams.ObtainShipDummyParams(base.App, shipInfo));
				base.App.AddExistingObject(shipDummy, list.ToArray());
				this._manager.PostObjectAddObjects(new IGameObject[]
				{
					shipDummy
				});
				this._pendingObjects.Add(shipDummy);
				Vector3? shipFleetPosition = base.App.GameDatabase.GetShipFleetPosition(shipInfo.ID);
				shipDummy.PostSetProp("SetShipID", shipInfo.ID);
				shipDummy.PostSetProp("SetDesignID", shipInfo.DesignID);
				int shipCommandPointCost = base.App.GameDatabase.GetShipCommandPointCost(shipInfo.ID, true);
				shipDummy.PostSetProp("SetShipCommandCost", shipCommandPointCost);
				shipDummy.FleetID = shipInfo.FleetID;
				shipDummy.PostSetProp("SetShipName", shipInfo.ShipName);
				if (shipFleetPosition.HasValue)
				{
					shipDummy.PostSetProp("SetFleetPosition", new object[]
					{
						shipFleetPosition.Value.X,
						shipFleetPosition.Value.Y,
						shipFleetPosition.Value.Z
					});
				}
				this._shipDummies.Add(shipDummy);
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
					this._crits.Add(current);
					list.Add(current);
				}
			}
			foreach (IGameObject current2 in list)
			{
				this._pendingObjects.Remove(current2);
			}
			if (this._fleetWidget != null)
			{
				this._fleetWidget.OnUpdate();
			}
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (panelName == RiderManagerState.UIExitButton)
			{
				base.App.SwitchGameState<StarMapState>(new object[0]);
			}
		}
		public override bool IsReady()
		{
			return this._crits.IsReady() && base.IsReady() && base.IsReady();
		}
		protected override void OnExit(GameState next, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			this._manager.Dispose();
			this._fleetWidget.Dispose();
			if (this._camera != null)
			{
				this._camera.Dispose();
				this._camera = null;
			}
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
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
