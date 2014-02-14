using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class OverlayDeployAccelMission : OverlayMission
	{
		private List<int> _GatePoints = new List<int>();
		private int _currentslider;
		private int gatecost;
		private List<int> _PotentialGates = new List<int>();
		public OverlayDeployAccelMission(App game, StarMapState state, StarMap starmap, string template = "OverlayAcceleratorMission") : base(game, state, starmap, MissionType.DEPLOY_NPG, template)
		{
		}
		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}
		protected override bool CanConfirmMission()
		{
			int num = this.gatecost * (this._GatePoints.Count + 1);
            int num2 = base.IsValidFleetID(base.SelectedFleet) ? Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this.App.Game, base.SelectedFleet) : 0;
			return base.IsValidFleetID(base.SelectedFleet) && base.TargetSystem != 0 && num <= num2;
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", base.TargetSystem, false, true);
			this._currentslider = 0;
			this._GatePoints.Clear();
			this.gatecost = this.App.GameDatabase.GetDesignInfosForPlayer(this.App.LocalPlayer.ID).FirstOrDefault((DesignInfo x) => x.IsAccelerator()).ProductionCost;
		}
		private void DistributeSliderNotches()
		{
			this.App.UI.ClearSliderNotches(base.UI.Path(new string[]
			{
				base.ID,
				"LYSlider"
			}));
			if (base.SelectedFleet != 0 && base.TargetSystem != 0)
			{
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"LYSlider"
				}), true);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gamePlaceAccelerator"
				}), true);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"AccelListBox"
				}), true);
				FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(base.SelectedFleet);
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
				StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(base.TargetSystem);
				float length = (starSystemInfo.Origin - starSystemInfo2.Origin).Length;
				List<int> list = new List<int>();
				list.Add(0);
                List<Vector3> list2 = Kerberos.Sots.StarFleet.StarFleet.GetAccelGateSlotsBetweenSystems(this._app.GameDatabase, starSystemInfo.ID, starSystemInfo2.ID).ToList<Vector3>();
				int count = list2.Count;
				for (int i = 0; i < count; i++)
				{
					list.Add((int)((starSystemInfo.Origin - list2[i]).Length / length * 100f));
				}
				list.Add(100);
				foreach (int current in list)
				{
					this.App.UI.AddSliderNotch(base.UI.Path(new string[]
					{
						base.ID,
						"LYSlider"
					}), current);
				}
				this._PotentialGates = list;
				this.App.UI.SetSliderAutoSnap(base.UI.Path(new string[]
				{
					base.ID,
					"LYSlider"
				}), true);
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"LYSlider",
					"right_label"
				}), this.GetLYValueFromPercent(this._currentslider).ToString("0.00"));
				return;
			}
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"LYSlider"
			}), false);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"gamePlaceAccelerator"
			}), false);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"AccelListBox"
			}), false);
		}
		private void UpdateAccelList()
		{
			this._app.UI.ClearItems("AccelList");
			foreach (int current in this._PotentialGates)
			{
				this._app.UI.AddItem("AccelList", "", current, "", "gate_Toggle");
				string itemGlobalID = this._app.UI.GetItemGlobalID("AccelList", "", current, "");
				this.App.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"loa_gate_button",
					"idle",
					"menulabel"
				}), this.GetLYValueFromPercent(current).ToString("0.00") + "LY - Gate");
				this.App.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"loa_gate_button",
					"mouse_over",
					"menulabel"
				}), this.GetLYValueFromPercent(current).ToString("0.00") + "LY - Gate");
				this.App.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"loa_gate_button",
					"pressed",
					"menulabel"
				}), this.GetLYValueFromPercent(current).ToString("0.00") + "LY - Gate");
				this.App.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"loa_gate_button",
					"disabled",
					"menulabel"
				}), this.GetLYValueFromPercent(current).ToString("0.00") + "LY - Gate");
				this.App.UI.SetChecked(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"Loa_gate_toggle"
				}), current == 0 || current == 100 || this._GatePoints.Contains(current));
				this.App.UI.SetEnabled(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"Loa_gate_toggle"
				}), current != 0 && current != 100);
				if (this._currentslider == current)
				{
					this.App.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"selected"
					}), true);
					this.App.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"unselected"
					}), false);
				}
				else
				{
					this.App.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"selected"
					}), false);
					this.App.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"unselected"
					}), true);
				}
				this.App.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"loa_gate_button"
				}), "id", "loa_gate_button|" + current.ToString());
				this.App.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"Loa_gate_toggle"
				}), "id", "Loa_gate_toggle|" + current.ToString());
			}
			DesignInfo designInfo = this._app.GameDatabase.GetDesignInfosForPlayer(this._app.LocalPlayer.ID, RealShipClasses.Cruiser, true).FirstOrDefault((DesignInfo x) => x.IsAccelerator());
			int num = 0;
			if (designInfo != null)
			{
				num = (this._GatePoints.Count<int>() + 1) * designInfo.ProductionCost;
			}
			this.App.UI.SetText("accelCubeCost", string.Format(App.Localize("@UI_MISSION_DEPLOYACCEL_REQUIREDCUBES"), num.ToString("N0")));
			base.UpdateCanConfirmMission();
		}
		protected override void OnCommitMission()
		{
            Kerberos.Sots.StarFleet.StarFleet.SetNPGMission(this.App.Game, base.SelectedFleet, base.TargetSystem, this._useDirectRoute, this._GatePoints, base.GetDesignsToBuild(), null);
			this.App.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
		}
		protected override void RefreshMissionDetails(StationType stationType = StationType.INVALID_TYPE, int stationLevel = 1)
		{
			base.RefreshMissionDetails(stationType, stationLevel);
			this.DistributeSliderNotches();
			this.UpdateAccelList();
		}
		protected override string GetMissionDetailsTitle()
		{
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(base.TargetSystem);
			return string.Format("DEPLOY NPG {0}", starSystemInfo.Name.ToUpperInvariant());
		}
		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			base.AddCommonMissionTimes(estimate);
			string hint = "NPG MISSION";
			base.AddMissionTime(2, App.Localize("DEPLOY NPG MISSION"), estimate.TurnsAtTarget, hint);
			base.AddMissionCost(estimate);
			base.UpdateCanConfirmMission();
		}
		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}
		private float GetLYValueFromPercent(int percent)
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(base.SelectedFleet);
			if (fleetInfo == null)
			{
				return 0f;
			}
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
			StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(base.TargetSystem);
			if (fleetInfo == null || starSystemInfo == null || starSystemInfo2 == null)
			{
				return 0f;
			}
			float length = (starSystemInfo2.Origin - starSystemInfo.Origin).Length;
			float num = length * ((float)percent / 100f);
			Vector3 v = starSystemInfo2.Origin - starSystemInfo.Origin;
			Vector3 v2 = v * (num / length) + starSystemInfo.Origin;
			return (v2 - starSystemInfo.Origin).Length;
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "slider_notched")
			{
				if (panelName == "LYSlider")
				{
					this._currentslider = int.Parse(msgParams[0]);
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						base.ID,
						"LYSlider",
						"right_label"
					}), this.GetLYValueFromPercent(this._currentslider).ToString("0.00"));
					this.UpdateAccelList();
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == "gamePlaceAccelerator")
					{
						if (this._currentslider != 0 && this._currentslider != 100)
						{
							if (!this._GatePoints.Contains(this._currentslider))
							{
								this._GatePoints.Add(this._currentslider);
							}
							else
							{
								this._GatePoints.Remove(this._currentslider);
							}
							this.UpdateAccelList();
						}
					}
					else
					{
						if (panelName.StartsWith("Loa_gate_toggle"))
						{
							string[] array = panelName.Split(new char[]
							{
								'|'
							});
							int num = int.Parse(array[1]);
							if (this._PotentialGates.Contains(num))
							{
								if (this._GatePoints.Contains(num))
								{
									this._GatePoints.Remove(num);
								}
								else
								{
									this._GatePoints.Add(num);
								}
								this._currentslider = num;
								this.App.UI.SetSliderValue(base.UI.Path(new string[]
								{
									base.ID,
									"LYSlider"
								}), this._currentslider);
								this.UpdateAccelList();
							}
						}
						else
						{
							if (panelName.StartsWith("loa_gate_button"))
							{
								string[] array2 = panelName.Split(new char[]
								{
									'|'
								});
								int num2 = int.Parse(array2[1]);
								if (this._PotentialGates.Contains(num2))
								{
									this._currentslider = num2;
									this.App.UI.SetSliderValue(base.UI.Path(new string[]
									{
										base.ID,
										"LYSlider"
									}), this._currentslider);
									this.UpdateAccelList();
								}
							}
						}
					}
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}
	}
}
