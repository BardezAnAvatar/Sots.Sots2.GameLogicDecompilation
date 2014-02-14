using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class AttackPlanetObjective : TacticalObjective
	{
		public AttackPlanetObjective(StellarBody planet)
		{
			this.m_ObjectiveType = ObjectiveType.ATTACK_TARGET;
			this.m_Planet = planet;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_RequestTaskGroup = false;
		}
		public override bool IsComplete()
		{
			return this.m_Planet.Population < 1.0;
		}
		public override Vector3 GetObjectiveLocation()
		{
			return this.m_Planet.Parameters.Position;
		}
		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
