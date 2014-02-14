using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class ModuleSelector : ModuleTilePanel
	{
		public ModuleSelector(UICommChannel ui, string id, string moduleInfoPanel = "") : base(ui, id, moduleInfoPanel)
		{
		}
		private void DisposeItems()
		{
			foreach (ModuleSelectorItem current in this._items.Values)
			{
				current.Dispose();
			}
		}
		protected override void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			ModuleSelectorItem selectedItem;
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
					base.ModuleSelectionChanged(this, rightClicked);
				}
			}
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			ModuleSelectorItem item;
			if (this._items.TryGetValue(panelId, out item))
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
					this.HoverItem(item);
					return;
				}
				if (msgType == "mouse_leave")
				{
					this.HoverItem(null);
				}
			}
		}
		protected override void OnDisposing()
		{
			base.OnDisposing();
		}
	}
}
