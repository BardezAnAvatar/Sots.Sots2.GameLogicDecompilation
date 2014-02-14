using System;
using System.Collections.Generic;
namespace Kerberos.Sots
{
	public class PiracyGlobalData
	{
		public enum PiracyBountyType
		{
			PirateBaseDestroyed,
			PirateShipDestroyed,
			FreighterDestroyed,
			FreighterCaptured,
			MaxBountyTypes
		}
		public float PiracyBaseOdds;
		public float PiracyBaseMod;
		public float PiracyModPolice;
		public float PiracyModNavalBase;
		public float PiracyModNoNavalBase;
		public float PiracyModZuulProximity;
		public float PiracyMinZuulProximity;
		public int PiracyMinShips;
		public int PiracyMaxShips;
		public int PiracyMinBaseShips;
		public int PiracyTotalMaxShips;
		public int PiracyBaseRange;
		public int PiracyBaseShipBonus;
		public int PiracyBaseTurnShipBonus;
		public int PiracyBaseTurnsPerUpdate;
		public int[] Bounties = new int[4];
		public Dictionary<string, int> ReactionBonuses = new Dictionary<string, int>();
	}
}
