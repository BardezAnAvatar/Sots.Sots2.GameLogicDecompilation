using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class Spectre
	{
		private const string FactionName = "specters";
		private const string PlayerName = "Spectre";
		private const string PlayerAvatar = "\\base\\factions\\specters\\avatars\\Specters_Avatar.tga";
		private const string FleetName = "Haunt of Spectres";
		private const string smallDesignFile = "small_specter.section";
		private const string mediumDesignFile = "medium_specter.section";
		private const string bigDesignFile = "big_specter.section";
		private int PlayerId = -1;
		public static bool ForceEncounter;
		private int _smallDesignId;
		private int _mediumDesignId;
		private int _bigDesignId;
		private bool _force;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int SmallDesignId
		{
			get
			{
				return this._smallDesignId;
			}
		}
		public int MediumDesignId
		{
			get
			{
				return this._mediumDesignId;
			}
		}
		public int BigDesignId
		{
			get
			{
				return this._bigDesignId;
			}
		}
		public static Spectre InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Spectre spectre = new Spectre();
			spectre.PlayerId = gamedb.InsertPlayer("Spectre", "specters", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\specters\\avatars\\Specters_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(spectre.PlayerId, "Small Spectre", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "specters", "small_specter.section")
			});
			DesignInfo design2 = new DesignInfo(spectre.PlayerId, "Medium Spectre", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "specters", "medium_specter.section")
			});
			DesignInfo design3 = new DesignInfo(spectre.PlayerId, "Big Spectre", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "specters", "big_specter.section")
			});
			spectre._smallDesignId = gamedb.InsertDesignByDesignInfo(design);
			spectre._mediumDesignId = gamedb.InsertDesignByDesignInfo(design2);
			spectre._bigDesignId = gamedb.InsertDesignByDesignInfo(design3);
			return spectre;
		}
		public static Spectre ResumeEncounter(GameDatabase gamedb)
		{
			Spectre spectre = new Spectre();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Spectre"));
			spectre.PlayerId = playerInfo.ID;
			List<DesignInfo> source2 = gamedb.GetDesignInfosForPlayer(spectre.PlayerId).ToList<DesignInfo>();
			spectre._smallDesignId = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("small_specter.section")).ID;
			spectre._mediumDesignId = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("medium_specter.section")).ID;
			spectre._bigDesignId = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("big_specter.section")).ID;
			return spectre;
		}
		public void UpdateTurn(GameSession game)
		{
			if (this._force)
			{
				this._force = false;
				return;
			}
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				game.GameDatabase.RemoveFleet(current.ID);
			}
		}
		public void ExecuteEncounter(GameSession game, PlayerInfo targetPlayer, int targetSystem, bool force = false)
		{
			this._force = force;
			Random safeRandom = App.GetSafeRandom();
			int num = safeRandom.Next(game.AssetDatabase.GlobalSpectreData.MinSpectres, game.AssetDatabase.GlobalSpectreData.MaxSpectres);
			int fleetID = game.GameDatabase.InsertFleet(this.PlayerId, 0, targetSystem, targetSystem, "Haunt of Spectres", FleetType.FL_NORMAL);
			for (int i = 0; i < num; i++)
			{
				int num2 = safeRandom.Next(100);
				if (num2 > 80)
				{
					game.GameDatabase.InsertShip(fleetID, this._bigDesignId, null, (ShipParams)0, null, 0);
				}
				else
				{
					if (num2 > 50)
					{
						game.GameDatabase.InsertShip(fleetID, this._mediumDesignId, null, (ShipParams)0, null, 0);
					}
					else
					{
						game.GameDatabase.InsertShip(fleetID, this._smallDesignId, null, (ShipParams)0, null, 0);
					}
				}
			}
		}
		public void AddEncounter(GameSession game, PlayerInfo targetPlayer, int? forceSystem = null)
		{
			if (game.GameDatabase.GetStratModifier<bool>(StratModifiers.ImmuneToSpectre, targetPlayer.ID))
			{
				return;
			}
			Random safeRandom = App.GetSafeRandom();
			List<int> list = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayer.ID).ToList<int>();
			if (list.Count > 0 || forceSystem.HasValue)
			{
				int num = forceSystem.HasValue ? forceSystem.Value : safeRandom.Choose(list);
				game.GameDatabase.InsertIncomingRandom(targetPlayer.ID, num, RandomEncounter.RE_SPECTORS, game.GameDatabase.GetTurnCount() + 1);
				if (game.DetectedIncomingRandom(targetPlayer.ID, num))
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_SPECTORS,
						EventMessage = TurnEventMessage.EM_INCOMING_SPECTORS,
						PlayerID = targetPlayer.ID,
						SystemID = num,
						TurnNumber = game.GameDatabase.GetTurnCount()
					});
				}
			}
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, int systemID)
		{
			return Spectre.GetSpawnTransform(app, systemID);
		}
		public static Matrix GetSpawnTransform(App app, int systemId)
		{
			bool flag = false;
			float num = 0f;
			float s = 0f;
			OrbitalObjectInfo orbitalObjectInfo = null;
			Vector3 v = Vector3.Zero;
			foreach (OrbitalObjectInfo current in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
			{
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(current.ID);
				if (!flag || colonyInfoForPlanet != null)
				{
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(current.ID);
					float num2 = 1000f;
					if (planetInfo != null)
					{
						num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					}
					Vector3 position = app.GameDatabase.GetOrbitalTransform(current.ID).Position;
					float num3 = position.Length + num2;
					if (num3 > num || (!flag && colonyInfoForPlanet != null))
					{
						orbitalObjectInfo = current;
						num = num3;
						flag = (colonyInfoForPlanet != null);
						v = position;
						s = num2 + 10000f;
						if (flag)
						{
							break;
						}
					}
				}
			}
			if (orbitalObjectInfo == null)
			{
				return Matrix.Identity;
			}
			Vector3 vector = -v;
			vector.Normalize();
			return Matrix.CreateWorld(v - vector * s, vector, Vector3.UnitY);
		}
	}
}
