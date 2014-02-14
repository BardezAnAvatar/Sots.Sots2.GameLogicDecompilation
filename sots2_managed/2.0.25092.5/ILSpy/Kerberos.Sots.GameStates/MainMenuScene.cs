using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class MainMenuScene
	{
		private GameObjectSet _set;
		private List<IGameObject> _postLoadedObjects;
		private OrbitCameraController _camera;
		private CombatInput _input;
		private Dictionary<string, Player> _players = new Dictionary<string, Player>
		{

			{
				"human",
				null
			},

			{
				"hiver",
				null
			},

			{
				"morrigi",
				null
			},

			{
				"tarkas",
				null
			},

			{
				"zuul",
				null
			},

			{
				"liir_zuul",
				null
			},

			{
				"loa",
				null
			}
		};
		private bool _ready;
		private bool _active;
		private Dictionary<string, CombatAI> _combatAIs = new Dictionary<string, CombatAI>();
		private App _app;
		private Random _rand = new Random();
		private StarSystem _starsystem;
		private DateTime _nextSwitchTime;
		public Vector3 CreateRandomShip(Vector3 off, string Faction = "", bool ForceDread = false)
		{
			Vector3 zero = Vector3.Zero;
			List<WeaponEnums.PayloadTypes> weaponTypes = new List<WeaponEnums.PayloadTypes>();
			weaponTypes.Add(WeaponEnums.PayloadTypes.Beam);
			weaponTypes.Add(WeaponEnums.PayloadTypes.Bolt);
			weaponTypes.Add(WeaponEnums.PayloadTypes.Emitter);
			weaponTypes.Add(WeaponEnums.PayloadTypes.Missile);
			weaponTypes.Add(WeaponEnums.PayloadTypes.Torpedo);
			IEnumerable<LogicalWeapon> preferredWeapons = 
				from x in this._app.AssetDatabase.Weapons
				where x.PayloadType == weaponTypes[new Random().Next(weaponTypes.Count<WeaponEnums.PayloadTypes>() - 1)]
				select x;
			KeyValuePair<string, Player> chosenPlayer = this._rand.Choose(this._players);
			if (Faction != string.Empty && this._players.ContainsKey(Faction))
			{
				chosenPlayer = this._players.FirstOrDefault((KeyValuePair<string, Player> x) => x.Key == Faction);
			}
			List<ShipSectionAsset> choices = new List<ShipSectionAsset>
			{
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\dn_cmd_assault.section", chosenPlayer.Key))
			};
			List<ShipSectionAsset> choices2 = new List<ShipSectionAsset>
			{
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\dn_mis_armor.section", chosenPlayer.Key))
			};
			List<ShipSectionAsset> choices3 = new List<ShipSectionAsset>
			{
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\dn_eng_fusion.section", chosenPlayer.Key))
			};
			List<ShipSectionAsset> choices4 = new List<ShipSectionAsset>
			{
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_cmd.section", chosenPlayer.Key)),
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_cmd_assault.section", chosenPlayer.Key)),
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_cmd_hammerhead.section", chosenPlayer.Key))
			};
			List<ShipSectionAsset> choices5 = new List<ShipSectionAsset>
			{
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_armor.section", chosenPlayer.Key)),
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_blazer.section", chosenPlayer.Key)),
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_cnc.section", chosenPlayer.Key)),
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_barrage.section", chosenPlayer.Key)),
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_projector.section", chosenPlayer.Key))
			};
			List<ShipSectionAsset> choices6 = new List<ShipSectionAsset>
			{
				this._app.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == string.Format("factions\\{0}\\sections\\cr_eng_fusion.section", chosenPlayer.Key))
			};
			ShipSectionAsset[] array = new ShipSectionAsset[3];
			if (this._rand.CoinToss(0.15) || ForceDread)
			{
				array[0] = this._rand.Choose(choices2);
				array[1] = this._rand.Choose(choices);
				array[2] = this._rand.Choose(choices3);
			}
			else
			{
				array[0] = this._rand.Choose(choices4);
				array[1] = this._rand.Choose(choices5);
				array[2] = this._rand.Choose(choices6);
			}
			CreateShipParams createShipParams = new CreateShipParams();
			createShipParams.player = chosenPlayer.Value;
			createShipParams.sections = array;
			createShipParams.turretHousings = this._app.AssetDatabase.TurretHousings;
			createShipParams.weapons = this._app.AssetDatabase.Weapons;
			createShipParams.preferredWeapons = preferredWeapons;
			createShipParams.psionics = this._app.AssetDatabase.Psionics;
			createShipParams.faction = this._app.AssetDatabase.Factions.First((Faction x) => x.Name == chosenPlayer.Key);
			Ship ship = Ship.CreateShip(this._app, createShipParams);
			ship.Position = off;
			this._set.Add(ship);
			return off;
		}
		public void Enter(App app)
		{
			this._app = app;
			if (this._app.GameDatabase == null)
			{
				this._app.NewGame();
			}
			app.Game.SetLocalPlayer(app.GetPlayer(1));
			this._set = new GameObjectSet(app);
			this._postLoadedObjects = new List<IGameObject>();
			Sky value = new Sky(app, SkyUsage.InSystem, new Random().Next());
			this._set.Add(value);
			if (ScriptHost.AllowConsole)
			{
				this._input = this._set.Add<CombatInput>(new object[0]);
			}
			this._camera = this._set.Add<OrbitCameraController>(new object[0]);
			this._camera.SetAttractMode(true);
			this._camera.TargetPosition = new Vector3(500000f, 0f, 0f);
			this._camera.MinDistance = 1f;
			this._camera.MaxDistance = 11000f;
			this._camera.DesiredDistance = 11000f;
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-2f);
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(45f);
			int systemId = 0;
			IEnumerable<HomeworldInfo> homeworlds = this._app.GameDatabase.GetHomeworlds();
			HomeworldInfo homeworldInfo = homeworlds.FirstOrDefault((HomeworldInfo x) => x.PlayerID == app.LocalPlayer.ID);
			if (homeworldInfo != null)
			{
				systemId = homeworldInfo.SystemID;
			}
			else
			{
				if (homeworlds.Count<HomeworldInfo>() > 0)
				{
					systemId = homeworlds.ElementAt(new Random().NextInclusive(0, homeworlds.Count<HomeworldInfo>() - 1)).SystemID;
				}
			}
			this._starsystem = new StarSystem(this._app, 1f, systemId, new Vector3(0f, 0f, 0f), false, null, true, 0, false, true);
			this._set.Add(this._starsystem);
			this._starsystem.PostSetProp("InputEnabled", false);
			this._starsystem.PostSetProp("RenderSuroundingItems", false);
			Vector3 vector = default(Vector3);
			float num = 10000f;
			IEnumerable<PlanetInfo> planetInfosOrbitingStar = this._app.GameDatabase.GetPlanetInfosOrbitingStar(systemId);
			bool flag = false;
			foreach (PlanetInfo current in planetInfosOrbitingStar)
			{
				if (current != null)
				{
					ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(current.ID);
					if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._app.LocalPlayer.ID)
					{
						vector = this._app.GameDatabase.GetOrbitalTransform(current.ID).Position;
						num = StarSystemVars.Instance.SizeToRadius(current.Size);
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				PlanetInfo[] array = planetInfosOrbitingStar.ToArray<PlanetInfo>();
				if (array.Length > 0)
				{
					PlanetInfo planetInfo = array[new Random().Next(array.Length)];
					vector = this._app.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position;
					num = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
				}
			}
			this._camera.DesiredYaw = -(float)Math.Atan2((double)vector.Z, (double)vector.X);
			this._camera.TargetPosition = vector;
			Matrix matrix = Matrix.CreateRotationYPR(this._camera.DesiredYaw, 0f, 0f);
			Vector3[] shuffledPlayerColors = Player.GetShuffledPlayerColors(this._rand);
			foreach (string current2 in this._players.Keys.ToList<string>())
			{
				this._players[current2] = new Player(app, app.Game, new PlayerInfo
				{
					FactionID = app.GameDatabase.GetFactionIdFromName(current2),
					AvatarAssetPath = string.Empty,
					BadgeAssetPath = app.AssetDatabase.GetRandomBadgeTexture(current2, this._rand),
					PrimaryColor = shuffledPlayerColors[0],
					SecondaryColor = new Vector3(this._rand.NextSingle(), this._rand.NextSingle(), this._rand.NextSingle())
				}, Player.ClientTypes.AI);
				this._set.Add(this._players[current2]);
			}
			Vector3 vector2 = Vector3.Zero;
			Vector3.Cross(vector, new Vector3(0f, 1f, 0f)).Normalize();
			float num2 = 500f;
			int num3 = 4;
			float num4 = num2 * (float)num3;
			Vector3 vector3 = new Vector3(-num4, 0f, -num4);
			Vector3 v = vector + -matrix.Forward * (num + 2000f + num4);
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < 81; i++)
			{
				int num5 = i % 5;
				int num6 = i / 5;
				Vector3 vector4 = new Vector3(vector3.X + (float)num6 * num2, 0f, vector3.Z + (float)num5 * num2);
				vector4 += v;
				list.Add(vector4);
			}
			List<Vector3> list2 = new List<Vector3>();
			foreach (Vector3 current3 in list)
			{
				if (this.PositionCollidesWithObject(current3, 400f))
				{
					list2.Add(current3);
				}
			}
			foreach (Vector3 current4 in list2)
			{
				list.Remove(current4);
			}
			int num7 = this._rand.NextInclusive(6, 12);
			List<int> list3 = new List<int>();
			for (int j = 0; j < num7; j++)
			{
				int num8 = 0;
				bool flag2 = true;
				int num9 = 0;
				while (flag2 && num9 < list.Count)
				{
					num8 = this._rand.NextInclusive(0, Math.Max(list.Count - 1, 0));
					flag2 = list3.Contains(num8);
					if (list3.Count == list.Count)
					{
						break;
					}
					num9++;
				}
				Vector3 off = (list.Count > 0) ? list[num8] : vector;
				if (j < 3)
				{
					vector2 += this.CreateRandomShip(off, "loa", j == 0);
				}
				else
				{
					vector2 += this.CreateRandomShip(off, "", false);
				}
				if (!list3.Contains(num8))
				{
					list3.Add(num8);
				}
			}
			if (num7 > 0)
			{
				vector2 /= (float)num7;
			}
		}
		private bool PositionCollidesWithObject(Vector3 pos, float safeRadius)
		{
			foreach (IGameObject current in this._starsystem.Crits.Objects)
			{
				bool flag = false;
				float num = 0f;
				Vector3 v = Vector3.Zero;
				if (current is StellarBody)
				{
					num = StarSystemVars.Instance.SizeToRadius((current as StellarBody).PlanetInfo.Size) + 2000f;
					v = (current as StellarBody).Parameters.Position;
					flag = true;
				}
				else
				{
					if (current is Ship)
					{
						num = 1000f;
						v = (current as Ship).Position;
						flag = true;
					}
				}
				if (flag)
				{
					float num2 = safeRadius + num;
					if ((pos - v).LengthSquared < num2 * num2)
					{
						return true;
					}
				}
			}
			return false;
		}
		public void Activate()
		{
			this._active = true;
		}
		public bool IsReady()
		{
			return this._set.IsReady();
		}
		public void Update()
		{
			if (this._active && !this._ready && this._set.IsReady())
			{
				this._ready = true;
				this._set.Activate();
				foreach (string current in this._players.Keys)
				{
					this._combatAIs.Add(current, new CombatAI(this._app, this._players[current], false, this._starsystem, null, true));
				}
			}
			if (this._ready)
			{
				List<IGameObject> list = new List<IGameObject>();
				foreach (IGameObject current2 in this._postLoadedObjects)
				{
					if (current2.ObjectStatus == GameObjectStatus.Ready)
					{
						if (current2 is IActive)
						{
							(current2 as IActive).Active = true;
						}
						this._set.Add(current2);
						list.Add(current2);
					}
				}
				foreach (IGameObject current3 in list)
				{
					this._postLoadedObjects.Remove(current3);
				}
				DateTime now = DateTime.Now;
				if (now >= this._nextSwitchTime)
				{
					this._nextSwitchTime = now + TimeSpan.FromSeconds(5.0);
				}
				List<IGameObject> combatGameObjects = CombatAI.GetCombatGameObjects(this._set);
				foreach (CombatAI current4 in this._combatAIs.Values)
				{
					current4.Update(combatGameObjects);
				}
			}
		}
		public void Exit()
		{
			foreach (CombatAI current in this._combatAIs.Values)
			{
				if (current != null)
				{
					current.Shutdown();
				}
			}
			this._combatAIs.Clear();
			foreach (IGameObject current2 in 
				from x in this._set.Objects
				where x is Ship
				select x)
			{
				current2.PostSetProp("CombatHasEnded", new object[0]);
			}
			this._set.Dispose();
		}
		public void AddObject(ScriptMessageReader data)
		{
			InteropGameObjectType interopGameObjectType = (InteropGameObjectType)data.ReadInteger();
			IGameObject gameObject = null;
			if (gameObject != null)
			{
				this._postLoadedObjects.Add(gameObject);
			}
		}
		public void RemoveObject(ScriptMessageReader data)
		{
			int id = data.ReadInteger();
			IGameObject gameObject = this._set.Objects.FirstOrDefault((IGameObject x) => x.ObjectID == id && x is IDisposable);
			if (gameObject != null)
			{
				(gameObject as IDisposable).Dispose();
				this._set.Remove(gameObject);
				return;
			}
			IGameObject gameObject2 = this._app.GetGameObject(id);
			if (gameObject2 != null)
			{
				this._app.ReleaseObject(gameObject2);
			}
		}
		public void RemoveObjects(ScriptMessageReader data)
		{
			int id;
			for (id = data.ReadInteger(); id != 0; id = data.ReadInteger())
			{
				IGameObject gameObject = this._set.Objects.FirstOrDefault((IGameObject x) => x.ObjectID == id && x is IDisposable);
				if (gameObject != null)
				{
					(gameObject as IDisposable).Dispose();
					this._set.Remove(gameObject);
				}
				else
				{
					IGameObject gameObject2 = this._app.GetGameObject(id);
					if (gameObject2 != null)
					{
						this._app.ReleaseObject(gameObject2);
					}
				}
			}
		}
	}
}
