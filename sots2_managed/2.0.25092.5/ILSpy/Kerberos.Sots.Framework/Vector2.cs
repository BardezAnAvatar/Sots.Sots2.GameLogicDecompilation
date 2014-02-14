using System;
namespace Kerberos.Sots.Framework
{
	public struct Vector2 : IEquatable<Vector2>
	{
		public float X;
		public float Y;
		public static readonly Vector2 Zero = new Vector2(0f, 0f);
		public static readonly Vector2 One = new Vector2(1f, 1f);
		public float Length
		{
			get
			{
				return (float)Math.Sqrt((double)(this.X * this.X + this.Y * this.Y));
			}
		}
		public float LengthSq
		{
			get
			{
				return this.X * this.X + this.Y * this.Y;
			}
		}
		public Vector2(float x, float y)
		{
			this.X = x;
			this.Y = y;
		}
		public Vector2(Vector2 value)
		{
			this.X = value.X;
			this.Y = value.Y;
		}
		public Vector2(float value)
		{
			this.Y = value;
			this.X = value;
		}
		public static Vector2 Lerp(Vector2 v0, Vector2 v1, float t)
		{
			return new Vector2(v0.X + (v1.X - v0.X) * t, v0.Y + (v1.Y - v0.Y) * t);
		}
		public static bool operator ==(Vector2 valueA, Vector2 valueB)
		{
			return valueA.Equals(valueB);
		}
		public static bool operator !=(Vector2 valueA, Vector2 valueB)
		{
			return !valueA.Equals(valueB);
		}
		public static Vector2 operator *(Vector2 v, float s)
		{
			return new Vector2(s * v.X, s * v.Y);
		}
		public bool Equals(Vector2 other)
		{
			return this.X == other.X && this.Y == other.Y;
		}
		public override bool Equals(object obj)
		{
			return obj != null && obj is Vector2 && this.Equals((Vector2)obj);
		}
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}
		public override string ToString()
		{
			return string.Format("{0}, {1}", this.X, this.Y);
		}
		public static Vector2 Parse(string value)
		{
			return Vector2.Parse(value, new char[]
			{
				','
			});
		}
		public static Vector2 Parse(string value, params char[] separator)
		{
			string[] array = value.Split(separator);
			return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
		}
	}
}
