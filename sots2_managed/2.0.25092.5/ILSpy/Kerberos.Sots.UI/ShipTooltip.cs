using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class ShipTooltip
	{
		private const string LabelDesignName = "lblDesignName";
		private const string LabelShipName = "lblShipName";
		private const string LabelFirstSection = "lblFirstSection";
		private const string LabelSecondSection = "lblSecondSection";
		private const string LabelThirdSection = "lblThirdSection";
		private const string LabelAttributeName = "lblAttributeName";
		private const string LabelUpkeepName = "lblUpkeep";
		private const string LabelShipAge = "lblShipAge";
		private const string LabelSupply = "lblSupply";
		private const string LabelPower = "lblPower";
		private const string LabelCrew = "lblCrew";
		private const string LabelEndurance = "lblEndurance";
		private const string ListWeaponIcons = "lstWeaponIcons";
		private const string ObjectHostShip = "ohShip";
		private string _rootPanel = "";
		private int _shipID;
		private int _designID;
		public App _game;
		public App App;
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private ShipBuilder _builder;
		private ShipHoloView _shipHoloView;
		private static Rectangle ArmorPanelShape = new Rectangle
		{
			X = 390f,
			Y = 22f,
			W = 54f,
			H = 54f
		};
		private bool _ready;
		private bool _activated;
		public ShipTooltip(App app)
		{
			this._game = app;
			this.App = app;
			this._rootPanel = this.App.UI.CreatePanelFromTemplate("ShipinfoTooltip", null);
		}
		public void Initialize()
		{
			this._builder = new ShipBuilder(this.App);
			this._crits = new GameObjectSet(this.App);
			this._camera = new OrbitCameraController(this.App);
			this._camera.DesiredDistance = 10000f;
			this._crits.Add(this._camera);
			this._shipHoloView = new ShipHoloView(this.App, this._camera);
			this._shipID = 0;
		}
		public void SyncShipTooltip(int shipid)
		{
			if (shipid == this._shipID)
			{
				return;
			}
			this.SyncDesignTooltip(this._rootPanel, shipid);
			this._shipID = shipid;
			this._designID = -1;
		}
		public void SyncShipTooltipByDesignID(int designid)
		{
			if (this._designID == designid)
			{
				return;
			}
			this.SyncDesignTooltipByDesign(this._rootPanel, designid);
			this._designID = designid;
			this._shipID = -1;
		}
		public int GetShipID()
		{
			return this._shipID;
		}
		public bool isvalid()
		{
			return this._builder != null && this._shipHoloView != null && this._crits != null;
		}
		public string GetPanelID()
		{
			return this._rootPanel;
		}
		private void SyncDesignTooltip(string panelId, int shipId)
		{
			ShipInfo shipInfo = this._game.GameDatabase.GetShipInfo(shipId, true);
			List<SectionEnumerations.DesignAttribute> list = this._game.GameDatabase.GetDesignAttributesForDesign(shipInfo.DesignInfo.ID).ToList<SectionEnumerations.DesignAttribute>();
			FleetInfo fleetInfo = this._game.GameDatabase.GetFleetInfo(shipInfo.FleetID);
			List<int> list2 = new List<int>();
			DesignSectionInfo[] designSections = shipInfo.DesignInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				foreach (WeaponBankInfo current in designSectionInfo.WeaponBanks)
				{
					if (current.WeaponID.HasValue && !list2.Contains(current.WeaponID.Value))
					{
						list2.Add(current.WeaponID.Value);
					}
				}
			}
			string str = "";
			switch (shipInfo.DesignInfo.Class)
			{
			case ShipClass.Cruiser:
				str = App.Localize("@SHIPCLASSES_ABBR_CR");
				break;
			case ShipClass.Dreadnought:
				str = App.Localize("@SHIPCLASSES_ABBR_DN");
				break;
			case ShipClass.Leviathan:
				str = App.Localize("@SHIPCLASSES_ABBR_LV");
				break;
			}
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblDesignName"
			}), shipInfo.ShipName);
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblShipName"
			}), str + " - " + shipInfo.DesignInfo.Name);
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblFirstSection"
			}), "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblSecondSection"
			}), "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblThirdSection"
			}), "");
			DesignSectionInfo[] designSections2 = shipInfo.DesignInfo.DesignSections;
			for (int j = 0; j < designSections2.Length; j++)
			{
				DesignSectionInfo designSectionInfo2 = designSections2[j];
				switch (designSectionInfo2.ShipSectionAsset.Type)
				{
				case ShipSectionType.Command:
					this.App.UI.SetText(this.App.UI.Path(new string[]
					{
						panelId,
						"lblFirstSection"
					}), App.Localize(designSectionInfo2.ShipSectionAsset.Title));
					break;
				case ShipSectionType.Mission:
					this.App.UI.SetText(this.App.UI.Path(new string[]
					{
						panelId,
						"lblSecondSection"
					}), App.Localize(designSectionInfo2.ShipSectionAsset.Title));
					break;
				case ShipSectionType.Engine:
					this.App.UI.SetText(this.App.UI.Path(new string[]
					{
						panelId,
						"lblThirdSection"
					}), App.Localize(designSectionInfo2.ShipSectionAsset.Title));
					break;
				}
			}
			float scale = this.App.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).Contains(AdmiralInfo.TraitType.Elite) ? this.App.AssetDatabase.EliteUpkeepCostScale : 1f;
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblAttributeName"
			}), (shipInfo.DesignInfo.isAttributesDiscovered && list.Count > 0) ? App.Localize("@UI_" + list.First<SectionEnumerations.DesignAttribute>().ToString()) : "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblUpkeep"
			}), GameSession.CalculateShipUpkeepCost(this._game.AssetDatabase, shipInfo.DesignInfo, scale, fleetInfo.IsReserveFleet).ToString());
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblShipAge"
			}), (this.App.GameDatabase.GetTurnCount() - shipInfo.ComissionDate).ToString() + " " + App.Localize("@UI_SHIPTOOLTIPAGE"));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblSupply"
			}), string.Format("{0}/{1}", shipInfo.DesignInfo.SupplyRequired, shipInfo.DesignInfo.SupplyAvailable));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblPower"
			}), string.Format("{0}/{1}", shipInfo.DesignInfo.PowerRequired, shipInfo.DesignInfo.PowerAvailable));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblCrew"
			}), string.Format("{0}/{1}", shipInfo.DesignInfo.CrewRequired, shipInfo.DesignInfo.CrewAvailable));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblEndurance"
			}), shipInfo.DesignInfo.GetEndurance(this._game.Game).ToString());
			this.App.UI.ClearItems("lstWeaponIcons");
			foreach (int current2 in list2)
			{
				string asset = this.App.GameDatabase.GetWeaponAsset(current2);
				LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == asset);
				if (logicalWeapon != null)
				{
					this.App.UI.AddItem("lstWeaponIcons", string.Empty, current2, "");
					string itemGlobalID = this.App.UI.GetItemGlobalID("lstWeaponIcons", string.Empty, current2, "");
					this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
					{
						itemGlobalID,
						"imgWeaponIcon"
					}), "sprite", logicalWeapon.IconSpriteName);
				}
			}
			List<SectionInstanceInfo> list3 = this.App.GameDatabase.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>();
			Dictionary<ArmorSide, float> dictionary = new Dictionary<ArmorSide, float>();
			Dictionary<ArmorSide, float> dictionary2 = new Dictionary<ArmorSide, float>();
			if (list3.Count > 0)
			{
				ArmorSide[] array = (ArmorSide[])Enum.GetValues(typeof(ArmorSide));
				foreach (SectionInstanceInfo current3 in list3)
				{
					ArmorSide[] array2 = array;
					for (int k = 0; k < array2.Length; k++)
					{
						ArmorSide armorSide = array2[k];
						if (armorSide != ArmorSide.NumSides)
						{
							if (!dictionary.ContainsKey(armorSide))
							{
								dictionary.Add(armorSide, 0f);
							}
							if (!dictionary2.ContainsKey(armorSide))
							{
								dictionary2.Add(armorSide, 0f);
							}
							if (current3.Armor.ContainsKey(armorSide))
							{
								Dictionary<ArmorSide, float> dictionary3;
								ArmorSide key;
								(dictionary3 = dictionary)[key = armorSide] = dictionary3[key] + (float)current3.Armor[armorSide].GetTotalFilled();
								Dictionary<ArmorSide, float> dictionary4;
								ArmorSide key2;
								(dictionary4 = dictionary2)[key2 = armorSide] = dictionary4[key2] + (float)current3.Armor[armorSide].GetTotalPoints();
							}
						}
					}
				}
				float num = (dictionary2[ArmorSide.Top] == 0f) ? 1f : (dictionary[ArmorSide.Top] / dictionary2[ArmorSide.Top]);
				float num2 = (dictionary2[ArmorSide.Bottom] == 0f) ? 1f : (dictionary[ArmorSide.Bottom] / dictionary2[ArmorSide.Bottom]);
				float num3 = (dictionary2[ArmorSide.Left] == 0f) ? 1f : (dictionary[ArmorSide.Left] / dictionary2[ArmorSide.Left]);
				float num4 = (dictionary2[ArmorSide.Right] == 0f) ? 1f : (dictionary[ArmorSide.Right] / dictionary2[ArmorSide.Right]);
				float num5 = ShipTooltip.ArmorPanelShape.W / 2f;
				this.App.UI.SetShape("top_armor", 0, (int)(num5 * (1f - num)), (int)ShipTooltip.ArmorPanelShape.W, (int)(num5 * num));
				this.App.UI.SetShape("bottom_armor", 0, (int)num5, (int)ShipTooltip.ArmorPanelShape.W, (int)(num2 * num5));
				this.App.UI.SetShape("left_armor", (int)(num5 * (1f - num3)), 0, (int)(num5 * num3), (int)ShipTooltip.ArmorPanelShape.H);
				this.App.UI.SetShape("right_armor", (int)num5, 0, (int)(num5 * num4), (int)ShipTooltip.ArmorPanelShape.H);
			}
			else
			{
				float num6 = ShipTooltip.ArmorPanelShape.W / 2f;
				this.App.UI.SetShape("top_armor", 0, 0, (int)ShipTooltip.ArmorPanelShape.W, (int)num6);
				this.App.UI.SetShape("bottom_armor", 0, (int)num6, (int)ShipTooltip.ArmorPanelShape.W, (int)num6);
				this.App.UI.SetShape("left_armor", 0, 0, (int)num6, (int)ShipTooltip.ArmorPanelShape.H);
				this.App.UI.SetShape("right_armor", (int)num6, 0, (int)num6, (int)ShipTooltip.ArmorPanelShape.H);
			}
			this._builder.New(this.App.GetPlayer(shipInfo.DesignInfo.PlayerID), shipInfo.DesignInfo, shipInfo.ShipName, shipInfo.SerialNumber, false);
			this._ready = false;
			this._activated = false;
		}
		private void SyncDesignTooltipByDesign(string panelId, int designID)
		{
			DesignInfo designInfo = this._game.GameDatabase.GetDesignInfo(designID);
			List<SectionEnumerations.DesignAttribute> list = this._game.GameDatabase.GetDesignAttributesForDesign(designInfo.ID).ToList<SectionEnumerations.DesignAttribute>();
			List<int> list2 = new List<int>();
			DesignSectionInfo[] designSections = designInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				foreach (WeaponBankInfo current in designSectionInfo.WeaponBanks)
				{
					if (current.WeaponID.HasValue && !list2.Contains(current.WeaponID.Value))
					{
						list2.Add(current.WeaponID.Value);
					}
				}
			}
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblDesignName"
			}), designInfo.Name);
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblShipName"
			}), "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblFirstSection"
			}), (designInfo.DesignSections.Count<DesignSectionInfo>() > 0) ? designInfo.DesignSections[0].ShipSectionAsset.Title : "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblSecondSection"
			}), (designInfo.DesignSections.Count<DesignSectionInfo>() > 1) ? designInfo.DesignSections[1].ShipSectionAsset.Title : "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblThirdSection"
			}), (designInfo.DesignSections.Count<DesignSectionInfo>() > 2) ? designInfo.DesignSections[2].ShipSectionAsset.Title : "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblAttributeName"
			}), (designInfo.isAttributesDiscovered && list.Count > 0) ? App.Localize("@UI_" + list.First<SectionEnumerations.DesignAttribute>().ToString()) : "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblUpkeep"
			}), GameSession.CalculateShipUpkeepCost(this._game.AssetDatabase, designInfo, 1f, false).ToString());
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblShipAge"
			}), "");
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblSupply"
			}), string.Format("{0}/{1}", designInfo.SupplyRequired, designInfo.SupplyAvailable));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblPower"
			}), string.Format("{0}/{1}", designInfo.PowerRequired, designInfo.PowerAvailable));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblCrew"
			}), string.Format("{0}/{1}", designInfo.CrewRequired, designInfo.CrewAvailable));
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				panelId,
				"lblEndurance"
			}), designInfo.GetEndurance(this._game.Game).ToString());
			this.App.UI.ClearItems("lstWeaponIcons");
			foreach (int current2 in list2)
			{
				string asset = this.App.GameDatabase.GetWeaponAsset(current2);
				LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == asset);
				if (logicalWeapon != null)
				{
					this.App.UI.AddItem("lstWeaponIcons", string.Empty, current2, "");
					string itemGlobalID = this.App.UI.GetItemGlobalID("lstWeaponIcons", string.Empty, current2, "");
					this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
					{
						itemGlobalID,
						"imgWeaponIcon"
					}), "sprite", logicalWeapon.IconSpriteName);
				}
			}
			float num = ShipTooltip.ArmorPanelShape.W / 2f;
			this.App.UI.SetShape("top_armor", 0, 0, (int)ShipTooltip.ArmorPanelShape.W, (int)num);
			this.App.UI.SetShape("bottom_armor", 0, (int)num, (int)ShipTooltip.ArmorPanelShape.W, (int)num);
			this.App.UI.SetShape("left_armor", 0, 0, (int)num, (int)ShipTooltip.ArmorPanelShape.H);
			this.App.UI.SetShape("right_armor", (int)num, 0, (int)num, (int)ShipTooltip.ArmorPanelShape.H);
			this._builder.New(this.App.GetPlayer(designInfo.PlayerID), designInfo, "The Ship You Wish your Ship was", 0, false);
			this._ready = false;
			this._activated = false;
		}
		public void Update()
		{
			if (this._builder != null && this._shipHoloView != null && this._crits != null)
			{
				this._builder.Update();
				if (this._builder.Ship != null && this._builder.Ship.Active && !this._ready && this._crits != null && this._crits.IsReady())
				{
					this._ready = true;
					if (!this._activated)
					{
						this._activated = true;
						this._crits.Activate();
						this._camera.MaxDistance = 2000f;
						this._camera.DesiredDistance = 300f;
						this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
						this._camera.DesiredPitch = MathHelper.DegreesToRadians(90f);
					}
					this._shipHoloView.SetShip(this._builder.Ship);
					this.App.UI.Send(new object[]
					{
						"SetGameObject",
						this.App.UI.Path(new string[]
						{
							this._rootPanel,
							"ohShip"
						}),
						this._shipHoloView.ObjectID
					});
				}
			}
		}
		public void Clear()
		{
			this._builder.Clear();
		}
		public void Dispose(bool KeepRoot = false)
		{
			if (this._builder != null)
			{
				this._builder.Dispose();
			}
			if (this._shipHoloView != null)
			{
				this._shipHoloView.Dispose();
			}
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
			}
			if (!KeepRoot)
			{
				this.App.UI.DestroyPanel(this._rootPanel);
			}
			this._shipID = 0;
		}
	}
}
