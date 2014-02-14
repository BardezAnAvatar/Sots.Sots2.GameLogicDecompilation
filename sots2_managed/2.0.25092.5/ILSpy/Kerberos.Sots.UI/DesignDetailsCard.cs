using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DesignDetailsCard : PanelBinding
	{
		private App App;
		private DesignInfo Design;
		private ShipInfo Ship;
		private static string UIDesignName = "shipDesignName";
		private static string UICrewPanel = "UICrew";
		private static string UICrewValue = "CrewValueLabel";
		private static string UISupplyPanel = "UISupply";
		private static string UISupplyValue = "SupplyValueLabel";
		private static string UIPowerPanel = "UIPower";
		private static string UIPowerValue = "PowerValueLabel";
		private static string UIUpkeepLabel = "UpkeepLabel";
		private static string UIEnduranceLabel = "EnduranceLabel";
		private static string UIConstructionCostLabelValue = "ConstructionCostValueLabel";
		private static string UIShipCostLabelValue = "ShipCostValueLabel";
		private static string UIShipCommandLabel = "CommandSectionLabel";
		private static string UIShipMissionLabel = "MissionSectionLabel";
		private static string UIShipEngineLabel = "EngineSectionLabel";
		private static string ListWeaponIcons = "lstWeaponIcons";
		public DesignDetailsCard(App game, int designid, int? ShipID, UICommChannel ui, string id) : base(ui, id)
		{
			this.App = game;
			this.Design = this.App.GameDatabase.GetDesignInfo(designid);
			if (ShipID.HasValue)
			{
				this.Ship = this.App.GameDatabase.GetShipInfo(ShipID.Value, false);
				return;
			}
			this.Ship = null;
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
		}
		public void Initialize()
		{
			this.FillOutCard();
		}
		public void FillOutCard()
		{
			Faction faction = this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerInfo(this.Design.PlayerID).FactionID);
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIDesignName
			}), this.Design.Name);
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UICrewPanel
			}), faction.Name != "loa");
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				base.ID,
				"constructionBg"
			}), faction.Name == "loa");
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				base.ID,
				"standardBg"
			}), faction.Name != "loa");
			int crewAvailable = this.Design.CrewAvailable;
			int num = crewAvailable;
			int supplyAvailable = this.Design.SupplyAvailable;
			int num2 = supplyAvailable;
			int powerAvailable = this.Design.PowerAvailable;
			int num3 = powerAvailable;
			float scale = 1f;
			if (this.Ship != null)
			{
				num = 0;
				num2 = 0;
				List<SectionInstanceInfo> list = this.App.GameDatabase.GetShipSectionInstances(this.Ship.ID).ToList<SectionInstanceInfo>();
				foreach (SectionInstanceInfo current in list)
				{
					num += current.Crew;
					num2 += current.Supply;
				}
				FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this.Ship.FleetID);
				if (fleetInfo != null && this.App.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).Contains(AdmiralInfo.TraitType.Elite))
				{
					scale = this.App.AssetDatabase.EliteUpkeepCostScale;
				}
			}
			double num4 = GameSession.CalculateShipUpkeepCost(this.App.AssetDatabase, this.Design, scale, false);
			int endurance = this.Design.GetEndurance(this.App.Game);
			int playerProductionCost = this.Design.GetPlayerProductionCost(this.App.GameDatabase, this.Design.PlayerID, !this.Design.isPrototyped, null);
			int savingsCost = this.Design.SavingsCost;
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIShipCommandLabel
			}), "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIShipMissionLabel
			}), "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIShipEngineLabel
			}), "");
			List<int> list2 = new List<int>();
			DesignSectionInfo[] designSections = this.Design.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				if (designSectionInfo.ShipSectionAsset.Type == ShipSectionType.Command)
				{
					this.App.UI.SetText(this.App.UI.Path(new string[]
					{
						base.ID,
						DesignDetailsCard.UIShipCommandLabel
					}), App.Localize(designSectionInfo.ShipSectionAsset.Title));
				}
				else
				{
					if (designSectionInfo.ShipSectionAsset.Type == ShipSectionType.Mission)
					{
						this.App.UI.SetText(this.App.UI.Path(new string[]
						{
							base.ID,
							DesignDetailsCard.UIShipMissionLabel
						}), App.Localize(designSectionInfo.ShipSectionAsset.Title));
					}
					else
					{
						if (designSectionInfo.ShipSectionAsset.Type == ShipSectionType.Engine)
						{
							this.App.UI.SetText(this.App.UI.Path(new string[]
							{
								base.ID,
								DesignDetailsCard.UIShipEngineLabel
							}), App.Localize(designSectionInfo.ShipSectionAsset.Title));
						}
					}
				}
				foreach (WeaponBankInfo current2 in designSectionInfo.WeaponBanks)
				{
					if (current2.WeaponID.HasValue && !list2.Contains(current2.WeaponID.Value))
					{
						list2.Add(current2.WeaponID.Value);
					}
				}
			}
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UICrewValue
			}), num.ToString() + "/" + crewAvailable.ToString());
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UISupplyValue
			}), num2.ToString() + "/" + supplyAvailable.ToString());
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIPowerValue
			}), num3.ToString() + "/" + powerAvailable.ToString());
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIUpkeepLabel
			}), "Upkeep " + num4.ToString());
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIEnduranceLabel
			}), "Endurance " + endurance.ToString() + "T");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIConstructionCostLabelValue
			}), playerProductionCost.ToString("N0"));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				DesignDetailsCard.UIShipCostLabelValue
			}), savingsCost.ToString("N0"));
			this.App.UI.ClearItems(DesignDetailsCard.ListWeaponIcons);
			foreach (int current3 in list2)
			{
				string asset = this.App.GameDatabase.GetWeaponAsset(current3);
				LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == asset);
				if (logicalWeapon != null)
				{
					this.App.UI.AddItem(DesignDetailsCard.ListWeaponIcons, string.Empty, current3, "");
					string itemGlobalID = this.App.UI.GetItemGlobalID(DesignDetailsCard.ListWeaponIcons, string.Empty, current3, "");
					this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
					{
						itemGlobalID,
						"imgWeaponIcon"
					}), "sprite", logicalWeapon.IconSpriteName);
				}
			}
		}
		public void SyncDesign(int DesignID, int? Shipid)
		{
			this.Design = this.App.GameDatabase.GetDesignInfo(DesignID);
			if (Shipid.HasValue)
			{
				this.Ship = this.App.GameDatabase.GetShipInfo(Shipid.Value, false);
			}
			else
			{
				this.Ship = null;
			}
			this.FillOutCard();
		}
	}
}
