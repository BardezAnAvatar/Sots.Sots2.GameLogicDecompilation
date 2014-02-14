using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots
{
	internal static class GasGiantHelper
	{
		private static IEnumerable<Weighted<Orbits>> OrbitCountWeights
		{
			get
			{
				yield return new Weighted<Orbits>(Orbits.Zero, StarSystemVars.Instance.GasGiantOrbitsWeightZero);
				yield return new Weighted<Orbits>(Orbits.One, StarSystemVars.Instance.GasGiantOrbitsWeightOne);
				yield return new Weighted<Orbits>(Orbits.Two, StarSystemVars.Instance.GasGiantOrbitsWeightTwo);
				yield return new Weighted<Orbits>(Orbits.Three, StarSystemVars.Instance.GasGiantOrbitsWeightThree);
				yield return new Weighted<Orbits>(Orbits.Four, StarSystemVars.Instance.GasGiantOrbitsWeightFour);
				yield return new Weighted<Orbits>(Orbits.Ring, StarSystemVars.Instance.GasGiantOrbitsWeightRing);
				yield break;
			}
		}
		private static IEnumerable<Weighted<OrbitContents>> OrbitContentWeights
		{
			get
			{
				yield return new Weighted<OrbitContents>(OrbitContents.Empty, StarSystemVars.Instance.GasGiantSatelliteWeightNone);
				yield return new Weighted<OrbitContents>(OrbitContents.Artifact, StarSystemVars.Instance.GasGiantSatelliteWeightArtifact);
				yield return new Weighted<OrbitContents>(OrbitContents.Moon, StarSystemVars.Instance.GasGiantSatelliteWeightMoon);
				yield return new Weighted<OrbitContents>(OrbitContents.Planet, StarSystemVars.Instance.GasGiantSatelliteWeightPlanet);
				yield break;
			}
		}
		public static IEnumerable<Kerberos.Sots.Data.StarMapFramework.Orbit> ChooseOrbitContents(Random random)
		{
			Orbits orbits = WeightedChoices.Choose<Orbits>(random.NextDouble(), GasGiantHelper.OrbitCountWeights);
			int orbitCount = StarSystemHelper.GetOrbitCount(orbits);
			if (orbitCount >= 0)
			{
				for (int i = 1; i <= orbitCount; i++)
				{
					OrbitContents contents = WeightedChoices.Choose<OrbitContents>(random.NextDouble(), GasGiantHelper.OrbitContentWeights);
					Kerberos.Sots.Data.StarMapFramework.Orbit orbit = StarSystemHelper.CreateOrbiterParams(contents);
					orbit.OrbitNumber = i;
					yield return orbit;
				}
			}
			else
			{
				if (orbits == Orbits.Ring)
				{
					int orbitNumber = 1;
					Kerberos.Sots.Data.StarMapFramework.Orbit orbit2 = StarSystemHelper.CreateOrbiterParams(OrbitContents.PlanetaryRing);
					orbit2.OrbitNumber = orbitNumber;
					yield return orbit2;
				}
			}
			yield break;
		}
	}
}
