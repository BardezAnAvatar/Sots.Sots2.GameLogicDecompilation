using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class ScoutObjective : TacticalObjective
	{
		public Vector3 m_Destination;
		private Player m_Player;
		public ScoutObjective(EnemyGroup eGroup, Player player)
		{
			this.m_ObjectiveType = ObjectiveType.SCOUT;
			this.m_TargetEnemyGroup = eGroup;
			this.m_Destination = eGroup.m_LastKnownPosition;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_Player = player;
			this.m_RequestTaskGroup = false;
			this.m_Planet = null;
		}
		public override void Shutdown()
		{
			this.m_Player = null;
			base.Shutdown();
		}
		public ScoutObjective(StellarBody ePlanet, Player player)
		{
			this.m_ObjectiveType = ObjectiveType.SCOUT;
			this.m_Planet = ePlanet;
			this.m_Destination = ePlanet.Parameters.Position;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_Player = player;
			this.m_RequestTaskGroup = false;
			this.m_TargetEnemyGroup = null;
		}
		public override Vector3 GetObjectiveLocation()
		{
			if (this.m_TargetEnemyGroup != null)
			{
				return this.m_TargetEnemyGroup.m_LastKnownPosition;
			}
			return this.m_Destination;
		}
		public void SetPatrolDestination(Vector3 dest)
		{
			this.m_Destination = dest;
		}
		public override bool IsComplete()
		{
			if (this.m_TargetEnemyGroup != null)
			{
				if (this.m_TargetEnemyGroup.m_Ships.Count != 0)
				{
					if (this.m_Player == null)
					{
						goto IL_44;
					}
					if (!this.m_TaskGroups.Any((TaskGroup x) => x.EnemyGroupInContact == this.m_TargetEnemyGroup))
					{
						goto IL_44;
					}
				}
				return true;
			}
			IL_44:
			if (this.m_Planet != null)
			{
				float offset = this.m_Planet.Parameters.Radius + 750f + 500f + 2000f;
				offset *= offset;
				if (this.m_TaskGroups.Any((TaskGroup x) => x.GetShipCount() > 0 && (x.GetBaseGroupPosition() - this.m_Planet.Parameters.Position).LengthSquared < offset))
				{
					return true;
				}
			}
			return this.m_TargetEnemyGroup == null && this.m_Planet == null;
		}
		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
