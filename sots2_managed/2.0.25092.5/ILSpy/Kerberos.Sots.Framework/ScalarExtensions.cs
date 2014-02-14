using System;
namespace Kerberos.Sots.Framework
{
	public static class ScalarExtensions
	{
		public static bool IsFinite(this Vector3 value)
		{
			return value.X.IsFinite() && value.Y.IsFinite() && value.Z.IsFinite();
		}
		public static bool IsFinite(this Matrix value)
		{
			return value.M11.IsFinite() && value.M12.IsFinite() && value.M13.IsFinite() && value.M14.IsFinite() && value.M21.IsFinite() && value.M22.IsFinite() && value.M23.IsFinite() && value.M24.IsFinite() && value.M31.IsFinite() && value.M32.IsFinite() && value.M33.IsFinite() && value.M34.IsFinite() && value.M41.IsFinite() && value.M42.IsFinite() && value.M43.IsFinite() && value.M44.IsFinite();
		}
		public static bool IsFinite(this double value)
		{
			return !double.IsInfinity(value) && !double.IsNaN(value);
		}
		public static bool IsFinite(this float value)
		{
			return !float.IsInfinity(value) && !float.IsNaN(value);
		}
		public static double Saturate(this double value)
		{
			return Math.Max(0.0, Math.Min(value, 1.0));
		}
		public static double Lerp(double x0, double x1, double t)
		{
			return x0 + t * (x1 - x0);
		}
		public static double SmoothStep(this double t)
		{
			t = t.Saturate();
			return t * t * (3.0 - 2.0 * t);
		}
		public static double SmoothStep(double x0, double x1, double t)
		{
			t = t.SmoothStep();
			return x0 + t * (x1 - x0);
		}
		public static float Saturate(this float value)
		{
			return Math.Max(0f, Math.Min(value, 1f));
		}
		public static int Clamp(this int value, int x0, int x1)
		{
			return Math.Max(x0, Math.Min(value, x1));
		}
		public static float Clamp(this float value, float x0, float x1)
		{
			return Math.Max(x0, Math.Min(value, x1));
		}
		public static double Clamp(this double value, double x0, double x1)
		{
			return Math.Max(x0, Math.Min(value, x1));
		}
		public static float Lerp(float x0, float x1, float t)
		{
			return x0 + t * (x1 - x0);
		}
		public static float SmoothStep(float t)
		{
			t = t.Saturate();
			return t * t * (3f - 2f * t);
		}
		public static float SmoothStep(float x0, float x1, float t)
		{
			t = ScalarExtensions.SmoothStep(t);
			return x0 + t * (x1 - x0);
		}
		public static void VerifyFinite(this float t)
		{
			if (!t.IsFinite())
			{
				throw new ArgumentException("The given value is not finite.");
			}
		}
		public static void VerifyFinite(this double t)
		{
			if (!t.IsFinite())
			{
				throw new ArgumentException("The given value is not finite.");
			}
		}
	}
}
