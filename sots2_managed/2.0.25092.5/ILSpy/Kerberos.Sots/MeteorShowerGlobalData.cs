using System;
namespace Kerberos.Sots
{
	public class MeteorShowerGlobalData
	{
		public enum MeteorSizes
		{
			Small,
			Medium,
			Large,
			NumMeteors
		}
		public int MinMeteors;
		public int MaxMeteors;
		public int LargeMeteorChance;
		public int NumBreakoffMeteors;
		public CombatAIDamageData[] Damage = new CombatAIDamageData[3];
		public int[] ResourceBonuses = new int[3];
	}
}
