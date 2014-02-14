using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class ScoutShipControl : TaskGroupShipControl
	{
		private float m_SensorRange;
		private bool m_EncounteredEnemy;
		private EnemyGroup m_EnemyGroup;
		public bool EncounteredEnemy
		{
			get
			{
				return this.m_EncounteredEnemy;
			}
		}
		public ScoutShipControl(App game, TacticalObjective to, CombatAI commanderAI, float sensorRange) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Scout;
			this.m_EncounteredEnemy = false;
			this.m_SensorRange = sensorRange;
		}
		public void NotifyEnemyGroupDetected(EnemyGroup eGroup)
		{
			this.m_EnemyGroup = eGroup;
			this.m_EncounteredEnemy = (this.m_EnemyGroup != null);
		}
		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null)
			{
				return;
			}
			Vector3 currentPosition = base.GetCurrentPosition();
			Vector3 vector = Vector3.Zero;
			Vector3 vector2 = -Vector3.UnitZ;
			if (this.m_EncounteredEnemy)
			{
				vector2 = currentPosition - this.m_EnemyGroup.m_LastKnownPosition;
				vector2.Y = 0f;
				vector2.Normalize();
				vector = this.m_EnemyGroup.m_LastKnownPosition + vector2 * this.m_SensorRange;
				vector2 *= -1f;
			}
			else
			{
				ScoutObjective scoutObjective = this.m_TaskGroupObjective as ScoutObjective;
				vector = scoutObjective.GetObjectiveLocation();
				vector2 = currentPosition - vector;
				vector2.Y = 0f;
				vector2.Normalize();
			}
			base.SetFUP(vector, vector2);
		}
	}
}
