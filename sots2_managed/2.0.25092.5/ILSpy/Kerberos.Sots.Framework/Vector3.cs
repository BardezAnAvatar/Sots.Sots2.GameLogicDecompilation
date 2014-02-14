using System;
namespace Kerberos.Sots.Framework
{
	public struct Vector3 : IEquatable<Vector3>
	{
		public float X;
		public float Y;
		public float Z;
		public static readonly Vector3 Zero = new Vector3(0f, 0f, 0f);
		public static readonly Vector3 One = new Vector3(1f, 1f, 1f);
		public static readonly Vector3 UnitX = new Vector3(1f, 0f, 0f);
		public static readonly Vector3 UnitY = new Vector3(0f, 1f, 0f);
		public static readonly Vector3 UnitZ = new Vector3(0f, 0f, 1f);
		public float Length
		{
			get
			{
				return (float)Math.Sqrt((double)(this.X * this.X + this.Y * this.Y + this.Z * this.Z));
			}
		}
		public float LengthSquared
		{
			get
			{
				return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
			}
		}
		public Vector3(float x, float y, float z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}
		public Vector3(Vector3 value)
		{
			this.X = value.X;
			this.Y = value.Y;
			this.Z = value.Z;
		}
		public Vector3(float value)
		{
			this.Z = value;
			this.Y = value;
			this.X = value;
		}
		public static Vector3 Normalize(Vector3 value)
		{
			return value / value.Length;
		}
		public static Vector3 Lerp(Vector3 v0, Vector3 v1, float t)
		{
			return new Vector3(v0.X + (v1.X - v0.X) * t, v0.Y + (v1.Y - v0.Y) * t, v0.Z + (v1.Z - v0.Z) * t);
		}
		public static bool operator ==(Vector3 valueA, Vector3 valueB)
		{
			return valueA.Equals(valueB);
		}
		public static bool operator !=(Vector3 valueA, Vector3 valueB)
		{
			return !valueA.Equals(valueB);
		}
		public static Vector3 operator *(Vector3 v, float s)
		{
			return new Vector3(s * v.X, s * v.Y, s * v.Z);
		}
		public static Vector3 operator /(Vector3 v, float s)
		{
			return new Vector3(v.X / s, v.Y / s, v.Z / s);
		}
		public static Vector3 operator +(Vector3 v0, Vector3 v1)
		{
			return new Vector3(v0.X + v1.X, v0.Y + v1.Y, v0.Z + v1.Z);
		}
		public static Vector3 operator -(Vector3 v0, Vector3 v1)
		{
			return new Vector3(v0.X - v1.X, v0.Y - v1.Y, v0.Z - v1.Z);
		}
		public static Vector3 operator -(Vector3 v0)
		{
			return new Vector3(-v0.X, -v0.Y, -v0.Z);
		}
		public static float Dot(Vector3 v0, Vector3 v1)
		{
			return v0.X * v1.X + v0.Y * v1.Y + v0.Z * v1.Z;
		}
		public float Normalize()
		{
			float num = (float)Math.Sqrt((double)(this.X * this.X + this.Y * this.Y + this.Z * this.Z));
			this.X /= num;
			this.Y /= num;
			this.Z /= num;
			return num;
		}
		public bool Equals(Vector3 other)
		{
			return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
		}
		public override bool Equals(object obj)
		{
			return obj != null && obj is Vector3 && this.Equals((Vector3)obj);
		}
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
		}
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", this.X, this.Y, this.Z);
		}
		public static Vector3 Parse(string value)
		{
			return Vector3.Parse(value, new char[]
			{
				','
			});
		}
		public static bool TryParse(string value, out Vector3 v)
		{
			return Vector3.TryParse(value, out v, new char[]
			{
				','
			});
		}
		public static bool TryParse(string value, out Vector3 v, params char[] separator)
		{
			bool flag = true;
			v = default(Vector3);
			string[] array = new string[0];
			if (string.IsNullOrEmpty(value))
			{
				flag = false;
			}
			else
			{
				array = value.Split(separator);
			}
			if (flag && array.Length != 3)
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[0], out v.X))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[1], out v.Y))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[2], out v.Z))
			{
				flag = false;
			}
			if (!flag)
			{
				v = default(Vector3);
			}
			return flag;
		}
		public static Vector3 Parse(string value, params char[] separator)
		{
			string[] array = value.Split(separator);
			return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
		}
		public static Vector3 RadiansToDegrees(Vector3 value)
		{
			return new Vector3(MathHelper.RadiansToDegrees(value.X), MathHelper.RadiansToDegrees(value.Y), MathHelper.RadiansToDegrees(value.Z));
		}
		public static Vector3 DegreesToRadians(Vector3 value)
		{
			return new Vector3(MathHelper.DegreesToRadians(value.X), MathHelper.DegreesToRadians(value.Y), MathHelper.DegreesToRadians(value.Z));
		}
		public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
		{
			Vector3 result;
			result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
			result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
			result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
			return result;
		}
		public static Vector3 Transform(Vector3 vec, Matrix mat)
		{
			return new Vector3
			{
				X = vec.X * mat.M11 + vec.Y * mat.M21 + vec.Z * mat.M31 + mat.M41,
				Y = vec.X * mat.M12 + vec.Y * mat.M22 + vec.Z * mat.M32 + mat.M42,
				Z = vec.X * mat.M13 + vec.Y * mat.M23 + vec.Z * mat.M33 + mat.M43
			};
		}
	}
}
