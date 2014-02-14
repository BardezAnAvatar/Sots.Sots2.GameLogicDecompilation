using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class SpecWeaponControl
	{
		protected static int kRemoveUpdateRate = 40;
		protected App m_Game;
		protected CombatAI m_CommanderAI;
		protected Ship m_Ship;
		protected WeaponEnums.TurretClasses m_WeaponType;
		protected WeaponTarget m_CurrentWeaponTarget;
		protected bool m_DisableWeaponFire;
		protected int m_CurrUpdateFrame;
		protected bool m_RequestNewTarget;
		protected bool m_RequestHoldShip;
		public Ship ControlledShip
		{
			get
			{
				return this.m_Ship;
			}
		}
		public WeaponEnums.TurretClasses Type
		{
			get
			{
				return this.m_WeaponType;
			}
		}
		public WeaponTarget CurrentWeaponTarget
		{
			get
			{
				return this.m_CurrentWeaponTarget;
			}
		}
		public bool DisableWeaponFire
		{
			get
			{
				return this.m_DisableWeaponFire;
			}
			set
			{
				this.m_DisableWeaponFire = value;
			}
		}
		public SpecWeaponControl(App game, CombatAI commanderAI, Ship ship, WeaponEnums.TurretClasses weaponType)
		{
			this.m_Game = game;
			this.m_CommanderAI = commanderAI;
			this.m_Ship = ship;
			this.m_WeaponType = weaponType;
			this.m_CurrentWeaponTarget = null;
			this.m_RequestNewTarget = false;
			this.m_RequestHoldShip = false;
			this.m_CurrUpdateFrame = commanderAI.AIRandom.NextInclusive(1, SpecWeaponControl.kRemoveUpdateRate);
			ship.WeaponControls.Add(this);
		}
		public virtual void Shutdown()
		{
			if (this.m_Ship != null)
			{
				this.m_Ship.WeaponControls.Remove(this);
			}
		}
		public virtual void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Ship == obj)
			{
				this.m_Ship = null;
			}
		}
		public virtual bool RemoveWeaponControl()
		{
			if (this.m_Ship == null)
			{
				return true;
			}
			this.m_CurrUpdateFrame--;
			if (this.m_CurrUpdateFrame > 0)
			{
				return false;
			}
			this.m_CurrUpdateFrame = SpecWeaponControl.kRemoveUpdateRate;
			foreach (Kerberos.Sots.GameObjects.Section current in this.m_Ship.Sections)
			{
				LogicalMount[] mounts = current.ShipSectionAsset.Mounts;
				for (int i = 0; i < mounts.Length; i++)
				{
					LogicalMount logicalMount = mounts[i];
					if (logicalMount.Bank != null && logicalMount.Bank != null && logicalMount.Bank.TurretClass == this.m_WeaponType)
					{
						return false;
					}
				}
			}
			return true;
		}
		public bool RequestNewTarget()
		{
			return this.m_RequestNewTarget;
		}
		public bool RequestHoldShip()
		{
			return this.m_RequestHoldShip;
		}
		public virtual void FindNewTarget(EnemyGroup targetEnemyGroup)
		{
			this.m_RequestNewTarget = false;
		}
		public virtual void Update(int framesElapsed)
		{
		}
	}
}
