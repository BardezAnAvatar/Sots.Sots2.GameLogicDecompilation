using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class AdmiralManagerDialog : Dialog
	{
		private enum AdmiralFilterMode
		{
			None,
			Age,
			Name,
			Location,
			Fleet
		}
		public const string OKButton = "okButton";
		public const string AdmiralList = "pnlAdmirals.admiralList";
		private int _playerID;
		private int _fleetToDissolve;
		private int _fleetToCancel;
		private int _currentSystemID;
		private int _currentAdmiralID;
		private int _currentShipID;
		private int _currentDesignID;
		private bool ComposeFleet;
		private List<int> _buttonMap = new List<int>();
		private string _confirmFleetDisolveDialog;
		private string _confirmCancelMissionDialog;
		private string _confirmCreateAdmiralEngram;
		private string _nameFleetDialog;
		private string _transfercubesDialog;
		private AdmiralManagerDialog.AdmiralFilterMode _currentFilterMode;
		public AdmiralManagerDialog(App game, int playerID, int targetSystem, bool composefleet = false, string template = "AdmiralManagerDialog") : base(game, template)
		{
			this._playerID = playerID;
			this._currentSystemID = targetSystem;
			this.ComposeFleet = composefleet;
		}
		public override void Initialize()
		{
			this.PopulateFilters();
			this.RefreshDisplay();
		}
		private void PopulateFilters()
		{
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"filterDropdown"
			}), "", 0, App.Localize("@ADMIRAL_DIALOG_NONE"));
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"filterDropdown"
			}), "", 1, App.Localize("@ADMIRAL_DIALOG_AGE"));
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"filterDropdown"
			}), "", 2, App.Localize("@ADMIRAL_DIALOG_NAME"));
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"filterDropdown"
			}), "", 3, App.Localize("@ADMIRAL_DIALOG_LOCATION"));
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"filterDropdown"
			}), "", 4, App.Localize("@ADMIRAL_DIALOG_FLEET"));
			this._app.UI.SetSelection(this._app.UI.Path(new string[]
			{
				base.ID,
				"filterDropdown"
			}), 0);
		}
		private string GetLocation(int admiralID)
		{
			string result = App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
			FleetInfo fleetInfoByAdmiralID = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiralID, FleetType.FL_NORMAL);
			if (fleetInfoByAdmiralID != null)
			{
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfoByAdmiralID.SystemID);
				if (starSystemInfo != null)
				{
					result = starSystemInfo.Name;
				}
			}
			return result;
		}
		private string GetFleetName(int admiralID)
		{
			string result = "zzzzzzzzzzzzzz";
			FleetInfo fleetInfoByAdmiralID = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiralID, FleetType.FL_NORMAL);
			if (fleetInfoByAdmiralID != null)
			{
				result = fleetInfoByAdmiralID.Name;
			}
			return result;
		}
		public void RefreshDisplay()
		{
			List<AdmiralInfo> list = this._app.GameDatabase.GetAdmiralInfosForPlayer(this._playerID).ToList<AdmiralInfo>();
			IEnumerable<SuulkaInfo> playerSuulkas = this._app.GameDatabase.GetPlayerSuulkas(new int?(this._playerID));
			foreach (SuulkaInfo current in playerSuulkas)
			{
				if (current.AdmiralID != 0)
				{
					list.Add(this._app.GameDatabase.GetAdmiralInfo(current.AdmiralID));
				}
			}
			List<AdmiralInfo> list2 = list.ToList<AdmiralInfo>();
			switch (this._currentFilterMode)
			{
			case AdmiralManagerDialog.AdmiralFilterMode.Age:
				list2 = (
					from x in list2
					orderby x.Age
					select x).ToList<AdmiralInfo>();
				break;
			case AdmiralManagerDialog.AdmiralFilterMode.Name:
				list2 = (
					from x in list2
					orderby x.Name
					select x).ToList<AdmiralInfo>();
				break;
			case AdmiralManagerDialog.AdmiralFilterMode.Location:
				list2 = (
					from x in list2
					orderby this.GetLocation(x.ID)
					select x).ToList<AdmiralInfo>();
				break;
			case AdmiralManagerDialog.AdmiralFilterMode.Fleet:
				list2 = (
					from x in list2
					orderby this.GetFleetName(x.ID)
					select x).ToList<AdmiralInfo>();
				break;
			}
			this._app.UI.ClearItemsTopLayer(this._app.UI.Path(new string[]
			{
				base.ID,
				"pnlAdmirals.admiralList"
			}));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"currentNumAdmirals"
			}), "text", string.Format(App.Localize("@NUMBER_OF_ADMIRALS"), list2.Count<AdmiralInfo>().ToString(), GameSession.GetPlayerMaxAdmirals(this._app.GameDatabase, this._playerID).ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"capturedAdmirals"
			}), "text", string.Format(App.Localize("@NUMBER_OF_CAPTURED_ADMIRALS"), list2.Count<AdmiralInfo>().ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"enemyAdmirals"
			}), "text", string.Format(App.Localize("@NUMBER_OF_ENEMY_ADMIRALS"), new object[0]));
			int num = 0;
			foreach (AdmiralInfo admiral in list2)
			{
				if (num + 1 > this._buttonMap.Count)
				{
					this._buttonMap.Add(admiral.ID);
				}
				else
				{
					this._buttonMap[num] = admiral.ID;
				}
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					base.ID,
					"pnlAdmirals.admiralList"
				}), "", admiral.ID, "");
				string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					base.ID,
					"pnlAdmirals.admiralList"
				}), "", admiral.ID, "");
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"fleetitem.expand_button"
				}), false);
				string arg = "Deep Space";
				if (admiral.HomeworldID.HasValue)
				{
					arg = this._app.GameDatabase.GetOrbitalObjectInfo(admiral.HomeworldID.Value).Name;
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"dissolveButton"
				}), "id", "dissolveButton" + num.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"cancelMissionButton"
				}), "id", "cancelMissionButton" + num.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"createFleetButton"
				}), "id", "createFleetButton" + num.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"createEngramButton"
				}), "id", "createEngramButton" + num.ToString());
				if (!admiral.Engram && this._app.GameDatabase.PlayerHasTech(this._app.LocalPlayer.ID, "CCC_Personality_Engrams"))
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"createEngramButton" + num.ToString()
					}), true);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"createEngramButton" + num.ToString()
					}), false);
				}
				FleetInfo fleetInfoByAdmiralID = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiral.ID, FleetType.FL_NORMAL);
				if (fleetInfoByAdmiralID != null)
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.expand_button"
					}), false);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.idle.list_item.listitem_name"
					}), "text", fleetInfoByAdmiralID.Name);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.mouse_over.list_item.listitem_name"
					}), "text", fleetInfoByAdmiralID.Name);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_sel.idle.list_item.listitem_name"
					}), "text", fleetInfoByAdmiralID.Name);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_sel.mouse_over.list_item.listitem_name"
					}), "text", fleetInfoByAdmiralID.Name);
					PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(fleetInfoByAdmiralID.PlayerID);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.idle.list_item.colony_insert.LC"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.idle.list_item.colony_insert.RC"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.idle.list_item.colony_insert.BG"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.mouse_over.list_item.colony_insert.LC"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.mouse_over.list_item.colony_insert.RC"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.mouse_over.list_item.colony_insert.BG"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.disabled.list_item.colony_insert.LC"
					}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.disabled.list_item.colony_insert.RC"
					}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_idle.disabled.list_item.colony_insert.BG"
					}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_sel.idle.list_item.colony_insert.LC"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_sel.idle.list_item.colony_insert.RC"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_sel.idle.list_item.colony_insert.BG"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_sel.mouse_over.list_item.colony_insert.LC"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_sel.mouse_over.list_item.colony_insert.RC"
					}), "color", playerInfo.PrimaryColor * 255f);
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.header_sel.mouse_over.list_item.colony_insert.BG"
					}), "color", playerInfo.PrimaryColor * 255f);
					StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfoByAdmiralID.SystemID);
					if (starSystemInfo != null)
					{
						arg = starSystemInfo.Name;
					}
					else
					{
						arg = "Deep Space";
					}
					MissionInfo missionByFleetID = this._app.GameDatabase.GetMissionByFleetID(fleetInfoByAdmiralID.ID);
					if (missionByFleetID != null)
					{
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"fleetitem.sub_title"
						}), "text", fleetInfoByAdmiralID.Name);
						this._app.UI.SetVisible(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"cancelMissionButton" + num.ToString()
						}), true);
					}
					else
					{
						this._app.UI.SetVisible(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"fleetitem.on_mission"
						}), false);
						this._app.UI.SetVisible(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"cancelMissionButton" + num.ToString()
						}), false);
					}
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"createFleetButton" + num.ToString()
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem"
					}), true);
					UICommChannel arg_EEA_0 = this._app.UI;
					string arg_EEA_1 = this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"dissolveButton" + num.ToString()
					});
					bool arg_EEA_2;
					if (missionByFleetID == null)
					{
						arg_EEA_2 = !playerSuulkas.Any((SuulkaInfo x) => x.AdmiralID == admiral.ID);
					}
					else
					{
						arg_EEA_2 = false;
					}
					arg_EEA_0.SetVisible(arg_EEA_1, arg_EEA_2);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem"
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"dissolveButton" + num.ToString()
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"createFleetButton" + num.ToString()
					}), !playerSuulkas.Any((SuulkaInfo x) => x.AdmiralID == admiral.ID));
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"admiralName"
				}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_NAME_COLON"), admiral.Name));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"admiralLocation"
				}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_LOCATION_COLON"), arg));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"admiralAge"
				}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_AGE_COLON"), ((int)admiral.Age).ToString()));
                string admiralAvatar = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(this._app, admiral.ID);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"admiralImage"
				}), "sprite", admiralAvatar);
				IEnumerable<AdmiralInfo.TraitType> admiralTraits = this._app.GameDatabase.GetAdmiralTraits(admiral.ID);
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"admiralTraits"
				}));
				int num2 = 0;
				foreach (AdmiralInfo.TraitType current2 in admiralTraits)
				{
					string text = OverlayMission.GetAdmiralTraitText(current2);
					if (current2 != admiralTraits.Last<AdmiralInfo.TraitType>())
					{
						text += ", ";
					}
					this._app.UI.AddItem(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"admiralTraits"
					}), "", num2, "");
					string itemGlobalID2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"admiralTraits"
					}), "", num2, "");
					num2++;
					this._app.UI.SetPropertyString(itemGlobalID2, "text", text);
					if (AdmiralInfo.IsGoodTrait(current2))
					{
						this._app.UI.SetPropertyColorNormalized(itemGlobalID2, "color", new Vector3(0f, 1f, 0f));
					}
					else
					{
						this._app.UI.SetPropertyColorNormalized(itemGlobalID2, "color", new Vector3(1f, 0f, 0f));
					}
					this._app.UI.SetTooltip(itemGlobalID2, AdmiralInfo.GetTraitDescription(current2, this._app.GameDatabase.GetLevelForAdmiralTrait(admiral.ID, current2)));
				}
				string itemGlobalID3 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"infoList"
				}), "", 0, "");
				this._app.UI.Send(new object[]
				{
					"SetSelected",
					itemGlobalID3,
					true
				});
				string propertyValue;
				if (!admiral.HomeworldID.HasValue)
				{
					propertyValue = App.Localize("@ADMIRAL_BORN_IN_SPACE");
				}
				else
				{
					propertyValue = string.Format(App.Localize("@ADMIRAL_BIRTHPLANET_X"), this._app.GameDatabase.GetOrbitalObjectInfo(admiral.HomeworldID.Value).Name);
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID3,
					"admiralBirthPlanet"
				}), "text", propertyValue);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID3,
					"admiralYears"
				}), "text", string.Format(App.Localize("@YEARS_AS_ADMIRAL"), ((this._app.GameDatabase.GetTurnCount() - admiral.TurnCreated) / 4).ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID3,
					"admiralLoyalty"
				}), "text", string.Format(App.Localize("@ADMIRAL_LOYALTY"), admiral.Loyalty.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID3,
					"admiralReaction"
				}), "text", string.Format(App.Localize("@ADMIRAL_REACTION"), admiral.ReactionBonus.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID3,
					"admiralEvasion"
				}), "text", string.Format(App.Localize("@ADMIRAL_EVASION"), admiral.EvasionBonus.ToString()));
				string itemGlobalID4 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"infoList"
				}), "", 1, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID4,
					"battlesWon"
				}), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_WON"), admiral.BattlesWon.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID4,
					"battlesFought"
				}), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_FOUGHT"), admiral.BattlesFought.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID4,
					"missionsAssigned"
				}), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ASSIGNED"), admiral.MissionsAssigned.ToString()));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID4,
					"missionsAccomplished"
				}), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ACCOMPLISHED"), admiral.MissionsAccomplished.ToString()));
				num++;
			}
		}
		public void RefreshAdmiral(int admiralID)
		{
			string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
			{
				base.ID,
				"pnlAdmirals.admiralList"
			}), "", admiralID, "");
			AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(admiralID);
			int num = this._buttonMap.IndexOf(admiralID);
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.expand_button"
			}), false);
			string arg = App.Localize("@ADMIRAL_IN_DEEP_SPACE");
			FleetInfo fleetInfoByAdmiralID = this._app.GameDatabase.GetFleetInfoByAdmiralID(admiralID, FleetType.FL_NORMAL);
			if (fleetInfoByAdmiralID != null)
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"fleetitem.expand_button"
				}), false);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"fleetitem.header_idle.idle.list_item.listitem_name"
				}), "text", fleetInfoByAdmiralID.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"fleetitem.header_idle.mouse_over.list_item.listitem_name"
				}), "text", fleetInfoByAdmiralID.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"fleetitem.header_sel.idle.list_item.listitem_name"
				}), "text", fleetInfoByAdmiralID.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
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
						itemGlobalID,
						"fleetitem.sub_title"
					}), "text", fleetInfoByAdmiralID.Name);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"cancelMissionButton" + num.ToString()
					}), true);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"fleetitem.on_mission"
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"cancelMissionButton" + num.ToString()
					}), false);
				}
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"createFleetButton" + num.ToString()
				}), false);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"fleetitem"
				}), true);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"dissolveButton" + num.ToString()
				}), missionByFleetID == null);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"fleetitem"
				}), false);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"dissolveButton" + num.ToString()
				}), false);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"createFleetButton" + num.ToString()
				}), true);
			}
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"admiralName"
			}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_NAME_COLON"), admiralInfo.Name));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"admiralLocation"
			}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_LOCATION_COLON"), arg));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"admiralAge"
			}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_AGE_COLON"), ((int)admiralInfo.Age).ToString()));
            string admiralAvatar = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(this._app, admiralInfo.ID);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"admiralImage"
			}), "sprite", admiralAvatar);
			if (!admiralInfo.Engram && this._app.GameDatabase.PlayerHasTech(this._app.LocalPlayer.ID, "CCC_Personality_Engrams"))
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"createEngramButton" + num.ToString()
				}), true);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"createEngramButton" + num.ToString()
				}), false);
			}
			IEnumerable<AdmiralInfo.TraitType> admiralTraits = this._app.GameDatabase.GetAdmiralTraits(admiralInfo.ID);
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"admiralTraits"
			}));
			int num2 = 0;
			foreach (AdmiralInfo.TraitType current in admiralTraits)
			{
				string text = OverlayMission.GetAdmiralTraitText(current);
				if (current != admiralTraits.Last<AdmiralInfo.TraitType>())
				{
					text += ", ";
				}
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"admiralTraits"
				}), "", num2, "");
				string itemGlobalID2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"admiralTraits"
				}), "", num2, "");
				num2++;
				this._app.UI.SetPropertyString(itemGlobalID2, "text", text);
				if (AdmiralInfo.IsGoodTrait(current))
				{
					this._app.UI.SetPropertyColorNormalized(itemGlobalID2, "color", new Vector3(0f, 1f, 0f));
				}
				else
				{
					this._app.UI.SetPropertyColorNormalized(itemGlobalID2, "color", new Vector3(1f, 0f, 0f));
				}
				this._app.UI.SetPropertyString(itemGlobalID2, "tooltip", AdmiralInfo.GetTraitDescription(current, this._app.GameDatabase.GetLevelForAdmiralTrait(admiralInfo.ID, current)));
			}
			string itemGlobalID3 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"infoList"
			}), "", 0, "");
			this._app.UI.Send(new object[]
			{
				"SetSelected",
				itemGlobalID3,
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
				itemGlobalID3,
				"admiralBirthPlanet"
			}), "text", propertyValue);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID3,
				"admiralYears"
			}), "text", string.Format(App.Localize("@YEARS_AS_ADMIRAL"), ((this._app.GameDatabase.GetTurnCount() - admiralInfo.TurnCreated) / 4).ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID3,
				"admiralLoyalty"
			}), "text", string.Format(App.Localize("@ADMIRAL_LOYALTY"), admiralInfo.Loyalty.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID3,
				"admiralReaction"
			}), "text", string.Format(App.Localize("@ADMIRAL_REACTION"), admiralInfo.ReactionBonus.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID3,
				"admiralEvasion"
			}), "text", string.Format(App.Localize("@ADMIRAL_EVASION"), admiralInfo.EvasionBonus.ToString()));
			string itemGlobalID4 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"infoList"
			}), "", 1, "");
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID4,
				"battlesWon"
			}), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_WON"), admiralInfo.BattlesWon.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID4,
				"battlesFought"
			}), "text", string.Format(App.Localize("@ADMIRAL_BATTLES_FOUGHT"), admiralInfo.BattlesFought.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID4,
				"missionsAssigned"
			}), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ASSIGNED"), admiralInfo.MissionsAssigned.ToString()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				itemGlobalID4,
				"missionsAccomplished"
			}), "text", string.Format(App.Localize("@ADMIRAL_MISSIONS_ACCOMPLISHED"), admiralInfo.MissionsAccomplished.ToString()));
			num++;
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(admiralInfo.PlayerID);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.idle.list_item.colony_insert.LC"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.idle.list_item.colony_insert.RC"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.idle.list_item.colony_insert.BG"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.mouse_over.list_item.colony_insert.LC"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.mouse_over.list_item.colony_insert.RC"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.mouse_over.list_item.colony_insert.BG"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.disabled.list_item.colony_insert.LC"
			}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.disabled.list_item.colony_insert.RC"
			}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_idle.disabled.list_item.colony_insert.BG"
			}), "color", playerInfo.PrimaryColor * 255f * 0.5f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_sel.idle.list_item.colony_insert.LC"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_sel.idle.list_item.colony_insert.RC"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_sel.idle.list_item.colony_insert.BG"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_sel.mouse_over.list_item.colony_insert.LC"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_sel.mouse_over.list_item.colony_insert.RC"
			}), "color", playerInfo.PrimaryColor * 255f);
			this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"fleetitem.header_sel.mouse_over.list_item.colony_insert.BG"
			}), "color", playerInfo.PrimaryColor * 255f);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName.StartsWith("createEngramButton"))
				{
					int index = int.Parse(panelName.Substring(18));
					this._currentAdmiralID = this._buttonMap[index];
					this._confirmCreateAdmiralEngram = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CREATE_ENGRAM_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_CREATE_ENGRAM_DESC"), this._app.GameDatabase.GetAdmiralInfo(this._currentAdmiralID).Name), "dialogGenericQuestion"), null);
					return;
				}
				if (panelName.StartsWith("cancelMissionButton"))
				{
					int index2 = int.Parse(panelName.Substring(19));
					this._fleetToCancel = this._app.GameDatabase.GetFleetInfoByAdmiralID(this._buttonMap[index2], FleetType.FL_NORMAL).ID;
					this._currentAdmiralID = this._buttonMap[index2];
					this._confirmCancelMissionDialog = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CANCELMISSION_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_CANCELMISSION_DESC"), this._app.GameDatabase.GetFleetInfo(this._fleetToCancel).Name), "dialogGenericQuestion"), null);
					return;
				}
				if (panelName.StartsWith("dissolveButton"))
				{
					int index3 = int.Parse(panelName.Substring(14));
					this._fleetToDissolve = this._app.GameDatabase.GetFleetInfoByAdmiralID(this._buttonMap[index3], FleetType.FL_NORMAL).ID;
					this._currentAdmiralID = this._buttonMap[index3];
                    if (this._app.GameDatabase.GetMissionByFleetID(this._fleetToDissolve) == null && !Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this._app.GameDatabase, this._app.GameDatabase.GetFleetInfo(this._fleetToDissolve)))
					{
						this._confirmFleetDisolveDialog = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@UI_FLEET_DIALOG_DISSOLVEFLEET_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_DISSOLVEFLEET_DESC"), this._app.GameDatabase.GetFleetInfo(this._fleetToDissolve).Name), "dialogGenericQuestion"), null);
						return;
					}
					this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CANNOTDISSOLVE_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOTDISSOLVE_DESC"), "dialogGenericMessage"), null);
					return;
				}
				else
				{
					if (panelName.StartsWith("createFleetButton"))
					{
						int index4 = int.Parse(panelName.Substring(17));
						AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(this._buttonMap[index4]);
						ShipInfo shipInfo = null;
						DesignInfo designInfo = null;
						int? reserveFleetID = this._app.GameDatabase.GetReserveFleetID(this._playerID, this._currentSystemID);
						if (reserveFleetID.HasValue)
						{
							if (this._app.LocalPlayer.Faction.Name == "loa")
							{
                                Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, reserveFleetID.Value);
                                int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, reserveFleetID.Value);
								designInfo = null;
								List<DesignInfo> list = (
									from x in this._app.GameDatabase.GetDesignInfosForPlayer(this._playerID)
									where x.Class == ShipClass.Cruiser && x.GetCommandPoints() > 0
									select x).ToList<DesignInfo>();
								foreach (DesignInfo current in list)
								{
									if (designInfo == null)
									{
										designInfo = current;
									}
									else
									{
										if (designInfo.ProductionCost > current.ProductionCost)
										{
											designInfo = current;
										}
									}
								}
								if (designInfo != null && designInfo.ProductionCost > fleetLoaCubeValue)
								{
									designInfo = null;
								}
							}
							else
							{
								IEnumerable<ShipInfo> shipInfoByFleetID = this._app.GameDatabase.GetShipInfoByFleetID(reserveFleetID.Value, false);
								foreach (ShipInfo current2 in shipInfoByFleetID)
								{
									if (this._app.GameDatabase.GetShipCommandPointQuota(current2.ID) > 0)
									{
										shipInfo = current2;
										break;
									}
								}
							}
						}
						if (shipInfo == null && designInfo == null)
						{
							this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_DESC"), "dialogGenericMessage"), null);
							return;
						}
						this._currentAdmiralID = admiralInfo.ID;
						if (shipInfo != null)
						{
							this._currentShipID = shipInfo.ID;
						}
						if (designInfo != null)
						{
							this._currentDesignID = designInfo.ID;
						}
						this._nameFleetDialog = this._app.UI.CreateDialog(new GenericTextEntryDialog(this._app, App.Localize("@UI_FLEET_DIALOG_FLEETNAME_TITLE"), App.Localize("@UI_FLEET_DIALOG_FLEETNAME_DESC"), this._app.GameDatabase.ResolveNewFleetName(this._app, this._playerID, this._app.Game.NamesPool.GetFleetName(this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._playerID)))), 24, 1, true, EditBoxFilterMode.None), null);
						return;
					}
				}
			}
			else
			{
				if (msgType == "dialog_closed")
				{
					if (panelName == this._confirmCancelMissionDialog)
					{
						if (bool.Parse(msgParams[0]))
						{
							FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._fleetToCancel);
							AdmiralInfo admiralInfo2 = this._app.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
							string cueName = string.Format("STRAT_008-01_{0}_{1}UniversalMissionNegation", this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)), admiralInfo2.GetAdmiralSoundCueContext(this._app.AssetDatabase));
							this._app.PostRequestSpeech(cueName, 50, 120, 0f);
                            Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._app.Game, fleetInfo, true);
							return;
						}
					}
					else
					{
						if (panelName == this._confirmCreateAdmiralEngram)
						{
							if (bool.Parse(msgParams[0]))
							{
								AdmiralInfo admiralInfo3 = this._app.GameDatabase.GetAdmiralInfo(this._currentAdmiralID);
								if (admiralInfo3 != null)
								{
									this._app.GameDatabase.UpdateEngram(admiralInfo3.ID, true);
								}
								this.RefreshAdmiral(this._currentAdmiralID);
								return;
							}
						}
						else
						{
							if (panelName == this._confirmFleetDisolveDialog)
							{
								if (bool.Parse(msgParams[0]))
								{
									FleetInfo fleetInfo2 = this._app.GameDatabase.GetFleetInfo(this._fleetToDissolve);
									int? reserveFleetID2 = this._app.GameDatabase.GetReserveFleetID(this._playerID, fleetInfo2.SystemID);
									if (reserveFleetID2.HasValue)
									{
										IEnumerable<ShipInfo> enumerable = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo2.ID, false).ToList<ShipInfo>();
										foreach (ShipInfo current3 in enumerable)
										{
											this._app.GameDatabase.TransferShip(current3.ID, reserveFleetID2.Value);
										}
										this._app.GameDatabase.RemoveFleet(this._fleetToDissolve);
										this.RefreshAdmiral(this._currentAdmiralID);
										return;
									}
								}
							}
							else
							{
								if (panelName == this._nameFleetDialog)
								{
									if (bool.Parse(msgParams[0]))
									{
										int num = this._app.GameDatabase.InsertFleet(this._playerID, this._currentAdmiralID, this._currentSystemID, this._currentSystemID, this._app.GameDatabase.ResolveNewFleetName(this._app, this._playerID, msgParams[1]), FleetType.FL_NORMAL);
										if (this._app.LocalPlayer.Faction.Name == "loa")
										{
											FleetInfo fleetInfo3 = this._app.GameDatabase.GetFleetsByPlayerAndSystem(this._playerID, this._currentSystemID, FleetType.FL_RESERVE).First<FleetInfo>();
											if (fleetInfo3 != null)
											{
												ShipInfo shipInfo2 = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo3.ID, true).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
												DesignInfo designInfo2 = this._app.GameDatabase.GetDesignInfo(this._currentDesignID);
												if (shipInfo2 != null && designInfo2 != null)
												{
													this._transfercubesDialog = this._app.UI.CreateDialog(new DialogLoaShipTransfer(this._app, num, fleetInfo3.ID, shipInfo2.ID, designInfo2.ProductionCost), null);
												}
											}
										}
										else
										{
											this._app.GameDatabase.TransferShip(this._currentShipID, num);
											this._app.UI.CreateDialog(new FleetCompositorDialog(this._app, this._currentSystemID, num, "dialogFleetCompositor"), null);
										}
										this.RefreshAdmiral(this._currentAdmiralID);
										return;
									}
								}
								else
								{
									if (panelName == this._transfercubesDialog && msgParams.Count<string>() == 4)
									{
										int fleetID = int.Parse(msgParams[0]);
										int.Parse(msgParams[1]);
										int shipID = int.Parse(msgParams[2]);
										int num2 = int.Parse(msgParams[3]);
										ShipInfo shipInfo3 = this._app.GameDatabase.GetShipInfo(shipID, true);
										ShipInfo shipInfo4 = this._app.GameDatabase.GetShipInfoByFleetID(fleetID, false).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
										if (shipInfo4 == null)
										{
											this._app.GameDatabase.InsertShip(fleetID, shipInfo3.DesignInfo.ID, "Cube", (ShipParams)0, null, num2);
										}
										else
										{
											this._app.GameDatabase.UpdateShipLoaCubes(shipInfo4.ID, shipInfo4.LoaCubes + num2);
										}
										if (shipInfo3.LoaCubes <= num2)
										{
											this._app.GameDatabase.RemoveShip(shipInfo3.ID);
										}
										else
										{
											this._app.GameDatabase.UpdateShipLoaCubes(shipInfo3.ID, shipInfo3.LoaCubes - num2);
										}
										if (this.ComposeFleet)
										{
											if (this._app.LocalPlayer.Faction.Name != "loa")
											{
												this._app.UI.CreateDialog(new FleetCompositorDialog(this._app, this._currentSystemID, fleetID, "dialogFleetCompositor"), null);
											}
											else
											{
												if (this._app.CurrentState == this._app.GetGameState<StarMapState>())
												{
													this._app.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
												}
												if (this._app.CurrentState == this._app.GetGameState<FleetManagerState>())
												{
													this._app.GetGameState<FleetManagerState>().Refresh();
												}
											}
											this._app.UI.CloseDialog(this, true);
											return;
										}
									}
								}
							}
						}
					}
				}
				else
				{
					if (msgType == "list_sel_changed" && panelName == "filterDropdown")
					{
						int currentFilterMode = int.Parse(msgParams[0]);
						this._currentFilterMode = (AdmiralManagerDialog.AdmiralFilterMode)currentFilterMode;
						this.RefreshDisplay();
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
