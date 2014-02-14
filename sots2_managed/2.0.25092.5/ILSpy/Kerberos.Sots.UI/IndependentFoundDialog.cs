using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class IndependentFoundDialog : Dialog
	{
		public const string IgnoreButton = "ignoreButton";
		public const string AttackButton = "attackButton";
		public const string DiploButton = "diploButton";
		private int _systemID;
		private int _colonyID;
		private int _playerID;
		private int _planetID;
		private List<PlanetWidget> _planetWidgets;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		public IndependentFoundDialog(App game, int systemid, int colonyid, int playerid) : base(game, "dialogIndependentFoundEvent")
		{
			this._systemID = systemid;
			this._colonyID = colonyid;
			this._playerID = playerid;
		}
		public override void Initialize()
		{
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._systemID);
			ColonyInfo colonyInfo = this._app.GameDatabase.GetColonyInfo(this._colonyID);
			PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID);
			this._playerID = playerInfo.ID;
			this._planetID = colonyInfo.OrbitalObjectID;
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"title"
			}), "text", App.Localize("@UI_INDEPENDENT_CIVILIZATION_FOUND_TITLE").ToUpperInvariant());
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"faction_name"
			}), "text", playerInfo.Name);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"tech_level"
			}), "text", "");
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"avatar"
			}), "texture", playerInfo.AvatarAssetPath);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"tech_level"
			}), "text", App.Localize("@UI_INDEPENDENT_TECH_" + playerInfo.Name.ToUpper()));
			StarSystemMapUI.Sync(this._app, this._systemID, this._app.UI.Path(new string[]
			{
				base.ID,
				"system_map"
			}), true);
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"diploButton"
			}), Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this._app.Game, this._app.LocalPlayer.ID, this._systemID, MissionType.CONSTRUCT_STN, true).Any<FleetInfo>());
			this._planetWidgets = new List<PlanetWidget>();
			this.SetSyncedSystem(starSystemInfo, planetInfo);
		}
		protected void SetSyncedSystem(StarSystemInfo system, PlanetInfo planet)
		{
			this._app.UI.ClearItems("system_list");
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Terminate();
			}
			foreach (PlanetWidget current2 in this._planetWidgets)
			{
				current2.Terminate();
			}
			this._planetWidgets.Clear();
			this._app.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			string itemGlobalID = this._app.UI.GetItemGlobalID("system_list", "", system.ID, "");
			this._systemWidgets.Add(new SystemWidget(this._app, itemGlobalID));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			this._app.UI.AddItem("system_list", "", planet.ID + 999999, "", "planetDetailsM_INDY_Card");
			string itemGlobalID2 = this._app.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "");
			this._planetWidgets.Add(new PlanetWidget(this._app, itemGlobalID2));
			this._planetWidgets.Last<PlanetWidget>().Sync(planet.ID, false, false);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "ignoreButton")
				{
					this._app.UI.CloseDialog(this, true);
				}
				if (panelName == "attackButton")
				{
					this._app.Game.DeclareWarFormally(this._app.Game.LocalPlayer.ID, this._playerID);
					this._app.UI.CloseDialog(this, true);
				}
				if (panelName == "diploButton")
				{
					StarMapState starMapState = (StarMapState)this._app.CurrentState;
					SpecialConstructionMission specialConstructionMission = new SpecialConstructionMission();
					specialConstructionMission._project = SpecialProjectType.IndependentStudy;
					specialConstructionMission._forcedStationType = StationType.SCIENCE;
					specialConstructionMission._targetplayerid = this._playerID;
					specialConstructionMission._targetsystemid = this._systemID;
					specialConstructionMission._targetplanet = this._planetID;
					OverlayConstructionMission overlayConstructionMission = new OverlayConstructionMission(this._app, starMapState, starMapState.StarMap, specialConstructionMission, "OverlayStationMission", MissionType.CONSTRUCT_STN);
					overlayConstructionMission.Show(this._systemID);
					this._app.UI.CloseDialog(this, true);
				}
			}
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
