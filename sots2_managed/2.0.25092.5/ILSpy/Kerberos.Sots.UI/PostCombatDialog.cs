using Kerberos.Sots.Data;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class PostCombatDialog : Dialog
	{
		private int _systemID;
		private int _turn;
		private int _combatID;
		public PostCombatDialog(App game, int systemID, int combatID, int turn, string template = "CombatSummaryDialog") : base(game, template)
		{
			this._turn = turn;
			this._systemID = systemID;
			this._combatID = combatID;
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "okButton")
			{
				this._app.UI.CloseDialog(this, true);
			}
		}
		public override void Initialize()
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player4 = combat.GetPlayer(this._app.LocalPlayer.ID);
			if (player4 == null)
			{
				this._app.UI.CloseDialog(this, true);
				return;
			}
			IEnumerable<PlayerInfo> playerInfos = this._app.GameDatabase.GetPlayerInfos();
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(combat.SystemID);
			string str = App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
			if (starSystemInfo != null)
			{
				str = starSystemInfo.Name;
			}
			this._app.UI.SetPropertyString("summaryText", "text", "Combat - " + player4.VictoryStatus.ToString() + " at " + str);
			int num = 0;
			int num2 = 0;
			float num3 = 0f;
			float num4 = 0f;
			double num5 = 0.0;
			double num6 = 0.0;
			float num7 = 0f;
			double num8 = 0.0;
			double num9 = 0.0;
			float num10 = 0f;
			int num11 = 0;
			int num12 = 0;
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
			foreach (PlayerInfo player in playerInfos)
			{
				PlayerCombatData player2 = combat.GetPlayer(player.ID);
				if (player2 != null)
				{
					DiplomacyInfo diplomacyInfo = this._app.GameDatabase.GetDiplomacyInfo(player4.PlayerID, player.ID);
					string itemGlobalID;
					if (diplomacyInfo.State == DiplomacyState.WAR)
					{
						this._app.UI.AddItem("enemiesAvatars", "", player.ID, "");
						itemGlobalID = this._app.UI.GetItemGlobalID("enemiesAvatars", "", player.ID, "");
					}
					else
					{
						this._app.UI.AddItem("alliesAvatars", "", player.ID, "");
						itemGlobalID = this._app.UI.GetItemGlobalID("alliesAvatars", "", player.ID, "");
					}
					PlayerSetup playerSetup = this._app.GameSetup.Players.FirstOrDefault((PlayerSetup x) => x.databaseId == player.ID);
					string name;
					if (playerSetup != null)
					{
						name = playerSetup.Name;
					}
					else
					{
						name = player.Name;
					}
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"name"
					}), "text", name);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"playeravatar"
					}), "texture", player.AvatarAssetPath);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"badge"
					}), "texture", player.BadgeAssetPath);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"primaryColor"
					}), "color", player.PrimaryColor);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"secondaryColor"
					}), "color", player.SecondaryColor);
					this._app.UI.AddItem("combatSummary", "", player.ID, "");
					string itemGlobalID2 = this._app.UI.GetItemGlobalID("combatSummary", "", player.ID, "");
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"itemName"
					}), "text", name);
					List<ShipData> shipData = player2.ShipData;
					List<WeaponData> weaponData = player2.WeaponData;
					List<PlanetData> planetData = player2.PlanetData;
					int count = shipData.Count;
					int num13 = 0;
					float num14 = 0f;
					float num15 = 0f;
					Dictionary<int, string> dictionary3 = new Dictionary<int, string>();
					foreach (ShipData current in shipData)
					{
						num13 += current.killCount;
						num14 += current.damageDealt;
						num15 += current.damageReceived;
						if (diplomacyInfo.State == DiplomacyState.WAR)
						{
							num12 += (current.destroyed ? 1 : 0);
						}
						else
						{
							num11 += (current.destroyed ? 1 : 0);
						}
						if (!dictionary3.ContainsKey(current.designID))
						{
							this._app.UI.AddItem(this._app.UI.Path(new string[]
							{
								itemGlobalID2,
								"shipList"
							}), "", current.designID, "");
							dictionary3.Add(current.designID, this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
							{
								itemGlobalID2,
								"shipList"
							}), "", current.designID, ""));
						}
					}
					foreach (WeaponData current2 in weaponData)
					{
						if (diplomacyInfo.State == DiplomacyState.WAR)
						{
							if (!dictionary2.ContainsKey(current2.weaponID))
							{
								dictionary2.Add(current2.weaponID, 0f);
							}
							Dictionary<int, float> dictionary4;
							int weaponID;
							(dictionary4 = dictionary2)[weaponID = current2.weaponID] = dictionary4[weaponID] + current2.damageDealt;
						}
						else
						{
							if (!dictionary.ContainsKey(current2.weaponID))
							{
								dictionary.Add(current2.weaponID, 0f);
							}
							Dictionary<int, float> dictionary4;
							int weaponID;
							(dictionary4 = dictionary)[weaponID = current2.weaponID] = dictionary4[weaponID] + current2.damageDealt;
						}
					}
					foreach (PlanetData current3 in planetData)
					{
						ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(current3.orbitalObjectID);
						if (colonyInfoForPlanet != null)
						{
							if (colonyInfoForPlanet.PlayerID != this._app.LocalPlayer.ID)
							{
								num5 += current3.imperialDamage;
								num6 += current3.civilianDamage.Sum((PopulationData x) => x.damage);
								num7 += current3.infrastructureDamage;
							}
							else
							{
								num8 += current3.imperialDamage;
								num9 += current3.civilianDamage.Sum((PopulationData x) => x.damage);
								num10 += current3.infrastructureDamage;
							}
						}
					}
					foreach (int des in dictionary3.Keys)
					{
						DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(des);
						int value = (
							from x in shipData
							where x.designID == des
							select x).Count<ShipData>();
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"stotalUnits"
						}), 0, count);
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"stotalUnits"
						}), value);
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"stotalUnitsLabel"
						}), "text", value.ToString());
						int value2 = (
							from x in shipData
							where x.designID == des
							select x).Sum((ShipData x) => x.killCount);
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdestroyedUnits"
						}), 0, num13);
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdestroyedUnits"
						}), value2);
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdestroyedUnitsLabel"
						}), "text", value2.ToString());
						float num16 = (
							from x in shipData
							where x.designID == des
							select x).Sum((ShipData x) => x.damageDealt);
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdamageInflicted"
						}), 0, (int)num14);
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdamageInflicted"
						}), (int)num16);
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdamageInflictedLabel"
						}), "text", num16.ToString("N0"));
						float num17 = (
							from x in shipData
							where x.designID == des
							select x).Sum((ShipData x) => x.damageReceived);
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdamageTaken"
						}), 0, (int)num15);
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdamageTaken"
						}), (int)num17);
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"sdamageTakenLabel"
						}), "text", num17.ToString("N0"));
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							dictionary3[des],
							"subitem_label"
						}), "text", designInfo.Name);
					}
					num = Math.Max(count, num);
					num2 = Math.Max(num13, num2);
					num3 = Math.Max(num14, num3);
					num4 = Math.Max(num15, num4);
					this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"totalUnits"
					}), 0, count);
					this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"totalUnits"
					}), count);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"totalUnitsLabel"
					}), "text", count.ToString());
					this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"destroyedUnits"
					}), 0, num13);
					this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"destroyedUnits"
					}), num13);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"destroyedUnitsLabel"
					}), "text", num13.ToString());
					this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"damageInflicted"
					}), 0, (int)num14);
					this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"damageInflicted"
					}), (int)num14);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"damageInflictedLabel"
					}), "text", num14.ToString("N0"));
					this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"damageTaken"
					}), 0, (int)num15);
					this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"damageTaken"
					}), (int)num15);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"damageTakenLabel"
					}), "text", num15.ToString("N0"));
				}
			}
			foreach (PlayerInfo current4 in playerInfos)
			{
				PlayerCombatData player3 = combat.GetPlayer(current4.ID);
				if (player3 != null)
				{
					string itemGlobalID3 = this._app.UI.GetItemGlobalID("combatSummary", "", current4.ID, "");
					this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
					{
						itemGlobalID3,
						"totalUnits"
					}), 0, num);
					this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
					{
						itemGlobalID3,
						"destroyedUnits"
					}), 0, num2);
					this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
					{
						itemGlobalID3,
						"damageInflicted"
					}), 0, (int)num3);
					this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
					{
						itemGlobalID3,
						"damageTaken"
					}), 0, (int)num4);
				}
			}
			foreach (int weapon in dictionary.Keys)
			{
				this._app.UI.AddItem("alliedWeaponList", "", weapon, "");
				string itemGlobalID4 = this._app.UI.GetItemGlobalID("alliedWeaponList", "", weapon, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID4,
					"damageDealt"
				}), "text", dictionary[weapon].ToString("N0"));
				LogicalWeapon logicalWeapon = this._app.AssetDatabase.Weapons.First((LogicalWeapon x) => x.UniqueWeaponID == weapon);
				string iconSpriteName = logicalWeapon.IconSpriteName;
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID4,
					"weaponIcon"
				}), "sprite", iconSpriteName);
				this._app.UI.SetPropertyString(itemGlobalID4, "tooltip", logicalWeapon.WeaponName);
			}
			foreach (int weapon in dictionary2.Keys)
			{
				this._app.UI.AddItem("enemyWeaponList", "", weapon, "");
				string itemGlobalID5 = this._app.UI.GetItemGlobalID("enemyWeaponList", "", weapon, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID5,
					"damageDealt"
				}), "text", dictionary2[weapon].ToString("N0"));
				LogicalWeapon logicalWeapon2 = this._app.AssetDatabase.Weapons.First((LogicalWeapon x) => x.UniqueWeaponID == weapon);
				string iconSpriteName2 = logicalWeapon2.IconSpriteName;
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID5,
					"weaponIcon"
				}), "sprite", iconSpriteName2);
			}
			int num18 = 0;
			string text = "";
			if (num9 > 0.0 && num8 > 0.0)
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					num8.ToString("N0"),
					" Imperialists and ",
					num9.ToString("N0"),
					" civilians lost. "
				});
			}
			else
			{
				if (num9 > 0.0)
				{
					text = text + num9.ToString("N0") + " civilians lost. ";
				}
				else
				{
					if (num8 > 0.0)
					{
						text = text + num8.ToString("N0") + " Imperialists lost. ";
					}
				}
			}
			if (num6 > 0.0 && num5 > 0.0)
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					num5.ToString("N0"),
					" enemy Imperialists and ",
					num6.ToString("N0"),
					" enemy civilians killed."
				});
			}
			else
			{
				if (num6 > 0.0)
				{
					text = text + num6.ToString("N0") + " enemy civilians killed.";
				}
				else
				{
					if (num5 > 0.0)
					{
						text = text + num5.ToString("N0") + " enemy Imperialists killed.";
					}
				}
			}
			if (text.Length > 0)
			{
				this._app.UI.AddItem("happenings", "", num18++, text);
			}
			string text3 = "";
			if ((double)num10 > 0.01)
			{
				text3 = text3 + "Infrastructure reduced by " + (num10 * 100f).ToString("#0.00") + "%";
			}
			if ((double)num7 > 0.01)
			{
				text3 = text3 + " Enemy infrastructure reduced by " + (num7 * 100f).ToString("#0.00") + "%";
			}
			if (text3.Length > 0)
			{
				this._app.UI.AddItem("happenings", "", num18++, text3);
			}
			string text4 = "";
			if (num11 > 0)
			{
				if (num11 > 1)
				{
					text4 = text4 + num11.ToString() + " friendly ships lost. ";
				}
				else
				{
					text4 = text4 + num11.ToString() + " friendly ship lost. ";
				}
			}
			if (num12 > 0)
			{
				if (num11 > 1)
				{
					text4 = text4 + num12.ToString() + " enemy ships destroyed.";
				}
				else
				{
					text4 = text4 + num12.ToString() + " enemy ship destroyed.";
				}
			}
			if (text4.Length > 0)
			{
				this._app.UI.AddItem("happenings", "", num18++, text4);
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
