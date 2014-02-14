using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.UI
{
	internal class WeaponRangeInfo : PanelBinding
	{
		private readonly Label _rangeLabel;
		private readonly Label _deviationLabel;
		private readonly Label _damageLabel;
		public WeaponRangeInfo(UICommChannel ui, string id) : base(ui, id)
		{
			this._rangeLabel = new Label(ui, base.UI.Path(new string[]
			{
				base.ID,
				"rangeLabel"
			}));
			this._deviationLabel = new Label(ui, base.UI.Path(new string[]
			{
				base.ID,
				"deviationLabel"
			}));
			this._damageLabel = new Label(ui, base.UI.Path(new string[]
			{
				base.ID,
				"damageLabel"
			}));
		}
		public void SetRangeInfo(WeaponRangeTableItem rangeTableItem)
		{
			this._rangeLabel.SetText(rangeTableItem.Range.ToString("N0"));
			this._deviationLabel.SetText(rangeTableItem.Deviation.ToString("N1"));
			this._damageLabel.SetText(rangeTableItem.Damage.ToString("N0"));
		}
	}
}
