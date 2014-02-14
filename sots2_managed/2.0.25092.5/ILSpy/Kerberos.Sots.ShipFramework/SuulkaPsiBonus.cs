using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class SuulkaPsiBonus
	{
		public string Name = "";
		public SuulkaPsiBonusAbilityType Ability;
		public float[] Rate = new float[20];
		public float[] PsiEfficiency = new float[20];
		public float PsiDrainMultiplyer = 1f;
		public float LifeDrainMultiplyer = 1f;
		public float TKFistMultiplyer = 1f;
		public float CrushMultiplyer = 1f;
		public float FearMultiplyer = 1f;
		public float ControlDuration;
		public float MovementMultiplyer = 1f;
		public float BioMissileMultiplyer = 1f;
		public SuulkaPsiBonus()
		{
			for (int i = 0; i <= 19; i++)
			{
				this.Rate[i] = 1f;
				this.PsiEfficiency[i] = 1f;
			}
		}
	}
}
