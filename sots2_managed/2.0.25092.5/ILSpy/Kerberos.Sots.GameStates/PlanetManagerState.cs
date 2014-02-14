using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class PlanetManagerState : GameState
	{
		private enum PlanetFilterMode
		{
			AllPlanets,
			SurveyedPlanets,
			OwnedPlanets,
			EnemyPlanets
		}
		private static readonly string UIExitButton = "gameExitButton";
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private List<PlanetWidget> _planetWidgets;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private int _targetSystem;
		private PlanetManagerState.PlanetFilterMode _currentFilterMode;
		public PlanetManagerState(App app) : base(app)
		{
		}
		public static bool CanOpen(GameSession game, int targetSystemId)
		{
			return game.GameDatabase.GetStationInfosByPlayerID(game.LocalPlayer.ID).Count<StationInfo>() > 0;
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			base.App.UI.LoadScreen("PlanetManager");
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame();
				this._targetSystem = base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID).SystemID;
				return;
			}
			if (parms.Count<object>() > 0)
			{
				this._targetSystem = (int)parms[0];
				return;
			}
			this._targetSystem = base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID).SystemID;
		}
		protected override void OnEnter()
		{
			base.App.UI.UnlockUI();
			base.App.UI.SetScreen("PlanetManager");
			base.App.UI.AddItem("filterDropdown", "", 0, App.Localize("@UI_PLANET_MANAGER_ALL_PLANETS"));
			base.App.UI.AddItem("filterDropdown", "", 1, App.Localize("@UI_PLANET_MANAGER_SURVEYED_PLANETS"));
			base.App.UI.AddItem("filterDropdown", "", 2, App.Localize("@UI_PLANET_MANAGER_OWNED_PLANETS"));
			base.App.UI.AddItem("filterDropdown", "", 3, App.Localize("@UI_PLANET_MANAGER_ENEMY_PLANETS"));
			base.App.UI.SetSelection("filterDropdown", 0);
			this._currentFilterMode = PlanetManagerState.PlanetFilterMode.AllPlanets;
			base.App.UI.SetPropertyBool("gameExitButton", "lockout_button", true);
			EmpireBarUI.SyncTitleFrame(base.App);
			this._planetWidgets = new List<PlanetWidget>();
			this.SyncPlanetList();
		}
		protected void SetSyncedSystem(StarSystemInfo system)
		{
			base.App.UI.ClearItems("system_list");
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Terminate();
			}
			foreach (PlanetWidget current2 in this._planetWidgets)
			{
				current2.Terminate();
			}
			this._planetWidgets.Clear();
			List<PlanetInfo> list = this.FilteredPlanetList(system);
			base.App.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			string itemGlobalID = base.App.UI.GetItemGlobalID("system_list", "", system.ID, "");
			this._systemWidgets.Add(new SystemWidget(base.App, itemGlobalID));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo current3 in list)
			{
				if (base.App.AssetDatabase.IsPotentialyHabitable(current3.Type))
				{
					base.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "planetDetailsM_Card");
					string itemGlobalID2 = base.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(base.App, itemGlobalID2));
					this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
				}
				else
				{
					if (base.App.AssetDatabase.IsGasGiant(current3.Type))
					{
						base.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "gasgiantDetailsM_Card");
						string itemGlobalID3 = base.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
						this._planetWidgets.Add(new PlanetWidget(base.App, itemGlobalID3));
						this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
					}
					else
					{
						if (base.App.AssetDatabase.IsMoon(current3.Type))
						{
							base.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "moonDetailsM_Card");
							string itemGlobalID4 = base.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
							this._planetWidgets.Add(new PlanetWidget(base.App, itemGlobalID4));
							this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
						}
					}
				}
			}
		}
		private List<PlanetInfo> FilteredPlanetList(StarSystemInfo system)
		{
			List<PlanetInfo> list = base.App.GameDatabase.GetPlanetInfosOrbitingStar(system.ID).ToList<PlanetInfo>();
			List<PlanetInfo> list2 = new List<PlanetInfo>();
			foreach (PlanetInfo current in list)
			{
				if (base.App.GameDatabase.IsSurveyed(base.App.LocalPlayer.ID, system.ID))
				{
					if (this._currentFilterMode == PlanetManagerState.PlanetFilterMode.AllPlanets)
					{
						list2.Add(current);
					}
					else
					{
						if (this._currentFilterMode == PlanetManagerState.PlanetFilterMode.SurveyedPlanets)
						{
							if (base.App.GameDatabase.GetColonyIntelForPlanet(base.App.LocalPlayer.ID, current.ID) == null)
							{
								list2.Add(current);
							}
						}
						else
						{
							if (this._currentFilterMode == PlanetManagerState.PlanetFilterMode.OwnedPlanets)
							{
								ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(current.ID);
								if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == base.App.LocalPlayer.ID)
								{
									list2.Add(current);
								}
							}
							else
							{
								if (this._currentFilterMode == PlanetManagerState.PlanetFilterMode.EnemyPlanets)
								{
									AIColonyIntel colonyIntelForPlanet = base.App.GameDatabase.GetColonyIntelForPlanet(base.App.LocalPlayer.ID, current.ID);
									if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != base.App.LocalPlayer.ID)
									{
										list2.Add(current);
									}
								}
							}
						}
					}
				}
			}
			return list2;
		}
		protected void SyncPlanetList()
		{
			base.App.UI.ClearItems("sys_list_left");
			List<StarSystemInfo> list = base.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			bool flag = true;
			foreach (StarSystemInfo current in list)
			{
				if (this.FilteredPlanetList(current).Count > 0)
				{
					base.App.UI.AddItem("sys_list_left", "", current.ID, current.Name);
					if (flag)
					{
						this.SetSyncedSystem(current);
						flag = false;
					}
				}
			}
		}
		protected override void OnExit(GameState next, ExitReason reason)
		{
			foreach (PlanetWidget current in this._planetWidgets)
			{
				current.Terminate();
			}
			foreach (SystemWidget current2 in this._systemWidgets)
			{
				current2.Terminate();
			}
		}
		protected override void OnUpdate()
		{
			foreach (PlanetWidget current in this._planetWidgets)
			{
				current.Update();
			}
			foreach (SystemWidget current2 in this._systemWidgets)
			{
				current2.Update();
			}
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "filterDropdown")
				{
					int currentFilterMode = int.Parse(msgParams[0]);
					this._currentFilterMode = (PlanetManagerState.PlanetFilterMode)currentFilterMode;
					this.SyncPlanetList();
				}
				else
				{
					if (panelName == "sys_list_left")
					{
						int systemId = int.Parse(msgParams[0]);
						StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(systemId);
						if (starSystemInfo != null)
						{
							this.SetSyncedSystem(starSystemInfo);
						}
					}
				}
			}
			if (msgType == "button_clicked" && panelName == PlanetManagerState.UIExitButton)
			{
				base.App.SwitchGameState<StarMapState>(new object[0]);
			}
		}
	}
}
