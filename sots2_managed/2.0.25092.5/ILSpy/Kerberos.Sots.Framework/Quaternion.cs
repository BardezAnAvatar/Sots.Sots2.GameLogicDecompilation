using System;
namespace Kerberos.Sots.Framework
{
	internal class Quaternion
	{
		public static readonly Quaternion Zero = new Quaternion(0f, 0f, 0f, 0f);
		public static readonly Quaternion Identity = new Quaternion(0f, 0f, 0f, 1f);
		public float X;
		public float Y;
		public float Z;
		public float W;
		public Quaternion()
		{
		}
		public Quaternion(float x, float y, float z, float w)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = w;
		}
		public static Quaternion CreateFromRotationMatrix(Matrix matrix)
		{
			float num = matrix.M11 + matrix.M22 + matrix.M33;
			Quaternion quaternion = new Quaternion();
			if (num > 0f)
			{
				float num2 = (float)Math.Sqrt((double)(num + 1f));
				quaternion.W = num2 * 0.5f;
				num2 = 0.5f / num2;
				quaternion.X = (matrix.M23 - matrix.M32) * num2;
				quaternion.Y = (matrix.M31 - matrix.M13) * num2;
				quaternion.Z = (matrix.M12 - matrix.M21) * num2;
				return quaternion;
			}
			if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
			{
				float num3 = (float)Math.Sqrt((double)(1f + matrix.M11 - matrix.M22 - matrix.M33));
				float num4 = 0.5f / num3;
				quaternion.X = 0.5f * num3;
				quaternion.Y = (matrix.M12 + matrix.M21) * num4;
				quaternion.Z = (matrix.M13 + matrix.M31) * num4;
				quaternion.W = (matrix.M23 - matrix.M32) * num4;
				return quaternion;
			}
			if (matrix.M22 > matrix.M33)
			{
				float num5 = (float)Math.Sqrt((double)(1f + matrix.M22 - matrix.M11 - matrix.M33));
				float num6 = 0.5f / num5;
				quaternion.X = (matrix.M21 + matrix.M12) * num6;
				quaternion.Y = 0.5f * num5;
				quaternion.Z = (matrix.M32 + matrix.M23) * num6;
				quaternion.W = (matrix.M31 - matrix.M13) * num6;
				return quaternion;
			}
			float num7 = (float)Math.Sqrt((double)(1f + matrix.M33 - matrix.M11 - matrix.M22));
			float num8 = 0.5f / num7;
			quaternion.X = (matrix.M31 + matrix.M13) * num8;
			quaternion.Y = (matrix.M32 + matrix.M23) * num8;
			quaternion.Z = 0.5f * num7;
			quaternion.W = (matrix.M12 - matrix.M21) * num8;
			return quaternion;
		}
	}
}
