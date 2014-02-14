using System;
using System.Collections.Generic;
namespace Kerberos.Sots.PlayerFramework
{
	internal class CivilianRatios
	{
		public readonly Dictionary<Faction, float> FactionPopulationWeightTargets = new Dictionary<Faction, float>();
		public float CivilianPopulationWeightTarget = 1f;
	}
}
