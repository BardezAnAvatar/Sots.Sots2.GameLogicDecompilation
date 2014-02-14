using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class MorrigiRelic
	{
		private const string FactionName = "morrigirelics";
		private const string PlayerName = "Morrigi Relic";
		private const string PlayerAvatar = "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga";
		private const string FleetName = "Morrigi Relic";
		private static string[] designFiles = new string[]
		{
			"sn_tholostombstationpristinelevelone.section",
			"sn_tholostombstationpristineleveltwo.section",
			"sn_tholostombstationpristinelevelthree.section",
			"sn_tholostombstationpristinelevelfour.section",
			"sn_tholostombstationpristinelevelfive.section",
			"sn_tholostombstationstealthlevelone.section",
			"sn_tholostombstationstealthleveltwo.section",
			"sn_tholostombstationstealthlevelthree.section",
			"sn_tholostombstationstealthlevelfour.section",
			"sn_tholostombstationstealthlevelfive.section",
			"br_drone.section"
		};
		private int PlayerId = -1;
		private static List<int> _designIds = new List<int>();
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public static List<int> DesignIds
		{
			get
			{
				return MorrigiRelic._designIds;
			}
		}
		public static MorrigiRelic InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			MorrigiRelic morrigiRelic = new MorrigiRelic();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Morrigi Relic"));
			if (playerInfo == null)
			{
				morrigiRelic.PlayerId = gamedb.InsertPlayer("Morrigi Relic", "morrigirelics", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			else
			{
				morrigiRelic.PlayerId = playerInfo.ID;
			}
			MorrigiRelic._designIds.Clear();
			string[] array = MorrigiRelic.designFiles;
			for (int i = 0; i < array.Length; i++)
			{
				string arg = array[i];
				DesignInfo design = new DesignInfo(morrigiRelic.PlayerId, "Tholos Tomb", new string[]
				{
					string.Format("factions\\{0}\\sections\\{1}", "morrigirelics", arg)
				});
				MorrigiRelic._designIds.Add(gamedb.InsertDesignByDesignInfo(design));
			}
			return morrigiRelic;
		}
		public static MorrigiRelic ResumeEncounter(GameDatabase gamedb)
		{
			MorrigiRelic morrigiRelic = new MorrigiRelic();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Morrigi Relic"));
			if (playerInfo == null)
			{
				morrigiRelic.PlayerId = gamedb.InsertPlayer("Morrigi Relic", "morrigirelics", null, new Vector3(0f), new Vector3(0f), "", "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			else
			{
				morrigiRelic.PlayerId = playerInfo.ID;
			}
			MorrigiRelic._designIds.Clear();
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(morrigiRelic.PlayerId).ToList<DesignInfo>();
			string[] array = MorrigiRelic.designFiles;
			string s;
			for (int i = 0; i < array.Length; i++)
			{
				s = array[i];
				if (list.Any((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith(s)))
				{
					DesignInfo designInfo = list.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith(s));
					MorrigiRelic._designIds.Add(designInfo.ID);
					list.Remove(designInfo);
				}
				else
				{
					DesignInfo design = new DesignInfo(morrigiRelic.PlayerId, "Tholos Tomb", new string[]
					{
						string.Format("factions\\{0}\\sections\\{1}", "morrigirelics", s)
					});
					MorrigiRelic._designIds.Add(gamedb.InsertDesignByDesignInfo(design));
				}
			}
			return morrigiRelic;
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId, int OrbitId)
		{
			OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(OrbitId);
			gamedb.RemoveOrbitalObject(orbitalObjectInfo.ID);
			int orbitalId = gamedb.InsertAsteroidBelt(orbitalObjectInfo.ParentID, orbitalObjectInfo.StarSystemID, orbitalObjectInfo.OrbitalPath, "Morrigi Relic Belt", App.GetSafeRandom().Next());
			int num = gamedb.InsertFleet(this.PlayerId, 0, SystemId, SystemId, "Morrigi Relic", FleetType.FL_NORMAL);
			gamedb.InsertMorrigiRelicInfo(new MorrigiRelicInfo
			{
				SystemId = SystemId,
				FleetId = num,
				IsAggressive = true
			});
			Random safeRandom = App.GetSafeRandom();
			Matrix orbitalTransform = gamedb.GetOrbitalTransform(orbitalId);
			List<int> choices = (
				from x in MorrigiRelic._designIds
				where x != MorrigiRelic._designIds.Last<int>()
				select x).ToList<int>();
			for (int i = 0; i < safeRandom.Next(1, assetdb.GlobalMorrigiRelicData.NumTombs + 1); i++)
			{
				int num2 = gamedb.InsertShip(num, safeRandom.Choose(choices), null, (ShipParams)0, null, 0);
				this.SetRelicPosition(safeRandom, gamedb, assetdb, num2, i, orbitalTransform);
				for (int j = 0; j < assetdb.GlobalMorrigiRelicData.NumFighters; j++)
				{
					int shipID = gamedb.InsertShip(num, MorrigiRelic._designIds.Last<int>(), null, (ShipParams)0, null, 0);
					gamedb.SetShipParent(shipID, num2);
				}
			}
		}
		public void UpdateTurn(GameSession game, int id)
		{
			if (game.GameDatabase.GetMorrigiRelicInfo(id) == null)
			{
				game.GameDatabase.RemoveEncounter(id);
			}
		}
		public void ApplyRewardsToPlayers(App game, MorrigiRelicInfo relicInfo, List<ShipInfo> aliveRelicShips, List<Player> rewardedPlayers)
		{
			if (relicInfo == null)
			{
				return;
			}
			relicInfo.IsAggressive = false;
			int num = 0;
			foreach (ShipInfo current in aliveRelicShips)
			{
				if (current.DesignID != MorrigiRelic._designIds.Last<int>() && current.DesignInfo != null)
				{
					string sectionName = (
						from x in current.DesignInfo.DesignSections
						select x.ShipSectionAsset).FirstOrDefault((ShipSectionAsset x) => x.Type == ShipSectionType.Mission).SectionName;
					int num2 = game.AssetDatabase.GlobalMorrigiRelicData.Rewards[(int)MorrigiRelicControl.GetMorrigiRelicTypeFromName(sectionName)];
					num += num2;
				}
			}
			game.GameDatabase.RemoveFleet(relicInfo.FleetId);
			game.GameDatabase.RemoveEncounter(relicInfo.Id);
			if (rewardedPlayers.Count == 0)
			{
				return;
			}
			int num3 = num / rewardedPlayers.Count;
			if (num3 > 0)
			{
				foreach (Player current2 in rewardedPlayers)
				{
					PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(current2.ID);
					if (playerInfo != null)
					{
						game.GameDatabase.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings + (double)num3);
						game.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_TOMB_DEFENDERS_DESTROYED,
							EventMessage = TurnEventMessage.EM_TOMB_DEFENDERS_DESTROYED,
							PlayerID = playerInfo.ID,
							SystemID = relicInfo.SystemId,
							Savings = (double)num3,
							TurnNumber = game.GameDatabase.GetTurnCount()
						});
					}
				}
			}
		}
		private void SetRelicPosition(Random r, GameDatabase db, AssetDatabase ad, int shipId, int shipIndex, Matrix beltTransform)
		{
			float length = beltTransform.Position.Length;
			float val = (float)Math.Tan((double)((ad.DefaultTacSensorRange + 5000f) / length));
			float num = Math.Min(6.28318548f / (float)ad.GlobalMorrigiRelicData.NumTombs, val);
			float num2 = num / 4f;
			float yawRadians = num * (float)shipIndex + r.NextInclusive(-num2, num2);
			float num3 = r.NextInclusive(-2500f, 2500f);
			Matrix value = Matrix.CreateRotationYPR(yawRadians, 0f, 0f);
			value.Position = value.Forward * (length + num3);
			db.UpdateShipSystemPosition(shipId, new Matrix?(value));
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, List<ShipInfo> shipIDs, OrbitalObjectInfo[] objects)
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
				int orbitalId = 0;
				for (int i = 0; i < objects.Length; i++)
				{
					OrbitalObjectInfo orbitalObjectInfo = objects[i];
					AsteroidBeltInfo asteroidBeltInfo = app.GameDatabase.GetAsteroidBeltInfo(orbitalObjectInfo.ID);
					if (asteroidBeltInfo != null)
					{
						orbitalId = orbitalObjectInfo.ID;
					}
				}
				float length = app.GameDatabase.GetOrbitalTransform(orbitalId).Position.Length;
				float yawRadians = 0f;
				result = Matrix.CreateRotationYPR(yawRadians, 0f, 0f);
				result.Position = -result.Forward * length;
			}
			return result;
		}
		public static Matrix GetSpawnTransform(App app, Random rand, int shipIndex, OrbitalObjectInfo[] objects)
		{
			int orbitalId = 0;
			for (int i = 0; i < objects.Length; i++)
			{
				OrbitalObjectInfo orbitalObjectInfo = objects[i];
				AsteroidBeltInfo asteroidBeltInfo = app.GameDatabase.GetAsteroidBeltInfo(orbitalObjectInfo.ID);
				if (asteroidBeltInfo != null)
				{
					orbitalId = orbitalObjectInfo.ID;
				}
			}
			float length = app.GameDatabase.GetOrbitalTransform(orbitalId).Position.Length;
			float val = (float)Math.Tan((double)((app.AssetDatabase.DefaultTacSensorRange + 1000f) / length));
			float num = Math.Min(6.28318548f / (float)app.AssetDatabase.GlobalMorrigiRelicData.NumTombs, val);
			float num2 = num / 4f;
			float yawRadians = num * (float)shipIndex + rand.NextInclusive(-num2, num2);
			float num3 = rand.NextInclusive(-2500f, 2500f);
			Matrix result = Matrix.CreateRotationYPR(yawRadians, 0f, 0f);
			result.Position = result.Forward * (length + num3);
			return result;
		}
	}
}
