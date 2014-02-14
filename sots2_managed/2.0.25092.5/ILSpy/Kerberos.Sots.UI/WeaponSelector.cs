using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class WeaponSelector : WeaponTilePanel
	{
		public WeaponSelector(UICommChannel ui, string id, string weaponInfoPanel = "") : base(ui, id, weaponInfoPanel)
		{
		}
		private void DisposeItems()
		{
			foreach (WeaponSelectorItem current in this._items.Values)
			{
				current.Dispose();
			}
		}
		public void SetAvailableWeapons(IEnumerable<LogicalWeapon> weapons, LogicalWeapon selected)
		{
			base.SetAvailableWeapons(weapons, true);
			if (selected != null)
			{
				this._weaponInfo.SetWeapons(selected, null);
			}
			WeaponSelectorItem weaponSelectorItem = this._items.Values.FirstOrDefault((WeaponSelectorItem x) => x.Weapon == selected);
			if (weaponSelectorItem != null)
			{
				this.SelectItem(weaponSelectorItem.ID, false, false);
			}
		}
		private void SelectComparativeItem(string panelID)
		{
			LogicalWeapon logicalWeapon = (this._selectedItem != null) ? this._selectedItem.Weapon : null;
			WeaponSelectorItem weaponSelectorItem;
			if (logicalWeapon != null && !string.IsNullOrEmpty(panelID) && this._items.TryGetValue(panelID, out weaponSelectorItem))
			{
				this._weaponInfo.SetWeapons(weaponSelectorItem.Weapon, logicalWeapon);
				return;
			}
			this._weaponInfo.SetWeapons(logicalWeapon, null);
		}
		protected override void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			WeaponSelectorItem selectedItem;
			if (this._items.TryGetValue(panelID, out selectedItem))
			{
				if (this._selectedItem != null)
				{
					this._selectedItem.SetSelected(false);
				}
				this._selectedItem = selectedItem;
				if (this._selectedItem != null)
				{
					this._selectedItem.SetSelected(true);
				}
				if (eventCallback)
				{
					base.WeaponSelectionChanged(this, rightClicked);
				}
			}
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			WeaponSelectorItem weaponSelectorItem;
			if (this._items.TryGetValue(panelId, out weaponSelectorItem))
			{
				if (msgType == "button_clicked")
				{
					this.SelectItem(panelId, true, false);
					return;
				}
				if (msgType == "button_rclicked")
				{
					this.SelectItem(panelId, true, true);
					return;
				}
				if (msgType == "mouse_enter")
				{
					this.SelectComparativeItem(panelId);
					return;
				}
				if (msgType == "mouse_leave")
				{
					this.SelectComparativeItem(null);
				}
			}
		}
		protected override void OnDisposing()
		{
			base.OnDisposing();
		}
	}
}
