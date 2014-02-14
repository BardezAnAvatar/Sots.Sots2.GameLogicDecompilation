using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STARSYSTEM)]
	internal class StarSystem : GameObject, IDisposable, IActive
	{
		public enum TerrestrialPlanetQuality
		{
			High = 512,
			Low = 128
		}
		public const float MasterScale = 5700f;
		public const float DefaultSystemViewScale = 1f;
		private OrbitPainter _orbitPainter;
		private bool _isDeepSpace;
		private float _scale = 1f;
		private readonly GameObjectSet _crits;
		private readonly GameObjectSet _slots;
		private Vector3 _origin;
		private bool _active;
		private int _system;
		public BidirMap<IGameObject, int> ObjectMap = new BidirMap<IGameObject, int>();
		public BidirMap<IGameObject, StationInfo> StationInfoMap = new BidirMap<IGameObject, StationInfo>();
		public BidirMap<IGameObject, int> PlanetMap = new BidirMap<IGameObject, int>();
		public List<CombatZonePositionInfo> CombatZones = new List<CombatZonePositionInfo>();
		public List<NeighboringSystemInfo> NeighboringSystems = new List<NeighboringSystemInfo>();
		public List<NodePoints> VisibleNodePoints = new List<NodePoints>();
		public List<ApproachingFleet> ApproachingFleets = new List<ApproachingFleet>();
		public List<AsteroidBelt> AsteroidBelts = new List<AsteroidBelt>();
		public Vector3 SystemOrigin = default(Vector3);
		public static readonly int CombatZoneIndices = 144;
		private static float RadiiMult = 0.7f;
		private static float BaseOffset = 0f;
		private float _starRadius;
		public int _furthestRing;
		public static readonly float[] CombatZoneMapRadii = new float[]
		{
			2f * StarSystem.RadiiMult,
			6f * StarSystem.RadiiMult,
			10f * StarSystem.RadiiMult,
			14f * StarSystem.RadiiMult,
			18f * StarSystem.RadiiMult,
			22f * StarSystem.RadiiMult,
			26f * StarSystem.RadiiMult,
			30f * StarSystem.RadiiMult,
			34f * StarSystem.RadiiMult,
			38f * StarSystem.RadiiMult
		};
		public static readonly int[] CombatZoneMapAngleDivs = new int[]
		{
			8,
			8,
			8,
			16,
			16,
			16,
			24,
			24,
			24,
			24
		};
		public OrbitPainter OrbitPainter
		{
			get
			{
				return this._orbitPainter;
			}
		}
		public bool IsDeepSpace
		{
			get
			{
				return this._isDeepSpace;
			}
		}
		public int SystemID
		{
			get
			{
				return this._system;
			}
		}
		public GameObjectSet Crits
		{
			get
			{
				return this._crits;
			}
		}
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active == value)
				{
					return;
				}
				this._active = true;
				this.PostSetActive(true);
			}
		}
		private static IEnumerable<object> GetCombatZoneMapInitParams()
		{
			yield return StarSystem.CombatZoneMapRadii.Length;
			try
			{
				float[] combatZoneMapRadii = StarSystem.CombatZoneMapRadii;
				for (int i = 0; i < combatZoneMapRadii.Length; i++)
				{
					float num = combatZoneMapRadii[i];
					yield return num;
				}
			}
			finally
			{
			}
			try
			{
				int[] combatZoneMapAngleDivs = StarSystem.CombatZoneMapAngleDivs;
				for (int j = 0; j < combatZoneMapAngleDivs.Length; j++)
				{
					int num2 = combatZoneMapAngleDivs[j];
					yield return num2;
				}
			}
			finally
			{
			}
			yield break;
		}
		public int GetCombatZoneIndexAtPosition(Vector3 position)
		{
			return this.GetCombatZoneIndexAtPosition(new Vector2(position.X, position.Z));
		}
		public int GetCombatZoneIndexAtPosition(Vector2 position)
		{
			position.X /= 5700f;
			position.Y /= 5700f;
			float lengthSq = position.LengthSq;
			int i;
			for (i = 0; i < StarSystem.CombatZoneMapRadii.Count<float>() - 1; i++)
			{
				float num = (this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[i]) * (this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[i]);
				if (lengthSq < num)
				{
					break;
				}
			}
			int num2 = 0;
			if (i > 0)
			{
				float num3 = 6.28318548f / (float)StarSystem.CombatZoneMapAngleDivs[i - 1];
				float num4 = (float)Math.Abs((Math.Atan2((double)position.Y, (double)position.X) + 12.566370614359172) % 6.2831853071795862);
				num2 = (int)(num4 / num3);
			}
			int num5 = 0;
			for (int j = 0; j < i - 1; j++)
			{
				num5 += StarSystem.CombatZoneMapAngleDivs[j];
			}
			return num5 + num2;
		}
		public static int GetCombatZoneIndex(int ring, int zone)
		{
			int num = 0;
			for (int i = 0; i < ring; i++)
			{
				num += StarSystem.CombatZoneMapAngleDivs[i];
			}
			return num + zone;
		}
		public CombatZonePositionInfo GetClosestZoneToPosition(App game, int playerID, Vector3 targetPosition)
		{
			CombatZonePositionInfo result = null;
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(this._system);
			if (starSystemInfo == null || starSystemInfo.ControlZones == null)
			{
				return result;
			}
			float num = 3.40282347E+38f;
			foreach (CombatZonePositionInfo current in this.CombatZones)
			{
				if (playerID == 0 || current.Player == playerID)
				{
					float lengthSquared = (current.Center - targetPosition).LengthSquared;
					if (lengthSquared < num)
					{
						num = lengthSquared;
						result = current;
					}
				}
			}
			return result;
		}
		public CombatZonePositionInfo GetCombatZonePositionInfo(int ringIndex, int zoneIndex)
		{
			return this.CombatZones.FirstOrDefault((CombatZonePositionInfo x) => x.RingIndex == ringIndex && x.ZoneIndex == zoneIndex);
		}
		public CombatZonePositionInfo GetEnteryZoneForOuterSystem(int outerSystemID)
		{
			if (outerSystemID == 0 || outerSystemID == this._system)
			{
				return null;
			}
			Vector3 starSystemOrigin = base.App.GameDatabase.GetStarSystemOrigin(outerSystemID);
			Vector3 position = starSystemOrigin - this.SystemOrigin;
			int combatZoneInRing = this.GetCombatZoneInRing(this._furthestRing - 1, position);
			return this.GetCombatZonePositionInfo(this._furthestRing - 1, combatZoneInRing);
		}
		public float GetBaseOffset()
		{
			return this._starRadius + StarSystem.BaseOffset;
		}
		public float GetStarRadius()
		{
			return this._starRadius * 5700f;
		}
		public int GetFurthestRing()
		{
			return this._furthestRing;
		}
		public float GetSystemRadius()
		{
			return (this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[this._furthestRing]) * 5700f;
		}
		public int GetCombatZoneRingAtRange(float range)
		{
			range /= 5700f;
			int num = 0;
			while (num < StarSystem.CombatZoneMapRadii.Count<float>() - 1 && range >= this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[num])
			{
				num++;
			}
			return num - 1;
		}
		public int GetCombatZoneInRing(int ring, Vector3 position)
		{
			int result = 0;
			if (ring >= 0)
			{
				float num = 6.28318548f / (float)StarSystem.CombatZoneMapAngleDivs[ring];
				float num2 = (float)Math.Abs((Math.Atan2((double)position.Z, (double)position.X) + 12.566370614359172) % 6.2831853071795862);
				result = (int)(num2 / num);
			}
			return result;
		}
		public static void PaintSystemPlayerColor(GameDatabase gamedb, int systemID, int playerID)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < StarSystem.CombatZoneIndices; i++)
			{
				list.Add(playerID);
			}
			gamedb.UpdateSystemCombatZones(systemID, list);
		}
		public static void RemoveSystemPlayerColor(GameDatabase gamedb, int systemID, int playerID)
		{
			int value = gamedb.GetSystemOwningPlayer(systemID) ?? 0;
			StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(systemID);
			if (starSystemInfo != null && starSystemInfo.ControlZones != null)
			{
				for (int i = 0; i < starSystemInfo.ControlZones.Count; i++)
				{
					if (starSystemInfo.ControlZones[i] == 0 || starSystemInfo.ControlZones[i] == playerID)
					{
						starSystemInfo.ControlZones[i] = value;
					}
				}
				gamedb.UpdateSystemCombatZones(systemID, starSystemInfo.ControlZones);
			}
		}
		public static void RestoreNeutralSystemColor(App game, int systemID, bool saveMultiCombat = false)
		{
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo == null)
			{
				return;
			}
			int value = 0;
			List<ColonyInfo> list = game.GameDatabase.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>();
			if (list.Count > 0)
			{
				value = list.First<ColonyInfo>().PlayerID;
			}
			if (starSystemInfo.ControlZones != null)
			{
				if (saveMultiCombat)
				{
					game.Game.MCCarryOverData.AddCarryOverCombatZoneInfo(systemID, starSystemInfo.ControlZones);
				}
				for (int i = 0; i < starSystemInfo.ControlZones.Count; i++)
				{
					if (starSystemInfo.ControlZones[i] == 0)
					{
						starSystemInfo.ControlZones[i] = value;
					}
				}
				game.GameDatabase.UpdateSystemCombatZones(systemID, starSystemInfo.ControlZones);
			}
		}
		public static void SaveCombatZonePlayerColor(GameDatabase gamedb, int systemID, int playerID, int index)
		{
			StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(systemID);
			if (starSystemInfo.ControlZones != null)
			{
				starSystemInfo.ControlZones[index] = playerID;
				gamedb.UpdateSystemCombatZones(systemID, starSystemInfo.ControlZones);
			}
		}
		private void ObtainFurthestRing(App game)
		{
			this._furthestRing = 0;
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(this._system);
			List<OrbitalObjectInfo> list = game.GameDatabase.GetStarSystemOrbitalObjectInfos(this._system).ToList<OrbitalObjectInfo>();
			if (starSystemInfo != null && list.Count != 0)
			{
				List<OrbitalObjectInfo> list2 = list.ToList<OrbitalObjectInfo>();
				float num = 0f;
				foreach (OrbitalObjectInfo current in list2)
				{
					Vector3 position = game.GameDatabase.GetOrbitalTransform(current.ID).Position;
					PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(current.ID);
					float num2 = position.Length;
					if (planetInfo != null)
					{
						num2 += StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					}
					if (num2 > num)
					{
						num = num2;
					}
				}
				num /= 5700f;
				num -= ((this.GetBaseOffset() > 0f) ? this.GetBaseOffset() : 2f);
				this._furthestRing = 0;
				this._furthestRing = 1;
				while (this._furthestRing < StarSystem.CombatZoneMapRadii.Count<float>() && num >= StarSystem.CombatZoneMapRadii[this._furthestRing - 1])
				{
					this._furthestRing++;
				}
				this._furthestRing = Math.Min(this._furthestRing, StarSystem.CombatZoneMapRadii.Length - 1);
			}
			this._furthestRing = Math.Max(this._furthestRing, 3);
		}
		private IEnumerable<object> CombatZoneMapInitParams()
		{
			yield return 5700f;
			yield return (this.GetBaseOffset() > 0f) ? this.GetBaseOffset() : 2f;
			if (this._furthestRing > 0)
			{
				yield return this._furthestRing + 1;
				for (int i = 0; i <= this._furthestRing; i++)
				{
					yield return StarSystem.CombatZoneMapRadii[i];
				}
				for (int j = 0; j <= this._furthestRing; j++)
				{
					yield return StarSystem.CombatZoneMapAngleDivs[j];
				}
			}
			else
			{
				yield return 0;
			}
			yield break;
		}
		public CombatZonePositionInfo ChangeCombatZoneOwner(int ring, int zone, Player player)
		{
			CombatZonePositionInfo combatZonePositionInfo = this.CombatZones.FirstOrDefault((CombatZonePositionInfo x) => x.RingIndex == ring && x.ZoneIndex == zone);
			if (combatZonePositionInfo == null)
			{
				return null;
			}
			combatZonePositionInfo.Player = ((player != null) ? player.ID : 0);
			return combatZonePositionInfo;
		}
		public void SetAutoDrawEnabled(bool value)
		{
			this.PostSetProp("AutoDrawEnabled", value);
		}
		public void SetCamera(OrbitCameraController value)
		{
			this.PostSetProp("CameraController", value.GetObjectID());
		}
		public void SetInputEnabled(bool value)
		{
			this.PostSetProp("InputEnabled", value);
		}
		public StarSystem(App game, float scale, int systemId, Vector3 origin, bool showOrbits, CombatSensor forCombatSensor, bool isInCombat, int inputID, bool showStationLabels = false, bool autodrawenabled = true)
		{
			this._crits = new GameObjectSet(game);
			this._slots = new GameObjectSet(game);
			this._system = systemId;
			this._scale = scale;
			this._origin = origin;
			this._furthestRing = 0;
			List<IGameObject> list = new List<IGameObject>();
			if (systemId != 0 && showOrbits)
			{
				this._orbitPainter = this._crits.Add<OrbitPainter>(new object[0]);
				list.Add(this._orbitPainter);
				if (forCombatSensor != null)
				{
					forCombatSensor.PostSetProp("OrbitPainter", this._orbitPainter.ObjectID);
				}
			}
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			this._starRadius = 0f;
			if (starSystemInfo != null && systemId != 0)
			{
				this._starRadius = StarHelper.CalcRadius(StellarClass.Parse(starSystemInfo.StellarClass).Size) / 5700f;
			}
			StarSystemInfo starSystemInfo2 = game.GameDatabase.GetStarSystemInfo(this._system);
			if (starSystemInfo2 != null)
			{
				this.SystemOrigin = game.GameDatabase.GetStarSystemOrigin(this._system);
			}
			this.ObtainFurthestRing(game);
			this.InitializeNeighboringSystems(game);
			this.InitializeSystemNodeLocations(game);
			this.InitializeApproachingFleets(game, isInCombat);
			List<object> list2 = new List<object>();
			list2.Add(this._orbitPainter.GetObjectID());
			list2.Add(this.NeighboringSystems.Count);
			foreach (NeighboringSystemInfo current in this.NeighboringSystems)
			{
				list2.Add(current.Name);
				list2.Add(current.Location);
			}
			list2.Add(this.VisibleNodePoints.Count);
			foreach (NodePoints current2 in this.VisibleNodePoints)
			{
				list2.Add(current2.Location);
				list2.Add(current2.Effect);
			}
			list2.Add(this.ApproachingFleets.Count);
			foreach (ApproachingFleet current3 in this.ApproachingFleets)
			{
				Player player = game.GetPlayer(current3.PlayerID);
				list2.Add(current3.Name);
				list2.Add(player.ObjectID);
				list2.Add(current3.Location);
			}
			list2.AddRange(this.CombatZoneMapInitParams());
			game.AddExistingObject(this, list2.ToArray());
			if (isInCombat)
			{
				this.AddNodeMaws(game);
			}
			this._isDeepSpace = (starSystemInfo != null && starSystemInfo.IsDeepSpace);
			IEnumerable<OrbitalObjectInfo> starSystemOrbitalObjectInfos = game.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId);
			if (starSystemInfo == null || starSystemInfo.IsDeepSpace)
			{
				foreach (OrbitalObjectInfo current4 in starSystemOrbitalObjectInfos)
				{
					StationInfo stationInfo = game.GameDatabase.GetStationInfo(current4.ID);
					if (stationInfo != null)
					{
						List<Ship> list3 = this.AddStationToSystem(game, stationInfo, stationInfo.OrbitalObjectID, inputID);
						foreach (Ship current5 in list3)
						{
							list.Add(current5);
							this._crits.Add(current5);
						}
					}
				}
				this.PostObjectAddObjects(list.ToArray());
				return;
			}
			StarModel starModel = this.CreateStar(this._crits, this._origin, starSystemInfo, true);
			this.ObjectMap.Insert(starModel, StarSystemDetailsUI.StarItemID);
			list.Add(starModel);
			this.CombatZones.Clear();
			StarSystemInfo starSystemInfo3 = base.App.GameDatabase.GetStarSystemInfo(systemId);
			if (starSystemInfo3.ControlZones != null)
			{
				List<object> list4 = new List<object>();
				List<int> list5 = base.App.Game.MCCarryOverData.GetPreviousControlZones(systemId);
				if (list5.Count != starSystemInfo.ControlZones.Count)
				{
					list5 = starSystemInfo.ControlZones;
				}
				list4.Add(list5.Count);
				for (int i = 0; i < list5.Count; i++)
				{
					Player player2 = base.App.GetPlayer(list5[i]);
					if (player2 != null)
					{
						list4.Add(player2.ObjectID);
					}
					else
					{
						list4.Add(0);
					}
				}
				this.PostSetProp("SyncZoneMapInfo", list4.ToArray());
			}
			this.CombatZones = StarSystem.GetCombatZonesForSystem(game.Game, systemId, scale);
			PlanetInfo[] starSystemPlanetInfos = game.GameDatabase.GetStarSystemPlanetInfos(systemId);
			PlanetInfo[] array = starSystemPlanetInfos;
			PlanetInfo planetInfo;
			for (int j = 0; j < array.Length; j++)
			{
				planetInfo = array[j];
				OrbitalObjectInfo orbitalObjectInfo = starSystemOrbitalObjectInfos.First((OrbitalObjectInfo x) => x.ID == planetInfo.ID);
				Matrix orbitalTransform = game.GameDatabase.GetOrbitalTransform(planetInfo.ID);
				StellarBody stellarBody = this.CreatePlanet(base.App.Game, this._crits, this._origin, planetInfo, orbitalTransform, true);
				if (isInCombat)
				{
					ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
					if (colonyInfoForPlanet != null)
					{
						stellarBody.WeaponBanks = this.AddWeaponsToPlanet(game, colonyInfoForPlanet, stellarBody);
						list.AddRange(stellarBody.WeaponBanks.ToArray());
						stellarBody.PostSetProp("SetHardenedStructures", colonyInfoForPlanet.isHardenedStructures && !game.GameDatabase.GetStarSystemInfo(colonyInfoForPlanet.CachedStarSystemID).IsOpen);
					}
				}
				this.ObjectMap.Insert(stellarBody, planetInfo.ID);
				this.PlanetMap.Insert(stellarBody, planetInfo.ID);
                list.Add(stellarBody);
                Vector3 scale1 = orbitalTransform.Position * this._scale;
				Vector3 vector = orbitalObjectInfo.ParentID.HasValue ? game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value).Position : this._origin;
				vector *= this._scale;
				float length = (orbitalTransform.Position * this._scale - vector).Length;
				Matrix orbitTransform = Matrix.CreateScale(length, length, length);
				orbitTransform.Position = vector;
				if (showOrbits)
				{
					this._orbitPainter.Add(orbitTransform);
				}
			}
			if (!isInCombat)
			{
				IEnumerable<FleetInfo> fleetInfoBySystemID = base.App.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_GATE | FleetType.FL_ACCELERATOR);
				if (fleetInfoBySystemID != null)
				{
					foreach (FleetInfo current6 in fleetInfoBySystemID)
					{
						foreach (ShipInfo current7 in base.App.GameDatabase.GetShipInfoByFleetID(current6.ID, false))
						{
							if (current7.ShipSystemPosition.HasValue)
							{
								Matrix value = current7.ShipSystemPosition.Value;
								ShipInfo shipInfo = new ShipInfo();
								shipInfo.SerialNumber = 1;
								shipInfo.ShipName = string.Empty;
								shipInfo.DesignID = current7.DesignInfo.ID;
								shipInfo.DesignInfo = current7.DesignInfo;
								shipInfo.ID = current7.ID;
								Ship ship = Ship.CreateShip(game.Game, value, shipInfo, 0, inputID, 0, false, null);
								list.Add(ship);
								this._crits.Add(ship);
							}
						}
					}
				}
			}
			foreach (OrbitalObjectInfo current8 in starSystemOrbitalObjectInfos)
			{
				StationInfo stationInfo2 = game.GameDatabase.GetStationInfo(current8.ID);
				if (stationInfo2 != null)
				{
					List<Ship> list6 = this.AddStationToSystem(game, stationInfo2, stationInfo2.OrbitalObjectID, inputID);
					foreach (Ship current9 in list6)
					{
						list.Add(current9);
						this._crits.Add(current9);
					}
					Ship ship2 = list6.FirstOrDefault<Ship>();
					if (ship2 != null)
					{
						this.StationInfoMap.Insert(ship2, stationInfo2);
						this.ObjectMap.Insert(ship2, stationInfo2.OrbitalObjectID);
						if (current8.ParentID.HasValue)
						{
							IGameObject state;
							if (!this.ObjectMap.Reverse.TryGetValue(current8.ParentID.Value, out state))
							{
								continue;
							}
							state.PostSetProp("AddStation", ship2);
						}
					}
				}
				this.SetAutoDrawEnabled(autodrawenabled);
			}
			List<AsteroidBeltInfo> list7 = game.GameDatabase.GetStarSystemAsteroidBeltInfos(systemId).ToList<AsteroidBeltInfo>();
			foreach (AsteroidBeltInfo belt in list7)
			{
				OrbitalObjectInfo orbitalObjectInfo2 = starSystemOrbitalObjectInfos.First((OrbitalObjectInfo x) => x.ID == belt.ID);
				Matrix orbitalTransform2 = game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo2.ID);
				List<LargeAsteroidInfo> list8 = game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(orbitalObjectInfo2.ID).ToList<LargeAsteroidInfo>();
				int num = 0;
				foreach (LargeAsteroidInfo current10 in list8)
				{
					float num2 = (float)(6.2831853071795862 / (double)((float)list8.Count<LargeAsteroidInfo>())) * (float)num;
					num++;
					float num3 = (float)(6.2831853071795862 / (double)((float)list8.Count<LargeAsteroidInfo>())) * (float)num;
					Matrix world = orbitalTransform2 * Matrix.CreateRotationY(num2 + App.GetSafeRandom().NextSingle() % num3);
					LargeAsteroid largeAsteroid = this.CreateLargeAsteroid(game, origin, current10, world);
					this.ObjectMap.Insert(largeAsteroid, current10.ID);
					list.Add(largeAsteroid);
					this._crits.Add(largeAsteroid);
				}
				AsteroidBelt asteroidBelt = this.CreateAsteroidBelt(game, origin, belt, orbitalTransform2);
				this.ObjectMap.Insert(asteroidBelt, belt.ID);
				list.Add(asteroidBelt);
				this.AsteroidBelts.Add(asteroidBelt);
				Vector3 scale1 = orbitalTransform2.Position * this._scale;
				Vector3 vector2 = orbitalObjectInfo2.ParentID.HasValue ? game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo2.ParentID.Value).Position : origin;
				vector2 *= this._scale;
				float length2 = (orbitalTransform2.Position * this._scale - vector2).Length;
				Matrix orbitTransform2 = Matrix.CreateScale(length2, length2, length2);
				orbitTransform2.Position = vector2;
				if (showOrbits)
				{
					this._orbitPainter.Add(orbitTransform2);
				}
			}
			this.PostObjectAddObjects(list.ToArray());
			this.InitializeSlots(systemId, isInCombat);
			if (showStationLabels)
			{
				List<Ship> list9 = this._crits.Objects.OfType<Ship>().ToList<Ship>();
				List<object> list10 = new List<object>();
				list10.Add(list9.Count);
				foreach (Ship current11 in list9)
				{
					DesignInfo designInfo = game.GameDatabase.GetDesignInfo(current11.DesignID);
					list10.Add(designInfo.Name);
					list10.Add(current11.Position);
				}
				this.PostSetProp("SyncStationLabels", list10.ToArray());
			}
		}
		private List<Ship> AddStationToSystem(App game, StationInfo stationInfo, int orbitalId, int inputId)
		{
			List<Ship> list = new List<Ship>();
			if (stationInfo != null)
			{
				Matrix orbitalTransform = game.GameDatabase.GetOrbitalTransform(orbitalId);
				ShipInfo shipInfo = new ShipInfo();
				shipInfo.SerialNumber = 1;
				shipInfo.ShipName = string.Empty;
				shipInfo.DesignID = stationInfo.DesignInfo.ID;
				shipInfo.DesignInfo = stationInfo.DesignInfo;
				shipInfo.ID = stationInfo.ShipID;
				Ship ship = Ship.CreateShip(game.Game, orbitalTransform, shipInfo, 0, inputId, 0, false, null);
				list.Add(ship);
				DesignSectionInfo[] designSections = stationInfo.DesignInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					foreach (WeaponBankInfo current in designSectionInfo.WeaponBanks)
					{
						if (current.DesignID.HasValue && current.DesignID != 0)
						{
							ShipSectionAsset shipSectionAsset = designSectionInfo.ShipSectionAsset;
							if (shipSectionAsset != null)
							{
								int num = 0;
								LogicalBank[] banks = shipSectionAsset.Banks;
								for (int j = 0; j < banks.Length; j++)
								{
									LogicalBank logicalBank = banks[j];
									if (current.BankGUID == logicalBank.GUID)
									{
										LogicalMount[] mounts = shipSectionAsset.Mounts;
										for (int k = 0; k < mounts.Length; k++)
										{
											LogicalMount logicalMount = mounts[k];
											if (logicalMount.Bank == logicalBank)
											{
												num++;
											}
										}
									}
								}
								for (int l = 0; l < num; l++)
								{
									ShipInfo shipInfo2 = new ShipInfo
									{
										SerialNumber = l,
										ShipName = string.Empty,
										DesignID = current.DesignID.Value
									};
									Ship item = Ship.CreateShip(game.Game, orbitalTransform, shipInfo2, ship.ObjectID, ship.InputID, ship.Player.ObjectID, false, null);
									list.Add(item);
								}
							}
						}
					}
				}
			}
			return list;
		}
		public void InitializeNeighboringSystems(App game)
		{
			this.NeighboringSystems.Clear();
			if (this._system == 0)
			{
				return;
			}
			int furthestRing = (this._furthestRing > 0) ? Math.Min(this._furthestRing + 1, StarSystem.CombatZoneMapRadii.Length - 1) : (StarSystem.CombatZoneMapRadii.Length - 1);
			float offsetDist = 2000f;
			List<StarSystemInfo> list = game.GameDatabase.GetSystemsInRange(this.SystemOrigin, game.AssetDatabase.StarSystemEntryPointRange).ToList<StarSystemInfo>();
			foreach (StarSystemInfo current in list)
			{
				if (current.ID != this._system)
				{
					this.AddNeighboringSystem(current, this.SystemOrigin, furthestRing, offsetDist);
				}
			}
		}
		public void InitializeSystemNodeLocations(App game)
		{
			this.VisibleNodePoints.Clear();
			Player localPlayer = game.LocalPlayer;
			if (localPlayer == null || this._system == 0)
			{
				return;
			}
			bool flag = localPlayer.Faction.CanUseNodeLine(null);
			bool flag2 = localPlayer.Faction.CanUseNodeLine(new bool?(true));
			bool flag3 = localPlayer.Faction.CanUseNodeLine(new bool?(false));
			if (!flag)
			{
				List<PlayerTechInfo> source = game.GameDatabase.GetPlayerTechInfos(localPlayer.ID).ToList<PlayerTechInfo>();
				List<PlayerTechInfo> list = (
					from x in source
					where x.TechFileID == "CCC_Node_Tracking:_Human" || x.TechFileID == "CCC_Node_Tracking:_Zuul"
					select x).ToList<PlayerTechInfo>();
				if (list.Count > 0)
				{
					flag = list.Any((PlayerTechInfo x) => x.State == TechStates.Researched);
					flag2 = list.Any((PlayerTechInfo x) => x.State == TechStates.Researched && x.TechFileID == "CCC_Node_Tracking:_Human");
					flag3 = list.Any((PlayerTechInfo x) => x.State == TechStates.Researched && x.TechFileID == "CCC_Node_Tracking:_Zuul");
				}
			}
			Faction faction = game.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.Name == "human");
			Faction faction2 = game.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.Name == "zuul");
			float num = (faction != null) ? faction.EntryPointOffset : 1000f;
			float num2 = (faction2 != null) ? faction2.EntryPointOffset : 1000f;
			int furthestRing = Math.Max(this._furthestRing - 1, 1);
			if (flag && (flag2 || flag3))
			{
				List<NodeLineInfo> list2 = (
					from x in game.GameDatabase.GetNodeLines()
					where !x.IsLoaLine
					select x).ToList<NodeLineInfo>();
				list2.AddRange((
					from x in game.GameDatabase.GetNonPermenantNodeLines()
					where !x.IsLoaLine
					select x).ToList<NodeLineInfo>());
				foreach (NodeLineInfo current in list2)
				{
					if (!current.IsLoaLine && (!current.IsPermenant || flag2) && (current.IsPermenant || flag3) && (current.System1ID == this._system || current.System2ID == this._system))
					{
						int systemId = (current.System1ID != this._system) ? current.System1ID : current.System2ID;
						StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
						float offsetDist = current.IsPermenant ? num : num2;
						this.AddNeighboringSystem(starSystemInfo, this.SystemOrigin, furthestRing, offsetDist);
						this.AddNodePointLocation(starSystemInfo, this.SystemOrigin, current.IsPermenant, current.ID, furthestRing, offsetDist);
					}
				}
			}
		}
		public void InitializeApproachingFleets(App game, bool isIncombat)
		{
			this.ApproachingFleets.Clear();
			if (this._system == 0)
			{
				return;
			}
			int furthestRing = (this._furthestRing > 0) ? Math.Min(this._furthestRing + 1, StarSystem.CombatZoneMapRadii.Length - 1) : (StarSystem.CombatZoneMapRadii.Length - 1);
			float offsetDist = 1000f;
			List<MoveOrderInfo> list = game.GameDatabase.GetMoveOrderInfos().ToList<MoveOrderInfo>();
			foreach (MoveOrderInfo mo in list)
			{
				if (mo.ToSystemID == this._system)
				{
					if (!this.ApproachingFleets.Any((ApproachingFleet x) => x.FleetID == mo.FleetID))
					{
						FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(mo.FleetID);
						if (fleetInfo != null && fleetInfo.Type == FleetType.FL_NORMAL)
						{
							FleetLocation fleetLocation = game.GameDatabase.GetFleetLocation(mo.FleetID, false);
							if (StarMap.IsInRange(game.GameDatabase, game.LocalPlayer.ID, fleetLocation.Coords, 1f, null))
							{
								FleetInfo fleetInfo2 = game.GameDatabase.GetFleetInfo(mo.FleetID);
								if (fleetInfo2 != null && game.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo2.PlayerID, game.LocalPlayer.ID) == DiplomacyState.WAR)
								{
									this.AddApproachingFleet(fleetInfo2, fleetLocation.Coords, this.SystemOrigin, furthestRing, offsetDist);
								}
							}
						}
					}
				}
			}
			if (!isIncombat)
			{
				foreach (MoveOrderInfo mo in game.GameDatabase.GetTempMoveOrders())
				{
					if (mo.ToSystemID == this._system)
					{
						if (!this.ApproachingFleets.Any((ApproachingFleet x) => x.FleetID == mo.FleetID))
						{
							FleetLocation fleetLocation2 = game.GameDatabase.GetFleetLocation(mo.FleetID, false);
							if (StarMap.IsInRange(game.GameDatabase, game.LocalPlayer.ID, fleetLocation2.Coords, 1f, null))
							{
								FleetInfo fleetInfo3 = game.GameDatabase.GetFleetInfo(mo.FleetID);
								if (fleetInfo3 != null && fleetInfo3.PreviousSystemID.HasValue)
								{
									StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(fleetInfo3.PreviousSystemID.Value);
									if (!(starSystemInfo == null) && game.GameDatabase.GetDiplomacyStateBetweenPlayers(fleetInfo3.PlayerID, game.LocalPlayer.ID) == DiplomacyState.WAR)
									{
										this.AddApproachingFleet(fleetInfo3, starSystemInfo.Origin, this.SystemOrigin, furthestRing, offsetDist);
									}
								}
							}
						}
					}
				}
			}
		}
		private void CorrectAllOverlaps()
		{
			float num = MathHelper.DegreesToRadians(5f);
			foreach (NeighboringSystemInfo current in this.NeighboringSystems)
			{
				bool flag = true;
				bool flag2 = false;
				Matrix rhs = Matrix.CreateWorld(Vector3.Zero, current.DirFromSystem, Vector3.UnitY);
				Vector3 vector = current.DirFromSystem;
				float num2 = 0f;
				while (flag)
				{
					flag = false;
					vector = (Matrix.CreateRotationYPR(num2, 0f, 0f) * rhs).Forward;
					foreach (NeighboringSystemInfo current2 in this.NeighboringSystems)
					{
						if (current.SystemID != current2.SystemID && Vector3.Dot(vector, current2.DirFromSystem) > 0.99f)
						{
							flag = true;
							flag2 = true;
							break;
						}
					}
					num2 += num;
				}
				if (flag2)
				{
					current.DirFromSystem = vector;
					current.BaseOffsetLocation = vector * current.BaseOffsetLocation.Length;
					current.Location = vector * current.Location.Length;
					foreach (NodePoints current3 in this.VisibleNodePoints)
					{
						if (current3.SystemID == current.SystemID)
						{
							current3.Location = vector * current3.Location.Length;
						}
					}
				}
			}
			foreach (ApproachingFleet current4 in this.ApproachingFleets)
			{
				bool flag3 = true;
				bool flag4 = false;
				Matrix rhs2 = Matrix.CreateWorld(Vector3.Zero, current4.DirFromSystem, Vector3.UnitY);
				Vector3 vector2 = current4.DirFromSystem;
				float num3 = 0f;
				while (flag3)
				{
					flag3 = false;
					vector2 = (Matrix.CreateRotationYPR(num3, 0f, 0f) * rhs2).Forward;
					foreach (ApproachingFleet current5 in this.ApproachingFleets)
					{
						if (current4.FleetID != current5.FleetID && Vector3.Dot(vector2, current5.DirFromSystem) > 0.99f)
						{
							flag3 = true;
							flag4 = true;
							break;
						}
					}
					num3 += num;
				}
				if (flag4)
				{
					current4.DirFromSystem = vector2;
					current4.Location = vector2 * current4.Location.Length;
				}
			}
		}
		private void AddNodeMaws(App game)
		{
			Player localPlayer = game.LocalPlayer;
			if (localPlayer == null || localPlayer.Faction == null)
			{
				return;
			}
			List<PlayerTechInfo> source = game.GameDatabase.GetPlayerTechInfos(localPlayer.ID).ToList<PlayerTechInfo>();
			PlayerTechInfo playerTechInfo = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "DRV_Node_Maw");
			if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
			{
				return;
			}
			LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == "NodeMaw");
			if (logicalWeapon != null)
			{
				List<NodeLineInfo> list = new List<NodeLineInfo>();
				if (localPlayer.Faction.CanUseNodeLine(new bool?(true)))
				{
					list = (
						from x in game.GameDatabase.GetNodeLines()
						where !x.IsLoaLine && (x.System1ID == this._system || x.System2ID == this._system) && this.VisibleNodePoints.Any((NodePoints y) => y.NodeID == x.ID)
						select x).ToList<NodeLineInfo>();
				}
				else
				{
					if (localPlayer.Faction.CanUseNodeLine(new bool?(false)))
					{
						list = (
							from x in game.GameDatabase.GetNonPermenantNodeLines()
							where !x.IsLoaLine && (x.System1ID == this._system || x.System2ID == this._system) && this.VisibleNodePoints.Any((NodePoints y) => y.NodeID == x.ID)
							select x).ToList<NodeLineInfo>();
					}
				}
				List<object> list2 = new List<object>();
				list2.Add(list.Count);
				foreach (NodeLineInfo node in list)
				{
					logicalWeapon.AddGameObjectReference();
					NodePoints nodePoints = this.VisibleNodePoints.First((NodePoints x) => x.NodeID == node.ID);
					list2.Add(logicalWeapon.GameObject.ObjectID);
					list2.Add(nodePoints.Location);
				}
				this.PostSetProp("SetNodeMawLocationsCombat", list2.ToArray());
			}
		}
		public List<Vector3> GetNodeMawLocationsForPlayer(App game, int playerID)
		{
			List<Vector3> list = new List<Vector3>();
			Player player = game.GetPlayer(playerID);
			if (player == null || player.Faction == null)
			{
				return list;
			}
			List<PlayerTechInfo> source = game.GameDatabase.GetPlayerTechInfos(playerID).ToList<PlayerTechInfo>();
			PlayerTechInfo playerTechInfo = source.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "DRV_Node_Maw");
			if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
			{
				return list;
			}
			float entryPointOffset = player.Faction.EntryPointOffset;
			int num = Math.Max(this._furthestRing - 1, 1);
			List<NodeLineInfo> list2 = new List<NodeLineInfo>();
			if (player.Faction.CanUseNodeLine(new bool?(true)))
			{
				list2 = (
					from x in game.GameDatabase.GetNodeLines()
					where !x.IsLoaLine && (x.System1ID == this._system || x.System2ID == this._system)
					select x).ToList<NodeLineInfo>();
			}
			else
			{
				if (player.Faction.CanUseNodeLine(new bool?(false)))
				{
					list2 = (
						from x in game.GameDatabase.GetNonPermenantNodeLines()
						where !x.IsLoaLine && (x.System1ID == this._system || x.System2ID == this._system)
						select x).ToList<NodeLineInfo>();
				}
			}
			foreach (NodeLineInfo current in list2)
			{
				StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo((current.System1ID != this._system) ? current.System1ID : current.System2ID);
				Vector3 v = starSystemInfo.Origin - this.SystemOrigin;
				v.Y = 0f;
				v.Normalize();
				list.Add(v * ((this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[num]) * 5700f + entryPointOffset));
			}
			return list;
		}
		private void AddNeighboringSystem(StarSystemInfo ssi, Vector3 systemOrigin, int furthestRing, float offsetDist)
		{
			if (ssi == null)
			{
				return;
			}
			Vector3 vector = ssi.Origin - systemOrigin;
			vector.Y = 0f;
			vector.Normalize();
			NeighboringSystemInfo neighboringSystemInfo = this.NeighboringSystems.FirstOrDefault((NeighboringSystemInfo x) => x.SystemID == ssi.ID);
			if (neighboringSystemInfo != null)
			{
				neighboringSystemInfo.Location = vector * ((this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[furthestRing]) * 5700f + offsetDist);
				return;
			}
			NeighboringSystemInfo neighboringSystemInfo2 = new NeighboringSystemInfo();
			neighboringSystemInfo2.Name = ssi.Name;
			neighboringSystemInfo2.SystemID = ssi.ID;
			neighboringSystemInfo2.DirFromSystem = vector;
			neighboringSystemInfo2.Location = vector * ((this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[furthestRing]) * 5700f + offsetDist);
			neighboringSystemInfo2.BaseOffsetLocation = vector * ((this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[this.GetFurthestRing()]) * 5700f);
			this.NeighboringSystems.Add(neighboringSystemInfo2);
		}
		private void AddNodePointLocation(StarSystemInfo ssi, Vector3 systemOrigin, bool isPermanent, int nodeID, int furthestRing, float offsetDist)
		{
			if (ssi == null)
			{
				return;
			}
			NodePoints nodePoints = new NodePoints();
			nodePoints.NodeID = nodeID;
			nodePoints.SystemID = ssi.ID;
			if (isPermanent)
			{
				nodePoints.Effect = "effects\\NodePoint_Human.effect";
			}
			else
			{
				nodePoints.Effect = "effects\\NodePoint_Zuul.effect";
			}
			Vector3 v = ssi.Origin - systemOrigin;
			v.Y = 0f;
			v.Normalize();
			nodePoints.Location = v * ((this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[furthestRing]) * 5700f + offsetDist);
			this.VisibleNodePoints.Add(nodePoints);
		}
		private void AddApproachingFleet(FleetInfo fi, Vector3 fleetPos, Vector3 systemOrigin, int furthestRing, float offsetDist)
		{
			if (fi == null)
			{
				return;
			}
			ApproachingFleet approachingFleet = new ApproachingFleet();
			Vector3 vector = fleetPos - systemOrigin;
			vector.Y = 0f;
			vector.Normalize();
			approachingFleet.Name = fi.Name;
			approachingFleet.FleetID = fi.ID;
			approachingFleet.PlayerID = fi.PlayerID;
			approachingFleet.DirFromSystem = vector;
			approachingFleet.Location = vector * ((this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[furthestRing]) * 5700f + offsetDist);
			this.ApproachingFleets.Add(approachingFleet);
		}
		public Vector3 GetClosestPermanentNodeToPosition(App game, Vector3 fleetPos)
		{
			Vector3 result = fleetPos;
			float num = 3.40282347E+38f;
			Faction faction = game.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.Name == "human");
			float num2 = (faction != null) ? faction.EntryPointOffset : 1000f;
			int num3 = Math.Max(this._furthestRing - 1, 1);
			List<NodeLineInfo> list = (
				from x in game.GameDatabase.GetNodeLines()
				where !x.IsLoaLine
				select x).ToList<NodeLineInfo>();
			foreach (NodeLineInfo current in list)
			{
				if (current.System1ID == this._system || current.System2ID == this._system)
				{
					int systemId = (current.System1ID != this._system) ? current.System1ID : current.System2ID;
					StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
					Vector3 v = starSystemInfo.Origin - this.SystemOrigin;
					v.Y = 0f;
					v.Normalize();
					Vector3 vector = v * ((this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[num3]) * 5700f + num2);
					float lengthSquared = (vector - fleetPos).LengthSquared;
					if (lengthSquared < num)
					{
						num = lengthSquared;
						result = vector;
					}
				}
			}
			return result;
		}
		public Vector3 GetClosestTempNodeToPosition(App game, Vector3 fleetPos)
		{
			Vector3 result = fleetPos;
			float num = 3.40282347E+38f;
			Faction faction = game.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.Name == "zuul");
			float num2 = (faction != null) ? faction.EntryPointOffset : 1000f;
			int num3 = Math.Max(this._furthestRing - 1, 1);
			List<NodeLineInfo> list = (
				from x in game.GameDatabase.GetNonPermenantNodeLines()
				where !x.IsLoaLine
				select x).ToList<NodeLineInfo>();
			foreach (NodeLineInfo current in list)
			{
				if (current.System1ID == this._system || current.System2ID == this._system)
				{
					int systemId = (current.System1ID != this._system) ? current.System1ID : current.System2ID;
					StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
					Vector3 v = starSystemInfo.Origin - this.SystemOrigin;
					v.Y = 0f;
					v.Normalize();
					Vector3 vector = v * ((this.GetBaseOffset() + StarSystem.CombatZoneMapRadii[num3]) * 5700f + num2);
					float lengthSquared = (vector - fleetPos).LengthSquared;
					if (lengthSquared < num)
					{
						num = lengthSquared;
						result = vector;
					}
				}
			}
			return result;
		}
		public static int GetFurthestRing(GameSession game, int systemID)
		{
			int num = 0;
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemID);
			List<OrbitalObjectInfo> list = game.GameDatabase.GetStarSystemOrbitalObjectInfos(systemID).ToList<OrbitalObjectInfo>();
			if (starSystemInfo != null && list.Count != 0)
			{
				float num2 = StarHelper.CalcRadius(StellarClass.Parse(starSystemInfo.StellarClass).Size);
				float num3 = num2 / 5700f + StarSystem.BaseOffset;
				List<OrbitalObjectInfo> list2 = list.ToList<OrbitalObjectInfo>();
				float num4 = 0f;
				foreach (OrbitalObjectInfo current in list2)
				{
					Vector3 position = game.GameDatabase.GetOrbitalTransform(current.ID).Position;
					PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(current.ID);
					float num5 = position.Length;
					if (planetInfo != null)
					{
						num5 += StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					}
					if (num5 > num4)
					{
						num4 = num5;
					}
				}
				num4 /= 5700f;
				num4 -= ((num3 > 0f) ? num3 : 2f);
				num = 1;
				while (num < StarSystem.CombatZoneMapRadii.Count<float>() && num4 >= StarSystem.CombatZoneMapRadii[num - 1])
				{
					num++;
				}
				num = Math.Min(num, StarSystem.CombatZoneMapRadii.Length - 1);
			}
			return Math.Max(num, 3);
		}
		public static List<CombatZonePositionInfo> GetCombatZonesForSystem(GameSession game, int systemID, float scale)
		{
			List<CombatZonePositionInfo> list = new List<CombatZonePositionInfo>();
			if (systemID == 0)
			{
				return list;
			}
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo == null)
			{
				return list;
			}
			float num = StarHelper.CalcRadius(StellarClass.Parse(starSystemInfo.StellarClass).Size);
			float num2 = num / 5700f;
			float num3 = 0f;
			List<OrbitalObjectInfo> list2 = game.GameDatabase.GetStarSystemOrbitalObjectInfos(systemID).ToList<OrbitalObjectInfo>();
			foreach (OrbitalObjectInfo current in list2)
			{
				Vector3 position = game.GameDatabase.GetOrbitalTransform(current.ID).Position;
				PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(current.ID);
				float num4 = position.Length;
				if (planetInfo != null)
				{
					num4 += StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
				}
				if (num4 > num3)
				{
					num3 = num4;
				}
			}
			StarSystemInfo starSystemInfo2 = game.GameDatabase.GetStarSystemInfo(systemID);
			int furthestRing = StarSystem.GetFurthestRing(game, systemID);
			int num5 = 0;
			while (num5 < StarSystem.CombatZoneMapRadii.Count<float>() - 1 && num5 != furthestRing)
			{
				for (int i = 0; i < StarSystem.CombatZoneMapAngleDivs[num5]; i++)
				{
					float num6 = 6.28318548f / (float)StarSystem.CombatZoneMapAngleDivs[num5];
					float num7 = (float)i * num6;
					float num8 = num7 + num6;
					float num9 = (num2 + StarSystem.BaseOffset + StarSystem.CombatZoneMapRadii[num5]) * 5700f;
					float num10 = (num2 + StarSystem.BaseOffset + StarSystem.CombatZoneMapRadii[num5 + 1]) * 5700f;
					float num11 = (num7 + num8) * 0.5f;
					Vector3 v = new Vector3((float)Math.Cos((double)num11), 0f, (float)Math.Sin((double)num11));
					Vector3 center = v * ((num10 + num9) * 0.5f);
					CombatZonePositionInfo combatZonePositionInfo = new CombatZonePositionInfo();
					int combatZoneIndex = StarSystem.GetCombatZoneIndex(num5, i);
					combatZonePositionInfo.Player = ((starSystemInfo2 != null && starSystemInfo2.ControlZones != null && combatZoneIndex < starSystemInfo2.ControlZones.Count) ? starSystemInfo2.ControlZones[StarSystem.GetCombatZoneIndex(num5, i)] : 0);
					combatZonePositionInfo.RingIndex = num5;
					combatZonePositionInfo.ZoneIndex = i;
					combatZonePositionInfo.Center = center;
					combatZonePositionInfo.AngleLeft = num7;
					combatZonePositionInfo.AngleRight = num8;
					combatZonePositionInfo.RadiusLower = num9;
					combatZonePositionInfo.RadiusUpper = num10;
					list.Add(combatZonePositionInfo);
				}
				num5++;
			}
			return list;
		}
		private LogicalWeapon GetBestPlanetBeamWeapon(App game, PlayerInfo planetOwner)
		{
			LogicalWeapon result = null;
			List<string> list = new List<string>
			{
				"Bem_Hvy_hclas",
				"Bem_Hvy_Lancer",
				"Bem_Hvy_Cutting"
			};
			List<LogicalWeapon> source = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, planetOwner.ID).ToList<LogicalWeapon>();
			foreach (string w in list)
			{
				LogicalWeapon logicalWeapon = source.FirstOrDefault((LogicalWeapon x) => string.Equals(x.WeaponName, w, StringComparison.InvariantCultureIgnoreCase));
				if (logicalWeapon != null)
				{
					result = logicalWeapon;
				}
			}
			return result;
		}
		private LogicalWeapon GetBestPlanetMissileWeapon(App game, PlayerInfo planetOwner)
		{
			return game.AssetDatabase.Weapons.First((LogicalWeapon x) => string.Equals(x.WeaponName, "Mis_IOBM", StringComparison.InvariantCultureIgnoreCase));
		}
		private LogicalWeapon GetBestPlanetHeavyMissileWeapon(App game, PlayerInfo planetOwner)
		{
			return game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => string.Equals(x.WeaponName, "Mis_HeavyIOBM", StringComparison.InvariantCultureIgnoreCase));
		}
		private LogicalWeapon GetBestMirvPlanetMissileWeapon(App game, PlayerInfo planetOwner)
		{
			return game.AssetDatabase.Weapons.First((LogicalWeapon x) => string.Equals(x.WeaponName, "Mis_Mirv_IOBM", StringComparison.InvariantCultureIgnoreCase));
		}
		private List<PlanetWeaponBank> AddWeaponsToPlanet(App game, ColonyInfo colony, StellarBody planet)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colony.PlayerID);
			List<PlanetWeaponBank> list = new List<PlanetWeaponBank>();
			if (!playerInfo.isStandardPlayer)
			{
				return list;
			}
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetFactionName(playerInfo.FactionID));
			LogicalWeapon weapon = null;
			if (game.GetStratModifier<bool>(StratModifiers.AllowPlanetBeam, playerInfo.ID))
			{
				weapon = this.GetBestPlanetBeamWeapon(game, playerInfo);
				if (weapon != null)
				{
					weapon.AddGameObjectReference();
					LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => string.Equals(x.WeaponName, weapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase));
					if (logicalWeapon != null)
					{
						logicalWeapon.AddGameObjectReference();
					}
					int num = (int)Math.Ceiling(game.GameDatabase.GetTotalPopulation(colony) / (double)game.AssetDatabase.PopulationPerPlanetBeam);
					if (num > 0)
					{
						int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(weapon, base.App.GameDatabase.GetPlayerTechInfos(playerInfo.ID).ToList<PlayerTechInfo>());
						WeaponModelPaths weaponModelPaths = LogicalWeapon.GetWeaponModelPaths(weapon, faction);
						WeaponModelPaths weaponModelPaths2 = LogicalWeapon.GetWeaponModelPaths(logicalWeapon, faction);
						PlanetWeaponBank planetWeaponBank = new PlanetWeaponBank(game, planet, null, null, weapon, weaponLevelFromTechs, logicalWeapon, weapon.DefaultWeaponClass, weaponModelPaths.ModelPath, weaponModelPaths2.ModelPath, game.AssetDatabase.PlanetBeamDelay, num);
						planetWeaponBank.AddExistingObject(game);
						list.Add(planetWeaponBank);
					}
				}
				else
				{
					App.Log.Warn("[[NON CRASHING BUG]] - Planet is allowed to create beams, but no available beam weapon was found, planet recieves no beam weapon", "game");
				}
			}
			if (game.GetStratModifier<bool>(StratModifiers.AllowMirvPlanetaryMissiles, playerInfo.ID))
			{
				weapon = this.GetBestMirvPlanetMissileWeapon(game, playerInfo);
				if (weapon != null)
				{
					weapon.AddGameObjectReference();
					LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => string.Equals(x.WeaponName, weapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase));
					string arg_200_0 = string.Empty;
					if (logicalWeapon != null)
					{
						logicalWeapon.AddGameObjectReference();
					}
					int num = (int)Math.Ceiling(game.GameDatabase.GetTotalPopulation(colony) / (double)game.AssetDatabase.PopulationPerPlanetMirv);
					if (num > 0)
					{
						int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(weapon, base.App.GameDatabase.GetPlayerTechInfos(playerInfo.ID).ToList<PlayerTechInfo>());
						WeaponModelPaths weaponModelPaths3 = LogicalWeapon.GetWeaponModelPaths(weapon, faction);
						WeaponModelPaths weaponModelPaths4 = LogicalWeapon.GetWeaponModelPaths(logicalWeapon, faction);
						PlanetWeaponBank planetWeaponBank2 = new PlanetWeaponBank(game, planet, null, null, weapon, weaponLevelFromTechs, logicalWeapon, weapon.DefaultWeaponClass, weaponModelPaths3.ModelPath, weaponModelPaths4.ModelPath, game.AssetDatabase.PlanetMissileDelay, num);
						planetWeaponBank2.AddExistingObject(game);
						list.Add(planetWeaponBank2);
					}
				}
				else
				{
					App.Log.Warn("[[NON CRASHING BUG]] - Planet is allowed to create MIRVs, but no available MIRV weapon was found, planet recieves no MIRV weapon", "game");
				}
			}
			LogicalWeapon missileWeapon = this.GetBestPlanetMissileWeapon(game, playerInfo);
			if (missileWeapon != null)
			{
				missileWeapon.AddGameObjectReference();
			}
			LogicalWeapon logicalWeapon2 = null;
			if (!string.IsNullOrEmpty(missileWeapon.SubWeapon))
			{
				logicalWeapon2 = game.AssetDatabase.Weapons.First((LogicalWeapon x) => string.Equals(x.WeaponName, missileWeapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase));
				if (logicalWeapon2 != null)
				{
					logicalWeapon2.AddGameObjectReference();
				}
			}
			int num2 = (int)Math.Ceiling(game.GameDatabase.GetTotalPopulation(colony) / (double)game.AssetDatabase.PopulationPerPlanetMissile);
			if (num2 > 0)
			{
				int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(missileWeapon, base.App.GameDatabase.GetPlayerTechInfos(playerInfo.ID).ToList<PlayerTechInfo>());
				WeaponModelPaths weaponModelPaths5 = LogicalWeapon.GetWeaponModelPaths(missileWeapon, faction);
				WeaponModelPaths weaponModelPaths6 = LogicalWeapon.GetWeaponModelPaths(logicalWeapon2, faction);
				PlanetWeaponBank planetWeaponBank3 = new PlanetWeaponBank(game, planet, null, null, missileWeapon, weaponLevelFromTechs, logicalWeapon2, missileWeapon.DefaultWeaponClass, weaponModelPaths5.ModelPath, weaponModelPaths6.ModelPath, game.AssetDatabase.PlanetMissileDelay, num2);
				planetWeaponBank3.AddExistingObject(game);
				list.Add(planetWeaponBank3);
			}
			PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfos(colony.PlayerID).FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "WAR_Heavy_Planet_Missiles");
			if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
			{
				num2 = (int)Math.Ceiling(game.GameDatabase.GetTotalPopulation(colony) / (double)game.AssetDatabase.PopulationPerHeavyPlanetMissile);
				if (num2 > 0)
				{
					missileWeapon = this.GetBestPlanetHeavyMissileWeapon(game, playerInfo);
					if (missileWeapon != null)
					{
						missileWeapon.AddGameObjectReference();
						logicalWeapon2 = null;
						if (!string.IsNullOrEmpty(missileWeapon.SubWeapon))
						{
							logicalWeapon2 = game.AssetDatabase.Weapons.First((LogicalWeapon x) => string.Equals(x.WeaponName, missileWeapon.SubWeapon, StringComparison.InvariantCultureIgnoreCase));
							if (logicalWeapon2 != null)
							{
								logicalWeapon2.AddGameObjectReference();
							}
						}
						if (num2 > 0)
						{
							int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(missileWeapon, base.App.GameDatabase.GetPlayerTechInfos(playerInfo.ID).ToList<PlayerTechInfo>());
							WeaponModelPaths weaponModelPaths7 = LogicalWeapon.GetWeaponModelPaths(missileWeapon, faction);
							WeaponModelPaths weaponModelPaths8 = LogicalWeapon.GetWeaponModelPaths(logicalWeapon2, faction);
							PlanetWeaponBank planetWeaponBank4 = new PlanetWeaponBank(game, planet, null, null, missileWeapon, weaponLevelFromTechs, logicalWeapon2, missileWeapon.DefaultWeaponClass, weaponModelPaths7.ModelPath, weaponModelPaths8.ModelPath, game.AssetDatabase.PlanetMissileDelay, num2);
							planetWeaponBank4.AddExistingObject(game);
							list.Add(planetWeaponBank4);
						}
					}
				}
			}
			return list;
		}
		private void InitializeSlots(int systemID, bool isInCombat)
		{
			base.App.GameDatabase.GetStarSystemOrbitalObjectInfos(systemID);
			foreach (IGameObject current in this._crits)
			{
				StellarBody planet = current as StellarBody;
				if (planet != null)
				{
					List<SlotData> slotsForPlanet = this.GetSlotsForPlanet(planet, false);
					float num = 360f / (float)slotsForPlanet.Count;
					for (int i = 0; i < slotsForPlanet.Count; i++)
					{
						Matrix orbitalTransform = base.App.GameDatabase.GetOrbitalTransform(planet.PlanetInfo.ID);
						float num2 = StarSystemVars.Instance.SizeToRadius(planet.PlanetInfo.Size);
						Matrix rhs = Matrix.CreateTranslation(orbitalTransform.Position);
						Matrix lhs = Matrix.CreateTranslation(num2 + (float)StarSystemVars.Instance.StationOrbitDistance, 0f, 0f);
						Matrix rhs2 = Matrix.CreateRotationY((num * (float)i + 45f) * 0.0174444448f);
						Matrix matrix = lhs * rhs2 * rhs;
						slotsForPlanet[i].Position = matrix.Position;
						slotsForPlanet[i].Rotation = matrix.EulerAngles.X;
						if (!isInCombat)
						{
							ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(planet.Parameters.OrbitalID);
							if (planet.Parameters.ColonyPlayerID == base.App.LocalPlayer.ID || (colonyInfoForPlanet != null && colonyInfoForPlanet.IsIndependentColony(base.App)) || planet.Parameters.BodyType == "gaseous" || planet.Parameters.BodyType == "barren" || (base.App.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowDeepSpaceConstruction, base.App.LocalPlayer.ID) && colonyInfoForPlanet == null))
							{
								StarSystemPlacementSlot starSystemPlacementSlot = new StarSystemPlacementSlot(base.App, slotsForPlanet[i]);
								this._slots.Add(starSystemPlacementSlot);
								this.PostObjectAddObjects(new IGameObject[]
								{
									starSystemPlacementSlot
								});
								starSystemPlacementSlot.SetTransform(matrix.Position, matrix.EulerAngles.X);
							}
						}
					}
					using (IEnumerator<IGameObject> enumerator2 = this._crits.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							IGameObject current2 = enumerator2.Current;
							Ship ship = current2 as Ship;
							if (ship != null && this.StationInfoMap.Forward.Keys.Contains(current2))
							{
								StationInfo stationInfo = this.StationInfoMap.Forward[current2];
								OrbitalObjectInfo orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
								if (orbitalObjectInfo.ParentID == planet.PlanetInfo.ID)
								{
									bool flag = false;
									StationInfo stinf = base.App.GameDatabase.GetStationInfo(orbitalObjectInfo.ID);
									foreach (SlotData current3 in slotsForPlanet)
									{
										if ((current3.SupportedTypes & (StationTypeFlags)(1 << (int)stinf.DesignInfo.StationType)) > (StationTypeFlags)0 && current3.OccupantID == 0)
										{
											if (!isInCombat)
											{
												StarSystemPlacementSlot starSystemPlacementSlot2 = this._slots.OfType<StarSystemPlacementSlot>().FirstOrDefault((StarSystemPlacementSlot x) => x._slotData.Parent == planet.ObjectID && (x._slotData.SupportedTypes & (StationTypeFlags)(1 << (int)stinf.DesignInfo.StationType)) > (StationTypeFlags)0 && x._slotData.OccupantID == 0);
												if (starSystemPlacementSlot2 != null)
												{
													starSystemPlacementSlot2.SetOccupant(ship);
													starSystemPlacementSlot2.PostSetProp("StationType", new object[]
													{
														stationInfo.DesignInfo.StationType.ToFlags(),
														base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(stationInfo.PlayerID)) == "zuul"
													});
												}
											}
											current3.OccupantID = ship.ObjectID;
											ship.InitialSetPos(current3.Position, Vector3.Zero);
											flag = true;
											break;
										}
									}
									if (!flag)
									{
										List<SlotData> slotsForPlanet2 = this.GetSlotsForPlanet(planet, true);
										for (int j = 0; j < slotsForPlanet2.Count; j++)
										{
											if ((slotsForPlanet2[j].SupportedTypes & (StationTypeFlags)(1 << (int)stinf.DesignInfo.StationType)) > (StationTypeFlags)0 && slotsForPlanet[j].OccupantID == 0)
											{
												slotsForPlanet[j].OccupantID = ship.ObjectID;
												ship.InitialSetPos(slotsForPlanet[j].Position, Vector3.Zero);
												break;
											}
										}
									}
								}
							}
						}
						continue;
					}
				}
				LargeAsteroid asteroid = current as LargeAsteroid;
				if (asteroid != null)
				{
					base.App.GameDatabase.GetOrbitalObjectInfo(asteroid.ID);
					List<SlotData> slotsForAsteroid = this.GetSlotsForAsteroid(asteroid);
					List<StarSystemPlacementSlot> list = new List<StarSystemPlacementSlot>();
					float num3 = 1000f;
					float num4 = 360f / (float)slotsForAsteroid.Count;
					for (int k = 0; k < slotsForAsteroid.Count; k++)
					{
						Matrix rhs3 = Matrix.CreateTranslation(asteroid.WorldTransform.Position);
						Matrix lhs2 = Matrix.CreateTranslation(num3 + (float)StarSystemVars.Instance.StationOrbitDistance, 0f, 0f);
						Matrix rhs4 = Matrix.CreateRotationY((num4 * (float)k + 45f) * 0.0174444448f);
						Matrix matrix2 = lhs2 * rhs4 * rhs3;
						slotsForAsteroid[k].Position = matrix2.Position;
						slotsForAsteroid[k].Rotation = matrix2.EulerAngles.X;
						if (!isInCombat)
						{
							StarSystemPlacementSlot starSystemPlacementSlot3 = new StarSystemPlacementSlot(base.App, slotsForAsteroid[k]);
							starSystemPlacementSlot3.PostSetProp("SetPlacementEnabled", true);
							starSystemPlacementSlot3.SetTransform(matrix2.Position, matrix2.EulerAngles.X);
							list.Add(starSystemPlacementSlot3);
							this._slots.Add(starSystemPlacementSlot3);
							this.PostObjectAddObjects(new IGameObject[]
							{
								starSystemPlacementSlot3
							});
						}
					}
					foreach (IGameObject current4 in this._crits)
					{
						Ship ship2 = current4 as Ship;
						if (ship2 != null && this.StationInfoMap.Forward.Keys.Contains(current4))
						{
							StationInfo stationInfo2 = this.StationInfoMap.Forward[current4];
							OrbitalObjectInfo orbitalObjectInfo2 = base.App.GameDatabase.GetOrbitalObjectInfo(stationInfo2.OrbitalObjectID);
							if (orbitalObjectInfo2.ParentID == asteroid.ID)
							{
								StationInfo stinf = base.App.GameDatabase.GetStationInfo(orbitalObjectInfo2.ID);
								foreach (SlotData current5 in slotsForAsteroid)
								{
									if ((current5.SupportedTypes & (StationTypeFlags)(1 << (int)stinf.DesignInfo.StationType)) > (StationTypeFlags)0 && current5.OccupantID == 0)
									{
										if (!isInCombat)
										{
											StarSystemPlacementSlot starSystemPlacementSlot4 = list.FirstOrDefault((StarSystemPlacementSlot x) => x._slotData.Parent == asteroid.ObjectID && (x._slotData.SupportedTypes & (StationTypeFlags)(1 << (int)stinf.DesignInfo.StationType)) > (StationTypeFlags)0 && x._slotData.OccupantID == 0);
											if (starSystemPlacementSlot4 != null)
											{
												starSystemPlacementSlot4.SetOccupant(ship2);
												starSystemPlacementSlot4.PostSetProp("StationType", new object[]
												{
													stationInfo2.DesignInfo.StationType.ToFlags(),
													base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(stationInfo2.PlayerID)) == "zuul"
												});
											}
										}
										current5.OccupantID = ship2.ObjectID;
										ship2.InitialSetPos(current5.Position, Vector3.Zero);
										break;
									}
								}
							}
						}
					}
				}
			}
		}
		public List<StellarBody> GetPlanetsInSystem()
		{
			List<StellarBody> list = new List<StellarBody>();
			foreach (IGameObject current in this._crits)
			{
				if (current is StellarBody)
				{
					list.Add(current as StellarBody);
				}
			}
			return list;
		}
		public List<Ship> GetStationsAroundPlanet(int planetID)
		{
			List<Ship> list = new List<Ship>();
			foreach (IGameObject current in this._crits)
			{
				StellarBody stellarBody = current as StellarBody;
				if (stellarBody != null && stellarBody.PlanetInfo.ID == planetID)
				{
					foreach (IGameObject current2 in this._crits)
					{
						Ship ship = current2 as Ship;
						if (ship != null)
						{
							StationInfo stationInfo = this.StationInfoMap.Forward[current2];
							if (base.App.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID).ParentID == stellarBody.PlanetInfo.ID)
							{
								list.Add(ship);
							}
						}
					}
				}
			}
			return list;
		}
		public StationInfo GetStationInfo(Ship station)
		{
			StationInfo result;
			this.StationInfoMap.Forward.TryGetValue(station, out result);
			return result;
		}
		protected int GetNumSupportedSlots(StellarBody body)
		{
			if (body.Parameters.BodyType == "gaseous")
			{
				return 2;
			}
			if (body.Parameters.BodyType == "barren")
			{
				return 1;
			}
			return 4;
		}
		public static bool IsColonizablePlanetType(string type)
		{
			return type == "normal" || type == "pastoral" || type == "volcanic" || type == "cavernous" || type == "tempestuous" || type == "magnar" || type == "primordial";
		}
		public static int? GetSuitablePlanetForStation(GameSession game, int playerID, int systemID, StationType stationtype)
		{
			int? result = null;
			if (stationtype != StationType.MINING)
			{
				if (stationtype == StationType.SCIENCE)
				{
					List<ColonyInfo> list = (
						from x in game.GameDatabase.GetColonyInfosForSystem(systemID)
						where x.IsIndependentColony(game.App)
						select x).ToList<ColonyInfo>();
					foreach (ColonyInfo current in list)
					{
						if (StarSystem.GetAvailSlotTypesForPlanet(game, systemID, current.OrbitalObjectID, playerID).Contains(stationtype))
						{
							result = new int?(current.OrbitalObjectID);
							break;
						}
					}
				}
				List<ColonyInfo> Colonies = (
					from x in game.GameDatabase.GetColonyInfosForSystem(systemID)
					where x.PlayerID == playerID
					select x).ToList<ColonyInfo>();
				if (!result.HasValue)
				{
					foreach (ColonyInfo current2 in Colonies)
					{
						if (StarSystem.GetAvailSlotTypesForPlanet(game, systemID, current2.OrbitalObjectID, playerID).Contains(stationtype))
						{
							result = new int?(current2.OrbitalObjectID);
							break;
						}
					}
				}
				if (result.HasValue)
				{
					return result;
				}
				List<PlanetInfo> list2 = (
					from x in game.GameDatabase.GetPlanetInfosOrbitingStar(systemID)
					where !Colonies.Any((ColonyInfo j) => j.OrbitalObjectID == x.ID)
					select x).ToList<PlanetInfo>();
				using (List<PlanetInfo>.Enumerator enumerator3 = list2.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						PlanetInfo current3 = enumerator3.Current;
						if (StarSystem.GetAvailSlotTypesForPlanet(game, systemID, current3.ID, playerID).Contains(stationtype))
						{
							result = new int?(current3.ID);
							break;
						}
					}
					return result;
				}
			}
			IList<AsteroidBeltInfo> list3 = game.GameDatabase.GetStarSystemAsteroidBeltInfos(systemID).ToList<AsteroidBeltInfo>();
			foreach (AsteroidBeltInfo current4 in list3)
			{
				List<LargeAsteroidInfo> list4 = game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(current4.ID).ToList<LargeAsteroidInfo>();
				foreach (LargeAsteroidInfo current5 in list4)
				{
					List<StationType> availSlotTypesForAsteroid = StarSystem.GetAvailSlotTypesForAsteroid(game, systemID, current5.ID, playerID);
					if (availSlotTypesForAsteroid.Contains(StationType.MINING))
					{
						result = new int?(current5.ID);
						break;
					}
				}
			}
			if (!result.HasValue)
			{
				List<PlanetInfo> list5 = game.GameDatabase.GetStarSystemPlanetInfos(systemID).ToList<PlanetInfo>();
				foreach (PlanetInfo current6 in list5)
				{
					if (StarSystem.GetAvailSlotTypesForPlanet(game, systemID, current6.ID, playerID).Contains(stationtype))
					{
						result = new int?(current6.ID);
						break;
					}
				}
			}
			return result;
		}
		private static List<StationType> GetAvailSlotTypesForAsteroid(GameSession game, int systemID, int asteroidid, int playerId)
		{
			Dictionary<StationType, int> dictionary = new Dictionary<StationType, int>();
			List<StationType> list = new List<StationType>();
			dictionary.Add(StationType.CIVILIAN, 0);
			list.Add(StationType.CIVILIAN);
			dictionary.Add(StationType.DEFENCE, 0);
			list.Add(StationType.DEFENCE);
			dictionary.Add(StationType.DIPLOMATIC, 0);
			list.Add(StationType.DIPLOMATIC);
			dictionary.Add(StationType.GATE, 0);
			list.Add(StationType.GATE);
			dictionary.Add(StationType.MINING, 0);
			list.Add(StationType.MINING);
			dictionary.Add(StationType.NAVAL, 0);
			list.Add(StationType.NAVAL);
			dictionary.Add(StationType.SCIENCE, 0);
			list.Add(StationType.SCIENCE);
			LargeAsteroidInfo asteroid = game.GameDatabase.GetLargeAsteroidInfo(asteroidid);
			if (asteroid != null)
			{
				Dictionary<StationType, int> dictionary2;
				(dictionary2 = dictionary)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] + 2;
				Dictionary<StationType, int> dictionary3;
				(dictionary3 = dictionary)[StationType.SCIENCE] = dictionary3[StationType.SCIENCE] + 1;
				Dictionary<StationType, int> dictionary4;
				(dictionary4 = dictionary)[StationType.MINING] = dictionary4[StationType.MINING] + 1;
			}
			List<StationInfo> list2 = (
				from x in game.GameDatabase.GetStationForSystem(systemID)
				where game.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).ParentID == asteroid.ID
				select x).ToList<StationInfo>();
			foreach (StationInfo current in list2)
			{
				int? parentID = game.GameDatabase.GetOrbitalObjectInfo(current.OrbitalObjectID).ParentID;
				if (parentID.HasValue && asteroid.ID == parentID)
				{
					if (current.DesignInfo.StationType == StationType.SCIENCE)
					{
						Dictionary<StationType, int> dictionary5;
						(dictionary5 = dictionary)[StationType.SCIENCE] = dictionary5[StationType.SCIENCE] - 1;
						break;
					}
					if (current.DesignInfo.StationType == StationType.MINING)
					{
						Dictionary<StationType, int> dictionary6;
						(dictionary6 = dictionary)[StationType.MINING] = dictionary6[StationType.MINING] - 1;
					}
				}
			}
			foreach (KeyValuePair<StationType, int> current2 in dictionary)
			{
				if (current2.Value <= 0)
				{
					list.Remove(current2.Key);
				}
			}
			if (!game.GetPlayerObject(playerId).Faction.CanUseGate() && !game.GameDatabase.PlayerHasTech(playerId, "DRV_Casting"))
			{
				list.Remove(StationType.GATE);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_Xeno-Colloquy") && (game.GetPlayerObject(playerId).Faction.Name != "zuul" || !game.GameDatabase.PlayerHasTech(playerId, "POL_Tribute_Systems")))
			{
				list.Remove(StationType.DIPLOMATIC);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_FTL_Economics") && game.GetPlayerObject(playerId).Faction.Name != "zuul" && !game.GameDatabase.GetStratModifier<bool>(StratModifiers.EnableTrade, playerId))
			{
				list.Remove(StationType.CIVILIAN);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "BRD_Stealthed_Structures"))
			{
				list.Remove(StationType.DEFENCE);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "IND_Mega-Strip_Mining"))
			{
				list.Remove(StationType.MINING);
			}
			return list;
		}
		public static List<StationType> GetAvailSlotTypesForPlanet(GameSession game, int systemID, int planetID, int playerId)
		{
			Dictionary<StationType, int> dictionary = new Dictionary<StationType, int>();
			List<StationType> list = new List<StationType>();
			dictionary.Add(StationType.CIVILIAN, 0);
			list.Add(StationType.CIVILIAN);
			dictionary.Add(StationType.DEFENCE, 0);
			list.Add(StationType.DEFENCE);
			dictionary.Add(StationType.DIPLOMATIC, 0);
			list.Add(StationType.DIPLOMATIC);
			dictionary.Add(StationType.GATE, 0);
			list.Add(StationType.GATE);
			dictionary.Add(StationType.MINING, 0);
			list.Add(StationType.MINING);
			dictionary.Add(StationType.NAVAL, 0);
			list.Add(StationType.NAVAL);
			dictionary.Add(StationType.SCIENCE, 0);
			list.Add(StationType.SCIENCE);
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(planetID);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetID);
			List<StationInfo> list2 = (
				from x in game.GameDatabase.GetStationForSystem(systemID)
				where game.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).ParentID == planetID
				select x).ToList<StationInfo>();
			if (planetInfo.Type == "barren")
			{
				Dictionary<StationType, int> dictionary2;
				(dictionary2 = dictionary)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] + 1;
				Dictionary<StationType, int> dictionary3;
				(dictionary3 = dictionary)[StationType.SCIENCE] = dictionary3[StationType.SCIENCE] + 1;
				Dictionary<StationType, int> dictionary4;
				(dictionary4 = dictionary)[StationType.NAVAL] = dictionary4[StationType.NAVAL] + 1;
				Dictionary<StationType, int> dictionary5;
				(dictionary5 = dictionary)[StationType.MINING] = dictionary5[StationType.MINING] + 2;
			}
			else
			{
				if (StarSystem.IsColonizablePlanetType(planetInfo.Type))
				{
					if ((colonyInfoForPlanet != null && (colonyInfoForPlanet.PlayerID == playerId || colonyInfoForPlanet.IsIndependentColony(game))) || (game.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowDeepSpaceConstruction, playerId) && colonyInfoForPlanet == null))
					{
						Dictionary<StationType, int> dictionary6;
						(dictionary6 = dictionary)[StationType.SCIENCE] = dictionary6[StationType.SCIENCE] + 4;
						Dictionary<StationType, int> dictionary7;
						(dictionary7 = dictionary)[StationType.NAVAL] = dictionary7[StationType.NAVAL] + 4;
						Dictionary<StationType, int> dictionary8;
						(dictionary8 = dictionary)[StationType.GATE] = dictionary8[StationType.GATE] + 4;
					}
				}
				else
				{
					if (planetInfo.Type == "gaseous")
					{
						Dictionary<StationType, int> dictionary9;
						(dictionary9 = dictionary)[StationType.SCIENCE] = dictionary9[StationType.SCIENCE] + 2;
						Dictionary<StationType, int> dictionary10;
						(dictionary10 = dictionary)[StationType.NAVAL] = dictionary10[StationType.NAVAL] + 2;
						Dictionary<StationType, int> dictionary11;
						(dictionary11 = dictionary)[StationType.GATE] = dictionary11[StationType.GATE] + 2;
					}
				}
			}
			if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == playerId)
			{
				Dictionary<StationType, int> dictionary12;
				(dictionary12 = dictionary)[StationType.DIPLOMATIC] = dictionary12[StationType.DIPLOMATIC] + 4;
				Dictionary<StationType, int> dictionary13;
				(dictionary13 = dictionary)[StationType.CIVILIAN] = dictionary13[StationType.CIVILIAN] + 4;
				Dictionary<StationType, int> dictionary14;
				(dictionary14 = dictionary)[StationType.DEFENCE] = dictionary14[StationType.DEFENCE] + 4;
			}
			foreach (StationInfo current in list2)
			{
				int? parentID = game.GameDatabase.GetOrbitalObjectInfo(current.OrbitalObjectID).ParentID;
				if (parentID.HasValue && !(parentID != planetID))
				{
					if (current.DesignInfo.StationType == StationType.DEFENCE)
					{
						Dictionary<StationType, int> dictionary15;
						(dictionary15 = dictionary)[StationType.DEFENCE] = dictionary15[StationType.DEFENCE] - 1;
					}
					else
					{
						if (current.DesignInfo.StationType == StationType.MINING)
						{
							Dictionary<StationType, int> dictionary16;
							(dictionary16 = dictionary)[StationType.MINING] = dictionary16[StationType.MINING] - 1;
						}
						else
						{
							if (current.DesignInfo.StationType == StationType.CIVILIAN || current.DesignInfo.StationType == StationType.DIPLOMATIC)
							{
								Dictionary<StationType, int> dictionary17;
								(dictionary17 = dictionary)[StationType.CIVILIAN] = dictionary17[StationType.CIVILIAN] - 1;
								Dictionary<StationType, int> dictionary18;
								(dictionary18 = dictionary)[StationType.DIPLOMATIC] = dictionary18[StationType.DIPLOMATIC] - 1;
								Dictionary<StationType, int> dictionary19;
								(dictionary19 = dictionary)[StationType.NAVAL] = dictionary19[StationType.NAVAL] - 1;
								Dictionary<StationType, int> dictionary20;
								(dictionary20 = dictionary)[StationType.SCIENCE] = dictionary20[StationType.SCIENCE] - 1;
								Dictionary<StationType, int> dictionary21;
								(dictionary21 = dictionary)[StationType.GATE] = dictionary21[StationType.GATE] - 1;
							}
							else
							{
								if (current.DesignInfo.StationType == StationType.SCIENCE || current.DesignInfo.StationType == StationType.NAVAL || current.DesignInfo.StationType == StationType.GATE)
								{
									bool flag = false;
									if (parentID.HasValue)
									{
										if (colonyInfoForPlanet != null && colonyInfoForPlanet.OrbitalObjectID == parentID)
										{
											Dictionary<StationType, int> dictionary22;
											(dictionary22 = dictionary)[StationType.DIPLOMATIC] = dictionary22[StationType.DIPLOMATIC] - 1;
											Dictionary<StationType, int> dictionary23;
											(dictionary23 = dictionary)[StationType.CIVILIAN] = dictionary23[StationType.CIVILIAN] - 1;
											Dictionary<StationType, int> dictionary24;
											(dictionary24 = dictionary)[StationType.NAVAL] = dictionary24[StationType.NAVAL] - 1;
											Dictionary<StationType, int> dictionary25;
											(dictionary25 = dictionary)[StationType.SCIENCE] = dictionary25[StationType.SCIENCE] - 1;
											Dictionary<StationType, int> dictionary26;
											(dictionary26 = dictionary)[StationType.GATE] = dictionary26[StationType.GATE] - 1;
											break;
										}
										if (!flag)
										{
											Dictionary<StationType, int> dictionary27;
											(dictionary27 = dictionary)[StationType.SCIENCE] = dictionary27[StationType.SCIENCE] - 1;
											Dictionary<StationType, int> dictionary28;
											(dictionary28 = dictionary)[StationType.NAVAL] = dictionary28[StationType.NAVAL] - 1;
											if (game.GameDatabase.GetPlanetInfo(parentID.Value).Type != "barren")
											{
												Dictionary<StationType, int> dictionary29;
												(dictionary29 = dictionary)[StationType.GATE] = dictionary29[StationType.GATE] - 1;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			List<StationType> list3 = new List<StationType>(list);
			foreach (StationType current2 in list3)
			{
				StationInfo stationForSystemPlayerAndType = game.GameDatabase.GetStationForSystemPlayerAndType(systemID, playerId, current2);
				if (stationForSystemPlayerAndType != null && stationForSystemPlayerAndType.DesignInfo.StationType != StationType.DEFENCE && stationForSystemPlayerAndType.DesignInfo.StationType != StationType.MINING)
				{
					dictionary[current2] = 0;
				}
			}
			foreach (KeyValuePair<StationType, int> current3 in dictionary)
			{
				if (current3.Value <= 0)
				{
					list.Remove(current3.Key);
				}
			}
			if (!game.GetPlayerObject(playerId).Faction.CanUseGate())
			{
				list.Remove(StationType.GATE);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_Xeno-Colloquy") && (game.GetPlayerObject(playerId).Faction.Name != "zuul" || !game.GameDatabase.PlayerHasTech(playerId, "POL_Tribute_Systems")))
			{
				list.Remove(StationType.DIPLOMATIC);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_FTL_Economics") && game.GetPlayerObject(playerId).Faction.Name != "zuul" && !game.GameDatabase.GetStratModifier<bool>(StratModifiers.EnableTrade, playerId))
			{
				list.Remove(StationType.CIVILIAN);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "BRD_Stealthed_Structures"))
			{
				list.Remove(StationType.DEFENCE);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "IND_Mega-Strip_Mining"))
			{
				list.Remove(StationType.MINING);
			}
			return list;
		}
		public static List<StationType> GetSystemCanSupportStations(GameSession game, int systemID, int playerId)
		{
			Dictionary<StationType, int> dictionary = new Dictionary<StationType, int>();
			List<StationType> list = new List<StationType>();
			dictionary.Add(StationType.CIVILIAN, 0);
			list.Add(StationType.CIVILIAN);
			dictionary.Add(StationType.DEFENCE, 0);
			list.Add(StationType.DEFENCE);
			dictionary.Add(StationType.DIPLOMATIC, 0);
			list.Add(StationType.DIPLOMATIC);
			dictionary.Add(StationType.GATE, 0);
			list.Add(StationType.GATE);
			dictionary.Add(StationType.MINING, 0);
			list.Add(StationType.MINING);
			dictionary.Add(StationType.NAVAL, 0);
			list.Add(StationType.NAVAL);
			dictionary.Add(StationType.SCIENCE, 0);
			list.Add(StationType.SCIENCE);
			IList<PlanetInfo> list2 = game.GameDatabase.GetStarSystemPlanetInfos(systemID).ToList<PlanetInfo>();
			IList<AsteroidBeltInfo> list3 = game.GameDatabase.GetStarSystemAsteroidBeltInfos(systemID).ToList<AsteroidBeltInfo>();
			List<StationInfo> list4 = game.GameDatabase.GetStationForSystem(systemID).ToList<StationInfo>();
			List<ColonyInfo> list5 = game.GameDatabase.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>();
			List<MissionInfo> list6 = (
				from x in game.GameDatabase.GetMissionsBySystemDest(systemID)
				where game.GameDatabase.GetFleetInfo(x.FleetID).PlayerID == playerId && x.Type == MissionType.CONSTRUCT_STN
				select x).ToList<MissionInfo>();
			foreach (PlanetInfo current in list2)
			{
				ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(current.ID);
				if (current.Type == "barren")
				{
					Dictionary<StationType, int> dictionary2;
					(dictionary2 = dictionary)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] + 1;
					Dictionary<StationType, int> dictionary3;
					(dictionary3 = dictionary)[StationType.SCIENCE] = dictionary3[StationType.SCIENCE] + 1;
					Dictionary<StationType, int> dictionary4;
					(dictionary4 = dictionary)[StationType.NAVAL] = dictionary4[StationType.NAVAL] + 1;
					Dictionary<StationType, int> dictionary5;
					(dictionary5 = dictionary)[StationType.MINING] = dictionary5[StationType.MINING] + 2;
				}
				else
				{
					if (StarSystem.IsColonizablePlanetType(current.Type))
					{
						if ((colonyInfoForPlanet != null && (colonyInfoForPlanet.PlayerID == playerId || colonyInfoForPlanet.IsIndependentColony(game))) || (game.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowDeepSpaceConstruction, playerId) && colonyInfoForPlanet == null))
						{
							Dictionary<StationType, int> dictionary6;
							(dictionary6 = dictionary)[StationType.SCIENCE] = dictionary6[StationType.SCIENCE] + 4;
							Dictionary<StationType, int> dictionary7;
							(dictionary7 = dictionary)[StationType.NAVAL] = dictionary7[StationType.NAVAL] + 4;
							Dictionary<StationType, int> dictionary8;
							(dictionary8 = dictionary)[StationType.GATE] = dictionary8[StationType.GATE] + 4;
						}
					}
					else
					{
						if (current.Type == "gaseous")
						{
							Dictionary<StationType, int> dictionary9;
							(dictionary9 = dictionary)[StationType.SCIENCE] = dictionary9[StationType.SCIENCE] + 2;
							Dictionary<StationType, int> dictionary10;
							(dictionary10 = dictionary)[StationType.NAVAL] = dictionary10[StationType.NAVAL] + 2;
							Dictionary<StationType, int> dictionary11;
							(dictionary11 = dictionary)[StationType.GATE] = dictionary11[StationType.GATE] + 2;
						}
					}
				}
				if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == playerId)
				{
					Dictionary<StationType, int> dictionary12;
					(dictionary12 = dictionary)[StationType.DIPLOMATIC] = dictionary12[StationType.DIPLOMATIC] + 4;
					Dictionary<StationType, int> dictionary13;
					(dictionary13 = dictionary)[StationType.CIVILIAN] = dictionary13[StationType.CIVILIAN] + 4;
					Dictionary<StationType, int> dictionary14;
					(dictionary14 = dictionary)[StationType.DEFENCE] = dictionary14[StationType.DEFENCE] + 4;
				}
			}
			foreach (AsteroidBeltInfo current2 in list3)
			{
				Dictionary<StationType, int> dictionary15;
				(dictionary15 = dictionary)[StationType.DEFENCE] = dictionary15[StationType.DEFENCE] + game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(current2.ID).Count<LargeAsteroidInfo>() * 2;
				Dictionary<StationType, int> dictionary16;
				(dictionary16 = dictionary)[StationType.SCIENCE] = dictionary16[StationType.SCIENCE] + game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(current2.ID).Count<LargeAsteroidInfo>();
				Dictionary<StationType, int> dictionary17;
				(dictionary17 = dictionary)[StationType.MINING] = dictionary17[StationType.MINING] + game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(current2.ID).Count<LargeAsteroidInfo>();
			}
			foreach (StationInfo current3 in list4)
			{
				if (current3.DesignInfo.StationType == StationType.DEFENCE)
				{
					Dictionary<StationType, int> dictionary18;
					(dictionary18 = dictionary)[StationType.DEFENCE] = dictionary18[StationType.DEFENCE] - 1;
				}
				else
				{
					if (current3.DesignInfo.StationType == StationType.MINING)
					{
						Dictionary<StationType, int> dictionary19;
						(dictionary19 = dictionary)[StationType.MINING] = dictionary19[StationType.MINING] - 1;
					}
					else
					{
						if (current3.DesignInfo.StationType == StationType.CIVILIAN || current3.DesignInfo.StationType == StationType.DIPLOMATIC)
						{
							Dictionary<StationType, int> dictionary20;
							(dictionary20 = dictionary)[StationType.CIVILIAN] = dictionary20[StationType.CIVILIAN] - 1;
							Dictionary<StationType, int> dictionary21;
							(dictionary21 = dictionary)[StationType.DIPLOMATIC] = dictionary21[StationType.DIPLOMATIC] - 1;
							Dictionary<StationType, int> dictionary22;
							(dictionary22 = dictionary)[StationType.NAVAL] = dictionary22[StationType.NAVAL] - 1;
							Dictionary<StationType, int> dictionary23;
							(dictionary23 = dictionary)[StationType.SCIENCE] = dictionary23[StationType.SCIENCE] - 1;
							Dictionary<StationType, int> dictionary24;
							(dictionary24 = dictionary)[StationType.GATE] = dictionary24[StationType.GATE] - 1;
						}
						else
						{
							if (current3.DesignInfo.StationType == StationType.GATE && current3.PlayerID == playerId)
							{
								dictionary[StationType.GATE] = 0;
							}
							else
							{
								if (current3.DesignInfo.StationType == StationType.SCIENCE || current3.DesignInfo.StationType == StationType.NAVAL || current3.DesignInfo.StationType == StationType.GATE)
								{
									bool flag = false;
									int? parentID = game.GameDatabase.GetOrbitalObjectInfo(current3.OrbitalObjectID).ParentID;
									if (parentID.HasValue)
									{
										using (List<ColonyInfo>.Enumerator enumerator4 = list5.GetEnumerator())
										{
											while (enumerator4.MoveNext())
											{
												if (enumerator4.Current.OrbitalObjectID == parentID)
												{
													Dictionary<StationType, int> dictionary2;
													(dictionary2 = dictionary)[StationType.DIPLOMATIC] = dictionary2[StationType.DIPLOMATIC] - 1;
													(dictionary2 = dictionary)[StationType.CIVILIAN] = dictionary2[StationType.CIVILIAN] - 1;
													(dictionary2 = dictionary)[StationType.NAVAL] = dictionary2[StationType.NAVAL] - 1;
													(dictionary2 = dictionary)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
													(dictionary2 = dictionary)[StationType.GATE] = dictionary2[StationType.GATE] - 1;
													flag = true;
													break;
												}
											}
										}
										if (!flag)
										{
											foreach (AsteroidBeltInfo current4 in list3)
											{
												IEnumerable<LargeAsteroidInfo> largeAsteroidsInAsteroidBelt = game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(current4.ID);
												using (IEnumerator<LargeAsteroidInfo> enumerator5 = largeAsteroidsInAsteroidBelt.GetEnumerator())
												{
													while (enumerator5.MoveNext())
													{
														if (enumerator5.Current.ID == parentID && current3.DesignInfo.StationType == StationType.SCIENCE)
														{
															Dictionary<StationType, int> dictionary2;
															(dictionary2 = dictionary)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
															flag = true;
															break;
														}
													}
												}
											}
											if (!flag)
											{
												Dictionary<StationType, int> dictionary2;
												(dictionary2 = dictionary)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
												(dictionary2 = dictionary)[StationType.NAVAL] = dictionary2[StationType.NAVAL] - 1;
												if (game.GameDatabase.GetPlanetInfo(parentID.Value) != null && game.GameDatabase.GetPlanetInfo(parentID.Value).Type != "barren")
												{
													(dictionary2 = dictionary)[StationType.GATE] = dictionary2[StationType.GATE] - 1;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			foreach (MissionInfo current5 in list6)
			{
				if (current5.StationType.HasValue)
				{
					if (current5.StationType.Value == 7)
					{
						Dictionary<StationType, int> dictionary2;
						(dictionary2 = dictionary)[StationType.DEFENCE] = dictionary2[StationType.DEFENCE] - 1;
					}
					else
					{
						if (current5.StationType.Value == 6)
						{
							Dictionary<StationType, int> dictionary2;
							(dictionary2 = dictionary)[StationType.MINING] = dictionary2[StationType.MINING] - 1;
						}
						else
						{
							if (current5.StationType.Value == 3 || current5.StationType.Value == 4)
							{
								Dictionary<StationType, int> dictionary2;
								(dictionary2 = dictionary)[StationType.CIVILIAN] = dictionary2[StationType.CIVILIAN] - 1;
								(dictionary2 = dictionary)[StationType.DIPLOMATIC] = dictionary2[StationType.DIPLOMATIC] - 1;
								(dictionary2 = dictionary)[StationType.NAVAL] = dictionary2[StationType.NAVAL] - 1;
								(dictionary2 = dictionary)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
								(dictionary2 = dictionary)[StationType.GATE] = dictionary2[StationType.GATE] - 1;
							}
							else
							{
								if (current5.StationType.Value == 5)
								{
									dictionary[StationType.GATE] = 0;
								}
								else
								{
									if (current5.StationType.Value == 2 || current5.StationType.Value == 1 || current5.StationType.Value == 5)
									{
										bool flag2 = false;
										int targetOrbitalObjectID = current5.TargetOrbitalObjectID;
										foreach (ColonyInfo current6 in list5)
										{
											if (current6.OrbitalObjectID == targetOrbitalObjectID)
											{
												Dictionary<StationType, int> dictionary2;
												(dictionary2 = dictionary)[StationType.DIPLOMATIC] = dictionary2[StationType.DIPLOMATIC] - 1;
												(dictionary2 = dictionary)[StationType.CIVILIAN] = dictionary2[StationType.CIVILIAN] - 1;
												(dictionary2 = dictionary)[StationType.NAVAL] = dictionary2[StationType.NAVAL] - 1;
												(dictionary2 = dictionary)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
												(dictionary2 = dictionary)[StationType.GATE] = dictionary2[StationType.GATE] - 1;
												flag2 = true;
												break;
											}
										}
										if (!flag2)
										{
											foreach (AsteroidBeltInfo current7 in list3)
											{
												IEnumerable<LargeAsteroidInfo> largeAsteroidsInAsteroidBelt2 = game.GameDatabase.GetLargeAsteroidsInAsteroidBelt(current7.ID);
												foreach (LargeAsteroidInfo current8 in largeAsteroidsInAsteroidBelt2)
												{
													if (current8.ID == targetOrbitalObjectID && current5.StationType.Value == 2)
													{
														Dictionary<StationType, int> dictionary2;
														(dictionary2 = dictionary)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
														flag2 = true;
														break;
													}
												}
											}
											if (!flag2)
											{
												Dictionary<StationType, int> dictionary2;
												(dictionary2 = dictionary)[StationType.SCIENCE] = dictionary2[StationType.SCIENCE] - 1;
												(dictionary2 = dictionary)[StationType.NAVAL] = dictionary2[StationType.NAVAL] - 1;
												if (game.GameDatabase.GetPlanetInfo(targetOrbitalObjectID) != null && game.GameDatabase.GetPlanetInfo(targetOrbitalObjectID).Type != "barren")
												{
													(dictionary2 = dictionary)[StationType.GATE] = dictionary2[StationType.GATE] - 1;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			int num = game.GameDatabase.GetNumberMaxStationsSupportedBySystem(game, systemID, playerId);
			num -= (
				from x in game.GameDatabase.GetStationForSystemAndPlayer(systemID, playerId)
				where game.GameDatabase.GetStationRequiresSupport(x.DesignInfo.StationType)
				select x).Count<StationInfo>();
			foreach (MissionInfo current9 in list6)
			{
				if (current9.StationType.HasValue && (game.GameDatabase.GetStationRequiresSupport((StationType)current9.StationType.Value) || (game.GameDatabase.GetStationRequiresSupport((StationType)current9.StationType.Value) && num <= 0)))
				{
					dictionary[(StationType)current9.StationType.Value] = 0;
					num--;
				}
			}
			List<StationType> list7 = new List<StationType>(list);
			foreach (StationType current10 in list7)
			{
				if ((game.GameDatabase.GetStationForSystemPlayerAndType(systemID, playerId, current10) != null || num <= 0) && game.GameDatabase.GetStationRequiresSupport(current10))
				{
					dictionary[current10] = 0;
				}
			}
			foreach (KeyValuePair<StationType, int> current11 in dictionary)
			{
				if (current11.Value <= 0)
				{
					list.Remove(current11.Key);
				}
			}
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(playerId).FactionID);
			if (!faction.CanUseGate() || !game.GameDatabase.PlayerHasTech(playerId, "DRV_Gate_Stations"))
			{
				list.Remove(StationType.GATE);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_Xeno-Colloquy") && (faction.Name != "zuul" || !game.GameDatabase.PlayerHasTech(playerId, "POL_Tribute_Systems")))
			{
				list.Remove(StationType.DIPLOMATIC);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "POL_FTL_Economics") && faction.Name != "zuul" && !game.GameDatabase.GetStratModifier<bool>(StratModifiers.EnableTrade, playerId))
			{
				list.Remove(StationType.CIVILIAN);
			}
			if (!game.GameDatabase.PlayerHasTech(playerId, "BRD_Stealthed_Structures"))
			{
				list.Remove(StationType.DEFENCE);
			}
			if (!Player.CanBuildMiningStations(game.GameDatabase, playerId))
			{
				list.Remove(StationType.MINING);
			}
			return list;
		}
		public static StationTypeFlags GetSupportedStationTypesForPlanet(GameDatabase db, PlanetInfo planet)
		{
			if (planet.Type == "gaseous")
			{
				return StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.GATE;
			}
			if (planet.Type == "barren")
			{
				return StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.MINING | StationTypeFlags.DEFENCE;
			}
			if (db.GetColonyInfoForPlanet(planet.ID) != null)
			{
				return StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.CIVILIAN | StationTypeFlags.DIPLOMATIC | StationTypeFlags.GATE | StationTypeFlags.DEFENCE;
			}
			return StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.GATE;
		}
		protected List<SlotData> GetSlotsForPlanet(StellarBody body, bool forceWithColony = false)
		{
			List<SlotData> list = new List<SlotData>();
			if (body.Parameters.BodyType == "gaseous")
			{
				for (int i = 0; i < 2; i++)
				{
					list.Add(new SlotData
					{
						OccupantID = 0,
						Parent = body.ObjectID,
						ParentDBID = body.PlanetInfo.ID,
						SupportedTypes = StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.GATE
					});
				}
			}
			else
			{
				if (body.Parameters.BodyType == "barren")
				{
					for (int j = 0; j < 2; j++)
					{
						list.Add(new SlotData
						{
							OccupantID = 0,
							Parent = body.ObjectID,
							ParentDBID = body.PlanetInfo.ID,
							SupportedTypes = StationTypeFlags.DEFENCE
						});
					}
					for (int k = 0; k < 2; k++)
					{
						list.Add(new SlotData
						{
							OccupantID = 0,
							Parent = body.ObjectID,
							ParentDBID = body.PlanetInfo.ID,
							SupportedTypes = StationTypeFlags.MINING
						});
					}
					list.Add(new SlotData
					{
						OccupantID = 0,
						Parent = body.ObjectID,
						ParentDBID = body.PlanetInfo.ID,
						SupportedTypes = StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE
					});
				}
				else
				{
					if (base.App.GameDatabase.GetColonyInfoForPlanet(body.PlanetInfo.ID) != null || forceWithColony)
					{
						for (int l = 0; l < 4; l++)
						{
							list.Add(new SlotData
							{
								OccupantID = 0,
								Parent = body.ObjectID,
								ParentDBID = body.PlanetInfo.ID,
								SupportedTypes = StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.CIVILIAN | StationTypeFlags.DIPLOMATIC | StationTypeFlags.GATE | StationTypeFlags.DEFENCE
							});
						}
					}
					else
					{
						for (int m = 0; m < 4; m++)
						{
							list.Add(new SlotData
							{
								OccupantID = 0,
								Parent = body.ObjectID,
								ParentDBID = body.PlanetInfo.ID,
								SupportedTypes = StationTypeFlags.NAVAL | StationTypeFlags.SCIENCE | StationTypeFlags.GATE
							});
						}
					}
				}
			}
			return list;
		}
		protected List<SlotData> GetSlotsForAsteroid(LargeAsteroid body)
		{
			List<SlotData> list = new List<SlotData>();
			for (int i = 0; i < 2; i++)
			{
				list.Add(new SlotData
				{
					OccupantID = 0,
					Parent = body.ObjectID,
					ParentDBID = body.ID,
					SupportedTypes = StationTypeFlags.DEFENCE
				});
			}
			list.Add(new SlotData
			{
				OccupantID = 0,
				Parent = body.ObjectID,
				ParentDBID = body.ID,
				SupportedTypes = StationTypeFlags.MINING
			});
			list.Add(new SlotData
			{
				OccupantID = 0,
				Parent = body.ObjectID,
				ParentDBID = body.ID,
				SupportedTypes = StationTypeFlags.SCIENCE
			});
			return list;
		}
		protected override GameObjectStatus OnCheckStatus()
		{
			return this._crits.CheckStatus();
		}
		public static StarModel CreateStar(App game, Vector3 origin, StellarClass sclass, string name, float scale, bool isInCombat)
		{
			float num = StarHelper.CalcRadius(sclass.Size);
			bool impostorEnabled = true;
			return new StarModel(game, StarHelper.GetDisplayParams(sclass).AssetPath, CommonCombatState.ApplyOriginShift(origin, Vector3.Zero) * scale, num * scale, isInCombat, DefaultStarModelParameters.ImposterMaterial, DefaultStarModelParameters.ImposterSpriteScale * 0.2f, DefaultStarModelParameters.ImposterRange, StarHelper.CalcModelColor(sclass).Xyz, impostorEnabled, name);
		}
		public static StarModel CreateStar(App game, Vector3 origin, StarSystemInfo starInfo, float scale, bool isInCombat)
		{
			return StarSystem.CreateStar(game, origin, StellarClass.Parse(starInfo.StellarClass), starInfo.Name, scale, isInCombat);
		}
		public StarModel CreateStar(GameObjectSet gameObjects, Vector3 origin, StarSystemInfo starInfo, bool isInCombat)
		{
			StarModel starModel = StarSystem.CreateStar(gameObjects.App, origin, StellarClass.Parse(starInfo.StellarClass), starInfo.Name, this._scale, isInCombat);
			gameObjects.Add(starModel);
			starModel.StarSystemDatabaseID = starInfo.ID;
			return starModel;
		}
		public static AsteroidBelt CreateAsteroidBelt(App game, Vector3 origin, AsteroidBeltInfo planetInfo, Matrix world, float scale)
		{
			float length = (world.Position - origin).Length;
			return new AsteroidBelt(game, 0, origin, length - 2500f, length + 2500f, -500f, 500f, 2000);
		}
		public static LargeAsteroid CreateLargeAsteroid(App game, Vector3 origin, LargeAsteroidInfo largeAsteroid, Matrix world, float scale)
		{
			return new LargeAsteroid(game, world.Position)
			{
				WorldTransform = world,
				ID = largeAsteroid.ID
			};
		}
		public static StellarBody CreatePlanet(GameSession game, Vector3 origin, PlanetInfo planetInfo, Matrix world, float scale, bool isInCombat, StarSystem.TerrestrialPlanetQuality quality = StarSystem.TerrestrialPlanetQuality.High)
		{
			if (StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
			{
				return StarSystem.CreateTerrestrialPlanet(game, origin, planetInfo, world, scale, isInCombat, quality);
			}
			if (planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Gaseous)
			{
				return StarSystem.CreateTerrestrialPlanet(game, origin, planetInfo, world, scale, isInCombat, quality);
			}
			if (planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Barren)
			{
				return StarSystem.CreateTerrestrialPlanet(game, origin, planetInfo, world, scale, isInCombat, quality);
			}
			return StarSystem.CreateTerrestrialPlanet(game, origin, planetInfo, world, scale, isInCombat, quality);
		}
		public StellarBody CreatePlanet(GameSession game, GameObjectSet gameObjects, Vector3 origin, PlanetInfo planetInfo, Matrix world, bool isInCombat)
		{
			StellarBody stellarBody = StarSystem.CreatePlanet(game, origin, planetInfo, world, this._scale, isInCombat, StarSystem.TerrestrialPlanetQuality.High);
			gameObjects.Add(stellarBody);
			return stellarBody;
		}
		public AsteroidBelt CreateAsteroidBelt(App game, Vector3 origin, AsteroidBeltInfo asteroidBeltInfo, Matrix world)
		{
			return StarSystem.CreateAsteroidBelt(game, origin, asteroidBeltInfo, world, this._scale);
		}
		public LargeAsteroid CreateLargeAsteroid(App game, Vector3 origin, LargeAsteroidInfo largeAsteroid, Matrix world)
		{
			return StarSystem.CreateLargeAsteroid(game, origin, largeAsteroid, world, this._scale);
		}
		private static StellarBody CreateTerrestrialPlanet(GameSession game, Vector3 origin, PlanetInfo planetInfo, Matrix world, float scale, bool isInCombat, StarSystem.TerrestrialPlanetQuality quality = StarSystem.TerrestrialPlanetQuality.High)
		{
			Vector3 position = CommonCombatState.ApplyOriginShift(origin, world.Position) * scale;
			StellarBody.Params stellarBodyParams = game.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(game, planetInfo.ID);
			stellarBodyParams.SurfaceMaterial = "planet_earth2";
			stellarBodyParams.Position = position;
			stellarBodyParams.Radius *= scale;
			stellarBodyParams.IsInCombat = isInCombat;
			stellarBodyParams.TextureSize = (int)quality;
			IEnumerable<ColonyFactionInfo> civilianPopulations = game.GameDatabase.GetCivilianPopulations(planetInfo.ID);
			stellarBodyParams.Civilians = (
				from civilian in civilianPopulations
				select new StellarBody.PlanetCivilianData
				{
					Faction = game.GameDatabase.GetFactionName(civilian.FactionID),
					Population = civilian.CivilianPop
				}).ToArray<StellarBody.PlanetCivilianData>();
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
			stellarBodyParams.ImperialPopulation = ((colonyInfoForPlanet != null) ? colonyInfoForPlanet.ImperialPop : 0.0);
			stellarBodyParams.Infrastructure = planetInfo.Infrastructure;
			stellarBodyParams.Suitability = planetInfo.Suitability;
			stellarBodyParams.OrbitalID = planetInfo.ID;
			stellarBodyParams.BodyName = game.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).Name;
			StellarBody stellarBody = StellarBody.Create(game.App, stellarBodyParams);
			stellarBody.Population = stellarBodyParams.ImperialPopulation;
			stellarBody.PlanetInfo = planetInfo;
			return stellarBody;
		}
		public void Dispose()
		{
			this._crits.Dispose();
			this._slots.Dispose();
			base.App.ReleaseObject(this);
		}
	}
}
