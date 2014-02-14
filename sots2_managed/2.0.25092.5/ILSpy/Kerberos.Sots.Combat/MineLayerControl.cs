using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class MineLayerControl : SpecWeaponControl
	{
		private Vector3 m_MaxPos;
		private Vector3 m_MinPos;
		private int m_WeaponID;
		private bool m_ForceOn;
		public bool ForceOn
		{
			get
			{
				return this.m_ForceOn;
			}
			set
			{
				this.m_ForceOn = value;
			}
		}
		public MineLayerControl(App game, CombatAI commanderAI, Ship ship, WeaponEnums.TurretClasses weaponType) : base(game, commanderAI, ship, weaponType)
		{
			this.m_ForceOn = false;
			WeaponBank weaponBank = ship.WeaponBanks.FirstOrDefault((WeaponBank x) => x.TurretClass == weaponType);
			if (weaponBank != null)
			{
				this.m_WeaponID = weaponBank.Weapon.UniqueWeaponID;
			}
			this.m_MaxPos = Vector3.Zero;
			this.m_MinPos = Vector3.Zero;
		}
		public void SetMineLayingArea(Vector3 maxPos, Vector3 minPos)
		{
			this.m_MaxPos = maxPos;
			this.m_MinPos = minPos;
		}
		public override void Update(int framesElapsed)
		{
			Vector3 position = this.m_Ship.Maneuvering.Position;
			bool on = !this.m_DisableWeaponFire && (this.m_ForceOn || (position.X < this.m_MaxPos.X && position.Y < this.m_MaxPos.Y && position.Z < this.m_MaxPos.Z && position.X > this.m_MinPos.X && position.Y > this.m_MinPos.Y && position.Z > this.m_MinPos.Z));
			this.m_Ship.SetShipWeaponToggleOn(this.m_WeaponID, on);
		}
	}
}
