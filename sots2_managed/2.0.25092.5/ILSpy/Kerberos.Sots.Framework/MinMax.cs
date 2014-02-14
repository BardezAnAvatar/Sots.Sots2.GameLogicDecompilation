using System;
namespace Kerberos.Sots.Framework
{
	public struct MinMax
	{
		public float Min;
		public float Max;
		public static MinMax Parse(string value)
		{
			string[] array = value.Split(new char[]
			{
				','
			});
			return new MinMax
			{
				Min = float.Parse(array[0]),
				Max = float.Parse(array[1])
			};
		}
	}
}
