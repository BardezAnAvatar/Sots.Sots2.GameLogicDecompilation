using System;
using System.Collections.Generic;
namespace Kerberos.Sots.PlayerFramework
{
	public class IndyDesc
	{
		public string BaseFactionSuitability;
		public float Suitability;
		public float BasePopulationMod;
		public float BiosphereMod;
		public float TradeFTL;
		public int TechLevel;
		public string StellarBodyType;
		public int MinPlanetSize;
		public int MaxPlanetSize;
		public List<SpecialAttribute> CoreSpecialAttributes;
		public List<SpecialAttribute> RandomSpecialAttributes;
	}
}
