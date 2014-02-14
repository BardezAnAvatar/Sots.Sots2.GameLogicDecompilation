using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class RetreatObjective : TacticalObjective
	{
		public Vector3 m_Destination;
		public RetreatObjective(Vector3 pos)
		{
			this.m_ObjectiveType = ObjectiveType.RETREAT;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_Destination = pos;
			this.m_RequestTaskGroup = false;
		}
		public RetreatObjective(StellarBody homeColony)
		{
			this.m_ObjectiveType = ObjectiveType.RETREAT;
			this.m_Planet = homeColony;
			this.m_Destination = homeColony.Parameters.Position;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_RequestTaskGroup = false;
		}
		public void ResetRetreatPosition(TaskGroup tg)
		{
			if (tg == null)
			{
				return;
			}
			float length = this.m_Destination.Length;
			Vector3 baseGroupPosition = tg.GetBaseGroupPosition();
			this.m_Destination = Vector3.Normalize(baseGroupPosition) * length;
		}
		public override Vector3 GetObjectiveLocation()
		{
			return this.m_Destination;
		}
		public void SetPatrolDestination(Vector3 dest)
		{
			this.m_Destination = dest;
		}
		public override bool IsComplete()
		{
			return false;
		}
		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
