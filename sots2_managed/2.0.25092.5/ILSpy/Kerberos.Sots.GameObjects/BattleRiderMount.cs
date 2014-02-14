using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_BATTLERIDERMOUNT)]
	internal class BattleRiderMount : MountObject
	{
		private int _designID;
		private int _squadIndex;
		private bool _isWeapon;
		private Section _section;
		private Module _module;
		private LogicalBank _bank;
		private string _icon;
		public int DesignID
		{
			get
			{
				return this._designID;
			}
			set
			{
				this._designID = value;
			}
		}
		public int SquadIndex
		{
			get
			{
				return this._squadIndex;
			}
			set
			{
				this._squadIndex = value;
			}
		}
		public bool IsWeapon
		{
			get
			{
				return this._isWeapon;
			}
			set
			{
				this._isWeapon = value;
			}
		}
		public Section AssignedSection
		{
			get
			{
				return this._section;
			}
			set
			{
				this._section = value;
			}
		}
		public Module AssignedModule
		{
			get
			{
				return this._module;
			}
			set
			{
				this._module = value;
			}
		}
		public LogicalBank WeaponBank
		{
			get
			{
				return this._bank;
			}
			set
			{
				this._bank = value;
			}
		}
		public string BankIcon
		{
			get
			{
				return this._icon;
			}
			set
			{
				this._icon = value;
			}
		}
		public override void Dispose()
		{
			base.Dispose();
			this._bank = null;
			this._module = null;
			this._section = null;
		}
		public static bool CanBattleRiderConnect(WeaponEnums.TurretClasses mountType, BattleRiderTypes brct, ShipClass sc)
		{
			switch (mountType)
			{
			case WeaponEnums.TurretClasses.Biomissile:
				return brct == BattleRiderTypes.biomissile;
			case WeaponEnums.TurretClasses.Drone:
				return brct == BattleRiderTypes.drone;
			case WeaponEnums.TurretClasses.AssaultShuttle:
				return brct == BattleRiderTypes.assaultshuttle;
			case WeaponEnums.TurretClasses.DestroyerRider:
				return brct.IsBattleRiderType() && sc == ShipClass.BattleRider;
			case WeaponEnums.TurretClasses.CruiserRider:
				return brct.IsControllableBattleRider() && sc == ShipClass.Cruiser;
			case WeaponEnums.TurretClasses.DreadnoughtRider:
				return brct.IsControllableBattleRider() && sc == ShipClass.Dreadnought;
			case WeaponEnums.TurretClasses.BoardingPod:
				return brct == BattleRiderTypes.boardingpod;
			case WeaponEnums.TurretClasses.EscapePod:
				return brct == BattleRiderTypes.escapepod;
			}
			return false;
		}
	}
}
