using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_NETWORK)]
	internal class Network : GameObject, IDisposable
	{
		private enum NetworkMessage
		{
			NM_LOBBY_GAME_FOUND,
			NM_START_GAME,
			NM_LOAD_GAME,
			NM_TRANSFER_PROGRESS,
			NM_START_COMBAT,
			NM_GAME_INFO,
			NM_GAME_NAME,
			NM_ERROR,
			NM_DB_GET,
			NM_DB_SET,
			NM_PLAYER_SET,
			NM_NEXT_TURN,
			NM_DB_HISTORY,
			NM_DB_APPLY_HISTORY,
			NM_REACTION_PHASE,
			NM_COMBAT_PHASE,
			NM_DIALOG,
			NM_PLAYER_DISCONNECTED,
			NM_PLAYER_CONNECTED,
			NM_POST_COMBAT,
			NM_SET_PROFILE_DATA,
			NM_REACTION_LIST,
			NM_COMBAT_LIST,
			NM_COMBAT_RESPONSE,
			NM_COMBAT_DATA,
			NM_SYNC_MULTI_COMBAT_DATA,
			NM_PLAYER_STATUS_CHANGED,
			NM_STARMAP_TIME,
			NM_REFRESH_COMPLETE,
			NM_SEND_IRC_MESSAGE,
			NM_IRC_RECONNECT
		}
		public enum DialogAction
		{
			DA_FINALIZE,
			DA_RAW_STRING,
			DA_LOGIN_CONNECTING,
			DA_LOGIN_CONNECTING_FAIL_GP,
			DA_LOGIN_CONNECTING_BAD_PASSWORD,
			DA_LOGIN_CONNECTING_BAD_USERNAME,
			DA_LOGIN_CONNECTING_LOGIN_IN_USE,
			DA_LOGIN_CONNECTING_SUCCESS_GP,
			DA_LOGIN_CONNECTING_CHAT,
			DA_LOGIN_CONNECTING_CHAT_FAILED,
			DA_LOGIN_CONNECTED,
			DA_NEWUSER_CREATING,
			DA_NEWUSER_PASSWORD_MISMATCH,
			DA_NEWUSER_INVALID_USERNAME,
			DA_NEWUSER_NICK_IN_USE,
			DA_NEWUSER_OFFLINE,
			DA_NEWUSER_INVALID_PASSWORD,
			DA_NEWUSER_FAILED,
			DA_NEWUSER_SUCCESS,
			DA_CONNECT_CONNECTING,
			DA_CONNECT_FAILED,
			DA_CONNECT_SUCCESS,
			DA_CONNECT_TIMED_OUT,
			DA_CONNECT_INVALID_PASSWORD,
			DA_CONNECT_NAT_FAILURE
		}
		private enum NetworkInteropMessage
		{
			NIM_LOBBY_REFRESH,
			NIM_LOBBY_REFRESH_BUDDIES,
			NIM_LOBBY_SERVER_SELECT,
			NIM_LOBBY_JOIN,
			NIM_LOBBY_DIRECT,
			NIM_LOBBY_HOST,
			NIM_LOBBY_DISCONNECT,
			NIM_LOBBY_KICK,
			NIM_LOBBY_STATE,
			NIM_LOBBY_LOGIN,
			NIM_LOBBY_UPDATE_GAME_INFO,
			NIM_LOBBY_PLAYER_INFO,
			NIM_LOBBY_GAME_START,
			NIM_LOBBY_GAME_INFO,
			NIM_LOBBY_READY,
			NIM_LOBBY_SLOT_CHANGE,
			NIM_LOBBY_SLOT_SWAP,
			NIM_LOBBY_SET_DATABASE,
			NIM_STARMAP_END_TURN,
			NIM_STARMAP_DB_HISTORY,
			NIM_STARMAP_REACTION_INFO,
			NIM_STARMAP_REACTION_COMPLETE,
			NIM_STARMAP_COMBAT_INFO,
			NIM_STARMAP_COMBAT_RESPONSES,
			NIM_STARMAP_TIME,
			NIM_COMBAT_CARRY_OVER_DATA,
			NIM_COMBAT_DATA,
			NIM_COMBAT_COMPLETE,
			NIM_NEWUSER,
			NIM_CHATWIDGET_ENABLED,
			NIM_CHATWIDGET_ENABLE_PLAYERS,
			NIM_CHATWIDGET_VISIBILITY,
			NIM_REQUEST_PLAYER_INFOS,
			NIM_LOG_PLAYER_INFO,
			NIM_HANDLE_IRC_MESSAGE,
			NIM_UPDATE_IRC_NICK
		}
		private enum SlotInfo
		{
			SLOT_NONE = -4,
			SLOT_CLOSED,
			SLOT_OPEN,
			SLOT_AI
		}
		private StarMapLobbyState _starMapLobby;
		private bool _gameLoaded;
		private bool _isHosting;
		private bool _isJoined;
		private bool _isOffline;
		private bool _isLoggedIn;
		private bool _dbLoaded;
		private Dictionary<int, List<string>> _dbBuffer = new Dictionary<int, List<string>>();
		private int _lastDBRow;
		private string _gameName = "";
		private bool _gameInfoUpdated;
		private List<ReactionInfo> _reactionList;
		private float _turnSeconds;
		private NetConnectionDialog _connectionDialog;
		private string _username;
		public string Username
		{
			get
			{
				return this._username;
			}
		}
		public float TurnSeconds
		{
			get
			{
				return this._turnSeconds;
			}
			set
			{
				this._turnSeconds = value;
			}
		}
		public StarMapLobbyState StarMapLobby
		{
			get
			{
				return this._starMapLobby;
			}
			set
			{
				this._starMapLobby = value;
			}
		}
		public bool IsHosting
		{
			get
			{
				return this._isHosting;
			}
			set
			{
				this._isHosting = value;
			}
		}
		public bool IsJoined
		{
			get
			{
				return this._isJoined;
			}
			set
			{
				this._isJoined = value;
			}
		}
		public bool IsOffline
		{
			get
			{
				return this._isOffline;
			}
			set
			{
				this._isOffline = value;
			}
		}
		public bool IsLoggedIn
		{
			get
			{
				return this._isLoggedIn;
			}
			set
			{
				this._isLoggedIn = value;
			}
		}
		public bool LoginRequired
		{
			get
			{
				return !this._isOffline && !this._isLoggedIn;
			}
		}
		public string GameName
		{
			get
			{
				return this._gameName;
			}
			set
			{
				this._gameName = value;
			}
		}
		public bool GameLoaded
		{
			get
			{
				return this._gameLoaded;
			}
			set
			{
				this._gameLoaded = value;
			}
		}
		public void PostLogPlayerInfo()
		{
			this.PostSetInt(33, new object[0]);
		}
		public void PostIRCChatMessage(string name, string message)
		{
			this.PostSetInt(34, new object[]
			{
				name,
				message
			});
		}
		public void PostIRCNick(string name)
		{
			this.PostSetInt(35, new object[]
			{
				name
			});
		}
		private void Reset()
		{
			this._gameLoaded = false;
			this._isHosting = false;
			this._isJoined = false;
			this._dbLoaded = false;
		}
		private void Trace(string message)
		{
			App.Log.Trace(message, "net");
		}
		public void SetChatWidgetVisibility(bool? visible = null)
		{
			if (visible.HasValue)
			{
				this.PostSetInt(31, new object[]
				{
					true,
					visible.Value
				});
				return;
			}
			this.PostSetInt(31, new object[]
			{
				false
			});
		}
		public void EnableChatWidgetPlayerList(bool enabled)
		{
			this.PostSetInt(30, new object[]
			{
				enabled
			});
		}
		public void EnableChatWidget(bool enabled)
		{
			this.PostSetInt(29, new object[]
			{
				enabled
			});
		}
		public void Initialize()
		{
			base.App.UI.PanelMessage += new UIEventPanelMessage(this.UI_PanelMessage);
		}
		public void Dispose()
		{
			if (base.App != null)
			{
				base.App.UI.PanelMessage -= new UIEventPanelMessage(this.UI_PanelMessage);
			}
		}
		private void UI_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_closed" && this._connectionDialog != null && panelName == this._connectionDialog.ID)
			{
				if (msgParams[0] == "True")
				{
					if (this._starMapLobby != null)
					{
						this._starMapLobby.OnJoined();
					}
				}
				else
				{
					this.Disconnect();
					if (this._starMapLobby != null)
					{
						this._starMapLobby.OnNetworkError();
					}
				}
				this._connectionDialog = null;
			}
		}
		private void NM_START_GAME_NewGame()
		{
			if (!this._gameLoaded)
			{
				base.App.NewGame();
			}
			else
			{
				base.App.GameSetup.IsMultiplayer = true;
			}
			base.App.ConfirmAI();
			base.App.GameDatabase.GetDatabaseHistory(out this._lastDBRow);
			if (base.App.Network != null && base.App.Network.IsHosting)
			{
				base.App.GameDatabase.SaveMultiplayerSyncPoint(base.App.CacheDir);
			}
			this.PostSetInt(17, new object[]
			{
				(int)base.App.GameDatabase.GetDbPointer()
			});
		}
		private bool ClientStartLoad()
		{
			return this._dbLoaded;
		}
		public void ProcessEngineMessage(ScriptMessageReader mr)
		{
			switch (mr.ReadInteger())
			{
			case 0:
			{
				uint num = (uint)mr.ReadInteger();
				uint num2 = (uint)mr.ReadInteger();
				ulong id = (ulong)num + ((ulong)num2 << 32);
				string name = mr.ReadString();
				string version = mr.ReadString();
				string map = mr.ReadString();
				int players = mr.ReadInteger();
				int maxPlayers = mr.ReadInteger();
				int ping = mr.ReadInteger();
				bool passworded = mr.ReadBool();
				List<PlayerSetup> list = new List<PlayerSetup>();
				int num3 = mr.ReadInteger();
				for (int i2 = 0; i2 < num3; i2++)
				{
					list.Add(new PlayerSetup
					{
						slot = i2,
						Name = mr.ReadString(),
						Avatar = mr.ReadString(),
						Badge = mr.ReadString(),
						Faction = mr.ReadString(),
						EmpireColor = new int?(mr.ReadInteger())
					});
				}
				if (this._starMapLobby != null)
				{
					this._starMapLobby.AddServer(id, name, map, version, players, maxPlayers, ping, passworded, list);
					return;
				}
				break;
			}
			case 1:
				if (this._isHosting)
				{
					base.App.StartGame(new Action(this.NM_START_GAME_NewGame), null, new object[0]);
					return;
				}
				break;
			case 2:
				if (base.App.CurrentState != base.App.GetGameState<LoadingScreenState>())
				{
					base.App.SwitchGameStateViaLoadingScreen(null, new LoadingFinishedDelegate(this.ClientStartLoad), base.App.GetGameState<StarMapState>(), new object[0]);
					return;
				}
				break;
			case 3:
				break;
			case 4:
			{
				this.Trace("Combat start message received.");
				int id2 = mr.ReadInteger();
				mr.ReadInteger();
				bool authority = mr.ReadBool();
				bool sim = mr.ReadBool();
				int num4 = mr.ReadInteger();
				PendingCombat pendingCombatByUniqueID = base.App.Game.GetPendingCombatByUniqueID(id2);
				if (pendingCombatByUniqueID != null)
				{
					for (int j = 0; j < num4; j++)
					{
						int key = mr.ReadInteger();
						int value = mr.ReadInteger();
						int value2 = mr.ReadInteger();
						int value3 = mr.ReadInteger();
						pendingCombatByUniqueID.SelectedPlayerFleets[key] = value;
						pendingCombatByUniqueID.CombatResolutionSelections[key] = (ResolutionType)value2;
						pendingCombatByUniqueID.CombatStanceSelections[key] = (AutoResolveStance)value3;
					}
					base.App.Game.LaunchCombat(pendingCombatByUniqueID, false, sim, authority);
					return;
				}
				break;
			}
			case 5:
			{
				GameSetup gameSetup = base.App.GameSetup;
				gameSetup.CombatTurnLength = mr.ReadSingle();
				gameSetup.StrategicTurnLength = mr.ReadSingle();
				int playerCount = mr.ReadInteger();
				int[] values = new int[]
				{
					mr.ReadInteger()
				};
				gameSetup._averageResources = mr.ReadInteger();
				gameSetup._economicEfficiency = mr.ReadInteger();
				gameSetup._grandMenaceCount = mr.ReadInteger();
				gameSetup._initialSystems = mr.ReadInteger();
				gameSetup._initialTechs = mr.ReadInteger();
				gameSetup._initialTreasury = mr.ReadInteger();
				gameSetup._randomEncounterFrequency = mr.ReadInteger();
				gameSetup._researchEfficiency = mr.ReadInteger();
				gameSetup._starCount = mr.ReadInteger();
				gameSetup._starSize = mr.ReadInteger();
				mr.ReadString();
				mr.ReadString();
				string gameName = mr.ReadString();
				this._gameName = gameName;
				int gameInProgressTurn = mr.ReadInteger();
				gameSetup._inProgress = mr.ReadBool();
				if (this._starMapLobby != null)
				{
					this._starMapLobby.EnablePlayerSetup(!gameSetup._inProgress);
					this._starMapLobby.GameInProgressTurn = gameInProgressTurn;
					this._starMapLobby.UpdateGameInProgress();
				}
				BitArray bitArray = new BitArray(values);
				gameSetup.AvailablePlayerFeatures.ReplaceFactions(
					from x in base.App.AssetDatabase.Factions
					where bitArray[x.ID]
					select x);
				gameSetup.SetPlayerCount(playerCount);
				base.App.GameSetup.IsMultiplayer = true;
				this._gameInfoUpdated = true;
				return;
			}
			case 6:
				this._gameName = mr.ReadString();
				return;
			case 7:
			{
				bool flag = mr.ReadBool();
				if (flag)
				{
					this.Reset();
					if (this._starMapLobby != null)
					{
						this._starMapLobby.OnNetworkError();
					}
					if (base.App.CurrentState != base.App.GetGameState<StarMapLobbyState>())
					{
						while (base.App.UI.GetTopDialog() != null)
						{
							base.App.UI.CloseDialog(base.App.UI.GetTopDialog(), true);
						}
						StarMapState gameState = base.App.GetGameState<StarMapState>();
						if (base.App.CurrentState == gameState)
						{
							gameState.Reset();
						}
						base.App.SwitchGameState<MainMenuState>(new object[0]);
					}
				}
				if (this._connectionDialog == null)
				{
					base.App.UI.CreateDialog(new GenericTextDialog(base.App, "Error!", mr.ReadString() + ".", "dialogGenericMessage"), null);
					return;
				}
				break;
			}
			case 8:
				base.App.GameDatabase.GetDatabaseHistory(out this._lastDBRow);
				if (base.App.Network != null && base.App.Network.IsHosting)
				{
					base.App.GameDatabase.SaveMultiplayerSyncPoint(base.App.CacheDir);
				}
				this.PostSetInt(17, new object[]
				{
					(int)base.App.GameDatabase.GetDbPointer()
				});
				return;
			case 9:
			{
				int num5 = mr.ReadInteger();
				IntPtr source = (IntPtr)mr.ReadInteger();
				byte[] array = new byte[num5];
				Marshal.Copy(source, array, 0, num5);
				BinaryWriter binaryWriter = new BinaryWriter(File.Open(base.App.CacheDir + "\\client.db", FileMode.Create));
				binaryWriter.Write(array);
				binaryWriter.Close();
				base.App.LoadGame(base.App.CacheDir + "\\client.db", base.App.GameSetup);
				base.App.GameDatabase.GetDatabaseHistory(out this._lastDBRow);
				this._dbLoaded = true;
				if (base.App.GameSetup._inProgress)
				{
					base.App.Game.State = SimState.SS_COMBAT;
					return;
				}
				break;
			}
			case 10:
			{
				int num6 = mr.ReadInteger();
				int? num7 = null;
				if (base.App.GameSetup.LocalPlayer != null)
				{
					num7 = new int?(base.App.GameSetup.LocalPlayer.slot);
				}
				if (num6 >= 0 && num6 < base.App.GameSetup.Players.Count)
				{
					if (base.App.GameSetup.Players[num6] == null)
					{
						base.App.GameSetup.Players[num6] = new PlayerSetup();
					}
					PlayerSetup playerSetup = base.App.GameSetup.Players[num6];
					string avatar = playerSetup.Avatar;
					string badge = playerSetup.Badge;
					string faction = playerSetup.Faction;
					playerSetup.localPlayer = mr.ReadBool();
					int value4 = mr.ReadInteger();
					playerSetup.Name = mr.ReadString();
					playerSetup.EmpireName = mr.ReadString();
					playerSetup.Faction = mr.ReadString();
					playerSetup.SubfactionIndex = mr.ReadInteger();
					string avatar2 = mr.ReadString();
					string badge2 = mr.ReadString();
					playerSetup.slot = num6;
					playerSetup.InitialColonies = mr.ReadInteger();
					playerSetup.InitialTechs = mr.ReadInteger();
					playerSetup.InitialTreasury = mr.ReadInteger();
					playerSetup.ShipColor = new Vector3(mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle());
					playerSetup.AI = mr.ReadBool();
					playerSetup.Status = (NPlayerStatus)mr.ReadInteger();
					playerSetup.Fixed = mr.ReadBool();
					playerSetup.Locked = mr.ReadBool();
					playerSetup.Ready = (playerSetup.Status == NPlayerStatus.PS_READY);
					playerSetup.Team = mr.ReadInteger();
					if (faction != playerSetup.Faction)
					{
						base.App.GameSetup.ReplaceAvatar(faction, avatar);
						base.App.GameSetup.ReplaceBadge(faction, badge);
					}
					base.App.GameSetup.SetEmpireColor(num6, new int?(value4));
					base.App.GameSetup.SetAvatar(num6, playerSetup.Faction, avatar2);
					base.App.GameSetup.SetBadge(num6, playerSetup.Faction, badge2);
					GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", base.App.GameSetup.Players, true);
					if (base.App.Game != null)
					{
						Player player = base.App.GetPlayer(base.App.GameSetup.Players[num6].databaseId);
						if (player != null && player.IsAI() != playerSetup.AI)
						{
							player.SetAI(playerSetup.AI);
							this.Trace(string.Concat(new object[]
							{
								"Setting Player AI on player ",
								num6,
								" to ",
								playerSetup.AI
							}));
						}
					}
					if (playerSetup.localPlayer)
					{
						if (base.App.Game != null && base.App.Game.LocalPlayer.ID != playerSetup.databaseId)
						{
							Player player2 = base.App.GetPlayer(base.App.GameSetup.Players[num6].databaseId);
							if (player2 != null)
							{
								base.App.Game.SetLocalPlayer(player2);
							}
						}
						if (this._starMapLobby != null)
						{
							if (playerSetup.Fixed)
							{
								this._starMapLobby.EnablePlayerSetup(false);
							}
							else
							{
								this._starMapLobby.EnablePlayerSetup(true);
							}
							if (num7.HasValue && num7.Value != num6)
							{
								this._starMapLobby.ShowPlayerSetup(num6);
							}
							this._starMapLobby.IsLocalPlayerReady = playerSetup.Ready;
							if (this._gameInfoUpdated)
							{
								this._starMapLobby.EnableGameInProgress(base.App.GameSetup._inProgress);
							}
							this._starMapLobby.UpdateGameInProgress();
						}
					}
					if (this._starMapLobby != null && (playerSetup.localPlayer || (this.IsHosting && this._starMapLobby.SelectedSlot == num6)))
					{
						this._starMapLobby.ShowPlayerSetup(num6);
						this._gameInfoUpdated = false;
						return;
					}
				}
				break;
			}
			case 11:
				this._turnSeconds = 0f;
				if (base.App.Game == null)
				{
					return;
				}
				base.App.Game.GetPendingCombats().Clear();
				if (this._isHosting)
				{
					base.App.Game.NextTurn();
					this.Trace("Third/First sync sent!");
					base.App.GameDatabase.LogComment("SYNC 3/1 (NM_NEXT_TURN)");
					this.SendHistory(base.App.GameDatabase.GetTurnCount() - 1);
					return;
				}
				break;
			case 12:
				this.SendHistory(base.App.GameDatabase.GetTurnCount());
				return;
			case 13:
			{
				int key2 = mr.ReadInteger();
				int num8 = mr.ReadInteger();
				if (base.App.GameDatabase != null)
				{
					int turnCount = base.App.GameDatabase.GetTurnCount();
					List<string> list2 = new List<string>();
					for (int k = 0; k < num8; k++)
					{
						list2.Add(mr.ReadString());
					}
					Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>();
					dictionary[key2] = list2;
					base.App.GameDatabase.ExecuteDatabaseHistory(dictionary);
					base.App.GameDatabase.SetClientId(base.App.LocalPlayer.ID);
					int turnCount2 = base.App.GameDatabase.GetTurnCount();
					base.App.GameDatabase.GetDatabaseHistoryForTurn(turnCount2, out this._lastDBRow, new int?(this._lastDBRow));
					if (turnCount != turnCount2)
					{
						base.App.Game.NextTurn();
					}
					this.Trace("Database history applied.");
					return;
				}
				if (!this._dbBuffer.ContainsKey(key2))
				{
					this._dbBuffer[key2] = new List<string>();
					return;
				}
				for (int l = 0; l < num8; l++)
				{
					this._dbBuffer[key2].Add(mr.ReadString());
				}
				return;
			}
			case 14:
			{
				base.App.Game.ProcessMidTurn();
				base.App.GameDatabase.LogComment("SYNC 2 (NM_REACTION_PHASE)");
				this.SendHistory(base.App.GameDatabase.GetTurnCount());
				this.Trace("Second sync sent!");
				List<ReactionInfo> source2 = base.App.Game.GetPendingReactions();
				source2 = (
					from x in source2
					where !base.App.GetPlayer(x.fleet.PlayerID).IsAI()
					select x).ToList<ReactionInfo>();
				List<object> list3 = new List<object>();
				list3.Add(source2.Count<ReactionInfo>());
				int i;
				for (i = 1; i <= 8; i++)
				{
					IEnumerable<ReactionInfo> enumerable = 
						from x in source2
						where x.fleet.PlayerID == i
						select x;
					if (enumerable.Count<ReactionInfo>() > 0)
					{
						list3.Add(i);
						list3.Add(enumerable.Count<ReactionInfo>());
						foreach (ReactionInfo current in enumerable)
						{
							list3.Add(current.fleet.ID);
							list3.Add(current.fleetsInRange.Count<FleetInfo>());
							foreach (FleetInfo current2 in current.fleetsInRange)
							{
								list3.Add(current2.ID);
							}
						}
					}
				}
				this.PostSetInt(20, list3.ToArray());
				return;
			}
			case 15:
			{
				base.App.Game.Phase4_Combat();
				base.App.GameDatabase.LogComment("SYNC 3 (NM_COMBAT_PHASE)");
				this.SendHistory(base.App.GameDatabase.GetTurnCount());
				this.Trace("Third sync sent!");
				List<PendingCombat> pendingCombats = base.App.Game.GetPendingCombats();
				List<object> list4 = new List<object>();
				list4.Add(pendingCombats.Count);
				foreach (PendingCombat current3 in pendingCombats)
				{
					list4.Add(current3.ConflictID);
					list4.Add(current3.SystemID);
					list4.Add((int)current3.Type);
					List<int> list5 = new List<int>();
					foreach (int current4 in current3.PlayersInCombat)
					{
						PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(current4);
						if (playerInfo != null && playerInfo.isStandardPlayer)
						{
							list5.Add(current4);
						}
					}
					list4.Add(list5.Count<int>());
					list4.AddRange(list5.Cast<object>());
				}
				this.PostSetInt(22, list4.ToArray());
				return;
			}
			case 16:
			{
				string title = mr.ReadString();
				string text = mr.ReadString();
				base.App.UI.CreateDialog(new GenericTextDialog(base.App, title, text, "dialogGenericMessage"), null);
				return;
			}
			case 17:
			{
				int index = mr.ReadInteger();
				base.App.GameSetup.Players[index].Name = "";
				base.App.GameSetup.Players[index].AI = true;
				if (base.App.CurrentState == base.App.GetGameState<StarMapLobbyState>())
				{
					GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", base.App.GameSetup.Players, true);
					return;
				}
				Player player3 = base.App.GetPlayer(base.App.GameSetup.Players[index].databaseId);
				if (player3 != null)
				{
					player3.ReplaceWithAI();
					base.App.GameSetup.Players[index].Name = "";
					this.Trace("PLAYER DISCONNECTED - Setting AI, ID: " + player3.ID);
					return;
				}
				break;
			}
			case 18:
			{
				int num9 = mr.ReadInteger();
				Player player4 = base.App.GetPlayer(base.App.GameSetup.Players[num9].databaseId);
				if (this._gameLoaded || base.App.CurrentState != base.App.GetGameState<StarMapLobbyState>())
				{
					int? num10 = base.App.GameDatabase.GetLastClientPlayerID(base.App.GameSetup.Players[num9].Name) - 1;
					if (num10.HasValue && base.App.GameSetup.Players[num10.Value].AI && num9 != num10 && !base.App.GameSetup.Players[num10.Value].Locked)
					{
						this.ChangeSlot(num9, num10.Value);
					}
				}
				if (player4 != null && player4.IsAI())
				{
					player4.SetAI(false);
					base.App.GameSetup.Players[num9].AI = false;
					this.Trace("PLAYER CONNECTED - Disabling AI, ID: " + player4.ID);
					return;
				}
				break;
			}
			case 19:
				this.EndTurn();
				return;
			case 20:
				if (base.App.UserProfile != null && base.App.UserProfile.Loaded)
				{
					base.App.UserProfile.Username = mr.ReadString();
					base.App.UserProfile.Password = mr.ReadString();
					base.App.UserProfile.SaveProfile();
					return;
				}
				break;
			case 21:
			{
				if (this._reactionList == null)
				{
					this._reactionList = new List<ReactionInfo>();
				}
				else
				{
					this._reactionList.Clear();
				}
				int num11 = mr.ReadInteger();
				for (int m = 0; m < num11; m++)
				{
					ReactionInfo reactionInfo = new ReactionInfo();
					reactionInfo.fleetsInRange = new List<FleetInfo>();
					int fleetID = mr.ReadInteger();
					reactionInfo.fleet = base.App.GameDatabase.GetFleetInfo(fleetID);
					int num12 = mr.ReadInteger();
					for (int n = 0; n < num12; n++)
					{
						int fleetID2 = mr.ReadInteger();
						reactionInfo.fleetsInRange.Add(base.App.GameDatabase.GetFleetInfo(fleetID2));
					}
					this._reactionList.Add(reactionInfo);
				}
				base.App.Game.SetPendingReactions(this._reactionList);
				base.App.UI.CreateDialog(new ReactionDialog(base.App, this._reactionList.First<ReactionInfo>()), null);
				return;
			}
			case 22:
			{
				List<PendingCombat> list6 = new List<PendingCombat>();
				Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
				int num13 = mr.ReadInteger();
				for (int num14 = 0; num14 < num13; num14++)
				{
					int num15 = mr.ReadInteger();
					int conflictID = mr.ReadInteger();
					int type = mr.ReadInteger();
					int cardID = 1;
					if (dictionary2.ContainsKey(num15))
					{
						cardID = dictionary2[num15] + 1;
						Dictionary<int, int> dictionary3;
						int key3;
						(dictionary3 = dictionary2)[key3 = num15] = dictionary3[key3] + 1;
					}
					else
					{
						dictionary2.Add(num15, 1);
					}
					List<PendingCombat> arg_1517_0 = list6;
					PendingCombat pendingCombat = new PendingCombat();
					pendingCombat.CardID = cardID;
					pendingCombat.SystemID = num15;
					pendingCombat.ConflictID = conflictID;
					pendingCombat.Type = (CombatType)type;
					pendingCombat.PlayersInCombat = (
						from x in GameSession.GetPlayersWithCombatAssets(base.App, num15)
						select x.ID).ToList<int>();
					pendingCombat.FleetIDs = (
						from x in base.App.GameDatabase.GetFleetInfoBySystemID(num15, FleetType.FL_NORMAL)
						select x.ID).ToList<int>();
					pendingCombat.NPCPlayersInCombat = base.App.GameDatabase.GetNPCPlayersBySystem(num15);
					arg_1517_0.Add(pendingCombat);
				}
				foreach (PendingCombat cmb in list6.ToList<PendingCombat>())
				{
					if (cmb.Type == CombatType.CT_Piracy)
					{
						List<int> PlayersToExclude = new List<int>();
						List<int> list7 = new List<int>();
						foreach (int current5 in cmb.FleetIDs.ToList<int>())
						{
							MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(current5);
							if (missionByFleetID != null && missionByFleetID.Type == MissionType.PIRACY)
							{
								if (!base.App.GameDatabase.GetPiracyFleetDetectionInfoForFleet(current5).Any((PiracyFleetDetectionInfo x) => cmb.PlayersInCombat.Any(y => y == x.PlayerID)))
								{
									FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(current5);
									PlayersToExclude.Add(fleetInfo.PlayerID);
									list7.Add(current5);
								}
							}
						}
						cmb.FleetIDs = (
							from x in base.App.GameDatabase.GetFleetInfoBySystemID(cmb.SystemID, FleetType.FL_NORMAL)
							where !PlayersToExclude.Any((int h) => h == x.PlayerID)
							select x.ID).ToList<int>();
						cmb.FleetIDs.AddRange(list7);
					}
					if (cmb.CardID > 1)
					{
						foreach (int current6 in cmb.FleetIDs)
						{
							FleetInfo fleetInfo2 = base.App.GameDatabase.GetFleetInfo(current6);
							PlayerInfo playerInfo2 = base.App.GameDatabase.GetPlayerInfo(fleetInfo2.PlayerID);
							if (!playerInfo2.isStandardPlayer)
							{
								list6.Remove(cmb);
							}
						}
					}
				}
				base.App.Game.GetPendingCombats().Clear();
				base.App.Game.GetPendingCombats().AddRange(list6);
				if (list6.Any((PendingCombat x) => x.PlayersInCombat.Contains(base.App.Game.LocalPlayer.ID)))
				{
					base.App.UI.CreateDialog(new EncounterDialog(base.App, list6), null);
					return;
				}
				base.App.Game.ShowCombatDialog(true, null);
				return;
			}
			case 23:
			{
				int key4 = mr.ReadInteger();
				int num16 = mr.ReadInteger();
				for (int num17 = 0; num17 < num16; num17++)
				{
					int id3 = mr.ReadInteger();
					mr.ReadInteger();
					int value5 = mr.ReadInteger();
					int value6 = mr.ReadInteger();
					int value7 = mr.ReadInteger();
					PendingCombat pendingCombatByUniqueID2 = base.App.Game.GetPendingCombatByUniqueID(id3);
					if (pendingCombatByUniqueID2 != null)
					{
						pendingCombatByUniqueID2.SelectedPlayerFleets[key4] = value5;
						pendingCombatByUniqueID2.CombatStanceSelections[key4] = (AutoResolveStance)value7;
						pendingCombatByUniqueID2.CombatResolutionSelections[key4] = (ResolutionType)value6;
					}
				}
				return;
			}
			case 24:
			{
				if (base.App.Game == null)
				{
					App.Log.Warn("Ignoring NM_COMBAT_DATA because we're not in the game yet.", "net");
					return;
				}
				base.App.Game.CombatData.AddCombat(mr, 1);
				CombatData lastCombat = base.App.Game.CombatData.GetLastCombat();
				if (base.App.Network.IsHosting)
				{
					base.App.GameDatabase.InsertCombatData(lastCombat.SystemID, lastCombat.CombatID, lastCombat.Turn, lastCombat.ToByteArray());
					return;
				}
				break;
			}
			case 25:
			{
				if (base.App.Game == null)
				{
					App.Log.Warn("Ignoring NM_SYNC_MULTI_COMBAT_DATA because we're not in the game yet.", "net");
					return;
				}
				int systemId = mr.ReadInteger();
				int num18 = mr.ReadInteger();
				List<int> list8 = new List<int>();
				for (int num19 = 0; num19 < num18; num19++)
				{
					list8.Add(mr.ReadInteger());
				}
				base.App.Game.MCCarryOverData.AddCarryOverCombatZoneInfo(systemId, list8);
				int num20 = mr.ReadInteger();
				for (int num21 = 0; num21 < num20; num21++)
				{
					int num22 = mr.ReadInteger();
					int retreatFleetId = mr.ReadInteger();
					base.App.Game.MCCarryOverData.SetRetreatFleetID(systemId, num22, retreatFleetId);
					int num23 = mr.ReadInteger();
					for (int num24 = 0; num24 < num23; num24++)
					{
						int shipId = mr.ReadInteger();
						float x2 = mr.ReadSingle();
						float y = mr.ReadSingle();
						float z = mr.ReadSingle();
						float yawRadians = mr.ReadSingle();
						float pitchRadians = mr.ReadSingle();
						float rollRadians = mr.ReadSingle();
						Matrix endShipTransform = Matrix.CreateRotationYPR(yawRadians, pitchRadians, rollRadians);
						endShipTransform.Position = new Vector3(x2, y, z);
						base.App.Game.MCCarryOverData.AddCarryOverInfo(systemId, num22, shipId, endShipTransform);
					}
				}
				return;
			}
			case 26:
			{
				int num25 = mr.ReadInteger();
				NPlayerStatus nPlayerStatus = (NPlayerStatus)mr.ReadInteger();
				if (num25 >= 0 && num25 < base.App.GameSetup.Players.Count && base.App.GameSetup.Players[num25].Status != NPlayerStatus.PS_DEFEATED)
				{
					this.Trace(string.Concat(new object[]
					{
						"Setting status on player ",
						num25,
						" to ",
						nPlayerStatus
					}));
					base.App.GameSetup.Players[num25].Status = nPlayerStatus;
					base.App.GameSetup.Players[num25].Ready = (nPlayerStatus == NPlayerStatus.PS_READY);
					if (base.App.CurrentState == base.App.GetGameState<StarMapLobbyState>())
					{
						GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", base.App.GameSetup.Players, true);
						return;
					}
				}
				break;
			}
			case 27:
			{
				this._turnSeconds = mr.ReadSingle();
				float strategicTurnLength = mr.ReadSingle();
				if (base.App.Game != null)
				{
					base.App.Game.TurnTimer.StrategicTurnLength = strategicTurnLength;
					return;
				}
				break;
			}
			case 28:
			{
				StarMapLobbyState gameState2 = base.App.GetGameState<StarMapLobbyState>();
				if (base.App.CurrentState == gameState2)
				{
					gameState2.OnRefreshComplete();
					return;
				}
				break;
			}
			case 29:
			{
				string msg = mr.ReadString();
				base.App.IRC.SendChatMessage(msg);
				return;
			}
			case 30:
				if (base.App.IRC.irc.IsConnected)
				{
					base.App.IRC.Disconnect();
					this.PostIRCChatMessage(string.Empty, "*** Disconnected.");
					return;
				}
				base.App.IRC.SetupIRCClient(base.App.UserProfile.ProfileName);
				break;
			default:
				return;
			}
		}
		public void RefreshServers(bool stop)
		{
			this.PostSetInt(0, new object[]
			{
				stop
			});
		}
		public void RequestPlayerInformation()
		{
			this.PostSetInt(32, new object[0]);
		}
		public void RefreshBuddies()
		{
			this.PostSetInt(1, new object[0]);
		}
		public void Host()
		{
			this._isHosting = true;
			this.PostSetInt(5, new object[0]);
		}
		public void Login(string username)
		{
			this._username = username;
			this.PostSetInt(9, new object[]
			{
				username
			});
		}
		public void NewUser(string guid, string email, string user, string password)
		{
			this.PostSetInt(28, new object[]
			{
				guid,
				email,
				user,
				password
			});
		}
		public void InLobby(bool val)
		{
			this.PostSetInt(8, new object[]
			{
				val
			});
		}
		public void UpdateGameInfo(string gameName, string gamePass)
		{
			this.PostSetInt(10, new object[]
			{
				gameName,
				gamePass
			});
		}
		public void SetGameInfo(GameSetup setup)
		{
			string text = setup.HasStarMapFile() ? setup.StarMapFile : string.Empty;
			if (text != string.Empty)
			{
				text = Path.GetFileNameWithoutExtension(text);
			}
			string text2 = setup.HasScenarioFile() ? setup.ScenarioFile : string.Empty;
			if (text2 != string.Empty)
			{
				text2 = Path.GetFileNameWithoutExtension(text2);
			}
			int arg_5A_0 = setup.StarCount;
			int arg_66_0 = setup.Players.Count;
			List<Faction> list = setup.AvailablePlayerFeatures.Factions.Keys.ToList<Faction>();
			BitArray bitArray = new BitArray(32);
			foreach (Faction current in list)
			{
				int? factionID = base.App.GetFactionID(current.Name);
				if (factionID.HasValue && factionID.Value < 32)
				{
					bitArray[factionID.Value] = true;
				}
			}
			int[] array = new int[1];
			bitArray.CopyTo(array, 0);
			int[] array2 = Enumerable.Repeat<int>(-4, 8).ToArray<int>();
			int num = 0;
			foreach (PlayerSetup current2 in setup.Players)
			{
				if (current2.Locked)
				{
					array2[num] = -3;
				}
				else
				{
					if (current2.AI)
					{
						array2[num] = -1;
					}
					else
					{
						array2[num] = -2;
					}
				}
				num++;
			}
			this.PostSetInt(13, new object[]
			{
				setup.CombatTurnLength,
				setup.StrategicTurnLength,
				setup.Players.Count,
				array[0],
				setup._averageResources,
				setup._economicEfficiency,
				setup._grandMenaceCount,
				setup._initialSystems,
				setup._initialTechs,
				setup._initialTreasury,
				setup._randomEncounterFrequency,
				setup._researchEfficiency,
				setup._starCount,
				setup._starSize,
				setup._inProgress,
				text,
				text2,
				array2[0],
				array2[1],
				array2[2],
				array2[3],
				array2[4],
				array2[5],
				array2[6],
				array2[7]
			});
		}
		public void SetPlayerInfo(PlayerSetup ps, int slot)
		{
			if (ps != null)
			{
				if (this._gameLoaded)
				{
					ps.Fixed = true;
				}
				if (ps.Faction == "" || ps.Faction == null)
				{
					throw new Exception("Faction set as null or empty.");
				}
				this.PostSetInt(11, new object[]
				{
					slot,
					ps.Faction ?? "",
					ps.SubfactionIndex,
					ps.Avatar ?? "",
					ps.Badge ?? "",
					ps.EmpireColor ?? -1,
					ps.InitialColonies,
					ps.InitialTechs,
					ps.InitialTreasury,
					ps.AI,
					ps.localPlayer,
					ps.ShipColor.X,
					ps.ShipColor.Y,
					ps.ShipColor.Z,
					ps.EmpireName,
					ps.Fixed,
					ps.Locked,
					ps.Team
				});
			}
		}
		public void JoinGame(ulong serverID, string password = "")
		{
			this._connectionDialog = new NetConnectionDialog(base.App, "Connecting", "", "dialogNetConnection");
			base.App.UI.CreateDialog(this._connectionDialog, null);
			uint num = (uint)serverID;
			uint num2 = (uint)(serverID >> 32);
			this._isJoined = true;
			this.PostSetInt(3, new object[]
			{
				(int)num,
				(int)num2,
				password,
				this._connectionDialog.ID
			});
		}
		public void SelectServer(int index)
		{
			this.PostSetInt(2, new object[]
			{
				index
			});
		}
		public bool DirectConnect(string address, string password)
		{
			this._connectionDialog = new NetConnectionDialog(base.App, "Connecting", "", "dialogNetConnection");
			base.App.UI.CreateDialog(this._connectionDialog, null);
			bool result;
			try
			{
				string[] array = address.Split(new char[]
				{
					':'
				});
				IPHostEntry hostEntry = Dns.GetHostEntry(array[0]);
				IPAddress iPAddress = hostEntry.AddressList.FirstOrDefault((IPAddress x) => x.AddressFamily == AddressFamily.InterNetwork);
				if (iPAddress != null)
				{
					string text = iPAddress.ToString();
					if (array.Count<string>() > 1)
					{
						text = text + ':' + array[1];
					}
					this._isJoined = true;
					this.PostSetInt(4, new object[]
					{
						text,
						password,
						this._connectionDialog.ID
					});
					result = true;
				}
				else
				{
					result = false;
				}
			}
			catch (SocketException)
			{
				this._connectionDialog.AddString("Unable connect to Host: Address Invalid.");
				result = false;
			}
			return result;
		}
		public void LoadGame()
		{
			this.PostSetInt(12, new object[0]);
		}
		public void Ready()
		{
			this.PostSetInt(14, new object[0]);
		}
		public void SetSlot(int index)
		{
			this.PostSetInt(16, new object[]
			{
				index
			});
		}
		public void ChangeSlot(int firstSlot, int secondSlot)
		{
			this.PostSetInt(15, new object[]
			{
				firstSlot,
				secondSlot
			});
		}
		public void Disconnect()
		{
			this.Reset();
			this.PostSetInt(6, new object[0]);
		}
		public void Kick(int index)
		{
			this.PostSetInt(7, new object[]
			{
				index
			});
		}
		public void DatabaseLoaded()
		{
			foreach (KeyValuePair<int, List<string>> arg_15_0 in this._dbBuffer)
			{
				base.App.GameDatabase.ExecuteDatabaseHistory(this._dbBuffer);
				base.App.GameDatabase.SetClientId(base.App.LocalPlayer.ID);
			}
			this._dbBuffer.Clear();
		}
		public void EndTurn()
		{
			if (base.App.GameDatabase == null)
			{
				return;
			}
			int turnCount = base.App.GameDatabase.GetTurnCount();
			this.SendHistoryCore(turnCount, true);
		}
		public void SendHistory(int turn)
		{
			this.SendHistoryCore(turn, false);
		}
		private void SendHistoryCore(int turn, bool endTurn)
		{
			if (base.App.GameDatabase == null)
			{
				return;
			}
			string[] array = base.App.GameDatabase.GetDatabaseHistoryForTurn(turn, out this._lastDBRow, new int?(this._lastDBRow)).ToArray();
			List<object> list = new List<object>();
			list.Add(turn);
			list.Add(base.App.LocalPlayer.ID);
			list.Add(array.Length);
			list.AddRange(array);
			Network.NetworkInteropMessage networkInteropMessage = endTurn ? Network.NetworkInteropMessage.NIM_STARMAP_END_TURN : Network.NetworkInteropMessage.NIM_STARMAP_DB_HISTORY;
			this.Trace(string.Format("{0} Submitting {1} lines of DB history as {2}.", networkInteropMessage, array.Length, base.App.GameDatabase.GetClientId()));
			this.PostSetInt((int)networkInteropMessage, list.ToArray());
		}
		public void SendCombatResponses(IEnumerable<PendingCombat> responses, int playerId)
		{
			if (responses.Count<PendingCombat>() == 0)
			{
				return;
			}
			List<object> list = new List<object>();
			IEnumerable<PendingCombat> enumerable = 
				from x in responses
				where x.PlayersInCombat.Contains(playerId)
				select x;
			if (enumerable.Count<PendingCombat>() == 0)
			{
				return;
			}
			list.Add(playerId);
			list.Add(enumerable.Count<PendingCombat>());
			foreach (PendingCombat current in enumerable)
			{
				list.Add(current.ConflictID);
				list.Add(current.SystemID);
				list.Add(current.SelectedPlayerFleets[playerId]);
				list.Add((int)current.CombatResolutionSelections[playerId]);
				list.Add((int)current.CombatStanceSelections[playerId]);
			}
			this.PostSetInt(23, list.ToArray());
		}
		public void SetTime(float current, float max)
		{
			if (this.IsHosting && base.App.CurrentState != base.App.GetGameState<MainMenuState>() && base.App.CurrentState != base.App.GetGameState<StarMapLobbyState>())
			{
				this.PostSetInt(24, new object[]
				{
					current,
					max
				});
			}
		}
		public void ReactionComplete()
		{
			this.PostSetInt(21, new object[0]);
		}
		public void SendCarryOverData(List<object> parms)
		{
			this.PostSetInt(25, parms.ToArray());
		}
		public void SendCombatData(CombatData cd)
		{
			this.PostSetInt(26, cd.ToList().ToArray());
		}
		public void CombatComplete(int systemId)
		{
			this.PostSetInt(27, new object[]
			{
				systemId
			});
		}
	}
}
