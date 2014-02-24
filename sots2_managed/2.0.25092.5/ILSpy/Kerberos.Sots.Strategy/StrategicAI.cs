using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PerformanceStarFleet = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.StarFleet;

namespace Kerberos.Sots.Strategy
{
	internal class StrategicAI
	{
		private struct DesignPriority
		{
			public ShipRole role;
			public float weight;
		}
		public class UpdatePlayerInfo
		{
			public readonly GameDatabase _db;
			public readonly PlayerInfo Player;
			public readonly Dictionary<int, DiplomacyInfo> Relations = new Dictionary<int, DiplomacyInfo>();
			internal UpdatePlayerInfo(GameDatabase db, IEnumerable<PlayerInfo> players, PlayerInfo player)
			{
				this._db = db;
				this.Player = player;
				foreach (PlayerInfo current in players)
				{
					this.Relations[current.ID] = this._db.GetDiplomacyInfo(player.ID, current.ID);
				}
			}
		}
		public class UpdateInfo
		{
			public readonly Dictionary<int, StrategicAI.UpdatePlayerInfo> Players = new Dictionary<int, StrategicAI.UpdatePlayerInfo>();
			public UpdateInfo(GameDatabase db)
			{
				List<PlayerInfo> list = db.GetPlayerInfos().ToList<PlayerInfo>();
				foreach (PlayerInfo current in list)
				{
					this.Players[current.ID] = new StrategicAI.UpdatePlayerInfo(db, list, current);
				}
			}
		}
		public class BattleRiderMountSet
		{
			private Dictionary<WeaponEnums.TurretClasses, List<ShipInfo>> tcmap;
			private List<ShipInfo> set;
			public static WeaponEnums.TurretClasses? GetMatchingTurretClass(DesignInfo design)
			{
				BattleRiderTypes battleRiderType = design.GetMissionSectionAsset().BattleRiderType;
				if (battleRiderType.IsBattleRiderType() && design.Class == ShipClass.BattleRider)
				{
					return new WeaponEnums.TurretClasses?(WeaponEnums.TurretClasses.DestroyerRider);
				}
				if (battleRiderType.IsControllableBattleRider() && design.GetRealShipClass() == RealShipClasses.BattleCruiser)
				{
					return new WeaponEnums.TurretClasses?(WeaponEnums.TurretClasses.CruiserRider);
				}
				if (battleRiderType.IsControllableBattleRider() && design.GetRealShipClass() == RealShipClasses.BattleShip)
				{
					return new WeaponEnums.TurretClasses?(WeaponEnums.TurretClasses.DreadnoughtRider);
				}
				return null;
			}
			public static IEnumerable<ShipRole> EnumerateShipRolesByTurretClass(WeaponEnums.TurretClasses value)
			{
				switch (value)
				{
				case WeaponEnums.TurretClasses.DestroyerRider:
					yield return ShipRole.BR_PATROL;
					yield return ShipRole.BR_SCOUT;
					yield return ShipRole.BR_SPINAL;
					yield return ShipRole.BR_ESCORT;
					yield return ShipRole.BR_INTERCEPTOR;
					yield return ShipRole.BR_TORPEDO;
					break;
				case WeaponEnums.TurretClasses.CruiserRider:
					yield return ShipRole.BATTLECRUISER;
					break;
				case WeaponEnums.TurretClasses.DreadnoughtRider:
					yield return ShipRole.BATTLESHIP;
					break;
				}
				yield break;
			}
			public static bool IsBattleRiderTurretClass(WeaponEnums.TurretClasses value)
			{
				switch (value)
				{
				case WeaponEnums.TurretClasses.DestroyerRider:
				case WeaponEnums.TurretClasses.CruiserRider:
				case WeaponEnums.TurretClasses.DreadnoughtRider:
					return true;
				default:
					return false;
				}
			}
			private void FilterAdd(ShipInfo value)
			{
				if (value.ParentID != 0)
				{
					return;
				}
				WeaponEnums.TurretClasses? matchingTurretClass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(value.DesignInfo);
				if (!matchingTurretClass.HasValue)
				{
					return;
				}
				this.tcmap[matchingTurretClass.Value].Add(value);
				this.set.Add(value);
			}
			public BattleRiderMountSet(IEnumerable<ShipInfo> ships)
			{
				this.tcmap = new Dictionary<WeaponEnums.TurretClasses, List<ShipInfo>>();
				this.tcmap[WeaponEnums.TurretClasses.DestroyerRider] = new List<ShipInfo>();
				this.tcmap[WeaponEnums.TurretClasses.CruiserRider] = new List<ShipInfo>();
				this.tcmap[WeaponEnums.TurretClasses.DreadnoughtRider] = new List<ShipInfo>();
				this.set = new List<ShipInfo>();
				foreach (ShipInfo current in ships)
				{
					this.FilterAdd(current);
				}
			}
			public IEnumerable<ShipInfo> EnumerateByTurretClass(WeaponEnums.TurretClasses value)
			{
				return this.tcmap[value];
			}
			public bool Contains(ShipInfo value)
			{
				return this.set.Contains(value);
			}
			public void Remove(ShipInfo value)
			{
				this.set.Remove(value);
				foreach (List<ShipInfo> current in this.tcmap.Values)
				{
					current.Remove(value);
				}
			}
			public static IEnumerable<KeyValuePair<LogicalMount, int>> EnumerateBattleRiderMounts(DesignInfo design)
			{
				int num = 0;
				try
				{
					DesignSectionInfo[] designSections = design.DesignSections;
					for (int i = 0; i < designSections.Length; i++)
					{
						DesignSectionInfo designSectionInfo = designSections[i];
						try
						{
							LogicalMount[] mounts = designSectionInfo.ShipSectionAsset.Mounts;
							for (int j = 0; j < mounts.Length; j++)
							{
								LogicalMount logicalMount = mounts[j];
								if (WeaponEnums.IsBattleRider(logicalMount.Bank.TurretClass))
								{
									if (StrategicAI.BattleRiderMountSet.IsBattleRiderTurretClass(logicalMount.Bank.TurretClass))
									{
										yield return new KeyValuePair<LogicalMount, int>(logicalMount, num);
									}
									num++;
								}
							}
						}
						finally
						{
						}
					}
				}
				finally
				{
				}
				yield break;
			}
			public ShipInfo FindShipByRole(ShipRole role)
			{
				return this.set.FirstOrDefault((ShipInfo x) => x.DesignInfo.Role == role);
			}
			public void AttachBattleRiderToShip(GameDatabase db, StrategicAI.BankRiderInfo bankRiderInfo, ShipInfo battleRider, ShipInfo carrier, int riderIndex)
			{
				this.Remove(battleRider);
				bankRiderInfo.FreeRiderIndices.Remove(riderIndex);
				bankRiderInfo.FilledRiderIndices.Add(riderIndex);
				bankRiderInfo.AllocatedRole = new ShipRole?(battleRider.DesignInfo.Role);
				db.SetShipParent(battleRider.ID, carrier.ID);
				db.UpdateShipRiderIndex(battleRider.ID, riderIndex);
			}
		}
		public class BankRiderInfo
		{
			public LogicalBank Bank;
			public ShipRole? AllocatedRole;
			public List<int> FilledRiderIndices = new List<int>();
			public List<int> FreeRiderIndices = new List<int>();
		}
		public struct DesignConfigurationInfo
		{
			public float Maxspeed;
			public float PointDefense;
			public float Defensive;
			public float MissileWeapons;
			public float EnergyDefense;
			public float BallisticsDefense;
			public float EnergyWeapons;
			public float HeavyBeamWeapons;
			public float BallisticsWeapons;
			public void Average(int size)
			{
				this.Maxspeed /= (float)size;
				this.PointDefense /= (float)size;
				this.Defensive /= (float)size;
				this.MissileWeapons /= (float)size;
				this.EnergyDefense /= (float)size;
				this.BallisticsDefense /= (float)size;
				this.EnergyWeapons /= (float)size;
				this.HeavyBeamWeapons /= (float)size;
				this.BallisticsWeapons /= (float)size;
			}
			public static StrategicAI.DesignConfigurationInfo operator +(StrategicAI.DesignConfigurationInfo c1, StrategicAI.DesignConfigurationInfo c2)
			{
				return new StrategicAI.DesignConfigurationInfo
				{
					Maxspeed = c1.Maxspeed + c2.Maxspeed,
					PointDefense = c1.PointDefense + c2.PointDefense,
					Defensive = c1.Defensive + c2.Defensive,
					MissileWeapons = c1.MissileWeapons + c2.MissileWeapons,
					EnergyDefense = c1.EnergyDefense + c2.EnergyDefense,
					BallisticsDefense = c1.BallisticsDefense + c2.BallisticsDefense,
					EnergyWeapons = c1.EnergyWeapons + c2.EnergyWeapons,
					HeavyBeamWeapons = c1.HeavyBeamWeapons + c2.HeavyBeamWeapons,
					BallisticsWeapons = c1.BallisticsWeapons + c2.BallisticsWeapons
				};
			}
		}
		private const float FastResearchRate = 0.7f;
		private const float SlowResearchRate = 0.3f;
		private const double AI_RELATION0 = 0.0;
		private const double AI_RELATION1 = 2000.0;
		private const double AI_RELATION_WAR0 = 0.0;
		private const double AI_RELATION_WAR = 300.0;
		private const double AI_RELATION_WAR1 = 750.0;
		private const double AI_RELATION_NEUTRAL0 = 750.0;
		private const double AI_RELATION_NEUTRAL = 925.0;
		private const double AI_RELATION_NEUTRAL1 = 1100.0;
		private const double AI_RELATION_CEASEFIRE0 = 1100.0;
		private const double AI_RELATION_CEASEFIRE = 1250.0;
		private const double AI_RELATION_CEASEFIRE1 = 1400.0;
		private const double AI_RELATION_NAP0 = 1400.0;
		private const double AI_RELATION_NAP = 1600.0;
		private const double AI_RELATION_NAP1 = 1800.0;
		private const double AI_RELATION_ALLY0 = 1800.0;
		private const double AI_RELATION_ALLY = 1900.0;
		private const double AI_RELATION_ALLY1 = 2000.0;
		private GameSession _game;
		private Player _player;
		private GameDatabase _db;
		private Random _random;
		private List<FleetInfo> m_AvailableFullFleets;
		private List<FleetInfo> m_AvailableShortFleets;
		private List<FleetInfo> m_DefenseCombatFleets;
		private List<FleetInfo> m_FleetsInSurveyRange;
		private FleetTemplate m_CombatFleetTemplate;
		private List<StrategicTask> m_AvailableTasks;
		private Dictionary<StrategicTask, List<StrategicTask>> m_RelocationTasks;
		private Dictionary<int, int> m_FleetCubePoints;
		private List<SystemDefendInfo> m_ColonizedSystems;
		private Budget m_TurnBudget;
		private int[] m_NumStations;
		private int m_GateCapacity;
		private int m_LoaLimit;
		private bool m_IsOldSave;
		private readonly int MAX_BUILD_TURNS = 5;
		private readonly AITechStyles _techStyles;
		private readonly MissionManager _missionMan;
		private int _dropInActivationTurn;
		public GameSession Game
		{
			get
			{
				return this._game;
			}
		}
		public Player Player
		{
			get
			{
				return this._player;
			}
		}
		public Random Random
		{
			get
			{
				return this._random;
			}
		}
		public AITechStyles TechStyles
		{
			get
			{
				return this._techStyles;
			}
		}
		public AIStance? LastStance
		{
			get;
			private set;
		}
		public int CachedAvailableResearchPointsPerTurn
		{
			get;
			private set;
		}
		public StrategicAI(GameSession game, Player player)
		{
			this._missionMan = new MissionManager(this);
			this._game = game;
			this._player = player;
			this._db = game.GameDatabase;
			this._random = new Random();
			this.m_IsOldSave = (this._db.GetUnixTimeCreated() == 0.0);
			this.m_AvailableFullFleets = new List<FleetInfo>();
			this.m_AvailableShortFleets = new List<FleetInfo>();
			this.m_DefenseCombatFleets = new List<FleetInfo>();
			this.m_FleetsInSurveyRange = new List<FleetInfo>();
			this.m_AvailableTasks = new List<StrategicTask>();
			this.m_RelocationTasks = new Dictionary<StrategicTask, List<StrategicTask>>();
			this.m_ColonizedSystems = new List<SystemDefendInfo>();
			this.m_FleetCubePoints = new Dictionary<int, int>();
			this.m_NumStations = new int[8];
			this.m_GateCapacity = 0;
			this.m_LoaLimit = 0;
			this.m_CombatFleetTemplate = game.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == "DEFAULT_COMBAT");
			if (player.ID != 0)
			{
				this._db.InsertOrIgnoreAI(player.ID, AIStance.EXPANDING);
				AIInfo aIInfo = this._db.GetAIInfo(player.ID);
				if ((aIInfo.Flags & AIInfoFlags.TechStylesGenerated) == (AIInfoFlags)0)
				{
					this._techStyles = new AITechStyles(this._game.AssetDatabase, game.AssetDatabase.AIResearchFramework.AISelectTechStyles(this, player.Faction));
					foreach (AITechStyleInfo current in this._techStyles.TechStyleInfos)
					{
						this._db.InsertAITechStyle(current);
					}
					aIInfo.Flags |= AIInfoFlags.TechStylesGenerated;
					this._db.UpdateAIInfo(aIInfo);
					return;
				}
				this._techStyles = new AITechStyles(this._game.AssetDatabase, this._db.GetAITechStyles(player.ID).ToList<AITechStyleInfo>());
			}
		}
		public static void InitializeResearch(Random rand, AssetDatabase assetdb, GameDatabase db, int playerID)
		{
			foreach (TechFamily current in assetdb.MasterTechTree.TechFamilies)
			{
				float num = rand.NextSingle() / 5f;
				float weight = 0.4f + num;
				db.InsertAITechWeight(playerID, current.Id, weight);
			}
		}
		public void SetDropInActivationTurn(int turn)
		{
			this._dropInActivationTurn = turn;
		}
		public void Update(StrategicAI.UpdateInfo updateInfo)
		{
			AIInfo aIInfo = this._db.GetAIInfo(this._player.ID);
			StrategicAI.TraceVerbose(string.Format("---- Processing AI for player {0} ----", aIInfo.PlayerInfo));
			if (this._db.GetTurnCount() < this._dropInActivationTurn)
			{
				StrategicAI.TraceVerbose(string.Format("AI processing skipped; drop-in moratorium is in place until turn {0}.", this._dropInActivationTurn));
				return;
			}
			if (aIInfo.PlayerInfo.isDefeated || (!aIInfo.PlayerInfo.isStandardPlayer && !aIInfo.PlayerInfo.isAIRebellionPlayer))
			{
				StrategicAI.TraceVerbose("AI processing skipped; player is exempt.");
				return;
			}
			this.LastStance = new AIStance?(aIInfo.Stance);
			if (this.m_IsOldSave)
			{
				this._missionMan.Update();
			}
			this.ResetData();
			double startOfTurnSavings;
			this.UpdateEmpire(aIInfo, out startOfTurnSavings);
			this.UpdateStance(aIInfo, updateInfo);
			this.ManageColonies(aIInfo);
			this.DesignShips(aIInfo);
			this.DoRepairs(aIInfo);
			this.ManageStations(aIInfo);
			if (this.m_IsOldSave)
			{
				this.ManageFleets(aIInfo);
				this.ManageDefenses(aIInfo);
				this.ManageReserves(aIInfo);
				this.SetFleetOrders(aIInfo);
			}
			else
			{
				this.ManageDefenses(aIInfo);
			}
			this.SetResearchOrders(aIInfo);
			this.UpdateRelations(aIInfo, updateInfo);
			this.OfferTreaties(aIInfo, updateInfo);
			this.FinalizeEmpire(aIInfo, startOfTurnSavings);
			if (!this.m_IsOldSave)
			{
				this.GatherAllResources(aIInfo.Stance);
				this.GatherAllTasks();
				this.ApplyScores(aIInfo.Stance);
				this.AssignFleetsToTasks(aIInfo.Stance);
				this.BuildFleets(aIInfo);
				this.ManageDebtLevels();
			}
			if (this._player.Faction.Name == "loa")
			{
				List<FleetInfo> source = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				List<LoaFleetComposition> list = (
					from x in this._db.GetLoaFleetCompositions()
					where x.PlayerID == this._player.ID
					select x).ToList<LoaFleetComposition>();
				foreach (LoaFleetComposition comp in list)
				{
					if (!source.Any((FleetInfo x) => x.FleetConfigID.HasValue && x.FleetConfigID.Value == comp.ID))
					{
						this._db.DeleteLoaFleetCompositon(comp.ID);
					}
				}
			}
			if (App.Log.Level >= LogLevel.Verbose)
			{
				StringBuilder stringBuilder = new StringBuilder();
				DesignLab.PrintPlayerDesignSummary(stringBuilder, this.Game.App, this._player.ID, false);
				StrategicAI.TraceVerbose(stringBuilder.ToString());
			}
		}
		private void DoRepairs(AIInfo aiInfo)
		{
			foreach (StarSystemInfo current in this._db.GetStarSystemInfos())
			{
				this._game.RepairFleetsAtSystem(current.ID, this._player.ID);
			}
		}
		private void UpdateEmpire(AIInfo aiInfo, out double startOfTurnSavings)
		{
			PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
			startOfTurnSavings = playerInfo.Savings;
			if (this.m_IsOldSave && playerInfo.Savings < 100000.0)
			{
				this._db.UpdatePlayerSavings(this._player.ID, 100000.0);
			}
			this.CachedAvailableResearchPointsPerTurn = this.Game.GetAvailableResearchPoints(playerInfo);
			if (this._db.GetSliderNotchSettingInfo(this._player.ID, UISlidertype.SecuritySlider) == null)
			{
				this._db.InsertUISliderNotchSetting(this._player.ID, UISlidertype.SecuritySlider, 0.0, 0);
			}
			playerInfo = this._db.GetPlayerInfo(this._player.ID);
			playerInfo.RateGovernmentSavings = 1f;
			playerInfo.RateGovernmentSecurity = 1f;
			playerInfo.RateGovernmentStimulus = 1f;
			playerInfo.RateStimulusColonization = 0f;
			playerInfo.RateStimulusMining = (float)(this._db.PlayerHasTech(this._player.ID, "IND_Mega-Strip_Mining") ? 1 : 0);
			playerInfo.RateStimulusTrade = (float)(this._db.GetStratModifier<bool>(StratModifiers.EnableTrade, this._player.ID) ? 1 : 0);
			playerInfo.RateSecurityCounterIntelligence = 1f;
			playerInfo.RateSecurityIntelligence = 1f;
			playerInfo.RateSecurityOperations = 0f;
			playerInfo.RateResearchCurrentProject = 1f;
			playerInfo.RateResearchSalvageResearch = 0.1f;
			playerInfo.RateResearchSpecialProject = 0.1f;
			playerInfo.NormalizeRates();
			this._db.UpdatePlayerSliders(this._game, playerInfo);
		}
		private void UpdateRelations(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo)
		{
			if (aiInfo.Stance == AIStance.DEFENDING)
			{
				List<CombatData> source = this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 1).ToList<CombatData>();
				foreach (StrategicAI.UpdatePlayerInfo updatePlayer in updateInfo.Players.Values)
				{
					DiplomacyInfo diplomacyInfo = updatePlayer.Relations[this._player.ID];
					if (diplomacyInfo.State == DiplomacyState.WAR)
					{
						if (!source.Any((CombatData x) => x.GetPlayers().Any((PlayerCombatData y) => y.PlayerID == updatePlayer.Player.ID)))
						{
							diplomacyInfo.Relations += 100;
							this._db.UpdateDiplomacyInfo(diplomacyInfo);
						}
					}
				}
			}
		}
		private void FinalizeEmpire(AIInfo aiInfo, double startOfTurnSavings)
		{
			PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
			bool flag = false;
			if (startOfTurnSavings > 0.0 && playerInfo.Savings > 0.0)
			{
				float num = 1f;
				if (playerInfo.Savings < 1500000.0)
				{
					num = 0.25f;
				}
				float num2 = playerInfo.GetResearchRate();
				float num3 = this._player.Faction.GetAIResearchRate(aiInfo.Stance) * num;
				if (playerInfo.Savings > 10000000.0)
				{
					num3 = 0.9f;
				}
				if (num2 < num3)
				{
					num2 = Math.Min(num2 + 0.1f, num3);
					playerInfo.SetResearchRate(num2);
					flag = true;
				}
				else
				{
					if (num2 > num3)
					{
						num2 = Math.Max(num2 - 0.1f, num3);
						playerInfo.SetResearchRate(num2);
						flag = true;
					}
				}
			}
			else
			{
				float num4 = 0.3f;
				if (this._player.Faction.Name == "loa" && playerInfo.Savings < 0.0)
				{
					num4 = 0f;
				}
				if ((this._player.Faction.Name == "hiver" && playerInfo.Savings < -100000.0) || playerInfo.Savings < -500000.0)
				{
					num4 *= 0.5f;
				}
				if ((this._player.Faction.Name == "hiver" && playerInfo.Savings < -250000.0) || playerInfo.Savings < -1000000.0)
				{
					num4 = 0f;
				}
				if (playerInfo.GetResearchRate() > num4)
				{
					playerInfo.SetResearchRate(num4);
					flag = true;
				}
			}
			if (flag)
			{
				this._game.GameDatabase.UpdatePlayerSliders(this._game, playerInfo);
			}
		}
		public static int GetDiplomacyStateRank(DiplomacyState value)
		{
			switch (value)
			{
			case DiplomacyState.CEASE_FIRE:
				return 5;
			case DiplomacyState.UNKNOWN:
				return 1;
			case DiplomacyState.NON_AGGRESSION:
				return 6;
			case DiplomacyState.WAR:
				return 2;
			case DiplomacyState.ALLIED:
				return 7;
			case DiplomacyState.NEUTRAL:
				return 3;
			case DiplomacyState.PEACE:
				return 4;
			default:
				return 1;
			}
		}
		private static double LerpRange(double t, double t0, double x0, double t1, double x1)
		{
			return ScalarExtensions.Lerp(x0, x1, (t - t0) / (t1 - t0));
		}
		private DiplomacyState EvolvePreferredDiplomacyState(DiplomacyState current, double relation)
		{
			DiplomacyState diplomacyState = current;
			double odds;
			if (relation >= 0.0 && relation <= 300.0)
			{
				odds = 1.0;
				diplomacyState = DiplomacyState.WAR;
			}
			else
			{
				if (relation >= 300.0 && relation <= 750.0)
				{
					odds = StrategicAI.LerpRange(relation, 300.0, 1.0, 750.0, 0.0);
					diplomacyState = DiplomacyState.WAR;
				}
				else
				{
					if (relation >= 750.0 && relation <= 925.0)
					{
						odds = StrategicAI.LerpRange(relation, 750.0, 0.0, 925.0, 1.0);
						diplomacyState = DiplomacyState.NEUTRAL;
					}
					else
					{
						if (relation >= 925.0 && relation <= 1100.0)
						{
							odds = StrategicAI.LerpRange(relation, 925.0, 1.0, 1100.0, 0.0);
							diplomacyState = DiplomacyState.NEUTRAL;
						}
						else
						{
							if (relation >= 1100.0 && relation <= 1250.0)
							{
								odds = StrategicAI.LerpRange(relation, 1100.0, 0.0, 1250.0, 1.0);
								diplomacyState = DiplomacyState.CEASE_FIRE;
							}
							else
							{
								if (relation >= 1250.0 && relation <= 1400.0)
								{
									odds = StrategicAI.LerpRange(relation, 1250.0, 1.0, 1400.0, 0.0);
									diplomacyState = DiplomacyState.CEASE_FIRE;
								}
								else
								{
									if (relation >= 1400.0 && relation <= 1600.0)
									{
										odds = StrategicAI.LerpRange(relation, 1400.0, 0.0, 1600.0, 1.0);
										diplomacyState = DiplomacyState.NON_AGGRESSION;
									}
									else
									{
										if (relation >= 1600.0 && relation <= 1800.0)
										{
											odds = StrategicAI.LerpRange(relation, 1600.0, 1.0, 1800.0, 0.0);
											diplomacyState = DiplomacyState.NON_AGGRESSION;
										}
										else
										{
											if (relation >= 1800.0 && relation <= 2000.0)
											{
												odds = StrategicAI.LerpRange(relation, 1800.0, 0.0, 2000.0, 1.0);
												diplomacyState = DiplomacyState.ALLIED;
											}
											else
											{
												odds = 0.0;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			DiplomacyState result = current;
			if (this._random.CoinToss(odds))
			{
				result = diplomacyState;
			}
			return result;
		}
		private DiplomacyState GetPreferredDiplomacyState(AIInfo aiInfo, StrategicAI.UpdatePlayerInfo updatePlayerInfo, DiplomacyInfo di)
		{
			return this.EvolvePreferredDiplomacyState(di.State, (double)di.Relations);
		}
		private void TryDiplomacyStateAction(AIInfo aiInfo, StrategicAI.UpdatePlayerInfo updatePlayerInfo, DiplomacyInfo di)
		{
			if (!di.isEncountered || updatePlayerInfo.Player.isDefeated || !updatePlayerInfo.Player.isStandardPlayer || aiInfo.PlayerID == di.TowardsPlayerID)
			{
				return;
			}
			DiplomacyState preferredDiplomacyState = this.GetPreferredDiplomacyState(aiInfo, updatePlayerInfo, di);
			if (preferredDiplomacyState == DiplomacyState.WAR && di.State != preferredDiplomacyState)
			{
				PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
				PlayerInfo playerInfo2 = this._db.GetPlayerInfo(di.TowardsPlayerID);
				if (this._game.CanPerformDiplomacyAction(playerInfo, playerInfo2, DiplomacyAction.DECLARATION, null, null))
				{
					int? diplomacyActionCost = this._game.GetDiplomacyActionCost(DiplomacyAction.DECLARATION, null, null);
					this._db.SpendDiplomacyPoints(playerInfo, playerInfo2.FactionID, diplomacyActionCost.Value);
					this._game.DeclareWarFormally(this._player.ID, di.TowardsPlayerID);
					return;
				}
			}
			else
			{
				if (StrategicAI.GetDiplomacyStateRank(preferredDiplomacyState) > StrategicAI.GetDiplomacyStateRank(di.State))
				{
					ArmisticeTreatyInfo armisticeTreatyInfo = new ArmisticeTreatyInfo();
					armisticeTreatyInfo.Active = false;
					armisticeTreatyInfo.Removed = false;
					armisticeTreatyInfo.InitiatingPlayerId = aiInfo.PlayerID;
					armisticeTreatyInfo.ReceivingPlayerId = di.TowardsPlayerID;
					armisticeTreatyInfo.StartingTurn = this._db.GetTurnCount();
					armisticeTreatyInfo.SuggestedDiplomacyState = preferredDiplomacyState;
					armisticeTreatyInfo.Type = TreatyType.Armistice;
					if (this._game.CanPerformTreaty(armisticeTreatyInfo))
					{
						PlayerInfo playerInfo3 = this._db.GetPlayerInfo(armisticeTreatyInfo.InitiatingPlayerId);
						PlayerInfo playerInfo4 = this._db.GetPlayerInfo(armisticeTreatyInfo.ReceivingPlayerId);
						int treatyRdpCost = this._game.GetTreatyRdpCost(armisticeTreatyInfo);
						this._db.SpendDiplomacyPoints(playerInfo3, playerInfo4.FactionID, treatyRdpCost);
						this._db.InsertTreaty(armisticeTreatyInfo);
					}
				}
			}
		}
		private void OfferTreaties(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo)
		{
			StrategicAI.UpdatePlayerInfo updatePlayerInfo = updateInfo.Players[aiInfo.PlayerID];
			if (App.Log.Level >= LogLevel.Verbose)
			{
				StrategicAI.TraceVerbose(string.Format("Relations for {0} vs...", updatePlayerInfo.Player));
				foreach (StrategicAI.UpdatePlayerInfo current in updateInfo.Players.Values)
				{
					DiplomacyInfo diplomacyInfo = current.Relations[updatePlayerInfo.Player.ID];
					StrategicAI.TraceVerbose(string.Format("   {0}: {1} ({2}), {3}", new object[]
					{
						current.Player,
						diplomacyInfo.GetDiplomaticMood().ToString(),
						diplomacyInfo.Relations,
						diplomacyInfo.State.ToString()
					}));
				}
			}
			int turnCount = this._db.GetTurnCount();
			foreach (DiplomacyInfo current2 in updatePlayerInfo.Relations.Values)
			{
				if (current2.TowardsPlayerID != updatePlayerInfo.Player.ID && updateInfo.Players[current2.TowardsPlayerID].Player.isStandardPlayer && current2.isEncountered)
				{
					if (turnCount >= 5 && (turnCount + aiInfo.PlayerID) % 5 == 0)
					{
						this.TryDiplomacyStateAction(aiInfo, updatePlayerInfo, current2);
					}
					if (turnCount >= 10 && this._random.CoinToss(10))
					{
						this.TryDiplomacyAction(aiInfo, updateInfo, current2);
					}
				}
			}
		}
		private void TryDiplomacyAction(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo, DiplomacyInfo di)
		{
			Array values = Enum.GetValues(typeof(DiplomacyAction));
			Dictionary<object, float> dictionary = new Dictionary<object, float>();
			IEnumerable<DiplomacyActionHistoryEntryInfo> diplomacyActionHistory = this._db.GetDiplomacyActionHistory(aiInfo.PlayerID, di.TowardsPlayerID, this._db.GetTurnCount(), 20);
			DiplomaticMood diplomaticMood = di.GetDiplomaticMood();
			foreach (object current in values)
			{
				DiplomacyAction action = (DiplomacyAction)current;
				Dictionary<object, float> weights = this._player.Faction.DiplomacyWeights.GetWeights(action, diplomaticMood);
				foreach (KeyValuePair<object, float> current2 in weights)
				{
					float diplomacyActionViability = this.GetDiplomacyActionViability(aiInfo, updateInfo, diplomacyActionHistory, di, action, current2.Key);
					if (diplomacyActionViability != 0f)
					{
						dictionary.Add(current2.Key, current2.Value * diplomacyActionViability);
					}
				}
			}
			if (dictionary.Count > 0)
			{
				IEnumerable<Weighted<object>> weights2 = 
					from x in dictionary
					select new Weighted<object>(x.Key, (int)x.Value);
				object obj = WeightedChoices.Choose<object>(this._random, weights2);
				this.ExecuteDiplomacyAction(aiInfo, updateInfo, di, DiplomacyActionWeights.GetActionFromType(obj.GetType()), obj);
			}
		}
        private void ExecuteDiplomacyAction(AIInfo aiInfo, UpdateInfo updateInfo, DiplomacyInfo di, DiplomacyAction action, object type)
        {
            RequestInfo info;
            Func<StarSystemInfo, bool> predicate = null;
            Func<StarSystemInfo, bool> func4 = null;
            Func<StarSystemInfo, bool> func5 = null;
            Func<StarSystemInfo, bool> func6 = null;
            Func<StarSystemInfo, bool> func7 = null;
            Func<StarSystemInfo, bool> func8 = null;
            TraceVerbose(string.Concat(new object[] { "P", aiInfo.PlayerID, " executing diplomacy action ", action.ToString(), " towards P", di.TowardsPlayerID.ToString() }));
            switch (action)
            {
                case DiplomacyAction.REQUEST:
                    info = new RequestInfo
                    {
                        InitiatingPlayer = aiInfo.PlayerID,
                        ReceivingPlayer = di.TowardsPlayerID,
                        Type = (RequestType)type
                    };
                    switch (info.Type)
                    {
                        case RequestType.SavingsRequest:
                            info.RequestValue = (float)((DiplomaticMood)(100000 * (int)di.GetDiplomaticMood()));
                            goto Label_020C;

                        case RequestType.SystemInfoRequest:
                            {
                                if (predicate == null)
                                {
                                    predicate = delegate(StarSystemInfo x)
                                    {
                                        int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
                                        int towardsPlayerID = di.TowardsPlayerID;
                                        return (systemOwningPlayer.GetValueOrDefault() == towardsPlayerID) && systemOwningPlayer.HasValue;
                                    };
                                }
                                if (func4 == null)
                                {
                                    func4 = x => !this._db.IsSurveyed(aiInfo.PlayerID, x.ID);
                                }
                                StarSystemInfo info2 = this._db.GetStarSystemInfos().Where<StarSystemInfo>(predicate).FirstOrDefault<StarSystemInfo>(func4);
                                if (info2 == null)
                                {
                                    return;
                                }
                                info.RequestValue = info2.ID;
                                goto Label_020C;
                            }
                        case RequestType.ResearchPointsRequest:
                            info.RequestValue = (float)((DiplomaticMood)(50000 * (int)di.GetDiplomaticMood()));
                            goto Label_020C;

                        case RequestType.GatePermissionRequest:
                            {
                                if (func5 == null)
                                {
                                    func5 = delegate(StarSystemInfo x)
                                    {
                                        int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
                                        int towardsPlayerID = di.TowardsPlayerID;
                                        return (systemOwningPlayer.GetValueOrDefault() == towardsPlayerID) && systemOwningPlayer.HasValue;
                                    };
                                }
                                if (func6 == null)
                                {
                                    func6 = x => this._db.GetHiverGateForSystem(x.ID, aiInfo.PlayerID) == null;
                                }
                                StarSystemInfo info3 = this._db.GetStarSystemInfos().Where<StarSystemInfo>(func5).FirstOrDefault<StarSystemInfo>(func6);
                                if (info3 != null)
                                {
                                    info.RequestValue = info3.ID;
                                }
                                else
                                {
                                    return;
                                }
                                goto Label_020C;
                            }
                    }
                    break;

                case DiplomacyAction.DEMAND:
                    {
                        DemandInfo demand = new DemandInfo
                        {
                            InitiatingPlayer = aiInfo.PlayerID,
                            ReceivingPlayer = di.TowardsPlayerID,
                            Type = (DemandType)type
                        };
                        switch (demand.Type)
                        {
                            case DemandType.SavingsDemand:
                                demand.DemandValue = (100000 * (4 - (int)di.GetDiplomaticMood()));
                                break;

                            case DemandType.SystemInfoDemand:
                                {
                                    if (func7 == null)
                                    {
                                        func7 = delegate(StarSystemInfo x)
                                        {
                                            int? systemOwningPlayer = this._db.GetSystemOwningPlayer(x.ID);
                                            int towardsPlayerID = di.TowardsPlayerID;
                                            return (systemOwningPlayer.GetValueOrDefault() == towardsPlayerID) && systemOwningPlayer.HasValue;
                                        };
                                    }
                                    if (func8 == null)
                                    {
                                        func8 = x => !this._db.IsSurveyed(aiInfo.PlayerID, x.ID);
                                    }
                                    StarSystemInfo info5 = this._db.GetStarSystemInfos().Where<StarSystemInfo>(func7).FirstOrDefault<StarSystemInfo>(func8);
                                    if (info5 == null)
                                    {
                                        return;
                                    }
                                    demand.DemandValue = info5.ID;
                                    break;
                                }
                            case DemandType.ResearchPointsDemand:
                                demand.DemandValue = 50000 * (4 - (int)di.GetDiplomaticMood());
                                break;
                        }
                        this._db.SpendDiplomacyPoints(aiInfo.PlayerInfo, updateInfo.Players[di.TowardsPlayerID].Player.FactionID, this._game.AssetDatabase.GetDiplomaticDemandPointCost(demand.Type));
                        this._db.InsertDemand(demand);
                        return;
                    }
                case DiplomacyAction.TREATY:
                    return;

                case DiplomacyAction.LOBBY:
                    {
                        Func<KeyValuePair<int, UpdatePlayerInfo>, bool> func = null;
                        Func<KeyValuePair<int, UpdatePlayerInfo>, bool> func2 = null;
                        UpdatePlayerInfo updatePlayer = updateInfo.Players[aiInfo.PlayerID];
                        List<KeyValuePair<int, UpdatePlayerInfo>> source = (from x in updateInfo.Players
                                                                            where (x.Value.Player.isStandardPlayer && updatePlayer.Relations[x.Key].isEncountered) && (x.Key != di.TowardsPlayerID)
                                                                            select x).ToList<KeyValuePair<int, UpdatePlayerInfo>>();
                        switch (((LobbyType)type))
                        {
                            case LobbyType.LobbySelf:
                                this._game.DoLobbyAction(aiInfo.PlayerID, di.TowardsPlayerID, aiInfo.PlayerID, false);
                                return;

                            case LobbyType.LobbyEnemy:
                                {
                                    if (func == null)
                                    {
                                        func = x => GetDiplomacyStateRank(updatePlayer.Relations[x.Key].State) < GetDiplomacyStateRank(DiplomacyState.NEUTRAL);
                                    }
                                    KeyValuePair<int, UpdatePlayerInfo> pair = source.First<KeyValuePair<int, UpdatePlayerInfo>>(func);
                                    this._game.DoLobbyAction(aiInfo.PlayerID, di.TowardsPlayerID, pair.Key, false);
                                    return;
                                }
                            case LobbyType.LobbyFriendly:
                                {
                                    if (func2 == null)
                                    {
                                        func2 = x => GetDiplomacyStateRank(updatePlayer.Relations[x.Key].State) > GetDiplomacyStateRank(DiplomacyState.NEUTRAL);
                                    }
                                    KeyValuePair<int, UpdatePlayerInfo> pair2 = source.First<KeyValuePair<int, UpdatePlayerInfo>>(func2);
                                    this._game.DoLobbyAction(aiInfo.PlayerID, di.TowardsPlayerID, pair2.Key, true);
                                    return;
                                }
                        }
                        return;
                    }
                default:
                    return;
            }
        Label_020C:
            this._db.SpendDiplomacyPoints(aiInfo.PlayerInfo, updateInfo.Players[di.TowardsPlayerID].Player.FactionID, this._game.AssetDatabase.GetDiplomaticRequestPointCost(info.Type));
            this._db.InsertRequest(info);
        }
        private float GetDiplomacyActionViability(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo, IEnumerable<DiplomacyActionHistoryEntryInfo> history, DiplomacyInfo di, DiplomacyAction action, object type)
		{
			if (history.FirstOrDefault((DiplomacyActionHistoryEntryInfo x) => x.Action == action) != null)
			{
				return 0f;
			}
			float result = 1f;
			switch (action)
			{
			case DiplomacyAction.DECLARATION:
				result = 0f;
				break;
			case DiplomacyAction.REQUEST:
				result = this.GetRequestViability(aiInfo, updateInfo, di, type);
				break;
			case DiplomacyAction.DEMAND:
				result = this.GetDemandViability(aiInfo, updateInfo, di, type);
				break;
			case DiplomacyAction.TREATY:
				if (type.GetType() == typeof(LimitationTreatyType))
				{
					result = 0f;
				}
				else
				{
					if (type.GetType() == typeof(TreatyType))
					{
						TreatyType treatyType = (TreatyType)type;
						if (treatyType == TreatyType.Limitation)
						{
							result = 0f;
						}
						else
						{
							result = 0f;
						}
					}
				}
				break;
			case DiplomacyAction.LOBBY:
				result = this.GetLobbyViability(aiInfo, updateInfo, di, type);
				break;
			case DiplomacyAction.SPIN:
				result = 0f;
				break;
			case DiplomacyAction.SURPRISEATTACK:
				result = 0f;
				break;
			case DiplomacyAction.GIVE:
				result = 0f;
				break;
			}
			return result;
		}
		private float GetRequestViability(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo, DiplomacyInfo di, object type)
		{
			if (type.GetType() != typeof(RequestType))
			{
				return 0f;
			}
			RequestType rt = (RequestType)type;
			DiplomaticMood diplomaticMood = di.GetDiplomaticMood();
			if (diplomaticMood <= DiplomaticMood.Indifference || aiInfo.PlayerInfo.GetTotalDiplomacyPoints(updateInfo.Players[di.TowardsPlayerID].Player.FactionID) < this._game.AssetDatabase.GetDiplomaticRequestPointCost(rt))
			{
				return 0f;
			}
			float result = 1f;
			switch (rt)
			{
			case RequestType.SavingsRequest:
				if (aiInfo.PlayerInfo.Savings < 0.0)
				{
					result = 4f;
				}
				else
				{
					if (aiInfo.PlayerInfo.Savings < 1000000.0)
					{
						result = 2f;
					}
				}
				break;
			case RequestType.SystemInfoRequest:
			{
				IEnumerable<StarSystemInfo> source = 
					from x in this._db.GetStarSystemInfos()
					where this._db.GetSystemOwningPlayer(x.ID) == di.TowardsPlayerID
					select x;
				StarSystemInfo s = source.FirstOrDefault((StarSystemInfo x) => !this._db.IsSurveyed(aiInfo.PlayerID, x.ID));
				if (s != null)
				{
					result = 2f;
				}
				break;
			}
			case RequestType.ResearchPointsRequest:
				if (aiInfo.PlayerInfo.RateResearchCurrentProject > 0f)
				{
					result = 1.5f;
				}
				else
				{
					result = 0f;
				}
				break;
			case RequestType.MilitaryAssistanceRequest:
				result = 0f;
				break;
			case RequestType.GatePermissionRequest:
				if (this._player.Faction.Name != "Hiver")
				{
					result = 0f;
				}
				else
				{
					IEnumerable<StarSystemInfo> source2 = 
						from x in this._db.GetStarSystemInfos()
						where this._db.GetSystemOwningPlayer(x.ID) == di.TowardsPlayerID
						select x;
					StarSystemInfo s2 = source2.FirstOrDefault((StarSystemInfo x) => this._db.GetHiverGateForSystem(x.ID, aiInfo.PlayerID) == null);
					if (s2 != null)
					{
						result = 1f;
					}
				}
				break;
			case RequestType.EstablishEnclaveRequest:
				result = 0f;
				break;
			}
			return result;
		}
		private float GetLobbyViability(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo, DiplomacyInfo di, object type)
		{
			if (type.GetType() != typeof(LobbyType) || !this._game.GetPlayerObject(di.TowardsPlayerID).IsAI())
			{
				return 0f;
			}
			float result = 1f;
			StrategicAI.UpdatePlayerInfo updatePlayer = updateInfo.Players[aiInfo.PlayerID];
			IEnumerable<KeyValuePair<int, StrategicAI.UpdatePlayerInfo>> source = 
				from x in updateInfo.Players
				where x.Value.Player.isStandardPlayer && updatePlayer.Relations[x.Key].isEncountered && x.Key != di.TowardsPlayerID
				select x;
			switch ((LobbyType)type)
			{
			case LobbyType.LobbyEnemy:
				if (!source.Any((KeyValuePair<int, StrategicAI.UpdatePlayerInfo> x) => StrategicAI.GetDiplomacyStateRank(updatePlayer.Relations[x.Key].State) < StrategicAI.GetDiplomacyStateRank(DiplomacyState.NEUTRAL)))
				{
					result = 0f;
				}
				break;
			case LobbyType.LobbyFriendly:
				if (!source.Any((KeyValuePair<int, StrategicAI.UpdatePlayerInfo> x) => StrategicAI.GetDiplomacyStateRank(updatePlayer.Relations[x.Key].State) > StrategicAI.GetDiplomacyStateRank(DiplomacyState.NEUTRAL)))
				{
					result = 0f;
				}
				break;
			}
			return result;
		}
		private float GetDemandViability(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo, DiplomacyInfo di, object type)
		{
			if (type.GetType() != typeof(DemandType))
			{
				return 0f;
			}
			DemandType dt = (DemandType)type;
			DiplomaticMood diplomaticMood = di.GetDiplomaticMood();
			if (diplomaticMood > DiplomaticMood.Indifference || aiInfo.PlayerInfo.GetTotalDiplomacyPoints(updateInfo.Players[di.TowardsPlayerID].Player.FactionID) < this._game.AssetDatabase.GetDiplomaticDemandPointCost(dt))
			{
				return 0f;
			}
			float result = 1f;
			switch (dt)
			{
			case DemandType.SavingsDemand:
				if (aiInfo.PlayerInfo.Savings < 0.0)
				{
					result = 4f;
				}
				else
				{
					if (aiInfo.PlayerInfo.Savings < 1000000.0)
					{
						result = 2f;
					}
				}
				break;
			case DemandType.SystemInfoDemand:
			{
				IEnumerable<StarSystemInfo> source = 
					from x in this._db.GetStarSystemInfos()
					where this._db.GetSystemOwningPlayer(x.ID) == di.TowardsPlayerID
					select x;
				StarSystemInfo s = source.FirstOrDefault((StarSystemInfo x) => this._db.IsSurveyed(aiInfo.PlayerID, x.ID));
				if (s != null)
				{
					result = 2f;
				}
				break;
			}
			case DemandType.ResearchPointsDemand:
				if (aiInfo.PlayerInfo.RateResearchCurrentProject > 0f)
				{
					result = 1.5f;
				}
				else
				{
					result = 0f;
				}
				break;
			case DemandType.SlavesDemand:
				result = 0f;
				break;
			case DemandType.WorldDemand:
				result = 0f;
				break;
			case DemandType.ProvinceDemand:
				result = 0f;
				break;
			case DemandType.SurrenderDemand:
				result = 0f;
				break;
			}
			return result;
		}
		private void UpdateStance(AIInfo aiInfo, StrategicAI.UpdateInfo updateInfo)
		{
			if (this._db == null)
			{
				this._db = this._game.GameDatabase;
			}
			AIStance stance = aiInfo.Stance;
			float num = this.AssessExpansionRoom();
			float num2 = this.AssessOwnStrength();
			float num3 = 300f;
			int num4 = this.AssessOwnTechLevel();
			int num5 = 0;
			List<int> list = new List<int>();
			foreach (PlayerInfo current in this._db.GetPlayerInfos())
			{
				if (current.ID != this._player.ID && !current.isDefeated && (current.isStandardPlayer || current.isAIRebellionPlayer))
				{
					if (this._db.GetDiplomacyStateBetweenPlayers(current.ID, this._player.ID) == DiplomacyState.WAR && this.AssessPlayerStrength(current.ID) > 0f)
					{
						list.Add(current.ID);
					}
					int num6 = this.AssessPlayerTechLevel(current.ID);
					if (num6 > num5)
					{
						num5 = num6;
					}
				}
			}
			if (list.Count<int>() > 0)
			{
				float num7 = 0f;
				int num8 = 0;
				int num9 = 0;
				float num10 = 0f;
				float num11 = 0f;
				foreach (int current2 in list)
				{
					float num12 = this.AssessPlayerStrength(current2);
					num7 += num12;
					if (num8 == 0 || num12 < num10)
					{
						num8 = current2;
						num10 = num12;
					}
					if (num9 == 0 || num12 > num11)
					{
						num9 = current2;
						num11 = num12;
					}
				}
				if (num2 > num10 * 2f)
				{
					aiInfo.Stance = AIStance.CONQUERING;
				}
				else
				{
					if (num2 >= num11)
					{
						aiInfo.Stance = AIStance.DESTROYING;
					}
					else
					{
						aiInfo.Stance = AIStance.DEFENDING;
					}
				}
			}
			else
			{
				if (num2 > num3)
				{
					int num13 = this.PickAFight();
					if (num13 != 0)
					{
						this._game.DeclareWarFormally(this._player.ID, num13);
						aiInfo.Stance = AIStance.CONQUERING;
					}
				}
				else
				{
					if (num > 1f)
					{
						aiInfo.Stance = AIStance.EXPANDING;
					}
					else
					{
						if (num4 < num5)
						{
							aiInfo.Stance = AIStance.HUNKERING;
						}
						else
						{
							if (num2 < num3)
							{
								aiInfo.Stance = AIStance.ARMING;
							}
						}
					}
				}
			}
			if (aiInfo.Stance != stance)
			{
				StrategicAI.TraceVerbose("Setting AI stance to: " + aiInfo.Stance);
				this._db.UpdateAIInfo(aiInfo);
			}
		}
		private void SetRequiredDefaultDesigns(AIInfo aiInfo)
		{
			ShipRole[] defaultAIShipRoles = this._player.Faction.DefaultAIShipRoles;
			for (int i = 0; i < defaultAIShipRoles.Length; i++)
			{
				ShipRole role = defaultAIShipRoles[i];
				DesignLab.SetDefaultDesign(this._game, role, null, this._player.ID, null, null, this._techStyles, new AIStance?(aiInfo.Stance));
			}
		}
		private void ManageStations(AIInfo aiInfo)
		{
			StationModuleQueue.UpdateStationMapsForFaction(this._player.Faction.Name);
			Dictionary<ModuleEnums.StationModuleType, int> dictionary = new Dictionary<ModuleEnums.StationModuleType, int>();
			foreach (StationInfo current in this._db.GetStationInfosByPlayerID(this._player.ID).ToList<StationInfo>())
			{
				if (!this._game.StationIsUpgradable(current) && !this._game.StationIsUpgrading(current))
				{
					StationModuleQueue.InitializeQueuedItemMap(this._game, current, dictionary);
					StationModuleQueue.AutoFillModules(this._game, current, dictionary);
					int num = dictionary.Values.Sum();
					if (num > 0)
					{
						StationModuleQueue.ConfirmStationQueuedItems(this._game, current, dictionary);
					}
				}
			}
		}
		public void DesignShips(AIInfo aiInfo)
		{
			StrategicAI.TraceVerbose("Designing ships...");
			this.SetRequiredDefaultDesigns(aiInfo);
			List<StrategicAI.DesignPriority> list = new List<StrategicAI.DesignPriority>();
			list.Add(new StrategicAI.DesignPriority
			{
				role = ShipRole.COMBAT,
				weight = 0.5f
			});
			list.Add(new StrategicAI.DesignPriority
			{
				role = ShipRole.POLICE,
				weight = 0.1f
			});
			list.Add(new StrategicAI.DesignPriority
			{
				role = ShipRole.PLATFORM,
				weight = 0.1f
			});
			if (this._player.Faction.Name == "zuul")
			{
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SCAVENGER,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SLAVEDISK,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER_BOARDING,
					weight = 0.15f
				});
			}
			else
			{
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER_BOARDING,
					weight = 0.05f
				});
			}
			if (this._player.Faction.Name == "morrigi")
			{
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.DRONE,
					weight = 0.15f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER_DRONE,
					weight = 0.2f
				});
			}
			else
			{
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.DRONE,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER_DRONE,
					weight = 0.1f
				});
			}
			if (this._db.GetDesignInfosForPlayer(this._player.ID).Any((DesignInfo x) => x.Role == ShipRole.CARRIER))
			{
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.BR_PATROL,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.BR_SCOUT,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.BR_SPINAL,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.BR_ESCORT,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.BR_INTERCEPTOR,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.BR_TORPEDO,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.BATTLECRUISER,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.BATTLESHIP,
					weight = 0.1f
				});
			}
			switch (aiInfo.Stance)
			{
			case AIStance.EXPANDING:
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COLONIZER,
					weight = 0.2f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COMMAND,
					weight = 0.4f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CONSTRUCTOR,
					weight = 0.2f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SUPPLY,
					weight = 0.2f
				});
				break;
			case AIStance.ARMING:
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COMMAND,
					weight = 0.4f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SCOUT,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SUPPLY,
					weight = 0.2f
				});
				break;
			case AIStance.HUNKERING:
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COMMAND,
					weight = 0.4f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CONSTRUCTOR,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COMBAT,
					weight = 0.1f
				});
				break;
			case AIStance.CONQUERING:
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COMMAND,
					weight = 0.4f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SCOUT,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER_ASSAULT,
					weight = 0.2f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER_BIO,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SUPPLY,
					weight = 0.2f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COLONIZER,
					weight = 0.2f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.ASSAULTSHUTTLE,
					weight = 0.05f
				});
				break;
			case AIStance.DESTROYING:
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COMMAND,
					weight = 0.4f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SCOUT,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER_ASSAULT,
					weight = 0.2f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER_BIO,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SUPPLY,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COMBAT,
					weight = 0.1f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.ASSAULTSHUTTLE,
					weight = 0.05f
				});
				break;
			case AIStance.DEFENDING:
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.COMMAND,
					weight = 0.4f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.SCOUT,
					weight = 0.3f
				});
				list.Add(new StrategicAI.DesignPriority
				{
					role = ShipRole.CARRIER,
					weight = 0.6f
				});
				break;
			}
			if (App.Log.Level >= LogLevel.Verbose)
			{
				StrategicAI.TraceVerbose(string.Format("  Base design priorities for {0}...", aiInfo.Stance));
				foreach (StrategicAI.DesignPriority current in list)
				{
					StrategicAI.TraceVerbose(string.Format("    {0,15}: {1}", current.role, current.weight));
				}
			}
			this.UpdateDesigns(aiInfo, list);
		}
		private static bool DoesDesignModuleMatch(DesignModuleInfo a, DesignModuleInfo b)
		{
			return !(a.DesignID != b.DesignID) && a.ModuleID == b.ModuleID && !(a.WeaponID != b.WeaponID);
		}
		private static bool DoesWeaponBankMatch(WeaponBankInfo a, WeaponBankInfo b)
		{
			return !(a.DesignID != b.DesignID) && !(a.WeaponID != b.WeaponID);
		}
		private static bool DoesDesignSectionMatch(DesignSectionInfo a, DesignSectionInfo b)
		{
			if (a.ShipSectionAsset != b.ShipSectionAsset)
			{
				return false;
			}
			if (a.Modules.Count != b.Modules.Count)
			{
				return false;
			}
			if (a.Techs != b.Techs && ((a.Techs == null && b.Techs.Count == 0) || (b.Techs == null && a.Techs.Count == 0)))
			{
				return false;
			}
			if (a.Techs.Count != b.Techs.Count)
			{
				return false;
			}
			if (a.WeaponBanks.Count != b.WeaponBanks.Count)
			{
				return false;
			}
			for (int i = 0; i < a.Techs.Count; i++)
			{
				if (a.Techs[i] != b.Techs[i])
				{
					return false;
				}
			}
			for (int j = 0; j < a.Modules.Count; j++)
			{
				if (!StrategicAI.DoesDesignModuleMatch(a.Modules[j], b.Modules[j]))
				{
					return false;
				}
			}
			for (int k = 0; k < a.WeaponBanks.Count; k++)
			{
				if (!StrategicAI.DoesWeaponBankMatch(a.WeaponBanks[k], b.WeaponBanks[k]))
				{
					return false;
				}
			}
			return true;
		}
		private static bool DoesDesignMatch(DesignInfo a, DesignInfo b)
		{
			if (a.Role != b.Role)
			{
				return false;
			}
			if (a.Role != ShipRole.CONSTRUCTOR && a.Role != ShipRole.FREIGHTER && a.WeaponRole != b.WeaponRole)
			{
				return false;
			}
			if (a.Class != b.Class)
			{
				return false;
			}
			if (a.DesignSections.Length != b.DesignSections.Length)
			{
				return false;
			}
			for (int i = 0; i < a.DesignSections.Length; i++)
			{
				if (!StrategicAI.DoesDesignSectionMatch(a.DesignSections[i], b.DesignSections[i]))
				{
					return false;
				}
			}
			return true;
		}
		private static bool IsShipRoleEquivilant(ShipRole shipRole, ShipRole desiredRole)
		{
			if (desiredRole == shipRole)
			{
				return true;
			}
			if (shipRole == ShipRole.COMBAT)
			{
				if (desiredRole <= ShipRole.E_WARFARE)
				{
					if (desiredRole != ShipRole.CARRIER && desiredRole != ShipRole.E_WARFARE)
					{
						return false;
					}
				}
				else
				{
					if (desiredRole != ShipRole.SCAVENGER)
					{
						switch (desiredRole)
						{
						case ShipRole.CARRIER_ASSAULT:
						case ShipRole.CARRIER_DRONE:
						case ShipRole.CARRIER_BIO:
						case ShipRole.CARRIER_BOARDING:
							break;
						default:
							return false;
						}
					}
				}
				return true;
			}
			if (desiredRole == ShipRole.COMBAT)
			{
				if (shipRole <= ShipRole.E_WARFARE)
				{
					if (shipRole != ShipRole.CARRIER && shipRole != ShipRole.E_WARFARE)
					{
						return false;
					}
				}
				else
				{
					if (shipRole != ShipRole.SCAVENGER)
					{
						switch (shipRole)
						{
						case ShipRole.CARRIER_ASSAULT:
						case ShipRole.CARRIER_DRONE:
						case ShipRole.CARRIER_BIO:
						case ShipRole.CARRIER_BOARDING:
							break;
						default:
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}
		public static List<ShipRole> GetEquivilantShipRoles(ShipRole shipRole)
		{
			List<ShipRole> list = new List<ShipRole>();
			list.Add(shipRole);
			if (shipRole == ShipRole.COMBAT)
			{
				list.Add(ShipRole.CARRIER);
				list.Add(ShipRole.CARRIER_ASSAULT);
				list.Add(ShipRole.CARRIER_BIO);
				list.Add(ShipRole.CARRIER_BOARDING);
				list.Add(ShipRole.CARRIER_DRONE);
				list.Add(ShipRole.E_WARFARE);
			}
			return list;
		}
		private void UpdateDesigns(AIInfo aiInfo, List<StrategicAI.DesignPriority> unsortedPriorities)
		{
			StrategicAI.TraceVerbose("  Retiring obsolete designs...");
			foreach (DesignInfo current in 
				from x in this._db.GetVisibleDesignInfosForPlayer(this._player.ID)
				where x.Class != ShipClass.Station
				select x)
			{
                if (!Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._game.App, current) && StrategicAI.IsDesignObsolete(this._game, this._player.ID, current.ID))
				{
					if (App.Log.Level >= LogLevel.Verbose)
					{
						StrategicAI.TraceVerbose(string.Format("    Obsolete: {0}", current));
					}
					this._game.GameDatabase.RemovePlayerDesign(current.ID);
				}
			}
			List<StrategicAI.DesignPriority> list = (
				from x in unsortedPriorities
				orderby x.weight descending
				select x).ToList<StrategicAI.DesignPriority>();
			List<DesignInfo> list2 = new List<DesignInfo>();
			List<float> list3 = new List<float>();
			StrategicAI.TraceVerbose("  Collecting priority designs...");
			bool flag = false;
			foreach (StrategicAI.DesignPriority current2 in list)
			{
				int num = 0;
				List<DesignInfo> list4 = new List<DesignInfo>(this._db.GetVisibleDesignInfosForPlayerAndRole(this._player.ID, current2.role, true));
				int num2 = 0;
				foreach (DesignInfo current3 in list4)
				{
                    if (!Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._game.App, current3))
					{
						list2.Add(current3);
						num2++;
						if (current3.DesignDate > num)
						{
							num = current3.DesignDate;
						}
					}
				}
				if (num2 == 0)
				{
					StrategicAI.TraceVerbose(string.Format("    Creating a new design for {0}...", current2.role));
					ShipClass shipClass = DesignLab.SuggestShipClassForNewDesign(this._db, this._player, current2.role);
					WeaponRole wpnRole = DesignLab.SuggestWeaponRoleForNewDesign(aiInfo.Stance, current2.role, shipClass);
					DesignInfo designInfo = null;
					if (current2.role == ShipRole.COMBAT)
					{
						designInfo = this.TryCounterDesign(shipClass);
					}
					if (designInfo == null)
					{
						designInfo = DesignLab.DesignShip(this._game, shipClass, current2.role, wpnRole, this._player.ID, this._techStyles);
					}
					if (designInfo != null)
					{
						this._db.InsertDesignByDesignInfo(designInfo);
						StrategicAI.TraceVerbose(string.Format("    New design inserted: {0}", designInfo));
						flag = true;
						list3.Add(0f);
					}
				}
				else
				{
					float num3 = current2.weight * (float)(this._db.GetTurnCount() - num);
					list3.Add(num3);
					StrategicAI.TraceVerbose(string.Format("    + {0} redesign weight {1}, turn {2}.", current2.role, num3, num));
				}
			}
			if (flag)
			{
				StrategicAI.TraceVerbose("  Stopping because a new design has already been inserted.");
				return;
			}
			float num4 = 10f;
			if (aiInfo.Stance == AIStance.EXPANDING || aiInfo.Stance == AIStance.HUNKERING)
			{
				num4 = 15f;
			}
			StrategicAI.TraceVerbose(string.Format("  Redesign minimum weight threshold for {0}: {1}", aiInfo.Stance, num4));
			if (list3.Count == 0)
			{
				StrategicAI.TraceVerbose("  Stopping because there are no redesign weights.");
				return;
			}
			float num5 = 0f;
			for (int i = 0; i < list3.Count<float>(); i++)
			{
				if (list3[i] < num4)
				{
					list3[i] = 0f;
				}
				num5 += list3[i];
			}
			StrategicAI.TraceVerbose(string.Format("  Filtered weight sum: {0}", num5));
			if (num5 < 1f)
			{
				StrategicAI.TraceVerbose("  Stopping because the weight sum is less than one.");
				return;
			}
			float num6 = (float)(App.GetSafeRandom().NextDouble() * (double)num5);
			StrategicAI.TraceVerbose(string.Format("  Testing candidates for redesign based on initial roll: {0} out of {1}... ", num6, num5));
			for (int j = 0; j < list3.Count<float>(); j++)
			{
				if (num6 < list3[j])
				{
					StrategicAI.TraceVerbose(string.Format("    {0} < {1}: Attempting redesign for {2}, {3}...", new object[]
					{
						num6,
						list3[j],
						list[j].role,
						list[j].weight
					}));
					ShipClass shipClass2 = DesignLab.SuggestShipClassForNewDesign(this._db, this._player, list[j].role);
					WeaponRole wpnRole2 = DesignLab.SuggestWeaponRoleForNewDesign(aiInfo.Stance, list[j].role, shipClass2);
					DesignInfo designInfo2 = null;
					if (list[j].role == ShipRole.COMBAT)
					{
						designInfo2 = this.TryCounterDesign(shipClass2);
					}
					if (designInfo2 == null)
					{
						designInfo2 = DesignLab.DesignShip(this._game, shipClass2, list[j].role, wpnRole2, this._player.ID, this._techStyles);
					}
					if (designInfo2 != null)
					{
						DesignInfo designInfo3 = null;
						foreach (DesignInfo current4 in this._db.GetVisibleDesignInfosForPlayerAndRole(this._player.ID, designInfo2.Role, true))
						{
							if (StrategicAI.DoesDesignMatch(designInfo2, current4))
							{
								designInfo3 = current4;
								break;
							}
						}
						if (designInfo3 != null)
						{
							StrategicAI.TraceVerbose(string.Format("    REJECTED new design {0} because it is too similar to existing design {1}.", designInfo2, designInfo3));
						}
						else
						{
							this._db.InsertDesignByDesignInfo(designInfo2);
							StrategicAI.TraceVerbose(string.Format("    New design inserted: {0}", designInfo2));
						}
					}
				}
				else
				{
					num6 -= list3[j];
				}
			}
		}
		public DesignInfo TryCounterDesign(ShipClass desiredClass)
		{
			StrategicAI.DesignConfigurationInfo designConfigurationInfo = default(StrategicAI.DesignConfigurationInfo);
			IEnumerable<CombatData> combatsForPlayer = this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 10);
			int num = 0;
			if (combatsForPlayer.Count<CombatData>() > 0)
			{
				foreach (CombatData current in combatsForPlayer)
				{
					IEnumerable<PlayerCombatData> enumerable = 
						from x in current.GetPlayers()
						where x.PlayerID != this._player.ID
						select x;
					foreach (PlayerCombatData current2 in enumerable)
					{
						Player playerObject = this._game.GetPlayerObject(current2.PlayerID);
						if (playerObject.IsStandardPlayer)
						{
							DiplomacyState diplomacyStateBetweenPlayers = this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, playerObject.ID);
							if (diplomacyStateBetweenPlayers == DiplomacyState.WAR)
							{
								foreach (ShipData current3 in current2.ShipData)
								{
									DesignInfo designInfo = this._db.GetDesignInfo(current3.designID);
									designConfigurationInfo += StrategicAI.GetDesignConfigurationInfo(this._game, designInfo);
									num++;
								}
							}
						}
					}
				}
				if (num > 0)
				{
					designConfigurationInfo.Average(num);
					return DesignLab.CreateCounterDesign(this._game, desiredClass, this._player.ID, designConfigurationInfo);
				}
			}
			return null;
		}
		public static bool IsDesignObsolete(GameSession game, int playerID, int designID)
		{
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(designID);
			if (designInfo.Class != ShipClass.Leviathan)
			{
				ShipSectionAsset shipSectionAsset = DesignLab.ChooseDriveSection(game, designInfo.Class, playerID, null);
				ShipSectionAsset designSection = GameDatabase.GetDesignSection(game, designID, ShipSectionType.Engine);
				if (designSection != null && designSection.FileName != shipSectionAsset.FileName)
				{
					return true;
				}
			}
			int num = 0;
			List<DesignInfo> list = new List<DesignInfo>(game.GameDatabase.GetVisibleDesignInfosForPlayerAndRole(playerID, designInfo.Role, true));
			foreach (DesignInfo current in list)
			{
                if (!Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(game.App, current) && current.ID != designInfo.ID && current.Class == designInfo.Class && current.DesignDate > designInfo.DesignDate)
				{
					if (designInfo.Role != ShipRole.COMBAT && designInfo.Role != ShipRole.POLICE)
					{
						bool result = true;
						return result;
					}
					ShipSectionAsset designSection2 = GameDatabase.GetDesignSection(game, current.ID, ShipSectionType.Mission);
					ShipSectionAsset designSection3 = GameDatabase.GetDesignSection(game, designInfo.ID, ShipSectionType.Mission);
					if (designSection3 != null && designSection2 != null && designSection3.FileName == designSection2.FileName)
					{
						bool result = true;
						return result;
					}
					num++;
					if (num > 3)
					{
						bool result = true;
						return result;
					}
				}
			}
			return false;
		}
		public bool HandleRequestRequest(RequestInfo ri)
		{
			DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(ri.ReceivingPlayer, ri.InitiatingPlayer);
			float num = (float)diplomacyInfo.Relations / 2000f;
			float stratModifier = this._game.GameDatabase.GetStratModifier<float>(StratModifiers.DiplomacyRequestWeight, ri.ReceivingPlayer);
			float num2 = num * stratModifier;
			bool result = false;
			RequestType type = ri.Type;
			if (type == RequestType.SavingsRequest)
			{
				PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
				if (playerInfo.Savings > (double)ri.RequestValue)
				{
					float num3 = (float)playerInfo.Savings / ri.RequestValue;
					result = this._random.CoinToss((double)(num2 * num3));
				}
			}
			else
			{
				result = this._random.CoinToss((double)num2);
			}
			return result;
		}
		public bool HandleDemandRequest(DemandInfo di)
		{
			float num = this.AssessOwnStrength();
			float num2 = this.AssessPlayerStrength(di.InitiatingPlayer);
			float num3 = num2 * 1.2f / num;
			DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(di.ReceivingPlayer, di.InitiatingPlayer);
			float num4 = (float)diplomacyInfo.Relations / 2000f;
			float stratModifier = this._game.GameDatabase.GetStratModifier<float>(StratModifiers.DiplomacyDemandWeight, di.ReceivingPlayer);
			float num5 = num4 * num3 * stratModifier;
			bool result = false;
			DemandType type = di.Type;
			if (type != DemandType.SavingsDemand)
			{
				if (type == DemandType.WorldDemand)
				{
					if (di.DemandValue != 0f)
					{
						IEnumerable<FleetInfo> fleetsByPlayerAndSystem = this._db.GetFleetsByPlayerAndSystem(di.ReceivingPlayer, (int)di.DemandValue, FleetType.FL_NORMAL);
						if (fleetsByPlayerAndSystem.Count<FleetInfo>() == 0)
						{
							result = this._random.CoinToss((double)num5);
						}
					}
				}
				else
				{
					result = this._random.CoinToss((double)num5);
				}
			}
			else
			{
				PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
				if (playerInfo.Savings > (double)di.DemandValue)
				{
					result = this._random.CoinToss((double)num5);
				}
			}
			return result;
		}
		public bool HandleTreatyOffer(TreatyInfo ti)
		{
			return this._random.CoinToss(50);
		}
		public void HandleGive(GiveInfo gi)
		{
			DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(this._player.ID, gi.InitiatingPlayer);
			diplomacyInfo.Relations += (int)(Math.Sqrt(2.0 * ((double)gi.GiveValue / 200.0) + 1.0) - 1.0);
			this._db.UpdateDiplomacyState(this._player.ID, gi.InitiatingPlayer, diplomacyInfo.State, diplomacyInfo.Relations, false);
		}
		private void ResetData()
		{
			StrategicAI.TraceVerbose("Assigning missions...");
			this.m_AvailableFullFleets.Clear();
			this.m_AvailableShortFleets.Clear();
			this.m_DefenseCombatFleets.Clear();
			this.m_FleetsInSurveyRange.Clear();
			this.m_AvailableTasks.Clear();
			this.m_RelocationTasks.Clear();
			this.m_ColonizedSystems.Clear();
			this.m_FleetCubePoints.Clear();
			for (int i = 0; i < 8; i++)
			{
				this.m_NumStations[i] = 0;
			}
		}
		private void GetNumRequiredDefenceFleets(out int forCapitalSystem, out int forOtherSystem, AIStance stance)
		{
			forCapitalSystem = 0;
			forOtherSystem = 0;
			switch (stance)
			{
			case AIStance.EXPANDING:
				forCapitalSystem = 1;
				forOtherSystem = 0;
				return;
			case AIStance.ARMING:
				forCapitalSystem = 1;
				forOtherSystem = 1;
				return;
			case AIStance.HUNKERING:
				forCapitalSystem = 2;
				forOtherSystem = 1;
				return;
			case AIStance.CONQUERING:
				forCapitalSystem = 1;
				forOtherSystem = 0;
				return;
			case AIStance.DESTROYING:
				forCapitalSystem = 1;
				forOtherSystem = 0;
				return;
			case AIStance.DEFENDING:
				forCapitalSystem = 3;
				forOtherSystem = 2;
				return;
			default:
				return;
			}
		}
        private void GatherAllResources(AIStance stance)
        {
            this.m_TurnBudget = Budget.GenerateBudget(this._game, this._db.GetPlayerInfo(this._player.ID), null, BudgetProjection.Actual);
            this.m_GateCapacity = GameSession.GetTotalGateCapacity(this._game, this._player.ID);
            this.m_LoaLimit = Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this._game, this._player.ID);
            List<FleetInfo> list = this._game.GameDatabase.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
            List<AIFleetInfo> source = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
            List<AIFleetInfo> list3 = (from x in source
                where !x.AdmiralID.HasValue
                select x).ToList<AIFleetInfo>();
            List<FleetInfo> list4 = (from x in list
                where x.AdmiralID == 0
                select x).ToList<FleetInfo>();
            foreach (AIFleetInfo info in list3)
            {
                this._db.RemoveAIFleetInfo(info.ID);
                source.Remove(info);
            }
            using (List<FleetInfo>.Enumerator enumerator2 = list4.GetEnumerator())
            {
                Func<AIFleetInfo, bool> predicate = null;
                FleetInfo fleet;
                while (enumerator2.MoveNext())
                {
                    fleet = enumerator2.Current;
                    if (predicate == null)
                    {
                        predicate = x => x.FleetID.HasValue && (x.FleetID.Value == fleet.ID);
                    }
                    AIFleetInfo item = source.FirstOrDefault<AIFleetInfo>(predicate);
                    if (item != null)
                    {
                        this._db.RemoveAIFleetInfo(item.ID);
                        source.Remove(item);
                    }
                    list.Remove(fleet);
                }
            }
            using (List<AIFleetInfo>.Enumerator enumerator3 = source.GetEnumerator())
            {
                Func<FleetTemplate, bool> func2 = null;
                Func<ShipInfo, bool> func3 = null;
                AIFleetInfo aiFleet;
                while (enumerator3.MoveNext())
                {
                    aiFleet = enumerator3.Current;
                    if (!aiFleet.InvoiceID.HasValue && !aiFleet.FleetID.HasValue)
                    {
                        if (func2 == null)
                        {
                            func2 = x => x.Name == aiFleet.FleetTemplate;
                        }
                        FleetTemplate template = this._db.AssetDatabase.FleetTemplates.First<FleetTemplate>(func2);
                        aiFleet.FleetID = new int?(this._db.InsertFleet(this._player.ID, aiFleet.AdmiralID.Value, aiFleet.SystemID, aiFleet.SystemID, template.FleetName, FleetType.FL_NORMAL));
                        aiFleet.InvoiceID = null;
                        this._db.UpdateAIFleetInfo(aiFleet);
                        FleetInfo info3 = this._db.InsertOrGetReserveFleetInfo(aiFleet.SystemID, this._player.ID);
                        if (func3 == null)
                        {
                            func3 = delegate (ShipInfo x) {
                                int? aIFleetID = x.AIFleetID;
                                int iD = aiFleet.ID;
                                return (aIFleetID.GetValueOrDefault() == iD) && aIFleetID.HasValue;
                            };
                        }
                        foreach (ShipInfo info4 in this._db.GetShipInfoByFleetID(info3.ID, false).Where<ShipInfo>(func3).ToList<ShipInfo>())
                        {
                            this._db.TransferShip(info4.ID, aiFleet.FleetID.Value);
                        }
                        if (this._player.Faction.Name == "loa")
                        {
                            this._db.SaveCurrentFleetCompositionToFleet(aiFleet.FleetID.Value);
                        }
                    }
                }
            }
            int forCapitalSystem = 0;
            int forOtherSystem = 0;
            this.GetNumRequiredDefenceFleets(out forCapitalSystem, out forOtherSystem, stance);
            int starSystemID = 0;
            if (this._player.PlayerInfo.Homeworld.HasValue)
            {
                OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(this._player.PlayerInfo.Homeworld.Value);
                if (orbitalObjectInfo != null)
                {
                    starSystemID = orbitalObjectInfo.StarSystemID;
                }
            }
            foreach (ColonyInfo info6 in this._db.GetColonyInfos())
            {
                if (info6.PlayerID == this._player.ID)
                {
                    OrbitalObjectInfo orbit = this._db.GetOrbitalObjectInfo(info6.OrbitalObjectID);
                    if (!this.m_ColonizedSystems.Any<SystemDefendInfo>(x => (x.SystemID == orbit.StarSystemID)))
                    {
                        StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(orbit.StarSystemID);
                        SystemDefendInfo info8 = new SystemDefendInfo {
                            SystemID = orbit.StarSystemID,
                            IsHomeWorld = starSystemID == orbit.StarSystemID,
                            IsCapital = ((starSystemInfo != null) && starSystemInfo.ProvinceID.HasValue) && (starSystemInfo.ID == this._db.GetProvinceCapitalSystemID(starSystemInfo.ProvinceID.Value)),
                            NumColonies = this._db.GetColonyInfosForSystem(orbit.StarSystemID).Count<ColonyInfo>(),
                            ProductionRate = 0f,
                            ConstructionRate = 0.0
                        };
                        double totalRevenue = 0.0;
                        BuildScreenState.ObtainConstructionCosts(out info8.ProductionRate, out info8.ConstructionRate, out totalRevenue, this._game.App, orbit.StarSystemID, this._player.ID);
                        this.m_ColonizedSystems.Add(info8);
                        int num5 = forOtherSystem;
                        if (info8.IsCapital || info8.IsHomeWorld)
                        {
                            num5 = forCapitalSystem;
                        }
                        if (num5 > 0)
                        {
                            List<FleetInfo> list6 = this._db.GetFleetsByPlayerAndSystem(this._player.ID, orbit.StarSystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
                            List<FleetManagement> list7 = new List<FleetManagement>();
                            using (List<FleetInfo>.Enumerator enumerator6 = list6.GetEnumerator())
                            {
                                Func<AIFleetInfo, bool> func4 = null;
                                FleetInfo fleet;
                                while (enumerator6.MoveNext())
                                {
                                    fleet = enumerator6.Current;
                                    if (this._db.GetMissionByFleetID(fleet.ID) == null)
                                    {
                                        if (func4 == null)
                                        {
                                            func4 = delegate (AIFleetInfo x) {
                                                int? fleetID = x.FleetID;
                                                int iD = fleet.ID;
                                                return (fleetID.GetValueOrDefault() == iD) && fleetID.HasValue;
                                            };
                                        }
                                        AIFleetInfo info9 = source.FirstOrDefault<AIFleetInfo>(func4);
                                        if ((info9 != null) && (info9.FleetTemplate == this.m_CombatFleetTemplate.Name))
                                        {
                                            FleetManagement management = new FleetManagement {
                                                Fleet = fleet,
                                                FleetStrength = this.GetFleetStrength(fleet)
                                            };
                                            list7.Add(management);
                                        }
                                    }
                                }
                            }
                            if (list7.Count > 0)
                            {
                                var unused = from x in list7
                                    orderby x.FleetStrength descending
                                    select x;
                                for (int i = 0; i < num5; i++)
                                {
                                    FleetManagement management2 = list7.FirstOrDefault<FleetManagement>();
                                    if (management2 == null)
                                    {
                                        break;
                                    }
                                    this.m_DefenseCombatFleets.Add(management2.Fleet);
                                }
                            }
                        }
                    }
                }
            }
            if (this.m_ColonizedSystems.Count > 0)
            {
                this.m_ColonizedSystems.Sort(new SystemDefendInfoComparision());
            }
            foreach (FleetInfo info10 in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>())
            {
                if (info10.AdmiralID != 0)
                {
                    if (this._player.Faction.Name == "loa")
                    {
                        this.TransferCubesFromReserve(info10);
                        this.m_FleetCubePoints.Add(info10.ID, Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, info10.ID));

                        PerformanceStarFleet.StarFleet.BuildFleetFromCompositionID(this._game, info10.ID, info10.FleetConfigID, MissionType.NO_MISSION);
                        //Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromCompositionID(this._game, info10.ID, info10.FleetConfigID, MissionType.NO_MISSION);
                        this.ConsumeRemainingLoaCubes(info10, stance);
                        if (!info10.FleetConfigID.HasValue)
                        {
                            this._db.SaveCurrentFleetCompositionToFleet(info10.ID);
                            info10.FleetConfigID = this._db.GetFleetInfo(info10.ID).FleetConfigID;
                        }
                    }
                    this.FixFullFleet(info10);
                    MissionInfo missionByFleetID = this._game.GameDatabase.GetMissionByFleetID(info10.ID);
                    if ((missionByFleetID != null) && ((missionByFleetID.Type != MissionType.RETURN) || (info10.SystemID == missionByFleetID.StartingSystem)))
                    {
                        if (this.FleetRequiresFill(info10))
                        {
                            Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, info10, true);
                        }
                    }
                    else if (Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandPoints(this._game.App, info10.ID, null) != 0)
                    {
                        if ((missionByFleetID != null) || this.FillFleet(info10))
                        {
                            this.m_AvailableFullFleets.Add(info10);
                        }
                        else
                        {
                            this.m_AvailableShortFleets.Add(info10);
                        }
                    }
                }
            }
            foreach (StationInfo info12 in this._db.GetStationInfosByPlayerID(this._player.ID))
            {
                this.m_NumStations[(int) info12.DesignInfo.StationType]++;
            }
            if (this._player.Faction.CanUseGate() || this._player.Faction.CanUseAccelerators())
            {
                foreach (MissionInfo info13 in this._db.GetMissionInfos().Where<MissionInfo>(delegate (MissionInfo x) {
                    if ((x.Type != MissionType.COLONIZATION) && (x.Type != MissionType.SUPPORT))
                    {
                        return (x.Type == MissionType.CONSTRUCT_STN);
                    }
                    return true;
                }).ToList<MissionInfo>())
                {
                    FleetInfo fleetInfo = this._db.GetFleetInfo(info13.FleetID);
                    if ((((fleetInfo != null) && (fleetInfo.PlayerID == this._player.ID)) && (fleetInfo.SupportingSystemID != info13.TargetSystemID)) && ((this._player.Faction.CanUseGate() && !this._db.SystemHasGate(info13.TargetSystemID, this._player.ID)) || (this._player.Faction.CanUseAccelerators() && !this._db.SystemHasAccelerator(info13.TargetSystemID, this._player.ID))))
                    {
                        Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleetInfo, true);
                    }
                }
            }
        }
        private void GatherAllTasks()
		{
			Dictionary<FleetInfo, FleetRangeData> dictionary = new Dictionary<FleetInfo, FleetRangeData>();
			List<FleetInfo> list = (
				from x in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL)
				where x.SystemID != 0
				select x).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				FleetRangeData fleetRangeData = new FleetRangeData();
                fleetRangeData.FleetRange = Kerberos.Sots.StarFleet.StarFleet.GetFleetRange(this._game, current);
				IEnumerable<ShipInfo> shipInfoByFleetID = this._db.GetShipInfoByFleetID(current.ID, false);
                fleetRangeData.FleetTravelSpeed = new float?(Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, current.ID, shipInfoByFleetID, false));
                fleetRangeData.FleetNodeTravelSpeed = new float?(Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, current.ID, shipInfoByFleetID, true));
				if (fleetRangeData.FleetRange > 0f)
				{
					dictionary.Add(current, fleetRangeData);
				}
			}
			List<StarSystemInfo> list2 = this._game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			foreach (StarSystemInfo current2 in list2)
			{
                this.m_FleetsInSurveyRange = Kerberos.Sots.StarFleet.StarFleet.GetFleetsInRangeOfSystem(this._game, current2.ID, dictionary, 1f);
				this.m_FleetsInSurveyRange.RemoveAll((FleetInfo x) => !this.m_AvailableFullFleets.Contains(x));
				if (this.m_FleetsInSurveyRange.Count != 0)
				{
					List<StrategicTask> tasksForSystem = this.GetTasksForSystem(current2, dictionary, this._game.isHostilesAtSystem(this._player.ID, current2.ID));
					if (tasksForSystem.Count > 0)
					{
						this.m_AvailableTasks.AddRange(tasksForSystem);
					}
					StrategicTask potentialRelocateTaskForSystem = this.GetPotentialRelocateTaskForSystem(current2, dictionary, false);
					if (potentialRelocateTaskForSystem != null)
					{
						this.m_RelocationTasks.Add(potentialRelocateTaskForSystem, new List<StrategicTask>());
					}
				}
			}
			this.m_FleetsInSurveyRange.Clear();
		}
		private void ApplyScores(AIStance stance)
		{
			foreach (StrategicTask current in this.m_AvailableTasks)
			{
				current.Score = this.GetScoreForTask(current, stance);
				current.NumFleetsRequested = this.GetNumFleetsRequestForTask(current);
				if (current.Score != 0 && current.NumFleetsRequested != 0)
				{
					foreach (FleetManagement current2 in current.UseableFleets)
					{
						current2.Score = this.GetScoreForFleet(current, current2);
					}
					current.UseableFleets.RemoveAll((FleetManagement x) => x.Score == 0);

					var unused = from x in current.UseableFleets
						orderby x.Score descending
						select x;
					current.SubScore = this.GetSubScoreForTask(current, stance);
				}
			}
			float num = GameSession.GetSupportRange(this._db.AssetDatabase, this._db, this._player.ID);
			num *= num;
			List<StrategicTask> list = new List<StrategicTask>();
			foreach (KeyValuePair<StrategicTask, List<StrategicTask>> current3 in this.m_RelocationTasks)
			{
				Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(current3.Key.SystemIDTarget);
				foreach (StrategicTask current4 in this.m_AvailableTasks)
				{
					Vector3 starSystemOrigin2 = this._db.GetStarSystemOrigin(current4.SystemIDTarget);
					if ((starSystemOrigin2 - starSystemOrigin).LengthSquared < num)
					{
						current3.Value.Add(current4);
					}
				}
				if (current3.Value.Count == 0 || current3.Key.UseableFleets.Count == 0)
				{
					list.Add(current3.Key);
				}
			}
			foreach (StrategicTask current5 in list)
			{
				this.m_RelocationTasks.Remove(current5);
			}
			this.m_AvailableTasks.RemoveAll((StrategicTask x) => x.Score == 0);
			this.m_AvailableTasks.Sort(new StrategicTaskComparision());
		}
		private bool MustRelocateCloser(StrategicTask task, FleetManagement fleet)
		{
			int num = 10;
			MissionType mission = task.Mission;
			if (mission != MissionType.SURVEY)
			{
				if (mission == MissionType.GATE || mission == MissionType.DEPLOY_NPG)
				{
					num *= 4;
				}
			}
			else
			{
				if (this._player.Faction.CanUseNodeLine(new bool?(true)))
				{
					return false;
				}
			}
			return fleet.MissionTime.TurnsToTarget > num;
		}
		private void AssignFleetsToTasks(AIStance stance)
		{
			List<int> fleetsUsed = new List<int>();
			foreach (FleetInfo fleet in this.m_AvailableFullFleets)
			{
				StrategicTask strategicTask = this.PickBestTaskForFleet(fleet, fleetsUsed);
				if (strategicTask != null)
				{
					int? rebaseTarget = null;
					StrategicTask strategicTask2 = (this._db.GetMissionByFleetID(fleet.ID) == null) ? this.PickBestRelocationTaskForFleet(fleet, strategicTask) : null;
					FleetManagement fleet2 = strategicTask.UseableFleets.First((FleetManagement x) => x.Fleet == fleet);
					if (strategicTask2 != null || this.MustRelocateCloser(strategicTask, fleet2))
					{
						if (strategicTask2 != null && strategicTask2.SystemIDTarget == strategicTask.SystemIDTarget)
						{
							rebaseTarget = new int?(strategicTask2.SystemIDTarget);
						}
						else
						{
							strategicTask = strategicTask2;
						}
					}
					if (strategicTask != null)
					{
						this.AssignFleetToTask(strategicTask, fleet, rebaseTarget);
						strategicTask.NumFleetsRequested--;
						fleetsUsed.Add(fleet.ID);
					}
				}
			}
			for (int i = 0; i < this.m_AvailableTasks.Count; i++)
			{
				StrategicTask strategicTask3 = this.m_AvailableTasks[i];
				if (strategicTask3.NumFleetsRequested > 0)
				{
					foreach (FleetManagement current in strategicTask3.UseableFleets)
					{
						StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(current.Fleet.SystemID);
						if (!(starSystemInfo == null) && !starSystemInfo.IsDeepSpace)
						{
							if (!fleetsUsed.Contains(current.Fleet.ID) && !this.m_AvailableShortFleets.Contains(current.Fleet) && !this.m_DefenseCombatFleets.Contains(current.Fleet))
							{
								StrategicTask strategicTask4 = this.PickBestRelocationTaskForFleet(current.Fleet, strategicTask3);
								if (strategicTask4 != null || this.MustRelocateCloser(strategicTask3, current))
								{
									if (strategicTask4 != null)
									{
										this.AssignFleetToTask(strategicTask4, current.Fleet, null);
										strategicTask3.NumFleetsRequested--;
										fleetsUsed.Add(current.Fleet.ID);
										continue;
									}
									continue;
								}
								else
								{
									this.AssignFleetToTask(strategicTask3, current.Fleet, null);
									strategicTask3.NumFleetsRequested--;
									fleetsUsed.Add(current.Fleet.ID);
								}
							}
							if (strategicTask3.NumFleetsRequested <= 0)
							{
								break;
							}
						}
					}
				}
			}
			foreach (FleetInfo current2 in (
				from x in this.m_AvailableFullFleets
				where !fleetsUsed.Contains(x.ID)
				select x).ToList<FleetInfo>())
			{
				StrategicTask strategicTask5 = this.PickBestRelocationTaskForFleet(current2, null);
				if (strategicTask5 != null)
				{
					this.AssignFleetToTask(strategicTask5, current2, null);
					fleetsUsed.Add(current2.ID);
				}
			}
			this.AssignDefenseFleetsToSystem();
		}
		private void BuildFleets(AIInfo aiInfo)
		{
			List<AIFleetInfo> myAIFleets = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			List<int> list = new List<int>();
			foreach (FleetInfo fleet in this.m_AvailableShortFleets)
			{
				if (!list.Contains(fleet.ID) && !this.FillFleet(fleet))
				{
					List<FleetInfo> list2 = (
						from x in this.m_AvailableShortFleets
						where x.SystemID == fleet.SystemID
						select x).ToList<FleetInfo>();
					list.AddRange((
						from x in list2
						select x.ID).ToList<int>());
					if (this.m_ColonizedSystems.Any((SystemDefendInfo x) => x.SystemID == fleet.SystemID))
					{
						this.FillFleetsWithBuild(fleet.SystemID, list2, aiInfo.Stance);
					}
					else
					{
						StrategicTask strategicTask = this.PickBestRelocationTaskForFleet(fleet, null);
						if (strategicTask != null)
						{
							this.AssignFleetToTask(strategicTask, fleet, null);
						}
					}
				}
			}
			double num = this.GetAvailableShipConstructionBudget(aiInfo.Stance);
			double num2 = this.GetAvailableFleetSupportBudget();
			List<SystemBuildInfo> list3 = new List<SystemBuildInfo>();
			foreach (SystemDefendInfo current in this.m_ColonizedSystems)
			{
				if (current.ConstructionRate > 0.0 && current.ProductionRate > 1f)
				{
					list3.Add(new SystemBuildInfo
					{
						SystemID = current.SystemID,
						SystemOrigin = this._db.GetStarSystemOrigin(current.SystemID),
						AvailableSupportPoints = this._db.GetRemainingSupportPoints(this._game, current.SystemID, this._player.ID),
						ProductionPerTurn = current.ProductionRate,
						RemainingBuildTime = this.GetRemainingAIBuildTime(current.SystemID, current.ProductionRate)
					});
				}
			}
			list3.RemoveAll((SystemBuildInfo x) => x.RemainingBuildTime < this.MAX_BUILD_TURNS);
			bool flag = false;
			if (list3.Count > 0)
			{
				int num3 = 0;
				int num4 = 0;
				this.GetNumRequiredDefenceFleets(out num3, out num4, aiInfo.Stance);
				int num5 = 0;
				if (this._player.PlayerInfo.Homeworld.HasValue)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(this._player.PlayerInfo.Homeworld.Value);
					if (orbitalObjectInfo != null)
					{
						num5 = orbitalObjectInfo.StarSystemID;
					}
				}
				List<SystemBuildInfo> list4 = new List<SystemBuildInfo>();
				foreach (SystemBuildInfo build in list3)
				{
					int num6 = num4;
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(build.SystemID);
					if ((starSystemInfo != null && starSystemInfo.ProvinceID.HasValue && starSystemInfo.ID == this._db.GetProvinceCapitalSystemID(starSystemInfo.ProvinceID.Value)) || num5 == build.SystemID)
					{
						num6 = num3;
					}
					int num7 = this.m_DefenseCombatFleets.Count((FleetInfo x) => x.SystemID == build.SystemID);
					num6 -= num7;
					if (num6 > 0 && this.m_CombatFleetTemplate != null)
					{
						List<AdmiralInfo> source = this._db.GetAdmiralInfosForPlayer(this._player.ID).ToList<AdmiralInfo>();
						List<FleetInfo> fleetInfos = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
						AdmiralInfo admiralInfo = source.FirstOrDefault((AdmiralInfo x) => !fleetInfos.Any((FleetInfo y) => y.AdmiralID == x.ID) && !myAIFleets.Any((AIFleetInfo y) => y.AdmiralID == x.ID));
						StarSystemInfo starSystemInfo2 = this._db.GetStarSystemInfo(build.SystemID);
						if (admiralInfo != null && starSystemInfo2 != null)
						{
							double num8 = (num5 == build.SystemID) ? 1.75 : 1.5;
							List<BuildScreenState.InvoiceItem> buildFleetInvoice = this.GetBuildFleetInvoice(this.m_CombatFleetTemplate, aiInfo.Stance);
							int buildTime = BuildScreenState.GetBuildTime(this._game.App, buildFleetInvoice, build.ProductionPerTurn);
							if (buildTime != 0)
							{
								List<BuildScreenState.InvoiceItem> invoiceAfterReserves = this.GetInvoiceAfterInculdingReserve(this.GetUnclaimedShipsInReserve(build.SystemID), buildFleetInvoice);
								int supportRequirementsForInvoice = this.GetSupportRequirementsForInvoice(buildFleetInvoice);
								int num9 = BuildScreenState.GetBuildInvoiceCost(this._game.App, invoiceAfterReserves) / buildTime;
								int num10 = buildFleetInvoice.Sum((BuildScreenState.InvoiceItem x) => GameSession.CalculateUpkeepCost(x.DesignID, this._game.App));
								if (supportRequirementsForInvoice <= build.AvailableSupportPoints && (double)num10 < num2 * num8)
								{
									if (!flag && (double)num9 < num * num8)
									{
										this.BuildFleet(aiInfo, admiralInfo, starSystemInfo2, this.m_CombatFleetTemplate, buildFleetInvoice, true);
										list4.Add(build);
										num -= (double)num9;
										num2 -= (double)num10;
										flag = true;
									}
									else
									{
										if ((float)invoiceAfterReserves.Count / (float)buildFleetInvoice.Count < 0.5f)
										{
											this.BuildFleet(aiInfo, admiralInfo, starSystemInfo2, this.m_CombatFleetTemplate, (
												from x in buildFleetInvoice
												where !invoiceAfterReserves.Contains(x) || this._db.GetDesignInfo(x.DesignID).Role == ShipRole.COMMAND
												select x).ToList<BuildScreenState.InvoiceItem>(), true);
										}
									}
								}
							}
						}
					}
				}
				foreach (SystemBuildInfo current2 in list4)
				{
					list3.Remove(current2);
				}
				if (list3.Count > 0)
				{
					myAIFleets = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
					List<AIFleetInfo> list5 = (
						from x in myAIFleets
						where !this.m_DefenseCombatFleets.Any((FleetInfo y) => y.ID == x.FleetID)
						select x).ToList<AIFleetInfo>();
					List<FleetTemplate> list6 = (
						from x in this._game.AssetDatabase.FleetTemplates
						where x.CanFactionUse(this._player.Faction.Name) && x.MinFleetsForStance[aiInfo.Stance] > 0
						select x).ToList<FleetTemplate>();
					list6.Sort(new FleetTemplateComparision(aiInfo.Stance, list5));
					bool flag2 = true;
					Dictionary<string, int> dictionary = new Dictionary<string, int>();
					foreach (FleetTemplate template in list6)
					{
						dictionary.Add(template.Name, list5.Count((AIFleetInfo x) => x.FleetTemplate == template.Name));
						if (template.MinFleetsForStance[aiInfo.Stance] - dictionary[template.Name] > 0)
						{
							flag2 = false;
						}
					}
					bool flag3 = false;
					foreach (FleetTemplate template in list6)
					{
						StrategicTask strategicTask2 = this.m_AvailableTasks.FirstOrDefault((StrategicTask x) => template.MissionTypes.Contains(x.Mission));
						if ((flag2 && strategicTask2 != null) || template.MinFleetsForStance[aiInfo.Stance] - dictionary[template.Name] > 0)
						{
							bool flag4 = FleetTemplateComparision.MustHaveTemplate(template);
							if (flag3 && !flag4)
							{
								break;
							}
							List<BuildScreenState.InvoiceItem> buildFleetInvoice2 = this.GetBuildFleetInvoice(template, aiInfo.Stance);
							int supportPointsRequired = this.GetSupportRequirementsForInvoice(buildFleetInvoice2);
							if (strategicTask2 != null)
							{
								Vector3 targetOrigin = this._db.GetStarSystemOrigin(strategicTask2.SystemIDTarget);
								list3 = (
									from x in list3
									orderby (x.SystemOrigin - targetOrigin).LengthSquared
									select x).ToList<SystemBuildInfo>();
							}
							SystemBuildInfo systemBuildInfo = list3.FirstOrDefault((SystemBuildInfo x) => x.AvailableSupportPoints >= supportPointsRequired);
							if (systemBuildInfo != null)
							{
								List<AdmiralInfo> source2 = this._db.GetAdmiralInfosForPlayer(this._player.ID).ToList<AdmiralInfo>();
								List<FleetInfo> fleetInfos = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
								AdmiralInfo admiralInfo2 = source2.FirstOrDefault((AdmiralInfo x) => !fleetInfos.Any((FleetInfo y) => y.AdmiralID == x.ID) && !myAIFleets.Any((AIFleetInfo y) => y.AdmiralID == x.ID));
								StarSystemInfo starSystemInfo3 = this._db.GetStarSystemInfo(systemBuildInfo.SystemID);
								if (admiralInfo2 != null && starSystemInfo3 != null)
								{
									double num11 = flag4 ? 1.25 : 1.0;
									int buildTime2 = BuildScreenState.GetBuildTime(this._game.App, buildFleetInvoice2, systemBuildInfo.ProductionPerTurn);
									if (buildTime2 == 0)
									{
										continue;
									}
									List<BuildScreenState.InvoiceItem> invoiceAfterReserves = this.GetInvoiceAfterInculdingReserve(this.GetUnclaimedShipsInReserve(systemBuildInfo.SystemID), buildFleetInvoice2);
									int num12 = BuildScreenState.GetBuildInvoiceCost(this._game.App, invoiceAfterReserves) / buildTime2;
									int num13 = buildFleetInvoice2.Sum((BuildScreenState.InvoiceItem x) => GameSession.CalculateUpkeepCost(x.DesignID, this._game.App));
									if ((double)num13 < num2 * num11)
									{
										if ((!flag && supportPointsRequired <= systemBuildInfo.AvailableSupportPoints && (double)num12 < num * num11) || (FleetTemplateComparision.MustHaveTemplate(template) && dictionary[template.Name] == 0) || (this._player.Faction.Name == "hiver" && template.MissionTypes.Contains(MissionType.COLONIZATION) && dictionary[template.Name] < 2))
										{
											this.BuildFleet(aiInfo, admiralInfo2, starSystemInfo3, template, buildFleetInvoice2, true);
											list3.Remove(systemBuildInfo);
											num -= (double)num12;
											num2 -= (double)num13;
											Dictionary<string, int> dictionary2;
											string name;
											(dictionary2 = dictionary)[name = template.Name] = dictionary2[name] + 1;
											flag = true;
										}
										else
										{
											if ((float)invoiceAfterReserves.Count / (float)buildFleetInvoice2.Count < 0.5f)
											{
												this.BuildFleet(aiInfo, admiralInfo2, starSystemInfo3, template, (
													from x in buildFleetInvoice2
													where !invoiceAfterReserves.Contains(x) || this._db.GetDesignInfo(x.DesignID).Role == ShipRole.COMMAND
													select x).ToList<BuildScreenState.InvoiceItem>(), true);
												Dictionary<string, int> dictionary2;
												string name;
												(dictionary2 = dictionary)[name = template.Name] = dictionary2[name] + 1;
											}
										}
									}
									if (flag4 && dictionary[template.Name] == 0)
									{
										flag3 = true;
									}
								}
								else
								{
									if (!flag2)
									{
										break;
									}
								}
								if (list3.Count == 0)
								{
									break;
								}
							}
						}
					}
					bool flag5 = true;
					foreach (SystemBuildInfo arg_C56_0 in list3)
					{
						SystemBuildInfo systemBuildInfo2 = list3.First<SystemBuildInfo>();
						List<AdmiralInfo> source3 = this._db.GetAdmiralInfosForPlayer(this._player.ID).ToList<AdmiralInfo>();
						List<FleetInfo> fleetInfos = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
						AdmiralInfo admiralInfo3 = source3.FirstOrDefault((AdmiralInfo x) => !fleetInfos.Any((FleetInfo y) => y.AdmiralID == x.ID) && !myAIFleets.Any((AIFleetInfo y) => y.AdmiralID == x.ID));
						StarSystemInfo starSystemInfo4 = this._db.GetStarSystemInfo(systemBuildInfo2.SystemID);
						if (admiralInfo3 != null && starSystemInfo4 != null)
						{
							if (flag5)
							{
								list6.Sort(new FleetTemplateComparision(aiInfo.Stance, (
									from x in this._db.GetAIFleetInfos(this._player.ID)
									where !this.m_DefenseCombatFleets.Any((FleetInfo y) => y.ID == x.FleetID)
									select x).ToList<AIFleetInfo>()));
								flag5 = false;
							}
							foreach (FleetTemplate current3 in list6)
							{
								List<BuildScreenState.InvoiceItem> list7 = this.GetBuildFleetInvoice(current3, aiInfo.Stance);
								int buildTime3 = BuildScreenState.GetBuildTime(this._game.App, list7, systemBuildInfo2.ProductionPerTurn);
								if (buildTime3 != 0)
								{
									List<BuildScreenState.InvoiceItem> invoiceAfterReserves = this.GetInvoiceAfterInculdingReserve(this.GetUnclaimedShipsInReserve(systemBuildInfo2.SystemID), list7);
									if ((float)invoiceAfterReserves.Count / (float)list7.Count < 0.5f)
									{
										list7 = (
											from x in list7
											where !invoiceAfterReserves.Contains(x) || this._db.GetDesignInfo(x.DesignID).Role == ShipRole.COMMAND
											select x).ToList<BuildScreenState.InvoiceItem>();
										int supportRequirementsForInvoice2 = this.GetSupportRequirementsForInvoice(list7);
										if (supportRequirementsForInvoice2 <= systemBuildInfo2.AvailableSupportPoints)
										{
											this.BuildFleet(aiInfo, admiralInfo3, starSystemInfo4, current3, list7, true);
											flag5 = true;
										}
									}
								}
							}
						}
					}
				}
			}
			if (list3.Count > 0)
			{
				num = Math.Max(num, 0.0) + this.GetAvailablePrototypeShipConstructionBudget();
				List<DesignInfo> list8 = (
					from x in this._db.GetDesignInfosForPlayer(this._player.ID)
					where !x.isPrototyped && x.Class != ShipClass.Station && !ShipSectionAsset.IsWeaponBattleRiderClass(x.GetRealShipClass().Value) && x.Role != ShipRole.POLICE
					orderby x.DesignDate, x.GetPlayerProductionCost(this._db, this._player.ID, true, null)
					select x).ToList<DesignInfo>();
				foreach (SystemDefendInfo current4 in this.m_ColonizedSystems)
				{
					List<BuildOrderInfo> list9 = this._db.GetBuildOrdersForSystem(current4.SystemID).ToList<BuildOrderInfo>();
					foreach (BuildOrderInfo b in list9)
					{
						list8.RemoveAll((DesignInfo x) => x.ID == b.DesignID);
					}
				}
				foreach (DesignInfo current5 in list8)
				{
					current5.isPrototyped = true;
				}
				list3 = (
					from x in list3
					orderby x.ProductionPerTurn descending
					select x).ToList<SystemBuildInfo>();
				foreach (SystemBuildInfo current6 in list3)
				{
					if (num <= 0.0)
					{
						break;
					}
					int? reserveFleetID = this._db.GetReserveFleetID(this._player.ID, current6.SystemID);
					if (reserveFleetID.HasValue)
					{
						if (this._db.GetShipInfoByFleetID(reserveFleetID.Value, true).Count((ShipInfo x) => x.DesignInfo.Class != ShipClass.BattleRider) <= 15)
						{
							this._db.GetInvoicesForSystem(this._player.ID, current6.SystemID).ToList<InvoiceInstanceInfo>();
							int arg_10CA_0 = current6.RemainingBuildTime;
							List<BuildScreenState.InvoiceItem> list10 = new List<BuildScreenState.InvoiceItem>();
							foreach (DesignInfo current7 in list8)
							{
								double num14 = (double)((float)BuildScreenState.GetDesignCost(this._game.App, current7, 0)) / Math.Ceiling((double)((float)current7.GetPlayerProductionCost(this._db, this._player.ID, false, null) / current6.ProductionPerTurn));
								if (num14 <= num)
								{
									num -= num14;
									list10.Add(new BuildScreenState.InvoiceItem
									{
										DesignID = current7.ID,
										ShipName = current7.Name,
										Progress = -1,
										isPrototypeOrder = false,
										LoaCubes = 0
									});
									list8.Remove(current7);
									break;
								}
							}
							if (list10.Count > 0)
							{
								StarSystemInfo starSystemInfo5 = this._db.GetStarSystemInfo(current6.SystemID);
								int invoiceId;
								int instanceId;
								this.OpenInvoice(current6.SystemID, starSystemInfo5.Name, out invoiceId, out instanceId);
								foreach (BuildScreenState.InvoiceItem current8 in list10)
								{
									this.AddShipToInvoice(current6.SystemID, this._db.GetDesignInfo(current8.DesignID), invoiceId, instanceId, null);
								}
							}
						}
					}
				}
			}
		}
		private List<FleetTemplate> GetRequiredFleetTemplatesForStance(AIStance stance)
		{
			List<FleetTemplate> list = (
				from x in this._game.AssetDatabase.FleetTemplates
				where x.StanceWeights.ContainsKey(stance) && !x.Initial
				select x).ToList<FleetTemplate>();

			var unused = from x in list
				orderby x.StanceWeights[stance] descending
				select x;
			return list;
		}
		private FleetTemplate GetBestFleetTemplate(AIStance stance, MissionType missionType)
		{
			List<FleetTemplate> list = (
				from x in this._game.AssetDatabase.FleetTemplates
				where x.MissionTypes.Contains(missionType) && !x.Initial
				select x).ToList<FleetTemplate>();
			if (list.Count == 0)
			{
				return null;
			}
			List<FleetTemplate> list2 = (
				from x in list
				where x.StanceWeights.ContainsKey(stance)
				select x).ToList<FleetTemplate>();
			List<Weighted<FleetTemplate>> list3 = (
				from x in list2
				select new Weighted<FleetTemplate>(x, x.StanceWeights[stance])).ToList<Weighted<FleetTemplate>>();
			if (list3.Count <= 0 || list2.Count <= 0)
			{
				return list.First<FleetTemplate>();
			}
			return WeightedChoices.Choose<FleetTemplate>(this._random, list3);
		}
		private void AssignFleetToTask(StrategicTask task, FleetInfo fleet, int? rebaseTarget = null)
		{
			MissionInfo missionByFleetID = this._db.GetMissionByFleetID(fleet.ID);
			if (missionByFleetID != null)
			{
				this._db.RemoveMission(missionByFleetID.ID);
			}
			if (this._player.Faction.CanUseAccelerators())
			{
				this._game.CheckLoaFleetGateCompliancy(fleet);
			}
			bool useDirectRoute = this._player.Faction.CanUseAccelerators();
			switch (task.Mission)
			{
			case MissionType.COLONIZATION:
                    Kerberos.Sots.StarFleet.StarFleet.SetColonizationMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, null, rebaseTarget);
				return;
			case MissionType.SUPPORT:
                Kerberos.Sots.StarFleet.StarFleet.SetSupportMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, null, this._game.GetNumSupportTrips(fleet.ID, task.SystemIDTarget), rebaseTarget);
				return;
			case MissionType.SURVEY:
                Kerberos.Sots.StarFleet.StarFleet.SetSurveyMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, null, rebaseTarget);
				return;
			case MissionType.RELOCATION:
                Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, null);
				return;
			case MissionType.CONSTRUCT_STN:
                Kerberos.Sots.StarFleet.StarFleet.SetConstructionMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, null, task.StationType, rebaseTarget);
				using (List<StrategicTask>.Enumerator enumerator = this.m_AvailableTasks.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						StrategicTask current = enumerator.Current;
						if (current != task && (current.Mission == MissionType.CONSTRUCT_STN || current.Mission == MissionType.UPGRADE_STN))
						{
							current.NumFleetsRequested = 0;
						}
					}
					return;
				}
				break;
			case MissionType.UPGRADE_STN:
				break;
			case MissionType.PATROL:
				goto IL_260;
			case MissionType.INTERDICTION:
                Kerberos.Sots.StarFleet.StarFleet.SetInterdictionMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, 0, null);
				return;
			case MissionType.STRIKE:
                Kerberos.Sots.StarFleet.StarFleet.SetStrikeMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, task.FleetIDTarget, null);
				return;
			case MissionType.INVASION:
                Kerberos.Sots.StarFleet.StarFleet.SetInvasionMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.PlanetIDTarget, null);
				return;
			case MissionType.INTERCEPT:
                Kerberos.Sots.StarFleet.StarFleet.SetFleetInterceptMission(this._game, fleet.ID, task.FleetIDTarget, useDirectRoute, null);
				return;
			case MissionType.GATE:
                Kerberos.Sots.StarFleet.StarFleet.SetGateMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, null, rebaseTarget);
				return;
			case MissionType.RETURN:
			case MissionType.RETREAT:
			case MissionType.PIRACY:
				return;
			case MissionType.DEPLOY_NPG:
                Kerberos.Sots.StarFleet.StarFleet.SetNPGMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, Kerberos.Sots.StarFleet.StarFleet.GetAccelGatePercentPointsBetweenSystems(this._db, fleet.SystemID, task.SystemIDTarget).ToList<int>(), null, rebaseTarget);
				return;
			default:
				return;
			}
            Kerberos.Sots.StarFleet.StarFleet.SetUpgradeStationMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, task.StationIDTarget, null, task.StationType, rebaseTarget);
			using (List<StrategicTask>.Enumerator enumerator2 = this.m_AvailableTasks.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					StrategicTask current2 = enumerator2.Current;
					if (current2 != task && (current2.Mission == MissionType.CONSTRUCT_STN || current2.Mission == MissionType.UPGRADE_STN))
					{
						current2.NumFleetsRequested = 0;
					}
				}
				return;
			}
			IL_260:
            Kerberos.Sots.StarFleet.StarFleet.SetPatrolMission(this._game, fleet.ID, task.SystemIDTarget, useDirectRoute, null, rebaseTarget);
		}
		private List<StrategicTask> GetTasksForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			List<StrategicTask> list = new List<StrategicTask>();
			StrategicTask strategicTask = this.GetPotentialSurveyTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strategicTask != null)
			{
				list.Add(strategicTask);
			}
			strategicTask = this.GetPotentialGateTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strategicTask != null)
			{
				list.Add(strategicTask);
			}
			strategicTask = this.GetPotentialNPGTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strategicTask != null)
			{
				list.Add(strategicTask);
			}
			List<StrategicTask> potentialColonizeTasksForSystem = this.GetPotentialColonizeTasksForSystem(system, fleetRanges, hostilesAtSystem);
			if (potentialColonizeTasksForSystem.Count > 0)
			{
				list.AddRange(potentialColonizeTasksForSystem);
			}
			List<StrategicTask> potentialConstructionTasksForSystem = this.GetPotentialConstructionTasksForSystem(system, fleetRanges, hostilesAtSystem);
			if (potentialConstructionTasksForSystem.Count > 0)
			{
				list.AddRange(potentialConstructionTasksForSystem);
			}
			List<StrategicTask> potentialUpgradeTasksForSystem = this.GetPotentialUpgradeTasksForSystem(system, fleetRanges, hostilesAtSystem);
			if (potentialUpgradeTasksForSystem.Count > 0)
			{
				list.AddRange(potentialUpgradeTasksForSystem);
			}
			strategicTask = this.GetPotentialPatrolTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strategicTask != null)
			{
				list.Add(strategicTask);
			}
			strategicTask = this.GetPotentialInterdictTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strategicTask != null)
			{
				list.Add(strategicTask);
			}
			strategicTask = this.GetPotentialInterceptTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strategicTask != null)
			{
				list.Add(strategicTask);
			}
			strategicTask = this.GetPotentialStrikeTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strategicTask != null)
			{
				list.Add(strategicTask);
			}
			strategicTask = this.GetPotentialInvadeTaskForSystem(system, fleetRanges, hostilesAtSystem);
			if (strategicTask != null)
			{
				list.Add(strategicTask);
			}
			return list;
		}
		private StrategicTask GetPotentialSurveyTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			if (!this._db.CanSurvey(this._player.ID, system.ID))
			{
				return null;
			}
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.SURVEY;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.SURVEY;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo current in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = null;
				float? nodeTravelSpeed = null;
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(current, out fleetRangeData))
				{
					new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.SURVEY, StationType.INVALID_TYPE, current.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement
					{
						Fleet = current,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}
		private StrategicTask GetPotentialGateTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			if (!this._player.Faction.CanUseGate() || !this._game.GameDatabase.CanGate(this._player.ID, system.ID))
			{
				return null;
			}
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.GATE;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.GATE;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo current in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = null;
				float? nodeTravelSpeed = null;
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(current, out fleetRangeData))
				{
					new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.GATE, StationType.INVALID_TYPE, current.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement
					{
						Fleet = current,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}
		private StrategicTask GetPotentialNPGTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			if (!this._player.Faction.CanUseAccelerators() || this._db.SystemHasAccelerator(system.ID, this._player.ID))
			{
				return null;
			}
			if (this.m_FleetsInSurveyRange.Count == 0)
			{
				return null;
			}
			DesignInfo designInfo = this._db.GetDesignInfosForPlayer(this._player.ID).FirstOrDefault((DesignInfo x) => x.IsAccelerator());
			int productionCost = designInfo.ProductionCost;
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.DEPLOY_NPG;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.NPG;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo current in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = null;
				float? nodeTravelSpeed = null;
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(current, out fleetRangeData))
				{
					new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
					int num = 0;
					if ((!this.m_FleetCubePoints.TryGetValue(current.ID, out num) || num >= productionCost) && this._db.GetMissionByFleetID(current.ID) == null)
					{
                        MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.DEPLOY_NPG, StationType.INVALID_TYPE, current.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
						strategicTask.UseableFleets.Add(new FleetManagement
						{
							Fleet = current,
							MissionTime = missionEstimate,
							FleetTypes = fleetTypeFlags
						});
					}
				}
			}
			return strategicTask;
		}
		private List<StrategicTask> GetPotentialColonizeTasksForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			List<StrategicTask> list = new List<StrategicTask>();
			if (this.m_TurnBudget.CurrentSavings < -1000000.0 || !this._db.CanColonize(this._player.ID, system.ID, this._db.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this._player.ID)) || this.m_AvailableFullFleets.Count == 0)
			{
				return list;
			}
			List<int> list2 = StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this._game.App, system.ID, this._player.ID).ToList<int>();
			if (list2.Count == 0)
			{
				return list;
			}
			foreach (int current in list2)
			{
				StrategicTask strategicTask = new StrategicTask();
				strategicTask.Mission = MissionType.COLONIZATION;
				strategicTask.RequiredFleetTypes = FleetTypeFlags.COLONIZE;
				strategicTask.SystemIDTarget = system.ID;
				strategicTask.PlanetIDTarget = current;
				strategicTask.HostilesAtSystem = hostilesAtSystem;
				foreach (FleetInfo current2 in this.m_FleetsInSurveyRange)
				{
					float? travelSpeed = null;
					float? nodeTravelSpeed = null;
					FleetRangeData fleetRangeData;
					if (fleetRanges.TryGetValue(current2, out fleetRangeData))
					{
						new float?(fleetRangeData.FleetRange);
						travelSpeed = fleetRangeData.FleetTravelSpeed;
						nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
					}
					FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current2.ID, this._player.ID, this._player.Faction.Name == "loa");
					if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
					{
                        MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.COLONIZATION, StationType.INVALID_TYPE, current2.ID, system.ID, current, null, 1, false, travelSpeed, nodeTravelSpeed);
						strategicTask.UseableFleets.Add(new FleetManagement
						{
							Fleet = current2,
							MissionTime = missionEstimate,
							FleetTypes = fleetTypeFlags
						});
					}
				}
				list.Add(strategicTask);
			}
			return list;
		}
		private List<StrategicTask> GetPotentialSupportTasksForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			List<StrategicTask> list = new List<StrategicTask>();
			if (!this._db.CanSupport(this._player.ID, system.ID))
			{
				return list;
			}
			if (this.m_FleetsInSurveyRange.Count == 0)
			{
				return list;
			}
			List<int> list2 = StarSystemDetailsUI.CollectPlanetListItemsForSupportMission(this._game.App, system.ID).ToList<int>();
			if (list2.Count == 0)
			{
				return list;
			}
			foreach (int current in list2)
			{
				StrategicTask strategicTask = new StrategicTask();
				strategicTask.Mission = MissionType.SUPPORT;
				strategicTask.RequiredFleetTypes = FleetTypeFlags.COLONIZE;
				strategicTask.SystemIDTarget = system.ID;
				strategicTask.PlanetIDTarget = current;
				strategicTask.HostilesAtSystem = hostilesAtSystem;
				foreach (FleetInfo current2 in this.m_FleetsInSurveyRange)
				{
					FleetRangeData fleetRangeData;
					if (fleetRanges.TryGetValue(current2, out fleetRangeData))
					{
						new float?(fleetRangeData.FleetRange);
						float? arg_DA_0 = fleetRangeData.FleetTravelSpeed;
						float? arg_E2_0 = fleetRangeData.FleetNodeTravelSpeed;
					}
					FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current2.ID, this._player.ID, this._player.Faction.Name == "loa");
					if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
					{
						MissionEstimate missionTime = new MissionEstimate();
						strategicTask.UseableFleets.Add(new FleetManagement
						{
							Fleet = current2,
							MissionTime = missionTime,
							FleetTypes = fleetTypeFlags
						});
					}
				}
				list.Add(strategicTask);
			}
			return list;
		}
		private List<StrategicTask> GetPotentialConstructionTasksForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			List<StrategicTask> list = new List<StrategicTask>();
			int? systemOwningPlayer = this._game.GameDatabase.GetSystemOwningPlayer(system.ID);
			if (systemOwningPlayer.HasValue && systemOwningPlayer.Value != this._player.ID && !StarMapState.SystemHasIndependentColony(this._game, system.ID))
			{
				return list;
			}
			List<StationType> list2 = (
				from j in Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this._game, system.ID, this._player.ID)
				where j != StationType.DEFENCE
				select j).ToList<StationType>();
			if (list2.Count == 0)
			{
				return list;
			}
			foreach (StationType current in list2)
			{
				int? suitablePlanetForStation = Kerberos.Sots.GameStates.StarSystem.GetSuitablePlanetForStation(this._game, this._player.ID, system.ID, current);
				if (suitablePlanetForStation.HasValue)
				{
					StrategicTask strategicTask = new StrategicTask();
					strategicTask.Mission = MissionType.CONSTRUCT_STN;
					strategicTask.RequiredFleetTypes = FleetTypeFlags.CONSTRUCTION;
					strategicTask.SystemIDTarget = system.ID;
					strategicTask.StationType = current;
					strategicTask.PlanetIDTarget = suitablePlanetForStation.Value;
					strategicTask.HostilesAtSystem = hostilesAtSystem;
					foreach (FleetInfo current2 in this.m_FleetsInSurveyRange)
					{
						float? travelSpeed = null;
						float? nodeTravelSpeed = null;
						FleetRangeData fleetRangeData;
						if (fleetRanges.TryGetValue(current2, out fleetRangeData))
						{
							new float?(fleetRangeData.FleetRange);
							travelSpeed = fleetRangeData.FleetTravelSpeed;
							nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
						}
						FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current2.ID, this._player.ID, this._player.Faction.Name == "loa");
                        if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN && Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this._game, current2.ID) >= 1f)
						{
                            MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.CONSTRUCT_STN, current, current2.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
							strategicTask.UseableFleets.Add(new FleetManagement
							{
								Fleet = current2,
								MissionTime = missionEstimate,
								FleetTypes = fleetTypeFlags
							});
						}
					}
					list.Add(strategicTask);
				}
			}
			return list;
		}
		private List<StrategicTask> GetPotentialUpgradeTasksForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			List<StrategicTask> list = new List<StrategicTask>();
			List<StationInfo> upgradableStations = this._game.GetUpgradableStations(this._game.GameDatabase.GetStationForSystemAndPlayer(system.ID, this._player.ID).ToList<StationInfo>());
			if (upgradableStations.Count == 0)
			{
				return list;
			}
			foreach (StationInfo current in upgradableStations)
			{
				StrategicTask strategicTask = new StrategicTask();
				strategicTask.Mission = MissionType.UPGRADE_STN;
				strategicTask.RequiredFleetTypes = FleetTypeFlags.CONSTRUCTION;
				strategicTask.SystemIDTarget = system.ID;
				strategicTask.StationIDTarget = current.OrbitalObjectID;
				strategicTask.StationType = current.DesignInfo.StationType;
				strategicTask.HostilesAtSystem = hostilesAtSystem;
				foreach (FleetInfo current2 in this.m_FleetsInSurveyRange)
				{
					float? travelSpeed = null;
					float? nodeTravelSpeed = null;
					FleetRangeData fleetRangeData;
					if (fleetRanges.TryGetValue(current2, out fleetRangeData))
					{
						new float?(fleetRangeData.FleetRange);
						travelSpeed = fleetRangeData.FleetTravelSpeed;
						nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
					}
					FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current2.ID, this._player.ID, this._player.Faction.Name == "loa");
                    if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN && Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this._game, current2.ID) >= 1f)
					{
                        MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.UPGRADE_STN, current.DesignInfo.StationType, current2.ID, system.ID, current.OrbitalObjectID, null, current.DesignInfo.StationLevel + 1, false, travelSpeed, nodeTravelSpeed);
						strategicTask.UseableFleets.Add(new FleetManagement
						{
							Fleet = current2,
							MissionTime = missionEstimate,
							FleetTypes = fleetTypeFlags
						});
					}
				}
				list.Add(strategicTask);
			}
			return list;
		}
		private StrategicTask GetPotentialRelocateTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			if (this.m_AvailableFullFleets.Count + this.m_AvailableShortFleets.Count == 0)
			{
				return null;
			}
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.RELOCATION;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.ANY;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			strategicTask.SupportPointsAtSystem = this._db.GetRemainingSupportPoints(this._game, system.ID, this._player.ID);
			List<FleetInfo> list = new List<FleetInfo>();
			list.AddRange(this.m_AvailableFullFleets);
			list.AddRange(this.m_AvailableShortFleets);
			foreach (FleetInfo current in list)
			{
				float? travelSpeed = null;
				float? nodeTravelSpeed = null;
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(current, out fleetRangeData))
				{
					new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current.ID, this._player.ID, this._player.Faction.Name == "loa");
                if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN && Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(this._game, system.ID, current.ID))
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.RELOCATION, StationType.INVALID_TYPE, current.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement
					{
						Fleet = current,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}
		private StrategicTask GetPotentialPatrolTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			if (!this._db.CanPatrol(this._player.ID, system.ID))
			{
				return null;
			}
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.PATROL;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.PATROL;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo current in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = null;
				float? nodeTravelSpeed = null;
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(current, out fleetRangeData))
				{
					new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.PATROL, StationType.INVALID_TYPE, current.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement
					{
						Fleet = current,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}
		private StrategicTask GetPotentialInterdictTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			if (!this._db.CanInterdict(this._player.ID, system.ID))
			{
				return null;
			}
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.INTERDICTION;
			strategicTask.RequiredFleetTypes = (FleetTypeFlags)5;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			foreach (FleetInfo current in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = null;
				float? nodeTravelSpeed = null;
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(current, out fleetRangeData))
				{
					new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.INTERDICTION, StationType.INVALID_TYPE, current.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement
					{
						Fleet = current,
						MissionTime = missionEstimate,
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}
		private StrategicTask GetPotentialInterceptTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			return null;
		}
		private StrategicTask GetPotentialStrikeTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			if (!this._db.CanStrike(this._player.ID, system.ID) || !hostilesAtSystem)
			{
				return null;
			}
			if (this.m_FleetsInSurveyRange.Count == 0)
			{
				return null;
			}
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.STRIKE;
			strategicTask.RequiredFleetTypes = FleetTypeFlags.COMBAT;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			strategicTask.EnemyStrength = this.GetEnemyFleetStrength(system.ID);
			List<FleetInfo> list = this._game.GameDatabase.GetFleetInfoBySystemID(system.ID, FleetType.FL_NORMAL | FleetType.FL_DEFENSE).ToList<FleetInfo>();
			strategicTask.NumStandardPlayersAtSystem = this.GetNumStandardPlayersAtSystem(strategicTask, list);
			strategicTask.EasterEggsAtSystem = this.GetEncountersAtSystem(system.ID, (
				from x in list
				where x.Type == FleetType.FL_NORMAL
				select x).ToList<FleetInfo>());
			foreach (FleetInfo current in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = null;
				float? nodeTravelSpeed = null;
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(current, out fleetRangeData))
				{
					new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.STRIKE, StationType.INVALID_TYPE, current.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement
					{
						Fleet = current,
						MissionTime = missionEstimate,
						FleetStrength = this.GetFleetStrength(current),
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}
		private StrategicTask GetPotentialInvadeTaskForSystem(StarSystemInfo system, Dictionary<FleetInfo, FleetRangeData> fleetRanges, bool hostilesAtSystem)
		{
			if (!this._db.CanInvade(this._player.ID, system.ID) || !hostilesAtSystem || StarSystemDetailsUI.CollectPlanetListItemsForInvasionMission(this._game.App, system.ID).Count<int>() == 0)
			{
				return null;
			}
			StrategicTask strategicTask = new StrategicTask();
			strategicTask.Mission = MissionType.INVASION;
			strategicTask.RequiredFleetTypes = (FleetTypeFlags)3;
			strategicTask.SystemIDTarget = system.ID;
			strategicTask.HostilesAtSystem = hostilesAtSystem;
			strategicTask.EnemyStrength = this.GetEnemyFleetStrength(system.ID);
			foreach (FleetInfo current in this.m_FleetsInSurveyRange)
			{
				float? travelSpeed = null;
				float? nodeTravelSpeed = null;
				FleetRangeData fleetRangeData;
				if (fleetRanges.TryGetValue(current, out fleetRangeData))
				{
					new float?(fleetRangeData.FleetRange);
					travelSpeed = fleetRangeData.FleetTravelSpeed;
					nodeTravelSpeed = fleetRangeData.FleetNodeTravelSpeed;
				}
				FleetTypeFlags fleetTypeFlags = FleetManagement.GetFleetTypeFlags(this._game.App, current.ID, this._player.ID, this._player.Faction.Name == "loa");
				if ((strategicTask.RequiredFleetTypes & fleetTypeFlags) != FleetTypeFlags.UNKNOWN)
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.INVASION, StationType.INVALID_TYPE, current.ID, system.ID, 0, null, 1, false, travelSpeed, nodeTravelSpeed);
					strategicTask.UseableFleets.Add(new FleetManagement
					{
						Fleet = current,
						MissionTime = missionEstimate,
						FleetStrength = this.GetFleetStrength(current),
						FleetTypes = fleetTypeFlags
					});
				}
			}
			return strategicTask;
		}
		private int GetScoreForTask(StrategicTask task, AIStance stance)
		{
			switch (task.Mission)
			{
			case MissionType.COLONIZATION:
				return this.ScoreTaskForColonize(task, stance);
			case MissionType.SUPPORT:
				return this.ScoreTaskForSupport(task, stance);
			case MissionType.SURVEY:
				return this.ScoreTaskForSurvey(task, stance);
			case MissionType.RELOCATION:
				return this.ScoreTaskForRelocate(task, stance);
			case MissionType.CONSTRUCT_STN:
				return this.ScoreTaskForConstruction(task, stance);
			case MissionType.UPGRADE_STN:
				return this.ScoreTaskForUpgrade(task, stance);
			case MissionType.PATROL:
				return this.ScoreTaskForPatrol(task, stance);
			case MissionType.INTERDICTION:
				return this.ScoreTaskForInterdict(task, stance);
			case MissionType.STRIKE:
				return this.ScoreTaskForStrike(task, stance);
			case MissionType.INVASION:
				return this.ScoreTaskForInvade(task, stance);
			case MissionType.INTERCEPT:
				return this.ScoreTaskForIntercept(task, stance);
			case MissionType.GATE:
				return this.ScoreTaskForGate(task, stance);
			case MissionType.DEPLOY_NPG:
				return this.ScoreTaskForNPG(task, stance);
			}
			return 0;
		}
		private int ScoreTaskForSurvey(StrategicTask task, AIStance stance)
		{
			if (task.HostilesAtSystem)
			{
				return 0;
			}
			if (!task.UseableFleets.Any((FleetManagement x) => x.Fleet.SystemID == task.SystemIDTarget))
			{
				if (this._player.Faction.CanUseAccelerators())
				{
					if (!this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
					{
						if (task.UseableFleets.Count != 0)
						{
							if (task.UseableFleets.Min((FleetManagement x) => x.MissionTime.TurnsToTarget) <= 5)
							{
								goto IL_EF;
							}
						}
						return 0;
					}
				}
				else
				{
					if (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
					{
						return 0;
					}
				}
			}
			IL_EF:
			if (this._player.Faction.CanUseNodeLine(new bool?(true)))
			{
				bool flag = false;
				List<NodeLineInfo> list = (
					from x in this._db.GetNodeLines()
					where x.IsPermenant && !x.IsLoaLine && (x.System1ID == task.SystemIDTarget || x.System2ID == task.SystemIDTarget)
					select x).ToList<NodeLineInfo>();
				foreach (NodeLineInfo arg_144_0 in list)
				{
					if (this._db.GetExploredNodeLinesFromSystem(this._player.ID, task.SystemIDTarget).Count<NodeLineInfo>() > 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return 0;
				}
			}
			switch (stance)
			{
			case AIStance.CONQUERING:
				return 3;
			case AIStance.DESTROYING:
				return 3;
			case AIStance.DEFENDING:
				return 2;
			default:
				return 2;
			}
		}
		private int ScoreTaskForGate(StrategicTask task, AIStance stance)
		{
			if (this._db.SystemHasGate(task.SystemIDTarget, this._player.ID) || task.EasterEggsAtSystem.Any<EasterEgg>())
			{
				return 0;
			}
			switch (stance)
			{
			case AIStance.CONQUERING:
				return 6;
			case AIStance.DESTROYING:
				return 5;
			case AIStance.DEFENDING:
				return 3;
			default:
				return 5;
			}
		}
		private int ScoreTaskForNPG(StrategicTask task, AIStance stance)
		{
			if (this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID) || task.EasterEggsAtSystem.Any<EasterEgg>())
			{
				return 0;
			}
			switch (stance)
			{
			case AIStance.CONQUERING:
				return 6;
			case AIStance.DESTROYING:
				return 5;
			case AIStance.DEFENDING:
				return 3;
			default:
				return 5;
			}
		}
		private int ScoreTaskForColonize(StrategicTask task, AIStance stance)
		{
			if (task.HostilesAtSystem || this.HaveNewColonizationMissionEnroute())
			{
				return 0;
			}
			if (!task.UseableFleets.Any((FleetManagement x) => x.Fleet.SystemID == task.SystemIDTarget))
			{
				if (this._player.Faction.CanUseAccelerators())
				{
					if (!this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
					{
						if (task.UseableFleets.Count != 0)
						{
							if (task.UseableFleets.Min((FleetManagement x) => x.MissionTime.TurnsToTarget) <= 5)
							{
								goto IL_F5;
							}
						}
						return 0;
					}
				}
				else
				{
					if (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
					{
						return 0;
					}
				}
			}
			IL_F5:
			double colonySupportCost = Colony.GetColonySupportCost(this._db, this._player.ID, task.PlanetIDTarget);
			if (colonySupportCost > 0.0 && colonySupportCost > this.GetAvailableColonySupportBudget())
			{
				return 0;
			}
			switch (stance)
			{
			case AIStance.CONQUERING:
				return 2;
			case AIStance.DESTROYING:
				return 2;
			case AIStance.DEFENDING:
				return 1;
			default:
				return 3;
			}
		}
		private int ScoreTaskForSupport(StrategicTask task, AIStance stance)
		{
			if (!this._db.CanSupportPlanet(this._player.ID, task.PlanetIDTarget))
			{
				return 0;
			}
			return this.ScoreTaskForColonize(task, stance) + 1;
		}
		private float GetStationShortFall(AIStance stance, StationType stationType)
		{
			float[] array = new float[8];
			if (this._player.Faction.CanUseGate())
			{
				array[5] = 1f;
			}
			else
			{
				array[5] = 0f;
			}
			switch (stance)
			{
			case AIStance.EXPANDING:
				array[1] = 0.4f;
				array[3] = 1f;
				array[2] = 0.15f;
				array[4] = 0f;
				break;
			case AIStance.ARMING:
				array[1] = 0.8f;
				array[3] = 1f;
				array[2] = 0.3f;
				array[4] = 0.15f;
				break;
			case AIStance.HUNKERING:
				array[1] = 1f;
				array[3] = 0.8f;
				array[2] = 0.6f;
				array[4] = 0.2f;
				break;
			case AIStance.CONQUERING:
				array[1] = 1f;
				array[3] = 0.5f;
				array[2] = 0.3f;
				array[4] = 0.15f;
				break;
			case AIStance.DESTROYING:
				array[1] = 1f;
				array[3] = 0.6f;
				array[2] = 0.2f;
				array[4] = 0f;
				break;
			case AIStance.DEFENDING:
				array[1] = 1f;
				array[3] = 0.1f;
				array[2] = 0.3f;
				array[4] = 0.1f;
				break;
			}
			int count = this.m_ColonizedSystems.Count;
			float num = (float)this.m_NumStations[(int)stationType] / (float)count;
			if (array[(int)stationType] <= 0f)
			{
				return 0f;
			}
			return (array[(int)stationType] - num) / array[(int)stationType];
		}
		private int ScoreTaskForConstruction(StrategicTask task, AIStance stance)
		{
			if (task.HostilesAtSystem)
			{
				return 0;
			}
			if (!task.UseableFleets.Any((FleetManagement x) => x.Fleet.SystemID == task.SystemIDTarget))
			{
				if (this._player.Faction.CanUseAccelerators())
				{
					if (!this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
					{
						return 0;
					}
				}
				else
				{
					if (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
					{
						return 0;
					}
				}
			}
			int num = 0;
			float num2 = 1f;
			if (task.StationType == StationType.SCIENCE && this._db.GetStationForSystemPlayerAndType(task.SystemIDTarget, this._player.ID, StationType.NAVAL) == null)
			{
				return 0;
			}
			if (task.StationType == StationType.DIPLOMATIC)
			{
				List<StationInfo> list = this._db.GetStationForSystemAndPlayer(task.SystemIDTarget, this._player.ID).ToList<StationInfo>();
				if (list.Count == 0)
				{
					return 0;
				}
				if (this._player.Faction.Name == "zuul")
				{
					if (list.Count((StationInfo x) => x.DesignInfo.StationType == StationType.NAVAL) == 0)
					{
						return 0;
					}
					if (this._db.GetStationInfosByPlayerID(this._player.ID).Count((StationInfo x) => x.DesignInfo.StationType == StationType.DIPLOMATIC) > 3)
					{
						return 0;
					}
					num2 = 0.25f;
					num = 2;
				}
				else
				{
					if (list.Count((StationInfo x) => x.DesignInfo.StationType == StationType.NAVAL && x.DesignInfo.StationLevel >= 2) == 0)
					{
						return 0;
					}
				}
			}
			if (this.GetAvailableStationSupportBudget() < (double)((float)GameSession.CalculateLVL1StationUpkeepCost(this._game.AssetDatabase, task.StationType) * num2))
			{
				return 0;
			}
			if (this.GetStationShortFall(stance, task.StationType) <= 0f)
			{
				return 0;
			}
			switch (stance)
			{
			case AIStance.HUNKERING:
				return 1 + num;
			case AIStance.CONQUERING:
				return 1 + num;
			case AIStance.DESTROYING:
				return 1 + num;
			case AIStance.DEFENDING:
				return 1 + num;
			default:
				return 4 + num;
			}
		}
		private int ScoreTaskForUpgrade(StrategicTask task, AIStance stance)
		{
			if (task.HostilesAtSystem)
			{
				return 0;
			}
			if (!task.UseableFleets.Any((FleetManagement x) => x.Fleet.SystemID == task.SystemIDTarget))
			{
				if (this._player.Faction.CanUseAccelerators())
				{
					if (!this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
					{
						return 0;
					}
				}
				else
				{
					if (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
					{
						return 0;
					}
				}
			}
			StationInfo stationInfo = this._db.GetStationInfo(task.StationIDTarget);
			if (stationInfo == null || !this._game.StationIsUpgradable(stationInfo))
			{
				return 0;
			}
			double num = 1.0;
			if (this._player.Faction.Name != "zuul" || stationInfo.DesignInfo.StationType != StationType.DIPLOMATIC)
			{
				if (stationInfo.DesignInfo.StationLevel > 3 && this._db.GetTurnCount() <= 100)
				{
					return 0;
				}
			}
			else
			{
				if (this._player.Faction.Name == "zuul" && stationInfo.DesignInfo.StationType == StationType.DIPLOMATIC)
				{
					num = 0.05;
				}
			}
			if (this.GetAvailableStationSupportBudget() < (double)GameSession.CalculateStationUpkeepCost(this._db, this._db.AssetDatabase, stationInfo) * num && (this._player.Faction.Name != "hiver" || stationInfo.DesignInfo.StationType != StationType.GATE || this.m_TurnBudget.ProjectedSavings < 500000.0))
			{
				return 0;
			}
			switch (stance)
			{
			case AIStance.HUNKERING:
				return 2;
			case AIStance.CONQUERING:
				return 2;
			case AIStance.DESTROYING:
				return 2;
			case AIStance.DEFENDING:
				return 2;
			default:
				return 2;
			}
		}
		private int ScoreTaskForPatrol(StrategicTask task, AIStance stance)
		{
			if (this.m_ColonizedSystems.Any((SystemDefendInfo x) => x.SystemID == task.SystemIDTarget))
			{
				if (task.EasterEggsAtSystem.Any((EasterEgg x) => x == EasterEgg.EE_SWARM))
				{
					return 6;
				}
			}
			switch (stance)
			{
			case AIStance.CONQUERING:
				return 3;
			case AIStance.DESTROYING:
				return 3;
			case AIStance.DEFENDING:
				return 4;
			default:
				return 0;
			}
		}
		private int ScoreTaskForRelocate(StrategicTask task, AIStance stance)
		{
			return 0;
		}
		private int ScoreTaskForInterdict(StrategicTask task, AIStance stance)
		{
			return 0;
		}
		private int ScoreTaskForIntercept(StrategicTask task, AIStance stance)
		{
			return 0;
		}
		private int ScoreTaskForStrike(StrategicTask task, AIStance stance)
		{
			if (task.NumStandardPlayersAtSystem == 0)
			{
				if (task.EasterEggsAtSystem.Any((EasterEgg x) => x == EasterEgg.EE_SWARM || x == EasterEgg.EE_MORRIGI_RELIC || x == EasterEgg.EE_ASTEROID_MONITOR || x == EasterEgg.EE_GARDENERS || x == EasterEgg.EE_PIRATE_BASE))
				{
					return 5;
				}
			}
			switch (stance)
			{
			case AIStance.CONQUERING:
				return 4;
			case AIStance.DESTROYING:
				return 4;
			case AIStance.DEFENDING:
				return 2;
			default:
				return 0;
			}
		}
		private int ScoreTaskForInvade(StrategicTask task, AIStance stance)
		{
			switch (stance)
			{
			case AIStance.CONQUERING:
				return 5;
			case AIStance.DESTROYING:
				return 4;
			case AIStance.DEFENDING:
				return 2;
			default:
				return 0;
			}
		}
		private int GetSubScoreForTask(StrategicTask task, AIStance stance)
		{
			switch (task.Mission)
			{
			case MissionType.COLONIZATION:
				return this.SubScoreTaskForColonize(task, stance);
			case MissionType.SUPPORT:
				return this.SubScoreTaskForSupport(task, stance);
			case MissionType.SURVEY:
				return this.SubScoreTaskForSurvey(task, stance);
			case MissionType.RELOCATION:
				return this.SubScoreTaskForRelocate(task, stance);
			case MissionType.CONSTRUCT_STN:
				return this.SubScoreTaskForConstruction(task, stance);
			case MissionType.UPGRADE_STN:
				return this.SubScoreTaskForUpgrade(task, stance);
			case MissionType.PATROL:
				return this.SubScoreTaskForPatrol(task, stance);
			case MissionType.INTERDICTION:
				return this.SubScoreTaskForInterdict(task, stance);
			case MissionType.STRIKE:
				return this.SubScoreTaskForStrike(task, stance);
			case MissionType.INVASION:
				return this.SubScoreTaskForInvade(task, stance);
			case MissionType.INTERCEPT:
				return this.SubScoreTaskForIntercept(task, stance);
			case MissionType.GATE:
				return this.SubScoreTaskForGate(task, stance);
			case MissionType.DEPLOY_NPG:
				return this.SubScoreTaskForNPG(task, stance);
			}
			return 0;
		}
		private int SubScoreTaskForSurvey(StrategicTask task, AIStance stance)
		{
			if (task.UseableFleets.Count == 0)
			{
				return -10000;
			}
			double num = 1.0;
			if (task.Mission != MissionType.GATE && this._player.Faction.CanUseGate() && this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
			{
				num = 0.5;
			}
			else
			{
				if (task.Mission != MissionType.DEPLOY_NPG && this._player.Faction.CanUseAccelerators() && this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
				{
					num = 0.5;
				}
				else
				{
					if (!this._player.Faction.CanUseGate() && !this._player.Faction.CanUseAccelerators())
					{
						List<int> list = StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this._game.App, task.SystemIDTarget, this._player.ID).ToList<int>();
						if (list.Count == 0)
						{
							num += 0.5;
						}
						if (stance == AIStance.EXPANDING && this._db.GetColonyInfosForSystem(task.SystemIDTarget).Count<ColonyInfo>() > 0)
						{
							num += 1.0;
						}
					}
				}
			}
			Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(task.SystemIDTarget);
			float num2 = 3.40282347E+38f;
			foreach (SystemDefendInfo current in this.m_ColonizedSystems)
			{
				num2 = Math.Min((this._db.GetStarSystemOrigin(current.SystemID) - starSystemOrigin).LengthSquared, num2);
			}
			return -(int)(Math.Sqrt((double)num2) * num);
		}
		private int SubScoreTaskForGate(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForSurvey(task, stance);
		}
		private int SubScoreTaskForNPG(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForSurvey(task, stance);
		}
		private int SubScoreTaskForColonize(StrategicTask task, AIStance stance)
		{
			float num = 1f;
			if (this._player.Faction.Name != "hiver" && (stance == AIStance.EXPANDING || stance == AIStance.ARMING || stance == AIStance.HUNKERING || stance == AIStance.CONQUERING))
			{
				if (this._db.GetColonyInfosForSystem(task.SystemIDTarget).Count<ColonyInfo>() == 0)
				{
					num = ((this._player.Faction.Name == "loa") ? 4f : 2f);
				}
			}
			else
			{
				if (stance == AIStance.DEFENDING && this._db.GetColonyInfosForSystem(task.SystemIDTarget).Count<ColonyInfo>() > 0)
				{
					num = 2f;
				}
			}
			List<CombatData> list = this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 3).ToList<CombatData>();
			if (list.Count > 0)
			{
				num = 0.25f;
			}
			float num2 = 0f;
			int stratModifier = this._db.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this._player.ID);
			PlanetInfo planetInfo = this._db.GetPlanetInfo(task.PlanetIDTarget);
			if (planetInfo != null && StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
			{
				if (this._player.Faction.Name == "loa")
				{
					float loaGrowthPotential = Colony.GetLoaGrowthPotential(this._game, planetInfo.ID, task.SystemIDTarget, this._player.ID);
					num2 = loaGrowthPotential;
				}
				else
				{
					int num3 = (int)Math.Round((double)this._db.GetPlanetHazardRating(this._player.ID, planetInfo.ID, true));
					num2 = (float)(stratModifier - num3) / (float)stratModifier;
				}
			}
			int num4 = 0;
			int num5 = 0;
			foreach (FleetManagement current in task.UseableFleets)
			{
				if (current.Score > 0)
				{
					num5 += current.MissionTime.TotalTurns;
					num4++;
				}
			}
			if (num4 > 0)
			{
				num5 = (int)Math.Round((double)((float)num5 / (float)num4));
			}
			float num6 = 50f * num2 * num;
			float num7 = 50f * (1f - Math.Min((float)num5 / 20f, 1f));
			return (int)(num7 + num6);
		}
		private int SubScoreTaskForSupport(StrategicTask task, AIStance stance)
		{
			return 0;
		}
		private int SubScoreTaskForConstruction(StrategicTask task, AIStance stance)
		{
			float num = this.GetStationShortFall(stance, task.StationType) * 100f;
			float num2 = 1f;
			List<StationInfo> source = this._db.GetStationForSystemAndPlayer(task.SystemIDTarget, this._player.ID).ToList<StationInfo>();
			if (source.Count((StationInfo x) => x.DesignInfo.StationType == task.StationType) > 0)
			{
				num2 -= 0.5f;
			}
			List<CombatData> list = this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 3).ToList<CombatData>();
			if (list.Count > 0)
			{
				num2 -= 0.5f;
			}
			switch (task.StationType)
			{
			case StationType.NAVAL:
			{
				List<int> list2 = StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this._game.App, task.SystemIDTarget, this._player.ID).ToList<int>();
				List<ColonyInfo> list3 = (
					from x in this._db.GetColonyInfosForSystem(task.SystemIDTarget)
					where x.PlayerID == this._player.ID
					select x).ToList<ColonyInfo>();
				int num3 = list2.Count + list3.Count;
				num2 += Math.Min((float)num3 / 3f, 1f);
				break;
			}
			case StationType.CIVILIAN:
			{
				int num4 = 0;
				float num5 = 0f;
				int stratModifier = this._db.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this._player.ID);
				List<PlanetInfo> list4 = this._db.GetPlanetInfosOrbitingStar(task.SystemIDTarget).ToList<PlanetInfo>();
				foreach (PlanetInfo current in list4)
				{
					if (StellarBodyTypes.IsTerrestrial(current.Type.ToLowerInvariant()))
					{
						if (this._player.Faction.Name == "loa")
						{
							float loaGrowthPotential = Colony.GetLoaGrowthPotential(this._game, current.ID, task.SystemIDTarget, this._player.ID);
							num5 += loaGrowthPotential;
						}
						else
						{
							int num6 = (int)Math.Round((double)this._db.GetPlanetHazardRating(this._player.ID, current.ID, true));
							num5 += (float)(stratModifier - num6) / (float)stratModifier;
						}
						num4++;
					}
				}
				if (num4 > 0)
				{
					num2 += num5 / (float)num4;
				}
				if (source.Count((StationInfo x) => x.DesignInfo.StationType == StationType.NAVAL) > 0)
				{
					if (source.Count((StationInfo x) => x.DesignInfo.StationType == task.StationType) == 0)
					{
						num2 += 0.5f;
					}
				}
				if (this.m_TurnBudget.CurrentSavings < 1500000.0)
				{
					num2 += 1f;
				}
				if (this.m_TurnBudget.NetSavingsIncome < this.m_TurnBudget.NetSavingsLoss)
				{
					num2 += 0.5f;
				}
				break;
			}
			case StationType.DIPLOMATIC:
				if (this._player.Faction.Name == "zuul")
				{
					num2 += 100f;
				}
				else
				{
					num2 += 1f;
				}
				break;
			}
			return (int)(num * num2);
		}
		private int SubScoreTaskForUpgrade(StrategicTask task, AIStance stance)
		{
			StationInfo stationInfo = this._db.GetStationInfo(task.StationIDTarget);
			if (stationInfo == null)
			{
				return -10000000;
			}
            float num = -(float)Kerberos.Sots.StarFleet.StarFleet.GetStationConstructionCost(this._game, stationInfo.DesignInfo.StationType, this._player.Faction.Name, stationInfo.DesignInfo.StationLevel + 1);
			if (task.StationType == StationType.NAVAL)
			{
				num *= 0.75f;
			}
			else
			{
				if (task.StationType == StationType.GATE)
				{
					num *= 0.8f;
				}
				else
				{
					if (task.StationType == StationType.CIVILIAN)
					{
						if (this.m_TurnBudget.NetSavingsIncome < this.m_TurnBudget.NetSavingsLoss)
						{
							num *= 0.3f;
						}
						else
						{
							num *= 0.8f;
						}
					}
					else
					{
						if (this._player.Faction.Name == "zuul" && task.StationType == StationType.DIPLOMATIC)
						{
							num *= 0.005f;
						}
					}
				}
			}
			return (int)num;
		}
		private int SubScoreTaskForPatrol(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForInvade(task, stance);
		}
		private int SubScoreTaskForRelocate(StrategicTask task, AIStance stance)
		{
			return 0;
		}
		private int SubScoreTaskForInterdict(StrategicTask task, AIStance stance)
		{
			return 0;
		}
		private int SubScoreTaskForIntercept(StrategicTask task, AIStance stance)
		{
			return 0;
		}
		private int SubScoreTaskForStrike(StrategicTask task, AIStance stance)
		{
			return this.SubScoreTaskForInvade(task, stance);
		}
		private int SubScoreTaskForInvade(StrategicTask task, AIStance stance)
		{
			if (!task.UseableFleets.Any((FleetManagement x) => x.Fleet.SystemID == task.SystemIDTarget))
			{
				if (this._player.Faction.CanUseAccelerators())
				{
					if (!this._db.SystemHasAccelerator(task.SystemIDTarget, this._player.ID))
					{
						if (task.UseableFleets.Count != 0)
						{
							if (task.UseableFleets.Min((FleetManagement x) => x.MissionTime.TurnsToTarget) <= 5)
							{
								goto IL_DE;
							}
						}
						return 0;
					}
				}
				else
				{
					if (this._player.Faction.CanUseGate() && !this._db.SystemHasGate(task.SystemIDTarget, this._player.ID))
					{
						return 0;
					}
				}
			}
			IL_DE:
			if (task.EnemyStrength <= 0)
			{
				return 2;
			}
			if (task.EasterEggsAtSystem.Contains(EasterEgg.GM_SYSTEM_KILLER))
			{
				return -(int)((float)task.EnemyStrength * 2f);
			}
			if (task.EasterEggsAtSystem.Contains(EasterEgg.GM_LOCUST_SWARM))
			{
				return -(int)((float)task.EnemyStrength * 1.25f);
			}
			if (task.EasterEggsAtSystem.Any((EasterEgg x) => x == EasterEgg.EE_SWARM || x == EasterEgg.EE_MORRIGI_RELIC || x == EasterEgg.EE_ASTEROID_MONITOR || x == EasterEgg.EE_GARDENERS || x == EasterEgg.EE_PIRATE_BASE))
			{
				return 1;
			}
			return -task.EnemyStrength;
		}
		private int GetScoreForFleet(StrategicTask task, FleetManagement fleet)
		{
			if ((task.RequiredFleetTypes & fleet.FleetTypes) == FleetTypeFlags.UNKNOWN || this.m_AvailableShortFleets.Any((FleetInfo x) => x == fleet.Fleet) || this.m_DefenseCombatFleets.Contains(fleet.Fleet) || fleet.MissionTime.TurnsToTarget > 20)
			{
				return 0;
			}
			switch (task.Mission)
			{
			case MissionType.COLONIZATION:
				return this.ScoreFleetForColonize(task, fleet);
			case MissionType.SUPPORT:
				return this.ScoreFleetForSupport(task, fleet);
			case MissionType.SURVEY:
				return this.ScoreFleetForSurvey(task, fleet);
			case MissionType.RELOCATION:
				return this.ScoreFleetForRelocate(task, fleet);
			case MissionType.CONSTRUCT_STN:
				return this.ScoreFleetForConstruction(task, fleet);
			case MissionType.UPGRADE_STN:
				return this.ScoreFleetForUpgrade(task, fleet);
			case MissionType.PATROL:
				return this.ScoreFleetForPatrol(task, fleet);
			case MissionType.INTERDICTION:
				return this.ScoreFleetForInterdict(task, fleet);
			case MissionType.STRIKE:
				return this.ScoreFleetForStrike(task, fleet);
			case MissionType.INVASION:
				return this.ScoreFleetForInvade(task, fleet);
			case MissionType.INTERCEPT:
				return this.ScoreFleetForIntercept(task, fleet);
			case MissionType.GATE:
				return this.ScoreFleetForGate(task, fleet);
			case MissionType.DEPLOY_NPG:
				return this.ScoreFleetForNPG(task, fleet);
			}
			return 0;
		}
		private int ScoreFleetForSurvey(StrategicTask task, FleetManagement fleet)
		{
			if (fleet.MissionTime.TurnsAtTarget == 0)
			{
				return 1;
			}
			return -fleet.MissionTime.TurnsToTarget;
		}
		private int ScoreFleetForGate(StrategicTask task, FleetManagement fleet)
		{
			return -fleet.MissionTime.TurnsToTarget;
		}
		private int ScoreFleetForNPG(StrategicTask task, FleetManagement fleet)
		{
			return -fleet.MissionTime.TurnsToTarget;
		}
		private int ScoreFleetForColonize(StrategicTask task, FleetManagement fleet)
		{
			float num = 60f * (1f - Math.Min((float)fleet.MissionTime.TotalTurns / 25f, 1f));
            float num2 = 40f * Math.Min((float)Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(this._game, fleet.Fleet.ID) / 500f, 1f);
			return (int)Math.Round((double)(num + num2));
		}
		private int ScoreFleetForSupport(StrategicTask task, FleetManagement fleet)
		{
            return (int)Math.Round(Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(this._game, fleet.Fleet.ID));
		}
		private int ScoreFleetForConstruction(StrategicTask task, FleetManagement fleet)
		{
            return (int)Math.Round((double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this._game, fleet.Fleet.ID));
		}
		private int ScoreFleetForUpgrade(StrategicTask task, FleetManagement fleet)
		{
            return (int)Math.Round((double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(this._game, fleet.Fleet.ID));
		}
		private int ScoreFleetForPatrol(StrategicTask task, FleetManagement fleet)
		{
			return fleet.FleetStrength;
		}
		private int ScoreFleetForRelocate(StrategicTask task, FleetManagement fleet)
		{
			return fleet.FleetStrength;
		}
		private int ScoreFleetForInterdict(StrategicTask task, FleetManagement fleet)
		{
			return fleet.FleetStrength;
		}
		private int ScoreFleetForIntercept(StrategicTask task, FleetManagement fleet)
		{
            float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, fleet.Fleet.ID, false);
            float fleetTravelSpeed2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, fleet.Fleet.ID, true);
			float num;
			if (fleetTravelSpeed2 > 0f)
			{
				num = fleetTravelSpeed2;
			}
			else
			{
				num = fleetTravelSpeed;
			}
			return fleet.FleetStrength + (int)num;
		}
		private int ScoreFleetForStrike(StrategicTask task, FleetManagement fleet)
		{
			if ((fleet.FleetTypes & FleetTypeFlags.SURVEY) != FleetTypeFlags.UNKNOWN && this.m_AvailableTasks.Count((StrategicTask x) => x.Mission == MissionType.SURVEY && x.UseableFleets.Any((FleetManagement y) => y.Fleet == fleet.Fleet)) > 0)
			{
				List<AIFleetInfo> source = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
				AIFleetInfo aiFleet = source.FirstOrDefault((AIFleetInfo x) => x.FleetID == fleet.Fleet.ID);
				if (aiFleet == null)
				{
					return 0;
				}
				this._db.GetAIInfo(this._player.ID);
				int num = this._game.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == aiFleet.FleetTemplate).MinFleetsForStance[this._db.GetAIInfo(this._player.ID).Stance];
				num = (int)Math.Ceiling((double)num / 2.0);
				if (source.Count((AIFleetInfo x) => x.FleetTemplate == aiFleet.FleetTemplate) < num)
				{
					return 0;
				}
			}
			return fleet.FleetStrength;
		}
		private int ScoreFleetForInvade(StrategicTask task, FleetManagement fleet)
		{
			return fleet.FleetStrength;
		}
		private bool IsSimilarMission(MissionType mt1, MissionType mt2)
		{
			if (mt1 == mt2)
			{
				return true;
			}
			if (mt1 == MissionType.CONSTRUCT_STN)
			{
				return mt2 == MissionType.UPGRADE_STN;
			}
			return mt1 == MissionType.UPGRADE_STN && mt2 == MissionType.CONSTRUCT_STN;
		}
		private StrategicTask PickBestTaskForFleet(FleetInfo fleet, List<int> fleetsAssigned)
		{
			int num = 2147483647;
			StrategicTask strategicTask = null;
			foreach (StrategicTask current in this.m_AvailableTasks)
			{
				FleetManagement fleetManagement = current.UseableFleets.FirstOrDefault((FleetManagement x) => x.Fleet == fleet);
				if (current.NumFleetsRequested > 0 && current.UseableFleets.Count >= current.NumFleetsRequested && fleetManagement != null)
				{
					FleetManagement fleetManagement2 = current.UseableFleets.FirstOrDefault((FleetManagement x) => !fleetsAssigned.Any((int y) => y == x.Fleet.ID));
					if (fleetManagement2 == null || fleetManagement2.Score <= fleetManagement.Score)
					{
						bool flag = strategicTask == null || strategicTask.Score < current.Score;
						bool flag2 = strategicTask != null && this.IsSimilarMission(strategicTask.Mission, current.Mission);
						bool flag3 = strategicTask != null && strategicTask.SubScore < current.SubScore;
						bool flag4 = strategicTask != null && strategicTask.SubScore == current.SubScore && fleetManagement.MissionTime.TurnsToTarget < num;
						if (flag || (flag2 && (flag3 || flag4)))
						{
							strategicTask = current;
							int arg_142_0 = fleetManagement.Score;
							num = fleetManagement.MissionTime.TurnsToTarget;
						}
					}
				}
			}
			return strategicTask;
		}
		private StrategicTask PickBestRelocationTaskForFleet(FleetInfo fleet, StrategicTask desiredTask = null)
		{
			if (this.m_RelocationTasks.Count == 0 || fleet.SupportingSystemID == 0)
			{
				return null;
			}
			FleetTypeFlags fleetFlags = FleetManagement.GetFleetTypeFlags(this._game.App, fleet.ID, this._player.ID, this._player.Faction.Name == "loa");
			int fleetCruiserEquivalent = this._db.GetFleetCruiserEquivalent(fleet.ID);
			StrategicTask strategicTask = null;
			if (desiredTask == null)
			{
				Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(fleet.SupportingSystemID);
				float num = GameSession.GetSupportRange(this._db.AssetDatabase, this._db, this._player.ID);
				num *= num;
				int num2 = 0;
				if (!this.m_AvailableShortFleets.Contains(fleet))
				{
					foreach (StrategicTask current in this.m_AvailableTasks)
					{
						if ((current.RequiredFleetTypes & fleetFlags) != FleetTypeFlags.UNKNOWN)
						{
							num2++;
						}
					}
				}
				float num3 = 3.40282347E+38f;
				using (Dictionary<StrategicTask, List<StrategicTask>>.Enumerator enumerator2 = this.m_RelocationTasks.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						KeyValuePair<StrategicTask, List<StrategicTask>> current2 = enumerator2.Current;
						if (current2.Key.SystemIDTarget != fleet.SupportingSystemID)
						{
							if (!current2.Key.UseableFleets.Any((FleetManagement x) => x.Fleet == fleet) || fleetCruiserEquivalent > current2.Key.SupportPointsAtSystem)
							{
								continue;
							}
						}
						int num4 = 0;
						if (!this.m_AvailableShortFleets.Contains(fleet))
						{
							num4 = current2.Value.Count((StrategicTask x) => (x.RequiredFleetTypes & fleetFlags) != FleetTypeFlags.UNKNOWN);
							if (num4 == 0)
							{
								continue;
							}
						}
						else
						{
							if (fleet.SystemID == current2.Key.StationIDTarget)
							{
								continue;
							}
						}
						Vector3 starSystemOrigin2 = this._db.GetStarSystemOrigin(current2.Key.SystemIDTarget);
						float lengthSquared = (starSystemOrigin2 - starSystemOrigin).LengthSquared;
						if (num2 == 0 || num4 > num2 + 3 || (num4 == num2 && lengthSquared < num3))
						{
							num3 = lengthSquared;
							num2 = current2.Value.Count;
							strategicTask = current2.Key;
						}
					}
					goto IL_3B1;
				}
			}
			Vector3 starSystemOrigin3 = this._db.GetStarSystemOrigin(desiredTask.SystemIDTarget);
			Vector3 starSystemOrigin4 = this._db.GetStarSystemOrigin(fleet.SupportingSystemID);
			float num5 = (starSystemOrigin3 - starSystemOrigin4).LengthSquared;
			foreach (KeyValuePair<StrategicTask, List<StrategicTask>> current3 in this.m_RelocationTasks)
			{
				if (current3.Key.UseableFleets.Any((FleetManagement x) => x.Fleet == fleet) && fleetCruiserEquivalent <= current3.Key.SupportPointsAtSystem)
				{
					if (current3.Value.Any((StrategicTask x) => x == desiredTask))
					{
						Vector3 starSystemOrigin5 = this._db.GetStarSystemOrigin(current3.Key.SystemIDTarget);
						float lengthSquared2 = (starSystemOrigin5 - starSystemOrigin3).LengthSquared;
						if (lengthSquared2 < num5)
						{
							strategicTask = current3.Key;
							num5 = lengthSquared2;
						}
					}
				}
			}
			IL_3B1:
			if (strategicTask != null && strategicTask.SystemIDTarget == fleet.SupportingSystemID)
			{
				strategicTask = null;
			}
			return strategicTask;
		}
		private void AssignDefenseFleetsToSystem()
		{
			if (this.m_ColonizedSystems.Count == 0 || this.m_DefenseCombatFleets.Count == 0)
			{
				return;
			}
			Dictionary<int, List<FleetInfo>> dictionary = new Dictionary<int, List<FleetInfo>>();
			foreach (SystemDefendInfo current in this.m_ColonizedSystems)
			{
				List<MoveOrderInfo> list = this._db.GetMoveOrdersByDestinationSystem(current.SystemID).ToList<MoveOrderInfo>();
				foreach (MoveOrderInfo current2 in list)
				{
					FleetInfo fleetInfo = this._db.GetFleetInfo(current2.FleetID);
					if (fleetInfo != null && this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, fleetInfo.PlayerID) == DiplomacyState.WAR)
					{
						if (!dictionary.ContainsKey(current.SystemID))
						{
							dictionary.Add(current.SystemID, new List<FleetInfo>());
						}
						if (!dictionary[current.SystemID].Contains(fleetInfo))
						{
							dictionary[current.SystemID].Add(fleetInfo);
						}
					}
				}
				List<FleetInfo> list2 = this._db.GetFleetInfoBySystemID(current.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				foreach (FleetInfo current3 in list2)
				{
					if (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, current3.PlayerID) == DiplomacyState.WAR)
					{
						if (!dictionary.ContainsKey(current.SystemID))
						{
							dictionary.Add(current.SystemID, new List<FleetInfo>());
						}
						if (!dictionary[current.SystemID].Contains(current3))
						{
							dictionary[current.SystemID].Add(current3);
						}
					}
				}
			}
			foreach (SystemDefendInfo sdi in this.m_ColonizedSystems)
			{
				List<FleetInfo> list3;
				if (dictionary.TryGetValue(sdi.SystemID, out list3))
				{
					int num = (
						from x in this.m_DefenseCombatFleets
						where x.SupportingSystemID == sdi.SystemID
						select x).Sum((FleetInfo x) => this.GetFleetStrength(x));
					List<MissionInfo> list4 = (
						from x in this._db.GetMissionsBySystemDest(sdi.SystemID)
						where x.Type == MissionType.RELOCATION
						select x).ToList<MissionInfo>();
					foreach (MissionInfo current4 in list4)
					{
						FleetInfo fleetInfo2 = this._db.GetFleetInfo(current4.FleetID);
						if (fleetInfo2 != null && fleetInfo2.PlayerID == this._player.ID)
						{
							num += this.GetFleetStrength(fleetInfo2);
						}
					}
					this.m_DefenseCombatFleets.RemoveAll((FleetInfo x) => x.SupportingSystemID == sdi.SystemID);
					int num2 = 0;
					int num3 = 2147483647;
					foreach (FleetInfo current5 in list3)
					{
						num2 += this.GetFleetStrength(current5);
						MoveOrderInfo moveOrderInfoByFleetID = this._db.GetMoveOrderInfoByFleetID(current5.ID);
						if (moveOrderInfoByFleetID != null)
						{
							num3 = Math.Min(this._game.GetArrivalTurns(moveOrderInfoByFleetID, current5.ID), num3);
						}
						else
						{
							num3 = 0;
						}
					}
					int num4 = num2 - num2;
					if (num4 <= 0)
					{
						break;
					}
					while (num4 > 0 && this.m_DefenseCombatFleets.Count > 0)
					{
						int num5 = 0;
						int num6 = 2147483647;
						int num7 = 2147483647;
						FleetInfo fleetInfo3 = null;
						foreach (FleetInfo current6 in this.m_DefenseCombatFleets)
						{
							List<FleetInfo> list5;
							if (!dictionary.TryGetValue(current6.SupportingSystemID, out list5) || list5.Count <= 0)
							{
								int fleetStrength = this.GetFleetStrength(current6);
								int num8 = fleetStrength - num4;
								int num9 = Math.Abs(num8);
								if (num9 < Math.Abs(num6) || (num9 == Math.Abs(num6) && num6 < 0 && num8 > 0))
								{
                                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.RELOCATION, StationType.INVALID_TYPE, current6.ID, sdi.SystemID, 0, null, 1, false, null, null);
									if ((num3 == 0 && missionEstimate.TurnsToTarget < num7) || missionEstimate.TurnsToTarget < num3)
									{
										fleetInfo3 = current6;
										num6 = num8;
										num5 = fleetStrength;
									}
								}
							}
						}
						if (fleetInfo3 == null)
						{
							break;
						}
						MissionInfo missionByFleetID = this._db.GetMissionByFleetID(fleetInfo3.ID);
						if (missionByFleetID != null)
						{
                            Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleetInfo3, true);
						}
						else
						{
							if (fleetInfo3.SupportingSystemID != sdi.SystemID)
							{
								this.AssignFleetToTask(new StrategicTask
								{
									Mission = MissionType.RELOCATION,
									SystemIDTarget = sdi.SystemID
								}, fleetInfo3, null);
							}
						}
						this.m_DefenseCombatFleets.Remove(fleetInfo3);
						num4 -= num5;
					}
				}
			}
		}
		private int GetEnemyFleetStrength(int systemID)
		{
			int num = 0;
			List<FleetInfo> list = (
				from x in this._game.GameDatabase.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL | FleetType.FL_DEFENSE)
				where this._game.GameDatabase.GetDiplomacyStateBetweenPlayers(this._player.ID, x.PlayerID) == DiplomacyState.WAR
				select x).ToList<FleetInfo>();
			if (list.Count > 0)
			{
				foreach (FleetInfo current in list)
				{
					num += this.GetFleetStrength(current);
				}
			}
			return num;
		}
		private int GetNumStandardPlayersAtSystem(StrategicTask task, List<FleetInfo> fleetsAtSystem)
		{
			int num = 0;
			List<int> list = new List<int>();
			foreach (FleetInfo current in fleetsAtSystem)
			{
				if (!list.Contains(current.PlayerID))
				{
					PlayerInfo playerInfo = this._db.GetPlayerInfo(current.PlayerID);
					if (playerInfo.isStandardPlayer)
					{
						num++;
					}
					list.Add(playerInfo.ID);
				}
			}
			List<ColonyInfo> list2 = this._db.GetColonyInfosForSystem(task.SystemIDTarget).ToList<ColonyInfo>();
			foreach (ColonyInfo current2 in list2)
			{
				if (!list.Contains(current2.PlayerID))
				{
					PlayerInfo playerInfo2 = this._db.GetPlayerInfo(current2.PlayerID);
					if (playerInfo2.isStandardPlayer)
					{
						num++;
					}
					list.Add(playerInfo2.ID);
				}
			}
			return num;
		}
		private List<EasterEgg> GetEncountersAtSystem(int systemID, List<FleetInfo> enemyFleets)
		{
			List<EasterEgg> list = new List<EasterEgg>();
			if (enemyFleets.Count > 0)
			{
				foreach (FleetInfo current in enemyFleets)
				{
					if (this._game.ScriptModules.IsEncounterPlayer(current.PlayerID))
					{
						EasterEgg easterEggTypeForPlayer = this._game.ScriptModules.GetEasterEggTypeForPlayer(current.PlayerID);
						if (easterEggTypeForPlayer != EasterEgg.UNKNOWN && !list.Contains(easterEggTypeForPlayer))
						{
							list.Add(easterEggTypeForPlayer);
						}
					}
				}
			}
			List<StationInfo> list2 = this._db.GetStationForSystem(systemID).ToList<StationInfo>();
			if (list2.Count > 0)
			{
				foreach (StationInfo current2 in list2)
				{
					if (this._game.ScriptModules.IsEncounterPlayer(current2.PlayerID))
					{
						EasterEgg easterEggTypeForPlayer2 = this._game.ScriptModules.GetEasterEggTypeForPlayer(current2.PlayerID);
						if (easterEggTypeForPlayer2 != EasterEgg.UNKNOWN && !list.Contains(easterEggTypeForPlayer2))
						{
							list.Add(easterEggTypeForPlayer2);
						}
					}
				}
			}
			return list;
		}
		private int GetFleetStrength(FleetInfo fleet)
		{
			int num = 0;
			List<ShipInfo> list = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				if (fleet.Type != FleetType.FL_DEFENSE || this._game.GameDatabase.GetShipSystemPosition(current.ID).HasValue)
				{
					num += CombatAI.GetShipStrength(current.DesignInfo.Class);
				}
			}
			return num;
		}
		private int GetNumFleetsRequestForTask(StrategicTask task)
		{
			int num = 1;
			if (task.Mission == MissionType.INVASION || task.Mission == MissionType.STRIKE)
			{
				List<ColonyInfo> list = (
					from x in this._game.GameDatabase.GetColonyInfosForSystem(task.SystemIDTarget)
					where this._game.GameDatabase.GetDiplomacyStateBetweenPlayers(this._player.ID, x.PlayerID) == DiplomacyState.WAR
					select x).ToList<ColonyInfo>();
				if (list.Count > 2)
				{
					num++;
				}
				int num2 = 0;
				if (task.UseableFleets.Count > 0)
				{
					num2 = task.UseableFleets.Sum((FleetManagement x) => x.FleetStrength);
				}
				if (task.EnemyStrength > num2)
				{
					num++;
				}
				if (task.EnemyStrength > num2 * 2)
				{
					num++;
				}
			}
			List<MissionInfo> list2 = this._game.GameDatabase.GetMissionsBySystemDest(task.SystemIDTarget).ToList<MissionInfo>();
			list2.RemoveAll((MissionInfo x) => this._game.GameDatabase.GetFleetInfo(x.FleetID).PlayerID != this._player.ID);
			if (task.Mission == MissionType.CONSTRUCT_STN || task.Mission == MissionType.UPGRADE_STN)
			{
				num = Math.Max(num - list2.Count((MissionInfo x) => x.Type == MissionType.CONSTRUCT_STN || x.Type == MissionType.UPGRADE_STN), 0);
			}
			else
			{
				num = Math.Max(num - list2.Count((MissionInfo x) => x.Type == task.Mission), 0);
			}
			return Math.Min(num, 3);
		}
		public static FleetTemplate GetTemplateForFleet(GameSession game, int playerID, int fleetID)
		{
			FleetTemplate fleetTemplate = null;
			AIFleetInfo aiFleetInfo = game.GameDatabase.GetAIFleetInfos(playerID).FirstOrDefault((AIFleetInfo x) => x.FleetID == fleetID);
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(game.GameDatabase, game, fleetID);
				if (!string.IsNullOrEmpty(templateName))
				{
					fleetTemplate = game.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
				}
			}
			if (fleetTemplate == null)
			{
				fleetTemplate = game.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == aiFleetInfo.FleetTemplate);
			}
			return fleetTemplate;
		}
		private List<ShipInclude> GetRequiredShipIncludes(FleetTemplate template)
		{
			List<ShipInclude> list = (
				from x in template.ShipIncludes
				where x.InclusionType == ShipInclusionType.REQUIRED
				select x).ToList<ShipInclude>();
			if (template.MissionTypes.Contains(MissionType.GATE) && this._player.Faction.CanUseGate())
			{
				float num = 0f;
				List<FleetInfo> list2 = (
					from x in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL)
					where this._db.GetShipInfoByFleetID(x.ID, true).Any((ShipInfo y) => y.DesignInfo.Role == ShipRole.GATE)
					select x).ToList<FleetInfo>();
				if (list2.Count > 0)
				{
					foreach (FleetInfo current in list2)
					{
                        int num2 = Math.Max(Kerberos.Sots.StarFleet.StarFleet.GetFleetEndurance(this._game, current.ID) - 1, 0);
                        num = Math.Max((float)num2 * Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game, current.ID, false), num);
					}
					bool flag = false;
					foreach (SystemDefendInfo current2 in this.m_ColonizedSystems)
					{
						flag = this._db.GetSystemsInRange(this._db.GetStarSystemOrigin(current2.SystemID), num).Any((StarSystemInfo x) => !this._db.SystemHasGate(x.ID, this._player.ID));
						if (flag)
						{
							break;
						}
					}
					if (!flag)
					{
						ShipInclude shipInclude = list.FirstOrDefault((ShipInclude x) => x.ShipRole == ShipRole.SUPPLY);
						if (shipInclude != null)
						{
							shipInclude.Amount = 3;
						}
						else
						{
							list.Add(new ShipInclude
							{
								Amount = 1,
								Faction = this._player.Faction.Name,
								InclusionType = ShipInclusionType.REQUIRED,
								WeaponRole = null
							});
						}
					}
				}
			}
			return list;
		}
		private bool FleetRequiresFill(FleetInfo fleet)
		{
			if (fleet == null)
			{
				return false;
			}
			List<AIFleetInfo> source = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			FleetTemplate fleetTemplate = null;
			AIFleetInfo aiFleetInfo = source.FirstOrDefault((AIFleetInfo x) => x.FleetID == fleet.ID);
			if (aiFleetInfo != null && aiFleetInfo.InvoiceID.HasValue && aiFleetInfo.InvoiceID != 0)
			{
				return false;
			}
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
				if (!string.IsNullOrEmpty(templateName))
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
					AIFleetInfo aIFleetInfo = new AIFleetInfo();
					aIFleetInfo.AdmiralID = new int?(fleet.AdmiralID);
					aIFleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes);
					aIFleetInfo.SystemID = fleet.SupportingSystemID;
					aIFleetInfo.FleetTemplate = templateName;
					aIFleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aIFleetInfo);
					aiFleetInfo = aIFleetInfo;
				}
			}
			if (fleetTemplate == null)
			{
				fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == aiFleetInfo.FleetTemplate);
			}
			int num = 0;
			int num2 = 0;
			bool flag = false;
			List<ShipInfo> source2 = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			foreach (ShipInclude include in this.GetRequiredShipIncludes(fleetTemplate))
			{
				if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
				{
					num2 += include.Amount;
					int num3 = source2.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
					if (num3 < include.Amount)
					{
						int arg_2B9_0 = include.Amount;
						num += include.Amount - num3;
					}
				}
			}
			float num4 = 0.3f;
			if (this._player.Faction.CanUseAccelerators())
			{
				num4 = 0.5f;
			}
			flag = ((float)num / (float)num2 > num4);
			if (!flag)
			{
				bool flag2 = false;
				foreach (ShipInclude current in (
					from x in fleetTemplate.ShipIncludes
					where x.InclusionType == ShipInclusionType.FILL
					select x).ToList<ShipInclude>())
				{
					if ((string.IsNullOrEmpty(current.Faction) || !(current.Faction != this._player.Faction.Name)) && !(current.FactionExclusion == this._player.Faction.Name))
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
                    int fleetCommandPoints = Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandPoints(this._game.App, fleet.ID, null);
                    int fleetCommandCost = Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandCost(this._game.App, fleet.ID, null);
					if ((float)fleetCommandCost / (float)fleetCommandPoints < 0.5f)
					{
						flag = true;
					}
				}
			}
			return flag;
		}
		private void TransferCubesFromReserve(FleetInfo fleet)
		{
			if (fleet == null || fleet.SystemID == 0 || this._db.GetMissionByFleetID(fleet.ID) != null)
			{
				return;
			}
			this._db.GetMissionByFleetID(fleet.ID);
			int? reserveFleetID = this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID);
			if (!reserveFleetID.HasValue)
			{
				return;
			}
            int remainingCubes = this.m_LoaLimit - Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID);
			if (remainingCubes > 0)
			{
				List<ShipInfo> list = (
					from x in this._game.GameDatabase.GetShipInfoByFleetID(reserveFleetID.Value, true)
					where x.DesignInfo.IsLoaCube() && (!x.AIFleetID.HasValue || x.AIFleetID == 0)
					select x).ToList<ShipInfo>();
				while (remainingCubes > 0)
				{
                    ShipInfo shipInfo = list.FirstOrDefault((ShipInfo x) => Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, x.ID) <= remainingCubes);
					if (shipInfo == null)
					{
						return;
					}
                    remainingCubes -= Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo.ID);
					this._db.TransferShip(shipInfo.ID, fleet.ID);
					list.Remove(shipInfo);
				}
			}
		}
		private void FixFullFleet(FleetInfo fleet)
		{
			if (fleet == null)
			{
				return;
			}
			List<AIFleetInfo> source = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			FleetTemplate fleetTemplate = null;
			AIFleetInfo aiFleetInfo = source.FirstOrDefault((AIFleetInfo x) => x.FleetID == fleet.ID);
			if (aiFleetInfo != null && aiFleetInfo.InvoiceID.HasValue && aiFleetInfo.InvoiceID != 0)
			{
				return;
			}
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
				if (!string.IsNullOrEmpty(templateName))
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
					AIFleetInfo aIFleetInfo = new AIFleetInfo();
					aIFleetInfo.AdmiralID = new int?(fleet.AdmiralID);
					aIFleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes);
					aIFleetInfo.SystemID = fleet.SupportingSystemID;
					aIFleetInfo.FleetTemplate = templateName;
					aIFleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aIFleetInfo);
					aiFleetInfo = aIFleetInfo;
				}
			}
			if (fleetTemplate == null)
			{
				fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == aiFleetInfo.FleetTemplate);
			}
			if (fleetTemplate == null)
			{
				return;
			}
			bool flag = this._db.GetMissionByFleetID(fleet.ID) != null;
			int? num = null;
			if (!flag)
			{
				num = this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID);
				if (!num.HasValue || num.Value == 0)
				{
					return;
				}
			}
			int num2 = 0;
			List<ShipInfo> list = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			foreach (ShipInclude include in this.GetRequiredShipIncludes(fleetTemplate))
			{
				if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
				{
					List<ShipInfo> list2 = (
						from x in list
						where StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole)
						orderby this._db.GetShipCruiserEquivalent(x.DesignInfo), x.DesignInfo.DesignDate
						select x).ToList<ShipInfo>();
					int i = list2.Count - include.Amount;
					while (i > 0)
					{
						ShipInfo shipInfo = list2.FirstOrDefault<ShipInfo>();
						if (shipInfo == null)
						{
							break;
						}
						i--;
						num2++;
						list.Remove(shipInfo);
						list2.Remove(shipInfo);
						if (num.HasValue)
						{
							this._db.TransferShip(shipInfo.ID, num.Value);
						}
					}
				}
			}
			int j = this._db.GetFleetCommandPointQuota(fleet.ID) - this._db.GetFleetCommandPointCost(fleet.ID);
			if (j < 0)
			{
				foreach (ShipInclude include in (
					from x in fleetTemplate.ShipIncludes
					where x.InclusionType == ShipInclusionType.FILL
					select x).ToList<ShipInclude>())
				{
					if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
					{
						List<ShipInfo> list3 = (
							from x in list
							where StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole)
							orderby this._db.GetShipCruiserEquivalent(x.DesignInfo), x.DesignInfo.DesignDate
							select x).ToList<ShipInfo>();
						while (j < 0)
						{
							ShipInfo shipInfo2 = list3.FirstOrDefault<ShipInfo>();
							if (shipInfo2 == null || list3.Count <= include.Amount)
							{
								break;
							}
							j += this._db.GetShipCommandPointCost(shipInfo2.ID, false);
							num2++;
							list.Remove(shipInfo2);
							list3.Remove(shipInfo2);
							if (num.HasValue)
							{
								this._db.TransferShip(shipInfo2.ID, num.Value);
							}
						}
					}
				}
			}
			if (flag && num2 > 3)
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleet, true);
			}
		}
		private void ConsumeRemainingLoaCubes(FleetInfo fleet, AIStance stance)
		{
			if (fleet == null)
			{
				return;
			}
			List<ShipInfo> list = this._db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			ShipInfo shipInfo = list.FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
			if (shipInfo == null)
			{
                shipInfo = this._db.GetShipInfo(Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._game, fleet.ID), false);
			}
			if (shipInfo == null || shipInfo.LoaCubes == 0)
			{
				return;
			}
			int num = shipInfo.LoaCubes;
			List<AIFleetInfo> source = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			FleetTemplate fleetTemplate = null;
			AIFleetInfo aiFleetInfo = source.FirstOrDefault((AIFleetInfo x) => x.FleetID == fleet.ID);
			if (aiFleetInfo != null && aiFleetInfo.InvoiceID.HasValue && aiFleetInfo.InvoiceID != 0)
			{
				return;
			}
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
				if (!string.IsNullOrEmpty(templateName))
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
					AIFleetInfo aIFleetInfo = new AIFleetInfo();
					aIFleetInfo.AdmiralID = new int?(fleet.AdmiralID);
					aIFleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes);
					aIFleetInfo.SystemID = fleet.SupportingSystemID;
					aIFleetInfo.FleetTemplate = templateName;
					aIFleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aIFleetInfo);
					aiFleetInfo = aIFleetInfo;
				}
			}
			if (fleetTemplate == null)
			{
				fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == aiFleetInfo.FleetTemplate);
			}
			List<DesignInfo> list2 = (
				from X in this._db.GetDesignInfosForPlayer(this._player.ID)
				where X.Class == ShipClass.BattleRider
				select X).ToList<DesignInfo>();
			foreach (ShipInclude include in this.GetRequiredShipIncludes(fleetTemplate))
			{
				if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
				{
					int num2 = list.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
					if (num2 < include.Amount)
					{
						DesignInfo designInfo = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), include.ShipRole, StrategicAI.GetEquivilantShipRoles(include.ShipRole), include.WeaponRole);
						if (designInfo == null)
						{
							designInfo = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), include.ShipRole, include.WeaponRole);
						}
						if (designInfo != null)
						{
							int playerProductionCost = designInfo.GetPlayerProductionCost(this._game.GameDatabase, this._player.ID, !designInfo.isPrototyped, null);
							int num3 = include.Amount - num2;
							while (num3 > 0 && num >= playerProductionCost)
							{
								int shipID = this._db.InsertShip(fleet.ID, designInfo.ID, null, (ShipParams)0, new int?((aiFleetInfo != null) ? aiFleetInfo.ID : 0), 0);
								ShipInfo shipInfo2 = this._db.GetShipInfo(shipID, true);
								if (shipInfo2 == null)
								{
									break;
								}
								num3--;
								num -= playerProductionCost;
								list.Add(shipInfo2);
								this._game.GameDatabase.TransferShip(shipInfo2.ID, fleet.ID);
								List<CarrierWingData> list3 = RiderManager.GetDesignBattleriderWingData(this._game.App, designInfo).ToList<CarrierWingData>();
								foreach (CarrierWingData wd in list3)
								{
									List<DesignInfo> classriders = (
										from x in list2
										where x.GetMissionSectionAsset().BattleRiderType != BattleRiderTypes.escort && StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
										select x).ToList<DesignInfo>();
									if (classriders.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
									{
										BattleRiderTypes SelectedType = (
											from x in classriders
											orderby x.DesignDate
											select x).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
										DesignInfo designInfo2 = classriders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count);
										int num4 = (designInfo2 != null) ? designInfo2.GetPlayerProductionCost(this._db, this._player.ID, !designInfo2.isPrototyped, null) : 0;
										foreach (int current in wd.SlotIndexes)
										{
											if (designInfo2 == null || num < num4)
											{
												break;
											}
											int num5 = this._db.InsertShip(fleet.ID, designInfo2.ID, designInfo2.Name, (ShipParams)0, null, 0);
											this._game.AddDefaultStartingRiders(fleet.ID, designInfo2.ID, num5);
											this._db.SetShipParent(num5, shipInfo2.ID);
											this._db.UpdateShipRiderIndex(num5, current);
											list2.Remove(designInfo2);
										}
									}
								}
							}
						}
					}
				}
			}
			if (list.Any((ShipInfo x) => x.DesignInfo.Role == ShipRole.COMMAND))
			{
				int num6 = this._db.GetFleetCommandPointQuota(fleet.ID) - this._db.GetFleetCommandPointCost(fleet.ID);
				if (num6 > 0 && num > 0)
				{
					foreach (ShipInclude include in (
						from x in fleetTemplate.ShipIncludes
						where x.InclusionType == ShipInclusionType.FILL
						select x).ToList<ShipInclude>())
					{
						if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
						{
							int num7 = 0;
							if (include.Amount > 0)
							{
								int num8 = list.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
								if (num8 < include.Amount)
								{
									num7 = include.Amount - num8;
								}
							}
							DesignInfo designInfo3 = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), include.ShipRole, StrategicAI.GetEquivilantShipRoles(include.ShipRole), include.WeaponRole);
							if (designInfo3 == null)
							{
								designInfo3 = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), include.ShipRole, include.WeaponRole);
							}
							if (designInfo3 != null)
							{
								int playerProductionCost2 = designInfo3.GetPlayerProductionCost(this._game.GameDatabase, this._player.ID, !designInfo3.isPrototyped, null);
								while (num >= playerProductionCost2 && ((include.Amount == 0 && num6 > 0) || (include.Amount > 0 && num7 > 0)))
								{
									int shipID2 = this._db.InsertShip(fleet.ID, designInfo3.ID, null, (ShipParams)0, new int?((aiFleetInfo != null) ? aiFleetInfo.ID : 0), 0);
									ShipInfo shipInfo3 = this._db.GetShipInfo(shipID2, true);
									if (shipInfo3 == null)
									{
										break;
									}
									num7--;
									num -= playerProductionCost2;
									num6 -= shipInfo3.DesignInfo.CommandPointCost;
									list.Add(shipInfo3);
									this._game.GameDatabase.TransferShip(shipInfo3.ID, fleet.ID);
									List<CarrierWingData> list4 = RiderManager.GetDesignBattleriderWingData(this._game.App, designInfo3).ToList<CarrierWingData>();
									foreach (CarrierWingData wd in list4)
									{
										List<DesignInfo> classriders = (
											from x in list2
											where x.GetMissionSectionAsset().BattleRiderType != BattleRiderTypes.escort && StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
											select x).ToList<DesignInfo>();
										if (classriders.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
										{
											BattleRiderTypes SelectedType = (
												from x in classriders
												orderby x.DesignDate
												select x).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
											DesignInfo designInfo4 = classriders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count);
											int num9 = (designInfo4 != null) ? designInfo4.GetPlayerProductionCost(this._db, this._player.ID, !designInfo4.isPrototyped, null) : 0;
											foreach (int current2 in wd.SlotIndexes)
											{
												if (designInfo4 == null || num < num9)
												{
													break;
												}
												int num10 = this._db.InsertShip(fleet.ID, designInfo4.ID, designInfo4.Name, (ShipParams)0, null, 0);
												this._game.AddDefaultStartingRiders(fleet.ID, designInfo4.ID, num10);
												this._db.SetShipParent(num10, shipInfo3.ID);
												this._db.UpdateShipRiderIndex(num10, current2);
												list2.Remove(designInfo4);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (shipInfo.LoaCubes <= 0)
			{
				this._db.RemoveShip(shipInfo.ID);
				return;
			}
			this._db.UpdateShipLoaCubes(shipInfo.ID, num);
		}
		private bool FillFleet(FleetInfo fleet)
		{
			bool flag = false;
			if (fleet == null || fleet.SystemID == 0)
			{
				return false;
			}
			List<AIFleetInfo> source = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			FleetTemplate fleetTemplate = null;
			AIFleetInfo aiFleetInfo = source.FirstOrDefault((AIFleetInfo x) => x.FleetID == fleet.ID);
			if (aiFleetInfo != null && aiFleetInfo.InvoiceID.HasValue && aiFleetInfo.InvoiceID != 0)
			{
				return false;
			}
			if (aiFleetInfo == null)
			{
				string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
				if (!string.IsNullOrEmpty(templateName))
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
					AIFleetInfo aIFleetInfo = new AIFleetInfo();
					aIFleetInfo.AdmiralID = new int?(fleet.AdmiralID);
					aIFleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes);
					aIFleetInfo.SystemID = fleet.SupportingSystemID;
					aIFleetInfo.FleetTemplate = templateName;
					aIFleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aIFleetInfo);
					aiFleetInfo = aIFleetInfo;
				}
			}
			if (fleetTemplate == null)
			{
				fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == aiFleetInfo.FleetTemplate);
			}
			int? reserveFleetID = this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID);
			if (!reserveFleetID.HasValue || reserveFleetID.Value == 0 || fleetTemplate == null)
			{
				return false;
			}
			int remainingCELimit = this._player.Faction.CanUseGate() ? (this.m_GateCapacity - this._db.GetFleetCruiserEquivalent(fleet.ID)) : 2147483647;
            int num = this._player.Faction.CanUseAccelerators() ? (this.m_LoaLimit - Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID)) : 2147483647;
			bool flag2 = true;
			bool flag3 = false;
			List<ShipInfo> list = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			List<ShipInfo> list2 = (
				from x in this._game.GameDatabase.GetShipInfoByFleetID(reserveFleetID.Value, true)
				where !x.AIFleetID.HasValue || x.AIFleetID == 0 || x.AIFleetID == aiFleetInfo.ID
				select x).ToList<ShipInfo>();
			foreach (ShipInclude include in this.GetRequiredShipIncludes(fleetTemplate))
			{
				if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
				{
					int num2 = list.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
					if (num2 < include.Amount)
					{
						int num3 = include.Amount - num2;
						while (num3 > 0 && list2.Count > 0)
						{
							ShipInfo shipInfo = (
								from x in list2
								where x.AIFleetID == aiFleetInfo.ID
								select x).FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
							if (shipInfo == null)
							{
								shipInfo = list2.FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
							}
							if (shipInfo == null)
							{
								break;
							}
							num3--;
							list2.Remove(shipInfo);
							list.Add(shipInfo);
							this._game.GameDatabase.TransferShip(shipInfo.ID, fleet.ID);
							flag = true;
						}
						if (num3 > 0 && (include.ShipRole != ShipRole.GATE || num2 == 0))
						{
							flag2 = false;
						}
					}
				}
			}
			int remainingPoints = this._db.GetFleetCommandPointQuota(fleet.ID) - this._db.GetFleetCommandPointCost(fleet.ID);
			if (remainingPoints > 0)
			{
				foreach (ShipInclude include in (
					from x in fleetTemplate.ShipIncludes
					where x.InclusionType == ShipInclusionType.FILL
					select x).ToList<ShipInclude>())
				{
					if ((string.IsNullOrEmpty(include.Faction) || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
					{
						flag3 = true;
						int num4 = 0;
						if (include.Amount > 0)
						{
							int num5 = list.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole));
							if (num5 < include.Amount)
							{
								num4 = include.Amount - num5;
							}
						}
						while (list2.Count > 0 && ((include.Amount == 0 && remainingPoints > 0) || (include.Amount > 0 && num4 > 0)))
						{
							ShipInfo shipInfo2 = list2.FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, include.ShipRole) && x.DesignInfo.CommandPointCost <= remainingPoints && (!this._player.Faction.CanUseGate() || this._db.GetShipCruiserEquivalent(x.DesignInfo) <= remainingCELimit));
							if (shipInfo2 == null)
							{
								break;
							}
							if (this._player.Faction.CanUseGate())
							{
								int shipCruiserEquivalent = this._db.GetShipCruiserEquivalent(shipInfo2.DesignInfo);
								remainingCELimit -= shipCruiserEquivalent;
								if (remainingCELimit < 0)
								{
									remainingPoints = 0;
									num4 = 0;
									break;
								}
							}
							else
							{
								if (this._player.Faction.CanUseAccelerators())
								{
                                    int shipLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, shipInfo2.ID);
									num -= shipLoaCubeValue;
									if (num < 0)
									{
										num = 0;
										num4 = 0;
										break;
									}
								}
							}
							num4--;
							remainingPoints -= shipInfo2.DesignInfo.CommandPointCost;
							list2.Remove(shipInfo2);
							list.Add(shipInfo2);
							this._game.GameDatabase.TransferShip(shipInfo2.ID, fleet.ID);
							flag = true;
						}
						if (include.Amount > 0 && num4 > 0)
						{
							flag2 = false;
						}
					}
				}
			}
			float num6 = this.FillFleetRiders(fleet);
			if (num6 > 0f)
			{
				flag2 = false;
			}
			if (flag3 && flag2 && remainingCELimit > 0 && num > 0)
			{
                int fleetCommandPoints = Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandPoints(this._game.App, fleet.ID, null);
                int fleetCommandCost = Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandCost(this._game.App, fleet.ID, null);
				float num7 = (float)fleetCommandCost / (float)fleetCommandPoints;
				if ((this._player.Faction.Name != "loa" && num7 < 0.9f) || num7 < 0.75f)
				{
					flag2 = false;
				}
			}
			if (this._player.Faction.Name == "loa" && flag)
			{
				this._db.SaveCurrentFleetCompositionToFleet(fleet.ID);
			}
			return flag2;
		}
		private int GetRemainingAIBuildTime(int systemId, float productionPerTurn)
		{
			int num = this.MAX_BUILD_TURNS;
			int num2 = 0;
			int num3 = 0;
			List<BuildOrderInfo> list = this._db.GetBuildOrdersForSystem(systemId).ToList<BuildOrderInfo>();
			foreach (BuildOrderInfo current in list)
			{
				num3 += current.Progress;
				num2 += current.ProductionTarget;
			}
			if (productionPerTurn > 0f)
			{
				int num4 = (int)Math.Ceiling((double)((float)(num2 - num3) / productionPerTurn));
				num -= num4;
			}
			return num;
		}
		private void FillFleetsWithBuild(int systemId, List<FleetInfo> fleetsAtSystem, AIStance stance)
		{
			float num = 0f;
			double num2 = 0.0;
			double num3 = 0.0;
			BuildScreenState.ObtainConstructionCosts(out num, out num2, out num3, this._game.App, systemId, this._player.ID);
			int num4 = this.GetRemainingAIBuildTime(systemId, num);
			if (num4 <= 0)
			{
				return;
			}
			double num5 = this.GetAvailableFillFleetConstructionBudget();
			List<BuildOrderInfo> source = this._db.GetBuildOrdersForSystem(systemId).ToList<BuildOrderInfo>();
			List<AIFleetInfo> list = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			List<BuildManagement> list2 = new List<BuildManagement>();
			foreach (FleetInfo fleet in fleetsAtSystem)
			{
				FleetTemplate fleetTemplate = null;
				AIFleetInfo aiFleetInfo = list.FirstOrDefault((AIFleetInfo x) => x.FleetID == fleet.ID);
				if (aiFleetInfo == null)
				{
					string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
					if (!string.IsNullOrEmpty(templateName))
					{
						fleetTemplate = this._db.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
						AIFleetInfo aIFleetInfo = new AIFleetInfo();
						aIFleetInfo.AdmiralID = new int?(fleet.AdmiralID);
						aIFleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes);
						aIFleetInfo.SystemID = fleet.SupportingSystemID;
						aIFleetInfo.FleetTemplate = templateName;
						aIFleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aIFleetInfo);
						aiFleetInfo = aIFleetInfo;
						list.Add(aiFleetInfo);
					}
				}
				if (fleetTemplate == null)
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == aiFleetInfo.FleetTemplate);
				}
				if (!source.Any((BuildOrderInfo x) => x.AIFleetID.HasValue && x.AIFleetID.Value == aiFleetInfo.ID) && (!aiFleetInfo.InvoiceID.HasValue || !(aiFleetInfo.InvoiceID != 0)))
				{
					List<InvoiceInstanceInfo> list3 = this._db.GetInvoicesForSystem(this._player.ID, systemId).ToList<InvoiceInstanceInfo>();
					BuildManagement buildManagement = new BuildManagement();
					List<ShipInfo> list4 = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
					foreach (ShipInfo current in list4)
					{
						buildManagement.Invoices.AddRange(this.BuildRidersForShip(stance, current.DesignInfo, list3, current));
					}
					int num6 = this._player.Faction.CanUseAccelerators() ? (this.m_LoaLimit - Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game, fleet.ID)) : 0;
					int num7 = this._player.Faction.CanUseGate() ? (this.m_GateCapacity - this._db.GetFleetCruiserEquivalent(fleet.ID)) : 0;
					foreach (ShipInclude ship in fleetTemplate.ShipIncludes)
					{
						if ((string.IsNullOrEmpty(ship.Faction) || !(ship.Faction != this._player.Faction.Name)) && !(ship.FactionExclusion == this._player.Faction.Name))
						{
							DesignInfo selectedDesign = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), ship.ShipRole, StrategicAI.GetEquivilantShipRoles(ship.ShipRole), ship.WeaponRole);
							if (selectedDesign == null)
							{
								selectedDesign = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), ship.ShipRole, ship.WeaponRole);
							}
							int num8;
							if (ship.InclusionType == ShipInclusionType.FILL)
							{
								List<ShipInfo> source2 = this._db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
								DesignInfo designInfo = (
									from x in source2
									select x.DesignInfo).FirstOrDefault((DesignInfo x) => x.Role == ShipRole.COMMAND);
								if (designInfo == null)
								{
									designInfo = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), null);
								}
								if (designInfo == null)
								{
									designInfo = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), ShipRole.COMMAND, null);
								}
								num8 = DesignLab.GetTemplateFillAmount(this._db, fleetTemplate, designInfo, selectedDesign);
								if (ship.Amount > 0)
								{
									int num9 = list4.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, ship.ShipRole));
									int val = ship.Amount - num9;
									num8 = Math.Min(num8, val);
								}
								if (this._player.Faction.CanUseGate())
								{
									int shipCruiserEquivalent = this._db.GetShipCruiserEquivalent(selectedDesign);
									if (shipCruiserEquivalent > 0)
									{
										num8 = Math.Min(num8, num7 / shipCruiserEquivalent);
									}
								}
							}
							else
							{
								int num10 = list4.Count((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, ship.ShipRole));
								num8 = ship.Amount - num10;
							}
							int i = 0;
							while (i < num8)
							{
								if (ship.InclusionType == ShipInclusionType.FILL && this._player.Faction.CanUseAccelerators())
								{
									int shipLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, selectedDesign);
									num6 -= shipLoaCubeValue;
									if (num6 < 0)
									{
										num6 = 0;
										break;
									}
								}
								List<BuildScreenState.InvoiceItem> arg_760_0 = buildManagement.Invoices;
								BuildScreenState.InvoiceItem invoiceItem = new BuildScreenState.InvoiceItem();
								invoiceItem.DesignID = selectedDesign.ID;
								invoiceItem.ShipName = selectedDesign.Name;
								invoiceItem.Progress = -1;
								BuildScreenState.InvoiceItem arg_751_0 = invoiceItem;
								if (selectedDesign.isPrototyped)
								{
									goto IL_750;
								}
								if (buildManagement.Invoices.Any((BuildScreenState.InvoiceItem x) => x.DesignID == selectedDesign.ID && x.isPrototypeOrder))
								{
									goto IL_750;
								}
								bool arg_751_1 = !list3.Any((InvoiceInstanceInfo x) => this._db.GetBuildOrdersForInvoiceInstance(x.ID).Any((BuildOrderInfo y) => y.DesignID == selectedDesign.ID));
								IL_751:
								arg_751_0.isPrototypeOrder = arg_751_1;
								invoiceItem.LoaCubes = 0;
								arg_760_0.Add(invoiceItem);
								i++;
								continue;
								IL_750:
								arg_751_1 = false;
								goto IL_751;
							}
							buildManagement.Invoices.AddRange(this.BuildRidersForShip(stance, selectedDesign, list3, null));
						}
					}
					if (aiFleetInfo != null)
					{
						buildManagement.AIFleetID = new int?(aiFleetInfo.ID);
					}
					buildManagement.FleetToFill = fleet;
					buildManagement.BuildTime = BuildScreenState.GetBuildTime(this._game.App, buildManagement.Invoices, num);
					list2.Add(buildManagement);
				}
			}
			list2.RemoveAll((BuildManagement x) => x.Invoices.Count == 0);

			var unused = from x in list2
				orderby x.BuildTime
				select x;
			foreach (BuildManagement build in list2)
			{
				AdmiralInfo admiralInfo = this._db.GetAdmiralInfo(build.FleetToFill.AdmiralID);
				if (admiralInfo != null)
				{
					int buildTime = BuildScreenState.GetBuildTime(this._game.App, build.Invoices, num);
					if (buildTime > 0)
					{
						int num11 = BuildScreenState.GetBuildInvoiceCost(this._game.App, build.Invoices) / buildTime;
						if (num5 >= (double)num11)
						{
							AIFleetInfo aIFleetInfo2 = list.FirstOrDefault((AIFleetInfo x) => x.ID == build.AIFleetID.Value);
							if (aIFleetInfo2 == null || !aIFleetInfo2.InvoiceID.HasValue)
							{
								int invoiceId;
								int num12;
								this.OpenInvoice(systemId, admiralInfo.Name, out invoiceId, out num12);
								if (aIFleetInfo2 != null)
								{
									aIFleetInfo2.InvoiceID = new int?(num12);
									this._db.UpdateAIFleetInfo(aIFleetInfo2);
								}
								foreach (BuildScreenState.InvoiceItem current2 in build.Invoices)
								{
									this.AddShipToInvoice(systemId, this._db.GetDesignInfo(current2.DesignID), invoiceId, num12, build.AIFleetID);
								}
								num5 -= (double)num11;
								num4 -= build.BuildTime;
								if (num4 <= 0)
								{
									break;
								}
							}
						}
					}
				}
			}
		}
		private List<BuildScreenState.InvoiceItem> BuildRidersForShip(AIStance stance, DesignInfo design, List<InvoiceInstanceInfo> orders, ShipInfo ship = null)
		{
			List<ShipInfo> source = (ship != null) ? this._db.GetBattleRidersByParentID(ship.ID).ToList<ShipInfo>() : new List<ShipInfo>();
			List<BuildScreenState.InvoiceItem> list = new List<BuildScreenState.InvoiceItem>();
			Dictionary<LogicalBank, StrategicAI.BankRiderInfo> dictionary = new Dictionary<LogicalBank, StrategicAI.BankRiderInfo>();
			foreach (KeyValuePair<LogicalMount, int> mountIndex in StrategicAI.BattleRiderMountSet.EnumerateBattleRiderMounts(design))
			{
				ShipInfo shipInfo = source.FirstOrDefault(delegate(ShipInfo x)
				{
					int arg_14_0 = x.RiderIndex;
					KeyValuePair<LogicalMount, int> mountIndex5 = mountIndex;
					return arg_14_0 == mountIndex5.Value;
				});
				Dictionary<LogicalBank, StrategicAI.BankRiderInfo> arg_8D_0 = dictionary;
				KeyValuePair<LogicalMount, int> mountIndex6 = mountIndex;
				StrategicAI.BankRiderInfo bankRiderInfo;
				if (!arg_8D_0.TryGetValue(mountIndex6.Key.Bank, out bankRiderInfo))
				{
					StrategicAI.BankRiderInfo bankRiderInfo2 = new StrategicAI.BankRiderInfo();
					StrategicAI.BankRiderInfo arg_B2_0 = bankRiderInfo2;
					KeyValuePair<LogicalMount, int> mountIndex2 = mountIndex;
					arg_B2_0.Bank = mountIndex2.Key.Bank;
					bankRiderInfo = bankRiderInfo2;
					dictionary.Add(bankRiderInfo.Bank, bankRiderInfo);
					if (shipInfo != null)
					{
						bankRiderInfo.AllocatedRole = new ShipRole?(shipInfo.DesignInfo.Role);
					}
				}
				if (shipInfo == null)
				{
					List<int> arg_FE_0 = bankRiderInfo.FreeRiderIndices;
					KeyValuePair<LogicalMount, int> mountIndex3 = mountIndex;
					arg_FE_0.Add(mountIndex3.Value);
				}
				else
				{
					List<int> arg_11C_0 = bankRiderInfo.FilledRiderIndices;
					KeyValuePair<LogicalMount, int> mountIndex4 = mountIndex;
					arg_11C_0.Add(mountIndex4.Value);
				}
			}
			foreach (StrategicAI.BankRiderInfo current in dictionary.Values)
			{
				ShipRole shipRole;
				if (current.AllocatedRole.HasValue)
				{
					shipRole = current.AllocatedRole.Value;
				}
				else
				{
					shipRole = this._random.Choose(StrategicAI.BattleRiderMountSet.EnumerateShipRolesByTurretClass(current.Bank.TurretClass));
				}
				int count = current.FreeRiderIndices.Count;
				for (int i = 0; i < count; i++)
				{
					DesignInfo buildDesign = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), shipRole, StrategicAI.GetEquivilantShipRoles(shipRole), null);
					if (buildDesign == null)
					{
						buildDesign = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), shipRole, null);
					}
					if (buildDesign != null)
					{
						List<BuildScreenState.InvoiceItem> arg_2C6_0 = list;
						BuildScreenState.InvoiceItem invoiceItem = new BuildScreenState.InvoiceItem();
						invoiceItem.DesignID = buildDesign.ID;
						invoiceItem.ShipName = buildDesign.Name;
						invoiceItem.Progress = -1;
						BuildScreenState.InvoiceItem arg_2B7_0 = invoiceItem;
						if (buildDesign.isPrototyped)
						{
							goto IL_2B6;
						}
						if (list.Any((BuildScreenState.InvoiceItem x) => x.DesignID == buildDesign.ID && x.isPrototypeOrder))
						{
							goto IL_2B6;
						}
						bool arg_2B7_1 = !orders.Any((InvoiceInstanceInfo x) => this._db.GetBuildOrdersForInvoiceInstance(x.ID).Any((BuildOrderInfo y) => y.DesignID == buildDesign.ID));
						IL_2B7:
						arg_2B7_0.isPrototypeOrder = arg_2B7_1;
						invoiceItem.LoaCubes = 0;
						arg_2C6_0.Add(invoiceItem);
						goto IL_2CB;
						IL_2B6:
						arg_2B7_1 = false;
						goto IL_2B7;
					}
					IL_2CB:;
				}
			}
			return list;
		}
		private int GetSupportRequirementsForInvoice(List<BuildScreenState.InvoiceItem> items)
		{
			int num = 0;
			foreach (BuildScreenState.InvoiceItem current in items)
			{
				num += this._db.GetShipCruiserEquivalent(this._db.GetDesignInfo(current.DesignID));
			}
			return num;
		}
		private List<BuildScreenState.InvoiceItem> GetBuildFleetInvoice(FleetTemplate template, AIStance stance)
		{
			int num = this.m_LoaLimit;
			int num2 = this.m_GateCapacity;
			List<BuildScreenState.InvoiceItem> list = new List<BuildScreenState.InvoiceItem>();
			foreach (ShipInclude current in template.ShipIncludes)
			{
				if ((string.IsNullOrEmpty(current.Faction) || !(current.Faction != this._player.Faction.Name)) && !(current.FactionExclusion == this._player.Faction.Name))
				{
					DesignInfo designInfo = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), current.ShipRole, StrategicAI.GetEquivilantShipRoles(current.ShipRole), current.WeaponRole);
					if (designInfo == null)
					{
						designInfo = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), current.ShipRole, current.WeaponRole);
					}
					int num3;
					if (current.InclusionType == ShipInclusionType.FILL)
					{
						DesignInfo designInfo2 = DesignLab.GetBestDesignByRole(this._game, this._player, new AIStance?(stance), ShipRole.COMMAND, StrategicAI.GetEquivilantShipRoles(ShipRole.COMMAND), null);
						if (designInfo2 == null)
						{
							designInfo2 = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(stance), ShipRole.COMMAND, null);
						}
						num3 = DesignLab.GetTemplateFillAmount(this._db, template, designInfo2, designInfo);
						if (current.Amount > 0)
						{
							num3 = Math.Min(num3, current.Amount);
						}
						if (this._player.Faction.CanUseGate())
						{
							int shipCruiserEquivalent = this._db.GetShipCruiserEquivalent(designInfo);
							if (shipCruiserEquivalent > 0)
							{
								num3 = Math.Min(num3, num2 / shipCruiserEquivalent);
							}
						}
					}
					else
					{
						num3 = current.Amount;
					}
					for (int i = 0; i < num3; i++)
					{
						if (current.InclusionType == ShipInclusionType.FILL && this._player.Faction.CanUseAccelerators())
						{
                            int shipLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(this._game, designInfo);
							num -= shipLoaCubeValue;
							if (num < 0)
							{
								num = 0;
								break;
							}
						}
						list.Add(new BuildScreenState.InvoiceItem
						{
							DesignID = designInfo.ID,
							ShipName = designInfo.Name,
							Progress = -1,
							isPrototypeOrder = !designInfo.isPrototyped,
							LoaCubes = 0
						});
						list.AddRange(this.BuildRidersForShip(stance, designInfo, new List<InvoiceInstanceInfo>(), null));
						if (this._player.Faction.CanUseGate())
						{
							num2 -= this._db.GetShipCruiserEquivalent(designInfo);
						}
					}
				}
			}
			return list;
		}
		private float FillFleetRiders(FleetInfo fleet)
		{
			int? reserveFleetID = this._game.GameDatabase.GetReserveFleetID(this._player.ID, fleet.SystemID);
			if (!reserveFleetID.HasValue || reserveFleetID.Value == 0)
			{
				return 0f;
			}
			List<ShipInfo> list = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			List<ShipInfo> ships = this._game.GameDatabase.GetShipInfoByFleetID(reserveFleetID.Value, true).ToList<ShipInfo>();
			int num = 0;
			int num2 = 0;
			StrategicAI.BattleRiderMountSet battleRiderMountSet = new StrategicAI.BattleRiderMountSet(ships);
			foreach (ShipInfo current in list)
			{
				List<ShipInfo> source = this._db.GetBattleRidersByParentID(current.ID).ToList<ShipInfo>();
				Dictionary<LogicalBank, StrategicAI.BankRiderInfo> dictionary = new Dictionary<LogicalBank, StrategicAI.BankRiderInfo>();
				foreach (KeyValuePair<LogicalMount, int> mountIndex in StrategicAI.BattleRiderMountSet.EnumerateBattleRiderMounts(current.DesignInfo))
				{
					ShipInfo shipInfo = source.FirstOrDefault(delegate(ShipInfo x)
					{
						int arg_14_0 = x.RiderIndex;
						KeyValuePair<LogicalMount, int> mountIndex5 = mountIndex;
						return arg_14_0 == mountIndex5.Value;
					});
					Dictionary<LogicalBank, StrategicAI.BankRiderInfo> arg_11F_0 = dictionary;
					KeyValuePair<LogicalMount, int> mountIndex6 = mountIndex;
					StrategicAI.BankRiderInfo bankRiderInfo;
					if (!arg_11F_0.TryGetValue(mountIndex6.Key.Bank, out bankRiderInfo))
					{
						StrategicAI.BankRiderInfo bankRiderInfo2 = new StrategicAI.BankRiderInfo();
						StrategicAI.BankRiderInfo arg_144_0 = bankRiderInfo2;
						KeyValuePair<LogicalMount, int> mountIndex2 = mountIndex;
						arg_144_0.Bank = mountIndex2.Key.Bank;
						bankRiderInfo = bankRiderInfo2;
						dictionary.Add(bankRiderInfo.Bank, bankRiderInfo);
						if (shipInfo != null)
						{
							bankRiderInfo.AllocatedRole = new ShipRole?(shipInfo.DesignInfo.Role);
						}
					}
					if (shipInfo == null)
					{
						List<int> arg_194_0 = bankRiderInfo.FreeRiderIndices;
						KeyValuePair<LogicalMount, int> mountIndex3 = mountIndex;
						arg_194_0.Add(mountIndex3.Value);
					}
					else
					{
						List<int> arg_1B2_0 = bankRiderInfo.FilledRiderIndices;
						KeyValuePair<LogicalMount, int> mountIndex4 = mountIndex;
						arg_1B2_0.Add(mountIndex4.Value);
					}
				}
				foreach (StrategicAI.BankRiderInfo current2 in 
					from x in dictionary.Values
					where x.FreeRiderIndices.Count > 0
					select x)
				{
					if (!current2.AllocatedRole.HasValue)
					{
						ShipInfo shipInfo2 = battleRiderMountSet.EnumerateByTurretClass(current2.Bank.TurretClass).FirstOrDefault<ShipInfo>();
						if (shipInfo2 != null)
						{
							current2.AllocatedRole = new ShipRole?(shipInfo2.DesignInfo.Role);
						}
					}
					if (current2.AllocatedRole.HasValue)
					{
						while (current2.FreeRiderIndices.Count > 0)
						{
							ShipInfo shipInfo3 = battleRiderMountSet.FindShipByRole(current2.AllocatedRole.Value);
							if (shipInfo3 == null)
							{
								num += current2.FreeRiderIndices.Count;
								break;
							}
							battleRiderMountSet.AttachBattleRiderToShip(this._db, current2, shipInfo3, current, current2.FreeRiderIndices[0]);
						}
					}
				}
			}
			if (num2 > 0)
			{
				return (float)num / (float)num2;
			}
			return 0f;
		}
		private void SetFleetOrders(AIInfo aiInfo)
		{
			StrategicAI.TraceVerbose("Assigning missions...");
			SortedDictionary<MissionType, int> sortedDictionary = new SortedDictionary<MissionType, int>();
			foreach (MissionType key in Enum.GetValues(typeof(MissionType)))
			{
				sortedDictionary[key] = 0;
			}
			switch (aiInfo.Stance)
			{
			case AIStance.CONQUERING:
				sortedDictionary[MissionType.GATE] = 6;
				sortedDictionary[MissionType.DEPLOY_NPG] = 6;
				sortedDictionary[MissionType.INVASION] = 5;
				sortedDictionary[MissionType.STRIKE] = 4;
				sortedDictionary[MissionType.PATROL] = 3;
				sortedDictionary[MissionType.COLONIZATION] = 2;
				sortedDictionary[MissionType.SURVEY] = 2;
				sortedDictionary[MissionType.UPGRADE_STN] = 1;
				sortedDictionary[MissionType.CONSTRUCT_STN] = 1;
				break;
			case AIStance.DESTROYING:
				sortedDictionary[MissionType.GATE] = 5;
				sortedDictionary[MissionType.DEPLOY_NPG] = 5;
				sortedDictionary[MissionType.INVASION] = 4;
				sortedDictionary[MissionType.STRIKE] = 4;
				sortedDictionary[MissionType.PATROL] = 3;
				sortedDictionary[MissionType.COLONIZATION] = 2;
				sortedDictionary[MissionType.SURVEY] = 2;
				sortedDictionary[MissionType.UPGRADE_STN] = 1;
				sortedDictionary[MissionType.CONSTRUCT_STN] = 1;
				break;
			case AIStance.DEFENDING:
				sortedDictionary[MissionType.PATROL] = 4;
				sortedDictionary[MissionType.GATE] = 3;
				sortedDictionary[MissionType.DEPLOY_NPG] = 3;
				sortedDictionary[MissionType.INVASION] = 2;
				sortedDictionary[MissionType.STRIKE] = 2;
				sortedDictionary[MissionType.SURVEY] = 1;
				sortedDictionary[MissionType.COLONIZATION] = 1;
				sortedDictionary[MissionType.UPGRADE_STN] = 1;
				sortedDictionary[MissionType.CONSTRUCT_STN] = 1;
				break;
			default:
				sortedDictionary[MissionType.GATE] = 5;
				sortedDictionary[MissionType.CONSTRUCT_STN] = 4;
				sortedDictionary[MissionType.DEPLOY_NPG] = 3;
				sortedDictionary[MissionType.COLONIZATION] = 3;
				sortedDictionary[MissionType.UPGRADE_STN] = 2;
				sortedDictionary[MissionType.SURVEY] = 1;
				break;
			}
			if (App.Log.Level >= LogLevel.Verbose)
			{
				StrategicAI.TraceVerbose(string.Format("  General priorities for stance {0} are:", aiInfo.Stance));
				foreach (KeyValuePair<MissionType, int> current in 
					from x in sortedDictionary
					orderby x.Value descending
					select x)
				{
					StrategicAI.TraceVerbose(string.Format("    {0}: {1}", current.Value, current.Key));
				}
			}
			foreach (FleetInfo current2 in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL))
			{
				MissionInfo missionByFleetID = this._db.GetMissionByFleetID(current2.ID);
				if (missionByFleetID != null && missionByFleetID.Type == MissionType.PATROL && aiInfo.Stance != AIStance.DEFENDING)
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, current2, true);
					StrategicAI.TraceVerbose(string.Format("  Cancelled existing patrol mission for fleet {0} because stance is not DEFENDING.", current2));
				}
			}
			IEnumerable<AIFleetInfo> aIFleetInfos = this._db.GetAIFleetInfos(this._player.ID);
			List<FleetInfo> list = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo fleet in list)
			{
				int supportingSystemID = fleet.SupportingSystemID;
				if (fleet.AdmiralID == 0 || fleet.SupportingSystemID == 0 || this._db.GetMissionByFleetID(fleet.ID) != null || this._db.GetShipInfoByFleetID(fleet.ID, false).Count<ShipInfo>() < 1 || fleet.SystemID != supportingSystemID)
				{
					StrategicAI.TraceVerbose(string.Format("  Skipping busy fleet {0}.", fleet));
				}
				else
				{
					AIFleetInfo aIFleetInfo = aIFleetInfos.FirstOrDefault((AIFleetInfo x) => x.FleetID.HasValue && x.FleetID.Value == fleet.ID);
					if (aIFleetInfo == null || aIFleetInfo.InvoiceID.HasValue)
					{
						StrategicAI.TraceVerbose(string.Format("  Skipping unqualified fleet {0}.", fleet));
					}
					else
					{
						List<MissionType> list2 = null;
						if (aIFleetInfo != null)
						{
							list2 = MissionTypeExtensions.DeserializeList(aIFleetInfo.FleetType);
						}
						IOrderedEnumerable<KeyValuePair<MissionType, int>> orderedEnumerable = 
							from x in sortedDictionary
							orderby x.Value descending
							select x;
						bool flag = false;
						StrategicAI.TraceVerbose(string.Format("  Trying missions for fleet {0}...", fleet));
						foreach (KeyValuePair<MissionType, int> current3 in orderedEnumerable)
						{
							if (flag)
							{
								break;
							}
							if (current3.Value >= 1 && (list2 == null || list2.Contains(current3.Key)))
							{
								StrategicAI.TraceVerbose(string.Format("    {0}: {1}...", current3.Value, current3.Key));
								switch (current3.Key)
								{
								case MissionType.COLONIZATION:
									flag = this.TryColonizationMission(fleet.ID);
									break;
								case MissionType.SURVEY:
									flag = this.TrySurveyMission(fleet.ID);
									break;
								case MissionType.CONSTRUCT_STN:
									flag = this.TryConstructionMission(aiInfo, fleet.ID);
									break;
								case MissionType.UPGRADE_STN:
									flag = this.TryUpgradeMission(aiInfo, fleet.ID);
									break;
								case MissionType.PATROL:
									flag = this.TryPatrolMission(fleet.ID);
									break;
								case MissionType.STRIKE:
									flag = this.TryStrikeMission(fleet.ID);
									break;
								case MissionType.INVASION:
									flag = this.TryInvasionMission(fleet.ID);
									break;
								case MissionType.GATE:
									if (this._player.Faction.Name == "hiver")
									{
										flag = this.TryGateMission(fleet.ID);
									}
									break;
								case MissionType.DEPLOY_NPG:
									if (this._player.Faction.Name == "loa")
									{
										flag = this.TryDeployNPGMission(fleet.ID);
									}
									break;
								}
								if (flag)
								{
									StrategicAI.TraceVerbose("      OK.");
								}
							}
						}
						if (!flag)
						{
							StrategicAI.TraceVerbose("     No missions could be assigned.");
						}
					}
				}
			}
		}
		private void ManageColonies(AIInfo aiInfo)
		{
			PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
			float playerSuitability = this._db.GetPlayerSuitability(this._player.ID);
			int num = 100;
			float num2 = 1f;
			foreach (ColonyInfo current in this._db.GetPlayerColoniesByPlayerId(this._player.ID).ToList<ColonyInfo>())
			{
				float num3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				float num6 = 0f;
				PlanetInfo planetInfo = this._db.GetPlanetInfo(current.OrbitalObjectID);
				this._db.GetOrbitalObjectInfo(current.OrbitalObjectID);
				if (planetInfo.Infrastructure < 1f)
				{
					num4 = 1f - planetInfo.Infrastructure;
				}
				float num7 = Math.Abs(planetInfo.Suitability - playerSuitability);
				if (num7 > 0f)
				{
					num3 = 1f - num4;
				}
				double industrialOutput = Colony.GetIndustrialOutput(this._game, current, planetInfo);
				IEnumerable<FreighterInfo> source = 
					from x in this._db.GetFreighterInfosForSystem(current.CachedStarSystemID)
					where x.PlayerId == aiInfo.PlayerID
					select x;
				if (source.Count<FreighterInfo>() > 0)
				{
					if (industrialOutput <= 1000.0)
					{
						num6 = 1f;
					}
					else
					{
						List<double> tradeRatesForWholeExportsForColony = this._game.GetTradeRatesForWholeExportsForColony(current.ID);
						num6 = ((tradeRatesForWholeExportsForColony.Count<double>() < source.Count<FreighterInfo>()) ? ((float)tradeRatesForWholeExportsForColony.Last<double>()) : ((float)tradeRatesForWholeExportsForColony.ElementAt(source.Count<FreighterInfo>() - 1)));
					}
				}
				if (planetInfo.Infrastructure > 0.8f && num7 < 100f && industrialOutput > 1000.0)
				{
					num5 = Math.Max(1f - (num3 + num4 + num6), 0f);
					if (aiInfo.Stance == AIStance.ARMING)
					{
						if (num5 < 0.3f)
						{
							num5 = 0.3f;
						}
					}
					else
					{
						if (aiInfo.Stance == AIStance.CONQUERING || aiInfo.Stance == AIStance.DESTROYING)
						{
							if (num5 < 0.5f)
							{
								num5 = 0.5f;
							}
						}
						else
						{
							if (aiInfo.Stance == AIStance.DEFENDING && num5 < 0.7f)
							{
								num5 = 0.7f;
							}
						}
					}
				}
				if (playerInfo.Savings < -1000000.0)
				{
					num5 = 0f;
					if (source.Count<FreighterInfo>() > 0)
					{
						num6 = 1f;
					}
				}
				num3 = Math.Max(0f, num3);
				num4 = Math.Max(0f, num4);
				num5 = Math.Max(0f, num5);
				num6 = Math.Max(0f, num6);
				float num8 = num3 + num4 + num5 + num6;
				if (num8 > 0f && num8 != 1f)
				{
					float num9 = 1f / num8;
					num3 *= num9;
					num4 *= num9;
					num5 *= num9;
					num6 *= num9;
				}
				current.TerraRate = num3;
				current.InfraRate = num4;
				current.ShipConRate = num5;
				current.TradeRate = num6;
				current.CivilianWeight = 0.75f;
				if (current.Factions.Any<ColonyFactionInfo>())
				{
					num = Math.Min(num, (int)current.Factions.Average((ColonyFactionInfo x) => x.Morale));
				}
				num2 = Math.Min(num2, current.EconomyRating);
				this._db.UpdateColony(current);
			}
			List<AssetDatabase.MoralModifier> moraleEffects = GameSession.GetMoraleEffects(this._game.App, MoralEvent.ME_TAX_INCREASED, this._player.ID);
			List<AssetDatabase.MoralModifier> list = new List<AssetDatabase.MoralModifier>();
			if (num2 >= 1f)
			{
				list = GameSession.GetMoraleEffects(this._game.App, MoralEvent.ME_ECONOMY_100, this._player.ID);
			}
			else
			{
				if (num2 >= 0.85f)
				{
					list = GameSession.GetMoraleEffects(this._game.App, MoralEvent.ME_ECONOMY_ABOVE_85, this._player.ID);
				}
			}
			int num10 = 0;
			foreach (AssetDatabase.MoralModifier current2 in moraleEffects)
			{
				num10 = Math.Max(num10, Math.Abs(current2.value));
			}
			int num11 = 2147483647;
			foreach (AssetDatabase.MoralModifier current3 in list)
			{
				num11 = Math.Min(num11, current3.value);
			}
			float num12 = (this._player.Faction.Name == "zuul") ? 0.07f : 0.08f;
			float num13 = 0.06f;
			if (num11 < 20)
			{
				if (num10 == 0)
				{
					num13 = num12;
				}
				else
				{
					int i;
					for (i = 0; i < 5; i++)
					{
						float num14 = (float)i * 2f;
						if ((float)Math.Abs(num10) * num14 > (float)num11)
						{
							i = Math.Max(i - 1, 0);
							break;
						}
					}
					num13 = 0.05f + 0.01f * (float)i;
					num13 = Math.Min(num13, num12);
				}
			}
			float num15 = ((num2 >= 0.85f && num > 60) || num2 >= 1f) ? num13 : 0.05f;
			float num16 = num12;
			if (num < 30)
			{
				num16 = 0.02f;
			}
			else
			{
				if (num < 40)
				{
					num16 = 0.03f;
				}
				else
				{
					if (num < 50)
					{
						num16 = num15;
					}
					else
					{
						if (num < 70)
						{
							if (playerInfo.RateTax >= num12)
							{
								num16 = num12;
							}
							else
							{
								num16 = num15;
							}
						}
					}
				}
			}
			if (Math.Abs(playerInfo.RateTax - num16) >= 0.005f)
			{
				this._db.UpdateTaxRate(this._player.ID, num16);
			}
		}
		private int GetNearestUngatedSystem(FleetInfo fleetInfo)
		{
			if (fleetInfo == null)
			{
				return 0;
			}
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fleetInfo.SupportingSystemID);
			if (starSystemInfo == null)
			{
				return 0;
			}
			Faction faction = this.Game.AssetDatabase.GetFaction(this.Game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			Vector3 arg_4F_0 = starSystemInfo.Origin;
			int result = 0;
			int num = 2147483647;
			foreach (StarSystemInfo current in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (!this._db.SystemHasGate(current.ID, this._player.ID))
				{
					bool flag = false;
					foreach (MissionInfo current2 in this._db.GetMissionInfos())
					{
						if (current2.TargetSystemID == current.ID && current2.Type == MissionType.SURVEY && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
                        int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this.Game, fleetInfo, current.ID, faction.CanUseNodeLine(new bool?(true)), null, null);
						if (travelTime.HasValue && travelTime.Value < num)
						{
							result = current.ID;
							num = travelTime.Value;
						}
					}
				}
			}
			return result;
		}
		private int GetNearestNPGRouteSystem(FleetInfo fleetInfo)
		{
			if (fleetInfo == null)
			{
				return 0;
			}
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fleetInfo.SupportingSystemID);
			if (starSystemInfo == null)
			{
				return 0;
			}
			Faction faction = this.Game.AssetDatabase.GetFaction(this.Game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			Vector3 arg_4F_0 = starSystemInfo.Origin;
			int result = 0;
			int num = 2147483647;
			foreach (StarSystemInfo current in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (current.ID != fleetInfo.SystemID && this._db.GetNodeLineBetweenSystems(fleetInfo.PlayerID, starSystemInfo.ID, current.ID, false, true) == null)
				{
					bool flag = false;
					foreach (MissionInfo current2 in this._db.GetMissionInfos())
					{
						if (current2.TargetSystemID == current.ID && current2.Type == MissionType.DEPLOY_NPG && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					ExploreRecordInfo exploreRecord = this._db.GetExploreRecord(current.ID, this._player.ID);
					if (exploreRecord != null && exploreRecord.Explored && !flag)
					{
                        int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this.Game, fleetInfo, current.ID, faction.CanUseNodeLine(new bool?(true)), null, null);
						float num2 = Math.Abs((starSystemInfo.Origin - current.Origin).Length);
						if (travelTime.HasValue && travelTime.Value < num && num2 <= 15f)
						{
							result = current.ID;
							num = travelTime.Value;
						}
					}
				}
			}
			return result;
		}
		private int GetBestUngatedSystem(FleetInfo fleet)
		{
			int nearestUnexploredSystem = this.GetNearestUnexploredSystem(fleet);
			if (nearestUnexploredSystem != 0)
			{
				return nearestUnexploredSystem;
			}
			return this.GetNearestUngatedSystem(fleet);
		}
		private bool TryGateMission(int fleetID)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int targetSystem = this.GetBestUngatedSystem(fleetInfo);
			if (targetSystem != 0)
			{
				IEnumerable<MissionInfo> enumerable = this._db.GetMissionInfos();
				if (enumerable != null)
				{
					enumerable = 
						from x in enumerable
						where x.TargetSystemID == targetSystem && x.Type == MissionType.GATE
						select x;
					foreach (MissionInfo current in enumerable)
					{
						FleetInfo fleetInfo2 = this._db.GetFleetInfo(current.FleetID);
						if (fleetInfo2.PlayerID == fleetInfo.PlayerID)
						{
							return false;
						}
					}
				}
				if (!this._db.SystemHasGate(targetSystem, this._player.ID))
				{
					List<int> designIds = null;
                    if (Kerberos.Sots.StarFleet.StarFleet.CanDoGateMissionToTarget(this._game, targetSystem, fleetID, null, null, null))
					{
                        Kerberos.Sots.StarFleet.StarFleet.SetGateMission(this._game, fleetID, targetSystem, false, designIds, null);
						return true;
					}
				}
				this.TryRelocateMissionCloserToTargetSystem(fleetID, targetSystem);
			}
			return false;
		}
		private bool TryDeployNPGMission(int fleetID)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int nearestNPGRouteSystem = this.GetNearestNPGRouteSystem(fleetInfo);
			if (nearestNPGRouteSystem != 0)
			{
                List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetAccelGatePercentPointsBetweenSystems(this._db, fleetInfo.SystemID, nearestNPGRouteSystem).ToList<int>();
				if (list.Count < 1)
				{
					list.Clear();
				}
                Kerberos.Sots.StarFleet.StarFleet.SetNPGMission(this._game, fleetID, nearestNPGRouteSystem, true, list, new List<int>(), null);
				return true;
			}
			return false;
		}
		private bool TryTransferMission(int fleetID)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			if (this._game.GameDatabase.GetRemainingSupportPoints(this._game, fleetInfo.SupportingSystemID, fleetInfo.PlayerID) == 0)
			{
				int num = this.FindSystemForTransferMission(fleetID);
                if (num > 0 && Kerberos.Sots.StarFleet.StarFleet.CanDoTransferMissionToTarget(this._game, num, fleetID))
				{
                    Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._game, fleetID, num, false, null);
					return true;
				}
				return false;
			}
			else
			{
				int num2 = 0;
				foreach (FleetInfo current in this._db.GetFleetsBySupportingSystem(fleetInfo.SupportingSystemID, FleetType.FL_NORMAL))
				{
					if (!current.IsReserveFleet)
					{
						num2++;
					}
				}
				if (num2 <= 1)
				{
					return false;
				}
				int num3 = this.FindSystemForTransferMission(fleetID);
                if (num3 > 0 && Kerberos.Sots.StarFleet.StarFleet.CanDoTransferMissionToTarget(this._game, num3, fleetID))
				{
                    Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._game, fleetID, num3, false, null);
					return true;
				}
				return false;
			}
		}
		private bool HaveNewColonizationMissionEnroute()
		{
			IEnumerable<MissionInfo> source = 
				from x in this._db.GetMissionInfos()
				where x.Type == MissionType.COLONIZATION && x.TargetOrbitalObjectID != 0 && this._db.GetFleetInfo(x.FleetID).PlayerID == this._player.ID && (this._db.GetColonyInfoForPlanet(x.TargetOrbitalObjectID) == null || this._db.GetColonyInfoForPlanet(x.TargetOrbitalObjectID).PlayerID != this._player.ID)
				select x;
			return source.Any<MissionInfo>();
		}
		private void ManageDebtLevels()
		{
			if (this._player.Faction.CanUseGate() && this.m_TurnBudget.CurrentSavings < -500000.0 && this.m_ColonizedSystems.Count > 3)
			{
				foreach (SystemDefendInfo system in this.m_ColonizedSystems)
				{
					if (!(this._player.PlayerInfo.Homeworld == system.SystemID) && !this._db.SystemHasGate(system.SystemID, this._player.ID))
					{
						bool flag = true;
						List<MissionInfo> list = (
							from x in this._db.GetMissionsBySystemDest(system.SystemID)
							where x.Type == MissionType.GATE && x.TargetSystemID == system.SystemID
							select x).ToList<MissionInfo>();
						foreach (MissionInfo current in list)
						{
							FleetInfo fleetInfo = this._db.GetFleetInfo(current.FleetID);
							if (fleetInfo != null && fleetInfo.PlayerID == this._player.ID)
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							float playerSuitability = this._db.GetPlayerSuitability(this._player.ID);
							List<ColonyInfo> list2 = this._db.GetColonyInfosForSystem(system.SystemID).ToList<ColonyInfo>();
							foreach (ColonyInfo current2 in list2)
							{
								PlanetInfo planetInfo = this._db.GetPlanetInfo(current2.OrbitalObjectID);
								if (planetInfo.Infrastructure < 0.8f || Math.Abs(planetInfo.Suitability - playerSuitability) > 100f)
								{
									GameSession.AbandonColony(this._game.App, current2.ID);
								}
							}
						}
					}
				}
			}
			if (this.m_TurnBudget.CurrentSavings < -1000000.0)
			{
				List<MissionInfo> list3 = (
					from x in this._db.GetMissionInfos()
					where x.Type == MissionType.CONSTRUCT_STN || x.Type == MissionType.UPGRADE_STN
					select x).ToList<MissionInfo>();
				if (this.m_TurnBudget.CurrentSavings < -2000000.0)
				{
					list3.AddRange((
						from x in this._db.GetMissionInfos()
						where x.Type == MissionType.COLONIZATION || x.Type == MissionType.SUPPORT
						select x).ToList<MissionInfo>());
				}
				foreach (MissionInfo current3 in list3)
				{
					FleetInfo fleetInfo2 = this._db.GetFleetInfo(current3.FleetID);
					if (fleetInfo2 != null && fleetInfo2.PlayerID == this._player.ID)
					{
                        Kerberos.Sots.StarFleet.StarFleet.CancelMission(this._game, fleetInfo2, true);
					}
				}
				foreach (SystemDefendInfo current4 in this.m_ColonizedSystems)
				{
					List<BuildOrderInfo> list4 = this._db.GetBuildOrdersForSystem(current4.SystemID).ToList<BuildOrderInfo>();
					foreach (BuildOrderInfo bi in list4)
					{
						if ((float)bi.Progress / (float)bi.ProductionTarget <= 0.9f)
						{
							if (bi.InvoiceID.HasValue)
							{
								List<BuildOrderInfo> source = this._db.GetBuildOrdersForInvoiceInstance(bi.InvoiceID.Value).ToList<BuildOrderInfo>();
								DesignInfo designInfo = this._db.GetDesignInfo(bi.DesignID);
								if (!designInfo.isPrototyped)
								{
									if (source.Any((BuildOrderInfo x) => x != bi && x.DesignID == bi.DesignID))
									{
										BuildOrderInfo buildOrderInfo = source.First((BuildOrderInfo x) => x != bi && x.DesignID == bi.DesignID);
										buildOrderInfo.ProductionTarget = designInfo.GetPlayerProductionCost(this._db, this._player.ID, true, null);
										this._db.UpdateBuildOrder(buildOrderInfo);
									}
								}
							}
							this._db.RemoveBuildOrder(bi.ID);
						}
					}
				}
			}
		}
		private double GetUsableSavings(Budget b)
		{
			if (b.NetSavingsIncome < b.NetSavingsLoss)
			{
				return 0.0;
			}
			return b.CurrentSavings - 2000000.0;
		}
		private double GetUsableBuildFunds(Budget b, double minSavings)
		{
			if (b.NetSavingsIncome < b.NetSavingsLoss && b.CurrentSavings < 4000000.0)
			{
				return 0.0;
			}
			return b.CurrentSavings - minSavings + b.NetSavingsIncome;
		}
		private bool canBudget(Budget b)
		{
			return (b.NetSavingsIncome >= b.NetSavingsLoss || b.CurrentSavings >= 4000000.0) && b.CurrentSavings >= 2000000.0;
		}
		private double GetAvailableColonySupportBudget()
		{
			double num = 0.4;
			double result;
			if (this.m_IsOldSave)
			{
				Budget budget = Budget.GenerateBudget(this._game, this._db.GetPlayerInfo(this._player.ID), null, BudgetProjection.Actual);
				result = (budget.TotalRevenue - budget.ColonySupportExpenses) * num;
			}
			else
			{
				result = this.m_TurnBudget.TotalRevenue * num - this.m_TurnBudget.ColonySupportExpenses;
			}
			return result;
		}
		private double GetAvailableStationSupportBudget()
		{
			if (this.m_TurnBudget.CurrentSavings < 1000000.0)
			{
				return 0.0;
			}
			double val = (this.m_TurnBudget.TotalRevenue - this.m_TurnBudget.ColonySupportExpenses) * 0.5 - this.m_TurnBudget.CurrentStationUpkeepExpenses;
			return Math.Max(val, 0.0);
		}
		private double GetAvailableFleetSupportBudget()
		{
			double num = this.m_TurnBudget.TotalRevenue * 0.5;
			num = num * 0.6 - this.m_TurnBudget.CurrentShipUpkeepExpenses;
			return Math.Max(num, 0.0);
		}
		private double GetAvailableDefenseBudget()
		{
			Budget b = Budget.GenerateBudget(this._game, this._db.GetPlayerInfo(this._player.ID), null, BudgetProjection.Actual);
			double num = this.GetUsableBuildFunds(b, 1500000.0);
			num *= 0.2;
			return Math.Max(num, 0.0);
		}
		private double GetAvailableFillFleetConstructionBudget()
		{
			double num = this.m_TurnBudget.ProjectedSavings;
			if (this._player.Faction.Name != "hiver")
			{
				num -= 250000.0;
			}
			return Math.Max(num * 0.2, 0.0);
		}
		private double GetAvailablePrototypeShipConstructionBudget()
		{
			if (!this.canBudget(this.m_TurnBudget))
			{
				return 0.0;
			}
			double num = this.GetUsableBuildFunds(this.m_TurnBudget, 1500000.0);
			if (num > 0.0)
			{
				double num2 = 500000.0;
				double num3 = num - num2;
				if (num3 > 0.0)
				{
					num = num2 * 0.8 + num3 * 0.4;
				}
				else
				{
					num *= 0.8;
				}
			}
			return Math.Max(num, 0.0);
		}
		private double GetAvailableShipConstructionBudget(AIStance stance)
		{
			if (!this.canBudget(this.m_TurnBudget))
			{
				return 0.0;
			}
			double num = this.GetUsableBuildFunds(this.m_TurnBudget, 2000000.0);
			num *= 0.5;
			return Math.Max(num, 0.0);
		}
		private bool TryColonizationMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int num = 0;
			if (!this.HaveNewColonizationMissionEnroute())
			{
				num = this.GetBestPlanetToColonize(this.GetAvailableColonySupportBudget());
			}
			if (num > 0)
			{
				int starSystemID = this._db.GetOrbitalObjectInfo(num).StarSystemID;
				bool flag = false;
				List<int> list = new List<int>();
				int num2 = 10;
                int designForColonizer = Kerberos.Sots.StarFleet.StarFleet.GetDesignForColonizer(this._game, this._player.ID);
				if (designForColonizer != 0)
				{
					for (int i = 0; i <= num2; i += 2)
					{
						if (i > 0)
						{
							list.Add(designForColonizer);
						}
                        flag = Kerberos.Sots.StarFleet.StarFleet.CanDoColonizeMissionToTarget(this._game, starSystemID, fleetInfo.ID, null, null, null);
                        if (flag && Kerberos.Sots.StarFleet.StarFleet.ProjectNumColonizationRuns(this._game, num, fleetInfo.ID, list, 10) > 10)
						{
							flag = false;
						}
						if (flag)
						{
							break;
						}
					}
				}
				else
				{
                    flag = Kerberos.Sots.StarFleet.StarFleet.CanDoColonizeMissionToTarget(this._game, starSystemID, fleetInfo.ID, null, null, null);
                    if (flag && Kerberos.Sots.StarFleet.StarFleet.ProjectNumColonizationRuns(this._game, num, fleetInfo.ID, list, 10) > 10)
					{
						flag = false;
					}
				}
				if (flag)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(num);
                    Kerberos.Sots.StarFleet.StarFleet.SetColonizationMission(this._game, fleetInfo.ID, orbitalObjectInfo.StarSystemID, false, num, list, null);
					return true;
				}
				this.TryRelocateMissionCloserToTargetSystem(fleetID, this._db.GetOrbitalObjectInfo(num).StarSystemID);
			}
			return false;
		}
		private static void TraceVerbose(string message)
		{
			App.Log.Trace(message, "ai", LogLevel.Verbose);
		}
		private bool TrySurveyMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int nearestUnexploredSystem = this.GetNearestUnexploredSystem(fleetInfo);
			if (nearestUnexploredSystem != 0)
			{
                if (Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(this._game, nearestUnexploredSystem, fleetInfo.ID, null, null, null))
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.SURVEY, StationType.INVALID_TYPE, fleetInfo.ID, nearestUnexploredSystem, 0, null, 1, false, null, null);
					if (missionEstimate.TurnsToTarget >= 0)
					{
						int num = missionEstimate.TurnsToTarget + missionEstimate.TurnsToReturn + missionEstimate.TurnsAtTarget;
                        Kerberos.Sots.StarFleet.StarFleet.SetSurveyMission(this._game, fleetInfo.ID, nearestUnexploredSystem, false, null, null);
						StrategicAI.TraceVerbose("Mission estimate, total " + num + " turns.");
						return true;
					}
				}
				this.TryRelocateMissionCloserToTargetSystem(fleetID, nearestUnexploredSystem);
			}
			return false;
		}
		public void HandleSurveyMissionCompleted(int fleetID, int systemID)
		{
			List<ShipInfo> list = new List<ShipInfo>();
			List<ShipInfo> list2 = this._game.GameDatabase.GetShipInfoByFleetID(fleetID, true).ToList<ShipInfo>();
			foreach (ShipInfo current in list2)
			{
				ShipSectionAsset shipSectionAsset = (
					from x in current.DesignInfo.DesignSections
					select x.ShipSectionAsset).FirstOrDefault((ShipSectionAsset x) => x.IsTrapShip);
				if (shipSectionAsset != null)
				{
					list.Add(current);
				}
			}
			if (list.Count > 0)
			{
				List<PlanetInfo> list3 = new List<PlanetInfo>();
				List<PlanetInfo> list4 = this._game.GameDatabase.GetStarSystemPlanetInfos(systemID).ToList<PlanetInfo>();
				foreach (PlanetInfo current2 in list4)
				{
					if (this._game.AssetDatabase.IsPotentialyHabitable(current2.Type) && this._game.GameDatabase.GetColonyInfoForPlanet(current2.ID) == null)
					{
						list3.Add(current2);
					}
				}
				if (list3.Count > 0)
				{
					int num = this._random.NextInclusive(0, Math.Min(list.Count, list3.Count));
					for (int i = 0; i < num; i++)
					{
						PlanetInfo planetInfo = this._random.Choose(list3);
						ShipInfo shipInfo = list.First<ShipInfo>();
						this._game.SetAColonyTrap(shipInfo, this._player.ID, systemID, planetInfo.ID);
						list3.Remove(planetInfo);
						list.Remove(shipInfo);
						if (list3.Count == 0)
						{
							break;
						}
						if (list.Count == 0)
						{
							return;
						}
					}
				}
			}
		}
		private bool ValidateConstructionFleet(int fleetID)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			FleetInfo fleetInfo2 = this._db.InsertOrGetReserveFleetInfo(fleetInfo.SupportingSystemID, fleetInfo.PlayerID);
            int num = Kerberos.Sots.StarFleet.StarFleet.GetNumConstructionShips(this._game, fleetInfo.ID);
			int num2 = 0;
			if (fleetInfo2 != null)
			{
                num2 = Kerberos.Sots.StarFleet.StarFleet.GetNumConstructionShips(this._game, fleetInfo2.ID);
			}
			if (num + num2 < 1)
			{
				return false;
			}
			if (num < 5 && num2 > 0)
			{
				foreach (ShipInfo current in this._db.GetShipInfoByFleetID(fleetInfo2.ID, false).ToList<ShipInfo>())
				{
					foreach (string sectionName in this._game.GameDatabase.GetDesignSectionNames(current.DesignID))
					{
						ShipSectionAsset shipSectionAsset = this._game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
						if (shipSectionAsset != null && shipSectionAsset.isConstructor)
						{
							this._db.TransferShip(current.ID, fleetInfo.ID);
							num++;
							break;
						}
					}
					if (num >= 5)
					{
						break;
					}
				}
			}
			return true;
		}
		private bool TryUpgradeMission(AIInfo aiInfo, int fleetID)
		{
			return this._db.GetMissionByFleetID(fleetID) == null && this.ValidateConstructionFleet(fleetID) && this.CreateConstructionMission(aiInfo, fleetID, true);
		}
		private bool TryConstructionMission(AIInfo aiInfo, int fleetID)
		{
			return this._db.GetMissionByFleetID(fleetID) == null && this.ValidateConstructionFleet(fleetID) && this.CreateConstructionMission(aiInfo, fleetID, false);
		}
		private bool OptimizeFleetForMission(FleetInfo fleet, MissionType missionType)
		{
			return true;
		}
		private bool TryGateConstructionMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int systemForHiverGate = this.GetSystemForHiverGate(fleetInfo.SystemID);
            if (systemForHiverGate > 0 && Kerberos.Sots.StarFleet.StarFleet.CanDoConstructionMissionToTarget(this._game, systemForHiverGate, fleetInfo.ID, false))
			{
                Kerberos.Sots.StarFleet.StarFleet.SetConstructionMission(this._game, fleetInfo.ID, systemForHiverGate, false, 0, null, StationType.GATE, null);
				return true;
			}
			return false;
		}
		private bool TryPatrolMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			int bestPatrolTargetSystem = this.GetBestPatrolTargetSystem(fleetID);
			if (bestPatrolTargetSystem <= 0)
			{
				return false;
			}
            if (Kerberos.Sots.StarFleet.StarFleet.CanDoPatrolMissionToTarget(this._game, bestPatrolTargetSystem, fleetID, null, null, null))
			{
                Kerberos.Sots.StarFleet.StarFleet.SetPatrolMission(this._game, fleetID, bestPatrolTargetSystem, false, null, null);
				return true;
			}
			this.TryRelocateMissionCloserToTargetSystem(fleetID, bestPatrolTargetSystem);
			return false;
		}
		private bool VerifyStrikeMission(int orbitalObject)
		{
			OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(orbitalObject);
			return this.VerifyStrikeMission(orbitalObjectInfo);
		}
		private bool VerifyStrikeMission(OrbitalObjectInfo orbit)
		{
			if (orbit == null)
			{
				return false;
			}
			IEnumerable<MissionInfo> enumerable = 
				from x in this._db.GetMissionInfos()
				where x.TargetSystemID == orbit.StarSystemID
				select x;
			foreach (MissionInfo current in enumerable)
			{
				FleetInfo fleetInfo = this._db.GetFleetInfo(current.FleetID);
				if (fleetInfo.PlayerID == this._player.ID && current.Type == MissionType.STRIKE)
				{
					return false;
				}
			}
			return true;
		}
		private bool TryStrikeMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			int bestStrikeTargetOrbitalObject = this.GetBestStrikeTargetOrbitalObject(fleetID);
			if (bestStrikeTargetOrbitalObject == 0)
			{
				return false;
			}
			OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(bestStrikeTargetOrbitalObject);
            if (Kerberos.Sots.StarFleet.StarFleet.CanDoStrikeMissionToTarget(this._game, orbitalObjectInfo.StarSystemID, fleetID, null, null, null))
			{
                Kerberos.Sots.StarFleet.StarFleet.SetStrikeMission(this._game, fleetID, orbitalObjectInfo.StarSystemID, false, bestStrikeTargetOrbitalObject, 0, null);
				return true;
			}
			this.TryRelocateMissionCloserToTargetSystem(fleetID, orbitalObjectInfo.StarSystemID);
			return false;
		}
		private int GetBestStrikeTargetOrbitalObject(int fleetID)
		{
			int fleetSupportingSystem = this._db.GetFleetSupportingSystem(fleetID);
			Vector3 origin = this._db.GetStarSystemInfo(fleetSupportingSystem).Origin;
			float num = this._db.FindCurrentDriveSpeedForPlayer(this._player.ID);
			int result = 0;
			List<int> list = new List<int>();
			List<int> list2 = (
				from x in this._db.GetPlayerIDs()
				where x != this._player.ID
				select x).ToList<int>();
			List<int> allies = (
				from x in list2
				where this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, x) == DiplomacyState.ALLIED
				select x).ToList<int>();
			list2.RemoveAll((int x) => allies.Contains(x));
			foreach (int pID in list2)
			{
				if (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, pID) == DiplomacyState.WAR)
				{
					bool flag = true;
					foreach (int ally in allies)
					{
						List<TreatyInfo> source = (
							from x in this._db.GetTreatyInfos()
							where x.Active && (x.InitiatingPlayerId == pID || x.InitiatingPlayerId == ally) && (x.ReceivingPlayerId == pID || x.ReceivingPlayerId == ally)
							select x).ToList<TreatyInfo>();
						if (source.Any((TreatyInfo x) => x.Type == TreatyType.Protectorate))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						int targetForStrikeMission = this.GetTargetForStrikeMission(fleetID, pID);
						if (targetForStrikeMission != 0)
						{
							list.Add(targetForStrikeMission);
						}
					}
				}
			}
			if (list.Count<int>() > 0)
			{
				if (list.Count<int>() > 1)
				{
					int num2 = 0;
					float num3 = 0f;
					foreach (int current in list)
					{
						OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(current);
						Vector3 origin2 = this._db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID).Origin;
						float length = (origin2 - origin).Length;
						if (num2 == 0 || length < num3)
						{
							num2 = current;
							num3 = length;
						}
					}
					List<int> list3 = new List<int>();
					using (List<int>.Enumerator enumerator4 = list.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							int current2 = enumerator4.Current;
							if (current2 != num2)
							{
								OrbitalObjectInfo orbitalObjectInfo2 = this._db.GetOrbitalObjectInfo(current2);
								Vector3 origin3 = this._db.GetStarSystemInfo(orbitalObjectInfo2.StarSystemID).Origin;
								bool flag2 = false;
								float length2 = (origin3 - origin).Length;
								if (length2 > num * 2f)
								{
									flag2 = true;
								}
								if (!this.VerifyStrikeMission(orbitalObjectInfo2))
								{
									flag2 = true;
								}
								if (flag2)
								{
									list3.Add(current2);
								}
							}
						}
						goto IL_331;
					}
					IL_313:
					list.Remove(list3.FirstOrDefault<int>());
					list3.Remove(list3.FirstOrDefault<int>());
					IL_331:
					if (list3.Count<int>() > 0)
					{
						goto IL_313;
					}
				}
				else
				{
					if (this.VerifyStrikeMission(list.First<int>()))
					{
						if (list.Count<int>() == 1)
						{
							result = list.FirstOrDefault<int>();
						}
						else
						{
							int index = this._random.Next(list.Count<int>());
							result = list[index];
						}
					}
				}
			}
			return result;
		}
		internal bool IsValidInvasionTarget(int orbitalObjectID)
		{
			AIColonyIntel colonyIntelForPlanet = this._db.GetColonyIntelForPlanet(this._player.ID, orbitalObjectID);
			return colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != 0 && this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, colonyIntelForPlanet.OwningPlayerID) == DiplomacyState.WAR;
		}
		private bool CanSendAnotherInvasionFleet(int targetOrbitalObjectID)
		{
			OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(targetOrbitalObjectID);
			int[] recentCombatTurns = this._db.GetRecentCombatTurns(orbitalObjectInfo.StarSystemID, this._db.GetTurnCount() - 15);
			if (recentCombatTurns.Length > 5)
			{
				return false;
			}
			int num = recentCombatTurns.Length + 1;
			int num2 = 0;
			IEnumerable<MissionInfo> missionsByPlanetDest = this._db.GetMissionsByPlanetDest(targetOrbitalObjectID);
			foreach (MissionInfo current in missionsByPlanetDest)
			{
				FleetInfo fleetInfo = this._db.GetFleetInfo(current.FleetID);
				if (current.Type == MissionType.INVASION && fleetInfo.PlayerID == this._player.ID)
				{
					num2++;
				}
			}
			return num2 < num;
		}
		private int GetBestInvasionTargetOrbitalObject(int fleetID)
		{
			List<int> targets = new List<int>();
			foreach (int current in this._db.GetPlayerIDs())
			{
				if (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, current) == DiplomacyState.WAR)
				{
					int? targetForInvasion = this.GetTargetForInvasion(fleetID, current);
					if (targetForInvasion.HasValue && this.CanSendAnotherInvasionFleet(targetForInvasion.Value))
					{
						targets.Add(targetForInvasion.Value);
					}
				}
			}
			List<TmpWeightedCheck> list = new List<TmpWeightedCheck>();
			int num = 2147483647;
			int num2 = 2147483647;
			foreach (MissionManagerTargetInfo current2 in 
				from x in this._missionMan.Targets
				where targets.Any((int y) => x.OrbitalObjectID == y)
				select x)
			{
				MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.INVASION, StationType.INVALID_TYPE, fleetID, this._db.GetOrbitalObjectInfo(current2.OrbitalObjectID).StarSystemID, current2.OrbitalObjectID, null, 1, false, null, null);
				int num3 = missionEstimate.TurnsForConstruction + missionEstimate.TurnsToTarget + this._db.GetTurnCount();
				if (num3 <= current2.ArrivalTurn)
				{
					if (num3 < num)
					{
						num = num3;
						num2 = current2.ArrivalTurn;
					}
					list.Add(new TmpWeightedCheck
					{
						TargetID = current2.OrbitalObjectID,
						ETA = num3,
						MissionETA = current2.ArrivalTurn
					});
				}
			}
			if (num < num2 || list.Count == 0)
			{
				num = 2147483647;
				num2 = 2147483647;
				return 0;
			}

			var unused = from x in list
				orderby x.ETA
				select x;
			return list[this._random.Next(Math.Min(list.Count - 1, 5))].TargetID;
		}
		private int GetBestPatrolTargetSystem(int fleetID)
		{
			this._db.GetFleetInfo(fleetID);
			IEnumerable<MissionInfo> missionInfos = this._db.GetMissionInfos();
			int num = 0;
			foreach (ColonyInfo current in this._db.GetPlayerColoniesByPlayerId(this._player.ID))
			{
				OrbitalObjectInfo ooi = this._db.GetOrbitalObjectInfo(current.OrbitalObjectID);
				if (missionInfos != null)
				{
					IEnumerable<MissionInfo> enumerable = 
						from x in missionInfos
						where x.TargetSystemID == ooi.StarSystemID || x.TargetOrbitalObjectID == ooi.ID
						select x;
					bool flag = false;
					foreach (MissionInfo current2 in enumerable)
					{
						FleetInfo fleetInfo = this._db.GetFleetInfo(current2.FleetID);
						if (current2.Type == MissionType.PATROL && fleetInfo.PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						foreach (MissionInfo current3 in enumerable)
						{
							if (current3.Type == MissionType.INVASION || current3.Type == MissionType.STRIKE)
							{
								num = ooi.StarSystemID;
								break;
							}
						}
						if (num > 0)
						{
							break;
						}
					}
				}
			}
			return num;
		}
		private bool TryInvasionMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			int bestInvasionTargetOrbitalObject = this.GetBestInvasionTargetOrbitalObject(fleetID);
			if (bestInvasionTargetOrbitalObject == 0)
			{
				return false;
			}
			OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(bestInvasionTargetOrbitalObject);
            if (Kerberos.Sots.StarFleet.StarFleet.CanDoInvasionMissionToTarget(this._game, orbitalObjectInfo.StarSystemID, fleetID, null, null, null))
			{
                Kerberos.Sots.StarFleet.StarFleet.SetInvasionMission(this._game, fleetID, orbitalObjectInfo.StarSystemID, false, bestInvasionTargetOrbitalObject, null);
				return true;
			}
			this.TryRelocateMissionCloserToTargetSystem(fleetID, orbitalObjectInfo.StarSystemID);
			return false;
		}
		private bool TryInterdictionMission(int fleetID)
		{
			if (this._db.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			this._db.GetFleetInfo(fleetID);
			return false;
		}
		private void SetResearchOrders(AIInfo aiInfo)
		{
			int playerResearchingTechID = this._db.GetPlayerResearchingTechID(this._player.ID);
			int playerFeasibilityStudyTechId = this._db.GetPlayerFeasibilityStudyTechId(this._player.ID);
			if (playerResearchingTechID == 0 && playerFeasibilityStudyTechId == 0)
			{
				List<PlayerTechInfo> list = new List<PlayerTechInfo>();
				if (this._player.Faction.Name == "zuul")
				{
					PlayerTechInfo playerTechInfo = this._db.GetPlayerTechInfos(this._player.ID).FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "POL_Tribute_Systems");
					if (playerTechInfo != null && playerTechInfo.State != TechStates.Researching && playerTechInfo.State != TechStates.Researched)
					{
						list.Add(playerTechInfo);
					}
				}
				Kerberos.Sots.Data.TechnologyFramework.Tech tech = this.Game.AssetDatabase.AIResearchFramework.AISelectNextTech(this, list, null);
				PlayerTechInfo playerTechInfo2 = null;
				if (tech != null)
				{
					playerTechInfo2 = this._db.GetPlayerTechInfo(this._player.ID, this._db.GetTechID(tech.Id));
				}
				if (playerTechInfo2 != null)
				{
					if (playerTechInfo2.State == TechStates.Branch)
					{
						ResearchScreenState.StartFeasibilityStudy(this._db, this._player.ID, playerTechInfo2.TechID);
						StrategicAI.TraceVerbose(string.Concat(new object[]
						{
							"AI Player ",
							this._player.ID,
							" studying feasibility ",
							playerTechInfo2.TechFileID
						}));
						return;
					}
					this._db.UpdatePlayerTechState(this._player.ID, playerTechInfo2.TechID, TechStates.Researching);
					StrategicAI.TraceVerbose(string.Concat(new object[]
					{
						"AI Player ",
						this._player.ID,
						" researching ",
						playerTechInfo2.TechFileID
					}));
					return;
				}
				else
				{
					StrategicAI.TraceVerbose("AI Player " + this._player.ID + " has no research!");
				}
			}
		}
		public List<PlayerTechInfo> GetDesiredTechs()
		{
			List<PlayerTechInfo> list = new List<PlayerTechInfo>();
			StrategicAI.DesignConfigurationInfo c = default(StrategicAI.DesignConfigurationInfo);
			StrategicAI.DesignConfigurationInfo c2 = default(StrategicAI.DesignConfigurationInfo);
			IEnumerable<CombatData> combatsForPlayer = this._game.CombatData.GetCombatsForPlayer(this._game.GameDatabase, this._player.ID, 10);
			int num = 0;
			int num2 = 0;
			if (combatsForPlayer.Count<CombatData>() > 0)
			{
				foreach (CombatData current in combatsForPlayer)
				{
					List<PlayerCombatData> list2 = current.GetPlayers().ToList<PlayerCombatData>();
					foreach (PlayerCombatData current2 in list2)
					{
						Player playerObject = this._game.GetPlayerObject(current2.PlayerID);
						if (playerObject.IsStandardPlayer)
						{
							if (playerObject.ID == this._player.ID)
							{
								using (List<ShipData>.Enumerator enumerator3 = current2.ShipData.GetEnumerator())
								{
									while (enumerator3.MoveNext())
									{
										ShipData current3 = enumerator3.Current;
										DesignInfo designInfo = this._db.GetDesignInfo(current3.designID);
										c2 += StrategicAI.GetDesignConfigurationInfo(this._game, designInfo);
										num2++;
									}
									continue;
								}
							}
							if (this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, playerObject.ID) == DiplomacyState.WAR)
							{
								foreach (ShipData current4 in current2.ShipData)
								{
									DesignInfo designInfo2 = this._db.GetDesignInfo(current4.designID);
									c += StrategicAI.GetDesignConfigurationInfo(this._game, designInfo2);
									num++;
								}
							}
						}
					}
				}
			}
			if (num == 0)
			{
				return list;
			}
			if (num2 > 0)
			{
				c2.Average(num2);
			}
			c.Average(num);
			float num3 = 0.5f;
			List<PlayerTechInfo> source = this._db.GetPlayerTechInfos(this._player.ID).ToList<PlayerTechInfo>();
			if (c.PointDefense > num3)
			{
				List<LogicalWeapon> list3 = (
					from x in this._db.AssetDatabase.Weapons
					where !x.Traits.Any((WeaponEnums.WeaponTraits y) => y == WeaponEnums.WeaponTraits.Tracking)
					select x).ToList<LogicalWeapon>();
				foreach (LogicalWeapon current5 in list3)
				{
					if (!current5.Traits.Contains(WeaponEnums.WeaponTraits.PlanetKilling))
					{
						Kerberos.Sots.Data.WeaponFramework.Tech[] requiredTechs = current5.RequiredTechs;
						Kerberos.Sots.Data.WeaponFramework.Tech req;
						for (int i = 0; i < requiredTechs.Length; i++)
						{
							req = requiredTechs[i];
							if (!list.Any((PlayerTechInfo x) => x.TechFileID == req.Name))
							{
								PlayerTechInfo playerTechInfo = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == req.Name);
								if (this.ShouldResearchTech(playerTechInfo))
								{
									list.Add(playerTechInfo);
								}
							}
						}
					}
				}
			}
			if (c.BallisticsDefense > num3)
			{
				List<LogicalWeapon> list4 = (
					from x in this._db.AssetDatabase.Weapons
					where !x.Traits.Any((WeaponEnums.WeaponTraits y) => y == WeaponEnums.WeaponTraits.Ballistic) && x.PayloadType != WeaponEnums.PayloadTypes.Missile
					select x).ToList<LogicalWeapon>();
				foreach (LogicalWeapon current6 in list4)
				{
					if (!current6.Traits.Contains(WeaponEnums.WeaponTraits.PlanetKilling))
					{
						Kerberos.Sots.Data.WeaponFramework.Tech[] requiredTechs = current6.RequiredTechs;
						Kerberos.Sots.Data.WeaponFramework.Tech req;
						for (int i = 0; i < requiredTechs.Length; i++)
						{
							req = requiredTechs[i];
							if (!list.Any((PlayerTechInfo x) => x.TechFileID == req.Name))
							{
								PlayerTechInfo playerTechInfo2 = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == req.Name);
								if (this.ShouldResearchTech(playerTechInfo2))
								{
									list.Add(playerTechInfo2);
								}
							}
						}
					}
				}
			}
			if (c.EnergyDefense > num3)
			{
				List<LogicalWeapon> list5 = (
					from x in this._db.AssetDatabase.Weapons
					where !x.Traits.Any((WeaponEnums.WeaponTraits y) => y == WeaponEnums.WeaponTraits.Energy || y == WeaponEnums.WeaponTraits.Laser)
					select x).ToList<LogicalWeapon>();
				foreach (LogicalWeapon current7 in list5)
				{
					if (!current7.Traits.Contains(WeaponEnums.WeaponTraits.PlanetKilling))
					{
						Kerberos.Sots.Data.WeaponFramework.Tech[] requiredTechs = current7.RequiredTechs;
						Kerberos.Sots.Data.WeaponFramework.Tech req;
						for (int i = 0; i < requiredTechs.Length; i++)
						{
							req = requiredTechs[i];
							if (!list.Any((PlayerTechInfo x) => x.TechFileID == req.Name))
							{
								PlayerTechInfo playerTechInfo3 = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == req.Name);
								if (this.ShouldResearchTech(playerTechInfo3))
								{
									list.Add(playerTechInfo3);
								}
							}
						}
					}
				}
			}
			if (c.MissileWeapons > num3)
			{
				List<LogicalWeapon> list6 = (
					from x in this._db.AssetDatabase.Weapons
					where x.IsPDWeapon()
					select x).ToList<LogicalWeapon>();
				foreach (LogicalWeapon current8 in list6)
				{
					if (!current8.Traits.Contains(WeaponEnums.WeaponTraits.PlanetKilling))
					{
						Kerberos.Sots.Data.WeaponFramework.Tech[] requiredTechs = current8.RequiredTechs;
						Kerberos.Sots.Data.WeaponFramework.Tech req;
						for (int i = 0; i < requiredTechs.Length; i++)
						{
							req = requiredTechs[i];
							if (!list.Any((PlayerTechInfo x) => x.TechFileID == req.Name))
							{
								PlayerTechInfo playerTechInfo4 = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == req.Name);
								if (this.ShouldResearchTech(playerTechInfo4))
								{
									list.Add(playerTechInfo4);
								}
							}
						}
					}
				}
			}
			if (c.BallisticsWeapons > num3)
			{
				List<string> list7 = new List<string>
				{
					"IND_Polysteel",
					"IND_MagnoCeramic_Latices",
					"IND_Quark_Resonators",
					"IND_Adamantine_Alloys",
					"SLD_Shield_Mk._I",
					"SLD_Shield_Mk._II",
					"SLD_Shield_Mk._III",
					"SLD_Shield_Mk._IV",
					"SLD_Deflector_Shields",
					"LD_Grav_Shields"
				};
				foreach (string techID in list7)
				{
					if (!list.Any((PlayerTechInfo x) => x.TechFileID == techID))
					{
						PlayerTechInfo playerTechInfo5 = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == techID);
						if (this.ShouldResearchTech(playerTechInfo5))
						{
							list.Add(playerTechInfo5);
						}
					}
				}
			}
			if (c.EnergyWeapons > num3)
			{
				List<string> list8 = new List<string>
				{
					"IND_Polysteel",
					"IND_MagnoCeramic_Latices",
					"IND_Quark_Resonators",
					"IND_Adamantine_Alloys",
					"SLD_Shield_Mk._I",
					"SLD_Shield_Mk._II",
					"SLD_Shield_Mk._III",
					"SLD_Shield_Mk._IV",
					"SLD_Meson_Shields",
					"IND_Reflective",
					"IND_Improved_Reflective",
					"SLD_Meson_Shields",
					"SLD_Disruptor_Shields",
					"NRG_Energy_Absorbers"
				};
				foreach (string techID in list8)
				{
					if (!list.Any((PlayerTechInfo x) => x.TechFileID == techID))
					{
						PlayerTechInfo playerTechInfo6 = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == techID);
						if (this.ShouldResearchTech(playerTechInfo6))
						{
							list.Add(playerTechInfo6);
						}
					}
				}
			}
			if (c.HeavyBeamWeapons > num3)
			{
				List<string> list9 = new List<string>
				{
					"SLD_Meson_Shields",
					"SLD_Disruptor_Shields",
					"NRG_Energy_Absorbers"
				};
				foreach (string techID in list9)
				{
					if (!list.Any((PlayerTechInfo x) => x.TechFileID == techID))
					{
						PlayerTechInfo playerTechInfo7 = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == techID);
						if (this.ShouldResearchTech(playerTechInfo7))
						{
							list.Add(playerTechInfo7);
						}
					}
				}
			}
			if (num2 > 0 && c.Maxspeed > c2.Maxspeed * 1.1f)
			{
				List<string> list10 = new List<string>
				{
					"NRG_Ionic_Thruster",
					"NRG_Small_Scale_Fusion"
				};
				foreach (string techID in list10)
				{
					if (!list.Any((PlayerTechInfo x) => x.TechFileID == techID))
					{
						PlayerTechInfo playerTechInfo8 = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == techID);
						if (this.ShouldResearchTech(playerTechInfo8))
						{
							list.Add(playerTechInfo8);
						}
					}
				}
			}
			return list;
		}
		private bool ShouldResearchTech(PlayerTechInfo tech)
		{
			return tech != null && tech.State != TechStates.Researching && tech.State != TechStates.Researched;
		}
		private int GetTechFamilyScore(TechFamilies family, AIStance stance, GameMode mode, List<PlayerTechInfo> playerTechs)
		{
			switch (family)
			{
			case TechFamilies.EnergyWeapons:
			case TechFamilies.Torpedos:
			case TechFamilies.EnergyTechnology:
			case TechFamilies.WarheadTechnology:
			case TechFamilies.BallisticWeapons:
			case TechFamilies.BioTechnology:
			case TechFamilies.IndustrialTechnology:
			case TechFamilies.C3Technology:
			case TechFamilies.XenoTechnology:
			case TechFamilies.DriveTechnology:
			case TechFamilies.PoliticalScience:
			case TechFamilies.Psionics:
			case TechFamilies.Engineering:
				if (mode == GameMode.LeviathanLimit)
				{
					PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "");
					if (playerTechInfo != null && playerTechInfo.State != TechStates.Researched && playerTechInfo.State != TechStates.Researching && this._db.GetTurnCount() > 75)
					{
						return 50;
					}
				}
				break;
			}
			return 1;
		}
		private int PickAFight()
		{
			int num = 0;
			float num2 = 0f;
			this.AssessOwnStrength();
			foreach (int current in this._db.GetPlayerIDs())
			{
				if (current != this._player.ID && this._db.IsPlayerAdjacent(this._game, this._player.ID, current))
				{
					DiplomacyState diplomacyStateBetweenPlayers = this._db.GetDiplomacyStateBetweenPlayers(this._player.ID, current);
					if (diplomacyStateBetweenPlayers != DiplomacyState.ALLIED && diplomacyStateBetweenPlayers != DiplomacyState.WAR && diplomacyStateBetweenPlayers != DiplomacyState.CEASE_FIRE && diplomacyStateBetweenPlayers != DiplomacyState.NON_AGGRESSION && diplomacyStateBetweenPlayers != DiplomacyState.PEACE)
					{
						Player playerObject = this._game.GetPlayerObject(current);
						if (playerObject == null || GameSession.SimAITurns != 0 || !playerObject.IsAI())
						{
							bool flag = false;
							foreach (int current2 in this._db.GetPlayerIDs())
							{
								if (current != current2 && this._player.ID != current2 && this._db.GetDiplomacyStateBetweenPlayers(current, current2) == DiplomacyState.ALLIED)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								float num3 = 200f;
								float num4 = ((float)DiplomacyInfo.MaxDeplomacyRelations + num3) / 600f * 100f;
								float num5 = this.AssessPlayerStrength(current);
								if (num == 0 || num5 < num2)
								{
									DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(this._player.ID, current);
									float num6 = Math.Max(1f - Math.Max((float)diplomacyInfo.Relations - num3, 0f) / num4, 0f);
									if (this.Random.CoinToss((double)num6))
									{
										num = current;
										num2 = num5;
									}
								}
							}
						}
					}
				}
			}
			return num;
		}
		private int GetNearestUnexploredSystem(FleetInfo fleetInfo)
		{
			if (fleetInfo == null)
			{
				return 0;
			}
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fleetInfo.SupportingSystemID);
			if (starSystemInfo == null)
			{
				return 0;
			}
			Faction faction = this.Game.AssetDatabase.GetFaction(this.Game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			Vector3 arg_4F_0 = starSystemInfo.Origin;
			int result = 0;
			int num = 2147483647;
			foreach (StarSystemInfo current in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (this._db.GetLastTurnExploredByPlayer(this._player.ID, current.ID) == 0 && current.ID != starSystemInfo.ID)
				{
					bool flag = false;
					foreach (MissionInfo current2 in this._db.GetMissionInfos())
					{
						if (current2.TargetSystemID == current.ID && current2.Type == MissionType.SURVEY && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					IEnumerable<FleetInfo> fleetInfoBySystemID = this._db.GetFleetInfoBySystemID(current.ID, FleetType.FL_NORMAL);
					foreach (FleetInfo current3 in fleetInfoBySystemID)
					{
						DiplomacyInfo diplomacyInfo = this._db.GetDiplomacyInfo(this._player.ID, current3.PlayerID);
						if (diplomacyInfo.State == DiplomacyState.WAR)
						{
							flag = true;
							break;
						}
					}
					IEnumerable<ColonyInfo> colonyInfosForSystem = this._db.GetColonyInfosForSystem(current.ID);
					foreach (ColonyInfo current4 in colonyInfosForSystem)
					{
						DiplomacyInfo diplomacyInfo2 = this._db.GetDiplomacyInfo(this._player.ID, current4.PlayerID);
						if (diplomacyInfo2.State == DiplomacyState.WAR)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
                        int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(this.Game, fleetInfo, current.ID, faction.CanUseNodeLine(new bool?(true)), null, null);
						if (travelTime.HasValue && travelTime.Value < num)
						{
							result = current.ID;
							num = travelTime.Value;
						}
					}
				}
			}
			return result;
		}
		private int FindSystemForTransferMission(int fleetID)
		{
			int num = 0;
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			List<int> list = new List<int>();
			int[] array = this._db.GetPlayerColonySystemIDs(this._player.ID).ToArray<int>();
			int[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				int num2 = array2[i];
				if (num2 != fleetInfo.SupportingSystemID)
				{
					int num3 = 0;
					foreach (FleetInfo current in this._db.GetFleetsBySupportingSystem(num2, FleetType.FL_NORMAL))
					{
						if (!current.IsReserveFleet)
						{
							num3++;
						}
					}
					if (num3 == 0)
					{
						list.Add(num2);
					}
				}
			}
			if (list.Count<int>() > 0)
			{
				float num4 = 0f;
				foreach (int current2 in list)
				{
					StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(current2);
					float num5 = this._db.ScoreSystemAsPeripheral(starSystemInfo.ID, this._player.ID);
					if (num == 0 || num5 < num4)
					{
						num = current2;
						num4 = num5;
					}
				}
				return num;
			}
			float num6 = 0f;
			int[] array3 = array;
			for (int j = 0; j < array3.Length; j++)
			{
				int num7 = array3[j];
				StarSystemInfo starSystemInfo2 = this._db.GetStarSystemInfo(num7);
				float num8 = this._db.ScoreSystemAsPeripheral(starSystemInfo2.ID, this._player.ID);
				if (num == 0 || num8 < num6)
				{
					num = num7;
					num6 = num8;
				}
			}
			return num;
		}
		private int GetBestUpgradeTargetSystem(AIInfo aiInfo, int fleetID, out StationInfo station)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			if (fleetInfo.SystemID == 0)
			{
				station = null;
				return 0;
			}
			Vector3 fleetpos = this._db.GetStarSystemInfo(fleetInfo.SystemID).Origin;
			List<MissionInfo> stationMissions = (
				from x in this._db.GetMissionInfos()
				where x.Type == MissionType.CONSTRUCT_STN || x.Type == MissionType.UPGRADE_STN
				select x).ToList<MissionInfo>();
			IEnumerable<IGrouping<int, StationInfo>> enumerable = (
				from x in this._db.GetStationInfosByPlayerID(this._player.ID)
				where this._game.StationIsUpgradable(x) && !stationMissions.Any((MissionInfo y) => y.TargetOrbitalObjectID == x.OrbitalObjectID)
				select x into z
				orderby (float)z.DesignInfo.StationLevel * (this._db.GetStarSystemInfo(z.OrbitalObjectInfo.StarSystemID).Origin - fleetpos).Length
				select z).GroupBy(delegate(StationInfo y)
			{
				if (y.DesignInfo.StationLevel <= 3)
				{
					return 0;
				}
				return 1;
			});
			foreach (IGrouping<int, StationInfo> current in enumerable)
			{
				foreach (StationInfo current2 in current)
				{
					if (current.Key == 0)
					{
						station = current2;
						int starSystemID = current2.OrbitalObjectInfo.StarSystemID;
						return starSystemID;
					}
					if (this._db.GetTurnCount() >= 100)
					{
						station = current2;
						int starSystemID = current2.OrbitalObjectInfo.StarSystemID;
						return starSystemID;
					}
				}
			}
			station = null;
			return 0;
		}
		private int GetBestConstructionTargetSystem(AIInfo aiInfo, int fleetID, out StationType stationType)
		{
			int[] array = new int[8];
			foreach (StationInfo current in this._db.GetStationInfosByPlayerID(this._player.ID))
			{
				array[(int)current.DesignInfo.StationType]++;
			}
			PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
			string factionName = this._db.GetFactionName(playerInfo.FactionID);
			float[] array2 = new float[8];
			if (factionName.Contains("hiver"))
			{
				array2[5] = 1f;
			}
			else
			{
				array2[5] = 0f;
			}
			switch (aiInfo.Stance)
			{
			case AIStance.EXPANDING:
				array2[1] = 0.4f;
				array2[3] = 0.2f;
				array2[2] = 0.15f;
				array2[4] = 0f;
				break;
			case AIStance.ARMING:
				array2[1] = 1f;
				array2[3] = 0.3f;
				array2[2] = 0.3f;
				array2[4] = 0.15f;
				break;
			case AIStance.HUNKERING:
				array2[1] = 1f;
				array2[3] = 0.6f;
				array2[2] = 0.6f;
				array2[4] = 0.2f;
				break;
			case AIStance.CONQUERING:
				array2[1] = 1f;
				array2[3] = 0.3f;
				array2[2] = 0.3f;
				array2[4] = 0.15f;
				break;
			case AIStance.DESTROYING:
				array2[1] = 1f;
				array2[3] = 0.2f;
				array2[2] = 0.2f;
				array2[4] = 0f;
				break;
			case AIStance.DEFENDING:
				array2[1] = 1f;
				array2[3] = 0.1f;
				array2[2] = 0.3f;
				array2[4] = 0.1f;
				break;
			}
			List<int> list = new List<int>();
			foreach (ColonyInfo current2 in this._db.GetColonyInfos())
			{
				if (current2.PlayerID == this._player.ID)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(current2.OrbitalObjectID);
					if (!list.Contains(orbitalObjectInfo.StarSystemID))
					{
						list.Add(orbitalObjectInfo.StarSystemID);
					}
				}
			}
			int num = list.Count<int>();
			float[] array3 = new float[8];
			for (int i = 0; i < 8; i++)
			{
				array3[i] = (float)array[i] / (float)num;
			}
			float[] array4 = new float[8];
			for (int j = 0; j < 8; j++)
			{
				if (array2[j] > 0f)
				{
					array4[j] = (array2[j] - array3[j]) / array2[j];
				}
				else
				{
					array4[j] = 0f;
				}
				array4[j] = Math.Min(array4[j], 1f);
			}
			float num2 = 0f;
			StationType stationType2 = StationType.INVALID_TYPE;
			for (int k = 0; k < 8; k++)
			{
				if (stationType2 == StationType.INVALID_TYPE || num2 < array4[k])
				{
					stationType2 = (StationType)k;
					num2 = array4[k];
				}
			}
			if (stationType2 == StationType.INVALID_TYPE)
			{
				stationType = stationType2;
				return 0;
			}
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			int systemID = fleetInfo.SystemID;
			int num3 = 0;
			switch (stationType2)
			{
			case StationType.NAVAL:
				num3 = this.GetSystemForNavalStation(systemID);
				break;
			case StationType.SCIENCE:
				num3 = this.GetSystemForScienceStation(systemID);
				break;
			case StationType.CIVILIAN:
				num3 = this.GetSystemForCivilianStation(systemID);
				break;
			case StationType.DIPLOMATIC:
				num3 = this.GetSystemForDiplomaticStation(systemID);
				break;
			case StationType.MINING:
				num3 = this.GetSystemForMiningStation(systemID);
				break;
			}
			if (num3 == 0 && factionName.Contains("hiver") && stationType2 != StationType.GATE)
			{
				num3 = this.GetSystemForHiverGate(systemID);
				stationType2 = StationType.GATE;
			}
			if (num3 == 0 && stationType2 != StationType.NAVAL)
			{
				num3 = this.GetSystemForNavalStation(systemID);
				stationType2 = StationType.NAVAL;
			}
			stationType = stationType2;
			return num3;
		}
		private StarSystemInfo TryRelocateMissionCloserToTargetSystem_FindBestSystem(FleetInfo fleetInfo, int targetSystemID)
		{
			IEnumerable<StarSystemInfo> enumerable = 
				from x in this._db.GetStarSystemInfos()
                where fleetInfo.SupportingSystemID == x.ID || (Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(this._game, x.ID, fleetInfo.ID) && Kerberos.Sots.StarFleet.StarFleet.CanSystemSupportFleet(this._game, x.ID, fleetInfo.ID))
				select x;
			StarSystemInfo result = null;
			float num = 3.40282347E+38f;
			foreach (StarSystemInfo current in enumerable)
			{
				int num2;
				float num3;
                Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this._game, fleetInfo.ID, current.ID, targetSystemID, out num2, out num3, false, null, null);
				if (num3 < num)
				{
					num = num3;
					result = current;
				}
			}
			return result;
		}
		private bool TryRelocateMissionCloserToTargetSystem(int fleetID, int targetSystemID)
		{
			if (fleetID < 0 || targetSystemID < 0)
			{
				return false;
			}
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			StarSystemInfo starSystemInfo = this.TryRelocateMissionCloserToTargetSystem_FindBestSystem(fleetInfo, targetSystemID);
			if (starSystemInfo != null)
			{
				if (fleetInfo.SupportingSystemID != starSystemInfo.ID)
				{
                    Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._game, fleetID, starSystemInfo.ID, false, null);
				}
				return true;
			}
			return false;
		}
		private bool CreateConstructionMission(AIInfo aiInfo, int fleetID, bool isUpgradeMission)
		{
			StationType stationType = StationType.CIVILIAN;
			StationInfo stationInfo = null;
			int num = 0;
			if (isUpgradeMission)
			{
				num = this.GetBestUpgradeTargetSystem(aiInfo, fleetID, out stationInfo);
			}
			else
			{
				num = this.GetBestConstructionTargetSystem(aiInfo, fleetID, out stationType);
			}
			if (num > 0)
			{
				if (stationType != StationType.MINING)
				{
					IEnumerable<StationInfo> stationForSystem = this._db.GetStationForSystem(num);
					foreach (StationInfo current in stationForSystem)
					{
						if (current.DesignInfo.StationType == stationType)
						{
							return false;
						}
					}
				}
                if (Kerberos.Sots.StarFleet.StarFleet.CanDoConstructionMissionToTarget(this._game, num, fleetID, false))
				{
					if (isUpgradeMission)
					{
						if (stationInfo != null)
						{
                            Kerberos.Sots.StarFleet.StarFleet.SetUpgradeStationMission(this._game, fleetID, num, false, stationInfo.ID, null, stationInfo.DesignInfo.StationType, null);
							return true;
						}
					}
					else
					{
						int? suitablePlanetForStation = Kerberos.Sots.GameStates.StarSystem.GetSuitablePlanetForStation(this._game, this._player.ID, num, stationType);
						if (suitablePlanetForStation.HasValue)
						{
                            Kerberos.Sots.StarFleet.StarFleet.SetConstructionMission(this._game, fleetID, num, false, suitablePlanetForStation.Value, null, stationType, null);
							return true;
						}
					}
				}
				this.TryRelocateMissionCloserToTargetSystem(fleetID, num);
			}
			return false;
		}
		private int GetBestPlanetToColonize(double maxSupportCost)
		{
			int result = 0;
			double num = 1.7976931348623157E+308;
			foreach (StarSystemInfo current in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				if (!this._game.isHostilesAtSystem(this._player.ID, current.ID))
				{
					int lastTurnExploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._player.ID, current.ID);
					if (lastTurnExploredByPlayer > 0)
					{
						PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(current.ID);
						for (int i = 0; i < starSystemPlanetInfos.Length; i++)
						{
							PlanetInfo planetInfo = starSystemPlanetInfos[i];
							if (this._game.GameDatabase.CanColonizePlanet(this._player.ID, planetInfo.ID, this._game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this._player.ID)))
							{
								bool flag = false;
								foreach (MissionInfo current2 in this._db.GetMissionInfos())
								{
									if (current2.TargetOrbitalObjectID == planetInfo.ID && current2.Type == MissionType.COLONIZATION && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
									{
										flag = true;
										break;
									}
								}
								if (!flag && this._db.GetColonyInfoForPlanet(planetInfo.ID) == null)
								{
									double colonySupportCost = Colony.GetColonySupportCost(this._db, this._player.ID, planetInfo.ID);
									if (colonySupportCost <= maxSupportCost && colonySupportCost < num)
									{
										int arg_1A6_0 = current.ID;
										result = planetInfo.ID;
										num = colonySupportCost;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}
		private int GetTargetForStrikeMission(int fleetID, int targetPlayerID)
		{
			int result = 0;
			float num = 0f;
			this._db.GetFleetInfo(fleetID);
			int fleetSupportingSystem = this._db.GetFleetSupportingSystem(fleetID);
			List<AIColonyIntel> list = this._db.GetColonyIntelOfTargetPlayer(this._player.ID, targetPlayerID).ToList<AIColonyIntel>();
			foreach (AIColonyIntel current in list)
			{
				if (current.ColonyID.HasValue)
				{
					ColonyInfo colonyInfo = this._db.GetColonyInfo(current.ColonyID.Value);
					if (colonyInfo != null)
					{
						this._db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
						AIPlanetIntel planetIntel = this._db.GetPlanetIntel(targetPlayerID, colonyInfo.OrbitalObjectID);
                        MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.STRIKE, StationType.INVALID_TYPE, fleetID, fleetSupportingSystem, colonyInfo.OrbitalObjectID, null, 1, false, null, null);
						int turnsToTarget = missionEstimate.TurnsToTarget;
						float num2 = 1000f;
						if (turnsToTarget > 0)
						{
							num2 /= (float)turnsToTarget;
						}
						num2 += (float)planetIntel.Resources / 200f;
						if (num2 > num)
						{
							num = num2;
							result = colonyInfo.OrbitalObjectID;
						}
					}
				}
			}
			List<AIStationIntel> list2 = this._db.GetStationIntelsOfTargetPlayer(this._player.ID, targetPlayerID).ToList<AIStationIntel>();
			foreach (AIStationIntel current2 in list2)
			{
				StationInfo stationInfo = this._db.GetStationInfo(current2.StationID);
                MissionEstimate missionEstimate2 = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.STRIKE, StationType.INVALID_TYPE, fleetID, fleetSupportingSystem, stationInfo.OrbitalObjectID, null, 1, false, null, null);
				int turnsToTarget2 = missionEstimate2.TurnsToTarget;
				float num3 = 1000f;
				if (turnsToTarget2 > 0)
				{
					num3 /= (float)turnsToTarget2;
				}
				num3 += 100f;
				num3 += (float)current2.Level * 10f;
				if (num3 > num)
				{
					num = num3;
					result = stationInfo.OrbitalObjectID;
				}
			}
			return result;
		}
		internal IEnumerable<TargetOrbitalObjectScore> GetTargetsForInvasion(int fleetID, int targetPlayerID)
		{
			FleetInfo fleetInfo = this._db.GetFleetInfo(fleetID);
			if (fleetInfo != null)
			{
				int fleetSupportingSystem = this._db.GetFleetSupportingSystem(fleetID);
				float num = 200f;
				float playerSuitability = this._db.GetPlayerSuitability(this._player.ID);
				foreach (AIColonyIntel current in this._db.GetColonyIntelOfTargetPlayer(this._player.ID, targetPlayerID))
				{
					if (current.ColonyID.HasValue)
					{
						ColonyInfo colonyInfo = this._db.GetColonyInfo(current.ColonyID.Value);
						if (colonyInfo != null)
						{
							OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
							this._db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
							AIPlanetIntel planetIntel = this._db.GetPlanetIntel(targetPlayerID, colonyInfo.OrbitalObjectID);
                            MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this._game, MissionType.STRIKE, StationType.INVALID_TYPE, fleetID, fleetSupportingSystem, colonyInfo.OrbitalObjectID, null, 1, false, null, null);
							int turnsToTarget = missionEstimate.TurnsToTarget;
							float num2 = 1000f;
							if (turnsToTarget > 0)
							{
								num2 /= (float)turnsToTarget;
							}
							PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(orbitalObjectInfo.StarSystemID);
							for (int i = 0; i < starSystemPlanetInfos.Length; i++)
							{
								PlanetInfo planetInfo = starSystemPlanetInfos[i];
								ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(planetInfo.ID);
								if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
								{
									num2 += 200f;
								}
							}
							if (this._db.GetNavalStationForSystemAndPlayer(orbitalObjectInfo.StarSystemID, this._player.ID) != null)
							{
								num2 += 250f;
							}
							num2 += (float)planetIntel.Resources / 200f;
							int[] recentCombatTurns = this._db.GetRecentCombatTurns(orbitalObjectInfo.StarSystemID, this._db.GetTurnCount() - 10);
							num2 += (float)recentCombatTurns.Length * 100f;
							if (Math.Abs(playerSuitability - planetIntel.Suitability) > num)
							{
								num2 /= 10f;
							}
							yield return new TargetOrbitalObjectScore
							{
								OrbitalObjectID = colonyInfo.OrbitalObjectID,
								Score = num2
							};
						}
					}
				}
			}
			yield break;
		}
		private int? GetTargetForInvasion(int fleetID, int targetPlayerID)
		{
			int? result = null;
			float num = 0f;
			foreach (TargetOrbitalObjectScore current in this.GetTargetsForInvasion(fleetID, targetPlayerID))
			{
				if (current.Score > num)
				{
					num = current.Score;
					result = new int?(current.OrbitalObjectID);
				}
			}
			return result;
		}
		private int GetSystemForHiverGate(int fromSystemID)
		{
			int num = 0;
			float num2 = 0f;
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fromSystemID);
			Vector3 origin = starSystemInfo.Origin;
			foreach (ColonyInfo current in this._db.GetColonyInfos())
			{
				if (current.PlayerID == this._player.ID)
				{
					OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(current.OrbitalObjectID);
					StarSystemInfo starSystemInfo2 = this._db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
					bool flag = false;
					foreach (MissionInfo current2 in this._db.GetMissionInfos())
					{
						if (current2.TargetSystemID == starSystemInfo2.ID && current2.Type == MissionType.GATE && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag && this._db.GetHiverGateForSystem(orbitalObjectInfo.StarSystemID, this._player.ID) == null)
					{
						float length = (starSystemInfo2.Origin - origin).Length;
						float num3 = 1000f - length;
						if (num == 0 || num3 > num2)
						{
							num = starSystemInfo2.ID;
							num2 = num3;
						}
					}
				}
			}
			return num;
		}
		private int GetSystemForNavalStation(int fromSystemID)
		{
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fromSystemID);
			Vector3 origin = starSystemInfo.Origin;
			int num = 0;
			float num2 = -3.40282347E+38f;
			foreach (StarSystemInfo current in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				int lastTurnExploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._player.ID, current.ID);
				if (lastTurnExploredByPlayer > 0 && this._db.GetNavalStationForSystemAndPlayer(current.ID, this._player.ID) == null)
				{
					List<StationType> systemCanSupportStations = Kerberos.Sots.GameStates.StarSystem.GetSystemCanSupportStations(this._game, current.ID, this._player.ID);
					if (systemCanSupportStations.Contains(StationType.NAVAL) && Kerberos.Sots.GameStates.StarSystem.GetSuitablePlanetForStation(this._game, this._player.ID, current.ID, StationType.NAVAL).HasValue)
					{
						bool flag = false;
						foreach (MissionInfo current2 in this._db.GetMissionInfos())
						{
							if (current2.TargetSystemID == current.ID && GameSession.GetConstructionMissionStationType(this._db, current2) == StationType.NAVAL && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							float num3 = 0f;
							PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(current.ID);
							for (int i = 0; i < starSystemPlanetInfos.Length; i++)
							{
								PlanetInfo planetInfo = starSystemPlanetInfos[i];
								ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(planetInfo.ID);
								if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
								{
									num3 += 100f;
								}
							}
							if (num3 >= 0f)
							{
								float length = (current.Origin - origin).Length;
								float num4 = 5f - Math.Abs(length - 2f * this._db.FindCurrentDriveSpeedForPlayer(this._player.ID));
								num3 *= num4;
								if (num == 0 || num3 > num2)
								{
									num = current.ID;
									num2 = num3;
								}
							}
						}
					}
				}
			}
			return num;
		}
		private int GetSystemForCivilianStation(int fromSystemID)
		{
			int num = 0;
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fromSystemID);
			Vector3 origin = starSystemInfo.Origin;
			double num2 = 0.0;
			foreach (StarSystemInfo current in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				float length = (origin - current.Origin).Length;
				float num3 = this._db.FindCurrentDriveSpeedForPlayer(this._player.ID);
				float num4 = length / num3;
				PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
				string factionName = this._db.GetFactionName(playerInfo.FactionID);
				if (factionName.Contains("hiver") && this._db.GetHiverGateForSystem(current.ID, this._player.ID) != null)
				{
					num4 = 1f;
				}
				int lastTurnExploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._player.ID, current.ID);
				if (lastTurnExploredByPlayer > 0 && this._db.GetCivilianStationForSystemAndPlayer(current.ID, this._player.ID) == null)
				{
					bool flag = false;
					foreach (MissionInfo current2 in this._db.GetMissionInfos())
					{
						if (current2.TargetSystemID == current.ID && GameSession.GetConstructionMissionStationType(this._db, current2) == StationType.CIVILIAN && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						double num5 = 0.0;
						PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(current.ID);
						for (int i = 0; i < starSystemPlanetInfos.Length; i++)
						{
							PlanetInfo planetInfo = starSystemPlanetInfos[i];
							ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(planetInfo.ID);
							if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
							{
								num5 += Colony.GetIndustrialOutput(this._game, colonyInfoForPlanet, planetInfo);
							}
						}
						if (num4 > 2f)
						{
							num5 /= (double)(num4 - 1f);
						}
						if (num5 > 0.0 && (num == 0 || num5 > num2))
						{
							num = current.ID;
							num2 = num5;
						}
					}
				}
			}
			return num;
		}
		private int GetSystemForDiplomaticStation(int fromSystemID)
		{
			int num = 0;
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fromSystemID);
			Vector3 origin = starSystemInfo.Origin;
			double num2 = 0.0;
			foreach (StarSystemInfo current in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				float length = (origin - current.Origin).Length;
				float num3 = this._db.FindCurrentDriveSpeedForPlayer(this._player.ID);
				float num4 = length / num3;
				PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
				string factionName = this._db.GetFactionName(playerInfo.FactionID);
				if (factionName.Contains("hiver") && this._db.GetHiverGateForSystem(current.ID, this._player.ID) != null)
				{
					num4 = 1f;
				}
				int lastTurnExploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._player.ID, current.ID);
				if (lastTurnExploredByPlayer > 0 && this._db.GetDiplomaticStationForSystemAndPlayer(current.ID, this._player.ID) == null)
				{
					bool flag = false;
					foreach (MissionInfo current2 in this._db.GetMissionInfos())
					{
						if (current2.TargetSystemID == current.ID && GameSession.GetConstructionMissionStationType(this._db, current2) == StationType.DIPLOMATIC && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						float num5 = 0f;
						bool flag2 = false;
						PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(current.ID);
						for (int i = 0; i < starSystemPlanetInfos.Length; i++)
						{
							PlanetInfo planetInfo = starSystemPlanetInfos[i];
							ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(planetInfo.ID);
							if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							float range = Math.Max(num3 * 4f, 20f);
							foreach (StarSystemInfo current3 in this._db.GetSystemsInRange(current.Origin, range))
							{
								PlanetInfo[] starSystemPlanetInfos2 = this._db.GetStarSystemPlanetInfos(current3.ID);
								for (int j = 0; j < starSystemPlanetInfos2.Length; j++)
								{
									PlanetInfo planetInfo2 = starSystemPlanetInfos2[j];
									AIColonyIntel colonyIntelForPlanet = this._db.GetColonyIntelForPlanet(this._player.ID, planetInfo2.ID);
									if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != this._player.ID)
									{
										num5 += 100f;
									}
								}
								if (this._db.GetDiplomaticStationForSystemAndPlayer(current3.ID, this._player.ID) != null)
								{
									num5 -= 150f;
								}
							}
						}
						if (num4 > 2f)
						{
							num5 /= num4 - 1f;
						}
						if (num5 > 0f && (num == 0 || (double)num5 > num2))
						{
							num = current.ID;
							num2 = (double)num5;
						}
					}
				}
			}
			return num;
		}
		private int GetSystemForScienceOrMiningStation(int fromSystemID)
		{
			int num = 0;
			StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(fromSystemID);
			Vector3 origin = starSystemInfo.Origin;
			double num2 = 0.0;
			foreach (StarSystemInfo current in this._db.GetVisibleStarSystemInfos(this._player.ID))
			{
				float length = (origin - current.Origin).Length;
				float num3 = this._db.FindCurrentDriveSpeedForPlayer(this._player.ID);
				float num4 = length / num3;
				PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
				string factionName = this._db.GetFactionName(playerInfo.FactionID);
				if (factionName.Contains("hiver") && this._db.GetHiverGateForSystem(current.ID, this._player.ID) != null)
				{
					num4 = 1f;
				}
				int lastTurnExploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._player.ID, current.ID);
				if (lastTurnExploredByPlayer > 0 && this._db.GetScienceStationForSystemAndPlayer(current.ID, this._player.ID) == null)
				{
					bool flag = false;
					foreach (MissionInfo current2 in this._db.GetMissionInfos())
					{
						if (current2.TargetSystemID == current.ID && GameSession.GetConstructionMissionStationType(this._db, current2) == StationType.SCIENCE && this._db.GetFleetInfo(current2.FleetID).PlayerID == this._player.ID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						double num5 = 0.0;
						PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(current.ID);
						for (int i = 0; i < starSystemPlanetInfos.Length; i++)
						{
							PlanetInfo planetInfo = starSystemPlanetInfos[i];
							ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(planetInfo.ID);
							if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._player.ID)
							{
								num5 += Colony.GetIndustrialOutput(this._game, colonyInfoForPlanet, planetInfo);
							}
						}
						if (num4 > 2f)
						{
							num5 /= (double)(num4 - 1f);
						}
						if (num5 > 0.0 && (num == 0 || num5 > num2))
						{
							num = current.ID;
							num2 = num5;
						}
					}
				}
			}
			return num;
		}
		private int GetSystemForScienceStation(int fromSystemID)
		{
			return this.GetSystemForScienceOrMiningStation(fromSystemID);
		}
		private int GetSystemForMiningStation(int fromSystemID)
		{
			if (!Player.CanBuildMiningStations(this._db, this._player.ID))
			{
				return 0;
			}
			return 0;
		}
		private void EnsureOpenDefenseInvoice(int systemId, ref bool isOpened, ref int invoiceId, ref int instanceId)
		{
			if (!isOpened)
			{
				this.OpenInvoice(systemId, "Defenses", out invoiceId, out instanceId);
				isOpened = true;
			}
		}
		private void ManageDefenses(AIInfo aiInfo)
		{
			double num = this.GetAvailableDefenseBudget();
			IEnumerable<StarSystemInfo> enumerable = 
				from x in this._db.GetStarSystemInfos()
				where this._db.GetColonyInfosForSystem(x.ID).Any((ColonyInfo y) => y.PlayerID == this._player.ID)
				select x;
			foreach (StarSystemInfo current in enumerable)
			{
				int systemDefensePoints = this._db.GetSystemDefensePoints(current.ID, this._player.ID);
				int allocatedSystemDefensePoints = this._db.GetAllocatedSystemDefensePoints(current, this._player.ID);
				int num2 = systemDefensePoints - allocatedSystemDefensePoints;
				if (num2 > 0)
				{
					FleetInfo defenseFleetInfo = this._db.GetDefenseFleetInfo(current.ID, this._player.ID);
					if (defenseFleetInfo != null)
					{
						FleetInfo fleetInfo = this._db.InsertOrGetReserveFleetInfo(current.ID, this._player.ID);
						foreach (ShipInfo current2 in (
							from x in this._db.GetShipInfoByFleetID(fleetInfo.ID, false)
							where x.IsPoliceShip()
							select x).ToList<ShipInfo>())
						{
							this._db.TransferShip(current2.ID, defenseFleetInfo.ID);
						}
						int num3 = num2;
						foreach (ShipInfo current3 in (
							from x in this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false)
							where !x.IsPlaced()
							select x).ToList<ShipInfo>())
						{
							int defenseAssetCPCost = this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(current3.DesignInfo);
                            if (defenseAssetCPCost <= num3 && Kerberos.Sots.StarFleet.StarFleet.AutoPlaceDefenseAsset(this._game.App, current3.ID, current.ID))
							{
								num3 -= defenseAssetCPCost;
							}
						}
						systemDefensePoints = this._db.GetSystemDefensePoints(current.ID, this._player.ID);
						allocatedSystemDefensePoints = this._db.GetAllocatedSystemDefensePoints(current, this._player.ID);
						num2 = systemDefensePoints - allocatedSystemDefensePoints;
						IEnumerable<DesignInfo> source = 
							from x in this._db.GetBuildOrdersForSystem(current.ID)
							select this._db.GetDesignInfo(x.DesignID) into y
							where y.PlayerID == this._player.ID
							select y;
						int num4 = (
							from x in this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false)
							where x.IsPlaced() && x.IsPlatform()
							select x).Sum((ShipInfo y) => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo));
						int num5 = (
							from x in this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false)
							where !x.IsPlaced() && x.IsPlatform()
							select x).Sum((ShipInfo y) => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo));
						int num6 = (
							from x in source
							where x.IsPlatform()
							select x).Sum((DesignInfo y) => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y));
						int num7 = (this._db.GetMostRecentCombatData(current.ID) != null) ? 2147483647 : 1;
						int num8 = (
							from x in this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false)
							where x.IsPlaced() && x.IsPoliceShip()
							select x).Sum((ShipInfo y) => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo));
						int num9 = (
							from x in this._db.GetShipInfoByFleetID(defenseFleetInfo.ID, false).Concat(this._db.GetShipInfoByFleetID(fleetInfo.ID, false))
							where !x.IsPlaced() && x.IsPoliceShip()
							select x).Sum((ShipInfo y) => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo));
						int num10 = (
							from x in source
							where x.IsPoliceShip()
							select x).Sum((DesignInfo y) => this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(y));
						int num11 = 1;
						if (num5 <= 0 && num9 <= 0)
						{
							int num12 = num2 - num5 - num6 - num9 - num10;
							float num13 = 0f;
							double num14 = 0.0;
							double num15 = 0.0;
							BuildScreenState.ObtainConstructionCosts(out num13, out num14, out num15, this._game.App, current.ID, this._player.ID);
							if (num14 != 0.0 && num13 > 0f && num > 0.0 && num12 > 0)
							{
								bool flag = false;
								int invoiceId = 0;
								int instanceId = 0;
								int num16 = Math.Max(3 - (num6 + num10), 0);
								int num17 = Math.Min(num11 - num9 - num10 - num8, num16);
								if (num17 > 0 && num17 < num12)
								{
									List<DesignInfo> list = (
										from x in this._db.GetVisibleDesignInfosForPlayer(this._player.ID)
										where x.IsPoliceShip()
										select x).ToList<DesignInfo>();
									if (list.Count > 0)
									{
										DesignInfo designInfo = this._game.Random.Choose(list);
										double num18 = (double)BuildScreenState.GetDesignCost(this._game.App, designInfo, 0);
										int defenseAssetCPCost2 = this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(designInfo);
										while (defenseAssetCPCost2 <= num12 && num >= num18)
										{
											this.EnsureOpenDefenseInvoice(current.ID, ref flag, ref invoiceId, ref instanceId);
											this.AddShipToInvoice(current.ID, designInfo, invoiceId, instanceId, null);
											num17 -= defenseAssetCPCost2;
											num12 -= defenseAssetCPCost2;
											num -= num18;
											num16--;
											if (num17 <= 0 || num16 <= 0)
											{
												break;
											}
										}
									}
								}
								int num19 = Math.Min(Math.Min(num7 - num5 - num6 - num4, num12), num16);
								if (num19 > 0)
								{
									List<DesignInfo> list2 = (
										from x in this._db.GetVisibleDesignInfosForPlayer(this._player.ID)
										where x.GetRealShipClass() == RealShipClasses.Platform
										select x).ToList<DesignInfo>();
									if (list2.Count > 0)
									{
										do
										{
											DesignInfo designInfo2 = this._game.Random.Choose(list2);
											int defenseAssetCPCost3 = this._db.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(designInfo2);
											double num20 = (double)BuildScreenState.GetDesignCost(this._game.App, designInfo2, 0);
											if (defenseAssetCPCost3 > num19 || num < num20)
											{
												break;
											}
											this.EnsureOpenDefenseInvoice(current.ID, ref flag, ref invoiceId, ref instanceId);
											this.AddShipToInvoice(current.ID, designInfo2, invoiceId, instanceId, null);
											num19 -= defenseAssetCPCost3;
											num12 -= defenseAssetCPCost3;
											num -= num20;
											num16--;
										}
										while (num19 > 0 && num16 > 0);
									}
								}
							}
						}
					}
				}
			}
		}
		private void ManageReserves(AIInfo aiInfo)
		{
			List<FleetInfo> list = this._db.GetFleetInfosByPlayerID(aiInfo.PlayerID, FleetType.FL_RESERVE).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				List<ShipInfo> list2 = this._db.GetShipInfoByFleetID(current.ID, false).ToList<ShipInfo>();
				StrategicAI.BattleRiderMountSet battleRiderMountSet = new StrategicAI.BattleRiderMountSet(list2);
				foreach (ShipInfo current2 in list2)
				{
					if (!battleRiderMountSet.Contains(current2) && (!current2.AIFleetID.HasValue || current2.AIFleetID.Value == 0))
					{
						this._db.RemoveShip(current2.ID);
					}
				}
			}
		}
		private void ManageFleets(AIInfo aiInfo)
		{
			StrategicAI.TraceVerbose("Maintaining fleets...");
			List<int> mySystemIDs = this._db.GetPlayerColonySystemIDs(this._player.ID).ToList<int>();
			List<StarSystemInfo> list = (
				from x in this._db.GetStarSystemInfos()
				where mySystemIDs.Contains(x.ID)
				select x).ToList<StarSystemInfo>();
			List<AIFleetInfo> myAIFleets = this._db.GetAIFleetInfos(this._player.ID).ToList<AIFleetInfo>();
			List<FleetInfo> list2 = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			StrategicAI.TraceVerbose("Forgetting AIFleetInfos without admirals...");
			foreach (AIFleetInfo current in myAIFleets.ToList<AIFleetInfo>())
			{
				if (!current.AdmiralID.HasValue)
				{
					StrategicAI.TraceVerbose("  " + current.ToString());
					this._db.RemoveAIFleetInfo(current.ID);
					myAIFleets.Remove(current);
				}
			}
			StrategicAI.TraceVerbose("Forgetting FleetInfos without admirals...");
			foreach (FleetInfo fleet in list2.ToList<FleetInfo>())
			{
				if (fleet.AdmiralID == 0)
				{
					StrategicAI.TraceVerbose("  " + fleet.ToString());
					this._db.RemoveAIFleetInfo(fleet.ID);
					myAIFleets.RemoveAll((AIFleetInfo x) => x.FleetID == fleet.ID);
					list2.Remove(fleet);
				}
			}
			StrategicAI.TraceVerbose("Assigning fleet templates...");
			foreach (FleetInfo fleet in list2)
			{
				FleetTemplate fleetTemplate = null;
				AIFleetInfo aiFleetInfo = myAIFleets.FirstOrDefault((AIFleetInfo x) => x.FleetID == fleet.ID);
				if (aiFleetInfo == null)
				{
					string templateName = DesignLab.DeduceFleetTemplate(this._db, this._game, fleet.ID);
					if (!string.IsNullOrEmpty(templateName))
					{
						fleetTemplate = this._db.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
						AIFleetInfo aIFleetInfo = new AIFleetInfo();
						aIFleetInfo.AdmiralID = new int?(fleet.AdmiralID);
						aIFleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate.MissionTypes);
						aIFleetInfo.SystemID = fleet.SupportingSystemID;
						aIFleetInfo.FleetTemplate = templateName;
						aIFleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aIFleetInfo);
						aiFleetInfo = aIFleetInfo;
						StrategicAI.TraceVerbose(string.Format("  Assigned template {0} to clean fleet {1}", fleetTemplate, fleet));
					}
				}
				if (fleetTemplate == null)
				{
					fleetTemplate = this._db.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == aiFleetInfo.FleetTemplate);
				}
				if (fleetTemplate != null && fleet.SystemID == fleet.SupportingSystemID)
				{
					StrategicAI.TraceVerbose(string.Format("  Performing maintenance on fleet {0} (template {1}) at supporting system...", fleet, fleetTemplate));
					List<DesignInfo> list3 = new List<DesignInfo>();
					IEnumerable<ShipInfo> shipInfoByFleetID = this._db.GetShipInfoByFleetID(fleet.ID, true);
					if (aiFleetInfo.InvoiceID.HasValue)
					{
						StrategicAI.TraceVerbose(string.Format("    Waiting for completion of invoice {0}.", aiFleetInfo.InvoiceID));
					}
					else
					{
						if (aiFleetInfo.FleetID.HasValue)
						{
							FleetInfo fleetInfo = this._db.InsertOrGetReserveFleetInfo(fleet.SupportingSystemID, this._player.ID);
							List<ShipInfo> list4 = (
								from x in this._db.GetShipInfoByFleetID(fleetInfo.ID, false)
								where x.AIFleetID == aiFleetInfo.ID
								select x).ToList<ShipInfo>();
							bool flag = true;
							foreach (ShipInfo current2 in list4)
							{
								if (flag)
								{
									flag = false;
									StrategicAI.TraceVerbose("    Transferring ships to fleet:");
								}
								this._db.TransferShip(current2.ID, aiFleetInfo.FleetID.Value);
								StrategicAI.TraceVerbose(string.Format("      + Ship {0}", current2));
							}
						}
						Dictionary<ShipInfo, Dictionary<LogicalBank, StrategicAI.BankRiderInfo>> dictionary = new Dictionary<ShipInfo, Dictionary<LogicalBank, StrategicAI.BankRiderInfo>>();
						if (aiFleetInfo.FleetID.HasValue)
						{
							IEnumerable<ShipInfo> shipInfoByFleetID2 = this._db.GetShipInfoByFleetID(aiFleetInfo.FleetID.Value, false);
							StrategicAI.BattleRiderMountSet battleRiderMountSet = new StrategicAI.BattleRiderMountSet(shipInfoByFleetID2);
							foreach (ShipInfo current3 in shipInfoByFleetID2)
							{
								if (!battleRiderMountSet.Contains(current3))
								{
									List<ShipInfo> source = this._db.GetBattleRidersByParentID(current3.ID).ToList<ShipInfo>();
									Dictionary<LogicalBank, StrategicAI.BankRiderInfo> dictionary2 = new Dictionary<LogicalBank, StrategicAI.BankRiderInfo>();
									dictionary.Add(current3, dictionary2);
									foreach (KeyValuePair<LogicalMount, int> mountIndex in StrategicAI.BattleRiderMountSet.EnumerateBattleRiderMounts(current3.DesignInfo))
									{
										ShipInfo shipInfo = source.FirstOrDefault(delegate(ShipInfo x)
										{
											int arg_14_0 = x.RiderIndex;
											KeyValuePair<LogicalMount, int> mountIndex2 = mountIndex;
											return arg_14_0 == mountIndex2.Value;
										});
										Dictionary<LogicalBank, StrategicAI.BankRiderInfo> arg_633_0 = dictionary2;
										KeyValuePair<LogicalMount, int> mountIndex3 = mountIndex;
										StrategicAI.BankRiderInfo bankRiderInfo;
										if (!arg_633_0.TryGetValue(mountIndex3.Key.Bank, out bankRiderInfo))
										{
											StrategicAI.BankRiderInfo bankRiderInfo2 = new StrategicAI.BankRiderInfo();
											StrategicAI.BankRiderInfo arg_658_0 = bankRiderInfo2;
											mountIndex3 = mountIndex;
											arg_658_0.Bank = mountIndex3.Key.Bank;
											bankRiderInfo = bankRiderInfo2;
											dictionary2.Add(bankRiderInfo.Bank, bankRiderInfo);
											if (shipInfo != null)
											{
												bankRiderInfo.AllocatedRole = new ShipRole?(shipInfo.DesignInfo.Role);
											}
										}
										if (shipInfo == null)
										{
											List<int> arg_6A8_0 = bankRiderInfo.FreeRiderIndices;
											mountIndex3 = mountIndex;
											arg_6A8_0.Add(mountIndex3.Value);
										}
										else
										{
											List<int> arg_6C6_0 = bankRiderInfo.FilledRiderIndices;
											mountIndex3 = mountIndex;
											arg_6C6_0.Add(mountIndex3.Value);
										}
									}
									foreach (StrategicAI.BankRiderInfo current4 in 
										from x in dictionary2.Values
										where x.FreeRiderIndices.Count > 0
										select x)
									{
										if (!current4.AllocatedRole.HasValue)
										{
											ShipInfo shipInfo2 = battleRiderMountSet.EnumerateByTurretClass(current4.Bank.TurretClass).FirstOrDefault<ShipInfo>();
											if (shipInfo2 != null)
											{
												current4.AllocatedRole = new ShipRole?(shipInfo2.DesignInfo.Role);
											}
										}
										if (current4.AllocatedRole.HasValue)
										{
											while (current4.FreeRiderIndices.Count > 0)
											{
												ShipInfo shipInfo3 = battleRiderMountSet.FindShipByRole(current4.AllocatedRole.Value);
												if (shipInfo3 == null)
												{
													break;
												}
												battleRiderMountSet.AttachBattleRiderToShip(this._db, current4, shipInfo3, current3, current4.FreeRiderIndices[0]);
											}
										}
									}
								}
							}
						}
						List<ShipInclude> list5 = new List<ShipInclude>(fleetTemplate.ShipIncludes);
						foreach (Dictionary<LogicalBank, StrategicAI.BankRiderInfo> current5 in dictionary.Values)
						{
							foreach (StrategicAI.BankRiderInfo current6 in current5.Values)
							{
								ShipRole shipRole;
								if (current6.AllocatedRole.HasValue)
								{
									shipRole = current6.AllocatedRole.Value;
								}
								else
								{
									shipRole = this._random.Choose(StrategicAI.BattleRiderMountSet.EnumerateShipRolesByTurretClass(current6.Bank.TurretClass));
								}
								list5.Add(new ShipInclude
								{
									Amount = current6.FilledRiderIndices.Count + current6.FreeRiderIndices.Count,
									Faction = this._player.Faction.Name,
									InclusionType = ShipInclusionType.REQUIRED,
									ShipRole = shipRole,
									WeaponRole = null
								});
							}
						}
						foreach (ShipInclude include in list5)
						{
							if (include.InclusionType == ShipInclusionType.REQUIRED && (include.Faction == null || !(include.Faction != this._player.Faction.Name)) && !(include.FactionExclusion == this._player.Faction.Name))
							{
								int num = shipInfoByFleetID.Count((ShipInfo x) => x.DesignInfo.Role == include.ShipRole);
								if (num < include.Amount)
								{
									int num2 = include.Amount - num;
									for (int i = 0; i < num2; i++)
									{
										DesignInfo designByRole = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(aiInfo.Stance), include.ShipRole, include.WeaponRole);
										if (designByRole != null)
										{
											list3.Add(designByRole);
										}
									}
								}
							}
						}
						if (list3.Count<DesignInfo>() > 0)
						{
							StrategicAI.TraceVerbose("    Requesting additional ships:");
							AdmiralInfo admiralInfo = this._db.GetAdmiralInfo(fleet.AdmiralID);
							int value = this._db.InsertInvoiceInstance(this._player.ID, fleet.SupportingSystemID, admiralInfo.Name);
							foreach (DesignInfo current7 in list3)
							{
								string shipName = this._game.NamesPool.GetShipName(this._game, this._player.ID, current7.Class, null);
								this._db.InsertBuildOrder(fleet.SupportingSystemID, current7.ID, 0, 0, shipName, current7.GetPlayerProductionCost(this._db, this._player.ID, !current7.isPrototyped, null), new int?(value), new int?(aiFleetInfo.ID), 0);
								StrategicAI.TraceVerbose(string.Format("      + Design {0}", current7));
							}
							aiFleetInfo.InvoiceID = new int?(value);
							this._db.UpdateAIFleetInfo(aiFleetInfo);
						}
					}
				}
			}
			StrategicAI.TraceVerbose("Checking progress of fleet creation...");
			foreach (AIFleetInfo aiFleet in myAIFleets)
			{
				if (!aiFleet.InvoiceID.HasValue && !aiFleet.FleetID.HasValue)
				{
					FleetTemplate fleetTemplate2 = this._db.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == aiFleet.FleetTemplate);
					aiFleet.FleetID = new int?(this._db.InsertFleet(this._player.ID, aiFleet.AdmiralID.Value, aiFleet.SystemID, aiFleet.SystemID, fleetTemplate2.FleetName, FleetType.FL_NORMAL));
					this._db.UpdateAIFleetInfo(aiFleet);
					StrategicAI.TraceVerbose(string.Format("  Creating new fleet for {0}...", aiFleet));
					FleetInfo fleetInfo2 = this._db.InsertOrGetReserveFleetInfo(aiFleet.SystemID, this._player.ID);
					List<ShipInfo> list6 = (
						from x in this._db.GetShipInfoByFleetID(fleetInfo2.ID, false)
						where x.AIFleetID == aiFleet.ID
						select x).ToList<ShipInfo>();
					foreach (ShipInfo current8 in list6)
					{
						this._db.TransferShip(current8.ID, aiFleet.FleetID.Value);
						StrategicAI.TraceVerbose(string.Format("    + Ship {0}", current8));
					}
				}
			}
			StrategicAI.TraceVerbose("Issuing build orders...");
			double num3 = -1000000.0;
			PlayerInfo playerInfo = this._db.GetPlayerInfo(this._player.ID);
			if (playerInfo.Savings <= num3)
			{
				StrategicAI.TraceVerbose(string.Format("  Savings at ship building debt cutoff ({0}). New orders will not be issued this turn.", num3));
				return;
			}
			list = (
				from x in list
				where myAIFleets.FirstOrDefault((AIFleetInfo y) => y.SystemID == x.ID && y.InvoiceID.HasValue) == null
				select x).ToList<StarSystemInfo>();
			if (list.Count == 0)
			{
				StrategicAI.TraceVerbose("  All pending systems are already busy building ships.");
				return;
			}
			IEnumerable<AdmiralInfo> admiralInfosForPlayer = this._db.GetAdmiralInfosForPlayer(this._player.ID);
			List<FleetInfo> fleetInfos = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			AdmiralInfo admiralInfo2 = admiralInfosForPlayer.FirstOrDefault((AdmiralInfo x) => !fleetInfos.Any((FleetInfo y) => y.AdmiralID == x.ID) && !myAIFleets.Any((AIFleetInfo y) => y.AdmiralID == x.ID));
			StarSystemInfo starSystemInfo = list.LastOrDefault<StarSystemInfo>();
			if (admiralInfo2 != null && starSystemInfo != null)
			{
				this.BuildFleet(aiInfo, admiralInfo2, starSystemInfo, null, null, false);
				return;
			}
			StrategicAI.TraceVerbose("  Missing an available admiral or star system to build a fleet at.");
		}
		private void OpenInvoice(int systemId, string name, out int invoiceId, out int instanceId)
		{
			invoiceId = this._db.InsertInvoice(name, this._player.ID, false);
			instanceId = this._db.InsertInvoiceInstance(this._player.ID, systemId, name);
			StrategicAI.TraceVerbose(string.Format("    Opening a new build invoice instance {0}...", instanceId));
		}
		private void AddShipToInvoice(int systemId, DesignInfo selectedDesign, int invoiceId, int instanceId, int? aiFleetID)
		{
			BuildScreenState.InvoiceItem invoiceItem = new BuildScreenState.InvoiceItem();
			invoiceItem.DesignID = selectedDesign.ID;
			invoiceItem.isPrototypeOrder = false;
			invoiceItem.ShipName = this._game.NamesPool.GetShipName(this._game, this._player.ID, selectedDesign.Class, null);
			if (selectedDesign.GetRealShipClass().Value == RealShipClasses.Drone)
			{
				int num = 0;
				num++;
			}
			this._db.InsertInvoiceBuildOrder(invoiceId, invoiceItem.DesignID, invoiceItem.ShipName, 0);
			this._db.InsertBuildOrder(systemId, invoiceItem.DesignID, 0, 0, invoiceItem.ShipName, selectedDesign.GetPlayerProductionCost(this._db, this._player.ID, !selectedDesign.isPrototyped, null), new int?(instanceId), aiFleetID, 0);
			StrategicAI.TraceVerbose(string.Format("      + Design {0} (ship name '{1}')", selectedDesign, invoiceItem.ShipName));
		}
		private List<ShipInclude> TryFillWithShipFromReserve(AIFleetInfo aifleet, List<ShipInfo> freeReserveShips, List<ShipInclude> templateIncludes)
		{
			List<ShipInclude> list = new List<ShipInclude>();
			list.AddRange(templateIncludes);
			if (freeReserveShips.Count > 0)
			{
				foreach (ShipInclude inc in templateIncludes)
				{
					ShipInfo shipInfo = freeReserveShips.FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, inc.ShipRole) && x.DesignInfo.WeaponRole == inc.WeaponRole);
					if (shipInfo == null)
					{
						shipInfo = freeReserveShips.FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, inc.ShipRole));
					}
					if (shipInfo != null)
					{
						this._db.UpdateShipAIFleetID(shipInfo.ID, new int?(aifleet.ID));
						list.Remove(inc);
					}
				}
			}
			return list;
		}
		private List<BuildScreenState.InvoiceItem> GetInvoiceAfterInculdingReserve(List<ShipInfo> freeReserveShips, List<BuildScreenState.InvoiceItem> desiredItems)
		{
			List<BuildScreenState.InvoiceItem> list = new List<BuildScreenState.InvoiceItem>();
			list.AddRange(desiredItems);
			if (freeReserveShips.Count > 0)
			{
				List<ShipInfo> list2 = new List<ShipInfo>();
				list2.AddRange(freeReserveShips);
				foreach (BuildScreenState.InvoiceItem current in desiredItems)
				{
					DesignInfo desiredDesign = this._db.GetDesignInfo(current.DesignID);
					if (desiredDesign != null)
					{
						ShipInfo shipInfo = list2.FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, desiredDesign.Role) && x.DesignInfo.WeaponRole == desiredDesign.WeaponRole);
						if (shipInfo == null)
						{
							shipInfo = list2.FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, desiredDesign.Role));
						}
						if (shipInfo != null)
						{
							list2.Remove(shipInfo);
							list.Remove(current);
						}
					}
				}
			}
			return list;
		}
		private List<BuildScreenState.InvoiceItem> TryFillWithShipFromReserve(AIFleetInfo aifleet, List<ShipInfo> freeReserveShips, List<BuildScreenState.InvoiceItem> desiredItems)
		{
			List<BuildScreenState.InvoiceItem> list = new List<BuildScreenState.InvoiceItem>();
			list.AddRange(desiredItems);
			if (freeReserveShips.Count > 0)
			{
				List<ShipInfo> list2 = new List<ShipInfo>();
				list2.AddRange(freeReserveShips);
				foreach (BuildScreenState.InvoiceItem current in desiredItems)
				{
					DesignInfo desiredDesign = this._db.GetDesignInfo(current.DesignID);
					if (desiredDesign != null)
					{
						ShipInfo shipInfo = list2.FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, desiredDesign.Role) && x.DesignInfo.WeaponRole == desiredDesign.WeaponRole);
						if (shipInfo == null)
						{
							shipInfo = list2.FirstOrDefault((ShipInfo x) => StrategicAI.IsShipRoleEquivilant(x.DesignInfo.Role, desiredDesign.Role));
						}
						if (shipInfo != null)
						{
							this._db.UpdateShipAIFleetID(shipInfo.ID, new int?(aifleet.ID));
							list2.Remove(shipInfo);
							list.Remove(current);
						}
					}
				}
			}
			return list;
		}
		private int GetBuildInvoiceCost(App app, List<BuildScreenState.InvoiceItem> items)
		{
			int num = 0;
			foreach (BuildScreenState.InvoiceItem current in items)
			{
				DesignInfo designInfo = app.GameDatabase.GetDesignInfo(current.DesignID);
				num += designInfo.GetPlayerProductionCost(this._db, this._player.ID, true, null);
			}
			return num;
		}
		private List<ShipInfo> GetUnclaimedShipsInReserve(int systemId)
		{
			int? reserveFleetID = this._db.GetReserveFleetID(this._player.ID, systemId);
			if (reserveFleetID.HasValue)
			{
				return (
					from x in this._db.GetShipInfoByFleetID(reserveFleetID.Value, true)
					where (x.AIFleetID == 0 || !x.AIFleetID.HasValue) && x.DesignInfo.GetRealShipClass().HasValue && !ShipSectionAsset.IsWeaponBattleRiderClass(x.DesignInfo.GetRealShipClass().Value)
					select x).ToList<ShipInfo>();
			}
			return new List<ShipInfo>();
		}
		private void BuildFleet(AIInfo aiInfo, AdmiralInfo admiral, StarSystemInfo system, FleetTemplate fleetTemplate = null, List<BuildScreenState.InvoiceItem> desiredInvoice = null, bool tryReserve = false)
		{
			StrategicAI.TraceVerbose(string.Format("  Building a new fleet with Admiral {0} at system {1}...", admiral, system));
			FleetTemplate fleetTemplate2 = fleetTemplate;
			if (fleetTemplate2 == null)
			{
				List<FleetTemplate> source = (
					from x in this._db.AssetDatabase.FleetTemplates
					where x.StanceWeights.ContainsKey(aiInfo.Stance) && !x.Initial
					select x).ToList<FleetTemplate>();
				IEnumerable<Weighted<FleetTemplate>> enumerable = 
					from x in source
					select new Weighted<FleetTemplate>(x, x.StanceWeights[aiInfo.Stance]);
				if (enumerable.Count<Weighted<FleetTemplate>>() <= 0 || source.Count<FleetTemplate>() <= 0)
				{
					StrategicAI.TraceVerbose("    Cannot proceed: no available fleet templates.");
					return;
				}
				fleetTemplate2 = WeightedChoices.Choose<FleetTemplate>(this._random, enumerable);
			}
			StrategicAI.TraceVerbose(string.Format("    Selected fleet template: {0}", fleetTemplate2.Name));
			AIFleetInfo aIFleetInfo = new AIFleetInfo();
			aIFleetInfo.AdmiralID = new int?(admiral.ID);
			aIFleetInfo.FleetType = MissionTypeExtensions.SerializeList(fleetTemplate2.MissionTypes);
			aIFleetInfo.SystemID = system.ID;
			aIFleetInfo.FleetTemplate = fleetTemplate2.Name;
			aIFleetInfo.ID = this._db.InsertAIFleetInfo(this._player.ID, aIFleetInfo);
			List<ShipInfo> freeReserveShips = new List<ShipInfo>();
			if (tryReserve)
			{
				freeReserveShips = this.GetUnclaimedShipsInReserve(system.ID);
			}
			int invoiceId;
			int num;
			if (desiredInvoice != null && desiredInvoice.Count > 0)
			{
				List<BuildScreenState.InvoiceItem> list = desiredInvoice;
				if (tryReserve)
				{
					list = this.TryFillWithShipFromReserve(aIFleetInfo, freeReserveShips, desiredInvoice);
				}
				if (list.Count == 0)
				{
					return;
				}
				this.OpenInvoice(system.ID, admiral.Name, out invoiceId, out num);
				aIFleetInfo.InvoiceID = new int?(num);
				this._db.UpdateAIFleetInfo(aIFleetInfo);
				using (List<BuildScreenState.InvoiceItem>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						BuildScreenState.InvoiceItem current = enumerator.Current;
						this.AddShipToInvoice(system.ID, this._db.GetDesignInfo(current.DesignID), invoiceId, num, new int?(aIFleetInfo.ID));
					}
					return;
				}
			}
			List<ShipInclude> list2 = fleetTemplate2.ShipIncludes;
			if (tryReserve)
			{
				list2 = this.TryFillWithShipFromReserve(aIFleetInfo, freeReserveShips, fleetTemplate2.ShipIncludes);
			}
			if (list2.Count == 0)
			{
				return;
			}
			this.OpenInvoice(system.ID, admiral.Name, out invoiceId, out num);
			aIFleetInfo.InvoiceID = new int?(num);
			this._db.UpdateAIFleetInfo(aIFleetInfo);
			foreach (ShipInclude current2 in list2)
			{
				if ((string.IsNullOrEmpty(current2.Faction) || !(current2.Faction != this._player.Faction.Name)) && !(current2.FactionExclusion == this._player.Faction.Name))
				{
					DesignInfo designByRole = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(aiInfo.Stance), current2.ShipRole, current2.WeaponRole);
					int num2;
					if (current2.InclusionType == ShipInclusionType.FILL)
					{
						DesignInfo designByRole2 = DesignLab.GetDesignByRole(this._game, this._player, this._techStyles, new AIStance?(aiInfo.Stance), ShipRole.COMMAND, null);
						num2 = DesignLab.GetTemplateFillAmount(this._db, fleetTemplate2, designByRole2, designByRole);
					}
					else
					{
						num2 = current2.Amount;
					}
					for (int i = 0; i < num2; i++)
					{
						if (designByRole != null)
						{
							this.AddShipToInvoice(system.ID, designByRole, invoiceId, num, new int?(aIFleetInfo.ID));
						}
					}
				}
			}
		}
		private float AssessExpansionRoom()
		{
			int playerFactionID = this._db.GetPlayerFactionID(this._player.ID);
			string factionName = this._db.GetFactionName(playerFactionID);
			float factionSuitability = this._db.GetFactionSuitability(factionName);
			List<int> list = new List<int>();
			float num = this._db.FindCurrentDriveSpeedForPlayer(this._player.ID) * 6f;
			if (factionName.Contains("hiver"))
			{
				num *= 6f;
			}
			foreach (StationInfo current in this._db.GetStationInfosByPlayerID(this._player.ID))
			{
				OrbitalObjectInfo orbitalObjectInfo = this._db.GetOrbitalObjectInfo(current.OrbitalObjectID);
				Vector3 starSystemOrigin = this._db.GetStarSystemOrigin(orbitalObjectInfo.StarSystemID);
				foreach (StarSystemInfo current2 in this._db.GetSystemsInRange(starSystemOrigin, num))
				{
					if (!list.Contains(current2.ID))
					{
						list.Add(current2.ID);
					}
				}
			}
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			foreach (int current3 in list)
			{
				StarSystemInfo starSystemInfo = this._db.GetStarSystemInfo(current3);
				int lastTurnExploredByPlayer = this._db.GetLastTurnExploredByPlayer(this._player.ID, starSystemInfo.ID);
				if (lastTurnExploredByPlayer > 0)
				{
					int num5 = 0;
					PlanetInfo[] starSystemPlanetInfos = this._db.GetStarSystemPlanetInfos(starSystemInfo.ID);
					for (int i = 0; i < starSystemPlanetInfos.Length; i++)
					{
						PlanetInfo planetInfo = starSystemPlanetInfos[i];
						ColonyInfo colonyInfoForPlanet = this._db.GetColonyInfoForPlanet(planetInfo.ID);
						if (colonyInfoForPlanet == null)
						{
							float num6 = Math.Abs(planetInfo.Suitability - factionSuitability);
							if (num6 < 200f)
							{
								num5++;
							}
						}
						else
						{
							if (colonyInfoForPlanet.PlayerID != this._player.ID && colonyInfoForPlanet.TurnEstablished < lastTurnExploredByPlayer)
							{
								num4++;
								break;
							}
						}
					}
					num2 += num5;
				}
				else
				{
					num3++;
				}
			}
			float result;
			if (num4 > 0)
			{
				result = (float)(num2 + num3) / (float)num4;
			}
			else
			{
				result = (float)(num2 + num3);
			}
			return result;
		}
		private float AssessPlayerStrength(int otherPlayerID)
		{
			int num = this._db.GetColonyIntelOfTargetPlayer(this._player.ID, otherPlayerID).Count<AIColonyIntel>();
			int num2 = 0;
			int num3 = 0;
			foreach (int current in this._db.GetStarSystemIDs())
			{
				num2++;
				if (this._db.GetLastTurnExploredByPlayer(this._player.ID, current) > 0)
				{
					num3++;
				}
			}
			float num4 = (float)(num2 - num3) / (float)num3;
			int num5 = (int)((float)num * num4) / 2;
			int num6 = 0;
			foreach (AIFleetIntel current2 in this._db.GetFleetIntelsOfTargetPlayer(this._player.ID, otherPlayerID))
			{
				num6 += current2.NumDestroyers + current2.NumCruisers * 3 + current2.NumDreadnoughts * 9 + current2.NumLeviathans * 27;
			}
			if (num5 + num6 == 0)
			{
				return 0f;
			}
			float num7 = (float)this.AssessPlayerTechLevel(otherPlayerID);
			float num8 = (float)num * 10f + (float)num5 * 7.5f + (float)num6 + num7 * 50f;
			StrategicAI.TraceVerbose(string.Format("  Player {0} strength: {1}", otherPlayerID, num8));
			return num8;
		}
		private float AssessOwnStrength()
		{
			int num = 0;
			foreach (ColonyInfo current in this._db.GetColonyInfos())
			{
				if (current.PlayerID == this._player.ID)
				{
					num++;
				}
			}
			int num2 = 0;
			int num3 = 0;
			foreach (FleetInfo current2 in this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL))
			{
				foreach (ShipInfo current3 in this._db.GetShipInfoByFleetID(current2.ID, false))
				{
					DesignInfo designInfo = this._db.GetDesignInfo(current3.DesignID);
					if (designInfo.Class == ShipClass.BattleRider)
					{
						num3++;
					}
					else
					{
						if (designInfo.Class == ShipClass.Cruiser)
						{
							if (designInfo.Role == ShipRole.COMBAT || designInfo.Role == ShipRole.COMMAND)
							{
								num3 += 3;
							}
							else
							{
								num2++;
							}
						}
						else
						{
							if (designInfo.Class == ShipClass.Dreadnought)
							{
								num3 += 9;
							}
							else
							{
								if (designInfo.Class == ShipClass.Leviathan)
								{
									num3 += 27;
								}
							}
						}
					}
				}
			}
			int num4 = this.AssessOwnTechLevel();
			float num5 = (float)num * 10f + (float)num3 * 2f + (float)num4 * 50f + (float)num2;
			StrategicAI.TraceVerbose(string.Format("Comparing {0} strength vs...", num5));
			return num5;
		}
		private int AssessPlayerTechLevel(int otherPlayerID)
		{
			int num = 0;
			bool flag = false;
			foreach (AIDesignIntel current in this._db.GetDesignIntelsOfTargetPlayer(this._player.ID, otherPlayerID))
			{
				foreach (string current2 in this._db.GetDesignSectionNames(current.DesignID))
				{
					if (current2.Contains("antimatter"))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					num += 3;
					break;
				}
			}
			int num2 = 1;
			foreach (AIFleetIntel current3 in this._db.GetFleetIntelsOfTargetPlayer(this._player.ID, otherPlayerID))
			{
				if (current3.NumLeviathans > 0)
				{
					num2 = 3;
					break;
				}
				if (current3.NumDreadnoughts > 0)
				{
					num2 = 2;
				}
			}
			num += num2;
			return num;
		}
		private int AssessOwnTechLevel()
		{
			int num = 1;
			if (this._db.PlayerHasAntimatter(this._player.ID))
			{
				num += 3;
			}
			if (this._db.PlayerHasDreadnoughts(this._player.ID))
			{
				num++;
			}
			if (this._db.PlayerHasLeviathans(this._player.ID))
			{
				num++;
			}
			return num;
		}
		public static StrategicAI.DesignConfigurationInfo GetDesignConfigurationInfo(GameSession game, DesignInfo design)
		{
			StrategicAI.DesignConfigurationInfo result = default(StrategicAI.DesignConfigurationInfo);
			result.Maxspeed += design.TopSpeed;
			DesignSectionInfo[] designSections = design.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				if (designSectionInfo.ShipSectionAsset.RequiredTechs.Contains("SLD_Disruptor_Shields"))
				{
					result.EnergyDefense += 10f;
				}
				foreach (int current in designSectionInfo.Techs)
				{
					string techFileID = game.GameDatabase.GetTechFileID(current);
					string key;
					switch (key = techFileID)
					{
					case "IND_Reflective":
						result.EnergyDefense += 5f;
						break;
					case "IND_Improved_Reflective":
						result.EnergyDefense += 10f;
						break;
					case "IND_Polysteel":
						result.Defensive += 1f;
						break;
					case "IND_MagnoCeramic_Latices":
						result.Defensive += 2f;
						break;
					case "IND_Quark_Resonators":
						result.Defensive += 3f;
						break;
					case "IND_Adamantine_Alloys":
						result.Defensive += 6f;
						break;
					case "SLD_Shield_Mk._I":
						result.Defensive += 1f;
						break;
					case "SLD_Shield_Mk._II":
						result.Defensive += 2f;
						break;
					case "SLD_Shield_Mk._III":
						result.Defensive += 3f;
						break;
					case "SLD_Shield_Mk._IV":
						result.Defensive += 4f;
						break;
					case "LD_Grav_Shields":
						result.BallisticsDefense += 100f;
						break;
					case "SLD_Meson_Shields":
						result.EnergyDefense += 100f;
						break;
					}
				}
				foreach (WeaponBankInfo weaponBank in designSectionInfo.WeaponBanks)
				{
					if (weaponBank.WeaponID.HasValue)
					{
						string weaponFile = game.GameDatabase.GetWeaponAsset(weaponBank.WeaponID.Value);
						LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weaponFile);
						if (logicalWeapon != null)
						{
							LogicalBank logicalBank = designSectionInfo.ShipSectionAsset.Banks.FirstOrDefault((LogicalBank x) => x.GUID == weaponBank.BankGUID);
							if (logicalBank != null)
							{
								WeaponEnums.TurretClasses turretClass = logicalBank.TurretClass;
								switch (turretClass)
								{
								case WeaponEnums.TurretClasses.Standard:
									if (logicalWeapon.PayloadType == WeaponEnums.PayloadTypes.Bolt)
									{
										if (logicalWeapon.Traits.Contains(WeaponEnums.WeaponTraits.Energy))
										{
											result.EnergyWeapons += 1f;
										}
										else
										{
											result.BallisticsWeapons += 1f;
										}
									}
									break;
								case WeaponEnums.TurretClasses.Missile:
									result.MissileWeapons += 1f;
									break;
								default:
									if (turretClass == WeaponEnums.TurretClasses.HeavyBeam)
									{
										result.HeavyBeamWeapons += 10f;
									}
									break;
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}
