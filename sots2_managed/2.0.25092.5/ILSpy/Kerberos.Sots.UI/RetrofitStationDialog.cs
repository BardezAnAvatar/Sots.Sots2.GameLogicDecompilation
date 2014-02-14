using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class RetrofitStationDialog : Dialog
	{
		private App App;
		private ShipInfo _ship;
		private static string RetrofitCostID = "costvalue";
		private static string RetrofitTimeID = "etavalue";
		private static string RetrofitBankListID = "BankPanel";
		private Dictionary<string, int> BankDict;
		private Dictionary<string, string> ItemIDDict;
		private Dictionary<int, string> ModuleBankdict;
		private string selecteditem;
		private DesignInfo WorkingDesign;
		private WeaponSelector _weaponSelector;
		private WeaponBankInfo _selectedWeaponBank;
		private DesignModuleInfo _selectedModule;
		public RetrofitStationDialog(App app, ShipInfo ship) : base(app, "dialogRetrofitStation")
		{
			this.App = app;
			this._ship = ship;
		}
		public override void Initialize()
		{
			this.App.UI.HideTooltip();
			this.BankDict = new Dictionary<string, int>();
			this.ItemIDDict = new Dictionary<string, string>();
			this.ModuleBankdict = new Dictionary<int, string>();
			DesignInfo designInfo = this._ship.DesignInfo;
			if (designInfo.DesignSections[0].Modules.Any((DesignModuleInfo x) => x.StationModuleType == ModuleEnums.StationModuleType.Combat && !x.WeaponID.HasValue))
			{
				this.UpdateStationDesignInfo(designInfo);
				this._app.GameDatabase.UpdateDesign(designInfo);
			}
			this.WorkingDesign = new DesignInfo(this._ship.DesignInfo.PlayerID, this._ship.DesignInfo.Name, new string[]
			{
				this._ship.DesignInfo.DesignSections[0].FilePath
			});
			this.WorkingDesign.StationLevel = designInfo.StationLevel;
			this.WorkingDesign.StationType = designInfo.StationType;
			DesignLab.SummarizeDesign(this._app.AssetDatabase, this._app.GameDatabase, this.WorkingDesign);
			this.WorkingDesign.DesignSections[0].Modules = new List<DesignModuleInfo>();
			this.WorkingDesign.DesignSections[0].WeaponBanks = new List<WeaponBankInfo>();
			this.WorkingDesign.DesignSections[0].Techs = new List<int>();
			this.WorkingDesign.DesignSections[0].ShipSectionAsset = designInfo.DesignSections[0].ShipSectionAsset;
			int num = 0;
			foreach (DesignModuleInfo current in designInfo.DesignSections[0].Modules)
			{
				DesignModuleInfo item = new DesignModuleInfo
				{
					MountNodeName = current.MountNodeName,
					ModuleID = current.ModuleID,
					WeaponID = current.WeaponID,
					DesignID = current.DesignID,
					StationModuleType = current.StationModuleType,
					ID = num
				};
				num++;
				this.WorkingDesign.DesignSections[0].Modules.Add(item);
			}
			this.UpdateStationDesignInfo(this.WorkingDesign);
			this.App.UI.ClearItems(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitStationDialog.RetrofitBankListID
			}));
			DesignLab.SummarizeDesign(this._app.AssetDatabase, this._app.GameDatabase, this.WorkingDesign);
			DesignSectionInfo[] designSections = this.WorkingDesign.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				foreach (DesignModuleInfo current2 in designSectionInfo.Modules)
				{
					if (current2.WeaponID.HasValue)
					{
						this.App.UI.AddItem(this.App.UI.Path(new string[]
						{
							base.ID,
							RetrofitStationDialog.RetrofitBankListID
						}), "", current2.ID, "");
						string itemGlobalID = this.App.UI.GetItemGlobalID(this.App.UI.Path(new string[]
						{
							base.ID,
							RetrofitStationDialog.RetrofitBankListID
						}), "", current2.ID, "");
						string asset = this.App.GameDatabase.GetWeaponAsset(current2.WeaponID.Value);
						LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.First((LogicalWeapon x) => x.FileName == asset);
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							itemGlobalID,
							"wepimg"
						}), "sprite", logicalWeapon.IconSpriteName);
						string text = "retrofitButton|" + current2.ID.ToString();
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							itemGlobalID,
							"btnImageButton"
						}), "id", text);
						if (!this.BankDict.ContainsKey(text))
						{
							this.BankDict[text] = current2.ID;
						}
						if (!this.ItemIDDict.ContainsKey(text))
						{
							this.ItemIDDict[text] = itemGlobalID;
						}
					}
				}
			}
			this._weaponSelector = new WeaponSelector(this.App.UI, "gameWeaponSelector", "");
			this.App.UI.SetParent(this._weaponSelector.ID, base.UI.Path(new string[]
			{
				base.ID,
				"gameWeaponSelectorbox"
			}));
			this._weaponSelector.SelectedWeaponChanged += new WeaponSelectionChangedEventHandler(this.WeaponSelectorSelectedWeaponChanged);
			StationInfo stationInfo = this.App.GameDatabase.GetStationInfosByPlayerID(this._ship.DesignInfo.PlayerID).FirstOrDefault((StationInfo x) => x.ShipID == this._ship.ID);
			if (stationInfo != null)
			{
				StationUI.SyncStationDetailsWidget(this.App.Game, base.UI.Path(new string[]
				{
					base.ID,
					"stationDetails"
				}), stationInfo.OrbitalObjectID, true);
			}
			this.UpdateUICostETA();
			this.App.UI.SetEnabled(base.UI.Path(new string[]
			{
				base.ID,
				"okButton"
			}), this.DesignChanged());
		}
		private void UpdateListitem(bool isRightClick)
		{
			if (isRightClick)
			{
				using (Dictionary<string, string>.ValueCollection.Enumerator enumerator = this.ItemIDDict.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							current,
							"wepimg"
						}), "sprite", this._weaponSelector.SelectedWeapon.IconSpriteName);
					}
					goto IL_CE;
				}
			}
			this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
			{
				this.selecteditem,
				"wepimg"
			}), "sprite", this._weaponSelector.SelectedWeapon.IconSpriteName);
			IL_CE:
			this.App.UI.SetEnabled(base.UI.Path(new string[]
			{
				base.ID,
				"okButton"
			}), this.DesignChanged());
		}
		private void WeaponSelectorSelectedWeaponChanged(object sender, bool isRightClick)
		{
			int? num = null;
			if (this._weaponSelector.SelectedWeapon != null)
			{
				num = this.App.GameDatabase.GetWeaponID(this._weaponSelector.SelectedWeapon.FileName, this.App.LocalPlayer.ID);
			}
			if (num.HasValue)
			{
				this._selectedModule.WeaponID = new int?(num.Value);
				if (isRightClick)
				{
					foreach (DesignModuleInfo current in this.WorkingDesign.DesignSections[0].Modules)
					{
						string moduleass = this.App.GameDatabase.GetModuleAsset(current.ModuleID);
						LogicalModule logicalModule = this.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == moduleass);
						if (logicalModule.Banks.Count<LogicalBank>() > 0)
						{
							current.WeaponID = new int?(num.Value);
						}
					}
				}
				this.UpdateListitem(isRightClick);
			}
			DesignLab.SummarizeDesign(this._app.AssetDatabase, this._app.GameDatabase, this.WorkingDesign);
			this.UpdateUICostETA();
			this.HideWeaponSelector();
		}
		private void HideWeaponSelector()
		{
			this._weaponSelector.SetVisible(false);
		}
		private void PopulateWeaponSelector(List<LogicalWeapon> weapons, LogicalWeapon selected)
		{
			this.App.UI.MovePanelToMouse(this._weaponSelector.ID, UICommChannel.AnchorPoint.TopLeft, new Vector2(-4f, -4f));
			this._weaponSelector.SetAvailableWeapons(
				from x in weapons
				orderby x.DefaultWeaponSize
				select x, selected);
			this._weaponSelector.SetVisible(true);
		}
		private void UpdateStationDesignInfo(DesignInfo di)
		{
			int num = 0;
			DesignSectionInfo[] designSections = di.DesignSections;
			DesignSectionInfo dsi;
			for (int i = 0; i < designSections.Length; i++)
			{
				dsi = designSections[i];
				if (dsi.WeaponBanks != null)
				{
					dsi.WeaponBanks.Clear();
				}
				ShipSectionAsset section = this.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == dsi.FilePath);
				if (dsi.Modules != null)
				{
					foreach (DesignModuleInfo current in dsi.Modules)
					{
						string moduleass = this.App.GameDatabase.GetModuleAsset(current.ModuleID);
						LogicalModule logicalModule = this.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == moduleass);
						if (logicalModule != null && logicalModule.Banks.Count<LogicalBank>() > 0)
						{
							int num2 = 0;
							ShipSectionAsset shipSectionAsset = this._ship.DesignInfo.DesignSections[0].ShipSectionAsset;
							IEnumerable<LogicalWeapon> enumerable = LogicalWeapon.EnumerateWeaponFits(shipSectionAsset.Faction, shipSectionAsset.SectionName, 
								from weapona in this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this.App.LocalPlayer.ID)
								where weapona.IsVisible
								select weapona, logicalModule.Banks[0].TurretSize, logicalModule.Banks[0].TurretClass);
							enumerable = 
								from x in enumerable
								where x.Range > 1500f || x.DefaultWeaponSize == WeaponEnums.WeaponSizes.VeryLight
								select x;
							int num3;
							int num4;
							LogicalWeapon logicalWeapon = Ship.SelectWeapon(section, logicalModule.Banks[0], null, enumerable, this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this.App.LocalPlayer.ID), current.MountNodeName, out num3, out num4, out num2);
							int? weaponID = null;
							if (logicalWeapon != null && !current.WeaponID.HasValue)
							{
								weaponID = this.App.GameDatabase.GetWeaponID(logicalWeapon.FileName, this.App.LocalPlayer.ID);
							}
							else
							{
								if (current.WeaponID.HasValue)
								{
									weaponID = current.WeaponID;
								}
							}
							this.ModuleBankdict[current.ID] = current.MountNodeName;
							num++;
							current.WeaponID = weaponID;
						}
					}
				}
			}
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
					this.HideWeaponSelector();
				}
				if (panelName == "okButton")
				{
					this.RetrofitShips();
					this._app.UI.CloseDialog(this, true);
					this.HideWeaponSelector();
				}
				if (this.BankDict.ContainsKey(panelName))
				{
					this.SelectBank(this.BankDict[panelName]);
					string asset = this.App.GameDatabase.GetWeaponAsset(this._selectedModule.WeaponID.Value);
					LogicalWeapon selected = this.App.AssetDatabase.Weapons.First((LogicalWeapon x) => x.FileName == asset);
					ShipSectionAsset shipSectionAsset = this._ship.DesignInfo.DesignSections[0].ShipSectionAsset;
					DesignModuleInfo selectedModule = this._selectedModule;
					string moduleass = this.App.GameDatabase.GetModuleAsset(selectedModule.ModuleID);
					LogicalModule logicalModule = this.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == moduleass);
					IEnumerable<LogicalWeapon> source = LogicalWeapon.EnumerateWeaponFits(shipSectionAsset.Faction, shipSectionAsset.SectionName, 
						from weapon in this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this.App.LocalPlayer.ID)
						where weapon.IsVisible
						select weapon, logicalModule.Banks[0].TurretSize, logicalModule.Banks[0].TurretClass);
					source = 
						from x in source
						where x.Range > 1500f || x.DefaultWeaponSize == WeaponEnums.WeaponSizes.VeryLight
						select x;
					this.PopulateWeaponSelector(source.ToList<LogicalWeapon>(), selected);
					this.selecteditem = this.ItemIDDict[panelName];
				}
			}
		}
		private bool RetrofitShips()
		{
			if (!this.DesignChanged())
			{
				return false;
			}
			int designid = this.App.GameDatabase.InsertDesignByDesignInfo(this.WorkingDesign);
			this.App.GameDatabase.InsertStationRetrofitOrder(designid, this._ship.ID);
			return true;
		}
		private void SelectBank(int bankid)
		{
			DesignSectionInfo[] designSections = this.WorkingDesign.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				foreach (WeaponBankInfo current in designSectionInfo.WeaponBanks)
				{
					if (current.ID == bankid)
					{
						this._selectedWeaponBank = current;
					}
				}
				this._selectedModule = designSectionInfo.Modules.First((DesignModuleInfo x) => x.MountNodeName == this.ModuleBankdict[bankid]);
			}
		}
		private bool DesignChanged()
		{
			foreach (DesignModuleInfo dmi in this.WorkingDesign.DesignSections[0].Modules)
			{
				if (this._ship.DesignInfo.DesignSections[0].Modules.Any((DesignModuleInfo x) => x.MountNodeName == dmi.MountNodeName && x.WeaponID != dmi.WeaponID))
				{
					return true;
				}
			}
			return false;
		}
		private void UpdateUICostETA()
		{
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitStationDialog.RetrofitTimeID
			}), "1");
            int num = Kerberos.Sots.StarFleet.StarFleet.CalculateStationRetrofitCost(this.App, this._ship.DesignInfo, this.WorkingDesign);
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				RetrofitStationDialog.RetrofitCostID
			}), num.ToString());
		}
		public override string[] CloseDialog()
		{
			this._weaponSelector.ClearItems();
			StarMapState gameState = this._app.GetGameState<StarMapState>();
			if (gameState != null)
			{
				gameState.RefreshSystemInterface();
			}
			return null;
		}
	}
}
