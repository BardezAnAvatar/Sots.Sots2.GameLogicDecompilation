using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class EvadeEnemyObjective : TacticalObjective
	{
		public List<EnemyGroup> m_ThreatsInRange;
		private CombatAI m_CommandAI;
		private PatrolObjective m_PatrolObjective;
		private float m_ShipSensorRange;
		private bool m_IsUnsafe;
		public bool IsUnsafe
		{
			get
			{
				return this.m_IsUnsafe;
			}
		}
		public PatrolObjective EvadePatrolObjective
		{
			get
			{
				return this.m_PatrolObjective;
			}
		}
		public EvadeEnemyObjective(CombatAI command, PatrolObjective patrolObjective, float sensorRange)
		{
			this.m_ObjectiveType = ObjectiveType.EVADE_ENEMY;
			this.m_CommandAI = command;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_ThreatsInRange = new List<EnemyGroup>();
			this.m_RequestTaskGroup = false;
			this.m_IsUnsafe = false;
			this.m_PatrolObjective = patrolObjective;
			this.m_ShipSensorRange = sensorRange;
		}
		public override void Shutdown()
		{
			this.m_CommandAI = null;
			this.m_PatrolObjective = null;
			this.m_ThreatsInRange.Clear();
			base.Shutdown();
		}
		public override Vector3 GetObjectiveLocation()
		{
			return this.m_PatrolObjective.GetObjectiveLocation();
		}
		public override bool IsComplete()
		{
			return false;
		}
		public override void Update()
		{
			int num = 0;
			this.m_ThreatsInRange.Clear();
			foreach (EnemyGroup current in this.m_CommandAI.GetEnemyGroups())
			{
				if (current.GetClosestShip(this.GetObjectiveLocation(), this.m_ShipSensorRange) != null)
				{
					this.m_ThreatsInRange.Add(current);
					num += CombatAI.AssessGroupStrength(current.m_Ships);
				}
			}
			this.m_IsUnsafe = (num > 50);
		}
		public Vector3 GetSafePatrolDirection(Vector3 currentLocation)
		{
			Vector3 objectiveLocation = this.GetObjectiveLocation();
			Vector3 vector = Vector3.UnitZ;
			if (this.m_ThreatsInRange.Count > 0)
			{
				foreach (EnemyGroup current in this.m_ThreatsInRange)
				{
					Vector3 v = objectiveLocation - current.m_LastKnownPosition;
					vector += v;
				}
				vector /= (float)this.m_ThreatsInRange.Count;
			}
			else
			{
				if (objectiveLocation.LengthSquared > 0f)
				{
					vector = objectiveLocation;
				}
				else
				{
					vector = currentLocation;
				}
			}
			vector.Normalize();
			return vector;
		}
		public EnemyGroup GetClosestThreat()
		{
			EnemyGroup result = null;
			float num = 3.40282347E+38f;
			foreach (EnemyGroup current in this.m_ThreatsInRange)
			{
				float lengthSquared = (current.m_LastKnownPosition - this.m_Planet.Parameters.Position).LengthSquared;
				if (lengthSquared < num)
				{
					num = lengthSquared;
					result = current;
				}
			}
			return result;
		}
		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
