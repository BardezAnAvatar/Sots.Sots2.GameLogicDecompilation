using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class Comet
	{
		private const string FactionName = "grandmenaces";
		private const string PlayerName = "Comet";
		private const string PlayerAvatar = "\\base\\factions\\grandmenaces\\avatars\\Comet_Avatar.tga";
		private const string FleetName = "Comet";
		private const string CometDesignFile = "Comet.section";
		private int PlayerId = -1;
		public static bool ForceEncounter;
		private int _cometDesignId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int CometDesignId
		{
			get
			{
				return this._cometDesignId;
			}
		}
		public static Comet InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			if (assetdb.GetFaction("grandmenaces") == null)
			{
				return null;
			}
			Comet comet = new Comet();
			comet.PlayerId = gamedb.InsertPlayer("Comet", "grandmenaces", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\grandmenaces\\avatars\\Comet_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(comet.PlayerId, "Comet", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "grandmenaces", "Comet.section")
			});
			comet._cometDesignId = gamedb.InsertDesignByDesignInfo(design);
			return comet;
		}
		public static Comet ResumeEncounter(GameDatabase gamedb)
		{
			if (gamedb.AssetDatabase.GetFaction("grandmenaces") == null)
			{
				return null;
			}
			Comet comet = new Comet();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Comet"));
			if (playerInfo != null)
			{
				comet.PlayerId = playerInfo.ID;
			}
			else
			{
				comet.PlayerId = gamedb.InsertPlayer("Comet", "grandmenaces", null, new Vector3(0f), new Vector3(0f), "", "\\base\\factions\\grandmenaces\\avatars\\Comet_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			List<DesignInfo> source2 = gamedb.GetDesignInfosForPlayer(comet.PlayerId).ToList<DesignInfo>();
			DesignInfo designInfo = source2.FirstOrDefault((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("Comet.section"));
			if (designInfo == null)
			{
				DesignInfo design = new DesignInfo(comet.PlayerId, "Comet", new string[]
				{
					string.Format("factions\\{0}\\sections\\{1}", "grandmenaces", "Comet.section")
				});
				comet._cometDesignId = gamedb.InsertDesignByDesignInfo(design);
			}
			else
			{
				comet._cometDesignId = designInfo.ID;
			}
			return comet;
		}
		public void UpdateTurn(GameSession game)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				game.GameDatabase.RemoveFleet(current.ID);
			}
		}
		public void ExecuteInstance(GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			int fleetID = gamedb.InsertFleet(this.PlayerId, 0, systemid, systemid, "Comet", FleetType.FL_NORMAL);
			gamedb.InsertShip(fleetID, this.CometDesignId, null, (ShipParams)0, null, 0);
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			if (!targetSystem.HasValue)
			{
				List<ColonyInfo> choices = gamedb.GetColonyInfos().ToList<ColonyInfo>();
				ColonyInfo colonyInfo = safeRandom.Choose(choices);
				targetSystem = new int?(gamedb.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID);
			}
			gamedb.InsertIncomingGM(targetSystem.Value, EasterEgg.GM_COMET, gamedb.GetTurnCount() + 1);
			List<PlayerInfo> list = gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, current.ID) > 0)
				{
					gamedb.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_COMET,
						EventMessage = TurnEventMessage.EM_INCOMING_COMET,
						PlayerID = current.ID,
						TurnNumber = gamedb.GetTurnCount()
					});
				}
			}
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, Kerberos.Sots.GameStates.StarSystem starSystem, OrbitalObjectInfo[] orbitalObjects)
		{
			return Comet.GetSpawnTransform(app, starSystem, orbitalObjects);
		}
		public static Matrix GetSpawnTransform(App app, Kerberos.Sots.GameStates.StarSystem starSystem, OrbitalObjectInfo[] orbitalObjects)
		{
			int planetID = 0;
			float num = 0f;
			float s = 5000f;
			Vector3 v = Vector3.Zero;
			for (int i = 0; i < orbitalObjects.Length; i++)
			{
				OrbitalObjectInfo orbitalObjectInfo = orbitalObjects[i];
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
				if (planetInfo != null)
				{
					ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID);
					if (colonyInfoForPlanet != null)
					{
						Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
						float lengthSquared = orbitalTransform.Position.LengthSquared;
						if (lengthSquared > num)
						{
							num = lengthSquared;
							v = orbitalTransform.Position;
							s = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 15000f;
							planetID = planetInfo.ID;
						}
					}
				}
			}
			if (num <= 0f)
			{
				return Matrix.Identity;
			}
			Vector3 vector = -v;
			vector.Y = 0f;
			vector.Normalize();
			Vector3 vector2 = v - vector * s;
			if (starSystem != null)
			{
				Vector3 vector3 = -Vector3.UnitZ;
				float num2 = -1f;
				List<Ship> stationsAroundPlanet = starSystem.GetStationsAroundPlanet(planetID);
				foreach (Ship current in stationsAroundPlanet)
				{
					Vector3 vector4 = v - current.Maneuvering.Position;
					vector4.Normalize();
					float num3 = Vector3.Dot(vector4, vector);
					if (num3 > 0.75f && num3 > num2)
					{
						num2 = num3;
						vector3 = vector4;
					}
				}
				if (num2 > 0f)
				{
					Matrix matrix = Matrix.CreateWorld(Vector3.Zero, vector3, Vector3.UnitY);
					Vector3 v2 = v + matrix.Right * s;
					Vector3 v3 = v - matrix.Right * s;
					if ((vector2 - v2).LengthSquared < (vector2 - v3).LengthSquared)
					{
						vector = (vector3 + matrix.Right) * 0.5f;
					}
					else
					{
						vector = (vector3 - matrix.Right) * 0.5f;
					}
					vector.Normalize();
					vector2 = v - vector * s;
				}
			}
			return Matrix.CreateWorld(vector2, vector, Vector3.UnitY);
		}
	}
}
