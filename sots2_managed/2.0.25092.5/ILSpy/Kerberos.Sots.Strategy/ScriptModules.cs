using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class ScriptModules
	{
		public VonNeumann VonNeumann
		{
			get;
			private set;
		}
		public Swarmers Swarmers
		{
			get;
			private set;
		}
		public Gardeners Gardeners
		{
			get;
			private set;
		}
		public AsteroidMonitor AsteroidMonitor
		{
			get;
			private set;
		}
		public MorrigiRelic MorrigiRelic
		{
			get;
			private set;
		}
		public Slaver Slaver
		{
			get;
			private set;
		}
		public Pirates Pirates
		{
			get;
			private set;
		}
		public Spectre Spectre
		{
			get;
			private set;
		}
		public GhostShip GhostShip
		{
			get;
			private set;
		}
		public MeteorShower MeteorShower
		{
			get;
			private set;
		}
		public SystemKiller SystemKiller
		{
			get;
			private set;
		}
		public Locust Locust
		{
			get;
			private set;
		}
		public Comet Comet
		{
			get;
			private set;
		}
		public NeutronStar NeutronStar
		{
			get;
			private set;
		}
		public SuperNova SuperNova
		{
			get;
			private set;
		}
		public static ScriptModules New(Random random, GameDatabase db, AssetDatabase assetdb, GameSession game, NamesPool namesPool, GameSetup gameSetup)
		{
			ScriptModules scriptModules = new ScriptModules();
			scriptModules.VonNeumann = VonNeumann.InitializeEncounter(db, assetdb);
			scriptModules.Swarmers = Swarmers.InitializeEncounter(db, assetdb);
			scriptModules.Gardeners = Gardeners.InitializeEncounter(db, assetdb);
			scriptModules.AsteroidMonitor = AsteroidMonitor.InitializeEncounter(db, assetdb);
			scriptModules.MorrigiRelic = MorrigiRelic.InitializeEncounter(db, assetdb);
			scriptModules.Slaver = Slaver.InitializeEncounter(db, assetdb);
			scriptModules.Pirates = Pirates.InitializeEncounter(db, assetdb);
			scriptModules.Spectre = Spectre.InitializeEncounter(db, assetdb);
			scriptModules.GhostShip = GhostShip.InitializeEncounter(db, assetdb);
			scriptModules.MeteorShower = MeteorShower.InitializeEncounter(db, assetdb);
			scriptModules.SystemKiller = SystemKiller.InitializeEncounter(db, assetdb);
			scriptModules.Locust = Locust.InitializeEncounter(db, assetdb);
			scriptModules.Comet = Comet.InitializeEncounter(db, assetdb);
			if (db.HasEndOfFleshExpansion())
			{
				scriptModules.NeutronStar = NeutronStar.InitializeEncounter(db, assetdb);
				scriptModules.SuperNova = SuperNova.InitializeEncounter();
			}
			scriptModules.AddEasterEggs(random, db, assetdb, game, namesPool, gameSetup);
			return scriptModules;
		}
		public static ScriptModules Resume(GameDatabase db)
		{
			ScriptModules scriptModules = new ScriptModules();
			scriptModules.VonNeumann = VonNeumann.ResumeEncounter(db);
			scriptModules.Swarmers = Swarmers.ResumeEncounter(db);
			scriptModules.Gardeners = Gardeners.ResumeEncounter(db);
			scriptModules.AsteroidMonitor = AsteroidMonitor.ResumeEncounter(db);
			scriptModules.MorrigiRelic = MorrigiRelic.ResumeEncounter(db);
			scriptModules.Slaver = Slaver.ResumeEncounter(db);
			scriptModules.Pirates = Pirates.ResumeEncounter(db);
			scriptModules.Spectre = Spectre.ResumeEncounter(db);
			scriptModules.GhostShip = GhostShip.ResumeEncounter(db);
			scriptModules.MeteorShower = MeteorShower.ResumeEncounter(db);
			scriptModules.SystemKiller = SystemKiller.ResumeEncounter(db);
			scriptModules.Locust = Locust.ResumeEncounter(db);
			scriptModules.Comet = Comet.ResumeEncounter(db);
			if (db.HasEndOfFleshExpansion())
			{
				scriptModules.NeutronStar = NeutronStar.ResumeEncounter(db);
				scriptModules.SuperNova = SuperNova.ResumeEncounter();
			}
			List<PlayerInfo> list = (
				from x in db.GetPlayerInfos()
				where !x.isStandardPlayer && !x.includeInDiplomacy
				select x).ToList<PlayerInfo>();
			List<int> list2 = db.GetStandardPlayerIDs().ToList<int>();
			foreach (int current in list2)
			{
				foreach (PlayerInfo current2 in list)
				{
					DiplomacyInfo diplomacyInfo = db.GetDiplomacyInfo(current, current2.ID);
					if (diplomacyInfo.State != DiplomacyState.WAR)
					{
						db.UpdateDiplomacyState(current, current2.ID, DiplomacyState.WAR, diplomacyInfo.Relations, true);
					}
				}
			}
			return scriptModules;
		}
		public void UpdateEasterEggs(GameSession game)
		{
			List<EncounterInfo> list = game.GameDatabase.GetEncounterInfos().ToList<EncounterInfo>();
			foreach (EncounterInfo current in list)
			{
				switch (current.Type)
				{
				case EasterEgg.EE_SWARM:
					if (this.Swarmers != null)
					{
						this.Swarmers.UpdateTurn(game, current.Id);
					}
					break;
				case EasterEgg.EE_ASTEROID_MONITOR:
					if (this.AsteroidMonitor != null)
					{
						this.AsteroidMonitor.UpdateTurn(game, current.Id);
					}
					break;
				case EasterEgg.EE_VON_NEUMANN:
					if (this.VonNeumann != null)
					{
						this.VonNeumann.UpdateTurn(game, current.Id);
					}
					break;
				case EasterEgg.EE_GARDENERS:
				case EasterEgg.GM_GARDENER:
					if (this.Gardeners != null)
					{
						this.Gardeners.UpdateTurn(game, current.Id);
					}
					break;
				case EasterEgg.EE_MORRIGI_RELIC:
					if (this.MorrigiRelic != null)
					{
						this.MorrigiRelic.UpdateTurn(game, current.Id);
					}
					break;
				case EasterEgg.GM_SYSTEM_KILLER:
					if (this.SystemKiller != null)
					{
						this.SystemKiller.UpdateTurn(game, current.Id);
					}
					break;
				case EasterEgg.GM_LOCUST_SWARM:
					if (this.Locust != null)
					{
						this.Locust.UpdateTurn(game, current.Id);
					}
					break;
				case EasterEgg.GM_NEUTRON_STAR:
					if (this.NeutronStar != null)
					{
						this.NeutronStar.UpdateTurn(game, current.Id);
					}
					break;
				case EasterEgg.GM_SUPER_NOVA:
					if (this.SuperNova != null)
					{
						this.SuperNova.UpdateTurn(game, current.Id);
					}
					break;
				}
			}
		}
		private void AddEasterEggs(Random random, GameDatabase gamedb, AssetDatabase assetdb, GameSession game, NamesPool namesPool, GameSetup gameSetup)
		{
			List<StarSystemInfo> list = gamedb.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<StarSystemInfo> list2 = new List<StarSystemInfo>(list);
			foreach (StarSystemInfo current in list2)
			{
				List<OrbitalObjectInfo> list3 = gamedb.GetStarSystemOrbitalObjectInfos(current.ID).ToList<OrbitalObjectInfo>();
				if (list3.Count<OrbitalObjectInfo>() == 0)
				{
					list.Remove(current);
				}
				bool flag = false;
				foreach (OrbitalObjectInfo current2 in list3)
				{
					if (gamedb.GetColonyInfoForPlanet(current2.ID) != null)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list.Remove(current);
				}
			}
			foreach (StarSystemInfo current3 in list)
			{
				List<OrbitalObjectInfo> list4 = gamedb.GetStarSystemOrbitalObjectInfos(current3.ID).ToList<OrbitalObjectInfo>();
				using (List<OrbitalObjectInfo>.Enumerator enumerator4 = list4.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						OrbitalObjectInfo current4 = enumerator4.Current;
						PlanetInfo planetInfo = gamedb.GetPlanetInfo(current4.ID);
						if (planetInfo != null && !(planetInfo.Type == "gaseous") && gamedb.GetLargeAsteroidInfo(current4.ID) == null && gamedb.GetAsteroidBeltInfo(current4.ID) == null && random.CoinToss((double)(assetdb.RandomEncOddsPerOrbital * ((float)gameSetup._randomEncounterFrequency / 100f))))
						{
							Dictionary<EasterEgg, int> availableEEOdds = game.GetAvailableEEOdds();
							int num = availableEEOdds.Sum((KeyValuePair<EasterEgg, int> x) => x.Value);
							if (num == 0)
							{
								return;
							}
							int num2 = random.Next(num);
							int num3 = 0;
							EasterEgg easterEgg = EasterEgg.EE_SWARM;
							foreach (KeyValuePair<EasterEgg, int> current5 in assetdb.EasterEggOdds)
							{
								num3 += current5.Value;
								if (num3 > num2)
								{
									easterEgg = current5.Key;
									break;
								}
							}
							App.Log.Warn(string.Format("Spawning {0} at {1}", easterEgg.ToString(), current3.ID), "game");
							switch (easterEgg)
							{
							case EasterEgg.EE_SWARM:
								if (this.Swarmers != null)
								{
									this.Swarmers.AddInstance(gamedb, assetdb, current3.ID, current4.ID);
									goto IL_353;
								}
								goto IL_353;
							case EasterEgg.EE_ASTEROID_MONITOR:
								if (this.AsteroidMonitor != null)
								{
									this.AsteroidMonitor.AddInstance(gamedb, assetdb, current3.ID, current4.ID);
									goto IL_353;
								}
								goto IL_353;
							case EasterEgg.EE_PIRATE_BASE:
								if (this.Pirates != null)
								{
									this.Pirates.AddInstance(gamedb, assetdb, game, current3.ID, current4.ID);
									goto IL_353;
								}
								goto IL_353;
							case EasterEgg.EE_VON_NEUMANN:
								if (this.VonNeumann != null)
								{
									this.VonNeumann.AddInstance(gamedb, assetdb, namesPool);
									goto IL_353;
								}
								goto IL_353;
							case EasterEgg.EE_GARDENERS:
								if (this.Gardeners != null)
								{
									this.Gardeners.AddInstance(gamedb, assetdb, current3.ID);
									goto IL_353;
								}
								goto IL_353;
							case EasterEgg.EE_INDEPENDENT:
								ScriptModules.InsertIndependentSystem(random, current3, current4, gamedb, assetdb);
								goto IL_353;
							case EasterEgg.EE_MORRIGI_RELIC:
								if (this.MorrigiRelic != null)
								{
									this.MorrigiRelic.AddInstance(gamedb, assetdb, current3.ID, current4.ID);
									goto IL_353;
								}
								goto IL_353;
							default:
								goto IL_353;
							}
						}
					}
					IL_353:;
				}
			}
		}
		private static void GenerateIndependentRace(Random random, StarSystemInfo system, OrbitalObjectInfo orbit, GameDatabase gamedb, AssetDatabase assetdb)
		{
			List<Faction> list = (
				from x in assetdb.Factions
				where x.IsIndependent()
				select x).ToList<Faction>();
			List<PlayerInfo> players = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			players.RemoveAll((PlayerInfo x) => x.isStandardPlayer || !x.includeInDiplomacy);
			list.RemoveAll((Faction x) => x.IndyDescrition == null || players.Any((PlayerInfo y) => y.Name == x.Name));
			if (list.Count == 0)
			{
				return;
			}
			Faction faction = random.Choose(list);
			IndyDesc indyDescrition = faction.IndyDescrition;
			double num = 1.0;
			if (indyDescrition.TechLevel == 1)
			{
				num = (double)((float)random.NextInclusive(30, 200) * indyDescrition.BasePopulationMod * 1000f);
			}
			else
			{
				if (indyDescrition.TechLevel == 2)
				{
					num = (double)((float)random.NextInclusive(1, 750) * indyDescrition.BasePopulationMod * 1000000f);
				}
				else
				{
					if (indyDescrition.TechLevel == 3)
					{
						num = (double)((float)random.NextInclusive(750, 2000) * indyDescrition.BasePopulationMod * 1000000f);
					}
					else
					{
						num = (double)((float)random.NextInclusive(1750, 10000) * indyDescrition.BasePopulationMod * 1000000f);
					}
				}
			}
			FactionInfo factionInfo = gamedb.GetFactionInfo(faction.ID);
			factionInfo.IdealSuitability = gamedb.GetFactionSuitability(indyDescrition.BaseFactionSuitability) + random.NextInclusive(-indyDescrition.Suitability, indyDescrition.Suitability);
			gamedb.UpdateFaction(factionInfo);
			gamedb.RemoveOrbitalObject(orbit.ID);
			PlanetOrbit planetOrbit = new PlanetOrbit();
			if (indyDescrition.MinPlanetSize != 0 && indyDescrition.MaxPlanetSize != 0)
			{
				planetOrbit.Size = new int?(random.NextInclusive(indyDescrition.MinPlanetSize, indyDescrition.MaxPlanetSize));
			}
			PlanetInfo planetInfo = StarSystemHelper.InferPlanetInfo(planetOrbit);
			planetInfo.Suitability = factionInfo.IdealSuitability;
			planetInfo.Biosphere = (int)((float)planetInfo.Biosphere * indyDescrition.BiosphereMod);
			planetInfo.ID = gamedb.InsertPlanet(orbit.ParentID, orbit.StarSystemID, orbit.OrbitalPath, orbit.Name, indyDescrition.StellarBodyType, null, planetInfo.Suitability, planetInfo.Biosphere, planetInfo.Resources, planetInfo.Size);
			num += (double)(1000 * planetInfo.Biosphere);
			num = Math.Min(num, Colony.GetMaxCivilianPop(gamedb, planetInfo));
			string avatarPath = "";
			if (faction.AvatarTexturePaths.Count<string>() > 0)
			{
				avatarPath = faction.AvatarTexturePaths[0];
			}
			int orInsertIndyPlayerId = gamedb.GetOrInsertIndyPlayerId(gamedb, faction.ID, faction.Name, avatarPath);
			players = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo pi in players)
			{
				Faction faction2 = assetdb.Factions.FirstOrDefault((Faction x) => x.ID == pi.FactionID);
				gamedb.InsertDiplomaticState(orInsertIndyPlayerId, pi.ID, (!pi.isStandardPlayer && !pi.includeInDiplomacy) ? DiplomacyState.WAR : DiplomacyState.NEUTRAL, faction.GetDefaultReactionToFaction(faction2), false, true);
			}
			gamedb.InsertColony(planetInfo.ID, orInsertIndyPlayerId, num / 2.0, 0.5f, 0, 1f, true);
			gamedb.InsertColonyFaction(planetInfo.ID, faction.ID, num / 2.0, 1f, 0);
			if (indyDescrition.TechLevel >= 4)
			{
				PlanetInfo[] starSystemPlanetInfos = gamedb.GetStarSystemPlanetInfos(system.ID);
				for (int i = 0; i < starSystemPlanetInfos.Length; i++)
				{
					PlanetInfo planetInfo2 = starSystemPlanetInfos[i];
					if (planetInfo2.ID != planetInfo.ID)
					{
						PlanetInfo planetInfo3 = gamedb.GetPlanetInfo(planetInfo2.ID);
						float num2 = Math.Abs(factionInfo.IdealSuitability - planetInfo3.Suitability);
						if (num2 < 200f)
						{
							double num3 = (double)((float)(random.NextInclusive(100, 200) * 100) * indyDescrition.BasePopulationMod);
							gamedb.InsertColony(planetInfo.ID, orInsertIndyPlayerId, num3, 0.5f, 0, 1f, true);
							gamedb.InsertColonyFaction(planetInfo.ID, faction.ID, num3 / 2.0, 1f, 0);
						}
						else
						{
							if (num2 < 600f && planetInfo3.Suitability != 0f)
							{
								float num4 = 100f + (float)random.Next(150);
								if (random.Next(2) == 0)
								{
									num4 *= -1f;
								}
								planetInfo3.Suitability = factionInfo.IdealSuitability + num4;
								gamedb.UpdatePlanet(planetInfo3);
								double num5 = (double)((float)(random.NextInclusive(50, 100) * 100) * indyDescrition.BasePopulationMod);
								gamedb.InsertColony(planetInfo.ID, orInsertIndyPlayerId, num5 / 2.0, 0.5f, 0, 1f, true);
								gamedb.InsertColonyFaction(planetInfo.ID, faction.ID, num5 / 2.0, 1f, 0);
							}
						}
					}
				}
			}
		}
		private static void InsertIndependentSystem(Random random, StarSystemInfo system, OrbitalObjectInfo orbit, GameDatabase gamedb, AssetDatabase assetdb)
		{
			ScriptModules.GenerateIndependentRace(random, system, orbit, gamedb, assetdb);
		}
		public bool IsEncounterPlayer(int playerID)
		{
			return this.VonNeumann.PlayerID == playerID || this.Swarmers.PlayerID == playerID || this.Gardeners.PlayerID == playerID || this.AsteroidMonitor.PlayerID == playerID || this.MorrigiRelic.PlayerID == playerID || this.Slaver.PlayerID == playerID || this.Pirates.PlayerID == playerID || this.Spectre.PlayerID == playerID || this.GhostShip.PlayerID == playerID || this.MeteorShower.PlayerID == playerID || this.SystemKiller.PlayerID == playerID || this.Locust.PlayerID == playerID || this.Comet.PlayerID == playerID;
		}
		public EasterEgg GetEasterEggTypeForPlayer(int playerID)
		{
			if (this.VonNeumann.PlayerID == playerID)
			{
				return EasterEgg.EE_VON_NEUMANN;
			}
			if (this.Swarmers.PlayerID == playerID)
			{
				return EasterEgg.EE_SWARM;
			}
			if (this.Gardeners.PlayerID == playerID)
			{
				return EasterEgg.EE_GARDENERS;
			}
			if (this.AsteroidMonitor.PlayerID == playerID)
			{
				return EasterEgg.EE_ASTEROID_MONITOR;
			}
			if (this.MorrigiRelic.PlayerID == playerID)
			{
				return EasterEgg.EE_MORRIGI_RELIC;
			}
			if (this.SystemKiller.PlayerID == playerID)
			{
				return EasterEgg.GM_SYSTEM_KILLER;
			}
			if (this.Locust.PlayerID == playerID)
			{
				return EasterEgg.GM_LOCUST_SWARM;
			}
			if (this.Comet.PlayerID == playerID)
			{
				return EasterEgg.GM_COMET;
			}
			if (this.Pirates.PlayerID == playerID)
			{
				return EasterEgg.EE_PIRATE_BASE;
			}
			return EasterEgg.UNKNOWN;
		}
	}
}
