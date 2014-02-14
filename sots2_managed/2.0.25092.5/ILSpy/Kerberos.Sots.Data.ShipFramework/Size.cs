using System;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class Size
	{
		public int X;
		public int Y;
		public static implicit operator Size(string rhs)
		{
			Size result = new Size();
			Size.TryParse(rhs, out result);
			return result;
		}
		public override string ToString()
		{
			return this.X.ToString() + "," + this.Y.ToString();
		}
		public static bool TryParse(string s, out Size value)
		{
			bool result;
			try
			{
				string[] array = s.Split(new char[]
				{
					','
				});
				value = new Size
				{
					X = int.Parse(array[0]),
					Y = int.Parse(array[1])
				};
				result = true;
			}
			catch (Exception)
			{
				value = new Size();
				result = false;
			}
			return result;
		}
	}
}
