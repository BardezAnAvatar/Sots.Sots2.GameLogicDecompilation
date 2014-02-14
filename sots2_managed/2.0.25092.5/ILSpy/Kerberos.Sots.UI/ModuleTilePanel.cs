using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class ModuleTilePanel : Panel
	{
		protected readonly ModuleInfoPanel _moduleInfo;
		protected readonly ModuleSelectorPage _page;
		protected readonly Dictionary<string, ModuleSelectorItem> _items = new Dictionary<string, ModuleSelectorItem>();
		protected ModuleSelectorItem _selectedItem;
		public event ModuleSelectionChangedEventHandler SelectedModuleChanged;
		public LogicalModule SelectedModule
		{
			get
			{
				if (this._selectedItem == null)
				{
					return null;
				}
				return this._selectedItem.Module;
			}
		}
		protected void ModuleSelectionChanged(object sender, bool isRightClick)
		{
			if (this.SelectedModuleChanged != null)
			{
				this.SelectedModuleChanged(sender, isRightClick);
			}
		}
		public ModuleTilePanel(UICommChannel ui, string id, string moduleInfoPanel = "") : base(ui, id, "ModuleSelector")
		{
			base.UI.ParentToMainPanel(base.ID);
			base.UI.SetDrawLayer(base.ID, 1);
			this._moduleInfo = new ModuleInfoPanel(base.UI, (moduleInfoPanel.Length > 0) ? moduleInfoPanel : base.UI.Path(new string[]
			{
				base.ID,
				"moduleInfo"
			}));
			this._page = new ModuleSelectorPage(base.UI, base.UI.Path(new string[]
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
			foreach (ModuleSelectorItem current in this._items.Values)
			{
				current.Dispose();
			}
		}
		public void SetAvailableModules(IEnumerable<LogicalModule> modules, LogicalModule selected, bool enableSelection = true)
		{
			if (enableSelection)
			{
				this._page.DetachItems();
			}
			this.DisposeItems();
			this._items.Clear();
			foreach (ModuleSelectorItem current in 
				from module in modules
				select new ModuleSelectorItem(base.UI, Guid.NewGuid().ToString(), module))
			{
				this._items[current.ID] = current;
			}
			if (enableSelection)
			{
				LogicalModule logicalModule = new LogicalModule();
				logicalModule.ModuleName = App.Localize("@UI_MODULENAME_NO_MODULE");
				logicalModule.ModuleTitle = App.Localize("@UI_MODULENAME_NO_MODULE");
				logicalModule.Description = App.Localize("@UI_NO_MODULE_DESC");
				logicalModule.Icon = "moduleicon_no_selection";
				ModuleSelectorItem moduleSelectorItem = new ModuleSelectorItem(base.UI, Guid.NewGuid().ToString(), logicalModule);
				this._items[moduleSelectorItem.ID] = moduleSelectorItem;
				bool flag = false;
				if (selected != null)
				{
					ModuleSelectorItem moduleSelectorItem2 = this._items.Values.FirstOrDefault((ModuleSelectorItem x) => x.Module == selected);
					if (moduleSelectorItem2 != null)
					{
						this.HoverItem(moduleSelectorItem2);
						flag = true;
					}
				}
				if (!flag)
				{
					this.HoverItem(moduleSelectorItem);
				}
			}
			this._page.ReplaceItems(this._items.Values, enableSelection);
		}
		protected virtual void HoverItem(ModuleSelectorItem item)
		{
			if (item != null)
			{
				this._moduleInfo.SetModule(item.Module);
				return;
			}
			this._moduleInfo.SetModule(null);
		}
		protected virtual void SelectItem(string panelID, bool eventCallback, bool rightClicked)
		{
			if (panelID == null)
			{
				return;
			}
			ModuleSelectorItem moduleSelectorItem;
			if (this._items.TryGetValue(panelID, out moduleSelectorItem))
			{
				if (this._selectedItem == moduleSelectorItem)
				{
					return;
				}
				this._selectedItem = moduleSelectorItem;
				if (eventCallback && this.SelectedModuleChanged != null)
				{
					this.SelectedModuleChanged(this, rightClicked);
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
