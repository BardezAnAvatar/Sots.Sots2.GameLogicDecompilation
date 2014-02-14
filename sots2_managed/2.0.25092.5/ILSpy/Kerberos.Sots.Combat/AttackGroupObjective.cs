using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class AttackGroupObjective : TacticalObjective
	{
		public enum AttackStyle
		{
			StandOff,
			Pursue,
			BroadsidePursue,
			Encircle,
			FlyBy,
			Flank
		}
		public AttackGroupObjective(EnemyGroup eGroup)
		{
			this.m_ObjectiveType = ObjectiveType.ATTACK_TARGET;
			this.m_TargetEnemyGroup = eGroup;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_RequestTaskGroup = false;
		}
		public override bool IsComplete()
		{
			return this.m_TargetEnemyGroup == null || this.m_TargetEnemyGroup.m_Ships.Count<Ship>() < 1;
		}
		public override int ResourceNeeds()
		{
			return CombatAI.AssessGroupStrength(this.m_TargetEnemyGroup.m_Ships);
		}
		public override void AssignResources(TaskGroup group)
		{
			base.AssignResources(group);
		}
		public override Vector3 GetObjectiveLocation()
		{
			return this.m_TargetEnemyGroup.m_LastKnownPosition;
		}
	}
}
