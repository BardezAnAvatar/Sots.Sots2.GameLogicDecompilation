using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.Combat
{
	internal class ShipCombatInfo
	{
		public ShipInfo shipInfo;
		public float trackingFireFactor;
		public float directFireFactor;
		public float armorFactor;
		public float structureFactor;
		public float pdFactor;
		public int battleRiders;
		public int drones;
		public float bombFactorPopulation;
		public float bombFactorInfrastructure;
		public float bombFactorHazard;
		public bool shipDead;
	}
}
