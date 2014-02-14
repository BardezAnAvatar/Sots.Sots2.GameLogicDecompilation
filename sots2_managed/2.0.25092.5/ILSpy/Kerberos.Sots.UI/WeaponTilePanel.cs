using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class WeaponTilePanel : Panel
	{
		protected readonly WeaponInfoPanel _weaponInfo;
		protected readonly WeaponSelectorPage _page;
		protected readonly Dictionary<string, WeaponSelectorItem> _items = new Dictionary<string, WeaponSelectorItem>();
		protected WeaponSelectorItem _selectedItem;
		public virtual event WeaponSelectionChangedEventHandler SelectedWeaponChanged;
		public LogicalWeapon SelectedWeapon
		{
			get
			{
				if (this._selectedItem == null)
				{
					return null;
				}
				return this._selectedItem.Weapon;
			}
		}
		protected void WeaponSelectionChanged(object sender, bool isRightClick)
		{
			if (this.SelectedWeaponChanged != null)
			{
				this.SelectedWeaponChanged(sender, isRightClick);
			}
		}
		public WeaponTilePanel(UICommChannel ui, string id, string weaponInfoPanel = "") : base(ui, id, "WeaponSelector")
		{
			base.UI.ParentToMainPanel(base.ID);
			base.UI.SetDrawLayer(base.ID, 1);
			this._weaponInfo = new WeaponInfoPanel(base.UI, (weaponInfoPanel.Length > 0) ? weaponInfoPanel : base.UI.Path(new string[]
			{
				base.ID,
				"weaponInfo"
			}));
			this._page = new WeaponSelectorPage(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"page"
			}));
			base.UI.PanelMessage += new UIEventPanelMessage(this.UIPanelMessage);
		}
		private void UIPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			this.OnPanelMessage(panelName, msgType, msgParams);
		}
		private void DisposeItems()
		{
			foreach (WeaponSelectorItem current in this._items.Values)
			{
				current.Dispose();
			}
		}
		public void ClearItems()
		{
			this._page.DetachItems();
			this.DisposeItems();
			this._items.Clear();
		}
		public void SetAvailableWeapons(IEnumerable<LogicalWeapon> weapons, bool detach = true)
		{
			if (detach)
			{
				this._page.DetachItems();
			}
			this.DisposeItems();
			this._items.Clear();
			foreach (WeaponSelectorItem current in 
				from weapon in weapons
				select new WeaponSelectorItem(base.UI, Guid.NewGuid().ToString(), weapon))
			{
				this._items[current.ID] = current;
			}
			this._page.ReplaceItems(this._items.Values);
		}
		protected virtual void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			WeaponSelectorItem selectedItem;
			if (this._items.TryGetValue(panelID, out selectedItem))
			{
				this._selectedItem = selectedItem;
				if (eventCallback)
				{
					this.WeaponSelectionChanged(this, rightClicked);
				}
			}
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
		}
		protected override void OnDisposing()
		{
			base.UI.PanelMessage -= new UIEventPanelMessage(this.UIPanelMessage);
			this.DisposeItems();
			base.OnDisposing();
		}
	}
}
