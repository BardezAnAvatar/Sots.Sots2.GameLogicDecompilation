using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.UI
{
	internal class WeaponInfoPanel : PanelBinding
	{
		private class Group
		{
			private readonly ImageLabel _titleImageLabel;
			private readonly WeaponRangeInfo _pointBlankRangeInfo;
			private readonly WeaponRangeInfo _effectiveRangeInfo;
			private readonly WeaponRangeInfo _maximumRangeInfo;
			private readonly string _titlePrefix;
			public Group(UICommChannel ui, string rootId, string idSuffix, string titlePrefix)
			{
				this._titleImageLabel = new ImageLabel(ui, ui.Path(new string[]
				{
					rootId,
					"title" + idSuffix
				}));
				this._pointBlankRangeInfo = new WeaponRangeInfo(ui, ui.Path(new string[]
				{
					rootId,
					"rangeInfo.pb" + idSuffix
				}));
				this._effectiveRangeInfo = new WeaponRangeInfo(ui, ui.Path(new string[]
				{
					rootId,
					"rangeInfo.eff" + idSuffix
				}));
				this._maximumRangeInfo = new WeaponRangeInfo(ui, ui.Path(new string[]
				{
					rootId,
					"rangeInfo.max" + idSuffix
				}));
				this._titlePrefix = titlePrefix;
			}
			public void SetWeapon(LogicalWeapon weapon)
			{
				if (weapon != null)
				{
					this._titleImageLabel.Image.SetTexture(weapon.IconTextureName);
					this._titleImageLabel.Label.SetText(this._titlePrefix + weapon.Name);
					this._pointBlankRangeInfo.SetRangeInfo(weapon.RangeTable.PointBlank);
					this._effectiveRangeInfo.SetRangeInfo(weapon.RangeTable.Effective);
					this._maximumRangeInfo.SetRangeInfo(weapon.RangeTable.Maximum);
				}
				bool visible = weapon != null;
				this._titleImageLabel.SetVisible(visible);
				this._pointBlankRangeInfo.SetVisible(visible);
				this._effectiveRangeInfo.SetVisible(visible);
				this._maximumRangeInfo.SetVisible(visible);
			}
		}
		private string _contentPanelID;
		private readonly WeaponDamageGraphPanel _damageGraph;
		private readonly WeaponDeviationGraphPanel _deviationGraph;
		private readonly WeaponInfoPanel.Group _primaryGroup;
		private readonly WeaponInfoPanel.Group _comparativeGroup;
		private readonly WeaponScalarStats _scalarStats;
		public WeaponInfoPanel(UICommChannel ui, string id) : base(ui, id)
		{
			this._contentPanelID = base.UI.Path(new string[]
			{
				id,
				"content"
			});
			this._damageGraph = new WeaponDamageGraphPanel(base.UI, base.UI.Path(new string[]
			{
				this._contentPanelID,
				"graphGroup.damGraphFrame.damGraph"
			}));
			this._deviationGraph = new WeaponDeviationGraphPanel(base.UI, base.UI.Path(new string[]
			{
				this._contentPanelID,
				"graphGroup.devGraphFrame.devGraph"
			}));
			this._scalarStats = new WeaponScalarStats(base.UI, base.UI.Path(new string[]
			{
				this._contentPanelID,
				"scalarStats"
			}));
			this._primaryGroup = new WeaponInfoPanel.Group(base.UI, this._contentPanelID, "1", string.Empty);
			this._comparativeGroup = new WeaponInfoPanel.Group(base.UI, this._contentPanelID, "2", AssetDatabase.CommonStrings.Localize("@UI_COMPARATIVE_WEAPON_VERSUS") + " ");
		}
		public void SetWeapons(LogicalWeapon primary, LogicalWeapon comparative)
		{
			WeaponRangeTable primary2 = (primary != null) ? primary.RangeTable : null;
			WeaponRangeTable comparative2 = (comparative != null) ? comparative.RangeTable : null;
			this._damageGraph.SetRangeTables(primary2, comparative2);
			this._deviationGraph.SetRangeTables(primary2, comparative2);
			this._scalarStats.SetWeapons(primary, comparative);
			this._primaryGroup.SetWeapon(primary);
			this._comparativeGroup.SetWeapon(comparative);
		}
	}
}
