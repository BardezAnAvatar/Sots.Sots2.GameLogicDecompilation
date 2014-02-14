using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class PatrolShipControl : TaskGroupShipControl
	{
		private static int kStuckDelay = 300;
		private PatrolType m_PatrolType;
		private bool m_ClockWise;
		private List<Vector3> m_PatrolWaypoints;
		private Vector3 m_PrevPosition;
		private Vector3 m_PrevDir;
		private int m_CurrWaypointIndex;
		private int m_StuckDelay;
		private float m_CloseToDist;
		public Vector3 PreviousDir
		{
			get
			{
				return this.m_PrevDir;
			}
		}
		public PatrolShipControl(App game, TacticalObjective to, CombatAI commanderAI, PatrolType pt, Vector3 dir, float maxPatrolDist, bool clockwise) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Patrol;
			this.m_PatrolType = pt;
			this.m_ClockWise = clockwise;
			this.m_PrevPosition = default(Vector3);
			this.m_PrevDir = default(Vector3);
			this.m_StuckDelay = PatrolShipControl.kStuckDelay;
			this.ResetPatrolWaypoints(pt, dir, maxPatrolDist);
			this.m_CloseToDist = 1000f;
		}
		public void ResetPatrolWaypoints(PatrolType pt, Vector3 dir, float maxPatrolDist)
		{
			if (this.m_TaskGroupObjective is PatrolObjective)
			{
				PatrolObjective patrolObjective = this.m_TaskGroupObjective as PatrolObjective;
				this.m_PatrolWaypoints = patrolObjective.GetPatrolWaypoints(pt, dir, maxPatrolDist);
				this.m_PrevDir = dir;
				Vector3 currentPosition = base.GetCurrentPosition();
				this.m_CurrWaypointIndex = 0;
				float num = 3.40282347E+38f;
				for (int i = 0; i < this.m_PatrolWaypoints.Count; i++)
				{
					float lengthSquared = (this.m_PatrolWaypoints[i] - currentPosition).LengthSquared;
					if (lengthSquared < num)
					{
						num = lengthSquared;
						this.m_CurrWaypointIndex = i;
					}
				}
			}
		}
		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null)
			{
				return;
			}
			Vector3 currentPosition = base.GetCurrentPosition();
			Vector3 destFacing = this.m_PatrolWaypoints[this.m_CurrWaypointIndex] - currentPosition;
			bool flag = false;
			if ((this.m_PrevPosition - currentPosition).LengthSquared < 10f)
			{
				this.m_StuckDelay -= framesElapsed;
				if (this.m_StuckDelay <= 0)
				{
					flag = true;
					this.m_StuckDelay = PatrolShipControl.kStuckDelay;
				}
			}
			else
			{
				this.m_PrevPosition = currentPosition;
				this.m_StuckDelay = PatrolShipControl.kStuckDelay;
			}
			if (destFacing.LengthSquared < this.m_CloseToDist * this.m_CloseToDist || flag)
			{
				if (!this.m_ClockWise)
				{
					this.m_CurrWaypointIndex = (this.m_CurrWaypointIndex + 1) % this.m_PatrolWaypoints.Count;
				}
				else
				{
					this.m_CurrWaypointIndex--;
					if (this.m_CurrWaypointIndex < 0)
					{
						this.m_CurrWaypointIndex = this.m_PatrolWaypoints.Count - 1;
					}
				}
				destFacing = this.m_PatrolWaypoints[this.m_CurrWaypointIndex] - base.GetCurrentPosition();
			}
			destFacing.Y = 0f;
			destFacing.Normalize();
			base.SetFUP(this.m_PatrolWaypoints[this.m_CurrWaypointIndex], destFacing);
		}
	}
}
