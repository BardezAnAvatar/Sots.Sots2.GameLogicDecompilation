using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.StarMapElements
{
	internal class LegacyStarMap
	{
		private readonly List<ILegacyStarMapObject> _objects = new List<ILegacyStarMapObject>();
		private readonly List<NodeLine> _nodelines = new List<NodeLine>();
		private readonly List<Province> _provinces = new List<Province>();
		private readonly List<LegacyTerrain> _terrain = new List<LegacyTerrain>();
		public IEnumerable<ILegacyStarMapObject> Objects
		{
			get
			{
				return this._objects;
			}
		}
		public IEnumerable<NodeLine> NodeLines
		{
			get
			{
				return this._nodelines;
			}
		}
		public IEnumerable<LegacyTerrain> Terrain
		{
			get
			{
				return this._terrain;
			}
		}
		public IEnumerable<PlanetOrbit> PlanetOrbits
		{
			get
			{
				foreach (Kerberos.Sots.StarSystem current in this._objects.OfType<Kerberos.Sots.StarSystem>())
				{
					foreach (IStellarEntity current2 in 
						from x in current.Objects
						where x.Params is PlanetOrbit
						select x)
					{
						yield return (PlanetOrbit)current2.Params;
					}
				}
				yield break;
			}
		}
		public void Add(ILegacyStarMapObject item)
		{
			this._objects.Add(item);
		}
		public void AddRange(IEnumerable<ILegacyStarMapObject> items)
		{
			this._objects.AddRange(items);
		}
		public static LegacyStarMap CreateStarMapFromFileCore(Random random, string starMapPath)
		{
			Starmap starmapParams = new Starmap();
			StarMapXmlUtility.LoadStarmapFromXml(starMapPath, ref starmapParams);
			return LegacyStarMap.CreateStarMap(random, starmapParams);
		}
		public static LegacyStarMap CreateStarMap(Random random, Starmap starmapParams)
		{
			LegacyStarMap legacyStarMap = new LegacyStarMap();
			foreach (Feature current in starmapParams.Features)
			{
				legacyStarMap.AddRange(LegacyStarMap.CreateFeature(random, current.LocalSpace, current, legacyStarMap, null));
			}
			legacyStarMap._nodelines.AddRange(starmapParams.NodeLines);
			legacyStarMap._provinces.AddRange(starmapParams.Provinces);
			legacyStarMap.FixPlanetTypes();
			return legacyStarMap;
		}
		private void FixPlanetTypes()
		{
			foreach (PlanetOrbit current in 
				from x in this.PlanetOrbits
				where !StellarBodyTypes.TerrestrialTypes.Contains(x.PlanetType.ToLowerInvariant())
				select x)
			{
				if (!string.IsNullOrWhiteSpace(current.PlanetType))
				{
					App.Log.Warn(string.Format("PlanetType '{0}' for planet '{1}' is invalid.", current.PlanetType, current.Name), "data");
				}
				current.PlanetType = StellarBodyTypes.Normal;
			}
		}
		public static IEnumerable<ILegacyStarMapObject> CreateFeature(Random random, Matrix worldTransform, Feature featureParams, LegacyStarMap map, LegacyTerrain parentTerrain)
		{
			if (featureParams.GetType() == typeof(Terrain))
			{
				return LegacyStarMap.CreateTerrain(random, worldTransform, featureParams as Terrain, map);
			}
			if (featureParams.GetType() == typeof(Kerberos.Sots.Data.StarMapFramework.StarSystem))
			{
				return LegacyStarMap.CreateStarSystem(random, worldTransform, featureParams as Kerberos.Sots.Data.StarMapFramework.StarSystem, parentTerrain);
			}
			if (featureParams.GetType() == typeof(StellarBody))
			{
				return LegacyStarMap.CreateStellarObject(random, worldTransform, featureParams as StellarBody);
			}
			throw new ArgumentException(string.Format("Unsupported starmap feature '{0}'.", featureParams.GetType()));
		}
		public static IEnumerable<ILegacyStarMapObject> CreateTerrain(Random random, Matrix worldTransform, Terrain terrainParams, LegacyStarMap map)
		{
			LegacyTerrain legacyTerrain = new LegacyTerrain();
			List<ILegacyStarMapObject> list = new List<ILegacyStarMapObject>();
			foreach (Feature current in terrainParams.Features)
			{
				Matrix matrix = current.LocalSpace;
				matrix *= worldTransform;
				list.AddRange(LegacyStarMap.CreateFeature(random, matrix, current, map, legacyTerrain));
			}
			legacyTerrain.Name = terrainParams.Name;
			legacyTerrain.Origin = new Vector3(worldTransform.M41, worldTransform.M42, worldTransform.M43);
			map._terrain.Add(legacyTerrain);
			map._nodelines.AddRange(terrainParams.NodeLines);
			map._provinces.AddRange(terrainParams.Provinces);
			return list;
		}
		public static IEnumerable<ILegacyStarMapObject> CreateStellarObject(Random random, Matrix worldTransform, StellarBody stellarBodyParams)
		{
			return new ILegacyStarMapObject[]
			{
				new StellarProp
				{
					Transform = worldTransform,
					Params = stellarBodyParams
				}
			};
		}
		public static IEnumerable<ILegacyStarMapObject> CreateStarSystem(Random random, Matrix worldTransform, Kerberos.Sots.Data.StarMapFramework.StarSystem systemParams, LegacyTerrain parentTerrain)
		{
			Kerberos.Sots.StarSystem starSystem = StarSystemHelper.CreateStarSystem(random, worldTransform, systemParams, parentTerrain);
			return new ILegacyStarMapObject[]
			{
				starSystem
			};
		}
		private static int CompareByOrbitNumber(IStellarEntity x, IStellarEntity y)
		{
			return x.Orbit.OrbitNumber.CompareTo(y.Orbit.OrbitNumber);
		}
		private static bool IsRandomOrbitName(Kerberos.Sots.Data.StarMapFramework.Orbit orbital)
		{
			return string.IsNullOrWhiteSpace(orbital.Name) || orbital.Name.Contains("NewOrbit") || orbital.Name.Contains("RandomOrbital");
		}
		internal void AssignEmptyPlanetTypes(Random random)
		{
			List<string> list = null;
			foreach (PlanetOrbit current in 
				from x in this.PlanetOrbits
				where x.PlanetType.ToLowerInvariant() == StellarBodyTypes.Normal
				select x)
			{
				int num = random.NextInclusive(0, 100);
				if (num <= 10)
				{
					if (list == null || list.Count == 0)
					{
						list = StellarBodyTypes.TerrestrialTypes.ToList<string>();
					}
					int index = random.NextInclusive(0, list.Count - 1);
					current.PlanetType = list[index];
					list.RemoveAt(index);
				}
			}
		}
		internal void AssignEmptySystemNames(Random random, NamesPool namesPool)
		{
			foreach (Kerberos.Sots.StarSystem current in this.Objects.OfType<Kerberos.Sots.StarSystem>())
			{
				if (string.IsNullOrWhiteSpace(current.DisplayName) || current.DisplayName.ToLower() == "random system")
				{
					current.DisplayName = namesPool.GetSystemName();
				}
				List<IStellarEntity> list = current.GetPlanets().ToList<IStellarEntity>();
				list.Sort(new Comparison<IStellarEntity>(LegacyStarMap.CompareByOrbitNumber));
				int num = 0;
				foreach (IStellarEntity current2 in list)
				{
					num++;
					if (LegacyStarMap.IsRandomOrbitName(current2.Params))
					{
						current2.Params.Name = string.Format("{0} {1}", current.DisplayName, num);
					}
					List<IStellarEntity> list2 = current.GetMoons(current2).ToList<IStellarEntity>();
					list2.Sort(new Comparison<IStellarEntity>(LegacyStarMap.CompareByOrbitNumber));
					int num2 = 0;
					foreach (IStellarEntity current3 in 
						from x in list2
						where LegacyStarMap.IsRandomOrbitName(x.Params)
						select x)
					{
						current3.Params.Name = string.Format("{0}{1}", current2.Params.Name, (char)(65 + num2));
						num2++;
					}
				}
				List<IStellarEntity> list3 = current.GetAsteroidBelts().ToList<IStellarEntity>();
				list3.Sort(new Comparison<IStellarEntity>(LegacyStarMap.CompareByOrbitNumber));
				int num3 = 0;
				foreach (IStellarEntity current4 in list3)
				{
					num3++;
					if (LegacyStarMap.IsRandomOrbitName(current4.Params))
					{
						current4.Params.Name = string.Format("{0} " + App.Localize("@UI_BELT_NAME_MOD") + " {1}", current.DisplayName, num3);
					}
				}
			}
		}
		internal void AssignEmptyPlanetParameters(Random random)
		{
			foreach (Kerberos.Sots.StarSystem current in this.Objects.OfType<Kerberos.Sots.StarSystem>())
			{
				foreach (IStellarEntity current2 in current.Objects)
				{
					if (current2.Params is PlanetOrbit)
					{
						PlanetOrbit planetOrbit = current2.Params as PlanetOrbit;
						if (!planetOrbit.Suitability.HasValue)
						{
							planetOrbit.Suitability = new float?(StarSystemHelper.ChoosePlanetSuitability(random));
						}
						if (!planetOrbit.Resources.HasValue)
						{
							planetOrbit.Resources = new int?((int)StarSystemHelper.ChoosePlanetResources(random));
						}
						if (!planetOrbit.Biosphere.HasValue)
						{
							planetOrbit.Biosphere = new int?((int)StarSystemHelper.ChoosePlanetBiosphere(random));
						}
						if (!planetOrbit.Size.HasValue)
						{
							if (current2.Orbit != null && current2.Orbit.Parent == current.Star.Params)
							{
								planetOrbit.Size = new int?((int)StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.PlanetMinRadius, StarSystemVars.Instance.PlanetMaxRadius));
							}
							else
							{
								planetOrbit.Size = new int?(random.NextInclusive(StarSystemVars.Instance.HabitalMoonMinSize, StarSystemVars.Instance.HabitalMoonMaxSize));
							}
						}
					}
					else
					{
						if (current2.Params is GasGiantSmallOrbit)
						{
							GasGiantSmallOrbit gasGiantSmallOrbit = current2.Params as GasGiantSmallOrbit;
							if (!gasGiantSmallOrbit.Size.HasValue)
							{
								gasGiantSmallOrbit.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.GasGiantMinRadiusSmall, StarSystemVars.Instance.GasGiantMaxRadiusSmall));
							}
						}
						else
						{
							if (current2.Params is GasGiantLargeOrbit)
							{
								GasGiantLargeOrbit gasGiantLargeOrbit = current2.Params as GasGiantLargeOrbit;
								if (!gasGiantLargeOrbit.Size.HasValue)
								{
									gasGiantLargeOrbit.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.GasGiantMinRadiusLarge, StarSystemVars.Instance.GasGiantMaxRadiusLarge));
								}
							}
							else
							{
								if (current2.Params is MoonOrbit)
								{
									MoonOrbit moonOrbit = current2.Params as MoonOrbit;
									if (!moonOrbit.Size.HasValue)
									{
										moonOrbit.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.MoonMinRadius, StarSystemVars.Instance.MoonMaxRadius));
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
