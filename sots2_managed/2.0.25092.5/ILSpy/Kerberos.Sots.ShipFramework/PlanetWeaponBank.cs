using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.ShipFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_PLANETWEAPONBANK)]
	internal class PlanetWeaponBank : WeaponBank
	{
		public LogicalWeapon SubWeapon
		{
			get;
			private set;
		}
		public string WeaponModel
		{
			get;
			private set;
		}
		public string SubWeaponModel
		{
			get;
			private set;
		}
		public float ThinkTime
		{
			get;
			private set;
		}
		public int NumLaunchers
		{
			get;
			private set;
		}
		public PlanetWeaponBank(App game, IGameObject owner, LogicalBank bank, Module module, LogicalWeapon weapon, int weaponLevel, LogicalWeapon subWeapon, WeaponEnums.TurretClasses tClass, string model, string subModel, float thinkTime, int numLaunchers) : base(game, owner, bank, module, weapon, weaponLevel, 0, 0, 0, weapon.DefaultWeaponSize, tClass)
		{
			this.SubWeapon = subWeapon;
			this.WeaponModel = model;
			this.SubWeaponModel = subModel;
			this.ThinkTime = thinkTime;
			this.NumLaunchers = numLaunchers;
		}
		public override void AddExistingObject(App game)
		{
			game.AddExistingObject(this, new List<object>
			{
				(base.Weapon != null) ? base.Weapon.GameObject.ObjectID : 0,
				base.Owner.ObjectID,
				base.WeaponLevel,
				base.TargetFilter,
				base.FireMode,
				(int)base.WeaponSize,
				(this.SubWeapon != null) ? this.SubWeapon.GameObject.ObjectID : 0,
				this.WeaponModel,
				this.SubWeaponModel,
				this.ThinkTime,
				this.NumLaunchers
			}.ToArray());
		}
	}
}
