using System;
using System.Runtime.InteropServices;
namespace Kerberos.Sots.Framework
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Circle
	{
		public static Vector2 CalcPoint(float radius, float theta)
		{
			float x = radius * (float)Math.Cos((double)theta);
			float y = radius * (float)Math.Sin((double)theta);
			return new Vector2(x, y);
		}
	}
}
