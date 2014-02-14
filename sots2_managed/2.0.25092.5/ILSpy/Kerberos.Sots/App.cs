using Kerberos.Sots.Console;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.IRC;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace Kerberos.Sots
{
	internal class App
	{
		private class StartLocation
		{
			public StarSystem System;
			public IStellarEntity Planet;
			public int PlayerIndex = -1;
		}
		private readonly IFileSystem _fileSystem;
		private readonly GameObjectMediator _gameObjectMediator;
		private readonly GameStateMachine _gameStateMachine;
		private readonly ScriptCommChannel _scriptCommChannel;
		private readonly UICommChannel _uiCommChannel;
		private ISteam _steam;
		private SteamHelper _steamHelper;
		private Network _network;
		private bool _materialsReady;
		private bool _initialized;
		private bool _receivedDirectoryInfo;
		private GameSetup _gameSetup;
		private string _gameRoot;
		private string _profileDir;
		private string _baseSaveDir;
		private string _cacheDir;
		private string _settingsDir;
		private bool _engineExitRequested;
		private AssetDatabase _assetDatabase;
		private ConsoleApplet _consoleApplet;
		private static Random _safeRandom;
		private Profile _userProfile;
		private Settings _gameSettings;
		private string _consoleScriptFileName;
		private SotsIRC _ircChat;
		private HotKeyManager _hotkeyManager;
		private bool _profileSelected;
		private static List<NonClosingStreamWrapper> locks = new List<NonClosingStreamWrapper>();
		private int _numExceptionErrorsDisplayed;
		private static GameUICommands m_Commands;
		private GameSession game;
		public static bool m_bAI_Enabled;
		public static bool m_bPlayerAI_Enabled;
		public static bool m_bDebugFup;
		public event Action<IGameObject> ObjectReleased;
		public SteamHelper SteamHelper
		{
			get
			{
				return this._steamHelper;
			}
		}
		public ISteam Steam
		{
			get
			{
				return this._steam;
			}
		}
		public IList<TurnEvent> TurnEvents
		{
			get
			{
				return this.game.TurnEvents;
			}
		}
		public Network Network
		{
			get
			{
				return this._network;
			}
		}
		public SotsIRC IRC
		{
			get
			{
				return this._ircChat;
			}
		}
		public string CacheDir
		{
			get
			{
				return this._cacheDir;
			}
		}
		public string SettingsDir
		{
			get
			{
				return this._settingsDir;
			}
		}
		public IFileSystem FileSystem
		{
			get
			{
				return this._fileSystem;
			}
		}
		public static Log Log
		{
			get
			{
				return ScriptHost.Log;
			}
		}
		public string GameRoot
		{
			get
			{
				return this._gameRoot;
			}
		}
		public string ProfileDir
		{
			get
			{
				return this._profileDir;
			}
		}
		public string SaveDir
		{
			get
			{
				if (this._userProfile != null && this._userProfile.Loaded)
				{
					string text = this._baseSaveDir + Path.DirectorySeparatorChar + this._userProfile.ProfileName;
					if (!Directory.Exists(text))
					{
						Directory.CreateDirectory(text);
					}
					return text;
				}
				return this._baseSaveDir;
			}
		}
		public string BaseSaveDir
		{
			get
			{
				return this._baseSaveDir;
			}
		}
		public UICommChannel UI
		{
			get
			{
				return this._uiCommChannel;
			}
		}
		public AssetDatabase AssetDatabase
		{
			get
			{
				return this._assetDatabase;
			}
		}
		public GameState CurrentState
		{
			get
			{
				return this._gameStateMachine.CurrentState;
			}
		}
		public GameState PendingState
		{
			get
			{
				return this._gameStateMachine.PendingState;
			}
		}
		public GameState PreviousState
		{
			get
			{
				return this._gameStateMachine.PreviousState;
			}
		}
		public IEnumerable<GameState> States
		{
			get
			{
				return this._gameStateMachine;
			}
		}
		public GameSetup GameSetup
		{
			get
			{
				return this._gameSetup;
			}
		}
		public Profile UserProfile
		{
			get
			{
				return this._userProfile;
			}
			set
			{
				this._userProfile = value;
				this._profileSelected = true;
				this._ircChat.SetNick(value.ProfileName);
			}
		}
		public HotKeyManager HotKeyManager
		{
			get
			{
				return this._hotkeyManager;
			}
			set
			{
			}
		}
		public Settings GameSettings
		{
			get
			{
				return this._gameSettings;
			}
		}
		public bool ProfileSelected
		{
			get
			{
				return this._profileSelected;
			}
			set
			{
				this._profileSelected = value;
			}
		}
		public GameDatabase GameDatabase
		{
			get
			{
				if (this.game == null)
				{
					return null;
				}
				return this.game.GameDatabase;
			}
		}
		public GameSession Game
		{
			get
			{
				return this.game;
			}
		}
		public Kerberos.Sots.PlayerFramework.Player LocalPlayer
		{
			get
			{
				if (this.game == null)
				{
					return null;
				}
				return this.game.LocalPlayer;
			}
		}
		public static GameUICommands Commands
		{
			get
			{
				return App.m_Commands;
			}
		}
		public App(ScriptHostParams scriptHostParams)
		{
			this._gameRoot = scriptHostParams.AssetDir;
			this._consoleScriptFileName = scriptHostParams.ConsoleScriptFileName;
			this._fileSystem = scriptHostParams.FileSystem;
			this._userProfile = new Profile();
			this._steam = scriptHostParams.SteamAPI;
			if (ScriptHost.AllowConsole)
			{
				this._consoleApplet = new ConsoleApplet();
				this._consoleApplet.Start();
				App.Log.MessageLogged += new MessageLoggedEventHandler(this.Log_MessageLogged);
			}
			this._gameObjectMediator = new GameObjectMediator(this);
			this._gameStateMachine = new GameStateMachine();
			this._scriptCommChannel = new ScriptCommChannel(scriptHostParams.ScriptMessageQueue);
			this._uiCommChannel = new UICommChannel(scriptHostParams.UIMessageQueue);
			this._network = (Network)this.AddObject(InteropGameObjectType.IGOT_NETWORK, null);
			this._ircChat = new SotsIRC(this);
			if (this._network != null)
			{
				this._network.Initialize();
			}
			this._uiCommChannel.GameEvent += new UIEventGameEvent(this.OnGameEvent);
			App.m_Commands = new GameUICommands(this);
			App.m_bAI_Enabled = true;
			App.m_bPlayerAI_Enabled = false;
			App.m_bDebugFup = false;
			this._hotkeyManager = new HotKeyManager(this, this._gameRoot);
			this._hotkeyManager.PostSetActive(true);
			this.AddGameStates();
			GameState gameState = null;
			if (!string.IsNullOrEmpty(scriptHostParams.InitialStateName))
			{
				try
				{
					gameState = this._gameStateMachine.GetGameState(scriptHostParams.InitialStateName);
				}
				catch (ArgumentOutOfRangeException)
				{
				}
			}
			if (gameState == null)
			{
				gameState = this.GetGameState<MainMenuState>();
			}
			this.SwitchGameStateWithoutTransitionSound<SplashState>(new object[]
			{
				gameState
			});
		}
		private void OnGameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "LocalizeText")
			{
				if (string.IsNullOrEmpty(eventParams[1]))
				{
					return;
				}
				this._uiCommChannel.LocalizeText(eventParams[0], App.Localize(eventParams[1]));
			}
		}
		private void Log_MessageLogged(LogMessageInfo messageInfo)
		{
			string arg;
			if (messageInfo.Category.Length == 0)
			{
				arg = "•";
			}
			else
			{
				arg = messageInfo.Category;
			}
			this._consoleApplet.WriteText(messageInfo.Category, true, string.Format("{0,6}  ", arg), Color.Orchid);
			this._consoleApplet.WriteText(messageInfo.Category, false, messageInfo.Message, (messageInfo.Severity == LogSeverity.Trace) ? Color.LightBlue : Color.Orange);
			this._consoleApplet.WriteText(messageInfo.Category, false, "\r\n", Color.LightBlue);
		}
		public static string Localize(string strId)
		{
			return AssetDatabase.CommonStrings.Localize(strId);
		}
		public T GetStratModifier<T>(StratModifiers sm, int playerId)
		{
			return this.GameDatabase.GetStratModifier<T>(sm, playerId, (T)((object)this.AssetDatabase.DefaultStratModifiers[sm]));
		}
		public Kerberos.Sots.PlayerFramework.Player GetPlayer(int playerId)
		{
			if (this.game == null)
			{
				return null;
			}
			return this.game.GetPlayerObject(playerId);
		}
		public Kerberos.Sots.PlayerFramework.Player GetPlayerByObjectID(int objectId)
		{
			if (this.game == null)
			{
				return null;
			}
			return this.game.GetPlayerObjectByObjectID(objectId);
		}
		public SavedGameFilename[] GetAllSaveGames()
		{
			string searchPattern = "*.sots2save";
			List<SavedGameFilename> list = new List<SavedGameFilename>();
			List<string> list2 = new List<string>();
			foreach (Profile current in Profile.GetAvailableProfiles())
			{
				string item = this._baseSaveDir + Path.DirectorySeparatorChar + current.ProfileName;
				list2.Add(item);
			}
			list2.Add(this._baseSaveDir);
			foreach (string current2 in list2)
			{
				try
				{
					list.AddRange(
						from x in Directory.EnumerateFiles(current2, searchPattern, SearchOption.TopDirectoryOnly)
						select new SavedGameFilename
						{
							RootedFilename = x,
							IsBuiltin = false
						});
				}
				catch (DirectoryNotFoundException)
				{
				}
			}
			return list.ToArray();
		}
		public SavedGameFilename[] GetAvailableSavedGames()
		{
			string searchPattern = "*.sots2save";
			SavedGameFilename[] array = null;
			try
			{
				array = (
					from x in Directory.EnumerateFiles(this.SaveDir, searchPattern, SearchOption.TopDirectoryOnly)
					select new SavedGameFilename
					{
						RootedFilename = x,
						IsBuiltin = false
					}).ToArray<SavedGameFilename>();
			}
			catch (DirectoryNotFoundException)
			{
				if (array == null)
				{
					array = new SavedGameFilename[0];
				}
			}
			return array.ToArray<SavedGameFilename>();
		}
		public void RequestExit()
		{
			if (!this._engineExitRequested)
			{
				this._engineExitRequested = true;
				this._ircChat.Disconnect();
				this.ReleaseObject(this._network);
				this.PostEngineMessage(new object[]
				{
					InteropMessageID.IMID_ENGINE_EXIT
				});
				App.UnlockAllStreams();
				if (this._consoleApplet != null)
				{
					this._consoleApplet.Stop();
				}
			}
		}
		public IGameObject GetGameObject(int id)
		{
			return this._gameObjectMediator.GetObject(id);
		}
		public T GetGameObject<T>(int id) where T : class, IGameObject
		{
			return this._gameObjectMediator.GetObject(id) as T;
		}
		public T GetGameState<T>() where T : GameState
		{
			return (T)((object)this._gameStateMachine.FirstOrDefault((GameState x) => x.GetType() == typeof(T)));
		}
		private bool SwitchGameStateViaLoadingScreen(TimeSpan minTime, string text, string image, Action action, LoadingFinishedDelegate loadingFinishedDelegate, GameState state, params object[] parms)
		{
			this.PostRequestStopSounds();
			this.PostRequestGuiSound("universal_screentransition");
			this.PostDisableAllSounds();
			return this._gameStateMachine.SwitchGameState(this.GetGameState<LoadingScreenState>(), new object[]
			{
				(float)minTime.TotalSeconds,
				text,
				image,
				action,
				loadingFinishedDelegate,
				state,
				parms
			});
		}
		public bool SwitchGameStateViaLoadingScreen(Action action, LoadingFinishedDelegate loadingFinishedDelgate, GameState state, params object[] parms)
		{
			TimeSpan minTime = TimeSpan.FromSeconds(5.0);
			string text = App.Localize("@UI_LOADING_SCREEN_LOADING");
			string randomSplashScreenImageName = this._assetDatabase.GetRandomSplashScreenImageName();
			return this.SwitchGameStateViaLoadingScreen(minTime, text, randomSplashScreenImageName, action, loadingFinishedDelgate, state, parms);
		}
		public bool SwitchGameState(GameState value, params object[] parms)
		{
			return this._gameStateMachine.SwitchGameState(value, parms);
		}
		public bool SwitchGameState(string stateName)
		{
			this.PostRequestStopSounds();
			this.PostRequestGuiSound("universal_screentransition");
			this.PostDisableAllSounds();
			return this.SwitchGameState(this._gameStateMachine.GetGameState(stateName), new object[0]);
		}
		public bool SwitchGameState<T>(params object[] parms) where T : GameState
		{
			this.PostRequestStopSounds();
			this.PostRequestGuiSound("universal_screentransition");
			this.PostDisableAllSounds();
			return this.SwitchGameState(this.GetGameState<T>(), parms);
		}
		public bool SwitchGameStateWithoutTransitionSound<T>(params object[] parms) where T : GameState
		{
			this.PostRequestStopSounds();
			this.PostDisableAllSounds();
			return this.SwitchGameState(this.GetGameState<T>(), parms);
		}
		public bool PrepareGameState(GameState state, params object[] parms)
		{
			return this._gameStateMachine.PrepareGameState(state, parms);
		}
		public bool PrepareGameState<T>(params object[] parms) where T : GameState
		{
			return this._gameStateMachine.PrepareGameState(this.GetGameState<T>(), parms);
		}
		public void SwitchToPreparedGameState()
		{
			this.PostRequestStopSounds();
			this.PostRequestGuiSound("universal_screentransition");
			this.PostDisableAllSounds();
			this._gameStateMachine.SwitchToPreparedGameState();
		}
		public IGameObject AddObject(InteropGameObjectType gameObjectType, object[] initParams)
		{
			return this._gameObjectMediator.AddObject(gameObjectType, initParams);
		}
		public T AddObject<T>(params object[] initParams) where T : IGameObject
		{
			return (T)((object)this._gameObjectMediator.AddObject(typeof(T), initParams));
		}
		public void AddExistingObject(IGameObject o, params object[] initParams)
		{
			this._gameObjectMediator.AddExistingObject(o, initParams);
		}
		public void SetObjectTag(IGameObject state, object value)
		{
			this._gameObjectMediator.SetObjectTag(state, value);
		}
		public void RemoveObjectTag(IGameObject state)
		{
			this._gameObjectMediator.RemoveObjectTag(state);
		}
		public object GetObjectTag(IGameObject state)
		{
			return this._gameObjectMediator.GetObjectTag(state);
		}
		public void PostEngineMessage(params object[] elements)
		{
			this._scriptCommChannel.SendMessage(elements);
		}
		public void PostEngineMessage(IEnumerable elements)
		{
			this._scriptCommChannel.SendMessage(elements);
		}
		public void ReleaseObject(IGameObject value)
		{
			this._gameObjectMediator.ReleaseObject(value);
			if (this.ObjectReleased != null)
			{
				this.ObjectReleased(value);
			}
		}
		public void ReleaseObjects(IEnumerable<IGameObject> range)
		{
			this._gameObjectMediator.ReleaseObjects(range);
		}
		private void AddMaterialDictionaries(IEnumerable<string> names)
		{
			foreach (string current in names)
			{
				this.PostEngineMessage(new object[]
				{
					InteropMessageID.IMID_ENGINE_MATERIALS_ADD,
					current
				});
			}
			this.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_MATERIALS_REQ_READY
			});
		}
		private void AddCritHitChances(AssetDatabase.CritHitChances[] chc)
		{
			List<object> list = new List<object>();
			list.Add(InteropMessageID.IMID_ENGINE_INIT_STATS_PERCENTS);
			for (int i = 0; i < chc.Length; i++)
			{
				AssetDatabase.CritHitChances critHitChances = chc[i];
				int[] chances = critHitChances.Chances;
				for (int j = 0; j < chances.Length; j++)
				{
					int num = chances[j];
					list.Add(num);
				}
			}
			this.PostEngineMessage(list.ToArray());
		}
		private NamesPool LoadNewNamesPool()
		{
			return NamesFramework.LoadFromXml("data\\Names.xml");
		}
		public void ResetGameSetup()
		{
			this._gameSetup.IsMultiplayer = false;
			this._gameSetup.Reset(this.AssetDatabase.Factions);
		}
		public void EndGame()
		{
			if (this.game != null)
			{
				this.game.Dispose();
				this.game = null;
			}
			this.ResetGameSetup();
		}
		public void NewGame()
		{
			if (this.game != null)
			{
				this.game.Dispose();
				this.game = null;
			}
			this.game = App.NewGame(this, null, this.GameSetup, this.AssetDatabase, this._gameSetup, (GameSession.Flags)0);
		}
		public void ConfirmAI()
		{
			if (this.Game == null)
			{
				return;
			}
			foreach (PlayerSetup current in this.GameSetup.Players)
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.Game.GetPlayerObject(current.databaseId);
				playerObject.SetAI(current.AI);
			}
		}
		public void UILoadGame(string fileToLoad)
		{
			bool isMultiplayer = this.GameSetup.IsMultiplayer;
			if (!isMultiplayer)
			{
				this.GameSetup.ResetPlayers();
			}
			this.LoadGame(fileToLoad, this.GameSetup);
			this.GameSetup.IsMultiplayer = isMultiplayer;
			if (this.CurrentState != this.GetGameState<StarMapLobbyState>())
			{
				this.SwitchGameState<StarMapLobbyState>(new object[]
				{
					LobbyEntranceState.SinglePlayerLoad
				});
			}
		}
		public void LoadGame(string filename, GameSetup gs)
		{
			GameDatabase gameDatabase = GameDatabase.Load(filename, this.AssetDatabase);
			if (!this.Network.IsJoined)
			{
				List<FactionInfo> list = gameDatabase.GetFactions().ToList<FactionInfo>();
				List<Faction> list2 = new List<Faction>();
				foreach (FactionInfo current in list)
				{
					list2.Add(this.AssetDatabase.GetFaction(current.Name));
				}
				gs.Reset(list2);
				gs.Players.Clear();
				gs.StrategicTurnLength = gameDatabase.GetTurnLength(GameDatabase.TurnLengthTypes.Strategic);
				gs.CombatTurnLength = gameDatabase.GetTurnLength(GameDatabase.TurnLengthTypes.Combat);
				gs.RandomEncounterFrequency = gameDatabase.GetRandomEncounterFrequency();
				gs.EconomicEfficiency = gameDatabase.GetEconomicEfficiency();
				gs.ResearchEfficiency = gameDatabase.GetResearchEfficiency();
				IEnumerable<PlayerInfo> playerInfos = gameDatabase.GetPlayerInfos();
				foreach (PlayerInfo current2 in playerInfos)
				{
					PlayerSetup playerSetup = new PlayerSetup();
					Faction faction = null;
					foreach (FactionInfo current3 in list)
					{
						if (current3.ID == current2.FactionID)
						{
							faction = this.AssetDatabase.GetFaction(current3.Name);
						}
					}
					if (faction != null && faction.IsPlayable && current2.isStandardPlayer)
					{
						playerSetup.Faction = faction.Name;
						playerSetup.SubfactionIndex = current2.SubfactionIndex;
						playerSetup.AI = true;
						playerSetup.Avatar = Path.GetFileNameWithoutExtension(current2.AvatarAssetPath);
						playerSetup.Badge = Path.GetFileNameWithoutExtension(current2.BadgeAssetPath);
						playerSetup.EmpireName = current2.Name;
						playerSetup.ShipColor = current2.SecondaryColor;
						playerSetup.slot = current2.ID - 1;
						playerSetup.Fixed = true;
						playerSetup.Team = current2.Team;
						playerSetup.AIDifficulty = current2.AIDifficulty;
						int num = Kerberos.Sots.PlayerFramework.Player.DefaultPrimaryPlayerColors.IndexOf(current2.PrimaryColor);
						if (num != -1)
						{
							playerSetup.EmpireColor = new int?(num);
						}
						gs.Players.Add(playerSetup);
					}
				}
			}
			NamesPool namesPool = this.LoadNewNamesPool();
			List<Trigger> activeTriggers = new List<Trigger>();
			this.game = new GameSession(this, gameDatabase, gs, filename, namesPool, activeTriggers, App.GetSafeRandom(), GameSession.Flags.ResumingGame);
			if (this.UserProfile != null)
			{
				this.UserProfile.LastGamePlayed = filename;
				this.UserProfile.SaveProfile();
			}
		}
		public void NewGame(Random initializationRandomSeed)
		{
			if (this.game != null)
			{
				this.game.GameDatabase.Dispose();
				this.game = null;
			}
			this.game = App.NewGame(this, initializationRandomSeed, this.GameSetup, this.AssetDatabase, this._gameSetup, (GameSession.Flags)0);
		}
		public void StartGame(Action action, LoadingFinishedDelegate del, params object[] parms)
		{
			this.SwitchGameStateViaLoadingScreen(action, del, this.GetGameState<StarMapState>(), parms);
		}
		public static GameSession NewGame(App game, Random initializationRandomSeed, GameSetup gameSetup, AssetDatabase assetDatabase, GameSetup gs, GameSession.Flags flags = (GameSession.Flags)0)
		{
			flags &= ~GameSession.Flags.ResumingGame;
			gameSetup.FinalizeSetup();
			if (initializationRandomSeed == null)
			{
				initializationRandomSeed = new Random();
			}
			NamesPool namesPool = game.LoadNewNamesPool();
			List<Trigger> list = new List<Trigger>();
			List<int> list2 = new List<int>();
			string arg_30_0 = gameSetup.ScenarioFile;
			Scenario scenario = null;
			bool flag = !string.IsNullOrEmpty(gameSetup.ScenarioFile);
			GameDatabase gameDatabase = GameDatabase.New(gameSetup.GameName, assetDatabase, true);
			gameDatabase.SetClientId(1);
			gameDatabase.InsertNameValuePair("VictoryCondition", gameSetup._mode.ToString());
			gameDatabase.InsertNameValuePair("VictoryValue", gameSetup._modeValue.ToString());
			gameDatabase.InsertNameValuePair("GMCount", 0.ToString());
			gameDatabase.InsertNameValuePair("GSGrandMenaceCount", gameSetup._grandMenaceCount.ToString());
			gameDatabase.InsertNameValuePair("ResearchEfficiency", gameSetup._researchEfficiency.ToString());
			gameDatabase.InsertNameValuePair("EconomicEfficiency", gameSetup._economicEfficiency.ToString());
			gameDatabase.InsertNameValuePair("RandomEncounterFrequency", gameSetup._randomEncounterFrequency.ToString());
			gameDatabase.SetTurnLength(GameDatabase.TurnLengthTypes.Strategic, gameSetup.StrategicTurnLength);
			gameDatabase.SetTurnLength(GameDatabase.TurnLengthTypes.Combat, gameSetup.CombatTurnLength);
			gameDatabase.SetRandomEncounterFrequency(gameSetup.RandomEncounterFrequency);
			gameDatabase.SetEconomicEfficiency(gameSetup.EconomicEfficiency);
			gameDatabase.SetResearchEfficiency(gameSetup.ResearchEfficiency);
			if ((flags & GameSession.Flags.NoTechTree) == (GameSession.Flags)0)
			{
				foreach (Kerberos.Sots.Data.TechnologyFramework.Tech current in assetDatabase.MasterTechTree.Technologies)
				{
					gameDatabase.InsertTech(current.Id);
				}
			}
			gameDatabase.InsertMissingFactions(initializationRandomSeed);
			StarSystemVars.LoadXml("data\\StarSystemVars.xml");
			string text;
			if (flag)
			{
				scenario = new Scenario();
				ScenarioXmlUtility.LoadScenarioFromXml(gameSetup.ScenarioFile, ref scenario);
				gameDatabase.SetEconomicEfficiency(scenario.EconomicEfficiency);
				gameDatabase.SetResearchEfficiency(scenario.ResearchEfficiency);
				list.AddRange(scenario.Triggers);
				gameSetup.Players.Clear();
				foreach (Kerberos.Sots.Data.ScenarioFramework.Player current2 in scenario.PlayerStartConditions)
				{
					gameSetup.Players.Add(new PlayerSetup
					{
						EmpireName = current2.Name,
						Faction = current2.Faction,
						AI = current2.isAI,
						AIDifficulty = current2.isAI ? ((AIDifficulty)Enum.Parse(typeof(AIDifficulty), current2.AIDifficulty)) : AIDifficulty.Normal,
						Avatar = current2.Avatar,
						Badge = current2.Badge,
						ShipColor = current2.ShipColor,
						InitialColonies = 0,
						InitialTechs = 0,
						InitialTreasury = (int)current2.Treasury
					});
				}
				text = scenario.Starmap;
			}
			else
			{
				text = "starmaps\\FIGHT.Starmap";
				if (!string.IsNullOrEmpty(gameSetup.StarMapFile))
				{
					text = gameSetup.StarMapFile;
				}
			}
			gameDatabase.InsertNameValuePair("map_name", Path.GetFileNameWithoutExtension(text));
			LegacyStarMap legacyStarMap = LegacyStarMap.CreateStarMapFromFileCore(initializationRandomSeed, text);
			List<App.StartLocation> choices = App.CollectStartLocations(legacyStarMap);
			List<App.StartLocation> list3 = App.RandomizeStartLocations(choices, gameSetup.Players);
			if (list3.Count != gameSetup.Players.Count)
			{
				throw new InvalidDataException(string.Format("Number of randomized start locations ({0}) does not match number of players in game setup ({1}).", list3.Count, gameSetup.Players.Count));
			}
			App.EnsureInhabitableStartLocation(initializationRandomSeed, list3, gameSetup.Players, gameDatabase);
			legacyStarMap.AssignEmptySystemNames(initializationRandomSeed, namesPool);
			legacyStarMap.AssignEmptyPlanetTypes(initializationRandomSeed);
			legacyStarMap.AssignEmptyPlanetParameters(initializationRandomSeed);
			StarSystemHelper.VerifyStarMap(legacyStarMap);
			gameDatabase.ImportStarMap(ref legacyStarMap, initializationRandomSeed, flags);
			new Random();
			List<int> list4 = Enumerable.Range(0, (!gameSetup.IsMultiplayer) ? Kerberos.Sots.PlayerFramework.Player.DefaultPrimaryPlayerColors.Count : Kerberos.Sots.PlayerFramework.Player.DefaultMPPrimaryPlayerColors.Count).ToList<int>();
			foreach (PlayerSetup current3 in gameSetup.Players)
			{
				if (current3.EmpireColor.HasValue)
				{
					list4.Remove(current3.EmpireColor.Value);
				}
			}
			foreach (PlayerSetup current4 in gameSetup.Players)
			{
				if (!current4.EmpireColor.HasValue && list4.Count > 0)
				{
					current4.EmpireColor = new int?(initializationRandomSeed.Choose(list4));
					list4.Remove(current4.EmpireColor.Value);
				}
			}
			for (int i = 0; i < list3.Count<App.StartLocation>(); i++)
			{
				PlayerSetup playerSetup = gameSetup.Players[list3[i].PlayerIndex];
				int? num = new int?(list3[i].Planet.ID);
				if (num == 0)
				{
					num = null;
				}
				if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0 && (!num.HasValue || gameDatabase.GetPlanetInfo(num.Value) == null || gameDatabase.GetOrbitalObjectInfo(num.Value) == null))
				{
					throw new NullReferenceException("Planet or orbital object ID missing: " + num);
				}
				if (string.IsNullOrEmpty(playerSetup.Badge))
				{
					playerSetup.Badge = Path.GetFileNameWithoutExtension(assetDatabase.GetRandomBadgeTexture(playerSetup.Faction, initializationRandomSeed));
				}
				if (string.IsNullOrEmpty(playerSetup.Avatar))
				{
					playerSetup.Avatar = Path.GetFileNameWithoutExtension(assetDatabase.GetRandomAvatarTexture(playerSetup.Faction, initializationRandomSeed));
				}
				Vector3 primaryColor = Vector3.One;
				if (playerSetup.EmpireColor.HasValue)
				{
					if (!gameSetup.IsMultiplayer)
					{
						primaryColor = Kerberos.Sots.PlayerFramework.Player.DefaultPrimaryPlayerColors[playerSetup.EmpireColor.Value];
					}
					else
					{
						primaryColor = Kerberos.Sots.PlayerFramework.Player.DefaultMPPrimaryPlayerColors[playerSetup.EmpireColor.Value];
					}
				}
				if (flag)
				{
					int num2;
					if (!playerSetup.AI)
					{
						num2 = gameDatabase.InsertPlayer(playerSetup.EmpireName, playerSetup.Faction, num, primaryColor, playerSetup.ShipColor, playerSetup.GetBadgeTextureAssetPath(assetDatabase), playerSetup.GetAvatarTextureAssetPath(assetDatabase), (double)playerSetup.InitialTreasury, 0, true, false, scenario.PlayerStartConditions[i].isAIRebellion, 0, playerSetup.AIDifficulty);
					}
					else
					{
						num2 = gameDatabase.InsertPlayer(playerSetup.EmpireName, playerSetup.Faction, num, primaryColor, playerSetup.ShipColor, playerSetup.GetBadgeTextureAssetPath(assetDatabase), playerSetup.GetAvatarTextureAssetPath(assetDatabase), (double)playerSetup.InitialTreasury, 0, true, false, scenario.PlayerStartConditions[i].isAIRebellion, 0, playerSetup.AIDifficulty);
						list2.Add(num2);
					}
					App.AddStratModifiers(gameDatabase, assetDatabase, num2);
					if ((flags & GameSession.Flags.NoTechTree) == (GameSession.Flags)0)
					{
						ResearchScreenState.BuildPlayerTechTree(game, assetDatabase, gameDatabase, num2, scenario.PlayerStartConditions[i].AvailableTechs);
						List<PlayerTechInfo> source = gameDatabase.GetPlayerTechInfos(num2).ToList<PlayerTechInfo>();
						foreach (Kerberos.Sots.Data.ScenarioFramework.Tech t in scenario.PlayerStartConditions[i].StartingTechs)
						{
							PlayerTechInfo playerTechInfo = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == t.Name);
							if (playerTechInfo != null)
							{
								playerTechInfo.Progress = 100;
								playerTechInfo.State = TechStates.Researched;
								gameDatabase.UpdatePlayerTechInfo(playerTechInfo);
							}
						}
						gameDatabase.UpdateLockedTechs(assetDatabase, num2);
					}
					if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0)
					{
						foreach (Kerberos.Sots.Data.ScenarioFramework.Colony c in scenario.PlayerStartConditions[i].Colonies)
						{
							StarSystem starSystem = legacyStarMap.Objects.OfType<StarSystem>().First((StarSystem x) => x.Params.Guid == c.SystemId);
							IStellarEntity stellarEntity = starSystem.Objects.First((IStellarEntity x) => x.Params.OrbitNumber == c.OrbitId);
							if (c.IsIdealColony)
							{
								GameSession.MakeIdealColony(gameDatabase, assetDatabase, c.OrbitId, num2, IdealColonyTypes.Primary);
							}
							double num3 = c.ImperialPopulation;
							foreach (CivilianPopulation current5 in c.CivilianPopulations)
							{
								num3 += current5.Population;
							}
							int colonyID = gameDatabase.InsertColony(stellarEntity.ID, num2, c.ImperialPopulation, (num3 == 0.0) ? 0f : ((float)c.ImperialPopulation / (float)num3), 1, (float)c.Infrastructure, true);
							ColonyInfo colonyInfo = gameDatabase.GetColonyInfo(colonyID);
							PlanetInfo planetInfo = gameDatabase.GetPlanetInfo(stellarEntity.ID);
							Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(gameDatabase, assetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, 0f);
							gameDatabase.UpdateColony(colonyInfo);
							gameDatabase.InsertExploreRecord(starSystem.ID, num2, 1, true, true);
							foreach (CivilianPopulation current6 in c.CivilianPopulations)
							{
								gameDatabase.InsertColonyFaction(stellarEntity.ID, gameDatabase.GetFactionIdFromName(current6.Faction), current6.Population, (float)(current6.Population / num3), 1);
							}
						}
						Dictionary<string, int> dictionary = new Dictionary<string, int>();
						foreach (Station s in scenario.PlayerStartConditions[i].Stations)
						{
							OrbitalPath path = default(OrbitalPath);
							path.Scale = new Vector2(10f, 10f);
							path.Rotation = new Vector3(0f, 0f, 0f);
							path.DeltaAngle = 10f;
							path.InitialAngle = 10f;
							StarSystem starSystem2 = legacyStarMap.Objects.OfType<StarSystem>().First((StarSystem x) => x.Params.Guid == s.Location);
							IStellarEntity se = starSystem2.Objects.First((IStellarEntity x) => x.Params.OrbitNumber == s.Orbit);
							List<OrbitalObjectInfo> source2 = gameDatabase.GetStarSystemOrbitalObjectInfos(starSystem2.ID).ToList<OrbitalObjectInfo>();
							OrbitalObjectInfo orbitalObjectInfo = source2.FirstOrDefault((OrbitalObjectInfo x) => x.OrbitalPath.Scale.Y == se.Orbit.SemiMajorAxis);
							DesignInfo design = DesignLab.CreateStationDesignInfo(assetDatabase, gameDatabase, num2, ScenarioEnumerations.StationTypes[s.Type], s.Stage, true);
							int value = gameDatabase.InsertStation(orbitalObjectInfo.ID, starSystem2.ID, path, s.Name, num2, design);
							dictionary.Add(s.Name, value);
						}
						Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
						foreach (Kerberos.Sots.Data.ScenarioFramework.Ship current7 in scenario.PlayerStartConditions[i].ShipDesigns)
						{
							DesignInfo designInfo = new DesignInfo();
							designInfo.PlayerID = num2;
							designInfo.Name = current7.Name;
							List<DesignSectionInfo> list5 = new List<DesignSectionInfo>();
							foreach (Kerberos.Sots.Data.ScenarioFramework.Section current8 in current7.Sections)
							{
								DesignSectionInfo designSectionInfo = new DesignSectionInfo();
								designSectionInfo.DesignInfo = designInfo;
								designSectionInfo.FilePath = string.Format("factions\\{0}\\sections\\{1}", current7.Faction, current8.SectionFile);
								List<WeaponBankInfo> list6 = new List<WeaponBankInfo>();
								foreach (Bank current9 in current8.Banks)
								{
									if (!string.IsNullOrEmpty(current9.GUID))
									{
										list6.Add(new WeaponBankInfo
										{
											WeaponID = gameDatabase.GetWeaponID(current9.Weapon, num2),
											BankGUID = Guid.Parse(current9.GUID)
										});
									}
								}
								designSectionInfo.WeaponBanks = list6;
								list5.Add(designSectionInfo);
							}
							designInfo.DesignSections = list5.ToArray();
							DesignLab.SummarizeDesign(assetDatabase, gameDatabase, designInfo);
							dictionary2.Add(current7.Name, gameDatabase.InsertDesignByDesignInfo(designInfo));
						}
						Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
						foreach (Admiral a in scenario.PlayerStartConditions[i].Admirals)
						{
							legacyStarMap.Objects.OfType<StarSystem>().First((StarSystem x) => x.Params.Guid == a.HomePlanet);
							int value2 = gameDatabase.InsertAdmiral(num2, null, a.Name, a.Faction, (float)a.Age, a.Gender, a.ReactionRating, a.EvasionRating, 50);
							foreach (SpecialCharacteristic arg_DE3_0 in a.SpecialCharacteristics)
							{
							}
							dictionary3.Add(a.Name, value2);
						}
						foreach (Fleet f in scenario.PlayerStartConditions[i].Fleets)
						{
							StarSystem starSystem3 = legacyStarMap.Objects.OfType<StarSystem>().First((StarSystem x) => x.Params.Guid == f.Location);
							int fleetID = gameDatabase.InsertFleet(num2, dictionary3[f.Admiral], starSystem3.ID, starSystem3.ID, f.Name, FleetType.FL_NORMAL);
							foreach (Kerberos.Sots.Data.ScenarioFramework.Ship current10 in f.Ships)
							{
								gameDatabase.InsertShip(fleetID, dictionary2[current10.Name], null, (ShipParams)0, null, 0);
							}
							gameDatabase.LayoutFleet(fleetID);
						}
					}
					flags |= GameSession.Flags.NoDefaultFleets;
				}
				else
				{
					int num2 = gameDatabase.InsertPlayer(playerSetup.EmpireName, playerSetup.Faction, num, primaryColor, playerSetup.ShipColor, playerSetup.GetBadgeTextureAssetPath(assetDatabase), playerSetup.GetAvatarTextureAssetPath(assetDatabase), (double)game.GameSetup.Players[list3[i].PlayerIndex].InitialTreasury, playerSetup.SubfactionIndex, true, false, false, playerSetup.Team, playerSetup.AIDifficulty);
					if (!playerSetup.AI)
					{
						if (playerSetup.localPlayer)
						{
						}
					}
					else
					{
						list2.Add(num2);
					}
					App.BuildTeamDiplomacyStates(gameDatabase);
					App.AddStratModifiers(gameDatabase, assetDatabase, num2);
					if ((flags & GameSession.Flags.NoTechTree) == (GameSession.Flags)0)
					{
						ResearchScreenState.BuildPlayerTechTree(game, assetDatabase, gameDatabase, num2);
					}
					if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0)
					{
						GameSession.MakeIdealColony(gameDatabase, assetDatabase, num.Value, num2, IdealColonyTypes.Primary);
						OrbitalPath path2 = default(OrbitalPath);
						path2.Scale = new Vector2(10f, 10f);
						path2.Rotation = new Vector3(0f, 0f, 0f);
						path2.DeltaAngle = 10f;
						path2.InitialAngle = 10f;
						DesignInfo design2 = DesignLab.CreateStationDesignInfo(assetDatabase, gameDatabase, num2, StationType.NAVAL, 3, true);
						gameDatabase.InsertStation(num.Value, list3[i].System.ID, path2, "Naval Base", num2, design2);
					}
				}
			}
			if ((flags & GameSession.Flags.NoDefaultFleets) == (GameSession.Flags)0)
			{
				App.AddSuulkas(assetDatabase, gameDatabase);
			}
			if (flag)
			{
				foreach (Kerberos.Sots.Data.ScenarioFramework.Player current11 in scenario.PlayerStartConditions)
				{
					foreach (PlayerRelation current12 in current11.Relations)
					{
						gameDatabase.UpdateDiplomacyState(current11.PlayerSlot, current12.Player, current12.DiplomacyState, current12.Relations, true);
					}
				}
			}
			GameSession gameSession = new GameSession(game, gameDatabase, gs, gs.GetDefaultSaveGameFileName(), namesPool, list, initializationRandomSeed, flags);
			List<ColonyInfo> list7 = gameDatabase.GetColonyInfos().ToList<ColonyInfo>();
			foreach (ColonyInfo current13 in list7)
			{
				current13.RepairPoints = Kerberos.Sots.Strategy.InhabitedPlanet.Colony.CalcColonyRepairPoints(gameSession, current13);
				current13.RepairPointsMax = current13.RepairPoints;
				gameDatabase.UpdateColony(current13);
			}
			if ((flags & GameSession.Flags.NoGameSetup) == (GameSession.Flags)0)
			{
				gameSetup.SavePlayerSlots(gameDatabase);
			}
			return gameSession;
		}
		public static void BuildTeamDiplomacyStates(GameDatabase gamedb)
		{
			List<PlayerInfo> list = (
				from x in gamedb.GetPlayerInfos()
				where x.isStandardPlayer
				select x).ToList<PlayerInfo>();
			foreach (PlayerInfo pi in list)
			{
				if (pi.Team != 0)
				{
					foreach (PlayerInfo current in 
						from x in list
						where x.ID != pi.ID && x.Team == pi.Team
						select x)
					{
						if (gamedb.GetDiplomacyStateBetweenPlayers(pi.ID, current.ID) != DiplomacyState.ALLIED)
						{
							gamedb.UpdateDiplomacyState(pi.ID, current.ID, DiplomacyState.ALLIED, DiplomacyInfo.MaxDeplomacyRelations, true);
							DiplomacyInfo diplomacyInfo = gamedb.GetDiplomacyInfo(pi.ID, current.ID);
							diplomacyInfo.isEncountered = true;
							gamedb.UpdateDiplomacyInfo(diplomacyInfo);
						}
					}
				}
			}
		}
		public static void AddSuulkas(AssetDatabase assetdb, GameDatabase gamedb)
		{
			DesignInfo[] array = new DesignInfo[7];
			for (int i = 0; i < 7; i++)
			{
				array[i] = new DesignInfo();
				array[i].PlayerID = 0;
				array[i].DesignSections = new DesignSectionInfo[1];
				array[i].DesignSections[0] = new DesignSectionInfo
				{
					DesignInfo = array[i]
				};
				switch (i)
				{
				case 0:
					array[i].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_cannibal.section";
					array[i].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_CANNIBAL");
					break;
				case 1:
					array[i].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_deaf.section";
					array[i].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_DEAF");
					break;
				case 2:
					array[i].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_hidden.section";
					array[i].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_HIDDEN");
					break;
				case 3:
					array[i].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_immortal.section";
					array[i].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_IMMORTAL");
					break;
				case 4:
					array[i].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_kraken.section";
					array[i].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_KRAKEN");
					break;
				case 5:
					array[i].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_shaper.section";
					array[i].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_SHAPER");
					break;
				case 6:
					array[i].DesignSections[0].FilePath = "factions\\zuul\\sections\\lv_suulka_the_siren.section";
					array[i].Name = App.Localize("@SECTIONTITLE_LV_SUULKA_THE_SIREN");
					break;
				}
				if (i != 2)
				{
					int designID = gamedb.InsertDesignByDesignInfo(array[i]);
					int shipID = gamedb.InsertShip(0, designID, null, (ShipParams)0, null, 0);
					int admiralID = gamedb.InsertAdmiral(0, null, array[i].Name, "suulka", 0f, "male", 100f, 100f, 0);
					gamedb.InsertSuulka(null, shipID, admiralID, null, -1);
				}
			}
		}
		public static void AddStratModifiers(GameDatabase gamedb, AssetDatabase assetdb, int playerID)
		{
			foreach (StratModifiers arg_24_0 in Enum.GetValues(typeof(StratModifiers)))
			{
			}
			int playerFactionID = gamedb.GetPlayerFactionID(playerID);
			foreach (KeyValuePair<StratModifiers, object> current in assetdb.DefaultStratModifiers)
			{
				object factionStratModifier = assetdb.GetFactionStratModifier(playerFactionID, current.Key.ToString());
				object obj = current.Value;
				if (factionStratModifier != null)
				{
					obj = Convert.ChangeType(factionStratModifier, obj.GetType());
				}
				gamedb.SetStratModifier(current.Key, playerID, obj);
			}
		}
		public static void UpdateStratModifiers(GameSession game, int playerID, int techDatabaseID)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			AssetDatabase assetDatabase = game.AssetDatabase;
			PlayerTechInfo playerTechInfo = gameDatabase.GetPlayerTechInfo(playerID, techDatabaseID);
			string techFileID;
			switch (techFileID = playerTechInfo.TechFileID)
			{
			case "POL_Slave_Husbandry":
			{
				float stratModifier = gameDatabase.GetStratModifier<float>(StratModifiers.SlaveDeathRateModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.SlaveDeathRateModifier, playerID, stratModifier + assetDatabase.GetTechBonus<float>("POL_Slave_Husbandry", "slavedeathrate"));
				return;
			}
			case "CCC_Tunneling_Sensors":
			{
				float stratModifier2 = gameDatabase.GetStratModifier<float>(StratModifiers.NavyStationSensorCloakBonus, playerID);
				float stratModifier3 = gameDatabase.GetStratModifier<float>(StratModifiers.ScienceStationSensorCloakBonus, playerID);
				gameDatabase.SetStratModifier(StratModifiers.NavyStationSensorCloakBonus, playerID, stratModifier2 + assetDatabase.GetTechBonus<float>("CCC_Tunneling_Sensors", "sensorbonusnavy"));
				gameDatabase.SetStratModifier(StratModifiers.ScienceStationSensorCloakBonus, playerID, stratModifier3 + assetDatabase.GetTechBonus<float>("CCC_Tunneling_Sensors", "sensorbonussci"));
				return;
			}
			case "CCC_Commerce_Raiding":
				gameDatabase.SetStratModifier(StratModifiers.AllowPrivateerMission, playerID, true);
				return;
			case "IND_Mega-Strip_Mining":
			{
				float stratModifier4 = gameDatabase.GetStratModifier<float>(StratModifiers.StripMiningMaximum, playerID);
				gameDatabase.SetStratModifier(StratModifiers.StripMiningMaximum, playerID, stratModifier4 * assetDatabase.GetTechBonus<float>("IND_Mega-Strip_Mining", "miningmaxscale"));
				return;
			}
			case "IND_Atmospheric_Processors":
			{
				float stratModifier5 = gameDatabase.GetStratModifier<float>(StratModifiers.TerraformingModifier, playerID);
				float stratModifier6 = gameDatabase.GetStratModifier<float>(StratModifiers.BiosphereDestructionModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.TerraformingModifier, playerID, stratModifier5 + assetDatabase.GetTechBonus<float>("IND_Atmospheric_Processors", "terra"));
				gameDatabase.SetStratModifier(StratModifiers.BiosphereDestructionModifier, playerID, stratModifier6 + assetDatabase.GetTechBonus<float>("IND_Atmospheric_Processors", "biosphere"));
				return;
			}
			case "IND_Hardened_Structures":
				gameDatabase.SetStratModifier(StratModifiers.AllowHardenedStructures, playerID, true);
				return;
			case "IND_Zero-G_Deconstruction":
			{
				float stratModifier7 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				float stratModifier8 = gameDatabase.GetStratModifier<float>(StratModifiers.ScrapShipModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier7 + assetDatabase.GetTechBonus<float>("IND_Zero-G_Deconstruction", "industrialoutput"));
				gameDatabase.SetStratModifier(StratModifiers.ScrapShipModifier, playerID, stratModifier8 + assetDatabase.GetTechBonus<float>("IND_Zero-G_Deconstruction", "scrapship"));
				return;
			}
			case "IND_Tractor_Beams":
			{
				float stratModifier7 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier7 + assetDatabase.GetTechBonus<float>("IND_Tractor_Beams", "industrialoutput"));
				return;
			}
			case "IND_Gravity_Control":
			{
				float stratModifier7 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier7 + assetDatabase.GetTechBonus<float>("IND_Gravity_Control", "industrialoutput"));
				return;
			}
			case "ENG_Turret_Installations":
				gameDatabase.SetStratModifier(StratModifiers.AllowPlanetBeam, playerID, true);
				return;
			case "WAR_MW_Warheads":
				gameDatabase.SetStratModifier(StratModifiers.AllowMirvPlanetaryMissiles, playerID, true);
				return;
			case "ENG_Deep_Space_Construction":
				gameDatabase.SetStratModifier(StratModifiers.AllowDeepSpaceConstruction, playerID, true);
				return;
			case "POL_Slave_Functionaries":
			{
				float stratModifier9 = gameDatabase.GetStratModifier<float>(StratModifiers.SlaveProductionModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.SlaveProductionModifier, playerID, stratModifier9 + assetDatabase.GetTechBonus<float>("POL_Slave_Functionaries", "slaveproduction"));
				return;
			}
			case "POL_Trade_Enclaves":
				gameDatabase.SetStratModifier(StratModifiers.AllowTradeEnclave, playerID, true);
				return;
			case "POL_Protectorate":
				gameDatabase.SetStratModifier(StratModifiers.AllowProtectorate, playerID, true);
				return;
			case "POL_Incorporate":
				gameDatabase.SetStratModifier(StratModifiers.AllowAlienPopulations, playerID, true);
				gameDatabase.SetStratModifier(StratModifiers.AllowIncorporate, playerID, true);
				return;
			case "POL_Accomodate":
			{
				float stratModifier10 = gameDatabase.GetStratModifier<float>(StratModifiers.AlienCivilianTaxRate, playerID);
				gameDatabase.SetStratModifier(StratModifiers.AlienCivilianTaxRate, playerID, stratModifier10 + assetDatabase.GetTechBonus<float>("POL_Accomodate", "aliencivtaxrate"));
				gameDatabase.SetStratModifier(StratModifiers.AllowIdealAlienGrowthRate, playerID, true);
				return;
			}
			case "POL_Proliferate":
				gameDatabase.SetStratModifier(StratModifiers.AllowAlienImmigration, playerID, true);
				return;
			case "POL_Comparitive_Analysis":
				gameDatabase.SetStratModifier(StratModifiers.ComparativeAnalysys, playerID, true);
				return;
			case "DRV_Mass_Induction_Projectors":
				gameDatabase.SetStratModifier(StratModifiers.MassInductionProjectors, playerID, true);
				return;
			case "DRV_Standing_Neutrino_Waves":
				gameDatabase.SetStratModifier(StratModifiers.StandingNeutrinoWaves, playerID, true);
				return;
			case "BIO_Terraforming_Bacteria":
			{
				float stratModifier5 = gameDatabase.GetStratModifier<float>(StratModifiers.TerraformingModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.TerraformingModifier, playerID, stratModifier5 + assetDatabase.GetTechBonus<float>("BIO_Terraforming_Bacteria", "terra"));
				return;
			}
			case "BIO_Environmental_Tailoring":
			{
				float stratModifier5 = gameDatabase.GetStratModifier<float>(StratModifiers.TerraformingModifier, playerID);
				float stratModifier6 = gameDatabase.GetStratModifier<float>(StratModifiers.BiosphereDestructionModifier, playerID);
				float stratModifier11 = gameDatabase.GetStratModifier<float>(StratModifiers.PopulationGrowthModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.PopulationGrowthModifier, playerID, stratModifier11 + assetDatabase.GetTechBonus<float>("BIO_Environmental_Tailoring", "popgrowth"));
				gameDatabase.SetStratModifier(StratModifiers.TerraformingModifier, playerID, stratModifier5 + assetDatabase.GetTechBonus<float>("BIO_Environmental_Tailoring", "terra"));
				gameDatabase.SetStratModifier(StratModifiers.BiosphereDestructionModifier, playerID, stratModifier6 + assetDatabase.GetTechBonus<float>("BIO_Environmental_Tailoring", "biosphere"));
				return;
			}
			case "BIO_Gravitational_Adaptation":
			{
				float stratModifier11 = gameDatabase.GetStratModifier<float>(StratModifiers.PopulationGrowthModifier, playerID);
				float stratModifier12 = gameDatabase.GetStratModifier<float>(StratModifiers.ColonySupportCostModifier, playerID);
				int stratModifier13 = gameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerID);
				gameDatabase.SetStratModifier(StratModifiers.PopulationGrowthModifier, playerID, stratModifier11 + assetDatabase.GetTechBonus<float>("BIO_Gravitational_Adaptation", "popgrowth"));
				gameDatabase.SetStratModifier(StratModifiers.ColonySupportCostModifier, playerID, stratModifier12 + assetDatabase.GetTechBonus<float>("BIO_Gravitational_Adaptation", "colonysupportcost"));
				gameDatabase.SetStratModifier(StratModifiers.MaxColonizableHazard, playerID, stratModifier13 + assetDatabase.GetTechBonus<int>("BIO_Gravitational_Adaptation", "maxcolonizehazard"));
				return;
			}
			case "BIO_Elemental_Nanites":
			{
				float stratModifier5 = gameDatabase.GetStratModifier<float>(StratModifiers.TerraformingModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.TerraformingModifier, playerID, stratModifier5 + assetDatabase.GetTechBonus<float>("BIO_Elemental_Nanites", "terra"));
				return;
			}
			case "PSI_Lesser_Glamour":
			{
				float stratModifier14 = gameDatabase.GetStratModifier<float>(StratModifiers.DiplomaticOfferingModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.DiplomaticOfferingModifier, playerID, stratModifier14 + assetDatabase.GetTechBonus<float>("PSI_Lesser_Glamour", "diplomaticoffering"));
				return;
			}
			case "BIO_Anagathics":
			{
				float stratModifier15 = gameDatabase.GetStratModifier<float>(StratModifiers.AdmiralCareerModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.AdmiralCareerModifier, playerID, stratModifier15 + assetDatabase.GetTechBonus<float>("BIO_Anagathics", "career"));
				return;
			}
			case "BIO_Biosphere_Preservation":
			{
				float stratModifier6 = gameDatabase.GetStratModifier<float>(StratModifiers.BiosphereDestructionModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.BiosphereDestructionModifier, playerID, stratModifier6 + assetDatabase.GetTechBonus<float>("BIO_Biosphere_Preservation", "biosphere"));
				return;
			}
			case "CCC_Convoy_Systems":
			{
				float num2 = gameDatabase.GetStratModifier<float>(StratModifiers.ChanceOfPirates, playerID);
				num2 += num2 * assetDatabase.GetTechBonus<float>("CCC_Convoy_Systems", "piratechance");
				gameDatabase.SetStratModifier(StratModifiers.ChanceOfPirates, playerID, num2);
				return;
			}
			case "IND_Vacuum_Preservation":
			{
				float stratModifier16 = gameDatabase.GetStratModifier<float>(StratModifiers.ShipSupplyModifier, playerID);
				float stratModifier17 = gameDatabase.GetStratModifier<float>(StratModifiers.WarehouseCapacityModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.ShipSupplyModifier, playerID, stratModifier16 + assetDatabase.GetTechBonus<float>("IND_Vacuum_Preservation", "shipsupply"));
				gameDatabase.SetStratModifier(StratModifiers.WarehouseCapacityModifier, playerID, stratModifier17 + assetDatabase.GetTechBonus<float>("IND_Vacuum_Preservation", "warehousecapacity"));
				return;
			}
			case "DRV_Specter_Camouflage":
				gameDatabase.SetStratModifier(StratModifiers.ImmuneToSpectre, playerID, true);
				return;
			case "DRV_Warp_Veil":
			{
				float stratModifier18 = gameDatabase.GetStratModifier<float>(StratModifiers.WarpDriveStratSignatureModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.WarpDriveStratSignatureModifier, playerID, stratModifier18 + assetDatabase.GetTechBonus<float>("DRV_Warp_Veil", "warpsignature"));
				return;
			}
			case "DRV_Warp_Extension":
				gameDatabase.SetStratModifier(StratModifiers.UseFastestShipForFTLSpeed, playerID, true);
				return;
			case "POL_Super_Worlds":
				gameDatabase.SetStratModifier(StratModifiers.AllowSuperWorlds, playerID, true);
				return;
			case "POL_Xeno-Analysis":
			{
				float stratModifier19 = gameDatabase.GetStratModifier<float>(StratModifiers.DiplomacyPointCostModifier, playerID);
				float stratModifier20 = gameDatabase.GetStratModifier<float>(StratModifiers.NegativeRelationsModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.DiplomacyPointCostModifier, playerID, stratModifier19 + assetDatabase.GetTechBonus<float>("POL_Xeno-Analysis", "diplocost"));
				gameDatabase.SetStratModifier(StratModifiers.NegativeRelationsModifier, playerID, stratModifier20 + assetDatabase.GetTechBonus<float>("POL_Xeno-Analysis", "negativerelations"));
				return;
			}
			case "POL_OmbudSapiens":
				gameDatabase.SetStratModifier(StratModifiers.AllowOneFightRebellionEnding, playerID, true);
				return;
			case "PSI_Empathy":
			{
				float stratModifier21 = gameDatabase.GetStratModifier<float>(StratModifiers.DiplomaticReactionBonus, playerID);
				int stratModifier22 = gameDatabase.GetStratModifier<int>(StratModifiers.MoralBonus, playerID);
				gameDatabase.SetStratModifier(StratModifiers.DiplomaticReactionBonus, playerID, stratModifier21 + assetDatabase.GetTechBonus<float>("PSI_Empathy", "diploreaction"));
				gameDatabase.SetStratModifier(StratModifiers.MoralBonus, playerID, stratModifier22 + assetDatabase.GetTechBonus<int>("PSI_Empathy", "moralbonus"));
				return;
			}
			case "PSI_Telepathy":
			{
				float stratModifier23 = gameDatabase.GetStratModifier<float>(StratModifiers.PsiResearchModifier, playerID);
				float stratModifier24 = gameDatabase.GetStratModifier<float>(StratModifiers.ResearchModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.PsiResearchModifier, playerID, stratModifier23 + assetDatabase.GetTechBonus<float>("PSI_Telepathy", "psiresearch"));
				gameDatabase.SetStratModifier(StratModifiers.ResearchModifier, playerID, stratModifier24 + assetDatabase.GetTechBonus<float>("PSI_Telepathy", "research"));
				return;
			}
			case "PSI_Farsense":
				gameDatabase.SetStratModifier(StratModifiers.AllowFarSense, playerID, true);
				return;
			case "PSI_Telekinesis":
			{
				float stratModifier25 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier25 + assetDatabase.GetTechBonus<float>("PSI_Telekinesis", "industrialoutput"));
				return;
			}
			case "PSI_Micro-Telekinesis":
			{
				float stratModifier26 = gameDatabase.GetStratModifier<float>(StratModifiers.C3ResearchModifier, playerID);
				float stratModifier24 = gameDatabase.GetStratModifier<float>(StratModifiers.ResearchModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.C3ResearchModifier, playerID, stratModifier26 + assetDatabase.GetTechBonus<float>("PSI_Micro-Telekinesis", "c3research"));
				gameDatabase.SetStratModifier(StratModifiers.ResearchModifier, playerID, stratModifier24 + assetDatabase.GetTechBonus<float>("PSI_Micro-Telekinesis", "research"));
				return;
			}
			case "PSI_Precognition":
			{
				float stratModifier27 = gameDatabase.GetStratModifier<float>(StratModifiers.AdmiralReactionModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.AdmiralReactionModifier, playerID, stratModifier27 + assetDatabase.GetTechBonus<float>("PSI_Precognition", "admiralreaction"));
				return;
			}
			case "PSI_Scientific_Prolepsis":
			{
				float num3 = gameDatabase.GetStratModifier<float>(StratModifiers.TechFeasibilityDeviation, playerID);
				num3 += num3 * assetDatabase.GetTechBonus<float>("PSI_Scientific_Prolepsis", "techfeasibilitydev");
				gameDatabase.SetStratModifier(StratModifiers.TechFeasibilityDeviation, playerID, num3);
				return;
			}
			case "PSI_Doomsayers":
			{
				gameDatabase.SetStratModifier(StratModifiers.GrandMenaceWarningTime, playerID, 5);
				float stratModifier28 = gameDatabase.GetStratModifier<float>(StratModifiers.RandomEncounterWarningPercent, playerID);
				gameDatabase.SetStratModifier(StratModifiers.RandomEncounterWarningPercent, playerID, stratModifier28 + assetDatabase.GetTechBonus<float>("PSI_Doomsayers", "randomnotification"));
				return;
			}
			case "CCC_Deep_Survey_Sensors":
			{
				float stratModifier29 = gameDatabase.GetStratModifier<float>(StratModifiers.SurveyTimeModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.SurveyTimeModifier, playerID, stratModifier29 + assetDatabase.GetTechBonus<float>("CCC_Deep_Survey_Sensors", "survey"));
				return;
			}
			case "IND_Pressure_Polarization":
			{
				int stratModifier30 = gameDatabase.GetStratModifier<int>(StratModifiers.DomeStageModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.DomeStageModifier, playerID, stratModifier30 + assetDatabase.GetTechBonus<int>("IND_Pressure_Polarization", "domestage"));
				return;
			}
			case "IND_Quantum_Disassociation":
			{
				float stratModifier7 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				float stratModifier31 = gameDatabase.GetStratModifier<float>(StratModifiers.CavernDmodModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier7 + assetDatabase.GetTechBonus<float>("IND_Quantum_Disassociation", "industrialoutput"));
				gameDatabase.SetStratModifier(StratModifiers.CavernDmodModifier, playerID, stratModifier31 + assetDatabase.GetTechBonus<float>("IND_Quantum_Disassociation", "caverndomemod"));
				return;
			}
			case "POL_Disinformation_Nets":
			{
				float stratModifier32 = gameDatabase.GetStratModifier<float>(StratModifiers.EnemyIntelSuccessModifier, playerID);
				float stratModifier33 = gameDatabase.GetStratModifier<float>(StratModifiers.EnemyOperationsSuccessModifier, playerID);
				float stratModifier34 = gameDatabase.GetStratModifier<float>(StratModifiers.CounterIntelSuccessModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.EnemyIntelSuccessModifier, playerID, stratModifier32 + assetDatabase.GetTechBonus<float>("POL_Disinformation_Nets", "enemyintel"));
				gameDatabase.SetStratModifier(StratModifiers.EnemyOperationsSuccessModifier, playerID, stratModifier33 + assetDatabase.GetTechBonus<float>("POL_Disinformation_Nets", "enemyoperations"));
				gameDatabase.SetStratModifier(StratModifiers.CounterIntelSuccessModifier, playerID, stratModifier34 + assetDatabase.GetTechBonus<float>("POL_Disinformation_Nets", "counterintel"));
				return;
			}
			case "POL_Enhanced_Jurisdiction":
			{
				int stratModifier35 = gameDatabase.GetStratModifier<int>(StratModifiers.MaxProvincePlanets, playerID);
				float stratModifier36 = gameDatabase.GetStratModifier<float>(StratModifiers.MaxProvincePlanetRange, playerID);
				gameDatabase.SetStratModifier(StratModifiers.MaxProvincePlanets, playerID, stratModifier35 + assetDatabase.GetTechBonus<int>("POL_Enhanced_Jurisdiction", "maxplanets"));
				gameDatabase.SetStratModifier(StratModifiers.MaxProvincePlanetRange, playerID, stratModifier36 + assetDatabase.GetTechBonus<float>("POL_Enhanced_Jurisdiction", "maxrange"));
				return;
			}
			case "POL_Cosmic_Bureaucracies":
			{
				int stratModifier35 = gameDatabase.GetStratModifier<int>(StratModifiers.MaxProvincePlanets, playerID);
				float stratModifier36 = gameDatabase.GetStratModifier<float>(StratModifiers.MaxProvincePlanetRange, playerID);
				gameDatabase.SetStratModifier(StratModifiers.MaxProvincePlanets, playerID, stratModifier35 + assetDatabase.GetTechBonus<int>("POL_Cosmic_Bureaucracies", "maxplanets"));
				gameDatabase.SetStratModifier(StratModifiers.MaxProvincePlanetRange, playerID, stratModifier36 + assetDatabase.GetTechBonus<float>("POL_Cosmic_Bureaucracies", "maxrange"));
				return;
			}
			case "POL_Paramilitary_Training":
			{
				int stratModifier37 = gameDatabase.GetStratModifier<int>(StratModifiers.PoliceMoralBonus, playerID);
				gameDatabase.SetStratModifier(StratModifiers.PoliceMoralBonus, playerID, stratModifier37 + assetDatabase.GetTechBonus<int>("POL_Paramilitary_Training", "policemoral"));
				gameDatabase.SetStratModifier(StratModifiers.AllowPoliceInCombat, playerID, true);
				return;
			}
			case "POL_Occupy":
				gameDatabase.SetStratModifier(StratModifiers.AllowWorldSurrender, playerID, true);
				return;
			case "POL_Annex":
				gameDatabase.SetStratModifier(StratModifiers.AllowProvinceSurrender, playerID, true);
				return;
			case "POL_Eclipse":
				gameDatabase.SetStratModifier(StratModifiers.AllowEmpireSurrender, playerID, true);
				return;
			case "ENG_Rapid_Prototyping":
			{
				float stratModifier38 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeConstructionCostModifierCR, playerID);
				float stratModifier39 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeConstructionCostModifierDN, playerID);
				float stratModifier40 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeConstructionCostModifierLV, playerID);
				float stratModifier41 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeConstructionCostModifierPF, playerID);
				float stratModifier42 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierCR, playerID);
				float stratModifier43 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierDN, playerID);
				float stratModifier44 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierLV, playerID);
				float stratModifier45 = gameDatabase.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierPF, playerID);
				float num4 = 1f + assetDatabase.GetTechBonus<float>("ENG_Rapid_Prototyping", "protocostscale");
				gameDatabase.SetStratModifier(StratModifiers.PrototypeConstructionCostModifierPF, playerID, stratModifier41 * num4);
				gameDatabase.SetStratModifier(StratModifiers.PrototypeConstructionCostModifierCR, playerID, stratModifier38 * num4);
				gameDatabase.SetStratModifier(StratModifiers.PrototypeConstructionCostModifierDN, playerID, stratModifier39 * num4);
				gameDatabase.SetStratModifier(StratModifiers.PrototypeConstructionCostModifierLV, playerID, stratModifier40 * num4);
				gameDatabase.SetStratModifier(StratModifiers.PrototypeSavingsCostModifierPF, playerID, stratModifier45 * num4);
				gameDatabase.SetStratModifier(StratModifiers.PrototypeSavingsCostModifierCR, playerID, stratModifier42 * num4);
				gameDatabase.SetStratModifier(StratModifiers.PrototypeSavingsCostModifierDN, playerID, stratModifier43 * num4);
				gameDatabase.SetStratModifier(StratModifiers.PrototypeSavingsCostModifierLV, playerID, stratModifier44 * num4);
				return;
			}
			case "ENG_Virtual_Engineering":
				gameDatabase.SetStratModifier(StratModifiers.ShowPrototypeDesignAttributes, playerID, true);
				return;
			case "ENG_Enhanced_Design":
			{
				int stratModifier46 = gameDatabase.GetStratModifier<int>(StratModifiers.GoodDesignAttributePercent, playerID);
				int stratModifier47 = gameDatabase.GetStratModifier<int>(StratModifiers.BadDesignAttributePercent, playerID);
				gameDatabase.SetStratModifier(StratModifiers.GoodDesignAttributePercent, playerID, stratModifier46 + 5);
				gameDatabase.SetStratModifier(StratModifiers.BadDesignAttributePercent, playerID, stratModifier47 - 5);
				return;
			}
			case "IND_Arcologies":
				gameDatabase.SetStratModifier(StratModifiers.AdditionalMaxCivilianPopulation, playerID, 200);
				gameDatabase.SetStratModifier(StratModifiers.AdditionalMaxImperialPopulation, playerID, 50);
				return;
			case "CCC_Artificial_Intelligence":
				gameDatabase.SetStratModifier(StratModifiers.AIResearchBonus, playerID, 0.5f);
				gameDatabase.SetStratModifier(StratModifiers.AllowAIRebellion, playerID, true);
				return;
			case "CCC_A.I._Administration":
				gameDatabase.SetStratModifier(StratModifiers.AIRevenueBonus, playerID, 0.5f);
				return;
			case "CCC_A.I._Factories":
				gameDatabase.SetStratModifier(StratModifiers.AIProductionBonus, playerID, 0.5f);
				return;
			case "CCC_A.I._Virus":
			{
				int[] aIOldColonyOwner = gameDatabase.GetAIOldColonyOwner(playerID);
				int[] array = aIOldColonyOwner;
				for (int i = 0; i < array.Length; i++)
				{
					int colonyID = array[i];
					gameDatabase.RemoveColonyOnPlanet(gameDatabase.GetColonyInfo(colonyID).OrbitalObjectID);
				}
				if (aIOldColonyOwner.Count<int>() > 0)
				{
					gameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_AI_REBELLION_END,
						EventMessage = TurnEventMessage.EM_AI_REBELLION_END,
						PlayerID = playerID,
						TurnNumber = gameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				gameDatabase.SetStratModifier(StratModifiers.AIBenefitBonus, playerID, 0);
				gameDatabase.SetStratModifier(StratModifiers.AllowAIRebellion, playerID, false);
				return;
			}
			case "CCC_A.I._Autonomy":
			{
				float stratModifier48 = gameDatabase.GetStratModifier<float>(StratModifiers.AIBenefitBonus, playerID);
				gameDatabase.SetStratModifier(StratModifiers.AIBenefitBonus, playerID, stratModifier48 + assetDatabase.GetTechBonus<float>("CCC_A.I._Autonomy", "aibenefits"));
				gameDatabase.SetStratModifier(StratModifiers.AllowAIRebellion, playerID, false);
				return;
			}
			case "CCC_A.I._Slaves":
			{
				int[] aIOldColonyOwner = gameDatabase.GetAIOldColonyOwner(playerID);
				int[] array = aIOldColonyOwner;
				for (int i = 0; i < array.Length; i++)
				{
					int colonyID2 = array[i];
					ColonyInfo colonyInfo = gameDatabase.GetColonyInfo(colonyID2);
					colonyInfo.PlayerID = playerID;
					gameDatabase.UpdateColony(colonyInfo);
				}
				if (aIOldColonyOwner.Count<int>() > 0)
				{
					gameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_AI_REBELLION_END,
						EventMessage = TurnEventMessage.EM_AI_REBELLION_END,
						PlayerID = playerID,
						TurnNumber = gameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				gameDatabase.SetStratModifier(StratModifiers.AllowAIRebellion, playerID, false);
				return;
			}
			case "DRV_Grav_Synergy":
			{
				float num5 = gameDatabase.GetStratModifier<float>(StratModifiers.MaxFlockBonusMod, playerID);
				num5 += assetDatabase.GetTechBonus<float>("DRV_Grav_Synergy", "flockbonusscale") - 1f;
				gameDatabase.SetStratModifier(StratModifiers.MaxFlockBonusMod, playerID, num5);
				return;
			}
			case "DRV_Casting":
			{
				float stratModifier49 = gameDatabase.GetStratModifier<float>(StratModifiers.GateCastDistance, playerID);
				gameDatabase.SetStratModifier(StratModifiers.GateCastDistance, playerID, stratModifier49 + assetDatabase.GetTechBonus<float>("DRV_Casting", "castdistance"));
				return;
			}
			case "DRV_Far_Casting":
			{
				float stratModifier49 = gameDatabase.GetStratModifier<float>(StratModifiers.GateCastDistance, playerID);
				gameDatabase.SetStratModifier(StratModifiers.GateCastDistance, playerID, stratModifier49 + assetDatabase.GetTechBonus<float>("DRV_Far_Casting", "castdistance"));
				return;
			}
			case "CCC_Expert_Systems":
			{
				float stratModifier7 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				float stratModifier50 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierCR, playerID);
				float stratModifier51 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierDN, playerID);
				float stratModifier52 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierLV, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier7 + 0.1f);
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierCR, playerID, stratModifier50 + assetDatabase.GetTechBonus<float>("CCC_Expert_Systems", "crproduction"));
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierDN, playerID, stratModifier51 + assetDatabase.GetTechBonus<float>("CCC_Expert_Systems", "dnproduction"));
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierLV, playerID, stratModifier52 + assetDatabase.GetTechBonus<float>("CCC_Expert_Systems", "lvproduction"));
				return;
			}
			case "ENG_Orbital_Drydocks":
			{
				float stratModifier50 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierCR, playerID);
				float stratModifier51 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierDN, playerID);
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierCR, playerID, stratModifier50 + assetDatabase.GetTechBonus<float>("ENG_Orbital_Drydocks", "crproduction"));
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierDN, playerID, stratModifier51 + assetDatabase.GetTechBonus<float>("ENG_Orbital_Drydocks", "dnproduction"));
				return;
			}
			case "ENG_Heavy_Platforms":
			{
				float stratModifier7 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier7 + assetDatabase.GetTechBonus<float>("ENG_Heavy_Platforms", "industrialoutput"));
				return;
			}
			case "IND_Materials_Applications":
			{
				float stratModifier50 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierCR, playerID);
				float stratModifier51 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierDN, playerID);
				float stratModifier52 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierLV, playerID);
				float stratModifier53 = gameDatabase.GetStratModifier<float>(StratModifiers.ConstructionCostModifierSN, playerID);
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierCR, playerID, stratModifier50 + assetDatabase.GetTechBonus<float>("IND_Materials_Applications", "crproduction"));
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierDN, playerID, stratModifier51 + assetDatabase.GetTechBonus<float>("IND_Materials_Applications", "dnproduction"));
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierLV, playerID, stratModifier52 + assetDatabase.GetTechBonus<float>("IND_Materials_Applications", "lvproduction"));
				gameDatabase.SetStratModifier(StratModifiers.ConstructionCostModifierLV, playerID, stratModifier53 + assetDatabase.GetTechBonus<float>("IND_Materials_Applications", "snproduction"));
				return;
			}
			case "POL_FTL_Economics":
				App.InitializeTrade(game, gameDatabase, playerID);
				return;
			case "DRV_Phase_Dislocation":
			{
				int stratModifier54 = gameDatabase.GetStratModifier<int>(StratModifiers.PhaseDislocationARBonus, playerID);
				gameDatabase.SetStratModifier(StratModifiers.PhaseDislocationARBonus, playerID, (float)stratModifier54 + assetDatabase.GetTechBonus<float>("DRV_Phase_Dislocation", "permarmorlayers"));
				return;
			}
			case "ENG_Modular_Construction":
			{
				float stratModifier55 = gameDatabase.GetStratModifier<float>(StratModifiers.TradeRevenue, playerID);
				gameDatabase.SetStratModifier(StratModifiers.TradeRevenue, playerID, stratModifier55 + assetDatabase.GetTechBonus<float>("ENG_Modular_Construction", "traderevenue"));
				return;
			}
			case "PSI_MechaEmpathy":
			{
				float stratModifier56 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier56 + assetDatabase.GetTechBonus<float>("PSI_MechaEmpathy", "industrialoutput"));
				return;
			}
			case "CYB_InFldManip":
			{
				float stratModifier7 = gameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, playerID);
				gameDatabase.SetStratModifier(StratModifiers.IndustrialOutputModifier, playerID, stratModifier7 + assetDatabase.GetTechBonus<float>("CYB_InFldManip", "industrialoutput"));
				break;
			}

				return;
			}
		}
		private static void InitializeTrade(GameSession game, GameDatabase gamedb, int playerID)
		{
			gamedb.SetStratModifier(StratModifiers.EnableTrade, playerID, true);
			game.CheckForNewEquipment(playerID);
			game.AvailableShipSectionsChanged();
			DesignInfo designInfo = DesignLab.DesignShip(game, ShipClass.Cruiser, ShipRole.FREIGHTER, WeaponRole.STAND_OFF, playerID);
			designInfo.Name = App.Localize("@DEFAULT_SHIPNAME_FREIGHTER");
			designInfo.isPrototyped = true;
			game.GameDatabase.InsertDesignByDesignInfo(designInfo);
		}
		private void ValidFleets(GameSession app, PendingCombat pendingCombat)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>(pendingCombat.SelectedPlayerFleets);
			foreach (KeyValuePair<int, int> current in pendingCombat.SelectedPlayerFleets)
			{
				FleetInfo fleetInfo = app.GameDatabase.GetFleetInfo(current.Value);
				List<ShipInfo> list = app.GameDatabase.GetShipInfoByFleetID(current.Value, false).ToList<ShipInfo>();
				bool flag = false;
				if (fleetInfo != null)
				{
					MissionInfo missionByFleetID = app.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
					if (missionByFleetID != null && missionByFleetID.StartingSystem != 0 && missionByFleetID.Type == MissionType.RETREAT && missionByFleetID.StartingSystem == pendingCombat.SystemID)
					{
						flag = true;
					}
				}
				if (fleetInfo == null || list.Count == 0 || flag)
				{
					foreach (int current2 in pendingCombat.FleetIDs)
					{
						FleetInfo fleetInfo2 = app.GameDatabase.GetFleetInfo(current2);
						List<ShipInfo> list2 = app.GameDatabase.GetShipInfoByFleetID(current2, false).ToList<ShipInfo>();
						bool flag2 = false;
						if (fleetInfo2 != null)
						{
							MissionInfo missionByFleetID2 = app.GameDatabase.GetMissionByFleetID(fleetInfo2.ID);
							if (missionByFleetID2 != null && missionByFleetID2.StartingSystem != 0 && missionByFleetID2.Type == MissionType.RETREAT && missionByFleetID2.StartingSystem == pendingCombat.SystemID)
							{
								flag2 = true;
							}
						}
						if (fleetInfo2 != null && list2.Count != 0 && !fleetInfo2.IsGateFleet && !fleetInfo2.IsAcceleratorFleet && !flag2 && fleetInfo2.PlayerID == current.Key)
						{
							dictionary[current.Key] = fleetInfo2.ID;
							break;
						}
					}
				}
			}
			pendingCombat.SelectedPlayerFleets = dictionary;
		}
		public void LaunchCombat(GameSession app, PendingCombat pendingCombat, bool testing, bool sim, bool authority)
		{
			this.ValidFleets(app, pendingCombat);
			GameState gameState;
			if (!sim)
			{
				gameState = this.GetGameState<CombatState>();
			}
			else
			{
				gameState = this.GetGameState<SimCombatState>();
			}
			this.SwitchGameStateViaLoadingScreen(null, null, gameState, new object[]
			{
				pendingCombat,
				null,
				testing,
				authority
			});
		}
		private void AddGameStates()
		{
			this._gameStateMachine.Add(new TestAssetsState(this));
			this._gameStateMachine.Add(new TestAnimationState(this));
			this._gameStateMachine.Add(new TestShipsState(this));
			this._gameStateMachine.Add(new TestLoadCombatState(this));
			this._gameStateMachine.Add(new TestPhysicsState(this));
			this._gameStateMachine.Add(new TestUIState(this));
			this._gameStateMachine.Add(new TestPlanetState(this));
			this._gameStateMachine.Add(new CologneShipsState(this));
			this._gameStateMachine.Add(new ResearchScreenState(this));
			this._gameStateMachine.Add(new DesignScreenState(this));
			this._gameStateMachine.Add(new DiplomacyScreenState(this));
			this._gameStateMachine.Add(new BuildScreenState(this));
			this._gameStateMachine.Add(new SplashState(this));
			this._gameStateMachine.Add(new MainMenuState(this));
			this._gameStateMachine.Add(new StarMapState(this));
			this._gameStateMachine.Add(new CombatState(this));
			this._gameStateMachine.Add(new SotspediaState(this));
			this._gameStateMachine.Add(new StarSystemState(this));
			this._gameStateMachine.Add(new GameSetupState(this));
			this._gameStateMachine.Add(new LoadGameState(this));
			this._gameStateMachine.Add(new StarMapLobbyState(this));
			this._gameStateMachine.Add(new ProfilesState(this));
			this._gameStateMachine.Add(new CinematicsState(this));
			this._gameStateMachine.Add(new EmpireSummaryState(this));
			this._gameStateMachine.Add(new TestNetworkState(this));
			this._gameStateMachine.Add(new TestCombatState(this));
			this._gameStateMachine.Add(new TestScratchCombatState(this));
			this._gameStateMachine.Add(new LoadingScreenState(this));
			this._gameStateMachine.Add(new StationPlacementState(this));
			this._gameStateMachine.Add(new PlanetManagerState(this));
			this._gameStateMachine.Add(new FleetManagerState(this));
			this._gameStateMachine.Add(new DefenseManagerState(this));
			this._gameStateMachine.Add(new MoviePlayerState(this));
			this._gameStateMachine.Add(new RiderManagerState(this));
			this._gameStateMachine.Add(new SimCombatState(this));
			this._gameStateMachine.Add(new ComparativeAnalysysState(this));
		}
		internal void Initialize()
		{
			if (this._assetDatabase != null)
			{
				throw new InvalidOperationException("Cannot initialize more than once.");
			}
			this._assetDatabase = new AssetDatabase(this);
			this.AddMaterialDictionaries(this._assetDatabase.MaterialDictionaries);
			this.AddCritHitChances(this._assetDatabase.CriticalHitChances);
			StarSystemVars.LoadXml("data\\StarSystemVars.xml");
			this._gameSetup = new GameSetup(this);
			this.ResetGameSetup();
			this.UI.Send(new object[]
			{
				"InitializeChatWidget"
			});
			if (this._steam.IsAvailable)
			{
				App.Log.Trace("Steam initialized app ID: " + this._steam.GetGameID(), "steam");
			}
			else
			{
				App.Log.Warn("Steam not available.", "steam");
			}
			this._steamHelper = new SteamHelper(this._steam);
			this.SteamHelper.DoAchievement(AchievementType.SOTS2_WELCOME);
			try
			{
				if (!string.IsNullOrEmpty(this._consoleScriptFileName))
				{
					ConsoleCommandParse.Evaluate(this, File.ReadLines(this._consoleScriptFileName));
				}
			}
			finally
			{
				this._initialized = true;
			}
			SteamDLCHelpers.LogAvailableDLC(this);
			this.PostSpeechSubtitles(this._gameSettings.SpeechSubtitles);
		}
		internal bool IsInitialized()
		{
			return this._initialized && this._materialsReady && this._receivedDirectoryInfo;
		}
		public bool CanEndTurn()
		{
			return true;
		}
		public void EndTurn()
		{
			bool flag = this.game.EndTurn(this.LocalPlayer.ID);
			if (flag)
			{
				this.game.ProcessEndTurn();
			}
		}
		private void Exit()
		{
			this._gameStateMachine.Exit();
			this._assetDatabase.Dispose();
		}
		private void LockFilesInFolder(string dir)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(dir);
			FileInfo[] files = directoryInfo.GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = files[i];
				if (!App.locks.Any((NonClosingStreamWrapper x) => ((FileStream)x.BaseStream).Name == dir))
				{
					NonClosingStreamWrapper item = new NonClosingStreamWrapper(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.ReadWrite));
					App.locks.Add(item);
				}
			}
		}
		public static Stream GetStreamForFile(string fullFilepath)
		{
			string a = fullFilepath.Substring(fullFilepath.LastIndexOf('\\') + 1);
			NonClosingStreamWrapper nonClosingStreamWrapper = null;
			foreach (NonClosingStreamWrapper current in App.locks)
			{
				string text = ((FileStream)current.BaseStream).Name;
				text = text.Substring(text.LastIndexOf('\\') + 1);
				if (a == text)
				{
					nonClosingStreamWrapper = current;
					break;
				}
			}
			if (nonClosingStreamWrapper != null)
			{
				nonClosingStreamWrapper.Close();
			}
			return nonClosingStreamWrapper;
		}
		public static void LockFileStream(string fullFilepath)
		{
			if (App.GetStreamForFile(fullFilepath) == null)
			{
				NonClosingStreamWrapper item = new NonClosingStreamWrapper(new FileStream(fullFilepath, FileMode.Open, FileAccess.ReadWrite));
				App.locks.Add(item);
			}
		}
		public static void UnLockFileStream(string fullFilepath)
		{
			string a = fullFilepath.Substring(fullFilepath.LastIndexOf('\\') + 1);
			NonClosingStreamWrapper nonClosingStreamWrapper = null;
			foreach (NonClosingStreamWrapper current in App.locks)
			{
				string text = ((FileStream)current.BaseStream).Name;
				text = text.Substring(text.LastIndexOf('\\') + 1);
				if (a == text)
				{
					nonClosingStreamWrapper = current;
					break;
				}
			}
			if (nonClosingStreamWrapper != null)
			{
				nonClosingStreamWrapper.CloseContainer();
				App.locks.Remove(nonClosingStreamWrapper);
			}
		}
		private static void UnlockAllStreams()
		{
			foreach (NonClosingStreamWrapper current in App.locks)
			{
				current.CloseContainer();
			}
			App.locks.Clear();
		}
		private void ProcessEngineMessage(ScriptMessageReader mr)
		{
			InteropMessageID interopMessageID = (InteropMessageID)mr.ReadInteger();
			InteropMessageID interopMessageID2 = interopMessageID;
			switch (interopMessageID2)
			{
			case InteropMessageID.IMID_SCRIPT_DIRECTORIES:
			{
				this._profileDir = mr.ReadString();
				this._baseSaveDir = mr.ReadString();
				this._cacheDir = mr.ReadString();
				this._settingsDir = mr.ReadString();
				this._receivedDirectoryInfo = true;
				this.LockFilesInFolder(this._profileDir);
				this.LockFilesInFolder(this._settingsDir);
				Profile.SetProfileDirectory(this._profileDir);
				this._gameSettings = new Settings(this._settingsDir);
				this._gameSettings.Commit(this);
				HotKeyManager.SetHotkeyProfileDirectory(this._profileDir);
				this.HotKeyManager.LoadProfile("~Default", false);
				string lastProfile = this._gameSettings.LastProfile;
				if (lastProfile != null)
				{
					this._userProfile.LoadProfile(lastProfile, false);
					if (this._userProfile.Loaded)
					{
						this._profileSelected = true;
					}
				}
				else
				{
					List<Profile> availableProfiles = Profile.GetAvailableProfiles();
					if (availableProfiles.Count != 0)
					{
						this._userProfile = availableProfiles.First<Profile>();
						this._gameSettings.LastProfile = this._userProfile.ProfileName;
						this._profileSelected = true;
					}
				}
				if (this._profileSelected)
				{
					if (!HotKeyManager.GetAvailableProfiles().Contains(this._userProfile.ProfileName))
					{
						this._hotkeyManager.CreateProfile(this._userProfile.ProfileName);
					}
					this._hotkeyManager.LoadProfile(this._userProfile.ProfileName, false);
					return;
				}
				return;
			}
			case InteropMessageID.IMID_SCRIPT_DIALOG:
				this.UI.HandleDialogMessage(mr);
				return;
			case InteropMessageID.IMID_ENGINE_MATERIALS_ADD:
			case InteropMessageID.IMID_ENGINE_MATERIALS_REQ_READY:
			case InteropMessageID.IMID_ENGINE_GAME_REPORT_EVENT:
			case InteropMessageID.IMID_ENGINE_SET_AUTHORITIVE_STATE:
			case InteropMessageID.IMID_ENGINE_OBJECT_ADD:
			case InteropMessageID.IMID_ENGINE_OBJECT_ADDED:
			case InteropMessageID.IMID_ENGINE_OBJECTS_ADDED:
			case InteropMessageID.IMID_ENGINE_OBJECT_RELEASE:
			case InteropMessageID.IMID_ENGINE_OBJECT_RELEASEMULTI:
			case InteropMessageID.IMID_ENGINE_OBJECT_SETPROP:
			case InteropMessageID.IMID_ENGINE_OBJECT_SETINT:
			case InteropMessageID.IMID_ENGINE_OBJECT_SETPLAYER:
			case InteropMessageID.IMID_ENGINE_CLEAR_WEAPON_SPAWNS:
				goto IL_2B9;
			case InteropMessageID.IMID_SCRIPT_MATERIALS_READY:
				this._materialsReady = true;
				return;
			case InteropMessageID.IMID_SCRIPT_SET_PAUSE_STATE:
			case InteropMessageID.IMID_SCRIPT_SET_COMBAT_ACTIVE:
			case InteropMessageID.IMID_SCRIPT_PLAYER_DIPLO_CHANGED:
			case InteropMessageID.IMID_SCRIPT_ZONE_OWNER_CHANGED:
			case InteropMessageID.IMID_SCRIPT_OBJECT_ADD:
			case InteropMessageID.IMID_SCRIPT_OBJECTS_ADD:
			case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
			case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
			case InteropMessageID.IMID_SCRIPT_MOVE_ORDER:
				break;
			case InteropMessageID.IMID_SCRIPT_OBJECT_STATUS:
				this._gameObjectMediator.OnObjectStatus(mr.ReadInteger(), (GameObjectStatus)mr.ReadInteger());
				return;
			case InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP:
			case InteropMessageID.IMID_SCRIPT_OBJECT_SETPLAYER:
			case InteropMessageID.IMID_SCRIPT_MANEUVER_INFO:
			case InteropMessageID.IMID_SCRIPT_FORMATION_REMOVE_SHIP:
				this._gameObjectMediator.OnObjectScriptMessage(interopMessageID, mr.ReadInteger(), mr);
				return;
			default:
				switch (interopMessageID2)
				{
				case InteropMessageID.IMID_SCRIPT_NETWORK:
					if (this._network != null)
					{
						this._network.ProcessEngineMessage(mr);
						return;
					}
					return;
				case InteropMessageID.IMID_ENGINE_COMBAT_ENDED:
				case InteropMessageID.IMID_ENGINE_COMBAT_CONNECT_UI:
				case InteropMessageID.IMID_ENGINE_COMBAT_SYNC_LISTENERS:
				case InteropMessageID.IMID_ENGINE_NEW_GAME:
				case InteropMessageID.IMID_ENGINE_SET_LOCAL_PLAYER:
				case InteropMessageID.IMID_ENGINE_SYNC_SOTSPEDIA_KEYS:
					goto IL_2B9;
				case InteropMessageID.IMID_SCRIPT_COMBAT_ENDED:
				case InteropMessageID.IMID_SCRIPT_START_SENDINGDATA:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_SHIP:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_PLANET:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DATA_STAR:
				case InteropMessageID.IMID_SCRIPT_COMBAT_DESTROYED_SHIPS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_CAPTURED_SHIPS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_WEAPON_DAMAGE_STATS:
				case InteropMessageID.IMID_SCRIPT_COMBAT_ZONE_STATES:
				case InteropMessageID.IMID_SCRIPT_END_SENDINGDATA:
				case InteropMessageID.IMID_SCRIPT_END_DELAYCOMPLETE:
				case InteropMessageID.IMID_SCRIPT_SYNC_FLEET_POSITIONS:
				case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSE_POSITIONS:
				case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSEBOAT_DATA:
					break;
				case InteropMessageID.IMID_SCRIPT_KEYCOMBO:
				case InteropMessageID.IMID_SCRIPT_VKREPORT:
					this._hotkeyManager.OnEngineMessage(interopMessageID, mr);
					return;
				default:
					goto IL_2B9;
				}
				break;
			}
			this._gameStateMachine.OnEngineMessage(interopMessageID, mr);
			return;
			IL_2B9:
			App.Log.Warn("Unhandled message (id=" + interopMessageID + ").", "engine");
		}
		public void Update()
		{
			try
			{
				ConsoleCommandParse.ProcessConsoleCommands(this, this._consoleApplet);
				foreach (ScriptMessageReader current in this._scriptCommChannel.PumpMessages())
				{
					this.ProcessEngineMessage(current);
				}
				this._uiCommChannel.Update();
				this._gameStateMachine.Update();
				App.m_Commands.Poll();
				this._ircChat.Update();
				if (this.GameSetup != null && this.Game != null && this.Game.TurnTimer.IsRunning() && this.Game.TurnTimer.StrategicTurnLength != 0f)
				{
					TimeSpan turnTime = this.Game.TurnTimer.GetTurnTime();
					float num = (float)turnTime.Seconds + (float)turnTime.Minutes * 60f;
					if (num >= this.Game.TurnTimer.StrategicTurnLength * 60f)
					{
						this.Game.TurnTimer.StopTurnTimer();
						this.GetGameState<StarMapState>().EndTurn(false);
					}
					if (this.GameSetup.IsMultiplayer)
					{
						this.Network.SetTime((float)turnTime.TotalSeconds, this.Game.TurnTimer.StrategicTurnLength);
					}
				}
			}
			catch (Exception ex)
			{
				App.Log.Warn(ex.ToString(), "game");
				if (this._numExceptionErrorsDisplayed < 2)
				{
					if (this._numExceptionErrorsDisplayed == 0 && this.Game != null)
					{
						string filename = Path.Combine(Path.GetDirectoryName(App.Log.FilePath), Path.GetFileNameWithoutExtension(App.Log.FilePath)) + "_localgame.db";
						App.Log.Warn("Writing local game database for debugging...", "game");
						this.Game.GameDatabase.Save(filename);
					}
					MessageBox.Show(string.Format("An error occurred:\n{0}\n\nRefer to log file for more information:\n{1}", ex.ToString(), App.Log.FilePath), "SotS Error");
					throw;
				}
			}
		}
		public void Exiting()
		{
		}
		private static List<App.StartLocation> CollectStartLocations(LegacyStarMap starmap)
		{
			List<App.StartLocation> list = new List<App.StartLocation>();
			IEnumerable<StarSystem> enumerable = 
				from x in starmap.Objects.OfType<StarSystem>()
				where x.IsStartPosition
				select x;
			foreach (StarSystem current in enumerable)
			{
				list.Add(new App.StartLocation
				{
					System = current
				});
			}
			return list;
		}
		private static List<App.StartLocation> RandomizeStartLocations(List<App.StartLocation> choices, IList<PlayerSetup> players)
		{
			List<App.StartLocation> list = new List<App.StartLocation>();
			int i = 0;
			while (i < players.Count<PlayerSetup>() && choices.Count != 0)
			{
				PlayerSetup playerSetup = players[i];
				App.StartLocation startLocation = null;
				if (!string.IsNullOrEmpty(playerSetup.Faction))
				{
					startLocation = choices.FirstOrDefault((App.StartLocation x) => x.System.Params.PlayerSlot == i + 1);
				}
				if (startLocation == null)
				{
					startLocation = choices.FirstOrDefault((App.StartLocation x) => !x.System.Params.PlayerSlot.HasValue);
				}
				if (startLocation == null)
				{
					startLocation = choices[0];
				}
				if (startLocation != null)
				{
					list.Add(new App.StartLocation
					{
						System = startLocation.System,
						Planet = startLocation.Planet,
						PlayerIndex = i
					});
					choices.Remove(startLocation);
				}
				i++;
			}
			return list;
		}
		private static void ConfigureStartLocation(Random random, App.StartLocation loc, IList<PlayerSetup> players, GameDatabase db)
		{
			PlanetOrbit planetOrbit = (PlanetOrbit)loc.Planet.Params;
			bool flag = !planetOrbit.CapitalOrbit;
			if (flag || string.IsNullOrEmpty(planetOrbit.PlanetType))
			{
				planetOrbit.PlanetType = StellarBodyTypes.Normal;
			}
			if (flag || !planetOrbit.Resources.HasValue)
			{
				planetOrbit.Resources = new int?(5000);
			}
			if (flag || !planetOrbit.Size.HasValue)
			{
				planetOrbit.Size = new int?(10);
			}
			if (flag || !planetOrbit.Biosphere.HasValue)
			{
				planetOrbit.Biosphere = new int?(3000);
			}
			planetOrbit.Suitability = new float?(db.GetFactionSuitability(players[loc.PlayerIndex].Faction));
		}
		private static void EnsureInhabitableStartLocation(Random random, List<App.StartLocation> startLocations, IList<PlayerSetup> players, GameDatabase db)
		{
			foreach (App.StartLocation current in startLocations)
			{
				if (current.Planet == null)
				{
					IEnumerable<IStellarEntity> colonizableWorlds = current.System.GetColonizableWorlds(true);
					IEnumerable<IStellarEntity> enumerable = 
						from x in colonizableWorlds
						where (x.Params as PlanetOrbit).CapitalOrbit
						select x;
					if (enumerable.Any<IStellarEntity>())
					{
						current.Planet = random.Choose(enumerable);
						App.ConfigureStartLocation(random, current, players, db);
					}
					else
					{
						if (colonizableWorlds.Any<IStellarEntity>())
						{
							current.Planet = random.Choose(colonizableWorlds);
							App.ConfigureStartLocation(random, current, players, db);
						}
						else
						{
							int num = current.System.Objects.Max(delegate(IStellarEntity x)
							{
								if (x.Orbit != null)
								{
									return x.Orbit.OrbitNumber;
								}
								return 1;
							});
							int orbitNumber = num + 1;
							IEnumerable<IStellarEntity> enumerable2 = StarSystemHelper.CreatePlanet(random, new PlanetOrbit
							{
								OrbitNumber = orbitNumber
							}, true);
							foreach (IStellarEntity current2 in 
								from x in enumerable2
								where x.Orbit == null
								select x)
							{
								current2.Orbit = StarSystem.SetOrbit(random, current.System.Star.Params, current2.Params);
							}
							current.System.AddRange(enumerable2);
							current.Planet = enumerable2.First<IStellarEntity>();
							App.ConfigureStartLocation(random, current, players, db);
						}
					}
				}
			}
		}
		public static string GetFactionIcon(string faction)
		{
			if (faction == null)
			{
				return string.Empty;
			}
			switch (faction)
			{
			case "human":
				return "sotspedia_humanlogo";
			case "hiver":
				return "sotspedia_hiverlogo";
			case "liir_zuul":
				return "sotspedia_liirzuullogo";
			case "morrigi":
				return "sotspedia_moriggilogo";
			case "tarkas":
				return "sotspedia_tarklogo";
			case "zuul":
				return "sotspedia_suulkazuullogo";
			case "loa":
				return "sotspedia_loalogo";
			}
			return string.Empty;
		}
		public static string GetFactionDescription(string faction)
		{
			if (faction == null)
			{
				return string.Empty;
			}
			switch (faction)
			{
			case "human":
				return App.Localize("@FACTION_DESCRIPTION_SOL_FORCE");
			case "hiver":
				return App.Localize("@FACTION_DESCRIPTION_HIVER_IMPERIUM");
			case "tarkas":
				return App.Localize("@FACTION_DESCRIPTION_TARKASIAN_EMPIRE");
			case "liir_zuul":
				return App.Localize("@FACTION_DESCRIPTION_LIIR_ZUUL_ALLIANCE");
			case "zuul":
				return App.Localize("@FACTION_DESCRIPTION_SUULKA_HORDE");
			case "morrigi":
				return App.Localize("@FACTION_DESCRIPTION_MORRIGI_CONFEDERATION");
			case "loa":
				return App.Localize("@FACTION_DESCRIPTION_LOA");
			}
			return string.Empty;
		}
		public static string GetLocalizedFactionName(string faction)
		{
			switch (faction)
			{
			case "human":
				return App.Localize("@FACTION_SOL_FORCE");
			case "hiver":
				return App.Localize("@FACTION_HIVER_IMPERIUM");
			case "tarkas":
				return App.Localize("@FACTION_TARKASIAN_EMPIRE");
			case "liir_zuul":
				return App.Localize("@FACTION_LIIR_ZUUL_ALLIANCE");
			case "zuul":
				return App.Localize("@FACTION_SUULKA_HORDE");
			case "morrigi":
				return App.Localize("@FACTION_MORRIGI_CONFEDERATION");
			case "loa":
				return App.Localize("@FACTION_LOA");
			}
			return "!!EMPTY FACTION!!";
		}
		public int? GetFactionID(string name)
		{
			foreach (Faction current in this._assetDatabase.Factions)
			{
				if (current.Name == name)
				{
					return new int?(current.ID);
				}
			}
			return null;
		}
		public static Random GetSafeRandom()
		{
			if (App._safeRandom == null)
			{
				App._safeRandom = new Random();
			}
			return App._safeRandom;
		}
	}
}
