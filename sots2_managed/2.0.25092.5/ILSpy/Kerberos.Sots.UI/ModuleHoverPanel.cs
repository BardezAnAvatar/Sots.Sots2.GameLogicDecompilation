using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class ModuleHoverPanel : ModuleTilePanel
	{
		public ModuleHoverPanel(UICommChannel ui, string id, string moduleInfoPanel = "") : base(ui, id, moduleInfoPanel)
		{
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			ModuleSelectorItem moduleSelectorItem;
			if (this._items.TryGetValue(panelId, out moduleSelectorItem))
			{
				if (msgType == "mouse_enter")
				{
					this.SelectItem(panelId, true, false);
					this._moduleInfo.SetModule(this._selectedItem.Module);
					this._moduleInfo.SetVisible(true);
					return;
				}
				if (msgType == "mouse_leave")
				{
					this._moduleInfo.SetVisible(false);
				}
			}
		}
	}
}
