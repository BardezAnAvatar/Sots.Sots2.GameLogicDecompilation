using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.UI
{
	internal class SuperWorldDialog : Dialog
	{
		private static readonly string UIGemWorld = "btnGem";
		private static readonly string UIForgeWorld = "btnForge";
		private static readonly string UIGemDesc = "lblGemDesc";
		private static readonly string UIForgeDesc = "lblForgeDesc";
		private static readonly string UILocation = "lblLocation";
		private int colonyId;
		public SuperWorldDialog(App game, int colonyId) : base(game, "dialogSuperWorld")
		{
			this.colonyId = colonyId;
		}
		public override void Initialize()
		{
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				SuperWorldDialog.UIGemDesc
			}), string.Format(App.Localize("@UI_DIALOGSUPERWORLD_GEM_DESC"), this._app.AssetDatabase.GemWorldCivMaxBonus));
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				SuperWorldDialog.UIForgeDesc
			}), string.Format(App.Localize("@UI_DIALOGSUPERWORLD_FORGE_DESC"), this._app.AssetDatabase.ForgeWorldImpMaxBonus, this._app.AssetDatabase.ForgeWorldIOBonus));
			ColonyInfo colonyInfo = this._app.GameDatabase.GetColonyInfo(this.colonyId);
			OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				SuperWorldDialog.UILocation
			}), string.Format(App.Localize("@UI_DIALOGSUPERWORLD_LOCATION"), orbitalObjectInfo.Name, starSystemInfo.Name));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == SuperWorldDialog.UIGemWorld)
				{
					ColonyInfo colonyInfo = this._app.GameDatabase.GetColonyInfo(this.colonyId);
					colonyInfo.CurrentStage = ColonyStage.GemWorld;
					colonyInfo.CivilianWeight /= 2f;
					this._app.GameDatabase.UpdateColony(colonyInfo);
					OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
					StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
					GameSession.ApplyMoralEvent(this._app, MoralEvent.ME_GEM_WORLD_FORMED, colonyInfo.PlayerID, new int?(colonyInfo.ID), starSystemInfo.ProvinceID, new int?(starSystemInfo.ID));
					this._app.GameDatabase.InsertGovernmentAction(colonyInfo.PlayerID, App.Localize("@GA_GEMWORLD"), "GemWorld", 0, 0);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == SuperWorldDialog.UIForgeWorld)
				{
					ColonyInfo colonyInfo2 = this._app.GameDatabase.GetColonyInfo(this.colonyId);
					PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(colonyInfo2.OrbitalObjectID);
					colonyInfo2.CurrentStage = ColonyStage.ForgeWorld;
					colonyInfo2.CivilianWeight = 1f;
					planetInfo.Biosphere = 0;
					this._app.GameDatabase.UpdatePlanet(planetInfo);
					this._app.GameDatabase.UpdateColony(colonyInfo2);
					OrbitalObjectInfo orbitalObjectInfo2 = this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfo2.OrbitalObjectID);
					StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo2.StarSystemID);
					GameSession.ApplyMoralEvent(this._app, MoralEvent.ME_FORGE_WORLD_FORMED, colonyInfo2.PlayerID, new int?(colonyInfo2.ID), starSystemInfo2.ProvinceID, new int?(starSystemInfo2.ID));
					this._app.GameDatabase.InsertGovernmentAction(colonyInfo2.PlayerID, App.Localize("@GA_FORGEWORLD"), "ForgeWorld", 0, 0);
					this._app.UI.CloseDialog(this, true);
				}
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
