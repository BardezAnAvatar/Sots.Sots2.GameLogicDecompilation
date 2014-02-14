using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class AttackPlanetShipControl : BaseAttackShipControl
	{
		public AttackPlanetShipControl(App game, TacticalObjective to, CombatAI commanderAI) : base(game, to, commanderAI)
		{
		}
		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || this.m_TaskGroupObjective.m_Planet == null)
			{
				return;
			}
			foreach (Ship current in this.m_Ships)
			{
				if (current.Target != this.m_TaskGroupObjective.m_Planet)
				{
					base.SetNewTarget(current, this.m_TaskGroupObjective.m_Planet);
				}
			}
			Vector3 currentPosition = base.GetCurrentPosition();
			Vector3 safeDestination = this.m_CommanderAI.GetSafeDestination(currentPosition, this.m_TaskGroupObjective.m_Planet.Parameters.Position);
			Vector3 destFacing = safeDestination - currentPosition;
			destFacing.Y = 0f;
			destFacing.Normalize();
			base.SetFUP(safeDestination, destFacing);
			float num = 15000f;
			float num2 = (!this.m_CommanderAI.EnemiesPresentInSystem) ? 15000f : 100000f;
			num *= num;
			num2 *= num2;
			foreach (Ship current2 in this.m_Ships)
			{
				ShipSpeedState shipSpeedState = ((current2.Position - safeDestination).LengthSquared > num2) ? ShipSpeedState.Overthrust : ShipSpeedState.Normal;
				if (shipSpeedState == ShipSpeedState.Overthrust && current2.TaskGroup != null && current2.TaskGroup.EnemyGroupInContact != null && (current2.Position - current2.TaskGroup.EnemyGroupInContact.m_LastKnownPosition).LengthSquared < num)
				{
					shipSpeedState = ShipSpeedState.Normal;
				}
				base.SetShipSpeed(current2, shipSpeedState);
			}
		}
	}
}
