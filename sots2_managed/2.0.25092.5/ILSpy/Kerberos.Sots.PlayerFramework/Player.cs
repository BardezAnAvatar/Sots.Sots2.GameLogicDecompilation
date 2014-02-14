using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.PlayerFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_PLAYER)]
	internal class Player : GameObject, IDisposable
	{
		public enum ClientTypes
		{
			User,
			AI
		}
		public const double DefaultSuitabilityTolerance = 6.0;
		public const double MinIdealSuitability = 6.0;
		public const double MaxIdealSuitability = 14.0;
		private static bool _overridePlayerColors;
		private static Vector3 _overridePrimaryPlayerColor = Vector3.One;
		private static Vector3 _overrideSecondaryPlayerColor = Vector3.One;
		public static List<Vector3> DefaultPrimaryPlayerColors = new List<Vector3>
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0.7098f, 0f, 1f),
			new Vector3(1f, 0.4902f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 1f, 1f),
			new Vector3(0.79607f, 0.56078f, 0.40784f),
			new Vector3(0.10196f, 0.52941f, 0.45098f),
			new Vector3(2.2222f, 0.1765f, 0.5098f)
		};
		public static readonly List<Vector3> DefaultMPPrimaryPlayerColors = new List<Vector3>
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0.7098f, 0f, 1f),
			new Vector3(1f, 0.4902f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 1f, 1f),
			new Vector3(0.79607f, 0.56078f, 0.40784f),
			new Vector3(0.10196f, 0.52941f, 0.45098f),
			new Vector3(2.2222f, 0.1765f, 0.5098f)
		};
		public static readonly Vector3[] DefaultPrimaryTeamColors = new Vector3[]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(2.2222f, 0.1765f, 0.5098f),
			new Vector3(0.7098f, 0f, 1f),
			new Vector3(1f, 0.4902f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 1f, 1f),
			new Vector3(0.79607f, 0.56078f, 0.40784f),
			new Vector3(0.10196f, 0.52941f, 0.45098f)
		};
		private PlayerInfo _pi;
		private GameSession game;
		private CivilianRatios m_DesiredCivilianRatios;
		public int _techPointsAtStartOfTurn;
		private Faction _faction;
		private StrategicAI m_AI;
		private App app;
		public AIStance? Stance
		{
			get
			{
				if (this.GetAI() != null)
				{
					return this.GetAI().LastStance;
				}
				return null;
			}
		}
		public AITechStyles TechStyles
		{
			get
			{
				if (this.GetAI() != null)
				{
					return this.GetAI().TechStyles;
				}
				return null;
			}
		}
		public int SubfactionIndex
		{
			get
			{
				return this._pi.SubfactionIndex;
			}
		}
		public int ID
		{
			get
			{
				return this._pi.ID;
			}
		}
		public bool IsStandardPlayer
		{
			get
			{
				return this._pi.isStandardPlayer;
			}
		}
		public GameSession Game
		{
			get
			{
				return this.game;
			}
		}
		public PlayerInfo PlayerInfo
		{
			get
			{
				return this._pi;
			}
		}
		public Faction Faction
		{
			get
			{
				return this._faction;
			}
		}
		public static bool CanBuildMiningStations(GameDatabase db, int playerId)
		{
			return db.PlayerHasTech(playerId, "IND_Mega-Strip_Mining");
		}
		public static void OverridePlayerColors(Vector3 primary, Vector3 secondary)
		{
			Player._overridePlayerColors = true;
			Player._overridePrimaryPlayerColor = primary;
			Player._overrideSecondaryPlayerColor = secondary;
		}
		public static void RestorePlayerColors()
		{
			Player._overridePlayerColors = false;
		}
		public static Vector3[] GetShuffledPlayerColors(Random rng)
		{
			return Player.DefaultPrimaryPlayerColors.Shuffle(rng).ToArray<Vector3>();
		}
		public void SetEmpireColor(int index)
		{
			this.PostSetProp("Color1", new object[]
			{
				new Vector4(Player.DefaultPrimaryPlayerColors[index % Player.DefaultPrimaryPlayerColors.Count], 1f)
			});
		}
		public void SetPlayerColor(Vector3 value)
		{
			this.PostSetProp("Color2", new object[]
			{
				new Vector4(value, 1f)
			});
		}
		public void SetBadgeTexture(string texturePath)
		{
			this.PostSetProp("Badge", texturePath);
		}
		public string GetName()
		{
			return this._pi.Name;
		}
		public Player(App app, GameSession game, PlayerInfo pi, Player.ClientTypes clientType)
		{
			this.game = game;
			this.app = app;
			this._pi = pi;
			Vector3 vector = pi.PrimaryColor;
			Vector3 vector2 = pi.SecondaryColor;
			if (Player._overridePlayerColors)
			{
				vector = Player._overridePrimaryPlayerColor;
				vector2 = Player._overrideSecondaryPlayerColor;
			}
			app.AddExistingObject(this, new object[]
			{
				this.ID,
				vector,
				vector2,
				pi.BadgeAssetPath
			});
			if (game != null)
			{
				FactionInfo factionInfo = game.GameDatabase.GetFactionInfo(pi.FactionID);
				Faction faction = app.AssetDatabase.Factions.First((Faction x) => x.Name == factionInfo.Name);
				this.SetFaction(faction);
			}
			this.SetAI(clientType == Player.ClientTypes.AI);
		}
		public bool IsAI()
		{
			return this.m_AI != null;
		}
		public void SetAI(bool enabled)
		{
			if (enabled && this.game != null)
			{
				if (this._pi.ID != 0)
				{
					this.game.GameDatabase.InsertOrIgnoreAI(this._pi.ID, AIStance.EXPANDING);
					this.m_AI = new StrategicAI(this.game, this);
				}
				this.PostSetProp("SetUseAI", new object[]
				{
					true,
					(int)this._pi.AIDifficulty
				});
				return;
			}
			this.m_AI = null;
			this.PostSetProp("SetUseAI", new object[]
			{
				false,
				(int)this._pi.AIDifficulty
			});
		}
		public void ReplaceWithAI()
		{
			this.SetAI(true);
			if (this.m_AI != null)
			{
				this.m_AI.SetDropInActivationTurn(this.game.GameDatabase.GetTurnCount() + 2);
			}
		}
		public StrategicAI GetAI()
		{
			return this.m_AI;
		}
		public bool InstantDefeatMorrigiRelics()
		{
			return this._faction != null && this._faction.Name == "morrigi";
		}
		public CivilianRatios GetDesiredCivilianRatios()
		{
			return this.m_DesiredCivilianRatios;
		}
		public void SetDesiredCivilianRatios(CivilianRatios ratios)
		{
			this.m_DesiredCivilianRatios = ratios;
		}
		public void SetFaction(Faction value)
		{
			if (this._faction != value)
			{
				if (this._faction != null)
				{
					this._faction.ReleaseFactionReference(base.App);
				}
				this._faction = value;
				if (this._faction != null)
				{
					this._faction.AddFactionReference(base.App);
				}
				FactionObject factionObject = (this._faction != null) ? this._faction.FactionObj : null;
				this.PostSetProp("Faction", (factionObject != null) ? factionObject.ObjectID : 0);
			}
		}
		public static double GetSuitabilityTolerance()
		{
			return 6.0;
		}
		public void Dispose()
		{
			if (this._faction != null)
			{
				this._faction.ReleaseFactionReference(base.App);
			}
			this._faction = null;
			base.App.ReleaseObject(this);
		}
		public static int GetWeaponLevelFromTechs(LogicalWeapon weapon, List<PlayerTechInfo> techs)
		{
			if (weapon != null)
			{
				if (weapon.Traits.Any((WeaponEnums.WeaponTraits x) => x == WeaponEnums.WeaponTraits.Upgradable) && techs.Count != 0)
				{
					switch (weapon.PayloadType)
					{
					case WeaponEnums.PayloadTypes.Missile:
					case WeaponEnums.PayloadTypes.BattleRider:
					{
						PlayerTechInfo playerTechInfo = techs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "WAR_Anti-Matter_Warheads");
						PlayerTechInfo playerTechInfo2 = techs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "WAR_Reflex_Warheads");
						if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched)
						{
							return 3;
						}
						if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
						{
							return 2;
						}
						break;
					}
					}
					return 1;
				}
			}
			return 1;
		}
		public static int GetPsiResistanceFromTech(AssetDatabase ab, List<PlayerTechInfo> techs)
		{
			PlayerTechInfo playerTechInfo = techs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "CYB_PsiShield");
			if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
			{
				return ab.GetTechBonus<int>("CYB_PsiShield", "psiPower");
			}
			return 0;
		}
		public static bool HasNodeDriveTech(List<PlayerTechInfo> techs)
		{
			return techs.Any((PlayerTechInfo x) => x.State == TechStates.Researched && (x.TechFileID == "DRV_Node" || x.TechFileID == "DRV_Node_Focusing" || x.TechFileID == "DRV_Node_Pathing"));
		}
		public static bool HasWarpPulseTech(List<PlayerTechInfo> techs)
		{
			return techs.Any((PlayerTechInfo x) => x.State == TechStates.Researched && x.TechFileID == "DRV_Warp_Pulse");
		}
		public static float GetSubversionRange(AssetDatabase ab, List<PlayerTechInfo> techs, bool isLoa)
		{
			PlayerTechInfo playerTechInfo = techs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "PSI_Subversion");
			if (isLoa || (playerTechInfo != null && playerTechInfo.State == TechStates.Researched))
			{
				return ab.GetTechBonus<float>("PSI_Subversion", "range");
			}
			return 0f;
		}
		public static float GetPowerBonus(AssetDatabase ab, List<PlayerTechInfo> techs)
		{
			float num = 0f;
			if (techs.Any((PlayerTechInfo x) => x.State == TechStates.Researched && x.TechFileID == "CYB_InFldManip"))
			{
				num += ab.GetTechBonus<float>("CYB_InFldManip", "power");
			}
			return num;
		}
		public static float GetKineticDampeningValue(AssetDatabase ab, List<PlayerTechInfo> techs)
		{
			float num = 1f;
			if (techs.Any((PlayerTechInfo x) => x.State == TechStates.Researched && x.TechFileID == "NRG_Internal_Kinetic_Dampers"))
			{
				num += ab.GetTechBonus<float>("NRG_Internal_Kinetic_Dampers", "force");
			}
			return num;
		}
		public static float GetPDAccuracyBonus(AssetDatabase ab, List<PlayerTechInfo> techs)
		{
			float num = 0f;
			if (techs.Any((PlayerTechInfo x) => x.State == TechStates.Researched && x.TechFileID == "PSI_MechaEmpathy"))
			{
				num += ab.GetTechBonus<float>("PSI_MechaEmpathy", "pdaccuracy");
			}
			return num;
		}
		public static WeaponTechModifiers ObtainWeaponTechModifiers(AssetDatabase ab, WeaponEnums.TurretClasses tc, LogicalWeapon weapon, IEnumerable<PlayerTechInfo> playerTechs)
		{
			WeaponTechModifiers result = default(WeaponTechModifiers);
			result.DamageModifier = 0f;
			result.SpeedModifier = 0f;
			result.AccelModifier = 0f;
			result.MassModifier = 0f;
			result.ROFModifier = 1f;
			result.RangeModifier = 0f;
			result.SmartNanites = false;
			if (weapon == null || playerTechs == null || playerTechs.Count<PlayerTechInfo>() == 0)
			{
				return result;
			}
			if (tc != WeaponEnums.TurretClasses.Torpedo && weapon.PayloadType == WeaponEnums.PayloadTypes.Bolt)
			{
				PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "BAL_Neutronium_Rounds");
				PlayerTechInfo playerTechInfo2 = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "BAL_Acceleration_Amplification");
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				{
					result.DamageModifier += ab.GetTechBonus<float>(playerTechInfo.TechFileID, "damage");
					result.MassModifier += ab.GetTechBonus<float>(playerTechInfo.TechFileID, "mass");
				}
				if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched)
				{
					result.DamageModifier += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "damage");
					result.SpeedModifier += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "speed");
				}
			}
			if (weapon.PayloadType == WeaponEnums.PayloadTypes.Missile)
			{
				PlayerTechInfo playerTechInfo3 = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "WAR_MicroFusion_Drives");
				PlayerTechInfo playerTechInfo4 = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "NRG_Ionic_Thruster");
				if (playerTechInfo3 != null && playerTechInfo3.State == TechStates.Researched)
				{
					result.SpeedModifier += ab.GetTechBonus<float>(playerTechInfo3.TechFileID, "speed");
					result.RangeModifier += ab.GetTechBonus<float>(playerTechInfo3.TechFileID, "range");
				}
				if (playerTechInfo4 != null && playerTechInfo4.State == TechStates.Researched)
				{
					result.SpeedModifier += ab.GetTechBonus<float>(playerTechInfo4.TechFileID, "speed");
					result.AccelModifier += ab.GetTechBonus<float>(playerTechInfo4.TechFileID, "accel");
				}
			}
			if (weapon.PayloadType == WeaponEnums.PayloadTypes.Beam)
			{
				PlayerTechInfo playerTechInfo5 = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "NRG_Quantum_Capacitors");
				if (playerTechInfo5 != null && playerTechInfo5.State == TechStates.Researched)
				{
					result.ROFModifier += ab.GetTechBonus<float>(playerTechInfo5.TechFileID, "rateoffire");
				}
			}
			if (weapon.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic))
			{
				PlayerTechInfo playerTechInfo6 = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "BAL_VRF_Systems");
				if (playerTechInfo6 != null && playerTechInfo6.State == TechStates.Researched)
				{
					result.ROFModifier += ab.GetTechBonus<float>(playerTechInfo6.TechFileID, "rateoffire");
				}
			}
			if (weapon.Traits.Contains(WeaponEnums.WeaponTraits.Nanite))
			{
				PlayerTechInfo playerTechInfo7 = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "IND_Smart_Nanites");
				result.SmartNanites = (playerTechInfo7 != null && playerTechInfo7.State == TechStates.Researched);
			}
			return result;
		}
		public static ShipSpeedModifiers GetShipSpeedModifiers(AssetDatabase ab, Player player, RealShipClasses shipClass, IEnumerable<PlayerTechInfo> playerTechs, bool isDeepSpace)
		{
			ShipSpeedModifiers result = default(ShipSpeedModifiers);
			result.SpeedModifier = 1f;
			result.RotSpeedModifier = 1f;
			result.LinearAccelModifier = 1f;
			result.RotAccelModifier = 1f;
			if (player != null)
			{
				PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "NRG_Ionic_Thruster");
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				{
					float techBonus = ab.GetTechBonus<float>(playerTechInfo.TechFileID, "shiprot");
					float techBonus2 = ab.GetTechBonus<float>(playerTechInfo.TechFileID, "shipaccel");
					result.RotSpeedModifier += techBonus;
					result.LinearAccelModifier += techBonus2;
					result.RotAccelModifier += techBonus2;
				}
				PlayerTechInfo playerTechInfo2 = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "NRG_Small_Scale_Fusion");
				if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched && (shipClass == RealShipClasses.AssaultShuttle || shipClass == RealShipClasses.Drone || shipClass == RealShipClasses.Biomissile))
				{
					result.SpeedModifier += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "speed");
				}
				if (isDeepSpace && player.Faction.Name == "liir_zuul")
				{
					result.SpeedModifier += 0.2f;
					result.RotSpeedModifier += 0.2f;
					result.LinearAccelModifier += 0.2f;
					result.RotAccelModifier += 0.2f;
				}
			}
			return result;
		}
	}
}
