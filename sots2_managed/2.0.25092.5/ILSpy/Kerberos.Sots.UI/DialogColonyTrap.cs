using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DialogColonyTrap : Dialog
	{
		private enum PlanetFilterMode
		{
			AllPlanets,
			SurveyedPlanets,
			OwnedPlanets,
			EnemyPlanets
		}
		public const string OKButton = "okButton";
		private int _systemID;
		private List<PlanetWidget> _planetWidgets;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private App App;
		private GameObjectSet _crits;
		private bool _critsInitialized;
		private List<ColonyTrapInfo> _existingTraps;
		private List<int> _placedTraps;
		private int _fleetID;
		private DialogColonyTrap.PlanetFilterMode _currentFilterMode;
		public DialogColonyTrap(App app, int systemid, int fleetid) : base(app, "dialogColonyTrapper")
		{
			this._systemID = systemid;
			this.App = app;
			this._fleetID = fleetid;
		}
		public override void Initialize()
		{
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._systemID);
			this._crits = new GameObjectSet(this._app);
			this._existingTraps = (
				from x in this.App.GameDatabase.GetColonyTrapInfosAtSystem(this._systemID)
				where this.App.GameDatabase.GetFleetInfo(x.FleetID).PlayerID == this.App.LocalPlayer.ID
				select x).ToList<ColonyTrapInfo>();
			this._placedTraps = new List<int>();
			this._currentFilterMode = DialogColonyTrap.PlanetFilterMode.AllPlanets;
			this._planetWidgets = new List<PlanetWidget>();
			this.SetSyncedSystem(starSystemInfo);
			this.UpdateNumRemainingTraps();
		}
		private void UpdateNumRemainingTraps()
		{
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			if (this._fleetID != 0 && this.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name.ToLower() == "morrigi")
			{
				List<ShipInfo> list = new List<ShipInfo>();
				List<ShipInfo> list2 = this.App.GameDatabase.GetShipInfoByFleetID(this._fleetID, true).ToList<ShipInfo>();
				foreach (ShipInfo current in list2)
				{
					ShipSectionAsset shipSectionAsset = (
						from x in current.DesignInfo.DesignSections
						select x.ShipSectionAsset).FirstOrDefault((ShipSectionAsset x) => x.IsTrapShip);
					if (shipSectionAsset != null)
					{
						list.Add(current);
					}
				}
				int num = list.Count - this._placedTraps.Count;
				this.App.UI.SetText(base.UI.Path(new string[]
				{
					base.ID,
					"remainingTraps"
				}), num.ToString() + " Remaining " + ((num == 1) ? "Trap" : "Traps"));
				this.SetTrapInputsEnabled(num != 0);
			}
		}
		private void SetTrapInputsEnabled(bool enabled)
		{
			foreach (PlanetWidget widget in this._planetWidgets)
			{
				if (!this._existingTraps.Any((ColonyTrapInfo x) => x.PlanetID == widget.GetPlanetID()))
				{
					if (!enabled)
					{
						if (this._placedTraps.Any((int x) => x == widget.GetPlanetID()))
						{
							continue;
						}
					}
					this.App.UI.SetPropertyBool("applyTrap|" + widget.GetPlanetID().ToString(), "input_enabled", enabled);
					this.App.UI.SetEnabled("applyTrap|" + widget.GetPlanetID().ToString(), enabled);
				}
			}
		}
		protected override void OnUpdate()
		{
			if (!this._critsInitialized && this._crits.IsReady())
			{
				this._critsInitialized = true;
				this._crits.Activate();
			}
			foreach (PlanetWidget current in this._planetWidgets)
			{
				current.Update();
			}
			foreach (SystemWidget current2 in this._systemWidgets)
			{
				current2.Update();
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
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			foreach (PlanetInfo planet in list)
			{
				if (this.App.AssetDatabase.IsPotentialyHabitable(planet.Type))
				{
					this.App.UI.AddItem("system_list", "", planet.ID + 999999, "", "planetDetailsM_Card");
					string itemGlobalID2 = this.App.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID2));
					this._planetWidgets.Last<PlanetWidget>().Sync(planet.ID, false, false);
					string itemGlobalID3 = this.App.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "");
					this.App.UI.SetPropertyString(base.UI.Path(new string[]
					{
						itemGlobalID3,
						"applyTrap"
					}), "id", "applyTrap|" + planet.ID.ToString());
					if (this._existingTraps.Any((ColonyTrapInfo x) => x.PlanetID == planet.ID))
					{
						this.App.UI.SetChecked("applyTrap|" + planet.ID.ToString(), true);
						this.App.UI.SetPropertyBool("applyTrap|" + planet.ID.ToString(), "input_enabled", false);
					}
					else
					{
						this.App.UI.SetChecked("applyTrap|" + planet.ID.ToString(), false);
					}
					if (this.App.GameDatabase.GetColonyInfoForPlanet(planet.ID) == null && this.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name.ToLower() == "morrigi")
					{
						this.App.UI.SetVisible(base.UI.Path(new string[]
						{
							itemGlobalID3,
							"applyTrap|" + planet.ID.ToString()
						}), true);
					}
				}
				else
				{
					if (this.App.AssetDatabase.IsGasGiant(planet.Type))
					{
						this.App.UI.AddItem("system_list", "", planet.ID + 999999, "", "gasgiantDetailsM_Card");
						string itemGlobalID4 = this.App.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "");
						this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID4));
						this._planetWidgets.Last<PlanetWidget>().Sync(planet.ID, false, false);
					}
					else
					{
						if (this.App.AssetDatabase.IsMoon(planet.Type))
						{
							this.App.UI.AddItem("system_list", "", planet.ID + 999999, "", "moonDetailsM_Card");
							string itemGlobalID5 = this.App.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "");
							this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID5));
							this._planetWidgets.Last<PlanetWidget>().Sync(planet.ID, false, false);
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
					if (this._currentFilterMode == DialogColonyTrap.PlanetFilterMode.AllPlanets)
					{
						list2.Add(current);
					}
					else
					{
						if (this._currentFilterMode == DialogColonyTrap.PlanetFilterMode.SurveyedPlanets)
						{
							if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID) == null)
							{
								list2.Add(current);
							}
						}
						else
						{
							if (this._currentFilterMode == DialogColonyTrap.PlanetFilterMode.OwnedPlanets)
							{
								AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID);
								if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
								{
									list2.Add(current);
								}
							}
							else
							{
								if (this._currentFilterMode == DialogColonyTrap.PlanetFilterMode.EnemyPlanets)
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
		private void SetTraps()
		{
			if (this._fleetID != 0)
			{
				List<ShipInfo> list = new List<ShipInfo>();
				List<ShipInfo> list2 = this.App.GameDatabase.GetShipInfoByFleetID(this._fleetID, true).ToList<ShipInfo>();
				foreach (ShipInfo current in list2)
				{
					ShipSectionAsset shipSectionAsset = (
						from x in current.DesignInfo.DesignSections
						select x.ShipSectionAsset).FirstOrDefault((ShipSectionAsset x) => x.IsTrapShip);
					if (shipSectionAsset != null)
					{
						list.Add(current);
					}
				}
				foreach (int current2 in this._placedTraps)
				{
					ShipInfo shipInfo = list.First<ShipInfo>();
					this.App.Game.SetAColonyTrap(shipInfo, this.App.LocalPlayer.ID, this._systemID, current2);
					list.Remove(shipInfo);
					if (list.Count == 0)
					{
						break;
					}
				}
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "ITS_A_TRAP")
				{
					this.SetTraps();
					this.App.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				if (msgType == "checkbox_clicked")
				{
					bool flag = int.Parse(msgParams[0]) > 0;
					if (panelName.StartsWith("applyTrap"))
					{
						string s = panelName.Split(new char[]
						{
							'|'
						})[1];
						int item;
						bool flag2 = int.TryParse(s, out item);
						if (flag2)
						{
							if (flag)
							{
								if (!this._placedTraps.Contains(item))
								{
									this._placedTraps.Add(item);
								}
							}
							else
							{
								if (this._placedTraps.Contains(item))
								{
									this._placedTraps.Remove(item);
								}
							}
							this.UpdateNumRemainingTraps();
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
