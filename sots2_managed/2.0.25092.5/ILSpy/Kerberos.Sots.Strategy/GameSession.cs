using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.StarSystemPathing;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class GameSession : IDisposable
	{
		private enum IntelMissionResults
		{
			CriticalSuccess,
			Success,
			Failure,
			CriticalFailure
		}
		private class SimPlayerInfo
		{
			public ShipSectionCollection AvailableShipSections;
		}
		[Flags]
		public enum Flags
		{
			NoNewGameMessage = 1,
			ResumingGame = 2,
			NoTechTree = 4,
			NoScriptModules = 8,
			NoDefaultFleets = 16,
			NoOrbitalObjects = 32,
			NoGameSetup = 64
		}
		public enum VictoryStatus
		{
			Win,
			Loss,
			Draw
		}
		public enum FleetLeaveReason
		{
			TRAVEL,
			KILLED
		}
		public struct IntersectingSystem
		{
			public int SystemID;
			public float Distance;
			public bool StartOrEnd;
		}
		private sealed class NetRevenueSummary
		{
			public readonly double TradeRevenue;
			public readonly double TaxRevenue;
			public readonly double IORevenue;
			public readonly double SavingsInterest;
			public readonly double ColonySupportCost;
			public readonly double UpkeepCost;
			public readonly double CorruptionExpenses;
			public readonly double DebtInterest;
			public double GetNetRevenue()
			{
				return this.TradeRevenue + this.TaxRevenue + this.IORevenue + this.SavingsInterest - this.ColonySupportCost - this.UpkeepCost - this.CorruptionExpenses - this.DebtInterest;
			}
			public NetRevenueSummary(App game, int playerId, double tradeRevenue)
            {
                Func<TreatyInfo, bool> predicate = null;
                Func<ColonyInfo, double> selector = null;
                Func<ColonyInfo, double> func3 = null;
                GameDatabase db = game.GameDatabase;
                AssetDatabase assetdb = game.AssetDatabase;
                List<ColonyInfo> source = db.GetPlayerColoniesByPlayerId(playerId).ToList<ColonyInfo>();
                List<ColonyInfo> list2 = new List<ColonyInfo>();
                if (predicate == null)
                {
                    predicate = x => (x.Type == TreatyType.Protectorate) && (x.InitiatingPlayerId == playerId);
                }
                foreach (TreatyInfo info in db.GetTreatyInfos().ToList<TreatyInfo>().Where<TreatyInfo>(predicate).ToList<TreatyInfo>())
                {
                    if (info.Active)
                    {
                        list2.AddRange(db.GetPlayerColoniesByPlayerId(info.ReceivingPlayerId));
                    }
                }
                PlayerInfo playerInfo = db.GetPlayerInfo(playerId);
                this.IORevenue = this.GetBaseIndustrialOutputRevenue(game.Game, db, playerInfo) * game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.IORevenue, playerInfo.ID);
                this.TradeRevenue = tradeRevenue;
                if (selector == null)
                {
                    selector = x => Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetTaxRevenue(game, x);
                }
                if (func3 == null)
                {
                    func3 = x => Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetTaxRevenue(game, x);
                }
                this.TaxRevenue = source.Sum<ColonyInfo>(selector) + list2.Sum<ColonyInfo>(func3);
                float num = db.GetNameValue<float>("EconomicEfficiency") / 100f;
                this.TradeRevenue *= num;
                this.TradeRevenue *= game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TradeRevenue, playerId);
                this.TaxRevenue *= num;
                this.SavingsInterest = GameSession.CalculateSavingsInterest(game.Game, playerInfo);
                this.DebtInterest = GameSession.CalculateDebtInterest(game.Game, playerInfo);
                this.ColonySupportCost = source.Sum<ColonyInfo>((Func<ColonyInfo, double>) (x => Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetColonySupportCost(assetdb, db, x)));
                Kerberos.Sots.PlayerFramework.Player player = game.GetPlayer(playerId);
                this.UpkeepCost = ((player != null) && !player.IsAI()) ? GameSession.CalculateUpkeepCosts(assetdb, db, playerId) : 0.0;
                if (game.AssetDatabase.GetFaction(playerInfo.FactionID).Name == "loa")
                {
                    this.CorruptionExpenses = 0.0;
                }
                else
                {
                    this.CorruptionExpenses = (((this.TradeRevenue + this.TaxRevenue) + this.IORevenue) + this.SavingsInterest) * Math.Max((float) 0f, (float) ((assetdb.BaseCorruptionRate + (0.02f * (playerInfo.RateImmigration * 10f))) - (2f * (playerInfo.RateGovernmentResearch * playerInfo.RateGovernmentSecurity))));
                }
            }

			public double GetBaseIndustrialOutputRevenue(GameSession game, GameDatabase db, PlayerInfo player)
			{
				double num = 0.0;
				List<int> list = db.GetPlayerColonySystemIDs(player.ID).ToList<int>();
				foreach (int current in list)
				{
					List<BuildOrderInfo> list2 = db.GetBuildOrdersForSystem(current).ToList<BuildOrderInfo>();
					float num2 = 0f;
					List<ColonyInfo> list3 = db.GetColonyInfosForSystem(current).ToList<ColonyInfo>();
					foreach (ColonyInfo current2 in list3)
					{
						if (current2.PlayerID == player.ID)
						{
							num2 += Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetConstructionPoints(game, current2);
						}
					}
					num2 *= game.GetStationBuildModifierForSystem(current, player.ID);
					foreach (BuildOrderInfo current3 in list2)
					{
						DesignInfo designInfo = db.GetDesignInfo(current3.DesignID);
						if (designInfo.PlayerID == player.ID)
						{
							int arg_EA_0 = designInfo.SavingsCost;
							if (designInfo.IsLoaCube())
							{
								int arg_FB_0 = current3.LoaCubes;
								int arg_107_0 = game.AssetDatabase.LoaCostPerCube;
							}
							int num3 = current3.ProductionTarget - current3.Progress;
							float num4 = 0f;
							if (!designInfo.isPrototyped)
							{
								num4 = (float)((int)(num2 * (db.GetStratModifierFloatToApply(StratModifiers.PrototypeTimeModifier, player.ID) - 1f)));
							}
							if ((float)num3 <= num2 - num4)
							{
								num2 -= (float)num3;
							}
						}
					}
					num += (double)num2;
				}
				return num;
			}
		}
		private class ImportNode
		{
			public StarSystemInfo System;
			public Dictionary<int, int> ImportCapacity;
		}
		private class ExportNode
		{
			public StarSystemInfo System;
			public StationInfo Station;
			public int ExportPoints;
			public float Range;
			public List<GameSession.ImportNode> InternationalSystems = new List<GameSession.ImportNode>();
			public List<GameSession.ImportNode> InterprovincialSystems = new List<GameSession.ImportNode>();
			public List<GameSession.ImportNode> GenericSystems = new List<GameSession.ImportNode>();
		}
		public const int IntelMissionCriticalSuccessPercent = 5;
		public const int IntelMissionSuccessPercent = 25;
		internal const float InstaBuildConstructionPoints = 1E+09f;
		private const double DebtInterestRate = 0.15;
		private const double SavingsInterestRate = 0.01;
		public static bool ForceIntelMissionCriticalSuccessHack;
		public List<TurnEvent> TurnEvents = new List<TurnEvent>();
		public readonly List<Trigger> ActiveTriggers = new List<Trigger>();
		public List<EventStub> TriggerEvents = new List<EventStub>();
		public readonly Dictionary<string, float> TriggerScalars = new Dictionary<string, float>();
		public readonly IList<Kerberos.Sots.PlayerFramework.Player> OtherPlayers = new List<Kerberos.Sots.PlayerFramework.Player>();
		public PendingCombat _currentCombat;
		public SimState State;
		private string _saveGameName;
		public OrbitCameraController StarMapCamera;
		public object StarMapSelectedObject = 0;
		protected GameObjectSet _crits;
		private Dictionary<int, int> _playerGateMap = new Dictionary<int, int>();
		private TurnTimer _turnTimer;
		private Dialog DialogCombatsPending;
		private CombatDataHelper _combatData;
		public static int _muniquecombatID;
		private readonly Dictionary<Kerberos.Sots.PlayerFramework.Player, GameSession.SimPlayerInfo> PlayerInfos = new Dictionary<Kerberos.Sots.PlayerFramework.Player, GameSession.SimPlayerInfo>();
		private Dictionary<int, double> _incomeFromTrade;
		public static bool ForceReactionHack;
		public static bool SkipCombatHack;
		internal static bool InstaBuildHackEnabled;
		internal static int SimAITurns;
		private App _app;
		private GameDatabase _db;
		private Random _random;
		private uint m_GameID;
		private readonly List<Kerberos.Sots.PlayerFramework.Player> m_Players = new List<Kerberos.Sots.PlayerFramework.Player>();
		private readonly List<Kerberos.Sots.StarSystem> m_Systems = new List<Kerberos.Sots.StarSystem>();
		private List<PendingCombat> m_Combats;
		private List<FleetInfo> fleetsInCombat;
		private List<ReactionInfo> _reactions;
		private MultiCombatCarryOverData m_MCCarryOverData;
		private OpenCloseSystemToggleData m_OCSystemToggleData;
		private readonly Dictionary<Faction, float> m_SpeciesIdealSuitability = new Dictionary<Faction, float>();
		public Kerberos.Sots.PlayerFramework.Player LocalPlayer
		{
			get;
			private set;
		}
		public NamesPool NamesPool
		{
			get;
			private set;
		}
		public bool IsMultiplayer
		{
			get;
			private set;
		}
		public ScriptModules ScriptModules
		{
			get;
			private set;
		}
		public bool HomeworldNamed
		{
			get;
			set;
		}
		public Random Random
		{
			get
			{
				return this._random;
			}
		}
		public string SaveGameName
		{
			get
			{
				return this._saveGameName;
			}
		}
		public AssetDatabase AssetDatabase
		{
			get
			{
				return this._app.AssetDatabase;
			}
		}
		public App App
		{
			get
			{
				return this._app;
			}
		}
		public UICommChannel UI
		{
			get
			{
				return this._app.UI;
			}
		}
		public TurnTimer TurnTimer
		{
			get
			{
				return this._turnTimer;
			}
		}
		public CombatDataHelper CombatData
		{
			get
			{
				return this._combatData;
			}
		}
		public GameDatabase GameDatabase
		{
			get
			{
				return this._db;
			}
		}
		public MultiCombatCarryOverData MCCarryOverData
		{
			get
			{
				return this.m_MCCarryOverData;
			}
		}
		public OpenCloseSystemToggleData OCSystemToggleData
		{
			get
			{
				return this.m_OCSystemToggleData;
			}
		}
		private void VerifyNotTreaty(DiplomacyAction action)
		{
			if (action == DiplomacyAction.TREATY)
			{
				throw new ArgumentException("TREATY is not supported here. Use other methods to gather the needed information instead.");
			}
		}
		public int? GetDiplomacyActionCost(DiplomacyAction action, RequestType? request, DemandType? demand)
		{
			switch (action)
			{
			case DiplomacyAction.DECLARATION:
			case DiplomacyAction.SURPRISEATTACK:
				return new int?((action == DiplomacyAction.DECLARATION) ? this.AssetDatabase.DeclareWarPointCost : 0);
			case DiplomacyAction.REQUEST:
				if (request.HasValue)
				{
					return new int?(this.AssetDatabase.GetDiplomaticRequestPointCost(request.Value));
				}
				return new int?(0);
			case DiplomacyAction.DEMAND:
				if (demand.HasValue)
				{
					return new int?(this.AssetDatabase.GetDiplomaticDemandPointCost(demand.Value));
				}
				return new int?(0);
			case DiplomacyAction.TREATY:
				return new int?(0);
			case DiplomacyAction.LOBBY:
				return new int?(50);
			case DiplomacyAction.SPIN:
				return null;
			default:
				return null;
			}
		}
		private int GetArmisticeStepCost(DiplomacyState lowerState, DiplomacyState upperState)
		{
			DiplomacyStateChange diplomacyStateChange = this.AssetDatabase.DiplomacyStateChangeMap.Keys.FirstOrDefault((DiplomacyStateChange x) => x.lower == lowerState && x.upper == upperState);
			if (diplomacyStateChange != null)
			{
				return this.AssetDatabase.DiplomacyStateChangeMap[diplomacyStateChange];
			}
			int num = 2147483647;
			Dictionary<DiplomacyStateChange, int> dictionary = (
				from x in this.AssetDatabase.DiplomacyStateChangeMap
				where x.Key.lower == lowerState
				select x).ToDictionary((KeyValuePair<DiplomacyStateChange, int> y) => y.Key, (KeyValuePair<DiplomacyStateChange, int> y) => y.Value);
			foreach (KeyValuePair<DiplomacyStateChange, int> current in dictionary)
			{
				num = Math.Min(num, current.Value + this.GetArmisticeStepCost(current.Key.upper, upperState));
			}
			if (num == 2147483647)
			{
				num = 0;
			}
			return num;
		}
		public int GetTreatyRdpCost(TreatyInfo ti)
		{
			if (ti.Type == TreatyType.Armistice)
			{
				DiplomacyState diplomacyStateBetweenPlayers = this.GameDatabase.GetDiplomacyStateBetweenPlayers(ti.InitiatingPlayerId, ti.ReceivingPlayerId);
				ArmisticeTreatyInfo armisticeTreatyInfo = ti as ArmisticeTreatyInfo;
				return this.GetArmisticeStepCost(diplomacyStateBetweenPlayers, armisticeTreatyInfo.SuggestedDiplomacyState);
			}
			if (ti.Type == TreatyType.Incorporate)
			{
				return this.AssetDatabase.TreatyIncorporatePointCost;
			}
			if (ti.Type == TreatyType.Protectorate)
			{
				return this.AssetDatabase.TreatyProtectoratePointCost;
			}
			if (ti.Type == TreatyType.Trade)
			{
				return this.AssetDatabase.TreatyTradePointCost;
			}
			if (ti.Type == TreatyType.Limitation)
			{
				int num = ti.Duration;
				LimitationTreatyInfo limitationTreatyInfo = ti as LimitationTreatyInfo;
				switch (limitationTreatyInfo.LimitationType)
				{
				case LimitationTreatyType.FleetSize:
					num += this.AssetDatabase.TreatyLimitationFleetsPointCost;
					break;
				case LimitationTreatyType.ShipClass:
					num += this.AssetDatabase.TreatyLimitationShipClassPointCost;
					break;
				case LimitationTreatyType.Weapon:
					num += this.AssetDatabase.TreatyLimitationWeaponsPointCost;
					break;
				case LimitationTreatyType.ResearchTree:
					num += this.AssetDatabase.TreatyLimitationResearchTreePointCost;
					break;
				case LimitationTreatyType.ResearchTech:
					num += this.AssetDatabase.TreatyLimitationResearchTechPointCost;
					break;
				case LimitationTreatyType.EmpireSize:
					num += this.AssetDatabase.TreatyLimitationColoniesPointCost;
					break;
				case LimitationTreatyType.ForgeGemWorlds:
					num += this.AssetDatabase.TreatyLimitationForgeGemWorldsPointCost;
					break;
				case LimitationTreatyType.StationType:
					num += this.AssetDatabase.TreatyLimitationStationType;
					break;
				}
				return num;
			}
			return 0;
		}
		private bool CanAffordDiplomacyAction(PlayerInfo self, DiplomacyAction action, RequestType? request, DemandType? demand)
		{
			int totalDiplomacyPoints = self.GetTotalDiplomacyPoints(self.FactionID);
			int? diplomacyActionCost = this.GetDiplomacyActionCost(action, request, demand);
			return diplomacyActionCost.HasValue && diplomacyActionCost.Value <= totalDiplomacyPoints;
		}
		private bool CanAffordTreaty(TreatyInfo treatyInfo)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(treatyInfo.InitiatingPlayerId);
			PlayerInfo playerInfo2 = this.GameDatabase.GetPlayerInfo(treatyInfo.ReceivingPlayerId);
			int treatyRdpCost = this.GetTreatyRdpCost(treatyInfo);
			int totalDiplomacyPoints = playerInfo.GetTotalDiplomacyPoints(playerInfo2.FactionID);
			return treatyRdpCost <= totalDiplomacyPoints;
		}
		public bool CanPerformTreaty(TreatyInfo treatyInfo)
		{
			return this.CanAffordTreaty(treatyInfo) && this.CanPerformDiplomacyAction(this.GameDatabase.GetPlayerInfo(treatyInfo.InitiatingPlayerId), this.GameDatabase.GetPlayerInfo(treatyInfo.ReceivingPlayerId), DiplomacyAction.TREATY, null, null);
		}
		private bool CanPerformDiplomacyRequestAction(PlayerInfo target, RequestType request)
		{
			switch (request)
			{
			case RequestType.SavingsRequest:
			case RequestType.SystemInfoRequest:
			case RequestType.ResearchPointsRequest:
			case RequestType.MilitaryAssistanceRequest:
				return true;
			case RequestType.GatePermissionRequest:
				return this.AssetDatabase.GetFaction(target.FactionID).CanUseGate();
			case RequestType.EstablishEnclaveRequest:
				return !this.AssetDatabase.GetFaction(target.FactionID).IsIndependent();
			default:
				throw new ArgumentOutOfRangeException("request");
			}
		}
		private void VerifyDiplomacyActionContext(DiplomacyAction action, RequestType? request, DemandType? demand)
		{
			switch (action)
			{
			case DiplomacyAction.DECLARATION:
			case DiplomacyAction.TREATY:
			case DiplomacyAction.LOBBY:
			case DiplomacyAction.SPIN:
			case DiplomacyAction.SURPRISEATTACK:
				if (demand.HasValue || request.HasValue)
				{
					throw new ArgumentException("This DiplomacyAction allows neither request nor demand contexts.");
				}
				break;
			case DiplomacyAction.REQUEST:
				if (demand.HasValue)
				{
					throw new ArgumentException("REQUEST action does not allow demand context.");
				}
				break;
			case DiplomacyAction.DEMAND:
				if (request.HasValue)
				{
					throw new ArgumentException("DEMAND action does not allow request context.");
				}
				break;
			default:
				throw new ArgumentOutOfRangeException("action");
			}
		}
		public bool CanPerformDiplomacyAction(PlayerInfo self, PlayerInfo target, DiplomacyAction action, RequestType? request, DemandType? demand)
		{
			this.VerifyDiplomacyActionContext(action, request, demand);
			if (self.ID == target.ID)
			{
				return false;
			}
			if (!this.CanAffordDiplomacyAction(self, action, request, demand))
			{
				return false;
			}
			if (target.isDefeated)
			{
				return false;
			}
			switch (action)
			{
			case DiplomacyAction.DECLARATION:
			case DiplomacyAction.SURPRISEATTACK:
			{
				IEnumerable<DiplomacyActionHistoryEntryInfo> source = 
					from x in this.GameDatabase.GetDiplomacyActionHistory(self.ID, target.ID, this.GameDatabase.GetTurnCount(), 1)
					where x.Action == DiplomacyAction.DECLARATION
					select x;
				return !self.IsOnTeam(target) && this.GameDatabase.GetDiplomacyStateBetweenPlayers(self.ID, target.ID) != DiplomacyState.WAR && !source.Any<DiplomacyActionHistoryEntryInfo>();
			}
			case DiplomacyAction.REQUEST:
				if (!target.isStandardPlayer)
				{
					return false;
				}
				if (request.HasValue && !this.CanPerformDiplomacyRequestAction(target, request.Value))
				{
					return false;
				}
				switch (this.GameDatabase.GetDiplomacyStateBetweenPlayers(self.ID, target.ID))
				{
				case DiplomacyState.NON_AGGRESSION:
				case DiplomacyState.ALLIED:
				case DiplomacyState.PEACE:
					return true;
				}
				return false;
			case DiplomacyAction.DEMAND:
			{
				if (!target.isStandardPlayer)
				{
					return false;
				}
				DiplomacyState diplomacyStateBetweenPlayers = this.GameDatabase.GetDiplomacyStateBetweenPlayers(self.ID, target.ID);
				if (diplomacyStateBetweenPlayers != DiplomacyState.CEASE_FIRE)
				{
					switch (diplomacyStateBetweenPlayers)
					{
					case DiplomacyState.WAR:
					case DiplomacyState.NEUTRAL:
						return true;
					}
					return false;
				}
				return true;
			}
			case DiplomacyAction.TREATY:
				return target.isStandardPlayer || this.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowProtectorate, self.ID) || this.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowIncorporate, self.ID);
			case DiplomacyAction.LOBBY:
				return this.GetPlayerObject(target.ID).IsAI();
			case DiplomacyAction.SPIN:
				return false;
			default:
				return false;
			}
		}
		public bool CanPerformLocalDiplomacyAction(PlayerInfo target, DiplomacyAction action, RequestType? request, DemandType? demand)
		{
			return this.CanPerformDiplomacyAction(this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID), target, action, request, demand);
		}
		public void DoLobbyAction(int issuingplayer, int targetplayerid, int towardsplayerID, bool improverelation)
		{
			this._app.GameDatabase.SpendDiplomacyPoints(this._app.GameDatabase.GetPlayerInfo(issuingplayer), this._app.GameDatabase.GetPlayerFactionID(targetplayerid), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.LOBBY, null, null).Value);
			float num = (float)this._app.GameDatabase.GetDiplomacyInfo(targetplayerid, issuingplayer).Relations / 2000f * 100f;
			Random random = new Random();
			int num2 = random.Next(0, 100);
			bool flag = (float)num2 < num;
			DiplomacyInfo diplomacyInfo = this._app.GameDatabase.GetDiplomacyInfo(targetplayerid, towardsplayerID);
			DiplomacyInfo diplomacyInfo2 = this._app.GameDatabase.GetDiplomacyInfo(targetplayerid, issuingplayer);
			if (flag)
			{
				int num3 = 70;
				if (towardsplayerID == issuingplayer)
				{
					num3 = 25;
				}
				if (!improverelation)
				{
					num3 *= -1;
				}
				diplomacyInfo.Relations += num3;
				diplomacyInfo.Relations = Math.Min(DiplomacyInfo.MaxDeplomacyRelations, Math.Max(DiplomacyInfo.MinDeplomacyRelations, diplomacyInfo.Relations));
				this._app.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo);
				if (towardsplayerID != issuingplayer)
				{
					diplomacyInfo2.Relations -= 10;
					diplomacyInfo2.Relations = Math.Min(DiplomacyInfo.MaxDeplomacyRelations, Math.Max(DiplomacyInfo.MinDeplomacyRelations, diplomacyInfo2.Relations));
					this._app.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo2);
				}
				if (!this.App.Game.GetPlayerObject(issuingplayer).IsAI())
				{
					this.App.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@UI_LOBBY_SUCCESS"), App.Localize("@UI_LOBBY_SUCCESS_MSG"), "dialogGenericMessage"), null);
					return;
				}
			}
			else
			{
				diplomacyInfo2.Relations -= 50;
				diplomacyInfo2.Relations = Math.Min(DiplomacyInfo.MaxDeplomacyRelations, Math.Max(DiplomacyInfo.MinDeplomacyRelations, diplomacyInfo2.Relations));
				this._app.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo2);
				if (!this.App.Game.GetPlayerObject(issuingplayer).IsAI())
				{
					this.App.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@UI_LOBBY_FAILURE"), App.Localize("@UI_LOBBY_FAILURE_MSG"), "dialogGenericMessage"), null);
				}
			}
		}
		private void ProcessTurnEventForEspionage(TurnEvent e)
		{
			foreach (IntelMissionDesc current in this.AssetDatabase.IntelMissions.ByTurnEventType(e.EventType))
			{
				current.OnProcessTurnEvent(this, e);
			}
		}
		public static float GetIntelSuccessRollChance(AssetDatabase assetdb, GameDatabase db, int player, int otherPlayer)
		{
			float num = 25f;
			num += 25f * assetdb.GetFaction(db.GetPlayerFactionID(player)).GetSpyingBonusValueForFaction(assetdb.GetFaction(db.GetPlayerFactionID(otherPlayer)));
			num += 0.2f * (db.GetPlayerInfo(otherPlayer).RateImmigration * 100f);
			num += 25f * db.GetStratModifierFloatToApply(StratModifiers.EnemyIntelSuccessModifier, otherPlayer);
			num += 25f * (db.GetStratModifierFloatToApply(StratModifiers.IntelSuccessModifier, player) - 1f);
			return Math.Min(num, 100f);
		}
		private static GameSession.IntelMissionResults RollForIntelMission(AssetDatabase assetdb, GameDatabase db, Random r, int player, int otherPlayer)
		{
			float intelSuccessRollChance = GameSession.GetIntelSuccessRollChance(assetdb, db, player, otherPlayer);
			int num = r.Next(0, 100);
			if (GameSession.ForceIntelMissionCriticalSuccessHack)
			{
				return GameSession.IntelMissionResults.CriticalSuccess;
			}
			if ((double)num <= (double)intelSuccessRollChance * 0.1)
			{
				return GameSession.IntelMissionResults.CriticalSuccess;
			}
			if ((float)num >= intelSuccessRollChance * 2f || num >= 99)
			{
				return GameSession.IntelMissionResults.CriticalFailure;
			}
			if ((float)num >= intelSuccessRollChance)
			{
				return GameSession.IntelMissionResults.Failure;
			}
			return GameSession.IntelMissionResults.Success;
		}
		public void DoIntelMission(int targetPlayer)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID);
			playerInfo.IntelPoints = Math.Max(playerInfo.IntelPoints - this.AssetDatabase.RequiredIntelPointsForMission, 0);
			this.GameDatabase.UpdatePlayerIntelPoints(this.LocalPlayer.ID, playerInfo.IntelPoints);
			switch (GameSession.RollForIntelMission(this.AssetDatabase, this.GameDatabase, this._random, this.LocalPlayer.ID, targetPlayer))
			{
			case GameSession.IntelMissionResults.CriticalSuccess:
				this.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_CRITICAL_SUCCESS,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_CRITICAL_SUCCESS,
					PlayerID = this.LocalPlayer.ID,
					TargetPlayerID = targetPlayer,
					TurnNumber = this.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				this.GameDatabase.InsertGovernmentAction(this.LocalPlayer.ID, App.Localize("@GA_INTEL"), "Intel", 0, 0);
				return;
			case GameSession.IntelMissionResults.Success:
				this.GameDatabase.InsertIntelMission(playerInfo.ID, targetPlayer, this.AssetDatabase.IntelMissions.Choose(this._random).ID, null);
				return;
			case GameSession.IntelMissionResults.Failure:
				this.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_FAILED,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_FAILED,
					PlayerID = this.LocalPlayer.ID,
					TargetPlayerID = targetPlayer,
					TurnNumber = this.GameDatabase.GetTurnCount()
				});
				return;
			case GameSession.IntelMissionResults.CriticalFailure:
				this.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_CRITICAL_FAILED,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_CRITICAL_FAILED,
					PlayerID = this.LocalPlayer.ID,
					TargetPlayerID = targetPlayer,
					TurnNumber = this.GameDatabase.GetTurnCount()
				});
				this.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_INTEL_MISSION_CRITICAL_FAILED_LEAK,
					EventMessage = TurnEventMessage.EM_INTEL_MISSION_CRITICAL_FAILED_LEAK,
					PlayerID = targetPlayer,
					TargetPlayerID = this.LocalPlayer.ID,
					TurnNumber = this.GameDatabase.GetTurnCount()
				});
				return;
			default:
				return;
			}
		}
		public void DoIntelMissionCriticalSuccess(int targetPlayerId, IEnumerable<IntelMissionDesc> selectedMissions, PlayerInfo blamePlayer)
		{
			foreach (IntelMissionDesc current in selectedMissions)
			{
				this.GameDatabase.InsertIntelMission(this.LocalPlayer.ID, targetPlayerId, current.ID, (blamePlayer != null) ? new int?(blamePlayer.ID) : null);
			}
		}
		private static GameSession.IntelMissionResults RollForCounterIntelMission(Random r)
		{
			int num = r.Next(0, 100);
			if (num <= 5)
			{
				return GameSession.IntelMissionResults.CriticalSuccess;
			}
			if (num <= 35)
			{
				return GameSession.IntelMissionResults.Success;
			}
			if (num >= 95)
			{
				return GameSession.IntelMissionResults.CriticalFailure;
			}
			return GameSession.IntelMissionResults.Failure;
		}
		public void DoCounterIntelMission(int SpyPlayer, int DefendingPlayer, IntelMissionInfo intelmission)
		{
			List<CounterIntelStingMission> list = this.GameDatabase.GetCountIntelStingsForPlayerAgainstPlayer(DefendingPlayer, SpyPlayer).ToList<CounterIntelStingMission>();
			if (!list.Any<CounterIntelStingMission>())
			{
				return;
			}
			int num = list.Count;
			int num2 = 0;
			foreach (CounterIntelStingMission current in list)
			{
				GameSession.IntelMissionResults intelMissionResults = GameSession.RollForCounterIntelMission(this._random);
				if (intelMissionResults == GameSession.IntelMissionResults.Failure && num > 1)
				{
					num--;
					num2++;
					this.GameDatabase.RemoveCounterIntelSting(current.ID);
				}
				else
				{
					switch (intelMissionResults)
					{
					case GameSession.IntelMissionResults.CriticalSuccess:
						this.GameDatabase.RemoveCounterIntelSting(current.ID);
						this.GameDatabase.InsertCounterIntelResponse(intelmission.ID, true, "");
						this.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_COUNTER_INTEL_CRITICAL_SUCCESS,
							EventMessage = TurnEventMessage.EM_COUNTER_INTEL_CRITICAL_SUCCESS,
							PlayerID = DefendingPlayer,
							TurnNumber = this.GameDatabase.GetTurnCount(),
							Param1 = intelmission.ID.ToString(),
							ShowsDialog = true
						});
						this.GameDatabase.InsertGovernmentAction(DefendingPlayer, App.Localize("@GA_COUNTERINTEL"), "CounterIntel", 0, 0);
						return;
					case GameSession.IntelMissionResults.Success:
						this.GameDatabase.RemoveCounterIntelSting(current.ID);
						this.GameDatabase.InsertCounterIntelResponse(intelmission.ID, true, "");
						this.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_COUNTER_INTEL_SUCCESS,
							EventMessage = TurnEventMessage.EM_COUNTER_INTEL_SUCCESS,
							PlayerID = DefendingPlayer,
							TurnNumber = this.GameDatabase.GetTurnCount(),
							Param1 = intelmission.ID.ToString(),
							ShowsDialog = true
						});
						this.GameDatabase.InsertGovernmentAction(DefendingPlayer, App.Localize("@GA_COUNTERINTEL"), "CounterIntel", 0, 0);
						return;
					case GameSession.IntelMissionResults.Failure:
						this.GameDatabase.RemoveCounterIntelSting(current.ID);
						this.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_COUNTER_INTEL_FAILURE,
							EventMessage = TurnEventMessage.EM_COUNTER_INTEL_FAILURE,
							PlayerID = DefendingPlayer,
							TurnNumber = this.GameDatabase.GetTurnCount(),
							Param1 = intelmission.ID.ToString(),
							ShowsDialog = true
						});
						return;
					case GameSession.IntelMissionResults.CriticalFailure:
						this.GameDatabase.RemoveCounterIntelSting(current.ID);
						this.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_COUNTER_INTEL_CRITICAL_FAILURE,
							EventMessage = TurnEventMessage.EM_COUNTER_INTEL_CRITICAL_FAILURE,
							PlayerID = DefendingPlayer,
							TurnNumber = this.GameDatabase.GetTurnCount(),
							Param1 = intelmission.ID.ToString(),
							ShowsDialog = true
						});
						return;
					}
				}
			}
		}
		public void DoOperationsMission()
		{
		}
		public void KillCombatDialog()
		{
			if (this.DialogCombatsPending != null)
			{
				this.UI.CloseDialog(this.DialogCombatsPending, true);
				this.DialogCombatsPending = null;
			}
		}
		public void ShowCombatDialog(bool visible, GameState state = null)
		{
			if (state == null)
			{
				state = this.App.CurrentState;
			}
			if (visible && (state == this.App.GetGameState<StarMapState>() || state == this.App.GetGameState<CommonCombatState>() || state == this.App.GetGameState<SimCombatState>()) && this.App.Game.GetPendingCombats().Any<PendingCombat>())
			{
				if (state == this.App.GetGameState<CommonCombatState>() || state == this.App.GetGameState<SimCombatState>())
				{
					CommonCombatState commonCombatState = (CommonCombatState)state;
					if (!commonCombatState.PlayersInCombat.Any((Kerberos.Sots.PlayerFramework.Player x) => x.ID == this.App.LocalPlayer.ID))
					{
						if (this.DialogCombatsPending != null)
						{
							this.App.UI.SetVisible(this.DialogCombatsPending.ID, true);
							return;
						}
						this.DialogCombatsPending = new DialogCombatsPending(this.App);
						this.App.UI.CreateDialog(this.DialogCombatsPending, null);
						return;
					}
					else
					{
						if (this.DialogCombatsPending != null)
						{
							this.App.UI.SetVisible(this.DialogCombatsPending.ID, false);
							this.App.UI.CloseDialog(this.DialogCombatsPending, true);
							this.DialogCombatsPending = null;
							return;
						}
					}
				}
				else
				{
					if (state == this.App.GetGameState<StarMapState>() || state == this.App.GetGameState<CommonCombatState>() || state == this.App.GetGameState<SimCombatState>())
					{
						if (this.App.UI.GetTopDialog() != null)
						{
							return;
						}
						if (this.DialogCombatsPending != null)
						{
							this.App.UI.SetVisible(this.DialogCombatsPending.ID, true);
							return;
						}
						this.DialogCombatsPending = new DialogCombatsPending(this.App);
						this.App.UI.CreateDialog(this.DialogCombatsPending, null);
						return;
					}
				}
			}
			else
			{
				if (this.DialogCombatsPending != null)
				{
					this.App.UI.SetVisible(this.DialogCombatsPending.ID, false);
					this.App.UI.CloseDialog(this.DialogCombatsPending, true);
					this.DialogCombatsPending = null;
				}
			}
		}
		public static int GetNextUniqueCombatID()
		{
			return ++GameSession._muniquecombatID;
		}
		public void SetLocalPlayer(Kerberos.Sots.PlayerFramework.Player player)
		{
			if (player != null)
			{
				App.Log.Trace(string.Concat(new object[]
				{
					"***** CHANGING LOCAL PLAYER FROM ",
					(this.LocalPlayer != null) ? this.LocalPlayer.ID : 0,
					" TO ",
					player.ID
				}), "net");
				if (this.GameDatabase != null)
				{
					this.GameDatabase.SetClientId(player.ID);
				}
				this.LocalPlayer = player;
				this.LocalPlayer.SetAI(false);
				this.App.PostSetLocalPlayer(this.LocalPlayer.ObjectID);
			}
		}
		public void CollectTurnEvents()
		{
			this.TurnEvents = this.GameDatabase.GetTurnEventsByTurnNumber(this.GameDatabase.GetTurnCount() - 1, this.LocalPlayer.ID).OrderByDescending(delegate(TurnEvent x)
			{
				if (!x.ShowsDialog)
				{
					return 0;
				}
				if (!x.IsCombatEvent)
				{
					return 1;
				}
				return 2;
			}).ToList<TurnEvent>();
			for (int i = 0; i < this.TurnEvents.Count<TurnEvent>() - 1; i++)
			{
				if (!this.TurnEvents[i].ShowsDialog && this.TurnEvents[i + 1].ShowsDialog)
				{
					TurnEvent value = this.TurnEvents[i + 1];
					this.TurnEvents[i + 1] = this.TurnEvents[i];
					this.TurnEvents[i] = value;
					i = -1;
				}
			}
			foreach (TurnEvent current in this.TurnEvents)
			{
				this.ProcessTurnEventForEspionage(current);
			}
		}
		public static string GetSpecialProjectDescription(SpecialProjectType type)
		{
			if (type == SpecialProjectType.Salvage)
			{
				return "Research Project - Completing this project will allow you to research a tech that you could not research before.";
			}
			return "";
		}
		private ShipSectionCollection GetAvailableShipSectionsCore(Kerberos.Sots.PlayerFramework.Player player)
		{
			return this.PlayerInfos[player].AvailableShipSections;
		}
		public IEnumerable<ShipSectionAsset> GetAvailableShipSections(int playerID, ShipSectionType type, ShipClass shipClass)
		{
			ShipSectionCollection availableShipSectionsCore = this.GetAvailableShipSectionsCore(this.GetPlayerObject(playerID));
			return availableShipSectionsCore.GetSectionsByType(shipClass, type);
		}
		public IEnumerable<ShipSectionAsset> GetAvailableShipSections(int playerID)
		{
			return this.GetAvailableShipSectionsCore(this.GetPlayerObject(playerID)).GetAllSections();
		}
		public double GetPlayerIncomeFromTrade(int playerID)
		{
			double result = 0.0;
			this._incomeFromTrade.TryGetValue(playerID, out result);
			return result;
		}
		public void CheckForNewSections(int player, IEnumerable<PlayerTechInfo> researchedTechs, HashSet<string> researchedGroups)
		{
			string faction = this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(player));
			List<ShipSectionAsset> list = new List<ShipSectionAsset>(this.GetAvailableShipSections(player));
			List<ShipSectionAsset> list2 = new List<ShipSectionAsset>(
				from x in this.AssetDatabase.ShipSections
				where x.Faction == faction
				select x);
			foreach (ShipSectionAsset current in list2)
			{
				if (!list.Contains(current))
				{
					bool flag = true;
					string[] requiredTechs = current.RequiredTechs;
					string t;
					for (int i = 0; i < requiredTechs.Length; i++)
					{
						t = requiredTechs[i];
						if (!researchedTechs.Any((PlayerTechInfo x) => x.TechFileID == t) && !researchedGroups.Contains(t))
						{
							flag = false;
						}
					}
					if (flag)
					{
						this.GameDatabase.InsertSectionAsset(current.FileName, player);
					}
				}
			}
		}
		private void SetupStartMapCamera()
		{
			this._crits = new GameObjectSet(this._app);
			this.StarMapCamera = this._crits.Add<OrbitCameraController>(new object[0]);
			this.StarMapCamera.MinDistance = 5f;
			this.StarMapCamera.MaxDistance = 400f;
			this.StarMapCamera.DesiredDistance = 13f;
			this.StarMapCamera.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this.StarMapCamera.DesiredPitch = -MathHelper.DegreesToRadians(25f);
			this.StarMapCamera.SnapToDesiredPosition();
		}
		public GameSession(App app, GameDatabase db, GameSetup gs, string saveGameFileName, NamesPool namesPool, IList<Trigger> activeTriggers, Random rand, GameSession.Flags flags = (GameSession.Flags)0)
		{
			GameSession.SimAITurns = 0;
			saveGameFileName = Path.GetFileName(saveGameFileName);
			if (saveGameFileName.EndsWith(".sots2save"))
			{
				saveGameFileName = saveGameFileName.Substring(0, saveGameFileName.Length - ".sots2save".Length);
			}
			if (string.IsNullOrEmpty(saveGameFileName))
			{
				throw new ArgumentNullException("saveGameFileName", "GameSession must be constructed with a valid save game filename. By default this could be gameSetup.GetDefaultSaveGameFileName().");
			}
			this._db = db;
			this._random = new Random();
			this._saveGameName = saveGameFileName;
			this._app = app;
			this.NamesPool = namesPool;
			this.IsMultiplayer = gs.IsMultiplayer;
			this._db.QueryLogging(this.IsMultiplayer);
			this._combatData = new CombatDataHelper();
			this._turnTimer = new TurnTimer(app);
			this._turnTimer.StrategicTurnLength = gs.StrategicTurnLength;
			GameSession.Trace("Creating game turn length: " + gs.StrategicTurnLength);
			this.m_Combats = new List<PendingCombat>();
			this.fleetsInCombat = new List<FleetInfo>();
			this._reactions = new List<ReactionInfo>();
			this.m_MCCarryOverData = new MultiCombatCarryOverData();
			this.m_OCSystemToggleData = new OpenCloseSystemToggleData();
			this.m_GameID = 0u;
			if ((flags & GameSession.Flags.ResumingGame) == (GameSession.Flags)0)
			{
				this._db.InsertTurnOne();
			}
			else
			{
				if ((flags & GameSession.Flags.NoScriptModules) == (GameSession.Flags)0)
				{
					this.ScriptModules = ScriptModules.Resume(this._db);
				}
			}
			foreach (PlayerInfo current in this._db.GetPlayerInfos())
			{
				this.AddPlayerObject(current, Kerberos.Sots.PlayerFramework.Player.ClientTypes.User);
			}
			IEnumerable<PlayerInfo> playerInfos = db.GetPlayerInfos();
			int num = 1;
			foreach (PlayerSetup current2 in gs.Players)
			{
				if (current2.localPlayer)
				{
					num = current2.databaseId;
					break;
				}
			}
			db.SetClientId(num);
			Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(num);
			App.Log.Trace(string.Concat(new object[]
			{
				"***** CHANGING LOCAL PLAYER FROM ",
				(this.LocalPlayer != null) ? this.LocalPlayer.ID : 0,
				" TO ",
				playerObject.ID
			}), "net");
			if (this.GameDatabase != null)
			{
				this.GameDatabase.SetClientId(playerObject.ID);
			}
			this.LocalPlayer = playerObject;
			if ((flags & GameSession.Flags.NoNewGameMessage) == (GameSession.Flags)0)
			{
				this.App.PostNewGame(this.LocalPlayer.ObjectID);
			}
			foreach (PlayerInfo current3 in playerInfos)
			{
				if (current3.ID != num)
				{
					Kerberos.Sots.PlayerFramework.Player playerObject2 = this.GetPlayerObject(current3.ID);
					playerObject2.SetAI(!this.App.GameSetup.IsMultiplayer || this.App.Network.IsHosting);
					this.OtherPlayers.Add(playerObject2);
				}
			}
			if ((flags & GameSession.Flags.ResumingGame) == (GameSession.Flags)0)
			{
				List<int> list = this._db.GetStandardPlayerIDs().ToList<int>();
				if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0 && !gs.HasScenarioFile())
				{
					GameSession.AddAdditionalColonies(app, this._db, app.AssetDatabase, list);
					foreach (int current4 in list)
					{
						string factionName = this._db.GetFactionName(this._db.GetPlayerFactionID(current4));
						IEnumerable<int> systems = this._db.GetPlayerColonySystemIDs(current4);
						HomeworldInfo playerHomeworld = this._db.GetPlayerHomeworld(current4);
						StarSystemInfo starSystemInfo2 = this._db.GetStarSystemInfo(playerHomeworld.SystemID);
						Vector3 origin = starSystemInfo2.Origin;
						int stratModifier = this._db.GetStratModifier<int>(StratModifiers.MaxProvincePlanets, current4, (int)this.AssetDatabase.DefaultStratModifiers[StratModifiers.MaxProvincePlanets]);
						int num2 = this._db.GetStratModifier<int>(StratModifiers.StartProvincePlanets, current4, (int)this.AssetDatabase.DefaultStratModifiers[StratModifiers.StartProvincePlanets]);
						if (num2 > stratModifier)
						{
							num2 = stratModifier;
						}
						string provinceName = namesPool.GetProvinceName(factionName);
						int[] systemIds = (
							from starSystemInfo in (
								from starSystemInfo in this._db.GetStarSystemInfos()
								where systems.Any((int colonySystemID) => starSystemInfo.ID == colonySystemID)
								orderby (starSystemInfo.Origin - origin).LengthSquared
								select starSystemInfo).Take(num2)
							select starSystemInfo.ID).ToArray<int>();
						db.InsertProvince(provinceName, current4, systemIds, playerHomeworld.SystemID);
					}
				}
				if ((flags & GameSession.Flags.NoScriptModules) == (GameSession.Flags)0)
				{
					this.ScriptModules = ScriptModules.New(this._random, db, app.AssetDatabase, this, namesPool, gs);
					foreach (PlayerInfo current5 in this._db.GetPlayerInfos())
					{
						if (this.GetPlayerObject(current5.ID) == null)
						{
							this.AddPlayerObject(current5, Kerberos.Sots.PlayerFramework.Player.ClientTypes.AI);
						}
					}
				}
			}
			foreach (PlayerInfo current6 in this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if ((flags & GameSession.Flags.ResumingGame) == (GameSession.Flags)0 && (flags & GameSession.Flags.NoGameSetup) == (GameSession.Flags)0)
				{
					this.GameDatabase.AcquireAdditionalInitialTechs(this.AssetDatabase, current6.ID, gs.Players[current6.ID - 1].InitialTechs);
				}
			}
			this.AvailableShipSectionsChanged();
			foreach (int current7 in this.GameDatabase.GetStandardPlayerIDs())
			{
				this.CheckForNewEquipment(current7);
			}
			this.AvailableShipSectionsChanged();
			if ((flags & GameSession.Flags.ResumingGame) == (GameSession.Flags)0 && (flags & GameSession.Flags.NoDefaultFleets) == (GameSession.Flags)0)
			{
				foreach (int current8 in this.GameDatabase.GetStandardPlayerIDs())
				{
					GameSession.AddDefaultGenerals(this.AssetDatabase, this.GameDatabase, current8, namesPool);
					this.AddDefaultStartingFleets(this.GetPlayerObject(current8));
					this.AddStartingDeployedShips(this.GameDatabase, current8);
				}
			}
			this.SetupStartMapCamera();
			this.PullTradeIncome();
			this._turnTimer.StartTurnTimer();
			if ((flags & GameSession.Flags.ResumingGame) != (GameSession.Flags)0)
			{
				IEnumerable<PlayerInfo> playerInfos2 = this.GameDatabase.GetPlayerInfos();
				foreach (PlayerInfo current9 in playerInfos2)
				{
					FactionInfo factionInfo = this.GameDatabase.GetFactionInfo(current9.FactionID);
					Faction faction = this.AssetDatabase.GetFaction(factionInfo.Name);
					if (faction.IsPlayable && current9.ID != this.LocalPlayer.ID)
					{
						Kerberos.Sots.PlayerFramework.Player playerObject3 = this.GetPlayerObject(current9.ID);
						playerObject3.SetAI(!this.App.GameSetup.IsMultiplayer || this.App.Network.IsHosting);
					}
				}
				PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID);
				this.UI.Send(new object[]
				{
					"SetLocalPlayerColor",
					playerInfo.PrimaryColor
				});
				this.UI.Send(new object[]
				{
					"SetLocalSecondaryColor",
					playerInfo.SecondaryColor
				});
				if (gs.IsMultiplayer)
				{
					this.App.Network.DatabaseLoaded();
				}
				else
				{
					gs.SetAllPlayerStatus(NPlayerStatus.PS_TURN);
				}
			}
			else
			{
				if ((flags & GameSession.Flags.NoGameSetup) == (GameSession.Flags)0)
				{
					this.ActiveTriggers.AddRange(activeTriggers);
					app.PostNewGame(this.LocalPlayer.ObjectID);
					this.AvailableShipSectionsChanged();
					foreach (PlayerInfo current10 in this.GameDatabase.GetStandardPlayerInfos())
					{
						this.CheckForNewEquipment(current10.ID);
						List<PlayerTechInfo> list2 = (
							from x in this.GameDatabase.GetPlayerTechInfos(current10.ID)
							where x.State == TechStates.Researched
							select x).ToList<PlayerTechInfo>();
						foreach (PlayerTechInfo current11 in list2)
						{
							App.UpdateStratModifiers(this, current10.ID, current11.TechID);
						}
					}
					this.AvailableShipSectionsChanged();
					List<int> list3 = this.GameDatabase.GetStandardPlayerIDs().ToList<int>();
					foreach (int current12 in list3)
					{
						List<StationInfo> list4 = this.GameDatabase.GetStationInfosByPlayerID(current12).ToList<StationInfo>();
						foreach (StationInfo current13 in list4)
						{
							this.FillUpgradeModules(current13.OrbitalObjectID);
						}
					}
					foreach (int current14 in list3)
					{
						StrategicAI.InitializeResearch(rand, this.AssetDatabase, this.GameDatabase, current14);
					}
					GameSession.UpdatePlayerFleets(this.GameDatabase, this);
					this.UpdateFleetSupply();
				}
			}
			this.UpdateProfileTechs();
			this.UpdatePlayerViews();
			if (this.App.Network != null && this.App.Network.IsHosting)
			{
				this._db.SaveMultiplayerSyncPoint(this.App.CacheDir);
			}
			if ((flags & GameSession.Flags.ResumingGame) != (GameSession.Flags)0)
			{
				GameSession.Trace("========================= RESUMING GAME SESSION AT TURN " + this._db.GetTurnCount().ToString() + " =========================");
			}
			else
			{
				GameSession.Trace("========================= NEW GAME SESSION INITIALIZED =========================");
			}
			if ((flags & GameSession.Flags.NoDefaultFleets) == (GameSession.Flags)0)
			{
				this.SetRequiredDefaultDesigns();
			}
			Kerberos.Sots.StarSystemPathing.StarSystemPathing.LoadAllNodes(this, this._db);
		}
		public void UpdateProfileTechs()
		{
			List<PlayerTechInfo> list = (
				from x in this.GameDatabase.GetPlayerTechInfos(this.LocalPlayer.ID)
				where x.State == TechStates.Researched
				select x).ToList<PlayerTechInfo>();
			foreach (PlayerTechInfo current in list)
			{
				if (!this.App.UserProfile.ResearchedTechs.Contains(current.TechFileID))
				{
					this.App.UserProfile.ResearchedTechs.Add(current.TechFileID);
				}
			}
		}
		public static void AddDefaultGenerals(AssetDatabase assetdb, GameDatabase gamedb, int playerID, NamesPool namesPool)
		{
			int playerMaxAdmirals = GameSession.GetPlayerMaxAdmirals(gamedb, playerID);
			for (int i = 0; i < playerMaxAdmirals; i++)
			{
				GameSession.GenerateNewAdmiral(assetdb, playerID, gamedb, null, namesPool);
			}
		}
		private static void AddAdditionalColonies(App game, GameDatabase gamedb, AssetDatabase assetdb, List<int> players)
		{
			List<HomeworldInfo> homeworlds = gamedb.GetHomeworlds().ToList<HomeworldInfo>();
			List<StarSystemInfo> list = gamedb.GetStarSystemInfos().ToList<StarSystemInfo>();
			list.RemoveAll((StarSystemInfo x) => homeworlds.Any((HomeworldInfo y) => y.SystemID == x.ID));
			Dictionary<int, HomeworldInfo> homeworldMap = new Dictionary<int, HomeworldInfo>();
			Dictionary<int, List<StarSystemInfo>> dictionary = new Dictionary<int, List<StarSystemInfo>>();
			foreach (int p in players)
			{
				homeworldMap.Add(p, homeworlds.First((HomeworldInfo x) => x.PlayerID == p));
				dictionary.Add(p, (
					from x in list
					orderby (x.Origin - gamedb.GetStarSystemOrigin(homeworldMap[p].SystemID)).Length
					select x).ToList<StarSystemInfo>());
			}
			Math.Max(game.GameSetup.Players.Max((PlayerSetup x) => x.InitialColonies) - 1, 0);
			foreach (int p in players)
			{
				int num = Math.Max(game.GameSetup.Players.First((PlayerSetup x) => x.databaseId == p).InitialColonies - 1, 0);
				for (int i = 0; i < num; i++)
				{
					if (game.GameSetup.Players.First((PlayerSetup x) => x.databaseId == p).InitialColonies >= i)
					{
						StarSystemInfo starSystemInfo;
						List<PlanetInfo> source;
						while (true)
						{
							starSystemInfo = dictionary[p].FirstOrDefault<StarSystemInfo>();
							if (starSystemInfo == null)
							{
								break;
							}
							source = gamedb.GetStarSystemPlanetInfos(starSystemInfo.ID).ToList<PlanetInfo>();
							foreach (int current in dictionary.Keys)
							{
								dictionary[current].Remove(starSystemInfo);
							}
							if (source.Any((PlanetInfo x) => x.Type != "gaseous" && x.Type != "barren"))
							{
								goto Block_15;
							}
						}
						return;
						Block_15:
						PlanetInfo planetInfo = source.First((PlanetInfo x) => x.Type != "gaseous" && x.Type != "barren");
						GameSession.MakeIdealColony(gamedb, assetdb, planetInfo.ID, p, IdealColonyTypes.Secondary);
						Faction faction = assetdb.GetFaction(gamedb.GetPlayerFactionID(p));
						if (faction.CanUseNodeLine(new bool?(true)) && GameSession.GetSystemPermenantNodeLine(gamedb, homeworldMap[p].SystemID, starSystemInfo.ID) == null)
						{
							gamedb.InsertNodeLine(homeworldMap[p].SystemID, starSystemInfo.ID, -1);
						}
						else
						{
							if (faction.CanUseNodeLine(new bool?(false)) && GameSession.GetSystemsNonPermenantNodeLine(gamedb, homeworldMap[p].SystemID, starSystemInfo.ID) == null)
							{
								gamedb.InsertNodeLine(homeworldMap[p].SystemID, starSystemInfo.ID, 1000);
							}
						}
						gamedb.IsSurveyed(p, homeworldMap[p].SystemID);
					}
				}
			}
		}
		public static int MakeIdealColony(GameDatabase gamedb, AssetDatabase assetdb, int planetID, int playerID, IdealColonyTypes idealColonyType)
		{
			PlanetInfo planetInfo = gamedb.GetPlanetInfo(planetID);
			if (idealColonyType == IdealColonyTypes.Primary)
			{
				planetInfo.Size = 10f;
			}
			else
			{
				planetInfo.Size = (float)App.GetSafeRandom().Next(3, 9);
			}
			if (gamedb.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerID))
			{
				planetInfo.Biosphere = 0;
			}
			double maxImperialPop = Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxImperialPop(gamedb, planetInfo);
			double num = Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxCivilianPop(gamedb, planetInfo) / 4.0;
			double d = maxImperialPop + App.GetSafeRandom().NextDouble() * maxImperialPop * (double)assetdb.PopulationNoise - maxImperialPop * (double)assetdb.PopulationNoise / 2.0;
			int num2 = gamedb.InsertColony(planetID, playerID, Math.Truncate(d), 0.5f, 1, 1f, true);
			planetInfo.Suitability = gamedb.GetFactionSuitability(gamedb.GetPlayerFactionID(playerID));
			planetInfo.Infrastructure = 1f;
			gamedb.UpdatePlanet(planetInfo);
			ColonyInfo colonyInfo = gamedb.GetColonyInfo(num2);
			colonyInfo.CurrentStage = Kerberos.Sots.Data.ColonyStage.Developed;
			Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(gamedb, assetdb, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, 0f);
			gamedb.UpdateColony(colonyInfo);
			gamedb.InsertExploreRecord(gamedb.GetOrbitalObjectInfo(planetID).StarSystemID, playerID, 1, true, true);
			double d2 = num + App.GetSafeRandom().NextDouble() * num * (double)assetdb.PopulationNoise - num * (double)assetdb.PopulationNoise / 2.0;
			gamedb.InsertColonyFaction(planetID, gamedb.GetPlayerFactionID(playerID), Math.Truncate(d2), 1f, 1);
			return num2;
		}
		public void DoModuleBuiltGovernmentShift(StationType stationType, ModuleEnums.StationModuleType moduleType, int playerId)
		{
			StationModules.StationModule stationModule = StationModules.Modules.FirstOrDefault((StationModules.StationModule x) => x.SMType == moduleType);
			string name = stationModule.Name;
			ModuleEnums.StationModuleType stationModuleType = moduleType;
			if (ModuleEnums.FactionHabitationModules.ContainsValue(stationModuleType))
			{
				if (stationModuleType == ModuleEnums.FactionHabitationModules[this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(playerId))])
				{
					stationModuleType = ModuleEnums.StationModuleType.Habitation;
				}
				else
				{
					stationModuleType = ModuleEnums.StationModuleType.AlienHabitation;
				}
			}
			else
			{
				if (ModuleEnums.FactionLargeHabitationModules.ContainsValue(stationModuleType))
				{
					if (stationModuleType == ModuleEnums.FactionLargeHabitationModules[this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(playerId))])
					{
						stationModuleType = ModuleEnums.StationModuleType.LargeHabitation;
					}
					else
					{
						stationModuleType = ModuleEnums.StationModuleType.LargeAlienHabitation;
					}
				}
			}
			switch (stationType)
			{
			case StationType.NAVAL:
				switch (stationModuleType)
				{
				case ModuleEnums.StationModuleType.Sensor:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Naval_Sensor", 0, 0);
					return;
				case ModuleEnums.StationModuleType.Customs:
				case ModuleEnums.StationModuleType.Combat:
				case ModuleEnums.StationModuleType.Warehouse:
					break;
				case ModuleEnums.StationModuleType.Repair:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Naval_Repair", 0, 0);
					return;
				case ModuleEnums.StationModuleType.Command:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Naval_Command", 0, 0);
					return;
				case ModuleEnums.StationModuleType.Dock:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Naval_Dock", 0, 0);
					return;
				default:
					return;
				}
				break;
			case StationType.SCIENCE:
			{
				ModuleEnums.StationModuleType stationModuleType2 = stationModuleType;
				if (stationModuleType2 == ModuleEnums.StationModuleType.Habitation || stationModuleType2 == ModuleEnums.StationModuleType.LargeHabitation)
				{
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_Hab", 0, 0);
					return;
				}
				switch (stationModuleType2)
				{
				case ModuleEnums.StationModuleType.EWPLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_EWPLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.TRPLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_TRPLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.NRGLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_NRGLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.WARLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_WARLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.BALLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_BALLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.BIOLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_BIOLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.INDLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_INDLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.CCCLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_CCCLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.DRVLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_DRVLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.POLLab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_POLLab", 0, 0);
					return;
				case ModuleEnums.StationModuleType.PSILab:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Sci_PSILab", 0, 0);
					break;
				default:
					return;
				}
				break;
			}
			case StationType.CIVILIAN:
			{
				ModuleEnums.StationModuleType stationModuleType3 = stationModuleType;
				switch (stationModuleType3)
				{
				case ModuleEnums.StationModuleType.Warehouse:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Civ_Warehouse", 0, 0);
					return;
				case ModuleEnums.StationModuleType.Command:
					break;
				case ModuleEnums.StationModuleType.Dock:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Civ_Dock", 0, 0);
					return;
				case ModuleEnums.StationModuleType.Terraform:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Civ_Terraform", 0, 0);
					return;
				default:
					switch (stationModuleType3)
					{
					case ModuleEnums.StationModuleType.AlienHabitation:
						goto IL_13C;
					case ModuleEnums.StationModuleType.Habitation:
						break;
					default:
						switch (stationModuleType3)
						{
						case ModuleEnums.StationModuleType.LargeHabitation:
							break;
						case ModuleEnums.StationModuleType.LargeAlienHabitation:
							goto IL_13C;
						default:
							return;
						}
						break;
					}
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Civ_Hab", 0, 0);
					return;
					IL_13C:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Civ_AlienHab", 0, 0);
					return;
				}
				break;
			}
			case StationType.DIPLOMATIC:
			{
				ModuleEnums.StationModuleType stationModuleType4 = stationModuleType;
				if (stationModuleType4 != ModuleEnums.StationModuleType.Sensor)
				{
					switch (stationModuleType4)
					{
					case ModuleEnums.StationModuleType.AlienHabitation:
						break;
					case ModuleEnums.StationModuleType.Habitation:
						goto IL_21D;
					default:
						switch (stationModuleType4)
						{
						case ModuleEnums.StationModuleType.LargeHabitation:
							goto IL_21D;
						case ModuleEnums.StationModuleType.LargeAlienHabitation:
							break;
						default:
							return;
						}
						break;
					}
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Dip_AlienHab", 0, 0);
					return;
					IL_21D:
					this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Dip_Hab", 0, 0);
					return;
				}
				this._db.InsertGovernmentAction(playerId, string.Format(App.Localize("@GA_MODULEBUILT"), name), "ModuleBuilt_Dip_Sensor", 0, 0);
				return;
			}
			default:
				return;
			}
		}
        public void FillUpgradeModules(int stationId)
        {
            Predicate<PlayerInfo> match = null;
            Predicate<PlayerInfo> predicate2 = null;
            StationInfo si = this.GameDatabase.GetStationInfo(stationId);
            if ((si.DesignInfo.StationType != StationType.MINING) && (si.DesignInfo.StationType != StationType.DEFENCE))
            {
                Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> stationUpgradeRequirements = this.AssetDatabase.GetStationUpgradeRequirements(si.DesignInfo.StationType);
                List<LogicalModuleMount> source = this.AssetDatabase.ShipSections.First<ShipSectionAsset>(x => (x.FileName == si.DesignInfo.DesignSections[0].FilePath)).Modules.ToList<LogicalModuleMount>();
                foreach (KeyValuePair<int, Dictionary<ModuleEnums.StationModuleType, int>> pair in stationUpgradeRequirements)
                {
                    if (pair.Key > si.DesignInfo.StationLevel)
                    {
                        break;
                    }
                    using (Dictionary<ModuleEnums.StationModuleType, int>.Enumerator enumerator2 = pair.Value.GetEnumerator())
                    {
                        Func<LogicalModuleMount, bool> predicate = null;
                        Func<LogicalModuleMount, bool> func2 = null;
                        KeyValuePair<ModuleEnums.StationModuleType, int> kvp;
                        while (enumerator2.MoveNext())
                        {
                            kvp = enumerator2.Current;
                            for (int i = 0; i < kvp.Value; i++)
                            {
                                if (predicate == null)
                                {
                                    predicate = x => x.ModuleType == kvp.Key.ToString();
                                }
                                if (source.Any<LogicalModuleMount>(predicate))
                                {
                                    if (func2 == null)
                                    {
                                        func2 = x => x.ModuleType == kvp.Key.ToString();
                                    }
                                    LogicalModuleMount mount = source.First<LogicalModuleMount>(func2);
                                    string factionName = this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(si.PlayerID));
                                    string moduleasset = this.AssetDatabase.GetStationModuleAsset(kvp.Key, factionName);
                                    int moduleID = this.GameDatabase.GetModuleID(moduleasset, si.PlayerID);
                                    ModuleEnums.StationModuleType key = kvp.Key;
                                    switch (key)
                                    {
                                        case ModuleEnums.StationModuleType.Habitation:
                                            key = ModuleEnums.FactionHabitationModules[this._db.GetFactionName(this._db.GetPlayerFactionID(si.PlayerID))];
                                            break;

                                        case ModuleEnums.StationModuleType.AlienHabitation:
                                            {
                                                List<PlayerInfo> list2 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
                                                if (match == null)
                                                {
                                                    match = x => x.ID == si.PlayerID;
                                                }
                                                list2.RemoveAll(match);
                                                key = ModuleEnums.FactionHabitationModules[this._db.GetFactionName(this._random.Choose<PlayerInfo>(((IList<PlayerInfo>)list2)).FactionID)];
                                                if (key == ModuleEnums.FactionHabitationModules[this._db.GetFactionName(this._db.GetPlayerFactionID(si.PlayerID))])
                                                {
                                                    key = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), key.ToString() + "Foreign");
                                                }
                                                break;
                                            }
                                    }
                                    if (key == ModuleEnums.StationModuleType.LargeHabitation)
                                    {
                                        key = ModuleEnums.FactionLargeHabitationModules[this._db.GetFactionName(this._db.GetPlayerFactionID(si.PlayerID))];
                                    }
                                    else if (key == ModuleEnums.StationModuleType.LargeAlienHabitation)
                                    {
                                        List<PlayerInfo> list3 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
                                        if (predicate2 == null)
                                        {
                                            predicate2 = x => x.ID == si.PlayerID;
                                        }
                                        list3.RemoveAll(predicate2);
                                        key = ModuleEnums.FactionLargeHabitationModules[this._db.GetFactionName(this._random.Choose<PlayerInfo>(((IList<PlayerInfo>)list3)).FactionID)];
                                        if (key == ModuleEnums.FactionLargeHabitationModules[this._db.GetFactionName(this._db.GetPlayerFactionID(si.PlayerID))])
                                        {
                                            key = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), key.ToString() + "Foreign");
                                        }
                                    }
                                    LogicalModule module = this._app.AssetDatabase.Modules.FirstOrDefault<LogicalModule>(x => x.ModulePath == moduleasset);
                                    SectionInstanceInfo info = this._db.GetShipSectionInstances(si.ShipID).First<SectionInstanceInfo>();
                                    DesignModuleInfo info2 = new DesignModuleInfo
                                    {
                                        DesignSectionInfo = si.DesignInfo.DesignSections[0],
                                        MountNodeName = mount.NodeName,
                                        ModuleID = moduleID,
                                        PsionicAbilities = new List<ModulePsionicInfo>(),
                                        StationModuleType = new ModuleEnums.StationModuleType?(key)
                                    };
                                    this.GameDatabase.InsertDesignModule(info2);
                                    this.GameDatabase.InsertModuleInstance(mount, module, info.ID);
                                    source.Remove(mount);
                                }
                            }
                        }
                    }
                }
            }
        }
        public uint GetGameID()
		{
			return this.m_GameID;
		}
		private static void Trace(string message)
		{
			App.Log.Trace(message, "game");
		}
		private static void Warn(string message)
		{
			App.Log.Warn(message, "game");
		}
		internal Kerberos.Sots.PlayerFramework.Player AddPlayerObject(PlayerInfo pi, Kerberos.Sots.PlayerFramework.Player.ClientTypes clientType)
		{
			Kerberos.Sots.PlayerFramework.Player player = new Kerberos.Sots.PlayerFramework.Player(this._app, this, pi, clientType);
			this.m_Players.Add(player);
			GameSession.Trace("Player " + this.m_Players.Count + " added.");
			return player;
		}
		public Kerberos.Sots.PlayerFramework.Player GetPlayerObject(int playerId)
		{
			return this.m_Players.FirstOrDefault((Kerberos.Sots.PlayerFramework.Player x) => x.ID == playerId);
		}
		public Kerberos.Sots.PlayerFramework.Player GetPlayerObjectByObjectID(int objectId)
		{
			return this.m_Players.FirstOrDefault((Kerberos.Sots.PlayerFramework.Player x) => x.ObjectID == objectId);
		}
		public bool AreCombatsPending()
		{
			return this.m_Combats.Count<PendingCombat>() > 0;
		}
		public bool EndTurn(int playerID)
		{
			return true;
		}
		public void AvailableShipSectionsChanged()
		{
			foreach (Kerberos.Sots.PlayerFramework.Player current in this.m_Players)
			{
				GameSession.SimPlayerInfo simPlayerInfo;
				if (!this.PlayerInfos.TryGetValue(current, out simPlayerInfo))
				{
					simPlayerInfo = new GameSession.SimPlayerInfo();
					this.PlayerInfos.Add(current, simPlayerInfo);
				}
				string[] availableSectionIds = this._db.GetGetAllPlayerSectionIds(current.ID).ToArray<string>();
				simPlayerInfo.AvailableShipSections = new ShipSectionCollection(this._db, this._app.AssetDatabase, current, availableSectionIds);
			}
		}
		public void CheckForNewEquipment(int player)
		{
			HashSet<string> researchedTechGroups = this.GameDatabase.GetResearchedTechGroups(player);
			List<PlayerTechInfo> researchedTechs = new List<PlayerTechInfo>(
				from x in this.GameDatabase.GetPlayerTechInfos(player)
				where x.State == TechStates.Researched
				select x);
			this.CheckForNewSections(player, researchedTechs, researchedTechGroups);
			this.CheckForNewModules(player, researchedTechs, researchedTechGroups);
			this.CheckForNewWeapons(player, researchedTechs, researchedTechGroups);
		}
		public void CheckForNewModules(int player, IEnumerable<PlayerTechInfo> researchedTechs, HashSet<string> researchedGroups)
		{
			string factionName = this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(player));
			List<LogicalModule> list = new List<LogicalModule>(
				from x in this.AssetDatabase.Modules
				where x.Faction == factionName
				select x);
			List<LogicalModule> list2 = new List<LogicalModule>(this.GameDatabase.GetAvailableModules(this.AssetDatabase, player));
			foreach (LogicalModule current in list)
			{
				if (!list2.Contains(current))
				{
					bool flag = true;
					foreach (Kerberos.Sots.Data.ShipFramework.Tech t in current.Techs)
					{
						if (!researchedTechs.Any((PlayerTechInfo x) => x.TechFileID == t.Name) && !researchedGroups.Contains(t.Name))
						{
							flag = false;
						}
					}
					if (flag)
					{
						this.GameDatabase.InsertModule(current, player);
					}
				}
			}
		}
		public void CheckForNewWeapons(int player, IEnumerable<PlayerTechInfo> researchedTechs, HashSet<string> researchedGroups)
		{
			string factionName = this._db.GetFactionName(this._db.GetPlayerFactionID(player));
			List<LogicalWeapon> list = new List<LogicalWeapon>(this.GameDatabase.GetAvailableWeapons(this.AssetDatabase, player));
			foreach (LogicalWeapon current in this.AssetDatabase.Weapons)
			{
				if (current.IsVisible && !list.Contains(current))
				{
					bool flag = true;
					Kerberos.Sots.Data.WeaponFramework.Tech[] requiredTechs = current.RequiredTechs;
					Kerberos.Sots.Data.WeaponFramework.Tech t;
					for (int i = 0; i < requiredTechs.Length; i++)
					{
						t = requiredTechs[i];
						if (!researchedTechs.Any((PlayerTechInfo x) => x.TechFileID == t.Name) && !researchedGroups.Contains(t.Name))
						{
							flag = false;
						}
					}
					if (flag && (current.CompatibleFactions.Count<string>() == 0 || current.CompatibleFactions.Contains(factionName)))
					{
						this.GameDatabase.InsertWeapon(current, player);
					}
				}
			}
		}
		public void ProcessEndTurn()
		{
			this.TurnTimer.StopTurnTimer();
			this.App.HotKeyManager.SetEnabled(false);
			if (this.m_Combats.Count<PendingCombat>() > 0)
			{
				return;
			}
			if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
			{
				StrategicAI.UpdateInfo updateInfo = new StrategicAI.UpdateInfo(this.GameDatabase);
				foreach (Kerberos.Sots.PlayerFramework.Player current in this.m_Players)
				{
					if (current.IsAI() && current.Faction.IsPlayable)
					{
						current.GetAI().Update(updateInfo);
					}
				}
			}
			if (GameSession.SimAITurns == 0 && this.App.GameSettings.AutoSave)
			{
				this.Autosave("(Precombat)");
			}
			this.State = SimState.SS_COMBAT;
			if (!this.App.GameSetup.IsMultiplayer)
			{
				this.ProcessMidTurn();
			}
		}
		public void ProcessMidTurn()
		{
			this.Phase2_FleetMovement();
			this.Phase3_ReactionMovement();
		}
		private void PushTradeIncome(Dictionary<int, double> tradeIncome)
		{
			IEnumerable<int> playerIDs = this._db.GetPlayerIDs();
			foreach (int current in playerIDs)
			{
				if (tradeIncome.ContainsKey(current))
				{
					this._db.UpdatePlayerCurrentTradeIncome(current, tradeIncome[current]);
				}
				else
				{
					this._db.UpdatePlayerCurrentTradeIncome(current, 0.0);
				}
			}
		}
		private void PullTradeIncome()
		{
			if (this._incomeFromTrade == null)
			{
				this._incomeFromTrade = new Dictionary<int, double>();
			}
			else
			{
				this._incomeFromTrade.Clear();
			}
			foreach (int current in this._db.GetPlayerIDs())
			{
				this._incomeFromTrade[current] = this._db.GetPlayerCurrentTradeIncome(current);
			}
		}
		public void UpdateOpenCloseSystemToggle()
		{
			foreach (OpenCloseSystemInfo current in this.m_OCSystemToggleData.ToggledSystems)
			{
				if (current.IsOpen)
				{
					GameSession.ApplyMoralEvent(this._app, MoralEvent.ME_SYSTEM_OPEN, current.PlayerID, null, null, new int?(current.SystemID));
					this._db.InsertGovernmentAction(current.PlayerID, App.Localize("@GA_DECLARESYSTEMOPEN"), "DeclareSystemOpen", 0, 0);
				}
				else
				{
					GameSession.ApplyMoralEvent(this._app, MoralEvent.ME_SYSTEM_CLOSE, current.PlayerID, null, null, new int?(current.SystemID));
					this._db.InsertGovernmentAction(current.PlayerID, App.Localize("@GA_DECLARESYSTEMCLOSED"), "DeclareSystemClosed", 0, 0);
				}
			}
		}
		public void NextTurn()
		{
			this.m_MCCarryOverData.ClearData();
			this.m_OCSystemToggleData.ClearData();
			this.App.GameDatabase.ClearTempMoveOrders();
			this.App.Game.ShowCombatDialog(false, null);
			this.App.Game.KillCombatDialog();
			this.State = SimState.SS_PLAYER;
			this.App.GetGameState<StarMapState>().TurnEnded();
			if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
			{
				this.Phase5_Results();
				this.Phase6_EndOfTurnBookKeeping();
			}
			this.PullTradeIncome();
			this.UpdatePlayerViews();
			this._db.CullQueryHistory(2);
			if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
			{
				this._db.IncrementTurnCount();
			}
			this._app.GameSetup.SavePlayerSlots(this._db);
			this.CollectTurnEvents();
			if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
			{
				List<int> list = new List<int>();
				List<int> list2 = this.GameDatabase.GetStandardPlayerIDs().ToList<int>();
				foreach (int current in list2)
				{
					List<TurnEvent> list3 = (
						from x in this.GameDatabase.GetTurnEventsByTurnNumber(this.GameDatabase.GetTurnCount() - 1, current)
						where x.EventType == TurnEventType.EV_SUULKA_LEAVES
						select x).ToList<TurnEvent>();
					foreach (TurnEvent current2 in list3)
					{
						int num = (!string.IsNullOrEmpty(current2.Param1)) ? int.Parse(current2.Param1) : 0;
						if (num != 0 && !list.Contains(num))
						{
							list.Add(num);
						}
					}
				}
				foreach (int current3 in list)
				{
					this._app.GameDatabase.ReturnSuulka(this, current3);
				}
			}
			if (this.App.CurrentState == this.App.GetGameState<StarMapState>())
			{
				this.App.GetGameState<StarMapState>().TurnStarted();
			}
			this._turnTimer.StartTurnTimer();
			this.App.PostRequestSpeech(string.Format("Newturn_{0}", this.App.LocalPlayer.Faction.Name), 50, 120, 0f);
			this.Autosave(App.Localize("@AUTOSAVE_SUFFIX"));
			if (this.App.Network != null && this.App.Network.IsHosting)
			{
				this._db.SaveMultiplayerSyncPoint(this.App.CacheDir);
			}
			this.SetRequiredDefaultDesigns();
			this.App.HotKeyManager.SetEnabled(true);
            Kerberos.Sots.StarSystemPathing.StarSystemPathing.LoadAllNodes(this, this._db);
			GameSession.Trace("========================= TURN " + this._db.GetTurnCount().ToString() + " =========================");
		}
		public void Autosave(string suffix)
		{
			string text = Path.GetFileNameWithoutExtension(this._saveGameName);
			if (!text.EndsWith(suffix))
			{
				text = text + " " + suffix;
			}
			string filename = Path.Combine(this.App.SaveDir, text + ".sots2save");
			this.Save(filename);
		}
		public void Save(string filename)
		{
			this._db.Save(filename);
			if (ScriptHost.AllowConsole)
			{
				string directoryName = Path.GetDirectoryName(filename);
				string fileName = Path.GetFileName(filename);
				this._db.Save(Path.Combine(directoryName, string.Format("T{0}P{1}-{2}.db", this._db.GetTurnCount(), this.LocalPlayer.ID, fileName)));
			}
			if (this._app.UserProfile != null)
			{
				this._app.UserProfile.LastGamePlayed = filename;
				this._app.UserProfile.SaveProfile();
			}
		}
		private void UpdatePlayerViews()
		{
			List<StarSystemInfo> list = this._db.GetStarSystemInfos().ToList<StarSystemInfo>();
			Dictionary<int, List<ShipInfo>> dictionary = new Dictionary<int, List<ShipInfo>>();
			foreach (ShipInfo current in this._db.GetShipInfos(false))
			{
				List<ShipInfo> list2;
				if (!dictionary.TryGetValue(current.FleetID, out list2))
				{
					list2 = new List<ShipInfo>();
					dictionary[current.FleetID] = list2;
				}
				list2.Add(current);
			}
			for (int i = 0; i < this.m_Players.Count; i++)
			{
				Kerberos.Sots.PlayerFramework.Player player = this.m_Players[i];
				this._db.PurgeOwnedColonyIntel(player.ID);
				foreach (StarSystemInfo current2 in list)
				{
					if (StarMap.IsInRange(this.GameDatabase, player.ID, current2, dictionary))
					{
						this._db.UpdatePlayerViewWithStarSystem(player.ID, current2.ID);
					}
				}
			}
			IEnumerable<int> standardPlayerIDs = this._db.GetStandardPlayerIDs();
			foreach (int current3 in standardPlayerIDs)
			{
				foreach (int current4 in standardPlayerIDs)
				{
					if (current3 != current4 && this._db.GetDiplomacyStateBetweenPlayers(current3, current4) == DiplomacyState.ALLIED)
					{
						this._db.ShareSensorData(current3, current4);
					}
				}
			}
		}
		private void UpdateIsEncounteredStates()
		{
			for (int i = 0; i < this.m_Players.Count; i++)
			{
				PlayerInfo playerInfo = this._db.GetPlayerInfo(this.m_Players[i].ID);
				if (playerInfo.isStandardPlayer || playerInfo.includeInDiplomacy)
				{
					Kerberos.Sots.PlayerFramework.Player player = this.m_Players[i];
					for (int j = 0; j < this.m_Players.Count; j++)
					{
						if (i != j)
						{
							PlayerInfo playerInfo2 = this._db.GetPlayerInfo(this.m_Players[j].ID);
							if (playerInfo2.isStandardPlayer || playerInfo2.includeInDiplomacy)
							{
								Kerberos.Sots.PlayerFramework.Player player2 = this.m_Players[j];
								DiplomacyInfo diplomacyInfo = this.GameDatabase.GetDiplomacyInfo(player.ID, player2.ID);
								if (diplomacyInfo != null && !diplomacyInfo.isEncountered)
								{
									bool flag = false;
									if (!flag)
									{
										List<int> list = this._db.GetPlayerColonySystemIDs(player2.ID).ToList<int>();
										foreach (int current in list)
										{
											StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(current);
											if (StarMap.IsInRange(this.GameDatabase, player.ID, starSystemInfo, null))
											{
												flag = true;
												break;
											}
										}
									}
									if (!flag)
									{
										List<FleetInfo> list2 = this._db.GetFleetInfosByPlayerID(player2.ID, FleetType.FL_NORMAL | FleetType.FL_DEFENSE | FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
										foreach (FleetInfo current2 in list2)
										{
											FleetLocation fleetLocation = this._db.GetFleetLocation(current2.ID, false);
											if (fleetLocation != null && StarMap.IsInRange(this.GameDatabase, player.ID, fleetLocation.Coords, 1f, null))
											{
												flag = true;
												break;
											}
										}
									}
									if (flag && diplomacyInfo != null)
									{
										diplomacyInfo.isEncountered = true;
										this.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo);
										this._app.GameDatabase.InsertTurnEvent(new TurnEvent
										{
											EventType = TurnEventType.EV_EMPIRE_ENCOUNTERED,
											EventMessage = TurnEventMessage.EM_EMPIRE_ENCOUNTERED,
											TurnNumber = this._db.GetTurnCount(),
											PlayerID = diplomacyInfo.PlayerID,
											TargetPlayerID = diplomacyInfo.TowardsPlayerID
										});
									}
								}
							}
						}
					}
				}
			}
		}
		public void CheckGovernmentTradeChanges(TradeResultsTable trt)
		{
			TradeResultsTable tradeResultsTable = this._db.GetTradeResultsTable();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (KeyValuePair<int, TradeNode> current in tradeResultsTable.TradeNodes)
			{
				int? systemOwningPlayer = this._db.GetSystemOwningPlayer(current.Key);
				if (systemOwningPlayer.HasValue)
				{
					if (dictionary.ContainsKey(systemOwningPlayer.Value))
					{
						Dictionary<int, int> dictionary2;
						int value;
						(dictionary2 = dictionary)[value = systemOwningPlayer.Value] = dictionary2[value] + current.Value.Produced;
					}
					else
					{
						dictionary.Add(systemOwningPlayer.Value, current.Value.Produced);
					}
				}
			}
			Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
			foreach (KeyValuePair<int, TradeNode> current2 in trt.TradeNodes)
			{
				int? systemOwningPlayer2 = this._db.GetSystemOwningPlayer(current2.Key);
				if (systemOwningPlayer2.HasValue)
				{
					if (dictionary3.ContainsKey(systemOwningPlayer2.Value))
					{
						Dictionary<int, int> dictionary4;
						int value2;
						(dictionary4 = dictionary3)[value2 = systemOwningPlayer2.Value] = dictionary4[value2] + current2.Value.Produced;
					}
					else
					{
						dictionary3.Add(systemOwningPlayer2.Value, current2.Value.Produced);
					}
				}
			}
			foreach (KeyValuePair<int, int> current3 in dictionary3)
			{
				int num = 0;
				if (dictionary.ContainsKey(current3.Key))
				{
					num = dictionary[current3.Key];
					dictionary.Remove(current3.Key);
				}
				if (num > current3.Value)
				{
					this._app.GameDatabase.InsertGovernmentAction(current3.Key, App.Localize("@GA_TRADEDECREASED"), "", 0, 2 * (current3.Value - num));
				}
				else
				{
					if (num < current3.Value)
					{
						this._app.GameDatabase.InsertGovernmentAction(current3.Key, App.Localize("@GA_TRADEINCREASED"), "", current3.Value - num, -(current3.Value - num));
					}
				}
			}
			foreach (KeyValuePair<int, int> current4 in dictionary)
			{
				this._app.GameDatabase.InsertGovernmentAction(current4.Key, App.Localize("@GA_TRADEDECREASED"), "", 0, 2 * current4.Value);
			}
		}
		private void DoAutoOptionActions()
		{
			foreach (PlayerInfo playerInfo in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (playerInfo.isStandardPlayer && !this.GetPlayerObject(playerInfo.ID).IsAI())
				{
					foreach (StarSystemInfo current in this._db.GetStarSystemInfos())
					{
						if (playerInfo.AutoPlaceDefenseAssets)
						{
							if (!this._db.GetColonyInfosForSystem(current.ID).Any((ColonyInfo x) => x.PlayerID == playerInfo.ID) && this._db.GetNavalStationForSystemAndPlayer(current.ID, playerInfo.ID) == null)
							{
								continue;
							}
							int systemDefensePoints = this._db.GetSystemDefensePoints(current.ID, playerInfo.ID);
							int allocatedSystemDefensePoints = this._db.GetAllocatedSystemDefensePoints(current, playerInfo.ID);
							int num = systemDefensePoints - allocatedSystemDefensePoints;
							if (num <= 0)
							{
								continue;
							}
							FleetInfo defenseFleetInfo = this._db.GetDefenseFleetInfo(current.ID, playerInfo.ID);
							if (defenseFleetInfo == null)
							{
								continue;
							}
							int num2 = num;
							foreach (ShipInfo current2 in (
								from x in this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false)
								where !x.IsPlaced()
								select x).ToList<ShipInfo>())
							{
								int defenseAssetCPCost = this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(current2.DesignInfo);
								if (defenseAssetCPCost <= num2 && Kerberos.Sots.StarFleet.StarFleet.AutoPlaceDefenseAsset(this._app, current2.ID, current.ID))
								{
									num2 -= defenseAssetCPCost;
								}
							}
						}
						if (playerInfo.AutoRepairShips)
						{
							this.RepairFleetsAtSystem(current.ID, playerInfo.ID);
						}
					}
				}
			}
		}
		public void Phase6_EndOfTurnBookKeeping()
		{
			List<MissionInfo> list = this._db.GetMissionInfos().ToList<MissionInfo>();
			foreach (MissionInfo current in list)
			{
				List<WaypointInfo> list2 = (
					from x in this._db.GetWaypointsByMissionID(current.ID)
					where x.Type == WaypointType.Intercepted
					select x).ToList<WaypointInfo>();
				foreach (WaypointInfo current2 in list2)
				{
					this._db.RemoveWaypoint(current2.ID);
				}
			}
			this.UpdateIsEncounteredStates();
			this.UpdatePlayerEmpireHistoryData();
			this._db.RemoveAllMoraleEventsHistory();
			int turnCount = this._db.GetTurnCount();
			foreach (PlayerInfo current3 in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(current3.ID);
				IEnumerable<PlayerTechInfo> playerTechInfos = this._db.GetPlayerTechInfos(playerObject.ID);
				playerObject._techPointsAtStartOfTurn = 0;
				foreach (PlayerTechInfo current4 in playerTechInfos)
				{
					playerObject._techPointsAtStartOfTurn += current4.Progress;
				}
				this.ValidateHomeWorld(current3.ID);
				this.HandleIntelMissionsForPlayer(current3.ID);
				this.HandleCounterIntelMissions(current3.ID);
				foreach (StarSystemInfo current5 in this._db.GetStarSystemInfos())
				{
					this.DetectPiracyFleets(current5.ID, current3.ID);
				}
			}
			TradeResultsTable trt;
			this._incomeFromTrade = this.Trade(out trt);
			this.CheckGovernmentTradeChanges(trt);
			this.PushTradeIncome(this._incomeFromTrade);
			this._db.SyncTradeNodes(trt);
			this.CollectTaxes();
			this.App.GameDatabase.RemoveAllColonyHistory();
			foreach (ColonyInfo current6 in this._db.GetColonyInfos().ToList<ColonyInfo>())
			{
				ColonyInfo colonyInfo = current6;
				OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
				this._db.InsertExploreRecord(orbitalObjectInfo.StarSystemID, colonyInfo.PlayerID, turnCount, true, true);
				List<PlagueInfo> list3 = this._db.GetPlagueInfoByColony(current6.ID).ToList<PlagueInfo>();
				bool flag = false;
				PlanetInfo planetInfo = this._db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
				int num = (int)Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetColonySupportCost(this.App.AssetDatabase, this._db, colonyInfo);
				Faction faction = this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID));
				ColonyHistoryData colonyHistoryData = new ColonyHistoryData
				{
					colonyID = current6.ID,
					resources = planetInfo.Resources,
					biosphere = planetInfo.Biosphere,
					infrastructure = planetInfo.Infrastructure,
					hazard = this.App.GameDatabase.GetPlanetHazardRating(current6.PlayerID, planetInfo.ID, false),
					life_support_cost = num,
					income = (int)Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetTaxRevenue(this.App, colonyInfo) - num,
					econ_rating = colonyInfo.EconomyRating,
					industrial_output = (int)Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetIndustrialOutput(this, colonyInfo, planetInfo),
					civ_pop = this.App.GameDatabase.GetCivilianPopulation(colonyInfo.OrbitalObjectID, faction.ID, faction.HasSlaves()),
					imp_pop = colonyInfo.ImperialPop,
					slave_pop = this.App.GameDatabase.GetSlavePopulation(colonyInfo.OrbitalObjectID, faction.ID)
				};
				colonyHistoryData.civ_pop -= colonyHistoryData.slave_pop;
				this.App.GameDatabase.InsertColonyHistory(colonyHistoryData);
				List<ColonyFactionInfo> list4 = this.App.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID).ToList<ColonyFactionInfo>();
				foreach (ColonyFactionInfo current7 in list4)
				{
					this.App.GameDatabase.RemoveColonyMoraleHistory(colonyInfo.ID);
					ColonyFactionMoraleHistory history = new ColonyFactionMoraleHistory
					{
						colonyID = colonyInfo.ID,
						factionid = current7.FactionID,
						morale = current7.Morale,
						population = current7.CivilianPop
					};
					this.App.GameDatabase.InsertColonyMoraleHistory(history);
				}
				int resources = planetInfo.Resources;
				double num2 = 0.0;
				int factionId = this._db.GetPlayerFactionID(current6.PlayerID);
				Faction faction2 = this.AssetDatabase.GetFaction(factionId);
				if (faction2.HasSlaves())
				{
					ColonyFactionInfo[] factions = colonyInfo.Factions;
					for (int i = 0; i < factions.Length; i++)
					{
						ColonyFactionInfo colonyFactionInfo = factions[i];
						if (colonyFactionInfo.FactionID != factionId)
						{
							num2 += colonyFactionInfo.CivilianPop;
						}
					}
				}
				double totalPopulation = this._db.GetTotalPopulation(colonyInfo);
				List<ColonyFactionInfo> list5;
				Kerberos.Sots.Strategy.InhabitedPlanet.Colony.MaintainColony(this, ref colonyInfo, ref planetInfo, ref list3, 0.0, 0.0, null, out list5, out flag, false);
				double num3 = this._db.GetTotalPopulation(colonyInfo) - totalPopulation;
				if (num3 < -50000.0)
				{
					colonyInfo.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.PopulationReductionOver50K, 1);
				}
				else
				{
					if (num3 > 30000.0)
					{
						colonyInfo.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.PopulationGrowthOver30K, 1);
					}
				}
				if (planetInfo.Resources == 0 && resources != 0)
				{
					this.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_PLANET_NO_RESOURCES,
						EventMessage = TurnEventMessage.EM_PLANET_NO_RESOURCES,
						PlayerID = colonyInfo.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = orbitalObjectInfo.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				double num4 = list5.Sum((ColonyFactionInfo x) => x.CivilianPop);
				double num5 = (double)colonyInfo.CivilianWeight * Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxCivilianPop(this._db, planetInfo);
				if (colonyInfo.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld)
				{
					num5 *= (double)this.AssetDatabase.GemWorldCivMaxBonus;
				}
				if (num5 < num4)
				{
					if (num5 + 100.0 < num4)
					{
						if (colonyInfo.CivilianWeight == 1f)
						{
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_OVERPOPULATION_PLANET, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, null);
						}
						else
						{
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_OVERPOPULATION_PLAYER, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, null);
						}
					}
					double num6 = num5 / num4;
					foreach (ColonyFactionInfo current8 in list5)
					{
						current8.CivilianPop *= num6;
						this._db.UpdateCivilianPopulation(current8);
					}
				}
				if (colonyInfo.ReplicantsOn)
				{
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_REPLICANTS_ON, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, null);
				}
				if (colonyInfo.InfraRate == 0f)
				{
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this._db, this.AssetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Infra, 0f);
				}
				if (colonyInfo.OverdevelopRate == 0f)
				{
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this._db, this.AssetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.OverDev, 0f);
				}
				if (colonyInfo.TerraRate == 0f)
				{
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this._db, this.AssetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Terra, 0f);
				}
				if (colonyInfo.OverharvestRate > 0f)
				{
					switch (colonyInfo.CurrentStage)
					{
					case Kerberos.Sots.Data.ColonyStage.Colony:
						this._app.GameDatabase.InsertGovernmentAction(colonyInfo.PlayerID, App.Localize("@GA_UNDERDEVELOPEDCOLONY_OVERHARVEST"), "UnderDevelopedColony_OverHarvest", 0, 0);
						break;
					case Kerberos.Sots.Data.ColonyStage.Developed:
						this._app.GameDatabase.InsertGovernmentAction(colonyInfo.PlayerID, App.Localize("@GA_DEVELOPEDCOLONY_OVERHARVEST"), "DevelopedColony_OverHarvest", 0, 0);
						break;
					case Kerberos.Sots.Data.ColonyStage.GemWorld:
					case Kerberos.Sots.Data.ColonyStage.ForgeWorld:
						this._app.GameDatabase.InsertGovernmentAction(colonyInfo.PlayerID, App.Localize("@GA_OVERDEVELOPEDCOLONY_OVERHARVEST"), "OverDevelopedColony_OverHarvest", 0, 0);
						break;
					}
				}
				if (colonyInfo.ImperialPop <= 0.0)
				{
					this._db.RemoveColonyOnPlanet(planetInfo.ID);
				}
				else
				{
					this._db.UpdateColony(colonyInfo);
				}
				UISliderNotchInfo sliderNotchSettingInfoForColony = this.App.GameDatabase.GetSliderNotchSettingInfoForColony(colonyInfo.PlayerID, colonyInfo.ID, UISlidertype.TradeSlider);
				if (sliderNotchSettingInfoForColony != null)
				{
					colonyInfo = this.App.GameDatabase.GetColonyInfo(colonyInfo.ID);
					List<double> tradeRatesForWholeExportsForColony = this.App.Game.GetTradeRatesForWholeExportsForColony(colonyInfo.ID);
					if (tradeRatesForWholeExportsForColony.Count - 1 >= (int)sliderNotchSettingInfoForColony.SliderValue)
					{
						double num7 = tradeRatesForWholeExportsForColony[(int)sliderNotchSettingInfoForColony.SliderValue];
						num7 = (double)((int)Math.Ceiling(num7 * 100.0));
						num7 /= 100.0;
						Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this.App.GameDatabase, this.App.AssetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, (float)num7);
						this.App.GameDatabase.UpdateColony(colonyInfo);
					}
					else
					{
						if (tradeRatesForWholeExportsForColony.Count != 0)
						{
							double num8 = tradeRatesForWholeExportsForColony.Last<double>();
							num8 = (double)((int)Math.Ceiling(num8 * 100.0));
							num8 /= 100.0;
							Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this.App.GameDatabase, this.App.AssetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, (float)num8);
							this.App.GameDatabase.UpdateColony(colonyInfo);
						}
					}
				}
				foreach (PlagueInfo current9 in list3)
				{
					bool flag2 = current9.InfectedPopulationCivilian <= 0.0 && current9.InfectedPopulationImperial <= 0.0;
					if (!flag2 && current9.PlagueType == WeaponEnums.PlagueType.ASSIM)
					{
						ColonyInfo colonyInfo2 = this._db.GetColonyInfo(current9.ColonyId);
						if (colonyInfo2 != null && colonyInfo2.PlayerID == current9.LaunchingPlayer)
						{
							flag2 = true;
						}
					}
					if (flag2)
					{
						this._db.RemovePlagueInfo(current9.ID);
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_PLAGUE_ENDED,
							EventMessage = TurnEventMessage.EM_PLAGUE_ENDED,
							PlagueType = current9.PlagueType,
							ColonyID = current9.ColonyId,
							PlayerID = colonyInfo.PlayerID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					else
					{
						this._db.UpdatePlagueInfo(current9);
					}
				}
				this._db.UpdatePlanet(planetInfo);
				if (flag)
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_SUPERWORLD_COMPLETE,
						EventMessage = TurnEventMessage.EM_SUPERWORLD_COMPLETE,
						PlayerID = colonyInfo.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = orbitalObjectInfo.ID,
						ColonyID = colonyInfo.ID,
						ShowsDialog = true,
						TurnNumber = this.App.GameDatabase.GetTurnCount()
					});
				}
				List<ColonyFactionInfo> list6 = this._db.GetCivilianPopulations(planetInfo.ID).ToList<ColonyFactionInfo>();
				foreach (ColonyFactionInfo cfi in list6)
				{
					if (!list5.Any((ColonyFactionInfo x) => x.FactionID == cfi.FactionID))
					{
						this._db.RemoveCivilianPopulation(cfi.OrbitalObjectID, cfi.FactionID);
					}
				}
				foreach (ColonyFactionInfo cfi in list5)
				{
					if (list6.Any((ColonyFactionInfo x) => x.FactionID == cfi.FactionID))
					{
						this._db.UpdateCivilianPopulation(cfi);
					}
					else
					{
						this._db.InsertColonyFaction(cfi.OrbitalObjectID, cfi.FactionID, cfi.CivilianPop, cfi.CivPopWeight, cfi.TurnEstablished);
					}
				}
				factionId = this._db.GetPlayerFactionID(current6.PlayerID);
				if (this.AssetDatabase.GetFaction(factionId).HasSlaves())
				{
					list5.RemoveAll((ColonyFactionInfo x) => x.FactionID == factionId);
					if (list5.Sum((ColonyFactionInfo x) => x.CivilianPop) == 0.0 && num2 > 0.0)
					{
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_SLAVES_DEAD,
							EventMessage = TurnEventMessage.EM_SLAVES_DEAD,
							PlayerID = current6.PlayerID,
							SystemID = current6.CachedStarSystemID,
							ColonyID = current6.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
					}
				}
			}
			this.CollectDiplomacyPoints();
			this.UpdateMorale();
			this.AvailableShipSectionsChanged();
			foreach (PlayerInfo current10 in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				this.CheckForNewEquipment(current10.ID);
			}
			this.AvailableShipSectionsChanged();
			this.UpdateConsumableShipStats();
			this.UpdateRepairPoints();
			this.HandleGives();
			foreach (PlayerInfo current11 in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (this.AssetDatabase.GetFaction(current11.FactionID).Name == "loa" && current11.Savings < 0.0)
				{
					current11.SetResearchRate(0f);
					this.GameDatabase.UpdatePlayerSliders(this, current11);
				}
				else
				{
					foreach (StationInfo current12 in this._db.GetStationInfosByPlayerID(current11.ID))
					{
						foreach (DesignModuleInfo current13 in current12.DesignInfo.DesignSections[0].Modules)
						{
							if (this.GameDatabase.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, current11.ID) && current13.StationModuleType == ModuleEnums.StationModuleType.Terraform)
							{
								float global = this.AssetDatabase.GetGlobal<float>("SterileModuleBiosphereBonus");
								PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(current12.OrbitalObjectInfo.StarSystemID);
								for (int i = 0; i < starSystemPlanetInfos.Length; i++)
								{
									PlanetInfo planetInfo2 = starSystemPlanetInfos[i];
									if (planetInfo2.Biosphere != 0)
									{
										planetInfo2.Biosphere -= (int)global;
										this._db.UpdatePlanet(planetInfo2);
									}
								}
							}
						}
						List<DesignModuleInfo> list7 = this._db.GetQueuedStationModules(current12.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
						if (list7.Count > 0)
						{
							StationModuleQueue.UpdateStationMapsForFaction(this.GetPlayerObject(current11.ID).Faction.Name);
							bool flag3 = this.StationIsUpgradable(current12);
							List<LogicalModuleMount> source = current12.DesignInfo.DesignSections[0].ShipSectionAsset.Modules.ToList<LogicalModuleMount>();
							DesignModuleInfo dmi = list7.First<DesignModuleInfo>();
							LogicalModuleMount logicalModuleMount = source.FirstOrDefault((LogicalModuleMount x) => x.NodeName == dmi.MountNodeName);
							while (logicalModuleMount == null && list7.Count > 1)
							{
								list7.RemoveAt(0);
								this._db.RemoveQueuedStationModule(dmi.ID);
								logicalModuleMount = source.FirstOrDefault((LogicalModuleMount x) => x.NodeName == dmi.MountNodeName);
								dmi = list7.First<DesignModuleInfo>();
							}
							if (logicalModuleMount == null)
							{
								this._db.RemoveQueuedStationModule(dmi.ID);
							}
							else
							{
								dmi.DesignSectionInfo = current12.DesignInfo.DesignSections[0];
								LogicalModule logicalModule = this.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == this._db.GetModuleAsset(dmi.ModuleID));
								SectionInstanceInfo sectionInstanceInfo = this._app.GameDatabase.GetShipSectionInstances(current12.ShipID).First<SectionInstanceInfo>();
								this.DoModuleBuiltGovernmentShift(current12.DesignInfo.StationType, dmi.StationModuleType.Value, current12.PlayerID);
								current11.Savings -= (double)logicalModule.SavingsCost;
								this._db.UpdatePlayerSavings(current11.ID, current11.Savings);
								this._db.InsertDesignModule(dmi);
								this._db.InsertModuleInstance(logicalModuleMount, logicalModule, sectionInstanceInfo.ID);
								this._db.RemoveQueuedStationModule(dmi.ID);
								if (!flag3 && this.StationIsUpgradable(current12))
								{
									this._db.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_STATION_UPGRADABLE,
										EventMessage = TurnEventMessage.EM_STATION_UPGRADABLE,
										PlayerID = current12.PlayerID,
										OrbitalID = current12.OrbitalObjectID,
										SystemID = this.App.GameDatabase.GetOrbitalObjectInfo(current12.OrbitalObjectID).StarSystemID,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								}
							}
						}
					}
					IEnumerable<int> enumerable = this._db.GetPlayerColonySystemIDs(current11.ID).ToList<int>();
					foreach (int current14 in enumerable)
					{
						this.BuildAtSystem(current14, current11.ID);
						this.RetrofitAtSystem(current14, current11.ID);
					}
				}
			}
			List<StationRetrofitOrderInfo> list8 = this.App.GameDatabase.GetStationRetrofitOrders().ToList<StationRetrofitOrderInfo>();
			foreach (StationRetrofitOrderInfo current15 in list8)
			{
				ShipInfo ship = this.App.GameDatabase.GetShipInfo(current15.ShipID, true);
				DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(current15.DesignID);
				PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(designInfo.PlayerID);
				if (!(this.AssetDatabase.GetFaction(playerInfo.FactionID).Name == "loa") || playerInfo.Savings >= 0.0)
				{
					StationInfo stationInfo = this.App.GameDatabase.GetStationInfosByPlayerID(designInfo.PlayerID).FirstOrDefault((StationInfo x) => x.ShipID == ship.ID);
					if (ship != null && designInfo != null && stationInfo != null && stationInfo.DesignInfo != null && ship.DesignInfo != null && ship.DesignInfo.StationLevel == designInfo.StationLevel && stationInfo.DesignInfo.StationLevel == designInfo.StationLevel && stationInfo.DesignInfo.DesignSections[0].Modules.Count == designInfo.DesignSections[0].Modules.Count)
					{
                        int num9 = Kerberos.Sots.StarFleet.StarFleet.CalculateStationRetrofitCost(this.App, ship.DesignInfo, designInfo);
						PlayerInfo playerInfo2 = this.App.GameDatabase.GetPlayerInfo(designInfo.PlayerID);
						playerInfo2.Savings -= (double)num9;
						this.App.GameDatabase.UpdatePlayerSavings(playerInfo2.ID, playerInfo2.Savings);
						this.App.GameDatabase.UpdateShipDesign(ship.ID, designInfo.ID, null);
						stationInfo.DesignInfo = designInfo;
						this.App.GameDatabase.UpdateStation(stationInfo);
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_RETROFIT_COMPLETE_STATION,
							EventMessage = TurnEventMessage.EM_RETROFIT_COMPLETE_STATION,
							PlayerID = stationInfo.PlayerID,
							ShipID = stationInfo.ShipID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					this.App.GameDatabase.RemoveStationRetrofitOrder(current15.ID);
				}
			}
			this.CheckAdmiralRetirement();
			foreach (PlayerInfo current16 in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (this.GetNumAdmirals(current16.ID) < GameSession.GetPlayerMaxAdmirals(this._db, current16.ID))
				{
					List<ColonyInfo> list9 = this._db.GetPlayerColoniesByPlayerId(current16.ID).ToList<ColonyInfo>();
					Random random = new Random();
					foreach (ColonyInfo arg_176D_0 in list9)
					{
						if (random.CoinToss(20))
						{
							int admiralID = GameSession.GenerateNewAdmiral(this.AssetDatabase, current16.ID, this.GameDatabase, null, this.NamesPool);
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_ADMIRAL_PROMOTED,
								EventMessage = TurnEventMessage.EM_ADMIRAL_PROMOTED,
								PlayerID = current16.ID,
								TurnNumber = this.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false,
								AdmiralID = admiralID
							});
							GameSession.Trace("New Admiral generated for player " + current16.ID);
							break;
						}
					}
				}
			}
			GameSession.UpdatePlayerFleets(this.GameDatabase, this);
			this.UpdateTreaties(this.GameDatabase);
			this.UpdateRequests(this.GameDatabase);
			this.UpdateDemands(this.GameDatabase);
			this.CheckAIRebellions(this.GameDatabase);
			this.CheckTriggers(TurnStage.STG_STAGE1);
			this.AvailableShipSectionsChanged();
			IEnumerable<CombatData> combats = this._combatData.GetCombats(this.App.GameDatabase.GetTurnCount());
			foreach (PlayerInfo player in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				GovernmentInfo governmentInfo = this._db.GetGovernmentInfo(player.ID);
				this._db.UpdateGovernmentInfo(governmentInfo);
				if (this._db.GetGovernmentInfo(player.ID).CurrentType != governmentInfo.CurrentType)
				{
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_GOVERNMENT_TYPE_CHANGED,
						EventMessage = TurnEventMessage.EM_GOVERNMENT_TYPE_CHANGED,
						PlayerID = this.App.LocalPlayer.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				IEnumerable<CombatData> enumerable2 = 
					from x in combats
					where x.GetPlayer(player.ID) != null && x.GetPlayer(player.ID).VictoryStatus != GameSession.VictoryStatus.Loss
					select x;
				foreach (CombatData current17 in enumerable2)
				{
					IEnumerable<PlayerCombatData> enumerable3 = 
						from x in current17.GetPlayers()
						where x.PlayerID != player.ID
						select x;
					PlayerCombatData player2 = current17.GetPlayer(player.ID);
					IEnumerable<ShipData> myShips = 
						from x in player2.ShipData
						where !x.destroyed
						select x;
                    Kerberos.Sots.StarFleet.StarFleet.CheckSystemCanSupportResidentFleets(this.App, current17.SystemID, player.ID);
					if (this._db.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, player.ID))
					{
						foreach (PlayerCombatData vsPlayer in enumerable3)
						{
							if (vsPlayer.PlayerID != player.ID)
							{
								if (this.App.GameDatabase.GetStandardPlayerIDs().Any((int x) => x == vsPlayer.PlayerID))
								{
									List<ShipData> list10 = vsPlayer.ShipData.ToList<ShipData>();
									foreach (ShipData current18 in list10)
									{
										this._app.GameDatabase.InsertDesignEncountered(player.ID, current18.designID);
									}
								}
							}
						}
					}
					this.AttemptToSalvageCombat(player, myShips, enumerable3);
				}
			}
			this.DoAutoOptionActions();
			this.UpdateColonyEconomyRating();
			this.CheckDestroyedIndys();
			this.CheckMoraleShipBonuses();
			this.CheckStationSlots();
			this.ClampMoraleValues();
			this.CheckEndGame();
			if (ScriptHost.AllowConsole)
			{
				App.Log.Trace("Diplomacy State Info Turn: " + this.App.GameDatabase.GetTurnCount(), "data");
				List<PlayerInfo> list11 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
				foreach (PlayerInfo current19 in list11)
				{
					foreach (PlayerInfo current20 in list11)
					{
						if (current20 != current19)
						{
							DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(current19.ID, current20.ID);
							if (diplomacyInfo != null)
							{
								if (!diplomacyInfo.isEncountered)
								{
									App.Log.Trace(string.Concat(new object[]
									{
										"P ",
										current19.ID,
										" and P ",
										current20.ID,
										" have not yet encountered"
									}), "data");
								}
								else
								{
									App.Log.Trace(string.Concat(new object[]
									{
										"P ",
										current19.ID,
										" and P ",
										current20.ID,
										" Relations[",
										diplomacyInfo.Relations,
										"] : Mood[",
										diplomacyInfo.GetDiplomaticMood().ToString(),
										"] : State[",
										diplomacyInfo.State.ToString(),
										"]"
									}), "data");
									List<DiplomacyReactionHistoryEntryInfo> list12 = this._db.GetDiplomacyReactionHistory(current19.ID, current20.ID, this.App.GameDatabase.GetTurnCount(), 1).ToList<DiplomacyReactionHistoryEntryInfo>();
									if (list12.Count > 0)
									{
										App.Log.Trace("Reaction Updates This Turn Between Players", "data");
										foreach (DiplomacyReactionHistoryEntryInfo current21 in list12)
										{
											App.Log.Trace(string.Concat(new object[]
											{
												"Reaction Type [",
												current21.Reaction.ToString(),
												"] difference [",
												current21.Difference,
												"]"
											}), "data");
										}
									}
								}
							}
						}
					}
				}
			}
		}
		private bool AttemptToSalvageCombat(PlayerInfo player, IEnumerable<ShipData> myShips, IEnumerable<PlayerCombatData> vsPlayers)
		{
			Faction faction = this.App.AssetDatabase.GetFaction(player.FactionID);
			List<ShipData> list = new List<ShipData>();
			if (faction.Name == "zuul")
			{
				list = myShips.ToList<ShipData>();
			}
			else
			{
				foreach (ShipData current in myShips)
				{
					DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(current.designID);
                    if (designInfo != null && Kerberos.Sots.StarFleet.StarFleet.GetIsSalavageCapable(this.App, designInfo))
					{
						list.Add(current);
					}
				}
			}
			if (!list.Any<ShipData>())
			{
				return false;
			}
			bool alreadySalavaging = this.SalavageColonies(player, list, vsPlayers, false);
			return this.SalvageShips(player, list, vsPlayers, alreadySalavaging);
		}
		private bool SalvageShips(PlayerInfo player, IEnumerable<ShipData> SalavageShips, IEnumerable<PlayerCombatData> vsPlayers, bool AlreadySalavaging)
		{
			List<string> list = new List<string>();
			IEnumerable<SpecialProjectInfo> source = 
				from x in this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(player.ID, true)
				where x.Type == SpecialProjectType.Salvage
				select x;
			bool flag = AlreadySalavaging;
			if (SalavageShips.Count<ShipData>() > 0)
			{
				foreach (PlayerCombatData current in vsPlayers)
				{
					List<ShipData> list2 = (
						from x in current.ShipData
						where x.destroyed
						select x).ToList<ShipData>();
					foreach (ShipData current2 in list2)
					{
						DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(current2.designID);
						DesignSectionInfo[] designSections = designInfo.DesignSections;
						for (int i = 0; i < designSections.Length; i++)
						{
							DesignSectionInfo designSectionInfo = designSections[i];
							foreach (WeaponBankInfo current3 in designSectionInfo.WeaponBanks)
							{
								if (current3.WeaponID.HasValue)
								{
									string wepAsset = this.App.GameDatabase.GetWeaponAsset(current3.WeaponID.Value);
									LogicalWeapon wep = this.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == wepAsset);
									if (wep != null)
									{
										List<PlayerTechInfo> list3 = (
											from x in this.App.GameDatabase.GetPlayerTechInfos(player.ID)
											where x.State != TechStates.Researched && x.State != TechStates.Locked
											select x).ToList<PlayerTechInfo>();
										list3 = (
											from x in list3
											where wep.RequiredTechs.Any((Kerberos.Sots.Data.WeaponFramework.Tech y) => y.Name == x.TechFileID)
											select x).ToList<PlayerTechInfo>();
										foreach (PlayerTechInfo pTech in list3)
										{
											if ((float)(pTech.Progress / pTech.ResearchCost) < 0.5f && !list.Contains(pTech.TechFileID))
											{
												pTech.Progress += new Random().NextInclusive(this.App.AssetDatabase.AccumulatedKnowledgeWeaponPerBattleMin, this.App.AssetDatabase.AccumulatedKnowledgeWeaponPerBattleMax);
												this.App.GameDatabase.UpdatePlayerTechInfo(pTech);
												list.Add(pTech.TechFileID);
												bool flag2 = false;
												if (!flag)
												{
													if (!source.Any((SpecialProjectInfo x) => x.TechID == pTech.TechID))
													{
														foreach (ShipData current4 in SalavageShips)
														{
															DesignInfo designInfo2 = this.App.GameDatabase.GetDesignInfo(current4.designID);
															if (designInfo2 != null)
															{
                                                                int num = Kerberos.Sots.StarFleet.StarFleet.GetSalvageChance(this._app, designInfo2);
																num += (int)((float)num * this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.SalvageModifier, player.ID));
                                                                if (App.GetSafeRandom().NextInclusive(1, 100) <= Kerberos.Sots.StarFleet.StarFleet.GetSalvageChance(this._app, designInfo2))
																{
																	flag2 = true;
																	break;
																}
															}
														}
														if ((flag2 && pTech.State == TechStates.PendingFeasibility) || pTech.State == TechStates.LowFeasibility || pTech.State == TechStates.HighFeasibility || pTech.State == TechStates.Branch || pTech.State != TechStates.Core)
														{
															int specialProjectID = GameSession.InsertNewSalvageProject(this.App, player.ID, pTech.TechID);
															this._app.GameDatabase.InsertTurnEvent(new TurnEvent
															{
																EventType = TurnEventType.EV_NEW_SALVAGE_PROJECT,
																EventMessage = TurnEventMessage.EM_NEW_SALVAGE_PROJECT,
																PlayerID = player.ID,
																TechID = pTech.TechID,
																TurnNumber = this._app.GameDatabase.GetTurnCount(),
																SpecialProjectID = specialProjectID,
																ShowsDialog = false
															});
															flag = true;
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
			}
			return flag;
		}
		private bool SalavageColonies(PlayerInfo player, IEnumerable<ShipData> SalavageShips, IEnumerable<PlayerCombatData> vsPlayers, bool AlreadySalavaging)
		{
			IEnumerable<SpecialProjectInfo> source = 
				from x in this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(player.ID, true)
				where x.Type == SpecialProjectType.Salvage
				select x;
			bool flag = AlreadySalavaging;
			foreach (PlayerCombatData current in vsPlayers)
			{
				if (flag)
				{
					break;
				}
				if (current.PlanetData.Count != 0)
				{
					Faction faction = this._app.AssetDatabase.Factions.First((Faction x) => x.ID == player.FactionID);
					List<PlayerTechInfo> list = (
						from x in this.App.GameDatabase.GetPlayerTechInfos(player.ID)
						where x.State != TechStates.Researched && x.State != TechStates.Locked
						select x).ToList<PlayerTechInfo>();
					List<PlayerTechInfo> vstechs = (
						from x in this.App.GameDatabase.GetPlayerTechInfos(current.PlayerID)
						where x.State == TechStates.Researched
						select x).ToList<PlayerTechInfo>();
					list = (
						from x in list
						where vstechs.Any((PlayerTechInfo y) => y.TechFileID == x.TechFileID)
						select x).ToList<PlayerTechInfo>();
					foreach (PlanetData current2 in current.PlanetData)
					{
						if (flag)
						{
							break;
						}
						if (this._app.GameDatabase.GetColonyInfoForPlanet(current2.orbitalObjectID) == null)
						{
							foreach (PlayerTechInfo pTech in list)
							{
								if (faction.CanFactionObtainTechBranch(pTech.TechFileID.Substring(0, 3)))
								{
									bool flag2 = false;
									if (!flag)
									{
										if (!source.Any((SpecialProjectInfo x) => x.TechID == pTech.TechID))
										{
											foreach (ShipData current3 in SalavageShips)
											{
												DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(current3.designID);
												if (designInfo != null)
												{
                                                    int num = Kerberos.Sots.StarFleet.StarFleet.GetSalvageChance(this._app, designInfo);
													num += (int)((float)num * this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.SalvageModifier, player.ID));
                                                    if (App.GetSafeRandom().NextInclusive(1, 100) <= Kerberos.Sots.StarFleet.StarFleet.GetSalvageChance(this._app, designInfo))
													{
														flag2 = true;
														break;
													}
												}
											}
											if ((flag2 && pTech.State == TechStates.PendingFeasibility) || pTech.State == TechStates.LowFeasibility || pTech.State == TechStates.HighFeasibility || pTech.State == TechStates.Branch || pTech.State != TechStates.Core)
											{
												int specialProjectID = GameSession.InsertNewSalvageProject(this.App, player.ID, pTech.TechID);
												this._app.GameDatabase.InsertTurnEvent(new TurnEvent
												{
													EventType = TurnEventType.EV_NEW_SALVAGE_PROJECT,
													EventMessage = TurnEventMessage.EM_NEW_SALVAGE_PROJECT,
													PlayerID = player.ID,
													TechID = pTech.TechID,
													TurnNumber = this._app.GameDatabase.GetTurnCount(),
													SpecialProjectID = specialProjectID,
													ShowsDialog = false
												});
												flag = true;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return flag;
		}
		private void UpdateColonyEconomyRating()
		{
			List<ColonyInfo> list = this._db.GetColonyInfos().ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				bool arg_46_0 = this._db.GetStarSystemInfo(current.CachedStarSystemID).IsOpen;
				PlayerInfo playerInfo = this._db.GetPlayerInfo(current.PlayerID);
				if (playerInfo.Savings <= -500000.0)
				{
					current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EmpireInDebt500K, 1);
				}
				if (playerInfo.Savings <= -1000000.0)
				{
					current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EmpireInDebt1Mil, 1);
				}
				if (playerInfo.Savings <= -2000000.0)
				{
					current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EmpireInDebt2Mil, 1);
				}
				if (playerInfo.Savings >= 500000.0)
				{
					current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.Savings500K, 1);
				}
				if (playerInfo.Savings >= 1000000.0)
				{
					current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.Savings1Mil, 1);
				}
				if (playerInfo.Savings >= 3000000.0)
				{
					current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.Savings3Mil, 1);
				}
				List<FleetInfo> list2 = this._db.GetFleetInfoBySystemID(current.CachedStarSystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				bool flag = true;
				bool flag2 = false;
				StarSystemInfo system = this._db.GetStarSystemInfo(current.CachedStarSystemID);
				if (system.ProvinceID.HasValue)
				{
					flag = false;
					List<StarSystemInfo> list3 = (
						from x in this._db.GetStarSystemInfos()
						where x.ProvinceID == system.ProvinceID
						select x).ToList<StarSystemInfo>();
					foreach (StarSystemInfo current2 in list3)
					{
						list2.AddRange(this._db.GetFleetInfoBySystemID(current2.ID, FleetType.FL_NORMAL).ToList<FleetInfo>());
					}
				}
				foreach (FleetInfo current3 in list2)
				{
					if (current.PlayerID != current3.PlayerID && this._db.GetDiplomacyStateBetweenPlayers(current.PlayerID, current3.PlayerID) == DiplomacyState.WAR)
					{
						if (!flag2 && current3.SystemID == current.CachedStarSystemID)
						{
							flag2 = true;
							current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EnemyInSystem, 1);
						}
						if (!flag)
						{
							flag = true;
							current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.EnemyInProvince, 1);
						}
						if (flag2 && flag)
						{
							break;
						}
					}
				}
				if (current.Factions.Any<ColonyFactionInfo>())
				{
					int num = (int)current.Factions.Average((ColonyFactionInfo x) => x.Morale);
					if (num < 20)
					{
						current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.MoraleUnder20, 1);
					}
					if (num >= 80)
					{
						current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.MoraleMin80, 1);
					}
				}
				List<MissionInfo> source = this._db.GetMissionsBySystemDest(current.CachedStarSystemID).ToList<MissionInfo>();
				if (source.Any((MissionInfo x) => x.Type == MissionType.CONSTRUCT_STN || x.Type == MissionType.UPGRADE_STN))
				{
					current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.ConstructionOrUpgradeInSystem, 1);
				}
				current.EconomyRating = Math.Min(Math.Max(current.EconomyRating, 0f), 1f);
				this._db.UpdateColony(current);
			}
		}
		private void ValidateHomeWorld(int playerid)
		{
			HomeworldInfo homeworldInfo = this._app.GameDatabase.GetPlayerHomeworld(playerid);
			ColonyInfo colonyInfo = null;
			if (homeworldInfo != null)
			{
				colonyInfo = this._app.GameDatabase.GetColonyInfo(homeworldInfo.ColonyID);
			}
			if (homeworldInfo == null || colonyInfo == null || colonyInfo.PlayerID != playerid)
			{
				if (homeworldInfo == null)
				{
					homeworldInfo = new HomeworldInfo
					{
						PlayerID = playerid
					};
				}
				List<ColonyInfo> list = (
					from x in this._app.GameDatabase.GetColonyInfos()
					where x.PlayerID == playerid
					select x).ToList<ColonyInfo>();
				ColonyInfo colonyInfo2 = null;
				foreach (ColonyInfo current in list)
				{
					if (colonyInfo2 == null)
					{
						colonyInfo2 = current;
					}
					else
					{
						if (this._app.GameDatabase.GetTotalPopulation(colonyInfo2) < this._app.GameDatabase.GetTotalPopulation(current))
						{
							colonyInfo2 = current;
						}
					}
				}
				if (colonyInfo2 != null)
				{
					int? num = null;
					List<StarSystemInfo> list2 = this._app.GameDatabase.GetVisibleStarSystemInfos(playerid).ToList<StarSystemInfo>();
					foreach (StarSystemInfo current2 in list2)
					{
						foreach (ColonyInfo current3 in this._app.GameDatabase.GetColonyInfosForSystem(current2.ID))
						{
							if (current3.ID == colonyInfo2.ID)
							{
								num = new int?(current2.ID);
								break;
							}
						}
						if (num.HasValue)
						{
							break;
						}
					}
					if (num.HasValue)
					{
						homeworldInfo.SystemID = num.Value;
						homeworldInfo.ColonyID = colonyInfo2.ID;
						this._app.GameDatabase.UpdateHomeworldInfo(homeworldInfo);
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_HOMEWORLD_REESTABLISHED,
							EventMessage = TurnEventMessage.EM_HOMEWORLD_REESTABLISHED,
							PlayerID = homeworldInfo.PlayerID,
							ColonyID = homeworldInfo.ColonyID,
							SystemID = homeworldInfo.SystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
					}
				}
			}
		}
		private void UpdatePlayerEmpireHistoryData()
		{
			foreach (PlayerInfo current in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				EmpireHistoryData empireHistoryData = new EmpireHistoryData();
				Budget budget = Budget.GenerateBudget(this.App.Game, current, null, BudgetProjection.Pessimistic);
				empireHistoryData.playerID = current.ID;
				empireHistoryData.colonies = this.App.GameDatabase.GetNumColonies(current.ID);
				empireHistoryData.provinces = this.App.GameDatabase.GetNumProvinces(current.ID);
				empireHistoryData.bases = this.App.GameDatabase.GetStationInfosByPlayerID(current.ID).Count<StationInfo>();
				empireHistoryData.fleets = this.App.GameDatabase.GetFleetInfosByPlayerID(current.ID, FleetType.FL_NORMAL).Count<FleetInfo>();
				empireHistoryData.ships = this.App.GameDatabase.GetNumShips(current.ID);
				empireHistoryData.empire_pop = this.App.GameDatabase.GetEmpirePopulation(current.ID);
				empireHistoryData.empire_economy = this.App.GameDatabase.GetEmpireEconomy(current.ID);
				empireHistoryData.empire_biosphere = this.App.GameDatabase.GetEmpireBiosphere(current.ID);
				empireHistoryData.empire_trade = budget.TradeRevenue;
				if (this.App.GameDatabase.GetEmpireMorale(current.ID).HasValue)
				{
					empireHistoryData.empire_morale = this.App.GameDatabase.GetEmpireMorale(current.ID).Value;
				}
				else
				{
					empireHistoryData.empire_morale = 0;
				}
				empireHistoryData.savings = current.Savings;
				empireHistoryData.psi_potential = current.PsionicPotential;
				this.App.GameDatabase.InsertEmpireHistoryForPlayer(empireHistoryData);
			}
		}
		private void CheckMoraleShipBonuses()
		{
			foreach (PlayerInfo current in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				List<ColonyInfo> list = this._db.GetPlayerColoniesByPlayerId(current.ID).ToList<ColonyInfo>();
				foreach (ColonyInfo current2 in list)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(current2.OrbitalObjectID);
					int stratModifier = this._db.GetStratModifier<int>(StratModifiers.PoliceMoralBonus, current2.PlayerID);
					int num = (GameSession.NumPoliceInSystem(this.App, orbitalObjectInfo.StarSystemID) > 0) ? stratModifier : 0;
					int propagandaBonusInSystem = GameSession.GetPropagandaBonusInSystem(this.App, orbitalObjectInfo.StarSystemID, current2.PlayerID);
					if (num > 0)
					{
						num += stratModifier * (this._db.GetStarSystemInfo(current2.CachedStarSystemID).IsOpen ? 1 : 2);
					}
					ColonyFactionInfo[] factions = current2.Factions;
					for (int i = 0; i < factions.Length; i++)
					{
						ColonyFactionInfo colonyFactionInfo = factions[i];
						if (colonyFactionInfo.LastMorale > colonyFactionInfo.Morale)
						{
							colonyFactionInfo.Morale += num;
						}
						colonyFactionInfo.Morale += propagandaBonusInSystem;
						colonyFactionInfo.LastMorale = colonyFactionInfo.Morale;
						this._db.UpdateCivilianPopulation(colonyFactionInfo);
					}
				}
			}
		}
		private void CheckStationSlots()
		{
			Dictionary<int, StationTypeFlags> dictionary = new Dictionary<int, StationTypeFlags>();
			List<StationInfo> list = this._db.GetStationInfos().ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(current.OrbitalObjectID);
				if (orbitalObjectInfo != null && orbitalObjectInfo.ParentID.HasValue)
				{
					StationTypeFlags stationTypeFlags = (StationTypeFlags)0;
					if (!dictionary.TryGetValue(orbitalObjectInfo.ParentID.Value, out stationTypeFlags))
					{
						PlanetInfo planetInfo = this._db.GetPlanetInfo(orbitalObjectInfo.ParentID.Value);
						if (planetInfo == null)
						{
							continue;
						}
						stationTypeFlags = Kerberos.Sots.GameStates.StarSystem.GetSupportedStationTypesForPlanet(this._db, planetInfo);
						dictionary.Add(orbitalObjectInfo.ParentID.Value, stationTypeFlags);
					}
					if ((stationTypeFlags & (StationTypeFlags)(1 << (int)current.DesignInfo.StationType)) == (StationTypeFlags)0)
					{
						this._db.DestroyStation(this, current.OrbitalObjectID, 0);
					}
				}
			}
		}
		private void CheckDestroyedIndys()
		{
			List<PlayerInfo> list = this._db.GetPlayerInfos().ToList<PlayerInfo>();
			list.RemoveAll((PlayerInfo x) => x.isStandardPlayer || !x.includeInDiplomacy);
			foreach (PlayerInfo current in list)
			{
				if (this._db.GetPlayerColoniesByPlayerId(current.ID).Count<ColonyInfo>() == 0)
				{
					this._db.SetPlayerDefeated(this, current.ID);
				}
			}
		}
		private void ClampMoraleValues()
		{
			foreach (PlayerInfo current in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				List<ColonyInfo> list = this._db.GetPlayerColoniesByPlayerId(current.ID).ToList<ColonyInfo>();
				foreach (ColonyInfo current2 in list)
				{
					ColonyFactionInfo[] factions = current2.Factions;
					for (int i = 0; i < factions.Length; i++)
					{
						ColonyFactionInfo colonyFactionInfo = factions[i];
						int morale = colonyFactionInfo.Morale;
						colonyFactionInfo.Morale = Math.Min(Math.Max(morale, 0), 100);
						if (colonyFactionInfo.Morale != morale)
						{
							this._db.UpdateCivilianPopulation(colonyFactionInfo);
						}
					}
				}
			}
		}
		private void CheckEndGame()
		{
			List<PlayerInfo> list = new List<PlayerInfo>();
			List<PlayerInfo> list2 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list2)
			{
				if (this._db.GetPlayerBankruptcyTurns(current.ID) > this.AssetDatabase.BankruptcyTurns || this._db.GetPlayerColoniesByPlayerId(current.ID).Count<ColonyInfo>() <= 0)
				{
					list.Add(current);
				}
			}
			string nameValue = this._app.GameDatabase.GetNameValue("VictoryCondition");
			GameMode gameMode = GameMode.LastSideStanding;
			int num = -1;
			if (!string.IsNullOrEmpty(nameValue))
			{
				gameMode = (GameMode)Enum.Parse(typeof(GameMode), nameValue);
				num = int.Parse(this._app.GameDatabase.GetNameValue("VictoryValue"));
			}
			switch (gameMode)
			{
			case GameMode.LastSideStanding:
				this.CheckLastSideStanding();
				goto IL_C5A;
			case GameMode.LastCapitalStanding:
			{
				int? homeworld = this._app.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID).Homeworld;
				if (!homeworld.HasValue || this._app.GameDatabase.GetColonyInfoForPlanet(homeworld.Value) == null)
				{
					this._app.UI.CreateDialog(new EndGameDialog(this._app, App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), "loseScreen"), null);
				}
				bool flag = false;
				List<PlayerInfo> list3 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
				list3.RemoveAll((PlayerInfo x) => x.ID == this.LocalPlayer.ID);
				foreach (PlayerInfo current2 in list3)
				{
					if (current2.ID != this.LocalPlayer.ID && this.GameDatabase.GetDiplomacyStateBetweenPlayers(current2.ID, this.LocalPlayer.ID) != DiplomacyState.ALLIED)
					{
						if (current2.Homeworld.HasValue && this.GameDatabase.GetColonyInfoForPlanet(current2.Homeworld.Value) != null)
						{
							flag = true;
						}
						else
						{
							list.Add(current2);
						}
					}
				}
				if (!flag)
				{
					this._app.UI.CreateDialog(new EndGameDialog(this._app, App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), "winScreen"), null);
					goto IL_C5A;
				}
				goto IL_C5A;
			}
			case GameMode.StarChamberLimit:
				goto IL_A9A;
			case GameMode.GemWorldLimit:
			{
				List<PlayerInfo> list4 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
				using (List<PlayerInfo>.Enumerator enumerator3 = list4.GetEnumerator())
				{
					PlayerInfo pi;
					while (enumerator3.MoveNext())
					{
						pi = enumerator3.Current;
						List<ColonyInfo> source = this.GameDatabase.GetPlayerColoniesByPlayerId(pi.ID).ToList<ColonyInfo>();
						if (source.Count((ColonyInfo x) => x.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld) >= num)
						{
							this.SendEndGameDialog((
								from x in list4
								where x.ID == pi.ID
								select x.ID).ToList<int>(), (
								from x in list4
								where x.ID != pi.ID
								select x.ID).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
						}
					}
					goto IL_C5A;
				}
				break;
			}
			case GameMode.ProvinceLimit:
				goto IL_8E8;
			case GameMode.LeviathanLimit:
				goto IL_6D3;
			case GameMode.LandGrab:
				break;
			default:
				goto IL_C5A;
			}
			List<PlayerInfo> list5 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			int num2 = 0;
			foreach (PlayerInfo current3 in list5)
			{
				List<int> list6 = this.GameDatabase.GetPlayerColonySystemIDs(current3.ID).ToList<int>();
				num2 += list6.Count;
				dictionary.Add(current3.ID, list6.Count);
			}
			using (List<PlayerInfo>.Enumerator enumerator5 = list5.GetEnumerator())
			{
				PlayerInfo pi;
				while (enumerator5.MoveNext())
				{
					pi = enumerator5.Current;
					if ((float)dictionary[pi.ID] / (float)num2 >= (float)num / 100f)
					{
						this.SendEndGameDialog((
							from x in list5
							where x.ID == pi.ID
							select x.ID).ToList<int>(), (
							from x in list5
							where x.ID != pi.ID
							select x.ID).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
					}
				}
				goto IL_C5A;
			}
			IL_6D3:
			List<PlayerInfo> list7 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			using (List<PlayerInfo>.Enumerator enumerator6 = list7.GetEnumerator())
			{
				PlayerInfo pi;
				while (enumerator6.MoveNext())
				{
					pi = enumerator6.Current;
					int num3 = 0;
					List<FleetInfo> list8 = this.GameDatabase.GetFleetInfosByPlayerID(pi.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
					foreach (FleetInfo current4 in list8)
					{
						List<ShipInfo> source2 = this.GameDatabase.GetShipInfoByFleetID(current4.ID, true).ToList<ShipInfo>();
						num3 += source2.Count((ShipInfo x) => x.DesignInfo.Class == ShipClass.Leviathan && !x.DesignInfo.IsSuulka());
					}
					if (num3 >= num)
					{
						this.SendEndGameDialog((
							from x in list7
							where x.ID == pi.ID
							select x.ID).ToList<int>(), (
							from x in list7
							where x.ID != pi.ID
							select x.ID).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
					}
				}
				goto IL_C5A;
			}
			IL_8E8:
			List<PlayerInfo> list9 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			List<ProvinceInfo> source3 = this.GameDatabase.GetProvinceInfos().ToList<ProvinceInfo>();
			using (List<PlayerInfo>.Enumerator enumerator8 = list9.GetEnumerator())
			{
				PlayerInfo pi;
				while (enumerator8.MoveNext())
				{
					pi = enumerator8.Current;
					if (source3.Count((ProvinceInfo x) => x.PlayerID == pi.ID) >= num)
					{
						this.SendEndGameDialog((
							from x in list9
							where x.ID == pi.ID
							select x.ID).ToList<int>(), (
							from x in list9
							where x.ID != pi.ID
							select x.ID).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
					}
				}
				goto IL_C5A;
			}
			IL_A9A:
			List<PlayerInfo> list10 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo pi in list10)
			{
				List<StationInfo> source4 = this.GameDatabase.GetStationInfosByPlayerID(pi.ID).ToList<StationInfo>();
				if (source4.Count((StationInfo x) => x.DesignInfo.StationLevel == 5 && x.DesignInfo.StationType == StationType.DIPLOMATIC) >= num)
				{
					this.SendEndGameDialog((
						from x in list10
						where x.ID == pi.ID
						select x.ID).ToList<int>(), (
						from x in list10
						where x.ID != pi.ID
						select x.ID).ToList<int>(), App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName));
				}
			}
			IL_C5A:
			if (gameMode != GameMode.LastSideStanding)
			{
				this.CheckLastSideStanding();
			}
			foreach (PlayerInfo current5 in list)
			{
				if (!current5.isDefeated)
				{
					GameSession.SimAITurns = 0;
					this._db.SetPlayerDefeated(this, current5.ID);
					this.App.GameSetup.Players[current5.ID - 1].Status = NPlayerStatus.PS_DEFEATED;
				}
			}
		}
		private void CheckLastSideStanding()
		{
			List<ColonyInfo> list = this.GameDatabase.GetPlayerColoniesByPlayerId(this.LocalPlayer.ID).ToList<ColonyInfo>();
			if (list.Count == 0)
			{
				this._app.UI.CreateDialog(new EndGameDialog(this._app, App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), "loseScreen"), null);
			}
			bool flag = false;
			List<PlayerInfo> list2 = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			list2.RemoveAll((PlayerInfo x) => x.ID == this.LocalPlayer.ID);
			foreach (PlayerInfo current in list2)
			{
				if (this.GameDatabase.GetDiplomacyStateBetweenPlayers(current.ID, this.LocalPlayer.ID) != DiplomacyState.ALLIED && this.GameDatabase.GetPlayerColoniesByPlayerId(current.ID).Count<ColonyInfo>() > 0)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				this._app.UI.CreateDialog(new EndGameDialog(this._app, App.Localize("@UI_ENDGAME_VICTORY"), string.Format(App.Localize("@UI_ENDGAME_VICTORY_CONQUEST"), this._app.GameSetup.Players[this._app.Game.LocalPlayer.ID - 1].EmpireName), "winScreen"), null);
				GameSession.SimAITurns = 0;
			}
		}
		public void SendEndGameDialog(List<int> winners, List<int> losers, string victoryTitle, string victoryDesc, string defeatTitle, string defeatDesc)
		{
			if (winners.Contains(this.LocalPlayer.ID))
			{
				this._app.UI.CreateDialog(new EndGameDialog(this._app, victoryTitle, victoryDesc, "winScreen"), null);
				return;
			}
			if (losers.Contains(this.LocalPlayer.ID))
			{
				this._app.UI.CreateDialog(new EndGameDialog(this._app, defeatTitle, defeatDesc, "loseScreen"), null);
			}
		}
		public static void ApplyConsequences(GameDatabase db, int offendingPlayer, int receivingPlayer, List<TreatyConsequenceInfo> consequences)
		{
			foreach (TreatyConsequenceInfo current in consequences)
			{
				PlayerInfo playerInfo = db.GetPlayerInfo(offendingPlayer);
				PlayerInfo playerInfo2 = db.GetPlayerInfo(receivingPlayer);
				List<TreatyInfo> list;
				switch (current.Type)
				{
				case ConsequenceType.Fine:
					db.UpdatePlayerSavings(offendingPlayer, playerInfo.Savings - (double)current.ConsequenceValue);
					db.UpdatePlayerSavings(receivingPlayer, playerInfo2.Savings + (double)current.ConsequenceValue);
					continue;
				case ConsequenceType.DiplomaticStatusPenalty:
				{
					DiplomacyState state = db.GetDiplomacyStateBetweenPlayers(offendingPlayer, receivingPlayer);
					switch (state)
					{
					case DiplomacyState.CEASE_FIRE:
						state = DiplomacyState.NEUTRAL;
						break;
					case DiplomacyState.NON_AGGRESSION:
						state = DiplomacyState.CEASE_FIRE;
						break;
					case DiplomacyState.ALLIED:
						state = DiplomacyState.PEACE;
						break;
					case DiplomacyState.NEUTRAL:
						state = DiplomacyState.WAR;
						db.InsertGovernmentAction(offendingPlayer, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
						break;
					case DiplomacyState.PEACE:
						state = DiplomacyState.NON_AGGRESSION;
						break;
					}
					db.ChangeDiplomacyState(offendingPlayer, receivingPlayer, state);
					continue;
				}
				case ConsequenceType.Trade:
					list = db.GetTreatyInfos().ToList<TreatyInfo>();
					using (List<TreatyInfo>.Enumerator enumerator2 = list.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							TreatyInfo current2 = enumerator2.Current;
							if (current2.Type == TreatyType.Trade && ((current2.ReceivingPlayerId == offendingPlayer && current2.InitiatingPlayerId == receivingPlayer) || (current2.InitiatingPlayerId == receivingPlayer && current2.ReceivingPlayerId == receivingPlayer)))
							{
								db.RemoveTreatyInfo(current2.ID);
							}
						}
						continue;
					}
					break;
				case ConsequenceType.Sanction:
					break;
				case ConsequenceType.DiplomaticPointPenalty:
					db.UpdateGenericDiplomacyPoints(offendingPlayer, playerInfo.GenericDiplomacyPoints - (int)current.ConsequenceValue);
					db.UpdateGenericDiplomacyPoints(offendingPlayer, playerInfo2.GenericDiplomacyPoints + (int)current.ConsequenceValue);
					continue;
				case ConsequenceType.War:
					goto IL_279;
				default:
					continue;
				}
				List<int> list2 = db.GetStandardPlayerIDs().ToList<int>();
				List<int> list3 = new List<int>();
				foreach (int current3 in list2)
				{
					if (current3 != receivingPlayer && current3 != offendingPlayer && db.GetDiplomacyStateBetweenPlayers(receivingPlayer, current3) == DiplomacyState.ALLIED)
					{
						list3.Add(current3);
					}
				}
				list = db.GetTreatyInfos().ToList<TreatyInfo>();
				using (List<TreatyInfo>.Enumerator enumerator4 = list.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						TreatyInfo current4 = enumerator4.Current;
						if (current4.Type == TreatyType.Trade && ((current4.ReceivingPlayerId == offendingPlayer && list3.Contains(current4.InitiatingPlayerId)) || (current4.InitiatingPlayerId == offendingPlayer && list3.Contains(current4.ReceivingPlayerId))))
						{
							db.RemoveTreatyInfo(current4.ID);
						}
					}
					continue;
				}
				IL_279:
				db.ChangeDiplomacyState(offendingPlayer, receivingPlayer, DiplomacyState.WAR);
				db.InsertGovernmentAction(offendingPlayer, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
			}
		}
        public static bool CheckLimitationTreaty(GameDatabase db, LimitationTreatyInfo lti, out int playerId)
        {
            List<ColonyInfo> list;
            List<FleetInfo> list2;
            Func<ColonyInfo, bool> predicate = null;
            Func<ColonyInfo, bool> func2 = null;
            Func<FleetInfo, bool> func3 = null;
            Func<FleetInfo, int> selector = null;
            Func<FleetInfo, bool> func5 = null;
            Func<FleetInfo, int> func6 = null;
            Func<ColonyInfo, bool> func7 = null;
            Func<ColonyInfo, bool> func8 = null;
            Func<FleetInfo, bool> func9 = null;
            Func<FleetInfo, bool> func10 = null;
            Func<StationInfo, bool> func11 = null;
            Func<StationInfo, bool> func12 = null;
            Func<StationInfo, bool> func13 = null;
            Func<StationInfo, bool> func14 = null;
            playerId = 0;
            string fileId;
            switch (lti.LimitationType)
            {
                case LimitationTreatyType.FleetSize:
                    list2 = db.GetFleetInfos(FleetType.FL_NORMAL).ToList<FleetInfo>();
                    if (func3 == null)
                    {
                        func3 = x => x.PlayerID == lti.ReceivingPlayerId;
                    }
                    if (selector == null)
                    {
                        selector = x => db.GetFleetCruiserEquivalent(x.ID);
                    }
                    if (list2.Where<FleetInfo>(func3).ToList<FleetInfo>().Sum<FleetInfo>(selector) > lti.LimitationAmount)
                    {
                        playerId = lti.ReceivingPlayerId;
                        return false;
                    }
                    if (func5 == null)
                    {
                        func5 = x => x.PlayerID == lti.InitiatingPlayerId;
                    }
                    if (func6 == null)
                    {
                        func6 = x => db.GetFleetCruiserEquivalent(x.ID);
                    }
                    if (list2.Where<FleetInfo>(func5).ToList<FleetInfo>().Sum<FleetInfo>(func6) > lti.LimitationAmount)
                    {
                        playerId = lti.InitiatingPlayerId;
                        return false;
                    }
                    return true;

                case LimitationTreatyType.ShipClass:
                    {
                        list2 = db.GetFleetInfos(FleetType.FL_NORMAL).ToList<FleetInfo>();
                        if (func9 == null)
                        {
                            func9 = x => x.PlayerID == lti.ReceivingPlayerId;
                        }
                        List<FleetInfo> list3 = list2.Where<FleetInfo>(func9).ToList<FleetInfo>();
                        int num2 = 0;
                        foreach (FleetInfo info in list3)
                        {
                            foreach (ShipInfo info2 in db.GetShipInfoByFleetID(info.ID, true).ToList<ShipInfo>())
                            {
                                if (info2.DesignInfo.Class == (ShipClass)int.Parse(lti.LimitationGroup))
                                {
                                    num2++;
                                }
                            }
                        }
                        if (num2 > lti.LimitationAmount)
                        {
                            playerId = lti.ReceivingPlayerId;
                            return false;
                        }
                        if (func10 == null)
                        {
                            func10 = x => x.PlayerID == lti.InitiatingPlayerId;
                        }
                        list3 = list2.Where<FleetInfo>(func10).ToList<FleetInfo>();
                        num2 = 0;
                        foreach (FleetInfo info3 in list3)
                        {
                            foreach (ShipInfo info4 in db.GetShipInfoByFleetID(info3.ID, true).ToList<ShipInfo>())
                            {
                                if (info4.DesignInfo.Class == (ShipClass)int.Parse(lti.LimitationGroup))
                                {
                                    num2++;
                                }
                            }
                        }
                        if (num2 > lti.LimitationAmount)
                        {
                            playerId = lti.InitiatingPlayerId;
                            return false;
                        }
                        return true;
                    }
                case LimitationTreatyType.Weapon:
                    foreach (DesignInfo info5 in db.GetDesignInfosForPlayer(lti.ReceivingPlayerId).ToList<DesignInfo>())
                    {
                        foreach (DesignSectionInfo info6 in info5.DesignSections)
                        {
                            foreach (WeaponBankInfo info7 in info6.WeaponBanks)
                            {
                                int? weaponID = info7.WeaponID;
                                int num4 = int.Parse(lti.LimitationGroup);
                                if ((weaponID.GetValueOrDefault() == num4) && weaponID.HasValue)
                                {
                                    playerId = lti.ReceivingPlayerId;
                                    return false;
                                }
                            }
                        }
                    }
                    foreach (DesignInfo info8 in db.GetDesignInfosForPlayer(lti.InitiatingPlayerId).ToList<DesignInfo>())
                    {
                        foreach (DesignSectionInfo info9 in info8.DesignSections)
                        {
                            foreach (WeaponBankInfo info10 in info9.WeaponBanks)
                            {
                                int? nullable2 = info10.WeaponID;
                                int num6 = int.Parse(lti.LimitationGroup);
                                if ((nullable2.GetValueOrDefault() == num6) && nullable2.HasValue)
                                {
                                    playerId = lti.InitiatingPlayerId;
                                    return false;
                                }
                            }
                        }
                    }
                    return true;

                case LimitationTreatyType.ResearchTree:
                    {
                        int playerResearchingTechID = db.GetPlayerResearchingTechID(lti.ReceivingPlayerId);
                        if (playerResearchingTechID != 0)
                        {
                            fileId = db.GetTechFileID(playerResearchingTechID);
                            if (db.AssetDatabase.MasterTechTree.Technologies.First(x => (x.Id == fileId)).Family == lti.LimitationGroup)
                            {
                                playerId = lti.ReceivingPlayerId;
                                return false;
                            }
                            playerResearchingTechID = db.GetPlayerResearchingTechID(lti.InitiatingPlayerId);
                            if (playerResearchingTechID != 0)
                            {
                                fileId = db.GetTechFileID(playerResearchingTechID);
                                if (db.AssetDatabase.MasterTechTree.Technologies.First(x => (x.Id == fileId)).Family == lti.LimitationGroup)
                                {
                                    playerId = lti.InitiatingPlayerId;
                                    return false;
                                }
                            }
                            return true;
                        }
                        return true;
                    }
                case LimitationTreatyType.ResearchTech:
                    if (db.GetPlayerResearchingTechID(lti.ReceivingPlayerId) != int.Parse(lti.LimitationGroup))
                    {
                        if (db.GetPlayerResearchingTechID(lti.InitiatingPlayerId) == int.Parse(lti.LimitationGroup))
                        {
                            playerId = lti.InitiatingPlayerId;
                            return false;
                        }
                        return true;
                    }
                    playerId = lti.ReceivingPlayerId;
                    return false;

                case LimitationTreatyType.EmpireSize:
                    list = db.GetColonyInfos().ToList<ColonyInfo>();
                    if (predicate == null)
                    {
                        predicate = x => x.PlayerID == lti.InitiatingPlayerId;
                    }
                    if (list.Where<ColonyInfo>(predicate).Count<ColonyInfo>() > ((int)lti.LimitationAmount))
                    {
                        playerId = lti.InitiatingPlayerId;
                        return false;
                    }
                    if (func2 == null)
                    {
                        func2 = x => x.PlayerID == lti.ReceivingPlayerId;
                    }
                    if (list.Where<ColonyInfo>(func2).Count<ColonyInfo>() > ((int)lti.LimitationAmount))
                    {
                        playerId = lti.ReceivingPlayerId;
                        return false;
                    }
                    return true;

                case LimitationTreatyType.ForgeGemWorlds:
                    list = db.GetColonyInfos().ToList<ColonyInfo>();
                    if (func7 == null)
                    {
                        func7 = delegate(ColonyInfo x)
                        {
                            if ((x.CurrentStage != Kerberos.Sots.Data.ColonyStage.GemWorld) && (x.CurrentStage != Kerberos.Sots.Data.ColonyStage.ForgeWorld))
                            {
                                return false;
                            }
                            return x.PlayerID == lti.InitiatingPlayerId;
                        };
                    }
                    if (list.Where<ColonyInfo>(func7).Count<ColonyInfo>() > ((int)lti.LimitationAmount))
                    {
                        playerId = lti.InitiatingPlayerId;
                        return false;
                    }
                    if (func8 == null)
                    {
                        func8 = delegate(ColonyInfo x)
                        {
                            if ((x.CurrentStage != Kerberos.Sots.Data.ColonyStage.GemWorld) && (x.CurrentStage != Kerberos.Sots.Data.ColonyStage.ForgeWorld))
                            {
                                return false;
                            }
                            return x.PlayerID == lti.ReceivingPlayerId;
                        };
                    }
                    if (list.Where<ColonyInfo>(func8).Count<ColonyInfo>() > ((int)lti.LimitationAmount))
                    {
                        playerId = lti.ReceivingPlayerId;
                        return false;
                    }
                    return true;

                case LimitationTreatyType.StationType:
                    {
                        List<StationInfo> source = db.GetStationInfos().ToList<StationInfo>();
                        if (func11 == null)
                        {
                            func11 = x => x.PlayerID == lti.ReceivingPlayerId;
                        }
                        if (func12 == null)
                        {
                            func12 = x => x.DesignInfo.StationType == (StationType)int.Parse(lti.LimitationGroup);
                        }
                        if (source.Where<StationInfo>(func11).ToList<StationInfo>().Count<StationInfo>(func12) > lti.LimitationAmount)
                        {
                            playerId = lti.ReceivingPlayerId;
                            return false;
                        }
                        if (func13 == null)
                        {
                            func13 = x => x.PlayerID == lti.InitiatingPlayerId;
                        }
                        if (func14 == null)
                        {
                            func14 = x => x.DesignInfo.StationType == (StationType)int.Parse(lti.LimitationGroup);
                        }
                        if (source.Where<StationInfo>(func13).ToList<StationInfo>().Count<StationInfo>(func14) > lti.LimitationAmount)
                        {
                            playerId = lti.InitiatingPlayerId;
                            return false;
                        }
                        return true;
                    }
            }
            return true;
        }
        public void UpdateRequests(GameDatabase db)
		{
			List<RequestInfo> list = (
				from x in db.GetRequestInfos().ToList<RequestInfo>()
				where x.State == AgreementState.Unrequested
				select x).ToList<RequestInfo>();
			foreach (RequestInfo current in list)
			{
				PlayerInfo playerInfo = db.GetPlayerInfo(current.ReceivingPlayer);
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerInfo.ID);
				if (playerObject.IsAI())
				{
					if (playerObject.GetAI().HandleRequestRequest(current))
					{
						this.AcceptRequest(current);
					}
					else
					{
						this.DeclineRequest(current);
					}
				}
				else
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_REQUEST_REQUESTED,
						EventMessage = TurnEventMessage.EM_REQUEST_REQUESTED,
						PlayerID = current.ReceivingPlayer,
						TargetPlayerID = current.InitiatingPlayer,
						TreatyID = current.ID,
						TurnNumber = this._app.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				}
			}
		}
		public int GetLOAPlayerForRebellion(PlayerInfo poorSap)
		{
			List<PlayerInfo> list = this._db.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = list.FirstOrDefault((PlayerInfo x) => x.isAIRebellionPlayer && x.FactionID == poorSap.FactionID);
			int num;
			if (playerInfo == null)
			{
				FactionInfo factionInfo = this._db.GetFactionInfo(poorSap.FactionID);
				num = this._db.InsertPlayer(App.Localize("@AI_REBELLION_NAME"), factionInfo.Name, null, new Vector3(0.19f, 0.04f, 0.04f), Vector3.Zero, "", "", 0.0, 0, false, true, true, 0, AIDifficulty.Normal);
				this.AddPlayerObject(this._db.GetPlayerInfo(num), Kerberos.Sots.PlayerFramework.Player.ClientTypes.AI);
				this._db.DuplicateStratModifiers(num, poorSap.ID);
				this._db.DuplicateTechs(num, poorSap.ID);
				this.AvailableShipSectionsChanged();
				this.CheckForNewEquipment(num);
				this.AvailableShipSectionsChanged();
				float stratModifier = this._db.GetStratModifier<float>(StratModifiers.AIProductionBonus, num);
				float stratModifier2 = this._db.GetStratModifier<float>(StratModifiers.AIResearchBonus, num);
				float stratModifier3 = this._db.GetStratModifier<float>(StratModifiers.ConstructionPointBonus, num);
				this._db.SetStratModifier(StratModifiers.AIProductionBonus, num, stratModifier + 5f);
				this._db.SetStratModifier(StratModifiers.AIResearchBonus, num, stratModifier2 + 3f);
				this._db.SetStratModifier(StratModifiers.ConstructionPointBonus, num, stratModifier3 + 3f);
				using (List<PlayerInfo>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PlayerInfo current = enumerator.Current;
						if (current.isAIRebellionPlayer)
						{
							this._db.UpdateDiplomacyState(current.ID, num, DiplomacyState.ALLIED, 2000, true);
						}
						else
						{
							this._db.UpdateDiplomacyState(current.ID, num, DiplomacyState.WAR, 0, true);
						}
					}
					return num;
				}
			}
			num = playerInfo.ID;
			return num;
		}
		public void DoAIRebellion(PlayerInfo poorSap, bool researchtrigger = false)
		{
			int lOAPlayerForRebellion = this.GetLOAPlayerForRebellion(poorSap);
			List<int> list = this._db.GetPlayerColonySystemIDs(poorSap.ID).ToList<int>();
			HomeworldInfo playerHomeworld = this._db.GetPlayerHomeworld(poorSap.ID);
			list.Remove(playerHomeworld.SystemID);
			int num = (int)Math.Floor((double)((float)list.Count * this.AssetDatabase.AIRebellionColonyPercent));
			List<string> list2 = new List<string>();
			for (int i = 0; i < num; i++)
			{
				int num2 = this._random.Choose(list);
				list.Remove(num2);
				List<ColonyInfo> list3 = this._db.GetColonyInfosForSystem(num2).ToList<ColonyInfo>();
				foreach (ColonyInfo current in list3)
				{
					PlanetInfo planetInfo = this._db.GetPlanetInfo(current.OrbitalObjectID);
					List<ColonyFactionInfo> list4 = this._db.GetCivilianPopulations(current.OrbitalObjectID).ToList<ColonyFactionInfo>();
					foreach (ColonyFactionInfo current2 in list4)
					{
						this._db.RemoveCivilianPopulation(current.OrbitalObjectID, current2.FactionID);
					}
					current.PlayerID = lOAPlayerForRebellion;
					planetInfo.Biosphere = 0;
					this._db.UpdateColony(current);
					this._db.UpdatePlanet(planetInfo);
					this._db.InsertAIOldColonyOwner(current.ID, poorSap.ID);
				}
				list2.Add(this._db.GetStarSystemInfo(num2).Name);
			}
			if (num > 0)
			{
				string text = "";
				for (int j = 0; j < list2.Count - 1; j++)
				{
					text = text + list2[j] + ", ";
				}
				text += list2[list2.Count - 1];
				if (researchtrigger)
				{
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_RESEARCH_AI_DISASTER,
						EventMessage = TurnEventMessage.EM_RESEARCH_AI_DISASTER,
						NamesList = text,
						PlayerID = poorSap.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				else
				{
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_AI_REBELLION_START,
						EventMessage = TurnEventMessage.EM_AI_REBELLION_START,
						NamesList = text,
						PlayerID = poorSap.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
			}
			List<ShipInfo> list5 = this._db.GetShipInfos(true).ToList<ShipInfo>();
			foreach (ShipInfo current3 in list5)
			{
				ShipSectionAsset commandSectionAsset = current3.DesignInfo.GetCommandSectionAsset(this.AssetDatabase);
				if (current3.DesignInfo.PlayerID == poorSap.ID && commandSectionAsset != null && commandSectionAsset.IsFireControl)
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(current3.FleetID);
					List<FleetInfo> list6 = this.App.GameDatabase.GetFleetInfoBySystemID(fleetInfo.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
					int? num3 = null;
					foreach (FleetInfo current4 in list6)
					{
						MissionInfo missionByFleetID = this.App.GameDatabase.GetMissionByFleetID(current4.ID);
						if (missionByFleetID != null && missionByFleetID.Type == MissionType.RETREAT && current4.PlayerID == lOAPlayerForRebellion)
						{
							num3 = new int?(current4.ID);
							break;
						}
					}
					if (!num3.HasValue)
					{
						num3 = new int?(this.App.GameDatabase.InsertFleet(fleetInfo.PlayerID, 0, fleetInfo.SystemID, 0, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL));
						int missionID = this.App.GameDatabase.InsertMission(num3.Value, MissionType.RETREAT, 0, 0, 0, 0, false, null);
						this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, null);
						this.App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, null);
					}
					this._db.TransferShipToPlayer(current3, lOAPlayerForRebellion);
					this._db.TransferShip(current3.ID, num3.Value);
				}
			}
		}
		public void CheckAIRebellions(GameDatabase db)
		{
			List<PlayerInfo> list = db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				bool flag = false;
				int playerResearchingTechID = db.GetPlayerResearchingTechID(current.ID);
				if (playerResearchingTechID != 0 || flag)
				{
					string techFileId = db.GetTechFileID(playerResearchingTechID);
					Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techFileId);
					if (((tech != null && tech.AllowAIRebellion && this._db.GetStratModifier<bool>(StratModifiers.AllowAIRebellion, current.ID)) || flag) && (this._random.CoinToss((double)this.AssetDatabase.AIRebellionChance) || flag))
					{
						this.DoAIRebellion(current, false);
					}
				}
			}
		}
		public void UpdateDemands(GameDatabase db)
		{
			List<DemandInfo> list = (
				from x in db.GetDemandInfos().ToList<DemandInfo>()
				where x.State == AgreementState.Unrequested
				select x).ToList<DemandInfo>();
			foreach (DemandInfo current in list)
			{
				PlayerInfo playerInfo = db.GetPlayerInfo(current.ReceivingPlayer);
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerInfo.ID);
				if (playerObject.IsAI())
				{
					if (playerObject.GetAI().HandleDemandRequest(current))
					{
						this.AcceptDemand(current);
					}
					else
					{
						this.DeclineDemand(current);
					}
				}
				else
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_DEMAND_REQUESTED,
						EventMessage = TurnEventMessage.EM_DEMAND_REQUESTED,
						PlayerID = current.ReceivingPlayer,
						TargetPlayerID = current.InitiatingPlayer,
						TreatyID = current.ID,
						TurnNumber = this._app.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				}
			}
		}
		public void AcceptRequest(RequestInfo ri)
		{
			this._db.SetRequestState(AgreementState.Accepted, ri.ID);
			PlayerInfo playerInfo = this._db.GetPlayerInfo(ri.ReceivingPlayer);
			PlayerInfo playerInfo2 = this._db.GetPlayerInfo(ri.InitiatingPlayer);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_REQUEST_ACCEPTED,
				EventMessage = TurnEventMessage.EM_REQUEST_ACCEPTED,
				PlayerID = ri.InitiatingPlayer,
				TargetPlayerID = ri.ReceivingPlayer,
				TreatyID = ri.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
			switch (ri.Type)
			{
			case RequestType.SavingsRequest:
				this._db.UpdatePlayerSavings(playerInfo2.ID, playerInfo2.Savings + (double)ri.RequestValue);
				this._db.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings - (double)ri.RequestValue);
				this.App.GameDatabase.ApplyDiplomacyReaction(ri.InitiatingPlayer, ri.ReceivingPlayer, StratModifiers.DiplomacyReactionMoney, (int)Math.Min(ri.RequestValue / 200000f, 10f));
				return;
			case RequestType.SystemInfoRequest:
			{
				int num = (int)ri.RequestValue;
				if (num != 0)
				{
					this._db.UpdatePlayerViewWithStarSystem(ri.InitiatingPlayer, num);
					int turnCount = this._db.GetTurnCount();
					this._db.InsertExploreRecord(num, ri.InitiatingPlayer, turnCount, true, true);
				}
				break;
			}
			case RequestType.ResearchPointsRequest:
				this._db.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings - (double)ri.RequestValue);
				this._db.UpdatePlayerAdditionalResearchPoints(ri.InitiatingPlayer, playerInfo2.AdditionalResearchPoints + this.ConvertToResearchPoints(playerInfo.ID, (double)ri.RequestValue));
				this.App.GameDatabase.ApplyDiplomacyReaction(ri.InitiatingPlayer, ri.ReceivingPlayer, StratModifiers.DiplomacyReactionResearch, (int)Math.Min(ri.RequestValue / 5000f, 10f));
				return;
			case RequestType.MilitaryAssistanceRequest:
			case RequestType.GatePermissionRequest:
				break;
			case RequestType.EstablishEnclaveRequest:
				this._app.GameDatabase.InsertGovernmentAction(ri.ReceivingPlayer, App.Localize("@GA_ENCLAVEACCEPTED"), "EnclaveAccepted", 0, 0);
				return;
			default:
				return;
			}
		}
        public void AcceptDemand(DemandInfo di)
        {
            Func<ColonyFactionInfo, bool> predicate = null;
            this._db.SetDemandState(AgreementState.Accepted, di.ID);
            PlayerInfo rplayer = this._db.GetPlayerInfo(di.ReceivingPlayer);
            PlayerInfo playerInfo = this._db.GetPlayerInfo(di.InitiatingPlayer);
            TurnEvent ev = new TurnEvent {
                EventType = TurnEventType.EV_DEMAND_ACCEPTED,
                EventMessage = TurnEventMessage.EM_DEMAND_ACCEPTED,
                PlayerID = di.InitiatingPlayer,
                TargetPlayerID = di.ReceivingPlayer,
                TreatyID = di.ID,
                TurnNumber = this._app.GameDatabase.GetTurnCount(),
                ShowsDialog = true
            };
            this._app.GameDatabase.InsertTurnEvent(ev);
            ProvinceInfo pi;
            switch (di.Type)
            {
                case DemandType.SavingsDemand:
                    this._db.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings + di.DemandValue);
                    this._db.UpdatePlayerSavings(rplayer.ID, rplayer.Savings - di.DemandValue);
                    this.App.GameDatabase.ApplyDiplomacyReaction(di.InitiatingPlayer, di.ReceivingPlayer, StratModifiers.DiplomacyReactionMoney, (int) Math.Min((float) (di.DemandValue / 200000f), (float) 10f));
                    return;

                case DemandType.SystemInfoDemand:
                {
                    int demandValue = (int) di.DemandValue;
                    if (demandValue == 0)
                    {
                        break;
                    }
                    this._db.UpdatePlayerViewWithStarSystem(di.InitiatingPlayer, demandValue);
                    int turnCount = this._db.GetTurnCount();
                    this._db.InsertExploreRecord(demandValue, di.InitiatingPlayer, turnCount, true, true);
                    return;
                }
                case DemandType.ResearchPointsDemand:
                    this._db.UpdatePlayerSavings(rplayer.ID, rplayer.Savings - di.DemandValue);
                    this._db.UpdatePlayerAdditionalResearchPoints(di.InitiatingPlayer, playerInfo.AdditionalResearchPoints + this.ConvertToResearchPoints(rplayer.ID, (double) di.DemandValue));
                    this.App.GameDatabase.ApplyDiplomacyReaction(di.InitiatingPlayer, di.ReceivingPlayer, StratModifiers.DiplomacyReactionResearch, (int) Math.Min((float) (di.DemandValue / 5000f), (float) 10f));
                    return;

                case DemandType.SlavesDemand:
                {
                    HomeworldInfo playerHomeworld = this._db.GetPlayerHomeworld(playerInfo.ID);
                    HomeworldInfo info5 = this._db.GetPlayerHomeworld(rplayer.ID);
                    ColonyInfo colonyInfo = this._db.GetColonyInfo(playerHomeworld.ColonyID);
                    ColonyInfo colony = this._db.GetColonyInfo(info5.ColonyID);
                    colony.ImperialPop -= di.DemandValue;
                    if (predicate == null)
                    {
                        predicate = x => x.FactionID == rplayer.FactionID;
                    }
                    ColonyFactionInfo civPop = colonyInfo.Factions.FirstOrDefault<ColonyFactionInfo>(predicate);
                    if (civPop != null)
                    {
                        civPop.CivilianPop += di.DemandValue;
                        this._db.UpdateCivilianPopulation(civPop);
                    }
                    else
                    {
                        this._db.InsertColonyFaction(colonyInfo.OrbitalObjectID, rplayer.FactionID, (double) di.DemandValue, 0.5f, this._db.GetTurnCount());
                    }
                    this.App.GameDatabase.ApplyDiplomacyReaction(di.InitiatingPlayer, di.ReceivingPlayer, StratModifiers.DiplomacyReactionSlave, 1);
                    this._db.UpdateColony(colony);
                    return;
                }
                case DemandType.WorldDemand:
                {
                    StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo((int) di.DemandValue);
                    if (starSystemInfo.ProvinceID.HasValue)
                    {
                        this._db.RemoveProvince(starSystemInfo.ProvinceID.Value);
                    }
                    foreach (ColonyInfo info13 in this._db.GetColonyInfosForSystem((int) di.DemandValue).ToList<ColonyInfo>())
                    {
                        info13.PlayerID = di.InitiatingPlayer;
                        info13.ImperialPop = 100.0;
                        this._db.UpdateColony(info13);
                    }
                    TurnEvent event6 = new TurnEvent {
                        EventType = TurnEventType.EV_SYSTEM_SURRENDERED_TO_YOU,
                        EventMessage = TurnEventMessage.EM_SYSTEM_SURRENDERED_TO_YOU,
                        PlayerID = di.InitiatingPlayer,
                        TargetPlayerID = di.ReceivingPlayer,
                        SystemID = starSystemInfo.ID,
                        TurnNumber = this._db.GetTurnCount(),
                        ShowsDialog = false
                    };
                    this._db.InsertTurnEvent(event6);
                    break;
                }
                case DemandType.ProvinceDemand:
                {
                    int provinceId = (int) di.DemandValue;
                    pi = this._db.GetProvinceInfo(provinceId);
                    pi.PlayerID = di.InitiatingPlayer;
                    this._db.UpdateProvinceInfo(pi);
                    List<StarSystemInfo> list2 = (
                        from x in this._db.GetStarSystemInfos().ToList<StarSystemInfo>()
                        where x.ProvinceID == pi.ID
                        select x).ToList<StarSystemInfo>();
                    foreach (StarSystemInfo info2 in list2)
                    {
                        foreach (ColonyInfo info3 in this._db.GetColonyInfosForSystem(info2.ID).ToList<ColonyInfo>())
                        {
                            info3.PlayerID = di.InitiatingPlayer;
                            info3.ImperialPop = 100.0;
                            this._db.UpdateColony(info3);
                        }
                    }
                    TurnEvent event2 = new TurnEvent {
                        EventType = TurnEventType.EV_PROVINCE_SURRENDERED_TO_YOU,
                        EventMessage = TurnEventMessage.EM_PROVINCE_SURRENDERED_TO_YOU,
                        PlayerID = di.InitiatingPlayer,
                        TargetPlayerID = di.ReceivingPlayer,
                        ProvinceID = provinceId,
                        TurnNumber = this._db.GetTurnCount(),
                        ShowsDialog = false
                    };
                    this._db.InsertTurnEvent(event2);
                    return;
                }
                case DemandType.SurrenderDemand:
                {
                    foreach (ColonyInfo info9 in this._db.GetPlayerColoniesByPlayerId(di.ReceivingPlayer).ToList<ColonyInfo>())
                    {
                        info9.PlayerID = di.InitiatingPlayer;
                        info9.ImperialPop = 100.0;
                        this._db.UpdateColony(info9);
                    }
                    foreach (FleetInfo info10 in this._db.GetFleetInfosByPlayerID(di.ReceivingPlayer, FleetType.FL_ALL).ToList<FleetInfo>())
                    {
                        this._db.RemoveFleet(info10.ID);
                    }
                    foreach (StationInfo info11 in this._db.GetStationInfosByPlayerID(di.ReceivingPlayer).ToList<StationInfo>())
                    {
                        this._db.DestroyStation(this, info11.OrbitalObjectID, 0);
                    }
                    this._db.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings + rplayer.Savings);
                    this._db.UpdatePlayerSavings(rplayer.ID, 0.0);
                    float stratModifier = this._db.GetStratModifier<float>(StratModifiers.ResearchModifier, playerInfo.ID);
                    this._db.SetStratModifier(StratModifiers.ResearchModifier, playerInfo.ID, stratModifier + 0.05f);
                    TurnEvent event3 = new TurnEvent {
                        EventType = TurnEventType.EV_PLAYER_SURRENDERED_TO_YOU,
                        EventMessage = TurnEventMessage.EM_PLAYER_SURRENDERED_TO_YOU,
                        PlayerID = di.InitiatingPlayer,
                        TargetPlayerID = di.ReceivingPlayer,
                        TurnNumber = this._db.GetTurnCount(),
                        ShowsDialog = false
                    };
                    this._db.InsertTurnEvent(event3);
                    TurnEvent event4 = new TurnEvent {
                        EventType = TurnEventType.EV_YOU_SURRENDER,
                        EventMessage = TurnEventMessage.EM_YOU_SURRENDER,
                        PlayerID = di.ReceivingPlayer,
                        TargetPlayerID = di.InitiatingPlayer,
                        TurnNumber = this._db.GetTurnCount(),
                        ShowsDialog = false
                    };
                    this._db.InsertTurnEvent(event4);
                    TurnEvent event5 = new TurnEvent {
                        EventType = TurnEventType.EV_SAVINGS_SURRENDERED_TO_YOU,
                        EventMessage = TurnEventMessage.EM_SAVINGS_SURRENDERED_TO_YOU,
                        PlayerID = di.InitiatingPlayer,
                        TargetPlayerID = di.ReceivingPlayer,
                        Savings = rplayer.Savings,
                        TurnNumber = this._db.GetTurnCount(),
                        ShowsDialog = false
                    };
                    this._db.InsertTurnEvent(event5);
                    return;
                }
                default:
                    return;
            }
        }
        public void DeclineRequest(RequestInfo ri)
		{
			this._db.SetRequestState(AgreementState.Rejected, ri.ID);
			this._db.GetPlayerInfo(ri.ReceivingPlayer);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_REQUEST_DECLINED,
				EventMessage = TurnEventMessage.EM_REQUEST_DECLINED,
				PlayerID = ri.InitiatingPlayer,
				TargetPlayerID = ri.ReceivingPlayer,
				TreatyID = ri.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}
		public void DeclineDemand(DemandInfo di)
		{
			this._db.SetDemandState(AgreementState.Rejected, di.ID);
			this._db.GetPlayerInfo(di.ReceivingPlayer);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_DEMAND_DECLINED,
				EventMessage = TurnEventMessage.EM_DEMAND_DECLINED,
				PlayerID = di.InitiatingPlayer,
				TargetPlayerID = di.ReceivingPlayer,
				TreatyID = di.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}
		public void UpdateTreaties(GameDatabase db)
		{
			List<TreatyInfo> source = db.GetTreatyInfos().ToList<TreatyInfo>();
			foreach (TreatyInfo current in 
				from x in source
				where x.Removed
				select x)
			{
				db.DeleteTreatyInfo(current.ID);
			}
			foreach (TreatyInfo current2 in 
				from x in source
				where !x.Removed
				select x)
			{
				if (current2.Active && !current2.Removed)
				{
					int num;
					if (current2.Type == TreatyType.Limitation && !GameSession.CheckLimitationTreaty(db, (LimitationTreatyInfo)current2, out num))
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_TREATY_BROKEN_OFFENDER,
							EventMessage = TurnEventMessage.EM_TREATY_BROKEN_OFFENDER,
							PlayerID = num,
							TreatyID = current2.ID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_TREATY_BROKEN_VICTIM,
							EventMessage = TurnEventMessage.EM_TREATY_BROKEN_VICTIM,
							PlayerID = (num == current2.InitiatingPlayerId) ? current2.ReceivingPlayerId : current2.InitiatingPlayerId,
							TreatyID = current2.ID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						GameSession.ApplyConsequences(db, num, (current2.InitiatingPlayerId == num) ? current2.ReceivingPlayerId : current2.InitiatingPlayerId, current2.Consequences);
						current2.Removed = true;
						this._app.GameDatabase.RemoveTreatyInfo(current2.ID);
					}
					if (current2.StartingTurn + current2.Duration < db.GetTurnCount() && current2.Type != TreatyType.Protectorate && !current2.Removed && current2.Duration != 0)
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_TREATY_EXPIRED,
							EventMessage = TurnEventMessage.EM_TREATY_EXPIRED,
							PlayerID = current2.InitiatingPlayerId,
							TreatyID = current2.ID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_TREATY_EXPIRED,
							EventMessage = TurnEventMessage.EM_TREATY_EXPIRED,
							PlayerID = current2.ReceivingPlayerId,
							TreatyID = current2.ID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						db.RemoveTreatyInfo(current2.ID);
					}
				}
				else
				{
					if (!current2.Active && !current2.Removed)
					{
						PlayerInfo playerInfo = db.GetPlayerInfo(current2.ReceivingPlayerId);
						Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerInfo.ID);
						if (playerObject.IsAI())
						{
							if (playerObject.GetAI().HandleTreatyOffer(current2))
							{
								this.AcceptTreaty(current2);
							}
							else
							{
								this.DeclineTreaty(current2);
							}
						}
						else
						{
							db.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_TREATY_REQUESTED,
								EventMessage = TurnEventMessage.EM_TREATY_REQUESTED,
								PlayerID = current2.ReceivingPlayerId,
								TreatyID = current2.ID,
								TurnNumber = db.GetTurnCount(),
								ShowsDialog = true
							});
						}
					}
				}
			}
		}
		public void AcceptIncentive(int sender, int receiver, TreatyIncentiveInfo tii)
		{
			PlayerInfo playerInfo = this._db.GetPlayerInfo(sender);
			PlayerInfo playerInfo2 = this._db.GetPlayerInfo(receiver);
			switch (tii.Type)
			{
			case IncentiveType.ResearchPoints:
				this._db.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings - (double)tii.IncentiveValue);
				this._db.UpdatePlayerAdditionalResearchPoints(sender, playerInfo2.AdditionalResearchPoints + this.ConvertToResearchPoints(playerInfo.ID, (double)tii.IncentiveValue));
				this.App.GameDatabase.ApplyDiplomacyReaction(sender, receiver, StratModifiers.DiplomacyReactionResearch, (int)Math.Min(tii.IncentiveValue / 5000f, 10f));
				return;
			case IncentiveType.Savings:
				this._db.UpdatePlayerSavings(playerInfo2.ID, playerInfo2.Savings + (double)tii.IncentiveValue);
				this._db.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings - (double)tii.IncentiveValue);
				this.App.GameDatabase.ApplyDiplomacyReaction(sender, receiver, StratModifiers.DiplomacyReactionMoney, (int)Math.Min(tii.IncentiveValue / 200000f, 10f));
				return;
			default:
				return;
			}
		}
		public void AcceptTreaty(TreatyInfo _treatyInfo)
		{
			foreach (TreatyIncentiveInfo current in _treatyInfo.Incentives)
			{
				this.AcceptIncentive(_treatyInfo.InitiatingPlayerId, _treatyInfo.ReceivingPlayerId, current);
			}
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_TREATY_ACCEPTED,
				EventMessage = TurnEventMessage.EM_TREATY_ACCEPTED,
				PlayerID = _treatyInfo.InitiatingPlayerId,
				TargetPlayerID = _treatyInfo.ReceivingPlayerId,
				TreatyID = _treatyInfo.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
			if (_treatyInfo.Type == TreatyType.Armistice)
			{
				ArmisticeTreatyInfo armisticeTreatyInfo = (ArmisticeTreatyInfo)_treatyInfo;
				this.GameDatabase.ChangeDiplomacyState(armisticeTreatyInfo.InitiatingPlayerId, armisticeTreatyInfo.ReceivingPlayerId, armisticeTreatyInfo.SuggestedDiplomacyState);
				this.GameDatabase.RemoveTreatyInfo(armisticeTreatyInfo.ID);
				if (armisticeTreatyInfo.SuggestedDiplomacyState != DiplomacyState.PEACE && armisticeTreatyInfo.SuggestedDiplomacyState != DiplomacyState.ALLIED)
				{
					goto IL_38B;
				}
				IEnumerable<int> standardPlayerIDs = this.GameDatabase.GetStandardPlayerIDs();
				using (IEnumerator<int> enumerator2 = standardPlayerIDs.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						if (this.App.GameDatabase.GetDiplomacyInfo(armisticeTreatyInfo.InitiatingPlayerId, current2).isEncountered)
						{
							this.App.GameDatabase.ApplyDiplomacyReaction(armisticeTreatyInfo.InitiatingPlayerId, current2, StratModifiers.DiplomacyReactionPeaceTreaty, 1);
						}
					}
					goto IL_38B;
				}
			}
			if (_treatyInfo.Type == TreatyType.Incorporate)
			{
				List<ColonyInfo> list = this.GameDatabase.GetPlayerColoniesByPlayerId(_treatyInfo.ReceivingPlayerId).ToList<ColonyInfo>();
				foreach (ColonyInfo current3 in list)
				{
					current3.PlayerID = _treatyInfo.InitiatingPlayerId;
					this.GameDatabase.UpdateColony(current3);
					OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(current3.OrbitalObjectID);
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INDY_ASSIMILATED,
						EventMessage = TurnEventMessage.EM_INDY_ASSIMILATED,
						PlayerID = _treatyInfo.InitiatingPlayerId,
						TargetPlayerID = _treatyInfo.ReceivingPlayerId,
						SystemID = orbitalObjectInfo.StarSystemID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				this.GameDatabase.SetPlayerDefeated(this, _treatyInfo.ReceivingPlayerId);
				this._app.GameDatabase.InsertGovernmentAction(_treatyInfo.InitiatingPlayerId, App.Localize("@GA_INDEPENDANTASSIMILATED"), "IndependantAssimilated", 0, 0);
			}
			else
			{
				if (_treatyInfo.Type == TreatyType.Protectorate)
				{
					List<ColonyInfo> list2 = this.GameDatabase.GetPlayerColoniesByPlayerId(_treatyInfo.ReceivingPlayerId).ToList<ColonyInfo>();
					foreach (ColonyInfo current4 in list2)
					{
						OrbitalObjectInfo orbitalObjectInfo2 = this.GameDatabase.GetOrbitalObjectInfo(current4.OrbitalObjectID);
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_INDY_PROTECTORATE,
							EventMessage = TurnEventMessage.EM_INDY_PROTECTORATE,
							PlayerID = _treatyInfo.InitiatingPlayerId,
							TargetPlayerID = _treatyInfo.ReceivingPlayerId,
							SystemID = orbitalObjectInfo2.StarSystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
			IL_38B:
			this._app.GameDatabase.SetTreatyActive(_treatyInfo.ID);
		}
		public void DeclineTreaty(TreatyInfo _treatyInfo)
		{
			this._app.GameDatabase.RemoveTreatyInfo(_treatyInfo.ID);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_TREATY_DECLINED,
				EventMessage = TurnEventMessage.EM_TREATY_DECLINED,
				PlayerID = _treatyInfo.InitiatingPlayerId,
				TargetPlayerID = _treatyInfo.ReceivingPlayerId,
				TreatyID = _treatyInfo.ID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = true
			});
		}
		public static bool CanRemoveShipFromDefenseFleet(DesignInfo di)
		{
			if (di == null)
			{
				return true;
			}
			bool result = true;
			foreach (ShipSectionAsset current in (
				from x in di.DesignSections
				select x.ShipSectionAsset).ToList<ShipSectionAsset>())
			{
				if (current.CombatAIType == SectionEnumerations.CombatAiType.CommandMonitor || current.CombatAIType == SectionEnumerations.CombatAiType.NormalMonitor)
				{
					result = false;
					break;
				}
			}
			return result;
		}
		public static void UpdatePlayerFleets(GameDatabase db, GameSession game)
		{
			List<PlayerInfo> list = db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				List<FleetInfo> list2 = db.GetFleetInfosByPlayerID(current.ID, FleetType.FL_DEFENSE).ToList<FleetInfo>();
				foreach (FleetInfo current2 in list2)
				{
					if (!GameSession.CanPlayerSupportDefenseAssets(db, current.ID, current2.SystemID))
					{
						List<ShipInfo> list3 = db.GetShipInfoByFleetID(current2.ID, true).ToList<ShipInfo>();
						List<ShipInfo> list4 = new List<ShipInfo>();
						list4.AddRange(list3);
						foreach (ShipInfo current3 in list4)
						{
							if (GameSession.CanRemoveShipFromDefenseFleet(current3.DesignInfo))
							{
								db.RemoveShip(current3.ID);
								list3.Remove(current3);
							}
						}
						if (list3.Count == 0)
						{
							db.RemoveFleet(current2.ID);
						}
					}
				}
				IEnumerable<int> enumerable = db.GetPlayerColonySystemIDs(current.ID).ToList<int>();
				List<FleetInfo> source = list2.ToList<FleetInfo>();
				foreach (int system in enumerable)
				{
					if ((
						from x in source
						where x.SystemID == system
						select x).Count<FleetInfo>() == 0)
					{
						db.InsertDefenseFleet(current.ID, system);
					}
				}
				List<FleetInfo> list5 = db.GetFleetInfosByPlayerID(current.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				foreach (FleetInfo fleet in list5)
				{
					PlayerInfo playerInfo = list.FirstOrDefault((PlayerInfo x) => x.ID == fleet.PlayerID);
					if (playerInfo != null && db.AssetDatabase.GetFaction(playerInfo.FactionID).Name == "loa")
					{
						MissionInfo missionByFleetID = db.GetMissionByFleetID(fleet.ID);
						if (missionByFleetID != null && (missionByFleetID.Type == MissionType.RETURN || missionByFleetID.Type == MissionType.RETREAT))
						{
							continue;
						}
						LoaFleetComposition loaFleetComposition = Kerberos.Sots.StarFleet.StarFleet.ObtainFleetComposition(game, fleet, fleet.FleetConfigID);
						if (loaFleetComposition == null || loaFleetComposition.designs.Count == 0)
						{
							continue;
						}
						MissionType mission_type = MissionType.NO_MISSION;
						if (missionByFleetID != null)
						{
							mission_type = missionByFleetID.Type;
						}
                        List<DesignInfo> source2 = Kerberos.Sots.StarFleet.StarFleet.GetDesignBuildOrderForComposition(game, fleet.ID, loaFleetComposition, mission_type).ToList<DesignInfo>();
                        int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(game, fleet.ID);
						DesignInfo designInfo = source2.FirstOrDefault((DesignInfo x) => x.GetCommandPoints() > 0);
						if (designInfo == null || fleetLoaCubeValue < designInfo.GetPlayerProductionCost(db, fleet.PlayerID, !designInfo.isPrototyped, null))
						{
                            Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, fleet, true);
							missionByFleetID = db.GetMissionByFleetID(fleet.ID);
							if (missionByFleetID != null)
							{
								game.GameDatabase.InsertWaypoint(missionByFleetID.ID, WaypointType.DisbandFleet, null);
							}
							else
							{
								if (fleet.SystemID != 0)
								{
									int? reserveFleetID = db.GetReserveFleetID(fleet.PlayerID, fleet.SystemID);
									if (reserveFleetID.HasValue)
									{
										List<ShipInfo> list6 = db.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
										foreach (ShipInfo current4 in list6)
										{
											db.UpdateShipAIFleetID(current4.ID, null);
											db.TransferShip(current4.ID, reserveFleetID.Value);
										}
										db.RemoveFleet(fleet.ID);
									}
									else
									{
										int num = db.FindNewHomeSystem(fleet);
										if (num != 0)
										{
											int missionID = db.InsertMission(fleet.ID, MissionType.RELOCATION, num, 0, 0, 1, false, null);
											db.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(num));
											db.InsertWaypoint(missionID, WaypointType.DisbandFleet, null);
										}
										else
										{
											db.RemoveFleet(fleet.ID);
										}
									}
								}
							}
						}
					}
					if (db.GetAdmiralTraits(fleet.AdmiralID).Contains(AdmiralInfo.TraitType.Sherman))
					{
						foreach (PlayerInfo current5 in list)
						{
							if (current5 != current)
							{
								db.ApplyDiplomacyReaction(current.ID, current5.ID, -2, null, 1);
							}
						}
					}
				}
			}
		}
		public bool FleetAtSystem(FleetInfo f, int targetSystem)
		{
			return f.SystemID == targetSystem;
		}
		public int GetArrivalTurns(MoveOrderInfo moi, int fleetID)
		{
			FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(fleetID);
			Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(fleetInfo.PlayerID);
			if (playerObject.Faction.CanUseGate() && fleetInfo.SystemID != 0 && this.GameDatabase.SystemHasGate(moi.ToSystemID, playerObject.ID))
			{
				return 1;
			}
			bool nodeTravel = false;
			if (playerObject.Faction.CanUseNodeLine(null) && !playerObject.Faction.CanUseAccelerators())
			{
				if (playerObject.Faction.CanUseNodeLine(new bool?(false)))
				{
					nodeTravel = (this.GameDatabase.GetNodeLineBetweenSystems(playerObject.ID, moi.FromSystemID, moi.ToSystemID, false, false) != null);
				}
				else
				{
					if (playerObject.Faction.CanUseNodeLine(new bool?(true)))
					{
						nodeTravel = (this.GameDatabase.GetNodeLineBetweenSystems(playerObject.ID, moi.FromSystemID, moi.ToSystemID, true, false) != null);
					}
				}
			}
			FleetLocation fleetLocation = this.GameDatabase.GetFleetLocation(fleetID, false);
            float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, moi.FleetID, nodeTravel);
			Vector3 coords = fleetLocation.Coords;
			Vector3 v = (moi.ToSystemID != 0) ? this.GameDatabase.GetStarSystemOrigin(moi.ToSystemID) : moi.ToCoords;
			return (int)Math.Ceiling((double)((coords - v).Length / fleetTravelSpeed));
		}
		public void ProcessWaypoint(WaypointInfo wi, FleetInfo fi, bool useDirectRoute)
		{
			bool flag = false;
			if (wi == null)
			{
				return;
			}
			if (fi.Type == FleetType.FL_ACCELERATOR)
			{
				return;
			}
			Faction faction = this._app.AssetDatabase.GetFaction(this.GameDatabase.GetPlayerInfo(fi.PlayerID).FactionID);
			if (faction.CanUseAccelerators())
			{
				if (fi.SystemID != 0)
				{
					if (this._app.GameDatabase.GetFleetsByPlayerAndSystem(fi.PlayerID, fi.SystemID, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
					{
						this._app.GameDatabase.UpdateFleetAccelerated(this, fi.ID, null);
						fi.LastTurnAccelerated = this._app.GameDatabase.GetTurnCount();
					}
				}
				else
				{
					if (fi.SystemID == 0 && this._app.GameDatabase.IsInAccelRange(fi.ID))
					{
						this._app.GameDatabase.UpdateFleetAccelerated(this, fi.ID, null);
						fi.LastTurnAccelerated = this._app.GameDatabase.GetTurnCount();
					}
				}
			}
			MissionInfo missionByFleetID;
			switch (wi.Type)
			{
			case WaypointType.TravelTo:
			case WaypointType.ReturnHome:
			{
				missionByFleetID = this._db.GetMissionByFleetID(fi.ID);
				if (fi.Type != FleetType.FL_CARAVAN && missionByFleetID != null && missionByFleetID.Type == MissionType.RELOCATION && this.GameDatabase.GetRemainingSupportPoints(this, missionByFleetID.TargetSystemID, fi.PlayerID) < 0)
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fi, true);
					flag = true;
					goto IL_C12;
				}
				flag = this.MoveFleet(wi, fi, useDirectRoute);
				if (flag && this.App.Game.ScriptModules != null && this.App.Game.ScriptModules.Swarmers != null && this.App.Game.ScriptModules.Swarmers.PlayerID == fi.PlayerID)
				{
					SwarmerInfo swarmerInfo = this.App.GameDatabase.GetSwarmerInfos().FirstOrDefault((SwarmerInfo x) => x.QueenFleetId.HasValue && x.QueenFleetId.Value == fi.ID);
					if (swarmerInfo != null)
					{
						Swarmers.SetInitialSwarmerPosition(this, swarmerInfo, fi.SystemID);
					}
				}
				else
				{
					if (this._app.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fi.PlayerID).FactionID).Name == "loa")
					{
						if (!flag)
						{
							if (!fi.FleetConfigID.HasValue)
							{
								this._db.SaveCurrentFleetCompositionToFleet(fi.ID);
							}
                            Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, fi.ID);
						}
						else
						{
							if (wi.Type == WaypointType.ReturnHome && flag)
							{
                                Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, fi.ID, MissionType.NO_MISSION);
							}
						}
					}
				}
				MoveOrderInfo moveOrderInfoByFleetID = this._db.GetMoveOrderInfoByFleetID(fi.ID);
				if (moveOrderInfoByFleetID == null)
				{
					goto IL_C12;
				}
				List<int> list = new List<int>();
				List<ColonyInfo> list2 = this._db.GetColonyInfosForSystem(moveOrderInfoByFleetID.ToSystemID).ToList<ColonyInfo>();
				using (List<ColonyInfo>.Enumerator enumerator = list2.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ColonyInfo current = enumerator.Current;
						DiplomacyState diplomacyStateBetweenPlayers = this._db.GetDiplomacyStateBetweenPlayers(current.PlayerID, fi.PlayerID);
						if (current.PlayerID != fi.PlayerID && !list.Contains(current.PlayerID) && this._db.GetPlayerInfo(current.PlayerID).isStandardPlayer && diplomacyStateBetweenPlayers != DiplomacyState.NON_AGGRESSION && diplomacyStateBetweenPlayers != DiplomacyState.PEACE && diplomacyStateBetweenPlayers != DiplomacyState.ALLIED && StarMap.IsInRange(this._db, current.PlayerID, this._db.GetFleetLocation(fi.ID, false).Coords, 1f, null))
						{
							list.Add(current.PlayerID);
							if (fi.PlayerID == this.App.Game.ScriptModules.Locust.PlayerID)
							{
								this._db.InsertTurnEvent(new TurnEvent
								{
									EventType = TurnEventType.EV_LOCUST_INCOMING,
									EventMessage = TurnEventMessage.EM_LOCUST_INCOMING,
									PlayerID = current.PlayerID,
									SystemID = moveOrderInfoByFleetID.ToSystemID,
									ArrivalTurns = this.GetArrivalTurns(moveOrderInfoByFleetID, fi.ID),
									TurnNumber = this._db.GetTurnCount()
								});
							}
							else
							{
								if (fi.PlayerID == this.App.Game.ScriptModules.Swarmers.PlayerID)
								{
									this._db.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_SWARM_QUEEN_INCOMING,
										EventMessage = TurnEventMessage.EM_SWARM_QUEEN_INCOMING,
										PlayerID = current.PlayerID,
										SystemID = moveOrderInfoByFleetID.ToSystemID,
										ArrivalTurns = this.GetArrivalTurns(moveOrderInfoByFleetID, fi.ID),
										TurnNumber = this._db.GetTurnCount()
									});
								}
								else
								{
									if (fi.PlayerID == this.App.Game.ScriptModules.SystemKiller.PlayerID)
									{
										this._db.InsertTurnEvent(new TurnEvent
										{
											EventType = TurnEventType.EV_SYS_KILLER_INCOMING,
											EventMessage = TurnEventMessage.EM_SYS_KILLER_INCOMING,
											PlayerID = current.PlayerID,
											SystemID = moveOrderInfoByFleetID.ToSystemID,
											ArrivalTurns = this.GetArrivalTurns(moveOrderInfoByFleetID, fi.ID),
											TurnNumber = this._db.GetTurnCount()
										});
									}
									else
									{
										this._db.InsertTurnEvent(new TurnEvent
										{
											EventType = TurnEventType.EV_INCOMING_ALIEN_FLEET,
											EventMessage = TurnEventMessage.EM_INCOMING_ALIEN_FLEET,
											PlayerID = current.PlayerID,
											SystemID = moveOrderInfoByFleetID.ToSystemID,
											FleetID = fi.ID,
											TargetPlayerID = fi.PlayerID,
											ArrivalTurns = this.GetArrivalTurns(moveOrderInfoByFleetID, fi.ID),
											TurnNumber = this._db.GetTurnCount()
										});
									}
								}
							}
						}
					}
					goto IL_C12;
				}
				break;
			}
			case WaypointType.DoMission:
				break;
			case WaypointType.DisbandFleet:
                if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this._db, fi))
				{
                    ShipInfo fleetSuulkaShipInfo = Kerberos.Sots.StarFleet.StarFleet.GetFleetSuulkaShipInfo(this._db, fi);
					SuulkaInfo suulkaByShipID = this._db.GetSuulkaByShipID(fleetSuulkaShipInfo.ID);
					if (suulkaByShipID != null)
					{
						fi.AdmiralID = suulkaByShipID.AdmiralID;
						fi.Name = fleetSuulkaShipInfo.DesignInfo.Name;
						this._db.UpdateFleetInfo(fi);
						flag = true;
					}
					else
					{
						this._db.RemoveShip(fleetSuulkaShipInfo.ID);
					}
				}
				if (!flag)
				{
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_FLEET_DISBANDED,
						EventMessage = TurnEventMessage.EM_FLEET_DISBANDED,
						PlayerID = fi.PlayerID,
						FleetID = fi.ID,
						SystemID = fi.SystemID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					int? reserveFleetID = this._db.GetReserveFleetID(fi.PlayerID, fi.SystemID);
					if (reserveFleetID.HasValue)
					{
						List<ShipInfo> list3 = this._db.GetShipInfoByFleetID(fi.ID, false).ToList<ShipInfo>();
						foreach (ShipInfo current2 in list3)
						{
							this._db.UpdateShipAIFleetID(current2.ID, null);
							this._db.TransferShip(current2.ID, reserveFleetID.Value);
						}
						flag = true;
					}
					this._db.RemoveFleet(fi.ID);
					goto IL_C12;
				}
				goto IL_C12;
			case WaypointType.CheckSupportColony:
				missionByFleetID = this._db.GetMissionByFleetID(fi.ID);
				if (missionByFleetID.Type != MissionType.SUPPORT)
				{
					missionByFleetID.Type = MissionType.SUPPORT;
					this._db.UpdateMission(missionByFleetID);
				}
				this.CompleteColonizationMission(missionByFleetID, fi);
				flag = true;
				goto IL_C12;
			case WaypointType.CheckEvacuate:
				missionByFleetID = this._db.GetMissionByFleetID(fi.ID);
				if (missionByFleetID.Type != MissionType.EVACUATE)
				{
					missionByFleetID.Type = MissionType.EVACUATE;
					this._db.UpdateMission(missionByFleetID);
				}
				this.CompleteEvecuateMission(missionByFleetID, fi);
				flag = true;
				goto IL_C12;
			case WaypointType.Intercepted:
				flag = true;
				goto IL_C12;
			case WaypointType.ReBase:
				flag = this.DoRelocationMission(null, fi);
				goto IL_C12;
			default:
				goto IL_C12;
			}
			missionByFleetID = this._db.GetMissionByFleetID(fi.ID);
			if (this._app.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fi.PlayerID).FactionID).Name == "loa" && missionByFleetID.Type != MissionType.DEPLOY_NPG && missionByFleetID.Type != MissionType.PATROL)
			{
                Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, fi.ID, MissionType.NO_MISSION);
			}
			switch (missionByFleetID.Type)
			{
			case MissionType.COLONIZATION:
				flag = this.DoColonizationMission(missionByFleetID, fi);
				break;
			case MissionType.SUPPORT:
				flag = this.DoSupportMission(missionByFleetID, fi);
				break;
			case MissionType.SURVEY:
				flag = this.DoSurveyMission(missionByFleetID, fi);
				break;
			case MissionType.RELOCATION:
				flag = this.DoRelocationMission(missionByFleetID, fi);
				break;
			case MissionType.CONSTRUCT_STN:
			case MissionType.UPGRADE_STN:
				flag = this.DoConstructionMission(missionByFleetID, fi);
				break;
			case MissionType.PATROL:
				flag = this.DoPatrolMission(missionByFleetID, fi);
				break;
			case MissionType.INTERDICTION:
				flag = this.DoInterdictMission(missionByFleetID, fi);
				break;
			case MissionType.STRIKE:
				flag = true;
				break;
			case MissionType.INVASION:
				flag = this.DoInvasionMission(missionByFleetID, fi);
				break;
			case MissionType.INTERCEPT:
				flag = this.DoInterceptMission(missionByFleetID, wi, fi);
				break;
			case MissionType.GATE:
				flag = this.DoGateMission(missionByFleetID, fi);
				break;
			case MissionType.PIRACY:
				flag = this.DoPiracyMission(missionByFleetID, fi);
				break;
			case MissionType.DEPLOY_NPG:
				flag = this.DoDeployNPGMission(missionByFleetID, fi);
				break;
			case MissionType.EVACUATE:
				flag = this.DoEvacuationMission(missionByFleetID, fi);
				break;
			case MissionType.SPECIAL_CONSTRUCT_STN:
				flag = this.DoSpecialConstructionMission(missionByFleetID, fi);
				break;
			}
			if (flag && fi.Type != FleetType.FL_CARAVAN && this._app.GameDatabase.GetFleetInfo(fi.ID) != null)
			{
				if (this._db.GetPlayerInfo(fi.PlayerID).AutoPatrol && missionByFleetID.TargetSystemID != 0)
				{
                    if (Kerberos.Sots.StarFleet.StarFleet.DoAutoPatrol(this, fi, missionByFleetID))
					{
						return;
					}
				}
				else
				{
                    if (Kerberos.Sots.StarFleet.StarFleet.SetReturnMission(this, fi, missionByFleetID))
					{
						return;
					}
				}
			}
			IL_C12:
			if (flag)
			{
				this._db.RemoveWaypoint(wi.ID);
			}
		}
		public int GetNumSupportTrips(int fleetId, int systemId)
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(fleetId);
			if (fleetInfo == null)
			{
				return 0;
			}
            float sensorTravelDistance = Kerberos.Sots.StarFleet.StarFleet.GetSensorTravelDistance(this._app.Game, fleetInfo.SystemID, systemId, fleetInfo.ID);
			float fleetRange = Kerberos.Sots.StarFleet.StarFleet.GetFleetRange(this._app.Game, fleetInfo);
			if (sensorTravelDistance == 0f)
			{
				return 5;
			}
			int val = (int)Math.Floor((double)(fleetRange / sensorTravelDistance));
			return Math.Max(val, 1);
		}
		public int GetNumSupportTrips(MissionInfo mission)
		{
			if (mission == null)
			{
				return 1;
			}
			return this.GetNumSupportTrips(mission.FleetID, mission.TargetSystemID);
		}
		public bool ProcessMoveOrder(MoveOrderInfo moi, FleetInfo fi, ref float remainingNodeDistance)
		{
            if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, this.GameDatabase.GetFleetInfo(moi.FleetID)))
			{
				moi.Progress = 1.1f;
				if (fi != null && moi.ToSystemID != 0)
				{
					fi.SupportingSystemID = moi.ToSystemID;
					this.GameDatabase.UpdateFleetInfo(fi);
				}
				return true;
			}
			if (moi.ToSystemID != 0 && fi.SystemID == moi.ToSystemID)
			{
				return true;
			}
			Faction faction = this._app.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(fi.PlayerID));
			if (!faction.CanUseGate() || !this._db.SystemHasGate(moi.FromSystemID, fi.PlayerID) || moi.Progress != 0f)
			{
				if (faction.CanUseNodeLine(new bool?(true)))
				{
					NodeLineInfo nodeLineBetweenSystems = this._db.GetNodeLineBetweenSystems(fi.PlayerID, moi.FromSystemID, moi.ToSystemID, true, false);
					if (nodeLineBetweenSystems != null)
					{
						return this.UpdateLinearMove(moi, fi, true, false, false, ref remainingNodeDistance);
					}
				}
				if (faction.CanUseNodeLine(new bool?(false)))
				{
					NodeLineInfo nodeLineBetweenSystems2 = this._db.GetNodeLineBetweenSystems(fi.PlayerID, moi.FromSystemID, moi.ToSystemID, false, false);
					if (nodeLineBetweenSystems2 != null)
					{
						return this.UpdateLinearMove(moi, fi, false, true, false, ref remainingNodeDistance);
					}
				}
				return this.UpdateLinearMove(moi, fi, false, false, false, ref remainingNodeDistance);
			}
			if (!this._db.SystemHasGate(moi.ToSystemID, fi.PlayerID))
			{
				return this.UpdateLinearMove(moi, fi, false, false, true, ref remainingNodeDistance);
			}
			int fleetCruiserEquivalent = this._db.GetFleetCruiserEquivalent(moi.FleetID);
			int num = 0;
			this._playerGateMap.TryGetValue(fi.PlayerID, out num);
			if (fleetCruiserEquivalent > num)
			{
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_GATE_CAPACITY_REACHED,
					EventMessage = TurnEventMessage.EM_GATE_CAPACITY_REACHED,
					PlayerID = fi.PlayerID,
					SystemID = moi.FromSystemID,
					FleetID = fi.ID,
					ShowsDialog = false,
					TurnNumber = this.App.GameDatabase.GetTurnCount()
				});
				return false;
			}
			Dictionary<int, int> playerGateMap;
			int playerID;
			(playerGateMap = this._playerGateMap)[playerID = fi.PlayerID] = playerGateMap[playerID] - fleetCruiserEquivalent;
			return true;
		}
		public MoveOrderInfo GetNextMoveOrder(WaypointInfo wi, FleetInfo fi, bool useDirectRoute)
		{
			if (this._db.GetMoveOrderInfoByFleetID(fi.ID) == null)
			{
				int num = 0;
				if (wi.Type == WaypointType.ReturnHome)
				{
					num = this._db.GetHomeSystem(this, wi.MissionID, fi);
				}
				else
				{
					if (wi.SystemID.HasValue)
					{
						num = wi.SystemID.Value;
					}
				}
				if (fi.SystemID == num || num == 0)
				{
					return null;
				}
				if (num == 0)
				{
					this._db.RemoveFleet(fi.ID);
					return null;
				}
				int num2;
				float num3;
                List<int> bestTravelPath = Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this, fi.ID, fi.SystemID, num, out num2, out num3, useDirectRoute, null, null);
				if (bestTravelPath[0] == 0)
				{
					this._db.InsertMoveOrder(fi.ID, 0, this._db.GetFleetLocation(fi.ID, false).Coords, bestTravelPath[1], Vector3.Zero);
				}
				else
				{
					this._db.InsertMoveOrder(fi.ID, bestTravelPath[0], Vector3.Zero, bestTravelPath[1], Vector3.Zero);
				}
			}
			return this._db.GetMoveOrderInfoByFleetID(fi.ID);
		}
		public bool MoveFleet(WaypointInfo wi, FleetInfo fleet, bool useDirectRoute)
		{
			int num = 0;
            float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, fleet.ID, true);
			if (this.IsFleetIntercepted(fleet))
			{
				num = fleet.SystemID;
			}
			MoveOrderInfo nextMoveOrder = this.GetNextMoveOrder(wi, fleet, useDirectRoute);
			bool flag = true;
			while (nextMoveOrder != null && flag)
			{
				if (num != 0)
				{
					this.GameDatabase.UpdateFleetLocation(fleet.ID, num, null);
				}
				flag = this.ProcessMoveOrder(nextMoveOrder, fleet, ref fleetTravelSpeed);
				if (flag)
				{
					this.FinishMove(nextMoveOrder, fleet);
					nextMoveOrder = this.GetNextMoveOrder(wi, fleet, useDirectRoute);
					if (nextMoveOrder != null && this.isHostilesAtSystem(fleet.PlayerID, nextMoveOrder.FromSystemID))
					{
						flag = false;
						break;
					}
				}
			}
			return nextMoveOrder == null && flag;
		}
		public bool isHostilesAtSystem(int playerId, int systemId)
		{
			List<StationInfo> list = this.App.GameDatabase.GetStationForSystem(systemId).ToList<StationInfo>();
			List<FleetInfo> list2 = this.App.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			List<ColonyInfo> list3 = this.App.GameDatabase.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>();
			List<EncounterInfo> list4 = this._db.GetEncounterInfos().ToList<EncounterInfo>();
			List<AsteroidMonitorInfo> list5 = new List<AsteroidMonitorInfo>();
			List<MorrigiRelicInfo> list6 = new List<MorrigiRelicInfo>();
			foreach (EncounterInfo current in list4)
			{
				if (current.Type == EasterEgg.EE_ASTEROID_MONITOR)
				{
					AsteroidMonitorInfo asteroidMonitorInfo = this._app.GameDatabase.GetAsteroidMonitorInfo(current.Id);
					if (asteroidMonitorInfo != null)
					{
						list5.Add(asteroidMonitorInfo);
					}
				}
				else
				{
					if (current.Type == EasterEgg.EE_MORRIGI_RELIC)
					{
						MorrigiRelicInfo morrigiRelicInfo = this._app.GameDatabase.GetMorrigiRelicInfo(current.Id);
						if (morrigiRelicInfo != null)
						{
							list6.Add(morrigiRelicInfo);
						}
					}
				}
			}
			int num = (this._app.Game.ScriptModules.AsteroidMonitor != null) ? this._app.Game.ScriptModules.AsteroidMonitor.PlayerID : 0;
			int num2 = (this._app.Game.ScriptModules.MorrigiRelic != null) ? this._app.Game.ScriptModules.MorrigiRelic.PlayerID : 0;
			List<int> list7 = new List<int>();
			foreach (StationInfo current2 in list)
			{
				if (!list7.Contains(current2.PlayerID) && current2.PlayerID != playerId)
				{
					list7.Add(current2.PlayerID);
				}
			}
			foreach (FleetInfo current3 in list2)
			{
				if (current3.PlayerID != playerId && !list7.Contains(current3.PlayerID) && !current3.IsReserveFleet && !current3.IsDefenseFleet && !current3.IsLimboFleet && !current3.IsTrapFleet)
				{
					bool flag = true;
					if (current3.PlayerID == num)
					{
						AsteroidMonitorInfo asteroidMonitorInfo2 = list5.FirstOrDefault((AsteroidMonitorInfo x) => x.SystemId == systemId);
						if (asteroidMonitorInfo2 != null && !asteroidMonitorInfo2.IsAggressive)
						{
							flag = false;
						}
					}
					else
					{
						if (current3.PlayerID == num2)
						{
							MorrigiRelicInfo morrigiRelicInfo2 = list6.FirstOrDefault((MorrigiRelicInfo x) => x.SystemId == systemId);
							if (morrigiRelicInfo2 != null && !morrigiRelicInfo2.IsAggressive)
							{
								flag = false;
							}
						}
					}
					if (flag)
					{
						list7.Add(current3.PlayerID);
					}
				}
			}
			foreach (ColonyInfo current4 in list3)
			{
				if (!list7.Contains(current4.PlayerID) && current4.PlayerID != playerId)
				{
					list7.Add(current4.PlayerID);
				}
			}
			Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerId);
			foreach (int current5 in list7)
			{
				if ((current5 != num2 || !playerObject.InstantDefeatMorrigiRelics()) && this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(current5, playerId) == DiplomacyState.WAR)
				{
					return true;
				}
			}
			return false;
		}
		public bool isTrapAtPlanet(int systemId, int planetId, int playerId)
		{
			ColonyTrapInfo colonyTrapInfoBySystemIDAndPlanetID = this.GameDatabase.GetColonyTrapInfoBySystemIDAndPlanetID(systemId, planetId);
			if (colonyTrapInfoBySystemIDAndPlanetID == null)
			{
				return false;
			}
			FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(colonyTrapInfoBySystemIDAndPlanetID.FleetID);
			return fleetInfo != null && fleetInfo.PlayerID != playerId && this.GameDatabase.GetDiplomacyStateBetweenPlayers(playerId, fleetInfo.PlayerID) != DiplomacyState.ALLIED;
		}
		public int IsIndependtRacePresent(int playerId, int systemId, out int planetid)
		{
			List<ColonyInfo> list = this.App.GameDatabase.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				if (current.PlayerID != playerId)
				{
					PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(current.PlayerID);
					if (!playerInfo.isStandardPlayer)
					{
						Faction faction = this.App.AssetDatabase.GetFaction(playerInfo.FactionID);
						if (faction.IsIndependent())
						{
							planetid = current.ID;
							return playerInfo.ID;
						}
					}
				}
			}
			planetid = 0;
			return 0;
		}
		public bool DoSurveyMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.ScriptModules.Locust != null && this.ScriptModules.Locust.PlayerID == fleet.PlayerID)
			{
				this.ScriptModules.Locust.UpdateScoutedSystems(this, fleet.SystemID);
				return true;
			}
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
			{
				return false;
			}
			MorrigiRelicInfo morrigiRelicInfo = this.GameDatabase.GetMorrigiRelicInfos().FirstOrDefault((MorrigiRelicInfo x) => x.SystemId == mission.TargetSystemID);
			if (morrigiRelicInfo != null)
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(fleet.PlayerID);
				if (playerObject.InstantDefeatMorrigiRelics())
				{
					this.ScriptModules.MorrigiRelic.ApplyRewardsToPlayers(this.App, morrigiRelicInfo, this.GameDatabase.GetShipInfoByFleetID(morrigiRelicInfo.FleetId, true).ToList<ShipInfo>(), new List<Kerberos.Sots.PlayerFramework.Player>
					{
						playerObject
					});
				}
			}
            int num = (int)((float)Kerberos.Sots.StarFleet.StarFleet.GetFleetSurveyPoints(this._db, fleet.ID) * this._db.GetStratModifierFloatToApply(StratModifiers.SurveyTimeModifier, fleet.PlayerID));
			mission.Duration -= num;
			this._db.UpdateMission(mission);
			if (mission.Duration <= 0)
			{
				int turnCount = this._db.GetTurnCount();
				this._db.InsertExploreRecord(mission.TargetSystemID, fleet.PlayerID, turnCount, true, true);
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_SYSTEM_SURVEYED,
					EventMessage = TurnEventMessage.EM_SYSTEM_SURVEYED,
					PlayerID = fleet.PlayerID,
					SystemID = mission.TargetSystemID,
					FleetID = fleet.ID,
					TurnNumber = this.App.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				int num3;
				int num2 = this.IsIndependtRacePresent(fleet.PlayerID, fleet.SystemID, out num3);
				if (num2 != 0 && num3 != 0)
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_SURVEY_INDEPENDENT_RACE_FOUND,
						EventMessage = TurnEventMessage.EM_SURVEY_INDEPENDENT_RACE_FOUND,
						PlayerID = fleet.PlayerID,
						TargetPlayerID = num2,
						SystemID = mission.TargetSystemID,
						FleetID = fleet.ID,
						ColonyID = num3,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				}
				if (fleet.PlayerID == this.LocalPlayer.ID)
				{
					this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_TO_BOLDY_GO);
				}
				Kerberos.Sots.PlayerFramework.Player playerObject2 = this.GetPlayerObject(fleet.PlayerID);
				if (playerObject2 != null && playerObject2.IsAI() && playerObject2.IsStandardPlayer)
				{
					playerObject2.GetAI().HandleSurveyMissionCompleted(fleet.ID, mission.TargetSystemID);
				}
				GameSession.Trace(string.Concat(new object[]
				{
					"Fleet ",
					fleet.ID,
					" has finished surveying system ",
					mission.TargetSystemID
				}));
				return true;
			}
			if (num != 0)
			{
				int num4 = mission.Duration / num;
				if (mission.Duration % num != 0)
				{
					num4++;
				}
				GameSession.Trace(string.Concat(new object[]
				{
					"Fleet ",
					fleet.ID,
					" surveying system ",
					mission.TargetSystemID,
					", ",
					num4,
					" turns left."
				}));
				return false;
			}
			return false;
		}
		public void CheckLoaFleetGateCompliancy(FleetInfo fi)
		{
			if (this._app.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fi.PlayerID).FactionID).Name == "loa")
			{
                int maxLoaFleetCubeMassForTransit = Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this, fi.PlayerID);
                int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this, fi.ID);
				if (fleetLoaCubeValue > maxLoaFleetCubeMassForTransit)
				{
					int num = fleetLoaCubeValue - maxLoaFleetCubeMassForTransit;
					ShipInfo shipInfo = this._app.GameDatabase.GetShipInfoByFleetID(fi.ID, false).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
					if (shipInfo != null && this._app.GameDatabase.GetShipInfoByFleetID(fi.ID, false).Count<ShipInfo>() == 1)
					{
						this._app.GameDatabase.UpdateShipLoaCubes(shipInfo.ID, fleetLoaCubeValue - num);
					}
					else
					{
                        int shipId = Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, fi.ID);
						this._app.GameDatabase.UpdateShipLoaCubes(shipId, fleetLoaCubeValue - num);
                        Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this, fi.ID, MissionType.NO_MISSION);
					}
					if (fi.SystemID != 0)
					{
						FleetInfo fleetInfo = this._app.GameDatabase.GetFleetsByPlayerAndSystem(fi.PlayerID, fi.SystemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).FirstOrDefault((FleetInfo x) => x.ID != fi.ID);
						if (fleetInfo == null)
						{
							this._app.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_LOA_CUBES_ABANDONED,
								EventMessage = TurnEventMessage.EM_LOA_CUBES_ABANDONED,
								FleetID = fi.ID,
								PlayerID = fi.PlayerID,
								SystemID = fi.SystemID,
								Savings = (double)num,
								TurnNumber = this._app.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
							return;
						}
						ShipInfo shipInfo2 = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
						if (shipInfo2 != null)
						{
							this._app.GameDatabase.UpdateShipLoaCubes(shipInfo2.ID, shipInfo2.LoaCubes + num);
							return;
						}
						DesignInfo designInfo = this._app.GameDatabase.GetDesignInfosForPlayer(fi.PlayerID).FirstOrDefault((DesignInfo x) => x.IsLoaCube());
						if (designInfo != null)
						{
							this._app.GameDatabase.InsertShip(fleetInfo.ID, designInfo.ID, "Cube", (ShipParams)0, null, num);
							return;
						}
					}
					else
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_LOA_CUBES_ABANDONED_DEEPSPACE,
							EventMessage = TurnEventMessage.EM_LOA_CUBES_ABANDONED_DEEPSPACE,
							FleetID = fi.ID,
							PlayerID = fi.PlayerID,
							Savings = (double)num,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
		}
		public void SetAColonyTrap(ShipInfo trapShip, int playerID, int systemID, int planetID)
		{
			ColonyTrapInfo colonyTrapInfoBySystemIDAndPlanetID = this.GameDatabase.GetColonyTrapInfoBySystemIDAndPlanetID(systemID, planetID);
			if (colonyTrapInfoBySystemIDAndPlanetID != null)
			{
				this.GameDatabase.RemoveColonyTrapInfo(colonyTrapInfoBySystemIDAndPlanetID.ID);
			}
			Matrix? matrix = new Matrix?(this._db.GetOrbitalTransform(planetID));
			PlanetInfo planetInfo = this._db.GetPlanetInfo(planetID);
			float num = (planetInfo != null) ? StarSystemVars.Instance.SizeToRadius(planetInfo.Size) : 0f;
			int num2 = this._db.InsertFleet(playerID, 0, systemID, systemID, "TRAP", FleetType.FL_TRAP);
			DesignInfo designInfo = this._db.GetVisibleDesignInfosForPlayerAndRole(playerID, ShipRole.TRAPDRONE, true).FirstOrDefault<DesignInfo>();
			if (designInfo == null)
			{
				return;
			}
			int num3 = 10;
			for (int i = 0; i < num3; i++)
			{
				int shipID = this._db.InsertShip(num2, designInfo.ID, null, (ShipParams)0, null, 0);
				this._db.TransferShip(shipID, num2);
				Matrix matrix2 = Matrix.Identity;
				Vector3 v = default(Vector3);
				v.X = this.Random.NextInclusive(-1f, 1f);
				v.Y = this.Random.NextInclusive(-1f, 1f);
				v.Z = this.Random.NextInclusive(-1f, 1f);
				if (v.LengthSquared > 1.401298E-45f)
				{
					v.Normalize();
					matrix2.Position = v * this.Random.NextInclusive(0f, num * 0.75f);
					matrix2 = Matrix.CreateWorld(matrix2.Position, -Vector3.Normalize(matrix2.Position), Vector3.UnitY);
					if (matrix.HasValue)
					{
						matrix2 *= matrix.Value;
					}
				}
				this._db.UpdateShipSystemPosition(shipID, new Matrix?(matrix2));
			}
			this._db.InsertColonyTrap(systemID, planetID, num2);
			this._db.RemoveShip(trapShip.ID);
		}
		private Vector3 GetBestInterceptPoint(FleetInfo interceptingFleet, FleetInfo targetFleet)
		{
			MoveOrderInfo moveOrderInfoByFleetID = this._db.GetMoveOrderInfoByFleetID(targetFleet.ID);
			this._db.GetMoveOrderInfoByFleetID(interceptingFleet.ID);
			Vector3 coords = this._db.GetFleetLocation(targetFleet.ID, true).Coords;
			Vector3 coords2 = this._db.GetFleetLocation(interceptingFleet.ID, false).Coords;
			if (moveOrderInfoByFleetID == null)
			{
				return coords;
			}
			if (targetFleet.IsAcceleratorFleet)
			{
				return coords;
			}
            float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, targetFleet.ID, false);
			float num = fleetTravelSpeed * 0.5f;
			Vector3 vector = (moveOrderInfoByFleetID.ToSystemID != 0) ? this._db.GetStarSystemOrigin(moveOrderInfoByFleetID.ToSystemID) : moveOrderInfoByFleetID.ToCoords;
			Vector3 v = vector - coords;
			float num2 = v.Normalize();
			if (num2 < num)
			{
				return vector;
			}
            float fleetTravelSpeed2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, interceptingFleet.ID, false);
			Vector3 vector2 = coords + v * Math.Min(fleetTravelSpeed, num2);
			Vector3 vector3 = coords + v * num;
			return ((coords2 - vector3).LengthSquared < fleetTravelSpeed2 * fleetTravelSpeed2) ? vector3 : vector2;
		}
        public bool DoInterceptMission(MissionInfo mission, WaypointInfo waypoint, FleetInfo fleet)
        {
            Func<NeutronStarInfo, bool> predicate = null;
            Func<GardenerInfo, bool> func4 = null;
            Func<GardenerInfo, bool> func5 = null;
            Func<GardenerInfo, bool> func6 = null;
            FleetInfo fi = this._db.GetFleetInfo(mission.TargetFleetID);
            if (fi != null)
            {
                Func<StarSystemInfo, bool> func = null;
                Func<StarSystemInfo, bool> func2 = null;
                PlayerInfo playerInfo = this._db.GetPlayerInfo(fi.PlayerID);
                this.AssetDatabase.GetFaction(playerInfo.FactionID);
                PlayerInfo info2 = this._db.GetPlayerInfo(fleet.PlayerID);
                this.AssetDatabase.GetFaction(info2.FactionID);
                MoveOrderInfo moveOrderInfoByFleetID = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
                Vector3 coords = this._db.GetFleetLocation(fi.ID, true).Coords;
                Vector3 targetPosition = coords;
                Vector3 fromCoords = this._db.GetFleetLocation(fleet.ID, false).Coords;
                Vector3 bestInterceptPoint = this.GetBestInterceptPoint(fleet, fi);
                if (!StarMap.IsInRange(this._db, fleet.PlayerID, targetPosition, (float)1f, (Dictionary<int, List<ShipInfo>>)null) || Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet))
                {
                    if (moveOrderInfoByFleetID != null)
                    {
                        this._db.RemoveMoveOrder(moveOrderInfoByFleetID.ID);
                        this._db.InsertMoveOrder(moveOrderInfoByFleetID.FleetID, 0, fromCoords, fleet.SupportingSystemID, Vector3.Zero);
                    }
                    return true;
                }
                Vector3 vector4 = fromCoords - bestInterceptPoint;
                if (vector4.Length < Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, fleet.ID, false))
                {
                    targetPosition = bestInterceptPoint;
                }
                if (moveOrderInfoByFleetID != null)
                {
                    this._db.RemoveMoveOrder(moveOrderInfoByFleetID.ID);
                }
                this._db.InsertMoveOrder(fleet.ID, 0, fromCoords, 0, targetPosition);
                float remainingNodeDistance = 0f;
                moveOrderInfoByFleetID = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
                if (!this.ProcessMoveOrder(moveOrderInfoByFleetID, fleet, ref remainingNodeDistance))
                {
                    return false;
                }
                PendingCombat item = new PendingCombat();
                bool flag = (this.App.Game.ScriptModules.NeutronStar != null) && (this.App.Game.ScriptModules.NeutronStar.PlayerID == fi.PlayerID);
                bool flag2 = (this.App.Game.ScriptModules.Gardeners != null) && (this.App.Game.ScriptModules.Gardeners.PlayerID == fi.PlayerID);
                if (flag || flag2)
                {
                    if (flag)
                    {
                        if (predicate == null)
                        {
                            predicate = x => x.FleetId == fi.ID;
                        }
                        NeutronStarInfo info4 = this.App.GameDatabase.GetNeutronStarInfos().FirstOrDefault<NeutronStarInfo>(predicate);
                        if ((info4 != null) && info4.DeepSpaceSystemId.HasValue)
                        {
                            item.SystemID = info4.DeepSpaceSystemId.Value;
                        }
                    }
                    else
                    {
                        if (func4 == null)
                        {
                            func4 = x => x.FleetId == fi.ID;
                        }
                        GardenerInfo info5 = this.App.GameDatabase.GetGardenerInfos().FirstOrDefault<GardenerInfo>(func4);
                        if ((info5 != null) && info5.DeepSpaceSystemId.HasValue)
                        {
                            item.SystemID = info5.DeepSpaceSystemId.Value;
                        }
                    }
                }
                if (item.SystemID == 0)
                {
                    StarSystemInfo starSystemInfo = this.GameDatabase.GetStarSystemInfo(fi.SystemID);
                    if ((starSystemInfo == null) || !starSystemInfo.IsDeepSpace)
                    {
                        if (func == null)
                        {
                            func = x => (x.Origin - targetPosition).Length < 0.0001f;
                        }
                        StarSystemInfo info7 = this.GameDatabase.GetDeepspaceStarSystemInfos().ToList<StarSystemInfo>().FirstOrDefault<StarSystemInfo>(func);
                        if (info7 == null)
                        {
                            if (func2 == null)
                            {
                                func2 = x => (x.Origin - targetPosition).Length < 0.0001f;
                            }
                            info7 = this.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>().FirstOrDefault<StarSystemInfo>(func2);
                            if (info7 == null)
                            {
                                int num2 = this.GameDatabase.InsertStarSystem(null, App.Localize("@UI_STARMAP_ENCOUNTER_DEEPSPACE"), null, "Deepspace", targetPosition, false, false, null);
                                item.SystemID = num2;
                            }
                            else
                            {
                                item.SystemID = info7.ID;
                            }
                        }
                        else
                        {
                            item.SystemID = info7.ID;
                        }
                    }
                    else
                    {
                        item.SystemID = fi.SystemID;
                    }
                }
                this.GameDatabase.UpdateFleetLocation(fleet.ID, item.SystemID, null);
                this.GameDatabase.UpdateFleetLocation(fi.ID, item.SystemID, null);
                int fleetId = 0;
                List<GardenerInfo> source = this.GameDatabase.GetGardenerInfos().ToList<GardenerInfo>();
                if (func5 == null)
                {
                    func5 = x => x.FleetId == fi.ID;
                }
                if (source.FirstOrDefault<GardenerInfo>(func5) != null)
                {
                    if (func6 == null)
                    {
                        func6 = x => (x.GardenerFleetId == fi.ID) && (x.TurnsToWait <= 0);
                    }
                    GardenerInfo info9 = source.FirstOrDefault<GardenerInfo>(func6);
                    if (info9 != null)
                    {
                        fleetId = info9.FleetId;
                        this.GameDatabase.UpdateFleetLocation(info9.FleetId, item.SystemID, null);
                    }
                }
                MissionInfo missionByFleetID = this.GameDatabase.GetMissionByFleetID(fi.ID);
                if (missionByFleetID != null)
                {
                    List<WaypointInfo> list3 = this.GameDatabase.GetWaypointsByMissionID(missionByFleetID.ID).ToList<WaypointInfo>();
                    foreach (WaypointInfo info11 in list3)
                    {
                        this.GameDatabase.RemoveWaypoint(info11.ID);
                    }
                    this.GameDatabase.InsertWaypoint(missionByFleetID.ID, WaypointType.Intercepted, new int?(item.SystemID));
                    foreach (WaypointInfo info12 in list3)
                    {
                        this.GameDatabase.InsertWaypoint(info12.MissionID, info12.Type, info12.SystemID);
                    }
                }
                this.GameDatabase.RemoveMoveOrder(moveOrderInfoByFleetID.ID);
                MoveOrderInfo info13 = this._db.GetMoveOrderInfoByFleetID(mission.TargetFleetID);
                if (((info13 != null) && (item.SystemID != 0)) && (info13.ToSystemID != item.SystemID))
                {
                    this._db.RemoveMoveOrder(info13.ID);
                    this._db.InsertMoveOrder(mission.TargetFleetID, 0, coords, item.SystemID, Vector3.Zero);
                    this._db.InsertMoveOrder(mission.TargetFleetID, 0, this._db.GetStarSystemOrigin(item.SystemID), info13.ToSystemID, info13.ToCoords);
                }
                item.FleetIDs.Add(fleet.ID);
                item.FleetIDs.Add(fi.ID);
                if (fleetId != 0)
                {
                    item.FleetIDs.Add(fleetId);
                }
                List<Kerberos.Sots.PlayerFramework.Player> list4 = GetPlayersWithCombatAssets(this.App, item.SystemID).ToList<Kerberos.Sots.PlayerFramework.Player>();
                List<int> list5 = new List<int>();
                foreach (Kerberos.Sots.PlayerFramework.Player player in list4)
                {
                    if (!player.IsStandardPlayer)
                    {
                        list5.Add(player.ID);
                    }
                }
                item.Type = CombatType.CT_Meeting;
                item.PlayersInCombat = (from x in list4 select x.ID).ToList<int>();
                item.NPCPlayersInCombat = list5;
                item.ConflictID = GetNextUniqueCombatID();
                item.CardID = 1;
                this.m_Combats.Add(item);
                foreach (Kerberos.Sots.PlayerFramework.Player player2 in list4)
                {
                    if ((player2.ID != fleet.PlayerID) && player2.IsStandardPlayer)
                    {
                        if (flag || flag2)
                        {
                            if (player2.ID != fi.PlayerID)
                            {
                                this.GameDatabase.UpdateDiplomacyState(fleet.PlayerID, player2.ID, DiplomacyState.WAR, 500, true);
                                this.GameDatabase.InsertGovernmentAction(fleet.PlayerID, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
                            }
                        }
                        else
                        {
                            this.GameDatabase.UpdateDiplomacyState(fleet.PlayerID, player2.ID, DiplomacyState.WAR, 500, true);
                            this.GameDatabase.InsertGovernmentAction(fleet.PlayerID, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
                        }
                    }
                }
            }
            return true;
        }
        public bool DoInterdictMission(MissionInfo mission, FleetInfo fleet)
		{
			GameSession.Trace(string.Concat(new object[]
			{
				"Fleet ",
				fleet.ID,
				" blockading system ",
				mission.TargetSystemID
			}));
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet);
		}
		public bool DoInvasionMission(MissionInfo mission, FleetInfo fleet)
		{
			AIColonyIntel aIColonyIntel = this._db.GetColonyIntelForPlanet(fleet.PlayerID, mission.TargetOrbitalObjectID);
			if (aIColonyIntel == null)
			{
				List<int> list = StarSystemDetailsUI.CollectPlanetListItemsForInvasionMission(this._app, mission.TargetSystemID).ToList<int>();
				foreach (int current in list)
				{
					aIColonyIntel = this._db.GetColonyIntelForPlanet(fleet.PlayerID, current);
					if (aIColonyIntel != null && aIColonyIntel.OwningPlayerID != fleet.PlayerID)
					{
						break;
					}
					aIColonyIntel = null;
				}
			}
			if (aIColonyIntel == null || aIColonyIntel.OwningPlayerID == fleet.PlayerID)
			{
				return true;
			}
			if (this._app.GameDatabase.GetDiplomacyStateBetweenPlayers(aIColonyIntel.OwningPlayerID, fleet.PlayerID) != DiplomacyState.WAR)
			{
				this.DeclareWarInformally(fleet.PlayerID, aIColonyIntel.OwningPlayerID);
			}
			GameSession.Trace(string.Concat(new object[]
			{
				"Fleet ",
				fleet.ID,
				" is invading system ",
				mission.TargetSystemID
			}));
            return Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet);
		}
		public bool DoConstructionMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
			{
				return false;
			}
			if (this.App.GameDatabase.GetStationInfo(mission.TargetOrbitalObjectID) == null)
			{
				if (this.App.GameDatabase.GetOrbitalObjectInfo(mission.TargetOrbitalObjectID) == null)
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
				if (mission.StationType.HasValue)
				{
					DesignInfo designInfo = DesignLab.CreateStationDesignInfo(this.App.AssetDatabase, this.App.GameDatabase, fleet.PlayerID, (StationType)mission.StationType.Value, 0, true);
					int targetOrbitalObjectID = this.ConstructStation(designInfo, mission.TargetOrbitalObjectID, false);
					mission.TargetOrbitalObjectID = targetOrbitalObjectID;
				}
			}
            int num = (int)(Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this, fleet.ID) * this._db.GetStratModifierFloatToApply(StratModifiers.ConstructionPointBonus, fleet.PlayerID));
			if (num == 0)
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			mission.Duration -= num;
			if (mission.Duration <= 0)
			{
				StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(mission.TargetOrbitalObjectID);
				if (stationInfo == null)
				{
					return false;
				}
				if (stationInfo.DesignInfo.StationLevel > 4)
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
				DesignInfo designInfo2 = DesignLab.CreateStationDesignInfo(this._app.AssetDatabase, this._app.GameDatabase, fleet.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, true);
				this.UpgradeStation(stationInfo, designInfo2);
				PlayerInfo playerInfo = this._db.GetPlayerInfo(stationInfo.PlayerID);
				if (playerInfo != null)
				{
					this._db.UpdatePlayerSavings(stationInfo.PlayerID, playerInfo.Savings - (double)designInfo2.SavingsCost);
				}
				if (stationInfo.DesignInfo.StationLevel == 1)
				{
					if (stationInfo.DesignInfo.StationType == StationType.CIVILIAN)
					{
						GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_CIVILIAN_STATION_BUILT, fleet.PlayerID, null, this.App.GameDatabase.GetStarSystemInfo(fleet.SystemID).ProvinceID, null);
					}
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_STATION_BUILT,
						EventMessage = TurnEventMessage.EM_STATION_BUILT,
						EventSoundCueName = this.GetUpgradeStationSoundCueName(designInfo2),
						TurnNumber = this._db.GetTurnCount(),
						PlayerID = fleet.PlayerID,
						SystemID = fleet.SystemID,
						OrbitalID = stationInfo.OrbitalObjectID,
						FleetID = fleet.ID,
						ShowsDialog = true
					});
					Faction faction = this.App.AssetDatabase.GetFaction(playerInfo.FactionID);
					string localizedStationTypeName = this.App.AssetDatabase.GetLocalizedStationTypeName(stationInfo.DesignInfo.StationType, faction.HasSlaves());
					switch (stationInfo.DesignInfo.StationType)
					{
					case StationType.NAVAL:
						this.App.GameDatabase.InsertGovernmentAction(playerInfo.ID, string.Format(App.Localize("@GA_STATIONBUILT"), localizedStationTypeName), "StationBuilt_Naval", 0, 0);
						break;
					case StationType.SCIENCE:
						this.App.GameDatabase.InsertGovernmentAction(playerInfo.ID, string.Format(App.Localize("@GA_STATIONBUILT"), localizedStationTypeName), "StationBuilt_Sci", 0, 0);
						break;
					case StationType.CIVILIAN:
						this.App.GameDatabase.InsertGovernmentAction(playerInfo.ID, string.Format(App.Localize("@GA_STATIONBUILT"), localizedStationTypeName), "StationBuilt_Civ", 0, 0);
						break;
					case StationType.DIPLOMATIC:
						this.App.GameDatabase.InsertGovernmentAction(playerInfo.ID, string.Format(App.Localize("@GA_STATIONBUILT"), localizedStationTypeName), "StationBuilt_Dip", 0, 0);
						break;
					case StationType.MINING:
						this.App.GameDatabase.InsertGovernmentAction(playerInfo.ID, string.Format(App.Localize("@GA_STATIONBUILT"), localizedStationTypeName), "StationBuilt_Mine", 0, 0);
						break;
					}
					if (stationInfo.DesignInfo.StationType == StationType.GATE && stationInfo.DesignInfo.StationLevel > 0)
					{
						List<FleetInfo> list = this._app.GameDatabase.GetFleetInfoBySystemID(fleet.SystemID, FleetType.FL_GATE).ToList<FleetInfo>();
						foreach (FleetInfo current in list)
						{
							if (current.PlayerID == stationInfo.PlayerID)
							{
								this._app.GameDatabase.RemoveFleet(current.ID);
							}
						}
					}
					if (stationInfo.DesignInfo.StationType == StationType.SCIENCE && stationInfo.DesignInfo.StationLevel > 0)
					{
						OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
						PlanetInfo planetInfo = this._app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ParentID.Value);
						if (planetInfo != null)
						{
							ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ParentID.Value);
							if (colonyInfoForPlanet != null)
							{
								PlayerInfo playerInfo2 = this._app.GameDatabase.GetPlayerInfo(colonyInfoForPlanet.PlayerID);
								Faction faction2 = this.App.AssetDatabase.GetFaction(playerInfo2.FactionID);
								if (faction2.IsIndependent() && !this.App.GameDatabase.GetHasPlayerStudyingIndependentRace(fleet.PlayerID, playerInfo2.ID))
								{
									this.InsertNewIndependentPlanetResearchProject(fleet.PlayerID, playerInfo2.ID);
								}
							}
						}
					}
					int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(fleet.SystemID);
					if ((!systemOwningPlayer.HasValue || systemOwningPlayer == fleet.PlayerID) && stationInfo.DesignInfo.StationType == StationType.NAVAL)
					{
						Kerberos.Sots.GameStates.StarSystem.PaintSystemPlayerColor(this._app.GameDatabase, fleet.SystemID, fleet.PlayerID);
					}
				}
				else
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_STATION_UPGRADED,
						EventMessage = TurnEventMessage.EM_STATION_UPGRADED,
						TurnNumber = this._db.GetTurnCount(),
						PlayerID = fleet.PlayerID,
						SystemID = fleet.SystemID,
						OrbitalID = stationInfo.OrbitalObjectID,
						FleetID = fleet.ID,
						ShowsDialog = false
					});
				}
			}
			this._db.UpdateMission(mission);
			return mission.Duration <= 0;
		}
		public bool DoSpecialConstructionMission(MissionInfo mission, FleetInfo fleet)
		{
			MoveOrderInfo moveOrderInfoByFleetID = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
			Vector3 coords = this._db.GetFleetLocation(fleet.ID, false).Coords;
			FleetInfo fleetInfo = this._db.GetFleetInfo(mission.TargetFleetID);
			if (fleetInfo == null)
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			if (mission.TargetSystemID == 0)
			{
				this._db.GetMoveOrderInfoByFleetID(mission.TargetFleetID);
				Vector3 coords2 = this._db.GetFleetLocation(fleetInfo.ID, true).Coords;
				Vector3 vector = coords2;
                if (!StarMap.IsInRange(this._db, fleet.PlayerID, vector, 1f, null) || Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet))
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
				if (moveOrderInfoByFleetID != null)
				{
					this._db.RemoveMoveOrder(moveOrderInfoByFleetID.ID);
				}
				this._db.InsertMoveOrder(fleet.ID, 0, coords, 0, vector);
				float num = 0f;
				moveOrderInfoByFleetID = this._db.GetMoveOrderInfoByFleetID(fleet.ID);
				if (!this.ProcessMoveOrder(moveOrderInfoByFleetID, fleet, ref num))
				{
					return false;
				}
				if (this._app.Game.ScriptModules.NeutronStar != null && this._app.Game.ScriptModules.NeutronStar.PlayerID == fleetInfo.PlayerID)
				{
					NeutronStarInfo neutronStarInfo = this._db.GetNeutronStarInfos().FirstOrDefault((NeutronStarInfo x) => x.FleetId == mission.TargetFleetID);
					if (neutronStarInfo == null)
					{
                        Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
						return true;
					}
					if (neutronStarInfo.DeepSpaceSystemId.HasValue)
					{
						mission.TargetSystemID = neutronStarInfo.DeepSpaceSystemId.Value;
						mission.TargetOrbitalObjectID = neutronStarInfo.DeepSpaceOrbitalId.Value;
						this.GameDatabase.UpdateFleetLocation(fleet.ID, neutronStarInfo.DeepSpaceSystemId.Value, null);
					}
				}
				else
				{
					if (this._app.Game.ScriptModules.Gardeners != null && this._app.Game.ScriptModules.Gardeners.PlayerID == fleetInfo.PlayerID)
					{
						GardenerInfo gardenerInfo = this._db.GetGardenerInfos().FirstOrDefault((GardenerInfo x) => x.FleetId == mission.TargetFleetID);
						if (gardenerInfo == null)
						{
                            Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
							return true;
						}
						if (gardenerInfo.DeepSpaceSystemId.HasValue)
						{
							mission.TargetSystemID = gardenerInfo.DeepSpaceSystemId.Value;
							mission.TargetOrbitalObjectID = gardenerInfo.DeepSpaceOrbitalId.Value;
							this.GameDatabase.UpdateFleetLocation(fleet.ID, gardenerInfo.DeepSpaceSystemId.Value, null);
						}
					}
				}
				this._db.UpdateMission(mission);
			}
			if (mission.TargetSystemID == 0 || mission.TargetOrbitalObjectID == 0)
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			List<StationInfo> list = this.App.GameDatabase.GetStationForSystem(mission.TargetSystemID).ToList<StationInfo>();
			if (list.Count > 0)
			{
				if (list.Any((StationInfo x) => this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(fleet.PlayerID, x.PlayerID) != DiplomacyState.WAR))
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
				PendingCombat pendingCombat = new PendingCombat();
				pendingCombat.SystemID = mission.TargetSystemID;
				pendingCombat.FleetIDs.Add(fleet.ID);
				pendingCombat.FleetIDs.Add(fleetInfo.ID);
				List<Kerberos.Sots.PlayerFramework.Player> list2 = GameSession.GetPlayersWithCombatAssets(this.App, pendingCombat.SystemID).ToList<Kerberos.Sots.PlayerFramework.Player>();
				List<int> list3 = new List<int>();
				foreach (Kerberos.Sots.PlayerFramework.Player current in list2)
				{
					if (!current.IsStandardPlayer)
					{
						list3.Add(current.ID);
					}
				}
				pendingCombat.Type = CombatType.CT_Meeting;
				pendingCombat.PlayersInCombat = (
					from x in list2
					select x.ID).ToList<int>();
				pendingCombat.NPCPlayersInCombat = list3;
				pendingCombat.ConflictID = GameSession.GetNextUniqueCombatID();
				pendingCombat.CardID = 1;
				this.m_Combats.Add(pendingCombat);
				return false;
			}
			else
			{
				if (this.App.GameDatabase.GetStationInfo(mission.TargetOrbitalObjectID) == null)
				{
					if (this.App.GameDatabase.GetOrbitalObjectInfo(mission.TargetOrbitalObjectID) == null)
					{
                        Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
						return true;
					}
					if (mission.StationType.HasValue)
					{
						DesignInfo designInfo = DesignLab.CreateStationDesignInfo(this.App.AssetDatabase, this.App.GameDatabase, fleet.PlayerID, (StationType)mission.StationType.Value, 0, true);
						int targetOrbitalObjectID = this.ConstructStation(designInfo, mission.TargetOrbitalObjectID, false);
						mission.TargetOrbitalObjectID = targetOrbitalObjectID;
					}
				}
                int num2 = (int)(Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this, fleet.ID) * this._db.GetStratModifierFloatToApply(StratModifiers.ConstructionPointBonus, fleet.PlayerID));
				if (num2 == 0)
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
					return true;
				}
				mission.Duration -= num2;
				if (mission.Duration <= 0)
				{
					StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(mission.TargetOrbitalObjectID);
					if (stationInfo == null)
					{
						return false;
					}
					DesignInfo designInfo2 = DesignLab.CreateStationDesignInfo(this._app.AssetDatabase, this._app.GameDatabase, fleet.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, true);
					this.UpgradeStation(stationInfo, designInfo2);
					PlayerInfo playerInfo = this._db.GetPlayerInfo(stationInfo.PlayerID);
					if (playerInfo != null)
					{
						this._db.UpdatePlayerSavings(stationInfo.PlayerID, playerInfo.Savings - (double)designInfo2.SavingsCost);
					}
					if (stationInfo.DesignInfo.StationLevel == 1)
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_STATION_BUILT,
							EventMessage = TurnEventMessage.EM_STATION_BUILT,
							EventSoundCueName = this.GetUpgradeStationSoundCueName(designInfo2),
							TurnNumber = this._db.GetTurnCount(),
							PlayerID = fleet.PlayerID,
							SystemID = fleet.SystemID,
							OrbitalID = stationInfo.OrbitalObjectID,
							FleetID = fleet.ID,
							ShowsDialog = true
						});
						Faction faction = this.App.AssetDatabase.GetFaction(playerInfo.FactionID);
						string localizedStationTypeName = this.App.AssetDatabase.GetLocalizedStationTypeName(stationInfo.DesignInfo.StationType, faction.HasSlaves());
						if (stationInfo.DesignInfo.StationType == StationType.SCIENCE)
						{
							this.App.GameDatabase.InsertGovernmentAction(playerInfo.ID, string.Format(App.Localize("@GA_STATIONBUILT"), localizedStationTypeName), "StationBuilt_Sci", 0, 0);
						}
					}
					if (fleetInfo != null)
					{
						if (this._app.Game.ScriptModules.NeutronStar != null && this._app.Game.ScriptModules.NeutronStar.PlayerID == fleetInfo.PlayerID)
						{
							NeutronStarInfo neutronStarInfo2 = this._db.GetNeutronStarInfos().FirstOrDefault((NeutronStarInfo x) => x.DeepSpaceSystemId.Value == mission.TargetSystemID);
							if (neutronStarInfo2 != null)
							{
								this.InsertNewNeutronStarResearchProject(fleet.PlayerID, neutronStarInfo2.Id);
							}
						}
						else
						{
							if (this._app.Game.ScriptModules.Gardeners != null && this._app.Game.ScriptModules.Gardeners.PlayerID == fleetInfo.PlayerID)
							{
								GardenerInfo gardenerInfo2 = this._db.GetGardenerInfos().FirstOrDefault((GardenerInfo x) => x.DeepSpaceSystemId.HasValue && x.DeepSpaceSystemId.Value == mission.TargetSystemID);
								if (gardenerInfo2 != null)
								{
									this.InsertNewGardenerResearchProject(fleet.PlayerID, gardenerInfo2.Id);
								}
							}
						}
					}
				}
				if (mission.Duration <= 0)
				{
					MoveOrderInfo moveOrderInfoByFleetID2 = this.App.GameDatabase.GetMoveOrderInfoByFleetID(fleet.ID);
					if (moveOrderInfoByFleetID2 != null)
					{
						this._db.RemoveMoveOrder(moveOrderInfoByFleetID.ID);
						this._db.InsertMoveOrder(moveOrderInfoByFleetID.FleetID, 0, this.GameDatabase.GetStarSystemOrigin(mission.TargetSystemID), fleet.SupportingSystemID, Vector3.Zero);
					}
					mission.TargetSystemID = 0;
					mission.TargetOrbitalObjectID = 0;
					mission.TargetFleetID = 0;
					this._db.UpdateMission(mission);
					return true;
				}
				this._db.UpdateMission(mission);
				return false;
			}
		}
		public bool DoColonizationMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
			{
				return false;
			}
			if (this.isTrapAtPlanet(fleet.SystemID, mission.TargetOrbitalObjectID, fleet.PlayerID))
			{
				return false;
			}
			List<ColonyInfo> list = this._db.GetColonyInfosForSystem(fleet.SystemID).ToList<ColonyInfo>();
			int? systemOwningPlayer = this.App.GameDatabase.GetSystemOwningPlayer(fleet.SystemID);
			if (list.Count > 0 && systemOwningPlayer.HasValue && systemOwningPlayer != fleet.PlayerID && !this.App.GameDatabase.hasPermissionToBuildEnclave(fleet.PlayerID, mission.TargetOrbitalObjectID))
			{
				return true;
			}
			ColonyInfo colonyInfo = this._db.GetColonyInfoForPlanet(mission.TargetOrbitalObjectID);
			PlanetInfo planetInfo = this._db.GetPlanetInfo(mission.TargetOrbitalObjectID);
			if (colonyInfo == null)
			{
                double colonizationSpace = Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(this, fleet.ID);
				if (colonizationSpace > 0.0)
				{
					int? owningPlayer = this._app.GameDatabase.GetSystemOwningPlayer(fleet.SystemID);
					if (owningPlayer.HasValue && owningPlayer != fleet.PlayerID)
					{
						List<RequestInfo> source = this._db.GetRequestInfos().ToList<RequestInfo>();
						RequestInfo requestInfo = source.FirstOrDefault((RequestInfo x) => x.Type == RequestType.EstablishEnclaveRequest && x.InitiatingPlayer == fleet.PlayerID && x.ReceivingPlayer == owningPlayer && x.RequestValue == (float)fleet.SystemID);
						if (requestInfo != null)
						{
							this._app.GameDatabase.InsertGovernmentAction(fleet.PlayerID, App.Localize("@GA_ENCLAVEBUILT"), "EnclaveBuilt", 0, 0);
						}
					}
					int colonyID = this._db.InsertColony(mission.TargetOrbitalObjectID, fleet.PlayerID, colonizationSpace, 0.5f, this._db.GetTurnCount(), planetInfo.Infrastructure + (float)(colonizationSpace * 0.0001), true);
					List<MissionInfo> list2 = this._db.GetMissionsByPlanetDest(mission.TargetOrbitalObjectID).ToList<MissionInfo>();
					foreach (MissionInfo current in list2)
					{
						FleetInfo fleetInfo = this._db.GetFleetInfo(current.FleetID);
						if (current.Type == MissionType.COLONIZATION && fleetInfo.PlayerID != fleet.PlayerID)
						{
							this._db.ApplyDiplomacyReaction(fleetInfo.PlayerID, fleet.PlayerID, StratModifiers.DiplomacyReactionColonizeClaimedWorld, 1);
						}
					}
					colonyInfo = this._db.GetColonyInfo(colonyID);
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_WORLD_COLONIZED, fleet.PlayerID, null, this.App.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID).ProvinceID, null);
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.SetOutputRate(this._db, this.AssetDatabase, ref colonyInfo, planetInfo, Kerberos.Sots.Strategy.InhabitedPlanet.Colony.OutputRate.Trade, 0f);
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_COLONY_ESTABLISHED,
						EventMessage = TurnEventMessage.EM_COLONY_ESTABLISHED,
						EventSoundCueName = string.Format("STRAT_016-01_{0}_ColonyEstablished", this._db.GetFactionName(this._db.GetPlayerFactionID(fleet.PlayerID))),
						PlayerID = fleet.PlayerID,
						SystemID = mission.TargetSystemID,
						FleetID = fleet.ID,
						OrbitalID = mission.TargetOrbitalObjectID,
						ColonyID = colonyID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
					this.App.GameDatabase.InsertGovernmentAction(fleet.PlayerID, App.Localize("@GA_COLONYSTARTED"), "ColonyStarted", 0, 0);
					if (fleet.PlayerID == this.LocalPlayer.ID)
					{
						this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_WE_WILL_CALL_IT);
					}
                    double terraformingSpace = Kerberos.Sots.StarFleet.StarFleet.GetTerraformingSpace(this, fleet.ID);
                    Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this, fleet.ID);
					List<PlagueInfo> list3 = new List<PlagueInfo>();
					int resources = planetInfo.Resources;
					if (this._db.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, fleet.PlayerID))
					{
                        ShipInfo firstConstructionShip = Kerberos.Sots.StarFleet.StarFleet.GetFirstConstructionShip(this, fleet);
						if (firstConstructionShip != null)
						{
							this._db.RemoveShip(firstConstructionShip.ID);
						}
						planetInfo.Infrastructure += this._app.AssetDatabase.GetGlobal<float>("AssimilatorInitialValue");
					}
					List<ColonyFactionInfo> list4;
					bool flag;
					Kerberos.Sots.Strategy.InhabitedPlanet.Colony.MaintainColony(this, ref colonyInfo, ref planetInfo, ref list3, colonizationSpace, terraformingSpace, fleet, out list4, out flag, true);
					this._db.UpdateColony(colonyInfo);
					this._db.UpdatePlanet(planetInfo);
					if (planetInfo.Resources == 0 && resources != 0)
					{
						OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID);
						this.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_PLANET_NO_RESOURCES,
							EventMessage = TurnEventMessage.EM_PLANET_NO_RESOURCES,
							PlayerID = colonyInfo.PlayerID,
							SystemID = orbitalObjectInfo.StarSystemID,
							OrbitalID = orbitalObjectInfo.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					foreach (ColonyFactionInfo cfi in list4)
					{
						if (colonyInfo.Factions.Any((ColonyFactionInfo x) => x.FactionID == cfi.FactionID))
						{
							cfi.LastMorale = this._app.AssetDatabase.CivilianPopulationStartMoral;
							cfi.Morale = this._app.AssetDatabase.CivilianPopulationStartMoral;
							this._db.UpdateCivilianPopulation(cfi);
						}
						else
						{
							this._db.InsertColonyFaction(cfi.OrbitalObjectID, cfi.FactionID, cfi.CivilianPop, cfi.CivPopWeight, cfi.TurnEstablished);
						}
					}
					GameSession.Trace("Colony established in system " + mission.TargetSystemID);
				}
				else
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				}
			}
			if (!this._db.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, fleet.PlayerID))
			{
				return true;
			}
			if (this._db.CanSupportPlanet(fleet.PlayerID, mission.TargetOrbitalObjectID))
			{
				this.DoSupportMission(mission, fleet);
				return false;
			}
			return true;
		}
		public bool DoEvacuationMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
			{
				return false;
			}
			List<ColonyInfo> list = (
				from x in this._db.GetColonyInfosForSystem(fleet.SystemID)
				where x.PlayerID == fleet.PlayerID
				orderby this.GameDatabase.GetCivilianPopulation(x.OrbitalObjectID, 0, false)
				select x).ToList<ColonyInfo>();
			ColonyInfo colonyInfo = this._db.GetColonyInfoForPlanet(mission.TargetOrbitalObjectID);
			if (colonyInfo == null && list.Count > 0)
			{
				colonyInfo = list.Last<ColonyInfo>();
				mission.TargetOrbitalObjectID = colonyInfo.OrbitalObjectID;
				this._db.UpdateMission(mission);
			}
			if (colonyInfo != null)
			{
                int numColonizationShips = Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this, fleet.ID);
				if (numColonizationShips > 0)
				{
					double civilianPopulation = this.GameDatabase.GetCivilianPopulation(colonyInfo.OrbitalObjectID, 0, false);
					double num = Math.Min(civilianPopulation, (double)numColonizationShips * (double)this.AssetDatabase.EvacCivPerCol);
					bool flag = num > 0.0;
					int factionId = this.GameDatabase.GetPlayerFactionID(fleet.PlayerID);
					ColonyFactionInfo colonyFactionInfo = colonyInfo.Factions.FirstOrDefault((ColonyFactionInfo x) => x.FactionID == factionId);
					if (colonyFactionInfo != null)
					{
						double num2 = Math.Min(num, colonyFactionInfo.CivilianPop);
						colonyFactionInfo.CivilianPop -= num2;
						num -= num2;
						this.GameDatabase.UpdateCivilianPopulation(colonyFactionInfo);
					}
					if (num > 0.0)
					{
						ColonyFactionInfo[] factions = colonyInfo.Factions;
						for (int i = 0; i < factions.Length; i++)
						{
							ColonyFactionInfo colonyFactionInfo2 = factions[i];
							if (colonyFactionInfo2 != colonyFactionInfo)
							{
								double num3 = Math.Min(num, colonyFactionInfo2.CivilianPop);
								colonyFactionInfo2.CivilianPop -= num3;
								num -= num3;
								this.GameDatabase.UpdateCivilianPopulation(colonyFactionInfo2);
								if (num <= 0.0)
								{
									break;
								}
							}
						}
					}
					if (flag)
					{
						this._db.UpdateColony(colonyInfo);
						this._db.UpdatePlanet(this._db.GetPlanetInfo(colonyInfo.OrbitalObjectID));
					}
					if (num <= 0.0)
					{
						return true;
					}
					list.RemoveAll((ColonyInfo x) => x.OrbitalObjectID == mission.TargetOrbitalObjectID);
					this.GameDatabase.RemoveColonyOnPlanet(mission.TargetOrbitalObjectID);
					using (List<ColonyInfo>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ColonyInfo current = enumerator.Current;
							if (num <= 0.0)
							{
								break;
							}
							flag = (num > 0.0);
							colonyFactionInfo = current.Factions.FirstOrDefault((ColonyFactionInfo x) => x.FactionID == factionId);
							if (colonyFactionInfo != null)
							{
								double num4 = Math.Min(num, colonyFactionInfo.CivilianPop);
								colonyFactionInfo.CivilianPop -= num4;
								num -= num4;
								this.GameDatabase.UpdateCivilianPopulation(colonyFactionInfo);
							}
							if (num > 0.0)
							{
								ColonyFactionInfo[] factions2 = colonyInfo.Factions;
								for (int j = 0; j < factions2.Length; j++)
								{
									ColonyFactionInfo colonyFactionInfo3 = factions2[j];
									if (colonyFactionInfo3 != colonyFactionInfo)
									{
										double num5 = Math.Min(num, colonyFactionInfo3.CivilianPop);
										colonyFactionInfo3.CivilianPop -= num5;
										num -= num5;
										this.GameDatabase.UpdateCivilianPopulation(colonyFactionInfo3);
										if (num <= 0.0)
										{
											break;
										}
									}
								}
							}
							if (flag)
							{
								this._db.UpdateColony(current);
								this._db.UpdatePlanet(this._db.GetPlanetInfo(current.OrbitalObjectID));
							}
							if (num > 0.0)
							{
								this.GameDatabase.RemoveColonyOnPlanet(current.OrbitalObjectID);
							}
						}
						return true;
					}
				}
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
			}
			return true;
		}
		public bool DoPiracyMission(MissionInfo mission, FleetInfo fleet)
		{
            return Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet);
		}
		public bool DoDeployNPGMission(MissionInfo mission, FleetInfo fleet)
		{
			List<WaypointInfo> source = this._app.GameDatabase.GetWaypointsByMissionID(mission.ID).ToList<WaypointInfo>();
            Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, fleet.ID);
			ShipInfo shipInfo = this._app.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
			if (shipInfo == null)
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this.App.Game, fleet, true);
				return true;
			}
			DesignInfo designInfo = this._app.GameDatabase.GetDesignInfosForPlayer(fleet.PlayerID).FirstOrDefault((DesignInfo x) => x.DesignSections.First<DesignSectionInfo>().ShipSectionAsset.IsAccelerator);
			if (shipInfo.LoaCubes < designInfo.ProductionCost)
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this.App.Game, fleet, true);
				return true;
			}
			if (!source.Any<WaypointInfo>())
			{
				this._db.InsertMoveOrder(fleet.ID, 0, this._db.GetFleetLocation(fleet.ID, false).Coords, fleet.SupportingSystemID, Vector3.Zero);
				return true;
			}
			if (fleet.SystemID != 0 && this.App.GameDatabase.GetStarSystemInfo(fleet.SystemID) != null && !this.App.GameDatabase.GetStarSystemInfo(fleet.SystemID).IsDeepSpace)
			{
				if (!this._app.GameDatabase.GetFleetsByPlayerAndSystem(fleet.PlayerID, fleet.SystemID, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
				{
					int num = this._app.GameDatabase.InsertFleet(fleet.PlayerID, 0, fleet.SystemID, fleet.SupportingSystemID, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
					this._app.GameDatabase.InsertShip(num, designInfo.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, null, 0);
					if (fleet.PreviousSystemID.HasValue)
					{
						NodeLineInfo nodeLineInfo = this._app.GameDatabase.GetNodeLineBetweenSystems(fleet.PlayerID, fleet.PreviousSystemID.Value, mission.TargetSystemID, false, true);
						if (nodeLineInfo == null)
						{
							int id = this._app.GameDatabase.InsertNodeLine(fleet.PreviousSystemID.Value, mission.TargetSystemID, 90);
							nodeLineInfo = this._app.GameDatabase.GetNodeLine(id);
						}
						this._app.GameDatabase.InsertLoaLineFleetRecord(nodeLineInfo.ID, num);
					}
					shipInfo.LoaCubes -= designInfo.ProductionCost;
					this._app.GameDatabase.UpdateShipLoaCubes(shipInfo.ID, shipInfo.LoaCubes);
				}
				this._app.GameDatabase.UpdateFleetAccelerated(this, fleet.ID, null);
			}
			bool useDirectRoute = true;
			WaypointInfo wi = source.First<WaypointInfo>();
            float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, fleet.ID, false);
			MoveOrderInfo moi = this.GetNextMoveOrder(wi, fleet, useDirectRoute);
			bool flag = true;
			while (moi != null && flag)
			{
				flag = this.ProcessMoveOrder(moi, fleet, ref fleetTravelSpeed);
				if (flag)
				{
					int num2 = moi.ToSystemID;
					if (moi.ToSystemID == 0)
					{
						List<StarSystemInfo> list = this.GameDatabase.GetDeepspaceStarSystemInfos().ToList<StarSystemInfo>();
						list.AddRange(this.GameDatabase.GetStarSystemInfos());
						StarSystemInfo starSystemInfo = list.FirstOrDefault((StarSystemInfo x) => (double)(x.Origin - moi.ToCoords).Length < 0.0001);
						if (starSystemInfo == null)
						{
							num2 = this._app.GameDatabase.InsertStarSystem(null, "Accelerator Node", null, "Deepspace", moi.ToCoords, false, false, null);
						}
						else
						{
							num2 = starSystemInfo.ID;
						}
					}
					if (!this._app.GameDatabase.GetFleetsByPlayerAndSystem(fleet.PlayerID, num2, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
					{
						int num3 = this._app.GameDatabase.InsertFleet(fleet.PlayerID, 0, num2, fleet.SupportingSystemID, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
						int shipID = this._app.GameDatabase.InsertShip(num3, designInfo.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, null, 0);
						if (moi.ToSystemID != 0)
						{
							Matrix? validGateShipTransform = GameSession.GetValidGateShipTransform(this.App.Game, moi.ToSystemID, num3);
							if (validGateShipTransform.HasValue)
							{
								this.App.GameDatabase.UpdateShipSystemPosition(shipID, new Matrix?(validGateShipTransform.Value));
							}
						}
						if (moi.ToSystemID == 0)
						{
							this._app.GameDatabase.InsertMoveOrder(num3, num2, moi.FromCoords, mission.TargetSystemID, moi.ToCoords);
						}
						if (fleet.PreviousSystemID.HasValue)
						{
							NodeLineInfo nodeLineInfo2 = this._app.GameDatabase.GetNodeLineBetweenSystems(fleet.PlayerID, fleet.PreviousSystemID.Value, mission.TargetSystemID, false, true);
							if (nodeLineInfo2 == null)
							{
								int id2 = this._app.GameDatabase.InsertNodeLine(fleet.PreviousSystemID.Value, mission.TargetSystemID, 90);
								nodeLineInfo2 = this._app.GameDatabase.GetNodeLine(id2);
							}
							this._app.GameDatabase.InsertLoaLineFleetRecord(nodeLineInfo2.ID, num3);
						}
						shipInfo.LoaCubes -= designInfo.ProductionCost;
						this._app.GameDatabase.UpdateShipLoaCubes(shipInfo.ID, shipInfo.LoaCubes);
					}
					this._app.GameDatabase.UpdateFleetAccelerated(this, fleet.ID, null);
					this.FinishMove(moi, fleet);
					moi = this.GetNextMoveOrder(wi, fleet, useDirectRoute);
					if (moi != null && this.isHostilesAtSystem(fleet.PlayerID, moi.FromSystemID))
					{
						return false;
					}
				}
			}
			return moi == null;
		}
		public bool DoSupportMission(MissionInfo mission, FleetInfo fleet)
		{
			if (this.isHostilesAtSystem(fleet.PlayerID, fleet.SystemID))
			{
				return false;
			}
			ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(mission.TargetOrbitalObjectID);
			PlanetInfo planetInfo = this._db.GetPlanetInfo(mission.TargetOrbitalObjectID);
			if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == fleet.PlayerID)
			{
                double colonizationSpace = Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(this, fleet.ID);
                double terraformingSpace = Kerberos.Sots.StarFleet.StarFleet.GetTerraformingSpace(this, fleet.ID);
                Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this, fleet.ID);
				List<PlagueInfo> list = new List<PlagueInfo>();
				int resources = planetInfo.Resources;
				List<ColonyFactionInfo> list2;
				bool flag;
				Kerberos.Sots.Strategy.InhabitedPlanet.Colony.MaintainColony(this, ref colonyInfoForPlanet, ref planetInfo, ref list, colonizationSpace, terraformingSpace, fleet, out list2, out flag, true);
				if (colonyInfoForPlanet.CurrentStage == Kerberos.Sots.Data.ColonyStage.Colony)
				{
					this._app.GameDatabase.InsertGovernmentAction(colonyInfoForPlanet.PlayerID, App.Localize("@GA_UNDERDEVELOPEDCOLONY"), "UnderDevelopedColony", 0, 0);
				}
				if (planetInfo.Resources == 0 && resources != 0)
				{
					OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID);
					this.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_PLANET_NO_RESOURCES,
						EventMessage = TurnEventMessage.EM_PLANET_NO_RESOURCES,
						PlayerID = colonyInfoForPlanet.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = orbitalObjectInfo.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				this._db.UpdateColony(colonyInfoForPlanet);
				this._db.UpdatePlanet(planetInfo);
				foreach (ColonyFactionInfo cfi in list2)
				{
					if (colonyInfoForPlanet.Factions.Any((ColonyFactionInfo x) => x.FactionID == cfi.FactionID))
					{
						this._db.UpdateCivilianPopulation(cfi);
					}
					else
					{
						this._db.InsertColonyFaction(cfi.OrbitalObjectID, cfi.FactionID, cfi.CivilianPop, cfi.CivPopWeight, cfi.TurnEstablished);
					}
				}
				if (!this._db.CanSupportPlanet(colonyInfoForPlanet.PlayerID, colonyInfoForPlanet.OrbitalObjectID))
				{
					this._db.ClearWaypoints(mission.ID);
					this._db.InsertWaypoint(mission.ID, WaypointType.ReturnHome, null);
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_COLONY_SUPPORT_COMPLETE,
						EventMessage = TurnEventMessage.EM_COLONY_SUPPORT_COMPLETE,
						PlayerID = fleet.PlayerID,
						SystemID = mission.TargetSystemID,
						FleetID = fleet.ID,
						OrbitalID = planetInfo.ID,
						ColonyID = colonyInfoForPlanet.ID,
						MissionID = mission.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				else
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_COLONY_SUPPORT,
						EventMessage = TurnEventMessage.EM_COLONY_SUPPORT,
						PlayerID = fleet.PlayerID,
						SystemID = mission.TargetSystemID,
						FleetID = fleet.ID,
						OrbitalID = planetInfo.ID,
						ColonyID = colonyInfoForPlanet.ID,
						MissionID = mission.ID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
			}
			else
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._app.Game, fleet, true);
			}
			return true;
		}
		public bool DoGateMission(MissionInfo mission, FleetInfo fleet)
		{
			if (fleet.SystemID != mission.TargetSystemID)
			{
				return false;
			}
			if (mission.Duration > 0)
			{
				mission.Duration--;
				this._db.UpdateMission(mission);
				return false;
			}
			List<ShipInfo> source = (
				from x in this._db.GetShipInfoByFleetID(fleet.ID, true)
				where x.DesignInfo.Role == ShipRole.GATE
				select x).ToList<ShipInfo>();
			if (source.Count<ShipInfo>() == 0)
			{
				return true;
			}
			ShipInfo shipInfo = source.First<ShipInfo>();
			this._db.UpdateShipParams(shipInfo.ID, ShipParams.HS_GATE_DEPLOYED);
			int num = this._db.InsertFleet(fleet.PlayerID, 0, mission.TargetSystemID, fleet.SupportingSystemID, "GATE", FleetType.FL_GATE);
			this._db.TransferShip(shipInfo.ID, num);
			Matrix? validGateShipTransform = GameSession.GetValidGateShipTransform(this.App.Game, mission.TargetSystemID, num);
			if (validGateShipTransform.HasValue)
			{
				this.App.GameDatabase.UpdateShipSystemPosition(shipInfo.ID, new Matrix?(validGateShipTransform.Value));
			}
			return true;
		}
		public static Matrix? GetValidGateShipTransform(GameSession game, int systemId, int gateFleet)
		{
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(game, systemId, 1f);
			if (combatZonesForSystem.Count == 0)
			{
				GameSession.Warn(">>>Gate Mission Error: no combat zones in system");
				return null;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(gateFleet);
			if (fleetInfo == null)
			{
				GameSession.Warn(">>>Gate Mission Error: GateFleet [" + gateFleet + "] is null");
				return null;
			}
			if (combatZonesForSystem.Count > 0)
			{
				CombatZonePositionInfo combatZonePositionInfo = combatZonesForSystem.Last<CombatZonePositionInfo>();
				Vector3 vector = combatZonePositionInfo.Center;
				if (fleetInfo.PreviousSystemID.HasValue && fleetInfo.PreviousSystemID != systemId)
				{
					string factionName = (fleetInfo.Type == FleetType.FL_ACCELERATOR) ? "loa" : "hiver";
					Faction faction = game.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.Name == factionName);
					Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(systemId);
					Vector3 starSystemOrigin2 = game.GameDatabase.GetStarSystemOrigin(fleetInfo.PreviousSystemID.Value);
					Vector3 v = starSystemOrigin2 - starSystemOrigin;
					v.Y = 0f;
					v.Normalize();
					vector = v * (combatZonePositionInfo.RadiusLower + faction.EntryPointOffset);
				}
				Vector3 forward = -vector;
				forward.Normalize();
				Matrix mat = Matrix.CreateWorld(vector, forward, Vector3.UnitY);
				List<EntrySpawnLocation> list = new List<EntrySpawnLocation>();
				for (int i = 0; i < 9; i++)
				{
					int num = i / 3;
					int num2 = (i % 3 + 1) / 2;
					int num3 = (i % 2 == 0) ? 1 : -1;
					Vector3 position = Vector3.Transform(new Vector3
					{
						X = (float)num3 * 3000f * (float)num2,
						Y = 0f,
						Z = 3000f * (float)num
					}, mat);
					list.Add(new EntrySpawnLocation
					{
						Position = position
					});
				}
				foreach (FleetInfo current in game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>())
				{
					if (current != null && (current.Type == FleetType.FL_GATE || current.Type == FleetType.FL_ACCELERATOR) && current.ID != gateFleet)
					{
						List<EntrySpawnLocation> list2 = new List<EntrySpawnLocation>();
						foreach (ShipInfo current2 in game.GameDatabase.GetShipInfoByFleetID(current.ID, false).ToList<ShipInfo>())
						{
							Matrix? shipSystemPosition = game.GameDatabase.GetShipSystemPosition(current2.ID);
							if (shipSystemPosition.HasValue && shipSystemPosition.HasValue)
							{
								foreach (EntrySpawnLocation current3 in list)
								{
									float lengthSquared = (current3.Position - shipSystemPosition.Value.Position).LengthSquared;
									if (lengthSquared < 9000000f)
									{
										list2.Add(current3);
									}
								}
								foreach (EntrySpawnLocation current4 in list2)
								{
									list.Remove(current4);
								}
							}
						}
					}
				}
				Vector3 position2 = combatZonePositionInfo.Center;
				if (list.Count > 0)
				{
					position2 = list.First<EntrySpawnLocation>().Position;
				}
				return new Matrix?(Matrix.CreateWorld(position2, forward, Vector3.UnitY));
			}
			return null;
		}
		public bool DoPatrolMission(MissionInfo mission, FleetInfo fleet)
		{
            return Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(this, fleet);
		}
		public bool DoReturnMission(MissionInfo mission, FleetInfo fleet)
		{
			return mission.TargetSystemID == fleet.SystemID;
		}
		public bool DoRelocationMission(MissionInfo mission, FleetInfo fleet)
		{
            if (fleet.Type != FleetType.FL_CARAVAN && this.GameDatabase.GetRemainingSupportPoints(this, fleet.SystemID, fleet.PlayerID) < 0 && !Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, fleet) && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleet))
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleet, true);
				return true;
			}
			this.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_RELOCATION_COMPLETE,
				EventMessage = TurnEventMessage.EM_RELOCATION_COMPLETE,
				PlayerID = fleet.PlayerID,
				SystemID = fleet.SystemID,
				FleetID = fleet.ID,
				TurnNumber = this.App.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			if (fleet.Type == FleetType.FL_CARAVAN)
			{
				List<ShipInfo> list = this._app.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
				foreach (ShipInfo current in list)
				{
					if (current.DesignInfo.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.IsFreighter))
					{
						this._app.GameDatabase.TransferShip(current.ID, this._app.GameDatabase.InsertOrGetLimboFleetID(fleet.SystemID, fleet.PlayerID));
						this._app.GameDatabase.InsertFreighterInfo(fleet.SystemID, fleet.PlayerID, current, true);
					}
					else
					{
						if (current.IsSDB() || current.IsPlatform())
						{
							this._app.GameDatabase.TransferShip(current.ID, this._app.GameDatabase.InsertOrGetDefenseFleetInfo(fleet.SystemID, fleet.PlayerID).ID);
						}
						else
						{
							this._app.GameDatabase.TransferShip(current.ID, this._app.GameDatabase.InsertOrGetReserveFleetInfo(fleet.SystemID, fleet.PlayerID).ID);
						}
					}
				}
				if (mission != null)
				{
					this._db.RemoveMission(mission.ID);
				}
				this._app.GameDatabase.RemoveAdmiral(fleet.AdmiralID);
				this._app.GameDatabase.RemoveFleet(fleet.ID);
			}
			else
			{
				fleet.SupportingSystemID = fleet.SystemID;
				this._db.UpdateFleetInfo(fleet);
				if (mission != null)
				{
					this._db.RemoveMission(mission.ID);
				}
			}
			return true;
		}
		public void CompleteMission(FleetInfo fleet)
		{
            double fleetSlaves = Kerberos.Sots.StarFleet.StarFleet.GetFleetSlaves(this, fleet.ID);
			if (fleetSlaves != 0.0)
			{
				List<ColonyInfo> list = (
					from x in this.App.GameDatabase.GetColonyInfosForSystem(fleet.SystemID)
					where x.PlayerID == fleet.PlayerID
					select x).ToList<ColonyInfo>();
				if (list.Count != 0)
				{
					ColonyInfo colonyInfo = this.Random.Choose(list);
					List<PlayerInfo> list2 = (
						from x in this._db.GetStandardPlayerInfos()
						where x.FactionID != this._db.GetPlayerFactionID(fleet.PlayerID)
						select x).ToList<PlayerInfo>();
					if (list2.Count > 0)
					{
						PlayerInfo rPlayer = this.Random.Choose(list2);
						if (colonyInfo.Factions.Any((ColonyFactionInfo x) => x.FactionID == rPlayer.FactionID))
						{
							ColonyFactionInfo colonyFactionInfo = colonyInfo.Factions.FirstOrDefault((ColonyFactionInfo x) => x.FactionID == rPlayer.FactionID);
							if (colonyFactionInfo != null)
							{
								foreach (int current in this._db.GetShipsByFleetID(fleet.ID))
								{
									this._db.UpdateShipObtainedSlaves(current, 0.0);
								}
								colonyFactionInfo.CivilianPop += fleetSlaves;
								this._db.UpdateCivilianPopulation(colonyFactionInfo);
								this.App.GameDatabase.InsertTurnEvent(new TurnEvent
								{
									EventType = TurnEventType.EV_SLAVES_DELIVERED,
									EventMessage = TurnEventMessage.EM_SLAVES_DELIVERED,
									PlayerID = fleet.PlayerID,
									ColonyID = colonyInfo.ID,
									CivilianPop = (float)fleetSlaves,
									TurnNumber = this.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
							}
						}
						else
						{
							foreach (int current2 in this._db.GetShipsByFleetID(fleet.ID))
							{
								this._db.UpdateShipObtainedSlaves(current2, 0.0);
							}
							this._db.InsertColonyFaction(colonyInfo.OrbitalObjectID, rPlayer.FactionID, fleetSlaves, 0f, this._db.GetTurnCount());
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_SLAVES_DELIVERED,
								EventMessage = TurnEventMessage.EM_SLAVES_DELIVERED,
								PlayerID = fleet.PlayerID,
								ColonyID = colonyInfo.ID,
								CivilianPop = (float)fleetSlaves,
								TurnNumber = this.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
					}
				}
			}
			this.App.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_MISSION_COMPLETE,
				EventMessage = TurnEventMessage.EM_MISSION_COMPLETE,
				PlayerID = fleet.PlayerID,
				FleetID = fleet.ID,
				SystemID = fleet.SystemID,
				TurnNumber = this.App.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
		}
		public void ProcessMission(MissionInfo mission)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(mission.FleetID);
			WaypointInfo nextWaypointForMission = this._db.GetNextWaypointForMission(mission.ID);
			if (fleetInfo == null)
			{
				this._db.RemoveMission(mission.ID);
				return;
			}
			if (nextWaypointForMission == null)
			{
				this._db.RemoveMission(mission.ID);
				GameSession.Trace(string.Concat(new object[]
				{
					"Fleet ",
					mission.FleetID,
					" has completed ",
					mission.Type,
					" mission to system ",
					mission.TargetSystemID
				}));
				this.CompleteMission(fleetInfo);
				return;
			}
            if (!Kerberos.Sots.StarFleet.StarFleet.IsFleetWaitingForBuildOrders(this._app, mission.ID, mission.FleetID))
			{
				Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
				foreach (int current in this._db.GetStandardPlayerIDs())
				{
					dictionary.Add(current, StarMap.IsInRange(this._db, current, this._db.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, null));
				}
				if (!this.IsFleetInterdicted(fleetInfo) && (nextWaypointForMission.Type == WaypointType.TravelTo || nextWaypointForMission.Type == WaypointType.ReturnHome))
				{
					this.ProcessWaypoint(nextWaypointForMission, fleetInfo, mission.UseDirectRoute);
				}
				nextWaypointForMission = this._db.GetNextWaypointForMission(mission.ID);
				if (nextWaypointForMission == null)
				{
					this._db.RemoveMission(mission.ID);
					GameSession.Trace(string.Concat(new object[]
					{
						"Fleet ",
						mission.FleetID,
						" has completed ",
						mission.Type,
						" mission to system ",
						mission.TargetSystemID
					}));
					this.CompleteMission(fleetInfo);
					return;
				}
				if (nextWaypointForMission.Type != WaypointType.TravelTo && nextWaypointForMission.Type != WaypointType.ReturnHome)
				{
					if (nextWaypointForMission.Type == WaypointType.DoMission && mission.Type == MissionType.SUPPORT && mission.TargetSystemID == fleetInfo.SupportingSystemID)
					{
						this.ProcessWaypoint(nextWaypointForMission, fleetInfo, mission.UseDirectRoute);
						nextWaypointForMission = this._db.GetNextWaypointForMission(mission.ID);
						this.ProcessWaypoint(nextWaypointForMission, fleetInfo, mission.UseDirectRoute);
					}
					else
					{
						this.ProcessWaypoint(nextWaypointForMission, fleetInfo, mission.UseDirectRoute);
					}
				}
				foreach (int current2 in this._db.GetStandardPlayerIDs())
				{
					if ((fleetInfo.PlayerID == this.ScriptModules.Locust.PlayerID || fleetInfo.PlayerID == this.ScriptModules.Swarmers.PlayerID || fleetInfo.PlayerID == this.ScriptModules.SystemKiller.PlayerID) && !dictionary[current2] && StarMap.IsInRange(this._db, current2, this._db.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, null))
					{
						MoveOrderInfo moveOrderInfoByFleetID = this._db.GetMoveOrderInfoByFleetID(fleetInfo.ID);
						if (moveOrderInfoByFleetID != null && StarMap.IsInRange(this._db, current2, (moveOrderInfoByFleetID.ToSystemID == 0) ? moveOrderInfoByFleetID.ToCoords : this._db.GetStarSystemOrigin(moveOrderInfoByFleetID.ToSystemID), 1f, null))
						{
							if (fleetInfo.PlayerID == this.ScriptModules.Locust.PlayerID)
							{
								this.App.GameDatabase.InsertTurnEvent(new TurnEvent
								{
									EventType = TurnEventType.EV_LOCUST_SPOTTED,
									EventMessage = TurnEventMessage.EM_LOCUST_SPOTTED,
									PlayerID = current2,
									TurnNumber = this.App.GameDatabase.GetTurnCount(),
									ShowsDialog = false
								});
							}
							else
							{
								if (fleetInfo.PlayerID == this.ScriptModules.Swarmers.PlayerID)
								{
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_SWARM_QUEEN_SPOTTED,
										EventMessage = TurnEventMessage.EM_SWARM_QUEEN_SPOTTED,
										PlayerID = current2,
										TurnNumber = this.App.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
								}
								else
								{
									if (fleetInfo.PlayerID == this.ScriptModules.SystemKiller.PlayerID)
									{
										this.App.GameDatabase.InsertTurnEvent(new TurnEvent
										{
											EventType = TurnEventType.EV_SYS_KILLER_SPOTTED,
											EventMessage = TurnEventMessage.EM_SYS_KILLER_SPOTTED,
											PlayerID = current2,
											TurnNumber = this.App.GameDatabase.GetTurnCount(),
											ShowsDialog = false
										});
									}
								}
							}
						}
					}
				}
			}
			if (nextWaypointForMission == null)
			{
				this._db.RemoveMission(mission.ID);
				GameSession.Trace(string.Concat(new object[]
				{
					"Fleet ",
					mission.FleetID,
					" has completed ",
					mission.Type,
					" mission to system ",
					mission.TargetSystemID
				}));
				this.CompleteMission(fleetInfo);
			}
		}
		public void Phase2_FleetMovement()
		{
			List<MissionInfo> list = this._db.GetMissionInfos().ToList<MissionInfo>();
			this._playerGateMap.Clear();
			IEnumerable<PlayerInfo> playerInfos = this._app.GameDatabase.GetPlayerInfos();
			foreach (PlayerInfo current in playerInfos)
			{
				this._playerGateMap.Add(current.ID, GameSession.GetTotalGateCapacity(this, current.ID));
			}
			List<MissionInfo> list2 = new List<MissionInfo>();
			list2.AddRange(list);
			List<MissionInfo> list3 = new List<MissionInfo>();
			foreach (MissionInfo current2 in list2)
			{
				if (current2.Type == MissionType.INTERCEPT)
				{
					WaypointInfo nextWaypointForMission = this._app.GameDatabase.GetNextWaypointForMission(current2.ID);
					if (nextWaypointForMission == null)
					{
						list.Remove(current2);
                        Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, this._db.GetFleetInfo(current2.FleetID), true);
					}
					else
					{
						if (nextWaypointForMission.Type == WaypointType.DoMission)
						{
							list3.Add(current2);
							list.Remove(current2);
						}
					}
				}
			}
			list.InsertRange(0, list3);
			List<MissionInfo> collection = (
				from x in list
				where x.Type == MissionType.SPECIAL_CONSTRUCT_STN
				select x).ToList<MissionInfo>();
			list.RemoveAll((MissionInfo x) => x.Type == MissionType.SPECIAL_CONSTRUCT_STN);
			list.AddRange(collection);
			foreach (MissionInfo current3 in list)
			{
				this.ProcessMission(current3);
			}
			this.UpdateFleetSupply();
			List<NodeLineInfo> list4 = this.GameDatabase.GetNonPermenantNodeLines().ToList<NodeLineInfo>();
			foreach (NodeLineInfo current4 in list4)
			{
				bool flag = false;
				List<MoveOrderInfo> list5 = this._db.GetMoveOrdersByDestinationSystem(current4.System2ID).ToList<MoveOrderInfo>().Union(this._db.GetMoveOrdersByDestinationSystem(current4.System1ID).ToList<MoveOrderInfo>()).ToList<MoveOrderInfo>();
				foreach (MoveOrderInfo current5 in list5)
				{
					if ((current5.FromSystemID == current4.System1ID || current5.FromSystemID == current4.System2ID) && (current5.ToSystemID == current4.System1ID || current5.ToSystemID == current4.System2ID))
					{
						flag = GameSession.FleetHasBore(this._db, current5.FleetID);
						if (flag)
						{
							break;
						}
					}
				}
				if (!flag)
				{
					current4.Health -= 6;
					if (current4.Health <= 0)
					{
						this.DissolveNodeLine(current4.ID);
					}
					else
					{
						this._db.UpdateNodeLineHealth(current4.ID, current4.Health);
					}
				}
			}
			IEnumerable<StationInfo> stationInfos = this.GameDatabase.GetStationInfos();
			foreach (StationInfo current6 in stationInfos)
			{
				if (current6.DesignInfo.StationType == StationType.DIPLOMATIC && current6.DesignInfo.StationLevel == 5 && this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(current6.PlayerID)) == "zuul")
				{
					SuulkaInfo suulkaByStationID = this.GameDatabase.GetSuulkaByStationID(current6.OrbitalObjectID);
					if (suulkaByStationID == null)
					{
						List<SuulkaInfo> list6 = (
							from x in this.GameDatabase.GetSuulkas().ToList<SuulkaInfo>()
							where (!x.StationID.HasValue || x.StationID == 0) && (!x.PlayerID.HasValue || x.PlayerID == 0)
							select x).ToList<SuulkaInfo>();
						if (list6.Count > 0)
						{
							SuulkaInfo suulkaInfo = list6.ElementAt(new Random().NextInclusive(0, list6.Count - 1));
							this.GameDatabase.UpdateSuulkaStation(suulkaInfo.ID, current6.OrbitalObjectID);
							this.GameDatabase.UpdateSuulkaArrivalTurns(suulkaInfo.ID, new Random().NextInclusive(1, 5));
						}
					}
					else
					{
						if (suulkaByStationID.ArrivalTurns > 0)
						{
							this.GameDatabase.UpdateSuulkaArrivalTurns(suulkaByStationID.ID, suulkaByStationID.ArrivalTurns - 1);
						}
						else
						{
							if (!suulkaByStationID.PlayerID.HasValue)
							{
								this.GameDatabase.UpdateSuulkaArrivalTurns(suulkaByStationID.ID, -1);
								int fleetID = this.InsertSuulkaFleet(current6.PlayerID, suulkaByStationID.ID);
								this._app.GameDatabase.InsertTurnEvent(new TurnEvent
								{
									EventType = TurnEventType.EV_SUULKA_ARRIVES,
									EventMessage = TurnEventMessage.EM_SUULKA_ARRIVES,
									PlayerID = current6.PlayerID,
									SystemID = this.GameDatabase.GetOrbitalObjectInfo(current6.OrbitalObjectID).StarSystemID,
									FleetID = fleetID,
									ShipID = suulkaByStationID.ShipID,
									ShowsDialog = true,
									TurnNumber = this.App.GameDatabase.GetTurnCount()
								});
							}
						}
					}
				}
			}
		}
		public bool CompleteColonizationMission(MissionInfo mission, FleetInfo fleet)
		{
			ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(mission.TargetOrbitalObjectID);
			if (colonyInfoForPlanet != null)
			{
				PlanetInfo planetInfo = this._db.GetPlanetInfo(colonyInfoForPlanet.OrbitalObjectID);
				if (!this._db.CanSupportPlanet(colonyInfoForPlanet.PlayerID, colonyInfoForPlanet.OrbitalObjectID))
				{
					return true;
				}
				bool flag = Kerberos.Sots.Strategy.InhabitedPlanet.Colony.IsColonySelfSufficient(this, colonyInfoForPlanet, planetInfo);
				if (colonyInfoForPlanet.PlayerID == fleet.PlayerID && !flag)
				{
                    Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(this._app.Game, MissionType.SUPPORT, mission.ID, fleet.ID, mission.TargetSystemID, 1, null);
					return false;
				}
				if (flag)
				{
					if (!this._app.GetPlayer(colonyInfoForPlanet.PlayerID).IsAI())
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_COLONY_SELF_SUFFICIENT,
							EventMessage = TurnEventMessage.EM_COLONY_SELF_SUFFICIENT,
							EventSoundCueName = string.Format("STRAT_017-01_{0}_ColonySelf-Sufficient", this._db.GetFactionName(this._db.GetPlayerFactionID(colonyInfoForPlanet.PlayerID))),
							PlayerID = fleet.PlayerID,
							SystemID = mission.TargetSystemID,
							OrbitalID = planetInfo.ID,
							ColonyID = colonyInfoForPlanet.ID,
							FleetID = fleet.ID,
							MissionID = mission.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = true
						});
						return false;
					}
					return true;
				}
			}
			return true;
		}
        public bool CompleteEvecuateMission(MissionInfo mission, FleetInfo fleet)
        {
            Func<ColonyInfo, bool> predicate = null;
            Func<ColonyInfo, double> keySelector = null;
            Func<ColonyInfo, bool> func3 = null;
            Func<ColonyInfo, double> func4 = null;
            int numColonizationShips = Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(this, fleet.ID);
            if (numColonizationShips > 0)
            {
                if (predicate == null)
                {
                    predicate = x => x.PlayerID == fleet.PlayerID;
                }
                List<ColonyInfo> source = this._db.GetColonyInfosForSystem(fleet.SystemID).Where<ColonyInfo>(predicate).ToList<ColonyInfo>();
                if (keySelector == null)
                {
                    keySelector = x => Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxCivilianPop(this._db, this._db.GetPlanetInfo(x.OrbitalObjectID)) - this._db.GetCivilianPopulation(x.OrbitalObjectID, 0, false);
                }
                source.OrderBy<ColonyInfo, double>(keySelector);
                if (source.Count == 0)
                {
                    return true;
                }
                ColonyInfo info = source.Last<ColonyInfo>();
                List<ColonyFactionInfo> list2 = info.Factions.ToList<ColonyFactionInfo>();
                if (list2.Count == 0)
                {
                    return true;
                }
                double num2 = numColonizationShips * this._app.AssetDatabase.EvacCivPerCol;
                num2 /= (double)list2.Count;
                foreach (ColonyFactionInfo info2 in list2)
                {
                    info2.CivilianPop += num2;
                    this._db.UpdateCivilianPopulation(info2);
                }
                if (this._db.GetCivilianPopulation(info.OrbitalObjectID, 0, false) >= (info.CivilianWeight * Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetMaxCivilianPop(this._db, this._db.GetPlanetInfo(info.OrbitalObjectID))))
                {
                    ApplyMoralEvent(this.App, MoralEvent.ME_EVAC_OVERPOPULATION_PLANET, info.PlayerID, new int?(info.ID), null, null);
                }
                if (func3 == null)
                {
                    func3 = x => x.PlayerID == fleet.PlayerID;
                }
                if (func4 == null)
                {
                    func4 = x => this.GameDatabase.GetCivilianPopulation(x.OrbitalObjectID, 0, false);
                }
                source = this._db.GetColonyInfosForSystem(mission.TargetSystemID).Where<ColonyInfo>(func3).OrderBy<ColonyInfo, double>(func4).ToList<ColonyInfo>();
                if ((source.Count > 0) && (this.GameDatabase.GetCivilianPopulation(source.Last<ColonyInfo>().OrbitalObjectID, 0, false) > 0.0))
                {
                    ColonyInfo info3 = source.Last<ColonyInfo>();
                    mission.TargetOrbitalObjectID = info3.OrbitalObjectID;
                    this.GameDatabase.UpdateMission(mission);
                    Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(this._app.Game, MissionType.EVACUATE, mission.ID, fleet.ID, mission.TargetSystemID, 0, null);
                }
            }
            return true;
        }
        public void Phase3_ReactionMovement()
		{
			this._reactions.Clear();
			List<FleetInfo> list = new List<FleetInfo>();
			foreach (FleetInfo current in this._db.GetFleetInfos(FleetType.FL_NORMAL))
			{
				MissionInfo missionByFleetID = this._db.GetMissionByFleetID(current.ID);
				if (missionByFleetID != null && current.SystemID != 0 && missionByFleetID.TargetSystemID == current.SystemID)
				{
					list.Add(current);
				}
			}
			if (list.Count<FleetInfo>() == 0 && !this.App.GameSetup.IsMultiplayer)
			{
				this.Phase4_Combat();
				return;
			}
			foreach (FleetInfo current2 in this._db.GetFleetInfos(FleetType.FL_NORMAL))
			{
				if (current2.SystemID != 0)
				{
					MissionInfo missionByFleetID2 = this._db.GetMissionByFleetID(current2.ID);
					if (current2.AdmiralID != 0 && !current2.IsDefenseFleet && !current2.IsReserveFleet && !current2.IsLimboFleet && current2.Type != FleetType.FL_CARAVAN)
					{
						bool flag = false;
						if (missionByFleetID2 != null)
						{
							if (missionByFleetID2.Type != MissionType.PATROL || missionByFleetID2.TargetSystemID != current2.SystemID)
							{
								continue;
							}
							flag = true;
						}
						Faction faction = this.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(current2.PlayerID));
						int num = 0;
						if (faction.CanUseGate() && this._playerGateMap.TryGetValue(current2.PlayerID, out num))
						{
							int fleetCruiserEquivalent = this._db.GetFleetCruiserEquivalent(current2.ID);
							if (fleetCruiserEquivalent > num)
							{
								continue;
							}
						}
						AdmiralInfo admiralInfo = this._db.GetAdmiralInfo(current2.AdmiralID);
						List<AdmiralInfo.TraitType> list2 = this._db.GetAdmiralTraits(current2.AdmiralID).ToList<AdmiralInfo.TraitType>();
						float num2 = (float)admiralInfo.ReactionBonus * this._db.GetStratModifier<float>(StratModifiers.AdmiralReactionModifier, current2.PlayerID) * (flag ? 1.5f : 1f);
                        float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, current2.ID, false);
                        float fleetTravelSpeed2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, current2.ID, true);
						ReactionInfo reactionInfo = new ReactionInfo();
						reactionInfo.fleetsInRange = new List<FleetInfo>();
						reactionInfo.fleet = current2;
						foreach (FleetInfo current3 in list)
						{
							if (current3.PlayerID != current2.PlayerID && current3.SystemID != current2.SystemID)
							{
								DiplomacyState diplomacyStateBetweenPlayers = this._db.GetDiplomacyStateBetweenPlayers(current2.PlayerID, current3.PlayerID);
								if (StrategicAI.GetDiplomacyStateRank(diplomacyStateBetweenPlayers) < StrategicAI.GetDiplomacyStateRank(DiplomacyState.PEACE))
								{
									float num3 = num2;
									if (list2.Contains(AdmiralInfo.TraitType.Technophobe) && this._db.GetFactionName(this._db.GetPlayerInfo(current3.PlayerID).FactionID) == "loa")
									{
										num3 -= 15f;
									}
									if (num3 >= 0f)
									{
										int num4 = GameSession.ForceReactionHack ? 1 : this._random.Next(1, 100);
										if ((float)num4 <= num3 && StarMap.IsInRange(this._db, current2.PlayerID, this._db.GetFleetLocation(current3.ID, false).Coords, 1f, null))
										{
                                            int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this, current2, current3.SystemID, faction.CanUseNodeLine(new bool?(true)), new float?(fleetTravelSpeed), new float?(fleetTravelSpeed2));
											if (GameSession.ForceReactionHack || (travelTime.HasValue && travelTime.Value <= 1))
											{
												reactionInfo.fleetsInRange.Add(current3);
											}
										}
									}
								}
							}
						}
						if (reactionInfo.fleetsInRange.Count > 0)
						{
							this._reactions.Add(reactionInfo);
						}
					}
				}
			}
			if (!this._app.GameSetup.IsMultiplayer || this._app.Network.IsHosting)
			{
				ReactionInfo nextReactionForPlayer = this.GetNextReactionForPlayer(this._app.LocalPlayer.ID);
				if (nextReactionForPlayer != null)
				{
					this._app.GetGameState<StarMapState>().ShowReactionOverlay(nextReactionForPlayer.fleet.SystemID);
					return;
				}
				if (!this._app.GameSetup.IsMultiplayer)
				{
					this.Phase4_Combat();
				}
			}
		}
		public void Phase4_Combat()
		{
			if (ScriptHost.AllowConsole && GameSession.SkipCombatHack)
			{
				return;
			}
			this.CheckRandomEncounters();
			this.CheckGMEncounters();
			this.CheckSpecialCaseEncounters();
			this.ScriptModules.UpdateEasterEggs(this);
			int num = 3;
			new List<StarSystemInfo>();
			List<int> list = new List<int>();
			List<EncounterInfo> list2 = this._db.GetEncounterInfos().ToList<EncounterInfo>();
			List<AsteroidMonitorInfo> list3 = new List<AsteroidMonitorInfo>();
			List<MorrigiRelicInfo> list4 = new List<MorrigiRelicInfo>();
			foreach (EncounterInfo current in list2)
			{
				if (current.Type == EasterEgg.EE_ASTEROID_MONITOR)
				{
					AsteroidMonitorInfo asteroidMonitorInfo = this._app.GameDatabase.GetAsteroidMonitorInfo(current.Id);
					if (asteroidMonitorInfo != null)
					{
						list3.Add(asteroidMonitorInfo);
					}
				}
				else
				{
					if (current.Type == EasterEgg.EE_MORRIGI_RELIC)
					{
						MorrigiRelicInfo morrigiRelicInfo = this._app.GameDatabase.GetMorrigiRelicInfo(current.Id);
						if (morrigiRelicInfo != null)
						{
							list4.Add(morrigiRelicInfo);
						}
					}
				}
			}
			int num2 = (this._app.Game.ScriptModules.AsteroidMonitor != null) ? this._app.Game.ScriptModules.AsteroidMonitor.PlayerID : 0;
			int num3 = (this._app.Game.ScriptModules.MorrigiRelic != null) ? this._app.Game.ScriptModules.MorrigiRelic.PlayerID : 0;
			List<PirateBaseInfo> source = this._app.GameDatabase.GetPirateBaseInfos().ToList<PirateBaseInfo>();
			foreach (PendingCombat current2 in this.m_Combats)
			{
				if (this._db.GetStarSystemInfo(current2.SystemID) == null)
				{
					current2.SystemID = 0;
				}
			}
			this.m_Combats.RemoveAll((PendingCombat x) => x.SystemID == 0);
			List<StarSystemInfo> list5 = this._db.GetStarSystemInfos().ToList<StarSystemInfo>();
			foreach (StarSystemInfo system in list5)
			{
				if (!system.IsDeepSpace)
				{
					PendingCombat pcom = new PendingCombat();
					pcom.SystemID = system.ID;
					List<int> playersPresent = new List<int>();
					List<int> list6 = new List<int>();
					bool flag = false;
					List<FleetInfo> list7 = this._db.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL | FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
					foreach (FleetInfo current3 in list7)
					{
						if (!current3.IsReserveFleet && this._db.GetShipInfoByFleetID(current3.ID, false).Any<ShipInfo>())
						{
							if (this.IsFleetInterdicted(current3))
							{
								MissionInfo missionByFleetID = this._db.GetMissionByFleetID(current3.ID);
								if (missionByFleetID != null)
								{
									List<int> list8 = this.CheckForInterdiction(current3, current3.SystemID);
									if (!playersPresent.Contains(current3.PlayerID))
									{
										playersPresent.Add(current3.PlayerID);
									}
									foreach (int current4 in list8)
									{
										int playerID = this._db.GetFleetInfo(current4).PlayerID;
										if (!playersPresent.Contains(playerID))
										{
											playersPresent.Add(playerID);
										}
									}
									flag = true;
								}
							}
							else
							{
								if (!playersPresent.Contains(current3.PlayerID))
								{
									bool flag2 = true;
									if (current3.PlayerID == num2)
									{
										AsteroidMonitorInfo asteroidMonitorInfo2 = list3.FirstOrDefault((AsteroidMonitorInfo x) => x.SystemId == system.ID);
										if (asteroidMonitorInfo2 != null && !asteroidMonitorInfo2.IsAggressive)
										{
											flag2 = false;
										}
									}
									else
									{
										if (current3.PlayerID == num3)
										{
											MorrigiRelicInfo morrigiRelicInfo2 = list4.FirstOrDefault((MorrigiRelicInfo x) => x.SystemId == system.ID);
											if (morrigiRelicInfo2 != null && !morrigiRelicInfo2.IsAggressive)
											{
												flag2 = false;
											}
										}
									}
									if (flag2)
									{
										MissionInfo missionByFleetID2 = this._db.GetMissionByFleetID(current3.ID);
										if ((missionByFleetID2 != null && missionByFleetID2.Type != MissionType.INTERDICTION && missionByFleetID2.Type != MissionType.PIRACY) || missionByFleetID2 == null)
										{
											playersPresent.Add(current3.PlayerID);
											flag = true;
										}
									}
								}
							}
						}
					}
					PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(system.ID);
					for (int i = 0; i < starSystemPlanetInfos.Length; i++)
					{
						PlanetInfo planetInfo = starSystemPlanetInfos[i];
						ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(planetInfo.ID);
						if (colonyInfoForPlanet != null && !playersPresent.Contains(colonyInfoForPlanet.PlayerID))
						{
							playersPresent.Add(colonyInfoForPlanet.PlayerID);
						}
						if (colonyInfoForPlanet != null && !list6.Contains(colonyInfoForPlanet.PlayerID))
						{
							list6.Add(colonyInfoForPlanet.PlayerID);
						}
					}
					foreach (StationInfo current5 in this._db.GetStationForSystem(system.ID))
					{
						if (!playersPresent.Contains(current5.PlayerID))
						{
							playersPresent.Add(current5.PlayerID);
						}
						if (!list6.Contains(current5.PlayerID))
						{
							list6.Add(current5.PlayerID);
						}
					}
					bool flag3 = false;
					foreach (FleetInfo current6 in list7)
					{
						if (!playersPresent.Contains(current6.PlayerID))
						{
							MissionInfo missionByFleetID3 = this.GameDatabase.GetMissionByFleetID(current6.ID);
							if (missionByFleetID3 != null && missionByFleetID3.Type == MissionType.PIRACY)
							{
								if (this.GameDatabase.GetPiracyFleetDetectionInfoForFleet(current6.ID).Any((PiracyFleetDetectionInfo x) => playersPresent.Any((int y) => y == x.PlayerID)))
								{
									playersPresent.Add(current6.PlayerID);
									flag3 = true;
									continue;
								}
							}
							if (missionByFleetID3 != null && missionByFleetID3.Type == MissionType.PIRACY)
							{
								flag3 = true;
							}
						}
					}
					List<int> list9 = new List<int>();
					foreach (int current7 in list6)
					{
						PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(current7);
						foreach (int current8 in list6)
						{
							if (current7 != current8 && playerInfo.isStandardPlayer && this._db.ShouldPlayersFight(current7, current8, system.ID) && !list9.Contains(current7))
							{
								list9.Add(current7);
							}
						}
					}
					foreach (int current9 in list9)
					{
						list6.Remove(current9);
					}
					foreach (int current10 in playersPresent)
					{
						foreach (int current11 in playersPresent)
						{
							if (current10 != current11 && this._db.ShouldPlayersFight(current10, current11, system.ID))
							{
								flag3 = true;
								break;
							}
						}
					}
					if (!flag3)
					{
						playersPresent.Clear();
					}
					if (playersPresent.Count<int>() > 1 && flag)
					{
						foreach (FleetInfo current12 in this._db.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL | FleetType.FL_GATE | FleetType.FL_ACCELERATOR))
						{
							if (playersPresent.Contains(current12.PlayerID))
							{
								MissionInfo missionByFleetID4 = this.GameDatabase.GetMissionByFleetID(current12.ID);
								if (missionByFleetID4 != null && missionByFleetID4.Type == MissionType.PIRACY)
								{
									if (!this.GameDatabase.GetPiracyFleetDetectionInfoForFleet(current12.ID).Any((PiracyFleetDetectionInfo x) => playersPresent.Any((int y) => y == x.PlayerID)))
									{
										continue;
									}
								}
								pcom.FleetIDs.Add(current12.ID);
								GameSession.Trace(string.Concat(new object[]
								{
									"Adding fleet ",
									current12.ID,
									" for player ",
									current12.PlayerID
								}));
							}
						}
						PirateBaseInfo pirateBaseInfo = source.FirstOrDefault((PirateBaseInfo x) => x.SystemId == system.ID);
						if (pirateBaseInfo != null)
						{
							pcom.FleetIDs.Add(this._app.Game.ScriptModules.Pirates.SpawnPirateFleet(this._app.Game, system.ID, pirateBaseInfo.NumShips));
						}
						pcom.Type = CombatType.CT_Meeting;
						pcom.PlayersInCombat = playersPresent;
						pcom.NPCPlayersInCombat = this._db.GetNPCPlayersBySystem(pcom.SystemID);
						if (pcom.FleetIDs.Count<int>() > 0)
						{
							int num4 = 0;
							foreach (int player in pcom.PlayersInCombat)
							{
								PlayerInfo playerInfo2 = this._app.GameDatabase.GetPlayerInfo(player);
								if (playerInfo2.isStandardPlayer && !list6.Contains(player))
								{
									if ((
										from x in list7
										where x.PlayerID == player && pcom.FleetIDs.Contains(x.ID)
										select x).Count<FleetInfo>() > num4)
									{
										num4 = (
											from x in list7
											where x.PlayerID == player && pcom.FleetIDs.Contains(x.ID)
											select x).Count<FleetInfo>();
									}
								}
							}
							if (num4 > num)
							{
								num4 = num;
							}
							int num5 = 1;
							bool flag4 = false;
							bool flag5 = false;
							bool flag6 = false;
							bool flag7 = false;
							foreach (int current13 in pcom.FleetIDs)
							{
								FleetInfo fleetInfo5 = this._db.GetFleetInfo(current13);
								PlayerInfo playerInfo3 = this._app.GameDatabase.GetPlayerInfo(fleetInfo5.PlayerID);
								bool flag8 = false;
								if (fleetInfo5.IsGateFleet || fleetInfo5.IsAcceleratorFleet)
								{
									flag4 = true;
								}
								else
								{
									if ((this._app.Game.ScriptModules.Swarmers != null && this._app.Game.ScriptModules.Swarmers.PlayerID == playerInfo3.ID) || (this._app.Game.ScriptModules.MorrigiRelic != null && this._app.Game.ScriptModules.MorrigiRelic.PlayerID == playerInfo3.ID) || (this._app.Game.ScriptModules.AsteroidMonitor != null && this._app.Game.ScriptModules.AsteroidMonitor.PlayerID == playerInfo3.ID))
									{
										flag6 = true;
										flag8 = true;
									}
									if (fleetInfo5.IsNormalFleet && !flag8)
									{
										flag7 = true;
									}
									if (!playerInfo3.isStandardPlayer && !flag5 && !flag8)
									{
										if ((
											from x in this.m_Combats
											where x.SystemID == pcom.SystemID
											select x).Count<PendingCombat>() == 0)
										{
											if (list6.Count > 0)
											{
												PendingCombat pendingCombat = new PendingCombat();
												pendingCombat.FleetIDs = pcom.FleetIDs;
												pendingCombat.NPCPlayersInCombat = pcom.NPCPlayersInCombat;
												pendingCombat.PlayersInCombat = pcom.PlayersInCombat;
												pendingCombat.SystemID = pcom.SystemID;
												pendingCombat.ConflictID = GameSession.GetNextUniqueCombatID();
												pendingCombat.CardID = num5;
												num5++;
												this.m_Combats.Add(pendingCombat);
												flag5 = true;
												continue;
											}
											continue;
										}
									}
									if (!flag8 && !list6.Contains(fleetInfo5.PlayerID) && fleetInfo5.Type != FleetType.FL_GATE && fleetInfo5.Type != FleetType.FL_ACCELERATOR)
									{
										if ((
											from x in this.m_Combats
											where x.SystemID == pcom.SystemID
											select x).Count<PendingCombat>() < num4)
										{
											PendingCombat pendingCombat2 = new PendingCombat();
											pendingCombat2.FleetIDs = pcom.FleetIDs;
											pendingCombat2.NPCPlayersInCombat = pcom.NPCPlayersInCombat;
											pendingCombat2.PlayersInCombat = pcom.PlayersInCombat;
											pendingCombat2.SystemID = pcom.SystemID;
											pendingCombat2.ConflictID = GameSession.GetNextUniqueCombatID();
											pendingCombat2.CardID = num5;
											num5++;
											this.m_Combats.Add(pendingCombat2);
										}
									}
								}
							}
							if ((flag4 || flag6) && flag7)
							{
								if ((
									from x in this.m_Combats
									where x.SystemID == pcom.SystemID
									select x).Count<PendingCombat>() == 0 && list6.Count > 0)
								{
									PendingCombat pendingCombat3 = new PendingCombat();
									pendingCombat3.FleetIDs = pcom.FleetIDs;
									pendingCombat3.NPCPlayersInCombat = pcom.NPCPlayersInCombat;
									pendingCombat3.PlayersInCombat = pcom.PlayersInCombat;
									pendingCombat3.SystemID = pcom.SystemID;
									pendingCombat3.ConflictID = GameSession.GetNextUniqueCombatID();
									pendingCombat3.CardID = num5;
									num5++;
									this.m_Combats.Add(pendingCombat3);
								}
							}
							foreach (PendingCombat current14 in this.m_Combats.ToList<PendingCombat>())
							{
								if (current14.CardID > 1)
								{
									foreach (int current15 in current14.FleetIDs)
									{
										FleetInfo fleetInfo2 = this._app.GameDatabase.GetFleetInfo(current15);
										PlayerInfo playerInfo4 = this._app.GameDatabase.GetPlayerInfo(fleetInfo2.PlayerID);
										if (!playerInfo4.isStandardPlayer)
										{
											this.m_Combats.Remove(current14);
										}
									}
								}
							}
							foreach (int current16 in playersPresent)
							{
								foreach (int current17 in playersPresent)
								{
									if (current16 != current17)
									{
										DiplomacyInfo diplomacyInfo = this.GameDatabase.GetDiplomacyInfo(current16, current17);
										if (diplomacyInfo.State == DiplomacyState.UNKNOWN)
										{
											this.GameDatabase.ChangeDiplomacyState(current16, current17, DiplomacyState.NEUTRAL);
										}
									}
								}
							}
							foreach (int current18 in playersPresent)
							{
								if (!list.Contains(current18))
								{
									list.Add(current18);
								}
							}
							GameSession.Trace("Combat pending in " + system.Name + " system.");
						}
					}
					if (!this.m_Combats.Any((PendingCombat x) => x.SystemID == system.ID))
					{
						foreach (FleetInfo current19 in list7)
						{
							MissionInfo missionByFleetID5 = this.GameDatabase.GetMissionByFleetID(current19.ID);
							if (missionByFleetID5 != null && missionByFleetID5.Type == MissionType.PIRACY)
							{
								playersPresent.Add(current19.PlayerID);
							}
							if (playersPresent.Contains(current19.PlayerID))
							{
								pcom.FleetIDs.Add(current19.ID);
								GameSession.Trace(string.Concat(new object[]
								{
									"PIRACY COMBATS - Adding fleet ",
									current19.ID,
									" for player ",
									current19.PlayerID
								}));
							}
						}
						foreach (FleetInfo current20 in list7)
						{
							MissionInfo missionByFleetID6 = this.GameDatabase.GetMissionByFleetID(current20.ID);
							if (missionByFleetID6 != null && missionByFleetID6.Type == MissionType.PIRACY)
							{
								if (!this.GameDatabase.GetPiracyFleetDetectionInfoForFleet(current20.ID).Any((PiracyFleetDetectionInfo x) => playersPresent.Any((int y) => y == x.PlayerID)))
								{
									List<FreighterInfo> list10 = this.GameDatabase.GetFreighterInfosForSystem(system.ID).ToList<FreighterInfo>();
									if (list10.Count != 0)
									{
										foreach (FreighterInfo current21 in list10)
										{
											if (!playersPresent.Contains(current21.PlayerId))
											{
												playersPresent.Add(current21.PlayerId);
											}
										}
										if (!playersPresent.Contains(current20.PlayerID))
										{
											playersPresent.Add(current20.PlayerID);
										}
										PendingCombat pendingCombat4 = new PendingCombat();
										pendingCombat4.FleetIDs = pcom.FleetIDs;
										pendingCombat4.NPCPlayersInCombat = this._db.GetNPCPlayersBySystem(pcom.SystemID);
										pendingCombat4.PlayersInCombat = playersPresent;
										pendingCombat4.SystemID = pcom.SystemID;
										pendingCombat4.ConflictID = GameSession.GetNextUniqueCombatID();
										pendingCombat4.CardID = 1;
										pendingCombat4.Type = CombatType.CT_Piracy;
										this.m_Combats.Add(pendingCombat4);
										break;
									}
								}
							}
						}
					}
				}
			}
			List<int> list11 = this.App.GameDatabase.GetStandardPlayerIDs().ToList<int>();
			foreach (int current22 in list11)
			{
				float num6 = this._db.GetStratModifier<float>(StratModifiers.OddsOfRandomEncounter, current22);
				if (list.Contains(current22))
				{
					this.App.GameDatabase.UpdatePlayerLastCombatTurn(current22, this.App.GameDatabase.GetTurnCount());
					num6 -= this.App.AssetDatabase.RandomEncDecOddsCombat * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f);
					this.App.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, current22, Math.Max(num6, 0f));
				}
				else
				{
					num6 += this.App.AssetDatabase.RandomEncIncOddsRounds * (float)(this.m_Combats.Count - 1);
					num6 += this.App.AssetDatabase.RandomEncIncOddsIdle * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f);
					this.App.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, current22, Math.Max(0f, num6));
				}
			}
			if (this.m_Combats.Count != 0)
			{
				this.App.GameDatabase.SetLastTurnWithCombat(this.App.GameDatabase.GetTurnCount());
			}
			int lastTurnWithCombat = this.App.GameDatabase.GetLastTurnWithCombat();
			if (this.App.AssetDatabase.RandomEncTurnsToResetOdds <= this.App.GameDatabase.GetTurnCount() - lastTurnWithCombat)
			{
				foreach (int current23 in list11)
				{
					this.App.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, current23, this.App.AssetDatabase.RandomEncBaseOdds * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f));
				}
			}
			if (ScriptHost.AllowConsole)
			{
				foreach (PendingCombat current24 in this.m_Combats)
				{
					foreach (int current25 in current24.FleetIDs)
					{
						if (this._db.GetFleetInfo(current25) == null)
						{
							GameSession.Warn("Fleet " + current25 + " does not exist!");
						}
					}
				}
			}
			List<int> list12 = new List<int>();
			List<ColonyTrapInfo> list13 = this.GameDatabase.GetColonyTrapInfos().ToList<ColonyTrapInfo>();
			foreach (ColonyTrapInfo cti in list13)
			{
				if (!list12.Contains(cti.SystemID))
				{
					list12.Add(cti.SystemID);
					if (!this.m_Combats.Any((PendingCombat x) => x.SystemID == cti.SystemID))
					{
						Dictionary<int, int> dictionary = new Dictionary<int, int>();
						Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
						List<int> list14 = new List<int>();
						List<int> list15 = new List<int>();
						List<ColonyTrapInfo> source2 = (
							from x in list13
							where x.SystemID == cti.SystemID
							select x).ToList<ColonyTrapInfo>();
						List<FleetInfo> source3 = (
							from x in list13
							select this._db.GetFleetInfo(x.FleetID)).ToList<FleetInfo>();
						List<FleetInfo> list16 = this._db.GetFleetInfoBySystemID(cti.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
						foreach (FleetInfo current26 in list16)
						{
							PlayerInfo pi = this._db.GetPlayerInfo(current26.PlayerID);
							if (pi != null && pi.isStandardPlayer)
							{
								Faction faction = this._app.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.ID == pi.FactionID);
								if (faction != null && !(faction.Name == "morrigi"))
								{
									list14.Add(current26.ID);
									if (!list15.Contains(current26.PlayerID))
									{
										list15.Add(current26.PlayerID);
									}
									MissionInfo mi = this._db.GetMissionByFleetID(current26.ID);
									if (mi != null && mi.Type == MissionType.COLONIZATION)
									{
										ColonyTrapInfo targetTrap = source2.FirstOrDefault((ColonyTrapInfo x) => x.PlanetID == mi.TargetOrbitalObjectID);
										if (targetTrap != null)
										{
											FleetInfo fleetInfo3 = source3.FirstOrDefault((FleetInfo x) => x.ID == targetTrap.FleetID);
											if (fleetInfo3 != null && fleetInfo3.PlayerID != current26.PlayerID && this._db.GetDiplomacyStateBetweenPlayers(fleetInfo3.PlayerID, current26.PlayerID) != DiplomacyState.ALLIED && !dictionary.Keys.Contains(current26.ID))
											{
												dictionary.Add(current26.ID, fleetInfo3.ID);
												dictionary2.Add(targetTrap.ID, fleetInfo3.PlayerID);
												list14.Add(fleetInfo3.ID);
												if (!list15.Contains(fleetInfo3.PlayerID))
												{
													list15.Add(fleetInfo3.PlayerID);
												}
											}
										}
									}
								}
							}
						}
						if (dictionary.Count > 0)
						{
							PendingCombat pendingCombat5 = new PendingCombat();
							pendingCombat5.FleetIDs = list14;
							pendingCombat5.Type = CombatType.CT_Colony_Trap;
							pendingCombat5.NPCPlayersInCombat = new List<int>();
							pendingCombat5.PlayersInCombat = list15;
							pendingCombat5.SystemID = cti.SystemID;
							pendingCombat5.ConflictID = GameSession.GetNextUniqueCombatID();
							pendingCombat5.CardID = 1;
							this.m_Combats.Add(pendingCombat5);
						}
					}
				}
			}
			this.fleetsInCombat.Clear();
			List<int> list17 = this.GameDatabase.GetStandardPlayerIDs().ToList<int>();
			foreach (PendingCombat current27 in this.m_Combats)
			{
				foreach (int current28 in list17)
				{
					if (!current27.PlayersInCombat.Contains(current28) && StarMap.IsInRange(this.GameDatabase, current28, current27.SystemID))
					{
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_COMBAT_DETECTED,
							EventMessage = TurnEventMessage.EM_COMBAT_DETECTED,
							PlayerID = current28,
							SystemID = current27.SystemID,
							TurnNumber = this.App.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
			foreach (PendingCombat current29 in this.m_Combats)
			{
				if (current29.Type != CombatType.CT_Colony_Trap)
				{
					bool flag9 = true;
					List<FleetInfo> list18 = new List<FleetInfo>();
					int num7 = 0;
					int num8 = 0;
					int num9 = 0;
					foreach (int playerId in current29.PlayersInCombat)
					{
						Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerId);
						if (playerObject == null || !playerObject.IsStandardPlayer || !playerObject.IsAI())
						{
							flag9 = false;
						}
						FleetInfo selectedFleetInfo = (
							from fleetId in current29.FleetIDs
							select this._db.GetFleetInfo(fleetId)).FirstOrDefault((FleetInfo fleetInfo) => fleetInfo != null && fleetInfo.PlayerID == playerId && fleetInfo.IsNormalFleet);
						if (selectedFleetInfo != null)
						{
							if (playerObject.Faction.Name == "loa")
							{
                                Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, selectedFleetInfo.ID, MissionType.NO_MISSION);
								selectedFleetInfo = this._app.GameDatabase.GetFleetInfo(selectedFleetInfo.ID);
							}
							list18.Add(selectedFleetInfo);
							if (this.fleetsInCombat.Any((FleetInfo x) => x.ID == selectedFleetInfo.ID))
							{
								this.SubtractExtendedCombatEndurance(selectedFleetInfo);
							}
							else
							{
								this.fleetsInCombat.Add(selectedFleetInfo);
							}
						}
						if (this.ScriptModules != null && this.ScriptModules.IsEncounterPlayer(playerObject.ID))
						{
							num7++;
						}
						else
						{
							num8++;
							if (playerObject.IsAI())
							{
								num9++;
							}
						}
					}
					List<FleetInfo> list19 = this._db.GetFleetInfoBySystemID(current29.SystemID, FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
					foreach (FleetInfo current30 in list19)
					{
						if (!list18.Contains(current30))
						{
							list18.Add(current30);
						}
					}
					if (num8 == 1 && num8 == num9)
					{
						if (num7 <= 0)
						{
							if (this._db.GetColonyInfosForSystem(current29.SystemID).Count((ColonyInfo x) => x.IsIndependentColony(this._app)) <= 0)
							{
								goto IL_1DB0;
							}
						}
						if (CombatSimulatorRandoms.Simulate(this, current29.SystemID, list18))
						{
							current29.complete = true;
							continue;
						}
					}
					IL_1DB0:
					if (flag9)
					{
						CombatSimulator.Simulate(this, current29.SystemID, list18);
						current29.complete = true;
					}
				}
			}
			this.m_Combats.RemoveAll((PendingCombat x) => x.complete);
			List<int> assignedFleets = new List<int>();
			foreach (PendingCombat current31 in this.m_Combats)
			{
				foreach (int playerId in current31.PlayersInCombat)
				{
					Kerberos.Sots.PlayerFramework.Player playerObject2 = this.GetPlayerObject(playerId);
					if (playerObject2.Faction.Name == "loa")
					{
						foreach (int current32 in current31.FleetIDs)
						{
							FleetInfo fleetInfo4 = this.App.GameDatabase.GetFleetInfo(current32);
                            if (fleetInfo4 != null && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this, fleetInfo4) && !fleetInfo4.IsAcceleratorFleet && fleetInfo4.PlayerID == playerId)
							{
								if (this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo4.ID, false).Any((ShipInfo x) => !x.ShipSystemPosition.HasValue))
								{
                                    Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, fleetInfo4.ID, MissionType.NO_MISSION);
								}
							}
						}
					}
					if (playerObject2.IsAI())
					{
						current31.CombatResolutionSelections[playerId] = ResolutionType.AUTO_RESOLVE;
						current31.CombatStanceSelections[playerId] = AutoResolveStance.AGGRESSIVE;
						IEnumerable<FleetInfo> source4 = 
							from x in current31.FleetIDs
							select this._db.GetFleetInfo(x) into x
							where x.PlayerID == playerId
							select x;
						IEnumerable<FleetInfo> source5 = 
							from x in source4
							where !assignedFleets.Contains(x.ID)
							select x;
						FleetInfo bestCombatFleet;
						if (source5.Any<FleetInfo>())
						{
							bestCombatFleet = this.GetBestCombatFleet(source5.ToList<FleetInfo>());
						}
						else
						{
							bestCombatFleet = this.GetBestCombatFleet(source4.ToList<FleetInfo>());
						}
						if (bestCombatFleet != null)
						{
							current31.SelectedPlayerFleets[playerId] = bestCombatFleet.ID;
							if (!assignedFleets.Contains(bestCombatFleet.ID))
							{
								assignedFleets.Add(bestCombatFleet.ID);
							}
						}
						else
						{
							current31.SelectedPlayerFleets[playerId] = 0;
						}
					}
				}
			}
			if (this.App.GameSetup.IsMultiplayer)
			{
				foreach (Kerberos.Sots.PlayerFramework.Player current33 in this.m_Players)
				{
					if (current33.IsAI())
					{
						this.App.Network.SendCombatResponses(this.m_Combats, current33.ID);
					}
				}
			}
			if (this.m_Combats.Any((PendingCombat x) => x.CombatResults == null && x.PlayersInCombat.Contains(this.LocalPlayer.ID)))
			{
				EncounterDialog dialog = new EncounterDialog(this._app, (
					from x in this.m_Combats
					where x.PlayersInCombat.Contains(this.LocalPlayer.ID)
					select x).ToList<PendingCombat>());
				this.App.UI.CreateDialog(dialog, null);
				return;
			}
			if (this.App.GameSetup.IsMultiplayer)
			{
				this.App.Game.ShowCombatDialog(true, null);
				return;
			}
			if (this.m_Combats.Count<PendingCombat>() > 0)
			{
				this.LaunchNextCombat();
				return;
			}
			this.NextTurn();
		}
		private float scoreFleet(FleetInfo fleet)
		{
			float num = 0f;
			float num2 = 0f;
			int num3 = 0;
			int num4 = 0;
			List<ShipInfo> list = this._db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				List<SectionInstanceInfo> list2 = this._db.GetShipSectionInstances(current.ID).ToList<SectionInstanceInfo>();
				if (list2.Count > 0)
				{
					using (List<SectionInstanceInfo>.Enumerator enumerator2 = list2.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							SectionInstanceInfo current2 = enumerator2.Current;
							num += (float)current2.Structure;
						}
						goto IL_AA;
					}
					goto IL_9B;
				}
				goto IL_9B;
				IL_AA:
				num2 += current.DesignInfo.Structure;
				num3 += CombatAI.GetShipStrength(current.DesignInfo.Class);
				DesignSectionInfo[] designSections = current.DesignInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					LogicalBank[] banks = designSectionInfo.ShipSectionAsset.Banks;
					for (int j = 0; j < banks.Length; j++)
					{
						LogicalBank logicalBank = banks[j];
						switch (logicalBank.TurretClass)
						{
						case WeaponEnums.TurretClasses.Standard:
							num4++;
							break;
						case WeaponEnums.TurretClasses.Missile:
							num4 += 3;
							break;
						case WeaponEnums.TurretClasses.IOBM:
						case WeaponEnums.TurretClasses.PolarisMissile:
							num4 += 4;
							break;
						case WeaponEnums.TurretClasses.FreeBeam:
						case WeaponEnums.TurretClasses.Spinal:
						case WeaponEnums.TurretClasses.Strafe:
							num4 += 2;
							break;
						case WeaponEnums.TurretClasses.HeavyBeam:
							num4 += 5;
							break;
						case WeaponEnums.TurretClasses.Torpedo:
							num4 += 5;
							break;
						case WeaponEnums.TurretClasses.Drone:
							num4 += 6;
							break;
						case WeaponEnums.TurretClasses.DestroyerRider:
							num4 += 9;
							break;
						case WeaponEnums.TurretClasses.CruiserRider:
							num4 += 12;
							break;
						case WeaponEnums.TurretClasses.DreadnoughtRider:
							num4 += 15;
							break;
						}
					}
				}
				continue;
				IL_9B:
				num += current.DesignInfo.Structure;
				goto IL_AA;
			}
			FleetTemplate templateForFleet = StrategicAI.GetTemplateForFleet(this, fleet.PlayerID, fleet.ID);
			float num5 = 10f * Math.Min(num / num2, 1f);
			float num6 = 20f * Math.Min((float)num3 / 50f, 1f);
			Math.Min((float)num4 / 200f, 1f);
			float num7 = 0f;
			if (templateForFleet != null)
			{
				if (templateForFleet.Name == "DEFAULT_COMBAT" || templateForFleet.Name == "SMALL_COMBAT")
				{
					num7 = 1f;
				}
				else
				{
					if (templateForFleet.Name == "DEFAULT_PATROL")
					{
						num7 = 0.75f;
					}
					else
					{
						if (templateForFleet.Name == "DEFAULT_INVASION")
						{
							num7 = 0.5f;
						}
						else
						{
							if (templateForFleet.Name == "DEFAULT_SURVEY")
							{
								num7 = 0.25f;
							}
						}
					}
				}
			}
			num7 *= 40f;
			return num5 + num6 + num7;
		}
		private FleetInfo GetBestCombatFleet(List<FleetInfo> fleets)
		{
			FleetInfo result = null;
			float num = 0f;
			foreach (FleetInfo current in fleets)
			{
				if (current.IsNormalFleet)
				{
					float num2 = this.scoreFleet(current);
					if (num2 > num)
					{
						result = current;
						num = num2;
					}
				}
			}
			return result;
		}
		public void SubtractExtendedCombatEndurance(FleetInfo fleet)
		{
			if (!this.IsFleetInSupplyRange(fleet.ID))
			{
                fleet.SupplyRemaining -= Kerberos.Sots.StarFleet.StarFleet.GetSupplyConsumption(this, fleet.ID) * 0.5f;
				this._db.UpdateFleetInfo(fleet);
			}
		}
		public void Phase5_Results()
		{
			List<MissionInfo> source = this._db.GetMissionInfos().ToList<MissionInfo>();
			List<MissionInfo> list = (
				from x in source
				where x.Type == MissionType.GATE && x.Duration == 0
				select x).ToList<MissionInfo>();
			foreach (MissionInfo current in list)
			{
				this.ProcessMission(current);
			}
			List<MissionInfo> list2 = (
				from x in source
				where x.Type == MissionType.SURVEY
				select x).ToList<MissionInfo>();
			foreach (MissionInfo current2 in list2)
			{
				WaypointInfo nextWaypointForMission = this._db.GetNextWaypointForMission(current2.ID);
				if (nextWaypointForMission != null && nextWaypointForMission.Type == WaypointType.DoMission)
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(current2.FleetID);
					if (fleetInfo != null && this.isHostilesAtSystem(fleetInfo.PlayerID, fleetInfo.SystemID))
					{
                        Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleetInfo, true);
					}
				}
			}
			List<FleetInfo> list3 = new List<FleetInfo>();
			foreach (FleetInfo current3 in this._db.GetFleetInfos(FleetType.FL_NORMAL))
			{
				List<ShipInfo> list4 = this._db.GetShipInfoByFleetID(current3.ID, true).ToList<ShipInfo>();
				if (list4.Count == 0)
				{
					list3.Add(current3);
				}
				else
				{
					int num = 0;
					foreach (ShipInfo current4 in list4)
					{
						if (current4.ParentID == 0 && current4.DesignInfo.DesignSections[0].ShipSectionAsset.Class == ShipClass.BattleRider)
						{
							num++;
						}
					}
					if (num == list4.Count)
					{
						foreach (ShipInfo current5 in list4)
						{
							this._db.RemoveShip(current5.ID);
						}
						list3.Add(current3);
					}
				}
			}
			foreach (FleetInfo current6 in list3)
			{
				this.FleetLeftSystem(current6, current6.SystemID, GameSession.FleetLeaveReason.KILLED);
				this._db.RemoveFleet(current6.ID);
				GameTrigger.PushEvent(EventType.EVNT_FLEETDIED, current6.Name, this);
				this._db.RemoveAdmiral(current6.AdmiralID);
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (PlayerInfo current7 in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				dictionary.Add(current7.ID, this._db.GetPlayerColoniesByPlayerId(current7.ID).Count<ColonyInfo>());
			}
			List<PlayerInfo> list5 = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current8 in list5)
			{
				foreach (PlayerInfo current9 in list5)
				{
					if (current8.ID != current9.ID && this._db.GetDiplomacyInfo(current8.ID, current9.ID).isEncountered)
					{
						int num2 = dictionary[current8.ID];
						int num3 = dictionary[current9.ID];
						if (num3 > num2)
						{
							this._db.ApplyDiplomacyReaction(current8.ID, current9.ID, StratModifiers.DiplomacyReactionBiggerEmpire, 1);
						}
						else
						{
							if (num3 < num2)
							{
								this._db.ApplyDiplomacyReaction(current8.ID, current9.ID, StratModifiers.DiplomacyReactionSmallerEmpire, 1);
							}
						}
					}
				}
			}
			foreach (PlayerInfo current10 in list5)
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(current10.ID);
				IEnumerable<PlayerTechInfo> playerTechInfos = this._db.GetPlayerTechInfos(playerObject.ID);
				int num4 = 0;
				foreach (PlayerTechInfo current11 in playerTechInfos)
				{
					num4 += current11.Progress;
				}
				int num5 = playerObject._techPointsAtStartOfTurn / 5000;
				int num6 = num4 / 5000;
				int num7 = num6 - num5;
				if (num7 > 0)
				{
					foreach (PlayerInfo current12 in list5)
					{
						int arg_4E3_0 = playerObject.ID;
						int arg_4E2_0 = current12.ID;
					}
				}
			}
			List<CombatData> list6 = this.CombatData.GetCombats(this._db.GetTurnCount()).ToList<CombatData>();
			foreach (CombatData current13 in list6)
			{
				foreach (int current14 in this.App.GameDatabase.GetStandardPlayerIDs())
				{
					PlayerCombatData player = current13.GetPlayer(current14);
					if (player != null)
					{
						if (player.VictoryStatus == GameSession.VictoryStatus.Win)
						{
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_COMBAT_WIN,
								EventMessage = TurnEventMessage.EM_COMBAT_WIN,
								TurnNumber = this._db.GetTurnCount(),
								PlayerID = current14,
								SystemID = current13.SystemID,
								CombatID = current13.CombatID,
								ShowsDialog = true
							});
						}
						else
						{
							if (player.VictoryStatus == GameSession.VictoryStatus.Loss)
							{
								this.App.GameDatabase.InsertTurnEvent(new TurnEvent
								{
									EventType = TurnEventType.EV_COMBAT_LOSS,
									EventMessage = TurnEventMessage.EM_COMBAT_LOSS,
									TurnNumber = this._db.GetTurnCount(),
									PlayerID = current14,
									SystemID = current13.SystemID,
									CombatID = current13.CombatID,
									ShowsDialog = true
								});
							}
							else
							{
								if (player.VictoryStatus == GameSession.VictoryStatus.Draw)
								{
									this.App.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_COMBAT_DRAW,
										EventMessage = TurnEventMessage.EM_COMBAT_DRAW,
										TurnNumber = this._db.GetTurnCount(),
										PlayerID = current14,
										SystemID = current13.SystemID,
										CombatID = current13.CombatID,
										ShowsDialog = true
									});
								}
							}
						}
					}
				}
			}
		}
		public static void AbandonColony(App game, int colonyID)
		{
			ColonyInfo colonyInfo = game.GameDatabase.GetColonyInfo(colonyID);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
			game.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_COLONY_ABANDONED,
				EventMessage = TurnEventMessage.EM_COLONY_ABANDONED,
				PlayerID = game.LocalPlayer.ID,
				SystemID = orbitalObjectInfo.StarSystemID,
				OrbitalID = orbitalObjectInfo.ID,
				ColonyID = colonyInfo.ID,
				TurnNumber = game.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			game.GameDatabase.RemoveColonyOnPlanet(colonyInfo.OrbitalObjectID);
			if (colonyInfo != null)
			{
				GameSession.ApplyMoralEvent(game, MoralEvent.ME_ABANDON_COLONY, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, new int?(colonyInfo.CachedStarSystemID));
			}
		}
		public GameSession.VictoryStatus GetPlayerVictoryStatus(int playerId, int systemId)
		{
			List<Kerberos.Sots.PlayerFramework.Player> source = GameSession.GetPlayersWithCombatAssets(this.App, systemId).ToList<Kerberos.Sots.PlayerFramework.Player>();
			bool flag = source.Any((Kerberos.Sots.PlayerFramework.Player x) => this.GameDatabase.GetDiplomacyStateBetweenPlayers(x.ID, playerId) == DiplomacyState.WAR);
			bool flag2 = source.Any((Kerberos.Sots.PlayerFramework.Player x) => x.ID == playerId || this.GameDatabase.GetDiplomacyStateBetweenPlayers(x.ID, playerId) == DiplomacyState.ALLIED);
			if (flag && flag2)
			{
				return GameSession.VictoryStatus.Draw;
			}
			if (flag2)
			{
				return GameSession.VictoryStatus.Win;
			}
			return GameSession.VictoryStatus.Loss;
		}
		public static IEnumerable<Kerberos.Sots.PlayerFramework.Player> GetPlayersWithCombatAssets(App Game, int systemId)
		{
			List<Kerberos.Sots.PlayerFramework.Player> list = new List<Kerberos.Sots.PlayerFramework.Player>();
			List<StationInfo> list2 = Game.GameDatabase.GetStationForSystem(systemId).ToList<StationInfo>();
			foreach (StationInfo current in list2)
			{
				if (!list.Contains(Game.GetPlayer(current.PlayerID)))
				{
					list.Add(Game.GetPlayer(current.PlayerID));
				}
			}
			List<ColonyInfo> list3 = Game.GameDatabase.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>();
			foreach (ColonyInfo current2 in list3)
			{
				if (!list.Contains(Game.GetPlayer(current2.PlayerID)))
				{
					list.Add(Game.GetPlayer(current2.PlayerID));
				}
			}
			List<FleetInfo> list4 = Game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL | FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
			foreach (FleetInfo current3 in list4)
			{
				if (!list.Contains(Game.GetPlayer(current3.PlayerID)))
				{
					list.Add(Game.GetPlayer(current3.PlayerID));
				}
			}
			return list;
		}
		public static bool PlayerPresentInSystem(GameDatabase db, int playerID, int systemID)
		{
			bool flag = db.GetPlayerColonySystemIDs(playerID).Contains(systemID);
			bool flag2 = db.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).Count<FleetInfo>() > 0;
			return flag || flag2;
		}
		public Dictionary<EasterEgg, int> GetAvailableGMOdds()
		{
			Dictionary<EasterEgg, int> dictionary = new Dictionary<EasterEgg, int>();
			foreach (KeyValuePair<EasterEgg, int> current in this._app.AssetDatabase.GMOdds)
			{
				if (GameSession.IsEasterEggAvailable(this.GameDatabase, current.Key) && (current.Key == EasterEgg.GM_COMET || this._db.GetNumEncountersOfType(current.Key) == 0))
				{
					dictionary.Add(current.Key, current.Value);
				}
			}
			return dictionary;
		}
		public Dictionary<EasterEgg, int> GetAvailableEEOdds()
		{
			Dictionary<EasterEgg, int> dictionary = new Dictionary<EasterEgg, int>();
			foreach (KeyValuePair<EasterEgg, int> current in this._app.AssetDatabase.EasterEggOdds)
			{
				if (GameSession.IsEasterEggAvailable(this.GameDatabase, current.Key))
				{
					dictionary.Add(current.Key, current.Value);
				}
			}
			return dictionary;
		}
		public static bool IsEasterEggAvailable(GameDatabase db, EasterEgg ee)
		{
			return (ee != EasterEgg.GM_NEUTRON_STAR && ee != EasterEgg.GM_GARDENER && ee != EasterEgg.EE_PIRATE_BASE) || db.HasEndOfFleshExpansion();
		}
		public Dictionary<RandomEncounter, int> GetAvailableRandomOdds(PlayerInfo player)
		{
			Dictionary<RandomEncounter, int> dictionary = new Dictionary<RandomEncounter, int>();
			foreach (KeyValuePair<RandomEncounter, int> current in this._app.AssetDatabase.RandomEncounterOdds)
			{
				if (GameSession.IsRandomAvailable(this._db, player, current.Key))
				{
					dictionary.Add(current.Key, current.Value);
				}
			}
			return dictionary;
		}
		public static bool IsRandomAvailable(GameDatabase db, PlayerInfo player, RandomEncounter rand)
		{
			if (rand == RandomEncounter.RE_SPECTORS)
			{
				FactionInfo factionInfo = db.GetFactionInfo(player.FactionID);
				return factionInfo.Name != "loa";
			}
			return true;
		}
		public static bool CanPlayerSupportDefenseAssets(GameDatabase db, int playerID, int systemID)
		{
			bool flag = db.GetPlayerColonySystemIDs(playerID).Contains(systemID);
			bool flag2 = db.GetStationForSystemPlayerAndType(systemID, playerID, StationType.NAVAL) != null;
			return flag || flag2;
		}
		public void CheckGMEncounters()
		{
			string nameValue = this.GameDatabase.GetNameValue("GMCount");
			if (string.IsNullOrEmpty(nameValue))
			{
				this.GameDatabase.InsertNameValuePair("GMCount", "0");
				nameValue = this.GameDatabase.GetNameValue("GMCount");
			}
			int num = int.Parse(nameValue);
			Random safeRandom = App.GetSafeRandom();
			if (num < this._db.GetNameValue<int>("GSGrandMenaceCount") && this.GameDatabase.GetTurnCount() > this.AssetDatabase.GrandMenaceMinTurn && safeRandom.CoinToss(this.AssetDatabase.GrandMenaceChance))
			{
				Dictionary<EasterEgg, int> availableGMOdds = this.GetAvailableGMOdds();
				int num2 = availableGMOdds.Sum((KeyValuePair<EasterEgg, int> x) => x.Value);
				if (num2 == 0)
				{
					return;
				}
				int num3 = this._random.Next(num2);
				int num4 = 0;
				EasterEgg easterEgg = EasterEgg.EE_SWARM;
				foreach (KeyValuePair<EasterEgg, int> current in availableGMOdds)
				{
					num4 += current.Value;
					if (num4 > num3)
					{
						easterEgg = current.Key;
						break;
					}
				}
				switch (easterEgg)
				{
				case EasterEgg.GM_SYSTEM_KILLER:
					if (this.ScriptModules.SystemKiller != null)
					{
						this.ScriptModules.SystemKiller.AddInstance(this.GameDatabase, this.AssetDatabase, null);
					}
					break;
				case EasterEgg.GM_LOCUST_SWARM:
					if (this.ScriptModules.Locust != null)
					{
						this.ScriptModules.Locust.AddInstance(this.GameDatabase, this.AssetDatabase, null);
					}
					break;
				case EasterEgg.GM_COMET:
					if (this.ScriptModules.Comet != null)
					{
						this.ScriptModules.Comet.AddInstance(this.GameDatabase, this.AssetDatabase, null);
					}
					break;
				case EasterEgg.GM_NEUTRON_STAR:
					if (this.ScriptModules.NeutronStar != null)
					{
						this.ScriptModules.NeutronStar.AddInstance(this.GameDatabase, this.AssetDatabase, null);
					}
					break;
				case EasterEgg.GM_GARDENER:
					if (this.ScriptModules.Gardeners != null)
					{
						this.ScriptModules.Gardeners.AddInstance(this.GameDatabase, this.AssetDatabase, 0);
					}
					break;
				}
				if (easterEgg != EasterEgg.GM_COMET)
				{
					GameDatabase arg_279_0 = this.GameDatabase;
					string arg_279_1 = "GMCount";
					int num5;
					num = (num5 = num + 1);
					arg_279_0.UpdateNameValuePair(arg_279_1, num5.ToString());
				}
			}
			List<IncomingGMInfo> list = this.GameDatabase.GetIncomingGMsThisTurn().ToList<IncomingGMInfo>();
			foreach (IncomingGMInfo current2 in list)
			{
				switch (current2.type)
				{
				case EasterEgg.GM_SYSTEM_KILLER:
					if (this.ScriptModules.SystemKiller != null)
					{
						this.ScriptModules.SystemKiller.ExecuteInstance(this.GameDatabase, this.AssetDatabase, current2.SystemId);
					}
					break;
				case EasterEgg.GM_LOCUST_SWARM:
					if (this.ScriptModules.Locust != null)
					{
						this.ScriptModules.Locust.ExecuteInstance(this.GameDatabase, this.AssetDatabase, current2.SystemId);
					}
					break;
				case EasterEgg.GM_COMET:
					if (this.ScriptModules.Comet != null)
					{
						this.ScriptModules.Comet.ExecuteInstance(this.GameDatabase, this.AssetDatabase, current2.SystemId);
					}
					break;
				case EasterEgg.GM_NEUTRON_STAR:
					if (this.ScriptModules.NeutronStar != null)
					{
						this.ScriptModules.NeutronStar.ExecuteInstance(this.GameDatabase, this.AssetDatabase, current2.SystemId);
					}
					break;
				case EasterEgg.GM_SUPER_NOVA:
					if (this.ScriptModules.SuperNova != null)
					{
						this.ScriptModules.SuperNova.ExecuteInstance(this, this.GameDatabase, this.AssetDatabase, current2.SystemId);
					}
					break;
				}
				this.GameDatabase.RemoveIncomingGM(current2.ID);
			}
		}
		public void CheckRandomEncounters()
		{
			if (this.ScriptModules.Spectre != null)
			{
				this.ScriptModules.Spectre.UpdateTurn(this);
			}
			if (this.ScriptModules.MeteorShower != null)
			{
				this.ScriptModules.MeteorShower.UpdateTurn(this);
			}
			if (this.ScriptModules.Slaver != null)
			{
				this.ScriptModules.Slaver.UpdateTurn(this);
			}
			if (this.ScriptModules.Pirates != null)
			{
				this.ScriptModules.Pirates.UpdateTurn(this);
			}
			if (this.ScriptModules.GhostShip != null)
			{
				this.ScriptModules.GhostShip.UpdateTurn(this);
			}
			if (this.ScriptModules.Comet != null)
			{
				this.ScriptModules.Comet.UpdateTurn(this);
			}
			if (Spectre.ForceEncounter && this.ScriptModules.Spectre != null)
			{
				this.ScriptModules.Spectre.AddEncounter(this, this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID), null);
			}
			if (Slaver.ForceEncounter && this.ScriptModules.Slaver != null)
			{
				this.ScriptModules.Slaver.AddEncounter(this, this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID));
			}
			if (GhostShip.ForceEncounter && this.ScriptModules.GhostShip != null)
			{
				this.ScriptModules.GhostShip.AddEncounter(this, this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID));
			}
			if (MeteorShower.ForceEncounter && this.ScriptModules.MeteorShower != null)
			{
				this.ScriptModules.MeteorShower.AddEncounter(this, this.GameDatabase.GetPlayerInfo(this.LocalPlayer.ID), null);
			}
			if (this.App.GameDatabase.GetTurnCount() > this.App.AssetDatabase.RandomEncMinTurns)
			{
				using (List<PlayerInfo>.Enumerator enumerator = this.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PlayerInfo current = enumerator.Current;
						Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(current.ID);
						if ((playerObject == null || !playerObject.IsAI()) && current.LastEncounterTurn <= this.App.GameDatabase.GetTurnCount() - this.App.AssetDatabase.RandomEncTurnsToExclude)
						{
							float num = this._db.GetStratModifier<float>(StratModifiers.OddsOfRandomEncounter, current.ID) * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f);
							num = Math.Max(Math.Min(num, this.App.AssetDatabase.RandomEncMaxOdds), this.App.AssetDatabase.RandomEncMinOdds);
							if (this._random.CoinToss((double)num))
							{
								this._app.GameDatabase.UpdateLastEncounterTurn(current.ID, this._app.GameDatabase.GetTurnCount());
								this._app.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, current.ID, num - this._app.AssetDatabase.RandomEncDecOddsCombat * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f));
								Dictionary<RandomEncounter, int> availableRandomOdds = this.GetAvailableRandomOdds(current);
								int num2 = availableRandomOdds.Sum((KeyValuePair<RandomEncounter, int> x) => x.Value);
								if (num2 == 0)
								{
									return;
								}
								int num3 = this._random.Next(num2);
								int num4 = 0;
								RandomEncounter randomEncounter = RandomEncounter.RE_ASTEROID_SHOWER;
								foreach (KeyValuePair<RandomEncounter, int> current2 in availableRandomOdds)
								{
									num4 += current2.Value;
									if (num4 > num3)
									{
										randomEncounter = current2.Key;
										break;
									}
								}
								switch (randomEncounter)
								{
								case RandomEncounter.RE_ASTEROID_SHOWER:
									if (this.ScriptModules.MeteorShower != null)
									{
										this.ScriptModules.MeteorShower.AddEncounter(this, current, null);
									}
									break;
								case RandomEncounter.RE_SPECTORS:
									this.ScriptModules.Spectre.AddEncounter(this, current, null);
									break;
								case RandomEncounter.RE_SLAVERS:
									if (this.ScriptModules.Slaver != null)
									{
										this.ScriptModules.Slaver.AddEncounter(this, current);
									}
									break;
								case RandomEncounter.RE_GHOST_SHIP:
									if (this.GameDatabase.GetFactionName(current.FactionID) != "zuul")
									{
										this.ScriptModules.GhostShip.AddEncounter(this, current);
									}
									break;
								}
							}
						}
					}
					goto IL_517;
				}
			}
			foreach (PlayerInfo current3 in this.App.GameDatabase.GetStandardPlayerInfos())
			{
				this._app.GameDatabase.SetStratModifier(StratModifiers.OddsOfRandomEncounter, current3.ID, this._app.AssetDatabase.RandomEncBaseOdds * (this._db.GetNameValue<float>("RandomEncounterFrequency") / 100f));
			}
			IL_517:
			foreach (PlayerInfo current4 in this.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				List<IncomingRandomInfo> list = this.GameDatabase.GetIncomingRandomsForPlayerThisTurn(current4.ID).ToList<IncomingRandomInfo>();
				foreach (IncomingRandomInfo current5 in list)
				{
					switch (current5.type)
					{
					case RandomEncounter.RE_ASTEROID_SHOWER:
						if (this.ScriptModules.MeteorShower != null)
						{
							this.ScriptModules.MeteorShower.ExecuteEncounter(this, current5.SystemId, 1f, false);
						}
						break;
					case RandomEncounter.RE_SPECTORS:
						this.ScriptModules.Spectre.ExecuteEncounter(this, current4, current5.SystemId, false);
						break;
					case RandomEncounter.RE_SLAVERS:
						if (this.ScriptModules.Slaver != null)
						{
							this.ScriptModules.Slaver.ExecuteEncounter(this, current4, current5.SystemId);
						}
						break;
					case RandomEncounter.RE_GHOST_SHIP:
						if (this.GameDatabase.GetFactionName(current4.FactionID) != "zuul")
						{
							this.ScriptModules.GhostShip.ExecuteEncounter(this, current4, current5.SystemId);
						}
						break;
					}
					this.GameDatabase.RemoveIncomingRandom(current5.ID);
				}
			}
		}
		private void CheckSpecialCaseEncounters()
		{
			if (this.App.GameDatabase.HasEndOfFleshExpansion())
			{
				if (this.ScriptModules.NeutronStar != null)
				{
					NeutronStar.GenerateMeteorAndCometEncounters(this.App);
				}
				if (this.ScriptModules.SuperNova != null)
				{
					SuperNova.AddSuperNovas(this.App.Game, this.App.GameDatabase, this.App.AssetDatabase);
				}
			}
		}
		private void AddAsteroidShower(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}
		private void AddFlyingDutchman(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}
		private void AddPirates(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}
		private void AddRefugees(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}
		private void AddSlavers(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}
		private void AddSpectors(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}
		private void AddVonNeumann(PlayerInfo pi)
		{
			this.App.GameDatabase.GetPlayerColonySystemIDs(pi.ID).ToList<int>();
		}
		public void CheckTriggers(TurnStage ts)
		{
			List<Trigger> list = new List<Trigger>(
				from x in this.ActiveTriggers
				orderby GameTrigger.GetTriggerTriggeredDepth(this.ActiveTriggers, x)
				select x);
			foreach (Trigger current in list)
			{
				if (GameTrigger.Evaluate(current.Context, this, current))
				{
					IEnumerable<TriggerCondition> enumerable = 
						from x in current.Conditions
						where x.isEventDriven
						select x;
					IEnumerable<TriggerCondition> enumerable2 = 
						from x in current.Conditions
						where !x.isEventDriven
						select x;
					bool flag = true;
					foreach (TriggerCondition current2 in enumerable2)
					{
						if (!GameTrigger.Evaluate(current2, this, current))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						int num = (enumerable.Count<TriggerCondition>() == 0) ? 1 : 2147483647;
						foreach (TriggerCondition current3 in enumerable)
						{
							int num2 = 0;
							List<EventStub> triggerEvents = this.TriggerEvents.ToList<EventStub>();
							while (GameTrigger.Evaluate(current3, this, current))
							{
								num2++;
							}
							num = Math.Min(num2, num);
							this.TriggerEvents = triggerEvents;
						}
						if (num != 2147483647)
						{
							for (int i = 0; i < num; i++)
							{
								foreach (TriggerAction current4 in current.Actions)
								{
									GameTrigger.Evaluate(current4, this, current);
								}
							}
							if (!current.IsRecurring)
							{
								this.ActiveTriggers.Remove(current);
							}
							GameTrigger.PushEvent(EventType.EVNT_TRIGGERTRIGGERED, current.Name, this);
						}
					}
				}
			}
			GameTrigger.ClearEvents(this);
		}
		public bool LaunchNextCombat()
		{
			if (this.m_Combats.Count == 0 && this._currentCombat == null)
			{
				return false;
			}
			if (this._currentCombat == null)
			{
				this._currentCombat = this.m_Combats.First((PendingCombat x) => x.CombatResults == null);
				foreach (int current in this._currentCombat.SelectedPlayerFleets.Values)
				{
					FleetInfo fi = this._db.GetFleetInfo(current);
					if (fi != null)
					{
						if (this.fleetsInCombat.Any((FleetInfo x) => x.ID == fi.ID))
						{
							this.SubtractExtendedCombatEndurance(fi);
						}
						else
						{
							this.fleetsInCombat.Add(fi);
						}
					}
				}
				if (this._currentCombat != null)
				{
					foreach (int current2 in this._currentCombat.PlayersInCombat)
					{
						int num = current2 - 1;
						if (this.App.GameSetup.Players.Count > num)
						{
							this.App.GameSetup.Players[num].Status = NPlayerStatus.PS_COMBAT;
						}
					}
					bool sim = true;
					if (this._currentCombat.CombatResolutionSelections.ContainsKey(this.App.LocalPlayer.ID) && this._currentCombat.CombatResolutionSelections[this.App.LocalPlayer.ID] == ResolutionType.FIGHT)
					{
						sim = false;
					}
					this.LaunchCombat(this._currentCombat, false, sim, true);
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(this._currentCombat.SystemID);
					GameSession.Trace("Launching combat in " + starSystemInfo.Name + " system.");
				}
			}
			return true;
		}
		public void LaunchCombat(PendingCombat pendingCombat, bool testing, bool sim, bool authority)
		{
			this._currentCombat = pendingCombat;
			this._app.LaunchCombat(this._app.Game, pendingCombat, testing, sim, authority);
		}
		public static float GetSystemToSystemDistance(GameDatabase _db, int system1, int system2)
		{
			StarSystemInfo starSystemInfo = _db.GetStarSystemInfo(system1);
			StarSystemInfo starSystemInfo2 = _db.GetStarSystemInfo(system2);
			return (starSystemInfo.Origin - starSystemInfo2.Origin).Length;
		}
		private bool UpdateLinearMove(MoveOrderInfo moveOrder, FleetInfo fi, bool usePermNodeLines, bool useTempNodeLines, bool useFarcast, ref float remainingNodeDistance)
		{
            float num = (usePermNodeLines || useTempNodeLines) ? remainingNodeDistance : Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this, moveOrder.FleetID, false);
			float num2;
			if (moveOrder.FromSystemID != 0 && moveOrder.ToSystemID != 0)
			{
				if (this._app.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(fi.PlayerID)).CanUseGravityWell() && !this._db.FleetHasCurvatureComp(fi))
				{
					num2 = GameSession.GetGravityWellTravelDistance(this._db, moveOrder);
				}
				else
				{
					num2 = GameSession.GetSystemToSystemDistance(this.App.Game.GameDatabase, moveOrder.FromSystemID, moveOrder.ToSystemID);
				}
			}
			else
			{
				Vector3 v;
				if (moveOrder.FromSystemID != 0)
				{
					v = this._db.GetStarSystemInfo(moveOrder.FromSystemID).Origin;
				}
				else
				{
					v = moveOrder.FromCoords;
				}
				Vector3 v2;
				if (moveOrder.ToSystemID != 0)
				{
					v2 = this._db.GetStarSystemInfo(moveOrder.ToSystemID).Origin;
				}
				else
				{
					v2 = moveOrder.ToCoords;
				}
				num2 = (v2 - v).Length;
			}
			if (num2 > 0f)
			{
				float num3 = num / num2;
				moveOrder.Progress += num3;
				int systemID = fi.SystemID;
				if (fi.SystemID != 0)
				{
					this._db.UpdateFleetLocation(fi.ID, 0, new int?(fi.SystemID));
					if (useFarcast)
					{
						float hiverCastingDistance = this._db.GetHiverCastingDistance(fi.PlayerID);
						if (hiverCastingDistance >= this._db.GetStratModifier<float>(StratModifiers.GateCastDistance, fi.PlayerID))
						{
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_FARCAST_SUCCESS,
								EventMessage = TurnEventMessage.EM_FARCAST_SUCCESS,
								PlayerID = fi.PlayerID,
								FleetID = fi.ID,
								TurnNumber = this.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
						else
						{
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_FARCAST_FAILED,
								EventMessage = TurnEventMessage.EM_FARCAST_FAILED,
								PlayerID = fi.PlayerID,
								FleetID = fi.ID,
								Savings = (double)(this._db.GetStratModifier<float>(StratModifiers.GateCastDistance, fi.PlayerID) - hiverCastingDistance),
								TurnNumber = this.App.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
						Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(moveOrder.FromSystemID);
						Vector3 starSystemOrigin2 = this._db.GetStarSystemOrigin(moveOrder.ToSystemID);
						float length = (starSystemOrigin2 - starSystemOrigin).Length;
						if (length <= hiverCastingDistance)
						{
							moveOrder.Progress = 1.1f;
						}
						else
						{
							moveOrder.Progress = hiverCastingDistance / length;
						}
					}
					this.FleetLeftSystem(fi, systemID, GameSession.FleetLeaveReason.TRAVEL);
				}
				remainingNodeDistance -= Math.Min(remainingNodeDistance, num2);
			}
			else
			{
				if (moveOrder.FromSystemID == moveOrder.ToSystemID)
				{
					moveOrder.Progress = 1.1f;
					return true;
				}
			}
			if (moveOrder.Progress > 1f)
			{
				return true;
			}
			if (num2 == 0f)
			{
				moveOrder.Progress = 1.1f;
				return true;
			}
			this._db.UpdateMoveOrder(moveOrder.ID, moveOrder.Progress);
			return false;
		}
		public void FleetLeftSystem(FleetInfo fleet, int systemID, GameSession.FleetLeaveReason reason = GameSession.FleetLeaveReason.TRAVEL)
		{
			if (systemID == 0)
			{
				return;
			}
			if (fleet != null)
			{
				if (!GameSession.PlayerPresentInSystem(this.GameDatabase, fleet.PlayerID, systemID))
				{
					Kerberos.Sots.GameStates.StarSystem.RemoveSystemPlayerColor(this.GameDatabase, systemID, fleet.PlayerID);
				}
				this.GameDatabase.UpdateFleetPreferred(fleet.ID, false);
			}
		}
		private void FinishMove(MoveOrderInfo moveOrder, FleetInfo fi)
		{
			if (moveOrder.ToSystemID != 0)
			{
				this.ArriveAtSystem(moveOrder, fi);
			}
			this._db.RemoveMoveOrder(moveOrder.ID);
		}
		public void InsertNewMonitorSpecialProject(int playerid, int encounterid, int fleetid)
		{
			Random random = new Random();
			FleetLocation fleetLocation = this.App.GameDatabase.GetFleetLocation(fleetid, false);
			string str = "";
			if (fleetLocation.SystemID != 0)
			{
				StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(fleetLocation.SystemID);
				str = starSystemInfo.Name;
			}
			int cost = random.NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumAsteroidMonitorStudy, this.App.AssetDatabase.SpecialProjectData.MaximumAsteroidMonitorStudy);
			int specialProjectID = this._db.InsertSpecialProject(playerid, App.Localize("@EVENTMSG_SPECPRJ_ASTEROIDMONITOR") + " - " + str, cost, SpecialProjectType.AsteroidMonitor, 0, encounterid, fleetid, 0);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = playerid,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = specialProjectID,
				ShowsDialog = false
			});
		}
		public void InsertNewIndependentPlanetResearchProject(int PlayerID, int TargetPlayerID)
		{
			Random random = new Random();
			int cost = random.NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumIndyInvestigate, this.App.AssetDatabase.SpecialProjectData.MaximumIndyInvestigate);
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(TargetPlayerID);
			int specialProjectID = this._db.InsertSpecialProject(PlayerID, string.Format(App.Localize("@EVENTMSG_SPECPRJ_INVESTIGATEINDEPENDENT"), playerInfo.Name), cost, SpecialProjectType.IndependentStudy, 0, 0, 0, TargetPlayerID);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = PlayerID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = specialProjectID,
				ShowsDialog = false
			});
		}
		public void InsertNewRadiationShieldResearchProject(int PlayerID)
		{
			if (this._db.GetHasPlayerStudiedSpecialProject(PlayerID, SpecialProjectType.RadiationShielding) || this._db.GetHasPlayerStudyingSpecialProject(PlayerID, SpecialProjectType.RadiationShielding))
			{
				return;
			}
			Random random = new Random();
			int cost = random.NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumRadiationShieldingStudy, this.App.AssetDatabase.SpecialProjectData.MaximumRadiationShieldingStudy);
			int specialProjectID = this._db.InsertSpecialProject(PlayerID, App.Localize("@EVENTMSG_SPECPRJ_RADIATIONSHIELDING"), cost, SpecialProjectType.RadiationShielding, 0, 0, 0, 0);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = PlayerID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = specialProjectID,
				ShowsDialog = false
			});
		}
		public void InsertNewNeutronStarResearchProject(int PlayerID, int encounterId)
		{
			if (this._db.GetHasPlayerStudiedSpecialProject(PlayerID, SpecialProjectType.NeutronStar) || this._db.GetHasPlayerStudyingSpecialProject(PlayerID, SpecialProjectType.NeutronStar))
			{
				return;
			}
			Random random = new Random();
			int cost = random.NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumRadiationShieldingStudy, this.App.AssetDatabase.SpecialProjectData.MaximumRadiationShieldingStudy);
			int specialProjectID = this._db.InsertSpecialProject(PlayerID, App.Localize("@EVENTMSG_SPECPRJ_NEUTRONSTAR"), cost, SpecialProjectType.NeutronStar, 0, encounterId, 0, 0);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = PlayerID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = specialProjectID,
				ShowsDialog = false
			});
		}
		public void InsertNewGardenerResearchProject(int PlayerID, int encounterId)
		{
			if (this._db.GetHasPlayerStudiedSpecialProject(PlayerID, SpecialProjectType.Gardener) || this._db.GetHasPlayerStudyingSpecialProject(PlayerID, SpecialProjectType.Gardener))
			{
				return;
			}
			Random random = new Random();
			int cost = random.NextInclusive(this.App.AssetDatabase.SpecialProjectData.MinimumRadiationShieldingStudy, this.App.AssetDatabase.SpecialProjectData.MaximumRadiationShieldingStudy);
			int specialProjectID = this._db.InsertSpecialProject(PlayerID, App.Localize("@EVENTMSG_SPECPRJ_GARDENER"), cost, SpecialProjectType.Gardener, 0, encounterId, 0, 0);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_NEW_SPECIAL_PROJECT,
				EventMessage = TurnEventMessage.EM_NEW_SPECIAL_PROJECT,
				PlayerID = PlayerID,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				SpecialProjectID = specialProjectID,
				ShowsDialog = false
			});
		}
		private static int GetNewBoreLineHealth(GameDatabase db, int playerid)
		{
			return (int)(450f * GameSession.GetBoreHealthMultiplier(db, playerid) + (float)new Random().NextInclusive(0, 50));
		}
		private static int GetNewUnstableBoreLineHealth(GameDatabase db, int playerid)
		{
			return (int)(20f * GameSession.GetBoreHealthMultiplier(db, playerid) + (float)new Random().NextInclusive(0, 30));
		}
		public static int GetPlayerSystemBoreLineLimit(GameDatabase db, int playerid)
		{
			if (!db.PlayerHasTech(playerid, "DRV_Radiant_Drive"))
			{
				return 4;
			}
			return 5;
		}
		public List<MoveOrderInfo> GetMoveOrdersBetweenSystems(int system1, int system2)
		{
			List<MoveOrderInfo> list = new List<MoveOrderInfo>();
			IEnumerable<MoveOrderInfo> enumerable = this._db.GetMoveOrdersByDestinationSystem(system1).ToList<MoveOrderInfo>().Union(this._db.GetMoveOrdersByDestinationSystem(system2).ToList<MoveOrderInfo>());
			foreach (MoveOrderInfo current in enumerable)
			{
				if ((current.FromSystemID == system1 || current.FromSystemID == system2) && (current.ToSystemID == system1 || current.ToSystemID == system2))
				{
					list.Add(current);
				}
			}
			return list;
		}
		private void DissolveNodeLine(int lineID)
		{
			NodeLineInfo nodeLine = this._db.GetNodeLine(lineID);
			if (nodeLine == null)
			{
				return;
			}
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(nodeLine.System1ID);
			StarSystemInfo starSystemInfo2 = this._db.GetStarSystemInfo(nodeLine.System2ID);
			IEnumerable<PlayerInfo> playerInfos = this._db.GetPlayerInfos();
			foreach (PlayerInfo current in playerInfos)
			{
				bool flag = this._app.GameDatabase.GetFactionName(current.FactionID) == "zuul" && (StarMap.IsInRange(this.GameDatabase, current.ID, starSystemInfo, null) || StarMap.IsInRange(this.GameDatabase, current.ID, starSystemInfo2, null));
				if (flag)
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_NODE_LINE_COLLAPSE,
						EventMessage = TurnEventMessage.EM_NODE_LINE_COLLAPSE,
						PlayerID = current.ID,
						SystemID = nodeLine.System1ID,
						SystemID2 = nodeLine.System2ID,
						ShowsDialog = false,
						TurnNumber = this.App.GameDatabase.GetTurnCount()
					});
				}
			}
			List<MoveOrderInfo> list = this._db.GetMoveOrdersByDestinationSystem(nodeLine.System2ID).ToList<MoveOrderInfo>().Union(this._db.GetMoveOrdersByDestinationSystem(nodeLine.System1ID).ToList<MoveOrderInfo>()).ToList<MoveOrderInfo>();
			foreach (MoveOrderInfo current2 in list)
			{
				if ((current2.FromSystemID == nodeLine.System1ID || current2.FromSystemID == nodeLine.System2ID) && (current2.ToSystemID == nodeLine.System1ID || current2.ToSystemID == nodeLine.System2ID))
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(current2.FleetID);
					string factionName = this._db.GetFactionName(this._db.GetPlayerFactionID(fleetInfo.PlayerID));
					if (factionName == "zuul")
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_NODE_LINE_COLLAPSE_FLEET_LOSS,
							EventMessage = TurnEventMessage.EM_NODE_LINE_COLLAPSE_FLEET_LOSS,
							PlayerID = fleetInfo.PlayerID,
							ShowsDialog = false,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
						List<ShipInfo> list2 = this._db.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>();
						foreach (ShipInfo current3 in list2)
						{
							this._db.RemoveShip(current3.ID);
						}
						this._db.RemoveFleet(fleetInfo.ID);
					}
				}
			}
			this._db.RemoveNodeLine(lineID);
		}
		private void ArriveAtSystem(MoveOrderInfo moveOrder, FleetInfo fleet)
		{
			if (moveOrder.ToSystemID == 0)
			{
				throw new InvalidDataException("moveOrder.ToSystemID cannot be 0 when arriving at a system.");
			}
			fleet.PreviousSystemID = new int?(moveOrder.FromSystemID);
			fleet.SystemID = moveOrder.ToSystemID;
			this._db.UpdateFleetLocation(moveOrder.FleetID, moveOrder.ToSystemID, new int?(moveOrder.FromSystemID));
			GameSession.Trace(string.Concat(new object[]
			{
				"Fleet ",
				moveOrder.FleetID,
				" has arrived at system ",
				moveOrder.ToSystemID
			}));
			if (this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerInfo(fleet.PlayerID).FactionID).CanUseAccelerators())
			{
				if (this._db.GetFleetsByPlayerAndSystem(fleet.PlayerID, fleet.SystemID, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
				{
					this._db.UpdateFleetAccelerated(this, fleet.ID, null);
				}
				else
				{
					this._db.UpdateFleetAccelerated(this, fleet.ID, new int?(-10));
				}
			}
			NodeLineInfo nodeLineInfo = GameSession.GetSystemsNonPermenantNodeLine(this._db, moveOrder.FromSystemID, moveOrder.ToSystemID);
            if (!Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, fleet) && GameSession.FleetHasBore(this._app.GameDatabase, fleet.ID))
			{
				if (nodeLineInfo != null)
				{
					this._db.UpdateNodeLineHealth(nodeLineInfo.ID, GameSession.GetNewBoreLineHealth(this._db, fleet.PlayerID));
				}
				else
				{
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(moveOrder.FromSystemID);
					if (starSystemInfo != null && !starSystemInfo.IsDeepSpace)
					{
						List<NodeLineInfo> list = (
							from x in this._db.GetExploredNodeLinesFromSystem(fleet.PlayerID, fleet.SystemID)
							where x.Health > -1
							select x).ToList<NodeLineInfo>();
						if (list.Count >= GameSession.GetPlayerSystemBoreLineLimit(this._db, fleet.PlayerID))
						{
							nodeLineInfo = this._db.GetNodeLine(this._db.InsertNodeLine(moveOrder.FromSystemID, moveOrder.ToSystemID, GameSession.GetNewUnstableBoreLineHealth(this._db, fleet.PlayerID)));
						}
						else
						{
							nodeLineInfo = this._db.GetNodeLine(this._db.InsertNodeLine(moveOrder.FromSystemID, moveOrder.ToSystemID, GameSession.GetNewBoreLineHealth(this._db, fleet.PlayerID)));
						}
					}
				}
			}
			else
			{
                if (!Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, fleet) && nodeLineInfo != null && this.AssetDatabase.GetFaction(this._db.GetPlayerInfo(fleet.PlayerID).FactionID).CanUseNodeLine(new bool?(false)))
				{
					int num = (int)(((float)this._db.GetFleetCommandPointCost(fleet.ID) + 4f) / 5f);
					nodeLineInfo.Health -= num;
					this._db.UpdateNodeLineHealth(nodeLineInfo.ID, nodeLineInfo.Health);
				}
			}
			int turnCount = this._db.GetTurnCount();
			if (this._db.GetLastTurnExploredByPlayer(fleet.PlayerID, moveOrder.ToSystemID) != 0)
			{
				this._db.InsertExploreRecord(moveOrder.ToSystemID, fleet.PlayerID, turnCount, true, true);
			}
			IEnumerable<ShipInfo> shipInfoByFleetID = this._db.GetShipInfoByFleetID(fleet.ID, false);
			foreach (ShipInfo current in shipInfoByFleetID)
			{
				this._db.UpdateShipSystemPosition(current.ID, null);
			}
		}
		public List<StarSystemInfo> GetClosestGates(int playerId, StarSystemInfo targetSystem, float MaxDist)
		{
			IEnumerable<StarSystemInfo> source = 
				from x in this._db.GetStarSystemInfos()
				where (x.Origin - targetSystem.Origin).Length <= MaxDist && this._db.SystemHasGate(x.ID, playerId)
				select x into y
				orderby (y.Origin - targetSystem.Origin).Length
				select y;
			return source.ToList<StarSystemInfo>();
		}
		public int RequestSuulkaFleet(int playerID, int systemID)
		{
			IEnumerable<SuulkaInfo> playerSuulkas = this.GameDatabase.GetPlayerSuulkas(new int?(0));
			if (playerSuulkas.Count<SuulkaInfo>() > 0)
			{
				SuulkaInfo suulkaInfo = playerSuulkas.ElementAt(new Random().NextInclusive(0, playerSuulkas.Count<SuulkaInfo>() - 1));
				this.GameDatabase.UpdateSuulkaPlayer(suulkaInfo.ID, playerID);
				DesignInfo designInfo = this.GameDatabase.GetDesignInfo(this.GameDatabase.GetShipInfo(suulkaInfo.ShipID, false).DesignID);
				int num = this.GameDatabase.InsertFleet(playerID, suulkaInfo.AdmiralID, systemID, systemID, designInfo.Name, FleetType.FL_NORMAL);
				this.GameDatabase.TransferShip(suulkaInfo.ShipID, num);
				AdmiralInfo admiralInfo = this.GameDatabase.GetAdmiralInfo(suulkaInfo.AdmiralID);
				admiralInfo.PlayerID = playerID;
				this.GameDatabase.UpdateAdmiralInfo(admiralInfo);
				designInfo.PlayerID = playerID;
				this.GameDatabase.UpdateDesign(designInfo);
				return num;
			}
			return 0;
		}
		public int InsertSuulkaFleet(int playerID, int suulkaID)
		{
			SuulkaInfo suulka = this.GameDatabase.GetSuulka(suulkaID);
			StationInfo stationInfo = suulka.StationID.HasValue ? this.GameDatabase.GetStationInfo(suulka.StationID.Value) : null;
			StarSystemInfo starSystemInfo = this.GameDatabase.GetStarSystemInfo(this.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).StarSystemID);
			this.GameDatabase.UpdateSuulkaPlayer(suulka.ID, playerID);
			DesignInfo designInfo = this.GameDatabase.GetDesignInfo(this.GameDatabase.GetShipInfo(suulka.ShipID, false).DesignID);
			int num = this.GameDatabase.InsertFleet(playerID, suulka.AdmiralID, starSystemInfo.ID, starSystemInfo.ID, designInfo.Name, FleetType.FL_NORMAL);
			this.GameDatabase.TransferShip(suulka.ShipID, num);
			designInfo.PlayerID = playerID;
			GameSession.InsertDefaultSuulkaEquipmentInstances(this.App, designInfo, suulka.ShipID, playerID, num);
			return num;
		}
        private static void InsertDefaultSuulkaEquipmentInstances(App game, DesignInfo design, int shipId, int playerId, int fleetId)
        {
            if (design != null)
            {
                string name = game.GameDatabase.GetPlayerFaction(playerId).Name;
                List<LogicalWeapon> availableWeapons = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, playerId).ToList<LogicalWeapon>();
                List<SectionInstanceInfo> list2 = game.GameDatabase.GetShipSectionInstances(shipId).ToList<SectionInstanceInfo>();
                Func<ShipSectionAsset, bool> predicate = null;
                for (int i = 0; i < list2.Count; i++)
                {
                    List<WeaponBankInfo> list3 = new List<WeaponBankInfo>();
                    List<DesignModuleInfo> list4 = new List<DesignModuleInfo>();
                    if (predicate == null)
                    {
                        predicate = x => x.FileName == design.DesignSections[i].FilePath;
                    }
                    ShipSectionAsset sectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>(predicate);
                    list3.AddRange(DesignLab.ChooseWeapons(game.Game, availableWeapons, ShipRole.COMMAND, WeaponRole.BRAWLER, sectionAsset, playerId, null).ToList<WeaponBankInfo>());
                    Func<LogicalModule, bool> func = null;
                    foreach (LogicalModuleMount lm in sectionAsset.Modules)
                    {
                        if (string.IsNullOrEmpty(lm.AssignedModuleName))
                        {
                            continue;
                        }
                        if (func == null)
                        {
                            func = x => x.ModuleName == lm.AssignedModuleName;
                        }
                        LogicalModule module = game.AssetDatabase.Modules.FirstOrDefault<LogicalModule>(func);
                        if (module != null)
                        {
                            int moduleID = game.GameDatabase.GetModuleID(module.ModulePath, playerId);
                            if (moduleID == 0)
                            {
                                moduleID = game.GameDatabase.InsertModule(module, playerId);
                            }
                            int? weaponID = null;
                            int? nullable2 = null;
                            LogicalBank bank = module.Banks.FirstOrDefault<LogicalBank>();
                            if (bank != null)
                            {
                                string defaultWeaponName = bank.DefaultWeaponName;
                                LogicalWeapon weapon = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>(x => x.WeaponName == defaultWeaponName);
                                if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
                                {
                                    if (weapon == null)
                                    {
                                        weapon = LogicalWeapon.EnumerateWeaponFits(sectionAsset.Faction, sectionAsset.SectionName, game.AssetDatabase.Weapons, bank.TurretSize, bank.TurretClass).FirstOrDefault<LogicalWeapon>();
                                    }
                                    ShipRole uNDEFINED = ShipRole.UNDEFINED;
                                    WeaponRole wpnRole = WeaponRole.UNDEFINED;
                                    switch (bank.TurretClass)
                                    {
                                        case WeaponEnums.TurretClasses.Biomissile:
                                            uNDEFINED = ShipRole.BIOMISSILE;
                                            wpnRole = WeaponRole.PLANET_ATTACK;
                                            break;

                                        case WeaponEnums.TurretClasses.Drone:
                                            uNDEFINED = ShipRole.DRONE;
                                            wpnRole = WeaponRole.BRAWLER;
                                            break;

                                        case WeaponEnums.TurretClasses.AssaultShuttle:
                                            uNDEFINED = ShipRole.ASSAULTSHUTTLE;
                                            wpnRole = WeaponRole.PLANET_ATTACK;
                                            break;

                                        case WeaponEnums.TurretClasses.BoardingPod:
                                            uNDEFINED = ShipRole.BOARDINGPOD;
                                            wpnRole = WeaponRole.DISABLING;
                                            break;
                                    }
                                    nullable2 = DesignLab.ChooseBattleRider(game.Game, uNDEFINED, wpnRole, playerId);
                                }
                                if (weapon != null)
                                {
                                    weaponID = game.GameDatabase.GetWeaponID(weapon.FileName, playerId);
                                    if (!weaponID.HasValue)
                                    {
                                        weaponID = new int?(game.GameDatabase.InsertWeapon(weapon, playerId));
                                    }
                                }
                                else
                                {
                                    weaponID = DesignLab.PickBestWeaponForRole(game.Game, LogicalWeapon.EnumerateWeaponFits(sectionAsset.Faction, sectionAsset.SectionName, availableWeapons, bank.TurretSize, bank.TurretClass).ToList<LogicalWeapon>(), playerId, WeaponRole.BRAWLER, null, bank.TurretClass, name);
                                }
                            }
                            DesignModuleInfo info = new DesignModuleInfo
                            {
                                DesignSectionInfo = design.DesignSections[i],
                                MountNodeName = lm.NodeName,
                                ModuleID = moduleID,
                                PsionicAbilities = new List<ModulePsionicInfo>(),
                                WeaponID = weaponID,
                                DesignID = nullable2
                            };
                            info.ID = game.GameDatabase.InsertDesignModule(info);
                            game.GameDatabase.InsertModuleInstance(lm, module, list2[i].ID);
                            list4.Add(info);
                        }
                    }
                    design.DesignSections[i].WeaponBanks = list3;
                    design.DesignSections[i].Modules = list4;
                }
                DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design);
                game.GameDatabase.UpdateDesign(design);
                if (design != null)
                {
                    game.Game.AddDefaultStartingRiders(fleetId, design.ID, shipId);
                }
            }
        }
        public static string GetBestEngineTechString(App game, int playerId)
		{
			string result = "Unknown";
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(playerId));
			List<PlayerTechInfo> source = new List<PlayerTechInfo>(
				from x in game.GameDatabase.GetPlayerTechInfos(playerId)
				where x.State == TechStates.Researched
				select x);
			string key;
			switch (key = factionName)
			{
			case "human":
				result = App.Localize("@TECH_DRIVE_NODE_FOCUS");
				if (source.Any((PlayerTechInfo x) => x.TechFileID == "DRV_Node_Pathing"))
				{
					result = App.Localize("@TECH_DRIVE_NODE_PATHING");
				}
				break;
			case "tarkas":
				result = App.Localize("@TECH_DRIVE_HYPER_FIELD");
				if (source.Any((PlayerTechInfo x) => x.TechFileID == "DRV_Warp"))
				{
					result = App.Localize("@TECH_DRIVE_WARP");
				}
				break;
			case "zuul":
				result = App.Localize("@TECH_DRIVE_REND_DRIVE");
				if (source.Any((PlayerTechInfo x) => x.TechFileID == "DRV_Radiant_Drive"))
				{
					result = App.Localize("@TECH_DRIVE_RADIANT_DRIVE");
				}
				break;
			case "morrigi":
				result = App.Localize("@TECH_DRIVE_VOID_CARVER");
				if (source.Any((PlayerTechInfo x) => x.TechFileID == "DRV_Void_Mastery"))
				{
					result = App.Localize("@TECH_DRIVE_VOID_MASTERY");
				}
				break;
			case "liir_zuul":
				result = App.Localize("@TECH_DRIVE_STUTTER_WARP");
				if (source.Any((PlayerTechInfo x) => x.TechFileID == "DRV_Flicker_Warp"))
				{
					result = App.Localize("@TECH_DRIVE_FLICKER_WARP");
				}
				break;
			case "hiver":
				result = App.Localize("@TECH_DRIVE_GATE");
				if (source.Any((PlayerTechInfo x) => x.TechFileID == "DRV_Casting"))
				{
					result = App.Localize("@TECH_DRIVE_CASTING");
				}
				if (source.Any((PlayerTechInfo x) => x.TechFileID == "DRV_Far_Casting"))
				{
					result = App.Localize("@TECH_DRIVE_FAR_CASTING");
				}
				break;
			case "loa":
				result = App.Localize("@TECH_DRIVE_NEUTRINO_PULSE_ACCELERATORS");
				if (source.Any((PlayerTechInfo x) => x.TechFileID == "DRV_Standing_Neutrino_Waves"))
				{
					result = App.Localize("@TECH_DRIVE_STANDING_NEUTRINO_WAVES");
				}
				break;
			}
			return result;
		}
		public static string GetBestEnginePlantTechString(App game, int playerId)
		{
			string result = App.Localize("@TECH_DRIVE_FUSION");
			List<PlayerTechInfo> source = new List<PlayerTechInfo>(
				from x in game.GameDatabase.GetPlayerTechInfos(playerId)
				where x.State == TechStates.Researched
				select x);
			if (source.Any((PlayerTechInfo x) => x.TechFileID == "NRG_Anti-Matter"))
			{
				result = App.Localize("@TECH_DRIVE_ANTIMATTER");
			}
			return result;
		}
		public static float GetGravityWellTravelDistance(GameDatabase gamedb, MoveOrderInfo moveOrder)
		{
			if (moveOrder.FromSystemID != 0 && moveOrder.FromSystemID == moveOrder.ToSystemID)
			{
				return 0f;
			}
			Vector3 vector;
			if (moveOrder.FromSystemID == 0)
			{
				vector = moveOrder.FromCoords;
			}
			else
			{
				vector = gamedb.GetStarSystemInfo(moveOrder.FromSystemID).Origin;
			}
			Vector3 vector2;
			if (moveOrder.ToSystemID == 0)
			{
				vector2 = moveOrder.ToCoords;
			}
			else
			{
				vector2 = gamedb.GetStarSystemInfo(moveOrder.ToSystemID).Origin;
			}
			float num = (vector - vector2).Length;
			float num2 = num;
			float arg_7B_0 = moveOrder.Progress;
			foreach (GameSession.IntersectingSystem current in GameSession.GetIntersectingSystems(gamedb, vector, vector2, 2f))
			{
				if (current.SystemID == moveOrder.FromSystemID || current.SystemID == moveOrder.ToSystemID)
				{
					num += 4f;
				}
				else
				{
					if (current.StartOrEnd)
					{
						num += 2f * (2f - current.Distance);
					}
					else
					{
						float val = 2f * (float)Math.Sqrt((double)(4f - current.Distance * current.Distance));
						float num3 = 2f - current.Distance;
						float num4 = (float)Math.Sqrt((double)(num3 * 2f * (num3 * 2f) + num3 * num3));
						float val2 = (num4 - num3 * 2f) * 2f;
						num += Math.Min(val, val2);
					}
				}
			}
			if (num > 3f * num2)
			{
				num = 3f * num2;
			}
			return num;
		}
		public static bool SystemsLinkedByNonPermenantNodes(GameSession game, int systemA, int systemB)
		{
			IEnumerable<NodeLineInfo> nodeLines = game.GameDatabase.GetNodeLines();
			foreach (NodeLineInfo current in nodeLines)
			{
				if (!current.IsPermenant && ((current.System1ID == systemA && current.System2ID == systemB) || (current.System1ID == systemB && current.System2ID == systemA)))
				{
					return true;
				}
			}
			return false;
		}
		public static NodeLineInfo GetSystemPermenantNodeLine(GameDatabase db, int SystemA, int SystemB)
		{
			IEnumerable<NodeLineInfo> nodeLines = db.GetNodeLines();
			foreach (NodeLineInfo current in nodeLines)
			{
				if (current.IsPermenant && ((current.System1ID == SystemA && current.System2ID == SystemB) || (current.System1ID == SystemB && current.System2ID == SystemA)))
				{
					return current;
				}
			}
			return null;
		}
		public static NodeLineInfo GetSystemsNonPermenantNodeLine(GameDatabase db, int systemA, int systemB)
		{
			IEnumerable<NodeLineInfo> nodeLines = db.GetNodeLines();
			foreach (NodeLineInfo current in nodeLines)
			{
				if (!current.IsPermenant && ((current.System1ID == systemA && current.System2ID == systemB) || (current.System1ID == systemB && current.System2ID == systemA)))
				{
					return current;
				}
			}
			return null;
		}
		public static bool FleetHasBore(GameDatabase db, int fleetid)
		{
			IEnumerable<ShipInfo> shipInfoByFleetID = db.GetShipInfoByFleetID(fleetid, false);
			foreach (ShipInfo current in shipInfoByFleetID)
			{
				DesignInfo designInfo = db.GetDesignInfo(current.DesignID);
				if (designInfo.Class == ShipClass.Cruiser)
				{
					ShipSectionAsset shipSectionAsset = null;
					DesignSectionInfo[] designSections = designInfo.DesignSections;
					DesignSectionInfo sect;
					for (int i = 0; i < designSections.Length; i++)
					{
						sect = designSections[i];
						ShipSectionAsset shipSectionAsset2 = db.AssetDatabase.ShipSections.FirstOrDefault((ShipSectionAsset x) => x.FileName == sect.FilePath);
						if (shipSectionAsset2 != null && shipSectionAsset2.Type == ShipSectionType.Mission)
						{
							shipSectionAsset = shipSectionAsset2;
							break;
						}
					}
					if (shipSectionAsset != null && shipSectionAsset.FileName.Contains("bore"))
					{
						bool result = true;
						return result;
					}
				}
				else
				{
					if (designInfo.Class == ShipClass.Dreadnought)
					{
						ShipSectionAsset shipSectionAsset3 = null;
						DesignSectionInfo[] designSections2 = designInfo.DesignSections;
						DesignSectionInfo sect;
						for (int j = 0; j < designSections2.Length; j++)
						{
							sect = designSections2[j];
							ShipSectionAsset shipSectionAsset4 = db.AssetDatabase.ShipSections.FirstOrDefault((ShipSectionAsset x) => x.FileName == sect.FilePath);
							if (shipSectionAsset4 != null && shipSectionAsset4.Type == ShipSectionType.Mission)
							{
								shipSectionAsset3 = shipSectionAsset4;
								break;
							}
						}
						if (shipSectionAsset3 != null && shipSectionAsset3.FileName.Contains("bore"))
						{
							bool result = true;
							return result;
						}
					}
				}
			}
			return false;
		}
		public static float GetBoreHealthMultiplier(GameDatabase db, int playerid)
		{
			if (db.PlayerHasTech(playerid, "DRV_Radiant_Drive"))
			{
				return 2f;
			}
			return 1f;
		}
		public static int GetPlayerMaxAdmirals(GameDatabase gamedb, int playerID)
		{
			int num = 0;
			IEnumerable<int> playerColonySystemIDs = gamedb.GetPlayerColonySystemIDs(playerID);
			foreach (int current in playerColonySystemIDs)
			{
				IEnumerable<ColonyInfo> colonyInfosForSystem = gamedb.GetColonyInfosForSystem(current);
				foreach (ColonyInfo current2 in colonyInfosForSystem)
				{
					PlanetInfo planetInfo = gamedb.GetPlanetInfo(current2.OrbitalObjectID);
					if (planetInfo.Size <= 5f)
					{
						num++;
					}
					else
					{
						if (planetInfo.Size <= 9f)
						{
							num += 2;
						}
						else
						{
							if (planetInfo.Size >= 10f)
							{
								num += 3;
							}
						}
					}
				}
				StationInfo navalStationForSystemAndPlayer = gamedb.GetNavalStationForSystemAndPlayer(current, playerID);
				if (navalStationForSystemAndPlayer != null && navalStationForSystemAndPlayer.DesignInfo.StationLevel >= 4)
				{
					num++;
				}
			}
			return num;
		}
		public static float GetStationTacSensorRange(GameSession app, StationInfo station)
		{
			return GameSession.GetStationBaseTacSensorRange(app, station) + GameSession.GetStationAdditionalTacSensorRange(app, station);
		}
		public static float GetStationBaseTacSensorRange(GameSession app, StationInfo station)
		{
			ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(station.DesignInfo.DesignSections[0].FilePath);
			if (shipSectionAsset != null)
			{
				return shipSectionAsset.TacticalSensorRange;
			}
			return 0f;
		}
		public static float GetStationAdditionalTacSensorRange(GameSession app, StationInfo station)
		{
			app.GameDatabase.GetDesignInfo(station.DesignInfo.ID);
			return station.DesignInfo.TacSensorRange;
		}
		public static float GetFleetSensorRange(AssetDatabase assetdb, GameDatabase db, int fleetid)
		{
			FleetInfo fleetInfo = db.GetFleetInfo(fleetid);
			return GameSession.GetFleetSensorRange(assetdb, db, fleetInfo, null);
		}
		public static float GetFleetSensorRange(AssetDatabase assetdb, GameDatabase db, FleetInfo fleet, List<ShipInfo> cachedShips = null)
		{
			float num = 0f;
			IEnumerable<ShipInfo> enumerable = (cachedShips != null) ? cachedShips : db.GetShipInfoByFleetID(fleet.ID, true);
			foreach (ShipInfo current in enumerable)
			{
				if (current.DesignInfo == null)
				{
					current.DesignInfo = db.GetDesignInfo(current.DesignID);
				}
				if (!ShipSectionAsset.IsBattleRiderClass(current.DesignInfo.GetRealShipClass().Value))
				{
					DesignInfo designInfo = db.GetDesignInfo(current.DesignID);
					DesignSectionInfo[] designSections = designInfo.DesignSections;
					for (int i = 0; i < designSections.Length; i++)
					{
						DesignSectionInfo designSectionInfo = designSections[i];
						num = Math.Max(num, designSectionInfo.ShipSectionAsset.StrategicSensorRange);
					}
				}
			}
			return num;
		}
		public bool SystemHasPlayerColony(int systemid, int playerid)
		{
			IEnumerable<ColonyInfo> colonyInfosForSystem = this.GameDatabase.GetColonyInfosForSystem(systemid);
			if (colonyInfosForSystem.Any<ColonyInfo>())
			{
				foreach (ColonyInfo current in colonyInfosForSystem)
				{
					if (current.PlayerID == playerid)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}
		public static float GetSupportRange(GameSession game, int playerID)
		{
			return GameSession.GetSupportRange(game.AssetDatabase, game.GameDatabase, playerID);
		}
		public static float GetSupportRange(AssetDatabase assetdb, GameDatabase db, int playerID)
		{
			return db.FindCurrentDriveSpeedForPlayer(playerID) * assetdb.StationSupportRangeModifier + db.GetStratModifier<float>(StratModifiers.GateCastDistance, playerID);
		}
		public static HashSet<int> GetStarSystemsWithGates(GameDatabase db, int playerId, IEnumerable<int> systemIds)
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (int current in systemIds)
			{
				if (db.SystemHasGate(current, playerId))
				{
					hashSet.Add(current);
				}
			}
			return hashSet;
		}
		public static int GetStationGateCapacity(GameSession game, StationInfo stationInfo)
		{
			int num = 0;
			if (stationInfo.DesignInfo.StationType != StationType.GATE)
			{
				return 0;
			}
			switch (stationInfo.DesignInfo.StationLevel)
			{
			case 1:
				num = 10;
				break;
			case 2:
				num = 15;
				break;
			case 3:
				num = 25;
				break;
			case 4:
				num = 30;
				break;
			case 5:
				num = 50;
				break;
			}
			Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(stationInfo, game.GameDatabase, true);
			if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Amp))
			{
				num += dictionary[ModuleEnums.StationModuleType.Amp] * 2;
			}
			return num;
		}
		public static int GetTotalGateCapacity(GameSession game, int playerid)
		{
			int num = 0;
			IEnumerable<StationInfo> stationInfosByPlayerID = game.GameDatabase.GetStationInfosByPlayerID(playerid);
			foreach (StationInfo current in stationInfosByPlayerID)
			{
				if (current.DesignInfo.StationType == StationType.GATE && current.DesignInfo.StationLevel > 0)
				{
					num += GameSession.GetStationGateCapacity(game, current);
				}
			}
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(playerid, FleetType.FL_GATE).ToList<FleetInfo>();
			num += list.Count * 6;
			return num;
		}
		public static int GetTotalGateUsage(GameSession game, int playerid)
		{
			int num = 0;
			IEnumerable<FleetInfo> fleetInfosByPlayerID = game.GameDatabase.GetFleetInfosByPlayerID(playerid, FleetType.FL_NORMAL);
			foreach (FleetInfo current in fleetInfosByPlayerID)
			{
				if (game.GameDatabase.GetMoveOrderInfoByFleetID(current.ID) == null)
				{
					MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(current.ID);
					if (missionByFleetID != null)
					{
						List<WaypointInfo> list = game.GameDatabase.GetWaypointsByMissionID(missionByFleetID.ID).ToList<WaypointInfo>();
						if (list.Count > 0)
						{
							WaypointInfo waypointInfo = list.First<WaypointInfo>();
							if ((waypointInfo.Type == WaypointType.TravelTo && waypointInfo.SystemID != current.SystemID) || (waypointInfo.Type == WaypointType.ReturnHome && current.SupportingSystemID != current.SystemID))
							{
								int num2;
								float num3;
                                List<int> bestTravelPath = Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(game, current.ID, current.SystemID, (waypointInfo.Type == WaypointType.ReturnHome) ? game.GameDatabase.GetHomeSystem(game, waypointInfo.MissionID, current) : waypointInfo.SystemID.Value, out num2, out num3, missionByFleetID.UseDirectRoute, null, null);
								if (game.GameDatabase.SystemHasGate(bestTravelPath.First<int>(), current.PlayerID) && game.GameDatabase.SystemHasGate(bestTravelPath[1], playerid))
								{
									num += game.GameDatabase.GetFleetCruiserEquivalent(current.ID);
								}
							}
						}
					}
				}
			}
			return num;
		}
		public static IEnumerable<GameSession.IntersectingSystem> GetIntersectingSystems(GameDatabase gamedb, Vector3 from, Vector3 to, float radius)
		{
			List<GameSession.IntersectingSystem> list = new List<GameSession.IntersectingSystem>();
			foreach (StarSystemInfo current in gamedb.GetStarSystemInfos())
			{
				float length = (current.Origin - from).Length;
				float length2 = (current.Origin - to).Length;
				if (length < radius)
				{
					GameSession.IntersectingSystem item;
					item.SystemID = current.ID;
					item.Distance = length;
					item.StartOrEnd = true;
					list.Add(item);
				}
				else
				{
					if (length2 < radius)
					{
						GameSession.IntersectingSystem item2;
						item2.SystemID = current.ID;
						item2.Distance = length2;
						item2.StartOrEnd = true;
						list.Add(item2);
					}
					else
					{
						Vector3 v = Vector3.Normalize(from - to);
						Vector3 v2 = Vector3.Normalize(from - current.Origin);
						float num = Vector3.Dot(v, v2);
						if (num > 0f)
						{
							float length3 = (from - current.Origin).Length;
							float num2 = length3 * num;
							float num3 = (float)Math.Sqrt((double)(length3 * length3 - num2 * num2));
							if (num3 < radius)
							{
								GameSession.IntersectingSystem item3;
								item3.SystemID = current.ID;
								item3.Distance = num3;
								item3.StartOrEnd = false;
								list.Add(item3);
							}
						}
					}
				}
			}
			return list;
		}
		public void UpdateFleetSupply()
		{
			List<FleetInfo> list = new List<FleetInfo>();
			foreach (FleetInfo current in this._db.GetFleetInfos(FleetType.FL_NORMAL))
			{
				if (current.SupportingSystemID != 0 || (!current.IsReserveFleet && this._db.GetPlayerInfo(current.PlayerID).isStandardPlayer))
				{
					if (current.SystemID == current.SupportingSystemID)
					{
                        current.SupplyRemaining = Kerberos.Sots.StarFleet.StarFleet.GetSupplyCapacity(this.GameDatabase, current.ID);
						current.TurnsAway = 0;
					}
					else
					{
						if (!this.IsFleetInSupplyRange(current.ID))
						{
                            current.SupplyRemaining -= Kerberos.Sots.StarFleet.StarFleet.GetSupplyConsumption(this, current.ID) * 0.5f;
						}
						current.TurnsAway++;
					}
					list.Add(current);
				}
			}
			foreach (FleetInfo current2 in list)
			{
				this._db.UpdateFleetInfo(current2);
			}
		}
		public bool IsFleetInSupplyRange(int fleetID)
		{
			FleetInfo fleetInfo = this.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo == null)
			{
				return false;
			}
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID);
			if (playerInfo != null)
			{
				FactionInfo factionInfo = this.GameDatabase.GetFactionInfo(playerInfo.FactionID);
				if (factionInfo != null && factionInfo.Name == "hiver" && this.GameDatabase.SystemHasGate(fleetInfo.SystemID, playerInfo.ID))
				{
					return true;
				}
			}
			FleetLocation fleetLocation = this.GameDatabase.GetFleetLocation(fleetID, false);
			float supportRange = GameSession.GetSupportRange(this, fleetInfo.PlayerID);
			foreach (int current in this.GameDatabase.GetPlayerColonySystemIDs(fleetInfo.PlayerID))
			{
				Vector3 starSystemOrigin = this.GameDatabase.GetStarSystemOrigin(current);
				float length = (starSystemOrigin - fleetLocation.Coords).Length;
				if (length <= supportRange)
				{
					return true;
				}
			}
			return false;
		}
		public bool IsSystemInSupplyRange(int systemID, int playerID)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(playerID);
			if (playerInfo != null)
			{
				FactionInfo factionInfo = this.GameDatabase.GetFactionInfo(playerInfo.FactionID);
				if (factionInfo != null && factionInfo.Name == "hiver" && this.GameDatabase.SystemHasGate(systemID, playerInfo.ID))
				{
					return true;
				}
			}
			List<int> list = this.GameDatabase.GetPlayerColonySystemIDs(playerID).ToList<int>();
			if (list.Contains(systemID))
			{
				return true;
			}
			float supportRange = GameSession.GetSupportRange(this, playerID);
			Vector3 starSystemOrigin = this.GameDatabase.GetStarSystemOrigin(systemID);
			foreach (int current in list)
			{
				Vector3 starSystemOrigin2 = this.GameDatabase.GetStarSystemOrigin(current);
				float length = (starSystemOrigin2 - starSystemOrigin).Length;
				if (length <= supportRange)
				{
					return true;
				}
			}
			return false;
		}
		public bool IsSystemInSupplyRangeOfSupportingSystem(int supportingSystemID, int systemID, int playerID)
		{
			GameDatabase gameDatabase = this.App.GameDatabase;
			StarSystemInfo starSystemInfo = gameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo == null)
			{
				return false;
			}
			StarSystemInfo starSystemInfo2 = gameDatabase.GetStarSystemInfo(supportingSystemID);
			if (starSystemInfo2 == null)
			{
				GameSession.Warn("Unable to find supporting system for player.");
				return false;
			}
			PlayerInfo playerInfo = gameDatabase.GetPlayerInfo(playerID);
			if (playerInfo != null)
			{
				FactionInfo factionInfo = gameDatabase.GetFactionInfo(playerInfo.FactionID);
				if (factionInfo != null && factionInfo.Name == "hiver" && gameDatabase.SystemHasGate(systemID, playerInfo.ID))
				{
					return true;
				}
			}
			float length = (starSystemInfo2.Origin - starSystemInfo.Origin).Length;
			float supportRange = GameSession.GetSupportRange(this, playerID);
			return length < supportRange;
		}
		public float GetStationBuildModifierForSystem(int systemId, int playerId)
		{
			float num = 1f;
			List<StationInfo> list = this._db.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(current, this._db, true);
				StationType stationType = current.DesignInfo.StationType;
				if (stationType == StationType.NAVAL && dictionary.ContainsKey(ModuleEnums.StationModuleType.Dock))
				{
					num += (float)dictionary[ModuleEnums.StationModuleType.Dock] * 0.02f;
				}
			}
			return num;
		}
		public float GetStationRepairModifierForSystem(int systemId, int playerId)
		{
			float num = 1f;
			List<StationInfo> list = this._db.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(current, this._db, true);
				StationType stationType = current.DesignInfo.StationType;
				if (stationType == StationType.NAVAL && dictionary.ContainsKey(ModuleEnums.StationModuleType.Repair))
				{
					num += (float)dictionary[ModuleEnums.StationModuleType.Repair] * 0.02f;
				}
			}
			return num;
		}
		public void BuildAtSystem(int SystemId, int playerId)
		{
			float num = 0f;
			List<ColonyInfo> list = this._db.GetColonyInfosForSystem(SystemId).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				if (current.PlayerID == playerId)
				{
					num += Kerberos.Sots.Strategy.InhabitedPlanet.Colony.GetConstructionPoints(this, current);
				}
			}
			num *= this.GetStationBuildModifierForSystem(SystemId, playerId);
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			List<BuildOrderInfo> list2 = this._db.GetBuildOrdersForSystem(SystemId).ToList<BuildOrderInfo>();
			foreach (BuildOrderInfo current2 in list2)
			{
				DesignInfo designInfo = this._db.GetDesignInfo(current2.DesignID);
				if (designInfo.PlayerID == playerId)
				{
					int num5 = current2.ProductionTarget - current2.Progress;
					float num6 = 0f;
					if (!designInfo.isPrototyped)
					{
						num6 = (float)((int)(num * (this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PrototypeTimeModifier, playerId) - 1f)));
					}
					if ((float)num5 > num - num6)
					{
						current2.Progress += (int)(num - num6);
						num = 0f;
						this._db.UpdateBuildOrder(current2);
						break;
					}
					if ((float)num5 <= num - num6)
					{
						if (playerId == this.LocalPlayer.ID)
						{
							this._app.SteamHelper.DoAchievement(AchievementType.SOTS2_STEEL_PUSHER);
							if (designInfo.Class == ShipClass.Dreadnought && this.LocalPlayer.Faction.Name == "human")
							{
								this._app.SteamHelper.DoAchievement(AchievementType.SOTS2_SWORD_OF_STARS);
							}
							if (designInfo.Class == ShipClass.Leviathan && this.LocalPlayer.Faction.Name == "human" && designInfo.Name == "Leviathan")
							{
								this._app.SteamHelper.DoAchievement(AchievementType.SOTS2_IMITATION);
							}
						}
						this.ConstructShip(designInfo, playerId, current2.SystemID, current2.MissionID, current2.ShipName, current2.AIFleetID, current2.LoaCubes);
						num -= (float)num5;
						this._db.RemoveBuildOrder(current2.ID);
						if (current2.InvoiceID.HasValue)
						{
							bool flag = false;
							List<BuildOrderInfo> list3 = this._db.GetBuildOrdersForInvoiceInstance(current2.InvoiceID.Value).ToList<BuildOrderInfo>();
							foreach (BuildOrderInfo current3 in list3)
							{
								if (current3.Progress != current3.ProductionTarget)
								{
									flag = true;
								}
							}
							if (!flag)
							{
								this._db.RemoveInvoiceInstance(current2.InvoiceID.Value);
								if (this._db.GetInvoicesForSystem(playerId, SystemId).Count<InvoiceInstanceInfo>() == 0)
								{
									this._app.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_INVOICES_COMPLETE,
										EventMessage = TurnEventMessage.EM_INVOICES_COMPLETE,
										TurnNumber = this._db.GetTurnCount(),
										PlayerID = playerId,
										SystemID = SystemId
									});
								}
							}
						}
						if (designInfo.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.FreighterSpace) > 0)
						{
							num3++;
						}
						else
						{
							if (designInfo.DesignSections.Any((DesignSectionInfo x) => x.FilePath.ToLower().Contains("_sdb")))
							{
								num4++;
							}
							else
							{
								num2++;
							}
						}
					}
				}
			}
			if (num2 > 0)
			{
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_SHIPS_BUILT,
					EventMessage = (num2 > 1) ? TurnEventMessage.EM_SHIPS_BUILT_MULTIPLE : TurnEventMessage.EM_SHIPS_BUILT_SINGLE,
					TurnNumber = this._db.GetTurnCount(),
					PlayerID = playerId,
					SystemID = SystemId,
					NumShips = num2
				});
			}
			if (num3 > 0)
			{
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_FREIGHTER_BUILT,
					EventMessage = (num3 > 1) ? TurnEventMessage.EM_FREIGHTER_BUILT : TurnEventMessage.EM_FREIGHTERS_BUILT,
					TurnNumber = this._db.GetTurnCount(),
					PlayerID = playerId,
					SystemID = SystemId,
					NumShips = num3
				});
			}
			if (num4 > 0)
			{
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_SDB_BUILT,
					EventMessage = (num4 > 1) ? TurnEventMessage.EM_SDB_BUILT : TurnEventMessage.EM_SDBS_BUILT,
					TurnNumber = this._db.GetTurnCount(),
					PlayerID = playerId,
					SystemID = SystemId,
					NumShips = num4
				});
			}
		}
		public void RetrofitShip(int shipid, int designid, float costmult = 1f)
		{
			ShipInfo shipInfo = this._db.GetShipInfo(shipid, true);
			DesignInfo designInfo = this._db.GetDesignInfo(designid);
			if (shipInfo != null && designInfo != null)
			{
				List<ShipInfo> list = this._app.GameDatabase.GetBattleRidersByParentID(shipInfo.ID).ToList<ShipInfo>();
				foreach (ShipInfo current in list)
				{
					RealShipClasses? realShipClass = current.DesignInfo.GetRealShipClass();
					if (realShipClass.HasValue && ShipSectionAsset.IsWeaponBattleRiderClass(realShipClass.Value))
					{
						this._db.RemoveShip(current.ID);
					}
				}
				this._app.GameDatabase.UpdateShipDesign(shipid, designid, null);
				this.AddDefaultStartingRiders(shipInfo.FleetID, designid, shipInfo.ID);
                double num = (double)((float)Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this._app, shipInfo.DesignInfo, designInfo) * costmult);
				PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(designInfo.PlayerID);
				this._db.UpdatePlayerSavings(designInfo.PlayerID, playerInfo.Savings - num);
			}
		}
		public void RetrofitAtSystem(int SystemId, int playerId)
		{
			int num = 0;
            bool flag = Kerberos.Sots.StarFleet.StarFleet.SystemSupportsRetrofitting(this._app, SystemId, playerId);
			List<RetrofitOrderInfo> list = this._db.GetRetrofitOrdersForSystem(SystemId).ToList<RetrofitOrderInfo>();
			if (flag)
			{
                int num2 = Kerberos.Sots.StarFleet.StarFleet.GetSystemRetrofitCapacity(this._app, SystemId, playerId);
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				foreach (RetrofitOrderInfo current in list)
				{
					if (num2 <= 0)
					{
						break;
					}
					if (!dictionary.ContainsKey(current.DesignID))
					{
						dictionary.Add(current.DesignID, 0);
					}
					Dictionary<int, int> dictionary2;
					int designID;
					(dictionary2 = dictionary)[designID = current.DesignID] = dictionary2[designID] + 1;
					ShipInfo shipInfo = this._db.GetShipInfo(current.ShipID, true);
					this.RetrofitShip(current.ShipID, current.DesignID, 1f);
					bool defenseasset = false;
					if (shipInfo.IsPlatform() || shipInfo.IsSDB())
					{
						defenseasset = true;
					}
					this._db.RemoveRetrofitOrder(current.ID, false, defenseasset);
					num2--;
					num++;
					List<RetrofitOrderInfo> list2 = this._db.GetRetrofitOrdersForInvoiceInstance(current.InvoiceID.Value).ToList<RetrofitOrderInfo>();
					if (list2.Count <= 0)
					{
						this._db.RemoveInvoiceInstance(current.InvoiceID.Value);
					}
				}
				using (Dictionary<int, int>.Enumerator enumerator2 = dictionary.GetEnumerator())
				{
					KeyValuePair<int, int> kvp;
					while (enumerator2.MoveNext())
					{
						kvp = enumerator2.Current;
						KeyValuePair<int, int> kvp5 = kvp;
						if (kvp5.Value > 1)
						{
							GameDatabase arg_1EE_0 = this._app.GameDatabase;
							TurnEvent turnEvent = new TurnEvent();
							turnEvent.EventType = TurnEventType.EV_RETROFIT_COMPLETE;
							turnEvent.EventMessage = TurnEventMessage.EM_RETROFIT_COMPLETE_MULTI;
							turnEvent.TurnNumber = this._db.GetTurnCount();
							turnEvent.PlayerID = playerId;
							turnEvent.SystemID = SystemId;
							TurnEvent arg_1E7_0 = turnEvent;
							KeyValuePair<int, int> kvp2 = kvp;
							arg_1E7_0.DesignID = kvp2.Key;
							arg_1EE_0.InsertTurnEvent(turnEvent);
						}
						else
						{
							RetrofitOrderInfo retrofitOrderInfo = list.First(delegate(RetrofitOrderInfo x)
							{
								int arg_14_0 = x.DesignID;
								KeyValuePair<int, int> kvp4 = kvp;
								return arg_14_0 == kvp4.Key;
							});
							GameDatabase arg_282_0 = this._app.GameDatabase;
							TurnEvent turnEvent2 = new TurnEvent();
							turnEvent2.EventType = TurnEventType.EV_RETROFIT_COMPLETE;
							turnEvent2.EventMessage = TurnEventMessage.EM_RETROFIT_COMPLETE_MULTI;
							turnEvent2.TurnNumber = this._db.GetTurnCount();
							turnEvent2.PlayerID = playerId;
							turnEvent2.SystemID = SystemId;
							TurnEvent arg_26D_0 = turnEvent2;
							KeyValuePair<int, int> kvp3 = kvp;
							arg_26D_0.DesignID = kvp3.Key;
							turnEvent2.ShipID = retrofitOrderInfo.ShipID;
							arg_282_0.InsertTurnEvent(turnEvent2);
						}
					}
					return;
				}
			}
			foreach (RetrofitOrderInfo current2 in list)
			{
				this._db.RemoveRetrofitOrder(current2.ID, false, false);
				List<RetrofitOrderInfo> list3 = this._db.GetRetrofitOrdersForInvoiceInstance(current2.InvoiceID.Value).ToList<RetrofitOrderInfo>();
				if (list3.Count <= 0)
				{
					this._db.RemoveInvoiceInstance(current2.InvoiceID.Value);
				}
			}
		}
		public void DetectPiracyFleets(int systemid, int playerid)
		{
			List<MissionInfo> list = (
				from x in this._db.GetMissionsBySystemDest(systemid)
				where x.Type == MissionType.PIRACY
				select x).ToList<MissionInfo>();
			MissionInfo missionInfo = null;
			foreach (MissionInfo current in list)
			{
				FleetInfo fleetInfo = this._db.GetFleetInfo(current.FleetID);
				if (this._db.GetAdmiralInfo(fleetInfo.AdmiralID) == null)
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleetInfo, true);
				}
				else
				{
					if (fleetInfo.PlayerID != playerid && missionInfo == null)
					{
						missionInfo = current;
					}
				}
			}
			if (missionInfo != null)
			{
				FleetInfo fleetInfo2 = this._db.GetFleetInfo(missionInfo.FleetID);
				AdmiralInfo admiralInfo = this._db.GetAdmiralInfo(fleetInfo2.AdmiralID);
				if (fleetInfo2.PlayerID == playerid)
				{
					return;
				}
				if (this._db.GetPiracyFleetDetectionInfoForFleet(fleetInfo2.ID).Any((PiracyFleetDetectionInfo x) => x.PlayerID == playerid))
				{
					return;
				}
				if (fleetInfo2.SystemID == systemid)
				{
					int num = 0;
					MissionInfo missionInfo2 = (
						from x in this._db.GetMissionsBySystemDest(systemid)
						where x.Type == MissionType.PATROL
						select x).FirstOrDefault((MissionInfo x) => this._db.GetFleetInfo(x.FleetID).PlayerID == playerid);
					if (missionInfo2 != null)
					{
						FleetInfo fleetInfo3 = this._db.GetFleetInfo(missionInfo2.FleetID);
						AdmiralInfo admiralInfo2 = this._db.GetAdmiralInfo(fleetInfo3.AdmiralID);
						if (admiralInfo2.ReactionBonus > admiralInfo.EvasionBonus)
						{
							num += 50;
						}
						List<ShipInfo> source = this._db.GetShipInfoByFleetID(fleetInfo3.ID, false).ToList<ShipInfo>();
						if (source.Any((ShipInfo x) => x.DesignInfo.DesignSections.Any((DesignSectionInfo j) => j.ShipSectionAsset.isDeepScan)))
						{
							num += 30;
						}
					}
					if (this._db.IsStealthFleet(fleetInfo2.ID))
					{
						num -= 25;
					}
					List<ShipInfo> list2 = this._db.GetShipInfoByFleetID(fleetInfo2.ID, false).ToList<ShipInfo>();
					if ((
						from x in list2
						where x.DesignInfo.DesignSections.Any((DesignSectionInfo j) => j.ShipSectionAsset.cloakingType == CloakingType.Cloaking || j.ShipSectionAsset.cloakingType == CloakingType.ImprovedCloaking)
						select x).Count<ShipInfo>() == list2.Count)
					{
						num -= 50;
					}
					int? defenseFleetID = this._db.GetDefenseFleetID(systemid, playerid);
					if (defenseFleetID.HasValue)
					{
						this._db.GetFleetInfo(defenseFleetID.Value);
						List<ShipInfo> source2 = this._db.GetShipInfoByFleetID(fleetInfo2.ID, false).ToList<ShipInfo>();
						int num2 = (
							from x in source2
							where x.DesignInfo.IsPlatform() && x.DesignInfo.GetPlatformType().HasValue && x.DesignInfo.GetPlatformType().Value == PlatformTypes.scansat
							select x).Count<ShipInfo>();
						num += num2 * 5;
					}
					Random random = new Random();
					int num3 = random.Next(0, 100);
					if (num3 < num)
					{
						this._db.InsertPiracyFleetDetectionInfo(fleetInfo2.ID, playerid);
					}
				}
			}
		}
		public int ConstructStation(DesignInfo designInfo, int parentOrbitalId, bool free = false)
		{
			PlayerInfo playerInfo = this._db.GetPlayerInfo(designInfo.PlayerID);
			if (!free)
			{
				this._db.UpdatePlayerSavings(designInfo.PlayerID, playerInfo.Savings - (double)designInfo.SavingsCost);
				if (playerInfo.ID == this.LocalPlayer.ID)
				{
					this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_STATION);
				}
			}
			return this.InsertStation(designInfo, parentOrbitalId, playerInfo.ID);
		}
        public int ConstructShip(DesignInfo design, int playerID, int SystemID, int missionID, string shipName, int? aiFleetID, int LoaCubes)
        {
            int num2;
            Func<ShipSectionAsset, bool> predicate = null;
            TradeResultsTable tradeResultsTable = this._db.GetTradeResultsTable();
            if (!tradeResultsTable.TradeNodes.ContainsKey(SystemID) || (tradeResultsTable.TradeNodes[SystemID].ProductionCapacity == ((tradeResultsTable.TradeNodes[SystemID].ExportInt + tradeResultsTable.TradeNodes[SystemID].ExportLoc) + tradeResultsTable.TradeNodes[SystemID].ExportProv)))
            {
                bool isOpen = this._db.GetStarSystemInfo(SystemID).IsOpen;
                foreach (ColonyInfo info in this._db.GetColonyInfosForSystem(SystemID).ToList<ColonyInfo>())
                {
                    info.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.ShipsBuiltWithTradeMaxed, 1);
                    this._db.UpdateColony(info);
                }
            }
            PlayerInfo playerInfo = this._db.GetPlayerInfo(playerID);
            Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(playerID);
            if (design.IsLoaCube())
            {
                design.SavingsCost = LoaCubes * this.AssetDatabase.LoaCostPerCube;
            }
            int savingsCost = design.SavingsCost;
            if (!design.isPrototyped)
            {
                switch (design.Class)
                {
                    case ShipClass.Cruiser:
                        savingsCost = (int)(design.SavingsCost * this._db.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierCR, playerID));
                        break;

                    case ShipClass.Dreadnought:
                        savingsCost = (int)(design.SavingsCost * this._db.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierDN, playerID));
                        break;

                    case ShipClass.Leviathan:
                        savingsCost = (int)(design.SavingsCost * this._db.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierLV, playerID));
                        break;

                    case ShipClass.Station:
                        if (((RealShipClasses)design.GetRealShipClass()) == RealShipClasses.Platform)
                        {
                            savingsCost = (int)(design.SavingsCost * this._db.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierPF, playerID));
                        }
                        break;
                }
            }
            if ((playerInfo.isStandardPlayer && playerObject.IsAI()) && (playerObject.Faction.Name == "loa"))
            {
                savingsCost = (int)(savingsCost * 0.5f);
            }
            this._db.UpdatePlayerSavings(playerID, playerInfo.Savings - savingsCost);
            Trace("Ship of type " + design.Name + " constructed at " + this._db.GetStarSystemInfo(SystemID).Name);
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            Func<ShipSectionAsset, bool> func = null;
            foreach (DesignSectionInfo dsi in design.DesignSections)
            {
                if (func == null)
                {
                    func = x => x.FileName == dsi.FilePath;
                }
                ShipSectionAsset asset = this.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>(func);
                if (asset.IsFreighter)
                {
                    flag = true;
                }
                if (dsi.FilePath.ToLower().Contains("_sdb"))
                {
                    flag2 = true;
                }
                if (asset.isPolice)
                {
                    flag3 = true;
                }
                bool isMineLayer = asset.isMineLayer;
                if (asset.ColonizationSpace != 0)
                {
                    flag4 = true;
                }
            }
            if (flag3 && this._app.GetStratModifier<bool>(StratModifiers.AllowPoliceInCombat, playerID))
            {
                this._app.GameDatabase.InsertGovernmentAction(playerID, App.Localize("@GA_POLICEBUILT"), "PoliceBuilt", 0, 0);
            }
            if (flag4)
            {
                this._app.GameDatabase.InsertGovernmentAction(playerID, App.Localize("@GA_COLONIZERBUILT"), "ColonizerBuilt", 0, 0);
            }
            if (flag)
            {
                num2 = this._db.InsertFreighter(SystemID, playerID, design.ID, true);
                this._app.GameDatabase.InsertGovernmentAction(playerID, App.Localize("@GA_BUILTFREIGHTER"), "BuiltFreighter", 0, 0);
            }
            else
            {
                int fleetID = 0;
                if (predicate == null)
                {
                    predicate = x => x.FileName == design.DesignSections[0].FilePath;
                }
                ShipSectionAsset asset2 = this._app.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>(predicate);
                if (((asset2 != null) && (design.DesignSections.Count<DesignSectionInfo>() > 0)) && (asset2.RealClass == RealShipClasses.Platform))
                {
                    MissionInfo missionInfo = this._db.GetMissionInfo(missionID);
                    if (missionInfo != null)
                    {
                        fleetID = missionInfo.FleetID;
                    }
                    fleetID = this._db.InsertOrGetDefenseFleetInfo(SystemID, playerID).ID;
                }
                else if (flag2)
                {
                    MissionInfo info4 = this._db.GetMissionInfo(missionID);
                    if (info4 != null)
                    {
                        fleetID = info4.FleetID;
                    }
                    fleetID = this._db.InsertOrGetDefenseFleetInfo(SystemID, playerID).ID;
                }
                else if (missionID > 0)
                {
                    MissionInfo info5 = this._db.GetMissionInfo(missionID);
                    if (info5 != null)
                    {
                        fleetID = info5.FleetID;
                    }
                    else
                    {
                        fleetID = this._db.InsertOrGetReserveFleetInfo(SystemID, playerID).ID;
                    }
                }
                else
                {
                    fleetID = this._db.InsertOrGetReserveFleetInfo(SystemID, playerID).ID;
                }
                if (design.IsLoaCube())
                {
                    ShipInfo info8 = this._db.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>().FirstOrDefault<ShipInfo>(x => x.DesignInfo.IsLoaCube());
                    if (info8 != null)
                    {
                        this._db.UpdateShipLoaCubes(info8.ID, info8.LoaCubes + LoaCubes);
                        return info8.ID;
                    }
                }
                num2 = this._db.InsertShip(fleetID, design.ID, shipName, 0, aiFleetID, LoaCubes);
                if (design.Class == ShipClass.Leviathan)
                {
                    ApplyMoralEvent(this.App, MoralEvent.ME_LEVIATHAN_BUILT, playerID, null, null, new int?(SystemID));
                }
                if (design.DesignSections.Any<DesignSectionInfo>(x => x.FilePath.Contains("cnc")))
                {
                    ApplyMoralEvent(this.App, MoralEvent.ME_FLAGSHIP_BUILT, playerID, null, null, new int?(SystemID));
                }
                this.AddDefaultStartingRiders(fleetID, design.ID, num2);
            }
            if (!design.isPrototyped)
            {
                design.isPrototyped = true;
                this._app.GameDatabase.UpdateDesignPrototype(design.ID, design.isPrototyped);
                if (design.CanHaveAttributes())
                {
                    int stratModifierIntToApply = this._db.GetStratModifierIntToApply(StratModifiers.BadDesignAttributePercent, playerInfo.ID);
                    int num5 = this._db.GetStratModifierIntToApply(StratModifiers.GoodDesignAttributePercent, playerInfo.ID);
                    int num6 = this._random.Next(100);
                    if (num6 < stratModifierIntToApply)
                    {
                        SectionEnumerations.DesignAttribute da = this._random.Choose<SectionEnumerations.DesignAttribute>((IList<SectionEnumerations.DesignAttribute>)SectionEnumerations.BadDesignAttributes);
                        this._app.GameDatabase.InsertDesignAttribute(design.ID, da);
                        Trace("Ship of type " + design.Name + " was designed with a bad attribute: " + da.ToString());
                    }
                    else if (num6 < (stratModifierIntToApply + num5))
                    {
                        SectionEnumerations.DesignAttribute attribute2 = this._random.Choose<SectionEnumerations.DesignAttribute>((IList<SectionEnumerations.DesignAttribute>)SectionEnumerations.GoodDesignAttributes);
                        this._app.GameDatabase.InsertDesignAttribute(design.ID, attribute2);
                        Trace("Ship of type " + design.Name + " was designed with a good attribute: " + attribute2.ToString());
                    }
                    if (this._db.GetStratModifier<bool>(StratModifiers.ShowPrototypeDesignAttributes, playerInfo.ID))
                    {
                        this._db.UpdateDesignAttributeDiscovered(design.ID, true);
                    }
                }
                TurnEvent item = new TurnEvent
                {
                    EventType = TurnEventType.EV_PROTOTYPE_COMPLETE,
                    EventMessage = TurnEventMessage.EM_PROTOTYPE_COMPLETE,
                    TurnNumber = this._db.GetTurnCount(),
                    PlayerID = playerID,
                    SystemID = SystemID,
                    DesignID = design.ID
                };
                this._app.TurnEvents.Add(item);
            }
            return num2;
        }
        public void UpgradeStation(StationInfo s, DesignInfo newDesign)
		{
			if (newDesign.StationType == StationType.DIPLOMATIC && newDesign.StationLevel == 5)
			{
				foreach (PlayerInfo current in this._db.GetStandardPlayerInfos().ToList<PlayerInfo>())
				{
					if (newDesign.PlayerID != current.ID)
					{
						this._db.ApplyDiplomacyReaction(current.ID, newDesign.PlayerID, StratModifiers.DiplomacyReactionStarChamber, 1);
					}
				}
			}
			newDesign.ID = s.DesignInfo.ID;
			newDesign.DesignSections[0].ID = s.DesignInfo.DesignSections[0].ID;
			ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == newDesign.DesignSections[0].FilePath);
			List<LogicalModuleMount> list = shipSectionAsset.Modules.ToList<LogicalModuleMount>();
			string name = this.AssetDatabase.GetFaction(this.GameDatabase.GetPlayerFactionID(s.PlayerID)).Name;
			StationModuleQueue.UpdateStationMapsForFaction(name);
			foreach (DesignModuleInfo current2 in s.DesignInfo.DesignSections[0].Modules)
			{
				if (current2.StationModuleType.HasValue && current2.StationModuleType.Value < ModuleEnums.StationModuleType.NumModuleTypes)
				{
					ModuleEnums.ModuleSlotTypes desiredModuleType = AssetDatabase.StationModuleTypeToMountTypeMap[current2.StationModuleType.Value];
					if (desiredModuleType == ModuleEnums.ModuleSlotTypes.Habitation && name != AssetDatabase.GetModuleFactionName(current2.StationModuleType.Value))
					{
						desiredModuleType = ModuleEnums.ModuleSlotTypes.AlienHabitation;
					}
					LogicalModuleMount logicalModuleMount = list.FirstOrDefault((LogicalModuleMount x) => x.ModuleType == desiredModuleType.ToString());
					if (logicalModuleMount != null)
					{
						current2.MountNodeName = logicalModuleMount.NodeName;
						current2.DesignSectionInfo = s.DesignInfo.DesignSections[0];
						this._db.UpdateDesignModuleNodeName(current2);
						list.Remove(logicalModuleMount);
					}
				}
			}
			List<DesignModuleInfo> list2 = this._db.GetQueuedStationModules(s.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
			foreach (DesignModuleInfo current3 in list2)
			{
				if (current3.StationModuleType.HasValue)
				{
					ModuleEnums.ModuleSlotTypes desiredModuleType = AssetDatabase.StationModuleTypeToMountTypeMap[current3.StationModuleType.Value];
					if (desiredModuleType == ModuleEnums.ModuleSlotTypes.Habitation && this.AssetDatabase.GetFaction(this.GameDatabase.GetPlayerFactionID(s.PlayerID)).Name != AssetDatabase.GetModuleFactionName(current3.StationModuleType.Value))
					{
						desiredModuleType = ModuleEnums.ModuleSlotTypes.AlienHabitation;
					}
					LogicalModuleMount logicalModuleMount2 = list.FirstOrDefault((LogicalModuleMount x) => x.ModuleType == desiredModuleType.ToString());
					if (logicalModuleMount2 != null)
					{
						current3.MountNodeName = logicalModuleMount2.NodeName;
						current3.DesignSectionInfo = s.DesignInfo.DesignSections[0];
						this._db.UpdateQueuedModuleNodeName(current3);
						list.Remove(logicalModuleMount2);
					}
				}
			}
			s.DesignInfo = newDesign;
			this._db.UpdateStation(s);
			this._db.UpdateShipDesign(s.ShipID, newDesign.ID, new int?(s.ID));
		}
		public string GetUpgradeStationSoundCueName(DesignInfo newDesign)
		{
			Kerberos.Sots.PlayerFramework.Player playerObject = this._app.Game.GetPlayerObject(newDesign.PlayerID);
			string name = playerObject.Faction.Name;
			string result = "";
			switch (newDesign.StationType)
			{
			case StationType.NAVAL:
				switch (newDesign.StationLevel)
				{
				case 1:
					result = string.Format("STRAT_040-01_{0}_NavalOutpostComplete", name);
					break;
				case 2:
					result = string.Format("STRAT_041-01_{0}_OutpostUpgradedToForwardBase", name);
					break;
				case 3:
					result = string.Format("STRAT_042-01_{0}_ForwardBaseUpgradedToNavalBase", name);
					break;
				case 4:
					result = string.Format("STRAT_043-01_{0}_NavalBaseUpgradedToStarBase", name);
					break;
				case 5:
					result = string.Format("STRAT_044-01_{0}_StarBaseUpgradedToSectorBase", name);
					break;
				}
				break;
			case StationType.SCIENCE:
				switch (newDesign.StationLevel)
				{
				case 1:
					result = string.Format("STRAT_082-01_{0}_FieldStationComplete", name);
					break;
				case 2:
					result = string.Format("STRAT_083-01_{0}_FieldStationUpgradedToStarLab", name);
					break;
				case 3:
					result = string.Format("STRAT_084-01_{0}_StarLabUpgradedToResearchBase", name);
					break;
				case 4:
					result = string.Format("STRAT_085-01_{0}_ResearchBaseUpgradedToPolytechnic", name);
					break;
				case 5:
					result = string.Format("STRAT_112-01_{0}_PolytechnicUpgradedToScienceCenter", name);
					break;
				}
				break;
			case StationType.CIVILIAN:
				switch (newDesign.StationLevel)
				{
				case 1:
					result = string.Format("STRAT_019-01_{0}_WayStationComplete", name);
					break;
				case 2:
					result = string.Format("STRAT_020-01_{0}_WayStationUpgradedToTradingPost", name);
					break;
				case 3:
					result = string.Format("STRAT_021-01_{0}_TradingPostUpgradedToMerchanterStation", name);
					break;
				case 4:
					result = string.Format("STRAT_022-01_{0}_MerchanterStationUpgradedToNexus", name);
					break;
				case 5:
					result = string.Format("STRAT_023-01_{0}_NexusUpgradedToStarCity", name);
					break;
				}
				break;
			case StationType.DIPLOMATIC:
				switch (newDesign.StationLevel)
				{
				case 1:
					result = string.Format("STRAT_063-01_{0}_CustomsStationComplete", name);
					break;
				case 2:
					result = string.Format("STRAT_111-01_{0}_CustomsStationUpgradedToConsulate", name);
					break;
				case 3:
					result = string.Format("STRAT_064-01_{0}_ConsulateUpgradedToEmbassy", name);
					break;
				case 4:
					result = string.Format("STRAT_065-01_{0}_EmbassyUpgradedToCouncilStation", name);
					break;
				case 5:
					result = string.Format("STRAT_066-01_{0}_CouncilStationUpgradedToStarChamber", name);
					break;
				}
				break;
			case StationType.GATE:
				switch (newDesign.StationLevel)
				{
				case 1:
					result = string.Format("STRAT_020-01_{0}_GatewayComplete", name);
					break;
				case 2:
					result = string.Format("STRAT_020-01_{0}_GatewayUpgradedToCaster", name);
					break;
				case 3:
					result = string.Format("STRAT_020-01_{0}_CasterUpgradedToFarCaster", name);
					break;
				case 4:
					result = string.Format("STRAT_020-01_{0}_FarCasterUpgradedToLense", name);
					break;
				case 5:
					result = string.Format("STRAT_020-01_{0}_LenseUpgradedToMirrorOfCreation", name);
					break;
				}
				break;
			}
			return result;
		}
		public int InsertStation(DesignInfo di, int orbitalObjectID, int playerID)
		{
			OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(orbitalObjectID);
			int parentOrbitalObjectID = 0;
			if (orbitalObjectID != 0)
			{
				parentOrbitalObjectID = orbitalObjectID;
			}
			OrbitalPath path = default(OrbitalPath);
			path.Scale = new Vector2(10f, 10f);
			path.Rotation = new Vector3(0f, 0f, 0f);
			path.DeltaAngle = 10f;
			path.InitialAngle = 10f;
			string text = di.StationType.ToDisplayText(this.LocalPlayer.Faction.Name);
			string name = text + " Station";
			int result = this._db.InsertStation(parentOrbitalObjectID, orbitalObjectInfo.StarSystemID, path, name, playerID, di);
			GameSession.Trace(string.Concat(new object[]
			{
				"New ",
				text,
				" station constructed in System ",
				orbitalObjectInfo.StarSystemID,
				"."
			}));
			return result;
		}
		public static int InsertNewSalvageProject(App app, int playerid, int techid)
		{
			string techFileID = app.GameDatabase.GetTechFileID(techid);
			string projtype = techFileID.Substring(0, 3);
			PlayerTechInfo playerTechInfo = app.GameDatabase.GetPlayerTechInfo(playerid, techid);
			return app.GameDatabase.InsertSpecialProject(playerid, app.Game.NamesPool.GetSalvageProjectName(projtype), (int)((float)playerTechInfo.ResearchCost * new Random().NextInclusive(0.5f, 1.5f)), SpecialProjectType.Salvage, techid, 0, 0, 0);
		}
		private void CompleteSpecialProject(SpecialProjectInfo project)
		{
			if (project.Type == SpecialProjectType.Salvage)
			{
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_SALVAGE_PROJECT_COMPLETE,
					EventMessage = TurnEventMessage.EM_SALVAGE_PROJECT_COMPLETE,
					PlayerID = project.PlayerID,
					TechID = project.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					SpecialProjectID = project.ID,
					ShowsDialog = true
				});
				PlayerTechInfo playerTechInfo = this._app.GameDatabase.GetPlayerTechInfo(project.PlayerID, project.TechID);
				if (playerTechInfo != null && playerTechInfo.State != TechStates.Researched && playerTechInfo.State != TechStates.Researching)
				{
					playerTechInfo.PlayerFeasibility = 1f;
					playerTechInfo.Feasibility = 1f;
					playerTechInfo.TurnResearched = new int?(this.GameDatabase.GetTurnCount());
					this._app.GameDatabase.UpdatePlayerTechInfo(playerTechInfo);
					this._app.GameDatabase.UpdatePlayerTechState(project.PlayerID, playerTechInfo.TechID, TechStates.Core);
					return;
				}
			}
			else
			{
				if (project.Type == SpecialProjectType.AsteroidMonitor)
				{
					FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(project.FleetID);
					if (fleetInfo == null)
					{
						return;
					}
					fleetInfo.PlayerID = project.PlayerID;
					this.App.GameDatabase.UpdateFleetInfo(fleetInfo);
					FleetInfo fleetInfo2 = this.App.GameDatabase.InsertOrGetDefenseFleetInfo(fleetInfo.SystemID, project.PlayerID);
					List<int> list = this.App.GameDatabase.GetShipsByFleetID(fleetInfo.ID).ToList<int>();
					foreach (int current in list)
					{
						if (current != 0)
						{
							Matrix? shipSystemPosition = this.App.GameDatabase.GetShipSystemPosition(current);
							this.App.GameDatabase.TransferShip(current, fleetInfo2.ID);
							this.App.GameDatabase.UpdateShipSystemPosition(current, shipSystemPosition);
						}
					}
					this.App.GameDatabase.RemoveFleet(fleetInfo.ID);
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_MONITOR_PROJECT_COMPLETE,
						EventMessage = TurnEventMessage.EM_MONITOR_PROJECT_COMPLETE,
						PlayerID = project.PlayerID,
						SystemID = fleetInfo.SystemID,
						TurnNumber = this._app.GameDatabase.GetTurnCount(),
						SpecialProjectID = project.ID,
						ShowsDialog = false
					});
					using (IEnumerator<int> enumerator2 = this.App.GameDatabase.GetStandardPlayerIDs().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							int current2 = enumerator2.Current;
							List<SpecialProjectInfo> list2 = this.App.GameDatabase.GetSpecialProjectInfosByPlayerID(current2, true).ToList<SpecialProjectInfo>();
							foreach (SpecialProjectInfo current3 in list2)
							{
								if (current3.FleetID == project.FleetID)
								{
									FleetInfo fleetInfo3 = this.GameDatabase.GetFleetInfo(project.FleetID);
									this._app.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_MONITOR_CAPTURED,
										EventMessage = TurnEventMessage.EM_MONITOR_CAPTURED,
										PlayerID = project.PlayerID,
										SystemID = fleetInfo3.SystemID,
										TurnNumber = this._app.GameDatabase.GetTurnCount(),
										SpecialProjectID = project.ID,
										ShowsDialog = false
									});
									this.GameDatabase.RemoveSpecialProject(current3.ID);
								}
							}
						}
						return;
					}
				}
				if (project.Type == SpecialProjectType.IndependentStudy)
				{
					if (project.PlayerID == this._app.LocalPlayer.ID)
					{
						PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(project.TargetPlayerID);
						if (playerInfo != null)
						{
							this.App.UI.CreateDialog(new IndependentStudied(this._app, project.TargetPlayerID), null);
							return;
						}
					}
				}
				else
				{
					if (project.Type == SpecialProjectType.RadiationShielding)
					{
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_RADIATION_SHIELDING_PROJECT_COMPLETE,
							EventMessage = TurnEventMessage.EM_RADIATION_SHIELDING_PROJECT_COMPLETE,
							PlayerID = project.PlayerID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							SpecialProjectID = project.ID,
							ShowsDialog = false
						});
						return;
					}
					if (project.Type == SpecialProjectType.NeutronStar)
					{
						NeutronStarInfo neutronStarInfo = this._app.GameDatabase.GetNeutronStarInfo(project.EncounterID);
						if (neutronStarInfo != null)
						{
							this._app.GameDatabase.RemoveFleet(neutronStarInfo.FleetId);
							this._app.GameDatabase.RemoveEncounter(neutronStarInfo.Id);
						}
						this._app.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_NEUTRONSTAR_PROJECT_COMPLETE,
							EventMessage = TurnEventMessage.EM_NEUTRONSTAR_PROJECT_COMPLETE,
							PlayerID = project.PlayerID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							SpecialProjectID = project.ID,
							ShowsDialog = false
						});
						foreach (int current4 in this.App.GameDatabase.GetStandardPlayerIDs())
						{
							List<SpecialProjectInfo> list3 = this.App.GameDatabase.GetSpecialProjectInfosByPlayerID(current4, true).ToList<SpecialProjectInfo>();
							foreach (SpecialProjectInfo current5 in list3)
							{
								if (current5.EncounterID == project.EncounterID)
								{
									this._app.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_NEUTRONSTAR_DESTROYED,
										EventMessage = TurnEventMessage.EM_NEUTRONSTAR_DESTROYED,
										PlayerID = project.PlayerID,
										TurnNumber = this._app.GameDatabase.GetTurnCount(),
										SpecialProjectID = project.ID,
										ShowsDialog = false
									});
									this.GameDatabase.RemoveSpecialProject(current5.ID);
								}
							}
						}
						if (!neutronStarInfo.DeepSpaceSystemId.HasValue)
						{
							return;
						}
						List<StationInfo> list4 = this._app.GameDatabase.GetStationForSystem(neutronStarInfo.DeepSpaceSystemId.Value).ToList<StationInfo>();
						foreach (StationInfo current6 in list4)
						{
							this._app.GameDatabase.DestroyStation(this._app.Game, current6.ID, 0);
						}
						List<FleetInfo> list5 = this._app.GameDatabase.GetFleetInfoBySystemID(neutronStarInfo.DeepSpaceSystemId.Value, FleetType.FL_NORMAL).ToList<FleetInfo>();
						foreach (FleetInfo current7 in list5)
						{
                            Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, current7, true);
						}
						List<MissionInfo> list6 = this._app.GameDatabase.GetMissionsBySystemDest(neutronStarInfo.DeepSpaceSystemId.Value).ToList<MissionInfo>();
						foreach (MissionInfo current8 in list6)
						{
							FleetInfo fleetInfo4 = this._app.GameDatabase.GetFleetInfo(current8.FleetID);
							if (fleetInfo4 != null)
							{
                                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this, fleetInfo4, true);
							}
							else
							{
								this._app.GameDatabase.RemoveMission(current8.ID);
							}
						}
						this._app.GameDatabase.DestroyStarSystem(this, neutronStarInfo.DeepSpaceSystemId.Value);
						List<int> list7 = this._db.GetStandardPlayerIDs().ToList<int>();
						using (List<int>.Enumerator enumerator9 = list7.GetEnumerator())
						{
							while (enumerator9.MoveNext())
							{
								int current9 = enumerator9.Current;
								List<TurnEvent> list8 = (
									from x in this._app.GameDatabase.GetTurnEventsByTurnNumber(this._app.GameDatabase.GetTurnCount(), current9)
									where x.EventType == TurnEventType.EV_NEUTRON_STAR_NEARBY
									select x).ToList<TurnEvent>();
								foreach (TurnEvent current10 in list8)
								{
									this._app.GameDatabase.RemoveTurnEvent(current10.ID);
								}
							}
							return;
						}
					}
					if (project.Type == SpecialProjectType.Gardener)
					{
						GardenerInfo gardenerInfo = this._app.GameDatabase.GetGardenerInfo(project.EncounterID);
						if (gardenerInfo != null)
						{
							this._app.Game.ScriptModules.Gardeners.HandleGardenerCaptured(this._app.Game, this._app.GameDatabase, project.PlayerID, project.EncounterID);
							this._app.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_GARDENER_PROJECT_COMPLETE,
								EventMessage = TurnEventMessage.EM_GARDENER_PROJECT_COMPLETE,
								PlayerID = project.PlayerID,
								TurnNumber = this._app.GameDatabase.GetTurnCount(),
								SpecialProjectID = project.ID,
								ShowsDialog = false
							});
						}
						foreach (int current11 in this.App.GameDatabase.GetStandardPlayerIDs())
						{
							List<SpecialProjectInfo> list9 = this.App.GameDatabase.GetSpecialProjectInfosByPlayerID(current11, true).ToList<SpecialProjectInfo>();
							foreach (SpecialProjectInfo current12 in list9)
							{
								if (current12.EncounterID == project.EncounterID)
								{
									this._app.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_GARDENER_CAPTURED,
										EventMessage = TurnEventMessage.EM_GARDENER_CAPTURED,
										PlayerID = project.PlayerID,
										TurnNumber = this._app.GameDatabase.GetTurnCount(),
										SpecialProjectID = project.ID,
										ShowsDialog = false
									});
									this.GameDatabase.RemoveSpecialProject(current12.ID);
								}
							}
						}
					}
				}
			}
		}
		private int UpdateResearchProjects(int playerId, int availableResearchPoints)
		{
			int num = 0;
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(playerId);
			if (playerInfo != null)
			{
				List<ResearchProjectInfo> list = this._db.GetResearchProjectInfos(playerId).ToList<ResearchProjectInfo>();
				foreach (ResearchProjectInfo current in list)
				{
					FeasibilityStudyInfo feasibilityStudyInfo = this._db.GetFeasibilityStudyInfo(current.ID);
					if (feasibilityStudyInfo != null)
					{
						num += this.UpdateFeasibilityStudy(current, feasibilityStudyInfo, availableResearchPoints);
					}
				}
				int num2 = (int)(playerInfo.RateResearchSalvageResearch * (float)availableResearchPoints);
				int num3 = num2;
				IEnumerable<SpecialProjectInfo> enumerable = 
					from x in this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(playerId, true)
					where x.Type == SpecialProjectType.Salvage && x.Progress >= 0
					select x;
				foreach (SpecialProjectInfo current2 in enumerable)
				{
					if (num3 <= 0)
					{
						break;
					}
					int num4 = (int)(current2.Rate * (float)num3);
					if (num3 - num4 < 0)
					{
						num4 = num3;
						num3 = 0;
					}
					else
					{
						num3 -= num4;
					}
					current2.Progress += num4;
					if (current2.Progress > current2.Cost)
					{
						num3 += current2.Progress - current2.Cost;
						current2.Progress = current2.Cost;
					}
					this._app.GameDatabase.UpdateSpecialProjectProgress(current2.ID, current2.Progress);
					if (current2.Progress == current2.Cost)
					{
						this.CompleteSpecialProject(current2);
					}
				}
				num += num2 - num3;
				int num5 = (int)(playerInfo.RateResearchSpecialProject * (float)availableResearchPoints);
				num3 = num5;
				List<SpecialProjectInfo> list2 = (
					from x in this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(playerId, true)
					where x.Type != SpecialProjectType.Salvage && x.Progress >= 0
					select x).ToList<SpecialProjectInfo>();
				foreach (SpecialProjectInfo project in list2)
				{
					if (num3 <= 0)
					{
						break;
					}
					int num6 = (int)(project.Rate * (float)num3);
					if (project.Type == SpecialProjectType.AsteroidMonitor)
					{
						num6 = (int)((float)num6 * this._db.GetStratModifierFloatToApply(StratModifiers.AsteroidMonitorResearchModifier, playerId));
					}
					if (project.Type == SpecialProjectType.NeutronStar)
					{
						NeutronStarInfo neutronStarInfo = this._app.GameDatabase.GetNeutronStarInfo(project.EncounterID);
						if (neutronStarInfo == null || !neutronStarInfo.DeepSpaceSystemId.HasValue)
						{
							this._app.GameDatabase.RemoveSpecialProject(project.ID);
							continue;
						}
						List<StationInfo> list3 = this._app.GameDatabase.GetStationForSystem(neutronStarInfo.DeepSpaceSystemId.Value).ToList<StationInfo>();
						if (list3.Count == 0)
						{
							continue;
						}
						if (!list3.Any((StationInfo x) => x.PlayerID == project.PlayerID))
						{
							continue;
						}
					}
					else
					{
						if (project.Type == SpecialProjectType.Gardener)
						{
							GardenerInfo gardenerInfo = this._app.GameDatabase.GetGardenerInfo(project.EncounterID);
							if (gardenerInfo == null || !gardenerInfo.DeepSpaceSystemId.HasValue)
							{
								this._app.GameDatabase.RemoveSpecialProject(project.ID);
								continue;
							}
							List<StationInfo> list4 = this._app.GameDatabase.GetStationForSystem(gardenerInfo.DeepSpaceSystemId.Value).ToList<StationInfo>();
							if (list4.Count == 0)
							{
								continue;
							}
							if (!list4.Any((StationInfo x) => x.PlayerID == project.PlayerID))
							{
								continue;
							}
						}
					}
					if (num3 - num6 < 0)
					{
						num6 = num3;
						num3 = 0;
					}
					else
					{
						num3 -= num6;
					}
					project.Progress += num6;
					if (project.Progress > project.Cost)
					{
						num3 += project.Progress - project.Cost;
						project.Progress = project.Cost;
					}
					this._app.GameDatabase.UpdateSpecialProjectProgress(project.ID, project.Progress);
					if (project.Progress == project.Cost)
					{
						this.CompleteSpecialProject(project);
					}
				}
				num += num5 - num3;
			}
			return num;
		}
		private float CalcResearchOdds(PlayerTechInfo techInfo)
		{
			if (techInfo.Feasibility <= 0f)
			{
				return 0f;
			}
			int num = (int)(0.5f * (float)techInfo.ResearchCost);
			int num2 = (int)(1.5f * (float)techInfo.ResearchCost);
			if (techInfo.Progress < num)
			{
				return 0f;
			}
			if (techInfo.Progress >= num2)
			{
				return 1f;
			}
			return (float)(techInfo.Progress - num) / (float)num2;
		}
		public int ConvertToResearchPoints(int playerId, double credits)
		{
			double num = credits / 100.0;
			return (int)(num * (double)this.GetGeneralResearchModifier(playerId, false));
		}
		internal static double SplitResearchRevenue(PlayerInfo player, double availableRevenue)
		{
			if (availableRevenue < 0.0)
			{
				return 0.0;
			}
			return Math.Floor(availableRevenue * (1.0 - (double)player.RateGovernmentResearch) * (double)player.RateResearchCurrentProject);
		}
		internal static double SplitSpecialProjectRevenue(PlayerInfo player, double availableRevenue)
		{
			if (availableRevenue < 0.0)
			{
				return 0.0;
			}
			return Math.Floor(availableRevenue * (1.0 - (double)player.RateGovernmentResearch) * (double)player.RateResearchSpecialProject);
		}
		internal static double SplitSalvageProjectRevenue(PlayerInfo player, double availableRevenue)
		{
			if (availableRevenue < 0.0)
			{
				return 0.0;
			}
			return Math.Floor(availableRevenue * (1.0 - (double)player.RateGovernmentResearch) * (double)player.RateResearchSalvageResearch);
		}
		public int GetAvailableResearchPoints(PlayerInfo player)
		{
			double val = this.CalculateNetRevenue(player);
			double credits = GameSession.SplitResearchRevenue(player, Math.Max(val, 0.0));
			return this.ConvertToResearchPoints(player.ID, credits) + this.ConvertToResearchPoints(player.ID, player.ResearchBoostFunds) * 2;
		}
		private float UpdateResearch(int playerId, float availableCredits)
		{
			float num = 0f;
			PlayerInfo playerInfo = this._db.GetPlayerInfo(playerId);
			int num2 = (int)GameSession.SplitSalvageProjectRevenue(playerInfo, (double)availableCredits);
			int num3 = (int)GameSession.SplitSpecialProjectRevenue(playerInfo, (double)availableCredits);
			int num4 = (int)GameSession.SplitResearchRevenue(playerInfo, (double)availableCredits);
			if (num4 + num2 + num3 <= 0 && playerInfo.ResearchBoostFunds <= 0.0)
			{
				return num;
			}
			int num5 = this.ConvertToResearchPoints(playerId, (double)num4) + this.ConvertToResearchPoints(playerId, (double)num3) + this.ConvertToResearchPoints(playerId, (double)num2);
			if (this.GetPlayerObject(playerId).IsAI())
			{
				num5 = (int)((float)num5 * 1.5f);
			}
			int num6 = this.ConvertToResearchPoints(playerId, playerInfo.ResearchBoostFunds) * 2;
			this.App.GameDatabase.UpdatePlayerSavings(playerId, playerInfo.Savings - playerInfo.ResearchBoostFunds);
			num5 += num6;
			num5 += playerInfo.AdditionalResearchPoints;
			this._db.UpdatePlayerAdditionalResearchPoints(playerId, 0);
			num5 = (int)((float)num5 * (this._db.GetNameValue<float>("ResearchEfficiency") / 100f));
			num5 -= this.UpdateResearchProjects(playerId, num5);
			if (num5 <= 0)
			{
				return (float)num4 + num;
			}
			int playerResearchingTechID = this._db.GetPlayerResearchingTechID(playerId);
			int playerFeasibilityStudyTechId = this._db.GetPlayerFeasibilityStudyTechId(playerId);
			if (playerResearchingTechID == 0 && playerFeasibilityStudyTechId == 0)
			{
				if (!this._app.GameDatabase.GetTurnHasEventType(playerId, this._app.GameDatabase.GetTurnCount(), TurnEventType.EV_NO_RESEARCH))
				{
					this._app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_NO_RESEARCH,
						EventMessage = TurnEventMessage.EM_NO_RESEARCH,
						PlayerID = playerId,
						TurnNumber = this._app.GameDatabase.GetTurnCount()
					});
				}
				return num;
			}
			if (this.DoBoostResearchAccidentRoll(playerId))
			{
				return num;
			}
			PlayerTechInfo techInfo = this._db.GetPlayerTechInfo(playerId, playerResearchingTechID);
			string techFamily = techInfo.TechFileID.Substring(0, 3);
			if (techInfo.TechFileID == "ENG_Leviathian_Construction")
			{
				num5 = (int)((float)num5 * this._db.GetStratModifierFloatToApply(StratModifiers.LeviathanResearchModifier, playerId));
			}
			int num7;
			techInfo.Progress += (int)((float)num5 * (1f + this.GetFamilySpecificResearchModifier(playerId, techFamily, out num7)));
			num += (float)num4;
			bool flag = false;
			if (techInfo.Progress > techInfo.ResearchCost / 2)
			{
				float num8 = this.CalcResearchOdds(techInfo);
				if (this._random.CoinToss((double)(num8 * this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.ResearchBreakthroughModifier, playerId))))
				{
					techInfo.State = TechStates.Researched;
					if (num8 < 0.5f)
					{
						flag = true;
					}
				}
			}
			if (techInfo.Progress > techInfo.ResearchCost * 2 && techInfo.State != TechStates.Researched)
			{
				techInfo.State = TechStates.Locked;
				this._db.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_RESEARCH_NEVER_COMPLETE,
					EventMessage = TurnEventMessage.EM_RESEARCH_NEVER_COMPLETE,
					PlayerID = playerInfo.ID,
					TechID = techInfo.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			}
			if (techInfo.State == TechStates.Researched)
			{
				techInfo.TurnResearched = new int?(this.GameDatabase.GetTurnCount());
			}
			this._db.UpdatePlayerTechInfo(techInfo);
			if (techInfo.State == TechStates.Researched)
			{
				this._db.UpdateLockedTechs(this._app.AssetDatabase, playerId);
				if (this.GetPlayerObject(playerInfo.ID).IsAI())
				{
					Kerberos.Sots.Data.TechnologyFramework.Tech tech = this._app.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techInfo.TechFileID);
					AITechWeightInfo aITechWeightInfo = this._db.GetAITechWeightInfo(playerInfo.ID, tech.Family);
					if (aITechWeightInfo != null)
					{
						aITechWeightInfo.TotalSpent += (double)techInfo.ResearchCost;
						this._db.UpdateAITechWeight(aITechWeightInfo);
					}
				}
				string eventSoundCueName;
				if (flag)
				{
					eventSoundCueName = string.Format("STRAT_051-01_{0}_ResearchCompletedEarly", this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)));
				}
				else
				{
					eventSoundCueName = string.Format("STRAT_047-01_{0}_ResearchComplete", this.GameDatabase.GetFactionName(this.GameDatabase.GetPlayerFactionID(playerInfo.ID)));
				}
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_RESEARCH_COMPLETE,
					EventMessage = TurnEventMessage.EM_RESEARCH_COMPLETE,
					EventSoundCueName = eventSoundCueName,
					PlayerID = playerInfo.ID,
					TechID = techInfo.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
				App.UpdateStratModifiers(this, playerInfo.ID, techInfo.TechID);
				if (playerInfo.ID == this.LocalPlayer.ID)
				{
					if (techInfo.TechFileID == "BIO_Xombie_Plague")
					{
						this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_ZWORD);
					}
					bool flag2 = true;
					IEnumerable<PlayerTechInfo> playerTechInfos = this._db.GetPlayerTechInfos(playerId);
					foreach (PlayerTechInfo current in playerTechInfos)
					{
						if (current.State != TechStates.Researched)
						{
							flag2 = false;
						}
					}
					if (flag2)
					{
						this.App.SteamHelper.DoAchievement(AchievementType.SOTS2_DR_SLEEPLESS);
					}
				}
			}
			return num;
		}
		private int UpdateFeasibilityStudy(ResearchProjectInfo projectInfo, FeasibilityStudyInfo feasibilityInfo, int availableResearchPoints)
		{
			int num = 0;
			PlayerTechInfo playerTechInfo = this._db.GetPlayerTechInfo(projectInfo.PlayerID, feasibilityInfo.TechID);
			float num2 = (float)availableResearchPoints / feasibilityInfo.ResearchCost;
			if (num2 + projectInfo.Progress > 1f)
			{
				num += (int)((1f - projectInfo.Progress) * feasibilityInfo.ResearchCost);
				float stratModifier = this._db.GetStratModifier<float>(StratModifiers.TechFeasibilityDeviation, projectInfo.PlayerID);
				playerTechInfo.PlayerFeasibility = Math.Min(Math.Max(playerTechInfo.Feasibility + this._random.NextSingle() % stratModifier - stratModifier / 2f, 0.01f), 0.99f);
				playerTechInfo.Progress += (int)(feasibilityInfo.ResearchCost / 2f);
				this._db.UpdatePlayerTechInfo(playerTechInfo);
				string factionName = this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID));
				string eventSoundCueName;
				if (playerTechInfo.PlayerFeasibility < 0.3f)
				{
					eventSoundCueName = string.Format("STRAT_050-01_{0}_FeasibilityStudyComplete-Less30Percent", factionName);
				}
				else
				{
					if (playerTechInfo.PlayerFeasibility > 0.8f)
					{
						eventSoundCueName = string.Format("STRAT_048-01_{0}_FeasibilityStudyComplete-Over80Percent", factionName);
					}
					else
					{
						eventSoundCueName = string.Format("STRAT_049-01_{0}_FeasibilityStudyComplete-31-79Percent", factionName);
					}
				}
				this._db.UpdatePlayerTechState(projectInfo.PlayerID, feasibilityInfo.TechID, ((double)playerTechInfo.PlayerFeasibility >= 0.5) ? TechStates.HighFeasibility : TechStates.LowFeasibility);
				this._db.RemoveFeasibilityStudy(projectInfo.ID);
				int num3 = (int)Math.Round((double)(playerTechInfo.PlayerFeasibility * 100f));
				TurnEventMessage eventMessage;
				if (num3 < 26)
				{
					eventMessage = TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_VERY_BAD;
				}
				else
				{
					if (num3 < 51)
					{
						eventMessage = TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_BAD;
					}
					else
					{
						if (num3 < 81)
						{
							eventMessage = TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_GOOD;
						}
						else
						{
							eventMessage = TurnEventMessage.EM_FEASIBILITY_STUDY_COMPLETE_VERY_GOOD;
						}
					}
				}
				this._app.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_FEASIBILITY_STUDY_COMPLETE,
					EventMessage = eventMessage,
					EventSoundCueName = eventSoundCueName,
					PlayerID = projectInfo.PlayerID,
					FeasibilityPercent = num3,
					TechID = playerTechInfo.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = true
				});
			}
			else
			{
				projectInfo.Progress += num2;
				num = availableResearchPoints;
				this._db.UpdateResearchProjectInfo(projectInfo);
			}
			return num;
		}
		public static Dictionary<ModuleEnums.StationModuleType, int> CountStationModuleTypes(StationInfo station, GameDatabase gamedb, bool groupModules = true)
		{
			string factionName = gamedb.GetFactionName(gamedb.GetPlayerFactionID(station.PlayerID));
			Dictionary<ModuleEnums.StationModuleType, int> dictionary = new Dictionary<ModuleEnums.StationModuleType, int>();
			List<DesignModuleInfo> modules = station.DesignInfo.DesignSections[0].Modules;
			foreach (DesignModuleInfo current in modules)
			{
				if (groupModules)
				{
					if (ModuleEnums.LabTypes.Contains(current.StationModuleType.Value))
					{
						if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Lab))
						{
							Dictionary<ModuleEnums.StationModuleType, int> dictionary2;
							(dictionary2 = dictionary)[ModuleEnums.StationModuleType.Lab] = dictionary2[ModuleEnums.StationModuleType.Lab] + 1;
						}
						else
						{
							dictionary.Add(ModuleEnums.StationModuleType.Lab, 1);
						}
					}
					else
					{
						if (ModuleEnums.HabitationModuleTypes.Contains(current.StationModuleType.Value))
						{
							ModuleEnums.StationModuleType stationModuleType = (ModuleEnums.FactionHabitationModules[factionName] == current.StationModuleType) ? ModuleEnums.StationModuleType.Habitation : ModuleEnums.StationModuleType.AlienHabitation;
							if (dictionary.ContainsKey(stationModuleType))
							{
								Dictionary<ModuleEnums.StationModuleType, int> dictionary3;
								ModuleEnums.StationModuleType key;
								(dictionary3 = dictionary)[key = stationModuleType] = dictionary3[key] + 1;
							}
							else
							{
								dictionary.Add(stationModuleType, 1);
							}
						}
						else
						{
							if (ModuleEnums.TradeModuleTypes.Contains(current.StationModuleType.Value))
							{
								ModuleEnums.StationModuleType stationModuleType2 = ModuleEnums.StationModuleType.Trade;
								if (dictionary.ContainsKey(stationModuleType2))
								{
									Dictionary<ModuleEnums.StationModuleType, int> dictionary4;
									ModuleEnums.StationModuleType key2;
									(dictionary4 = dictionary)[key2 = stationModuleType2] = dictionary4[key2] + 1;
								}
								else
								{
									dictionary.Add(stationModuleType2, 1);
								}
							}
							else
							{
								if (ModuleEnums.LargeHabitationModuleTypes.Contains(current.StationModuleType.Value))
								{
									ModuleEnums.StationModuleType stationModuleType3 = (ModuleEnums.FactionLargeHabitationModules[factionName] == current.StationModuleType) ? ModuleEnums.StationModuleType.LargeHabitation : ModuleEnums.StationModuleType.LargeAlienHabitation;
									if (dictionary.ContainsKey(stationModuleType3))
									{
										Dictionary<ModuleEnums.StationModuleType, int> dictionary5;
										ModuleEnums.StationModuleType key3;
										(dictionary5 = dictionary)[key3 = stationModuleType3] = dictionary5[key3] + 1;
									}
									else
									{
										dictionary.Add(stationModuleType3, 1);
									}
								}
								else
								{
									if (dictionary.ContainsKey(current.StationModuleType.Value))
									{
										Dictionary<ModuleEnums.StationModuleType, int> dictionary6;
										ModuleEnums.StationModuleType value;
										(dictionary6 = dictionary)[value = current.StationModuleType.Value] = dictionary6[value] + 1;
									}
									else
									{
										dictionary.Add(current.StationModuleType.Value, 1);
									}
								}
							}
						}
					}
				}
				else
				{
					if (dictionary.ContainsKey(current.StationModuleType.Value))
					{
						Dictionary<ModuleEnums.StationModuleType, int> dictionary7;
						ModuleEnums.StationModuleType value2;
						(dictionary7 = dictionary)[value2 = current.StationModuleType.Value] = dictionary7[value2] + 1;
					}
					else
					{
						dictionary.Add(current.StationModuleType.Value, 1);
					}
				}
			}
			return dictionary;
		}
		private float CalculateStationUpgradeProgress(StationInfo station, Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> typeRequirements, out Dictionary<ModuleEnums.StationModuleType, int> requiredModules)
		{
			Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(station, this.GameDatabase, true);
			int num = 0;
			Dictionary<ModuleEnums.StationModuleType, int> dictionary2 = null;
			foreach (KeyValuePair<int, Dictionary<ModuleEnums.StationModuleType, int>> current in typeRequirements)
			{
				if (current.Key == station.DesignInfo.StationLevel)
				{
					dictionary2 = current.Value;
					num = dictionary2.Sum((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Value);
					break;
				}
				foreach (KeyValuePair<ModuleEnums.StationModuleType, int> current2 in current.Value)
				{
					if (dictionary.ContainsKey(current2.Key))
					{
						dictionary[current2.Key] = Math.Max(0, dictionary[current2.Key] - current2.Value);
					}
				}
			}
			if (dictionary2 == null)
			{
				requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>();
				return 0f;
			}
			requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>(dictionary2);
			int num2 = 0;
			foreach (KeyValuePair<ModuleEnums.StationModuleType, int> current3 in dictionary)
			{
				if (dictionary2.ContainsKey(current3.Key))
				{
					int num3 = Math.Min(current3.Value, dictionary2[current3.Key]);
					Dictionary<ModuleEnums.StationModuleType, int> dictionary3;
					ModuleEnums.StationModuleType key;
					(dictionary3 = requiredModules)[key = current3.Key] = dictionary3[key] - num3;
					num2 += num3;
				}
			}
			Dictionary<ModuleEnums.StationModuleType, int> dictionary4 = new Dictionary<ModuleEnums.StationModuleType, int>(requiredModules);
			foreach (KeyValuePair<ModuleEnums.StationModuleType, int> current4 in dictionary4)
			{
				if (current4.Value == 0)
				{
					requiredModules.Remove(current4.Key);
				}
			}
			return (float)num2 / (float)num;
		}
		public List<LogicalModuleMount> GetAvailableStationModuleMounts(StationInfo si)
		{
			List<LogicalModuleMount> stationModuleMounts = this.GetStationModuleMounts(si);
			foreach (DesignModuleInfo dmi in si.DesignInfo.DesignSections[0].Modules)
			{
				LogicalModuleMount logicalModuleMount = stationModuleMounts.FirstOrDefault((LogicalModuleMount x) => x.NodeName == dmi.MountNodeName);
				if (logicalModuleMount != null)
				{
					stationModuleMounts.Remove(logicalModuleMount);
				}
			}
			return stationModuleMounts;
		}
		public List<LogicalModuleMount> GetStationModuleMounts(StationInfo si)
		{
			return new List<LogicalModuleMount>(this.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == si.DesignInfo.DesignSections[0].FilePath).Modules);
		}
		public float GetStationUpgradeProgress(StationInfo station, out Dictionary<ModuleEnums.StationModuleType, int> requiredModules)
		{
			switch (station.DesignInfo.StationType)
			{
			case StationType.NAVAL:
				return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.NavalStationUpgradeRequirements, out requiredModules);
			case StationType.SCIENCE:
				return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.ScienceStationUpgradeRequirements, out requiredModules);
			case StationType.CIVILIAN:
				return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.CivilianStationUpgradeRequirements, out requiredModules);
			case StationType.DIPLOMATIC:
				return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.DiplomaticStationUpgradeRequirements, out requiredModules);
			case StationType.GATE:
				return this.CalculateStationUpgradeProgress(station, this.AssetDatabase.GateStationUpgradeRequirements, out requiredModules);
			default:
				requiredModules = new Dictionary<ModuleEnums.StationModuleType, int>();
				return 0f;
			}
		}
		public static int CalculateLVL1StationUpkeepCost(AssetDatabase assetdb, StationType stationType)
		{
			switch (stationType)
			{
			case StationType.NAVAL:
				return assetdb.UpkeepNavalStation[0];
			case StationType.SCIENCE:
				return assetdb.UpkeepScienceStation[0];
			case StationType.CIVILIAN:
				return assetdb.UpkeepCivilianStation[0];
			case StationType.DIPLOMATIC:
				return assetdb.UpkeepDiplomaticStation[0];
			case StationType.GATE:
				return assetdb.UpkeepGateStation[0];
			default:
				return 0;
			}
		}
		public static int CalculateStationUpkeepCost(GameDatabase gamedb, AssetDatabase assetdb, StationInfo si)
		{
			int num = 0;
			foreach (DesignModuleInfo current in si.DesignInfo.DesignSections[0].Modules)
			{
				string moduleAsset = gamedb.GetModuleAsset(current.ModuleID);
				LogicalModule logicalModule = assetdb.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == moduleAsset);
				if (logicalModule != null)
				{
					num += logicalModule.UpkeepCost;
				}
			}
			int num2 = si.DesignInfo.StationLevel - 1;
			if (num2 >= 0)
			{
				switch (si.DesignInfo.StationType)
				{
				case StationType.NAVAL:
					return num + assetdb.UpkeepNavalStation[num2];
				case StationType.SCIENCE:
				{
					int num3 = num + assetdb.UpkeepScienceStation[num2];
					float num4 = 0f;
					List<DesignModuleInfo> modules = si.DesignInfo.DesignSections[0].Modules;
					using (List<DesignModuleInfo>.Enumerator enumerator2 = modules.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current.StationModuleType == ModuleEnums.StationModuleType.Dock)
							{
								num4 += (float)num3 * 0.02f;
							}
						}
					}
					return num3 - (int)num4;
				}
				case StationType.CIVILIAN:
					return num + assetdb.UpkeepCivilianStation[num2];
				case StationType.DIPLOMATIC:
				{
					int num5 = num + assetdb.UpkeepDiplomaticStation[num2];
					float num6 = 0f;
					List<DesignModuleInfo> modules2 = si.DesignInfo.DesignSections[0].Modules;
					using (List<DesignModuleInfo>.Enumerator enumerator3 = modules2.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							if (enumerator3.Current.StationModuleType == ModuleEnums.StationModuleType.Dock)
							{
								num6 += (float)num5 * 0.02f;
							}
						}
					}
					return num5 - (int)num6;
				}
				case StationType.GATE:
				{
					int num7 = num + assetdb.UpkeepGateStation[num2];
					float num8 = 0f;
					List<DesignModuleInfo> modules3 = si.DesignInfo.DesignSections[0].Modules;
					using (List<DesignModuleInfo>.Enumerator enumerator4 = modules3.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							if (enumerator4.Current.StationModuleType == ModuleEnums.StationModuleType.Dock)
							{
								num8 += (float)num7 * 0.02f;
							}
						}
					}
					return num7 - (int)num8;
				}
				}
			}
			return 0;
		}
		public static int CalculateUpkeepCost(int DesignId, App game)
		{
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(DesignId);
			switch (designInfo.Class)
			{
			case ShipClass.Cruiser:
				return game.AssetDatabase.UpkeepCruiser;
			case ShipClass.Dreadnought:
				return game.AssetDatabase.UpkeepDreadnaught;
			case ShipClass.Leviathan:
				return game.AssetDatabase.UpkeepLeviathan;
			case ShipClass.BattleRider:
				return game.AssetDatabase.UpkeepBattleRider;
			default:
				return 0;
			}
		}
		public static double CalculateStationUpkeepCosts(GameDatabase gamedb, AssetDatabase assetdb, IEnumerable<StationInfo> stationInfos)
		{
			return (double)stationInfos.Sum((StationInfo x) => GameSession.CalculateStationUpkeepCost(gamedb, assetdb, x));
		}
		public static double CalculateShipUpkeepCost(AssetDatabase assetdb, DesignInfo shipDesignInfo, float scale, bool isInReserveFleet)
		{
			double num = 0.0;
			switch (shipDesignInfo.Class)
			{
			case ShipClass.Cruiser:
				num = (double)assetdb.UpkeepCruiser;
				break;
			case ShipClass.Dreadnought:
				num = (double)assetdb.UpkeepDreadnaught;
				break;
			case ShipClass.Leviathan:
				num = (double)assetdb.UpkeepLeviathan;
				break;
			case ShipClass.BattleRider:
				num = (double)assetdb.UpkeepBattleRider;
				break;
			case ShipClass.Station:
				if (shipDesignInfo.StationType == StationType.INVALID_TYPE)
				{
					num = (double)assetdb.UpkeepDefensePlatform;
				}
				break;
			}
			if (isInReserveFleet)
			{
				num /= 3.0;
			}
			return num * (double)scale;
		}
		public static double CalculateShipUpkeepCosts(AssetDatabase assetdb, IEnumerable<DesignInfo> shipDesignInfos, float scale, bool areInReserveFleets)
		{
			if (shipDesignInfos == null)
			{
				return 0.0;
			}
			return shipDesignInfos.Sum((DesignInfo x) => GameSession.CalculateShipUpkeepCost(assetdb, x, scale, areInReserveFleets));
		}
		public static List<DesignInfo> MergeShipDesignInfos(GameDatabase db, IEnumerable<FleetInfo> allFleetInfos, bool reserveFleets)
		{
			IEnumerable<FleetInfo> source = 
				from x in allFleetInfos
				where x.IsReserveFleet == reserveFleets
				select x;
			IEnumerable<ShipInfo> source2 = source.SelectMany((FleetInfo x) => db.GetShipInfoByFleetID(x.ID, false));
			return (
				from x in source2
				select db.GetDesignInfo(x.DesignID)).ToList<DesignInfo>();
		}
		public static double CalculateFleetUpkeepCosts(AssetDatabase assetdb, IEnumerable<DesignInfo> reserveShipDesignInfos, IEnumerable<DesignInfo> shipDesignInfos, IEnumerable<DesignInfo> eliteShipDesignInfos)
		{
			double num = GameSession.CalculateShipUpkeepCosts(assetdb, eliteShipDesignInfos, assetdb.EliteUpkeepCostScale, false);
			double num2 = GameSession.CalculateShipUpkeepCosts(assetdb, shipDesignInfos, 1f, false);
			double num3 = GameSession.CalculateShipUpkeepCosts(assetdb, reserveShipDesignInfos, 1f, true);
			return num + num2 + num3;
		}
		public static double CalculateUpkeepCosts(AssetDatabase assetdb, GameDatabase db, int playerId)
		{
			List<StationInfo> stationInfos = db.GetStationInfosByPlayerID(playerId).ToList<StationInfo>();
			List<FleetInfo> list = db.GetFleetInfosByPlayerID(playerId, FleetType.FL_ALL).ToList<FleetInfo>();
			List<FleetInfo> eliteFleetInfos = (
				from x in list
				where db.GetAdmiralTraits(x.AdmiralID).Contains(AdmiralInfo.TraitType.Elite)
				select x).ToList<FleetInfo>();
			if (eliteFleetInfos.Count > 0)
			{
				list.RemoveAll((FleetInfo x) => eliteFleetInfos.Contains(x));
			}
			List<DesignInfo> reserveShipDesignInfos = GameSession.MergeShipDesignInfos(db, list, true);
			List<DesignInfo> shipDesignInfos = GameSession.MergeShipDesignInfos(db, list, false);
			List<DesignInfo> eliteShipDesignInfos = GameSession.MergeShipDesignInfos(db, eliteFleetInfos, false);
			return GameSession.CalculateFleetUpkeepCosts(assetdb, reserveShipDesignInfos, shipDesignInfos, eliteShipDesignInfos) + GameSession.CalculateStationUpkeepCosts(db, assetdb, stationInfos);
		}
		public static double CalculateDebtInterest(GameSession game, PlayerInfo ply)
		{
			if (game.AssetDatabase.GetFaction(ply.FactionID).Name == "loa")
			{
				return 0.0;
			}
			double num = 0.15;
			Kerberos.Sots.PlayerFramework.Player playerObject = game.GetPlayerObject(ply.ID);
			if (playerObject != null && playerObject.IsAI())
			{
				num = 0.05;
			}
			double num2 = Math.Max(-ply.Savings, 0.0);
			return num2 * num;
		}
		public static double CalculateSavingsInterest(GameSession game, PlayerInfo ply)
		{
			if (game.AssetDatabase.GetFaction(ply.FactionID).Name == "loa")
			{
				return 0.0;
			}
			double num = Math.Max(ply.Savings, 0.0);
			return num * 0.01;
		}
		private void CollectDiplomacyPoints()
		{
			foreach (PlayerInfo p in this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				List<ProvinceInfo> list = (
					from x in this.GameDatabase.GetProvinceInfos()
					where x.PlayerID == p.ID
					select x).ToList<ProvinceInfo>();
				int num = p.GenericDiplomacyPoints + list.Count * this.AssetDatabase.DiplomacyPointsPerProvince;
				Dictionary<int, int> factionDiplomacyPoints = this.App.GameDatabase.GetFactionDiplomacyPoints(p.ID);
				List<StationInfo> list2 = (
					from x in this.GameDatabase.GetStationInfosByPlayerID(p.ID)
					where x.DesignInfo.StationType == StationType.DIPLOMATIC
					select x).ToList<StationInfo>();
				foreach (StationInfo current in list2)
				{
					if (current.DesignInfo.StationLevel > 0)
					{
						num += this.AssetDatabase.DiplomacyPointsPerStation[current.DesignInfo.StationLevel - 1];
						List<DesignModuleInfo> list3 = current.DesignInfo.DesignSections[0].Modules.ToList<DesignModuleInfo>();
						foreach (DesignModuleInfo current2 in list3)
						{
							if (current.DesignInfo.StationType == StationType.DIPLOMATIC)
							{
								if (AssetDatabase.StationModuleTypeToMountTypeMap[current2.StationModuleType.Value] == ModuleEnums.ModuleSlotTypes.Habitation || AssetDatabase.StationModuleTypeToMountTypeMap[current2.StationModuleType.Value] == ModuleEnums.ModuleSlotTypes.AlienHabitation)
								{
									if (current2.StationModuleType == ModuleEnums.FactionHabitationModules[this.App.GameDatabase.GetFactionName(p.FactionID)])
									{
										num++;
									}
									else
									{
										string text = current2.StationModuleType.ToString();
										text = text.Replace("Foreign", "");
										ModuleEnums.StationModuleType smt = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), text);
										Dictionary<int, int> dictionary;
										int factionIdFromName;
										(dictionary = factionDiplomacyPoints)[factionIdFromName = this.App.GameDatabase.GetFactionIdFromName(ModuleEnums.FactionHabitationModules.First((KeyValuePair<string, ModuleEnums.StationModuleType> x) => x.Value == smt).Key)] = dictionary[factionIdFromName] + 1;
									}
								}
								else
								{
									if (AssetDatabase.StationModuleTypeToMountTypeMap[current2.StationModuleType.Value] == ModuleEnums.ModuleSlotTypes.LargeHabitation || AssetDatabase.StationModuleTypeToMountTypeMap[current2.StationModuleType.Value] == ModuleEnums.ModuleSlotTypes.LargeAlienHabitation)
									{
										if (current2.StationModuleType == ModuleEnums.FactionLargeHabitationModules[this.App.GameDatabase.GetFactionName(p.FactionID)])
										{
											num += 3;
										}
										else
										{
											string text2 = current2.StationModuleType.ToString();
											text2 = text2.Replace("Foreign", "");
											ModuleEnums.StationModuleType smt = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), text2);
											Dictionary<int, int> dictionary2;
											int factionIdFromName2;
											(dictionary2 = factionDiplomacyPoints)[factionIdFromName2 = this.App.GameDatabase.GetFactionIdFromName(ModuleEnums.FactionLargeHabitationModules.First((KeyValuePair<string, ModuleEnums.StationModuleType> x) => x.Value == smt).Key)] = dictionary2[factionIdFromName2] + 3;
										}
									}
								}
							}
						}
					}
				}
				foreach (KeyValuePair<int, int> current3 in factionDiplomacyPoints)
				{
					this._db.UpdateFactionDiplomacyPoints(p.ID, current3.Key, current3.Value);
				}
				this._db.UpdateGenericDiplomacyPoints(p.ID, num);
			}
		}
		internal double CalculateNetRevenue(PlayerInfo player)
		{
			double tradeRevenue = 0.0;
			this._incomeFromTrade.TryGetValue(player.ID, out tradeRevenue);
			return new GameSession.NetRevenueSummary(this._app, player.ID, tradeRevenue).GetNetRevenue();
		}
		private void CollectTaxes()
		{
			List<PlayerInfo> list = this._db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				if (!current.isDefeated)
				{
					if (current.RateTaxPrev != current.RateTax)
					{
						int num = (int)Math.Floor((double)((current.RateTaxPrev - current.RateTax) * 100f + 0.5f));
						if (num != 0)
						{
							this.App.GameDatabase.InsertGovernmentAction(this.App.LocalPlayer.ID, (num > 0) ? App.Localize("@GA_TAXDECREASED") : App.Localize("@GA_TAXINCREASED"), "", -num, -num);
						}
						current.RateTaxPrev = current.RateTax;
						this._db.UpdatePreviousTaxRate(current.ID, current.RateTaxPrev);
					}
					double num2 = this.CalculateNetRevenue(current);
					double num3 = GameSession.CalculateDebtInterest(this, current);
					int playerBankruptcyTurns = this._db.GetPlayerBankruptcyTurns(current.ID);
					if (current.Savings < -5000000.0 && num2 + num3 < num3)
					{
						this.App.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_BANKRUPTCY_IMMINENT,
							EventMessage = TurnEventMessage.EM_BANKRUPTCY_IMMINENT,
							PlayerID = current.ID,
							TurnNumber = this.App.GameDatabase.GetTurnCount()
						});
						this._db.UpdatePlayerBankruptcyTurns(current.ID, playerBankruptcyTurns + 1);
						GameSession.SimAITurns = 0;
					}
					else
					{
						if (playerBankruptcyTurns != 0)
						{
							this.App.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_BANKRUPTCY_AVOIDED,
								EventMessage = TurnEventMessage.EM_BANKRUPTCY_AVOIDED,
								PlayerID = current.ID,
								TurnNumber = this.App.GameDatabase.GetTurnCount()
							});
							this._db.UpdatePlayerBankruptcyTurns(current.ID, 0);
						}
					}
					if (num2 > 0.0 || (current.ResearchBoostFunds > 0.0 && (!(this.AssetDatabase.GetFaction(current.FactionID).Name == "loa") || current.Savings >= 0.0)))
					{
						num2 -= (double)this.UpdateResearch(current.ID, (float)num2);
						current.Savings = this.GameDatabase.GetPlayerInfo(current.ID).Savings;
						if (num2 > 0.0)
						{
							this.UpdateGovernmentSpending(current, (double)((float)num2));
						}
						else
						{
							this.UpdateSavingsSpending(current, (double)((float)num2));
						}
					}
					else
					{
						this.UpdateSavingsSpending(current, (double)((float)num2));
					}
					this.GameDatabase.UpdatePlayerResearchBoost(current.ID, 0.0);
					PlayerInfo playerInfo = this._db.GetPlayerInfo(current.ID);
					Budget budget = Budget.GenerateBudget(this.App.Game, playerInfo, null, BudgetProjection.Pessimistic);
					if (this._db.GetSliderNotchSettingInfo(playerInfo.ID, UISlidertype.SecuritySlider) != null)
					{
						EmpireSummaryState.DistributeGovernmentSpending(this.App.Game, EmpireSummaryState.GovernmentSpendings.Security, (float)Math.Min((double)((float)budget.RequiredSecurity / 100f), 1.0), playerInfo);
					}
				}
			}
		}
		private void UpdateConsumableShipStats()
		{
			List<PlayerInfo> list = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				List<ColonyInfo> list2 = this._db.GetPlayerColoniesByPlayerId(current.ID).ToList<ColonyInfo>();
				float num = 0f;
				double num2 = 0.0;
				foreach (ColonyInfo current2 in list2)
				{
					PlanetInfo planetInfo = this._db.GetPlanetInfo(current2.OrbitalObjectID);
					num += (float)planetInfo.Biosphere;
					num2 += current2.ImperialPop * (double)this.AssetDatabase.GetFaction(current.FactionID).PsionicPowerModifier;
					ColonyFactionInfo[] factions = current2.Factions;
					for (int i = 0; i < factions.Length; i++)
					{
						ColonyFactionInfo colonyFactionInfo = factions[i];
						num2 += colonyFactionInfo.CivilianPop * (double)this.AssetDatabase.GetFaction(colonyFactionInfo.FactionID).PsionicPowerModifier;
					}
				}
				int newPotential = (int)((num / 10f + (float)(num2 / 10000000.0)) * this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PsiPotentialModifier, current.ID));
				this._db.UpdatePsionicPotential(current.ID, newPotential);
			}
			List<ShipInfo> list3 = this._db.GetShipInfos(true).ToList<ShipInfo>();
			foreach (ShipInfo si in list3)
			{
				if (si.DesignInfo.PlayerID != 0)
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(si.FleetID);
					List<AdmiralInfo.TraitType> admiralTraits = (fleetInfo != null) ? this._db.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>() : new List<AdmiralInfo.TraitType>();
					int maxPsionicPower = ShipInfo.GetMaxPsionicPower(this.App, si.DesignInfo, admiralTraits);
					if (si.PsionicPower != maxPsionicPower)
					{
						float stratModifierFloatToApply = this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PsiPotentialApplyModifier, si.DesignInfo.PlayerID);
						si.PsionicPower += (int)((float)this._db.GetPlayerInfo(si.DesignInfo.PlayerID).PsionicPotential / stratModifierFloatToApply);
						si.PsionicPower = Math.Min(si.PsionicPower, ShipInfo.GetMaxPsionicPower(this.App, si.DesignInfo, admiralTraits));
						this._db.UpdateShipPsionicPower(si.ID, si.PsionicPower);
					}
					StationInfo stationInfo = this._db.GetStationInfos().FirstOrDefault((StationInfo x) => x.ShipID == si.ID);
					if ((fleetInfo != null && fleetInfo.SupportingSystemID == fleetInfo.SystemID) || stationInfo != null)
					{
						List<SectionInstanceInfo> list4 = this._db.GetShipSectionInstances(si.ID).ToList<SectionInstanceInfo>();
						foreach (SectionInstanceInfo sii in list4)
						{
							DesignSectionInfo designSectionInfo = si.DesignInfo.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ID == sii.SectionID);
							List<string> list5 = new List<string>();
							if (designSectionInfo != null && designSectionInfo.Techs.Count > 0)
							{
								foreach (int current3 in designSectionInfo.Techs)
								{
									list5.Add(this._db.GetTechFileID(current3));
								}
							}
							ShipSectionAsset shipSectionAsset = si.DesignInfo.DesignSections.First((DesignSectionInfo x) => x.ID == sii.SectionID).ShipSectionAsset;
							int supplyWithTech = Kerberos.Sots.GameObjects.Ship.GetSupplyWithTech(this.AssetDatabase, list5, shipSectionAsset.Supply);
							if (sii.Crew != shipSectionAsset.Crew || sii.Supply != supplyWithTech)
							{
								sii.Crew = shipSectionAsset.Crew;
								sii.Supply = supplyWithTech;
								this._db.UpdateSectionInstance(sii);
							}
						}
					}
				}
			}
		}
		private void UpdateRepairPoints()
		{
			List<ShipInfo> list = this._db.GetShipInfos(true).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				List<SectionInstanceInfo> list2 = this._db.GetShipSectionInstances(current.ID).ToList<SectionInstanceInfo>();
				foreach (SectionInstanceInfo sii in list2)
				{
					DesignSectionInfo designSectionInfo = current.DesignInfo.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ID == sii.SectionID);
					if (designSectionInfo != null)
					{
						if (sii.RepairPoints != designSectionInfo.ShipSectionAsset.RepairPoints)
						{
							sii.RepairPoints = designSectionInfo.ShipSectionAsset.RepairPoints;
							this._db.UpdateSectionInstance(sii);
						}
						foreach (ModuleInstanceInfo mii in sii.ModuleInstances)
						{
							DesignModuleInfo designModuleInfo = designSectionInfo.Modules.FirstOrDefault((DesignModuleInfo x) => x.MountNodeName == mii.ModuleNodeID);
							if (designModuleInfo != null)
							{
								string path = this._db.GetModuleAsset(designModuleInfo.ModuleID);
								LogicalModule logicalModule = this.AssetDatabase.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == path);
								if (mii.RepairPoints != logicalModule.RepairPointsBonus)
								{
									mii.RepairPoints = logicalModule.RepairPointsBonus;
									this._db.UpdateModuleInstance(mii);
								}
							}
						}
					}
				}
			}
		}
		private void HandleGives()
		{
			List<PlayerInfo> list = new List<PlayerInfo>();
			foreach (GiveInfo give in this.GameDatabase.GetGiveInfos())
			{
				if (!list.Any((PlayerInfo x) => x.ID == give.ReceivingPlayer))
				{
					list.Add(this.GameDatabase.GetPlayerInfo(give.ReceivingPlayer));
				}
				switch (give.Type)
				{
				case GiveType.GiveSavings:
					list.FirstOrDefault((PlayerInfo x) => x.ID == give.ReceivingPlayer).Savings += (double)give.GiveValue;
					this.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_RECEIVED_MONEY,
						EventMessage = TurnEventMessage.EM_RECEIVED_MONEY,
						PlayerID = give.ReceivingPlayer,
						TargetPlayerID = give.InitiatingPlayer,
						Savings = (double)give.GiveValue,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					break;
				case GiveType.GiveResearchPoints:
					list.FirstOrDefault((PlayerInfo x) => x.ID == give.ReceivingPlayer).AdditionalResearchPoints += this.ConvertToResearchPoints(give.ReceivingPlayer, (double)give.GiveValue);
					this.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_RECEIVED_RESEARCH_MONEY,
						EventMessage = TurnEventMessage.EM_RECEIVED_RESEARCH_MONEY,
						PlayerID = give.ReceivingPlayer,
						TargetPlayerID = give.InitiatingPlayer,
						Savings = (double)give.GiveValue,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					break;
				}
			}
			foreach (PlayerInfo current in list)
			{
				this.GameDatabase.UpdatePlayerAdditionalResearchPoints(current.ID, current.AdditionalResearchPoints);
				this.GameDatabase.UpdatePlayerSavings(current.ID, current.Savings);
			}
			this.GameDatabase.ClearGiveInfos();
		}
		private void UpdateMorale()
		{
			List<PlayerInfo> list = this.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				if (current.Savings < -3000000.0)
				{
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_3MILLION_DEBT, current.ID, null, null, null);
				}
				else
				{
					if (current.Savings < -1000000.0)
					{
						GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_1MILLION_DEBT, current.ID, null, null, null);
					}
					else
					{
						if (current.Savings > 25000000.0)
						{
							GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_25MILLION_SAVINGS, current.ID, null, null, null);
						}
						else
						{
							if (current.Savings > 10000000.0)
							{
								GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_10MILLION_SAVINGS, current.ID, null, null, null);
							}
						}
					}
				}
				if (current.RateTax != 0.05f)
				{
					int num = (int)Math.Round((double)Math.Abs((current.RateTax - 0.05f) * 100f));
					MoralEvent eventType = MoralEvent.ME_TAX_DECREASED;
					if (current.RateTax > 0.05f)
					{
						eventType = MoralEvent.ME_TAX_INCREASED;
						num *= 2;
					}
					GameSession.ApplyMoralEvent(this.App, num, eventType, current.ID, null, null, null);
				}
				List<EncounterInfo> list2 = this.GameDatabase.GetEncounterInfos().ToList<EncounterInfo>();
				List<AsteroidMonitorInfo> list3 = new List<AsteroidMonitorInfo>();
				List<MorrigiRelicInfo> list4 = new List<MorrigiRelicInfo>();
				foreach (EncounterInfo current2 in list2)
				{
					if (current2.Type == EasterEgg.EE_ASTEROID_MONITOR)
					{
						AsteroidMonitorInfo asteroidMonitorInfo = this.GameDatabase.GetAsteroidMonitorInfo(current2.Id);
						if (asteroidMonitorInfo != null)
						{
							list3.Add(asteroidMonitorInfo);
						}
					}
					if (current2.Type == EasterEgg.EE_MORRIGI_RELIC)
					{
						MorrigiRelicInfo morrigiRelicInfo = this.GameDatabase.GetMorrigiRelicInfo(current2.Id);
						if (morrigiRelicInfo != null)
						{
							list4.Add(morrigiRelicInfo);
						}
					}
				}
				List<ColonyInfo> list5 = this.GameDatabase.GetPlayerColoniesByPlayerId(current.ID).ToList<ColonyInfo>();
				foreach (ColonyInfo current3 in list5)
				{
					OrbitalObjectInfo ooi = this.GameDatabase.GetOrbitalObjectInfo(current3.OrbitalObjectID);
					List<FleetInfo> list6 = this.App.GameDatabase.GetFleetInfoBySystemID(ooi.StarSystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
					foreach (FleetInfo current4 in list6)
					{
						if (current4.PlayerID != current.ID && this.GameDatabase.GetDiplomacyStateBetweenPlayers(current4.PlayerID, current.ID) == DiplomacyState.WAR)
						{
							if (this.ScriptModules.AsteroidMonitor.PlayerID == current4.PlayerID || this.ScriptModules.MorrigiRelic.PlayerID == current4.PlayerID)
							{
								AsteroidMonitorInfo asteroidMonitorInfo2 = list3.FirstOrDefault((AsteroidMonitorInfo x) => x.SystemId == ooi.StarSystemID);
								MorrigiRelicInfo morrigiRelicInfo2 = list4.FirstOrDefault((MorrigiRelicInfo x) => x.SystemId == ooi.StarSystemID);
								if ((morrigiRelicInfo2 != null && morrigiRelicInfo2.IsAggressive) || (asteroidMonitorInfo2 != null && asteroidMonitorInfo2.IsAggressive))
								{
									GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ENEMY_IN_SYSTEM, current.ID, null, null, new int?(ooi.StarSystemID));
								}
							}
							else
							{
								GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ENEMY_IN_SYSTEM, current.ID, null, null, new int?(ooi.StarSystemID));
							}
						}
					}
					if (current3.Factions.Count<ColonyFactionInfo>() > 0)
					{
						if (current3.Factions.Sum((ColonyFactionInfo x) => x.CivilianPop) > 0.0)
						{
							if (current3.EconomyRating <= 0f)
							{
								GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ECONOMY_0, current.ID, new int?(current3.ID), null, null);
							}
							else
							{
								if (current3.EconomyRating <= 0.15f)
								{
									GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ECONOMY_BELOW_15, current.ID, new int?(current3.ID), null, null);
								}
								else
								{
									if (current3.EconomyRating >= 1f)
									{
										GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ECONOMY_100, current.ID, new int?(current3.ID), null, null);
									}
									else
									{
										if (current3.EconomyRating > 0.85f)
										{
											GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_ECONOMY_ABOVE_85, current.ID, new int?(current3.ID), null, null);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		private double UpdateGovernmentSpending(PlayerInfo pi, double revenue)
		{
			this.UpdateSavingsSpending(pi, revenue * (double)pi.RateGovernmentSavings);
			this.UpdateSecuritySpending(pi, revenue * (double)pi.RateGovernmentSecurity);
			this.UpdateStimulusSpending(pi, revenue * (double)pi.RateGovernmentStimulus);
			return revenue;
		}
		private void UpdateSavingsSpending(PlayerInfo pi, double revenue)
		{
			pi.Savings += revenue;
			this._db.UpdatePlayerSavings(pi.ID, pi.Savings);
		}
		private void UpdateSecuritySpending(PlayerInfo pi, double revenue)
		{
			double num = revenue * (double)pi.RateSecurityOperations;
			double num2 = revenue * (double)pi.RateSecurityIntelligence;
			double num3 = revenue * (double)pi.RateSecurityCounterIntelligence;
			List<StationInfo> list = this.App.GameDatabase.GetStationInfosByPlayerID(pi.ID).ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				if (current.DesignInfo.StationType == StationType.DIPLOMATIC && current.DesignInfo.StationLevel > 0)
				{
					List<DesignModuleInfo> list2 = (
						from x in current.DesignInfo.DesignSections[0].Modules
						where x.StationModuleType == ModuleEnums.StationModuleType.Sensor
						select x).ToList<DesignModuleInfo>();
					pi.IntelPoints += list2.Count;
				}
			}
			pi.IntelAccumulator += (int)num2;
			pi.CounterIntelAccumulator += (int)num3;
			pi.OperationsAccumulator += (int)num;
			int num4 = (int)Math.Floor((double)((float)pi.IntelAccumulator / (float)this.App.AssetDatabase.SecurityPointCost));
			int num5 = (int)Math.Floor((double)((float)pi.CounterIntelAccumulator / (float)this.App.AssetDatabase.SecurityPointCost));
			int num6 = (int)Math.Floor((double)((float)pi.OperationsAccumulator / (float)this.App.AssetDatabase.SecurityPointCost));
			pi.IntelPoints += num4;
			pi.CounterIntelPoints += num5;
			pi.OperationsPoints += num6;
			pi.IntelAccumulator -= num4 * this.App.AssetDatabase.SecurityPointCost;
			pi.CounterIntelAccumulator -= num5 * this.App.AssetDatabase.SecurityPointCost;
			pi.OperationsAccumulator -= num6 * this.App.AssetDatabase.SecurityPointCost;
			this.App.GameDatabase.UpdatePlayerIntelPoints(pi.ID, pi.IntelPoints);
			this.App.GameDatabase.UpdatePlayerCounterintelPoints(pi.ID, pi.CounterIntelPoints);
			this.App.GameDatabase.UpdatePlayerOperationsPoints(pi.ID, pi.OperationsPoints);
			this.App.GameDatabase.UpdatePlayerIntelAccumulator(pi.ID, pi.IntelAccumulator);
			this.App.GameDatabase.UpdatePlayerCounterintelAccumulator(pi.ID, pi.CounterIntelAccumulator);
			this.App.GameDatabase.UpdatePlayerOperationsAccumulator(pi.ID, pi.OperationsAccumulator);
		}
		private bool DoStimulusMiningRoll(float miningChance, PlayerInfo player)
		{
			if (!this._random.CoinToss((double)miningChance))
			{
				return false;
			}
			List<int> list = this.GameDatabase.GetPlayerColonySystemIDs(player.ID).ToList<int>();
			list.RemoveAll((int x) => !Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this, x, player.ID).Contains(StationType.MINING));
			if (list.Count == 0)
			{
				list = (
					from x in this.GameDatabase.GetStarSystemIDs()
					where this.GameDatabase.GetExploreRecord(x, player.ID) != null && this.GameDatabase.GetExploreRecord(x, player.ID).Explored
					select x).ToList<int>();
				list.RemoveAll((int x) => !Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this, x, player.ID).Contains(StationType.MINING));
			}
			list.RemoveAll((int x) => !this.GameDatabase.GetStarSystemInfo(x).IsOpen);
			if (list.Count == 0)
			{
				return false;
			}
			int num = this._random.Choose(list);
			List<OrbitalObjectInfo> list2 = this.GameDatabase.GetStarSystemOrbitalObjectInfos(num).ToList<OrbitalObjectInfo>();
			List<StationInfo> source = this.GameDatabase.GetStationForSystem(num).ToList<StationInfo>();
			int num2 = 0;
			foreach (OrbitalObjectInfo current in list2)
			{
				PlanetInfo pi = this.GameDatabase.GetPlanetInfo(current.ID);
				LargeAsteroidInfo li = this.GameDatabase.GetLargeAsteroidInfo(current.ID);
				if (pi != null && pi.Type == "barren" && !source.Any((StationInfo x) => this.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).ParentID == pi.ID && x.DesignInfo.StationType == StationType.MINING))
				{
					num2 = pi.ID;
					break;
				}
				if (li != null && !source.Any((StationInfo x) => this.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).ParentID == li.ID && x.DesignInfo.StationType == StationType.MINING))
				{
					num2 = li.ID;
					break;
				}
			}
			if (num2 == 0)
			{
				return false;
			}
			DesignInfo designInfo = DesignLab.CreateStationDesignInfo(this.AssetDatabase, this.GameDatabase, player.ID, StationType.MINING, 1, true);
			this.ConstructStation(designInfo, num2, true);
			Faction faction = this.App.AssetDatabase.GetFaction(player.FactionID);
			string localizedStationTypeName = this.App.AssetDatabase.GetLocalizedStationTypeName(designInfo.StationType, faction.HasSlaves());
			this.App.GameDatabase.InsertGovernmentAction(player.ID, string.Format(App.Localize("@GA_STATIONBUILT"), localizedStationTypeName), "StationBuilt_Mine_Stimulus", 0, 0);
			this.App.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_MINING_STIMULUS,
				EventMessage = TurnEventMessage.EM_MINING_STIMULUS,
				PlayerID = player.ID,
				SystemID = num,
				TurnNumber = this.App.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			return true;
		}
        private bool DoStimulusColonizationRoll(float colonizationChance, PlayerInfo player)
        {
            Func<int, bool> func = null;
            Func<ColonyInfo, bool> func2 = null;
            Predicate<PlanetInfo> match = null;
            Predicate<PlanetInfo> predicate2 = null;
            Func<PlanetInfo, float> keySelector = null;
            Func<ColonyFactionInfo, bool> predicate = null;
            Func<ColonyFactionInfo, bool> func5 = null;
            if (!this._random.CoinToss(((double)colonizationChance)))
            {
                return false;
            }
            if (func == null)
            {
                func = x => (this.GameDatabase.GetExploreRecord(x, player.ID) != null) && this.GameDatabase.GetExploreRecord(x, player.ID).Explored;
            }
            List<int> list = this.GameDatabase.GetStarSystemIDs().Where<int>(func).ToList<int>();
            List<PlanetInfo> source = new List<PlanetInfo>();
            foreach (int num in list)
            {
                if (this.GameDatabase.GetStarSystemInfo(num).IsOpen)
                {
                    if (func2 == null)
                    {
                        func2 = x => x.PlayerID != player.ID;
                    }
                    if (this.GameDatabase.GetColonyInfosForSystem(num).Where<ColonyInfo>(func2).Count<ColonyInfo>() == 0)
                    {
                        source.AddRange(this.GameDatabase.GetStarSystemPlanetInfos(num).ToList<PlanetInfo>());
                    }
                }
            }
            source.RemoveAll(delegate(PlanetInfo x)
            {
                if (!(x.Type == "gaseous"))
                {
                    return x.Type == "barren";
                }
                return true;
            });
            if (match == null)
            {
                match = x => this.GameDatabase.GetPlanetHazardRating(player.ID, x.ID, false) > ((float)this.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, player.ID));
            }
            source.RemoveAll(match);
            if (predicate2 == null)
            {
                predicate2 = x => this.GameDatabase.GetColonyInfoForPlanet(x.ID) != null;
            }
            source.RemoveAll(predicate2);
            if (source.Count == 0)
            {
                return false;
            }
            if (keySelector == null)
            {
                keySelector = x => this.GameDatabase.GetPlanetHazardRating(player.ID, x.ID, false);
            }
            source = source.OrderBy<PlanetInfo, float>(keySelector).ToList<PlanetInfo>();
            int num2 = this._random.Next(0, Math.Min(source.Count, 3));
            PlanetInfo info = source[num2];
            OrbitalObjectInfo ooi = this.GameDatabase.GetOrbitalObjectInfo(info.ID);
            List<int> list3 = new List<int>();
            foreach (ProvinceInfo info2 in this.GameDatabase.GetProvinceInfos().ToList<ProvinceInfo>())
            {
                if (info2.PlayerID == player.ID)
                {
                    list3.Add(info2.CapitalSystemID);
                }
            }
            HomeworldInfo playerHomeworld = this.GameDatabase.GetPlayerHomeworld(player.ID);
            if (playerHomeworld != null)
            {
                list3.Add(playerHomeworld.SystemID);
            }
            list3 = (from x in list3
                     orderby (this.GameDatabase.GetStarSystemOrigin(x) - this.GameDatabase.GetStarSystemOrigin(ooi.StarSystemID)).Length
                     select x).ToList<int>();
            string factionName = string.Format("{0} {1}", ooi.Name, App.Localize("@UI_PLAYER_NAME_CIVILIAN_COLONY"));
            Faction faction = this.AssetDatabase.GetFaction(player.FactionID);
            int playerID = this.GameDatabase.GetOrInsertIndyPlayerId(this.App.Game, player.FactionID, factionName, faction.SplinterAvatarPath());
            Vector3 vector = this.GameDatabase.GetStarSystemOrigin(list3.First<int>()) - this.GameDatabase.GetStarSystemOrigin(ooi.StarSystemID);
            this.GameDatabase.UpdateDiplomacyState(playerID, player.ID, DiplomacyState.ALLIED, 0x5dc - ((int)vector.Length), true);
            DiplomacyInfo diplomacyInfo = this.GameDatabase.GetDiplomacyInfo(player.ID, playerID);
            diplomacyInfo.isEncountered = true;
            this.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo);
            int colonyID = this.GameDatabase.InsertColony(info.ID, playerID, 500.0, 0.5f, this.GameDatabase.GetTurnCount(), info.Infrastructure, false);
            ColonyInfo colonyInfo = this.GameDatabase.GetColonyInfo(colonyID);
            if (predicate == null)
            {
                predicate = x => x.FactionID == player.FactionID;
            }
            if (colonyInfo.Factions.Any<ColonyFactionInfo>(predicate))
            {
                if (func5 == null)
                {
                    func5 = x => x.FactionID == player.FactionID;
                }
                ColonyFactionInfo civPop = colonyInfo.Factions.First<ColonyFactionInfo>(func5);
                civPop.CivilianPop += this.GameDatabase.AssetDatabase.StimulusColonizationBonus;
                this.GameDatabase.UpdateCivilianPopulation(civPop);
            }
            else
            {
                this.GameDatabase.InsertColonyFaction(info.ID, player.FactionID, (double)this.GameDatabase.AssetDatabase.StimulusColonizationBonus, 0f, this.GameDatabase.GetTurnCount());
            }
            foreach (PlayerInfo info7 in this.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>())
            {
                DiplomacyInfo info8 = this.GameDatabase.GetDiplomacyInfo(player.ID, info7.ID);
                if (this.GameDatabase.GetFactionName(info7.FactionID) == "morrigi")
                {
                    this.GameDatabase.UpdateDiplomacyState(playerID, info7.ID, info8.State, info8.Relations + 300, true);
                }
                else
                {
                    this.GameDatabase.UpdateDiplomacyState(playerID, info7.ID, info8.State, info8.Relations, true);
                }
            }
            this.App.GameDatabase.InsertGovernmentAction(player.ID, App.Localize("@GA_CIVCOLONYSTARTED"), "Civ_ColonyStarted", 0, 0);
            TurnEvent ev = new TurnEvent
            {
                EventType = TurnEventType.EV_COLONY_STIMULUS,
                EventMessage = TurnEventMessage.EM_COLONY_STIMULUS,
                PlayerID = player.ID,
                SystemID = ooi.StarSystemID,
                TurnNumber = this.App.GameDatabase.GetTurnCount(),
                ShowsDialog = false
            };
            this.App.GameDatabase.InsertTurnEvent(ev);
            this.GameDatabase.DuplicateStratModifiers(playerID, player.ID);
            TreatyInfo ti = new TreatyInfo
            {
                Active = false,
                Removed = false,
                Type = TreatyType.Trade,
                ReceivingPlayerId = player.ID,
                InitiatingPlayerId = playerID,
                StartingTurn = this._db.GetTurnCount(),
                Duration = 0
            };
            this._db.InsertTreaty(ti);
            colonyInfo.ShipConRate = 0f;
            colonyInfo.TerraRate = 0.75f;
            colonyInfo.InfraRate = 0.25f;
            colonyInfo.TradeRate = 0f;
            this.GameDatabase.UpdateColony(colonyInfo);
            return true;
        }
        private bool DoStimulusTradeRoll(float tradeChance, PlayerInfo player)
		{
			if (this._random.CoinToss((double)tradeChance))
			{
				List<StationInfo> list = (
					from x in this._db.GetStationInfosByPlayerID(player.ID)
					where x.DesignInfo.StationType == StationType.CIVILIAN
					select x).ToList<StationInfo>();
				List<StationInfo> list2 = list.ToList<StationInfo>();
				foreach (StationInfo current in list2)
				{
					OrbitalObjectInfo orbitalObjectInfo = this.GameDatabase.GetOrbitalObjectInfo(current.OrbitalObjectID);
					bool flag = orbitalObjectInfo != null;
					if (flag)
					{
						StarSystemInfo starSystemInfo = this.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
						flag = (starSystemInfo != null && starSystemInfo.IsOpen);
						if (flag)
						{
							int tradeDockCapacity = this.GetTradeDockCapacity(current);
							int num = this._db.GetFreighterInfosForSystem(starSystemInfo.ID).Count<FreighterInfo>();
							if (tradeDockCapacity <= num)
							{
								flag = false;
							}
						}
					}
					if (!flag)
					{
						list.Remove(current);
					}
				}
				StationInfo stationInfo = null;
				if (list.Count != 0)
				{
					stationInfo = this._random.Choose(list);
				}
				int? num2 = null;
				if (stationInfo != null)
				{
					num2 = new int?(this._db.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).StarSystemID);
				}
				if (num2.HasValue)
				{
					List<DesignInfo> list3 = (
						from x in this._db.GetVisibleDesignInfosForPlayerAndRole(player.ID, ShipRole.FREIGHTER, true)
						where x.isPrototyped
						select x).ToList<DesignInfo>();
					list3 = (
						from x in list3
						orderby x.DesignDate
						select x).ToList<DesignInfo>();
					if (list3.Count == 0)
					{
						return false;
					}
					this._db.InsertFreighter(num2.Value, player.ID, list3.Last<DesignInfo>().ID, false);
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_TRADE_STIMULUS,
						EventMessage = TurnEventMessage.EM_TRADE_STIMULUS,
						PlayerID = player.ID,
						SystemID = num2.Value,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					this._app.GameDatabase.InsertGovernmentAction(player.ID, App.Localize("@GA_BUILTFREIGHTER"), "BuiltFreighter_Trade", 0, 0);
				}
				return true;
			}
			return false;
		}
		private void UpdateStimulusSpending(PlayerInfo pi, double revenue)
		{
			double num = 300000.0;
			int num2 = (int)Math.Floor(revenue / num);
			if (num2 > 0)
			{
				List<ColonyInfo> list = this._db.GetPlayerColoniesByPlayerId(pi.ID).ToList<ColonyInfo>();
				foreach (ColonyInfo current in list)
				{
					current.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.Stimulus300K, num2);
					this._db.UpdateColony(current);
				}
			}
			double num3 = revenue * (double)pi.RateStimulusMining;
			double num4 = revenue * (double)pi.RateStimulusColonization;
			double num5 = revenue * (double)pi.RateStimulusTrade;
			pi.CivilianMiningAccumulator += (int)num3;
			pi.CivilianColonizationAccumulator += (int)num4;
			pi.CivilianTradeAccumulator += (int)num5;
			float num6 = ((float)pi.CivilianMiningAccumulator - (float)this.AssetDatabase.StimulusMiningMin) / (float)this.AssetDatabase.StimulusMiningMax;
			float num7 = ((float)pi.CivilianColonizationAccumulator - (float)this.AssetDatabase.StimulusColonizationMin) / (float)this.AssetDatabase.StimulusColonizationMax;
			float num8 = ((float)pi.CivilianTradeAccumulator - (float)this.AssetDatabase.StimulusTradeMin) / (float)this.AssetDatabase.StimulusTradeMax;
			if (num6 > 0f)
			{
				pi.CivilianMiningAccumulator = (this.DoStimulusMiningRoll(num6, pi) ? 0 : pi.CivilianMiningAccumulator);
			}
			if (num7 > 0f)
			{
				pi.CivilianColonizationAccumulator = (this.DoStimulusColonizationRoll(num7, pi) ? 0 : pi.CivilianColonizationAccumulator);
			}
			if (num8 > 0f)
			{
				pi.CivilianTradeAccumulator = (this.DoStimulusTradeRoll(num8, pi) ? 0 : pi.CivilianTradeAccumulator);
			}
			this.App.GameDatabase.UpdatePlayerCivilianMiningAccumulator(pi.ID, pi.CivilianMiningAccumulator);
			this.App.GameDatabase.UpdatePlayerCivilianColonizationAccumulator(pi.ID, pi.CivilianColonizationAccumulator);
			this.App.GameDatabase.UpdatePlayerCivilianTradeAccumulator(pi.ID, pi.CivilianTradeAccumulator);
		}
		private List<int> CheckForInterdiction(FleetInfo fleet, int systemID)
		{
			List<int> list = new List<int>();
			foreach (FleetInfo current in this._db.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL))
			{
				if (current.PlayerID != fleet.PlayerID && this._db.GetDiplomacyStateBetweenPlayers(fleet.PlayerID, current.PlayerID) == DiplomacyState.WAR)
				{
					MissionInfo missionByFleetID = this._db.GetMissionByFleetID(current.ID);
					if (missionByFleetID != null && missionByFleetID.Type == MissionType.INTERDICTION)
					{
						list.Add(current.ID);
					}
				}
			}
			return list;
		}
		private bool IsFleetIntercepted(FleetInfo fleet)
		{
			MissionInfo missionByFleetID = this._db.GetMissionByFleetID(fleet.ID);
			if (missionByFleetID != null)
			{
				return this._db.GetWaypointsByMissionID(missionByFleetID.ID).Any((WaypointInfo x) => x.Type == WaypointType.Intercepted);
			}
			return false;
		}
		private bool IsFleetInterdicted(FleetInfo fleet)
		{
			IEnumerable<FleetInfo> fleetInfoBySystemID = this._db.GetFleetInfoBySystemID(fleet.SystemID, FleetType.FL_NORMAL);
			foreach (FleetInfo current in fleetInfoBySystemID)
			{
				if (current.PlayerID != fleet.PlayerID)
				{
					MissionInfo missionByFleetID = this._db.GetMissionByFleetID(current.ID);
					if (missionByFleetID != null && missionByFleetID.Type == MissionType.INTERDICTION)
					{
						DiplomacyState diplomacyStateBetweenPlayers = this._db.GetDiplomacyStateBetweenPlayers(current.PlayerID, fleet.PlayerID);
						if (diplomacyStateBetweenPlayers == DiplomacyState.WAR)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		public List<int> GetAvailableFleetsForPlayer(int playerID, int systemID)
		{
			List<int> list = new List<int>();
			foreach (FleetInfo current in this._db.GetFleetInfosByPlayerID(playerID, FleetType.FL_NORMAL))
			{
				if (this._db.GetMissionByFleetID(current.ID) == null && current.SystemID == current.SupportingSystemID)
				{
					list.Add(current.ID);
				}
			}
			return list;
		}
		public static int NumPoliceInSystem(App game, int systemId)
		{
			int num = 0;
			List<FleetInfo> list = game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				if (current.IsReserveFleet)
				{
					List<ShipInfo> list2 = game.GameDatabase.GetShipInfoByFleetID(current.ID, false).ToList<ShipInfo>();
					foreach (ShipInfo current2 in list2)
					{
						if (current2.IsPoliceShip())
						{
							num++;
						}
					}
				}
			}
			return num;
		}
		public static int GetPropagandaBonusInSystem(App game, int systemId, int playerId)
		{
			int num = 0;
			List<int> list = new List<int>();
			List<FleetInfo> list2 = game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list2)
			{
				if (!current.IsReserveFleet && !list.Contains(current.PlayerID))
				{
					List<ShipInfo> list3 = game.GameDatabase.GetShipInfoByFleetID(current.ID, true).ToList<ShipInfo>();
					foreach (ShipInfo current2 in list3)
					{
						DesignSectionInfo[] designSections = current2.DesignInfo.DesignSections;
						for (int i = 0; i < designSections.Length; i++)
						{
							DesignSectionInfo designSectionInfo = designSections[i];
							if (designSectionInfo.ShipSectionAsset.SectionName.Contains("propaganda") && !list.Contains(current.PlayerID))
							{
								list.Add(current.PlayerID);
								break;
							}
						}
						if (list.Contains(current.PlayerID))
						{
							break;
						}
					}
				}
			}
			foreach (int current3 in list)
			{
				if (game.GameDatabase.GetDiplomacyStateBetweenPlayers(current3, playerId) == DiplomacyState.WAR)
				{
					num -= 2;
				}
				else
				{
					if (current3 == playerId)
					{
						num += 2;
					}
				}
			}
			return num;
		}
		public static StationType GetConstructionMissionStationType(GameDatabase db, MissionInfo mission)
		{
			if (mission != null && mission.Type == MissionType.CONSTRUCT_STN && mission.StationType.HasValue)
			{
				return (StationType)mission.StationType.Value;
			}
			return StationType.INVALID_TYPE;
		}
		public List<StationInfo> GetUpgradableStations(List<StationInfo> stations)
		{
			List<StationInfo> list = new List<StationInfo>();
			foreach (StationInfo current in stations)
			{
				if (this.StationIsUpgradable(current))
				{
					list.Add(current);
				}
			}
			return list;
		}
		public bool StationIsUpgradable(StationInfo station)
		{
			if (station.DesignInfo.StationLevel > 4 || station.DesignInfo.StationLevel == 0 || station.DesignInfo.StationType == StationType.MINING || station.DesignInfo.StationType == StationType.DEFENCE)
			{
				return false;
			}
			if (station.DesignInfo.StationType == StationType.GATE && station.DesignInfo.StationLevel == 2 && !this.GameDatabase.PlayerHasTech(station.PlayerID, "DRV_Far_Casting"))
			{
				return false;
			}
			ShipSectionAsset shipSectionAsset = this.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == station.DesignInfo.DesignSections[0].FilePath);
			shipSectionAsset.Modules.ToList<LogicalModuleMount>();
			List<DesignModuleInfo> arg_DF_0 = station.DesignInfo.DesignSections[0].Modules;
			Dictionary<ModuleEnums.StationModuleType, int> dictionary = new Dictionary<ModuleEnums.StationModuleType, int>();
			float stationUpgradeProgress = this.GetStationUpgradeProgress(station, out dictionary);
			return stationUpgradeProgress >= 1f && !this.StationIsUpgrading(station);
		}
		public bool StationIsUpgrading(StationInfo station)
		{
			IEnumerable<MissionInfo> missionInfos = this.GameDatabase.GetMissionInfos();
			foreach (MissionInfo current in missionInfos)
			{
				if (current.TargetOrbitalObjectID == station.OrbitalObjectID && current.Type == MissionType.UPGRADE_STN)
				{
					return true;
				}
			}
			return false;
		}
		public static int ApplyParamilitaryToMorale(App game, int value, int playerId, int systemId)
		{
			if (value < 0)
			{
				int paramilitaryTechMoralBonus = GameSession.GetParamilitaryTechMoralBonus(game, playerId, systemId);
				return Math.Min(value + paramilitaryTechMoralBonus, 0);
			}
			return value;
		}
		public static void ApplyMoralEventToSystem(App game, int value, int? playerId, int? colonyId, int? systemId, int? provinceId)
		{
			int num = GameSession.ApplyParamilitaryToMorale(game, value, playerId.Value, systemId.Value);
			List<ColonyInfo> list = game.GameDatabase.GetColonyInfosForSystem(systemId.Value).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				if (current.PlayerID == playerId)
				{
					List<ColonyFactionInfo> list2 = game.GameDatabase.GetCivilianPopulations(current.OrbitalObjectID).ToList<ColonyFactionInfo>();
					foreach (ColonyFactionInfo current2 in list2)
					{
						if (current2.CivilianPop > 0.0)
						{
							current2.Morale += num;
							game.GameDatabase.UpdateCivilianPopulation(current2);
						}
					}
				}
			}
		}
		public static void ApplyMoralEventToProvince(App game, int value, int? playerId, int? colonyId, int? systemId, int? provinceId)
		{
			if (!provinceId.HasValue)
			{
				return;
			}
			List<ColonyInfo> list = game.GameDatabase.GetColonyInfosForProvince(provinceId.Value).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				if (current.PlayerID == playerId)
				{
					int num = GameSession.ApplyParamilitaryToMorale(game, value, playerId.Value, current.CachedStarSystemID);
					List<ColonyFactionInfo> list2 = game.GameDatabase.GetCivilianPopulations(current.OrbitalObjectID).ToList<ColonyFactionInfo>();
					foreach (ColonyFactionInfo current2 in list2)
					{
						if (current2.CivilianPop > 0.0)
						{
							current2.Morale += num;
							game.GameDatabase.UpdateCivilianPopulation(current2);
						}
					}
				}
			}
		}
		public static void ApplyMoralEventToColony(App game, int value, int? playerId, int? colonyId, int? systemId, int? provinceId)
		{
			if (!colonyId.HasValue)
			{
				return;
			}
			ColonyInfo colonyInfo = game.GameDatabase.GetColonyInfo(colonyId.Value);
			if (colonyInfo == null)
			{
				return;
			}
			int num = GameSession.ApplyParamilitaryToMorale(game, value, playerId.Value, colonyInfo.CachedStarSystemID);
			List<ColonyFactionInfo> list = game.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID).ToList<ColonyFactionInfo>();
			foreach (ColonyFactionInfo current in list)
			{
				if (current.CivilianPop > 0.0)
				{
					current.Morale += num;
					game.GameDatabase.UpdateCivilianPopulation(current);
				}
			}
		}
		public static void ApplyMoralEventToAllColonies(App game, int value, int? playerId, int? colonyId, int? systemId, int? provinceId)
		{
			List<ColonyInfo> list = game.GameDatabase.GetColonyInfos().ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				if (current.PlayerID == playerId)
				{
					int num = GameSession.ApplyParamilitaryToMorale(game, value, playerId.Value, current.CachedStarSystemID);
					List<ColonyFactionInfo> list2 = game.GameDatabase.GetCivilianPopulations(current.OrbitalObjectID).ToList<ColonyFactionInfo>();
					foreach (ColonyFactionInfo current2 in list2)
					{
						if (current2.CivilianPop > 0.0)
						{
							current2.Morale += num;
							game.GameDatabase.UpdateCivilianPopulation(current2);
						}
					}
				}
			}
		}
		public static List<AssetDatabase.MoralModifier> GetMoraleEffects(App game, MoralEvent eventType, int player)
		{
			GovernmentInfo.GovernmentType currentType = game.GameDatabase.GetGovernmentInfo(player).CurrentType;
			Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>> dictionary;
			if (AssetDatabase.MoralModifierMap[currentType].ContainsKey(eventType))
			{
				dictionary = AssetDatabase.MoralModifierMap[currentType];
			}
			else
			{
				dictionary = AssetDatabase.MoralModifierMap[GovernmentInfo.GovernmentType.Centrism];
			}
			return dictionary[eventType];
		}
		public static void ApplyMoralEvent(App game, int multiplier, MoralEvent eventType, int player, int? colonyId = null, int? provinceId = null, int? systemId = null)
		{
			GovernmentInfo.GovernmentType currentType = game.GameDatabase.GetGovernmentInfo(player).CurrentType;
			Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>> dictionary;
			if (AssetDatabase.MoralModifierMap[currentType].ContainsKey(eventType))
			{
				dictionary = AssetDatabase.MoralModifierMap[currentType];
			}
			else
			{
				dictionary = AssetDatabase.MoralModifierMap[GovernmentInfo.GovernmentType.Centrism];
			}
			GameSession.ApplyMoraleEvent(dictionary[eventType], game, currentType, eventType, multiplier, player, colonyId, systemId, provinceId);
		}
		public static void ApplyMoralEvent(App game, MoralEvent eventType, int player, int? colonyId = null, int? provinceId = null, int? systemId = null)
		{
			GovernmentInfo.GovernmentType governmentType = game.GameDatabase.GetGovernmentInfo(player).CurrentType;
			if (!AssetDatabase.MoralModifierMap[governmentType].ContainsKey(eventType))
			{
				governmentType = GovernmentInfo.GovernmentType.Centrism;
			}
			Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>> dictionary = AssetDatabase.MoralModifierMap[governmentType];
			GameSession.ApplyMoraleEvent(dictionary[eventType], game, governmentType, eventType, 1, player, colonyId, systemId, provinceId);
		}
		public static void ApplyMoraleEvent(List<AssetDatabase.MoralModifier> modifiers, App game, GovernmentInfo.GovernmentType governmentType, MoralEvent eventType, int multiplier, int playerId, int? colonyId, int? systemId, int? provinceId)
		{
			Kerberos.Sots.PlayerFramework.Player playerObject = game.Game.GetPlayerObject(playerId);
			int stratModifier = game.GetStratModifier<int>(StratModifiers.MoralBonus, playerId);
			if (modifiers.Count == 1)
			{
				int num = playerObject.Faction.GetMoralValue(governmentType, eventType, modifiers[0].value) * multiplier + stratModifier;
				num = game.AssetDatabase.GovEffects.GetMoralTotal(game.GameDatabase, governmentType, eventType, playerId, num);
				switch (modifiers[0].type)
				{
				case AssetDatabase.MoraleModifierType.AllColonies:
					GameSession.ApplyMoralEventToAllColonies(game, num, new int?(playerId), colonyId, systemId, provinceId);
					break;
				case AssetDatabase.MoraleModifierType.Province:
					GameSession.ApplyMoralEventToProvince(game, num, new int?(playerId), colonyId, systemId, provinceId);
					break;
				case AssetDatabase.MoraleModifierType.System:
					GameSession.ApplyMoralEventToSystem(game, num, new int?(playerId), colonyId, systemId, provinceId);
					break;
				case AssetDatabase.MoraleModifierType.Colony:
					GameSession.ApplyMoralEventToColony(game, num, new int?(playerId), colonyId, systemId, provinceId);
					break;
				}
				game.GameDatabase.InsertMoralHistoryEvent(eventType, playerId, num, colonyId, systemId, provinceId);
				return;
			}
			List<ColonyInfo> list = game.GameDatabase.GetColonyInfos().ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				if (current.PlayerID == playerId)
				{
					int num2 = 0;
					bool flag = false;
					if (modifiers.Any((AssetDatabase.MoralModifier x) => x.type == AssetDatabase.MoraleModifierType.Colony) && colonyId.HasValue && current.ID == colonyId.Value)
					{
						flag = true;
						num2 = modifiers.First((AssetDatabase.MoralModifier x) => x.type == AssetDatabase.MoraleModifierType.Colony).value;
					}
					if (!flag)
					{
						if (modifiers.Any((AssetDatabase.MoralModifier x) => x.type == AssetDatabase.MoraleModifierType.System) && systemId.HasValue && current.CachedStarSystemID == systemId.Value)
						{
							flag = true;
							num2 = modifiers.First((AssetDatabase.MoralModifier x) => x.type == AssetDatabase.MoraleModifierType.System).value;
						}
					}
					if (!flag)
					{
						if (modifiers.Any((AssetDatabase.MoralModifier x) => x.type == AssetDatabase.MoraleModifierType.Province) && provinceId.HasValue)
						{
							StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(current.CachedStarSystemID);
							if (starSystemInfo != null && starSystemInfo.ProvinceID.HasValue && starSystemInfo.ProvinceID.Value == provinceId.Value)
							{
								flag = true;
								num2 = modifiers.First((AssetDatabase.MoralModifier x) => x.type == AssetDatabase.MoraleModifierType.Province).value;
							}
						}
					}
					if (!flag)
					{
						if (modifiers.Any((AssetDatabase.MoralModifier x) => x.type == AssetDatabase.MoraleModifierType.AllColonies))
						{
							flag = true;
							num2 = modifiers.First((AssetDatabase.MoralModifier x) => x.type == AssetDatabase.MoraleModifierType.AllColonies).value;
						}
					}
					if (flag)
					{
						num2 = playerObject.Faction.GetMoralValue(governmentType, eventType, num2) * multiplier + stratModifier;
						num2 = game.AssetDatabase.GovEffects.GetMoralTotal(game.GameDatabase, governmentType, eventType, playerId, num2);
						num2 = GameSession.ApplyParamilitaryToMorale(game, num2, playerId, current.CachedStarSystemID);
						List<ColonyFactionInfo> list2 = game.GameDatabase.GetCivilianPopulations(current.OrbitalObjectID).ToList<ColonyFactionInfo>();
						foreach (ColonyFactionInfo current2 in list2)
						{
							if (current2.CivilianPop > 0.0)
							{
								current2.Morale = Math.Min(100, Math.Max(current2.Morale + num2, 0));
								game.GameDatabase.UpdateCivilianPopulation(current2);
							}
						}
						game.GameDatabase.InsertMoralHistoryEvent(eventType, playerId, num2, new int?(current.ID), null, null);
					}
				}
			}
		}
		public static int GetParamilitaryTechMoralBonus(App game, int player, int system)
		{
			int num = 0;
			PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfos(player).FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "POL_Paramilitary_Training");
			if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
			{
				List<FleetInfo> list = game.GameDatabase.GetFleetsByPlayerAndSystem(player, system, FleetType.FL_DEFENSE).ToList<FleetInfo>();
				foreach (FleetInfo current in list)
				{
					List<ShipInfo> list2 = game.GameDatabase.GetShipInfoByFleetID(current.ID, false).ToList<ShipInfo>();
					foreach (ShipInfo current2 in list2)
					{
						if (current2.IsPoliceShip() && game.GameDatabase.GetShipSystemPosition(current2.ID).HasValue)
						{
							num++;
						}
					}
				}
			}
			return num;
		}
		public void DeclareWarInformally(int playerId, int targetPlayerId)
		{
			if (this.GameDatabase.GetPlayerInfo(playerId).IsOnTeam(this.GameDatabase.GetPlayerInfo(targetPlayerId)))
			{
				return;
			}
			this.GameDatabase.InsertDiplomacyActionHistoryEntry(playerId, targetPlayerId, this.GameDatabase.GetTurnCount(), DiplomacyAction.SURPRISEATTACK, null, null, null, null, null);
			this.GameDatabase.ChangeDiplomacyState(playerId, targetPlayerId, DiplomacyState.WAR);
			this.GameDatabase.InsertGovernmentAction(playerId, App.Localize("@GA_BETRAYAL"), "Betrayal", 0, 0);
			foreach (int current in this.GameDatabase.GetStandardPlayerIDs())
			{
				if (current != playerId && current != targetPlayerId)
				{
					this.GameDatabase.ApplyDiplomacyReaction(playerId, current, StratModifiers.DiplomacyReactionBetrayal, 1);
				}
			}
			GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_BETRAYAL, playerId, null, null, null);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_BETRAYAL,
				EventMessage = TurnEventMessage.EM_BETRAYAL,
				PlayerID = playerId,
				TargetPlayerID = targetPlayerId,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			foreach (int current2 in this.GameDatabase.GetStandardPlayerIDs())
			{
				if (current2 != playerId && current2 != targetPlayerId)
				{
					this.GameDatabase.ApplyDiplomacyReaction(targetPlayerId, current2, StratModifiers.DiplomacyReactionBetrayed, 1);
				}
			}
			int num = this.GameDatabase.GetPlayerColoniesByPlayerId(playerId).Count<ColonyInfo>();
			int num2 = this.GameDatabase.GetPlayerColoniesByPlayerId(targetPlayerId).Count<ColonyInfo>();
			if (num > num2)
			{
				GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_BETRAYED_LARGER, playerId, null, null, null);
			}
			else
			{
				GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_BETRAYED_SMALLER, playerId, null, null, null);
			}
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_BETRAYED,
				EventMessage = TurnEventMessage.EM_BETRAYED,
				PlayerID = targetPlayerId,
				TargetPlayerID = playerId,
				TurnNumber = this._app.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
		}
		public void DeclareWarFormally(int playerId, int targetPlayerId)
		{
			if (this.GameDatabase.GetPlayerInfo(playerId).IsOnTeam(this.GameDatabase.GetPlayerInfo(targetPlayerId)))
			{
				return;
			}
			this.GameDatabase.InsertDiplomacyActionHistoryEntry(playerId, targetPlayerId, this.GameDatabase.GetTurnCount(), DiplomacyAction.DECLARATION, null, null, null, null, null);
			this.GameDatabase.ChangeDiplomacyState(playerId, targetPlayerId, DiplomacyState.WAR);
			this.GameDatabase.InsertGovernmentAction(playerId, App.Localize("@GA_DECLAREWAR"), "DeclareWar", 0, 0);
			this.GameDatabase.ApplyDiplomacyReaction(playerId, targetPlayerId, StratModifiers.DiplomacyReactionDeclareWar, 1);
			GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_WAR_DECLARED, playerId, null, null, null);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				PlayerID = targetPlayerId,
				TargetPlayerID = playerId,
				EventMessage = TurnEventMessage.EM_WAR_DECLARED_DEFENDER,
				EventType = TurnEventType.EV_WAR_DECLARED,
				TurnNumber = this.GameDatabase.GetTurnCount()
			});
			this.GameDatabase.ApplyDiplomacyReaction(playerId, targetPlayerId, StratModifiers.DiplomacyReactionDeclareWar, 1);
			GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_WAR_DECLARED, playerId, null, null, null);
			this._app.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				PlayerID = playerId,
				TargetPlayerID = targetPlayerId,
				EventMessage = TurnEventMessage.EM_WAR_DECLARED_AGGRESSOR,
				EventType = TurnEventType.EV_WAR_DECLARED,
				TurnNumber = this.GameDatabase.GetTurnCount()
			});
		}
		public int GetNumAdmirals(int playerID)
		{
			return this._db.GetAdmiralInfosForPlayer(playerID).Count<AdmiralInfo>();
		}
		public static string GetMissionDesc(GameDatabase gamedb, MissionInfo mission)
		{
			string text = "";
			text += mission.Type.ToDisplayText();
			if (mission.Type == MissionType.CONSTRUCT_STN || mission.Type == MissionType.UPGRADE_STN)
			{
				StationInfo stationInfo = gamedb.GetStationInfo(mission.TargetOrbitalObjectID);
				if (stationInfo != null)
				{
					text += " ";
					text += stationInfo.DesignInfo.StationType.ToDisplayText(gamedb.GetFactionName(gamedb.GetPlayerFactionID(gamedb.GetFleetInfo(mission.FleetID).PlayerID)));
				}
			}
			if (mission.Type == MissionType.COLONIZATION || mission.Type == MissionType.SUPPORT)
			{
				OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(mission.TargetOrbitalObjectID);
				if (orbitalObjectInfo != null)
				{
					text += " - ";
					text += orbitalObjectInfo.Name;
				}
			}
			return text;
		}
		public static int GetAvailableAdmiral(GameDatabase gamedb, int playerID)
		{
			IEnumerable<AdmiralInfo> admiralInfosForPlayer = gamedb.GetAdmiralInfosForPlayer(playerID);
			foreach (AdmiralInfo current in admiralInfosForPlayer)
			{
				if (gamedb.GetFleetInfoByAdmiralID(current.ID, FleetType.FL_NORMAL) == null)
				{
					return current.ID;
				}
			}
			return 0;
		}
		public static int GenerateNewAdmiral(AssetDatabase assetdb, int playerID, GameDatabase _db, AdmiralInfo.TraitType? inheritedType, NamesPool namesPool)
		{
			Random safeRandom = App.GetSafeRandom();
			IEnumerable<ColonyInfo> playerColoniesByPlayerId = _db.GetPlayerColoniesByPlayerId(playerID);
			List<ColonyInfo> list = new List<ColonyInfo>();
			string factionName = _db.GetFactionName(_db.GetPlayerInfo(playerID).FactionID);
			List<string> racesForFaction = ScenarioEnumerations.GetRacesForFaction(factionName);
			string text = racesForFaction[safeRandom.Next(racesForFaction.Count)];
			Race race = assetdb.GetRace(text);
			float minValue = float.Parse(race.GetVariable("AdmiralMinStartAge"));
			float maxValue = float.Parse(race.GetVariable("AdmiralMaxStartAge"));
			float num = safeRandom.NextInclusive(minValue, maxValue);
			foreach (ColonyInfo current in playerColoniesByPlayerId)
			{
				if (current.TurnEstablished == 1 || (float)(_db.GetTurnCount() - current.TurnEstablished) > num)
				{
					list.Add(current);
				}
			}
			int? homeworldID = new int?(0);
			if (list.Any<ColonyInfo>())
			{
				int index = safeRandom.NextInclusive(0, list.Count<ColonyInfo>() - 1);
				homeworldID = new int?(list.ElementAt(index).OrbitalObjectID);
			}
			else
			{
				homeworldID = null;
			}
			string text2 = (safeRandom.NextInclusive(0, 1) == 0) ? "female" : "male";
			int num2 = safeRandom.NextInclusive(1, 25);
			int num3 = safeRandom.NextInclusive(1, 99);
			int loyalty = safeRandom.NextInclusive(1, 99);
			string admiralName = namesPool.GetAdmiralName(text, text2);
			int num4 = _db.InsertAdmiral(playerID, homeworldID, admiralName, text, num, text2, (float)num2, (float)num3, loyalty);
			if (inheritedType.HasValue)
			{
				_db.AddAdmiralTrait(num4, inheritedType.Value, 1);
			}
			AdmiralInfo.TraitType traitType;
			while (true)
			{
				if (text == "presterzuul" || text == "liir")
				{
					if (safeRandom.NextInclusive(1, 10) > 8)
					{
						break;
					}
				}
				else
				{
					if (text == "hordezuul" && safeRandom.NextInclusive(1, 10) > 8)
					{
						goto Block_8;
					}
				}
				traitType = safeRandom.Choose((IEnumerable<AdmiralInfo.TraitType>)Enum.GetValues(typeof(AdmiralInfo.TraitType)));
				if (AdmiralInfo.CanRaceHaveTrait(traitType, text) && (!inheritedType.HasValue || traitType != inheritedType.Value))
				{
					goto IL_215;
				}
			}
			traitType = AdmiralInfo.TraitType.Evangelist;
			goto IL_215;
			Block_8:
			traitType = AdmiralInfo.TraitType.Inquisitor;
			IL_215:
			_db.AddAdmiralTrait(num4, traitType, 1);
			return num4;
		}
		public static int? CalculateTurnsToCompleteResearch(int researchCostInPoints, int progressInPoints, int researchPointsPerTurn)
		{
			if (researchPointsPerTurn > 0)
			{
				return new int?((Math.Max(researchCostInPoints - progressInPoints, 0) + (researchPointsPerTurn - 1)) / researchPointsPerTurn);
			}
			return null;
		}
		private void CheckAdmiralRetirement()
		{
			foreach (int current in this._db.GetStandardPlayerIDs())
			{
				foreach (AdmiralInfo admiral in this._db.GetAdmiralInfosForPlayer(current))
				{
					admiral.Age += 0.25f;
					this._db.UpdateAdmiralInfo(admiral);
					Race race = this._app.AssetDatabase.GetRace(admiral.Race);
					float num = float.Parse(race.GetVariable("AdmiralRetirementAge")) * this._db.GetStratModifierFloatToApply(StratModifiers.AdmiralCareerModifier, current);
					float num2 = float.Parse(race.GetVariable("AdmiralDeathAge")) * this._db.GetStratModifierFloatToApply(StratModifiers.AdmiralCareerModifier, current);
					bool flag = false;
					bool flag2 = false;
					if (num2 > 0f && admiral.Age >= num2 && !admiral.Engram)
					{
						float num3 = 10f * (admiral.Age - num2);
						if (this._random.NextNormal(0.0, 100.0) < (double)((int)num3))
						{
							flag2 = true;
						}
					}
					if (!flag2 && admiral.Age >= num && !admiral.Engram)
					{
						bool flag3 = false;
						FleetInfo fleetInfoByAdmiralID = this._db.GetFleetInfoByAdmiralID(admiral.ID, FleetType.FL_NORMAL);
						if (fleetInfoByAdmiralID != null)
						{
							MissionInfo missionByFleetID = this._db.GetMissionByFleetID(fleetInfoByAdmiralID.ID);
							if (missionByFleetID != null)
							{
								flag3 = true;
							}
						}
						if (!flag3)
						{
							float num4 = 5f * (admiral.Age - num);
							num4 += (float)this._db.GetLevelForAdmiralTrait(admiral.ID, AdmiralInfo.TraitType.Conscript) * 20f;
							num4 -= (float)this._db.GetLevelForAdmiralTrait(admiral.ID, AdmiralInfo.TraitType.TrueBeliever) * 20f;
							num4 = Math.Max(0f, Math.Min(100f, num4));
							if (this._random.NextNormal(0.0, 100.0) < (double)((int)num4))
							{
								flag = true;
							}
						}
					}
					TurnEvent turnEvent = null;
					if (flag2)
					{
						turnEvent = new TurnEvent
						{
							EventType = TurnEventType.EV_ADMIRAL_DEAD,
							EventMessage = TurnEventMessage.EM_ADMIRAL_DEAD,
							AdmiralID = admiral.ID,
							PlayerID = admiral.PlayerID,
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						};
					}
					else
					{
						if (flag)
						{
							turnEvent = new TurnEvent
							{
								EventType = TurnEventType.EV_ADMIRAL_RETIRED,
								EventMessage = TurnEventMessage.EM_ADMIRAL_RETIRED,
								AdmiralID = admiral.ID,
								PlayerID = admiral.PlayerID,
								TurnNumber = this._app.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							};
						}
					}
					if (turnEvent != null)
					{
						this._app.GameDatabase.InsertTurnEvent(turnEvent);
					}
					if (flag2 || flag)
					{
						AdmiralInfo.TraitType? inheritedType = null;
						int num5;
						if (flag2)
						{
							num5 = int.Parse(race.GetVariable("AdmiralDeathInheritTraitChance"));
						}
						else
						{
							num5 = int.Parse(race.GetVariable("AdmiralInheritTraitChance"));
						}
						if (this._random.Next(100) < num5)
						{
							IEnumerable<AdmiralInfo.TraitType> admiralTraits = this._db.GetAdmiralTraits(admiral.ID);
							List<AdmiralInfo.TraitType> list = new List<AdmiralInfo.TraitType>();
							foreach (AdmiralInfo.TraitType current2 in admiralTraits)
							{
								if (AdmiralInfo.IsGoodTrait(current2))
								{
									list.Add(current2);
								}
							}
							if (list.Count > 0)
							{
								inheritedType = new AdmiralInfo.TraitType?(list[this._random.Next(list.Count)]);
							}
						}
						int num6 = GameSession.GenerateNewAdmiral(this.AssetDatabase, current, this.GameDatabase, inheritedType, this.NamesPool);
						FleetInfo fleetInfoByAdmiralID2 = this._db.GetFleetInfoByAdmiralID(admiral.ID, FleetType.FL_NORMAL);
						if (fleetInfoByAdmiralID2 != null)
						{
							fleetInfoByAdmiralID2.AdmiralID = num6;
							this._db.UpdateFleetInfo(fleetInfoByAdmiralID2);
						}
						List<AIFleetInfo> source = this._db.GetAIFleetInfos(admiral.PlayerID).ToList<AIFleetInfo>();
						AIFleetInfo aIFleetInfo = source.FirstOrDefault((AIFleetInfo x) => x.AdmiralID == admiral.ID);
						if (aIFleetInfo != null)
						{
							aIFleetInfo.AdmiralID = new int?(num6);
							this._db.UpdateAIFleetInfo(aIFleetInfo);
						}
						this._db.RemoveAdmiral(admiral.ID);
					}
				}
			}
		}
		public float GetFamilySpecificResearchModifier(int playerId, string techFamily, out int totalModulesCounted)
		{
			totalModulesCounted = 0;
			float num = 0f;
			List<StationInfo> list = this._db.GetStationInfosByPlayerID(playerId).ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(current, this._db, false);
				StationType stationType = current.DesignInfo.StationType;
				if (stationType != StationType.SCIENCE)
				{
					if (stationType == StationType.GATE)
					{
						if (dictionary.ContainsKey(ModuleEnums.StationModuleType.GateLab) && techFamily == "DRV")
						{
							num += (float)dictionary[ModuleEnums.StationModuleType.GateLab] * 0.03f;
							totalModulesCounted += dictionary[ModuleEnums.StationModuleType.GateLab];
						}
					}
				}
				else
				{
					if (dictionary.Keys.Any((ModuleEnums.StationModuleType x) => x.ToString().Substring(0, 3) == techFamily))
					{
						num += (float)dictionary.First((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Key.ToString().Substring(0, 3) == techFamily).Value * 0.03f;
						totalModulesCounted += dictionary.First((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Key.ToString().Substring(0, 3) == techFamily).Value;
					}
				}
			}
			if (techFamily == "PSI")
			{
				num += this._db.GetStratModifierFloatToApply(StratModifiers.PsiResearchModifier, playerId) - 1f;
			}
			else
			{
				if (techFamily == "CCC")
				{
					num += this._db.GetStratModifierFloatToApply(StratModifiers.C3ResearchModifier, playerId) - 1f;
				}
			}
			return num;
		}
		public float GetGeneralResearchModifier(int playerId, bool forDisplay)
		{
			float num = 1f;
			List<StationInfo> list = this._db.GetStationInfosByPlayerID(playerId).ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(current, this._db, true);
				StationType stationType = current.DesignInfo.StationType;
				if (stationType == StationType.SCIENCE)
				{
					if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Habitation))
					{
						num += (float)dictionary[ModuleEnums.StationModuleType.Habitation] * 0.01f;
					}
					if (dictionary.ContainsKey(ModuleEnums.StationModuleType.AlienHabitation))
					{
						num += (float)dictionary[ModuleEnums.StationModuleType.AlienHabitation] * 0.02f;
					}
				}
			}
			num += this._db.GetStratModifierFloatToApply(StratModifiers.AIResearchBonus, playerId) * this.App.GetStratModifier<float>(StratModifiers.AIBenefitBonus, playerId);
			num += this._db.GetStratModifierFloatToApply(StratModifiers.ResearchModifier, playerId) - 1f;
			num += this._app.AssetDatabase.GetAIModifier(this._app, DifficultyModifiers.ResearchBonus, playerId);
			if (!forDisplay)
			{
				num += this._db.GetStratModifierFloatToApply(StratModifiers.GlobalResearchModifier, playerId) - 1f;
			}
			return num;
		}
		private float GetStationTradeModifierForSystem(int playerId, int systemId, bool isLocalTrade)
		{
			float num = 1f;
			List<StationInfo> list = this._db.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(current, this._db, true);
				switch (current.DesignInfo.StationType)
				{
				case StationType.CIVILIAN:
					if (isLocalTrade && dictionary.ContainsKey(ModuleEnums.StationModuleType.Habitation))
					{
						num += (float)dictionary[ModuleEnums.StationModuleType.Habitation] * 0.01f;
					}
					if (!isLocalTrade && dictionary.ContainsKey(ModuleEnums.StationModuleType.AlienHabitation))
					{
						num += (float)dictionary[ModuleEnums.StationModuleType.AlienHabitation] * 0.02f;
					}
					if (isLocalTrade && dictionary.ContainsKey(ModuleEnums.StationModuleType.LargeHabitation))
					{
						num += (float)dictionary[ModuleEnums.StationModuleType.LargeHabitation] * 0.05f;
					}
					if (!isLocalTrade && dictionary.ContainsKey(ModuleEnums.StationModuleType.LargeAlienHabitation))
					{
						num += (float)dictionary[ModuleEnums.StationModuleType.LargeAlienHabitation] * 0.1f;
					}
					break;
				case StationType.DIPLOMATIC:
					if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Customs))
					{
						num += (float)dictionary[ModuleEnums.StationModuleType.Customs] * 0.01f;
					}
					break;
				}
			}
			return num;
		}
		private int GetStationTradeRouteCapabilityBonus(int playerId, int systemId)
		{
			int num = 0;
			List<StationInfo> list = this._db.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(current, this._db, true);
				StationType stationType = current.DesignInfo.StationType;
				if (stationType == StationType.CIVILIAN && dictionary.ContainsKey(ModuleEnums.StationModuleType.Dock))
				{
					num += dictionary[ModuleEnums.StationModuleType.Dock];
				}
			}
			return num;
		}
		public int GetExportCapacity(int SystemId)
		{
			int num = 0;
			List<ColonyInfo> list = this._db.GetColonyInfosForSystem(SystemId).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				num += (int)(this._db.GetTotalPopulation(current) * (double)current.TradeRate / (double)this.AssetDatabase.PopulationPerTradePoint);
			}
			return num;
		}
		public List<double> GetTradeRatesForWholeExports(int SystemId)
		{
			List<double> list = new List<double>();
			int maxExportCapacity = this.GetMaxExportCapacity(SystemId);
			if (maxExportCapacity > 0)
			{
				double num = (double)(100f / (float)maxExportCapacity);
				int num2 = (int)(100.0 / num);
				for (int i = 0; i <= num2; i++)
				{
					double item = num * (double)i / 100.0;
					list.Add(item);
				}
			}
			return list;
		}
		public double GetColonyExportCapacity(int ColonyID)
		{
			double num = 0.0;
			ColonyInfo colonyInfo = this._db.GetColonyInfo(ColonyID);
			if (colonyInfo != null)
			{
				num += this._db.GetTotalPopulation(colonyInfo) * 1.0 / (double)this.AssetDatabase.PopulationPerTradePoint;
			}
			return num;
		}
		public List<double> GetTradeRatesForWholeExportsForColony(int ColonyId)
		{
			List<double> list = new List<double>();
			double colonyExportCapacity = this.GetColonyExportCapacity(ColonyId);
			if (colonyExportCapacity > 0.0)
			{
				double num = 100.0 / colonyExportCapacity;
				int num2 = (int)(100.0 / num);
				for (int i = 0; i <= num2; i++)
				{
					double item = num * (double)i / 100.0;
					list.Add(item);
				}
			}
			return list;
		}
		public int GetMaxExportCapacity(int SystemId)
		{
			int num = 0;
			List<ColonyInfo> list = this._db.GetColonyInfosForSystem(SystemId).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				num += (int)(this._db.GetTotalPopulation(current) * 1.0 / (double)this.AssetDatabase.PopulationPerTradePoint);
			}
			return num;
		}
		private Dictionary<int, int> GetImportCapacity(int SystemId)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			List<ColonyInfo> list = this._db.GetColonyInfosForSystem(SystemId).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				if (!dictionary.ContainsKey(current.PlayerID))
				{
					dictionary.Add(current.PlayerID, 0);
				}
				double totalPopulation = this._db.GetTotalPopulation(current);
				Dictionary<int, int> dictionary2;
				int playerID;
				(dictionary2 = dictionary)[playerID = current.PlayerID] = dictionary2[playerID] + (int)Math.Ceiling(totalPopulation / (double)this.AssetDatabase.PopulationPerTradePoint);
			}
			return dictionary;
		}
		private bool IsStationInterdicted(StationInfo info)
		{
			IEnumerable<MissionInfo> missionsByPlanetDest = this._db.GetMissionsByPlanetDest(info.OrbitalObjectID);
			foreach (MissionInfo current in missionsByPlanetDest)
			{
				if (current.Type == MissionType.INTERDICTION)
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(current.FleetID);
					DiplomacyState diplomacyStateBetweenPlayers = this._db.GetDiplomacyStateBetweenPlayers(info.PlayerID, fleetInfo.PlayerID);
					if (diplomacyStateBetweenPlayers == DiplomacyState.WAR)
					{
						return true;
					}
				}
			}
			return false;
		}
		private int GetTradeDockCapacity(StationInfo si)
		{
			int num = 1;
			Dictionary<ModuleEnums.StationModuleType, int> dictionary = GameSession.CountStationModuleTypes(si, this._db, true);
			if (dictionary.ContainsKey(ModuleEnums.StationModuleType.Dock))
			{
				num += dictionary[ModuleEnums.StationModuleType.Dock];
			}
			return num;
		}
		private int GetFreighterCapacity(int systemId)
		{
			int num = 0;
			List<FreighterInfo> list = this._db.GetFreighterInfosForSystem(systemId).ToList<FreighterInfo>();
			foreach (FreighterInfo current in list)
			{
				DesignSectionInfo[] designSections = current.Design.DesignSections;
				DesignSectionInfo dsi;
				for (int i = 0; i < designSections.Length; i++)
				{
					dsi = designSections[i];
					ShipSectionAsset shipSectionAsset = this.AssetDatabase.ShipSections.FirstOrDefault((ShipSectionAsset x) => x.FileName == dsi.FilePath);
					num += shipSectionAsset.FreighterSpace;
				}
			}
			return num;
		}
		private int GetDockExportCapacity(int numDocks, int systemId)
		{
			List<FreighterInfo> list = this.App.GameDatabase.GetFreighterInfosForSystem(systemId).ToList<FreighterInfo>();
			Dictionary<FreighterInfo, int> dictionary = new Dictionary<FreighterInfo, int>();
			foreach (FreighterInfo current in list)
			{
				dictionary.Add(current, current.Design.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.FreighterSpace));
			}
			dictionary = (
				from x in dictionary
				orderby x.Value descending
				select x).ToDictionary((KeyValuePair<FreighterInfo, int> y) => y.Key, (KeyValuePair<FreighterInfo, int> y) => y.Value);
			int num = 0;
			int num2 = 0;
			while (num2 < numDocks && dictionary.Count != 0)
			{
				num += dictionary.First<KeyValuePair<FreighterInfo, int>>().Value;
				dictionary.Remove(dictionary.First<KeyValuePair<FreighterInfo, int>>().Key);
				num2++;
			}
			return num;
		}
		private Dictionary<int, double> Trade(out TradeResultsTable tradeResult)
		{
			List<TreatyInfo> source = this._db.GetTreatyInfos().ToList<TreatyInfo>();
			int turn = this._db.GetTurnCount();
			source = (
				from x in source
				where x.Type == TreatyType.Trade && x.StartingTurn < turn && x.StartingTurn + x.Duration > turn && x.Active
				select x).ToList<TreatyInfo>();
			tradeResult = new TradeResultsTable();
			Dictionary<int, double> dictionary = new Dictionary<int, double>();
			List<StationInfo> source2 = this._db.GetStationInfos().ToList<StationInfo>();
			List<StationInfo> list = (
				from x in source2
				where x.DesignInfo.StationType == StationType.CIVILIAN && x.DesignInfo.StationLevel > 0
				select x).ToList<StationInfo>();
			if (list.Count == 0)
			{
				return dictionary;
			}
			List<GameSession.ExportNode> list2 = new List<GameSession.ExportNode>();
			foreach (StationInfo current in list)
			{
				if (!this.IsStationInterdicted(current))
				{
					int tradeDockCapacity = this.GetTradeDockCapacity(current);
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(current.OrbitalObjectID);
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
					int num = this.GetExportCapacity(starSystemInfo.ID) + current.WarehousedGoods;
					float range = this._db.FindCurrentDriveSpeedForPlayer(current.PlayerID) * this.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TradeRangeModifier, current.PlayerID);
					int freighterCapacity = this.GetFreighterCapacity(starSystemInfo.ID);
					int dockExportCapacity = this.GetDockExportCapacity(tradeDockCapacity, starSystemInfo.ID);
					list2.Add(new GameSession.ExportNode
					{
						System = starSystemInfo,
						Station = current,
						ExportPoints = Math.Min(num, dockExportCapacity),
						Range = range
					});
					if (!tradeResult.TradeNodes.ContainsKey(starSystemInfo.ID))
					{
						tradeResult.TradeNodes.Add(starSystemInfo.ID, new TradeNode
						{
							Produced = num,
							ProductionCapacity = this.GetMaxExportCapacity(starSystemInfo.ID),
							Freighters = freighterCapacity,
							DockCapacity = tradeDockCapacity,
							DockExportCapacity = dockExportCapacity,
							Range = range
						});
					}
					else
					{
						tradeResult.TradeNodes[starSystemInfo.ID].Produced = num;
						tradeResult.TradeNodes[starSystemInfo.ID].ProductionCapacity = this.GetMaxExportCapacity(starSystemInfo.ID);
						tradeResult.TradeNodes[starSystemInfo.ID].Freighters = freighterCapacity;
						tradeResult.TradeNodes[starSystemInfo.ID].DockCapacity = tradeDockCapacity;
						tradeResult.TradeNodes[starSystemInfo.ID].DockExportCapacity = dockExportCapacity;
						tradeResult.TradeNodes[starSystemInfo.ID].Range = range;
					}
				}
			}
			List<StarSystemInfo> list3 = this._db.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<GameSession.ImportNode> list4 = new List<GameSession.ImportNode>();
			foreach (StarSystemInfo current2 in list3)
			{
				Dictionary<int, int> importCapacity = this.GetImportCapacity(current2.ID);
				list4.Add(new GameSession.ImportNode
				{
					System = current2,
					ImportCapacity = importCapacity
				});
				if (!tradeResult.TradeNodes.ContainsKey(current2.ID))
				{
					Dictionary<int, TradeNode> arg_385_0 = tradeResult.TradeNodes;
					int arg_385_1 = current2.ID;
					TradeNode tradeNode = new TradeNode();
					tradeNode.Consumption = importCapacity.Sum((KeyValuePair<int, int> x) => x.Value);
					arg_385_0.Add(arg_385_1, tradeNode);
				}
				else
				{
					tradeResult.TradeNodes[current2.ID].Consumption = importCapacity.Sum((KeyValuePair<int, int> x) => x.Value);
				}
			}
			foreach (GameSession.ExportNode en in list2)
			{
				if (this.AssetDatabase.GetFaction(this._db.GetPlayerFactionID(en.Station.PlayerID)).CanUseGate())
				{
					en.GenericSystems = (
						from x in list4
						where x.System != en.System && this._db.SystemHasGate(x.System.ID, en.Station.PlayerID)
						select x).ToList<GameSession.ImportNode>();
				}
				en.GenericSystems.AddRange((
					from x in list4
					where x.System != en.System && (x.System.Origin - en.System.Origin).Length < en.Range
					select x).ToList<GameSession.ImportNode>());
				en.InternationalSystems = (
					from x in en.GenericSystems
					where x.ImportCapacity.Keys.Any((int y) => y != en.Station.PlayerID)
					select x).ToList<GameSession.ImportNode>();
				GameSession.ExportNode arg_534_0 = en;
				List<GameSession.ImportNode> arg_534_1;
				if (!en.System.ProvinceID.HasValue)
				{
					arg_534_1 = new List<GameSession.ImportNode>();
				}
				else
				{
					arg_534_1 = (
						from x in en.GenericSystems
						where x.System.ProvinceID != en.System.ProvinceID
						select x).ToList<GameSession.ImportNode>();
				}
				arg_534_0.InterprovincialSystems = arg_534_1;
				en.GenericSystems.RemoveAll((GameSession.ImportNode x) => en.InternationalSystems.Contains(x) || en.InterprovincialSystems.Contains(x));
			}
			for (int i = 0; i < list2.Count; i++)
			{
				int index = this._random.Next(list2.Count);
				GameSession.ExportNode value = list2[index];
				list2[index] = list2[i];
				list2[i] = value;
			}
			foreach (GameSession.ExportNode en in list2)
			{
				while (en.ExportPoints > 0 && en.InternationalSystems.Count > 0)
				{
					GameSession.ImportNode importNode = en.InternationalSystems.First<GameSession.ImportNode>();
					foreach (KeyValuePair<int, int> kvp in importNode.ImportCapacity)
					{
						KeyValuePair<int, int> kvp4 = kvp;
						if (kvp4.Key != en.Station.PlayerID)
						{
							if (source.Any(delegate(TreatyInfo x)
							{
								int arg_14_0 = x.InitiatingPlayerId;
								KeyValuePair<int, int> kvp2 = kvp;
								if (arg_14_0 != kvp2.Key || x.ReceivingPlayerId != en.Station.PlayerID)
								{
									int arg_47_0 = x.ReceivingPlayerId;
									KeyValuePair<int, int> kvp3 = kvp;
									return arg_47_0 == kvp3.Key && x.InitiatingPlayerId == en.Station.PlayerID;
								}
								return true;
							}))
							{
								Dictionary<int, int> arg_6A3_0 = importNode.ImportCapacity;
								kvp4 = kvp;
								int num2 = Math.Min(arg_6A3_0[kvp4.Key], en.ExportPoints);
								en.ExportPoints -= num2;
								if (tradeResult.TradeNodes.ContainsKey(en.System.ID))
								{
									tradeResult.TradeNodes[en.System.ID].ExportInt += num2;
								}
								else
								{
									tradeResult.TradeNodes.Add(en.System.ID, new TradeNode
									{
										ExportInt = num2
									});
								}
								if (tradeResult.TradeNodes.ContainsKey(importNode.System.ID))
								{
									tradeResult.TradeNodes[importNode.System.ID].ImportInt += num2;
								}
								else
								{
									tradeResult.TradeNodes.Add(importNode.System.ID, new TradeNode
									{
										ImportInt = num2
									});
								}
								float num3 = (float)num2 * this.AssetDatabase.IncomePerInternationalTradePointMoved;
								if (importNode.System.IsOpen)
								{
									num3 *= 1.1f;
								}
								float num4 = num3 * 0.7f;
								float num5 = num3 * 0.3f;
								num4 *= this.GetStationTradeModifierForSystem(en.Station.PlayerID, en.System.ID, false);
								float arg_847_0 = num5;
								kvp4 = kvp;
								num5 = arg_847_0 * this.GetStationTradeModifierForSystem(kvp4.Key, importNode.System.ID, false);
								this.GameDatabase.UpdatePlayerViewWithStarSystem(en.Station.PlayerID, importNode.System.ID);
								GameDatabase arg_894_0 = this.GameDatabase;
								kvp4 = kvp;
								arg_894_0.UpdatePlayerViewWithStarSystem(kvp4.Key, importNode.System.ID);
								if (!dictionary.ContainsKey(en.Station.PlayerID))
								{
									dictionary.Add(en.Station.PlayerID, (double)num4);
								}
								else
								{
									Dictionary<int, double> dictionary2;
									int key;
									(dictionary2 = dictionary)[key = en.Station.PlayerID] = dictionary2[key] + (double)num4;
								}
								Dictionary<int, double> arg_909_0 = dictionary;
								kvp4 = kvp;
								if (!arg_909_0.ContainsKey(kvp4.Key))
								{
									Dictionary<int, double> arg_924_0 = dictionary;
									kvp4 = kvp;
									arg_924_0.Add(kvp4.Key, (double)num5);
								}
								else
								{
									Dictionary<int, double> dictionary2;
									Dictionary<int, double> expr_92C = dictionary2 = dictionary;
									kvp4 = kvp;
									int key;
									expr_92C[key = kvp4.Key] = dictionary2[key] + (double)num5;
								}
							}
						}
					}
					en.InternationalSystems.Remove(importNode);
				}
			}
			list2.RemoveAll((GameSession.ExportNode x) => x.ExportPoints == 0);
			foreach (GameSession.ExportNode current3 in list2)
			{
				while (current3.ExportPoints > 0 && current3.InterprovincialSystems.Count > 0)
				{
					GameSession.ImportNode importNode2 = current3.InterprovincialSystems.First<GameSession.ImportNode>();
					int num6 = 0;
					if (importNode2.ImportCapacity.ContainsKey(current3.Station.PlayerID))
					{
						num6 = Math.Min(importNode2.ImportCapacity[current3.Station.PlayerID], current3.ExportPoints);
					}
					current3.ExportPoints -= num6;
					if (tradeResult.TradeNodes.ContainsKey(current3.System.ID))
					{
						tradeResult.TradeNodes[current3.System.ID].ExportProv += num6;
					}
					else
					{
						tradeResult.TradeNodes.Add(current3.System.ID, new TradeNode
						{
							ExportProv = num6
						});
					}
					if (tradeResult.TradeNodes.ContainsKey(importNode2.System.ID))
					{
						tradeResult.TradeNodes[importNode2.System.ID].ImportProv += num6;
					}
					else
					{
						tradeResult.TradeNodes.Add(importNode2.System.ID, new TradeNode
						{
							ImportProv = num6
						});
					}
					float num7 = (float)num6 * this.AssetDatabase.IncomePerProvincialTradePointMoved;
					num7 *= this.GetStationTradeModifierForSystem(current3.Station.PlayerID, current3.System.ID, true);
					if (importNode2.System.IsOpen)
					{
						num7 *= 1.1f;
					}
					if (!dictionary.ContainsKey(current3.Station.PlayerID))
					{
						dictionary.Add(current3.Station.PlayerID, (double)num7);
					}
					else
					{
						Dictionary<int, double> dictionary2;
						int key;
						(dictionary2 = dictionary)[key = current3.Station.PlayerID] = dictionary2[key] + (double)num7;
					}
					current3.InterprovincialSystems.Remove(importNode2);
				}
			}
			list2.RemoveAll((GameSession.ExportNode x) => x.ExportPoints == 0);
			foreach (GameSession.ExportNode current4 in list2)
			{
				while (current4.ExportPoints > 0 && current4.GenericSystems.Count > 0)
				{
					GameSession.ImportNode importNode3 = current4.GenericSystems.First<GameSession.ImportNode>();
					int num8 = 0;
					if (importNode3.ImportCapacity.ContainsKey(current4.Station.PlayerID))
					{
						num8 = Math.Min(importNode3.ImportCapacity[current4.Station.PlayerID], current4.ExportPoints);
					}
					current4.ExportPoints -= num8;
					if (tradeResult.TradeNodes.ContainsKey(current4.System.ID))
					{
						tradeResult.TradeNodes[current4.System.ID].ExportLoc += num8;
					}
					else
					{
						tradeResult.TradeNodes.Add(current4.System.ID, new TradeNode
						{
							ExportLoc = num8
						});
					}
					if (tradeResult.TradeNodes.ContainsKey(importNode3.System.ID))
					{
						tradeResult.TradeNodes[importNode3.System.ID].ImportLoc += num8;
					}
					else
					{
						tradeResult.TradeNodes.Add(importNode3.System.ID, new TradeNode
						{
							ImportLoc = num8
						});
					}
					float num9 = (float)num8 * this.AssetDatabase.IncomePerGenericTradePointMoved;
					num9 *= this.GetStationTradeModifierForSystem(current4.Station.PlayerID, current4.System.ID, true);
					if (importNode3.System.IsOpen)
					{
						num9 *= 1.1f;
					}
					if (!dictionary.ContainsKey(current4.Station.PlayerID))
					{
						dictionary.Add(current4.Station.PlayerID, (double)num9);
					}
					else
					{
						Dictionary<int, double> dictionary2;
						int key;
						(dictionary2 = dictionary)[key = current4.Station.PlayerID] = dictionary2[key] + (double)num9;
					}
					current4.GenericSystems.Remove(importNode3);
				}
			}
			list2.RemoveAll((GameSession.ExportNode x) => x.ExportPoints == 0);
			foreach (GameSession.ExportNode current5 in list2)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary3 = GameSession.CountStationModuleTypes(current5.Station, this._db, true);
				if (dictionary3.ContainsKey(ModuleEnums.StationModuleType.Warehouse))
				{
					current5.Station.WarehousedGoods = Math.Min((int)((float)dictionary3[ModuleEnums.StationModuleType.Warehouse] * this._db.GetStratModifierFloatToApply(StratModifiers.WarehouseCapacityModifier, current5.Station.PlayerID)), current5.ExportPoints);
				}
				current5.Station.WarehousedGoods = 0;
				current5.ExportPoints -= current5.Station.WarehousedGoods;
				foreach (ColonyInfo current6 in this._db.GetColonyInfosForSystem(current5.System.ID).ToList<ColonyInfo>())
				{
					current6.ModifyEconomyRating(this._db, ColonyInfo.EconomicChangeReason.SpoiledGoods, current5.ExportPoints);
					this._db.UpdateColony(current6);
				}
			}
			return dictionary;
		}
		public bool DetectedIncomingRandom(int playerid, int systemid)
		{
			StationInfo stationForSystemPlayerAndType = this.GameDatabase.GetStationForSystemPlayerAndType(systemid, playerid, StationType.SCIENCE);
			if (stationForSystemPlayerAndType == null)
			{
				return false;
			}
			int num = 0;
			DesignSectionInfo[] designSections = stationForSystemPlayerAndType.DesignInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				num += (
					from x in designSectionInfo.Modules
					where x.StationModuleType.HasValue && x.StationModuleType == ModuleEnums.StationModuleType.Sensor
					select x).Count<DesignModuleInfo>();
			}
			int num2 = num * 10;
			Random safeRandom = App.GetSafeRandom();
			int num3 = safeRandom.Next(0, 100);
			return num3 < num2;
		}
		public List<PendingCombat> GetPendingCombats()
		{
			return this.m_Combats;
		}
		public void OrderCombatsByResponse()
		{
			if (!this.App.GameSetup.IsMultiplayer)
			{
				this.m_Combats = (
					from x in this.m_Combats
					orderby !x.PlayersInCombat.Contains(this.App.LocalPlayer.ID) || this.m_Combats.Any((PendingCombat y) => y.SystemID == x.SystemID && y.PlayersInCombat.Contains(this.App.LocalPlayer.ID) && y.CombatResolutionSelections[this.App.LocalPlayer.ID] == ResolutionType.AUTO_RESOLVE) || x.CombatResolutionSelections[this.App.LocalPlayer.ID] == ResolutionType.AUTO_RESOLVE
					select x).ToList<PendingCombat>();
			}
		}
		public PendingCombat GetPendingCombatBySystemID(int systemId)
		{
			return this.m_Combats.First((PendingCombat x) => x.SystemID == systemId);
		}
		public PendingCombat GetPendingCombatByUniqueID(int Id)
		{
			return this.m_Combats.First((PendingCombat x) => x.ConflictID == Id);
		}
		public PendingCombat GetCurrentCombat()
		{
			return this._currentCombat;
		}
		public List<ReactionInfo> GetPendingReactions()
		{
			return this._reactions;
		}
		public void SetPendingReactions(List<ReactionInfo> reactions)
		{
			this._reactions = reactions;
		}
		public ReactionInfo GetNextReactionForPlayer(int playerID)
		{
			foreach (ReactionInfo current in this._reactions)
			{
				if (current.fleet.PlayerID == playerID)
				{
					return current;
				}
			}
			return null;
		}
		public void RemoveReaction(ReactionInfo info)
		{
			this._reactions.Remove(info);
		}
		public void CombatComplete()
		{
			if (this._currentCombat != null)
			{
				foreach (int current in this._currentCombat.PlayersInCombat)
				{
					if (current < this.App.GameSetup.Players.Count<PlayerSetup>() && current > 0 && !this.App.GameSetup.IsMultiplayer)
					{
						this.App.GameSetup.Players[current].Status = NPlayerStatus.PS_TURN;
					}
				}
				this._currentCombat = null;
			}
			if (!this.m_Combats.Any((PendingCombat x) => x.CombatResults == null))
			{
				this.m_Combats.Clear();
			}
			else
			{
				if (!this.App.GameSetup.IsMultiplayer)
				{
					this.LaunchNextCombat();
					return;
				}
			}
			if (!this.App.GameSetup.IsMultiplayer)
			{
				this._app.Game.NextTurn();
			}
		}
		public float GetIdealSuitability(Faction faction)
		{
			return this.m_SpeciesIdealSuitability[faction];
		}
		private Dictionary<ShipRole, List<DesignInfo>> GetFactionShipDesigns(IList<ShipRole> roles, IEnumerable<DesignInfo> initialDesigns, Kerberos.Sots.PlayerFramework.Player player)
		{
			Dictionary<ShipRole, List<DesignInfo>> dictionary = new Dictionary<ShipRole, List<DesignInfo>>();
			foreach (ShipRole role in roles)
			{
				DesignInfo designInfo = initialDesigns.FirstOrDefault((DesignInfo x) => x.Role == role);
				if (designInfo == null)
				{
					ShipRole role2 = role;
					string optionalName;
					switch (role2)
					{
					case ShipRole.COMMAND:
						optionalName = App.Localize("@DEFAULT_SHIPNAME_COMMAND");
						break;
					case ShipRole.COLONIZER:
						optionalName = App.Localize("@DEFAULT_SHIPNAME_COLONIZER");
						break;
					case ShipRole.CONSTRUCTOR:
						optionalName = App.Localize("@DEFAULT_SHIPNAME_CONSTRUCTOR");
						break;
					case ShipRole.SCOUT:
					case ShipRole.E_WARFARE:
						goto IL_10C;
					case ShipRole.SUPPLY:
						optionalName = App.Localize("@DEFAULT_SHIPNAME_SUPPLY");
						break;
					case ShipRole.GATE:
						optionalName = App.Localize("@DEFAULT_SHIPNAME_GATE");
						break;
					case ShipRole.BORE:
						optionalName = App.Localize("@DEFAULT_SHIPNAME_BORE");
						break;
					case ShipRole.FREIGHTER:
						optionalName = App.Localize("@DEFAULT_SHIPNAME_FREIGHTER");
						break;
					default:
						switch (role2)
						{
						case ShipRole.ACCELERATOR_GATE:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_ACCELERATOR");
							break;
						case ShipRole.LOA_CUBE:
							optionalName = App.Localize("@DEFAULT_SHIPNAME_LOA_CUBE");
							break;
						default:
							goto IL_10C;
						}
						break;
					}
					IL_10E:
					designInfo = DesignLab.SetDefaultDesign(this, role, null, player.ID, optionalName, new bool?(true), null, null);
					goto IL_13E;
					IL_10C:
					optionalName = null;
					goto IL_10E;
				}
				IL_13E:
				if (designInfo != null)
				{
					List<DesignInfo> list;
					if (!dictionary.TryGetValue(role, out list))
					{
						list = new List<DesignInfo>();
						dictionary[role] = list;
					}
					list.Add(designInfo);
				}
			}
			return dictionary;
		}
		public Dictionary<int, int> GetFleetDesignsFromTemplate(Kerberos.Sots.PlayerFramework.Player player, string templateName)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			FleetTemplate fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == templateName);
			if (fleetTemplate == null)
			{
				return dictionary;
			}
			foreach (ShipInclude current in fleetTemplate.ShipIncludes)
			{
				if ((string.IsNullOrEmpty(current.Faction) || !(current.Faction != player.Faction.Name)) && !(current.FactionExclusion == player.Faction.Name))
				{
					DesignInfo designInfo = DesignLab.GetBestDesignByRole(this, player, null, current.ShipRole, StrategicAI.GetEquivilantShipRoles(current.ShipRole), current.WeaponRole);
					if (designInfo == null)
					{
						designInfo = DesignLab.GetDesignByRole(this, player, null, null, current.ShipRole, current.WeaponRole);
					}
					if (designInfo != null)
					{
						int num;
						if (current.InclusionType == ShipInclusionType.FILL)
						{
							DesignInfo designInfo2 = DesignLab.GetBestDesignByRole(this, player, null, ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), null);
							if (designInfo2 == null)
							{
								designInfo2 = DesignLab.GetDesignByRole(this, player, null, null, ShipRole.COMMAND, null);
							}
							num = DesignLab.GetTemplateFillAmount(this._db, fleetTemplate, designInfo2, designInfo);
						}
						else
						{
							num = current.Amount;
						}
						if (dictionary.ContainsKey(designInfo.ID))
						{
							Dictionary<int, int> dictionary2;
							int iD;
							(dictionary2 = dictionary)[iD = designInfo.ID] = dictionary2[iD] + num;
						}
						else
						{
							dictionary.Add(designInfo.ID, num);
						}
					}
				}
			}
			return dictionary;
		}
		private int CreateFleetFromTemplate(Kerberos.Sots.PlayerFramework.Player player, string fleetName, string templateName, int admiralID, int systemID)
		{
			FleetTemplate fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == templateName);
			if (fleetTemplate == null)
			{
				return 0;
			}
			AIFleetInfo aIFleetInfo = new AIFleetInfo();
			aIFleetInfo.AdmiralID = new int?(admiralID);
			aIFleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes);
			aIFleetInfo.SystemID = systemID;
			aIFleetInfo.FleetTemplate = fleetTemplate.Name;
			aIFleetInfo.FleetID = new int?(this._db.InsertFleet(player.ID, admiralID, systemID, systemID, fleetName, FleetType.FL_NORMAL));
			int value = this._db.InsertAIFleetInfo(player.ID, aIFleetInfo);
			foreach (ShipInclude current in fleetTemplate.ShipIncludes)
			{
				if ((string.IsNullOrEmpty(current.Faction) || !(current.Faction != player.Faction.Name)) && !(current.FactionExclusion == player.Faction.Name))
				{
					DesignInfo designInfo = DesignLab.GetBestDesignByRole(this, player, null, current.ShipRole, StrategicAI.GetEquivilantShipRoles(current.ShipRole), current.WeaponRole);
					if (designInfo == null)
					{
						designInfo = DesignLab.GetDesignByRole(this, player, null, null, current.ShipRole, current.WeaponRole);
					}
					int num;
					if (current.InclusionType == ShipInclusionType.FILL)
					{
						DesignInfo designInfo2 = DesignLab.GetBestDesignByRole(this, player, null, ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), null);
						if (designInfo2 == null)
						{
							designInfo2 = DesignLab.GetDesignByRole(this, player, null, null, ShipRole.COMMAND, null);
						}
						num = DesignLab.GetTemplateFillAmount(this._db, fleetTemplate, designInfo2, designInfo);
					}
					else
					{
						num = current.Amount;
					}
					for (int i = 0; i < num; i++)
					{
						if (designInfo != null)
						{
							int numShipsBuiltFromDesign = this._db.GetNumShipsBuiltFromDesign(designInfo.ID);
							string text = this._db.GetDefaultShipName(designInfo.ID, numShipsBuiltFromDesign);
							text = this._db.ResolveNewShipName(player.ID, text);
							int carrierID = this._db.InsertShip(aIFleetInfo.FleetID.Value, designInfo.ID, text, (ShipParams)0, new int?(value), 0);
							this.AddDefaultStartingRiders(aIFleetInfo.FleetID.Value, designInfo.ID, carrierID);
						}
					}
				}
			}
			return aIFleetInfo.FleetID.Value;
		}
		private void SetRequiredDefaultDesigns()
		{
			foreach (int current in this.GameDatabase.GetStandardPlayerIDs())
			{
				Kerberos.Sots.PlayerFramework.Player playerObject = this.GetPlayerObject(current);
				ShipRole[] defaultBattleRiderShipRoles = playerObject.Faction.DefaultBattleRiderShipRoles;
				for (int i = 0; i < defaultBattleRiderShipRoles.Length; i++)
				{
					ShipRole role = defaultBattleRiderShipRoles[i];
					DesignLab.SetDefaultDesign(this, role, null, playerObject.ID, null, null, playerObject.TechStyles, playerObject.Stance);
				}
			}
		}
		public void AddStartingDeployedShips(GameDatabase db, int playerid)
		{
			if (db.GetFactionName(db.GetPlayerFactionID(playerid)) == "hiver")
			{
				DesignInfo designInfo = db.GetDesignInfosForPlayer(playerid, RealShipClasses.Cruiser, true).FirstOrDefault((DesignInfo x) => x.DesignSections.First<DesignSectionInfo>().ShipSectionAsset.IsGateShip);
				foreach (StarSystemInfo current in (
					from x in db.GetStarSystemInfos()
					where db.GetSystemOwningPlayer(x.ID).HasValue && db.GetSystemOwningPlayer(x.ID) == playerid
					select x).ToList<StarSystemInfo>())
				{
					int num = db.InsertFleet(playerid, 0, current.ID, current.ID, "GATE", FleetType.FL_GATE);
					int shipID = db.InsertShip(num, designInfo.ID, "Gate", ShipParams.HS_GATE_DEPLOYED, null, 0);
					Matrix? validGateShipTransform = GameSession.GetValidGateShipTransform(this, current.ID, num);
					if (validGateShipTransform.HasValue)
					{
						db.UpdateShipSystemPosition(shipID, new Matrix?(validGateShipTransform.Value));
					}
				}
			}
			if (db.GetFactionName(db.GetPlayerFactionID(playerid)) == "loa")
			{
				DesignInfo designInfo2 = db.GetDesignInfosForPlayer(playerid, RealShipClasses.Cruiser, true).FirstOrDefault((DesignInfo x) => x.IsAccelerator());
				HomeworldInfo homeworldInfo = this._db.GetHomeworlds().FirstOrDefault((HomeworldInfo x) => x.PlayerID == playerid);
				StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(homeworldInfo.SystemID);
				int num2 = db.InsertFleet(playerid, 0, starSystemInfo.ID, starSystemInfo.ID, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
				int shipID2 = db.InsertShip(num2, designInfo2.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, null, 0);
				Matrix? validGateShipTransform2 = GameSession.GetValidGateShipTransform(this, starSystemInfo.ID, num2);
				if (validGateShipTransform2.HasValue)
				{
					db.UpdateShipSystemPosition(shipID2, new Matrix?(validGateShipTransform2.Value));
				}
				foreach (StarSystemInfo current2 in (
					from x in db.GetStarSystemInfos()
					where db.GetSystemOwningPlayer(x.ID).HasValue && db.GetSystemOwningPlayer(x.ID) == playerid
					select x).ToList<StarSystemInfo>())
				{
					if (current2.ID != starSystemInfo.ID && db.GetSystemOwningPlayer(current2.ID).HasValue && !(db.GetSystemOwningPlayer(current2.ID) != playerid))
					{
						foreach (Vector3 current3 in Kerberos.Sots.StarFleet.StarFleet.GetAccelGateSlotsBetweenSystems(db, starSystemInfo.ID, current2.ID))
						{
							int num3 = db.InsertStarSystem(null, "Accelerator Node", null, "Deepspace", current3, false, false, null);
							int num4 = db.InsertFleet(playerid, 0, num3, num3, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
							db.InsertShip(num4, designInfo2.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, null, 0);
							db.InsertMoveOrder(num4, num3, current3, current2.ID, current2.Origin);
							NodeLineInfo nodeLineInfo = db.GetNodeLineBetweenSystems(playerid, starSystemInfo.ID, current2.ID, false, true);
							if (nodeLineInfo == null)
							{
								int id = db.InsertNodeLine(starSystemInfo.ID, current2.ID, 100);
								nodeLineInfo = db.GetNodeLine(id);
							}
							db.InsertLoaLineFleetRecord(nodeLineInfo.ID, num4);
						}
						int num5 = db.InsertFleet(playerid, 0, current2.ID, current2.ID, "ACCELERATOR FLEET", FleetType.FL_ACCELERATOR);
						int shipID3 = db.InsertShip(num5, designInfo2.ID, "NPG", ShipParams.HS_GATE_DEPLOYED, null, 0);
						NodeLineInfo nodeLineInfo2 = db.GetNodeLineBetweenSystems(playerid, starSystemInfo.ID, current2.ID, false, true);
						if (nodeLineInfo2 == null)
						{
							int id2 = db.InsertNodeLine(starSystemInfo.ID, current2.ID, 100);
							nodeLineInfo2 = db.GetNodeLine(id2);
						}
						db.InsertLoaLineFleetRecord(nodeLineInfo2.ID, num5);
						validGateShipTransform2 = GameSession.GetValidGateShipTransform(this, current2.ID, num5);
						if (validGateShipTransform2.HasValue)
						{
							db.UpdateShipSystemPosition(shipID3, new Matrix?(validGateShipTransform2.Value));
						}
					}
				}
			}
		}
		public void AddDefaultStartingFleets(Kerberos.Sots.PlayerFramework.Player player)
		{
			List<DesignInfo> list = new List<DesignInfo>();
			if (player.Faction.InitialDesigns != null)
			{
				InitialDesign[] initialDesigns = player.Faction.InitialDesigns;
				for (int i = 0; i < initialDesigns.Length; i++)
				{
					InitialDesign initialDesign = initialDesigns[i];
					AITechStyles optionalAITechStyles = null;
					if (!string.IsNullOrEmpty(initialDesign.WeaponBiasTechFamilyID))
					{
						optionalAITechStyles = new AITechStyles(this.AssetDatabase, new AITechStyleInfo[]
						{
							new AITechStyleInfo
							{
								CostFactor = 0.1f,
								PlayerID = player.ID,
								TechFamily = this.AssetDatabase.MasterTechTree.GetTechFamilyEnumFromName(initialDesign.WeaponBiasTechFamilyID)
							}
						});
					}
					DesignInfo design = DesignLab.CreateInitialShipDesign(this, initialDesign.Name, initialDesign.EnumerateShipSectionAssets(this.AssetDatabase, player.Faction), player.ID, optionalAITechStyles);
					list.Add(this._db.GetDesignInfo(this._db.InsertDesignByDesignInfo(design)));
				}
			}
			Dictionary<ShipRole, List<DesignInfo>> factionShipDesigns = this.GetFactionShipDesigns(player.Faction.DefaultCombinedShipRoles, list, player);
			Dictionary<ShipRole, int> dictionary = new Dictionary<ShipRole, int>();
			foreach (KeyValuePair<ShipRole, List<DesignInfo>> current in factionShipDesigns)
			{
				dictionary[current.Key] = current.Value[0].ID;
			}
			int systemID = this.GameDatabase.GetPlayerHomeworld(player.ID).SystemID;
			int arg_183_0 = this.GameDatabase.GetNavalStationForSystemAndPlayer(systemID, player.ID).OrbitalObjectID;
			int availableAdmiral = GameSession.GetAvailableAdmiral(this.GameDatabase, player.ID);
			if (availableAdmiral == 0)
			{
				return;
			}
			this.GameDatabase.ClearAdmiralTraits(availableAdmiral);
			this.GameDatabase.AddAdmiralTrait(availableAdmiral, AdmiralInfo.TraitType.Pathfinder, 1);
			int num = this.CreateFleetFromTemplate(player, "1st Survey Fleet", "DEFAULT_SURVEY", availableAdmiral, systemID);
			int value = 0;
			if (num != 0)
			{
				this.GameDatabase.LayoutFleet(num);
				if (player.Faction.Name == "loa")
				{
					List<ShipInfo> source = this.GameDatabase.GetShipInfoByFleetID(num, false).ToList<ShipInfo>();
					value = this.GameDatabase.InsertLoaFleetComposition(player.ID, "Basic Survey", 
						from x in source
						select x.DesignID);
                    int shipId = Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, num);
					this.GameDatabase.UpdateShipLoaCubes(shipId, 70000);
					this.GameDatabase.UpdateFleetCompositionID(num, new int?(value));
				}
			}
			availableAdmiral = GameSession.GetAvailableAdmiral(this.GameDatabase, player.ID);
			if (availableAdmiral == 0)
			{
				return;
			}
			this.GameDatabase.ClearAdmiralTraits(availableAdmiral);
			this.GameDatabase.AddAdmiralTrait(availableAdmiral, AdmiralInfo.TraitType.Pathfinder, 1);
			num = this.CreateFleetFromTemplate(player, "2nd Survey Fleet", "DEFAULT_SURVEY", availableAdmiral, systemID);
			if (num != 0)
			{
				this.GameDatabase.LayoutFleet(num);
				if (player.Faction.Name == "loa")
				{
                    int shipId2 = Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, num);
					this.GameDatabase.UpdateShipLoaCubes(shipId2, 70000);
					this.GameDatabase.UpdateFleetCompositionID(num, new int?(value));
				}
			}
			availableAdmiral = GameSession.GetAvailableAdmiral(this.GameDatabase, player.ID);
			if (availableAdmiral == 0)
			{
				return;
			}
			this.GameDatabase.ClearAdmiralTraits(availableAdmiral);
			this.GameDatabase.AddAdmiralTrait(availableAdmiral, AdmiralInfo.TraitType.Architect, 1);
			num = this.CreateFleetFromTemplate(player, "1st Construction Fleet", "DEFAULT_CONSTRUCTION", availableAdmiral, systemID);
			if (num != 0)
			{
				this.GameDatabase.LayoutFleet(num);
				if (player.Faction.Name == "loa")
				{
					List<ShipInfo> source2 = this.GameDatabase.GetShipInfoByFleetID(num, false).ToList<ShipInfo>();
					int value2 = this.GameDatabase.InsertLoaFleetComposition(player.ID, "Basic Construction", 
						from x in source2
						select x.DesignID);
                    int shipId3 = Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, num);
					this.GameDatabase.UpdateShipLoaCubes(shipId3, 70000);
					this.GameDatabase.UpdateFleetCompositionID(num, new int?(value2));
				}
			}
			availableAdmiral = GameSession.GetAvailableAdmiral(this.GameDatabase, player.ID);
			if (availableAdmiral == 0)
			{
				return;
			}
			this.GameDatabase.ClearAdmiralTraits(availableAdmiral);
			this.GameDatabase.AddAdmiralTrait(availableAdmiral, AdmiralInfo.TraitType.GreenThumb, 1);
			num = this.CreateFleetFromTemplate(player, "1st Colonization Fleet", "DEFAULT_COLONIZATION", availableAdmiral, systemID);
			if (num != 0)
			{
				this.GameDatabase.LayoutFleet(num);
				if (player.Faction.Name == "loa")
				{
					List<ShipInfo> source3 = this.GameDatabase.GetShipInfoByFleetID(num, false).ToList<ShipInfo>();
					int value3 = this.GameDatabase.InsertLoaFleetComposition(player.ID, "Basic Colonization", 
						from x in source3
						select x.DesignID);
                    int shipId4 = Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this, num);
					this.GameDatabase.UpdateShipLoaCubes(shipId4, 70000);
					this.GameDatabase.UpdateFleetCompositionID(num, new int?(value3));
				}
			}
			if (player.Faction.Name == "hiver")
			{
				availableAdmiral = GameSession.GetAvailableAdmiral(this.GameDatabase, player.ID);
				if (availableAdmiral == 0)
				{
					return;
				}
				num = this.CreateFleetFromTemplate(player, "1st Gate Fleet", "DEFAULT_GATE", availableAdmiral, systemID);
				if (num != 0)
				{
					this.GameDatabase.LayoutFleet(num);
				}
				availableAdmiral = GameSession.GetAvailableAdmiral(this.GameDatabase, player.ID);
				if (availableAdmiral == 0)
				{
					return;
				}
				num = this.CreateFleetFromTemplate(player, "2nd Gate Fleet", "DEFAULT_GATE", availableAdmiral, systemID);
				if (num != 0)
				{
					this.GameDatabase.LayoutFleet(num);
				}
			}
		}
		private void AddDefaultShip(int fleetID, int designID)
		{
			int carrierID = this.GameDatabase.InsertShip(fleetID, designID, null, (ShipParams)0, null, 0);
			this.AddDefaultStartingRiders(fleetID, designID, carrierID);
		}
		public void AddDefaultStartingRiders(int fleetID, int designID, int carrierID)
		{
			bool flag = false;
			int num = 0;
			DesignInfo designInfo = this._db.GetDesignInfo(designID);
			DesignSectionInfo[] designSections = designInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				foreach (WeaponBankInfo wpnBank in designSectionInfo.WeaponBanks)
				{
					ShipSectionAsset shipSectionAsset = designSectionInfo.ShipSectionAsset;
					if (shipSectionAsset != null)
					{
						LogicalBank lb = shipSectionAsset.Banks.FirstOrDefault((LogicalBank x) => x.GUID == wpnBank.BankGUID);
						if (lb != null)
						{
							if (wpnBank.DesignID.HasValue && wpnBank.DesignID != 0)
							{
								if (shipSectionAsset != null)
								{
									int num2 = (
										from x in shipSectionAsset.Mounts
										where x.Bank == lb
										select x).Count<LogicalMount>();
									for (int j = 0; j < num2; j++)
									{
										int shipID = this._db.InsertShip(fleetID, wpnBank.DesignID.Value, null, (ShipParams)0, null, 0);
										this._db.SetShipParent(shipID, carrierID);
										this._db.UpdateShipRiderIndex(shipID, num);
										num++;
									}
								}
							}
							else
							{
								if (WeaponEnums.IsBattleRider(lb.TurretClass))
								{
									num += (
										from x in shipSectionAsset.Mounts
										where x.Bank == lb
										select x).Count<LogicalMount>();
								}
							}
						}
					}
				}
				foreach (DesignModuleInfo current in designSectionInfo.Modules)
				{
					string moduleAsset = this._db.GetModuleAsset(current.ModuleID);
					LogicalModule logicalModule = this.AssetDatabase.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == moduleAsset);
					if (logicalModule != null)
					{
						if (logicalModule.Banks.Count<LogicalBank>() > 0 && WeaponEnums.IsWeaponBattleRider(logicalModule.Banks.First<LogicalBank>().TurretClass) && (!current.DesignID.HasValue || current.DesignID.Value == 0))
						{
							current.DesignID = DesignLab.ChooseBattleRider(this, DesignLab.GetWeaponRiderShipRole(logicalModule.Banks.First<LogicalBank>().TurretClass, designSectionInfo.ShipSectionAsset.IsScavenger), designInfo.WeaponRole, designInfo.PlayerID);
							if (current.DesignID.HasValue)
							{
								flag = true;
							}
						}
						if (current.DesignID.HasValue && current.DesignID != 0)
						{
							if (logicalModule != null)
							{
								int num3 = logicalModule.Mounts.Count<LogicalMount>();
								for (int k = 0; k < num3; k++)
								{
									int shipID2 = this._db.InsertShip(fleetID, current.DesignID.Value, null, (ShipParams)0, null, 0);
									this._db.SetShipParent(shipID2, carrierID);
									this._db.UpdateShipRiderIndex(shipID2, num);
									num++;
								}
							}
						}
						else
						{
							if (logicalModule.Banks.Count<LogicalBank>() > 0 && WeaponEnums.IsBattleRider(logicalModule.Banks.First<LogicalBank>().TurretClass))
							{
								num += logicalModule.Mounts.Count<LogicalMount>();
							}
						}
					}
				}
			}
			if (flag)
			{
				this._db.UpdateDesign(designInfo);
			}
		}
		private void HandleIntelMissionsForPlayer(int playerid)
		{
			List<IntelMissionInfo> list = (
				from x in this.GameDatabase.GetIntelInfosForPlayer(playerid)
				where x.Turn == this.GameDatabase.GetTurnCount() - 1
				select x).ToList<IntelMissionInfo>();
			List<int> list2 = new List<int>();
			foreach (IntelMissionInfo mis in list)
			{
				this.AssetDatabase.IntelMissions.First((IntelMissionDesc x) => x.ID == mis.MissionType).OnCommit(this, playerid, mis.TargetPlayerId, new int?(mis.ID));
				if (mis.BlamePlayer.HasValue && !list2.Contains(mis.BlamePlayer.Value))
				{
					list2.Add(mis.BlamePlayer.Value);
					this.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INTEL_MISSION_CRITICAL_FAILED_LEAK,
						EventMessage = TurnEventMessage.EM_INTEL_MISSION_CRITICAL_FAILED_LEAK,
						PlayerID = mis.TargetPlayerId,
						TargetPlayerID = mis.BlamePlayer.Value,
						TurnNumber = this.GameDatabase.GetTurnCount()
					});
				}
				this.GameDatabase.RemoveIntelMission(mis.ID);
			}
		}
		private void HandleCounterIntelMissions(int playerid)
		{
			List<IntelMissionInfo> list = (
				from x in this.GameDatabase.GetIntelInfosForPlayer(playerid)
				where x.Turn == this.GameDatabase.GetTurnCount()
				select x).ToList<IntelMissionInfo>();
			foreach (IntelMissionInfo current in list)
			{
				this.DoCounterIntelMission(playerid, current.TargetPlayerId, current);
			}
		}
		public int GetSystemRepairPoints(int systemid, int playerid)
		{
			List<ColonyInfo> list = (
				from x in this._db.GetColonyInfosForSystem(systemid)
				where x.PlayerID == playerid
				select x).ToList<ColonyInfo>();
			int num = 0;
			foreach (ColonyInfo current in list)
			{
				num += current.RepairPoints;
			}
			List<StationInfo> list2 = this._db.GetStationForSystemAndPlayer(systemid, playerid).ToList<StationInfo>();
			foreach (StationInfo current2 in list2)
			{
				List<SectionInstanceInfo> list3 = this._db.GetShipSectionInstances(current2.ShipID).ToList<SectionInstanceInfo>();
				foreach (SectionInstanceInfo current3 in list3)
				{
					num += current3.RepairPoints;
				}
			}
			List<FleetInfo> list4 = this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current4 in list4)
			{
				List<ShipInfo> list5 = this._db.GetShipInfoByFleetID(current4.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo current5 in list5)
				{
					List<SectionInstanceInfo> list6 = this._db.GetShipSectionInstances(current5.ID).ToList<SectionInstanceInfo>();
					foreach (SectionInstanceInfo current6 in list6)
					{
						num += current6.RepairPoints;
					}
				}
			}
			return num;
		}
		public void UseSystemRepairPoints(int systemid, int playerid, int points)
		{
			List<ColonyInfo> list = (
				from x in this._db.GetColonyInfosForSystem(systemid)
				where x.PlayerID == playerid
				select x).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				if (points <= 0)
				{
					return;
				}
				int num = current.RepairPoints - points;
				if (num < 0)
				{
					num = 0;
				}
				points -= current.RepairPoints;
				current.RepairPoints = num;
				this.App.GameDatabase.UpdateColony(current);
			}
			List<StationInfo> list2 = this._db.GetStationForSystemAndPlayer(systemid, playerid).ToList<StationInfo>();
			foreach (StationInfo current2 in list2)
			{
				List<SectionInstanceInfo> list3 = this._db.GetShipSectionInstances(current2.ShipID).ToList<SectionInstanceInfo>();
				foreach (SectionInstanceInfo current3 in list3)
				{
					if (points <= 0)
					{
						return;
					}
					int num2 = current3.RepairPoints - points;
					if (num2 < 0)
					{
						num2 = 0;
					}
					points -= current3.RepairPoints;
					current3.RepairPoints -= num2;
					this.App.GameDatabase.UpdateSectionInstance(current3);
				}
			}
			List<FleetInfo> list4 = this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current4 in list4)
			{
				List<ShipInfo> list5 = this._db.GetShipInfoByFleetID(current4.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo current5 in list5)
				{
					List<SectionInstanceInfo> list6 = this._db.GetShipSectionInstances(current5.ID).ToList<SectionInstanceInfo>();
					foreach (SectionInstanceInfo current6 in list6)
					{
						if (points <= 0)
						{
							return;
						}
						int num3 = current6.RepairPoints - points;
						if (num3 < 0)
						{
							num3 = 0;
						}
						points -= current6.RepairPoints;
						current6.RepairPoints -= num3;
						this.App.GameDatabase.UpdateSectionInstance(current6);
					}
				}
			}
		}
		private int RepairFleet(int fleetid, int availpoints)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetid);
			if (fleetInfo == null || fleetInfo.SystemID == 0)
			{
				return availpoints;
			}
			if (availpoints <= 0)
			{
				return availpoints;
			}
			List<ShipInfo> list = this._db.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
			int num = 0;
			foreach (ShipInfo current in list)
			{
                int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(this, current.DesignInfo, current.ID);
				int num2 = healthAndHealthMax[1] - healthAndHealthMax[0];
				if (availpoints <= 0)
				{
					break;
				}
				if (num2 > 0)
				{
					if (num2 > availpoints)
					{
						num2 = availpoints;
					}
					if (num2 > availpoints)
					{
						break;
					}
					num += num2;
					availpoints -= num2;
                    Kerberos.Sots.StarFleet.StarFleet.RepairShip(this.App, current, num2);
				}
			}
			this.UseSystemRepairPoints(fleetInfo.SystemID, fleetInfo.PlayerID, num);
			return availpoints - num;
		}
		public void RepairFleet(int fleetid)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetid);
			int systemRepairPoints = this.GetSystemRepairPoints(fleetInfo.SystemID, fleetInfo.PlayerID);
			if (systemRepairPoints <= 0)
			{
				return;
			}
			this.RepairFleet(fleetInfo.ID, systemRepairPoints);
		}
		public void RepairFleetsAtSystem(int systemid, int playerid)
		{
			int num = this.GetSystemRepairPoints(systemid, playerid);
			if (num <= 0)
			{
				return;
			}
			List<FleetInfo> list = (
				from x in this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_DEFENSE | FleetType.FL_GATE | FleetType.FL_STATION | FleetType.FL_CARAVAN | FleetType.FL_ACCELERATOR)
				where x.PlayerID == playerid
				select x).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				num = this.RepairFleet(current.ID, num);
			}
		}
		private float CalcBoostResearchAccidentOdds(int playerid)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(playerid);
			Faction faction = this.AssetDatabase.GetFaction(playerInfo.FactionID);
			double researchBoostFunds = playerInfo.ResearchBoostFunds;
			if (researchBoostFunds <= 0.0)
			{
				return 0f;
			}
			double savings = playerInfo.Savings;
			if (savings <= 0.0)
			{
				return 1f;
			}
			float num = (float)researchBoostFunds / (float)savings;
			num *= 2f;
			num *= faction.ResearchBoostFailureMod;
			if (num < 0f)
			{
				num = 0f;
			}
			if (num > 1f)
			{
				num = 1f;
			}
			return num;
		}
		private bool DoResearchAccident(int playerid)
		{
			List<StarSystemInfo> source = (
				from x in this.GameDatabase.GetStarSystemInfos()
				where this.GameDatabase.GetSystemOwningPlayer(x.ID) == playerid
				select x).ToList<StarSystemInfo>();
			if (!source.Any<StarSystemInfo>())
			{
				return false;
			}
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(playerid);
			int playerResearchingTechID = this.GameDatabase.GetPlayerResearchingTechID(playerid);
			if (playerResearchingTechID == 0)
			{
				return false;
			}
			string techID = this.App.GameDatabase.GetTechFileID(playerResearchingTechID);
			Kerberos.Sots.Data.TechnologyFramework.Tech techno = this.App.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech tech) => tech.Id == techID);
			if (techno.Group == "Group Bioweapons")
			{
				LogicalWeapon logicalWeapon = this.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.RequiredTechs.Any((Kerberos.Sots.Data.WeaponFramework.Tech j) => j.Name == techno.Name) && x.PlagueType != WeaponEnums.PlagueType.NONE);
				if (logicalWeapon == null)
				{
					return false;
				}
				PlayerTechInfo playerTechInfo = this.GameDatabase.GetPlayerTechInfo(playerid, playerResearchingTechID);
				playerTechInfo.Progress = 0;
				this.GameDatabase.UpdatePlayerTechInfo(playerTechInfo);
				ColonyInfo colonyInfo = this.GameDatabase.GetColonyInfos().FirstOrDefault((ColonyInfo x) => x.PlayerID == playerid);
				if (colonyInfo != null)
				{
					GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_PLAGUE_OUTBREAK, colonyInfo.PlayerID, new int?(colonyInfo.ID), null, null);
					PlagueInfo pi = new PlagueInfo
					{
						PlagueType = logicalWeapon.PlagueType,
						ColonyId = colonyInfo.ID,
						LaunchingPlayer = playerid,
						InfectedPopulationCivilian = Math.Floor((double)(logicalWeapon.PopDamage * 0.75f)),
						InfectedPopulationImperial = Math.Floor((double)(logicalWeapon.PopDamage * 0.25f)),
						InfectionRate = this.App.AssetDatabase.GetPlagueInfectionRate(logicalWeapon.PlagueType)
					};
					this.App.GameDatabase.InsertPlagueInfo(pi);
					this.App.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_RESEARCH_PLAGUE_DISASTER,
						EventMessage = TurnEventMessage.EM_RESEARCH_PLAGUE_DISASTER,
						PlagueType = logicalWeapon.PlagueType,
						ColonyID = colonyInfo.ID,
						PlayerID = colonyInfo.PlayerID,
						TurnNumber = this.App.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					return true;
				}
				return false;
			}
			else
			{
				if (techno.Group == "Group AI" && techno.AllowAIRebellion)
				{
					this.DoAIRebellion(playerInfo, true);
					PlayerTechInfo playerTechInfo2 = this.GameDatabase.GetPlayerTechInfo(playerid, playerResearchingTechID);
					playerTechInfo2.Progress = 0;
					this.GameDatabase.UpdatePlayerTechInfo(playerTechInfo2);
					return true;
				}
				return false;
			}
		}
		private bool DoBoostResearchAccidentRoll(int playerid)
		{
			PlayerInfo playerInfo = this.GameDatabase.GetPlayerInfo(playerid);
			int playerResearchingTechID = this.GameDatabase.GetPlayerResearchingTechID(playerid);
			this.AssetDatabase.GetFaction(playerInfo.FactionID);
			if (playerResearchingTechID == 0)
			{
				return false;
			}
			string techID = this.App.GameDatabase.GetTechFileID(playerResearchingTechID);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech2 = this.App.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech tech) => tech.Id == techID);
			int num = (int)(this.CalcBoostResearchAccidentOdds(playerid) * 100f);
			if (num <= 0)
			{
				return false;
			}
			int num2 = new Random().NextInclusive(0, 100);
			if (num2 > num)
			{
				return false;
			}
			if (this.DoResearchAccident(playerid))
			{
				return true;
			}
			Kerberos.Sots.Data.TechnologyFramework.Tech.TechThreatDamage techThreatDamage = Kerberos.Sots.Data.TechnologyFramework.Tech.GetTechThreatDamage(tech2.DangerLevel - 1);
			StarSystemInfo starSystemInfo = null;
			ColonyInfo colonyInfo = null;
			if (techThreatDamage.m_bRequiresSystem)
			{
				List<StarSystemInfo> list = (
					from x in this.GameDatabase.GetStarSystemInfos()
					where this.GameDatabase.GetSystemOwningPlayer(x.ID) == playerid
					select x).ToList<StarSystemInfo>();
				if (!list.Any<StarSystemInfo>())
				{
					return false;
				}
				starSystemInfo = list[new Random().Next(list.Count)];
				colonyInfo = this.GameDatabase.GetColonyInfosForSystem(starSystemInfo.ID).FirstOrDefault((ColonyInfo x) => x.PlayerID == playerid);
				if (colonyInfo == null)
				{
					return false;
				}
			}
			float num3 = (float)new Random().Next(1, 5);
			techThreatDamage.m_PopulationMin *= num3;
			techThreatDamage.m_PopulationMax *= num3;
			techThreatDamage.m_InfraMin *= num3;
			techThreatDamage.m_InfraMax *= num3;
			double num4 = 0.0;
			double num5 = 0.0;
			PlayerTechInfo playerTechInfo = this.GameDatabase.GetPlayerTechInfo(playerid, playerResearchingTechID);
			int arg_20C_0 = playerTechInfo.Progress;
			int num6 = (int)((float)new Random().Next(techThreatDamage.m_ProgressMin, techThreatDamage.m_ProgressMax) / 100f * (float)playerTechInfo.Progress);
			int progress = playerTechInfo.Progress;
			int progress2 = Math.Max(progress - num6, 0);
			playerTechInfo.Progress = progress2;
			this.GameDatabase.UpdatePlayerTechInfo(playerTechInfo);
			int arg_26B_0 = playerTechInfo.Progress;
			if (starSystemInfo != null && colonyInfo != null)
			{
				PlanetInfo planetInfo = this.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
				num4 = this.GameDatabase.GetTotalPopulation(colonyInfo);
				float arg_2AA_0 = planetInfo.Infrastructure;
				float num7 = techThreatDamage.m_PopulationMin + (float)new Random().NextDouble() * (techThreatDamage.m_PopulationMax - techThreatDamage.m_PopulationMin);
				double imperialPop = colonyInfo.ImperialPop;
				double num8 = (double)num7 * imperialPop;
				double imperialPop2 = imperialPop - num8;
				colonyInfo.ImperialPop = imperialPop2;
				this.GameDatabase.UpdateColony(colonyInfo);
				float num9 = techThreatDamage.m_InfraMin + (float)new Random().NextDouble() * (techThreatDamage.m_InfraMax - techThreatDamage.m_InfraMin);
				float infrastructure = planetInfo.Infrastructure;
				float num10 = num9 * infrastructure;
				float infrastructure2 = infrastructure - num10;
				planetInfo.Infrastructure = infrastructure2;
				this.GameDatabase.UpdatePlanet(planetInfo);
				num5 = this.GameDatabase.GetTotalPopulation(colonyInfo);
				float arg_367_0 = planetInfo.Infrastructure;
			}
			if (tech2.DangerLevel == 1)
			{
				this._db.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_BOOSTFAILED_1,
					EventMessage = TurnEventMessage.EM_BOOSTFAILED_1,
					PlayerID = playerid,
					TechID = playerTechInfo.TechID,
					TurnNumber = this._app.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			}
			else
			{
				if (tech2.DangerLevel == 2 && colonyInfo != null)
				{
					this._db.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_BOOSTFAILED_2,
						EventMessage = TurnEventMessage.EM_BOOSTFAILED_2,
						PlayerID = playerid,
						TechID = playerTechInfo.TechID,
						ColonyID = colonyInfo.ID,
						ImperialPop = (float)(num4 - num5),
						TurnNumber = this._app.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
				else
				{
					if (tech2.DangerLevel == 3 && colonyInfo != null)
					{
						this._db.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_BOOSTFAILED_3,
							EventMessage = TurnEventMessage.EM_BOOSTFAILED_3,
							PlayerID = playerid,
							TechID = playerTechInfo.TechID,
							ColonyID = colonyInfo.ID,
							ImperialPop = (float)(num4 - num5),
							TurnNumber = this._app.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
			return true;
		}
		public void Dispose()
		{
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
			}
			if (this._db != null)
			{
				this._db.Dispose();
				this._db = null;
			}
		}
	}
}
