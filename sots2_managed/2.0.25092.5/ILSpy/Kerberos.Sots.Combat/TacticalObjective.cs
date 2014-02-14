using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal abstract class TacticalObjective
	{
		public ObjectiveType m_ObjectiveType;
		public EnemyGroup m_TargetEnemyGroup;
		public Ship m_PoliceOwner;
		public StellarBody m_Planet;
		public List<TaskGroup> m_TaskGroups;
		public TaskGroup m_TargetTaskGroup;
		public int m_CurrentResources;
		public bool m_RequestTaskGroup;
		public TacticalObjective()
		{
		}
		public abstract bool IsComplete();
		public abstract int ResourceNeeds();
		public abstract Vector3 GetObjectiveLocation();
		public virtual void Update()
		{
		}
		public virtual void Shutdown()
		{
			this.m_TargetTaskGroup = null;
			this.m_TargetEnemyGroup = null;
			this.m_PoliceOwner = null;
			this.m_Planet = null;
			this.m_TaskGroups.Clear();
		}
		public virtual int CurrentResources()
		{
			int num = 0;
			foreach (TaskGroup current in this.m_TaskGroups)
			{
				num += CombatAI.AssessGroupStrength(current.GetShips());
			}
			return num;
		}
		public virtual void AssignResources(TaskGroup group)
		{
			this.m_TaskGroups.Add(group);
			group.Objective = this;
			this.m_CurrentResources = this.CurrentResources();
		}
		public virtual void RemoveTaskGroup(TaskGroup group)
		{
			if (this.m_TaskGroups.Contains(group))
			{
				this.m_TaskGroups.Remove(group);
				group.Objective = null;
				this.m_CurrentResources = this.CurrentResources();
			}
		}
	}
}
