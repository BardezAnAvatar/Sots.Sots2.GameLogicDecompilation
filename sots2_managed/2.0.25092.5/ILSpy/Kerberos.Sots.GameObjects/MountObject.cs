using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameObjects
{
	internal class MountObject : AutoGameObject
	{
		public class WeaponModels
		{
			public WeaponModelPaths WeaponModelPath;
			public WeaponModelPaths SubWeaponModelPath;
			public WeaponModelPaths SecondaryWeaponModelPath;
			public WeaponModelPaths SecondarySubWeaponModelPath;
			public void FillOutModelFilesWithWeapon(LogicalWeapon weapon, Faction faction, IEnumerable<LogicalWeapon> weapons)
			{
				this.WeaponModelPath = LogicalWeapon.GetWeaponModelPaths(weapon, faction);
				LogicalWeapon weapon2 = (weapon != null) ? weapon.GetSubWeapon(weapons) : null;
				LogicalWeapon logicalWeapon = (weapon != null) ? weapon.GetSecondaryWeapon(weapons) : null;
				LogicalWeapon weapon3 = (logicalWeapon != null) ? logicalWeapon.GetSubWeapon(weapons) : null;
				this.SubWeaponModelPath = LogicalWeapon.GetWeaponModelPaths(weapon2, faction);
				this.SecondaryWeaponModelPath = LogicalWeapon.GetWeaponModelPaths(logicalWeapon, faction);
				this.SecondarySubWeaponModelPath = LogicalWeapon.GetWeaponModelPaths(weapon3, faction);
			}
			public void FillOutModelFilesWithWeapon(LogicalWeapon weapon, Faction faction, string preferredMount, IEnumerable<LogicalWeapon> weapons)
			{
				this.WeaponModelPath = Ship.GetWeaponModelPathsWithFixAssetNameForDLC(weapon, faction, preferredMount);
				LogicalWeapon weapon2 = (weapon != null) ? weapon.GetSubWeapon(weapons) : null;
				LogicalWeapon logicalWeapon = (weapon != null) ? weapon.GetSecondaryWeapon(weapons) : null;
				LogicalWeapon weapon3 = (logicalWeapon != null) ? logicalWeapon.GetSubWeapon(weapons) : null;
				this.SubWeaponModelPath = Ship.GetWeaponModelPathsWithFixAssetNameForDLC(weapon2, faction, preferredMount);
				this.SecondaryWeaponModelPath = Ship.GetWeaponModelPathsWithFixAssetNameForDLC(logicalWeapon, faction, preferredMount);
				this.SecondarySubWeaponModelPath = Ship.GetWeaponModelPathsWithFixAssetNameForDLC(weapon3, faction, preferredMount);
			}
		}
		private int _parentID;
		private string _nodeName;
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
		public string NodeName
		{
			get
			{
				return this._nodeName;
			}
			set
			{
				this._nodeName = value;
			}
		}
	}
}
