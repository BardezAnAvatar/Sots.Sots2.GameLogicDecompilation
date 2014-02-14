using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class RetrofitShipDialog : Dialog
	{
		private App App;
		private ShipInfo _ship;
		private bool Allshiptog;
		private static string currentnameID = "current_shipname";
		private static string retrofitnameID = "retrofit_shipname";
		private static string RetrofitCostID = "costvalue";
		private static string RetrofitTimeID = "etavalue";
		private WeaponHoverPanel _weaponTooltip;
		private ModuleHoverPanel _moduleTooltip;
		private WeaponHoverPanel _oldweaponTooltip;
		private ModuleHoverPanel _oldmoduleTooltip;
		public RetrofitShipDialog(App app, ShipInfo ship) : base(app, "dialogRetrofitShip")
		{
			this.App = app;
			this._ship = ship;
		}
		public override void Initialize()
		{
			this.App.UI.HideTooltip();
			this.Allshiptog = false;
            DesignInfo designInfo = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(this._ship.DesignInfo, this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitShipDialog.currentnameID
			}), this._ship.DesignInfo.Name);
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitShipDialog.retrofitnameID
			}), designInfo.Name);
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitShipDialog.RetrofitCostID
			}), Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this.App, this._ship.DesignInfo, designInfo).ToString());
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitShipDialog.RetrofitTimeID
			}), Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(this.App, this._ship, 1).ToString());
			this.App.GameDatabase.GetFleetInfo(this._ship.FleetID);
			List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(this._ship.FleetID, false).ToList<ShipInfo>();
			List<int> list2 = new List<int>();
			foreach (ShipInfo current in list)
			{
				if (current.DesignID == this._ship.DesignID)
				{
					list2.Add(current.ID);
				}
			}
            if (Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShips(this.App, this._ship.FleetID, list2))
			{
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					base.ID,
					"allships"
				}), true);
			}
			else
			{
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					base.ID,
					"allships"
				}), false);
			}
			if (this._weaponTooltip == null)
			{
				this._weaponTooltip = new WeaponHoverPanel(this.App.UI, this.App.UI.Path(new string[]
				{
					base.ID,
					"WeaponPanel"
				}), "weaponInfo");
			}
			if (this._moduleTooltip == null)
			{
				this._moduleTooltip = new ModuleHoverPanel(this.App.UI, this.App.UI.Path(new string[]
				{
					base.ID,
					"WeaponPanel"
				}), "moduleInfo");
			}
			List<LogicalWeapon> list3 = new List<LogicalWeapon>();
			DesignSectionInfo[] designSections = designInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				IEnumerable<WeaponBankInfo> weaponBanks = designSectionInfo.WeaponBanks;
				foreach (WeaponBankInfo current2 in weaponBanks)
				{
					if (current2.WeaponID.HasValue)
					{
						string weaponPath = this.App.GameDatabase.GetWeaponAsset(current2.WeaponID.Value);
						LogicalWeapon weapon = this.App.AssetDatabase.Weapons.First((LogicalWeapon x) => x.FileName == weaponPath);
						if (weapon != null && (
							from x in list3
							where x.FileName == weapon.FileName
							select x).Count<LogicalWeapon>() == 0)
						{
							list3.Add(weapon);
						}
					}
				}
			}
			this._weaponTooltip.SetAvailableWeapons(list3, true);
			List<LogicalModule> list4 = new List<LogicalModule>();
			DesignSectionInfo[] designSections2 = designInfo.DesignSections;
			for (int j = 0; j < designSections2.Length; j++)
			{
				DesignSectionInfo designSectionInfo2 = designSections2[j];
				IEnumerable<DesignModuleInfo> modules = designSectionInfo2.Modules;
				foreach (DesignModuleInfo current3 in modules)
				{
					string modulePath = this.App.GameDatabase.GetModuleAsset(current3.ModuleID);
					LogicalModule module = this.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == modulePath);
					if (module != null && (
						from x in list4
						where x.ModulePath == module.ModulePath
						select x).Count<LogicalModule>() == 0)
					{
						list4.Add(module);
					}
				}
			}
			this._moduleTooltip.SetAvailableModules(list4, null, false);
			designInfo = this._ship.DesignInfo;
			if (this._oldweaponTooltip == null)
			{
				this._oldweaponTooltip = new WeaponHoverPanel(this.App.UI, this.App.UI.Path(new string[]
				{
					base.ID,
					"OldWeaponPanel"
				}), "oldweaponInfo");
			}
			if (this._oldmoduleTooltip == null)
			{
				this._oldmoduleTooltip = new ModuleHoverPanel(this.App.UI, this.App.UI.Path(new string[]
				{
					base.ID,
					"OldWeaponPanel"
				}), "oldmoduleInfo");
			}
			List<LogicalWeapon> list5 = new List<LogicalWeapon>();
			DesignSectionInfo[] designSections3 = designInfo.DesignSections;
			for (int k = 0; k < designSections3.Length; k++)
			{
				DesignSectionInfo designSectionInfo3 = designSections3[k];
				IEnumerable<WeaponBankInfo> weaponBanks2 = designSectionInfo3.WeaponBanks;
				foreach (WeaponBankInfo current4 in weaponBanks2)
				{
					if (current4.WeaponID.HasValue)
					{
						string weaponPath = this.App.GameDatabase.GetWeaponAsset(current4.WeaponID.Value);
						LogicalWeapon weapon = this.App.AssetDatabase.Weapons.First((LogicalWeapon x) => x.FileName == weaponPath);
						if (weapon != null && (
							from x in list5
							where x.FileName == weapon.FileName
							select x).Count<LogicalWeapon>() == 0)
						{
							list5.Add(weapon);
						}
					}
				}
			}
			this._oldweaponTooltip.SetAvailableWeapons(list5, true);
			List<LogicalModule> list6 = new List<LogicalModule>();
			DesignSectionInfo[] designSections4 = designInfo.DesignSections;
			for (int l = 0; l < designSections4.Length; l++)
			{
				DesignSectionInfo designSectionInfo4 = designSections4[l];
				IEnumerable<DesignModuleInfo> modules2 = designSectionInfo4.Modules;
				foreach (DesignModuleInfo current5 in modules2)
				{
					string modulePath = this.App.GameDatabase.GetModuleAsset(current5.ModuleID);
					LogicalModule module = this.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == modulePath);
					if (module != null && (
						from x in list6
						where x.ModulePath == module.ModulePath
						select x).Count<LogicalModule>() == 0)
					{
						list6.Add(module);
					}
				}
			}
			this._oldmoduleTooltip.SetAvailableModules(list6, null, false);
		}
		protected override void OnUpdate()
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "cancelButton")
				{
					this._app.UI.CloseDialog(this, true);
				}
				if (panelName == "okButton")
				{
					this.RetrofitShips();
					this._app.UI.CloseDialog(this, true);
				}
				if (panelName == "allships")
				{
					this.Allshiptog = !this.Allshiptog;
					this.UpdateUICostETA();
				}
			}
		}
		private bool RetrofitShips()
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._ship.FleetID);
			if (fleetInfo == null)
			{
				return false;
			}
			if (this.Allshiptog)
			{
				List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(this._ship.FleetID, false).ToList<ShipInfo>();
				List<int> list2 = new List<int>();
				foreach (ShipInfo current in list)
				{
					if (current.DesignID == this._ship.DesignID)
					{
						list2.Add(current.ID);
					}
				}
                if (Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShips(this.App, this._ship.FleetID, list2))
				{
					this._app.GameDatabase.RetrofitShips(list2, fleetInfo.SystemID, this.App.LocalPlayer.ID);
					return true;
				}
				return false;
			}
			else
			{
                if (Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShip(this.App, this._ship.FleetID, this._ship.ID))
				{
					this._app.GameDatabase.RetrofitShip(this._ship.ID, fleetInfo.SystemID, this.App.LocalPlayer.ID, null);
					return true;
				}
				return false;
			}
		}
		private void UpdateUICostETA()
		{
			if (this.Allshiptog)
			{
				this.App.GameDatabase.GetFleetInfo(this._ship.FleetID);
				List<ShipInfo> source = this.App.GameDatabase.GetShipInfoByFleetID(this._ship.FleetID, false).ToList<ShipInfo>();
				int num = (
					from x in source
					where x.DesignID == this._ship.DesignID
					select x).Count<ShipInfo>();
				this.App.UI.SetText(this.App.UI.Path(new string[]
				{
					base.ID,
					RetrofitShipDialog.RetrofitTimeID
				}), Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(this.App, this._ship, num).ToString());
                DesignInfo newestRetrofitDesign = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(this._ship.DesignInfo, this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID));
				this.App.UI.SetText(this.App.UI.Path(new string[]
				{
					base.ID,
					RetrofitShipDialog.RetrofitCostID
				}), (Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this.App, this._ship.DesignInfo, newestRetrofitDesign) * num).ToString());
				return;
			}
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitShipDialog.RetrofitTimeID
			}), Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(this.App, this._ship, 1).ToString());
            DesignInfo newestRetrofitDesign2 = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(this._ship.DesignInfo, this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitShipDialog.RetrofitCostID
			}), Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this.App, this._ship.DesignInfo, newestRetrofitDesign2).ToString());
		}
		public override string[] CloseDialog()
		{
			this.Allshiptog = false;
			StarMapState gameState = this._app.GetGameState<StarMapState>();
			if (gameState != null)
			{
				gameState.RefreshSystemInterface();
			}
			return null;
		}
	}
}
