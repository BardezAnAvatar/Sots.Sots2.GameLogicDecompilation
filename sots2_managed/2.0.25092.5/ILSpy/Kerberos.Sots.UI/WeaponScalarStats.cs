using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.UI
{
	internal class WeaponScalarStats : PanelBinding
	{
		private readonly ImageLabel _rateOfFireLabel;
		private readonly ImageLabel _popDamageLabel;
		private readonly ImageLabel _infraDamageLabel;
		private readonly ImageLabel _terraDamageLabel;
		public WeaponScalarStats(UICommChannel ui, string id) : base(ui, id)
		{
			this._rateOfFireLabel = new ImageLabel(ui, base.UI.Path(new string[]
			{
				base.ID,
				"rofIconLabel"
			}));
			this._popDamageLabel = new ImageLabel(ui, base.UI.Path(new string[]
			{
				base.ID,
				"popIconLabel"
			}));
			this._infraDamageLabel = new ImageLabel(ui, base.UI.Path(new string[]
			{
				base.ID,
				"infraIconLabel"
			}));
			this._terraDamageLabel = new ImageLabel(ui, base.UI.Path(new string[]
			{
				base.ID,
				"terraIconLabel"
			}));
		}
		private static string GetSignSuffix(float delta)
		{
			if (delta > 0f)
			{
				return " (+)";
			}
			if (delta < 0f)
			{
				return " (-)";
			}
			return string.Empty;
		}
		public void SetWeapons(LogicalWeapon primary, LogicalWeapon comparative)
		{
			bool visible = primary != null;
			this._rateOfFireLabel.Label.SetVisible(visible);
			this._popDamageLabel.Label.SetVisible(visible);
			this._infraDamageLabel.Label.SetVisible(visible);
			this._terraDamageLabel.Label.SetVisible(visible);
			if (primary != null)
			{
				float num = primary.GetRateOfFire() * 60f;
				float popDamage = primary.PopDamage;
				float num2 = primary.InfraDamage * 100f;
				float terraDamage = primary.TerraDamage;
				if (comparative != null)
				{
					float num3 = comparative.GetRateOfFire() * 60f;
					float popDamage2 = comparative.PopDamage;
					float num4 = comparative.InfraDamage * 100f;
					float terraDamage2 = comparative.TerraDamage;
					float delta = num - num3;
					float delta2 = popDamage - popDamage2;
					float delta3 = num2 - num4;
					float delta4 = terraDamage - terraDamage2;
					this._rateOfFireLabel.Label.SetText(num.ToString("N1") + WeaponScalarStats.GetSignSuffix(delta));
					this._popDamageLabel.Label.SetText(popDamage.ToString("N0") + WeaponScalarStats.GetSignSuffix(delta2));
					this._infraDamageLabel.Label.SetText(num2.ToString("N3") + WeaponScalarStats.GetSignSuffix(delta3));
					this._terraDamageLabel.Label.SetText(terraDamage.ToString("N2") + WeaponScalarStats.GetSignSuffix(delta4));
					return;
				}
				this._rateOfFireLabel.Label.SetText(num.ToString("N1"));
				this._popDamageLabel.Label.SetText(popDamage.ToString("N0"));
				this._infraDamageLabel.Label.SetText(num2.ToString("N5"));
				this._terraDamageLabel.Label.SetText(terraDamage.ToString("N2"));
			}
		}
	}
}
