using Kerberos.Sots.Data.GenericFramework;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.StarSystemPathing;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
namespace Kerberos.Sots.Data
{
	internal class GameDatabase : IDisposable
	{
		public enum TurnLengthTypes
		{
			Strategic,
			Combat
		}
		private const string clientIdLine = "--Setting client ID to: ";
		public const string DefaultLiveLocation = ":memory:";
		public const int Version = 2080;
		public const int EOF_MIN_DB = 2000;
		public const int CombatDataVersion = 1;
		public const float DefaultSystemSensorRange = 5f;
		private static Dictionary<int, Dictionary<StratModifiers, CachedStratMod>> _cachedStratMods = new Dictionary<int, Dictionary<StratModifiers, CachedStratMod>>();
		private SQLiteConnection db;
		private AssetDatabase assetdb;
		private string _location = ":memory:";
		private int _clientId;
		private readonly DataObjectCache _dom;
		private int insertPlayerTechBranchCount;
		private List<MoveOrderInfo> TempMoveOrders;
		private static bool PlayersAlwaysFight = false;
		public AssetDatabase AssetDatabase
		{
			get
			{
				return this.assetdb;
			}
		}
		public string LiveLocation
		{
			get
			{
				return this._location;
			}
		}
		public void ReplayQueryHistory(string outputDatabaseFilename, int count)
		{
			GameDatabase gameDatabase = GameDatabase.New(this.GetGameName(), this.assetdb, false);
			Table table = this.db.ExecuteTableQuery(string.Format("SELECT query FROM query_history WHERE id <= {0}", count), true);
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				if (row[0].StartsWith("--Setting client ID to: "))
				{
					int clientId = int.Parse(row[0].Substring("--Setting client ID to: ".Length));
					gameDatabase.SetClientId(clientId);
				}
				else
				{
					try
					{
						gameDatabase.db.ExecuteNonQuery(row[0], true, true);
					}
					catch (SQLiteException ex)
					{
						throw new SQLiteException(string.Format("While executing query: {0}\n{1}", row[0], ex.Message));
					}
				}
			}
			gameDatabase.Save(outputDatabaseFilename);
		}
		public void SetResearchEfficiency(int odds)
		{
			string name = "ResearchEfficiency";
			if (this.GetNameValue(name) == null)
			{
				this.InsertNameValuePair(name, string.Empty);
			}
			this.UpdateNameValuePair(name, odds.ToString());
		}
		public int GetResearchEfficiency()
		{
			string name = "ResearchEfficiency";
			string nameValue = this.GetNameValue(name);
			int result;
			if (string.IsNullOrEmpty(nameValue) || !int.TryParse(nameValue, out result))
			{
				return 0;
			}
			return result;
		}
		public void SetEconomicEfficiency(int odds)
		{
			string name = "EconomicEfficiency";
			if (this.GetNameValue(name) == null)
			{
				this.InsertNameValuePair(name, string.Empty);
			}
			this.UpdateNameValuePair(name, odds.ToString());
		}
		public int GetEconomicEfficiency()
		{
			string name = "EconomicEfficiency";
			string nameValue = this.GetNameValue(name);
			int result;
			if (string.IsNullOrEmpty(nameValue) || !int.TryParse(nameValue, out result))
			{
				return 0;
			}
			return result;
		}
		public void SetRandomEncounterFrequency(int odds)
		{
			string name = "RandomEncounterFrequency";
			if (this.GetNameValue(name) == null)
			{
				this.InsertNameValuePair(name, string.Empty);
			}
			this.UpdateNameValuePair(name, odds.ToString());
		}
		public int GetRandomEncounterFrequency()
		{
			string name = "RandomEncounterFrequency";
			string nameValue = this.GetNameValue(name);
			int result;
			if (string.IsNullOrEmpty(nameValue) || !int.TryParse(nameValue, out result))
			{
				return 0;
			}
			return result;
		}
		public void SetTurnLength(GameDatabase.TurnLengthTypes type, float minutes)
		{
			string name = (type == GameDatabase.TurnLengthTypes.Strategic) ? "StrategicTurnLength" : "CombatTurnLength";
			if (this.GetNameValue(name) == null)
			{
				this.InsertNameValuePair(name, string.Empty);
			}
			int num = (int)Math.Floor((double)(Math.Max(minutes, 0f) * 60f));
			this.UpdateNameValuePair(name, (minutes == 3.40282347E+38f) ? string.Empty : num.ToString());
		}
		public float GetTurnLength(GameDatabase.TurnLengthTypes type)
		{
			if (type == GameDatabase.TurnLengthTypes.Strategic)
			{
				return 3.40282347E+38f;
			}
			string name = (type == GameDatabase.TurnLengthTypes.Strategic) ? "StrategicTurnLength" : "CombatTurnLength";
			string nameValue = this.GetNameValue(name);
			int num;
			if (string.IsNullOrEmpty(nameValue) || !int.TryParse(nameValue, out num))
			{
				return 3.40282347E+38f;
			}
			return (float)num / 60f;
		}
		public int GetSystemDefensePoints(int SystemID, int PlayerID)
		{
			StationInfo navalStationForSystemAndPlayer = this.GetNavalStationForSystemAndPlayer(SystemID, PlayerID);
			int num = 0;
			if (navalStationForSystemAndPlayer != null)
			{
				num = this.GetDesignCommandPointQuota(this.assetdb, navalStationForSystemAndPlayer.DesignInfo.ID);
			}
			List<ColonyInfo> source = this.GetColonyInfosForSystem(SystemID).ToList<ColonyInfo>();
			if (source.Count((ColonyInfo x) => x.PlayerID == PlayerID) > 0)
			{
				num += 3;
			}
			return num;
		}
		public int GetAllocatedSystemDefensePoints(StarSystemInfo system, int playerId)
		{
			FleetInfo fleetInfo = this.InsertOrGetDefenseFleetInfo(system.ID, playerId);
			if (fleetInfo == null)
			{
				return 0;
			}
			return (
				from x in this.GetShipInfoByFleetID(fleetInfo.ID, false)
				where x.IsPlaced()
				select x).Sum((ShipInfo y) => this.assetdb.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo));
		}
		public int GetDefenseAssetCPCost(int shipid)
		{
			ShipInfo shipInfo = this.GetShipInfo(shipid, false);
			if (shipInfo != null)
			{
				return this.assetdb.DefenseManagerSettings.GetDefenseAssetCPCost(shipInfo.DesignInfo);
			}
			return 0;
		}
		private CounterIntelResponse ParseCounterIntelResponse(Row row)
		{
			if (row != null)
			{
				return new CounterIntelResponse
				{
					ID = row[0].SQLiteValueToInteger(),
					IntelMissionID = row[1].SQLiteValueToInteger(),
					auto = row[2].SQLiteValueToBoolean(),
					value = row[3].ToSQLiteValue()
				};
			}
			return null;
		}
		public void InsertCounterIntelResponse(int intelmission_id, bool auto = true, string value = "")
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertCounterIntelResponse, intelmission_id.ToSQLiteValue(), auto.ToSQLiteValue(), value), false, true);
		}
		public IEnumerable<CounterIntelResponse> GetCounterIntelResponses(int intel_mission_id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetCounterIntelResponsesForIntel, intel_mission_id), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseCounterIntelResponse(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void RemoveCounterIntelResponse(int responseid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveCounterIntelResponse, responseid.ToSQLiteValue()), false, true);
		}
		private CounterIntelStingMission ParseIntelStingInfo(Row row)
		{
			if (row != null)
			{
				return new CounterIntelStingMission
				{
					ID = row[0].SQLiteValueToInteger(),
					PlayerId = row[1].SQLiteValueToInteger(),
					TargetPlayerId = row[2].SQLiteValueToInteger()
				};
			}
			return null;
		}
		public IEnumerable<CounterIntelStingMission> GetCountIntelStingsForPlayer(int playerid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.SelectCounterIntelStingsForPlayer, playerid), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseIntelStingInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<CounterIntelStingMission> GetCountIntelStingsAgainstPlayer(int playerid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.SelectCounterIntelStingsAgainstPlayer, playerid), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseIntelStingInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<CounterIntelStingMission> GetCountIntelStingsForPlayerAgainstPlayer(int playerid, int targetplayer)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.SelectCounterIntelStingsForPlayerAgainstPlayer, playerid, targetplayer), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseIntelStingInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void InsertCounterIntelSting(int playerid, int targetplayerid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertCounterIntelSting, playerid.ToSQLiteValue(), targetplayerid.ToSQLiteValue()), false, true);
		}
		public void RemoveCounterIntelSting(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveCounterIntelSting, id), false, true);
		}
		private IntelMissionInfo ParseIntelInfo(Row row)
		{
			if (row != null)
			{
				return new IntelMissionInfo
				{
					ID = row[0].SQLiteValueToInteger(),
					PlayerId = row[1].SQLiteValueToInteger(),
					TargetPlayerId = row[2].SQLiteValueToInteger(),
					BlamePlayer = row[3].SQLiteValueToNullableInteger(),
					Turn = row[4].SQLiteValueToInteger(),
					MissionType = (IntelMission)row[5].SQLiteValueToInteger()
				};
			}
			return null;
		}
		public IEnumerable<IntelMissionInfo> GetIntelInfosForPlayer(int playerid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelMissionsForPlayer, playerid), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseIntelInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IntelMissionInfo GetIntelInfo(int intel_id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.SelectIntelMissionInfo, intel_id), true);
			if (table.Count<Row>() > 0)
			{
				return this.ParseIntelInfo(table[0]);
			}
			return null;
		}
		public void InsertIntelMission(int playerid, int targetplayerid, IntelMission type, int? BlamePlayer = null)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIntelMission, new object[]
			{
				playerid.ToSQLiteValue(),
				targetplayerid.ToSQLiteValue(),
				this.GetTurnCount().ToSQLiteValue(),
				((int)type).ToSQLiteValue(),
				BlamePlayer.ToNullableSQLiteValue()
			}), false, true);
		}
		public void RemoveIntelMission(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveIntelMission, id.ToSQLiteValue()), false, true);
		}
		public int InsertLoaFleetComposition(int playerid, string Name, IEnumerable<int> Designs)
		{
			int num = this.db.ExecuteIntegerQuery(string.Format(Queries.InsertLoaFleetComposition, playerid.ToSQLiteValue(), Name.ToSQLiteValue()));
			foreach (int current in Designs)
			{
				DesignInfo designInfo = this.GetDesignInfo(current);
				if (!(designInfo.GetRealShipClass() == RealShipClasses.Drone) && !(designInfo.GetRealShipClass() == RealShipClasses.BoardingPod) && !(designInfo.GetRealShipClass() == RealShipClasses.AssaultShuttle) && !designInfo.IsLoaCube())
				{
					this.db.ExecuteIntegerQuery(string.Format(Queries.InsertLoaFleetShipDef, num, current));
				}
			}
			return num;
		}
		private LoaFleetComposition ParseLoaFleetComposition(Row row)
		{
			if (row != null)
			{
				return new LoaFleetComposition
				{
					ID = row[0].SQLiteValueToInteger(),
					PlayerID = row[1].SQLiteValueToInteger(),
					Name = row[2].SQLiteValueToString()
				};
			}
			return null;
		}
		private LoaFleetShipDef ParseLoaFleetShipDef(Row row)
		{
			if (row != null)
			{
				return new LoaFleetShipDef
				{
					ID = row[0].SQLiteValueToInteger(),
					CompositionID = row[1].SQLiteValueToInteger(),
					DesignID = row[2].SQLiteValueToInteger()
				};
			}
			return null;
		}
		public IEnumerable<LoaFleetComposition> GetLoaFleetCompositions()
		{
			Table table = this.db.ExecuteTableQuery(Queries.SelectLoaFleetCompositions, true);
			List<LoaFleetComposition> list = new List<LoaFleetComposition>();
			foreach (Row current in table)
			{
				list.Add(this.ParseLoaFleetComposition(current));
			}
			foreach (LoaFleetComposition current2 in list)
			{
				current2.designs = new List<LoaFleetShipDef>();
				Table table2 = this.db.ExecuteTableQuery(string.Format(Queries.SelectLoaShipDefForComposition, current2.ID), true);
				foreach (Row current3 in table2)
				{
					current2.designs.Add(this.ParseLoaFleetShipDef(current3));
				}
			}
			return list;
		}
		public void DeleteLoaFleetCompositon(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DeleteLoaFleetCompositon, id.ToSQLiteValue()), false, true);
		}
		internal void ChangeLiveLocationAndOpen(string value)
		{
			this.ChangeLiveLocation(value);
			ShellHelper.ShellOpen(value);
		}
		public void ChangeLiveLocation(string value)
		{
			this.db.SaveBackup(value);
			SQLiteConnection sQLiteConnection = new SQLiteConnection(value);
			this.db.Dispose();
			this.db = sQLiteConnection;
			this._location = value;
		}
		private int QueryVersion()
		{
			return int.Parse(this.GetNameValue("dbver"));
		}
		private GameDatabase(SQLiteConnection dbConnection, AssetDatabase assetdb)
		{
			if (dbConnection == null)
			{
				throw new ArgumentNullException("dbConnection");
			}
			this.db = dbConnection;
			this.assetdb = assetdb;
			this.db.ExecuteNonQuery("PRAGMA foreign_keys = TRUE;", true, true);
			this._dom = new DataObjectCache(this.db, assetdb);
		}
		public static GameDatabase New(string gameName, AssetDatabase assetdb, bool initialize = true)
		{
			if (string.IsNullOrWhiteSpace(gameName))
			{
				throw new ArgumentNullException("Game name must be a valid non-whitespace string.");
			}
			gameName = gameName.Trim();
			SQLiteConnection sQLiteConnection = new SQLiteConnection(":memory:");
			sQLiteConnection.ExecuteNonQuery("PRAGMA synchronous = OFF;", false, true);
			sQLiteConnection.ExecuteNonQuery("PRAGMA journal_mode = OFF;", false, true);
			string statement;
			using (Stream manifestResourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream("Kerberos.Sots.Data.empty_game.sql"))
			{
				using (StreamReader streamReader = new StreamReader(manifestResourceStream))
				{
					statement = streamReader.ReadToEnd();
				}
			}
			sQLiteConnection.ExecuteNonQueryReferenceUTF8(statement);
			GameDatabase gameDatabase = new GameDatabase(sQLiteConnection, assetdb);
			if (initialize)
			{
				gameDatabase.InsertNameValuePair("dbver", 2080.ToString());
				gameDatabase.InsertNameValuePair("game_name", gameName);
				gameDatabase.InsertNameValuePair("turn", 0.ToString());
				gameDatabase.InsertNameValuePair("time_stamp", (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds.ToString());
			}
			gameDatabase.ClearStratModCache();
			return gameDatabase;
		}
		public void QueryLogging(bool value)
		{
			this.db.LogQueries = true;
		}
		public void UpdateDBVer(int value)
		{
			this.UpdateNameValuePair("dbver", value.ToString());
		}
		public void SaveMultiplayerSyncPoint(string cacheDir)
		{
			this.Save(Path.Combine(cacheDir, "network.db"));
		}
		public static GameDatabase Load(string filename, AssetDatabase assetdb)
		{
			SQLiteConnection sQLiteConnection = new SQLiteConnection(":memory:");
			sQLiteConnection.ExecuteNonQuery("PRAGMA synchronous = OFF;", false, true);
			sQLiteConnection.ExecuteNonQuery("PRAGMA journal_mode = OFF;", false, true);
			sQLiteConnection.LoadBackup(filename);
			GameDatabase gameDatabase = new GameDatabase(sQLiteConnection, assetdb);
			int num = gameDatabase.QueryVersion();
			if (num > 2080)
			{
				throw new InvalidDataException(string.Format(AssetDatabase.CommonStrings.Localize("@ERROR_UNSUPPORTED_SAVE_GAME"), 2080, num));
			}
			if (num < 2000)
			{
				sQLiteConnection.Dispose();
				throw new InvalidDataException(string.Format(AssetDatabase.CommonStrings.Localize("@PRE_EOF_SAVEGAME_UNSUPPORTED"), 2000, num));
			}
			if (num < 2080)
			{
				gameDatabase.Upgrade(num);
			}
			gameDatabase.ClearStratModCache();
			if (ScriptHost.AllowConsole)
			{
				gameDatabase.ValidateShipParents();
			}
			return gameDatabase;
		}
		public static GameDatabase Connect(string filename, AssetDatabase assetdb)
		{
			return new GameDatabase(new SQLiteConnection(filename), assetdb);
		}
		private bool IncrementalUpgrade(int dbver)
		{
			if (dbver <= 2000)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n\r\n                    CREATE TABLE [gives]\r\n                    (\r\n\t                    [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,\r\n\t                    [initiating_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,\r\n\t                    [receiving_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,\r\n\t                    [give_type] INTEGER NOT NULL,\r\n\t                    [give_value] NUMERIC\r\n                    );\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2010);
				return true;
			}
			if (dbver == 2010)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE loa_fleet_compositions ADD COLUMN [visible] BOOLEAN NOT NULL DEFAULT 'True';\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2020);
				return true;
			}
			if (dbver == 2020)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE invoice_build_orders ADD COLUMN [loa_cubes]\tINTEGER NOT NULL DEFAULT 0;\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2030);
				return true;
			}
			if (dbver == 2030)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE players ADD COLUMN [auto_patrol] BOOLEAN NOT NULL DEFAULT 'False';\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2040);
				return true;
			}
			if (dbver == 2040)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE players ADD COLUMN [ai_difficulty] TEXT NOT NULL DEFAULT 'Normal';\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2050);
				return true;
			}
			if (dbver == 2050)
			{
				List<ShipInfo> list = this.GetShipInfos(true).ToList<ShipInfo>();
				foreach (ShipInfo current in list)
				{
					ShipRole role = DesignLab.GetRole(current.DesignInfo.DesignSections.First((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission).ShipSectionAsset);
					if (current.DesignInfo.Role != role)
					{
						current.DesignInfo.Role = role;
						this.UpdateDesign(current.DesignInfo);
					}
				}
				this.UpdateDBVer(2060);
				return true;
			}
			if (dbver == 2060)
			{
				List<ShipInfo> list2 = this.GetShipInfos(true).ToList<ShipInfo>();
				foreach (ShipInfo current2 in list2)
				{
					if (current2.DesignInfo.Role != ShipRole.E_WARFARE)
					{
						ShipRole role2 = DesignLab.GetRole(current2.DesignInfo.DesignSections.First((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission).ShipSectionAsset);
						if (current2.DesignInfo.Role != role2)
						{
							current2.DesignInfo.Role = role2;
							this.UpdateDesign(current2.DesignInfo);
						}
					}
				}
				this.UpdateDBVer(2070);
				return true;
			}
			if (dbver == 2070)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE government_actions ADD COLUMN [turn] INTEGER NOT NULL DEFAULT 0;\r\n                    ALTER TABLE players ADD COLUMN [rate_tax_prev] NUMERIC NOT NULL DEFAULT '0.05';\r\n\r\n                    COMMIT;\r\n                ", true, true);
				List<PlayerInfo> list3 = this.GetStandardPlayerInfos().ToList<PlayerInfo>();
				foreach (PlayerInfo current3 in list3)
				{
					this.UpdatePreviousTaxRate(current3.ID, current3.RateTax);
				}
				this.UpdateDBVer(2080);
				return true;
			}
			return false;
		}
		public static bool CheckForPre_EOFSaves(App App)
		{
			SavedGameFilename[] allSaveGames = App.GetAllSaveGames();
			bool result = false;
			SavedGameFilename[] array = allSaveGames;
			for (int i = 0; i < array.Length; i++)
			{
				SavedGameFilename savedGameFilename = array[i];
				using (GameDatabase gameDatabase = GameDatabase.Connect(savedGameFilename.RootedFilename, App.AssetDatabase))
				{
					int num = gameDatabase.QueryVersion();
					if (num < 2000)
					{
						gameDatabase.Dispose();
						File.Delete(savedGameFilename.RootedFilename);
						result = true;
					}
				}
			}
			return result;
		}
		private void FixGenUniqueTable(string table, string column)
		{
			int[] array = this.db.ExecuteIntegerArrayQuery(string.Format("SELECT {0} FROM {1} ORDER BY {0} DESC;", column.ToSQLiteValue(), table.ToSQLiteValue()));
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < 16; i++)
			{
				hashSet.Add(i);
			}
			int[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				int num = array2[j];
				if (hashSet.Count == 0)
				{
					return;
				}
				int num2 = num & 15;
				if (hashSet.Contains(num2))
				{
					hashSet.Remove(num2);
					this.db.ExecuteNonQuery(string.Format("INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('{0}', {1});", table.ToSQLiteValue(), num2.ToSQLiteValue()), true, true);
					int value = num >> 4;
					this.db.ExecuteNonQuery(string.Format("UPDATE gen_unique SET current = {0} WHERE generator = '{1}' AND player_id = {2};", value.ToSQLiteValue(), table.ToSQLiteValue(), num2.ToSQLiteValue()), true, true);
				}
			}
		}
		private void ValidateBuildOrderPlayers()
		{
			this.RemoveBuildOrder(0);
			string query = "SELECT DISTINCT build_orders.id FROM build_orders\r\n                JOIN missions ON build_orders.mission_id=missions.id\r\n                JOIN designs ON build_orders.design_id=designs.id\r\n                JOIN fleets ON missions.fleet_id=fleets.id\r\n                WHERE fleets.player_id<>designs.player_id;";
			int[] array = this.db.ExecuteIntegerArrayQuery(query);
			int[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				int num = array2[i];
				GameDatabase.Warn(string.Format("Removing build order {0} because design owner doesn't match fleet owner!", num));
				this.RemoveBuildOrder(num);
			}
			bool arg_54_0 = ScriptHost.AllowConsole;
		}
		private void ValidateShipParents()
		{
			string query = "SELECT ships.id FROM ships\r\n                JOIN designs ON designs.id=ships.design_id\r\n                JOIN ships AS parent_ships ON ships.parent_id=parent_ships.id\r\n                JOIN designs AS parent_designs ON parent_designs.id=parent_ships.design_id\r\n                WHERE (ships.parent_id<>0 AND designs.player_id<>parent_designs.player_id)\r\n                ORDER BY designs.player_id;";
			int[] array = this.db.ExecuteIntegerArrayQuery(query);
			int[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				int num = array2[i];
				GameDatabase.Warn(string.Format("Removing ship {0} because it was parented to another player's ship!", num));
				this.RemoveShip(num);
			}
			bool arg_4D_0 = ScriptHost.AllowConsole;
		}
		private void FixShipStructures()
		{
			List<ShipInfo> list = this.GetShipInfos(true).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				List<SectionInstanceInfo> list2 = this.GetShipSectionInstances(current.ID).ToList<SectionInstanceInfo>();
				foreach (SectionInstanceInfo sect in list2)
				{
					DesignSectionInfo designSectionInfo = current.DesignInfo.DesignSections.First((DesignSectionInfo x) => x.ID == sect.SectionID);
					ShipSectionAsset shipSectionAsset = this.assetdb.GetShipSectionAsset(designSectionInfo.FilePath);
					List<string> list3 = new List<string>();
					if (designSectionInfo.Techs.Count > 0)
					{
						foreach (int current2 in designSectionInfo.Techs)
						{
							list3.Add(this.GetTechFileID(current2));
						}
					}
					int structureWithTech = Ship.GetStructureWithTech(this.assetdb, list3, shipSectionAsset.Structure);
					int minStructure = designSectionInfo.GetMinStructure(this, this.assetdb);
					if (sect.Structure > structureWithTech)
					{
						sect.Structure = structureWithTech;
						this.UpdateSectionInstance(sect);
					}
					else
					{
						if (sect.Structure < minStructure)
						{
							sect.Structure = minStructure;
							this.UpdateSectionInstance(sect);
						}
					}
					List<ModuleInstanceInfo> list4 = this.GetModuleInstances(sect.ID).ToList<ModuleInstanceInfo>();
					foreach (ModuleInstanceInfo current3 in list4)
					{
						if (current3.Structure < 0)
						{
							current3.Structure = 0;
							this.UpdateModuleInstance(current3);
						}
					}
					List<WeaponInstanceInfo> list5 = this.GetWeaponInstances(sect.ID).ToList<WeaponInstanceInfo>();
					foreach (WeaponInstanceInfo current4 in list5)
					{
						if (current4.Structure < 0f)
						{
							current4.Structure = 0f;
							this.UpdateWeaponInstance(current4);
						}
					}
				}
			}
		}
		private void FixStationModuleInstances()
		{
			List<StationInfo> list = this.GetStationInfos().ToList<StationInfo>();
			foreach (StationInfo si in list)
			{
				if (si.DesignInfo.StationLevel >= 2)
				{
					SectionInstanceInfo sectionInstanceInfo = this.GetShipSectionInstances(si.ShipID).FirstOrDefault<SectionInstanceInfo>();
					List<ModuleInstanceInfo> list2 = (sectionInstanceInfo != null) ? this.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>() : new List<ModuleInstanceInfo>();
					ShipSectionAsset shipSectionAsset = this.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == si.DesignInfo.DesignSections[0].FilePath);
					List<LogicalModuleMount> list3 = shipSectionAsset.Modules.ToList<LogicalModuleMount>();
					foreach (DesignModuleInfo module in si.DesignInfo.DesignSections[0].Modules)
					{
						ModuleInstanceInfo mii = list2.FirstOrDefault((ModuleInstanceInfo x) => x.ModuleNodeID == module.MountNodeName);
						if (mii != null)
						{
							list2.Remove(mii);
							list3.RemoveAll((LogicalModuleMount x) => x.NodeName == mii.ModuleNodeID);
						}
					}
					if (list2.Count != 0)
					{
						DesignInfo oldDesign = DesignLab.CreateStationDesignInfo(this.AssetDatabase, this, si.PlayerID, si.DesignInfo.StationType, si.DesignInfo.StationLevel - 1, false);
						string name = this.AssetDatabase.GetFaction(this.GetPlayerFactionID(si.PlayerID)).Name;
						StationModuleQueue.UpdateStationMapsForFaction(name);
						ShipSectionAsset shipSectionAsset2 = this.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == oldDesign.DesignSections[0].FilePath);
						List<LogicalModuleMount> source = shipSectionAsset2.Modules.ToList<LogicalModuleMount>();
						foreach (ModuleInstanceInfo moduleInst in list2)
						{
							LogicalModuleMount logicalModuleMount = source.FirstOrDefault((LogicalModuleMount x) => x.NodeName == moduleInst.ModuleNodeID);
							if (logicalModuleMount != null)
							{
								ModuleEnums.StationModuleType stationModuleType = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), logicalModuleMount.ModuleType);
								ModuleEnums.ModuleSlotTypes desiredModuleType = AssetDatabase.StationModuleTypeToMountTypeMap[stationModuleType];
								if (desiredModuleType == ModuleEnums.ModuleSlotTypes.Habitation && name != AssetDatabase.GetModuleFactionName(stationModuleType))
								{
									desiredModuleType = ModuleEnums.ModuleSlotTypes.AlienHabitation;
								}
								LogicalModuleMount logicalModuleMount2 = list3.FirstOrDefault((LogicalModuleMount x) => x.ModuleType == desiredModuleType.ToString());
								if (logicalModuleMount2 != null)
								{
									moduleInst.ModuleNodeID = logicalModuleMount2.NodeName;
									this.UpdateModuleInstance(moduleInst);
									list3.Remove(logicalModuleMount2);
								}
							}
						}
					}
				}
			}
		}
		private void ValidateGardenerFleets()
		{
			int playerID = Gardeners.GetPlayerID(this);
			List<GardenerInfo> list = this.GetGardenerInfos().ToList<GardenerInfo>();
			foreach (GardenerInfo current in list)
			{
				List<FleetInfo> source = this.GetFleetsByPlayerAndSystem(playerID, current.SystemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
				FleetInfo fleetInfo = source.FirstOrDefault<FleetInfo>();
				if (fleetInfo != null)
				{
					current.FleetId = fleetInfo.ID;
					this.UpdateGardenerInfo(current);
				}
			}
		}
		private void Upgrade(int dbver)
		{
			for (int i = dbver; i < 2080; i = this.QueryVersion())
			{
				if (!this.IncrementalUpgrade(i))
				{
					throw new InvalidDataException("Cannot upgrade save game: Database version " + dbver + " is not supported.");
				}
			}
		}
		internal void SaveAndOpen(string filename)
		{
			this.Save(filename);
			ShellHelper.ShellOpen(filename);
		}
		private static void Trace(string message)
		{
			App.Log.Trace(message, "data");
		}
		private static void TraceVerbose(string message)
		{
			App.Log.Trace(message, "data", LogLevel.Verbose);
		}
		private static void Warn(string message)
		{
			App.Log.Warn(message, "data");
		}
		public void Save(string filename)
		{
			GameDatabase.Trace("Saving game database to '" + filename + "'...");
			try
			{
				this.db.SaveBackup(filename);
			}
			catch (Exception)
			{
				GameDatabase.Trace("FAILED.");
				throw;
			}
			GameDatabase.Trace("OK.");
		}
		public void ClearStratModCache()
		{
			GameDatabase._cachedStratMods.Clear();
		}
		public void Dispose()
		{
			this.db.Dispose();
		}
		public IntPtr GetDbPointer()
		{
			return this.db.GetDbPointer();
		}
		public void ReplaceMapWithExtremeStars()
		{
			string text = new StellarClass(Kerberos.Sots.StellarType.O, 0, Kerberos.Sots.StellarSize.VII).ToString();
			string text2 = new StellarClass(Kerberos.Sots.StellarType.M, 0, Kerberos.Sots.StellarSize.Ia).ToString();
			Random random = new Random();
			List<int> list = this.GetStarSystemIDs().ToList<int>();
			this._dom.star_systems.Clear();
			foreach (int current in list)
			{
				string value = random.CoinToss(50) ? text : text2;
				this.db.ExecuteNonQuery(string.Format(Queries.UpdateSystemStellarClass, current.ToSQLiteValue(), value.ToSQLiteValue()), false, true);
			}
		}
		private void UpdateAIIntelColony(int playerID, int planetID, int owningPlayerID, int colonyID, double imperialPopulation)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAIIntelColony, new object[]
			{
				playerID.ToSQLiteValue(),
				owningPlayerID.ToSQLiteValue(),
				planetID.ToSQLiteValue(),
				colonyID.ToSQLiteValue(),
				this.GetTurnCount().ToSQLiteValue(),
				imperialPopulation.ToSQLiteValue()
			}), true, true);
		}
		private void UpdateAIIntelPlanet(int playerID, int planetID, int biosphere, int resources, float infrastructure, float suitability)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAIIntelPlanet, new object[]
			{
				playerID.ToSQLiteValue(),
				planetID.ToSQLiteValue(),
				this.GetTurnCount().ToSQLiteValue(),
				biosphere.ToSQLiteValue(),
				resources.ToSQLiteValue(),
				infrastructure.ToSQLiteValue(),
				suitability.ToSQLiteValue()
			}), true, true);
		}
		public void PurgeOwnedColonyIntel(int playerID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.PurgeOwnedColonyIntel, playerID.ToSQLiteValue()), false, true);
		}
		public void ShareSensorData(int receivingPlayer, int sourcePlayer)
		{
			IEnumerable<int> starSystemIDs = this.GetStarSystemIDs();
			foreach (int current in starSystemIDs)
			{
				ExploreRecordInfo exploreRecord = this.GetExploreRecord(current, sourcePlayer);
				if (exploreRecord != null)
				{
					ExploreRecordInfo exploreRecord2 = this.GetExploreRecord(current, receivingPlayer);
					if (exploreRecord2 == null)
					{
						this.InsertExploreRecord(current, receivingPlayer, exploreRecord.LastTurnExplored, exploreRecord.Visible, exploreRecord.Explored);
					}
					else
					{
						exploreRecord2.LastTurnExplored = Math.Max(exploreRecord.LastTurnExplored, exploreRecord2.LastTurnExplored);
						exploreRecord2.Visible = (exploreRecord.Visible || exploreRecord2.Visible);
						exploreRecord2.Explored = (exploreRecord.Explored || exploreRecord2.Explored);
						this.UpdateExploreRecord(exploreRecord2);
					}
				}
			}
			IEnumerable<AIColonyIntel> colonyIntelsForPlayer = this.GetColonyIntelsForPlayer(sourcePlayer);
			foreach (AIColonyIntel current2 in colonyIntelsForPlayer)
			{
				if (current2.ColonyID.HasValue)
				{
					AIColonyIntel colonyIntelForPlanet = this.GetColonyIntelForPlanet(receivingPlayer, current2.PlanetID);
					if (colonyIntelForPlanet == null || colonyIntelForPlanet.LastSeen < current2.LastSeen)
					{
						this.UpdateAIIntelColony(receivingPlayer, current2.PlanetID, current2.OwningPlayerID, current2.ColonyID.Value, current2.ImperialPopulation);
					}
				}
			}
			IEnumerable<AIPlanetIntel> planetIntelsForPlayer = this.GetPlanetIntelsForPlayer(sourcePlayer);
			foreach (AIPlanetIntel current3 in planetIntelsForPlayer)
			{
				AIPlanetIntel planetIntel = this.GetPlanetIntel(receivingPlayer, current3.PlanetID);
				if (planetIntel == null || planetIntel.LastSeen < current3.LastSeen)
				{
					this.UpdateAIIntelPlanet(receivingPlayer, current3.PlanetID, current3.Biosphere, current3.Resources, current3.Infrastructure, current3.Suitability);
				}
			}
		}
		public void UpdatePlayerViewWithStarSystem(int playerID, int systemID)
		{
			List<PlanetInfo> list = this.GetStarSystemPlanetInfos(systemID).ToList<PlanetInfo>();
			foreach (PlanetInfo current in list)
			{
				this.db.ExecuteNonQuery(string.Format(Queries.RemoveNullAIColonyIntel, playerID.ToSQLiteValue(), current.ID.ToSQLiteValue()), true, true);
			}
			List<ColonyInfo> list2 = this.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>();
			foreach (ColonyInfo current2 in list2)
			{
				this.UpdateAIIntelColony(playerID, current2.OrbitalObjectID, current2.PlayerID, current2.ID, current2.ImperialPop);
			}
			foreach (PlanetInfo current3 in list)
			{
				this.UpdateAIIntelPlanet(playerID, current3.ID, current3.Biosphere, current3.Resources, current3.Infrastructure, current3.Suitability);
			}
		}
		public void LogComment(string comment)
		{
			this.db.LogComment(comment);
		}
		public List<string> GetUniqueGenerators()
		{
			List<string> list = new List<string>();
			Table table = this.db.ExecuteTableQuery(Queries.GetUniqueGenerators, false);
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				list.Add(row[0]);
			}
			return list;
		}
		private List<string> GetDatabaseHistoryCore(int? turn, out int lastId, int? id = null)
		{
			if (!turn.HasValue && id.HasValue)
			{
				throw new ArgumentException("No query supports ID without a Turn.", "turn");
			}
			if (id.HasValue)
			{
				lastId = id.Value;
			}
			else
			{
				lastId = 0;
			}
			Table table;
			if (turn.HasValue)
			{
				if (id.HasValue)
				{
					table = this.db.ExecuteTableQuery(string.Format(Queries.GetDatabaseHistoryByTurnAndId, turn, id), true);
				}
				else
				{
					table = this.db.ExecuteTableQuery(string.Format(Queries.GetDatabaseHistoryByTurn, turn), true);
				}
			}
			else
			{
				table = this.db.ExecuteTableQuery(Queries.GetDatabaseHistory, true);
			}
			List<string> list = new List<string>();
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				string item = row[2];
				list.Add(item);
			}
			if (table.Rows.Length > 0)
			{
				lastId = table.Rows[table.Rows.Length - 1][0].SQLiteValueToInteger();
			}
			return list;
		}
		public List<string> GetDatabaseHistory(out int lastId)
		{
			return this.GetDatabaseHistoryCore(null, out lastId, null);
		}
		public List<string> GetDatabaseHistoryForTurn(int turn, out int lastId, int? id = null)
		{
			return this.GetDatabaseHistoryCore(new int?(turn), out lastId, id);
		}
		public void InsertTurnOne()
		{
			this.UpdateNameValuePair("turn", 1.ToString());
		}
		public bool HasEndOfFleshExpansion()
		{
			return true;
		}
		public void SetClientId(int id)
		{
			this._clientId = id;
			this.db.LogComment("Setting client ID to: " + id);
			this.db.ExecuteNonQuery(string.Format(Queries.SetClientID, id), true, true);
		}
		public int GetClientId()
		{
			return this._clientId;
		}
		public int InsertWeapon(LogicalWeapon weapon, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertWeapon, playerId, weapon.FileName));
		}
		public IEnumerable<LogicalWeapon> GetAvailableWeapons(AssetDatabase assetdb, int playerId)
		{
			Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerWeapons, playerId), true);
			return 
				from x in assetdb.Weapons
				where t.Rows.Any((Row y) => y[0] == x.FileName)
				select x;
		}
		public void RemoveWeapon(int? weaponId)
		{
			if (weaponId.HasValue)
			{
				this.db.ExecuteNonQuery(string.Format(Queries.RemoveWeapon, weaponId), false, true);
			}
		}
		public IEnumerable<LogicalModule> GetAvailableModules(AssetDatabase assetdb, int playerId)
		{
			Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerModules, playerId), true);
			return 
				from x in assetdb.Modules
				where t.Rows.Any((Row y) => y[0] == x.ModulePath)
				select x;
		}
		public int InsertModule(LogicalModule module, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertModule, module.ModulePath, playerId));
		}
		public void RemoveModule(int moduleId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveModule, moduleId), false, true);
		}
		public int InsertSpecialProject(int playerId, string name, int cost, SpecialProjectType type, int techid = 0, int encounterid = 0, int fleetid = 0, int targetplayerid = 0)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertSpecialProject, new object[]
			{
				playerId.ToOneBasedSQLiteValue(),
				name.ToSQLiteValue(),
				cost.ToSQLiteValue(),
				((int)type).ToSQLiteValue(),
				techid.ToSQLiteValue(),
				encounterid.ToOneBasedSQLiteValue(),
				fleetid.ToOneBasedSQLiteValue(),
				targetplayerid.ToOneBasedSQLiteValue()
			}));
		}
		public void RemoveSpecialProject(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSpecialProject, id), false, true);
		}
		public SpecialProjectInfo GetSpecialProjectInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSpecialProjectInfo, id), true);
			if (table.Count<Row>() > 0)
			{
				return this.ParseSpecialProjectInfo(table[0]);
			}
			return null;
		}
		public bool GetHasPlayerStudiedIndependentRace(int Playerid, int IndependentRacePlayerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetCompleteSpecialProjectsByPlayerID, Playerid), true))
			{
				SpecialProjectInfo specialProjectInfo = this.ParseSpecialProjectInfo(current);
				if (specialProjectInfo.Type == SpecialProjectType.IndependentStudy && specialProjectInfo.TargetPlayerID == IndependentRacePlayerID)
				{
					return true;
				}
			}
			return false;
		}
		public bool GetHasPlayerStudyingIndependentRace(int Playerid, int IndependentRacePlayerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetSpecialProjectInfosByPlayerID, Playerid), true))
			{
				SpecialProjectInfo specialProjectInfo = this.ParseSpecialProjectInfo(current);
				if (specialProjectInfo.Type == SpecialProjectType.IndependentStudy && specialProjectInfo.TargetPlayerID == IndependentRacePlayerID)
				{
					return true;
				}
			}
			return false;
		}
		public bool GetHasPlayerStudiedSpecialProject(int Playerid, SpecialProjectType type)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetCompleteSpecialProjectsByPlayerID, Playerid), true))
			{
				SpecialProjectInfo specialProjectInfo = this.ParseSpecialProjectInfo(current);
				if (specialProjectInfo.Type == type)
				{
					return true;
				}
			}
			return false;
		}
		public bool GetHasPlayerStudyingSpecialProject(int Playerid, SpecialProjectType type)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetSpecialProjectInfosByPlayerID, Playerid), true))
			{
				SpecialProjectInfo specialProjectInfo = this.ParseSpecialProjectInfo(current);
				if (specialProjectInfo.Type == type)
				{
					return true;
				}
			}
			return false;
		}
		public IEnumerable<SpecialProjectInfo> GetSpecialProjectInfosByPlayerID(int playerid, bool onlyIncomplete)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(onlyIncomplete ? Queries.GetIncompleteSpecialProjectInfosByPlayerID : Queries.GetSpecialProjectInfosByPlayerID, playerid), true))
			{
				SpecialProjectInfo specialProjectInfo = this.ParseSpecialProjectInfo(current);
				yield return specialProjectInfo;
			}
			yield break;
		}
		public void UpdateSpecialProjectProgress(int id, int progress)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSpecialProjectProgress, id, progress), false, true);
		}
		public void UpdateSpecialProjectRate(int id, float rate)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSpecialProjectRate, id, rate.ToSQLiteValue()), false, true);
		}
		public void UpdateStarSystemVisible(int id, bool isVisible)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateStarSystemVisible, id.ToSQLiteValue(), isVisible.ToSQLiteValue()), false, true);
		}
		public void UpdateStarSystemOpen(int id, bool isOpen)
		{
			this._dom.star_systems.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateStarSystemOpen, id.ToSQLiteValue(), isOpen.ToSQLiteValue()), false, true);
		}
		public int InsertStarSystem(int? id, string name, int? provinceId, string stellarClass, Vector3 origin, bool isVisible, bool isOpen = true, int? terrainID = null)
		{
			this._dom.star_systems.Clear();
			int value;
			if (id.HasValue)
			{
				value = this.db.ExecuteIntegerQuery(string.Format(Queries.InsertStellarInfo, id.Value.ToSQLiteValue(), origin.ToSQLiteValue()));
			}
			else
			{
				value = this.db.ExecuteIntegerQuery(string.Format(Queries.InsertNewStellarInfo, origin.ToSQLiteValue()));
			}
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertStarSystem, new object[]
			{
				value.ToSQLiteValue(),
				name.ToSQLiteValue(),
				provinceId.ToNullableSQLiteValue(),
				stellarClass.ToSQLiteValue(),
				isVisible.ToSQLiteValue(),
				terrainID.ToNullableSQLiteValue(),
				isOpen.ToSQLiteValue()
			}));
		}
		public int InsertAsteroidBelt(int? parentOrbitalObjectId, int starSystemId, OrbitalPath path, string name, int randomSeed)
		{
			path.VerifyFinite();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertAsteroidBelt, new object[]
			{
				parentOrbitalObjectId.ToNullableSQLiteValue(),
				starSystemId.ToSQLiteValue(),
				path.ToString().ToSQLiteValue(),
				name.ToNullableSQLiteValue(),
				randomSeed.ToSQLiteValue()
			}));
		}
		public int InsertLargeAsteroid(int parentOrbitalObjectId, int starSystemId, OrbitalPath path, string name, int resources)
		{
			path.VerifyFinite();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertLargeAsteroid, new object[]
			{
				parentOrbitalObjectId.ToSQLiteValue(),
				starSystemId.ToSQLiteValue(),
				path.ToString().ToSQLiteValue(),
				name.ToNullableSQLiteValue(),
				resources.ToSQLiteValue()
			}));
		}
		public void UpdateLargeAsteroidInfo(LargeAsteroidInfo lai)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLargeAsteroidInfo, lai.ID.ToSQLiteValue(), lai.Resources.ToSQLiteValue()), false, true);
		}
		public void ExecuteDatabaseHistory(IDictionary<int, List<string>> history)
		{
			this._dom.Clear();
			GameDatabase._cachedStratMods.Clear();
			foreach (KeyValuePair<int, List<string>> current in history)
			{
				this.SetClientId(current.Key);
				GameDatabase.Trace(string.Format("Executing {0} lines of DB history as {1}.", current.Value.Count, this.GetClientId()));
				foreach (string current2 in current.Value)
				{
					this.db.ExecuteNonQuery(current2, false, true);
				}
			}
		}
		public int InsertPlanet(int? parentOrbitalObjectId, int starSystemId, OrbitalPath path, string name, string type, int? ringId, float suitability, int biosphere, int resources, float size)
		{
			path.VerifyFinite();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertPlanet, new object[]
			{
				parentOrbitalObjectId.ToNullableSQLiteValue(),
				starSystemId.ToSQLiteValue(),
				path.ToString().ToSQLiteValue(),
				name.ToNullableSQLiteValue(),
				type.ToSQLiteValue(),
				ringId.ToNullableSQLiteValue(),
				suitability.ToSQLiteValue(),
				biosphere.ToSQLiteValue(),
				resources.ToSQLiteValue(),
				size.ToSQLiteValue()
			}));
		}
		public int InsertOrbitalObject(int? parentOrbitalObjectId, int starSystemId, OrbitalPath path, string name)
		{
			path.VerifyFinite();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertOrbitalObject, new object[]
			{
				parentOrbitalObjectId.ToNullableSQLiteValue(),
				starSystemId.ToSQLiteValue(),
				path.ToString().ToSQLiteValue(),
				name.ToNullableSQLiteValue()
			}));
		}
		public int InsertColony(int orbitalObjectID, int playerID, double impPop, float civWeight, int turn, float infrastructure, bool paintsystem = true)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalObjectID);
			if (infrastructure > 0f)
			{
				this.UpdatePlanetInfrastructure(orbitalObjectID, infrastructure);
			}
			if (paintsystem)
			{
				Kerberos.Sots.GameStates.StarSystem.PaintSystemPlayerColor(this, this.GetOrbitalObjectInfo(orbitalObjectID).StarSystemID, playerID);
			}
			int result = this._dom.colonies.Insert(null, new ColonyInfo
			{
				OrbitalObjectID = orbitalObjectID,
				PlayerID = playerID,
				ImperialPop = impPop,
				CivilianWeight = civWeight,
				TurnEstablished = turn
			});
			if (!this.GetReserveFleetID(playerID, orbitalObjectInfo.StarSystemID).HasValue)
			{
				this.InsertReserveFleet(playerID, orbitalObjectInfo.StarSystemID);
			}
			ColonyTrapInfo colonyTrapInfoByPlanetID = this.GetColonyTrapInfoByPlanetID(orbitalObjectID);
			if (colonyTrapInfoByPlanetID != null)
			{
				this.RemoveColonyTrapInfo(colonyTrapInfoByPlanetID.ID);
			}
			return result;
		}
		public int InsertStation(int parentOrbitalObjectID, int starSystemID, OrbitalPath path, string name, int playerID, DesignInfo design)
		{
			path.VerifyFinite();
			int shipID = this.InsertShip(0, design.ID, design.Name, (ShipParams)0, null, 0);
			int result = this._dom.stations.Insert(null, new StationInfo
			{
				DesignInfo = design,
				OrbitalObjectInfo = new OrbitalObjectInfo
				{
					Name = name,
					OrbitalPath = path,
					ParentID = new int?(parentOrbitalObjectID),
					StarSystemID = starSystemID
				},
				ShipID = shipID,
				PlayerID = playerID
			});
			if (design.StationType == StationType.NAVAL)
			{
				this.InsertOrGetReserveFleetID(starSystemID, playerID);
			}
			return result;
		}
		public void UpdateSystemCombatZones(int systemID, List<int> zones)
		{
			this._dom.star_systems.Clear();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < zones.Count; i++)
			{
				stringBuilder.Append(zones[i]);
				if (i != zones.Count - 1)
				{
					stringBuilder.Append(",");
				}
			}
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSystemCombatZones, stringBuilder.ToString(), systemID), false, true);
		}
		public void UpdateStation(StationInfo station)
		{
			this.UpdateDesign(station.DesignInfo);
			this.UpdateDesignSection(station.DesignInfo.DesignSections[0]);
			this._dom.stations.Update(station.OrbitalObjectInfo.ID, station);
		}
		public void UpdateDesign(DesignInfo design)
		{
			this._dom.designs.Update(design.ID, design);
			this._dom.CachedPlayerDesignNames.Clear();
		}
		public void RemoveDesign(int id)
		{
			this._dom.designs.Remove(id);
			this._dom.CachedPlayerDesignNames.Clear();
		}
		public void UpdateDesignSection(DesignSectionInfo dsi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateDesignSection, dsi.ID, dsi.FilePath), false, true);
			this._dom.designs.Sync(dsi.DesignInfo.ID);
		}
		private void InsertNewShipSectionInstances(DesignInfo designInfo, int? shipId, int? stationId)
		{
			DesignSectionInfo[] designSections = designInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				ShipSectionAsset shipSectionAsset = this.assetdb.GetShipSectionAsset(designSectionInfo.FilePath);
				List<string> list = new List<string>();
				if (designSectionInfo.Techs.Count > 0)
				{
					foreach (int current in designSectionInfo.Techs)
					{
						list.Add(this.GetTechFileID(current));
					}
				}
				int supplyWithTech = Ship.GetSupplyWithTech(this.assetdb, list, shipSectionAsset.Supply);
				int structureWithTech = Ship.GetStructureWithTech(this.assetdb, list, shipSectionAsset.Structure);
				int num = this.InsertSectionInstance(designSectionInfo.ID, shipId, stationId, structureWithTech, (float)supplyWithTech, shipSectionAsset.Crew, shipSectionAsset.Signature, shipSectionAsset.RepairPoints);
				this.InsertNewArmorInstances(shipSectionAsset, this.GetDesignAttributesForDesign(designInfo.ID).ToList<SectionEnumerations.DesignAttribute>(), num);
				this.InsertNewShipWeaponInstancesForSection(shipSectionAsset.Mounts.ToList<LogicalMount>(), designSectionInfo.WeaponBanks.ToList<WeaponBankInfo>(), num);
				if (designSectionInfo.Modules != null)
				{
					this.InsertNewShipModuleInstances(shipSectionAsset, designSectionInfo.Modules, num);
				}
			}
		}
		private void InsertNewArmorInstances(ShipSectionAsset sectionAsset, List<SectionEnumerations.DesignAttribute> attributes, int sectionId)
		{
			Dictionary<ArmorSide, DamagePattern> dictionary = new Dictionary<ArmorSide, DamagePattern>();
			int num = 0;
			Kerberos.Sots.Framework.Size[] armor = sectionAsset.Armor;
			for (int i = 0; i < armor.Length; i++)
			{
				Kerberos.Sots.Framework.Size arg_21_0 = armor[i];
				DamagePattern damagePattern = sectionAsset.CreateFreshArmor((ArmorSide)num, Ship.CalcArmorWidthModifier(attributes, 0));
				if (damagePattern.Height != 0 && damagePattern.Width != 0)
				{
					dictionary[(ArmorSide)num] = damagePattern;
					num++;
				}
			}
			this._dom.armor_instances.Insert(new int?(sectionId), dictionary);
		}
		private void InsertNewShipWeaponInstancesForSection(List<LogicalMount> mounts, List<WeaponBankInfo> banks, int sectionInstId)
		{
			int num = 0;
			foreach (LogicalMount mount in mounts)
			{
				if (!WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
				{
					float num2 = Ship.GetTurretHealth(mount.Bank.TurretSize);
					WeaponBankInfo weaponBankInfo = banks.FirstOrDefault((WeaponBankInfo x) => x.BankGUID == mount.Bank.GUID);
					if (weaponBankInfo != null && weaponBankInfo.WeaponID.HasValue)
					{
						string weapon = this.GetWeaponAsset(weaponBankInfo.WeaponID.Value);
						LogicalWeapon logicalWeapon = this.assetdb.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weapon);
						if (logicalWeapon != null)
						{
							num2 += logicalWeapon.Health;
						}
					}
					this.InsertWeaponInstance(mount, sectionInstId, null, num, num2, mount.NodeName);
					num++;
				}
			}
		}
		private void InsertNewShipWeaponInstancesForModule(List<LogicalMount> mounts, int? weaponId, int sectionInstId, int moduleInstId)
		{
			string weapon = weaponId.HasValue ? this.GetWeaponAsset(weaponId.Value) : "";
			LogicalWeapon logicalWeapon = (!string.IsNullOrEmpty(weapon)) ? this.assetdb.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weapon) : null;
			int num = 0;
			foreach (LogicalMount current in mounts)
			{
				if (!WeaponEnums.IsBattleRider(current.Bank.TurretClass))
				{
					float num2 = Ship.GetTurretHealth(current.Bank.TurretSize);
					if (logicalWeapon != null)
					{
						num2 += logicalWeapon.Health;
					}
					this.InsertWeaponInstance(current, sectionInstId, new int?(moduleInstId), num, num2, current.NodeName);
					num++;
				}
			}
		}
		private void InsertNewShipModuleInstances(ShipSectionAsset sectionAsset, List<DesignModuleInfo> modules, int sectionId)
		{
			foreach (DesignModuleInfo module in modules)
			{
				string mPath = this.GetModuleAsset(module.ModuleID);
				LogicalModule logicalModule = this.assetdb.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == mPath);
				int moduleInstId = this.InsertModuleInstance(sectionAsset.Modules.First((LogicalModuleMount x) => x.NodeName == module.MountNodeName), logicalModule, sectionId);
				if (logicalModule.Mounts.Count<LogicalMount>() > 0)
				{
					this.InsertNewShipWeaponInstancesForModule(logicalModule.Mounts.ToList<LogicalMount>(), module.WeaponID, sectionId, moduleInstId);
				}
			}
		}
		private Dictionary<string, int> GetShipNamesByPlayer(int playerId)
		{
			Dictionary<string, int> dictionary;
			if (!this._dom.CachedPlayerShipNames.TryGetValue(playerId, out dictionary))
			{
				dictionary = new Dictionary<string, int>();
				this._dom.CachedPlayerShipNames[playerId] = dictionary;
			}
			return dictionary;
		}
		private List<string> GetFleetNamesByPlayer(int playerId)
		{
			List<FleetInfo> source = this.GetFleetInfosByPlayerID(playerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			return (
				from x in source
				select x.Name).ToList<string>();
		}
		public string ResolveNewFleetName(App App, int playerId, string name)
		{
			bool flag = App.Game.NamesPool.GetFleetNamesForFaction(App.GameDatabase.GetFactionName(App.GameDatabase.GetPlayerFactionID(playerId))).Any((string x) => x.Contains(name));
			string text = name;
			List<string> fleetNamesByPlayer = this.GetFleetNamesByPlayer(playerId);
			if (fleetNamesByPlayer.Any((string x) => x.StartsWith(name)) || flag)
			{
				int num = 0;
				foreach (string current in 
					from x in fleetNamesByPlayer
					where x.StartsWith(name)
					select x)
				{
					int num2 = 0;
					string[] array = current.Split(new char[]
					{
						' '
					});
					int.TryParse(array[array.Length - 1], out num2);
					num = ((num2 > num) ? num2 : num);
				}
				if (num == 0)
				{
					if ((
						from x in fleetNamesByPlayer
						where x.StartsWith(name)
						select x).Any<string>())
					{
						num = 1;
					}
				}
				text = string.Format("{0} {1}", text, (num + (flag ? 1 : 0)).ToString());
			}
			return text;
		}
		public string ResolveNewShipName(int playerId, string name)
		{
			string text = name;
			Dictionary<string, int> shipNamesByPlayer = this.GetShipNamesByPlayer(playerId);
			int num = shipNamesByPlayer.Keys.Count((string x) => x.Contains(name));
			if (num > 0)
			{
				text = string.Format("{0} - {1}", text, num.ToString());
			}
			return text;
		}
		public void TransferShipToPlayer(ShipInfo ship, int newPlayerId)
		{
			List<SectionInstanceInfo> list = this.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
			Dictionary<SectionInstanceInfo, string> dictionary = new Dictionary<SectionInstanceInfo, string>();
			DesignInfo designInfo = ship.DesignInfo;
			foreach (SectionInstanceInfo sii in list)
			{
				DesignSectionInfo designSectionInfo = designInfo.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ID == sii.SectionID);
				dictionary.Add(sii, designSectionInfo.FilePath);
			}
			designInfo.PlayerID = newPlayerId;
			int num = this.InsertDesignByDesignInfo(designInfo);
			foreach (KeyValuePair<SectionInstanceInfo, string> kvp in dictionary)
			{
				DesignSectionInfo designSectionInfo2 = designInfo.DesignSections.First(delegate(DesignSectionInfo x)
				{
					string arg_14_0 = x.FilePath;
					KeyValuePair<SectionInstanceInfo, string> kvp3 = kvp;
					return arg_14_0 == kvp3.Value;
				});
				SQLiteConnection arg_11D_0 = this.db;
				string arg_116_0 = Queries.ChangeSectionInstanceSectionId;
				KeyValuePair<SectionInstanceInfo, string> kvp4 = kvp;
				arg_11D_0.ExecuteNonQuery(string.Format(arg_116_0, kvp4.Key.ID, designSectionInfo2.ID), false, true);
				RowCache<int, SectionInstanceInfo> arg_142_0 = this._dom.section_instances;
				KeyValuePair<SectionInstanceInfo, string> kvp2 = kvp;
				arg_142_0.Sync(kvp2.Key.ID);
			}
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipDesign, ship.ID, num), false, true);
			this._dom.ships.Sync(ship.ID);
		}
		public int InsertShip(int fleetID, int designID, string shipName = null, ShipParams parms = (ShipParams)0, int? aiFleetID = null, int Loacubes = 0)
		{
			DesignInfo designInfo = this.GetDesignInfo(designID);
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo != null && designInfo.PlayerID != fleetInfo.PlayerID)
			{
				throw new InvalidOperationException(string.Format("Mismatched design and fleet players (designID={0},design playerID={1},fleet playerID={2}).", designID, designInfo.PlayerID, fleetInfo.PlayerID));
			}
			bool flag = false;
			int num = 0;
			DesignSectionInfo[] designSections = designInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				ShipSectionAsset shipSectionAsset = this.assetdb.GetShipSectionAsset(designSectionInfo.FilePath);
				num += (int)shipSectionAsset.PsionicPowerLevel;
				flag = (flag || shipSectionAsset.IsSuulka);
			}
			if (flag && shipName == null)
			{
				shipName = designInfo.Name;
			}
			if (!flag)
			{
				int playerFactionID = this.GetPlayerFactionID(designInfo.PlayerID);
				Faction faction = this.assetdb.GetFaction(playerFactionID);
				if (faction != null)
				{
					num = (int)((float)designInfo.CrewAvailable * faction.PsionicPowerPerCrew);
				}
			}
			this.AddNumShipsBuiltFromDesign(designID, 1);
			int numShipsBuiltFromDesign = this.GetNumShipsBuiltFromDesign(designID);
			if (!flag && (shipName == null || shipName == designInfo.Name))
			{
				shipName = this.GetDefaultShipName(designID, numShipsBuiltFromDesign);
			}
			shipName = this.ResolveNewShipName(designInfo.PlayerID, shipName);
			int turnCount = this.GetTurnCount();
			int parentID = 0;
			int num2 = this._dom.ships.Insert(null, new ShipInfo
			{
				FleetID = fleetID,
				DesignID = designID,
				DesignInfo = designInfo,
				ParentID = parentID,
				ShipName = shipName,
				SerialNumber = numShipsBuiltFromDesign,
				Params = parms,
				RiderIndex = -1,
				PsionicPower = num,
				AIFleetID = aiFleetID,
				ComissionDate = turnCount,
				LoaCubes = Loacubes
			});
			this.TryAddFleetShip(num2, fleetID);
			this.InsertNewShipSectionInstances(designInfo, new int?(num2), null);
			this.AddCachedShipNameReference(designInfo.PlayerID, shipName);
			return num2;
		}
		public int InsertDesignByDesignInfo(DesignInfo design)
		{
			DesignLab.SummarizeDesign(this.AssetDatabase, this, design);
			int result = this._dom.designs.Insert(null, design);
			HashSet<string> hashSet;
			if (!this._dom.CachedPlayerDesignNames.TryGetValue(design.PlayerID, out hashSet))
			{
				hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
				this._dom.CachedPlayerDesignNames.Add(design.PlayerID, hashSet);
			}
			hashSet.Add(design.Name);
			return result;
		}
		public void UpdateDesignAttributeDiscovered(int id, bool isAttributeDiscovered)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateDesignAttributesDiscovered, id.ToSQLiteValue(), isAttributeDiscovered.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(id);
		}
		public void UpdateDesignPrototype(int id, bool isPrototyped)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateDesignPrototype, id.ToSQLiteValue(), isPrototyped.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(id);
		}
		public void InsertDesignAttribute(int id, SectionEnumerations.DesignAttribute da)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertDesignAttribute, id.ToSQLiteValue(), ((int)da).ToSQLiteValue()), false, true);
		}
		public IEnumerable<SectionEnumerations.DesignAttribute> GetDesignAttributesForDesign(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetDesignAttributesForDesign, id), true);
			foreach (Row current in table)
			{
				yield return (SectionEnumerations.DesignAttribute)int.Parse(current[0]);
			}
			yield break;
		}
		public IEnumerable<string> GetGetAllPlayerSectionIds(int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerShipSections, playerId), true);
			foreach (Row current in table)
			{
				yield return current[0];
			}
			yield break;
		}
		public int InsertSectionAsset(string filepath, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertSection, filepath, playerId));
		}
		private int InsertSectionInstance(int sectionID, int? shipID, int? stationID, int structure, float supply, int crew, float signature, int repairPoints)
		{
			SectionInstanceInfo value = new SectionInstanceInfo
			{
				SectionID = sectionID,
				ShipID = shipID,
				StationID = stationID,
				Structure = structure,
				Supply = (int)supply,
				Crew = crew,
				Signature = signature,
				RepairPoints = repairPoints
			};
			return this._dom.section_instances.Insert(null, value);
		}
		private void RemoveSectionInstance(int sectioninstanceID)
		{
			this._dom.section_instances.Remove(sectioninstanceID);
		}
		public int InsertWeaponInstance(LogicalMount mount, int sectionId, int? moduleId, int index, float structure, string nodeName)
		{
			return this._dom.weapon_instances.Insert(null, new WeaponInstanceInfo
			{
				SectionInstanceID = sectionId,
				ModuleInstanceID = moduleId,
				Structure = structure,
				MaxStructure = structure,
				WeaponID = index,
				NodeName = nodeName
			});
		}
		public int InsertModuleInstance(LogicalModuleMount mount, LogicalModule module, int sectionId)
		{
			return this._dom.module_instances.Insert(null, new ModuleInstanceInfo
			{
				SectionInstanceID = sectionId,
				RepairPoints = module.RepairPointsBonus,
				Structure = (int)module.Structure,
				ModuleNodeID = mount.NodeName
			});
		}
		public void RemovePlayerDesign(int designId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemovePlayerDesign, designId.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(designId);
			this._dom.CachedPlayerDesignNames.Clear();
		}
		public void RemoveSection(int sectionId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSection, sectionId), false, true);
		}
		public void SetLastTurnWithCombat(int turn)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetLastTurnWithCombat, turn), false, true);
		}
		public int GetLastTurnWithCombat()
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetLastTurnWithCombat, new object[0]));
		}
		public int InsertDesignModule(DesignModuleInfo value)
		{
			int result = DesignsCache.InsertDesignModuleInfo(this.db, value);
			this._dom.designs.Sync(value.DesignSectionInfo.DesignInfo.ID);
			return result;
		}
		public void UpdateQueuedModuleNodeName(DesignModuleInfo module)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateQueuedModuleNodeName, module.ID.ToSQLiteValue(), module.MountNodeName.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(module.DesignSectionInfo.DesignInfo.ID);
		}
		public void UpdateDesignModuleNodeName(DesignModuleInfo module)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateDesignModuleNodeName, module.ID.ToSQLiteValue(), module.MountNodeName.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(module.DesignSectionInfo.DesignInfo.ID);
		}
		public void RemoveDesignModule(DesignModuleInfo module)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveDesignModule, module.ID.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(module.DesignSectionInfo.DesignInfo.ID);
		}
		public bool canBuildDesignOrder(DesignInfo di, int SystemId, out bool requiresPrototype)
		{
			requiresPrototype = false;
			if (di.isPrototyped)
			{
				return true;
			}
			List<BuildOrderInfo> list = this.GetDesignBuildOrders(di).ToList<BuildOrderInfo>();
			if (list.Count > 0)
			{
				List<BuildOrderInfo> source = this.GetBuildOrdersForSystem(SystemId).ToList<BuildOrderInfo>();
				return source.Any((BuildOrderInfo x) => x.DesignID == di.ID);
			}
			requiresPrototype = true;
			return true;
		}
		public IEnumerable<BuildOrderInfo> GetDesignBuildOrders(DesignInfo di)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetDesignBuildOrders, di.ID), true);
			foreach (Row current in table)
			{
				yield return this.GetBuildOrderInfo(current);
			}
			yield break;
		}
		public int InsertInvoiceBuildOrder(int invoiceId, int designID, string shipName, int loacubes = 0)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertInvoiceBuildOrder, new object[]
			{
				invoiceId.ToSQLiteValue(),
				designID.ToSQLiteValue(),
				shipName.ToSQLiteValue(),
				loacubes.ToSQLiteValue()
			}));
		}
		private int InsertBuildOrderCore(int systemId, int designId, int progress, int priority, int? missionId, string name, int productionCost, int? invoiceInstanceId, int? aiFleetId, int LoaCubes)
		{
			if (missionId.HasValue)
			{
				MissionInfo missionInfo = this.GetMissionInfo(missionId.Value);
				if (missionInfo.FleetID != 0)
				{
					FleetInfo fleetInfo = this.GetFleetInfo(missionInfo.FleetID);
					DesignInfo designInfo = this.GetDesignInfo(designId);
					if (fleetInfo.PlayerID != designInfo.PlayerID)
					{
						throw new ArgumentException(string.Format("Tried inserting a build order belonging to player {0} into a fleet ({1}) belonging to player {1}.", designInfo.PlayerID, fleetInfo.ID, fleetInfo.PlayerID));
					}
				}
			}
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertBuildOrder, new object[]
			{
				systemId.ToSQLiteValue(),
				designId.ToSQLiteValue(),
				progress.ToSQLiteValue(),
				priority.ToSQLiteValue(),
				missionId.ToNullableSQLiteValue(),
				name.ToSQLiteValue(),
				productionCost.ToSQLiteValue(),
				invoiceInstanceId.ToNullableSQLiteValue(),
				aiFleetId.ToNullableSQLiteValue(),
				LoaCubes.ToSQLiteValue()
			}));
		}
		public int InsertBuildOrder(int systemID, int designID, int progress, int priority, string shipName, int productionTarget, int? invoiceInstanceId = null, int? aiFleetID = null, int LoaCubes = 0)
		{
			if (priority == 0)
			{
				foreach (BuildOrderInfo current in this.GetBuildOrdersForSystem(systemID))
				{
					if (current.Priority > priority)
					{
						priority = current.Priority;
					}
				}
				priority++;
			}
			return this.InsertBuildOrderCore(systemID, designID, progress, priority, null, shipName, productionTarget, invoiceInstanceId, aiFleetID, LoaCubes);
		}
		public void InsertBuildOrders(int systemId, IEnumerable<int> designIDs, int priorityOrder, int missionID, int? invoiceInstanceID = null, int? aiFleetID = null)
		{
			if (designIDs.Count<int>() < 1)
			{
				return;
			}
			IEnumerable<BuildOrderInfo> buildOrdersForSystem = this.GetBuildOrdersForSystem(systemId);
			int num = 1;
			if (buildOrdersForSystem.Count<BuildOrderInfo>() < 1)
			{
				foreach (int current in designIDs)
				{
					DesignInfo designInfo = this.GetDesignInfo(current);
					this.InsertBuildOrderCore(systemId, current, 0, num, new int?(missionID), designInfo.Name, designInfo.ProductionCost, invoiceInstanceID, aiFleetID, 0);
					num++;
				}
				return;
			}
			num = 1;
			foreach (BuildOrderInfo current2 in buildOrdersForSystem)
			{
				if (num == priorityOrder)
				{
					foreach (int current3 in designIDs)
					{
						DesignInfo designInfo2 = this.GetDesignInfo(current3);
						this.InsertBuildOrderCore(systemId, current3, 0, num, new int?(missionID), designInfo2.Name, designInfo2.ProductionCost, invoiceInstanceID, aiFleetID, 0);
						num++;
					}
				}
				current2.Priority = num;
				this.UpdateBuildOrder(current2);
				num++;
			}
		}
		private int InsertRetrofitOrderCore(int systemId, int designId, int shipid, int? invoiceInstanceId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertRetrofitOrder, new object[]
			{
				systemId.ToSQLiteValue(),
				designId.ToSQLiteValue(),
				shipid.ToSQLiteValue(),
				invoiceInstanceId.ToNullableSQLiteValue()
			}));
		}
		public int InsertRetrofitOrder(int systemID, int designID, int shipID, int? invoiceInstanceId = null)
		{
			return this.InsertRetrofitOrderCore(systemID, designID, shipID, invoiceInstanceId);
		}
		private RetrofitOrderInfo ParseRetrofitOrderInfo(Row row)
		{
			if (row != null)
			{
				return new RetrofitOrderInfo
				{
					ID = row[0].SQLiteValueToInteger(),
					DesignID = row[1].SQLiteValueToInteger(),
					ShipID = row[2].SQLiteValueToInteger(),
					SystemID = row[3].SQLiteValueToInteger(),
					InvoiceID = row[4].SQLiteValueToNullableInteger()
				};
			}
			return null;
		}
		public RetrofitOrderInfo GetRetrofitOrderInfo(int retrofitOrderId)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetRetrofitOrderInfo, retrofitOrderId), true)[0];
			return this.ParseRetrofitOrderInfo(row);
		}
		public IEnumerable<RetrofitOrderInfo> GetRetrofitOrdersForInvoiceInstance(int invoiceInstanceId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetRetrofitOrdersForInvoice, invoiceInstanceId), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseRetrofitOrderInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<RetrofitOrderInfo> GetRetrofitOrdersForSystem(int systemId)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetRetrofitOrdersForSystem, systemId), true))
			{
				yield return this.ParseRetrofitOrderInfo(current);
			}
			yield break;
		}
		public void RemoveRetrofitOrder(int retrofitOrderID, bool destroyship = false, bool defenseasset = false)
		{
			RetrofitOrderInfo retrofitOrderInfo = this.GetRetrofitOrderInfo(retrofitOrderID);
			ShipInfo shipInfo = this.GetShipInfo(retrofitOrderInfo.ShipID, true);
			FleetInfo fleetInfo = this.GetFleetInfo(shipInfo.FleetID);
			if (destroyship)
			{
				this.RemoveShip(shipInfo.ID);
			}
			else
			{
				if (defenseasset)
				{
					this.TransferShip(shipInfo.ID, this.InsertOrGetDefenseFleetID(fleetInfo.SystemID, fleetInfo.PlayerID));
				}
				else
				{
					this.TransferShip(shipInfo.ID, this.InsertOrGetReserveFleetID(fleetInfo.SystemID, fleetInfo.PlayerID));
				}
			}
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveRetrofitOrder, retrofitOrderID), true);
		}
		public int InsertStationRetrofitOrder(int designid, int shipid)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertStationRetrofitOrder, designid.ToSQLiteValue(), shipid.ToSQLiteValue()));
		}
		public void RemoveStationRetrofitOrder(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveStationRetrofitOrder, id.ToSQLiteValue()), false, true);
		}
		public IEnumerable<StationRetrofitOrderInfo> GetStationRetrofitOrders()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetStationRetrofitOrders, new object[0]), true))
			{
				yield return this.ParseStationRetrofitOrderInfo(current);
			}
			yield break;
		}
		private StationRetrofitOrderInfo ParseStationRetrofitOrderInfo(Row row)
		{
			if (row != null)
			{
				return new StationRetrofitOrderInfo
				{
					ID = row[0].SQLiteValueToInteger(),
					DesignID = row[1].SQLiteValueToInteger(),
					ShipID = row[2].SQLiteValueToInteger()
				};
			}
			return null;
		}
		public void InsertPlayerTech(int playerId, string techId, TechStates state, double progress, double totalCost, int? turnResearched)
		{
			float value = 0f;
			float value2 = 0.01f;
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPlayerTech, new object[]
			{
				playerId.ToSQLiteValue(),
				techId.ToSQLiteValue(),
				((int)state).ToSQLiteValue(),
				progress.ToSQLiteValue(),
				totalCost.ToSQLiteValue(),
				value.ToSQLiteValue(),
				value2.ToSQLiteValue(),
				turnResearched.ToNullableSQLiteValue()
			}), false, true);
			this._dom.player_techs.Clear();
		}
		public void InsertPlayerTechBranch(int playerId, int fromTechId, int toTechId, int researchCost, float feasibility)
		{
			this.insertPlayerTechBranchCount++;
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPlayerTechBranch, new object[]
			{
				playerId,
				fromTechId,
				toTechId,
				researchCost,
				feasibility
			}), false, true);
			this._dom.player_tech_branches.Clear();
		}
		public void InsertTech(string idFromFile)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertTech, idFromFile), false, true);
		}
		public int InsertAdmiral(int playerID, int? homeworldID, string name, string race, float age, string gender, float reaction, float evasion, int loyalty)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertAdmiral, new object[]
			{
				playerID.ToOneBasedSQLiteValue(),
				homeworldID.ToNullableSQLiteValue(),
				name.ToSQLiteValue(),
				race.ToSQLiteValue(),
				age.ToSQLiteValue(),
				gender.ToSQLiteValue(),
				reaction.ToSQLiteValue(),
				evasion.ToSQLiteValue(),
				loyalty.ToSQLiteValue(),
				0,
				0,
				0,
				0,
				this.GetTurnCount(),
				false.ToSQLiteValue()
			}));
		}
		private int InsertFleetCore(int playerID, int admiralID, int systemID, int supportSystemID, string name, FleetType type)
		{
			this._dom.fleets.Clear();
			this._dom.CachedSystemHasGateFlags.Remove(new DataObjectCache.SystemPlayerID
			{
				PlayerID = playerID,
				SystemID = systemID
			});
			int value = 0;
			float value2 = 0f;
			if (this.AssetDatabase.GetFaction(this.GetPlayerInfo(playerID).FactionID).Name == "loa")
			{
				value2 = 10f;
			}
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertFleet, new object[]
			{
				playerID.ToSQLiteValue(),
				admiralID.ToOneBasedSQLiteValue(),
				systemID.ToOneBasedSQLiteValue(),
				supportSystemID.ToOneBasedSQLiteValue(),
				name.ToSQLiteValue(),
				value.ToSQLiteValue(),
				value2.ToSQLiteValue(),
				((int)type).ToSQLiteValue(),
				systemID.ToOneBasedSQLiteValue(),
				false.ToSQLiteValue()
			}));
		}
		public int GetHomeSystem(GameSession sim, int missionId, FleetInfo fi)
		{
			if (this.GetStarSystemInfo(fi.SupportingSystemID) == null || (!this.GetColonyInfosForSystem(fi.SupportingSystemID).ToList<ColonyInfo>().Any((ColonyInfo x) => x.PlayerID == fi.PlayerID) && this.GetStationForSystemPlayerAndType(fi.SupportingSystemID, fi.PlayerID, StationType.NAVAL) == null))
			{
				int num = this.FindNewHomeSystem(fi);
				if (this.GetRemainingSupportPoints(sim, fi.PlayerID, num) < this.GetFleetCruiserEquivalent(fi.ID) || this.GetFleetCommandPointCost(fi.ID) > this.GetFleetCommandPointQuota(fi.ID))
				{
					this.InsertWaypoint(missionId, WaypointType.DisbandFleet, null);
				}
				return num;
			}
			return fi.SupportingSystemID;
		}
		public int FindNewHomeSystem(FleetInfo fi)
		{
			Vector3 v;
			if (fi.SystemID != 0)
			{
				v = this.GetStarSystemOrigin(fi.SystemID);
			}
			else
			{
				v = this.GetFleetLocation(fi.ID, false).Coords;
			}
			List<int> list = this.GetPlayerColonySystemIDs(fi.PlayerID).ToList<int>();
			int result = 0;
			float num = 3.40282347E+38f;
			foreach (int current in list)
			{
				float length = (v - this.GetStarSystemOrigin(current)).Length;
				if (length < num)
				{
					num = length;
					result = current;
				}
			}
			return result;
		}
		public int InsertFleet(int playerID, int admiralID, int systemID, int supportSystemID, string name, FleetType type = FleetType.FL_NORMAL)
		{
			return this.InsertFleetCore(playerID, admiralID, systemID, supportSystemID, name, type);
		}
		private Vector3 GetVFormationPositionAtIndex(int index)
		{
			int num = (index + 1) / 2;
			int num2 = (index + 1) / 2;
			int num3 = (index % 2 == 0) ? 1 : -1;
			Vector3 result = default(Vector3);
			result.X = (float)num3 * 200f * (float)num2;
			result.Y = 0f;
			result.Z = 400f * (float)num;
			result.Z -= 1300f;
			return result;
		}
		private Vector3 GetBackLinePositionAtIndex(int index)
		{
			int num = (int)((float)index / 5f);
			Vector3 result;
			result.X = -1200f + (float)(index % 5) * 600f;
			if (num == 0)
			{
				result.Y = 0f;
			}
			else
			{
				if (num == 1)
				{
					result.Y = 300f;
				}
				else
				{
					result.Y = -300f;
				}
			}
			result.Z = 2000f;
			return result;
		}
		public void LayoutFleet(int fleetID)
		{
			int num = 0;
			int num2 = 0;
			List<ShipInfo> list = this.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				DesignInfo designInfo = this.GetDesignInfo(current.DesignID);
				Vector3 value;
				if (DesignLab.GetShipSize(designInfo).H > 10f)
				{
					value = this.GetBackLinePositionAtIndex(num2);
					num2++;
				}
				else
				{
					value = this.GetVFormationPositionAtIndex(num);
					num++;
				}
				float num3 = 111.111115f;
				value.X = (float)Math.Floor((double)(value.X / num3 + 0.5f)) * num3;
				value.Z = (float)Math.Floor((double)(value.Z / num3 + 0.5f)) * num3;
				this.UpdateShipFleetPosition(current.ID, new Vector3?(value));
			}
		}
		public int InsertMoveOrder(int fleetID, int fromSystemID, Vector3 fromCoords, int toSystemID, Vector3 toCoords)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertMoveOrder, new object[]
			{
				fleetID.ToSQLiteValue(),
				fromSystemID.ToOneBasedSQLiteValue(),
				fromCoords.ToSQLiteValue(),
				toSystemID.ToOneBasedSQLiteValue(),
				toCoords.ToSQLiteValue()
			}));
		}
		public int InsertWaypoint(int missionID, WaypointType type, int? systemID)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertWaypoint, missionID.ToSQLiteValue(), ((int)type).ToSQLiteValue(), systemID.ToNullableSQLiteValue()));
		}
		public void ClearWaypoints(int missionID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.ClearWaypoints, missionID.ToSQLiteValue()), false, true);
		}
		public int InsertMission(int fleetID, MissionType type, int systemID, int orbitalObjectID, int targetFleetID, int duration, bool useDirectRoute, int? stationtype = null)
		{
			GameDatabase.Trace(string.Concat(new object[]
			{
				"InsertMission: ",
				fleetID,
				" ",
				type.ToString(),
				" ",
				systemID.ToString()
			}));
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet)
			{
				throw new InvalidOperationException("Caught attempt to send reserve fleet " + fleetID + " on a mission.");
			}
			MissionInfo missionByFleetID = this.GetMissionByFleetID(fleetID);
			if (missionByFleetID != null)
			{
				this.RemoveMission(missionByFleetID.ID);
			}
			return this._dom.missions.Insert(null, new MissionInfo
			{
				FleetID = fleetID,
				Type = type,
				TargetSystemID = systemID,
				TargetOrbitalObjectID = orbitalObjectID,
				TargetFleetID = targetFleetID,
				Duration = duration,
				UseDirectRoute = useDirectRoute,
				TurnStarted = this.GetTurnCount(),
				StartingSystem = fleetInfo.SystemID,
				StationType = stationtype
			});
		}
		public int InsertColonyTrap(int systemID, int planetID, int fleetID)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo == null || !fleetInfo.IsTrapFleet)
			{
				throw new InvalidOperationException("Insert Colony Trap Fleet " + fleetID + " is invalid.");
			}
			return this._dom.colony_traps.Insert(null, new ColonyTrapInfo
			{
				SystemID = systemID,
				PlanetID = planetID,
				FleetID = fleetID
			});
		}
		private PlayerClientInfo ParsePlayerClientInfo(Row row)
		{
			return new PlayerClientInfo
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				UserName = row[1].SQLiteValueToString()
			};
		}
		public IEnumerable<PlayerClientInfo> GetPlayerClientInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(Queries.GetPlayerClientInfos, true))
			{
				yield return this.ParsePlayerClientInfo(current);
			}
			yield break;
		}
		public void InsertPlayerClientInfo(PlayerClientInfo value)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPlayerClientInfo, value.PlayerID.ToSQLiteValue(), value.UserName.ToSQLiteValue()), false, true);
		}
		public void UpdatePlayerClientInfo(PlayerClientInfo value)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerClientInfo, value.PlayerID.ToSQLiteValue(), value.UserName.ToSQLiteValue()), false, true);
		}
		public int? GetLastClientPlayerID(string username)
		{
			return this.db.ExecuteIntegerQueryDefault(string.Format(Queries.GetPlayerClientInfo, username), null);
		}
		public int InsertPlayer(string name, string factionName, int? homeworldID, Vector3 primaryColor, Vector3 secondaryColor, string badgeAssetPath, string avatarAssetPath, double savings, int subfactionIndex, bool standardPlayer = true, bool includeInDiplomacy = false, bool isAIRebellionPlayer = false, int team = 0, AIDifficulty difficulty = AIDifficulty.Normal)
		{
			PlayerInfo value = new PlayerInfo
			{
				Name = name,
				FactionID = this.GetFactionIdFromName(factionName),
				Homeworld = homeworldID,
				PrimaryColor = primaryColor,
				SecondaryColor = secondaryColor,
				BadgeAssetPath = badgeAssetPath,
				AvatarAssetPath = avatarAssetPath,
				Savings = savings,
				SubfactionIndex = subfactionIndex,
				isStandardPlayer = standardPlayer,
				includeInDiplomacy = includeInDiplomacy,
				isAIRebellionPlayer = isAIRebellionPlayer,
				Team = team,
				AIDifficulty = difficulty
			};
			return this._dom.players.Insert(null, value);
		}
		public void InsertMissingFactions(Random initializationRandomSeed)
		{
			foreach (Faction current in this.assetdb.Factions)
			{
				float suitability = current.ChooseIdealSuitability(initializationRandomSeed);
				this.InsertOrIgnoreFaction(current.ID, current.Name, suitability);
			}
		}
		public void UpdateFaction(FactionInfo fi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFactionInfo, fi.ID.ToSQLiteValue(), fi.Name.ToSQLiteValue(), fi.IdealSuitability.ToSQLiteValue()), false, true);
		}
		public void InsertOrIgnoreFaction(int id, string factionName, float suitability)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertFaction, id.ToSQLiteValue(), factionName.ToSQLiteValue(), suitability.ToSQLiteValue()), false, true);
		}
		public void InsertOrIgnoreAI(int playerID, AIStance stance = AIStance.EXPANDING)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertOrIgnoreAI, playerID.ToSQLiteValue(), ((int)stance).ToSQLiteValue()), false, true);
		}
		public void InsertAIOldColonyOwner(int colonyId, int playerId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertAIOldColonyOwner, colonyId, playerId), false, true);
		}
		public int[] GetAIOldColonyOwner(int playerId)
		{
			return this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetAIOldColoniesByPlayer, playerId));
		}
		public void InsertAIStationIntel(int playerID, int intelOnPlayerID, int stationID, int level)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIntelStation, new object[]
			{
				playerID.ToSQLiteValue(),
				intelOnPlayerID.ToSQLiteValue(),
				stationID.ToSQLiteValue(),
				this.GetTurnCount().ToSQLiteValue(),
				level.ToSQLiteValue()
			}), false, true);
		}
		public void InsertAIDesignIntel(int playerID, int intelOnPlayerID, int designID, bool salvaged)
		{
			int turnCount = this.GetTurnCount();
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIntelDesign, new object[]
			{
				playerID.ToSQLiteValue(),
				intelOnPlayerID.ToSQLiteValue(),
				designID.ToSQLiteValue(),
				turnCount.ToSQLiteValue(),
				turnCount.ToSQLiteValue(),
				salvaged.ToSQLiteValue()
			}), false, true);
		}
		public void InsertAIFleetIntel(int playerID, int intelOnPlayerID, int system, Vector3 coords, int numDestroyers, int numCruisers, int numDreadnought, int numLeviathan)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIntelFleet, new object[]
			{
				playerID.ToSQLiteValue(),
				intelOnPlayerID.ToSQLiteValue(),
				this.GetTurnCount().ToSQLiteValue(),
				system.ToSQLiteValue(),
				coords.ToSQLiteValue(),
				numDestroyers.ToSQLiteValue(),
				numCruisers.ToSQLiteValue(),
				numDreadnought.ToSQLiteValue(),
				numLeviathan.ToSQLiteValue()
			}), false, true);
		}
		public void InsertAITechWeight(int playerID, string family, float weight)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertAITechWeight, new object[]
			{
				family.ToSQLiteValue(),
				playerID.ToSQLiteValue(),
				0f.ToSQLiteValue(),
				weight.ToSQLiteValue()
			}), false, true);
		}
		public int InsertDiplomaticState(int playerID, int towardPlayerID, DiplomacyState type, int relations, bool isEncountered, bool reciprocal = false)
		{
			return this._dom.diplomacy_states.InsertDiplomaticState(playerID, towardPlayerID, type, relations, isEncountered, reciprocal);
		}
		public int InsertIndependentRace(string name, int orbitalObjectID, double population, int techLevel, int rxHuman, int rxTarka, int rxLiir, int rxZuul, int rxMorrigi, int rxHiver)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertIndependentRace, new object[]
			{
				name.ToSQLiteValue(),
				orbitalObjectID.ToSQLiteValue(),
				population.ToSQLiteValue(),
				techLevel.ToSQLiteValue(),
				rxHuman.ToSQLiteValue(),
				rxTarka.ToSQLiteValue(),
				rxLiir.ToSQLiteValue(),
				rxZuul.ToSQLiteValue(),
				rxMorrigi.ToSQLiteValue(),
				rxHiver.ToSQLiteValue()
			}));
		}
		public int InsertIndependentRaceColony(int raceID, int orbitalObjectID, double population)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertIndependentRaceColony, orbitalObjectID.ToSQLiteValue(), raceID.ToSQLiteValue(), population.ToSQLiteValue()));
		}
		public int InsertFeasibilityStudy(int playerId, int techId, string projectName)
		{
			PlayerTechInfo playerTechInfo = this.GetPlayerTechInfo(playerId, techId);
			if (playerTechInfo.State != TechStates.Branch)
			{
				throw new ArgumentException(string.Format("Player {0} cannot start of feasibility study for {1} (current state is {2})", playerId, playerTechInfo.TechFileID, playerTechInfo.State));
			}
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertFeasibilityStudy, new object[]
			{
				playerId.ToSQLiteValue(),
				1.ToSQLiteValue(),
				projectName.ToSQLiteValue(),
				techId.ToSQLiteValue(),
				(playerTechInfo.ResearchCost / 10).ToSQLiteValue()
			}));
		}
		private void GuaranteeAsset(string assetPath)
		{
			this.db.ExecuteNonQuery(string.Format("INSERT OR REPLACE INTO assets (id,path) VALUES ((SELECT id FROM assets WHERE path=\"{0}\"),\"{0}\")", assetPath.ToSQLiteValue()), false, true);
		}
		private int InsertStellarProp(string assetPath, Matrix transform)
		{
			this.GuaranteeAsset(assetPath);
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertStellarProp, assetPath.ToSQLiteValue(), transform.ToSQLiteValue()));
		}
		public int InsertProvince(string name, int playerId, IEnumerable<int> systemIds, int capitalId)
		{
			int num = this.db.ExecuteIntegerQuery(string.Format(Queries.InsertProvince, playerId.ToSQLiteValue(), name.ToSQLiteValue(), capitalId.ToSQLiteValue()));
			foreach (int current in systemIds)
			{
				this.UpdateSystemProvinceID(current, new int?(num));
			}
			return num;
		}
		public void RemoveProvince(int provinceId)
		{
			this._dom.star_systems.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveProvince, provinceId), false, true);
		}
		public void UpdateSystemProvinceID(int systemId, int? provinceId)
		{
			this._dom.star_systems.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSystemProvinceID, systemId.ToSQLiteValue(), provinceId.ToNullableSQLiteValue()), false, true);
		}
		public int InsertNodeLine(int systemAId, int systemBId, int health)
		{
			return this._dom.node_lines.Insert(null, new NodeLineInfo
			{
				System1ID = systemAId,
				System2ID = systemBId,
				Health = health
			});
		}
		public void RemoveNodeLine(int nodeid)
		{
            Kerberos.Sots.StarSystemPathing.StarSystemPathing.RemoveNodeLine(nodeid);
			this._dom.node_lines.Remove(nodeid);
		}
		public void UpdateNodeLineHealth(int nodeid, int health)
		{
			NodeLineInfo nodeLineInfo = this._dom.node_lines[nodeid];
			nodeLineInfo.Health = health;
			this._dom.node_lines.Update(nodeLineInfo.ID, nodeLineInfo);
		}
		public int InsertTerrain(string name, Vector3 origin)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertTerrain, name.ToSQLiteValue(), origin.ToSQLiteValue()));
		}
		public void FloodTest()
		{
			this.db.ExecuteNonQuery("BEGIN DEFERRED TRANSACTION", false, true);
			this.InsertOrIgnoreFaction(0, "blah", 12.34f);
			for (int i = 0; i < 1000; i++)
			{
				this.InsertPlayer("player" + (i + 1), "blah", new int?(0), new Vector3((float)i, (float)i, (float)i), Vector3.One, "factions\\human\\badges\\badge01.tga", "factions\\human\\avatar\\avatar01.tga", 0.0, 0, true, false, false, 0, AIDifficulty.Normal);
			}
			for (int j = 0; j < 1000; j++)
			{
				this.InsertStarSystem(new int?(j), "starsystem" + (j + 1), new int?(0), "abc", new Vector3((float)j, (float)j, (float)j), true, true, new int?(0));
			}
			for (int k = 0; k < 1000; k++)
			{
				this.InsertPlanet(null, k / 2 + 1, OrbitalPath.Zero, "Planet" + (k + 1), "barren", null, 10f, 0, 0, 10f);
			}
			this.db.ExecuteNonQuery("COMMIT TRANSACTION", false, true);
		}
		public int InsertCombatData(int systemID, int combatID, int turn, byte[] data)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertCombatData, new object[]
			{
				systemID.ToSQLiteValue(),
				combatID.ToSQLiteValue(),
				turn.ToSQLiteValue(),
				data.ToSQLiteValue(),
				1.ToSQLiteValue()
			}));
		}
		public ScriptMessageReader GetMostRecentCombatData(int systemId)
		{
			string text = this.db.ExecuteStringQuery(string.Format(Queries.GetMostRecentCombatData, systemId));
			if (text != null)
			{
				byte[] buffer = text.SQLiteValueToByteArray();
				MemoryStream stream = new MemoryStream(buffer);
				return new ScriptMessageReader(true, stream);
			}
			return null;
		}
		public int[] GetRecentCombatTurns(int systemId, int oldestTurn)
		{
			int[] array = this.db.ExecuteIntegerArrayQuery(string.Format("SELECT turn FROM combat_data WHERE system_id={0} AND turn >= {1};", systemId.ToSQLiteValue(), oldestTurn.ToSQLiteValue()));
			Array.Sort<int>(array);
			return array;
		}
		public ScriptMessageReader GetCombatData(int systemId, int turn, out int version)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetCombatData, systemId, turn), true);
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				string text = row[0].SQLiteValueToString();
				version = row[1].SQLiteValueToInteger();
				if (text != null)
				{
					byte[] buffer = text.SQLiteValueToByteArray();
					MemoryStream stream = new MemoryStream(buffer);
					return new ScriptMessageReader(true, stream);
				}
			}
			version = 0;
			return null;
		}
		public ScriptMessageReader GetCombatData(int systemId, int combatID, int turn, out int version)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetCombatSpecificData, systemId, combatID, turn), true);
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				string text = row[0].SQLiteValueToString();
				version = row[1].SQLiteValueToInteger();
				if (text != null)
				{
					byte[] buffer = text.SQLiteValueToByteArray();
					MemoryStream stream = new MemoryStream(buffer);
					return new ScriptMessageReader(true, stream);
				}
			}
			version = 0;
			return null;
		}
		private IEnumerable<ScriptMessageReader> GetCombatDatas()
		{
			foreach (Row current in this.db.ExecuteTableQuery(Queries.GetCombatDatas, true))
			{
				byte[] buffer = current[0].SQLiteValueToByteArray();
				using (MemoryStream memoryStream = new MemoryStream(buffer))
				{
					yield return new ScriptMessageReader(true, memoryStream);
				}
			}
			yield break;
		}
		public void InsertColonyFaction(int orbitalObjectID, int factionId, double civPopulation, float civPopulationWeight, int turnEstablished)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertColonyFaction, new object[]
			{
				orbitalObjectID.ToSQLiteValue(),
				factionId.ToSQLiteValue(),
				civPopulation.ToSQLiteValue(),
				this.assetdb.CivilianPopulationStartMoral.ToSQLiteValue(),
				civPopulationWeight.ToSQLiteValue(),
				turnEstablished.ToSQLiteValue(),
				this.assetdb.CivilianPopulationStartMoral.ToSQLiteValue()
			}), false, true);
			this._dom.colonies.SyncRange((
				from x in this._dom.colonies
				where x.Value.OrbitalObjectID == orbitalObjectID
				select x into y
				select y.Key).ToList<int>());
		}
		public void AddPlanetToSystem(int SystemId, int? parentOrbitId, string Name, PlanetInfo pi, int? OrbitNumber = null)
		{
			Random safeRandom = App.GetSafeRandom();
			float orbitStep = StarSystemVars.Instance.StarOrbitStep;
			float parentRadius = StarSystemVars.Instance.StarRadius(StellarClass.Parse(this.GetStarSystemInfo(SystemId).StellarClass).Size);
			int arg_76_0;
			if (!OrbitNumber.HasValue)
			{
				arg_76_0 = (
					from x in this.GetStarSystemOrbitalObjectInfos(SystemId)
					where !x.ParentID.HasValue
					select x).Count<OrbitalObjectInfo>();
			}
			else
			{
				arg_76_0 = OrbitNumber.Value;
			}
			int num = arg_76_0;
			if (parentOrbitId.HasValue)
			{
				if (this.GetPlanetInfo(parentOrbitId.Value) != null)
				{
					orbitStep = StarSystemVars.Instance.PlanetOrbitStep;
				}
				else
				{
					orbitStep = StarSystemVars.Instance.GasGiantOrbitStep;
				}
				parentRadius = StarSystemVars.Instance.SizeToRadius(this.GetPlanetInfo(parentOrbitId.Value).Size);
			}
			float eccentricity = safeRandom.NextNormal(StarSystemVars.Instance.OrbitEccentricityRange);
			safeRandom.NextNormal(StarSystemVars.Instance.OrbitInclinationRange);
			float num2 = Kerberos.Sots.Orbit.CalcOrbitRadius(parentRadius, orbitStep, num);
			float x2 = Ellipse.CalcSemiMinorAxis(num2, eccentricity);
			StarSystemInfo starSystemInfo = this.GetStarSystemInfo(SystemId);
			string name = string.IsNullOrEmpty(Name) ? (starSystemInfo.Name + " " + num) : Name;
			this.InsertPlanet(parentOrbitId, SystemId, new OrbitalPath
			{
				Scale = new Vector2(x2, num2),
				InitialAngle = safeRandom.NextSingle() % 6.28318548f
			}, name, pi.Type, null, pi.Suitability, pi.Biosphere, pi.Resources, pi.Size);
		}
		public OrbitalPath OrbitNumberToOrbitalPath(int orbitNumber, Kerberos.Sots.StellarSize size, float? orbitStep = null)
		{
			Random safeRandom = App.GetSafeRandom();
			float orbitStep2 = StarSystemVars.Instance.StarOrbitStep;
			if (orbitStep.HasValue)
			{
				orbitStep2 = orbitStep.Value;
			}
			float parentRadius = StarSystemVars.Instance.StarRadius(size);
			float eccentricity = safeRandom.NextNormal(StarSystemVars.Instance.OrbitEccentricityRange);
			safeRandom.NextNormal(StarSystemVars.Instance.OrbitInclinationRange);
			float num = Kerberos.Sots.Orbit.CalcOrbitRadius(parentRadius, orbitStep2, orbitNumber);
			float x = Ellipse.CalcSemiMinorAxis(num, eccentricity);
			return new OrbitalPath
			{
				Scale = new Vector2(x, num),
				InitialAngle = safeRandom.NextSingle() % 6.28318548f
			};
		}
		public void ImportStarMap(ref LegacyStarMap starmap, Random random, GameSession.Flags flags)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Dictionary<LegacyTerrain, int> dictionary2 = new Dictionary<LegacyTerrain, int>();
			foreach (LegacyTerrain current in starmap.Terrain)
			{
				int value = this.InsertTerrain(current.Name, current.Origin);
				dictionary2[current] = value;
			}
			foreach (Kerberos.Sots.StarSystem starsystem in starmap.Objects.OfType<Kerberos.Sots.StarSystem>())
			{
				int num = this.InsertStarSystem(new int?(starsystem.Params.Guid.Value), starsystem.DisplayName, null, starsystem.StellarClass.ToString(), starsystem.WorldTransform.Position, starsystem.Params.isVisible, true, (starsystem.Terrain != null) ? new int?(dictionary2[starsystem.Terrain]) : null);
				starsystem.ID = num;
				dictionary.Add(starsystem.Params.Guid.Value, starsystem.ID);
				if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0)
				{
					Dictionary<Kerberos.Sots.Data.StarMapFramework.Orbit, int> dictionary3 = new Dictionary<Kerberos.Sots.Data.StarMapFramework.Orbit, int>();
					List<IStellarEntity> list = (
						from x in starsystem.Objects
						where x != starsystem.Star
						select x).ToList<IStellarEntity>();
					while (list.Count > 0)
					{
						IStellarEntity stellarEntity = list[0];
						list.RemoveAt(0);
						int? parentOrbitalObjectId = (stellarEntity.Orbit.Parent == starsystem.Star.Params) ? null : new int?(dictionary3[stellarEntity.Orbit.Parent]);
						OrbitalPath path = new OrbitalPath
						{
							Scale = new Vector2(stellarEntity.Orbit.SemiMinorAxis, stellarEntity.Orbit.SemiMajorAxis),
							InitialAngle = (float)((double)(stellarEntity.Orbit.Position * 2f) * 3.1415926535897931)
						};
						PlanetInfo planetInfo = StarSystemHelper.InferPlanetInfo(stellarEntity.Params);
						if (planetInfo != null)
						{
							int num2 = this.InsertPlanet(parentOrbitalObjectId, num, path, stellarEntity.Params.Name, planetInfo.Type, null, planetInfo.Suitability, planetInfo.Biosphere, planetInfo.Resources, planetInfo.Size);
							dictionary3[stellarEntity.Params] = num2;
							stellarEntity.ID = num2;
						}
						else
						{
							if (stellarEntity.Params is AsteroidOrbit)
							{
								int num3 = this.InsertAsteroidBelt(parentOrbitalObjectId, num, path, stellarEntity.Params.Name, random.Next());
								dictionary3[stellarEntity.Params] = num3;
								stellarEntity.ID = num3;
								int num4 = random.Next(2, 5) - (3 - stellarEntity.Params.OrbitNumber);
								for (int i = 0; i < num4; i++)
								{
									int resources = random.Next(3000);
									this.InsertLargeAsteroid(num3, num, default(OrbitalPath), stellarEntity.Params.Name + (char)(65 + i), resources);
								}
							}
						}
						if (!dictionary3.ContainsKey(stellarEntity.Params))
						{
							GameDatabase.Warn("Unhandled star system object type encountered during import to db: " + stellarEntity.Params.GetType());
						}
					}
				}
			}
			foreach (StellarProp current2 in starmap.Objects.OfType<StellarProp>())
			{
				this.InsertStellarProp(current2.Params.Model, current2.Transform);
			}
			foreach (NodeLine nodeline in starmap.NodeLines)
			{
				if (starmap.Objects.OfType<Kerberos.Sots.StarSystem>().Any((Kerberos.Sots.StarSystem x) => x.Params.Guid == nodeline.SystemA))
				{
					if (starmap.Objects.OfType<Kerberos.Sots.StarSystem>().Any((Kerberos.Sots.StarSystem x) => x.Params.Guid == nodeline.SystemB))
					{
						if (!starmap.NodeLines.Any((NodeLine x) => x.SystemA == nodeline.SystemB && x.SystemB == nodeline.SystemA && x.isPermanent == nodeline.isPermanent))
						{
							int iD = starmap.Objects.OfType<Kerberos.Sots.StarSystem>().First((Kerberos.Sots.StarSystem x) => x.Params.Guid == nodeline.SystemA).ID;
							int iD2 = starmap.Objects.OfType<Kerberos.Sots.StarSystem>().First((Kerberos.Sots.StarSystem x) => x.Params.Guid == nodeline.SystemB).ID;
							this.InsertNodeLine(iD, iD2, nodeline.isPermanent ? -1 : 1000);
						}
					}
				}
			}
		}
		public float GetStationAdditionalStratSensorRange(StationInfo station)
		{
			DesignInfo designInfo = this.GetDesignInfo(station.DesignInfo.ID);
			int num = designInfo.DesignSections.Sum((DesignSectionInfo x) => x.Modules.Count((DesignModuleInfo y) => y.StationModuleType.Value == ModuleEnums.StationModuleType.Sensor));
			float num2 = 0f;
			switch (designInfo.StationType)
			{
			case StationType.NAVAL:
				num2 += (float)num * 0.5f;
				break;
			case StationType.SCIENCE:
			case StationType.CIVILIAN:
				num2 += (float)num * 0.25f;
				break;
			case StationType.DIPLOMATIC:
				num2 += (float)num * 0.2f;
				break;
			}
			return station.DesignInfo.StratSensorRange + num2;
		}
		public float GetStationStratSensorRange(StationInfo station)
		{
			return station.GetBaseStratSensorRange() + this.GetStationAdditionalStratSensorRange(station);
		}
		public float GetSystemStratSensorRange(int systemid, int playerid)
		{
			DataObjectCache.SystemPlayerID key = new DataObjectCache.SystemPlayerID
			{
				SystemID = systemid,
				PlayerID = playerid
			};
			float num = 0f;
			List<PlayerInfo> list = (
				from x in this.GetPlayerInfos()
				where x.ID != playerid && this.GetDiplomacyStateBetweenPlayers(x.ID, playerid) == DiplomacyState.ALLIED
				select x).ToList<PlayerInfo>();
			list.Add(this.GetPlayerInfo(playerid));
			if (!this._dom.CachedSystemStratSensorRanges.TryGetValue(key, out num))
			{
				foreach (ColonyInfo colony in this.GetColonyInfosForSystem(systemid))
				{
					if (list.Any((PlayerInfo x) => x.ID == colony.PlayerID))
					{
						num = 5f;
					}
				}
				foreach (PlayerInfo current in list)
				{
					foreach (StationInfo current2 in this.GetStationForSystemAndPlayer(systemid, current.ID))
					{
						num = Math.Max(num, this.GetStationStratSensorRange(current2));
					}
				}
				foreach (PlayerInfo current3 in list)
				{
					foreach (FleetInfo current4 in this.GetFleetsByPlayerAndSystem(current3.ID, systemid, FleetType.FL_ALL))
					{
						num = Math.Max(num, GameSession.GetFleetSensorRange(this.assetdb, this, current4.ID));
					}
				}
				this._dom.CachedSystemStratSensorRanges.Add(key, num);
			}
			return num;
		}
		public int InsertNameValuePair(string name, string value)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertNameValuePair, name, value));
		}
		internal static string GetNameValue(SQLiteConnection db, string name)
		{
			return db.ExecuteStringQuery(string.Format(Queries.GetNameValue, name));
		}
		public string GetNameValue(string name)
		{
			return GameDatabase.GetNameValue(this.db, name);
		}
		public T GetNameValue<T>(string name)
		{
			string value = this.db.ExecuteStringQuery(string.Format(Queries.GetNameValue, name));
			if (string.IsNullOrEmpty(value))
			{
				return default(T);
			}
			return (T)((object)Convert.ChangeType(value, typeof(T)));
		}
		public T GetStratModifier<T>(StratModifiers modifier, int playerId, T defaultValue)
		{
			if (!GameDatabase._cachedStratMods.ContainsKey(playerId))
			{
				GameDatabase._cachedStratMods.Add(playerId, new Dictionary<StratModifiers, CachedStratMod>());
			}
			else
			{
				if (GameDatabase._cachedStratMods[playerId].ContainsKey(modifier) && GameDatabase._cachedStratMods[playerId][modifier].isUpToDate)
				{
					return (T)((object)GameDatabase._cachedStratMods[playerId][modifier].CachedValue);
				}
			}
			string value = this.db.ExecuteStringQuery(string.Format(Queries.GetStratModifier, modifier.ToString().ToSQLiteValue(), playerId.ToSQLiteValue()));
			if (string.IsNullOrEmpty(value))
			{
				return defaultValue;
			}
			T t = (T)((object)Convert.ChangeType(value, typeof(T)));
			if (GameDatabase._cachedStratMods[playerId].ContainsKey(modifier))
			{
				GameDatabase._cachedStratMods[playerId][modifier].CachedValue = t;
				GameDatabase._cachedStratMods[playerId][modifier].isUpToDate = true;
			}
			else
			{
				GameDatabase._cachedStratMods[playerId].Add(modifier, new CachedStratMod
				{
					CachedValue = t,
					isUpToDate = true
				});
			}
			return t;
		}
		public float GetStratModifierFloatToApply(StratModifiers modifier, int playerId)
		{
			float stratModifier = this.GetStratModifier<float>(modifier, playerId);
			return this.assetdb.GovEffects.GetStratModifierTotal(this, modifier, playerId, stratModifier);
		}
		public int GetStratModifierIntToApply(StratModifiers modifier, int playerId)
		{
			int stratModifier = this.GetStratModifier<int>(modifier, playerId);
			return this.assetdb.GovEffects.GetStratModifierTotal(this, modifier, playerId, stratModifier);
		}
		public T GetStratModifier<T>(StratModifiers modifier, int playerId)
		{
			if (!GameDatabase._cachedStratMods.ContainsKey(playerId))
			{
				GameDatabase._cachedStratMods.Add(playerId, new Dictionary<StratModifiers, CachedStratMod>());
			}
			else
			{
				if (GameDatabase._cachedStratMods[playerId].ContainsKey(modifier) && GameDatabase._cachedStratMods[playerId][modifier].isUpToDate)
				{
					return (T)((object)GameDatabase._cachedStratMods[playerId][modifier].CachedValue);
				}
			}
			object obj = this.db.ExecuteStringQuery(string.Format(Queries.GetStratModifier, modifier.ToString().ToSQLiteValue(), playerId.ToSQLiteValue()));
			if (obj == null)
			{
				obj = this.assetdb.DefaultStratModifiers[modifier];
			}
			T t = (T)((object)Convert.ChangeType(obj, typeof(T)));
			if (GameDatabase._cachedStratMods[playerId].ContainsKey(modifier))
			{
				GameDatabase._cachedStratMods[playerId][modifier].CachedValue = t;
				GameDatabase._cachedStratMods[playerId][modifier].isUpToDate = true;
			}
			else
			{
				GameDatabase._cachedStratMods[playerId].Add(modifier, new CachedStratMod
				{
					CachedValue = t,
					isUpToDate = true
				});
			}
			return t;
		}
		public void SetStratModifier(StratModifiers modifier, int playerId, object value)
		{
			if (GameDatabase._cachedStratMods.ContainsKey(playerId) && GameDatabase._cachedStratMods[playerId].ContainsKey(modifier))
			{
				GameDatabase._cachedStratMods[playerId][modifier].isUpToDate = false;
			}
			this.db.ExecuteNonQuery(string.Format(Queries.SetStratModifier, modifier.ToString().ToSQLiteValue(), playerId.ToSQLiteValue(), value.ToString().ToSQLiteValue()), false, true);
		}
		private void AddNumShipsBuiltFromDesign(int designID, int count)
		{
			if (count <= 0)
			{
				throw new ArgumentOutOfRangeException("count", "Must be > 0.");
			}
			this.db.ExecuteNonQuery(string.Format(Queries.AddNumShipsBuiltFromDesign, count.ToSQLiteValue(), designID.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(designID);
		}
		private void AddNumShipsDestroyedFromDesign(int designID, int count)
		{
			if (count <= 0)
			{
				throw new ArgumentOutOfRangeException("count", "Must be > 0.");
			}
			this.db.ExecuteNonQuery(string.Format(Queries.AddNumShipsDestroyedFromDesign, count.ToSQLiteValue(), designID.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(designID);
		}
		public IEnumerable<NodeLineInfo> GetNodeLines()
		{
			return this._dom.node_lines.Values;
		}
		public IEnumerable<NodeLineInfo> GetNonPermenantNodeLines()
		{
			return 
				from x in this._dom.node_lines.Values
				where !x.IsPermenant && x.Health > -1 && !x.IsLoaLine
				select x;
		}
		public void SpendDiplomacyPoints(PlayerInfo spendingPlayer, int factionId, int pointCost)
		{
			int num = spendingPlayer.FactionDiplomacyPoints.ContainsKey(factionId) ? spendingPlayer.FactionDiplomacyPoints[factionId] : 0;
			int num2 = Math.Min(pointCost, num);
			this.UpdateFactionDiplomacyPoints(spendingPlayer.ID, factionId, num - num2);
			this.UpdateGenericDiplomacyPoints(spendingPlayer.ID, spendingPlayer.GenericDiplomacyPoints - (pointCost - num2) * 2);
		}
		public NodeLineInfo GetNodeLine(int id)
		{
			return this._dom.node_lines.Values.FirstOrDefault((NodeLineInfo x) => x.ID == id);
		}
		public bool IsStealthFleet(int fleetId)
		{
			int techID = this.GetTechID("IND_Stealth_Armor");
			List<ShipInfo> list = this.GetShipInfoByFleetID(fleetId, true).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				DesignSectionInfo[] designSections = current.DesignInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					if (!designSectionInfo.Techs.Contains(techID))
					{
						return false;
					}
				}
			}
			return true;
		}
		public IEnumerable<NodeLineInfo> GetExploredNodeLines(int playerId)
		{
			int playerFactionID = this.GetPlayerFactionID(playerId);
			string factionName = this.GetFactionName(playerFactionID);
			if (factionName == "human" || factionName == "zuul" || factionName == "loa")
			{
				List<ExploreRecordInfo> source = (
					from x in this._dom.explore_records.Values
					where x.PlayerId == playerId
					select x).ToList<ExploreRecordInfo>();
				foreach (NodeLineInfo nli in this._dom.node_lines.Values)
				{
					if (source.Any((ExploreRecordInfo x) => x.SystemId == nli.System1ID || x.SystemId == nli.System2ID))
					{
						yield return nli;
					}
				}
			}
			yield break;
		}
		public NodeLineInfo GetNodeLineBetweenSystems(int playerID, int systemA, int systemB, bool isPermanent, bool loaline = false)
		{
			if (!loaline)
			{
				return this.GetExploredNodeLinesFromSystem(playerID, systemA).ToList<NodeLineInfo>().FirstOrDefault((NodeLineInfo x) => (x.System1ID == systemB || x.System2ID == systemB) && x.IsPermenant == isPermanent);
			}
			return this.GetExploredNodeLinesFromSystem(playerID, systemA).ToList<NodeLineInfo>().FirstOrDefault((NodeLineInfo x) => (x.System1ID == systemB || x.System2ID == systemB) && !x.IsPermenant && x.IsLoaLine == loaline);
		}
		public void InsertLoaLineFleetRecord(int lineid, int fleetid)
		{
			this.db.ExecuteNonQuery(string.Format("INSERT INTO loa_line_records (node_line_id, fleet_id) VALUES ({0},{1})", lineid.ToSQLiteValue(), fleetid.ToSQLiteValue()), false, true);
			this._dom.node_lines.Sync(lineid);
		}
		public IEnumerable<int> GetFleetsForLoaLine(int lineID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format("SELECT * FROM loa_line_records WHERE node_line_id = {0}", lineID.ToSQLiteValue()), true);
			foreach (Row current in table)
			{
				yield return current[1].SQLiteValueToInteger();
			}
			yield break;
		}
		public IEnumerable<NodeLineInfo> GetExploredNodeLinesFromSystem(int playerID, int systemID)
		{
			List<NodeLineInfo> list = (
				from x in this._dom.node_lines.Values
				where x.System1ID == systemID || x.System2ID == systemID
				select x).ToList<NodeLineInfo>();
			List<ExploreRecordInfo> source = (
				from x in this._dom.explore_records.Values
				where x.PlayerId == playerID
				select x).ToList<ExploreRecordInfo>();
			foreach (NodeLineInfo nli in list)
			{
				if (source.Any((ExploreRecordInfo x) => x.SystemId == nli.System1ID || x.SystemId == nli.System2ID))
				{
					yield return new NodeLineInfo
					{
						ID = nli.ID,
						System1ID = nli.System1ID,
						System2ID = nli.System2ID,
						Health = nli.Health,
						IsLoaLine = nli.IsLoaLine
					};
				}
			}
			yield break;
		}
		public HomeworldInfo GetPlayerHomeworld(int playerid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerHomeworld, playerid.ToSQLiteValue()), true);
			if (table.Rows.Count<Row>() == 0)
			{
				return null;
			}
			Row row = table.Rows[0];
			HomeworldInfo homeworldInfo = new HomeworldInfo
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				ColonyID = row[1].SQLiteValueToInteger(),
				SystemID = row[2].SQLiteValueToInteger()
			};
			ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(homeworldInfo.ColonyID);
			if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == homeworldInfo.PlayerID)
			{
				homeworldInfo.ColonyID = colonyInfoForPlanet.ID;
				return homeworldInfo;
			}
			return null;
		}
		public IEnumerable<HomeworldInfo> GetHomeworlds()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetHomeworlds, true);
			foreach (Row current in table)
			{
				HomeworldInfo homeworldInfo = new HomeworldInfo
				{
					PlayerID = current[0].SQLiteValueToInteger(),
					ColonyID = current[1].SQLiteValueToInteger(),
					SystemID = current[2].SQLiteValueToInteger()
				};
				ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(homeworldInfo.ColonyID);
				if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == homeworldInfo.PlayerID)
				{
					homeworldInfo.ColonyID = colonyInfoForPlanet.ID;
					yield return homeworldInfo;
				}
			}
			yield break;
		}
		public void UpdateHomeworldInfo(HomeworldInfo hw)
		{
			ColonyInfo colonyInfo = this.GetColonyInfo(hw.ColonyID);
			this.UpdatePlayerHomeworld(hw.PlayerID, colonyInfo.OrbitalObjectID);
		}
		internal static int GetTurnCount(SQLiteConnection db)
		{
			return GameDatabase.GetNameValue(db, "turn").SQLiteValueToInteger();
		}
		public int GetTurnCount()
		{
			return GameDatabase.GetTurnCount(this.db);
		}
		public Matrix GetOrbitalTransform(int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalId);
			return this.GetOrbitalTransform(orbitalObjectInfo);
		}
		public Matrix GetOrbitalTransform(OrbitalObjectInfo orbital)
		{
			int turnCount = this.GetTurnCount();
			return GameDatabase.CalcTransform(orbital.ID, (float)turnCount, this.GetStarSystemOrbitalObjectInfos(orbital.StarSystemID));
		}
		public void UpdateNameValuePair(string name, string value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateNameValuePair, name.ToSQLiteValue(), value.ToSQLiteValue()), true);
		}
		public void IncrementTurnCount()
		{
			int turnCount = this.GetTurnCount();
			this.UpdateNameValuePair("turn", (turnCount + 1).ToString());
			this._dom.CachedSystemStratSensorRanges.Clear();
			if ((turnCount + 1) % 20 == 0)
			{
				this.db.VacuumDatabase();
			}
		}
		public void CullQueryHistory(int numTurnsToKeep)
		{
			int turnCount = this.GetTurnCount();
			int value = turnCount - numTurnsToKeep;
			this.db.ExecuteNonQuery(string.Format("DELETE FROM query_history WHERE turn <= {0}", value.ToSQLiteValue()), true, true);
		}
		public IEnumerable<int> GetStandardPlayerIDs()
		{
			return this._dom.players.GetStandardPlayerIDs();
		}
		public IEnumerable<int> GetPlayerIDs()
		{
			return this._dom.players.Keys;
		}
		public int GetPlayerResearchingTechID(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetPlayerResearchingTechID, playerId.ToSQLiteValue()));
		}
		public int GetPlayerFeasibilityStudyTechId(int playerId)
		{
			FeasibilityStudyInfo feasibilityStudyInfoByPlayer = this.GetFeasibilityStudyInfoByPlayer(playerId);
			if (feasibilityStudyInfoByPlayer != null)
			{
				return feasibilityStudyInfoByPlayer.TechID;
			}
			return 0;
		}
		private PlayerTechInfo GetPlayerTechInfo(Row row)
		{
			return new PlayerTechInfo
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				TechID = row[1].SQLiteValueToInteger(),
				State = (TechStates)row[2].SQLiteValueToInteger(),
				Progress = row[3].SQLiteValueToInteger(),
				TechFileID = row[4].SQLiteValueToString(),
				ResearchCost = row[5].SQLiteValueToInteger(),
				Feasibility = row[6].SQLiteValueToSingle(),
				PlayerFeasibility = row[7].SQLiteValueToSingle(),
				TurnResearched = row[8].SQLiteValueToNullableInteger()
			};
		}
		public PlayerTechInfo GetPlayerTechInfo(PlayerTechInfo.PrimaryKey id)
		{
			return this.GetPlayerTechInfo(id.PlayerID, id.TechID);
		}
		public PlayerTechInfo GetPlayerTechInfo(int playerId, int techId)
		{
			PlayerTechInfo playerTechInfo = this._dom.player_techs.Find(new PlayerTechInfo.PrimaryKey
			{
				PlayerID = playerId,
				TechID = techId
			});
			if (playerTechInfo != null)
			{
				return playerTechInfo;
			}
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerTechInfo, playerId.ToSQLiteValue(), techId.ToSQLiteValue()), true);
			if (table == null || table.Rows.Length == 0)
			{
				return null;
			}
			playerTechInfo = this.GetPlayerTechInfo(table[0]);
			this._dom.player_techs.Cache(playerTechInfo.ID, playerTechInfo);
			return playerTechInfo;
		}
		private ResearchProjectInfo GetResearchProjectInfo(Row row)
		{
			return new ResearchProjectInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToInteger(),
				State = (ProjectStates)row[2].SQLiteValueToInteger(),
				Name = row[3].SQLiteValueToString(),
				Progress = row[4].SQLiteValueToSingle()
			};
		}
		public IEnumerable<ResearchProjectInfo> GetResearchProjectInfos(int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetResearchProjectInfosForPlayer, playerId.ToSQLiteValue()), true);
			foreach (Row current in table)
			{
				yield return this.GetResearchProjectInfo(current);
			}
			yield break;
		}
		public IEnumerable<ResearchProjectInfo> GetResearchProjectInfos()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetResearchProjectInfos, true);
			foreach (Row current in table)
			{
				yield return this.GetResearchProjectInfo(current);
			}
			yield break;
		}
		public ResearchProjectInfo GetResearchProjectInfo(int playerId, int projectId)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetResearchProjectInfo, playerId.ToSQLiteValue(), projectId.ToSQLiteValue()), true)[0];
			return this.GetResearchProjectInfo(row);
		}
		public PlayerTechInfo GetLastPlayerTechResearched(int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLastPlayerTechResearched, playerId), true);
			if (table.Rows.Count<Row>() == 0)
			{
				return null;
			}
			return this.GetPlayerTechInfo(table.Rows[0]);
		}
		private FeasibilityStudyInfo GetFeasibilityStudyInfo(Row row)
		{
			return new FeasibilityStudyInfo
			{
				ProjectID = row[0].SQLiteValueToInteger(),
				TechID = row[1].SQLiteValueToInteger(),
				ResearchCost = row[2].SQLiteValueToSingle()
			};
		}
		public FeasibilityStudyInfo GetFeasibilityStudyInfo(int projectId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetFeasibilityStudyInfo, projectId.ToSQLiteValue()), true);
			if (table == null || table.Rows.Length == 0)
			{
				return null;
			}
			return this.GetFeasibilityStudyInfo(table[0]);
		}
		public FeasibilityStudyInfo GetFeasibilityStudyInfo(int playerId, int techId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetFeasibilityStudyInfoForTech, playerId.ToSQLiteValue(), techId.ToSQLiteValue()), true);
			if (table == null || table.Rows.Length == 0)
			{
				return null;
			}
			return this.GetFeasibilityStudyInfo(table[0]);
		}
		public FeasibilityStudyInfo GetFeasibilityStudyInfoByPlayer(int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetFeasibilityStudyInfoForPlayer, playerId.ToSQLiteValue()), true);
			if (table == null || table.Rows.Length == 0)
			{
				return null;
			}
			return this.GetFeasibilityStudyInfo(table[0]);
		}
		public void UpdatePlayerCurrentTradeIncome(int playerId, double value)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCurrentTradeIncome, playerId, value.ToSQLiteValue()), false, true);
			this._dom.players.Sync(playerId);
		}
		public double GetPlayerCurrentTradeIncome(int playerId)
		{
			return this._dom.players[playerId].CurrentTradeIncome;
		}
		public void UpdatePlayerAdditionalResearchPoints(int playerId, int researchPoints)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerAdditionalResearchPoints, playerId, researchPoints), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdateResearchProjectState(int projectId, ProjectStates state)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateResearchProjectState, projectId.ToSQLiteValue(), ((int)state).ToSQLiteValue()), true);
		}
		private PlayerBranchInfo GetPlayerBranchInfo(Row row)
		{
			return new PlayerBranchInfo
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				FromTechID = row[1].SQLiteValueToInteger(),
				ToTechID = row[2].SQLiteValueToInteger(),
				ResearchCost = row[3].SQLiteValueToInteger(),
				Feasibility = row[4].SQLiteValueToSingle()
			};
		}
		public IEnumerable<PlayerBranchInfo> GetBranchesToTech(int playerId, int techId)
		{
			if (this._dom.player_tech_branches.IsDirty)
			{
				Table table = this.db.ExecuteTableQuery("SELECT player_id,from_id,to_id,research_cost,feasibility FROM player_tech_branches", true);
				foreach (Row current in table)
				{
					PlayerBranchInfo playerBranchInfo = this.GetPlayerBranchInfo(current);
					this._dom.player_tech_branches.Cache(playerBranchInfo.ID, playerBranchInfo);
				}
				this._dom.player_tech_branches.IsDirty = false;
			}
			return 
				from x in this._dom.player_tech_branches.Values
				where x.PlayerID == playerId && x.ToTechID == techId
				select x;
		}
		public IEnumerable<PlayerBranchInfo> GetUnlockedBranchesToTech(int playerId, int techId)
		{
			return 
				from x in this.GetBranchesToTech(playerId, techId)
				where this.GetPlayerTechInfo(playerId, x.FromTechID).State != TechStates.Locked
				select x;
		}
		public void UpdateResearchProjectInfo(ResearchProjectInfo rpi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateResearchProjectInfo, new object[]
			{
				rpi.ID.ToSQLiteValue(),
				rpi.PlayerID.ToSQLiteValue(),
				rpi.Name.ToSQLiteValue(),
				rpi.Progress.ToSQLiteValue(),
				((int)rpi.State).ToSQLiteValue()
			}), false, true);
		}
		public void UpdatePlayerTechState(int playerId, int techId, TechStates state)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerTechState, playerId.ToSQLiteValue(), techId.ToSQLiteValue(), ((int)state).ToSQLiteValue()), true);
			this._dom.player_techs.Clear();
		}
		public void UpdatePlayerTechInfo(PlayerTechInfo techInfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerTechInfo, new object[]
			{
				techInfo.PlayerID.ToSQLiteValue(),
				techInfo.TechID.ToSQLiteValue(),
				((int)techInfo.State).ToSQLiteValue(),
				techInfo.Progress.ToSQLiteValue(),
				techInfo.ResearchCost.ToSQLiteValue(),
				techInfo.Feasibility.ToSQLiteValue(),
				techInfo.PlayerFeasibility.ToSQLiteValue(),
				techInfo.TurnResearched.ToNullableSQLiteValue()
			}), false, true);
			this._dom.player_techs.Clear();
		}
		public HashSet<string> GetResearchedTechGroups(int playerId)
		{
			List<PlayerTechInfo> source = new List<PlayerTechInfo>(
				from x in this.GetPlayerTechInfos(playerId)
				where x.State == TechStates.Researched
				select x);
			HashSet<string> hashSet = new HashSet<string>();
			foreach (BasicNameField techGroup in this.AssetDatabase.MasterTechTree.TechGroups)
			{
				bool flag = false;
				foreach (Kerberos.Sots.Data.TechnologyFramework.Tech t in 
					from x in this.AssetDatabase.MasterTechTree.Technologies
					where x.Group == techGroup.Name
					select x)
				{
					if (source.Any((PlayerTechInfo y) => y.TechFileID == t.Id))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					hashSet.Add(techGroup.Name);
				}
			}
			return hashSet;
		}
		private bool TechMeetsRequirements(AssetDatabase assetdb, int playerId, int techId, HashSet<string> researchedGroups)
		{
			string techFileId = this.GetTechFileID(techId);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = assetdb.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techFileId);
			foreach (BasicNameField current in tech.Requires)
			{
				int techID = this.GetTechID(current.Name);
				if (techID == 0)
				{
					if (!researchedGroups.Contains(current.Name))
					{
						bool result = false;
						return result;
					}
				}
				else
				{
					PlayerTechInfo playerTechInfo = this.GetPlayerTechInfo(playerId, techID);
					if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
					{
						bool result = false;
						return result;
					}
				}
			}
			return true;
		}
		private bool CalcResearchCost(AssetDatabase assetdb, int playerId, int techId, out int researchCost, out float feasibility)
		{
			IEnumerable<PlayerBranchInfo> branchesToTech = this.GetBranchesToTech(playerId, techId);
			if (!branchesToTech.Any<PlayerBranchInfo>())
			{
				researchCost = 0;
				feasibility = 1f;
				return true;
			}
			researchCost = 2147483647;
			feasibility = 0f;
			foreach (PlayerBranchInfo current in branchesToTech)
			{
				PlayerTechInfo playerTechInfo = this.GetPlayerTechInfo(playerId, current.FromTechID);
				if (playerTechInfo.State == TechStates.Researched)
				{
					researchCost = Math.Min(researchCost, current.ResearchCost);
					feasibility = Math.Max(feasibility, current.Feasibility);
				}
			}
			return researchCost != 2147483647;
		}
		private void UnlockTechs(AssetDatabase assetdb, int playerId)
		{
			HashSet<string> researchedTechGroups = this.GetResearchedTechGroups(playerId);
			IEnumerable<PlayerTechInfo> enumerable = 
				from x in this.GetPlayerTechInfos(playerId)
				where x.State == TechStates.Locked
				select x;
			foreach (PlayerTechInfo current in enumerable)
			{
				int researchCost;
				float num;
				if (this.TechMeetsRequirements(assetdb, playerId, current.TechID, researchedTechGroups) && this.CalcResearchCost(assetdb, playerId, current.TechID, out researchCost, out num))
				{
					current.ResearchCost = researchCost;
					current.Feasibility = num;
					current.PlayerFeasibility = Math.Max(Math.Min(num, 1f), 0.01f);
					if (num >= 1f)
					{
						current.State = TechStates.Core;
					}
					else
					{
						current.State = TechStates.Branch;
					}
					this.UpdatePlayerTechInfo(current);
				}
			}
		}
		private bool AutoAcquireTechs(AssetDatabase assetdb, int playerId)
		{
			bool result = false;
			IEnumerable<PlayerTechInfo> playerTechInfos = this.GetPlayerTechInfos(playerId);
			foreach (PlayerTechInfo current in playerTechInfos)
			{
				if (current.ResearchCost <= 0 && current.State != TechStates.Researched && current.State != TechStates.Locked)
				{
					result = true;
					this.UpdatePlayerTechState(playerId, current.TechID, TechStates.Researched);
				}
			}
			return result;
		}
		public void UpdateLockedTechs(AssetDatabase assetdb, int playerId)
		{
			do
			{
				this.UnlockTechs(assetdb, playerId);
			}
			while (this.AutoAcquireTechs(assetdb, playerId));
		}
		public void AcquireAdditionalInitialTechs(AssetDatabase assetdb, int playerId, int numTechs)
		{
			if (numTechs <= 0)
			{
				return;
			}
			int i = 0;
			while (i < numTechs)
			{
				List<PlayerTechInfo> list = (
					from y in (
						from x in this.GetPlayerTechInfos(playerId)
						where x.State == TechStates.Core
						select x).ToList<PlayerTechInfo>()
					orderby y.ResearchCost
					select y).ToList<PlayerTechInfo>();
				if (list.Count == 0)
				{
					return;
				}
				foreach (PlayerTechInfo current in list)
				{
					current.Progress = 100;
					current.State = TechStates.Researched;
					this.UpdatePlayerTechInfo(current);
					i++;
					if (i >= numTechs)
					{
						break;
					}
				}
				this.UpdateLockedTechs(assetdb, playerId);
			}
		}
		public IEnumerable<PlayerTechInfo> GetPlayerTechInfos(int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerTechInfos, playerId.ToSQLiteValue()), true);
			foreach (Row current in table)
			{
				yield return this.GetPlayerTechInfo(current);
			}
			yield break;
		}
		public string GetTechFileID(int techId)
		{
			return this.db.ExecuteStringQuery(string.Format(Queries.GetTechFileID, techId.ToSQLiteValue()));
		}
		public int GetTechID(string techFileId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetTechID, techFileId.ToSQLiteValue()));
		}
		public IEnumerable<AITechWeightInfo> GetAITechWeightInfos(int playerID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAITechWeights, playerID.ToSQLiteValue()), true);
			foreach (Row current in table)
			{
				yield return new AITechWeightInfo
				{
					PlayerID = playerID,
					Family = current[0].SQLiteValueToString(),
					TotalSpent = current[1].SQLiteValueToDouble(),
					Weight = current[2].SQLiteValueToSingle()
				};
			}
			yield break;
		}
		public AITechWeightInfo GetAITechWeightInfo(int playerID, string family)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAITechWeight, family.ToSQLiteValue(), playerID.ToSQLiteValue()), true);
			if (table.Count<Row>() < 1)
			{
				return null;
			}
			Row row = table[0];
			return new AITechWeightInfo
			{
				PlayerID = playerID,
				Family = family,
				TotalSpent = row[0].SQLiteValueToDouble(),
				Weight = row[1].SQLiteValueToSingle()
			};
		}
		public void UpdateAITechWeight(AITechWeightInfo techInfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAITechWeight, new object[]
			{
				techInfo.Family.ToSQLiteValue(),
				techInfo.PlayerID.ToSQLiteValue(),
				techInfo.TotalSpent.ToSQLiteValue(),
				techInfo.Weight.ToSQLiteValue()
			}), false, true);
		}
		public void InsertAITechStyle(AITechStyleInfo style)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertAITechStyle, style.PlayerID.ToSQLiteValue(), ((int)style.TechFamily).ToSQLiteValue(), style.CostFactor.ToSQLiteValue()), false, true);
		}
		public IEnumerable<AITechStyleInfo> GetAITechStyles(int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAITechStyles, playerId.ToSQLiteValue()), true);
			if (table != null)
			{
				foreach (Row current in table)
				{
					yield return new AITechStyleInfo
					{
						PlayerID = current[0].SQLiteValueToInteger(),
						TechFamily = (TechFamilies)current[1].SQLiteValueToInteger(),
						CostFactor = current[2].SQLiteValueToSingle()
					};
				}
			}
			yield break;
		}
		public AIInfo GetAIInfo(int playerID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAIInfo, playerID.ToSQLiteValue()), true);
			if (table.Count<Row>() < 1)
			{
				return null;
			}
			Row row = table[0];
			return new AIInfo
			{
				PlayerID = playerID,
				PlayerInfo = this.GetPlayerInfo(playerID),
				Stance = (AIStance)row[0].SQLiteValueToInteger(),
				Flags = (AIInfoFlags)row[1].SQLiteValueToInteger()
			};
		}
		public IEnumerable<AIFleetInfo> GetAIFleetInfos(int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAIFleetInfos, playerId), true);
			foreach (Row current in table)
			{
				yield return new AIFleetInfo
				{
					ID = current[0].SQLiteValueToInteger(),
					FleetID = current[2].SQLiteValueToNullableInteger(),
					FleetType = current[3].SQLiteValueToInteger(),
					SystemID = current[4].SQLiteValueToInteger(),
					InvoiceID = current[5].SQLiteValueToNullableInteger(),
					AdmiralID = current[6].SQLiteValueToNullableInteger(),
					FleetTemplate = current[7].SQLiteValueToString()
				};
			}
			yield break;
		}
		public int InsertAIFleetInfo(int playerID, AIFleetInfo fleetInfo)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertAIFleetInfo, new object[]
			{
				playerID.ToSQLiteValue(),
				fleetInfo.FleetID.ToNullableSQLiteValue(),
				fleetInfo.FleetType.ToSQLiteValue(),
				fleetInfo.SystemID.ToSQLiteValue(),
				fleetInfo.InvoiceID.ToNullableSQLiteValue(),
				fleetInfo.AdmiralID.ToNullableSQLiteValue(),
				fleetInfo.FleetTemplate.ToSQLiteValue()
			}));
		}
		public void UpdateAIFleetInfo(AIFleetInfo fleetInfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAIFleetInfo, new object[]
			{
				fleetInfo.ID,
				fleetInfo.FleetID.ToNullableSQLiteValue(),
				fleetInfo.FleetType.ToSQLiteValue(),
				fleetInfo.SystemID.ToSQLiteValue(),
				fleetInfo.InvoiceID.ToNullableSQLiteValue(),
				fleetInfo.AdmiralID.ToNullableSQLiteValue()
			}), false, true);
		}
		public void UpdateAIInfo(AIInfo aiInfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAIInfo, aiInfo.PlayerID.ToSQLiteValue(), ((int)aiInfo.Stance).ToSQLiteValue(), ((int)aiInfo.Flags).ToSQLiteValue()), false, true);
		}
		public void RemoveAIFleetInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveAIInfo, id.ToSQLiteValue()), false, true);
			foreach (int current in (
				from x in this._dom.ships.Values
				where x.AIFleetID == id
				select x into y
				select y.ID).ToList<int>())
			{
				this._dom.ships.Sync(current);
			}
		}
		private static AIColonyIntel GetColonyIntel(Row row)
		{
			return new AIColonyIntel
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				OwningPlayerID = row[1].SQLiteValueToInteger(),
				PlanetID = row[2].SQLiteValueToInteger(),
				ColonyID = row[3].SQLiteValueToNullableInteger(),
				LastSeen = row[4].SQLiteValueToInteger(),
				ImperialPopulation = row[5].SQLiteValueToDouble()
			};
		}
		private AIColonyIntel GetColonyIntelWithFallback(int playerID, int? colonyID, int? planetID, Table table)
		{
			if (table.Rows.Length > 0)
			{
				return GameDatabase.GetColonyIntel(table[0]);
			}
			if (colonyID.HasValue)
			{
				this.GetColonyInfo(colonyID.Value);
			}
			else
			{
				if (planetID.HasValue)
				{
					this.GetColonyInfoForPlanet(planetID.Value);
				}
			}
			return null;
		}
		public IEnumerable<AIPlanetIntel> GetPlanetIntelsForPlayer(int playerID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelPlanetForPlayer, playerID.ToSQLiteValue()), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return GameDatabase.GetPlanetIntel(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<AIColonyIntel> GetColonyIntelsForPlayer(int playerID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelColonyForPlayer, playerID.ToSQLiteValue()), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return GameDatabase.GetColonyIntel(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public AIColonyIntel GetColonyIntel(int playerID, int colonyID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelColony, playerID.ToSQLiteValue(), colonyID.ToSQLiteValue()), true);
			int? colonyID2 = new int?(colonyID);
			int? planetID = null;
			Table table2 = table;
			return this.GetColonyIntelWithFallback(playerID, colonyID2, planetID, table2);
		}
		public AIColonyIntel GetColonyIntelForPlanet(int playerID, int planetID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelColonyForPlanet, playerID.ToSQLiteValue(), planetID.ToSQLiteValue()), true);
			int? colonyID = null;
			int? planetID2 = new int?(planetID);
			Table table2 = table;
			return this.GetColonyIntelWithFallback(playerID, colonyID, planetID2, table2);
		}
		public IEnumerable<AIColonyIntel> GetColonyIntelOfTargetPlayer(int playerID, int intelOnPlayerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetIntelColoniesOfTargetPlayer, playerID.ToSQLiteValue(), intelOnPlayerID.ToSQLiteValue()), true))
			{
				yield return GameDatabase.GetColonyIntel(current);
			}
			yield break;
		}
		private static AIPlanetIntel GetPlanetIntel(Row row)
		{
			return new AIPlanetIntel
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				PlanetID = row[1].SQLiteValueToInteger(),
				LastSeen = row[2].SQLiteValueToInteger(),
				Biosphere = row[3].SQLiteValueToInteger(),
				Resources = row[4].SQLiteValueToInteger(),
				Infrastructure = row[5].SQLiteValueToSingle(),
				Suitability = row[6].SQLiteValueToSingle()
			};
		}
		public AIPlanetIntel GetPlanetIntel(int playerID, int planet)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAIIntelPlanet, playerID.ToSQLiteValue(), planet.ToSQLiteValue()), true);
			if (table.Rows.Length == 0)
			{
				PlanetInfo planetInfo = this.GetPlanetInfo(planet);
				return new AIPlanetIntel
				{
					Biosphere = planetInfo.Biosphere,
					Infrastructure = planetInfo.Infrastructure,
					LastSeen = 0,
					PlanetID = planet,
					PlayerID = playerID,
					Resources = planetInfo.Resources,
					Suitability = planetInfo.Suitability
				};
			}
			return GameDatabase.GetPlanetIntel(table[0]);
		}
		public AIStationIntel GetStationIntel(int playerID, int stationID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelStation, playerID.ToSQLiteValue(), stationID.ToSQLiteValue()), true);
			if (table.Count<Row>() < 1)
			{
				return null;
			}
			Row row = table[0];
			return new AIStationIntel
			{
				PlayerID = playerID,
				IntelOnPlayerID = row[0].SQLiteValueToInteger(),
				StationID = stationID,
				LastSeen = row[1].SQLiteValueToInteger(),
				Level = row[2].SQLiteValueToInteger()
			};
		}
		public IEnumerable<AIStationIntel> GetStationIntelsOfTargetPlayer(int playerID, int intelOnPlayerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetIntelStationsOfTargetPlayer, playerID.ToSQLiteValue(), intelOnPlayerID.ToSQLiteValue()), true))
			{
				yield return new AIStationIntel
				{
					PlayerID = playerID,
					IntelOnPlayerID = intelOnPlayerID,
					StationID = current[0].SQLiteValueToInteger(),
					LastSeen = current[1].SQLiteValueToInteger(),
					Level = current[2].SQLiteValueToInteger()
				};
			}
			yield break;
		}
		public void UpdateStationIntel(int playerID, int stationID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateIntelStation, playerID.ToSQLiteValue(), stationID.ToSQLiteValue(), this.GetTurnCount().ToSQLiteValue()), false, true);
		}
		public void RemoveStationIntel(int playerID, int stationID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveIntelStation, playerID.ToSQLiteValue(), stationID.ToSQLiteValue()), false, true);
		}
		public IEnumerable<AIFleetIntel> GetFleetIntelsOfTargetPlayer(int playerID, int intelOnPlayerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetIntelFleetsOfTargetPlayer, playerID.ToSQLiteValue(), intelOnPlayerID.ToSQLiteValue()), true))
			{
				yield return new AIFleetIntel
				{
					PlayerID = playerID,
					IntelOnPlayerID = intelOnPlayerID,
					LastSeen = current[0].SQLiteValueToInteger(),
					LastSeenSystem = current[1].SQLiteValueToInteger(),
					LastSeenCoords = current[2].SQLiteValueToVector3(),
					NumDestroyers = current[3].SQLiteValueToInteger(),
					NumCruisers = current[4].SQLiteValueToInteger(),
					NumDreadnoughts = current[5].SQLiteValueToInteger(),
					NumLeviathans = current[6].SQLiteValueToInteger()
				};
			}
			yield break;
		}
		public IEnumerable<AIDesignIntel> GetDesignIntelsOfTargetPlayer(int playerID, int intelOnPlayerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetIntelDesignsOfTargetPlayer, playerID, intelOnPlayerID), true))
			{
				yield return new AIDesignIntel
				{
					PlayerID = playerID,
					IntelOnPlayerID = intelOnPlayerID,
					DesignID = current[0].SQLiteValueToInteger(),
					FirstSeen = current[1].SQLiteValueToInteger(),
					LastSeen = current[2].SQLiteValueToInteger(),
					Salvaged = current[3].SQLiteValueToBoolean()
				};
			}
			yield break;
		}
		public int GetSectionAssetID(string filepath, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetSectionAssetID, filepath.ToSQLiteValue(), playerId.ToSQLiteValue()));
		}
		public int GetModuleID(string module, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetModuleID, module.ToSQLiteValue(), playerId.ToSQLiteValue()));
		}
		public string GetModuleAsset(int moduleId)
		{
			return this.db.ExecuteStringQuery(string.Format(Queries.GetModuleAsset, moduleId.ToSQLiteValue()));
		}
		public int? GetWeaponID(string weapon, int playerId)
		{
			return this.db.ExecuteIntegerQueryDefault(string.Format(Queries.GetWeaponID, playerId.ToSQLiteValue(), weapon.ToSQLiteValue()), null);
		}
		public string GetWeaponAsset(int weaponId)
		{
			return this.db.ExecuteStringQuery(string.Format(Queries.GetWeaponAsset, weaponId.ToSQLiteValue()));
		}
		public string GetFactionName(int factionId)
		{
			return this.db.ExecuteStringQuery(string.Format(Queries.GetFactionName, factionId.ToSQLiteValue()));
		}
		public int GetFactionIdFromName(string factionName)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetFactionIdFromName, factionName.ToSQLiteValue()), true)[0];
			return row[0].SQLiteValueToInteger();
		}
		public FactionInfo ParseFactionInfo(Row row)
		{
			return PlayersCache.GetFactionInfoFromRow(row);
		}
		public FactionInfo GetFactionInfo(int factionId)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetFactionInfo, factionId.ToSQLiteValue()), true)[0];
			return this.ParseFactionInfo(row);
		}
		public float GetFactionSuitability(string faction)
		{
			string s = this.db.ExecuteTableQuery(string.Format(Queries.GetFactionSuitability, faction), true).Rows[0].Values[0];
			return float.Parse(s);
		}
		public float GetFactionSuitability(int factionId)
		{
			string s = this.db.ExecuteTableQuery(string.Format(Queries.GetFactionSuitabilityById, factionId), true).Rows[0].Values[0];
			return float.Parse(s);
		}
		public float GetPlayerSuitability(int playerID)
		{
			PlayerInfo playerInfo = this.GetPlayerInfo(playerID);
			string factionName = this.GetFactionName(playerInfo.FactionID);
			return this.GetFactionSuitability(factionName);
		}
		private PlayerInfo ParsePlayer(Row row)
		{
			return PlayersCache.GetPlayerInfoFromRow(this.db, row);
		}
		public void SyncTradeNodes(TradeResultsTable trt)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.ClearTradeResults, this.GetTurnCount() - 1), false, true);
			foreach (KeyValuePair<int, TradeNode> current in trt.TradeNodes)
			{
				this.db.ExecuteNonQuery(string.Format(Queries.InsertTradeResult, new object[]
				{
					current.Key.ToSQLiteValue(),
					current.Value.Produced.ToSQLiteValue(),
					current.Value.ProductionCapacity.ToSQLiteValue(),
					current.Value.Consumption.ToSQLiteValue(),
					current.Value.Freighters.ToSQLiteValue(),
					current.Value.DockCapacity.ToSQLiteValue(),
					current.Value.ExportInt.ToSQLiteValue(),
					current.Value.ExportProv.ToSQLiteValue(),
					current.Value.ExportLoc.ToSQLiteValue(),
					current.Value.ImportInt.ToSQLiteValue(),
					current.Value.ImportProv.ToSQLiteValue(),
					current.Value.ImportLoc.ToSQLiteValue(),
					current.Value.Range.ToSQLiteValue(),
					this.GetTurnCount()
				}), false, true);
			}
		}
		public TradeResultsTable GetTradeResultsTable()
		{
			TradeResultsTable tradeResultsTable = new TradeResultsTable();
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetTradeResults, this.GetTurnCount() - 1), true);
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				tradeResultsTable.TradeNodes.Add(row[0].SQLiteValueToInteger(), this.ParseTradeNode(row));
			}
			return tradeResultsTable;
		}
		public TradeResultsTable GetLastTradeResultsHistoryTable()
		{
			TradeResultsTable tradeResultsTable = new TradeResultsTable();
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetTradeResults, this.GetTurnCount() - 2), true);
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				tradeResultsTable.TradeNodes.Add(row[0].SQLiteValueToInteger(), this.ParseTradeNode(row));
			}
			return tradeResultsTable;
		}
		public TradeNode ParseTradeNode(Row r)
		{
			return new TradeNode
			{
				Produced = r[1].SQLiteValueToInteger(),
				ProductionCapacity = r[2].SQLiteValueToInteger(),
				Consumption = r[3].SQLiteValueToInteger(),
				Freighters = r[4].SQLiteValueToInteger(),
				DockCapacity = r[5].SQLiteValueToInteger(),
				ExportInt = r[6].SQLiteValueToInteger(),
				ExportProv = r[7].SQLiteValueToInteger(),
				ExportLoc = r[8].SQLiteValueToInteger(),
				ImportInt = r[9].SQLiteValueToInteger(),
				ImportProv = r[10].SQLiteValueToInteger(),
				ImportLoc = r[11].SQLiteValueToInteger(),
				Range = r[12].SQLiteValueToSingle(),
				Turn = r[13].SQLiteValueToInteger()
			};
		}
		public int GetPlayerBankruptcyTurns(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetBankruptcyTurns, playerId));
		}
		public void UpdatePlayerBankruptcyTurns(int playerId, int turn)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateBankruptcyTurns, playerId, turn), false, true);
		}
		public void UpdatePlayerIntelPoints(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerIntelPoints, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePlayerCounterintelPoints(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCounterintelPoints, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePlayerOperationsPoints(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerOperationsPoints, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePlayerIntelAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerIntelAccumulator, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePlayerCounterintelAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCounterintelAccumulator, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePlayerOperationsAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerOperationsAccumulator, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePlayerCivilianMiningAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCivilianMiningAccumulator, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePlayerCivilianColonizationAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCivilianColonizationAccumulator, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePlayerCivilianTradeAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCivilianTradeAccumulator, playerId, newValue), false, true);
			this._dom.players.Sync(playerId);
		}
		public Dictionary<int, int> GetFactionDiplomacyPoints(int playerId)
		{
			return PlayersCache.GetFactionDiplomacyPoints(this.db, playerId);
		}
		public IEnumerable<FactionInfo> GetFactions()
		{
			return PlayersCache.GetFactions(this.db);
		}
		public void UpdateFactionDiplomacyPoints(int playerId, int factionId, int newNumPoints)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFactionDiplomacyPoints, playerId.ToSQLiteValue(), factionId.ToSQLiteValue(), newNumPoints.ToSQLiteValue()), false, true);
		}
		public void UpdateGenericDiplomacyPoints(int playerId, int newNumPoints)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateGenericDiplomacyPoints, playerId.ToSQLiteValue(), newNumPoints.ToSQLiteValue()), false, true);
			this._dom.players.Sync(playerId);
		}
		public IEnumerable<PlayerInfo> GetStandardPlayerInfos()
		{
			return 
				from x in this.GetPlayerInfos()
				where x.isStandardPlayer
				select x;
		}
		public IEnumerable<PlayerInfo> GetIndyPlayerInfos()
		{
			List<Faction> indyFactions = (
				from x in this.assetdb.Factions
				where x.IsIndependent()
				select x).ToList<Faction>();
			return 
				from x in this.GetPlayerInfos()
				where !x.isStandardPlayer && indyFactions.Any((Faction y) => y.Name == x.Name)
				select x;
		}
		public IEnumerable<PlayerInfo> GetPlayerInfos()
		{
			return this._dom.players.Values;
		}
		public PlayerInfo GetPlayerInfo(int playerID)
		{
			if (playerID == 0 || !this._dom.players.ContainsKey(playerID))
			{
				return null;
			}
			return this._dom.players[playerID];
		}
		public int GetOrInsertIndyPlayerId(GameDatabase gamedb, int factionId, string factionName, string avatarPath = "")
		{
			List<PlayerInfo> source = this.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => x.FactionID == factionId && !x.isStandardPlayer && x.Name.Contains(factionName));
			if (playerInfo == null)
			{
				FactionInfo factionInfo = this.GetFactionInfo(factionId);
				return this.InsertPlayer(factionName, factionInfo.Name, null, Vector3.One, Vector3.One, "", avatarPath, 0.0, 0, false, true, false, 0, AIDifficulty.Normal);
			}
			return playerInfo.ID;
		}
		public int GetOrInsertIndyPlayerId(GameSession sim, int factionId, string factionName, string avatarPath = "")
		{
			List<PlayerInfo> source = this.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => x.FactionID == factionId && !x.isStandardPlayer && x.Name.Contains(factionName));
			if (playerInfo == null)
			{
				FactionInfo factionInfo = this.GetFactionInfo(factionId);
				int num = this.InsertPlayer(factionName, factionInfo.Name, null, Vector3.One, Vector3.One, "", avatarPath, 0.0, 0, false, true, false, 0, AIDifficulty.Normal);
				sim.AddPlayerObject(sim.GameDatabase.GetPlayerInfo(num), Player.ClientTypes.AI);
				return num;
			}
			return playerInfo.ID;
		}
		public void DuplicateStratModifiers(int newPlayer, int oldPlayer)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DuplicateStratModifiers, newPlayer, oldPlayer), false, true);
		}
		public void DuplicateTechs(int newPlayer, int oldPlayer)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DuplicateTechs, newPlayer, oldPlayer), false, true);
		}
		public void UpdatePsionicPotential(int playerId, int newPotential)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerPsionicPotential, playerId, newPotential), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdateLastEncounterTurn(int id, int turn)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLastEncounterTurn, id, turn), false, true);
			this._dom.players.Sync(id);
		}
		public void UpdatePlayerLastCombatTurn(int id, int turn)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCombatTurn, id, turn), false, true);
			this._dom.players.Sync(id);
		}
		public void UpdatePlayerSavings(int playerID, double savings)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerSavings, playerID, savings), true);
			this._dom.players.Sync(playerID);
		}
		public void UpdatePlayerResearchBoost(int playerID, double boostamount)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerResearchBoost, playerID, boostamount), true);
			this._dom.players.Sync(playerID);
		}
		public void UpdatePlayerAutoPlaceDefenses(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoPlaceDefenses, playerID, value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}
		public void UpdatePlayerAutoRepairFleets(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoRepairFleets, playerID, value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}
		public void UpdatePlayerAutoUseGoop(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoUseGoop, playerID, value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}
		public void UpdatePlayerAutoUseJoker(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoUseJoker, playerID, value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}
		public void UpdatePlayerAutoUseAOE(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoAOE, playerID, value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}
		public void UpdatePlayerTeam(int playerID, int team)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerTeam, playerID, team.ToSQLiteValue()), true);
		}
		public void UpdatePlayerAutoPatrol(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoPatrol, playerID, value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}
		public void UpdatePlayerHomeworld(int playerid, int planetid)
		{
			ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(planetid);
			if (colonyInfoForPlanet != null)
			{
				int arg_13_0 = colonyInfoForPlanet.PlayerID;
			}
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerHomeWorld, planetid, playerid), false, true);
			this._dom.players.Sync(playerid);
		}
		public void SetPlayerDefeated(GameSession game, int playerID)
		{
			PlayerInfo playerInfo = this.GetPlayerInfo(playerID);
			bool isDefeated = playerInfo.isDefeated;
			this.db.ExecuteNonQuery(string.Format(Queries.SetPlayerDefeated, playerID), false, true);
			this._dom.players.Sync(playerID);
			if (!isDefeated && playerInfo.isStandardPlayer)
			{
				foreach (int current in this.GetStandardPlayerIDs())
				{
					if (current != playerID)
					{
						this.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_EMPIRE_DESTROYED,
							EventMessage = TurnEventMessage.EM_EMPIRE_DESTROYED,
							TurnNumber = this.GetTurnCount(),
							PlayerID = current,
							TargetPlayerID = playerID
						});
					}
				}
			}
			List<SuulkaInfo> list = this.GetSuulkas().ToList<SuulkaInfo>();
			foreach (SuulkaInfo current2 in list)
			{
				if (current2.PlayerID.HasValue && current2.PlayerID.Value == playerID)
				{
					this.ReturnSuulka(game, current2.ID);
				}
			}
			List<FleetInfo> list2 = this.GetFleetInfosByPlayerID(playerID, FleetType.FL_ALL).ToList<FleetInfo>();
			foreach (FleetInfo current3 in list2)
			{
				this.RemoveFleet(current3.ID);
			}
			List<StationInfo> list3 = this.GetStationInfosByPlayerID(playerID).ToList<StationInfo>();
			foreach (StationInfo current4 in list3)
			{
				this.DestroyStation(game, current4.OrbitalObjectID, 0);
			}
			List<ColonyInfo> list4 = this.GetPlayerColoniesByPlayerId(playerID).ToList<ColonyInfo>();
			foreach (ColonyInfo current5 in list4)
			{
				this.RemoveColonyOnPlanet(current5.OrbitalObjectID);
			}
		}
		public void UpdatePlayerSliders(GameSession game, PlayerInfo p)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerSliderRates, new object[]
			{
				p.ID,
				p.RateGovernmentResearch,
				p.RateResearchCurrentProject,
				p.RateResearchSpecialProject,
				p.RateResearchSalvageResearch,
				p.RateGovernmentStimulus,
				p.RateGovernmentSecurity,
				p.RateGovernmentSavings,
				p.RateStimulusMining,
				p.RateStimulusColonization,
				p.RateStimulusTrade,
				p.RateSecurityOperations,
				p.RateSecurityIntelligence,
				p.RateSecurityCounterIntelligence
			}), true);
			this._dom.players.Sync(p.ID);
		}
		public void UpdateTaxRate(int playerId, float taxRate)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateTaxRate, playerId, taxRate), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdatePreviousTaxRate(int playerId, float taxRate)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePreviousTaxRate, playerId, taxRate), false, true);
			this._dom.players.Sync(playerId);
		}
		public void UpdateImmigrationRate(int playerId, float immigrationRate)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateImmigrationRate, playerId, immigrationRate), false, true);
			this._dom.players.Sync(playerId);
		}
		public IEnumerable<int> GetHomeWorldIDs()
		{
			return 
				from x in this._dom.players
				where x.Value.Homeworld.HasValue
				select x into y
				select y.Value.Homeworld.Value;
		}
		public GovernmentInfo GetGovernmentInfo(int playerID)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetGovernmentInfo, playerID), true)[0];
			return new GovernmentInfo
			{
				PlayerID = playerID,
				Authoritarianism = float.Parse(row[0]),
				EconomicLiberalism = float.Parse(row[1]),
				CurrentType = (GovernmentInfo.GovernmentType)Enum.Parse(typeof(GovernmentInfo.GovernmentType), row[2])
			};
		}
		public GovernmentActionInfo ParseGovernmentActionInfo(Row r)
		{
			return new GovernmentActionInfo
			{
				PlayerId = r[0].SQLiteValueToInteger(),
				ID = r[1].SQLiteValueToInteger(),
				Description = r[2].SQLiteValueToString(),
				AuthoritarianismChange = r[3].SQLiteValueToInteger(),
				EconLiberalismChange = r[4].SQLiteValueToInteger(),
				Turn = r[5].SQLiteValueToInteger()
			};
		}
		public IEnumerable<GovernmentActionInfo> GetGovernmentActions(int playerID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetGovernmentActions, playerID), true);
			foreach (Row current in table)
			{
				yield return this.ParseGovernmentActionInfo(current);
			}
			yield break;
		}
		public void InsertGovernmentAction(int playerId, string description, string govAction, int x = 0, int y = 0)
		{
			GovActionValues govActionValues = null;
			if (!string.IsNullOrEmpty(govAction))
			{
				govActionValues = this.assetdb.GetGovActionValues(govAction);
			}
			if (govActionValues == null)
			{
				govActionValues = new GovActionValues
				{
					XChange = x,
					YChange = y
				};
			}
			this.db.ExecuteNonQuery(string.Format(Queries.InsertGovernmentAction, new object[]
			{
				playerId,
				description,
				govActionValues.YChange,
				govActionValues.XChange,
				this.GetTurnCount(),
				this.assetdb.MaxGovernmentShift
			}), false, true);
		}
		public void UpdateGovernmentInfo(GovernmentInfo government)
		{
			float num = 1f;
			float num2 = -0.67f * (float)this.assetdb.MaxGovernmentShift;
			float num3 = 0.67f * (float)this.assetdb.MaxGovernmentShift;
			int num4 = 0;
			int num5 = 0;
			switch (government.CurrentType)
			{
			case GovernmentInfo.GovernmentType.Centrism:
				num4 = 1;
				num5 = 1;
				break;
			case GovernmentInfo.GovernmentType.Communalism:
				num4 = 2;
				num5 = 0;
				break;
			case GovernmentInfo.GovernmentType.Junta:
				num4 = 2;
				num5 = 1;
				break;
			case GovernmentInfo.GovernmentType.Plutocracy:
				num4 = 2;
				num5 = 2;
				break;
			case GovernmentInfo.GovernmentType.Socialism:
				num4 = 1;
				num5 = 0;
				break;
			case GovernmentInfo.GovernmentType.Mercantilism:
				num4 = 1;
				num5 = 2;
				break;
			case GovernmentInfo.GovernmentType.Cooperativism:
				num4 = 0;
				num5 = 0;
				break;
			case GovernmentInfo.GovernmentType.Anarchism:
				num4 = 0;
				num5 = 1;
				break;
			case GovernmentInfo.GovernmentType.Liberationism:
				num4 = 0;
				num5 = 2;
				break;
			}
			if (num4 == 0 && government.Authoritarianism > num2 + num)
			{
				num4 = 1;
			}
			else
			{
				if (num4 == 1 && government.Authoritarianism > num3 + num)
				{
					num4 = 2;
				}
				else
				{
					if (num4 == 2 && government.Authoritarianism < num3 - num)
					{
						num4 = 1;
					}
					else
					{
						if (num4 == 1 && government.Authoritarianism < num2 - num)
						{
							num4 = 0;
						}
					}
				}
			}
			if (num5 == 0 && government.EconomicLiberalism > num2 + num)
			{
				num5 = 1;
			}
			else
			{
				if (num5 == 1 && government.EconomicLiberalism > num3 + num)
				{
					num5 = 2;
				}
				else
				{
					if (num5 == 2 && government.EconomicLiberalism < num3 - num)
					{
						num5 = 1;
					}
					else
					{
						if (num5 == 1 && government.EconomicLiberalism < num2 - num)
						{
							num5 = 0;
						}
					}
				}
			}
			if (num4 == 0)
			{
				if (num5 == 0)
				{
					government.CurrentType = GovernmentInfo.GovernmentType.Cooperativism;
				}
				else
				{
					if (num5 == 1)
					{
						government.CurrentType = GovernmentInfo.GovernmentType.Anarchism;
					}
					else
					{
						government.CurrentType = GovernmentInfo.GovernmentType.Liberationism;
					}
				}
			}
			else
			{
				if (num4 == 1)
				{
					if (num5 == 0)
					{
						government.CurrentType = GovernmentInfo.GovernmentType.Socialism;
					}
					else
					{
						if (num5 == 1)
						{
							government.CurrentType = GovernmentInfo.GovernmentType.Centrism;
						}
						else
						{
							government.CurrentType = GovernmentInfo.GovernmentType.Mercantilism;
						}
					}
				}
				else
				{
					if (num5 == 0)
					{
						government.CurrentType = GovernmentInfo.GovernmentType.Communalism;
					}
					else
					{
						if (num5 == 1)
						{
							government.CurrentType = GovernmentInfo.GovernmentType.Junta;
						}
						else
						{
							government.CurrentType = GovernmentInfo.GovernmentType.Plutocracy;
						}
					}
				}
			}
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateGovernmentInfo, new object[]
			{
				government.PlayerID,
				government.Authoritarianism,
				government.EconomicLiberalism,
				government.CurrentType
			}), true);
		}
		public IEnumerable<int> GetStarSystemIDs()
		{
			this.CacheStarSystemInfos();
			return this._dom.star_systems.Keys;
		}
		public int? GetStarSystemProvinceID(int systemId)
		{
			this.CacheStarSystemInfos();
			StarSystemInfo starSystemInfo;
			if (!this._dom.star_systems.TryGetValue(systemId, out starSystemInfo))
			{
				return null;
			}
			return starSystemInfo.ProvinceID;
		}
		public Vector3 GetStarSystemOrigin(int systemId)
		{
			return Vector3.Parse(this.db.ExecuteStringQuery(string.Format(Queries.GetStarSystemOrigin, systemId)));
		}
		public void UpdateStarSystemOrigin(int systemId, Vector3 origin)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateStarSystemOrigin, systemId, origin.ToSQLiteValue()), false, true);
		}
		public IEnumerable<int> GetStarSystemOrbitalObjectIDs(int systemId)
		{
			return this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetStarSystemOrbitalObjectsBySystemId, systemId));
		}
		public static OrbitalObjectInfo GetOrbitalObjectInfo(SQLiteConnection db, int orbitalObjectID)
		{
			Table table = db.ExecuteTableQuery(string.Format(Queries.GetOrbitalObjectInfo, orbitalObjectID), true);
			if (table.Rows.Length == 0)
			{
				return null;
			}
			Row row = table[0];
			return new OrbitalObjectInfo
			{
				ID = orbitalObjectID,
				ParentID = row[0].SQLiteValueToNullableInteger(),
				StarSystemID = row[1].SQLiteValueToInteger(),
				OrbitalPath = OrbitalPath.Parse(row[2].SQLiteValueToString()),
				Name = row[3].SQLiteValueToString()
			};
		}
		public OrbitalObjectInfo GetOrbitalObjectInfo(int orbitalObjectID)
		{
			return GameDatabase.GetOrbitalObjectInfo(this.db, orbitalObjectID);
		}
		public void RemoveStarSystem(int systemId)
		{
			List<int> list = (
				from x in this._dom.missions.Values
				where x.StartingSystem == systemId
				select x into y
				select y.ID).ToList<int>();
			foreach (int current in list)
			{
				this._dom.missions.Sync(current);
			}
			List<int> list2 = (
				from x in this._dom.colony_traps.Values
				where x.SystemID == systemId
				select x into y
				select y.ID).ToList<int>();
			foreach (int current2 in list2)
			{
				this.RemoveColonyTrapInfo(current2);
			}
			List<NodeLineInfo> list3 = this.GetNodeLines().ToList<NodeLineInfo>();
			foreach (NodeLineInfo current3 in list3)
			{
				if (current3.System1ID == systemId || current3.System2ID == systemId)
				{
					List<int> list4 = this.GetFleetsForLoaLine(current3.ID).ToList<int>();
					foreach (int current4 in list4)
					{
						this.RemoveFleet(current4);
					}
					this.RemoveNodeLine(current3.ID);
				}
			}
			this._dom.star_systems.Clear();
			this._dom.fleets.Clear();
			this._dom.ships.Clear();
			this._dom.players.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveStarSystem, systemId), false, true);
		}
		public void DestroyStarSystem(GameSession game, int systemId)
		{
			StarSystemInfo ssi = this.GetStarSystemInfo(systemId);
			List<OrbitalObjectInfo> list = this.GetStarSystemOrbitalObjectInfos(systemId).OrderBy(delegate(OrbitalObjectInfo x)
			{
				if (!x.ParentID.HasValue)
				{
					return 1;
				}
				return 0;
			}).ToList<OrbitalObjectInfo>();
			foreach (OrbitalObjectInfo current in list)
			{
				this.DestroyOrbitalObject(game, current.ID);
			}
			List<FreighterInfo> list2 = this.GetFreighterInfosForSystem(systemId).ToList<FreighterInfo>();
			foreach (FreighterInfo current2 in list2)
			{
				this.RemoveFreighterInfo(current2.ID);
			}
			List<FleetInfo> list3 = this.GetFleetInfos(FleetType.FL_ALL).ToList<FleetInfo>();
			foreach (FleetInfo current3 in list3)
			{
				if (current3.SupportingSystemID == systemId)
				{
					if (!current3.IsNormalFleet)
					{
						this.RemoveFleet(current3.ID);
					}
					List<int> list4 = this.GetPlayerColonySystemIDs(current3.PlayerID).ToList<int>();
					list4.Remove(systemId);
					list4 = (
						from x in list4
						orderby (this.GetStarSystemOrigin(x) - ssi.Origin).Length
						select x).ToList<int>();
					if (list4.Count == 0)
					{
						this.RemoveFleet(current3.ID);
					}
					else
					{
						current3.SupportingSystemID = list4.First<int>();
						this.UpdateFleetInfo(current3);
					}
				}
			}
			list3 = this.GetFleetInfos(FleetType.FL_ALL).ToList<FleetInfo>();
			List<MoveOrderInfo> list5 = this.GetMoveOrderInfos().ToList<MoveOrderInfo>();
			foreach (MoveOrderInfo current4 in list5)
			{
				if (current4.ToSystemID == systemId)
				{
					this.RemoveMoveOrder(current4.ID);
					this.InsertMoveOrder(current4.FleetID, 0, this.GetFleetLocation(current4.FleetID, false).Coords, current4.FromSystemID, Vector3.Zero);
				}
				else
				{
					if (current4.FromSystemID == systemId)
					{
						this.InsertMoveOrder(current4.FleetID, 0, this.GetFleetLocation(current4.FleetID, false).Coords, current4.ToSystemID, Vector3.Zero);
						this.RemoveMoveOrder(current4.ID);
					}
				}
			}
			List<MissionInfo> list6 = this.GetMissionInfos().ToList<MissionInfo>();
			foreach (MissionInfo current5 in list6)
			{
				if (current5.TargetSystemID == systemId)
				{
					MissionInfo missionByFleetID = this.GetMissionByFleetID(current5.FleetID);
					if (missionByFleetID != null && missionByFleetID.ID != current5.ID)
					{
						this.RemoveMission(current5.ID);
						continue;
					}
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, this.GetFleetInfo(current5.FleetID), true);
				}
				List<WaypointInfo> list7 = this.GetWaypointsByMissionID(current5.ID).ToList<WaypointInfo>();
				using (List<WaypointInfo>.Enumerator enumerator6 = list7.GetEnumerator())
				{
					while (enumerator6.MoveNext())
					{
						if (enumerator6.Current.SystemID == systemId)
						{
                            Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, this.GetFleetInfo(current5.FleetID), true);
						}
					}
				}
			}
			foreach (FleetInfo current6 in list3)
			{
				if (current6.SystemID == systemId)
				{
					PlayerInfo playerInfo = this.GetPlayerInfo(current6.PlayerID);
					if (!current6.IsNormalFleet || playerInfo == null || !playerInfo.isStandardPlayer)
					{
						this.RemoveFleet(current6.ID);
					}
					else
					{
						List<StarSystemInfo> list8 = this.GetStarSystemInfos().ToList<StarSystemInfo>();
						list8.RemoveAll((StarSystemInfo x) => x.ID == systemId);
						list8 = (
							from x in list8
							orderby (x.Origin - ssi.Origin).Length
							select x).ToList<StarSystemInfo>();
                        Kerberos.Sots.StarFleet.StarFleet.ForceReturnMission(this, current6);
						this.InsertMoveOrder(current6.ID, 0, this.GetFleetLocation(current6.ID, false).Coords, list8.First<StarSystemInfo>().ID, Vector3.Zero);
						this.UpdateFleetLocation(current6.ID, 0, null);
						this.UpdateFleetInfo(current6);
					}
				}
			}
			List<RetrofitOrderInfo> list9 = this.GetRetrofitOrdersForSystem(systemId).ToList<RetrofitOrderInfo>();
			foreach (RetrofitOrderInfo current7 in list9)
			{
				this.RemoveRetrofitOrder(current7.ID, true, false);
			}
			list3 = this.GetFleetInfos(FleetType.FL_LIMBOFLEET | FleetType.FL_TRAP).ToList<FleetInfo>();
			foreach (FleetInfo current8 in list3)
			{
				if (current8.SystemID == systemId)
				{
					if (current8.Type == FleetType.FL_TRAP)
					{
						ColonyTrapInfo colonyTrapInfoByFleetID = this.GetColonyTrapInfoByFleetID(current8.ID);
						if (colonyTrapInfoByFleetID != null)
						{
							this.RemoveColonyTrapInfo(colonyTrapInfoByFleetID.ID);
						}
					}
					this.RemoveFleet(current8.ID);
				}
			}
			if (ssi.ProvinceID.HasValue)
			{
				this.RemoveProvince(ssi.ProvinceID.Value);
			}
			this.RemoveStarSystem(systemId);
		}
		public void RemoveOrbitalObject(int orbitalObjectID)
		{
			this.RemoveColonyOnPlanet(orbitalObjectID);
			ColonyTrapInfo colonyTrapInfoByPlanetID = this.GetColonyTrapInfoByPlanetID(orbitalObjectID);
			if (colonyTrapInfoByPlanetID != null)
			{
				this.RemoveColonyTrapInfo(colonyTrapInfoByPlanetID.ID);
			}
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalObjectID);
			if (orbitalObjectInfo != null)
			{
				List<OrbitalObjectInfo> list = this.GetChildOrbitals(orbitalObjectID).ToList<OrbitalObjectInfo>();
				foreach (OrbitalObjectInfo current in list)
				{
					this.RemoveOrbitalObject(current.ID);
				}
			}
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveOrbitalObject, orbitalObjectID), false, true);
			this._dom.players.Clear();
		}
        public void DestroyOrbitalObject(GameSession game, int orbitalObjectID)
        {
            Func<HomeworldInfo, bool> predicate = null;
            Func<ColonyInfo, double> keySelector = null;
            List<StationInfo> stations = this.GetStationInfos().Where<StationInfo>(delegate(StationInfo x)
            {
                int? parentID = x.OrbitalObjectInfo.ParentID;
                int num1 = orbitalObjectID;
                return ((parentID.GetValueOrDefault() == num1) && parentID.HasValue);
            }).ToList<StationInfo>();
            foreach (StationInfo info in stations)
            {
                this.DestroyStation(game, info.OrbitalObjectID, 0);
            }
            ColonyInfo colony = this.GetColonyInfoForPlanet(orbitalObjectID);
            if (colony != null)
            {
                Func<ColonyInfo, bool> func = null;
                this.RemoveColonyOnPlanet(orbitalObjectID);
                List<HomeworldInfo> source = this.GetHomeworlds().ToList<HomeworldInfo>();
                if (predicate == null)
                {
                    predicate = x => x.ColonyID == colony.ID;
                }
                HomeworldInfo hw = source.FirstOrDefault<HomeworldInfo>(predicate);
                if (hw != null)
                {
                    if (func == null)
                    {
                        func = x => x.PlayerID == hw.PlayerID;
                    }
                    List<ColonyInfo> list2 = this.GetColonyInfos().ToList<ColonyInfo>().Where<ColonyInfo>(func).ToList<ColonyInfo>();
                    if (keySelector == null)
                    {
                        keySelector = x => this.GetTotalPopulation(x);
                    }
                    list2.OrderBy<ColonyInfo, double>(keySelector);
                    ColonyInfo info2 = list2.FirstOrDefault<ColonyInfo>();
                    if (info2 != null)
                    {
                        hw.ColonyID = info2.ID;
                        hw.SystemID = this.GetOrbitalObjectInfo(info2.OrbitalObjectID).StarSystemID;
                        this.UpdateHomeworldInfo(hw);
                    }
                }
            }
            foreach (MissionInfo info3 in this.GetMissionInfos().ToList<MissionInfo>().Where<MissionInfo>(delegate(MissionInfo x)
            {
                if (x.TargetOrbitalObjectID != orbitalObjectID)
                {
                    return stations.Any<StationInfo>(y => (y.OrbitalObjectID == x.TargetOrbitalObjectID));
                }
                return true;
            }).ToList<MissionInfo>())
            {
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, this.GetFleetInfo(info3.FleetID), true);
            }
            if (this.GetOrbitalObjectInfo(orbitalObjectID) != null)
            {
                foreach (OrbitalObjectInfo info5 in this.GetChildOrbitals(orbitalObjectID).ToList<OrbitalObjectInfo>())
                {
                    this.DestroyOrbitalObject(game, info5.ID);
                }
            }
            this.RemoveOrbitalObject(orbitalObjectID);
        }
        public IEnumerable<int> GetAlliancePlayersByAllianceId(int allianceId)
		{
			return this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetAlliancePlayersByAllianceId, allianceId));
		}
		public int? GetSystemOwningPlayer(int systemId)
		{
			List<ColonyInfo> list = this.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>();
			ColonyInfo colonyInfo = list.FirstOrDefault((ColonyInfo x) => x.OwningColony);
			if (colonyInfo != null)
			{
				return new int?(colonyInfo.PlayerID);
			}
			double num = 0.0;
			foreach (ColonyInfo current in list)
			{
				if (current.ImperialPop > num)
				{
					num = current.ImperialPop;
					colonyInfo = current;
				}
			}
			if (colonyInfo != null)
			{
				return new int?(colonyInfo.PlayerID);
			}
			List<StationInfo> source = this.GetStationForSystem(systemId).ToList<StationInfo>();
			if (source.Any((StationInfo x) => x.DesignInfo.StationType == StationType.NAVAL))
			{
				StationInfo stationInfo = source.FirstOrDefault((StationInfo x) => x.DesignInfo.StationType == StationType.NAVAL);
				if (stationInfo != null)
				{
					return new int?(stationInfo.PlayerID);
				}
			}
			return null;
		}
		public bool hasPermissionToBuildEnclave(int playerId, int orbitalObject)
		{
			int ssid = this.GetOrbitalObjectInfo(orbitalObject).StarSystemID;
			List<RequestInfo> source = this.GetRequestInfos().ToList<RequestInfo>();
			int? owningPlayer = this.GetSystemOwningPlayer(ssid);
			if (!owningPlayer.HasValue)
			{
				return true;
			}
			if (owningPlayer.Value == playerId)
			{
				return true;
			}
			PlayerInfo playerInfo = this.GetPlayerInfo(owningPlayer.Value);
			if (playerInfo.includeInDiplomacy && this.GetDiplomacyStateBetweenPlayers(owningPlayer.Value, playerId) != DiplomacyState.WAR)
			{
				return true;
			}
			RequestInfo requestInfo = source.FirstOrDefault((RequestInfo x) => x.Type == RequestType.EstablishEnclaveRequest && x.InitiatingPlayer == playerId && x.ReceivingPlayer == owningPlayer && x.RequestValue == (float)ssid);
			return requestInfo != null;
		}
		public IEnumerable<ColonyInfo> GetColonyInfos()
		{
			return this._dom.colonies.Values;
		}
		public ColonyInfo GetColonyInfo(int colonyID)
		{
			if (colonyID == 0 || !this._dom.colonies.ContainsKey(colonyID))
			{
				return null;
			}
			return this._dom.colonies[colonyID];
		}
		public IEnumerable<int> GetPlayerColonySystemIDs(int playerid)
		{
			return (
				from x in this._dom.colonies
				where x.Value.PlayerID == playerid
				select x into y
				select y.Value.CachedStarSystemID).Distinct<int>();
		}
		public IEnumerable<ColonyInfo> GetPlayerColoniesByPlayerId(int playerId)
		{
			return 
				from x in this._dom.colonies.Values
				where x.PlayerID == playerId
				select x;
		}
		public IEnumerable<ColonyInfo> GetColonyInfosForProvince(int provinceID)
		{
			try
			{
				int[] array = this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetColonyIDsForProvince, provinceID.ToSQLiteValue()));
				for (int i = 0; i < array.Length; i++)
				{
					int key = array[i];
					yield return this._dom.colonies[key];
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<ColonyInfo> GetColonyInfosForSystem(int systemID)
		{
			return 
				from x in this._dom.colonies.Values
				where x.CachedStarSystemID == systemID
				select x;
		}
		public ColonyInfo GetColonyInfoForPlanet(int orbitalObjectID)
		{
			return this._dom.colonies.Values.FirstOrDefault((ColonyInfo x) => x.OrbitalObjectID == orbitalObjectID);
		}
		public string GetColonyName(int orbitalObjectID)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalObjectID);
			return orbitalObjectInfo.Name;
		}
		public void UpdateColony(ColonyInfo colony)
		{
			this._dom.colonies.Update(colony.ID, colony);
		}
		public void RemoveColonyOnPlanet(int planetID)
		{
			ColonyInfo colonyInfo = this.GetColonyInfoForPlanet(planetID);
			if (colonyInfo != null)
			{
				ColonyFactionInfo[] factions = colonyInfo.Factions;
				for (int i = 0; i < factions.Length; i++)
				{
					ColonyFactionInfo colonyFactionInfo = factions[i];
					this.RemoveCivilianPopulation(colonyInfo.OrbitalObjectID, colonyFactionInfo.FactionID);
				}
				this._dom.colonies.Remove(colonyInfo.ID);
				OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
				if (orbitalObjectInfo != null)
				{
					int starSystemID = orbitalObjectInfo.StarSystemID;
					List<ColonyInfo> list = this.GetColonyInfosForSystem(starSystemID).ToList<ColonyInfo>();
					if (list.Count == 0)
					{
						foreach (InvoiceInstanceInfo current in this.GetInvoicesForSystem(colonyInfo.PlayerID, starSystemID).ToList<InvoiceInstanceInfo>())
						{
							this.RemoveInvoiceInstance(current.ID);
						}
						StarSystemInfo starSystemInfo = this.GetStarSystemInfo(starSystemID);
						if (starSystemInfo.ProvinceID.HasValue)
						{
							this.RemoveProvince(starSystemInfo.ProvinceID.Value);
						}
						int? reserveFleetID = this.GetReserveFleetID(colonyInfo.PlayerID, starSystemID);
						if (reserveFleetID.HasValue)
						{
							List<int> list2 = this.GetShipsByFleetID(reserveFleetID.Value).ToList<int>();
							foreach (int current2 in list2)
							{
								this.RemoveShip(current2);
							}
							this.RemoveFleet(reserveFleetID.Value);
							return;
						}
					}
					else
					{
						if (colonyInfo.OwningColony)
						{
							ColonyInfo colonyInfo2 = colonyInfo;
							double num = 0.0;
							foreach (ColonyInfo current3 in list)
							{
								if (current3.ImperialPop > num)
								{
									num = current3.ImperialPop;
									colonyInfo = current3;
								}
							}
							colonyInfo.OwningColony = true;
							if (colonyInfo.PlayerID != colonyInfo2.PlayerID)
							{
								StarSystemInfo starSystemInfo2 = this.GetStarSystemInfo(this.GetOrbitalObjectInfo(colonyInfo2.OrbitalObjectID).StarSystemID);
								if (starSystemInfo2.ProvinceID.HasValue)
								{
									this.RemoveProvince(starSystemInfo2.ProvinceID.Value);
								}
								foreach (InvoiceInstanceInfo current4 in this.GetInvoicesForSystem(colonyInfo2.PlayerID, starSystemID).ToList<InvoiceInstanceInfo>())
								{
									this.RemoveInvoiceInstance(current4.ID);
								}
							}
							this.UpdateColony(colonyInfo);
						}
					}
				}
			}
		}
		public PlagueInfo ParsePlagueInfo(Row r)
		{
			return new PlagueInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				ColonyId = r[1].SQLiteValueToInteger(),
				PlagueType = (WeaponEnums.PlagueType)r[2].SQLiteValueToInteger(),
				InfectionRate = r[3].SQLiteValueToSingle(),
				InfectedPopulationImperial = r[4].SQLiteValueToDouble(),
				InfectedPopulationCivilian = r[5].SQLiteValueToDouble(),
				LaunchingPlayer = r[6].SQLiteValueToInteger()
			};
		}
		public int InsertPlagueInfo(PlagueInfo pi)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertPlague, new object[]
			{
				pi.ColonyId.ToSQLiteValue(),
				((int)pi.PlagueType).ToSQLiteValue(),
				pi.InfectionRate.ToSQLiteValue(),
				pi.InfectedPopulationImperial.ToSQLiteValue(),
				pi.InfectedPopulationCivilian.ToSQLiteValue(),
				pi.LaunchingPlayer.ToSQLiteValue()
			}));
		}
		public void UpdatePlagueInfo(PlagueInfo pi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlague, new object[]
			{
				pi.ID,
				pi.ColonyId,
				(int)pi.PlagueType,
				pi.InfectionRate,
				pi.InfectedPopulationImperial,
				pi.InfectedPopulationCivilian,
				pi.LaunchingPlayer
			}), false, true);
		}
		public IEnumerable<PlagueInfo> GetPlagueInfoByColony(int colonyId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPlagueInfosByColony, colonyId), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row r = rows[i];
					yield return this.ParsePlagueInfo(r);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void RemovePlagueInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemovePlague, id), false, true);
		}
		public void RemoveFeasibilityStudy(int projectId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveFeasibilityStudy, projectId), false, true);
		}
		public FreighterInfo ParseFreighterInfo(Row r)
		{
			return new FreighterInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				SystemId = r[1].SQLiteValueToInteger(),
				PlayerId = r[2].SQLiteValueToInteger(),
				Design = this.GetDesignInfo(r[3].SQLiteValueToInteger()),
				ShipId = (r[4] != null) ? r[4].SQLiteValueToInteger() : 0,
				IsPlayerBuilt = r[5] != null && r[5].SQLiteValueToBoolean()
			};
		}
		public IEnumerable<FreighterInfo> GetFreighterInfosForSystem(int systemID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetFreighterInfosForSystem, systemID), true);
			foreach (Row current in table)
			{
				yield return this.ParseFreighterInfo(current);
			}
			yield break;
		}
		public IEnumerable<FreighterInfo> GetFreighterInfosBuiltByPlayer(int playerID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetFreighterInfosBuiltByPlayer, playerID), true);
			foreach (Row current in table)
			{
				yield return this.ParseFreighterInfo(current);
			}
			yield break;
		}
		public void UpdateFreighterInfo(FreighterInfo fi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFreighterInfo, fi.ID, fi.SystemId, fi.PlayerId), false, true);
		}
		public void RemoveFreighterInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveFreighterInfo, id), false, true);
		}
		public int InsertFreighterInfo(int SystemId, int PlayerId, ShipInfo ship, bool isPlayerBuilt)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertFreighterInfo, new object[]
			{
				SystemId,
				PlayerId,
				ship.DesignID,
				ship.ID,
				isPlayerBuilt.ToSQLiteValue()
			}));
		}
		public int InsertFreighter(int SystemId, int PlayerId, int DesignId, bool isPlayerBuilt)
		{
			int num = this.InsertShip(this.InsertOrGetLimboFleetID(SystemId, PlayerId), DesignId, null, (ShipParams)0, null, 0);
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertFreighterInfo, new object[]
			{
				SystemId,
				PlayerId,
				DesignId,
				num,
				isPlayerBuilt.ToSQLiteValue()
			}));
		}
		public IEnumerable<StationInfo> GetStationForSystemAndPlayer(int systemID, int playerID)
		{
			return 
				from x in this._dom.stations.Values
				where x.OrbitalObjectInfo.StarSystemID == systemID && x.PlayerID == playerID
				select x;
		}
		public IEnumerable<StationInfo> GetStationForSystem(int systemID)
		{
			return 
				from x in this._dom.stations.Values
				where x.OrbitalObjectInfo.StarSystemID == systemID
				select x;
		}
		public IEnumerable<StationInfo> GetStationInfos()
		{
			return this._dom.stations.Values;
		}
		public IEnumerable<StationInfo> GetStationInfosByPlayerID(int playerID)
		{
			return 
				from x in this._dom.stations.Values
				where x.PlayerID == playerID
				select x;
		}
		public bool GetStationRequiresSupport(StationType type)
		{
			return type == StationType.CIVILIAN || type == StationType.DIPLOMATIC || type == StationType.NAVAL || type == StationType.SCIENCE;
		}
		public int GetNumberMaxStationsSupportedBySystem(GameSession game, int systemID, int playerID)
		{
			if (!this.IsSurveyed(playerID, systemID) || (this.GetSystemOwningPlayer(systemID).HasValue && !(this.GetSystemOwningPlayer(systemID) == playerID) && !StarMapState.SystemHasIndependentColony(game, systemID)))
			{
				return 0;
			}
			if (!this.GetStarSystemPlanetInfos(systemID).Any<PlanetInfo>())
			{
				return 0;
			}
			List<ColonyInfo> list = (
				from x in this.GetColonyInfosForSystem(systemID)
				where x.PlayerID == playerID
				select x).ToList<ColonyInfo>();
			double num = 0.0;
			foreach (ColonyInfo current in list)
			{
				num += this.GetTotalPopulation(current);
			}
			return Math.Min(Math.Max((int)Math.Ceiling(num / (double)game.AssetDatabase.StationsPerPop), 1), 4);
		}
		public StationInfo GetStationForSystemPlayerAndType(int systemID, int playerID, StationType type)
		{
			return this._dom.stations.Values.FirstOrDefault((StationInfo x) => x.OrbitalObjectInfo.StarSystemID == systemID && x.PlayerID == playerID && x.DesignInfo.StationType == type);
		}
		public StationInfo GetNavalStationForSystemAndPlayer(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.NAVAL);
		}
		public StationInfo GetScienceStationForSystemAndPlayer(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.SCIENCE);
		}
		public StationInfo GetCivilianStationForSystemAndPlayer(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.CIVILIAN);
		}
		public StationInfo GetDiplomaticStationForSystemAndPlayer(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.DIPLOMATIC);
		}
		public StationInfo GetHiverGateForSystem(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.GATE);
		}
		public void RemoveStation(int stationID)
		{
			this._dom.stations.Remove(stationID);
			this._dom.CachedSystemHasGateFlags.Clear();
		}
		public void DestroyStation(GameSession game, int stationID, int ignoreMissionID = 0)
		{
			List<MissionInfo> list = (
				from x in game.GameDatabase.GetMissionInfos()
				where x.ID != ignoreMissionID && x.TargetOrbitalObjectID == stationID
				select x).ToList<MissionInfo>();
			foreach (MissionInfo current in list)
			{
                Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, game.GameDatabase.GetFleetInfo(current.FleetID), false);
			}
			StationInfo stationInfo = game.GameDatabase.GetStationInfo(stationID);
			if (stationInfo != null && stationInfo.DesignInfo.StationType == StationType.CIVILIAN)
			{
				OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(stationID);
				if (orbitalObjectInfo != null)
				{
					if (!game.GameDatabase.GetStationForSystemAndPlayer(orbitalObjectInfo.StarSystemID, stationInfo.DesignInfo.PlayerID).Any((StationInfo x) => x.ID != stationID && x.DesignInfo.StationType == StationType.CIVILIAN))
					{
						List<FreighterInfo> list2 = game.GameDatabase.GetFreighterInfosForSystem(orbitalObjectInfo.StarSystemID).ToList<FreighterInfo>();
						foreach (FreighterInfo current2 in list2)
						{
							game.GameDatabase.RemoveFreighterInfo(current2.ID);
						}
					}
				}
			}
			this.RemoveStation(stationID);
			this.RemoveOrbitalObject(stationID);
		}
		private static AdmiralInfo GetAdmiralInfo(Row row)
		{
			return new AdmiralInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToOneBasedIndex(),
				HomeworldID = row[2].SQLiteValueToNullableInteger(),
				Name = row[3].SQLiteValueToString(),
				Race = row[4].SQLiteValueToString(),
				Age = row[5].SQLiteValueToSingle(),
				Gender = row[6].SQLiteValueToString(),
				ReactionBonus = row[7].SQLiteValueToInteger(),
				EvasionBonus = row[8].SQLiteValueToInteger(),
				Loyalty = row[9].SQLiteValueToInteger(),
				BattlesFought = row[10].SQLiteValueToInteger(),
				BattlesWon = row[11].SQLiteValueToInteger(),
				MissionsAssigned = row[12].SQLiteValueToInteger(),
				MissionsAccomplished = row[13].SQLiteValueToInteger(),
				TurnCreated = row[14].SQLiteValueToInteger(),
				Engram = row[15].SQLiteValueToBoolean()
			};
		}
		public AdmiralInfo GetAdmiralInfo(int admiralID)
		{
			if (admiralID == 0)
			{
				return null;
			}
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAdmiral, admiralID), true);
			if (table.Count<Row>() <= 0)
			{
				return null;
			}
			return GameDatabase.GetAdmiralInfo(table[0]);
		}
		public IEnumerable<AdmiralInfo> GetAdmiralInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetAdmirals, new object[0]), true))
			{
				yield return GameDatabase.GetAdmiralInfo(current);
			}
			yield break;
		}
		public IEnumerable<AdmiralInfo> GetAdmiralInfosForPlayer(int playerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetAdmiralsForPlayer, playerID), true))
			{
				yield return GameDatabase.GetAdmiralInfo(current);
			}
			yield break;
		}
		public void UpdateAdmiralInfo(AdmiralInfo admiral)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAdmiralInfo, new object[]
			{
				admiral.ID,
				admiral.PlayerID,
				admiral.Age,
				admiral.ReactionBonus,
				admiral.EvasionBonus,
				admiral.Loyalty,
				admiral.BattlesFought,
				admiral.BattlesWon,
				admiral.MissionsAssigned,
				admiral.MissionsAccomplished
			}), false, true);
		}
		public void UpdateEngram(int id, bool engram)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAdmiralIsEngram, id.ToSQLiteValue(), engram.ToSQLiteValue()), false, true);
		}
		public void RemoveAdmiral(int admiralID)
		{
			this._dom.fleets.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveAdmiral, admiralID), false, true);
		}
		public void AddAdmiralTrait(int admiralID, AdmiralInfo.TraitType trait, int level)
		{
			if (this.GetAdmiralInfo(admiralID).Engram)
			{
				return;
			}
			if (this.GetLevelForAdmiralTrait(admiralID, trait) > 0)
			{
				this.UpdateAdmiralTrait(admiralID, trait, level);
				return;
			}
			this.db.ExecuteNonQuery(string.Format(Queries.AddAdmiralTrait, admiralID, (int)trait, level), false, true);
		}
		public void ClearAdmiralTraits(int admiralID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.ClearAdmiralTraits, admiralID), false, true);
		}
		public void UpdateAdmiralTrait(int admiralID, AdmiralInfo.TraitType trait, int level)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAdmiralTrait, admiralID, (int)trait, level), false, true);
		}
		public int GetLevelForAdmiralTrait(int admiralID, AdmiralInfo.TraitType type)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetLevelForAdmiralTrait, admiralID, (int)type));
		}
		public IEnumerable<AdmiralInfo.TraitType> GetAdmiralTraits(int admiralID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetAdmiralTraits, admiralID), true))
			{
				yield return (AdmiralInfo.TraitType)current[0].SQLiteValueToInteger();
			}
			yield break;
		}
		public float GetPlanetHazardRating(int playerId, int planetId, bool useIntel = false)
		{
			float idealSuitability = this.GetFactionInfo(this.GetPlayerFactionID(playerId)).IdealSuitability;
			float num = useIntel ? this.GetPlanetIntel(playerId, planetId).Suitability : this.GetPlanetInfo(planetId).Suitability;
			float value = num - idealSuitability;
			if (this.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerId))
			{
				value = 0f;
			}
			return Math.Abs(value);
		}
		public float GetNonAbsolutePlanetHazardRating(int playerId, int planetId, bool useIntel = false)
		{
			float idealSuitability = this.GetFactionInfo(this.GetPlayerFactionID(playerId)).IdealSuitability;
			float num = useIntel ? this.GetPlanetIntel(playerId, planetId).Suitability : this.GetPlanetInfo(planetId).Suitability;
			return num - idealSuitability;
		}
		public double GetCivilianPopulation(int orbitalId, int factionId, bool splitSlaves)
		{
			double result;
			if (splitSlaves)
			{
				result = (
					from x in this.GetCivilianPopulations(orbitalId).ToList<ColonyFactionInfo>()
					where x.FactionID == factionId
					select x).Sum((ColonyFactionInfo x) => x.CivilianPop);
			}
			else
			{
				result = this.GetCivilianPopulations(orbitalId).ToList<ColonyFactionInfo>().Sum((ColonyFactionInfo x) => x.CivilianPop);
			}
			return result;
		}
		public double GetSlavePopulation(int orbitalId, int factionid)
		{
			return (
				from x in this.GetCivilianPopulations(orbitalId)
				where x.FactionID != factionid
				select x).Sum((ColonyFactionInfo x) => x.CivilianPop);
		}
		public double GetTotalPopulation(ColonyInfo ci)
		{
			Faction faction = this.AssetDatabase.GetFaction(this.GetPlayerFactionID(ci.PlayerID));
			return this.GetCivilianPopulation(ci.OrbitalObjectID, faction.ID, faction.HasSlaves()) + ci.ImperialPop;
		}
		public IEnumerable<ColonyFactionInfo> GetCivilianPopulations(int orbitalObjectID)
		{
			return ColoniesCache.GetColonyFactionInfosFromOrbitalObjectID(this.db, orbitalObjectID);
		}
		public void RemoveCivilianPopulation(int orbitalObjectID, int factionId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveCivilianPopulation, orbitalObjectID, factionId), false, true);
			this._dom.colonies.SyncRange((
				from x in this._dom.colonies
				where x.Value.OrbitalObjectID == orbitalObjectID
				select x into y
				select y.Key).ToList<int>());
		}
		public void UpdateCivilianPopulation(ColonyFactionInfo civPop)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateColonyFaction, new object[]
			{
				civPop.OrbitalObjectID,
				civPop.FactionID,
				civPop.CivilianPop,
				civPop.Morale,
				civPop.CivPopWeight,
				civPop.LastMorale
			}), false, true);
			this._dom.colonies.SyncRange((
				from x in this._dom.colonies
				where x.Value.OrbitalObjectID == civPop.OrbitalObjectID
				select x into y
				select y.Key).ToList<int>());
		}
		public int CountMoons(int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalId);
			IEnumerable<OrbitalObjectInfo> starSystemOrbitalObjectInfos = this.GetStarSystemOrbitalObjectInfos(orbitalObjectInfo.StarSystemID);
			IEnumerable<int> planets = this.GetStarSystemPlanets(orbitalObjectInfo.StarSystemID);
			IEnumerable<OrbitalObjectInfo> source = 
				from x in starSystemOrbitalObjectInfos
				where x.ParentID.HasValue && x.ParentID.Value == orbitalId && planets.Contains(x.ID)
				select x;
			return source.Count<OrbitalObjectInfo>();
		}
		public IEnumerable<OrbitalObjectInfo> GetMoons(int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalId);
			IEnumerable<OrbitalObjectInfo> starSystemOrbitalObjectInfos = this.GetStarSystemOrbitalObjectInfos(orbitalObjectInfo.StarSystemID);
			IEnumerable<int> planets = this.GetStarSystemPlanets(orbitalObjectInfo.StarSystemID);
			return 
				from x in starSystemOrbitalObjectInfos
				where x.ParentID.HasValue && x.ParentID.Value == orbitalId && planets.Contains(x.ID)
				select x;
		}
		public IEnumerable<OrbitalObjectInfo> GetChildOrbitals(int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalId);
			IEnumerable<OrbitalObjectInfo> starSystemOrbitalObjectInfos = this.GetStarSystemOrbitalObjectInfos(orbitalObjectInfo.StarSystemID);
			return 
				from x in starSystemOrbitalObjectInfos
				where x.ParentID.HasValue && x.ParentID.Value == orbitalId
				select x;
		}
		public IEnumerable<int> GetStarSystemPlanets(int systemId)
		{
			return this.db.ExecuteIntegerArrayQuery(string.Format("SELECT id FROM planets,orbital_objects WHERE planets.orbital_object_id == orbital_objects.id AND orbital_objects.star_system_id == {0};", systemId));
		}
		public List<int> ParseCombatZoneString(string value)
		{
			if (value == null)
			{
				return null;
			}
			List<int> list = new List<int>();
			string text = value.SQLiteValueToString();
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			string[] array = text.Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(int.Parse(array[i]));
			}
			return list;
		}
		public StarSystemInfo ParseStarSystemInfo(Row row)
		{
			return new StarSystemInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				ProvinceID = row[1].SQLiteValueToNullableInteger(),
				Name = row[2].SQLiteValueToString(),
				StellarClass = row[3].SQLiteValueToString(),
				IsVisible = row[4].SQLiteValueToBoolean(),
				TerrainID = row[5].SQLiteValueToNullableInteger(),
				ControlZones = this.ParseCombatZoneString(row[6]),
				IsOpen = row[7].SQLiteValueToBoolean(),
				Origin = row[9].SQLiteValueToVector3()
			};
		}
		private void CacheStarSystemInfos()
		{
			if (this._dom.star_systems.IsDirty)
			{
				this._dom.star_systems.Clear();
				foreach (Row current in this.db.ExecuteTableQuery(Queries.GetStarSystemInfos, true))
				{
					StarSystemInfo starSystemInfo = this.ParseStarSystemInfo(current);
					this._dom.star_systems[starSystemInfo.ID] = starSystemInfo;
				}
				this._dom.star_systems.IsDirty = false;
			}
		}
		public StarSystemInfo GetStarSystemInfo(int systemId)
		{
			if (systemId == 0)
			{
				return null;
			}
			this.CacheStarSystemInfos();
			StarSystemInfo result;
			this._dom.star_systems.TryGetValue(systemId, out result);
			return result;
		}
		public IEnumerable<StarSystemInfo> GetStarSystemInfos()
		{
			this.CacheStarSystemInfos();
			return 
				from x in this._dom.star_systems.Values
				where x.StellarClass != "Deepspace"
				select x;
		}
		public IEnumerable<StarSystemInfo> GetDeepspaceStarSystemInfos()
		{
			this.CacheStarSystemInfos();
			return 
				from x in this._dom.star_systems.Values
				where x.StellarClass == "Deepspace"
				select x;
		}
		public IEnumerable<StarSystemInfo> GetVisibleStarSystemInfos(int playerId)
		{
			foreach (StarSystemInfo current in this.GetStarSystemInfos())
			{
				if (!current.IsVisible)
				{
					ExploreRecordInfo exploreRecord = this.GetExploreRecord(current.ID, playerId);
					if (exploreRecord != null && exploreRecord.Visible)
					{
						yield return current;
					}
				}
				else
				{
					yield return current;
				}
			}
			yield break;
		}
		public bool IsStarSystemVisibleToPlayer(int playerId, int systemId)
		{
			ExploreRecordInfo exploreRecord = this.GetExploreRecord(systemId, playerId);
			return this.GetStarSystemInfo(systemId).IsVisible || (exploreRecord != null && exploreRecord.Visible);
		}
		public LargeAsteroidInfo ParseLargeAsteroidInfo(Row row)
		{
			return new LargeAsteroidInfo
			{
				ID = int.Parse(row[0]),
				Resources = int.Parse(row[1])
			};
		}
		public LargeAsteroidInfo GetLargeAsteroidInfo(int orbitalObjectId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLargeAsteroidInfo, orbitalObjectId), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseLargeAsteroidInfo(table.Rows[0]);
			}
			return null;
		}
		public IEnumerable<LargeAsteroidInfo> GetLargeAsteroidsInAsteroidBelt(int orbitalObjectId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLargeAsteroidInfoInAsteroidBelt, orbitalObjectId), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseLargeAsteroidInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<AsteroidBeltInfo> GetStarSystemAsteroidBeltInfos(int systemId)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetStarSystemAsteroidBeltInfos, systemId), true))
			{
				yield return this.ParseAsteroidBeltInfo(current);
			}
			yield break;
		}
		public AsteroidBeltInfo ParseAsteroidBeltInfo(Row r)
		{
			return new AsteroidBeltInfo
			{
				ID = int.Parse(r[0]),
				RandomSeed = int.Parse(r[1])
			};
		}
		public AsteroidBeltInfo GetAsteroidBeltInfo(int objectId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAsteroidBeltInfo, objectId), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseAsteroidBeltInfo(table[0]);
			}
			return null;
		}
		public SpecialProjectInfo ParseSpecialProjectInfo(Row r)
		{
			return new SpecialProjectInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerID = r[1].SQLiteValueToOneBasedIndex(),
				Name = r[2].SQLiteValueToString(),
				Progress = r[3].SQLiteValueToInteger(),
				Cost = r[4].SQLiteValueToInteger(),
				Type = (SpecialProjectType)r[5].SQLiteValueToInteger(),
				TechID = r[6].SQLiteValueToOneBasedIndex(),
				Rate = r[7].SQLiteValueToSingle(),
				EncounterID = r[8].SQLiteValueToOneBasedIndex(),
				FleetID = r[9].SQLiteValueToOneBasedIndex(),
				TargetPlayerID = r[10].SQLiteValueToOneBasedIndex()
			};
		}
		public StationInfo GetStationInfo(int orbitalObjectID)
		{
			if (!this._dom.stations.ContainsKey(orbitalObjectID))
			{
				return null;
			}
			return this._dom.stations[orbitalObjectID];
		}
		public PlanetInfo ParsePlanetInfo(Row row)
		{
			return new PlanetInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				Type = row[1].SQLiteValueToString(),
				RingID = row[2].SQLiteValueToNullableInteger(),
				Suitability = row[3].SQLiteValueToSingle(),
				Biosphere = row[4].SQLiteValueToInteger(),
				Resources = row[5].SQLiteValueToInteger(),
				Size = row[6].SQLiteValueToSingle(),
				Infrastructure = row[7].SQLiteValueToSingle(),
				MaxResources = row[8].SQLiteValueToInteger()
			};
		}
		public PlanetInfo[] GetStarSystemPlanetInfos(int systemId)
		{
			return (
				from row in this.db.ExecuteTableQuery(string.Format(Queries.GetStarSystemPlanetInfos, systemId), true)
				select this.ParsePlanetInfo(row)).ToArray<PlanetInfo>();
		}
		public IEnumerable<PlanetInfo> GetPlanetInfosOrbitingStar(int systemId)
		{
			IEnumerable<int> starSystemPlanets = this.GetStarSystemPlanets(systemId);
			IEnumerable<OrbitalObjectInfo> orbitals = this.GetStarSystemOrbitalObjectInfos(systemId);
			foreach (int current in 
				from p in starSystemPlanets
				where !orbitals.First((OrbitalObjectInfo o) => o.ID == p).ParentID.HasValue
				select p)
			{
				yield return this.GetPlanetInfo(current);
			}
			yield break;
		}
		public PlanetInfo GetPlanetInfo(int orbitalObjectID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPlanetInfo, orbitalObjectID), true);
			if (table.Count<Row>() == 0)
			{
				return null;
			}
			Row row = table[0];
			return this.ParsePlanetInfo(row);
		}
		public void UpdatePlanet(PlanetInfo planet)
		{
			planet.Suitability = Math.Min(Math.Max(Constants.MinSuitability, planet.Suitability), Constants.MaxSuitability);
			planet.Infrastructure = Math.Min(Math.Max(Constants.MinInfrastructure, planet.Infrastructure), Constants.MaxInfrastructure);
			planet.Biosphere = Math.Max(planet.Biosphere, 0);
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlanet, new object[]
			{
				planet.ID,
				planet.Suitability,
				planet.Biosphere,
				planet.Resources,
				planet.Infrastructure,
				planet.Size,
				planet.MaxResources
			}), true);
		}
		public void UpdateOrbitalObjectInfo(OrbitalObjectInfo info)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateOrbitalObjectInfo, new object[]
			{
				info.ID,
				info.ParentID.HasValue ? info.ParentID.ToString() : "NULL",
				info.StarSystemID,
				info.OrbitalPath,
				info.Name
			}), false, true);
		}
		public void UpdatePlanetInfrastructure(int orbitalObjectID, float infrastructure)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlanetInfrastructure, orbitalObjectID, infrastructure), false, true);
		}
		public IEnumerable<OrbitalObjectInfo> GetStarSystemOrbitalObjectInfos(int systemId)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetStarSystemOrbitalObjectInfos, systemId), true))
			{
				yield return new OrbitalObjectInfo
				{
					ID = int.Parse(current[0]),
					ParentID = (current[1] != null) ? new int?(int.Parse(current[1])) : null,
					StarSystemID = systemId,
					OrbitalPath = OrbitalPath.Parse(current[2]),
					Name = current[3]
				};
			}
			yield break;
		}
		public int GetLastTurnExploredByPlayer(int playerID, int systemID)
		{
			ExploreRecordInfo exploreRecordInfo = this._dom.explore_records.Values.FirstOrDefault((ExploreRecordInfo x) => x.SystemId == systemID && x.PlayerId == playerID);
			if (exploreRecordInfo != null)
			{
				return exploreRecordInfo.LastTurnExplored;
			}
			return 0;
		}
		public void UpdateExploreRecord(ExploreRecordInfo eri)
		{
			this._dom.explore_records.Update(ExploreRecordsCache.GetRecordKey(eri), eri);
		}
		public void SetStarSystemVisible(int systemId, bool visible)
		{
			this._dom.star_systems.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.SetStarSystemVisible, systemId, visible), false, true);
		}
		public void InsertExploreRecord(int systemId, int playerId, int lastExplored, bool visible = true, bool explored = true)
		{
			this._dom.explore_records.Insert(null, new ExploreRecordInfo
			{
				SystemId = systemId,
				PlayerId = playerId,
				LastTurnExplored = lastExplored,
				Explored = explored,
				Visible = visible
			});
		}
		public ExploreRecordInfo GetExploreRecord(int StarSystemId, int PlayerId)
		{
			return this._dom.explore_records.Values.FirstOrDefault((ExploreRecordInfo x) => x.SystemId == StarSystemId && x.PlayerId == PlayerId);
		}
		public ProvinceInfo GetProvinceInfo(int provinceId)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetProvinceInfo, provinceId), true)[0];
			return new ProvinceInfo
			{
				ID = int.Parse(row[0]),
				PlayerID = int.Parse(row[1]),
				Name = row[2],
				CapitalSystemID = int.Parse(row[3])
			};
		}
		public void UpdateProvinceInfo(ProvinceInfo pi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateProvinceInfo, new object[]
			{
				pi.ID,
				pi.PlayerID,
				pi.Name,
				pi.CapitalSystemID
			}), false, true);
		}
		public TerrainInfo ParseTerrainInfo(Row row)
		{
			return new TerrainInfo
			{
				ID = int.Parse(row[0]),
				Name = row[1],
				Origin = Vector3.Parse(row[2])
			};
		}
		public IEnumerable<TerrainInfo> GetTerrainInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(Queries.GetTerrainInfos, true))
			{
				yield return this.ParseTerrainInfo(current);
			}
			yield break;
		}
		public IEnumerable<ProvinceInfo> GetProvinceInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(Queries.GetProvinceInfos, true))
			{
				yield return new ProvinceInfo
				{
					ID = int.Parse(current[0]),
					PlayerID = int.Parse(current[1]),
					Name = current[2],
					CapitalSystemID = int.Parse(current[3])
				};
			}
			yield break;
		}
		public int GetProvinceCapitalSystemID(int provinceId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetProvinceCapitalSystemID, provinceId));
		}
		private FleetInfo GetFleetInfo(Row row)
		{
			if (row == null)
			{
				return null;
			}
			return new FleetInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToInteger(),
				AdmiralID = row[2].SQLiteValueToOneBasedIndex(),
				SystemID = row[3].SQLiteValueToOneBasedIndex(),
				SupportingSystemID = row[4].SQLiteValueToOneBasedIndex(),
				Name = row[5].SQLiteValueToString(),
				TurnsAway = row[6].SQLiteValueToInteger(),
				SupplyRemaining = row[7].SQLiteValueToSingle(),
				Type = (FleetType)row[8].SQLiteValueToInteger(),
				PreviousSystemID = row[9].SQLiteValueToNullableInteger(),
				Preferred = row[10].SQLiteValueToBoolean(),
				LastTurnAccelerated = row[11].SQLiteValueToInteger(),
				FleetConfigID = row[12].SQLiteValueToNullableInteger()
			};
		}
		private void TryRemoveFleetShip(int shipId, int fleetId)
		{
			List<int> list = null;
			if (this._dom.fleetShips.TryGetValue(fleetId, out list))
			{
				list.Remove(shipId);
			}
		}
		private void TryAddFleetShip(int shipId, int fleetId)
		{
			if (fleetId != 0)
			{
				List<int> list = null;
				if (!this._dom.fleetShips.TryGetValue(fleetId, out list))
				{
					list = new List<int>();
					this._dom.fleetShips[fleetId] = list;
				}
				list.Add(shipId);
			}
		}
		private void CacheFleetInfos()
		{
			if (this._dom.fleets.IsDirty)
			{
				this._dom.fleets.Clear();
				foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetFleetInfos, 479.ToSQLiteValue()), true))
				{
					FleetInfo fleetInfo = this.GetFleetInfo(current);
					this._dom.fleets[fleetInfo.ID] = fleetInfo;
				}
				this._dom.fleetShips.Clear();
				foreach (ShipInfo current2 in this._dom.ships.Values)
				{
					this.TryAddFleetShip(current2.ID, current2.FleetID);
				}
				this._dom.fleets.IsDirty = false;
			}
		}
		public IEnumerable<FleetInfo> GetFleetInfos(FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return 
				from x in this._dom.fleets.Values
				where (x.Type & filter) != (FleetType)0
				select x;
		}
		public FleetInfo GetFleetInfo(int fleetID)
		{
			if (fleetID == 0)
			{
				return null;
			}
			this.CacheFleetInfos();
			FleetInfo result;
			this._dom.fleets.TryGetValue(fleetID, out result);
			return result;
		}
		public FleetInfo GetFleetInfo(int fleetID, FleetType filter)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo == null || (fleetInfo.Type & filter) == (FleetType)0)
			{
				return null;
			}
			return fleetInfo;
		}
		public IEnumerable<FleetInfo> GetFleetInfoBySystemID(int systemID, FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return 
				from x in this._dom.fleets.Values
				where x.SystemID == systemID && (x.Type & filter) != (FleetType)0
				select x;
		}
		public FleetInfo GetFleetInfoByFleetName(string name, FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.FirstOrDefault((FleetInfo x) => name == x.Name && (x.Type & filter) != (FleetType)0);
		}
		public IEnumerable<FleetInfo> GetFleetInfosByPlayerID(int playerID, FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return 
				from x in this._dom.fleets.Values
				where x.PlayerID == playerID && (x.Type & filter) != (FleetType)0
				select x;
		}
		public int GetFleetFaction(int fleetID)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo == null)
			{
				return 0;
			}
			return this.GetPlayerInfo(fleetInfo.PlayerID).FactionID;
		}
		public FleetInfo GetFleetInfoByAdmiralID(int admiralID, FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.FirstOrDefault((FleetInfo x) => x.AdmiralID == admiralID && (x.Type & filter) != (FleetType)0);
		}
		public IEnumerable<FleetInfo> GetFleetsBySupportingSystem(int systemId, FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return 
				from x in this._dom.fleets.Values
				where x.SupportingSystemID == systemId && (x.Type & filter) != (FleetType)0
				select x;
		}
		public IEnumerable<FleetInfo> GetFleetsByPlayerAndSystem(int playerID, int systemID, FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return 
				from x in this._dom.fleets.Values
				where x.PlayerID == playerID && x.SystemID == systemID && (x.Type & filter) != (FleetType)0
				select x;
		}
		public int InsertReserveFleet(int playerID, int systemID)
		{
			return this.InsertFleetCore(playerID, 0, systemID, 0, App.Localize("@FLEET_RESERVE_NAME"), FleetType.FL_RESERVE);
		}
		public int InsertDefenseFleet(int playerID, int systemID)
		{
			return this.InsertFleetCore(playerID, 0, systemID, 0, App.Localize("@FLEET_DEFENSE_NAME"), FleetType.FL_DEFENSE);
		}
		public int? GetReserveFleetID(int playerID, int systemID)
		{
			FleetInfo fleetInfo = this.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_RESERVE).FirstOrDefault<FleetInfo>();
			if (fleetInfo == null)
			{
				return null;
			}
			return new int?(fleetInfo.ID);
		}
		private int InsertOrGetReserveFleetID(int systemID, int playerID)
		{
			int? reserveFleetID = this.GetReserveFleetID(playerID, systemID);
			if (!reserveFleetID.HasValue)
			{
				reserveFleetID = new int?(this.InsertReserveFleet(playerID, systemID));
			}
			return reserveFleetID.Value;
		}
		public int? GetDefenseFleetID(int systemID, int playerID)
		{
			FleetInfo fleetInfo = this.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_DEFENSE).FirstOrDefault<FleetInfo>();
			if (fleetInfo == null)
			{
				return null;
			}
			return new int?(fleetInfo.ID);
		}
		private int InsertOrGetDefenseFleetID(int systemID, int playerID)
		{
			FleetInfo fleetInfo = this.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_DEFENSE).FirstOrDefault<FleetInfo>();
			int result;
			if (fleetInfo == null)
			{
				result = this.InsertDefenseFleet(playerID, systemID);
			}
			else
			{
				result = fleetInfo.ID;
			}
			return result;
		}
		public FleetInfo InsertOrGetReserveFleetInfo(int systemID, int playerID)
		{
			return this.GetFleetInfo(this.InsertOrGetReserveFleetID(systemID, playerID));
		}
		public FleetInfo InsertOrGetDefenseFleetInfo(int systemID, int playerID)
		{
			return this.GetFleetInfo(this.InsertOrGetDefenseFleetID(systemID, playerID));
		}
		public FleetInfo GetDefenseFleetInfo(int systemID, int playerID)
		{
			int? defenseFleetID = this.GetDefenseFleetID(systemID, playerID);
			if (!defenseFleetID.HasValue)
			{
				return null;
			}
			return this.GetFleetInfo(defenseFleetID.Value);
		}
		public List<int> GetNPCPlayersBySystem(int systemId)
		{
			List<int> list = new List<int>();
			List<FleetInfo> list2 = this.GetFleetInfoBySystemID(systemId, FleetType.FL_ALL).ToList<FleetInfo>();
			foreach (FleetInfo current in list2)
			{
				PlayerInfo playerInfo = this.GetPlayerInfo(current.PlayerID);
				if (playerInfo != null && !playerInfo.isStandardPlayer)
				{
					list.Add(playerInfo.ID);
				}
			}
			return list;
		}
		public IEnumerable<MissionInfo> GetMissionsBySystemDest(int systemID)
		{
			return 
				from x in this._dom.missions.Values
				where x.TargetSystemID == systemID
				select x;
		}
		public IEnumerable<MissionInfo> GetMissionsByPlanetDest(int orbitalObjectID)
		{
			return 
				from x in this._dom.missions.Values
				where x.TargetOrbitalObjectID == orbitalObjectID
				select x;
		}
		public FleetLocation GetFleetLocation(int fleetID, bool gapAroundStars = false)
		{
			FleetLocation fleetLocation = new FleetLocation();
			fleetLocation.FleetID = fleetID;
			MoveOrderInfo moveOrderInfoByFleetID = this.GetMoveOrderInfoByFleetID(fleetID);
			if (moveOrderInfoByFleetID != null)
			{
				Vector3 vector = Vector3.Zero;
				Vector3 vector2 = Vector3.Zero;
				if (moveOrderInfoByFleetID.FromSystemID == 0)
				{
					vector = moveOrderInfoByFleetID.FromCoords;
				}
				else
				{
					StarSystemInfo starSystemInfo = this.GetStarSystemInfo(moveOrderInfoByFleetID.FromSystemID);
					vector = starSystemInfo.Origin;
				}
				if (moveOrderInfoByFleetID.ToSystemID == 0)
				{
					vector2 = moveOrderInfoByFleetID.ToCoords;
				}
				else
				{
					StarSystemInfo starSystemInfo2 = this.GetStarSystemInfo(moveOrderInfoByFleetID.ToSystemID);
					vector2 = starSystemInfo2.Origin;
				}
				Vector3 vector3 = vector2 - vector;
				float length = vector3.Length;
				if (gapAroundStars && length >= 0.01f)
				{
					float num = 0.1f;
					float num2 = length - num * 2f;
					if (num2 < 0f)
					{
						num2 = 0f;
					}
					float s = (num2 * moveOrderInfoByFleetID.Progress + num) / length;
					vector3 *= s;
				}
				else
				{
					vector3 *= moveOrderInfoByFleetID.Progress;
				}
				fleetLocation.Coords = vector + vector3;
				if (moveOrderInfoByFleetID.Progress == 0f)
				{
					fleetLocation.SystemID = moveOrderInfoByFleetID.FromSystemID;
				}
				else
				{
					fleetLocation.SystemID = 0;
				}
				Vector3 value = vector2 - vector;
				if (value.Length >= 0.01f)
				{
					fleetLocation.Direction = new Vector3?(Vector3.Normalize(value));
					fleetLocation.FromSystemCoords = new Vector3?(vector);
					fleetLocation.ToSystemCoords = new Vector3?(vector2);
				}
				else
				{
					fleetLocation.Direction = null;
				}
			}
			else
			{
				FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
				if (fleetInfo != null)
				{
					fleetLocation.SystemID = fleetInfo.SystemID;
					StarSystemInfo starSystemInfo3 = this.GetStarSystemInfo(fleetInfo.SystemID);
					if (starSystemInfo3 != null)
					{
						fleetLocation.Coords = starSystemInfo3.Origin;
					}
					fleetLocation.Direction = null;
				}
			}
			return fleetLocation;
		}
		public bool FleetHasCurvatureComp(FleetInfo fi)
		{
			int curveId = this.GetTechID("DRV_Curvature_Compensator");
			List<ShipInfo> list = this.GetShipInfoByFleetID(fi.ID, true).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				if (current.ParentID == 0)
				{
					if (!current.DesignInfo.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.FileName.Contains("antimatter_enhanced") || x.ShipSectionAsset.FileName.Contains("enhancedantimatter") || x.Techs.Any((int y) => y == curveId)))
					{
						return false;
					}
				}
			}
			return true;
		}
		public FleetLocation GetLiirFleetLocation(int fleetID)
		{
			FleetLocation fleetLocation = new FleetLocation();
			fleetLocation.FleetID = fleetID;
			MoveOrderInfo moveOrderInfoByFleetID = this.GetMoveOrderInfoByFleetID(fleetID);
			if (moveOrderInfoByFleetID != null)
			{
				Vector3 arg_1D_0 = Vector3.Zero;
				Vector3 arg_23_0 = Vector3.Zero;
				if (moveOrderInfoByFleetID.FromSystemID == 0)
				{
					Vector3 arg_32_0 = moveOrderInfoByFleetID.FromCoords;
				}
				else
				{
					StarSystemInfo starSystemInfo = this.GetStarSystemInfo(moveOrderInfoByFleetID.FromSystemID);
					Vector3 arg_48_0 = starSystemInfo.Origin;
				}
				if (moveOrderInfoByFleetID.ToSystemID == 0)
				{
					Vector3 arg_57_0 = moveOrderInfoByFleetID.ToCoords;
				}
				else
				{
					StarSystemInfo starSystemInfo2 = this.GetStarSystemInfo(moveOrderInfoByFleetID.ToSystemID);
					Vector3 arg_6D_0 = starSystemInfo2.Origin;
				}
			}
			else
			{
				FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
				fleetLocation.SystemID = fleetInfo.SystemID;
				StarSystemInfo starSystemInfo3 = this.GetStarSystemInfo(fleetInfo.SystemID);
				fleetLocation.Coords = starSystemInfo3.Origin;
			}
			return fleetLocation;
		}
		public IEnumerable<SuulkaInfo> GetSuulkas()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetSuulkas, new object[0]), true))
			{
				yield return this.GetSuulkaInfo(current);
			}
			yield break;
		}
		public IEnumerable<SuulkaInfo> GetPlayerSuulkas(int? playerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerSuulkas, playerID.ToNullableSQLiteValue()), true))
			{
				yield return this.GetSuulkaInfo(current);
			}
			yield break;
		}
		public SuulkaInfo GetSuulkaByShipID(int shipID)
		{
			Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSuulkaByShipID, shipID.ToSQLiteValue()), true);
			if (source.Count<Row>() > 0)
			{
				return this.GetSuulkaInfo(source.First<Row>());
			}
			return null;
		}
		public SuulkaInfo GetSuulkaByAdmiralID(int admiralID)
		{
			Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSuulkaByAdmiralID, admiralID.ToSQLiteValue()), true);
			if (source.Count<Row>() > 0)
			{
				return this.GetSuulkaInfo(source.First<Row>());
			}
			return null;
		}
		public SuulkaInfo GetSuulkaByStationID(int stationID)
		{
			Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSuulkaByStationID, stationID.ToSQLiteValue()), true);
			if (source.Count<Row>() > 0)
			{
				return this.GetSuulkaInfo(source.First<Row>());
			}
			return null;
		}
		public SuulkaInfo GetSuulka(int suulkaID)
		{
			Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSuulka, suulkaID.ToSQLiteValue()), true);
			if (source.Count<Row>() > 0)
			{
				return this.GetSuulkaInfo(source.First<Row>());
			}
			return null;
		}
		public void UpdateSuulkaPlayer(int suulkaID, int playerID)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateSuulkaPlayer, suulkaID.ToSQLiteValue(), playerID.ToSQLiteValue()), true);
		}
		public void UpdateSuulkaArrivalTurns(int suulkaID, int turns)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateSuulkaArrivalTurns, suulkaID.ToSQLiteValue(), turns.ToSQLiteValue()), true);
		}
		public void UpdateSuulkaStation(int suulkaID, int stationID)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateSuulkaStation, suulkaID.ToSQLiteValue(), stationID.ToSQLiteValue()), true);
		}
		public void RemoveSuulka(int suulkaID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSuulka, suulkaID.ToSQLiteValue()), false, true);
		}
		public void ReturnSuulka(GameSession game, int suulkaID)
		{
			SuulkaInfo suulka = this.GetSuulka(suulkaID);
			if (suulka == null)
			{
				return;
			}
			ShipInfo shipInfo = new ShipInfo();
			shipInfo = this.GetShipInfo(suulka.ShipID, true);
			if (shipInfo == null)
			{
				return;
			}
			FleetInfo fleetInfo = this.GetFleetInfo(shipInfo.FleetID);
			this.RemoveSuulka(suulka.ID);
			this.RemoveShip(suulka.ShipID);
			this.RemoveAdmiral(suulka.AdmiralID);
			DesignInfo designInfo = new DesignInfo();
			designInfo.PlayerID = 0;
			designInfo.Name = shipInfo.DesignInfo.Name;
			designInfo.DesignSections = shipInfo.DesignInfo.DesignSections;
			int designID = this.InsertDesignByDesignInfo(designInfo);
			int shipID = this.InsertShip(0, designID, null, (ShipParams)0, null, 0);
			int admiralID = this.InsertAdmiral(0, null, designInfo.Name, "suulka", 0f, "male", 100f, 100f, 0);
			this.InsertSuulka(null, shipID, admiralID, null, -1);
			if (fleetInfo != null)
			{
				if (this.GetShipsByFleetID(fleetInfo.ID).Count<int>() == 0)
				{
					this.RemoveFleet(fleetInfo.ID);
					return;
				}
				MissionInfo missionByFleetID = this.GetMissionByFleetID(fleetInfo.ID);
				if (missionByFleetID != null)
				{
                    Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, fleetInfo, true);
					if (!this.GetWaypointsByMissionID(missionByFleetID.ID).Any((WaypointInfo x) => x.Type == WaypointType.DisbandFleet))
					{
						this.InsertWaypoint(missionByFleetID.ID, WaypointType.DisbandFleet, null);
						return;
					}
				}
				else
				{
					this.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_FLEET_DISBANDED,
						EventMessage = TurnEventMessage.EM_FLEET_DISBANDED,
						PlayerID = fleetInfo.PlayerID,
						FleetID = fleetInfo.ID,
						SystemID = fleetInfo.SystemID,
						TurnNumber = this.GetTurnCount(),
						ShowsDialog = false
					});
					int? reserveFleetID = this.GetReserveFleetID(fleetInfo.PlayerID, fleetInfo.SystemID);
					if (reserveFleetID.HasValue)
					{
						List<ShipInfo> list = this.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>();
						foreach (ShipInfo current in list)
						{
							this.UpdateShipAIFleetID(current.ID, null);
							this.TransferShip(current.ID, reserveFleetID.Value);
						}
					}
					this.RemoveFleet(fleetInfo.ID);
				}
			}
		}
		public int InsertSuulka(int? playerID, int shipID, int admiralID, int? stationID = null, int arrivalTurns = -1)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertSuulka, new object[]
			{
				playerID.ToNullableSQLiteValue(),
				shipID.ToSQLiteValue(),
				admiralID.ToSQLiteValue(),
				stationID.ToNullableSQLiteValue(),
				arrivalTurns.ToSQLiteValue()
			}));
		}
		public void UpdateSectionInstance(SectionInstanceInfo section)
		{
			this._dom.section_instances.Update(section.ID, section);
		}
		public void UpdateArmorInstances(int sectionInstanceId, Dictionary<ArmorSide, DamagePattern> patterns)
		{
			if (this._dom.armor_instances.ContainsKey(sectionInstanceId))
			{
				this._dom.armor_instances.Update(sectionInstanceId, patterns);
			}
		}
		public void UpdateWeaponInstance(WeaponInstanceInfo weapon)
		{
			this._dom.weapon_instances.Update(weapon.ID, weapon);
		}
		public void UpdateModuleInstance(ModuleInstanceInfo module)
		{
			this._dom.module_instances.Update(module.ID, module);
		}
		private void UpdateCachedFleetInfo(FleetInfo fleet)
		{
			FleetInfo fleetInfo;
			if (this._dom.fleets.TryGetValue(fleet.ID, out fleetInfo))
			{
				fleetInfo.CopyFrom(fleet);
			}
		}
		private void UpdateCachedFleetPreferred(int id, bool preferred)
		{
			FleetInfo fleetInfo;
			if (this._dom.fleets.TryGetValue(id, out fleetInfo))
			{
				fleetInfo.Preferred = preferred;
			}
		}
		private void RemoveCachedFleet(int fleetID)
		{
			this._dom.fleets.Remove(fleetID);
			this._dom.fleetShips.Remove(fleetID);
		}
		public void UpdateFleetInfo(FleetInfo fleet)
		{
			this._dom.CachedSystemStratSensorRanges.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFleetInfo, new object[]
			{
				fleet.ID,
				fleet.AdmiralID.ToOneBasedSQLiteValue(),
				fleet.SupportingSystemID.ToOneBasedSQLiteValue(),
				fleet.Name,
				fleet.TurnsAway,
				fleet.SupplyRemaining,
				fleet.PlayerID
			}), false, true);
			this.UpdateCachedFleetInfo(fleet);
		}
		public void UpdateCachedFleetLocation(int fleetID, int systemID, int? prevSystemID = null)
		{
			FleetInfo fleetInfo;
			if (this._dom.fleets.TryGetValue(fleetID, out fleetInfo))
			{
				fleetInfo.SystemID = systemID;
				fleetInfo.PreviousSystemID = prevSystemID;
			}
		}
		public void UpdateFleetAccelerated(GameSession game, int fleetID, int? turn = null)
		{
			int num = this.GetTurnCount();
			if (turn.HasValue)
			{
				num = turn.Value;
			}
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
            if (Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(game, fleetInfo.ID) > Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(game, fleetInfo.PlayerID))
			{
				num = -10;
			}
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFleetLastAccelerated, fleetID, num), false, true);
			fleetInfo.LastTurnAccelerated = num;
			this.UpdateCachedFleetInfo(fleetInfo);
		}
		public bool IsInAccelRange(int fleetid)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetid);
			MissionInfo missionByFleetID = this.GetMissionByFleetID(fleetInfo.ID);
			WaypointInfo waypointInfo = null;
			if (missionByFleetID != null)
			{
				waypointInfo = this.GetNextWaypointForMission(missionByFleetID.ID);
			}
			int num = missionByFleetID.TargetSystemID;
			if (waypointInfo != null && waypointInfo.SystemID.HasValue)
			{
				num = waypointInfo.SystemID.Value;
			}
			else
			{
				if (waypointInfo != null && waypointInfo.Type == WaypointType.ReturnHome)
				{
					num = fleetInfo.SupportingSystemID;
				}
			}
			if (missionByFleetID != null && fleetInfo.SystemID == 0 && fleetInfo.PreviousSystemID.HasValue && fleetInfo.PreviousSystemID != num)
			{
				FleetLocation fleetLocation = this.GetFleetLocation(fleetid, false);
				NodeLineInfo nodeLineBetweenSystems = this.GetNodeLineBetweenSystems(fleetInfo.PlayerID, fleetInfo.PreviousSystemID.Value, num, false, true);
				if (nodeLineBetweenSystems != null)
				{
					List<int> list = this.GetFleetsForLoaLine(nodeLineBetweenSystems.ID).ToList<int>();
					foreach (int current in list)
					{
						this.GetFleetInfo(current);
						FleetLocation fleetLocation2 = this.GetFleetLocation(current, false);
						if ((fleetLocation2.Coords - fleetLocation.Coords).Length <= 4f)
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}
		public void UpdateFleetCompositionID(int fleetID, int? CompositionID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFleetCompositionID, fleetID.ToSQLiteValue(), CompositionID.ToNullableSQLiteValue()), false, true);
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			fleetInfo.FleetConfigID = CompositionID;
			this.UpdateCachedFleetInfo(fleetInfo);
		}
		public void SaveCurrentFleetCompositionToFleet(int fleetid)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetid);
			List<ShipInfo> source = this.GetShipInfoByFleetID(fleetid, false).ToList<ShipInfo>();
			if (source.Any<ShipInfo>())
			{
				int value = this.InsertLoaFleetComposition(fleetInfo.PlayerID, fleetInfo.Name, 
					from x in source
					select x.DesignID);
				this.UpdateFleetCompositionID(fleetid, new int?(value));
			}
		}
		public void UpdateFleetLocation(int fleetID, int systemID, int? prevSystemID = null)
		{
			if (prevSystemID == 0)
			{
				prevSystemID = null;
			}
			this._dom.CachedSystemStratSensorRanges.Clear();
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateFleetLocation, fleetID.ToSQLiteValue(), systemID.ToOneBasedSQLiteValue(), prevSystemID.ToNullableSQLiteValue()), true);
			this.UpdateCachedFleetLocation(fleetID, systemID, prevSystemID);
		}
		public void UpdateFleetPreferred(int fleetID, bool preferred)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateFleetPreferred, fleetID.ToSQLiteValue(), preferred.ToSQLiteValue()), true);
			this.UpdateCachedFleetPreferred(fleetID, preferred);
		}
		public void RemoveFleet(int fleetID)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			MissionInfo missionByFleetID;
			do
			{
				missionByFleetID = this.GetMissionByFleetID(fleetID);
				if (missionByFleetID != null)
				{
					this.RemoveMission(missionByFleetID.ID);
				}
			}
			while (missionByFleetID != null);
			if (fleetInfo != null && fleetInfo.IsGateFleet)
			{
				this._dom.CachedSystemHasGateFlags.Clear();
			}
			this._dom.CachedSystemStratSensorRanges.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveFleet, fleetID), false, true);
			foreach (int current in (
				from x in this._dom.ships.Values
				where x.FleetID == fleetID
				select x into y
				select y.ID).ToList<int>())
			{
				this._dom.ships.Remove(current);
			}
			List<int> list = (
				from x in this._dom.missions.Values
				where x.TargetFleetID == fleetID
				select x into y
				select y.ID).ToList<int>();
			foreach (int current2 in list)
			{
				this._dom.missions.Sync(current2);
			}
			this.RemoveCachedFleet(fleetID);
		}
		public MoveOrderInfo ParseMoveOrder(Row row)
		{
			return new MoveOrderInfo
			{
				ID = int.Parse(row[0]),
				FleetID = int.Parse(row[1]),
				FromSystemID = string.IsNullOrEmpty(row[2]) ? 0 : int.Parse(row[2]),
				FromCoords = string.IsNullOrEmpty(row[3]) ? Vector3.Zero : Vector3.Parse(row[3]),
				ToSystemID = string.IsNullOrEmpty(row[4]) ? 0 : int.Parse(row[4]),
				ToCoords = string.IsNullOrEmpty(row[5]) ? Vector3.Zero : Vector3.Parse(row[5]),
				Progress = float.Parse(row[6])
			};
		}
		public IEnumerable<MoveOrderInfo> GetMoveOrderInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetMoveOrderInfos, new object[0]), true))
			{
				yield return this.ParseMoveOrder(current);
			}
			yield break;
		}
		public MoveOrderInfo GetMoveOrderInfoByFleetID(int fleetID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetMoveOrderInfoByFleetID, fleetID), true);
			if (table.Count<Row>() == 0)
			{
				return null;
			}
			Row row = table[0];
			return this.ParseMoveOrder(row);
		}
		public IEnumerable<MoveOrderInfo> GetMoveOrdersByDestinationSystem(int systemID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetMoveOrdersByDestinationSystem, systemID), true))
			{
				yield return this.ParseMoveOrder(current);
			}
			yield break;
		}
		public void UpdateMoveOrder(int moveID, float progress)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateMoveOrder, moveID, progress), true);
		}
		public IEnumerable<MoveOrderInfo> GetTempMoveOrders()
		{
			if (this.TempMoveOrders == null)
			{
				this.TempMoveOrders = new List<MoveOrderInfo>();
			}
			return this.TempMoveOrders;
		}
		public void ClearTempMoveOrders()
		{
			if (this.TempMoveOrders != null)
			{
				this.TempMoveOrders.Clear();
			}
		}
		public void RemoveMoveOrder(int moveID)
		{
			if (this.TempMoveOrders == null)
			{
				this.TempMoveOrders = new List<MoveOrderInfo>();
			}
			MoveOrderInfo item = this.GetMoveOrderInfos().FirstOrDefault((MoveOrderInfo x) => x.ID == moveID);
			this.TempMoveOrders.Add(item);
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveMoveOrder, moveID), true);
		}
		public void RemoveTurnEvent(int turnEventID)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveTurnEvent, turnEventID), true);
		}
		public TurnEvent ParseTurnEvent(Row row)
		{
			return new TurnEvent
			{
				ID = row[0].SQLiteValueToInteger(),
				EventType = (TurnEventType)row[1].SQLiteValueToInteger(),
				EventMessage = (TurnEventMessage)row[2].SQLiteValueToInteger(),
				EventDesc = row[3].SQLiteValueToString(),
				PlayerID = row[4].SQLiteValueToOneBasedIndex(),
				SystemID = row[5].SQLiteValueToOneBasedIndex(),
				OrbitalID = row[6].SQLiteValueToOneBasedIndex(),
				ColonyID = row[7].SQLiteValueToOneBasedIndex(),
				FleetID = row[8].SQLiteValueToOneBasedIndex(),
				TechID = row[9].SQLiteValueToOneBasedIndex(),
				MissionID = row[10].SQLiteValueToOneBasedIndex(),
				DesignID = row[11].SQLiteValueToOneBasedIndex(),
				FeasibilityPercent = row[12].SQLiteValueToInteger(),
				TurnNumber = row[13].SQLiteValueToInteger(),
				ShowsDialog = row[14].SQLiteValueToBoolean(),
				AdmiralID = row[15].SQLiteValueToOneBasedIndex(),
				TreatyID = row[16].SQLiteValueToOneBasedIndex(),
				SpecialProjectID = row[17].SQLiteValueToOneBasedIndex(),
				SystemID2 = row[18].SQLiteValueToOneBasedIndex(),
				PlagueType = (WeaponEnums.PlagueType)row[19].SQLiteValueToOneBasedIndex(),
				ImperialPop = (row[20] == null) ? 0f : row[20].SQLiteValueToSingle(),
				CivilianPop = (row[21] == null) ? 0f : row[21].SQLiteValueToSingle(),
				Infrastructure = (row[22] == null) ? 0f : row[22].SQLiteValueToSingle(),
				CombatID = row[23].SQLiteValueToOneBasedIndex(),
				TargetPlayerID = row[24].SQLiteValueToOneBasedIndex(),
				ShipID = row[25].SQLiteValueToOneBasedIndex(),
				ProvinceID = row[26].SQLiteValueToOneBasedIndex(),
				DesignAttribute = (SectionEnumerations.DesignAttribute)row[27].SQLiteValueToOneBasedIndex(),
				ArrivalTurns = row[28].SQLiteValueToOneBasedIndex(),
				NamesList = (row[29] == null) ? "" : row[29].SQLiteValueToString(),
				NumShips = row[30].SQLiteValueToOneBasedIndex(),
				Savings = (row[31] == null) ? 0.0 : row[31].SQLiteValueToDouble(),
				EventSoundCueName = (row[32] == null) ? "" : row[32].SQLiteValueToString(),
				Param1 = (row[33] == null) ? "" : row[33].SQLiteValueToString(),
				dialogShown = row[34].SQLiteValueToBoolean(),
				EventName = row[35].SQLiteValueToString()
			};
		}
		public IEnumerable<TurnEvent> GetTurnEventsByPlayerID(int playerID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetTurnEventsByPlayerID, playerID), true))
			{
				yield return this.ParseTurnEvent(current);
			}
			yield break;
		}
		public IEnumerable<TurnEvent> GetTurnEventsByTurnNumber(int turnnumber, int playerID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetTurnEventsByTurnNumber, turnnumber, playerID), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseTurnEvent(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public bool GetTurnHasEventType(int playerID, int turnnumber, TurnEventType type)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetTurnHasEventType, playerID, turnnumber, (int)type), true);
			return table.Rows.Count<Row>() > 0;
		}
		public void UpdateTurnEventDialogShown(int EventID, bool shown)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateTurnEventDialogShown, EventID.ToSQLiteValue(), shown.ToSQLiteValue()), false, true);
		}
		public void UpdateTurnEventSoundQue(int EventID, string queName)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateTurnEventSoundQue, EventID.ToSQLiteValue(), queName.ToSQLiteValue()), false, true);
		}
		public void InsertTurnEvent(TurnEvent ev)
		{
			if (string.IsNullOrEmpty(ev.EventDesc))
			{
				ev.RebuildEventDesc(this);
			}
			if (string.IsNullOrEmpty(ev.EventName))
			{
				ev.RebuildEventName(this);
			}
			this.db.ExecuteNonQuery(string.Format(Queries.InsertTurnEvent, new object[]
			{
				((int)ev.EventType).ToSQLiteValue(),
				((int)ev.EventMessage).ToSQLiteValue(),
				ev.EventDesc.ToSQLiteValue(),
				ev.PlayerID.ToOneBasedSQLiteValue(),
				ev.SystemID.ToOneBasedSQLiteValue(),
				ev.OrbitalID.ToOneBasedSQLiteValue(),
				ev.ColonyID.ToOneBasedSQLiteValue(),
				ev.FleetID.ToOneBasedSQLiteValue(),
				ev.TechID.ToOneBasedSQLiteValue(),
				ev.MissionID.ToOneBasedSQLiteValue(),
				ev.DesignID.ToOneBasedSQLiteValue(),
				ev.FeasibilityPercent.ToSQLiteValue(),
				ev.TurnNumber.ToSQLiteValue(),
				ev.ShowsDialog.ToSQLiteValue(),
				ev.AdmiralID.ToOneBasedSQLiteValue(),
				ev.TreatyID.ToOneBasedSQLiteValue(),
				ev.SpecialProjectID.ToOneBasedSQLiteValue(),
				ev.SystemID2.ToOneBasedSQLiteValue(),
				((int)ev.PlagueType).ToSQLiteValue(),
				ev.ImperialPop.ToSQLiteValue(),
				ev.CivilianPop.ToSQLiteValue(),
				ev.Infrastructure.ToSQLiteValue(),
				ev.CombatID.ToSQLiteValue(),
				ev.TargetPlayerID.ToOneBasedSQLiteValue(),
				ev.ShipID.ToOneBasedSQLiteValue(),
				ev.ProvinceID.ToOneBasedSQLiteValue(),
				((int)ev.DesignAttribute).ToSQLiteValue(),
				ev.ArrivalTurns.ToSQLiteValue(),
				ev.NamesList.ToSQLiteValue(),
				ev.NumShips.ToSQLiteValue(),
				ev.Savings.ToSQLiteValue(),
				ev.EventSoundCueName.ToSQLiteValue(),
				ev.Param1.ToSQLiteValue(),
				ev.dialogShown.ToSQLiteValue(),
				ev.EventName.ToSQLiteValue()
			}), false, true);
		}
		public WaypointInfo ParseWaypointInfo(Row r)
		{
			return new WaypointInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				MissionID = r[1].SQLiteValueToInteger(),
				Type = (WaypointType)r[2].SQLiteValueToInteger(),
				SystemID = r[3].SQLiteValueToNullableInteger()
			};
		}
		public IEnumerable<WaypointInfo> GetWaypointsByMissionID(int missionID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetWaypointsByMissionID, missionID), true))
			{
				yield return this.ParseWaypointInfo(current);
			}
			yield break;
		}
		public WaypointInfo GetNextWaypointForMission(int missionID)
		{
			List<WaypointInfo> source = this.GetWaypointsByMissionID(missionID).ToList<WaypointInfo>();
			return source.FirstOrDefault((WaypointInfo x) => x.Type != WaypointType.Intercepted);
		}
		public void RemoveWaypoint(int waypointId)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveWaypoint, waypointId), true);
		}
		public IEnumerable<MissionInfo> GetPlayerMissionInfosAtSystem(int playerId, int systemId)
		{
			return 
				from x in this._dom.missions.Values
				where x.TargetSystemID == systemId && this.GetFleetInfo(x.FleetID).PlayerID == playerId
				select x;
		}
		public IEnumerable<MissionInfo> GetMissionInfos()
		{
			return this._dom.missions.Values;
		}
		public MissionInfo GetMissionInfo(int missionID)
		{
			if (!this._dom.missions.ContainsKey(missionID))
			{
				return null;
			}
			return this._dom.missions[missionID];
		}
		public MissionInfo GetMissionByFleetID(int fleetID)
		{
			return this._dom.missions.Values.FirstOrDefault((MissionInfo x) => x.FleetID == fleetID);
		}
		public void RemoveMission(int missionID)
		{
			GameDatabase.TraceVerbose("Remove: " + missionID.ToString());
			MissionInfo missionInfo = this.GetMissionInfo(missionID);
			if (missionInfo != null && missionInfo.Type == MissionType.CONSTRUCT_STN)
			{
				StationInfo stationInfo = this.GetStationInfo(missionInfo.TargetOrbitalObjectID);
				if (stationInfo != null && stationInfo.DesignInfo.StationLevel == 0)
				{
					this.RemoveStation(missionInfo.TargetOrbitalObjectID);
				}
			}
			this._dom.missions.Remove(missionID);
		}
		public void UpdateMission(MissionInfo mission)
		{
			GameDatabase.TraceVerbose("UpdateMission: " + mission.ID.ToString());
			this._dom.missions.Update(mission.ID, mission);
		}
		public IEnumerable<ColonyTrapInfo> GetColonyTrapInfos()
		{
			return this._dom.colony_traps.Values;
		}
		public IEnumerable<ColonyTrapInfo> GetColonyTrapInfosAtSystem(int systemId)
		{
			return 
				from x in this._dom.colony_traps.Values
				where x.SystemID == systemId
				select x;
		}
		public ColonyTrapInfo GetColonyTrapInfo(int trapID)
		{
			if (!this._dom.colony_traps.ContainsKey(trapID))
			{
				return null;
			}
			return this._dom.colony_traps[trapID];
		}
		public ColonyTrapInfo GetColonyTrapInfoBySystemIDAndPlanetID(int systemID, int planetID)
		{
			return this._dom.colony_traps.Values.FirstOrDefault((ColonyTrapInfo x) => x.SystemID == systemID && x.PlanetID == planetID);
		}
		public ColonyTrapInfo GetColonyTrapInfoByFleetID(int fleetID)
		{
			return this._dom.colony_traps.Values.FirstOrDefault((ColonyTrapInfo x) => x.FleetID == fleetID);
		}
		public ColonyTrapInfo GetColonyTrapInfoByPlanetID(int planetID)
		{
			return this._dom.colony_traps.Values.FirstOrDefault((ColonyTrapInfo x) => x.PlanetID == planetID);
		}
		public void RemoveColonyTrapInfo(int trapID)
		{
			ColonyTrapInfo colonyTrapInfo = this.GetColonyTrapInfo(trapID);
			if (colonyTrapInfo == null)
			{
				return;
			}
			this.RemoveFleet(colonyTrapInfo.FleetID);
			this._dom.colony_traps.Remove(trapID);
		}
		public IEnumerable<int> GetShipsByFleetID(int fleetID)
		{
			return 
				from x in this._dom.ships.Values
				where x.FleetID == fleetID
				select x into y
				select y.ID;
		}
		public IEnumerable<int> GetShipsByAIFleetID(int aiFleetID)
		{
			return 
				from x in this._dom.ships.Values
				where x.AIFleetID.HasValue && x.AIFleetID.Value == aiFleetID
				select x into y
				select y.ID;
		}
		public Vector3? GetShipFleetPosition(int shipID)
		{
			return this._dom.ships[shipID].ShipFleetPosition;
		}
		public Matrix? GetShipSystemPosition(int shipID)
		{
			return this._dom.ships[shipID].ShipSystemPosition;
		}
		public void UpdateShipFleetPosition(int shipID, Vector3? position)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipFleetPosition, shipID, position.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipID);
		}
		public void UpdateShipSystemPosition(int shipID, Matrix? position)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipSystemPosition, shipID, position.ToNullableSQLiteValue()), false, true);
			this._dom.ships.Sync(shipID);
		}
		public void UpdateShipRiderIndex(int shipID, int index)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipRiderIndex, shipID, index.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipID);
		}
		public IEnumerable<ShipInfo> GetShipInfos(bool getDesignInfos = false)
		{
			return this._dom.ships.Values;
		}
		public IEnumerable<ShipInfo> GetShipInfoByFleetID(int fleetID, bool getDesignInfo = false)
		{
			if (this._dom.fleetShips.ContainsKey(fleetID))
			{
				foreach (ShipInfo current in 
					from x in this._dom.fleetShips[fleetID]
					select this.GetShipInfo(x, false))
				{
					yield return current;
				}
			}
			yield break;
		}
		public ShipInfo GetShipInfo(int shipID, bool getDesign = false)
		{
			if (!this._dom.ships.ContainsKey(shipID))
			{
				return null;
			}
			return this._dom.ships[shipID];
		}
		private SuulkaInfo GetSuulkaInfo(Row row)
		{
			return new SuulkaInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToNullableInteger(),
				ShipID = row[2].SQLiteValueToInteger(),
				AdmiralID = row[3].SQLiteValueToOneBasedIndex(),
				StationID = row[4].SQLiteValueToNullableInteger(),
				ArrivalTurns = row[5].SQLiteValueToInteger()
			};
		}
		public SectionInstanceInfo GetShipSectionInstance(int sectionInstanceID)
		{
			return this._dom.section_instances[sectionInstanceID];
		}
		public IEnumerable<SectionInstanceInfo> GetShipSectionInstances(int shipID)
		{
			return 
				from x in this._dom.section_instances.Values
				where x.ShipID == shipID
				select x;
		}
		private SDBInfo GetSDBInfo(Row row)
		{
			return new SDBInfo
			{
				OrbitalId = row[0].SQLiteValueToInteger(),
				ShipId = row[1].SQLiteValueToInteger()
			};
		}
		public IEnumerable<SDBInfo> GetSDBInfoFromOrbital(int orbitalID)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetSDBByOrbital, orbitalID), true))
			{
				yield return this.GetSDBInfo(current);
			}
			yield break;
		}
		public SDBInfo GetSDBInfoFromShip(int shipID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSDBByShip, shipID), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.GetSDBInfo(table.First<Row>());
			}
			return null;
		}
		public void RemoveSDBByShipID(int shipID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSDBByShip, shipID), false, true);
		}
		public void RemoveSDBbyOrbitalID(int orbitalID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSDBByOrbital, orbitalID), false, true);
		}
		public int InsertSDB(int OrbitalID, int shipID)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertSDB, OrbitalID, shipID));
		}
		public void InsertUISliderNotchSetting(int playerid, UISlidertype type, double slidervalue = 0.0, int colonyid = 0)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertUINotchSetting, new object[]
			{
				playerid,
				(int)type,
				slidervalue.ToSQLiteValue(),
				colonyid.ToOneBasedSQLiteValue()
			}), false, true);
		}
		public void UpdateUISliderNotchSetting(UISliderNotchInfo notchinfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateUINotchSetting, new object[]
			{
				notchinfo.Id,
				notchinfo.PlayerId,
				(int)notchinfo.Type,
				notchinfo.SliderValue.ToSQLiteValue(),
				notchinfo.ColonyId.ToNullableSQLiteValue()
			}), false, true);
		}
		public void DeleteUISliderNotchSetting(int playerid, UISlidertype type)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveUINotchSetting, playerid, (int)type), false, true);
		}
		public void DeleteUISliderNotchSettingForColony(int playerid, int ColonyId, UISlidertype type)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveUINotchSettingForColony, playerid, (int)type, ColonyId), false, true);
		}
		private UISliderNotchInfo parseSliderNotchSetting(Row row)
		{
			if (row != null)
			{
				return new UISliderNotchInfo
				{
					Id = row[0].SQLiteValueToInteger(),
					PlayerId = row[1].SQLiteValueToInteger(),
					Type = (UISlidertype)row[2].SQLiteValueToInteger(),
					SliderValue = row[3].SQLiteValueToDouble(),
					ColonyId = row[4].SQLiteValueToNullableInteger()
				};
			}
			return null;
		}
		public UISliderNotchInfo GetSliderNotchSettingInfo(int playerid, UISlidertype type)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetUINotchSetting, playerid, (int)type), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.parseSliderNotchSetting(table.Rows.First<Row>());
			}
			return null;
		}
		public UISliderNotchInfo GetSliderNotchSettingInfoForColony(int playerid, int ColonyId, UISlidertype type)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetUINotchSettingForColony, playerid, (int)type, ColonyId), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.parseSliderNotchSetting(table.Rows.First<Row>());
			}
			return null;
		}
		public Dictionary<ArmorSide, DamagePattern> GetArmorInstances(int sectionInstanceId)
		{
			if (this._dom.armor_instances.ContainsKey(sectionInstanceId))
			{
				return this._dom.armor_instances[sectionInstanceId];
			}
			return ArmorInstancesCache.EmptyArmorInstances;
		}
		public IEnumerable<WeaponInstanceInfo> GetWeaponInstances(int sectionInstanceId)
		{
			return this._dom.weapon_instances.EnumerateBySectionInstanceID(sectionInstanceId);
		}
		public IEnumerable<ModuleInstanceInfo> GetModuleInstances(int sectionInstanceId)
		{
			return this._dom.module_instances.EnumerateBySectionInstanceID(sectionInstanceId);
		}
		public IEnumerable<ShipInfo> GetBattleRidersByParentID(int parentID)
		{
			return 
				from x in this._dom.ships.Values
				where x.ParentID == parentID
				select x;
		}
		public void RemoveShipFromFleet(int shipID)
		{
		}
		public void TransferShip(int shipID, int toFleetID)
		{
			if (shipID < 0)
			{
				return;
			}
			ShipInfo shipInfo = this.GetShipInfo(shipID, false);
			FleetInfo fleetInfo = this.GetFleetInfo(shipInfo.FleetID);
			FleetInfo fleetInfo2 = this.GetFleetInfo(toFleetID);
			GameDatabase.Trace(string.Format("Transferring ship({0}) into fleet({1}).", shipID, toFleetID));
			if (fleetInfo != null && fleetInfo.PlayerID != fleetInfo2.PlayerID)
			{
				throw new InvalidOperationException(string.Format("Oops! Someone tried transferring a ship to a different player's fleet (from fleetID={0}, to fleetID={1})", shipInfo.FleetID, toFleetID));
			}
			this.TryRemoveFleetShip(shipID, shipInfo.FleetID);
			this.db.ExecuteTableQuery(string.Format(Queries.TransferShip, shipID, toFleetID), true);
			this._dom.ships.Sync(shipID);
			this.TryAddFleetShip(shipID, toFleetID);
			this.UpdateShipFleetPosition(shipID, null);
			FleetInfo fleetInfo3 = this.GetFleetInfo(toFleetID);
			if (fleetInfo3.SystemID == fleetInfo3.SupportingSystemID)
			{
                fleetInfo3.SupplyRemaining = Kerberos.Sots.StarFleet.StarFleet.GetSupplyCapacity(this, toFleetID);
				this.UpdateFleetInfo(fleetInfo3);
			}
			foreach (ShipInfo current in this.GetBattleRidersByParentID(shipID).ToList<ShipInfo>())
			{
				this.TransferShip(current.ID, toFleetID);
			}
		}
		private void RemoveCachedShipNameReference(int playerId, string shipName)
		{
			Dictionary<string, int> dictionary;
			int num;
			if (this._dom.CachedPlayerShipNames.TryGetValue(playerId, out dictionary) && dictionary.TryGetValue(shipName, out num))
			{
				if (num <= 1)
				{
					dictionary.Remove(shipName);
					return;
				}
				Dictionary<string, int> dictionary2;
				(dictionary2 = dictionary)[shipName] = dictionary2[shipName] - 1;
			}
		}
		private void AddCachedShipNameReference(int playerId, string shipName)
		{
			Dictionary<string, int> dictionary;
			if (!this._dom.CachedPlayerShipNames.TryGetValue(playerId, out dictionary))
			{
				dictionary = new Dictionary<string, int>();
				this._dom.CachedPlayerShipNames[playerId] = dictionary;
				dictionary[shipName] = 1;
				return;
			}
			if (dictionary.ContainsKey(shipName))
			{
				Dictionary<string, int> dictionary2;
				(dictionary2 = dictionary)[shipName] = dictionary2[shipName] + 1;
				return;
			}
			dictionary[shipName] = 1;
		}
		public void RemoveShip(int shipID)
		{
			ShipInfo shipInfo = this.GetShipInfo(shipID, false);
			if (shipInfo == null)
			{
				return;
			}
			this.AddNumShipsDestroyedFromDesign(shipInfo.DesignID, 1);
			this._dom.ships.Remove(shipID);
			this.TryRemoveFleetShip(shipID, shipInfo.FleetID);
			this._dom.CachedSystemHasGateFlags.Clear();
			List<ShipInfo> list = this.GetBattleRidersByParentID(shipID).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				this.RemoveShip(current.ID);
			}
			int playerID = this.GetDesignInfo(shipInfo.DesignID).PlayerID;
			this.RemoveCachedShipNameReference(playerID, shipInfo.ShipName);
		}
		public void SetShipParent(int shipID, int parentID)
		{
			if (parentID != 0)
			{
				ShipInfo shipInfo = this.GetShipInfo(shipID, true);
				ShipInfo shipInfo2 = this.GetShipInfo(parentID, true);
				if (shipInfo.DesignInfo.PlayerID != shipInfo2.DesignInfo.PlayerID)
				{
					throw new ArgumentException(string.Format("Attempted to parent player {0} ship {1} ({2}) to player {3} ship {4} ({5}).", new object[]
					{
						shipInfo.DesignInfo.PlayerID,
						shipInfo.ID,
						shipInfo.ShipName,
						shipInfo2.DesignInfo.PlayerID,
						shipInfo2.ID,
						shipInfo2.ShipName
					}), "parentID");
				}
			}
			this.db.ExecuteNonQuery(string.Format(Queries.SetShipParentID, shipID, parentID), false, true);
			this._dom.ships.Sync(shipID);
		}
		public void UpdateShipDesign(int shipId, int newdesignid, int? stationId = null)
		{
			ShipInfo shipInfo = this.GetShipInfo(shipId, false);
			DesignInfo designInfo = this.GetDesignInfo(newdesignid);
			if (shipInfo != null && designInfo != null)
			{
				List<SectionInstanceInfo> list = this.GetShipSectionInstances(shipId).ToList<SectionInstanceInfo>();
				foreach (SectionInstanceInfo current in list)
				{
					this.RemoveSectionInstance(current.ID);
				}
				this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipDesign, shipId.ToSQLiteValue(), newdesignid.ToSQLiteValue()), false, true);
				this._dom.ships.Sync(shipId);
				this.InsertNewShipSectionInstances(designInfo, new int?(shipInfo.ID), stationId);
			}
		}
		public void UpdateShipParams(int shipId, ShipParams parms)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipParams, shipId.ToSQLiteValue(), ((int)parms).ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}
		public void UpdateShipAIFleetID(int shipId, int? aiFleetID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipAIFleetID, shipId.ToSQLiteValue(), aiFleetID.ToNullableSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}
		public void UpdateShipObtainedSlaves(int shipId, double slaves)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipSlavesObtained, shipId.ToSQLiteValue(), slaves.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}
		public void UpdateShipLoaCubes(int shipId, int loacubes)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipLoaCubes, shipId.ToSQLiteValue(), loacubes.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}
		public void UpdateShipPsionicPower(int shipId, int power)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipPsionicPower, shipId.ToSQLiteValue(), power.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}
		public DesignInfo GetDesignInfo(int designID)
		{
			if (designID == 0)
			{
				return null;
			}
			return this._dom.designs[designID];
		}
		private HashSet<string> GetDesignNamesByPlayer(int playerId)
		{
			HashSet<string> hashSet;
			if (!this._dom.CachedPlayerDesignNames.TryGetValue(playerId, out hashSet))
			{
				hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
				foreach (string current in 
					from x in this.GetDesignInfosForPlayer(playerId)
					select x.Name)
				{
					hashSet.Add(current);
				}
				this._dom.CachedPlayerDesignNames.Add(playerId, hashSet);
			}
			return hashSet;
		}
		public string ResolveNewDesignName(int playerId, string name)
		{
			string pattern = " [Mm][Kk]\\. [0-9]+$";
			string pattern2 = "[0-9]+$";
			string arg;
			int num;
			if (Regex.IsMatch(name, pattern, RegexOptions.CultureInvariant))
			{
				arg = Regex.Replace(name, pattern, string.Empty, RegexOptions.CultureInvariant);
				Match match = Regex.Match(name, pattern2, RegexOptions.CultureInvariant);
				num = int.Parse(match.ToString());
			}
			else
			{
				arg = name;
				num = 1;
			}
			string text = name;
			HashSet<string> designNamesByPlayer = this.GetDesignNamesByPlayer(playerId);
			while (designNamesByPlayer.Contains(text))
			{
				num++;
				text = string.Format("{0} Mk. {1}", arg, num);
			}
			return text;
		}
		public int GetNumShipsBuiltFromDesign(int designID)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumShipsBuiltFromDesign, designID));
		}
		public int GetNumShipsDestroyedFromDesign(int designID)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumShipsDestroyedFromDesign, designID));
		}
		public int GetNumColonies(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumColoniesByPlayer, playerId));
		}
		public int GetNumProvinces(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumProvincesByPlayer, playerId));
		}
		public int GetNumShips(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumShipsByPlayer, playerId));
		}
		public double GetNumCivilians(int playerId)
		{
			Faction faction = this.assetdb.GetFaction(this.GetPlayerInfo(playerId).FactionID);
			double num = 0.0;
			foreach (ColonyInfo current in 
				from x in this.GetColonyInfos()
				where x.PlayerID == playerId
				select x)
			{
				num += this.GetCivilianPopulation(current.OrbitalObjectID, faction.ID, faction.HasSlaves());
			}
			return num;
		}
		public double GetNumImperials(int playerId)
		{
			return (
				from x in this.GetColonyInfos()
				where x.PlayerID == playerId
				select x).Sum((ColonyInfo x) => x.ImperialPop);
		}
		public double GetEmpirePopulation(int playerId)
		{
			double num = 0.0;
			num += this.GetNumCivilians(playerId);
			return num + this.GetNumImperials(playerId);
		}
		public int GetEmpireBiosphere(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetEmpireBiosphere, playerId));
		}
		public float GetEmpireEconomy(int playerId)
		{
			List<ColonyInfo> list = this.GetPlayerColoniesByPlayerId(playerId).ToList<ColonyInfo>();
			if (list.Count > 0)
			{
				return list.Average((ColonyInfo x) => x.EconomyRating);
			}
			return 0f;
		}
		public int? GetEmpireMorale(int playerId)
		{
			int playerFactionID = this.GetPlayerFactionID(playerId);
			List<ColonyFactionInfo> list = new List<ColonyFactionInfo>();
			foreach (ColonyInfo current in this.GetPlayerColoniesByPlayerId(playerId).ToList<ColonyInfo>())
			{
				ColonyFactionInfo[] factions = current.Factions;
				for (int i = 0; i < factions.Length; i++)
				{
					ColonyFactionInfo colonyFactionInfo = factions[i];
					if (colonyFactionInfo.FactionID == playerFactionID)
					{
						list.Add(colonyFactionInfo);
						break;
					}
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return new int?((int)list.Average((ColonyFactionInfo x) => (double)x.Morale));
		}
		public IEnumerable<DesignInfo> GetDesignInfosForPlayer(int playerID)
		{
			return 
				from x in this._dom.designs.Values
				where x.PlayerID == playerID
				select x;
		}
		public IEnumerable<DesignInfo> GetVisibleDesignInfosForPlayer(int playerID)
		{
			try
			{
				int[] array = this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetVisibleDesignIDsForPlayer, playerID));
				for (int i = 0; i < array.Length; i++)
				{
					int designID = array[i];
					yield return this.GetDesignInfo(designID);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<DesignInfo> GetDesignInfosForPlayer(int playerID, RealShipClasses shipClass, bool retrieveSections = true)
		{
			return 
				from x in this._dom.designs.Values
				where x.PlayerID == playerID && x.GetRealShipClass() == shipClass
				select x;
		}
		public IEnumerable<DesignInfo> GetVisibleDesignInfosForPlayer(int playerID, RealShipClasses shipClass)
		{
			IEnumerable<DesignInfo> visibleDesignInfosForPlayer = this.GetVisibleDesignInfosForPlayer(playerID);
			return 
				from x in visibleDesignInfosForPlayer
				where x.GetRealShipClass() == shipClass
				select x;
		}
		public IEnumerable<DesignInfo> GetVisibleDesignInfosForPlayerAndRole(int playerID, ShipRole role, bool retrieveSections = true)
		{
			return 
				from x in this.GetVisibleDesignInfosForPlayer(playerID)
				where x.Role == role
				select x;
		}
		public IEnumerable<DesignInfo> GetVisibleDesignInfosForPlayerAndRole(int playerID, ShipRole role, WeaponRole? weaponRole)
		{
			return 
				from x in this.GetVisibleDesignInfosForPlayer(playerID)
				where x.Role == role && (!weaponRole.HasValue || x.WeaponRole == weaponRole)
				select x;
		}
		private void CopyBuildOrderInfo(BuildOrderInfo output, Row row)
		{
			output.ID = row[0].SQLiteValueToInteger();
			output.DesignID = row[1].SQLiteValueToInteger();
			output.Priority = row[2].SQLiteValueToInteger();
			output.SystemID = row[3].SQLiteValueToInteger();
			output.Progress = row[4].SQLiteValueToInteger();
			output.MissionID = row[5].SQLiteValueToOneBasedIndex();
			output.ShipName = row[6].SQLiteValueToString();
			output.ProductionTarget = row[7].SQLiteValueToInteger();
			output.InvoiceID = row[8].SQLiteValueToNullableInteger();
			output.AIFleetID = row[9].SQLiteValueToNullableInteger();
			output.LoaCubes = row[10].SQLiteValueToInteger();
		}
		private InvoiceBuildOrderInfo ParseInvoiceBuildOrderInfo(Row row)
		{
			return new InvoiceBuildOrderInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				DesignID = row[1].SQLiteValueToInteger(),
				ShipName = row[2].SQLiteValueToString(),
				InvoiceID = row[3].SQLiteValueToInteger(),
				LoaCubes = row[4].SQLiteValueToInteger()
			};
		}
		private BuildOrderInfo GetBuildOrderInfo(Row row)
		{
			BuildOrderInfo buildOrderInfo = new BuildOrderInfo();
			this.CopyBuildOrderInfo(buildOrderInfo, row);
			return buildOrderInfo;
		}
		public BuildOrderInfo GetBuildOrderInfo(int buildOrderId)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetBuildOrderInfo, buildOrderId), true)[0];
			return this.GetBuildOrderInfo(row);
		}
		public InvoiceInstanceInfo ParseInvoiceInstanceInfo(Row r)
		{
			return new InvoiceInstanceInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerID = r[1].SQLiteValueToInteger(),
				SystemID = r[2].SQLiteValueToInteger(),
				Name = r[3].SQLiteValueToString()
			};
		}
		public InvoiceInfo ParseInvoiceInfo(Row r)
		{
			return new InvoiceInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerID = r[1].SQLiteValueToInteger(),
				Name = r[2].SQLiteValueToString(),
				isFavorite = r[3].SQLiteValueToBoolean()
			};
		}
		public InvoiceInfo GetInvoiceInfo(int id, int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceInfo, id, playerId), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseInvoiceInfo(table.Rows[0]);
			}
			return null;
		}
		public IEnumerable<InvoiceInfo> GetInvoiceInfosForPlayer(int playerId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceInfosForPlayer, playerId), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row r = rows[i];
					yield return this.ParseInvoiceInfo(r);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<InvoiceBuildOrderInfo> GetInvoiceBuildOrders(int invoiceId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceBuildOrders, invoiceId), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseInvoiceBuildOrderInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<BuildOrderInfo> GetBuildOrdersForInvoiceInstance(int invoiceInstanceId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetBuildOrderForInvoice, invoiceInstanceId), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.GetBuildOrderInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public InvoiceInstanceInfo GetInvoiceInstanceInfo(int id)
		{
			return this.ParseInvoiceInstanceInfo(this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceInstance, id), true).Rows.First<Row>());
		}
		public int InsertInvoice(string name, int playerId, bool isFavorite)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertInvoice, name, playerId, isFavorite.ToSQLiteValue()));
		}
		public int InsertInvoiceInstance(int playerId, int systemId, string name)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertInvoiceInstance, playerId, systemId, name));
		}
		public void RemoveFavoriteInvoice(int invoiceId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveFavoriteInvoice, invoiceId), false, true);
		}
		public void RemoveInvoiceInstance(int invoiceInstanceId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveInvoiceInstance, invoiceInstanceId), false, true);
		}
		public IEnumerable<InvoiceInstanceInfo> GetInvoicesForSystem(int playerId, int systemId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceInstancesBySystemAndPlayer, playerId, systemId), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row r = rows[i];
					yield return this.ParseInvoiceInstanceInfo(r);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<BuildOrderInfo> GetBuildOrdersForSystem(int systemId)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetBuildOrdersForSystem, systemId), true))
			{
				yield return this.GetBuildOrderInfo(current);
			}
			yield break;
		}
		public BuildOrderInfo GetFirstBuildOrderForSite(int buildSiteID)
		{
			int num = 0;
			BuildOrderInfo buildOrderInfo = new BuildOrderInfo();
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetBuildOrdersForSystem, buildSiteID), true))
			{
				if (num == 0 || num > int.Parse(current[2]))
				{
					this.CopyBuildOrderInfo(buildOrderInfo, current);
					num = int.Parse(current[2]);
				}
			}
			return buildOrderInfo;
		}
		public void UpdateBuildOrder(BuildOrderInfo buildOrder)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateBuildOrder, new object[]
			{
				buildOrder.ID,
				buildOrder.ProductionTarget,
				buildOrder.Priority,
				buildOrder.Progress,
				buildOrder.ShipName.ToSQLiteValue()
			}), true);
		}
		public void RemoveBuildOrder(int buildOrderID)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveBuildOrder, buildOrderID), true);
		}
		private BuildSiteInfo GetBuildSiteInfo(Row row)
		{
			return new BuildSiteInfo
			{
				ID = int.Parse(row[0]),
				StationID = int.Parse(row[1]),
				PlanetID = int.Parse(row[2]),
				ShipID = int.Parse(row[3]),
				Resources = int.Parse(row[4])
			};
		}
		public void UpdateBuildSiteResources(int buildSiteID, int resources)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateBuildSiteResources, buildSiteID, resources), true);
		}
		public void RemoveBuildSite(int buildSiteID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveBuildSite, buildSiteID), false, true);
		}
		private DesignModuleInfo ParseDesignModuleInfo(DesignSectionInfo parent, Row row)
		{
			return DesignsCache.GetDesignModuleInfoFromRow(this.db, row, parent);
		}
		public IEnumerable<DesignModuleInfo> GetQueuedStationModules(DesignSectionInfo sectionInfo)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetQueuedStationModuleInfos, sectionInfo.ID.ToSQLiteValue()), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseDesignModuleInfo(sectionInfo, row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void RemoveQueuedStationModule(int queuedModuleId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveQueuedStationModule, queuedModuleId), false, true);
		}
		public int InsertQueuedStationModule(int designSectionId, int moduleId, int? weaponId, string mountId, ModuleEnums.StationModuleType moduleType)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertQueuedStationModule.ToSQLiteValue(), new object[]
			{
				designSectionId.ToSQLiteValue(),
				moduleId.ToSQLiteValue(),
				weaponId.ToNullableSQLiteValue(),
				mountId.ToSQLiteValue(),
				((int)moduleType).ToSQLiteValue()
			}));
		}
		public IEnumerable<string> GetDesignSectionNames(int designID)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetDesignSectionNames, designID), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return row.Values[0];
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<StellarPropInfo> GetStellarProps()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetStellarProps, true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return new StellarPropInfo
					{
						ID = int.Parse(row[0]),
						AssetPath = row[1],
						Transform = Matrix.Parse(row[2])
					};
				}
			}
			finally
			{
			}
			yield break;
		}
		public int GetPlayerFactionID(int playerId)
		{
			return this._dom.players.GetPlayerFactionID(playerId);
		}
		public FactionInfo GetPlayerFaction(int playerId)
		{
			return this.GetFactionInfo(this._dom.players.GetPlayerFactionID(playerId));
		}
		private ArmisticeTreatyInfo ParseArmisticeTreatyInfo(Row r, ArmisticeTreatyInfo ati)
		{
			if (r[13] != null)
			{
				ati.SuggestedDiplomacyState = (DiplomacyState)r[13].SQLiteValueToInteger();
			}
			return ati;
		}
		private LimitationTreatyInfo ParseLimitationTreatyInfo(Row r, LimitationTreatyInfo lti)
		{
			lti.LimitationType = (LimitationTreatyType)r[9].SQLiteValueToInteger();
			lti.LimitationAmount = r[10].SQLiteValueToSingle();
			lti.LimitationGroup = r[11].SQLiteValueToString();
			return lti;
		}
		private TreatyInfo ParseTreatyInfo(Row r, TreatyInfo ti, List<TreatyConsequenceInfo> treatyConsequences, List<TreatyIncentiveInfo> treatyIncentives)
		{
			ti.ID = r[0].SQLiteValueToInteger();
			ti.InitiatingPlayerId = r[1].SQLiteValueToInteger();
			ti.ReceivingPlayerId = r[2].SQLiteValueToInteger();
			ti.Type = (TreatyType)r[3].SQLiteValueToInteger();
			ti.Duration = r[4].SQLiteValueToInteger();
			ti.StartingTurn = r[5].SQLiteValueToInteger();
			ti.Active = r[6].SQLiteValueToBoolean();
			ti.Removed = r[7].SQLiteValueToBoolean();
			ti.Consequences.AddRange((
				from x in treatyConsequences
				where x.TreatyId == ti.ID
				select x).ToList<TreatyConsequenceInfo>());
			ti.Incentives.AddRange((
				from x in treatyIncentives
				where x.TreatyId == ti.ID
				select x).ToList<TreatyIncentiveInfo>());
			return ti;
		}
		private TreatyInfo ParseTreatyInfo(Row r, List<TreatyConsequenceInfo> treatyConsequences, List<TreatyIncentiveInfo> treatyIncentives)
		{
			if (r[3].SQLiteValueToInteger() == 0)
			{
				ArmisticeTreatyInfo ati = (ArmisticeTreatyInfo)this.ParseTreatyInfo(r, new ArmisticeTreatyInfo(), treatyConsequences, treatyIncentives);
				return this.ParseArmisticeTreatyInfo(r, ati);
			}
			if (r[3].SQLiteValueToInteger() == 2)
			{
				LimitationTreatyInfo lti = (LimitationTreatyInfo)this.ParseTreatyInfo(r, new LimitationTreatyInfo(), treatyConsequences, treatyIncentives);
				return this.ParseLimitationTreatyInfo(r, lti);
			}
			return this.ParseTreatyInfo(r, new TreatyInfo(), treatyConsequences, treatyIncentives);
		}
		public void InsertGive(GiveInfo give)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertGive, new object[]
			{
				give.InitiatingPlayer,
				give.ReceivingPlayer,
				(int)give.Type,
				give.GiveValue
			}), false, true);
			this.InsertDiplomacyActionHistoryEntry(give.InitiatingPlayer, give.ReceivingPlayer, this.GetTurnCount(), DiplomacyAction.GIVE, new int?((int)give.Type), null, null, null, null);
			switch (give.Type)
			{
			case GiveType.GiveSavings:
				this.InsertGovernmentAction(give.InitiatingPlayer, App.Localize("@GA_GIVESAVINGS"), "GiveSavings", 0, 0);
				return;
			case GiveType.GiveResearchPoints:
				this.InsertGovernmentAction(give.InitiatingPlayer, App.Localize("@GA_GIVERESEARCHPOINTS"), "GiveResearch", 0, 0);
				return;
			default:
				return;
			}
		}
		public void InsertRequest(RequestInfo request)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertRequest, new object[]
			{
				request.InitiatingPlayer,
				request.ReceivingPlayer,
				(int)request.Type,
				request.RequestValue,
				(int)request.State
			}), false, true);
			this.InsertDiplomacyActionHistoryEntry(request.InitiatingPlayer, request.ReceivingPlayer, this.GetTurnCount(), DiplomacyAction.REQUEST, new int?((int)request.Type), null, null, null, null);
			switch (request.Type)
			{
			case RequestType.SavingsRequest:
				this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTSAVINGS"), "RequestSavings", 0, 0);
				return;
			case RequestType.SystemInfoRequest:
				this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTSYSTEMINFO"), "RequestSystemInfo", 0, 0);
				return;
			case RequestType.ResearchPointsRequest:
				this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTRESEARCHPOINTS"), "RequestResearch", 0, 0);
				return;
			case RequestType.MilitaryAssistanceRequest:
				this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTMILITARYASSISTANCE"), "RequestMilitaryAssistance", 0, 0);
				return;
			case RequestType.GatePermissionRequest:
				this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTGATEPERMISSION"), "RequestGatePermission", 0, 0);
				return;
			case RequestType.EstablishEnclaveRequest:
				this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTESTABLISHENCLAVE"), "RequestEstablishEnclave", 0, 0);
				return;
			default:
				return;
			}
		}
		public void InsertDemand(DemandInfo demand)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertDemand, new object[]
			{
				demand.InitiatingPlayer,
				demand.ReceivingPlayer,
				(int)demand.Type,
				demand.DemandValue,
				(int)demand.State
			}), false, true);
			this.InsertDiplomacyActionHistoryEntry(demand.InitiatingPlayer, demand.ReceivingPlayer, this.GetTurnCount(), DiplomacyAction.DEMAND, new int?((int)demand.Type), null, null, null, null);
			switch (demand.Type)
			{
			case DemandType.SavingsDemand:
				this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSAVINGS"), "DemandSavings", 0, 0);
				return;
			case DemandType.SystemInfoDemand:
				this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSYSTEMINFO"), "DemandSystemInfo", 0, 0);
				return;
			case DemandType.ResearchPointsDemand:
				this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDRESEARCHPOINTS"), "DemandResearch", 0, 0);
				return;
			case DemandType.SlavesDemand:
				this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSLAVES"), "DemandSlaves", 0, 0);
				return;
			case DemandType.WorldDemand:
				this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSYSTEM"), "DemandSystem", 0, 0);
				return;
			case DemandType.ProvinceDemand:
				this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDPROVINCE"), "DemandProvince", 0, 0);
				return;
			case DemandType.SurrenderDemand:
				this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSURRENDER"), "DemandSurrender", 0, 0);
				return;
			default:
				return;
			}
		}
		private RequestInfo ParseRequest(Row r)
		{
			return new RequestInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				InitiatingPlayer = r[1].SQLiteValueToInteger(),
				ReceivingPlayer = r[2].SQLiteValueToInteger(),
				Type = (RequestType)r[3].SQLiteValueToInteger(),
				RequestValue = r[4].SQLiteValueToSingle(),
				State = (AgreementState)r[5].SQLiteValueToInteger()
			};
		}
		private GiveInfo ParseGive(Row r)
		{
			return new GiveInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				InitiatingPlayer = r[1].SQLiteValueToInteger(),
				ReceivingPlayer = r[2].SQLiteValueToInteger(),
				Type = (GiveType)r[3].SQLiteValueToInteger(),
				GiveValue = r[4].SQLiteValueToSingle()
			};
		}
		private DemandInfo ParseDemand(Row r)
		{
			return new DemandInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				InitiatingPlayer = r[1].SQLiteValueToInteger(),
				ReceivingPlayer = r[2].SQLiteValueToInteger(),
				Type = (DemandType)r[3].SQLiteValueToInteger(),
				DemandValue = r[4].SQLiteValueToSingle(),
				State = (AgreementState)r[5].SQLiteValueToInteger()
			};
		}
		public IEnumerable<DemandInfo> GetDemandInfos()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetDemandInfos, true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row r = rows[i];
					yield return this.ParseDemand(r);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<RequestInfo> GetRequestInfos()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetRequestInfos, true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row r = rows[i];
					yield return this.ParseRequest(r);
				}
			}
			finally
			{
			}
			yield break;
		}
		public IEnumerable<GiveInfo> GetGiveInfos()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetGiveInfos, true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row r = rows[i];
					yield return this.ParseGive(r);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void ClearGiveInfos()
		{
			this.db.ExecuteNonQuery(Queries.ClearGives, false, true);
		}
		public DemandInfo GetDemandInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetDemandInfo, id), true);
			return this.ParseDemand(table.Rows[0]);
		}
		public RequestInfo GetRequestInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetRequestInfo, id), true);
			return this.ParseRequest(table.Rows[0]);
		}
		public void SetRequestState(AgreementState state, int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetRequestState, id, (int)state), false, true);
		}
		public void SetDemandState(AgreementState state, int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetDemandState, id, (int)state), false, true);
		}
		private TreatyConsequenceInfo ParseTreatyConsequenceInfo(Row r)
		{
			return new TreatyConsequenceInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				TreatyId = r[1].SQLiteValueToInteger(),
				Type = (ConsequenceType)r[2].SQLiteValueToInteger(),
				ConsequenceValue = r[3].SQLiteValueToSingle()
			};
		}
		private TreatyIncentiveInfo ParseTreatyIncentiveInfo(Row r)
		{
			return new TreatyIncentiveInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				TreatyId = r[1].SQLiteValueToInteger(),
				Type = (IncentiveType)r[2].SQLiteValueToInteger(),
				IncentiveValue = r[3].SQLiteValueToSingle()
			};
		}
		public IEnumerable<TreatyInfo> GetTreatyInfos()
		{
			List<TreatyConsequenceInfo> list = new List<TreatyConsequenceInfo>();
			Table table = this.db.ExecuteTableQuery(Queries.GetTreatyConsequences, true);
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row r = rows[i];
				list.Add(this.ParseTreatyConsequenceInfo(r));
			}
			List<TreatyIncentiveInfo> list2 = new List<TreatyIncentiveInfo>();
			table = this.db.ExecuteTableQuery(Queries.GetTreatyIncentives, true);
			Row[] rows2 = table.Rows;
			for (int j = 0; j < rows2.Length; j++)
			{
				Row r2 = rows2[j];
				list2.Add(this.ParseTreatyIncentiveInfo(r2));
			}
			table = this.db.ExecuteTableQuery(Queries.GetTreatyInfos, true);
			try
			{
				Row[] rows3 = table.Rows;
				for (int k = 0; k < rows3.Length; k++)
				{
					Row r3 = rows3[k];
					yield return this.ParseTreatyInfo(r3, list, list2);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void InsertTreaty(TreatyInfo ti)
		{
			int iD = this.db.ExecuteIntegerQuery(string.Format(Queries.InsertTreatyInfo, new object[]
			{
				ti.InitiatingPlayerId.ToSQLiteValue(),
				ti.ReceivingPlayerId.ToSQLiteValue(),
				((int)ti.Type).ToSQLiteValue(),
				ti.Duration.ToSQLiteValue(),
				ti.StartingTurn.ToSQLiteValue(),
				ti.Active.ToSQLiteValue(),
				ti.Removed.ToSQLiteValue()
			}));
			ti.ID = iD;
			this.InsertDiplomacyActionHistoryEntry(ti.InitiatingPlayerId, ti.ReceivingPlayerId, this.GetTurnCount(), DiplomacyAction.TREATY, new int?((int)ti.Type), null, null, null, null);
			if (ti.Type != TreatyType.Incorporate && ti.Type != TreatyType.Protectorate)
			{
				if (ti.Type == TreatyType.Trade)
				{
					this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYTRADE"), "TreatyTrade", 0, 0);
				}
				else
				{
					if (ti.Type == TreatyType.Limitation)
					{
						LimitationTreatyInfo limitationTreatyInfo = (LimitationTreatyInfo)ti;
						this.db.ExecuteNonQuery(string.Format(Queries.InsertLimitationTreatyInfo, new object[]
						{
							limitationTreatyInfo.ID.ToSQLiteValue(),
							((int)limitationTreatyInfo.LimitationType).ToSQLiteValue(),
							limitationTreatyInfo.LimitationAmount.ToSQLiteValue(),
							limitationTreatyInfo.LimitationGroup.ToSQLiteValue()
						}), false, true);
						switch (limitationTreatyInfo.LimitationType)
						{
						case LimitationTreatyType.FleetSize:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYFLEETSIZELIMIT"), "TreatyFleetSizeLimit", 0, 0);
							break;
						case LimitationTreatyType.ShipClass:
						{
							string limitationGroup;
							if ((limitationGroup = limitationTreatyInfo.LimitationGroup) != null)
							{
								if (!(limitationGroup == "Dreadnought"))
								{
									if (limitationGroup == "Leviathan")
									{
										this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYSHIPCLASSLEV"), "TreatyShipClassLev", 0, 0);
									}
								}
								else
								{
									this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYSHIPCLASSDREAD"), "TreatyShipClassDread", 0, 0);
								}
							}
							break;
						}
						case LimitationTreatyType.EmpireSize:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYEMPIRESIZELIMIT"), "TreatyEmpireSizeLimit", 0, 0);
							break;
						case LimitationTreatyType.ForgeGemWorlds:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYDEVELOPMENTLIMIT"), "TreatyDevelopmentLimit", 0, 0);
							break;
						case LimitationTreatyType.StationType:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYSTATIONTYPE"), "TreatyStationType", 0, 0);
							break;
						}
					}
					else
					{
						if (ti.Type == TreatyType.Armistice)
						{
							ArmisticeTreatyInfo armisticeTreatyInfo = (ArmisticeTreatyInfo)ti;
							this.db.ExecuteNonQuery(string.Format(Queries.InsertArmisticeTreatyInfo, armisticeTreatyInfo.ID.ToSQLiteValue(), ((int)armisticeTreatyInfo.SuggestedDiplomacyState).ToSQLiteValue()), false, true);
							switch (armisticeTreatyInfo.SuggestedDiplomacyState)
							{
							case DiplomacyState.NON_AGGRESSION:
								this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYNONAGGRO"), "TreatyNonAggro", 0, 0);
								break;
							case DiplomacyState.ALLIED:
								this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYALLIED"), "TreatyAllied", 0, 0);
								break;
							case DiplomacyState.PEACE:
								this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYPEACE"), "TreatyPeace", 0, 0);
								break;
							}
						}
					}
				}
			}
			foreach (TreatyConsequenceInfo current in ti.Consequences)
			{
				this.db.ExecuteNonQuery(string.Format(Queries.InsertTreatyConsequenceInfo, ti.ID.ToSQLiteValue(), ((int)current.Type).ToSQLiteValue(), current.ConsequenceValue.ToSQLiteValue()), false, true);
			}
			foreach (TreatyIncentiveInfo current2 in ti.Incentives)
			{
				this.db.ExecuteNonQuery(string.Format(Queries.InsertTreatyIncentiveInfo, ti.ID.ToSQLiteValue(), ((int)current2.Type).ToSQLiteValue(), current2.IncentiveValue.ToSQLiteValue()), false, true);
			}
		}
		public void RemoveTreatyInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetTreatyRemoved, id), false, true);
		}
		public void DeleteTreatyInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveTreaty, id), false, true);
		}
		public void SetTreatyActive(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetTreatyActive, id), false, true);
		}
		public void UpdateDiplomacyInfo(DiplomacyInfo di)
		{
			di.Relations = Math.Max(Math.Min(di.Relations, DiplomacyInfo.MaxDeplomacyRelations), DiplomacyInfo.MinDeplomacyRelations);
			this._dom.diplomacy_states.Update(di.ID, di);
		}
		public DiplomacyInfo GetDiplomacyInfo(int playerID, int towardsPlayerID)
		{
			DiplomacyInfo diplomacyInfoByPlayer = this._dom.diplomacy_states.GetDiplomacyInfoByPlayer(playerID, towardsPlayerID);
			if (diplomacyInfoByPlayer == null)
			{
				DiplomacyInfo value = new DiplomacyInfo
				{
					PlayerID = playerID,
					TowardsPlayerID = towardsPlayerID,
					State = DiplomacyState.NEUTRAL,
					isEncountered = false
				};
				return this._dom.diplomacy_states[this._dom.diplomacy_states.Insert(null, value)];
			}
			return diplomacyInfoByPlayer;
		}
		public void UpdateDiplomacyState(int playerID, int towardsPlayerID, DiplomacyState state, int relations, bool reciprocal = true)
		{
			relations = Math.Max(Math.Min(relations, DiplomacyInfo.MaxDeplomacyRelations), DiplomacyInfo.MinDeplomacyRelations);
			if (this.GetPlayerInfo(playerID).IsOnTeam(this.GetPlayerInfo(towardsPlayerID)) && state != DiplomacyState.ALLIED)
			{
				state = DiplomacyState.ALLIED;
			}
			DiplomacyInfo diplomacyInfo = this.GetDiplomacyInfo(playerID, towardsPlayerID);
			diplomacyInfo.State = state;
			diplomacyInfo.Relations = relations;
			this._dom.diplomacy_states.Update(diplomacyInfo.ID, diplomacyInfo);
			if (reciprocal)
			{
				DiplomacyInfo diplomacyInfo2 = this.GetDiplomacyInfo(towardsPlayerID, playerID);
				diplomacyInfo2.State = state;
				diplomacyInfo2.Relations = relations;
				this._dom.diplomacy_states.Update(diplomacyInfo2.ID, diplomacyInfo2);
			}
		}
		public void ChangeDiplomacyState(int playerID, int towardsPlayerID, DiplomacyState state)
		{
			if (this.GetPlayerInfo(playerID).IsOnTeam(this.GetPlayerInfo(towardsPlayerID)) && state != DiplomacyState.ALLIED)
			{
				return;
			}
			DiplomacyInfo diplomacyInfo = this.GetDiplomacyInfo(playerID, towardsPlayerID);
			if (diplomacyInfo.State == DiplomacyState.ALLIED & state != DiplomacyState.ALLIED)
			{
				this.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_LEFT_ALLIANCE,
					EventMessage = TurnEventMessage.EM_LEFT_ALLIANCE,
					PlayerID = playerID,
					TargetPlayerID = towardsPlayerID,
					TurnNumber = this.GetTurnCount(),
					ShowsDialog = false
				});
			}
			this.UpdateDiplomacyState(playerID, towardsPlayerID, state, diplomacyInfo.Relations, true);
		}
		public void InsertDiplomacyReactionHistoryEntry(int player1Id, int player2Id, int turn, int reactionValue, StratModifiers reaction)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertDiplomacyReactionHistoryEntry, new object[]
			{
				player1Id.ToSQLiteValue(),
				player2Id.ToSQLiteValue(),
				turn.ToSQLiteValue(),
				reactionValue.ToSQLiteValue(),
				((int)reaction).ToSQLiteValue()
			}), false, true);
		}
		public void InsertDiplomacyActionHistoryEntry(int player1Id, int player2Id, int turn, DiplomacyAction action, int? actionSubType, float? actionData, int? duration, int? consequenceType, float? consequenceData)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertDiplomacyActionHistoryEntry, new object[]
			{
				player1Id.ToSQLiteValue(),
				player2Id.ToSQLiteValue(),
				turn.ToSQLiteValue(),
				((int)action).ToSQLiteValue(),
				actionSubType.ToNullableSQLiteValue(),
				actionData.ToNullableSQLiteValue(),
				duration.ToNullableSQLiteValue(),
				consequenceType.ToNullableSQLiteValue(),
				consequenceData.ToNullableSQLiteValue()
			}), false, true);
		}
		public DiplomacyReactionHistoryEntryInfo ParseDiplomacyReactionHistoryEntry(Row r)
		{
			return new DiplomacyReactionHistoryEntryInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerId = r[1].SQLiteValueToInteger(),
				TowardsPlayerId = r[2].SQLiteValueToInteger(),
				TurnCount = r[3].SQLiteValueToNullableInteger(),
				Difference = r[4].SQLiteValueToInteger(),
				Reaction = (StratModifiers)r[5].SQLiteValueToInteger()
			};
		}
		public IEnumerable<DiplomacyReactionHistoryEntryInfo> GetDiplomacyReactionHistory(int player1Id, int player2Id, int turnCount, int turnRange)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetDiplomacyReactionHistory, new object[]
			{
				turnCount,
				turnRange,
				player1Id,
				player2Id
			}), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row r = rows[i];
					yield return this.ParseDiplomacyReactionHistoryEntry(r);
				}
			}
			finally
			{
			}
			yield break;
		}
		public DiplomacyActionHistoryEntryInfo ParseDiplomacyActionHistoryEntry(Row r)
		{
			return new DiplomacyActionHistoryEntryInfo
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerId = r[1].SQLiteValueToInteger(),
				TowardsPlayerId = r[2].SQLiteValueToInteger(),
				TurnCount = r[3].SQLiteValueToNullableInteger(),
				Action = (DiplomacyAction)r[4].SQLiteValueToInteger(),
				ActionSubType = r[5].SQLiteValueToNullableInteger(),
				ActionData = r[6].SQLiteValueToNullableSingle(),
				Duration = r[7].SQLiteValueToNullableInteger(),
				ConsequenceType = r[8].SQLiteValueToNullableInteger(),
				ConsequenceData = r[9].SQLiteValueToNullableSingle()
			};
		}
		public IEnumerable<DiplomacyActionHistoryEntryInfo> GetDiplomacyActionHistory(int player1Id, int player2Id, int turnCount, int turnRange)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetDiplomacyActionHistory, new object[]
			{
				turnCount,
				turnRange,
				player1Id,
				player2Id
			}), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row r = rows[i];
					yield return this.ParseDiplomacyActionHistoryEntry(r);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void ApplyDiplomacyReaction(int playerID, int towardsPlayerID, int reactionAmount, StratModifiers? reactionValueHintToRecord, int reactionMultiplier = 1)
		{
			if (reactionAmount == 0)
			{
				return;
			}
			int num = (int)(this.GetStratModifierFloatToApply(StratModifiers.DiplomaticReactionBonus, playerID) * (float)reactionAmount);
			int num2 = 0;
			if (reactionAmount < 0)
			{
				num2 = (int)(this.GetStratModifierFloatToApply(StratModifiers.NegativeRelationsModifier, playerID) * (float)reactionAmount);
			}
			int num3 = (int)((float)reactionAmount * this.assetdb.GovEffects.GetDiplomacyBonus(this, this.assetdb, this.GetPlayerInfo(playerID), this.GetPlayerInfo(towardsPlayerID)));
			reactionAmount = (reactionAmount + num + num2 + num3) * reactionMultiplier;
			if (reactionValueHintToRecord.HasValue)
			{
				this.InsertDiplomacyReactionHistoryEntry(playerID, towardsPlayerID, this.GetTurnCount(), reactionAmount, reactionValueHintToRecord.Value);
			}
			DiplomacyInfo diplomacyInfo = this.GetDiplomacyInfo(playerID, towardsPlayerID);
			int num4 = diplomacyInfo.Relations + reactionAmount;
			if (num4 < DiplomacyInfo.MinDeplomacyRelations)
			{
				num4 = DiplomacyInfo.MinDeplomacyRelations;
			}
			if (num4 > DiplomacyInfo.MaxDeplomacyRelations)
			{
				num4 = DiplomacyInfo.MaxDeplomacyRelations;
			}
			if (num4 == diplomacyInfo.Relations)
			{
				return;
			}
			this.UpdateDiplomacyState(playerID, towardsPlayerID, diplomacyInfo.State, num4, false);
		}
		public void ApplyDiplomacyReaction(int playerID, int towardsPlayerID, StratModifiers reactionValue, int reactionMultiplier = 1)
		{
			if (reactionMultiplier == 0)
			{
				return;
			}
			int stratModifierIntToApply = this.GetStratModifierIntToApply(reactionValue, towardsPlayerID);
			this.ApplyDiplomacyReaction(playerID, towardsPlayerID, stratModifierIntToApply, new StratModifiers?(reactionValue), reactionMultiplier);
		}
		public IEnumerable<IndependentRaceInfo> GetIndependentRaces()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetIndependentRaces, new object[0]), true))
			{
				yield return new IndependentRaceInfo
				{
					ID = int.Parse(current[0]),
					Name = current[1],
					OrbitalObjectID = int.Parse(current[2]),
					Population = double.Parse(current[3]),
					TechLevel = int.Parse(current[4]),
					ReactionHuman = int.Parse(current[5]),
					ReactionTarka = int.Parse(current[6]),
					ReactionLiir = int.Parse(current[7]),
					ReactionZuul = int.Parse(current[8]),
					ReactionMorrigi = int.Parse(current[9]),
					ReactionHiver = int.Parse(current[10])
				};
			}
			yield break;
		}
		public IndependentRaceInfo GetIndependentRace(int raceID)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetIndependentRace, raceID), true)[0];
			return new IndependentRaceInfo
			{
				ID = raceID,
				Name = row[0],
				OrbitalObjectID = int.Parse(row[1]),
				Population = double.Parse(row[2]),
				TechLevel = int.Parse(row[3]),
				ReactionHuman = int.Parse(row[4]),
				ReactionTarka = int.Parse(row[5]),
				ReactionLiir = int.Parse(row[6]),
				ReactionZuul = int.Parse(row[7]),
				ReactionMorrigi = int.Parse(row[8]),
				ReactionHiver = int.Parse(row[9])
			};
		}
		public IndependentRaceInfo GetIndependentRaceForWorld(int orbitalObjectID)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetIndependentRace, orbitalObjectID), true)[0];
			return new IndependentRaceInfo
			{
				ID = int.Parse(row[0]),
				Name = row[1],
				OrbitalObjectID = orbitalObjectID,
				Population = double.Parse(row[2]),
				TechLevel = int.Parse(row[3]),
				ReactionHuman = int.Parse(row[4]),
				ReactionTarka = int.Parse(row[5]),
				ReactionLiir = int.Parse(row[6]),
				ReactionZuul = int.Parse(row[7]),
				ReactionMorrigi = int.Parse(row[8]),
				ReactionHiver = int.Parse(row[9])
			};
		}
		public IndependentRaceColonyInfo GetIndependentRaceColonyInfo(int orbitalObjectID)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetIndependentRaceColony, orbitalObjectID), true)[0];
			return new IndependentRaceColonyInfo
			{
				OrbitalObjectID = orbitalObjectID,
				RaceID = int.Parse(row[0]),
				Population = double.Parse(row[1])
			};
		}
		public EncounterInfo ParseEncounterInfo(Row row)
		{
			return new EncounterInfo
			{
				Id = int.Parse(row[0]),
				Type = (EasterEgg)Enum.Parse(typeof(EasterEgg), row[1])
			};
		}
		public IEnumerable<EncounterInfo> GetEncounterInfos()
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetEncounterInfos, new object[0]), true);
			foreach (Row current in table)
			{
				yield return this.ParseEncounterInfo(current);
			}
			yield break;
		}
		public EncounterInfo GetEncounterInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetEncounterInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseEncounterInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public void RemoveEncounter(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveEncounter, id), false, true);
		}
		public int GetEncounterOrbitalId(EasterEgg type, int systemId)
		{
			foreach (EncounterInfo current in this.GetEncounterInfos())
			{
				if (current.Type == type)
				{
					switch (current.Type)
					{
					case EasterEgg.EE_SWARM:
					{
						SwarmerInfo swarmerInfo = this.GetSwarmerInfo(current.Id);
						if (swarmerInfo != null && swarmerInfo.SystemId == systemId)
						{
							int orbitalId = swarmerInfo.OrbitalId;
							return orbitalId;
						}
						break;
					}
					case EasterEgg.EE_ASTEROID_MONITOR:
					{
						AsteroidMonitorInfo asteroidMonitorInfo = this.GetAsteroidMonitorInfo(current.Id);
						if (asteroidMonitorInfo != null && asteroidMonitorInfo.SystemId == systemId)
						{
							int orbitalId = asteroidMonitorInfo.OrbitalId;
							return orbitalId;
						}
						break;
					}
					case EasterEgg.EE_VON_NEUMANN:
					{
						VonNeumannInfo vonNeumannInfo = this.GetVonNeumannInfo(current.Id);
						if (vonNeumannInfo != null && vonNeumannInfo.SystemId == systemId)
						{
							int orbitalId = vonNeumannInfo.OrbitalId;
							return orbitalId;
						}
						break;
					}
					}
				}
			}
			return 0;
		}
		public int GetNumEncountersOfType(EasterEgg type)
		{
			int num = 0;
			List<EncounterInfo> list = this.GetEncounterInfos().ToList<EncounterInfo>();
			foreach (EncounterInfo current in list)
			{
				if (current.Type == type)
				{
					num++;
				}
			}
			return num;
		}
		public int GetEncounterIDAtSystem(EasterEgg type, int systemId)
		{
			foreach (EncounterInfo current in this.GetEncounterInfos())
			{
				if (current.Type == type)
				{
					switch (current.Type)
					{
					case EasterEgg.EE_SWARM:
					{
						SwarmerInfo swarmerInfo = this.GetSwarmerInfo(current.Id);
						if (swarmerInfo != null && swarmerInfo.SystemId == systemId)
						{
							int id = swarmerInfo.Id;
							return id;
						}
						break;
					}
					case EasterEgg.EE_ASTEROID_MONITOR:
					{
						AsteroidMonitorInfo asteroidMonitorInfo = this.GetAsteroidMonitorInfo(current.Id);
						if (asteroidMonitorInfo != null && asteroidMonitorInfo.SystemId == systemId)
						{
							int id = asteroidMonitorInfo.Id;
							return id;
						}
						break;
					}
					case EasterEgg.EE_VON_NEUMANN:
					{
						VonNeumannInfo vonNeumannInfo = this.GetVonNeumannInfo(current.Id);
						if (vonNeumannInfo != null && vonNeumannInfo.SystemId == systemId)
						{
							int id = vonNeumannInfo.Id;
							return id;
						}
						break;
					}
					case EasterEgg.EE_GARDENERS:
					{
						GardenerInfo gardenerInfo = this.GetGardenerInfo(current.Id);
						if (gardenerInfo != null && gardenerInfo.SystemId == systemId)
						{
							int id = gardenerInfo.Id;
							return id;
						}
						break;
					}
					}
				}
			}
			return 0;
		}
		public int GetEncounterPlayerId(GameSession sim, int systemId)
		{
			foreach (EncounterInfo current in this.GetEncounterInfos())
			{
				switch (current.Type)
				{
				case EasterEgg.EE_SWARM:
				{
					SwarmerInfo swarmerInfo = this.GetSwarmerInfo(current.Id);
					if (swarmerInfo != null && swarmerInfo.SystemId == systemId && sim.ScriptModules.Swarmers != null)
					{
						int playerID = sim.ScriptModules.Swarmers.PlayerID;
						return playerID;
					}
					break;
				}
				case EasterEgg.EE_ASTEROID_MONITOR:
				{
					AsteroidMonitorInfo asteroidMonitorInfo = this.GetAsteroidMonitorInfo(current.Id);
					if (asteroidMonitorInfo != null && asteroidMonitorInfo.SystemId == systemId && sim.ScriptModules.AsteroidMonitor != null)
					{
						int playerID = sim.ScriptModules.AsteroidMonitor.PlayerID;
						return playerID;
					}
					break;
				}
				case EasterEgg.EE_VON_NEUMANN:
				{
					VonNeumannInfo vonNeumannInfo = this.GetVonNeumannInfo(current.Id);
					if (vonNeumannInfo != null && vonNeumannInfo.SystemId == systemId && sim.ScriptModules.VonNeumann != null)
					{
						int playerID = sim.ScriptModules.VonNeumann.PlayerID;
						return playerID;
					}
					break;
				}
				case EasterEgg.EE_GARDENERS:
				{
					GardenerInfo gardenerInfo = this.GetGardenerInfo(current.Id);
					if (gardenerInfo != null && gardenerInfo.SystemId == systemId && sim.ScriptModules.Gardeners != null)
					{
						int playerID = sim.ScriptModules.Gardeners.PlayerID;
						return playerID;
					}
					break;
				}
				}
			}
			return 0;
		}
		public LocustSwarmInfo ParseLocustSwarmInfo(Row row)
		{
			return new LocustSwarmInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				FleetId = row[1].SQLiteValueToNullableInteger(),
				Resources = row[2].SQLiteValueToInteger(),
				NumDrones = row[3].SQLiteValueToInteger()
			};
		}
		public LocustSwarmScoutInfo ParseLocustSwarmScoutInfo(Row row)
		{
			return new LocustSwarmScoutInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				LocustInfoId = row[1].SQLiteValueToInteger(),
				ShipId = row[2].SQLiteValueToInteger(),
				TargetSystemId = row[3].SQLiteValueToInteger(),
				NumDrones = row[4].SQLiteValueToInteger()
			};
		}
		public LocustSwarmScoutTargetInfo ParseLocustSwarmScoutTargetInfo(Row row)
		{
			return new LocustSwarmScoutTargetInfo
			{
				SystemId = row[0].SQLiteValueToInteger(),
				IsHostile = row[1].SQLiteValueToBoolean()
			};
		}
		public void UpdateLocustSwarmInfo(LocustSwarmInfo li)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLocustSwarmInfo, new object[]
			{
				li.Id.ToSQLiteValue(),
				li.FleetId.ToNullableSQLiteValue(),
				li.Resources.ToSQLiteValue(),
				li.NumDrones.ToSQLiteValue()
			}), false, true);
		}
		public void InsertLocustSwarmInfo(LocustSwarmInfo li)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertLocustSwarmInfo, new object[]
			{
				EasterEgg.GM_LOCUST_SWARM.ToString(),
				li.FleetId.ToNullableSQLiteValue(),
				li.Resources.ToSQLiteValue(),
				li.NumDrones.ToSQLiteValue()
			}), false, true);
		}
		public LocustSwarmInfo GetLocustSwarmInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseLocustSwarmInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<LocustSwarmInfo> GetLocustSwarmInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmInfos, new object[0]), true))
			{
				yield return this.ParseLocustSwarmInfo(current);
			}
			yield break;
		}
		public void InsertLocustSwarmTarget(int encounterId, int systemId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertLocustSwarmTarget, systemId), false, true);
		}
		public IEnumerable<int> GetLocustSwarmTargets()
		{
			return this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetLocustSwarmTargets, new object[0]));
		}
		public void InsertLocustSwarmScoutInfo(LocustSwarmScoutInfo ls)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertLocustSwarmScoutInfo, new object[]
			{
				ls.LocustInfoId.ToSQLiteValue(),
				ls.ShipId.ToSQLiteValue(),
				ls.TargetSystemId.ToSQLiteValue(),
				ls.NumDrones.ToSQLiteValue()
			}), false, true);
		}
		public void UpdateLocustSwarmScoutInfo(LocustSwarmScoutInfo ls)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLocustSwarmScoutInfo, ls.Id.ToSQLiteValue(), ls.TargetSystemId.ToSQLiteValue(), ls.NumDrones.ToSQLiteValue()), false, true);
		}
		public LocustSwarmScoutInfo GetLocustSwarmScoutInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseLocustSwarmScoutInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<LocustSwarmScoutInfo> GetLocustSwarmScoutInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutInfos, new object[0]), true))
			{
				yield return this.ParseLocustSwarmScoutInfo(current);
			}
			yield break;
		}
		public IEnumerable<LocustSwarmScoutInfo> GetLocustSwarmScoutsForLocustNest(int worldId)
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutsForLocustNest, worldId.ToSQLiteValue()), true))
			{
				yield return this.ParseLocustSwarmScoutInfo(current);
			}
			yield break;
		}
		public void InsertLocustSwarmScoutTargetInfo(LocustSwarmScoutTargetInfo lst)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertLocustSwarmScoutTargetInfo, lst.SystemId.ToSQLiteValue(), lst.IsHostile.ToSQLiteValue()), false, true);
		}
		public void UpdateLocustSwarmScoutTargetInfo(LocustSwarmScoutTargetInfo lst)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLocustSwarmScoutTargetInfo, lst.SystemId.ToSQLiteValue(), lst.IsHostile.ToSQLiteValue()), false, true);
		}
		public LocustSwarmScoutTargetInfo GetLocustSwarmScoutTargetInfos(int systemId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutTargetInfo, systemId), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseLocustSwarmScoutTargetInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<LocustSwarmScoutTargetInfo> GetLocustSwarmScoutTargetInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutTargetInfos, new object[0]), true))
			{
				yield return this.ParseLocustSwarmScoutTargetInfo(current);
			}
			yield break;
		}
		public SystemKillerInfo ParseSystemKillerInfo(Row row)
		{
			return new SystemKillerInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				FleetId = row[1].SQLiteValueToNullableInteger(),
				Target = row[2].SQLiteValueToVector3()
			};
		}
		public void UpdateSystemKillerInfo(SystemKillerInfo si)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSystemKillerInfo, si.Id.ToSQLiteValue(), si.FleetId.ToNullableSQLiteValue(), si.Target.ToSQLiteValue()), false, true);
		}
		public void InsertSystemKillerInfo(SystemKillerInfo si)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertSystemKillerInfo, EasterEgg.GM_SYSTEM_KILLER.ToString(), si.FleetId.ToNullableSQLiteValue(), si.Target.ToSQLiteValue()), false, true);
		}
		public SystemKillerInfo GetSystemKillerInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSystemKillerInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseSystemKillerInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public NeutronStarInfo ParseNeutronStarInfo(Row row)
		{
			return new NeutronStarInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				FleetId = row[1].SQLiteValueToInteger(),
				TargetSystemId = row[2].SQLiteValueToInteger(),
				DeepSpaceSystemId = row[3].SQLiteValueToNullableInteger(),
				DeepSpaceOrbitalId = row[4].SQLiteValueToNullableInteger()
			};
		}
		public void InsertNeutronStarInfo(NeutronStarInfo nsi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertNeutronStarInfo, new object[]
			{
				EasterEgg.GM_NEUTRON_STAR.ToString(),
				nsi.FleetId.ToSQLiteValue(),
				nsi.TargetSystemId.ToSQLiteValue(),
				nsi.DeepSpaceSystemId.ToNullableSQLiteValue(),
				nsi.DeepSpaceOrbitalId.ToNullableSQLiteValue()
			}), false, true);
		}
		public NeutronStarInfo GetNeutronStarInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetNeutronStarInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseNeutronStarInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<NeutronStarInfo> GetNeutronStarInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetNeutronStarInfos, new object[0]), true))
			{
				yield return this.ParseNeutronStarInfo(current);
			}
			yield break;
		}
		public SuperNovaInfo ParseSuperNovaInfo(Row row)
		{
			return new SuperNovaInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				TurnsRemaining = row[2].SQLiteValueToInteger()
			};
		}
		public void InsertSuperNovaInfo(SuperNovaInfo sni)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertSuperNovaInfo, EasterEgg.GM_SUPER_NOVA.ToString(), sni.SystemId.ToSQLiteValue(), sni.TurnsRemaining.ToSQLiteValue()), false, true);
		}
		public void UpdateSuperNovaInfo(SuperNovaInfo sni)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSuperNovaInfo, sni.Id.ToSQLiteValue(), sni.TurnsRemaining.ToSQLiteValue()), false, true);
		}
		public SuperNovaInfo GetSuperNovaInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSuperNovaInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseSuperNovaInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<SuperNovaInfo> GetSuperNovaInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetSuperNovaInfos, new object[0]), true))
			{
				yield return this.ParseSuperNovaInfo(current);
			}
			yield break;
		}
		public PirateBaseInfo ParsePirateBaseInfo(Row row)
		{
			return new PirateBaseInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				BaseStationId = row[2].SQLiteValueToInteger(),
				TurnsUntilAddShip = row[3].SQLiteValueToInteger(),
				NumShips = row[4].SQLiteValueToInteger()
			};
		}
		public void InsertPirateBaseInfo(PirateBaseInfo pbi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPirateBaseInfo, new object[]
			{
				EasterEgg.EE_PIRATE_BASE.ToString(),
				pbi.SystemId.ToSQLiteValue(),
				pbi.BaseStationId.ToSQLiteValue(),
				pbi.TurnsUntilAddShip.ToSQLiteValue(),
				pbi.NumShips.ToSQLiteValue()
			}), false, true);
		}
		public void UpdatePirateBaseInfo(PirateBaseInfo pbi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePirateBaseInfo, pbi.Id.ToSQLiteValue(), pbi.TurnsUntilAddShip.ToSQLiteValue(), pbi.NumShips.ToSQLiteValue()), false, true);
		}
		public PirateBaseInfo GetPirateBaseInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPirateBaseInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParsePirateBaseInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<PirateBaseInfo> GetPirateBaseInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetPirateBaseInfos, new object[0]), true))
			{
				yield return this.ParsePirateBaseInfo(current);
			}
			yield break;
		}
		public SystemKillerInfo GetSystemKillerInfoByFleetID(int fleetId)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSystemKillerInfoByFleetID, fleetId), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseSystemKillerInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public MorrigiRelicInfo ParseMorrigiRelicInfo(Row row)
		{
			return new MorrigiRelicInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				FleetId = row[2].SQLiteValueToInteger(),
				IsAggressive = row[3].SQLiteValueToBoolean()
			};
		}
		public void UpdateMorrigiRelicInfo(MorrigiRelicInfo mi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateMorrigiRelicInfo, new object[]
			{
				mi.Id,
				mi.SystemId,
				mi.FleetId,
				mi.IsAggressive.ToSQLiteValue()
			}), false, true);
		}
		public void InsertMorrigiRelicInfo(MorrigiRelicInfo mi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertMorrigiRelicInfo, new object[]
			{
				EasterEgg.EE_MORRIGI_RELIC.ToString(),
				mi.SystemId,
				mi.FleetId,
				mi.IsAggressive.ToSQLiteValue()
			}), false, true);
		}
		public MorrigiRelicInfo GetMorrigiRelicInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetMorrigiRelicInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseMorrigiRelicInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<MorrigiRelicInfo> GetMorrigiRelicInfos()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetMorrigiRelicInfos, true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseMorrigiRelicInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public AsteroidMonitorInfo ParseAsteroidMonitorInfo(Row row)
		{
			return new AsteroidMonitorInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				OrbitalId = row[2].SQLiteValueToInteger(),
				IsAggressive = row[3].SQLiteValueToBoolean()
			};
		}
		public void UpdateAsteroidMonitorInfo(AsteroidMonitorInfo gi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAsteroidMonitorInfo, new object[]
			{
				gi.Id,
				gi.SystemId,
				gi.OrbitalId,
				gi.IsAggressive.ToSQLiteValue()
			}), false, true);
		}
		public void InsertAsteroidMonitorInfo(AsteroidMonitorInfo gi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertAsteroidMonitorInfo, new object[]
			{
				EasterEgg.EE_ASTEROID_MONITOR.ToString(),
				gi.SystemId,
				gi.OrbitalId,
				gi.IsAggressive.ToSQLiteValue()
			}), false, true);
		}
		public IEnumerable<AsteroidMonitorInfo> GetAllAsteroidMonitorInfos()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetAsteroidMonitorInfos, true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseAsteroidMonitorInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public AsteroidMonitorInfo GetAsteroidMonitorInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAsteroidMonitorInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseAsteroidMonitorInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public GardenerInfo ParseGardenerInfo(Row row)
		{
			GardenerInfo gardenerInfo = new GardenerInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				FleetId = row[2].SQLiteValueToInteger(),
				GardenerFleetId = row[3].SQLiteValueToInteger(),
				TurnsToWait = row[4].SQLiteValueToInteger(),
				IsGardener = row[5].SQLiteValueToBoolean(),
				DeepSpaceSystemId = row[6].SQLiteValueToNullableInteger(),
				DeepSpaceOrbitalId = row[7].SQLiteValueToNullableInteger()
			};
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetProteanShipOrbitMaps, gardenerInfo.Id), true);
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row2 = rows[i];
				gardenerInfo.ShipOrbitMap.Add(row2[0].SQLiteValueToInteger(), row2[1].SQLiteValueToInteger());
			}
			return gardenerInfo;
		}
		public void InsertProteanShipOrbitMap(int encounterId, int shipId, int orbitalObjectId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertProteanShipOrbitMap, encounterId, shipId, orbitalObjectId), false, true);
		}
		public void UpdateGardenerInfo(GardenerInfo gi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateGardenerInfo, new object[]
			{
				gi.Id,
				gi.SystemId.ToSQLiteValue(),
				gi.GardenerFleetId.ToSQLiteValue(),
				gi.TurnsToWait.ToSQLiteValue()
			}), false, true);
		}
		public void InsertGardenerInfo(GardenerInfo gi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertGardenerInfo, new object[]
			{
				gi.IsGardener ? EasterEgg.GM_GARDENER.ToString() : EasterEgg.EE_GARDENERS.ToString(),
				gi.SystemId.ToSQLiteValue(),
				gi.FleetId.ToSQLiteValue(),
				gi.GardenerFleetId.ToSQLiteValue(),
				gi.TurnsToWait.ToSQLiteValue(),
				gi.IsGardener.ToSQLiteValue(),
				gi.DeepSpaceSystemId.ToNullableSQLiteValue(),
				gi.DeepSpaceOrbitalId.ToNullableSQLiteValue()
			}), false, true);
		}
		public GardenerInfo GetGardenerInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetGardenerInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseGardenerInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<GardenerInfo> GetGardenerInfos()
		{
			Table table = this.db.ExecuteTableQuery(Queries.GetGardenerInfos, true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseGardenerInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public SwarmerInfo ParseSwarmerInfo(Row row)
		{
			return new SwarmerInfo
			{
				Id = int.Parse(row[0]),
				SystemId = int.Parse(row[1]),
				OrbitalId = int.Parse(row[2]),
				GrowthStage = int.Parse(row[3]),
				HiveFleetId = row[4].SQLiteValueToNullableInteger(),
				QueenFleetId = row[5].SQLiteValueToNullableInteger(),
				SpawnHiveDelay = row[6].SQLiteValueToInteger()
			};
		}
		public void UpdateSwarmerInfo(SwarmerInfo si)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSwarmerInfo, new object[]
			{
				si.Id,
				si.SystemId,
				si.OrbitalId,
				si.GrowthStage,
				si.HiveFleetId.ToNullableSQLiteValue(),
				si.QueenFleetId.ToNullableSQLiteValue(),
				si.SpawnHiveDelay
			}), false, true);
		}
		public void InsertSwarmerInfo(SwarmerInfo si)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertSwarmerInfo, new object[]
			{
				EasterEgg.EE_SWARM.ToString(),
				si.SystemId,
				si.OrbitalId,
				si.GrowthStage,
				si.HiveFleetId.ToNullableSQLiteValue(),
				si.QueenFleetId.ToNullableSQLiteValue(),
				si.SpawnHiveDelay
			}), false, true);
		}
		public SwarmerInfo GetSwarmerInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSwarmerInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseSwarmerInfo(table.Rows.First<Row>());
			}
			return null;
		}
		public IEnumerable<SwarmerInfo> GetSwarmerInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetSwarmerInfos, new object[0]), true))
			{
				yield return this.ParseSwarmerInfo(current);
			}
			yield break;
		}
		public VonNeumannTargetInfo ParseVonNeumannTargetInfo(Row row)
		{
			return new VonNeumannTargetInfo
			{
				SystemId = int.Parse(row[0]),
				ThreatLevel = int.Parse(row[1])
			};
		}
		public VonNeumannInfo ParseVonNeumannInfo(Row row)
		{
			VonNeumannInfo vonNeumannInfo = new VonNeumannInfo
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				OrbitalId = row[2].SQLiteValueToInteger(),
				Resources = row[3].SQLiteValueToInteger(),
				ConstructionProgress = row[4].SQLiteValueToInteger(),
				ResourcesCollectedLastTurn = row[5].SQLiteValueToInteger(),
				ProjectDesignId = row[6].SQLiteValueToNullableInteger(),
				LastCollectionSystem = row[7].SQLiteValueToInteger(),
				LastTargetSystem = row[8].SQLiteValueToInteger(),
				LastCollectionTurn = row[9].SQLiteValueToInteger(),
				LastTargetTurn = row[10].SQLiteValueToInteger(),
				FleetId = row[11].SQLiteValueToNullableInteger()
			};
			if (vonNeumannInfo.ProjectDesignId == 0)
			{
				vonNeumannInfo.ProjectDesignId = null;
			}
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetVonNeumannTargetInfos, vonNeumannInfo.Id), true);
			foreach (Row current in table)
			{
				vonNeumannInfo.TargetInfos.Add(this.ParseVonNeumannTargetInfo(current));
			}
			return vonNeumannInfo;
		}
		public IEnumerable<VonNeumannInfo> GetVonNeumannInfos()
		{
			foreach (Row current in this.db.ExecuteTableQuery(string.Format(Queries.GetVonNeumannInfos, new object[0]), true))
			{
				yield return this.ParseVonNeumannInfo(current);
			}
			yield break;
		}
		public void UpdateVonNeumannTargetInfo(int encounterId, VonNeumannTargetInfo vi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateVonNeumannTargetInfo, encounterId, vi.SystemId, vi.ThreatLevel), false, true);
		}
		public void RemoveVonNeumannTargetInfo(int encounterId, int systemId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveVonNeumannTargetInfo, encounterId, systemId), false, true);
		}
		public void InsertVonNeumannInfo(VonNeumannInfo vi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertVonNeumannInfo, new object[]
			{
				EasterEgg.EE_VON_NEUMANN.ToString(),
				vi.SystemId,
				vi.OrbitalId,
				vi.FleetId.ToNullableSQLiteValue(),
				vi.Resources,
				vi.ConstructionProgress
			}), false, true);
		}
		public void UpdateVonNeumannInfo(VonNeumannInfo vi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateVonNeumannInfo, new object[]
			{
				vi.Id.ToSQLiteValue(),
				vi.SystemId.ToSQLiteValue(),
				vi.OrbitalId.ToSQLiteValue(),
				vi.Resources.ToSQLiteValue(),
				vi.ConstructionProgress.ToSQLiteValue(),
				vi.ResourcesCollectedLastTurn.ToSQLiteValue(),
				vi.ProjectDesignId.ToNullableSQLiteValue(),
				vi.LastCollectionSystem.ToSQLiteValue(),
				vi.LastTargetSystem.ToSQLiteValue(),
				vi.LastCollectionTurn.ToSQLiteValue(),
				vi.LastTargetTurn.ToSQLiteValue(),
				vi.FleetId.ToNullableSQLiteValue()
			}), false, true);
			foreach (VonNeumannTargetInfo current in vi.TargetInfos)
			{
				this.UpdateVonNeumannTargetInfo(vi.Id, current);
			}
		}
		public VonNeumannInfo GetVonNeumannInfo(int id)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetVonNeumannInfo, id), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseVonNeumannInfo(table.Rows.First<Row>());
			}
			return null;
		}
		private int InsertLimboFleet(int playerID, int systemID)
		{
			return this.InsertFleetCore(playerID, 0, systemID, 0, "Limbo", FleetType.FL_LIMBOFLEET);
		}
		public int InsertOrGetLimboFleetID(int systemID, int playerID)
		{
			FleetInfo fleetInfo = this.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_LIMBOFLEET).FirstOrDefault<FleetInfo>();
			int result;
			if (fleetInfo == null)
			{
				result = this.InsertLimboFleet(playerID, systemID);
			}
			else
			{
				result = fleetInfo.ID;
			}
			return result;
		}
		public void RetrofitShip(int shipid, int systemid, int playerid, int? retrofitorderid = null)
		{
			int toFleetID = this.InsertOrGetLimboFleetID(systemid, playerid);
			this.TransferShip(shipid, toFleetID);
			if (!retrofitorderid.HasValue)
			{
				retrofitorderid = new int?(this.InsertInvoiceInstance(playerid, systemid, "Retrofit Order"));
			}
			ShipInfo shipInfo = this.GetShipInfo(shipid, true);
            DesignInfo newestRetrofitDesign = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(shipInfo.DesignInfo, this.GetDesignInfosForPlayer(playerid));
			this.InsertRetrofitOrder(systemid, newestRetrofitDesign.ID, shipid, new int?(retrofitorderid.Value));
		}
		public void RetrofitShips(IEnumerable<int> ships, int systemid, int playerid)
		{
			int value = this.InsertInvoiceInstance(playerid, systemid, "Retrofit Order");
			foreach (int current in ships)
			{
				this.RetrofitShip(current, systemid, playerid, new int?(value));
			}
		}
		private EmpireHistoryData ParseEmpireHistoryData(Row row)
		{
			return new EmpireHistoryData
			{
				id = row[0].SQLiteValueToInteger(),
				playerID = row[1].SQLiteValueToInteger(),
				turn = row[2].SQLiteValueToInteger(),
				colonies = row[3].SQLiteValueToInteger(),
				provinces = row[4].SQLiteValueToInteger(),
				bases = row[5].SQLiteValueToInteger(),
				fleets = row[6].SQLiteValueToInteger(),
				ships = row[7].SQLiteValueToInteger(),
				empire_pop = row[8].SQLiteValueToDouble(),
				empire_economy = row[9].SQLiteValueToSingle(),
				empire_biosphere = row[10].SQLiteValueToInteger(),
				empire_trade = row[11].SQLiteValueToDouble(),
				empire_morale = row[12].SQLiteValueToInteger(),
				savings = row[13].SQLiteValueToDouble(),
				psi_potential = row[14].SQLiteValueToInteger()
			};
		}
		public EmpireHistoryData GetLastEmpireHistoryForPlayer(int playerid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetEmpireHistoryByPlayerIDAndTurn, playerid, this.GetTurnCount() - 1), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseEmpireHistoryData(table.Rows[0]);
			}
			return null;
		}
		public int InsertEmpireHistoryForPlayer(EmpireHistoryData history)
		{
			this.RemoveEmpireHistoryForPlayer(history.playerID);
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertEmpireHistory, new object[]
			{
				history.playerID,
				this.GetTurnCount(),
				history.colonies,
				history.provinces,
				history.bases,
				history.fleets,
				history.ships,
				history.empire_pop,
				history.empire_economy,
				history.empire_biosphere,
				history.empire_trade,
				history.empire_morale,
				history.savings,
				history.psi_potential
			}));
		}
		public void RemoveEmpireHistoryForPlayer(int playerid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveEmpireHistoryByPlayerID, playerid), false, true);
		}
		private ColonyHistoryData ParseColonyHistoryData(Row row)
		{
			return new ColonyHistoryData
			{
				id = row[0].SQLiteValueToInteger(),
				colonyID = row[1].SQLiteValueToInteger(),
				turn = row[2].SQLiteValueToInteger(),
				resources = row[3].SQLiteValueToInteger(),
				biosphere = row[4].SQLiteValueToInteger(),
				infrastructure = row[5].SQLiteValueToSingle(),
				hazard = row[6].SQLiteValueToSingle(),
				income = row[7].SQLiteValueToInteger(),
				econ_rating = row[8].SQLiteValueToSingle(),
				life_support_cost = row[9].SQLiteValueToInteger(),
				industrial_output = row[10].SQLiteValueToInteger(),
				civ_pop = row[11].SQLiteValueToDouble(),
				imp_pop = row[12].SQLiteValueToDouble(),
				slave_pop = row[13].SQLiteValueToDouble()
			};
		}
		public ColonyHistoryData GetLastColonyHistoryForColony(int colonyid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetColonyHistoryByColonyIDAndTurn, colonyid, this.GetTurnCount() - 1), true);
			if (table.Rows.Count<Row>() > 0)
			{
				return this.ParseColonyHistoryData(table.Rows[0]);
			}
			return null;
		}
		public int InsertColonyHistory(ColonyHistoryData history)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertColonyHistory, new object[]
			{
				history.colonyID,
				this.GetTurnCount(),
				history.resources,
				history.biosphere,
				history.infrastructure,
				history.hazard,
				history.income,
				history.econ_rating,
				history.life_support_cost,
				history.industrial_output,
				history.civ_pop,
				history.imp_pop,
				history.slave_pop
			}));
		}
		public void RemoveColonyHistory(int colonyid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveColonyHistoryByColonyID, colonyid), false, true);
		}
		public void RemoveAllColonyHistory()
		{
			this.db.ExecuteNonQuery(Queries.RemoveAllColonyHistory, false, true);
		}
		private ColonyFactionMoraleHistory ParseColonyFactionMoraleHistory(Row row)
		{
			return new ColonyFactionMoraleHistory
			{
				id = row[0].SQLiteValueToInteger(),
				colonyID = row[1].SQLiteValueToInteger(),
				turn = row[2].SQLiteValueToInteger(),
				factionid = row[3].SQLiteValueToInteger(),
				morale = row[4].SQLiteValueToInteger(),
				population = row[5].SQLiteValueToDouble()
			};
		}
		public IEnumerable<ColonyFactionMoraleHistory> GetLastColonyMoraleHistoryForColony(int colonyid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetColonyFactionMoraleHistoryByColonyIDAndTurn, colonyid, this.GetTurnCount() - 1), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseColonyFactionMoraleHistory(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public int InsertColonyMoraleHistory(ColonyFactionMoraleHistory history)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertColonyFactionMoraleHistory, new object[]
			{
				history.colonyID,
				this.GetTurnCount(),
				history.factionid,
				history.morale,
				history.population
			}));
		}
		public void RemoveColonyMoraleHistory(int colonyid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveColonyFactionMoraleHistoryByColonyID, colonyid), false, true);
		}
		public IEnumerable<DesignInfo> GetDesignsEncountered(int playerid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetEnounteredDesigns, playerid.ToSQLiteValue()), true);
			List<DesignInfo> list = new List<DesignInfo>();
			Row[] rows = table.Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				DesignInfo di = this.GetDesignInfo(row[1].SQLiteValueToInteger());
				if (di != null && this.GetStandardPlayerIDs().Any((int x) => x == di.PlayerID))
				{
					list.Add(di);
				}
			}
			return list;
		}
		public void InsertDesignEncountered(int playerid, int designid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertEnounteredDesign, playerid.ToSQLiteValue(), designid.ToSQLiteValue()), false, true);
		}
		public void RemoveMoraleEventHistory(int eventid)
		{
			this._dom.morale_event_history.Remove(eventid);
		}
		public void RemoveAllMoraleEventsHistory()
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveAllMoraleHistoryEvents, this.GetTurnCount() - 1), false, true);
			this._dom.morale_event_history.Clear();
		}
		public IEnumerable<MoraleEventHistory> GetMoraleHistoryEventsForColony(int colonyid)
		{
			ColonyInfo ci = this.GetColonyInfo(colonyid);
			StarSystemInfo ssi = this.GetStarSystemInfo(ci.CachedStarSystemID);
			List<MoraleEventHistory> source = (
				from x in this._dom.morale_event_history
				where x.Value.turn == this.GetTurnCount() - 1 && x.Value.playerId == ci.PlayerID
				select x.Value).ToList<MoraleEventHistory>();
			return (
				from x in source
				where x.colonyId == colonyid || x.systemId == ci.CachedStarSystemID || (ssi.ProvinceID.HasValue && x.provinceId == ssi.ProvinceID) || (!x.colonyId.HasValue && !x.systemId.HasValue && !x.provinceId.HasValue)
				select x).ToList<MoraleEventHistory>();
		}
		public IEnumerable<MoraleEventHistory> GetMoraleHistoryEventsForPlayer(int playerid)
		{
			return (
				from x in this._dom.morale_event_history
				where x.Value.turn == this.GetTurnCount() - 1 && x.Value.playerId == playerid
				select x.Value).ToList<MoraleEventHistory>();
		}
		public int InsertMoralHistoryEvent(MoralEvent type, int playerid, int value, int? colonyid, int? systemid, int? provinceid)
		{
			MoraleEventHistory moraleEventHistory = new MoraleEventHistory();
			moraleEventHistory.playerId = playerid;
			moraleEventHistory.value = value;
			moraleEventHistory.moraleEvent = type;
			moraleEventHistory.colonyId = colonyid;
			moraleEventHistory.systemId = systemid;
			moraleEventHistory.provinceId = provinceid;
			moraleEventHistory.turn = this.GetTurnCount();
			return this._dom.morale_event_history.Insert(null, moraleEventHistory);
		}
		private IncomingRandomInfo ParseIncomingRandom(Row row)
		{
			return new IncomingRandomInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerId = row[1].SQLiteValueToInteger(),
				SystemId = row[2].SQLiteValueToInteger(),
				type = (RandomEncounter)row[3].SQLiteValueToInteger(),
				turn = row[4].SQLiteValueToInteger()
			};
		}
		public IEnumerable<IncomingRandomInfo> GetIncomingRandomsForPlayerThisTurn(int playerid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.SelectIncomingRandomsForPlayer, playerid, this.GetTurnCount()), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseIncomingRandom(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void InsertIncomingRandom(int playerid, int systemid, RandomEncounter type, int turntoappear)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIncomingRandom, new object[]
			{
				playerid.ToSQLiteValue(),
				systemid.ToSQLiteValue(),
				((int)type).ToSQLiteValue(),
				turntoappear.ToSQLiteValue()
			}), false, true);
		}
		public void RemoveIncomingRandom(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DeleteIncomingRandom, id.ToSQLiteValue()), false, true);
		}
		private IncomingGMInfo ParseIncomingGM(Row row)
		{
			return new IncomingGMInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				type = (EasterEgg)row[2].SQLiteValueToInteger(),
				turn = row[3].SQLiteValueToInteger()
			};
		}
		public IEnumerable<IncomingGMInfo> GetIncomingGMsThisTurn()
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.SelectIncomingGMsForPlayer, this.GetTurnCount()), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParseIncomingGM(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void InsertIncomingGM(int systemid, EasterEgg type, int turntoappear)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIncomingGM, systemid.ToSQLiteValue(), ((int)type).ToSQLiteValue(), turntoappear.ToSQLiteValue()), false, true);
		}
		public void RemoveIncomingGM(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DeleteIncomingGM, id.ToSQLiteValue()), false, true);
		}
		private PiracyFleetDetectionInfo ParsePiracyFleetDetetcionInfo(Row row)
		{
			return new PiracyFleetDetectionInfo
			{
				FleetID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToInteger()
			};
		}
		public IEnumerable<PiracyFleetDetectionInfo> GetPiracyFleetDetectionInfoForFleet(int fleetid)
		{
			Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPiracyFleetDectectionInfoForFleet, fleetid), true);
			try
			{
				Row[] rows = table.Rows;
				for (int i = 0; i < rows.Length; i++)
				{
					Row row = rows[i];
					yield return this.ParsePiracyFleetDetetcionInfo(row);
				}
			}
			finally
			{
			}
			yield break;
		}
		public void InsertPiracyFleetDetectionInfo(int fleetid, int playerID)
		{
			MissionInfo missionByFleetID = this.GetMissionByFleetID(fleetid);
			if (missionByFleetID != null && missionByFleetID.Type == MissionType.PIRACY)
			{
				this.db.ExecuteNonQuery(string.Format(Queries.InsertPiracyFleetDetection, fleetid.ToSQLiteValue(), playerID.ToSQLiteValue(), missionByFleetID.ID.ToSQLiteValue()), false, true);
			}
		}
		public static Matrix CalcTransform(int orbitalId, float t, IEnumerable<OrbitalObjectInfo> orbitals)
		{
			t = 0f;
			OrbitalObjectInfo orbitalObjectInfo = orbitals.First((OrbitalObjectInfo x) => x.ID == orbitalId);
			Matrix matrix = orbitalObjectInfo.OrbitalPath.GetTransform((double)t);
			if (orbitalObjectInfo.ParentID.HasValue && orbitalObjectInfo.ParentID.Value != 0)
			{
				Matrix lhs = GameDatabase.CalcTransform(orbitalObjectInfo.ParentID.Value, t, orbitals);
				matrix = lhs * matrix;
			}
			return matrix;
		}
		public int GetFleetSupportingSystem(int fleetID)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo != null && fleetInfo.SupportingSystemID != 0)
			{
				return fleetInfo.SupportingSystemID;
			}
			return 0;
		}
		public bool IsSurveyed(int playerID, int systemID)
		{
			return this.GetLastTurnExploredByPlayer(playerID, systemID) > 0;
		}
		public bool CanSurvey(int playerID, int systemID)
		{
			return !this.IsSurveyed(playerID, systemID);
		}
		public bool CanColonize(int playerID, int systemID, int MaxHazard)
		{
			if (this.GetLastTurnExploredByPlayer(playerID, systemID) < 1)
			{
				return false;
			}
			List<int> list = this.GetStarSystemPlanets(systemID).ToList<int>();
			foreach (int current in list)
			{
				if (this.CanColonizePlanet(playerID, current, MaxHazard) && this.hasPermissionToBuildEnclave(playerID, current))
				{
					return true;
				}
			}
			return false;
		}
		public bool CanRelocate(GameSession sim, int playerId, int systemId)
		{
			return this.GetRemainingSupportPoints(sim, systemId, playerId) > 0;
		}
		public int GetFleetCruiserEquivalent(int fleetId)
		{
			int num = 0;
			List<ShipInfo> list = this.GetShipInfoByFleetID(fleetId, true).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				num += this.GetShipCruiserEquivalent(current.DesignInfo);
			}
			return num;
		}
		public int GetShipCruiserEquivalent(DesignInfo shipDesign)
		{
			if (shipDesign == null || shipDesign.IsPlatform() || shipDesign.IsSDB())
			{
				return 0;
			}
			switch (shipDesign.Class)
			{
			case ShipClass.Cruiser:
				return 1;
			case ShipClass.Dreadnought:
				return 3;
			case ShipClass.Leviathan:
				return 9;
			default:
				return 0;
			}
		}
		public int GetFleetCruiserEquivalentEstimate(int fleetId, int shipid)
		{
			int num = 0;
			List<ShipInfo> list = this.GetShipInfoByFleetID(fleetId, false).ToList<ShipInfo>();
			foreach (ShipInfo current in list)
			{
				if (!current.DesignInfo.IsPlatform() && !current.DesignInfo.IsSDB())
				{
					switch (this.GetDesignInfo(current.DesignID).Class)
					{
					case ShipClass.Cruiser:
						num++;
						break;
					case ShipClass.Dreadnought:
						num += 3;
						break;
					case ShipClass.Leviathan:
						num += 9;
						break;
					}
				}
			}
			ShipInfo shipInfo = this.GetShipInfo(shipid, true);
			if (!shipInfo.DesignInfo.IsPlatform() && !shipInfo.DesignInfo.IsSDB())
			{
				switch (shipInfo.DesignInfo.Class)
				{
				case ShipClass.Cruiser:
					num++;
					break;
				case ShipClass.Dreadnought:
					num += 3;
					break;
				case ShipClass.Leviathan:
					num += 9;
					break;
				}
			}
			return num;
		}
		public string GetGameName()
		{
			return this.GetNameValue("game_name");
		}
		public int GetRemainingSupportPoints(GameSession sim, int systemId, int playerId)
		{
			int num = 0;
			List<FleetInfo> list = this.GetFleetsBySupportingSystem(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			List<MissionInfo> list2 = (
				from x in this.GetMissionsBySystemDest(systemId)
				where x.Type == MissionType.RELOCATION
				select x).ToList<MissionInfo>();
			foreach (MissionInfo current in list2)
			{
				FleetInfo fleetInfo = this.GetFleetInfo(current.FleetID);
                if (fleetInfo != null && fleetInfo.PlayerID == playerId && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(sim, fleetInfo) && fleetInfo.Type != FleetType.FL_CARAVAN && !list.Contains(fleetInfo))
				{
					list.Add(fleetInfo);
				}
			}
			foreach (FleetInfo current2 in list)
			{
				MissionInfo missionByFleetID = this.GetMissionByFleetID(current2.ID);
				if (current2.PlayerID == playerId && (current2.SupportingSystemID != systemId || missionByFleetID == null || missionByFleetID.Type != MissionType.RELOCATION))
				{
					num += this.GetFleetCruiserEquivalent(current2.ID);
				}
			}
			return this.GetSystemSupportedCruiserEquivalent(sim, systemId, playerId) - num;
		}
		public int GetSystemSupportedCruiserEquivalent(GameSession sim, int systemid, int playerId)
		{
			List<ColonyInfo> list = (
				from x in sim.GameDatabase.GetColonyInfosForSystem(systemid)
				where x.PlayerID == playerId && x.CurrentStage != ColonyStage.Colony
				select x).ToList<ColonyInfo>();
			int num = list.Count * sim.AssetDatabase.ColonyFleetSupportPoints;
			List<StationInfo> list2 = sim.GameDatabase.GetStationForSystemAndPlayer(systemid, playerId).ToList<StationInfo>();
			foreach (StationInfo current in list2)
			{
				num += this.GetStationSupportedCruiserEquivalent(sim, current);
			}
			return num;
		}
		private int GetStationSupportedCruiserEquivalent(GameSession sim, StationInfo stationInfo)
		{
			if (stationInfo.DesignInfo.StationType != StationType.NAVAL)
			{
				return 0;
			}
			List<DesignModuleInfo> list = (
				from x in stationInfo.DesignInfo.DesignSections[0].Modules
				where x.StationModuleType == ModuleEnums.StationModuleType.Warehouse
				select x).ToList<DesignModuleInfo>();
			int num = list.Count * 2;
			switch (stationInfo.DesignInfo.StationLevel)
			{
			case 1:
				return sim.AssetDatabase.StationLvl1FleetSupportPoints + num;
			case 2:
				return sim.AssetDatabase.StationLvl2FleetSupportPoints + num;
			case 3:
				return sim.AssetDatabase.StationLvl3FleetSupportPoints + num;
			case 4:
				return sim.AssetDatabase.StationLvl4FleetSupportPoints + num;
			case 5:
				return sim.AssetDatabase.StationLvl5FleetSupportPoints + num;
			default:
				return 0;
			}
		}
		public bool CanSupportPlanet(int playerId, int planetId)
		{
			ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(planetId);
			if (colonyInfoForPlanet != null)
			{
				PlanetInfo planetInfo = this.GetPlanetInfo(planetId);
				if (colonyInfoForPlanet.PlayerID == playerId)
				{
					if (this.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerId))
					{
						if (planetInfo.Biosphere > 0)
						{
							return true;
						}
					}
					else
					{
						if (this.GetPlanetHazardRating(playerId, planetId, false) != 0f || planetInfo.Infrastructure != 1f)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		public bool CanInvadePlanet(int playerID, int planetId)
		{
			AIColonyIntel colonyIntelForPlanet = this.GetColonyIntelForPlanet(playerID, planetId);
			return colonyIntelForPlanet != null && playerID != colonyIntelForPlanet.OwningPlayerID;
		}
		public bool CanColonizePlanet(int playerID, int planetId, int MaxHazard)
		{
			PlanetInfo planetInfo = this.GetPlanetInfo(planetId);
			if (planetInfo != null && StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
			{
				AIColonyIntel colonyIntelForPlanet = this.GetColonyIntelForPlanet(playerID, planetId);
				bool flag = colonyIntelForPlanet == null;
				float planetHazardRating = this.GetPlanetHazardRating(playerID, planetId, true);
				bool flag2 = planetHazardRating <= (float)MaxHazard || this.assetdb.GetFaction(this.GetPlayerInfo(playerID).FactionID).Name == "loa";
				if (flag && flag2)
				{
					return true;
				}
			}
			return false;
		}
		public bool IsHazardousPlanet(int playerID, int planetId, int MaxHazard)
		{
			PlanetInfo planetInfo = this.GetPlanetInfo(planetId);
			if (planetInfo != null && StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
			{
				float planetHazardRating = this.GetPlanetHazardRating(playerID, planetId, true);
				bool flag = planetHazardRating <= (float)MaxHazard;
				if (flag)
				{
					return false;
				}
			}
			return true;
		}
		public bool CanConstructStation(App game, int playerId, int systemId, bool canBuildGate)
		{
			if (!this.IsSurveyed(playerId, systemId))
			{
				return false;
			}
			if (!this.GetColonyInfosForSystem(systemId).Any((ColonyInfo x) => x.PlayerID == playerId) && !this.GetStratModifier<bool>(StratModifiers.AllowDeepSpaceConstruction, playerId) && !this.GetColonyInfosForSystem(systemId).Any((ColonyInfo x) => x.IsIndependentColony(game)))
			{
				return false;
			}
			StationTypeFlags stationTypeFlags = (StationTypeFlags)0;
			for (int i = 0; i < 8; i++)
			{
				stationTypeFlags |= ((StationType)i).ToFlags();
			}
			if (!canBuildGate)
			{
				stationTypeFlags &= ~StationTypeFlags.GATE;
			}
			for (int j = 0; j < 8; j++)
			{
				if ((stationTypeFlags & ((StationType)j).ToFlags()) != (StationTypeFlags)0 && this.GetStationForSystemPlayerAndType(systemId, playerId, (StationType)j) == null)
				{
					return true;
				}
			}
			return false;
		}
		public bool CanPatrol(int playerID, int systemID)
		{
			return this.GetLastTurnExploredByPlayer(playerID, systemID) >= 1;
		}
		public bool CanInterdict(int playerID, int systemID)
		{
			return this.SystemContainsEnemyColony(playerID, systemID) || (this.SystemContainsEnemyStation(playerID, systemID) && StarMap.IsInRange(this, playerID, systemID));
		}
		public bool CanStrike(int playerID, int systemID)
		{
			return !this.GetSystemOwningPlayer(systemID).HasValue || this.GetSystemOwningPlayer(systemID) != playerID;
		}
		public bool CanInvade(int playerID, int systemID)
		{
			return this.SystemContainsEnemyColony(playerID, systemID);
		}
		public bool CanSupport(int playerID, int systemID)
		{
			return this.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>().Any((ColonyInfo x) => x.PlayerID == playerID && this.CanSupportPlanet(x.PlayerID, x.OrbitalObjectID));
		}
		public bool CanPirate(int playerID, int systemID)
		{
			int? systemOwningPlayer = this.GetSystemOwningPlayer(systemID);
			return systemOwningPlayer.HasValue && this.GetStratModifier<bool>(StratModifiers.AllowPrivateerMission, playerID, false) && systemOwningPlayer.Value != playerID;
		}
		public bool PirateFleetVisibleToPlayer(int fleetID, int PlayerID)
		{
			MissionInfo missionByFleetID = this.GetMissionByFleetID(fleetID);
			if (missionByFleetID == null || missionByFleetID.Type != MissionType.PIRACY)
			{
				return true;
			}
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo.PlayerID == PlayerID)
			{
				return true;
			}
			List<PiracyFleetDetectionInfo> source = this.GetPiracyFleetDetectionInfoForFleet(fleetID).ToList<PiracyFleetDetectionInfo>();
			return source.Any((PiracyFleetDetectionInfo x) => x.PlayerID == PlayerID);
		}
		public bool SystemHasAccelerator(int systemid, int playerid)
		{
			bool result = false;
			IEnumerable<FleetInfo> fleetInfoBySystemID = this.GetFleetInfoBySystemID(systemid, FleetType.FL_ACCELERATOR);
			foreach (FleetInfo current in fleetInfoBySystemID)
			{
				if (current.PlayerID == playerid)
				{
					result = true;
					break;
				}
			}
			return result;
		}
		public bool CanGate(int playerID, int systemID)
		{
			return !this.SystemHasGate(systemID, playerID);
		}
		public bool SystemHasGate(int systemid, int playerid)
		{
			bool flag = false;
			DataObjectCache.SystemPlayerID key = new DataObjectCache.SystemPlayerID
			{
				SystemID = systemid,
				PlayerID = playerid
			};
			if (!this._dom.CachedSystemHasGateFlags.TryGetValue(key, out flag))
			{
				IEnumerable<FleetInfo> fleetInfoBySystemID = this.GetFleetInfoBySystemID(systemid, FleetType.FL_GATE);
				foreach (FleetInfo current in fleetInfoBySystemID)
				{
					if (current.PlayerID == playerid)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					IEnumerable<StationInfo> stationForSystemAndPlayer = this.GetStationForSystemAndPlayer(systemid, playerid);
					foreach (StationInfo current2 in stationForSystemAndPlayer)
					{
						if (current2.DesignInfo.StationType == StationType.GATE && current2.DesignInfo.StationLevel > 0)
						{
							flag = true;
							break;
						}
					}
				}
				this._dom.CachedSystemHasGateFlags.Add(key, flag);
			}
			return flag;
		}
		public bool SystemContainsEnemyStation(int playerID, int systemID)
		{
			return (
				from x in this.GetStationForSystem(systemID)
				where x.PlayerID != playerID
				select x).Count<StationInfo>() > 0;
		}
		public bool SystemContainsEnemyColony(int playerID, int systemID)
		{
			foreach (OrbitalObjectInfo current in this.GetStarSystemOrbitalObjectInfos(systemID))
			{
				AIColonyIntel colonyIntelForPlanet = this.GetColonyIntelForPlanet(playerID, current.ID);
				if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != playerID && !this.GetPlayerInfo(playerID).IsOnTeam(this.GetPlayerInfo(colonyIntelForPlanet.OwningPlayerID)))
				{
					return true;
				}
			}
			return false;
		}
		public int GetShipCommandPointQuota(int shipId)
		{
			return this.GetDesignCommandPointQuota(this.assetdb, this.GetShipInfo(shipId, false).DesignID);
		}
		public int GetDesignCommandPointQuota(AssetDatabase adb, int designID)
		{
			int num = 0;
			DesignInfo designInfo = this.GetDesignInfo(designID);
			int num2 = 0;
			if (this.PlayerHasTech(designInfo.PlayerID, "CCC_Combat_Algorithms"))
			{
				num2 += adb.GetTechBonus<int>("CCC_Combat_Algorithms", "commandpoints");
			}
			if (this.PlayerHasTech(designInfo.PlayerID, "CCC_Flag_Central_Command"))
			{
				num2 += adb.GetTechBonus<int>("CCC_Flag_Central_Command", "commandpoints");
			}
			if (this.PlayerHasTech(designInfo.PlayerID, "CCC_Holo-Tactics"))
			{
				num2 += adb.GetTechBonus<int>("CCC_Holo-Tactics", "commandpoints");
			}
			if (this.PlayerHasTech(designInfo.PlayerID, "PSI_Warmind"))
			{
				num2 += adb.GetTechBonus<int>("PSI_Warmind", "commandpoints");
			}
			if (designInfo != null)
			{
				DesignSectionInfo[] designSections = designInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					using (List<DesignModuleInfo>.Enumerator enumerator = designSectionInfo.Modules.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.StationModuleType == ModuleEnums.StationModuleType.Command)
							{
								num2++;
							}
						}
					}
					num = Math.Max(num, adb.GetShipSectionAsset(designSectionInfo.FilePath).CommandPoints);
				}
			}
			if (num != 0)
			{
				return num + num2;
			}
			return 0;
		}
		public int GetCommandPointCost(int designID)
		{
			DesignInfo designInfo = this.GetDesignInfo(designID);
			return designInfo.CommandPointCost;
		}
		public int GetShipsCommandPointQuota(IEnumerable<ShipInfo> ships)
		{
			int num = 0;
			foreach (ShipInfo current in ships)
			{
				num = Math.Max(num, this.GetShipCommandPointQuota(current.ID));
			}
			return num;
		}
		public int GetShipsCommandPointCost(IEnumerable<ShipInfo> ships)
		{
			int num = 0;
			foreach (ShipInfo current in ships)
			{
				num += this.GetShipCommandPointCost(current.ID, false);
			}
			return num;
		}
		public int GetFleetCommandPointQuota(int fleetid)
		{
			return this.GetShipsCommandPointQuota(this.GetShipInfoByFleetID(fleetid, false));
		}
		public int GetFleetCommandPointCost(int fleetid)
		{
			return this.GetShipsCommandPointCost(this.GetShipInfoByFleetID(fleetid, false));
		}
		public int GetShipCommandPointCost(int shipID, bool battleriders)
		{
			int result = 0;
			ShipInfo shipInfo = this.GetShipInfo(shipID, false);
			DesignInfo designInfo = this.GetDesignInfo(shipInfo.DesignID);
			switch (designInfo.Class)
			{
			case ShipClass.Cruiser:
				result = 6;
				break;
			case ShipClass.Dreadnought:
				result = 18;
				break;
			case ShipClass.Leviathan:
				result = 54;
				break;
			case ShipClass.BattleRider:
				result = 0;
				break;
			}
			return result;
		}
		public bool IsPlayerAdjacent(GameSession game, int toPlayerID, int otherPlayerID)
		{
			float num = this.FindCurrentDriveSpeedForPlayer(toPlayerID);
			if (this.GetFactionName(this.GetPlayerFactionID(toPlayerID)).Contains("hiver"))
			{
				num *= 12f;
			}
			else
			{
				num *= 2f;
			}
			num = 1000f;
			foreach (AIColonyIntel current in this.GetColonyIntelOfTargetPlayer(toPlayerID, otherPlayerID))
			{
				if (current.ColonyID.HasValue)
				{
					ColonyInfo colonyInfo = this.GetColonyInfo(current.ColonyID.Value);
					if (colonyInfo != null)
					{
						OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
						StarSystemInfo starSystemInfo = this.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
						Vector3 origin = starSystemInfo.Origin;
						foreach (StarSystemInfo current2 in this.GetSystemsInRange(origin, num))
						{
							PlanetInfo[] starSystemPlanetInfos = this.GetStarSystemPlanetInfos(current2.ID);
							for (int i = 0; i < starSystemPlanetInfos.Length; i++)
							{
								PlanetInfo planetInfo = starSystemPlanetInfos[i];
								ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(planetInfo.ID);
								if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == toPlayerID)
								{
									return true;
								}
							}
						}
					}
				}
			}
			return false;
		}
		public DiplomacyState GetDiplomacyStateBetweenPlayers(int playerID, int otherPlayerID)
		{
			if (playerID == 0 || otherPlayerID == 0)
			{
				return DiplomacyState.WAR;
			}
			DiplomacyState state = this.GetDiplomacyInfo(playerID, otherPlayerID).State;
			DiplomacyState state2 = this.GetDiplomacyInfo(otherPlayerID, playerID).State;
			if (state != state2)
			{
				throw new InvalidDataException("Inconsistency in player diplomacy states");
			}
			return state;
		}
		public bool ShouldPlayersFight(int playerID1, int playerID2, int systemID)
		{
			if (GameDatabase.PlayersAlwaysFight && playerID1 != playerID2)
			{
				return true;
			}
			DiplomacyState diplomacyStateBetweenPlayers = this.GetDiplomacyStateBetweenPlayers(playerID1, playerID2);
			return diplomacyStateBetweenPlayers == DiplomacyState.WAR || diplomacyStateBetweenPlayers == DiplomacyState.UNKNOWN;
		}
		public float FindCurrentDriveSpeedForPlayer(int playerID)
		{
			float num = -1f;
			foreach (DesignInfo current in this.GetDesignInfosForPlayer(playerID))
			{
				foreach (ShipSectionAsset current2 in 
					from x in current.DesignSections
					select x.ShipSectionAsset)
				{
					if (current2 != null)
					{
						if (current2.NodeSpeed > 0f && current2.NodeSpeed > num)
						{
							num = current2.NodeSpeed;
						}
						else
						{
							if (current2.FtlSpeed > 0f && current2.FtlSpeed > num)
							{
								num = current2.FtlSpeed;
							}
						}
					}
				}
			}
			return num;
		}
		public float GetHiverCastingDistance(int playerId)
		{
			Random safeRandom = App.GetSafeRandom();
			float stratModifier = this.GetStratModifier<float>(StratModifiers.GateCastDistance, playerId);
			float num = stratModifier * this.GetStratModifier<float>(StratModifiers.GateCastDeviation, playerId);
			return stratModifier + (-num + safeRandom.NextSingle() * (num * 2f));
		}
		public IEnumerable<StarSystemInfo> GetSystemsInRange(Vector3 from, float range)
		{
			List<StarSystemInfo> list = new List<StarSystemInfo>();
			foreach (StarSystemInfo current in this.GetStarSystemInfos())
			{
				float length = (from - current.Origin).Length;
				if (length <= range)
				{
					list.Add(current);
				}
			}
			return list;
		}
		public bool PlayerHasTech(int playerID, string techName)
		{
			int techID = this.GetTechID(techName);
			if (techID > 0)
			{
				PlayerTechInfo playerTechInfo = this.GetPlayerTechInfo(playerID, techID);
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				{
					return true;
				}
			}
			return false;
		}
		public bool PlayerHasAntimatter(int playerID)
		{
			return this.PlayerHasTech(playerID, "NRG_Anti-Matter");
		}
		public bool PlayerHasDreadnoughts(int playerID)
		{
			return this.PlayerHasTech(playerID, "ENG_Dreadnought_Construction");
		}
		public bool PlayerHasLeviathans(int playerID)
		{
			return this.PlayerHasTech(playerID, "ENG_Leviathian_Construction");
		}
		public float GetLaggingTechWeightBonus(int playerID, string family)
		{
			double num = 0.0;
			double num2 = 0.0;
			int num3 = 0;
			foreach (AITechWeightInfo current in this.GetAITechWeightInfos(playerID))
			{
				num2 += current.TotalSpent;
				num3++;
				if (current.Family == family)
				{
					num = current.TotalSpent;
				}
			}
			if (num3 > 0)
			{
				num2 /= (double)num3;
			}
			if (num < num2 * 0.5)
			{
				return 1.5f;
			}
			if (num < num2)
			{
				return 1.2f;
			}
			return 1f;
		}
		public static ShipSectionAsset GetDesignSection(GameSession game, int designID, ShipSectionType sectionType)
		{
			foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(designID))
			{
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
				if (shipSectionAsset != null && shipSectionAsset.Type == sectionType)
				{
					return shipSectionAsset;
				}
			}
			return null;
		}
		public string GetDefaultShipName(int designID, int serial)
		{
			DesignInfo designInfo = this.GetDesignInfo(designID);
			string text = designInfo.Name;
			if (text == null)
			{
				switch (designInfo.Class)
				{
				case ShipClass.Cruiser:
					text = "CR";
					break;
				case ShipClass.Dreadnought:
					text = "DN";
					break;
				case ShipClass.Leviathan:
					text = "LV";
					break;
				case ShipClass.BattleRider:
					text = "DE";
					break;
				}
				text = text + "-" + designID;
			}
			return text + "-" + serial;
		}
		public float ScoreSystemAsPeripheral(int systemID, int playerID)
		{
			Vector3 vector = Vector3.Zero;
			int num = 0;
			foreach (ColonyInfo current in this.GetPlayerColoniesByPlayerId(playerID))
			{
				OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(current.OrbitalObjectID);
				Vector3 origin = this.GetStarSystemInfo(orbitalObjectInfo.StarSystemID).Origin;
				vector += origin;
				num++;
			}
			if (num == 0)
			{
				return 0f;
			}
			vector /= (float)num;
			float num2 = 0f;
			foreach (ColonyInfo current2 in this.GetPlayerColoniesByPlayerId(playerID))
			{
				OrbitalObjectInfo orbitalObjectInfo2 = this.GetOrbitalObjectInfo(current2.OrbitalObjectID);
				Vector3 origin2 = this.GetStarSystemInfo(orbitalObjectInfo2.StarSystemID).Origin;
				num2 += (origin2 - vector).Length;
			}
			float num3 = num2 / (float)num;
			Vector3 starSystemOrigin = this.GetStarSystemOrigin(systemID);
			float length = (vector - starSystemOrigin).Length;
			return length / num3;
		}
		public string GetMapName()
		{
			return this.GetNameValue("map_name");
		}
		public double GetUnixTimeCreated()
		{
			string nameValue = this.GetNameValue("time_stamp");
			if (string.IsNullOrEmpty(nameValue))
			{
				return 0.0;
			}
			return double.Parse(nameValue);
		}
	}
}
