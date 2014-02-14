using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class AsteroidMonitor
	{
		private const string FactionName = "morrigirelics";
		private const string PlayerName = "Asteroid Monitor";
		private const string PlayerAvatar = "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga";
		private const string FleetName = "Asteroid Monitor";
		private const string MonitorDesignFile = "sn_monitor.section";
		private const string MonitorCommandDesignFile = "sn_monitorcommander.section";
		private int PlayerId = -1;
		private int _monitorDesignId;
		private int _monitorCommandDesignId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int MonitorDesignId
		{
			get
			{
				return this._monitorDesignId;
			}
		}
		public int MonitorCommandDesignId
		{
			get
			{
				return this._monitorCommandDesignId;
			}
		}
		public static AsteroidMonitor InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			AsteroidMonitor asteroidMonitor = new AsteroidMonitor();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Asteroid Monitor"));
			if (playerInfo == null)
			{
				asteroidMonitor.PlayerId = gamedb.InsertPlayer("Asteroid Monitor", "morrigirelics", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			else
			{
				asteroidMonitor.PlayerId = playerInfo.ID;
			}
			List<DesignInfo> designs = gamedb.GetDesignInfosForPlayer(asteroidMonitor.PlayerId).ToList<DesignInfo>();
			asteroidMonitor._monitorDesignId = gamedb.AddOrGetEncounterDesignInfo(designs, asteroidMonitor.PlayerId, "Monitor", "morrigirelics", new string[]
			{
				"sn_monitor.section"
			});
			asteroidMonitor._monitorCommandDesignId = gamedb.AddOrGetEncounterDesignInfo(designs, asteroidMonitor.PlayerId, "Monitor Command", "morrigirelics", new string[]
			{
				"sn_monitorcommander.section"
			});
			return asteroidMonitor;
		}
		public static AsteroidMonitor ResumeEncounter(GameDatabase gamedb)
		{
			AsteroidMonitor asteroidMonitor = new AsteroidMonitor();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Asteroid Monitor"));
			if (playerInfo == null)
			{
				asteroidMonitor.PlayerId = gamedb.InsertPlayer("Asteroid Monitor", "morrigirelics", null, new Vector3(0f), new Vector3(0f), "", "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			else
			{
				asteroidMonitor.PlayerId = playerInfo.ID;
			}
			List<DesignInfo> designs = gamedb.GetDesignInfosForPlayer(asteroidMonitor.PlayerId).ToList<DesignInfo>();
			asteroidMonitor._monitorDesignId = gamedb.AddOrGetEncounterDesignInfo(designs, asteroidMonitor.PlayerId, "Monitor", "morrigirelics", new string[]
			{
				"sn_monitor.section"
			});
			asteroidMonitor._monitorCommandDesignId = gamedb.AddOrGetEncounterDesignInfo(designs, asteroidMonitor.PlayerId, "Monitor Command", "morrigirelics", new string[]
			{
				"sn_monitorcommander.section"
			});
			return asteroidMonitor;
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId, int OrbitId)
		{
			OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(OrbitId);
			gamedb.RemoveOrbitalObject(orbitalObjectInfo.ID);
			int orbitalId = gamedb.InsertAsteroidBelt(orbitalObjectInfo.ParentID, orbitalObjectInfo.StarSystemID, orbitalObjectInfo.OrbitalPath, "Asteroid Monitor Belt", App.GetSafeRandom().Next());
			gamedb.InsertAsteroidMonitorInfo(new AsteroidMonitorInfo
			{
				SystemId = SystemId,
				OrbitalId = orbitalId,
				IsAggressive = true
			});
			Matrix orbitalTransform = gamedb.GetOrbitalTransform(orbitalId);
			int fleetID = gamedb.InsertFleet(this.PlayerId, 0, SystemId, SystemId, "Asteroid Monitor", FleetType.FL_NORMAL);
			int shipId = gamedb.InsertShip(fleetID, this._monitorCommandDesignId, null, (ShipParams)0, null, 0);
			this.SetMonitorPosition(gamedb, shipId, 0, assetdb.GlobalAsteroidMonitorData.NumMonitors, true, orbitalTransform);
			for (int i = 0; i < assetdb.GlobalAsteroidMonitorData.NumMonitors; i++)
			{
				int shipId2 = gamedb.InsertShip(fleetID, this._monitorDesignId, null, (ShipParams)0, null, 0);
				this.SetMonitorPosition(gamedb, shipId2, i, assetdb.GlobalAsteroidMonitorData.NumMonitors, false, orbitalTransform);
			}
		}
		public void UpdateTurn(GameSession game, int id)
		{
			List<AsteroidMonitorInfo> source = game.GameDatabase.GetAllAsteroidMonitorInfos().ToList<AsteroidMonitorInfo>();
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo fi in list)
			{
				if (!source.Any((AsteroidMonitorInfo x) => x.SystemId == fi.SystemID))
				{
					game.GameDatabase.RemoveFleet(fi.ID);
				}
			}
			if (game.GameDatabase.GetAsteroidMonitorInfo(id) == null)
			{
				game.GameDatabase.RemoveEncounter(id);
			}
		}
		private void SetMonitorPosition(GameDatabase db, int shipId, int shipIndex, int numShips, bool isCommand, Matrix beltTransform)
		{
			Random random = new Random();
			float num = beltTransform.Position.Length;
			float num2 = 360f / (float)numShips;
			if (isCommand)
			{
				int num3 = random.NextInclusive(0, numShips);
				if (num3 == numShips)
				{
					num2 = -(num2 * 0.5f);
				}
				else
				{
					num2 = num2 * (float)num3 + num2 * 0.5f;
				}
				num -= 2500f;
			}
			else
			{
				num2 *= (float)(shipIndex - 1);
			}
			Matrix value = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(num2), 0f, 0f);
			value.Position = value.Forward * num;
			db.UpdateShipSystemPosition(shipId, new Matrix?(value));
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, List<ShipInfo> shipIDs, int systemID)
		{
			bool flag = false;
			Matrix result = Matrix.Identity;
			if (shipIDs.Count > 0)
			{
				int index = Math.Abs(App.GetSafeRandom().Next()) % shipIDs.Count;
				Matrix? shipSystemPosition = app.GameDatabase.GetShipSystemPosition(shipIDs[index].ID);
				if (shipSystemPosition.HasValue)
				{
					result = shipSystemPosition.Value;
					flag = true;
				}
			}
			if (!flag)
			{
				int encounterOrbitalId = app.GameDatabase.GetEncounterOrbitalId(EasterEgg.EE_ASTEROID_MONITOR, systemID);
				float length = app.GameDatabase.GetOrbitalTransform(encounterOrbitalId).Position.Length;
				float degrees = 0f;
				result = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(degrees), 0f, 0f);
				result.Position = -result.Forward * length;
			}
			return result;
		}
		public static Matrix GetSpawnTransform(App app, int designId, int shipIndex, int fleetCount, int systemID)
		{
			int encounterOrbitalId = app.GameDatabase.GetEncounterOrbitalId(EasterEgg.EE_ASTEROID_MONITOR, systemID);
			float num = app.GameDatabase.GetOrbitalTransform(encounterOrbitalId).Position.Length;
			float num2 = 36f;
			if (app.Game.ScriptModules.AsteroidMonitor != null && designId == app.Game.ScriptModules.AsteroidMonitor.MonitorCommandDesignId)
			{
				int num3 = Math.Abs(App.GetSafeRandom().Next()) % (fleetCount + 1);
				if (num3 == fleetCount)
				{
					num2 = -(num2 * 0.5f);
				}
				else
				{
					num2 = num2 * (float)num3 + num2 * 0.5f;
				}
				num -= 2500f;
			}
			else
			{
				num2 *= (float)(shipIndex - 1);
			}
			Matrix result = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(num2), 0f, 0f);
			result.Position = result.Forward * num;
			return result;
		}
	}
}
