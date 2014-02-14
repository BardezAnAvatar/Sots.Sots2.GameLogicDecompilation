using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DialogSystemIntel : Dialog
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
		private SystemWidget _systemWidget;
		private Kerberos.Sots.GameStates.StarSystem _starsystem;
		private OrbitCameraController _camera;
		private Sky _sky;
		private GameObjectSet _crits;
		private bool _critsInitialized;
		private PlayerInfo _targetPlayer;
		private string _descriptor;
		private DialogSystemIntel.PlanetFilterMode _currentFilterMode;
		public DialogSystemIntel(App app, int systemid, PlayerInfo targetPlayer, string descriptor) : base(app, "dialogSystemIntelEvent")
		{
			this._descriptor = descriptor;
			this._systemID = systemid;
			this.App = app;
			this._targetPlayer = targetPlayer;
		}
		public override void Initialize()
		{
			this._systemWidget = new SystemWidget(this.App, this.App.UI.Path(new string[]
			{
				base.ID,
				"starDetailsCard"
			}));
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._systemID);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"intel_desc"
			}), this._descriptor);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"playerAvatar"
			}), "sprite", Path.GetFileNameWithoutExtension(this._targetPlayer.AvatarAssetPath));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"playerBadge"
			}), "sprite", Path.GetFileNameWithoutExtension(this._targetPlayer.BadgeAssetPath));
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
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameViewportList"
			}), "", 1, "System");
			this._app.UI.SetSelection(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameViewportList"
			}), 0);
			this._crits = new GameObjectSet(this._app);
			this._camera = new OrbitCameraController(this._app);
			this._sky = new Sky(this._app, SkyUsage.InSystem, this._systemID);
			this._starsystem = new Kerberos.Sots.GameStates.StarSystem(this.App, 1f, this._systemID, Vector3.Zero, true, null, false, 0, false, true);
			this._starsystem.SetAutoDrawEnabled(false);
			this._starsystem.SetCamera(this._camera);
			this._starsystem.SetInputEnabled(true);
			this._starsystem.PostObjectAddObjects(new IGameObject[]
			{
				this._sky
			});
			foreach (IGameObject current in 
				from x in this._starsystem.Crits.Objects
				where x is StellarBody || x is StarModel
				select x)
			{
				current.PostSetProp("AutoDrawEnabled", false);
			}
			this._crits.Add(new IGameObject[]
			{
				this._camera,
				this._sky,
				this._starsystem
			});
			this._app.UI.Send(new object[]
			{
				"SetGameObject",
				this._app.UI.Path(new string[]
				{
					base.ID,
					"gameStarSystemViewport"
				}),
				this._starsystem.ObjectID
			});
			this._critsInitialized = false;
			this._camera.PostSetLook(new Vector3(0f, 0f, 0f));
			this._camera.PostSetPosition(new Vector3(0f, 100000f, 0f));
			this._camera.MaxDistance = 500000f;
			this._camera.MinDistance = 100000f;
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
			this._currentFilterMode = DialogSystemIntel.PlanetFilterMode.AllPlanets;
			this._planetWidgets = new List<PlanetWidget>();
			this.SetSyncedSystem(starSystemInfo);
			this._systemWidget.Sync(this._systemID);
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
			this._systemWidget.Update();
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
					if (this._currentFilterMode == DialogSystemIntel.PlanetFilterMode.AllPlanets)
					{
						list2.Add(current);
					}
					else
					{
						if (this._currentFilterMode == DialogSystemIntel.PlanetFilterMode.SurveyedPlanets)
						{
							if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID) == null)
							{
								list2.Add(current);
							}
						}
						else
						{
							if (this._currentFilterMode == DialogSystemIntel.PlanetFilterMode.OwnedPlanets)
							{
								AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID);
								if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
								{
									list2.Add(current);
								}
							}
							else
							{
								if (this._currentFilterMode == DialogSystemIntel.PlanetFilterMode.EnemyPlanets)
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
					else
					{
						if (panelName == "gameViewportList")
						{
							this.SetColonyViewMode(int.Parse(msgParams[0]));
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
