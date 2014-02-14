using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class PsionicTilePanel : Panel
	{
		protected readonly PsionicInfoPanel _moduleInfo;
		protected readonly PsionicSelectorPage _page;
		protected readonly Dictionary<string, PsionicSelectorItem> _items = new Dictionary<string, PsionicSelectorItem>();
		protected PsionicSelectorItem _selectedItem;
		public event PsionicSelectionChangedEventHandler SelectedPsionicChanged;
		public LogicalPsionic SelectedPsionic
		{
			get
			{
				if (this._selectedItem == null)
				{
					return null;
				}
				return this._selectedItem.Psionic;
			}
		}
		protected void ModuleSelectionChanged(object sender, bool isRightClick)
		{
			if (this.SelectedPsionicChanged != null)
			{
				this.SelectedPsionicChanged(sender, isRightClick);
			}
		}
		public PsionicTilePanel(UICommChannel ui, string id, string moduleInfoPanel = "") : base(ui, id, "PsionicSelector")
		{
			base.UI.ParentToMainPanel(base.ID);
			base.UI.SetDrawLayer(base.ID, 1);
			this._moduleInfo = new PsionicInfoPanel(base.UI, (moduleInfoPanel.Length > 0) ? moduleInfoPanel : base.UI.Path(new string[]
			{
				base.ID,
				"psionicInfo"
			}));
			this._page = new PsionicSelectorPage(base.UI, base.UI.Path(new string[]
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
			foreach (PsionicSelectorItem current in this._items.Values)
			{
				current.Dispose();
			}
		}
		public void SetAvailablePsionics(IEnumerable<LogicalPsionic> modules, LogicalPsionic selected, bool enableSelection = true)
		{
			if (enableSelection)
			{
				this._page.DetachItems();
			}
			this.DisposeItems();
			this._items.Clear();
			foreach (PsionicSelectorItem current in 
				from module in modules
				select new PsionicSelectorItem(base.UI, Guid.NewGuid().ToString(), module))
			{
				this._items[current.ID] = current;
			}
			if (enableSelection)
			{
				LogicalPsionic logicalPsionic = new LogicalPsionic();
				logicalPsionic.Name = "No Psionic";
				logicalPsionic.PsionicTitle = "No Psionic";
				logicalPsionic.Description = "The selected psionic slot will be empty.";
				logicalPsionic.Icon = "moduleicon_no_selection";
				PsionicSelectorItem psionicSelectorItem = new PsionicSelectorItem(base.UI, Guid.NewGuid().ToString(), logicalPsionic);
				this._items[psionicSelectorItem.ID] = psionicSelectorItem;
				bool flag = false;
				if (selected != null)
				{
					PsionicSelectorItem psionicSelectorItem2 = this._items.Values.FirstOrDefault((PsionicSelectorItem x) => x.Psionic == selected);
					if (psionicSelectorItem2 != null)
					{
						this.HoverItem(psionicSelectorItem2);
						flag = true;
					}
				}
				if (!flag)
				{
					this.HoverItem(psionicSelectorItem);
				}
			}
			this._page.ReplaceItems(this._items.Values, enableSelection);
		}
		protected virtual void HoverItem(PsionicSelectorItem item)
		{
			if (item != null)
			{
				this._moduleInfo.SetPsionic(item.Psionic);
				return;
			}
			this._moduleInfo.SetPsionic(null);
		}
		protected virtual void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			if (panelID == null)
			{
				return;
			}
			PsionicSelectorItem psionicSelectorItem;
			if (this._items.TryGetValue(panelID, out psionicSelectorItem))
			{
				if (this._selectedItem == psionicSelectorItem)
				{
					return;
				}
				this._selectedItem = psionicSelectorItem;
				if (eventCallback && this.SelectedPsionicChanged != null)
				{
					this.SelectedPsionicChanged(this, rightClicked);
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
