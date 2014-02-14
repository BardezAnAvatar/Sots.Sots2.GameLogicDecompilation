using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class StationBuiltDialog : Dialog
	{
		public const string OKButton = "event_dialog_close";
		private int _stationID;
		private string _enteredStationName;
		public StationBuiltDialog(App game, int stationid) : base(game, "dialogStationBuilt")
		{
			this._stationID = stationid;
		}
		public override void Initialize()
		{
			StationInfo stationInfo = this._app.GameDatabase.GetStationInfo(this._stationID);
			OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._stationID);
			OrbitalObjectInfo orbitalObjectInfo2 = this._app.GameDatabase.GetOrbitalObjectInfo(orbitalObjectInfo.ParentID.Value);
			if (stationInfo == null || orbitalObjectInfo == null)
			{
				this._app.UI.CloseDialog(this, true);
				return;
			}
			string text = string.Format(App.Localize("@STATION_LEVEL"), stationInfo.DesignInfo.StationLevel.ToString(), stationInfo.DesignInfo.StationType.ToDisplayText(this._app.LocalPlayer.Faction.Name));
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
			StationUI.SyncStationDetailsWidget(this._app.Game, "detailsCard", this._stationID, true);
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo2.StarSystemID);
			this._app.UI.SetText("gameStationName", orbitalObjectInfo.Name);
			this._enteredStationName = orbitalObjectInfo2.Name;
			string text2 = string.Format(App.Localize("@STATION_BUILT"), starSystemInfo.Name).ToUpperInvariant();
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"system_name"
			}), text2);
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
			return null;
		}
	}
}
