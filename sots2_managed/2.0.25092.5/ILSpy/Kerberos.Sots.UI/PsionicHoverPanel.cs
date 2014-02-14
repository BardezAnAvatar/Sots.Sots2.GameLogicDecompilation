using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class PsionicHoverPanel : PsionicTilePanel
	{
		public PsionicHoverPanel(UICommChannel ui, string id, string psionicInfoPanel = "") : base(ui, id, psionicInfoPanel)
		{
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			PsionicSelectorItem psionicSelectorItem;
			if (this._items.TryGetValue(panelId, out psionicSelectorItem))
			{
				if (msgType == "mouse_enter")
				{
					this.SelectItem(panelId, true, false);
					this._moduleInfo.SetPsionic(this._selectedItem.Psionic);
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
