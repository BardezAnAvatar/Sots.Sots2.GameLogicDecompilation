using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalWeapon : IDisposable
	{
		private const string DEFAULT_WEAPON_PATH = "props\\Weapons\\";
		public int UniqueWeaponID;
		public string FileName;
		public string WeaponName;
		public WeaponEnums.WeaponSizes DefaultWeaponSize;
		public WeaponEnums.PayloadTypes PayloadType;
		public WeaponEnums.PlagueType PlagueType;
		public LogicalTurretClass[] TurretClasses;
		public WeaponRangeTable RangeTable;
		public float PopDamage;
		public float InfraDamage;
		public float TerraDamage;
		public int Cost;
		public float Range;
		public float Speed;
		public float Mass;
		public float Acceleration;
		public float RechargeTime;
		public float Duration;
		public float TimeToLive;
		public float BuildupTime;
		public float ShotDelay;
		public float VolleyPeriod;
		public float VolleyDeviation;
		public float Health;
		public float TrackSpeedModifier;
		public int NumVolleys;
		public int CritHitRolls;
		public int NumArcs;
		public int ArmorPiercingLevel;
		public int DisruptorValue;
		public int DrainValue;
		public float SolutionTolerance;
		public float DumbFireTime;
		public float MalfunctionDamage;
		public float MalfunctionPercent;
		public float CritHitBonus;
		public float Signature;
		public float RicochetModifier;
		public float ExplosiveMinEffectRange;
		public float ExplosiveMaxEffectRange;
		public float DetonationRange;
		public float MaxGravityForce;
		public float GravityAffectRange;
		public float ArcRange;
		public float EMPRange;
		public float EMPDuration;
		public float DOTDamage;
		public float BeamDamagePeriod;
		public LogicalEffect MuzzleEffect;
		public LogicalEffect BuildupEffect;
		public LogicalEffect BulletEffect;
		public LogicalEffect ImpactEffect;
		public LogicalEffect PlanetImpactEffect;
		public LogicalEffect RicochetEffect;
		public bool isMuzzleEffectLooping;
		public bool isBuildupEffectLooping;
		public bool isBulletEffectLooping;
		public bool isImpactEffectLooping;
		public bool isRicochetEffectLooping;
		public string MuzzleSound;
		public string BuildupSound;
		public string ImpactSound;
		public string PlanetImpactSound;
		public string ExpireSound;
		public string BulletSound;
		public string RicochetSound;
		public int Crew;
		public bool isCrewPerBank;
		public int Power;
		public bool isPowerPerBank;
		public int Supply;
		public bool isSupplyPerBank;
		public bool IsVisible;
		public string Model;
		public string IconSpriteName;
		public string IconTextureName;
		public string SubWeapon;
		public string SecondaryWeapon;
		public int NumSubWeapons;
		public WeaponEnums.SubmunitionBlastType SubMunitionBlastType;
		public float SubmunitionConeDeviation;
		public string Animation;
		public float AnimationDelay;
		public WeaponEnums.WeaponTraits[] Traits;
		public string[] CompatibleSections;
		public string[] DeployableSections;
		public string[] CompatibleFactions;
		public LogicalShield.ShieldType[] PassThroughShields;
		public Tech[] RequiredTechs;
		public DamagePattern[] DamagePattern;
		public MuzzleDescriptor MuzzleSize;
		public string DamageDecalMaterial;
		public float DamageDecalSize;
		private Weapon _weapon;
		private int _weaponRefCount;
		private readonly App _game;
		public string Name
		{
			get
			{
				return App.Localize("@" + this.WeaponName);
			}
		}
		public WeaponEnums.TurretClasses DefaultWeaponClass
		{
			get
			{
				return this.TurretClasses.First<LogicalTurretClass>().TurretClass;
			}
		}
		public Weapon GameObject
		{
			get
			{
				return this._weapon;
			}
		}
		public float GetRateOfFire()
		{
			if (this.RechargeTime < 1.401298E-45f)
			{
				return float.PositiveInfinity;
			}
			return 1f / this.RechargeTime;
		}
		public bool IsPDWeapon()
		{
			return this.DefaultWeaponSize == WeaponEnums.WeaponSizes.VeryLight;
		}
		public LogicalWeapon(App game)
		{
			this._game = game;
		}
		public LogicalTurretClass GetLogicalTurretClassForMount(WeaponEnums.WeaponSizes mountTurretSize, WeaponEnums.TurretClasses mountTurretClass)
		{
			return LogicalTurretClass.GetLogicalTurretClassForMount(this.TurretClasses, this.DefaultWeaponSize, this.DefaultWeaponClass, mountTurretSize, mountTurretClass);
		}
		public static IEnumerable<LogicalWeapon> EnumerateWeaponFits(string faction, string sectionName, IEnumerable<LogicalWeapon> weapons, WeaponEnums.WeaponSizes mountTurretSize, WeaponEnums.TurretClasses mountTurretClass)
		{
			foreach (LogicalWeapon current in weapons)
			{
				if (current.IsVisible && current.IsSectionCompatable(faction, sectionName) && LogicalTurretClass.GetLogicalTurretClassForMount(current.TurretClasses, current.DefaultWeaponSize, current.DefaultWeaponClass, mountTurretSize, mountTurretClass) != null)
				{
					yield return current;
				}
			}
			yield break;
		}
		public LogicalWeapon GetSecondaryWeapon(IEnumerable<LogicalWeapon> weapons)
		{
			return weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == this.SecondaryWeapon);
		}
		public LogicalWeapon GetSubWeapon(IEnumerable<LogicalWeapon> weapons)
		{
			return weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == this.SubWeapon);
		}
		public static WeaponModelPaths GetWeaponModelPaths(LogicalWeapon weapon, Faction faction)
		{
			WeaponModelPaths result = default(WeaponModelPaths);
			result.ModelPath = "";
			result.DefaultModelPath = "";
			if (weapon != null && !string.IsNullOrEmpty(weapon.Model))
			{
				result.ModelPath = faction.GetWeaponModelPath(weapon.Model);
				result.DefaultModelPath = "props\\Weapons\\" + weapon.Model;
			}
			return result;
		}
		public bool IsSectionCompatable(string faction, string sectionName)
		{
			return (this.CompatibleFactions.Length <= 0 || this.CompatibleFactions.Contains(faction)) && (this.CompatibleSections.Length == 0 || sectionName == "" || this.CompatibleSections.Contains(sectionName));
		}
		public void AddGameObjectReference()
		{
			this._weaponRefCount++;
			if (this._weaponRefCount == 1)
			{
				List<object> list = new List<object>();
				LogicalWeapon logicalWeapon = this._game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == this.SubWeapon);
				if (logicalWeapon != null)
				{
					logicalWeapon.AddGameObjectReference();
				}
				LogicalWeapon logicalWeapon2 = this._game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == this.SecondaryWeapon);
				if (logicalWeapon2 != null)
				{
					logicalWeapon2.AddGameObjectReference();
				}
				list.Add(this.UniqueWeaponID);
				list.Add((logicalWeapon != null) ? logicalWeapon.GameObject.ObjectID : 0);
				list.Add((logicalWeapon2 != null) ? logicalWeapon2.GameObject.ObjectID : 0);
				list.Add(string.IsNullOrEmpty(this.MuzzleEffect.Name) ? "effects\\placeholder.effect" : this.MuzzleEffect.Name);
				list.Add(this.isMuzzleEffectLooping);
				list.Add(string.IsNullOrEmpty(this.BuildupEffect.Name) ? string.Empty : this.BuildupEffect.Name);
				list.Add(this.isBuildupEffectLooping);
				list.Add(string.IsNullOrEmpty(this.BulletEffect.Name) ? "effects\\placeholder.effect" : this.BulletEffect.Name);
				list.Add(this.isBulletEffectLooping);
				list.Add(string.IsNullOrEmpty(this.ImpactEffect.Name) ? "effects\\placeholder.effect" : this.ImpactEffect.Name);
				list.Add(this.isImpactEffectLooping);
				list.Add(string.IsNullOrEmpty(this.PlanetImpactEffect.Name) ? string.Empty : this.ImpactEffect.Name);
				list.Add(string.IsNullOrEmpty(this.RicochetEffect.Name) ? "effects\\Weapons\\Ricochet_Impact.effect" : this.RicochetEffect.Name);
				list.Add(this.isRicochetEffectLooping);
				list.Add(this.MuzzleSound);
				list.Add(this.BuildupSound);
				list.Add(this.ImpactSound);
				list.Add(this.PlanetImpactSound);
				list.Add(this.ExpireSound);
				list.Add(this.BulletSound);
				list.Add(this.RicochetSound);
				list.Add(this.WeaponName);
				list.Add(this.IconSpriteName);
				list.Add(this.Name);
				list.Add((int)this.PayloadType);
				list.Add(this.DefaultWeaponSize);
				list.Add(this.PlagueType);
				list.Add(this.NumVolleys);
				list.Add(this.NumSubWeapons);
				list.Add((int)this.SubMunitionBlastType);
				list.Add(this.SubmunitionConeDeviation);
				list.Add(this.CritHitRolls);
				list.Add(this.NumArcs);
				list.AddRange(this.RangeTable.EnumerateScriptMessageParams());
				list.Add(this.PopDamage);
				list.Add(this.InfraDamage);
				list.Add(this.TerraDamage);
				list.Add(this.VolleyDeviation);
				list.Add(this.Speed);
				list.Add(this.Mass);
				list.Add(this.Acceleration);
				list.Add(this.Duration);
				list.Add(this.TimeToLive);
				list.Add(this.Health);
				list.Add(this.Range);
				list.Add(this.BuildupTime);
				list.Add(this.ShotDelay);
				list.Add(this.VolleyPeriod);
				list.Add(this.RechargeTime);
				list.Add(this.DumbFireTime);
				list.Add(this.CritHitBonus);
				list.Add(this.Signature);
				list.Add(this.RicochetModifier);
				list.Add(this.BeamDamagePeriod);
				list.Add(this.ExplosiveMinEffectRange);
				list.Add(this.ExplosiveMaxEffectRange);
				list.Add(this.DetonationRange);
				list.Add(this.MaxGravityForce);
				list.Add(this.GravityAffectRange);
				list.Add(this.ArcRange);
				list.Add(this.EMPRange);
				list.Add(this.EMPDuration);
				list.Add(this.DOTDamage);
				list.Add(this.Animation);
				list.Add(this.AnimationDelay);
				list.Add(this.MuzzleSize.MuzzleType ?? WeaponEnums.MuzzleShape.Rectangle.ToString());
				list.Add(this.MuzzleSize.Height);
				list.Add(this.MuzzleSize.Width);
				list.Add(this.Crew);
				list.Add(this.Power);
				list.Add(this.Supply);
				list.Add(this.isCrewPerBank);
				list.Add(this.isPowerPerBank);
				list.Add(this.isSupplyPerBank);
				list.Add(this.Traits.Length);
				WeaponEnums.WeaponTraits[] traits = this.Traits;
				for (int i = 0; i < traits.Length; i++)
				{
					WeaponEnums.WeaponTraits weaponTraits = traits[i];
					list.Add((int)weaponTraits);
				}
				list.Add(this.ArmorPiercingLevel);
				list.Add(this.DisruptorValue);
				list.Add(this.DrainValue);
				DamagePattern[] damagePattern = this.DamagePattern;
				for (int j = 0; j < damagePattern.Length; j++)
				{
					DamagePattern item = damagePattern[j];
					list.Add(item);
				}
				list.Add(this.DamageDecalMaterial);
				list.Add(this.DamageDecalSize);
				list.Add(this.PassThroughShields.Length);
				LogicalShield.ShieldType[] passThroughShields = this.PassThroughShields;
				for (int k = 0; k < passThroughShields.Length; k++)
				{
					LogicalShield.ShieldType shieldType = passThroughShields[k];
					list.Add(shieldType);
				}
				this._weapon = this._game.AddObject<Weapon>(list.ToArray());
			}
		}
		public void ReleaseGameObjectReference()
		{
			if (this._weaponRefCount == 0)
			{
				throw new InvalidOperationException("Weapon reference count already 0.");
			}
			this._weaponRefCount--;
			if (this._weaponRefCount == 0)
			{
				this._game.ReleaseObject(this._weapon);
				this._weapon = null;
			}
		}
		public void Dispose()
		{
			if (this._weapon != null)
			{
				this._game.ReleaseObject(this._weapon);
				this._weapon = null;
			}
		}
		public override string ToString()
		{
			return this.Name + "," + this.DefaultWeaponSize;
		}
	}
}
