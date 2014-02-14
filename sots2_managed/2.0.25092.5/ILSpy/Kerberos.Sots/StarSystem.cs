using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots
{
	internal class StarSystem : ILegacyStarMapObject
	{
		private List<IStellarEntity> _objects = new List<IStellarEntity>();
		public int ID
		{
			get;
			set;
		}
		Feature ILegacyStarMapObject.Params
		{
			get
			{
				return this.Params;
			}
		}
		public Kerberos.Sots.Data.StarMapFramework.StarSystem Params
		{
			get;
			set;
		}
		public string DisplayName
		{
			get;
			set;
		}
		public Matrix WorldTransform
		{
			get;
			set;
		}
		public bool IsStartPosition
		{
			get;
			set;
		}
		public LegacyTerrain Terrain
		{
			get;
			set;
		}
		public IStellarEntity Star
		{
			get
			{
				return this.Objects.FirstOrDefault((IStellarEntity x) => x.Params is StarOrbit);
			}
		}
		public StellarClass StellarClass
		{
			get
			{
				return StellarClass.Parse((this.Star.Params as StarOrbit).StellarClass);
			}
		}
		public IEnumerable<IStellarEntity> Objects
		{
			get
			{
				return this._objects;
			}
		}
		public static StarSystemVars Vars
		{
			get
			{
				return StarSystemVars.Instance;
			}
		}
		public static bool TraceEnabled
		{
			get;
			set;
		}
		public void Add(IStellarEntity obj)
		{
			this._objects.Add(obj);
		}
		public void AddRange(IEnumerable<IStellarEntity> objs)
		{
			this._objects.AddRange(objs);
		}
		public IEnumerable<IStellarEntity> GetPlanets()
		{
			foreach (IStellarEntity current in 
				from x in this.Objects
				where x.Orbit != null && x.Orbit.Parent == this.Star.Params
				select x)
			{
				if (current.Params is PlanetOrbit || current.Params is GasGiantSmallOrbit || current.Params is GasGiantLargeOrbit)
				{
					yield return current;
				}
			}
			yield break;
		}
		public IEnumerable<IStellarEntity> GetAsteroidBelts()
		{
			foreach (IStellarEntity current in 
				from x in this.Objects
				where x.Orbit != null && x.Orbit.Parent == this.Star.Params
				select x)
			{
				if (current.Params is AsteroidOrbit)
				{
					yield return current;
				}
			}
			yield break;
		}
		public IEnumerable<IStellarEntity> GetColonizableWorlds(bool planetsOnly)
		{
			foreach (IStellarEntity current in this.Objects)
			{
				PlanetOrbit planetOrbit = current.Params as PlanetOrbit;
				if (planetOrbit != null && current.Orbit != null && (!planetsOnly || current.Orbit.Parent == this.Star.Params))
				{
					yield return current;
				}
			}
			yield break;
		}
		public IEnumerable<IStellarEntity> GetMoons(IStellarEntity planet)
		{
			foreach (IStellarEntity current in 
				from x in this.Objects
				where x.Orbit != null && x.Orbit.Parent == planet.Params
				select x)
			{
				if (current.Params is MoonOrbit || current.Params is PlanetOrbit)
				{
					yield return current;
				}
			}
			yield break;
		}
		internal static Orbit SetOrbit(Random random, Kerberos.Sots.Data.StarMapFramework.Orbit orbitParent, Kerberos.Sots.Data.StarMapFramework.Orbit orbiter)
		{
			if (orbiter.OrbitNumber < 1)
			{
				throw new ArgumentOutOfRangeException(string.Format("Orbit numbers start at 1.", new object[0]));
			}
			float eccentricity = orbiter.Eccentricity.HasValue ? orbiter.Eccentricity.Value : ((random == null) ? 0f : random.NextNormal(StarSystemVars.Instance.OrbitEccentricityRange));
			float inclination = orbiter.Inclination.HasValue ? orbiter.Inclination.Value : ((random == null) ? 0f : random.NextNormal(StarSystemVars.Instance.OrbitInclinationRange));
			float semiMajorAxis = StarSystemHelper.CalcOrbitRadius(orbitParent, orbiter.OrbitNumber);
			float semiMinorAxis = Ellipse.CalcSemiMinorAxis(semiMajorAxis, eccentricity);
			return new Orbit
			{
				Parent = orbitParent,
				SemiMajorAxis = semiMajorAxis,
				SemiMinorAxis = semiMinorAxis,
				OrbitNumber = orbiter.OrbitNumber,
				Inclination = inclination,
				Position = random.NextInclusive(0f, 1f)
			};
		}
		public static void Trace(string format, params object[] args)
		{
			if (StarSystem.TraceEnabled)
			{
				App.Log.Trace(string.Format(format, args), "StarSystem");
			}
		}
	}
}
