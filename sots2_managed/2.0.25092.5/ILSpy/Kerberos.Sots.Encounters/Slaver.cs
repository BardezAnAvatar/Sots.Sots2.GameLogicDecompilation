using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class Slaver
	{
		private const string FactionName = "slavers";
		private const string PlayerName = "Slaver";
		private const string PlayerAvatar = "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga";
		private const string FleetName = "Slaver Band";
		private const string wraithAbductorFile = "cr_wraith_abductor.section";
		private const string commandSectionFile = "cr_cmd.section";
		private const string engineSectionFile = "cr_eng_fusion.section";
		private const string scavengerSectionFile = "cr_mis_scavenger.section";
		private const string slaveDiskSectionFile = "br_slavedisk.section";
		private int PlayerId = -1;
		public static bool ForceEncounter;
		private int _wraithAbductorDesignId;
		private int _scavengerDesignId;
		private int _slaveDiskDesignId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int WraithAbductorDesignId
		{
			get
			{
				return this._wraithAbductorDesignId;
			}
		}
		public int ScavengerDesignId
		{
			get
			{
				return this._scavengerDesignId;
			}
		}
		public int SlaveDiskDesignId
		{
			get
			{
				return this._slaveDiskDesignId;
			}
		}
		public void InitDesigns(GameDatabase db)
		{
			List<DesignInfo> designs = db.GetDesignInfosForPlayer(this.PlayerId).ToList<DesignInfo>();
			this._slaveDiskDesignId = db.AddOrGetEncounterDesignInfo(designs, this.PlayerId, "Slave Disk", "slavers", new string[]
			{
				"br_slavedisk.section"
			});
			this._wraithAbductorDesignId = db.AddOrGetEncounterDesignInfo(designs, this.PlayerId, "Slaver Wraith Abductor", "slavers", new string[]
			{
				"cr_wraith_abductor.section"
			});
			this._scavengerDesignId = db.AddOrGetEncounterDesignInfo(designs, this.PlayerId, "Slaver Scavenger", "slavers", new string[]
			{
				"cr_mis_scavenger.section",
				"cr_cmd.section",
				"cr_eng_fusion.section"
			});
			DesignInfo designInfo = db.GetDesignInfo(this._slaveDiskDesignId);
			DesignInfo designInfo2 = db.GetDesignInfo(this._scavengerDesignId);
			designInfo.Role = ShipRole.SLAVEDISK;
			designInfo2.Role = ShipRole.SCAVENGER;
			db.UpdateDesign(designInfo);
			db.UpdateDesign(designInfo2);
		}
		public static Slaver InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Slaver slaver = new Slaver();
			slaver.PlayerId = gamedb.InsertPlayer("Slaver", "slavers", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			slaver.InitDesigns(gamedb);
			return slaver;
		}
		public static Slaver ResumeEncounter(GameDatabase gamedb)
		{
			Slaver slaver = new Slaver();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Slaver"));
			if (playerInfo != null)
			{
				slaver.PlayerId = playerInfo.ID;
			}
			else
			{
				slaver.PlayerId = gamedb.InsertPlayer("Slaver", "slavers", null, new Vector3(0f), new Vector3(0f), "", "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			slaver.InitDesigns(gamedb);
			return slaver;
		}
		public void UpdateTurn(GameSession game)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				game.GameDatabase.RemoveFleet(current.ID);
			}
		}
		public void ExecuteEncounter(GameSession game, PlayerInfo targetPlayer, int targetSystem)
		{
			Random safeRandom = App.GetSafeRandom();
			int num = safeRandom.Next(game.AssetDatabase.GlobalSlaverData.MinAbductors, game.AssetDatabase.GlobalSlaverData.MaxAbductors);
			int num2 = safeRandom.Next(game.AssetDatabase.GlobalSlaverData.MinScavengers, game.AssetDatabase.GlobalSlaverData.MaxScavengers);
			int fleetID = game.GameDatabase.InsertFleet(this.PlayerId, 0, targetSystem, targetSystem, "Slaver Band", FleetType.FL_NORMAL);
			for (int i = 0; i < num; i++)
			{
				game.GameDatabase.InsertShip(fleetID, this._wraithAbductorDesignId, null, (ShipParams)0, null, 0);
			}
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(this._scavengerDesignId);
			for (int j = 0; j < num2; j++)
			{
				int parentID = game.GameDatabase.InsertShip(fleetID, this._scavengerDesignId, null, (ShipParams)0, null, 0);
				if (designInfo != null)
				{
					int num3 = 0;
					DesignSectionInfo[] designSections = designInfo.DesignSections;
					for (int k = 0; k < designSections.Length; k++)
					{
						DesignSectionInfo designSectionInfo = designSections[k];
						List<WeaponBankInfo> list = DesignLab.ChooseWeapons(game, game.AssetDatabase.Weapons.ToList<LogicalWeapon>(), ShipRole.SCAVENGER, WeaponRole.PLANET_ATTACK, designSectionInfo.ShipSectionAsset, this.PlayerId, null);
						foreach (WeaponBankInfo current in list)
						{
							if (current.DesignID.HasValue && current.DesignID.Value != 0)
							{
								int shipID = game.GameDatabase.InsertShip(fleetID, current.DesignID.Value, null, (ShipParams)0, null, 0);
								game.GameDatabase.SetShipParent(shipID, parentID);
								game.GameDatabase.UpdateShipRiderIndex(shipID, num3);
								num3++;
							}
						}
					}
				}
			}
		}
		public void AddEncounter(GameSession game, PlayerInfo targetPlayer)
		{
			Random safeRandom = App.GetSafeRandom();
			List<int> list = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayer.ID).ToList<int>();
			if (list.Count > 0)
			{
				int num = safeRandom.Choose(list);
				game.GameDatabase.InsertIncomingRandom(targetPlayer.ID, num, RandomEncounter.RE_SLAVERS, game.GameDatabase.GetTurnCount() + 1);
				if (game.DetectedIncomingRandom(targetPlayer.ID, num))
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_SLAVERS,
						EventMessage = TurnEventMessage.EM_INCOMING_SLAVERS,
						PlayerID = targetPlayer.ID,
						SystemID = num,
						TurnNumber = game.GameDatabase.GetTurnCount()
					});
				}
			}
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			return Slaver.GetSpawnTransform(app, starSystem);
		}
		public static Matrix GetSpawnTransform(App app, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			List<CombatZonePositionInfo> list = (
				from x in starSystem.NeighboringSystems
				select starSystem.GetEnteryZoneForOuterSystem(x.SystemID)).ToList<CombatZonePositionInfo>();
			CombatZonePositionInfo combatZonePositionInfo = null;
			Vector3 vector = default(Vector3);
			float num = 3.40282347E+38f;
			new List<StellarBody>();
			foreach (StellarBody current in starSystem.GetPlanetsInSystem())
			{
				if (current.Population != 0.0)
				{
					foreach (CombatZonePositionInfo current2 in list)
					{
						Vector3 vector2 = current.Parameters.Position - current2.Center;
						float lengthSquared = vector2.LengthSquared;
						if (lengthSquared < num)
						{
							num = lengthSquared;
							vector = vector2;
							combatZonePositionInfo = current2;
						}
					}
				}
			}
			if (combatZonePositionInfo == null)
			{
				vector.X = (App.GetSafeRandom().CoinToss(0.5) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(0.0001f, 1f);
				vector.Y = 0f;
				vector.Z = (App.GetSafeRandom().CoinToss(0.5) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(0.0001f, 1f);
				vector.Normalize();
				float s = App.GetSafeRandom().NextInclusive(10000f, starSystem.GetSystemRadius());
				return Matrix.CreateWorld(vector * s, -vector, Vector3.UnitY);
			}
			vector.Normalize();
			return Matrix.CreateWorld(combatZonePositionInfo.Center, vector, Vector3.UnitY);
		}
	}
}
