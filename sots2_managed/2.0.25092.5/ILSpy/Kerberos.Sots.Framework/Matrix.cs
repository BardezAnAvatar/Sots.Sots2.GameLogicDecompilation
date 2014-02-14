using System;
namespace Kerberos.Sots.Framework
{
	public struct Matrix
	{
		public static readonly Matrix Identity = new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
		public float M11;
		public float M12;
		public float M13;
		public float M14;
		public float M21;
		public float M22;
		public float M23;
		public float M24;
		public float M31;
		public float M32;
		public float M33;
		public float M34;
		public float M41;
		public float M42;
		public float M43;
		public float M44;
		public Vector3 Right
		{
			get
			{
				return new Vector3(this.M11, this.M12, this.M13);
			}
		}
		public Vector3 Up
		{
			get
			{
				return new Vector3(this.M21, this.M22, this.M23);
			}
		}
		public Vector3 Forward
		{
			get
			{
				return new Vector3(-this.M31, -this.M32, -this.M33);
			}
		}
		public Vector3 Position
		{
			get
			{
				return new Vector3(this.M41, this.M42, this.M43);
			}
			set
			{
				this.M41 = value.X;
				this.M42 = value.Y;
				this.M43 = value.Z;
			}
		}
		public Vector3 EulerAngles
		{
			get
			{
				float y = (float)Math.Asin((double)this.Forward.Y.Clamp(-1f, 1f));
				float x;
				float z;
				if ((double)Math.Abs(this.Forward.Y) >= 0.9)
				{
					x = (float)Math.Atan2((double)(-(double)this.Right.Z), (double)this.Right.Y);
					z = 0f;
				}
				else
				{
					x = (float)Math.Atan2((double)(-(double)this.Forward.X), (double)(-(double)this.Forward.Z));
					z = (float)Math.Atan2((double)this.Right.Y, (double)this.Up.Y);
				}
				return new Vector3(x, y, z);
			}
		}
		public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
		{
			this.M11 = m11;
			this.M12 = m12;
			this.M13 = m13;
			this.M14 = m14;
			this.M21 = m21;
			this.M22 = m22;
			this.M23 = m23;
			this.M24 = m24;
			this.M31 = m31;
			this.M32 = m32;
			this.M33 = m33;
			this.M34 = m34;
			this.M41 = m41;
			this.M42 = m42;
			this.M43 = m43;
			this.M44 = m44;
		}
		public Matrix(Matrix value)
		{
			this.M11 = value.M11;
			this.M12 = value.M12;
			this.M13 = value.M13;
			this.M14 = value.M14;
			this.M21 = value.M21;
			this.M22 = value.M22;
			this.M23 = value.M23;
			this.M24 = value.M24;
			this.M31 = value.M31;
			this.M32 = value.M32;
			this.M33 = value.M33;
			this.M34 = value.M34;
			this.M41 = value.M41;
			this.M42 = value.M42;
			this.M43 = value.M43;
			this.M44 = value.M44;
		}
		public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
		{
			Vector3 vector = Vector3.Normalize(-forward);
			Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
			Vector3 vector3 = Vector3.Cross(vector, vector2);
			Matrix result;
			result.M11 = vector2.X;
			result.M12 = vector2.Y;
			result.M13 = vector2.Z;
			result.M14 = 0f;
			result.M21 = vector3.X;
			result.M22 = vector3.Y;
			result.M23 = vector3.Z;
			result.M24 = 0f;
			result.M31 = vector.X;
			result.M32 = vector.Y;
			result.M33 = vector.Z;
			result.M34 = 0f;
			result.M41 = position.X;
			result.M42 = position.Y;
			result.M43 = position.Z;
			result.M44 = 1f;
			return result;
		}
		public static Matrix CreateScale(Vector3 scale)
		{
			return new Matrix(scale.X, 0f, 0f, 0f, 0f, scale.Y, 0f, 0f, 0f, 0f, scale.Z, 0f, 0f, 0f, 0f, 1f);
		}
		public static Matrix CreateScale(float x, float y, float z)
		{
			return new Matrix(x, 0f, 0f, 0f, 0f, y, 0f, 0f, 0f, 0f, z, 0f, 0f, 0f, 0f, 1f);
		}
		public static Matrix CreateTranslation(Vector3 trans)
		{
			return new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, trans.X, trans.Y, trans.Z, 1f);
		}
		public static Matrix CreateTranslation(float x, float y, float z)
		{
			return new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, x, y, z, 1f);
		}
		public static Matrix CreateRotationX(float radians)
		{
			float num = (float)Math.Cos((double)radians);
			float num2 = (float)Math.Sin((double)radians);
			return new Matrix(1f, 0f, 0f, 0f, 0f, num, num2, 0f, 0f, -num2, num, 0f, 0f, 0f, 0f, 1f);
		}
		public static Matrix CreateRotationY(float radians)
		{
			float num = (float)Math.Cos((double)radians);
			float num2 = (float)Math.Sin((double)radians);
			return new Matrix(num, 0f, -num2, 0f, 0f, 1f, 0f, 0f, num2, 0f, num, 0f, 0f, 0f, 0f, 1f);
		}
		public static Matrix CreateRotationZ(float radians)
		{
			float num = (float)Math.Cos((double)radians);
			float num2 = (float)Math.Sin((double)radians);
			return new Matrix(num, num2, 0f, 0f, -num2, num, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
		}
		public static Matrix CreateRotationYPR(Vector3 yawPitchRoll)
		{
			return Matrix.CreateRotationYPR(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z);
		}
		public static Matrix CreateRotationYPR(float yawRadians, float pitchRadians, float rollRadians)
		{
			Matrix matrix = Matrix.CreateRotationZ(rollRadians);
			Matrix matrix2 = Matrix.CreateRotationX(pitchRadians);
			Matrix matrix3 = Matrix.CreateRotationY(yawRadians);
			Matrix result;
			Matrix.Multiply(out result, ref matrix, ref matrix2);
			Matrix.Multiply(out result, ref result, ref matrix3);
			return result;
		}
		public static Matrix operator *(Matrix lhs, Matrix rhs)
		{
			Matrix result;
			Matrix.Multiply(out result, ref lhs, ref rhs);
			return result;
		}
		public static void Multiply(out Matrix output, ref Matrix lhs, ref Matrix rhs)
		{
			output = new Matrix(lhs.M11 * rhs.M11 + lhs.M12 * rhs.M21 + lhs.M13 * rhs.M31 + lhs.M14 * rhs.M41, lhs.M11 * rhs.M12 + lhs.M12 * rhs.M22 + lhs.M13 * rhs.M32 + lhs.M14 * rhs.M42, lhs.M11 * rhs.M13 + lhs.M12 * rhs.M23 + lhs.M13 * rhs.M33 + lhs.M14 * rhs.M43, lhs.M11 * rhs.M14 + lhs.M12 * rhs.M24 + lhs.M13 * rhs.M34 + lhs.M14 * rhs.M44, lhs.M21 * rhs.M11 + lhs.M22 * rhs.M21 + lhs.M23 * rhs.M31 + lhs.M24 * rhs.M41, lhs.M21 * rhs.M12 + lhs.M22 * rhs.M22 + lhs.M23 * rhs.M32 + lhs.M24 * rhs.M42, lhs.M21 * rhs.M13 + lhs.M22 * rhs.M23 + lhs.M23 * rhs.M33 + lhs.M24 * rhs.M43, lhs.M21 * rhs.M14 + lhs.M22 * rhs.M24 + lhs.M23 * rhs.M34 + lhs.M24 * rhs.M44, lhs.M31 * rhs.M11 + lhs.M32 * rhs.M21 + lhs.M33 * rhs.M31 + lhs.M34 * rhs.M41, lhs.M31 * rhs.M12 + lhs.M32 * rhs.M22 + lhs.M33 * rhs.M32 + lhs.M34 * rhs.M42, lhs.M31 * rhs.M13 + lhs.M32 * rhs.M23 + lhs.M33 * rhs.M33 + lhs.M34 * rhs.M43, lhs.M31 * rhs.M14 + lhs.M32 * rhs.M24 + lhs.M33 * rhs.M34 + lhs.M34 * rhs.M44, lhs.M41 * rhs.M11 + lhs.M42 * rhs.M21 + lhs.M43 * rhs.M31 + lhs.M44 * rhs.M41, lhs.M41 * rhs.M12 + lhs.M42 * rhs.M22 + lhs.M43 * rhs.M32 + lhs.M44 * rhs.M42, lhs.M41 * rhs.M13 + lhs.M42 * rhs.M23 + lhs.M43 * rhs.M33 + lhs.M44 * rhs.M43, lhs.M41 * rhs.M14 + lhs.M42 * rhs.M24 + lhs.M43 * rhs.M34 + lhs.M44 * rhs.M44);
		}
		public static float Determinant(Matrix m)
		{
			return m.M11 * m.M22 * m.M33 * m.M44 + m.M11 * m.M23 * m.M34 * m.M42 + m.M11 * m.M24 * m.M32 * m.M43 + m.M12 * m.M21 * m.M34 * m.M43 + m.M12 * m.M23 * m.M31 * m.M44 + m.M12 * m.M24 * m.M33 * m.M41 + m.M13 * m.M21 * m.M32 * m.M44 + m.M13 * m.M22 * m.M34 * m.M41 + m.M13 * m.M24 * m.M31 * m.M42 + m.M14 * m.M21 * m.M33 * m.M42 + m.M14 * m.M22 * m.M31 * m.M43 + m.M14 * m.M23 * m.M32 * m.M41 - m.M11 * m.M22 * m.M34 * m.M43 - m.M11 * m.M23 * m.M32 * m.M44 - m.M11 * m.M24 * m.M33 * m.M42 - m.M12 * m.M21 * m.M33 * m.M44 - m.M12 * m.M23 * m.M34 * m.M41 - m.M12 * m.M24 * m.M31 * m.M43 - m.M13 * m.M21 * m.M34 * m.M42 - m.M13 * m.M22 * m.M31 * m.M44 - m.M13 * m.M24 * m.M32 * m.M41 - m.M14 * m.M21 * m.M32 * m.M43 - m.M14 * m.M22 * m.M33 * m.M41 - m.M14 * m.M23 * m.M31 * m.M42;
		}
		public static Matrix Inverse(Matrix m)
		{
			float num = Matrix.Determinant(m);
			if (num != 0f)
			{
				float num2 = 1f / num;
				return new Matrix(num2 * (m.M22 * m.M33 * m.M44 + m.M23 * m.M34 * m.M42 + m.M24 * m.M32 * m.M43 - m.M22 * m.M34 * m.M43 - m.M23 * m.M32 * m.M44 - m.M24 * m.M33 * m.M42), num2 * (m.M12 * m.M34 * m.M43 + m.M13 * m.M32 * m.M44 + m.M14 * m.M33 * m.M42 - m.M12 * m.M33 * m.M44 - m.M13 * m.M34 * m.M42 - m.M14 * m.M32 * m.M43), num2 * (m.M12 * m.M23 * m.M44 + m.M13 * m.M24 * m.M42 + m.M14 * m.M22 * m.M43 - m.M12 * m.M24 * m.M43 - m.M13 * m.M22 * m.M44 - m.M14 * m.M23 * m.M42), num2 * (m.M12 * m.M24 * m.M33 + m.M13 * m.M22 * m.M34 + m.M14 * m.M23 * m.M32 - m.M12 * m.M23 * m.M34 - m.M13 * m.M24 * m.M32 - m.M14 * m.M22 * m.M33), num2 * (m.M21 * m.M34 * m.M43 + m.M23 * m.M31 * m.M44 + m.M24 * m.M33 * m.M41 - m.M21 * m.M33 * m.M44 - m.M23 * m.M34 * m.M41 - m.M24 * m.M31 * m.M43), num2 * (m.M11 * m.M33 * m.M44 + m.M13 * m.M34 * m.M41 + m.M14 * m.M31 * m.M43 - m.M11 * m.M34 * m.M43 - m.M13 * m.M31 * m.M44 - m.M14 * m.M33 * m.M41), num2 * (m.M11 * m.M24 * m.M43 + m.M13 * m.M21 * m.M44 + m.M14 * m.M23 * m.M41 - m.M11 * m.M23 * m.M44 - m.M13 * m.M24 * m.M41 - m.M14 * m.M21 * m.M43), num2 * (m.M11 * m.M23 * m.M34 + m.M13 * m.M24 * m.M31 + m.M14 * m.M21 * m.M33 - m.M11 * m.M24 * m.M33 - m.M13 * m.M21 * m.M34 - m.M14 * m.M23 * m.M31), num2 * (m.M21 * m.M32 * m.M44 + m.M22 * m.M34 * m.M41 + m.M24 * m.M31 * m.M42 - m.M21 * m.M34 * m.M42 - m.M22 * m.M31 * m.M44 - m.M24 * m.M32 * m.M41), num2 * (m.M11 * m.M34 * m.M42 + m.M12 * m.M31 * m.M44 + m.M14 * m.M32 * m.M41 - m.M11 * m.M32 * m.M44 - m.M12 * m.M34 * m.M41 - m.M14 * m.M31 * m.M42), num2 * (m.M11 * m.M22 * m.M44 + m.M12 * m.M24 * m.M41 + m.M14 * m.M21 * m.M42 - m.M11 * m.M24 * m.M42 - m.M12 * m.M21 * m.M44 - m.M14 * m.M22 * m.M41), num2 * (m.M11 * m.M24 * m.M32 + m.M12 * m.M21 * m.M34 + m.M14 * m.M22 * m.M31 - m.M11 * m.M22 * m.M34 - m.M12 * m.M24 * m.M31 - m.M14 * m.M21 * m.M32), num2 * (m.M21 * m.M33 * m.M42 + m.M22 * m.M31 * m.M43 + m.M23 * m.M32 * m.M41 - m.M21 * m.M32 * m.M43 - m.M22 * m.M33 * m.M41 - m.M23 * m.M31 * m.M42), num2 * (m.M11 * m.M32 * m.M43 + m.M12 * m.M33 * m.M41 + m.M13 * m.M31 * m.M42 - m.M11 * m.M33 * m.M42 - m.M12 * m.M31 * m.M43 - m.M13 * m.M32 * m.M41), num2 * (m.M11 * m.M23 * m.M42 + m.M12 * m.M21 * m.M43 + m.M13 * m.M22 * m.M41 - m.M11 * m.M22 * m.M43 - m.M12 * m.M23 * m.M41 - m.M13 * m.M21 * m.M42), num2 * (m.M11 * m.M22 * m.M33 + m.M12 * m.M23 * m.M31 + m.M13 * m.M21 * m.M32 - m.M11 * m.M23 * m.M32 - m.M12 * m.M21 * m.M33 - m.M13 * m.M22 * m.M31));
			}
			return m;
		}
		public static Matrix Parse(string value)
		{
			return Matrix.Parse(value, new char[]
			{
				','
			});
		}
		public static Matrix Parse(string value, params char[] separator)
		{
			string[] array = value.Split(separator);
			return new Matrix(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]), float.Parse(array[4]), float.Parse(array[5]), float.Parse(array[6]), float.Parse(array[7]), float.Parse(array[8]), float.Parse(array[9]), float.Parse(array[10]), float.Parse(array[11]), float.Parse(array[12]), float.Parse(array[13]), float.Parse(array[14]), float.Parse(array[15]));
		}
		public static bool TryParse(string value, out Matrix m)
		{
			return Matrix.TryParse(value, out m, new char[]
			{
				','
			});
		}
		public static bool TryParse(string value, out Matrix m, params char[] separator)
		{
			bool flag = true;
			m = default(Matrix);
			string[] array = new string[0];
			if (string.IsNullOrEmpty(value))
			{
				flag = false;
			}
			else
			{
				array = value.Split(separator);
			}
			if (flag && array.Length != 16)
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[0], out m.M11))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[1], out m.M12))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[2], out m.M13))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[3], out m.M14))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[4], out m.M21))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[5], out m.M22))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[6], out m.M23))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[7], out m.M24))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[8], out m.M31))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[9], out m.M32))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[10], out m.M33))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[11], out m.M34))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[12], out m.M41))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[13], out m.M42))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[14], out m.M43))
			{
				flag = false;
			}
			if (flag && !float.TryParse(array[15], out m.M44))
			{
				flag = false;
			}
			if (!flag)
			{
				m = default(Matrix);
			}
			return flag;
		}
		public override string ToString()
		{
			return this.ToString(',');
		}
		public string ToString(char separator)
		{
			return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}", new object[]
			{
				separator,
				this.M11,
				this.M12,
				this.M13,
				this.M14,
				this.M21,
				this.M22,
				this.M23,
				this.M24,
				this.M31,
				this.M32,
				this.M33,
				this.M34,
				this.M41,
				this.M42,
				this.M43,
				this.M44
			});
		}
		public static Matrix PolarDeviation(Random random, float maxAngle)
		{
			if (maxAngle >= 0.0001f)
			{
				float num = Math.Abs(random.NextSingle());
				float num2 = random.NextInclusive(0f, 6.28318548f);
				float z = -1f / (float)Math.Tan((double)maxAngle);
				Vector3 vector = new Vector3(num * (float)Math.Cos((double)num2), num * (float)Math.Sin((double)num2), z);
				vector.Normalize();
				float num3 = Vector3.Dot(vector, Vector3.UnitY);
				Vector3 up = (num3 > 0.99f) ? (-Vector3.UnitZ) : ((num3 < -0.99f) ? Vector3.UnitZ : Vector3.UnitY);
				return Matrix.CreateWorld(Vector3.Zero, vector, up);
			}
			return Matrix.Identity;
		}
	}
}
