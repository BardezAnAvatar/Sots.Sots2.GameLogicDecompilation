using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class GameSetup
	{
		public const int MinStarCount = 30;
		public const int MaxStarCount = 200;
		public const int MinInitialTreasury = 0;
		public const int MaxInitialTreasury = 1000000;
		public const int DefaultInitialTreasury = 500000;
		public const int MinInitialSystems = 3;
		public const int MaxInitialSystems = 9;
		public const int MinInitialTechnologies = 0;
		public const int MaxInitialTechnologies = 10;
		public const int MinPlayerCount = 2;
		public const int MaxPlayerCount = 8;
		public const int MinGrandMenaceCount = 0;
		public const int MaxGrandMenaceCount = 5;
		public const float MinStrategicTurnLength = 0.25f;
		public const float MaxStrategicTurnLength = 15f;
		public const float DefaultStrategicTurnLength = 3.40282347E+38f;
		public const float StrategicTurnLengthGranularity = 0.25f;
		public const float MinCombatTurnLength = 3f;
		public const float MaxCombatTurnLength = 12f;
		public const float CombatTurnLengthGranularity = 1f;
		public const float DefaultCombatTurnLength = 5f;
		public GameMode _mode;
		public int _modeValue;
		public int _starCount;
		public int _starSize;
		public int _averageResources;
		public int _initialTreasury;
		public int _initialSystems;
		public int _initialTechs;
		public int _economicEfficiency;
		public int _researchEfficiency;
		public int _randomEncounterFrequency;
		public int _grandMenaceCount;
		public bool _inProgress;
		private App _game;
		public bool IsMultiplayer;
		private List<PlayerSetup> _players = new List<PlayerSetup>();
		public string GameName
		{
			get;
			private set;
		}
		public string StarMapFile
		{
			get;
			set;
		}
		public string ScenarioFile
		{
			get;
			set;
		}
		public int StarCount
		{
			get
			{
				return this._starCount;
			}
			set
			{
				this._starCount = value.Clamp(30, 200);
			}
		}
		public int PlanetSize
		{
			get
			{
				return this._starSize;
			}
			set
			{
				this._starSize = value.Clamp(50, 150);
			}
		}
		public int PlanetResources
		{
			get
			{
				return this._averageResources;
			}
			set
			{
				this._averageResources = value.Clamp(50, 150);
			}
		}
		public float StrategicTurnLength
		{
			get;
			set;
		}
		public float CombatTurnLength
		{
			get;
			set;
		}
		public int EconomicEfficiency
		{
			get
			{
				return this._economicEfficiency;
			}
			set
			{
				this._economicEfficiency = value.Clamp(50, 200);
			}
		}
		public int ResearchEfficiency
		{
			get
			{
				return this._researchEfficiency;
			}
			set
			{
				this._researchEfficiency = value.Clamp(50, 200);
			}
		}
		public int InitialTreasury
		{
			get
			{
				return this._initialTreasury;
			}
			set
			{
				this._initialTreasury = Math.Max(0, value);
			}
		}
		public int InitialSystems
		{
			get
			{
				return this._initialSystems;
			}
			set
			{
				this._initialSystems = Math.Max(0, value);
			}
		}
		public int InitialTechnologies
		{
			get
			{
				return this._initialTechs;
			}
			set
			{
				this._initialTechs = Math.Max(0, value);
			}
		}
		public int RandomEncounterFrequency
		{
			get
			{
				return this._randomEncounterFrequency;
			}
			set
			{
				this._randomEncounterFrequency = value.Clamp(0, 200);
			}
		}
		public int GrandMenaceCount
		{
			get
			{
				return this._grandMenaceCount;
			}
			set
			{
				this._grandMenaceCount = value;
			}
		}
		public AvailablePlayerFeatures AvailablePlayerFeatures
		{
			get;
			set;
		}
		public List<PlayerSetup> Players
		{
			get
			{
				return this._players;
			}
		}
		public PlayerSetup LocalPlayer
		{
			get
			{
				foreach (PlayerSetup current in this._players)
				{
					if (current.localPlayer)
					{
						return current;
					}
				}
				return null;
			}
		}
		public bool TrySetGameName(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			value = value.Trim();
			return true;
		}
		public string GetDefaultSaveGameFileName()
		{
			string gameName = this.GameName;
			char[] array = new char[gameName.Length];
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			char[] invalidPathChars = Path.GetInvalidPathChars();
			for (int i = 0; i < array.Length; i++)
			{
				char c = gameName[i];
				if (invalidFileNameChars.Contains(c) || invalidPathChars.Contains(c))
				{
					c = '_';
				}
				array[i] = c;
			}
			return new string(array);
		}
		public bool HasStarMapFile()
		{
			return !string.IsNullOrEmpty(this.StarMapFile);
		}
		public bool HasScenarioFile()
		{
			return !string.IsNullOrEmpty(this.ScenarioFile);
		}
		public void SetPlayerCount(int numPlayers)
		{
			if (numPlayers < this.Players.Count)
			{
				while (this.Players.Count > numPlayers)
				{
					if (this.IsMultiplayer)
					{
						this._game.Network.Kick(this.Players.Count - 1);
					}
					this.Players.RemoveAt(this.Players.Count - 1);
				}
				return;
			}
			if (numPlayers > this.Players.Count)
			{
				while (this.Players.Count < numPlayers)
				{
					this.Players.Add(new PlayerSetup());
					this.Players.Last<PlayerSetup>().slot = this.Players.Count<PlayerSetup>() - 1;
				}
			}
		}
		public static string PlayerStatusToString(NPlayerStatus status)
		{
			switch (status)
			{
			case NPlayerStatus.PS_LOBBY:
				return App.Localize("@PLAYER_STATUS_IN_LOBBY");
			case NPlayerStatus.PS_READY:
				return App.Localize("@PLAYER_STATUS_READY");
			case NPlayerStatus.PS_DOWNLOADING:
				return App.Localize("@PLAYER_STATUS_RECEIVING_DATA");
			case NPlayerStatus.PS_TURN:
				return App.Localize("@PLAYER_STATUS_PLAYING");
			case NPlayerStatus.PS_WAIT:
				return App.Localize("@PLAYER_STATUS_WAITING");
			case NPlayerStatus.PS_REACTION:
				return App.Localize("@PLAYER_STATUS_REACTION_MOVEMENT");
			case NPlayerStatus.PS_PRECOMBAT:
				return App.Localize("@PLAYER_STATUS_WAITING_FOR_COMBAT");
			case NPlayerStatus.PS_COMBAT_WAIT:
				return App.Localize("@PLAYER_STATUS_WAITING_FOR_COMBAT");
			case NPlayerStatus.PS_COMBAT:
				return App.Localize("@PLAYER_STATUS_IN_COMBAT");
			case NPlayerStatus.PS_POSTCOMBAT:
				return App.Localize("@PLAYER_STATUS_FINISHING_TURN");
			case NPlayerStatus.PS_DEFEATED:
				return App.Localize("@PLAYER_STATUS_DEFEATED");
			default:
				return "";
			}
		}
		public GameSetup(App game)
		{
			this.GameName = App.Localize("@DEFAULT_GAME_NAME");
			this._game = game;
			this.Reset(this._game.AssetDatabase.Factions);
		}
		public void FinalizeSetup()
		{
			foreach (PlayerSetup current in this.Players)
			{
				current.FinalizeSetup(this._game, this.AvailablePlayerFeatures);
				current.Fixed = true;
				if (this._game.GameSetup.IsMultiplayer)
				{
					this._game.Network.SetPlayerInfo(current, current.slot);
				}
				if (current.localPlayer)
				{
					if (current.EmpireColor.HasValue)
					{
						this._game.UI.Send(new object[]
						{
							"SetLocalPlayerColor",
							Player.DefaultPrimaryPlayerColors[current.EmpireColor.Value]
						});
					}
					this._game.UI.Send(new object[]
					{
						"SetLocalSecondaryColor",
						current.ShipColor
					});
				}
			}
			if (!this._game.GameSetup.IsMultiplayer)
			{
				if (this.Players[0].EmpireColor.HasValue)
				{
					this._game.UI.Send(new object[]
					{
						"SetLocalPlayerColor",
						Player.DefaultPrimaryPlayerColors[this.Players[0].EmpireColor.Value]
					});
				}
				this._game.UI.Send(new object[]
				{
					"SetLocalSecondaryColor",
					this.Players[0].ShipColor
				});
				this._game.GameSetup.SetAllPlayerStatus(NPlayerStatus.PS_TURN);
			}
		}
		public void SavePlayerSlots(GameDatabase db)
		{
			List<PlayerClientInfo> list = db.GetPlayerClientInfos().ToList<PlayerClientInfo>();
			foreach (PlayerSetup playerSetup in this.Players)
			{
				if (!playerSetup.AI)
				{
					PlayerClientInfo playerClientInfo = list.FirstOrDefault((PlayerClientInfo x) => x.PlayerID == playerSetup.databaseId);
					if (playerClientInfo != null)
					{
						if (playerClientInfo.UserName != playerSetup.Name)
						{
							playerClientInfo.UserName = playerSetup.Name;
							db.UpdatePlayerClientInfo(playerClientInfo);
						}
					}
					else
					{
						playerClientInfo = new PlayerClientInfo
						{
							PlayerID = playerSetup.databaseId,
							UserName = playerSetup.Name
						};
						list.Add(playerClientInfo);
						db.InsertPlayerClientInfo(playerClientInfo);
					}
				}
			}
		}
		public void Reset(IEnumerable<Faction> availableFactions)
		{
			this.IsMultiplayer = false;
			this.StarMapFile = string.Empty;
			this._starCount = 80;
			this._starSize = 100;
			this._averageResources = 100;
			this._initialTreasury = 500000;
			this._initialSystems = 3;
			this._initialTechs = 0;
			this._economicEfficiency = 100;
			this._researchEfficiency = 100;
			this._randomEncounterFrequency = 100;
			this._grandMenaceCount = 1;
			this.StrategicTurnLength = 3.40282347E+38f;
			this.CombatTurnLength = 5f;
			this.ResetPlayers();
			this.AvailablePlayerFeatures = new AvailablePlayerFeatures(this._game.AssetDatabase, new Random(), availableFactions);
		}
		public void ResetPlayers()
		{
			this._players.Clear();
			this.SetPlayerCount(2);
		}
		public void SetEmpireColor(int slot, int? empireColorId)
		{
			if (!empireColorId.HasValue || empireColorId == -1)
			{
				this.Players[slot].EmpireColor = null;
				return;
			}
			if (empireColorId == this.Players[slot].EmpireColor)
			{
				this.AvailablePlayerFeatures.EmpireColors.Take(empireColorId.Value);
				return;
			}
			if (this.AvailablePlayerFeatures.EmpireColors.IsTaken(empireColorId.Value))
			{
				return;
			}
			if (this.Players[slot].EmpireColor.HasValue)
			{
				this.AvailablePlayerFeatures.EmpireColors.Replace(this.Players[slot].EmpireColor.Value);
			}
			this.AvailablePlayerFeatures.EmpireColors.Take(empireColorId.Value);
			this.Players[slot].EmpireColor = new int?(empireColorId.Value);
		}
		public void ReplaceAvatar(string faction, string avatar)
		{
			if (string.IsNullOrEmpty(faction) || string.IsNullOrEmpty(avatar))
			{
				return;
			}
			Faction faction2 = this._game.AssetDatabase.GetFaction(faction);
			this.AvailablePlayerFeatures.Factions[faction2].Avatars.Replace(avatar);
		}
		public void ReplaceBadge(string faction, string badge)
		{
			if (string.IsNullOrEmpty(faction) || string.IsNullOrEmpty(badge))
			{
				return;
			}
			Faction faction2 = this._game.AssetDatabase.GetFaction(faction);
			this.AvailablePlayerFeatures.Factions[faction2].Badges.Replace(badge);
		}
		public void NextTeam(int slot)
		{
			int num = this.Players[slot].Team + 1;
			while (!this.CanJoinTeam(slot, num))
			{
				if (num > this.Players.Count<PlayerSetup>())
				{
					num = 0;
				}
				else
				{
					num++;
				}
			}
			this.SetTeam(slot, num);
		}
		public bool CanJoinTeam(int slot, int team)
		{
			return team >= 0 && team <= this.Players.Count<PlayerSetup>() - 2 && (team == 0 || this.Players[slot].Team == team || (
				from x in this.Players
				where x.Team == team
				select x).Count<PlayerSetup>() < this.Players.Count<PlayerSetup>() - 1);
		}
		public bool SetTeam(int slot, int team)
		{
			if (!this.CanJoinTeam(slot, team))
			{
				return false;
			}
			this.Players[slot].Team = team;
			return true;
		}
		public string SetAvatar(int slot, string faction, string avatar)
		{
			Faction faction2 = this._game.AssetDatabase.GetFaction(faction);
			if (faction2 == null)
			{
				this.Players[slot].Avatar = string.Empty;
				return string.Empty;
			}
			if (string.IsNullOrEmpty(avatar))
			{
				if (this.Players[slot].Avatar != null)
				{
					this.AvailablePlayerFeatures.Factions[faction2].Avatars.Replace(this.Players[slot].Avatar);
				}
				this.Players[slot].Avatar = string.Empty;
				return avatar;
			}
			if (avatar == this.Players[slot].Avatar)
			{
				this.AvailablePlayerFeatures.Factions[faction2].Avatars.Take(avatar);
				return avatar;
			}
			if (this.AvailablePlayerFeatures.Factions[faction2].Avatars.IsTaken(avatar))
			{
				return avatar;
			}
			if (this.Players[slot].Avatar != null)
			{
				this.AvailablePlayerFeatures.Factions[faction2].Avatars.Replace(this.Players[slot].Avatar);
			}
			if (!this.AvailablePlayerFeatures.Factions[faction2].Avatars.Take(avatar))
			{
				avatar = this.AvailablePlayerFeatures.Factions[faction2].Avatars.TakeRandom();
			}
			this.Players[slot].Avatar = avatar;
			return avatar;
		}
		public string SetBadge(int slot, string faction, string badge)
		{
			Faction faction2 = this._game.AssetDatabase.GetFaction(faction);
			if (string.IsNullOrEmpty(badge))
			{
				if (this.Players[slot].Badge != null)
				{
					this.AvailablePlayerFeatures.Factions[faction2].Badges.Replace(this.Players[slot].Badge);
				}
				this.Players[slot].Badge = null;
				return badge;
			}
			if (badge == this.Players[slot].Badge)
			{
				this.AvailablePlayerFeatures.Factions[faction2].Badges.Take(badge);
				return badge;
			}
			if (this.AvailablePlayerFeatures.Factions[faction2].Badges.IsTaken(badge))
			{
				return badge;
			}
			if (this.Players[slot].Badge != null)
			{
				this.AvailablePlayerFeatures.Factions[faction2].Badges.Replace(this.Players[slot].Badge);
			}
			if (!this.AvailablePlayerFeatures.Factions[faction2].Badges.Take(badge))
			{
				badge = this.AvailablePlayerFeatures.Factions[faction2].Badges.TakeRandom();
			}
			this.Players[slot].Badge = badge;
			return badge;
		}
		public bool IsEmpireColorUsed(int index)
		{
			return this.AvailablePlayerFeatures.EmpireColors.IsTaken(index);
		}
		public bool IsAvatarUsed(string avatar)
		{
			return this.AvailablePlayerFeatures.Factions.Values.Any((AvailableFactionFeatures x) => x.Avatars.IsTaken(avatar));
		}
		public bool IsBadgeUsed(string badge)
		{
			return this.AvailablePlayerFeatures.Factions.Values.Any((AvailableFactionFeatures x) => x.Badges.IsTaken(badge));
		}
		public void ClearUsedAvatars()
		{
			foreach (AvailableFactionFeatures current in this.AvailablePlayerFeatures.Factions.Values)
			{
				current.Avatars.Reset();
			}
		}
		public void ClearUsedEmpireColors()
		{
			this.AvailablePlayerFeatures.EmpireColors.Reset();
		}
		public void ClearUsedBadges()
		{
			foreach (AvailableFactionFeatures current in this.AvailablePlayerFeatures.Factions.Values)
			{
				current.Badges.Reset();
			}
		}
		public Vector3 GetEmpireColor(int? colorId)
		{
			if (colorId.HasValue)
			{
				return Player.DefaultPrimaryPlayerColors[colorId.Value];
			}
			return Vector3.One;
		}
		public void SetDifficulty(int slot, string difficultyItemId)
		{
			if (difficultyItemId == "easy")
			{
				this.Players[slot].AIDifficulty = AIDifficulty.Easy;
			}
			if (difficultyItemId == "medium")
			{
				this.Players[slot].AIDifficulty = AIDifficulty.Normal;
			}
			if (difficultyItemId == "hard")
			{
				this.Players[slot].AIDifficulty = AIDifficulty.Hard;
			}
			if (difficultyItemId == "veryhard")
			{
				this.Players[slot].AIDifficulty = AIDifficulty.VeryHard;
			}
		}
		public bool SetShipColor(int slot, Vector3 shipColor)
		{
			if (slot >= 0 && slot < this.Players.Count<PlayerSetup>())
			{
				this.Players[slot].ShipColor = shipColor;
				return true;
			}
			return false;
		}
		public List<string> GetAvailableBadges(string faction)
		{
			if (string.IsNullOrEmpty(faction))
			{
				return new List<string>();
			}
			Faction faction2 = this._game.AssetDatabase.GetFaction(faction);
			return this.AvailablePlayerFeatures.Factions[faction2].Badges.GetAvailableItems().ToList<string>();
		}
		public List<string> GetAvailableAvatars(string faction)
		{
			if (string.IsNullOrEmpty(faction))
			{
				return new List<string>();
			}
			Faction faction2 = this._game.AssetDatabase.GetFaction(faction);
			return this.AvailablePlayerFeatures.Factions[faction2].Avatars.GetAvailableItems().ToList<string>();
		}
		public void SetAllPlayerStatus(NPlayerStatus status)
		{
			foreach (PlayerSetup current in this.Players)
			{
				if (current.Status != NPlayerStatus.PS_DEFEATED)
				{
					current.Status = status;
				}
			}
		}
	}
}
