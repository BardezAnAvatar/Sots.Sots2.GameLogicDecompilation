using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots
{
	internal class Orbit
	{
		private float _position;
		public Kerberos.Sots.Data.StarMapFramework.Orbit Parent
		{
			get;
			set;
		}
		public float SemiMajorAxis
		{
			get;
			set;
		}
		public float SemiMinorAxis
		{
			get;
			set;
		}
		public float Inclination
		{
			get;
			set;
		}
		public int OrbitNumber
		{
			get;
			set;
		}
		public float Position
		{
			get
			{
				return this._position;
			}
			set
			{
				this._position = Math.Max(0f, Math.Min(1f, value));
			}
		}
		public static Matrix CalcTransform(Orbit orbit)
		{
			OrbitalPath orbitalPath = default(OrbitalPath);
			orbitalPath.Scale.X = orbit.SemiMajorAxis;
			orbitalPath.Scale.Y = orbit.SemiMinorAxis;
			orbitalPath.InitialAngle = orbit.Position * 3.14159274f * 2f;
			orbitalPath.DeltaAngle = 0.2617994f;
			return orbitalPath.GetTransform(0.0);
		}
		private static float CalcOrbitDistance(float orbitStep, int orbitNumber)
		{
			return (float)orbitNumber * orbitStep + (float)orbitNumber * 0.1f * orbitStep;
		}
		public static float CalcOrbitRadius(float parentRadius, float orbitStep, int orbitNumber)
		{
			return parentRadius + Orbit.CalcOrbitDistance(orbitStep, orbitNumber);
		}
	}
}
