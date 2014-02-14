using System;
namespace Kerberos.Sots.Combat
{
	internal struct EnemyGroupData
	{
		public int NumAggressive;
		public int NumPassive;
		public int NumCivilian;
		public int NumUnarmed;
		public float DistanceFromColony;
		public bool IsAttackingPlanetOrStation;
		public bool IsEncounter;
		public bool IsFreighter;
		public bool IsStation;
	}
}
