using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots
{
	internal static class StarSystemHelper
	{
		private static float CalcRadius(Kerberos.Sots.Data.StarMapFramework.Orbit orbital)
		{
			if (orbital is StarOrbit)
			{
				StellarClass stellarClass = StellarClass.Parse((orbital as StarOrbit).StellarClass);
				return StarSystemVars.Instance.StarRadius(stellarClass.Size);
			}
			PlanetInfo planetInfo = StarSystemHelper.InferPlanetInfo(orbital);
			if (planetInfo != null)
			{
				return StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
			}
			throw new ArgumentException("unexpected type");
		}
		public static float CalcOrbitStep(IStellarEntity orbitParent)
		{
			return StarSystemHelper.CalcOrbitStep(orbitParent.Params);
		}
		private static float CalcOrbitStep(Kerberos.Sots.Data.StarMapFramework.Orbit orbital)
		{
			if (orbital is StarOrbit)
			{
				return StarSystemVars.Instance.StarOrbitStep;
			}
			if (orbital is GasGiantLargeOrbit)
			{
				return StarSystemVars.Instance.GasGiantOrbitStep;
			}
			if (orbital is GasGiantSmallOrbit)
			{
				return StarSystemVars.Instance.GasGiantOrbitStep;
			}
			if (orbital is PlanetOrbit)
			{
				return StarSystemVars.Instance.PlanetOrbitStep;
			}
			throw new ArgumentException("unexpected type");
		}
		public static float CalcOrbitRadius(Kerberos.Sots.Data.StarMapFramework.Orbit orbitParent, int orbitNumber)
		{
			float parentRadius = StarSystemHelper.CalcRadius(orbitParent);
			float orbitStep = StarSystemHelper.CalcOrbitStep(orbitParent);
			return Orbit.CalcOrbitRadius(parentRadius, orbitStep, orbitNumber);
		}
		public static float ChoosePlanetSuitability(Random random)
		{
			return random.NextInclusive(Constants.MinSuitability, Constants.MaxSuitability);
		}
		public static PlanetInfo InferPlanetInfo(Kerberos.Sots.Data.StarMapFramework.Orbit orbit)
		{
			if (orbit is PlanetOrbit)
			{
				PlanetOrbit planetOrbit = orbit as PlanetOrbit;
				Random safeRandom = App.GetSafeRandom();
				float size = (float)(planetOrbit.Size.HasValue ? planetOrbit.Size.Value : safeRandom.Next(1, 10));
				string type = (!string.IsNullOrEmpty(planetOrbit.PlanetType)) ? planetOrbit.PlanetType : ((safeRandom.NextNormal(0.0, 1.0) > 0.75) ? StellarBodyTypes.Normal : StellarBodyTypes.SpecialTerrestrialTypes[safeRandom.Next(0, StellarBodyTypes.SpecialTerrestrialTypes.Count<string>())]);
				float suitability = planetOrbit.Suitability.HasValue ? planetOrbit.Suitability.Value : ((float)safeRandom.Next(-1000, 1000));
				int biosphere = planetOrbit.Biosphere.HasValue ? planetOrbit.Biosphere.Value : safeRandom.Next(500, 1500);
				int resources = planetOrbit.Resources.HasValue ? planetOrbit.Resources.Value : safeRandom.Next(1500, 8000);
				return new PlanetInfo
				{
					Size = size,
					Type = type,
					Suitability = suitability,
					Biosphere = biosphere,
					Resources = resources
				};
			}
			if (orbit is MoonOrbit)
			{
				MoonOrbit moonOrbit = orbit as MoonOrbit;
				Random safeRandom2 = App.GetSafeRandom();
				float size2 = moonOrbit.Size.HasValue ? moonOrbit.Size.Value : (0.1f + safeRandom2.NextSingle() * 0.4f);
				int resources2 = safeRandom2.Next(1000, 7000);
				return new PlanetInfo
				{
					Size = size2,
					Resources = resources2,
					Type = StellarBodyTypes.Barren
				};
			}
			if (orbit is GasGiantSmallOrbit)
			{
				GasGiantSmallOrbit gasGiantSmallOrbit = orbit as GasGiantSmallOrbit;
				Random safeRandom3 = App.GetSafeRandom();
				float size3 = gasGiantSmallOrbit.Size.HasValue ? gasGiantSmallOrbit.Size.Value : ((float)safeRandom3.Next(13, 18));
				return new PlanetInfo
				{
					Size = size3,
					Type = StellarBodyTypes.Gaseous
				};
			}
			if (orbit is GasGiantLargeOrbit)
			{
				GasGiantLargeOrbit gasGiantLargeOrbit = orbit as GasGiantLargeOrbit;
				Random safeRandom4 = App.GetSafeRandom();
				if (!gasGiantLargeOrbit.Size.HasValue)
				{
					safeRandom4.Next(19, 30);
				}
				else
				{
					float arg_293_0 = gasGiantLargeOrbit.Size.Value;
				}
				return new PlanetInfo
				{
					Size = gasGiantLargeOrbit.Size.Value,
					Type = StellarBodyTypes.Gaseous
				};
			}
			return null;
		}
		public static float ChooseSize(Random random, float minRadius, float maxRadius)
		{
			float num = 1f;
			float num2 = (minRadius + maxRadius) / 2f;
			num2 *= num;
			float radius = (float)random.NextNormal((double)minRadius, (double)maxRadius, (double)num2);
			return StarSystemVars.Instance.RadiusToSize(radius);
		}
		public static float ChoosePlanetResources(Random random)
		{
			int num = 5000;
			num = (int)((float)num * 1f);
			int num2 = (int)random.NextNormal(2000.0, 8000.0, (double)num);
			return (float)num2;
		}
		public static float ChoosePlanetBiosphere(Random random)
		{
			int num = 4250;
			num = (int)((float)num * 1f);
			int num2 = (int)random.NextNormal(500.0, 8000.0, (double)num);
			return (float)num2;
		}
		private static List<Kerberos.Sots.Data.StarMapFramework.Orbit> PopulateRandomOrbitsCore(Random random, Kerberos.Sots.Data.StarMapFramework.Orbit parentOrbital)
		{
			if (parentOrbital is StarOrbit)
			{
				StarOrbit starOrbit = parentOrbital as StarOrbit;
				StellarClass stellarClass = StellarClass.Parse(starOrbit.StellarClass);
				return StarHelper.ChooseOrbitContents(random, stellarClass).ToList<Kerberos.Sots.Data.StarMapFramework.Orbit>();
			}
			if (parentOrbital is GasGiantLargeOrbit)
			{
				return GasGiantHelper.ChooseOrbitContents(random).ToList<Kerberos.Sots.Data.StarMapFramework.Orbit>();
			}
			if (parentOrbital is GasGiantSmallOrbit)
			{
				return GasGiantHelper.ChooseOrbitContents(random).ToList<Kerberos.Sots.Data.StarMapFramework.Orbit>();
			}
			return new List<Kerberos.Sots.Data.StarMapFramework.Orbit>();
		}
		private static List<Kerberos.Sots.Data.StarMapFramework.Orbit> PopulateRandomOrbits(Random random, Kerberos.Sots.Data.StarMapFramework.Orbit parentOrbital)
		{
			List<Kerberos.Sots.Data.StarMapFramework.Orbit> list = StarSystemHelper.PopulateRandomOrbitsCore(random, parentOrbital);
			list.ForEach(delegate(Kerberos.Sots.Data.StarMapFramework.Orbit x)
			{
				x.Parent = parentOrbital.Name;
			});
			return list;
		}
		private static IStellarEntity FindOrbitParent(StarSystem system, IStellarEntity orbiter)
		{
			if (!system.Objects.Contains(orbiter))
			{
				throw new ArgumentOutOfRangeException("System does not contain orbiter.");
			}
			if (orbiter.Params == null)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(orbiter.Params.Parent))
			{
				return system.Objects.FirstOrDefault((IStellarEntity x) => x.Params != null && x.Params.Name == orbiter.Params.Parent);
			}
			if (orbiter.Params is StarOrbit)
			{
				return null;
			}
			return system.Star;
		}
		private static void AssignOrbitNumbers(StellarClass stellarClass, List<Kerberos.Sots.Data.StarMapFramework.Orbit> orbitals)
		{
			int orbitNumber2 = StarHelper.CalcMinOrbit(stellarClass);
			int orbitNumber = orbitNumber2;
			foreach (Kerberos.Sots.Data.StarMapFramework.Orbit current in 
				from x in orbitals
				where x.OrbitNumber < 1
				select x)
			{
				while (true)
				{
					if (!orbitals.Any((Kerberos.Sots.Data.StarMapFramework.Orbit x) => x.OrbitNumber == orbitNumber))
					{
						break;
					}
					orbitNumber++;
				}
				current.OrbitNumber = orbitNumber;
				orbitNumber++;
			}
		}
		public static StarSystem CreateStarSystem(Random random, Matrix worldTransform, Kerberos.Sots.Data.StarMapFramework.StarSystem systemParams, LegacyTerrain parentTerrain)
		{
			StellarClass stellarClass = StarHelper.ResolveStellarClass(random, systemParams.Type, systemParams.SubType, systemParams.Size);
			StarOrbit starOrbit = new StarOrbit();
			starOrbit.Name = systemParams.Name;
			starOrbit.StellarClass = stellarClass.ToString();
			int randomOrbital = 1;
			List<Kerberos.Sots.Data.StarMapFramework.Orbit> list = new List<Kerberos.Sots.Data.StarMapFramework.Orbit>();
			list.Add(starOrbit);
			for (int i = 0; i < list.Count; i++)
			{
				Kerberos.Sots.Data.StarMapFramework.Orbit thisOrbital = list[i];
				if (!(thisOrbital is EmptyOrbit))
				{
					List<Kerberos.Sots.Data.StarMapFramework.Orbit> predefinedOrbitals = new List<Kerberos.Sots.Data.StarMapFramework.Orbit>();
					predefinedOrbitals.AddRange(systemParams.Orbits.Where(delegate(Kerberos.Sots.Data.StarMapFramework.Orbit x)
					{
						if (string.IsNullOrEmpty(x.Parent))
						{
							return thisOrbital is StarOrbit;
						}
						return x.Parent == thisOrbital.Name;
					}));
					if (thisOrbital is StarOrbit)
					{
						StarSystemHelper.AssignOrbitNumbers(stellarClass, predefinedOrbitals);
					}
					else
					{
						int orbitNumber = 1;
						predefinedOrbitals.ForEach(delegate(Kerberos.Sots.Data.StarMapFramework.Orbit x)
						{
							x.OrbitNumber = orbitNumber++;
						});
					}
					List<Kerberos.Sots.Data.StarMapFramework.Orbit> list2 = StarSystemHelper.PopulateRandomOrbits(random, thisOrbital);
					list2.RemoveAll((Kerberos.Sots.Data.StarMapFramework.Orbit x) => predefinedOrbitals.Any((Kerberos.Sots.Data.StarMapFramework.Orbit y) => y.OrbitNumber == x.OrbitNumber));
					list2.ForEach(delegate(Kerberos.Sots.Data.StarMapFramework.Orbit x)
					{
						x.Name = string.Format("RandomOrbital{0}", ++randomOrbital);
					});
					list.AddRange(predefinedOrbitals);
					list.AddRange(list2);
				}
			}
			StarSystem starSystem = new StarSystem();
			starSystem.Params = systemParams;
			starSystem.WorldTransform = worldTransform;
			starSystem.DisplayName = systemParams.Name;
			starSystem.IsStartPosition = systemParams.isStartLocation;
			starSystem.WorldTransform = worldTransform;
			starSystem.Terrain = parentTerrain;
			foreach (Kerberos.Sots.Data.StarMapFramework.Orbit current in list)
			{
				bool isOrbitingStar = current.Parent == starOrbit.Name;
				List<IStellarEntity> objs = new List<IStellarEntity>(StarSystemHelper.CreateOrbiters(random, current, isOrbitingStar));
				starSystem.AddRange(objs);
			}
			foreach (IStellarEntity current2 in starSystem.Objects)
			{
				IStellarEntity stellarEntity = StarSystemHelper.FindOrbitParent(starSystem, current2);
				if (stellarEntity != null)
				{
					current2.Orbit = StarSystem.SetOrbit(random, stellarEntity.Params, current2.Params);
				}
			}
			return starSystem;
		}
		public static IEnumerable<IStellarEntity> CreateOrbiters(Random random, Kerberos.Sots.Data.StarMapFramework.Orbit orbiterParams, bool isOrbitingStar)
		{
			if (orbiterParams.GetType() == typeof(EmptyOrbit))
			{
				return Enumerable.Empty<IStellarEntity>();
			}
			if (orbiterParams.GetType() == typeof(StarOrbit))
			{
				return StarSystemHelper.CreateStar(random, orbiterParams as StarOrbit);
			}
			if (orbiterParams.GetType() == typeof(ArtifactOrbit))
			{
				return StarSystemHelper.CreateArtifact(random, orbiterParams as ArtifactOrbit);
			}
			if (orbiterParams.GetType() == typeof(GasGiantSmallOrbit))
			{
				return StarSystemHelper.CreateGasGiantSmall(random, orbiterParams as GasGiantSmallOrbit);
			}
			if (orbiterParams.GetType() == typeof(GasGiantLargeOrbit))
			{
				return StarSystemHelper.CreateGasGiantLarge(random, orbiterParams as GasGiantLargeOrbit);
			}
			if (orbiterParams.GetType() == typeof(MoonOrbit))
			{
				return StarSystemHelper.CreateMoon(random, orbiterParams as MoonOrbit);
			}
			if (orbiterParams.GetType() == typeof(PlanetaryRingOrbit))
			{
				return StarSystemHelper.CreatePlanetaryRing(random, orbiterParams as PlanetaryRingOrbit);
			}
			if (orbiterParams.GetType() == typeof(PlanetOrbit))
			{
				return StarSystemHelper.CreatePlanet(random, orbiterParams as PlanetOrbit, isOrbitingStar);
			}
			if (orbiterParams.GetType() == typeof(AsteroidOrbit))
			{
				return StarSystemHelper.CreateAsteroidBelt(random, orbiterParams as AsteroidOrbit);
			}
			throw new ArgumentException(string.Format("Unsupported orbit type '{0}'.", orbiterParams.GetType()));
		}
		public static IEnumerable<IStellarEntity> CreateStar(Random random, StarOrbit orbiterParams)
		{
			StellarClass.Parse(orbiterParams.StellarClass);
			return new IStellarEntity[]
			{
				new StellarEntity
				{
					Params = orbiterParams
				}
			};
		}
		public static IEnumerable<IStellarEntity> CreateArtifact(Random random, ArtifactOrbit orbiterParams)
		{
			return new IStellarEntity[]
			{
				new StellarEntity
				{
					Params = orbiterParams
				}
			};
		}
		public static IEnumerable<IStellarEntity> CreateGasGiantSmall(Random random, GasGiantSmallOrbit orbiterParams)
		{
			if (!orbiterParams.Size.HasValue)
			{
				orbiterParams.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.GasGiantMinRadiusSmall, StarSystemVars.Instance.GasGiantMaxRadiusSmall));
			}
			return new IStellarEntity[]
			{
				new StellarEntity
				{
					Params = orbiterParams
				}
			};
		}
		public static IEnumerable<IStellarEntity> CreateGasGiantLarge(Random random, GasGiantLargeOrbit orbiterParams)
		{
			if (!orbiterParams.Size.HasValue)
			{
				orbiterParams.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.GasGiantMinRadiusLarge, StarSystemVars.Instance.GasGiantMaxRadiusLarge));
			}
			return new IStellarEntity[]
			{
				new StellarEntity
				{
					Params = orbiterParams
				}
			};
		}
		public static IEnumerable<IStellarEntity> CreateMoon(Random random, MoonOrbit orbiterParams)
		{
			if (!orbiterParams.Size.HasValue)
			{
				orbiterParams.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.MoonMinRadius, StarSystemVars.Instance.MoonMaxRadius));
			}
			return new IStellarEntity[]
			{
				new StellarEntity
				{
					Params = orbiterParams
				}
			};
		}
		public static IEnumerable<IStellarEntity> CreatePlanetaryRing(Random random, PlanetaryRingOrbit orbiterParams)
		{
			return new IStellarEntity[]
			{
				new StellarEntity
				{
					Params = orbiterParams
				}
			};
		}
		public static IEnumerable<IStellarEntity> CreatePlanet(Random random, PlanetOrbit orbiterParams, bool isOrbitingStar)
		{
			if (!orbiterParams.Size.HasValue)
			{
				if (isOrbitingStar)
				{
					orbiterParams.Size = new int?((int)StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.PlanetMinRadius, StarSystemVars.Instance.PlanetMaxRadius));
				}
				else
				{
					orbiterParams.Size = new int?(random.NextInclusive(StarSystemVars.Instance.HabitalMoonMinSize, StarSystemVars.Instance.HabitalMoonMaxSize));
				}
			}
			return new IStellarEntity[]
			{
				new StellarEntity
				{
					Params = orbiterParams
				}
			};
		}
		public static IEnumerable<IStellarEntity> CreateAsteroidBelt(Random random, AsteroidOrbit orbiterParams)
		{
			return new IStellarEntity[]
			{
				new StellarEntity
				{
					Params = orbiterParams
				}
			};
		}
		public static int GetOrbitCount(Orbits orbits)
		{
			return Math.Max(0, (int)orbits);
		}
		public static Kerberos.Sots.Data.StarMapFramework.Orbit CreateOrbiterParams(OrbitContents contents)
		{
			switch (contents)
			{
			case OrbitContents.Empty:
				return new EmptyOrbit();
			case OrbitContents.Artifact:
				return new ArtifactOrbit();
			case OrbitContents.Planet:
				return new PlanetOrbit();
			case OrbitContents.GasGiantLarge:
				return new GasGiantLargeOrbit();
			case OrbitContents.GasGiantSmall:
				return new GasGiantSmallOrbit();
			case OrbitContents.AsteroidBelt:
				return new AsteroidOrbit();
			case OrbitContents.Moon:
				return new MoonOrbit();
			case OrbitContents.PlanetaryRing:
				return new PlanetaryRingOrbit();
			default:
				throw new ArgumentOutOfRangeException(string.Format("Unhandled OrbitContents.{0}", contents));
			}
		}
		internal static void VerifyStarMap(LegacyStarMap starmap)
		{
			foreach (StarSystem current in starmap.Objects.OfType<StarSystem>())
			{
				List<PlanetOrbit> list = (
					from x in current.Objects
					where x.Params is PlanetOrbit
					select (PlanetOrbit)x.Params).ToList<PlanetOrbit>();
				foreach (PlanetOrbit arg_7D_0 in list)
				{
				}
			}
		}
	}
}
