using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.UI
{
	internal class WeaponSelectorItem : ImageButton
	{
		private readonly Image _selectedOverlayImage;
		public LogicalWeapon Weapon
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
		public WeaponSelectorItem(UICommChannel ui, string id, LogicalWeapon weapon) : base(ui, id, "WeaponSelectorIcon")
		{
			if (weapon == null)
			{
				throw new ArgumentNullException("weapon");
			}
			this.Weapon = weapon;
			this._selectedOverlayImage = new Image(ui, base.UI.Path(new string[]
			{
				id,
				"selectedOverlay"
			}));
			base.SetTexture(weapon.IconTextureName);
			base.UI.SetPostMouseOverEvents(base.ID, true);
		}
	}
}
