using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class StarMapLobbyState : GameState
	{
		private const int PLAYER_UPDATE_INTERVAL = 60;
		private const string DebugTestCombatButton = "debugTestCombatButton";
		private const string UIEmpireBar = "gameEmpireBar";
		public const string UIProvinceSelectDetailsPanel = "gameProvinceSelectDetails";
		private const string UIColonyDetailsWidget = "colonyDetailsWidget";
		private const string UIExitButton = "gameExitButton";
		private const string UIOptionsButton = "gameOptionsButton";
		private const string UISaveGameButton = "gameSaveGameButton";
		private const string UIEndTurnButton = "gameEndTurnButton";
		private const string UIEmpireSummaryButton = "gameEmpireSummaryButton";
		private const string UIResearchButton = "gameResearchButton";
		private const string UIDiplomacyButton = "gameDiplomacyButton";
		private const string UIDesignButton = "gameDesignButton";
		private const string UIBuildButton = "gameBuildButton";
		private const string UISystemButton = "gameSystemButton";
		private const string UISotspediaButton = "gameSotspediaButton";
		private const string UIProvinceModeButton = "gameProvinceModeButton";
		public const string UIPlayerList = "lstPlayers";
		private const string UISurveyButton = "gameSurveyButton";
		private const string UIColonizeButton = "gameColonizeButton";
		private const string UIRelocateButton = "gameRelocateButton";
		private const string UIPatrolButton = "gamePatrolButton";
		private const string UIInterdictButton = "gameInterdictButton";
		private const string UIStrikeButton = "gameStrikeButton";
		private const string UIInvadeButton = "gameInvadeButton";
		private const string UIConstructStationButton = "gameConstructStationButton";
		private const string UIStationManagerButton = "gameStationManagerButton";
		private const string UIFleetManagerButton = "gameFleetManagerButton";
		private const string UIDefenseManagerButton = "gameDefenseManagerButton";
		private LobbyEntranceState _enterState;
		private GameObjectSet _crits;
		private ArrowPainter _painter;
		private Sky _sky;
		private StarMapLobby _starmap;
		private List<ServerInfo> _servers = new List<ServerInfo>();
		private List<Vector3> _serverPositions;
		private OrbitCameraController _camera;
		private Random _rand = new Random();
		private int _selectedPlanet;
		private static int _selectedIndex = -1;
		private GameState _previousState;
		private string _contextMenuID;
		private string _loginGUID = "";
		private string _empireGUID = "";
		private string _createGameGUID = "";
		private string _directConnectGUID = "";
		private string _passwordGUID = "";
		private Dictionary<int, bool> _settingsDirty = new Dictionary<int, bool>();
		private int _frameCount;
		private int _numPlayerSlots;
		private int _selectedSlot;
		private int _contextSlot;
		private TreasurySlider _playerInitialTreasurySlider;
		private ValueBoundSpinner _playerInitialTechnologiesSpinner;
		private ValueBoundSpinner _playerInitialSystemsSpinner;
		private ShipHoloView _shipHoloView;
		private ShipBuilder _builder;
		private OrbitCameraController _shipCamera;
		private GameObjectSet _shipCrits;
		private Player _tempPlayer;
		private PlayerInfo _tempPlayerInfo;
		private bool _dlgSelectEmpireColorVisible;
		private bool _dlgSelectShipColorVisible;
		private bool _inPlayerSetup;
		private bool _betaDisable;
		private bool _refreshing;
		private static string _lastIPTyped = "";
		public int SelectedServer
		{
			get
			{
				return StarMapLobbyState._selectedIndex;
			}
			set
			{
				StarMapLobbyState._selectedIndex = value;
			}
		}
		private int SelectedPlanet
		{
			get
			{
				return this._selectedPlanet;
			}
			set
			{
				this._selectedPlanet = value;
			}
		}
		public int SelectedSlot
		{
			get
			{
				return this._selectedSlot;
			}
			private set
			{
				if (this._selectedSlot != value)
				{
					this.SetSelectedSlot(value);
				}
			}
		}
		private int NumPlayerSlots
		{
			get
			{
				return this._numPlayerSlots;
			}
			set
			{
				if (value == this._numPlayerSlots)
				{
					return;
				}
				this._numPlayerSlots = value;
				base.App.GameSetup.SetPlayerCount(this._numPlayerSlots);
			}
		}
		private string SelectedAvatar
		{
			get;
			set;
		}
		private string SelectedBadge
		{
			get;
			set;
		}
		private string SelectedFaction
		{
			get;
			set;
		}
		private int SelectedSubfactionIndex
		{
			get;
			set;
		}
		public bool InPlayerSetup
		{
			get
			{
				return this._inPlayerSetup;
			}
			set
			{
				this._inPlayerSetup = value;
			}
		}
		public int GameInProgressTurn
		{
			get;
			set;
		}
		public bool IsLocalPlayerReady
		{
			get;
			set;
		}
		private void SelectBadge(string badgeItemId)
		{
			if (!string.IsNullOrEmpty(this.SelectedFaction))
			{
				this.SelectedBadge = base.App.GameSetup.SetBadge(this.SelectedSlot, this.SelectedFaction, badgeItemId);
				if (!string.IsNullOrEmpty(this.SelectedBadge))
				{
					this._tempPlayerInfo.BadgeAssetPath = Path.Combine("factions", this.SelectedFaction, "badges", this.SelectedBadge + ".tga");
				}
				this.UpdateShipPreview(StarMapLobbyState._selectedIndex);
				return;
			}
			this._tempPlayerInfo.BadgeAssetPath = string.Empty;
		}
		private void SelectAvatar(string avatarItemId)
		{
			if (!string.IsNullOrEmpty(this.SelectedFaction))
			{
				this.SelectedAvatar = base.App.GameSetup.SetAvatar(this.SelectedSlot, this.SelectedFaction, avatarItemId);
				if (!string.IsNullOrEmpty(this.SelectedAvatar))
				{
					this._tempPlayerInfo.AvatarAssetPath = Path.Combine("factions", this.SelectedFaction, "avatars", this.SelectedAvatar + ".tga");
					return;
				}
			}
			else
			{
				this._tempPlayerInfo.AvatarAssetPath = string.Empty;
			}
		}
		private void SelectFaction(string factionItemId, int subfactionIndex)
		{
			if (base.App.GameSetup.Players[this.SelectedSlot].Faction == factionItemId && base.App.GameSetup.Players[this.SelectedSlot].SubfactionIndex == subfactionIndex)
			{
				return;
			}
			this.SelectedFaction = factionItemId;
			this.SelectedSubfactionIndex = subfactionIndex;
			base.App.GameSetup.SetBadge(this.SelectedSlot, base.App.GameSetup.Players[this.SelectedSlot].Faction, null);
			base.App.GameSetup.SetAvatar(this.SelectedSlot, base.App.GameSetup.Players[this.SelectedSlot].Faction, null);
			base.App.GameSetup.Players[this.SelectedSlot].Faction = this.SelectedFaction;
			base.App.GameSetup.Players[this.SelectedSlot].SubfactionIndex = this.SelectedSubfactionIndex;
		}
		private void SelectDifficulty(string difficultyItemId)
		{
			base.App.GameSetup.SetDifficulty(this.SelectedSlot, difficultyItemId);
		}
		private void SetEmpireColor(int slot, int? empireColorId)
		{
			base.App.GameSetup.SetEmpireColor(slot, empireColorId);
			if (empireColorId.HasValue)
			{
				this._tempPlayerInfo.PrimaryColor = Player.DefaultPrimaryPlayerColors[empireColorId.Value];
				if (this._tempPlayer != null)
				{
					this._tempPlayer.SetEmpireColor(empireColorId.Value);
				}
				base.App.UI.SetPropertyColor("imgEmpireColor", "color", this._tempPlayerInfo.PrimaryColor * 255f);
			}
		}
		private void SetShipColor(int slot, Vector3 shipColor, bool setColorSample = true)
		{
			if (shipColor != base.App.GameSetup.Players[slot].ShipColor)
			{
				this._settingsDirty[slot] = true;
			}
			base.App.GameSetup.SetShipColor(slot, shipColor);
			this._tempPlayerInfo.SecondaryColor = shipColor;
			if (this._tempPlayer != null)
			{
				this._tempPlayer.SetPlayerColor(this._tempPlayerInfo.SecondaryColor);
			}
			if (setColorSample)
			{
				base.App.UI.SetPropertyColor("pickerSecondaryColor", "color", this._tempPlayerInfo.SecondaryColor * 255f);
			}
		}
		private void SetSelectedSlot(int value)
		{
			this._selectedSlot = value;
			PlayerSetup playerSetup = base.App.GameSetup.Players[this._selectedSlot];
			this.EnablePlayerSetup(!playerSetup.Fixed);
			this._playerInitialSystemsSpinner.SetValue((double)playerSetup.InitialColonies);
			this._playerInitialTechnologiesSpinner.SetValue((double)playerSetup.InitialTechs);
			this._playerInitialTreasurySlider.SetValue(playerSetup.InitialTreasury);
			this.SelectedFaction = base.App.GameSetup.Players[value].Faction;
			this.SelectedSubfactionIndex = base.App.GameSetup.Players[value].SubfactionIndex;
			this.SelectedAvatar = base.App.GameSetup.Players[value].Avatar;
			this.SelectedBadge = base.App.GameSetup.Players[value].Badge;
			bool flag = !base.App.GameSetup.IsMultiplayer || base.App.Network.IsHosting;
			this._playerInitialSystemsSpinner.SetEnabled(flag && !playerSetup.Fixed);
			this._playerInitialTechnologiesSpinner.SetEnabled(flag && !playerSetup.Fixed);
			this._playerInitialTreasurySlider.SetEnabled(flag && !playerSetup.Fixed);
		}
		protected void UpdateShipColors(int index)
		{
			if (base.App.GameSetup.Players.Count <= index || index < 0)
			{
				return;
			}
			this.SetEmpireColor(index, base.App.GameSetup.Players[index].EmpireColor);
			this.SetShipColor(index, base.App.GameSetup.Players[index].ShipColor, true);
		}
		protected void OnPlayerChanged(int slot, bool updateShip = true, bool rebuildPlayerList = true)
		{
			GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", base.App.GameSetup.Players, rebuildPlayerList);
			GameSetupUI.SyncPlayerSetupWidget(base.App, "pnlPlayerSetup", base.App.GameSetup.Players[slot]);
			if (updateShip)
			{
				this.UpdateShipPreview(slot);
			}
			if (base.App.GameSetup.IsMultiplayer)
			{
				base.App.Network.SetPlayerInfo(base.App.GameSetup.Players[slot], slot);
			}
		}
		protected void DefaultPlayer(int iPlayer, bool isScenario)
		{
			PlayerSetup playerSetup = base.App.GameSetup.Players[iPlayer];
			if (!isScenario)
			{
				playerSetup.InitialColonies = base.App.GameSetup.InitialSystems;
				playerSetup.InitialTechs = base.App.GameSetup.InitialTechnologies;
				playerSetup.InitialTreasury = base.App.GameSetup.InitialTreasury;
			}
			Faction faction = base.App.AssetDatabase.GetFaction(playerSetup.Faction);
			if (faction == null || !base.App.GameSetup.AvailablePlayerFeatures.Factions.ContainsKey(faction))
			{
				List<string> list = (
					from x in base.App.GameSetup.AvailablePlayerFeatures.Factions.Keys
					select x.Name).ToList<string>();
				Vector3 shipColor;
				shipColor.X = App.GetSafeRandom().NextSingle();
				shipColor.Y = App.GetSafeRandom().NextSingle();
				shipColor.Z = App.GetSafeRandom().NextSingle();
				this.SetShipColor(iPlayer, shipColor, true);
				playerSetup.Faction = list[App.GetSafeRandom().Next(list.Count)];
				playerSetup.Badge = null;
				playerSetup.Avatar = null;
				this.SetEmpireColor(iPlayer, null);
			}
			else
			{
				base.App.GameSetup.SetEmpireColor(iPlayer, playerSetup.EmpireColor);
				base.App.GameSetup.SetAvatar(iPlayer, playerSetup.Faction, playerSetup.Avatar);
			}
			this.UpdateShipColors(iPlayer);
		}
		public StarMapLobbyState(App game) : base(game)
		{
		}
		private List<Vector3> GenerateMeASpiralGalaxyPlease()
		{
			List<float> list = new List<float>();
			List<Vector3> list2 = new List<Vector3>();
			int num = 12;
			float num2 = 0f;
			float num3 = 360f / (float)num;
			for (int i = 0; i < num; i++)
			{
				list.Add(num2 + (float)this._rand.Next(0, (int)num3));
				num2 += num3;
			}
			float num4 = 0.5f;
			float num5 = 0.05f;
			float num6 = 2f;
			float num7 = 3.5f;
			float num8 = 300f;
			float num9 = 0f;
			float num10 = 0f;
			int num11 = (int)Math.Floor((double)((num8 - num6) / num7));
			for (int j = 0; j < num11; j++)
			{
				for (int k = 0; k < num; k++)
				{
					float num12 = list[k];
					float num13 = (float)this._rand.Next(0, (int)num3);
					float num14 = num12 + num13 / 2f;
					float x = num9 + (float)Math.Cos((double)(num14 * 0.0174444448f)) * num6;
					float z = num10 + (float)(-(float)Math.Sin((double)(num14 * 0.0174444448f))) * num6;
					Vector3 vector = new Vector3(x, 0f, z);
					float num15 = 5f;
					bool flag = false;
					for (int l = 0; l < list2.Count<Vector3>(); l++)
					{
						float length = (vector - list2[l]).Length;
						if (length < num15)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						list2.Add(vector);
					}
					num12 = (float)((int)(num12 + num4) % 360);
					bool flag2 = this._rand.Next(0, 100) == 0;
					if (flag2)
					{
						float num16 = this._rand.NextSingle() * 5f;
						num12 = (float)((int)(num12 + num16) % 360);
					}
					list[k] = num12;
				}
				num4 += num5;
				num6 += num7;
			}
			return list2;
		}
		public void AddServer(ulong id, string name, string map, string version, int players, int maxPlayers, int ping, bool passworded, List<PlayerSetup> playerInfo)
		{
			ServerInfo serverInfo = this._servers.FirstOrDefault((ServerInfo x) => x.serverID == id);
			if (serverInfo == null)
			{
				this._servers.Add(new ServerInfo());
				serverInfo = this._servers.Last<ServerInfo>();
			}
			int num = this._servers.Count - 1;
			serverInfo.name = name;
			serverInfo.map = map;
			serverInfo.version = version;
			serverInfo.players = players;
			serverInfo.maxPlayers = maxPlayers;
			serverInfo.ping = ping;
			serverInfo.serverID = id;
			serverInfo.ID = num;
			serverInfo.passworded = passworded;
			serverInfo.playerInfo = playerInfo;
			App.Log.Trace(string.Concat(new object[]
			{
				"Adding server, ",
				serverInfo.name,
				", to lobby @ pos: ",
				id
			}), "net");
			Vector3 origin = this._serverPositions[num] * 15f;
			origin.Y = this._rand.NextSingle() * 3f;
			serverInfo.Origin = origin;
			this._starmap.AddServer(this._crits, serverInfo);
			if (this._servers.Count < 2)
			{
				this.FocusOnServer(num);
			}
			if (this.SelectedServer == num && playerInfo.Count<PlayerSetup>() > 0 && !base.App.Network.IsJoined && !base.App.Network.IsHosting)
			{
				GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", playerInfo, true);
			}
		}
		protected void FocusOnServer(int index)
		{
			StarMapServer target = null;
			if (this._starmap.Servers.Reverse.TryGetValue(index, out target))
			{
				this._starmap.SetFocus(target, 100f);
			}
		}
		protected void SetGameDetails(int serverId)
		{
			ServerInfo serverInfo = this._servers.FirstOrDefault((ServerInfo x) => x.ID == serverId);
			if (serverInfo != null)
			{
				base.App.UI.ClearItems("lstGameSettings");
				base.App.UI.AddItem("lstGameSettings", "property", 0, "Map:");
				base.App.UI.SetItemText("lstGameSettings", "value", 0, serverInfo.map);
				base.App.UI.AddItem("lstGameSettings", "property", 1, "Players:");
				base.App.UI.SetItemText("lstGameSettings", "value", 1, serverInfo.players.ToString());
				base.App.UI.AddItem("lstGameSettings", "property", 2, "Max Players:");
				base.App.UI.SetItemText("lstGameSettings", "value", 2, serverInfo.maxPlayers.ToString());
			}
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._previousState = prev;
			if (parms.Count<object>() > 0)
			{
				this._enterState = (LobbyEntranceState)parms[0];
			}
			this._camera = new OrbitCameraController(base.App);
			this._crits = new GameObjectSet(base.App);
			this._sky = new Sky(base.App, SkyUsage.InSystem, 3);
			this._crits.Add(this._sky);
			this._starmap = new StarMapLobby(base.App, this._sky);
			this._starmap.SetCamera(this._camera);
			this._painter = new ArrowPainter(base.App);
			this._shipCrits = new GameObjectSet(base.App);
			this._shipCamera = new OrbitCameraController(base.App);
			this._shipCrits.Add(this._shipCamera);
			this._shipHoloView = new ShipHoloView(base.App, this._shipCamera);
			this._shipCrits.Add(this._shipHoloView);
			this._builder = new ShipBuilder(base.App);
			this._tempPlayerInfo = new PlayerInfo();
			this._tempPlayerInfo.BadgeAssetPath = "";
			this._tempPlayerInfo.AvatarAssetPath = "";
			this._contextMenuID = base.App.UI.CreatePanelFromTemplate("SlotSwapContextMenu", null);
			base.App.UI.LoadScreen("StarMapLobby");
			this._starmap.Initialize(this._crits, new object[0]);
			this._crits.Add(this._starmap);
			base.App.Network.EnableChatWidget(true);
			this._settingsDirty.Clear();
			GameSetupUI.ClearPlayerListWidget(base.App, "lstPlayers");
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "ObjectClicked")
			{
				this.ProcessGameEvent_ObjectClicked(eventParams);
				return;
			}
			if (eventName == "ListContextMenu")
			{
				this.ShowSlotSwapPopup(eventParams);
				return;
			}
			if (eventName == "ContextMenu")
			{
				this.ProcessGameEvent_ContextMenu(eventParams);
				return;
			}
			if (eventName == "MouseOver")
			{
				this.ProcessGameEvent_MouseOver(eventParams);
				return;
			}
			if (eventName == "DragAndDropEvent")
			{
				string arg_64_0 = eventParams[0];
				string arg_68_0 = eventParams[1];
				string arg_6C_0 = eventParams[2];
				int.Parse(eventParams[3]);
			}
		}
		private void SetSelectedPlanet(int value, string trigger)
		{
			if (this._selectedPlanet == value)
			{
				return;
			}
			this._selectedPlanet = value;
		}
		public GameObjectSet GetCrits()
		{
			return this._crits;
		}
		private int InferServer(IGameObject obj)
		{
			if (!(obj is StarMapServer))
			{
				return 0;
			}
			int result;
			if (this._starmap.Servers.Forward.TryGetValue((StarMapServer)obj, out result))
			{
				return result;
			}
			return 0;
		}
		private void SelectObject(IGameObject o)
		{
			if (o == null)
			{
				this.SelectedServer = -1;
				GameSetupUI.ClearPlayerListWidget(base.App, "lstPlayers");
				base.App.UI.ClearItems("lstGameSettings");
				return;
			}
			this.SelectedServer = this.InferServer(o);
			base.App.Network.SelectServer(this.SelectedServer);
			this.SetGameDetails(this.SelectedServer);
			GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", this._servers[this.SelectedServer].playerInfo, true);
		}
		private void ProcessGameEvent_ContextMenu(string[] eventParams)
		{
			int num = int.Parse(eventParams[0]);
			if (num == 0)
			{
				return;
			}
			StarMapSystem gameObject = base.App.GetGameObject<StarMapSystem>(num);
		}
		private void ProcessGameEvent_ObjectClicked(string[] eventParams)
		{
			int id = int.Parse(eventParams[0]);
			this.SelectObject(base.App.GetGameObject(id));
		}
		private void ProcessGameEvent_MouseOver(string[] eventParams)
		{
			int.Parse(eventParams[0]);
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._playerInitialSystemsSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive) || this._playerInitialTechnologiesSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive) || this._playerInitialTreasurySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
			{
				return;
			}
			if (msgType == "ChatMessage")
			{
				base.App.UI.SetPropertyBool("btnChat", "flashing", true);
			}
			if (msgType == "button_clicked")
			{
				if (panelName == "selectBackground")
				{
					base.App.UI.SetVisible("dlgSelectBadge", false);
					base.App.UI.SetVisible("dlgSelectAvatar", false);
				}
				else
				{
					if (panelName == "buttonProcess")
					{
						base.App.UI.SetText("process_text", "");
						base.App.UI.SetVisible("buttonProcess", false);
						base.App.UI.SetVisible("process_dialog", false);
					}
					else
					{
						if (panelName.EndsWith("btnFactionSelected"))
						{
							string text = panelName.Split(new char[]
							{
								'|'
							})[0];
							int subfactionIndex = 0;
							if (text.EndsWith("_dlc"))
							{
								subfactionIndex = 1;
								text = text.Substring(0, text.Length - "_dlc".Length);
							}
							this.SelectFaction(text, subfactionIndex);
							base.App.UI.SetVisible("dlgSelectFaction", false);
							this.OnPlayerChanged(this.SelectedSlot, true, true);
						}
						else
						{
							if (panelName.EndsWith("btnDifficultySelected"))
							{
								this.SelectDifficulty(panelName.Split(new char[]
								{
									'|'
								})[0]);
								base.App.UI.SetVisible("dlgSelectDifficulty", false);
								this.OnPlayerChanged(this.SelectedSlot, true, true);
							}
							else
							{
								if (panelName.EndsWith("btnAvatarSelected"))
								{
									this.SelectAvatar(panelName.Split(new char[]
									{
										'|'
									})[0]);
									base.App.UI.SetVisible("dlgSelectAvatar", false);
									this.OnPlayerChanged(this.SelectedSlot, true, true);
								}
								else
								{
									if (panelName.EndsWith("btnBadgeSelected"))
									{
										this.SelectBadge(panelName.Split(new char[]
										{
											'|'
										})[0]);
										base.App.UI.SetVisible("dlgSelectBadge", false);
										this.OnPlayerChanged(this.SelectedSlot, true, true);
									}
									else
									{
										if (panelName.EndsWith("btnEmpireColorSelected"))
										{
											base.App.UI.SetVisible("dlgSelectEmpireColor", false);
											this._dlgSelectEmpireColorVisible = false;
											this.SetEmpireColor(this.SelectedSlot, new int?(int.Parse(panelName.Split(new char[]
											{
												'|'
											})[0])));
											this.OnPlayerChanged(this.SelectedSlot, false, false);
										}
										else
										{
											if (panelName.EndsWith("btnShipColorAccept"))
											{
												base.App.UI.SetVisible("dlgSelectShipColor", false);
												this._dlgSelectShipColorVisible = false;
												this.OnPlayerChanged(this.SelectedSlot, false, false);
											}
											else
											{
												if (panelName.StartsWith("team_button"))
												{
													string[] array = panelName.Split(new char[]
													{
														'|'
													});
													if (array.Count<string>() == 2)
													{
														int num = int.Parse(array[1]);
														base.App.GameSetup.NextTeam(num);
														GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", base.App.GameSetup.Players, false);
														if (base.App.GameSetup.IsMultiplayer)
														{
															base.App.Network.SetPlayerInfo(base.App.GameSetup.Players[num], num);
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				switch (panelName)
				{
				case "gameSwapButton":
				{
					int slot = base.App.GameSetup.LocalPlayer.slot;
					int databaseId = base.App.GameSetup.Players[this._contextSlot].databaseId;
					if (base.App.GameSetup.IsMultiplayer)
					{
						base.App.Network.ChangeSlot(slot, this._contextSlot);
						return;
					}
					if (base.App.Game != null && base.App.Game.LocalPlayer.ID != databaseId)
					{
						Player playerObject = base.App.Game.GetPlayerObject(databaseId);
						if (playerObject != null)
						{
							base.App.Game.SetLocalPlayer(playerObject);
						}
					}
					base.App.GameSetup.Players[this._contextSlot].Name = base.App.GameSetup.Players[slot].Name;
					base.App.GameSetup.Players[this._contextSlot].localPlayer = true;
					base.App.GameSetup.Players[this._contextSlot].AI = false;
					base.App.GameSetup.Players[slot].Name = string.Empty;
					base.App.GameSetup.Players[slot].localPlayer = false;
					base.App.GameSetup.Players[slot].AI = true;
					this.OnPlayerChanged(this._contextSlot, true, true);
					this.OnPlayerChanged(slot, true, true);
					this.SetSelectedSlot(this._contextSlot);
					GameSetupUI.SyncPlayerSetupWidget(base.App, "pnlPlayerSetup", base.App.GameSetup.Players[this._contextSlot]);
					this.UpdateShipColors(this._contextSlot);
					this.UpdateShipPreview(this._contextSlot);
					return;
				}
				case "gameKickButton":
					base.App.Network.Kick(this._contextSlot);
					return;
				case "btnSelectFaction":
					this.DisableAllFactionButtons();
					foreach (Faction current in base.App.GameSetup.AvailablePlayerFeatures.Factions.Keys)
					{
						base.App.UI.SetVisible(current.Name + "|btnFactionSelected", true);
						base.App.UI.SetEnabled(current.Name + "|btnFactionSelected", true);
						SteamDLCIdentifiers? dLCIdentifierFromFaction = SteamDLCHelpers.GetDLCIdentifierFromFaction(current);
						if (dLCIdentifierFromFaction.HasValue && base.App.Steam.HasDLC((int)dLCIdentifierFromFaction.Value))
						{
							base.App.UI.SetEnabled(current.Name + "_dlc|btnFactionSelected", true);
						}
					}
					base.App.UI.SetVisible("dlgSelectFaction", true);
					return;
				case "btnSelectDifficulty":
					base.App.UI.SetVisible("dlgSelectDifficulty", true);
					return;
				case "btnSelectBadge":
				{
					int num3 = 0;
					base.App.UI.ClearItems("lstBadges");
					foreach (string current2 in base.App.GameSetup.GetAvailableBadges(this.SelectedFaction))
					{
						string text2 = current2.ToLower();
						Faction faction = base.App.AssetDatabase.GetFaction(this.SelectedFaction);
						bool flag = false;
						if (faction.DlcID.HasValue && base.App.Steam.HasDLC(faction.DlcID.Value))
						{
							flag = true;
						}
						if ((!text2.Contains("dlc") || flag) && current2 != base.App.GameSetup.Players[this.SelectedSlot].Badge)
						{
							base.App.UI.AddItem("lstBadges", string.Empty, num3, string.Empty);
							string itemGlobalID = base.App.UI.GetItemGlobalID("lstBadges", string.Empty, num3, string.Empty);
							base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
							{
								itemGlobalID,
								"imgItemImage"
							}), "sprite", current2);
							base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
							{
								itemGlobalID,
								"btnImageButton"
							}), "id", string.Format("{0}|btnBadgeSelected", current2));
							num3++;
						}
					}
					base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
					{
						"dlgSelectBadge",
						"imgSelectedBadge"
					}), "sprite", base.App.GameSetup.Players[this.SelectedSlot].Badge ?? string.Empty);
					base.App.UI.SetVisible("dlgSelectBadge", true);
					return;
				}
				case "btnEmpireName":
					this._empireGUID = base.App.UI.CreateDialog(new GenericTextEntryDialog(base.App, App.Localize("@UI_EMPIRE_NAME"), App.Localize("@UI_ENTER_NEW_EMPIRE_NAME"), "", 32, 2, true, EditBoxFilterMode.None), null);
					return;
				case "btnSelectAvatar":
				{
					int num3 = 0;
					base.App.UI.ClearItems("lstAvatars");
					foreach (string current3 in base.App.GameSetup.GetAvailableAvatars(this.SelectedFaction))
					{
						string text3 = current3.ToLower();
						Faction faction2 = base.App.AssetDatabase.GetFaction(this.SelectedFaction);
						bool flag2 = false;
						if (faction2.DlcID.HasValue && base.App.Steam.HasDLC(faction2.DlcID.Value))
						{
							flag2 = true;
						}
						if ((!text3.Contains("dlc") || flag2) && current3 != base.App.GameSetup.Players[this.SelectedSlot].Avatar && !base.App.GameSetup.IsAvatarUsed(current3))
						{
							base.App.UI.AddItem("lstAvatars", string.Empty, num3, string.Empty);
							string itemGlobalID2 = base.App.UI.GetItemGlobalID("lstAvatars", string.Empty, num3, string.Empty);
							base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
							{
								itemGlobalID2,
								"imgItemImage"
							}), "sprite", current3);
							base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
							{
								itemGlobalID2,
								"btnImageButton"
							}), "id", string.Format("{0}|btnAvatarSelected", current3));
							num3++;
						}
					}
					base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
					{
						"dlgSelectAvatar",
						"imgSelectedAvatar"
					}), "sprite", base.App.GameSetup.Players[this.SelectedSlot].Avatar ?? string.Empty);
					base.App.UI.SetVisible("dlgSelectAvatar", true);
					return;
				}
				case "btnSelectEmpireColor":
					if (this._dlgSelectEmpireColorVisible)
					{
						this._dlgSelectEmpireColorVisible = false;
						base.App.UI.SetVisible("dlgSelectEmpireColor", false);
					}
					else
					{
						this._dlgSelectEmpireColorVisible = true;
						int num3 = 0;
						base.App.UI.ClearItems("lstEmpireColors");
						foreach (int current4 in 
							from x in base.App.GameSetup.AvailablePlayerFeatures.EmpireColors
							select x.Value)
						{
							string globalID = base.App.UI.GetGlobalID(string.Format("dlgSelectEmpireColor.color{0}", current4));
							if (base.App.GameSetup.IsEmpireColorUsed(num3))
							{
								base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
								{
									globalID,
									"imgItemImage"
								}), "color", Player.DefaultPrimaryPlayerColors[current4] * 255f);
								base.App.UI.SetVisible(base.App.UI.Path(new string[]
								{
									globalID,
									"imgDisabled"
								}), true);
								base.App.UI.SetEnabled(base.App.UI.Path(new string[]
								{
									globalID,
									"btnImageButton"
								}), false);
								base.App.UI.SetEnabled(base.App.UI.Path(new string[]
								{
									globalID,
									string.Format("{0}|btnEmpireColorSelected", current4)
								}), false);
							}
							else
							{
								base.App.UI.SetVisible(base.App.UI.Path(new string[]
								{
									globalID,
									"imgDisabled"
								}), false);
								base.App.UI.SetEnabled(base.App.UI.Path(new string[]
								{
									globalID,
									"btnImageButton"
								}), true);
								base.App.UI.SetEnabled(base.App.UI.Path(new string[]
								{
									globalID,
									string.Format("{0}|btnEmpireColorSelected", current4)
								}), true);
								base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
								{
									globalID,
									"imgItemImage"
								}), "color", Player.DefaultPrimaryPlayerColors[current4] * 255f);
								base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
								{
									globalID,
									"btnImageButton"
								}), "id", string.Format("{0}|btnEmpireColorSelected", current4));
							}
							num3++;
						}
						base.App.UI.SetVisible("dlgSelectEmpireColor", true);
					}
					if (this._dlgSelectShipColorVisible)
					{
						this._dlgSelectShipColorVisible = false;
						base.App.UI.SetVisible("dlgSelectShipColor", false);
						return;
					}
					break;
				case "btnSelectSecondaryColor":
					this._dlgSelectShipColorVisible = !this._dlgSelectShipColorVisible;
					base.App.UI.SetVisible("dlgSelectShipColor", this._dlgSelectShipColorVisible);
					if (this._dlgSelectEmpireColorVisible)
					{
						this._dlgSelectEmpireColorVisible = false;
						base.App.UI.SetVisible("dlgSelectEmpireColor", false);
						return;
					}
					break;
				case "btnPlayerSetupOk":
					this.HidePlayerSetup();
					return;
				case "btnRefreshServers":
					this._servers.Clear();
					this._starmap.ClearServers(this._crits);
					return;
				case "btnRefresh":
					this.RefreshServers();
					return;
				case "designShipClick":
					this.HideColorPicker();
					return;
				case "btnChat":
					base.App.Network.SetChatWidgetVisibility(null);
					base.App.UI.SetPropertyBool("btnChat", "flashing", false);
					return;
				case "btnBack":
					if (base.App.Network.IsJoined || base.App.Network.IsHosting)
					{
						this.Reset();
						base.App.Network.Disconnect();
						this.HidePlayerSetup();
						this.MultiplayerButtonState();
						base.App.ResetGameSetup();
						base.App.GameSetup.IsMultiplayer = true;
						this.HideShipPreview();
						GameSetupUI.ClearPlayerListWidget(base.App, "lstPlayers");
						this.RefreshServers();
						return;
					}
					if (this._refreshing)
					{
						this.RefreshServers();
					}
					base.App.SwitchGameState("MainMenuState");
					return;
				case "btnHostGame":
					if (base.App.Network.IsHosting)
					{
						base.App.SwitchGameState<GameSetupState>(new object[0]);
						return;
					}
					this._createGameGUID = base.App.UI.CreateDialog(new NetCreateGameDialog(base.App), null);
					return;
				case "buttonGameSettings":
					if (base.App.Network.IsHosting)
					{
						base.App.SwitchGameState<GameSetupState>(new object[0]);
						return;
					}
					break;
				case "btnDirectConnect":
					this._directConnectGUID = base.App.UI.CreateDialog(new DirectConnectDialog(base.App), null);
					return;
				case "btnJoinGame":
				case "btnStart":
				{
					if (!base.App.GameSetup.IsMultiplayer)
					{
						Action action = null;
						if (this._enterState == LobbyEntranceState.SinglePlayerLoad)
						{
							action = new Action(this.LoadSinglePlayer);
						}
						else
						{
							if (this._enterState == LobbyEntranceState.SinglePlayer)
							{
								action = new Action(this.NewSinglePlayer);
							}
						}
						base.App.SwitchGameStateViaLoadingScreen(action, null, base.App.GetGameState<StarMapState>(), null);
						return;
					}
					if (base.App.Network.IsJoined || base.App.Network.IsHosting)
					{
						base.App.Network.Ready();
						return;
					}
					base.App.GameSetup.ClearUsedAvatars();
					base.App.GameSetup.ClearUsedBadges();
					base.App.GameSetup.ClearUsedEmpireColors();
					base.App.UI.UnlockUI();
					ServerInfo serverInfo = this._servers[StarMapLobbyState._selectedIndex];
					if (serverInfo != null)
					{
						if (serverInfo.passworded)
						{
							this._passwordGUID = base.App.UI.CreateDialog(new GenericTextEntryDialog(base.App, "Protected Game", "Enter Password: ", "", 30, 1, true, EditBoxFilterMode.None), null);
							return;
						}
						base.App.Network.JoinGame(serverInfo.serverID, "");
						return;
					}
					break;
				}

					return;
				}
			}
			else
			{
				if (msgType == "dialog_closed")
				{
					if (panelName == this._loginGUID && msgParams.Count<string>() > 0)
					{
						if (msgParams[0] == "True")
						{
							if (base.App.GameSetup.IsMultiplayer)
							{
								base.App.SwitchGameState<MainMenuState>(new object[0]);
							}
							base.App.Network.SetChatWidgetVisibility(new bool?(false));
							return;
						}
						this.RefreshServers();
						return;
					}
					else
					{
						if (panelName == this._directConnectGUID)
						{
							if (msgParams[0] == "True")
							{
								StarMapLobbyState._lastIPTyped = msgParams[1];
								base.App.Network.DirectConnect(msgParams[1], msgParams[2]);
								return;
							}
						}
						else
						{
							if (panelName == this._passwordGUID)
							{
								if (msgParams[0] == "True")
								{
									ulong serverID = this._servers[StarMapLobbyState._selectedIndex].serverID;
									base.App.Network.JoinGame(serverID, msgParams[1]);
									return;
								}
							}
							else
							{
								if (panelName == this._createGameGUID)
								{
									if (msgParams[0] == "False")
									{
										if (msgParams[1] == "False")
										{
											string gameName = msgParams[2];
											string gamePass = msgParams[3];
											base.App.Network.UpdateGameInfo(gameName, gamePass);
											base.App.UI.SetVisible("buttonGameSettings", true);
											base.App.UI.SetVisible("buttonPlayerSettings", true);
											base.App.GameSetup.IsMultiplayer = true;
											this.CreateGame();
											base.App.Network.SetChatWidgetVisibility(new bool?(false));
											return;
										}
										string gameName2 = msgParams[2];
										string gamePass2 = msgParams[3];
										base.App.Network.UpdateGameInfo(gameName2, gamePass2);
										base.App.Network.Host();
										base.App.Network.GameLoaded = true;
										this.EnablePlayerSetup(false);
										for (int i = 0; i < base.App.GameSetup.Players.Count<PlayerSetup>(); i++)
										{
											PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(i + 1);
											if (playerInfo.isDefeated)
											{
												base.App.GameSetup.Players[i].Locked = true;
											}
										}
										base.App.Network.SetGameInfo(base.App.GameSetup);
										for (int j = 0; j < base.App.GameSetup.Players.Count<PlayerSetup>(); j++)
										{
											this.SetSelectedSlot(j);
											this.OnPlayerChanged(j, true, true);
										}
										int? num4 = base.App.GameDatabase.GetLastClientPlayerID(base.App.Network.Username) - 1;
										if (!num4.HasValue)
										{
											num4 = new int?(0);
										}
										base.App.GameSetup.Players[num4.Value].AI = false;
										base.App.GameSetup.Players[num4.Value].localPlayer = true;
										this.SetSelectedSlot(num4.Value);
										this.OnPlayerChanged(num4.Value, true, true);
										this.ShowPlayerSetup(num4.Value);
										this.HostButtonState();
										return;
									}
								}
								else
								{
									if (panelName == this._empireGUID && msgParams[0] == "True")
									{
										base.App.GameSetup.Players[this.SelectedSlot].EmpireName = msgParams[1];
										App.Log.Trace("Setting empire name to " + msgParams[1], "net");
										this.OnPlayerChanged(this.SelectedSlot, true, true);
										return;
									}
								}
							}
						}
					}
				}
				else
				{
					if (msgType == "list_sel_changed")
					{
						if (panelName == "lstPlayers")
						{
							int num5 = int.Parse(msgParams[0]);
							if ((base.App.GameSetup.LocalPlayer != null && base.App.GameSetup.LocalPlayer.slot == num5) || base.App.Network.IsHosting || !base.App.GameSetup.IsMultiplayer)
							{
								this.ShowPlayerSetup(num5);
								return;
							}
						}
						else
						{
							if (panelName == "lstServers")
							{
								int num6 = int.Parse(msgParams[0]);
								this.SelectedServer = num6;
								this.FocusOnServer(num6 + 1);
								this.SetGameDetails(num6);
								return;
							}
						}
					}
					else
					{
						if (msgType == "color_changed" && panelName == "pickerSecondaryColor")
						{
							this.SetShipColor(this.SelectedSlot, new Vector3(float.Parse(msgParams[0]), float.Parse(msgParams[1]), float.Parse(msgParams[2])), false);
							this.OnPlayerChanged(this.SelectedSlot, false, false);
						}
					}
				}
			}
		}
		protected override void OnEnter()
		{
			base.App.PostPlayMusic("Ambient_GameSetup");
			base.App.UI.UnlockUI();
			base.App.UI.SetScreen("StarMapLobby");
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			base.App.UI.Send(new object[]
			{
				"SetGameObject",
				"ohStarMapLobby",
				this._starmap.ObjectID
			});
			base.App.UI.SetListCleanClear("lstPlayers", true);
			base.App.Network.StarMapLobby = this;
			base.App.Network.InLobby(true);
			this._serverPositions = this.GenerateMeASpiralGalaxyPlease();
			this._crits.Activate();
			this._starmap.ClearServers(this._crits);
			this._servers.Clear();
			this._shipCamera.Active = false;
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			base.App.UI.Send(new object[]
			{
				"SetGameObject",
				"designShip",
				this._shipHoloView.ObjectID
			});
			this._camera.Active = true;
			this._camera.MaxDistance = 800f;
			this._camera.DesiredDistance = 200f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
			this._playerInitialTreasurySlider = new TreasurySlider(base.App.UI, "sldInitialTreasury", 0, base.App.GameSetup.HasScenarioFile() ? 0 : 0, base.App.GameSetup.HasScenarioFile() ? 2147483647 : 1000000);
			this._playerInitialTreasurySlider.ValueChanged += new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialSystemsSpinner = new ValueBoundSpinner(base.App.UI, "sldInitialSystems", (double)(base.App.GameSetup.HasScenarioFile() ? 0 : 3), (double)(base.App.GameSetup.HasScenarioFile() ? 2147483647 : 9), (double)base.App.GameSetup.InitialSystems, 1.0);
			this._playerInitialSystemsSpinner.ValueChanged += new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialTechnologiesSpinner = new ValueBoundSpinner(base.App.UI, "sldInitialTechs", (double)(base.App.GameSetup.HasScenarioFile() ? 0 : 0), (double)(base.App.GameSetup.HasScenarioFile() ? 2147483647 : 10), (double)base.App.GameSetup.InitialTechnologies, 1.0);
			this._playerInitialTechnologiesSpinner.ValueChanged += new ValueChangedEventHandler(this.PanelValueChanged);
			this.HideColorPicker();
			base.App.IRC.SetupIRCClient(base.App.UserProfile.ProfileName);
			if (this._enterState == LobbyEntranceState.Browser)
			{
				GameSetupUI.ClearPlayerListWidget(base.App, "lstPlayers");
				this.Reset();
			}
			else
			{
				if (this._enterState == LobbyEntranceState.Multiplayer)
				{
					base.App.Network.Host();
					base.App.Network.SetGameInfo(base.App.GameSetup);
				}
				base.App.GameSetup.ClearUsedAvatars();
				base.App.GameSetup.ClearUsedEmpireColors();
				this._numPlayerSlots = 0;
				this.NumPlayerSlots = base.App.GameSetup.Players.Count;
				for (int i = 0; i < this.NumPlayerSlots; i++)
				{
					if (i != 0)
					{
						base.App.GameSetup.Players[i].AI = true;
						this._settingsDirty[i] = true;
					}
					else
					{
						base.App.GameSetup.Players[0].localPlayer = true;
						base.App.GameSetup.Players[0].AI = false;
					}
					this.DefaultPlayer(i, base.App.GameSetup.HasScenarioFile());
					this.SetSelectedSlot(i);
					this.OnPlayerChanged(i, true, true);
				}
				this.ShowPlayerSetup(0);
			}
			if (this._enterState == LobbyEntranceState.Multiplayer || this._enterState == LobbyEntranceState.Browser)
			{
				if (this._enterState == LobbyEntranceState.Browser)
				{
					base.App.Network.Login(base.App.UserProfile.ProfileName);
				}
				if (base.App.Network.IsHosting)
				{
					this.HostButtonState();
				}
				else
				{
					if (base.App.Network.IsJoined)
					{
						this.JoinButtonState();
					}
					else
					{
						this.MultiplayerButtonState();
					}
				}
			}
			else
			{
				int num = 0;
				if (this._enterState == LobbyEntranceState.SinglePlayerLoad)
				{
					int? lastClientPlayerID = base.App.GameDatabase.GetLastClientPlayerID(base.App.UserProfile.ProfileName);
					if (lastClientPlayerID.HasValue)
					{
						num = lastClientPlayerID.Value - 1;
						Player playerObject = base.App.Game.GetPlayerObject(lastClientPlayerID.Value);
						base.App.Game.SetLocalPlayer(playerObject);
					}
				}
				base.App.GameSetup.Players[num].Name = (base.App.UserProfile.ProfileName ?? string.Empty);
				if (base.App.GameSetup.Players[num].Name == null)
				{
					base.App.SwitchGameState<MainMenuState>(new object[0]);
				}
				base.App.GameSetup.Players[num].AI = false;
				base.App.GameSetup.Players[num].localPlayer = true;
				if (num != 0)
				{
					base.App.GameSetup.Players[0].AI = true;
					base.App.GameSetup.Players[0].localPlayer = false;
				}
				this.SinglePlayerButtonState();
				this.SetSelectedSlot(num);
				this.OnPlayerChanged(num, true, true);
				this.ShowPlayerSetup(num);
			}
			if (this._enterState == LobbyEntranceState.SinglePlayerLoad)
			{
				List<PlayerInfo> list = base.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
				using (List<PlayerInfo>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PlayerInfo current = enumerator.Current;
						this.SetSlotEnabled(current.ID - 1, !current.isDefeated);
					}
					goto IL_6FC;
				}
			}
			foreach (PlayerSetup current2 in base.App.GameSetup.Players)
			{
				this.SetSlotEnabled(current2.databaseId, true);
			}
			IL_6FC:
			base.App.UI.SetEnabled("btnBack", true);
			base.App.UI.SetEnabled("btnStart", true);
			base.App.Network.InLobby(true);
		}
		private void SetSlotEnabled(int slot, bool enabled)
		{
			string itemGlobalID = base.App.UI.GetItemGlobalID("lstPlayers", string.Empty, slot, "");
			base.App.UI.SetVisible(base.App.UI.Path(new string[]
			{
				itemGlobalID,
				"eliminatedState"
			}), !enabled);
		}
		public void OnRefreshComplete()
		{
			this._refreshing = false;
			base.App.UI.SetText("btnRefresh", "@UI_GAMEMODEBUTTONS_REFRESH");
		}
		protected void RefreshServers()
		{
			if (!this._betaDisable)
			{
				if (!this._refreshing)
				{
					base.App.UI.SetText("btnRefresh", "Stop Refresh");
					this._servers.Clear();
					this._starmap.ClearServers(this._crits);
					GameSetupUI.ClearPlayerListWidget(base.App, "lstPlayers");
					this._refreshing = true;
				}
				else
				{
					this.OnRefreshComplete();
				}
				base.App.Network.RefreshServers(!this._refreshing);
			}
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.Reset();
			base.App.Network.StarMapLobby = null;
			base.App.Network.InLobby(false);
			base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._playerInitialSystemsSpinner.ValueChanged -= new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialSystemsSpinner = null;
			this._playerInitialTechnologiesSpinner.ValueChanged -= new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialTechnologiesSpinner = null;
			this._playerInitialTreasurySlider.ValueChanged -= new ValueChangedEventHandler(this.PanelValueChanged);
			this._playerInitialTreasurySlider = null;
			base.App.Network.InLobby(false);
			this.HidePlayerSetup();
			this._camera.Active = false;
			this._camera.TargetID = 0;
			this._builder.Dispose();
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
			}
			if (this._shipCrits != null)
			{
				this._shipCrits.Dispose();
				this._shipCrits = null;
			}
			base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			base.App.UI.DestroyPanel(this._contextMenuID);
			this._painter.Dispose();
			this._starmap = null;
			base.App.UI.DeleteScreen("StarMapLobby");
		}
		protected override void OnUpdate()
		{
			if (base.App.Network.IsHosting)
			{
				this._frameCount++;
				if (this._frameCount > 60)
				{
					foreach (KeyValuePair<int, bool> current in this._settingsDirty)
					{
						base.App.Network.SetPlayerInfo(base.App.GameSetup.Players[current.Key], current.Key);
					}
					this._settingsDirty.Clear();
					this._frameCount = 0;
				}
			}
			if (base.App.GameSetup.IsMultiplayer)
			{
				base.App.UI.SetEnabled("btnStart", StarMapLobbyState._selectedIndex != -1 || base.App.Network.IsHosting || base.App.Network.IsJoined);
			}
			this._builder.Update();
			if (this._builder.Ship != null && !this._builder.Loading && this._builder.Ship.Active && this._shipCamera.TargetID != this._builder.Ship.ObjectID)
			{
				this._shipCamera.TargetID = this._builder.Ship.ObjectID;
				this._shipHoloView.SetUseViewport(true);
				this._shipHoloView.SetShip(this._builder.Ship);
			}
			if (this._painter.ObjectStatus == GameObjectStatus.Ready && !this._painter.Active)
			{
				this._painter.Active = true;
				this._starmap.PostObjectAddObjects(new IGameObject[]
				{
					this._painter
				});
			}
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		private void ShowSlotSwapPopup(string[] eventParams)
		{
			base.App.UI.AutoSize(this._contextMenuID);
			this._contextSlot = int.Parse(eventParams[0]);
			if (this._contextSlot < 0 || this._contextSlot > base.App.GameSetup.Players.Count<PlayerSetup>())
			{
				return;
			}
			if (!base.App.Network.IsHosting)
			{
				if (base.App.GameSetup.Players[this._contextSlot].localPlayer)
				{
					return;
				}
				base.App.UI.SetEnabled("gameKickButton", false);
				if (!base.App.GameSetup.Players[this._contextSlot].AI)
				{
					return;
				}
				base.App.UI.SetEnabled("gameSwapButton", true);
			}
			else
			{
				if (!base.App.Network.IsHosting)
				{
					return;
				}
				if (base.App.GameSetup.Players[this._contextSlot].localPlayer)
				{
					return;
				}
				if (!base.App.GameSetup.Players[this._contextSlot].AI)
				{
					base.App.UI.SetEnabled("gameKickButton", true);
				}
				else
				{
					base.App.UI.SetEnabled("gameKickButton", false);
				}
				base.App.UI.SetEnabled("gameSwapButton", true);
			}
			base.App.UI.ShowTooltip(this._contextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
		}
		private void PanelValueChanged(object sender, ValueChangedEventArgs e)
		{
			PlayerSetup playerSetup = base.App.GameSetup.Players[this.SelectedSlot];
			if (sender == this._playerInitialTreasurySlider)
			{
				playerSetup.InitialTreasury = this._playerInitialTreasurySlider.Value;
			}
			else
			{
				if (sender == this._playerInitialSystemsSpinner)
				{
					playerSetup.InitialColonies = (int)this._playerInitialSystemsSpinner.Value;
				}
				else
				{
					if (sender == this._playerInitialTechnologiesSpinner)
					{
						playerSetup.InitialTechs = (int)this._playerInitialTechnologiesSpinner.Value;
					}
				}
			}
			this._settingsDirty[this.SelectedSlot] = true;
		}
		protected void HostButtonState()
		{
			base.App.UI.SetText("btnStart", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_READY"));
			base.App.UI.SetText("btnBack", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_STOP_GAME"));
			base.App.UI.SetVisible("btnHostGame", false);
			base.App.UI.SetVisible("btnDirectConnect", false);
			base.App.UI.SetVisible("btnRefresh", false);
		}
		protected void JoinButtonState()
		{
			base.App.UI.SetText("btnStart", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_READY"));
			base.App.UI.SetText("btnBack", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_LEAVE_GAME"));
			base.App.UI.SetVisible("pnlMultiplayerGameType", false);
		}
		protected void MultiplayerButtonState()
		{
			base.App.UI.SetText("btnStart", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_JOIN_GAME"));
			base.App.UI.SetText("btnBack", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_CANCEL_GAME"));
			base.App.UI.SetText("btnHostGame", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_HOST_GAME"));
			base.App.UI.SetPropertyBool("btnStart", "lockout_button", false);
			base.App.UI.SetVisible("btnHostGame", true);
			base.App.UI.SetVisible("btnDirectConnect", true);
			base.App.UI.SetVisible("pnlMultiplayerBar", true);
			base.App.UI.SetVisible("pnlSingleplayerBar", false);
			base.App.UI.SetVisible("pnlMultiplayerGameType", true);
			base.App.UI.SetVisible("btnRefresh", true);
			if (this._betaDisable)
			{
				base.App.UI.SetEnabled("btnHostGame", false);
				base.App.UI.SetEnabled("btnRefresh", false);
			}
		}
		protected void SinglePlayerButtonState()
		{
			base.App.UI.SetText("btnStart", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_START_GAME"));
			base.App.UI.SetText("btnBack", AssetDatabase.CommonStrings.Localize("@UI_GAMESETUP_CANCEL_GAME"));
			base.App.UI.SetVisible("pnlMultiplayerGameType", false);
			base.App.UI.SetVisible("pnlMultiplayerBar", false);
			base.App.UI.SetVisible("pnlSingleplayerBar", true);
			base.App.UI.SetPropertyBool("btnStart", "lockout_button", true);
		}
		protected void DisableAllFactionButtons()
		{
			base.App.UI.SetEnabled("human|btnFactionSelected", false);
			base.App.UI.SetEnabled("human_dlc|btnFactionSelected", false);
			base.App.UI.SetEnabled("hiver|btnFactionSelected", false);
			base.App.UI.SetEnabled("hiver_dlc|btnFactionSelected", false);
			base.App.UI.SetEnabled("tarkas|btnFactionSelected", false);
			base.App.UI.SetEnabled("tarkas_dlc|btnFactionSelected", false);
			base.App.UI.SetEnabled("zuul|btnFactionSelected", false);
			base.App.UI.SetEnabled("zuul_dlc|btnFactionSelected", false);
			base.App.UI.SetEnabled("morrigi|btnFactionSelected", false);
			base.App.UI.SetEnabled("morrigi_dlc|btnFactionSelected", false);
			base.App.UI.SetEnabled("liir_zuul|btnFactionSelected", false);
			base.App.UI.SetEnabled("liir_zuul_dlc|btnFactionSelected", false);
		}
		public void EnablePlayerSetup(bool enable)
		{
			base.App.UI.SetEnabled("btnEmpireName", enable);
			base.App.UI.SetEnabled("btnSelectAvatar", enable);
			base.App.UI.SetEnabled("btnSelectEmpireColor", enable);
			base.App.UI.SetEnabled("btnSelectSecondaryColor", enable);
			base.App.UI.SetEnabled("btnSelectBadge", enable);
			base.App.UI.SetEnabled("btnSelectFaction", enable);
			base.App.UI.SetEnabled("btnSelectDifficulty", enable);
			if (this._playerInitialTreasurySlider != null)
			{
				this._playerInitialTreasurySlider.SetEnabled(enable);
			}
			if (this._playerInitialSystemsSpinner != null)
			{
				this._playerInitialSystemsSpinner.SetEnabled(enable);
			}
			if (this._playerInitialTechnologiesSpinner != null)
			{
				this._playerInitialTechnologiesSpinner.SetEnabled(enable);
			}
		}
		public void EnableGameInProgress(bool enable)
		{
			base.App.UI.SetVisible("pnlGameInProgressStatus", enable);
		}
		public void UpdateGameInProgress()
		{
			string text = string.Format("Game in progress: Turn {0}", this.GameInProgressTurn);
			if (this.IsLocalPlayerReady)
			{
				text = text + " " + string.Format("(You will join on the next turn update.)", new object[0]);
			}
			base.App.UI.SetText("lblGameInProgressStatus", text);
		}
		private void HideShipPreview()
		{
			this._shipHoloView.HideViewport(true);
		}
		public void ShowPlayerSetup(int index)
		{
			this._camera.Active = false;
			this._shipCamera.DesiredYaw = 1.57079637f;
			this._shipCamera.Active = true;
			base.App.UI.SetVisible("ohStarMapLobby", false);
			base.App.UI.SetVisible("pnlPlayerSetup", true);
			this._inPlayerSetup = true;
			PlayerSetup playerSetup = base.App.GameSetup.Players[index];
			if (!string.IsNullOrEmpty(playerSetup.Badge))
			{
				this._tempPlayerInfo.BadgeAssetPath = Path.Combine("factions", playerSetup.Faction, "badges", playerSetup.Badge + ".tga");
			}
			else
			{
				this._tempPlayerInfo.BadgeAssetPath = string.Empty;
			}
			this.SetSelectedSlot(index);
			this.UpdatePlayerSetupWidget(index);
		}
		protected void HidePlayerSetup()
		{
			this._camera.Active = true;
			this._shipCamera.Active = false;
			base.App.UI.SetVisible("pnlPlayerSetup", false);
			base.App.UI.SetVisible("ohStarMapLobby", true);
			this.HideShipPreview();
			this._inPlayerSetup = false;
		}
		protected void UpdatePlayerSetupWidget(int playerIndex)
		{
			this.UpdateShipColors(playerIndex);
			this.UpdateShipPreview(playerIndex);
			GameSetupUI.SyncPlayerSetupWidget(base.App, "pnlPlayerSetup", base.App.GameSetup.Players[playerIndex]);
		}
		private void CreateGame()
		{
			base.App.GameSetup.IsMultiplayer = true;
			base.App.Network.InLobby(false);
			base.App.SwitchGameState<GameSetupState>(new object[]
			{
				true
			});
		}
		private void LoadSinglePlayer()
		{
			base.App.ConfirmAI();
		}
		private void NewSinglePlayer()
		{
			base.App.NewGame();
			base.App.ConfirmAI();
		}
		public void ClearStatus()
		{
			StarMapLobbyState._selectedIndex = -1;
			base.App.UI.SetVisible("servers", true);
		}
		public void OnNetworkError()
		{
			this.Reset();
		}
		public void OnJoined()
		{
			this.JoinButtonState();
		}
		public void Reset()
		{
			this._enterState = LobbyEntranceState.Browser;
			StarMapLobbyState._selectedIndex = 0;
			this._selectedSlot = 0;
			this.EnableGameInProgress(false);
			this.EnablePlayerSetup(true);
			this.ClearStatus();
			GameSetupUI.ClearPlayerListWidget(base.App, "lstPlayers");
			this.HideShipPreview();
			this.HideColorPicker();
			this.HidePlayerSetup();
			if (base.App.GameSetup.IsMultiplayer)
			{
				this.MultiplayerButtonState();
				return;
			}
			this.SinglePlayerButtonState();
		}
		private void UpdateShipPreview(int index)
		{
			if (base.App.GameSetup.Players.Count <= index || index < 0)
			{
				return;
			}
			PlayerSetup player = base.App.GameSetup.Players[index];
			if (player.Faction == null || player.Faction == "")
			{
				return;
			}
			this._shipHoloView.HideViewport(false);
			this._tempPlayerInfo.SubfactionIndex = this.SelectedSubfactionIndex;
			this._tempPlayer = new Player(base.App, null, this._tempPlayerInfo, Player.ClientTypes.User);
			this._builder.Clear();
			this._builder.New(this._tempPlayer, 
				from x in base.App.AssetDatabase.ShipSections
				where x.Faction == player.Faction && x.Class == ShipClass.Cruiser && (x.FileName.Contains("cr_cmd.section") || x.FileName.Contains("cr_eng_fusion.section") || x.FileName.Contains("cr_mis_armor.section"))
				select x, base.App.AssetDatabase.TurretHousings, base.App.AssetDatabase.Weapons, null, null, null, null, null, null, new DesignSectionInfo[0], base.App.AssetDatabase.Factions.Where((Faction x) => x.Name == player.Faction).First<Faction>(), "", "");
		}
		private void HideColorPicker()
		{
			base.App.UI.SetVisible("dlgSelectEmpireColor", false);
			this._dlgSelectEmpireColorVisible = false;
			base.App.UI.SetVisible("dlgSelectShipColor", false);
			this._dlgSelectShipColorVisible = false;
		}
	}
}
