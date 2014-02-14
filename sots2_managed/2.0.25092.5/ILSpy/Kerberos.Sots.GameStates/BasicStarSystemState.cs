using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal abstract class BasicStarSystemState : GameState
	{
		protected const string UISystemMap = "partMiniSystem";
		protected const string UIEmpireBar = "gameEmpireBar";
		protected const string UIContentsList = "gameSystemContentsList";
		protected const string UIBackButton = "gameExitButton";
		protected const string UISystemDetails = "SystemView";
		protected const string UIColonyDetailsWidget = "colonyDetailsWidget";
		protected const string UISystemDetailsWidget = "systemDetailsWidget";
		protected const string UIPlanetDetailsWidget = "planetDetailsWidget";
		protected const string UIPlanetListWidget = "planetListWidget";
		private readonly Random _random = new Random();
		private readonly List<IGameObject> _objects = new List<IGameObject>();
		protected StarSystem _starsystem;
		private Sky _sky;
		private OrbitCameraController _camera;
		private GameObjectSet _crits;
		private int _currentSystem;
		protected PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private bool _cachedPlanetReady;
		private StarModel _cachedStar;
		private bool _cachedStarReady;
		protected OrbitCameraController Camera
		{
			get
			{
				return this._camera;
			}
		}
		public int SelectedObject
		{
			get;
			set;
		}
		public StarSystem StarSystem
		{
			get
			{
				return this._starsystem;
			}
		}
		public int CurrentSystem
		{
			get
			{
				return this._currentSystem;
			}
			private set
			{
				this._currentSystem = value;
			}
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame(new Random(12345));
				if (stateParams == null || stateParams.Length == 0)
				{
					int value = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).Homeworld.Value;
					int starSystemID = base.App.GameDatabase.GetOrbitalObjectInfo(value).StarSystemID;
					stateParams = new object[]
					{
						starSystemID,
						value
					};
				}
			}
			int num = (int)stateParams[0];
			int selectedObject = (int)stateParams[1];
			this._crits = new GameObjectSet(base.App);
			this._sky = new Sky(base.App, SkyUsage.InSystem, num);
			this._crits.Add(this._sky);
			this._camera = this._crits.Add<OrbitCameraController>(new object[0]);
			this._planetView = this._crits.Add<PlanetView>(new object[0]);
			this._starsystem = new StarSystem(base.App, 1f, num, Vector3.Zero, true, null, false, 0, this is DefenseManagerState, true);
			this._crits.Add(this._starsystem);
			this._starsystem.PostSetProp("CameraController", this._camera);
			this._starsystem.PostSetProp("InputEnabled", true);
			this.CurrentSystem = num;
			this.SelectedObject = selectedObject;
		}
		protected override void OnEnter()
		{
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			float num = 0f;
			IEnumerable<OrbitalObjectInfo> starSystemOrbitalObjectInfos = base.App.GameDatabase.GetStarSystemOrbitalObjectInfos(this.CurrentSystem);
			foreach (OrbitalObjectInfo current in starSystemOrbitalObjectInfos)
			{
				float length = current.OrbitalPath.Scale.Length;
				if (num < length)
				{
					num = length;
				}
			}
			this._camera.MaxDistance = num * 4f;
			this._camera.DesiredDistance = num * 0.2f;
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-25f);
			this._camera.SnapToDesiredPosition();
			this._camera.DesiredDistance = num * 1.6f;
			this._crits.Activate();
			if (this.SelectedObject == 0)
			{
				this.SelectedObject = this._starsystem.ObjectMap.Forward.Values.First<int>();
			}
			if (this.SelectedObject != 0)
			{
				int selectedObject = this.SelectedObject;
				this.SelectedObject = 0;
				this.SetSelectedObject(selectedObject, string.Empty);
				this.Focus(this.SelectedObject);
			}
		}
		protected IGameObject GetPlanetViewGameObject(int systemId, int orbitId)
		{
			IGameObject result = null;
			if (systemId != 0)
			{
				if (orbitId > 0)
				{
					PlanetInfo planetInfo = base.App.GameDatabase.GetPlanetInfo(orbitId);
					if (planetInfo != null)
					{
						this.CachePlanet(planetInfo);
						result = this._cachedPlanet;
					}
				}
				else
				{
					this.CacheStar(base.App.GameDatabase.GetStarSystemInfo(systemId));
					result = this._cachedStar;
				}
			}
			return result;
		}
		private void CacheStar(StarSystemInfo systemInfo)
		{
			if (this._cachedStar != null)
			{
				base.App.ReleaseObject(this._cachedStar);
				this._cachedStar = null;
			}
			this._cachedStarReady = false;
			this._cachedStar = StarSystem.CreateStar(base.App, Vector3.Zero, systemInfo, 1f, false);
			this._cachedStar.PostSetProp("AutoDraw", false);
		}
		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (planetInfo == null)
			{
				return;
			}
			if (this._cachedPlanet != null)
			{
				base.App.ReleaseObject(this._cachedPlanet);
				this._cachedPlanet = null;
			}
			this._cachedPlanetReady = false;
			this._cachedPlanet = StarSystem.CreatePlanet(base.App.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, StarSystem.TerrestrialPlanetQuality.High);
			this._cachedPlanet.PostSetProp("AutoDraw", false);
			this._crits.Add(this._cachedPlanet);
		}
		private void UpdateCachedPlanet()
		{
			if (this._cachedPlanet != null && !this._cachedPlanetReady && this._cachedPlanet.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedPlanetReady = true;
				this._cachedPlanet.Active = true;
			}
		}
		private void UpdateCachedStar()
		{
			if (this._cachedStar != null && !this._cachedStarReady && this._cachedStar.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedStarReady = true;
				this._cachedStar.Active = true;
			}
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "ObjectClicked")
			{
				this.ProcessGameEvent_ObjectClicked(eventParams);
			}
			this.OnUIGameEvent(eventName, eventParams);
		}
		protected abstract void OnUIGameEvent(string eventName, string[] eventParams);
		private void ProcessGameEvent_ObjectClicked(string[] eventParams)
		{
			int id = int.Parse(eventParams[0]);
			IGameObject gameObject = base.App.GetGameObject(id);
			if (gameObject != null)
			{
				int orbitalId = 0;
				this._starsystem.ObjectMap.Forward.TryGetValue(gameObject, out orbitalId);
				this.SetSelectedObject(orbitalId, string.Empty);
			}
		}
		protected virtual void OnBack()
		{
			if (base.App.PreviousState is EmpireSummaryState || base.App.PreviousState is DesignScreenState)
			{
				base.App.SwitchGameState<StarMapState>(new object[0]);
				return;
			}
			base.App.SwitchGameState(base.App.PreviousState, new object[0]);
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "mapicon_clicked")
			{
				if (panelName == "partMiniSystem")
				{
					int orbitalId = int.Parse(msgParams[0]);
					this.SetSelectedObject(orbitalId, "partMiniSystem");
					this.Focus(orbitalId);
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == "gameExitButton")
					{
						this.OnBack();
					}
				}
				else
				{
					if (msgType == "list_sel_changed")
					{
						if (panelName == "gamePlanetList")
						{
							int num = 0;
							if (msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
							{
								num = int.Parse(msgParams[0]);
							}
							if (num != this.SelectedObject)
							{
								this.SetSelectedObject(num, "gameSystemContentsList");
								this.Focus(num);
							}
						}
					}
					else
					{
						if (msgType == "OutputRatesChanged")
						{
							throw new NotImplementedException();
						}
					}
				}
			}
			this.OnPanelMessage(panelName, msgType, msgParams);
		}
		protected abstract void OnPanelMessage(string panelName, string msgType, string[] msgParams);
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._starsystem = null;
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
			}
		}
		protected override void OnUpdate()
		{
			this.UpdateCachedPlanet();
			this.UpdateCachedStar();
		}
		public override bool IsReady()
		{
			return this._crits.IsReady() && base.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		public BasicStarSystemState(App game) : base(game)
		{
		}
		private void Focus(int orbitalId)
		{
			if (orbitalId != 0)
			{
				this._camera.TargetID = this._starsystem.ObjectMap.Reverse[orbitalId].ObjectID;
			}
		}
		protected void SetSelectedObject(int orbitalId, string trigger)
		{
			this.SelectedObject = orbitalId;
			if (trigger != "gameSystemContentsList")
			{
				if (this.SelectedObject == 0)
				{
					base.App.UI.ClearSelection("gameSystemContentsList");
				}
				else
				{
					if (this.SelectedObject == StarSystemDetailsUI.StarItemID)
					{
						base.App.UI.SetSelection("gameSystemContentsList", this.SelectedObject);
					}
					else
					{
						OrbitalObjectInfo orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(this.SelectedObject);
						if (!orbitalObjectInfo.ParentID.HasValue)
						{
							base.App.UI.SetSelection("gameSystemContentsList", this.SelectedObject);
						}
						else
						{
							IGameObject gameObject;
							this._starsystem.PlanetMap.Reverse.TryGetValue(orbitalObjectInfo.ParentID.Value, out gameObject);
							StarSystemUI.SyncPlanetDetailsWidget(base.App.Game, "planetDetailsWidget", this._currentSystem, orbitalId, gameObject, this._planetView);
							if (gameObject != null)
							{
								base.App.UI.SetSelection("gameSystemContentsList", orbitalObjectInfo.ParentID.Value);
							}
							else
							{
								base.App.UI.ClearSelection("gameSystemContentsList");
							}
						}
					}
				}
				this.Focus(this.SelectedObject);
			}
			StarSystemUI.SyncPlanetDetailsWidget(base.App.Game, "planetDetailsWidget", this.CurrentSystem, this.SelectedObject, this.GetPlanetViewGameObject(this.CurrentSystem, this.SelectedObject), this._planetView);
			StarSystemUI.SyncColonyDetailsWidget(base.App.Game, "colonyDetailsWidget", this.SelectedObject, "");
		}
	}
}
