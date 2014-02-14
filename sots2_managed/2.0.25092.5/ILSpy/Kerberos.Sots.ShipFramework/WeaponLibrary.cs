using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.ShipFramework
{
	internal static class WeaponLibrary
	{
		public static LogicalWeapon CreateLogicalWeaponFromFile(App app, string weaponFile, int uniqueWeaponID = -1)
		{
			Kerberos.Sots.Data.WeaponFramework.Weapon weapon = new Kerberos.Sots.Data.WeaponFramework.Weapon();
			WeaponXmlUtility.LoadWeaponFromXml(weaponFile, ref weapon);
			LogicalWeapon logicalWeapon = new LogicalWeapon(app);
			logicalWeapon.UniqueWeaponID = uniqueWeaponID;
			logicalWeapon.FileName = Path.Combine("weapons", Path.GetFileName(weaponFile));
			logicalWeapon.WeaponName = Path.GetFileNameWithoutExtension(weaponFile);
			logicalWeapon.PayloadType = ((!string.IsNullOrEmpty(weapon.PayloadType)) ? ((WeaponEnums.PayloadTypes)Enum.Parse(typeof(WeaponEnums.PayloadTypes), weapon.PayloadType)) : WeaponEnums.PayloadTypes.Bolt);
			logicalWeapon.PlagueType = ((!string.IsNullOrEmpty(weapon.PlagueType)) ? ((WeaponEnums.PlagueType)Enum.Parse(typeof(WeaponEnums.PlagueType), weapon.PlagueType)) : WeaponEnums.PlagueType.NONE);
			logicalWeapon.Duration = weapon.Duration;
			logicalWeapon.TimeToLive = weapon.TimeToLive;
			logicalWeapon.Model = weapon.Model;
			logicalWeapon.IsVisible = weapon.isVisible;
			logicalWeapon.SubWeapon = weapon.SubmunitionType;
			logicalWeapon.SecondaryWeapon = weapon.SubweaponType;
			logicalWeapon.NumSubWeapons = weapon.SubmunitionAmount;
			logicalWeapon.SubMunitionBlastType = ((!string.IsNullOrEmpty(weapon.SubmunitionBlastType)) ? ((WeaponEnums.SubmunitionBlastType)Enum.Parse(typeof(WeaponEnums.SubmunitionBlastType), weapon.SubmunitionBlastType)) : WeaponEnums.SubmunitionBlastType.Focus);
			logicalWeapon.SubmunitionConeDeviation = weapon.SubmunitionConeDeviation;
			logicalWeapon.CritHitRolls = Math.Max(weapon.CritHitRolls, 1);
			logicalWeapon.NumArcs = weapon.NumArcs;
			logicalWeapon.Cost = weapon.Cost;
			logicalWeapon.Range = weapon.EffectiveRanges.MaxRange;
			logicalWeapon.IconSpriteName = weapon.Icon;
			logicalWeapon.IconTextureName = Path.Combine("weapons\\Icons", logicalWeapon.IconSpriteName + ".bmp");
			logicalWeapon.RechargeTime = weapon.BaseRechargeTime;
			logicalWeapon.BuildupTime = weapon.BuildupDelay;
			logicalWeapon.ShotDelay = weapon.VolleyDelay;
			logicalWeapon.VolleyPeriod = weapon.VolleyPeriod;
			logicalWeapon.VolleyDeviation = weapon.BaseVolleyDeviation;
			logicalWeapon.Health = weapon.Health;
			logicalWeapon.TrackSpeedModifier = weapon.TrackSpeedModifier;
			logicalWeapon.NumVolleys = Math.Max(weapon.Volleys, 1);
			logicalWeapon.DefaultWeaponSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), weapon.WeaponSize);
			logicalWeapon.Speed = weapon.MuzzleSpeed;
			logicalWeapon.Mass = weapon.RoundMass;
			logicalWeapon.Acceleration = weapon.ShotAcceleration;
			logicalWeapon.SolutionTolerance = weapon.SolutionTolerance;
			logicalWeapon.DumbFireTime = weapon.DumbfireTime;
			logicalWeapon.CritHitBonus = weapon.CritHitBonus;
			logicalWeapon.Signature = weapon.Signature;
			logicalWeapon.RicochetModifier = weapon.BaseRicochetModifier;
			logicalWeapon.MalfunctionDamage = weapon.MalfunctionDamage;
			logicalWeapon.MalfunctionPercent = weapon.MalfunctionPercent;
			logicalWeapon.Crew = weapon.Crew;
			logicalWeapon.isCrewPerBank = weapon.isCrewPerBank;
			logicalWeapon.Power = weapon.Power;
			logicalWeapon.isPowerPerBank = weapon.isPowerPerBank;
			logicalWeapon.Supply = weapon.Supply;
			logicalWeapon.isSupplyPerBank = weapon.isSupplyPerBank;
			logicalWeapon.TurretClasses = weapon.TurretClasses.SelectMany((TurretClass x) => x.GetLogicalTurretClasses(false)).ToArray<LogicalTurretClass>();
			logicalWeapon.Traits = (
				from x in weapon.Attributes
				select (WeaponEnums.WeaponTraits)Enum.Parse(typeof(WeaponEnums.WeaponTraits), x.Name)).ToArray<WeaponEnums.WeaponTraits>();
			logicalWeapon.CompatibleSections = (
				from x in weapon.CompatibleSections
				select x.Name).ToArray<string>();
			logicalWeapon.DeployableSections = (
				from x in weapon.DeployableSections
				select x.Name).ToArray<string>();
			logicalWeapon.CompatibleFactions = (
				from x in weapon.CompatibleFactions
				select x.Name).ToArray<string>();
			logicalWeapon.RangeTable = new WeaponRangeTable();
			logicalWeapon.RangeTable.PointBlank.Range = weapon.EffectiveRanges.PbRange;
			logicalWeapon.RangeTable.PointBlank.Damage = weapon.EffectiveRanges.PbDamage;
			logicalWeapon.RangeTable.PointBlank.Deviation = weapon.EffectiveRanges.PbDeviation;
			logicalWeapon.RangeTable.Effective.Range = weapon.EffectiveRanges.EffectiveRange;
			logicalWeapon.RangeTable.Effective.Damage = weapon.EffectiveRanges.EffectiveDamage;
			logicalWeapon.RangeTable.Effective.Deviation = weapon.EffectiveRanges.EffectiveDeviation;
			logicalWeapon.RangeTable.Maximum.Range = weapon.EffectiveRanges.MaxRange;
			logicalWeapon.RangeTable.Maximum.Damage = weapon.EffectiveRanges.MaxDamage;
			logicalWeapon.RangeTable.Maximum.Deviation = weapon.EffectiveRanges.MaxDeviation;
			logicalWeapon.RangeTable.PlanetRange = weapon.EffectiveRanges.PlanetRange;
			logicalWeapon.PopDamage = weapon.PopDamage;
			logicalWeapon.InfraDamage = weapon.InfraDamage;
			logicalWeapon.TerraDamage = weapon.TerraDamage;
			logicalWeapon.MuzzleSize = weapon.MuzzleSize;
			logicalWeapon.Animation = (weapon.Animation ?? string.Empty);
			logicalWeapon.AnimationDelay = weapon.AnimationDelay;
			logicalWeapon.ExplosiveMinEffectRange = weapon.ExplosiveMinEffectRange;
			logicalWeapon.ExplosiveMaxEffectRange = weapon.ExplosiveMaxEffectRange;
			logicalWeapon.DetonationRange = weapon.DetonationRange;
			logicalWeapon.MaxGravityForce = weapon.MaxGravityForce;
			logicalWeapon.GravityAffectRange = weapon.GravityAffectRange;
			logicalWeapon.ArcRange = weapon.ArcRange;
			logicalWeapon.EMPRange = weapon.EMPRange;
			logicalWeapon.EMPDuration = weapon.EMPDuration;
			logicalWeapon.DOTDamage = weapon.DOT;
			logicalWeapon.BeamDamagePeriod = ((weapon.BeamDamagePeriod > 0f) ? weapon.BeamDamagePeriod : 0.5f);
			List<DamagePattern> list = new List<DamagePattern>();
			logicalWeapon.ArmorPiercingLevel = weapon.ArmorPiercingLevel;
			logicalWeapon.DisruptorValue = weapon.DisruptorValue;
			logicalWeapon.DrainValue = weapon.DrainValue;
			list.Add(new DamagePattern(weapon.PbGrid.Width, weapon.PbGrid.Height, weapon.PbGrid.CollisionX, weapon.PbGrid.CollisionY, weapon.PbGrid.Data));
			list.Add(new DamagePattern(weapon.EffectiveGrid.Width, weapon.EffectiveGrid.Height, weapon.EffectiveGrid.CollisionX, weapon.EffectiveGrid.CollisionY, weapon.EffectiveGrid.Data));
			list.Add(new DamagePattern(weapon.MaxGrid.Width, weapon.MaxGrid.Height, weapon.MaxGrid.CollisionX, weapon.MaxGrid.CollisionY, weapon.MaxGrid.Data));
			logicalWeapon.DamagePattern = list.ToArray();
			logicalWeapon.MuzzleEffect = new LogicalEffect
			{
				Name = weapon.MuzzleEffect
			};
			logicalWeapon.BuildupEffect = new LogicalEffect
			{
				Name = weapon.BuildupEffect
			};
			logicalWeapon.ImpactEffect = new LogicalEffect
			{
				Name = weapon.ImpactEffect
			};
			logicalWeapon.PlanetImpactEffect = new LogicalEffect
			{
				Name = weapon.PlanetImpactEffect
			};
			logicalWeapon.BulletEffect = new LogicalEffect
			{
				Name = weapon.BulletEffect
			};
			logicalWeapon.RicochetEffect = new LogicalEffect
			{
				Name = weapon.RicochetEffect
			};
			logicalWeapon.DamageDecalMaterial = weapon.DecalMaterial;
			logicalWeapon.DamageDecalSize = weapon.DecalSize;
			logicalWeapon.isMuzzleEffectLooping = weapon.isMuzzleEffectLooping;
			logicalWeapon.isBuildupEffectLooping = weapon.isBuildupEffectLooping;
			logicalWeapon.isImpactEffectLooping = weapon.isImpactEffectLooping;
			logicalWeapon.isBulletEffectLooping = weapon.isBulletEffectLooping;
			logicalWeapon.isRicochetEffectLooping = weapon.isRicochetEffectLooping;
			logicalWeapon.MuzzleSound = weapon.MuzzleSound;
			logicalWeapon.BuildupSound = weapon.BuildupSound;
			logicalWeapon.ImpactSound = weapon.ImpactSound;
			logicalWeapon.PlanetImpactSound = (weapon.PlanetImpactSound ?? string.Empty);
			logicalWeapon.ExpireSound = weapon.ExpireSound;
			logicalWeapon.BulletSound = weapon.BulletSound;
			logicalWeapon.RicochetSound = weapon.RicochetSound;
			logicalWeapon.RequiredTechs = weapon.RequiredTech.ToArray();
			logicalWeapon.PassThroughShields = (
				from x in weapon.PassThroughShields
				select (LogicalShield.ShieldType)Enum.Parse(typeof(LogicalShield.ShieldType), x.Name)).ToArray<LogicalShield.ShieldType>();
			return logicalWeapon;
		}
		public static IEnumerable<LogicalWeapon> Enumerate(App app)
		{
			string[] array = ScriptHost.FileSystem.FindFiles("weapons\\*.weapon");
			List<LogicalWeapon> list = new List<LogicalWeapon>();
			int num = 0;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array2[i];
				num++;
				try
				{
					LogicalWeapon item = WeaponLibrary.CreateLogicalWeaponFromFile(app, text, num);
					list.Add(item);
				}
				catch (Exception ex)
				{
					App.Log.Trace(string.Format("Weapon failed to load: {0} \r\n Exception: {1}", text, ex.Message), "data");
				}
			}
			return list;
		}
	}
}
