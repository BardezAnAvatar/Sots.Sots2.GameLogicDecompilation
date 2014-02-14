using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarMapElements;
using System;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class ColonizeDialog : Dialog
	{
		public const string OKButton = "event_dialog_close";
		private int _planetID;
		private OrbitCameraController _cameraReduced;
		private StarMap _starmapReduced;
		private Sky _sky;
		private GameObjectSet _crits;
		private PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private PlanetInfo _cachedPlanetInfo;
		private bool _cachedPlanetReady;
		private bool _homeworld;
		private string _enteredColonyName;
		public ColonizeDialog(App game, int planetid, bool homeworld = false) : base(game, "dialogColonizeEvent")
		{
			this._planetID = planetid;
			this._homeworld = homeworld;
		}
		public override void Initialize()
		{
			this._sky = new Sky(this._app, SkyUsage.StarMap, 0);
			this._crits = new GameObjectSet(this._app);
			this._planetView = this._crits.Add<PlanetView>(new object[0]);
			this._crits.Add(this._sky);
			OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._planetID);
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
			this._app.UI.SetText("gameColonyName", orbitalObjectInfo.Name);
			this._enteredColonyName = orbitalObjectInfo.Name;
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"btnAbandon"
			}), false);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				"colonyCreateTitle"
			}), (this._homeworld ? App.Localize("@UI_STARMAP_HOMEWORLD_ESTABLISHED") : App.Localize("@UI_STARMAP_COLONY_ESTABLISHED")) + " - " + starSystemInfo.Name);
			this._cameraReduced = new OrbitCameraController(this._app);
			this._cameraReduced.MinDistance = 2.5f;
			this._cameraReduced.MaxDistance = 100f;
			this._cameraReduced.DesiredDistance = 50f;
			this._cameraReduced.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this._cameraReduced.DesiredPitch = -MathHelper.DegreesToRadians(25f);
			this._cameraReduced.SnapToDesiredPosition();
			this._starmapReduced = new StarMap(this._app, this._app.Game, this._sky);
			this._starmapReduced.Initialize(this._crits, new object[0]);
			this._starmapReduced.SetCamera(this._cameraReduced);
			this._starmapReduced.FocusEnabled = false;
			this._starmapReduced.SetFocus(this._starmapReduced.Systems.Reverse[starSystemInfo.ID]);
			this._starmapReduced.Select(this._starmapReduced.Systems.Reverse[starSystemInfo.ID]);
			this._starmapReduced.SelectEnabled = false;
			this._starmapReduced.PostSetProp("MissionTarget", new object[]
			{
				this._starmapReduced.Systems.Reverse[starSystemInfo.ID].ObjectID,
				true
			});
			StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(starSystemInfo.ID);
			this._starmapReduced.PostSetProp("CullCenter", starSystemInfo2.Origin);
			this._starmapReduced.PostSetProp("CullRadius", 15f);
			string text = this._app.UI.Path(new string[]
			{
				base.ID,
				"gameStarMapViewport"
			});
			this._app.UI.Send(new object[]
			{
				"SetGameObject",
				text,
				this._starmapReduced.ObjectID
			});
			this.CachePlanet(this._app.GameDatabase.GetPlanetInfo(this._planetID));
			this._planetView.PostSetProp("Planet", (this._cachedPlanet != null) ? this._cachedPlanet.ObjectID : 0);
			this._app.UI.Send(new object[]
			{
				"SetGameObject",
				this._app.UI.Path(new string[]
				{
					base.ID,
					"system_details.Planet_panel"
				}),
				this._planetView.ObjectID
			});
			StarSystemMapUI.Sync(this._app, orbitalObjectInfo.StarSystemID, this._app.UI.Path(new string[]
			{
				base.ID,
				"system_map"
			}), true);
			StarSystemUI.SyncSystemDetailsWidget(this._app, "colony_event_dialog.system_details", orbitalObjectInfo.StarSystemID, true, true);
			StarSystemUI.SyncPlanetDetailsControl(this._app.Game, this._app.UI.Path(new string[]
			{
				base.ID,
				"system_details"
			}), this._planetID);
			StarSystemUI.SyncColonyDetailsWidget(this._app.Game, this._app.UI.Path(new string[]
			{
				base.ID,
				"colony_details"
			}), orbitalObjectInfo.ID, "");
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"system_map"
			}), true);
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameStarMapViewport"
			}), false);
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameViewportList"
			}), "", 0, App.Localize("@SYSTEMDETAILS_SYS_MAP"));
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameViewportList"
			}), "", 1, App.Localize("@SYSTEMDETAILS_STAR_MAP"));
			this._app.UI.SetSelection(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameViewportList"
			}), 0);
			this._crits.Activate();
		}
		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (this._cachedPlanet != null)
			{
				if (PlanetInfo.AreSame(planetInfo, this._cachedPlanetInfo))
				{
					return;
				}
				this._app.ReleaseObject(this._cachedPlanet);
				this._cachedPlanet = null;
			}
			this._cachedPlanetInfo = planetInfo;
			this._cachedPlanetReady = false;
			this._cachedPlanet = Kerberos.Sots.GameStates.StarSystem.CreatePlanet(this._app.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, Kerberos.Sots.GameStates.StarSystem.TerrestrialPlanetQuality.High);
			this._cachedPlanet.PostSetProp("AutoDraw", false);
		}
		private void UpdateCachedPlanet()
		{
			if (this._cachedPlanet != null && !this._cachedPlanetReady && this._cachedPlanet.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedPlanetReady = true;
				this._cachedPlanet.Active = true;
			}
		}
		public void Update()
		{
			if (this._starmapReduced != null && !this._starmapReduced.Active && this._starmapReduced.ObjectStatus != GameObjectStatus.Pending)
			{
				this._starmapReduced.Active = true;
			}
			if (this._cameraReduced != null && !this._cameraReduced.Active && this._cameraReduced.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cameraReduced.Active = true;
			}
			this.UpdateCachedPlanet();
		}
		private void Confirm()
		{
			OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._planetID);
			if (orbitalObjectInfo != null)
			{
				orbitalObjectInfo.Name = this._enteredColonyName;
				this._app.GameDatabase.UpdateOrbitalObjectInfo(orbitalObjectInfo);
			}
			if (!string.IsNullOrWhiteSpace(this._enteredColonyName) && this._enteredColonyName.Count<char>() > 0)
			{
				this._app.UI.CloseDialog(this, true);
				return;
			}
			this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@INVALID_COLONY_NAME"), App.Localize("@INVALID_COLONY_NAME_TEXT"), "dialogGenericMessage"), null);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "edit_confirmed")
			{
				this.Confirm();
			}
			if (msgType == "button_clicked")
			{
				if (panelName == "event_dialog_close")
				{
					this.Confirm();
					return;
				}
			}
			else
			{
				if (msgType == "text_changed")
				{
					if (panelName == "gameColonyName")
					{
						this._enteredColonyName = msgParams[0];
						return;
					}
				}
				else
				{
					if (msgType == "slider_value")
					{
						if (StarSystemDetailsUI.IsOutputRateSlider(panelName))
						{
							StarSystemDetailsUI.SetOutputRate(this._app, this._planetID, panelName, msgParams[0]);
							StarSystemUI.SyncColonyDetailsWidget(this._app.Game, this._app.UI.Path(new string[]
							{
								base.ID,
								"colony_details"
							}), this._planetID, panelName);
							return;
						}
						if (panelName == "partOverharvestSlider")
						{
							ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(this._planetID);
							colonyInfoForPlanet.OverharvestRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
							this._app.GameDatabase.UpdateColony(colonyInfoForPlanet);
							StarSystemUI.SyncColonyDetailsWidget(this._app.Game, this._app.UI.Path(new string[]
							{
								base.ID,
								"colony_details"
							}), this._planetID, panelName);
							return;
						}
						if (panelName == "partCivSlider")
						{
							ColonyInfo colonyInfoForPlanet2 = this._app.GameDatabase.GetColonyInfoForPlanet(this._planetID);
							colonyInfoForPlanet2.CivilianWeight = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
							this._app.GameDatabase.UpdateColony(colonyInfoForPlanet2);
							StarSystemUI.SyncColonyDetailsWidget(this._app.Game, this._app.UI.Path(new string[]
							{
								base.ID,
								"colony_details"
							}), this._planetID, panelName);
							return;
						}
						if (panelName == "partWorkRate")
						{
							ColonyInfo colonyInfoForPlanet3 = this._app.GameDatabase.GetColonyInfoForPlanet(this._planetID);
							colonyInfoForPlanet3.SlaveWorkRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
							this._app.GameDatabase.UpdateColony(colonyInfoForPlanet3);
							StarSystemUI.SyncColonyDetailsWidget(this._app.Game, this._app.UI.Path(new string[]
							{
								base.ID,
								"colony_details"
							}), this._planetID, panelName);
							return;
						}
					}
					else
					{
						if (msgType == "list_sel_changed" && panelName == "gameViewportList")
						{
							this.SetColonyViewMode(int.Parse(msgParams[0]));
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
					"gameStarMapViewport"
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
				"gameStarMapViewport"
			}), true);
		}
		public override string[] CloseDialog()
		{
			if (this._cachedPlanet != null)
			{
				this._app.ReleaseObject(this._cachedPlanet);
			}
			this._cachedPlanet = null;
			this._cachedPlanetReady = false;
			this._crits.Dispose();
			this._starmapReduced.Dispose();
			this._cameraReduced.Dispose();
			return null;
		}
	}
}
