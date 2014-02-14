using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class StationBuiltDialogAlt : Dialog
	{
		public const string OKButton = "event_dialog_close";
		private int _stationID;
		private OrbitCameraController _cameraReduced;
		private StarMap _starmapReduced;
		private Sky _sky;
		private GameObjectSet _crits;
		private PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private StarSystemDummyOccupant _stationModel;
		private string _enteredStationName;
		private Vector3 _trans;
		public StationBuiltDialogAlt(App game, int stationid, Vector3 trans) : base(game, "dialogStationBuiltAlt")
		{
			this._stationID = stationid;
			this._trans = trans;
		}
		public override void Initialize()
		{
			this._sky = new Sky(this._app, SkyUsage.StarMap, 0);
			this._crits = new GameObjectSet(this._app);
			this._planetView = this._crits.Add<PlanetView>(new object[0]);
			this._crits.Add(this._sky);
			StationInfo stationInfo = this._app.GameDatabase.GetStationInfo(this._stationID);
			OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._stationID);
			OrbitalObjectInfo orbitalObjectInfo2 = this._app.GameDatabase.GetOrbitalObjectInfo(orbitalObjectInfo.ParentID.Value);
			string text = string.Format(App.Localize("@STATION_LEVEL"), stationInfo.DesignInfo.StationLevel.ToString(), stationInfo.DesignInfo.StationType.ToString());
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"station_class"
			}), text);
			int num = GameSession.CalculateStationUpkeepCost(this._app.GameDatabase, this._app.AssetDatabase, stationInfo);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"upkeep_cost"
			}), string.Format(App.Localize("@STATION_UPKEEP_COST"), num.ToString()));
			if (stationInfo.DesignInfo.StationType == StationType.NAVAL)
			{
				int systemSupportedCruiserEquivalent = this._app.GameDatabase.GetSystemSupportedCruiserEquivalent(this._app.Game, orbitalObjectInfo2.StarSystemID, this._app.LocalPlayer.ID);
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"naval_capacity"
				}), string.Format(App.Localize("@STATION_FLEET_CAPACITY"), systemSupportedCruiserEquivalent.ToString()));
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"naval_capacity"
				}), false);
			}
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo2.StarSystemID);
			this._app.UI.SetText("gameStationName", orbitalObjectInfo.Name);
			this._enteredStationName = orbitalObjectInfo2.Name;
			string text2 = string.Format(App.Localize("@STATION_BUILT"), starSystemInfo.Name).ToUpperInvariant();
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"system_name"
			}), text2);
			this._cameraReduced = new OrbitCameraController(this._app);
			this._cameraReduced.MinDistance = 1002.5f;
			this._cameraReduced.MaxDistance = 10000f;
			this._cameraReduced.DesiredDistance = 2000f;
			this._cameraReduced.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this._cameraReduced.DesiredPitch = -MathHelper.DegreesToRadians(25f);
			this._cameraReduced.SnapToDesiredPosition();
			this._starmapReduced = new StarMap(this._app, this._app.Game, this._sky);
			this._starmapReduced.Initialize(this._crits, new object[0]);
			this._starmapReduced.SetCamera(this._cameraReduced);
			this._starmapReduced.FocusEnabled = false;
			int objectID = this._starmapReduced.Systems.Reverse[starSystemInfo.ID].ObjectID;
			this._starmapReduced.PostSetProp("Selected", objectID);
			StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(starSystemInfo.ID);
			this._starmapReduced.PostSetProp("CullCenter", starSystemInfo2.Origin);
			this._starmapReduced.PostSetProp("CullRadius", 15f);
			DesignInfo di = DesignLab.CreateStationDesignInfo(this._app.AssetDatabase, this._app.GameDatabase, this._app.LocalPlayer.ID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel, false);
			ShipSectionAsset shipSectionAsset = this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == di.DesignSections[0].FilePath);
			this._stationModel = new StarSystemDummyOccupant(this._app, shipSectionAsset.ModelName, stationInfo.DesignInfo.StationType);
			this._stationModel.PostSetScale(0.002f);
			this._stationModel.PostSetPosition(this._trans);
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
			if (this._stationModel != null && !this._stationModel.Active && this._stationModel.ObjectStatus != GameObjectStatus.Pending)
			{
				this._stationModel.Active = true;
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "event_dialog_close")
				{
					OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._stationID);
					if (orbitalObjectInfo != null)
					{
						orbitalObjectInfo.Name = this._enteredStationName;
						this._app.GameDatabase.UpdateOrbitalObjectInfo(orbitalObjectInfo);
					}
					if (!string.IsNullOrWhiteSpace(this._enteredStationName) && this._enteredStationName.Count<char>() > 0)
					{
						this._app.UI.CloseDialog(this, true);
						return;
					}
					this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@INVALID_STATION_NAME"), App.Localize("@INVALID_STATION_NAME_TEXT"), "dialogGenericMessage"), null);
					return;
				}
			}
			else
			{
				if (msgType == "text_changed" && panelName == "gameStationName")
				{
					this._enteredStationName = msgParams[0];
				}
			}
		}
		public override string[] CloseDialog()
		{
			if (this._cachedPlanet != null)
			{
				this._app.ReleaseObject(this._cachedPlanet);
			}
			this._cachedPlanet = null;
			this._crits.Dispose();
			this._starmapReduced.Dispose();
			this._cameraReduced.Dispose();
			this._stationModel.Dispose();
			return null;
		}
	}
}
