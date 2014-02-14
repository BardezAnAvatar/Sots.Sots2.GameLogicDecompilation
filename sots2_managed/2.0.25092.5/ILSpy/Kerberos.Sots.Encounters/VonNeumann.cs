using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class VonNeumann
	{
		public enum VonNeumannShipDesigns
		{
			BerserkerMothership,
			BoardingPod,
			CollectorMothership,
			CollectorProbe,
			Disc,
			DiscAbsorber,
			DiscCloaker,
			DiscDisintegrator,
			DiscEmitter,
			DiscEMPulser,
			DiscImpactor,
			DiscOpressor,
			DiscPossessor,
			DiscScreamer,
			DiscShielder,
			NeoBerserker,
			PlanetKiller,
			Pyramid,
			SeekerMothership,
			SeekerProbe,
			Moon,
			UnderConstruction1,
			UnderConstruction2,
			UnderConstruction3
		}
		public class VonNeumannDesignInfo
		{
			public int DesignId;
			public string AssetName;
		}
		private const string FactionName = "vonneumann";
		private const string PlayerName = "Von Neumann";
		private const string PlayerAvatar = "\\base\\factions\\vonneumann\\avatars\\Vonneumann_Avatar.tga";
		private const string CollectorFleetName = "Von Neumann Collector";
		private const string SeekerFleetName = "Von Neumann Seeker";
		private const string BerserkerFleetName = "Von Neumann Berserker";
		private const string NeoBerserkerFleetName = "Von Neumann NeoBerserker";
		private const string SystemKillerFleetName = "Von Neumann System Killer";
		public const float VPriDiffToChangeTarget = 500f;
		public const float VPriDefencePlatformShift = -1500f;
		public const float VPriPlanetShift = 20f;
		public const float VChildIntegrateTime = 8f;
		private int PlayerId = -1;
		private int HomeWorldSystemId = -1;
		private int HomeWorldPlanetId = -1;
		private int NumInstances;
		private static int MaxVonNeumanns = 1;
		private List<KeyValuePair<StarSystemInfo, Vector3>> _outlyingStars;
		public static Dictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> StaticShipDesigns = new Dictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo>
		{

			{
				VonNeumann.VonNeumannShipDesigns.BerserkerMothership,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_berserker_mothership.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.BoardingPod,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_boarding_pod.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.CollectorMothership,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_collector_mothership.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.CollectorProbe,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_collector_probe.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscAbsorber,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_absorber.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscCloaker,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_cloaker.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscDisintegrator,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_disintegrator.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscEmitter,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_emitter.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscEMPulser,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_EMPulser.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscImpactor,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_impactor.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscOpressor,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_opressor.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscPossessor,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_possessor.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscScreamer,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_screamer.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.DiscShielder,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_disc_shielder.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.NeoBerserker,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_neo_berserker.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.PlanetKiller,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_planet_killer.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.Pyramid,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_pyramid.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.SeekerMothership,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_seeker_mothership.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.SeekerProbe,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_seeker_probe.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.Moon,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_moon.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.UnderConstruction1,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_underconstruction_v1.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.UnderConstruction2,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_underconstruction_v2.section"
				}
			},

			{
				VonNeumann.VonNeumannShipDesigns.UnderConstruction3,
				new VonNeumann.VonNeumannDesignInfo
				{
					AssetName = "vnm_underconstruction_v3.section"
				}
			}
		};
		public bool ForceVonNeumannAttack;
		public bool ForceVonNeumannAttackCycle;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int HomeWorldSystemID
		{
			get
			{
				return this.HomeWorldSystemId;
			}
		}
		public int HomeWorldPlanetID
		{
			get
			{
				return this.HomeWorldPlanetId;
			}
		}
		private VonNeumann()
		{
			this.PlayerId = -1;
			this.NumInstances = 0;
		}
		public static VonNeumann InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			VonNeumann vonNeumann = new VonNeumann();
			vonNeumann.PlayerId = gamedb.InsertPlayer("Von Neumann", "vonneumann", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\vonneumann\\avatars\\Vonneumann_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			List<KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo>> list = VonNeumann.StaticShipDesigns.ToList<KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo>>();
			foreach (KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> current in list)
			{
				DesignInfo design = new DesignInfo(vonNeumann.PlayerId, current.Key.ToString(), new string[]
				{
					string.Format("factions\\{0}\\sections\\{1}", "vonneumann", current.Value.AssetName)
				});
				DesignLab.SummarizeDesign(assetdb, gamedb, design);
				VonNeumann.StaticShipDesigns[current.Key] = new VonNeumann.VonNeumannDesignInfo
				{
					DesignId = gamedb.InsertDesignByDesignInfo(design),
					AssetName = current.Value.AssetName
				};
			}
			vonNeumann._outlyingStars = EncounterTools.GetOutlyingStars(gamedb);
			return vonNeumann;
		}
		public static VonNeumann ResumeEncounter(GameDatabase gamedb)
		{
			VonNeumann vonNeumann = new VonNeumann();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Von Neumann"));
			vonNeumann.PlayerId = playerInfo.ID;
			List<DesignInfo> source2 = gamedb.GetDesignInfosForPlayer(vonNeumann.PlayerId).ToList<DesignInfo>();
			Dictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> dictionary = new Dictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo>(VonNeumann.StaticShipDesigns);
			foreach (KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> kvp in dictionary)
			{
				DesignInfo designInfo = source2.FirstOrDefault(delegate(DesignInfo x)
				{
					string arg_20_0 = x.DesignSections[0].FilePath;
					KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> kvp2 = kvp;
					return arg_20_0.EndsWith(kvp2.Value.AssetName);
				});
				if (designInfo != null)
				{
					Dictionary<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> arg_B9_0 = VonNeumann.StaticShipDesigns;
					KeyValuePair<VonNeumann.VonNeumannShipDesigns, VonNeumann.VonNeumannDesignInfo> kvp3 = kvp;
					arg_B9_0[kvp3.Key].DesignId = designInfo.ID;
				}
			}
			List<FleetInfo> source3 = gamedb.GetFleetInfosByPlayerID(vonNeumann.PlayerID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			FleetInfo fleetInfo = source3.FirstOrDefault((FleetInfo x) => x.Name == "Von Neumann NeoBerserker");
			if (fleetInfo != null)
			{
				vonNeumann.HomeWorldSystemId = fleetInfo.SystemID;
				List<PlanetInfo> source4 = gamedb.GetPlanetInfosOrbitingStar(fleetInfo.SystemID).ToList<PlanetInfo>();
				PlanetInfo planetInfo = source4.FirstOrDefault<PlanetInfo>();
				if (planetInfo != null)
				{
					vonNeumann.HomeWorldPlanetId = planetInfo.ID;
				}
			}
			return vonNeumann;
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, NamesPool namesPool)
		{
			if (this.NumInstances >= VonNeumann.MaxVonNeumanns)
			{
				return;
			}
			int num = this._outlyingStars.Count<KeyValuePair<StarSystemInfo, Vector3>>();
			if (num == 0)
			{
				return;
			}
			Random safeRandom = App.GetSafeRandom();
			int val = 5;
			float s = 5f;
			KeyValuePair<StarSystemInfo, Vector3> item = this._outlyingStars[safeRandom.Next(Math.Min(num, val))];
			this._outlyingStars.Remove(item);
			Vector3 vector = item.Key.Origin + Vector3.Normalize(item.Value) * s;
			App.Log.Trace(string.Format("Found von neumann homeworld target - Picked System = {0}   Target Coords = {1}", item.Key.Name, vector), "game");
			StellarClass stellarClass = StarHelper.ChooseStellarClass(safeRandom);
			this.HomeWorldSystemId = gamedb.InsertStarSystem(null, namesPool.GetSystemName(), null, stellarClass.ToString(), vector, false, true, null);
			gamedb.GetStarSystemInfo(this.HomeWorldSystemId);
			int num2 = 5;
			float starOrbitStep = StarSystemVars.Instance.StarOrbitStep;
			float num3 = StarHelper.CalcRadius(stellarClass.Size) + (float)num2 * ((float)num2 * 0.1f * starOrbitStep);
			float x = Ellipse.CalcSemiMinorAxis(num3, 0f);
			OrbitalPath path = default(OrbitalPath);
			path.Scale = new Vector2(x, num3);
			path.InitialAngle = 0f;
			this.HomeWorldPlanetId = gamedb.InsertPlanet(null, this.HomeWorldSystemId, path, "VonNeumonia", "normal", null, 0f, 0, 0, 5f);
			PlanetInfo planetInfo = gamedb.GetPlanetInfo(this.HomeWorldPlanetId);
			path = default(OrbitalPath);
			path.Scale = new Vector2(15f, 15f);
			path.Rotation = new Vector3(0f, 0f, 0f);
			path.DeltaAngle = 10f;
			path.InitialAngle = 10f;
			VonNeumannInfo vonNeumannInfo = new VonNeumannInfo
			{
				SystemId = this.HomeWorldSystemId,
				OrbitalId = this.HomeWorldPlanetId,
				Resources = assetdb.GlobalVonNeumannData.StartingResources,
				ConstructionProgress = 0
			};
			float num4 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
			vonNeumannInfo.FleetId = new int?(gamedb.InsertFleet(this.PlayerId, 0, vonNeumannInfo.SystemId, vonNeumannInfo.SystemId, "Von Neumann NeoBerserker", FleetType.FL_NORMAL));
			float s2 = num4 + 2000f;
			float s3 = 1000f;
			Matrix matrix = gamedb.GetOrbitalTransform(this.HomeWorldPlanetId);
			matrix = Matrix.CreateWorld(matrix.Position, Vector3.Normalize(matrix.Position), Vector3.UnitY);
			Matrix value = matrix;
			value.Position = value.Position + value.Forward * s2 - value.Right * s3;
			for (int i = 0; i < 3; i++)
			{
				int shipID = gamedb.InsertShip(vonNeumannInfo.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.NeoBerserker].DesignId, null, (ShipParams)0, null, 0);
				gamedb.UpdateShipSystemPosition(shipID, new Matrix?(value));
				value.Position += value.Right * s3;
			}
			Random random = new Random();
			float radians = (random.CoinToss(0.5) ? -1f : 1f) * 0.7853982f;
			float radians2 = random.NextInclusive(0f, 6.28318548f);
			Matrix matrix2 = Matrix.CreateRotationY(radians);
			Matrix matrix3 = Matrix.CreateRotationY(radians2);
			Matrix value2 = Matrix.CreateWorld(matrix2.Forward * (matrix.Position.Length * 0.75f), matrix3.Forward, Vector3.UnitY);
			VonNeumann.VonNeumannShipDesigns key = (VonNeumann.VonNeumannShipDesigns)random.NextInclusive(21, 23);
			int shipID2 = gamedb.InsertShip(vonNeumannInfo.FleetId.Value, VonNeumann.StaticShipDesigns[key].DesignId, null, (ShipParams)0, null, 0);
			gamedb.UpdateShipSystemPosition(shipID2, new Matrix?(value2));
			value2.Position -= value2.Right * 1000f;
			shipID2 = gamedb.InsertShip(vonNeumannInfo.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.Moon].DesignId, null, (ShipParams)0, null, 0);
			gamedb.UpdateShipSystemPosition(shipID2, new Matrix?(value2));
			value2 = Matrix.CreateWorld(value2.Position + value2.Right * 1000f * 2f, -value2.Forward, Vector3.UnitY);
			shipID2 = gamedb.InsertShip(vonNeumannInfo.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.Moon].DesignId, null, (ShipParams)0, null, 0);
			gamedb.UpdateShipSystemPosition(shipID2, new Matrix?(value2));
			gamedb.InsertVonNeumannInfo(vonNeumannInfo);
			this.NumInstances++;
		}
		public void UpdateTurn(GameSession game, int id)
		{
			if (game.GameDatabase.GetTurnCount() < game.AssetDatabase.RandomEncMinTurns && !this.ForceVonNeumannAttack && !this.ForceVonNeumannAttackCycle)
			{
				return;
			}
			VonNeumannInfo vonNeumannInfo = game.GameDatabase.GetVonNeumannInfo(id);
			bool flag = vonNeumannInfo == null || this.HomeWorldSystemId <= 0;
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			bool arg_8D_0;
			if (!flag)
			{
				arg_8D_0 = (list.FirstOrDefault((FleetInfo x) => x.Name == "Von Neumann NeoBerserker") == null);
			}
			else
			{
				arg_8D_0 = true;
			}
			flag = arg_8D_0;
			if (flag)
			{
				foreach (FleetInfo current in list)
				{
					game.GameDatabase.RemoveFleet(current.ID);
				}
				game.GameDatabase.RemoveEncounter(id);
				return;
			}
			vonNeumannInfo.Resources += vonNeumannInfo.ResourcesCollectedLastTurn;
			vonNeumannInfo.ResourcesCollectedLastTurn = 0;
			if (vonNeumannInfo.ProjectDesignId.HasValue)
			{
				this.BuildProject(game, ref vonNeumannInfo);
			}
			this.ProcessTargets(game, ref vonNeumannInfo);
			this.SendCollector(game, ref vonNeumannInfo, this.ForceVonNeumannAttack);
			game.GameDatabase.UpdateVonNeumannInfo(vonNeumannInfo);
		}
		public static void HandleMomRetreated(GameSession game, int id, int ru)
		{
			VonNeumannInfo vonNeumannInfo = game.GameDatabase.GetVonNeumannInfo(id);
			if (vonNeumannInfo != null)
			{
				vonNeumannInfo.Resources += ru;
				game.GameDatabase.UpdateVonNeumannInfo(vonNeumannInfo);
			}
		}
		public bool IsHomeWorldFleet(FleetInfo fi)
		{
			return fi.Name == "Von Neumann NeoBerserker";
		}
		public bool CanSpawnFleetAtHomeWorld(FleetInfo fi)
		{
			return fi.Name == "Von Neumann NeoBerserker" || fi.Name == "Von Neumann System Killer";
		}
		public Matrix? GetVNFleetSpawnMatrixAtHomeWorld(GameDatabase gamedb, FleetInfo fi, int fleetIndex)
		{
			if (!this.CanSpawnFleetAtHomeWorld(fi))
			{
				return null;
			}
			Matrix orbitalTransform = gamedb.GetOrbitalTransform(this.HomeWorldPlanetId);
			Matrix value = Matrix.CreateWorld(orbitalTransform.Position, -Vector3.Normalize(orbitalTransform.Position), Vector3.UnitY);
			if (fi.Name == "Von Neumann System Killer")
			{
				value.Position += -value.Right * 30000f;
				return new Matrix?(value);
			}
			value.Position += -value.Forward * (3000f + 1000f * (float)fleetIndex);
			return new Matrix?(value);
		}
		private static FleetInfo GetCollectorFleetInfo(List<FleetInfo> fleets)
		{
			foreach (FleetInfo current in fleets)
			{
				if (current.Name == "Von Neumann Collector")
				{
					return current;
				}
			}
			return null;
		}
		private static FleetInfo GetSeekerFleetInfo(List<FleetInfo> fleets)
		{
			foreach (FleetInfo current in fleets)
			{
				if (current.Name == "Von Neumann Seeker")
				{
					return current;
				}
			}
			return null;
		}
		private static FleetInfo GetBerserkerFleetInfo(List<FleetInfo> fleets)
		{
			foreach (FleetInfo current in fleets)
			{
				if (current.Name == "Von Neumann Berserker")
				{
					return current;
				}
			}
			return null;
		}
		private static FleetInfo GetSystemKillerFleetInfo(List<FleetInfo> fleets)
		{
			foreach (FleetInfo current in fleets)
			{
				if (current.Name == "Von Neumann System Killer")
				{
					return current;
				}
			}
			return null;
		}
		private void ProcessTargets(GameSession game, ref VonNeumannInfo vi)
		{
			List<FleetInfo> fleets = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			FleetInfo seekerFleetInfo = VonNeumann.GetSeekerFleetInfo(fleets);
			FleetInfo berserkerFleetInfo = VonNeumann.GetBerserkerFleetInfo(fleets);
			FleetInfo systemKillerFleetInfo = VonNeumann.GetSystemKillerFleetInfo(fleets);
			int lastTargetSystem = vi.LastTargetSystem;
			VonNeumannTargetInfo vonNeumannTargetInfo = vi.TargetInfos.FirstOrDefault((VonNeumannTargetInfo x) => x.SystemId == lastTargetSystem);
			if (vonNeumannTargetInfo != null)
			{
				FleetInfo fleetInfo = null;
				switch (vonNeumannTargetInfo.ThreatLevel)
				{
				case 1:
					fleetInfo = seekerFleetInfo;
					break;
				case 2:
					fleetInfo = berserkerFleetInfo;
					break;
				case 3:
					fleetInfo = systemKillerFleetInfo;
					break;
				}
				if (fleetInfo == null)
				{
					if (vonNeumannTargetInfo.ThreatLevel < 3)
					{
						vonNeumannTargetInfo.ThreatLevel++;
					}
				}
				else
				{
					vi.TargetInfos.Remove(vonNeumannTargetInfo);
					game.GameDatabase.RemoveVonNeumannTargetInfo(vi.Id, vonNeumannTargetInfo.SystemId);
					fleetInfo.SystemID = vi.SystemId;
					game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, fleetInfo.SystemID, null);
					vi.FleetId = new int?(fleetInfo.ID);
				}
			}
			vi.LastTargetSystem = 0;
			int turnCount = game.GameDatabase.GetTurnCount();
			if (turnCount > vi.LastTargetTurn + game.AssetDatabase.GlobalVonNeumannData.TargetCycle || this.ForceVonNeumannAttackCycle)
			{
				vi.LastTargetTurn = turnCount;
				VonNeumannTargetInfo vonNeumannTargetInfo2 = vi.TargetInfos.FirstOrDefault<VonNeumannTargetInfo>();
				if (vonNeumannTargetInfo2 != null)
				{
					switch (vonNeumannTargetInfo2.ThreatLevel)
					{
					case 1:
						if (seekerFleetInfo != null)
						{
							seekerFleetInfo.SystemID = vonNeumannTargetInfo2.SystemId;
							vi.LastTargetSystem = vonNeumannTargetInfo2.SystemId;
							game.GameDatabase.UpdateFleetLocation(seekerFleetInfo.ID, seekerFleetInfo.SystemID, null);
							vi.FleetId = new int?(seekerFleetInfo.ID);
							return;
						}
						if (!vi.ProjectDesignId.HasValue)
						{
							vi.ProjectDesignId = new int?(VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.SeekerMothership].DesignId);
							return;
						}
						break;
					case 2:
						if (berserkerFleetInfo != null)
						{
							berserkerFleetInfo.SystemID = vonNeumannTargetInfo2.SystemId;
							vi.LastTargetSystem = vonNeumannTargetInfo2.SystemId;
							game.GameDatabase.UpdateFleetLocation(berserkerFleetInfo.ID, berserkerFleetInfo.SystemID, null);
							vi.FleetId = new int?(berserkerFleetInfo.ID);
							return;
						}
						if (!vi.ProjectDesignId.HasValue)
						{
							vi.ProjectDesignId = new int?(VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.BerserkerMothership].DesignId);
							return;
						}
						break;
					case 3:
						if (systemKillerFleetInfo != null)
						{
							systemKillerFleetInfo.SystemID = vonNeumannTargetInfo2.SystemId;
							vi.LastTargetSystem = vonNeumannTargetInfo2.SystemId;
							game.GameDatabase.UpdateFleetLocation(systemKillerFleetInfo.ID, systemKillerFleetInfo.SystemID, null);
							vi.FleetId = new int?(systemKillerFleetInfo.ID);
							return;
						}
						if (!vi.ProjectDesignId.HasValue)
						{
							vi.ProjectDesignId = new int?(VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.PlanetKiller].DesignId);
						}
						break;
					default:
						return;
					}
				}
			}
		}
		private void BuildProject(GameSession game, ref VonNeumannInfo vi)
		{
			int num = Math.Min(game.AssetDatabase.GlobalVonNeumannData.BuildRate, (int)((float)vi.Resources * (float)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier]));
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(vi.ProjectDesignId.Value);
			if (vi.ConstructionProgress + num > designInfo.ProductionCost || this.ForceVonNeumannAttackCycle)
			{
				vi.Resources -= (int)((float)(designInfo.ProductionCost - vi.ConstructionProgress) / (float)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier]);
				string name = "";
				if (vi.ProjectDesignId == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.SeekerMothership].DesignId)
				{
					name = "Von Neumann Seeker";
				}
				else
				{
					if (vi.ProjectDesignId == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.BerserkerMothership].DesignId)
					{
						name = "Von Neumann Berserker";
					}
					else
					{
						if (vi.ProjectDesignId == VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.PlanetKiller].DesignId)
						{
							name = "Von Neumann System Killer";
						}
					}
				}
				int fleetID = game.GameDatabase.InsertFleet(this.PlayerId, 0, vi.SystemId, vi.SystemId, name, FleetType.FL_NORMAL);
				game.GameDatabase.InsertShip(fleetID, vi.ProjectDesignId.Value, null, (ShipParams)0, null, 0);
				vi.ConstructionProgress = 0;
				vi.ProjectDesignId = null;
				return;
			}
			vi.Resources -= (int)((float)game.AssetDatabase.GlobalVonNeumannData.BuildRate / (float)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier]);
			vi.ConstructionProgress += num;
		}
		public void SendCollector(GameSession game, ref VonNeumannInfo vi, bool forceHomeworldAttack = false)
		{
			List<FleetInfo> fleets = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			VonNeumannGlobalData globalVonNeumannData = game.AssetDatabase.GlobalVonNeumannData;
			FleetInfo fleetInfo = VonNeumann.GetCollectorFleetInfo(fleets);
			if (fleetInfo == null)
			{
				DesignInfo designInfo = game.GameDatabase.GetDesignInfo(VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId);
				if ((float)vi.Resources > (float)designInfo.ProductionCost / (float)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier])
				{
					vi.Resources -= (int)((float)designInfo.ProductionCost / (float)game.AssetDatabase.DefaultStratModifiers[StratModifiers.OverharvestModifier]);
					vi.FleetId = new int?(game.GameDatabase.InsertFleet(this.PlayerId, 0, vi.SystemId, vi.SystemId, "Von Neumann Collector", FleetType.FL_NORMAL));
					game.GameDatabase.InsertShip(vi.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId, null, (ShipParams)0, null, 0);
					fleetInfo = game.GameDatabase.GetFleetInfo(vi.FleetId.Value);
				}
				else
				{
					vi.Resources = 0;
					vi.FleetId = new int?(game.GameDatabase.InsertFleet(this.PlayerId, 0, vi.SystemId, vi.SystemId, "Von Neumann Collector", FleetType.FL_NORMAL));
					game.GameDatabase.InsertShip(vi.FleetId.Value, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId, null, (ShipParams)0, null, 0);
					fleetInfo = game.GameDatabase.GetFleetInfo(vi.FleetId.Value);
				}
				int sysId = vi.LastCollectionSystem;
				if (!vi.TargetInfos.Any((VonNeumannTargetInfo x) => x.SystemId == sysId) && sysId != 0)
				{
					vi.TargetInfos.Add(new VonNeumannTargetInfo
					{
						SystemId = sysId,
						ThreatLevel = 1
					});
				}
			}
			else
			{
				if (vi.LastCollectionSystem != 0)
				{
					List<int> list = game.GameDatabase.GetStarSystemOrbitalObjectIDs(fleetInfo.SystemID).ToList<int>();
					foreach (int current in list)
					{
						PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(current);
						LargeAsteroidInfo largeAsteroidInfo = game.GameDatabase.GetLargeAsteroidInfo(current);
						int num = globalVonNeumannData.SalvageCapacity - vi.ResourcesCollectedLastTurn;
						if (planetInfo != null)
						{
							if (planetInfo.Resources > num)
							{
								planetInfo.Resources -= num;
								vi.ResourcesCollectedLastTurn = globalVonNeumannData.SalvageCapacity;
								break;
							}
							vi.ResourcesCollectedLastTurn += planetInfo.Resources;
							planetInfo.Resources = 0;
							game.GameDatabase.UpdatePlanet(planetInfo);
						}
						else
						{
							if (largeAsteroidInfo != null)
							{
								if (largeAsteroidInfo.Resources > num)
								{
									largeAsteroidInfo.Resources -= num;
									vi.ResourcesCollectedLastTurn = globalVonNeumannData.SalvageCapacity;
									break;
								}
								vi.ResourcesCollectedLastTurn += largeAsteroidInfo.Resources;
								largeAsteroidInfo.Resources = 0;
								game.GameDatabase.UpdateLargeAsteroidInfo(largeAsteroidInfo);
							}
						}
					}
					vi.Resources += vi.ResourcesCollectedLastTurn;
					fleetInfo.SystemID = vi.SystemId;
					game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, fleetInfo.SystemID, null);
				}
			}
			vi.LastCollectionSystem = 0;
			int turnCount = game.GameDatabase.GetTurnCount();
			if (fleetInfo != null && (turnCount > vi.LastCollectionTurn + globalVonNeumannData.SalvageCycle || forceHomeworldAttack || this.ForceVonNeumannAttackCycle))
			{
				vi.LastCollectionTurn = turnCount;
				List<int> list2 = new List<int>();
				List<int> list3 = game.GameDatabase.GetStarSystemIDs().ToList<int>();
				foreach (int system in list3)
				{
					if (!vi.TargetInfos.Any((VonNeumannTargetInfo x) => x.SystemId == system))
					{
						int? systemOwningPlayer = game.GameDatabase.GetSystemOwningPlayer(system);
						if (systemOwningPlayer.HasValue)
						{
							Player playerObject = game.GetPlayerObject(systemOwningPlayer.Value);
							if (playerObject == null || playerObject.IsAI())
							{
								continue;
							}
						}
						list2.Add(system);
					}
				}
				if (list2.Count > 0)
				{
					if (forceHomeworldAttack || this.ForceVonNeumannAttackCycle)
					{
						fleetInfo.SystemID = game.GameDatabase.GetOrbitalObjectInfo(game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID).Homeworld.Value).StarSystemID;
					}
					else
					{
						fleetInfo.SystemID = list2[App.GetSafeRandom().Next(list2.Count)];
					}
					vi.FleetId = new int?(fleetInfo.ID);
					vi.LastCollectionSystem = fleetInfo.SystemID;
					game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, fleetInfo.SystemID, null);
				}
			}
		}
		public void HandleHomeSystemDefeated(App app, FleetInfo fi, List<int> attackingPlayers)
		{
			VonNeumannInfo vonNeumannInfo = app.Game.GameDatabase.GetVonNeumannInfos().FirstOrDefault((VonNeumannInfo x) => x.FleetId == fi.ID);
			if (vonNeumannInfo != null)
			{
				foreach (int current in attackingPlayers)
				{
					float stratModifier = app.GameDatabase.GetStratModifier<float>(StratModifiers.ResearchModifier, current);
					app.GameDatabase.SetStratModifier(StratModifiers.ResearchModifier, current, stratModifier + 0.05f);
					app.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_VN_HW_DEFEATED,
						EventMessage = TurnEventMessage.EM_VN_HW_DEFEATED,
						PlayerID = current,
						TurnNumber = app.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				}
				List<FleetInfo> list = app.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
				foreach (FleetInfo current2 in list)
				{
					app.GameDatabase.RemoveFleet(current2.ID);
				}
				app.GameDatabase.RemoveEncounter(vonNeumannInfo.Id);
			}
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, int systemID)
		{
			return VonNeumann.GetSpawnTransform(app, systemID);
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
						s = num2 + 7000f;
					}
				}
			}
			if (orbitalObjectInfo == null)
			{
				return Matrix.CreateWorld(-Vector3.UnitZ * 50000f, Vector3.UnitZ, Vector3.UnitY);
			}
			Vector3 vector = -v;
			vector.Normalize();
			return Matrix.CreateWorld(v - vector * s, vector, Vector3.UnitY);
		}
	}
}
