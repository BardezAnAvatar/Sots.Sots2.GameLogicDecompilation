using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.UI
{
	internal class ModuleSelectorItem : ImageButton
	{
		private readonly Image _selectedOverlayImage;
		public LogicalModule Module
		{
			get;
			private set;
		}
		public bool IsSelected
		{
			get;
			private set;
		}
		public void SetSelected(bool value)
		{
			if (value == this.IsSelected)
			{
				return;
			}
			this.IsSelected = value;
			this._selectedOverlayImage.SetVisible(value);
		}
		public ModuleSelectorItem(UICommChannel ui, string id, LogicalModule module) : base(ui, id, "WeaponSelectorIcon")
		{
			if (module == null)
			{
				throw new ArgumentNullException("module");
			}
			this.Module = module;
			this._selectedOverlayImage = new Image(ui, base.UI.Path(new string[]
			{
				id,
				"selectedOverlay"
			}));
			base.SetSprite(module.Icon);
			base.UI.SetPostMouseOverEvents(base.ID, true);
		}
	}
}
