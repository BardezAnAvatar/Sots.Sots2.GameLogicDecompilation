using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class PatrolObjective : TacticalObjective
	{
		private Vector3 m_Destination;
		private float m_MinRadius;
		private float m_MaxRadius;
		private bool m_ForPolice;
		public PatrolObjective(Vector3 dest, float minRadius, float maxRadius)
		{
			this.m_ObjectiveType = ObjectiveType.PATROL;
			this.m_Destination = dest;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_MinRadius = minRadius;
			this.m_MaxRadius = maxRadius;
			this.m_RequestTaskGroup = false;
			this.m_ForPolice = false;
		}
		public PatrolObjective(Ship police, Vector3 dest, float minRadius, float maxRadius)
		{
			this.m_ObjectiveType = ObjectiveType.PATROL;
			this.m_Destination = dest;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_MinRadius = minRadius;
			this.m_MaxRadius = maxRadius;
			this.m_RequestTaskGroup = false;
			this.m_PoliceOwner = police;
			this.m_ForPolice = true;
		}
		public override void AssignResources(TaskGroup group)
		{
			base.AssignResources(group);
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
			return this.m_ForPolice && (this.m_PoliceOwner == null || !this.m_PoliceOwner.IsPolicePatrolling || !Ship.IsActiveShip(this.m_PoliceOwner));
		}
		public override int ResourceNeeds()
		{
			return 1;
		}
		public List<Vector3> GetPatrolWaypoints(PatrolType pt, Vector3 dir, float maxDist)
		{
			List<Vector3> list = new List<Vector3>();
			float num = Math.Min(this.m_MaxRadius - this.m_MinRadius, maxDist);
			Vector3 v = this.m_Destination + dir * this.m_MinRadius;
			Vector3 v2 = Vector3.Cross(dir, Vector3.UnitY);
			switch (pt)
			{
			case PatrolType.Circular:
			{
				float s = num * 0.5f;
				Vector3 v3 = v + dir * s;
				int num2 = 8;
				float num3 = MathHelper.DegreesToRadians(360f / (float)num2);
				for (int i = 0; i < num2; i++)
				{
					float num4 = num3 * (float)i;
					Vector3 v4 = new Vector3((float)Math.Sin((double)num4), 0f, -(float)Math.Cos((double)num4));
					list.Add(v3 + v4 * s);
				}
				break;
			}
			case PatrolType.Line:
			{
				float s2 = 1000f;
				Vector3 v5 = v + dir * s2;
				list.Add(v5 + v2 * num);
				list.Add(v5 - v2 * num);
				break;
			}
			case PatrolType.Box:
				list.Add(this.m_Destination + dir * this.m_MaxRadius + v2 * num);
				list.Add(this.m_Destination + dir * this.m_MaxRadius - v2 * num);
				list.Add(this.m_Destination + dir * this.m_MinRadius + v2 * num);
				list.Add(this.m_Destination + dir * this.m_MinRadius - v2 * num);
				break;
			case PatrolType.Orbit:
			{
				float num5 = 1000f;
				int num6 = 8;
				float num7 = MathHelper.DegreesToRadians(360f / (float)num6);
				for (int j = 0; j < num6; j++)
				{
					float num8 = num7 * (float)j;
					Vector3 v6 = new Vector3((float)Math.Sin((double)num8), 0f, -(float)Math.Cos((double)num8));
					list.Add(this.m_Destination + v6 * (this.m_MinRadius + num5));
				}
				break;
			}
			}
			return list;
		}
	}
}
