using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class PlanetWidget
	{
		public const string UITradeSlider = "partTradeSlider";
		public const string UITerraSlider = "partTerraSlider";
		public const string UIInfraSlider = "partInfraSlider";
		public const string UIOverDevSlider = "partOverDevelopment";
		public const string UIShipConSlider = "partShipConSlider";
		public const string UIOverharvestSlider = "partOverharvestSlider";
		public const string UICivPopulationSlider = "partCivSlider";
		public const string UISlaveWorkSlider = "partWorkRate";
		private string _rootPanel = "";
		private int _planetID;
		public App App;
		private StellarBody _cachedPlanet;
		private PlanetInfo _cachedPlanetInfo;
		private bool _cachedPlanetReady;
		private PlanetView _planetView;
		private GameObjectSet _crits;
		private bool _initialized;
		private bool _planetViewLinked;
		private static int kWidgetID;
		private int _widgetID;
		public PlanetWidget(App app, string rootPanel)
		{
			this._rootPanel = rootPanel;
			this.App = app;
			this._crits = new GameObjectSet(this.App);
			this._planetView = this._crits.Add<PlanetView>(new object[0]);
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			PlanetWidget.kWidgetID++;
			this._widgetID = PlanetWidget.kWidgetID;
		}
		public int GetPlanetID()
		{
			return this._planetID;
		}
		public void Sync(int planetID, bool PopulationSliders = false, bool ShowColonizebuttons = false)
		{
			StarSystemUI.ClearColonyDetailsControl(this.App.Game, this._rootPanel);
			PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(planetID);
			if (planetInfo == null)
			{
				return;
			}
			this._planetID = planetID;
			this.CachePlanet(planetInfo);
			StarSystemUI.SyncPlanetDetailsControlNew(this.App.Game, this._rootPanel, planetID);
			this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
			{
				this._rootPanel,
				"systemName"
			}), "text", this.App.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID).Name);
			ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(planetID);
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				this._rootPanel,
				"rebellionActive"
			}), colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this.App.LocalPlayer.ID && colonyInfoForPlanet.RebellionType != RebellionType.None);
			int num = 0;
			if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this.App.LocalPlayer.ID)
			{
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					this._rootPanel,
					"partOverharvestSlider"
				}), "id", "__partOverharvestSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					this._rootPanel,
					"partTradeSlider"
				}), "id", "__partTradeSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					this._rootPanel,
					"partTerraSlider"
				}), "id", "__partTerraSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					this._rootPanel,
					"partInfraSlider"
				}), "id", "__partInfraSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					this._rootPanel,
					"partShipConSlider"
				}), "id", "__partShipConSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					this._rootPanel,
					"partCivSlider"
				}), "id", "__partCivSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfoForPlanet.ID, this._widgetID, "");
				this.App.UI.SetPropertyBool(this._rootPanel, "expanded", true);
				if (PopulationSliders)
				{
					ColonyFactionInfo[] factions = colonyInfoForPlanet.Factions;
					for (int i = 0; i < factions.Length; i++)
					{
						ColonyFactionInfo colonyFactionInfo = factions[i];
						this.App.UI.AddItem(this.App.UI.Path(new string[]
						{
							this._rootPanel,
							"MoraleRow"
						}), "", colonyFactionInfo.FactionID, "", "popItem");
						string itemGlobalID = this.App.UI.GetItemGlobalID(this.App.UI.Path(new string[]
						{
							this._rootPanel,
							"MoraleRow"
						}), "", colonyFactionInfo.FactionID, "");
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							itemGlobalID,
							"partPopSlider"
						}), "id", string.Concat(new object[]
						{
							"__partPopSlider|",
							this._widgetID.ToString(),
							"|",
							colonyInfoForPlanet.ID.ToString(),
							"|",
							colonyFactionInfo.FactionID
						}));
						Faction faction = this.App.AssetDatabase.GetFaction(colonyFactionInfo.FactionID);
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							itemGlobalID,
							"factionicon"
						}), "sprite", "logo_" + faction.Name.ToLower());
						this.App.UI.SetSliderValue(this.App.UI.Path(new string[]
						{
							itemGlobalID,
							string.Concat(new object[]
							{
								"__partPopSlider|",
								this._widgetID.ToString(),
								"|",
								colonyInfoForPlanet.ID.ToString(),
								"|",
								colonyFactionInfo.FactionID
							})
						}), (int)(colonyFactionInfo.CivPopWeight * 100f));
						this.App.UI.SetText(this.App.UI.Path(new string[]
						{
							itemGlobalID,
							"gameMorale_human"
						}), colonyFactionInfo.Morale.ToString());
						double num2 = (colonyInfoForPlanet.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld) ? (Colony.GetMaxCivilianPop(this.App.GameDatabase, planetInfo) * (double)this.App.AssetDatabase.GemWorldCivMaxBonus) : Colony.GetMaxCivilianPop(this.App.GameDatabase, planetInfo);
						num2 *= (double)colonyInfoForPlanet.CivilianWeight;
						num2 *= (double)colonyFactionInfo.CivPopWeight;
						num2 *= (double)this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerFactionID(colonyInfoForPlanet.PlayerID)).GetImmigrationPopBonusValueForFaction(this.App.AssetDatabase.GetFaction(colonyFactionInfo.FactionID));
						this.App.UI.SetText(this.App.UI.Path(new string[]
						{
							itemGlobalID,
							"popRatio"
						}), (colonyFactionInfo.CivilianPop / 1000000.0).ToString("0.0") + "M / " + (num2 / 1000000.0).ToString("0.0") + "M");
						num++;
					}
					this.App.UI.SetShape(this._rootPanel, 0, 0, 360, 90 + 22 * ((colonyInfoForPlanet.Factions.Count<ColonyFactionInfo>() > 1) ? colonyInfoForPlanet.Factions.Count<ColonyFactionInfo>() : 0));
					return;
				}
			}
			else
			{
				if (colonyInfoForPlanet == null && ShowColonizebuttons && this.App.CurrentState.GetType() == typeof(StarMapState))
				{
					StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID);
                    bool value = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, starSystemInfo.ID, MissionType.COLONIZATION, false).Any<FleetInfo>() && StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this.App, starSystemInfo.ID, this.App.LocalPlayer.ID).Contains(planetID);
					this.App.UI.SetVisible(this.App.UI.Path(new string[]
					{
						this._rootPanel,
						"btnColoninzePlanet"
					}), value);
					this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
					{
						this._rootPanel,
						"btnColoninzePlanet"
					}), "id", "btnColoninzePlanet|" + starSystemInfo.ID.ToString() + "|" + planetInfo.ID.ToString());
				}
			}
		}
		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (this._cachedPlanet != null)
			{
				if (PlanetInfo.AreSame(planetInfo, this._cachedPlanetInfo))
				{
					return;
				}
				this.App.ReleaseObject(this._cachedPlanet);
				this._cachedPlanet = null;
			}
			this._cachedPlanetInfo = planetInfo;
			this._cachedPlanetReady = false;
			this._cachedPlanet = Kerberos.Sots.GameStates.StarSystem.CreatePlanet(this.App.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, Kerberos.Sots.GameStates.StarSystem.TerrestrialPlanetQuality.High);
			this._cachedPlanet.PostSetProp("AutoDraw", false);
			this._initialized = false;
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				this._rootPanel,
				"loadingCircle"
			}), true);
		}
		public void Update()
		{
			if (this._crits == null || !this._crits.IsReady())
			{
				return;
			}
			if (this._cachedPlanet != null && !this._cachedPlanetReady && this._cachedPlanet.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedPlanetReady = true;
				this._cachedPlanet.Active = true;
			}
			if (this._cachedPlanetReady && !this._initialized)
			{
				this._planetView.PostSetProp("Planet", (this._cachedPlanet != null) ? this._cachedPlanet.ObjectID : 0);
				if (!this._planetViewLinked)
				{
					this.App.UI.Send(new object[]
					{
						"SetGameObject",
						this.App.UI.Path(new string[]
						{
							this._rootPanel,
							"contentPreview.desc_viewport"
						}),
						this._planetView.ObjectID
					});
					this._planetViewLinked = true;
				}
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					this._rootPanel,
					"loadingCircle"
				}), false);
				this._initialized = true;
			}
		}
		public static bool IsOutputRateSlider(string panelName)
		{
			return panelName.Contains("partTradeSlider") || panelName.Contains("partTerraSlider") || panelName.Contains("partInfraSlider") || panelName.Contains("partOverDevelopment") || panelName.Contains("partShipConSlider");
		}
		protected void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "slider_value")
			{
				if (panelName.StartsWith("__"))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					int num = int.Parse(array[1]);
					if (num == this._widgetID)
					{
						int colonyID = int.Parse(array[2]);
						ColonyInfo colonyInfo = this.App.GameDatabase.GetColonyInfo(colonyID);
						if (colonyInfo != null)
						{
							if (PlanetWidget.IsOutputRateSlider(panelName))
							{
								StarSystemDetailsUI.SetOutputRateNew(this.App, colonyInfo.OrbitalObjectID, panelName, msgParams[0]);
								StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfo.ID, this._widgetID, panelName);
							}
							if (array[0].Contains("partOverharvestSlider"))
							{
								colonyInfo.OverharvestRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
								this.App.GameDatabase.UpdateColony(colonyInfo);
								StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfo.ID, this._widgetID, panelName);
								return;
							}
							if (array[0].Contains("partCivSlider"))
							{
								colonyInfo.CivilianWeight = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
								this.App.GameDatabase.UpdateColony(colonyInfo);
								StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfo.ID, this._widgetID, panelName);
								return;
							}
							if (array[0].Contains("partPopSlider"))
							{
								int lockedVar = int.Parse(array[3]);
								Dictionary<int, float> dictionary = new Dictionary<int, float>();
								ColonyFactionInfo[] factions = colonyInfo.Factions;
								for (int i = 0; i < factions.Length; i++)
								{
									ColonyFactionInfo colonyFactionInfo = factions[i];
									dictionary.Add(colonyFactionInfo.FactionID, colonyFactionInfo.CivPopWeight);
								}
								AlgorithmExtensions.DistributePercentages<int>(ref dictionary, lockedVar, StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0])));
								ColonyFactionInfo[] factions2 = colonyInfo.Factions;
								for (int j = 0; j < factions2.Length; j++)
								{
									ColonyFactionInfo colonyFactionInfo2 = factions2[j];
									colonyFactionInfo2.CivPopWeight = dictionary[colonyFactionInfo2.FactionID];
									this.App.GameDatabase.UpdateCivilianPopulation(colonyFactionInfo2);
								}
								this.App.GameDatabase.UpdateColony(colonyInfo);
								StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfo.ID, this._widgetID, panelName);
								return;
							}
						}
					}
				}
			}
			else
			{
				if (msgType == "slider_notched" && panelName.StartsWith("__"))
				{
					string[] array2 = panelName.Split(new char[]
					{
						'|'
					});
					int num2 = int.Parse(array2[1]);
					if (num2 == this._widgetID)
					{
						int colonyID2 = int.Parse(array2[2]);
						ColonyInfo colonyInfo2 = this.App.GameDatabase.GetColonyInfo(colonyID2);
						if (colonyInfo2 != null && panelName.Contains("partTradeSlider"))
						{
							PlanetWidget.UpdateTradeSliderNotchInfo(this.App, colonyInfo2.ID, int.Parse(msgParams[0]));
						}
					}
				}
			}
		}
		public static void UpdateTradeSliderNotchInfo(App App, int ColonyID, int value)
		{
			ColonyInfo colonyInfo = App.GameDatabase.GetColonyInfo(ColonyID);
			if (value == -1)
			{
				UISliderNotchInfo sliderNotchSettingInfoForColony = App.GameDatabase.GetSliderNotchSettingInfoForColony(colonyInfo.PlayerID, colonyInfo.ID, UISlidertype.TradeSlider);
				if (sliderNotchSettingInfoForColony != null)
				{
					App.GameDatabase.DeleteUISliderNotchSettingForColony(colonyInfo.PlayerID, colonyInfo.ID, UISlidertype.TradeSlider);
					return;
				}
			}
			else
			{
				List<double> tradeRatesForWholeExportsForColony = App.Game.GetTradeRatesForWholeExportsForColony(colonyInfo.ID);
				UISliderNotchInfo sliderNotchSettingInfoForColony2 = App.GameDatabase.GetSliderNotchSettingInfoForColony(colonyInfo.PlayerID, colonyInfo.ID, UISlidertype.TradeSlider);
				foreach (double num in tradeRatesForWholeExportsForColony)
				{
					if ((int)Math.Ceiling(num * 100.0) == value)
					{
						if (sliderNotchSettingInfoForColony2 != null)
						{
							sliderNotchSettingInfoForColony2.SliderValue = (double)tradeRatesForWholeExportsForColony.IndexOf(num);
							App.GameDatabase.UpdateUISliderNotchSetting(sliderNotchSettingInfoForColony2);
							break;
						}
						App.GameDatabase.InsertUISliderNotchSetting(App.LocalPlayer.ID, UISlidertype.TradeSlider, (double)tradeRatesForWholeExportsForColony.IndexOf(num), colonyInfo.ID);
						break;
					}
				}
			}
		}
		public void Terminate()
		{
			if (this._cachedPlanet != null)
			{
				this.App.ReleaseObject(this._cachedPlanet);
				this._cachedPlanet = null;
			}
			this._crits.Dispose();
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}
	}
}
