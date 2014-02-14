using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.Combat
{
	internal class FlyByShipControl : BaseAttackShipControl
	{
		private static int kHoldDuration = 900;
		private static int kStuckDuration = 600;
		private float m_DistancePast;
		private float m_CurrentDistPast;
		private int m_HoldDuration;
		private int m_CurrentHoldDelay;
		private bool m_InitialPass;
		private Matrix m_AttackMatrix;
		private int m_StuckDelay;
		private Vector3 m_PrevPos;
		public FlyByShipControl(App game, TacticalObjective to, CombatAI commanderAI, float distPast) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Flyby;
			this.m_DistancePast = distPast;
			this.m_CurrentDistPast = 0f;
			this.m_HoldDuration = FlyByShipControl.kHoldDuration;
			this.m_AttackMatrix = Matrix.Identity;
			this.m_InitialPass = true;
			this.m_CurrentHoldDelay = this.m_CommanderAI.AIRandom.NextInclusive(FlyByShipControl.kHoldDuration - 20, FlyByShipControl.kHoldDuration + 20);
			this.m_StuckDelay = FlyByShipControl.kStuckDuration;
			this.m_PrevPos = Vector3.Zero;
		}
		protected override void OnAttackUpdate(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || !(this.m_TaskGroupObjective is AttackGroupObjective))
			{
				return;
			}
			if (this.m_InitialPass)
			{
				this.m_InitialPass = false;
				Vector3 v = (this.m_Formation.Ships.Count > 0) ? ShipFormation.GetCenterOfMass(this.m_Formation.Ships) : ShipFormation.GetCenterOfMass(this.m_Formation.ShipsOnBackLine);
				Vector3 forward = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition - v;
				forward.Y = 0f;
				forward.Normalize();
				this.m_AttackMatrix = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
				this.m_CurrentDistPast = this.m_CommanderAI.AIRandom.NextInclusive(this.m_DistancePast - 500f, this.m_DistancePast + 500f);
			}
			Vector3 currentPosition = base.GetCurrentPosition();
			Vector3 vector = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition + this.m_AttackMatrix.Forward * this.m_CurrentDistPast;
			StellarBody stellarBody = (this.m_CommanderAI != null) ? this.m_CommanderAI.GetPlanetContainingPosition(vector) : null;
			if (stellarBody != null)
			{
				vector = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition - this.m_AttackMatrix.Forward * this.m_CurrentDistPast;
			}
			if (base.IsPlanetSeparatingTarget(vector, currentPosition, 1000f))
			{
				Vector3 value = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition - currentPosition;
				value.Y = 0f;
				vector = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition + Vector3.Normalize(value) * 5000f;
			}
			Vector3 vector2 = currentPosition - vector;
			vector2.Y = 0f;
			float lengthSquared = vector2.LengthSquared;
			if ((this.m_PrevPos - currentPosition).LengthSquared < 1f)
			{
				this.m_StuckDelay -= framesElapsed;
			}
			else
			{
				this.m_StuckDelay = FlyByShipControl.kStuckDuration;
			}
			this.m_PrevPos = currentPosition;
			if ((lengthSquared < 40000f && this.m_Formation.DestinationSet && this.m_HoldDuration > 0) || this.m_StuckDelay <= 0)
			{
				base.FaceShipsToTarget();
				this.m_HoldDuration -= framesElapsed;
				if (this.m_HoldDuration <= 0)
				{
					float num = MathHelper.DegreesToRadians(this.m_CommanderAI.AIRandom.NextInclusive(145f, 225f));
					this.m_AttackMatrix = Matrix.CreateRotationYPR(this.m_AttackMatrix.EulerAngles.X + num, 0f, 0f);
					this.m_CurrentDistPast = this.m_CommanderAI.AIRandom.NextInclusive(this.m_DistancePast - 500f, this.m_DistancePast + 500f);
					this.m_CurrentHoldDelay = this.m_CommanderAI.AIRandom.NextInclusive(FlyByShipControl.kHoldDuration - 20, FlyByShipControl.kHoldDuration + 20);
					float addedFlyByAngle = base.GetAddedFlyByAngle(this.m_AttackMatrix, this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition, this.m_CurrentDistPast);
					this.m_AttackMatrix = Matrix.CreateRotationYPR(this.m_AttackMatrix.EulerAngles.X + addedFlyByAngle, 0f, 0f);
					this.m_StuckDelay = FlyByShipControl.kStuckDuration;
				}
				return;
			}
			this.m_HoldDuration = this.m_CurrentHoldDelay;
			base.SetFUP(vector, -this.m_AttackMatrix.Forward);
		}
	}
}
