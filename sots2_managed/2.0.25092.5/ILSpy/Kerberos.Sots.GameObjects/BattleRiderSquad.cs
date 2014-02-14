using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_BATTLERIDERSQUAD)]
	internal class BattleRiderSquad : GameObject, IDisposable
	{
		private int _parentID;
		private Section _section;
		private Module _module;
		private List<BattleRiderMount> _mounts;
		private int _numRiders;
		public int ParentID
		{
			get
			{
				return this._parentID;
			}
			set
			{
				this._parentID = value;
			}
		}
		public Section AttachedSection
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
		public Module AttachedModule
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
		public List<BattleRiderMount> Mounts
		{
			get
			{
				return this._mounts;
			}
			set
			{
				this._mounts = value;
			}
		}
		public int NumRiders
		{
			get
			{
				return this._numRiders;
			}
			set
			{
				this._numRiders = value;
			}
		}
		public BattleRiderSquad()
		{
			this._mounts = new List<BattleRiderMount>();
		}
		public void Dispose()
		{
			if (this._mounts != null)
			{
				this._mounts.Clear();
			}
			this._module = null;
			this._section = null;
		}
		public static int GetMinRiderSlotsPerSquad(WeaponEnums.TurretClasses mountType, ShipClass carrierClass)
		{
			int result = 0;
			switch (carrierClass)
			{
			case ShipClass.Cruiser:
			case ShipClass.Dreadnought:
				result = 3;
				break;
			case ShipClass.Leviathan:
				result = ((mountType != WeaponEnums.TurretClasses.DestroyerRider) ? 3 : 6);
				break;
			}
			return result;
		}
		public static int GetNumRidersPerSquad(WeaponEnums.TurretClasses mountType, ShipClass carrierClass, int totalMounts)
		{
			int val;
			switch (carrierClass)
			{
			case ShipClass.Cruiser:
			case ShipClass.Dreadnought:
				val = 3;
				break;
			case ShipClass.Leviathan:
				val = ((mountType != WeaponEnums.TurretClasses.DestroyerRider) ? 3 : 6);
				break;
			default:
				val = totalMounts;
				break;
			}
			if (WeaponEnums.IsWeaponBattleRider(mountType))
			{
				val = totalMounts;
			}
			return Math.Min(val, totalMounts);
		}
	}
}
