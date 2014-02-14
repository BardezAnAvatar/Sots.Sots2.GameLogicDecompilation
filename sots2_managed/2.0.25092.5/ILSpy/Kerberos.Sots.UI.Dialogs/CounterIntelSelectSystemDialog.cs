using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI.Dialogs
{
	internal class CounterIntelSelectSystemDialog : Dialog
	{
		private readonly string LeftContentList = "CIleftContent";
		private readonly string RightContentList = "CIrightContent";
		private readonly string SystemButton = "systembtn";
		private readonly string ToggleButton = "systemtoggle";
		private readonly string HeaderText = "headertxt";
		private readonly string DirectionTest = "dirtext";
		private int intelMissionID;
		private readonly GameSession _game;
		private IntelMissionInfo missioninfo;
		private List<StarSystemInfo> SyncedSystems;
		private List<PlanetWidget> _planetWidgets;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private Dictionary<int, string> _systemcardpanels;
		private Dictionary<int, bool> _checkedsystems;
		private Dictionary<int, string> SystemUnselectedPanels;
		private Dictionary<int, string> SystemSelectedPanels;
		private Dictionary<int, string> SystemContentSelectedPanels;
		public CounterIntelSelectSystemDialog(GameSession game, int intel_mission_id) : base(game.App, "counterIntel_Standard")
		{
			this.intelMissionID = intel_mission_id;
			this.missioninfo = game.GameDatabase.GetIntelInfo(this.intelMissionID);
			this._game = game;
		}
		public override void Initialize()
		{
			this._systemcardpanels = new Dictionary<int, string>();
			this._checkedsystems = new Dictionary<int, bool>();
			this._planetWidgets = new List<PlanetWidget>();
			this.SystemUnselectedPanels = new Dictionary<int, string>();
			this.SystemSelectedPanels = new Dictionary<int, string>();
			this.SystemContentSelectedPanels = new Dictionary<int, string>();
			this._app.UI.SetEnabled(base.UI.Path(new string[]
			{
				base.ID,
				"okbtn"
			}), false);
			this.PopulateSystemList(true);
			this._game.UI.SetText(base.UI.Path(new string[]
			{
				base.ID,
				this.HeaderText
			}), App.Localize("@UI_COUNTER_INTEL_" + this.missioninfo.MissionType.ToString().ToUpper()));
			this._game.UI.SetText(base.UI.Path(new string[]
			{
				base.ID,
				this.DirectionTest
			}), App.Localize("@UI_COUNTER_INTEL_" + this.missioninfo.MissionType.ToString().ToUpper() + "_DIR"));
		}
		private void PopulateSystemList(bool SelectFirst = false)
		{
			List<StarSystemInfo> list = (
				from x in this._game.GameDatabase.GetVisibleStarSystemInfos(this._app.LocalPlayer.ID)
				where this._game.GameDatabase.IsSurveyed(this._game.LocalPlayer.ID, x.ID)
				select x).ToList<StarSystemInfo>();
			this.SyncedSystems = list;
			foreach (StarSystemInfo current in list)
			{
				this._game.UI.AddItem(this.LeftContentList, "", current.ID, "", "TinySystemCard_Toggle");
				string itemGlobalID = this._game.UI.GetItemGlobalID(this.LeftContentList, "", current.ID, "");
				this._systemcardpanels.Add(current.ID, itemGlobalID);
				this._checkedsystems.Add(current.ID, false);
				this._game.UI.SetPropertyString(this._game.UI.Path(new string[]
				{
					itemGlobalID,
					this.SystemButton
				}), "id", this.SystemButton + "|" + current.ID);
				this._game.UI.SetPropertyString(this._game.UI.Path(new string[]
				{
					itemGlobalID,
					this.ToggleButton
				}), "id", this.ToggleButton + "|" + current.ID);
				string globalID = this._game.UI.GetGlobalID(this._game.UI.Path(new string[]
				{
					itemGlobalID,
					"unselected"
				}));
				string globalID2 = this._game.UI.GetGlobalID(this._game.UI.Path(new string[]
				{
					itemGlobalID,
					"selected"
				}));
				this._game.UI.GetGlobalID(this._game.UI.Path(new string[]
				{
					itemGlobalID,
					"contentselected"
				}));
				this.SystemSelectedPanels.Add(current.ID, globalID);
				this.SystemUnselectedPanels.Add(current.ID, globalID2);
				this.SystemContentSelectedPanels.Add(current.ID, globalID2);
				this._game.UI.SetText(this._game.UI.Path(new string[]
				{
					itemGlobalID,
					"itemName"
				}), current.Name);
				StellarClass stellarClass = new StellarClass(current.StellarClass);
				Vector4 vector = StarHelper.CalcModelColor(stellarClass);
				this._game.UI.SetPropertyColor(this._game.UI.Path(new string[]
				{
					itemGlobalID,
					"colorGradient"
				}), "color", new Vector3(vector.X, vector.Y, vector.Z) * 255f);
			}
			if (SelectFirst && list.Any<StarSystemInfo>())
			{
				this.SelectSystem(list.First<StarSystemInfo>().ID);
			}
		}
		private void SelectSystem(int systemid)
		{
			this.SetSyncedSystem(this._game.GameDatabase.GetStarSystemInfo(systemid));
		}
		protected void SetSyncedSystem(StarSystemInfo system)
		{
			this._game.UI.ClearItems(this.RightContentList);
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Terminate();
			}
			this._systemWidgets.Clear();
			foreach (PlanetWidget current2 in this._planetWidgets)
			{
				current2.Terminate();
			}
			this._planetWidgets.Clear();
			this._game.UI.ClearItems(this.RightContentList);
			List<PlanetInfo> list = this._game.GameDatabase.GetStarSystemPlanetInfos(system.ID).ToList<PlanetInfo>();
			this._game.UI.AddItem(this.RightContentList, "", system.ID, "", "systemTitleCard");
			string itemGlobalID = this._game.UI.GetItemGlobalID(this.RightContentList, "", system.ID, "");
			this._systemWidgets.Add(new SystemWidget(this._game.App, itemGlobalID));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo current3 in list)
			{
				if (this._game.AssetDatabase.IsPotentialyHabitable(current3.Type))
				{
					this._game.UI.AddItem(this.RightContentList, "", current3.ID + 999999, "", "planetDetailsM_Card");
					string itemGlobalID2 = this._game.UI.GetItemGlobalID(this.RightContentList, "", current3.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(this._game.App, itemGlobalID2));
					this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
				}
				else
				{
					if (this._game.AssetDatabase.IsGasGiant(current3.Type))
					{
						this._game.UI.AddItem("system_list", "", current3.ID + 999999, "", "gasgiantDetailsM_Card");
						string itemGlobalID3 = this._game.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
						this._planetWidgets.Add(new PlanetWidget(this._game.App, itemGlobalID3));
						this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
					}
					else
					{
						if (this._game.AssetDatabase.IsMoon(current3.Type))
						{
							this._game.UI.AddItem(this.RightContentList, "", current3.ID + 999999, "", "moonDetailsM_Card");
							string itemGlobalID4 = this._game.UI.GetItemGlobalID(this.RightContentList, "", current3.ID + 999999, "");
							this._planetWidgets.Add(new PlanetWidget(this._game.App, itemGlobalID4));
							this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
						}
					}
				}
			}
		}
		private void SetChecked(int systemid, bool forcechecked = false)
		{
			foreach (StarSystemInfo current in this.SyncedSystems)
			{
				if (this._checkedsystems[current.ID] || forcechecked)
				{
					this._game.UI.SetChecked(this._game.UI.Path(new string[]
					{
						this._systemcardpanels[current.ID],
						this.ToggleButton + "|" + current.ID
					}), current.ID == systemid);
				}
				this._checkedsystems[current.ID] = (current.ID == systemid);
			}
			this._app.UI.SetEnabled(base.UI.Path(new string[]
			{
				base.ID,
				"okbtn"
			}), this._checkedsystems.Any((KeyValuePair<int, bool> x) => x.Value));
		}
		private void AutoSelect()
		{
			Random random = new Random();
			this.SetChecked(this.SyncedSystems[random.Next(0, this.SyncedSystems.Count)].ID, true);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName.StartsWith(this.SystemButton))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					int systemid = int.Parse(array[1]);
					this.SelectSystem(systemid);
				}
				else
				{
					if (panelName.StartsWith(this.ToggleButton))
					{
						string[] array2 = panelName.Split(new char[]
						{
							'|'
						});
						int systemid2 = int.Parse(array2[1]);
						this.SetChecked(systemid2, false);
					}
					else
					{
						if (panelName == "autoselectbtn")
						{
							this.AutoSelect();
						}
						else
						{
							if (panelName == "okbtn")
							{
								this.DeployCounterIntel();
								this._game.UI.CloseDialog(this, true);
							}
						}
					}
				}
			}
			bool flag1 = msgType == "list_sel_changed";
		}
		private void DeployCounterIntel()
		{
			List<CounterIntelResponse> list = this._game.GameDatabase.GetCounterIntelResponses(this.intelMissionID).ToList<CounterIntelResponse>();
			foreach (CounterIntelResponse current in list)
			{
				this._app.GameDatabase.RemoveCounterIntelResponse(current.ID);
			}
			this._app.GameDatabase.InsertCounterIntelResponse(this.intelMissionID, false, this._checkedsystems.First((KeyValuePair<int, bool> x) => x.Value).Key.ToString());
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
		public override string[] CloseDialog()
		{
			foreach (PlanetWidget current in this._planetWidgets)
			{
				current.Terminate();
			}
			foreach (SystemWidget current2 in this._systemWidgets)
			{
				current2.Terminate();
			}
			return null;
		}
	}
}
