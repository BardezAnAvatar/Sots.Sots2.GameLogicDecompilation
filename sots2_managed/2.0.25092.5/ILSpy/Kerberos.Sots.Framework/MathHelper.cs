using System;
namespace Kerberos.Sots.Framework
{
	public static class MathHelper
	{
		public const double TwoPi = 6.2831853071795862;
		public const double PiOverTwo = 1.5707963267948966;
		public const double PiOverFour = 0.78539816339744828;
		public static float DegreesToRadians(float degrees)
		{
			return (float)((double)degrees * 3.1415926535897931 / 180.0);
		}
		public static float RadiansToDegrees(float radians)
		{
			return (float)((double)radians * 180.0 / 3.1415926535897931);
		}
		public static Vector3 DegreesToRadians(Vector3 degrees)
		{
			return new Vector3(MathHelper.DegreesToRadians(degrees.X), MathHelper.DegreesToRadians(degrees.Y), MathHelper.DegreesToRadians(degrees.Z));
		}
		public static Vector3 RadiansToDegrees(Vector3 radians)
		{
			return new Vector3(MathHelper.RadiansToDegrees(radians.X), MathHelper.RadiansToDegrees(radians.Y), MathHelper.RadiansToDegrees(radians.Z));
		}
		public static double Square(double value)
		{
			return value * value;
		}
		public static float Square(float value)
		{
			return value * value;
		}
	}
}
