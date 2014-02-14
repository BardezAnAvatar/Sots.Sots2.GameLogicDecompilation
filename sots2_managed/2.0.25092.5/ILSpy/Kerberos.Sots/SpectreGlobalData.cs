using System;
namespace Kerberos.Sots
{
	public class SpectreGlobalData
	{
		public enum SpectreSize
		{
			Small,
			Medium,
			Large,
			NumSizes
		}
		public int MinSpectres;
		public int MaxSpectres;
		public CombatAIDamageData[] Damage = new CombatAIDamageData[3];
	}
}
