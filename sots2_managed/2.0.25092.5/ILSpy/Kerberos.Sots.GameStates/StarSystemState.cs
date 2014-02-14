using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class StarSystemState : BasicStarSystemState, IKeyBindListener
	{
		private const string UIEmpireSummaryButton = "gameEmpireSummaryButton";
		private const string UIAbandonColony = "btnAbandon";
		private const string UIOpenSystemButton = "btnSystemOpen";
		private string _confirmAbandon = "";
		private BudgetPiechart _piechart;
		public StarSystemState(App game) : base(game)
		{
		}
		private void InitializeClimateSlider(string sliderId)
		{
			float minSuitability = Constants.MinSuitability;
			float maxSuitability = Constants.MaxSuitability;
			float arg_15_0 = (maxSuitability - minSuitability) / 2f;
			int minValue = (int)(-(int)maxSuitability);
			int maxValue = (int)maxSuitability;
			int value = 0;
			base.App.UI.InitializeSlider(sliderId, minValue, maxValue, value);
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			base.OnPrepare(prev, stateParams);
			base.App.UI.LoadScreen("StarSystem");
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			base.App.UI.SetScreen("StarSystem");
			this._piechart = new BudgetPiechart(base.App.UI, "piechart", base.App.AssetDatabase);
			EmpireBarUI.SyncTitleBar(base.App, "gameEmpireBar", this._piechart);
			EmpireBarUI.SyncTitleFrame(base.App);
			this.InitializeClimateSlider(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partClimateSlider"
			}));
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partTradeSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partTerraSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partInfraSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partShipConSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partCivSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partOverDevelopment"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool("gameSystemContentsList", "only_user_events", true);
			base.App.UI.InitializeSlider(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partCivSlider"
			}), 0, 100, 50);
			base.App.UI.InitializeSlider(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partTerraSlider"
			}), 0, 100, 50);
			base.App.UI.InitializeSlider(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partInfraSlider"
			}), 0, 100, 50);
			base.App.UI.InitializeSlider(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partShipConSlider"
			}), 0, 100, 50);
			base.App.UI.InitializeSlider(base.App.UI.Path(new string[]
			{
				"colonyControl",
				"partOverDevelopment"
			}), 0, 100, 50);
			this.InitializeClimateSlider(base.App.UI.Path(new string[]
			{
				"gamePlanetDetails",
				"partClimateSlider"
			}));
			StarSystemUI.SyncSystemDetailsWidget(base.App, "systemDetailsWidget", base.CurrentSystem, false, true);
			StarSystemUI.SyncPlanetListWidget(base.App.Game, "planetListWidget", base.App.GameDatabase.GetStarSystemPlanets(base.CurrentSystem));
			StarSystemUI.SyncColonyDetailsWidget(base.App.Game, "colonyDetailsWidget", base.SelectedObject, "");
			StarSystemUI.SyncPlanetDetailsWidget(base.App.Game, "planetDetailsWidget", base.CurrentSystem, base.SelectedObject, base.GetPlanetViewGameObject(base.CurrentSystem, base.SelectedObject), this._planetView);
			base.App.HotKeyManager.AddListener(this);
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			base.OnExit(prev, reason);
			this._piechart = null;
		}
		protected override void OnUIGameEvent(string eventName, string[] eventParams)
		{
			this._piechart.TryGameEvent(eventName, eventParams);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
			{
				return;
			}
			if (msgType == "slider_value")
			{
				if (base.SelectedObject != StarSystemDetailsUI.StarItemID)
				{
					if (StarSystemDetailsUI.IsOutputRateSlider(panelName))
					{
						StarSystemDetailsUI.SetOutputRate(base.App, base.SelectedObject, panelName, msgParams[0]);
						StarSystemUI.SyncColonyDetailsWidget(base.App.Game, "colonyDetailsWidget", base.SelectedObject, "");
						return;
					}
					if (panelName == "partOverharvestSlider")
					{
						ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(base.SelectedObject);
						colonyInfoForPlanet.OverharvestRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
						base.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
						StarSystemUI.SyncColonyDetailsWidget(base.App.Game, "colonyDetailsWidget", base.SelectedObject, "");
						return;
					}
					if (panelName == "gameEmpireResearchSlider")
					{
						StarMapState.SetEmpireResearchRate(base.App.Game, msgParams[0], this._piechart);
						return;
					}
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == "gameEmpireSummaryButton")
					{
						base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
						return;
					}
					if (panelName == "btnAbandon")
					{
						this._confirmAbandon = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, "@UI_DIALOGCONFIRMABANDON_TITLE", "@UI_DIALOGCONFIRMABANDON_DESC", "dialogGenericQuestion"), null);
						return;
					}
					if (panelName == "btnSystemOpen")
					{
						bool flag = !base.App.GameDatabase.GetStarSystemInfo(base.CurrentSystem).IsOpen;
						base.App.GameDatabase.UpdateStarSystemOpen(base.CurrentSystem, flag);
						base.App.UI.SetVisible("SystemDetailsWidget.ClosedSystem", !flag);
						base.App.Game.OCSystemToggleData.SystemToggled(base.App.LocalPlayer.ID, base.CurrentSystem, flag);
						return;
					}
				}
				else
				{
					if (msgType == "dialog_closed" && panelName == this._confirmAbandon && bool.Parse(msgParams[0]))
					{
						PlanetInfo planetInfo = base.App.GameDatabase.GetPlanetInfo(base.SelectedObject);
						if (planetInfo != null)
						{
							ColonyInfo colonyInfoForPlanet2 = base.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
							GameSession.AbandonColony(base.App, colonyInfoForPlanet2.ID);
						}
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
					return false;
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
