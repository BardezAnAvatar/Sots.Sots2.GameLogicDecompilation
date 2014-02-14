using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI.Dialogs
{
	internal class DialogPostCombat : Dialog
	{
		private int _systemID;
		private int _turn;
		private int _combatID;
		private List<PlanetWidget> _PlayerSideplanetWidgets;
		private List<PlanetWidget> _EnemySideplanetWidgets;
		private List<int> EnemyPlayers;
		public DialogPostCombat(App game, int systemID, int combatID, int turn, string template = "postCombat") : base(game, template)
		{
			this._turn = turn;
			this._systemID = systemID;
			this._combatID = combatID;
			this._PlayerSideplanetWidgets = new List<PlanetWidget>();
			this._EnemySideplanetWidgets = new List<PlanetWidget>();
			this.EnemyPlayers = new List<int>();
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnOK")
				{
					this._app.UI.CloseDialog(this, true);
				}
				if (panelName.StartsWith("avatarButton"))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					if (array.Count<string>() == 3)
					{
						bool flag = array[1] == "A";
						int playerid = int.Parse(array[2]);
						if (flag)
						{
							this.SyncPlayerSide(playerid);
							return;
						}
						this.SyncEnemySide(playerid);
					}
				}
			}
		}
		public override void Initialize()
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player = combat.GetPlayer(this._app.LocalPlayer.ID);
			if (player == null)
			{
				this._app.UI.CloseDialog(this, true);
				return;
			}
			this._app.GameDatabase.GetPlayerInfos();
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(combat.SystemID);
			string str = App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
			if (starSystemInfo != null)
			{
				str = starSystemInfo.Name;
			}
			this._app.UI.SetPropertyString("title", "text", "Combat - " + player.VictoryStatus.ToString() + " at " + str);
			this.syncCombatantsLists();
			this.SyncPlayerSide(player.PlayerID);
			if (this.EnemyPlayers.Any<int>())
			{
				this.SyncEnemySide(this.EnemyPlayers.First<int>());
			}
			StarSystemMapUI.Sync(this._app, this._systemID, "systemMapContent", false);
		}
		public void syncCombatantsLists()
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player3 = combat.GetPlayer(this._app.LocalPlayer.ID);
			IEnumerable<PlayerInfo> playerInfos = this._app.GameDatabase.GetPlayerInfos();
			foreach (PlayerInfo player in playerInfos)
			{
				PlayerCombatData player2 = combat.GetPlayer(player.ID);
				if (player2 != null)
				{
					DiplomacyInfo diplomacyInfo = this._app.GameDatabase.GetDiplomacyInfo(player3.PlayerID, player.ID);
					string itemGlobalID;
					if (diplomacyInfo.State == DiplomacyState.WAR)
					{
						this._app.UI.AddItem("enemiesList", "", player.ID, "");
						itemGlobalID = this._app.UI.GetItemGlobalID("enemiesList", "", player.ID, "");
						this.EnemyPlayers.Add(player.ID);
					}
					else
					{
						this._app.UI.AddItem("alliesList", "", player.ID, "");
						itemGlobalID = this._app.UI.GetItemGlobalID("alliesList", "", player.ID, "");
					}
					PlayerSetup playerSetup = this._app.GameSetup.Players.FirstOrDefault((PlayerSetup x) => x.databaseId == player.ID);
					string name;
					if (playerSetup != null && playerSetup.Name != "")
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
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"avatarButton"
					}), "id", "avatarButton|" + ((diplomacyInfo.State == DiplomacyState.WAR) ? "E|" : "A|") + player.ID.ToString());
				}
			}
		}
		public void SyncPlayerSide(int playerid)
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player3 = combat.GetPlayer(this._app.LocalPlayer.ID);
			if (player3 == null)
			{
				this._app.UI.CloseDialog(this, true);
				return;
			}
			foreach (PlanetWidget current in this._PlayerSideplanetWidgets)
			{
				current.Terminate();
			}
			this._PlayerSideplanetWidgets.Clear();
			IEnumerable<PlayerInfo> playerInfos = this._app.GameDatabase.GetPlayerInfos();
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(combat.SystemID);
			App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
			if (starSystemInfo != null)
			{
				string arg_F8_0 = starSystemInfo.Name;
			}
			int val = 0;
			int val2 = 0;
			float val3 = 0f;
			float val4 = 0f;
			double num = 0.0;
			double num2 = 0.0;
			float num3 = 0f;
			int num4 = 0;
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			PlayerInfo player = playerInfos.FirstOrDefault((PlayerInfo x) => x.ID == playerid);
			PlayerCombatData player2 = combat.GetPlayer(player.ID);
			if (player2 != null)
			{
				this._app.GameDatabase.GetDiplomacyInfo(player3.PlayerID, player.ID);
				PlayerSetup playerSetup = this._app.GameSetup.Players.FirstOrDefault((PlayerSetup x) => x.databaseId == player.ID);
				string name;
				if (playerSetup != null && playerSetup.Name != "" && !playerSetup.AI)
				{
					name = playerSetup.Name;
				}
				else
				{
					name = player.Name;
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"playerSide",
					"alliesAvatars",
					"name"
				}), "text", name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"playerSide",
					"alliesAvatars",
					"playeravatar"
				}), "texture", player.AvatarAssetPath);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"playerSide",
					"alliesAvatars",
					"badge"
				}), "texture", player.BadgeAssetPath);
				this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
				{
					"playerSide",
					"alliesAvatars",
					"primaryColor"
				}), "color", player.PrimaryColor);
				this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
				{
					"playerSide",
					"alliesAvatars",
					"secondaryColor"
				}), "color", player.SecondaryColor);
				this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
				{
					"playerSide",
					"empireColor"
				}), "color", player.PrimaryColor);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"playerSide",
					"playerName"
				}), "text", name);
				List<ShipData> shipData = player2.ShipData;
				List<WeaponData> weaponData = player2.WeaponData;
				List<PlanetData> planetData = player2.PlanetData;
				int count = shipData.Count;
				int num5 = 0;
				float num6 = 0f;
				float num7 = 0f;
				new Dictionary<int, string>();
				int num8 = 0;
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					"playerSide",
					"fleetDamage"
				}));
				foreach (ShipData current2 in shipData)
				{
					num5 += current2.killCount;
					num6 += current2.damageDealt;
					num7 += current2.damageReceived;
					num4 += (current2.destroyed ? 1 : 0);
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(current2.designID);
					if (designInfo != null && designInfo.GetRealShipClass() != RealShipClasses.AssaultShuttle && designInfo.GetRealShipClass() != RealShipClasses.Drone && designInfo.GetRealShipClass() != RealShipClasses.BoardingPod && this._app.Game.ScriptModules.MeteorShower.PlayerID != player.ID)
					{
						string text = "";
						if (current2.destroyed)
						{
							text = designInfo.Name + " class ship has been destroyed.";
						}
						else
						{
							if (current2.damageReceived > 0f)
							{
								text = designInfo.Name + " class ship has been damaged.";
							}
						}
						if (text != "")
						{
							this._app.UI.AddItem(this._app.UI.Path(new string[]
							{
								"playerSide",
								"fleetDamage"
							}), "", num8, "");
							string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
							{
								"playerSide",
								"fleetDamage"
							}), "", num8, "");
							this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
							{
								itemGlobalID,
								"name"
							}), "text", text);
							num8++;
						}
					}
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"playerSide",
					"playerData",
					"playerScore"
				}), "text", (shipData.Count - num4).ToString() + "/" + shipData.Count.ToString());
				this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
				{
					"playerSide",
					"playerData",
					"assets"
				}), 0, shipData.Count);
				this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
				{
					"playerSide",
					"playerData",
					"assets"
				}), shipData.Count - num4);
				foreach (WeaponData current3 in weaponData)
				{
					if (!dictionary.ContainsKey(current3.weaponID))
					{
						dictionary.Add(current3.weaponID, 0f);
					}
					Dictionary<int, float> dictionary2;
					int weaponID;
					(dictionary2 = dictionary)[weaponID = current3.weaponID] = dictionary2[weaponID] + current3.damageDealt;
				}
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					"playerSide",
					"weaponDamage"
				}));
				int num9 = 0;
				int num10 = 0;
				string text2 = null;
				foreach (int weapon in dictionary.Keys)
				{
					if (num9 == 5 || text2 == null)
					{
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							"playerSide",
							"weaponDamage"
						}), "", num10, "");
						text2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							"playerSide",
							"weaponDamage"
						}), "", num10, "");
						num10++;
						num9 = 0;
						for (int i = 0; i < 5; i++)
						{
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								text2,
								"weapon" + i.ToString()
							}), false);
						}
					}
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text2,
						"weapon" + num9.ToString(),
						"damageDealt"
					}), "text", dictionary[weapon].ToString("N0"));
					LogicalWeapon logicalWeapon = this._app.AssetDatabase.Weapons.First((LogicalWeapon x) => x.UniqueWeaponID == weapon);
					string iconSpriteName = logicalWeapon.IconSpriteName;
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text2,
						"weapon" + num9.ToString(),
						"weaponIcon"
					}), "sprite", iconSpriteName);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text2,
						"weapon" + num9.ToString()
					}), "tooltip", logicalWeapon.WeaponName);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						text2,
						"weapon" + num9.ToString()
					}), true);
					num9++;
				}
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					"playerSide",
					"planetDamage"
				}));
				foreach (PlanetData current4 in planetData)
				{
					ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(current4.orbitalObjectID);
					if (colonyInfoForPlanet != null)
					{
						num += current4.imperialDamage;
						num2 += current4.civilianDamage.Sum((PopulationData x) => x.damage);
						num3 += current4.infrastructureDamage;
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							"playerSide",
							"planetDamage"
						}), "", colonyInfoForPlanet.ID, "");
						string itemGlobalID2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							"playerSide",
							"planetDamage"
						}), "", colonyInfoForPlanet.ID, "");
						OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfoForPlanet.OrbitalObjectID);
						PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(colonyInfoForPlanet.OrbitalObjectID);
						Faction faction = this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerFactionID(colonyInfoForPlanet.PlayerID));
						double civilianPopulation = this._app.GameDatabase.GetCivilianPopulation(colonyInfoForPlanet.OrbitalObjectID, faction.ID, faction.HasSlaves());
						float num11 = (planetInfo != null) ? planetInfo.Infrastructure : 0f;
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"planetName"
						}), "text", (orbitalObjectInfo != null) ? orbitalObjectInfo.Name : "?");
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"civslbl"
						}), "text", civilianPopulation.ToString("N0"));
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"implbl"
						}), "text", colonyInfoForPlanet.ImperialPop.ToString("N0"));
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"infralbl"
						}), "text", num11.ToString());
						double num12 = civilianPopulation + current4.civilianDamage.Sum((PopulationData x) => x.damage);
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"civAmount"
						}), 0, (int)num12);
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"civAmount"
						}), (int)(num12 - current4.civilianDamage.Sum((PopulationData x) => x.damage)));
						double num13 = colonyInfoForPlanet.ImperialPop + current4.imperialDamage;
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"impAmount"
						}), 0, (int)num13);
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"impAmount"
						}), (int)(num13 - current4.imperialDamage));
						float num14 = num11 + current4.infrastructureDamage;
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"infraAmount"
						}), 0, (int)(100f * num14));
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"infraAmount"
						}), (int)(100f * (num14 - current4.infrastructureDamage)));
						if (planetInfo != null)
						{
							this._PlayerSideplanetWidgets.Add(new PlanetWidget(this._app, itemGlobalID2));
							this._PlayerSideplanetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
						}
					}
				}
				val = Math.Max(count, val);
				val2 = Math.Max(num5, val2);
				val3 = Math.Max(num6, val3);
				val4 = Math.Max(num7, val4);
			}
			this._app.UI.AutoSizeContents(this._app.UI.Path(new string[]
			{
				base.ID,
				"playerSide"
			}));
		}
		protected override void OnUpdate()
		{
			foreach (PlanetWidget current in this._PlayerSideplanetWidgets)
			{
				current.Update();
			}
			foreach (PlanetWidget current2 in this._EnemySideplanetWidgets)
			{
				current2.Update();
			}
		}
		public void SyncEnemySide(int playerid)
		{
			CombatData combat = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, this._combatID, this._systemID, this._turn);
			PlayerCombatData player3 = combat.GetPlayer(this._app.LocalPlayer.ID);
			if (player3 == null)
			{
				this._app.UI.CloseDialog(this, true);
				return;
			}
			foreach (PlanetWidget current in this._EnemySideplanetWidgets)
			{
				current.Terminate();
			}
			this._EnemySideplanetWidgets.Clear();
			IEnumerable<PlayerInfo> playerInfos = this._app.GameDatabase.GetPlayerInfos();
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(combat.SystemID);
			App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE");
			if (starSystemInfo != null)
			{
				string arg_F8_0 = starSystemInfo.Name;
			}
			int val = 0;
			int val2 = 0;
			float val3 = 0f;
			float val4 = 0f;
			double num = 0.0;
			double num2 = 0.0;
			float num3 = 0f;
			int num4 = 0;
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			PlayerInfo player = playerInfos.FirstOrDefault((PlayerInfo x) => x.ID == playerid);
			PlayerCombatData player2 = combat.GetPlayer(player.ID);
			if (player2 != null)
			{
				this._app.GameDatabase.GetDiplomacyInfo(player3.PlayerID, player.ID);
				PlayerSetup playerSetup = this._app.GameSetup.Players.FirstOrDefault((PlayerSetup x) => x.databaseId == player.ID);
				string name;
				if (playerSetup != null && playerSetup.Name != "" && !playerSetup.AI)
				{
					name = playerSetup.Name;
				}
				else
				{
					name = player.Name;
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"enemySide",
					"alliesAvatars",
					"name"
				}), "text", name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"enemySide",
					"alliesAvatars",
					"playeravatar"
				}), "texture", player.AvatarAssetPath);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"enemySide",
					"alliesAvatars",
					"badge"
				}), "texture", player.BadgeAssetPath);
				this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
				{
					"enemySide",
					"alliesAvatars",
					"primaryColor"
				}), "color", player.PrimaryColor);
				this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
				{
					"enemySide",
					"alliesAvatars",
					"secondaryColor"
				}), "color", player.SecondaryColor);
				this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
				{
					"enemySide",
					"empireColor"
				}), "color", player.PrimaryColor);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"enemySide",
					"playerData",
					"playerName"
				}), "text", name);
				List<ShipData> shipData = player2.ShipData;
				List<WeaponData> weaponData = player2.WeaponData;
				List<PlanetData> planetData = player2.PlanetData;
				int count = shipData.Count;
				int num5 = 0;
				float num6 = 0f;
				float num7 = 0f;
				new Dictionary<int, string>();
				int num8 = 0;
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					"enemySide",
					"fleetDamage"
				}));
				foreach (ShipData current2 in shipData)
				{
					num5 += current2.killCount;
					num6 += current2.damageDealt;
					num7 += current2.damageReceived;
					num4 += (current2.destroyed ? 1 : 0);
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(current2.designID);
					if (designInfo != null && designInfo.GetRealShipClass() != RealShipClasses.AssaultShuttle && designInfo.GetRealShipClass() != RealShipClasses.Drone && designInfo.GetRealShipClass() != RealShipClasses.BoardingPod && this._app.Game.ScriptModules.MeteorShower.PlayerID != player.ID)
					{
						string text = "";
						if (current2.destroyed)
						{
							text = designInfo.Name + " class ship has been destroyed.";
						}
						else
						{
							if (current2.damageReceived > 0f)
							{
								text = designInfo.Name + " class ship has been damaged.";
							}
						}
						if (text != "")
						{
							this._app.UI.AddItem(this._app.UI.Path(new string[]
							{
								"enemySide",
								"fleetDamage"
							}), "", num8, "");
							string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
							{
								"enemySide",
								"fleetDamage"
							}), "", num8, "");
							this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
							{
								itemGlobalID,
								"name"
							}), "text", text);
							num8++;
						}
					}
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					"enemySide",
					"playerData",
					"playerScore"
				}), "text", (shipData.Count - num4).ToString() + "/" + shipData.Count.ToString());
				this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
				{
					"enemySide",
					"playerData",
					"assets"
				}), 0, shipData.Count);
				this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
				{
					"enemySide",
					"playerData",
					"assets"
				}), shipData.Count - num4);
				foreach (WeaponData current3 in weaponData)
				{
					if (!dictionary.ContainsKey(current3.weaponID))
					{
						dictionary.Add(current3.weaponID, 0f);
					}
					Dictionary<int, float> dictionary2;
					int weaponID;
					(dictionary2 = dictionary)[weaponID = current3.weaponID] = dictionary2[weaponID] + current3.damageDealt;
				}
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					"enemySide",
					"weaponDamage"
				}));
				int num9 = 0;
				int num10 = 0;
				string text2 = null;
				foreach (int weapon in dictionary.Keys)
				{
					if (num9 == 5 || text2 == null)
					{
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							"enemySide",
							"weaponDamage"
						}), "", num10, "");
						text2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							"enemySide",
							"weaponDamage"
						}), "", num10, "");
						num10++;
						num9 = 0;
						for (int i = 0; i < 5; i++)
						{
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								text2,
								"weapon" + i.ToString()
							}), false);
						}
					}
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text2,
						"weapon" + num9.ToString(),
						"damageDealt"
					}), "text", dictionary[weapon].ToString("N0"));
					LogicalWeapon logicalWeapon = this._app.AssetDatabase.Weapons.First((LogicalWeapon x) => x.UniqueWeaponID == weapon);
					string iconSpriteName = logicalWeapon.IconSpriteName;
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text2,
						"weapon" + num9.ToString(),
						"weaponIcon"
					}), "sprite", iconSpriteName);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text2,
						"weapon" + num9.ToString()
					}), "tooltip", logicalWeapon.WeaponName);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						text2,
						"weapon" + num9.ToString()
					}), true);
					num9++;
				}
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					"enemySide",
					"planetDamage"
				}));
				foreach (PlanetData current4 in planetData)
				{
					ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(current4.orbitalObjectID);
					if (colonyInfoForPlanet != null)
					{
						num += current4.imperialDamage;
						num2 += current4.civilianDamage.Sum((PopulationData x) => x.damage);
						num3 += current4.infrastructureDamage;
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							"enemySide",
							"planetDamage"
						}), "", colonyInfoForPlanet.ID, "");
						string itemGlobalID2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							"enemySide",
							"planetDamage"
						}), "", colonyInfoForPlanet.ID, "");
						OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(colonyInfoForPlanet.OrbitalObjectID);
						PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(colonyInfoForPlanet.OrbitalObjectID);
						Faction faction = this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerFactionID(colonyInfoForPlanet.PlayerID));
						double civilianPopulation = this._app.GameDatabase.GetCivilianPopulation(colonyInfoForPlanet.OrbitalObjectID, faction.ID, faction.HasSlaves());
						float num11 = (planetInfo != null) ? planetInfo.Infrastructure : 0f;
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"planetName"
						}), "text", (orbitalObjectInfo != null) ? orbitalObjectInfo.Name : "?");
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"civslbl"
						}), "text", civilianPopulation.ToString("N0"));
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"implbl"
						}), "text", colonyInfoForPlanet.ImperialPop.ToString("N0"));
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"infralbl"
						}), "text", num11.ToString());
						double num12 = civilianPopulation + current4.civilianDamage.Sum((PopulationData x) => x.damage);
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"civAmount"
						}), 0, (int)num12);
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"civAmount"
						}), (int)(num12 - current4.civilianDamage.Sum((PopulationData x) => x.damage)));
						double num13 = colonyInfoForPlanet.ImperialPop + current4.imperialDamage;
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"impAmount"
						}), 0, (int)num13);
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"impAmount"
						}), (int)(num13 - current4.imperialDamage));
						float num14 = num11 + current4.infrastructureDamage;
						this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"infraAmount"
						}), 0, (int)(100f * num14));
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"infraAmount"
						}), (int)(100f * (num14 - current4.infrastructureDamage)));
						if (planetInfo != null)
						{
							this._EnemySideplanetWidgets.Add(new PlanetWidget(this._app, itemGlobalID2));
							this._EnemySideplanetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
						}
					}
				}
				val = Math.Max(count, val);
				val2 = Math.Max(num5, val2);
				val3 = Math.Max(num6, val3);
				val4 = Math.Max(num7, val4);
			}
			this._app.UI.AutoSizeContents(this._app.UI.Path(new string[]
			{
				base.ID,
				"enemySide"
			}));
		}
		public override string[] CloseDialog()
		{
			foreach (PlanetWidget current in this._PlayerSideplanetWidgets)
			{
				current.Terminate();
			}
			this._PlayerSideplanetWidgets.Clear();
			foreach (PlanetWidget current2 in this._EnemySideplanetWidgets)
			{
				current2.Terminate();
			}
			this._EnemySideplanetWidgets.Clear();
			return null;
		}
	}
}
