using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class NormalMonitorCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Monitor;
		private CommandMonitorCombatAIControl m_Command;
		private MonitorAIStates m_State;
		private bool m_HadParent;
		private bool m_DisableWeapons;
		public override Ship GetShip()
		{
			return this.m_Monitor;
		}
		public override void SetTarget(IGameObject target)
		{
		}
		public override IGameObject GetTarget()
		{
			return null;
		}
		public NormalMonitorCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Monitor = ship;
		}
		public override void Initialize()
		{
			this.m_State = MonitorAIStates.DISABLE;
			this.m_Command = null;
			this.m_HadParent = false;
			this.m_DisableWeapons = false;
			foreach (WeaponBank current in this.m_Monitor.WeaponBanks)
			{
				current.PostSetProp("DisableAllTurrets", true);
			}
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Command != null && obj == this.m_Command.GetShip())
			{
				this.m_Command = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_Monitor == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case MonitorAIStates.IDLE:
				this.ThinkIdle();
				return;
			case MonitorAIStates.DISABLE:
				this.ThinkDisable();
				return;
			default:
				return;
			}
		}
		public override void ForceFlee()
		{
		}
		public override bool VictoryConditionIsMet()
		{
			return this.m_HadParent && (this.m_Command == null || this.m_Command.VictoryConditionIsMet());
		}
		public override bool RequestingNewTarget()
		{
			return false;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
		}
		public override bool NeedsAParent()
		{
			return !this.m_HadParent && this.m_Command == null;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is CommandMonitorCombatAIControl)
				{
					this.m_Command = (current as CommandMonitorCombatAIControl);
					this.m_Command.AddNormal(this);
					break;
				}
			}
			if (this.m_Command != null)
			{
				this.m_HadParent = true;
			}
		}
		public void ForceDisable()
		{
			this.m_DisableWeapons = true;
		}
		private void ThinkIdle()
		{
			if (this.m_Command == null || this.m_Command.GetShip() == null || this.m_Command.GetShip().IsDestroyed)
			{
				this.m_Command = null;
			}
			if (this.m_Monitor.IsDestroyed || (this.m_HadParent && this.m_Command == null) || this.m_DisableWeapons)
			{
				foreach (WeaponBank current in this.m_Monitor.WeaponBanks)
				{
					current.PostSetProp("DisableAllTurrets", true);
				}
				this.m_State = MonitorAIStates.DISABLE;
			}
		}
		private void ThinkDisable()
		{
			if (!this.m_Monitor.IsDestroyed && this.m_Command != null && !this.m_DisableWeapons)
			{
				foreach (WeaponBank current in this.m_Monitor.WeaponBanks)
				{
					current.PostSetProp("DisableAllTurrets", false);
				}
				this.m_State = MonitorAIStates.IDLE;
			}
		}
	}
}
