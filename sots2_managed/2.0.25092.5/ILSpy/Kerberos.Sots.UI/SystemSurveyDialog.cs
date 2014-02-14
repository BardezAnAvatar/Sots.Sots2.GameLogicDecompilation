using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class SystemSurveyDialog : Dialog
	{
		private enum PlanetFilterMode
		{
			AllPlanets,
			SurveyedPlanets,
			OwnedPlanets,
			EnemyPlanets
		}
		public const string OKButton = "okButton";
		public const string TRAPButton = "colonyTrapper";
		private int _systemID;
		private List<PlanetWidget> _planetWidgets;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private App App;
		private SystemWidget _systemWidget;
		private Kerberos.Sots.GameStates.StarSystem _starsystem;
		private OrbitCameraController _camera;
		private Sky _sky;
		private GameObjectSet _crits;
		private bool _critsInitialized;
		private int _fleetID;
		private string _colonytrapDialog = "";
		private SystemSurveyDialog.PlanetFilterMode _currentFilterMode;
		private static bool FORCETRAPHACK;
		public SystemSurveyDialog(App app, int systemid, int fleetID) : base(app, "dialogSurveyEvent")
		{
			this._systemID = systemid;
			this.App = app;
			this._fleetID = fleetID;
		}
		public override void Initialize()
		{
			this._systemWidget = new SystemWidget(this.App, this.App.UI.Path(new string[]
			{
				base.ID,
				"starDetailsCard"
			}));
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._systemID);
			string str = string.Format(App.Localize("@SURVEY_SYSTEM_THINGY"), starSystemInfo.Name).ToUpperInvariant();
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"system_title"
			}), "text", App.Localize("@SURVEY_OF") + " " + str);
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"system_map"
			}), true);
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameStarSystemViewport"
			}), false);
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameViewportList"
			}), "", 0, App.Localize("@SYSTEMDETAILS_SYS_MAP"));
			this._app.UI.SetSelection(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameViewportList"
			}), 0);
			StarSystemUI.SyncSystemDetailsWidget(this._app, this._app.UI.Path(new string[]
			{
				base.ID,
				"system_details"
			}), this._systemID, false, true);
			StarSystemMapUI.Sync(this._app, this._systemID, this._app.UI.Path(new string[]
			{
				base.ID,
				"system_map"
			}), true);
			this._currentFilterMode = SystemSurveyDialog.PlanetFilterMode.AllPlanets;
			this._planetWidgets = new List<PlanetWidget>();
			this.SetSyncedSystem(starSystemInfo);
			this._systemWidget.Sync(this._systemID);
			this.UpdateCanPlaceTraps();
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
			this._systemWidget.Update();
		}
		protected void UpdateCanPlaceTraps()
		{
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			if (SystemSurveyDialog.FORCETRAPHACK || (this._fleetID != 0 && this.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name.ToLower() == "morrigi"))
			{
				bool flag = false;
				foreach (PlanetWidget current in this._planetWidgets)
				{
					PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(current.GetPlanetID());
					if (planetInfo != null && this.App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type) && this.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID) == null)
					{
						flag = true;
					}
				}
				List<ShipInfo> list = new List<ShipInfo>();
				List<ShipInfo> list2 = this.App.GameDatabase.GetShipInfoByFleetID(this._fleetID, true).ToList<ShipInfo>();
				foreach (ShipInfo current2 in list2)
				{
					ShipSectionAsset shipSectionAsset = (
						from x in current2.DesignInfo.DesignSections
						select x.ShipSectionAsset).FirstOrDefault((ShipSectionAsset x) => x.IsTrapShip);
					if (shipSectionAsset != null)
					{
						list.Add(current2);
					}
				}
				if (list.Count > 0 && flag)
				{
					this.App.UI.SetVisible(this.App.UI.Path(new string[]
					{
						base.ID,
						"colonyTrapper"
					}), true);
					return;
				}
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					base.ID,
					"colonyTrapper"
				}), false);
			}
		}
		protected void SetSyncedSystem(StarSystemInfo system)
		{
			this.App.UI.ClearItems("system_list");
			this.App.UI.ClearDisabledItems("system_list");
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
			this.App.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			string itemGlobalID = this.App.UI.GetItemGlobalID("system_list", "", system.ID, "");
			this._systemWidgets.Add(new SystemWidget(this.App, itemGlobalID));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo current3 in list)
			{
				if (this.App.AssetDatabase.IsPotentialyHabitable(current3.Type))
				{
					this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "planetDetailsM_Card");
					string itemGlobalID2 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID2));
					this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
				}
				else
				{
					if (this.App.AssetDatabase.IsGasGiant(current3.Type))
					{
						this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "gasgiantDetailsM_Card");
						string itemGlobalID3 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
						this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID3));
						this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
					}
					else
					{
						if (this.App.AssetDatabase.IsMoon(current3.Type))
						{
							this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "moonDetailsM_Card");
							string itemGlobalID4 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
							this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID4));
							this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
						}
					}
				}
			}
		}
		private List<PlanetInfo> FilteredPlanetList(StarSystemInfo system)
		{
			List<PlanetInfo> list = this.App.GameDatabase.GetStarSystemPlanetInfos(system.ID).ToList<PlanetInfo>();
			List<PlanetInfo> list2 = new List<PlanetInfo>();
			foreach (PlanetInfo current in list)
			{
				if (this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, system.ID))
				{
					if (this._currentFilterMode == SystemSurveyDialog.PlanetFilterMode.AllPlanets)
					{
						list2.Add(current);
					}
					else
					{
						if (this._currentFilterMode == SystemSurveyDialog.PlanetFilterMode.SurveyedPlanets)
						{
							if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID) == null)
							{
								list2.Add(current);
							}
						}
						else
						{
							if (this._currentFilterMode == SystemSurveyDialog.PlanetFilterMode.OwnedPlanets)
							{
								AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID);
								if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
								{
									list2.Add(current);
								}
							}
							else
							{
								if (this._currentFilterMode == SystemSurveyDialog.PlanetFilterMode.EnemyPlanets)
								{
									AIColonyIntel colonyIntelForPlanet2 = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID);
									if (colonyIntelForPlanet2 != null && colonyIntelForPlanet2.OwningPlayerID != this.App.LocalPlayer.ID)
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
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._app.UI.CloseDialog(this, true);
				}
				if (panelName == "detailbutton")
				{
					return;
				}
				if (panelName == "placeColonyTrapsbtn")
				{
					this._colonytrapDialog = this._app.UI.CreateDialog(new DialogColonyTrap(this._app, this._systemID, this._fleetID), null);
					return;
				}
			}
			else
			{
				if (msgType == "dialog_closed")
				{
					if (panelName == this._colonytrapDialog)
					{
						this.UpdateCanPlaceTraps();
						return;
					}
				}
				else
				{
					if (msgType == "list_sel_changed")
					{
						if (panelName == "gamePlanetList")
						{
							int num = int.Parse(msgParams[0]);
							OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(num);
							bool flag = false;
							if (orbitalObjectInfo != null && orbitalObjectInfo.ParentID.HasValue)
							{
								PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ParentID.Value);
								if (planetInfo != null)
								{
									this._app.UI.Send(new object[]
									{
										"SetSel",
										this._app.UI.Path(new string[]
										{
											base.ID,
											"system_map"
										}),
										1,
										planetInfo.ID
									});
									flag = true;
								}
							}
							if (!flag)
							{
								this._app.UI.Send(new object[]
								{
									"SetSel",
									this._app.UI.Path(new string[]
									{
										base.ID,
										"system_map"
									}),
									1,
									num
								});
								return;
							}
						}
					}
					else
					{
						if (msgType == "mapicon_clicked")
						{
							int num2 = int.Parse(msgParams[0]);
							this._app.UI.Send(new object[]
							{
								"SetSel",
								this._app.UI.Path(new string[]
								{
									base.ID,
									"planetListWidget"
								}),
								num2
							});
						}
					}
				}
			}
		}
		private void SetColonyViewMode(int mode)
		{
			if (mode == 0)
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"system_map"
				}), true);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"gameStarSystemViewport"
				}), false);
				return;
			}
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"system_map"
			}), false);
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameStarSystemViewport"
			}), true);
		}
		public override string[] CloseDialog()
		{
			if (this._crits != null)
			{
				this._crits.Dispose();
			}
			this._crits = null;
			this._starsystem = null;
			this._camera = null;
			this._sky = null;
			foreach (PlanetWidget current in this._planetWidgets)
			{
				current.Terminate();
			}
			foreach (SystemWidget current2 in this._systemWidgets)
			{
				current2.Terminate();
			}
			this._systemWidget.Terminate();
			return null;
		}
	}
}
