using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.UI
{
	internal struct PiechartSlice
	{
		public Vector3 Color;
		public double Fraction;
		public PiechartSlice(Vector3 color, double fraction)
		{
			this.Color = color;
			this.Fraction = fraction;
		}
	}
}
