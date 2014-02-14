using System;
namespace Kerberos.Sots
{
	public class MorrigiRelicGlobalData
	{
		public enum ResearchBonusType
		{
			Captured,
			Destroyed,
			NumTypes
		}
		public enum RelicType
		{
			Pristine1,
			Stealth1,
			Pristine2,
			Stealth2,
			Pristine3,
			Stealth3,
			Pristine4,
			Stealth4,
			Pristine5,
			Stealth5,
			NumTypes
		}
		public int NumTombs;
		public int NumFighters;
		public ResearchBonusData[] ResearchBonus = new ResearchBonusData[2];
		public int[] Rewards = new int[10];
	}
}
