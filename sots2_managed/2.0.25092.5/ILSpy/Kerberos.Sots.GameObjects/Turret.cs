using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_TURRET)]
	internal class Turret : MountObject
	{
		public enum FiringEnum
		{
			NotFiring,
			Firing,
			Failed,
			Completed
		}
		public class TurretDescription
		{
			public LogicalWeapon Weapon;
			public LogicalMount Mount;
			public ShipInfo SInfo;
			public FleetInfo Fleet;
			public Ship Ship;
			public Section Section;
			public Module Module;
			public IGameObject ParentObject;
			public LogicalBank LogicalBank;
			public WeaponBank WeaponBank;
			public TurretBase TurretBase;
			public GenericCollisionShape TurretCollisionShape;
			public LogicalEffect DestroyedTurretEffect;
			public LogicalTurretHousing Housing;
			public WeaponTechModifiers TechModifiers;
			public string TurretModelName;
			public string BarrelModelName;
			public float CollisionShapeRadius;
			public MountObject.WeaponModels WeaponModels;
			public float MaxTurretHealth;
			public float TurretHealth;
			public int TurretIndex;
		}
		private LogicalWeapon _weapon;
		public LogicalWeapon Weapon
		{
			get
			{
				return this._weapon;
			}
			set
			{
				if (this._weapon != null)
				{
					throw new InvalidOperationException("Cannot change a turret's weapon once it has been set.");
				}
				this._weapon = value;
			}
		}
		private float CalcRateOfFire(List<SectionEnumerations.DesignAttribute> attributes)
		{
			if (attributes.Count == 0)
			{
				return 1f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Aces_And_Eights))
			{
				return 0.85f;
			}
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ol_Yellow_Streak))
			{
				return 1.1f;
			}
			return 1f;
		}
		private float CalcBallisticWeaponRangeModifier(List<SectionEnumerations.DesignAttribute> attributes, float value)
		{
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Sniper))
			{
				value *= 1.25f;
			}
			return value;
		}
		private float CalcAccuracyModifier(List<SectionEnumerations.DesignAttribute> attributes)
		{
			float num = 0f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Dead_Eye))
			{
				num = 0.1f;
			}
			return Math.Max(1f - num, 0f);
		}
		public Turret(App game, Turret.TurretDescription description)
		{
			List<SectionEnumerations.DesignAttribute> attributes = new List<SectionEnumerations.DesignAttribute>();
			if (description.SInfo != null)
			{
				attributes = game.GameDatabase.GetDesignAttributesForDesign(description.SInfo.DesignID).ToList<SectionEnumerations.DesignAttribute>();
			}
			List<object> list = new List<object>();
			list.Add(description.Weapon.GameObject.ObjectID);
			list.Add(description.Ship.ObjectID);
			list.Add(description.Section.ObjectID);
			list.Add((description.Module != null) ? description.Module.ObjectID : 0);
			list.Add(description.WeaponBank.ObjectID);
			list.Add(description.WeaponModels.WeaponModelPath.ModelPath);
			list.Add(description.WeaponModels.WeaponModelPath.DefaultModelPath);
			list.Add(description.WeaponModels.SubWeaponModelPath.ModelPath);
			list.Add(description.WeaponModels.SubWeaponModelPath.DefaultModelPath);
			list.Add(description.WeaponModels.SecondaryWeaponModelPath.ModelPath);
			list.Add(description.WeaponModels.SecondaryWeaponModelPath.DefaultModelPath);
			list.Add(description.WeaponModels.SecondarySubWeaponModelPath.ModelPath);
			list.Add(description.WeaponModels.SecondarySubWeaponModelPath.DefaultModelPath);
			list.Add(ScriptHost.AllowConsole);
			list.Add((description.TurretBase != null) ? description.TurretBase.ObjectID : 0);
			list.Add(description.CollisionShapeRadius);
			list.Add((description.TurretCollisionShape != null) ? description.TurretCollisionShape.ObjectID : 0);
			list.Add(description.TurretIndex);
			list.Add(description.MaxTurretHealth + description.Weapon.Health);
			list.Add(description.TurretHealth);
			list.Add(description.Housing.TrackSpeed + description.Weapon.TrackSpeedModifier);
			list.Add(this.CalcRateOfFire(attributes));
			list.Add(description.Weapon.CritHitBonus);
			list.Add(this.CalcBallisticWeaponRangeModifier(attributes, 1f));
			float num = 1f;
			float num2 = ((description.Ship.RealShipClass == RealShipClasses.Drone) ? 1.5f : 1f) * description.TechModifiers.ROFModifier;
			if (description.Ship.CombatAI == SectionEnumerations.CombatAiType.SwarmerQueen)
			{
				num2 *= 1.25f;
			}
			if (description.Fleet != null)
			{
				List<AdmiralInfo.TraitType> list2 = game.GameDatabase.GetAdmiralTraits(description.Fleet.AdmiralID).ToList<AdmiralInfo.TraitType>();
				if (list2.Contains(AdmiralInfo.TraitType.DrillSergeant))
				{
					num -= 0.1f;
				}
				if (description.Fleet.Type == FleetType.FL_NORMAL && description.Fleet.SupplyRemaining == 0f && (description.Weapon.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic) || description.Weapon.PayloadType == WeaponEnums.PayloadTypes.Missile))
				{
					num2 *= 0.2f;
				}
			}
			list.Add(num2);
			list.Add(description.Ship.AccuracyModifier * this.CalcAccuracyModifier(attributes) * num);
			list.Add(description.Ship.PDAccuracyModifier);
			list.Add(description.Weapon.MalfunctionPercent);
			list.Add(description.Weapon.MalfunctionDamage);
			list.Add(description.TechModifiers.DamageModifier);
			list.Add(description.TechModifiers.SpeedModifier);
			list.Add(description.TechModifiers.AccelModifier);
			list.Add(description.TechModifiers.MassModifier);
			list.Add(description.TechModifiers.RangeModifier);
			list.Add(description.TechModifiers.SmartNanites);
			list.Add(description.TurretModelName);
			list.Add(description.BarrelModelName);
			list.Add(description.DestroyedTurretEffect.Name);
			list.Add(description.Mount.NodeName);
			list.Add(description.Mount.FireAnimName);
			list.Add(description.Mount.ReloadAnimName);
			list.Add(description.Weapon.SolutionTolerance);
			list.Add(description.Mount.Yaw.Min);
			list.Add(description.Mount.Yaw.Max);
			list.Add(description.Mount.Pitch.Min);
			list.Add(description.Mount.Pitch.Max);
			list.Add(description.LogicalBank.TurretSize);
			list.Add(description.LogicalBank.TurretClass);
			game.AddExistingObject(this, list.ToArray());
			this._weapon = description.Weapon;
			base.ParentID = description.ParentObject.ObjectID;
			base.NodeName = description.Mount.NodeName;
			this.SetTag(description.Mount);
		}
		public override void Dispose()
		{
			base.Dispose();
			this._weapon = null;
		}
	}
}
