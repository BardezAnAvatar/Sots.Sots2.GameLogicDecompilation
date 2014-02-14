using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class DefendPlanetObjective : TacticalObjective
	{
		private const float THREAT_RANGE = 10000f;
		private const float MAX_ATTACKING_THREAT_RANGE = 50000f;
		public List<EnemyGroup> m_ThreatsInRange;
		private CombatAI m_CommandAI;
		private PatrolObjective m_PatrolObjective;
		public PatrolObjective DefendPatrolObjective
		{
			get
			{
				return this.m_PatrolObjective;
			}
		}
		public DefendPlanetObjective(StellarBody planet, CombatAI command, PatrolObjective patrolObjective)
		{
			this.m_ObjectiveType = ObjectiveType.DEFEND_TARGET;
			this.m_Planet = planet;
			this.m_CommandAI = command;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_ThreatsInRange = new List<EnemyGroup>();
			this.m_RequestTaskGroup = false;
			this.m_PatrolObjective = patrolObjective;
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
			return this.m_Planet.Parameters.Position;
		}
		public override bool IsComplete()
		{
			return false;
		}
		public override void Update()
		{
			this.m_RequestTaskGroup = false;
			this.m_ThreatsInRange.Clear();
			Ship ship = null;
			if (this.m_Planet.LastAttackingObject is Ship)
			{
				ship = (this.m_Planet.LastAttackingObject as Ship);
				if ((this.m_Planet.Parameters.Position - ship.Position).LengthSquared > 2.5E+09f)
				{
					ship = null;
				}
			}
			foreach (EnemyGroup current in this.m_CommandAI.GetEnemyGroups())
			{
				if (current.GetClosestShip(this.m_Planet.Parameters.Position, this.m_Planet.Parameters.Radius + 10000f) != null || (ship != null && current.m_Ships.Contains(ship)))
				{
					this.m_ThreatsInRange.Add(current);
					this.m_RequestTaskGroup = true;
				}
			}
			if (this.m_Planet.LastAttackingObject is Ship)
			{
				Ship ship2 = this.m_Planet.LastAttackingObject as Ship;
				if (this.m_CommandAI != null && this.m_CommandAI.GetDiplomacyState(ship2.Player.ID) == DiplomacyState.WAR)
				{
					this.m_CommandAI.FlagAttackingShip(ship2);
				}
			}
		}
		public Vector3 GetPatrolDirection(TaskGroup tg)
		{
			int num = 0;
			foreach (TaskGroup current in this.m_TaskGroups)
			{
				if (current == tg)
				{
					break;
				}
				num++;
			}
			return this.GetPatrolDirection(num);
		}
		public Vector3 GetPatrolDirection(int index)
		{
			Vector3 unitZ = Vector3.UnitZ;
			float num = MathHelper.DegreesToRadians(60f);
			int num2 = (index % 6 + 1) / 2;
			int num3 = (index % 2 == 0) ? 1 : -1;
			float x = (float)(num3 * num2) * num;
			Vector3 forward = -this.GetObjectiveLocation();
			forward.Normalize();
			Matrix lhs = Matrix.CreateRotationYPR(new Vector3(x, 0f, 0f));
			Matrix rhs = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
			return (lhs * rhs).Forward;
		}
		public EnemyGroup GetClosestThreat()
		{
			EnemyGroup enemyGroup = null;
			foreach (EnemyGroup current in this.m_ThreatsInRange)
			{
				if (enemyGroup == null || current.IsHigherPriorityThan(enemyGroup, this.m_CommandAI, true))
				{
					enemyGroup = current;
				}
			}
			return enemyGroup;
		}
		public override int ResourceNeeds()
		{
			int num = 0;
			foreach (EnemyGroup current in this.m_ThreatsInRange)
			{
				num += CombatAI.AssessGroupStrength(current.m_Ships);
			}
			return num;
		}
	}
}
