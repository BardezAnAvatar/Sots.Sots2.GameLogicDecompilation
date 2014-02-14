using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots
{
	internal struct OrbitalPath
	{
		public static readonly OrbitalPath Zero = new OrbitalPath
		{
			Scale = Vector2.Zero,
			Rotation = Vector3.Zero,
			InitialAngle = 0f,
			DeltaAngle = 0f
		};
		public Vector2 Scale;
		public Vector3 Rotation;
		public float InitialAngle;
		public float DeltaAngle;
		public Matrix GetTransform(double t)
		{
			this.DeltaAngle = 0.2f;
			float num = (this.InitialAngle + (float)t * this.DeltaAngle) % 6.28318548f;
			float x = (float)Math.Sin((double)(-(double)num)) * this.Scale.X;
			float z = -(float)Math.Cos((double)(-(double)num)) * this.Scale.Y;
			return Matrix.CreateRotationYPR(this.Rotation.X, this.Rotation.Y, this.Rotation.Z) * Matrix.CreateTranslation(new Vector3(x, 0f, z));
		}
		public static OrbitalPath Parse(string fromdb)
		{
			string[] array = fromdb.Split(new char[]
			{
				','
			});
			return new OrbitalPath
			{
				Scale = new Vector2(float.Parse(array[0]), float.Parse(array[1])),
				Rotation = new Vector3(float.Parse(array[2]), float.Parse(array[3]), float.Parse(array[4])),
				InitialAngle = float.Parse(array[5]),
				DeltaAngle = float.Parse(array[6])
			};
		}
		public void VerifyFinite()
		{
			this.Scale.X.VerifyFinite();
			this.Scale.Y.VerifyFinite();
			this.Rotation.X.VerifyFinite();
			this.Rotation.Y.VerifyFinite();
			this.Rotation.Z.VerifyFinite();
			this.InitialAngle.VerifyFinite();
			this.DeltaAngle.VerifyFinite();
		}
		public override string ToString()
		{
			return string.Format("{0},{1},{2},{3},{4},{5},{6}", new object[]
			{
				this.Scale.X,
				this.Scale.Y,
				this.Rotation.X,
				this.Rotation.Y,
				this.Rotation.Z,
				this.InitialAngle,
				this.DeltaAngle
			});
		}
	}
}
