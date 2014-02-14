using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots
{
	internal static class StarHelper
	{
		private static readonly KeyValuePair<StellarType, Vector4>[] IconColorTable = new KeyValuePair<StellarType, Vector4>[]
		{
			new KeyValuePair<StellarType, Vector4>(StellarType.O, new Vector4(0f, 0.75f, 1f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.B, new Vector4(0.5f, 0.75f, 1f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.A, new Vector4(1f, 1f, 1f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.F, new Vector4(1f, 1f, 0.65f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.G, new Vector4(1f, 1f, 0f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.K, new Vector4(1f, 0.5f, 0f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.M, new Vector4(0.9f, 0f, 0f, 1f))
		};
		private static readonly KeyValuePair<StellarType, Vector4>[] ModelColorTable = new KeyValuePair<StellarType, Vector4>[]
		{
			new KeyValuePair<StellarType, Vector4>(StellarType.O, new Vector4(0.3f, 0.4f, 0.5f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.B, new Vector4(0.45f, 0.5f, 0.5f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.A, new Vector4(0.5f, 0.5f, 0.5f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.F, new Vector4(0.5f, 0.5f, 0.45f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.G, new Vector4(0.5f, 0.5f, 0.3f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.K, new Vector4(0.5f, 0.45f, 0f, 1f)),
			new KeyValuePair<StellarType, Vector4>(StellarType.M, new Vector4(0.5f, 0.4f, 0.4f, 1f))
		};
		private static Dictionary<StellarType, StarDisplayParams> _displayParams = StarHelper.CreateDisplayParams();
		private static IEnumerable<Weighted<StellarType>> StarTypeWeights
		{
			get
			{
				yield return new Weighted<StellarType>(StellarType.O, StarSystem.Vars.StarTypeWeightO);
				yield return new Weighted<StellarType>(StellarType.B, StarSystem.Vars.StarTypeWeightB);
				yield return new Weighted<StellarType>(StellarType.A, StarSystem.Vars.StarTypeWeightA);
				yield return new Weighted<StellarType>(StellarType.F, StarSystem.Vars.StarTypeWeightF);
				yield return new Weighted<StellarType>(StellarType.G, StarSystem.Vars.StarTypeWeightG);
				yield return new Weighted<StellarType>(StellarType.K, StarSystem.Vars.StarTypeWeightK);
				yield return new Weighted<StellarType>(StellarType.M, StarSystem.Vars.StarTypeWeightM);
				yield break;
			}
		}
		private static IEnumerable<Weighted<StellarSize>> StarSizeWeights
		{
			get
			{
				yield return new Weighted<StellarSize>(StellarSize.Ia, StarSystem.Vars.StarSizeWeightIa);
				yield return new Weighted<StellarSize>(StellarSize.Ib, StarSystem.Vars.StarSizeWeightIb);
				yield return new Weighted<StellarSize>(StellarSize.II, StarSystem.Vars.StarSizeWeightII);
				yield return new Weighted<StellarSize>(StellarSize.III, StarSystem.Vars.StarSizeWeightIII);
				yield return new Weighted<StellarSize>(StellarSize.IV, StarSystem.Vars.StarSizeWeightIV);
				yield return new Weighted<StellarSize>(StellarSize.V, StarSystem.Vars.StarSizeWeightV);
				yield return new Weighted<StellarSize>(StellarSize.VI, StarSystem.Vars.StarSizeWeightVI);
				yield return new Weighted<StellarSize>(StellarSize.VII, StarSystem.Vars.StarSizeWeightVII);
				yield break;
			}
		}
		private static IEnumerable<Weighted<Orbits>> OrbitCountWeights
		{
			get
			{
				yield return new Weighted<Orbits>(Orbits.Zero, StarSystem.Vars.StarOrbitsWeightZero);
				yield return new Weighted<Orbits>(Orbits.One, StarSystem.Vars.StarOrbitsWeightOne);
				yield return new Weighted<Orbits>(Orbits.Two, StarSystem.Vars.StarOrbitsWeightTwo);
				yield return new Weighted<Orbits>(Orbits.Three, StarSystem.Vars.StarOrbitsWeightThree);
				yield return new Weighted<Orbits>(Orbits.Four, StarSystem.Vars.StarOrbitsWeightFour);
				yield return new Weighted<Orbits>(Orbits.Five, StarSystem.Vars.StarOrbitsWeightFive);
				yield return new Weighted<Orbits>(Orbits.Six, StarSystem.Vars.StarOrbitsWeightSix);
				yield break;
			}
		}
		private static IEnumerable<Weighted<OrbitContents>> OrbitContentWeights
		{
			get
			{
				yield return new Weighted<OrbitContents>(OrbitContents.Empty, StarSystemVars.Instance.StarSatelliteWeightNone);
				yield return new Weighted<OrbitContents>(OrbitContents.Planet, StarSystemVars.Instance.StarSatelliteWeightPlanet);
				yield return new Weighted<OrbitContents>(OrbitContents.AsteroidBelt, StarSystemVars.Instance.StarSatelliteWeightAsteroidBelt);
				yield return new Weighted<OrbitContents>(OrbitContents.GasGiantSmall, StarSystemVars.Instance.StarSatelliteWeightGasGiantSmall);
				yield return new Weighted<OrbitContents>(OrbitContents.GasGiantLarge, StarSystemVars.Instance.StarSatelliteWeightGasGiantLarge);
				yield break;
			}
		}
		public static StellarClass ResolveStellarClass(Random random, string typeStr, string subTypeStr, string sizeStr)
		{
			StellarType type;
			if (!StellarClass.TryParseType(typeStr, out type))
			{
				type = StarHelper.ChooseStellarType(random);
			}
			int subtype;
			if (!StellarClass.TryParseSubType(subTypeStr, out subtype))
			{
				subtype = StarHelper.ChooseStellarSubType(random);
			}
			StellarSize size;
			if (!StellarClass.TryParseSize(sizeStr, out size))
			{
				size = StarHelper.ChooseStellarSize(random, type, subtype);
			}
			return new StellarClass(type, subtype, size);
		}
		public static StellarType ChooseStellarType(Random random)
		{
			return WeightedChoices.Choose<StellarType>(random, StarHelper.StarTypeWeights);
		}
		public static int ChooseStellarSubType(Random random)
		{
			Dice dice = new Dice("1D10+-1");
			return dice.Roll(random);
		}
		public static StellarSize ChooseStellarSize(Random random, StellarType type, int subtype)
		{
			StellarSize stellarSize = WeightedChoices.Choose<StellarSize>(random, StarHelper.StarSizeWeights);
			if (stellarSize == StellarSize.IV && type >= StellarType.K && subtype >= 5 && type <= StellarType.M && subtype <= 9)
			{
				StarSystem.Trace("Special Rule: IF Star is type K5 through M9, THEN treat result IV as V.", new object[0]);
				stellarSize = StellarSize.V;
			}
			if (stellarSize == StellarSize.VI && type >= StellarType.B && subtype >= 0 && type <= StellarType.F && subtype <= 4)
			{
				StarSystem.Trace("Special Rule: IF Star is type B0 through F4, THEN treat result VI as V", new object[0]);
				stellarSize = StellarSize.V;
			}
			return stellarSize;
		}
		public static StellarClass ChooseStellarClass(Random random)
		{
			StellarType type = StarHelper.ChooseStellarType(random);
			int subtype = StarHelper.ChooseStellarSubType(random);
			StellarSize size = StarHelper.ChooseStellarSize(random, type, subtype);
			StellarClass result = new StellarClass(type, subtype, size);
			return result;
		}
		public static float CalcRadius(StellarSize size)
		{
			return StarSystem.Vars.StarRadius(size);
		}
		private static Vector4 CalcColor(StellarType type, KeyValuePair<StellarType, Vector4>[] colorTable)
		{
			return colorTable.First((KeyValuePair<StellarType, Vector4> x) => x.Key == type).Value;
		}
		private static Vector4 CalcColor(StellarType type, int subtype, KeyValuePair<StellarType, Vector4>[] colorTable)
		{
			Vector4 v = StarHelper.CalcColor(type, colorTable);
			StellarType type2 = (StellarType)Math.Min(6, (int)(type + 1));
			Vector4 v2 = StarHelper.CalcColor(type2, colorTable);
			float t = (float)subtype / 10f;
			return Vector4.Lerp(v, v2, t);
		}
		public static Vector4 CalcIconColor(StellarClass stellarClass)
		{
			return StarHelper.CalcColor(stellarClass.Type, stellarClass.SubType, StarHelper.IconColorTable);
		}
		public static Vector4 CalcModelColor(StellarClass stellarClass)
		{
			return StarHelper.CalcColor(stellarClass.Type, stellarClass.SubType, StarHelper.ModelColorTable);
		}
		private static int ChooseOrbitCount(Random random, StellarClass stellarClass)
		{
			Orbits orbits = WeightedChoices.Choose<Orbits>(random, StarHelper.OrbitCountWeights);
			int num = Math.Max(0, (int)orbits);
			int num2 = num;
			if (num2 != (int)orbits)
			{
				throw new NotImplementedException(string.Format("Orbits.{0} not handled for Stars", orbits));
			}
			switch (stellarClass.Size)
			{
			case StellarSize.Ia:
			case StellarSize.Ib:
			case StellarSize.II:
				StarSystem.Trace("Special Rule: If Star Size I or II THEN # of orbits = # of orbits + 4", new object[0]);
				num2 += 4;
				break;
			case StellarSize.III:
				StarSystem.Trace("Special Rule: If Star Size III THEN # of orbits = # of orbits + 2", new object[0]);
				num2 += 2;
				break;
			}
			switch (stellarClass.Type)
			{
			case StellarType.K:
				StarSystem.Trace("Special Rule: If Star Type K THEN # of orbits = # of orbits - 1", new object[0]);
				num2--;
				break;
			case StellarType.M:
				StarSystem.Trace("Special Rule: If Star Type M THEN # of orbits = # of orbits - 2", new object[0]);
				num2 -= 2;
				break;
			}
			num2 = Math.Max(0, num2);
			StarSystem.Trace("Final orbit count: {0}\n", new object[]
			{
				num2
			});
			if (num2 == 0 && num > 0)
			{
				return 1;
			}
			return num2;
		}
		private static int CalcMinOrbitCore(StellarClass value)
		{
			switch (value.Size)
			{
			case StellarSize.Ia:
				if (StellarClass.Contains("B0", "B4", value))
				{
					return 8;
				}
				if (StellarClass.Contains("B5", "A9", value))
				{
					return 7;
				}
				if (StellarClass.Contains("F0", "F9", value))
				{
					return 6;
				}
				if (StellarClass.Contains("G0", "M4", value))
				{
					return 7;
				}
				if (StellarClass.Contains("M5", "M9", value))
				{
					return 8;
				}
				break;
			case StellarSize.Ib:
				if (StellarClass.Contains("B0", "B4", value))
				{
					return 8;
				}
				if (StellarClass.Contains("B5", "B9", value))
				{
					return 6;
				}
				if (StellarClass.Contains("A0", "F4", value))
				{
					return 5;
				}
				if (StellarClass.Contains("F5", "G4", value))
				{
					return 4;
				}
				if (StellarClass.Contains("G5", "K4", value))
				{
					return 5;
				}
				if (StellarClass.Contains("K5", "M4", value))
				{
					return 6;
				}
				if (StellarClass.Contains("M5", "M8", value))
				{
					return 7;
				}
				if (StellarClass.Contains("M9", "M9", value))
				{
					return 8;
				}
				break;
			case StellarSize.II:
				if (StellarClass.Contains("B0", "B4", value))
				{
					return 7;
				}
				if (StellarClass.Contains("B5", "B9", value))
				{
					return 5;
				}
				if (StellarClass.Contains("A0", "A4", value))
				{
					return 3;
				}
				if (StellarClass.Contains("A5", "K4", value))
				{
					return 2;
				}
				if (StellarClass.Contains("K5", "K9", value))
				{
					return 3;
				}
				if (StellarClass.Contains("M0", "M4", value))
				{
					return 4;
				}
				if (StellarClass.Contains("M5", "M9", value))
				{
					return 6;
				}
				break;
			case StellarSize.III:
				if (StellarClass.Contains("B0", "B4", value))
				{
					return 7;
				}
				if (StellarClass.Contains("B5", "B9", value))
				{
					return 5;
				}
				if (StellarClass.Contains("M0", "M4", value))
				{
					return 2;
				}
				if (StellarClass.Contains("M5", "M8", value))
				{
					return 4;
				}
				if (StellarClass.Contains("M9", "M9", value))
				{
					return 5;
				}
				break;
			case StellarSize.IV:
				if (StellarClass.Contains("B0", "B4", value))
				{
					return 7;
				}
				if (StellarClass.Contains("B5", "B9", value))
				{
					return 3;
				}
				break;
			case StellarSize.V:
				if (StellarClass.Contains("B0", "B4", value))
				{
					return 6;
				}
				if (StellarClass.Contains("B5", "B9", value))
				{
					return 3;
				}
				break;
			}
			return 1;
		}
		public static int CalcMinOrbit(StellarClass value)
		{
			int num = StarHelper.CalcMinOrbitCore(value);
			if (num > 1)
			{
				StarSystem.Trace("Invalidating orbits below {0} due to size and heat of {1} star.", new object[]
				{
					num,
					value
				});
			}
			return num;
		}
		private static Range<int> ChooseOrbits(Random random, StellarClass stellarClass)
		{
			Range<int> result = new Range<int>(0, 0);
			int num = StarHelper.ChooseOrbitCount(random, stellarClass);
			if (num <= 0)
			{
				return result;
			}
			int num2 = num;
			int num3 = StarHelper.CalcMinOrbit(stellarClass);
			if (num3 > num2)
			{
				return result;
			}
			Range<int> result2 = new Range<int>(num3, num2);
			return result2;
		}
		private static float CalcOrbitContentRoll(Random random, int orbitNumber)
		{
			double num = (double)orbitNumber * 0.1 - 0.35 + random.NextDouble();
			return (float)num;
		}
		public static IEnumerable<Kerberos.Sots.Data.StarMapFramework.Orbit> ChooseOrbitContents(Random random, StellarClass stellarClass)
		{
			Range<int> range = StarHelper.ChooseOrbits(random, stellarClass);
			int num = 0;
			for (int i = 1; i <= range.Max; i++)
			{
				OrbitContents orbitContents = OrbitContents.Empty;
				if (i >= range.Min)
				{
					bool flag = false;
					while (!flag)
					{
						flag = true;
						float num2 = StarHelper.CalcOrbitContentRoll(random, i);
						orbitContents = WeightedChoices.Choose<OrbitContents>((double)num2, StarHelper.OrbitContentWeights);
						if (orbitContents == OrbitContents.AsteroidBelt && num >= 2)
						{
							flag = false;
						}
					}
				}
				if (orbitContents == OrbitContents.AsteroidBelt)
				{
					num++;
				}
				Kerberos.Sots.Data.StarMapFramework.Orbit orbit = StarSystemHelper.CreateOrbiterParams(orbitContents);
				orbit.OrbitNumber = i;
				yield return orbit;
			}
			yield break;
		}
		private static Dictionary<StellarType, StarDisplayParams> CreateDisplayParams()
		{
			return new Dictionary<StellarType, StarDisplayParams>
			{

				{
					StellarType.O,
					new StarDisplayParams
					{
						AssetPath = "props/TESTSUN_blue.scene",
						ImposterColor = DefaultStarModelParameters.ImposterColor
					}
				},

				{
					StellarType.B,
					new StarDisplayParams
					{
						AssetPath = "props/TESTSUN_bluewhite.scene",
						ImposterColor = DefaultStarModelParameters.ImposterColor
					}
				},

				{
					StellarType.A,
					new StarDisplayParams
					{
						AssetPath = "props/TESTSUN_white.scene",
						ImposterColor = DefaultStarModelParameters.ImposterColor
					}
				},

				{
					StellarType.F,
					new StarDisplayParams
					{
						AssetPath = "props/TESTSUN_yellowWhite.scene",
						ImposterColor = DefaultStarModelParameters.ImposterColor
					}
				},

				{
					StellarType.G,
					new StarDisplayParams
					{
						AssetPath = "props/TESTSUN_yellow.scene",
						ImposterColor = DefaultStarModelParameters.ImposterColor
					}
				},

				{
					StellarType.K,
					new StarDisplayParams
					{
						AssetPath = "props/TESTSUN_orange.scene",
						ImposterColor = DefaultStarModelParameters.ImposterColor
					}
				},

				{
					StellarType.M,
					new StarDisplayParams
					{
						AssetPath = "props/TESTSUN_Red.scene",
						ImposterColor = DefaultStarModelParameters.ImposterColor
					}
				}
			};
		}
		public static StarDisplayParams GetDisplayParams(StellarClass stellarClass)
		{
			return StarHelper._displayParams[stellarClass.Type];
		}
	}
}
