using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class AdmiralInfoDialog : Dialog
	{
		private int _admiralID;
		private static string panel = "admiralContent";
		public AdmiralInfoDialog(App game, int admiralID, string template = "admiralPopUp") : base(game, template)
		{
			this._admiralID = admiralID;
		}
		public override void Initialize()
		{
			this.RefreshAdmiral(this._admiralID);
		}
		public void RefreshAdmiral(int admiralID)
		{
			string text = AdmiralInfoDialog.panel;
			AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(admiralID);
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				text,
				"fleetitem.expand_button"
			}), false);
			string arg = App.Localize("@ADMIRAL_IN_DEEP_SPACE");
			FleetInfo fleetInfoByAdmiralID = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiralID, FleetType.FL_NORMAL);
			if (fleetInfoByAdmiralID != null)
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.expand_button"
				}), false);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.idle.list_item.listitem_name"
				}), "text", fleetInfoByAdmiralID.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.mouse_over.list_item.listitem_name"
				}), "text", fleetInfoByAdmiralID.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_sel.idle.list_item.listitem_name"
				}), "text", fleetInfoByAdmiralID.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_sel.mouse_over.list_item.listitem_name"
				}), "text", fleetInfoByAdmiralID.Name);
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfoByAdmiralID.SystemID);
				if (starSystemInfo != null)
				{
					arg = starSystemInfo.Name;
				}
				else
				{
					arg = App.Localize("@ADMIRAL_IN_DEEP_SPACE");
				}
				MissionInfo missionByFleetID = this._app.GameDatabase.GetMissionByFleetID(fleetInfoByAdmiralID.ID);
				if (missionByFleetID != null)
				{
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text,
						"fleetitem.sub_title"
					}), "text", fleetInfoByAdmiralID.Name);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						text,
						"cancelMissionButton"
					}), false);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						text,
						"fleetitem.on_mission"
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						text,
						"cancelMissionButton"
					}), false);
				}
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"createFleetButton"
				}), false);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem"
				}), true);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"dissolveButton"
				}), false);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem"
				}), false);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"dissolveButton"
				}), false);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"createFleetButton"
				}), false);
			}
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				text,
				"admiralName"
			}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_NAME_COLON"), admiralInfo.Name));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				text,
				"admiralLocation"
			}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_LOCATION_COLON"), arg));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				text,
				"admiralAge"
			}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_AGE_COLON"), ((int)admiralInfo.Age).ToString()));
			IEnumerable<AdmiralInfo.TraitType> admiralTraits = this._app.GameDatabase.GetAdmiralTraits(admiralInfo.ID);
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				base.ID,
				text,
				"admiralTraits"
			}));
			int num = 1;
			foreach (AdmiralInfo.TraitType current in admiralTraits)
			{
				string text2 = OverlayMission.GetAdmiralTraitText(current);
				if (current != admiralTraits.Last<AdmiralInfo.TraitType>())
				{
					text2 += ", ";
				}
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					base.ID,
					text,
					"admiralTraits"
				}), "", num, "");
				string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					base.ID,
					text,
					"admiralTraits"
				}), "", num, "");
				num++;
				this._app.UI.SetPropertyString(itemGlobalID, "text", text2);
				if (AdmiralInfo.IsGoodTrait(current))
				{
					this._app.UI.SetPropertyColorNormalized(itemGlobalID, "color", new Vector3(0f, 1f, 0f));
				}
				else
				{
					this._app.UI.SetPropertyColorNormalized(itemGlobalID, "color", new Vector3(1f, 0f, 0f));
				}
				this._app.UI.SetTooltip(itemGlobalID, AdmiralInfo.GetTraitDescription(current, this._app.GameDatabase.GetLevelForAdmiralTrait(admiralInfo.ID, current)));
				this._app.UI.SetPropertyString(itemGlobalID, "tooltip", AdmiralInfo.GetTraitDescription(current, this._app.GameDatabase.GetLevelForAdmiralTrait(admiralInfo.ID, current)));
			}
            string admiralAvatar = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(this._app, admiralInfo.ID);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				text,
				"admiralImage"
			}), "sprite", admiralAvatar);
			if (!admiralInfo.Engram && this._app.GameDatabase.PlayerHasTech(this._app.LocalPlayer.ID, "CCC_Personality_Engrams"))
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"createEngramButton"
				}), false);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					text,
					"createEngramButton"
				}), false);
			}
			string itemGlobalID2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
			{
				text,
				"infoList"
			}), "", 0, "");
			this._app.UI.Send(new object[]
			{
				"SetSelected",
				itemGlobalID2,
				true
			});
			string propertyValue;
			if (!admiralInfo.HomeworldID.HasValue)
			{
				propertyValue = "Born in Deep Space";
			}
			else
			{
				propertyValue = "Birth Planet: " + this._app.GameDatabase.GetOrbitalObjectInfo(admiralInfo.HomeworldID.Value).Name;
			}
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID2,
				"admiralBirthPlanet"
			}), "text", propertyValue);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID2,
				"admiralYears"
			}), "text", string.Format(App.Localize("@YEARS_AS_ADMIRAL"), ((this._app.GameDatabase.GetTurnCount() - admiralInfo.TurnCreated) / 4).ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID2,
				"admiralLoyalty"
			}), "text", string.Format(App.Localize("@ADMIRAL_LOYALTY"), admiralInfo.Loyalty.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID2,
				"admiralReaction"
			}), "text", string.Format(App.Localize("@ADMIRAL_REACTION"), admiralInfo.ReactionBonus.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID2,
				"admiralEvasion"
			}), "text", string.Format(App.Localize("@ADMIRAL_EVASION"), admiralInfo.EvasionBonus.ToString()));
			string itemGlobalID3 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
			{
				text,
				"infoList"
			}), "", 1, "");
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID3,
				"battlesWon"
			}), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_WON"), admiralInfo.BattlesWon.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID3,
				"battlesFought"
			}), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_FOUGHT"), admiralInfo.BattlesFought.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID3,
				"missionsAssigned"
			}), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ASSIGNED"), admiralInfo.MissionsAssigned.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID3,
				"missionsAccomplished"
			}), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ACCOMPLISHED"), admiralInfo.MissionsAccomplished.ToString()));
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(admiralInfo.PlayerID);
			if (playerInfo != null)
			{
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.idle.list_item.colony_insert.LC"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.idle.list_item.colony_insert.RC"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.idle.list_item.colony_insert.BG"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.mouse_over.list_item.colony_insert.LC"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.mouse_over.list_item.colony_insert.RC"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.mouse_over.list_item.colony_insert.BG"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.disabled.list_item.colony_insert.LC"
				}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.disabled.list_item.colony_insert.RC"
				}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_idle.disabled.list_item.colony_insert.BG"
				}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_sel.idle.list_item.colony_insert.LC"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_sel.idle.list_item.colony_insert.RC"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_sel.idle.list_item.colony_insert.BG"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_sel.mouse_over.list_item.colony_insert.LC"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_sel.mouse_over.list_item.colony_insert.RC"
				}), "color", playerInfo.PrimaryColor * 255f);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					text,
					"fleetitem.header_sel.mouse_over.list_item.colony_insert.BG"
				}), "color", playerInfo.PrimaryColor * 255f);
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okbtn")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				bool flag1 = msgType == "dialog_closed";
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
