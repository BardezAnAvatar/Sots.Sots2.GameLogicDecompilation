using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class FlankShipControl : SurroundShipControl
	{
		private Vector3 m_CurrentFlankPosition;
		private Vector3 m_FlankDriveDir;
		private bool m_InFlankPosition;
		public FlankShipControl(App game, TacticalObjective to, CombatAI commanderAI, Vector3 attackVector, float minDist, float desiredDist) : base(game, to, commanderAI, attackVector, minDist, desiredDist)
		{
			this.m_Type = ShipControlType.Flank;
			this.m_CurrentFlankPosition = Vector3.Zero;
			this.m_FlankDriveDir = Vector3.UnitZ;
			this.m_InFlankPosition = false;
		}
		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || this.m_TaskGroupObjective.m_TargetEnemyGroup == null)
			{
				return;
			}
			if (this.m_InFlankPosition)
			{
				base.OnAttackUpdate(framesElapsed);
				return;
			}
			Vector3 currentPosition = base.GetCurrentPosition();
			float num = this.m_Game.AssetDatabase.DefaultTacSensorRange + this.m_TaskGroupObjective.m_TargetEnemyGroup.GetGroupRadius() + 500f;
			Vector3 forward = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownDestination - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
			if (forward.LengthSquared <= 0f)
			{
				forward = currentPosition - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
			}
			forward.Normalize();
			Matrix m = Matrix.CreateWorld(this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition, forward, Vector3.UnitY);
			Matrix mat = Matrix.Inverse(m);
			Vector3 attackVector = this.GetAttackVector(currentPosition, m.Position);
			this.m_CurrentFlankPosition = m.Position + attackVector * this.m_DesiredStandOffDist;
			Vector3 vector = Vector3.Transform(this.m_CurrentFlankPosition, mat);
			if (Math.Abs(Vector3.Transform(currentPosition, mat).X) > num)
			{
				if (vector.Z < 0f)
				{
					this.m_CurrentFlankPosition = currentPosition + m.Forward * num;
				}
				else
				{
					this.m_CurrentFlankPosition = currentPosition - m.Forward * num;
				}
			}
			else
			{
				if (vector.X < 0f)
				{
					this.m_CurrentFlankPosition = currentPosition + m.Right * num;
				}
				else
				{
					this.m_CurrentFlankPosition = currentPosition - m.Right * num;
				}
			}
			Vector3 v = vector;
			v.Normalize();
			this.m_InFlankPosition = (Vector3.Dot(v, -Vector3.UnitZ) > 0.8f);
			foreach (Ship current in this.m_Ships)
			{
				base.SetShipSpeed(current, this.m_InFlankPosition ? ShipSpeedState.Normal : ShipSpeedState.Overthrust);
			}
			base.SetFUP(this.m_CurrentFlankPosition, -attackVector);
		}
	}
}
