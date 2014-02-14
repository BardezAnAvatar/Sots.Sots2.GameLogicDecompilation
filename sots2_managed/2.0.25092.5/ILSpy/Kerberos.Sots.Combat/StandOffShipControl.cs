using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.Combat
{
	internal class StandOffShipControl : BaseAttackShipControl
	{
		private enum StandoffState
		{
			Hold,
			MoveFromOutside,
			MoveFromInside,
			MoveToTargetPosition
		}
		private static int kMinRunDuration = 1200;
		private static int kMinHoldGroundDuration = 2400;
		protected float m_MinStandOffDist;
		protected float m_DesiredStandOffDist;
		private float m_PrevDistFromEnemy;
		private int m_MinRunDuration;
		private int m_HoldGroundDuration;
		private bool m_UseStoredDir;
		private StandOffShipControl.StandoffState m_StandOffState;
		private Vector3 m_MoveFromInsideDir;
		public StandOffShipControl(App game, TacticalObjective to, CombatAI commanderAI, float minDist, float desiredDist) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.StandOff;
			this.m_StandOffState = StandOffShipControl.StandoffState.MoveToTargetPosition;
			this.m_MinStandOffDist = minDist;
			this.m_DesiredStandOffDist = desiredDist;
			this.m_MoveFromInsideDir = -Vector3.UnitZ;
			this.m_UseStoredDir = false;
		}
		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || !(this.m_TaskGroupObjective is AttackGroupObjective))
			{
				return;
			}
			Vector3 currentPosition = base.GetCurrentPosition();
			Vector3 lastKnownPosition = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
			Vector3 vector = Vector3.Zero;
			Ship ship = (this.m_GroupPriorityTarget != null) ? this.m_GroupPriorityTarget : this.m_CommanderAI.GetBestShipTarget(currentPosition, this.m_TaskGroupObjective.m_TargetEnemyGroup.m_Ships);
			if (ship != null)
			{
				vector = ship.Maneuvering.Position;
			}
			else
			{
				vector = lastKnownPosition;
			}
			vector.Y = 0f;
			Vector3 v = vector - currentPosition;
			v.Y = 0f;
			float num = v.Normalize();
			Vector3 vector2 = this.GetAttackVector(currentPosition, vector);
			if (vector2.LengthSquared <= 0f)
			{
				vector2 = -Vector3.UnitZ;
			}
			Vector3 v2 = vector + vector2 * this.m_DesiredStandOffDist;
			switch (this.m_StandOffState)
			{
			case StandOffShipControl.StandoffState.Hold:
				if (num > this.m_DesiredStandOffDist)
				{
					this.m_StandOffState = StandOffShipControl.StandoffState.MoveFromOutside;
				}
				else
				{
					if (num < this.m_MinStandOffDist)
					{
						this.m_StandOffState = StandOffShipControl.StandoffState.MoveFromInside;
					}
				}
				this.m_UseStoredDir = false;
				break;
			case StandOffShipControl.StandoffState.MoveFromOutside:
				this.m_HoldGroundDuration = 0;
				if (num < this.m_DesiredStandOffDist && num > this.m_MinStandOffDist)
				{
					if ((currentPosition - v2).LengthSquared > this.m_MinStandOffDist)
					{
						this.m_StandOffState = StandOffShipControl.StandoffState.MoveToTargetPosition;
					}
					else
					{
						this.m_StandOffState = StandOffShipControl.StandoffState.Hold;
					}
				}
				this.m_UseStoredDir = false;
				break;
			case StandOffShipControl.StandoffState.MoveFromInside:
				if (num > this.m_DesiredStandOffDist)
				{
					this.m_StandOffState = StandOffShipControl.StandoffState.Hold;
				}
				break;
			case StandOffShipControl.StandoffState.MoveToTargetPosition:
				if ((currentPosition - v2).LengthSquared < 250000f)
				{
					this.m_StandOffState = StandOffShipControl.StandoffState.MoveFromInside;
				}
				break;
			}
			Vector3 vector3 = Vector3.Zero;
			Vector3 vector4 = -vector2;
			if (this.m_StandOffState == StandOffShipControl.StandoffState.Hold || this.m_HoldGroundDuration > 0)
			{
				base.FaceShipsToTarget();
				if (num < this.m_MinStandOffDist)
				{
					this.m_HoldGroundDuration -= 2 * framesElapsed;
				}
				else
				{
					if (num < this.m_DesiredStandOffDist)
					{
						this.m_HoldGroundDuration -= framesElapsed;
					}
				}
				vector3 = currentPosition;
				vector3.Y = vector.Y;
			}
			else
			{
				if (this.m_StandOffState == StandOffShipControl.StandoffState.MoveFromInside && !(this is SurroundShipControl))
				{
					if (this.m_HoldGroundDuration <= 0 && num - this.m_PrevDistFromEnemy < 20f)
					{
						this.m_MinRunDuration -= framesElapsed;
						if (this.m_MinRunDuration <= 0)
						{
							this.m_HoldGroundDuration = StandOffShipControl.kMinHoldGroundDuration;
							this.m_MinRunDuration = StandOffShipControl.kMinRunDuration;
							Vector3 averageVelocity = this.m_TaskGroupObjective.m_TargetEnemyGroup.GetAverageVelocity(this.m_CommanderAI);
							averageVelocity.Normalize();
							if (vector2.LengthSquared > 0f)
							{
								this.m_MoveFromInsideDir = vector2;
							}
							if (Vector3.Dot(averageVelocity, vector2) > 0.25f)
							{
								this.m_MoveFromInsideDir *= -1f;
							}
							this.m_UseStoredDir = true;
						}
					}
					else
					{
						this.m_MinRunDuration = StandOffShipControl.kMinRunDuration;
					}
				}
				if (this.m_UseStoredDir)
				{
					vector4 = -this.m_MoveFromInsideDir;
				}
				vector3 = vector + -vector4 * this.m_DesiredStandOffDist;
				StellarBody stellarBody = (this.m_CommanderAI != null) ? this.m_CommanderAI.GetPlanetContainingPosition(vector3) : null;
				if (stellarBody != null)
				{
					vector3 = vector + vector4 * this.m_DesiredStandOffDist;
				}
				if (base.IsPlanetSeparatingTarget(vector3, currentPosition, 1000f))
				{
					vector3 = vector + v * 5000f;
				}
			}
			this.m_PrevDistFromEnemy = num;
			base.SetFUP(vector3, vector4);
		}
		protected virtual Vector3 GetAttackVector(Vector3 currentPos, Vector3 enemyPos)
		{
			Vector3 vector = currentPos - enemyPos;
			vector.Y = 0f;
			vector.Normalize();
			Vector3 forward = Vector3.Normalize(new Vector3(enemyPos.X, 0f, enemyPos.Z));
			Matrix matrix = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
			Vector3 vector2 = (Vector3.Dot(vector, matrix.Right) > 0f) ? matrix.Right : (-matrix.Right);
			float length = currentPos.Length;
			float num = this.m_CommanderAI.SystemRadius - this.m_CommanderAI.MinSystemRadius;
			float num2 = num * 0.8f;
			float num3 = Math.Max(Math.Min((length - num2) / (num - num2), 1f), 0f);
			float t = Math.Max(Math.Min((length - this.m_CommanderAI.MinSystemRadius) / num, 1f), 0f);
			if (num3 > 0f)
			{
				vector2 = Vector3.Lerp(vector2, -Vector3.Normalize(currentPos), num3);
				vector2.Normalize();
			}
			vector = Vector3.Lerp(vector, vector2, t);
			vector.Normalize();
			return vector;
		}
	}
}
