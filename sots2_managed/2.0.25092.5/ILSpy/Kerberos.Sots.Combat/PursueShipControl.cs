using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.Combat
{
	internal class PursueShipControl : BaseAttackShipControl
	{
		private static int kMinFlyByDelay = 600;
		private Vector3 m_PrevEnemyPosition;
		private Vector3 m_FlyByPosition;
		private int m_MinFlyByDelay;
		private float m_PursueRange;
		private float m_PrevFlyByDistSq;
		private bool m_DoFlyBy;
		public PursueShipControl(App game, TacticalObjective to, CombatAI commanderAI, float pursueRange) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Pursue;
			this.m_PursueRange = pursueRange;
			this.m_DoFlyBy = false;
			this.m_PrevEnemyPosition = Vector3.Zero;
			this.m_FlyByPosition = Vector3.Zero;
			this.m_MinFlyByDelay = PursueShipControl.kMinFlyByDelay;
			this.m_PrevFlyByDistSq = 0f;
		}
		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || this.m_TaskGroupObjective.m_TargetEnemyGroup == null)
			{
				return;
			}
			Vector3 lastKnownPosition = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
			Vector3 currentPosition = base.GetCurrentPosition();
			Vector3 vector = Vector3.Zero;
			if (this.m_DoFlyBy)
			{
				vector = this.m_FlyByPosition;
				if ((currentPosition - vector).LengthSquared < 10000f)
				{
					this.m_DoFlyBy = false;
					this.m_MinFlyByDelay = PursueShipControl.kMinFlyByDelay;
				}
			}
			else
			{
				Ship ship = (this.m_GroupPriorityTarget != null) ? this.m_GroupPriorityTarget : this.m_CommanderAI.GetBestShipTarget(currentPosition, this.m_TaskGroupObjective.m_TargetEnemyGroup.m_Ships);
				if (ship != null)
				{
					vector = ship.Maneuvering.Position;
				}
				else
				{
					vector = lastKnownPosition;
				}
			}
			vector.Y = 0f;
			Vector3 vector2 = currentPosition - vector;
			vector2.Y = 0f;
			bool flag = false;
			if ((this.m_PrevEnemyPosition - lastKnownPosition).LengthSquared < 10f)
			{
				if (!this.m_DoFlyBy)
				{
					float num = this.m_PursueRange + 200f;
					if (vector2.LengthSquared < num * num)
					{
						flag = true;
					}
				}
				else
				{
					float lengthSquared = vector2.LengthSquared;
					if (Math.Abs(lengthSquared - this.m_PrevFlyByDistSq) < 10f)
					{
						flag = true;
					}
					this.m_PrevFlyByDistSq = lengthSquared;
				}
			}
			else
			{
				this.m_DoFlyBy = false;
			}
			if (flag)
			{
				this.m_MinFlyByDelay -= framesElapsed;
				if (this.m_MinFlyByDelay <= 0)
				{
					this.m_DoFlyBy = true;
					this.m_MinFlyByDelay = PursueShipControl.kMinFlyByDelay;
					Vector3 vector3 = lastKnownPosition - currentPosition;
					vector3.Y = 0f;
					vector3.Normalize();
					this.m_FlyByPosition = base.GetFlyByPosition(currentPosition, lastKnownPosition, 2f * this.m_PursueRange + this.m_TaskGroupObjective.m_TargetEnemyGroup.GetGroupRadius(), 0f);
				}
			}
			else
			{
				this.m_MinFlyByDelay = PursueShipControl.kMinFlyByDelay;
			}
			this.m_PrevEnemyPosition = lastKnownPosition;
			Vector3 vector4 = Vector3.Zero;
			Vector3 destFacing = -Vector3.UnitZ;
			if (!this.m_DoFlyBy && vector2.LengthSquared < this.m_PursueRange * this.m_PursueRange && this.m_Formation.DestinationSet)
			{
				base.FaceShipsToTarget();
				return;
			}
			if (this.m_DoFlyBy)
			{
				vector4 = this.m_FlyByPosition;
				vector2.Normalize();
				destFacing = vector2 * -1f;
			}
			else
			{
				float length = vector2.Length;
				if (length > 0f)
				{
					vector2 /= length;
					float s = Math.Min(length, this.m_PursueRange);
					vector4 = vector + vector2 * s;
					destFacing = vector2 * -1f;
					StellarBody stellarBody = (this.m_CommanderAI != null) ? this.m_CommanderAI.GetPlanetContainingPosition(vector4) : null;
					if (stellarBody != null)
					{
						vector4 = vector - vector2 * s;
						destFacing = vector2;
					}
					if (base.IsPlanetSeparatingTarget(vector4, currentPosition, 1000f))
					{
						float num2 = (stellarBody != null) ? -1f : 1f;
						vector4 = vector + vector2 * (5000f * num2);
						destFacing = vector2 * -num2;
					}
				}
				else
				{
					vector4 = currentPosition;
					destFacing = this.m_Formation.Facing;
				}
			}
			base.SetFUP(vector4, destFacing);
		}
	}
}
