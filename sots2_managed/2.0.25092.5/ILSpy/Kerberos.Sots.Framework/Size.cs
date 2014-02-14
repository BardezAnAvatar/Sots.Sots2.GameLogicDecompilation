using System;
namespace Kerberos.Sots.Framework
{
	internal struct Size
	{
		public int X;
		public int Y;
		public static Size Parse(string value)
		{
			string[] array = value.Split(new char[]
			{
				','
			});
			return new Size
			{
				X = int.Parse(array[0]),
				Y = int.Parse(array[1])
			};
		}
	}
}
