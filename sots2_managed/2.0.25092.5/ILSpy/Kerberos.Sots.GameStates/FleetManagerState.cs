using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class FleetManagerState : GameState, IKeyBindListener
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
		private OrbitCameraController _camera;
		private GameObjectSet _crits;
		private FleetManager _manager;
		private Sky _sky;
		private int _prevFleetID;
		private int _commandQuota;
		private string _contextMenuID;
		private string _shipContextMenuID;
		private int _contextSlot;
		private string _admiralManagerDialog;
		private int _contextMenuShip;
		private List<ShipDummy> _shipDummies;
		private bool _finalSync;
		private FleetWidget _fleetWidget;
		protected static readonly string UICancelButton = "gameCancelMissionButton";
		protected static readonly string UICommitButton = "gameConfirmMissionButton";
		private static readonly string UIExitButton = "gameExitButton";
		public FleetManagerState(App game) : base(game)
		{
		}
		protected void OnBack()
		{
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame();
			}
			if (stateParams.Count<object>() > 0)
			{
				this._targetSystemID = (int)stateParams[0];
			}
			else
			{
				this._targetSystemID = base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID).SystemID;
			}
			this._prevFleetID = -1;
			this._sky = new Sky(base.App, SkyUsage.InSystem, 0);
			this._crits = new GameObjectSet(base.App);
			this._crits.Add(this._sky);
			this._manager = new FleetManager(base.App);
			this._shipDummies = new List<ShipDummy>();
			this.SyncFleetShipModels(this._targetSystemID);
			base.App.UI.LoadScreen("FleetManager");
			this._contextMenuID = base.App.UI.CreatePanelFromTemplate("FleetManagerContextMenu", null);
			this._shipContextMenuID = base.App.UI.CreatePanelFromTemplate("FleetManagerShipContextMenu", null);
			this._fleetWidget = new FleetWidget(base.App, "FleetManager.fleetDetailsWidget.gameFleetList");
			this._fleetWidget.DisableTooltips = true;
			this._fleetWidget.SeparateDefenseFleet = false;
			this._fleetWidget.EnableMissionButtons = false;
			FleetWidget expr_14D = this._fleetWidget;
			expr_14D.OnFleetsModified = (FleetWidget.FleetsModifiedDelegate)Delegate.Combine(expr_14D.OnFleetsModified, new FleetWidget.FleetsModifiedDelegate(this.FleetsModified));
			this._fleetWidget.EnableCreateFleetButton = false;
		}
		private void FleetsModified(App app, int[] modifiedFleetIds)
		{
			this.RefreshFleetShipModels();
		}
		protected override void OnEnter()
		{
			this._finalSync = false;
			base.App.UI.SetScreen("FleetManager");
			base.App.UI.UnlockUI();
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._sky.Active = true;
			this._camera = new OrbitCameraController(base.App);
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-75f);
			this._camera.TargetPosition = new Vector3(0f, 0f, 0f);
			this._camera.MaxDistance = 6000f;
			this._camera.DesiredDistance = this._camera.MaxDistance;
			base.App.UI.ClearItems("partFleetShips");
			this._manager.PostSetProp("CameraController", this._camera);
			this._manager.PostSetProp("InputEnabled", true);
			this._manager.PostSetProp("SyncShipList", "partFleetShips");
			this._manager.PostSetProp("SyncCommandPointDisplay", "cmdpointsValue");
			this._manager.Active = true;
			base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID);
			IEnumerable<FleetInfo> fleetInfoBySystemID = base.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL);
			this.SyncShipContextMenu(fleetInfoBySystemID);
			EmpireBarUI.SyncTitleFrame(base.App);
			this._fleetWidget.SetSyncedFleets((
				from x in fleetInfoBySystemID
                where x.PlayerID == base.App.LocalPlayer.ID && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, x)
				select x).ToList<FleetInfo>());
			this._manager.PostSetProp("FleetWidget", this._fleetWidget.ObjectID);
			this._fleetWidget.ShipSelectionEnabled = true;
			if (base.App.LocalPlayer.Faction.Name == "loa")
			{
				base.App.UI.SetEnabled("fleetmanagerloaCompose", true);
				base.App.UI.SetVisible("fleetmanagerloaCompose", true);
				base.App.UI.SetVisible("ngpWarning", true);
			}
			else
			{
				base.App.UI.SetEnabled("fleetmanagerloaCompose", false);
				base.App.UI.SetVisible("fleetmanagerloaCompose", false);
				base.App.UI.SetVisible("ngpWarning", false);
			}
			base.App.UI.AutoSize("buttonPanel");
			base.App.UI.ForceLayout("buttonPanel");
			base.App.UI.AutoSize("fleetDetailsWidget");
			base.App.UI.ForceLayout("fleetDetailsWidget");
			base.App.UI.AutoSize("leftPanel");
			base.App.UI.ForceLayout("leftPanel");
			base.App.UI.AutoSize("buttonPanel");
			base.App.UI.ForceLayout("buttonPanel");
			base.App.UI.AutoSize("fleetDetailsWidget");
			base.App.UI.ForceLayout("fleetDetailsWidget");
			base.App.UI.AutoSize("leftPanel");
			base.App.UI.ForceLayout("leftPanel");
			base.App.HotKeyManager.AddListener(this);
		}
		private void ShowFleetPopup(string[] eventParams)
		{
			base.App.UI.AutoSize(this._contextMenuID);
			this._contextSlot = int.Parse(eventParams[3]);
			base.App.UI.ShowTooltip(this._contextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
		}
		private void ShowShipPopup(string[] eventParams)
		{
			this._contextMenuShip = int.Parse(eventParams[3]);
			IEnumerable<FleetInfo> fleetInfoBySystemID = base.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE);
			this.SyncShipContextMenu(fleetInfoBySystemID);
			base.App.UI.ShowTooltip(this._shipContextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
		}
		public static bool CanOpen(GameSession sim, int systemID)
		{
			return sim != null && systemID > 0 && sim.GameDatabase.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).Any((FleetInfo x) => x.PlayerID == sim.LocalPlayer.ID);
		}
		private void SyncFleetShipModels(int systemID)
		{
			IEnumerable<FleetInfo> fleetInfoBySystemID = base.App.GameDatabase.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE);
			foreach (FleetInfo current in fleetInfoBySystemID)
			{
				if (current.PlayerID == base.App.LocalPlayer.ID)
				{
					IEnumerable<ShipInfo> shipInfoByFleetID = base.App.GameDatabase.GetShipInfoByFleetID(current.ID, true);
					foreach (ShipInfo current2 in shipInfoByFleetID)
					{
						DesignSectionInfo designSectionInfo = current2.DesignInfo.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission);
						if ((designSectionInfo == null || !ShipSectionAsset.IsBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass)) && !current2.DesignInfo.IsLoaCube())
						{
							List<object> list = new List<object>();
							list.Add(0);
							ShipDummy shipDummy = new ShipDummy(base.App, CreateShipDummyParams.ObtainShipDummyParams(base.App, current2));
							base.App.AddExistingObject(shipDummy, list.ToArray());
							this._manager.PostObjectAddObjects(new IGameObject[]
							{
								shipDummy
							});
							Vector3? shipFleetPosition = base.App.GameDatabase.GetShipFleetPosition(current2.ID);
							shipDummy.PostSetProp("SetShipID", current2.ID);
							shipDummy.PostSetProp("SetDesignID", current2.DesignID);
							int shipCommandPointCost = base.App.GameDatabase.GetShipCommandPointCost(current2.ID, true);
							shipDummy.PostSetProp("SetShipCommandCost", shipCommandPointCost);
							shipDummy.FleetID = current.ID;
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
							this._shipDummies.Add(shipDummy);
						}
					}
				}
			}
		}
		private void RefreshFleetShipModels()
		{
			List<FleetInfo> list = base.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				if (current.PlayerID == base.App.LocalPlayer.ID)
				{
					IEnumerable<ShipInfo> shipInfoByFleetID = base.App.GameDatabase.GetShipInfoByFleetID(current.ID, false);
					foreach (ShipInfo current2 in shipInfoByFleetID)
					{
						foreach (ShipDummy current3 in this._shipDummies)
						{
							if (current3.ShipID == current2.ID && current3.FleetID != current2.FleetID)
							{
								current3.FleetID = current2.FleetID;
								current3.PostSetProp("ClearFleetPosition", new object[0]);
							}
						}
					}
				}
			}
			if (this._fleetWidget.SelectedFleet != -1)
			{
				this._manager.PostSetProp("LayoutGridForFleet", new object[]
				{
					this._fleetWidget.SelectedFleet
				});
			}
		}
		private void SyncShipContextMenu(IEnumerable<FleetInfo> fleets)
		{
			int num = 0;
			foreach (FleetInfo current in fleets)
			{
				string text = this._shipContextMenuID + ".menuItem" + num;
				string propertyValue = "Move to " + current.Name + " Fleet";
				base.App.UI.SetPropertyString(text + ".idle.menulabel", "text", propertyValue);
				base.App.UI.SetPropertyString(text + ".mouse_over.menulabel", "text", propertyValue);
				base.App.UI.SetPropertyString(text + ".pressed.menulabel", "text", propertyValue);
				base.App.UI.SetPropertyString(text + ".disabled.menulabel", "text", propertyValue);
				base.App.UI.SetVisible(text, true);
				num++;
			}
			for (int i = num; i < 10; i++)
			{
				string panelId = this._shipContextMenuID + ".menuItem" + i;
				base.App.UI.SetVisible(panelId, false);
			}
			base.App.UI.AutoSize(this._shipContextMenuID);
		}
		private void SyncFleetShipsList(int fleetID)
		{
			this._manager.PostSetProp("FleetSelectionChanged", new object[]
			{
				this._prevFleetID,
				fleetID
			});
			IEnumerable<ShipInfo> shipInfoByFleetID = base.App.GameDatabase.GetShipInfoByFleetID(fleetID, false);
			int num = 0;
			int num2 = 0;
			foreach (ShipInfo current in shipInfoByFleetID)
			{
				num++;
				num2 = Math.Max(num2, base.App.GameDatabase.GetDesignCommandPointQuota(base.App.AssetDatabase, current.DesignID));
			}
			this._manager.PostSetProp("SetCommandQuota", num2);
			this._commandQuota = num2;
			this._prevFleetID = fleetID;
		}
		protected override void OnUpdate()
		{
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			if (this._manager != null)
			{
				this._manager.Dispose();
				this._manager = null;
			}
			if (this._fleetWidget != null)
			{
				FleetWidget expr_39 = this._fleetWidget;
				expr_39.OnFleetsModified = (FleetWidget.FleetsModifiedDelegate)Delegate.Remove(expr_39.OnFleetsModified, new FleetWidget.FleetsModifiedDelegate(this.FleetsModified));
				this._fleetWidget.Dispose();
				this._fleetWidget = null;
			}
			foreach (ShipDummy current in this._shipDummies)
			{
				current.Dispose();
			}
			this._shipDummies = null;
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
			base.App.UI.DestroyPanel(this._contextMenuID);
			base.App.UI.DestroyPanel(this._shipContextMenuID);
			base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
		}
		public override bool IsReady()
		{
			if (!this._crits.IsReady() || !base.IsReady())
			{
				return false;
			}
			bool result = true;
			foreach (ShipDummy current in this._shipDummies)
			{
				if (current.ObjectStatus == GameObjectStatus.Pending)
				{
					result = false;
				}
			}
			return result;
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
			case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
				this.RemoveObject(mr);
				return;
			case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
				this.RemoveObjects(mr);
				return;
			default:
			{
				if (messageID != InteropMessageID.IMID_SCRIPT_SYNC_FLEET_POSITIONS)
				{
					return;
				}
				mr.ReadInteger();
				int num = mr.ReadInteger();
				for (int i = 0; i < num; i++)
				{
					bool flag = mr.ReadBool();
					int shipID = mr.ReadInteger();
					Vector3? position;
					if (flag)
					{
						position = new Vector3?(new Vector3(mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle()));
					}
					else
					{
						position = null;
					}
					base.App.GameDatabase.UpdateShipFleetPosition(shipID, position);
				}
				if (this._finalSync)
				{
					base.App.SwitchGameState<StarMapState>(new object[0]);
				}
				return;
			}
			}
		}
		protected void RemoveObject(ScriptMessageReader data)
		{
			int id = data.ReadInteger();
			this.RemoveGameObject(base.App.GetGameObject(id));
		}
		protected void RemoveObjects(ScriptMessageReader data)
		{
			for (int id = data.ReadInteger(); id != 0; id = data.ReadInteger())
			{
				this.RemoveGameObject(base.App.GetGameObject(id));
			}
		}
		public override void RemoveGameObject(IGameObject gameObject)
		{
			if (gameObject == null)
			{
				return;
			}
			IGameObject gameObject2 = this._crits.Objects.FirstOrDefault((IGameObject x) => x.ObjectID == gameObject.ObjectID && x is IDisposable);
			if (gameObject2 != null)
			{
				(gameObject2 as IDisposable).Dispose();
				this._crits.Remove(gameObject2);
				return;
			}
			IGameObject gameObject3 = base.App.GetGameObject(gameObject.ObjectID);
			if (gameObject3 != null)
			{
				base.App.ReleaseObject(gameObject3);
			}
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
		}
		public void Refresh()
		{
			this.RefreshFleetShipModels();
			IEnumerable<FleetInfo> source = 
				from x in base.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL)
				where x.Type != FleetType.FL_RESERVE || x.PlayerID == base.App.LocalPlayer.ID
				select x;
			this._fleetWidget.SetSyncedFleets(source.ToList<FleetInfo>());
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "gameFleetList")
				{
					int fleetID = 0;
					if (!string.IsNullOrEmpty(msgParams[0]))
					{
						fleetID = int.Parse(msgParams[0]);
					}
					this.SyncFleetShipsList(fleetID);
				}
				if (panelName == "partFleetShips")
				{
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == FleetManagerState.UIExitButton)
					{
						this._finalSync = true;
						this._manager.PostSetProp("RequestResync", new object[0]);
						return;
					}
					if (panelName == "gameTutorialButton")
					{
						base.App.UI.SetVisible("FleetManagerTutorial", true);
						return;
					}
					if (panelName == "fleetManagerTutImage")
					{
						base.App.UI.SetVisible("FleetManagerTutorial", false);
						return;
					}
					if (panelName == "fleetManagerCreateFleet")
					{
						this._admiralManagerDialog = base.App.UI.CreateDialog(new AdmiralManagerDialog(base.App, base.App.LocalPlayer.ID, this._targetSystemID, false, "AdmiralManagerDialog"), null);
						return;
					}
					if (panelName == "fleetmanagerloaCompose")
					{
						base.App.UI.CreateDialog(new DialogLoaFleetCompositor(base.App, MissionType.NO_MISSION), null);
						return;
					}
					if (panelName == "fleetManagerUpperButton")
					{
						this._manager.PostSetProp("SelectLevel", 2);
						return;
					}
					if (panelName == "fleetManagerMiddleButton")
					{
						this._manager.PostSetProp("SelectLevel", 1);
						return;
					}
					if (panelName == "fleetManagerLowerButton")
					{
						this._manager.PostSetProp("SelectLevel", 0);
						return;
					}
					if (panelName == "gameFormationVButton")
					{
						this._manager.PostSetProp("VFormation", new object[0]);
						return;
					}
					if (panelName == "gameFormationLineButton")
					{
						this._manager.PostSetProp("LineFormation", new object[0]);
						return;
					}
					if (panelName.StartsWith("menuItem"))
					{
						int num = int.Parse(panelName.Substring(8));
						IEnumerable<FleetInfo> fleetInfoBySystemID = base.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL);
						int num2 = 0;
						foreach (FleetInfo current in fleetInfoBySystemID)
						{
							if (num == 0)
							{
								num2 = current.ID;
								break;
							}
							num--;
						}
						if (num2 != 0 && this._contextMenuShip != 0)
						{
							base.App.GameDatabase.TransferShip(this._contextMenuShip, num2);
							foreach (ShipDummy current2 in this._shipDummies)
							{
								if (current2.ShipID == this._contextMenuShip)
								{
									current2.FleetID = num2;
									current2.PostSetProp("ClearFleetPosition", new object[0]);
								}
							}
							this.SyncFleetShipsList(this._prevFleetID);
							this._fleetWidget.SetSyncedFleets((
								from x in base.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL)
								where x.Type != FleetType.FL_RESERVE || x.PlayerID == base.App.LocalPlayer.ID
								select x).ToList<FleetInfo>());
							return;
						}
					}
				}
				else
				{
					if (msgType == "dialog_closed" && panelName == this._admiralManagerDialog)
					{
						this.RefreshFleetShipModels();
						IEnumerable<FleetInfo> source = 
							from x in base.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL)
							where x.Type != FleetType.FL_RESERVE || x.PlayerID == base.App.LocalPlayer.ID
							select x;
						this._fleetWidget.SetSyncedFleets(source.ToList<FleetInfo>());
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
