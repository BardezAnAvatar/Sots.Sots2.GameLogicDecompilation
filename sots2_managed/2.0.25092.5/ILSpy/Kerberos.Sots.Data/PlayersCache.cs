using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal sealed class PlayersCache : RowCache<int, PlayerInfo>
	{
		private DiplomacyStatesCache diplomacy_states;
		public PlayersCache(SQLiteConnection db, AssetDatabase assets, DiplomacyStatesCache diplomacy_states) : base(db, assets)
		{
			this.diplomacy_states = diplomacy_states;
		}
		public static PlayerInfo GetPlayerInfoFromRow(SQLiteConnection db, Row row)
		{
			PlayerInfo playerInfo = new PlayerInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				Name = row[1].ToString(),
				FactionID = row[2].SQLiteValueToInteger(),
				PrimaryColor = row[4].SQLiteValueToVector3(),
				SecondaryColor = row[5].SQLiteValueToVector3(),
				BadgeAssetPath = row[6].ToString(),
				Savings = row[7].SQLiteValueToDouble(),
				Homeworld = row[8].SQLiteValueToNullableInteger(),
				AvatarAssetPath = row[9].ToString(),
				LastCombatTurn = row[10].SQLiteValueToInteger(),
				LastEncounterTurn = row[11].SQLiteValueToInteger(),
				RateGovernmentResearch = row[12].SQLiteValueToSingle(),
				RateResearchCurrentProject = row[13].SQLiteValueToSingle(),
				RateResearchSpecialProject = row[14].SQLiteValueToSingle(),
				RateResearchSalvageResearch = row[15].SQLiteValueToSingle(),
				RateGovernmentStimulus = row[16].SQLiteValueToSingle(),
				RateGovernmentSecurity = row[17].SQLiteValueToSingle(),
				RateGovernmentSavings = row[18].SQLiteValueToSingle(),
				RateStimulusMining = row[19].SQLiteValueToSingle(),
				RateStimulusColonization = row[20].SQLiteValueToSingle(),
				RateStimulusTrade = row[21].SQLiteValueToSingle(),
				RateSecurityOperations = row[22].SQLiteValueToSingle(),
				RateSecurityIntelligence = row[23].SQLiteValueToSingle(),
				RateSecurityCounterIntelligence = row[24].SQLiteValueToSingle(),
				isStandardPlayer = row[25].SQLiteValueToBoolean(),
				GenericDiplomacyPoints = row[26].SQLiteValueToInteger(),
				RateTax = row[27].SQLiteValueToSingle(),
				RateImmigration = row[28].SQLiteValueToSingle(),
				IntelPoints = row[29].SQLiteValueToInteger(),
				CounterIntelPoints = row[30].SQLiteValueToInteger(),
				OperationsPoints = row[31].SQLiteValueToInteger(),
				IntelAccumulator = row[32].SQLiteValueToInteger(),
				CounterIntelAccumulator = row[33].SQLiteValueToInteger(),
				OperationsAccumulator = row[34].SQLiteValueToInteger(),
				CivilianMiningAccumulator = row[35].SQLiteValueToInteger(),
				CivilianColonizationAccumulator = row[36].SQLiteValueToInteger(),
				CivilianTradeAccumulator = row[37].SQLiteValueToInteger(),
				SubfactionIndex = row[38].SQLiteValueToInteger(),
				AdditionalResearchPoints = row[39].SQLiteValueToInteger(),
				PsionicPotential = row[40].SQLiteValueToInteger(),
				isDefeated = row[41].SQLiteValueToBoolean(),
				CurrentTradeIncome = row[42].SQLiteValueToDouble(),
				includeInDiplomacy = row[43].SQLiteValueToBoolean(),
				isAIRebellionPlayer = row[44].SQLiteValueToBoolean(),
				AutoPlaceDefenseAssets = row[45].SQLiteValueToBoolean(),
				AutoRepairShips = row[46].SQLiteValueToBoolean(),
				AutoUseGoopModules = row[47].SQLiteValueToBoolean(),
				AutoUseJokerModules = row[48].SQLiteValueToBoolean(),
				ResearchBoostFunds = row[49].SQLiteValueToDouble(),
				AutoAoe = row[50].SQLiteValueToBoolean(),
				Team = row[51].SQLiteValueToInteger(),
				AutoPatrol = row[52].SQLiteValueToBoolean(),
				AIDifficulty = (row.Count<string>() > 53 && row[53] != null) ? ((AIDifficulty)Enum.Parse(typeof(AIDifficulty), row[53].ToString())) : AIDifficulty.Normal,
				RateTaxPrev = (row.Count<string>() > 54 && row[54] != null) ? row[54].SQLiteValueToSingle() : row[27].SQLiteValueToSingle()
			};
			playerInfo.FactionDiplomacyPoints = PlayersCache.GetFactionDiplomacyPoints(db, playerInfo.ID);
			return playerInfo;
		}
		private static void InsertGovernment(SQLiteConnection db, int playerID, float auth, float econLib)
		{
			db.ExecuteNonQuery(string.Format(Queries.InsertGovernment, playerID.ToSQLiteValue(), auth.ToSQLiteValue(), econLib.ToSQLiteValue()), false, true);
		}
		private IEnumerable<string> GetPlayerNames()
		{
			return 
				from x in base.Values
				select x.Name;
		}
		public IEnumerable<int> GetStandardPlayerIDs()
		{
			return 
				from x in base.Values
				where x.isStandardPlayer
				select x into y
				select y.ID;
		}
		public IEnumerable<int> GetPlayerIDs()
		{
			return 
				from y in base.Values
				select y.ID;
		}
		private string MakeUniquePlayerName(string name)
		{
			string newname = name;
			List<string> source = this.GetPlayerNames().ToList<string>();
			int num = 2;
			while (true)
			{
				if (!source.Any((string x) => x.Equals(newname, StringComparison.InvariantCultureIgnoreCase)))
				{
					break;
				}
				newname = string.Concat(new object[]
				{
					name,
					" (",
					num,
					")"
				});
				num++;
			}
			return newname;
		}
		public static FactionInfo GetFactionInfoFromRow(Row row)
		{
			return new FactionInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				Name = row[1].SQLiteValueToString(),
				IdealSuitability = row[2].SQLiteValueToSingle()
			};
		}
		public int GetPlayerFactionID(int playerId)
		{
			if (base.ContainsKey(playerId))
			{
				return base[playerId].FactionID;
			}
			return 0;
		}
		public static IEnumerable<FactionInfo> GetFactions(SQLiteConnection db)
		{
			Table table = db.ExecuteTableQuery(Queries.GetFactions, true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return PlayersCache.GetFactionInfoFromRow(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public int GetDefaultDiplomacyReactionValue(int player1, int player2)
		{
			int result = DiplomacyInfo.DefaultDeplomacyRelations;
			int p1Faction = this.GetPlayerFactionID(player1);
			int p2Faction = this.GetPlayerFactionID(player2);
			Faction faction = base.Assets.Factions.FirstOrDefault((Faction x) => x.ID == p1Faction);
			Faction faction2 = base.Assets.Factions.FirstOrDefault((Faction x) => x.ID == p2Faction);
			if (faction != null && faction2 != null)
			{
				result = faction.GetDefaultReactionToFaction(faction2);
			}
			return result;
		}
		public static Dictionary<int, int> GetFactionDiplomacyPoints(SQLiteConnection db, int playerId)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Table table = db.ExecuteTableQuery(string.Format(Queries.GetFactionDiplomacyPointsForPlayer, playerId.ToSQLiteValue()), true);
			foreach (Row current in table)
			{
				dictionary.Add(current[1].SQLiteValueToInteger(), current[2].SQLiteValueToInteger());
			}
			foreach (FactionInfo current2 in PlayersCache.GetFactions(db))
			{
				if (!dictionary.ContainsKey(current2.ID))
				{
					dictionary.Add(current2.ID, 0);
				}
			}
			return dictionary;
		}
		protected override int OnInsert(SQLiteConnection db, int? key, PlayerInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Player insertion does not permit explicit specification of an ID.");
			}
			if (value.Homeworld == 0)
			{
				throw new ArgumentOutOfRangeException("value.Homeworld", "Nullable foreign key must never be 0 because this can violate database equivalence constraints. If the intent is to say that no such foreign key exists then use null instead.");
			}
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertPlayer, new object[]
			{
				"NULL".ToSQLiteValue(),
				this.MakeUniquePlayerName(value.Name).ToSQLiteValue(),
				value.FactionID.ToSQLiteValue(),
				value.Homeworld.ToNullableSQLiteValue(),
				value.PrimaryColor.ToSQLiteValue(),
				value.SecondaryColor.ToSQLiteValue(),
				value.BadgeAssetPath.ToSQLiteValue(),
				value.Savings.ToSQLiteValue(),
				value.AvatarAssetPath.ToSQLiteValue(),
				value.isStandardPlayer.ToSQLiteValue(),
				value.SubfactionIndex.ToSQLiteValue(),
				value.includeInDiplomacy.ToSQLiteValue(),
				value.isAIRebellionPlayer.ToSQLiteValue(),
				value.AutoPlaceDefenseAssets.ToSQLiteValue(),
				value.AutoRepairShips.ToSQLiteValue(),
				value.AutoUseGoopModules.ToSQLiteValue(),
				value.AutoUseJokerModules.ToSQLiteValue(),
				value.AutoAoe.ToSQLiteValue(),
				value.Team.ToSQLiteValue(),
				value.AIDifficulty.ToString().ToSQLiteValue()
			}));
			PlayersCache.InsertGovernment(db, num, 0f, 0f);
			foreach (int current in this.GetPlayerIDs())
			{
				if (current != num)
				{
					this.diplomacy_states.InsertDiplomaticState(num, current, (value.isStandardPlayer || value.includeInDiplomacy) ? DiplomacyState.NEUTRAL : DiplomacyState.WAR, this.GetDefaultDiplomacyReactionValue(num, current), false, true);
				}
			}
			value.ID = num;
			return num;
		}
		protected override void OnUpdate(SQLiteConnection db, int key, PlayerInfo value)
		{
			throw new NotImplementedException("There is no general PlayerInfo update.");
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			throw new NotImplementedException("There is no general PlayerInfo remove.");
		}
		protected override IEnumerable<KeyValuePair<int, PlayerInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row current in db.ExecuteTableQuery(Queries.GetPlayerInfos, true))
				{
					PlayerInfo playerInfoFromRow = PlayersCache.GetPlayerInfoFromRow(db, current);
					yield return new KeyValuePair<int, PlayerInfo>(playerInfoFromRow.ID, playerInfoFromRow);
				}
			}
			else
			{
				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetPlayerInfo, current2.ToSQLiteValue()), true))
					{
						PlayerInfo playerInfoFromRow2 = PlayersCache.GetPlayerInfoFromRow(db, current3);
						yield return new KeyValuePair<int, PlayerInfo>(playerInfoFromRow2.ID, playerInfoFromRow2);
					}
				}
			}
			yield break;
		}
	}
}
