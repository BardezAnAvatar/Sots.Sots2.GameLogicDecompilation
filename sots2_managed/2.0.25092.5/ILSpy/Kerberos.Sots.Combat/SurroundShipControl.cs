using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class SurroundShipControl : StandOffShipControl
	{
		private Vector3 m_AttackVec;
		public SurroundShipControl(App game, TacticalObjective to, CombatAI commanderAI, Vector3 attackVector, float minDist, float desiredDist) : base(game, to, commanderAI, minDist, desiredDist)
		{
			this.m_Type = ShipControlType.Surround;
			this.m_AttackVec = attackVector;
		}
		protected override Vector3 GetAttackVector(Vector3 currentPos, Vector3 enemyPos)
		{
			return this.m_AttackVec;
		}
	}
}
