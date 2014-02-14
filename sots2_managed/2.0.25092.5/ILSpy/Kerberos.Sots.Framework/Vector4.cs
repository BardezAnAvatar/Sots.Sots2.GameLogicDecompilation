using System;
namespace Kerberos.Sots.Framework
{
	public struct Vector4 : IEquatable<Vector4>
	{
		public float X;
		public float Y;
		public float Z;
		public float W;
		public static readonly Vector4 Zero = new Vector4(0f, 0f, 0f, 0f);
		public static readonly Vector4 One = new Vector4(1f, 1f, 1f, 1f);
		public static readonly Vector4 UnitX = new Vector4(1f, 0f, 0f, 0f);
		public static readonly Vector4 UnitY = new Vector4(0f, 1f, 0f, 0f);
		public static readonly Vector4 UnitZ = new Vector4(0f, 0f, 1f, 0f);
		internal Vector3 Xyz
		{
			get
			{
				return new Vector3(this.X, this.Y, this.Z);
			}
		}
		public Vector4(float x, float y, float z, float w)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = w;
		}
		public Vector4(Vector3 value, float w)
		{
			this.X = value.X;
			this.Y = value.Y;
			this.Z = value.Z;
			this.W = w;
		}
		public Vector4(Vector4 value)
		{
			this.X = value.X;
			this.Y = value.Y;
			this.Z = value.Z;
			this.W = value.W;
		}
		public Vector4(float value)
		{
			this.W = value;
			this.Z = value;
			this.Y = value;
			this.X = value;
		}
		public static Vector4 Lerp(Vector4 v0, Vector4 v1, float t)
		{
			return new Vector4(v0.X + (v1.X - v0.X) * t, v0.Y + (v1.Y - v0.Y) * t, v0.Z + (v1.Z - v0.Z) * t, v0.W + (v1.W - v0.W) * t);
		}
		public static bool operator ==(Vector4 valueA, Vector4 valueB)
		{
			return valueA.Equals(valueB);
		}
		public static bool operator !=(Vector4 valueA, Vector4 valueB)
		{
			return !valueA.Equals(valueB);
		}
		public bool Equals(Vector4 other)
		{
			return this.X == other.X && this.Y == other.Y && this.Z == other.Z && this.W == other.W;
		}
		public override bool Equals(object obj)
		{
			return obj != null && obj is Vector4 && this.Equals((Vector4)obj);
		}
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.W.GetHashCode();
		}
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", new object[]
			{
				this.X,
				this.Y,
				this.Z,
				this.W
			});
		}
	}
}
