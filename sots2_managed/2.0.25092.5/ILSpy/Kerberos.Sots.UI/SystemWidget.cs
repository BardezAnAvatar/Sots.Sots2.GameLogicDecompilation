using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.UI
{
	internal class SystemWidget
	{
		private string _rootPanel = "";
		private int _systemID;
		public App App;
		private StarModel _cachedStar;
		private StarSystemInfo _cachedStarInfo;
		private bool _cachedStarReady;
		private PlanetView _planetView;
		private GameObjectSet _crits;
		private bool _initialized;
		private bool _starViewLinked;
		private static int kWidgetID;
		private int _widgetID;
		public SystemWidget(App app, string rootPanel)
		{
			this._rootPanel = rootPanel;
			this.App = app;
			this._crits = new GameObjectSet(this.App);
			this._planetView = this._crits.Add<PlanetView>(new object[0]);
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			SystemWidget.kWidgetID++;
			this._widgetID = SystemWidget.kWidgetID;
		}
		public string GetRootPanel()
		{
			return this._rootPanel;
		}
		public void Sync(int systemID)
		{
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo == null)
			{
				return;
			}
			this._systemID = systemID;
			this.CacheStar(starSystemInfo);
			StarSystemUI.SyncStarDetailsControl(this.App.Game, this._rootPanel, systemID);
			StarSystemUI.SyncStarDetailsStations(this.App.Game, this._rootPanel, systemID, this.App.LocalPlayer.ID);
			StellarClass stellarClass = new StellarClass(starSystemInfo.StellarClass);
			Vector4 vector = StarHelper.CalcModelColor(stellarClass);
			this.App.UI.SetPropertyColorNormalized(this.App.UI.Path(new string[]
			{
				this._rootPanel,
				"colorGradient"
			}), "color", vector.X, vector.Y, vector.Z, 0.5f);
			this._initialized = false;
		}
		private void CacheStar(StarSystemInfo systemInfo)
		{
			if (this._cachedStar != null)
			{
				if (systemInfo == this._cachedStarInfo)
				{
					return;
				}
				this.App.ReleaseObject(this._cachedStar);
				this._cachedStar = null;
			}
			this._cachedStarInfo = systemInfo;
			this._cachedStarReady = false;
			this._cachedStar = Kerberos.Sots.GameStates.StarSystem.CreateStar(this.App, Vector3.Zero, systemInfo, 1f, false);
			this._cachedStar.PostSetProp("AutoDraw", false);
		}
		public void Update()
		{
			if (this._crits == null || !this._crits.IsReady())
			{
				return;
			}
			if (this._cachedStar != null && !this._cachedStarReady && this._cachedStar.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedStarReady = true;
				this._cachedStar.Active = true;
			}
			if (this._cachedStarReady && !this._initialized)
			{
				this._planetView.PostSetProp("Planet", (this._cachedStar != null) ? this._cachedStar.ObjectID : 0);
				if (!this._starViewLinked)
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
					this._starViewLinked = true;
				}
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					this._rootPanel,
					"loadingCircle"
				}), false);
				this._initialized = true;
			}
		}
		protected void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
		}
		public void Terminate()
		{
			if (this._cachedStar != null)
			{
				this._cachedStar.Dispose();
			}
			this._crits.Dispose();
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}
	}
}
