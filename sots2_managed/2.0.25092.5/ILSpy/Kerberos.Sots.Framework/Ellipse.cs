using System;
using System.Runtime.InteropServices;
namespace Kerberos.Sots.Framework
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Ellipse
	{
		public static float CalcEccentricity(float semiMajorAxis, float semiMinorAxis)
		{
			return (float)Math.Sqrt((double)(semiMajorAxis * semiMajorAxis - semiMinorAxis * semiMinorAxis)) / semiMajorAxis;
		}
		public static Vector2 CalcPoint(float semiMajorAxis, float semiMinorAxis, float theta)
		{
			float x = semiMajorAxis * (float)Math.Cos((double)theta);
			float y = semiMinorAxis * (float)Math.Sin((double)theta);
			return new Vector2(x, y);
		}
		public static float CalcSemiMinorAxis(float semiMajorAxis, float eccentricity)
		{
			if (eccentricity < 0f || eccentricity >= 1f)
			{
				throw new ArgumentOutOfRangeException("eccentricity", "Value must be in the range 0..1.");
			}
			return (float)((double)semiMajorAxis * Math.Sqrt((double)(1f - eccentricity * eccentricity)));
		}
	}
}
