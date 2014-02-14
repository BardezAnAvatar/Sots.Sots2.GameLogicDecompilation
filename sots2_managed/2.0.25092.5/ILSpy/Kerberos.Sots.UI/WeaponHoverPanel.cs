using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class WeaponHoverPanel : WeaponTilePanel
	{
		public WeaponHoverPanel(UICommChannel ui, string id, string weaponInfoPanel = "") : base(ui, id, weaponInfoPanel)
		{
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			WeaponSelectorItem weaponSelectorItem;
			if (this._items.TryGetValue(panelId, out weaponSelectorItem))
			{
				if (msgType == "mouse_enter")
				{
					this.SelectItem(panelId, true, false);
					this._weaponInfo.SetWeapons(this._selectedItem.Weapon, null);
					this._weaponInfo.SetVisible(true);
					return;
				}
				if (msgType == "mouse_leave")
				{
					this._weaponInfo.SetVisible(false);
				}
			}
		}
	}
}
